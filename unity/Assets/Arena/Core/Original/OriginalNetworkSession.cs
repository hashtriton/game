using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Arena.Original
{
    public enum OriginalNetworkResponseKind { Reply = 1, Snapshot }

    [Serializable]
    public sealed class OriginalNetworkResponse
    {
        public OriginalNetworkResponseKind kind;
        public OriginalSessionReplyCode code;
        public long commandSequence;
        public int assignedSlot;
        public long acknowledgedSequence;
        public OriginalSessionView snapshot;
    }

    public interface IOriginalSessionCodec
    {
        byte[] EncodeCommand(OriginalSessionCommand command);
        bool TryDecodeCommand(byte[] bytes, out OriginalSessionCommand command);
        byte[] EncodeResponse(OriginalNetworkResponse response);
        bool TryDecodeResponse(byte[] bytes, out OriginalNetworkResponse response);
    }

    public static class OriginalSessionWire
    {
        public const int HeaderBytes = 20;
        public const int MaximumMessageBytes = 4 * 1024 * 1024;
        // Transport already frames each fragment. Server responses additionally use
        // LIA1 + uint64 message ID + uint32 total bytes + uint32 offset, big endian.
        // Client commands remain one codec payload in one transport frame.
        public static byte[] Fragment(byte[] message, long messageId, int offset)
        {
            if (message == null || message.Length == 0 || message.Length > MaximumMessageBytes ||
                messageId <= 0 || offset < 0 || offset >= message.Length)
                throw new ArgumentOutOfRangeException(nameof(message));
            var count = Math.Min(message.Length - offset, OriginalFrameDecoder.MaximumPayload - HeaderBytes);
            var frame = new byte[HeaderBytes + count];
            frame[0] = 76; frame[1] = 73; frame[2] = 65; frame[3] = 49;
            WriteUInt(frame, 4, (uint)((ulong)messageId >> 32));
            WriteUInt(frame, 8, (uint)messageId);
            WriteUInt(frame, 12, (uint)message.Length);
            WriteUInt(frame, 16, (uint)offset);
            Buffer.BlockCopy(message, offset, frame, HeaderBytes, count);
            return frame;
        }

        internal static uint ReadUInt(byte[] bytes, int offset) =>
            (uint)bytes[offset] << 24 | (uint)bytes[offset + 1] << 16 | (uint)bytes[offset + 2] << 8 | bytes[offset + 3];

        private static void WriteUInt(byte[] bytes, int offset, uint value)
        {
            bytes[offset] = (byte)(value >> 24); bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8); bytes[offset + 3] = (byte)value;
        }
    }

    public sealed class OriginalSessionWireReader
    {
        private byte[] pending;
        private int used;
        private long completedId, currentId;
        public bool Failed { get; private set; }
        public string FailureReason { get; private set; }
        public int BufferedBytes => pending?.Length ?? 0;

        public bool Feed(byte[] frame, out byte[] message)
        {
            message = null;
            if (Failed) return false;
            if (frame == null || frame.Length <= OriginalSessionWire.HeaderBytes || frame.Length > OriginalFrameDecoder.MaximumPayload ||
                frame[0] != 76 || frame[1] != 73 || frame[2] != 65 || frame[3] != 49)
                return Reject("invalid-response-fragment");
            var id = (ulong)OriginalSessionWire.ReadUInt(frame, 4) << 32 | OriginalSessionWire.ReadUInt(frame, 8);
            var length = OriginalSessionWire.ReadUInt(frame, 12);
            var offset = OriginalSessionWire.ReadUInt(frame, 16);
            var count = frame.Length - OriginalSessionWire.HeaderBytes;
            if (id == 0 || id > long.MaxValue || length == 0 || length > OriginalSessionWire.MaximumMessageBytes ||
                offset > length || count > length - offset)
                return Reject("invalid-response-size");
            if (pending == null)
            {
                if (completedId == long.MaxValue || (long)id != completedId + 1 || offset != 0)
                    return Reject("invalid-response-order");
                pending = new byte[(int)length]; currentId = (long)id;
            }
            if ((long)id != currentId || length != pending.Length || offset != used)
                return Reject("interleaved-response-fragment");
            Buffer.BlockCopy(frame, OriginalSessionWire.HeaderBytes, pending, used, count);
            used += count;
            if (used == pending.Length)
            {
                message = pending; pending = null; used = 0; completedId = currentId;
            }
            return true;
        }

        internal bool Reject(string reason)
        {
            Failed = true; FailureReason = reason; pending = null; used = 0;
            return false;
        }
    }

    // Owns the routing of every peer on this host. Call on the session's single
    // simulation thread; do not also invoke Session.Apply for remote connections.
    public sealed class OriginalNetworkSession : IDisposable
    {
        private sealed class Channel
        {
            public OriginalTcpPeer peer;
            public int slot, replyBytes, offset;
            public long nextMessageId;
            public double acceptedAt;
            public readonly Queue<byte[]> replies = new Queue<byte[]>();
            public byte[] snapshot, active;
            public bool CanAcceptCommand => replies.Count < 16 && replyBytes <= 256 * 1024 - OriginalFrameDecoder.MaximumPayload;
        }

        private readonly OriginalTcpHost host;
        private readonly OriginalSession session;
        private readonly IOriginalSessionCodec codec;
        private readonly bool ownsHost;
        private readonly Func<double> now;
        private readonly Dictionary<long, Channel> channels = new Dictionary<long, Channel>();
        private long lastRevision = -1;
        private string lastHalt;
        private bool disposed;
        public string FaultReason { get; private set; }

        public OriginalNetworkSession(OriginalTcpHost host, OriginalSession session, IOriginalSessionCodec codec,
            bool ownsHost = false, Func<double> monotonicSeconds = null)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
            var initial = session.Snapshot();
            if (initial.players.Length != 1 || initial.players[0].slot != 1)
                throw new ArgumentException("Attach the adapter before admitting remote players.", nameof(session));
            this.ownsHost = ownsHost;
            now = monotonicSeconds ?? (() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency);
        }

        public OriginalSessionReplyCode ApplyLocal(OriginalSessionCommand command)
        {
            ThrowIfDisposed();
            if (FaultReason != null) return OriginalSessionReplyCode.RuleUnavailable;
            var result = session.Apply(OriginalSession.LocalHostConnection, command);
            RefreshSnapshots(false);
            return result;
        }

        public void Advance(double seconds)
        {
            ThrowIfDisposed();
            if (FaultReason != null) return;
            session.Advance(seconds); RefreshSnapshots(false);
        }

        public bool SetOptions(OriginalMatchOptions options)
        {
            ThrowIfDisposed();
            if (FaultReason != null || !session.SetOptions(options)) return false;
            RefreshSnapshots(false); return true;
        }

        public void BroadcastSnapshot() { ThrowIfDisposed(); RefreshSnapshots(true); }

        public void Pump()
        {
            ThrowIfDisposed();
            if (FaultReason != null) return;
            host.Pump(); ReapDisconnected();
            var time = now();
            if (double.IsNaN(time) || double.IsInfinity(time)) throw new InvalidOperationException("Invalid monotonic clock.");
            foreach (var peer in host.Peers)
                if (!channels.ContainsKey(peer.ConnectionId))
                {
                    if (peer.ConnectionId <= 0) { Fail("invalid-host-connection-id"); return; }
                    channels.Add(peer.ConnectionId, new Channel { peer = peer, acceptedAt = time });
                }
            foreach (var channel in new List<Channel>(channels.Values))
            {
                if (channel.slot == 0 && time - channel.acceptedAt >= 30)
                { channel.peer.Close("handshake-timeout"); continue; }
                Flush(channel);
                for (var budget = 0; budget < 8 && channel.peer.IsConnected && channel.CanAcceptCommand; budget++)
                {
                    var bytes = channel.peer.Receive();
                    if (bytes == null) break;
                    OriginalSessionCommand command;
                    try
                    {
                        if (!codec.TryDecodeCommand(bytes, out command) || command == null)
                        { channel.peer.Close("malformed-command"); break; }
                    }
                    catch (Exception) { channel.peer.Close("malformed-command"); break; }
                    var before = session.Snapshot();
                    var code = session.Apply(channel.peer.ConnectionId, command);
                    var after = session.Snapshot();
                    if (command.kind == OriginalSessionCommandKind.Hello && code == OriginalSessionReplyCode.Accepted)
                        foreach (var player in after.players)
                            if (Array.Find(before.players, x => x.slot == player.slot) == null) channel.slot = player.slot;
                    var own = Array.Find(after.players, x => x.slot == channel.slot);
                    var reply = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Reply, code = code,
                        commandSequence = command.sequence, assignedSlot = channel.slot, acknowledgedSequence = own?.acknowledgedSequence ?? 0 };
                    byte[] encoded;
                    try { encoded = codec.EncodeResponse(reply); }
                    catch (Exception) { Fail("response-codec-failed"); return; }
                    if (encoded == null || encoded.Length == 0 || encoded.Length > OriginalFrameDecoder.MaximumPayload)
                    { Fail("reply-exceeds-frame-limit"); return; }
                    channel.replies.Enqueue(encoded); channel.replyBytes += encoded.Length;
                }
            }
            ReapDisconnected(); RefreshSnapshots(false);
            if (FaultReason != null) return;
            foreach (var channel in channels.Values) Flush(channel);
            host.Pump(); ReapDisconnected(); RefreshSnapshots(false);
        }

        private void RefreshSnapshots(bool force)
        {
            if (FaultReason != null) return;
            var view = session.Snapshot();
            if (!force && view.revision == lastRevision && view.haltReason == lastHalt) return;
            lastRevision = view.revision; lastHalt = view.haltReason;
            foreach (var channel in channels.Values)
            {
                if (channel.slot == 0 || !channel.peer.IsConnected) continue;
                var own = Array.Find(view.players, x => x.slot == channel.slot);
                var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot,
                    code = OriginalSessionReplyCode.Accepted, assignedSlot = channel.slot,
                    acknowledgedSequence = own?.acknowledgedSequence ?? 0, snapshot = view };
                byte[] bytes;
                try { bytes = codec.EncodeResponse(response); }
                catch (Exception) { Fail("response-codec-failed"); return; }
                if (bytes == null || bytes.Length == 0 || bytes.Length > OriginalSessionWire.MaximumMessageBytes)
                { Fail("snapshot-exceeds-message-limit"); return; }
                // Only an unstarted snapshot can be replaced. An active message
                // finishes first, so fragments from different snapshots never mix.
                channel.snapshot = bytes;
            }
        }

        private static void Flush(Channel channel)
        {
            for (var budget = 0; budget < 32 && channel.peer.IsConnected; budget++)
            {
                if (channel.active == null)
                {
                    if (channel.replies.Count > 0)
                    {
                        channel.active = channel.replies.Dequeue();
                        channel.replyBytes -= channel.active.Length;
                    }
                    else { channel.active = channel.snapshot; channel.snapshot = null; }
                    if (channel.active == null) return;
                    if (channel.nextMessageId == long.MaxValue) { channel.peer.Close("message-id-exhausted"); return; }
                    channel.nextMessageId++; channel.offset = 0;
                }
                var fragment = OriginalSessionWire.Fragment(channel.active, channel.nextMessageId, channel.offset);
                if (!channel.peer.Send(fragment)) return;
                channel.offset += fragment.Length - OriginalSessionWire.HeaderBytes;
                if (channel.offset == channel.active.Length) channel.active = null;
            }
        }

        private void ReapDisconnected()
        {
            foreach (var peer in new List<OriginalTcpPeer>(host.Peers))
                if (!peer.IsConnected)
                {
                    session.Disconnect(peer.ConnectionId);
                    channels.Remove(peer.ConnectionId); host.Release(peer.ConnectionId);
                }
        }

        private void Fail(string reason)
        {
            FaultReason = reason;
            foreach (var peer in host.Peers) peer.Close(reason);
            ReapDisconnected();
        }

        private void ThrowIfDisposed() { if (disposed) throw new ObjectDisposedException(nameof(OriginalNetworkSession)); }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var peer in host.Peers) peer.Close("session-disposed");
            ReapDisconnected(); channels.Clear();
            if (ownsHost) host.Dispose();
        }
    }

    // Client callers assign command sequences and consume Reply results. A false
    // Send means no bytes were queued; retry that same sequence after pumping.
    public sealed class OriginalNetworkClient : IDisposable
    {
        private sealed class Received { public OriginalNetworkResponse response; public int bytes; }
        private readonly OriginalTcpPeer peer;
        private readonly IOriginalSessionCodec codec;
        private readonly OriginalSessionWireReader reader = new OriginalSessionWireReader();
        private readonly Queue<Received> received = new Queue<Received>();
        private int queuedBytes;
        private bool disposed;
        private string expectedHash;
        public int AssignedSlot { get; private set; }
        public long AcknowledgedSequence { get; private set; }
        public bool IsConnected => !disposed && peer.IsConnected;
        public string DisconnectReason => peer.DisconnectReason;
        public OriginalSessionView LatestSnapshot { get; private set; }

        public OriginalNetworkClient(OriginalTcpPeer peer, IOriginalSessionCodec codec)
        {
            this.peer = peer ?? throw new ArgumentNullException(nameof(peer));
            this.codec = codec ?? throw new ArgumentNullException(nameof(codec));
        }

        public bool Send(OriginalSessionCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (!IsConnected) return false;
            var bytes = codec.EncodeCommand(command);
            if (bytes == null || bytes.Length == 0 || bytes.Length > OriginalFrameDecoder.MaximumPayload)
                throw new ArgumentOutOfRangeException(nameof(command), "Command exceeds transport payload limit.");
            if (!peer.Send(bytes)) return false;
            if (command.kind == OriginalSessionCommandKind.Hello && AssignedSlot == 0) expectedHash = command.contentHash;
            return true;
        }

        public void Pump()
        {
            if (disposed) return;
            peer.Pump();
            // Reserve room for a maximum-sized next message before consuming any
            // fragment. This bounds queued encoded sizes to 8 MiB plus one reader.
            for (var budget = 0; budget < 64 && received.Count < 32 &&
                queuedBytes <= OriginalSessionWire.MaximumMessageBytes; budget++)
            {
                var frame = peer.Receive();
                if (frame == null) break;
                if (!reader.Feed(frame, out var bytes)) { peer.Close(reader.FailureReason); return; }
                if (bytes == null) continue;
                OriginalNetworkResponse response;
                try
                {
                    if (!codec.TryDecodeResponse(bytes, out response) || response == null)
                    { Close("malformed-response"); return; }
                }
                catch (Exception) { Close("malformed-response"); return; }
                if (!Enum.IsDefined(typeof(OriginalNetworkResponseKind), response.kind) ||
                    !Enum.IsDefined(typeof(OriginalSessionReplyCode), response.code) || response.assignedSlot < 0 ||
                    response.assignedSlot > 8 || response.acknowledgedSequence < 0)
                { Close("invalid-response-envelope"); return; }
                if (response.kind == OriginalNetworkResponseKind.Snapshot)
                {
                    if (response.snapshot == null || response.snapshot.protocol != OriginalSession.Protocol ||
                        expectedHash == null || response.snapshot.contentHash != expectedHash)
                    { Close("snapshot-content-mismatch"); return; }
                    if (LatestSnapshot == null || response.snapshot.revision >= LatestSnapshot.revision)
                        LatestSnapshot = response.snapshot;
                }
                if (response.assignedSlot > 0) AssignedSlot = response.assignedSlot;
                AcknowledgedSequence = Math.Max(AcknowledgedSequence, response.acknowledgedSequence);
                received.Enqueue(new Received { response = response, bytes = bytes.Length }); queuedBytes += bytes.Length;
            }
            if (!peer.IsConnected && reader.BufferedBytes > 0) reader.Reject("truncated-response");
        }

        public OriginalNetworkResponse Receive()
        {
            if (received.Count == 0) return null;
            var result = received.Dequeue(); queuedBytes -= result.bytes; return result.response;
        }

        private void Close(string reason) { reader.Reject(reason); peer.Close(reason); }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true; reader.Reject("disposed"); received.Clear(); queuedBytes = 0; peer.Dispose();
        }
    }
}

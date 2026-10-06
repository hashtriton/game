using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Arena.Original
{
    // Transport only. Payload decoding and authoritative command validation
    // belong to the session; a socket never supplies a trusted player slot.
    public sealed class OriginalFrameDecoder
    {
        public const int MaximumPayload = 64 * 1024;
        public const int MaximumQueuedBytes = 256 * 1024;
        private readonly byte[] header = new byte[4];
        private readonly Queue<byte[]> frames = new Queue<byte[]>();
        private byte[] payload;
        private int headerUsed, payloadUsed, queuedBytes;
        public bool Failed { get; private set; }
        public int Count => frames.Count;
        public bool CanReceive => !Failed && frames.Count < 32 && queuedBytes < MaximumQueuedBytes;

        public bool Feed(byte[] bytes, int offset, int count, out int consumed)
        {
            consumed = 0;
            if (bytes == null || offset < 0 || count < 0 || offset > bytes.Length - count)
                throw new ArgumentOutOfRangeException();
            if (Failed) return false;
            while (count > 0)
            {
                if (payload == null)
                {
                    var take = Math.Min(4 - headerUsed, count);
                    Buffer.BlockCopy(bytes, offset, header, headerUsed, take);
                    headerUsed += take; offset += take; count -= take; consumed += take;
                    if (headerUsed != 4) continue;
                    var length = (uint)header[0] << 24 | (uint)header[1] << 16 | (uint)header[2] << 8 | header[3];
                    if (length == 0 || length > MaximumPayload)
                    {
                        Failed = true;
                        frames.Clear(); queuedBytes = 0;
                        return false;
                    }
                    // Keep the parsed header and let the owner retain any
                    // unread bytes until its command consumer drains frames.
                    if (frames.Count >= 32 || queuedBytes + length > MaximumQueuedBytes) return true;
                    payload = new byte[(int)length];
                }
                var copied = Math.Min(payload.Length - payloadUsed, count);
                Buffer.BlockCopy(bytes, offset, payload, payloadUsed, copied);
                payloadUsed += copied; offset += copied; count -= copied; consumed += copied;
                if (payloadUsed == payload.Length)
                {
                    frames.Enqueue(payload); queuedBytes += payload.Length;
                    payload = null; payloadUsed = headerUsed = 0;
                }
            }
            return true;
        }

        public byte[] Take()
        {
            if (frames.Count == 0) return null;
            var result = frames.Dequeue(); queuedBytes -= result.Length;
            return result;
        }

        public static byte[] Encode(byte[] payload)
        {
            if (payload == null || payload.Length == 0 || payload.Length > MaximumPayload)
                throw new ArgumentOutOfRangeException(nameof(payload));
            var result = new byte[4 + payload.Length];
            result[0] = (byte)(payload.Length >> 24); result[1] = (byte)(payload.Length >> 16);
            result[2] = (byte)(payload.Length >> 8); result[3] = (byte)payload.Length;
            Buffer.BlockCopy(payload, 0, result, 4, payload.Length);
            return result;
        }
    }

    public sealed class OriginalTcpPeer : IDisposable
    {
        private readonly Socket socket;
        private readonly OriginalFrameDecoder decoder = new OriginalFrameDecoder();
        private readonly Queue<byte[]> outgoing = new Queue<byte[]>();
        private readonly byte[] readBuffer = new byte[4096];
        private int writeOffset, queuedBytes, readOffset, readRemaining;
        public long ConnectionId { get; }
        public bool IsConnected { get; private set; } = true;
        public string DisconnectReason { get; private set; }
        public int PendingFrames => decoder.Count;

        public OriginalTcpPeer(Socket connectedSocket, long connectionId)
        {
            socket = connectedSocket ?? throw new ArgumentNullException(nameof(connectedSocket));
            if (!socket.Connected) throw new ArgumentException("Socket is not connected.");
            ConnectionId = connectionId;
            socket.Blocking = false;
            socket.NoDelay = true;
        }

        public bool Send(byte[] payload)
        {
            if (!IsConnected) return false;
            var frame = OriginalFrameDecoder.Encode(payload);
            if (queuedBytes + frame.Length > OriginalFrameDecoder.MaximumQueuedBytes) return false;
            outgoing.Enqueue(frame); queuedBytes += frame.Length;
            return true;
        }

        public byte[] Receive() => decoder.Take();

        public void Pump()
        {
            if (!IsConnected) return;
            try
            {
                var receiveReady = readRemaining == 0 || ConsumeReadBuffer();
                if (!IsConnected) return;
                for (var attempt = 0; receiveReady && attempt < 16 && socket.Poll(0, SelectMode.SelectRead); attempt++)
                {
                    if (!decoder.CanReceive) break;
                    var count = socket.Receive(readBuffer, 0, readBuffer.Length, SocketFlags.None);
                    if (count == 0) { Close("peer-closed"); return; }
                    readOffset = 0; readRemaining = count;
                    if (!ConsumeReadBuffer()) break;
                }
                if (!IsConnected) return;
                var budget = 64 * 1024;
                while (outgoing.Count > 0 && budget > 0 && socket.Poll(0, SelectMode.SelectWrite))
                {
                    var frame = outgoing.Peek();
                    var count = socket.Send(frame, writeOffset, Math.Min(frame.Length - writeOffset, budget), SocketFlags.None);
                    if (count == 0) { Close("send-closed"); return; }
                    writeOffset += count; budget -= count;
                    if (writeOffset == frame.Length)
                    {
                        outgoing.Dequeue(); queuedBytes -= frame.Length; writeOffset = 0;
                    }
                }
            }
            catch (SocketException exception)
            {
                if (exception.SocketErrorCode != SocketError.WouldBlock && exception.SocketErrorCode != SocketError.IOPending)
                    Close("socket-" + exception.SocketErrorCode);
            }
            catch (ObjectDisposedException) { Close("socket-disposed"); }
        }

        private bool ConsumeReadBuffer()
        {
            if (!decoder.Feed(readBuffer, readOffset, readRemaining, out var consumed))
            { Close("invalid-frame"); return false; }
            readOffset += consumed; readRemaining -= consumed;
            return readRemaining == 0;
        }

        public void Close(string reason)
        {
            if (!IsConnected) return;
            IsConnected = false; DisconnectReason = reason;
            outgoing.Clear(); queuedBytes = 0;
            socket.Dispose();
        }

        public void Dispose() => Close("disposed");
    }

    public sealed class OriginalTcpHost : IDisposable
    {
        private readonly Socket listener;
        private readonly List<OriginalTcpPeer> peers = new List<OriginalTcpPeer>();
        private long nextConnectionId;
        private bool disposed;
        public int Port { get; }
        public IPAddress BindAddress { get; }
        public IReadOnlyList<OriginalTcpPeer> Peers => peers.AsReadOnly();

        // LAN binding must be explicitly selected by the local host UI.
        public OriginalTcpHost(int port = 0, IPAddress bindAddress = null)
        {
            if (port < 0 || port > 65535) throw new ArgumentOutOfRangeException(nameof(port));
            BindAddress = bindAddress ?? IPAddress.Loopback;
            listener = new Socket(BindAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(new IPEndPoint(BindAddress, port));
                listener.Listen(8);
                listener.Blocking = false;
                Port = ((IPEndPoint)listener.LocalEndPoint).Port;
            }
            catch { listener.Dispose(); throw; }
        }

        public void Pump()
        {
            if (disposed) return;
            // Closed peers remain available until the session acknowledges the
            // disconnect, so cleanup cannot silently remove a participant.
            for (var accepted = 0; accepted < 8 && listener.Poll(0, SelectMode.SelectRead); accepted++)
            {
                Socket socket;
                try { socket = listener.Accept(); }
                catch (SocketException exception) when (exception.SocketErrorCode == SocketError.WouldBlock) { break; }
                if (peers.Count >= 8) { socket.Dispose(); continue; }
                peers.Add(new OriginalTcpPeer(socket, ++nextConnectionId));
            }
            foreach (var peer in peers) peer.Pump();
        }

        public bool Release(long connectionId)
        {
            var index = peers.FindIndex(peer => peer.ConnectionId == connectionId);
            if (index < 0) return false;
            peers[index].Dispose(); peers.RemoveAt(index);
            return true;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var peer in peers) peer.Dispose();
            peers.Clear(); listener.Dispose();
        }
    }
}

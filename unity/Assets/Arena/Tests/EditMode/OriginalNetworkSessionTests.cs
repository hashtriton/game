using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Threading;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalNetworkSessionTests
    {
        private const string SourceHash = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        private static readonly string ContentHash = new string('a', 64);

        [Test]
        public void Wire_ReassemblesLargeMessagesWithoutTruncation()
        {
            var bytes = new byte[OriginalSessionWire.MaximumMessageBytes];
            new Random(981).NextBytes(bytes);
            var decoder = new OriginalSessionWireReader();
            byte[] result = null;
            for (var offset = 0; offset < bytes.Length;)
            {
                var fragment = OriginalSessionWire.Fragment(bytes, 1, offset);
                Assert.That(fragment.Length, Is.LessThanOrEqualTo(OriginalFrameDecoder.MaximumPayload));
                Assert.That(decoder.Feed(fragment, out result), Is.True);
                offset += fragment.Length - OriginalSessionWire.HeaderBytes;
            }
            Assert.That(result, Is.EqualTo(bytes));
            Assert.That(decoder.BufferedBytes, Is.Zero);
        }

        [Test]
        public void Wire_RejectsInterleavedRepeatedAndOversizedMessages()
        {
            var data = new byte[70000];
            var reader = new OriginalSessionWireReader();
            Assert.That(reader.Feed(OriginalSessionWire.Fragment(data, 1, 0), out _), Is.True);
            Assert.That(reader.Feed(OriginalSessionWire.Fragment(data, 2, 0), out _), Is.False);
            Assert.That(reader.Failed, Is.True);
            Assert.That(reader.BufferedBytes, Is.Zero);
            reader = new OriginalSessionWireReader();
            var single = OriginalSessionWire.Fragment(new byte[] { 7 }, 1, 0);
            Assert.That(reader.Feed(single, out _), Is.True);
            Assert.That(reader.Feed(single, out _), Is.False);
            reader = new OriginalSessionWireReader();
            var oversized = (byte[])single.Clone();
            oversized[12] = 0; oversized[13] = 64; oversized[14] = 0; oversized[15] = 1;
            Assert.That(reader.Feed(oversized, out _), Is.False);
            Assert.That(reader.BufferedBytes, Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalSessionWire.Fragment(new byte[OriginalSessionWire.MaximumMessageBytes + 1], 1, 0));
        }

        [Test]
        public void Loopback_HandshakeBindsSlotToSocketAndRejectsReplayAndRemoteStart()
        {
            using (var fixture = new Connection())
            {
                var bad = Command(OriginalSessionCommandKind.Hello, 1); bad.contentHash = new string('b', 64);
                Assert.That(fixture.Client.Send(bad), Is.True);
                Assert.That(fixture.WaitReply(1).code, Is.EqualTo(OriginalSessionReplyCode.VersionMismatch));
                Assert.That(fixture.Session.Snapshot().players.Length, Is.EqualTo(1));
                fixture.Join();
                Assert.That(fixture.Client.AssignedSlot, Is.EqualTo(2));
                fixture.Client.Send(Command(OriginalSessionCommandKind.SelectHero, 2, "H024"));
                Assert.That(fixture.WaitReply(2).code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
                fixture.Client.Send(Command(OriginalSessionCommandKind.SelectHero, 2, "H008"));
                var replay = fixture.WaitReply(2);
                Assert.That(replay.code, Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
                Assert.That(replay.acknowledgedSequence, Is.EqualTo(2));
                fixture.Client.Send(Command(OriginalSessionCommandKind.Start, 3));
                Assert.That(fixture.WaitReply(3).code, Is.EqualTo(OriginalSessionReplyCode.NotHost));
                Assert.That(fixture.Session.Snapshot().players.Single(x => x.slot == 2).heroId, Is.EqualTo("H024"));
            }
        }

        [Test]
        public void Loopback_HostStartBroadcastsOriginalResourcesAndDisconnectsAuthoritatively()
        {
            using (var fixture = new Connection())
            {
                fixture.Join();
                fixture.Network.ApplyLocal(Command(OriginalSessionCommandKind.SelectHero, 1, "H008"));
                fixture.Network.ApplyLocal(Command(OriginalSessionCommandKind.LobbyReady, 2));
                fixture.Client.Send(Command(OriginalSessionCommandKind.SelectHero, 2, "H024")); fixture.WaitReply(2);
                fixture.Client.Send(Command(OriginalSessionCommandKind.LobbyReady, 3)); fixture.WaitReply(3);
                Assert.That(fixture.Network.ApplyLocal(Command(OriginalSessionCommandKind.Start, 3)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                fixture.Wait(() => fixture.Client.LatestSnapshot != null && fixture.Client.LatestSnapshot.started);
                Assert.That(fixture.Client.LatestSnapshot.players.Select(x => x.gold), Is.All.EqualTo(130));
                Assert.That(fixture.Client.LatestSnapshot.players.Select(x => x.souls), Is.All.EqualTo(4));
                fixture.Client.Dispose();
                fixture.Wait(() => !fixture.Session.Snapshot().players.Single(x => x.slot == 2).connected);
                Assert.That(fixture.Session.HaltReason, Is.Null);
                Assert.That(fixture.Session.Snapshot().players.Single(x => x.slot == 2).connected, Is.False);
                Assert.That(fixture.Host.Peers, Is.Empty);
            }
        }

        [Test]
        public void Loopback_LargeSnapshotAndQueuedRepliesSurviveBackpressure()
        {
            using (var fixture = new Connection(340000))
            {
                fixture.Join();
                for (var i = 2; i <= 70; i++) Assert.That(fixture.Client.Send(Command(OriginalSessionCommandKind.SelectHero, i, "H024")), Is.True);
                var acknowledged = new List<long>();
                fixture.Wait(() =>
                {
                    for (var i = fixture.Responses.Count - 1; i >= 0; i--)
                        if (fixture.Responses[i].kind == OriginalNetworkResponseKind.Reply)
                        { acknowledged.Add(fixture.Responses[i].commandSequence); fixture.Responses.RemoveAt(i); }
                    return acknowledged.Count == 69 && fixture.Client.LatestSnapshot.players.Single(x => x.slot == 2).acknowledgedSequence == 70;
                });
                Assert.That(acknowledged.OrderBy(x => x), Is.EqualTo(Enumerable.Range(2, 69).Select(x => (long)x)));
                Assert.That(fixture.Client.IsConnected, Is.True);
                Assert.That(fixture.Network.FaultReason, Is.Null);
            }
        }

        [Test]
        public void Loopback_MalformedPayloadRemovesJoinedPlayerWithoutExecutingACommand()
        {
            using (var fixture = new Connection())
            {
                fixture.Join();
                Assert.That(fixture.Peer.Send(new byte[] { 255, 254, 0 }), Is.True);
                fixture.Wait(() => fixture.Session.Snapshot().players.Length == 1);
                Assert.That(fixture.Session.Started, Is.False);
                Assert.That(fixture.Host.Peers, Is.Empty);
            }
        }

        [Test]
        public void Loopback_OversizedSnapshotIsAnExplicitHostFault()
        {
            using (var fixture = new Connection(OriginalSessionWire.MaximumMessageBytes + 1))
            {
                fixture.Client.Send(Command(OriginalSessionCommandKind.Hello, 1));
                fixture.Wait(() => fixture.Network.FaultReason != null);
                Assert.That(fixture.Network.FaultReason, Is.EqualTo("snapshot-exceeds-message-limit"));
                Assert.That(fixture.Host.Peers, Is.Empty);
                Assert.That(fixture.Session.Snapshot().players.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void Loopback_HandshakeDeadlineReleasesAnUnjoinedSocket()
        {
            var time = 5.0;
            using (var fixture = new Connection(monotonicSeconds: () => time))
            {
                fixture.Network.Pump();
                Assert.That(fixture.Host.Peers.Count, Is.EqualTo(1));
                time = 34.999; fixture.Network.Pump();
                Assert.That(fixture.Host.Peers.Count, Is.EqualTo(1));
                time = 35.0; fixture.Network.Pump();
                Assert.That(fixture.Host.Peers, Is.Empty);
                Assert.That(fixture.Session.Snapshot().players.Length, Is.EqualTo(1));
                Assert.That(fixture.Network.FaultReason, Is.Null);
            }
        }

        [Test]
        public void Loopback_SeparateSocketsKeepTheirSlotsAndReceiveLobbyDisconnectUpdates()
        {
            using (var fixture = new Connection())
            {
                fixture.Join();
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(IPAddress.Loopback, fixture.Host.Port);
                using (var second = new OriginalNetworkClient(new OriginalTcpPeer(socket, 0), new JsonCodec()))
                {
                    second.Send(Command(OriginalSessionCommandKind.Hello, 1));
                    fixture.Wait(() => { second.Pump(); while (second.Receive() != null) { } return second.AssignedSlot == 3; });
                    fixture.Client.Send(Command(OriginalSessionCommandKind.SelectHero, 2, "H024"));
                    fixture.WaitReply(2);
                    second.Send(Command(OriginalSessionCommandKind.SelectHero, 2, "N0A0"));
                    fixture.Wait(() =>
                    {
                        second.Pump(); while (second.Receive() != null) { }
                        return second.LatestSnapshot?.players.Single(x => x.slot == 3).heroId == "N0A0" &&
                            fixture.Client.LatestSnapshot.players.Length == 3 &&
                            fixture.Client.LatestSnapshot.players.Single(x => x.slot == 3).heroId == "N0A0";
                    });
                    Assert.That(second.LatestSnapshot.players.Single(x => x.slot == 2).heroId, Is.EqualTo("H024"));
                    second.Dispose();
                    fixture.Wait(() => fixture.Client.LatestSnapshot.players.Length == 2);
                    Assert.That(fixture.Client.AssignedSlot, Is.EqualTo(2));
                    Assert.That(fixture.Session.HaltReason, Is.Null);
                }
            }
        }

        private static OriginalSessionCommand Command(OriginalSessionCommandKind kind, long sequence, string hero = null) =>
            new OriginalSessionCommand { protocol = OriginalSession.Protocol, contentHash = ContentHash, kind = kind, sequence = sequence, heroId = hero, ready = true };

        private static OriginalSession CreateSession()
        {
            // Catalog fixture supplies only declarations required by lobby and first-round rules.
            var match = new OriginalMatchCatalog { schemaVersion = 1, sourceSha256 = SourceHash,
                waves = Enumerable.Range(1, 30).Select(x => new OriginalWaveDefinition
                    { round = x, bossId = "boss", regularId = "crep", casterId = "cast" }).ToArray(),
                enemies = new[] { "boss", "crep", "cast" }.Select(x => new OriginalEnemyDefinition { id = x }).ToArray() };
            var heroes = new[] { "H008", "H024", "N0A0" };
            var combat = new OriginalCombatCatalog { sourceSha256 = SourceHash,
                units = heroes.Select(x => new OriginalCombatDefinition { id = x }).ToArray(),
                selectedHeroes = heroes.Select(x => new OriginalHeroDefinition { id = x }).ToArray() };
            return new OriginalSession(match, new OriginalItemCatalog { mapSha256 = SourceHash }, combat,
                UnityEngine.JsonUtility.FromJson<OriginalDuelCatalog>(System.IO.File.ReadAllText(
                    System.IO.Path.Combine(UnityEngine.Application.dataPath, "Arena/Data/lia39-duels.json"))),
                ContentHash, OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
        }

        private sealed class JsonCodec : IOriginalSessionCodec
        {
            public int SnapshotPadding;
            public byte[] EncodeCommand(OriginalSessionCommand command) => Encode(command);
            public bool TryDecodeCommand(byte[] bytes, out OriginalSessionCommand command) => Decode(bytes, out command);
            public bool TryDecodeResponse(byte[] bytes, out OriginalNetworkResponse response) => Decode(bytes, out response);
            public byte[] EncodeResponse(OriginalNetworkResponse response)
            {
                var bytes = Encode(response);
                if (response.kind == OriginalNetworkResponseKind.Snapshot && bytes.Length < SnapshotPadding)
                {
                    var length = bytes.Length; Array.Resize(ref bytes, SnapshotPadding);
                    for (var i = length; i < bytes.Length; i++) bytes[i] = 32;
                }
                return bytes;
            }
            private static byte[] Encode<T>(T value)
            {
                using (var stream = new MemoryStream())
                { new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value); return stream.ToArray(); }
            }
            private static bool Decode<T>(byte[] bytes, out T value) where T : class
            {
                try { using (var stream = new MemoryStream(bytes)) value = new DataContractJsonSerializer(typeof(T)).ReadObject(stream) as T; return value != null; }
                catch (Exception) { value = null; return false; }
            }
        }

        private sealed class Connection : IDisposable
        {
            public readonly OriginalTcpHost Host = new OriginalTcpHost();
            public readonly OriginalSession Session = CreateSession();
            public readonly OriginalNetworkSession Network;
            public readonly OriginalTcpPeer Peer;
            public readonly OriginalNetworkClient Client;
            public readonly List<OriginalNetworkResponse> Responses = new List<OriginalNetworkResponse>();
            public Connection(int padding = 0, Func<double> monotonicSeconds = null)
            {
                var codec = new JsonCodec { SnapshotPadding = padding };
                Network = new OriginalNetworkSession(Host, Session, codec, monotonicSeconds: monotonicSeconds);
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(IPAddress.Loopback, Host.Port);
                Peer = new OriginalTcpPeer(socket, 0);
                Client = new OriginalNetworkClient(Peer, codec);
            }
            public void Join()
            {
                Client.Send(Command(OriginalSessionCommandKind.Hello, 1));
                Assert.That(WaitReply(1).code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Wait(() => Client.LatestSnapshot != null);
            }
            public OriginalNetworkResponse WaitReply(long sequence)
            {
                Wait(() => Responses.Any(x => x.kind == OriginalNetworkResponseKind.Reply && x.commandSequence == sequence));
                var response = Responses.First(x => x.kind == OriginalNetworkResponseKind.Reply && x.commandSequence == sequence);
                Responses.Remove(response); return response;
            }
            public void Wait(Func<bool> condition)
            {
                var clock = Stopwatch.StartNew();
                while (!condition() && clock.ElapsedMilliseconds < 5000)
                {
                    Network.Pump(); Client.Pump();
                    OriginalNetworkResponse response;
                    while ((response = Client.Receive()) != null) Responses.Add(response);
                    if (!condition()) Thread.Sleep(1);
                }
                Assert.That(condition(), Is.True, "Loopback session did not reach the expected state.");
            }
            public void Dispose() { Client.Dispose(); Network.Dispose(); Host.Dispose(); }
        }
    }
}

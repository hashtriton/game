using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalNetworkTransportTests
    {
        [Test]
        public void Frames_ReassembleBytewiseAndKeepMessageBoundaries()
        {
            var first = Encoding.UTF8.GetBytes("Рыцарь");
            var second = Encoding.UTF8.GetBytes("Пиромант");
            var bytes = OriginalFrameDecoder.Encode(first).Concat(OriginalFrameDecoder.Encode(second)).ToArray();
            var decoder = new OriginalFrameDecoder();
            foreach (var value in bytes) Assert.That(decoder.Feed(new[] { value }, 0, 1, out _), Is.True);
            Assert.That(decoder.Take(), Is.EqualTo(first));
            Assert.That(decoder.Take(), Is.EqualTo(second));
            Assert.That(decoder.Take(), Is.Null);
        }

        [TestCase(0u)]
        [TestCase(65537u)]
        [TestCase(uint.MaxValue)]
        public void Frames_RejectInvalidLengthsBeforePayloadAllocation(uint size)
        {
            var decoder = new OriginalFrameDecoder();
            var header = new[] { (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size };
            Assert.That(decoder.Feed(header, 0, 4, out _), Is.False);
            Assert.That(decoder.Failed, Is.True);
            Assert.That(decoder.Count, Is.Zero);
        }

        [Test]
        public void Frames_BackpressureRetainsValidMessageAfterQueueDrains()
        {
            var decoder = new OriginalFrameDecoder();
            var frame = OriginalFrameDecoder.Encode(new byte[] { 1 });
            for (var i = 0; i < 32; i++) Assert.That(decoder.Feed(frame, 0, frame.Length, out _), Is.True);
            Assert.That(decoder.Feed(frame, 0, frame.Length, out var consumed), Is.True);
            Assert.That(consumed, Is.EqualTo(4));
            Assert.That(decoder.Count, Is.EqualTo(32));
            Assert.That(decoder.Failed, Is.False);
            decoder.Take();
            Assert.That(decoder.Feed(frame, consumed, frame.Length - consumed, out var remainder), Is.True);
            Assert.That(remainder, Is.EqualTo(1));
            Assert.That(decoder.Count, Is.EqualTo(32));
        }

        [Test]
        public void TwoLoopbackClients_ExchangeFramesAndDisconnectIndependently()
        {
            using (var host = new OriginalTcpHost())
            using (var firstSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            using (var secondSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                Assert.That(host.BindAddress, Is.EqualTo(IPAddress.Loopback));
                firstSocket.Connect(IPAddress.Loopback, host.Port);
                secondSocket.Connect(IPAddress.Loopback, host.Port);
                using (var first = new OriginalTcpPeer(firstSocket, 0))
                using (var second = new OriginalTcpPeer(secondSocket, 0))
                {
                    PumpUntil(host, first, second, () => host.Peers.Count == 2);
                    Assert.That(host.Peers.Select(x => x.ConnectionId).Distinct().Count(), Is.EqualTo(2));
                    var firstMessage = Encoding.UTF8.GetBytes("first-command");
                    var secondMessage = Encoding.UTF8.GetBytes("second-command");
                    first.Send(firstMessage); second.Send(secondMessage);
                    PumpUntil(host, first, second, () => host.Peers.All(x => x.PendingFrames == 1));
                    Assert.That(host.Peers[0].Receive(), Is.EqualTo(firstMessage));
                    Assert.That(host.Peers[1].Receive(), Is.EqualTo(secondMessage));
                    var snapshot = Enumerable.Range(0, OriginalFrameDecoder.MaximumPayload).Select(x => (byte)x).ToArray();
                    foreach (var peer in host.Peers) Assert.That(peer.Send(snapshot), Is.True);
                    PumpUntil(host, first, second, () => first.PendingFrames == 1 && second.PendingFrames == 1);
                    Assert.That(first.Receive(), Is.EqualTo(snapshot));
                    Assert.That(second.Receive(), Is.EqualTo(snapshot));
                    for (var i = 0; i < 33; i++) Assert.That(first.Send(new[] { (byte)i }), Is.True);
                    PumpUntil(host, first, second, () => host.Peers[0].PendingFrames == 32);
                    for (var i = 0; i < 32; i++) Assert.That(host.Peers[0].Receive(), Is.EqualTo(new[] { (byte)i }));
                    PumpUntil(host, first, second, () => host.Peers[0].PendingFrames == 1);
                    Assert.That(host.Peers[0].Receive(), Is.EqualTo(new byte[] { 32 }));
                    Assert.That(host.Peers[0].IsConnected, Is.True);
                    first.Dispose();
                    PumpUntil(host, first, second, () => !host.Peers[0].IsConnected);
                    Assert.That(host.Peers[1].IsConnected, Is.True);
                    var oldId = host.Peers[0].ConnectionId;
                    Assert.That(host.Release(oldId), Is.True);
                    Assert.That(host.Peers.Single().ConnectionId, Is.Not.EqualTo(oldId));
                }
            }
        }

        private static void PumpUntil(OriginalTcpHost host, OriginalTcpPeer first, OriginalTcpPeer second, Func<bool> done)
        {
            var clock = Stopwatch.StartNew();
            while (!done() && clock.ElapsedMilliseconds < 3000)
            {
                host.Pump(); first.Pump(); second.Pump();
                if (!done()) Thread.Sleep(1);
            }
            Assert.That(done(), Is.True, "Loopback exchange did not complete within three seconds.");
        }
    }
}

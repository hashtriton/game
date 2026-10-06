using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalMotionTests
    {
        // These fixtures are convex half-planes or obstacle-free terrain, for
        // which endpoint occupancy proves segment clearance.
        static OriginalMotion Convex(Func<double, double, double, bool> walkable) =>
            new OriginalMotion(walkable, (a, b, r) => walkable(a.x, a.y, r) && walkable(b.x, b.y, r));
        [Test]
        public void OneBodyCannotTunnelThroughStationaryBodyEvenWithLongOrder()
        {
            var motion = Convex((x, y, r) => true);
            motion.Add(1, 0, 0, 24); motion.Add(2, 96, 0, 24);
            var p = motion.Move(1, 300, 0, 300);
            Assert.That(p.x, Is.InRange(47.99, 48.001));
            Assert.That(p.y, Is.EqualTo(0).Within(.001));
            Assert.That(motion.Position(2).x, Is.EqualTo(96));
        }

        [Test]
        public void NarrowPassageQueuesCreepsAndRemovingFrontBodyAdvancesNext()
        {
            // A 64 WC passage with 24 WC radii admits one body across.
            var motion = Convex((x, y, r) => x - r >= 0 && x + r <= 64);
            motion.Add(1, 32, 0, 24);
            for (int i = 2; i <= 8; i++) motion.Add(i, 32, i * 50, 24);
            for (int tick = 0; tick < 300; tick++)
                for (int i = 2; i <= 8; i++) motion.Move(i, 32, 0, 9);
            for (int i = 2; i <= 8; i++)
            {
                Assert.That(motion.Position(i).y, Is.EqualTo(48 * (i - 1)).Within(.02));
                Assert.That(motion.Position(i).x, Is.InRange(24, 40));
            }
            Assert.That(motion.Remove(2), Is.True);
            for (int tick = 0; tick < 20; tick++) motion.Move(3, 32, 0, 9);
            Assert.That(motion.Position(3).y, Is.EqualTo(48).Within(.02));
        }

        [Test]
        public void TerrainRemainsSolidAndOpeningItAllowsSameOrderToContinue()
        {
            bool barrelAlive = true;
            var motion = Convex((x, y, r) => !barrelAlive || x + r <= 64);
            motion.Add(1, 0, 0, 24);
            Assert.That(motion.Move(1, 160, 0, 160).x, Is.EqualTo(40).Within(.02));
            barrelAlive = false;
            Assert.That(motion.Move(1, 160, 0, 160).x, Is.EqualTo(160).Within(.02));
        }

        [Test]
        public void GlancingBodySlidesWithoutOverlapOrExceedingMovementBudget()
        {
            var motion = Convex((x, y, r) => true);
            motion.Add(1, 0, 30, 24); motion.Add(2, 70, 0, 24);
            var p = motion.Move(1, 200, 30, 100);
            Assert.That(p.x, Is.GreaterThan(40));
            Assert.That(p.y, Is.GreaterThan(30));
            Assert.That(Math.Sqrt((p.x - 70) * (p.x - 70) + p.y * p.y), Is.GreaterThanOrEqualTo(47.999));
            Assert.That(Math.Sqrt(p.x * p.x + (p.y - 30) * (p.y - 30)), Is.LessThanOrEqualTo(100.001));
        }

        [Test]
        public void TouchingBodiesCanSeparateButCannotMoveThroughEachOther()
        {
            var motion = Convex((x, y, r) => true);
            motion.Add(1, 0, 0, 24); motion.Add(2, 48, 0, 24);
            Assert.That(motion.Move(1, 100, 0, 20).x, Is.LessThan(.001));
            Assert.That(motion.Move(1, -100, 0, 20).x, Is.EqualTo(-20).Within(.001));
        }

        [Test]
        public void InvalidAndOverlappingSpawnsAreRejectedBeforeMutatingState()
        {
            var motion = Convex((x, y, r) => true);
            motion.Add(1, 0, 0, 24);
            Assert.Throws<ArgumentException>(() => motion.Add(2, 20, 0, 24));
            Assert.Throws<ArgumentException>(() => motion.Add(1, 100, 0, 24));
            Assert.Throws<ArgumentOutOfRangeException>(() => motion.Add(2, double.NaN, 0, 24));
            Assert.Throws<ArgumentOutOfRangeException>(() => motion.Move(1, 10, 0, -1));
            Assert.That(motion.Position(1).x, Is.Zero);
            var small = Convex((x, y, r) => true);
            small.Add(1, 0, 0, .0001);
            Assert.Throws<ArgumentException>(() => small.Add(2, 0, 0, .0001));
        }

        [Test]
        public void ClearEndpointsDoNotPermitCrossingBlockedTerrainBetweenThem()
        {
            var motion = new OriginalMotion((x, y, r) => x + r <= .4 || x - r >= .6,
                (a, b, r) => Math.Max(a.x, b.x) + r <= .4 || Math.Min(a.x, b.x) - r >= .6);
            motion.Add(1, 0, 0, .01);
            Assert.That(motion.Move(1, 1, 0, 1).x, Is.EqualTo(.39).Within(.00001));
        }
    }
}

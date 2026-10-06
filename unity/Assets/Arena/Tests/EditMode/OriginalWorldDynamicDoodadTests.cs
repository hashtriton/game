using System;
using System.Collections.Generic;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldDynamicDoodadTests
    {
        sealed class Navigation : IOriginalWorldNavigation, IOriginalWorldDynamicNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision { get; private set; }
            public readonly HashSet<int> ids = new HashSet<int>();
            public bool rejectAdd, rejectRemove;
            public bool IsWalkable(double x, double y, double radius) => ids.Count == 0 || Math.Abs(x) > 128 + radius;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => IsWalkable(a.x, a.y, radius) && IsWalkable(b.x, b.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { a, b };
            public bool SetDoodadAlive(int editorId, bool alive) => false;
            public bool TryAddDynamicDoodad(int editorId, string rawcode, OriginalPoint position, double facingDegrees, double scale)
            {
                if (rejectAdd || !ids.Add(editorId)) return false;
                NavigationRevision++; return true;
            }
            public bool RemoveDynamicDoodad(int editorId)
            {
                if (rejectRemove || !ids.Remove(editorId)) return false;
                NavigationRevision++; return true;
            }
        }
        [Test] public void DynamicDoodadPublishesDetachedInvulnerableStateAndRemovesItsOwnFootprint()
        {
            var nav = new Navigation(); var world = new OriginalWorld(nav);
            int first = world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, true, 0, 1.2);
            int second = world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, true, 0, 1.2);
            Assert.That(first, Is.GreaterThanOrEqualTo(OriginalWorld.FirstDynamicDoodadId));
            Assert.That(second, Is.GreaterThan(first)); Assert.That(nav.ids.Count, Is.EqualTo(2));
            var snapshot = world.Snapshot(); Assert.That(snapshot.doodads[0].dynamic, Is.True);
            Assert.That(snapshot.doodads[0].invulnerable, Is.True); Assert.That(snapshot.doodads[0].scale, Is.EqualTo(1.2));
            snapshot.doodads[0].health = 0;
            Assert.That(world.ApplyDoodadDamage(first, 99999), Is.False);
            Assert.That(world.Snapshot().doodads[0].health, Is.EqualTo(9999));
            Assert.That(world.RemoveDynamicDoodad(first), Is.True); Assert.That(nav.ids.SetEquals(new[] { second }), Is.True);
            Assert.That(world.RemoveDynamicDoodad(first), Is.False);
            Assert.That(world.RemoveDynamicDoodad(second), Is.True); Assert.That(world.Snapshot().doodads, Is.Empty);
        }
        [Test] public void FailedDynamicNavigationCannotPublishOrConsumeAnIdentity()
        {
            var nav = new Navigation { rejectAdd = true }; var world = new OriginalWorld(nav); var before = world.Snapshot();
            Assert.Throws<InvalidOperationException>(() => world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, true, 0, 1.2));
            Assert.That(world.Snapshot().revision, Is.EqualTo(before.revision)); Assert.That(world.Snapshot().doodads, Is.Empty);
            nav.rejectAdd = false;
            int id = world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, true, 0, 1.2);
            Assert.That(id, Is.EqualTo(OriginalWorld.FirstDynamicDoodadId));
            nav.rejectRemove = true; long revision = world.Snapshot().revision;
            Assert.That(world.RemoveDynamicDoodad(id), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(revision)); Assert.That(world.Snapshot().doodads.Length, Is.EqualTo(1));
        }
        [Test] public void DynamicIdentityAndSourceMaskContractsRejectMalformedOrUnsupportedInputs()
        {
            var nav = new Navigation(); var world = new OriginalWorld(nav);
            Assert.Throws<ArgumentOutOfRangeException>(() => world.AddDoodad(OriginalWorld.FirstDynamicDoodadId, "LTbr", new OriginalPoint(0, 0), 10, 10));
            Assert.Throws<ArgumentException>(() => world.AddDynamicDoodad("LTbr", new OriginalPoint(0, 0), 9999, true, 0, 1.2));
            Assert.Throws<ArgumentException>(() => world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, false, 0, 1.2));
            Assert.Throws<ArgumentException>(() => world.AddDynamicDoodad("B009", new OriginalPoint(0, 0), 9999, true, 45, 1.2));
            Assert.That(world.Snapshot().doodads, Is.Empty); Assert.That(nav.ids, Is.Empty);
        }
    }
}

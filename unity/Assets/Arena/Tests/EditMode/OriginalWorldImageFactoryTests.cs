using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldImageFactoryTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint();
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => x < 500;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalWorldUnitProfile Profile() => new OriginalWorldUnitProfile
            { maxHealth = 100, maxMana = 80, collisionRadius = 10, moveSpeed = 200 };
        static OriginalIllusionSpawn Row(int id = 1000000000, double x = 100) => new OriginalIllusionSpawn
            { entityId = id, profile = Profile(), position = new OriginalPoint(x, 0), health = 50, mana = 60 };
        static OriginalWorld World()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(1001, 0, "n017", Profile(), new OriginalPoint(300, 0));
            world.DrainEvents(); return world;
        }
        [Test] public void EnemyWandCopiesHeroAppearanceWithIndependentOwnerAndRetainedLineage()
        {
            var world = World(); var row = Row();
            Assert.That(world.TryPublishImages(1, 0, OriginalImageFactory.EnemyWand, new[] { row }), Is.True);
            row.profile.maxHealth = 999;
            var image = world.UnitState(1000000000);
            Assert.That(image.ownerSlot, Is.Zero); Assert.That(image.rawcode, Is.EqualTo("H008"));
            Assert.That(image.copySourceEntityId, Is.EqualTo(1)); Assert.That(image.sourceHeroEntityId, Is.EqualTo(1));
            Assert.That(image.imageFactory, Is.EqualTo(OriginalImageFactory.EnemyWand));
            Assert.That(image.profile.maxHealth, Is.EqualTo(100));
            Assert.That(world.RemoveUnit(1), Is.True); Assert.That(world.UnitState(image.entityId).health, Is.EqualTo(50));
        }
        [Test] public void BossImageSurvivesSourceRemovalInTransitionAndKeepsItsOwnOrders()
        {
            var world = World();
            Assert.That(world.TryPublishImages(1001, 0, OriginalImageFactory.BossMirror, new[] { Row() }), Is.True);
            var image = world.UnitState(1000000000);
            Assert.That(image.rawcode, Is.EqualTo("n017")); Assert.That(image.sourceHeroEntityId, Is.Zero);
            Assert.That(image.copySourceEntityId, Is.EqualTo(1001));
            Assert.That(world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1001, remove = true } }), Is.True);
            Assert.That(world.TryMove(image.entityId, new OriginalPoint(200, 0)), Is.True);
        }
        [Test] public void WrongFactoryOwnerOrSourceAndLatePlacementLeaveNoPartialImage()
        {
            var world = World(); long revision = world.Snapshot().revision;
            Assert.That(world.TryPublishImages(1, 1, OriginalImageFactory.EnemyWand, new[] { Row() }), Is.False);
            Assert.That(world.TryPublishImages(1, 0, OriginalImageFactory.BossMirror, new[] { Row() }), Is.False);
            Assert.That(world.TryPublishImages(1001, 0, OriginalImageFactory.EnemyWand, new[] { Row() }), Is.False);
            Assert.That(world.TryPublishImages(1, 0, OriginalImageFactory.None, new[] { Row() }), Is.False);
            Assert.That(world.TryPublishImages(1, 0, OriginalImageFactory.EnemyWand, new[] { Row(), Row(1000000001, 600) }), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(revision)); Assert.That(world.DrainEvents(), Is.Empty);
        }
    }
}

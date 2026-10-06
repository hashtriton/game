using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldSummonTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => x < 500;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => IsWalkable(b.x, b.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { a, b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalWorldUnitProfile Profile(double radius = 10) => new OriginalWorldUnitProfile
            { maxHealth = 100, maxMana = 50, moveSpeed = 100, collisionRadius = radius };
        static OriginalWorld World()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            world.DrainEvents(); return world;
        }
        static OriginalWorldSummonSpawn Row(int id = 100000000, double x = 100) => new OriginalWorldSummonSpawn
            { entityId = id, ownerSlot = 1, sourceHeroEntityId = 1, rawcode = "n001", profile = Profile(),
                position = new OriginalPoint(x, 0), health = 80, mana = 30 };

        [Test] public void SummonsPublishWithFinalHeroProfileAndDetachedState()
        {
            var world = World(); var row = Row(); row.invulnerable = true;
            var update = new OriginalWorldProfileUpdate { entityId = 1, profile = Profile(20), health = 70, mana = 10 };
            Assert.That(world.TryPublishSummons(new[] { row, Row(100000001, 130) }, update), Is.True);
            row.profile.maxHealth = 999; update.profile.collisionRadius = 999;
            var summon = world.UnitState(100000000);
            Assert.That(summon.kind, Is.EqualTo(OriginalWorldUnitKind.Summon));
            Assert.That(summon.sourceHeroEntityId, Is.EqualTo(1)); Assert.That(summon.ownerSlot, Is.EqualTo(1));
            Assert.That(summon.health, Is.EqualTo(80)); Assert.That(summon.profile.maxHealth, Is.EqualTo(100));
            Assert.That(summon.invulnerable, Is.True); Assert.That(world.ApplyUnitDamage(summon.entityId, 10), Is.False);
            Assert.That(world.UnitState(1).profile.collisionRadius, Is.EqualTo(20)); Assert.That(world.UnitState(1).health, Is.EqualTo(70));
            Assert.That(world.DrainEvents().Length, Is.EqualTo(2));
        }
        [Test] public void LateTerrainFailureDoesNotDebitHeroOrPublishAnyBodyOrEvent()
        {
            var world = World(); long revision = world.Snapshot().revision;
            var update = new OriginalWorldProfileUpdate { entityId = 1, profile = Profile(), health = 25, mana = 0 };
            Assert.That(world.TryPublishSummons(new[] { Row(), Row(100000001, 600) }, update), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(revision)); Assert.That(world.Snapshot().units.Length, Is.EqualTo(1));
            Assert.That(world.UnitState(1).health, Is.EqualTo(100)); Assert.That(world.DrainEvents(), Is.Empty);
            Assert.That(world.CanPlace(new OriginalPoint(100, 0), 10), Is.True);
        }
        [Test] public void MutualBodiesAndFinalHeroRadiusAreCheckedBeforePublication()
        {
            var world = World();
            Assert.That(world.TryPublishSummons(new[] { Row(), Row(100000001, 105) }), Is.False);
            var update = new OriginalWorldProfileUpdate { entityId = 1, profile = Profile(100), health = 100, mana = 50 };
            Assert.That(world.TryPublishSummons(new[] { Row() }, update), Is.False);
            Assert.That(world.UnitState(1).profile.collisionRadius, Is.EqualTo(10));
            Assert.That(world.TryPublishSummons(new[] { Row(100000000, 110) }, update), Is.True);
        }
        [Test] public void InvalidOwnerIdentityAncestryAndVitalityCannotPublish()
        {
            var world = World();
            Action<OriginalWorldSummonSpawn>[] mutations = { r => r.ownerSlot = 0, r => r.ownerSlot = 2,
                r => r.sourceHeroEntityId = 2, r => r.entityId = 99999999, r => r.entityId = 500000000,
                r => r.health = 0, r => r.health = double.NaN, r => r.mana = 51 };
            foreach (var mutation in mutations) { var row = Row(); mutation(row); Assert.That(world.TryPublishSummons(new[] { row }), Is.False); }
            Assert.That(world.TryPublishSummons(new[] { Row(), Row() }), Is.False);
            Assert.That(world.DrainEvents(), Is.Empty);
            world.ForceUnitDeath(1);
            Assert.That(world.TryPublishSummons(new[] { Row() }), Is.False);
        }
        [Test] public void DonorRemovalKeepsSummonAndImageDetachedLineage()
        {
            var world = World(); Assert.That(world.TryPublishSummons(new[] { Row() }), Is.True);
            Assert.That(world.RemoveUnit(1), Is.True); Assert.That(world.UnitState(100000000).health, Is.EqualTo(80));
            var second = World(); Assert.That(second.TryPublishSummons(new[] { Row() }), Is.True);
            Assert.That(second.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1, remove = true } }), Is.True);
            Assert.That(second.UnitState(100000000), Is.Not.Null);
            var third = World(); third.AddIllusion(1000000000, 1, Profile(), new OriginalPoint(200, 0), 100, 50);
            Assert.That(third.RemoveUnit(1), Is.True);
            Assert.That(third.UnitState(1000000000).copySourceEntityId, Is.EqualTo(1));
        }
        [Test] public void EnemyFactoryCannotOccupyReservedSummonIdentity()
        {
            var world = World();
            Assert.Throws<ArgumentException>(() => world.AddUnit(100000000, 0, "n001", Profile(), new OriginalPoint(100, 0)));
            Assert.That(world.TryPublishSummons(new[] { Row() }), Is.True);
            Assert.That(world.TryPublishSummons(new[] { Row() }), Is.False);
        }
    }
}

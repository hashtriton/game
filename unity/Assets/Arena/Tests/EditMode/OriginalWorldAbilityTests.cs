using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldAbilityTests
    {
        sealed class OpenNavigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint();
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalWorldUnitProfile Profile() => new OriginalWorldUnitProfile
            { collisionRadius = 24, moveSpeed = 250, maxHealth = 631, maxMana = 145 };
        static OriginalWorld Create()
        {
            var world = new OriginalWorld(new OpenNavigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(1001, 0, "n008", Profile(), new OriginalPoint(200, 0));
            world.DrainEvents(); return world;
        }
        [Test] public void IllusionKeepsItsOwnCopiedVitalsAndCanOutliveItsCanonicalHero()
        {
            var world = Create(); var profile = Profile();
            world.AddIllusion(1000000000, 1, profile, new OriginalPoint(64, 0), 315.5, 90);
            profile.maxHealth = 999;
            world.ForceUnitDeath(1);
            Assert.That(world.TryMove(1000000000, new OriginalPoint(96, 0)), Is.True);
            world.Advance(.05);
            var image = world.UnitState(1000000000);
            Assert.That(image.kind, Is.EqualTo(OriginalWorldUnitKind.Illusion));
            Assert.That(image.ownerSlot, Is.EqualTo(1)); Assert.That(image.sourceHeroEntityId, Is.EqualTo(1));
            Assert.That(image.rawcode, Is.EqualTo("H008")); Assert.That(image.profile.maxHealth, Is.EqualTo(631));
            Assert.That(image.health, Is.EqualTo(315.5)); Assert.That(image.mana, Is.EqualTo(90));
            Assert.That(image.position.x, Is.GreaterThan(64));
            Assert.That(world.UnitState(1).kind, Is.EqualTo(OriginalWorldUnitKind.Hero));
            Assert.That(world.UnitState(1001).kind, Is.EqualTo(OriginalWorldUnitKind.Enemy));
            Assert.That(world.UnitState(1).health, Is.Zero);
            Assert.That(world.UnitState(1).sourceHeroEntityId, Is.Zero);
            Assert.That(world.RemoveUnit(1), Is.True);
            Assert.That(world.UnitState(1000000000).copySourceEntityId, Is.EqualTo(1));
        }
        [Test] public void InvalidIllusionAncestryAndPlacementCannotPartiallyPublishAnEntity()
        {
            var world = Create(); long revision = world.Snapshot().revision;
            Assert.Throws<ArgumentException>(() => world.AddIllusion(1000000000, 1001, Profile(), new OriginalPoint(64, 0), 100, 20));
            Assert.Throws<ArgumentException>(() => world.AddIllusion(1000000000, 1, Profile(), new OriginalPoint(20, 0), 100, 20));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.AddIllusion(1000000000, 1, Profile(), new OriginalPoint(64, 0), double.NaN, 20));
            Assert.That(world.Snapshot().revision, Is.EqualTo(revision));
            Assert.That(world.DrainEvents(), Is.Empty); Assert.That(world.UnitState(1000000000), Is.Null);
            world.AddIllusion(1000000000, 1, Profile(), new OriginalPoint(64, 0), 100, 20);
            Assert.Throws<ArgumentException>(() => world.AddIllusion(1000000001, 1000000000, Profile(), new OriginalPoint(128, 0), 100, 20));
        }
        [Test] public void CanonicalAndEnemyIdentitiesCannotCollideWithTheSummonIdentityRange()
        {
            var world = Create(); var before = world.Snapshot();
            Assert.Throws<ArgumentException>(() => world.AddUnit(9, 1, "H008", Profile(), new OriginalPoint(64, 0)));
            Assert.Throws<ArgumentException>(() => world.AddUnit(8, 0, "n008", Profile(), new OriginalPoint(64, 0)));
            Assert.Throws<ArgumentException>(() => world.AddUnit(1000000000, 0, "n008", Profile(), new OriginalPoint(64, 0)));
            Assert.That(world.Snapshot().revision, Is.EqualTo(before.revision));
            Assert.That(world.Snapshot().units.Length, Is.EqualTo(2)); Assert.That(world.DrainEvents(), Is.Empty);
        }
        [Test] public void ManaDebitRejectsInvalidInsufficientDeadAndPausedActorsAtomically()
        {
            var world = Create();
            Assert.Throws<ArgumentOutOfRangeException>(() => world.TrySpendMana(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.TrySpendMana(1, double.PositiveInfinity));
            Assert.That(world.TrySpendMana(1, 146), Is.False);
            Assert.That(world.UnitState(1).mana, Is.EqualTo(145));
            world.SetUnitState(1, paused: true); Assert.That(world.TrySpendMana(1, 60), Is.False);
            world.SetUnitState(1, paused: false); Assert.That(world.TrySpendMana(1, 60), Is.True);
            Assert.That(world.UnitState(1).mana, Is.EqualTo(85));
            world.ForceUnitDeath(1); Assert.That(world.TrySpendMana(1, 0), Is.False);
            Assert.That(world.UnitState(1).mana, Is.EqualTo(85));
        }
        [Test] public void ExpiryIsRemovalRatherThanDeathAndFacingAndCastCountersAreDetached()
        {
            var world = Create(); world.AddIllusion(1000000000, 1, Profile(), new OriginalPoint(64, 0), 100, 20);
            Assert.That(world.UnitState(1).hasFacing, Is.False); Assert.That(world.UnitState(1).facingDegrees, Is.Zero);
            Assert.That(world.SetFacing(1, 450), Is.True); Assert.That(world.MarkCast(1), Is.True);
            var state = world.UnitState(1); state.facingDegrees = double.NaN; state.castSequence = 999;
            Assert.That(world.UnitState(1).facingDegrees, Is.EqualTo(90)); Assert.That(world.UnitState(1).hasFacing, Is.True);
            Assert.That(world.UnitState(1).castSequence, Is.EqualTo(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.SetFacing(1, double.NaN));
            world.DrainEvents();
            Assert.That(world.RemoveUnit(1000000000, OriginalWorldRemovalReason.Expired), Is.True);
            Assert.That(world.RemoveUnit(1000000000, OriginalWorldRemovalReason.Expired), Is.False);
            var events = world.DrainEvents(); Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events[0].kind, Is.EqualTo(OriginalWorldEventKind.UnitRemoved));
            Assert.That(events[0].removalReason, Is.EqualTo(OriginalWorldRemovalReason.Expired));
            Assert.That(world.UnitState(1).health, Is.EqualTo(631));
        }
    }
}

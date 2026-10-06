using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldProfileTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => r <= 80;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        static OriginalWorldUnitProfile Profile(double radius = 24) => new OriginalWorldUnitProfile
            { collisionRadius = radius, moveSpeed = 250, maxHealth = 100, maxMana = 50 };
        static OriginalWorld Create()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            world.AddUnit(1001, 0, "n008", Profile(), new OriginalPoint(64, 0));
            world.DrainEvents(); return world;
        }
        [Test] public void ProfileUpdateUsesExplicitCurrentValuesAndCopiesTheSuppliedProfile()
        {
            var world = Create(); var profile = Profile(); profile.maxHealth = 160; profile.maxMana = 90; profile.moveSpeed = 300;
            Assert.That(world.UpdateProfile(1, profile, 60, 17), Is.True);
            profile.maxHealth = 900;
            var unit = world.UnitState(1);
            Assert.That(unit.profile.maxHealth, Is.EqualTo(160)); Assert.That(unit.health, Is.EqualTo(60));
            Assert.That(unit.mana, Is.EqualTo(17)); Assert.That(unit.profile.moveSpeed, Is.EqualTo(300));
        }
        [Test] public void ProfileUpdateIsAtomicWhenALargerBodyCannotFit()
        {
            var world = Create(); var before = world.Snapshot(); var profile = Profile(48); profile.maxHealth = 500;
            Assert.That(world.UpdateProfile(1, profile, 500, 30), Is.False);
            var after = world.Snapshot(); Assert.That(after.revision, Is.EqualTo(before.revision));
            Assert.That(after.units[0].profile.maxHealth, Is.EqualTo(100)); Assert.That(after.units[0].health, Is.EqualTo(100));
            Assert.That(after.units[0].profile.collisionRadius, Is.EqualTo(24));
        }
        [Test] public void ProfileUpdateCannotKillResurrectOrPublishNonFiniteState()
        {
            var world = Create(); Assert.That(world.UpdateProfile(1, Profile(), 0, 30), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => world.UpdateProfile(1, Profile(), double.NaN, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.UpdateProfile(1, Profile(), 101, 0));
            world.ForceUnitDeath(1);
            Assert.That(world.UpdateProfile(1, Profile(), 100, 30), Is.False);
            var profile = Profile(); profile.maxHealth = 140;
            Assert.That(world.UpdateProfile(1, profile, 0, 30), Is.True);
            Assert.That(world.UnitState(1).health, Is.Zero);
        }
        [Test] public void ForceDeathBypassesInvulnerabilityOnceAndClearsBodiesAndTargetOrders()
        {
            var world = Create(); world.SetUnitState(1001, invulnerable: true);
            world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001);
            Assert.That(world.ApplyUnitDamage(1001, 100), Is.False);
            Assert.That(world.ForceUnitDeath(1001), Is.True); Assert.That(world.ForceUnitDeath(1001), Is.False);
            var events = world.DrainEvents();
            Assert.That(events.Count(e => e.kind == OriginalWorldEventKind.UnitDied), Is.EqualTo(1));
            Assert.That(events.Count(e => e.kind == OriginalWorldEventKind.UnitDamaged), Is.Zero);
            Assert.That(world.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            world.AddUnit(1002, 0, "n008", Profile(), new OriginalPoint(64, 0));
            Assert.That(world.UnitState(1002).health, Is.EqualTo(100));
        }
    }
}

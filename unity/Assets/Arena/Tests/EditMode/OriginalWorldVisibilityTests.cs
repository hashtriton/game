using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldVisibilityTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => x >= -1000 && x <= 1000;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => IsWalkable(b.x, b.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { a, b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalWorld World()
        {
            var world = new OriginalWorld(new Navigation());
            var profile = new OriginalWorldUnitProfile { collisionRadius = 24, moveSpeed = 100, maxHealth = 200, maxMana = 100 };
            world.AddUnit(1, 1, "H008", profile, new OriginalPoint(0, 0));
            world.AddUnit(1001, 0, "n008", profile, new OriginalPoint(100, 0));
            return world;
        }

        [Test]
        public void HiddenHeroRetainsStateButCannotMoveCastOrBeTargeted()
        {
            var world = World();
            world.ApplyUnitDamage(1, 70); world.TrySpendMana(1, 20);
            world.TryMove(1, new OriginalPoint(50, 0));
            Assert.That(world.SetVisibility(1, false), Is.True);
            Assert.That(world.TryMove(1, new OriginalPoint(200, 0)), Is.False);
            Assert.That(world.TrySpendMana(1, 10), Is.False);
            Assert.That(world.TryAttackTarget(1001, OriginalWorldTargetKind.Unit, 1), Is.False);
            world.Advance(.05);
            var unit = world.UnitState(1);
            Assert.That(unit.hidden, Is.True);
            Assert.That(unit.health, Is.EqualTo(130)); Assert.That(unit.mana, Is.EqualTo(80));
            Assert.That(unit.position.x, Is.Zero); Assert.That(unit.order, Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(world.SetVisibility(1, true), Is.True);
            world.Advance(.05);
            Assert.That(world.UnitState(1).position.x, Is.EqualTo(5).Within(.00001));
        }

        [Test]
        public void ShowingOccupiedHeroFailsAtomicallyAndHiddenProfileDoesNotCreateBody()
        {
            var world = World(); world.SetVisibility(1, false);
            Assert.That(world.Relocate(1001, new OriginalPoint(0, 0)), Is.True);
            Assert.That(world.UpdateProfile(1, new OriginalWorldUnitProfile { collisionRadius = 32, moveSpeed = 90, maxHealth = 300, maxMana = 100 }, 150, 50), Is.True);
            Assert.That(world.SetVisibility(1, true), Is.False);
            Assert.That(world.UnitState(1).hidden, Is.True);
            Assert.That(world.UnitState(1).health, Is.EqualTo(150));
            Assert.That(world.Relocate(1, new OriginalPoint(200, 0)), Is.True);
            Assert.That(world.SetVisibility(1, true), Is.True);
            Assert.That(world.Relocate(1001, new OriginalPoint(200, 0)), Is.False);
        }

        [Test]
        public void ReturningDeadHeroMovesCorpseWithoutRevivingOrReservingBody()
        {
            var world = World(); world.ForceUnitDeath(1);
            Assert.That(world.RelocateStoredPosition(1, new OriginalPoint(100, 0)), Is.True);
            Assert.That(world.UnitState(1).health, Is.Zero);
            Assert.That(world.UnitState(1).position.x, Is.EqualTo(100));
            Assert.That(world.SetVisibility(1, false), Is.True);
            Assert.That(world.SetVisibility(1, true), Is.True);
            Assert.That(world.RestoreUnit(1, new OriginalPoint(100, 0)), Is.False);
            Assert.That(world.RestoreUnit(1, new OriginalPoint(200, 0)), Is.True);
            Assert.That(world.UnitState(1).health, Is.EqualTo(200));
            Assert.That(world.RelocateStoredPosition(1, new OriginalPoint(100, 0)), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => world.RelocateStoredPosition(1, new OriginalPoint(double.NaN, 0)));
        }
    }
}

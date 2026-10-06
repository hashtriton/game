using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalWorldTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision { get; private set; }
            public bool barrelAlive;
            public int pathCalls, changes;
            public bool corridor;
            public bool IsWalkable(double x, double y, double r) => (!corridor || x - r >= 0 && x + r <= 64) &&
                (!barrelAlive || x + r <= 64 || x - r >= 128);
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(a.x, a.y, r) && IsWalkable(b.x, b.y, r) &&
                (!barrelAlive || Math.Max(a.x, b.x) + r <= 64 || Math.Min(a.x, b.x) - r >= 128);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r)
            { pathCalls++; return SegmentClear(a, b, r) ? new[] { a, b } : Array.Empty<OriginalPoint>(); }
            public bool SetDoodadAlive(int id, bool alive)
            {
                if (id != 42 || alive == barrelAlive) return false;
                barrelAlive = alive; changes++; NavigationRevision++; return true;
            }
        }
        static OriginalWorldUnitProfile Profile(double speed = 100, double radius = 24) => new OriginalWorldUnitProfile
            { moveSpeed = speed, collisionRadius = radius, maxHealth = 631, maxMana = 145 };
        static OriginalWorldUnitView Unit(OriginalWorld world, int id = 1) => world.Snapshot().units.Single(u => u.entityId == id);
        static void Tick(OriginalWorld world, int count) { for (int i = 0; i < count; i++) world.Advance(.05); }

        [Test]
        public void DuelBatchReservesSharedReturnPositionsAndCannotPartiallyRevive()
        {
            var nav = new Navigation(); var world = new OriginalWorld(nav);
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            world.AddUnit(2, 2, "H008", Profile(), new OriginalPoint(100, 0));
            world.ForceUnitDeath(1); world.SetVisibility(2, false);
            var before = world.Snapshot();
            var rows = new[] {
                new OriginalWorldTransition { entityId = 1, relocate = true, position = new OriginalPoint(200, 0), revive = true, fullMana = true },
                new OriginalWorldTransition { entityId = 2, relocate = true, position = new OriginalPoint(200, 0), visible = true } };
            Assert.That(world.TryApplyTransitions(rows, 0), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(before.revision));
            Assert.That(world.UnitState(1).health, Is.Zero);
            Assert.That(world.UnitState(2).hidden, Is.True);
            Assert.That(world.TryApplyTransitions(rows, 128), Is.True);
            var a = world.UnitState(1); var b = world.UnitState(2);
            Assert.That(a.health, Is.EqualTo(a.profile.maxHealth));
            Assert.That(b.hidden, Is.False);
            Assert.That(Math.Sqrt(Math.Pow(a.position.x - b.position.x, 2) + Math.Pow(a.position.y - b.position.y, 2)), Is.GreaterThanOrEqualTo(48));
            world.ForceUnitDeath(1);
            Assert.That(world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1, fullHealth = true, relocate = true, position = new OriginalPoint(300, 0) } }), Is.True);
            Assert.That(world.UnitState(1).health, Is.Zero, "Set life percent does not replace ReviveHero.");
        }

        [Test]
        public void IllusionBatchRejectsLateCollisionWithoutPublishingAnEarlierActor()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            world.DrainEvents(); var before = world.Snapshot();
            var rows = new[] {
                new OriginalIllusionSpawn { entityId = 1000000000, profile = Profile(), position = new OriginalPoint(100, 0), health = 500, mana = 60 },
                new OriginalIllusionSpawn { entityId = 1000000001, profile = Profile(), position = new OriginalPoint(101, 0), health = 500, mana = 60 } };
            Assert.That(world.TryPublishIllusions(1, rows), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(before.revision));
            Assert.That(world.Snapshot().units.Length, Is.EqualTo(1));
            Assert.That(world.DrainEvents(), Is.Empty);
            rows[1].position = new OriginalPoint(-100, 0);
            Assert.That(world.TryPublishIllusions(1, rows), Is.True);
            rows[0].profile.maxHealth = 9999;
            Assert.That(world.UnitState(1000000000).profile.maxHealth, Is.EqualTo(631));
            Assert.That(world.DrainEvents().Length, Is.EqualTo(2));
        }

        [Test]
        public void SourceDisplacementPreservesOrdersAndRestoresOverlappingCollisionBody()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            world.AddUnit(2, 2, "H008", Profile(), new OriginalPoint(100, 0));
            world.TryMove(1, new OriginalPoint(500, 0));
            Assert.That(world.SetPathingEnabled(1, false), Is.True);
            Assert.That(world.ForcePosition(1, new OriginalPoint(100, 0)), Is.True);
            Assert.That(world.UnitState(1).hidden || world.UnitState(1).paused, Is.False);
            Assert.That(world.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(world.SetPathingEnabled(1, true), Is.True);
            Tick(world, 20);
            Assert.That(world.UnitState(1).position.x, Is.GreaterThan(100));
            // Source can force again after pathing was restored on an earlier tick.
            Assert.That(world.ForcePosition(1, new OriginalPoint(100, 0)), Is.True);
            Assert.That(world.UnitState(1).position.x, Is.EqualTo(100));
            world.ForceUnitDeath(1);
            Assert.That(world.ForcePosition(1, new OriginalPoint(200, 0)), Is.True);
            Assert.That(world.UnitState(1).health, Is.Zero);
            Assert.That(world.CanPlace(new OriginalPoint(200, 0), 24), Is.True);
        }

        [Test]
        public void HiddenIllusionBatchDoesNotOccupyBodiesBeforeVisibility()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            Assert.That(world.TryPublishIllusions(1, new[] {
                new OriginalIllusionSpawn { entityId = 1000000000, profile = Profile(), position = new OriginalPoint(0, 0), health = 500, mana = 60, hidden = true }
            }), Is.True);
            Assert.That(world.SetVisibility(1000000000, true), Is.False);
            Assert.That(world.UnitState(1000000000).hidden, Is.True);
            Assert.That(world.Relocate(1000000000, new OriginalPoint(100, 0)), Is.True);
            Assert.That(world.SetVisibility(1000000000, true), Is.True);
        }

        [Test]
        public void HostSpeedAndBoundedTimeDetermineMotionNotRequestedDistance()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            Assert.That(world.TryMove(1, new OriginalPoint(10000, 0)), Is.True);
            world.Advance(.05);
            Assert.That(Unit(world).position.x, Is.EqualTo(5).Within(.000001));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.Advance(.051));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.Advance(double.NaN));
            Assert.That(Unit(world).position.x, Is.EqualTo(5).Within(.000001));
            Assert.That(world.Stop(1), Is.True); Tick(world, 20);
            Assert.That(Unit(world).position.x, Is.EqualTo(5).Within(.000001));
        }

        [Test]
        public void SortedIdentityMovementIsIndependentOfInsertionOrder()
        {
            OriginalWorld Make(bool reverse)
            {
                var world = new OriginalWorld(new Navigation());
                foreach (int id in reverse ? new[] { 2, 1 } : new[] { 1, 2 })
                    world.AddUnit(id, id, "H008", Profile(), new OriginalPoint(id == 1 ? -60 : 60, 0));
                world.TryMove(1, new OriginalPoint(100, 0)); world.TryMove(2, new OriginalPoint(-100, 0));
                return world;
            }
            var first = Make(false); var second = Make(true); Tick(first, 100); Tick(second, 100);
            Assert.That(Unit(first).position.x, Is.EqualTo(Unit(second).position.x));
            Assert.That(Unit(first, 2).position.x, Is.EqualTo(Unit(second, 2).position.x));
            Assert.That(Unit(first, 2).position.x - Unit(first).position.x, Is.GreaterThanOrEqualTo(47.99999));
        }

        [Test]
        public void LiveBodiesQueueInNarrowPassageAndRemovedBodyReleasesFollowingUnit()
        {
            var world = new OriginalWorld(new Navigation { corridor = true });
            for (int i = 1; i <= 4; i++)
            {
                world.AddUnit(i, i, "H008", Profile(180), new OriginalPoint(32, (i - 1) * 64));
                if (i > 1) world.TryMove(i, new OriginalPoint(32, 0));
            }
            Tick(world, 150);
            Assert.That(Unit(world, 3).position.y, Is.EqualTo(96).Within(.001));
            Assert.That(world.RemoveUnit(2), Is.True); Tick(world, 20);
            Assert.That(Unit(world, 3).position.y, Is.EqualTo(48).Within(.001));
        }

        [Test]
        public void OrdinaryBarrelDeathChangesPathingOnceAndResumesPendingMove()
        {
            var nav = new Navigation { barrelAlive = true };
            var world = new OriginalWorld(nav);
            world.AddDoodad(42, "LTbr", new OriginalPoint(96, 0), 20, 20);
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            Assert.That(world.TryMove(1, new OriginalPoint(160, 0)), Is.True);
            Tick(world, 20); Assert.That(Unit(world).position.x, Is.Zero);
            Assert.That(world.ApplyDoodadDamage(42, 10), Is.True);
            Assert.That(nav.barrelAlive, Is.True); Assert.That(nav.changes, Is.Zero);
            Assert.That(world.ApplyDoodadDamage(42, 20), Is.True);
            Assert.That(world.ApplyDoodadDamage(42, 20), Is.False);
            Assert.That(nav.changes, Is.EqualTo(1)); Tick(world, 40);
            Assert.That(Unit(world).position.x, Is.EqualTo(160).Within(.001));
            Assert.That(nav.pathCalls, Is.EqualTo(2));
            var events = world.DrainEvents();
            Assert.That(events.Count(e => e.kind == OriginalWorldEventKind.DoodadDestroyed), Is.EqualTo(1));
            Assert.That(events.Select(e => e.sequence), Is.Ordered.Ascending);
            Assert.That(world.DrainEvents(), Is.Empty);
        }

        [Test]
        public void FreeSpawnChecksTerrainAndAllBodiesWithoutDisablingCollision()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint(0, 0));
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(0, 0), 24, 16, out _), Is.False);
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(0, 0), 24, 96, out var point), Is.True);
            Assert.That(Math.Sqrt(point.x * point.x + point.y * point.y), Is.InRange(48, 96));
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(0, 0), 24, 96, out var repeated), Is.True);
            Assert.That(repeated.x, Is.EqualTo(point.x)); Assert.That(repeated.y, Is.EqualTo(point.y));
            world.AddUnit(2, 2, "H024", Profile(), point);
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(200, 0), 24, 0, out var exact), Is.True);
            Assert.That(exact.x, Is.EqualTo(200));
        }

        [Test]
        public void SourceProfileAndEveryNestedSnapshotAreDetachedFromAuthority()
        {
            var profile = Profile(); var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", profile, new OriginalPoint());
            world.AddDoodad(5, "LTbr", new OriginalPoint(500, 0), 20, 12);
            profile.moveSpeed = 999;
            var view = world.Snapshot(); view.units[0].profile.moveSpeed = 999;
            view.units[0].health = 0; view.units[0].position.x = 999;
            view.doodads[0].health = 0;
            Assert.That(Unit(world).profile.moveSpeed, Is.EqualTo(100));
            Assert.That(Unit(world).position.x, Is.Zero); Assert.That(Unit(world).health, Is.EqualTo(631));
            Assert.That(world.Snapshot().doodads[0].health, Is.EqualTo(12));
        }

        [Test]
        public void AttackTargetIsOnlyAnIntentUntilTrustedCombatResolvesIt()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(1001, 0, "n008", Profile(), new OriginalPoint(200, 0));
            Assert.That(world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001), Is.True); Tick(world, 20);
            Assert.That(Unit(world).order, Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(Unit(world).targetId, Is.EqualTo(1001)); Assert.That(Unit(world).position.x, Is.Zero);
            Assert.That(Unit(world, 1001).health, Is.EqualTo(631));
            Assert.That(world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1), Is.False);
            Assert.That(world.TryAttackTarget(1, OriginalWorldTargetKind.Doodad, 999), Is.False);
        }

        [Test]
        public void InvalidSourceValuesAndOverlappingSpawnsDoNotPartiallyMutateRegistry()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            Assert.Throws<ArgumentException>(() => world.AddUnit(1, 2, "H024", Profile(), new OriginalPoint(200, 0)));
            Assert.Throws<ArgumentException>(() => world.AddUnit(2, 2, "H024", Profile(), new OriginalPoint(20, 0)));
            var invalid = Profile(); invalid.maxHealth = double.NaN;
            Assert.Throws<ArgumentOutOfRangeException>(() => world.AddUnit(2, 2, "H024", invalid, new OriginalPoint(200, 0)));
            Assert.That(world.Snapshot().units.Length, Is.EqualTo(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalWorld.HeroEntityId(9));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalWorld.EnemyEntityId(0));
            Assert.Throws<OverflowException>(() => OriginalWorld.EnemyEntityId(int.MaxValue));
            Assert.That(OriginalWorld.EnemyEntityId(42), Is.EqualTo(1042));
        }

        [Test]
        public void InvalidOrDuplicateDoodadDamageCannotMintHealthOrRepeatedDeaths()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddDoodad(42, "LTex", new OriginalPoint(96, 0), 30, 30);
            Assert.Throws<ArgumentOutOfRangeException>(() => world.ApplyDoodadDamage(42, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.ApplyDoodadDamage(42, double.NaN));
            Assert.That(world.ApplyDoodadDamage(42, 0), Is.False);
            Assert.That(world.Snapshot().doodads[0].health, Is.EqualTo(30));
            world.ApplyDoodadDamage(42, 30);
            Assert.That(world.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.DoodadDestroyed), Is.EqualTo(1));
            // No explosion damage/effects are invented by the registry layer.
            Assert.That(world.Snapshot().units, Is.Empty);
        }

        [Test]
        public void ResolvedUnitDeathRemovesCollisionOnceButPreservesDeadSnapshot()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(2, 2, "H024", Profile(), new OriginalPoint(96, 0));
            world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 2); world.DrainEvents();
            Assert.That(world.ApplyUnitDamage(2, 700), Is.True);
            Assert.That(world.ApplyUnitDamage(2, 700), Is.False);
            Assert.That(Unit(world, 2).health, Is.Zero);
            Assert.That(Unit(world).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(world.TryMove(2, new OriginalPoint(200, 0)), Is.False);
            Assert.That(world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 2), Is.False);
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(96, 0), 24, 0, out _), Is.True);
            world.TryMove(1, new OriginalPoint(200, 0)); Tick(world, 50);
            Assert.That(Unit(world).position.x, Is.EqualTo(200).Within(.001));
            Assert.That(world.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.UnitDied), Is.EqualTo(1));
        }

        [Test]
        public void RestoreAndRelocationValidateDestinationBeforeChangingBodyOrHealth()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(2, 2, "H024", Profile(), new OriginalPoint(96, 0));
            world.ApplyUnitDamage(1, 100);
            Assert.That(world.Relocate(1, new OriginalPoint(96, 0), fullHealth: true), Is.False);
            Assert.That(Unit(world).health, Is.EqualTo(531)); Assert.That(Unit(world).position.x, Is.Zero);
            Assert.That(world.Relocate(1, new OriginalPoint(0, 0)), Is.True, "Own body must not obstruct relocation.");
            world.ApplyUnitDamage(1, 1000);
            Assert.That(world.RestoreUnit(1, new OriginalPoint(96, 0)), Is.False);
            Assert.That(Unit(world).health, Is.Zero);
            Assert.That(world.RestoreUnit(1, new OriginalPoint(0, 100)), Is.True);
            Assert.That(Unit(world).health, Is.EqualTo(631)); Assert.That(Unit(world).position.y, Is.EqualTo(100));
            Assert.That(world.TryFindFreeSpawn(new OriginalPoint(0, 100), 24, 0, out _), Is.False);
        }

        [Test]
        public void MarkAttackOnlyChangesAnimationSequenceAndDamageRejectsInvalidValues()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            Assert.That(world.MarkAttack(1), Is.True); Assert.That(world.MarkAttack(1), Is.True);
            Assert.That(Unit(world).attackSequence, Is.EqualTo(2));
            Assert.That(Unit(world).health, Is.EqualTo(631));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.ApplyUnitDamage(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => world.ApplyUnitDamage(1, double.NaN));
            Assert.That(Unit(world).health, Is.EqualTo(631));
            world.ApplyUnitDamage(1, 1000); Assert.That(world.MarkAttack(1), Is.False);
            Assert.That(Unit(world).attackSequence, Is.EqualTo(2));
        }

        [Test]
        public void TrustedCombatSuppliesApproachPointAndArrivalKeepsAttackIntent()
        {
            var world = new OriginalWorld(new Navigation());
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(1001, 0, "n008", Profile(), new OriginalPoint(200, 0));
            Assert.That(world.TryApproachTarget(1, new OriginalPoint(100, 0)), Is.False);
            world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001);
            Assert.That(world.TryApproachTarget(1, new OriginalPoint(100, 0)), Is.True);
            Assert.That(Unit(world).approaching, Is.True); Tick(world, 25);
            Assert.That(Unit(world).position.x, Is.EqualTo(100).Within(.001));
            Assert.That(Unit(world).approaching, Is.False);
            Assert.That(Unit(world).order, Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(Unit(world).targetId, Is.EqualTo(1001));
            Assert.That(Unit(world, 1001).health, Is.EqualTo(631));
        }

        [Test]
        public void StopApproachPreservesTargetAndDoesNotResumeOnNavigationRevision()
        {
            var nav = new Navigation(); var world = new OriginalWorld(nav);
            world.AddUnit(1, 1, "H008", Profile(), new OriginalPoint());
            world.AddUnit(1001, 0, "n008", Profile(), new OriginalPoint(200, 0));
            world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001);
            world.TryApproachTarget(1, new OriginalPoint(100, 0)); Tick(world, 5);
            Assert.That(world.StopApproach(1), Is.True);
            double x = Unit(world).position.x; nav.SetDoodadAlive(42, true); nav.SetDoodadAlive(42, false); Tick(world, 20);
            Assert.That(Unit(world).position.x, Is.EqualTo(x));
            Assert.That(Unit(world).targetId, Is.EqualTo(1001));
            Assert.That(Unit(world).order, Is.EqualTo(OriginalWorldOrder.AttackTarget));
        }
    }
}

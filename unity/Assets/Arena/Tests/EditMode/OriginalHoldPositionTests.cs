using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalHoldPositionTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 0);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => Math.Abs(x) < 4096 && Math.Abs(y) < 4096;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        sealed class Fixture
        {
            internal OriginalSession session;
            internal OriginalWorld world;
            internal long sequence = 3;
            internal OriginalWorldUnitView Hero => world.UnitState(1);
            internal OriginalSessionReplyCode Command(OriginalSessionCommandKind kind, double x = 0, int target = 0) => session.Apply(0,
                new OriginalSessionCommand { sequence = ++sequence, kind = kind, x = x, y = 0,
                    targetKind = target == 0 ? OriginalWorldTargetKind.None : OriginalWorldTargetKind.Unit, targetId = target });
            internal void Advance(double seconds)
            {
                while (seconds > 1e-9) { double step = Math.Min(.05, seconds); session.Advance(step); session.DrainEvents(); seconds -= step; }
                Assert.That(session.HaltReason, Is.Null);
            }
            internal void AddTarget(int id, double x)
            {
                world.AddUnit(id, 0, "n008", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 1000, moveSpeed = 300 }, new OriginalPoint(x, 0));
                world.SetUnitState(id, paused: true);
            }
        }
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static Fixture Create()
        {
            var f = new Fixture();
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            f.session.ConfigureWorld(new Navigation(), Load<OriginalNativeCatalog>("lia39-native126.json"), Array.Empty<OriginalWorldDoodadView>(), Load<OriginalObservedCatalog>("lia39-observed126.json"));
            f.session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            f.session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            f.session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start });
            f.session.DrainEvents();
            f.world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
            return f;
        }
        [Test] public void HoldingDoesNotAcquireOrPursueTargetsBeyondWeaponRange()
        {
            var f = Create(); f.AddTarget(1001, 300);
            Assert.That(f.Command(OriginalSessionCommandKind.HoldPosition), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(2);
            Assert.That(f.Hero.holding, Is.True); Assert.That(f.Hero.position.x, Is.Zero); Assert.That(f.Hero.position.y, Is.Zero);
            Assert.That(f.Hero.attackSequence, Is.Zero); Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void HoldingAttacksInRangeAndDropsARetreatingTargetWithoutFollowing()
        {
            var f = Create(); f.AddTarget(1001, 100); f.Command(OriginalSessionCommandKind.HoldPosition); f.Advance(1);
            Assert.That(f.world.UnitState(1001).health, Is.LessThan(1000)); Assert.That(f.Hero.holding, Is.True);
            Assert.That(f.world.Relocate(1001, new OriginalPoint(400, 0)), Is.True); f.Advance(1);
            Assert.That(f.Hero.position.x, Is.Zero); Assert.That(f.Hero.position.y, Is.Zero);
            Assert.That(f.Hero.holding, Is.True); Assert.That(f.Hero.approaching, Is.False);
            Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void HoldAcquisitionUsesTheSameCollisionExtendedRangeAsWeaponRelease()
        {
            var f = Create(); f.AddTarget(1001, 167.9);
            f.Command(OriginalSessionCommandKind.HoldPosition); f.Advance(1);
            Assert.That(f.world.UnitState(1001).health, Is.LessThan(1000));
            Assert.That(f.Hero.position.x, Is.Zero); Assert.That(f.Hero.approaching, Is.False);
            Assert.That(f.world.Relocate(1001, new OriginalPoint(168.1, 0)), Is.True);
            f.Advance(1);
            Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(f.Hero.holding, Is.True); Assert.That(f.Hero.position.x, Is.Zero);
        }
        [Test] public void ADeadTargetDoesNotReleaseHoldAndAnotherInRangeTargetCanBeAcquired()
        {
            var f = Create(); f.AddTarget(1001, 100); f.Command(OriginalSessionCommandKind.HoldPosition); f.Advance(.3);
            Assert.That(f.Hero.targetId, Is.EqualTo(1001)); f.world.ForceUnitDeath(1001);
            Assert.That(f.Hero.holding, Is.True); Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
            f.AddTarget(1002, 100); f.Advance(2);
            Assert.That(f.Hero.targetId, Is.EqualTo(1002)); Assert.That(f.Hero.holding, Is.True);
            Assert.That(f.world.UnitState(1002).health, Is.LessThan(1000)); Assert.That(f.Hero.position.x, Is.Zero);
        }
        [Test] public void ExplicitMoveStopAndAttackReleaseHoldButRejectedCommandsDoNot()
        {
            var f = Create(); f.AddTarget(1001, 300); f.Command(OriginalSessionCommandKind.HoldPosition);
            Assert.That(f.Command(OriginalSessionCommandKind.Move, double.NaN), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(f.Hero.holding, Is.True);
            Assert.That(f.world.TryApproachTarget(1, new OriginalPoint(100, 0)), Is.False);
            f.Command(OriginalSessionCommandKind.Stop); Assert.That(f.Hero.holding, Is.False);
            f.Command(OriginalSessionCommandKind.HoldPosition); f.Command(OriginalSessionCommandKind.Move, -400);
            Assert.That(f.Hero.holding, Is.False); f.Advance(.2); Assert.That(f.Hero.position.x, Is.LessThan(0));
            f.Command(OriginalSessionCommandKind.HoldPosition); f.Command(OriginalSessionCommandKind.AttackTarget, target: 1001);
            Assert.That(f.Hero.holding, Is.False); f.Advance(.4); Assert.That(f.Hero.approaching, Is.True);
        }
        [Test] public void AnInvulnerableTargetDoesNotBlockHeldReacquisitionAndOwnDeathClearsHold()
        {
            var f = Create(); f.AddTarget(1001, 100); f.Command(OriginalSessionCommandKind.HoldPosition); f.Advance(.3);
            Assert.That(f.Hero.targetId, Is.EqualTo(1001));
            f.world.SetUnitState(1001, invulnerable: true); f.AddTarget(1002, -100); f.Advance(2);
            Assert.That(f.Hero.targetId, Is.EqualTo(1002)); Assert.That(f.Hero.holding, Is.True);
            Assert.That(f.Hero.position.x, Is.Zero); Assert.That(f.world.UnitState(1002).health, Is.LessThan(1000));
            f.world.ForceUnitDeath(1);
            Assert.That(f.Hero.holding, Is.False); Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void HoldCommandAndSnapshotRoundTripButHeldMovementIsRejectedOnTheWire()
        {
            var f = Create(); var codec = new OriginalUnitySessionCodec();
            var command = new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.HoldPosition };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out var decoded), Is.True);
            Assert.That(f.session.Apply(0, decoded), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            OriginalNetworkResponse Response() => new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot,
                assignedSlot = 1, acknowledgedSequence = 4, snapshot = f.session.Snapshot() };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Response()), out var read), Is.True);
            Assert.That(read.snapshot.world.units.Single(u => u.ownerSlot == 1).holding, Is.True);
            var invalid = Response(); invalid.snapshot.world.units[0].order = OriginalWorldOrder.Move;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(invalid), out _), Is.False);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionShieldEffectTests
    {
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { null, false };
            var session = (OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            Assert.That(world.Relocate(1, new OriginalPoint(0, 1000)), Is.True);
            world.SetFacing(1, 0);
            return session;
        }
        static object Call(OriginalSession session, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(session, args);
        static void Step(OriginalSession session, double seconds)
        {
            while (seconds > 1e-9) { double d = Math.Min(.01, seconds); session.Advance(d); session.DrainEvents(); seconds -= d; }
            Assert.That(session.HaltReason, Is.Null);
        }
        static void Add(OriginalWorld world, int id, double x, double hp = 1000)
        {
            world.AddUnit(id, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = hp, moveSpeed = 270, collisionRadius = 31 }, new OriginalPoint(x, 1000));
            world.SetUnitState(id, paused: true);
        }

        [Test] public void SourceEffectHitsOnlyTheForwardConeAndMovesThirtyThreeTicksThenRestores()
        {
            var session = Create(out var world); Add(world, 1001, 100); Add(world, 1002, -100);
            Call(session, "BeginShieldEffect", 1, 1, false);
            Assert.That(world.UnitState(1001).health, Is.EqualTo(1000 - 110 / 1.12).Within(1e-8));
            Assert.That(world.UnitState(1002).health, Is.EqualTo(1000));
            Assert.That(world.UnitState(1001).pathingDisabled, Is.True);
            Assert.That(world.UnitState(1001).profile.moveSpeed, Is.Zero);
            Step(session, .99);
            Assert.That(world.UnitState(1001).position.x, Is.EqualTo(248.5).Within(1e-8));
            Assert.That(world.UnitState(1001).pathingDisabled, Is.True);
            Step(session, .03);
            Assert.That(world.UnitState(1001).pathingDisabled, Is.False);
            Assert.That(world.UnitState(1001).profile.moveSpeed, Is.EqualTo(270));
        }

        [Test] public void ForcedVictimKeepsAttackIntentAndIsNotTreatedAsStunnedOrRingExcluded()
        {
            var session = Create(out var world); Add(world, 1001, 100);
            world.SetUnitState(1001, paused: false); world.TryAttackTarget(1001, OriginalWorldTargetKind.Unit, 1);
            Call(session, "BeginShieldEffect", 1, 1, false);
            Assert.That(world.UnitState(1001).order, Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(world.UnitState(1001).paused, Is.False);
            Assert.That((bool)Call(session, "AbilityControlsActor", 1001), Is.False);
            Assert.That((bool)Call(session, "IsShieldForceActive", 1001), Is.True);
        }

        [Test] public void SourceTimerMovesDeadButRetainedVictimsAndRestoresTheirPathing()
        {
            var session = Create(out var world); Add(world, 1001, 100, hp: 1);
            Call(session, "BeginShieldEffect", 1, 1, false);
            Assert.That(world.UnitState(1001).health, Is.Zero); Step(session, 1.02);
            Assert.That(world.UnitState(1001).position.x, Is.EqualTo(248.5).Within(1e-8));
            Assert.That(world.UnitState(1001).pathingDisabled, Is.False);
            Assert.That(world.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.UnitDied), Is.LessThanOrEqualTo(1));
        }

        [Test] public void ObservedDarkGiftFlagsBypassNumericArmorAndIncreaseDisplacement()
        {
            var session = Create(out var world); Add(world, 1001, 100);
            Call(session, "BeginShieldEffect", 1, 1, true);
            Assert.That(world.UnitState(1001).health, Is.EqualTo(890));
            Assert.That(world.UnitState(1001).pathingDisabled, Is.True);
            Step(session, .99);
            Assert.That(world.UnitState(1001).position.x, Is.EqualTo(322.75).Within(1e-8));
        }
    }
}

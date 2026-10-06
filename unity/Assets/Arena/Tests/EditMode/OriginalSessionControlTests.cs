using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionControlTests
    {
        static object Call(OriginalSession s, string name, params object[] args) =>
            typeof(OriginalSession).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { null, false };
            var session = (OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            return session;
        }
        static void Set(OriginalSession s, string token, OriginalActorControlMask mask, double seconds, bool negative = true, bool pause = true) =>
            Assert.That(Call(s, "SetActorControl", 1, token, mask, seconds, negative, pause), Is.True);
        static bool Blocked(OriginalSession s, string axis) => (bool)Call(s, "Actor" + axis + "Blocked", 1);
        static void Elapse(OriginalSession s, double seconds) => Call(s, "AdvanceActorControls", seconds);

        [Test] public void SleepProtectsForTwoSecondsThenPositiveDamageWakesWithoutClearingOtherControls()
        {
            var s = Create(out var world); double hp = world.UnitState(1).health;
            Assert.That(Call(s, "AddNativeSleep", 1, 0), Is.True);
            Assert.That(Blocked(s, "Move") && Blocked(s, "Weapon") && Blocked(s, "Cast"), Is.True);
            Assert.That(world.UnitState(1).paused, Is.False);
            Elapse(s, .5);
            Assert.That(Call(s, "ApplyResolvedUnitHit", 2001, 0, world.UnitState(1), 1d, null), Is.True);
            Assert.That(world.UnitState(1).health, Is.EqualTo(hp));
            Assert.That(Call(s, "HasNativeSleep", 1), Is.True);
            Elapse(s, 2);
            Call(s, "SetNativeAbun", 1, "independent", true);
            Call(s, "ApplyResolvedUnitHit", 2001, 0, world.UnitState(1), 1d, null);
            Assert.That(world.UnitState(1).health, Is.EqualTo(hp - 1));
            Assert.That(Call(s, "HasNativeSleep", 1), Is.False);
            Assert.That(Blocked(s, "Move") || Blocked(s, "Cast"), Is.False);
            Assert.That(Blocked(s, "Weapon"), Is.True);
        }

        [Test] public void SleepExpiresAtEightSecondsAndDispelDeathNeverLeaveProtectionBehind()
        {
            var s = Create(out var world);
            Call(s, "AddNativeSleep", 1, 0); Elapse(s, 7.99);
            Assert.That(Call(s, "HasNativeSleep", 1), Is.True);
            Elapse(s, .01); Assert.That(Call(s, "HasNativeSleep", 1), Is.False);
            Call(s, "AddNativeSleep", 1, 0); Call(s, "ClearNegativeActorControls", 1);
            Assert.That(Call(s, "ResolveNativeSleepDamage", 1, 40d), Is.EqualTo(40));
            Call(s, "AddNativeSleep", 1, 0); world.ForceUnitDeath(1); Call(s, "ForgetActorControls", 1);
            Assert.That(Call(s, "HasNativeSleep", 1), Is.False);
            Assert.That(Call(s, "ResolveNativeSleepDamage", 1, 40d), Is.EqualTo(40));
        }

        [Test] public void SleepRefreshRestartsItsSingleBuffAndRejectsInvulnerableTarget()
        {
            var s = Create(out var world);
            Call(s, "AddNativeSleep", 1, 0); Elapse(s, 7);
            Call(s, "AddNativeSleep", 1, 0); Elapse(s, 1);
            Assert.That(Call(s, "HasNativeSleep", 1), Is.True);
            Assert.That(Call(s, "ResolveNativeSleepDamage", 1, 40d), Is.Zero);
            Call(s, "ClearNegativeActorControls", 1); world.SetUnitState(1, invulnerable: true);
            Assert.That(Call(s, "AddNativeSleep", 1, 0), Is.False);
        }

        [Test] public void MeasuredNativeSilenceBlocksWeaponAndCastButLeavesMovementAndClearsOnDispel()
        {
            var s = Create(out _);
            Assert.That(Call(s, "AddNativeSilence", 1, 2001, 7d), Is.True);
            Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.True);
            Assert.That(Blocked(s, "Move"), Is.False);
            Assert.That(Blocked(s, "Weapon"), Is.True); Assert.That(Blocked(s, "Cast"), Is.True);
            Call(s, "ClearNegativeActorControls", 1);
            Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.False);
            Assert.That(Blocked(s, "Weapon") || Blocked(s, "Cast"), Is.False);
        }

        [Test] public void MeasuredAbunDisablesOnlyWeaponAndSurvivesBuffDispelUntilItsSourceRemovesIt()
        {
            var s = Create(out var world); double speed = world.UnitState(1).profile.moveSpeed;
            Assert.That(Call(s, "SetNativeAbun", 1, "test-pull", true), Is.True);
            Assert.That(Blocked(s, "Weapon"), Is.True); Assert.That(Blocked(s, "Move"), Is.False); Assert.That(Blocked(s, "Cast"), Is.False);
            Call(s, "ClearNegativeActorControls", 1);
            Assert.That(Blocked(s, "Weapon"), Is.True);
            Call(s, "SetNativeAbun", 1, "test-boss", true);
            Call(s, "SetNativeAbun", 1, "test-pull", false);
            Assert.That(Blocked(s, "Weapon"), Is.True);
            Call(s, "SetNativeAbun", 1, "test-boss", false);
            Assert.That(Blocked(s, "Weapon"), Is.False);
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(speed));
        }

        [Test] public void MeasuredRootBlocksOnlyMovementAndHasItsOwnPredicateAndBuffRemoval()
        {
            var s = Create(out _);
            Set(s, "unrelated-move", OriginalActorControlMask.Move, 0, negative: false);
            Assert.That(Call(s, "IsNativeRooted", 1), Is.False);
            Assert.That(Call(s, "AddTimedNativeRoot", 1, 1001, 6d), Is.True);
            Assert.That(Call(s, "IsNativeRooted", 1), Is.True);
            Assert.That(Blocked(s, "Move"), Is.True); Assert.That(Blocked(s, "Weapon"), Is.False); Assert.That(Blocked(s, "Cast"), Is.False);
            Call(s, "ClearNativeRoot", 1);
            Assert.That(Call(s, "IsNativeRooted", 1), Is.False);
            Assert.That(Blocked(s, "Move"), Is.True);
        }

        [Test] public void IndependentTokensCombineAxesAndClearingOneDoesNotReleaseAnother()
        {
            var s = Create(out var world); var before = world.UnitState(1);
            Set(s, "root:12", OriginalActorControlMask.Move, 1);
            Set(s, "silence:13", OriginalActorControlMask.Cast, 2);
            Assert.That(Blocked(s, "Move"), Is.True); Assert.That(Blocked(s, "Weapon"), Is.False); Assert.That(Blocked(s, "Cast"), Is.True);
            Call(s, "ClearActorControl", 1, "root:12");
            Assert.That(Blocked(s, "Move"), Is.False); Assert.That(Blocked(s, "Cast"), Is.True);
            Assert.That(world.UnitState(1).paused, Is.EqualTo(before.paused));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(before.profile.moveSpeed));
        }

        [Test] public void ExpirationNeverRestoresStaleOrdersOrChangesBossPause()
        {
            var s = Create(out var world);
            Set(s, "stun", OriginalActorControlMask.Move | OriginalActorControlMask.Weapon | OriginalActorControlMask.Cast, .1, pause: false);
            Assert.That(world.TryMove(1, new OriginalPoint(300, -1400)), Is.True);
            world.SetUnitState(1, paused: true);
            Elapse(s, .1);
            Assert.That(Blocked(s, "Move") || Blocked(s, "Weapon") || Blocked(s, "Cast"), Is.False);
            var actor = world.UnitState(1);
            Assert.That(actor.paused, Is.True); Assert.That(actor.order, Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(actor.destination.x, Is.EqualTo(300));
        }

        [Test] public void PausePolicyAndUntimedControlsAreExplicitPerToken()
        {
            var s = Create(out var world);
            Set(s, "native-pause", OriginalActorControlMask.Cast, .1, pause: true);
            Set(s, "source-clock", OriginalActorControlMask.Weapon, .1, pause: false);
            Set(s, "until-remove", OriginalActorControlMask.Move, 0);
            world.SetUnitState(1, paused: true); Elapse(s, .2);
            Assert.That(Blocked(s, "Cast"), Is.True); Assert.That(Blocked(s, "Weapon"), Is.False); Assert.That(Blocked(s, "Move"), Is.True);
            world.SetUnitState(1, paused: false); Elapse(s, .1);
            Assert.That(Blocked(s, "Cast"), Is.False); Assert.That(Blocked(s, "Move"), Is.True);
        }

        [Test] public void NegativeDispelPreservesIndependentSourceControlAndReapplicationIsLocal()
        {
            var s = Create(out _);
            Set(s, "forced", OriginalActorControlMask.Move, 0, negative: false);
            Set(s, "enemy", OriginalActorControlMask.Cast, .1);
            Set(s, "enemy", OriginalActorControlMask.Weapon, .2);
            Assert.That(Blocked(s, "Cast"), Is.False); Assert.That(Blocked(s, "Weapon"), Is.True);
            Call(s, "ClearNegativeActorControls", 1);
            Assert.That(Blocked(s, "Move"), Is.True); Assert.That(Blocked(s, "Weapon"), Is.False);
        }

        [Test] public void InvalidReplacementCannotDiscardAnActiveControlAndDeadActorsDoNotRemainBlocked()
        {
            var s = Create(out var world); Set(s, "root", OriginalActorControlMask.Move, 1);
            Assert.Throws<TargetInvocationException>(() => Set(s, "root", OriginalActorControlMask.Cast, double.NaN));
            Assert.Throws<TargetInvocationException>(() => Set(s, "root", (OriginalActorControlMask)16, 1));
            Assert.That(Blocked(s, "Move"), Is.True); Assert.That(Blocked(s, "Cast"), Is.False);
            world.ForceUnitDeath(1); Assert.That(Blocked(s, "Move"), Is.False); Elapse(s, .05);
            Assert.That(world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1, revive = true, fullHealth = true } }), Is.True);
            Assert.That(Blocked(s, "Move"), Is.False);
        }

        [Test] public void MovementGateRetainsBodyDestinationAndOrdersIncludingDisabledPathing()
        {
            foreach (bool pathing in new[] { true, false })
            {
                var s = Create(out var world);
                world.SetPathingEnabled(1, pathing); world.TryMove(1, new OriginalPoint(500, -1400));
                var before = world.UnitState(1);
                world.Advance(.05, _ => false);
                var blocked = world.UnitState(1);
                Assert.That(blocked.position.x, Is.EqualTo(before.position.x));
                Assert.That(blocked.position.y, Is.EqualTo(before.position.y));
                Assert.That(blocked.destination.x, Is.EqualTo(500)); Assert.That(blocked.order, Is.EqualTo(OriginalWorldOrder.Move));
                Assert.That(blocked.profile.moveSpeed, Is.EqualTo(before.profile.moveSpeed));
                world.Advance(.05, _ => true);
                Assert.That(world.UnitState(1).position.x, Is.GreaterThan(before.position.x));
            }
        }

        [Test] public void DeathAndRevivalWithinOneBatchCannotCarryAnOldControlIntoTheRevivedActor()
        {
            var s = Create(out var world);
            Set(s, "stun-before-death", OriginalActorControlMask.Move | OriginalActorControlMask.Weapon | OriginalActorControlMask.Cast, 5);
            world.ForceUnitDeath(1);
            Assert.That(world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1, revive = true, fullHealth = true } }), Is.True);
            Call(s, "ProcessWorldEvents");
            Assert.That(Blocked(s, "Move") || Blocked(s, "Weapon") || Blocked(s, "Cast"), Is.False);
            Assert.That(world.UnitState(1).health, Is.GreaterThan(0));
        }

        [Test] public void SessionTickUsesMovementGateAndExpiresWithoutLosingTheMoveIntent()
        {
            var s = Create(out var world); var before = world.UnitState(1).position;
            world.TryMove(1, new OriginalPoint(500, -1400));
            Set(s, "root", OriginalActorControlMask.Move, .1);
            s.Advance(.05); s.Advance(.05);
            Assert.That(world.UnitState(1).position.x, Is.EqualTo(before.x));
            s.Advance(.05);
            Assert.That(world.UnitState(1).position.x, Is.GreaterThan(before.x));
            Assert.That(s.HaltReason, Is.Null);
        }
    }
}

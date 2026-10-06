using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalDuelRingTests
    {
        static readonly OriginalPoint Center = new OriginalPoint(0, -2700);
        static object Call(object target, string name, params object[] args) => typeof(OriginalSession)
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession)
            .GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
        static OriginalSession Start() => (OriginalSession)typeof(OriginalSessionDuelWorldTests)
            .GetMethod("Start", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        static void ReachRing(OriginalSession s)
        {
            typeof(OriginalSessionDuelWorldTests).GetMethod("ReachWorldDuel", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { s });
            Tick(s, 11);
            foreach (var hero in World(s).Snapshot().units.Where(u => u.kind == OriginalWorldUnitKind.Hero))
                World(s).SetUnitState(hero.entityId, invulnerable: true);
            Tick(s, 61.1);
            Assert.That(s.Snapshot().duel.ringStage, Is.EqualTo(1), JsonUtility.ToJson(s.Snapshot().duel));
        }
        static void Tick(OriginalSession s, double seconds)
        {
            while (seconds > 1e-9) { double step = Math.Min(.05, seconds); s.Advance(step); s.DrainEvents(); seconds -= step; }
            Assert.That(s.HaltReason, Is.Null);
        }
        static void AssertNetworkRoundtrip(OriginalSession s)
        {
            var view = s.Snapshot(); var codec = new OriginalUnitySessionCodec();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                code = OriginalSessionReplyCode.Accepted, acknowledgedSequence = view.players[0].acknowledgedSequence, snapshot = view };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True,
                "Ring books apply to every hero and must not invalidate the cooperative snapshot.");
            Assert.That(decoded.snapshot.duel.ringStage, Is.EqualTo(view.duel.ringStage));
            Assert.That(decoded.snapshot.players[0].auxiliaryAbilities.Length, Is.EqualTo(view.players[0].auxiliaryAbilities.Length));
        }
        [Test] public void PushKeepsCapturedDirectionAndEndsOnFollowingTimerCallback()
        {
            var p = new OriginalPoint(705, -2700); var push = new OriginalDuelRingPush(p, Center);
            for (int i = 0; i < 70; i++) Assert.That(push.Tick(p, out p), Is.True);
            Assert.That(p.x, Is.EqualTo(5).Within(1e-8)); Assert.That(p.y, Is.EqualTo(-2700));
            Assert.That(push.Completed, Is.False);
            Assert.That(push.Tick(p, out _), Is.False); Assert.That(push.Completed, Is.True);
        }
        [Test] public void AuthorisedRegionGuardStopsBeforeEnteringForbiddenGeometry()
        {
            var push = new OriginalDuelRingPush(new OriginalPoint(900, -2700), Center);
            Assert.That(push.Tick(new OriginalPoint(900, -2700), out var next), Is.False);
            Assert.That(next.x, Is.EqualTo(900)); Assert.That(push.Completed, Is.True);
        }
        [Test] public void RingIncludesOnlyOutsideRadiusAndInsideEnumerationDistance()
        {
            Assert.That(OriginalDuelRingPush.Outside(new OriginalPoint(810, -2700), Center, 810), Is.False);
            Assert.That(OriginalDuelRingPush.Outside(new OriginalPoint(811, -2700), Center, 810), Is.True);
            Assert.That(OriginalDuelRingPush.Outside(new OriginalPoint(1620, -2700), Center, 810), Is.True);
            Assert.That(OriginalDuelRingPush.Outside(new OriginalPoint(1621, -2700), Center, 810), Is.False);
        }
        [Test] public void ActualDuelTimerChangesAttackAndArmorAtBothPressureStages()
        {
            var s = Start(); var before = (OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1);
            ReachRing(s); var first = (OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1);
            AssertNetworkRoundtrip(s);
            Assert.That(first.attackMinimum.Require(), Is.EqualTo(before.attackMinimum.Require() + 75));
            Assert.That(first.armor.Require(), Is.EqualTo(before.armor.Require() - 35));
            Tick(s, 32.1); var second = (OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1);
            Assert.That(s.Snapshot().duel.ringStage, Is.EqualTo(2));
            AssertNetworkRoundtrip(s);
            Assert.That(second.attackMinimum.Require(), Is.EqualTo(before.attackMinimum.Require() + 150));
            Assert.That(second.armor.Require(), Is.EqualTo(before.armor.Require() - 70));
        }
        [Test] public void RingPhysicallyReturnsHeroAndRestoresMovement()
        {
            var s = Start(); ReachRing(s); Tick(s, 32.1); var w = World(s);
            int slot = s.Snapshot().players.Single(p => p.matchSlot == s.Snapshot().duel.firstSlot).slot;
            w.ForcePosition(slot, new OriginalPoint(780, -2700)); Tick(s, .1);
            Assert.That(w.UnitState(slot).profile.moveSpeed, Is.Zero);
            Tick(s, 2.5);
            Assert.That(w.UnitState(slot).position.x, Is.LessThan(15));
            Assert.That(w.UnitState(slot).profile.moveSpeed, Is.GreaterThan(0));
            Assert.That((bool)Call(s, "IsRingForcedActor", slot), Is.False);
        }
        [Test] public void RingForceOnAnImageRestoresItsOwnRawcodeMovementWithoutHeroUnitLookup()
        {
            var s = Start(); ReachRing(s); Tick(s, 32.1); var w = World(s);
            int source = s.Snapshot().players.Single(p => p.matchSlot == s.Snapshot().duel.firstSlot).slot;
            var actor = w.UnitState(source);
            Assert.That(w.TryPublishIllusions(source, new[] { new OriginalIllusionSpawn { entityId = 1000000999,
                position = new OriginalPoint(780, -2700), profile = actor.profile, health = actor.health, mana = actor.mana } }), Is.True);
            // This isolated image tests ring restoration; no automatic attack or
            // regeneration is requested without a real mirror-cast capture.
            w.SetUnitState(1000000999, paused: true);
            var observedField = typeof(OriginalSession).GetField("observed", BindingFlags.Instance | BindingFlags.NonPublic);
            var observed = observedField.GetValue(s); observedField.SetValue(s, null);
            Tick(s, .1); Assert.That(w.UnitState(1000000999).profile.moveSpeed, Is.Zero);
            Tick(s, 2.5); observedField.SetValue(s, observed);
            Assert.That(w.UnitState(1000000999).profile.moveSpeed, Is.EqualTo(actor.profile.moveSpeed));
        }
        [Test] public void RingSkipsDynamicAbilityOverlayAndIncludesItAfterRemoval()
        {
            var s = Start(); ReachRing(s); Tick(s, 32.1); var w = World(s);
            int id = 500000001;
            w.AddUnit(id, 0, "n07C", new OriginalWorldUnitProfile { maxHealth = 840, maxMana = 250, moveSpeed = 350, collisionRadius = 24 }, new OriginalPoint(780, -2700));
            w.SetUnitState(id, paused: true);
            Call(s, "ApplyUnitAbilityOverlay", id, new[] { "A0K4" }, null);
            Tick(s, .1); Assert.That((bool)Call(s, "IsRingForcedActor", id), Is.False);
            Call(s, "ApplyUnitAbilityOverlay", id, new[] { "A0VY" }, new[] { "A0K4" });
            Tick(s, .1); Assert.That((bool)Call(s, "IsRingForcedActor", id), Is.False);
            Call(s, "ApplyUnitAbilityOverlay", id, null, new[] { "A0VY" });
            Tick(s, .1); Assert.That((bool)Call(s, "IsRingForcedActor", id), Is.True);
        }
    }
}

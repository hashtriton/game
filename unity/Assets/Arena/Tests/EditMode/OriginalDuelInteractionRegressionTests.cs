using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    // PvE kills are a fixture bypass; match and duel clocks,
    // stop-all events, placements, and item intent consumption are real consumers.
    public sealed class OriginalDuelInteractionRegressionTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionRuneTests)
            .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { null, null, true, true, 3, "H008" });
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", Hidden).GetValue(s);
        static IDictionary Queue(OriginalSession s) => (IDictionary)typeof(OriginalSession).GetField("queuedInteractions", Hidden).GetValue(s);
        static void Tick(OriginalSession s, double seconds)
        {
            for (double left = seconds; left > 1e-9;)
            {
                double step = Math.Min(.05, left); s.Advance(step); s.DrainEvents(); left -= step;
                Assert.That(s.HaltReason, Is.Null);
            }
        }
        static OriginalSessionReplyCode Send(OriginalSession s, int slot, OriginalSessionCommandKind kind, long item = 0) =>
            s.Apply(slot == 1 ? 0 : slot - 1, new OriginalSessionCommand { kind = kind,
                sequence = s.Snapshot().players.Single(p => p.slot == slot).acknowledgedSequence + 1, itemInstanceId = item });
        static void ReachDuelPreparation(OriginalSession s)
        {
            var w = World(s);
            foreach (var p in s.Snapshot().players) w.SetUnitState(p.slot, paused: true, invulnerable: true);
            for (int round = 1; round <= 4; round++)
            {
                Tick(s, 2.05);
                foreach (var p in s.Snapshot().players) Assert.That(Send(s, p.slot, OriginalSessionCommandKind.WaveReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Tick(s, 3.1);
                foreach (var enemy in s.Snapshot().enemies)
                {
                    w.ForceUnitDeath(OriginalWorld.EnemyEntityId(enemy.entityId));
                    Assert.That(s.ReportEnemyKilled(enemy.entityId, false), Is.True);
                }
                Tick(s, 3.1);
            }
            Assert.That(s.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(s.Snapshot().hasDuel, Is.False);
        }
        static void ResolvePair(OriginalSession s)
        {
            Tick(s, 11.1); var view = s.Snapshot();
            int victim = view.players.Single(p => p.matchSlot == view.duel.secondSlot).slot;
            World(s).ForceUnitDeath(victim); Assert.That(s.ReportHeroDied(victim), Is.True);
            Tick(s, 2.1);
        }
        [Test] public void AutomaticDuelStopCancelsPreDuelPickupBeforeNaturalReturn()
        {
            var control = Create(); ReachDuelPreparation(control);
            Tick(control, control.Snapshot().remainingSeconds + .1); ResolvePair(control);
            var returnPoint = World(control).UnitState(1).position;
            var s = Create(); ReachDuelPreparation(s);
            Tick(s, Math.Max(0, s.Snapshot().remainingSeconds - .02));
            var w = World(s); w.SetUnitState(1, paused: false, invulnerable: true);
            w.ForcePosition(1, new OriginalPoint(returnPoint.x + 900, returnPoint.y));
            var player = ((IList)typeof(OriginalSession).GetField("players", Hidden).GetValue(s))[0];
            var inventory = (OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            var item = inventory.CreateInstance("I04J");
            var ground = (SortedDictionary<long, OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems", Hidden).GetValue(s);
            ground.Add(item.instanceId, new OriginalGroundItemView { item = item, position = returnPoint });
            Assert.That(Send(s, 1, OriginalSessionCommandKind.InteractItem, item.instanceId), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Queue(s).Count, Is.EqualTo(1)); Assert.That(s.Snapshot().hasDuel, Is.False);
            Tick(s, .12);
            Assert.That(s.Snapshot().duel.kind, Is.EqualTo(OriginalDuelKind.Pairs));
            Assert.That(s.Snapshot().duel.phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            Assert.That(w.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(Queue(s).Count, Is.Zero, "Source pair-prepare:stop-all must cancel smart intent.");
            ResolvePair(s);
            Assert.That(s.Snapshot().players[0].inventory.heroSlots.Any(i => i?.instanceId == item.instanceId), Is.False);
            Assert.That(ground.ContainsKey(item.instanceId), Is.True);
        }
    }
}

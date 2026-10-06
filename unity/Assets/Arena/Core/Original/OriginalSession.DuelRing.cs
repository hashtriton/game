using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class RingForce
        {
            internal int id;
            internal double next;
            internal OriginalDuelRingPush push;
        }
        readonly SortedDictionary<int, RingForce> ringForces = new SortedDictionary<int, RingForce>();
        bool ringRunning;
        int ringTicks;
        double ringNext, worldStepEndsAt;
        bool IsRingForcedActor(int entityId) => ringForces.ContainsKey(entityId);

        void ApplyDuelRingEvent(OriginalDuelEvent item)
        {
            if (world == null) return;
            if (item.kind == OriginalDuelEventKind.RingStarted)
            {
                ringRunning = true; ringTicks = 0;
                ringNext = Math.Max(world.Clock, worldStepEndsAt) - (duel.Time - item.time) + duelCatalog.ringPeriod;
                foreach (var player in players) SetDuelPressure(player, 1, true);
            }
            else if (item.kind == OriginalDuelEventKind.RingStage)
                foreach (var player in players) SetDuelPressure(player, item.amount, false);
            else if (item.kind == OriginalDuelEventKind.RingStopped)
            {
                ringRunning = false;
                foreach (var player in players) SetDuelPressure(player, 0, false);
                // Gs owns independent timers. Removing the book/ring does not
                // cancel an already running displacement in the original.
            }
            else if (item.kind == OriginalDuelEventKind.WorldRule && item.code == "pair-result:remove-A10H")
                SetDuelPressure(players.Find(player => player.matchSlot == item.slot), 0, false);
        }

        void SetDuelPressure(Player player, int stage, bool add)
        {
            if (player == null) return;
            if (stage == 0)
            {
                player.auxiliaryAbilities.Remove("A10H"); player.auxiliaryAbilities.Remove("A10I");
                player.auxiliaryAbilities.Remove("A0LW"); player.auxiliaryAbilities.Remove("A10J");
            }
            else if (add || player.auxiliaryAbilities.ContainsKey("A10H"))
            {
                player.auxiliaryAbilities["A10H"] = 1;
                player.auxiliaryAbilities["A10I"] = stage; player.auxiliaryAbilities["A0LW"] = stage;
                player.auxiliaryAbilities["A10J"] = stage;
            }
        }

        OriginalHeroStatsSnapshot ApplyDuelPressureStats(int slot, OriginalHeroStatsSnapshot stats)
        {
            var player = players.Find(p => p.slot == slot);
            if (player == null || !player.auxiliaryAbilities.ContainsKey("A10H")) return stats;
            int rank = player.auxiliaryAbilities["A10I"];
            double attack = combatCatalog.Ability("A10I").Number("DataA" + rank);
            stats.itemAttackDamageBonus += attack;
            stats.attackMinimum = Plus(stats.attackMinimum, attack); stats.attackMaximum = Plus(stats.attackMaximum, attack);
            stats.armor = Plus(stats.armor, combatCatalog.Ability("A0LW").Number("DataA" + rank));
            return stats;
        }

        void AdvanceDuelRing()
        {
            double now = world.Clock;
            while (true)
            {
                double next = ringRunning ? ringNext : double.PositiveInfinity;
                RingForce due = null;
                foreach (var force in ringForces.Values)
                    if (force.next < next) { next = force.next; due = force; }
                if (next > now + 1e-9) break;
                if (due != null)
                {
                    var actor = world.UnitState(due.id);
                    if (actor == null) { ringForces.Remove(due.id); continue; }
                    due.next += OriginalDuelRingPush.Period;
                    if (due.push.Tick(actor.position, out var position)) world.ForcePosition(due.id, position);
                    if (due.push.Completed) FinishRingForce(due.id);
                }
                else
                {
                    double firingAt = ringNext;
                    ringNext += duelCatalog.ringPeriod;
                    double radius = Math.Max(duelCatalog.ringMinimumRadius,
                        duelCatalog.ringInitialRadius - ringTicks++ * duelCatalog.ringShrinkPerTick);
                    var center = new OriginalPoint(duelCatalog.ringCenterX, duelCatalog.ringCenterY);
                    foreach (var actor in world.Snapshot().units)
                    {
                        if (actor.hidden || actor.health <= .405 || IsRingForcedActor(actor.entityId)) continue;
                        var definition = combatCatalog.Unit(actor.rawcode);
                        string type = definition.Text("type") ?? "";
                        if (Array.IndexOf(type.Split(','), "structure") >= 0) continue;
                        if (HasEffectiveUnitAbility(actor, "A0K4") || HasEffectiveUnitAbility(actor, "A0VY")) continue;
                        if (!OriginalDuelRingPush.Outside(actor.position, center, radius)) continue;
                        var force = new RingForce { id = actor.entityId, next = firingAt + OriginalDuelRingPush.Period,
                            push = new OriginalDuelRingPush(actor.position, center) };
                        CaptureAbilityMovementBase(force.id);
                        ringForces.Add(force.id, force);
                        world.SetPathingEnabled(force.id, false);
                        var profile = actor.profile.Copy(); profile.moveSpeed = 0;
                        if (!world.UpdateProfile(force.id, profile, actor.health, actor.mana)) throw new InvalidOperationException("Ring movement setup failed.");
                    }
                }
            }
        }

        void FinishRingForce(int id)
        {
            ringForces.Remove(id);
            var actor = world.UnitState(id); if (actor == null) return;
            world.SetPathingEnabled(id, true);
            RefreshAbilityMovement(id);
            ReleaseAbilityMovementBase(id);
        }
    }
}

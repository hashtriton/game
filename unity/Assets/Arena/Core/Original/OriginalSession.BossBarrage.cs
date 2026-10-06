using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossBarragePhase
        {
            internal int actor, owner, callback, remaining;
            internal double due, health, angle, orbit;
            internal readonly OriginalPoint[] crystals = new OriginalPoint[4];
        }
        sealed class BossBarrageBolt
        {
            internal int actor, owner, steps;
            internal double due, angle;
            internal OriginalPoint point;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        sealed class BossProphecy { internal int actor, owner; internal double due; internal float remaining = 10; }
        readonly Dictionary<int, int> bossBarrageRemaining = new Dictionary<int, int>();
        readonly Dictionary<int, BossBarragePhase> bossBarrages = new Dictionary<int, BossBarragePhase>();
        readonly List<BossBarrageBolt> bossBarrageBolts = new List<BossBarrageBolt>();
        readonly List<BossProphecy> bossProphecies = new List<BossProphecy>();
        double nextBossBarrageScan = 1;
        uint bossBarrageRandom;

        double NextBossBarrageRandom()
        {
            if (bossBarrageRandom == 0) bossBarrageRandom = unchecked((uint)seed) ^ 0xB025A6Eu;
            if (bossBarrageRandom == 0) bossBarrageRandom = 1;
            bossBarrageRandom ^= bossBarrageRandom << 13; bossBarrageRandom ^= bossBarrageRandom >> 17;
            bossBarrageRandom ^= bossBarrageRandom << 5;
            return bossBarrageRandom / 4294967296d;
        }
        static bool BossBarrageThreshold(int remaining, double fraction) =>
            remaining == 4 ? fraction <= .76 : remaining == 3 ? fraction <= .56 :
            remaining == 2 ? fraction <= .26 : remaining == 1 && fraction <= .06;

        void BeginBossBarrage(int actorId, double angle)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || actor.health <= .405 || bossBarrages.ContainsKey(actorId) ||
                !bossBarrageRemaining.TryGetValue(actorId, out var remaining) || remaining <= 0)
                throw new InvalidOperationException("boss-barrage-unavailable");
            var phase = new BossBarragePhase { actor = actorId, owner = actor.ownerSlot, remaining = remaining,
                health = actor.health, angle = angle, due = world.Clock + .03125 };
            PositionBarrageBoss(phase); PositionBarrageCrystals(phase);
            CancelBossCastOnOrder(actorId); bossSpecialPhases.Add(actorId); bossBarrages.Add(actorId, phase);
        }
        void PositionBarrageBoss(BossBarragePhase phase)
        {
            var actor = world.UnitState(phase.actor); if (actor == null) return;
            if (!world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = phase.actor,
                relocate = true, position = new OriginalPoint(0, -2700), paused = true } }))
                throw new InvalidOperationException("boss-barrage-placement-unavailable");
            RemoveNegativeAbilityBuffs(phase.actor);
            actor = world.UnitState(phase.actor);
            if (actor.health > .405) world.UpdateProfile(phase.actor, actor.profile, phase.health, actor.mana);
        }
        static void PositionBarrageCrystals(BossBarragePhase phase)
        {
            for (int i = 1; i <= 4; i++)
            {
                double angle = (phase.angle + i * 90 + phase.orbit) * .0174532;
                phase.crystals[i - 1] = new OriginalPoint(600 * Math.Cos(angle), -2700 + 600 * Math.Sin(angle));
            }
        }
        void BeginBossBarrageBolt(int actor, int owner, OriginalPoint point, double angle, double due) =>
            bossBarrageBolts.Add(new BossBarrageBolt { actor = actor, owner = owner, point = point,
                angle = angle * .0174532, due = due + .04 });

        void AdvanceBossBarrage()
        {
            while (nextBossBarrageScan <= world.Clock + 1e-9)
            {
                nextBossBarrageScan += 1;
                if (match.Phase != OriginalMatchPhase.Combat && match.Phase != OriginalMatchPhase.FinalIntermission) continue;
                foreach (var pair in bossBarrageRemaining)
                {
                    var actor = world.UnitState(pair.Key);
                    if (actor != null && actor.health > .405 && !actor.hidden && !bossBarrages.ContainsKey(pair.Key) &&
                        BossBarrageThreshold(pair.Value, actor.health / actor.profile.maxHealth))
                        BeginBossBarrage(pair.Key, NextBossBarrageRandom() * 360);
                }
            }
            // VXv uses .03125, while the independent n8v projectile timer is .04.
            // Resolve both clocks in timestamp order, including their final callbacks.
            for (int budget = 0; ; budget++)
            {
                BossBarragePhase phase = null; BossBarrageBolt bolt = null;
                double due = double.PositiveInfinity;
                foreach (var p in bossBarrages.Values) if (p.due < due) { phase = p; due = p.due; }
                foreach (var b in bossBarrageBolts) if (b.due < due) { phase = null; bolt = b; due = b.due; }
                if (due > world.Clock + 1e-9) break;
                if (budget >= 8192) throw new InvalidOperationException("boss-barrage-callback-budget-exceeded");
                if (phase != null)
                {
                    int index = phase.callback++;
                    if (index <= 640)
                    {
                        PositionBarrageBoss(phase);
                        double angle = phase.angle + index * .8;
                        world.SetFacing(phase.actor, angle);
                        if (index > 38 && index < 602 && index % 3 == 0)
                        {
                            BeginBossBarrageBolt(phase.actor, phase.owner, new OriginalPoint(0, -2700), angle, due);
                            if (phase.remaining == 1) BeginBossBarrageBolt(phase.actor, phase.owner, new OriginalPoint(0, -2700), angle + 180, due);
                        }
                        if (phase.remaining <= 3)
                        {
                            phase.orbit -= phase.remaining == 2 && index / 160 % 2 == 1 ? -.5 : .5;
                            PositionBarrageCrystals(phase);
                        }
                        phase.due += .03125;
                    }
                    else
                    {
                        int remaining = --bossBarrageRemaining[phase.actor];
                        if (remaining == 3) ApplyUnitAbilityOverlay(phase.actor, new[] { "A1D6" }, null);
                        if (remaining == 2) ApplyUnitAbilityOverlay(phase.actor, new[] { "A1D7" }, null);
                        if (remaining == 1) ApplyUnitAbilityOverlay(phase.actor, new[] { "A1D8" }, null);
                        if (remaining == 0) bossProphecies.Add(new BossProphecy { actor = phase.actor, owner = phase.owner, due = due + .04 });
                        bossBarrages.Remove(phase.actor); bossSpecialPhases.Remove(phase.actor);
                        world.SetUnitState(phase.actor, paused: false);
                        BeginBossRifts(phase.actor, phase.owner, due);
                    }
                }
                else
                {
                    bolt.due += .04; bolt.steps++;
                    bolt.point = new OriginalPoint(bolt.point.x + 40 * Math.Cos(bolt.angle), bolt.point.y + 40 * Math.Sin(bolt.angle));
                    bool blocked = false;
                    foreach (var p in bossBarrages.Values)
                        foreach (var crystal in p.crystals)
                            if (SquaredDistance(crystal, bolt.point) <= 150 * 150) blocked = true;
                    // Native FirstOfGroup order is unspecified. The host resolves
                    // invulnerable, nonselectable n02P shields before mobile units.
                    // Crystals do not enter the ground-body or wave-count registry.
                    if (!blocked)
                        foreach (var target in world.Snapshot().units)
                        {
                            if (target.health <= .405 || !AreEnemies(bolt.owner, target.ownerSlot) || bolt.victims.Contains(target.entityId) ||
                                SquaredDistance(target.position, bolt.point) > 150 * 150) continue;
                            bolt.victims.Add(target.entityId);
                            ApplyTriggeredHit(bolt.actor, bolt.owner, target, 1500 * .04, OriginalTriggeredDamageMode.ChaosUniversal);
                        }
                    if (blocked || bolt.steps * 40 >= 1050)
                    { bossBarrageBolts.Remove(bolt); ObserveScriptedHelperDeath(); }
                }
            }
            foreach (var prophecy in new List<BossProphecy>(bossProphecies))
                while (prophecy.due <= world.Clock + 1e-9)
                {
                    var actor = world.UnitState(prophecy.actor);
                    if (actor == null || actor.health < .405) { bossProphecies.Remove(prophecy); break; }
                    prophecy.due += .04; prophecy.remaining -= .04f;
                    if (prophecy.remaining > 0) continue;
                    foreach (var target in world.Snapshot().units)
                        if (target.health > .405 && AreEnemies(prophecy.owner, target.ownerSlot) &&
                            SquaredDistance(actor.position, target.position) <= 1200 * 1200)
                            ApplyTriggeredHit(prophecy.actor, prophecy.owner, target, 99999, OriginalTriggeredDamageMode.ChaosUniversal);
                    ClearBossRifts(prophecy.actor); bossProphecies.Remove(prophecy); break;
                }
        }
        void AppendBossBarrageVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var phase in bossBarrages.Values)
                foreach (var point in phase.crystals)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "n02P",
                        sourceEntityId = phase.actor, position = point, radius = 150, progress = 1, variant = 1 });
            foreach (var bolt in bossBarrageBolts)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "h05V", sourceEntityId = bolt.actor,
                    position = bolt.point, end = new OriginalPoint(bolt.point.x + 90 * Math.Cos(bolt.angle), bolt.point.y + 90 * Math.Sin(bolt.angle)),
                    radius = 150, progress = bolt.steps * 40 / 1050d });
            foreach (var prophecy in bossProphecies)
            {
                var actor = world.UnitState(prophecy.actor); if (actor == null) continue;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "n02R",
                    sourceEntityId = prophecy.actor, position = actor.position, radius = 1200, progress = 1 - prophecy.remaining / 10 });
            }
        }
    }
}

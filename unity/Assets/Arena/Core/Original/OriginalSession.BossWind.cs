using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossWindPhase
        {
            internal int actor, owner, callback;
            internal double next, health;
        }
        sealed class BossWind
        {
            internal int actor, owner;
            internal double next;
            internal OriginalBossWindRules rule;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly Dictionary<int, int> bossWindRemaining = new Dictionary<int, int>();
        readonly Dictionary<int, BossWindPhase> bossWindPhases = new Dictionary<int, BossWindPhase>();
        readonly List<BossWind> bossWinds = new List<BossWind>();
        double nextBossWindScan = 1;

        void BeginBossWindPhase(int actorId)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || actor.health <= .405 || bossWindPhases.ContainsKey(actorId) ||
                !bossWindRemaining.ContainsKey(actorId)) throw new InvalidOperationException("boss-wind-phase-unavailable");
            CancelBossCastOnOrder(actorId);
            var phase = new BossWindPhase { actor = actorId, owner = actor.ownerSlot, next = world.Clock + 1.5, health = actor.health };
            PositionWindBoss(phase);
            bossSpecialPhases.Add(actorId); bossWindPhases.Add(actorId, phase);
        }
        void PositionWindBoss(BossWindPhase phase)
        {
            var actor = world.UnitState(phase.actor); if (actor == null) return;
            if (!world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = phase.actor,
                relocate = true, position = new OriginalPoint(0, -2700), paused = true } }))
                throw new InvalidOperationException("boss-wind-placement-unavailable");
            RemoveNegativeAbilityBuffs(phase.actor); actor = world.UnitState(phase.actor);
            if (actor.health > 0) world.UpdateProfile(phase.actor, actor.profile,
                Math.Min(phase.health, actor.profile.maxHealth), actor.mana);
        }
        void AdvanceBossWind()
        {
            while (nextBossWindScan <= world.Clock + 1e-9)
            {
                nextBossWindScan += 1;
                if (match.Phase != OriginalMatchPhase.Combat && match.Phase != OriginalMatchPhase.FinalIntermission) continue;
                foreach (var entry in bossWindRemaining)
                {
                    var actor = world.UnitState(entry.Key);
                    if (actor != null && actor.health > .405 && !actor.hidden && !bossWindPhases.ContainsKey(entry.Key) &&
                        OriginalBossWindRules.ThresholdReached(entry.Value, actor.health / actor.profile.maxHealth)) BeginBossWindPhase(entry.Key);
                }
            }
            for (int callbacks = 0; ; callbacks++)
            {
                BossWindPhase phase = null; BossWind wind = null; double due = double.PositiveInfinity;
                foreach (var item in bossWindPhases.Values) if (item.next < due) { phase = item; due = item.next; }
                foreach (var item in bossWinds) if (item.next < due) { phase = null; wind = item; due = item.next; }
                if (due > world.Clock + 1e-9) return;
                if (callbacks >= 4096) throw new InvalidOperationException("boss-wind-callback-budget-exceeded");
                if (phase != null)
                {
                    int index = phase.callback++;
                    if (index > 2)
                    {
                        PositionWindBoss(phase);
                        var actor = world.UnitState(phase.actor);
                        double facing = (actor?.facingDegrees ?? 0) + (index % 2 == 0 ? 22.5 : 0);
                        for (int i = 8; i > 0; i--)
                        {
                            double angle = i * 45 * .0174532;
                            bossWinds.Add(new BossWind { actor = phase.actor, owner = phase.owner, next = due + .04,
                                rule = new OriginalBossWindRules(new OriginalPoint(30 * Math.Cos(angle), -2700 + 30 * Math.Sin(angle)), i * 45 + facing) });
                        }
                    }
                    if (index > 12)
                    {
                        bossWindRemaining[phase.actor]--;
                        if (bossWindRemaining[phase.actor] == 2) ApplyUnitAbilityOverlay(phase.actor, new[] { "A0TS" }, null);
                        if (bossWindRemaining[phase.actor] == 1) ApplyUnitAbilityOverlay(phase.actor, new[] { "A0TU" }, null);
                        bossWindPhases.Remove(phase.actor); bossSpecialPhases.Remove(phase.actor);
                        world.SetUnitState(phase.actor, paused: false);
                    }
                    else phase.next += 1.5;
                }
                else
                {
                    wind.next += .04;
                    if (wind.rule.Tick(bossWindRemaining[wind.actor], out var center))
                        foreach (var target in world.Snapshot().units)
                        {
                            // n_v has no structure/magic-immune filter. Native
                            // incoming damage rules decide whether the hit hurts.
                            if (target.health <= .405 || !AreEnemies(wind.owner, target.ownerSlot) || wind.victims.Contains(target.entityId) ||
                                SquaredDistance(center, target.position) > 120 * 120) continue;
                            wind.victims.Add(target.entityId);
                            ApplyTriggeredHit(wind.actor, wind.owner, target, target.profile.maxHealth * .18, OriginalTriggeredDamageMode.SpellMagic);
                        }
                    if (wind.rule.Completed) bossWinds.Remove(wind);
                }
            }
        }
        void AppendBossWindVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var wind in bossWinds)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "A0LH", sourceEntityId = wind.actor,
                    position = wind.rule.Position, end = new OriginalPoint(wind.rule.Position.x + 90 * Math.Cos(wind.rule.Heading),
                        wind.rule.Position.y + 90 * Math.Sin(wind.rule.Heading)), radius = 120,
                    progress = Math.Min(1, wind.rule.Travelled / OriginalBossWindRules.Range) });
        }
    }
}

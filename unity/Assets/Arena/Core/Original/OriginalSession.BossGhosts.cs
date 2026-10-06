using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossGhostPhase
        {
            internal int actor, owner, bursts;
            internal double next, health, heading;
            internal bool odd = true;
        }
        sealed class BossGhost
        {
            internal int actor, owner;
            internal double next;
            internal OriginalBossGhostRules rules;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly Dictionary<int, BossGhostPhase> bossGhostPhases = new Dictionary<int, BossGhostPhase>();
        readonly List<BossGhost> bossGhosts = new List<BossGhost>();
        readonly Dictionary<int, int> bossGhostRemaining = new Dictionary<int, int>();
        double nextBossGhostScan = 1;

        // Trusted nFv timer entry, after the caller resolves native A0LH and
        // starts the separate IZ rain helper. This is not a client command.
        void BeginBossGhostPhase(int actorId, double heading)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || actor.health <= .405 || bossGhostPhases.ContainsKey(actorId) ||
                !OriginalCombatDefinition.IsFinite(heading) || !TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic))
                throw new InvalidOperationException("boss-ghost-phase-unavailable");
            CancelBossCastOnOrder(actorId);
            SpawnBossPhaseAddIfRequired();
            ApplyUnitAbilityOverlay(actorId, new[] { "A0A7", "A0LH" }, new[] { "A101" });
            BeginBossRain(actorId, actor.ownerSlot, new OriginalPoint(-576 + NextBossMeteorRandom() * 1152, -3328 + NextBossMeteorRandom() * 1216));
            var phase = new BossGhostPhase { actor = actorId, owner = actor.ownerSlot, health = actor.health,
                heading = heading, next = world.Clock + 2, bursts = 4 };
            PositionGhostBoss(phase);
            bossSpecialPhases.Add(actorId); bossGhostPhases.Add(actorId, phase);
        }
        void PositionGhostBoss(BossGhostPhase phase)
        {
            var actor = world.UnitState(phase.actor); if (actor == null) return;
            if (!world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = actor.entityId,
                relocate = true, position = new OriginalPoint(0, -2680), paused = true,
                setFacing = true, facing = phase.heading } }))
                throw new InvalidOperationException("boss-ghost-placement-unavailable");
            RemoveNegativeAbilityBuffs(actor.entityId);
            actor = world.UnitState(phase.actor);
            // SetWidgetLife does not revive a corpse. Every burst restores the
            // life captured at phase entry, not a percentage of current max HP.
            if (actor.health > 0) world.UpdateProfile(actor.entityId, actor.profile,
                Math.Min(phase.health, actor.profile.maxHealth), actor.mana);
        }
        void AdvanceBossGhosts()
        {
            while (nextBossGhostScan <= world.Clock + 1e-9)
            {
                nextBossGhostScan += 1;
                if (match.Phase != OriginalMatchPhase.Combat && match.Phase != OriginalMatchPhase.FinalIntermission) continue;
                foreach (var pair in bossGhostRemaining)
                {
                    var actor = world.UnitState(pair.Key);
                    double threshold = pair.Value == 2 ? .65 : pair.Value == 1 ? .35 : -1;
                    if (actor == null || actor.health <= .405 || actor.hidden || bossGhostPhases.ContainsKey(pair.Key) ||
                        actor.health / actor.profile.maxHealth > threshold) continue;
                    double heading = (int)(NextBossMeteorRandom() * 4) * 90 + (pair.Value == 2 ? 0 : 45);
                    BeginBossGhostPhase(pair.Key, heading);
                }
            }
            // Merge phase and projectile callbacks by due time. Catch-up frames
            // must not process the fifth cleanup before earlier projectile hits.
            for (int callbacks = 0; ; callbacks++)
            {
                BossGhostPhase phase = null; BossGhost ghost = null; double next = double.PositiveInfinity;
                foreach (var value in bossGhostPhases.Values)
                    if (value.next < next) { next = value.next; phase = value; }
                foreach (var value in bossGhosts)
                    if (value.next < next) { next = value.next; phase = null; ghost = value; }
                if (next > world.Clock + 1e-9) return;
                if (callbacks >= 4096) throw new InvalidOperationException("boss-ghost-callback-budget-exceeded");
                if (phase != null)
                {
                    if (phase.bursts-- == 0)
                    {
                        bossGhostPhases.Remove(phase.actor); bossSpecialPhases.Remove(phase.actor);
                        if (bossGhostRemaining.ContainsKey(phase.actor)) bossGhostRemaining[phase.actor]--;
                        ApplyUnitAbilityOverlay(phase.actor, new[] { "A04V", "A101" }, new[] { "A0A7", "A0LH" });
                        world.SetUnitState(phase.actor, paused: false);
                        continue;
                    }
                    PositionGhostBoss(phase);
                    foreach (var point in OriginalBossGhostRules.Formation(phase.heading, phase.odd))
                        bossGhosts.Add(new BossGhost { actor = phase.actor, owner = phase.owner, next = next + OriginalBossGhostRules.Period,
                            rules = new OriginalBossGhostRules(point, phase.heading) });
                    phase.odd = !phase.odd; phase.next += 2;
                }
                else
                {
                    ghost.next += OriginalBossGhostRules.Period;
                    if (ghost.rules.Tick(out var center))
                        foreach (var target in world.Snapshot().units)
                        {
                            if (target.health <= .405 || !AreEnemies(ghost.owner, target.ownerSlot) || ghost.victims.Contains(target.entityId) ||
                                CasterMagicImmune(target) || CasterHasType(target, "structure") ||
                                SquaredDistance(center, target.position) > OriginalBossGhostRules.Radius * OriginalBossGhostRules.Radius) continue;
                            ghost.victims.Add(target.entityId);
                            ApplyTriggeredHit(ghost.actor, ghost.owner, target,
                                target.profile.maxHealth * OriginalBossGhostRules.HealthFraction, OriginalTriggeredDamageMode.SpellMagic);
                        }
                    if (ghost.rules.Completed) bossGhosts.Remove(ghost);
                }
            }
        }
        void AppendBossGhostVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var ghost in bossGhosts)
            {
                double angle = ghost.rules.heading * Math.PI / 180;
                var point = ghost.rules.Position;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "A101",
                    sourceEntityId = ghost.actor, position = point, end = new OriginalPoint(point.x + 90 * Math.Cos(angle), point.y + 90 * Math.Sin(angle)),
                    radius = OriginalBossGhostRules.Radius, progress = ghost.rules.Steps / (double)OriginalBossGhostRules.Ticks });
            }
        }
    }
}

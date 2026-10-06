using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossMeteorPhase { internal int actor, owner, remaining = 6; internal double due; }
        sealed class BossMeteorWarning { internal int actor, owner; internal double due; internal OriginalPoint point; }
        readonly Dictionary<int, int> bossMeteorRemaining = new Dictionary<int, int>();
        readonly Dictionary<int, BossMeteorPhase> bossMeteorPhases = new Dictionary<int, BossMeteorPhase>();
        readonly List<BossMeteorWarning> bossMeteorWarnings = new List<BossMeteorWarning>();
        readonly Dictionary<int, double> bossTimedSummons = new Dictionary<int, double>();
        double nextBossMeteorScan = 1;
        uint bossMeteorRandom;

        double NextBossMeteorRandom()
        {
            // Deterministic host replacement for Warcraft GetRandomReal/Int.
            if (bossMeteorRandom == 0) bossMeteorRandom = unchecked((uint)seed) ^ 0x92F5E301u;
            if (bossMeteorRandom == 0) bossMeteorRandom = 1;
            bossMeteorRandom ^= bossMeteorRandom << 13; bossMeteorRandom ^= bossMeteorRandom >> 17; bossMeteorRandom ^= bossMeteorRandom << 5;
            return bossMeteorRandom / 4294967296.0;
        }
        void BeginBossMeteorPhase(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || !bossMeteorRemaining.ContainsKey(id) || bossMeteorPhases.ContainsKey(id))
                throw new InvalidOperationException("boss-meteor-phase-unavailable");
            SpawnBossPhaseAddIfRequired();
            int point = (int)(NextBossMeteorRandom() * 3);
            var positions = new[] { new OriginalPoint(-430, -2400), new OriginalPoint(430, -2400), new OriginalPoint(0, -3200) };
            var headings = new[] { 325d, 225d, 95d };
            if (!world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = id, relocate = true,
                position = positions[point], setFacing = true, facing = headings[point] } }))
                throw new InvalidOperationException("boss-meteor-placement-unavailable");
            RemoveNegativeAbilityBuffs(id);
            TryStartBossShieldCast(id);
            bossMeteorPhases[id] = new BossMeteorPhase { actor = id, owner = actor.ownerSlot, due = world.Clock + 1.5 };
        }
        OriginalPoint BossMeteorTarget()
        {
            // nHv selects one of all eight slots, not one of the living list.
            int slot = 1 + (int)(NextBossMeteorRandom() * 8);
            var player = players.Find(p => p.matchSlot == slot);
            var hero = player == null ? null : world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (player != null && hero != null && hero.health > .405) return hero.position;
            return new OriginalPoint(-576 + NextBossMeteorRandom() * 1152, -3328 + NextBossMeteorRandom() * 1216);
        }
        void ResolveBossMeteor(int actor, int owner, OriginalPoint center, double due)
        {
            // nhv kills its warning first. CA observes this even when nobody is hit.
            ObserveScriptedHelperDeath();
            bool hit = false;
            foreach (var target in world.Snapshot().units)
            {
                if (target.health <= .405 || !AreEnemies(owner, target.ownerSlot) || CasterMagicImmune(target) ||
                    CasterHasType(target, "structure") || SquaredDistance(center, target.position) > 275 * 275) continue;
                hit = true;
                ApplyTriggeredHit(actor, owner, target, target.profile.maxHealth * .16, OriginalTriggeredDamageMode.SpellMagic);
            }
            if (!hit) return;
            // MINION1 fresh native getter confirms the sparse mana maximum0.
            // Other positive fields remain authored, not copied from a guess.
            var definition = combatCatalog.Unit("n06W");
            int summon = SpawnScriptedEnemy("n06W", center, 0, null, null, new OriginalWorldUnitProfile {
                maxHealth = definition.Number("HP"), maxMana = 0, moveSpeed = definition.Number("spd"),
                collisionRadius = OriginalUnitCollisionRules.Resolve(combatCatalog, "n06W").radius }, 0);
            bossTimedSummons[summon] = due + 60;
        }
        void AdvanceBossMeteors()
        {
            while (nextBossMeteorScan <= world.Clock + 1e-9)
            {
                nextBossMeteorScan += 1;
                if (match.Phase != OriginalMatchPhase.Combat && match.Phase != OriginalMatchPhase.FinalIntermission) continue;
                foreach (var pair in bossMeteorRemaining)
                {
                    var actor = world.UnitState(pair.Key);
                    if (actor != null && actor.health > .405 && !actor.hidden && !bossMeteorPhases.ContainsKey(pair.Key) &&
                        OriginalBossQuadrantRules.ThresholdReached(pair.Value, actor.health / actor.profile.maxHealth)) BeginBossMeteorPhase(pair.Key);
                }
            }
            for (int callbacks = 0; ; callbacks++)
            {
                BossMeteorPhase phase = null; BossMeteorWarning warning = null; double due = double.PositiveInfinity;
                foreach (var value in bossMeteorPhases.Values) if (value.due < due) { phase = value; due = value.due; }
                foreach (var value in bossMeteorWarnings) if (value.due < due) { phase = null; warning = value; due = value.due; }
                if (due > world.Clock + 1e-9) break;
                if (callbacks >= 4096) throw new InvalidOperationException("boss-meteor-callback-budget-exceeded");
                if (warning != null)
                {
                    bossMeteorWarnings.Remove(warning); ResolveBossMeteor(warning.actor, warning.owner, warning.point, due);
                }
                else if (phase.remaining-- > 0)
                {
                    bossMeteorWarnings.Add(new BossMeteorWarning { actor = phase.actor, owner = phase.owner, point = BossMeteorTarget(), due = due + 1.3 });
                    phase.due += 1.5;
                }
                else { bossMeteorPhases.Remove(phase.actor); bossMeteorRemaining[phase.actor]--; }
            }
            foreach (var pair in new List<KeyValuePair<int, double>>(bossTimedSummons))
            {
                if (pair.Value > world.Clock + 1e-9) continue;
                bossTimedSummons.Remove(pair.Key);
                if (world.ForceUnitDeath(pair.Key)) { OnPyroUnitDied(pair.Key); OnScriptedEnemyDied(pair.Key, 0); }
            }
        }
        void AppendBossMeteorVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var warning in bossMeteorWarnings)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "h02L",
                    sourceEntityId = warning.actor, position = warning.point, radius = 275,
                    progress = Math.Max(0, Math.Min(1, 1 - (warning.due - world.Clock) / 1.3)) });
        }
    }
}

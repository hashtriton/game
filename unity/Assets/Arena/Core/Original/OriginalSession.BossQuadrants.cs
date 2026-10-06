using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossQuadrants
        {
            internal int actor, safe, pulse;
            internal double due;
            internal float markerScale = 1, healthFraction = .04f;
            internal bool growing = true;
        }
        readonly Dictionary<int, int> bossQuadrantsRemaining = new Dictionary<int, int>();
        readonly Dictionary<int, BossQuadrants> bossQuadrants = new Dictionary<int, BossQuadrants>();
        readonly Dictionary<int, double> bossDispelDamage = new Dictionary<int, double>();
        double nextBossQuadrantScan = 1;
        uint bossQuadrantRandom;

        void ObserveBossNativeDamage(OriginalWorldUnitView actor, double damage)
        {
            // KZ/JZ14900 plus XGv32340: qualifying n017 images entering sV
            // receive their own damage watcher, independently of the original.
            if (actor.rawcode != "n017" || !bossScaling.ContainsKey(actor.entityId) && !HasBossImageDamageWatch(actor.entityId)) return;
            bossDispelDamage.TryGetValue(actor.entityId, out double previous);
            double total = previous + damage; bossDispelDamage[actor.entityId] = total;
            if (total < 3500) return;
            bossDispelDamage[actor.entityId] = 0; // Discard excess, as SaveReal(...,0).
            foreach (var unit in world.Snapshot().units)
                if (unit.rawcode == "n017" && unit.health > .405 && SquaredDistance(unit.position, actor.position) <= 3000 * 3000)
                RemoveNegativeAbilityBuffs(unit.entityId);
        }
        int NextBossSafeQuadrant()
        {
            if (bossQuadrantRandom == 0) bossQuadrantRandom = unchecked((uint)seed) ^ 0x7B301CE9u;
            if (bossQuadrantRandom == 0) bossQuadrantRandom = 1;
            bossQuadrantRandom ^= bossQuadrantRandom << 13; bossQuadrantRandom ^= bossQuadrantRandom >> 17; bossQuadrantRandom ^= bossQuadrantRandom << 5;
            return 1 + (int)(bossQuadrantRandom / 4294967296.0 * 4);
        }
        void BeginBossQuadrants(int actorId, int safe)
        {
            OriginalBossQuadrantRules.Minimum(safe);
            if (!bossQuadrantsRemaining.ContainsKey(actorId) || bossQuadrants.ContainsKey(actorId))
                throw new InvalidOperationException("boss-quadrant-phase-unavailable");
            SpawnBossPhaseAddIfRequired();
            bossQuadrants[actorId] = new BossQuadrants { actor = actorId, safe = safe, due = world.Clock + .03 };
        }
        void AdvanceBossQuadrants()
        {
            while (nextBossQuadrantScan <= world.Clock + 1e-9)
            {
                nextBossQuadrantScan += 1;
                if (match.Phase != OriginalMatchPhase.Combat && match.Phase != OriginalMatchPhase.FinalIntermission) continue;
                foreach (var entry in bossQuadrantsRemaining)
                {
                    var actor = world.UnitState(entry.Key);
                    if (actor != null && actor.health > .405 && !actor.hidden && !bossQuadrants.ContainsKey(entry.Key) &&
                        OriginalBossQuadrantRules.ThresholdReached(entry.Value, actor.health / actor.profile.maxHealth))
                        BeginBossQuadrants(entry.Key, NextBossSafeQuadrant());
                }
            }
            foreach (var phase in new List<BossQuadrants>(bossQuadrants.Values))
                while (phase.due <= world.Clock + 1e-9)
                {
                    if (phase.growing)
                    {
                        phase.markerScale += .12f;
                        if (phase.markerScale < 8) phase.due += .03;
                        else { phase.growing = false; phase.due += 1; }
                        continue;
                    }
                    if (phase.pulse > 10)
                    {
                        bossQuadrants.Remove(phase.actor); bossQuadrantsRemaining[phase.actor]--; break;
                    }
                    ResolveBossQuadrantPulse(phase.actor, phase.safe, phase.healthFraction);
                    phase.healthFraction += .01f; phase.pulse++; phase.due += 1;
                }
        }
        void ResolveBossQuadrantPulse(int actorId, int safe, double fraction)
        {
            double healingBasis = 0;
            for (int quadrant = 1; quadrant <= 4; quadrant++)
            {
                if (quadrant == safe) continue;
                foreach (var target in world.Snapshot().units)
                {
                    bool hero = target.kind == OriginalWorldUnitKind.Hero || target.rawcode == "O006";
                    if (!hero || target.health <= .405 || !AreEnemies(0, target.ownerSlot) || CasterHasType(target, "structure") ||
                        CasterMagicImmune(target) || CasterHasAbility(target, "A0K4") || !OriginalBossQuadrantRules.Contains(quadrant, target.position)) continue;
                    double amount = target.profile.maxHealth * fraction;
                    ApplyTriggeredHit(actorId, 0, target, amount, OriginalTriggeredDamageMode.SpellMagic);
                    healingBasis += amount; // Raw attempted amount, even if native invulnerability rejected damage.
                }
            }
            foreach (var boss in world.Snapshot().units)
                if (boss.rawcode == "n017" && boss.health > .405 && healingBasis > 0 && boss.position.x >= -1184 && boss.position.x <= 1088 &&
                    boss.position.y >= -3968 && boss.position.y <= -1536)
                    world.UpdateProfile(boss.entityId, boss.profile, Math.Min(boss.profile.maxHealth, boss.health + healingBasis * 1.5), boss.mana);
        }
        void AppendBossQuadrantVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var phase in bossQuadrants.Values)
                for (int quadrant = 1; quadrant <= 4; quadrant++)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Rectangle, abilityId = "h04S", sourceEntityId = phase.actor,
                        position = OriginalBossQuadrantRules.Minimum(quadrant), end = OriginalBossQuadrantRules.Maximum(quadrant),
                        variant = quadrant == phase.safe ? 1 : 0, progress = phase.growing ? Math.Min(1, (phase.markerScale - 1) / 7) : 1 });
        }
    }
}

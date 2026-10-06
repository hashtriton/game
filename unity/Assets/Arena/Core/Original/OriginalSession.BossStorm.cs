using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossStormWarning { internal int actor, owner; internal OriginalPoint point; internal double due; }
        sealed class BossStormStun { internal int target; internal double due; }
        readonly List<BossStormWarning> bossStormWarnings = new List<BossStormWarning>();
        readonly List<BossStormStun> bossStormStuns = new List<BossStormStun>();
        readonly List<double> bossStormHelperDeaths = new List<double>();
        readonly Dictionary<int, double> bossStormCooldowns = new Dictionary<int, double>();

        bool TryStartBossStormCast(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 150 ||
                !HasEffectiveUnitAbility(actor, "A1D7") ||
                bossStormCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A1D7", effectAt = world.Clock + .3 };
            return true;
        }
        void BeginBossStorm(int id)
        {
            var actor = world.UnitState(id); if (actor == null) return;
            // aXv27874: three independent square offsets, not a disk or a
            // sampled target position. The host random stream is deterministic.
            for (int i = 0; i < 3; i++)
                bossStormWarnings.Add(new BossStormWarning { actor = id, owner = actor.ownerSlot, due = world.Clock + 1,
                    point = new OriginalPoint(actor.position.x - 700 + 1400 * NextBossBarrageRandom(),
                        actor.position.y - 700 + 1400 * NextBossBarrageRandom()) });
        }
        void ResolveBossStorm(int actor, int owner, OriginalPoint point, double time)
        {
            // aVv removes its warning without a death. A10D orders precede
            // hL1000, but the native missile reaches its target afterwards.
            foreach (var target in world.Snapshot().units)
            {
                double distance = SquaredDistance(point, target.position);
                if (target.health <= .405 || !AreEnemies(owner, target.ownerSlot) || distance > 250 * 250) continue;
                if (!target.hidden && !target.invulnerable && !CasterMagicImmune(target) &&
                    !CasterHasType(target, "mechanical") && !CasterHasType(target, "structure"))
                    bossStormStuns.Add(new BossStormStun { target = target.entityId,
                        // STORM1 observes .00476..005 at coincident coordinates.
                        // Longer flight uses authored9000 speed; that distance
                        // dependence remains a host reconstruction.
                        due = time + Math.Max(.005, Math.Sqrt(distance) / 9000) });
                ApplyTriggeredHit(actor, owner, target, 1000, OriginalTriggeredDamageMode.SpellMagic);
            }
            var definition = combatCatalog.Unit("n025");
            int summon = SpawnScriptedEnemy("n025", point, 0, null, null, new OriginalWorldUnitProfile {
                maxHealth = definition.Number("HP"), maxMana = 0, moveSpeed = definition.Number("spd"),
                collisionRadius = OriginalUnitCollisionRules.Resolve(combatCatalog, "n025").radius }, 0);
            world.SetFacing(summon, NextBossBarrageRandom() * 360);
            bossTimedSummons[summon] = time + 15;
            bossStormHelperDeaths.Add(time + 1);
        }
        void AdvanceBossStorms()
        {
            foreach (var warning in new List<BossStormWarning>(bossStormWarnings))
                if (warning.due <= world.Clock + 1e-9)
                { bossStormWarnings.Remove(warning); ResolveBossStorm(warning.actor, warning.owner, warning.point, warning.due); }
            foreach (var stun in new List<BossStormStun>(bossStormStuns))
            {
                if (stun.due > world.Clock + 1e-9) continue;
                bossStormStuns.Remove(stun);
                var target = world.UnitState(stun.target);
                if (target == null || target.health <= .405 || target.hidden || target.invulnerable || CasterMagicImmune(target)) continue;
                // STORM1: each of three same-callback orders completes. Two
                // native zero events surround BPSE application, no native damage.
                ApplyResolvedUnitHit(0, 0, target, 0);
                target = world.UnitState(stun.target);
                if (target == null || target.health <= .405) continue;
                AddTimedNativeStun(stun.target, "BPSE", 0, 2);
                ApplyResolvedUnitHit(0, 0, target, 0);
            }
            for (int i = bossStormHelperDeaths.Count - 1; i >= 0; i--)
                if (bossStormHelperDeaths[i] <= world.Clock + 1e-9)
                { bossStormHelperDeaths.RemoveAt(i); ObserveScriptedHelperDeath(); }
        }
        void AppendBossStormVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var warning in bossStormWarnings)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A1D7",
                    sourceEntityId = warning.actor, position = warning.point, radius = 250,
                    progress = Math.Max(0, Math.Min(1, 1 - (warning.due - world.Clock))) });
        }
    }
}

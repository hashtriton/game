using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossInferno
        {
            internal int actor, owner, summon;
            internal double due;
            internal OriginalPoint point;
        }
        readonly List<BossInferno> bossInfernos = new List<BossInferno>();
        readonly Dictionary<int, double> bossInfernoCooldowns = new Dictionary<int, double>();
        readonly List<double> bossInfernoHelperDeaths = new List<double>();

        void BeginBossInferno(int actor, int owner, OriginalPoint point)
        {
            bossInfernos.Add(new BossInferno { actor = actor, owner = owner, point = point, due = world.Clock + 1 });
            // BSPAR2: native A0QD's hiddenh011 dies .0097656s after EFFECT,
            // independently of the scripted warning killed one second later.
            // Host uses the nearest .01s deadline before its bounded tick.
            bossInfernoHelperDeaths.Add(world.Clock + .01);
        }

        void AdvanceBossInfernos()
        {
            for (int i = bossInfernoHelperDeaths.Count - 1; i >= 0; i--)
                if (bossInfernoHelperDeaths[i] <= world.Clock + 1e-9)
                { bossInfernoHelperDeaths.RemoveAt(i); ObserveScriptedHelperDeath(); }
            foreach (var cast in new List<BossInferno>(bossInfernos))
            {
                if (cast.due > world.Clock + 1e-9) continue;
                if (cast.summon == 0)
                {
                    // IGv/Igv36278: warning KillUnit precedes the A0YJ helper.
                    ObserveScriptedHelperDeath();
                    var definition = combatCatalog.Unit("n025");
                    cast.summon = SpawnScriptedEnemy("n025", cast.point, 0, null, null, new OriginalWorldUnitProfile {
                        maxHealth = definition.Number("HP"), maxMana = 0, moveSpeed = definition.Number("spd"),
                        collisionRadius = OriginalUnitCollisionRules.Resolve(combatCatalog, "n025").radius }, 0);
                    // INFSTATE2: native summon is hidden, not paused. The normal
                    // hostile AI does not issue orders to hidden world actors.
                    world.SetVisibility(cast.summon, false);
                    bossTimedSummons[cast.summon] = cast.due + 60;
                    cast.due += 1;
                }
                if (cast.due > world.Clock + 1e-9) continue;
                bossInfernos.Remove(cast);
                var summon = world.UnitState(cast.summon);
                if (summon == null || summon.health <= .405) continue;
                // Native visibility is sampled hidden at+1, visible at+1.1.
                // Host reveal at the authored1s impact is a derived subframe
                // ordering. Landing restores the cast point with body displacement.
                if (!world.TryFindFreeSpawn(cast.point, summon.profile.collisionRadius, 512, out var position) ||
                    !world.RelocateStoredPosition(cast.summon, position) || !world.SetVisibility(cast.summon, true))
                    throw new InvalidOperationException("infernal-landing-placement-unavailable");
                SeedNativeImmolationLanding(cast.summon);
                foreach (var target in world.Snapshot().units)
                {
                    // BOSSHELP3 and INFSTATE2 hit at260WC with a hfoo body.
                    // Radius plus body remains a derived exact frontier.
                    double range = 250 + target.profile.collisionRadius;
                    if (target.health <= .405 || target.hidden || target.invulnerable || !AreEnemies(cast.owner, target.ownerSlot) ||
                        CasterMagicImmune(target) || combatCatalog.Unit(target.rawcode).Text("movetp") == "fly" ||
                        SquaredDistance(cast.point, target.position) > range * range) continue;
                    // INFSTATE2 attributes the native impact to n025, not h011.
                    ApplyTriggeredHit(cast.summon, cast.owner, target, 200, OriginalTriggeredDamageMode.SpellMagic);
                    var live = world.UnitState(target.entityId);
                    if (live != null && live.health > .405) AddTimedNativeStun(target.entityId, "BPSE", cast.summon, 2);
                }
            }
        }
        bool TryStartBossInfernoCast(int id, OriginalPoint target)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 150 ||
                !HasEffectiveUnitAbility(actor, "A0QD") || SquaredDistance(actor.position, target) > 1000 * 1000 ||
                bossInfernoCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A0QD", target = target, effectAt = world.Clock + .5 };
            return true;
        }
        int LivingInfernalCount()
        {
            int count = 0;
            foreach (var unit in world.Snapshot().units) if (unit.ownerSlot == 0 && unit.rawcode == "n025" && unit.health > .405) count++;
            return count;
        }
        void AppendBossInfernoVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var cast in bossInfernos)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A0QD",
                    sourceEntityId = cast.actor, position = cast.point, radius = 250,
                    progress = Math.Max(0, Math.Min(1, 1 - (cast.due + (cast.summon == 0 ? 1 : 0) - world.Clock) / 2)) });
        }
    }
}

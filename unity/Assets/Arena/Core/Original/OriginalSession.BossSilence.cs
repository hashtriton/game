using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossSilence { internal int actor, owner; internal OriginalPoint point; internal double due; }
        readonly List<BossSilence> bossSilences = new List<BossSilence>();
        readonly Dictionary<int, double> bossSilenceCooldowns = new Dictionary<int, double>();

        void BeginBossSilence(int actor, int owner, OriginalPoint point) => bossSilences.Add(new BossSilence {
            actor = actor, owner = owner, point = point, due = world.Clock + 2 });
        void AdvanceBossSilences()
        {
            foreach (var cast in new List<BossSilence>(bossSilences))
            {
                if (cast.due > world.Clock + 1e-9) continue;
                bossSilences.Remove(cast); ObserveScriptedHelperDeath();
                foreach (var target in world.Snapshot().units)
                {
                    if (target.health <= .405 || target.hidden || !AreEnemies(cast.owner, target.ownerSlot) || CasterMagicImmune(target)) continue;
                    double distance = SquaredDistance(cast.point, target.position);
                    // Ijv36321 orders A0YI before its separate scripted damage.
                    // CONTROL2 measures DataA15 as Weapon+Cast, leaving movement.
                    double nativeRange = 350 + target.profile.collisionRadius;
                    if (!target.invulnerable && !CasterHasType(target, "mechanical") && !CasterHasType(target, "structure") &&
                        distance <= nativeRange * nativeRange) AddNativeSilence(target.entityId, cast.actor, 7);
                    if (IsNativeHeroPredicate(target) && !CasterHasType(target, "structure") && distance <= 350 * 350)
                        ApplyTriggeredHit(cast.actor, cast.owner, target, target.profile.maxMana - target.mana * .5,
                            OriginalTriggeredDamageMode.SpellMagic);
                }
            }
        }
        bool TryStartBossSilenceCast(int id, OriginalPoint target)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 200 ||
                !HasEffectiveUnitAbility(actor, "A07B") || SquaredDistance(actor.position, target) > 1000 * 1000 ||
                bossSilenceCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A07B", target = target, effectAt = world.Clock + .75 };
            return true;
        }
        bool BossHasNearbyEnemy(OriginalWorldUnitView actor, double radius)
        {
            foreach (var target in world.Snapshot().units)
                if (target.health > .405 && AreEnemies(actor.ownerSlot, target.ownerSlot) && !HasEffectiveUnitAbility(target, "A0K4") &&
                    SquaredDistance(actor.position, target.position) <= radius * radius) return true;
            return false;
        }
        void AppendBossSilenceVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var cast in bossSilences)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A07B",
                    sourceEntityId = cast.actor, position = cast.point, radius = 350,
                    progress = Math.Max(0, Math.Min(1, 1 - (cast.due - world.Clock) / 2)) });
        }
    }
}

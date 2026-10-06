using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossWard
        {
            internal int actor, boss, retaliate;
            internal double effectAt = -1, removeAt = double.PositiveInfinity, cooldown, restoreAt = double.PositiveInfinity;
        }
        readonly Dictionary<int, BossWard> bossWards = new Dictionary<int, BossWard>();
        readonly Dictionary<int, double> bossWardCooldowns = new Dictionary<int, double>();

        void BeginBossWards(int id)
        {
            var actor = world.UnitState(id);
            int count = actor.health / actor.profile.maxHealth <= .15 ? 8 : actor.health / actor.profile.maxHealth <= .3 ? 6 : 4;
            for (int i = count; i > 0; i--)
            {
                double angle = i * (360d / count) * .0174532;
                var point = new OriginalPoint(900 * Math.Cos(angle), -2710 + 900 * Math.Sin(angle));
                int ward = SpawnScriptedEnemy("u00H", actor.position, 0, null, null,
                    new OriginalWorldUnitProfile { maxHealth = 8, maxMana = 0, moveSpeed = 100, collisionRadius = 16 }, 0);
                // Source movetp=fly. Host ignores ground terrain/bodies for this
                // flight; native air-to-air collision is not reconstructed here.
                world.SetPathingEnabled(ward, false); world.ForcePosition(ward, point);
                world.SetFacing(ward, i * (360d / count));
                bossWards[ward] = new BossWard { actor = ward, boss = id };
            }
        }
        bool ObserveBossWardDamage(int attacker, OriginalWorldUnitView target, double eventDamage)
        {
            if (target == null || !bossWards.TryGetValue(target.entityId, out var ward) || target.health <= 2 || eventDamage <= 0) return false;
            // WARD1 hook1/hook40:8->6->4->2; damage is cancelled, not applied
            // after the direct life setter. At<=2HP the source hook is inactive.
            world.SetUnitState(target.entityId, invulnerable: true);
            world.UpdateProfile(target.entityId, target.profile, target.health - 2, target.mana);
            ward.restoreAt = world.Clock + .0001; ward.retaliate = attacker;
            return true;
        }
        void AdvanceBossWards()
        {
            foreach (var ward in new List<BossWard>(bossWards.Values))
            {
                var actor = world.UnitState(ward.actor);
                if (actor == null) { bossWards.Remove(ward.actor); continue; }
                if (ward.removeAt <= world.Clock + 1e-9)
                { world.RemoveUnit(ward.actor); bossWards.Remove(ward.actor); continue; }
                if (actor.health <= .405)
                {
                    if (double.IsPositiveInfinity(ward.removeAt)) bossWards.Remove(ward.actor);
                    continue;
                }
                if (ward.restoreAt <= world.Clock + 1e-9)
                {
                    // Native timer0 observed+.0001; the host rounds to its next
                    // <=.05s simulation tick, retaining temporary invulnerability.
                    world.SetUnitState(ward.actor, invulnerable: false); ward.restoreAt = double.PositiveInfinity;
                    if (ward.retaliate != 0 && world.TryAttackTarget(ward.retaliate, OriginalWorldTargetKind.Unit, ward.actor))
                        OnAcceptedWorldOrder(ward.retaliate);
                    // aQv reissues the same healing order. A pending native
                    // windup is replaced; a committed heal remains on cooldown.
                    if (ward.cooldown <= world.Clock + 1e-9) ward.effectAt = -1;
                }
                actor = world.UnitState(ward.actor);
                var boss = world.UnitState(ward.boss);
                if (boss == null || boss.health <= .405 || boss.hidden || actor.paused || actor.hidden || ActorCastBlocked(ward.actor))
                { ward.effectAt = -1; continue; }
                if (ward.cooldown > world.Clock + 1e-9) continue;
                // WARD1 approach stops at138.25WC for native range100. The
                // target body addition is the host's labelled range frontier.
                double range = 100 + boss.profile.collisionRadius;
                if (SquaredDistance(actor.position, boss.position) > range * range)
                {
                    ward.effectAt = -1;
                    if (!ActorMoveBlocked(ward.actor)) world.TryMove(ward.actor, boss.position);
                    continue;
                }
                world.Stop(ward.actor);
                if (ward.effectAt < 0) { world.MarkCast(ward.actor); ward.effectAt = world.Clock + .5; }
                if (ward.effectAt > world.Clock + 1e-9) continue;
                // Native cost0, effect+.5 and direct healing1000. a2v schedules
                // RemoveUnit.75s after EFFECT, independently of the caster's life.
                world.UpdateProfile(boss.entityId, boss.profile, Math.Min(boss.profile.maxHealth, boss.health + 1000), boss.mana);
                ward.cooldown = ward.effectAt + 1; ward.removeAt = ward.effectAt + .75; ward.effectAt = -1;
            }
        }
        bool TryStartBossWardCast(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 200 ||
                !HasEffectiveUnitAbility(actor, "A0TS") || bossWardCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A0TS", effectAt = world.Clock + .5 };
            return true;
        }
    }
}

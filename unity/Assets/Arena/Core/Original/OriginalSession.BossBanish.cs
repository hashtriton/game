using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossBanish { internal int actor, owner, target; internal double expires; }
        readonly Dictionary<int, BossBanish> bossBanishes = new Dictionary<int, BossBanish>();
        readonly Dictionary<int, double> bossBanishCooldowns = new Dictionary<int, double>();
        bool HasBossBanish(int target) => bossBanishes.ContainsKey(target);
        double BossBanishMovementBonus(int target) => HasBossBanish(target) ? -.75 : 0;

        void BeginBossBanish(int actor, int target)
        {
            var caster = world.UnitState(actor); var victim = world.UnitState(target);
            if (caster == null || victim == null || victim.health <= .405 || victim.hidden || victim.invulnerable || CasterMagicImmune(victim)) return;
            // BAND3 reports the native zero event before BHbn becomes visible.
            ApplyResolvedUnitHit(actor, caster.ownerSlot, victim, 0);
            CaptureAbilityMovementBase(target);
            bossBanishes[target] = new BossBanish { actor = actor, owner = caster.ownerSlot, target = target, expires = world.Clock + 5 };
            SetActorControl(target, "native-banish:BHbn", OriginalActorControlMask.Weapon, 0, true, false);
            RefreshAbilityMovement(target);
        }
        void RemoveBossBanish(int target, bool nativeExpiry = false)
        {
            if (!bossBanishes.TryGetValue(target, out var state)) return;
            bossBanishes.Remove(target); ClearActorControl(target, "native-banish:BHbn");
            RefreshAbilityMovement(target); ReleaseAbilityMovementBase(target);
            // Only ordinary expiry's second zero event is observed. Do not
            // fabricate the same event for a dispel or a dead/removed target.
            var victim = world.UnitState(target);
            if (nativeExpiry && victim != null && victim.health > .405)
                ApplyResolvedUnitHit(state.actor, state.owner, victim, 0);
        }
        void AdvanceBossBanishes()
        {
            foreach (var state in new List<BossBanish>(bossBanishes.Values))
            {
                var target = world.UnitState(state.target);
                if (target == null || target.health <= .405) RemoveBossBanish(state.target);
                else if (state.expires <= world.Clock + 1e-9) RemoveBossBanish(state.target, true);
            }
        }
        bool TryStartBossBanishCast(int actorId, int targetId)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(actorId) ||
                target.health <= .405 || target.hidden || target.invulnerable || CasterMagicImmune(target) || CasterHasType(target, "mechanical") ||
                CasterHasType(target, "structure") || !AreEnemies(actor.ownerSlot, target.ownerSlot) || actor.mana < 200 ||
                !HasEffectiveUnitAbility(actor, "A055") || SquaredDistance(actor.position, target.position) > 800 * 800 ||
                bossBanishCooldowns.TryGetValue(actorId, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(actorId); world.Stop(actorId); world.MarkCast(actorId);
            if (weaponCycles.TryGetValue(actorId, out var cycle)) cycle.winding = false;
            bossCasts[actorId] = new BossNativeCast { actor = actorId, ability = "A055", targetEntity = targetId, effectAt = world.Clock + .75 };
            return true;
        }
        void SelectBossBanish(OriginalWorldUnitView actor)
        {
            OriginalWorldUnitView selected = null; double farthest = -1;
            foreach (var target in world.Snapshot().units)
            {
                double distance = SquaredDistance(actor.position, target.position);
                if (target.health <= .405 || !AreEnemies(actor.ownerSlot, target.ownerSlot) || CasterHasAbility(target, "A0K4") || distance > 800 * 800) continue;
                if (distance > farthest) { selected = target; farthest = distance; }
            }
            // EBv selects the farthest first, then checks BNsi on that target.
            if (selected != null && !HasNativeSilence(selected.entityId)) TryStartBossBanishCast(actor.entityId, selected.entityId);
        }
    }
}

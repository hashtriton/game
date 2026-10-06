using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossBinding
        {
            internal int actor, owner, target;
            internal double expires;
            internal bool handling;
        }
        readonly List<BossBinding> bossBindings = new List<BossBinding>();
        readonly HashSet<int> bossBindingDebuffs = new HashSet<int>();
        readonly Dictionary<int, double> bossBindingCooldowns = new Dictionary<int, double>();
        double BossBindingMovementBonus(int target) => bossBindingDebuffs.Contains(target) ? -.9 : 0;

        void ApplyBossBindingDebuff(int target)
        {
            // BSPAR2: B09F takes250->25 without reducing the observed weapon
            // damage or1.49194s cadence. awv's4s timer removes the native10s buff.
            CaptureAbilityMovementBase(target); bossBindingDebuffs.Add(target); RefreshAbilityMovement(target);
        }
        void RemoveBossBindingDebuff(int target)
        {
            if (!bossBindingDebuffs.Remove(target)) return;
            RefreshAbilityMovement(target); ReleaseAbilityMovementBase(target);
        }

        void BeginBossBinding(int actor, int target)
        {
            var caster = world.UnitState(actor);
            if (caster == null || world.UnitState(target) == null) throw new InvalidOperationException("boss-binding-actor-missing");
            // awv28370: the source attaches a4s damage listener to the caster.
            // Removing the target's native B09F buff does not destroy this listener.
            bossBindings.Add(new BossBinding { actor = actor, owner = caster.ownerSlot, target = target, expires = world.Clock + 4 });
        }
        void ObserveBossBindingDamage(OriginalWorldUnitView actor, double eventDamage)
        {
            foreach (var binding in new List<BossBinding>(bossBindings))
            {
                if (binding.actor != actor.entityId || binding.handling || binding.expires <= world.Clock + 1e-9) continue;
                var target = world.UnitState(binding.target);
                if (target == null) continue;
                // aUv disables only its own trigger during the nested hL(mode3).
                binding.handling = true;
                try { ApplyTriggeredHit(binding.actor, binding.owner, target, eventDamage, OriginalTriggeredDamageMode.ChaosUniversal); }
                finally { binding.handling = false; }
            }
        }
        void AdvanceBossBindings()
        {
            for (int i = bossBindings.Count - 1; i >= 0; i--)
                if (bossBindings[i].expires <= world.Clock + 1e-9)
                { RemoveBossBindingDebuff(bossBindings[i].target); bossBindings.RemoveAt(i); }
        }
        bool TryStartBossBindingCast(int actorId, int targetId)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(actorId) ||
                target.health <= .405 || target.hidden || target.invulnerable || CasterMagicImmune(target) || CasterHasType(target, "mechanical") ||
                CasterHasType(target, "structure") || !AreEnemies(actor.ownerSlot, target.ownerSlot) || actor.mana < 300 ||
                !HasEffectiveUnitAbility(actor, "A0TU") || SquaredDistance(actor.position, target.position) > 900 * 900 ||
                bossBindingCooldowns.TryGetValue(actorId, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(actorId); world.Stop(actorId); world.MarkCast(actorId);
            if (weaponCycles.TryGetValue(actorId, out var cycle)) cycle.winding = false;
            bossCasts[actorId] = new BossNativeCast { actor = actorId, ability = "A0TU", targetEntity = targetId, effectAt = world.Clock + .5 };
            return true;
        }
        void SelectBossBinding(OriginalWorldUnitView actor)
        {
            OriginalWorldUnitView chosen = null; double farthest = -1;
            foreach (var target in world.Snapshot().units)
            {
                if (target.health <= .405 || !IsNativeHeroPredicate(target) || !AreEnemies(actor.ownerSlot, target.ownerSlot) ||
                    CasterHasAbility(target, "A0K4")) continue;
                double distance = SquaredDistance(actor.position, target.position);
                if (distance > 800 * 800 || distance <= farthest) continue;
                chosen = target; farthest = distance;
            }
            if (chosen != null) TryStartBossBindingCast(actor.entityId, chosen.entityId);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossShield { internal int actor; internal double expires, accumulated; }
        readonly Dictionary<int, BossShield> bossShields = new Dictionary<int, BossShield>();
        readonly Dictionary<int, double> bossShieldCooldowns = new Dictionary<int, double>();

        void BeginBossShield(int id, double effectTime)
        {
            // aav/aiv27771: A0V3 is the hidden spellbook containing A0V2 Amim.
            ApplyUnitAbilityOverlay(id, new[] { "A0XA", "A0V3", "A0V2" }, null);
            bossShields[id] = new BossShield { actor = id, expires = effectTime + 7 };
        }
        void ObserveBossShieldDamage(OriginalWorldUnitView target, double damage)
        {
            if (!bossShields.TryGetValue(target.entityId, out var shield)) return;
            // Native damage callbacks run before HP subtraction. Capped healing
            // means this is deliberately not a blanket damage absorption shield.
            world.UpdateProfile(target.entityId, target.profile, Math.Min(target.profile.maxHealth, target.health + damage), target.mana);
            shield.accumulated += damage;
        }
        void AdvanceBossShields()
        {
            foreach (var shield in new List<BossShield>(bossShields.Values))
            {
                if (shield.expires > world.Clock + 1e-9) continue;
                bossShields.Remove(shield.actor);
                var actor = world.UnitState(shield.actor); if (actor == null) continue;
                if (actor.health > 0) world.UpdateProfile(shield.actor, actor.profile,
                    Math.Min(actor.profile.maxHealth, actor.health + shield.accumulated), actor.mana);
                ApplyUnitAbilityOverlay(shield.actor, null, new[] { "A0XA", "A0V3", "A0V2" });
            }
        }
        bool TryStartBossShieldCast(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 150 ||
                !HasEffectiveUnitAbility(actor, "A0X9") || bossShieldCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A0X9", effectAt = world.Clock + .5 };
            return true;
        }
    }
}

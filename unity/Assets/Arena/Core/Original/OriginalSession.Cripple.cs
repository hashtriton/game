using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ShieldCripple
        {
            internal double remaining, updatedAt;
            internal OriginalCrippleRules rules;
        }
        readonly Dictionary<int, ShieldCripple> shieldCripples = new Dictionary<int, ShieldCripple>();

        void ValidateShieldNative(int rank)
        {
            if (rank < 1 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            var cast = combatCatalog.Ability("A102");
            if (cast.Text("code") != "ANcl" || cast.Number("levels") != 3 || cast.Text("Order") != "battleroar" ||
                cast.Text("targs" + rank) != "_" || cast.Number("Cost" + rank) != 20 + 10 * rank ||
                cast.Number("Cool" + rank) != 13 - 2 * rank || cast.Number("DataC" + rank) != 1 ||
                cast.overrides.Length != 0 || combatCatalog.Unit("H008").Number("castpt") != .3)
                throw new InvalidOperationException("shield-native-declaration-conflict");
            _ = new OriginalCrippleRules(combatCatalog);
            // CHCR5 rank1..3 + CHREM1: EFFECT at .3, debit then cooldown;
            // Stop before effect cancels without debit, Stop after does not refund.
            if (!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellNormal) ||
                !TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.ChaosUniversal))
                throw new InvalidOperationException("shield-native-damage-mode-unavailable");
        }

        void OrderShieldCripple(int owner, int targetId)
        {
            var target = world.UnitState(targetId);
            if (!ArcherNativeTarget(target, owner)) return;
            var rules = new OriginalCrippleRules(combatCatalog);
            // CHREM1 single and same-callback burst3 all apply immediately.
            // CRIPDMG1 records one helper zero-damage event BEFORE B08M.
            ApplyResolvedUnitHit(0, owner, target, 0, 0);
            target = world.UnitState(targetId);
            if (!ArcherNativeTarget(target, owner)) return;
            // Only rank1 is authored. Reapplying that same native buff resets
            // its duration; cross-rank Acri arbitration is outside this rule.
            shieldCripples[targetId] = new ShieldCripple { rules = rules, remaining = rules.duration, updatedAt = world.Clock };
        }

        double ApplyCrippleWeaponDamage(int id, double rolledBaseAndPrimary, double flatBonus)
        {
            double white=OrdinaryWhiteDamage(id,rolledBaseAndPrimary);
            // Multiple native debuff families are reconstructed in one white
            // stage; the measured A103 stage remains separate. Flat bonuses
            // never enter either percentage reduction.
            return shieldCripples.TryGetValue(id,out var buff)?buff.rules.WeaponDamage(white,flatBonus):white+flatBonus;
        }

        void RemoveShieldCripple(int id) => shieldCripples.Remove(id);

        void AdvanceShieldCripples()
        {
            foreach (var pair in new List<KeyValuePair<int, ShieldCripple>>(shieldCripples))
            {
                var actor = world.UnitState(pair.Key);
                if (actor == null || actor.health <= .405) { shieldCripples.Remove(pair.Key); continue; }
                var buff = pair.Value;
                double elapsed = Math.Max(0, world.Clock - buff.updatedAt); buff.updatedAt = world.Clock;
                // CHREM1 paused targets retain B08M through8s; CRIPDMG1's
                // unpaused attackers recover after the declared6s duration.
                if (!actor.paused) buff.remaining -= elapsed;
                if (buff.remaining <= 1e-9) shieldCripples.Remove(pair.Key);
            }
        }
    }
}

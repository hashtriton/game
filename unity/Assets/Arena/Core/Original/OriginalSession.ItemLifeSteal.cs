using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        static bool ItemOrbFamily(string code) => code == "AIva" || code == "AIob" || code == "AIcb" ||
            code == "AIfb" || code == "AIpb" || code == "AIll" || code == "AIos";

        OriginalItemNativeCombatAbility HighestItemOrb(int actorId)
        {
            // Blizzard's classic item reference: only the highest inventory
            // orb effect applies, including Mask of Death. The ordered rows
            // preserve physical slots; flat orb damage is composed separately.
            foreach (var row in ItemNativeCombatAbilities(actorId))
                if (ItemOrbFamily(combatCatalog.Ability(row.abilityId)?.Text("code"))) return row;
            return null;
        }

        bool ItemAuraRecipient(OriginalWorldUnitView emitter, OriginalWorldUnitView target,
            OriginalCombatDefinition ability, int rank)
        {
            if (emitter == null || target == null || emitter.health <= .405 || target.health <= .405 ||
                emitter.hidden || target.hidden) return false;
            string suffix = rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string declared = ability.Text("targs" + suffix);
            if (declared == null || !ability.TryNumber("Area" + suffix, out double radius, out _) || radius < 0 ||
                SquaredDistance(emitter.position, target.position) > radius * radius) return false;
            var flags = declared.Split(',');
            bool Has(string flag) => Array.IndexOf(flags, flag) >= 0;
            bool enemy = AreEnemies(emitter.ownerSlot, target.ownerSlot);
            if (emitter.entityId == target.entityId ? !Has("self") :
                enemy ? !(Has("enemy") || Has("enemies")) : !(Has("friend") || Has("allies"))) return false;
            var definition = combatCatalog.Unit(target.rawcode);
            var targetClasses = (definition.Text("targType") ?? "").Split(',');
            // A self-only aura such as ACvp has no ground/air filter at all.
            bool classFilter = false, classMatch = false;
            foreach (string kind in new[] { "air", "ground", "structure", "ward" })
                if (Has(kind)) { classFilter = true; classMatch |= Array.IndexOf(targetClasses, kind) >= 0; }
            if (classFilter && !classMatch) return false;
            if (Has("hero") != Has("nonhero") && IsNativeHeroPredicate(target) != Has("hero")) return false;
            string types = definition.Text("type") ?? "";
            if (Has("organic") && Array.Exists(types.Split(','), type => string.Equals(type, "mechanical", StringComparison.OrdinalIgnoreCase))) return false;
            if (target.invulnerable && !(Has("invu") || Has("invulnerable"))) return false;
            return true;
        }

        void ApplyItemWeaponLifeSteal(int attackerId, int targetId, double actualHealthLoss)
        {
            if (!OriginalCombatDefinition.IsFinite(actualHealthLoss) || actualHealthLoss <= 0) return;
            var attacker = world.UnitState(attackerId); var victim = world.UnitState(targetId);
            if (attacker == null || victim == null || attacker.health <= .405 || attacker.hidden ||
                attacker.kind == OriginalWorldUnitKind.Illusion || !AreEnemies(attacker.ownerSlot, victim.ownerSlot)) return;
            string victimTypes = combatCatalog.Unit(victim.rawcode).Text("type") ?? "";
            if (Array.Exists(victimTypes.Split(','), type => string.Equals(type.Trim(), "structure", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type.Trim(), "mechanical", StringComparison.OrdinalIgnoreCase)) ||
                Array.IndexOf((combatCatalog.Unit(victim.rawcode).Text("targType") ?? "").Split(','), "structure") >= 0) return;
            double fraction = 0;
            var orb = HighestItemOrb(attackerId);
            if (orb != null)
            {
                var ability = combatCatalog.Ability(orb.abilityId);
                if (ability.Text("code") == "AIva" && ability.TryNumber("DataA" + orb.rank, out double value, out _) && value >= 0)
                    fraction = value;
            }
            // AIpv potion/rune and an AIva mask use the strongest source as a
            // host stacking policy. Native mixed copies were not measured.
            fraction=Math.Max(fraction,RuneVampireFraction(attackerId,victim));
            // Classic AUav affects allied melee attacks. We transfer authored
            // radii and percentages, selecting the strongest aura instead of
            // multiplying copies. Continuous membership/no linger and the
            // additive mask+aura combination are explicit host reconstruction.
            var weaponDefinition = combatCatalog.Unit(attacker.rawcode);
            int weapon = SelectedNativeWeaponIndex(weaponDefinition, victim);
            if (weapon > 0 && weaponDefinition.Text("weapTp" + weapon) == "normal" && !CasterMagicImmune(victim))
            {
                double aura = 0;
                foreach (var emitter in world.Snapshot().units)
                {
                    if (emitter.health <= .405 || emitter.hidden || emitter.kind == OriginalWorldUnitKind.Illusion) continue;
                    // Native unit and item emitters share authored AUav target
                    // filters. Image inheritance remains factory-specific.
                    foreach (var ability in NativeUnitAbilities(emitter))
                        if (ability.Text("code") == "AUav" && ItemAuraRecipient(emitter, attacker, ability, 1) &&
                            ability.TryNumber("DataA1", out double nativeValue, out _) && nativeValue >= 0)
                            aura = Math.Max(aura, nativeValue);
                    foreach (var row in ItemNativeCombatAbilities(emitter.entityId))
                    {
                        var ability = combatCatalog.Ability(row.abilityId);
                        if (ability.Text("code") == "AUav" && ItemAuraRecipient(emitter, attacker, ability, row.rank) &&
                            ability.TryNumber("DataA" + row.rank, out double value, out _) && value >= 0) aura = Math.Max(aura, value);
                    }
                }
                fraction += aura;
            }
            if (fraction <= 0) return;
            double health = Math.Min(attacker.profile.maxHealth, attacker.health + actualHealthLoss * fraction);
            if (!world.UpdateProfile(attackerId, attacker.profile, health, attacker.mana))
                throw new InvalidOperationException("item-lifesteal-profile-rejected");
        }
    }
}

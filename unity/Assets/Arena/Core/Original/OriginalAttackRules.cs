using System;

namespace Arena.Original
{
    // Formula source: Blizzard's official classic Warcraft III guides:
    // https://classic.battle.net/war3/basics/armorandweapontypes.shtml
    // https://classic.battle.net/war3/basics/heroes.shtml
    // Coefficients and matrix values come from the exact map/native catalog,
    // not from the guide's stock balance table. These are declared/derived rules.
    public static class OriginalAttackRules
    {
        public static double ArmorMultiplier(OriginalNativeCatalog catalog, double armor)
        {
            RequireCatalog(catalog); RequireFinite(armor, nameof(armor));
            double coefficient = catalog.Constant("DefenseArmor").Require();
            if (coefficient != .06)
                throw new InvalidOperationException("The verified positive/negative armor formulas require this map's 0.06 coefficient.");
            // Client 1.26 DUEL_PRESSURE (LiADuelP1), H008 level 10:
            // base armor 8.8; A10H stages subtract 35/70. Both resolved 40
            // CHAOS/NORMAL to 68.39562988, matching the -20 curve plateau.
            // This is a runtime-derived clamp, not stated by the guide above.
            return armor >= 0 ? 1 / (1 + coefficient * armor) : 2 - Math.Pow(1 - coefficient, -Math.Max(-20, armor));
        }

        public static double DamageTypeMultiplier(OriginalNativeCatalog catalog, string attackType, string defenseType)
        {
            RequireCatalog(catalog);
            string key;
            switch (attackType)
            {
                case "normal": key = "DamageBonusNormal"; break;
                case "pierce": key = "DamageBonusPierce"; break;
                case "siege": key = "DamageBonusSiege"; break;
                case "magic": key = "DamageBonusMagic"; break;
                case "chaos": key = "DamageBonusChaos"; break;
                case "spells": key = "DamageBonusSpells"; break;
                case "hero": key = "DamageBonusHero"; break;
                default: throw new ArgumentException("Unknown native attack type.", nameof(attackType));
            }
            // Native MiscGame.txt:182 explicitly specifies this eight-column
            // order. In particular NORMAL and DIVINE must not shift HERO/NONE.
            int column;
            switch (defenseType)
            {
                case "small": column = 0; break;
                case "medium": column = 1; break;
                case "large": column = 2; break;
                case "fort": column = 3; break;
                case "normal": column = 4; break;
                case "hero": column = 5; break;
                case "divine": column = 6; break;
                case "none": column = 7; break;
                default: throw new ArgumentException("Expected unit defType, not armor material.", nameof(defenseType));
            }
            var values = catalog.Constant(key).RequireNumbers();
            if (values.Length != 8) throw new InvalidOperationException("Native attack matrix must have eight defense columns.");
            foreach (var value in values)
                if (value < 0) throw new InvalidOperationException("Negative native damage multiplier.");
            return values[column];
        }

        // Ordinary unit weapon strikes only. The world must already resolve
        // target eligibility, invulnerability, ethereal/magic immunity, evasion,
        // attack dice, block and triggered effects. Spell damage flags are a
        // separate rule and must not accidentally inherit numeric armor here.
        public static double WeaponDamage(OriginalNativeCatalog catalog, double damage,
            string attackType, string defenseType, double armor)
        {
            RequireFinite(damage, nameof(damage));
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (attackType == "spells") throw new ArgumentException("Spell damage needs explicit damage-type/armor semantics.", nameof(attackType));
            var multiplier = DamageTypeMultiplier(catalog, attackType, defenseType) * ArmorMultiplier(catalog, armor);
            var result = damage * multiplier;
            if (!Finite(result)) throw new OverflowException("Weapon damage overflow.");
            return result;
        }

        // The official guide proves this formula. No native IAS cap was found
        // in the extracted Misc declarations. This intentionally returns the
        // uncapped formula, not a claim about capped engine/animation timing.
        // Item/ability IAS and damage-point/backswing scaling are not inferred.
        public static double UncappedHeroCooldown(OriginalNativeCatalog catalog, double baseCooldown, double agility)
        {
            RequireCatalog(catalog); RequireFinite(baseCooldown, nameof(baseCooldown)); RequireFinite(agility, nameof(agility));
            if (baseCooldown <= 0) throw new ArgumentOutOfRangeException(nameof(baseCooldown));
            if (agility < 0) throw new ArgumentOutOfRangeException(nameof(agility));
            double coefficient = catalog.Constant("AgiAttackSpeedBonus").Require();
            if (coefficient < 0) throw new InvalidOperationException("Negative native agility attack-speed coefficient.");
            double rate = 1 + agility * coefficient;
            double result = baseCooldown / rate;
            if (!Finite(rate) || !Finite(result) || result <= 0) throw new OverflowException("Attack cooldown cannot be represented.");
            return result;
        }

        private static void RequireCatalog(OriginalNativeCatalog catalog)
        { if (catalog == null) throw new ArgumentNullException(nameof(catalog)); }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static void RequireFinite(double value, string name)
        { if (!Finite(value)) throw new ArgumentOutOfRangeException(name); }
    }
}

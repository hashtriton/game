using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public struct OriginalHeroStatValue
    {
        public bool known;
        public double value;
        public string unresolved;

        public double Require()
        {
            if (!known) throw new InvalidOperationException(unresolved ?? "Unresolved hero stat.");
            if (!OriginalCombatDefinition.IsFinite(value)) throw new InvalidOperationException("Non-finite hero stat.");
            return value;
        }
    }

    [Serializable]
    public sealed class OriginalHeroStatsSnapshot
    {
        public string heroId, primaryAttribute, sourceSha256;
        public int level, maximumLevel, attackDice, attackSides;
        // Declaration arithmetic only: start + (level - 1) * growth. Fractional growth is not
        // promoted to a native integer attribute until its accumulation/rounding is verified.
        public double declaredStrength, declaredAgility, declaredIntelligence;
        public double strengthPerLevel, agilityPerLevel, intelligencePerLevel;
        public double baseHealth, baseMana, baseArmor, baseAttackDamage, baseAttackMinimum, baseAttackMaximum;
        public double baseMoveSpeed, attackRange, baseAttackInterval, baseDamagePoint, baseBackswing;
        public OriginalHeroStatValue strength, agility, intelligence, primary;
        public OriginalHeroStatValue maxHealth, maxMana, armor, primaryDamageBonus, attackMinimum, attackMaximum;
        public OriginalHeroStatValue agilityAttackSpeedBonus, effectiveAttackInterval, effectiveDamagePoint, effectiveBackswing;
        public double itemAttackDamageBonus, itemAttackSpeedBonus, itemHealthRegen, itemManaRegenFraction;
        public double upgradeAttackDamageBonus, upgradeAttackSpeedBonus, upgradeRegenPerSecond;
        public string[] sources, limits;

        public OriginalHeroStatsSnapshot Copy()
        {
            var copy = (OriginalHeroStatsSnapshot)MemberwiseClone();
            copy.sources = sources == null ? null : (string[])sources.Clone();
            copy.limits = limits == null ? null : (string[])limits.Clone();
            return copy;
        }
    }

    // Pure, unitemized base stats. Weapon endpoints are unrounded formula values, not tooltip integers
    // or the scripted Cm ability-attack proxy. No skill, item, upgrade, curse or temporary bonus applies.
    public static class OriginalHeroStats
    {
        public static OriginalHeroStatsSnapshot Calculate(OriginalCombatCatalog catalog, string heroId, int level,
            OriginalObservedCatalog observed = null)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.schemaVersion != 1 || catalog.version != "3.9c" ||
                catalog.sourceSha256 != "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34")
                throw new ArgumentException("Hero stats require the verified 3.9c map catalog.", nameof(catalog));
            if (heroId != "H008" && heroId != "N0A0" && heroId != "H024")
                throw new ArgumentException("Expected one of the three selected original heroes.", nameof(heroId));
            var source = new List<string>();
            var maximumLevel = Integer(Constant(catalog, "MaxHeroLevel", source), "MaxHeroLevel", 1);
            if (level < 1 || level > maximumLevel) throw new ArgumentOutOfRangeException(nameof(level));
            var unit = catalog.Unit(heroId);
            if (unit == null) throw new InvalidOperationException("Missing original unit " + heroId);
            if (unit.overrides == null || unit.overrides.Length != 0)
                throw new InvalidOperationException("Unit binary overrides require explicit resolution before calculating hero stats.");
            var primary = unit.Text("Primary");
            if (primary != "STR" && primary != "AGI" && primary != "INT")
                throw new InvalidOperationException("Unresolved original primary attribute " + heroId);
            AddFieldSources(unit, "Primary", source);

            var str = Number(unit, "STR", source, 0); var agi = Number(unit, "AGI", source, 0);
            var intelligence = Number(unit, "INT", source, 0);
            Integer(str, "STR", 0); Integer(agi, "AGI", 0); Integer(intelligence, "INT", 0);
            var strGrowth = Number(unit, "STRplus", source, 0);
            var agiGrowth = Number(unit, "AGIplus", source, 0);
            var intGrowth = Number(unit, "INTplus", source, 0);
            var result = new OriginalHeroStatsSnapshot
            {
                heroId = heroId, primaryAttribute = primary, sourceSha256 = catalog.sourceSha256,
                level = level, maximumLevel = maximumLevel,
                strengthPerLevel = strGrowth, agilityPerLevel = agiGrowth, intelligencePerLevel = intGrowth,
                declaredStrength = Finite(str + (level - 1) * strGrowth, "STR growth"),
                declaredAgility = Finite(agi + (level - 1) * agiGrowth, "AGI growth"),
                declaredIntelligence = Finite(intelligence + (level - 1) * intGrowth, "INT growth"),
                baseHealth = Number(unit, "HP", source, 0), baseMana = Number(unit, "manaN", source, 0),
                baseArmor = Number(unit, "def", source), baseAttackDamage = Number(unit, "dmgplus1", source),
                attackDice = Integer(Number(unit, "dice1", source), "dice1", 1),
                attackSides = Integer(Number(unit, "sides1", source), "sides1", 1),
                baseMoveSpeed = Number(unit, "spd", source, 0), attackRange = Number(unit, "rangeN1", source, 0),
                baseAttackInterval = Number(unit, "cool1", source, 0),
                baseDamagePoint = Number(unit, "dmgpt1", source, 0), baseBackswing = Number(unit, "backSw1", source, 0)
            };
            if (result.baseAttackInterval == 0)
                throw new InvalidOperationException("The selected hero needs a positive declared attack interval.");

            result.strength = Attribute(result.declaredStrength, strGrowth, level, "STR");
            result.agility = Attribute(result.declaredAgility, agiGrowth, level, "AGI");
            result.intelligence = Attribute(result.declaredIntelligence, intGrowth, level, "INT");
            OriginalObservedHeroLevel measured = null;
            if (observed != null)
            {
                if (observed.TryHero(heroId, level, out measured))
                {
                    result.strength = Known(measured.strength);
                    result.agility = Known(measured.agility);
                    result.intelligence = Known(measured.intelligence);
                    foreach (var key in measured.sourceKeys) AddSource("runtime-cache:" + key, source);
                }
                else
                {
                    result.strength = result.agility = result.intelligence = Unknown("hero-level-not-observed:" + level);
                }
            }
            result.primary = primary == "STR" ? result.strength : primary == "AGI" ? result.agility : result.intelligence;

            // war3mapMisc.txt:4-8,26 overrides stock Warcraft values. In particular, using the
            // stock 25 HP/STR or 15 mana/INT would contradict both the map and the H008 L1 UI probe.
            var hpPerStrength = Constant(catalog, "StrHitPointBonus", source);
            var manaPerIntelligence = Constant(catalog, "IntManaBonus", source);
            var attackPerPrimary = Constant(catalog, "StrAttackBonus", source);
            var armorPerAgility = Constant(catalog, "AgiDefenseBonus", source);
            var armorOffset = Constant(catalog, "AgiDefenseBase", source);
            var attackSpeedPerAgility = Constant(catalog, "AgiAttackSpeedBonus", source);
            result.maxHealth = Scaled(result.strength, hpPerStrength, result.baseHealth, "max-health");
            result.maxMana = Scaled(result.intelligence, manaPerIntelligence, result.baseMana, "max-mana");
            if (measured != null)
            {
                result.maxHealth = Known(measured.maxHP);
                result.maxMana = Known(measured.maxMP);
            }
            result.armor = Scaled(result.agility, armorPerAgility, Finite(result.baseArmor + armorOffset, "armor offset"), "armor");
            result.primaryDamageBonus = Scaled(result.primary, attackPerPrimary, 0, "primary-damage");
            result.baseAttackMinimum = Finite(result.baseAttackDamage + result.attackDice, "minimum weapon damage");
            result.baseAttackMaximum = Finite(result.baseAttackDamage + (double)result.attackDice * result.attackSides, "maximum weapon damage");
            result.attackMinimum = Scaled(result.primaryDamageBonus, 1, result.baseAttackMinimum, "minimum attack");
            result.attackMaximum = Scaled(result.primaryDamageBonus, 1, result.baseAttackMaximum, "maximum attack");
            result.agilityAttackSpeedBonus = Scaled(result.agility, attackSpeedPerAgility, 0, "agility attack speed");

            // The declarations prove an IAS contribution, not the native timing formula, caps,
            // animation scaling, or actual attack-event interval. Keep the raw SLK timings usable.
            const string timingGap = "native-attack-speed-formula-and-caps-unverified";
            result.effectiveAttackInterval = Unknown(timingGap);
            result.effectiveDamagePoint = Unknown(timingGap);
            result.effectiveBackswing = Unknown(timingGap);
            var limits = new List<string>
            {
                "unitemized-base-stats-only", "native-ui-number-rounding-unverified",
                "native-attack-dice-distribution-unverified", timingGap,
                "base-movement-speed-excludes-pathing-and-modifiers"
            };
            if (!result.strength.known || !result.agility.known || !result.intelligence.known)
                limits.Add("fractional-attribute-growth-unverified");
            result.sources = source.ToArray(); result.limits = limits.ToArray();
            return result;
        }

        static OriginalHeroStatValue Attribute(double declared, double growth, int level, string attribute)
        {
            // Both SLK growth metadata and catalog decimals retain fractions. No native-engine
            // evidence currently distinguishes cumulative floor, nearest rounding, or float drift.
            return level == 1 || growth == Math.Truncate(growth)
                ? Known(declared)
                : Unknown("fractional-attribute-growth-unverified:" + attribute);
        }

        static OriginalHeroStatValue Scaled(OriginalHeroStatValue dependency, double coefficient, double baseline, string name)
        {
            return dependency.known
                ? Known(Finite(baseline + dependency.Require() * coefficient, name))
                : Unknown(dependency.unresolved);
        }

        static OriginalHeroStatValue Known(double value)
        { return new OriginalHeroStatValue { known = true, value = Finite(value, "hero stat") }; }
        static OriginalHeroStatValue Unknown(string reason)
        { return new OriginalHeroStatValue { known = false, value = 0, unresolved = reason }; }

        static double Number(OriginalCombatDefinition unit, string key, List<string> sources, double minimum = double.NegativeInfinity)
        {
            if (!unit.TryNumber(key, out var value, out _) || value < minimum)
                throw new InvalidOperationException("Unresolved or invalid original field " + unit.id + "." + key);
            AddFieldSources(unit, key, sources);
            return Finite(value, key);
        }

        static void AddFieldSources(OriginalCombatDefinition unit, string key, List<string> sources)
        {
            foreach (var field in unit.fields)
                if (field.key == key && field.sources != null)
                    foreach (var source in field.sources) AddSource(source, sources);
        }

        static double Constant(OriginalCombatCatalog catalog, string key, List<string> sources)
        {
            OriginalCombatConstant found = null;
            if (catalog.constants != null)
                foreach (var candidate in catalog.constants)
                    if (candidate != null && candidate.key == key)
                    {
                        if (found != null) throw new InvalidOperationException("Duplicate original constant " + key);
                        found = candidate;
                    }
            if (found == null || found.values == null || found.values.Length != 1)
                throw new InvalidOperationException("Unresolved original scalar constant " + key);
            AddSource(found.source, sources);
            return Finite(found.values[0], key);
        }

        static void AddSource(string source, List<string> sources)
        { if (!string.IsNullOrEmpty(source) && !sources.Contains(source)) sources.Add(source); }
        static int Integer(double value, string name, int minimum)
        {
            if (!OriginalCombatDefinition.IsFinite(value) || value < minimum || value > int.MaxValue || value != Math.Truncate(value))
                throw new InvalidOperationException("Invalid original integer field " + name);
            return (int)value;
        }
        static double Finite(double value, string name)
        {
            if (!OriginalCombatDefinition.IsFinite(value)) throw new InvalidOperationException("Non-finite original stat " + name);
            return value;
        }
    }
}

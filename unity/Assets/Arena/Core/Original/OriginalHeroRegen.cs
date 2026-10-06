using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalHeroRegenSnapshot
    {
        public string heroId, sourceSha256, regenerationType;
        public int level;
        public double baseHealthPerSecond, baseManaPerSecond, healthPerStrength, manaPerIntelligence;
        public OriginalHeroStatValue healthPerSecond, manaPerSecond;
        public string[] sources, limits;
    }

    public static class OriginalHeroRegen
    {
        // This is an unitemized rate, not a simulated native regeneration tick.
        // https://classic.battle.net/war3/basics/heroes.shtml documents the additive
        // base + attribute contribution. Map UnitBalance.slk overrides the bases;
        // War3Patch.mpq/{Custom_V1/}Units/MiscGame.txt:137,139 supplies coefficients.
        public static OriginalHeroRegenSnapshot Calculate(OriginalCombatCatalog combat,
            OriginalNativeCatalog native, string heroId, int level)
        {
            if (combat == null) throw new ArgumentNullException(nameof(combat));
            if (native == null) throw new ArgumentNullException(nameof(native));
            var stats = OriginalHeroStats.Calculate(combat, heroId, level);
            var unit = combat.Unit(heroId);
            var source = new List<string>(stats.sources);
            var regeneration = Field(unit, "regenType", source);
            if (regeneration.isNumber || regeneration.text != "always")
                throw new InvalidOperationException("Unresolved regeneration conditions for " + heroId);
            var hpBase = Number(unit, "regenHP", source);
            var manaBase = Number(unit, "regenMana", source);
            var hpCoefficient = Coefficient(native, "StrRegenBonus", source);
            var manaCoefficient = Coefficient(native, "IntRegenBonus", source);
            const string formulaSource = "https://classic.battle.net/war3/basics/heroes.shtml";
            AddSource(formulaSource, source);
            var limits = new List<string>
            {
                "unitemized-base-regeneration-rates-only",
                "native-regeneration-tick-cadence-unverified",
                "native-dead-paused-and-hidden-regeneration-policy-unverified",
                "item-skill-aura-and-temporary-regeneration-modifiers-excluded"
            };
            if (!stats.strength.known || !stats.intelligence.known)
                limits.Add("fractional-attribute-growth-unverified");
            return new OriginalHeroRegenSnapshot
            {
                heroId = heroId, level = level, sourceSha256 = combat.sourceSha256,
                regenerationType = regeneration.text,
                baseHealthPerSecond = hpBase, baseManaPerSecond = manaBase,
                healthPerStrength = hpCoefficient, manaPerIntelligence = manaCoefficient,
                healthPerSecond = Rate(hpBase, hpCoefficient, stats.strength),
                manaPerSecond = Rate(manaBase, manaCoefficient, stats.intelligence),
                sources = source.ToArray(), limits = limits.ToArray()
            };
        }

        static OriginalHeroStatValue Rate(double baseline, double coefficient, OriginalHeroStatValue attribute)
        {
            if (!attribute.known)
                return new OriginalHeroStatValue { known = false, value = 0, unresolved = attribute.unresolved };
            var result = baseline + coefficient * attribute.Require();
            if (!OriginalCombatDefinition.IsFinite(result))
                throw new InvalidOperationException("Non-finite original regeneration rate.");
            return new OriginalHeroStatValue { known = true, value = result };
        }

        static OriginalCombatField Field(OriginalCombatDefinition unit, string key, List<string> sources)
        {
            OriginalCombatField found = null;
            foreach (var field in unit.fields)
                if (field.key == key)
                {
                    if (found != null) throw new InvalidOperationException("Duplicate original regeneration field " + key);
                    found = field;
                }
            if (found == null || found.conflict)
                throw new InvalidOperationException("Unresolved original regeneration field " + unit.id + "." + key);
            if (found.sources != null) foreach (var source in found.sources) AddSource(source, sources);
            return found;
        }

        static double Number(OriginalCombatDefinition unit, string key, List<string> sources)
        {
            var field = Field(unit, key, sources);
            if (!field.isNumber || !OriginalCombatDefinition.IsFinite(field.number))
                throw new InvalidOperationException("Invalid original regeneration field " + unit.id + "." + key);
            return field.number;
        }

        static double Coefficient(OriginalNativeCatalog native, string key, List<string> sources)
        {
            var field = native.Constant(key);
            var number = field.Require();
            foreach (var evidence in field.sources)
                foreach (var source in native.sources)
                    if (source.id == evidence.source)
                        AddSource(source.archive + "!" + source.entry + ":" + evidence.line, sources);
            return number;
        }

        static void AddSource(string source, List<string> sources)
        { if (!string.IsNullOrEmpty(source) && !sources.Contains(source)) sources.Add(source); }
    }
}

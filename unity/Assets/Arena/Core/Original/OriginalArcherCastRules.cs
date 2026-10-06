using System;

namespace Arena.Original
{
    // Map declarations plus ARCHER_NATIVE, LiA3.9c under1.26.0.6401.
    // tools/arena/extract_observed_archer.py verifies the exact nine-row cache
    // 2978e9b993b18cdf124f11e45e09d94399b625cf2be36193eaa1a409778c7fd9.
    public sealed class OriginalArcherCastRules
    {
        public readonly double manaCost, cooldown, castPoint, range, duration;
        public readonly double attackSpeedBonus, movementMultiplier, incomingMultiplier;
        public readonly bool preservesAttackOrder;
        public OriginalArcherCastRules(OriginalCombatCatalog catalog, string id, int rank)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected source map.");
            if (rank < 1 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            if (id != "A0AS" && id != "A15W" && id != "A15Z" && id != "A160" && id != "A161" && id != "A162" && id != "A17M")
                throw new ArgumentException("Unknown Archer cast.");
            var ability = catalog.Ability(id);
            if (ability.Text("code") != (id == "A0AS" ? "Absk" : "ANcl")) throw new InvalidOperationException("Archer native family changed.");
            manaCost = Positive(ability.Number("Cost" + rank)); cooldown = Positive(ability.Number("Cool" + rank));
            castPoint = id == "A0AS" ? 0 : catalog.Unit("N0A0").Number("castpt");
            if (id == "A15W") range = Positive(ability.Number("Rng" + rank));
            if (id == "A0AS")
            {
                duration = Positive(ability.Number("Dur" + rank));
                attackSpeedBonus = Positive(ability.Number("DataB" + rank));
                // All3 ranks: speed250 throughout before/during/after B0AB;
                // controlled incoming40 spell-normal and melee-normal match.
                // These close this exact ability's omitted values, not generic
                // empty SLK fields. Declared IAS is corroborated by real shots.
                movementMultiplier = 1 + MeasuredZero(ability, "DataA" + rank);
                incomingMultiplier = 1 + MeasuredZero(ability, "DataC" + rank);
                if (duration != 5 || attackSpeedBonus != .5 * rank || manaCost != 25 + 15 * rank || cooldown != 16)
                    throw new InvalidOperationException("Absk declaration differs from measured ranks.");
            }
            // Native after-order is0. Later attacks follow incoming damage,
            // so the experiment does not prove preserved weapon intent.
            preservesAttackOrder = false;
        }
        static double MeasuredZero(OriginalCombatDefinition ability, string field)
        {
            if (ability.TryNumber(field, out var value, out var source))
            { if (value != 0) throw new InvalidOperationException("Absk declaration conflicts with native neutral modifier."); }
            else if (source != null) throw new InvalidOperationException("Conflicting Absk modifier declaration.");
            return 0;
        }
        static double Positive(double x) { if (x <= 0) throw new InvalidOperationException("Invalid Archer cost/timing."); return x; }
    }
}

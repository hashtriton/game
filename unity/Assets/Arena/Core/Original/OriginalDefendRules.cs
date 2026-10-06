using System;

namespace Arena.Original
{
    // A05M declaration + actual LiA3.9c/Warcraft1.26.0.6401 ADEF_PROBE.
    // Probe map35eaa9486d0e3cfa9d94bde87220e7a2c4317404d7669a5ceed06a20b1de96d9;
    // captured campaign6132a0c92eff55cf40a034c1867f6ee773e0078054505f67e94b96f4984fc2c5,
    // cache LiADef1.w3v schema7, all42 rows valid, original triggers absent.
    // Cost0/instant toggle, speed and incoming type multipliers are observed.
    // No reflected hit occurred in54 on-state projectile hits; that is not a
    // proof of exact reflection probability0. Missing DataD/F/G/H, outgoing
    // attack timing and death/rank-change persistence remain evidence limits.
    public sealed class OriginalDefendRules
    {
        public readonly double movementMultiplier, pierceMultiplier, magicMultiplier;
        public OriginalDefendRules(OriginalCombatCatalog catalog, int rank)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected map version.");
            if (rank < 1 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            var ability = catalog.Ability("A05M");
            if (ability.Text("code") != "Adef") throw new InvalidOperationException("Unexpected shield native family.");
            movementMultiplier = 1 - Fraction(ability.Number("DataC" + rank));
            pierceMultiplier = Fraction(ability.Number("DataA" + rank));
            magicMultiplier = Fraction(ability.Number("DataE" + rank));
        }
        public double IncomingMultiplier(string attackType)
        {
            switch (attackType)
            {
                case "pierce": return pierceMultiplier;
                case "magic": case "spells": return magicMultiplier;
                case "normal": case "hero": case "siege": case "chaos": return 1;
                default: throw new ArgumentException("Unknown native attack type.", nameof(attackType));
            }
        }
        public double IncomingWeaponDamage(double resolvedDamage, string attackType)
        {
            if (!OriginalCombatDefinition.IsFinite(resolvedDamage) || resolvedDamage < 0) throw new ArgumentOutOfRangeException(nameof(resolvedDamage));
            if (attackType == "spells") throw new ArgumentException("Triggered spell damage needs its own flags.", nameof(attackType));
            double result = resolvedDamage * IncomingMultiplier(attackType);
            // Actual nsca/uskm rank3 hits are1, while the unbounded product is
            // .5..69. This floor is observed for positive pierce weapon hits.
            return attackType == "pierce" && resolvedDamage > 0 ? Math.Max(1, result) : result;
        }
        static double Fraction(double value)
        {
            if (!OriginalCombatDefinition.IsFinite(value) || value < 0 || value > 1) throw new InvalidOperationException("Invalid shield coefficient.");
            return value;
        }
    }
}

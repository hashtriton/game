using System;

namespace Arena.Original
{
    public readonly struct OriginalUnitCollisionResolution
    {
        public readonly double radius;
        public readonly bool derivedHostProxy;
        public readonly bool nativeCollisionKnown;
        public readonly string evidence;

        internal OriginalUnitCollisionResolution(double radius, bool proxy, string evidence)
        { this.radius = radius; derivedHostProxy = proxy; nativeCollisionKnown = false; this.evidence = evidence; }
    }

    public static class OriginalUnitCollisionRules
    {
        const string SourceMap = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        const string BodyCache = "32c0989880c4057f4acc83f826c13c8de24bc415afec78f9d40b2a2cb58df052";

        public static OriginalUnitCollisionResolution Resolve(OriginalCombatCatalog catalog, string id)
        {
            var definition = catalog?.Unit(id) ?? throw new InvalidOperationException("Unknown collision unit: " + id);
            // A conflicting or malformed authored cell is never a missing cell.
            foreach (var field in definition.fields)
            {
                if (field.key != "collision") continue;
                if (!definition.TryNumber("collision", out double declared, out string evidence) || declared <= 0)
                    throw new InvalidOperationException("Invalid original collision: " + id);
                return new OriginalUnitCollisionResolution(declared, false, "declaration: " + evidence);
            }

            // BODY2 rooted controls bracket range150 + anchor24 + [0.8999,1.0996].
            // Radius1 is an explicitly derived host proxy for physical placement;
            // the probe measured a weapon frontier, not native body overlap.
            // Do not apply its separate rooted n008 melee128 finding to ordinary AI.
            if ((id == "n06C" || id == "n06I") && catalog.version == "3.9c" && catalog.sourceSha256 == SourceMap &&
                definition.overrides.Length == 0 && definition.Text("movetp") == "foot" &&
                definition.TryNumber("rangeN1", out double range, out _) && range == 150)
                return new OriginalUnitCollisionResolution(1, true,
                    "derived host proxy from LiABody2.w3v cache SHA256 " + BodyCache + "; physical collision remains unmeasured");
            throw new InvalidOperationException("Unresolved original collision: " + id);
        }
    }
}

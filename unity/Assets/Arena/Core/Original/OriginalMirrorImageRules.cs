using System;

namespace Arena.Original
{
    // LiA3.9c A05N/AOmi declaration, map02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34.
    // Native1.26 LiAOmi1 schema10, campaign09ce84dcf0a8f1f9b827dd13763a2331c50d256af717a884676ac531e220cdcd:
    // effect at .30005; mana60/80/100; images first sampled effect+.52;
    // cooldown16/15/14 from effect; new cast kills old images; lifetime30.
    // B/C are authored combat factors, not measurements of item inheritance.
    // Native hidden placeholder and staggered reveal (.82..98 from command)
    // are omitted from the replacement placement algorithm.
    public sealed class OriginalMirrorImageRules
    {
        public readonly int count;
        public readonly double manaCost, cooldown, castPoint, creationDelay, lifetime, placementRadius, outgoing, incoming;
        public const double HelperDelay = .7, CorpseDelay = 1.5;
        public OriginalMirrorImageRules(OriginalCombatCatalog catalog, int rank)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected map version.");
            OriginalHeroRules.SkillLevel(rank);
            var a = catalog.Ability("A05N");
            if (a.Text("code") != "AOmi") throw new InvalidOperationException("Unexpected mirror-image native family.");
            double n = a.Number("DataA" + rank);
            if (n != rank) throw new InvalidOperationException("Unexpected mirror image count.");
            count = rank; manaCost = Positive(a.Number("Cost" + rank)); cooldown = Positive(a.Number("Cool" + rank));
            creationDelay = Positive(a.Number("DataD" + rank)); lifetime = Positive(a.Number("Dur" + rank));
            placementRadius = Positive(a.Number("Rng" + rank)); outgoing = Positive(a.Number("DataB" + rank));
            incoming = Positive(a.Number("DataC" + rank)); castPoint = Positive(catalog.Unit("H008").Number("castpt"));
        }
        static double Positive(double value)
        {
            if (!OriginalCombatDefinition.IsFinite(value) || value <= 0) throw new InvalidOperationException("Invalid mirror image declaration.");
            return value;
        }
        public static bool ReturnsToOrigin(OriginalPoint p) => p.x >= 640 && p.x <= 1408 && p.y >= 2048 && p.y <= 2720;
    }
}

using System;

namespace Arena.Original
{
    // OOv33569/OKv34284 and IMAGE2 cache1c423f16...cc2db0.
    // Native AIil copies one enemy hero. Values are validated declarations;
    // rank1/2 outgoing and all three incoming axes have positive native pairs.
    public sealed class OriginalEnemyImageRules
    {
        public readonly int rank;
        public readonly double outgoing, incoming, lifetime;
        public const double HelperCastDelay = .05;
        public OriginalEnemyImageRules(OriginalCombatCatalog catalog, int rank)
        {
            if (catalog == null || catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256 || rank < 1 || rank > 2)
                throw new InvalidOperationException("enemy-image-factory-identity-unavailable");
            var ability = catalog.Ability("A0LZ");
            if (ability == null || ability.Text("code") != "AIil" || ability.Number("levels") != 2)
                throw new InvalidOperationException("enemy-image-native-code-conflict");
            this.rank = rank;
            outgoing = ability.Number("DataA" + rank); incoming = ability.Number("DataB" + rank);
            lifetime = ability.Number("Dur" + rank);
            if (outgoing != (rank == 1 ? 1.5 : 1.75) || incoming != (rank == 1 ? 1 : .25) || lifetime != 7 || ability.Number("HeroDur" + rank) != 7)
                throw new InvalidOperationException("enemy-image-measured-declaration-conflict");
        }
    }
}

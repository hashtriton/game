using System;

namespace Arena.Original
{
    // A04C declaration and avv/i8v27692..27747. Native AOmi behavior is
    // measured separately from the source's fixed-center mana-drain timer.
    public sealed class OriginalBossImageRules
    {
        public readonly double cost, cooldown, castPoint, creationDelay, lifetime, radius;
        public readonly int count;
        public OriginalBossImageRules(OriginalCombatCatalog catalog)
        {
            if (catalog == null || catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256)
                throw new InvalidOperationException("boss-image-source-unavailable");
            var ability = catalog.Ability("A04C"); var unit = catalog.Unit("n017");
            if (ability == null || unit == null || ability.Text("code") != "AOmi" || ability.Number("levels") != 1)
                throw new InvalidOperationException("boss-image-factory-unavailable");
            cost=ability.Number("Cost1"); cooldown=ability.Number("Cool1"); castPoint=unit.Number("castpt");
            creationDelay=ability.Number("DataD1"); lifetime=ability.Number("Dur1"); radius=ability.Number("Rng1"); count=(int)ability.Number("DataA1");
            if (cost!=250 || cooldown!=13 || castPoint!=.75 || creationDelay!=.5 || lifetime!=10 || radius!=128 ||
                ability.Number("DataA1")!=2 || ability.Number("DataB1")!=1 || ability.Number("DataC1")!=1 || ability.Number("HeroDur1")!=10)
                throw new InvalidOperationException("boss-image-declaration-conflict");
        }
    }
}

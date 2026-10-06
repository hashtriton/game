using System;

namespace Arena.Original
{
    // Native AIil declarations, source N6/b6 at20667..20678 and CR85265.
    // IMAGE2 validates the family for the three selected heroes. Ordinary
    // donor copying and item activation timing are labelled family transfers.
    public sealed class OriginalItemImageRules
    {
        public const string ItemId="I048", AbilityId="AIil";
        public readonly double manaCost,cooldown,range,outgoing,incoming,lifetime;
        public OriginalItemImageRules(OriginalItemCatalog items,OriginalCombatCatalog combat)
        {
            if(items==null||combat==null||items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256)
                throw new InvalidOperationException("item-image-catalog-identity-conflict");
            var item=items.Item(ItemId);var a=combat.Ability(AbilityId);
            if(item==null||item.cooldownId!=AbilityId||Array.IndexOf(item.abilityIds,AbilityId)<0||
                a==null||a.Text("code")!="AIil"||a.Number("levels")!=1||a.Text("BuffID1")!="BIil")
                throw new InvalidOperationException("item-image-native-identity-conflict");
            manaCost=a.Number("Cost1");cooldown=a.Number("Cool1");range=a.Number("Rng1");
            outgoing=a.Number("DataA1");incoming=a.Number("DataB1");lifetime=a.Number("Dur1");
            if(manaCost!=240||cooldown!=18||range!=600||outgoing!=1||incoming!=2||lifetime!=10||a.Number("HeroDur1")!=10)
                throw new InvalidOperationException("item-image-native-declaration-conflict");
        }
    }
}

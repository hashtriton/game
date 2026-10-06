using System;

namespace Arena.Original
{
    public sealed class OriginalItemFortitudeRules
    {
        public readonly string itemId,abilityId;
        public readonly double cooldown,duration;
        public OriginalItemFortitudeRules(OriginalItemCatalog items,OriginalCombatCatalog combat,string id)
        {
            if(id!="I072"&&id!="I05Q")throw new ArgumentException("Unknown fortitude item.");
            itemId=id;abilityId=id=="I072"?"A0FK":"A11Z";cooldown=id=="I072"?22:20;duration=id=="I072"?6:8;
            var item=items.Item(id);var a=combat.Ability(abilityId);
            if(items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256||
                item==null||item.cooldownId!=abilityId||Array.IndexOf(item.abilityIds,abilityId)<0||a?.Text("code")!="Aami"||
                a.Number("levels")!=1||a.Number("Cool1")!=cooldown||a.Number("Dur1")!=.01||a.Number("HeroDur1")!=.01||
                a.Text("targs1")!="none"||combat.Ability("A18P")?.Text("code")!="Amim"||
                combat.Ability("A18O")?.Text("code")!="Aspb"||combat.Ability("A18Q")?.Text("BuffID1")!="B0B5")
                throw new InvalidOperationException("Fortitude source identity changed.");
            // Native1.26 War3Patch AbilityData AIxs(code Aami), row60117,
            // Cost1=0. Exact aliases have no conflicting map override. Their
            // immediate activation remains an explicit host family transfer.
            bool declared=Array.Exists(a.fields,f=>f.key=="Cost1")||Array.Exists(a.overrides,f=>f.field=="amcs"&&f.level==1);
            if(declared&&(!a.TryNumber("Cost1",out double cost,out _)||cost!=0))
                throw new InvalidOperationException("Fortitude mana conflicts with inherited Aami declaration.");
        }
    }
}

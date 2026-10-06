using System;

namespace Arena.Original
{
    // ITEMEX2 campaign90b192cf: exact native costs90/0, instant SPELL5,
    // retained items; original G8/Tu/wu actions remain source-script rules.
    public sealed class OriginalItemExchangeRules
    {
        public readonly string itemId,abilityId;
        public readonly double manaCost,cooldown;
        public OriginalItemExchangeRules(OriginalItemCatalog items,OriginalCombatCatalog combat,string id)
        {
            itemId=id;abilityId=id=="I017"?"A0UD":id=="I05E"?"A15B":throw new ArgumentException("Unknown exchange item.");
            manaCost=id=="I017"?90:0;cooldown=id=="I017"?16:6;
            var item=items.Item(id);var ability=combat.Ability(abilityId);
            if(items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256||
                item.cooldownId!=abilityId||Array.IndexOf(item.abilityIds,abilityId)<0||ability.Text("code")!="ACtc"||
                ability.Number("Cool1")!=cooldown||ability.Text("targs1")!="none")throw new InvalidOperationException("Item exchange identity conflict.");
            bool declared=Array.Exists(ability.fields,x=>x.key=="Cost1")||Array.Exists(ability.overrides,x=>x.field=="amcs"&&x.level==1);
            if((declared&&!ability.TryNumber("Cost1",out _,out _))||(ability.TryNumber("Cost1",out var cost,out _)&&cost!=manaCost))
                throw new InvalidOperationException("Item exchange measured cost conflicts with declaration.");
            if(id=="I017")
            {
                var a=combat.Ability("A0VG");
                if(a.Text("code")!="ANso"||a.Text("BuffID1")!="B010"||a.Number("DataB1")!=1||a.Number("DataD1")!=-.2||
                    a.Number("DataE1")!=-2||a.Number("Dur1")!=8||a.Number("HeroDur1")!=8)
                    throw new InvalidOperationException("Soul Burn declaration conflict.");
                foreach(string field in new[]{"DataA1","DataC1"})
                    if(a.TryNumber(field,out var value,out _)&&value!=0)throw new InvalidOperationException("Soul Burn sparse field changed.");
            }
        }
    }
}

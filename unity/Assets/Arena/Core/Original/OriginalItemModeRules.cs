using System;

namespace Arena.Original
{
    // MW/qW/jW12949..13097 and FT/GT/jT/kT/KT9777..10049.
    // Replacement is source behavior; no unmeasured AIha healing is invented.
    public static class OriginalItemModeRules
    {
        public static int Family(string id)=>id=="I082"||id=="I083"?1:id=="I09L"||id=="I09M"||id=="I09N"?2:0;
        public static string Next(string id)=>id=="I082"?"I083":id=="I083"?"I082":id=="I09L"?"I09M":id=="I09M"?"I09N":id=="I09N"?"I09L":null;
        public static string Ability(string id)=>Family(id)==1?"A0K0":Family(id)==2?"A0SW":null;
        public static int Armor(double sourceAttack)=>Math.Min(30,(int)(sourceAttack/25));
        public static int Attack(double measuredArmor)=>Math.Min(200,(int)(measuredArmor*2));
        public static void Validate(OriginalItemCatalog items,OriginalCombatCatalog combat,string id)
        {
            string ability=Ability(id);var item=items?.Item(id);var a=combat?.Ability(ability);
            if(ability==null||items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256||
                item==null||item.cooldownId!=ability||Array.IndexOf(item.abilityIds,ability)<0||a?.Text("code")!="AIha"||a.Number("levels")!=1)
                throw new InvalidOperationException("item-mode-source-identity-conflict:"+id);
        }
    }
}

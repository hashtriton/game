using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalItemScriptActRule
    {
        public string itemId, abilityId, cooldownGroup;
        public double manaCost, cooldown;
        internal OriginalItemScriptActRule Copy()=>(OriginalItemScriptActRule)MemberwiseClone();
    }

    // AIda is the native activation carrier. Effects below come from the
    // exact source handlers, not from a fabricated zero armor bonus. Native
    // instant activation transfers from ITEMACT2; this is not17 measured uses.
    public sealed class OriginalItemScriptActRules
    {
        readonly Dictionary<string,OriginalItemScriptActRule> rules=new Dictionary<string,OriginalItemScriptActRule>();
        public OriginalItemScriptActRules(OriginalItemCatalog items,OriginalCombatCatalog combat)
        {
            if(items==null||combat==null||items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256)
                throw new ArgumentException("Script item catalog identity differs.");
            Add("I01R","A0WK",16,0);Add("I03N","A09B",14,275);Add("I03S","A0BI",13,0);
            Add("I03T","A0BM",30,120);Add("I03Y","A0BP",12,120);Add("I0A6","A16O",24,425);Add("I0B1","A1CV",18,50);
            Add("I07O","A0DV",16,90);Add("I07T","A0HZ",16,90);Add("I087","A0JJ",16,90);Add("I076","A0FX",30,85);
            Add("I07U","A1BM",15,50);Add("I09A","A0QQ",20,100);Add("I054","A0KQ",20,200);
            Add("I070","A0T5",18,125);Add("I0AZ","A1CT",20,150);Add("I0B0","A1CU",22,175);
            void Add(string itemId,string abilityId,double cooldown,double mana)
            {
                var item=items.Item(itemId);var ability=combat.Ability(abilityId);
                if(item==null||Array.IndexOf(item.abilityIds,abilityId)<0||item.cooldownId!=abilityId||ability==null||
                    ability.Text("code")!="AIda"||ability.Number("Cool1")!=cooldown)
                    throw new InvalidOperationException("script-item-identity-conflict:"+itemId);
                bool declared=Array.Exists(ability.fields,f=>f.key=="Cost1")||Array.Exists(ability.overrides,f=>f.field=="amcs"&&f.level==1);
                // Sparse AIda Cost1=0 is explicitly inherited from1.26
                // War3Patch AbilityData.slk row56189, as for A057/A0BZ.
                if(declared&&(!ability.TryNumber("Cost1",out double actual,out _)||actual!=mana)||!declared&&mana!=0)
                    throw new InvalidOperationException("script-item-cost-conflict:"+itemId);
                if(!HelpersAvailable(combat,abilityId))return;
                rules.Add(itemId,new OriginalItemScriptActRule{itemId=itemId,abilityId=abilityId,cooldownGroup=item.cooldownId,manaCost=mana,cooldown=cooldown});
            }
        }
        public OriginalItemScriptActRule Rule(string itemId)=>itemId!=null&&rules.TryGetValue(itemId,out var rule)?rule.Copy():null;
        static bool HelpersAvailable(OriginalCombatCatalog combat,string id)
        {
            bool Helper(string raw,string code,string buff,double duration,params object[] fields)
            {
                var a=combat.Ability(raw);if(a==null||a.overrides.Length!=0||a.Text("code")!=code||
                    buff!=null&&a.Text("BuffID1")!=buff)return false;
                if(duration>0&&(!a.TryNumber("Dur1",out double d,out _)||d!=duration||!a.TryNumber("HeroDur1",out d,out _)||d!=duration))return false;
                for(int i=0;i<fields.Length;i+=2)if(!a.TryNumber((string)fields[i],out double v,out _)||v!=Convert.ToDouble(fields[i+1]))return false;
                return true;
            }
            // Helper declarations are validated before a rule is published,
            // hence before activation can spend mana or commit cooldown.
            switch(id)
            {
                case "A09B":return Helper("A03W","AHtb","B02Q",3);
                case "A0FX":return Helper("A0FT","AUts","B03C",0,"levels",2,"DataA1",.22,"DataA2",.44,"DataB1",1,"DataB2",1,"DataC1",20,"DataC2",20);
                case "A1BM":return Helper("A0OR","Aslo","B05X",3,"DataA1",.3,"DataB1",.3);
                case "A0QQ":return Helper("A0WJ","ANsi","B08E",.7,"Area1",250,"DataB1",.3);
                case "A1CT":return Helper("A1CY","ANsi","B0CJ",3,"Area1",400,"DataB1",.6);
                case "A1CU":return Helper("A1CZ","Aslo","B0CK",2,"DataA1",.4);
                case "A0KQ":return Helper("A0X7","Aspb",null,0)&&Helper("A0X8","Aspb",null,0)&&
                    Helper("A0X5","Aasl","B07Q",0)&&Helper("A0X6","Aasl","B07P",0)&&
                    Helper("A0X4","AIdd",null,0,"DataA1",1,"DataB1",1,"DataE1",.7)&&
                    Helper("A0X3","AIdd",null,0,"DataA1",1,"DataB1",1,"DataE1",1.3);
                default:return true;
            }
        }
    }
}

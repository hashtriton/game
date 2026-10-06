using System;

namespace Arena.Original
{
    public sealed class OriginalItemSummonScriptRules
    {
        public readonly string itemId,abilityId="S000",cooldownGroup;
        public readonly double manaCost=250,cooldown=55,range=800;
        public readonly OriginalAbilityTargetMode targetMode;
        public OriginalItemSummonScriptRules(OriginalItemCatalog items,OriginalCombatCatalog combat,string itemId="I00Z")
        {
            if(items==null||combat==null||items.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combat.sourceSha256!=items.mapSha256)
                throw new InvalidOperationException("item-summon-script-catalog-conflict");
            this.itemId=itemId;
            if(itemId=="I049")
            {
                abilityId="A0W8";manaCost=100;cooldown=30;range=700;targetMode=OriginalAbilityTargetMode.Unit;
                var finger=items.Item(itemId);var ability=combat.Ability(abilityId);
                if(finger==null||Array.IndexOf(finger.abilityIds,abilityId)<0||ability.overrides.Length!=0||
                    ability.Text("code")!="ANfd"||ability.Number("levels")!=1||ability.Number("Cost1")!=100||
                    ability.Number("Cool1")!=30||ability.Number("Rng1")!=700||ability.Number("DataA1")!=.25||ability.Number("DataB1")!=1||
                    ability.Text("targs1")!="air,ground,enemy,organic,neutral")
                    throw new InvalidOperationException("item-finger-native-declaration-conflict");
                // SUMSCRIPT3 diagnoses only the exact own A14K book's
                // CHANNEL/FINISH/ENDCAST. It has no extra SPELL_EFFECT.
                cooldownGroup=finger.cooldownId;return;
            }
            if(itemId!="I00Z")throw new ArgumentException("Unsupported source summon item.");
            targetMode=OriginalAbilityTargetMode.Point;
            var item=items.Item(itemId);var a=combat.Ability(abilityId);var unit=combat.Unit("n01S");
            if(item==null||Array.IndexOf(item.abilityIds,abilityId)<0||a.Text("code")!="AUin"||a.Number("levels")!=1||
                a.Number("Cost1")!=manaCost||a.Number("Cool1")!=cooldown||a.Number("Rng1")!=range||a.Number("Area1")!=220||
                a.Number("DataB1")!=60||a.Number("DataC1")!=1||a.Number("Dur1")!=2.7||a.Number("HeroDur1")!=2.7||
                a.Text("UnitID1")!="n01S"||a.Text("targs1")!="ground,structure,debris,enemy,neutral"||
                unit.Number("HP")!=1800||unit.Number("spd")!=320||unit.Number("collision")!=32)
                throw new InvalidOperationException("item-orb-native-declaration-conflict");
            // SUMSCRIPT2 fea96e8875fa: absent DataA1 gives native zero events,
            // not the scripted W7 300. Reject a new explicit conflicting value.
            bool present=Array.Exists(a.fields,f=>f.key=="DataA1")||Array.Exists(a.overrides,f=>f.field=="Uin1"&&f.level==1);
            if(present&&(!a.TryNumber("DataA1",out double value,out _)||value!=0))
                throw new InvalidOperationException("item-orb-native-impact-conflict");
            cooldownGroup=item.cooldownId;
        }
    }
}

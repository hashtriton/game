using System;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedItemWard
    {
        public string itemId,unitId,auraId;
        public bool known,healthRegenFractionKnown,manaStableRateKnown;
        public double health,mana,moveSpeed,armor,duration,lastAliveSeconds,firstDeadSeconds,healthRegenFraction;
    }
    [Serializable] public sealed class OriginalObservedItemWards
    {
        public int schemaVersion;
        public string mapSha256,engineVersion;
        public OriginalObservedItemSource source;
        public OriginalObservedItemWard[] wards;
    }
    public sealed partial class OriginalItemActiveRules
    {
        void BuildWards(OriginalItemCatalog items,OriginalCombatCatalog combat,OriginalObservedItemActives active)
        {
            var data=active.wardObservations;if(data==null)return;
            var source=data.source;
            Check(data.schemaVersion==1 && data.mapSha256==items.mapSha256 && data.engineVersion=="1.26.0.6401" &&
                source!=null && source.cacheName=="LiAItemWard2.w3v" &&
                source.cacheSha256=="b27eb9dffab9d081a2d4d032c2cc635fe9c1643e8fa39d6ccd5c77bf7d62c3bd" &&
                source.probeMapSha256=="ba58069eb80e153e39b941709ea92303a5bb1c5e276f04bb860e9bb4de86391f" &&
                source.probeScriptSha256=="61401c3ab8355e9aed8dffb1d92d46fe819139975d96071b047150954a299332" &&
                source.complete && source.records==2 && source.passed==2 && source.failed==0 && data.wards?.Length==2,
                "Incomplete native ward evidence.");
            for(int i=0;i<2;i++)
            {
                string id=i==0?"I021":"I094", hidden=i==0?"I0A1":"I0A2", unit=i==0?"ohwd":"o00J";
                string abilityId=i==0?"A0UK":"A0UL",nativeId=i==0?"AIhw":"A0PV",aura=i==0?"Aoar":"A0PU";
                double duration=i==0?30:15;
                var row=data.wards[i];var use=Array.Find(active.items,x=>x.itemId==id);
                var item=items.Item(id);var ability=combat.Ability(abilityId);var summon=combat.Ability(nativeId);
                Check(row!=null && row.known && row.itemId==hidden && row.unitId==unit && row.auraId==aura &&
                    row.health==5 && row.mana==0 && row.moveSpeed==0 && row.armor==0 && row.duration==duration &&
                    row.lastAliveSeconds>duration-.2 && row.lastAliveSeconds<duration &&
                    row.firstDeadSeconds>=duration-.01 && row.firstDeadSeconds<duration+.11 &&
                    row.healthRegenFractionKnown==(i==0) && !row.manaStableRateKnown,
                    "Ward native profile or lifetime differs.");
                Check(use!=null && use.abilityId==abilityId && use.nativeUseObserved && use.retired && use.mode==2 &&
                    use.effectSeconds==0 && use.manaCost==0 && item.cooldownId==abilityId && Array.IndexOf(item.abilityIds,abilityId)>=0 &&
                    ability.Text("code")=="ANcl" && ability.Number("Cool1")==1 && ability.Number("Rng1")==500 &&
                    summon.Text("code")=="Ahwd" && summon.Text("UnitID1")==unit && summon.Number("Dur1")==duration &&
                    combat.Unit(unit).Number("HP")==5, "Ward source identity differs.");
                var cost=Array.Find(ability.fields,x=>x.key=="Cost1");
                Check(cost==null && !Array.Exists(ability.overrides,x=>x.field=="amcs" && x.level==1) ||
                    ability.TryNumber("Cost1",out double declared,out _) && declared==0,"Ward mana cost conflict.");
                summons.Add(id,new OriginalItemSummonRule {itemId=id,abilityId=abilityId,cooldownGroup=abilityId,
                    known=true,retired=true,manaCost=0,cooldown=1,duration=duration,units=new[]{unit},
                    pointTarget=true,castRange=ability.Number("Rng1"),
                    // Acv37261..85 uses spell coordinates; matching item target
                    // selects the caster position. Private placement remains
                    // reconstructed by the shared free-spawn search, radius16.
                    profiles=new[]{new OriginalWorldUnitProfile{maxHealth=5,maxMana=0,moveSpeed=0,collisionRadius=16}},
                    limitation=i==0?"Source point/item-self placement; radius and aura membership are declared/derived.":
                        "A0PU uses declared3% maxMP/s. Native refresh sometimes doubles this rate; exact scheduling unresolved."});
            }
        }
    }
}

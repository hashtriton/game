using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // Authored native-family parameters; host reconstruction uses .5s
        // acquisition, no linger, strongest signed value per buff/axis and
        // addition across distinct buffs. Missing percent flags are explicitly
        // interpreted as flat; missing optional movement is zero. These are
        // playable sparse-data policies, not measured engine defaults.
        readonly Dictionary<int, Dictionary<string, double>> itemAuraAmounts = new Dictionary<int, Dictionary<string, double>>();
        double nextItemAuraScan;
        double nextDawnRestoration = 1;
        double ItemAuraAxis(int id, string axis)
        {
            if (!itemAuraAmounts.TryGetValue(id, out var values)) return 0;
            return values.TryGetValue(axis, out double value) ? value : 0;
        }
        double ItemAuraArmorFlat(int id) => ItemAuraAxis(id,"armor");
        double ItemAuraArmorFraction(int id) => ItemAuraAxis(id,"armorPercent");
        double ItemAuraMovementBonus(int id) => ItemAuraAxis(id,"movement");
        double ItemAuraAttackSpeedBonus(int id) => ItemAuraAxis(id,"attackSpeed");
        double ItemAuraHealthRegen(int id,double maximum) => ItemAuraAxis(id,"health")+maximum*ItemAuraAxis(id,"healthPercent");
        double ItemAuraManaRegen(int id,double maximum) => ItemAuraAxis(id,"mana")+maximum*ItemAuraAxis(id,"manaPercent");
        double ItemAuraDamageBonus(int id,int weapon,double white)
        {
            var actor=world.UnitState(id); if(actor==null) return 0;
            string suffix=combatCatalog.Unit(actor.rawcode).Text("weapTp"+weapon)=="normal"?"Melee":"Ranged";
            return white*ItemAuraAxis(id,"damage"+suffix);
        }
        static bool AuraOptional(OriginalCombatDefinition ability,string key,out double value)
        {
            if(ability.TryNumber(key,out value,out string evidence))return true;
            // A present conflicting/invalid declaration is never a zero.
            value=0;return evidence==null;
        }
        Dictionary<string,double> ItemAuraAxes(OriginalCombatDefinition ability,int rank)
        {
            string n=rank.ToString(System.Globalization.CultureInfo.InvariantCulture),code=ability.Text("code");
            var result=new Dictionary<string,double>(); double a,b,c;
            // yQ:7339 checks B0CH once per second and directly restores both
            // maxima fractions. A1CS's absent native Hab1 is not a mana rate.
            if(ability.id=="A1CS" && rank==1 && code=="AHab" && ability.Text("BuffID1")=="B0CH")
            { result["dawnRestoration"]=.005; return result; }
            switch(code)
            {
                case "AHad": case "AHab": case "Aoar": case "Aarm":
                    if(!ability.TryNumber("DataA"+n,out a,out _) || !AuraOptional(ability,"DataB"+n,out b) || (b!=0&&b!=1))return result;
                    // A0PU ITEMWARD2 initially agrees with3% maximum mana,
                    // then native refresh can double it. Use the declaration
                    // as an explicit approximation, without inventing timings.
                    result[(code=="AHad"?"armor":code=="AHab"||code=="Aarm"?"mana":"health")+(b==1?"Percent":"")]=a;break;
                case "AUau":
                    if(!AuraOptional(ability,"DataA"+n,out a) || !AuraOptional(ability,"DataB"+n,out b) ||
                        !AuraOptional(ability,"DataC"+n,out c) || (c!=0&&c!=1))return result;
                    result["movement"]=a;result[c==1?"healthPercent":"health"]=b;break;
                case "AOae":
                    if(!AuraOptional(ability,"DataA"+n,out a) || !AuraOptional(ability,"DataB"+n,out b))return result;
                    result["movement"]=a;result["attackSpeed"]=b;break;
                case "Aakb":
                    // Akb1 is a fraction; Ear2/Ear3 enable melee/ranged.
                    if(!ability.TryNumber("DataA"+n,out a,out _) || !AuraOptional(ability,"DataB"+n,out b) ||
                        !AuraOptional(ability,"DataC"+n,out c))return result;
                    if(b==1)result["damageMelee"]=a;if(c==1)result["damageRanged"]=a;break;
            }
            return result;
        }
        void AdvanceItemAuras()
        {
            if(world==null || itemEffects==null)return;
            if(world.Clock+1e-9<nextItemAuraScan){AdvanceDawnRestoration();return;}
            nextItemAuraScan=world.Clock+.5;
            var units=world.Snapshot().units;
            var grouped=new Dictionary<int,Dictionary<string,(double positive,double negative)>>();
            foreach(var emitter in units)
            {
                if(emitter.hidden || emitter.health<=.405)continue;
                var sources=new List<OriginalItemNativeCombatAbility>(ItemNativeCombatAbilities(emitter.entityId));
                if(emitter.kind==OriginalWorldUnitKind.Summon && itemSummons.ContainsKey(emitter.entityId) &&
                    (emitter.rawcode=="ohwd"||emitter.rawcode=="o00J"))
                    foreach(var ability in NativeUnitAbilities(emitter))
                        sources.Add(new OriginalItemNativeCombatAbility{abilityId=ability.id,rank=1});
                foreach(var row in sources)
                {
                    var ability=combatCatalog.Ability(row.abilityId); if(ability==null)continue;
                    var axes=ItemAuraAxes(ability,row.rank); if(axes.Count==0)continue;
                    string buff=ability.Text("BuffID"+row.rank);if(string.IsNullOrEmpty(buff)||buff=="_")buff=row.abilityId;
                    foreach(var target in units)
                    {
                        if(!ItemAuraRecipient(emitter,target,ability,row.rank))continue;
                        if(!grouped.TryGetValue(target.entityId,out var values))
                            grouped[target.entityId]=values=new Dictionary<string,(double,double)>();
                        foreach(var axis in axes)
                        {
                            string key=buff+":"+axis.Key;values.TryGetValue(key,out var prior);
                            values[key]=(Math.Max(prior.positive,axis.Value),Math.Min(prior.negative,axis.Value));
                        }
                    }
                }
            }
            var next=new Dictionary<int,Dictionary<string,double>>();
            foreach(var unit in grouped)
            {
                var amounts=new Dictionary<string,double>();next.Add(unit.Key,amounts);
                foreach(var row in unit.Value)
                { string axis=row.Key.Substring(row.Key.IndexOf(':')+1);amounts.TryGetValue(axis,out double old);amounts[axis]=old+row.Value.positive+row.Value.negative; }
            }
            var affected=new HashSet<int>(itemAuraAmounts.Keys);affected.UnionWith(next.Keys);
            var oldMove=new Dictionary<int,double>();var oldRate=new Dictionary<int,double>();
            foreach(int id in affected)
            {
                oldMove[id]=ItemAuraMovementBonus(id);oldRate[id]=ItemAuraAttackSpeedBonus(id);
                if(next.TryGetValue(id,out var amounts) && amounts.TryGetValue("movement",out double move) && move!=0)
                    CaptureAbilityMovementBase(id);
            }
            itemAuraAmounts.Clear();foreach(var row in next)itemAuraAmounts.Add(row.Key,row.Value);
            foreach(int id in affected)
            {
                if(world.UnitState(id)==null){ReleaseAbilityMovementBase(id);continue;}
                if(oldMove[id]!=ItemAuraMovementBonus(id))RefreshAbilityMovement(id);
                if(oldRate[id]!=ItemAuraAttackSpeedBonus(id))RescaleWeaponRate(id,world.Clock);
                ReleaseAbilityMovementBase(id);
            }
            AdvanceDawnRestoration();
        }
        void AdvanceDawnRestoration()
        {
            if(world.Clock+1e-9<nextDawnRestoration)return;
            // Source global ke timer is periodic1s. Its phase relative to
            // startup is reconstructed on the host clock, with aura membership
            // from the shared scan above. Direct setters also affect paused units.
            nextDawnRestoration=Math.Floor(world.Clock+1e-9)+1;
            foreach(var unit in world.Snapshot().units)
            {
                if(unit.health<=.405 || ItemAuraAxis(unit.entityId,"dawnRestoration")<=0)continue;
                world.UpdateProfile(unit.entityId,unit.profile,
                    Math.Min(unit.profile.maxHealth,unit.health+unit.profile.maxHealth*.005),
                    Math.Min(unit.profile.maxMana,unit.mana+unit.profile.maxMana*.005));
            }
        }
    }
}

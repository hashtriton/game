using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemAxeState { internal double nextCheck; }
        readonly Dictionary<int,ItemAxeState> itemAxes=new Dictionary<int,ItemAxeState>();
        readonly HashSet<int> itemAxeRankReset=new HashSet<int>();

        NativeItemActionRule ItemAxeRule()
        {
            var item=itemCatalog.Item("I07C");var a=combatCatalog.Ability("A158");var buff=combatCatalog.Ability("A159");
            if(Array.IndexOf(item.abilityIds,"A158")<0||item.cooldownId!="A0UD"||a.Text("code")!="ACtc"||a.Number("Cool1")!=6||
                buff.Text("code")!="ANso"||buff.Text("BuffID1")!="B0A1"||buff.Number("Dur1")!=666||buff.Number("HeroDur1")!=666||
                buff.Number("DataB1")!=1||buff.Number("DataD1")!=-.2||buff.Number("DataE1")!=-2.75)
                throw new InvalidOperationException("Axe native declaration changed.");
            bool hasCost=Array.Exists(a.fields,f=>f.key=="Cost1")||Array.Exists(a.overrides,f=>f.field=="amcs"&&f.level==1);
            if(hasCost&&(!a.TryNumber("Cost1",out double cost,out _)||cost!=0))throw new InvalidOperationException("A158 measured zero cost conflicts with declaration.");
            foreach(string field in new[]{"DataA1","DataC1"})
                if(Array.Exists(buff.fields,f=>f.key==field)&&(!buff.TryNumber(field,out double value,out _)||value!=0))
                    throw new InvalidOperationException("A159 sparse damage field conflicts with measured profile.");
            // ITEMAXE1 native A158 five stages+USE at one time, MP325 unchanged.
            return new NativeItemActionRule{abilityId="A158",cooldownGroup=item.cooldownId,requiresCharge=false,cooldown=6,sourceEffect="axe-toggle"};
        }
        bool HasItemAxeBuff(int actor)=>itemScriptDebuffs.TryGetValue(actor,out var buffs)&&buffs.ContainsKey("B0A1");
        void ApplyItemAxeToggle(int actor)
        {
            if(HasItemAxeBuff(actor))RemoveItemAxeBuff(actor);else BeginItemAxeBuff(actor);
        }

        // Source Nt/At/Bt, 3.9c rawcodes.j:9122..9188. This seam represents
        // an admitted B0A1. ITEMAXE1 verifies A159's exact native axes separately
        // from the A158 item activation; the original Nt handler was absent.
        void BeginItemAxeBuff(int actor)
        {
            var unit=world.UnitState(actor);
            if(unit==null||unit.health<=.405)return;
            itemAxes[actor]=new ItemAxeState{nextCheck=world.Clock+1};
            if(unit.invulnerable||CasterMagicImmune(unit))return;
            // Measured +20%MS/+275%IAS and cast-only disable, periodic native
            // zero at+.01 then each1s. Duration666 is declared, not observed;
            // paused lifetime, refresh, mixed sums and allowing item-use while
            // Cast alone is blocked use the shared host policy. Sparse DataC1
            // is not asserted0: only the observed unscaled weapon subset is used.
            CaptureAbilityMovementBase(actor);
            if(!itemScriptDebuffs.TryGetValue(actor,out var buffs))itemScriptDebuffs[actor]=buffs=new Dictionary<string,ItemScriptDebuff>();
            buffs["B0A1"]=new ItemScriptDebuff{ability="A159",updated=world.Clock,remaining=666,move=.2,slow=-2.75,pulse=.01,owner=unit.ownerSlot};
            SetActorControl(actor,"item-soulburn:B0A1",OriginalActorControlMask.Cast,0,true,true);
            RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);
        }
        void RemoveItemAxeBuff(int actor)
        {
            itemAxes.Remove(actor);
            if(itemScriptDebuffs.TryGetValue(actor,out var buffs)&&buffs.Remove("B0A1"))
            {
                if(buffs.Count==0)itemScriptDebuffs.Remove(actor);
                ClearActorControl(actor,"item-soulburn:B0A1");
                RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);ReleaseAbilityMovementBase(actor);
            }
            // The source only sets A0JR to1. No call sets it to2 on activation.
            // SetUnitAbilityLevel affects an existing ability, not future items.
            var unit=world.UnitState(actor);
            if(unit!=null&&HasItemAxe(unit))itemAxeRankReset.Add(actor);
        }
        int ItemAxeAbilityRank(int actor,string ability,int rank) =>
            ability=="A0JR"&&itemAxeRankReset.Contains(actor)?1:rank;
        void SyncItemAxeInventory(int actor,OriginalInventory candidate)
        {
            if(!Array.Exists(candidate.HeroSlots,item=>item?.itemId=="I07C"))itemAxeRankReset.Remove(actor);
        }
        bool HasItemAxe(OriginalWorldUnitView unit)
        {
            if(unit.kind!=OriginalWorldUnitKind.Hero||unit.ownerSlot<1||unit.entityId!=OriginalWorld.HeroEntityId(unit.ownerSlot))return false;
            var inventory=players.Find(p=>p.slot==unit.ownerSlot)?.inventory;
            if(inventory==null)return false;
            foreach(var item in inventory.HeroSlots)if(item?.itemId=="I07C")return true;
            return false;
        }
        void AdvanceItemAxes()
        {
            foreach(var pair in new List<KeyValuePair<int,ItemAxeState>>(itemAxes))
                while(itemAxes.ContainsKey(pair.Key)&&pair.Value.nextCheck<=world.Clock+1e-9)
                {
                    pair.Value.nextCheck+=1;
                    var unit=world.UnitState(pair.Key);
                    // This script timer is not paused by PauseUnit.
                    if(unit==null||!HasItemAxeBuff(pair.Key)||unit.health<=unit.profile.maxHealth*.25)
                    {RemoveItemAxeBuff(pair.Key);break;}
                }
            foreach(int actor in new List<int>(itemAxeRankReset))
            {var unit=world.UnitState(actor);if(unit==null||!HasItemAxe(unit))itemAxeRankReset.Remove(actor);}
        }
        void ObserveItemAxeWeapon(int actor,OriginalWorldUnitView target,double eventDamage,bool primary)
        {
            var source=world.UnitState(actor);
            // DW:12815 uses gW's positive enemy weapon-event gate and actual
            // item residency. Mapping that event to primaryWeapon is the shared
            // host policy; the nested UnitDamageTarget is not another weapon.
            if(!primary||eventDamage<=0||source==null||!HasItemAxe(source)||target==null||
                !AreEnemies(source.ownerSlot,target.ownerSlot)||!HasItemAxeBuff(actor))return;
            if(source.health<=source.profile.maxHealth*.25){RemoveItemAxeBuff(actor);return;}
            int owner=source.ownerSlot,targetId=target.entityId;
            ApplyTriggeredHit(actor,owner,source,source.profile.maxHealth*.12,OriginalTriggeredDamageMode.ChaosUniversal);
            target=world.UnitState(targetId);
            if(target==null||target.health<=.405||target.invulnerable)return;
            // Bt passes the original resolved event Ct as new raw HERO/NORMAL
            // damage. It is not hL and gains no item spell power/magic vamp.
            // Native attack=true/ranged=false receives physical defenses again;
            // exact interactions with unrelated proc/block families are derived.
            var definition=combatCatalog.Unit(target.rawcode);
            double armor=UsesHeroCombatStats(target)?CombatStatsFor(target).armor:EnemyArmor(definition,targetId);
            double damage=OriginalAttackRules.WeaponDamage(native,IncomingItemWeaponDamage(target,eventDamage,true),"hero",definition.Text("defType"),armor);
            damage=IncomingInnateWeaponDamage(target,damage);
            damage=ApplyAbilityIncomingWeaponDamage(target,damage,"hero");
            ApplyResolvedUnitHit(actor,owner,target,damage);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int,double> pendingItemWindWalkStrikes=new Dictionary<int,double>();
        sealed class WidowArmor { internal int rank,remaining; internal double next; }
        sealed class WidowSweep { internal int actor; internal float remaining=8; internal double next; internal readonly HashSet<int> seen=new HashSet<int>(); }
        readonly Dictionary<int,WidowArmor> widowArmor=new Dictionary<int,WidowArmor>();
        readonly List<WidowSweep> widowSweeps=new List<WidowSweep>();

        NativeItemActionRule WindWalkItemRule(string item)
        {
            bool widow=item=="I013";string id=widow?"A01X":"A19K",buff=widow?"B031":"B0BB";
            var a=combatCatalog.Ability(id);var i=itemCatalog.Item(item);
            double duration=widow?8:6,cooldown=widow?16:18,movement=widow?.3:.1,strike=widow?100:50;
            if(i==null||Array.IndexOf(i.abilityIds,id)<0||a?.Text("code")!="AOwk"||a.Number("levels")!=1||
                a.Number("DataA1")!=.3||a.Number("DataB1")!=movement||a.Number("DataC1")!=strike||a.Number("DataD1")!=1||
                a.Number("Cost1")!=70||a.Number("Cool1")!=cooldown||a.Number("Dur1")!=duration||a.Number("HeroDur1")!=duration||a.Text("BuffID1")!=buff)
                throw new InvalidOperationException("Wind Walk item declaration changed.");
            if(widow)
            {
                var armor=combatCatalog.Ability("A14N");
                if(armor?.Text("code")!="AIde"||armor.Number("levels")!=5||combatCatalog.Ability("A14M")?.Text("BuffID1")!="B09W")
                    throw new InvalidOperationException("Widow armor identity changed.");
                for(int rank=1;rank<=5;rank++)if(armor.Number("DataA"+rank)!=-4*rank)
                    throw new InvalidOperationException("Widow armor rank changed.");
            }
            // Authored AOwk fields. Instant activation, stop-order, reveal and
            // release-time strike arbitration are explicit host family policies;
            // no per-alias native interruption/RNG proof is claimed.
            return new NativeItemActionRule{abilityId=id,cooldownGroup=i.cooldownId,status=buff,duration=duration,fade=.3,
                cooldown=cooldown,manaCost=70,movement=movement,strike=strike,requiresCharge=false};
        }
        void ArmItemWindWalkStrike(int actor)
        {
            if(ItemInvisibilityActive(actor)&&itemInvisibility.TryGetValue(actor,out var status)&&status.strike>0)
                pendingItemWindWalkStrikes[actor]=status.strike;
        }
        double ConsumeItemWindWalkStrike(int actor)
        {
            if(!pendingItemWindWalkStrikes.TryGetValue(actor,out var value))return 0;
            pendingItemWindWalkStrikes.Remove(actor);return value;
        }
        double WidowArmorBonus(int actor)
        {
            if(!widowArmor.TryGetValue(actor,out var status))return 0;
            // A14N has five declared levels, including binary -16/-20 at4/5.
            // SetUnitAbilityLevel requests above the maximum saturate in this
            // host policy; out-of-range binary cells do not create new levels.
            return combatCatalog.Ability("A14N").Number("DataA"+Math.Min(5,status.rank));
        }
        void AddWidowArmor(int actor)
        {
            if(widowArmor.TryGetValue(actor,out var status)){status.rank=Math.Min(5,status.rank+1);status.remaining=6;}
            else widowArmor[actor]=new WidowArmor{rank=1,remaining=6,next=world.Clock+1};
        }
        void ObserveWidowWeapon(int actor,OriginalWorldUnitView target,double damage,bool primary)
        {
            var source=world.UnitState(actor);
            if(!primary||damage<=0||source==null||source.kind!=OriginalWorldUnitKind.Hero||source.ownerSlot<1||
                actor!=OriginalWorld.HeroEntityId(source.ownerSlot)||target==null||!AreEnemies(source.ownerSlot,target.ownerSlot))return;
            var inventory=players.Find(p=>p.slot==source.ownerSlot)?.inventory;
            if(inventory==null)return;
            foreach(var item in inventory.HeroSlots)if(item?.itemId=="I013"){AddWidowArmor(target.entityId);break;}
        }
        void BeginWidowSweep(int actor)=>widowSweeps.Add(new WidowSweep{actor=actor,next=world.Clock+.1});
        void AdvanceWidowEffects()
        {
            foreach(int actor in new List<int>(pendingItemWindWalkStrikes.Keys))
            {var unit=world.UnitState(actor);if(unit==null||unit.health<=.405)pendingItemWindWalkStrikes.Remove(actor);}
            // rW/oW12682: reset the6 counter without resetting its1s phase;
            // cleanup tests zero BEFORE decrement, independent of PauseUnit.
            foreach(var pair in new List<KeyValuePair<int,WidowArmor>>(widowArmor))
                while(widowArmor.ContainsKey(pair.Key)&&world.Clock+1e-9>=pair.Value.next)
                {
                    pair.Value.next+=1;var unit=world.UnitState(pair.Key);
                    if(unit==null||unit.health<=.405||pair.Value.remaining==0){widowArmor.Remove(pair.Key);break;}
                    pair.Value.remaining--;
                }
            // nW/aW/iW12719: each cast retains its own victim group. Revealing,
            // dropping the boots or caster death does not stop this script timer.
            foreach(var sweep in new List<WidowSweep>(widowSweeps))
                while(widowSweeps.Contains(sweep)&&world.Clock+1e-9>=sweep.next)
                {
                    sweep.next+=.1;sweep.remaining-=.1f;var source=world.UnitState(sweep.actor);
                    if(sweep.remaining<=0||source==null){widowSweeps.Remove(sweep);break;}
                    foreach(var target in world.Snapshot().units)
                        if(target.health>.405&&AreEnemies(source.ownerSlot,target.ownerSlot)&&!CasterHasType(target,"structure")&&
                            !Array.Exists((combatCatalog.Unit(target.rawcode).Text("targType")??"").Split(','),x=>string.Equals(x.Trim(),"structure",StringComparison.OrdinalIgnoreCase))&&
                            CanSeeForCombat(source.ownerSlot,target)&&SquaredDistance(source.position,target.position)<=100*100&&sweep.seen.Add(target.entityId))
                            AddWidowArmor(target.entityId);
                }
        }
    }
}

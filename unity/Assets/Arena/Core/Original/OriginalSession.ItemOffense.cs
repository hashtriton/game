using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ChargeBladeState { internal int charge; internal bool held; internal OriginalPoint previous; }
        sealed class ItemFireWave
        {
            internal int actor,owner;
            internal double due,angle,remaining=600,radius=150;
            internal OriginalPoint point;
            internal readonly HashSet<int> visited=new HashSet<int>();
        }
        readonly Dictionary<int,ChargeBladeState> chargeBlades=new Dictionary<int,ChargeBladeState>();
        readonly List<ItemFireWave> itemFireWaves=new List<ItemFireWave>();
        double nextChargeBladeTick=.2;
        bool sourceChainDamageRunning;

        void SyncChargeBladeInventory(int actorId,OriginalInventory inventory)
        {
            var actor=world?.UnitState(actorId);if(actor==null||inventory==null)return;
            if(!chargeBlades.TryGetValue(actorId,out var state))chargeBlades[actorId]=state=new ChargeBladeState();
            bool held=Array.Exists(inventory.HeroSlots,i=>i?.itemId=="I07Y");
            if(held&&!state.held)state.previous=actor.position;
            state.held=held;
            for(int i=0;i<6;i++)
                if(inventory.HeroSlots[i]?.itemId=="I07Y")
                    inventory.SetScriptCharges(OriginalInventoryBag.Hero,i,inventory.HeroSlots[i].instanceId,state.charge);
        }
        void PublishChargeBlade(int actorId)
        {
            var actor=world.UnitState(actorId);
            if(actor?.kind==OriginalWorldUnitKind.Hero)SyncChargeBladeInventory(actorId,PlayerAt(actor.ownerSlot).inventory);
        }
        void ObserveChargeBladeWeapon(int actorId,OriginalWorldUnitView target,double damage,bool primaryWeapon)
        {
            // AW requires a positive primary-weapon event and enemy target;
            // kQ/CQ use De to prevent chains from charging/proccing themselves.
            if(!primaryWeapon||damage<=0||sourceChainDamageRunning||target==null||
                !chargeBlades.TryGetValue(actorId,out var state)||!state.held||state.charge<150)return;
            var actor=world.UnitState(actorId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero||!AreEnemies(actor.ownerSlot,target.ownerSlot))return;
            int amount=state.charge;state.charge=0;PublishChargeBlade(actorId);
            BeginItemSourceChain(actorId,target.entityId,amount,false,true,"A028");
        }
        bool BeginItemOffense(int actorId,string ability,int targetId,OriginalPoint point)
        {
            var actor=world.UnitState(actorId);
            if(ability=="A028")
            {
                var target=world.UnitState(targetId);
                if(target==null||HasEffectiveUnitAbility(target,"B06X")||sourceChainDamageRunning)return true;
                bool charged=chargeBlades.TryGetValue(actorId,out var state)&&state.held;
                double amount=charged?state.charge:350;
                if(charged){state.charge=0;PublishChargeBlade(actorId);}
                BeginItemSourceChain(actorId,targetId,amount,false,charged,"A028");return true;
            }
            if(ability=="A08A")
            {
                // l8 -> jZ/hZ: expanding150..200 radius,900 speed,.03 tick,
                // 600 damage, original post-move remaining-distance check.
                itemFireWaves.Add(new ItemFireWave{actor=actorId,owner=actor.ownerSlot,point=actor.position,
                    angle=Math.Atan2(point.y-actor.position.y,point.x-actor.position.x),due=world.Clock+.03});
                return true;
            }
            return false;
        }
        int ApplyItemShockNative(int actorId,OriginalPoint point)
        {
            var actor=world.UnitState(actorId);if(actor==null)return 0;
            var ability=combatCatalog.Ability("A08A");
            double length=ability.Number("DataC1"),start=ability.Number("Area1"),end=ability.Number("DataD1");
            double angle=Math.Atan2(point.y-actor.position.y,point.x-actor.position.x),dx=Math.Cos(angle),dy=Math.Sin(angle);
            int count=0;
            // ITEMCHAIN1 observes the zero callback at EFFECT on a ground
            // enemy200WC away. The swept capsule700/150..200 is a declared
            // geometry reconstruction; wider native geometry was not measured.
            foreach(var target in CasterUnits())
            {
                if(target.health<=.405||target.hidden||target.invulnerable||CasterMagicImmune(target)||
                    !AreEnemies(actor.ownerSlot,target.ownerSlot)||
                    !WeaponTargetTypeAllowed("ground,structure",combatCatalog.Unit(target.rawcode).Text("targType")))continue;
                double x=target.position.x-actor.position.x,y=target.position.y-actor.position.y;
                double along=Math.Max(0,Math.Min(length,x*dx+y*dy));
                double radius=start+(end-start)*along/length;
                double distanceX=x-along*dx,distanceY=y-along*dy;
                if(distanceX*distanceX+distanceY*distanceY>radius*radius)continue;
                ApplyNativeTriggeredHit(actorId,actor.ownerSlot,target,0,OriginalTriggeredDamageMode.SpellMagic);count++;
            }
            return count;
        }
        void AdvanceItemOffense()
        {
            while(nextChargeBladeTick<=world.Clock+1e-9)
            {
                nextChargeBladeTick+=.2;
                foreach(var pair in chargeBlades)
                {
                    var state=pair.Value;var actor=world.UnitState(pair.Key);if(!state.held||actor==null)continue;
                    int gain=(int)(.15*Math.Sqrt(SquaredDistance(state.previous,actor.position)));
                    if(gain>0&&gain<180&&state.charge<350){state.charge=Math.Min(350,state.charge+gain);PublishChargeBlade(pair.Key);}
                    state.previous=actor.position;
                }
            }
            foreach(var wave in itemFireWaves.ToArray())
                while(itemFireWaves.Contains(wave)&&wave.due<=world.Clock+1e-9)
                {
                    wave.due+=.03;double remaining=wave.remaining,radius=wave.radius;
                    wave.remaining-=27;wave.radius+=2.25;
                    wave.point=new OriginalPoint(wave.point.x+27*Math.Cos(wave.angle),wave.point.y+27*Math.Sin(wave.angle));
                    foreach(var target in CasterUnits())
                        if(target.health>.405&&AreEnemies(wave.owner,target.ownerSlot)&&!CasterHasType(target,"structure")&&
                            !wave.visited.Contains(target.entityId)&&SquaredDistance(wave.point,target.position)<=radius*radius)
                        {
                            wave.visited.Add(target.entityId);
                            if(!CasterMagicImmune(target))ApplyTriggeredHit(wave.actor,wave.owner,target,600,OriginalTriggeredDamageMode.SpellMagic);
                        }
                    if(remaining<=0){itemFireWaves.Remove(wave);ObserveScriptedHelperDeath();break;}
                }
        }
    }
}

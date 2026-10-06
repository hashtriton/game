using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int,int> disconnectedRetreatCycles=new Dictionary<int,int>();
        readonly Dictionary<int,long> disconnectedGroundGoals=new Dictionary<int,long>();
        bool AiPointOrder(Player player,OriginalPoint point,bool attack=false)=>ApplyWorldCommand(player,new OriginalSessionCommand{
            kind=attack?OriginalSessionCommandKind.AttackMove:OriginalSessionCommandKind.Move,x=point.x,y=point.y})==OriginalSessionReplyCode.Accepted;

        bool DisconnectedBossRegion(Player player)
        {
            // eHv's ordered Gd/sd/Ud/Zd branches. These read the same active
            // phase state as the source effects, rather than a new dodge AI.
            foreach(var phase in bossGhostPhases.Values)
            {AiPointOrder(player,new OriginalPoint(phase.odd?32:256,-3168));return true;}
            foreach(var phase in bossQuadrants.Values)
            {
                if(!phase.growing)
                {
                    OriginalPoint p=phase.safe==1?new OriginalPoint(432,-2272):phase.safe==2?new OriginalPoint(-432,-2272):
                        phase.safe==3?new OriginalPoint(-432,-3152):new OriginalPoint(432,-3152);
                    AiPointOrder(player,p);
                }
                return true;
            }
            foreach(var phase in bossWindPhases.Values)
            {
                bool wd=phase.callback>0&&phase.callback%2==0;
                double angle=(player.slot+(wd?0:1))*45*.0174532;
                AiPointOrder(player,new OriginalPoint(775*Math.Cos(angle),-2700+775*Math.Sin(angle)));return true;
            }
            if(bossBarrages.Count>0)
            {AiPointOrder(player,player.slot<=4?new OriginalPoint(-577,-2434):new OriginalPoint(-737,-2942));return true;}
            return false;
        }
        void DisconnectedCompanionOrder(Player player,OriginalWorldUnitView actor)
        {
            double angle=actor.facingDegrees*Math.PI/180;
            var point=new OriginalPoint(actor.position.x+600*Math.Cos(angle),actor.position.y+600*Math.Sin(angle));
            foreach(var ally in world.Snapshot().units)
                if(ally.entityId!=actor.entityId&&ally.ownerSlot==player.slot&&ally.health>.405&&!HasEffectiveUnitAbility(ally,"A0K4")&&
                    SquaredDistance(actor.position,ally.position)<=900*900)
                    ApplyWorldCommand(player,new OriginalSessionCommand{kind=OriginalSessionCommandKind.AttackMove,actorEntityId=ally.entityId,x=point.x,y=point.y});
        }
        void AdvanceDisconnectedBattleMovement(Player player,OriginalWorldUnitView actor)
        {
            bool boss=match.Round%5==0&&!DuelActive;
            disconnectedAttackCycles.TryGetValue(player.slot,out int cycle);
            disconnectedAttackCycles[player.slot]=(cycle+1)%5;
            if(!boss&&cycle==0)
            {
                var target=DisconnectedAiEnemy(actor,2500);
                if(target!=null)AiPointOrder(player,target.position,true);
                DisconnectedCompanionOrder(player,actor);
            }
            double fraction=actor.health/actor.profile.maxHealth;
            // Source subtracts360 degrees, not180; preserve the forward retreat
            // quirk instead of inventing a more capable flee strategy.
            if(!DuelActive&&fraction>.2&&fraction<.4)
            {
                disconnectedRetreatCycles.TryGetValue(player.slot,out int retreat);retreat++;
                if(retreat==1)
                {
                    double angle=(actor.facingDegrees-360)*.0174532;
                    AiPointOrder(player,new OriginalPoint(actor.position.x+350*Math.Cos(angle),actor.position.y+350*Math.Sin(angle)));
                }
                disconnectedRetreatCycles[player.slot]=retreat>3?0:retreat;
            }
            if(boss&&!DisconnectedBossRegion(player))
            {
                var target=DisconnectedAiEnemy(actor,1600);
                if(target!=null)AiPointOrder(player,target.position,true);
                DisconnectedCompanionOrder(player,actor);
            }
        }
        OriginalWorldUnitView AiItemTarget(OriginalWorldUnitView actor,int mode,double radius,double threshold)
        {
            foreach(var unit in world.Snapshot().units)
            {
                if(unit.health<=.405||unit.hidden||HasEffectiveUnitAbility(unit,"A0K4")||SquaredDistance(actor.position,unit.position)>radius*radius)continue;
                bool enemy=AreEnemies(actor.ownerSlot,unit.ownerSlot);
                if((mode==1||mode==3)&&enemy)return unit;
                if(mode==2&&!enemy&&IsNativeHeroPredicate(unit)&&unit.health/unit.profile.maxHealth<=threshold)return unit;
                if(mode==5&&!enemy&&unit.profile.maxMana>0&&unit.mana/unit.profile.maxMana<=threshold)return unit;
            }
            return null;
        }
        void UseDisconnectedSourceItems(Player player,OriginalWorldUnitView actor)
        {
            // efv23527..23580 / ocv86040..80: preserve the source selector
            // and UnitUseItem/Target/Point shape, then use the shared gate.
            foreach(var view in ItemUseViews(player))
            {
                if(view.bag!=OriginalInventoryBag.Hero||view.code!=OriginalItemUseCode.Ready)continue;
                int mode=0,order=1;double radius=0,threshold=1;
                switch(view.itemId)
                {
                    case "I09P":mode=1;order=2;radius=400;break;
                    case "I05A":case "I015":case "I07Y":mode=1;order=2;radius=700;break;
                    // ITEMPOINTUNIT1 accepts native Unit and Point A08A at the same XY.
                    // Source l8 consumes XY; bridge its enemy Unit order to the existing Point gate.
                    case "I02C":mode=1;order=3;radius=700;break;
                    case "I026":mode=1;order=2;radius=490;break;
                    case "I00Z":mode=1;order=3;radius=800;break;
                    case "I08I":case "I045":mode=1;order=3;radius=600;break;
                    case "I05P":
                        if(!DuelActive&&match.Round%5!=0)continue;
                        mode=1;order=3;radius=600;break;
                    case "I00T":mode=2;order=2;radius=700;threshold=.6;break;
                    case "I00V":mode=2;order=2;radius=700;threshold=.5;break;
                    case "I021":mode=3;order=3;radius=1500;break;
                    case "I060":mode=AiShoppingRandom()<.5?1:2;order=2;radius=900;threshold=.6;break;
                    case "I01R":mode=2;radius=100;threshold=.3;break;
                    case "I076":mode=2;radius=100;threshold=.8;break;
                    case "I02E":mode=2;radius=250;threshold=.6;break;
                    case "I03Z":mode=2;radius=250;threshold=.5;break;
                    case "I03S":mode=2;radius=250;threshold=.7;break;
                    case "I072":mode=2;radius=100;threshold=.5;break;
                    case "I05Q":mode=2;radius=100;threshold=.7;break;
                    case "I03L":mode=2;radius=100;threshold=.25;break;
                    case "I022":mode=2;radius=100;threshold=.35;break;
                    case "I07P":mode=2;order=2;radius=100;threshold=.8;break;
                    case "I08D":mode=2;order=2;radius=100;threshold=.75;break;
                    case "I03M":mode=5;radius=100;threshold=.3;break;
                    case "I023":mode=5;radius=100;threshold=.4;break;
                    case "I02G":mode=3;radius=1000;break;
                    case "I06R":mode=3;radius=500;break;
                    case "I03Y":mode=3;radius=300;break;
                    case "I07U":mode=3;radius=250;break;
                    case "I04B":mode=3;radius=300;break;
                    case "I02T":case "I07O":case "I07T":mode=3;radius=600;break;
                    case "I07E":case "I01M":case "I01A":mode=3;radius=1500;break;
                    case "I090":mode=3;radius=390;break;
                    case "I03N":mode=3;radius=325;break;
                    case "I070":mode=3;radius=700;break;
                }
                if(mode==0)continue;
                var target=AiItemTarget(actor,mode,radius,mode==1?1:threshold);
                if(target==null)continue;
                if(mode==3)target=actor;
                if((view.itemId=="I090"||view.itemId=="I03N")&&match.Round%5!=0&&!DuelActive)
                {
                    int count=0;foreach(var unit in world.Snapshot().units)
                        if(unit.health>.405&&!unit.hidden&&AreEnemies(actor.ownerSlot,unit.ownerSlot)&&!HasEffectiveUnitAbility(unit,"A0K4")&&
                            SquaredDistance(actor.position,unit.position)<=radius*radius)count++;
                    if(count<7)continue;
                }
                var command=new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,bag=view.bag,itemSlot=view.slot,itemInstanceId=view.instanceId};
                if(order==2){command.targetKind=OriginalWorldTargetKind.Unit;command.targetId=target.entityId;}
                else if(order==3){command.x=target.position.x;command.y=target.position.y;}
                ApplyCastCommand(player,command);
                actor=world.UnitState(actor.entityId);if(actor==null||actor.health<=.405)return;
            }
        }
        bool TryDisconnectedPickup(Player player,OriginalGroundItemView ground)
        {
            var actor=world.UnitState(player.slot);var candidate=player.inventory.Copy();
            if(actor==null||actor.health<=.405||!candidate.CanPickup(ground.item))return false;
            var powerup=PreparePowerupPickup(player,actor,candidate,ground.item.itemId);
            if(powerup.handled)
            {if(powerup.code!=OriginalItemActionCode.Success)return false;CommitPowerupPickup(powerup);}
            else if(!CommitAiInventory(player,candidate,actor,candidate.TryPickup(CopyItem(ground.item))))return false;
            groundItems.Remove(ground.item.instanceId);return true;
        }
        void DisconnectedPreparationPickup(Player player,OriginalWorldUnitView actor)
        {
            // ecv: preparation scans XV and directly gives only own items
            // within1000; native UnitAddItem has no walking/pickup-range order.
            foreach(var item in new List<OriginalGroundItemView>(groundItems.Values))
                if(item.item.ownerId==player.slot&&item.position.x>=-384&&item.position.x<=256&&item.position.y>=736&&item.position.y<=1248&&
                    SquaredDistance(actor.position,item.position)<=1000*1000)TryDisconnectedPickup(player,item);
        }
        static bool AiGroundEligible(string id)=>id=="rspd"||id=="rres"||id=="rsps"||id=="rhe2"||
            id=="tdex"||id=="tint"||id=="lmbr"||id=="gold";
        void DisconnectedCombatPickup(Player player,OriginalWorldUnitView actor)
        {
            if(DuelActive)return;
            bool boss=match.Round%5==0;
            var pool=new List<OriginalGroundItemView>();
            foreach(var ground in groundItems.Values)
            {
                var p=ground.position;
                if(boss?p.x>=-608&&p.x<=608&&p.y>=-3360&&p.y<=-2080:
                    p.x>=-1920&&p.x<=1664&&p.y>=-896&&p.y<=2560)pool.Add(ground);
            }
            if(pool.Count==0)return;
            pool.Sort((a,b)=>a.item.instanceId.CompareTo(b.item.instanceId));
            var picked=pool[(int)(AiShoppingRandom()*pool.Count)];
            // eHv selects an item BEFORE applying JL/range. Preserve failed
            // draws; do not bias the pool toward useful eligible pickups.
            double radius=boss?600:1200;
            if(!AiGroundEligible(picked.item.itemId)||SquaredDistance(actor.position,picked.position)>radius*radius)return;
            if(AiPointOrder(player,picked.position))disconnectedGroundGoals[player.slot]=picked.item.instanceId;
        }
        void AdvanceDisconnectedGroundPickup()
        {
            if(world==null||itemRules==null)return;
            foreach(var entry in new List<KeyValuePair<int,long>>(disconnectedGroundGoals))
            {
                var player=players.Find(p=>p.slot==entry.Key);var actor=world.UnitState(entry.Key);
                if(player==null||player.connected||actor==null||actor.health<=.405||
                    !groundItems.TryGetValue(entry.Value,out var ground))
                {disconnectedGroundGoals.Remove(entry.Key);continue;}
                // A replacement order cancels the simulated native smart-item
                // order. Arrival/pickup uses the shared native range gate.
                if(actor.order==OriginalWorldOrder.AttackTarget||actor.holding||
                    SquaredDistance(actor.destination,ground.position)>1e-8)
                {disconnectedGroundGoals.Remove(entry.Key);continue;}
                var range=native.Constant("PickupItemRange");
                if(!range.known||actor.paused||actor.hidden||ActorMoveBlocked(actor.entityId)||
                    SquaredDistance(actor.position,ground.position)>range.Require()*range.Require())continue;
                ApplyItemCommand(player,new OriginalSessionCommand{kind=OriginalSessionCommandKind.PickupItem,
                    bag=OriginalInventoryBag.Hero,itemInstanceId=entry.Value});
                disconnectedGroundGoals.Remove(entry.Key);
            }
        }
    }
}


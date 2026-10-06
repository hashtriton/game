using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemFortitude { internal double remaining,next; }
        sealed class AstralGuardian
        {
            internal bool active;
            internal int ticks;
            internal string inventory;
            internal OriginalPoint position,destination,beamEnd;
            internal double updated,beamUntil;
        }
        readonly Dictionary<int,ItemFortitude> itemFortitudes=new Dictionary<int,ItemFortitude>();
        readonly Dictionary<int,AstralGuardian> astralGuardians=new Dictionary<int,AstralGuardian>();
        readonly Dictionary<string,OriginalItemFortitudeRules> itemFortitudeRules=new Dictionary<string,OriginalItemFortitudeRules>();
        double nextAstralGuardianTick=1;
        OriginalItemFortitudeRules ItemFortitudeRule(string id)
        {
            if(id!="I072"&&id!="I05Q")return null;
            if(!itemFortitudeRules.TryGetValue(id,out var rule))itemFortitudeRules[id]=rule=new OriginalItemFortitudeRules(itemCatalog,combatCatalog,id);
            return rule;
        }
        OriginalSessionReplyCode UseItemFortitude(Player player,OriginalItemFortitudeRules rule)
        {
            // The retained item has no charges or scripted mana debit. Native
            // instant activation is a family transfer, not a measured alias.
            itemCooldowns[ItemCooldownKey(player.slot,rule.abilityId)]=ItemClock+rule.cooldown;
            player.lastItemAction=OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(player.slot,rule.abilityId);
            BeginItemFortitude(player.slot,rule.duration);
            return OriginalSessionReplyCode.Accepted;
        }
        void BeginItemFortitude(int actor,double duration)
        {
            if(world.UnitState(actor)==null)return;
            // Vq6355: refreshing6/8 resets the remaining counter; the
            // guardian's1 adds exactly1. Only initial entry removes debuffs.
            if(itemFortitudes.TryGetValue(actor,out var existing))
            {existing.remaining=duration==6||duration==8?duration:existing.remaining+1;return;}
            RemoveNegativeAbilityBuffs(actor);
            if(world.UnitState(actor)==null)return;
            // A18O's exact DataA1=A18P is in the source SLK35936, omitted
            // from the numeric combat projection. Grant its Amim leaf
            // explicitly. A18Q's unmeasured Aasl numeric axes remain absent.
            ApplyUnitAbilityOverlay(actor,new[]{"A18Q","A18O","A18P"},Array.Empty<string>());
            itemFortitudes[actor]=new ItemFortitude{remaining=duration,next=world.Clock+1};
        }
        void EndItemFortitude(int actor)
        {
            itemFortitudes.Remove(actor);
            if(world.UnitState(actor)!=null)ApplyUnitAbilityOverlay(actor,Array.Empty<string>(),new[]{"A18Q","A18O","A18P"});
        }
        void SyncAstralGuardian(int actor,OriginalInventory inventory)
        {
            var ids=new List<string>();
            if(inventory!=null)foreach(var item in inventory.HeroSlots)if(item?.itemId=="I05Q")ids.Add(item.instanceId.ToString());
            string signature=string.Join(",",ids);var unit=world?.UnitState(actor);
            if(!astralGuardians.TryGetValue(actor,out var guardian))
            {
                if(ids.Count==0||unit==null)return;
                guardian=new AstralGuardian{position=unit.position,destination=unit.position,updated=world.Clock};
                astralGuardians[actor]=guardian;
            }
            if(guardian.inventory==signature)return;
            // cq pauses/hides the retained helper when its last copy leaves;
            // Ee's partial six-tick counter survives a later pickup.
            guardian.inventory=signature;guardian.active=ids.Count>0;
            if(guardian.active&&unit!=null)guardian.position=guardian.destination=unit.position;
        }
        void AdvanceItemFortitude()
        {
            foreach(var pair in new List<KeyValuePair<int,ItemFortitude>>(itemFortitudes))
                while(itemFortitudes.TryGetValue(pair.Key,out var current)&&ReferenceEquals(current,pair.Value)&&world.Clock+1e-9>=current.next)
                {
                    current.next+=1;var actor=world.UnitState(pair.Key);
                    // nq tests zero/death BEFORE decrement. Six means six
                    // decrements and cleanup on callback7, even when paused.
                    if(current.remaining==0||actor==null||actor.health<.405){EndItemFortitude(pair.Key);break;}
                    current.remaining-=1;
                }
            foreach(var player in players)SyncAstralGuardian(player.slot,player.inventory);
            foreach(var guardian in astralGuardians.Values)
            {
                double elapsed=Math.Max(0,world.Clock-guardian.updated);guardian.updated=world.Clock;
                if(!guardian.active)continue;
                double distance=Math.Sqrt(SquaredDistance(guardian.position,guardian.destination));
                if(distance<=0)continue;double t=Math.Min(1,elapsed*300/distance);
                guardian.position=new OriginalPoint(guardian.position.x+(guardian.destination.x-guardian.position.x)*t,
                    guardian.position.y+(guardian.destination.y-guardian.position.y)*t);
            }
            // Bq is a global1s timer. The flying Aloc/Avul helper is modeled
            // as a noninteractive visual with declared300 speed; movement,
            // target enumeration and RNG sequence are explicit host policies.
            while(world.Clock+1e-9>=nextAstralGuardianTick)
            {
                nextAstralGuardianTick+=1;
                foreach(var pair in astralGuardians)
                {
                    var guardian=pair.Value;var actor=world.UnitState(pair.Key);
                    if(!guardian.active||actor==null)continue;
                    if(guardian.ticks<6)
                    {
                        guardian.ticks++;
                        if(SquaredDistance(guardian.position,actor.position)>700*700)guardian.position=guardian.destination=actor.position;
                        else
                        {
                            double angle=CasterRandomUnit()*6.2832,radius=CasterRandomUnit()*50;
                            guardian.destination=new OriginalPoint(actor.position.x+radius*Math.Cos(angle),actor.position.y+radius*Math.Sin(angle));
                        }
                    }
                    if(guardian.ticks<6)continue;guardian.ticks=0;
                    OriginalWorldUnitView selected=null;
                    foreach(var target in world.Snapshot().units)
                        if(SourceUnitUserData(target.entityId)!=2&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&target.health>.405&&
                            !CasterMagicImmune(target)&&!CasterHasType(target,"structure")&&SquaredDistance(guardian.position,target.position)<=700*700)
                            selected=target; // Source retains the last qualifying Group member.
                    if(selected!=null)
                    {
                        guardian.beamEnd=selected.position;guardian.beamUntil=world.Clock+.6;
                        ApplyTriggeredHit(actor.entityId,actor.ownerSlot,selected,300,OriginalTriggeredDamageMode.SpellMagic);
                    }
                    BeginItemFortitude(actor.entityId,1); // Also executes with no target or a dead holder.
                }
            }
        }
        void AppendItemFortitudeVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var pair in astralGuardians)
            {
                var g=pair.Value;if(!g.active)continue;
                output.Add(new OriginalVisualEffectView{kind=OriginalVisualEffectKind.Ghost,abilityId="A11Z",sourceEntityId=pair.Key,position=g.position,radius=30,progress=1});
                if(g.beamUntil>world.Clock)output.Add(new OriginalVisualEffectView{kind=OriginalVisualEffectKind.Beam,abilityId="A11Z",sourceEntityId=pair.Key,
                    position=g.position,end=g.beamEnd,radius=8,progress=Math.Max(0,1-(g.beamUntil-world.Clock)/.6)});
            }
        }
    }
}

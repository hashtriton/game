using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ErosAmulet
        {internal double pool,expires,due,damage;internal int source,pending;}
        readonly Dictionary<int,ErosAmulet> erosAmulets=new Dictionary<int,ErosAmulet>();
        bool erosReflection;
        void ChargeErosAmulets(int actorId,string ability)
        {
            var actor=world?.UnitState(actorId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero||!OriginalCasterFieldRules.SpellTriggersCurse(ability,actor.rawcode))return;
            var inventory=PlayerAt(actor.ownerSlot).inventory;
            for(int i=0;i<6;i++)
            {
                var item=inventory.HeroSlots[i];
                if(item?.itemId=="I05D"&&item.chargesKnown&&item.charges<10)
                    inventory.SetScriptCharges(OriginalInventoryBag.Hero,i,item.instanceId,item.charges+1);
            }
        }
        void BeginErosAmulet(int actorId)
        {
            var actor=world.UnitState(actorId);var inventory=PlayerAt(actor.ownerSlot).inventory;double pool=0;
            for(int i=0;i<6;i++)
            {
                var item=inventory.HeroSlots[i];
                if(item?.itemId!="I05D"||item.charges<=0)continue;
                pool=150+item.charges*100;
                inventory.SetScriptCharges(OriginalInventoryBag.Hero,i,item.instanceId,0);
            }
            if(pool<=0)return;
            ApplyUnitAbilityOverlay(actorId,new[]{"A0YN"},null);
            erosAmulets[actorId]=new ErosAmulet{pool=pool,expires=world.Clock+10};
        }
        void ObserveErosAmuletDamage(int source,OriginalWorldUnitView actor,double damage)
        {
            if(actor==null||!erosAmulets.TryGetValue(actor.entityId,out var amulet))return;
            var attacker=world.UnitState(source);if(attacker!=null&&CasterHasType(attacker,"structure"))return;
            // Zw overwrites its shared hashtable slots on every event; each
            // queued zero-delay callback then reads the latest damage/source.
            amulet.damage=damage;amulet.source=source;amulet.pending++;
            amulet.due=world.Clock+1e-6;
        }
        void RemoveErosAmulet(int actor)
        {
            erosAmulets.Remove(actor);
            if(world.UnitState(actor)!=null)ApplyUnitAbilityOverlay(actor,null,new[]{"A0YN"});
        }
        void AdvanceErosAmulets()
        {
            foreach(var pair in new List<KeyValuePair<int,ErosAmulet>>(erosAmulets))
            {
                var actor=world.UnitState(pair.Key);var amulet=pair.Value;
                if(actor==null||actor.health<=.405||amulet.expires<=world.Clock+1e-9)
                {RemoveErosAmulet(pair.Key);continue;}
                if(amulet.due>world.Clock+1e-9)continue;
                int pending=amulet.pending;amulet.pending=0;
                while(pending-->0&&erosAmulets.ContainsKey(pair.Key)&&amulet.damage>0)
                {
                    if(amulet.pool<=amulet.damage){RemoveErosAmulet(pair.Key);break;}
                    amulet.pool-=amulet.damage;
                    var source=world.UnitState(amulet.source);
                    if(source==null||source.health<=.405||!AreEnemies(actor.ownerSlot,source.ownerSlot)||erosReflection)continue;
                    erosReflection=true;
                    try{ApplyTriggeredHit(actor.entityId,actor.ownerSlot,source,amulet.damage,OriginalTriggeredDamageMode.ChaosUniversal);}
                    finally{erosReflection=false;}
                }
            }
        }
    }
}

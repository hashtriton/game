using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemJetJump
        {
            internal int actor,owner,target;
            internal bool exchange;
            internal double due,travel,distance,angle;
            internal OriginalPoint start,destination;
        }
        readonly List<ItemJetJump> itemJetJumps=new List<ItemJetJump>();
        readonly List<KeyValuePair<int,double>> itemMoonExpiries=new List<KeyValuePair<int,double>>();

        // k7/K7/l7/m7/M7 and W8/Z8/y8, source map02a790123096.
        // Native item activation and sparse base-family defaults are separate
        // from these authored callbacks; ITEMTARGET1 measures that boundary.
        bool BeginItemLegacyActive(int actorId,string ability,int targetId)
        {
            var actor=world.UnitState(actorId);
            if(ability=="A0FI"||ability=="A0NA")
            {
                var target=world.UnitState(targetId);if(target==null)return true;
                if(ability=="A0FI"&&HasEffectiveUnitAbility(target,"B06X"))return true;
                world.SetPathingEnabled(actorId,false);
                if(ability=="A0NA")world.SetPathingEnabled(targetId,false);
                itemJetJumps.Add(new ItemJetJump {actor=actorId,owner=actor.ownerSlot,target=targetId,exchange=ability=="A0NA",
                    start=actor.position,destination=target.position,due=world.Clock+.03,
                    distance=Math.Sqrt(SquaredDistance(actor.position,target.position)),
                    angle=Math.Atan2(target.position.y-actor.position.y,target.position.x-actor.position.x)});
                return true;
            }
            if(ability=="A0JE")
            {
                var target=world.UnitState(targetId);if(target==null||target.kind!=OriginalWorldUnitKind.Hero)return true;
                bool umbra=Array.Exists(PlayerAt(actor.ownerSlot).inventory.HeroSlots,i=>i?.itemId=="I08D");
                double fraction=umbra?.8:.5,cap=umbra?80:50;
                foreach(string axis in new[]{"A0HZ","A0DV","A0JJ"})
                    itemScriptActs.Add(new ItemScriptAct {actor=targetId,owner=target.ownerSlot,ability=axis,
                        amount=Math.Min(cap,Math.Truncate(ItemScriptBaseAttribute(target.ownerSlot,axis)*fraction)),period=6,due=world.Clock+6});
                ApplyUnitAbilityOverlay(targetId,new[]{umbra?"A17O":"A0SC","A0I4"},null);
                nativeSpellResistance.Set(targetId,"moon-necklace:A0I4",.2);
                itemMoonExpiries.Add(new KeyValuePair<int,double>(targetId,world.Clock+6));
                RefreshItemScriptAttributes(targetId);return true;
            }
            if(ability=="A0OU"||ability=="A0OS"){BeginItemTaunt(actorId,ability);return true;}
            if(ability=="A0YK"){BeginErosAmulet(actorId);return true;}
            return false;
        }
        void AdvanceItemLegacyActives()
        {
            AdvanceItemTauntApplications();
            AdvanceItemOffense();
            AdvanceErosAmulets();
            foreach(var expiry in itemMoonExpiries.ToArray())
                if(expiry.Value<=world.Clock+1e-9)
                {
                    if(world.UnitState(expiry.Key)!=null)ApplyUnitAbilityOverlay(expiry.Key,null,new[]{"A0SC","A17O","A0I4"});
                    nativeSpellResistance.Remove(expiry.Key,"moon-necklace:A0I4");itemMoonExpiries.Remove(expiry);
                }
            foreach(var jump in itemJetJumps.ToArray())
                while(itemJetJumps.Contains(jump)&&jump.due<=world.Clock+1e-9)
                {
                    jump.due+=.03;
                    var actor=world.UnitState(jump.actor);var target=world.UnitState(jump.target);
                    if(actor==null||target==null)
                    {FinishItemJetJump(jump);break;}
                    jump.travel+=25;
                    var destination=jump.exchange?jump.destination:target.position;
                    double distance=jump.exchange?jump.distance:Math.Sqrt(SquaredDistance(jump.start,destination));
                    double angle=jump.exchange?jump.angle:Math.Atan2(destination.y-jump.start.y,destination.x-jump.start.x);
                    // Source SetUnitX/Y deliberately ignores terrain collision,
                    // and retains its final25-unit overshoot.
                    world.ForcePosition(jump.actor,new OriginalPoint(jump.start.x+jump.travel*Math.Cos(angle),jump.start.y+jump.travel*Math.Sin(angle)));
                    if(jump.exchange)world.ForcePosition(jump.target,new OriginalPoint(jump.destination.x-jump.travel*Math.Cos(angle),jump.destination.y-jump.travel*Math.Sin(angle)));
                    if(jump.travel<distance)continue;
                    foreach(var victim in CasterUnits())
                        if(victim.health>.405&&AreEnemies(jump.owner,victim.ownerSlot)&&!CasterMagicImmune(victim)&&
                            (SquaredDistance(destination,victim.position)<=250*250||jump.exchange&&SquaredDistance(jump.start,victim.position)<=250*250))
                        {
                            ApplyTriggeredHit(jump.actor,jump.owner,victim,jump.exchange?300:150,OriginalTriggeredDamageMode.SpellMagic);
                            ApplyItemJetHex(world.UnitState(victim.entityId),jump.actor);
                        }
                    FinishItemJetJump(jump);
                }
        }
        void FinishItemJetJump(ItemJetJump jump)
        {
            if(world.UnitState(jump.actor)!=null)world.SetPathingEnabled(jump.actor,true);
            if(jump.exchange&&world.UnitState(jump.target)!=null)world.SetPathingEnabled(jump.target,true);
            itemJetJumps.Remove(jump);
        }
        void ApplyItemJetHex(OriginalWorldUnitView target,int source)
        {
            ApplyNativeItemHex(target,source);
        }
    }
}

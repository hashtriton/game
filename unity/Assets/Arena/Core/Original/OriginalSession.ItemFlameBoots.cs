using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemFlamePatch
        { internal int actor,owner,ticks=6;internal double next,radius;internal OriginalPoint point; }
        double itemFlameClock,itemFlameScan=.2;int itemFlameEpoch;
        readonly Dictionary<int,double> itemFlameActive=new Dictionary<int,double>();
        readonly Dictionary<int,OriginalPoint> itemFlamePositions=new Dictionary<int,OriginalPoint>();
        readonly Dictionary<int,int> itemFlameLastHit=new Dictionary<int,int>();
        readonly List<ItemFlamePatch> itemFlamePatches=new List<ItemFlamePatch>();

        NativeItemActionRule FlameBootItemRule()
        {
            var a=combatCatalog.Ability("A1CW");
            if(a.Text("code")!="AIsa"||a.Number("DataA1")!=.2||a.Number("Dur1")!=6||a.Number("Cool1")!=24||a.Text("BuffID1")!="B0CI")
                throw new InvalidOperationException("Flame boot declaration changed.");
            if(Array.Exists(a.fields,f=>f.key=="Cost1")&&(!a.TryNumber("Cost1",out double cost,out _)||cost!=0))
                throw new InvalidOperationException("Flame boot mana conflicts with inherited AIsa0 policy.");
            return new NativeItemActionRule{abilityId=a.id,cooldownGroup=itemCatalog.Item("I0B2").cooldownId,
                requiresCharge=false,cooldown=24,sourceEffect="flame-boots"};
        }
        double FlameBootMovementBonus(int actor) => itemFlameActive.TryGetValue(actor,out double until)&&until>itemFlameClock? .2:0;
        void BeginFlameBoots(int actor)
        {
            CaptureAbilityMovementBase(actor);itemFlameActive[actor]=itemFlameClock+5;RefreshAbilityMovement(actor);
        }
        void AdvanceFlameBoots(double seconds)
        {
            double end=itemFlameClock+seconds;
            while(true)
            {
                ItemFlamePatch patch=null;
                foreach(var row in itemFlamePatches)if(patch==null||row.next<patch.next)patch=row;
                double next=Math.Min(itemFlameScan,patch?.next??double.PositiveInfinity);
                if(next>end+1e-9)break;itemFlameClock=next;
                if(itemFlameScan<=next)
                {
                    itemFlameEpoch=itemFlameEpoch>=1000000?1:itemFlameEpoch+1;itemFlameScan+=.2;
                    foreach(var player in players)
                    {
                        var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                        if(actor==null||actor.health<=.405||player.inventory==null)continue;
                        bool held=false;foreach(var item in player.inventory.HeroSlots)if(item?.itemId=="I0B2"){held=true;break;}
                        if(!held)continue;itemFlamePositions.TryGetValue(player.slot,out var before);
                        if(Math.Abs(actor.position.x-before.x)<=20&&Math.Abs(actor.position.y-before.y)<=20)continue;
                        itemFlamePositions[player.slot]=actor.position;
                        itemFlamePatches.Add(new ItemFlamePatch{actor=actor.entityId,owner=actor.ownerSlot,point=actor.position,
                            next=next+.5,radius=FlameBootMovementBonus(actor.entityId)>0?250:150});
                    }
                }
                else
                {
                    if(patch.ticks==0){itemFlamePatches.Remove(patch);continue;}
                    patch.ticks--;patch.next+=.5;
                    var nearby=new List<OriginalWorldUnitView>();
                    foreach(var unit in world.Snapshot().units)if(SquaredDistance(unit.position,patch.point)<=patch.radius*patch.radius)nearby.Add(unit);
                    // HS8611 computes kS from FirstOfGroup ONCE before its loop.
                    // Preserve this literal shared key, rather than inventing
                    // an AoE hit on each victim. Host entity order replaces
                    // opaque native group order; that ordering is derived.
                    nearby.Sort((a,b)=>a.entityId.CompareTo(b.entityId));
                    if(nearby.Count==0)continue;int stamp=nearby[0].entityId;
                    foreach(var target in nearby)
                    {
                        if(itemFlameLastHit.TryGetValue(stamp,out int epoch)&&epoch==itemFlameEpoch)continue;
                        if(target.health<=.405||!AreEnemies(patch.owner,target.ownerSlot)||CasterMagicImmune(target))continue;
                        ApplyTriggeredHit(patch.actor,patch.owner,target,22.5,OriginalTriggeredDamageMode.SpellMagic);
                        itemFlameLastHit[stamp]=itemFlameEpoch;
                    }
                }
            }
            itemFlameClock=end;
            foreach(int actor in new List<int>(itemFlameActive.Keys))
                if(itemFlameActive[actor]<=end+1e-9)
                {itemFlameActive.Remove(actor);RefreshAbilityMovement(actor);ReleaseAbilityMovementBase(actor);}
            // PS/hS uses an unpaused five-second timer even though native Dur6.
            // Patch helper placement/health is represented by a stationary VFX;
            // six source pulses survive caster death and inventory removal.
        }
        void AppendFlameBootVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var patch in itemFlamePatches)output.Add(new OriginalVisualEffectView{kind=OriginalVisualEffectKind.ActiveCircle,
                abilityId="A1CW",sourceEntityId=patch.actor,position=patch.point,radius=patch.radius,progress=1,variant=patch.radius==250?1:0});
        }
    }
}

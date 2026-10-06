using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrdinarySwarm
        {
            internal int actor,owner,steps;
            internal string ability;
            internal OriginalPoint position,direction;
            internal double fraction,next;
            internal readonly HashSet<int> hit=new HashSet<int>();
        }
        readonly List<OrdinarySwarm> ordinarySwarms=new List<OrdinarySwarm>();

        void BeginOrdinaryCarrionSwarm(OriginalWorldUnitView actor,OriginalWorldUnitView target,OriginalCombatDefinition ability)
        {
            if(!OrdinarySpellTarget(actor,target,ability.id))return;
            BeginOrdinaryCarrionSwarmAtPoint(actor,target.position,ability);
        }
        void BeginOrdinaryCarrionSwarmAtPoint(OriginalWorldUnitView actor,OriginalPoint point,OriginalCombatDefinition ability)
        {
            double dx=point.x-actor.position.x,dy=point.y-actor.position.y;
            double distance=Math.Sqrt(dx*dx+dy*dy);if(distance<1e-9){dx=1;dy=0;distance=1;}
            var direction=new OriginalPoint(dx/distance,dy/distance);
            // SWARM1 proves A0RA's one immediate native zero at165WC. Its
            // declared taper is reconstructed for other recipients/A0RB;
            // no missing DataA/DataB value becomes an invented positive hit.
            foreach(var previous in world.Snapshot().units)
            {
                var victim=world.UnitState(previous.entityId);
                if(!OrdinarySpellTarget(actor,victim,ability.id))continue;
                double x=victim.position.x-actor.position.x,y=victim.position.y-actor.position.y;
                double along=x*direction.x+y*direction.y;
                if(along<0||along>ability.Number("DataC1"))continue;
                double width=ability.Number("Area1")+(ability.Number("DataD1")-ability.Number("Area1"))*along/ability.Number("DataC1");
                if(Math.Abs(x*direction.y-y*direction.x)<=width)
                    ApplyNativeTriggeredHit(actor.entityId,actor.ownerSlot,victim,0,OriginalTriggeredDamageMode.SpellMagic);
            }
            // IKv/ILv/Ilv36379..36462. The original handler has its own
            // moving h01X, fixed100 radius and retained group, independent
            // of the native zero wave. Source death does not cancel it.
            ordinarySwarms.Add(new OrdinarySwarm{actor=actor.entityId,owner=actor.ownerSlot,ability=ability.id,direction=direction,
                fraction=HasEffectiveUnitAbility(actor,"A0RA")?.2:.25,next=world.Clock+.03,
                position=new OriginalPoint(actor.position.x+30*direction.x,actor.position.y+30*direction.y)});
        }
        void AdvanceOrdinaryCarrionSwarms()
        {
            foreach(var wave in ordinarySwarms.ToArray())
                while(wave.next<=world.Clock+1e-9&&wave.steps<32)
                {
                    wave.next+=.03;wave.steps++;
                    wave.position=new OriginalPoint(wave.position.x+25*wave.direction.x,wave.position.y+25*wave.direction.y);
                    foreach(var previous in world.Snapshot().units)
                    {
                        var target=world.UnitState(previous.entityId);
                        if(target==null||target.health<=.405||target.hidden||wave.hit.Contains(target.entityId)||
                            !AreEnemies(wave.owner,target.ownerSlot)||CasterMagicImmune(target)||CasterHasType(target,"structure")||
                            SquaredDistance(target.position,wave.position)>100*100)continue;
                        // ILv retains eligible recipients before hL, including
                        // an invulnerable recipient whose damage is rejected.
                        wave.hit.Add(target.entityId);
                        ApplyTriggeredHit(wave.actor,wave.owner,target,target.profile.maxHealth*wave.fraction,OriginalTriggeredDamageMode.SpellMagic);
                    }
                    if(wave.steps==32)ordinarySwarms.Remove(wave);
                }
        }
        void ApplyOrdinaryRejuvenationCompanion(OriginalWorldUnitView actor)
        {
            // IMv/Ipv36464..36489: one h011/A03V orders every nearby
            // living allied nonstructure except the caster. Native helper
            // order timing/refresh is a declared family transfer. Processing
            // before the main native buff preserves the source EFFECT order.
            var helper=combatCatalog.Ability("A03V");
            foreach(var previous in world.Snapshot().units)
            {
                var target=world.UnitState(previous.entityId);
                if(target==null||target.entityId==actor.entityId||target.health<=.405||target.hidden||
                    AreEnemies(actor.ownerSlot,target.ownerSlot)||CasterHasType(target,"structure")||
                    SquaredDistance(actor.position,target.position)>200*200)continue;
                if(!WeaponTargetTypeAllowed(helper.Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))||
                    CasterHasType(target,"mechanical"))continue;
                AddOrdinaryBuff(target,helper,helper.Number(IsNativeHeroPredicate(target)?"HeroDur1":"Dur1"));
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PermanentInvisibilityState
        { internal string ability; internal double remaining,updated; }
        readonly Dictionary<int,PermanentInvisibilityState> permanentInvisibility=new Dictionary<int,PermanentInvisibilityState>();

        // Apiv aliases explicitly declare Dur/HeroDur. Gho1/DataA means
        // auto-acquisition, not fade duration (patch MetaData10434/UI6263).
        // Initial registration, pause freezing and reset at accepted attack
        // or cast are host timing policies, not a native probe of these aliases.
        OriginalCombatDefinition PermanentInvisibilityAbility(OriginalWorldUnitView actor)
        {
            if(actor==null||actor.kind==OriginalWorldUnitKind.Illusion)return null;
            foreach(var a in NativeUnitAbilities(actor))
                if(a.Text("code")=="Apiv"&&a.overrides.Length==0&&
                    a.TryNumber("Dur1",out double duration,out _)&&duration>=0&&
                    a.TryNumber("HeroDur1",out double heroDuration,out _)&&heroDuration>=0)return a;
            return null;
        }
        bool NativeInvisibilityActive(int actorId)
        {
            var actor=world?.UnitState(actorId);
            return actor!=null&&actor.health>.405&&permanentInvisibility.TryGetValue(actorId,out var state)&&
                state.remaining<=1e-9&&PermanentInvisibilityAbility(actor)?.id==state.ability;
        }
        bool CombatInvisibilityActive(int actorId)=>ItemInvisibilityActive(actorId)||NativeInvisibilityActive(actorId);
        void RevealNativeInvisibility(int actorId)
        {
            var actor=world?.UnitState(actorId);var a=PermanentInvisibilityAbility(actor);
            if(a==null){permanentInvisibility.Remove(actorId);return;}
            permanentInvisibility[actorId]=new PermanentInvisibilityState{ability=a.id,updated=world.Clock,
                remaining=a.Number(IsNativeHeroPredicate(actor)?"HeroDur1":"Dur1")};
        }
        void AdvanceNativeInvisibility()
        {
            foreach(int id in new List<int>(permanentInvisibility.Keys))
            {var actor=world.UnitState(id);if(actor==null||actor.health<=.405||PermanentInvisibilityAbility(actor)==null)permanentInvisibility.Remove(id);}
            foreach(var actor in world.Snapshot().units)
            {
                if(actor.health<=.405)continue;var a=PermanentInvisibilityAbility(actor);if(a==null)continue;
                if(!permanentInvisibility.TryGetValue(actor.entityId,out var state)||state.ability!=a.id)
                {RevealNativeInvisibility(actor.entityId);continue;}
                double elapsed=Math.Max(0,world.Clock-state.updated);state.updated=world.Clock;
                if(!actor.paused)state.remaining=Math.Max(0,state.remaining-elapsed);
            }
        }
        bool NativeDetectionSees(int observerOwner,OriginalWorldUnitView target)
        {
            if(target==null||target.hidden)return false;
            if(SummonDetectionSees(observerOwner,target))return true;
            foreach(var detector in world.Snapshot().units)
            {
                if(detector.health<=.405||detector.hidden||detector.kind==OriginalWorldUnitKind.Illusion||
                    AreEnemies(observerOwner,detector.ownerSlot))continue;
                foreach(var a in NativeUnitAbilities(detector))
                {
                    // Det1/DataA=1 invisibility,2burrow,3both (effective patch
                    // UI/UnitEditorData103..106). Rng is authored. Terrain vision
                    // and unknown image-passive inheritance are not fabricated.
                    if(a.Text("code")!="Atru"||a.overrides.Length!=0||
                        !a.TryNumber("DataA1",out double detection,out _)||
                        (detection!=1&&detection!=3)||!a.TryNumber("Rng1",out double range,out _)||range<0)continue;
                    if(SquaredDistance(detector.position,target.position)<=range*range)return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrdinaryNativeStatus
        {
            internal int actor,owner,target;
            internal string ability,buff;
            internal double age,duration,updatedAt,nextPulse;
        }
        sealed class OrdinaryImmolation { internal double updatedAt,nextPulse; }
        readonly Dictionary<int,Dictionary<string,OrdinaryNativeStatus>> ordinaryStatuses=new Dictionary<int,Dictionary<string,OrdinaryNativeStatus>>();
        readonly Dictionary<int,OrdinaryImmolation> ordinaryImmolations=new Dictionary<int,OrdinaryImmolation>();

        // ORDINARY2 c6b6f9df392a: these are per-alias observations, not
        // generic handler defaults. DataA1 of Aply is a level threshold;
        // the observed100 movement is independent of that field.
        static double OrdinaryObservedRecovery(string ability)=>ability=="A0B1"?1.2:ability=="A074"||ability=="ACpu"||ability=="A0RA"||ability=="A0RB"?.51:ability=="A07A"?.5:0;
        bool HasOrdinaryNativeStatus(int actor,string buff)=>buff!=null&&ordinaryStatuses.TryGetValue(actor,out var states)&&states.ContainsKey(buff);
        bool OrdinaryEntangled(int actor)=>HasOrdinaryNativeStatus(actor,"BEer");
        double OrdinaryStatusMovement(int actor)
        {
            if(!ordinaryStatuses.TryGetValue(actor,out var states))return 0;
            double value=0;bool transformed=false;
            foreach(var s in states.Values)
            {
                var a=combatCatalog.Ability(s.ability);
                if(s.buff=="BEsh")value-=a.Number("DataB1")*Math.Pow(Math.Max(0,1-Math.Floor(s.age+1e-8)/s.duration),a.Number("DataD1"));
                else if(s.buff=="Bprg")value-=Math.Max(0,1-Math.Floor((s.age+1e-8)*a.Number("DataA1")/s.duration)/a.Number("DataA1"));
                else if(s.buff=="Bply"||s.buff=="B05S")transformed=true;
            }
            if(transformed)
            {
                var unit=world.UnitState(actor);
                if(unit!=null)
                {
                    double baseline=unit.kind==OriginalWorldUnitKind.Hero?HeroCombatStats(unit.ownerSlot).baseMoveSpeed:
                        unit.kind==OriginalWorldUnitKind.Illusion?combatCatalog.Unit(unit.rawcode).Number("spd"):UnmodifiedNonHeroMovement(unit);
                    if(baseline>0)value+=100/baseline-1;
                }
                // Other percentages share the original baseline. Overlapping
                // absolute transformations apply100 once: explicit host policy.
            }
            return value;
        }
        void ApplyNativeItemHex(OriginalWorldUnitView target,int source)
        {
            if(target==null||target.health<=.405||target.hidden||target.invulnerable||CasterMagicImmune(target)||
                CasterHasType(target,"mechanical")||CasterHasType(target,"structure"))return;
            var a=combatCatalog.Ability("A0NC");
            if(a.Text("code")!="AOhx"||a.Text("BuffID1")!="B05S"||a.Number("Dur1")!=2||a.Number("HeroDur1")!=2)
                throw new InvalidOperationException("A0NC differs from observed HEX2 profile.");
            // HEX2 952bfd6df95c: h011 helper emits zero at effect and expiry,
            // B05S lasts2s, movement100, Weapon/Cast blocked. Item blocking,
            // pause freezing and latest-buff refresh remain explicit host policies.
            var s=new OrdinaryNativeStatus{actor=0,owner=world.UnitState(source)?.ownerSlot??0,target=target.entityId,
                ability=a.id,buff="B05S",duration=2,updatedAt=world.Clock,nextPulse=double.PositiveInfinity};
            OrdinaryStatusDamage(s,0);target=world.UnitState(s.target);
            if(target==null||target.health<=.405||target.hidden||target.invulnerable||CasterMagicImmune(target))return;
            CaptureAbilityMovementBase(target.entityId);
            if(!ordinaryStatuses.TryGetValue(target.entityId,out var states))ordinaryStatuses[target.entityId]=states=new Dictionary<string,OrdinaryNativeStatus>();
            states[s.buff]=s;
            SetActorControl(target.entityId,"ordinary-status:"+s.buff,
                OriginalActorControlMask.Weapon|OriginalActorControlMask.Cast|OriginalActorControlMask.Item,0,true,true);
            RefreshAbilityMovement(target.entityId);RescaleWeaponRate(target.entityId,world.Clock);
        }
        double OrdinaryStatusAttackSlow(int actor)
        {
            if(!ordinaryStatuses.TryGetValue(actor,out var states)||!states.TryGetValue("BEsh",out var s))return 0;
            var a=combatCatalog.Ability(s.ability);
            // Esh3 declares IAS loss; transferring measured movement decay
            // to IAS is explicit reconstruction, not a cadence measurement.
            return a.Number("DataC1")*Math.Pow(Math.Max(0,1-Math.Floor(s.age+1e-8)/s.duration),a.Number("DataD1"));
        }
        bool ResolveOrdinaryNativeStatus(OriginalWorldUnitView actor,OriginalWorldUnitView target,OriginalCombatDefinition a)
        {
            string code=a.Text("code");
            if(code=="AEim")
            {
                if(!ordinaryImmolations.ContainsKey(actor.entityId))ordinaryImmolations[actor.entityId]=new OrdinaryImmolation{updatedAt=world.Clock,nextPulse=world.Clock+.01};
                return true;
            }
            if(code!="AEer"&&code!="AEsh"&&code!="Aprg"&&code!="Aply")return false;
            if(!OrdinarySpellTarget(actor,target,a.id))return true;
            double duration=a.Number(IsNativeHeroPredicate(target)?"HeroDur1":"Dur1");
            var s=new OrdinaryNativeStatus{actor=actor.entityId,owner=actor.ownerSlot,target=target.entityId,ability=a.id,buff=a.Text("BuffID1"),
                updatedAt=world.Clock,duration=duration,nextPulse=code=="AEer"?.01:code=="AEsh"?a.Number("Cast1"):double.PositiveInfinity};
            // ORDINARY2's first zero and Shadow's initial positive event see
            // the old speed/buff. Only Shadow's second zero sees the new buff.
            OrdinaryStatusDamage(s,0);
            target=world.UnitState(s.target);if(!OrdinarySpellTarget(actor,target,a.id))return true;
            if(code=="AEsh")OrdinaryStatusDamage(s,a.Number("DataE1"));
            else if(code=="Aprg")OrdinaryStatusDamage(s,0);
            target=world.UnitState(s.target);if(!OrdinarySpellTarget(actor,target,a.id))return true;
            if(code=="Aprg")
            {
                // Authored purge removes known dispellable buffs. Breadth and
                // order across port modules are explicit host reconstruction.
                RemoveOrdinaryNativeBuffs(target.entityId);ClearNegativeActorControls(target.entityId);
                RemovePoison(target.entityId);RemoveArcherDebuffs(target.entityId);RemoveKnightAcid(target.entityId);
                RemovePyroChainBuff(target.entityId);RemoveShieldCripple(target.entityId);RemoveNativeCorruption(target.entityId);
                if(target.kind==OriginalWorldUnitKind.Hero)RemoveArcherBuffs(target.ownerSlot);
            }
            CaptureAbilityMovementBase(target.entityId);
            if(!ordinaryStatuses.TryGetValue(target.entityId,out var states))ordinaryStatuses[target.entityId]=states=new Dictionary<string,OrdinaryNativeStatus>();
            states[s.buff]=s; // Latest same-buff refresh and pause freezing are host policies.
            if(code=="AEer"||code=="Aply")SetActorControl(target.entityId,"ordinary-status:"+s.buff,
                OriginalActorControlMask.Weapon|(code=="AEer"?OriginalActorControlMask.Move:OriginalActorControlMask.Cast),0,true,true);
            RefreshAbilityMovement(target.entityId);RescaleWeaponRate(target.entityId,world.Clock);
            if(!OrdinaryStatusCurrent(s))return true;
            if(code=="AEsh")OrdinaryStatusDamage(s,0);
            else if(code=="Aprg")
            {
                // Prg3 is declared summoned-unit damage. This target axis is
                // not part of ORDINARY2's ordinary hero control.
                if(OrdinaryStatusCurrent(s)&&(target.kind==OriginalWorldUnitKind.Summon||target.kind==OriginalWorldUnitKind.Illusion))
                    OrdinaryStatusDamage(s,a.Number("DataC1"));
            }
            return true;
        }
        bool OrdinaryStatusCurrent(OrdinaryNativeStatus s)=>ordinaryStatuses.TryGetValue(s.target,out var states)&&
            states.TryGetValue(s.buff,out var current)&&ReferenceEquals(current,s);
        void OrdinaryStatusDamage(OrdinaryNativeStatus s,double damage)
        {
            var target=world.UnitState(s.target);
            if(target!=null&&target.health>.405&&!target.hidden&&!target.invulnerable&&!CasterMagicImmune(target))
                ApplyNativeTriggeredHit(s.actor,s.owner,target,damage,OriginalTriggeredDamageMode.SpellMagic);
        }
        void RemoveOrdinaryStatuses(int actor)
        {
            if(!ordinaryStatuses.TryGetValue(actor,out var states))return;
            ordinaryStatuses.Remove(actor);
            foreach(var s in states.Values)ClearActorControl(actor,"ordinary-status:"+s.buff);
            RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);ReleaseAbilityMovementBase(actor);
        }
        void AdvanceOrdinaryNativeStatuses()
        {
            foreach(var pair in new List<KeyValuePair<int,Dictionary<string,OrdinaryNativeStatus>>>(ordinaryStatuses))
            {
                var target=world.UnitState(pair.Key);
                if(target==null||target.health<=.405){RemoveOrdinaryStatuses(pair.Key);continue;}
                foreach(var s in new List<OrdinaryNativeStatus>(pair.Value.Values))
                {
                    if(!OrdinaryStatusCurrent(s))continue;
                    double elapsed=Math.Max(0,world.Clock-s.updatedAt);s.updatedAt=world.Clock;if(target.paused)continue;
                    double previousAge=s.age;s.age+=elapsed;var a=combatCatalog.Ability(s.ability);
                    if(s.age<s.duration-1e-9&&Math.Floor(previousAge+1e-8)!=Math.Floor(s.age+1e-8))
                    {RefreshAbilityMovement(pair.Key);RescaleWeaponRate(pair.Key,world.Clock);}
                    while(s.nextPulse<=s.age+1e-9&&s.nextPulse<s.duration-1e-9&&OrdinaryStatusCurrent(s))
                    {s.nextPulse+=s.buff=="BEer"?1:a.Number("Cast1");OrdinaryStatusDamage(s,a.Number("DataA1"));}
                    if(!OrdinaryStatusCurrent(s))continue;
                    if(s.age+1e-9>=s.duration)
                    {
                        pair.Value.Remove(s.buff);ClearActorControl(pair.Key,"ordinary-status:"+s.buff);
                        if(pair.Value.Count==0)ordinaryStatuses.Remove(pair.Key);
                        RefreshAbilityMovement(pair.Key);RescaleWeaponRate(pair.Key,world.Clock);ReleaseAbilityMovementBase(pair.Key);
                        if(s.buff!="BEsh")OrdinaryStatusDamage(s,0);
                    }
                }
            }
            foreach(var pair in new List<KeyValuePair<int,OrdinaryImmolation>>(ordinaryImmolations))
            {
                var actor=world.UnitState(pair.Key);var s=pair.Value;var a=combatCatalog.Ability("A05Y");
                if(actor==null||actor.health<=.405||!HasEffectiveUnitAbility(actor,a.id)){ordinaryImmolations.Remove(pair.Key);continue;}
                double elapsed=Math.Max(0,world.Clock-s.updatedAt);s.updatedAt=world.Clock;
                if(actor.paused||actor.hidden){s.nextPulse+=elapsed;continue;} // Explicit unmeasured pause/hidden phase policy.
                double drain=elapsed*a.Number("DataB1");
                if(actor.mana<drain){world.TrySpendMana(actor.entityId,actor.mana);ordinaryImmolations.Remove(pair.Key);continue;}
                world.TrySpendMana(actor.entityId,drain);
                while(s.nextPulse<=world.Clock+1e-9)
                {
                    s.nextPulse+=a.Number("Dur1");
                    foreach(var previous in world.Snapshot().units)
                    {
                        var target=world.UnitState(previous.entityId);actor=world.UnitState(pair.Key);
                        if(actor==null||actor.health<=.405)break;
                        if(OrdinarySpellTarget(actor,target,a.id)&&SquaredDistance(actor.position,target.position)<=a.Number("Area1")*a.Number("Area1"))
                            ApplyNativeTriggeredHit(actor.entityId,actor.ownerSlot,target,a.Number("DataA1"),OriginalTriggeredDamageMode.SpellMagic);
                    }
                }
            }
        }
    }
}

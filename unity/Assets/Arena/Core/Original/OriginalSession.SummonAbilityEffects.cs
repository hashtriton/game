using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class SummonBloodlust { internal double remaining=20,updated; }
        sealed class SummonNet { internal int owner;internal OriginalPoint point;internal double due; }
        sealed class SummonNetHit { internal int target,owner;internal double due; }
        sealed class SummonShadowShield { internal int target,owner;internal double due,received; }
        readonly Dictionary<int,SummonBloodlust> summonBloodlust=new Dictionary<int,SummonBloodlust>();
        readonly Dictionary<string,double> summonDetection=new Dictionary<string,double>();
        readonly List<SummonNet> summonNets=new List<SummonNet>();
        readonly List<SummonNetHit> summonNetHits=new List<SummonNetHit>();
        readonly List<double> summonNetHelperDeaths=new List<double>();
        readonly List<SummonShadowShield> summonShadowShields=new List<SummonShadowShield>();
        double SummonAbilityMovementBonus(int id)=>summonBloodlust.ContainsKey(id)?.1:0;
        double SummonAbilityAttackSpeedBonus(int id)=>summonBloodlust.ContainsKey(id)?.5:0;
        bool SummonDetectionSees(int owner,OriginalWorldUnitView target)=>target!=null&&
            summonDetection.TryGetValue(owner+":"+target.entityId,out double until)&&until>world.Clock+1e-9;

        void ValidateSummonSourceAbility(string id)
        {
            if(id=="A18I")ValidateAbilityChanges(new[]{"A18M"});
            if(id=="A18J")
            {
                var root=combatCatalog.Ability("A18L");
                if(root.Text("code")!="Aens"||root.Text("BuffID1")!="B0B3,B0B3"||root.Number("Dur1")!=3||root.Number("HeroDur1")!=3||root.Number("DataC1")!=128)
                    throw new InvalidOperationException("summon-net-native-declaration-conflict");
            }
        }
        void ResolveSummonSourceAbility(OriginalWorldUnitView actor,string id,int targetId,OriginalPoint point)
        {
            var target=world.UnitState(targetId);
            if(id=="A0WH")
            {
                // AItb true sight: a captured1000 area and8s reveal use declared
                // alias values. The stock zero cost and target memory are host
                // family policies; no terrain fog-of-war is simulated.
                foreach(var unit in world.Snapshot().units)
                    if(unit.health>.405&&!unit.hidden&&AreEnemies(actor.ownerSlot,unit.ownerSlot)&&SquaredDistance(actor.position,unit.position)<=1000*1000)
                        summonDetection[actor.ownerSlot+":"+unit.entityId]=world.Clock+8;
                return;
            }
            if(id=="A18J")
            {
                // AXv/AEv37104..37180: fixed target,18 per .03s. Native
                // projectile facing/float arithmetic use the host geometry.
                double distance=Math.Sqrt(SquaredDistance(actor.position,point));
                summonNets.Add(new SummonNet{owner=actor.ownerSlot,point=point,due=world.Clock+Math.Max(1,Math.Ceiling(distance/18))*.03});return;
            }
            if(!SummonAbilityTarget(actor,target,id))return;
            if(id=="A0FD")
            {
                // vAv22466:5% of the selected target's maxHP to all its living
                // allies within500, including structures, then native Ahea50
                // on the selected target. Direct life writes do not run fL.
                double amount=target.profile.maxHealth*.05;
                foreach(var ally in world.Snapshot().units)
                    if(ally.health>.405&&!AreEnemies(target.ownerSlot,ally.ownerSlot)&&SquaredDistance(target.position,ally.position)<=500*500)
                        SummonDirectHeal(ally.entityId,amount);
                SummonDirectHeal(targetId,50);return;
            }
            if(id=="A030")
            {
                CaptureAbilityMovementBase(targetId);summonBloodlust[targetId]=new SummonBloodlust{updated=world.Clock};
                RefreshAbilityMovement(targetId);RescaleWeaponRate(targetId,world.Clock);return;
            }
            if(id=="A0A2")
            {
                // CONTROL2 AUsl family transfer; exact alias durations are
                // declared4s HERO/12s others, with2s protected sleep.
                ApplyResolvedUnitHit(actor.entityId,actor.ownerSlot,target,0);target=world.UnitState(targetId);
                if(target==null||target.health<=.405)return;
                nativeSleeps[targetId]=new NativeSleep{source=actor.entityId,owner=actor.ownerSlot,
                    remaining=IsNativeHeroPredicate(target)?4:12,protectedRemaining=2};
                SetActorControl(targetId,NativeSleepToken,AllActorControls,0,true,true);return;
            }
            if(id=="A18I")
            {
                // ARv/AOv37183..37237: each cast has an independent event
                // accumulator. Rp replaces only its shared shield reservoir.
                AddItemSourceShield(targetId,600,8,.5,"A18M");
                summonShadowShields.Add(new SummonShadowShield{target=targetId,owner=actor.ownerSlot,due=world.Clock+8});
            }
        }
        void SummonDirectHeal(int id,double amount)
        {
            var target=world.UnitState(id);if(target!=null&&target.health>.405)
                world.UpdateProfile(id,target.profile,Math.Min(target.profile.maxHealth,target.health+amount),target.mana);
        }
        void ObserveSummonShieldDamage(OriginalWorldUnitView target,double damage)
        {if(target!=null)foreach(var shield in summonShadowShields)if(shield.target==target.entityId)shield.received+=damage*.5;}
        void AdvanceSummonSourceAbilities()
        {
            foreach(int id in new List<int>(summonBloodlust.Keys))
            {
                var unit=world.UnitState(id);var buff=summonBloodlust[id];double elapsed=Math.Max(0,world.Clock-buff.updated);buff.updated=world.Clock;
                if(unit!=null&&!unit.paused)buff.remaining-=elapsed;
                if(unit!=null&&unit.health>.405&&buff.remaining>1e-9)continue;
                summonBloodlust.Remove(id);RefreshAbilityMovement(id);RescaleWeaponRate(id,world.Clock);ReleaseAbilityMovementBase(id);
            }
            foreach(string key in new List<string>(summonDetection.Keys))if(summonDetection[key]<=world.Clock+1e-9)summonDetection.Remove(key);
            foreach(var net in summonNets.ToArray())
            {
                if(net.due>world.Clock+1e-9)continue;summonNets.Remove(net);summonNetHelperDeaths.Add(net.due+1);
                foreach(var target in world.Snapshot().units)
                    if(target.health>.405&&AreEnemies(net.owner,target.ownerSlot)&&!CasterHasType(target,"structure")&&SquaredDistance(net.point,target.position)<=200*200)
                    {
                        if(target.invulnerable||!WeaponTargetTypeAllowed(combatCatalog.Ability("A18L").Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType")))continue;
                        // Single helper repeated orders are reconstructed as
                        // accepted per-target Aens shots. Native multi-order
                        // cancellation and exact projectile tracking unmeasured.
                        summonNetHits.Add(new SummonNetHit{target=target.entityId,owner=net.owner,
                            due=net.due+Math.Max(.005,Math.Sqrt(SquaredDistance(net.point,target.position))/1500)});
                    }
            }
            foreach(var hit in summonNetHits.ToArray())
            {
                if(hit.due>world.Clock+1e-9)continue;summonNetHits.Remove(hit);var target=world.UnitState(hit.target);
                if(target==null||target.health<=.405||target.invulnerable)continue;
                ApplyResolvedUnitHit(0,hit.owner,target,0);target=world.UnitState(hit.target);if(target==null||target.health<=.405)continue;
                if(world.Stop(hit.target))OnAcceptedWorldOrder(hit.target);
                SetActorControl(hit.target,"native-root:A18L:B0B3",OriginalActorControlMask.Move,3,true,true);
                ApplyResolvedUnitHit(0,hit.owner,world.UnitState(hit.target),0);
            }
            foreach(double due in summonNetHelperDeaths.ToArray())if(due<=world.Clock+1e-9){summonNetHelperDeaths.Remove(due);ObserveScriptedHelperDeath();}
            foreach(var shield in summonShadowShields.ToArray())
            {
                var target=world.UnitState(shield.target);
                if(target!=null&&target.health>.405&&shield.due>world.Clock+1e-9)continue;
                summonShadowShields.Remove(shield);if(target==null)continue;
                // Event death is observed at the next bounded host step. The
                // source timer and accumulator survive caster death and pause.
                foreach(var ally in world.Snapshot().units)
                    if(ally.health>.405&&!AreEnemies(target.ownerSlot,ally.ownerSlot)&&!CasterHasType(ally,"structure")&&SquaredDistance(target.position,ally.position)<=500*500)
                        SummonDirectHeal(ally.entityId,shield.received);
                if(itemSourceShields.TryGetValue(shield.target,out var reservoir)&&reservoir.ability=="A18M")RemoveItemSourceShield(shield.target);
                else ApplyUnitAbilityOverlay(shield.target,null,new[]{"A18M"});
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemTaunt { internal int caster; internal double expires,updatedAt,triggerExpires; }
        sealed class ItemTauntApplication { internal int caster,owner; internal bool red; internal double due; internal OriginalPoint center; }
        readonly Dictionary<int,ItemTaunt> itemTaunts=new Dictionary<int,ItemTaunt>();
        readonly List<ItemTauntApplication> itemTauntApplications=new List<ItemTauntApplication>();
        readonly List<KeyValuePair<int,double>> itemTauntHeroTimers=new List<KeyValuePair<int,double>>();
        void BeginItemTaunt(int actorId,string ability)
        {
            var actor=world.UnitState(actorId);bool red=ability=="A0OS";
            // ITEMTARGET1/3: native B037/B05Y admits enemies for5/6s.
            // ST's pause/stun conditions belong to the later script callback,
            // not the native buff admission. TriggerSleepAction(.01) is real.
            foreach(var target in CasterUnits())
                if(target.health>.405&&!target.invulnerable&&!target.hidden&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&!CasterMagicImmune(target)&&
                    SquaredDistance(actor.position,target.position)<=400*400)
                {
                    itemTaunts[target.entityId]=new ItemTaunt{caster=actorId,expires=world.Clock+(red?6:5),
                        updatedAt=world.Clock,triggerExpires=world.Clock+.01+(red?8:5)};
                    SetActorControl(target.entityId,"item-taunt",OriginalActorControlMask.Cast|OriginalActorControlMask.Item,0,true,true);
                }
            itemTauntApplications.Add(new ItemTauntApplication{caster=actorId,owner=actor.ownerSlot,red=red,center=actor.position,due=world.Clock+.01});
        }
        void AdvanceItemTauntApplications()
        {
            foreach(var application in itemTauntApplications.ToArray())
            {
                if(application.due>world.Clock+1e-9)continue;
                itemTauntApplications.Remove(application);
                foreach(var target in CasterUnits())
                    if(itemTaunts.ContainsKey(target.entityId)&&!target.paused&&!ItemTauntExcludedStun(target)&&
                        SquaredDistance(application.center,target.position)<=400*400)
                    {
                        if(HasEffectiveUnitAbility(target,"B03N"))
                        {itemTaunts.Remove(target.entityId);ClearActorControl(target.entityId,"item-taunt");}
                        else EnforceItemTaunt(target.entityId);
                        // wT clears B03N's taunt, then still deals red damage.
                        if(application.red)ApplyTriggeredHit(application.caster,application.owner,target,400,OriginalTriggeredDamageMode.SpellMagic);
                        if(IsNativeHeroPredicate(target)||SourceUnitUserData(target.entityId)==2)
                            itemTauntHeroTimers.Add(new KeyValuePair<int,double>(target.entityId,world.Clock+2.5));
                    }
                if(application.red&&world.UnitState(application.caster)!=null)
                {
                    AddItemSourceShield(application.caster,400,8);
                    itemScriptActs.Add(new ItemScriptAct{actor=application.caster,owner=application.owner,ability="A1CT",period=1,due=world.Clock+1});
                }
            }
            // UT is an authored timer: pausing the recipient does not pause it.
            foreach(var timer in itemTauntHeroTimers.ToArray())
                if(timer.Value<=world.Clock+1e-9)
                {itemTauntHeroTimers.Remove(timer);itemTaunts.Remove(timer.Key);ClearActorControl(timer.Key,"item-taunt");}
        }
        bool ItemTauntExcludedStun(OriginalWorldUnitView target)
        {
            // ST tests these two buff identities, not the general weapon mask.
            foreach(string buff in new[]{"B02O","BPSE"})
            {
                if(HasEffectiveUnitAbility(target,buff))return true;
                if(!actorControls.TryGetValue(target.entityId,out var controls))continue;
                foreach(string token in controls.Keys)
                    if(token.StartsWith("native-stun:"+buff+":",StringComparison.Ordinal))return true;
                if(nativeBashes.Exists(b=>b.target==target.entityId&&b.buff==buff&&controls.ContainsKey(b.token)))return true;
            }
            return false;
        }
        void EnforceItemTaunt(int actor)
        {
            if(!itemTaunts.TryGetValue(actor,out var taunt))return;
            var unit=world.UnitState(actor);var caster=world.UnitState(taunt.caster);
            if(unit!=null&&unit.paused)taunt.expires+=Math.Max(0,world.Clock-taunt.updatedAt);
            taunt.updatedAt=world.Clock;
            if(taunt.expires<=world.Clock+1e-9||taunt.triggerExpires<=world.Clock+1e-9||unit==null||unit.health<=.405||caster==null||caster.health<=.405)
            {itemTaunts.Remove(actor);ClearActorControl(actor,"item-taunt");return;}
            if(unit.paused||ActorWeaponBlocked(actor))return;
            if(unit.order!=OriginalWorldOrder.AttackTarget||unit.targetKind!=OriginalWorldTargetKind.Unit||unit.targetId!=taunt.caster)
                if(world.TryAttackTarget(actor,OriginalWorldTargetKind.Unit,taunt.caster))CancelQueuedCastApproach(actor);
        }
        void AdvanceItemTauntOrders()
        {foreach(int actor in new List<int>(itemTaunts.Keys))EnforceItemTaunt(actor);}
    }
}

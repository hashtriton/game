using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class CasterTotem
        { internal int id,retaliate; internal double restoreAt=double.PositiveInfinity; }
        readonly Dictionary<int,CasterTotem> casterTotems=new Dictionary<int,CasterTotem>();
        readonly HashSet<int> casterSlowedUnits=new HashSet<int>();
        bool IsCasterTotem(int actor)=>casterTotems.ContainsKey(actor);

        void BeginCasterField(CasterEffect effect)
        {
            var rules=new OriginalCasterFieldRules(combatCatalog,effect.rules.abilityId);
            effect.ticks=rules.activeCallbacks;
            if(effect.rules.family==OriginalCasterFamily.Totem)
            {
                // HP20/MS1 are declarations. MP0, armor0 and body radius1 are
                // labelled sparse-ward host proxies; no native getter claim.
                int id=SpawnScriptedEnemy("n06L",effect.center,0,null,null,
                    new OriginalWorldUnitProfile{maxHealth=20,maxMana=0,moveSpeed=1,collisionRadius=1},0);
                world.SetPathingEnabled(id,false); world.ForcePosition(id,effect.center); world.SetUnitState(id,paused:true);
                effect.target=id; casterTotems.Add(id,new CasterTotem{id=id});
            }
            effect.nextAt+=1;
        }

        void TickCasterField(CasterEffect effect)
        {
            if(effect.rules.family==OriginalCasterFamily.Totem)
            {
                var unit=world.UnitState(effect.target);
                if(effect.ticks==0||unit==null||unit.health<=.405)
                {
                    if(unit!=null)world.RemoveUnit(unit.entityId);
                    casterTotems.Remove(effect.target); CompleteCasterEffect(effect);return;
                }
                foreach(var target in CasterUnits())
                    if(target.health>.405&&!target.hidden&&AreEnemies(effect.owner,target.ownerSlot)&&
                        !CasterHasType(target,"structure")&&!HasEffectiveUnitAbility(target,"A0K4")&&
                        SquaredDistance(target.position,effect.center)<=4000d*4000)
                        CasterScriptDamage(effect,target,150,3);
            }
            else if(effect.ticks==0){CompleteCasterEffect(effect);return;}
            effect.ticks--;effect.nextAt+=1;
        }

        bool ObserveCasterTotemDamage(int attacker,OriginalWorldUnitView target,double eventDamage)
        {
            if(target==null||!casterTotems.TryGetValue(target.entityId,out var totem)||target.health<=2||eventDamage<=0)return false;
            // Identical yd/asv pre-hit callback as the WARD57 native control.
            world.SetUnitState(target.entityId,invulnerable:true);
            world.UpdateProfile(target.entityId,target.profile,target.health-2,target.mana);
            totem.retaliate=attacker;totem.restoreAt=world.Clock+.0001;return true;
        }

        bool InCasterAura(OriginalWorldUnitView actor,OriginalCasterFamily family)
        {
            if(actor==null||actor.health<=.405||actor.hidden)return false;
            foreach(var effect in casterEffects)
                if(!effect.warning&&effect.rules.family==family&&AreEnemies(effect.owner,actor.ownerSlot)&&
                    SquaredDistance(effect.center,actor.position)<=effect.rules.radius*effect.rules.radius)return true;
            return false;
        }
        double CasterAuraMovementBonus(int actor)=>InCasterAura(world?.UnitState(actor),OriginalCasterFamily.SlowAura)?-.5:0;

        void AdvanceCasterFields()
        {
            foreach(var totem in casterTotems.Values)
                if(totem.restoreAt<=world.Clock+1e-9)
                {
                    totem.restoreAt=double.PositiveInfinity;
                    var unit=world.UnitState(totem.id);if(unit==null||unit.health<=.405)continue;
                    world.SetUnitState(totem.id,invulnerable:false);
                    if(totem.retaliate!=0&&world.TryAttackTarget(totem.retaliate,OriginalWorldTargetKind.Unit,totem.id))
                        OnAcceptedWorldOrder(totem.retaliate);
                }
            // Immediate membership and no linger are explicit host policies.
            // Auras do not accumulate an additional multiplier every update.
            var slowed=new HashSet<int>();
            foreach(var actor in CasterUnits())if(CasterAuraMovementBonus(actor.entityId)!=0)slowed.Add(actor.entityId);
            var changed=new HashSet<int>(casterSlowedUnits);changed.SymmetricExceptWith(slowed);
            casterSlowedUnits.Clear();casterSlowedUnits.UnionWith(slowed);
            foreach(int id in changed)
            {
                if(world.UnitState(id)!=null)
                {
                    if(slowed.Contains(id))CaptureAbilityMovementBase(id);
                    RefreshAbilityMovement(id);
                }
                if(!slowed.Contains(id))ReleaseAbilityMovementBase(id);
            }
        }

        // Actual SPELL_EFFECT only, never order acceptance or visual MarkCast.
        void NotifyNativeSpellEffect(int actorId,string abilityId)
        {
            RecordRoundSpellEffect(actorId,abilityId);
            ObserveCurseSpellEffect(actorId,abilityId);
            pendingItemWindWalkStrikes.Remove(actorId);
            if(abilityId!="AIv1"&&abilityId!="A01X"&&abilityId!="A19K")RevealItemInvisibility(actorId);
            OnPyroSpellEffect(actorId);
            ChargeErosAmulets(actorId,abilityId);
            ObserveWarpathSpell(actorId,abilityId);
            var actor=world?.UnitState(actorId);
            if(!InCasterAura(actor,OriginalCasterFamily.SpellCurseAura)||!IsNativeHeroPredicate(actor)||
                HasEffectiveUnitAbility(actor,"A0K4")||!OriginalCasterFieldRules.SpellTriggersCurse(abilityId,actor.rawcode))return;
            // MD is the arena's independent Player11 helper, not the field's
            // caster or the punished hero. Keep source-specific procs separate.
            ApplyTriggeredHit(0,0,actor,1200,OriginalTriggeredDamageMode.SpellMagic);
        }

        OriginalPoint CasterAuraPointOrder(int actorId,int nativeOrder,OriginalPoint point)
        {
            var actor=world?.UnitState(actorId);
            if((nativeOrder!=851971&&nativeOrder!=851986&&nativeOrder!=851983)||
                !InCasterAura(actor,OriginalCasterFamily.ReverseOrderAura)||!IsNativeHeroPredicate(actor))return point;
            return new OriginalPoint(2*actor.position.x-point.x,2*actor.position.y-point.y);
        }
    }
}

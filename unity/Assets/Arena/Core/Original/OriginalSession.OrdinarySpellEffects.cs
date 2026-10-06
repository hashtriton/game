using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrdinaryDelayedEffect
        {
            internal int actor,owner,target;
            internal string ability;
            internal double due;
            internal bool healing;
        }
        readonly List<OrdinaryDelayedEffect> ordinaryDelayedEffects=new List<OrdinaryDelayedEffect>();

        bool ResolveOrdinarySpellEffect(OriginalWorldUnitView actor,OriginalWorldUnitView target,OriginalCombatDefinition ability)
        {
            string code=ability.Text("code");
            if(code!="AUdc"&&code!="AEmb"&&code!="AUfn")return false;
            if(!OrdinarySpellTarget(actor,target,ability.id))return true;
            if(code=="AUfn")
            {
                // Native metadata Ufn1=area damage, Ufn2=target damage;
                // A09S NeutralAbilityStrings1330 names both contributions.
                // Atomic area enumeration and damage-before-cold ordering are
                // host policies; helper A168's zero callbacks are not copied.
                var center=target.position;int primary=target.entityId;
                foreach(var previous in world.Snapshot().units)
                {
                    var victim=world.UnitState(previous.entityId);
                    if(!OrdinarySpellTarget(actor,victim,ability.id)||SquaredDistance(victim.position,center)>ability.Number("Area1")*ability.Number("Area1"))continue;
                    double damage=ability.Number("DataA1")+(victim.entityId==primary?ability.Number("DataB1"):0);
                    ApplyNativeTriggeredHit(actor.entityId,actor.ownerSlot,victim,damage,OriginalTriggeredDamageMode.SpellMagic);
                    victim=world.UnitState(victim.entityId);
                    if(OrdinarySpellTarget(actor,victim,ability.id))ApplyArcherNativeBuff(victim.entityId,new OriginalArcherDebuffRules(combatCatalog,native,ability.id,1));
                }
                return true;
            }
            // A04T source profile NeutralAbilityFunc943 declares1100 speed.
            // Emb2 is bolt delay, Emb3 visual lifetime, never a damage factor.
            // Initial-distance scheduling transfers declared flight speed;
            // native homing/callback micro-order is not claimed measured.
            double delay=code=="AUdc"?Math.Max(.005,Math.Sqrt(SquaredDistance(actor.position,target.position))/1100):ability.Number("DataB1");
            ordinaryDelayedEffects.Add(new OrdinaryDelayedEffect{actor=actor.entityId,owner=actor.ownerSlot,target=target.entityId,
                ability=ability.id,due=world.Clock+delay,healing=code=="AUdc"&&!AreEnemies(actor.ownerSlot,target.ownerSlot)});
            return true;
        }
        void AdvanceOrdinarySpellEffects()
        {
            foreach(var effect in ordinaryDelayedEffects.ToArray())
            {
                if(effect.due>world.Clock+1e-9)continue;ordinaryDelayedEffects.Remove(effect);
                var target=world.UnitState(effect.target);var a=combatCatalog.Ability(effect.ability);
                if(target==null||target.health<=.405||target.hidden)continue;
                bool enemy=AreEnemies(effect.owner,target.ownerSlot);
                if(effect.healing)
                {
                    if(enemy||!CasterHasType(target,"undead"))continue;
                    world.UpdateProfile(target.entityId,target.profile,Math.Min(target.profile.maxHealth,target.health+a.Number("DataA1")),target.mana);
                    continue;
                }
                if(!enemy||target.invulnerable||CasterMagicImmune(target))continue;
                if(a.Text("code")=="AUdc")
                {
                    if(CasterHasType(target,"undead"))continue;
                    // A04T tooltip explicitly specifies half DataA damage.
                    ApplyNativeTriggeredHit(effect.actor,effect.owner,target,a.Number("DataA1")*.5,OriginalTriggeredDamageMode.SpellMagic);
                }
                else
                {
                    double burned=Math.Min(target.mana,a.Number("DataA1"));if(burned<=0)continue;
                    if(!world.UpdateProfile(target.entityId,target.profile,target.health,target.mana-burned))continue;
                    // A0AN source tooltip1372: damage equals actual mana burned.
                    // Debit-before-damage and low-mana clipping are declared
                    // host reconstruction; no artificial native zero event.
                    ApplyNativeTriggeredHit(effect.actor,effect.owner,target,burned,OriginalTriggeredDamageMode.SpellMagic);
                }
            }
        }
    }
}

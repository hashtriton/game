using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrdinaryCast { internal int actor,target; internal string ability; internal double due; }
        sealed class OrdinaryBuff
        {
            internal string ability,buff;
            internal double remaining,updatedAt,movement,attackSlow,armor,damageReduction,healthRate;
        }
        sealed class OrdinaryBolt { internal int actor,owner,target; internal string ability; internal double due; }
        readonly Dictionary<int,OrdinaryCast> ordinaryCasts=new Dictionary<int,OrdinaryCast>();
        readonly Dictionary<string,double> ordinaryCooldowns=new Dictionary<string,double>();
        readonly Dictionary<int,Dictionary<string,OrdinaryBuff>> ordinaryBuffs=new Dictionary<int,Dictionary<string,OrdinaryBuff>>();
        readonly List<OrdinaryBolt> ordinaryBolts=new List<OrdinaryBolt>();

        bool OrdinarySpellSupported(string ability)
        {
            var a=combatCatalog.Ability(ability);if(a==null || a.overrides.Length!=0)return false;
            string code=a.Text("code");
            bool Required(params string[] fields)
            {foreach(var field in fields)if(!a.TryNumber(field,out double value,out _) || value<0)return false;return true;}
            if(!Required("Cost1") || string.IsNullOrEmpty(a.Text("targs1")))return false;
            if(code=="AEim")return ability=="A05Y"&&Required("Area1","DataA1","DataB1","DataC1","Dur1")&&
                (!a.TryNumber("Cool1",out double toggleCooldown,out _)||toggleCooldown==0);
            if(!Required("Cool1"))return false;
            if(code=="AUdc")return ability=="A04T"&&Required("Rng1","DataA1");
            if(code=="AEmb")return Required("Rng1","DataA1","DataB1","DataC1");
            if(code=="AUcs")return (ability=="A0RA"||ability=="A0RB")&&Required("Rng1","Area1","DataC1","DataD1")&&
                (!a.TryNumber("DataA1",out double swarmDamage,out _)||swarmDamage==0)&&
                (!a.TryNumber("DataB1",out double swarmCap,out _)||swarmCap==0);
            if(!Required("Dur1","HeroDur1"))return false;
            switch(code)
            {
                case "ACtc":return Required("Area1","DataA1","DataC1","DataD1");
                case "AOws":return Required("Area1","DataA1");
                case "ANht":return Required("Area1","DataA1");
                case "Aslo":return Required("Rng1","DataA1","DataB1");
                case "Acri":return Required("Rng1","DataA1","DataB1","DataC1");
                case "Afae":case "Arej":return Required("Rng1","DataA1");
                case "AUfn":return ability=="A09S"&&Required("Rng1","Area1","DataA1","DataB1");
                case "AHtb":case "ACtb":return Required("Rng1","DataA1") && OrdinaryBoltSpeed(ability)>0;
                case "AEer":return ability=="A074"&&Required("Rng1","DataA1");
                case "AEsh":return ability=="A07A"&&Required("Rng1","Cast1","DataA1","DataB1","DataC1","DataD1","DataE1");
                case "Aprg":return ability=="ACpu"&&Required("Rng1","DataA1","DataC1");
                case "Aply":return ability=="A0B1"&&Required("Rng1","DataA1");
                default:return false;
            }
        }
        static double OrdinaryBoltSpeed(string ability)
        {
            // Exact source profile declarations, not code-handler defaults:
            // HumanAbilityFunc562/648, NeutralAbilityFunc1053/3782.
            return ability=="A06Z"||ability=="A03E"?700:ability=="A078"||ability=="A1D9"?1000:0;
        }
        bool OrdinarySpellTarget(OriginalWorldUnitView actor,OriginalWorldUnitView target,string ability)
        {
            if(actor==null || target==null || target.health<=.405 || target.hidden)return false;
            var a=combatCatalog.Ability(ability);string code=a.Text("code");bool healing=code=="Arej";
            if(code=="AUdc")
            {
                bool undead=CasterHasType(target,"undead");bool enemy=AreEnemies(actor.ownerSlot,target.ownerSlot);
                if(enemy==undead)return false;
                healing=!enemy;
            }
            else if(AreEnemies(actor.ownerSlot,target.ownerSlot)==healing)return false;
            string mask=a.Text("targs1")??"";
            if(target.invulnerable && !mask.Contains("invu"))return false;
            if(!WeaponTargetTypeAllowed(mask,combatCatalog.Unit(target.rawcode).Text("targType")))return false;
            if(mask.Contains("organic") && (CasterHasType(target,"mechanical")||CasterHasType(target,"structure")))return false;
            if(!healing && CasterMagicImmune(target))return false;
            if(code=="AEmb"&&target.mana<=0)return false;
            return target.entityId!=actor.entityId || mask.Contains("self");
        }

        bool TryStartOrdinaryNativeSpell(int actorId,string ability,int targetId)
        {
            var actor=world.UnitState(actorId);var target=world.UnitState(targetId);
            if(!OrdinarySpellSupported(ability) || actor==null || actor.health<=.405 || actor.hidden || actor.paused ||
                ActorCastBlocked(actorId) || AbilityControlsActor(actorId) || !HasEffectiveUnitAbility(actor,ability) ||
                !OrdinarySpellTarget(actor,target,ability) || !CanSeeForCombat(actor.ownerSlot,target))return false;
            var a=combatCatalog.Ability(ability);bool area=a.Text("code")=="ACtc"||a.Text("code")=="AOws"||a.Text("code")=="ANht"||a.Text("code")=="AEim";
            if(ordinaryImmolations.ContainsKey(actorId)&&a.Text("code")=="AEim")return false;
            double range=a.Number(area?"Area1":"Rng1");
            if(SquaredDistance(actor.position,target.position)>range*range || actor.mana<a.Number("Cost1") ||
                ordinaryCooldowns.TryGetValue(actorId+":"+ability,out double end)&&world.Clock+1e-9<end)return false;
            // Sparse cast points are not defaulted to a convenient delay.
            // WAVECAST1 supplies the exact n009/n019 zero controls.
            double castPoint;
            if(ability=="A05Y"||ability=="A0B1")castPoint=0; // ORDINARY2 exact native instant effects.
            else if(!OriginalCasterRules.TryCastPoint(combatCatalog,actor.rawcode,out castPoint))return false;
            RevealItemInvisibility(actorId);world.Stop(actorId);OnAcceptedWorldOrder(actorId);world.MarkCast(actorId);
            if(weaponCycles.TryGetValue(actorId,out var cycle))cycle.winding=false;
            var cast=new OrdinaryCast{actor=actorId,target=targetId,ability=ability,due=world.Clock+castPoint};
            ordinaryCasts[actorId]=cast;
            if(castPoint==0)CompleteOrdinaryNativeSpell(cast);
            return true;
        }

        void CompleteOrdinaryNativeSpell(OrdinaryCast cast)
        {
            ordinaryCasts.Remove(cast.actor);var a=combatCatalog.Ability(cast.ability);
            if(!world.TrySpendMana(cast.actor,a.Number("Cost1")))return;
            // AEim is a toggle with no declared cooldown. One active instance
            // prevents repeated AI debit; reactivation after exhaustion is a
            // declared family policy, not measured cooldown expiry.
            ordinaryCooldowns[cast.actor+":"+cast.ability]=cast.due+(a.Text("code")=="AEim"?0:a.Number("Cool1"));
            // WAVECAST1 A046 EFFECT is immediate, FINISH/END follows .51.
            // Explicit new orders can replace this attack recovery as usual.
            if(cast.ability=="A046"&&world.UnitState(cast.actor)?.rawcode=="n009")nativeAttackRecovery[cast.actor]=cast.due+.51;
            double recovery=OrdinaryObservedRecovery(cast.ability);if(recovery>0)nativeAttackRecovery[cast.actor]=cast.due+recovery;
            NotifyNativeSpellEffect(cast.actor,cast.ability);ResolveOrdinaryNativeSpell(cast.actor,cast.ability,cast.target);
        }

        void ResolveOrdinaryNativeSpell(int actorId,string ability,int targetId)
        {
            var actor=world.UnitState(actorId);var target=world.UnitState(targetId);
            if(actor==null || !OrdinarySpellSupported(ability))return;
            var a=combatCatalog.Ability(ability);string code=a.Text("code");
            if(code=="AUcs"){BeginOrdinaryCarrionSwarm(actor,target,a);return;}
            if(ability=="A04E"||ability=="A068")ApplyOrdinaryRejuvenationCompanion(actor);
            if(ResolveOrdinaryNativeStatus(actor,target,a))return;
            if(ResolveOrdinarySpellEffect(actor,target,a))return;
            if(code=="AHtb"||code=="ACtb")
            {
                if(!OrdinarySpellTarget(actor,target,ability))return;
                ordinaryBolts.Add(new OrdinaryBolt{actor=actorId,owner=actor.ownerSlot,target=targetId,ability=ability,
                    due=world.Clock+Math.Max(.005,Math.Sqrt(SquaredDistance(actor.position,target.position))/OrdinaryBoltSpeed(ability))});return;
            }
            bool area=code=="ACtc"||code=="AOws"||code=="ANht";
            var candidates=area?world.Snapshot().units:new[]{target};
            foreach(var previous in candidates)
            {
                var victim=previous==null?null:world.UnitState(previous.entityId);
                if(!OrdinarySpellTarget(actor,victim,ability) || area&&SquaredDistance(actor.position,victim.position)>a.Number("Area1")*a.Number("Area1"))continue;
                if(code=="ACtc"||code=="AOws")
                {
                    ApplyNativeTriggeredHit(actorId,actor.ownerSlot,victim,a.Number("DataA1"),OriginalTriggeredDamageMode.SpellMagic);
                    victim=world.UnitState(victim.entityId);if(!OrdinarySpellTarget(actor,victim,ability))continue;
                }
                double duration=a.Number(IsNativeHeroPredicate(victim)?"HeroDur1":"Dur1");
                if(code=="AOws")AddTimedNativeStun(victim.entityId,a.Text("BuffID1"),actorId,duration);
                else AddOrdinaryBuff(victim,a,duration);
            }
        }
        void AddOrdinaryBuff(OriginalWorldUnitView target,OriginalCombatDefinition a,double duration)
        {
            if(duration<=0)return;
            var state=new OrdinaryBuff{ability=a.id,buff=a.Text("BuffID1"),remaining=duration,updatedAt=world.Clock};
            switch(a.Text("code"))
            {
                // Effective1.26 AbilityMetaData Cri1/2/3, Slo1/2, Fae1,
                // Ctc1/3/4, Roa1, Rej1; matching WorldEditStrings6030..6249.
                case "ACtc":state.movement=-a.Number("DataC1");state.attackSlow=a.Number("DataD1");break;
                case "Aslo":state.movement=-a.Number("DataA1");state.attackSlow=a.Number("DataB1");break;
                case "Acri":state.movement=-a.Number("DataA1");state.attackSlow=a.Number("DataB1");state.damageReduction=a.Number("DataC1");break;
                case "Afae":state.armor=-a.Number("DataA1");break;
                case "ANht":state.damageReduction=a.Number("DataA1");break;
                case "Arej":state.healthRate=a.Number("DataA1")/duration;break;
            }
            if(string.IsNullOrEmpty(state.buff))return;
            CaptureAbilityMovementBase(target.entityId);
            if(!ordinaryBuffs.TryGetValue(target.entityId,out var buffs))ordinaryBuffs[target.entityId]=buffs=new Dictionary<string,OrdinaryBuff>();
            // Same-buff latest refresh, additive different-buff slow and
            // continuous rejuvenation are explicit host policies. Runtime
            // A103 validates only its own white/flat separation, not these ranks.
            buffs[state.buff]=state;RefreshAbilityMovement(target.entityId);RescaleWeaponRate(target.entityId,world.Clock);
        }
        double OrdinaryBuffValue(int actor,int field)
        {
            double value=0;if(ordinaryBuffs.TryGetValue(actor,out var buffs))foreach(var buff in buffs.Values)
                value+=field==0?buff.movement:field==1?buff.attackSlow:field==2?buff.armor:buff.damageReduction;
            return value;
        }
        double OrdinaryMovementBonus(int actor)=>OrdinaryBuffValue(actor,0)+OrdinaryStatusMovement(actor);
        double OrdinaryAttackSlow(int actor)=>OrdinaryBuffValue(actor,1)+OrdinaryStatusAttackSlow(actor);
        double OrdinaryArmorDelta(int actor)=>OrdinaryBuffValue(actor,2);
        double OrdinaryWhiteDamage(int actor,double damage)
        {double reduction=Math.Max(0,Math.Min(1,OrdinaryBuffValue(actor,3)));return reduction>0?Math.Floor(damage*(1-reduction)):damage;}
        void RemoveOrdinaryNativeBuffs(int actor)
        {RemoveOrdinaryStatuses(actor);if(!ordinaryBuffs.Remove(actor))return;RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);ReleaseAbilityMovementBase(actor);}
        void RemoveOrdinaryNegativeBuffs(int actor)
        {
            RemoveOrdinaryStatuses(actor);
            if(!ordinaryBuffs.TryGetValue(actor,out var buffs))return;
            foreach(var buff in new List<OrdinaryBuff>(buffs.Values))
                if(combatCatalog.Ability(buff.ability).Text("code")!="Arej")buffs.Remove(buff.buff);
            if(buffs.Count==0)ordinaryBuffs.Remove(actor);
            RefreshAbilityMovement(actor);RescaleWeaponRate(actor,world.Clock);ReleaseAbilityMovementBase(actor);
        }

        void AdvanceOrdinaryNativeSpells()
        {
            AdvanceOrdinaryNativeStatuses();
            AdvanceOrdinaryCarrionSwarms();
            AdvanceOrdinarySpellEffects();
            foreach(var pair in new List<KeyValuePair<int,Dictionary<string,OrdinaryBuff>>>(ordinaryBuffs))
            {
                var actor=world.UnitState(pair.Key);if(actor==null || actor.health<=.405){RemoveOrdinaryNativeBuffs(pair.Key);continue;}
                bool changed=false;
                foreach(var buff in new List<OrdinaryBuff>(pair.Value.Values))
                {
                    double elapsed=Math.Max(0,world.Clock-buff.updatedAt);buff.updatedAt=world.Clock;
                    if(actor.paused)continue; // Native pause-family transfer, not a new rawcode probe.
                    double applied=Math.Min(elapsed,buff.remaining);buff.remaining-=elapsed;
                    if(buff.healthRate>0)
                    {
                        actor=world.UnitState(pair.Key);if(actor!=null && actor.health>.405)
                            world.UpdateProfile(actor.entityId,actor.profile,Math.Min(actor.profile.maxHealth,actor.health+applied*buff.healthRate),actor.mana);
                    }
                    if(buff.remaining<=1e-9){pair.Value.Remove(buff.buff);changed=true;}
                }
                if(changed){if(pair.Value.Count==0)ordinaryBuffs.Remove(pair.Key);RefreshAbilityMovement(pair.Key);RescaleWeaponRate(pair.Key,world.Clock);ReleaseAbilityMovementBase(pair.Key);}
            }
            foreach(var cast in new List<OrdinaryCast>(ordinaryCasts.Values))
            {
                var actor=world.UnitState(cast.actor);var target=world.UnitState(cast.target);
                if(actor==null || actor.health<=.405 || actor.hidden || actor.paused || ActorCastBlocked(cast.actor) ||
                    !HasEffectiveUnitAbility(actor,cast.ability) || !OrdinarySpellTarget(actor,target,cast.ability) || !CanSeeForCombat(actor.ownerSlot,target))
                {ordinaryCasts.Remove(cast.actor);continue;}
                if(cast.due>world.Clock+1e-9)continue;
                CompleteOrdinaryNativeSpell(cast);
            }
            foreach(var bolt in ordinaryBolts.ToArray())
            {
                if(bolt.due>world.Clock+1e-9)continue;ordinaryBolts.Remove(bolt);
                var target=world.UnitState(bolt.target);var a=combatCatalog.Ability(bolt.ability);
                if(target==null || target.health<=.405 || target.hidden || target.invulnerable || CasterMagicImmune(target) || !AreEnemies(bolt.owner,target.ownerSlot))continue;
                ApplyNativeTriggeredHit(bolt.actor,bolt.owner,target,a.Number("DataA1"),OriginalTriggeredDamageMode.SpellMagic);
                target=world.UnitState(bolt.target);if(target!=null && target.health>.405 && !target.invulnerable && !CasterMagicImmune(target))
                    AddTimedNativeStun(target.entityId,a.Text("BuffID1"),bolt.actor,a.Number(IsNativeHeroPredicate(target)?"HeroDur1":"Dur1"));
                // Authored missile speed with initial-distance scheduling;
                // native tracking and extra zero callbacks remain unmeasured.
            }
        }

        void SelectOrdinaryNativeSpellOrders()
        {
            var units=world.Snapshot().units;
            foreach(var actor in units)
            {
                if(actor.kind!=OriginalWorldUnitKind.Enemy || actor.ownerSlot!=0 || actor.health<=.405 || actor.hidden || actor.paused ||
                    ActorCastBlocked(actor.entityId)||AbilityControlsActor(actor.entityId))continue;
                foreach(var ability in NativeUnitAbilities(actor))
                {
                    if(!OrdinarySpellSupported(ability.id))continue;
                    string buffId=ability.Text("BuffID1");
                    OriginalWorldUnitView selected=null;double best=double.MaxValue;
                    foreach(var target in units)
                    {
                        if(!OrdinarySpellTarget(actor,target,ability.id) || !CanSeeForCombat(actor.ownerSlot,target) || ability.Text("code")=="Arej"&&target.health>=target.profile.maxHealth)continue;
                        // Damage-only spells such as AEmb/AUdc have no BuffID.
                        // An unrelated existing buff must not block their AI scan.
                        if(!string.IsNullOrEmpty(buffId))
                        {
                            if(ordinaryBuffs.TryGetValue(target.entityId,out var buffs)&&buffs.ContainsKey(buffId))continue;
                            if(HasOrdinaryNativeStatus(target.entityId,buffId))continue;
                        }
                        double d=SquaredDistance(actor.position,target.position);if(d<best){best=d;selected=target;}
                    }
                    if(selected!=null && TryStartOrdinaryNativeSpell(actor.entityId,ability.id,selected.entityId))break;
                }
            }
        }
    }
}

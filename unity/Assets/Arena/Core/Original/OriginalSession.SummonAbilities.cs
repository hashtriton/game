using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalUnitAbilityView
    {
        public int entityId;
        public OriginalAbilityView[] abilities=Array.Empty<OriginalAbilityView>();
    }
    public sealed partial class OriginalSession
    {
        sealed class SummonCast { internal int actor,target;internal string ability;internal OriginalPoint point;internal double due; }
        readonly Dictionary<int,SummonCast> summonCasts=new Dictionary<int,SummonCast>();
        bool SummonControlsActor(int id)=>summonCasts.ContainsKey(id);
        void CancelSummonCastOnOrder(int id)=>summonCasts.Remove(id);
        bool SummonSpecialAbility(string id)=>id=="A0FD"||id=="A030"||id=="A0A2"||id=="A0WH"||id=="A18I"||id=="A18J";
        bool SummonAreaAbility(string code)=>code=="AOws"||code=="ACtc"||code=="ANht"||code=="AEim";
        OriginalAbilityTargetMode SummonTargetMode(OriginalCombatDefinition a)
        {
            if(a.id=="A18J"||a.Text("code")=="AUcs")return OriginalAbilityTargetMode.Point;
            if(a.id=="A0WH"||IsScriptedWaveSpell(a.id)||SummonAreaAbility(a.Text("code")))return OriginalAbilityTargetMode.None;
            return OriginalAbilityTargetMode.Unit;
        }
        bool SummonAbilityRules(OriginalWorldUnitView actor,OriginalCombatDefinition a,out double cost,out double cooldown,out double range,out double castPoint)
        {
            cost=cooldown=range=castPoint=0;
            if(a==null||a.overrides.Length!=0)return false;
            bool instant=a.id=="A05Y"||a.id=="A0B1"||IsScriptedWaveSpell(a.id)&&a.id!="A1DB";
            if(!instant&&!OriginalCasterRules.TryCastPoint(combatCatalog,actor.rawcode,out castPoint))return false;
            // ORDINARY2 exact A05Y toggle has no cooldown; this exception is
            // scoped to that observed alias, never a generic missing default.
            if(!a.TryNumber("Cool1",out cooldown,out _)&&a.id!="A05Y"||cooldown<0)return false;
            // The exact sparse AItb alias transfers the stock native zero cost.
            // It is a declared family policy, not a native A0WH measurement.
            if(!a.TryNumber("Cost1",out cost,out _)&&a.id!="A0WH")return false;
            if(cost<0)return false;
            if(IsScriptedWaveSpell(a.id))return a.Text("code")== (a.id=="A1DB"?"Aroa":"Absk");
            if(!SummonSpecialAbility(a.id)&&!OrdinarySpellSupported(a.id))return false;
            if(SummonTargetMode(a)!=OriginalAbilityTargetMode.None&&!a.TryNumber("Rng1",out range,out _))return false;
            if(a.id=="A0FD")return a.Text("code")=="Ahea"&&a.Number("DataA1")==50&&cost==5&&cooldown==1.5&&range==500;
            if(a.id=="A030")return a.Text("code")=="Ablo"&&a.Text("BuffID1")=="Bblo"&&a.Number("DataA1")==.5&&a.Number("DataB1")==.1&&a.Number("Dur1")==20&&a.Number("HeroDur1")==20;
            if(a.id=="A0A2")return a.Text("code")=="AUsl"&&a.Text("BuffID1")=="BUsl,BUsp,Bust"&&a.Number("DataA1")==2&&a.Number("Dur1")==12&&a.Number("HeroDur1")==4;
            if(a.id=="A0WH")return a.Text("code")=="AItb"&&a.Number("DataA1")==3&&a.Number("Area1")==1000&&a.Number("Dur1")==8&&cooldown==14&&cost==0;
            if(a.id=="A18I"||a.id=="A18J")return a.Text("code")=="ANcl"&&cost==50&&cooldown==25&&range==700;
            return true;
        }
        OriginalAbilityView[] SummonAbilityViews(OriginalWorldUnitView actor)
        {
            var result=new List<OriginalAbilityView>();
            foreach(string id in EffectiveUnitAbilityIds(actor))
            {
                var a=combatCatalog.Ability(id);if(a==null)continue;
                var caster=OriginalCasterRules.ForUnit(combatCatalog,actor.rawcode);
                bool scripted=caster!=null&&caster.abilityId==id;
                if(!SummonSpecialAbility(id)&&!IsScriptedWaveSpell(id)&&!OrdinarySpellSupported(id)&&!scripted&&string.IsNullOrEmpty(a.Text("Order")))continue;
                var view=new OriginalAbilityView{id=id,castAbilityId=id,name=a.name,rank=1,targetMode=SummonTargetMode(a),code=OriginalAbilityUseCode.RuleUnavailable};
                bool known=SummonAbilityRules(actor,a,out double cost,out _,out _,out _);
                if(scripted)
                {known=caster.scriptImplemented&&OriginalCasterRules.TryCastPoint(combatCatalog,actor.rawcode,out _);cost=caster.manaCost;view.targetMode=OriginalAbilityTargetMode.Point;}
                view.implemented=known;view.manaCostKnown=known;view.manaCost=known?cost:0;
                if(id=="A05Y"&&ordinaryImmolations.ContainsKey(actor.entityId)){view.toggledOn=true;view.manaCost=cost=0;}
                view.cooldownRemaining=ordinaryCooldowns.TryGetValue(actor.entityId+":"+id,out double until)?Math.Max(0,until-world.Clock):0;
                if(IsScriptedWaveSpell(id)&&waveSpellCooldowns.TryGetValue(actor.entityId+":"+id,out double waveUntil))view.cooldownRemaining=Math.Max(0,waveUntil-world.Clock);
                if(scripted&&casterCooldowns.TryGetValue(actor.entityId,out var casterUntil))view.cooldownRemaining=Math.Max(view.cooldownRemaining,casterUntil-world.Clock);
                if(known)view.code=view.cooldownRemaining>1e-9?OriginalAbilityUseCode.Cooldown:actor.mana<cost?OriginalAbilityUseCode.NoMana:OriginalAbilityUseCode.Ready;
                if(actor.health<=.405)view.code=OriginalAbilityUseCode.Dead;
                else if(actor.paused)view.code=OriginalAbilityUseCode.Paused;
                else if(actor.hidden||ActorCastBlocked(actor.entityId)||AbilityControlsActor(actor.entityId)||SummonControlsActor(actor.entityId))view.code=OriginalAbilityUseCode.Busy;
                result.Add(view);
            }
            return result.ToArray();
        }
        OriginalUnitAbilityView[] UnitAbilityViews()
        {
            var result=new List<OriginalUnitAbilityView>();if(world==null)return result.ToArray();
            foreach(var actor in world.Snapshot().units)
                if(actor.kind==OriginalWorldUnitKind.Summon)
                    result.Add(new OriginalUnitAbilityView{entityId=actor.entityId,abilities=SummonAbilityViews(actor)});
            return result.ToArray();
        }
        bool SummonAbilityTarget(OriginalWorldUnitView actor,OriginalWorldUnitView target,string id)
        {
            if(target==null||target.health<=.405||!CanSeeForCombat(actor.ownerSlot,target))return false;
            if(!SummonSpecialAbility(id))return OrdinarySpellTarget(actor,target,id);
            var a=combatCatalog.Ability(id);bool ally=id=="A0FD"||id=="A030"||id=="A18I";
            if(AreEnemies(actor.ownerSlot,target.ownerSlot)==ally)return false;
            if(!WeaponTargetTypeAllowed(a.Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType")))return false;
            if(id!="A18I"&&(CasterHasType(target,"mechanical")||CasterHasType(target,"structure")))return false;
            if(id=="A0FD")return !CasterHasType(target,"ancient")&&target.health<target.profile.maxHealth;
            return !target.invulnerable&&(ally||!CasterMagicImmune(target));
        }
        OriginalSessionReplyCode ApplySummonAbilityCommand(Player player,OriginalSessionCommand command)
        {
            var actor=world?.UnitState(command.actorEntityId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Summon||actor.ownerSlot!=player.slot)return OriginalSessionReplyCode.InvalidCommand;
            if(!Started||pendingDuel||match.Phase==OriginalMatchPhase.Won||match.Phase==OriginalMatchPhase.Lost)return OriginalSessionReplyCode.NotReady;
            var view=Array.Find(SummonAbilityViews(actor),a=>a.id==command.skillId);
            if(view==null)return OriginalSessionReplyCode.InvalidCommand;
            if(view.code==OriginalAbilityUseCode.RuleUnavailable)return OriginalSessionReplyCode.RuleUnavailable;
            if(view.code!=OriginalAbilityUseCode.Ready)return OriginalSessionReplyCode.NotReady;
            var a=combatCatalog.Ability(view.id);var caster=OriginalCasterRules.ForUnit(combatCatalog,actor.rawcode);
            bool scripted=caster!=null&&caster.abilityId==view.id;
            double range=scripted?caster.castRange:view.targetMode==OriginalAbilityTargetMode.None?0:a.Number("Rng1");
            var point=new OriginalPoint(command.x,command.y);
            if(view.targetMode==OriginalAbilityTargetMode.Unit)
            {
                var target=world.UnitState(command.targetId);
                if(command.targetKind!=OriginalWorldTargetKind.Unit||!SummonAbilityTarget(actor,target,view.id))return OriginalSessionReplyCode.InvalidCommand;
                if(SquaredDistance(actor.position,target.position)>range*range)return OriginalSessionReplyCode.NotReady;
            }
            else if(command.targetKind!=OriginalWorldTargetKind.None||command.targetId!=0)return OriginalSessionReplyCode.InvalidCommand;
            else if(view.targetMode==OriginalAbilityTargetMode.Point&&(!ValidPoint(point.x,point.y)||SquaredDistance(actor.position,point)>range*range))return OriginalSessionReplyCode.NotReady;
            // Source helper identities and shield declarations are validated
            // before accepting an order or touching the actor's resources.
            ValidateSummonSourceAbility(a.id);
            if(a.id=="A05Y"&&ordinaryImmolations.ContainsKey(actor.entityId))
            {
                // Manual toggle-off removes the running effect without a
                // second EFFECT, cost or cooldown. AI never issues this order.
                ordinaryImmolations.Remove(actor.entityId);OnAcceptedWorldOrder(actor.entityId);
                return OriginalSessionReplyCode.Accepted;
            }
            if(IsScriptedWaveSpell(a.id))return TryStartWaveSpell(actor.entityId,a.id)?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.NotReady;
            if(scripted)
            {
                ValidateCasterHelper(caster);OnAcceptedWorldOrder(actor.entityId);RevealItemInvisibility(actor.entityId);
                return TryStartCaster(actor,point)?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.NotReady;
            }
            SummonAbilityRules(actor,a,out _,out _,out _,out double castPoint);
            RevealItemInvisibility(actor.entityId);world.Stop(actor.entityId);OnAcceptedWorldOrder(actor.entityId);world.MarkCast(actor.entityId);
            if(weaponCycles.TryGetValue(actor.entityId,out var cycle))cycle.winding=false;
            var cast=new SummonCast{actor=actor.entityId,ability=view.id,target=command.targetId,point=point,due=world.Clock+castPoint};
            summonCasts[actor.entityId]=cast;if(castPoint==0)CompleteSummonAbility(cast);
            return OriginalSessionReplyCode.Accepted;
        }
        void CompleteSummonAbility(SummonCast cast)
        {
            summonCasts.Remove(cast.actor);var actor=world.UnitState(cast.actor);var a=combatCatalog.Ability(cast.ability);
            if(actor==null||!SummonAbilityRules(actor,a,out double cost,out double cooldown,out _,out _)||!world.TrySpendMana(cast.actor,cost))return;
            ordinaryCooldowns[cast.actor+":"+cast.ability]=world.Clock+cooldown;
            NotifyNativeSpellEffect(cast.actor,cast.ability);actor=world.UnitState(cast.actor);if(actor==null)return;
            if(SummonSpecialAbility(cast.ability))ResolveSummonSourceAbility(actor,cast.ability,cast.target,cast.point);
            else if(a.Text("code")=="AUcs")BeginOrdinaryCarrionSwarmAtPoint(actor,cast.point,a);
            else ResolveOrdinaryNativeSpell(cast.actor,cast.ability,cast.target);
        }
        void AdvanceSummonAbilities()
        {
            foreach(var cast in new List<SummonCast>(summonCasts.Values))
            {
                var actor=world.UnitState(cast.actor);var a=combatCatalog.Ability(cast.ability);
                if(actor==null||actor.health<=.405||actor.hidden||actor.paused||ActorCastBlocked(cast.actor)||!HasEffectiveUnitAbility(actor,cast.ability)||
                    SummonTargetMode(a)==OriginalAbilityTargetMode.Unit&&!SummonAbilityTarget(actor,world.UnitState(cast.target),cast.ability))
                {summonCasts.Remove(cast.actor);continue;}
                if(cast.due<=world.Clock+1e-9)CompleteSummonAbility(cast);
            }
            AdvanceSummonSourceAbilities();
        }
    }
}

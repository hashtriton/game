using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class WaveNativeCast { internal int actor; internal string ability; internal double due; }
        readonly Dictionary<int,WaveNativeCast> waveNativeCasts=new Dictionary<int,WaveNativeCast>();
        readonly Dictionary<string,double> waveSpellCooldowns=new Dictionary<string,double>();
        static bool IsScriptedWaveSpell(string id)=>id=="A15T"||id=="A0TR"||id=="A11L"||id=="A1DB"||id=="A1DD"||id=="A1DI";

        bool TryStartWaveSpell(int actorId,string ability)
        {
            var actor=world.UnitState(actorId);
            if(!IsScriptedWaveSpell(ability) || actor==null || actor.health<=.405 || actor.hidden || actor.paused ||
                ActorCastBlocked(actorId) || AbilityControlsActor(actorId) || !HasEffectiveUnitAbility(actor,ability))return false;
            var declaration=combatCatalog.Ability(ability);
            if(declaration.Text("code")!=(ability=="A1DB"?"Aroa":"Absk") || declaration.overrides.Length!=0 ||
                !declaration.TryNumber("Cost1",out double cost,out _) || !declaration.TryNumber("Cool1",out double cooldown,out _) ||
                cost<0 || cooldown<0)throw new InvalidOperationException("wave-native-cast-declaration-conflict:"+ability);
            if(actor.mana<cost || waveSpellCooldowns.TryGetValue(actorId+":"+ability,out double until)&&until>world.Clock+1e-9)return false;
            if(ability!="A1DB")
            {
                // Measured Absk lifecycle on A0AS/A10K transfers to these
                // authored Absk abilities: instant EFFECT and preserved order.
                // Their sparse native numeric buff modifiers are not invented.
                if(!world.TrySpendMana(actorId,cost))return false;
                waveSpellCooldowns[actorId+":"+ability]=world.Clock+cooldown;
                world.MarkCast(actorId);NotifyNativeSpellEffect(actorId,ability);BeginWaveSpellEffect(actorId,ability);return true;
            }
            double castPoint=combatCatalog.Unit(actor.rawcode).Number("castpt");
            if(castPoint<0)throw new InvalidOperationException("wave-native-negative-castpoint");
            world.Stop(actorId);OnAcceptedWorldOrder(actorId);world.MarkCast(actorId);
            if(weaponCycles.TryGetValue(actorId,out var cycle))cycle.winding=false;
            waveNativeCasts[actorId]=new WaveNativeCast{actor=actorId,ability=ability,due=world.Clock+castPoint};return true;
        }

        void AdvanceWaveNativeCasts()
        {
            foreach(var cast in new List<WaveNativeCast>(waveNativeCasts.Values))
            {
                var actor=world.UnitState(cast.actor);
                if(actor==null || actor.health<=.405 || actor.hidden || actor.paused || ActorCastBlocked(cast.actor) ||
                    !HasEffectiveUnitAbility(actor,cast.ability))
                {waveNativeCasts.Remove(cast.actor);continue;}
                if(cast.due>world.Clock+1e-9)continue;
                waveNativeCasts.Remove(cast.actor);var declaration=combatCatalog.Ability(cast.ability);
                if(!world.TrySpendMana(cast.actor,declaration.Number("Cost1")))continue;
                waveSpellCooldowns[cast.actor+":"+cast.ability]=cast.due+declaration.Number("Cool1");
                NotifyNativeSpellEffect(cast.actor,cast.ability);BeginWaveSpellEffect(cast.actor,cast.ability);
            }
        }

        void SelectWaveSpellOrders()
        {
            // Native creep spell selection is private engine AI. The host uses
            // its existing .2s acquisition scan, declared acquire radius and
            // source ability order. This is a bounded, deterministic AI policy.
            var units=world.Snapshot().units;
            foreach(var actor in units)
            {
                if(actor.kind!=OriginalWorldUnitKind.Enemy || actor.ownerSlot!=0 || actor.health<=.405 || actor.hidden || actor.paused ||
                    ActorCastBlocked(actor.entityId) || AbilityControlsActor(actor.entityId))continue;
                var definition=combatCatalog.Unit(actor.rawcode);
                if(!definition.TryNumber("acquire",out double range,out _) || range<=0)continue;
                bool nearby=false;
                foreach(var target in units)
                    if(target.health>.405 && CanSeeForCombat(actor.ownerSlot,target) && !target.invulnerable && AreEnemies(actor.ownerSlot,target.ownerSlot) &&
                        SquaredDistance(actor.position,target.position)<=range*range){nearby=true;break;}
                if(!nearby)continue;
                foreach(var ability in NativeUnitAbilities(actor))
                    if(IsScriptedWaveSpell(ability.id) && TryStartWaveSpell(actor.entityId,ability.id))break;
            }
        }

        void AppendWaveSpellVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var effect in waveSpellEffects)
            {
                var actor=world.UnitState(effect.actor);
                var view=new OriginalVisualEffectView{abilityId=effect.ability,sourceEntityId=effect.actor,position=effect.origin};
                switch(effect.ability)
                {
                    case "A15T":view.kind=OriginalVisualEffectKind.WarningCircle;view.radius=350;
                        view.progress=Math.Max(0,Math.Min(1,effect.stage==0?effect.height/1600:1-effect.height/1600));break;
                    case "A0TR":view.kind=OriginalVisualEffectKind.ActiveCircle;view.radius=1200;view.progress=effect.ticks/130d;break;
                    case "A11L":view.kind=OriginalVisualEffectKind.Beam;view.position=actor?.position??effect.origin;view.end=effect.goal;view.radius=20;break;
                    case "A1DB":view.kind=OriginalVisualEffectKind.Beam;view.position=actor?.position??effect.origin;
                        view.end=effect.goal;view.radius=200;view.variant=effect.stage;break;
                    default:var first=world.UnitState(effect.first);var second=world.UnitState(effect.second);
                        if(first==null)continue;view.kind=OriginalVisualEffectKind.Beam;view.position=first.position;
                        view.end=second?.position??effect.goal;view.radius=10;view.progress=1-effect.ticks/80d;break;
                }
                output.Add(view);
            }
        }
    }
}

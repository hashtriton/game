using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class WaveSpellEffect
        {
            internal string ability;
            internal int actor, owner, first, second, ticks, stage;
            internal double due, period, height, distance, traveled, bearing;
            internal OriginalPoint origin, goal;
            internal readonly HashSet<int> victims=new HashSet<int>();
        }
        sealed class WaveSpellStun { internal int target; internal double due, deathAt; internal bool applied; }
        readonly List<WaveSpellEffect> waveSpellEffects=new List<WaveSpellEffect>();
        readonly List<WaveSpellStun> waveSpellStuns=new List<WaveSpellStun>();
        uint waveSpellRandom;
        double NextWaveSpellRandom()
        {
            if(waveSpellRandom==0)waveSpellRandom=unchecked((uint)seed)^0x713AED29u;
            if(waveSpellRandom==0)waveSpellRandom=1;
            waveSpellRandom^=waveSpellRandom<<13;waveSpellRandom^=waveSpellRandom>>17;waveSpellRandom^=waveSpellRandom<<5;
            return waveSpellRandom/4294967296.0;
        }
        static OriginalPoint WaveStep(OriginalPoint p,double angle,double distance)=>
            new OriginalPoint(p.x+distance*Math.Cos(angle),p.y+distance*Math.Sin(angle));
        static double WaveBearing(OriginalPoint from,OriginalPoint to)=>Math.Atan2(to.y-from.y,to.x-from.x);

        // ISv/IUv/IYv/I1v/I5v/Anv, source3.9c36485..37100. This seam is
        // called only after SPELL_EFFECT. Timers retain their source identity
        // after death; no client command can publish an effect directly.
        bool BeginWaveSpellEffect(int actorId,string ability)
        {
            var actor=world.UnitState(actorId);if(actor==null)return false;
            var effect=new WaveSpellEffect{actor=actorId,owner=actor.ownerSlot,ability=ability,origin=actor.position};
            switch(ability)
            {
                case "A15T":effect.period=.02;world.SetPathingEnabled(actorId,false);break;
                case "A0TR":effect.period=.03;break;
                case "A11L":
                    var centers=new[]{new OriginalPoint(-1024,0),new OriginalPoint(768,1792),new OriginalPoint(-640,1568),
                        new OriginalPoint(384,256),new OriginalPoint(1728,1216),new OriginalPoint(-192,-800),
                        new OriginalPoint(64,2624),new OriginalPoint(-1984,576)};
                    // IYv uses the first cD Emv regions. Host RNG replaces the
                    // private engine stream; the authored rectangle centers remain.
                    effect.goal=centers[(int)(NextWaveSpellRandom()*Math.Max(1,Math.Min(8,match.Participants)))];
                    effect.distance=Math.Sqrt(SquaredDistance(actor.position,effect.goal));
                    effect.bearing=WaveBearing(actor.position,effect.goal);effect.period=.05;
                    world.SetPathingEnabled(actorId,false);break;
                case "A1DB":
                    OriginalWorldUnitView chosen=null;
                    foreach(var target in world.Snapshot().units)
                    {
                        if(target.health<=.405 || !AreEnemies(actor.ownerSlot,target.ownerSlot) || !IsNativeHeroPredicate(target) ||
                            CasterHasAbility(target,"A0K4") || SquaredDistance(actor.position,target.position)>2200*2200)continue;
                        bool priority=SquaredDistance(target.position,new OriginalPoint(-1150,1160))<160000 ||
                            SquaredDistance(target.position,new OriginalPoint(1150,400))<160000;
                        if(chosen==null || priority)chosen=target;
                    }
                    if(chosen==null)return false;
                    effect.goal=chosen.position;effect.period=.2;effect.ticks=8;
                    world.SetUnitState(actorId,paused:true);world.SetPathingEnabled(actorId,false);break;
                case "A1DD":case "A1DI":
                    var party=new List<OriginalWorldUnitView>();
                    foreach(var player in players)
                    {
                        var hero=world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                        if(hero!=null)party.Add(hero); // Retained HK/ar includes disconnected players.
                    }
                    if(party.Count==0)return false;
                    var first=party[(int)(NextWaveSpellRandom()*party.Count)];
                    if(first.health<=.405)first=party.Find(h=>h.health>.405);
                    if(first==null)return false; // Source bounded dead-party fallback.
                    OriginalWorldUnitView second=null;double nearest=double.MaxValue;
                    foreach(var hero in party)
                    {
                        if(hero.entityId==first.entityId || hero.health<=.405)continue;
                        double d=SquaredDistance(first.position,hero.position);
                        if(d<nearest){nearest=d;second=hero;}
                    }
                    effect.first=first.entityId;effect.second=second?.entityId??(ability=="A1DI"?actorId:0);
                    effect.goal=first.position;effect.period=.1;effect.ticks=80;break;
                default:return false;
            }
            effect.due=world.Clock+effect.period;waveSpellEffects.Add(effect);return true;
        }

        void QueueWaveSpellStun(int target)
        {
            var definition=combatCatalog.Ability("A0O1");
            if(definition.Text("code")!="AHtb" || definition.Text("BuffID1")!="BPSE" ||
                definition.Number("Dur1")!=3 || definition.Number("HeroDur1")!=3)
                throw new InvalidOperationException("wave-tether-stun-declaration-conflict");
            foreach(var field in definition.fields)
                if(field.key=="DataA1" && (!field.isNumber || field.conflict || field.number!=0))
                    throw new InvalidOperationException("wave-tether-bolt-native-damage-conflict");
            foreach(var field in definition.overrides)
                if(field.field=="Htb1" && field.level==1 && (!field.isNumber || field.number!=0))
                    throw new InvalidOperationException("wave-tether-bolt-native-damage-conflict");
            // Exact LEAPBOLT1/A0O1 capture2c7cec42: same-position helper
            // impact+.00494, two native0 events around BPSE, targetHP631
            // unchanged. BPSE3s outlives source BTLF1s. Host .005 delivery
            // approximates the measured clock; distant targets are not inferred.
            NotifyNativeSpellEffect(0,"A0O1");
            waveSpellStuns.Add(new WaveSpellStun{target=target,due=world.Clock+.005,deathAt=world.Clock+1});
        }

        void AdvanceWaveSpells()
        {
            AdvanceWaveNativeCasts();
            AdvanceNativeInvisibility();
            AdvanceOrdinaryNativeSpells();
            foreach(var effect in waveSpellEffects.ToArray())
                while(waveSpellEffects.Contains(effect) && effect.due<=world.Clock+1e-9)
                {
                    effect.due+=effect.period;
                    if(!TickWaveSpell(effect))waveSpellEffects.Remove(effect);
                }
            foreach(var stun in waveSpellStuns.ToArray())
            {
                if(!stun.applied && stun.due<=world.Clock+1e-9)
                {
                    stun.applied=true;var target=world.UnitState(stun.target);
                    if(target!=null && target.health>.405 && !target.hidden && !target.invulnerable && !CasterMagicImmune(target))
                    {
                        ApplyNativeTriggeredHit(0,0,target,0,OriginalTriggeredDamageMode.SpellMagic);
                        target=world.UnitState(stun.target);
                        if(target!=null && target.health>.405 && !target.hidden && !target.invulnerable && !CasterMagicImmune(target))
                        {
                            AddTimedNativeStun(target.entityId,"BPSE",0,3);
                            ApplyNativeTriggeredHit(0,0,target,0,OriginalTriggeredDamageMode.SpellMagic);
                        }
                    }
                }
                if(stun.deathAt>world.Clock+1e-9)continue;
                waveSpellStuns.Remove(stun);ObserveScriptedHelperDeath();
            }
        }

        bool TickWaveSpell(WaveSpellEffect effect)
        {
            var actor=world.UnitState(effect.actor);
            if(effect.ability=="A0TR")
            {
                if(++effect.ticks>130)return false;
                foreach(var target in world.Snapshot().units)
                    if(target.health>.405 && AreEnemies(effect.owner,target.ownerSlot) && !CasterHasType(target,"structure") &&
                        SquaredDistance(effect.origin,target.position)<=1200*1200 && SquaredDistance(effect.origin,target.position)>=20*20)
                        world.ForcePosition(target.entityId,WaveStep(target.position,WaveBearing(target.position,effect.origin),8));
                return true;
            }
            if(effect.ability=="A1DD" || effect.ability=="A1DI")return TickWaveBinding(effect);
            if(actor==null)
            {
                if(effect.ability=="A1DB" && effect.stage==0)ObserveScriptedHelperDeath();
                return false;
            }
            if(effect.ability=="A15T")
            {
                world.ForcePosition(effect.actor,effect.origin);
                if(effect.stage==0)
                {if(effect.height<800)effect.height+=12.5;else effect.stage=1;return true;}
                if(effect.height>0){effect.height-=20;return true;}
                world.SetPathingEnabled(effect.actor,true);waveSpellEffects.Remove(effect);
                // Ba is Qc==3 only. Nightmare sets Gn, not Ba.
                double damage=options.difficulty==OriginalDifficulty.Extreme?1500:750;
                foreach(var target in world.Snapshot().units)
                    if(target.health>.405 && AreEnemies(effect.owner,target.ownerSlot) && !CasterMagicImmune(target) &&
                        !CasterHasType(target,"structure") && SquaredDistance(effect.origin,target.position)<=350*350)
                        ApplyTriggeredHit(effect.actor,effect.owner,target,damage,OriginalTriggeredDamageMode.SpellMagic);
                return false; // IQv removes h02R without KillUnit.
            }
            if(effect.ability=="A11L")
            {
                world.ForcePosition(effect.actor,WaveStep(actor.position,effect.bearing,20));effect.traveled+=20;
                if(effect.traveled<effect.distance)return true;
                world.SetPathingEnabled(effect.actor,true);return false;
            }
            if(effect.stage==0)
            {
                if(actor.health<.405){ObserveScriptedHelperDeath();return false;}
                if(effect.ticks-->0)
                {world.SetFacing(effect.actor,WaveBearing(actor.position,effect.goal)*180/Math.PI);return true;}
                ObserveScriptedHelperDeath();effect.stage=1;effect.period=.03;
                effect.due=effect.due-.2+.03;return true;
            }
            effect.traveled+=25.5;
            if(effect.traveled>=800 || actor.health<.405)
            {
                foreach(var target in world.Snapshot().units)
                    if(AreEnemies(effect.owner,target.ownerSlot) && IsNativeHeroPredicate(target) &&
                        SquaredDistance(actor.position,target.position)<=350*350)
                        world.ForcePosition(target.entityId,WaveStep(target.position,WaveBearing(actor.position,target.position),400));
                world.SetPathingEnabled(effect.actor,true);world.SetUnitState(effect.actor,paused:false);return false;
            }
            var point=WaveStep(actor.position,actor.facingDegrees*Math.PI/180,25.5);
            if(OriginalShieldBashRules.AllowsForcedPoint(point))world.ForcePosition(effect.actor,point);
            foreach(var target in world.Snapshot().units)
                if(target.health>.405 && AreEnemies(effect.owner,target.ownerSlot) && SquaredDistance(point,target.position)<=200*200 &&
                    effect.victims.Add(target.entityId))
                    ApplyTriggeredHit(effect.actor,effect.owner,target,400,OriginalTriggeredDamageMode.SpellMagic);
            return true;
        }

        bool TickWaveBinding(WaveSpellEffect effect)
        {
            var first=world.UnitState(effect.first);var second=effect.second==0?null:world.UnitState(effect.second);
            if(effect.ticks<=0 || first==null || first.health<.405 || effect.second!=0&&(second==null || second.health<.405))return false;
            var other=second?.position??effect.goal;
            double distance=Math.Sqrt(SquaredDistance(first.position,other));
            bool trigger=effect.ability=="A1DD"?distance>900:distance<=150;
            if(trigger)
            {
                waveSpellEffects.Remove(effect);double damage=effect.ability=="A1DD"?700:900;
                ApplyTriggeredHit(effect.actor,effect.owner,first,damage,OriginalTriggeredDamageMode.ChaosUniversal);
                if(effect.ability=="A1DD")QueueWaveSpellStun(first.entityId);
                if(second!=null)ApplyTriggeredHit(effect.actor,effect.owner,second,damage,OriginalTriggeredDamageMode.ChaosUniversal);
                if(effect.ability=="A1DI")QueueWaveSpellStun(first.entityId);
                if(second!=null)QueueWaveSpellStun(second.entityId);return false;
            }
            if(effect.ability=="A1DI")
            {
                double angle=WaveBearing(first.position,other);
                // Aav uses SetUnitPosition, unlike IUv's SetUnitX/Y. The host
                // uses its collision-aware relocation and rejects occupied
                // points; Warcraft's private nearest-placement search is not replayed.
                world.Relocate(first.entityId,WaveStep(first.position,angle,8));
                world.Relocate(second.entityId,WaveStep(other,angle,-8));
            }
            effect.ticks--;return true;
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionControlIntegrationTests
    {
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, Private).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args=new object[]{null,false};
            var session=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(session); return session;
        }
        static void Block(OriginalSession s, OriginalActorControlMask mask, double duration=2) => Call(s,"SetActorControl",1,"test-native-control",mask,duration,true,true);
        static OriginalSessionReplyCode Cast(OriginalSession s,long sequence) => s.Apply(0,new OriginalSessionCommand{sequence=sequence,kind=OriginalSessionCommandKind.CastSkill,skillId="A05N"});
        static void Step(OriginalSession s,double duration)
        { while(duration>1e-8){double step=Math.Min(.05,duration);s.Advance(step);s.DrainEvents();duration-=step;} Assert.That(s.HaltReason,Is.Null); }
        static void Clock(OriginalWorld w,double duration)
        { while(duration>1e-8){double step=Math.Min(.05,duration);w.Advance(step);duration-=step;} }

        [Test] public void SilenceCancelsPreEffectCastWithoutManaOrCooldownAndRejectsNewCast()
        {
            var s=Create(out var w); Assert.That(Cast(s,5),Is.EqualTo(OriginalSessionReplyCode.Accepted)); Step(s,.1);
            Block(s,OriginalActorControlMask.Cast); Step(s,.8);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(145)); Assert.That(w.Snapshot().units.Length,Is.EqualTo(1));
            Assert.That(s.Snapshot().players[0].abilities.Single(x=>x.id=="A05N").code,Is.EqualTo(OriginalAbilityUseCode.Busy));
            Assert.That(Cast(s,6),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Call(s,"ClearActorControl",1,"test-native-control"); Assert.That(Cast(s,7),Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }
        [Test] public void SilenceAfterMirrorEffectPreservesCommittedSummonAndReleasedHelper()
        {
            var s=Create(out var w); Assert.That(Cast(s,5),Is.EqualTo(OriginalSessionReplyCode.Accepted)); Step(s,.3);
            Block(s,OriginalActorControlMask.Cast); Step(s,.5);
            Assert.That(w.UnitState(1).hidden,Is.False); Assert.That(w.Snapshot().units.Count(x=>x.kind==OriginalWorldUnitKind.Illusion),Is.EqualTo(1));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(85));
        }
        [Test] public void WeaponBlockCancelsWindupPreservesTargetAndResumesAfterExpiry()
        {
            var s=Create(out var w); w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(100,-1400));
            Assert.That(w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001),Is.True); Call(s,"AdvanceWeapons");
            Assert.That(w.UnitState(1).attackSequence,Is.EqualTo(1)); Block(s,OriginalActorControlMask.Weapon);
            Clock(w,.5); Call(s,"AdvanceWeapons"); Assert.That(w.UnitState(1001).health,Is.EqualTo(1000));
            Assert.That(w.UnitState(1).targetId,Is.EqualTo(1001)); Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Call(s,"ClearActorControl",1,"test-native-control"); Clock(w,2);Call(s,"AdvanceWeapons");Clock(w,.5);Call(s,"AdvanceWeapons");
            Assert.That(w.UnitState(1001).health,Is.LessThan(1000));
        }
        [Test] public void CastOnlyControlDoesNotBlockAnExistingWeaponOrder()
        {
            var s=Create(out var w); w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(100,-1400));
            w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001);Block(s,OriginalActorControlMask.Cast);
            Call(s,"AdvanceWeapons");Clock(w,.5);Call(s,"AdvanceWeapons");Assert.That(w.UnitState(1001).health,Is.LessThan(1000));
        }
        [Test] public void MagicImmunitySuppressesDamageEventRatherThanSendingZeroToWatcher()
        {
            var s=Create(out var w); w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(100,-1400));
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"ACmi"},Array.Empty<string>());
            // Prime a real native-damage observer at zero charges. Any emitted
            // event consumes this active watch even when its numeric damage=0.
            var state=Call(s,"Archer",1);state.GetType().GetField("active",Private).SetValue(state,OriginalBowElement.Venom);
            var type=typeof(OriginalSession).GetNestedType("ArcherWatch",BindingFlags.NonPublic);var watch=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("actor",1),("owner",1),("target",1001),("expires",10.0)})type.GetField(pair.Item1,Private).SetValue(watch,pair.Item2);
            var watches=(IList)typeof(OriginalSession).GetField("archerWatches",Private).GetValue(s);watches.Add(watch);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000));Assert.That(watches.Count,Is.EqualTo(1));
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1001).health,Is.LessThan(1000));Assert.That(watches.Count,Is.Zero);
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"ACmi"});double before=w.UnitState(1001).health;
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(before-40).Within(1e-8));
        }
    }
}

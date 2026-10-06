using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemFortitudeTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string method,params object[] args)=>typeof(OriginalSession).GetMethod(method,Private).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create(string item,out OriginalWorld w)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            var candidate=Inventory(s).Copy();candidate.TryPickup(candidate.CreateInstance(item));Publish(s,w,candidate);return s;
        }
        static void Publish(OriginalSession s,OriginalWorld w,OriginalInventory inventory)
        {Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),inventory,w.UnitState(1)),Is.True);Player(s).GetType().GetField("inventory").SetValue(Player(s),inventory);}
        static void Tick(OriginalSession s,OriginalWorld w,double time)
        {while(time>1e-9){double step=Math.Min(.05,time);w.Advance(step);Call(s,"AdvanceItemFortitude");time-=step;}}
        static OriginalSessionReplyCode Use(OriginalSession s)
        {var item=Inventory(s).HeroSlots[0];return s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,
            sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemInstanceId=item.instanceId,itemSlot=0});}
        static void Add(OriginalWorld w,int id,double x)=>w.AddUnit(id,0,"hfoo",new OriginalWorldUnitProfile{
            maxHealth=2000,maxMana=0,collisionRadius=8},new OriginalPoint(x,1000));
        static bool Immune(OriginalSession s,OriginalWorld w)=>(bool)Call(s,"CasterMagicImmune",w.UnitState(1));

        [Test] public void FortitudePublicUseCleansesOnceKeepsTheItemAndUsesSourceCounterBeforeDecrement()
        {
            var s=Create("I072",out var w);long item=Inventory(s).HeroSlots[0].instanceId;double mana=w.UnitState(1).mana;
            Call(s,"AddNativeSilence",1,9001,7d);Assert.That(Call(s,"ActorCastBlocked",1),Is.True);
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
            Assert.That(Immune(s,w),Is.True);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(22));
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Tick(s,w,6);Assert.That(Immune(s,w),Is.True);Tick(s,w,.99);Assert.That(Immune(s,w),Is.True);Tick(s,w,.01);Assert.That(Immune(s,w),Is.False);
            var codec=new OriginalUnitySessionCodec();var snapshot=s.Snapshot();Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
                kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _),Is.True);
        }
        [Test] public void FortitudeRejectsMagicWithoutAZeroEventAndDoesNotBlockUniversalDamage()
        {
            var s=Create("I072",out var w);Use(s);double hp=w.UnitState(1).health;
            Call(s,"ApplyNativeTriggeredHit",0,0,w.UnitState(1),40d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Call(s,"ApplyNativeTriggeredHit",0,0,w.UnitState(1),40d,OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-40));
            w.SetUnitState(1,paused:true);Tick(s,w,7);Assert.That(Immune(s,w),Is.False,"nq is a script timer, independent of PauseUnit");
        }
        [Test] public void ReapplicationSetsExplicitDurationAndPassivePulseAddsOneWithoutRepeatingCleanse()
        {
            var s=Create("I072",out var w);Call(s,"BeginItemFortitude",1,6d);Tick(s,w,3);
            Call(s,"SetActorControl",1,"fixture-negative",OriginalActorControlMask.Move,0d,true,true);
            Call(s,"BeginItemFortitude",1,8d);Assert.That(Call(s,"ActorMoveBlocked",1),Is.True,"Vq refresh skips UnitRemoveBuffs");
            Tick(s,w,8);Assert.That(Immune(s,w),Is.True);Call(s,"BeginItemFortitude",1,1d);
            Tick(s,w,1);Assert.That(Immune(s,w),Is.True);Tick(s,w,1);Assert.That(Immune(s,w),Is.False);
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.True,"Removing fortitude must not clear another control");
        }
        [Test] public void GuardianHitsLastEligibleTargetEverySixGlobalTicksAndDropPreservesPartialCounter()
        {
            var s=Create("I05Q",out var w);Add(w,9001,400);Add(w,9002,450);Add(w,9003,1000);
            Tick(s,w,5.99);Assert.That(w.UnitState(9002).health,Is.EqualTo(2000));Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(2000));Assert.That(w.UnitState(9002).health,Is.EqualTo(1700));
            Assert.That(w.UnitState(9003).health,Is.EqualTo(2000));Assert.That(Immune(s,w),Is.True);
            Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="A11Z"&&e.kind==OriginalVisualEffectKind.Beam),Is.True);
            Tick(s,w,2);var candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Hero,0);Publish(s,w,candidate);
            Tick(s,w,10);Assert.That(w.UnitState(9002).health,Is.EqualTo(1700));Assert.That(Immune(s,w),Is.False);
            candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Servant,0);Publish(s,w,candidate);
            Tick(s,w,3.99);Assert.That(w.UnitState(9002).health,Is.EqualTo(1700));Tick(s,w,.01);Assert.That(w.UnitState(9002).health,Is.EqualTo(1400));
        }
        [Test] public void GuardianStillGrantsFortitudeWithoutATargetAndRefreshDoesNotResetItsSixTickPhase()
        {
            var s=Create("I05Q",out var w);Tick(s,w,5.5);Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s,w,.5);Assert.That(Immune(s,w),Is.True);
            var states=(IDictionary)typeof(OriginalSession).GetField("itemFortitudes",Private).GetValue(s);var state=states[1];
            Assert.That(state.GetType().GetField("remaining",Private).GetValue(state),Is.EqualTo(9d));
            w.ForceUnitDeath(1);Tick(s,w,.5);Assert.That(states.Contains(1),Is.False);
            Assert.That(s.HaltReason,Is.Null);
        }
    }
}

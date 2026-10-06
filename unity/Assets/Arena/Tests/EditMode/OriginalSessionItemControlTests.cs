using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemControlTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s, string method, params object[] args) => typeof(OriginalSession).GetMethod(method, Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld w, out OriginalInventory inventory, out OriginalItemInstance potion)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            inventory=(OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
            potion=inventory.CreateInstance("I03L");potion.chargesKnown=true;potion.charges=1;
            Assert.That(inventory.TryPickup(potion,OriginalInventoryBag.Hero).Applied,Is.True);
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);w.SetUnitState(1,paused:false);
            return s;
        }
        static OriginalSessionCommand Use(OriginalSession s, OriginalItemInstance item) => new OriginalSessionCommand {
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemInstanceId=item.instanceId,itemSlot=0 };
        [Test] public void StunQueuesPotionAndConsumesExactlyOnceAfterExpiry()
        {
            var s=Create(out var w,out _,out var item);Call(s,"AddTimedNativeStun",1,"BPSE",0,1d);
            Assert.That(s.Apply(0,Use(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            Call(s,"AdvanceActorControls",.99);Call(s,"AdvanceQueuedItems");Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            Call(s,"AdvanceActorControls",.01);Call(s,"AdvanceQueuedItems");Assert.That(w.UnitState(1).health,Is.EqualTo(400));
            Call(s,"AdvanceQueuedItems");Assert.That(w.UnitState(1).health,Is.EqualTo(400));
            Assert.That(s.Snapshot().players[0].inventory.heroSlots[0],Is.Null);
        }
        [Test] public void SleepDefersItemButSilenceAndRootDoNot()
        {
            foreach(string status in new[]{"sleep","silence","root"})
            {
                var s=Create(out var w,out _,out var item);
                if(status=="sleep")Call(s,"AddNativeSleep",1,0);
                else if(status=="silence")Call(s,"AddNativeSilence",1,0,7d);
                else Call(s,"AddTimedNativeRoot",1,0,7d);
                Assert.That(s.Apply(0,Use(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(w.UnitState(1).health,Is.EqualTo(status=="sleep"?100:400),status);
                if(status=="sleep") { Call(s,"ClearNegativeActorControls",1);Call(s,"AdvanceQueuedItems");Assert.That(w.UnitState(1).health,Is.EqualTo(400)); }
            }
        }
        [Test] public void DoomRejectsPotionAndSuppressesOnlyObservedStrengthHealthRate()
        {
            var s=Create(out var w,out _,out var item);Call(s,"QueueBossDoom",0,1);
            Assert.That(s.Apply(0,Use(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Call(s,"AdvanceRegeneration",1d);Assert.That(w.UnitState(1).health,Is.EqualTo(101.3).Within(1e-6));
            Assert.That(s.Snapshot().players[0].inventory.heroSlots[0].instanceId,Is.EqualTo(item.instanceId));
        }
        [Test] public void ReplacementOrderCancelsQueuedPotionWithoutConsumption()
        {
            var s=Create(out var w,out _,out var item);Call(s,"AddTimedNativeStun",1,"BPSE",0,1d);
            Assert.That(s.Apply(0,Use(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Stop,sequence=s.Snapshot().players[0].acknowledgedSequence+1}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(s,"AdvanceActorControls",1d);Call(s,"AdvanceQueuedItems");
            Assert.That(w.UnitState(1).health,Is.EqualTo(100));Assert.That(s.Snapshot().players[0].inventory.heroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void DeathCleanupThenImmediateReviveDoesNotExecuteAnOldItemOrder()
        {
            var s=Create(out var w,out _,out var item);Call(s,"AddTimedNativeStun",1,"BPSE",0,1d);
            Assert.That(s.Apply(0,Use(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var actor=w.UnitState(1);w.ForceUnitDeath(1);Call(s,"ForgetActorControls",1);
            Assert.That(w.RestoreUnit(1,actor.position),Is.True);w.UpdateProfile(1,actor.profile,100,actor.mana);
            Call(s,"AdvanceQueuedItems");Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            Assert.That(s.Snapshot().players[0].inventory.heroSlots[0].charges,Is.EqualTo(1));
        }
    }
}


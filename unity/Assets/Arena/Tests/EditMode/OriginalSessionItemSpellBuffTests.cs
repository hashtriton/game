using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemSpellBuffTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
        static OriginalInventory Bag(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create(out OriginalWorld w)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,maxMana=100,collisionRadius=8},new OriginalPoint(300,1000));
            w.SetUnitState(1,paused:false);return s;
        }
        static OriginalItemInstance Equip(OriginalSession s,OriginalWorld w,string id)
        {
            var bag=Bag(s).Copy();var item=bag.CreateInstance(id);
            Assert.That(bag.TryPickup(item).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),bag,w.UnitState(1)),Is.True);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),bag);Call(s,"SyncItemScriptInventory",Player(s));return item;
        }
        static OriginalSessionReplyCode Use(OriginalSession s,OriginalItemInstance item,int target=0)=>s.Apply(0,new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemInstanceId=item.instanceId,itemSlot=Array.FindIndex(Bag(s).HeroSlots,i=>i!=null&&i.instanceId==item.instanceId),
            targetKind=target==0?OriginalWorldTargetKind.None:OriginalWorldTargetKind.Unit,targetId=target});
        static void State(OriginalWorld w,double health=100,double mana=1000)
        {var u=w.UnitState(1);var p=u.profile;w.UpdateProfile(1,p,health,Math.Min(p.maxMana,mana));}
        static void Auxiliary(OriginalSession s,string id,int rank=1)=>((IDictionary)Player(s).GetType().GetField("auxiliaryAbilities").GetValue(Player(s)))[id]=rank;
        static double Prepared(OriginalSession s,int actor=1)
        {var effect=Call(s,"PrepareItemSpellEffect",actor,100d);return (double)effect.GetType().GetField("damage",Hidden).GetValue(effect);}
        static void Hit(OriginalSession s,OriginalWorld w)=>Call(s,"ApplyTriggeredHit",1,1,w.UnitState(9001),100d,OriginalTriggeredDamageMode.ChaosUniversal);
        static bool Carrier(OriginalSession s,OriginalWorld w,string id)=>(bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),id);
        static int Regen(OriginalSession s)=>((IDictionary)typeof(OriginalSession).GetField("itemRegeneration",Hidden).GetValue(s)).Count;
        static void MoonTick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double step=Math.Min(.05,seconds);w.Advance(step);Call(s,"AdvanceItemScriptActs");Call(s,"AdvanceItemLegacyActives");seconds-=step;}}

        [Test] public void AcceptedElixirUsesActualBuffAndAddsPowerToExistingItemsAndAuxiliaryRank()
        {
            var s=Create(out var w);Equip(s,w,"I07R");var potion=Equip(s,w,"I0AJ");Auxiliary(s,"A19C",2);State(w,mana:0);
            Assert.That(Prepared(s),Is.EqualTo(115).Within(1e-9));
            Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.Accepted));Auxiliary(s,"A19C",2);Assert.That(Regen(s),Is.EqualTo(1));
            Assert.That(Prepared(s),Is.EqualTo(135).Within(1e-9));
            Hit(s,w);Assert.That(w.UnitState(9001).health,Is.EqualTo(9865).Within(1e-8));
        }
        [Test] public void ElixirAndDemonicAuxiliaryShareOneTwentyPercentSourceBranch()
        {
            var s=Create(out var w);var potion=Equip(s,w,"I0AJ");Auxiliary(s,"A19O");State(w,mana:0);
            Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.Accepted));Auxiliary(s,"A19O");Assert.That(Prepared(s),Is.EqualTo(120).Within(1e-9));
            Hit(s,w);Assert.That(w.UnitState(9001).health,Is.EqualTo(9880).Within(1e-8));
            Assert.That(w.UnitState(1).health,Is.EqualTo(88).Within(1e-8));Assert.That(Regen(s),Is.Zero,"The self hit interrupts the actual regeneration buff.");
        }
        [Test] public void FullManaElixirRefusalPreservesChargeAndDoesNotCreatePowerBuff()
        {
            var s=Create(out var w);var potion=Equip(s,w,"I0AJ");State(w);
            Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.NotReady));Assert.That(Regen(s),Is.Zero);
            Assert.That(Bag(s).HeroSlots.Single(i=>i!=null).instanceId,Is.EqualTo(potion.instanceId));Assert.That(Prepared(s),Is.EqualTo(100));
        }
        [Test] public void ElixirPowerEndsAtExistingTenPulseExpiry()
        {
            var s=Create(out var w);var potion=Equip(s,w,"I0AJ");State(w,mana:0);Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(s,"AdvanceItemRegeneration",9.99d);Assert.That(Prepared(s),Is.EqualTo(120).Within(1e-9));
            Call(s,"AdvanceItemRegeneration",.01d);Assert.That(Regen(s),Is.Zero);Assert.That(Prepared(s),Is.EqualTo(100));
        }
        [Test] public void ElixirPowerFollowsPositiveDamageInterruptionButNotZeroDamage()
        {
            var s=Create(out var w);var potion=Equip(s,w,"I0AJ");State(w,mana:0);Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(s,"ApplyNativeTriggeredHit",9001,0,w.UnitState(1),0d,OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(Prepared(s),Is.EqualTo(120).Within(1e-9));
            Call(s,"ApplyNativeTriggeredHit",9001,0,w.UnitState(1),1d,OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(Regen(s),Is.Zero);Assert.That(Prepared(s),Is.EqualTo(100));
        }
        [Test] public void StunnedElixirOrderHasNoBuffOrDebitBeforeActualExecutionAndStopCancels()
        {
            foreach(bool stop in new[]{false,true})
            {
                var s=Create(out var w);var potion=Equip(s,w,"I0AJ");State(w,mana:0);Call(s,"AddTimedNativeStun",1,"BPSE",0,1d);
                Assert.That(Use(s,potion),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Regen(s),Is.Zero);Assert.That(Prepared(s),Is.EqualTo(100));
                Assert.That(Bag(s).HeroSlots.Single(i=>i!=null).instanceId,Is.EqualTo(potion.instanceId));
                if(stop)Assert.That(s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Stop,sequence=s.Snapshot().players[0].acknowledgedSequence+1}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Call(s,"AdvanceActorControls",1d);Call(s,"AdvanceQueuedItems");
                Assert.That(Regen(s),Is.EqualTo(stop?0:1));Assert.That(Prepared(s),Is.EqualTo(stop?100:120).Within(1e-9));
            }
        }
        [Test] public void UmbraActiveCarrierDoublesActualItemVampAndExpiresAfterSixSeconds()
        {
            var s=Create(out var w);var moon=Equip(s,w,"I08D");State(w);Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(115).Within(1e-8));
            Assert.That(Use(s,moon,1),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Carrier(s,w,"A17O"),Is.True);
            State(w);Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(130).Within(1e-8));
            MoonTick(s,w,5.99);Assert.That(Carrier(s,w,"A17O"),Is.True);MoonTick(s,w,.01);Assert.That(Carrier(s,w,"A17O"),Is.False);
            State(w);Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(115).Within(1e-8));
        }
        [Test] public void UmbraRejectedEnemyAndMissingTargetOrdersDoNotGrantCarrierOrSpendMana()
        {
            var s=Create(out var w);var moon=Equip(s,w,"I08D");State(w);
            Assert.That(Use(s,moon,9001),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Use(s,moon),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(w.UnitState(1).profile.maxMana));Assert.That(Carrier(s,w,"A17O"),Is.False);
            Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(115).Within(1e-8));
        }
        [Test] public void LesserMoonDoesNotCreateUmbraAmplifierAndStunnedUmbraWaitsForControlExpiry()
        {
            var s=Create(out var w);var lesser=Equip(s,w,"I07P");Equip(s,w,"I05K");State(w);
            Assert.That(Use(s,lesser,1),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Carrier(s,w,"A17O"),Is.False);
            State(w);Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(110).Within(1e-8));
            s=Create(out w);var moon=Equip(s,w,"I08D");State(w);double beforeMana=w.UnitState(1).mana;Call(s,"AddTimedNativeStun",1,"BPSE",0,1d);
            Assert.That(Use(s,moon,1),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Carrier(s,w,"A17O"),Is.False);Assert.That(w.UnitState(1).mana,Is.EqualTo(beforeMana));
            Call(s,"AdvanceActorControls",1d);Call(s,"AdvanceQueuedItems");Assert.That(Carrier(s,w,"A17O"),Is.True);var direct=Create(out var directWorld);var directMoon=Equip(direct,directWorld,"I08D");State(directWorld);
            Assert.That(Use(direct,directMoon,1),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(directWorld.UnitState(1).mana));
            State(w);Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(130).Within(1e-8));
        }
        [Test] public void UmbraComposesWithDemonicHalvingWeaknessAndImprovedHealingInSourceOrder()
        {
            var s=Create(out var w);var moon=Equip(s,w,"I08D");Equip(s,w,"I05K");Equip(s,w,"I05Y");State(w);
            Assert.That(Use(s,moon,1),Is.EqualTo(OriginalSessionReplyCode.Accepted));Auxiliary(s,"A19O");Auxiliary(s,"A19M");State(w);
            Hit(s,w);Assert.That(w.UnitState(1).health,Is.EqualTo(113.2).Within(1e-8),"100-12+(120*.25*.5*2*.7*1.2)");
        }
    }
}

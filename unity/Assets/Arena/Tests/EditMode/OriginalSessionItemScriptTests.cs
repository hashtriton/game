using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemScriptTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string method,params object[] args)=>typeof(OriginalSession).GetMethod(method,Private).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalWorld World(OriginalSession s)=>(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
        static OriginalSession Create()=>(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        static OriginalItemInstance Equip(OriginalSession s,string id)
        {
            var candidate=Inventory(s).Copy();var item=candidate.CreateInstance(id);
            Assert.That(candidate.TryPickup(item).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,World(s).UnitState(1)),Is.True,id);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
            Call(s,"SyncItemScriptInventory",Player(s));return item;
        }
        static OriginalSessionReplyCode Use(OriginalSession s,OriginalItemInstance item,long target=0)=>s.Apply(0,new OriginalSessionCommand {
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemInstanceId=item.instanceId,
            itemSlot=Array.FindIndex(Inventory(s).HeroSlots,x=>x!=null&&x.instanceId==item.instanceId),targetItemInstanceId=target });
        static void Mana(OriginalSession s,double value)
        { var w=World(s);var u=w.UnitState(1);var p=u.profile;p.maxMana=Math.Max(p.maxMana,value);w.UpdateProfile(1,p,u.health,value); }
        static IDictionary Cooldowns(OriginalSession s)=>(IDictionary)typeof(OriginalSession).GetField("itemCooldowns",Private).GetValue(s);
        static void Tick(OriginalSession s,int seconds)
        { var w=World(s);for(int i=0;i<seconds*20;i++)w.Advance(.05);Call(s,"AdvanceItemScripts"); }

        [Test] public void HammerReturnsFirstComponentAndDropsSecondWithoutDuplicatingConsumedItems()
        {
            var s=Create();var boots=Equip(s,"I08U");var hammer=Equip(s,"I0B7");
            Assert.That(Use(s,hammer),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var slots=Inventory(s).HeroSlots.Where(x=>x!=null).ToArray();
            Assert.That(slots.Select(x=>x.itemId),Is.EqualTo(new[]{"I06J"}));
            Assert.That(slots.All(x=>x.instanceId!=boots.instanceId&&x.instanceId!=hammer.instanceId),Is.True);
            var ground=s.Snapshot().groundItems.Single();Assert.That(ground.item.itemId,Is.EqualTo("I071"));
            Assert.That(ground.item.ownerId,Is.EqualTo(1));Assert.That(ground.position.x,Is.EqualTo(World(s).UnitState(1).position.x));
        }
        [Test] public void HammerInvalidRecipeRefundsChargeAndStaleTargetDoesNotConsume()
        {
            var s=Create();var claw=Equip(s,"I000");var hammer=Equip(s,"I0B7");
            Assert.That(Use(s,hammer,long.MaxValue),Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(Inventory(s).HeroSlots.Count(x=>x!=null&&x.itemId=="I0B7"),Is.EqualTo(1));
            Assert.That(Use(s,hammer,claw.instanceId),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots.Where(x=>x!=null).Select(x=>x.itemId),Is.EquivalentTo(new[]{"I000","I0B7"}));
            Assert.That(s.Snapshot().groundItems,Is.Empty);
        }
        [Test] public void NemesisSpendsNativeManaAndResetPoolKeepsSourcePublishBeforeDecrement()
        {
            var s=Create();var amulet=Equip(s,"I0AE");Mana(s,1000);
            Cooldowns(s)["1:other"]=100d;
            Assert.That(Use(s,amulet),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(500));Assert.That(Cooldowns(s).Count,Is.EqualTo(0));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(Call(s,"NemesisItemSpellPower",1),Is.EqualTo(.03));
            Assert.That(Use(s,amulet),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(0));
            Assert.That(Cooldowns(s)["1:A0WW"],Is.EqualTo(120d));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void NemesisTimerGrantsOnCallback61AndPausesWhenNobodyHoldsIt()
        {
            var s=Create();Equip(s,"I0AE");Tick(s,120);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));Tick(s,2);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(2));
            Assert.That(Call(s,"NemesisItemMagicVamp",1),Is.EqualTo(.06));
            Assert.That(Inventory(s).Transfer(OriginalInventoryBag.Hero,0).Applied,Is.True);
            Call(s,"SyncItemScriptInventory",Player(s));Tick(s,122);
            Assert.That(Inventory(s).Transfer(OriginalInventoryBag.Servant,0).Applied,Is.True);
            Call(s,"SyncItemScriptInventory",Player(s));Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(2));
        }
        [Test] public void DuelResetPreservesAmuletAndClearsOtherItemCooldowns()
        {
            var s=Create();Equip(s,"I0AE");Cooldowns(s)["1:A0WW"]=120d;Cooldowns(s)["1:A1E0"]=1d;
            Call(s,"ResetAbilityCooldowns",1,true);
            Assert.That(Cooldowns(s).Count,Is.EqualTo(1));Assert.That(Cooldowns(s)["1:A0WW"],Is.EqualTo(120d));
        }
        [Test] public void ItemTargetCodecRejectsNegativeAndNonItemCommandTargets()
        {
            var codec=new OriginalUnitySessionCodec();var command=new OriginalSessionCommand{
                sequence=1,kind=OriginalSessionCommandKind.UseItem,itemInstanceId=1,targetItemInstanceId=2};
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out var decoded),Is.True);
            Assert.That(decoded.targetItemInstanceId,Is.EqualTo(2));command.targetItemInstanceId=-1;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out _),Is.False);
            command.targetItemInstanceId=2;command.kind=OriginalSessionCommandKind.Stop;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out _),Is.False);
        }
    }
}

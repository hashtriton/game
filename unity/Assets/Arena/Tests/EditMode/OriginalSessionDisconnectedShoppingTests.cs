using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionDisconnectedShoppingTests
    {
        const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalWorld World(OriginalSession s)=>(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
        static OriginalItemCatalog Catalog(OriginalSession s)=>(OriginalItemCatalog)typeof(OriginalSession).GetField("itemCatalog",Hidden).GetValue(s);
        static OriginalSession Create()=>(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
        static OriginalSession CreateHero(string id)=>(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{id});

        [Test] public void ScriptShoppingUsesExactKnightSequenceAndScriptPriceWithoutNativeShopStock()
        {
            var s=Create();long before=Inventory(s).Gold;
            Assert.That(Call(s,"TryDisconnectedShoppingPurchase",Player(s),1),Is.True);
            Assert.That(Inventory(s).HeroSlots.Single(x=>x!=null).itemId,Is.EqualTo("I02A"));
            Assert.That(before-Inventory(s).Gold,Is.EqualTo(Catalog(s).QuickBuy("I02A").scriptGoldValue));
            Assert.That(Call(s,"TryDisconnectedShoppingPurchase",Player(s),2),Is.True);
            Assert.That(Inventory(s).HeroSlots.Any(x=>x!=null&&x.itemId=="I000"),Is.True);
        }
        [Test] public void EndOfPresetDoesNotDebitOrPublishCreatedInstance()
        {
            var s=Create();
            long before=Inventory(s).Gold;var ids=Inventory(s).HeroSlots.Select(x=>x?.instanceId??0).ToArray();
            Assert.That(Call(s,"TryDisconnectedShoppingPurchase",Player(s),28),Is.False);
            Assert.That(Inventory(s).Gold,Is.EqualTo(before));
            Assert.That(Inventory(s).HeroSlots.Select(x=>x?.instanceId??0),Is.EqualTo(ids));
        }
        [Test] public void PurchaseCapturesGoldBeforeRefundAndDiscardsRefundWhenSourceOverwritesBalance()
        {
            var s=Create();var candidate=Inventory(s).Copy();candidate.TryPickup(candidate.CreateInstance("I03L"));
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,World(s).UnitState(1)),Is.True);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
            long before=candidate.Gold;
            Assert.That(Call(s,"TryDisconnectedShoppingPurchase",Player(s),1),Is.True);
            Assert.That(Inventory(s).HeroSlots.Any(x=>x!=null&&x.itemId=="I03L"),Is.False);
            Assert.That(Inventory(s).Gold,Is.EqualTo(before-Catalog(s).QuickBuy("I02A").scriptGoldValue));
        }
        [Test] public void NoFundsStopsPresetButKeepsSourcePotionRefund()
        {
            var s=Create();var candidate=Inventory(s).Copy();candidate.TryPickup(candidate.CreateInstance("I03L"));
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,World(s).UnitState(1)),Is.True);
            candidate.TrySpendResources(candidate.Gold,0);Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
            Assert.That(Call(s,"TryDisconnectedShoppingPurchase",Player(s),1),Is.False);
            Assert.That(Inventory(s).HeroSlots.All(x=>x==null),Is.True);
            Assert.That(Inventory(s).Gold,Is.EqualTo(Catalog(s).QuickBuy("I03L").scriptGoldValue));
        }
        [Test] public void EnterRegionSchedulesDelayedShoppingAndInitialInsideIsNotSyntheticEntry()
        {
            var s=Create();var w=World(s);Player(s).GetType().GetField("connected").SetValue(Player(s),false);
            long initial=Inventory(s).Gold;Call(s,"AdvanceDisconnectedShopping");
            for(int i=0;i<70;i++){w.Advance(.05);Call(s,"AdvanceDisconnectedShopping");}
            Assert.That(Inventory(s).Gold,Is.EqualTo(initial));
            w.ForcePosition(1,new OriginalPoint(500,1000));Call(s,"AdvanceDisconnectedShopping");
            w.ForcePosition(1,new OriginalPoint(0,1000));Call(s,"AdvanceDisconnectedShopping");
            for(int i=0;i<39;i++){w.Advance(.05);Call(s,"AdvanceDisconnectedShopping");}
            Assert.That(Inventory(s).Gold,Is.EqualTo(initial));
            for(int i=0;i<30;i++){w.Advance(.05);Call(s,"AdvanceDisconnectedShopping");}
            Assert.That(Inventory(s).Gold,Is.LessThan(initial));
            var states=(IDictionary)typeof(OriginalSession).GetField("disconnectedShopping",Hidden).GetValue(s);
            Assert.That((int)states[1].GetType().GetField("index",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(states[1]),Is.GreaterThan(1));
        }
        [Test] public void AllThreeAuthoredPresetSequencesCanAdvanceWithoutStockDependency()
        {
            var failures=new System.Collections.Generic.List<string>();
            foreach(string hero in new[]{"H008","N0A0","H024"})
            {
                var s=CreateHero(hero);Inventory(s).GrantResources(1000000,0);
                for(int i=1;OriginalDisconnectedShoppingRules.Item(hero,i)!=null;i++)
                    if(!(bool)Call(s,"TryDisconnectedShoppingPurchase",Player(s),i))
                    {failures.Add(hero+" index="+i+" item="+OriginalDisconnectedShoppingRules.Item(hero,i));break;}
            }
            Assert.That(failures,Is.Empty);
        }
        [Test] public void PreparationPickupTakesOnlyOwnItemsInsideSourceRegion()
        {
            var s=Create();var inv=Inventory(s);
            var own=inv.CreateInstance("I000",1);var foreign=inv.CreateInstance("I000",2);var outside=inv.CreateInstance("I000",1);
            var ground=(System.Collections.Generic.IDictionary<long,OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems",Hidden).GetValue(s);
            ground.Add(own.instanceId,new OriginalGroundItemView{item=own,position=new OriginalPoint(0,1000)});
            ground.Add(foreign.instanceId,new OriginalGroundItemView{item=foreign,position=new OriginalPoint(0,1000)});
            ground.Add(outside.instanceId,new OriginalGroundItemView{item=outside,position=new OriginalPoint(300,1000)});
            Call(s,"DisconnectedPreparationPickup",Player(s),World(s).UnitState(1));
            Assert.That(Inventory(s).HeroSlots.Any(x=>x?.instanceId==own.instanceId),Is.True);
            Assert.That(ground.Keys,Is.EquivalentTo(new[]{foreign.instanceId,outside.instanceId}));
        }
        [Test] public void CombatSmartPickupUsesSharedRangeAndNativeGoldPipeline()
        {
            var s=Create();var w=World(s);w.SetUnitState(1,paused:false);Player(s).GetType().GetField("connected").SetValue(Player(s),false);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Hidden).GetValue(s);
            typeof(OriginalMatch).GetProperty("Round").SetValue(match,1);
            var item=Inventory(s).CreateInstance("gold",0);
            var ground=(System.Collections.Generic.IDictionary<long,OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems",Hidden).GetValue(s);
            ground.Add(item.instanceId,new OriginalGroundItemView{item=item,position=new OriginalPoint(650,1000)});
            Call(s,"DisconnectedCombatPickup",Player(s),w.UnitState(1));Call(s,"AdvanceDisconnectedGroundPickup");
            long before=Inventory(s).Gold;
            Assert.That(ground.ContainsKey(item.instanceId),Is.True);
            var goals=(IDictionary)typeof(OriginalSession).GetField("disconnectedGroundGoals",Hidden).GetValue(s);
            Assert.That(goals.Contains(1),Is.True);
            for(int i=0;i<60;i++){w.Advance(.05);Call(s,"AdvanceDisconnectedGroundPickup");}
            Assert.That(goals.Contains(1),Is.False);
            Assert.That(ground.ContainsKey(item.instanceId),Is.False);
            Assert.That(Inventory(s).Gold,Is.EqualTo(before+50));
            Assert.That(w.UnitState(1).position.x,Is.GreaterThan(450));
        }
        [Test] public void BossPhaseRoutingPreservesSourcePriorityAndQuadrantGrowthHold()
        {
            var s=Create();var w=World(s);w.SetUnitState(1,paused:false);
            object AddPhase(string field,string type,params object[] fields)
            {
                var value=Activator.CreateInstance(typeof(OriginalSession).GetNestedType(type,BindingFlags.NonPublic));
                for(int i=0;i<fields.Length;i+=2)value.GetType().GetField((string)fields[i],Hidden).SetValue(value,fields[i+1]);
                ((IDictionary)typeof(OriginalSession).GetField(field,Hidden).GetValue(s)).Add(6001,value);return value;
            }
            var quadrant=AddPhase("bossQuadrants","BossQuadrants","safe",2,"growing",false);
            var ghost=AddPhase("bossGhostPhases","BossGhostPhase","odd",true);
            Assert.That(Call(s,"DisconnectedBossRegion",Player(s)),Is.True);
            Assert.That(w.UnitState(1).destination.x,Is.EqualTo(32));Assert.That(w.UnitState(1).destination.y,Is.EqualTo(-3168));
            ghost.GetType().GetField("odd",Hidden).SetValue(ghost,false);Call(s,"DisconnectedBossRegion",Player(s));
            Assert.That(w.UnitState(1).destination.x,Is.EqualTo(256));
            ((IDictionary)typeof(OriginalSession).GetField("bossGhostPhases",Hidden).GetValue(s)).Clear();
            Call(s,"DisconnectedBossRegion",Player(s));Assert.That(w.UnitState(1).destination.x,Is.EqualTo(-432));
            Assert.That(w.UnitState(1).destination.y,Is.EqualTo(-2272));
            quadrant.GetType().GetField("growing",Hidden).SetValue(quadrant,true);w.Stop(1);
            Assert.That(Call(s,"DisconnectedBossRegion",Player(s)),Is.True);Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.None));
        }
        static void EnterSoulShop(OriginalSession s)
        {
            Player(s).GetType().GetField("connected").SetValue(Player(s),false);
            Call(s,"AdvanceDisconnectedShopping");
            World(s).ForcePosition(1,new OriginalPoint(500,1000));Call(s,"AdvanceDisconnectedShopping");
            World(s).ForcePosition(1,new OriginalPoint(0,1000));Call(s,"AdvanceDisconnectedShopping");
        }
        static void AdvanceSoulShopping(OriginalSession s,double seconds)
        {
            for(int i=0;i<(int)Math.Round(seconds/.05);i++)
            {World(s).Advance(.05);Call(s,"AdvanceSoulUpgrades");Call(s,"AdvanceDisconnectedShopping");}
        }
        [Test] public void SoulShoppingCompletesAllSixtyAuthoredBasicsAtSourceTotalCost()
        {
            var s=Create();var inv=Inventory(s);inv.TrySpendResources(inv.Gold,inv.Lumber);inv.GrantResources(0,390);
            EnterSoulShop(s);AdvanceSoulShopping(s,4);
            Assert.That(Call(s,"SoulUpgradeRank",1,"R002"),Is.EqualTo(0));
            AdvanceSoulShopping(s,124);
            foreach(string id in new[]{"R002","R003","R001","R000","R004","R006"})
                Assert.That(Call(s,"SoulUpgradeRank",1,id),Is.EqualTo(10),id);
            Assert.That(Inventory(s).Lumber,Is.EqualTo(0));
        }
        [Test] public void SoulShoppingResumesAtNextSourceIndexAfterFundsAndFreshRegionEntry()
        {
            var s=Create();var inv=Inventory(s);inv.TrySpendResources(inv.Gold,inv.Lumber);inv.GrantResources(0,4);
            EnterSoulShop(s);AdvanceSoulShopping(s,12);
            Assert.That(Call(s,"SoulUpgradeRank",1,"R002"),Is.EqualTo(1));
            Assert.That(Call(s,"SoulUpgradeRank",1,"R003"),Is.EqualTo(1));
            Assert.That(Call(s,"SoulUpgradeRank",1,"R001"),Is.EqualTo(0));
            inv.GrantResources(0,4);AdvanceSoulShopping(s,4);
            Assert.That(Call(s,"SoulUpgradeRank",1,"R001"),Is.EqualTo(0));
            EnterSoulShop(s);AdvanceSoulShopping(s,12);
            Assert.That(Call(s,"SoulUpgradeRank",1,"R001"),Is.EqualTo(1));
            Assert.That(Call(s,"SoulUpgradeRank",1,"R000"),Is.EqualTo(1));
            Assert.That(Inventory(s).Lumber,Is.EqualTo(0));
        }
    }
}


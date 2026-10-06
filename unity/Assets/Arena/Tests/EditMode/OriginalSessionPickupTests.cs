using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionPickupTests
    {
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        static object Player(OriginalSession session) => ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
        static OriginalInventory Inventory(OriginalSession session) => (OriginalInventory)Player(session).GetType().GetField("inventory").GetValue(Player(session));
        static OriginalWorld World(OriginalSession session) => (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
        static object Prepare(OriginalSession session, string id, OriginalInventory candidate = null) =>
            typeof(OriginalSession).GetMethod("PreparePowerupPickup", Private).Invoke(session,
                new[] { Player(session), (object)World(session).UnitState(1), candidate ?? Inventory(session).Copy(), id });
        static T Field<T>(object value, string field) => (T)value.GetType().GetField(field, Private | BindingFlags.Public).GetValue(value);
        static void Commit(OriginalSession session, object prepared) => typeof(OriginalSession).GetMethod("CommitPowerupPickup", Private).Invoke(session, new[] { prepared });
        static OriginalSessionCommand Command(OriginalSession session, OriginalSessionCommandKind kind, long item = 0, string offer = null) =>
            new OriginalSessionCommand { sequence = session.Snapshot().players[0].acknowledgedSequence + 1,
                kind = kind, itemInstanceId = item, itemId = offer, shopInstanceId = 203, bag = OriginalInventoryBag.Hero };
        static long Ground(OriginalSession session, string itemId)
        {
            var item = Inventory(session).CreateInstance(itemId);
            var registry = (SortedDictionary<long, OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems", Private).GetValue(session);
            registry.Add(item.instanceId, new OriginalGroundItemView { item = item, position = World(session).UnitState(1).position });
            return item.instanceId;
        }
        static void InterRoundShopFixture(OriginalSession session)
        {
            // The source u00E inter-round shop offers these powerups. Its
            // lifecycle is outside this test: placement isolates authority.
            var placements = typeof(OriginalSession).GetField("shopPlacements", Private);
            placements.SetValue(session, ((OriginalShopView[])placements.GetValue(session)).Where(s => s.instanceId != 203).Concat(new[] {
                new OriginalShopView { instanceId = 203, unitId = "u00E", position = World(session).UnitState(1).position } }).ToArray());
            typeof(OriginalSession).GetField("centralShopsOpen", Private).SetValue(session, true);
            typeof(OriginalSession).GetField("acolyteSpawnAt", Private).SetValue(session, 0d);
            typeof(OriginalSession).GetField("acolyteStockInitialized", Private).SetValue(session, true);
            var stocks = (IDictionary)typeof(OriginalSession).GetField("shopStocks", Private).GetValue(session);
            var type = typeof(OriginalSession).GetNestedType("ShopStock", BindingFlags.NonPublic);
            foreach (string id in new[] { "I07W", "I0AI", "I05F", "I0AT", "I0B8" })
            {
                var stock = Activator.CreateInstance(type, true);
                type.GetField("known", Private).SetValue(stock, true); type.GetField("count", Private).SetValue(stock, 1);
                type.GetField("maximum", Private).SetValue(stock, 1); type.GetField("period", Private).SetValue(stock, 1d);
                stocks["203:" + id] = stock;
            }
        }

        [Test] public void PowerupCurrencyPublishesOnlyOnCommitAndNeverOccupiesInventory()
        {
            var session = Create(); long before = Inventory(session).Gold;
            var prepared = Prepare(session, "I07W");
            Assert.That(Field<bool>(prepared, "handled"), Is.True);
            Assert.That(Field<OriginalItemActionCode>(prepared, "code"), Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(Inventory(session).Gold, Is.EqualTo(before));
            Commit(session, prepared);
            Assert.That(Inventory(session).Gold - before, Is.InRange(20L, 70L));
            Assert.That(Inventory(session).HeroSlots.All(item => item == null), Is.True);
            Assert.That(Inventory(session).ServantSlots.All(item => item == null), Is.True);
        }

        [Test] public void UncommittedPreparationDoesNotAdvanceRandomOrPublishCurrency()
        {
            var first = Create(); var second = Create();
            var uncommitted = Prepare(first, "I0AT");
            Assert.That(Field<OriginalItemActionCode>(uncommitted, "code"), Is.EqualTo(OriginalItemActionCode.Success));
            var a = Prepare(first, "I07W"); var b = Prepare(second, "I07W");
            Commit(first, a); Commit(second, b);
            Assert.That(Inventory(first).Gold, Is.EqualTo(Inventory(second).Gold));
            Assert.That(Prepare(first, "I000") is object, Is.True);
            Assert.That(Field<bool>(Prepare(first, "I000"), "handled"), Is.False);
        }

        [Test] public void ExperiencePickupIsIndependentOfMatchWatermarkAndUpdatesProfileAtomically()
        {
            var session = Create(); var before = session.Snapshot().players[0].progression;
            var prepared = Prepare(session, "I05F");
            Assert.That(Field<OriginalItemActionCode>(prepared, "code"), Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(session.Snapshot().players[0].progression.experience, Is.EqualTo(before.experience));
            Commit(session, prepared);
            var after = session.Snapshot().players[0].progression;
            Assert.That(after.experience - before.experience, Is.InRange(200, 700));
            Assert.That(after.matchExperienceWatermark, Is.EqualTo(before.matchExperienceWatermark));
            Assert.That(after.level, Is.GreaterThan(before.level));
            Assert.That(World(session).UnitState(1).profile.maxHealth, Is.GreaterThan(631));
            typeof(OriginalSession).GetMethod("SyncProgressionExperience", Private).Invoke(session, new[] { Player(session) });
            Assert.That(session.Snapshot().players[0].progression.experience, Is.EqualTo(after.experience));
        }

        [Test] public void PreparationCannotBeCommittedTwiceOrAfterInventoryReplacement()
        {
            var session = Create(); var prepared = Prepare(session, "I0AI");
            Commit(session, prepared); long after = Inventory(session).Lumber;
            Assert.Throws<TargetInvocationException>(() => Commit(session, prepared));
            Assert.That(Inventory(session).Lumber, Is.EqualTo(after));
            prepared = Prepare(session, "I0AI");
            Player(session).GetType().GetField("inventory").SetValue(Player(session), Inventory(session).Copy());
            Assert.Throws<TargetInvocationException>(() => Commit(session, prepared));
            Assert.That(Inventory(session).Lumber, Is.EqualTo(after));
        }

        [Test] public void AreaGoldPaysOnlyLivingAlliedHeroAndNotIllusionOrEnemy()
        {
            var session = Create(); var world = World(session); var hero = world.UnitState(1);
            world.AddUnit(1001, 0, "n008", hero.profile, new OriginalPoint(300, 1000));
            long before = Inventory(session).Gold;
            Commit(session, Prepare(session, "I05U"));
            Assert.That(Inventory(session).Gold - before, Is.EqualTo(10 * session.Snapshot().round));
        }

        [Test] public void GroundPowerupCommandIsRemovedExactlyOnceAndReplayCannotGrantAgain()
        {
            var session = Create(); long id = Ground(session, "I07W"), before = Inventory(session).Gold;
            var command = Command(session, OriginalSessionCommandKind.PickupItem, id);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            long after = Inventory(session).Gold;
            Assert.That(after - before, Is.InRange(20L, 70L));
            Assert.That(session.Snapshot().groundItems, Is.Empty);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Apply(0, Command(session, OriginalSessionCommandKind.PickupItem, id)), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(Inventory(session).Gold, Is.EqualTo(after));
        }

        [Test] public void WeightedChestPublishesOneOwnedItemAndReplayCannotGrantAnother()
        {
            var session = Create(); long id = Ground(session, "I0AT"), before = Inventory(session).Gold;
            var command=Command(session,OriginalSessionCommandKind.PickupItem,id);
            Assert.That(session.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Snapshot().groundItems,Is.Empty);
            Assert.That(Inventory(session).Gold, Is.EqualTo(before));
            Assert.That(Inventory(session).HeroSlots.Count(x=>x!=null),Is.EqualTo(1));
        }

        [Test] public void EveryFatePoolOutcomeCanBeEquippedAndFullBagKeepsOwnedRewardOnGround()
        {
            var session=Create();var inventory=Inventory(session);var player=Player(session);
            foreach(string id in Enumerable.Range(1,OriginalItemFatePool.TotalWeight).Select(OriginalItemFatePool.At).Distinct())
            {
                var candidate=inventory.Copy();Assert.That(candidate.TryPickup(candidate.CreateInstance(id)).Applied,Is.True,id);
                Assert.DoesNotThrow(()=>typeof(OriginalSession).GetMethod("PrepareEquipmentProfile",Private)
                    .Invoke(session,new object[]{player,candidate,World(session).UnitState(1)}),id);
            }
            for(int i=0;i<6;i++)Assert.That(inventory.TryPickup(inventory.CreateInstance("I007")).Applied,Is.True);
            Assert.That(inventory.HeroSlots.Count(x=>x!=null),Is.EqualTo(6));
            long chest=Ground(session,"I0AT");
            Assert.That(session.Apply(0,Command(session,OriginalSessionCommandKind.PickupItem,chest)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var reward=session.Snapshot().groundItems.Single();
            Assert.That(reward.item.instanceId,Is.Not.EqualTo(chest));Assert.That(reward.item.ownerId,Is.EqualTo(1));
            Assert.That(reward.item.itemId,Is.Not.EqualTo("I0AT"));Assert.That(inventory.HeroSlots.Count(x=>x!=null),Is.EqualTo(6));
        }

        [Test] public void SourcePowerupBuyAndWeightedChestDebitOnceAndConsumeStock()
        {
            var session = Create(); InterRoundShopFixture(session);
            long gold = Inventory(session).Gold, lumber = Inventory(session).Lumber;
            var command = Command(session, OriginalSessionCommandKind.BuyItem, offer: "I05F");
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(session).Gold, Is.EqualTo(gold - 70));
            Assert.That(Inventory(session).Lumber, Is.EqualTo(lumber - 9));
            Assert.That(session.Snapshot().players[0].experience, Is.InRange(200, 700));
            Assert.That(Inventory(session).HeroSlots.All(x => x == null), Is.True);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            gold = Inventory(session).Gold; lumber = Inventory(session).Lumber;
            Assert.That(session.Apply(0, Command(session, OriginalSessionCommandKind.BuyItem, offer: "I0AT")), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(session).Gold, Is.EqualTo(gold-1200)); Assert.That(Inventory(session).Lumber, Is.EqualTo(lumber-19));
            Assert.That(session.Snapshot().shops.Single(s => s.instanceId == 203).stock.Single(s => s.itemId == "I0AT").available, Is.EqualTo(0));
        }
        [Test] public void NativeCurrencyAndRunesUsePublicPickupWithoutInventoryResidency()
        {
            var s=Create();var w=World(s);var u=w.UnitState(1);w.UpdateProfile(1,u.profile,100,0);
            long gold=Inventory(s).Gold,lumber=Inventory(s).Lumber;
            foreach(string id in new[]{"gold","lmbr","rres"})
            {
                long instance=Ground(s,id);var command=Command(s,OriginalSessionCommandKind.PickupItem,instance);
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence),id);
            }
            Assert.That(Inventory(s).Gold,Is.EqualTo(gold+50));Assert.That(Inventory(s).Lumber,Is.EqualTo(lumber+2));
            Assert.That(w.UnitState(1).health,Is.EqualTo(400));Assert.That(w.UnitState(1).mana,Is.EqualTo(145));
            Assert.That(Inventory(s).HeroSlots.All(x=>x==null),Is.True);
        }
        [Test] public void SourceTomesPersistAcrossExperienceSyncAndInventoryRecomposition()
        {
            var s=Create();var w=World(s);
            foreach(string id in new[]{"tstr","tdex","tint"})
                Assert.That(s.Apply(0,Command(s,OriginalSessionCommandKind.PickupItem,Ground(s,id))),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
            Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(639));Assert.That(w.UnitState(1).profile.maxMana,Is.EqualTo(155));
            typeof(OriginalSession).GetMethod("SyncProgressionExperience",Private).Invoke(s,new[]{Player(s)});
            var stats=(OriginalHeroStatsSnapshot)typeof(OriginalSession).GetMethod("HeroCombatStats",Private).Invoke(s,new object[]{1});
            Assert.That(stats.strength.Require(),Is.EqualTo(23));Assert.That(stats.agility.Require(),Is.EqualTo(7));Assert.That(stats.intelligence.Require(),Is.EqualTo(8));
            Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void NativeSpeedRunePublishesAlliedHasteOnlyAtCommitAndRestoresOnExpiry()
        {
            var s=Create();var w=World(s);var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=100,maxMana=0,moveSpeed=200,collisionRadius=8};
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=100000001,ownerSlot=1,
                sourceHeroEntityId=1,rawcode="hfoo",profile=profile,position=new OriginalPoint(300,1000),health=100,mana=0}}),Is.True);
            w.AddUnit(9001,0,"hfoo",profile,new OriginalPoint(500,1000));
            var prepared=Prepare(s,"rspd");
            Assert.That(Field<bool>(prepared,"handled"),Is.True);
            Assert.That(Field<OriginalItemActionCode>(prepared,"code"),Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(hero.profile.moveSpeed));
            Assert.That(w.UnitState(100000001).profile.moveSpeed,Is.EqualTo(200));
            Commit(s,prepared);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(522));
            Assert.That(w.UnitState(100000001).profile.moveSpeed,Is.EqualTo(522));
            Assert.That(w.UnitState(9001).profile.moveSpeed,Is.EqualTo(200));
            typeof(OriginalSession).GetMethod("AdvanceItemStatuses",Private).Invoke(s,new object[]{14.9d});
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(522));
            typeof(OriginalSession).GetMethod("AdvanceItemStatuses",Private).Invoke(s,new object[]{.11d});
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(hero.profile.moveSpeed));
            Assert.That(w.UnitState(100000001).profile.moveSpeed,Is.EqualTo(200));
            Assert.Throws<TargetInvocationException>(()=>Commit(s,prepared));
            Assert.That(Inventory(s).HeroSlots.All(x=>x==null),Is.True);
        }

    }
}

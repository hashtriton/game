using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemRulesTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));

        static OriginalItemCatalog Items()
        {
            var result = new OriginalItemCatalog { mapSha256 = OriginalNativeCatalog.ExpectedMapSha256 };
            result.items = new[] { Item("shop"), Item("held"), Item("zero") };
            result.shops = new[] { new OriginalShopDefinition { unitId = "vend", offerIds = new[] { "shop", "zero" } } };
            result.conversions = new[] { new OriginalItemConversion { worldShopId = "shop", inventoryId = "held" } };
            return result;
        }

        static OriginalItemDefinition Item(string id) => new OriginalItemDefinition
        {
            id = id, classId = "Permanent", goldCost = OriginalDeclaredInt.Known(99),
            lumberCost = OriginalDeclaredInt.Known(99), initialCharges = OriginalDeclaredInt.Known(99),
            droppable = OriginalDeclaredInt.Known(1)
        };

        // The declarations below are test data; they are not claims about native defaults.
        static OriginalNativeValue Value(string field, int number) => new OriginalNativeValue
        {
            field = field, known = true, state = "map-declaration", kind = "number", number = number,
            sources = new[] { new OriginalNativeEvidence { source = 0, line = 1 } }
        };

        static OriginalNativeCatalog Native()
        {
            var native = Load<OriginalNativeCatalog>("lia39-native126");
            native.items = new[] { Row("shop", 23, 3, 0, 1), Row("held", 91, 9, 7, 0), Row("zero", 0, 0, 2, 1) };
            return native;
        }

        static OriginalNativeObject Row(string id, int gold, int lumber, int uses, int drop) => new OriginalNativeObject
        {
            id = id, fields = new[] { Value("goldcost", gold), Value("lumbercost", lumber), Value("uses", uses), Value("droppable", drop) }
        };

        [Test] public void BuyQuoteAndPurchaseUseTheExactOfferIdAndBothCurrencies()
        {
            var items = Items(); var rules = new OriginalItemRules(items, Native());
            var inventory = new OriginalInventory(items, 1, 100, 10, itemRules: rules);
            var quote = inventory.QuoteBuy("vend", "shop");
            Assert.That(quote.Code, Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(quote.GoldCost, Is.EqualTo(23)); Assert.That(quote.LumberCost, Is.EqualTo(3));
            Assert.That(inventory.Gold, Is.EqualTo(100)); Assert.That(inventory.Snapshot().heroSlots.All(x => x == null), Is.True);
            var action = inventory.TryBuy("vend", "shop");
            Assert.That(action.Code, Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(action.Item.itemId, Is.EqualTo("held"));
            Assert.That(action.Item.chargesKnown, Is.True); Assert.That(action.Item.charges, Is.EqualTo(7));
            Assert.That(inventory.Gold, Is.EqualTo(77)); Assert.That(inventory.Lumber, Is.EqualTo(7));
            Assert.That(inventory.Drop(OriginalInventoryBag.Hero, 0).Code, Is.EqualTo(OriginalItemActionCode.NotAllowed));
        }

        [Test] public void DeclaredZeroIsKnownAndDoesNotBorrowAnotherItemsValues()
        {
            var items = Items(); var rules = new OriginalItemRules(items, Native());
            var inventory = new OriginalInventory(items, 1, itemRules: rules);
            Assert.That(inventory.TryBuy("vend", "zero").Applied, Is.True);
            Assert.That(inventory.HeroSlots[0].charges, Is.EqualTo(2));
            Assert.That(inventory.Gold, Is.Zero); Assert.That(inventory.Lumber, Is.Zero);
            Assert.That(inventory.QuoteBuy("vend", "held").Code, Is.EqualTo(OriginalItemActionCode.NotSoldHere));
            Assert.That(inventory.QuoteBuy("vend", "shop").Code, Is.EqualTo(OriginalItemActionCode.NotEnoughResources));
        }

        [Test] public void UnknownNativeFieldCannotFallBackToTheFlatCatalogOrAnotherDataset()
        {
            foreach (var state in new[] { "unresolved-native-dataset-selection", "unresolved-profile-precedence", "unresolved-absent-map-slk-cell" })
            {
                var items = Items(); var native = Native();
                var lumber = native.items[0].fields.Single(x => x.field == "lumbercost");
                lumber.known = false; lumber.state = state; lumber.number = 0;
                var inventory = new OriginalInventory(items, 1, 1000, 1000, itemRules: new OriginalItemRules(items, native));
                Assert.That(inventory.TryBuy("vend", "shop").Code, Is.EqualTo(OriginalItemActionCode.UnresolvedRule));
                Assert.That(inventory.Gold, Is.EqualTo(1000)); Assert.That(inventory.Lumber, Is.EqualTo(1000));
                Assert.That(inventory.HeroSlots.All(x => x == null), Is.True);
            }
        }

        [Test] public void ResolverAndReturnedEvidenceAreDetachedFromInputMutation()
        {
            var items = Items(); var native = Native(); var rules = new OriginalItemRules(items, native);
            var first = rules.Field("shop", "goldcost"); var source = first.sources[0];
            native.items[0].fields[0].number = 300; native.items[0].fields[0].sources[0].line = 600;
            items.items[0].goldCost.value = 900; first.value = 700; first.sources[0] = "changed";
            var next = rules.Field("shop", "goldcost");
            Assert.That(next.known, Is.True); Assert.That(next.value, Is.EqualTo(23));
            Assert.That(next.sources[0], Is.EqualTo(source));
            Assert.That(new OriginalInventory(items, 1, 100, 10, itemRules: rules).TryBuy("vend", "shop").GoldCost, Is.EqualTo(23));
        }

        [Test] public void InvalidNumericRulesAreRejectedInsteadOfTruncatedOrCoerced()
        {
            foreach (double bad in new[] { -1d, .5, (double)int.MaxValue + 1 })
            {
                var native = Native(); native.items[0].fields[0].number = bad;
                Assert.Throws<ArgumentException>(() => new OriginalItemRules(Items(), native));
            }
            var flag = Native(); flag.items[0].fields.Single(x => x.field == "droppable").number = 2;
            Assert.Throws<ArgumentException>(() => new OriginalItemRules(Items(), flag));
        }

        [Test] public void ResolverRejectsDifferentMapAndDifferentInventoryCatalog()
        {
            var items = Items(); items.mapSha256 = new string('0', 64);
            Assert.Throws<ArgumentException>(() => new OriginalItemRules(items, Native()));
            items = Items(); var rules = new OriginalItemRules(items, Native());
            Assert.Throws<ArgumentException>(() => new OriginalInventory(Items(), 1, itemRules: rules));
            Assert.Throws<ArgumentException>(() => new OriginalInventory(items, 1,
                engineDefaults: new OriginalItemEngineDefaults(), itemRules: rules));
        }

        [Test] public void SnapshotAndPublicSlotReadsCannotMutateEitherBagOrBalances()
        {
            var items = Items(); var inventory = new OriginalInventory(items, 3, 80, 10,
                itemRules: new OriginalItemRules(items, Native())) { QuickBuyEnabled = true };
            inventory.TryPickup(inventory.CreateInstance("zero", 3));
            inventory.TryPickup(inventory.CreateInstance("zero", 3), OriginalInventoryBag.Servant);
            var snapshot = inventory.Snapshot();
            Assert.That(snapshot.heroSlots.Length, Is.EqualTo(6)); Assert.That(snapshot.servantSlots.Length, Is.EqualTo(6));
            Assert.That(snapshot.ownerId, Is.EqualTo(3)); Assert.That(snapshot.quickBuyEnabled, Is.True);
            snapshot.gold = 999; snapshot.lumber = 999; snapshot.heroSlots[0].charges = 999;
            snapshot.servantSlots[0].itemId = "bad!"; snapshot.heroSlots[1] = snapshot.heroSlots[0];
            var slots = inventory.HeroSlots; slots[0].charges = 888; slots[0] = null;
            var servant = inventory.ServantSlots; servant[0].ownerId = 8;
            var after = inventory.Snapshot();
            Assert.That(after.gold, Is.EqualTo(80)); Assert.That(after.lumber, Is.EqualTo(10));
            Assert.That(after.heroSlots[0].charges, Is.EqualTo(2)); Assert.That(after.heroSlots[1], Is.Null);
            Assert.That(after.servantSlots[0].itemId, Is.EqualTo("zero")); Assert.That(after.servantSlots[0].ownerId, Is.EqualTo(3));
        }

        [Test] public void EntireSourceCatalogIsRetainedAndUnknownLumberRemainsExplicit()
        {
            var items = Load<OriginalItemCatalog>("lia39-items"); var native = Load<OriginalNativeCatalog>("lia39-native126");
            var rules = new OriginalItemRules(items, native);
            Assert.That(rules.ItemCount, Is.EqualTo(440));
            foreach (var item in items.items) Assert.That(rules.Field(item.id, "uses"), Is.Not.Null);
            Assert.That(rules.Field("I000", "goldcost").value, Is.EqualTo(65));
            Assert.That(rules.Field("I000", "lumbercost").known, Is.False);
            Assert.That(rules.Field("I000", "lumbercost").state, Is.EqualTo("unresolved-absent-map-slk-cell"));
            Assert.That(items.Item("I000").lumberCost.known, Is.False);
            Assert.That(new OriginalInventory(items, 1, 1000, 1000, itemRules: rules).TryBuy("n004", "I04J").Code,
                Is.EqualTo(OriginalItemActionCode.UnresolvedRule));
        }
    }
}

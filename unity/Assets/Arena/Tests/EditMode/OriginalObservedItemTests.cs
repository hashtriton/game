using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalObservedItemTests
    {
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file + ".json")));
        [Test] public void ActualControlledCapturesResolveAllChargesAndAllOriginalShopOfferPrices()
        {
            var data = Load<OriginalObservedItemCatalog>("lia39-observed-items126");
            var items = Load<OriginalItemCatalog>("lia39-items");
            var rules = new OriginalItemRules(items, Load<OriginalNativeCatalog>("lia39-native126"), data);
            Assert.That(data.source.cacheSha256, Is.EqualTo("ae7ed283712c7c7d9f6fbab40b3014a4f057cfb0e3c7f015f92e0c917712dc53"));
            Assert.That(data.items.Count(x => x.directKnown), Is.EqualTo(440));
            Assert.That(data.items.Count(x => x.priceKnown), Is.EqualTo(391));
            Assert.That(data.source.nonSellableSkipped, Is.EqualTo(49));
            foreach (var item in items.items) Assert.That(rules.Field(item.id, "uses").known, Is.True, item.id);
            Assert.That(rules.Field("I04J", "goldcost").value, Is.EqualTo(65));
            Assert.That(rules.Field("I04J", "lumbercost").known, Is.True);
            Assert.That(rules.Field("I04J", "lumbercost").value, Is.Zero);
            Assert.That(rules.Field("I05F", "uses").value, Is.EqualTo(1));
            Assert.That(rules.Field("I05F", "goldcost").value, Is.EqualTo(70));
            Assert.That(rules.Field("I05F", "lumbercost").value, Is.EqualTo(9));
            Assert.That(data.Field("I0B8", "goldcost").known, Is.True);
            Assert.That(rules.Field("I0B8", "goldcost").value, Is.EqualTo(2));
            Assert.That(data.Field("I000", "stockStart").known, Is.False);
            var offers = items.shops.SelectMany(x => x.offerIds).Distinct().ToArray();
            Assert.That(offers.Length, Is.EqualTo(134));
            Assert.That(offers.Count(x => data.Field(x, "goldcost").known), Is.EqualTo(134));
            var inventory = new OriginalInventory(items, 1, 130, 4, itemRules: rules);
            Assert.That(inventory.TryBuy("n004", "I04J").Applied, Is.True);
            Assert.That(inventory.HeroSlots[0].itemId, Is.EqualTo("I000"));
            Assert.That(inventory.Gold, Is.EqualTo(65)); Assert.That(inventory.Lumber, Is.EqualTo(4));
        }
        [Test] public void SkippedPurchaseClosesPotionPriceWithoutChangingSellableOrResourcePowerupEvidence()
        {
            var data = Load<OriginalObservedItemCatalog>("lia39-observed-items126"); data.BuildIndexes();
            Assert.That(data.skippedPurchases.items.Length, Is.EqualTo(49));
            Assert.That(data.skippedPurchases.items.Count(i => i.priceKnown), Is.EqualTo(47));
            Assert.That(data.Field("I06F", "goldcost").value, Is.EqualTo(6));
            Assert.That(data.Field("I06F", "lumbercost").known, Is.True);
            Assert.That(data.Field("I06F", "lumbercost").value, Is.Zero);
            Assert.That(data.Field("I06F", "sellable").value, Is.Zero);
            Assert.That(data.Field("I06F", "goldcost").sources[0], Does.Contain("LiAItemsS1.w3v"));
            Assert.That(data.Field("gold", "goldcost").known, Is.False);
            Assert.That(data.Field("lmbr", "lumbercost").known, Is.False);
            Assert.That(data.items.Single(i => i.id == "I06F").priceKnown, Is.False);
        }
        [Test] public void InvalidSkippedBatchCannotReplacePreviouslyPublishedDetachedPrices()
        {
            var data = Load<OriginalObservedItemCatalog>("lia39-observed-items126"); data.BuildIndexes();
            data.skippedPurchases.items.Single(i => i.id == "I06F").goldDebit = -1;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            Assert.That(data.Field("I06F", "goldcost").value, Is.EqualTo(6));
            var copy = data.Field("I06F", "goldcost"); copy.value = 10; copy.sources[0] = "changed";
            Assert.That(data.Field("I06F", "goldcost").value, Is.EqualTo(6));
            data = Load<OriginalObservedItemCatalog>("lia39-observed-items126"); data.skippedPurchases.source.controlsPassed = false;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            data = Load<OriginalObservedItemCatalog>("lia39-observed-items126"); data.skippedPurchases.items[0].id = "I000";
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
        }
        // Synthetic observations exercise validation; they make no native runtime claim.
        static OriginalObservedItemCatalog Fixture()
        {
            var items = Load<OriginalItemCatalog>("lia39-items");
            return new OriginalObservedItemCatalog
            {
                schemaVersion = 1, mapSha256 = OriginalNativeCatalog.ExpectedMapSha256, engineVersion = "1.26.0.6401",
                runtimeObserved = true,
                source = new OriginalObservedItemSource
                {
                    cacheName = "LiAItemsB1.w3v", cacheSha256 = new string('a', 64),
                    probeMapSha256 = OriginalObservedItemCatalog.ExpectedProbeMapSha256,
                    probeScriptSha256 = OriginalObservedItemCatalog.ExpectedProbeScriptSha256,
                    capturedUtc = "2026-10-05T21:30:00Z", complete = true, controlsPassed = true,
                    records = 448, passed = 448, failed = 0, directKnown = 448, priceObserved = 4, nonSellableSkipped = 440
                },
                items = items.items.Select(x => new OriginalObservedItem
                {
                    id = x.id, sourceKey = "item_" + x.id, directKnown = true, recordPassed = true,
                    priceState = "native-nonsellable"
                }).ToArray()
            };
        }
        static OriginalObservedItem Purchased(OriginalObservedItemCatalog data, string id, int gold, int lumber)
        {
            var item = data.items.Single(x => x.id == id);
            item.sellable = true; item.pawnable = true; item.purchaseAttempted = true; item.priceObserved = true;
            item.priceKnown = id != "gold" && id != "lmbr"; item.goldDebit = gold; item.lumberDebit = lumber;
            item.priceState = item.priceKnown ? "observed-stable-debit" : "unresolved-resource-changing-powerup";
            data.source.priceObserved++; data.source.nonSellableSkipped--;
            return item;
        }
        [Test] public void MeasuredZeroClosesOnlyTheExactMeasuredField()
        {
            var data = Fixture(); Purchased(data, "I000", 65, 0);
            var items = Load<OriginalItemCatalog>("lia39-items");
            var native = Load<OriginalNativeCatalog>("lia39-native126");
            // Only I000 is observed here; remove the synthetic other direct measurements.
            foreach (var row in data.items.Where(x => x.id != "I000"))
            { row.directKnown = false; row.recordPassed = false; row.error = "synthetic missing row"; row.priceState = "unresolved-item-creation"; }
            data.source.directKnown = 9; data.source.passed = 9; data.source.failed = 439; data.source.nonSellableSkipped = 0;
            var rules = new OriginalItemRules(items, native, data);
            Assert.That(rules.Field("I000", "lumbercost").known, Is.True);
            Assert.That(rules.Field("I000", "lumbercost").value, Is.Zero);
            Assert.That(rules.Field("I000", "uses").known, Is.True);
            Assert.That(rules.Field("I001", "lumbercost").known, Is.False);
            Assert.That(rules.Field("I000", "stockStart").known, Is.False);
            Assert.That(items.Item("I000").lumberCost.known, Is.False);
        }
        [Test] public void NetDebitOfResourcePowerupsCannotBecomeAGrossPrice()
        {
            var data = Fixture(); Purchased(data, "gold", 100, 0); Purchased(data, "lmbr", 150, 0);
            data.BuildIndexes();
            Assert.That(data.Field("gold", "goldcost").known, Is.False);
            Assert.That(data.Field("lmbr", "lumbercost").known, Is.False);
            Assert.That(data.Field("gold", "uses").known, Is.True);
            data.items.Single(x => x.id == "gold").priceKnown = true;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
        }
        [Test] public void IncompleteControlsDuplicateIdsAndInvalidIntegersFailClosed()
        {
            var data = Fixture(); data.source.controlsPassed = false;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            data = Fixture(); data.items[1].id = data.items[0].id; data.items[1].sourceKey = data.items[0].sourceKey;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            data = Fixture(); data.items[0].initialCharges = -1;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            data = Fixture(); data.source.directKnown--;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
        }
        [Test] public void ObservedLookupIsDetachedAndFailedRebuildPreservesPriorIndexes()
        {
            var data = Fixture(); Purchased(data, "I000", 65, 0); data.BuildIndexes();
            var value = data.Field("I000", "goldcost"); value.value = 900; value.sources[0] = "changed";
            data.items.Single(x => x.id == "I000").goldDebit = -1;
            Assert.Throws<ArgumentException>(() => data.BuildIndexes());
            Assert.That(data.Field("I000", "goldcost").value, Is.EqualTo(65));
            Assert.That(data.Field("I000", "goldcost").sources[0], Does.Contain("LiAItemsB1.w3v"));
        }
        [Test] public void ProvenNativeConflictIsRejectedRatherThanOverridden()
        {
            var data = Fixture(); Purchased(data, "I000", 66, 0);
            Assert.Throws<ArgumentException>(() => new OriginalItemRules(Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalNativeCatalog>("lia39-native126"), data));
        }
        [Test] public void PricesAndNativeFlagsHaveSeparateSuccessGates()
        {
            var data = Fixture(); var item = data.items.Single(x => x.id == "I000");
            item.sellable = true; item.purchaseAttempted = true; item.recordPassed = false;
            item.error = "Purchase failed"; item.priceState = "unresolved-purchase-failed";
            data.source.passed--; data.source.failed++; data.source.nonSellableSkipped--;
            data.BuildIndexes();
            Assert.That(data.Field("I000", "uses").known, Is.True);
            Assert.That(data.Field("I000", "sellable").value, Is.EqualTo(1));
            Assert.That(data.Field("I000", "goldcost").known, Is.False);
        }
    }
}

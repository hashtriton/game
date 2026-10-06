using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalObservedShopStockTests
    {
        [Serializable] sealed class Container { public OriginalObservedShopStock readyStock = null; }
        static OriginalObservedShopStock Load() => JsonUtility.FromJson<Container>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-observed-items126.json"))).readyStock;
        static OriginalItemCatalog Items() => JsonUtility.FromJson<OriginalItemCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-items.json")));
        [Test] public void All275OriginalOffersHaveMeasuredReadyCountsWithoutResolvingStart()
        {
            var stock = Load(); var items = Items(); stock.BuildIndexes(items);
            Assert.That(stock.OfferCount, Is.EqualTo(275));
            foreach (var shop in items.shops) foreach (var id in shop.offerIds)
            {
                var item = Array.Find(items.items, value => value.id == id);
                Assert.That(stock.ReadyCount(shop.unitId, id), Is.EqualTo(item.stockMax.value));
                Assert.That(item.stockStart.known, Is.False);
            }
            Assert.That(stock.TryGetReadyCount("n004", "XXXX", out _), Is.False);
            Assert.Throws<InvalidOperationException>(() => stock.ReadyCount("n004", "XXXX"));
        }
        [Test] public void ReadinessAndMaximumCorruptionCannotBecomeUsableStock()
        {
            foreach (Action<OriginalObservedShopStock> corrupt in new Action<OriginalObservedShopStock>[] {
                s => s.offers[0].known = false, s => s.offers[0].readyCount++, s => s.offers[0].exhaustedAfterAttempts--,
                s => s.offers[0].shopAgeSeconds = 0, s => s.offers[0].shopAgeSeconds = double.NaN,
                s => s.offers[0].sourceKey = "other", s => s.offers[1] = s.offers[0],
                s => s.source.controlsPassed = false, s => s.source.cacheSha256 = new string('a', 64),
                s => s.openingPairs[0].bought = true })
            {
                var stock = Load(); corrupt(stock);
                Assert.Throws<ArgumentException>(() => stock.BuildIndexes(Items()));
            }
        }
        [Test] public void SourceMismatchAndForeignOfferDoNotReuseAnotherShopStock()
        {
            var stock = Load(); var items = Items(); items.shops[0].offerIds[0] = "I000";
            Assert.Throws<ArgumentException>(() => stock.BuildIndexes(items));
            items = Items(); items.mapSha256 = new string('0', 64);
            Assert.Throws<ArgumentException>(() => stock.BuildIndexes(items));
            Assert.Throws<InvalidOperationException>(() => stock.ReadyCount("n004", "I04J"));
        }
        [Test] public void PublishedIndexAndReturnedEvidenceAreDetachedAndFailedRebuildIsAtomic()
        {
            var stock = Load(); var items = Items(); stock.BuildIndexes(items);
            var original = stock.Offer("n004", "I04J"); original.readyCount = 99;
            stock.offers[0].readyCount = 99;
            Assert.Throws<ArgumentException>(() => stock.BuildIndexes(items));
            Assert.That(stock.ReadyCount("n004", "I04J"), Is.EqualTo(1));
            Assert.That(stock.Offer("n004", "I04J").readyCount, Is.EqualTo(1));
        }
        [Test] public void FreshAcolyteUsesItsEarlyObservationWithoutRewritingBulkAge()
        {
            var stock = Load(); stock.BuildIndexes(Items());
            foreach (string id in new[] { "I07W", "I0AI", "I05F", "I0AT", "I0B8" })
            {
                Assert.That(stock.ReadyAgeSeconds("u00E", id), Is.EqualTo(2.2).Within(.001));
                Assert.That(stock.Offer("u00E", id).shopAgeSeconds, Is.GreaterThan(10));
            }
            stock.acolyteReadiness.observations[0].shopAgeSeconds = 0;
            Assert.Throws<ArgumentException>(() => stock.BuildIndexes(Items()));
            Assert.That(stock.ReadyAgeSeconds("u00E", "I07W"), Is.EqualTo(2.2).Within(.001));
        }
        [Test] public void FreshReadinessRequiresEveryLateControlAndMatchingProvenance()
        {
            foreach (Action<OriginalObservedShopStock> corrupt in new Action<OriginalObservedShopStock>[] {
                s => s.acolyteReadiness.source.cacheSha256 = new string('0', 64),
                s => s.acolyteReadiness.observations[10].known = false,
                s => s.acolyteReadiness.observations[1].itemId = "I000",
                s => s.acolyteReadiness.observations[2].readyCount = 2 })
            {
                var stock = Load(); corrupt(stock);
                Assert.Throws<ArgumentException>(() => stock.BuildIndexes(Items()));
            }
        }
    }
}

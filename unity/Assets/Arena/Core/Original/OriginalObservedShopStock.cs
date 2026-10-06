using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedShopStockSource
    {
        public string cacheName, cacheSha256, probeMapSha256, probeScriptSha256, capturedUtc;
        public bool complete, controlsPassed;
        public int records, passed, failed;
    }
    [Serializable] public sealed class OriginalObservedShopStockOffer
    {
        public string shopId, itemId, sourceKey, state;
        public bool known;
        public int readyCount, declaredMaximum, exhaustedAfterAttempts;
        public double shopAgeSeconds, orderTimeSeconds;
        internal OriginalObservedShopStockOffer Copy() => (OriginalObservedShopStockOffer)MemberwiseClone();
    }
    [Serializable] public sealed class OriginalObservedShopStockOpening
    {
        public string sourceKey;
        public bool forcedStock, bought;
        public double shopAgeSeconds;
    }
    [Serializable] public sealed class OriginalObservedAcolyteStockRow
    {
        public string sourceKey, shopId, itemId;
        public bool known;
        public int readyCount, declaredMaximum, goldDebit, lumberDebit;
        public double shopAgeSeconds, orderTimeSeconds;
    }
    [Serializable] public sealed class OriginalObservedAcolyteStock
    {
        public OriginalObservedShopStockSource source;
        public OriginalObservedAcolyteStockRow[] observations;
        public string[] limits;
    }

    // Exact inventory after native shop readiness. This is deliberately not
    // an override for a missing ItemData.stockStart value, nor a refill rule.
    [Serializable] public sealed class OriginalObservedShopStock
    {
        public const string ExpectedMapSha256 = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        public const string ExpectedCacheSha256 = "f9ec0990900ca78c4cd852fcd31d2f6c1720a0b0de1d9b3442bc5290333b3970";
        public const string ExpectedProbeMapSha256 = "881c0cd692e8fad3d211612dc57017682ee09d8e4816024bbae3f3386b1decc0";
        public const string ExpectedProbeScriptSha256 = "68103fc5a14dcd4f427a52fc91d7b089324d4ba32ffba7515e765d02c155f869";
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public bool runtimeObserved;
        public OriginalObservedShopStockSource source;
        public OriginalObservedShopStockOffer[] offers;
        public OriginalObservedShopStockOpening[] openingPairs;
        public OriginalObservedAcolyteStock acolyteReadiness;
        public string[] limits;
        [NonSerialized] Dictionary<string, OriginalObservedShopStockOffer> index;
        [NonSerialized] Dictionary<string, double> readyAges;
        public int OfferCount => index == null ? 0 : index.Count;

        public void BuildIndexes(OriginalItemCatalog catalog)
        {
            Check(schemaVersion == 1 && mapSha256 == ExpectedMapSha256 && engineVersion == "1.26.0.6401" && runtimeObserved,
                "Unexpected observed shop-stock identity.");
            Check(source != null && source.cacheName == "LiAStock1.w3v" && source.cacheSha256 == ExpectedCacheSha256 &&
                source.probeMapSha256 == ExpectedProbeMapSha256 && source.probeScriptSha256 == ExpectedProbeScriptSha256 &&
                DateTimeOffset.TryParse(source.capturedUtc, out _) && source.complete && source.controlsPassed &&
                source.records == 293 && source.passed == 293 && source.failed == 0, "Incomplete or unrecognized stock provenance.");
            Check(catalog != null && catalog.mapSha256 == ExpectedMapSha256 && catalog.items != null && catalog.shops != null,
                "Stock registry requires the matching original item catalog.");
            var items = new Dictionary<string, OriginalItemDefinition>(StringComparer.Ordinal);
            foreach (var item in catalog.items)
            {
                Check(item != null && Rawcode(item.id) && !items.ContainsKey(item.id), "Duplicate or invalid source item.");
                items.Add(item.id, item);
            }
            var expected = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var shop in catalog.shops)
            {
                Check(shop != null && Rawcode(shop.unitId) && shop.offerIds != null, "Invalid source shop.");
                foreach (string itemId in shop.offerIds)
                {
                    Check(itemId != null && items.TryGetValue(itemId, out var unused), "Source offer has no item.");
                    var item = items[itemId]; var key = Key(shop.unitId, itemId);
                    Check(item.stockMax != null && item.stockMax.known && item.stockMax.value >= 1 && item.stockMax.value <= 6 &&
                        !expected.ContainsKey(key), "Unresolved source maximum or duplicate source offer.");
                    expected.Add(key, item.stockMax.value);
                }
            }
            Check(expected.Count == 275 && offers != null && offers.Length == 275, "Expected all 275 original offers.");
            var next = new Dictionary<string, OriginalObservedShopStockOffer>(StringComparer.Ordinal);
            foreach (var offer in offers)
            {
                Check(offer != null && Rawcode(offer.shopId) && Rawcode(offer.itemId), "Invalid observed shop/offer.");
                var key = Key(offer.shopId, offer.itemId);
                Check(expected.TryGetValue(key, out var maximum) && !next.ContainsKey(key), "Duplicate or foreign observed offer.");
                Check(offer.known && offer.state == "runtime-ready-stock" && offer.sourceKey == "stock_" + offer.shopId + "_" + offer.itemId &&
                    offer.readyCount == maximum && offer.declaredMaximum == maximum && offer.exhaustedAfterAttempts == maximum + 1,
                    "Measured stock must sell the source maximum and reject the next synchronous attempt.");
                Check(Finite(offer.shopAgeSeconds) && offer.shopAgeSeconds >= 2.5 && Finite(offer.orderTimeSeconds) &&
                    offer.orderTimeSeconds >= offer.shopAgeSeconds, "Stock measurement predates readiness.");
                next.Add(key, offer.Copy());
            }
            Check(openingPairs != null && openingPairs.Length == 18, "Missing opening readiness controls.");
            int[] waits = { 0, 3, 13, 28, 53, 83, 98, 108, 123 };
            for (int i = 0; i < openingPairs.Length; i++)
            {
                var opening = openingPairs[i]; bool forced = i % 2 == 1;
                double expectedAge = (waits[i / 2] + 2) * .02;
                Check(opening != null && opening.sourceKey == "early_" + waits[i / 2] + "_" + (forced ? "1" : "0") &&
                    opening.forcedStock == forced && opening.bought == (i >= 14) && Finite(opening.shopAgeSeconds) &&
                    Math.Abs(opening.shopAgeSeconds - expectedAge) < .001, "Opening control differs from captured native observations.");
            }
            var ages = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var pair in next) ages.Add(pair.Key, pair.Value.shopAgeSeconds);
            if (acolyteReadiness != null)
            {
                var proof = acolyteReadiness.source;
                Check(proof != null && proof.cacheName == "LiAAcol1.w3v" &&
                    proof.cacheSha256 == "6202a00d4c5eae136896a849d62f696d484d5b52713d358d9fdb304c6c9cc304" &&
                    proof.probeMapSha256 == "40192a454fe633b6263fbe271485d0fbf5bdadb3281128c59534be858413b57b" &&
                    proof.probeScriptSha256 == "c789fbef5e3acda2e880842c2bebcd6aca9678fade59f065072f5f41e79ca2f9" &&
                    DateTimeOffset.TryParse(proof.capturedUtc, out _) && proof.complete && proof.controlsPassed &&
                    proof.records == 15 && proof.passed == 15 && proof.failed == 0, "Unrecognized fresh acolyte evidence.");
                Check(acolyteReadiness.observations != null && acolyteReadiness.observations.Length == 15, "Missing fresh acolyte controls.");
                string[] ids = { "I07W", "I0AI", "I05F", "I0AT", "I0B8" };
                string[] labels = { "early22", "early30", "late122" };
                double[] observedAges = { 2.2, 3, 12.2 };
                int[] gold = { 0, 50, 70, 1200, 2 }, lumber = { 6, 0, 9, 19, 1 };
                double previousOrder = -1;
                for (int i = 0; i < 15; i++)
                {
                    var row = acolyteReadiness.observations[i]; int item = i % 5, age = i / 5;
                    string key = Key("u00E", ids[item]);
                    Check(row != null && row.known && row.shopId == "u00E" && row.itemId == ids[item] &&
                        row.sourceKey == "acolyte_" + ids[item] + "_" + labels[age] &&
                        expected.TryGetValue(key, out int maximum) && maximum == 1 && row.readyCount == 1 && row.declaredMaximum == 1 &&
                        row.goldDebit == gold[item] && row.lumberDebit == lumber[item] &&
                        Finite(row.shopAgeSeconds) && Math.Abs(row.shopAgeSeconds - observedAges[age]) < .001 &&
                        Finite(row.orderTimeSeconds) && row.orderTimeSeconds - row.shopAgeSeconds > previousOrder,
                        "Fresh acolyte sale or late control differs from the captured observation.");
                    previousOrder = row.orderTimeSeconds;
                    ages[key] = Math.Min(ages[key], row.shopAgeSeconds);
                }
            }
            index = next; readyAges = ages;
        }

        // Caller chooses the source-equivalent ready-shop phase. These methods
        // do not claim that the inventory was available at unit creation.
        public bool TryGetReadyCount(string shopId, string itemId, out int count)
        {
            if (index == null) throw new InvalidOperationException("BuildIndexes requires the source item catalog first.");
            count = 0;
            if (!Rawcode(shopId) || !Rawcode(itemId) || !index.TryGetValue(Key(shopId, itemId), out var offer)) return false;
            count = offer.readyCount; return true;
        }
        public int ReadyCount(string shopId, string itemId)
        {
            if (!TryGetReadyCount(shopId, itemId, out var count)) throw new InvalidOperationException("Unobserved ready-stock: " + shopId + "/" + itemId);
            return count;
        }
        public OriginalObservedShopStockOffer Offer(string shopId, string itemId)
        {
            ReadyCount(shopId, itemId);
            return index[Key(shopId, itemId)].Copy();
        }
        // Earliest measured success, not an inferred stockStart/default or an
        // exact first-available instant. The raw bulk evidence stays unchanged.
        public double ReadyAgeSeconds(string shopId, string itemId)
        {
            ReadyCount(shopId, itemId);
            return readyAges[Key(shopId, itemId)];
        }
        static string Key(string shopId, string itemId) => shopId + ":" + itemId;
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static bool Rawcode(string value)
        {
            if (value == null || value.Length != 4) return false;
            foreach (char c in value) if (c < 32 || c > 126) return false;
            return true;
        }
        static void Check(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
    }
}

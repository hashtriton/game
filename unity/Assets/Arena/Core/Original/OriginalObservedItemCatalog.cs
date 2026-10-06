using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedItemSource
    {
        public string cacheName, cacheSha256, probeMapSha256, probeScriptSha256, capturedUtc;
        public bool complete, controlsPassed;
        public int records, passed, failed, directKnown, priceObserved, nonSellableSkipped;
    }
    [Serializable] public sealed class OriginalObservedItem
    {
        public string id, sourceKey, error, priceState;
        public bool directKnown, recordPassed, purchaseAttempted, priceObserved, priceKnown;
        public bool powerup, sellable, pawnable;
        public int initialCharges, nativeLevel, goldDebit, lumberDebit;
    }
    [Serializable] public sealed class OriginalObservedItemPurchaseBatch
    {
        public OriginalObservedItemSource source;
        public OriginalObservedItem[] items;
    }

    // Exact native getter and controlled purchase observations. This does not
    // infer stock timing, pickup/use effects, pawn value or original triggers.
    [Serializable] public sealed class OriginalObservedItemCatalog
    {
        public const string ExpectedProbeMapSha256 = "7dc22256453548a47cb150cbf4e66ac90cb9c2fc6a5c86eca6271402f032660b";
        public const string ExpectedProbeScriptSha256 = "3ded85d02ce855db766a6c8ed40069ece3cd8255e649251a510eaa826198e24c";
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public bool runtimeObserved;
        public OriginalObservedItemSource source;
        public OriginalObservedItem[] items;
        public OriginalObservedShopStock readyStock;
        public OriginalObservedItemUse itemUse;
        public OriginalObservedItemActives itemActives;
        public OriginalObservedItemPurchaseBatch skippedPurchases;
        public string[] limits;
        [NonSerialized] Dictionary<string, Dictionary<string, OriginalItemRuleValue>> index;

        public void BuildIndexes(OriginalItemCatalog itemCatalog = null)
        {
            Check(schemaVersion == 1 && mapSha256 == OriginalNativeCatalog.ExpectedMapSha256 &&
                engineVersion == "1.26.0.6401" && runtimeObserved, "Wrong observed item catalog identity");
            Check(source != null && source.cacheName == "LiAItemsB1.w3v" && Hash(source.cacheSha256) &&
                source.probeMapSha256 == ExpectedProbeMapSha256 && source.probeScriptSha256 == ExpectedProbeScriptSha256 &&
                !string.IsNullOrEmpty(source.capturedUtc) && source.complete && source.controlsPassed && source.records == 448 &&
                source.passed >= 8 && source.failed >= 0 && source.passed + source.failed == 448, "Incomplete item measurement/control source");
            Check(items != null && items.Length == 440, "Observed item coverage must retain all 440 declarations");
            var next = new Dictionary<string, Dictionary<string, OriginalItemRuleValue>>(StringComparer.Ordinal);
            int passed = 8, direct = 8, observed = 4, skipped = 0;
            foreach (var item in items)
            {
                Check(item != null && Rawcode(item.id) && item.sourceKey == "item_" + item.id &&
                    !next.ContainsKey(item.id), "Invalid or duplicate observed item identity");
                Check(item.initialCharges >= 0 && item.nativeLevel >= 0 && item.goldDebit >= 0 && item.lumberDebit >= 0,
                    "Negative native item measurement");
                if (item.directKnown) direct++;
                else Check(!item.powerup && !item.sellable && !item.pawnable && item.initialCharges == 0 && item.nativeLevel == 0 &&
                    !item.purchaseAttempted && !item.priceObserved && !item.recordPassed &&
                    item.priceState == "unresolved-item-creation", "Failed creation contains known measurements");
                if (item.recordPassed) { passed++; Check(string.IsNullOrEmpty(item.error), "Successful row contains error"); }
                else Check(!string.IsNullOrEmpty(item.error), "Failed row has no diagnostic");
                if (item.priceObserved)
                {
                    observed++;
                    Check(item.directKnown && item.sellable && item.purchaseAttempted && item.recordPassed,
                        "Price lacks successful native sale");
                    bool excluded = item.id == "gold" || item.id == "lmbr";
                    Check(item.priceKnown == !excluded && item.priceState == (excluded ?
                        "unresolved-resource-changing-powerup" : "observed-stable-debit"), "Net currency debit is not always a price");
                }
                else
                {
                    Check(!item.priceKnown && item.goldDebit == 0 && item.lumberDebit == 0, "Unknown price has an inferred value");
                    if (item.directKnown && !item.sellable)
                    {
                        skipped++; Check(!item.purchaseAttempted && item.recordPassed && item.priceState == "native-nonsellable",
                            "Non-sellable native item must not imply a free purchase");
                    }
                    else if (item.directKnown)
                        Check(item.purchaseAttempted && !item.recordPassed && item.priceState == "unresolved-purchase-failed",
                            "Missing purchase failure state");
                }
                var row = new Dictionary<string, OriginalItemRuleValue>(StringComparer.Ordinal);
                string origin = "native1.26:" + source.cacheName + "/" + item.sourceKey + ":cacheSha256=" + source.cacheSha256 +
                    ":scriptSha256=" + source.probeScriptSha256;
                Add(row, "uses", item.directKnown, item.initialCharges, item.directKnown ? "runtime-native-getter" : "unresolved-item-creation", origin);
                Add(row, "powerup", item.directKnown, item.powerup ? 1 : 0, item.directKnown ? "runtime-native-getter" : "unresolved-item-creation", origin);
                Add(row, "sellable", item.directKnown, item.sellable ? 1 : 0, item.directKnown ? "runtime-native-getter" : "unresolved-item-creation", origin);
                Add(row, "pawnable", item.directKnown, item.pawnable ? 1 : 0, item.directKnown ? "runtime-native-getter" : "unresolved-item-creation", origin);
                Add(row, "goldcost", item.priceKnown, item.priceKnown ? item.goldDebit : 0, item.priceState, origin);
                Add(row, "lumbercost", item.priceKnown, item.priceKnown ? item.lumberDebit : 0, item.priceState, origin);
                next.Add(item.id, row);
            }
            Check(source.passed == passed && source.failed == 448 - passed && source.directKnown == direct &&
                source.priceObserved == observed && source.nonSellableSkipped == skipped, "Native item summary disagrees with rows");
            if (skippedPurchases != null) ApplySkippedPurchases(next);
            // Ready stock is a separate measured phase baseline. Resolving
            // item fields alone must not invent ItemData.stockStart defaults.
            if (readyStock != null && itemCatalog != null) readyStock.BuildIndexes(itemCatalog);
            index = next;
        }

        void ApplySkippedPurchases(Dictionary<string, Dictionary<string, OriginalItemRuleValue>> next)
        {
            var batch = skippedPurchases; var origin = batch.source;
            Check(origin != null && origin.cacheName == "LiAItemsS1.w3v" && Hash(origin.cacheSha256) &&
                origin.probeMapSha256 == "a6984b87db6f3db752f162544d1ebf5eef608cf0740a67ea489e9a4dfb839554" &&
                origin.probeScriptSha256 == "153f9889f4348c22aa814454d6b9eda8e1324f0a7a00e7d5b83d5fd14f9d5780" &&
                !string.IsNullOrEmpty(origin.capturedUtc) && origin.complete && origin.controlsPassed && origin.records == 57 &&
                origin.nonSellableSkipped == 0, "Unrecognized skipped purchase controls");
            var expected = new Dictionary<string, OriginalObservedItem>(StringComparer.Ordinal);
            foreach (var item in items) if (item.directKnown && !item.sellable) expected.Add(item.id, item);
            Check(expected.Count == 49 && batch.items != null && batch.items.Length == 49, "Skipped purchase coverage changed");
            int passed = 8, direct = 8, observed = 4;
            foreach (var item in batch.items)
            {
                Check(item != null && item.id != null && expected.TryGetValue(item.id, out var prior) &&
                    item.sourceKey == "item_" + item.id, "Unexpected skipped purchase identity");
                prior = expected[item.id]; expected.Remove(item.id);
                Check(item.directKnown && item.initialCharges == prior.initialCharges && item.nativeLevel == prior.nativeLevel &&
                    item.powerup == prior.powerup && item.sellable == prior.sellable && item.pawnable == prior.pawnable && item.purchaseAttempted,
                    "Repeated native getter disagrees with original item measurement");
                direct++;
                Check(item.recordPassed == string.IsNullOrEmpty(item.error), "Skipped purchase error mismatch");
                if (item.recordPassed) passed++;
                if (item.priceObserved)
                {
                    observed++;
                    bool excluded = item.id == "gold" || item.id == "lmbr";
                    Check(item.recordPassed && item.goldDebit >= 0 && item.lumberDebit >= 0 && item.priceKnown == !excluded &&
                        item.priceState == (excluded ? "unresolved-resource-changing-powerup" : "observed-stable-debit"), "Invalid skipped purchase result");
                }
                else Check(!item.recordPassed && !item.priceKnown && item.goldDebit == 0 && item.lumberDebit == 0 &&
                    item.priceState == "unresolved-purchase-failed", "Failed skipped purchase contains an inferred price");
                if (!item.priceKnown) continue;
                string evidence = "native1.26:" + origin.cacheName + "/" + item.sourceKey + ":cacheSha256=" + origin.cacheSha256 +
                    ":scriptSha256=" + origin.probeScriptSha256;
                next[item.id]["goldcost"] = new OriginalItemRuleValue { field = "goldcost", known = true, value = item.goldDebit,
                    state = item.priceState, sources = new[] { evidence } };
                next[item.id]["lumbercost"] = new OriginalItemRuleValue { field = "lumbercost", known = true, value = item.lumberDebit,
                    state = item.priceState, sources = new[] { evidence } };
            }
            Check(expected.Count == 0 && origin.passed == passed && origin.failed == 57 - passed && origin.directKnown == direct &&
                origin.priceObserved == observed, "Skipped purchase counters disagree");
        }

        // Unsupported fields were not measured by this probe. Returning an
        // explicit gap permits retaining a separately verified declaration.
        public OriginalItemRuleValue Field(string itemId, string field)
        {
            if (index == null) BuildIndexes();
            if (itemId == null || !index.TryGetValue(itemId, out var row)) throw new ArgumentException("Unobserved item identity: " + itemId);
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (!row.TryGetValue(field, out var value)) return new OriginalItemRuleValue
                { field = field, state = "unresolved-not-measured-by-item-probe" };
            return new OriginalItemRuleValue { field = value.field, known = value.known, value = value.value,
                state = value.state, sources = (string[])value.sources.Clone() };
        }

        static void Add(Dictionary<string, OriginalItemRuleValue> row, string field, bool known, int value, string state, string source) =>
            row.Add(field, new OriginalItemRuleValue { field = field, known = known, value = value, state = state, sources = new[] { source } });
        static bool Rawcode(string value)
        { if (value == null || value.Length != 4) return false; foreach (char c in value) if (c < 32 || c > 126) return false; return true; }
        static bool Hash(string value)
        { if (value == null || value.Length != 64) return false; foreach (char c in value) if (!(c >= '0' && c <= '9' || c >= 'a' && c <= 'f')) return false; return true; }
        static void Check(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
    }
}

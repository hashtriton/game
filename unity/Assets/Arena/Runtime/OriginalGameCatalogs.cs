using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    // Scene/build references are TextAssets. File IO and AssetDatabase are not
    // available to a player build and must not be used to load match rules.
    public sealed class OriginalGameCatalogs
    {
        public const string RulesVersion = "lia39-unity-rules-10";
        public const string MapSha256 = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        public OriginalMatchCatalog Match { get; private set; }
        public OriginalItemCatalog Items { get; private set; }
        public OriginalCombatCatalog Combat { get; private set; }
        public OriginalItemPassiveCatalog Passives { get; private set; }
        public OriginalDuelCatalog Duels { get; private set; }
        public OriginalNativeCatalog Native { get; private set; }
        public OriginalObservedCatalog Observed { get; private set; }
        public OriginalObservedItemCatalog ObservedItems { get; private set; }
        public string Fingerprint { get; private set; }
        private string matchJson, itemsJson, combatJson, passivesJson, duelJson, nativeJson, observedJson, observedItemsJson;

        public static OriginalGameCatalogs Load(IEnumerable<TextAsset> assets)
        {
            if (assets == null) throw new ArgumentNullException(nameof(assets));
            var byName = new Dictionary<string, TextAsset>(StringComparer.Ordinal);
            var parts = new List<OriginalContentPart>();
            foreach (var asset in assets)
            {
                if (!asset) throw new ArgumentException("Missing original game data asset.");
                if (byName.ContainsKey(asset.name)) throw new ArgumentException("Duplicate data asset " + asset.name);
                byName.Add(asset.name, asset);
                parts.Add(new OriginalContentPart(asset.name + ".json", asset.bytes));
            }
            TextAsset Required(string name)
            {
                if (!byName.TryGetValue(name, out var asset)) throw new ArgumentException("Missing data asset " + name);
                return asset;
            }
            var result = new OriginalGameCatalogs
            {
                matchJson = Required("lia39-match").text,
                itemsJson = Required("lia39-items").text,
                combatJson = Required("lia39-combat").text,
                passivesJson = Required("lia39-item-passives").text,
                duelJson = Required("lia39-duels").text,
                nativeJson = Required("lia39-native126").text,
                observedJson = Required("lia39-observed126").text,
                observedItemsJson = Required("lia39-observed-items126").text
            };
            result.Match = JsonUtility.FromJson<OriginalMatchCatalog>(result.matchJson);
            result.Items = JsonUtility.FromJson<OriginalItemCatalog>(result.itemsJson);
            result.Combat = JsonUtility.FromJson<OriginalCombatCatalog>(result.combatJson);
            result.Passives = JsonUtility.FromJson<OriginalItemPassiveCatalog>(result.passivesJson);
            result.Duels = JsonUtility.FromJson<OriginalDuelCatalog>(result.duelJson);
            result.Native = JsonUtility.FromJson<OriginalNativeCatalog>(result.nativeJson);
            if (result.Native == null) throw new ArgumentException("Missing native mechanical data.");
            result.Native.BuildIndexes();
            result.Observed = JsonUtility.FromJson<OriginalObservedCatalog>(result.observedJson);
            if (result.Observed == null) throw new ArgumentException("Missing measured native rules.");
            result.Observed.BuildIndexes();
            result.ObservedItems = JsonUtility.FromJson<OriginalObservedItemCatalog>(result.observedItemsJson);
            if (result.ObservedItems == null) throw new ArgumentException("Missing measured native item rules.");
            result.ObservedItems.BuildIndexes();
            var layout = JsonUtility.FromJson<ArenaMapLayout>(Required("lia39-layout").text);
            if (result.Match == null || result.Items == null || result.Combat == null || result.Passives == null || result.Duels == null ||
                layout == null || result.Match.schemaVersion != 1 || result.Items.schemaVersion != 1 ||
                result.Combat.schemaVersion != 1 || result.Passives.schemaVersion != 1 || layout.schemaVersion != 1 ||
                result.Match.sourceSha256 != MapSha256 || result.Items.mapSha256 != MapSha256 ||
                result.Combat.sourceSha256 != MapSha256 || result.Passives.mapSha256 != MapSha256 ||
                layout.provenance?.mapSha256 != MapSha256 || layout.version != "3.9c" || layout.wcUnitsPerUnityUnit != 64)
                throw new ArgumentException("Incompatible original game data version or source map.");
            ArenaMap.Validate(layout);
            result.Match.Validate();
            result.Duels.Validate();
            result.Items.BuildIndexes();
            result.ObservedItems.BuildIndexes(result.Items);
            result.Combat.BuildIndexes();
            result.Fingerprint = OriginalContentFingerprint.Compute(RulesVersion, parts);
            return result;
        }

        // Published DTOs support presentation and inspection. The authority
        // gets private copies from the exact validated source strings, so a UI
        // consumer cannot change match rules while retaining the same hash.
        public OriginalSession CreateSession(OriginalMatchOptions options, int seed, OriginalMapNavigation navigation = null)
        {
            var result = new OriginalSession(JsonUtility.FromJson<OriginalMatchCatalog>(matchJson),
                JsonUtility.FromJson<OriginalItemCatalog>(itemsJson),
                JsonUtility.FromJson<OriginalCombatCatalog>(combatJson),
                JsonUtility.FromJson<OriginalDuelCatalog>(duelJson), Fingerprint, options, seed);
            var native = JsonUtility.FromJson<OriginalNativeCatalog>(nativeJson); native.BuildIndexes();
            var observed = JsonUtility.FromJson<OriginalObservedCatalog>(observedJson);
            result.ConfigureProgression(native, observed);
            result.ConfigureBounty(observed.bounty);
            result.ConfigureItems(native, JsonUtility.FromJson<OriginalObservedItemCatalog>(observedItemsJson),
                JsonUtility.FromJson<OriginalItemPassiveCatalog>(passivesJson));
            if (navigation != null)
            {
                result.ConfigureWorld(navigation, native, navigation.Destructables(native), observed);
            }
            return result;
        }
    }
}

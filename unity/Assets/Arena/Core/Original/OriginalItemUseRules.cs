using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedConsumable
    {
        public string itemId, abilityId;
        public int abilityRank, initialCharges;
        public double amount;
        public bool manaCostZero, fullUseRejected, sharedCooldownRejected, cooldownRetryAccepted, lastChargeRemoved;
    }
    [Serializable] public sealed class OriginalObservedItemUse
    {
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public OriginalObservedItemSource source;
        public OriginalObservedConsumable[] items;
    }
    public enum OriginalItemUseCode { Ready, Unsupported, RuleUnavailable, NoCharges, FullResource, Cooldown, Dead, Paused, NotReady }
    [Serializable] public sealed class OriginalItemUseView
    {
        public OriginalInventoryBag bag;
        public int slot;
        public long instanceId;
        public string itemId;
        public bool implemented, requiresCharge;
        public OriginalItemUseCode code;
        public OriginalAbilityTargetMode targetMode;
        public double cooldownRemaining;
    }
    public sealed class OriginalConsumableRule
    {
        public string itemId, abilityId, cooldownGroup, resource;
        public bool known;
        public double amount, cooldown;
        internal OriginalConsumableRule Copy() => (OriginalConsumableRule)MemberwiseClone();
    }

    // Native potion amounts/cooldowns are declarations. Missing mana cost,
    // full-resource refusal, charge retirement and shared cooldown semantics
    // require the independent UnitUseItem control observations.
    public sealed class OriginalItemUseRules
    {
        const string ProbeMap = "d8049903467e9eb748e40e3cb06a5d7fbf5ab77a2e70ea314a555deeb5d31f62";
        const string ProbeScript = "156ef327cf0943dbb5248f52baab62ee87ebfa4d201885253fe298f5a03e9a2a";
        readonly Dictionary<string, OriginalConsumableRule> rules = new Dictionary<string, OriginalConsumableRule>(StringComparer.Ordinal);
        public OriginalItemUseRules(OriginalItemCatalog items, OriginalCombatCatalog combat, OriginalObservedItemUse observed = null)
        {
            if (items == null || combat == null || items.mapSha256 != OriginalNativeCatalog.ExpectedMapSha256 ||
                combat.sourceSha256 != items.mapSha256) throw new ArgumentException("Consumable catalogs describe different source maps.");
            string[] ids = { "I03L", "I03M", "I022", "I023" }, abilities = { "A0B7", "A0B8", "AIh2", "AIm2" };
            for (int i = 0; i < ids.Length; i++)
            {
                var item = items.Item(ids[i]); var ability = combat.Ability(abilities[i]);
                if (item == null || ability == null || Array.IndexOf(item.abilityIds, abilities[i]) < 0 || item.cooldownId != abilities[i])
                    throw new ArgumentException("Consumable identity differs from the source.");
                var rule = new OriginalConsumableRule { itemId = ids[i], abilityId = abilities[i], cooldownGroup = item.cooldownId,
                    resource = i % 2 == 0 ? "health" : "mana" };
                if (ability.Text("code") != (i % 2 == 0 ? "AIhe" : "AIma") ||
                    !ability.TryNumber("DataA1", out rule.amount, out _) || rule.amount <= 0 ||
                    !ability.TryNumber("Cool1", out rule.cooldown, out _) || rule.cooldown <= 0)
                    throw new ArgumentException("Unresolved consumable amount or cooldown.");
                rules.Add(rule.itemId, rule);
            }
            if (observed == null) return;
            var source = observed.source;
            if (observed.schemaVersion != 1 || observed.mapSha256 != items.mapSha256 || observed.engineVersion != "1.26.0.6401" ||
                source == null || source.cacheName != "LiAItemUse1.w3v" || !Hash(source.cacheSha256) ||
                source.probeMapSha256 != ProbeMap || source.probeScriptSha256 != ProbeScript ||
                !source.complete || !source.controlsPassed || source.records != 12 || source.passed != 12 || source.failed != 0 ||
                observed.items == null || observed.items.Length != 4)
                throw new ArgumentException("Incomplete consumable observations.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in observed.items)
            {
                if (row == null || row.itemId == null || !seen.Add(row.itemId) || !rules.TryGetValue(row.itemId, out var rule) ||
                    row.abilityId != rule.abilityId || row.abilityRank != 1 || row.initialCharges != 1 || row.amount != rule.amount ||
                    !row.manaCostZero || !row.fullUseRejected || !row.sharedCooldownRejected || !row.cooldownRetryAccepted || !row.lastChargeRemoved)
                    throw new ArgumentException("Consumable measurements do not close its native use rules.");
                rule.known = true;
            }
        }
        public OriginalConsumableRule Rule(string itemId) => itemId != null && rules.TryGetValue(itemId, out var rule) ? rule.Copy() : null;
        static bool Hash(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (char c in value) if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) return false;
            return true;
        }
    }
}

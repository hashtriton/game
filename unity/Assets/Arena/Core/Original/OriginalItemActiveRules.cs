using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedItemArmor
    {
        public bool known, radiusMeasured;
        public string buffId;
        public double armorAdded, observedAliveAt, observedGoneAt;
    }
    [Serializable] public sealed class OriginalObservedItemActive
    {
        public string itemId, abilityId;
        public int mode;
        public double effectSeconds, manaCost;
        public bool retired, orderAccepted, nativeUseObserved;
        public OriginalObservedItemArmor armor;
        public OriginalObservedItemSummon[] summons;
    }
    [Serializable] public sealed class OriginalObservedSummonProfile
    {
        public double hp, maxhp, mp, maxmp, speed;
        public int owner, dead, paused, BFig;
    }
    [Serializable] public sealed class OriginalObservedSummonLifetime
    { public double declaredSeconds, lastAliveSeconds; public bool expiryObserved; }
    [Serializable] public sealed class OriginalObservedItemSummon
    { public string unitId; public OriginalObservedSummonProfile profile; public OriginalObservedSummonLifetime lifetime; }
    public sealed class OriginalItemSummonRule
    {
        public string itemId, abilityId, cooldownGroup, limitation;
        public bool known, retired;
        public double manaCost, cooldown, duration;
        public bool pointTarget;
        public double castRange;
        public string[] units;
        public OriginalWorldUnitProfile[] profiles;
        internal OriginalItemSummonRule Copy()
        {
            var copy = (OriginalItemSummonRule)MemberwiseClone(); copy.units = (string[])units.Clone();
            copy.profiles = Array.ConvertAll(profiles, x => x.Copy()); return copy;
        }
    }
    [Serializable] public sealed class OriginalObservedItemActives
    {
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public OriginalObservedItemSource source;
        public OriginalObservedItemActive[] items;
        public OriginalObservedItemWards wardObservations;
    }
    public sealed class OriginalItemArmorRule
    {
        public string itemId, abilityId, cooldownGroup, buffId;
        public double armor, duration, radius, cooldown, manaCost, restoration;
        public bool known, retired;
        internal OriginalItemArmorRule Copy() => (OriginalItemArmorRule)MemberwiseClone();
    }

    // ITEMACT2 measures successful native activation and the armor delta.
    // Radius600/duration6/cooldown12 remain source declarations. Recipient
    // edge geometry, refresh/stacking and item control-state rules are separate.
    public sealed partial class OriginalItemActiveRules
    {
        readonly Dictionary<string, OriginalItemArmorRule> armors = new Dictionary<string, OriginalItemArmorRule>(StringComparer.Ordinal);
        readonly Dictionary<string, OriginalItemSummonRule> summons = new Dictionary<string, OriginalItemSummonRule>(StringComparer.Ordinal);
        public OriginalItemActiveRules(OriginalItemCatalog items, OriginalCombatCatalog combat, OriginalObservedItemActives observed)
        {
            if (items == null || combat == null || items.mapSha256 != OriginalNativeCatalog.ExpectedMapSha256 ||
                combat.sourceSha256 != items.mapSha256) throw new ArgumentException("Active item catalog identity differs.");
            if (observed == null) return;
            var source = observed.source;
            Check(observed.schemaVersion == 1 && observed.mapSha256 == items.mapSha256 && observed.engineVersion == "1.26.0.6401" &&
                source != null && source.cacheName == "LiAItemAct2.w3v" &&
                source.cacheSha256 == "34c14e51e3a0b66fc03688f3d706a48397e93867e65cb595c9a0c7c6d6af3029" &&
                source.probeMapSha256 == "666d13b17e7c62402ceab69b711376da6a0c7a5d5d057ebefadd1564facf58b1" &&
                source.probeScriptSha256 == "b7d724d33c7fee8370456d04f84643ee78a97fc6bea06c6a46a2a361c22c24eb" &&
                source.complete && source.records == 12 && source.passed == 12 && source.failed == 0 &&
                observed.items != null && observed.items.Length == 12, "Incomplete active item observations.");
            var expected = new HashSet<string>(new[] { "I01A", "I01D", "I01J", "I01M", "I02G", "I07E", "I07K", "I0AE", "I02H", "I021", "I094", "I0B7" }, StringComparer.Ordinal);
            foreach (var row in observed.items)
            {
                Check(row != null && expected.Remove(row.itemId), "Duplicate or unknown active item row.");
                BuildSummon(items, combat, row);
                if (row.itemId != "I02H") continue;
                var item = items.Item(row.itemId); var ability = combat.Ability(row.abilityId);
                Check(row.abilityId == "A0A0" && row.mode == 1 && row.nativeUseObserved && row.retired && row.orderAccepted &&
                    row.effectSeconds == 0 && row.manaCost == 0 && row.armor != null && row.armor.known &&
                    row.armor.armorAdded == 100 && row.armor.buffId == "Bdef" && !row.armor.radiusMeasured &&
                    row.armor.observedAliveAt > 0 && row.armor.observedAliveAt < 6 && row.armor.observedGoneAt > 6 &&
                    row.armor.observedGoneAt < 8, "Unresolved armor item lifecycle.");
                Check(item != null && item.abilityIds.Length == 1 && item.abilityIds[0] == "A0A0" && item.cooldownId == "A0A0" &&
                    ability != null && ability.Text("code") == "AIda", "Wrong native armor item identity.");
                double Require(string field, double value)
                {
                    Check(ability.TryNumber(field, out var actual, out _) && actual == value, "Active item declaration changed: " + field);
                    return actual;
                }
                bool costDeclared = Array.Exists(ability.fields, f => f.key == "Cost1") ||
                    Array.Exists(ability.overrides, f => f.field == "amcs" && f.level == 1);
                Check(!costDeclared || ability.TryNumber("Cost1", out var cost, out _) && cost == 0,
                    "Native zero mana cost conflicts with map data.");
                armors.Add(row.itemId, new OriginalItemArmorRule { itemId = row.itemId, abilityId = row.abilityId, cooldownGroup = item.cooldownId,
                    buffId = "Bdef", armor = Require("DataA1", 100), duration = Require("Dur1", 6), radius = Require("Area1", 600),
                    cooldown = Require("Cool1", 12), known = true, retired = true });
            }
            BuildUnitySwords(items, combat);
            BuildWards(items, combat, observed);
        }
        void BuildUnitySwords(OriginalItemCatalog items, OriginalCombatCatalog combat)
        {
            // S8:22209..22238 adds275/550 healing AND mana to organic allies.
            // A057/A0BZ add14/24 armor under the same native B011 buff. Other
            // AIda-based items are script trigger dummies, not armor actions.
            string[] ids = { "I02E", "I03Z" }, abilities = { "A057", "A0BZ" };
            for (int index = 0; index < ids.Length; index++)
            {
                var item = items.Item(ids[index]); var ability = combat.Ability(abilities[index]);
                Check(item != null && item.cooldownId == abilities[index] && Array.IndexOf(item.abilityIds, abilities[index]) >= 0 &&
                    ability != null && ability.Text("code") == "AIda" && ability.Text("BuffID1") == "B011" &&
                    ability.Text("targs1") == "ground,air,friend,self,invu,vuln" && ability.Number("DataA1") == (index == 0 ? 14 : 24) &&
                    ability.Number("Dur1") == 16 && ability.Number("HeroDur1") == 16 && ability.Number("Cool1") == 28 && ability.Number("Area1") == 800,
                    "Unity sword declaration conflict.");
                // Native1.26 AIda Cost1=0 in war3/War3x/War3Patch SLK;
                // latest source: War3Patch Units/AbilityData.slk row56189.
                // Transfer of this sparse base cost is declared/derived, not an
                // I02E/I03Z activation observation. A conflicting override fails.
                bool costDeclared = Array.Exists(ability.fields, f => f.key == "Cost1") ||
                    Array.Exists(ability.overrides, f => f.field == "amcs" && f.level == 1);
                Check(!costDeclared || ability.TryNumber("Cost1", out var cost, out _) && cost == 0, "Unity sword mana conflict.");
                armors.Add(item.id, new OriginalItemArmorRule { itemId = item.id, abilityId = ability.id, cooldownGroup = item.cooldownId,
                    buffId = "B011", armor = index == 0 ? 14 : 24, duration = 16, radius = 800, cooldown = 28,
                    known = true, retired = false, manaCost = 0, restoration = index == 0 ? 275 : 550 });
            }
        }
        public OriginalItemArmorRule Armor(string itemId) => itemId != null && armors.TryGetValue(itemId, out var armor) ? armor.Copy() : null;
        public OriginalItemSummonRule Summon(string itemId) => itemId != null && summons.TryGetValue(itemId, out var rule) ? rule.Copy() : null;
        void BuildSummon(OriginalItemCatalog items, OriginalCombatCatalog combat, OriginalObservedItemActive row)
        {
            string[] ids = { "I01A", "I01D", "I01J", "I01M", "I02G", "I07E", "I07K" };
            string[] abilities = { "A0F9", "AIfu", "AIfs", "A0FC", "A099", "A0HB", "A0H9" };
            string[][] types = { new[] { "n01V" }, new[] { "n02K" }, new[] { "n02N", "n02N", "n026", "n026" },
                new[] { "n022" }, new[] { "n01R" }, new[] { "n0AC" }, new[] { "n0AD" } };
            double[] durations = { 45, 60, 30, 45, 60, 45, 45 }, cooldowns = { 25, 15, 15, 25, 45, 20, 20 };
            int index = Array.IndexOf(ids, row.itemId); if (index < 0) return;
            var item = items.Item(row.itemId); var ability = combat.Ability(row.abilityId);
            Check(row.abilityId == abilities[index] && row.nativeUseObserved && row.orderAccepted && row.mode == 0 && row.effectSeconds == 0 &&
                row.manaCost == (index == 4 ? 125 : 0) && row.retired == (index != 4) && item != null &&
                Array.IndexOf(item.abilityIds, row.abilityId) >= 0 && item.cooldownId == row.abilityId &&
                ability != null && ability.Text("code") == "AIfs" && ability.Number("Dur1") == durations[index] &&
                ability.Number("Cool1") == cooldowns[index] && row.summons != null && row.summons.Length == types[index].Length,
                "Unresolved native summon activation.");
            bool costDeclared = Array.Exists(ability.fields, f => f.key == "Cost1") || Array.Exists(ability.overrides, f => f.field == "amcs" && f.level == 1);
            Check(!costDeclared || ability.TryNumber("Cost1", out var declaredCost, out _) && declaredCost == row.manaCost, "Summon mana conflict.");
            var rule = new OriginalItemSummonRule { itemId = row.itemId, abilityId = row.abilityId, cooldownGroup = item.cooldownId,
                manaCost = row.manaCost, cooldown = cooldowns[index], duration = durations[index], retired = row.retired,
                // SUMSP1 closes n026 armor and n01R's enabled second weapon.
                // n02K uses the shared native weapon selector and declared splash rings.
                units = (string[])types[index].Clone(), profiles = new OriginalWorldUnitProfile[row.summons.Length], known = true,
                limitation = null };
            for (int j = 0; j < row.summons.Length; j++)
            {
                var summon = row.summons[j]; var p = summon?.profile; var life = summon?.lifetime;
                Check(summon != null && summon.unitId == types[index][j] && p != null && life != null &&
                    p.hp == p.maxhp && p.hp > 0 && p.mp == p.maxmp && p.mp >= 0 && p.speed > 0 &&
                    OriginalCombatDefinition.IsFinite(p.hp) && OriginalCombatDefinition.IsFinite(p.mp) && OriginalCombatDefinition.IsFinite(p.speed) &&
                    p.owner == 0 && p.dead == 0 && p.paused == 0 && p.BFig == 1 && life.declaredSeconds == rule.duration,
                    "Summon birth profile differs.");
                var unit = combat.Unit(summon.unitId);
                Check(unit != null && unit.Number("HP") == p.maxhp && unit.Number("spd") == p.speed, "Summon declared profile conflict.");
                if (unit.TryNumber("manaN", out var mana, out _)) Check(mana == p.maxmp, "Summon mana maximum conflict.");
                rule.profiles[j] = new OriginalWorldUnitProfile { maxHealth = p.maxhp, maxMana = p.maxmp, moveSpeed = p.speed,
                    collisionRadius = OriginalUnitCollisionRules.Resolve(combat, summon.unitId).radius };
            }
            summons.Add(rule.itemId, rule);
        }
        static void Check(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
    }
}

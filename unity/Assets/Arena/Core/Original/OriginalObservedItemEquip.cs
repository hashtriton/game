using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalEquipSource
    {
        public string cacheName, cacheSha256, probeMapSha256, probeScriptSha256, capturedUtc;
        public int records;
        public bool complete;
    }
    [Serializable]
    public sealed class OriginalEquipSnapshot
    {
        public string phase;
        public int copies, level, xp;
        public int baseStrength, baseAgility, baseIntelligence;
        public double time, strength, agility, intelligence, maxHealth, maxMana, health, mana, moveSpeed;
        public int[] abilityRanks = Array.Empty<int>();
    }
    [Serializable]
    public sealed class OriginalEquipItem
    {
        public string id, error, sourceKey;
        public bool known;
        public string[] abilityIds = Array.Empty<string>();
        public string[] directAbilityIds = Array.Empty<string>();
        public OriginalEquipSnapshot[] snapshots = Array.Empty<OriginalEquipSnapshot>();
    }
    [Serializable]
    public sealed class OriginalObservedItemEquip
    {
        public int schemaVersion, heroLevel;
        public string mapSha256, engineVersion, heroId;
        public OriginalEquipSource source;
        public OriginalEquipItem[] items = Array.Empty<OriginalEquipItem>();
        public string[] limits = Array.Empty<string>();
    }

    // Frozen measurements are scoped to the exact sampled item identities.
    // Ordinary-family application outside this sample is a declared rule, not
    // a claim that every hero/level/mixed inventory was measured in Warcraft.
    internal sealed class OriginalEquipResolver
    {
        internal sealed class Measurement
        {
            internal string[] abilities;
            internal int[] ranks;
            internal double strength, agility, intelligence, maxHealth, maxMana;
        }
        readonly Dictionary<string, Measurement> values = new Dictionary<string, Measurement>(StringComparer.Ordinal);
        internal OriginalEquipResolver(OriginalObservedItemEquip source)
        {
            if (source == null || source.schemaVersion != 1 || source.mapSha256 != OriginalNativeCatalog.ExpectedMapSha256 ||
                source.engineVersion != "1.26.0.6401" || source.heroId != "H008" || source.heroLevel != 1 || source.items == null || source.items.Length != 43 ||
                source.source == null || !source.source.complete || source.source.records != 43 || source.source.cacheName != "LiAEquip1.w3v" ||
                source.source.cacheSha256 != "b0a5ae8f73d03fbfcd8bb4de09901b188242a5751551c538ffd19f6644933623" ||
                source.source.probeMapSha256 != "495c55160012f8aa818d73db54f8e9794e58ffaebfd1fb5b00f78375721d13a3" ||
                source.source.probeScriptSha256 != "ba7d937448a3153ae86dae9b760d2ce5a39173404ab0c48c9bbcc4f86fdfe7bd")
                throw new ArgumentException("Invalid native equip evidence identity.");
            string[] phases = { "baseline", "one_immediate", "one_delayed", "two_immediate", "two_delayed", "remove_first_immediate", "remove_first_delayed", "empty_immediate", "empty_delayed" };
            int[] counts = { 0, 1, 1, 2, 2, 1, 1, 0, 0 };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in source.items)
            {
                if (item == null || item.id == null || item.id.Length != 4 || !seen.Add(item.id) || item.sourceKey != "item_" + item.id ||
                    item.abilityIds == null || item.abilityIds.Length == 0 || item.known == !string.IsNullOrEmpty(item.error))
                    throw new ArgumentException("Invalid native equip item identity/state.");
                if (!item.known) continue;
                if (item.snapshots == null || item.snapshots.Length != 9) throw new ArgumentException("Incomplete equip sequence.");
                for (var i = 0; i < item.snapshots.Length; i++)
                {
                    var s = item.snapshots[i];
                    if (s == null || s.phase != phases[i] || s.copies != counts[i] || s.level != 1 || s.xp != 0 ||
                        !Finite(s.time) || s.time < 0 || (i > 0 && s.time < item.snapshots[i - 1].time) ||
                        !Finite(s.strength) || !Finite(s.agility) || !Finite(s.intelligence) || s.strength < 0 || s.agility < 0 || s.intelligence < 0 ||
                        !Finite(s.health) || !Finite(s.maxHealth) || s.health <= 0 || s.health > s.maxHealth ||
                        !Finite(s.mana) || !Finite(s.maxMana) || s.mana < 0 || s.mana > s.maxMana ||
                        !Finite(s.moveSpeed) || s.moveSpeed != 250 || s.abilityRanks == null || s.abilityRanks.Length != item.abilityIds.Length)
                        throw new ArgumentException("Invalid native equip snapshot.");
                    foreach (var rank in s.abilityRanks)
                        if (rank != (counts[i] == 0 ? 0 : 1)) throw new ArgumentException("Unresolved native equip rank transition.");
                }
                var baseline = item.snapshots[0]; var one = item.snapshots[1];
                if (baseline.strength != 22 || baseline.agility != 6 || baseline.intelligence != 7 || baseline.maxHealth != 631 || baseline.maxMana != 145)
                    throw new ArgumentException("Unexpected equip baseline.");
                var measurement = new Measurement { abilities = (string[])item.abilityIds.Clone(), ranks = (int[])one.abilityRanks.Clone(),
                    strength = one.strength - 22, agility = one.agility - 6, intelligence = one.intelligence - 7,
                    maxHealth = one.maxHealth - 631, maxMana = one.maxMana - 145 };
                foreach (var s in item.snapshots)
                    if (s.strength != 22 + measurement.strength * s.copies || s.agility != 6 + measurement.agility * s.copies ||
                        s.intelligence != 7 + measurement.intelligence * s.copies || s.maxHealth != 631 + measurement.maxHealth * s.copies ||
                        s.maxMana != 145 + measurement.maxMana * s.copies)
                        throw new ArgumentException("Equip measurements do not establish reversible additive stats.");
                values.Add(item.id, measurement);
            }
        }
        internal bool TryGet(string id, out Measurement value) => values.TryGetValue(id, out value);
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

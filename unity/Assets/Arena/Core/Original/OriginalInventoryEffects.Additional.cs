using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalInventoryEffects
    {
        readonly Dictionary<string, OriginalEquipResolver.Measurement> additionalEquip =
            new Dictionary<string, OriginalEquipResolver.Measurement>(StringComparer.Ordinal);
        readonly Dictionary<string, string> additionalProfileEvidence =
            new Dictionary<string, string>(StringComparer.Ordinal);

        // Additional batches retain failed observations and unsupported native
        // families. A valid observation is not automatically an executable
        // profile; each item must independently pass the decomposition below.
        void RegisterAdditionalEquip(OriginalObservedItemEquip[] batches, OriginalItemPassiveCatalog frozen)
        {
            if (batches == null || batches.Length == 0) return;
            var allItems = new HashSet<string>(StringComparer.Ordinal);
            var sources = new HashSet<string>(StringComparer.Ordinal);
            foreach (var batch in batches)
            {
                CheckAdditional(batch != null && batch.mapSha256 == MapSha256 && batch.engineVersion == "1.26.0.6401" &&
                    batch.heroId == "H008" && batch.heroLevel == 1 && batch.source != null && batch.source.complete &&
                    !string.IsNullOrEmpty(batch.source.capturedUtc) && IsHash(batch.source.cacheSha256), "Invalid additional equip identity");
                bool compact = batch.schemaVersion == 3;
                string cache = compact ? "LiAEq2.w3v" : "LiAEqExtra1.w3v";
                string map = compact ? "980448f918e4b9c4434070f48622b1a6029d21cb01a87a9cd4ac93a76a28d831" : "1a0aff5cfcf431ba8f13d6ee348c90f18087a1b015fe44c74b61e782f9fa17f6";
                string script = compact ? "fae3aacc7c0e764c79473aaf6ed02046923c861605d1e4935a892c0c71d8c3b2" : "7e59c7019f5c260f7b4775ff0d21cd122658a1b525358610f9356446a4fbb370";
                int count = compact ? 221 : 8;
                CheckAdditional((compact || batch.schemaVersion == 4) && batch.source.cacheName == cache && sources.Add(cache) &&
                    batch.source.probeMapSha256 == map && batch.source.probeScriptSha256 == script &&
                    batch.source.records == count && batch.items != null && batch.items.Length == count, "Wrong additional equip source/matrix");
                foreach (var row in batch.items)
                {
                    CheckAdditional(row != null && Rawcode(row.id) && items.ContainsKey(row.id) && allItems.Add(row.id) &&
                        !nativeProfiles.ContainsKey(row.id) && row.sourceKey == "item_" + row.id &&
                        row.directAbilityIds != null && row.abilityIds != null && row.snapshots != null &&
                        row.known == string.IsNullOrEmpty(row.error), "Invalid additional item state");
                    var item = items[row.id];
                    CheckAdditional(Equal(item.abilityIds, row.directAbilityIds), "Additional direct abilities differ from map");
                    var expected = new List<string>();
                    foreach (string id in item.abilityIds) AdditionalQueries(id, expected, frozen);
                    CheckAdditional(Equal(expected.ToArray(), row.abilityIds), "Additional query closure differs from map");
                    if (!row.known)
                    { CheckAdditional(row.snapshots.Length == 0, "Failed equip row contains known snapshots"); continue; }
                    ValidateAdditionalSnapshots(row);
                    var baseline = row.snapshots[0]; var first = row.snapshots[1];
                    var measurement = new OriginalEquipResolver.Measurement { abilities = Strings(item.abilityIds), ranks = new int[item.abilityIds.Length],
                        strength = first.strength - baseline.strength, agility = first.agility - baseline.agility,
                        intelligence = first.intelligence - baseline.intelligence, maxHealth = first.maxHealth - baseline.maxHealth,
                        maxMana = first.maxMana - baseline.maxMana };
                    bool ordinary = true;
                    double moveBonus = 0;
                    bool hasMovementAura = false;
                    for (int i = 0; i < item.abilityIds.Length; i++)
                    {
                        int query = Array.IndexOf(row.abilityIds, item.abilityIds[i]);
                        measurement.ranks[i] = first.abilityRanks[query];
                        var ability = Array.Find(frozen.abilities, a => a.id == item.abilityIds[i]);
                        hasMovementAura |= ability.baseCode == "AUau" || ability.baseCode == "AOae";
                        // Spellbook child occurrence/order and non-rank1 item
                        // abilities need a separate executor, not a flat bonus.
                        ordinary &= measurement.ranks[i] == 1 || ability.baseCode == "ANcl" && measurement.ranks[i] == 0;
                        if (ability.baseCode == "AIms")
                            foreach (var parameter in ability.levels[0].modifiers)
                                if (parameter.known && parameter.stat == "moveSpeedFlat" && parameter.operation == "maximum")
                                    moveBonus = Math.Max(moveBonus, parameter.value);
                    }
                    // Spellbook children can supply the same native boot maximum.
                    // Use queried leaves once; the container contributes no speed.
                    foreach (string id in row.abilityIds)
                    {
                        var ability = Array.Find(frozen.abilities, a => a.id == id);
                        hasMovementAura |= ability.baseCode == "AUau" || ability.baseCode == "AOae";
                        if (ability.baseCode == "AIms")
                            foreach (var parameter in ability.levels[0].modifiers)
                                if (parameter.known && parameter.stat == "moveSpeedFlat" && parameter.operation == "maximum")
                                    moveBonus = Math.Max(moveBonus, parameter.value);
                    }
                    // Exact reversible native movement is validated below;
                    // aura movement itself is evaluated on recipients at runtime.
                    if (hasMovementAura) moveBonus = first.moveSpeed - baseline.moveSpeed;
                    foreach (var state in row.snapshots)
                    {
                        ordinary &= state.strength == baseline.strength + measurement.strength * state.copies &&
                            state.agility == baseline.agility + measurement.agility * state.copies &&
                            state.intelligence == baseline.intelligence + measurement.intelligence * state.copies &&
                            state.maxHealth == baseline.maxHealth + measurement.maxHealth * state.copies &&
                            state.maxMana == baseline.maxMana + measurement.maxMana * state.copies &&
                            (state.moveSpeed == baseline.moveSpeed + (state.copies == 0 ? 0 : moveBonus) ||
                             hasMovementAura && state.copies == 0 && state.moveSpeed == baseline.moveSpeed + moveBonus);
                        for (int q = 0; q < row.abilityIds.Length; q++)
                            ordinary &= state.abilityRanks[q] == (state.copies == 0 ? 0 : first.abilityRanks[q]);
                    }
                    // A spellbook is a container, not a second copy of each
                    // bonus. Query evidence supplies every leaf's actual rank;
                    // declared traversal retains occurrences for vitality order.
                    var profileItem = new OriginalItemEffectDefinition { id = item.id };
                    var leaves = new List<string>(); var ranks = new List<int>();
                    void Expand(string id, HashSet<string> path)
                    {
                        CheckAdditional(path.Add(id), "Cyclic observed spellbook");
                        var ability = Array.Find(frozen.abilities, a => a.id == id);
                        int rank = first.abilityRanks[Array.IndexOf(row.abilityIds,id)];
                        if (ability.isSpellbook && rank > 0)
                        {
                            var level = Array.Find(ability.levels,l=>l.level==rank);
                            CheckAdditional(level != null && level.spellbookFieldKnown,"Unresolved observed spellbook");
                            foreach(string child in level.spellbookAbilityIds)Expand(child,path);
                        }
                        else { leaves.Add(id); ranks.Add(rank); }
                        path.Remove(id);
                    }
                    foreach(string id in item.abilityIds)Expand(id,new HashSet<string>(StringComparer.Ordinal));
                    profileItem.abilityIds=leaves.ToArray();
                    additionalEquip.Add(item.id,new OriginalEquipResolver.Measurement { abilities=Strings(row.abilityIds),
                        ranks=(int[])first.abilityRanks.Clone(),strength=measurement.strength,agility=measurement.agility,
                        intelligence=measurement.intelligence,maxHealth=measurement.maxHealth,maxMana=measurement.maxMana });
                    measurement.abilities=leaves.ToArray();measurement.ranks=ranks.ToArray();
                    if (!ordinary) continue;
                    var profile = Profile(profileItem, measurement, frozen, true);
                    if (profile != null)
                    {
                        nativeProfiles.Add(item.id, profile);
                        additionalProfileEvidence.Add(item.id, "Observed equip levels and reversible attributes/maxima:" +
                            batch.source.cacheName + ";cacheSha256=" + batch.source.cacheSha256 + ";row=" + row.sourceKey +
                            ". Native-family decomposition is separate from active/scripted effects.");
                    }
                }
            }
        }
        static void AdditionalQueries(string id, List<string> target, OriginalItemPassiveCatalog source)
        {
            if (target.Contains(id)) return;
            var ability = Array.Find(source.abilities, a => a.id == id);
            CheckAdditional(ability != null, "Unknown additional query ability"); target.Add(id);
            foreach (var level in ability.levels)
                foreach (var child in level.spellbookAbilityIds) AdditionalQueries(child, target, source);
        }
        static void ValidateAdditionalSnapshots(OriginalEquipItem row)
        {
            string[] phases = { "baseline", "one_immediate", "one_delayed", "two_immediate", "two_delayed",
                "remove_first_immediate", "remove_first_delayed", "empty_immediate", "empty_delayed" };
            int[] counts = { 0,1,1,2,2,1,1,0,0 };
            CheckAdditional(row.snapshots.Length == 9, "Incomplete additional equip snapshots");
            double priorTime = -1;
            for (int i = 0; i < 9; i++)
            {
                var s = row.snapshots[i];
                CheckAdditional(s != null && s.phase == phases[i] && s.copies == counts[i] && s.level == 1 && s.xp == 0 &&
                    s.baseStrength == 22 && s.baseAgility == 6 && s.baseIntelligence == 7 &&
                    Finite(s.time) && s.time >= priorTime && Finite(s.strength) && s.strength >= 0 &&
                    Finite(s.agility) && s.agility >= 0 && Finite(s.intelligence) && s.intelligence >= 0 &&
                    Finite(s.health) && s.health > 0 && Finite(s.maxHealth) && s.health <= s.maxHealth &&
                    Finite(s.mana) && s.mana >= 0 && Finite(s.maxMana) && s.mana <= s.maxMana &&
                    Finite(s.moveSpeed) && s.moveSpeed >= 0 && s.abilityRanks != null && s.abilityRanks.Length == row.abilityIds.Length,
                    "Invalid additional native state");
                foreach (int rank in s.abilityRanks) CheckAdditional(rank >= 0, "Negative observed native rank");
                priorTime = s.time;
            }
            var b = row.snapshots[0];
            CheckAdditional(b.strength == 22 && b.agility == 6 && b.intelligence == 7 && b.maxHealth == 631 &&
                b.maxMana == 145 && b.moveSpeed == 250, "Unexpected additional equip baseline");
        }
        static bool Equal(string[] first, string[] second)
        { if (first.Length != second.Length) return false; for (int i = 0; i < first.Length; i++) if (first[i] != second[i]) return false; return true; }
        static bool IsHash(string value)
        { if (value == null || value.Length != 64) return false; foreach (char c in value) if (c < '0' || c > '9' && c < 'a' || c > 'f') return false; return true; }
        static void CheckAdditional(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
    }
}

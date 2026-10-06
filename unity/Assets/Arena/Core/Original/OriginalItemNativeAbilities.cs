using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalItemNativeCombatAbility
    {
        public long instanceId;
        public string itemId, abilityId;
        public int rank, occurrence;
        public bool rankDerived;
    }

    public sealed partial class OriginalInventoryEffects
    {
        readonly Dictionary<string, OriginalPassiveAbility> nativeAbilityDefinitions =
            new Dictionary<string, OriginalPassiveAbility>(StringComparer.Ordinal);

        // Ordered physical item occurrences, never a set of native families.
        // Callers choose which runtime effects they implement. Servant items do
        // not affect the hero; image inheritance requires its separate capture.
        public OriginalItemNativeCombatAbility[] CombatAbilities(OriginalInventorySnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var identities = new HashSet<long>();
            var hero = ValidateSlots(snapshot.heroSlots, identities);
            ValidateSlots(snapshot.servantSlots, identities);
            var result = new List<OriginalItemNativeCombatAbility>();
            foreach (var instance in hero)
            {
                if (instance == null) continue;
                var item = items[instance.itemId];
                OriginalEquipResolver.Measurement measurement = null;
                equip?.TryGet(item.id, out measurement);
                if (measurement == null) additionalEquip.TryGetValue(item.id, out measurement);
                var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
                void Visit(string id, HashSet<string> stack)
                {
                    if (!stack.Add(id)) throw new InvalidOperationException("Item spellbook cycle.");
                    var definition = nativeAbilityDefinitions[id];
                    int index = measurement == null ? -1 : Array.IndexOf(measurement.abilities, id);
                    bool derived = index < 0;
                    // A runtime query of zero stays zero. Only absent evidence
                    // uses the authored first level, explicitly labelled derived.
                    int rank = derived ? (definition.levelsKnown && definition.declaredLevels >= 1 ? 1 : 0) : measurement.ranks[index];
                    if (rank > 0)
                    {
                        var level = Array.Find(definition.levels, x => x.level == rank && x.withinDeclaredLevels);
                        if (level == null) throw new InvalidOperationException("Unknown item native ability rank.");
                        if (definition.isSpellbook)
                        {
                            if (!level.spellbookFieldKnown) throw new InvalidOperationException("Unknown item spellbook contents.");
                            foreach (var child in level.spellbookAbilityIds) Visit(child, stack);
                        }
                        else
                        {
                            occurrences.TryGetValue(id, out var occurrence); occurrences[id] = occurrence + 1;
                            result.Add(new OriginalItemNativeCombatAbility { instanceId = instance.instanceId,
                                itemId = item.id, abilityId = id, rank = rank, occurrence = occurrence, rankDerived = derived });
                        }
                    }
                    stack.Remove(id);
                }
                foreach (var id in item.abilityIds) Visit(id, new HashSet<string>(StringComparer.Ordinal));
            }
            return result.ToArray();
        }
    }
}

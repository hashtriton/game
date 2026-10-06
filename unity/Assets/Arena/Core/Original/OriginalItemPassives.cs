using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalPassiveParameter
    {
        public string stat;
        public string operation;
        public string stackingGroup;
        public string stackingRule;
        public bool known;
        public double value;
        public string field;
        public string column;
        public string state;
        public string[] sources = Array.Empty<string>();
    }

    [Serializable]
    public sealed class OriginalPassiveLevel
    {
        public int level;
        public bool withinDeclaredLevels;
        public OriginalPassiveParameter[] modifiers = Array.Empty<OriginalPassiveParameter>();
        public string[] spellbookAbilityIds = Array.Empty<string>();
        public bool spellbookFieldKnown;
    }

    [Serializable]
    public sealed class OriginalPassiveAbility
    {
        public string id;
        public string baseCode;
        public bool directItemAbility;
        public bool passiveFamilyMapped;
        public bool isSpellbook;
        public int declaredLevels;
        public bool levelsKnown;
        public OriginalPassiveLevel[] levels = Array.Empty<OriginalPassiveLevel>();
        public string[] jassFunctions = Array.Empty<string>();
    }

    [Serializable]
    public sealed class OriginalItemPassiveCatalog
    {
        public int schemaVersion;
        public string mapSha256;
        public OriginalPassiveAbility[] abilities = Array.Empty<OriginalPassiveAbility>();
        public OriginalItemEffectDefinition[] items = Array.Empty<OriginalItemEffectDefinition>();
        public OriginalObservedItemEquip observedEquip;
        public OriginalObservedItemEquip[] observedAdditionalEquip = Array.Empty<OriginalObservedItemEquip>();
        public string[] supportedNativeFamilies = Array.Empty<string>();
        public int directAbilityCount;
        public int spellbookClosureCount;
        public int mappedAbilityCount;
        public bool runtimeBaselineObserved;
        public bool retailStackingResolved;
        public string[] mappingEvidence = Array.Empty<string>();
        public string[] referenceUrls = Array.Empty<string>();
    }

    public sealed class OriginalItemAbilityState
    {
        public long ItemInstanceId;
        public string AbilityId;
        public int Level;
        public int Occurrence;
    }

    public sealed class OriginalStatModifier
    {
        public long ItemInstanceId;
        public string AbilityId;
        public int Level;
        public int Occurrence;
        public string Stat;
        public string Operation;
        public double Value;
        public string StackingGroup;
        public string StackingRule;
        public string NativeField;
        public string[] Sources;
    }

    public sealed class OriginalItemEffectGap
    {
        public long ItemInstanceId;
        public string AbilityId;
        public int Level;
        public int Occurrence;
        public string Reason;
    }

    public sealed class OriginalPassiveEvaluation
    {
        public readonly List<OriginalStatModifier> Modifiers = new List<OriginalStatModifier>();
        public readonly List<OriginalItemEffectGap> Gaps = new List<OriginalItemEffectGap>();
        public bool AllRequestedFieldsMapped => Gaps.Count == 0;
        public bool RetailStackingVerified { get; internal set; }
        public bool RuntimeBaselineObserved { get; internal set; }
    }

    // Produces individual contributions only. It does not silently select a retail stacking rule.
    // A combat integration must explicitly consume each operation and resolve stacking policy.
    public sealed class OriginalItemPassiveEvaluator
    {
        private readonly OriginalItemPassiveCatalog catalog;
        private readonly Dictionary<string, OriginalPassiveAbility> abilities = new Dictionary<string, OriginalPassiveAbility>(StringComparer.Ordinal);

        public OriginalItemPassiveEvaluator(OriginalItemPassiveCatalog catalog)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            foreach (var ability in catalog.abilities) abilities.Add(ability.id, ability);
        }

        public OriginalPassiveEvaluation Evaluate(IEnumerable<OriginalItemAbilityState> states,
            Func<long, string, int?> spellbookChildLevel = null)
        {
            if (states == null) throw new ArgumentNullException(nameof(states));
            var result = new OriginalPassiveEvaluation
            {
                RetailStackingVerified = catalog.retailStackingResolved,
                RuntimeBaselineObserved = catalog.runtimeBaselineObserved
            };
            var evaluated = new HashSet<string>(StringComparer.Ordinal);
            var declared = new Dictionary<string, int>(StringComparer.Ordinal);
            var input = new List<OriginalItemAbilityState>();
            foreach (var state in states)
            {
                if (state == null) throw new ArgumentException("Null ability state.");
                if (state.Occurrence < 0) throw new ArgumentException("Invalid ability occurrence.");
                var identity = state.ItemInstanceId + ":" + state.AbilityId + ":" + state.Occurrence;
                if (declared.TryGetValue(identity, out var existing) && existing != state.Level)
                    throw new ArgumentException("Conflicting levels for one item ability instance.");
                declared[identity] = state.Level;
                input.Add(state);
            }
            foreach (var state in input)
            {
                Visit(state, spellbookChildLevel, evaluated, new HashSet<string>(StringComparer.Ordinal), result);
            }
            return result;
        }

        private void Visit(OriginalItemAbilityState state, Func<long, string, int?> childLevel,
            HashSet<string> evaluated, HashSet<string> active, OriginalPassiveEvaluation result)
        {
            var key = state.ItemInstanceId + ":" + state.AbilityId + ":" + state.Level + ":" + state.Occurrence;
            if (active.Contains(key)) { Gap(state, "spellbook-cycle", result); return; }
            if (!evaluated.Add(key)) return;
            if (state.Level <= 0 || string.IsNullOrEmpty(state.AbilityId) || !abilities.TryGetValue(state.AbilityId, out var ability))
            { Gap(state, "unknown-ability-or-level", result); return; }
            var level = Array.Find(ability.levels, x => x.level == state.Level);
            if (!ability.levelsKnown || level == null || !level.withinDeclaredLevels)
            { Gap(state, "level-not-supported-by-declaration", result); return; }
            active.Add(key);
            if (ability.isSpellbook)
            {
                if (!level.spellbookFieldKnown) Gap(state, "spellbook-contents-unresolved", result);
                foreach (var id in level.spellbookAbilityIds)
                {
                    var currentLevel = childLevel?.Invoke(state.ItemInstanceId, id);
                    if (!currentLevel.HasValue)
                    { Gap(new OriginalItemAbilityState { ItemInstanceId = state.ItemInstanceId, AbilityId = id, Occurrence = state.Occurrence }, "spellbook-child-level-unresolved", result); continue; }
                    Visit(new OriginalItemAbilityState { ItemInstanceId = state.ItemInstanceId, AbilityId = id, Level = currentLevel.Value, Occurrence = state.Occurrence },
                        childLevel, evaluated, active, result);
                }
            }
            else if (!ability.passiveFamilyMapped) Gap(state, "native-family-unimplemented:" + ability.baseCode, result);
            else
            {
                foreach (var parameter in level.modifiers)
                {
                    if (!parameter.known) { Gap(state, parameter.state + ":" + parameter.column, result); continue; }
                    if (double.IsNaN(parameter.value) || double.IsInfinity(parameter.value))
                    { Gap(state, "nonfinite-parameter:" + parameter.column, result); continue; }
                    result.Modifiers.Add(new OriginalStatModifier
                    {
                        ItemInstanceId = state.ItemInstanceId, AbilityId = state.AbilityId, Level = state.Level, Occurrence = state.Occurrence,
                        Stat = parameter.stat, Operation = parameter.operation, Value = parameter.value,
                        StackingGroup = parameter.stackingGroup, StackingRule = parameter.stackingRule,
                        NativeField = parameter.field, Sources = parameter.sources
                    });
                }
            }
            if (ability.jassFunctions.Length > 0) Gap(state, "script-references-need-handler-coverage", result);
            active.Remove(key);
        }

        private static void Gap(OriginalItemAbilityState state, string reason, OriginalPassiveEvaluation result)
        {
            result.Gaps.Add(new OriginalItemEffectGap
            {
                ItemInstanceId = state.ItemInstanceId, AbilityId = state.AbilityId, Level = state.Level, Occurrence = state.Occurrence, Reason = reason
            });
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalNativeEvidence
    {
        public int source;
        public int line;
        public int offset;
        public int valueOffset;
        public string variant;
    }

    [Serializable]
    public sealed class OriginalNativeSource
    {
        public int id;
        public string archive;
        public string entry;
        public string sha256;
        public bool independentReadersAgree;
    }

    [Serializable]
    public sealed class OriginalNativeValue
    {
        public string key;
        public string field;
        public bool known;
        public string state;
        public string kind;
        public double number;
        public double[] numbers;
        public string text;
        public OriginalNativeEvidence[] sources;

        public double Require()
        {
            RequireKind("number");
            if (!Finite(number)) throw new InvalidOperationException("Non-finite native value.");
            return number;
        }

        public double[] RequireNumbers()
        {
            RequireKind("numbers");
            if (numbers == null || numbers.Length == 0) throw new InvalidOperationException("Empty native number table.");
            foreach (var value in numbers)
                if (!Finite(value)) throw new InvalidOperationException("Non-finite native table.");
            return (double[])numbers.Clone();
        }

        public string RequireText()
        {
            if (!known || !ResolvedState(state) ||
                (kind != "mechanical-string" && kind != "destructable-armor-string") || text == null)
                throw new InvalidOperationException("Unresolved native text " + (field ?? key) + ": " + state);
            return text;
        }

        private void RequireKind(string expected)
        {
            if (!known || !ResolvedState(state) || kind != expected)
                throw new InvalidOperationException("Unresolved native value " + (field ?? key) + ": " + state);
        }

        internal static bool ResolvedState(string value) => value == "map-declaration" ||
            value == "map-binary-override" || value == "native126-dataset-invariant-declaration" ||
            value == "native126-original-table-inherited-declaration";
        internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        internal static OriginalNativeValue Missing(string name) => new OriginalNativeValue
            { key = name, field = name, known = false, state = "unresolved-missing-field" };
    }

    [Serializable]
    public sealed class OriginalNativeObject
    {
        public string id;
        public bool hasStockSameRawcode;
        public OriginalNativeValue[] fields;
    }

    [Serializable]
    public sealed class OriginalNativeXpLevel
    {
        public int level;
        public int cumulative;
        public int fromPrevious;
    }

    [Serializable]
    public sealed class OriginalNativeHeroXp
    {
        public bool known;
        public string state;
        public string thresholdMeaning;
        public string[] dependencies;
        public OriginalNativeXpLevel[] levels;
    }

    // Selective JSON projection: the full source keeps its byte fingerprint, while
    // unused abilities, candidate defaults and pathing images are not materialized.
    // Known declaration values remain declarations, not a native-runtime claim.
    [Serializable]
    public sealed class OriginalNativeCatalog
    {
        public const string ExpectedMapSha256 = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        public int schemaVersion;
        public string rulesVersion;
        public string mapSha256;
        public string clientFileVersion;
        public bool runtimeObserved;
        public bool datasetSelectionKnown;
        public bool originalMapAuthorEngineVersionKnown;
        public OriginalNativeSource[] sources;
        public OriginalNativeValue[] constants;
        public OriginalNativeHeroXp heroXp;
        public OriginalNativeObject[] items;
        public OriginalNativeObject[] destructables;

        private Dictionary<string, OriginalNativeValue> constantIndex;
        private Dictionary<string, Dictionary<string, OriginalNativeValue>> itemIndex, destructableIndex;
        private int[] xpThresholds;

        public void BuildIndexes()
        {
            if (schemaVersion != 1 || rulesVersion != "lia39-native126-static-v1" ||
                mapSha256 != ExpectedMapSha256 || clientFileVersion != "1.26.0.6401" ||
                runtimeObserved || datasetSelectionKnown || originalMapAuthorEngineVersionKnown)
                throw new ArgumentException("Unsupported native declaration catalog or evidence flags.");
            var sourceIds = new HashSet<int>();
            if (sources == null || sources.Length == 0) throw new ArgumentException("Native sources are missing.");
            foreach (var source in sources)
                if (source == null || source.id < 0 || !sourceIds.Add(source.id) ||
                    string.IsNullOrEmpty(source.archive) || string.IsNullOrEmpty(source.entry) ||
                    !IsSha(source.sha256) || !source.independentReadersAgree)
                    throw new ArgumentException("Invalid native source.");
            var newConstants = IndexFields(constants, true, sourceIds);
            var newItems = IndexObjects(items, sourceIds);
            var newDestructables = IndexObjects(destructables, sourceIds);
            // These three declarations are the complete destructable scope exported
            // for this map. Do not infer HP for other decorative rawcodes.
            if (newDestructables.Count != 3) throw new ArgumentException("Incomplete native barrel declarations.");
            foreach (var id in new[] { "LTbr", "LTbs", "LTex" })
                if (!newDestructables.TryGetValue(id, out var fields) || !fields.TryGetValue("HP", out var hp) || hp.Require() <= 0)
                    throw new ArgumentException("Missing or invalid native barrel health.");
            var thresholds = ValidateXp(newConstants);
            // Publish only after every section passes, including the derived XP table.
            constantIndex = newConstants;
            itemIndex = newItems;
            destructableIndex = newDestructables;
            xpThresholds = thresholds;
        }

        public OriginalNativeValue Constant(string key)
        {
            CheckName(key); EnsureIndexes();
            return constantIndex.TryGetValue(key, out var value) ? value : OriginalNativeValue.Missing(key);
        }

        public OriginalNativeValue ItemField(string itemId, string field)
        {
            CheckName(itemId); CheckName(field); EnsureIndexes();
            return Field(itemIndex, itemId, field);
        }

        public OriginalNativeValue DestructableField(string id, string field)
        {
            CheckName(id); CheckName(field); EnsureIndexes();
            return Field(destructableIndex, id, field);
        }

        public OriginalNativeValue DestructableHP(string id) => DestructableField(id, "HP");

        public int LevelFromExperience(int experience)
        {
            if (experience < 0) throw new ArgumentOutOfRangeException(nameof(experience));
            EnsureIndexes();
            // Cumulative XP, inclusive at the threshold. Experience over the last
            // threshold stays at the declared maximum level.
            var index = Array.BinarySearch(xpThresholds, experience);
            return index >= 0 ? index + 1 : ~index;
        }

        private void EnsureIndexes() { if (constantIndex == null) BuildIndexes(); }
        private static void CheckName(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("A native ID/field is required.");
        }

        private static OriginalNativeValue Field(Dictionary<string, Dictionary<string, OriginalNativeValue>> index, string id, string field)
        {
            return index.TryGetValue(id, out var fields) && fields.TryGetValue(field, out var value)
                ? value : OriginalNativeValue.Missing(id + "." + field);
        }

        private static Dictionary<string, Dictionary<string, OriginalNativeValue>> IndexObjects(OriginalNativeObject[] rows, HashSet<int> sources)
        {
            if (rows == null || rows.Length == 0) throw new ArgumentException("Missing native objects.");
            var result = new Dictionary<string, Dictionary<string, OriginalNativeValue>>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row == null || row.id == null || row.id.Length != 4 || result.ContainsKey(row.id))
                    throw new ArgumentException("Duplicate or invalid native object ID.");
                result.Add(row.id, IndexFields(row.fields, false, sources));
            }
            return result;
        }

        private static Dictionary<string, OriginalNativeValue> IndexFields(OriginalNativeValue[] rows, bool constants, HashSet<int> sources)
        {
            if (rows == null || rows.Length == 0) throw new ArgumentException("Missing native fields.");
            var result = new Dictionary<string, OriginalNativeValue>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row == null) throw new ArgumentException("Null native field.");
                var key = constants ? row.key : row.field;
                if (string.IsNullOrEmpty(key) || result.ContainsKey(key) || string.IsNullOrEmpty(row.state))
                    throw new ArgumentException("Duplicate or invalid native field.");
                if (!row.known)
                {
                    if (!row.state.StartsWith("unresolved", StringComparison.Ordinal))
                        throw new ArgumentException("Unknown field has a resolved state.");
                    // A candidate/default payload does not become usable when known=false.
                }
                else
                {
                    if (!OriginalNativeValue.ResolvedState(row.state) || row.sources == null || row.sources.Length == 0)
                        throw new ArgumentException("Known native field has no declaration evidence.");
                    switch (row.kind)
                    {
                        case "number": row.Require(); break;
                        case "numbers": row.RequireNumbers(); break;
                        case "mechanical-string": case "destructable-armor-string": row.RequireText(); break;
                        default: throw new ArgumentException("Unknown native value kind.");
                    }
                }
                if (row.sources != null)
                    foreach (var origin in row.sources)
                        if (origin == null || !sources.Contains(origin.source) || origin.line < 0 || origin.offset < 0 || origin.valueOffset < 0)
                            throw new ArgumentException("Invalid native field source reference.");
                result.Add(key, row);
            }
            return result;
        }

        private int[] ValidateXp(Dictionary<string, OriginalNativeValue> index)
        {
            var dependencies = new[] { "MaxHeroLevel", "NeedHeroXP", "NeedHeroXPFormulaA", "NeedHeroXPFormulaB", "NeedHeroXPFormulaC" };
            if (heroXp == null || !heroXp.known || heroXp.state != "derived" || heroXp.levels == null ||
                heroXp.thresholdMeaning != "cumulative total to reach target level" || heroXp.dependencies == null ||
                heroXp.dependencies.Length != dependencies.Length ||
                !new HashSet<string>(heroXp.dependencies, StringComparer.Ordinal).SetEquals(dependencies))
                throw new ArgumentException("Missing or unresolved native XP derivation.");
            foreach (var key in dependencies)
                if (!index.ContainsKey(key)) throw new ArgumentException("Missing XP constant " + key);
            var maximum = Integer(index["MaxHeroLevel"].Require());
            if (maximum < 1 || maximum > 10000 || heroXp.levels.Length != maximum)
                throw new ArgumentException("XP level count does not match MaxHeroLevel.");
            var tableValue = index["NeedHeroXP"];
            var table = tableValue.kind == "number" ? new[] { tableValue.Require() } : tableValue.RequireNumbers();
            var a = Integer(index["NeedHeroXPFormulaA"].Require());
            var b = Integer(index["NeedHeroXPFormulaB"].Require());
            var c = Integer(index["NeedHeroXPFormulaC"].Require());
            foreach (var value in table) Integer(value);
            var result = new int[maximum];
            long previous = 0;
            for (var i = 0; i < maximum; i++)
            {
                var row = heroXp.levels[i];
                long expected = i == 0 ? 0 : i - 1 < table.Length ? Integer(table[i - 1]) :
                    checked((long)a * previous + (long)b * (i + 1) + c);
                if (row == null || row.level != i + 1 || expected < 0 || expected > int.MaxValue ||
                    row.cumulative != expected || row.fromPrevious != expected - previous || (i > 0 && expected <= previous))
                    throw new ArgumentException("Native XP table contradicts declaration recurrence.");
                result[i] = row.cumulative;
                previous = expected;
            }
            return result;
        }

        private static int Integer(double value)
        {
            if (!OriginalNativeValue.Finite(value) || value < int.MinValue || value > int.MaxValue || value != Math.Truncate(value))
                throw new ArgumentException("Native XP declaration must be an integer.");
            return (int)value;
        }

        private static bool IsSha(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (var c in value) if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) return false;
            return true;
        }
    }
}

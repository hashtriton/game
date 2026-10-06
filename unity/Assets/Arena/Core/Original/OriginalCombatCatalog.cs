using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalCombatField
    {
        public string key;
        public double number;
        public bool isNumber;
        public string text;
        public bool conflict;
        public string[] sources = Array.Empty<string>();
    }

    [Serializable]
    public sealed class OriginalCombatOverride
    {
        public string field;
        public int level;
        public int pointer;
        public bool isNumber;
        public double number;
        public string text;
        public string source;
    }

    [Serializable]
    public sealed class OriginalCombatHandler
    {
        public string name;
        public int startLine;
        public int endLine;
    }

    // Field resolution is deliberately separate from an engine default profile.
    // A sparse optimized SLK cell is not evidence for a zero or previous level.
    [Serializable]
    public sealed class OriginalCombatDefinition
    {
        public string id;
        public string name;
        public OriginalCombatField[] fields = Array.Empty<OriginalCombatField>();
        public OriginalCombatOverride[] overrides = Array.Empty<OriginalCombatOverride>();
        public OriginalCombatHandler[] handlers = Array.Empty<OriginalCombatHandler>();

        public bool TryNumber(string key, out double value, out string evidence)
        {
            value = 0;
            evidence = null;
            int pointer = 0, level = 0;
            string binaryField = null;
            if (key.StartsWith("Data", StringComparison.Ordinal) && key.Length > 5 &&
                key[4] >= 'A' && key[4] <= 'I')
            {
                pointer = key[4] - 'A' + 1;
                int.TryParse(key.Substring(5), NumberStyles.None, CultureInfo.InvariantCulture, out level);
            }
            else
            {
                var prefix = key.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                if (prefix.Length < key.Length)
                    int.TryParse(key.Substring(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out level);
                switch (prefix)
                {
                    case "Cost": binaryField = "amcs"; break;
                    case "Cool": binaryField = "acdn"; break;
                    case "Rng": binaryField = "aran"; break;
                    case "Area": binaryField = "aare"; break;
                    case "Dur": binaryField = "adur"; break;
                    case "HeroDur": binaryField = "ahdu"; break;
                }
            }
            OriginalCombatOverride chosen = null;
            foreach (var row in overrides)
                if (level > 0 && row.level == level &&
                    (pointer > 0 ? row.pointer == pointer : binaryField != null && row.field == binaryField))
                    chosen = row;
            if (chosen != null)
            {
                evidence = chosen.source;
                value = chosen.number;
                return chosen.isNumber && IsFinite(value);
            }
            foreach (var field in fields)
            {
                if (field.key != key) continue;
                value = field.number;
                evidence = string.Join(",", field.sources);
                return field.isNumber && !field.conflict && IsFinite(value);
            }
            return false;
        }

        public double Number(string key)
        {
            if (!TryNumber(key, out var value, out _))
                throw new InvalidOperationException("Unresolved original field " + id + "." + key);
            return value;
        }

        public string Text(string key)
        {
            foreach (var field in fields)
                if (field.key == key && !field.isNumber && !field.conflict) return field.text;
            return null;
        }

        internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    [Serializable]
    public sealed class OriginalHeroDefinition
    {
        public string id;
        public string name;
        public string[] skills = Array.Empty<string>();
        public string[] innate = Array.Empty<string>();
        public int sourceLine;
    }

    [Serializable]
    public sealed class OriginalCombatConstant
    {
        public string key;
        public double[] values = Array.Empty<double>();
        public string source;
    }

    [Serializable]
    public sealed class OriginalCombatObservation
    {
        public string id;
        public string source;
        public string rule;
    }

    [Serializable]
    public sealed class OriginalCombatCatalog
    {
        public int schemaVersion;
        public string version;
        public string sourceSha256;
        public string scriptSha256;
        public OriginalHeroDefinition[] selectedHeroes = Array.Empty<OriginalHeroDefinition>();
        public OriginalCombatDefinition[] units = Array.Empty<OriginalCombatDefinition>();
        public OriginalCombatDefinition[] abilities = Array.Empty<OriginalCombatDefinition>();
        public OriginalCombatConstant[] constants = Array.Empty<OriginalCombatConstant>();
        public OriginalCombatObservation[] observations = Array.Empty<OriginalCombatObservation>();
        public string[] limits = Array.Empty<string>();
        private Dictionary<string, OriginalCombatDefinition> unitIndex, abilityIndex;

        public void BuildIndexes()
        {
            unitIndex = Index(units);
            abilityIndex = Index(abilities);
            var heroes = new HashSet<string>(StringComparer.Ordinal);
            if (selectedHeroes == null || selectedHeroes.Length != 3)
                throw new ArgumentException("Expected the three selected original heroes.");
            foreach (var hero in selectedHeroes)
            {
                if (hero == null || (hero.id != "H008" && hero.id != "N0A0" && hero.id != "H024") ||
                    !heroes.Add(hero.id) || !unitIndex.ContainsKey(hero.id) || hero.skills == null)
                    throw new ArgumentException("Invalid selected original hero.");
                foreach (var skill in hero.skills)
                    if (!abilityIndex.ContainsKey(skill)) throw new ArgumentException("Missing hero skill " + skill);
            }
        }

        private static Dictionary<string, OriginalCombatDefinition> Index(OriginalCombatDefinition[] records)
        {
            var result = new Dictionary<string, OriginalCombatDefinition>(StringComparer.Ordinal);
            foreach (var record in records) result.Add(record.id, record);
            return result;
        }

        public OriginalCombatDefinition Unit(string id)
        {
            if (unitIndex == null) BuildIndexes();
            return id != null && unitIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalCombatDefinition Ability(string id)
        {
            if (abilityIndex == null) BuildIndexes();
            return id != null && abilityIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalHeroDefinition Hero(string id)
        {
            foreach (var hero in selectedHeroes) if (hero.id == id) return hero;
            return null;
        }

        public double Constant(string key, int index = 0)
        {
            foreach (var constant in constants)
                if (constant.key == key && index >= 0 && index < constant.values.Length)
                    return constant.values[index];
            throw new InvalidOperationException("Unresolved original constant " + key);
        }
    }
}

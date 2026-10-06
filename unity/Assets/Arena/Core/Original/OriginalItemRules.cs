using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalItemRuleValue
    {
        public string field;
        public bool known;
        public int value;
        public string state;
        public string[] sources = Array.Empty<string>();
    }

    // Freeze each rawcode independently. The native catalog has already resolved
    // map declarations and proven inheritance; candidates and dataset conflicts
    // remain unknown here, even if the older flat item catalog has a value.
    public sealed class OriginalItemRules
    {
        static readonly string[] Fields = { "goldcost", "lumbercost", "uses", "usable", "perishable", "powerup",
            "sellable", "pawnable", "droppable", "stockStart", "stockRegen", "stockMax" };
        static readonly HashSet<string> Flags = new HashSet<string>(StringComparer.Ordinal)
            { "usable", "perishable", "powerup", "sellable", "pawnable", "droppable" };
        readonly OriginalItemCatalog itemCatalog;
        readonly Dictionary<string, Dictionary<string, OriginalItemRuleValue>> values =
            new Dictionary<string, Dictionary<string, OriginalItemRuleValue>>(StringComparer.Ordinal);
        public int ItemCount => values.Count;

        public OriginalItemRules(OriginalItemCatalog items, OriginalNativeCatalog native, OriginalObservedItemCatalog observed = null)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (native == null) throw new ArgumentNullException(nameof(native));
            if (items.mapSha256 != OriginalNativeCatalog.ExpectedMapSha256 || items.mapSha256 != native.mapSha256)
                throw new ArgumentException("Item and native catalogs must describe the same verified map.");
            items.BuildIndexes(); native.BuildIndexes();
            if (observed != null)
            {
                observed.BuildIndexes();
                if (items.items.Length != observed.items.Length) throw new ArgumentException("Item observation coverage differs from the source catalog.");
            }
            var sources = new Dictionary<int, OriginalNativeSource>();
            foreach (var source in native.sources) sources.Add(source.id, source);
            foreach (var item in items.items)
            {
                var row = new Dictionary<string, OriginalItemRuleValue>(StringComparer.Ordinal);
                foreach (var field in Fields)
                {
                    var declaration = native.ItemField(item.id, field);
                    var resolved = new OriginalItemRuleValue { field = field, known = declaration.known, state = declaration.state };
                    if (declaration.known)
                    {
                        double number = declaration.Require();
                        if (number < 0 || number > int.MaxValue || Math.Truncate(number) != number ||
                            (Flags.Contains(field) && number > 1))
                            throw new ArgumentException("Invalid native item integer: " + item.id + "." + field);
                        resolved.value = (int)number;
                    }
                    if (declaration.sources != null)
                    {
                        var origins = new List<string>();
                        foreach (var origin in declaration.sources)
                        {
                            var source = sources[origin.source];
                            origins.Add(source.archive + ":" + source.entry + ":line=" + origin.line +
                                ":offset=" + origin.offset + ":valueOffset=" + origin.valueOffset +
                                ":sha256=" + source.sha256 + ":variant=" + (origin.variant ?? ""));
                        }
                        resolved.sources = origins.ToArray();
                    }
                    if (observed != null)
                    {
                        var measurement = observed.Field(item.id, field);
                        if (measurement.known)
                        {
                            if (resolved.known && resolved.value != measurement.value)
                                throw new ArgumentException("Native declaration/measurement conflict: " + item.id + "." + field);
                            var origins = new List<string>(resolved.sources); origins.AddRange(measurement.sources);
                            resolved.known = true; resolved.value = measurement.value;
                            resolved.state = measurement.state; resolved.sources = origins.ToArray();
                        }
                    }
                    row.Add(field, resolved);
                }
                values.Add(item.id, row);
            }
            itemCatalog = items;
        }

        public OriginalItemRuleValue Field(string itemId, string field)
        {
            if (itemId == null || !values.TryGetValue(itemId, out var row)) throw new ArgumentException("Undefined item: " + itemId);
            if (field == null || !row.TryGetValue(field, out var value)) throw new ArgumentException("Unsupported item rule: " + field);
            return new OriginalItemRuleValue { field = value.field, known = value.known, value = value.value,
                state = value.state, sources = (string[])value.sources.Clone() };
        }

        internal bool IsFor(OriginalItemCatalog catalog) => ReferenceEquals(itemCatalog, catalog);

        internal OriginalDeclaredInt Declared(string itemId, string field)
        {
            var value = Field(itemId, field);
            return new OriginalDeclaredInt { known = value.known, value = value.value, sources = value.sources };
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    // LiAItemFam3 tests native AIsr20/25 and A0E5=40 in both addition
    // orders. The last added source wins; removing it restores its predecessor.
    // Other source histories are an explicit application of that observed rule.
    public sealed class OriginalSpellResistanceLedger
    {
        sealed class Entry { internal string source; internal double reduction; }
        readonly Dictionary<int, List<Entry>> actors = new Dictionary<int, List<Entry>>();

        public void Set(int actorId, string source, double reduction)
        {
            Validate(actorId, source);
            if (double.IsNaN(reduction) || double.IsInfinity(reduction) || reduction < 0 || reduction > 1)
                throw new ArgumentException("Invalid native spell resistance.");
            if (!actors.TryGetValue(actorId, out var entries))
                actors.Add(actorId, entries = new List<Entry>());
            var existing = entries.Find(e => e.source == source);
            if (existing != null)
            {
                // Repeated ability upkeep is not a new native addition.
                if (existing.reduction != reduction)
                    throw new InvalidOperationException("Remove an existing resistance source before changing its value.");
                return;
            }
            entries.Add(new Entry { source = source, reduction = reduction });
        }

        public void Remove(int actorId, string source)
        {
            Validate(actorId, source);
            if (!actors.TryGetValue(actorId, out var entries)) return;
            entries.RemoveAll(e => e.source == source);
            if (entries.Count == 0) actors.Remove(actorId);
        }

        public double Multiplier(int actorId)
        {
            if (actorId <= 0) throw new ArgumentException("Invalid resistance actor.");
            return actors.TryGetValue(actorId, out var entries) ? 1 - entries[entries.Count - 1].reduction : 1;
        }

        public OriginalSpellResistanceLedger Copy()
        {
            var copy = new OriginalSpellResistanceLedger();
            foreach (var actor in actors)
                foreach (var entry in actor.Value) copy.Set(actor.Key, entry.source, entry.reduction);
            return copy;
        }

        public void ReplaceActorFrom(int actorId, OriginalSpellResistanceLedger candidate)
        {
            if (actorId <= 0 || candidate == null) throw new ArgumentException("Invalid resistance candidate.");
            var copy = new List<Entry>();
            if (candidate.actors.TryGetValue(actorId, out var entries))
                foreach (var entry in entries) copy.Add(new Entry { source = entry.source, reduction = entry.reduction });
            // A multi-recipient pickup stages all players before publishing.
            // Commit only this actor so another staged actor cannot overwrite
            // an earlier recipient's successful transaction.
            if (copy.Count == 0) actors.Remove(actorId); else actors[actorId] = copy;
        }

        static void Validate(int actorId, string source)
        {
            if (actorId <= 0 || string.IsNullOrEmpty(source) || source.Length > 128)
                throw new ArgumentException("Invalid resistance source identity.");
        }
    }
}

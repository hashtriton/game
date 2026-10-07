using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>One stop of a buying guide: what to have by then, in the order to buy it.</summary>
    [Serializable]
    public sealed class GuideStep
    {
        public string when;
        public string note;
        /// <summary>Finished items by id; an id repeated means that many copies.</summary>
        public string[] items = Array.Empty<string>();
    }

    [Serializable]
    public sealed class GuideDefinition
    {
        public string id;
        public string title;
        public string role;
        public string summary;
        public string source;
        public GuideStep[] steps = Array.Empty<GuideStep>();
    }

    [Serializable]
    public sealed class GuideFile
    {
        public int schemaVersion;
        public GuideDefinition[] guides = Array.Empty<GuideDefinition>();
    }

    public enum ItemMark { Todo, Next, Done }

    public static class GuideProgress
    {
        /// <summary>
        /// Marks every item of a guide: done when the bag holds it or something built from it, the first one still
        /// missing is the next one to buy. A held finished item counts for one copy of each part it is built from.
        /// </summary>
        public static ItemMark[][] Evaluate(ItemBook book, IReadOnlyList<string> owned, GuideDefinition guide)
        {
            var have = new Dictionary<string, int>();
            foreach (var id in owned)
            {
                have.TryGetValue(id, out var count);
                have[id] = count + 1;
            }

            var absorbed = new List<HashSet<string>>();
            foreach (var _ in owned) absorbed.Add(new HashSet<string>());

            var used = new Dictionary<string, int>();
            var marks = new ItemMark[guide.steps.Length][];
            var nextFound = false;
            for (var s = 0; s < guide.steps.Length; s++)
            {
                var items = guide.steps[s].items;
                marks[s] = new ItemMark[items.Length];
                for (var i = 0; i < items.Length; i++)
                {
                    var id = items[i];
                    used.TryGetValue(id, out var taken);

                    var done = false;
                    if (have.TryGetValue(id, out var held) && held > taken)
                    {
                        used[id] = taken + 1;
                        done = true;
                    }
                    else
                    {
                        for (var o = 0; o < owned.Count && !done; o++)
                        {
                            if (owned[o] == id || absorbed[o].Contains(id) || !BuiltFrom(book, owned[o], id, 0)) continue;
                            absorbed[o].Add(id);
                            done = true;
                        }
                    }

                    if (done) marks[s][i] = ItemMark.Done;
                    else if (!nextFound)
                    {
                        marks[s][i] = ItemMark.Next;
                        nextFound = true;
                    }
                    else marks[s][i] = ItemMark.Todo;
                }
            }
            return marks;
        }

        /// <summary>True when the finished item <paramref name="made"/> is built, at any depth and by any of its recipes, from <paramref name="part"/>.</summary>
        public static bool BuiltFrom(ItemBook book, string made, string part, int depth)
        {
            if (depth > 8) return false;
            foreach (var recipe in book.RecipesFor(made))
            {
                foreach (var ingredient in recipe.ingredients)
                {
                    if (ingredient.itemId == part) return true;
                    if (!book.IsScroll(ingredient.itemId) && BuiltFrom(book, ingredient.itemId, part, depth + 1)) return true;
                }
            }
            return false;
        }
    }
}

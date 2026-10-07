using System.Collections.Generic;
using Arena.Original;

namespace Game
{
    /// <summary>The purchases that turn what the hero carries into a finished item.</summary>
    public sealed class PurchasePlan
    {
        public string targetId;
        /// <summary>In buying order: parts first, the recipe scroll last, which completes the item on pickup.</summary>
        public readonly List<ShopEntry> steps = new List<ShopEntry>();
        public int gold;
        public int souls;
        public bool alreadyOwned;
        /// <summary>Some part is not sold anywhere or has no price in the data, so the plan cannot be carried out.</summary>
        public string problem;

        public bool IsPossible => problem == null && !alreadyOwned && steps.Count > 0;
    }

    /// <summary>
    /// Works out what to buy for an item the way Dota's quick buy does: parts already in the bag count,
    /// the rest is bought part by part, then the recipe. When an item has several recipes the cheapest one wins.
    /// </summary>
    public static class ShoppingPlanner
    {
        private sealed class Trial
        {
            public Dictionary<string, int> have = new Dictionary<string, int>();
            public List<ShopEntry> steps = new List<ShopEntry>();
            public int gold;
            public int souls;
            public string problem;

            public Trial Clone() => new Trial
            {
                have = new Dictionary<string, int>(have),
                steps = new List<ShopEntry>(steps),
                gold = gold,
                souls = souls
            };

            // Souls weigh more than gold when two variants are compared: there are far fewer of them.
            public int Weight => gold + souls * 100;
        }

        /// <summary>
        /// <paramref name="another"/> plans a further copy of an item the bag already holds instead of reporting it as owned.
        /// </summary>
        public static PurchasePlan Plan(ItemBook book, IEnumerable<OriginalItemInstance> owned, string finalId, bool another = false)
        {
            var plan = new PurchasePlan { targetId = finalId };
            var trial = new Trial();
            foreach (var item in owned)
            {
                if (item == null || item.removed) continue;
                trial.have.TryGetValue(item.itemId, out var count);
                trial.have[item.itemId] = count + 1;
            }

            if (trial.have.TryGetValue(finalId, out var already) && already > 0)
            {
                if (!another)
                {
                    plan.alreadyOwned = true;
                    return plan;
                }
                // The copy in the bag is not material for the new one.
                trial.have.Remove(finalId);
            }

            if (Collect(book, finalId, trial, 0))
            {
                plan.steps.AddRange(trial.steps);
                plan.gold = trial.gold;
                plan.souls = trial.souls;
            }
            else plan.problem = trial.problem ?? "Нельзя собрать";
            return plan;
        }

        private static bool Collect(ItemBook book, string id, Trial trial, int depth)
        {
            if (trial.have.TryGetValue(id, out var count) && count > 0)
            {
                trial.have[id] = count - 1;
                return true;
            }

            var recipes = depth < 8 ? book.RecipesFor(id) : null;
            if (recipes == null || recipes.Count == 0) return Buy(book, id, trial);
            if (recipes.Count == 1) return Apply(book, recipes[0], trial, depth);

            Trial best = null;
            string lastProblem = null;
            foreach (var recipe in recipes)
            {
                var attempt = trial.Clone();
                if (Apply(book, recipe, attempt, depth))
                {
                    if (best == null || attempt.Weight < best.Weight) best = attempt;
                }
                else if (attempt.problem != null) lastProblem = attempt.problem;
            }
            if (best == null)
            {
                trial.problem = lastProblem;
                return false;
            }
            trial.have = best.have;
            trial.steps = best.steps;
            trial.gold = best.gold;
            trial.souls = best.souls;
            return true;
        }

        private static bool Apply(ItemBook book, OriginalItemRecipe recipe, Trial trial, int depth)
        {
            string scroll = null;
            foreach (var ingredient in recipe.ingredients)
            {
                if (book.IsScroll(ingredient.itemId))
                {
                    scroll = ingredient.itemId;
                    continue;
                }
                for (var i = 0; i < ingredient.count; i++)
                    if (!Collect(book, ingredient.itemId, trial, depth + 1)) return false;
            }

            if (scroll == null) return true;
            if (trial.have.TryGetValue(scroll, out var scrolls) && scrolls > 0)
            {
                trial.have[scroll] = scrolls - 1;
                return true;
            }
            return Buy(book, scroll, trial);
        }

        private static bool Buy(ItemBook book, string id, Trial trial)
        {
            var entry = book.EntryFor(id);
            if (entry == null)
            {
                trial.problem = "«" + book.Name(id) + "» не продаётся в лавках";
                return false;
            }
            if (!entry.PriceKnown)
            {
                trial.problem = "Цена «" + book.Name(id) + "» не определена в данных карты";
                return false;
            }
            trial.steps.Add(entry);
            trial.gold += entry.gold;
            trial.souls += entry.souls;
            return true;
        }
    }
}

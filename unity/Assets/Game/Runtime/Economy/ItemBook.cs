using System;
using System.Collections.Generic;
using System.Text;
using Arena;
using Arena.Original;

namespace Game
{
    /// <summary>One thing a shop sells: what the shelf shows and what buying it costs.</summary>
    public sealed class ShopEntry
    {
        public string shopId;
        public string offerId;
        /// <summary>The object the shop hands over (a recipe scroll for most upgraded items).</summary>
        public string itemId;
        /// <summary>The item the icon stands for: the finished item for a scroll, otherwise the item itself.</summary>
        public string displayId;
        public int gold = -1;
        public int souls = -1;
        public bool PriceKnown => gold >= 0 && souls >= 0;
    }

    public sealed class ShopTab
    {
        public string id;
        public string title;
        public readonly List<ShopEntry> entries = new List<ShopEntry>();
    }

    /// <summary>
    /// Read-only view of the item data the shops and the guides need: names, prices, recipes and what each shelf sells.
    /// Built over the catalogs of the original map; nothing here changes state.
    /// </summary>
    public sealed class ItemBook
    {
        // Shops of the standard layout, grouped into shelves by what they sell. The shelf names are our own.
        private static readonly (string id, string title, string[] shops)[] TabLayout =
        {
            ("base", "Базовые", new[] { "n05V", "n05W", "n05X" }),
            ("arms", "Оружие", new[] { "n05P" }),
            ("relics", "Артефакты", new[] { "n05Q" }),
            ("staves", "Посохи и маски", new[] { "n05R" }),
            ("blades", "Мечи и луки", new[] { "n05S" }),
            ("boots", "Сапоги", new[] { "n0AF" }),
            ("armor", "Броня и щиты", new[] { "n05T" }),
            ("special", "Особые", new[] { "n05U" }),
            ("heirlooms", "Реликвии", new[] { "n06V" }),
            ("potions", "Зелья и свитки", new[] { "n05Y" }),
            ("summons", "Призыв", new[] { "n05Z" }),
            ("acolyte", "Послушник", new[] { "u00E" }),
        };

        private readonly Dictionary<string, ShopEntry> entryByItem = new Dictionary<string, ShopEntry>(StringComparer.Ordinal);
        private static readonly IReadOnlyList<OriginalItemRecipe> NoRecipes = new OriginalItemRecipe[0];
        private readonly Dictionary<string, List<OriginalItemRecipe>> recipesByResult = new Dictionary<string, List<OriginalItemRecipe>>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> scrollResult = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> nameCache = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<ShopTab> tabs = new List<ShopTab>();

        public OriginalGameCatalogs Catalogs { get; }
        public OriginalItemCatalog Items => Catalogs.Items;
        public OriginalItemRules Rules { get; }
        public OriginalItemText Text { get; }
        public OriginalInventoryEffects Effects { get; }
        public OriginalItemEffectText EffectText { get; }
        public IReadOnlyList<ShopTab> Tabs => tabs;

        public ItemBook(OriginalGameCatalogs catalogs)
        {
            Catalogs = catalogs ?? throw new ArgumentNullException(nameof(catalogs));
            Rules = new OriginalItemRules(catalogs.Items, catalogs.Native, catalogs.ObservedItems);
            Text = new OriginalItemText(catalogs);
            Effects = new OriginalInventoryEffects(catalogs.Passives,
                new OriginalItemUseRules(catalogs.Items, catalogs.Combat, catalogs.ObservedItems.itemUse));
            EffectText = new OriginalItemEffectText(catalogs.Items, catalogs.Combat, Effects);

            // Several recipes may share a result (the boots of Space have one per attribute): all are kept.
            foreach (var recipe in Items.recipes)
            {
                if (!recipesByResult.TryGetValue(recipe.resultId, out var variants))
                    recipesByResult.Add(recipe.resultId, variants = new List<OriginalItemRecipe>());
                variants.Add(recipe);
                foreach (var ingredient in recipe.ingredients)
                {
                    var definition = Items.Item(ingredient.itemId);
                    if (definition != null && definition.classId == "Charged" && !scrollResult.ContainsKey(ingredient.itemId))
                        scrollResult.Add(ingredient.itemId, recipe.resultId);
                }
            }

            foreach (var layout in TabLayout)
            {
                var tab = new ShopTab { id = layout.id, title = layout.title };
                foreach (var shopId in layout.shops)
                {
                    var shop = Items.Shop(shopId);
                    if (shop == null) continue;
                    foreach (var offerId in shop.offerIds)
                    {
                        var itemId = Items.FromWorld(offerId)?.inventoryId ?? offerId;
                        if (entryByItem.ContainsKey(itemId)) continue;
                        var entry = MakeEntry(shopId, offerId, itemId);
                        entryByItem.Add(itemId, entry);
                        tab.entries.Add(entry);
                    }
                }
                tabs.Add(tab);
            }
        }

        private ShopEntry MakeEntry(string shopId, string offerId, string itemId)
        {
            var gold = Rules.Field(offerId, "goldcost");
            var souls = Rules.Field(offerId, "lumbercost");
            return new ShopEntry
            {
                shopId = shopId,
                offerId = offerId,
                itemId = itemId,
                displayId = scrollResult.TryGetValue(itemId, out var result) ? result : itemId,
                gold = gold.known ? gold.value : -1,
                // The inventory rules refuse a purchase whose soul price is not known, so it counts as unknown here too.
                souls = souls.known ? souls.value : -1
            };
        }

        // ---- lookups ---------------------------------------------------------------------------------------

        public ShopEntry EntryFor(string itemId) => itemId != null && entryByItem.TryGetValue(itemId, out var entry) ? entry : null;

        /// <summary>The first recipe of an item (the one shown in descriptions); null for items that are not built.</summary>
        public OriginalItemRecipe RecipeFor(string resultId) => RecipesFor(resultId).Count > 0 ? RecipesFor(resultId)[0] : null;

        /// <summary>Every recipe that makes the item; most items have one.</summary>
        public IReadOnlyList<OriginalItemRecipe> RecipesFor(string resultId) =>
            resultId != null && recipesByResult.TryGetValue(resultId, out var variants) ? variants : NoRecipes;

        public bool IsScroll(string itemId) => itemId != null && scrollResult.ContainsKey(itemId);

        /// <summary>The finished item a recipe scroll makes; the id itself for anything else.</summary>
        public string ResultOfScroll(string itemId) => scrollResult.TryGetValue(itemId ?? "", out var result) ? result : itemId;

        public string ScrollFor(string resultId)
        {
            var recipe = RecipeFor(resultId);
            if (recipe == null) return null;
            foreach (var ingredient in recipe.ingredients)
                if (IsScroll(ingredient.itemId)) return ingredient.itemId;
            return null;
        }

        /// <summary>Everything a finished item is built from, scroll excluded, with repeats.</summary>
        public List<string> ComponentsOf(string resultId)
        {
            var parts = new List<string>();
            var recipe = RecipeFor(resultId);
            if (recipe == null) return parts;
            foreach (var ingredient in recipe.ingredients)
            {
                if (IsScroll(ingredient.itemId)) continue;
                for (var i = 0; i < ingredient.count; i++) parts.Add(ingredient.itemId);
            }
            return parts;
        }

        /// <summary>Every finished item that needs this one as a direct ingredient.</summary>
        public List<string> UsedIn(string itemId)
        {
            var uses = new List<string>();
            foreach (var recipe in Items.recipes)
            {
                if (uses.Contains(recipe.resultId)) continue;
                foreach (var ingredient in recipe.ingredients)
                {
                    if (ingredient.itemId != itemId) continue;
                    uses.Add(recipe.resultId);
                    break;
                }
            }
            return uses;
        }

        public string Name(string itemId)
        {
            if (itemId == null) return "";
            if (nameCache.TryGetValue(itemId, out var cached)) return cached;
            var definition = Items.Item(itemId);
            return nameCache[itemId] = definition == null ? itemId : Homoglyphs.Clean(definition.displayName);
        }

        /// <summary>What the finished item is worth in gold, counting its components and its recipe.</summary>
        public int TotalCost(string itemId)
        {
            var row = Items.QuickBuy(itemId);
            if (row != null && row.scriptGoldValue > 0) return row.scriptGoldValue;
            var gold = Rules.Field(itemId, "goldcost");
            return gold.known ? gold.value : -1;
        }

        public int ItemGold(string itemId)
        {
            var gold = Rules.Field(itemId, "goldcost");
            return gold.known ? gold.value : -1;
        }
    }

    /// <summary>
    /// Item names of the original are typed with Latin letters that look like Cyrillic ones, which makes them
    /// unsearchable and ugly in a font that treats the scripts differently. This swaps them for the real letters.
    /// </summary>
    public static class Homoglyphs
    {
        private const string Latin = "aeopcxyABCEHKMOPTXY";
        private const string Cyrillic = "аеорсхуАВСЕНКМОРТХУ";

        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text) || !HasCyrillic(text)) return text?.Trim() ?? "";
            var builder = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                var index = Latin.IndexOf(c);
                builder.Append(index >= 0 ? Cyrillic[index] : c);
            }
            return builder.ToString().Trim();
        }

        private static bool HasCyrillic(string text)
        {
            foreach (var c in text)
                if (c >= 'А' && c <= 'я' || c == 'ё' || c == 'Ё') return true;
            return false;
        }
    }
}

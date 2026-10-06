using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalDeclaredInt
    {
        public bool known;
        public int value;
        public string[] sources = Array.Empty<string>();

        public static OriginalDeclaredInt Known(int value)
        {
            return new OriginalDeclaredInt { known = true, value = value };
        }
    }

    [Serializable]
    public sealed class OriginalItemDefinition
    {
        public string id;
        public string displayName;
        public string classId;
        public string cooldownId;
        public string effectsStatus;
        public bool connectedToShopOrRecipe;
        public string[] abilityIds = Array.Empty<string>();
        public OriginalDeclaredInt goldCost = new OriginalDeclaredInt();
        public OriginalDeclaredInt lumberCost = new OriginalDeclaredInt();
        public OriginalDeclaredInt initialCharges = new OriginalDeclaredInt();
        public OriginalDeclaredInt usable = new OriginalDeclaredInt();
        public OriginalDeclaredInt perishable = new OriginalDeclaredInt();
        public OriginalDeclaredInt pawnable = new OriginalDeclaredInt();
        public OriginalDeclaredInt droppable = new OriginalDeclaredInt();
        public OriginalDeclaredInt sellable = new OriginalDeclaredInt();
        public OriginalDeclaredInt stockMax = new OriginalDeclaredInt();
        public OriginalDeclaredInt stockStart = new OriginalDeclaredInt();
        public OriginalDeclaredInt stockRegen = new OriginalDeclaredInt();
    }

    [Serializable]
    public sealed class OriginalShopDefinition
    {
        public string unitId;
        public string displayName;
        public string[] offerIds = Array.Empty<string>();
        public string[] sources = Array.Empty<string>();
    }

    [Serializable]
    public sealed class OriginalItemConversion
    {
        public string inventoryId;
        public string worldShopId;
        public int stackChargeCap;
        public int sourceLine;
    }

    [Serializable]
    public sealed class OriginalIngredient
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public sealed class OriginalItemRecipe
    {
        public int registrationIndex;
        public string resultId;
        public OriginalIngredient[] ingredients = Array.Empty<OriginalIngredient>();
        public int sourceLine;
    }

    [Serializable]
    public sealed class OriginalQuickBuyEntry
    {
        public string itemId;
        public int scriptGoldValue;
        public string linkedResultId;
        public string[] ingredientSlots = Array.Empty<string>();
        public int sourceLine;
    }

    [Serializable]
    public sealed class OriginalItemCoverage
    {
        public int declaredItems;
        public int implementedItemEffects;
        public int directItemAbilities;
        public bool pawnGoldFactorResolved;
        public bool engineDefaultsResolved;
        public bool runtimeBaselineObserved;
    }

    [Serializable]
    public sealed class OriginalItemCatalog
    {
        public int schemaVersion;
        public string sourceVersion;
        public string mapSha256;
        public string evidenceScope;
        public OriginalItemDefinition[] items = Array.Empty<OriginalItemDefinition>();
        public OriginalShopDefinition[] shops = Array.Empty<OriginalShopDefinition>();
        public OriginalItemConversion[] conversions = Array.Empty<OriginalItemConversion>();
        public OriginalItemRecipe[] recipes = Array.Empty<OriginalItemRecipe>();
        public OriginalQuickBuyEntry[] quickBuy = Array.Empty<OriginalQuickBuyEntry>();
        public OriginalItemCoverage coverage = new OriginalItemCoverage();

        private Dictionary<string, OriginalItemDefinition> itemIndex;
        private Dictionary<string, OriginalShopDefinition> shopIndex;
        private Dictionary<string, OriginalItemConversion> worldIndex;
        private Dictionary<string, OriginalItemConversion> inventoryIndex;
        private Dictionary<string, OriginalQuickBuyEntry> quickIndex;

        public void BuildIndexes()
        {
            itemIndex = new Dictionary<string, OriginalItemDefinition>(StringComparer.Ordinal);
            shopIndex = new Dictionary<string, OriginalShopDefinition>(StringComparer.Ordinal);
            worldIndex = new Dictionary<string, OriginalItemConversion>(StringComparer.Ordinal);
            inventoryIndex = new Dictionary<string, OriginalItemConversion>(StringComparer.Ordinal);
            quickIndex = new Dictionary<string, OriginalQuickBuyEntry>(StringComparer.Ordinal);
            foreach (var item in items) itemIndex.Add(item.id, item);
            foreach (var shop in shops)
            {
                shopIndex.Add(shop.unitId, shop);
                foreach (var id in shop.offerIds) RequireItem(id);
            }
            foreach (var row in conversions)
            {
                RequireItem(row.inventoryId);
                RequireItem(row.worldShopId);
                if (row.stackChargeCap < 0) throw new ArgumentException("Negative charge cap.");
                worldIndex.Add(row.worldShopId, row);
                inventoryIndex.Add(row.inventoryId, row);
            }
            var previousOrder = -1;
            foreach (var recipe in recipes)
            {
                if (recipe.registrationIndex <= previousOrder) throw new ArgumentException("Recipe order must match JASS registration order.");
                previousOrder = recipe.registrationIndex;
                RequireItem(recipe.resultId);
                foreach (var ingredient in recipe.ingredients)
                {
                    RequireItem(ingredient.itemId);
                    if (ingredient.count <= 0) throw new ArgumentException("Invalid ingredient count.");
                }
            }
            // WL writes the ID lookup on every registration; later duplicates win.
            foreach (var row in quickBuy) quickIndex[row.itemId] = row;
        }

        private void RequireItem(string id)
        {
            if (!itemIndex.ContainsKey(id)) throw new ArgumentException("Undefined item: " + id);
        }

        private void EnsureIndexes()
        {
            if (itemIndex == null) BuildIndexes();
        }

        public OriginalItemDefinition Item(string id)
        {
            EnsureIndexes();
            return id != null && itemIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalShopDefinition Shop(string id)
        {
            EnsureIndexes();
            return id != null && shopIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalItemConversion FromWorld(string id)
        {
            EnsureIndexes();
            return id != null && worldIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalItemConversion FromInventory(string id)
        {
            EnsureIndexes();
            return id != null && inventoryIndex.TryGetValue(id, out var result) ? result : null;
        }

        public OriginalQuickBuyEntry QuickBuy(string id)
        {
            EnsureIndexes();
            return id != null && quickIndex.TryGetValue(id, out var result) ? result : null;
        }
    }
}

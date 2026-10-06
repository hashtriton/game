using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalInventoryTests
    {
        private static OriginalItemCatalog LoadCatalog()
        {
            return JsonUtility.FromJson<OriginalItemCatalog>(File.ReadAllText(
                Path.Combine(Application.dataPath, "Arena/Data/lia39-items.json")));
        }

        private static OriginalItemDefinition Definition(string id, int cost = 0)
        {
            return new OriginalItemDefinition
            {
                id = id, classId = "Permanent", goldCost = OriginalDeclaredInt.Known(cost),
                lumberCost = OriginalDeclaredInt.Known(0), initialCharges = OriginalDeclaredInt.Known(0),
                droppable = OriginalDeclaredInt.Known(1), pawnable = OriginalDeclaredInt.Known(1)
            };
        }

        private static OriginalItemCatalog Fixture()
        {
            return new OriginalItemCatalog
            {
                items = new[] { Definition("base", 20), Definition("stor", 20), Definition("reca", 10),
                    Definition("resu", 50), Definition("next", 80), Definition("noop"), Definition("poti", 6), Definition("potw", 6) },
                conversions = new[]
                {
                    new OriginalItemConversion { inventoryId = "base", worldShopId = "stor" },
                    new OriginalItemConversion { inventoryId = "poti", worldShopId = "potw", stackChargeCap = 4 }
                },
                shops = new[] { new OriginalShopDefinition { unitId = "shop", offerIds = new[] { "stor", "reca", "potw" } } },
                recipes = new[]
                {
                    new OriginalItemRecipe { registrationIndex = 0, resultId = "resu", ingredients = new[]
                    {
                        new OriginalIngredient { itemId = "reca", count = 1 },
                        new OriginalIngredient { itemId = "base", count = 2 }
                    } },
                    new OriginalItemRecipe { registrationIndex = 1, resultId = "next", ingredients = new[]
                    {
                        new OriginalIngredient { itemId = "resu", count = 1 },
                        new OriginalIngredient { itemId = "noop", count = 1 }
                    } }
                },
                quickBuy = new[]
                {
                    new OriginalQuickBuyEntry { itemId = "reca", linkedResultId = "resu", scriptGoldValue = 10,
                        ingredientSlots = new[] { "reca", "base", "base", null, null, null } },
                    new OriginalQuickBuyEntry { itemId = "base", scriptGoldValue = 20 },
                    new OriginalQuickBuyEntry { itemId = "resu", scriptGoldValue = 50 }
                }
            };
        }

        [Test]
        public void ConsumeCharge_RequiresExactInstanceAndPerishableRuleAndRetiresLastCharge()
        {
            var catalog = Fixture(); var potion = Array.Find(catalog.items, item => item.id == "poti");
            potion.usable = OriginalDeclaredInt.Known(1); potion.perishable = OriginalDeclaredInt.Known(1);
            potion.initialCharges = OriginalDeclaredInt.Known(2);
            var inventory = new OriginalInventory(catalog, 1); var item = inventory.CreateInstance("poti");
            inventory.TryPickup(item); var candidate = inventory.Copy();
            Assert.That(candidate.ConsumeCharge(OriginalInventoryBag.Hero, 0, item.instanceId + 1).Code, Is.EqualTo(OriginalItemActionCode.InvalidInstance));
            Assert.That(candidate.HeroSlots[0].charges, Is.EqualTo(2));
            Assert.That(candidate.ConsumeCharge(OriginalInventoryBag.Hero, 0, item.instanceId).Applied, Is.True);
            Assert.That(candidate.HeroSlots[0].charges, Is.EqualTo(1));
            Assert.That(inventory.HeroSlots[0].charges, Is.EqualTo(2));
            Assert.That(candidate.ConsumeCharge(OriginalInventoryBag.Hero, 0, item.instanceId).Applied, Is.True);
            Assert.That(candidate.HeroSlots[0], Is.Null);
            Assert.That(candidate.TryPickup(inventory.HeroSlots[0]).Code, Is.EqualTo(OriginalItemActionCode.InvalidInstance));
            potion.perishable = new OriginalDeclaredInt();
            Assert.That(inventory.ConsumeCharge(OriginalInventoryBag.Hero, 0, item.instanceId).Code, Is.EqualTo(OriginalItemActionCode.UnresolvedRule));
            Assert.That(inventory.HeroSlots[0].charges, Is.EqualTo(2));
        }

        [Test]
        public void Copy_CandidateCraftAndDebitsDoNotMutateOriginalAndKeepIdentityHistory()
        {
            var source = new OriginalInventory(Fixture(), 3, 100, 7) { QuickBuyEnabled = true, HeroItemAffinity = 2 };
            var consumed = source.CreateInstance("potw"); consumed.charges = 2;
            source.TryPickup(consumed);
            var merged = source.CreateInstance("potw"); merged.charges = 2;
            source.TryPickup(merged);
            source.TryPickup(source.CreateInstance("base"));
            source.TryPickup(source.CreateInstance("base"), OriginalInventoryBag.Servant);
            var candidate = source.Copy();
            Assert.That(candidate.QuickBuyEnabled, Is.True); Assert.That(candidate.HeroItemAffinity, Is.EqualTo(2));
            var action = candidate.TryBuy("shop", "reca");
            Assert.That(action.CraftedRecipeIndex, Is.EqualTo(0));
            Assert.That(candidate.Gold, Is.EqualTo(90)); Assert.That(source.Gold, Is.EqualTo(100));
            Assert.That(source.HeroSlots.Any(x => x != null && x.itemId == "base"), Is.True);
            Assert.That(source.ServantSlots.Any(x => x != null && x.itemId == "base"), Is.True);
            Assert.That(candidate.HeroSlots.Any(x => x != null && x.itemId == "base"), Is.False);
            Assert.That(candidate.ServantSlots.All(x => x == null), Is.True);
            candidate.Drop(OriginalInventoryBag.Hero, 0).Item.charges = 999;
            Assert.That(source.HeroSlots[0].charges, Is.EqualTo(4));
            Assert.That(candidate.TryPickup(new OriginalItemInstance { instanceId = merged.instanceId, itemId = "poti", chargesKnown = true }).Code,
                Is.EqualTo(OriginalItemActionCode.InvalidInstance));
            // Rejected candidates do not advance the original identity counter. Publishing a
            // candidate replaces the original inventory; it is not a second authority.
            var before = source.CreateInstance("noop"); var after = candidate.CreateInstance("noop");
            Assert.That(after.instanceId, Is.EqualTo(before.instanceId + 2));
            Assert.That(after.instanceId >> 48, Is.EqualTo(3));
            var committed = candidate.Copy();
            Assert.That(committed.CreateInstance("noop").instanceId, Is.EqualTo(after.instanceId + 1));
            Assert.That(committed.TrySpendResources(91, 0), Is.False);
            Assert.That(candidate.Gold, Is.EqualTo(90)); Assert.That(committed.Gold, Is.EqualTo(90));
        }

        [Test]
        public void SourceCatalog_PreservesCompleteCountsAndUnknownDefaults()
        {
            var catalog = LoadCatalog();
            catalog.BuildIndexes();
            Assert.That(catalog.mapSha256, Is.EqualTo("02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34"));
            Assert.That(catalog.items.Length, Is.EqualTo(440));
            Assert.That(catalog.shops.Length, Is.EqualTo(28));
            Assert.That(catalog.conversions.Length, Is.EqualTo(132));
            Assert.That(catalog.recipes.Length, Is.EqualTo(101));
            Assert.That(catalog.recipes.Select(x => x.resultId).Distinct().Count(), Is.EqualTo(99));
            Assert.That(catalog.quickBuy.Length, Is.EqualTo(360));
            Assert.That(catalog.Item("I000").goldCost.value, Is.EqualTo(65));
            Assert.That(catalog.Item("I000").lumberCost.known, Is.False);
            Assert.That(catalog.Item("I001").abilityIds, Is.EquivalentTo(new[] { "A00B", "A00C" }));
            Assert.That(catalog.coverage.implementedItemEffects, Is.Zero);
            Assert.That(catalog.coverage.runtimeBaselineObserved, Is.False);
        }

        [Test]
        public void Pickup_ConvertsWorldIdAndPreservesOwnershipAndPositiveCharges()
        {
            var inventory = new OriginalInventory(Fixture(), 2);
            var item = inventory.CreateInstance("potw");
            item.charges = 3;
            item.chargesKnown = true;
            Assert.That(inventory.TryPickup(item).Applied, Is.True);
            Assert.That(item.itemId, Is.EqualTo("poti"));
            Assert.That(item.ownerId, Is.EqualTo(2));
            Assert.That(item.charges, Is.EqualTo(3));
            Assert.That(inventory.Drop(OriginalInventoryBag.Hero, 0).Code, Is.EqualTo(OriginalItemActionCode.Grounded));
            Assert.That(item.itemId, Is.EqualTo("potw"));
            Assert.That(item.charges, Is.EqualTo(3));
            Assert.That(inventory.HeroSlots[0], Is.Null);
        }

        [Test]
        public void Pickup_RejectsOtherOwnerUnlessClassExplicitlyBypassesOwnership()
        {
            var catalog = Fixture();
            var inventory = new OriginalInventory(catalog, 1);
            var foreign = inventory.CreateInstance("stor", 2);
            Assert.That(inventory.TryPickup(foreign).Code, Is.EqualTo(OriginalItemActionCode.WrongOwner));
            Assert.That(foreign.itemId, Is.EqualTo("stor"));
            catalog.Item("stor").classId = "PowerUp";
            Assert.That(inventory.TryPickup(foreign).Applied, Is.True);
            Assert.That(foreign.ownerId, Is.EqualTo(1));
        }

        [Test]
        public void ChargedStacks_RequireWholeMergeAndDoNotAllowConsumedInstanceReuse()
        {
            var inventory = new OriginalInventory(Fixture(), 1);
            var first = inventory.CreateInstance("potw"); first.charges = 3;
            var second = inventory.CreateInstance("potw"); second.charges = 2;
            inventory.TryPickup(first);
            inventory.TryPickup(second);
            Assert.That(inventory.HeroSlots.Count(x => x != null), Is.EqualTo(2));
            Assert.That(first.charges, Is.EqualTo(3));
            var third = inventory.CreateInstance("potw"); third.charges = 1;
            inventory.TryPickup(third);
            Assert.That(first.charges, Is.EqualTo(4));
            Assert.That(third.removed, Is.True);
            Assert.That(inventory.TryPickup(third).Code, Is.EqualTo(OriginalItemActionCode.InvalidInstance));
        }

        [Test]
        public void AutoCraft_ConsumesRepeatedIngredientsAcrossBagsAndDoesNotInventRecursiveCraft()
        {
            var inventory = new OriginalInventory(Fixture(), 1);
            var first = inventory.CreateInstance("base");
            var second = inventory.CreateInstance("base");
            inventory.TryPickup(first);
            inventory.TryPickup(second, OriginalInventoryBag.Servant);
            inventory.TryPickup(inventory.CreateInstance("noop"));
            var recipe = inventory.CreateInstance("reca");
            var result = inventory.TryPickup(recipe);
            Assert.That(result.CraftedRecipeIndex, Is.Zero);
            Assert.That(result.Item.itemId, Is.EqualTo("resu"));
            Assert.That(first.removed && second.removed && recipe.removed, Is.True);
            Assert.That(inventory.ServantSlots.All(x => x == null), Is.True);
            Assert.That(inventory.HeroSlots.Any(x => x != null && x.itemId == "next"), Is.False);
            Assert.That(inventory.HeroSlots.Any(x => x != null && x.itemId == "noop"), Is.True);
        }

        [Test]
        public void AutoCraft_MissingDuplicateDoesNotConsumeAnyIngredients()
        {
            var inventory = new OriginalInventory(Fixture(), 1);
            var first = inventory.CreateInstance("base"); inventory.TryPickup(first);
            var recipe = inventory.CreateInstance("reca");
            var result = inventory.TryPickup(recipe);
            Assert.That(result.CraftedRecipeIndex, Is.EqualTo(-1));
            Assert.That(first.removed, Is.False);
            Assert.That(recipe.removed, Is.False);
        }

        [Test]
        public void FullHeroUsesServant_AndBothFullLeaveOwnedWorldRepresentation()
        {
            var inventory = new OriginalInventory(Fixture(), 3);
            for (var n = 0; n < 12; n++) inventory.TryPickup(inventory.CreateInstance("noop"));
            Assert.That(inventory.HeroSlots.Count(x => x != null), Is.EqualTo(6));
            Assert.That(inventory.ServantSlots.Count(x => x != null), Is.EqualTo(6));
            var result = inventory.TryPickup(inventory.CreateInstance("stor"));
            Assert.That(result.Code, Is.EqualTo(OriginalItemActionCode.Grounded));
            Assert.That(result.Item.itemId, Is.EqualTo("stor"));
            Assert.That(result.Item.ownerId, Is.EqualTo(3));
        }

        [Test]
        public void Buy_ValidatesOfferAndBothCurrenciesBeforeMutating()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 30);
            Assert.That(inventory.TryBuy("shop", "noop").Code, Is.EqualTo(OriginalItemActionCode.NotSoldHere));
            Assert.That(inventory.TryBuy("shop", "stor").Code, Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(inventory.Gold, Is.EqualTo(10));
            Assert.That(inventory.TryBuy("shop", "stor").Code, Is.EqualTo(OriginalItemActionCode.NotEnoughResources));
            Assert.That(inventory.HeroSlots.Count(x => x != null), Is.EqualTo(1));
        }

        [Test]
        public void Buy_UnknownSourceLumberCostDoesNotBecomeZero()
        {
            var inventory = new OriginalInventory(LoadCatalog(), 1, 1000, 1000);
            Assert.That(inventory.TryBuy("n004", "I04J").Code, Is.EqualTo(OriginalItemActionCode.UnresolvedRule));
            Assert.That(inventory.Gold, Is.EqualTo(1000));
            Assert.That(inventory.HeroSlots.All(x => x == null), Is.True);
        }

        [Test]
        public void QuickBuy_UsesScriptPricesAndOnlyActingInventory()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 40);
            inventory.TryPickup(inventory.CreateInstance("base"), OriginalInventoryBag.Servant);
            Assert.That(inventory.QuoteQuickBuy("reca").Code, Is.EqualTo(OriginalItemActionCode.NoRecipe));
            inventory.TryPickup(inventory.CreateInstance("reca"));
            var quote = inventory.QuoteQuickBuy("reca");
            Assert.That(quote.GoldCost, Is.EqualTo(40));
            Assert.That(inventory.TryQuickBuy("reca").Code, Is.EqualTo(OriginalItemActionCode.NotAllowed));
            inventory.QuickBuyEnabled = true;
            Assert.That(inventory.TryQuickBuy("reca").Applied, Is.True);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(inventory.ServantSlots[0].itemId, Is.EqualTo("base"));
        }

        [Test]
        public void QuickBuy_RetainsDocumentedRepeatedLookupQuirk()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 10) { QuickBuyEnabled = true };
            inventory.TryPickup(inventory.CreateInstance("base"));
            var quote = inventory.QuoteQuickBuy("reca");
            Assert.That(quote.HasRepeatedIngredientLookup, Is.True);
            Assert.That(quote.GoldCost, Is.EqualTo(10));
            Assert.That(inventory.TryQuickBuy("reca").Applied, Is.True);
            Assert.That(inventory.HeroSlots.Single(x => x != null).itemId, Is.EqualTo("resu"));
            Assert.That(inventory.Gold, Is.Zero);
        }

        [Test]
        public void Transfer_RequiresSpaceAndDoesNotConvertInventoryId()
        {
            var inventory = new OriginalInventory(Fixture(), 1);
            var item = inventory.CreateInstance("stor"); inventory.TryPickup(item);
            Assert.That(inventory.Transfer(OriginalInventoryBag.Hero, 0).Applied, Is.True);
            Assert.That(inventory.ServantSlots[0].instanceId, Is.EqualTo(item.instanceId));
            Assert.That(inventory.ServantSlots[0], Is.Not.SameAs(item));
            Assert.That(item.itemId, Is.EqualTo("base"));
            Assert.That(item.removed, Is.False);
        }

        [Test]
        public void UnimplementedUseAndUnresolvedPawn_DoNotConsumeItemOrCurrency()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 100);
            var item = inventory.CreateInstance("base"); inventory.TryPickup(item);
            Assert.That(inventory.TrySell(OriginalInventoryBag.Hero, 0).Code, Is.EqualTo(OriginalItemActionCode.UnresolvedRule));
            Assert.That(inventory.TryUse(OriginalInventoryBag.Hero, 0).Code, Is.EqualTo(OriginalItemActionCode.UnimplementedEffect));
            Assert.That(inventory.HeroSlots[0].instanceId, Is.EqualTo(item.instanceId));
            Assert.That(inventory.Gold, Is.EqualTo(100));
        }

        [Test]
        public void FullBagConversion_UsesWorldDefaultChargesUnlikeDrop()
        {
            var catalog = Fixture();
            catalog.items.Single(x => x.id == "potw").initialCharges = OriginalDeclaredInt.Known(1);
            var inventory = new OriginalInventory(catalog, 1);
            for (var n = 0; n < 12; n++) inventory.TryPickup(inventory.CreateInstance("noop"));
            var incoming = inventory.CreateInstance("potw"); incoming.charges = 3;
            var result = inventory.TryPickup(incoming);
            Assert.That(result.Code, Is.EqualTo(OriginalItemActionCode.Grounded));
            Assert.That(result.Item.itemId, Is.EqualTo("potw"));
            Assert.That(result.Item.charges, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateSerializedIdentity_IsRejectedBeforeAndAfterConsumption()
        {
            var inventory = new OriginalInventory(Fixture(), 1);
            var existing = inventory.CreateInstance("poti"); existing.charges = 1;
            inventory.TryPickup(existing);
            var duplicate = new OriginalItemInstance { instanceId = existing.instanceId, itemId = "poti", chargesKnown = true, charges = 1 };
            Assert.That(inventory.TryPickup(duplicate).Code, Is.EqualTo(OriginalItemActionCode.InvalidInstance));
            var incoming = inventory.CreateInstance("poti"); incoming.charges = 1;
            inventory.TryPickup(incoming);
            var consumedClone = new OriginalItemInstance { instanceId = incoming.instanceId, itemId = "poti", chargesKnown = true, charges = 1 };
            Assert.That(inventory.TryPickup(consumedClone).Code, Is.EqualTo(OriginalItemActionCode.InvalidInstance));
            Assert.That(existing.charges, Is.EqualTo(2));
        }

        [Test]
        public void PlayerInventories_MintDistinctWorldItemIdentities()
        {
            var first = new OriginalInventory(Fixture(), 1).CreateInstance("noop");
            var second = new OriginalInventory(Fixture(), 2).CreateInstance("noop");
            Assert.That(first.instanceId, Is.Not.EqualTo(second.instanceId));
        }

        [Test]
        public void SpendResources_DeductsBothCurrenciesWithoutTouchingItems()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 100, 50);
            var item = inventory.CreateInstance("noop"); inventory.TryPickup(item);
            Assert.That(inventory.TrySpendResources(40, 20), Is.True);
            Assert.That(inventory.Gold, Is.EqualTo(60));
            Assert.That(inventory.Lumber, Is.EqualTo(30));
            Assert.That(inventory.HeroSlots[0].instanceId, Is.EqualTo(item.instanceId));
        }

        [Test]
        public void SpendResources_InsufficientEitherCurrencyLeavesBothBalancesUnchanged()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 100, 50);
            Assert.That(inventory.TrySpendResources(101, 1), Is.False);
            Assert.That(inventory.Gold, Is.EqualTo(100));
            Assert.That(inventory.Lumber, Is.EqualTo(50));
            Assert.That(inventory.TrySpendResources(1, long.MaxValue), Is.False);
            Assert.That(inventory.Gold, Is.EqualTo(100));
            Assert.That(inventory.Lumber, Is.EqualTo(50));
            Assert.That(inventory.TrySpendResources(100, 50), Is.True);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(inventory.Lumber, Is.Zero);
        }

        [Test]
        public void SpendResources_NegativeAmountsCannotMintCurrencyOrPartiallyDebit()
        {
            var inventory = new OriginalInventory(Fixture(), 1, 100, 50);
            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TrySpendResources(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TrySpendResources(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TrySpendResources(long.MinValue, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => inventory.TrySpendResources(0, long.MinValue));
            Assert.That(inventory.Gold, Is.EqualTo(100));
            Assert.That(inventory.Lumber, Is.EqualTo(50));
        }

        [Test]
        public void SpendResources_LongMaximumAndZeroDoNotOverflowOrProduceNegativeBalance()
        {
            var inventory = new OriginalInventory(Fixture(), 1, long.MaxValue, long.MaxValue);
            Assert.That(inventory.TrySpendResources(0, 0), Is.True);
            Assert.That(inventory.Gold, Is.EqualTo(long.MaxValue));
            Assert.That(inventory.Lumber, Is.EqualTo(long.MaxValue));
            Assert.That(inventory.TrySpendResources(long.MaxValue, long.MaxValue), Is.True);
            Assert.That(inventory.Gold, Is.Zero);
            Assert.That(inventory.Lumber, Is.Zero);
            Assert.That(inventory.TrySpendResources(1, 0), Is.False);
            Assert.That(inventory.TrySpendResources(0, 1), Is.False);
            Assert.That(inventory.TrySpendResources(0, 0), Is.True);
        }
    }
}

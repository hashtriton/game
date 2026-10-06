using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalInventoryBag { Hero, Servant }
    public enum OriginalItemActionCode
    {
        Success, Grounded, UnknownItem, WrongOwner, InvalidSlot, NoSpace,
        NotSoldHere, NotEnoughResources, NotAllowed, NoRecipe, UnresolvedRule,
        UnimplementedEffect, InvalidInstance
    }

    [Serializable]
    public sealed class OriginalItemInstance
    {
        public long instanceId;
        public string itemId;
        public int ownerId;
        public bool chargesKnown;
        public int charges;
        public bool removed;
    }

    public sealed class OriginalItemAction
    {
        public OriginalItemActionCode Code { get; internal set; }
        public string Reason { get; internal set; }
        public OriginalItemInstance Item { get; internal set; }
        public int CraftedRecipeIndex { get; internal set; } = -1;
        public long GoldCost { get; internal set; }
        public long LumberCost { get; internal set; }
        public bool Applied => Code == OriginalItemActionCode.Success || Code == OriginalItemActionCode.Grounded;
    }

    // These remain unknown until a separately verified Warcraft engine profile is supplied.
    [Serializable]
    public sealed class OriginalItemEngineDefaults
    {
        public OriginalDeclaredInt goldCost = new OriginalDeclaredInt();
        public OriginalDeclaredInt lumberCost = new OriginalDeclaredInt();
        public OriginalDeclaredInt initialCharges = new OriginalDeclaredInt();
        public OriginalDeclaredInt droppable = new OriginalDeclaredInt();
    }

    public sealed class OriginalQuickBuyQuote
    {
        public OriginalItemActionCode Code { get; internal set; }
        public string ResultId { get; internal set; }
        public long GoldCost { get; internal set; }
        public string[] MatchedIngredientSlots { get; internal set; } = Array.Empty<string>();
        public bool HasRepeatedIngredientLookup { get; internal set; }
    }

    public sealed class OriginalBuyQuote
    {
        public OriginalItemActionCode Code { get; internal set; }
        public string Reason { get; internal set; }
        public long GoldCost { get; internal set; }
        public long LumberCost { get; internal set; }
        public bool GoldCostKnown { get; internal set; }
        public bool LumberCostKnown { get; internal set; }
    }

    [Serializable]
    public sealed class OriginalInventorySnapshot
    {
        public int ownerId;
        public long gold;
        public long lumber;
        public bool quickBuyEnabled;
        public OriginalItemInstance[] heroSlots;
        public OriginalItemInstance[] servantSlots;
    }

    public sealed class OriginalInventory
    {
        public const int SlotsPerBag = 6;
        public int OwnerId { get; }
        public long Gold { get; private set; }
        public long Lumber { get; private set; }
        public bool QuickBuyEnabled { get; set; }
        public OriginalItemInstance[] HeroSlots => CopySlots(heroSlots);
        public OriginalItemInstance[] ServantSlots => CopySlots(servantSlots);

        // dm/cm maps original unit IDs to 0=strength, 1=agility, 2=intelligence.
        // -1 explicitly means unknown; I07O conversion cannot guess the hero category.
        public int HeroItemAffinity { get; set; } = -1;

        private readonly OriginalItemCatalog catalog;
        private readonly OriginalItemEngineDefaults defaults;
        private readonly OriginalItemRules itemRules;
        private readonly OriginalItemInstance[] heroSlots = new OriginalItemInstance[SlotsPerBag];
        private readonly OriginalItemInstance[] servantSlots = new OriginalItemInstance[SlotsPerBag];
        private readonly HashSet<long> consumedInstanceIds = new HashSet<long>();
        private long nextInstanceId = 1;

        public OriginalInventory(OriginalItemCatalog catalog, int ownerId, long gold = 0, long lumber = 0,
            OriginalItemEngineDefaults engineDefaults = null, OriginalItemRules itemRules = null)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (ownerId < 1 || ownerId > 8) throw new ArgumentOutOfRangeException(nameof(ownerId));
            if (gold < 0 || lumber < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (itemRules != null && (!itemRules.IsFor(catalog) || engineDefaults != null))
                throw new ArgumentException("Per-item rules must belong to this catalog and cannot be combined with global defaults.");
            this.catalog = catalog;
            this.itemRules = itemRules;
            defaults = engineDefaults ?? new OriginalItemEngineDefaults();
            catalog.BuildIndexes();
            OwnerId = ownerId;
            Gold = gold;
            Lumber = lumber;
        }

        public OriginalInventorySnapshot Snapshot() => new OriginalInventorySnapshot
        {
            ownerId = OwnerId, gold = Gold, lumber = Lumber, quickBuyEnabled = QuickBuyEnabled,
            heroSlots = CopySlots(heroSlots), servantSlots = CopySlots(servantSlots)
        };

        // A host transaction mutates a detached candidate and publishes it only
        // after world/profile checks pass. Copies must never run as two owners.
        public OriginalInventory Copy() => new OriginalInventory(this);

        // iYv places inert I09D blockers in physical slots2..5. They are not
        // equip profiles or transferable consumables; iHv removes one at a time.
        internal void InstallCurseSlotBlock(int slot)
        {
            if(slot<2||slot>=6)throw new ArgumentOutOfRangeException(nameof(slot));
            if(heroSlots[slot]?.itemId=="I09D")return;
            if(heroSlots[slot]!=null)throw new InvalidOperationException("Curse slot is occupied before hero acquisition.");
            heroSlots[slot]=CreateInstance("I09D");
        }

        private OriginalInventory(OriginalInventory source)
        {
            catalog = source.catalog; itemRules = source.itemRules;
            defaults = new OriginalItemEngineDefaults
            {
                goldCost = CopyDeclared(source.defaults.goldCost), lumberCost = CopyDeclared(source.defaults.lumberCost),
                initialCharges = CopyDeclared(source.defaults.initialCharges), droppable = CopyDeclared(source.defaults.droppable)
            };
            OwnerId = source.OwnerId; Gold = source.Gold; Lumber = source.Lumber;
            QuickBuyEnabled = source.QuickBuyEnabled; HeroItemAffinity = source.HeroItemAffinity;
            nextInstanceId = source.nextInstanceId;
            Array.Copy(CopySlots(source.heroSlots), heroSlots, SlotsPerBag);
            Array.Copy(CopySlots(source.servantSlots), servantSlots, SlotsPerBag);
            consumedInstanceIds.UnionWith(source.consumedInstanceIds);
        }

        private static OriginalDeclaredInt CopyDeclared(OriginalDeclaredInt value) => value == null ? null :
            new OriginalDeclaredInt { known = value.known, value = value.value,
                sources = value.sources == null ? null : (string[])value.sources.Clone() };

        public void GrantResources(long gold, long lumber)
        {
            if (gold < 0 || lumber < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            var nextGold = checked(Gold + gold);
            var nextLumber = checked(Lumber + lumber);
            Gold = nextGold;
            Lumber = nextLumber;
        }

        // The authoritative session validates the action; this only commits an
        // all-or-nothing debit. Both checks precede either balance mutation.
        public bool TrySpendResources(long gold, long lumber)
        {
            if (gold < 0) throw new ArgumentOutOfRangeException(nameof(gold));
            if (lumber < 0) throw new ArgumentOutOfRangeException(nameof(lumber));
            if (Gold < gold || Lumber < lumber) return false;
            Gold -= gold;
            Lumber -= lumber;
            return true;
        }

        public OriginalItemInstance CreateInstance(string id, int ownerId = 0)
        {
            var definition = catalog.Item(id);
            if (definition == null) throw new ArgumentException("Undefined item: " + id);
            var charges = Resolve(definition, "uses", definition.initialCharges, defaults.initialCharges);
            return new OriginalItemInstance
            {
                // Reserve the creator's upper bits so unowned world items cannot collide across players.
                instanceId = ((long)OwnerId << 48) | nextInstanceId++, itemId = id, ownerId = ownerId,
                chargesKnown = charges.known, charges = charges.known ? charges.value : 0
            };
        }

        public bool CanPickup(OriginalItemInstance item)
        {
            var definition = item == null ? null : catalog.Item(item.itemId);
            return definition != null && (definition.classId == "PowerUp" || definition.classId == "Purchasable" ||
                                          item.ownerId == 0 || item.ownerId == OwnerId);
        }

        public OriginalItemAction TryPickup(OriginalItemInstance item, OriginalInventoryBag actor = OriginalInventoryBag.Hero)
        {
            if (item == null || item.instanceId <= 0 || item.charges < 0 || item.removed || consumedInstanceIds.Contains(item.instanceId))
                return Fail(OriginalItemActionCode.InvalidInstance);
            if (catalog.Item(item.itemId) == null) return Fail(OriginalItemActionCode.UnknownItem);
            if (Contains(item)) return Fail(OriginalItemActionCode.InvalidInstance, "Item identity is already in this inventory.");
            if (!CanPickup(item)) return Fail(OriginalItemActionCode.WrongOwner);

            var conversion = catalog.FromWorld(item.itemId);
            if (conversion != null)
            {
                var oldCharges = item.charges;
                var preserveCharges = item.chargesKnown && item.charges > 0;
                item.itemId = conversion.inventoryId;
                SetDefaultCharges(item);
                if (preserveCharges) { item.chargesKnown = true; item.charges = oldCharges; }
                item.ownerId = OwnerId;
            }
            var actorSlots = Slots(actor);
            // Rzv/R0v reject another seal/mask on this physical holder and
            // leave the newly picked item on the ground, without consuming it.
            int modeFamily = OriginalItemModeRules.Family(item.itemId);
            if (modeFamily != 0 && Array.Exists(actorSlots,held=>held!=null&&OriginalItemModeRules.Family(held.itemId)==modeFamily))
                return Applied(OriginalItemActionCode.Grounded,item);
            if (TryMerge(actorSlots, item) || (actor != OriginalInventoryBag.Servant && TryMerge(servantSlots, item)))
                return Applied(OriginalItemActionCode.Success, item);
            var crafted = CraftFromPickup(item, actorSlots);
            if (crafted != null) return crafted;
            if (TryInsert(actorSlots, item) || (actor != OriginalInventoryBag.Servant && TryInsert(servantSlots, item)))
                return Applied(OriginalItemActionCode.Success, item);
            // aM full-bag fallback creates a fresh world representation without copying positive charges.
            // This differs from the deferred drop conversion in Mm.
            ConvertToWorld(item, false);
            return Applied(OriginalItemActionCode.Grounded, item);
        }

        // Spatial access, stock and host authority are checked by the command handler before this transaction.
        // This is the ordinary native shop purchase plus pickup path; delayed quick-buy is a separate operation.
        public OriginalBuyQuote QuoteBuy(string shopId, string offerId)
        {
            var shop = catalog.Shop(shopId);
            if (shop == null || Array.IndexOf(shop.offerIds, offerId) < 0)
                return new OriginalBuyQuote { Code = OriginalItemActionCode.NotSoldHere };
            var item = catalog.Item(offerId);
            if (item == null) return new OriginalBuyQuote { Code = OriginalItemActionCode.UnknownItem };
            var gold = Resolve(item, "goldcost", item.goldCost, defaults.goldCost);
            var lumber = Resolve(item, "lumbercost", item.lumberCost, defaults.lumberCost);
            var quote = new OriginalBuyQuote { GoldCostKnown = gold.known, LumberCostKnown = lumber.known,
                GoldCost = gold.known ? gold.value : 0, LumberCost = lumber.known ? lumber.value : 0 };
            if (!gold.known || !lumber.known || gold.value < 0 || lumber.value < 0)
            {
                quote.Code = OriginalItemActionCode.UnresolvedRule;
                quote.Reason = "Native item price has an unresolved or invalid component.";
            }
            else quote.Code = Gold < gold.value || Lumber < lumber.value ? OriginalItemActionCode.NotEnoughResources : OriginalItemActionCode.Success;
            return quote;
        }

        public OriginalItemAction TryBuy(string shopId, string offerId, OriginalInventoryBag actor = OriginalInventoryBag.Hero)
        {
            Slots(actor); // Validate the actor before creating or debiting anything.
            var quote = QuoteBuy(shopId, offerId);
            if (quote.Code != OriginalItemActionCode.Success) return Fail(quote.Code, quote.Reason);
            var result = TryPickup(CreateInstance(offerId), actor);
            if (!result.Applied) return result;
            Gold -= quote.GoldCost;
            Lumber -= quote.LumberCost;
            result.GoldCost = quote.GoldCost;
            result.LumberCost = quote.LumberCost;
            return result;
        }

        public OriginalItemAction Drop(OriginalInventoryBag from, int slot)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            var item = slots[slot];
            if(item.itemId=="I09D")return Fail(OriginalItemActionCode.NotAllowed);
            var definition = catalog.Item(item.itemId);
            var drop = Resolve(definition, "droppable", definition.droppable, defaults.droppable);
            if (!drop.known) return Fail(OriginalItemActionCode.UnresolvedRule, "Native droppable flag is unresolved.");
            if (drop.value == 0) return Fail(OriginalItemActionCode.NotAllowed);
            slots[slot] = null;
            ConvertToWorld(item, true);
            return Applied(OriginalItemActionCode.Grounded, item);
        }

        public OriginalItemAction Transfer(OriginalInventoryBag from, int slot)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            if(slots[slot].itemId=="I09D")return Fail(OriginalItemActionCode.NotAllowed);
            var target = from == OriginalInventoryBag.Hero ? servantSlots : heroSlots;
            // EM checks a free hero slot before moving the instance, even if a merge could have worked.
            if (FirstEmpty(target) < 0) return Fail(OriginalItemActionCode.NoSpace);
            var item = slots[slot];
            slots[slot] = null;
            // Flag777 protects the item from the deferred world-ID conversion during transfer.
            return TryPickup(item, from == OriginalInventoryBag.Hero ? OriginalInventoryBag.Servant : OriginalInventoryBag.Hero);
        }

        public OriginalQuickBuyQuote QuoteQuickBuy(string shopOfferId, OriginalInventoryBag actor = OriginalInventoryBag.Hero)
        {
            var row = catalog.QuickBuy(shopOfferId);
            if (row == null || string.IsNullOrEmpty(row.linkedResultId)) return QuoteFail(OriginalItemActionCode.NoRecipe);
            var result = catalog.QuickBuy(row.linkedResultId);
            if (result == null || catalog.Item(row.linkedResultId) == null) return QuoteFail(OriginalItemActionCode.UnresolvedRule);
            if (row.ingredientSlots.Length != SlotsPerBag) return QuoteFail(OriginalItemActionCode.UnresolvedRule);
            var matched = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var repeated = false;
            long cost = result.scriptGoldValue;
            foreach (var id in row.ingredientSlots)
            {
                if (string.IsNullOrEmpty(id) || Find(Slots(actor), id) == null) continue;
                var entry = catalog.QuickBuy(id);
                if (entry == null) return QuoteFail(OriginalItemActionCode.UnresolvedRule);
                matched.Add(id);
                repeated |= !seen.Add(id);
                cost -= entry.scriptGoldValue;
            }
            if (matched.Count == 0) return QuoteFail(OriginalItemActionCode.NoRecipe);
            var resultId = row.linkedResultId;
            if (resultId == "I07O")
            {
                if (HeroItemAffinity < 0 || HeroItemAffinity > 2) return QuoteFail(OriginalItemActionCode.UnresolvedRule);
                resultId = HeroItemAffinity == 0 ? "I07T" : HeroItemAffinity == 2 ? "I087" : "I07O";
            }
            return new OriginalQuickBuyQuote { Code = Gold >= cost ? OriginalItemActionCode.Success : OriginalItemActionCode.NotEnoughResources,
                ResultId = resultId, GoldCost = cost, MatchedIngredientSlots = matched.ToArray(), HasRepeatedIngredientLookup = repeated };
        }

        // Call after ordinary shop delivery for oS; recipe-item use is routed separately to this same transaction.
        public OriginalItemAction TryQuickBuy(string shopOfferId, OriginalInventoryBag actor = OriginalInventoryBag.Hero,
            bool fromChargedItemUse = false)
        {
            if (!fromChargedItemUse && !QuickBuyEnabled) return Fail(OriginalItemActionCode.NotAllowed);
            var quote = QuoteQuickBuy(shopOfferId, actor);
            if (quote.Code != OriginalItemActionCode.Success) return Fail(quote.Code);
            var slots = Slots(actor);
            foreach (var id in quote.MatchedIngredientSlots)
            {
                var found = Find(slots, id);
                if (found != null) { RemoveReference(found); Consume(found); }
            }
            Gold -= quote.GoldCost;
            var result = TryPickup(CreateInstance(quote.ResultId, OwnerId), actor);
            result.GoldCost = quote.GoldCost;
            return result;
        }

        public OriginalItemAction TrySell(OriginalInventoryBag from, int slot)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            var item = slots[slot];
            if(item.itemId=="I09D")return Fail(OriginalItemActionCode.NotAllowed);
            var definition = catalog.Item(item.itemId);
            var flag = Resolve(definition, "pawnable", definition.pawnable, null);
            if (flag != null && flag.known && flag.value == 0) return Fail(OriginalItemActionCode.NotAllowed);
            if (itemRules == null || flag == null || !flag.known || !item.chargesKnown || item.charges < 0)
                return Fail(OriginalItemActionCode.UnresolvedRule);
            var gold = itemRules.Field(item.itemId, "goldcost");
            var lumber = itemRules.Field(item.itemId, "lumbercost");
            var uses = itemRules.Field(item.itemId, "uses");
            if (!gold.known || !lumber.known || !uses.known)
                return Fail(OriginalItemActionCode.UnresolvedRule);
            // PAWN1 native1.26: floor65/2=32; I03L charges0/1/3 gives0/3/9.
            // Native default0 ignores script charges (I0AE charges2 still1340).
            // Other positive default-charge counts use the same explicit ratio
            // as a derived engine policy, rather than rounding each charge.
            long credit = uses.value == 0 ? gold.value / 2 : (long)gold.value * item.charges / (2L * uses.value);
            // PAWN2 cache938dc548cacaae7d1a13392f6a000ceba3f837c2498af02ec63f72e7ae727c82:
            // source cost9 souls with3 charges returns floor(27/2)=13.
            long soulCredit = uses.value == 0 ? lumber.value / 2 : (long)lumber.value * item.charges / (2L * uses.value);
            if (Gold > long.MaxValue - credit || Lumber > long.MaxValue - soulCredit) return Fail(OriginalItemActionCode.NotAllowed);
            Gold += credit; Lumber += soulCredit; slots[slot] = null; Consume(item);
            var result = Applied(OriginalItemActionCode.Success, item);
            result.GoldCost = -credit;
            result.LumberCost = -soulCredit;
            return result;
        }

        public OriginalItemAction TryUse(OriginalInventoryBag from, int slot)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            var definition = catalog.Item(slots[slot].itemId);
            var usable = Resolve(definition, "usable", definition.usable, null);
            if (usable.known && usable.value == 0)
                return Fail(OriginalItemActionCode.NotAllowed);
            return Fail(OriginalItemActionCode.UnimplementedEffect, "An effect executor must validate target, cost and charge consumption before mutation.");
        }

        // Only the validated host effect executor calls this on its candidate.
        // Reusable zero-charge items have separate native semantics and are not
        // silently treated as a one-charge consumable by this method.
        public OriginalItemAction ConsumeCharge(OriginalInventoryBag from, int slot, long instanceId)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            var item = slots[slot];
            if (instanceId <= 0 || item.instanceId != instanceId || item.removed) return Fail(OriginalItemActionCode.InvalidInstance);
            var definition = catalog.Item(item.itemId);
            var usable = Resolve(definition, "usable", definition.usable, null);
            var perishable = Resolve(definition, "perishable", definition.perishable, null);
            if (!usable.known || !perishable.known || !item.chargesKnown) return Fail(OriginalItemActionCode.UnresolvedRule);
            if (usable.value != 1 || perishable.value != 1 || item.charges <= 0) return Fail(OriginalItemActionCode.NotAllowed);
            item.charges--;
            if (item.charges == 0) { slots[slot] = null; Consume(item); }
            return Applied(OriginalItemActionCode.Success, item);
        }

        // Script-only operations run on a detached host transaction. Neither
        // method is exposed as a client command or bypasses instance identity.
        internal OriginalItemAction RemoveForScript(OriginalInventoryBag from, int slot, long instanceId)
        {
            var slots = Slots(from);
            if (slot < 0 || slot >= SlotsPerBag || slots[slot] == null) return Fail(OriginalItemActionCode.InvalidSlot);
            var item = slots[slot];
            if (instanceId <= 0 || item.instanceId != instanceId || item.removed) return Fail(OriginalItemActionCode.InvalidInstance);
            slots[slot] = null; Consume(item);
            return Applied(OriginalItemActionCode.Success, item);
        }

        internal OriginalItemAction ReplaceForScript(OriginalInventoryBag from,int slot,long instanceId,string replacementId)
        {
            var slots=Slots(from);
            if(slot<0||slot>=SlotsPerBag||slots[slot]==null)return Fail(OriginalItemActionCode.InvalidSlot);
            var item=slots[slot];
            if(instanceId<=0||item.instanceId!=instanceId||item.removed)return Fail(OriginalItemActionCode.InvalidInstance);
            if(OriginalItemModeRules.Next(item.itemId)!=replacementId)return Fail(OriginalItemActionCode.NotAllowed);
            var replacement=CreateInstance(replacementId,OwnerId);
            Consume(item);slots[slot]=replacement;
            return Applied(OriginalItemActionCode.Success,replacement);
        }

        internal bool SetScriptCharges(OriginalInventoryBag from, int slot, long instanceId, int charges)
        {
            var slots = Slots(from);
            if (charges < 0 || slot < 0 || slot >= SlotsPerBag || slots[slot] == null ||
                instanceId <= 0 || slots[slot].instanceId != instanceId || slots[slot].removed) return false;
            slots[slot].chargesKnown = true; slots[slot].charges = charges; return true;
        }

        private OriginalItemAction CraftFromPickup(OriginalItemInstance pickup, OriginalItemInstance[] actorSlots)
        {
            foreach (var recipe in catalog.recipes)
            {
                var touched = false;
                foreach (var ingredient in recipe.ingredients) touched |= ingredient.itemId == pickup.itemId;
                if (!touched) continue;
                var selected = new List<OriginalItemInstance>();
                var satisfied = true;
                foreach (var ingredient in recipe.ingredients)
                {
                    var remaining = ingredient.count;
                    if (pickup.itemId == ingredient.itemId) { selected.Add(pickup); remaining--; }
                    Collect(actorSlots, ingredient.itemId, selected, ref remaining);
                    if (!ReferenceEquals(actorSlots, servantSlots)) Collect(servantSlots, ingredient.itemId, selected, ref remaining);
                    else if (remaining > 0)
                    {
                        // Sm scans the actor and da[player] separately, even when they are the same servant.
                        // Preserve this observed static-path quirk without deleting a physical instance twice.
                        foreach (var candidate in servantSlots)
                            if (candidate != null && candidate.itemId == ingredient.itemId) remaining--;
                    }
                    if (remaining > 0) { satisfied = false; break; }
                }
                if (!satisfied) continue;
                foreach (var item in selected) { RemoveReference(item); Consume(item); }
                var result = CreateInstance(recipe.resultId, OwnerId);
                var inserted = TryInsert(actorSlots, result) || (!ReferenceEquals(actorSlots, servantSlots) && TryInsert(servantSlots, result));
                var action = Applied(inserted ? OriginalItemActionCode.Success : OriginalItemActionCode.Grounded, result);
                action.CraftedRecipeIndex = recipe.registrationIndex;
                return action;
            }
            return null;
        }

        private static void Collect(OriginalItemInstance[] slots, string id, List<OriginalItemInstance> selected, ref int remaining)
        {
            foreach (var candidate in slots)
            {
                if (remaining <= 0) return;
                if (candidate == null || candidate.itemId != id || selected.Contains(candidate)) continue;
                selected.Add(candidate);
                remaining--;
            }
        }

        private bool TryMerge(OriginalItemInstance[] slots, OriginalItemInstance item)
        {
            if (!item.chargesKnown || item.charges <= 0) return false;
            var conversion = catalog.FromInventory(item.itemId);
            if (conversion == null || conversion.stackChargeCap <= 0) return false;
            foreach (var existing in slots)
            {
                if (existing == null || existing.itemId != item.itemId || !existing.chargesKnown) continue;
                if ((long)existing.charges + item.charges > conversion.stackChargeCap) continue;
                existing.charges += item.charges;
                Consume(item);
                return true;
            }
            return false;
        }

        private void ConvertToWorld(OriginalItemInstance item, bool preservePositiveCharges)
        {
            var conversion = catalog.FromInventory(item.itemId);
            if (conversion != null)
            {
                var charges = item.charges;
                var preserve = preservePositiveCharges && item.chargesKnown && charges > 0;
                item.itemId = conversion.worldShopId;
                SetDefaultCharges(item);
                if (preserve) { item.chargesKnown = true; item.charges = charges; }
                item.ownerId = OwnerId;
            }
            else if (item.ownerId == 0) item.ownerId = OwnerId;
        }

        private void SetDefaultCharges(OriginalItemInstance item)
        {
            var definition = catalog.Item(item.itemId);
            var value = Resolve(definition, "uses", definition.initialCharges, defaults.initialCharges);
            item.chargesKnown = value.known;
            item.charges = value.known ? value.value : 0;
        }

        private OriginalItemInstance[] Slots(OriginalInventoryBag bag)
        {
            if (bag == OriginalInventoryBag.Hero) return heroSlots;
            if (bag == OriginalInventoryBag.Servant) return servantSlots;
            throw new ArgumentOutOfRangeException(nameof(bag));
        }

        private bool Contains(OriginalItemInstance item) =>
            Array.Exists(heroSlots, x => x != null && x.instanceId == item.instanceId) ||
            Array.Exists(servantSlots, x => x != null && x.instanceId == item.instanceId);

        private void Consume(OriginalItemInstance item)
        {
            item.removed = true;
            consumedInstanceIds.Add(item.instanceId);
        }

        private void RemoveReference(OriginalItemInstance item)
        {
            for (var i = 0; i < SlotsPerBag; i++)
            {
                if (ReferenceEquals(heroSlots[i], item)) heroSlots[i] = null;
                if (ReferenceEquals(servantSlots[i], item)) servantSlots[i] = null;
            }
        }

        private static int FirstEmpty(OriginalItemInstance[] slots) => Array.FindIndex(slots, x => x == null);

        private static bool TryInsert(OriginalItemInstance[] slots, OriginalItemInstance item)
        {
            var slot = FirstEmpty(slots);
            if (slot < 0) return false;
            slots[slot] = item;
            return true;
        }

        private static OriginalItemInstance Find(OriginalItemInstance[] slots, string id) => Array.Find(slots, x => x != null && x.itemId == id);

        private OriginalDeclaredInt Resolve(OriginalItemDefinition item, string field, OriginalDeclaredInt declaration, OriginalDeclaredInt fallback)
        {
            if (itemRules != null) return itemRules.Declared(item.id, field);
            return declaration != null && declaration.known ? declaration : fallback ?? new OriginalDeclaredInt();
        }

        private static OriginalItemInstance[] CopySlots(OriginalItemInstance[] slots)
        {
            var result = new OriginalItemInstance[SlotsPerBag];
            for (int i = 0; i < result.Length; i++)
            {
                var item = slots[i];
                if (item != null) result[i] = new OriginalItemInstance { instanceId = item.instanceId, itemId = item.itemId,
                    ownerId = item.ownerId, chargesKnown = item.chargesKnown, charges = item.charges, removed = item.removed };
            }
            return result;
        }

        private static OriginalItemAction Fail(OriginalItemActionCode code, string reason = null) => new OriginalItemAction { Code = code, Reason = reason };
        private static OriginalItemAction Applied(OriginalItemActionCode code, OriginalItemInstance item) => new OriginalItemAction { Code = code, Item = item };
        private static OriginalQuickBuyQuote QuoteFail(OriginalItemActionCode code) => new OriginalQuickBuyQuote { Code = code };
    }
}

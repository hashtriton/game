using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ShopStock
        {
            internal bool known;
            internal int count, maximum;
            internal double period, next;
        }
        OriginalItemRules itemRules;
        OriginalObservedShopStock readyShopStock;
        OriginalInventoryEffects itemEffects;
        OriginalShopView[] shopPlacements = Array.Empty<OriginalShopView>();
        readonly Dictionary<string, ShopStock> shopStocks = new Dictionary<string, ShopStock>(StringComparer.Ordinal);
        readonly SortedDictionary<long, OriginalGroundItemView> groundItems = new SortedDictionary<long, OriginalGroundItemView>();
        bool centralShopsOpen;
        double acolyteSpawnAt = double.PositiveInfinity;
        bool acolyteStockInitialized;

        // D4 calls OU after Vr advances (18754..58), after the whole duel
        // series. Its pre-duel branch returns before OU. O4:18484 is the
        // mega completion path, not a second pre-duel call. Q3 removes GB.
        void ApplyShopAccessEvent(OriginalMatchEvent item)
        {
            centralShopsOpen = item.enabled;
            bool create = item.enabled && match.Phase == OriginalMatchPhase.Preparation && item.round > 1 && item.round <= 30;
            // Opening ordinary shops before either duel does not recreate
            // u00E or reset its native stock. Only OU creates a fresh shop.
            if(item.enabled&&!create)return;
            acolyteStockInitialized = false;
            acolyteSpawnAt = create ? ItemClock + item.time - match.Clock + 1 : double.PositiveInfinity;
            if (itemCatalog == null || itemRules == null) return;
            foreach (string offer in itemCatalog.Shop("u00E").offerIds)
                if (shopStocks.TryGetValue(StockKey(OriginalShops.AcolyteInstanceId, offer), out var state))
                { state.known = false; state.count = 0; state.next = acolyteSpawnAt; }
        }

        bool AcolytePresent => centralShopsOpen && ItemClock >= acolyteSpawnAt;
        void RefreshAcolyteStock()
        {
            if (!AcolytePresent || acolyteStockInitialized || readyShopStock == null || itemRules == null) return;
            // A fresh shop has native activation delay. Use the earliest
            // measured successful age, preserving unknown stockStart.
            bool allReady = true;
            foreach (string offer in itemCatalog.Shop("u00E").offerIds)
            {
                var state = shopStocks[StockKey(OriginalShops.AcolyteInstanceId, offer)];
                if (state.known) continue;
                var measured = readyShopStock.Offer("u00E", offer);
                var maximum = itemRules.Field(offer, "stockMax"); var period = itemRules.Field(offer, "stockRegen");
                double readyAt = acolyteSpawnAt + readyShopStock.ReadyAgeSeconds("u00E", offer);
                state.next = readyAt;
                if (ItemClock < readyAt || !maximum.known || !period.known) { allReady = false; continue; }
                state.known = true; state.maximum = maximum.value; state.period = period.value;
                state.count = measured.readyCount; state.next = readyAt + state.period;
            }
            acolyteStockInitialized = allReady;
        }

        public void ConfigureItems(OriginalNativeCatalog nativeCatalog, OriginalObservedItemCatalog measuredItems = null,
            OriginalItemPassiveCatalog passiveCatalog = null)
        {
            if (Started) throw new InvalidOperationException("Configure items before starting a match.");
            itemRules = new OriginalItemRules(itemCatalog, nativeCatalog, measuredItems);
            ConfigureItemUse(measuredItems?.itemUse);
            ConfigureItemActives(measuredItems?.itemActives);
            itemEffects = passiveCatalog == null ? null : new OriginalInventoryEffects(passiveCatalog, consumableRules);
            measuredItems?.BuildIndexes(itemCatalog);
            readyShopStock = measuredItems?.readyStock;
        }

        OriginalInventory StartingInventory(Player player)
        {
            var result = new OriginalInventory(itemCatalog, player.slot, itemRules: itemRules);
            // dm/cm, the original hero affinity used by I07O quick-buy.
            result.HeroItemAffinity = player.hero == "H008" ? 0 : player.hero == "N0A0" ? 1 : 2;
            return result;
        }

        void StartShops()
        {
            if (itemRules == null) return;
            shopPlacements = OriginalShops.Placements(options.compactShops);
            shopStocks.Clear(); groundItems.Clear();
            foreach (var shop in shopPlacements)
                foreach (string item in itemCatalog.Shop(shop.unitId).offerIds)
                {
                    var maximum = itemRules.Field(item, "stockMax");
                    var period = itemRules.Field(item, "stockRegen");
                    var start = itemRules.Field(item, "stockStart");
                    var state = new ShopStock { known = maximum.known && period.known && start.known };
                    // The Unity match begins after lobby selection, at the
                    // measured ready-shop phase. Keep unknown stockStart intact:
                    // this observation supplies inventory, not an invented delay.
                    if (maximum.known && period.known && readyShopStock != null &&
                        readyShopStock.TryGetReadyCount(shop.unitId, item, out int readyCount))
                    {
                        state.known = true; state.maximum = maximum.value;
                        state.period = period.value; state.count = readyCount;
                        state.next = ItemClock + state.period;
                    }
                    else if (state.known)
                    {
                        state.maximum = maximum.value; state.period = period.value;
                        state.count = start.value == 0 ? maximum.value : 0;
                        state.next = start.value;
                    }
                    shopStocks.Add(StockKey(shop.instanceId, item), state);
                }
            acolyteSpawnAt = double.PositiveInfinity; acolyteStockInitialized = false;
        }

        static string StockKey(int shop, string item) => shop + ":" + item;
        double ItemClock => world?.Clock ?? match?.Clock ?? 0;
        static void RefreshStock(ShopStock state, double time)
        {
            if (!state.known || state.count >= state.maximum || time < state.next) return;
            if (state.period == 0) state.count = state.maximum;
            else
            {
                int granted = (int)Math.Min(state.maximum - state.count, Math.Floor((time - state.next) / state.period) + 1);
                state.count += granted; state.next += granted * state.period;
            }
        }

        OriginalSessionReplyCode ApplyItemCommand(Player player, OriginalSessionCommand command)
        {
            if (!Started || world == null || itemRules == null || pendingDuel ||
                match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null || actor.health <= 0 || actor.paused || actor.hidden) return OriginalSessionReplyCode.NotReady;
            if (!Enum.IsDefined(typeof(OriginalInventoryBag), command.bag)) return OriginalSessionReplyCode.InvalidCommand;
            var candidate = player.inventory.Copy();
            OriginalItemAction action;
            ShopStock stock = null;
            OriginalGroundItemView picked = null;
            var additionalGround = new List<OriginalItemInstance>();
            PickupTransaction powerup = null;
            switch (command.kind)
            {
                case OriginalSessionCommandKind.SellItem:
                    var pawnShop = Array.Find(shopPlacements, s => s.instanceId == command.shopInstanceId);
                    var pawnRange = native.Constant("PawnItemRange");
                    if (pawnShop == null || !pawnRange.known || duelShopsHidden ||
                        !OriginalShops.IsOpen(pawnShop.instanceId, centralShopsOpen, AcolytePresent) ||
                        Array.IndexOf(combatCatalog.Unit(pawnShop.unitId).Text("abilList").Split(','), "Apit") < 0 ||
                        SquaredDistance(actor.position, pawnShop.position) > pawnRange.Require() * pawnRange.Require())
                        return RejectItem(player, OriginalItemActionCode.NotAllowed);
                    var pawnSlots = command.bag == OriginalInventoryBag.Hero ? candidate.HeroSlots : candidate.ServantSlots;
                    if (command.itemSlot < 0 || command.itemSlot >= pawnSlots.Length || pawnSlots[command.itemSlot] == null ||
                        command.itemInstanceId <= 0 || pawnSlots[command.itemSlot].instanceId != command.itemInstanceId)
                        return RejectItem(player, OriginalItemActionCode.InvalidInstance);
                    action = candidate.TrySell(command.bag, command.itemSlot); break;
                case OriginalSessionCommandKind.BuyItem:
                    RefreshAcolyteStock();
                    var shop = Array.Find(shopPlacements, s => s.instanceId == command.shopInstanceId);
                    if (shop == null || command.itemId == null ||
                        !shopStocks.TryGetValue(StockKey(shop.instanceId, command.itemId), out stock))
                        return RejectItem(player, OriginalItemActionCode.NotSoldHere);
                    if (duelShopsHidden || !OriginalShops.IsOpen(shop.instanceId, centralShopsOpen, AcolytePresent)) return OriginalSessionReplyCode.NotReady;
                    // Authored Aneu Rng1=3000, AbilityData.slk row168. Its
                    // activation radius DataA1=3150 is a separate selection rule.
                    if (SquaredDistance(actor.position, shop.position) > 3000 * 3000)
                        return RejectItem(player, OriginalItemActionCode.NotAllowed);
                    RefreshStock(stock, ItemClock);
                    if (!stock.known) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                    if (stock.count <= 0) return RejectItem(player, OriginalItemActionCode.NotAllowed);
                    var quote = candidate.QuoteBuy(shop.unitId, command.itemId);
                    if (quote.Code != OriginalItemActionCode.Success) return RejectItem(player, quote.Code);
                    var powerupCandidate = candidate.Copy();
                    if (!powerupCandidate.TrySpendResources(quote.GoldCost, quote.LumberCost))
                        return RejectItem(player, OriginalItemActionCode.NotEnoughResources);
                    var boughtPowerup = PreparePowerupPickup(player, actor, powerupCandidate, command.itemId);
                    if (boughtPowerup.handled)
                    {
                        if (command.bag != OriginalInventoryBag.Hero) return RejectItem(player, OriginalItemActionCode.UnimplementedEffect);
                        if (boughtPowerup.code != OriginalItemActionCode.Success) return RejectItem(player, boughtPowerup.code);
                        powerup = boughtPowerup; action = new OriginalItemAction { Code = OriginalItemActionCode.Success }; break;
                    }
                    action = candidate.TryBuy(shop.unitId, command.itemId, command.bag);
                    if (action.Applied && candidate.QuickBuyEnabled)
                    {
                        // oS8351 waits for native purchase/pickup before
                        // looking only in the buying unit's inventory.
                        var automatic = candidate.Copy();
                        var quick = automatic.TryQuickBuy(command.itemId, command.bag);
                        if (quick.Applied)
                        {
                            if (action.Code == OriginalItemActionCode.Grounded) additionalGround.Add(action.Item);
                            candidate = automatic; action = quick;
                        }
                    }
                    break;
                case OriginalSessionCommandKind.DropItem:
                    action = candidate.Drop(command.bag, command.itemSlot); break;
                case OriginalSessionCommandKind.TransferItem:
                    action = candidate.Transfer(command.bag, command.itemSlot); break;
                case OriginalSessionCommandKind.PickupItem:
                    if (!groundItems.TryGetValue(command.itemInstanceId, out picked))
                        return RejectItem(player, OriginalItemActionCode.InvalidInstance);
                    var pickupRange = native.Constant("PickupItemRange");
                    if (!pickupRange.known) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                    if (SquaredDistance(actor.position, picked.position) > pickupRange.Require() * pickupRange.Require())
                        return RejectItem(player, OriginalItemActionCode.NotAllowed);
                    if (!candidate.CanPickup(picked.item)) return RejectItem(player, OriginalItemActionCode.WrongOwner);
                    var pickedPowerup = PreparePowerupPickup(player, actor, candidate, picked.item.itemId);
                    if (pickedPowerup.handled)
                    {
                        if (command.bag != OriginalInventoryBag.Hero) return RejectItem(player, OriginalItemActionCode.UnimplementedEffect);
                        if (pickedPowerup.code != OriginalItemActionCode.Success) return RejectItem(player, pickedPowerup.code);
                        powerup = pickedPowerup; action = new OriginalItemAction { Code = OriginalItemActionCode.Success }; break;
                    }
                    action = candidate.TryPickup(CopyItem(picked.item), command.bag); break;
                default: return OriginalSessionReplyCode.InvalidCommand;
            }
            if (!action.Applied) return RejectItem(player, action.Code);
            if (groundItems.Count - (picked == null ? 0 : 1) + additionalGround.Count +
                (action.Code == OriginalItemActionCode.Grounded ? 1 : 0) > 8192)
                return RejectItem(player, OriginalItemActionCode.NoSpace);
            if (powerup == null && !ApplyEquipmentProfile(player, candidate, actor))
                return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
            // All guards and inventory mutations occurred on a detached candidate.
            // Commit one residency per instance; a full bag purchase stays on the
            // ground and is never lost after charging the buyer.
            if (powerup != null) CommitPowerupPickup(powerup);
            if (picked != null) groundItems.Remove(command.itemInstanceId);
            if (action.Code == OriginalItemActionCode.Grounded)
                groundItems[action.Item.instanceId] = new OriginalGroundItemView { item = CopyItem(action.Item), position = actor.position };
            foreach (var item in additionalGround)
                groundItems[item.instanceId] = new OriginalGroundItemView { item = CopyItem(item), position = actor.position };
            if (stock != null)
            {
                bool wasFull = stock.count == stock.maximum;
                stock.count--;
                if (wasFull) stock.next = ItemClock + stock.period;
            }
            if (powerup == null) player.inventory = candidate;
            SyncItemScriptInventory(player);
            player.lastItemAction = action.Code;
            return OriginalSessionReplyCode.Accepted;
        }

        static OriginalSessionReplyCode RejectItem(Player player, OriginalItemActionCode code)
        {
            player.lastItemAction = code;
            return code == OriginalItemActionCode.UnresolvedRule || code == OriginalItemActionCode.UnimplementedEffect ?
                OriginalSessionReplyCode.RuleUnavailable : OriginalSessionReplyCode.ItemRejected;
        }

        static OriginalItemInstance CopyItem(OriginalItemInstance item) => item == null ? null : new OriginalItemInstance
        { instanceId = item.instanceId, itemId = item.itemId, ownerId = item.ownerId, chargesKnown = item.chargesKnown, charges = item.charges, removed = item.removed };

        OriginalShopView[] ShopViews()
        {
            var result = new List<OriginalShopView>();
            RefreshAcolyteStock();
            double now = ItemClock;
            foreach (var shop in shopPlacements)
            {
                if (shop.instanceId == OriginalShops.AcolyteInstanceId && !AcolytePresent) continue;
                var stocks = new List<OriginalShopStockView>();
                foreach (string item in itemCatalog.Shop(shop.unitId).offerIds)
                {
                    var stock = shopStocks[StockKey(shop.instanceId, item)]; RefreshStock(stock, now);
                    stocks.Add(new OriginalShopStockView { itemId = item, known = stock.known,
                        available = stock.count, nextAvailableAt = stock.count > 0 ? 0 : stock.next });
                }
                result.Add(new OriginalShopView { instanceId = shop.instanceId, unitId = shop.unitId,
                    position = shop.position, open = !duelShopsHidden && OriginalShops.IsOpen(shop.instanceId, centralShopsOpen, AcolytePresent), stock = stocks.ToArray() });
            }
            return result.ToArray();
        }

        OriginalGroundItemView[] GroundItemViews()
        {
            var result = new List<OriginalGroundItemView>();
            foreach (var item in groundItems.Values) result.Add(new OriginalGroundItemView { item = CopyItem(item.item), position = item.position });
            return result.ToArray();
        }
    }
}

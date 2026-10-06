using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        GameObject marketPanel, marketToggle;
        Text marketTitle, marketMessage, bagTitle;
        readonly Button[] shopButtons = new Button[8];
        readonly Button[] offerButtons = new Button[12];
        readonly Button[] inventoryButtons = new Button[12];
        Button dropButton, transferButton, useItemButton, quickBuyButton;
        OriginalGameCatalogs marketCatalogs;
        OriginalItemRules marketRules;
        readonly List<OriginalShopView> listedShops = new List<OriginalShopView>();
        readonly List<OriginalGroundItemView> nearbyItems = new List<OriginalGroundItemView>();
        int shopPage, selectedShop, selectedSlot = -1, groundPage;
        Button previousGroundPage, nextGroundPage;
        Text groundPageLabel;
        bool marketOpen;
        bool groundMode;
        long hammerInstance;
        int hammerSlot;
        Button sellButton;
        bool soulMode;

        void BuildMarket(Transform root)
        {
            marketToggle = MakeButton(root, "B  ПРЕДМЕТЫ", new Vector2(.815f, .227f), new Vector2(.982f, .27f),
                () => marketOpen = !marketOpen, 16).gameObject;
            var body = Box(root, "Items and shops", new Vector2(.08f, .285f), new Vector2(.982f, .89f), panel);
            marketPanel = body.gameObject;
            marketTitle = Label(body, "ПРЕДМЕТЫ", 23, gold, new Vector2(.02f, .90f), new Vector2(.54f, .99f));
            MakeButton(body, "ДУШИ", new Vector2(.55f, .92f), new Vector2(.70f, .985f), () => { soulMode = true; groundMode = false; }, 12);
            MakeButton(body, "РЯДОМ", new Vector2(.71f, .92f), new Vector2(.84f, .985f), () => { groundMode = true; soulMode = false; groundPage = 0; }, 12);
            MakeButton(body, "ЗАКРЫТЬ", new Vector2(.85f, .92f), new Vector2(.98f, .985f), () => marketOpen = false, 12);
            for (int i = 0; i < shopButtons.Length; i++)
            {
                int row = i;
                shopButtons[i] = MakeButton(body, "", new Vector2(.02f, .84f - i * .064f), new Vector2(.22f, .90f - i * .064f), () =>
                {
                    int index = shopPage * 8 + row;
                    if (index < listedShops.Count) { selectedShop = listedShops[index].instanceId; groundMode = false; soulMode = false; }
                }, 12);
            }
            MakeButton(body, "<", new Vector2(.02f, .325f), new Vector2(.11f, .382f), () => shopPage = Math.Max(0, shopPage - 1), 14);
            MakeButton(body, ">", new Vector2(.13f, .325f), new Vector2(.22f, .382f), () =>
                shopPage = Math.Min(Math.Max(0, (listedShops.Count - 1) / 8), shopPage + 1), 14);
            for (int i = 0; i < offerButtons.Length; i++)
            {
                int index = i; int column = i % 3, row = i / 3;
                offerButtons[i] = MakeButton(body, "", new Vector2(.245f + column * .245f, .772f - row * .126f),
                    new Vector2(.48f + column * .245f, .89f - row * .126f), () => BuyOffer(index), 12);
            }
            previousGroundPage=MakeButton(body,"<",new Vector2(.245f,.325f),new Vector2(.282f,.382f),()=>groundPage=Math.Max(0,groundPage-1),14);
            groundPageLabel=Label(body,"",11,muted,new Vector2(.285f,.325f),new Vector2(.35f,.382f),TextAnchor.MiddleCenter);
            nextGroundPage=MakeButton(body,">",new Vector2(.353f,.325f),new Vector2(.39f,.382f),
                ()=>groundPage=Math.Min(Math.Max(0,(nearbyItems.Count-1)/12),groundPage+1),14);
            marketMessage = Label(body, "", 12, muted, new Vector2(.405f, .32f), new Vector2(.98f, .395f));
            bagTitle = Label(body, "ГЕРОЙ / ПРИСЛУЖНИК", 12, gold, new Vector2(.02f, .269f), new Vector2(.75f, .319f));
            for (int i = 0; i < inventoryButtons.Length; i++)
            {
                int slot = i; int row = i / 6, column = i % 6;
                inventoryButtons[i] = MakeButton(body, "", new Vector2(.02f + column * .123f, .166f - row * .082f),
                    new Vector2(.136f + column * .123f, .243f - row * .082f), () => SelectInventorySlot(slot), 10);
            }
            useItemButton = MakeButton(body, "ИСПОЛЬЗОВАТЬ", new Vector2(.78f, .190f), new Vector2(.98f, .243f),
                () => InventoryCommand(OriginalSessionCommandKind.UseItem), 11);
            sellButton = MakeButton(body, "ПРОДАТЬ", new Vector2(.78f, .245f), new Vector2(.98f, .298f),
                () => InventoryCommand(OriginalSessionCommandKind.SellItem), 11);
            transferButton = MakeButton(body, "ПЕРЕЛОЖИТЬ", new Vector2(.78f, .135f), new Vector2(.98f, .188f),
                () => InventoryCommand(OriginalSessionCommandKind.TransferItem), 11);
            dropButton = MakeButton(body, "ВЫЛОЖИТЬ", new Vector2(.78f, .080f), new Vector2(.98f, .133f),
                () => InventoryCommand(OriginalSessionCommandKind.DropItem), 11);
            Label(body, "Покупки доступны во время боя.", 11, muted, new Vector2(.02f, .012f), new Vector2(.52f, .07f));
            quickBuyButton = MakeButton(body, "БЫСТРАЯ СБОРКА: ВЫКЛ", new Vector2(.53f, .012f), new Vector2(.98f, .07f), () =>
                Network.SendCommand(OriginalSessionCommandKind.SetQuickBuy, ready: !(LocalPlayer(Network.View)?.inventory?.quickBuyEnabled ?? false)), 11);
            marketPanel.SetActive(false);
        }

        void BuyOffer(int index)
        {
            if (soulMode)
            {
                var upgrades = LocalPlayer(Network.View)?.soulUpgrades;
                if (upgrades != null && index < upgrades.Length)
                    Network.SendCommand(OriginalSessionCommandKind.BuySoulUpgrade, skillId: upgrades[index].id);
                return;
            }
            if (groundMode)
            {
                index+=groundPage*offerButtons.Length;
                if (index < nearbyItems.Count)
                {
                    if (hammerInstance != 0) UseHammerTarget(nearbyItems[index].item.instanceId);
                    else Network.SendCommand(OriginalSessionCommandKind.PickupItem, itemInstanceId: nearbyItems[index].item.instanceId);
                }
                return;
            }
            var shop = listedShops.Find(s => s.instanceId == selectedShop);
            if (shop == null || marketCatalogs == null) return;
            var definition = marketCatalogs.Items.Shop(shop.unitId);
            if (definition == null) return;
            var offers = definition.offerIds;
            if (index >= offers.Length) return;
            Network.SendCommand(OriginalSessionCommandKind.BuyItem, itemId: offers[index], shopInstanceId: selectedShop);
        }

        void InventoryCommand(OriginalSessionCommandKind kind)
        {
            if (selectedSlot < 0 || selectedSlot >= 12) return;
            var inventory = LocalPlayer(Network.View)?.inventory;
            var item = inventory == null ? null : (selectedSlot < 6 ? inventory.heroSlots[selectedSlot] : inventory.servantSlots[selectedSlot - 6]);
            if (item == null) return;
            if (kind == OriginalSessionCommandKind.UseItem && item.itemId == "I0B7" && selectedSlot < 6)
            {
                if (hammerInstance == item.instanceId) UseHammerTarget(0);
                else { hammerInstance = item.instanceId; hammerSlot = selectedSlot; }
                return;
            }
            hammerInstance = 0;
            if (kind == OriginalSessionCommandKind.UseItem)
            {
                if (runtime.RequestItemUse(item.instanceId) && runtime.ArmedItemInstance != 0) marketOpen = false;
                return;
            }
            Network.SendCommand(kind, bag: selectedSlot < 6 ? OriginalInventoryBag.Hero : OriginalInventoryBag.Servant,
                itemSlot: selectedSlot % 6, itemInstanceId: item.instanceId,
                shopInstanceId: kind == OriginalSessionCommandKind.SellItem ? NearbyPawnShop(Network.View) : 0);
        }

        int NearbyPawnShop(OriginalSessionView view)
        {
            var player = LocalPlayer(view);
            var hero = player == null || view.world == null ? null : Array.Find(view.world.units,
                u => u.entityId == OriginalWorld.HeroEntityId(player.slot));
            if (hero == null || marketCatalogs == null || view.shops == null) return 0;
            double range = marketCatalogs.Native.Constant("PawnItemRange").Require();
            foreach (var shop in view.shops)
                if (shop.open && DistanceSquared(hero.position, shop.position) <= range * range &&
                    Array.IndexOf(marketCatalogs.Combat.Unit(shop.unitId).Text("abilList").Split(','), "Apit") >= 0)
                    return shop.instanceId;
            return 0;
        }

        void SelectInventorySlot(int slot)
        {
            var inventory = LocalPlayer(Network.View)?.inventory;
            var item = inventory == null ? null : slot < 6 ? inventory.heroSlots[slot] : inventory.servantSlots[slot - 6];
            if (hammerInstance != 0 && slot < 6 && item != null && item.instanceId != hammerInstance)
            { UseHammerTarget(item.instanceId); return; }
            selectedSlot = slot;
        }

        void UseHammerTarget(long target)
        {
            Network.SendCommand(OriginalSessionCommandKind.UseItem, itemSlot: hammerSlot,
                itemInstanceId: hammerInstance, targetItemInstanceId: target);
            hammerInstance = 0;
        }

        void RefreshMarket(bool playing, OriginalSessionView view, OriginalSessionPlayerView player)
        {
            marketToggle.SetActive(playing);
            if (!playing) { marketOpen = false; selectedSlot = -1; groundMode = false; hammerInstance = 0; soulMode = false; groundPage = 0; }
            var keys = Keyboard.current;
            if (playing && !optionsOpen && keys != null && keys.bKey.wasPressedThisFrame && !TypingInHud()) marketOpen = !marketOpen;
            if (marketOpen && keys != null && keys.escapeKey.wasPressedThisFrame)
            { if (hammerInstance != 0) hammerInstance = 0; else marketOpen = false; }
            marketPanel.SetActive(playing && marketOpen);
            if (!playing || !marketOpen) return;
            EnsureMarketCatalogs();
            listedShops.Clear();
            if (view.shops != null) foreach (var shop in view.shops)
                if (shop != null && shop.open && marketCatalogs.Items.Shop(shop.unitId) != null) listedShops.Add(shop);
            shopPage = Math.Min(shopPage, Math.Max(0, (listedShops.Count - 1) / 8));
            var selected = listedShops.Find(s => s.instanceId == selectedShop);
            if (selected == null && listedShops.Count > 0) { selected = listedShops[0]; selectedShop = selected.instanceId; }
            for (int i = 0; i < shopButtons.Length; i++)
            {
                int index = shopPage * 8 + i; bool available = index < listedShops.Count;
                shopButtons[i].gameObject.SetActive(available);
                if (available)
                {
                    var shop = listedShops[index];
                    Write(shopButtons[i].GetComponentInChildren<Text>(), marketCatalogs.Items.Shop(shop.unitId).displayName +
                        (shop.instanceId == 201 ? " / СЕВЕР" : shop.instanceId == 202 ? " / ЮГ" : ""));
                    shopButtons[i].image.color = shop.instanceId == selectedShop ? new Color(.24f, .21f, .13f) : inset;
                }
            }
            var definition = selected == null ? null : marketCatalogs.Items.Shop(selected.unitId);
            Write(marketTitle, soulMode ? "УЛУЧШЕНИЯ ЗА ДУШИ" : groundMode ? "ПРЕДМЕТЫ РЯДОМ" : definition == null ? "ПРЕДМЕТЫ" : definition.displayName.ToUpperInvariant());
            var unit = runtime.LocalUnit;
            nearbyItems.Clear();
            var pickup = marketCatalogs.Native.Constant("PickupItemRange");
            if (unit != null && pickup.known && view.groundItems != null)
                foreach (var ground in view.groundItems)
                    if (CanPickUpGround(ground, player) &&
                        DistanceSquared(unit.position, ground.position) <= pickup.Require() * pickup.Require()) nearbyItems.Add(ground);
            nearbyItems.Sort((a,b)=>a.item.instanceId.CompareTo(b.item.instanceId));
            groundPage=Math.Min(groundPage,Math.Max(0,(nearbyItems.Count-1)/offerButtons.Length));
            previousGroundPage.gameObject.SetActive(groundMode); nextGroundPage.gameObject.SetActive(groundMode); groundPageLabel.gameObject.SetActive(groundMode);
            previousGroundPage.interactable=groundPage>0; nextGroundPage.interactable=(groundPage+1)*offerButtons.Length<nearbyItems.Count;
            Write(groundPageLabel,(groundPage+1)+" / "+Math.Max(1,(nearbyItems.Count+offerButtons.Length-1)/offerButtons.Length));
            bool inRange = selected != null && unit != null && DistanceSquared(unit.position, selected.position) <= 3000 * 3000;
            for (int i = 0; i < offerButtons.Length; i++)
            {
                if (soulMode)
                {
                    bool present = player?.soulUpgrades != null && i < player.soulUpgrades.Length;
                    offerButtons[i].gameObject.SetActive(present);
                    if (!present) continue;
                    var upgrade = player.soulUpgrades[i];
                    string state = upgrade.researching ? "ИССЛЕДУЕТСЯ" : !upgrade.unlocked ? "НУЖНЫ ВСЕ 60 УЛУЧШЕНИЙ" :
                        upgrade.rank == upgrade.maximumRank ? "МАКСИМУМ" : upgrade.soulCost + " ДУШ";
                    Write(offerButtons[i].GetComponentInChildren<Text>(), upgrade.name + "  " + upgrade.rank + "/" + upgrade.maximumRank +
                        "\n" + OriginalSoulUpgradeRules.Description(upgrade.id) + "\n" + state);
                    offerButtons[i].interactable = upgrade.unlocked && upgrade.rank < upgrade.maximumRank && player.souls >= upgrade.soulCost &&
                        !Array.Exists(player.soulUpgrades, row => row.researching) && string.IsNullOrEmpty(view.haltReason);
                    continue;
                }
                int groundIndex=groundPage*offerButtons.Length+i;
                bool available = groundMode ? groundIndex < nearbyItems.Count : definition != null && i < definition.offerIds.Length;
                offerButtons[i].gameObject.SetActive(available);
                if (!available) continue;
                if (groundMode)
                {
                    var ground = nearbyItems[groundIndex];
                    Write(offerButtons[i].GetComponentInChildren<Text>(), marketCatalogs.Items.Item(ground.item.itemId).displayName + "\nПОДОБРАТЬ");
                    offerButtons[i].interactable = player != null && player.alive && !unit.paused && string.IsNullOrEmpty(view.haltReason);
                    continue;
                }
                string id = definition.offerIds[i]; var item = marketCatalogs.Items.Item(id);
                var cost = marketRules.Field(id, "goldcost"); var souls = marketRules.Field(id, "lumbercost");
                var stock = Array.Find(selected.stock, s => s.itemId == id);
                string price = (cost.known ? cost.value.ToString() : "?") + " золота" +
                    (souls.known&&souls.value==0?"":" / "+(souls.known?souls.value.ToString():"?")+" душ");
                string inventoryId=marketCatalogs.Items.FromWorld(id)?.inventoryId;
                string offerName=inventoryId==null?item.displayName:marketCatalogs.Items.Item(inventoryId).displayName;
                Write(offerButtons[i].GetComponentInChildren<Text>(), offerName + "\n" + price);
                offerButtons[i].interactable = inRange && player != null && player.alive && unit != null && !unit.paused &&
                    cost.known && souls.known && player.gold >= cost.value && player.souls >= souls.value &&
                    stock != null && stock.known && stock.available > 0 && string.IsNullOrEmpty(view.haltReason);
            }
            Write(marketMessage, groundMode ? (nearbyItems.Count == 0 ? "Рядом нет предметов." : "Выберите предмет, чтобы подобрать.") : selected == null ? "Нет доступных лавок." : !inRange ? "Подойдите ближе к лавке." :
                "Золото: " + player.gold + "    Души: " + player.souls + "    " + ItemActionMessage(player.lastItemAction));
            var inventory = player?.inventory;
            if (hammerInstance != 0 && (inventory == null || hammerSlot >= inventory.heroSlots.Length ||
                inventory.heroSlots[hammerSlot]?.instanceId != hammerInstance)) hammerInstance = 0;
            if (hammerInstance != 0) Write(marketMessage, "Выберите предмет для разбора. Ещё раз нажмите «Использовать» для первой подходящей вещи. Esc - отмена.");
            if (soulMode) Write(marketMessage, "Улучшения у прислужника сохраняются до конца матча. Исследование занимает 1 секунду.");
            Write(quickBuyButton.GetComponentInChildren<Text>(), "БЫСТРАЯ СБОРКА: " + (inventory != null && inventory.quickBuyEnabled ? "ВКЛ" : "ВЫКЛ"));
            for (int i = 0; i < inventoryButtons.Length; i++)
            {
                var item = inventory == null ? null : (i < 6 ? inventory.heroSlots[i] : inventory.servantSlots[i - 6]);
                var itemDefinition = item == null ? null : marketCatalogs.Items.Item(item.itemId);
                string name = item == null ? (i % 6 + 1).ToString() : (itemDefinition?.displayName ?? "Неизвестный предмет") +
                    (item.chargesKnown && item.charges > 0 ? " x" + item.charges : "");
                Write(inventoryButtons[i].GetComponentInChildren<Text>(), name);
                inventoryButtons[i].interactable = item != null;
                inventoryButtons[i].image.color = selectedSlot == i ? new Color(.24f, .21f, .13f) : inset;
            }
            bool occupied = inventory != null && selectedSlot >= 0 &&
                (selectedSlot < 6 ? inventory.heroSlots[selectedSlot] : inventory.servantSlots[selectedSlot - 6]) != null;
            transferButton.interactable = dropButton.interactable = occupied && player.alive;
            sellButton.interactable = occupied && player.alive && NearbyPawnShop(view) != 0;
            Write(sellButton.GetComponentInChildren<Text>(), NearbyPawnShop(view) != 0 ? "ПРОДАТЬ" : "ПРОДАЖА У ЛАВКИ");
            var use = player?.itemUses == null || selectedSlot < 0 ? null : Array.Find(player.itemUses,
                entry => (int)entry.bag == selectedSlot / 6 && entry.slot == selectedSlot % 6);
            useItemButton.interactable = use != null && use.code == OriginalItemUseCode.Ready && string.IsNullOrEmpty(view.haltReason);
            Write(useItemButton.GetComponentInChildren<Text>(), use != null && use.cooldownRemaining > 0 ?
                "ПОДОЖДИТЕ " + Math.Ceiling(use.cooldownRemaining) + " С" : "ИСПОЛЬЗОВАТЬ");
        }

        bool CanPickUpGround(OriginalGroundItemView ground,OriginalSessionPlayerView player)
        {
            var item=ground?.item;var definition=item==null?null:marketCatalogs.Items.Item(item.itemId);
            return player!=null&&item!=null&&!item.removed&&definition!=null&&
                (definition.classId=="PowerUp"||definition.classId=="Purchasable"||item.ownerId==0||item.ownerId==player.slot);
        }

        static double DistanceSquared(OriginalPoint a, OriginalPoint b) => (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        static string ItemActionMessage(OriginalItemActionCode code)
        {
            switch (code)
            {
                case OriginalItemActionCode.NoSpace: return "Нет свободного места.";
                case OriginalItemActionCode.NotEnoughResources: return "Не хватает ресурсов.";
                case OriginalItemActionCode.WrongOwner: return "Предмет принадлежит другому игроку.";
                case OriginalItemActionCode.UnresolvedRule:
                case OriginalItemActionCode.UnimplementedEffect: return "Действие пока недоступно в этой сборке.";
                case OriginalItemActionCode.NotAllowed: return "Действие недоступно.";
                case OriginalItemActionCode.NoRecipe: return "Этот предмет нельзя разобрать. Молот возвращён.";
                case OriginalItemActionCode.Grounded: return "Предмет лежит рядом с героем.";
                default: return "";
            }
        }
    }
}

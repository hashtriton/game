using System.Collections.Generic;
using Arena.Original;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public sealed partial class GameHud
    {
        private const int ShelfColumns = 8;
        private const float CellSize = 100f;

        private GameObject shopWindow;
        private Text goldShop;
        private RectTransform shelfArea;
        private RectTransform guideArea;
        private readonly List<Image> tabImages = new List<Image>();
        private readonly List<ItemSlotView> shelfCells = new List<ItemSlotView>();
        private readonly List<ShopEntry> shelfEntries = new List<ShopEntry>();
        private readonly ItemSlotView[] shopBag = new ItemSlotView[6];
        private int currentTab;
        private bool lastWasGuides;

        private int GuideTab => Book.Tabs.Count;

        private void BuildShopWindow()
        {
            var window = UiKit.Picture(transform, "Shop window", assets.panel, Color.white, true);
            UiKit.Place(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 62f), new Vector2(1240f, 740f));
            shopWindow = window.gameObject;
            var root = window.rectTransform;

            var title = UiKit.Label(root, "Title", assets.titleFont, 36, UiColors.Gold, TextAnchor.UpperLeft, "ЛАВКИ");
            Corner(title.rectTransform, 36f, -24f, 400f, 46f);

            var coin = UiKit.Picture(root, "Coin", assets.coin, Color.white);
            Corner(coin.rectTransform, 930f, -30f, 30f, 30f);
            goldShop = UiKit.Label(root, "Gold", assets.titleFont, 32, UiColors.Gold, TextAnchor.MiddleLeft, "0");
            Corner(goldShop.rectTransform, 968f, -28f, 170f, 34f);
            PanelButton(root, "X", new Vector2(1176f, -22f), new Vector2(40f, 40f), CloseShop);

            shelfArea = UiKit.Rect("Shelf", root);
            Corner(shelfArea, 276f, -98f, 940f, 520f);
            var grid = shelfArea.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellSize, CellSize + 24f);
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = ShelfColumns;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.childAlignment = TextAnchor.UpperLeft;

            guideArea = UiKit.Rect("Guides", root);
            Corner(guideArea, 276f, -98f, 940f, 520f);
            guideArea.gameObject.SetActive(false);

            var bagLabel = UiKit.Label(root, "Bag label", assets.bodyFont, 21, UiColors.Muted, TextAnchor.MiddleLeft, "Рюкзак");
            Corner(bagLabel.rectTransform, 276f, -640f, 90f, 64f);
            for (var i = 0; i < shopBag.Length; i++)
            {
                var slot = new ItemSlotView(assets, root, "Shop bag " + (i + 1), 64f, false);
                Corner(slot.root, 372f + i * 72f, -640f, 64f, 64f);
                var index = i;
                var relay = UiKit.Relay(slot.root.gameObject);
                relay.entered = _ => ShowTooltip(slot.ItemId, BagFooter(index));
                relay.exited = _ => HideTooltip();
                relay.clicked = data =>
                {
                    if (data.button == PointerEventData.InputButton.Right) SellFromBag(index);
                };
                shopBag[i] = slot;
            }
            var hint = UiKit.Label(root, "Hint", assets.bodyFont, 17, UiColors.Muted, TextAnchor.MiddleLeft,
                "ЛКМ по предмету: купить вместе с недостающими частями. ПКМ: только сам предмет (для составных это рецепт). ПКМ по рюкзаку: продать за половину цены.");
            Corner(hint.rectTransform, 820f, -632f, 396f, 84f);

            shopWindow.SetActive(false);
        }

        private void BuildShelves()
        {
            // Tabs: one per shelf, then the guides.
            var tabsRoot = shopWindow.transform;
            var names = new List<string>();
            foreach (var tab in Book.Tabs) names.Add(tab.title);
            names.Add("ГАЙДЫ");
            for (var i = 0; i < names.Count; i++)
            {
                var index = i;
                var image = UiKit.Picture(tabsRoot, "Tab " + names[i], assets.tab, Color.white, true);
                Corner(image.rectTransform, 34f, -98f - i * 44f, 226f, 40f);
                var button = image.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => SelectTab(index));
                var label = UiKit.Label(image.rectTransform, "Text", assets.bodyFont, 20, UiColors.Text, TextAnchor.MiddleLeft, names[i]);
                UiKit.Stretch(label.rectTransform, 14f, 0f, 6f, 0f);
                tabImages.Add(image);
            }

            var most = 0;
            foreach (var tab in Book.Tabs) most = Mathf.Max(most, tab.entries.Count);
            for (var i = 0; i < most; i++)
            {
                var cell = new ItemSlotView(assets, shelfArea, "Shelf cell " + (i + 1), CellSize, true);
                var cellIndex = i;
                var relay = UiKit.Relay(cell.root.gameObject);
                relay.entered = _ => ShowTooltip(cell.ItemId, ShelfFooter(cellIndex));
                relay.exited = _ => HideTooltip();
                relay.clicked = data =>
                {
                    if (cellIndex >= shelfEntries.Count) return;
                    BuyEntry(shelfEntries[cellIndex], data.button != PointerEventData.InputButton.Right);
                };
                shelfCells.Add(cell);
            }
            SelectTab(0);
        }

        private void ToggleShop(bool guides)
        {
            // The buttons are on screen before the item data is parsed, or for good when it failed to load.
            if (!session.Ready) return;
            if (ShopOpen && lastWasGuides == guides)
            {
                CloseShop();
                return;
            }
            lastWasGuides = guides;
            shopWindow.SetActive(true);
            SelectTab(guides ? GuideTab : (currentTab == GuideTab ? 0 : currentTab));
            RefreshShopBag(Loadout.Slots);
        }

        /// <summary>Opens the shop window on a shelf (by index) or on the guides; used by tests and by the keys.</summary>
        public void OpenShop(int tab)
        {
            if (!session.Ready) return;
            shopWindow.SetActive(true);
            SelectTab(Mathf.Clamp(tab, 0, GuideTab));
            RefreshShopBag(Loadout.Slots);
        }

        private void CloseShop()
        {
            shopWindow.SetActive(false);
            HideTooltip();
        }

        private void SelectTab(int index)
        {
            currentTab = index;
            lastWasGuides = index == GuideTab;
            for (var i = 0; i < tabImages.Count; i++)
                tabImages[i].color = i == index ? new Color(1.35f, 1.1f, 0.75f) : Color.white;

            var guides = index == GuideTab;
            shelfArea.gameObject.SetActive(!guides);
            guideArea.gameObject.SetActive(guides);
            HideTooltip();

            if (guides)
            {
                RefreshGuides();
                return;
            }

            shelfEntries.Clear();
            shelfEntries.AddRange(Book.Tabs[index].entries);
            for (var i = 0; i < shelfCells.Count; i++)
            {
                var show = i < shelfEntries.Count;
                shelfCells[i].root.gameObject.SetActive(show);
            }
            RefreshShop();
        }

        /// <summary>Redraws the visible shelf: icons, what each one still costs given the parts already carried.</summary>
        private void RefreshShop()
        {
            if (shelfEntries.Count == 0 || !session.Ready) return;
            for (var i = 0; i < shelfEntries.Count && i < shelfCells.Count; i++)
            {
                var entry = shelfEntries[i];
                var cell = shelfCells[i];
                var shownId = entry.displayId;
                var worth = Book.TotalCost(shownId);
                cell.Set(shownId, Book.Name(shownId), UiColors.Grade(worth), "", UiColors.Gold);

                var line = ShelfPrice(entry, out var affordable);
                cell.SetPrice(line, affordable ? UiColors.Gold : new Color(0.72f, 0.36f, 0.30f));
            }
        }

        private string ShelfPrice(ShopEntry entry, out bool affordable)
        {
            affordable = false;
            if (!entry.PriceKnown) return "-";
            if (entry.souls > 0)
            {
                affordable = Loadout.Souls >= entry.souls && Loadout.Gold >= entry.gold;
                return (entry.gold > 0 ? entry.gold + " + " : "") + entry.souls + " душ";
            }
            var gold = entry.gold;
            if (Book.RecipeFor(entry.displayId) != null)
            {
                var plan = Loadout.PlanFor(entry.displayId, true);
                if (plan.problem == null && plan.steps.Count > 0) gold = plan.gold;
            }
            affordable = Loadout.Gold >= gold;
            return gold.ToString();
        }

        private string ShelfFooter(int index)
        {
            if (index >= shelfEntries.Count) return null;
            var entry = shelfEntries[index];
            if (!entry.PriceKnown) return "Цена не определена в данных карты, купить нельзя";
            var finished = Book.RecipeFor(entry.displayId) != null;
            if (!finished) return "ЛКМ или ПКМ: купить";

            var plan = Loadout.PlanFor(entry.displayId, true);
            var line = plan.problem != null
                ? plan.problem
                : "С вашими частями: " + plan.gold + " золота, покупок: " + plan.steps.Count;
            return line + "\nЛКМ: купить всё нужное. ПКМ: только рецепт";
        }

        private void RefreshShopBag(OriginalItemInstance[] items)
        {
            for (var i = 0; i < shopBag.Length; i++)
            {
                var item = items[i];
                if (item == null) shopBag[i].Clear();
                else shopBag[i].Set(item.itemId, Book.Name(item.itemId), UiColors.Grade(Book.TotalCost(item.itemId)), "", UiColors.Gold);
            }
        }

        private void UpdateShopWindow()
        {
            var time = Time.unscaledTime;
            foreach (var slot in guideSlots) slot.view.Pulse(time);
        }

        // ---- buying --------------------------------------------------------------------------------------------

        /// <summary>
        /// Buys what a shelf item stands for. With <paramref name="withParts"/> the missing parts are bought first, like Dota's quick buy;
        /// without it only the shelf item itself (the recipe scroll of a finished item) is bought.
        /// </summary>
        private void BuyEntry(ShopEntry entry, bool withParts)
        {
            if (!session.Ready) return;
            var finished = Book.RecipeFor(entry.displayId) != null;
            if (!withParts || !finished)
            {
                var outcome = Loadout.Buy(entry);
                var name = Book.Name(entry.itemId);
                Say(outcome == BuyOutcome.Bought
                    ? (Book.IsScroll(entry.itemId) ? "Куплен рецепт: " : "Куплено: ") + name
                    : Outcome(outcome, 0, 1, null));
                return;
            }
            BuyFinished(entry.displayId);
        }

        private void BuyFinished(string finalId)
        {
            var plan = Loadout.PlanFor(finalId, true);
            if (!plan.IsPossible)
            {
                Say(plan.problem ?? "Нечего покупать");
                return;
            }
            var outcome = Loadout.BuyPlan(plan, out var bought);
            Say(Outcome(outcome, bought, plan.steps.Count, plan));
        }
    }
}

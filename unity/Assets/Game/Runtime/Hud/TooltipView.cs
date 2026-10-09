using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// The item tooltip: name, worth, what the item does in words, what it is built from, and at the bottom the stats it adds.
    /// It follows the pointer and stays inside the screen.
    /// </summary>
    public sealed class TooltipView
    {
        private const float Width = 520f;
        private const int MaxStats = 12;

        private readonly HudAssets assets;
        private readonly HudSkin skin;
        private readonly Image gradeAccent;
        private readonly RectTransform canvasRect;
        private readonly RectTransform root;
        private readonly float reservedBottom;
        private readonly Text title;
        private readonly Text kind;
        private readonly GameObject costRow;
        private readonly Image coinImage;
        private readonly Text cost;
        private readonly Text recipe;
        private readonly GameObject firstDivider;
        private readonly Text activeHeader;
        private readonly Text active;
        private readonly Text passive;
        private readonly GameObject partsBlock;
        private readonly Text partsHeader;
        private readonly RectTransform partsRow;
        private readonly Text partsNames;
        private readonly Text usedIn;
        private readonly GameObject statsDivider;
        private readonly Text[] statLines = new Text[MaxStats];
        private readonly Text footer;
        private readonly List<ItemSlotView> partSlots = new List<ItemSlotView>();

        public bool Visible => root.gameObject.activeSelf;

        public TooltipView(HudAssets assets, RectTransform canvasRect, Transform parent, float reservedBottom = 8f, HudSkin hudSkin = null)
        {
            this.assets = assets;
            skin = hudSkin ?? Resources.Load<HudSkin>("HudSkin");
            this.canvasRect = canvasRect;
            this.reservedBottom = reservedBottom;

            root = UiKit.Rect("Tooltip", parent);
            var back = root.gameObject.AddComponent<Image>();
            back.sprite = skin.shopTooltip;
            back.type = Image.Type.Sliced;
            back.raycastTarget = false;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.zero;
            root.pivot = new Vector2(0f, 1f);
            root.sizeDelta = new Vector2(Width, 100f);
            UiKit.Vertical(root.gameObject, 18, 6f, true);
            gradeAccent = UiKit.Picture(root, "Grade accent", null, Color.white);
            gradeAccent.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            gradeAccent.rectTransform.anchorMin = new Vector2(0f, 1f);
            gradeAccent.rectTransform.anchorMax = Vector2.one;
            gradeAccent.rectTransform.pivot = new Vector2(0.5f, 1f);
            gradeAccent.rectTransform.offsetMin = new Vector2(22f, -10f);
            gradeAccent.rectTransform.offsetMax = new Vector2(-22f, -7f);

            title = Line("Title", assets.titleFont, 28, HudSkin.White);
            title.fontStyle = FontStyle.Italic;
            kind = Line("Kind", assets.bodyFont, 20, HudSkin.ShopMuted);

            costRow = UiKit.Rect("Cost row", root).gameObject;
            UiKit.Sized(costRow, -1f, 26f);
            var coin = UiKit.Picture(costRow.transform, "Coin", skin.coin, Color.white);
            coinImage = coin;
            coin.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            coin.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            coin.rectTransform.pivot = new Vector2(0f, 0.5f);
            coin.rectTransform.anchoredPosition = Vector2.zero;
            coin.rectTransform.sizeDelta = new Vector2(24f, 24f);
            cost = UiKit.Label(costRow.transform, "Cost", assets.titleFont, 22, HudSkin.ShopGold, TextAnchor.MiddleLeft);
            cost.fontStyle = FontStyle.Italic;
            cost.rectTransform.anchorMin = new Vector2(0f, 0f);
            cost.rectTransform.anchorMax = new Vector2(1f, 1f);
            cost.rectTransform.offsetMin = new Vector2(32f, 0f);
            cost.rectTransform.offsetMax = Vector2.zero;
            recipe = Line("Recipe", assets.bodyFont, 20, HudSkin.ShopMuted);

            firstDivider = Divider();

            activeHeader = Line("Active header", assets.bodyFont, 20, HudSkin.ManaNumeral);
            active = Line("Active", assets.bodyFont, 20, HudSkin.White);
            passive = Line("Passive", assets.bodyFont, 20, HudSkin.White);

            partsBlock = UiKit.Rect("Parts", root).gameObject;
            UiKit.Vertical(partsBlock, 0, 4f, true);
            partsHeader = UiKit.Label(partsBlock.transform, "Parts header", assets.bodyFont, 20, HudSkin.White, TextAnchor.MiddleLeft, "Состав");
            partsRow = UiKit.Rect("Parts row", partsBlock.transform);
            UiKit.Sized(partsRow.gameObject, -1f, 48f);
            UiKit.Horizontal(partsRow.gameObject, 6f, TextAnchor.MiddleLeft);
            partsNames = UiKit.Label(partsBlock.transform, "Parts names", assets.bodyFont, 20, HudSkin.ShopMuted, TextAnchor.UpperLeft);
            usedIn = UiKit.Label(partsBlock.transform, "Used in", assets.bodyFont, 20, HudSkin.ShopMuted, TextAnchor.UpperLeft);

            statsDivider = Divider();
            for (var i = 0; i < MaxStats; i++) statLines[i] = Line("Stat " + (i + 1), assets.bodyFont, 21, HudSkin.ShopGood);
            footer = Line("Footer", assets.bodyFont, 20, HudSkin.ShopMuted);
            root.gameObject.SetActive(false);
        }

        private Text Line(string name, Font font, int size, Color color)
        {
            var label = UiKit.Label(root, name, font, size, color, TextAnchor.UpperLeft);
            return label;
        }

        private GameObject Divider()
        {
            var line = UiKit.Picture(root, "Divider", null, new Color(0.72f, 0.82f, 0.92f, 0.5f));
            UiKit.Sized(line.gameObject, -1f, 2f);
            return line.gameObject;
        }

        /// <summary>Fills the tooltip. <paramref name="owned"/> marks parts already in the bag; <paramref name="footerText"/> is a hint or a price line.</summary>
        public void Show(ItemBook book, ItemInfo info, ICollection<string> owned, string footerText)
        {
            title.text = info.name;
            gradeAccent.color = UiColors.Grade(info.totalCost);
            kind.text = info.kind ?? "";
            kind.gameObject.SetActive(!string.IsNullOrEmpty(info.kind));

            var paidInSouls = info.souls > 0;
            coinImage.sprite = paidInSouls ? skin.soul : skin.coin;
            if (paidInSouls) cost.text = (info.totalCost > 0 ? info.totalCost + " золота и " : "") + info.souls + " душ";
            else cost.text = info.totalCost >= 0 ? info.totalCost.ToString() : "цена не определена";
            costRow.SetActive(true);
            recipe.gameObject.SetActive(info.recipeCost >= 0);
            if (info.recipeCost >= 0) recipe.text = "Рецепт: " + info.recipeCost + " золота";

            var hasText = !string.IsNullOrEmpty(info.active) || !string.IsNullOrEmpty(info.passive);
            firstDivider.SetActive(hasText);
            activeHeader.gameObject.SetActive(!string.IsNullOrEmpty(info.active));
            activeHeader.text = info.activeHeader;
            active.gameObject.SetActive(!string.IsNullOrEmpty(info.active));
            active.text = info.active;
            passive.gameObject.SetActive(!string.IsNullOrEmpty(info.passive));
            passive.text = info.passive;

            ShowParts(book, info, owned);

            statsDivider.SetActive(info.stats.Count > 0);
            for (var i = 0; i < MaxStats; i++)
            {
                var show = i < info.stats.Count;
                statLines[i].gameObject.SetActive(show);
                if (!show) continue;
                statLines[i].text = info.stats[i].text;
                statLines[i].color = info.stats[i].color;
            }

            footer.gameObject.SetActive(!string.IsNullOrEmpty(footerText));
            footer.text = footerText ?? "";

            root.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }

        private void ShowParts(ItemBook book, ItemInfo info, ICollection<string> owned)
        {
            var hasParts = info.components.Count > 0;
            var hasUses = info.usedIn.Count > 0;
            partsBlock.SetActive(hasParts || hasUses);
            partsHeader.gameObject.SetActive(hasParts);
            partsRow.gameObject.SetActive(hasParts);
            partsNames.gameObject.SetActive(hasParts);
            usedIn.gameObject.SetActive(hasUses);

            while (partSlots.Count < info.components.Count)
                partSlots.Add(new ItemSlotView(assets, partsRow, "Part " + partSlots.Count, 44f, false, skin, true));
            for (var i = 0; i < partSlots.Count; i++)
            {
                var slot = partSlots[i];
                var show = i < info.components.Count;
                slot.root.gameObject.SetActive(show);
                if (!show) continue;
                var id = info.components[i];
                slot.Set(id, book.Name(id), UiColors.Grade(book.TotalCost(id)), "", UiColors.Gold);
                slot.SetMark(owned != null && owned.Contains(id) ? ItemMark.Done : ItemMark.Todo);
            }

            if (hasParts)
            {
                var names = new List<string>();
                foreach (var id in info.components) names.Add(book.Name(id));
                partsNames.text = string.Join(", ", names);
            }
            if (hasUses)
            {
                var names = new List<string>();
                foreach (var id in info.usedIn)
                {
                    if (names.Count >= 4) break;
                    names.Add(book.Name(id));
                }
                usedIn.text = "Входит в: " + string.Join(", ", names) + (info.usedIn.Count > 4 ? " и ещё " + (info.usedIn.Count - 4) : "");
            }
        }

        public void Hide() => root.gameObject.SetActive(false);

        /// <summary>Puts the tooltip beside the pointer, flipping or clamping so it never leaves the screen.</summary>
        public void Follow(Vector2 screenPoint)
        {
            if (!Visible) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out var local);
            var canvas = canvasRect.rect.size;
            var size = root.rect.size;
            // Keep rare long descriptions above the HUD, preserving cursor follow and the reserved bottom zone.
            var fit = Mathf.Min(1f, Mathf.Max(1f, canvas.y - reservedBottom - 8f) / Mathf.Max(1f, size.y));
            root.localScale = Vector3.one * fit;
            size *= fit;
            var anchorPoint = local + canvas * 0.5f;

            var x = anchorPoint.x + 26f;
            if (x + size.x > canvas.x - 8f) x = anchorPoint.x - 26f - size.x;
            var y = anchorPoint.y - 18f;
            y = Mathf.Clamp(y, Mathf.Min(size.y + reservedBottom, canvas.y - 8f), canvas.y - 8f);
            root.anchoredPosition = new Vector2(Mathf.Max(8f, x), y);
        }
    }
}

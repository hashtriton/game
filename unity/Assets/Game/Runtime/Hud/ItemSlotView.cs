using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>A square item cell: icon in a bevelled slot, a rim tinted by grade, an optional price line under it.</summary>
    public sealed class ItemSlotView
    {
        private readonly HudAssets assets;
        private readonly bool compactHud;
        private readonly bool shopStyle;
        private readonly Image gradeAccent;
        private readonly Image tagBacking;
        private readonly Image priceCoin;
        private readonly Image priceBacking;
        private readonly Image icon;
        private readonly Image frame;
        private readonly Text fallback;
        private readonly Text price;
        private readonly Text tag;
        private Color gradeColor = Color.white;
        private ItemMark mark = ItemMark.Todo;

        public readonly RectTransform root;
        public readonly float size;

        /// <summary>The item the cell shows (a finished item for a recipe scroll); null when empty.</summary>
        public string ItemId { get; private set; }

        public ItemSlotView(HudAssets assets, Transform parent, string name, float size, bool withPrice, HudSkin hudSkin = null, bool shopStyle = false)
        {
            this.assets = assets;
            this.shopStyle = shopStyle;
            compactHud = hudSkin != null && !shopStyle;
            this.size = size;
            var priceHeight = withPrice ? 24f : 0f;

            root = UiKit.Rect(name, parent);
            root.sizeDelta = new Vector2(size, size + priceHeight);
            var layout = UiKit.Sized(root.gameObject, size, size + priceHeight);
            layout.minWidth = size;
            layout.minHeight = size + priceHeight;

            if (shopStyle)
            {
                // Keep the original rectangular hit area, including the root pivot used by shelf clicks.
                var hit = UiKit.Picture(root, "Hit area", null, Color.clear, true);
                Anchor(hit.rectTransform, size, size);
            }
            var back = UiKit.Picture(root, "Slot", hudSkin != null ? hudSkin.plate : assets.slot, Color.white, !shopStyle);
            Anchor(back.rectTransform, size, size);
            if (shopStyle) back.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            if (compactHud)
            {
                UiKit.Stretch(back.rectTransform);
                back.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                back.gameObject.AddComponent<HudPlateRaycast>();
            }

            icon = UiKit.Picture(hudSkin != null ? back.transform : root, "Icon", null, Color.white);
            icon.preserveAspect = true;
            Anchor(icon.rectTransform, size - 8f, size - 8f, 4f, -4f);
            if (compactHud) UiKit.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 3f), new Vector2(size - 16f, size - 16f));
            if (shopStyle) UiKit.Place(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size - 16f, size - 16f));

            fallback = UiKit.Label(root, "Fallback", assets.bodyFont, Mathf.RoundToInt(size * 0.3f), shopStyle ? HudSkin.ShopMuted : UiColors.Muted, TextAnchor.MiddleCenter);
            Anchor(fallback.rectTransform, size, size);

            frame = UiKit.Picture(root, "Frame", hudSkin != null ? hudSkin.frame : assets.frame, Color.white);
            Anchor(frame.rectTransform, size, size);
            if (compactHud) UiKit.Stretch(frame.rectTransform);

            if (shopStyle)
            {
                gradeAccent = UiKit.Picture(root, "Grade accent", null, Color.clear);
                Anchor(gradeAccent.rectTransform, size * 0.52f, 3f, size * 0.20f, -size + 6f);
                tagBacking = UiKit.Picture(root, "Progress backing", null, new Color(0.04f, 0.09f, 0.14f, 0.97f));
                Anchor(tagBacking.rectTransform, size * 0.62f, 22f, size * 0.16f, -size + 26f);
            }

            tag = UiKit.Label(root, "Tag", assets.bodyFont, shopStyle ? 20 : Mathf.Max(11, Mathf.RoundToInt(size * 0.2f)), shopStyle ? HudSkin.ShopGood : UiColors.Good, TextAnchor.LowerCenter);
            Anchor(tag.rectTransform, size, size, 0f, -2f);
            if (shopStyle) Anchor(tag.rectTransform, size, 24f, 0f, -size + 28f);

            if (withPrice)
            {
                if (shopStyle)
                {
                    priceBacking = UiKit.Picture(root, "Price backing", hudSkin.backing, Color.white);
                    Anchor(priceBacking.rectTransform, size, priceHeight, 0f, -size - 1f);
                }
                price = UiKit.Label(root, "Price", assets.bodyFont, shopStyle ? 20 : 17, shopStyle ? HudSkin.ShopGold : UiColors.Gold, TextAnchor.MiddleCenter);
                var rect = price.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(0f, -size - 1f);
                rect.sizeDelta = new Vector2(size, priceHeight);
                if (shopStyle)
                {
                    var coin = UiKit.Picture(root, "Price coin", hudSkin.coin, Color.white);
                    priceCoin = coin;
                    Anchor(coin.rectTransform, 14f, 14f, 2f, -size - 6f);
                    rect.anchoredPosition += Vector2.right * 16f;
                    rect.sizeDelta -= Vector2.right * 16f;
                }
            }
            Clear();
        }

        private static void Anchor(RectTransform rect, float width, float height, float x = 0f, float y = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public void Clear()
        {
            ItemId = null;
            icon.enabled = false;
            fallback.text = "";
            frame.color = new Color(1f, 1f, 1f, compactHud || shopStyle ? 0.65f : 0.12f);
            gradeColor = Color.white;
            if (price != null) price.text = "";
            if (priceCoin != null) priceCoin.enabled = false;
            if (priceBacking != null) priceBacking.enabled = false;
            tag.text = "";
            if (gradeAccent != null) gradeAccent.color = Color.clear;
            if (tagBacking != null) tagBacking.gameObject.SetActive(false);
        }

        public void Set(string itemId, string displayName, Color grade, string priceText, Color priceColor)
        {
            ItemId = itemId;
            gradeColor = compactHud ? HudSkin.White : grade;
            var sprite = assets.Icon(itemId);
            icon.enabled = sprite != null;
            icon.sprite = sprite;
            fallback.text = sprite == null && !string.IsNullOrEmpty(displayName) ? displayName.Substring(0, Mathf.Min(2, displayName.Length)) : "";
            frame.color = shopStyle ? HudSkin.White : gradeColor;
            if (gradeAccent != null) gradeAccent.color = grade;
            if (price != null)
            {
                price.text = priceText;
                price.color = priceColor;
                if (priceCoin != null) priceCoin.enabled = !string.IsNullOrEmpty(priceText);
                if (priceBacking != null) priceBacking.enabled = !string.IsNullOrEmpty(priceText);
            }
            SetMark(ItemMark.Todo);
        }

        /// <summary>Guide state: done items fade, the next one to buy glows (see <see cref="Pulse"/>).</summary>
        public void SetMark(ItemMark next)
        {
            mark = next;
            icon.color = next == ItemMark.Done ? UiColors.Dim : Color.white;
            tag.text = next == ItemMark.Done ? "есть" : "";
            if (tagBacking != null) tagBacking.gameObject.SetActive(next == ItemMark.Done);
            if (next != ItemMark.Next) frame.color = ItemId == null ? new Color(1f, 1f, 1f, compactHud || shopStyle ? 0.65f : 0.12f) : shopStyle ? HudSkin.White : gradeColor;
        }

        public void Pulse(float time)
        {
            if (mark != ItemMark.Next) return;
            var glow = 0.65f + 0.35f * Mathf.Sin(time * 4f);
            frame.color = shopStyle ? Color.Lerp(HudSkin.White, HudSkin.Cyan, 0.55f + 0.45f * glow) : Color.Lerp(gradeColor, UiColors.Gold, 0.55f + 0.45f * glow);
        }

        public void SetPrice(string text, Color color)
        {
            if (price == null) return;
            price.text = text;
            price.color = color;
            if (priceCoin != null) priceCoin.enabled = !string.IsNullOrEmpty(text);
            if (priceBacking != null) priceBacking.enabled = !string.IsNullOrEmpty(text);
        }
    }
}

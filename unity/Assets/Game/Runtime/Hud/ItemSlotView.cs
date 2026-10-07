using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>A square item cell: icon in a bevelled slot, a rim tinted by grade, an optional price line under it.</summary>
    public sealed class ItemSlotView
    {
        private readonly HudAssets assets;
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

        public ItemSlotView(HudAssets assets, Transform parent, string name, float size, bool withPrice)
        {
            this.assets = assets;
            this.size = size;
            var priceHeight = withPrice ? 24f : 0f;

            root = UiKit.Rect(name, parent);
            root.sizeDelta = new Vector2(size, size + priceHeight);
            var layout = UiKit.Sized(root.gameObject, size, size + priceHeight);
            layout.minWidth = size;
            layout.minHeight = size + priceHeight;

            var back = UiKit.Picture(root, "Slot", assets.slot, Color.white, true);
            Anchor(back.rectTransform, size, size);

            icon = UiKit.Picture(root, "Icon", null, Color.white);
            icon.preserveAspect = true;
            Anchor(icon.rectTransform, size - 8f, size - 8f, 4f, -4f);

            fallback = UiKit.Label(root, "Fallback", assets.bodyFont, Mathf.RoundToInt(size * 0.3f), UiColors.Muted, TextAnchor.MiddleCenter);
            Anchor(fallback.rectTransform, size, size);

            frame = UiKit.Picture(root, "Frame", assets.frame, Color.white);
            Anchor(frame.rectTransform, size, size);

            tag = UiKit.Label(root, "Tag", assets.bodyFont, Mathf.Max(11, Mathf.RoundToInt(size * 0.2f)), UiColors.Good, TextAnchor.LowerCenter);
            Anchor(tag.rectTransform, size, size, 0f, -2f);

            if (withPrice)
            {
                price = UiKit.Label(root, "Price", assets.bodyFont, 17, UiColors.Gold, TextAnchor.MiddleCenter);
                var rect = price.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(0f, -size - 1f);
                rect.sizeDelta = new Vector2(size, priceHeight);
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
            frame.color = new Color(1f, 1f, 1f, 0.12f);
            gradeColor = Color.white;
            if (price != null) price.text = "";
            tag.text = "";
        }

        public void Set(string itemId, string displayName, Color grade, string priceText, Color priceColor)
        {
            ItemId = itemId;
            gradeColor = grade;
            var sprite = assets.Icon(itemId);
            icon.enabled = sprite != null;
            icon.sprite = sprite;
            fallback.text = sprite == null && !string.IsNullOrEmpty(displayName) ? displayName.Substring(0, Mathf.Min(2, displayName.Length)) : "";
            frame.color = grade;
            if (price != null)
            {
                price.text = priceText;
                price.color = priceColor;
            }
            SetMark(ItemMark.Todo);
        }

        /// <summary>Guide state: done items fade, the next one to buy glows (see <see cref="Pulse"/>).</summary>
        public void SetMark(ItemMark next)
        {
            mark = next;
            icon.color = next == ItemMark.Done ? UiColors.Dim : Color.white;
            tag.text = next == ItemMark.Done ? "есть" : "";
            if (next != ItemMark.Next) frame.color = ItemId == null ? new Color(1f, 1f, 1f, 0.12f) : gradeColor;
        }

        public void Pulse(float time)
        {
            if (mark != ItemMark.Next) return;
            var glow = 0.65f + 0.35f * Mathf.Sin(time * 4f);
            frame.color = Color.Lerp(gradeColor, UiColors.Gold, 0.55f + 0.45f * glow);
        }

        public void SetPrice(string text, Color color)
        {
            if (price == null) return;
            price.text = text;
            price.color = color;
        }
    }
}

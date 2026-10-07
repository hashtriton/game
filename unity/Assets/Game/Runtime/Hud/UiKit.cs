using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    /// <summary>Colours of the interface: aged metal and bone, with muted accents instead of bright ones.</summary>
    public static class UiColors
    {
        public static readonly Color Text = new Color(0.84f, 0.80f, 0.70f);
        public static readonly Color Muted = new Color(0.56f, 0.53f, 0.48f);
        public static readonly Color Gold = new Color(0.90f, 0.72f, 0.32f);
        public static readonly Color Bronze = new Color(0.62f, 0.47f, 0.26f);
        public static readonly Color Good = new Color(0.52f, 0.78f, 0.46f);
        public static readonly Color Bad = new Color(0.86f, 0.34f, 0.28f);
        public static readonly Color Health = new Color(0.66f, 0.12f, 0.10f);
        public static readonly Color Mana = new Color(0.16f, 0.32f, 0.70f);
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.42f);

        /// <summary>Rim colour by what an item is worth: steel, bronze, cold silver, violet, ember.</summary>
        public static Color Grade(int value)
        {
            if (value < 100) return new Color(0.50f, 0.50f, 0.52f);
            if (value < 400) return new Color(0.66f, 0.49f, 0.27f);
            if (value < 900) return new Color(0.52f, 0.64f, 0.78f);
            if (value < 1700) return new Color(0.64f, 0.46f, 0.80f);
            return new Color(0.94f, 0.60f, 0.22f);
        }
    }

    /// <summary>Hands pointer events of a UI element to callbacks, so views need no custom classes of their own.</summary>
    public sealed class PointerRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Action<PointerEventData> entered;
        public Action<PointerEventData> exited;
        public Action<PointerEventData> clicked;

        public void OnPointerEnter(PointerEventData eventData) => entered?.Invoke(eventData);
        public void OnPointerExit(PointerEventData eventData) => exited?.Invoke(eventData);
        public void OnPointerClick(PointerEventData eventData) => clicked?.Invoke(eventData);
    }

    /// <summary>Small factory for the pieces the interface is built from. Sizes are in canvas pixels (1920 x 1080).</summary>
    public static class UiKit
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static Image Picture(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = raycast;
            return image;
        }

        public static Text Label(Transform parent, string name, Font font, int size, Color color, TextAnchor anchor, string text = "")
        {
            var rect = Rect(name, parent);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.text = text;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = true;
            return label;
        }

        public static PointerRelay Relay(GameObject target)
        {
            var relay = target.GetComponent<PointerRelay>();
            return relay != null ? relay : target.AddComponent<PointerRelay>();
        }

        public static VerticalLayoutGroup Vertical(GameObject target, int padding, float spacing, bool fitHeight)
        {
            var layout = target.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            if (fitHeight) target.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return layout;
        }

        public static HorizontalLayoutGroup Horizontal(GameObject target, float spacing, TextAnchor alignment)
        {
            var layout = target.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return layout;
        }

        public static LayoutElement Sized(GameObject target, float width = -1f, float height = -1f)
        {
            var element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            if (width >= 0f) element.preferredWidth = width;
            if (height >= 0f) element.preferredHeight = height;
            return element;
        }
    }
}

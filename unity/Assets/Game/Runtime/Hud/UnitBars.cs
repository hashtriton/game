using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>Health bars over the heads of units, drawn on the canvas so they keep one size at any zoom.</summary>
    // After the camera has moved, so the bars do not lag a frame behind the units.
    [DefaultExecutionOrder(100)]
    public sealed class UnitBars : MonoBehaviour
    {
        private sealed class Bar
        {
            public RectTransform root;
            public Image fill;
        }

        public Camera view;
        public HudAssets assets;
        public RectTransform canvasRect;
        public Vector2 size = new Vector2(82f, 8f);

        private readonly Dictionary<Unit, Bar> bars = new Dictionary<Unit, Bar>();
        private readonly List<Unit> gone = new List<Unit>();

        private void LateUpdate()
        {
            if (view == null || canvasRect == null) return;

            foreach (var unit in Unit.All)
            {
                if (!unit.IsAlive) continue;
                if (!bars.TryGetValue(unit, out var bar)) bars[unit] = bar = Create(unit);

                var top = unit.transform.position + Vector3.up * (unit.height + 0.35f);
                var screen = view.WorldToScreenPoint(top);
                var visible = screen.z > 0f;
                bar.root.gameObject.SetActive(visible);
                if (!visible) continue;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out var local);
                bar.root.anchoredPosition = local;
                bar.fill.fillAmount = Mathf.Clamp01(unit.health / unit.MaxHealth);
            }

            gone.Clear();
            foreach (var pair in bars)
                if (pair.Key == null || !pair.Key.IsAlive) gone.Add(pair.Key);
            foreach (var unit in gone)
            {
                if (bars.TryGetValue(unit, out var bar) && bar.root != null) Destroy(bar.root.gameObject);
                bars.Remove(unit);
            }
        }

        private Bar Create(Unit unit)
        {
            var root = UiKit.Rect("Bar " + unit.displayName, transform);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0f);
            root.sizeDelta = size;

            var skin = Resources.Load<HudSkin>("HudSkin");
            var back = UiKit.Picture(root, "Back", skin.segment, new Color(0.035f, 0.055f, 0.075f, 0.94f));
            UiKit.Stretch(back.rectTransform);
            var fill = UiKit.Picture(root, "Fill", skin.segment, unit.faction == Faction.Hero ? new Color(0.5f, 0.94f, 0.70f) : new Color(1f, 0.22f, 0.17f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            UiKit.Stretch(fill.rectTransform, 1f, 1f, 1f, 1f);
            return new Bar { root = root, fill = fill };
        }
    }
}

using UnityEngine;

namespace Game
{
    // Matches the plate polygon in tools/art/gen_hud_skin.py without making textures CPU-readable.
    public sealed class HudPlateRaycast : MonoBehaviour, ICanvasRaycastFilter
    {
        private static readonly Vector2[] Outline =
        {
            new Vector2(0.17f, 2f / 128f), new Vector2(184f / 192f, 2f / 128f),
            new Vector2(190f / 192f, 8f / 128f), new Vector2(0.83f, 126f / 128f),
            new Vector2(8f / 192f, 126f / 128f), new Vector2(2f / 192f, 120f / 128f)
        };

        private RectTransform rect;

        private void Awake() => rect = (RectTransform)transform;

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out var local)) return false;
            var bounds = rect.rect;
            if (bounds.width <= 0f || bounds.height <= 0f) return false;
            var point = new Vector2((local.x - bounds.xMin) / bounds.width, (bounds.yMax - local.y) / bounds.height);
            for (var i = 0; i < Outline.Length; i++)
            {
                var a = Outline[i];
                var edge = Outline[(i + 1) % Outline.Length] - a;
                var offset = point - a;
                if (edge.x * offset.y - edge.y * offset.x < 0f) return false;
            }
            return true;
        }
    }
}

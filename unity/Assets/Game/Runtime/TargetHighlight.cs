using UnityEngine;

namespace Game
{
    /// <summary>Red ring under the enemy (or barrel) the cursor is over, so the player knows a click will attack it.</summary>
    public sealed class TargetHighlight : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public Color color = new Color(1f, 0.22f, 0.16f, 0.95f);
        // The ring texture spans 82% of its quad.
        public float quadToRing = 0.82f;

        private Renderer ringRenderer;
        private MaterialPropertyBlock block;
        private Targetable target;

        private void Awake()
        {
            ringRenderer = GetComponentInChildren<Renderer>(true);
            block = new MaterialPropertyBlock();
            block.SetColor(BaseColor, color);
            ringRenderer.SetPropertyBlock(block);
            ringRenderer.enabled = false;
        }

        public void Show(Targetable next)
        {
            target = next;
            ringRenderer.enabled = next != null;
        }

        public void Hide() => Show(null);

        private void LateUpdate()
        {
            if (target == null || !target.IsAlive)
            {
                ringRenderer.enabled = false;
                return;
            }
            var diameter = (target.radius + 0.18f) * 2f / quadToRing;
            transform.position = target.Position + Vector3.up * 0.09f;
            transform.localScale = new Vector3(diameter, 1f, diameter);
        }
    }
}

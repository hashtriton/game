using UnityEngine;

namespace Game
{
    /// <summary>Short ring flash on the ground where a move order was given.</summary>
    public sealed class ClickMarker : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public float life = 0.55f;
        public float startSize = 1.5f;
        public float endSize = 0.7f;
        public Color color = new Color(0.45f, 1f, 0.75f, 1f);

        private Renderer markerRenderer;
        private MaterialPropertyBlock block;
        private float age;

        private void Awake()
        {
            markerRenderer = GetComponentInChildren<Renderer>(true);
            block = new MaterialPropertyBlock();
            gameObject.SetActive(false);
        }

        public void Show(Vector3 position)
        {
            transform.position = position + Vector3.up * 0.08f;
            age = 0f;
            gameObject.SetActive(true);
            Apply(0f);
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= life)
            {
                gameObject.SetActive(false);
                return;
            }
            Apply(age / life);
        }

        private void Apply(float t)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, t);
            var tint = color;
            tint.a = 1f - t * t;
            block.SetColor(BaseColor, tint);
            markerRenderer.SetPropertyBlock(block);
        }
    }
}

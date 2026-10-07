using UnityEngine;

namespace Game
{
    /// <summary>Fire-like intensity and colour wobble for torches and braziers.</summary>
    [RequireComponent(typeof(Light))]
    public sealed class FlickerLight : MonoBehaviour
    {
        public float amplitude = 0.28f;
        public float speed = 7f;

        private Light source;
        private float baseIntensity;
        private Color baseColor;
        private float seed;

        private void Awake()
        {
            source = GetComponent<Light>();
            baseIntensity = source.intensity;
            baseColor = source.color;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            var t = Time.time * speed + seed;
            // Two octaves of noise: slow breathing plus fast crackle.
            var wobble = (Mathf.PerlinNoise(t, seed) - 0.5f) * 1.6f + (Mathf.PerlinNoise(t * 3.1f, seed + 7f) - 0.5f) * 0.6f;
            source.intensity = baseIntensity * (1f + wobble * amplitude);
            source.color = Color.Lerp(baseColor, baseColor * new Color(1f, 0.82f, 0.62f), Mathf.Clamp01(0.5f - wobble * 0.5f));
        }
    }
}

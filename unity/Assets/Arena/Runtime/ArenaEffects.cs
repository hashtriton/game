using UnityEngine;
using UnityEngine.Rendering;

namespace Arena
{
    /// <summary>Short-lived world feedback with a serialized, build-safe material.</summary>
    public sealed class ArenaEffects : MonoBehaviour
    {
        private Renderer effectRenderer;
        private MaterialPropertyBlock properties;
        private Color color;
        private float lifetime;
        private float remaining;
        private float expansion;
        private Vector3 initialScale;
        private bool fade;

        public static ArenaEffects Ring(Transform parent, Material material, Vector3 position,
            float radius, Color color, float duration, float expansion = 0f, bool fade = true)
        {
            return Arc(parent, material, position, radius, color, duration, 360f,
                Vector3.forward, expansion, fade);
        }

        public static ArenaEffects Arc(Transform parent, Material material, Vector3 position,
            float radius, Color color, float duration, float degrees, Vector3 direction,
            float expansion = 0f, bool fade = true)
        {
            var obj = new GameObject("Arena feedback");
            obj.transform.SetParent(parent, false);
            obj.transform.position = position + Vector3.up * 0.07f;
            if (direction.sqrMagnitude > 0.001f) obj.transform.rotation = Quaternion.LookRotation(direction);
            var line = obj.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = false;
            line.loop = degrees >= 359f;
            line.widthMultiplier = 0.09f;
            line.numCapVertices = 3;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = degrees >= 359f ? 64 : 25;
            for (var i = 0; i < line.positionCount; i++)
            {
                var progress = (float)i / (line.loop ? line.positionCount : line.positionCount - 1);
                var angle = Mathf.Deg2Rad * (-degrees * 0.5f + progress * degrees);
                line.SetPosition(i, new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius));
            }
            var effect = obj.AddComponent<ArenaEffects>();
            effect.Initialize(line, color, duration, expansion, fade);
            return effect;
        }

        public static GameObject Orb(Transform parent, Material material, Vector3 position,
            float diameter, Color color)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Enemy projectile";
            obj.transform.SetParent(parent, false);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * diameter;
            var collider = obj.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            var renderer = obj.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ApplyColor(renderer, new MaterialPropertyBlock(), color);
            return obj;
        }

        private void Initialize(Renderer renderer, Color effectColor, float duration,
            float scaleExpansion, bool fadeOverLifetime)
        {
            effectRenderer = renderer;
            properties = new MaterialPropertyBlock();
            color = effectColor;
            remaining = lifetime = Mathf.Max(0.05f, duration);
            expansion = scaleExpansion;
            initialScale = transform.localScale;
            fade = fadeOverLifetime;
            ApplyColor(effectRenderer, properties, color);
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }
            var progress = 1f - remaining / lifetime;
            transform.localScale = initialScale * (1f + expansion * progress);
            var tint = color;
            if (fade) tint.a *= 1f - progress;
            ApplyColor(effectRenderer, properties, tint);
        }

        private static void ApplyColor(Renderer renderer, MaterialPropertyBlock properties, Color color)
        {
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            properties.SetColor("_EmissionColor", color * 0.8f);
            renderer.SetPropertyBlock(properties);
        }
    }
}

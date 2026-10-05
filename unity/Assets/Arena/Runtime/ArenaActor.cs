using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    /// <summary>Visual presentation only. The run and enemy simulation own health.</summary>
    public sealed class ArenaActor : MonoBehaviour
    {
        private Animation legacyAnimation;
        private Transform visual;
        private string playingClip;
        private float actionRemaining;
        private float deathRemaining;
        private bool hasDeathClip;
        private bool wantsToMove;

        public bool IsDead { get; private set; }
        public bool IsHero { get; private set; }
        public float Radius { get; private set; }

        public void Initialize(GameObject prefab, float height, float radius, bool isHero,
            Material fallbackMaterial)
        {
            Radius = radius;
            IsHero = isHero;
            // Imported clips can animate their own root scale. Keep normalization on a
            // separate parent which no imported animation binding can overwrite.
            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);
            var model = prefab != null ? Instantiate(prefab, visual) : CreateFallback(fallbackMaterial);
            model.name = "Model";
            model.transform.SetParent(visual, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // Movement and hit tests use the actor root, never the imported skeleton or collider.
            foreach (var collider in model.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var body in model.GetComponentsInChildren<Rigidbody>(true))
                body.isKinematic = true;

            legacyAnimation = model.GetComponentInChildren<Animation>(true);
            if (legacyAnimation != null)
            {
                SetLoop("Idle");
                SetLoop("Run");
                SetOnce("Attack");
                hasDeathClip = legacyAnimation.GetClip("Death") != null;
                if (hasDeathClip) legacyAnimation["Death"].wrapMode = WrapMode.ClampForever;
                legacyAnimation.cullingType = AnimationCullingType.AlwaysAnimate;
                if (legacyAnimation.GetClip("Idle") != null)
                {
                    // Sample immediately, without a crossfade, before reading geometry.
                    legacyAnimation.Play("Idle");
                    legacyAnimation["Idle"].time = 0f;
                    legacyAnimation.Sample();
                    playingClip = "Idle";
                }
            }

            if (TryMeasureGeometry(model, out var bounds) && bounds.size.y > 0.001f)
            {
                var scale = height / bounds.size.y;
                visual.localScale = Vector3.one * scale;
                visual.localPosition = Vector3.up * (-bounds.min.y * scale);
            }
        }

        private bool TryMeasureGeometry(GameObject model, out Bounds bounds)
        {
            bounds = default;
            var found = false;
            var vertices = new List<Vector3>();
            var baked = new Mesh();
            try
            {
                foreach (var renderer in model.GetComponentsInChildren<Renderer>())
                {
                    if (!renderer.enabled) continue;
                    if (renderer is SkinnedMeshRenderer skinned)
                    {
                        if (skinned.sharedMesh == null) continue;
                        skinned.updateWhenOffscreen = true;
                        // In Unity 6, true compensates the renderer Transform scale.
                        // False already scales the vertices and would apply an imported
                        // FBX scale (100 here) twice when transformed to Visual space.
                        baked.Clear();
                        skinned.BakeMesh(baked, true);
                        baked.GetVertices(vertices);
                        var toVisual = visual.worldToLocalMatrix * skinned.transform.localToWorldMatrix;
                        foreach (var vertex in vertices)
                            IncludePoint(toVisual.MultiplyPoint3x4(vertex), ref bounds, ref found);
                    }
                    else
                    {
                        var filter = renderer.GetComponent<MeshFilter>();
                        if (filter == null || filter.sharedMesh == null) continue;
                        var toVisual = visual.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                        // Static meshes need no CPU-readable import flag to read bounds.
                        var local = filter.sharedMesh.bounds;
                        for (var corner = 0; corner < 8; corner++)
                        {
                            var point = new Vector3(
                                (corner & 1) == 0 ? local.min.x : local.max.x,
                                (corner & 2) == 0 ? local.min.y : local.max.y,
                                (corner & 4) == 0 ? local.min.z : local.max.z);
                            IncludePoint(toVisual.MultiplyPoint3x4(point), ref bounds, ref found);
                        }
                    }
                }
            }
            finally
            {
                Destroy(baked);
            }
            return found;
        }

        private static void IncludePoint(Vector3 point, ref Bounds bounds, ref bool found)
        {
            if (!found)
            {
                bounds = new Bounds(point, Vector3.zero);
                found = true;
            }
            else bounds.Encapsulate(point);
        }

        public void Face(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (IsDead || direction.sqrMagnitude < 0.0001f) return;
            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, 1080f * deltaTime);
        }

        public void SetMoving(bool moving)
        {
            wantsToMove = moving;
            if (!IsDead && actionRemaining <= 0f) Play(moving ? "Run" : "Idle", false);
        }

        public void Attack(float duration)
        {
            if (IsDead) return;
            actionRemaining = Mathf.Max(0.12f, duration);
            if (legacyAnimation != null && legacyAnimation["Attack"] != null)
                legacyAnimation["Attack"].speed = legacyAnimation["Attack"].length / actionRemaining;
            Play("Attack", true);
        }

        public void Die()
        {
            if (IsDead) return;
            IsDead = true;
            deathRemaining = IsHero ? 0f : 0.85f;
            Play("Death", true);
            if (!hasDeathClip && visual != null)
                visual.localRotation = Quaternion.Euler(0f, 0f, 78f);
        }

        private void Update()
        {
            var wasActing = actionRemaining > 0f;
            actionRemaining = Mathf.Max(0f, actionRemaining - Time.deltaTime);
            if (wasActing && actionRemaining <= 0f && !IsDead)
                Play(wantsToMove ? "Run" : "Idle", false);
            if (!IsDead || IsHero) return;
            deathRemaining -= Time.deltaTime;
            if (deathRemaining <= 0f) Destroy(gameObject);
        }

        private GameObject CreateFallback(Material material)
        {
            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.transform.SetParent(transform, false);
            if (material != null) fallback.GetComponent<Renderer>().sharedMaterial = material;
            return fallback;
        }

        private void SetLoop(string clip)
        {
            if (legacyAnimation[clip] != null) legacyAnimation[clip].wrapMode = WrapMode.Loop;
        }

        private void SetOnce(string clip)
        {
            if (legacyAnimation[clip] != null) legacyAnimation[clip].wrapMode = WrapMode.Once;
        }

        private void Play(string clip, bool restart)
        {
            if (legacyAnimation == null || legacyAnimation.GetClip(clip) == null) return;
            if (!restart && playingClip == clip && legacyAnimation.IsPlaying(clip)) return;
            playingClip = clip;
            if (restart) legacyAnimation[clip].time = 0f;
            legacyAnimation.CrossFade(clip, 0.08f);
        }
    }
}

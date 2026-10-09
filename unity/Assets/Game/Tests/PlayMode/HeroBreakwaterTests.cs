using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class HeroBreakwaterTests
    {
        private HeroController controller;
        private Unit hero;
        private Animation animation;

        [UnitySetUp]
        public IEnumerator Load_built_arena()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Game/Scenes/Arena.unity", new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            yield return null;
            controller = Object.FindAnyObjectByType<HeroController>();
            hero = controller.Unit;
            animation = hero.GetComponentInChildren<Animation>();
            controller.Stop();
            controller.enabled = false;
#if UNITY_EDITOR
            Assert.AreEqual("Assets/Game/Heroes/Breakwater/Breakwater.prefab", UnityEditor.AssetDatabase.GetAssetPath(hero.modelPrefab));
#endif
        }

        [UnityTest]
        public IEnumerator Merged_model_mounts_at_player_height_with_six_materials()
        {
            animation.enabled = false;
            animation.GetClip("Idle").SampleAnimation(animation.gameObject, 0);
            Assert.That(hero.height, Is.EqualTo(2.4f).Within(0.0001f));
            Assert.That(hero.radius, Is.EqualTo(0.4f).Within(0.0001f));
            var model = hero.transform.Find("Visual/Model");
            Assert.AreEqual(1, model.GetComponentsInChildren<Renderer>().Length);
            var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.IsNotNull(renderer);
            Assert.AreEqual(6, renderer.sharedMesh.subMeshCount);
            Assert.AreEqual(6, renderer.sharedMaterials.Distinct().Count());
            Assert.IsTrue(renderer.sharedMaterials.All(m => m != null));
            var bounds = GeometryBounds();
            Assert.That(bounds.size.y, Is.EqualTo(2.4f).Within(0.048f));
            Assert.That(bounds.min.y - hero.transform.position.y, Is.EqualTo(0).Within(0.025f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Merged_material_regions_preserve_the_P5_surface_areas()
        {
#if UNITY_EDITOR
            var root = new GameObject("Unmerged material reference");
            try
            {
                var reference = root.AddComponent<Arena.ArenaActor>();
                reference.Initialize(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/P5/HeroProbeP5.prefab"), 2.4f, .4f, true, null);
                reference.enabled = false;
                var sourceAnimation = root.GetComponentInChildren<Animation>();
                sourceAnimation.enabled = false;
                sourceAnimation.GetClip("Idle").SampleAnimation(sourceAnimation.gameObject, 0);
                animation.enabled = false;
                animation.GetClip("Idle").SampleAnimation(animation.gameObject, 0);
                var source = MaterialAreas(root);
                var actual = MaterialAreas(hero.gameObject);
                CollectionAssert.AreEquivalent(source.Keys, actual.Keys);
                foreach (var pair in source)
                    Assert.That(actual[pair.Key], Is.EqualTo(pair.Value).Within(.0001f), pair.Key + " material area");
            }
            finally { Object.Destroy(root); }
#endif
            yield return null;
        }

        private static Dictionary<string, float> MaterialAreas(GameObject root)
        {
            var areas = new Dictionary<string, float>();
            var mesh = new Mesh();
            try
            {
                foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    mesh.Clear();
                    renderer.BakeMesh(mesh, true);
                    var points = mesh.vertices.Select(v => renderer.transform.TransformPoint(v)).ToArray();
                    for (var slot = 0; slot < mesh.subMeshCount; slot++)
                    {
                        var name = renderer.sharedMaterials[slot].name;
                        if (!areas.ContainsKey(name)) areas[name] = 0;
                        var indices = mesh.GetTriangles(slot);
                        for (var i = 0; i < indices.Length; i += 3)
                            areas[name] += Vector3.Cross(points[indices[i + 1]] - points[indices[i]], points[indices[i + 2]] - points[indices[i]]).magnitude * .5f;
                    }
                }
            }
            finally { Object.Destroy(mesh); }
            return areas;
        }

        [UnityTest]
        public IEnumerator All_four_legacy_clips_resolve_the_mounted_skeleton()
        {
            foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
            {
                var clip = animation.GetClip(name);
                Assert.IsNotNull(clip, name);
                Assert.IsTrue(clip.legacy, name);
#if UNITY_EDITOR
                foreach (var binding in UnityEditor.AnimationUtility.GetCurveBindings(clip))
                    Assert.IsTrue(binding.path.Length == 0 || animation.transform.Find(binding.path) != null, name + ": " + binding.path);
#endif
                Assert.AreEqual(name == "Death" ? WrapMode.ClampForever : name == "Attack" ? WrapMode.Once : WrapMode.Loop, animation[name].wrapMode);
            }
            hero.Actor.SetMoving(true);
            yield return new WaitForSeconds(.15f);
            Assert.IsTrue(animation.IsPlaying("Run"));
            Assert.Greater(animation["Run"].time, .05f);
        }

        [UnityTest]
        public IEnumerator Attack_order_damages_a_creep_at_the_hero_windup()
        {
            var creep = Unit.All.First(u => u.faction == Faction.Creep);
            foreach (var other in Unit.All.Where(u => u.faction == Faction.Creep))
            {
                var ai = other.GetComponent<CreepAi>();
                if (ai != null) ai.enabled = false;
            }
            creep.transform.position = controller.map.FindNearestWalkable(hero.transform.position + Vector3.forward, creep.pathRadius);
            creep.health = creep.MaxHealth;
            creep.healthRegen = 0;
            var before = creep.health;
            controller.enabled = true;
            controller.OrderAttack(creep);
            var deadline = Time.time + 2f;
            while (!hero.IsSwinging && Time.time < deadline) yield return null;
            Assert.IsTrue(hero.IsSwinging);
            var start = Time.time;
            while (creep.health >= before && Time.time < deadline) yield return null;
            Assert.Less(creep.health, before);
            var swing = Mathf.Min(hero.AttackInterval, .9f);
            Assert.That(Time.time - start, Is.EqualTo(swing * hero.swingFraction).Within(.10f));
            controller.Stop();
            controller.enabled = false;
        }

        [UnityTest]
        public IEnumerator Death_retains_the_last_pose_above_the_floor()
        {
            hero.ApplyDamage(100000f, null);
            yield return new WaitForSeconds(.15f);
            Assert.IsTrue(hero.Actor.IsDead);
            Assert.IsTrue(animation.IsPlaying("Death"));
            Assert.AreEqual(WrapMode.ClampForever, animation["Death"].wrapMode);
            yield return new WaitForSeconds(animation["Death"].length + .1f);
            var before = GeometryBounds();
            yield return new WaitForSeconds(.2f);
            var after = GeometryBounds();
            Assert.Less(Vector3.Distance(before.center, after.center), .003f);
            Assert.Less(Vector3.Distance(before.size, after.size), .003f);
            Assert.Greater(after.min.y - hero.transform.position.y, -.02f);
        }

        private Bounds GeometryBounds()
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var found = false;
            var bounds = new Bounds();
            try
            {
                foreach (var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    mesh.Clear();
                    renderer.BakeMesh(mesh, true);
                    mesh.GetVertices(vertices);
                    foreach (var vertex in vertices)
                    {
                        var point = renderer.transform.TransformPoint(vertex);
                        if (found) bounds.Encapsulate(point);
                        else { bounds = new Bounds(point, Vector3.zero); found = true; }
                    }
                }
            }
            finally { Object.Destroy(mesh); }
            Assert.IsTrue(found);
            return bounds;
        }
    }
}

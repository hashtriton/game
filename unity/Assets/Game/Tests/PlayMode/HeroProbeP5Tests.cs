using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class HeroProbeP5Tests
    {
        private Unit hero;
        private Animation animation;
        private Scene probe;
        private Scene previous;

        [UnitySetUp]
        public IEnumerator Mount_through_unit_start()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/P5/HeroProbeP5.prefab");
            Assert.IsNotNull(prefab, "The separate P5 probe prefab must exist.");
            previous = SceneManager.GetActiveScene();
            probe = SceneManager.CreateScene("HeroProbeP5Test");
            SceneManager.SetActiveScene(probe);
            var root = new GameObject("ProbeHeroP5");
            root.SetActive(false);
            hero = root.AddComponent<Unit>();
            hero.faction = Faction.Hero;
            hero.modelPrefab = prefab;
            hero.height = 2.4f;
            hero.radius = 0.4f;
            hero.baseAttackInterval = 1.35f;
            root.SetActive(true);
            yield return null;
            animation = hero.GetComponentInChildren<Animation>();
            Assert.IsNotNull(animation);
#else
            yield return null;
#endif
        }

        [UnityTearDown]
        public IEnumerator Remove_probe_scene()
        {
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (probe.IsValid() && probe.isLoaded) yield return SceneManager.UnloadSceneAsync(probe);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Four_legacy_clips_resolve_every_binding()
        {
#if UNITY_EDITOR
            foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
            {
                var clip = animation.GetClip(name);
                Assert.IsNotNull(clip, name);
                Assert.IsTrue(clip.legacy, name);
                foreach (var binding in UnityEditor.AnimationUtility.GetCurveBindings(clip))
                    Assert.IsTrue(binding.path.Length == 0 || animation.transform.Find(binding.path) != null, name + ": " + binding.path);
                var donor = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Arena/Generated/Warrior_" + name + ".anim");
                foreach (var path in UnityEditor.AnimationUtility.GetCurveBindings(donor).Select(b => b.path).Distinct())
                    Assert.IsTrue(path.Length == 0 || animation.transform.Find(path) != null, path);
            }
            var materials = hero.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Distinct().ToArray();
            Assert.AreEqual(6, materials.Length);
            Assert.IsTrue(materials.All(m => m != null && m.shader.name == "Universal Render Pipeline/Lit"));
            Assert.IsTrue(materials.All(m => m.GetTexture("_BaseMap") == null));
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator Actor_normalizes_idle_to_2_4_metres_with_grounded_feet()
        {
            animation.GetClip("Idle").SampleAnimation(animation.gameObject, 0);
            var bounds = GeometryBounds(false);
            Assert.That(bounds.size.y, Is.EqualTo(2.4f).Within(0.048f));
            Assert.That(GeometryBounds(true).min.y, Is.EqualTo(0f).Within(0.025f));
            Assert.AreEqual(83, hero.GetComponentsInChildren<SkinnedMeshRenderer>().Length);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Four_clip_sweep_checks_standing_feet_and_all_death_geometry()
        {
            foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
            {
                for (var sample = 0; sample <= 96; sample++)
                {
                    animation.GetClip(name).SampleAnimation(animation.gameObject, animation.GetClip(name).length * sample / 96f);
                    var bounds = GeometryBounds(false);
                    Assert.Less(bounds.size.x, 5f, name + " width");
                    Assert.Less(bounds.size.y, 4f, name + " height");
                    Assert.Less(bounds.size.z, 5f, name + " depth");
                    Assert.Less(bounds.center.magnitude, 3f, name + " displacement");
                    var feet = GeometryBounds(true);
                    Assert.Greater(feet.min.y, -0.025f, name + " feet penetration at " + sample);
                    if (name == "Idle" || name == "Attack") Assert.Less(feet.min.y, 0.025f, name + " equipment must not lift the feet at " + sample);
                    Assert.Greater(bounds.min.y, -0.02f, name + " all geometry penetration at " + sample);
                }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Actor_run_advances_and_loops()
        {
            var idleTime = animation["Idle"].time;
            yield return new WaitForSeconds(0.15f);
            Assert.Greater(animation["Idle"].time, idleTime + 0.08f);
            hero.Actor.SetMoving(true);
            yield return new WaitForSeconds(0.12f);
            Assert.IsTrue(animation.IsPlaying("Run"));
            Assert.AreEqual(WrapMode.Loop, animation["Run"].wrapMode);
            var fist = animation.transform.Find("CharacterArmature/Root/Body/Hips/Abdomen/Torso/Shoulder.R/UpperArm.R/LowerArm.R/Fist.R");
            var before = fist.position;
            yield return new WaitForSeconds(0.18f);
            Assert.Greater(Vector3.Distance(before, fist.position), 0.015f);
            yield return new WaitForSeconds(animation["Run"].length);
            Assert.IsTrue(animation.IsPlaying("Run"));
        }

        [UnityTest]
        public IEnumerator Unit_swing_plays_attack_once_then_idle()
        {
            hero.BeginAttack(null);
            yield return new WaitForSeconds(0.12f);
            Assert.IsTrue(animation.IsPlaying("Attack"));
            Assert.AreEqual(WrapMode.Once, animation["Attack"].wrapMode);
            Assert.That(animation["Attack"].speed, Is.EqualTo(animation["Attack"].length / 0.9f).Within(0.001f));
            yield return new WaitForSeconds(0.9f);
            Assert.IsTrue(animation.IsPlaying("Idle"));
            Assert.IsFalse(hero.IsSwinging);
        }

        [UnityTest]
        public IEnumerator Hero_death_clamps_with_all_equipment_above_the_floor()
        {
            hero.ApplyDamage(100000f, null);
            yield return new WaitForSeconds(0.15f);
            Assert.IsTrue(hero.Actor.IsDead);
            Assert.IsTrue(animation.IsPlaying("Death"));
            Assert.AreEqual(WrapMode.ClampForever, animation["Death"].wrapMode);
            yield return new WaitForSeconds(animation["Death"].length + 0.1f);
            var before = GeometryBounds(false);
            yield return new WaitForSeconds(0.2f);
            var after = GeometryBounds(false);
            Assert.Less(Vector3.Distance(before.center, after.center), 0.003f);
            Assert.Less(Vector3.Distance(before.size, after.size), 0.003f);
            Assert.Greater(GeometryBounds(false).min.y, -0.02f);
        }

        [UnityTest]
        public IEnumerator Standing_shield_stays_below_helmet_and_within_shoulder_limit()
        {
            var transforms = hero.GetComponentsInChildren<Transform>();
            var left = transforms.Single(t => t.name == "Shoulder.L");
            var right = transforms.Single(t => t.name == "Shoulder.R");
            foreach (var name in new[] { "Idle", "Run", "Attack" })
                for (var sample = 0; sample <= 96; sample++)
                {
                    var clip = animation.GetClip(name);
                    clip.SampleAnimation(animation.gameObject, clip.length * sample / 96f);
                    var top = GeometryBounds(false, "HB_Shield").max.y;
                    var shoulder = (left.position.y + right.position.y) * .5f;
                    Assert.LessOrEqual(top - shoulder, .20f, name + " shield top at " + sample);
                    Assert.LessOrEqual(top, GeometryBounds(false, "HB_Helmet").max.y, name + " helmet at " + sample);
                }
            yield return null;
        }

        private Bounds GeometryBounds(bool feetOnly, string prefix = null)
        {
            var found = false;
            var bounds = new Bounds();
            var vertices = new List<Vector3>();
            var mesh = new Mesh();
            try
            {
                foreach (var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (prefix != null && !renderer.name.StartsWith(prefix)) continue;
                    if (feetOnly && !renderer.name.StartsWith("HB_Boot_") && !renderer.name.StartsWith("HB_Sabaton_")) continue;
                    mesh.Clear();
                    renderer.BakeMesh(mesh, true);
                    mesh.GetVertices(vertices);
                    foreach (var vertex in vertices)
                    {
                        var point = renderer.transform.TransformPoint(vertex);
                        Assert.IsFalse(float.IsNaN(point.x) || float.IsInfinity(point.x) || float.IsNaN(point.y) || float.IsInfinity(point.y) || float.IsNaN(point.z) || float.IsInfinity(point.z));
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

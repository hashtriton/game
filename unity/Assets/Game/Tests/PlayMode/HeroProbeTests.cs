using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arena;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class HeroProbeTests
    {
        private Unit hero;
        private Animation animation;
        private Scene probe;
        private Scene previous;

        [UnitySetUp]
        public IEnumerator Mount_through_unit_start()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/HeroProbe.prefab");
            Assert.IsNotNull(prefab);
            previous = SceneManager.GetActiveScene();
            probe = SceneManager.CreateScene("HeroProbeTest");
            SceneManager.SetActiveScene(probe);
            var root = new GameObject("ProbeHero");
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
        public IEnumerator Probe_prefab_has_the_four_legacy_clips()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/HeroProbe.prefab");
            Assert.IsNotNull(prefab, "The isolated rig probe prefab must exist.");
            var animation = prefab.GetComponentInChildren<Animation>();
            Assert.IsNotNull(animation);
            foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
            {
                var clip = animation.GetClip(name);
                Assert.IsNotNull(clip, name);
                Assert.IsTrue(clip.legacy, name);
                foreach (var binding in UnityEditor.AnimationUtility.GetCurveBindings(clip))
                    Assert.IsTrue(binding.path.Length == 0 || animation.transform.Find(binding.path) != null, name + ": " + binding.path);
                var donor = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Arena/Generated/Warrior_" + name + ".anim");
                foreach (var path in UnityEditor.AnimationUtility.GetCurveBindings(donor).Select(b => b.path).Distinct())
                    Assert.IsTrue(path.Length == 0 || animation.transform.Find(path) != null, "Donor compatibility: " + path);
            }
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator Actor_normalizes_baked_idle_geometry_to_2_4_metres()
        {
            animation.Play("Idle");
            animation["Idle"].time = 0;
            animation.Sample();
            var bounds = GeometryBounds(hero.gameObject);
            Assert.That(bounds.size.y, Is.EqualTo(2.4f).Within(0.048f));
            Assert.That(bounds.min.y, Is.EqualTo(0f).Within(0.02f));
            Assert.AreEqual(64, hero.GetComponentsInChildren<SkinnedMeshRenderer>().Length);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Idle_and_run_advance_and_loop_through_actor_commands()
        {
            Assert.AreEqual(WrapMode.Loop, animation["Idle"].wrapMode);
            var idleStart = animation["Idle"].time;
            yield return new WaitForSeconds(0.15f);
            Assert.Greater(animation["Idle"].time, idleStart + 0.08f);
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
        public IEnumerator All_clips_keep_all_parts_above_the_floor_inside_a_sane_box()
        {
#if UNITY_EDITOR
            var donor = Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Arena/Generated/Warrior.prefab"));
            foreach (var playback in donor.GetComponentsInChildren<Animation>()) playback.enabled = false;
            try
            {
#endif
            foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
            {
                animation.Play(name);
                for (var sample = 0; sample <= 96; sample++)
                {
                    animation[name].time = animation[name].length * sample / 96f;
                    animation.Sample();
                    animation.GetClip(name).SampleAnimation(animation.gameObject, animation[name].time);
                    var bounds = GeometryBounds(hero.gameObject);
                    Assert.Less(bounds.size.x, 5f, name + " width");
                    Assert.Less(bounds.size.y, 4f, name + " height");
                    Assert.Less(bounds.size.z, 5f, name + " depth");
                    Assert.Less(bounds.center.magnitude, 3f, name + " root displacement");
                    Assert.Greater(bounds.min.y, -0.025f, name + " floor penetration");
                    foreach (var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        Assert.Less(Vector3.Distance(GeometryBounds(renderer.gameObject).center, hero.transform.position), 4f, renderer.name);
                    }
                }
#if UNITY_EDITOR
                var source = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Arena/Generated/Warrior_" + name + ".anim");
                CompareDonorMotion(donor, source, animation.GetClip(name));
#endif
            }
#if UNITY_EDITOR
            }
            finally { Object.Destroy(donor); }
#endif
            yield return null;
        }

        private void CompareDonorMotion(GameObject donor, AnimationClip source, AnimationClip probeClip)
        {
            var paths = new[]
            {
                "CharacterArmature/Root/Body/Hips/Abdomen/Torso/Shoulder.R/UpperArm.R",
                "CharacterArmature/Root/Body/Hips/Abdomen/Torso/Shoulder.R/UpperArm.R/LowerArm.R",
                "CharacterArmature/Root/Body/Hips/Abdomen/Torso/Shoulder.L/UpperArm.L/LowerArm.L",
                "CharacterArmature/Root/Body/UpperLeg.L"
            };
            source.SampleAnimation(donor, 0);
            probeClip.SampleAnimation(animation.gameObject, 0);
            var donorRest = paths.Select(p => donor.transform.Find(p).rotation).ToArray();
            var probeRest = paths.Select(p => animation.transform.Find(p).rotation).ToArray();
            foreach (var phase in new[] { 0.25f, 0.5f, 0.75f })
            {
                source.SampleAnimation(donor, source.length * phase);
                probeClip.SampleAnimation(animation.gameObject, probeClip.length * phase);
                for (var i = 0; i < paths.Length; i++)
                {
                    var expected = donor.transform.Find(paths[i]).rotation * Quaternion.Inverse(donorRest[i]);
                    var actual = animation.transform.Find(paths[i]).rotation * Quaternion.Inverse(probeRest[i]);
                    Assert.Less(Quaternion.Angle(expected, actual), 2f, source.name + " donor rotation delta " + paths[i]);
                }
            }
        }

        [UnityTest]
        public IEnumerator Unit_swing_fits_attack_once_and_returns_to_idle()
        {
            hero.BeginAttack(null);
            yield return new WaitForSeconds(0.12f);
            Assert.AreEqual(WrapMode.Once, animation["Attack"].wrapMode);
            Assert.That(animation["Attack"].length, Is.EqualTo(20f / 24f).Within(0.001f));
            Assert.That(animation["Attack"].speed, Is.EqualTo(animation["Attack"].length / 0.9f).Within(0.001f));
            Assert.IsTrue(animation.IsPlaying("Attack"));
            Assert.IsTrue(hero.IsSwinging);
            yield return new WaitForSeconds(0.9f);
            Assert.IsFalse(animation.IsPlaying("Attack"));
            Assert.IsTrue(animation.IsPlaying("Idle"));
            Assert.IsFalse(hero.IsSwinging);
        }

        [UnityTest]
        public IEnumerator Hero_death_clamps_the_last_pose()
        {
            hero.ApplyDamage(100000f, null);
            yield return new WaitForSeconds(0.15f);
            Assert.IsTrue(hero.Actor.IsDead);
            Assert.IsTrue(animation.IsPlaying("Death"));
            Assert.AreEqual(WrapMode.ClampForever, animation["Death"].wrapMode);
            yield return new WaitForSeconds(animation["Death"].length + 0.1f);
            var before = GeometryBounds(hero.gameObject);
            yield return new WaitForSeconds(0.2f);
            var after = GeometryBounds(hero.gameObject);
            Assert.Greater(after.min.y, -0.025f, "Fallen armour must clear the floor.");
            Assert.Less(Vector3.Distance(before.center, after.center), 0.003f);
            Assert.Less(Vector3.Distance(before.size, after.size), 0.003f);
            Assert.IsNotNull(hero);
        }

        private static Bounds GeometryBounds(GameObject root)
        {
            var found = false;
            var bounds = new Bounds();
            var vertices = new List<Vector3>();
            var mesh = new Mesh();
            try
            {
                foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    mesh.Clear();
                    renderer.BakeMesh(mesh, true);
                    mesh.GetVertices(vertices);
                    foreach (var vertex in vertices)
                    {
                        var point = renderer.transform.TransformPoint(vertex);
                        Assert.IsFalse(float.IsNaN(point.x) || float.IsInfinity(point.x) ||
                            float.IsNaN(point.y) || float.IsInfinity(point.y) || float.IsNaN(point.z) || float.IsInfinity(point.z));
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

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Creatures.Tests
{
    public sealed class CreaturePrefabTests
    {
        // Девять волновых существ с листа art/concepts/2026-10-05-nine-wave-creeps-warcraft-modern-v1.png.
        static readonly string[] Expected =
        {
            "BoarWarrior", "CrystalArachnid", "DarkTroll", "IceGiant", "OceanSpirit", "PlagueEnt", "SiegeGolem",
            "SwampHydra", "YoungDragonspawn"
        };

        static readonly string[] Clips = { "Idle", "Run", "Attack", "Death" };

        static IEnumerable<string> Names() => Expected;

        static GameObject Load(string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Creatures/{name}/{name}.prefab");
            Assert.That(prefab, Is.Not.Null, name + " prefab");
            return Object.Instantiate(prefab);
        }

        static Bounds RendererBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        [Test]
        public void AllNineCreaturesAreImported()
        {
            foreach (var name in Expected)
            {
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Creatures/{name}/{name}.prefab"), Is.Not.Null, name);
                Assert.That(AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Creatures/{name}/{name}.creature.json"), Is.Not.Null, name);
            }
        }

        [TestCaseSource(nameof(Names))]
        public void PrefabHasArenaCompatibleLegacyClips(string name)
        {
            var go = Load(name);
            try
            {
                var anim = go.GetComponent<Animation>();
                Assert.That(anim, Is.Not.Null);
                foreach (var clip in Clips)
                {
                    var c = anim.GetClip(clip);
                    Assert.That(c, Is.Not.Null, clip);
                    Assert.That(c.legacy, Is.True, clip);
                    Assert.That(c.length, Is.GreaterThan(.3f), clip);
                }
                Assert.That(anim.GetClip("Idle").wrapMode, Is.EqualTo(WrapMode.Loop));
                Assert.That(anim.GetClip("Run").wrapMode, Is.EqualTo(WrapMode.Loop));
                Assert.That(anim.GetClip("Death").wrapMode, Is.EqualTo(WrapMode.ClampForever));
                Assert.That(go.GetComponentsInChildren<Animator>(), Is.Empty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCaseSource(nameof(Names))]
        public void SkinnedMeshUsesBakedUrpMaterial(string name)
        {
            var go = Load(name);
            try
            {
                var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
                Assert.That(smr, Is.Not.Null);
                Assert.That(smr.bones.Length, Is.GreaterThanOrEqualTo(10));
                Assert.That(smr.sharedMesh.boneWeights.Length, Is.EqualTo(smr.sharedMesh.vertexCount));
                Assert.That(smr.sharedMesh.triangles.Length / 3, Is.InRange(4000, 30000));
                var mat = smr.sharedMaterial;
                Assert.That(mat.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
                foreach (var tex in new[] { "_BaseMap", "_BumpMap", "_MetallicGlossMap", "_EmissionMap" })
                    Assert.That(mat.GetTexture(tex), Is.Not.Null, tex);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCaseSource(nameof(Names))]
        public void StandsOnGroundAtCreatureScale(string name)
        {
            var go = Load(name);
            try
            {
                var c = go.GetComponent<Animation>().GetClip("Idle");
                c.SampleAnimation(go, 0);
                var b = RendererBounds(go);
                Assert.That(b.min.y, Is.InRange(-.15f, .2f), "feet on ground");
                Assert.That(b.size.y, Is.InRange(1.2f, 5.5f), "height in meters");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCaseSource(nameof(Names))]
        public void ClipsActuallyMoveTheSkeleton(string name)
        {
            var go = Load(name);
            try
            {
                var anim = go.GetComponent<Animation>();
                var bones = go.GetComponentInChildren<SkinnedMeshRenderer>().bones;
                foreach (var clip in new[] { "Run", "Attack", "Death" })
                {
                    var c = anim.GetClip(clip);
                    c.SampleAnimation(go, 0);
                    var a = bones.Select(t => t.position).ToArray();
                    c.SampleAnimation(go, c.length * (clip == "Run" ? .25f : clip == "Attack" ? .45f : 1f));
                    float moved = bones.Select((t, i) => (t.position - a[i]).magnitude).Max();
                    Assert.That(moved, Is.GreaterThan(.05f), clip);
                }
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}

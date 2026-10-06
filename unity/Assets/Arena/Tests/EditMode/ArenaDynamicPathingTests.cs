using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class ArenaDynamicPathingTests
    {
        GameObject root;
        ArenaMap map;
        TextAsset fixture;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("Dynamic pathing test");
            map = root.AddComponent<ArenaMap>();
            map.layoutJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-layout.json");
        }

        [TearDown]
        public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(root);
            if (fixture) UnityEngine.Object.DestroyImmediate(fixture);
        }

        [Test]
        public void DestroyingOriginalNortheastBarrelOpensItsSpaceAndRevivalBlocksItAgain()
        {
            // war3map.doo editorId 686, independently measured in the source map.
            var point = new Vector3(1376f / 64, 0, 2464f / 64);
            var neighbor = new Vector3(1376f / 64, 0, 2400f / 64);
            Assert.That(map.IsWalkable(point), Is.False);
            long initialRevision = map.NavigationRevision;
            Assert.That(map.SetDoodadAlive(686, false), Is.True);
            Assert.That(map.NavigationRevision, Is.GreaterThan(initialRevision));
            long removedRevision = map.NavigationRevision;
            Assert.That(map.IsWalkable(point, 24f / 64), Is.True);
            Assert.That(map.IsWalkable(neighbor), Is.False);
            Assert.That(map.SetDoodadAlive(686, false), Is.False);
            Assert.That(map.SetDoodadAlive(-1, false), Is.False);
            Assert.That(map.NavigationRevision, Is.EqualTo(removedRevision));
            Assert.That(map.SetDoodadAlive(686, true), Is.True);
            Assert.That(map.NavigationRevision, Is.GreaterThan(removedRevision));
            Assert.That(map.IsWalkable(point), Is.False);
        }

        [TestCase(64, 24f)]
        [TestCase(64, 28.8f)]
        [TestCase(64, 32f)]
        [TestCase(32, 16f)]
        public void BodyThatFitsCorridorCanUseItAfterOpeningOneBarrel(int opening, float wcRadius)
        {
            var data = Corridor(opening == 32);
            SetFixture(data);
            float axis = opening == 32 ? 208f : 224f;
            var from = new Vector3(axis / 64, 0, 48f / 64);
            var to = new Vector3(axis / 64, 0, 304f / 64);
            float radius = wcRadius / 64;
            Assert.That(map.FindPath(from, to, radius), Is.Empty);
            Assert.That(map.SetDoodadAlive(42, false), Is.True);
            var opened = map.FindPath(from, to, radius);
            Assert.That(opened, Is.Not.Empty, "The body fits physically within the opening.");
            Assert.That(Vector3.Distance(opened.Last(), to), Is.LessThan(.001f));
            foreach (var point in opened) Assert.That(map.IsWalkable(point, radius), Is.True);
            Assert.That(map.SetDoodadAlive(42, true), Is.True);
            Assert.That(map.FindPath(from, to, radius), Is.Empty, "Cached clearance must be invalidated on revival.");
        }

        [Test]
        public void RemovingOverlappingObjectsNeverClearsAnotherObjectOrTerrain()
        {
            var data = Corridor();
            var second = JsonUtility.FromJson<MapDoodad>(JsonUtility.ToJson(data.doodads[0]));
            second.editorId = 43;
            data.doodads = new[] { data.doodads[0], second };
            data.pathing.flags[4 * 12 + 6] = 2;
            SetFixture(data);
            var objectOnly = new Vector3(240f / 64, 0, 152f / 64);
            var terrain = new Vector3(208f / 64, 0, 152f / 64);
            Assert.That(map.SetDoodadAlive(42, false), Is.True);
            Assert.That(map.IsWalkable(objectOnly), Is.False);
            Assert.That(map.SetDoodadAlive(43, false), Is.True);
            Assert.That(map.IsWalkable(objectOnly), Is.True);
            Assert.That(map.IsWalkable(terrain), Is.False);
            map.Initialize();
            Assert.That(map.IsWalkable(objectOnly), Is.False, "New match initialization restores original objects.");
        }

        static ArenaMapLayout Corridor(bool narrow = false)
        {
            var flags = new int[12 * 12];
            for (int y = 0; y < 12; y++)
                foreach (var x in narrow ? new[] { 4, 5, 7, 8 } : new[] { 4, 5, 8, 9 }) flags[y * 12 + x] = 2;
            return new ArenaMapLayout
            {
                terrain = new TerrainLayout { width = 2, height = 2, cellSize = 384, origin = new[] { 0f, 0f },
                    vertices = Enumerable.Range(0, 4).Select(_ => new TerrainVertex()).ToArray() },
                pathing = new PathingLayout { width = 12, height = 12, cellSize = 32, origin = new[] { 0f, 0f },
                    flags = flags, blockedMask = 2 },
                doodads = new[] { new MapDoodad { editorId = 42, id = "LTbr", life = 100, x = 224, y = 160,
                    pathingTexture = "PathTextures\\2x2Unflyable.tga" } },
                regions = Array.Empty<MapRegion>()
            };
        }

        void SetFixture(ArenaMapLayout data)
        {
            fixture = new TextAsset(JsonUtility.ToJson(data));
            map.layoutJson = fixture;
        }
    }
}

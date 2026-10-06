using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class ArenaMapTests
    {
        GameObject root;
        ArenaMap map;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("Map test");
            map = root.AddComponent<ArenaMap>();
            map.layoutJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-layout.json");
            Assert.That(map.layoutJson,Is.Not.Null);
        }

        [TearDown]
        public void Cleanup() => Object.DestroyImmediate(root);

        [Test]
        public void DynamicFootprintsKeepOverlapAndOriginalBarrelMasksWhenRemoved()
        {
            var point = map.WcToWorld(0, 1000);
            Assert.That(map.IsWalkable(point, 0), Is.True);
            long revision = map.NavigationRevision;
            Assert.That(map.TryAddDynamicDoodad(1000000, "B009", point, 0, 1.2f), Is.True);
            Assert.That(map.TryAddDynamicDoodad(1000001, "B009", point, 0, 1.2f), Is.True);
            Assert.That(map.NavigationRevision, Is.EqualTo(revision + 2));
            Assert.That(map.IsWalkable(point, 0), Is.False);
            Assert.That(map.RemoveDynamicDoodad(1000000), Is.True); Assert.That(map.IsWalkable(point, 0), Is.False);
            Assert.That(map.RemoveDynamicDoodad(1000001), Is.True); Assert.That(map.IsWalkable(point, 0), Is.True);
            point = map.WcToWorld(1376, 2464);
            Assert.That(map.TryAddDynamicDoodad(1000002, "B009", point, 0, 1.2f), Is.True);
            Assert.That(map.RemoveDynamicDoodad(1000002), Is.True); Assert.That(map.IsWalkable(point, 0), Is.False);
            revision = map.NavigationRevision;
            Assert.That(map.TryAddDynamicDoodad(1000003, "B009", map.WorldBounds.min, 0, 1.2f), Is.False);
            Assert.That(map.NavigationRevision, Is.EqualTo(revision));
        }

        [Test]
        public void Original39cLandmarksAndNortheastBarrelsArePreserved()
        {
            Assert.That(map.Layout.version,Is.EqualTo("3.9c"));
            Assert.That(map.Layout.terrain.width,Is.EqualTo(65));
            Assert.That(map.Layout.terrain.height,Is.EqualTo(65));
            Assert.That(map.Layout.doodads.Length,Is.EqualTo(2736));
            var barrels=map.Layout.doodads.Where(d=>(d.id=="LTbr" || d.id=="LTbs" || d.id=="LTex") && d.x>=512 && d.x<=1472 && d.y>=1952 && d.y<=2688).ToArray();
            Assert.That(barrels.Length,Is.EqualTo(94),"independently counted in original war3map.doo");
            Assert.That(barrels.Any(d=>d.id=="LTbr" && d.x==1376 && d.y==2464),Is.True);
            Assert.That(map.IsWalkable(new Vector3(1376/64f,0,2464/64f)),Is.False,"barrels must obstruct movement");
            Assert.That(map.WorldBounds.size.x,Is.EqualTo(128));
        }

        [Test]
        public void AllWaveEntrancesCanReachHeroWithoutCrossingBlockedCells()
        {
            Assert.That(map.IsWalkable(map.HeroSpawn),Is.True);
            foreach(var spawn in map.SpawnPoints)
            {
                var path=map.FindPath(spawn,map.HeroSpawn,.4f);
                Assert.That(path.Count,Is.GreaterThan(0),"No route from "+spawn);
                Assert.That(Vector3.Distance(path.Last(),map.HeroSpawn),Is.LessThan(1));
                foreach(var point in path) Assert.That(map.IsWalkable(point),Is.True,"Blocked waypoint "+point);
            }
        }

        [Test]
        public void DashCannotTunnelThroughBarrelRows()
        {
            var start=map.FindNearestWalkable(new Vector3(1408/64f,0,2464/64f));
            var end=map.Move(start,Vector3.left*8,.45f);
            Assert.That(map.IsWalkable(end),Is.True);
            Assert.That(end.x,Is.GreaterThan(1300/64f),"Swept movement crossed dense original barrel rows");
        }
    }
}

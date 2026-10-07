using System.Collections;
using System.Linq;
using Arena;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class ArenaSceneTests
    {
        private const string ScenePath = "Assets/Game/Scenes/Arena.unity";

        private ArenaMap map;
        private HeroController hero;
        private RtsCamera rtsCamera;

        [UnitySetUp]
        public IEnumerator LoadArena()
        {
            // Tests only run in the Editor, where the scene can be loaded by path without being in the build list.
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            // Start() of the hero and camera runs on the first frame after load.
            yield return null;
            yield return null;
            map = Object.FindAnyObjectByType<ArenaMap>();
            hero = Object.FindAnyObjectByType<HeroController>();
            rtsCamera = Object.FindAnyObjectByType<RtsCamera>();
        }

        [UnityTest]
        public IEnumerator Hero_starts_on_walkable_ground_at_the_map_spawn()
        {
            Assert.IsNotNull(hero);
            Assert.IsTrue(map.IsWalkable(hero.transform.position, hero.radius));
            Assert.Less(Vector3.Distance(hero.transform.position, map.HeroSpawn), 0.01f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hero_runs_to_an_ordered_point_and_stops()
        {
            var target = map.FindNearestWalkable(hero.transform.position + new Vector3(12f, 0f, 5f));
            hero.Order(target, false);
            Assert.IsTrue(hero.IsMoving, "a route to open ground next to the spawn must exist");

            var deadline = Time.time + 8f;
            while (hero.IsMoving && Time.time < deadline) yield return null;

            Assert.IsFalse(hero.IsMoving, "hero did not finish the route in time");
            var flat = hero.transform.position - target;
            flat.y = 0f;
            Assert.Less(flat.magnitude, 0.5f);
        }

        [UnityTest]
        public IEnumerator Order_into_a_blocked_point_ends_on_walkable_ground()
        {
            var barrel = map.Layout.doodads.First(d => d.id == "LTbr" && d.life != 0);
            var blocked = new Vector3(barrel.x, barrel.z, barrel.y) / map.unitsPerMeter;
            Assert.IsFalse(map.IsWalkable(blocked, hero.radius), "a standing barrel must block the pathing grid");

            hero.Order(blocked, false);
            if (hero.IsMoving)
            {
                var deadline = Time.time + 30f;
                while (hero.IsMoving && Time.time < deadline) yield return null;
            }
            Assert.IsTrue(map.IsWalkable(hero.transform.position, hero.radius * 0.9f));
        }

        [UnityTest]
        public IEnumerator Destroying_a_barrel_opens_its_cell_for_pathing()
        {
            var barrel = map.Layout.doodads.First(d => d.id == "LTbr" && d.life != 0);
            var position = new Vector3(barrel.x, barrel.z, barrel.y) / map.unitsPerMeter;
            var revision = map.NavigationRevision;
            Assert.IsFalse(map.IsWalkable(position));

            Assert.IsTrue(map.SetDoodadAlive(barrel.editorId, false));

            Assert.IsTrue(map.IsWalkable(position));
            Assert.Greater(map.NavigationRevision, revision, "paths in flight must be able to notice the change");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Camera_follows_the_hero_and_stays_inside_the_arena_limits()
        {
            var target = map.FindNearestWalkable(hero.transform.position + new Vector3(10f, 0f, 4f));
            hero.Order(target, false);
            var deadline = Time.time + 8f;
            while (hero.IsMoving && Time.time < deadline) yield return null;
            // Let the soft follow settle.
            yield return new WaitForSeconds(1f);

            Assert.IsTrue(rtsCamera.lockToHero);
            var viewCenter = rtsCamera.transform.position + rtsCamera.transform.forward * rtsCamera.distance;
            Assert.Less(Mathf.Abs(viewCenter.x - hero.transform.position.x), 0.6f);
            Assert.Less(Mathf.Abs(viewCenter.z - hero.transform.position.z), 0.6f);
            Assert.IsTrue(rtsCamera.limits.Contains(new Vector3(viewCenter.x, rtsCamera.limits.center.y, viewCenter.z)));
        }
    }
}

using System.Collections;
using System.Linq;
using Arena;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class CombatTests
    {
        private const string ScenePath = "Assets/Game/Scenes/Arena.unity";

        private ArenaMap map;
        private HeroController hero;
        private Camera view;

        [UnitySetUp]
        public IEnumerator LoadArena()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            yield return null;
            map = Object.FindAnyObjectByType<ArenaMap>();
            hero = Object.FindAnyObjectByType<HeroController>();
            view = hero.viewCamera;
        }

        private static Unit AnyCreep() => Unit.All.First(u => u.faction == Faction.Creep);

        private static IEnumerator WaitUntil(System.Func<bool> condition, float seconds)
        {
            var deadline = Time.time + seconds;
            while (!condition() && Time.time < deadline) yield return null;
        }

        [UnityTest]
        public IEnumerator Armor_curve_follows_the_warcraft_formula()
        {
            Assert.AreEqual(1f, Unit.DamageMultiplier(0f), 1e-5f);
            Assert.AreEqual(1f - 0.6f / 1.6f, Unit.DamageMultiplier(10f), 1e-5f);
            Assert.Greater(Unit.DamageMultiplier(-5f), 1f, "negative armor must increase the damage taken");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Edge_pan_scrolls_only_inside_the_window_and_only_at_the_border()
        {
            Assert.AreEqual(Vector2.zero, RtsCamera.EdgePan(new Vector2(960f, 540f), 1920f, 1080f, 12f));
            Assert.AreEqual(new Vector2(-1f, 0f), RtsCamera.EdgePan(new Vector2(3f, 540f), 1920f, 1080f, 12f));
            Assert.AreEqual(new Vector2(1f, 1f), RtsCamera.EdgePan(new Vector2(1919f, 1079f), 1920f, 1080f, 12f));
            Assert.AreEqual(new Vector2(0f, -1f), RtsCamera.EdgePan(new Vector2(960f, 0f), 1920f, 1080f, 12f));
            // In the Editor the cursor can be anywhere on the desktop: outside the window nothing may scroll.
            Assert.AreEqual(Vector2.zero, RtsCamera.EdgePan(new Vector2(-40f, 540f), 1920f, 1080f, 12f));
            Assert.AreEqual(Vector2.zero, RtsCamera.EdgePan(new Vector2(960f, 1500f), 1920f, 1080f, 12f));
            yield return null;
        }

        [UnityTest]
        public IEnumerator North_east_corner_is_full_of_barrels_that_block_the_grid()
        {
            var corner = map.Layout.doodads.Where(d => d.id == "LTbr" && d.x >= 1376f && d.y >= 2016f).ToArray();
            Assert.GreaterOrEqual(corner.Length, 30, "the corner east of the layout's barrel block must be filled");
            foreach (var barrel in corner)
                Assert.IsFalse(map.IsWalkable(new Vector3(barrel.x, 0f, barrel.y) / map.unitsPerMeter), "barrel " + barrel.editorId + " must block its cell");
            // 214 barrels of the layout that block their cell plus the corner ones; scenery stacks are not breakable.
            Assert.GreaterOrEqual(Destructible.All.Count, 250);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Right_click_picking_prefers_the_enemy_and_ignores_empty_ground()
        {
            var creep = AnyCreep();
            var centre = creep.Position + Vector3.up * creep.height * 0.5f;
            var onCreep = new Ray(view.transform.position, centre - view.transform.position);
            Assert.AreSame(creep, TargetPicker.Pick(onCreep, hero.Unit));

            // A click a few metres beside the creep hits the ground, not the creep.
            var beside = new Ray(view.transform.position, centre + Vector3.right * 4f - view.transform.position);
            Assert.IsNull(TargetPicker.Pick(beside, hero.Unit));

            // The hero cannot target himself or his own side.
            var onHero = new Ray(view.transform.position, hero.transform.position + Vector3.up - view.transform.position);
            Assert.IsNull(TargetPicker.Pick(onHero, hero.Unit));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Attack_order_walks_up_to_a_distant_creep_and_kills_it()
        {
            var creep = AnyCreep();
            var start = Vector3.Distance(hero.transform.position, creep.Position);
            Assert.Greater(start, 8f, "the training creeps stand far enough that the hero has to walk");

            hero.OrderAttack(creep);
            Assert.AreSame(creep, hero.Target);
            yield return WaitUntil(() => creep == null || !creep.IsAlive, 45f);

            Assert.IsTrue(creep == null || !creep.IsAlive, "the creep must fall to repeated attacks");
            Assert.IsTrue(hero.Unit.IsAlive);
            Assert.Less(Vector3.Distance(hero.transform.position, map.HeroSpawn), 40f);
            Assert.Greater(Vector3.Distance(hero.transform.position, map.HeroSpawn), 3f, "the hero went to the creep");
        }

        [UnityTest]
        public IEnumerator A_new_move_order_cancels_the_attack()
        {
            var creep = AnyCreep();
            hero.OrderAttack(creep);
            yield return new WaitForSeconds(0.5f);
            hero.OrderMove(map.FindNearestWalkable(hero.transform.position + new Vector3(-6f, 0f, 0f)), false);
            Assert.IsNull(hero.Target);
        }

        [UnityTest]
        public IEnumerator Idle_hero_and_a_creep_that_comes_close_fight_each_other()
        {
            var creep = AnyCreep();
            creep.transform.position = map.FindNearestWalkable(hero.transform.position + new Vector3(2.5f, 0f, 0f));
            creep.GetComponent<CreepAi>().alwaysHunt = true;
            var heroStart = hero.Unit.health;
            var creepStart = creep.health;

            yield return new WaitForSeconds(3f);

            Assert.Less(hero.Unit.health, heroStart, "the creep must hit the hero");
            Assert.Less(creep.health, creepStart, "an idle hero fights back");
        }

        [UnityTest]
        public IEnumerator Attack_order_on_a_barrel_cuts_a_passage_through_the_field()
        {
            // Open ground south of the barrel lets the hero approach and open a passage with one hit.
            var barrel = Destructible.All.First(b => !b.explosive && map.IsWalkable(b.Position + new Vector3(0f, 0f, -1.8f), hero.radius));
            hero.transform.position = map.FindNearestWalkable(barrel.Position + new Vector3(0f, 0f, -6f));
            var position = barrel.Position;
            var revision = map.NavigationRevision;
            Assert.IsFalse(map.IsWalkable(position));

            hero.OrderAttack(barrel);
            yield return WaitUntil(() => barrel == null || !barrel.IsAlive, 20f);

            Assert.IsTrue(barrel == null || !barrel.IsAlive, "the hero's first hit must break a barrel");
            Assert.IsTrue(map.IsWalkable(position), "the broken barrel's cell is open");
            Assert.Greater(map.NavigationRevision, revision, "routes in flight must notice the new gap");
        }

        [UnityTest]
        public IEnumerator A_normal_barrel_breaks_on_the_first_hit()
        {
            yield return BarrelBreaksOnFirstHit(false);
        }

        [UnityTest]
        public IEnumerator An_explosive_barrel_breaks_on_the_first_hit()
        {
            yield return BarrelBreaksOnFirstHit(true);
        }

        private IEnumerator BarrelBreaksOnFirstHit(bool explosive)
        {
            hero.Stop();
            foreach (var creep in Unit.All.Where(u => u.faction == Faction.Creep))
                creep.GetComponent<CreepAi>().enabled = false;

            Destructible barrel = null;
            var stand = Vector3.zero;
            foreach (var candidate in Destructible.All.Where(b => b.explosive == explosive))
            {
                for (var angle = 0; angle < 360; angle += 15)
                {
                    var point = candidate.Position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward
                        * (candidate.radius + hero.Unit.attackRange - 0.05f);
                    if (!map.IsWalkable(point, hero.Unit.pathRadius)) continue;
                    if (Unit.All.Any(u => u.faction == Faction.Creep && Vector3.Distance(u.Position, point) < 5f)) continue;
                    point.y = map.SampleHeight(point);
                    barrel = candidate;
                    stand = point;
                    break;
                }
                if (barrel != null) break;
            }
            Assert.IsNotNull(barrel, "a barrel must have a clear spot within melee reach");
            hero.transform.position = stand;
            Assert.LessOrEqual(barrel.EdgeDistance(stand), hero.Unit.attackRange);
            Assert.IsTrue(hero.Unit.CanAttackNow);
            var position = barrel.Position;
            var brokenAt = -1f;
            System.Action<Destructible> onBroken = victim =>
            {
                if (ReferenceEquals(victim, barrel)) brokenAt = Time.time;
            };
            Destructible.Broken += onBroken;
            try
            {
                // BeginAttack cannot start a second swing before this absolute deadline.
                var secondAttackEarliest = Time.time + hero.Unit.AttackInterval;
                hero.OrderAttack(barrel);
                Assert.AreSame(barrel, hero.Target);
                yield return WaitUntil(() => brokenAt >= 0f, hero.Unit.AttackInterval);
                Assert.GreaterOrEqual(brokenAt, 0f, "the first hero attack must break the barrel");
                Assert.Less(brokenAt, secondAttackEarliest, "the barrel must break before a second swing can start");
                yield return null;
                Assert.IsTrue(barrel == null, "the broken barrel must be removed from the scene");
                Assert.IsTrue(map.IsWalkable(position), "the first hit opens the barrel's cell");
            }
            finally
            {
                Destructible.Broken -= onBroken;
            }
        }

        [UnityTest]
        public IEnumerator Breaking_a_barrel_opens_exactly_its_cell()
        {
            var barrel = Destructible.All.First(b => !b.explosive);
            var position = barrel.Position;
            Assert.IsFalse(map.IsWalkable(position));
            barrel.ApplyDamage(1000f, hero.Unit);
            yield return null;
            Assert.IsTrue(map.IsWalkable(position), "the cell of a broken barrel is open ground");
        }

        [UnityTest]
        public IEnumerator An_explosive_barrel_hurts_the_units_around_it()
        {
            var barrel = Destructible.All.First(b => b.explosive);
            var creep = AnyCreep();
            creep.transform.position = barrel.Position + new Vector3(1.2f, 0f, 0f);
            var before = creep.health;

            barrel.ApplyDamage(1000f, hero.Unit);
            yield return null;

            Assert.Less(creep.health, before);
        }

        [UnityTest]
        public IEnumerator Units_are_solid_and_do_not_stack()
        {
            var creep = AnyCreep();
            creep.GetComponent<CreepAi>().enabled = false;
            creep.transform.position = hero.transform.position + new Vector3(0.1f, 0f, 0f);
            yield return new WaitForSeconds(0.5f);
            var distance = Vector3.Distance(hero.transform.position, creep.transform.position);
            Assert.GreaterOrEqual(distance, hero.radius + creep.radius - 0.1f);
        }
    }
}

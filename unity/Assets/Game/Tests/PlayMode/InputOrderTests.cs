using System.Collections;
using System.Linq;
using Arena;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>The same orders as in <see cref="CombatTests"/>, but given through real mouse and keyboard events.</summary>
    public sealed class InputOrderTests : InputTestFixture
    {
        private const string ScenePath = "Assets/Game/Scenes/Arena.unity";

        private ArenaMap map;
        private HeroController hero;
        private Camera view;
        private Mouse mouse;
        private Keyboard keyboard;

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

        // UnitySetUp runs before the fixture's Setup(), and devices may only be added after it.
        private void AddDevices()
        {
            mouse = InputSystem.AddDevice<Mouse>();
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        private Vector2 ScreenOf(Vector3 world) => view.WorldToScreenPoint(world);

        private IEnumerator WaitForCameraToSettle()
        {
            const float epsilon = 0.0001f;
            const int requiredFrames = 5;
            var deadline = Time.realtimeSinceStartup + 10f;
            var previous = view.transform.position;
            var stableFrames = 0;
            while (stableFrames < requiredFrames && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                var current = view.transform.position;
                stableFrames = (current - previous).sqrMagnitude < epsilon * epsilon ? stableFrames + 1 : 0;
                previous = current;
            }
            Assert.AreEqual(requiredFrames, stableFrames, "the camera must settle within 10 seconds before projecting a click");
        }

        private IEnumerator RightClickAt(Vector3 world)
        {
            Set(mouse.position, ScreenOf(world));
            yield return null;
            Press(mouse.rightButton);
            yield return null;
            Release(mouse.rightButton);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Right_click_on_the_ground_walks_there_and_does_not_attack()
        {
            AddDevices();
            var goal = map.FindNearestWalkable(hero.transform.position + new Vector3(0f, 0f, -5f));
            var start = hero.transform.position;
            yield return RightClickAt(goal);
            Assert.IsNull(hero.Target);
            Assert.IsTrue(hero.IsMoving);

            var deadline = Time.time + 6f;
            while (hero.IsMoving && Time.time < deadline) yield return null;
            Assert.Less(Vector3.Distance(hero.transform.position, goal), 0.7f);
            Assert.Greater(Vector3.Distance(hero.transform.position, start), 3f);
        }

        [UnityTest]
        public IEnumerator Right_click_on_an_enemy_far_away_walks_up_and_attacks_it()
        {
            AddDevices();
            var creep = Unit.All.First(u => u.faction == Faction.Creep);
            yield return RightClickAt(creep.Position + Vector3.up * (creep.height * 0.5f));
            Assert.AreSame(creep, hero.Target, "the click lands on the creep, not on the ground below it");

            var deadline = Time.time + 45f;
            while (creep != null && creep.IsAlive && Time.time < deadline) yield return null;
            Assert.IsTrue(creep == null || !creep.IsAlive);
        }

        [UnityTest]
        public IEnumerator Right_click_on_a_barrel_attacks_it()
        {
            AddDevices();
            var candidates = Destructible.All.Where(b => !b.explosive && map.IsWalkable(b.Position + new Vector3(0f, 0f, -1.8f), hero.radius))
                .OrderBy(b => b.editorId).ToArray();
            Assert.IsNotEmpty(candidates, "a normal barrel must have open ground to the south");
            // A new virtual mouse starts on the screen edge and can otherwise pan the camera during the wait.
            Set(mouse.position, new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            yield return null;
            view.GetComponent<RtsCamera>().lockToHero = true;
            hero.Stop();
            hero.transform.position = map.FindNearestWalkable(candidates[0].Position + new Vector3(0f, 0f, -5f));
            yield return WaitForCameraToSettle();

            Destructible barrel = null;
            var margin = Mathf.Max(4f, view.GetComponent<RtsCamera>().edgeMargin * Screen.height / 1080f) + 2f;
            foreach (var candidate in candidates)
            {
                // Keep the barrel alive through the input frames so this assertion checks the order, not the first hit.
                if (!candidate.IsAlive || candidate.EdgeDistance(hero.transform.position) <= hero.Unit.attackRange + 2f) continue;
                var screen = view.WorldToScreenPoint(candidate.Position + Vector3.up * 0.4f);
                if (screen.z <= 0f || screen.x <= margin || screen.x >= Screen.width - margin
                    || screen.y <= margin || screen.y >= Screen.height - margin) continue;
                // Neighbours may win the picker even when the ray passes through this barrel's centre.
                if (TargetPicker.Pick(view.ScreenPointToRay(screen), hero.Unit) != candidate) continue;
                Set(mouse.position, (Vector2)screen);
                yield return null;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) continue;
                if (TargetPicker.Pick(view.ScreenPointToRay(mouse.position.ReadValue()), hero.Unit) != candidate) continue;
                barrel = candidate;
                break;
            }
            Assert.IsNotNull(barrel, "a visible normal barrel with open ground to the south must be selectable by the settled camera");
            Press(mouse.rightButton);
            yield return null;
            Release(mouse.rightButton);
            yield return null;
            Assert.AreSame(barrel, hero.Target);
        }

        [UnityTest]
        public IEnumerator A_then_left_click_attack_moves_and_S_stops()
        {
            AddDevices();
            var goal = map.FindNearestWalkable(hero.transform.position + new Vector3(-6f, 0f, 0f));
            Set(mouse.position, ScreenOf(goal));
            yield return null;
            Press(keyboard.aKey);
            yield return null;
            Release(keyboard.aKey);
            yield return null;
            Assert.IsTrue(hero.IsAttackMoveArmed);
            Press(mouse.leftButton);
            yield return null;
            Release(mouse.leftButton);
            yield return null;
            Assert.IsFalse(hero.IsAttackMoveArmed);
            Assert.IsTrue(hero.IsMoving);

            Press(keyboard.sKey);
            yield return null;
            Release(keyboard.sKey);
            yield return null;
            Assert.IsFalse(hero.IsMoving, "S must stop the hero");
        }
    }
}

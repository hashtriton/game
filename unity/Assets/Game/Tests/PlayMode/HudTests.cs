using System.Collections;
using System.Linq;
using Arena;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class HudTests : InputTestFixture
    {
        private const string ScenePath = "Assets/Game/Scenes/Arena.unity";

        private GameSession session;
        private GameHud hud;
        private HeroController hero;
        private Keyboard keyboard;

        // Loaded inside each test, after the fixture has set up its own input system: actions that existed before would be dead.
        private IEnumerator LoadArena()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            session = Object.FindAnyObjectByType<GameSession>();
            hud = Object.FindAnyObjectByType<GameHud>();
            hero = Object.FindAnyObjectByType<HeroController>();
            // The item data is parsed on the second frame; give it a moment.
            var deadline = Time.realtimeSinceStartup + 40f;
            while (!session.Ready && session.LoadError == null && Time.realtimeSinceStartup < deadline) yield return null;
        }

        private IEnumerator Tap(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            Press(key);
            yield return null;
            Release(key);
            yield return null;
        }

        [UnityTest]
        public IEnumerator The_session_loads_the_item_data_with_the_original_start_purse()
        {
            yield return LoadArena();
            Assert.IsNull(session.LoadError, session.LoadError);
            Assert.IsTrue(session.Ready);
            Assert.AreEqual(130, session.Loadout.Gold);
            Assert.AreEqual(4, session.Loadout.Souls);
            Assert.GreaterOrEqual(session.Guides.guides.Length, 5);
            Assert.GreaterOrEqual(session.Book.Tabs.Count, 10);
            yield return null;
        }

        [UnityTest]
        public IEnumerator B_and_G_open_the_shop_and_the_guides_and_Escape_closes_them()
        {
            yield return LoadArena();
            keyboard = InputSystem.AddDevice<Keyboard>();
            Assert.IsFalse(hud.ShopOpen);
            yield return Tap(keyboard.bKey);
            Assert.IsTrue(hud.ShopOpen);
            yield return Tap(keyboard.bKey);
            Assert.IsFalse(hud.ShopOpen, "B toggles");
            yield return Tap(keyboard.gKey);
            Assert.IsTrue(hud.ShopOpen);
            yield return Tap(keyboard.escapeKey);
            Assert.IsFalse(hud.ShopOpen);
        }

        [UnityTest]
        public IEnumerator Killing_a_creep_pays_its_bounty_to_the_hero()
        {
            yield return LoadArena();
            var creep = Unit.All.First(u => u.faction == Faction.Creep);
            var before = session.Loadout.Gold;
            creep.ApplyDamage(100000f, hero.Unit);
            yield return null;
            Assert.AreEqual(before + creep.bounty, session.Loadout.Gold);
        }

        [UnityTest]
        public IEnumerator A_guide_purchase_through_the_hud_adds_the_item_and_its_bonuses_to_the_hero()
        {
            yield return LoadArena();
            session.Loadout.Grant(2000);
            hud.OpenShop(session.Book.Tabs.Count);
            var damageBefore = hero.Unit.DamageMax;
            var healthBefore = hero.Unit.MaxHealth;

            // The first guide is "Start: 130 gold": the Sphere of Fire and a Health Stone.
            hud.BuyNextOfGuide();
            hud.BuyNextOfGuide();
            yield return null;

            CollectionAssert.AreEquivalent(new[] { "I02A", "I022" }, session.Loadout.OwnedIds());
            Assert.Greater(hero.Unit.DamageMax, damageBefore, "the Sphere of Fire adds damage");
            Assert.GreaterOrEqual(hero.Unit.MaxHealth, healthBefore);
        }

        [UnityTest]
        public IEnumerator Buying_a_huge_axe_raises_strength_derived_health_and_damage()
        {
            yield return LoadArena();
            var plan = session.Loadout.PlanFor("I005", true);
            session.Loadout.Grant(plan.gold);
            var healthBefore = hero.Unit.MaxHealth;
            var damageBefore = hero.Unit.DamageMax;

            Assert.AreEqual(BuyOutcome.Bought, session.Loadout.BuyPlan(plan, out _));
            yield return null;

            Assert.Greater(hero.Unit.MaxHealth, healthBefore, "+15 strength gives health");
            Assert.GreaterOrEqual(hero.Unit.DamageMax, damageBefore + 32f, "+32 damage from the axe");

            session.Loadout.Sell(System.Array.FindIndex(session.Loadout.Slots, s => s != null));
            yield return null;
            Assert.AreEqual(healthBefore, hero.Unit.MaxHealth, 0.01f, "selling takes the bonuses away again");
        }
        [UnityTest]
        public IEnumerator A_mouse_click_on_a_shelf_icon_buys_the_item_and_hovering_shows_the_tooltip()
        {
            yield return LoadArena();
            var mouse = InputSystem.AddDevice<Mouse>();
            hud.OpenShop(0);
            // The grid positions its cells during the canvas update, so force it before reading positions.
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;

            var cell = GameObject.Find("Shelf cell 1");
            Assert.IsNotNull(cell, "the first shelf cell must exist");
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, cell.transform.position);

            Set(mouse.position, screen);
            yield return null;
            yield return null;
            Assert.IsTrue(hud.TooltipVisible, "pointing at an icon opens its tooltip");

            var goldBefore = session.Loadout.Gold;
            Click(mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(1, session.Loadout.OwnedIds().Count, "one click buys the item");
            Assert.Less(session.Loadout.Gold, goldBefore);

            Set(mouse.position, new Vector2(2f, 2f));
            yield return null;
            yield return null;
            Assert.IsFalse(hud.TooltipVisible, "moving off the icon closes the tooltip");
        }
    }
}

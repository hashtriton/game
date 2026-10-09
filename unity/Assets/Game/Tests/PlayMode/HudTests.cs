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
        [UnityTest]
        public IEnumerator The_bottom_bag_hover_and_sale_require_an_open_shop()
        {
            yield return LoadArena();
            session.Loadout.Grant(1000);
            Assert.AreEqual(BuyOutcome.Bought, session.Loadout.BuyPlan(session.Loadout.PlanFor("I007", true), out _));
            yield return null;
            var mouse = InputSystem.AddDevice<Mouse>();
            Canvas.ForceUpdateCanvases();
            var bag = GameObject.Find("Bag 1").GetComponent<RectTransform>();
            Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null, bag.TransformPoint(bag.rect.center)));
            yield return null;
            yield return null;
            Assert.IsTrue(hud.TooltipVisible, "bottom bag hover must show the existing tooltip");
            Canvas.ForceUpdateCanvases();
            var tooltipRect = GameObject.Find("Tooltip").GetComponent<RectTransform>();
            Assert.GreaterOrEqual(tooltipRect.anchoredPosition.y - tooltipRect.rect.height, 244f - 0.1f,
                "the existing tooltip must clear the bottom HUD");
            var gold = session.Loadout.Gold;
            Click(mouse.rightButton);
            yield return null;
            yield return null;
            Assert.AreEqual(1, session.Loadout.OwnedIds().Count, "outside shop, right click must not sell");
            Assert.AreEqual(gold, session.Loadout.Gold);
            hud.OpenShop(0);
            var sellValue = session.Loadout.SellValue(0);
            Click(mouse.rightButton);
            yield return null;
            yield return null;
            Assert.AreEqual(0, session.Loadout.OwnedIds().Count);
            Assert.AreEqual(gold + sellValue, session.Loadout.Gold);
            Assert.IsFalse(hud.TooltipVisible);
        }

        [UnityTest]
        public IEnumerator The_floating_buttons_accept_mouse_input_and_F9_still_grants_gold()
        {
            yield return LoadArena();
            var mouse = InputSystem.AddDevice<Mouse>();
            keyboard = InputSystem.AddDevice<Keyboard>();
            foreach (var name in new[] { "ЛАВКА B", "ГАЙДЫ G" })
            {
                var button = GameObject.Find(name).GetComponent<RectTransform>();
                Canvas.ForceUpdateCanvases();
                Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null, button.TransformPoint(button.rect.center)));
                yield return null;
                yield return null;
                Click(mouse.leftButton);
                yield return null;
                yield return null;
                Assert.IsTrue(hud.ShopOpen, name);
                yield return Tap(keyboard.escapeKey);
                Assert.IsFalse(hud.ShopOpen);
            }
            var gold = session.Loadout.Gold;
            yield return Tap(keyboard.f9Key);
            Assert.AreEqual(gold + 500, session.Loadout.Gold);
        }

        [UnityTest]
        public IEnumerator Segmented_values_follow_real_stats_and_placeholders_are_inert()
        {
            yield return LoadArena();
            hero.Unit.healthRegen = 0f;
            hero.Unit.manaRegen = 0f;
            hero.Unit.health = hero.Unit.MaxHealth * 0.7f;
            hero.Unit.mana = hero.Unit.MaxMana * 0.6f;
            yield return null;
            var health = hud.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i => i.name.StartsWith("Health segment ")).ToArray();
            var mana = hud.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i => i.name.StartsWith("Mana segment ")).ToArray();
            Assert.AreEqual(10, health.Length);
            Assert.AreEqual(10, mana.Length);
            Assert.AreEqual(7f, health.Sum(i => i.fillAmount), 0.01f);
            Assert.AreEqual(6f, mana.Sum(i => i.fillAmount), 0.01f);
            foreach (var name in new[] { "Q locked", "W locked", "E locked", "Ultimate locked", "Passive locked" })
            {
                var plate = GameObject.Find(name);
                Assert.IsNotNull(plate);
                Assert.AreEqual(0, plate.GetComponentsInChildren<UnityEngine.UI.Button>().Length);
                Assert.AreEqual(0, plate.GetComponentsInChildren<PointerRelay>().Length);
                Assert.IsTrue(plate.GetComponentsInChildren<UnityEngine.UI.Graphic>().All(g => !g.raycastTarget));
            }
            var beforeHealth = hero.Unit.health;
            var beforeMana = hero.Unit.mana;
            keyboard = InputSystem.AddDevice<Keyboard>();
            foreach (var key in new[] { keyboard.qKey, keyboard.wKey, keyboard.eKey, keyboard.rKey }) yield return Tap(key);
            Assert.AreEqual(beforeHealth, hero.Unit.health);
            Assert.AreEqual(beforeMana, hero.Unit.mana);
            hero.Unit.health = 0f;
            yield return null;
            Assert.IsTrue(GameObject.Find("Death overlay").activeSelf);
            Assert.AreEqual("0", GameObject.Find("Health value").GetComponent<UnityEngine.UI.Text>().text);
            Assert.IsTrue(health.All(i => i.fillAmount == 0f));
        }
        [UnityTest]
        public IEnumerator A_tall_shelf_tooltip_stays_above_the_HUD_and_inside_the_screen()
        {
            yield return LoadArena();
            // I08D has the longest shelf tooltip with its real recipe purchase instructions.
            var tab = session.Book.Tabs.ToList().FindIndex(t => t.entries.Any(e => e.displayId == "I08D"));
            Assert.GreaterOrEqual(tab, 0);
            var index = session.Book.Tabs[tab].entries.FindIndex(e => e.displayId == "I08D");
            hud.OpenShop(tab);
            var mouse = InputSystem.AddDevice<Mouse>();
            Canvas.ForceUpdateCanvases();
            var cell = GameObject.Find("Shelf cell " + (index + 1)).GetComponent<RectTransform>();
            Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null, cell.TransformPoint(cell.rect.center)));
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            yield return null;
            Assert.IsTrue(hud.TooltipVisible);
            var tip = GameObject.Find("Tooltip").GetComponent<RectTransform>();
            var canvas = hud.GetComponent<RectTransform>();
            var corners = new Vector3[4];
            tip.GetWorldCorners(corners);
            var bottom = canvas.InverseTransformPoint(corners[0]).y + canvas.rect.height * 0.5f;
            var top = canvas.InverseTransformPoint(corners[1]).y + canvas.rect.height * 0.5f;
            Assert.GreaterOrEqual(bottom, 243.9f, "even a tall tooltip must clear the new HUD");
            Assert.LessOrEqual(top, canvas.rect.height - 7.9f);
        }
        [UnityTest]
        public IEnumerator Transparent_plate_corners_pass_world_input_but_the_centres_remain_interactive()
        {
            yield return LoadArena();
            var mouse = InputSystem.AddDevice<Mouse>();
            keyboard = InputSystem.AddDevice<Keyboard>();
            Canvas.ForceUpdateCanvases();
            var events = UnityEngine.EventSystems.EventSystem.current;
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            foreach (var name in new[] { "Bag 1", "Bag 2", "Bag 3", "Bag 4", "Bag 5", "Bag 6", "ЛАВКА B", "ГАЙДЫ G" })
            {
                var rect = GameObject.Find(name).GetComponent<RectTransform>();
                var pointer = new UnityEngine.EventSystems.PointerEventData(events);
                pointer.position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
                hits.Clear();
                events.RaycastAll(pointer, hits);
                Assert.IsNotEmpty(hits, name + " centre must stay interactive");
                pointer.position = RectTransformUtility.WorldToScreenPoint(null,
                    rect.TransformPoint(new Vector2(rect.rect.xMin + 1f, rect.rect.yMax - 1f)));
                hits.Clear();
                events.RaycastAll(pointer, hits);
                Assert.IsEmpty(hits, name + " transparent corner must pass through");
            }
            var bag = GameObject.Find("Bag 1").GetComponent<RectTransform>();
            Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null,
                bag.TransformPoint(new Vector2(bag.rect.xMin + 1f, bag.rect.yMax - 1f))));
            yield return null;
            yield return null;
            Assert.IsFalse(events.IsPointerOverGameObject());
            yield return Tap(keyboard.aKey);
            Assert.IsTrue(hero.IsAttackMoveArmed);
            Click(mouse.leftButton);
            yield return null;
            yield return null;
            Assert.IsFalse(hero.IsAttackMoveArmed, "A+click must reach the world controller");
            yield return Tap(keyboard.sKey);
            Click(mouse.rightButton);
            yield return null;
            yield return null;
            Assert.IsTrue(hero.marker.gameObject.activeSelf, "right click must issue a move order");
        }

        [UnityTest]
        public IEnumerator Narrow_canvas_keeps_the_three_clusters_separate_and_below_the_shop()
        {
            yield return LoadArena();
            hud.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var canvas = hud.GetComponent<RectTransform>();
            canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1440f);
            canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1080f);
            yield return null;
            Canvas.ForceUpdateCanvases();
            System.Func<string, Rect> bounds = name =>
            {
                var corners = new Vector3[4];
                hud.transform.Find(name).GetComponent<RectTransform>().GetWorldCorners(corners);
                var lo = canvas.InverseTransformPoint(corners[0]);
                var hi = canvas.InverseTransformPoint(corners[2]);
                return Rect.MinMaxRect(lo.x, lo.y, hi.x, hi.y);
            };
            var left = bounds("Hero panel");
            var ring = bounds("Ultimate locked");
            var right = bounds("Controls cluster");
            Assert.Less(left.xMax, ring.xMin);
            Assert.Less(ring.xMax, right.xMin);
            hud.OpenShop(0);
            var shop = bounds("Shop window");
            Assert.Greater(shop.yMin, Mathf.Max(left.yMax, right.yMax));
        }

        [UnityTest]
        public IEnumerator Large_numerals_fit_and_zero_and_full_bars_follow_the_hero()
        {
            yield return LoadArena();
            hero.Unit.healthRegen = 0f;
            hero.Unit.manaRegen = 0f;
            hero.Unit.baseMaxHealth = 10000000f;
            hero.Unit.health = 1000000f;
            hero.Unit.baseMaxMana = 10000000f;
            hero.Unit.mana = 1000000f;
            session.Loadout.Grant(1000000 - session.Loadout.Gold, 1000000 - session.Loadout.Souls);
            yield return null;
            Canvas.ForceUpdateCanvases();
            foreach (var name in new[] { "Health value", "Health maximum", "Mana value", "Mana maximum", "Gold", "Souls" })
            {
                var text = GameObject.Find(name).GetComponent<UnityEngine.UI.Text>();
                var settings = text.GetGenerationSettings(text.rectTransform.rect.size);
                // Measure layout in canvas units, without Game View pixel rounding or italic ink overhang.
                settings.scaleFactor = 1f;
                using (var layout = new TextGenerator())
                {
                    Assert.IsTrue(layout.Populate(text.text, settings), name + " must generate successfully");
                    Assert.AreEqual(text.text.Length, layout.characterCountVisible, name + " must show every character");
                    Assert.AreEqual(1, layout.lineCount, name + " must stay on one line");
                    var characters = layout.characters.Take(text.text.Length).ToArray();
                    Assert.AreEqual(text.text.Length, characters.Length, name);
                    var width = characters.Last().cursorPos.x + characters.Last().charWidth - characters.First().cursorPos.x;
                    Assert.LessOrEqual(width, text.rectTransform.rect.width, name + " must fit its own field");
                }
            }
            var health = hud.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i => i.name.StartsWith("Health segment ")).ToArray();
            var mana = hud.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i => i.name.StartsWith("Mana segment ")).ToArray();
            hero.Unit.health = -10f;
            hero.Unit.mana = -10f;
            yield return null;
            Assert.AreEqual("0", GameObject.Find("Health value").GetComponent<UnityEngine.UI.Text>().text);
            Assert.IsTrue(health.All(i => i.fillAmount == 0f));
            Assert.IsTrue(mana.All(i => i.fillAmount == 0f));
            hero.Unit.health = hero.Unit.MaxHealth;
            hero.Unit.mana = hero.Unit.MaxMana;
            yield return null;
            Assert.IsTrue(health.All(i => i.fillAmount == 1f));
            Assert.IsTrue(mana.All(i => i.fillAmount == 1f));
            Assert.IsFalse(hud.transform.Find("Death overlay").gameObject.activeSelf);
        }
    }
}

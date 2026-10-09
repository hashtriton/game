using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.EditorTools
{
    // Preview-only state changes, discarded with Play Mode. Never saves scene or assets.
    public static class HudVisualPreview
    {
        private static Behaviour[] captureHidden;

        public static object TextVisible(bool visible)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Text capture requires Play Mode.");
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var tip = hud.transform.Find("Tooltip").GetComponent<RectTransform>();
            var before = tip.rect.size;
            if (visible)
            {
                if (captureHidden != null) foreach (var component in captureHidden) if (component != null) component.enabled = true;
                captureHidden = null;
            }
            else
            {
                captureHidden = hud.GetComponentsInChildren<Behaviour>()
                    .Where(b => b.enabled && (b is LayoutGroup || b is ContentSizeFitter || b is Text))
                    .OrderBy(b => b is Text ? 1 : 0).ToArray();
                foreach (var component in captureHidden) component.enabled = false;
            }
            Canvas.ForceUpdateCanvases();
            var after = tip.rect.size;
            if (!visible && before != after) throw new InvalidOperationException("Backdrop capture changed tooltip geometry.");
            return new { visible, before, after };
        }

        public static object ShopState(string state, int guideIndex = 0)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Shop preview requires Play Mode.");
            if (!new[] { "first", "busy", "tooltip", "tall", "bag", "low", "guides" }.Contains(state))
                throw new ArgumentException("Unknown shop state.", nameof(state));
            State("bag");
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var session = hud.session;
            for (var i = 0; i < 6; i++) session.Loadout.Sell(i);
            var inventory = typeof(Loadout).GetField("inventory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(session.Loadout);
            var purse = inventory.GetType().GetProperty("Gold");
            purse.SetValue(inventory, 20000L);
            var entries = session.Book.Tabs.SelectMany(t => t.entries)
                .Where(e => e.PriceKnown && e.souls == 0 && !session.Book.IsScroll(e.itemId) && session.Book.RecipeFor(e.displayId) == null)
                .GroupBy(e => e.itemId).Select(g => g.First()).Take(6);
            if (state == "guides") session.Loadout.BuyPlan(session.Loadout.PlanFor("I02A", true), out _);
            else foreach (var entry in entries) session.Loadout.Buy(entry);
            hud.hero.Unit.healthRegen = hud.hero.Unit.manaRegen = 0f;
            if (state == "low")
            {
                inventory = typeof(Loadout).GetField("inventory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(session.Loadout);
                purse.SetValue(inventory, 0L);
            }
            var tab = state == "busy" ? session.Book.Tabs.ToList().FindIndex(t => t.entries.Count == session.Book.Tabs.Max(x => x.entries.Count)) : 0;
            var index = 0;
            if (state == "tall")
            {
                tab = session.Book.Tabs.ToList().FindIndex(t => t.entries.Any(e => e.displayId == "I08D"));
                index = session.Book.Tabs[tab].entries.FindIndex(e => e.displayId == "I08D");
            }
            hud.OpenShop(state == "guides" ? session.Book.Tabs.Count : tab);
            if (state == "guides" && guideIndex != 0)
                hud.transform.Find("Shop window/Guides/Guide " + session.Guides.guides[guideIndex].id).GetComponent<Button>().onClick.Invoke();
            Canvas.ForceUpdateCanvases();
            var point = new Vector2(2f, Screen.height - 2f);
            if (state == "tooltip" || state == "tall" || state == "bag")
            {
                var name = state == "bag" ? "Shop bag 1" : "Shelf cell " + (index + 1);
                var cell = GameObject.Find(name).GetComponent<RectTransform>();
                point = RectTransformUtility.WorldToScreenPoint(null, cell.TransformPoint(cell.rect.center));
            }
            if (Mouse.current != null) InputSystem.QueueDeltaStateEvent(Mouse.current.position, point);
            return new { state, tab, index, width = Screen.width, height = Screen.height, gold = session.Loadout.Gold };
        }

        public static object State(string state)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("HUD preview requires Play Mode.");
            if (!new[] { "full", "damaged", "bag", "tooltip", "shop", "death" }.Contains(state))
                throw new ArgumentException("Unknown HUD state.", nameof(state));
            ArenaVisualPreview.Frame("arena");
            var session = UnityEngine.Object.FindAnyObjectByType<GameSession>();
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var hero = hud.hero.Unit;
            if (!session.Ready) throw new InvalidOperationException("Wait for item data before preview.");
            if (state != "shop") hud.transform.Find("Shop window").gameObject.SetActive(false);
            if (state == "bag" && !session.Loadout.OwnedIds().Any())
            {
                session.Loadout.Grant(20000);
                var entries = session.Book.Tabs.SelectMany(tab => tab.entries)
                    .Where(entry => entry.PriceKnown && entry.souls == 0 && !session.Book.IsScroll(entry.itemId) &&
                        session.Book.RecipeFor(entry.displayId) == null)
                    .GroupBy(entry => entry.itemId).Select(group => group.First()).Take(6);
                foreach (var entry in entries) session.Loadout.Buy(entry);
                if (session.Loadout.OwnedIds().Count != 6) throw new InvalidOperationException("Preview needs six distinct items.");
            }
            hero.health = hero.MaxHealth * (state == "full" ? 1f : 0.7f);
            hero.mana = hero.MaxMana * (state == "full" ? 1f : 0.6f);
            if (state == "death") hero.ApplyDamage(100000f, null);
            if (state == "shop") hud.OpenShop(0);
            hud.Say("", 0f);
            Canvas.ForceUpdateCanvases();
            if (Mouse.current != null)
            {
                var point = new Vector2(2f, Screen.height - 2f);
                if (state == "tooltip")
                {
                    var bag = GameObject.Find("Bag 1").GetComponent<RectTransform>();
                    point = RectTransformUtility.WorldToScreenPoint(null, bag.TransformPoint(bag.rect.center));
                }
                InputSystem.QueueDeltaStateEvent(Mouse.current.position, point);
            }
            return new { state, frame = Time.frameCount, width = Screen.width, height = Screen.height,
                hp = hero.health, maxHp = hero.MaxHealth, mp = hero.mana, maxMp = hero.MaxMana,
                items = session.Loadout.OwnedIds().ToArray(), gold = session.Loadout.Gold, souls = session.Loadout.Souls };
        }
    }
}

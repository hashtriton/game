using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    public sealed class ShopSkinTests : InputTestFixture
    {
        private GameHud hud;

        private IEnumerator LoadArena()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Game/Scenes/Arena.unity", new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            hud = Object.FindAnyObjectByType<GameHud>();
            var deadline = Time.realtimeSinceStartup + 40f;
            while (!hud.session.Ready && hud.session.LoadError == null && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(hud.session.Ready, hud.session.LoadError);
        }

        private Rect Bounds(string name)
        {
            var canvas = hud.GetComponent<RectTransform>();
            var corners = new Vector3[4];
            hud.transform.Find(name).GetComponent<RectTransform>().GetWorldCorners(corners);
            var lo = canvas.InverseTransformPoint(corners[0]);
            var hi = canvas.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(lo.x, lo.y, hi.x, hi.y);
        }

        [UnityTest]
        public IEnumerator Shop_fits_logical_16_9_16_10_and_4_3_canvases_with_a_14_pixel_HUD_gap()
        {
            yield return LoadArena();
            hud.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var canvas = hud.GetComponent<RectTransform>();
            hud.OpenShop(0);
            foreach (var width in new[] { 1920f, 1728f, 1440f })
            {
                canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                canvas.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1080f);
                yield return null;
                Canvas.ForceUpdateCanvases();
                var window = Bounds("Shop window");
                Assert.GreaterOrEqual(window.xMin, canvas.rect.xMin, "left edge at " + width);
                Assert.LessOrEqual(window.xMax, canvas.rect.xMax, "right edge at " + width);
                Assert.GreaterOrEqual(window.yMin, canvas.rect.yMin);
                Assert.LessOrEqual(window.yMax, canvas.rect.yMax);
                var top = new[] { "Hero panel", "Ultimate locked", "Controls cluster" }.Max(n => Bounds(n).yMax);
                Assert.GreaterOrEqual(window.yMin - top, 13.99f, "14px gap with floating point precision at " + width);
            }
        }

        [UnityTest]
        public IEnumerator Closed_shop_and_guides_leave_their_entire_former_rectangle_clear_of_UI_raycast_hits()
        {
            yield return LoadArena();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var events = EventSystem.current;
            var hits = new List<RaycastResult>();
            foreach (var tab in new[] { 0, hud.session.Book.Tabs.Count })
            {
                hud.OpenShop(tab);
                Canvas.ForceUpdateCanvases();
                var window = hud.transform.Find("Shop window").GetComponent<RectTransform>();
                var points = new List<Vector2>();
                for (var x = 0; x < 7; x++)
                    for (var y = 0; y < 7; y++)
                        points.Add(RectTransformUtility.WorldToScreenPoint(null, window.TransformPoint(
                            new Vector2(Mathf.Lerp(window.rect.xMin + 4f, window.rect.xMax - 4f, x / 6f),
                                Mathf.Lerp(window.rect.yMin + 4f, window.rect.yMax - 4f, y / 6f)))));
                Press(keyboard.escapeKey);
                yield return null;
                Release(keyboard.escapeKey);
                yield return null;
                Assert.IsFalse(hud.ShopOpen);
                foreach (var point in points)
                {
                    hits.Clear();
                    events.RaycastAll(new PointerEventData(events) { position = point }, hits);
                    Assert.IsEmpty(hits, "closed window must pass world input at " + point);
                }
            }
        }

        [UnityTest]
        public IEnumerator The_price_row_preserves_the_original_noninteractive_area()
        {
            yield return LoadArena();
            hud.OpenShop(0);
            var mouse = InputSystem.AddDevice<Mouse>();
            Canvas.ForceUpdateCanvases();
            var price = GameObject.Find("Shelf cell 1").transform.Find("Price").GetComponent<RectTransform>();
            Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null, price.TransformPoint(price.rect.center)));
            yield return null;
            yield return null;
            Assert.IsFalse(hud.TooltipVisible, "price row must not become a new hover target");
            var gold = hud.session.Loadout.Gold;
            Click(mouse.leftButton);
            yield return null;
            yield return null;
            Assert.AreEqual(gold, hud.session.Loadout.Gold);
            Assert.AreEqual(0, hud.session.Loadout.OwnedIds().Count);
        }

        [UnityTest]
        public IEnumerator Every_guide_header_fits_its_text_and_clears_the_next_purchase_button()
        {
            yield return LoadArena();
            hud.OpenShop(hud.session.Book.Tabs.Count);
            var area = hud.transform.Find("Shop window/Guides");
            foreach (var guide in hud.session.Guides.guides)
            {
                area.Find("Guide " + guide.id).GetComponent<Button>().onClick.Invoke();
                yield return null;
                Canvas.ForceUpdateCanvases();
                var summary = area.Find("Guide summary").GetComponent<Text>();
                var source = area.Find("Guide source").GetComponent<Text>();
                var next = area.Find("Buy next").GetComponent<RectTransform>();
                Assert.LessOrEqual(summary.preferredHeight, summary.rectTransform.rect.height, guide.id + " summary");
                Assert.LessOrEqual(source.preferredHeight, source.rectTransform.rect.height, guide.id + " source");
                Assert.GreaterOrEqual(source.rectTransform.anchoredPosition.y - source.rectTransform.rect.height - next.anchoredPosition.y,
                    7.99f, guide.id + " source must clear the next button");
            }
        }

        [UnityTest]
        public IEnumerator Tall_tooltip_body_remains_at_least_18_pixels_after_its_fit_scale_at_1080p()
        {
            yield return LoadArena();
            var book = hud.session.Book;
            var tab = book.Tabs.ToList().FindIndex(t => t.entries.Any(e => e.displayId == "I08D"));
            var index = book.Tabs[tab].entries.FindIndex(e => e.displayId == "I08D");
            hud.OpenShop(tab);
            var mouse = InputSystem.AddDevice<Mouse>();
            Canvas.ForceUpdateCanvases();
            var cell = GameObject.Find("Shelf cell " + (index + 1)).GetComponent<RectTransform>();
            Set(mouse.position, RectTransformUtility.WorldToScreenPoint(null, cell.TransformPoint(cell.rect.center)));
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            Assert.IsTrue(hud.TooltipVisible);
            var tip = hud.transform.Find("Tooltip");
            foreach (var text in tip.GetComponentsInChildren<Text>())
                if (!string.IsNullOrEmpty(text.text))
                    Assert.GreaterOrEqual(text.fontSize * tip.localScale.x, 18f, text.name);
        }
    }
}

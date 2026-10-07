using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// The interface of the match: hero panel with bag and purse, the shop window with shelves and buying guides,
    /// item tooltips, health bars and short messages. It is built in code when the scene starts.
    /// </summary>
    public sealed partial class GameHud : MonoBehaviour
    {
        public GameSession session;
        public HudAssets assets;
        public Camera view;
        public HeroController hero;

        private RectTransform canvasRect;
        private TooltipView tooltip;
        private Text toast;
        private float toastUntil;
        private GameObject deathOverlay;
        private Text loadingLabel;

        private Loadout Loadout => session.Loadout;
        private ItemBook Book => session.Book;

        public bool ShopOpen => shopWindow != null && shopWindow.activeSelf;
        public bool TooltipVisible => tooltip != null && tooltip.Visible;

        private void Awake()
        {
            BuildCanvas();
            BuildHeroPanel();
            BuildShopWindow();
            BuildToastAndOverlays();
            // The tooltip goes last so it draws above every window.
            tooltip = new TooltipView(assets, canvasRect, canvasRect);
        }

        private void Start()
        {
            session.BecameReady += OnSessionReady;
            if (session.Ready) OnSessionReady();
        }

        private void OnDestroy()
        {
            if (session == null) return;
            session.BecameReady -= OnSessionReady;
            if (session.Loadout != null) session.Loadout.Changed -= OnLoadoutChanged;
        }

        private void BuildCanvas()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // Height drives the scale, so the window stack (shop above the hero panel) keeps its proportions on any aspect ratio.
            scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();
            canvasRect = (RectTransform)transform;

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var bars = UiKit.Rect("Unit bars", transform);
            UiKit.Stretch(bars);
            var unitBars = bars.gameObject.AddComponent<UnitBars>();
            unitBars.view = view;
            unitBars.assets = assets;
            unitBars.canvasRect = canvasRect;
        }

        private void BuildToastAndOverlays()
        {
            toast = UiKit.Label(transform, "Toast", assets.bodyFont, 28, UiColors.Text, TextAnchor.MiddleCenter);
            UiKit.Place(toast.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 250f), new Vector2(1100f, 44f));
            toast.gameObject.SetActive(false);

            loadingLabel = UiKit.Label(transform, "Loading", assets.bodyFont, 24, UiColors.Muted, TextAnchor.MiddleCenter, "Загрузка данных предметов...");
            UiKit.Place(loadingLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(900f, 40f));

            var overlay = UiKit.Rect("Death overlay", transform);
            UiKit.Stretch(overlay);
            var tint = overlay.gameObject.AddComponent<Image>();
            tint.color = new Color(0.25f, 0f, 0f, 0.35f);
            tint.raycastTarget = false;
            var title = UiKit.Label(overlay, "Title", assets.titleFont, 84, new Color(0.78f, 0.16f, 0.12f), TextAnchor.MiddleCenter, "ГЕРОЙ ПАЛ");
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1200f, 120f));
            var sub = UiKit.Label(overlay, "Subtitle", assets.bodyFont, 28, UiColors.Muted, TextAnchor.MiddleCenter, "Перезапустите сцену, чтобы начать заново");
            UiKit.Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(1200f, 50f));
            deathOverlay = overlay.gameObject;
            deathOverlay.SetActive(false);
        }

        private void OnSessionReady()
        {
            loadingLabel.gameObject.SetActive(false);
            if (!session.Ready)
            {
                Say("Не удалось загрузить данные предметов: " + session.LoadError, 8f);
                return;
            }
            Loadout.Changed += OnLoadoutChanged;
            BuildShelves();
            BuildGuides();
            OnLoadoutChanged();
        }

        private void OnLoadoutChanged()
        {
            RefreshBag();
            RefreshShop();
            RefreshGuides();
        }

        public void Say(string text, float seconds = 2.6f)
        {
            toast.text = text;
            toast.gameObject.SetActive(true);
            toastUntil = Time.unscaledTime + seconds;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && session.Ready)
            {
                if (keyboard.bKey.wasPressedThisFrame) ToggleShop(false);
                if (keyboard.gKey.wasPressedThisFrame) ToggleShop(true);
                if (keyboard.escapeKey.wasPressedThisFrame && ShopOpen) CloseShop();
            }

            if (toast.gameObject.activeSelf && Time.unscaledTime > toastUntil) toast.gameObject.SetActive(false);
            deathOverlay.SetActive(hero != null && !hero.Unit.IsAlive);

            UpdateHeroPanel();
            if (ShopOpen) UpdateShopWindow();

            var mouse = Mouse.current;
            if (mouse != null) tooltip.Follow(mouse.position.ReadValue());
        }

        // ---- tooltips ------------------------------------------------------------------------------------------

        private void ShowTooltip(string displayId, string footer)
        {
            if (!session.Ready || string.IsNullOrEmpty(displayId)) return;
            // A recipe scroll is described as the item it makes.
            var info = ItemInfoBuilder.Build(Book, Book.ResultOfScroll(displayId));
            tooltip.Show(Book, info, Loadout.OwnedIds(), footer);
            var mouse = Mouse.current;
            if (mouse != null) tooltip.Follow(mouse.position.ReadValue());
        }

        private void HideTooltip() => tooltip.Hide();

        /// <summary>Makes a slot show the tooltip while the pointer is on it.</summary>
        private void Hover(ItemSlotView slot, System.Func<string> footer)
        {
            var relay = UiKit.Relay(slot.root.gameObject);
            relay.entered = _ => ShowTooltip(slot.ItemId, footer?.Invoke());
            relay.exited = _ => HideTooltip();
        }

        private string Outcome(BuyOutcome outcome, int bought, int wanted, PurchasePlan plan)
        {
            switch (outcome)
            {
                case BuyOutcome.Bought:
                    return wanted > 1 ? "Куплено: " + Book.Name(plan.targetId) : "Куплено";
                case BuyOutcome.NotEnoughGold:
                    return bought > 0 ? "Куплено частями: " + bought + " из " + wanted + ". Не хватило золота" : "Не хватает золота";
                case BuyOutcome.NotEnoughSouls:
                    return "Не хватает душ";
                case BuyOutcome.NoSpace:
                    return bought > 0 ? "Куплено частями: " + bought + " из " + wanted + ". Нет места в рюкзаке" : "Нет места в рюкзаке";
                default:
                    return plan != null && plan.problem != null ? plan.problem : "Недоступно";
            }
        }
    }
}

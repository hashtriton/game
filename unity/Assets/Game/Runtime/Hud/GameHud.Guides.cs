using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game
{
    public sealed partial class GameHud
    {
        private sealed class GuideSlot
        {
            public ItemSlotView view;
            public string itemId;
            public int step;
            public int index;
        }

        private readonly List<Image> guideCards = new List<Image>();
        private readonly List<GuideSlot> guideSlots = new List<GuideSlot>();
        private RectTransform guideContent;
        private ScrollRect guideScroll;
        private Text guideTitle;
        private Text guideSummary;
        private Text guideSource;
        private Image nextButtonImage;
        private Text nextButtonLabel;
        private int selectedGuide;
        private string nextItemId;

        private GuideDefinition CurrentGuide =>
            session.Guides != null && selectedGuide < session.Guides.guides.Length ? session.Guides.guides[selectedGuide] : null;

        private void BuildGuides()
        {
            var guides = session.Guides?.guides ?? new GuideDefinition[0];
            if (guides.Length == 0)
            {
                var none = UiKit.Label(guideArea, "None", assets.bodyFont, 24, UiColors.Muted, TextAnchor.UpperLeft, "Гайдов пока нет.");
                Corner(none.rectTransform, 0f, 0f, 600f, 40f);
                return;
            }

            // Left column: one card per guide.
            for (var i = 0; i < guides.Length; i++)
            {
                var index = i;
                var card = UiKit.Picture(guideArea, "Guide " + guides[i].id, assets.button, Color.white, true);
                Corner(card.rectTransform, 0f, -i * 76f, 262f, 70f);
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => SelectGuide(index));
                var title = UiKit.Label(card.rectTransform, "Title", assets.bodyFont, 21, UiColors.Text, TextAnchor.UpperLeft, guides[i].title);
                Corner(title.rectTransform, 14f, -9f, 238f, 30f);
                title.horizontalOverflow = HorizontalWrapMode.Overflow;
                var role = UiKit.Label(card.rectTransform, "Role", assets.bodyFont, 16, UiColors.Muted, TextAnchor.UpperLeft, guides[i].role);
                Corner(role.rectTransform, 14f, -38f, 238f, 24f);
                role.horizontalOverflow = HorizontalWrapMode.Overflow;
                guideCards.Add(card);
            }

            // Right column: header, then the steps in a scrolling list.
            guideTitle = UiKit.Label(guideArea, "Guide title", assets.titleFont, 32, UiColors.Gold, TextAnchor.UpperLeft);
            Corner(guideTitle.rectTransform, 286f, 0f, 654f, 40f);
            guideSummary = UiKit.Label(guideArea, "Guide summary", assets.bodyFont, 19, UiColors.Text, TextAnchor.UpperLeft);
            Corner(guideSummary.rectTransform, 286f, -42f, 654f, 54f);
            guideSource = UiKit.Label(guideArea, "Guide source", assets.bodyFont, 15, UiColors.Muted, TextAnchor.UpperLeft);
            Corner(guideSource.rectTransform, 286f, -98f, 654f, 44f);

            nextButtonImage = UiKit.Picture(guideArea, "Buy next", assets.button, Color.white, true);
            Corner(nextButtonImage.rectTransform, 286f, -146f, 654f, 42f);
            var next = nextButtonImage.gameObject.AddComponent<Button>();
            next.targetGraphic = nextButtonImage;
            next.transition = Selectable.Transition.None;
            next.onClick.AddListener(BuyNextOfGuide);
            nextButtonLabel = UiKit.Label(nextButtonImage.rectTransform, "Text", assets.bodyFont, 21, UiColors.Gold, TextAnchor.MiddleCenter);
            UiKit.Stretch(nextButtonLabel.rectTransform);

            var scroll = UiKit.Rect("Steps", guideArea);
            Corner(scroll, 286f, -198f, 654f, 322f);
            guideScroll = scroll.gameObject.AddComponent<ScrollRect>();
            var viewport = UiKit.Rect("Viewport", scroll);
            UiKit.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var catcher = viewport.gameObject.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f);
            guideContent = UiKit.Rect("Content", viewport);
            guideContent.anchorMin = new Vector2(0f, 1f);
            guideContent.anchorMax = new Vector2(1f, 1f);
            guideContent.pivot = new Vector2(0.5f, 1f);
            guideContent.offsetMin = Vector2.zero;
            guideContent.offsetMax = Vector2.zero;
            var layout = guideContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 8, 0, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            guideContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            guideScroll.viewport = viewport;
            guideScroll.content = guideContent;
            guideScroll.horizontal = false;
            guideScroll.vertical = true;
            guideScroll.movementType = ScrollRect.MovementType.Clamped;
            guideScroll.scrollSensitivity = 36f;

            SelectGuide(0);
        }

        private void SelectGuide(int index)
        {
            selectedGuide = index;
            var guide = CurrentGuide;
            if (guide == null) return;

            for (var i = 0; i < guideCards.Count; i++)
                guideCards[i].color = i == index ? new Color(1.35f, 1.1f, 0.75f) : Color.white;

            guideTitle.text = guide.title;
            guideSummary.text = guide.summary;
            guideSource.text = guide.source;
            RebuildGuideSteps(guide);
            RefreshGuides();
            guideScroll.verticalNormalizedPosition = 1f;
        }

        private void RebuildGuideSteps(GuideDefinition guide)
        {
            guideSlots.Clear();
            for (var i = guideContent.childCount - 1; i >= 0; i--) Destroy(guideContent.GetChild(i).gameObject);

            for (var s = 0; s < guide.steps.Length; s++)
            {
                var step = guide.steps[s];
                var row = UiKit.Picture(guideContent, "Step " + (s + 1), assets.slot, new Color(1f, 1f, 1f, 0.8f), false);
                UiKit.Vertical(row.gameObject, 10, 4f, false);

                var when = UiKit.Label(row.transform, "When", assets.bodyFont, 21, UiColors.Gold, TextAnchor.UpperLeft, (s + 1) + ".  " + step.when);
                when.horizontalOverflow = HorizontalWrapMode.Overflow;
                var note = UiKit.Label(row.transform, "Note", assets.bodyFont, 18, UiColors.Text, TextAnchor.UpperLeft, step.note);
                note.color = new Color(0.78f, 0.75f, 0.68f);

                if (step.items == null || step.items.Length == 0) continue;
                var items = UiKit.Rect("Items", row.transform);
                UiKit.Sized(items.gameObject, -1f, 92f);
                UiKit.Horizontal(items.gameObject, 12f, TextAnchor.UpperLeft);
                for (var i = 0; i < step.items.Length; i++)
                {
                    var id = step.items[i];
                    var view = new ItemSlotView(assets, items, "Item " + (i + 1), 64f, true);
                    var slotRef = new GuideSlot { view = view, itemId = id, step = s, index = i };
                    var relay = UiKit.Relay(view.root.gameObject);
                    relay.entered = _ => ShowTooltip(id, GuideFooter(slotRef));
                    relay.exited = _ => HideTooltip();
                    relay.clicked = data => ClickGuideItem(slotRef, data.button);
                    guideSlots.Add(slotRef);
                }
            }
        }

        /// <summary>Marks done, next and still-missing items of the open guide and prices what is left to buy.</summary>
        private void RefreshGuides()
        {
            var guide = CurrentGuide;
            if (guide == null || !session.Ready || guideContent == null) return;

            var owned = Loadout.OwnedIds();
            var marks = GuideProgress.Evaluate(Book, owned, guide);
            nextItemId = null;
            foreach (var slot in guideSlots)
            {
                var mark = marks[slot.step][slot.index];
                var id = slot.itemId;
                var affordableColor = UiColors.Gold;
                var priceText = "";
                if (mark != ItemMark.Done)
                {
                    var plan = Loadout.PlanFor(id, true);
                    priceText = plan.problem != null ? "-" : plan.gold.ToString();
                    affordableColor = plan.problem == null && Loadout.Gold >= plan.gold ? UiColors.Gold : new Color(0.72f, 0.36f, 0.30f);
                }
                slot.view.Set(id, Book.Name(id), UiColors.Grade(Book.TotalCost(id)), priceText, affordableColor);
                slot.view.SetMark(mark);
                if (mark == ItemMark.Next && nextItemId == null) nextItemId = id;
            }

            if (nextButtonLabel == null) return;
            if (nextItemId == null)
            {
                nextButtonLabel.text = "Все предметы гайда собраны";
                nextButtonLabel.color = UiColors.Muted;
            }
            else
            {
                var plan = Loadout.PlanFor(nextItemId, true);
                nextButtonLabel.text = "КУПИТЬ СЛЕДУЮЩИЙ: " + Book.Name(nextItemId) + (plan.problem == null ? "  (" + plan.gold + ")" : "");
                nextButtonLabel.color = UiColors.Gold;
            }
        }

        private string GuideFooter(GuideSlot slot)
        {
            var marks = GuideProgress.Evaluate(Book, Loadout.OwnedIds(), CurrentGuide);
            if (marks[slot.step][slot.index] == ItemMark.Done) return "Уже есть. Shift + ЛКМ: купить ещё один";
            var plan = Loadout.PlanFor(slot.itemId, true);
            if (plan.problem != null) return plan.problem;
            return "ЛКМ: купить всё нужное: " + plan.gold + " золота, покупок: " + plan.steps.Count;
        }

        private void ClickGuideItem(GuideSlot slot, PointerEventData.InputButton button)
        {
            if (!session.Ready || CurrentGuide == null || button != PointerEventData.InputButton.Left) return;
            var marks = GuideProgress.Evaluate(Book, Loadout.OwnedIds(), CurrentGuide);
            var shift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            if (marks[slot.step][slot.index] == ItemMark.Done && !shift)
            {
                Say("Уже есть: " + Book.Name(slot.itemId) + ". Shift + ЛКМ покупает ещё один");
                return;
            }
            BuyFinished(slot.itemId);
        }

        /// <summary>Buys what the open guide says comes next.</summary>
        public void BuyNextOfGuide()
        {
            if (!session.Ready) return;
            if (nextItemId == null)
            {
                Say("Все предметы гайда уже собраны");
                return;
            }
            BuyFinished(nextItemId);
        }
    }
}

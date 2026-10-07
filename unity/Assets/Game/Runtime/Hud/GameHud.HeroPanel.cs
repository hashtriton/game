using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public sealed partial class GameHud
    {
        private readonly ItemSlotView[] bagSlots = new ItemSlotView[6];
        private Image healthFill;
        private Image manaFill;
        private Text healthText;
        private Text manaText;
        private Text statsText;
        private Text goldText;
        private Text soulsText;

        private void BuildHeroPanel()
        {
            var panel = UiKit.Picture(transform, "Hero panel", assets.panel, Color.white, true);
            UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(830f, 200f));
            var root = panel.rectTransform;

            var name = UiKit.Label(root, "Name", assets.titleFont, 26, UiColors.Gold, TextAnchor.UpperLeft, hero != null ? hero.Unit.displayName : "Герой");
            Corner(name.rectTransform, 30f, -22f, 260f, 34f);

            healthFill = Bar(root, "Health", new Vector2(30f, -64f), new Vector2(262f, 26f), UiColors.Health, out healthText, 19);
            manaFill = Bar(root, "Mana", new Vector2(30f, -96f), new Vector2(262f, 22f), UiColors.Mana, out manaText, 17);

            statsText = UiKit.Label(root, "Stats", assets.bodyFont, 19, UiColors.Text, TextAnchor.UpperLeft);
            Corner(statsText.rectTransform, 30f, -128f, 270f, 64f);
            statsText.verticalOverflow = VerticalWrapMode.Truncate;

            const float size = 70f;
            const float gap = 6f;
            for (var i = 0; i < bagSlots.Length; i++)
            {
                var slot = new ItemSlotView(assets, root, "Bag " + (i + 1), size, false);
                Corner(slot.root, 326f + (i % 3) * (size + gap), -28f - (i / 3) * (size + gap), size, size);
                var index = i;
                var relay = UiKit.Relay(slot.root.gameObject);
                relay.entered = _ => ShowTooltip(slot.ItemId, BagFooter(index));
                relay.exited = _ => HideTooltip();
                relay.clicked = data =>
                {
                    if (data.button == PointerEventData.InputButton.Right) SellFromBag(index);
                };
                bagSlots[i] = slot;
            }

            var coin = UiKit.Picture(root, "Coin", assets.coin, Color.white);
            Corner(coin.rectTransform, 584f, -30f, 28f, 28f);
            goldText = UiKit.Label(root, "Gold", assets.titleFont, 30, UiColors.Gold, TextAnchor.MiddleLeft, "0");
            Corner(goldText.rectTransform, 620f, -28f, 190f, 32f);
            var soul = UiKit.Picture(root, "Soul", assets.soul, Color.white);
            Corner(soul.rectTransform, 584f, -68f, 28f, 28f);
            soulsText = UiKit.Label(root, "Souls", assets.titleFont, 26, new Color(0.46f, 0.78f, 0.92f), TextAnchor.MiddleLeft, "0");
            Corner(soulsText.rectTransform, 620f, -66f, 190f, 32f);

            PanelButton(root, "ЛАВКИ  [B]", new Vector2(584f, -108f), new Vector2(222f, 38f), () => ToggleShop(false));
            PanelButton(root, "ГАЙДЫ  [G]", new Vector2(584f, -152f), new Vector2(222f, 38f), () => ToggleShop(true));
        }

        private static void Corner(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private Image Bar(RectTransform parent, string name, Vector2 position, Vector2 size, Color color, out Text label, int fontSize)
        {
            var back = UiKit.Picture(parent, name + " bar", assets.slot, Color.white, false);
            Corner(back.rectTransform, position.x, position.y, size.x, size.y);
            var fill = UiKit.Picture(back.rectTransform, "Fill", assets.barFill, color);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            UiKit.Stretch(fill.rectTransform, 3f, 3f, 3f, 3f);
            label = UiKit.Label(back.rectTransform, "Value", assets.bodyFont, fontSize, new Color(0.95f, 0.92f, 0.85f), TextAnchor.MiddleCenter);
            UiKit.Stretch(label.rectTransform);
            return fill;
        }

        private Button PanelButton(RectTransform parent, string text, Vector2 position, Vector2 size, System.Action click)
        {
            var image = UiKit.Picture(parent, text, assets.button, Color.white, true);
            Corner(image.rectTransform, position.x, position.y, size.x, size.y);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = new Color(0.9f, 0.88f, 0.84f);
            colors.highlightedColor = new Color(1.15f, 1.05f, 0.85f);
            colors.pressedColor = new Color(0.65f, 0.6f, 0.55f);
            colors.selectedColor = colors.normalColor;
            button.colors = colors;
            button.onClick.AddListener(() => click());
            var label = UiKit.Label(image.rectTransform, "Text", assets.bodyFont, 20, UiColors.Text, TextAnchor.MiddleCenter, text);
            UiKit.Stretch(label.rectTransform);
            return button;
        }

        private void UpdateHeroPanel()
        {
            if (hero == null) return;
            var unit = hero.Unit;
            healthFill.fillAmount = Mathf.Clamp01(unit.health / unit.MaxHealth);
            healthText.text = Mathf.CeilToInt(unit.health) + " / " + Mathf.CeilToInt(unit.MaxHealth);
            manaFill.fillAmount = unit.MaxMana > 0f ? Mathf.Clamp01(unit.mana / unit.MaxMana) : 0f;
            manaText.text = Mathf.CeilToInt(unit.mana) + " / " + Mathf.CeilToInt(unit.MaxMana);

            statsText.text =
                "Урон  <color=#E6C77A>" + Mathf.RoundToInt(unit.DamageMin) + " - " + Mathf.RoundToInt(unit.DamageMax) + "</color>     Броня  <color=#E6C77A>" +
                ItemInfoBuilder.Number(System.Math.Round(unit.Armor, 1)) + "</color>\n" +
                "Скорость  <color=#E6C77A>" + ItemInfoBuilder.Number(System.Math.Round(unit.MoveSpeed * 64f)) + "</color>     Атака  <color=#E6C77A>" +
                ItemInfoBuilder.Number(System.Math.Round(1f / unit.AttackInterval, 2)) + "/с</color>";

            if (!session.Ready) return;
            goldText.text = Loadout.Gold.ToString();
            soulsText.text = Loadout.Souls.ToString();
            if (ShopOpen) goldShop.text = Loadout.Gold.ToString();
        }

        private void RefreshBag()
        {
            var items = Loadout.Slots;
            for (var i = 0; i < bagSlots.Length; i++)
            {
                var item = items[i];
                if (item == null) bagSlots[i].Clear();
                else
                {
                    var id = item.itemId;
                    bagSlots[i].Set(id, Book.Name(id), UiColors.Grade(Book.TotalCost(id)), "", UiColors.Gold);
                }
            }
            RefreshShopBag(items);
        }

        private string BagFooter(int slot)
        {
            if (!ShopOpen) return null;
            var value = Loadout.SellValue(slot);
            return value >= 0 ? "ПКМ: продать за " + value : "Этот предмет нельзя продать";
        }

        private void SellFromBag(int slot)
        {
            if (!session.Ready || !ShopOpen || Loadout.Slots[slot] == null) return;
            var name = Book.Name(Loadout.Slots[slot].itemId);
            var value = Loadout.SellValue(slot);
            if (value < 0 || !Loadout.Sell(slot))
            {
                Say("Этот предмет нельзя продать");
                return;
            }
            HideTooltip();
            Say("Продано: " + name + " (+" + value + ")");
        }
    }
}

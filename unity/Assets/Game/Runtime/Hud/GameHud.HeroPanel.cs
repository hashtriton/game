using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public sealed partial class GameHud
    {
        private readonly ItemSlotView[] bagSlots = new ItemSlotView[6];
        private readonly Image[] healthSegments = new Image[10];
        private readonly Image[] manaSegments = new Image[10];
        private HudSkin skin;
        private Text healthText, healthMaxText, manaText, manaMaxText, damageText, armorText, statsText, goldText, soulsText;
        private RectTransform heroCluster, ultimateCluster, controlsCluster;
        private float clusterScale = -1f;

        private void BuildHeroPanel()
        {
            skin = Resources.Load<HudSkin>("HudSkin");
            if (skin == null) throw new System.InvalidOperationException("Import HUD skin with Game/HUD/Render hero portrait.");
            var left = UiKit.Rect("Hero panel", transform);
            heroCluster = left;
            UiKit.Place(left, Vector2.zero, Vector2.zero, new Vector2(18f, 12f), new Vector2(660f, 224f));
            var contrast = UiKit.Picture(left, "Local contrast", skin.backing, Color.white);
            Corner(contrast.rectTransform, 150f, 0f, 528f, 224f);
            var portrait = UiKit.Picture(left, "Portrait", skin.portraitMask, new Color(0.03f, 0.12f, 0.19f, 0.90f));
            Corner(portrait.rectTransform, 0f, 0f, 180f, 224f);
            var mask = portrait.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;
            var render = UiKit.Picture(portrait.transform, "Hero render", skin.portrait, Color.white);
            UiKit.Stretch(render.rectTransform);
            var rim = UiKit.Picture(left, "Portrait rim", skin.portraitFrame, HudSkin.Cyan);
            Corner(rim.rectTransform, 0f, 0f, 180f, 224f);

            HudLabel(left, "Name", hero != null ? hero.Unit.displayName.ToUpperInvariant() : "ГЕРОЙ", 22, 208f, 0f, 180f, 30f);
            var level = HudPlate(left, "Level badge", skin.badge, 398f, -1f, 85f, 28f);
            // There is no level system yet; this badge is intentionally constant.
            HudLabel(level.transform, "Level", "Ур. 1", 18, 0f, 0f, 85f, 28f);
            healthText = HudLabel(left, "Health value", "0", 58, 204f, -31f, 210f, 64f);
            healthMaxText = HudLabel(left, "Health maximum", "/ 0", 32, 415f, -51f, 180f, 40f);
            FitNumeral(healthText, 26);
            FitNumeral(healthMaxText, 22);
            Segments(left, "Health", healthSegments, 208f, -100f, 350f, 16f, HudSkin.White);
            manaText = HudLabel(left, "Mana value", "0", 32, 208f, -123f, 146f, 39f, HudSkin.ManaNumeral);
            manaMaxText = HudLabel(left, "Mana maximum", "/ 0", 30, 355f, -123f, 200f, 39f);
            FitNumeral(manaText, 20);
            FitNumeral(manaMaxText, 20);
            Segments(left, "Mana", manaSegments, 208f, -164f, 350f, 11f, HudSkin.Cyan);
            HudGlyph(left, "Attack glyph", skin.attack, 208f, -186f, 28f, HudSkin.White);
            damageText = HudLabel(left, "Damage range", "0 - 0", 23, 245f, -184f, 145f, 32f);
            var line = UiKit.Picture(left, "Stat separator", null, new Color(0.6f, 0.7f, 0.8f, 0.6f));
            Corner(line.rectTransform, 390f, -185f, 1f, 30f);
            HudGlyph(left, "Armor glyph", skin.armor, 405f, -186f, 28f, HudSkin.White);
            armorText = HudLabel(left, "Armor value", "0", 23, 442f, -184f, 75f, 32f);
            statsText = HudLabel(left, "Speeds", "", 20, 534f, -177f, 126f, 46f, HudSkin.White, false);
            statsText.font = assets.bodyFont;

            var ultimate = UiKit.Rect("Ultimate locked", transform);
            ultimateCluster = ultimate;
            UiKit.Place(ultimate, new Vector2(0.46f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(180f, 180f));
            var ring = UiKit.Picture(ultimate, "Ring", skin.ring, Color.white);
            UiKit.Stretch(ring.rectTransform);
            HudGlyph(ultimate, "Ultimate glyph", skin.r, 52f, -37f, 76f, HudSkin.LockedGlyph);
            var charge = HudLabel(ultimate, "Charge", "0%", 30, 25f, -109f, 130f, 37f);
            charge.alignment = TextAnchor.MiddleCenter;
            KeyBadge(ultimate, "R", 63f, -157f, true);

            var right = UiKit.Rect("Controls cluster", transform);
            controlsCluster = right;
            UiKit.Place(right, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 12f), new Vector2(800f, 224f));
            var support = UiKit.Picture(right, "Resource contrast", skin.backing, Color.white);
            Corner(support.rectTransform, -8f, 5f, 338f, 44f);
            HudPlate(right, "Item contrast", skin.backing, -6f, -42f, 806f, 99f);
            HudPlate(right, "Ability contrast", skin.backing, -6f, -140f, 544f, 86f);
            HudPlate(right, "Passive contrast", skin.backing, 585f, -140f, 188f, 86f);
            HudGlyph(right, "Coin", skin.coin, 0f, 0f, 30f, Color.white);
            goldText = HudLabel(right, "Gold", "0", 25, 37f, 0f, 159f, 32f);
            HudGlyph(right, "Soul", skin.soul, 197f, 0f, 30f, Color.white);
            soulsText = HudLabel(right, "Souls", "0", 25, 233f, 0f, 104f, 32f);
            FitNumeral(goldText, 20);
            FitNumeral(soulsText, 20);
            HudButton(right, "ЛАВКА B", 340f, 0f, () => ToggleShop(false));
            HudButton(right, "ГАЙДЫ G", 566f, 0f, () => ToggleShop(true));
            for (var i = 0; i < bagSlots.Length; i++)
            {
                var slot = new ItemSlotView(assets, right, "Bag " + (i + 1), 70f, false, skin);
                Corner(slot.root, i * 133f, -47f, 122f, 70f);
                var index = i;
                var relay = UiKit.Relay(slot.root.gameObject);
                relay.entered = _ => ShowTooltip(slot.ItemId, BagFooter(index));
                relay.exited = _ => HideTooltip();
                relay.clicked = data =>
                {
                    if (data.button == PointerEventData.InputButton.Right) SellFromBag(index);
                };
                KeyBadge(right, (i + 1).ToString(), i * 133f + 41f, -112f, false);
                bagSlots[i] = slot;
            }
            Ability(right, "Q", skin.q, 0f);
            Ability(right, "W", skin.w, 176f);
            Ability(right, "E", skin.e, 352f);
            var divider = UiKit.Picture(right, "Passive separator", null, new Color(0.6f, 0.7f, 0.8f, 0.6f));
            Corner(divider.rectTransform, 557f, -145f, 1f, 76f);
            var passive = HudPlate(right, "Passive locked", skin.plate, 596f, -145f, 162f, 66f);
            HudGlyph(passive.transform, "Passive glyph", skin.passive, 59f, -5f, 48f, HudSkin.LockedGlyph);
            HudGlyph(passive.transform, "Lock", skin.locked, 130f, -40f, 16f, HudSkin.LockedGlyph);
            var passiveLabel = HudLabel(right, "Passive label", "ПАССИВНО", 18, 573f, -207f, 207f, 25f, HudSkin.LockedGlyph);
            passiveLabel.alignment = TextAnchor.MiddleCenter;
            UpdateClusterLayout();
        }

        private void UpdateClusterLayout()
        {
            var scale = Mathf.Min(1f, canvasRect.rect.width / 1920f);
            if (Mathf.Approximately(scale, clusterScale)) return;
            clusterScale = scale;
            heroCluster.localScale = ultimateCluster.localScale = controlsCluster.localScale = Vector3.one * scale;
            heroCluster.anchoredPosition = new Vector2(18f, 12f) * scale;
            ultimateCluster.anchoredPosition = new Vector2(0f, 22f) * scale;
            controlsCluster.anchoredPosition = new Vector2(-24f, 12f) * scale;
        }

        private static void FitNumeral(Text label, int minimum)
        {
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = minimum;
            label.resizeTextMaxSize = label.fontSize;
        }

        private Image HudPlate(Transform parent, string name, Sprite sprite, float x, float y, float width, float height)
        {
            var image = UiKit.Picture(parent, name, sprite, Color.white);
            Corner(image.rectTransform, x, y, width, height);
            return image;
        }

        private Text HudLabel(Transform parent, string name, string text, int size, float x, float y, float width, float height,
            Color? color = null, bool italic = true)
        {
            var label = UiKit.Label(parent, name, assets.titleFont, size, color ?? HudSkin.White, TextAnchor.MiddleLeft, text);
            Corner(label.rectTransform, x, y, width, height);
            label.fontStyle = italic ? FontStyle.Italic : FontStyle.Normal;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            var shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.02f, 0.04f, 0.07f, 0.85f);
            shadow.effectDistance = new Vector2(1f, -2f);
            return label;
        }

        private void HudGlyph(Transform parent, string name, Sprite sprite, float x, float y, float size, Color color)
        {
            var glyph = UiKit.Picture(parent, name, sprite, color);
            Corner(glyph.rectTransform, x, y, size, size);
            glyph.preserveAspect = true;
        }

        private void KeyBadge(Transform parent, string key, float x, float y, bool locked)
        {
            var plate = HudPlate(parent, key + " badge", skin.badge, x, y, 54f, 27f);
            var label = HudLabel(plate.transform, "Key", key, 21, 0f, 0f, 54f, 27f);
            label.alignment = TextAnchor.MiddleCenter;
            if (locked) HudGlyph(plate.transform, "Lock", skin.locked, 41f, -5f, 15f, HudSkin.LockedGlyph);
        }

        private void Ability(Transform parent, string key, Sprite glyph, float x)
        {
            var plate = HudPlate(parent, key + " locked", skin.plate, x, -144f, 152f, 78f);
            var frame = UiKit.Picture(plate.transform, "Outline", skin.frame, HudSkin.White);
            UiKit.Stretch(frame.rectTransform);
            HudGlyph(plate.transform, "Glyph", glyph, 43f, -4f, 68f, HudSkin.LockedGlyph);
            KeyBadge(parent, key, x + 49f, -202f, true);
        }

        private void HudButton(Transform parent, string text, float x, float y, System.Action click)
        {
            var image = HudPlate(parent, text, skin.plate, x, y, 222f, 34f);
            image.raycastTarget = true;
            image.gameObject.AddComponent<HudPlateRaycast>();
            var outline = UiKit.Picture(image.transform, "Outline", skin.frame, HudSkin.White);
            UiKit.Stretch(outline.rectTransform);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.5f, 0.8f, 1f);
            colors.pressedColor = new Color(0.25f, 0.5f, 0.7f);
            button.colors = colors;
            button.onClick.AddListener(() => click());
            var label = HudLabel(image.transform, "Text", text, 23, 8f, 0f, 206f, 34f);
            label.alignment = TextAnchor.MiddleCenter;
        }

        private void Segments(Transform parent, string name, Image[] fills, float x, float y, float width, float height, Color color)
        {
            var step = width / fills.Length;
            for (var i = 0; i < fills.Length; i++)
            {
                HudPlate(parent, name + " empty " + i, skin.segment, x + step * i, y, step - 2f, height).color = new Color(0.23f, 0.3f, 0.37f, 0.85f);
                var fill = HudPlate(parent, name + " segment " + i, skin.segment, x + step * i, y, step - 2f, height);
                fill.color = color;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fills[i] = fill;
            }
        }

        private static void FillSegments(Image[] fills, float fraction)
        {
            for (var i = 0; i < fills.Length; i++) fills[i].fillAmount = Mathf.Clamp01(fraction * fills.Length - i);
        }

        private static void Corner(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
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
            UpdateClusterLayout();
            if (hero == null || hero.Unit == null) return;
            var unit = hero.Unit;
            FillSegments(healthSegments, unit.MaxHealth > 0f ? Mathf.Clamp01(unit.health / unit.MaxHealth) : 0f);
            healthText.text = Mathf.CeilToInt(Mathf.Max(0f, unit.health)).ToString();
            healthMaxText.text = "/ " + Mathf.CeilToInt(Mathf.Max(0f, unit.MaxHealth));
            FillSegments(manaSegments, unit.MaxMana > 0f ? Mathf.Clamp01(unit.mana / unit.MaxMana) : 0f);
            manaText.text = Mathf.CeilToInt(Mathf.Max(0f, unit.mana)).ToString();
            manaMaxText.text = "/ " + Mathf.CeilToInt(Mathf.Max(0f, unit.MaxMana));
            damageText.text = Mathf.RoundToInt(unit.DamageMin) + " - " + Mathf.RoundToInt(unit.DamageMax);
            armorText.text = ItemInfoBuilder.Number(System.Math.Round(unit.Armor, 1));
            statsText.text = "Ход " + Mathf.RoundToInt(unit.MoveSpeed * 64f) + "\n" +
                ItemInfoBuilder.Number(System.Math.Round(1f / unit.AttackInterval, 2)) + " атак/с";
            if (!session.Ready) return;
            goldText.text = Loadout.Gold.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
            soulsText.text = Loadout.Souls.ToString();
            if (ShopOpen && goldShop != null) goldShop.text = Loadout.Gold.ToString();
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

using System;
using System.Collections.Generic;
using System.Globalization;
using Arena.Original;
using UnityEngine;

namespace Game
{
    public struct StatLine
    {
        public string text;
        public Color color;
    }

    /// <summary>Everything a tooltip says about an item, split into the parts it is laid out from.</summary>
    public sealed class ItemInfo
    {
        public string id;
        public string name;
        public string kind;
        /// <summary>What the finished item is worth in gold; -1 when the data has no price.</summary>
        public int totalCost = -1;
        /// <summary>Souls the shop asks for besides gold (summons and a few special items); -1 when none.</summary>
        public int souls = -1;
        /// <summary>Price of the recipe scroll alone, or -1 when the item is not built from a recipe.</summary>
        public int recipeCost = -1;
        public string activeHeader;
        public string active;
        public string passive;
        public readonly List<StatLine> stats = new List<StatLine>();
        public readonly List<string> components = new List<string>();
        public readonly List<string> usedIn = new List<string>();
    }

    public static class ItemInfoBuilder
    {
        private static readonly Color Positive = new Color(0.56f, 0.80f, 0.48f);
        private static readonly Color Magic = new Color(0.46f, 0.66f, 0.92f);

        /// <summary>Builds the description of a finished or basic item (pass the finished item for a recipe scroll).</summary>
        public static ItemInfo Build(ItemBook book, string itemId)
        {
            var item = book.Items.Item(itemId);
            var info = new ItemInfo { id = itemId, name = book.Name(itemId), totalCost = book.TotalCost(itemId) };
            if (item == null) return info;

            var shelf = book.EntryFor(itemId);
            if (shelf != null && shelf.souls > 0)
            {
                info.souls = shelf.souls;
                info.totalCost = shelf.gold > 0 ? shelf.gold : -1;
            }

            var recipe = book.RecipeFor(itemId);
            if (recipe != null)
            {
                info.kind = "Составной предмет";
                info.components.AddRange(book.ComponentsOf(itemId));
                var scroll = book.ScrollFor(itemId);
                var scrollEntry = scroll != null ? book.EntryFor(scroll) : null;
                if (scrollEntry != null && scrollEntry.PriceKnown) info.recipeCost = scrollEntry.gold;
            }
            else if (item.classId == "Permanent") info.kind = "Основной предмет";
            else if (item.classId == "Purchasable" || item.classId == "PowerUp") info.kind = "Расходуемый предмет";
            else info.kind = "Предмет";

            foreach (var used in book.UsedIn(itemId))
                if (!info.usedIn.Contains(used)) info.usedIn.Add(used);

            AddStats(book, itemId, info);

            var active = Clean(book.EffectText.DescribeActive(itemId));
            if (active.Length > 0)
            {
                info.active = active;
                info.activeHeader = ActiveHeader(book, item);
            }
            var passive = Clean(book.EffectText.DescribePassive(itemId));
            if (passive.Length > 0) info.passive = passive;
            return info;
        }

        private static string ActiveHeader(ItemBook book, OriginalItemDefinition item)
        {
            var parts = new List<string>();
            var ability = book.Catalogs.Combat.Ability(item.cooldownId);
            if (ability != null)
            {
                if (ability.TryNumber("Cost1", out var mana, out _) && mana > 0) parts.Add("Мана: " + Number(mana));
                if (ability.TryNumber("Cool1", out var cooldown, out _) && cooldown > 0) parts.Add("Перезарядка: " + Number(cooldown) + " с");
            }
            return parts.Count == 0 ? "Применение" : "Применение  (" + string.Join(", ", parts) + ")";
        }

        private static void AddStats(ItemBook book, string itemId, ItemInfo info)
        {
            try
            {
                var slots = new OriginalItemInstance[6];
                slots[0] = new OriginalItemInstance { instanceId = 1, itemId = itemId, ownerId = 1 };
                var plan = book.Effects.Plan(new OriginalInventorySnapshot
                {
                    ownerId = 1, heroSlots = slots, servantSlots = new OriginalItemInstance[6]
                });
                var profile = plan.NativeProfile;
                if (profile == null || !profile.known) return;

                Add(info, profile.attackDamage, "к урону", Positive);
                Add(info, profile.armor, "к броне", Positive);
                Add(info, profile.strength, "к силе", Positive);
                Add(info, profile.agility, "к ловкости", Positive);
                Add(info, profile.intelligence, "к интеллекту", Positive);
                Add(info, profile.maxHealthFlat, "к здоровью", Positive);
                Add(info, profile.maxManaFlat, "к мане", Magic);
                Add(info, profile.attackSpeedFraction * 100.0, "% к скорости атаки", Positive);
                Add(info, profile.moveSpeedFlat, "к скорости движения", Positive);
                Add(info, profile.healthRegenPerSecond, "здоровья в секунду", Positive);
                Add(info, profile.manaRegenBaseFraction * 100.0, "% к восстановлению маны", Magic);
            }
            catch (Exception)
            {
                // The data cannot describe this item's bonuses; the tooltip simply has no stat block.
            }
        }

        private static void Add(ItemInfo info, double value, string label, Color color)
        {
            if (Math.Abs(value) < 1e-9) return;
            var sign = value > 0 ? "+" : "";
            var glue = label.StartsWith("%") ? "" : " ";
            info.stats.Add(new StatLine { text = sign + Number(value) + glue + label, color = value > 0 ? color : new Color(0.86f, 0.38f, 0.32f) });
        }

        public static string Number(double value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

        private static string Clean(string text) => string.IsNullOrEmpty(text) ? "" : Homoglyphs.Clean(text).Trim();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemEffectTextTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalItemEffectText Create(out OriginalItemCatalog items, out OriginalCombatCatalog combat)
        {
            items = Load<OriginalItemCatalog>("items"); items.BuildIndexes();
            combat = Load<OriginalCombatCatalog>("combat"); combat.BuildIndexes();
            return new OriginalItemEffectText(items, combat, new OriginalInventoryEffects(Load<OriginalItemPassiveCatalog>("item-passives")));
        }

        [Test] public void ShopConversionsAndAllRecipeCatalogEntriesAreReadOnlyAndSafe()
        {
            var text = Create(out var items, out var combat);
            string before = JsonUtility.ToJson(items) + JsonUtility.ToJson(combat);
            var seen = new HashSet<string>();
            foreach (var shop in items.shops)
                foreach (string offer in shop.offerIds)
                {
                    string inventory = items.FromWorld(offer)?.inventoryId ?? offer;
                    seen.Add(inventory);
                    Assert.That(text.Describe(offer), Is.EqualTo(text.Describe(inventory)), offer);
                }
            foreach (var recipe in items.recipes) seen.Add(recipe.resultId);
            foreach (string id in seen)
                Assert.That(text.Describe(id), Does.Not.Contain("NaN").And.Not.Contain("Infinity").And.Not.Contain("unimplemented"), id);
            Assert.That(seen.Count, Is.EqualTo(233));
            Assert.That(JsonUtility.ToJson(items) + JsonUtility.ToJson(combat), Is.EqualTo(before));
        }

        [Test] public void EveryDirectActiveFamilyExplainsAnEffectRatherThanOnlyCostOrCooldown()
        {
            var text = Create(out _, out _);
            string[] ids = {
                "I00Z","I013","I015","I017","I01Y","I01B","I026","I02C","I048","I06R","I06J","I072","I07P","I04B","I07Y",
                "I082","I08U","I090","I09L","I09X","I049","I05D","I05E","I05Q","I07C","I07M","I08D","I0AJ","I0AL","I0AP","I0B2",
                "I01R","I03N","I03S","I03T","I03Y","I0A6","I0B1","I07O","I07T","I087","I076","I07U","I09A","I054","I070","I0AZ","I0B0",
                "I00T","I00V","I04D","I08I","I05A","I045","I096","I08Q","I09P","I05P","I060",
                "I022","I023","I03L","I03M","I01L","I06M","I06O","I02H","I02E","I03Z","I021","I094","I01A","I01D","I01M","I02G","I07E","I07K","I0AE","I0B7",
                "I083","I09M","I09N"
            };
            foreach (string id in ids)
                Assert.That(text.DescribeActive(id).Length, Is.GreaterThan(45), id);
        }

        [Test] public void ConsumableNumbersFollowCatalogAndPassiveItemsDoNotGainInventedUse()
        {
            var text = Create(out _, out var combat);
            var field = Array.Find(combat.Ability("A0B7").fields, value => value.key == "DataA1");
            field.number = 777;
            Assert.That(text.Describe("I03L"), Does.Contain("777 здоровья").And.Contain("один заряд"));
            Assert.That(text.DescribeActive("I007"), Is.Empty);
            Assert.That(text.Describe("xxxx"), Is.Empty);
            Assert.That(text.Describe(null), Is.Empty);
        }

        [Test] public void NativeExceptionsAndRiskyTogglesHaveHonestPlayerText()
        {
            var text = Create(out _, out _);
            Assert.That(text.Describe("I0AJ"), Does.Contain("всего 1 мана").And.Not.Contain("восстанавливает здоровье"));
            Assert.That(text.Describe("I01Y"), Does.Contain("Физический и чистый урон проходят").And.Not.Contain("100 урона"));
            Assert.That(text.Describe("I07C"), Does.Contain("12%").And.Contain("25%").And.Contain("заклинания недоступны"));
            Assert.That(text.Describe("I0AE"), Does.Contain("навыков и предметов").And.Contain("122 с"));
            Assert.That(text.Describe("I07M"), Does.Contain("200").And.Contain("8 импульсов").And.Contain("275"));
            Assert.That(text.DescribeActive("I0B7"), Does.Contain("второй всегда остаётся на земле"));
            Assert.That(text.DescribeActive("I02C"), Does.Contain("примерно 650").And.Contain("600 магического"));
            Assert.That(text.DescribeActive("I09M"), Does.Contain("по цели с аурой").And.Contain("10%").And.Contain("25% ловкости"));
        }

        [Test] public void SpecialPassivesUseEffectiveFamiliesAndKeepIntrinsicBonusesSeparate()
        {
            var text = Create(out _, out var combat);
            Assert.That(text.DescribePassive("I008"), Does.Contain("15%").And.Contain("×1,5"));
            Array.Find(combat.Ability("A00I").fields, f => f.key == "DataB1").number = 2.25;
            Assert.That(text.DescribePassive("I008"), Does.Contain("×2,25"));
            Assert.That(text.DescribePassive("I00F"), Does.Contain("15%").And.Contain("1,25 с").And.Contain("80 дополнительного магического"));
            Assert.That(text.DescribePassive("I06B"), Does.Contain("броню цели на 4 на 6 с").And.Contain("первого удара"));
            Assert.That(text.DescribePassive("I06R"), Does.Contain("45").And.Contain("до учёта брони"));
            Assert.That(text.DescribePassive("I024"), Does.Contain("20%").And.Contain("последний полученный"));
            Assert.That(text.DescribePassive("I02A"), Does.Contain("15").And.Contain("радиусе 180"));
            Assert.That(text.DescribePassive("I007"), Is.Empty, "Plain intrinsic damage belongs to the existing base stats section.");
        }

        [Test] public void AllSixOrbItemsDescribeTheirWorkingSecondaryWithoutRepeatingIntrinsicDamage()
        {
            var text = Create(out _, out _);
            foreach (string id in new[] { "I01N", "I03N", "I0B0" })
                Assert.That(text.DescribePassive(id), Does.Contain("40%").And.Contain("25%").And.Contain("3 с").And.Contain("герои: 1 с"), id);
            foreach (var pair in new[] { ("I02A", "15"), ("I02C", "75"), ("I0A6", "75") })
                Assert.That(text.DescribePassive(pair.Item1), Does.Contain("радиусе 180").And.Contain(pair.Item2 + " урона").And.Contain("основной цели"), pair.Item1);
        }

        [Test] public void AurasLifestealAndSpellPowerExplainTheirDifferentRecipientsAndBases()
        {
            var text = Create(out _, out _);
            Assert.That(text.DescribePassive("I00H"), Does.Contain("Вампиризм атак").And.Contain("фактически снятого"));
            Assert.That(text.DescribePassive("I04B"), Does.Contain("Аура 700").And.Contain("ближнего боя").And.Contain("15%"));
            Assert.That(text.DescribePassive("I0AZ"), Does.Contain("0,5%").And.Contain("здоровья и маны"));
            Assert.That(text.DescribePassive("I05K"), Does.Contain("10%").And.Contain("до защиты цели").And.Contain("половина"));
            Assert.That(text.DescribePassive("I00Z"), Does.Contain("на 40").And.Contain("по-прежнему 5").And.Contain("9 с вместо 6"));
        }

        [Test] public void ChargedRecipesDescribeTheActualProductWithoutPretendingToCast()
        {
            var text = Create(out var items, out _); int count = 0;
            foreach (var item in items.items)
            {
                string link = items.QuickBuy(item.id)?.linkedResultId;
                if (item.classId != "Charged" || link == null || items.QuickBuy(link)?.linkedResultId == null) continue;
                string target = items.QuickBuy(link).linkedResultId;
                Assert.That(text.Describe(item.id), Does.Contain(items.Item(target).displayName).And.Contain("Применение собирает"), item.id);
                count++;
            }
            Assert.That(count, Is.GreaterThanOrEqualTo(86));
        }
    }
}

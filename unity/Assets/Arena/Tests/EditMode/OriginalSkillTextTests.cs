using System;
using System.IO;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSkillTextTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalGameCatalogs Catalogs()
        {
            var result = new OriginalGameCatalogs();
            var combat = Load<OriginalCombatCatalog>("combat"); combat.BuildIndexes();
            var native = Load<OriginalNativeCatalog>("native126"); native.BuildIndexes();
            var observed = Load<OriginalObservedCatalog>("observed126"); observed.BuildIndexes();
            typeof(OriginalGameCatalogs).GetProperty("Combat").SetValue(result, combat);
            typeof(OriginalGameCatalogs).GetProperty("Native").SetValue(result, native);
            typeof(OriginalGameCatalogs).GetProperty("Observed").SetValue(result, observed);
            return result;
        }
        [Test] public void AllFifteenSlotsAndEveryRankProduceReadOnlyRussianDescriptions()
        {
            var c = Catalogs(); string before = JsonUtility.ToJson(c.Combat) + JsonUtility.ToJson(c.Observed);
            int slots = 0;
            foreach (var hero in c.Combat.selectedHeroes)
                foreach (string id in hero.skills)
                {
                    slots++;
                    for (int rank = 0; rank <= (id == "A001" ? 15 : 3); rank++)
                    {
                        string text = OriginalSkillText.Describe(c, hero.id, id, rank);
                        Assert.That(text.Length, Is.GreaterThan(100), hero.id + "/" + id + "/" + rank);
                        Assert.That(text, Does.Contain("Требуется уровень героя:"));
                        Assert.That(text, Does.Not.Contain("NaN").And.Not.Contain("Infinity"));
                    }
                }
            Assert.That(slots, Is.EqualTo(15));
            Assert.That(JsonUtility.ToJson(c.Combat) + JsonUtility.ToJson(c.Observed), Is.EqualTo(before));
        }
        [Test] public void AttributeTextUsesMeasuredRankFourAndFifteenInsteadOfStaleSlkCells()
        {
            var c = Catalogs();
            Assert.That(Array.Find(c.Combat.Ability("A001").fields, field => field.key == "DataA4").number, Is.EqualTo(12));
            Assert.That(OriginalSkillText.Describe(c, "H008", "A001", 4), Does.Contain("+8 к силе").And.Contain("+8 к ловкости").And.Contain("+64 к запасу здоровья"));
            Assert.That(OriginalSkillText.Describe(c, "H008", "A001", 15), Does.Contain("+30 к интеллекту").And.Contain("+300 к запасу маны"));
        }
        [Test] public void KnightTextDistinguishesDamageAxesWhiteCrippleAndImageMultipliers()
        {
            var c = Catalogs();
            Assert.That(OriginalSkillText.Describe(c, "H008", "A05M", 3), Does.Contain("колющий урон снижен на 90%").And.Contain("магический урон - на 60%").And.Contain("движения - на 20%"));
            Assert.That(OriginalSkillText.Describe(c, "H008", "A102", 3), Does.Contain("325").And.Contain("290 урона").And.Contain("6 с").And.Contain("50%").And.Contain("прямые добавки"));
            Assert.That(OriginalSkillText.Describe(c, "H008", "A05N", 2), Does.Contain("30%").And.Contain("165%").And.Contain("100 + 40%").And.Contain("Мана: 80"));
        }
        [Test] public void ArcherTextIncludesFiveElementsAndRankGatedCombinations()
        {
            var c = Catalogs(); string text = OriginalSkillText.Describe(c, "N0A0", "A15X", 3);
            Assert.That(text, Does.Contain("Яд:").And.Contain("Лёд:").And.Contain("Огонь:").And.Contain("Тьма:").And.Contain("Молния:"));
            Assert.That(text, Does.Contain("-15 брони").And.Contain("150 магического").And.Contain("75 соседним").And.Contain("65%").And.Contain("100 магического").And.Contain("Мана: 100"));
            Assert.That(OriginalSkillText.Describe(c, "N0A0", "A0AC", 3), Does.Contain("+60").And.Contain("Уровень 1:").And.Contain("Уровень 2:").And.Contain("Уровень 3:"));
        }
        [Test] public void PyroDescriptionsKeepUpgradeDamageCostAndDurationSeparate()
        {
            var c = Catalogs();
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SP", 2), Does.Contain("50 магического").And.Contain("урон сферы 90").And.Contain("Мана: 150"));
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SR", 2), Does.Contain("90 магического").And.Contain("Мана: 200"));
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SM", 2), Does.Contain("200 магического").And.Contain("60 магического").And.Contain("6 с").And.Contain("до 9 с").And.Contain("375 маны").And.Contain("Мана: 250"));
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SS", 2), Does.Contain("Мана: 375").And.Contain("Перезарядка: 80"));
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SN", 2), Does.Contain("Мана: 0. Перезарядка: 0"));
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SO", 3), Does.Contain("до 900").And.Contain("70 магического").And.Contain("110").And.Contain("Мана: 0"));
        }
        [Test] public void RankZeroPreviewsNextSkillAndWrongHeroCannotBorrowAnotherSkill()
        {
            var c = Catalogs();
            Assert.That(OriginalSkillText.Describe(c, "H008", "A001", 0), Does.Contain("Не изучено").And.Contain("уровень героя: 12").And.Contain("+2 к силе"));
            Assert.That(OriginalSkillText.Describe(c, "H008", "A0SM", 1), Is.Empty);
            Assert.That(OriginalSkillText.Describe(c, "hfoo", "A001", 1), Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalSkillText.Describe(c, "H024", "A0SJ", 4));
        }
        [Test] public void ResourceTextReadsCatalogInsteadOfDuplicatingManaValues()
        {
            var c = Catalogs();
            Array.Find(c.Combat.Ability("A0SJ").fields, field => field.key == "Cost2").number = 151;
            Assert.That(OriginalSkillText.Describe(c, "H024", "A0SJ", 2), Does.Contain("Мана: 151"));
        }
    }
}

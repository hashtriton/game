using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalObservedCatalogTests
    {
        static OriginalObservedCatalog Load()
        {
            var result = JsonUtility.FromJson<OriginalObservedCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-observed126.json")));
            result.BuildIndexes();
            return result;
        }
        [Test] public void NativeUnitMeasurementsLoadAndMissingCreationStaysUnknown()
        {
            var catalog = Load();
            Assert.That(catalog.units.Count(row => row.created), Is.EqualTo(83));
            Assert.That(catalog.Unit("n008").RequireMaxHP(), Is.GreaterThan(0));
            Assert.That(catalog.Unit("n008").RequireMaxMP(), Is.EqualTo(0));
            Assert.That(catalog.Unit("n068").created, Is.False);
            Assert.Throws<InvalidOperationException>(() => catalog.Unit("n068").RequireMaxHP());
            Assert.Throws<InvalidOperationException>(() => catalog.Unit("zzzz").RequireMoveSpeed());
        }
        [Test] public void BaselinesUseExactObservedLevelsWithoutInterpolation()
        {
            var catalog = Load();
            Assert.That(catalog.Hero("N0A0", 11).intelligence, Is.EqualTo(23));
            Assert.That(catalog.Hero("H024", 50).maxHP, Is.EqualTo(1445));
            Assert.That(catalog.Hero("H024", 50).maxMP, Is.EqualTo(1745));
            Assert.That(catalog.TryHero("H024", 28, out _), Is.True);
            Assert.That(catalog.TryHero("H024", 51, out _), Is.False);
            Assert.Throws<InvalidOperationException>(() => catalog.Hero("H024", 0));
        }
        [Test] public void NativeRankGatesAndMissingDeclaredA001RankSixAreMeasured()
        {
            var catalog = Load();
            Assert.That(catalog.Skill("H008", "A05N").minimumHeroLevels, Is.EqualTo(new[] { 1, 3, 5 }));
            Assert.That(catalog.Skill("H008", "A05M").RequiredLevel(2), Is.EqualTo(4));
            Assert.That(catalog.Skill("H024", "A0SM").minimumHeroLevels, Is.EqualTo(new[] { 5, 9, 13 }));
            foreach (var hero in new[] { "H008", "N0A0", "H024" })
            {
                var skill = catalog.Skill(hero, "A001");
                Assert.That(skill.RequiredLevel(6), Is.EqualTo(17));
                var six = skill.rankEffects[5];
                Assert.That(six.strengthBonus, Is.EqualTo(12));
                Assert.That(six.agilityBonus, Is.EqualTo(12));
                Assert.That(six.intelligenceBonus, Is.EqualTo(12));
                Assert.That(six.baseStrengthBonus, Is.Zero);
                Assert.That(six.maxHPBonus, Is.EqualTo(96));
                Assert.That(six.maxMPBonus, Is.EqualTo(120));
                Assert.Throws<InvalidOperationException>(() => skill.RequiredLevel(16));
            }
        }
        [Test] public void MalformedTablesCannotPublishKnownZeroOrIncompleteObservations()
        {
            var catalog = Load(); catalog.units[0].maxHP = double.NaN;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
            catalog = Load(); catalog.heroes[0] = catalog.heroes[1];
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
            catalog = Load(); catalog.sources[1].complete = false;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
            catalog = Load(); catalog.Unit("n068").known = true;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
            Assert.Throws<InvalidOperationException>(() => catalog.Unit("n068").RequireMaxHP());
            catalog = Load(); catalog.skills[0].minimumHeroLevels[0] = 0;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
        }
        [Test] public void OptionalHeroResolverUsesMeasurementsAndLeavesUnmeasuredLevelsUnknown()
        {
            var combat = JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));
            combat.BuildIndexes();
            var measured = OriginalHeroStats.Calculate(combat, "N0A0", 11, Load());
            Assert.That(measured.intelligence.Require(), Is.EqualTo(23));
            Assert.That(measured.maxMana.Require(), Is.EqualTo(400));
            var missing = Load(); missing.Hero("N0A0", 28).known = false;
            var absent = OriginalHeroStats.Calculate(combat, "N0A0", 28, missing);
            Assert.That(absent.strength.known, Is.False);
            Assert.That(absent.maxHealth.known, Is.False);
            Assert.Throws<InvalidOperationException>(() => absent.maxHealth.Require());
        }
        [Test] public void ArmorOnlyPromotesTheValidatedZeroAndVitalityPoliciesStaySeparate()
        {
            var catalog = Load();
            Assert.That(catalog.Unit("n008").RequireArmor(), Is.Zero);
            Assert.Throws<InvalidOperationException>(() => catalog.Unit("n009").RequireArmor());
            foreach (var hero in new[] { "H008", "N0A0", "H024" })
            {
                var level = catalog.VitalityChange(hero, "level-up");
                Assert.That(level.policyKnown, Is.True);
                Assert.That(level.policy, Is.EqualTo("preserve-deficit"));
                var learn = catalog.VitalityChange(hero, "learn-A001");
                Assert.That(learn.policyKnown, Is.False);
                Assert.That(learn.policy, Is.EqualTo("ratio-rounding-unresolved"));
            }
            catalog.VitalityChange("H008", "learn-A001").policyKnown = true;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
        }
    }
}

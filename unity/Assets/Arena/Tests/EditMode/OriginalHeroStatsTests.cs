using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalHeroStatsTests
    {
        static OriginalCombatCatalog Catalog() => JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));

        [Test] public void KnightLevelOneMatchesObservedClassic126ValuesWithoutInventingUiRounding()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "H008", 1);
            Assert.That(stats.strength.Require(), Is.EqualTo(22));
            Assert.That(stats.agility.Require(), Is.EqualTo(6));
            Assert.That(stats.intelligence.Require(), Is.EqualTo(7));
            Assert.That(stats.maxHealth.Require(), Is.EqualTo(631));
            Assert.That(stats.maxMana.Require(), Is.EqualTo(145));
            Assert.That(stats.armor.Require(), Is.EqualTo(5.2).Within(1e-9)); // Source formula; observed UI is 5.
            Assert.That(stats.attackMinimum.Require(), Is.EqualTo(45));
            Assert.That(stats.attackMaximum.Require(), Is.EqualTo(65));
            Assert.That(stats.baseAttackMinimum, Is.EqualTo(12));
            Assert.That(stats.baseAttackMaximum, Is.EqualTo(32));
            Assert.That(stats.limits, Does.Contain("native-ui-number-rounding-unverified"));
        }

        [Test] public void ArcherLevelOneKeepsFractionalDamageAndDeclaredWeaponDice()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "N0A0", 1);
            Assert.That(stats.strength.Require(), Is.EqualTo(5));
            Assert.That(stats.agility.Require(), Is.EqualTo(25));
            Assert.That(stats.intelligence.Require(), Is.EqualTo(5));
            Assert.That(stats.primaryAttribute, Is.EqualTo("AGI"));
            Assert.That(stats.maxHealth.Require(), Is.EqualTo(430));
            Assert.That(stats.maxMana.Require(), Is.EqualTo(220));
            Assert.That(stats.armor.Require(), Is.EqualTo(6));
            Assert.That(stats.attackMinimum.Require(), Is.EqualTo(49.5));
            Assert.That(stats.attackMaximum.Require(), Is.EqualTo(54.5));
            Assert.That(stats.attackDice, Is.EqualTo(1)); Assert.That(stats.attackSides, Is.EqualTo(6));
        }

        [Test] public void PyroLevelOneUsesIntelligenceForDamageAndStrengthForHealth()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "H024", 1);
            Assert.That(stats.strength.Require(), Is.EqualTo(8));
            Assert.That(stats.agility.Require(), Is.EqualTo(7));
            Assert.That(stats.intelligence.Require(), Is.EqualTo(20));
            Assert.That(stats.primaryAttribute, Is.EqualTo("INT"));
            Assert.That(stats.maxHealth.Require(), Is.EqualTo(589));
            Assert.That(stats.maxMana.Require(), Is.EqualTo(275));
            Assert.That(stats.armor.Require(), Is.EqualTo(2.4).Within(1e-9));
            Assert.That(stats.attackMinimum.Require(), Is.EqualTo(46));
            Assert.That(stats.attackMaximum.Require(), Is.EqualTo(66));
        }

        [Test] public void IntegralGrowthKeepsKnightLevelTwoAndFiftyResolvable()
        {
            var level2 = OriginalHeroStats.Calculate(Catalog(), "H008", 2);
            Assert.That(level2.strength.Require(), Is.EqualTo(25));
            Assert.That(level2.agility.Require(), Is.EqualTo(8));
            Assert.That(level2.intelligence.Require(), Is.EqualTo(9));
            Assert.That(level2.maxHealth.Require(), Is.EqualTo(655));
            Assert.That(level2.maxMana.Require(), Is.EqualTo(165));
            Assert.That(level2.attackMinimum.Require(), Is.EqualTo(49.5));
            var level50 = OriginalHeroStats.Calculate(Catalog(), "H008", 50);
            Assert.That(level50.strength.Require(), Is.EqualTo(169));
            Assert.That(level50.agility.Require(), Is.EqualTo(104));
            Assert.That(level50.intelligence.Require(), Is.EqualTo(105));
            Assert.That(level50.maxHealth.Require(), Is.EqualTo(1807));
            Assert.That(level50.maxMana.Require(), Is.EqualTo(1125));
            Assert.That(level50.armor.Require(), Is.EqualTo(24.8).Within(1e-9));
        }

        [Test] public void FractionalGrowthDoesNotSilentlyFloorArcherAttributes()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "N0A0", 2);
            Assert.That(stats.declaredStrength, Is.EqualTo(6.9).Within(1e-9));
            Assert.That(stats.declaredAgility, Is.EqualTo(27.7).Within(1e-9));
            Assert.That(stats.declaredIntelligence, Is.EqualTo(6.8).Within(1e-9));
            Assert.That(stats.strength.known, Is.False); Assert.That(stats.agility.known, Is.False);
            Assert.That(stats.intelligence.known, Is.False); Assert.That(stats.maxHealth.known, Is.False);
            Assert.That(stats.maxMana.known, Is.False); Assert.That(stats.attackMinimum.known, Is.False);
            Assert.Throws<InvalidOperationException>(() => stats.strength.Require());
            Assert.That(stats.strength.unresolved, Does.Contain("fractional-attribute-growth"));
            Assert.That(OriginalHeroStats.Calculate(Catalog(), "N0A0", 11).strength.known, Is.False,
                "An integral mathematical sum does not prove how the engine accumulated fractional growth.");
        }

        [Test] public void UnknownGrowthOnlyBlocksDependentPyroStats()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "H024", 2);
            Assert.That(stats.strength.known, Is.False); Assert.That(stats.agility.known, Is.False);
            Assert.That(stats.maxHealth.known, Is.False); Assert.That(stats.armor.known, Is.False);
            Assert.That(stats.intelligence.Require(), Is.EqualTo(23));
            Assert.That(stats.maxMana.Require(), Is.EqualTo(305));
            Assert.That(stats.attackMinimum.Require(), Is.EqualTo(50.5));
            Assert.That(stats.attackMaximum.Require(), Is.EqualTo(70.5));
        }

        [Test] public void AttackTimingsStayBaseDeclarationsUntilNativeScalingIsVerified()
        {
            var knight = OriginalHeroStats.Calculate(Catalog(), "H008", 1);
            Assert.That(knight.baseMoveSpeed, Is.EqualTo(250)); Assert.That(knight.attackRange, Is.EqualTo(120));
            Assert.That(knight.baseAttackInterval, Is.EqualTo(1.85));
            Assert.That(knight.baseDamagePoint, Is.EqualTo(.5)); Assert.That(knight.baseBackswing, Is.EqualTo(.5));
            Assert.That(knight.agilityAttackSpeedBonus.Require(), Is.EqualTo(.06));
            Assert.That(knight.effectiveAttackInterval.known, Is.False);
            Assert.That(knight.effectiveDamagePoint.known, Is.False); Assert.That(knight.effectiveBackswing.known, Is.False);
            var pyro = OriginalHeroStats.Calculate(Catalog(), "H024", 1);
            Assert.That(pyro.baseMoveSpeed, Is.EqualTo(255)); Assert.That(pyro.attackRange, Is.EqualTo(600));
            Assert.That(pyro.baseAttackInterval, Is.EqualTo(1.9));
            Assert.That(pyro.baseDamagePoint, Is.EqualTo(.75)); Assert.That(pyro.baseBackswing, Is.EqualTo(.78));
            var archer = OriginalHeroStats.Calculate(Catalog(), "N0A0", 1);
            Assert.That(archer.attackRange, Is.EqualTo(550)); Assert.That(archer.baseDamagePoint, Is.EqualTo(.72));
            Assert.That(archer.baseBackswing, Is.EqualTo(.28));
        }

        [Test] public void WeaponRangeIsNotTheScriptedAbilityAttackProxy()
        {
            var stats = OriginalHeroStats.Calculate(Catalog(), "H008", 1);
            var abilityAttack = OriginalHeroRules.AbilityAttack(stats.primary.Require(), 0, Array.Empty<double>());
            Assert.That(abilityAttack, Is.EqualTo(58));
            Assert.That(stats.attackMinimum.Require(), Is.EqualTo(45)); Assert.That(stats.attackMaximum.Require(), Is.EqualTo(65));
        }

        [Test] public void FormulaUsesCatalogCoefficientsAndSnapshotHasNoMutableCatalogReference()
        {
            var catalog = Catalog(); var original = OriginalHeroStats.Calculate(catalog, "H008", 1);
            catalog.constants.Single(x => x.key == "StrHitPointBonus").values[0] = 9;
            var changed = OriginalHeroStats.Calculate(catalog, "H008", 1);
            Assert.That(changed.maxHealth.Require(), Is.EqualTo(653));
            Assert.That(original.maxHealth.Require(), Is.EqualTo(631));
            Assert.That(original.sources, Does.Contain("war3mapMisc.txt:7"));
            Assert.That(original.sources, Does.Contain("Units\\UnitBalance.slk:6307"));
            Assert.That(original.sources, Does.Contain("Units\\UnitWeapons.slk:6782"));
        }

        [Test] public void MissingConflictedOrNonFiniteDeclarationsFailExplicitly()
        {
            var missing = Catalog(); missing.Unit("H008").fields = missing.Unit("H008").fields.Where(x => x.key != "STR").ToArray();
            Assert.Throws<InvalidOperationException>(() => OriginalHeroStats.Calculate(missing, "H008", 1));
            var conflict = Catalog(); conflict.Unit("H008").fields.Single(x => x.key == "HP").conflict = true;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroStats.Calculate(conflict, "H008", 1));
            var invalid = Catalog(); invalid.constants.Single(x => x.key == "StrHitPointBonus").values[0] = double.NaN;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroStats.Calculate(invalid, "H008", 1));
            var overflow = Catalog(); overflow.constants.Single(x => x.key == "StrHitPointBonus").values[0] = double.MaxValue;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroStats.Calculate(overflow, "H008", 1));
        }

        [Test] public void InvalidHeroLevelMapAndDiceAreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => OriginalHeroStats.Calculate(null, "H008", 1));
            Assert.Throws<ArgumentException>(() => OriginalHeroStats.Calculate(Catalog(), "n008", 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalHeroStats.Calculate(Catalog(), "H008", 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalHeroStats.Calculate(Catalog(), "H008", 51));
            var wrongMap = Catalog(); wrongMap.sourceSha256 = new string('0', 64);
            Assert.Throws<ArgumentException>(() => OriginalHeroStats.Calculate(wrongMap, "H008", 1));
            var wrongDice = Catalog(); wrongDice.Unit("H008").fields.Single(x => x.key == "dice1").number = 2.5;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroStats.Calculate(wrongDice, "H008", 1));
        }

        [Test] public void EveryLevelHasFiniteSerializableValuesIncludingUnresolvedSlots()
        {
            var catalog = Catalog();
            foreach (var id in new[] { "H008", "N0A0", "H024" })
                for (var level = 1; level <= 50; level++)
                {
                    var stats = OriginalHeroStats.Calculate(catalog, id, level);
                    foreach (var field in typeof(OriginalHeroStatsSnapshot).GetFields())
                    {
                        if (field.FieldType == typeof(double))
                        {
                            var value = (double)field.GetValue(stats);
                            Assert.That(double.IsNaN(value) || double.IsInfinity(value), Is.False, field.Name);
                        }
                        if (field.FieldType == typeof(OriginalHeroStatValue))
                        {
                            var value = ((OriginalHeroStatValue)field.GetValue(stats)).value;
                            Assert.That(double.IsNaN(value) || double.IsInfinity(value), Is.False, field.Name);
                        }
                    }
                }
        }
    }
}

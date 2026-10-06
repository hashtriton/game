using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalAttackRulesTests
    {
        static OriginalNativeCatalog Catalog() => JsonUtility.FromJson<OriginalNativeCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-native126.json")));

        [Test]
        public void ArmorUsesDifferentPositiveAndNegativeCurvesWithoutUiRounding()
        {
            var catalog = Catalog();
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, 0), Is.EqualTo(1));
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, 10), Is.EqualTo(.625).Within(1e-12));
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, -1), Is.EqualTo(1.06).Within(1e-12));
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, -10), Is.EqualTo(1.4613848859051006).Within(1e-12));
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, 5.2), Is.EqualTo(.7621951219512195).Within(1e-12));
        }

        [Test]
        public void DuelPressureDamageMatchesNativeNegativeArmorPlateau()
        {
            var catalog = Catalog();
            foreach (double armor in new[] { 8.8 - 35, 8.8 - 70 })
                Assert.That(OriginalAttackRules.WeaponDamage(catalog, 40, "chaos", "hero", armor),
                    Is.EqualTo(68.39562988).Within(.0002));
            Assert.That(OriginalAttackRules.ArmorMultiplier(catalog, -19),
                Is.LessThan(OriginalAttackRules.ArmorMultiplier(catalog, -20)));
        }

        [Test]
        public void AttackMatrixUsesMapOverridesAndNativeEightColumnOrder()
        {
            var catalog = Catalog();
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "normal", "medium"), Is.EqualTo(1.5));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "pierce", "hero"), Is.EqualTo(.7));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "magic", "large"), Is.EqualTo(1.75));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "magic", "hero"), Is.EqualTo(.8));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "spells", "hero"), Is.EqualTo(.8));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "hero", "fort"), Is.EqualTo(.5));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "normal", "normal"), Is.EqualTo(1));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "normal", "divine"), Is.EqualTo(.05));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "chaos", "divine"), Is.EqualTo(1));
            Assert.That(OriginalAttackRules.DamageTypeMultiplier(catalog, "siege", "none"), Is.EqualTo(1.5));
        }

        [Test]
        public void OrdinaryWeaponDamageCombinesTypeAndArmorButDoesNotGuessSpellFlags()
        {
            var catalog = Catalog();
            Assert.That(OriginalAttackRules.WeaponDamage(catalog, 100, "normal", "medium", 10), Is.EqualTo(93.75).Within(1e-12));
            Assert.That(OriginalAttackRules.WeaponDamage(catalog, 0, "hero", "hero", -10), Is.Zero);
            Assert.Throws<ArgumentException>(() => OriginalAttackRules.WeaponDamage(catalog, 100, "spells", "hero", 10));
        }

        [Test]
        public void UncappedHeroCooldownUsesMapOnePercentAgilityAndKeepsFractionalTime()
        {
            var catalog = Catalog();
            Assert.That(OriginalAttackRules.UncappedHeroCooldown(catalog, 1.85, 6), Is.EqualTo(1.7452830188679245).Within(1e-12));
            Assert.That(OriginalAttackRules.UncappedHeroCooldown(catalog, 1.9, 25), Is.EqualTo(1.52).Within(1e-12));
            Assert.That(OriginalAttackRules.UncappedHeroCooldown(catalog, 1.9, 7), Is.EqualTo(1.7757009345794392).Within(1e-12));
        }

        [Test]
        public void UnknownNativeValuesCannotBecomeStockDefaults()
        {
            var catalog = Catalog();
            catalog.constants.Single(x => x.key == "DefenseArmor").known = false;
            catalog.constants.Single(x => x.key == "DefenseArmor").state = "unresolved-native-dataset-selection";
            Assert.Throws<InvalidOperationException>(() => OriginalAttackRules.ArmorMultiplier(catalog, 10));
        }

        [Test]
        public void InvalidInputsAndMaterialStringsAreRejected()
        {
            var catalog = Catalog();
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalAttackRules.ArmorMultiplier(catalog, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalAttackRules.WeaponDamage(catalog, -1, "normal", "hero", 5));
            Assert.Throws<ArgumentException>(() => OriginalAttackRules.DamageTypeMultiplier(catalog, "normal", "Wood"));
            Assert.Throws<ArgumentException>(() => OriginalAttackRules.DamageTypeMultiplier(catalog, "unknown", "hero"));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalAttackRules.UncappedHeroCooldown(catalog, 0, 6));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalAttackRules.UncappedHeroCooldown(catalog, 1, double.PositiveInfinity));
        }

        [Test]
        public void MalformedMatrixAndOverflowCannotProduceDamage()
        {
            var catalog = Catalog();
            catalog.BuildIndexes();
            catalog.constants.Single(x => x.key == "DamageBonusNormal").numbers = new[] { 1.0, 1.5 };
            Assert.Throws<InvalidOperationException>(() => OriginalAttackRules.DamageTypeMultiplier(catalog, "normal", "medium"));
            Assert.Throws<OverflowException>(() => OriginalAttackRules.WeaponDamage(Catalog(), double.MaxValue, "pierce", "small", 0));
        }

        [Test]
        public void RepresentableFinalDamageDoesNotOverflowAnIntermediateProduct()
        {
            Assert.That(OriginalAttackRules.WeaponDamage(Catalog(), double.MaxValue, "pierce", "small", 100),
                Is.EqualTo(5.136266099606616E307).Within(1E293));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalHeroRulesTests
    {
        private static OriginalCombatCatalog Catalog() => JsonUtility.FromJson<OriginalCombatCatalog>(
            File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));

        [Test]
        public void Catalog_ContainsOnlyTheThreeChosenHeroesAndEveryReferencedSkill()
        {
            var catalog = Catalog();
            catalog.BuildIndexes();
            Assert.That(catalog.sourceSha256, Is.EqualTo("02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34"));
            Assert.That(catalog.selectedHeroes.Select(x => x.id), Is.EquivalentTo(new[] { "H008", "N0A0", "H024" }));
            foreach (var hero in catalog.selectedHeroes)
                foreach (var skill in hero.skills) Assert.That(catalog.Ability(skill), Is.Not.Null);
        }

        [Test]
        public void BinaryOverrides_WinOverConflictingSlkButMissingLevelRemainsUnknown()
        {
            var attributes = Catalog().Ability("A001");
            Assert.That(attributes.Number("DataA4"), Is.EqualTo(8)); // SLK says 12; binary says 8.
            Assert.That(attributes.Number("DataB4"), Is.EqualTo(8));
            Assert.That(attributes.Number("DataC15"), Is.EqualTo(30));
            Assert.That(attributes.TryNumber("DataA6", out _, out _), Is.False);
            Assert.That(Catalog().Ability("A05M").TryNumber("Cost1", out _, out _), Is.False);
        }

        [Test]
        public void MapConstants_DoNotFallBackToWarcraftStockAttributes()
        {
            var catalog = Catalog();
            Assert.That(catalog.Constant("StrHitPointBonus"), Is.EqualTo(8));
            Assert.That(catalog.Constant("StrAttackBonus"), Is.EqualTo(1.5));
            Assert.That(catalog.Constant("IntManaBonus"), Is.EqualTo(10));
            Assert.That(catalog.Constant("AgiDefenseBonus"), Is.EqualTo(0.2));
            Assert.That(catalog.Constant("DamageBonusSpells", 5), Is.EqualTo(0.8));
            Assert.Throws<InvalidOperationException>(() => catalog.Constant("NeedHeroXP"));
        }

        [Test]
        public void AbilityAttack_UsesPrimaryAndRegistryBonusesInsteadOfWeaponDice()
        {
            Assert.That(OriginalHeroRules.AbilityAttack(22, 0, Array.Empty<double>()), Is.EqualTo(58));
            Assert.That(OriginalHeroRules.AbilityAttack(22, 2, new[] { 0.25, 0.5, 20.0 }), Is.EqualTo(139));
            Assert.That(OriginalHeroRules.KnightMirrorDamage(2, 58), Is.EqualTo(123.2).Within(1e-6));
            Assert.That(OriginalHeroRules.ArcherPowerShotDamage(3, 62.5), Is.EqualTo(178.75));
        }

        [TestCase(0, 0, 275, 1, true)]
        [TestCase(0, 40, 275, 1, true)]
        [TestCase(0, 40.01, 275, 1, false)]
        [TestCase(0, -40, 275, 1, true)]
        [TestCase(0, -41, 275, 1, false)]
        [TestCase(350, 10, 300, 2, true)]
        [TestCase(0, 180, 10, 3, false)]
        [TestCase(0, 0, 326, 3, false)]
        public void ShieldBash_MatchesReversedBearingComparison(double facing, double bearing, double distance, int level, bool expected)
        {
            Assert.That(OriginalHeroRules.KnightShieldContains(facing, bearing, distance, level), Is.EqualTo(expected));
        }

        [Test]
        public void Bow_ChargesDoNotClearOrbUntilNextAttackAndElementsCycle()
        {
            var bow = new OriginalArcherBow();
            Assert.That(bow.Activate(), Is.True);
            for (var i = 0; i < 5; i++) Assert.That(bow.SuccessfulAttack(), Is.EqualTo(OriginalBowElement.Venom));
            Assert.That(bow.Charges, Is.Zero);
            Assert.That(bow.ActiveElement, Is.EqualTo(OriginalBowElement.Venom));
            Assert.That(bow.SuccessfulAttack(), Is.EqualTo(OriginalBowElement.None));
            Assert.That(bow.ComboSum, Is.Zero);
            Assert.That(bow.Activate(), Is.False);
            bow.Advance(18.9);
            Assert.That(bow.NextElement, Is.EqualTo(OriginalBowElement.Ice));
            Assert.That(bow.Activate(), Is.True);
            Assert.That(bow.ActiveElement, Is.EqualTo(OriginalBowElement.Ice));
        }

        [Test]
        public void Bow_ComboUnlocksDependOnUltimateLevelAndResetOnSourceTimer()
        {
            var bow = new OriginalArcherBow();
            bow.VolleyStarted();
            bow.PowerShotStarted();
            Assert.That(bow.PowerShotSplits(0), Is.False);
            Assert.That(bow.PowerShotSplits(1), Is.True);
            bow.SuccessfulAttack();
            Assert.That(bow.ComboSum, Is.EqualTo(3));
            bow.PowerShotFinished();
            bow.Activate();
            bow.PowerShotStarted();
            Assert.That(bow.PowerShotUsesElement(1), Is.False);
            Assert.That(bow.PowerShotUsesElement(2), Is.True);
            bow.Advance(5);
            Assert.That(bow.ComboSum, Is.Zero);
            var volleyBow = new OriginalArcherBow();
            volleyBow.Activate();
            volleyBow.VolleyStarted();
            Assert.That(volleyBow.VolleyUsesElement(2), Is.False);
            Assert.That(volleyBow.VolleyUsesElement(3), Is.True);
        }

        [Test]
        public void Vacuum_ManualDetonationIncrementsDamageCounterWithoutMoving()
        {
            var vacuum = new OriginalPyroVacuum(2);
            vacuum.Advance(0.4);
            Assert.That(vacuum.TravelDistance, Is.EqualTo(250));
            vacuum.Detonate();
            vacuum.Advance(0.04);
            Assert.That(vacuum.Detonated, Is.True);
            Assert.That(vacuum.TravelDistance, Is.EqualTo(250));
            Assert.That(vacuum.DistanceCounter, Is.EqualTo(275));
            Assert.That(vacuum.Damage, Is.EqualTo(100 * (1 + 275.0 / 900)).Within(1e-7));
            vacuum.Advance(10);
            Assert.That(vacuum.DistanceCounter, Is.EqualTo(275));
        }

        [Test]
        public void Vacuum_LargeAndSmallAdvancesReachSameNaturalDetonation()
        {
            var large = new OriginalPyroVacuum(3);
            var small = new OriginalPyroVacuum(3);
            large.Advance(10);
            for (var i = 0; i < 144; i++) small.Advance(0.01);
            Assert.That(large.Damage, Is.EqualTo(400));
            Assert.That(small.Damage, Is.EqualTo(large.Damage));
            Assert.That(small.TravelDistance, Is.EqualTo(900));
        }

        [Test]
        public void Chains_PreserveCumulativeEnumerationQuirk()
        {
            Assert.That(OriginalHeroRules.PyroAreaDamage(100, new[] { false, true, false, true }, 2),
                Is.EqualTo(new[] { 100.0, 150.0, 150.0, 225.0 }));
            Assert.That(OriginalHeroRules.PyroSphereDamage(3, true), Is.EqualTo(110));
            Assert.That(OriginalHeroRules.PyroAreaDamage(70, new[] { false, true }, 0), Is.EqualTo(new[] { 70.0, 70.0 }));
            Assert.That(OriginalHeroRules.ArcherFireDamage(3), Is.EqualTo(75));
        }
    }
}

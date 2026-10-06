using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalHeroRegenTests
    {
        static OriginalCombatCatalog Combat() => JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));
        static OriginalNativeCatalog Native() => JsonUtility.FromJson<OriginalNativeCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-native126.json")));

        [Test] public void SelectedLevelOneRatesUseMapBasesAndNativeAttributeContributions()
        {
            var combat = Combat(); var native = Native();
            var ids = new[] { "H008", "N0A0", "H024" };
            var hp = new[] { 2.4, 1.25, 1.55 }; var mana = new[] { .4, .3, 1.01 };
            for (var i = 0; i < ids.Length; i++)
            {
                var value = OriginalHeroRegen.Calculate(combat, native, ids[i], 1);
                Assert.That(value.healthPerSecond.Require(), Is.EqualTo(hp[i]).Within(1e-12), ids[i]);
                Assert.That(value.manaPerSecond.Require(), Is.EqualTo(mana[i]).Within(1e-12), ids[i]);
                Assert.That(value.regenerationType, Is.EqualTo("always"));
                Assert.That(value.healthPerStrength, Is.EqualTo(.05));
                Assert.That(value.manaPerIntelligence, Is.EqualTo(.05));
                Assert.That(value.sourceSha256, Is.EqualTo(OriginalNativeCatalog.ExpectedMapSha256));
                Assert.That(value.limits, Does.Contain("native-regeneration-tick-cadence-unverified"));
                Assert.That(value.sources.Any(x => x.Contains("MiscGame.txt:137")), Is.True);
                Assert.That(value.sources.Any(x => x.Contains("MiscGame.txt:139")), Is.True);
            }
        }

        [Test] public void IntegralKnightGrowthRemainsKnownAndOnlyDependentFractionalRatesBecomeUnknown()
        {
            var combat = Combat(); var native = Native();
            var knight = OriginalHeroRegen.Calculate(combat, native, "H008", 50);
            Assert.That(knight.healthPerSecond.Require(), Is.EqualTo(9.75).Within(1e-12));
            Assert.That(knight.manaPerSecond.Require(), Is.EqualTo(5.3).Within(1e-12));
            var pyro = OriginalHeroRegen.Calculate(combat, native, "H024", 2);
            Assert.That(pyro.healthPerSecond.known, Is.False);
            Assert.That(pyro.healthPerSecond.unresolved, Does.Contain("fractional-attribute-growth"));
            Assert.That(pyro.manaPerSecond.Require(), Is.EqualTo(1.16).Within(1e-12));
            var archer = OriginalHeroRegen.Calculate(combat, native, "N0A0", 11);
            Assert.That(archer.healthPerSecond.known, Is.False);
            Assert.That(archer.manaPerSecond.known, Is.False);
            Assert.Throws<InvalidOperationException>(() => archer.healthPerSecond.Require());
        }

        [Test] public void MissingUnknownOrConflictedInputsNeverBecomeZeroOrStockDefaults()
        {
            var missing = Combat();
            missing.Unit("H008").fields = missing.Unit("H008").fields.Where(x => x.key != "regenHP").ToArray();
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(missing, Native(), "H008", 1));
            var conflict = Combat(); conflict.Unit("H008").fields.Single(x => x.key == "regenMana").conflict = true;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(conflict, Native(), "H008", 1));
            var unknown = Native(); var coefficient = unknown.constants.Single(x => x.key == "StrRegenBonus");
            coefficient.known = false; coefficient.state = "unresolved-test"; coefficient.number = .05;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(Combat(), unknown, "H008", 1));
            var absent = Native(); absent.constants = absent.constants.Where(x => x.key != "IntRegenBonus").ToArray();
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(Combat(), absent, "H008", 1));
        }

        [Test] public void DifferentOrUnresolvedRegenerationConditionsAreNotTreatedAsAlways()
        {
            foreach (var type in new[] { "night", "blight", "", null })
            {
                var combat = Combat(); combat.Unit("H008").fields.Single(x => x.key == "regenType").text = type;
                Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(combat, Native(), "H008", 1));
            }
            var conflicted = Combat(); conflicted.Unit("H008").fields.Single(x => x.key == "regenType").conflict = true;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(conflicted, Native(), "H008", 1));
        }

        [Test] public void NonFiniteInputsAndArithmeticOverflowFailBeforePublishingASnapshot()
        {
            var combat = Combat(); combat.Unit("H008").fields.Single(x => x.key == "regenHP").number = double.NaN;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(combat, Native(), "H008", 1));
            var native = Native(); native.constants.Single(x => x.key == "StrRegenBonus").number = double.MaxValue;
            Assert.Throws<InvalidOperationException>(() => OriginalHeroRegen.Calculate(Combat(), native, "H008", 1));
        }

        [Test] public void ResultIsDetachedAndUsesSuppliedResolvedCoefficients()
        {
            var combat = Combat(); var native = Native();
            var before = OriginalHeroRegen.Calculate(combat, native, "H008", 1);
            var hpBase = combat.Unit("H008").fields.Single(x => x.key == "regenHP");
            hpBase.number = 2; hpBase.sources[0] = "modified-source";
            native.constants.Single(x => x.key == "StrRegenBonus").number = .1;
            var after = OriginalHeroRegen.Calculate(combat, native, "H008", 1);
            Assert.That(after.healthPerSecond.Require(), Is.EqualTo(4.2).Within(1e-12));
            Assert.That(before.healthPerSecond.Require(), Is.EqualTo(2.4).Within(1e-12));
            Assert.That(before.baseHealthPerSecond, Is.EqualTo(1.3));
            Assert.That(before.sources, Does.Not.Contain("modified-source"));
        }

        [Test] public void InvalidCatalogHeroAndLevelAreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => OriginalHeroRegen.Calculate(null, Native(), "H008", 1));
            Assert.Throws<ArgumentNullException>(() => OriginalHeroRegen.Calculate(Combat(), null, "H008", 1));
            Assert.Throws<ArgumentException>(() => OriginalHeroRegen.Calculate(Combat(), Native(), "n008", 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalHeroRegen.Calculate(Combat(), Native(), "H008", 0));
            var native = Native(); native.mapSha256 = new string('0', 64);
            Assert.Throws<ArgumentException>(() => OriginalHeroRegen.Calculate(Combat(), native, "H008", 1));
        }
    }
}

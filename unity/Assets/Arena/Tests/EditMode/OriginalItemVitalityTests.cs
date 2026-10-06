using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemVitalityTests
    {
        static OriginalItemPassiveCatalog Load() => JsonUtility.FromJson<OriginalItemPassiveCatalog>(
            File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-item-passives.json")));

        [Test]
        public void SequentialOccurrenceRuleReproducesAll344CapturedResourceTransitions()
        {
            var source = Load(); var effects = new OriginalInventoryEffects(source); var count = 0;
            foreach (var item in source.observedEquip.items)
            {
                int[] before = { 0, 2, 4, 6 }; int[] after = { 1, 3, 5, 7 };
                for (var i = 0; i < 4; i++)
                {
                    var a = item.snapshots[before[i]]; var b = item.snapshots[after[i]];
                    var result = effects.ChangeVitality(item.id, i < 2, a.health, a.maxHealth, a.mana, a.maxMana, 8, 10).Require();
                    Assert.That(result.health, Is.EqualTo(b.health), item.id + ":" + b.phase + ":HP");
                    Assert.That(result.mana, Is.EqualTo(b.mana), item.id + ":" + b.phase + ":MP");
                    Assert.That(result.maxHealth, Is.EqualTo(b.maxHealth));
                    Assert.That(result.maxMana, Is.EqualTo(b.maxMana));
                    count += 2;
                }
            }
            Assert.That(count, Is.EqualTo(344));
        }

        [Test]
        public void MultiAbilityManaIsRoundedAfterEachOccurrence_NotOnceAtFinalMaximum()
        {
            var source = Load(); var effects = new OriginalInventoryEffects(source);
            var a = source.observedEquip.items.Single(x => x.id == "I04F").snapshots[2];
            var result = effects.ChangeVitality("I04F", true, a.health, a.maxHealth, a.mana, a.maxMana, 8, 10).Require();
            Assert.That(result.mana, Is.EqualTo(573));
            Assert.That(Math.Floor(a.mana * result.maxMana / a.maxMana + .5), Is.EqualTo(574));
            Assert.That(result.steps.Count(x => x.manaMaximumDelta != 0), Is.EqualTo(2));
        }

        [Test]
        public void DuplicateAbilityOccurrencesAndRemovalRemainSeparateAndDetached()
        {
            var effects = new OriginalInventoryEffects(Load());
            var one = effects.ChangeVitality("I050", true, 315.739, 631, 72.5, 145, 8, 10).Require();
            Assert.That(one.health, Is.EqualTo(460));
            Assert.That(one.maxHealth, Is.EqualTo(919));
            Assert.That(one.steps.Count(x => x.abilityId == "A00T" && x.healthMaximumDelta == 144), Is.EqualTo(2));
            var removed = effects.ChangeVitality("I050", false, one.health, one.maxHealth, one.mana, one.maxMana, 8, 10).Require();
            Assert.That(removed.maxHealth, Is.EqualTo(631));
            one.steps[1].healthMaximumDelta = 999;
            Assert.That(effects.ChangeVitality("I050", true, 315.739, 631, 72.5, 145, 8, 10).Require().maxHealth, Is.EqualTo(919));
        }

        [Test]
        public void OrdinaryZeroVitalityChangePreservesFractionalOrZeroResourcesExactly()
        {
            var effects = new OriginalInventoryEffects(Load());
            var result = effects.ChangeVitality("I000", true, .125, 631, 0, 0, 8, 10).Require();
            Assert.That(result.health, Is.EqualTo(.125));
            Assert.That(result.mana, Is.Zero);
            Assert.That(result.maxMana, Is.Zero);
            Assert.That(result.maxHealth, Is.EqualTo(631));
        }

        [Test]
        public void UnknownItemsAndLowHealthRoundingDoNotPublishPartialResourceChanges()
        {
            var effects = new OriginalInventoryEffects(Load());
            var unknown = effects.ChangeVitality("I05F", true, 100, 631, 50, 145, 8, 10);
            Assert.That(unknown.known, Is.False);
            Assert.Throws<InvalidOperationException>(() => unknown.Require());
            var low = effects.ChangeVitality("I04F", false, .125, 931, 100, 645, 8, 10);
            Assert.That(low.known, Is.False);
            Assert.That(low.health, Is.EqualTo(.125));
            Assert.That(low.maxHealth, Is.EqualTo(931));
            Assert.That(low.mana, Is.EqualTo(100));
            Assert.That(low.maxMana, Is.EqualTo(645));
            Assert.That(low.unresolved, Does.Contain("zero"));
            Assert.That(effects.ChangeVitality("I04F", true, 100, 631, 0, 0, 8, 10).known, Is.False);
        }

        [Test]
        public void NativeItemResourceProbeKeepsAllElevenNearDeathTransitionsAliveAtOne()
        {
            // LiAItemR1 ba4a1fcb34ce...19f6fc8b, exact immediate getters.
            var effects = new OriginalInventoryEffects(Load());
            double[] removals = { .40509033203125, .45001220703125, .4990234375, .5,
                .5001220703125, .75, 1, 1.0009765625 };
            foreach (double health in removals)
            {
                var result = effects.ChangeVitality("I00C", false, health, 727, 29, 145, 8, 10).Require();
                Assert.That(result.health, Is.EqualTo(1)); Assert.That(result.maxHealth, Is.EqualTo(631));
            }
            foreach (double health in new[] { .40509033203125, .45001220703125, .5001220703125 })
            {
                var result = effects.ChangeVitality("I00C", true, health, 631, 29, 145, 8, 10).Require();
                Assert.That(result.health, Is.EqualTo(1)); Assert.That(result.maxHealth, Is.EqualTo(727));
            }
        }

        [Test]
        public void NativeItemHalfTieMatrixMatchesWithoutAStateLookupAndRemainsMarkedApproximate()
        {
            var effects = new OriginalInventoryEffects(Load());
            double[,] rows = {
                {205.4990234375, 822, 229}, {205.5, 822, 229}, {205.5009765625, 822, 230},
                {616.4989624023438, 822, 688}, {616.5, 822, 688}, {616.5009765625, 822, 689},
                {261.49896240234375, 1046, 285}, {261.5, 1046, 286}, {261.5009765625, 1046, 286},
                {784.4990234375, 1046, 856}, {784.5000610351562, 1046, 857}, {784.5010375976562, 1046, 857}
            };
            for (int i = 0; i < rows.GetLength(0); i++)
            {
                var result = effects.ChangeVitality("I00C", true, rows[i, 0], rows[i, 1], 50, 145, 8, 10).Require();
                Assert.That(result.health, Is.EqualTo(rows[i, 2]));
                Assert.That(result.approximate, Is.True);
                Assert.That(result.mana, Is.EqualTo(50));
            }
        }

        [Test]
        public void InvalidVitalsAndCoefficientsFailWithoutMutation()
        {
            var effects = new OriginalInventoryEffects(Load());
            Assert.Throws<ArgumentException>(() => effects.ChangeVitality("I000", true, 632, 631, 50, 145, 8, 10));
            Assert.Throws<ArgumentException>(() => effects.ChangeVitality("I000", true, 100, 631, 50, 145, double.NaN, 10));
            Assert.Throws<ArgumentException>(() => effects.ChangeVitality("I000", true, 100, 631, 50, 145, 8, -1));
        }
    }
}

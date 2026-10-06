using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemPassiveTests
    {
        private static OriginalItemPassiveCatalog Load()
        {
            return JsonUtility.FromJson<OriginalItemPassiveCatalog>(File.ReadAllText(
                Path.Combine(Application.dataPath, "Arena/Data/lia39-item-passives.json")));
        }

        [Test]
        public void SourcePassives_KeepSpellbookClosureAndOriginalAmounts()
        {
            var catalog = Load();
            Assert.That(catalog.directAbilityCount, Is.EqualTo(328));
            Assert.That(catalog.spellbookClosureCount, Is.EqualTo(351));
            Assert.That(catalog.mappedAbilityCount, Is.EqualTo(169));
            Assert.That(catalog.abilities.Count(a => a.passiveFamilyMapped), Is.EqualTo(catalog.mappedAbilityCount));
            Assert.That(catalog.abilities.Where(a => a.passiveFamilyMapped &&
                (a.baseCode == "AUts" || a.levels.Any(l => l.modifiers.Any(m => m.field == "Idam")))).Select(a => a.id),
                Is.EquivalentTo(new[] { "A059", "A062", "A07K", "A08B", "A0BJ", "A0FT", "A0TO", "A16T", "AIfb", "AIpb" }),
                "Uts3 armor and Idam orb damage account for the ten newly mapped source abilities.");
            var evaluation = new OriginalItemPassiveEvaluator(catalog).Evaluate(new[]
            {
                new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = "A00A", Level = 1 },
                new OriginalItemAbilityState { ItemInstanceId = 2, AbilityId = "A07M", Level = 1 },
                new OriginalItemAbilityState { ItemInstanceId = 3, AbilityId = "A014", Level = 1 }
            });
            Assert.That(evaluation.Modifiers.Single(x => x.AbilityId == "A00A").Value, Is.EqualTo(16));
            Assert.That(evaluation.Modifiers.Single(x => x.AbilityId == "A07M").Value, Is.EqualTo(.15).Within(.000001));
            Assert.That(evaluation.Modifiers.Single(x => x.AbilityId == "A014").Stat, Is.EqualTo("manaRegenBaseFraction"));
            Assert.That(evaluation.AllRequestedFieldsMapped, Is.True);
            Assert.That(evaluation.RetailStackingVerified, Is.False);
            Assert.That(evaluation.RuntimeBaselineObserved, Is.False);
        }

        [Test]
        public void AttributeColumns_DoNotConvertMissingAttributesIntoZero()
        {
            var evaluation = new OriginalItemPassiveEvaluator(Load()).Evaluate(new[]
            { new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = "A00F", Level = 1 } });
            Assert.That(evaluation.Modifiers.Single().Stat, Is.EqualTo("strength"));
            Assert.That(evaluation.Modifiers.Single().Value, Is.EqualTo(12));
            Assert.That(evaluation.Gaps.Count, Is.EqualTo(2));
            Assert.That(evaluation.AllRequestedFieldsMapped, Is.False);
        }

        [Test]
        public void BinaryOverride_PreservedWithoutPretendingUndeclaredLevelIsUsable()
        {
            var catalog = Load();
            var ability = catalog.abilities.Single(x => x.id == "A0JF");
            var level = ability.levels.Single(x => x.level == 4);
            Assert.That(level.modifiers.Single().value, Is.EqualTo(240));
            Assert.That(level.modifiers.Single().state, Is.EqualTo("binary-override"));
            Assert.That(level.modifiers.Single().sources.Single(), Does.Contain("byte 5208"));
            var evaluation = new OriginalItemPassiveEvaluator(catalog).Evaluate(new[]
            { new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = "A0JF", Level = 4 } });
            Assert.That(evaluation.Modifiers, Is.Empty);
            Assert.That(evaluation.Gaps.Single().Reason, Is.EqualTo("level-not-supported-by-declaration"));
        }

        [Test]
        public void UnknownFamiliesAndSpellbookLevels_AreExplicitGaps()
        {
            var evaluator = new OriginalItemPassiveEvaluator(Load());
            var evaluation = evaluator.Evaluate(new[]
            {
                new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = "A0B7", Level = 1 },
                new OriginalItemAbilityState { ItemInstanceId = 2, AbilityId = "A0JU", Level = 1 }
            });
            Assert.That(evaluation.Gaps.Any(x => x.Reason == "native-family-unimplemented:AIhe"), Is.True);
            Assert.That(evaluation.Gaps.Any(x => x.Reason == "spellbook-child-level-unresolved"), Is.True);
            Assert.That(evaluation.Modifiers, Is.Empty);
        }

        [Test]
        public void RepeatedRequests_DoNotDuplicateAnInstanceContribution_ButDifferentInstancesStaySeparate()
        {
            var first = new OriginalItemAbilityState { ItemInstanceId = 10, AbilityId = "A00A", Level = 1 };
            var second = new OriginalItemAbilityState { ItemInstanceId = 11, AbilityId = "A00A", Level = 1 };
            var evaluation = new OriginalItemPassiveEvaluator(Load()).Evaluate(new[] { first, first, second });
            Assert.That(evaluation.Modifiers.Count, Is.EqualTo(2));
            Assert.That(evaluation.Modifiers.Select(x => x.ItemInstanceId), Is.EquivalentTo(new long[] { 10, 11 }));
            Assert.That(evaluation.Modifiers.All(x => x.StackingRule == "unresolved-retail-stacking"), Is.True);
        }

        [Test]
        public void ConflictingLevelsForOneInstance_AreRejectedBeforeEvaluation()
        {
            var evaluator = new OriginalItemPassiveEvaluator(Load());
            Assert.Throws<System.ArgumentException>(() => evaluator.Evaluate(new[]
            {
                new OriginalItemAbilityState { ItemInstanceId = 10, AbilityId = "A00A", Level = 1 },
                new OriginalItemAbilityState { ItemInstanceId = 10, AbilityId = "A00A", Level = 2 }
            }));
        }
    }
}

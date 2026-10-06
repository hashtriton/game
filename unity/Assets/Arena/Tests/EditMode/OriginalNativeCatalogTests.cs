using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalNativeCatalogTests
    {
        private static OriginalNativeCatalog Catalog()
        {
            var path = Path.Combine(Application.dataPath, "Arena/Data/lia39-native126.json");
            var result = JsonUtility.FromJson<OriginalNativeCatalog>(File.ReadAllText(path));
            result.BuildIndexes();
            return result;
        }

        [Test] public void RealDeclarationsLoadWithoutChoosingAnUnresolvedDataset()
        {
            var catalog = Catalog();
            Assert.That(catalog.runtimeObserved, Is.False);
            Assert.That(catalog.datasetSelectionKnown, Is.False);
            Assert.That(catalog.originalMapAuthorEngineVersionKnown, Is.False);
            Assert.That(catalog.Constant("MaxHeroLevel").Require(), Is.EqualTo(50));
            Assert.That(catalog.Constant("StrHitPointBonus").Require(), Is.EqualTo(8));
            Assert.That(catalog.items.Length, Is.EqualTo(440));
            Assert.That(catalog.destructables.Length, Is.EqualTo(3));
        }

        [Test] public void CumulativeXpUsesInclusiveBoundariesAndCapsAtTheMaximum()
        {
            var catalog = Catalog();
            Assert.That(catalog.LevelFromExperience(0), Is.EqualTo(1));
            foreach (var row in catalog.heroXp.levels.Skip(1))
            {
                Assert.That(catalog.LevelFromExperience(row.cumulative - 1), Is.EqualTo(row.level - 1));
                Assert.That(catalog.LevelFromExperience(row.cumulative), Is.EqualTo(row.level));
            }
            Assert.That(catalog.LevelFromExperience(5400), Is.EqualTo(10));
            Assert.That(catalog.LevelFromExperience(127400), Is.EqualTo(50));
            Assert.That(catalog.LevelFromExperience(int.MaxValue), Is.EqualTo(50));
            Assert.Throws<ArgumentOutOfRangeException>(() => catalog.LevelFromExperience(-1));
        }

        [Test] public void UnknownAndMissingFieldsNeverTurnIntoZeroOrNativeCandidates()
        {
            var catalog = Catalog();
            var conflict = catalog.constants.First(row => !row.known);
            Assert.That(catalog.Constant(conflict.key).state, Is.EqualTo("unresolved-native-dataset-selection"));
            Assert.Throws<InvalidOperationException>(() => catalog.Constant(conflict.key).Require());
            var absent = catalog.ItemField("I000", "lumbercost");
            Assert.That(absent.known, Is.False);
            Assert.That(absent.state, Is.EqualTo("unresolved-absent-map-slk-cell"));
            Assert.Throws<InvalidOperationException>(() => absent.Require());
            Assert.Throws<InvalidOperationException>(() => catalog.ItemField("ZZZZ", "goldcost").Require());
            Assert.Throws<InvalidOperationException>(() => catalog.Constant("missing").Require());
            Assert.Throws<InvalidOperationException>(() => catalog.DestructableHP("B000").Require());
        }

        [Test] public void KnownItemAndBarrelValuesRetainTheirEvidenceAndTypes()
        {
            var catalog = Catalog();
            var gold = catalog.ItemField("I000", "goldcost");
            Assert.That(gold.Require(), Is.EqualTo(65));
            Assert.That(gold.state, Is.EqualTo("map-declaration"));
            Assert.That(gold.sources.Length, Is.GreaterThan(0));
            Assert.That(catalog.ItemField("I000", "cooldownID").RequireText(), Is.EqualTo("A00A"));
            Assert.That(catalog.DestructableHP("LTbr").Require(), Is.EqualTo(10));
            Assert.That(catalog.DestructableHP("LTbr").state, Is.EqualTo("map-binary-override"));
            Assert.That(catalog.DestructableHP("LTbs").Require(), Is.EqualTo(20));
            Assert.That(catalog.DestructableHP("LTex").Require(), Is.EqualTo(20));
            Assert.That(catalog.DestructableField("LTbr", "pathTexDeath").RequireText(), Is.EqualTo("_"));
        }

        [Test] public void VectorValuesCannotBeReadAsScalarsAndReturnedArraysAreCopies()
        {
            var vector = Catalog().constants.First(row => row.known && row.kind == "numbers");
            var values = vector.RequireNumbers();
            var first = vector.numbers[0];
            values[0] = first + 100;
            Assert.That(vector.RequireNumbers()[0], Is.EqualTo(first));
            Assert.Throws<InvalidOperationException>(() => vector.Require());
            Assert.Throws<InvalidOperationException>(() => vector.RequireText());
        }

        [Test] public void MonotonicButIncorrectXpTableIsRejectedAgainstTheFormula()
        {
            var catalog = Catalog();
            catalog.heroXp.levels[9].cumulative++;
            catalog.heroXp.levels[9].fromPrevious++;
            catalog.heroXp.levels[10].fromPrevious--;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog();
            catalog.Constant("NeedHeroXPFormulaB").number++;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
        }

        [Test] public void WrongMapVersionAndUnprovenEvidenceFlagsAreRejected()
        {
            var catalog = Catalog(); catalog.mapSha256 = new string('0', 64);
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.clientFileVersion = "1.27";
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.datasetSelectionKnown = true;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.runtimeObserved = true;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            Assert.Throws<ArgumentException>(() => new OriginalNativeCatalog().BuildIndexes());
        }

        [Test] public void DuplicateObjectFieldAndConstantIdentitiesAreRejected()
        {
            var catalog = Catalog(); catalog.items[1].id = catalog.items[0].id;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.items[0].fields[1].field = catalog.items[0].fields[0].field;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.constants[1].key = catalog.constants[0].key;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
        }

        [Test] public void NonfiniteKnownAndUnresolvedKnownValuesAreRejected()
        {
            var catalog = Catalog(); catalog.Constant("StrHitPointBonus").number = double.NaN;
            Assert.Throws<InvalidOperationException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.Constant("StrHitPointBonus").state = "unresolved-native-dataset-selection";
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            Assert.Throws<InvalidOperationException>(() => catalog.Constant("StrHitPointBonus").Require());
            catalog = Catalog(); catalog.constants.First(row => !row.known).state = "map-declaration";
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
        }

        [Test] public void MissingBarrelHealthAndInvalidEvidenceReferencesAreRejected()
        {
            var catalog = Catalog();
            catalog.destructables[0].fields = catalog.destructables[0].fields.Where(row => row.field != "HP").ToArray();
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.ItemField("I000", "goldcost").sources[0].source = int.MaxValue;
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
            catalog = Catalog(); catalog.heroXp.dependencies = new[] { "MaxHeroLevel" };
            Assert.Throws<ArgumentException>(() => catalog.BuildIndexes());
        }
    }
}

using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalObservedSparseTests
    {
        static OriginalObservedCatalog Load() => JsonUtility.FromJson<OriginalObservedCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-observed126.json")));
        [Test] public void ExactSparseBatchKeepsBaselineAndOrnNativeProfileSeparate()
        {
            var catalog = Load(); catalog.BuildIndexes();
            Assert.That(catalog.sources.Length, Is.EqualTo(3)); Assert.That(catalog.heroes.Length, Is.EqualTo(150));
            var orn = catalog.sparse.Unit("O006", 50);
            Assert.That(orn.RequireMaxHP(), Is.EqualTo(30000)); Assert.That(orn.RequireMaxMP(), Is.EqualTo(2500));
            Assert.That(orn.RequireStrength(), Is.EqualTo(250)); Assert.That(orn.RequireArmor(), Is.EqualTo(80));
            Assert.That(catalog.sparse.Unit("n00D").RequireArmor(), Is.Zero);
            Assert.That(catalog.sparse.Unit("n00D").intactChaosNormalRatio, Is.EqualTo(.2));
            Assert.Throws<InvalidOperationException>(() => catalog.sparse.Unit("O006", 2).RequireMaxHP());
            Assert.Throws<InvalidOperationException>(() => catalog.sparse.Unit("n06I").RequireArmor());
        }
        [Test] public void ProjectionMutationAndMissingBodyProofAreRejected()
        {
            var c = Load().sparse; c.units[3].armor = 66.6667;
            Assert.Throws<InvalidOperationException>(() => c.BuildIndexes());
            c = Load().sparse; c.bodyRows[23].samples = new OriginalObservedBodyPosition[0];
            Assert.Throws<InvalidOperationException>(() => c.BuildIndexes());
        }
        [Test] public void ReturnedRowsAndFailedRebuildCannotChangePublishedIndex()
        {
            var c = Load().sparse; c.BuildIndexes(); c.Unit("O006", 50).armor = 0;
            c.units[10].armor = 31;
            Assert.That(c.Unit("O006", 50).RequireArmor(), Is.EqualTo(80));
            Assert.Throws<InvalidOperationException>(() => c.BuildIndexes());
            Assert.That(c.Unit("O006", 50).RequireArmor(), Is.EqualTo(80));
        }
        [Test] public void UnknownUnsafeDamageAndNonlinearControlCannotBecomeArmor()
        {
            var c = Load().sparse;
            Assert.That(c.armorRows[16].damage[2].known, Is.False);
            c.armorRows[16].damage[2].known = true;
            Assert.Throws<InvalidOperationException>(() => c.BuildIndexes());
            c = Load().sparse; c.armorRows[1].damage[2].eventDamage = 30; c.armorRows[1].damage[2].after = 170;
            Assert.Throws<InvalidOperationException>(() => c.BuildIndexes());
        }
    }
}

using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalUnitCollisionRulesTests
    {
        static OriginalCombatCatalog Catalog() => JsonUtility.FromJson<OriginalCombatCatalog>(
            File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-combat.json")));
        static void AddCollision(OriginalCombatDefinition unit, double value, bool conflict = false)
        {
            var fields = unit.fields;
            Array.Resize(ref fields, fields.Length + 1);
            fields[fields.Length - 1] = new OriginalCombatField { key="collision",number=value,isNumber=true,conflict=conflict };
            unit.fields = fields;
        }

        [Test] public void MeasuredSparseUnitsUseExplicitlyDerivedHostProxyWithoutClaimingNativeBodyRadius()
        {
            var catalog = Catalog();
            foreach (var id in new[]{"n06C","n06I"})
            {
                var result = OriginalUnitCollisionRules.Resolve(catalog,id);
                Assert.That(result.radius,Is.EqualTo(1)); Assert.That(result.derivedHostProxy,Is.True);
                Assert.That(result.nativeCollisionKnown,Is.False);
                Assert.That(result.evidence,Does.Contain("32c0989880c4057f4acc83f826c13c8de24bc415afec78f9d40b2a2cb58df052"));
            }
        }

        [Test] public void ExplicitDeclarationWinsAndIsNotPromotedToNativeGetter()
        {
            var catalog = Catalog();
            AddCollision(catalog.Unit("n06I"),12);
            foreach (var pair in new[]{("n06I",12.0),("n008",24.0),("n06B",8.0)})
            {
                var result = OriginalUnitCollisionRules.Resolve(catalog,pair.Item1);
                Assert.That(result.radius,Is.EqualTo(pair.Item2)); Assert.That(result.derivedHostProxy,Is.False);
                Assert.That(result.nativeCollisionKnown,Is.False);
            }
        }

        [Test] public void PresentInvalidOrConflictingCollisionCannotBeRescuedByTheProxy()
        {
            foreach (double value in new[]{0.0,-1,double.NaN,double.PositiveInfinity})
            {
                var catalog = Catalog(); AddCollision(catalog.Unit("n06I"),value);
                Assert.Throws<InvalidOperationException>(()=>OriginalUnitCollisionRules.Resolve(catalog,"n06I"));
            }
            var conflicting = Catalog(); AddCollision(conflicting.Unit("n06I"),12,true);
            Assert.Throws<InvalidOperationException>(()=>OriginalUnitCollisionRules.Resolve(conflicting,"n06I"));
        }

        [Test] public void SparseProxyRequiresExactSourceAndMeasuredMovementRange()
        {
            foreach (string mutation in new[]{"source","range","movement","override"})
            {
                var catalog = Catalog(); var unit = catalog.Unit("n06I");
                if(mutation=="source") catalog.sourceSha256="different";
                if(mutation=="range") Array.Find(unit.fields,x=>x.key=="rangeN1").number=151;
                if(mutation=="movement") Array.Find(unit.fields,x=>x.key=="movetp").text="fly";
                if(mutation=="override") unit.overrides=new[]{new OriginalCombatOverride{field="ucol",isNumber=true,number=16}};
                Assert.Throws<InvalidOperationException>(()=>OriginalUnitCollisionRules.Resolve(catalog,"n06I"));
            }
        }

        [Test] public void UnmeasuredMissingCollisionRemainsUnavailable()
        {
            var catalog=Catalog(); var unit=catalog.Unit("n008");
            unit.fields=Array.FindAll(unit.fields,x=>x.key!="collision");
            Assert.Throws<InvalidOperationException>(()=>OriginalUnitCollisionRules.Resolve(catalog,"n008"));
            Assert.Throws<InvalidOperationException>(()=>OriginalUnitCollisionRules.Resolve(catalog,"xxxx"));
        }
    }
}

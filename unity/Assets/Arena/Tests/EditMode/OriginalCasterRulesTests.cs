using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalCasterRulesTests
    {
        static OriginalCombatCatalog Catalog()
        {
            var c = JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));
            c.BuildIndexes(); return c;
        }
        [Test] public void CompleteSourceDispatcherHasTwentyFourDistinctIdsAndDetachedListing()
        {
            var c = Catalog(); var ids = OriginalCasterRules.AbilityIds;
            Assert.That(ids.Length, Is.EqualTo(24));
            foreach (var id in ids) Assert.That(new OriginalCasterRules(c, id).manaCost, Is.EqualTo(150));
            ids[0] = "oops"; Assert.That(OriginalCasterRules.AbilityIds[0], Is.EqualTo("A0Z3"));
            Assert.That(OriginalCasterRules.ForUnit(c, "n05J").abilityId, Is.EqualTo("A0Z3"));
            Assert.That(OriginalCasterRules.ForUnit(c, "n008"), Is.Null);
        }
        [Test] public void FiveMeasuredSparseCastPointsAreInstantAndExplicitControlsRemainUnchanged()
        {
            var c = Catalog();
            foreach (var id in new[] { "n05J", "o00C", "n06K", "n02J", "n02O" })
            {
                Assert.That(OriginalCasterRules.TryCastPoint(c, id, out var zero), Is.True, id);
                Assert.That(zero, Is.Zero, id);
            }
            Assert.That(OriginalCasterRules.TryCastPoint(c, "n05M", out double delay), Is.True);
            Assert.That(delay, Is.EqualTo(.5));
            Assert.That(OriginalCasterRules.TryCastPoint(c, "n06J", out delay), Is.True);
            Assert.That(delay, Is.EqualTo(.3));
            var unit = c.Unit("n05J");
            unit.fields = new[] { new OriginalCombatField { key = "castpt", conflict = true } };
            Assert.That(OriginalCasterRules.TryCastPoint(c, "n05J", out _), Is.False);
        }
        [Test] public void PulseTimersKeepSixHitsAndAnAdditionalCleanupTick()
        {
            var small = new OriginalCasterRules(Catalog(), "A0Z3");
            var large = new OriginalCasterRules(Catalog(), "A121");
            Assert.That(small.warningSeconds, Is.EqualTo(2)); Assert.That(small.radius, Is.EqualTo(400));
            Assert.That(small.damage, Is.EqualTo(50)); Assert.That(small.tickCount, Is.EqualTo(6));
            Assert.That(large.warningSeconds, Is.EqualTo(1)); Assert.That(large.radius, Is.EqualTo(600));
            Assert.That(large.damage, Is.EqualTo(400)); Assert.That(large.tickCount, Is.EqualTo(6));
        }
        [Test] public void ManaBurstUsesMaxMinusScaledCurrentRatherThanScaledMissingMana()
        {
            var c = Catalog(); Assert.That(new OriginalCasterRules(c, "A0ZB").ManaBurst(100, 60), Is.EqualTo(70));
            Assert.That(new OriginalCasterRules(c, "A0ZF").ManaBurst(100, 100), Is.EqualTo(30).Within(1e-9));
        }
        [Test] public void OutsideRingIncludesExactBoundaryAndCenterIsTranslatedFiveHundred()
        {
            var r = new OriginalCasterRules(Catalog(), "A0Z6");
            var center = r.WarningCenter(new OriginalPoint(100, 0));
            Assert.That(center.x, Is.EqualTo(100).Within(1e-9)); Assert.That(center.y, Is.EqualTo(500));
            Assert.That(r.OutsideSafeRadius(new OriginalPoint(0, 0), new OriginalPoint(400, 0)), Is.True);
            Assert.That(r.OutsideSafeRadius(new OriginalPoint(0, 0), new OriginalPoint(399.99, 0)), Is.False);
        }
        [Test] public void WarningMovePreservesAuthoredSequentialXMutation()
        {
            var next = OriginalCasterRules.WarningScatter(new OriginalPoint(10, 20), Math.PI / 2, 300);
            Assert.That(next.x, Is.EqualTo(310).Within(1e-9)); Assert.That(next.y, Is.EqualTo(20));
        }
        [Test] public void RemainingFieldFamiliesHaveExplicitValidatedSourceRules()
        {
            var c = Catalog();
            foreach (var id in new[] { "A123", "A1DC", "A1DE", "A1DF" })
            { var r = new OriginalCasterRules(c, id); Assert.That(r.scriptImplemented, Is.True, id); Assert.DoesNotThrow(()=>new OriginalCasterFieldRules(c,id)); }
            Assert.Throws<InvalidOperationException>(() => new OriginalCasterRules(c, "A05N"));
        }
    }
}

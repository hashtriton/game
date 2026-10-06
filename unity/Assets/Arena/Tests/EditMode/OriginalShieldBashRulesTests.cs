using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalShieldBashRulesTests
    {
        static OriginalShieldBashCandidate Candidate(int id, double x, double y, double health = 100, bool enemy = true, bool structure = false) =>
            new OriginalShieldBashCandidate(id, new OriginalPoint(x, y), health, enemy, structure);
        static OriginalShieldBashRules Effect(bool gifts = false, int rank = 1) =>
            new OriginalShieldBashRules(1, rank, gifts, new OriginalPoint(0, 0), 0, new[] { Candidate(1001, 100, 0) });

        [Test] public void SourceConeRadiusEligibilityAndOrderArePreserved()
        {
            double a = 39.9 * Math.PI / 180, b = 40.1 * Math.PI / 180;
            var effect = new OriginalShieldBashRules(1, 1, false, new OriginalPoint(0, 0), 0, new[] {
                Candidate(1004, 100*Math.Cos(a),100*Math.Sin(a)), Candidate(1002, 275,0),
                Candidate(1003, 100*Math.Cos(b),100*Math.Sin(b)), Candidate(1005, 275.1,0),
                Candidate(1006,-100,0), Candidate(1007,100,0,.405), Candidate(1008,100,0,enemy:false), Candidate(1009,100,0,structure:true) });
            CollectionAssert.AreEqual(new[] { 1004, 1002 }, effect.Targets);
            var events = effect.BeginEvents();
            CollectionAssert.AreEqual(new[] { OriginalShieldBashEventKind.DisablePathingAndMovement, OriginalShieldBashEventKind.Damage,
                OriginalShieldBashEventKind.CrippleOrder }, events.Take(3).Select(e => e.kind));
            Assert.That(events[1].damage, Is.EqualTo(110)); Assert.That(events[1].damageMode, Is.EqualTo(OriginalTriggeredDamageMode.SpellNormal));
            Assert.That(effect.BeginEvents(), Is.Empty); Assert.That(OriginalShieldBashRules.CrippleOrderId, Is.EqualTo(0xD00DD));
            effect.Targets[0] = 9999; Assert.That(effect.Targets[0], Is.EqualTo(1004));
        }

        [Test] public void DarkGiftsUsesChaosModeAndSourcePushSpeedForExactlyThirtyThreeTicks()
        {
            foreach (bool gifts in new[] { false, true })
            {
                var effect = Effect(gifts, 3); var position = new OriginalPoint(100, 0);
                var initial = effect.BeginEvents();
                Assert.That(initial[1].damage, Is.EqualTo(290));
                Assert.That(initial[1].damageMode, Is.EqualTo(gifts ? OriginalTriggeredDamageMode.ChaosUniversal : OriginalTriggeredDamageMode.SpellNormal));
                for (int tick = 0; tick < 33; tick++)
                {
                    var events = effect.Tick(new OriginalPoint(0, 0), _ => position);
                    Assert.That(events.Length, Is.EqualTo(2));
                    Assert.That(events[0].kind, Is.EqualTo(OriginalShieldBashEventKind.ForcedPosition));
                    Assert.That(events[1].kind, Is.EqualTo(OriginalShieldBashEventKind.DestructableSweep));
                    position = events[0].position;
                }
                Assert.That(position.x, Is.EqualTo(100 + 33 * (gifts ? 6.75 : 4.5)).Within(1e-9));
                var cleanup = effect.Tick(new OriginalPoint(0, 0), _ => position);
                CollectionAssert.AreEqual(new[] { OriginalShieldBashEventKind.RestoreDefaultMovement, OriginalShieldBashEventKind.Completed }, cleanup.Select(e => e.kind));
                Assert.That(effect.Completed, Is.True); Assert.That(effect.Tick(new OriginalPoint(0, 0), _ => position), Is.Empty);
            }
        }

        [Test] public void CoincidentPositionKeepsTheSourcesSubtractionDirection()
        {
            var candidates = new[] { Candidate(1001, 100, 100) };
            var forward = new OriginalShieldBashRules(1, 1, false, new OriginalPoint(100, 100), 0, candidates);
            var backward = new OriginalShieldBashRules(1, 1, false, new OriginalPoint(100, 100), 180, candidates);
            Assert.That(forward.Targets, Is.Empty);
            Assert.That(backward.Targets, Is.EqualTo(new[] { 1001 }));
        }

        [Test] public void BlockedPointRestoresMovementButDoesNotRemoveRetainedTarget()
        {
            var effect = Effect(); effect.BeginEvents();
            var blocked = effect.Tick(new OriginalPoint(1790, 0), _ => new OriginalPoint(1791, 0));
            Assert.That(blocked.Single().kind, Is.EqualTo(OriginalShieldBashEventKind.RestoreDefaultMovement));
            var retry = effect.Tick(new OriginalPoint(1800, 0), _ => new OriginalPoint(1791, 0));
            Assert.That(retry[0].kind, Is.EqualTo(OriginalShieldBashEventKind.ForcedPosition));
            Assert.That(retry[0].position.x, Is.EqualTo(1786.5).Within(1e-9));
            Assert.That(effect.Targets, Is.EqualTo(new[] { 1001 }));
        }

        [Test] public void ForcedDirectionUsesCurrentCasterAndTargetAndRemovedTargetsAreSkipped()
        {
            var effect = Effect(); effect.BeginEvents();
            var moved = effect.Tick(new OriginalPoint(100, -100), _ => new OriginalPoint(100, 0));
            Assert.That(moved[0].position.x, Is.EqualTo(100).Within(1e-9));
            Assert.That(moved[0].position.y, Is.EqualTo(4.5).Within(1e-9));
            Assert.That(effect.Tick(new OriginalPoint(0, 0), _ => null), Is.Empty);
        }

        [Test] public void TerrainRectangleAndBossCircleBoundariesAreInclusiveAndIndependentOfWpm()
        {
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(-1920, 1400)), Is.False);
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(-1920.01, 1400)), Is.True);
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(819.99, -2700)), Is.True);
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(820, -2700)), Is.False);
            // Far outside those scripted exclusions eL itself returns true;
            // iL/nL subsequently clamp coordinates to playable bounds +/-25.
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(4000, 4000)), Is.True);
            var clamped = OriginalShieldBashRules.ClampPlayable(new OriginalPoint(4000, -5000));
            Assert.That(clamped.x, Is.EqualTo(2407)); Assert.That(clamped.y, Is.EqualTo(-4071));
        }

        [Test] public void SweepRetainsTheOriginalRectangleVersusCirclePrecedence()
        {
            var origin = new OriginalPoint(0, 0); var corner = new OriginalPoint(64, 64);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTba", origin, corner), Is.True);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTbs", origin, corner), Is.True);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTbr", origin, corner), Is.False);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTbr", origin, new OriginalPoint(64, 0)), Is.True);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTex", origin, origin), Is.False);
            Assert.That(OriginalShieldBashRules.SweepDestroys("LTba", origin, new OriginalPoint(64.01, 0)), Is.False);
        }

        [Test] public void InvalidInputsCannotConsumeAPartialTimerStep()
        {
            Assert.Throws<ArgumentException>(() => new OriginalShieldBashRules(1, 1, false, new OriginalPoint(0, 0), 0,
                new[] { Candidate(1001,100,0), Candidate(1001,200,0) }));
            var effect = Effect();
            Assert.Throws<InvalidOperationException>(() => effect.Tick(new OriginalPoint(0, 0), _ => new OriginalPoint(100,0)));
            effect.BeginEvents();
            Assert.Throws<ArgumentOutOfRangeException>(() => effect.Tick(new OriginalPoint(0, 0), _ => new OriginalPoint(double.NaN, 0)));
            for (int i = 0; i < 33; i++) effect.Tick(new OriginalPoint(0, 0), _ => new OriginalPoint(100, 0));
            Assert.That(effect.Completed, Is.False); effect.Tick(new OriginalPoint(0, 0), _ => null); Assert.That(effect.Completed, Is.True);
        }
    }
}

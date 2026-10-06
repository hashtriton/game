using System;
using System.Collections.Generic;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalPyroEffectRulesTests
    {
        [Test] public void VacuumPullMovesBeforeTheSixteenthCallbackDealsDamage()
        {
            var pull = new OriginalPyroVacuumPull(new OriginalPoint(0, 0), new OriginalPoint(150, 0));
            var point = new OriginalPoint(150, 0);
            for (int i = 0; i < 15; i++) { Assert.That(pull.Tick(point, false, out point, out var damage), Is.True); Assert.That(damage, Is.False); }
            Assert.That(point.x, Is.Zero); Assert.That(pull.Completed, Is.False);
            Assert.That(pull.Tick(point, false, out point, out var finalDamage), Is.True);
            Assert.That(point.x, Is.EqualTo(-10)); Assert.That(finalDamage, Is.True); Assert.That(pull.Completed, Is.True);
            Assert.That(pull.Tick(point, false, out _, out finalDamage), Is.False); Assert.That(finalDamage, Is.False);
        }
        [Test] public void VacuumPullUsesLivePositionButCapturedVectorAndCancelsOnDeath()
        {
            var pull = new OriginalPyroVacuumPull(new OriginalPoint(0, 0), new OriginalPoint(150, 0));
            pull.Tick(new OriginalPoint(300, 20), false, out var point, out _);
            Assert.That(point.x, Is.EqualTo(290)); Assert.That(point.y, Is.EqualTo(20));
            Assert.That(pull.Tick(point, true, out _, out var damage), Is.False); Assert.That(damage, Is.False); Assert.That(pull.Completed, Is.True);
        }
        [Test] public void SpherePushUsesRetainedCursorAndMaxFloorForExactlyTenMoves()
        {
            var push = new OriginalPyroSpherePush(new OriginalPoint(0, 1000), new OriginalPoint(100, 1000));
            OriginalPoint point = default;
            for (int i = 0; i < 10; i++) Assert.That(push.Tick(false, out point), Is.True);
            Assert.That(point.x, Is.EqualTo(275)); Assert.That(push.Completed, Is.False);
            Assert.That(push.Tick(false, out _), Is.False); Assert.That(push.Completed, Is.True);
            var far = new OriginalPyroSpherePush(new OriginalPoint(0, 1000), new OriginalPoint(300, 1000));
            far.Tick(false, out point); Assert.That(point.x, Is.EqualTo(301));
        }
        [Test] public void PortalHasInclusive160And175BoundsAndDoesNotInventCenterMovement()
        {
            foreach (double distance in new[] { 160.0, 175.0 })
            { Assert.That(OriginalPyroEffectRules.PortalPull(new OriginalPoint(0, 0), new OriginalPoint(distance, 0), out var point), Is.True); Assert.That(point.x, Is.EqualTo(distance - 15)); }
            foreach (double distance in new[] { 0.0, 159.999, 175.001 })
                Assert.That(OriginalPyroEffectRules.PortalPull(new OriginalPoint(0, 0), new OriginalPoint(distance, 0), out _), Is.False);
        }
        [Test] public void MeteorTrailOrdersAtSixSourcePointsBeforeMovingAndFinishesOn101()
        {
            var trail = new OriginalPyroMeteorTrail(new OriginalPoint(0, 0), 0); var casts = new List<double>();
            for (int i = 0; i < 100; i++) if (trail.Tick(out var point)) casts.Add(point.x);
            Assert.That(casts, Is.EqualTo(new[] { 15.0, 205, 405, 605, 805, 1005 }));
            Assert.That(trail.Position.x, Is.EqualTo(1015)); Assert.That(trail.Completed, Is.False);
            Assert.That(trail.Tick(out _), Is.False); Assert.That(trail.Completed, Is.True);
        }
        [Test] public void NonfiniteObservationDoesNotAdvancePull()
        {
            var pull = new OriginalPyroVacuumPull(new OriginalPoint(0, 0), new OriginalPoint(150, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => pull.Tick(new OriginalPoint(double.NaN, 0), false, out _, out _));
            pull.Tick(new OriginalPoint(150, 0), false, out var point, out _); Assert.That(point.x, Is.EqualTo(140));
        }
    }
}

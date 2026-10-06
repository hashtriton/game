using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalCasterSleepWaveRulesTests
    {
        [Test] public void SourceStartsAtCurrentCasterAndSweepsAfterMovingTowardWarning()
        {
            var wave = new OriginalCasterSleepWaveRules(new OriginalPoint(700, 1000), new OriginalPoint(135, 1000));
            Assert.That(wave.Center.x, Is.EqualTo(700));
            wave.Advance();
            Assert.That(wave.Center.x, Is.EqualTo(673).Within(1e-8));
            Assert.That(wave.Center.y, Is.EqualTo(1000).Within(1e-8));
            Assert.That(OriginalCasterSleepWaveRules.Radius, Is.EqualTo(450));
            Assert.That(OriginalCasterSleepWaveRules.Damage, Is.EqualTo(1500));
        }

        [Test] public void OldRemainingDistanceKeepsThirtySeventhSweepPastAuthoredDistance()
        {
            var wave = new OriginalCasterSleepWaveRules(new OriginalPoint(0, 0), new OriginalPoint(100, 0));
            for (int i = 0; i < 36; i++) wave.Advance();
            Assert.That(wave.Completed, Is.False);
            Assert.That(wave.RemainingDistance, Is.EqualTo(-22));
            wave.Advance();
            Assert.That(wave.Completed, Is.True);
            Assert.That(wave.Ticks, Is.EqualTo(37));
            Assert.That(wave.Center.x, Is.EqualTo(999));
            Assert.Throws<InvalidOperationException>(() => wave.Advance());
        }

        [Test] public void DirectionIsCapturedAtActivationAndInvalidPointsAreRejected()
        {
            var origin = new OriginalPoint(10, 20); var target = new OriginalPoint(10, 50);
            var wave = new OriginalCasterSleepWaveRules(origin, target);
            origin = new OriginalPoint(-300, -300); target = new OriginalPoint(-400, -400);
            wave.Advance(); wave.Advance();
            Assert.That(wave.Center.x, Is.EqualTo(10).Within(1e-8));
            Assert.That(wave.Center.y, Is.EqualTo(74).Within(1e-8));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OriginalCasterSleepWaveRules(new OriginalPoint(double.NaN, 0), target));
        }
    }
}

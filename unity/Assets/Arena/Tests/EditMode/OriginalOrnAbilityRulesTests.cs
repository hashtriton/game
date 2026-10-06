using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalOrnAbilityRulesTests
    {
        [Test]
        public void KickUsesFixedBearingTwentyNineMovesAndCleanupCallback()
        {
            var rule = new OriginalOrnKickRules(new OriginalPoint(-100, -2700), new OriginalPoint(0, -2700));
            var point = new OriginalPoint(0, -2700);
            for (int i = 0; i < 29; i++)
            {
                Assert.That(rule.Tick(point, true, out var next), Is.True);
                point = next;
            }
            Assert.That(point.x, Is.EqualTo(580).Within(.01));
            Assert.That(rule.Completed, Is.False);
            Assert.That(rule.Tick(point, true, out _), Is.False);
            Assert.That(rule.Completed, Is.True);
        }

        [Test]
        public void DistantKickReversesAndInvalidPointEndsWithoutTeleport()
        {
            var rule = new OriginalOrnKickRules(new OriginalPoint(-100, -2000), new OriginalPoint(0, -2000));
            Assert.That(rule.Tick(new OriginalPoint(0, -2000), true, out var point), Is.True);
            Assert.That(point.x, Is.EqualTo(-20).Within(.01));
            Assert.That(rule.Tick(point, false, out var same), Is.False);
            Assert.That(same.x, Is.EqualTo(point.x));
        }

        [Test]
        public void SpinUsesSourceRealCountersAndSeparateChaseLifetime()
        {
            var rule = new OriginalOrnSpinRules();
            int warning = 0;
            while (!rule.TickWarning()) warning++;
            Assert.That(warning, Is.EqualTo(38));
            int damage = 0;
            while (rule.TickDamage(true)) damage++;
            Assert.That(damage, Is.EqualTo(10));
            int chase = 0;
            var point = new OriginalPoint(0, 0);
            while (!rule.ChaseCompleted)
            {
                point = rule.TickChase(point, new OriginalPoint(10000, 0)); chase++;
            }
            Assert.That(chase, Is.EqualTo(101));
            Assert.That(point.x, Is.EqualTo(1131.2).Within(.1));
        }

        [Test]
        public void SpinStopsDamageOnDeathButChaseRetainsTargetAndMinimumDistance()
        {
            var rule = new OriginalOrnSpinRules();
            Assert.That(rule.TickDamage(false), Is.False);
            Assert.That(rule.DamageCompleted, Is.True);
            var point = rule.TickChase(new OriginalPoint(0, 0), new OriginalPoint(99, 0));
            Assert.That(point.x, Is.Zero);
            point = rule.TickChase(point, new OriginalPoint(100, 0));
            Assert.That(point.x, Is.EqualTo(11.2).Within(.001));
            Assert.That(OriginalOrnAbilityRules.DecoyDamage(1000, false), Is.EqualTo(300));
            Assert.That(OriginalOrnAbilityRules.DecoyDamage(1000, true), Is.EqualTo(600));
        }
    }
}

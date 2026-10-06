using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalBossChargeTests
    {
        [Test] public void ChargeHasNineTelegraphTicksAndTwentyEightMovementSweeps()
        {
            var p = new OriginalPoint(0, -2700);
            var charge = new OriginalBossChargeRules(new OriginalPoint(1000, -2700), 90);
            for (int i = 0; i < 8; i++) { charge.TickTelegraph(p, true); Assert.That(charge.Charging, Is.False); }
            charge.TickTelegraph(p, true); Assert.That(charge.Charging, Is.True);
            Assert.That(charge.Facing, Is.Zero);
            for (int i = 0; i < 28; i++) Assert.That(charge.TickCharge(p, 0, true, false, out p), Is.True);
            Assert.That(p.x, Is.EqualTo(588));
            Assert.That(charge.TickCharge(p, 0, true, false, out p), Is.False);
            Assert.That(charge.Completed, Is.True);
        }
        [Test] public void ExcludedTerrainKeepsSweepWhileDeathStopsAndSpecialPhaseAborts()
        {
            var p = new OriginalPoint(400, 2680);
            var charge = new OriginalBossChargeRules(p, 90);
            for (int i = 0; i < 9; i++) charge.TickTelegraph(p, true);
            Assert.That(charge.TickCharge(p, 90, true, false, out var next), Is.True);
            Assert.That(next.y, Is.EqualTo(p.y));
            Assert.That(charge.TickCharge(p, 0, true, true, out _), Is.False);
            var dead = new OriginalBossChargeRules(p, 0); dead.TickTelegraph(p, false);
            Assert.That(dead.TickCharge(p, 0, false, false, out _), Is.False);
        }
    }
}

using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalBossGhostTests
    {
        [Test] public void FinalGhostSweepUsesOldPositionAndStillMovesOnTheRemovalCallback()
        {
            var ghost = new OriginalBossGhostRules(new OriginalPoint(-800, -2680), 0);
            for (int i = 0; i < 73; i++)
            {
                Assert.That(ghost.Tick(out var sweep), Is.True);
                Assert.That(sweep.x, Is.EqualTo(-800 + 22 * i).Within(1e-9));
            }
            Assert.That(ghost.Completed, Is.True);
            Assert.That(ghost.Position.x, Is.EqualTo(806));
            Assert.That(ghost.Tick(out _), Is.False);
        }
        [Test] public void SixGhostsAlternateSidesLeavingTheOriginalGaps()
        {
            var first = OriginalBossGhostRules.Formation(0, true);
            var second = OriginalBossGhostRules.Formation(0, false);
            Assert.That(first.Length, Is.EqualTo(6));
            double[] firstY = { -2585, -2325, -2065, -2905, -3165, -3425 };
            double[] secondY = { -2455, -2195, -1935, -2775, -3035, -3295 };
            for (int i = 0; i < 6; i++)
            {
                Assert.That(first[i].x, Is.EqualTo(-800).Within(.01));
                Assert.That(first[i].y, Is.EqualTo(firstY[i]).Within(.01));
                Assert.That(second[i].y, Is.EqualTo(secondY[i]).Within(.01));
            }
            Assert.That(OriginalBossGhostRules.Formation(90, true)[0].y, Is.EqualTo(-3480).Within(.01));
        }
        [Test] public void OnlyTheNextRemainingThresholdCanStartAnotherPhase()
        {
            Assert.That(OriginalBossGhostRules.ThresholdReached(2, .65), Is.True);
            Assert.That(OriginalBossGhostRules.ThresholdReached(2, .651), Is.False);
            Assert.That(OriginalBossGhostRules.ThresholdReached(1, .35), Is.True);
            Assert.That(OriginalBossGhostRules.ThresholdReached(1, .351), Is.False);
            Assert.That(OriginalBossGhostRules.ThresholdReached(0, .01), Is.False);
        }
    }
}

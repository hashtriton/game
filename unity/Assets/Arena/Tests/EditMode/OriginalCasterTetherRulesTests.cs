using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalCasterTetherRulesTests
    {
        [Test] public void PullRecomputesBearingFromVictimOriginWhenCasterMoves()
        {
            var rule=new OriginalCasterTetherRules(false,new OriginalPoint(0,0),default);
            var first=rule.Advance(new OriginalPoint(100,0));
            Assert.That(first.x,Is.EqualTo(8)); Assert.That(first.y,Is.Zero);
            var second=rule.Advance(new OriginalPoint(0,100));
            Assert.That(second.x,Is.EqualTo(0).Within(1e-9)); Assert.That(second.y,Is.EqualTo(16));
            Assert.That(rule.TargetDistance,Is.EqualTo(100)); Assert.That(rule.ShouldFinish(1,1),Is.False);
        }
        [Test] public void PullMovesPastNearCasterBeforeItsCompletionTest()
        {
            var rule=new OriginalCasterTetherRules(false,new OriginalPoint(10,20),default);
            var last=rule.Advance(new OriginalPoint(15,20));
            Assert.That(last.x,Is.EqualTo(18)); Assert.That(last.y,Is.EqualTo(20));
            Assert.That(rule.ShouldFinish(1,1),Is.True); Assert.That(rule.DamagePerTick,Is.Zero);
        }
        [Test] public void DragUsesItsFixedDestinationAndStillDealsEightOnTheFinalStep()
        {
            var rule=new OriginalCasterTetherRules(true,new OriginalPoint(0,0),new OriginalPoint(24,0));
            var first=rule.Advance(new OriginalPoint(800,700));
            Assert.That(first.x,Is.EqualTo(16)); Assert.That(rule.ShouldFinish(1,1),Is.False);
            var last=rule.Advance(new OriginalPoint(-1000,1000));
            Assert.That(last.x,Is.EqualTo(32)); Assert.That(last.y,Is.Zero);
            Assert.That(rule.DamagePerTick,Is.EqualTo(8)); Assert.That(rule.ShouldFinish(1,1),Is.True);
        }
        [Test] public void NativeDeadThresholdIsCheckedAfterMovementAndDamage()
        {
            var rule=new OriginalCasterTetherRules(true,new OriginalPoint(0,0),new OriginalPoint(100,0));
            Assert.That(rule.Advance(default).x,Is.EqualTo(16));
            Assert.That(rule.ShouldFinish(.40501,.40501),Is.False);
            Assert.That(rule.ShouldFinish(.405,100),Is.True); Assert.That(rule.ShouldFinish(100,0),Is.True);
        }
        [Test] public void DragPreservesTheAuthoredInitialYAssignmentAndGlobalRandomRectangle()
        {
            var initial=OriginalCasterTetherRules.DragInitialCasterPosition(new OriginalPoint(125,900));
            Assert.That(initial.x,Is.EqualTo(125)); Assert.That(initial.y,Is.EqualTo(125));
            var lower=OriginalCasterTetherRules.DragDestination(0,0);
            var center=OriginalCasterTetherRules.DragDestination(.5,.5);
            Assert.That(lower.x,Is.EqualTo(-1400)); Assert.That(lower.y,Is.EqualTo(-500));
            Assert.That(center.x,Is.EqualTo(100)); Assert.That(center.y,Is.EqualTo(1000));
        }
        [Test] public void InvalidNumbersCannotContaminateTheTimerState()
        {
            var rule=new OriginalCasterTetherRules(false,default,default);
            Assert.Throws<ArgumentOutOfRangeException>(()=>rule.Advance(new OriginalPoint(double.NaN,0)));
            Assert.That(rule.Travelled,Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(()=>OriginalCasterTetherRules.DragDestination(1,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>rule.ShouldFinish(double.PositiveInfinity,1));
        }
    }
}

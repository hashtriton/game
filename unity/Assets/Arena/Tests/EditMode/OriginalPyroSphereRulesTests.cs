using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalPyroSphereRulesTests
    {
        [Test] public void OrbitHasLiteralInitialNegativeAnglesAndFollowsCurrentCasterWithPositiveTickAngles()
        {
            var orbit = new OriginalPyroSphereOrbit(new OriginalPoint(100, 1000));
            var initial = orbit.Positions;
            Assert.That(initial[1].y, Is.EqualTo(1000 + 250 * Math.Sin(-288 * .0174532)).Within(1e-8));
            orbit.Tick(new OriginalPoint(200, 1200), false);
            Assert.That(orbit.Positions[1].y, Is.EqualTo(1200 + 250 * Math.Sin((288 - 1.44) * .0174532)).Within(1e-8));
            initial[0] = default;
            Assert.That(orbit.Positions[0].x, Is.GreaterThan(400));
        }
        [Test] public void LaunchesCreationOrderAndOnlyFifthLaunchReenablesTheMainSpell()
        {
            var orbit = new OriginalPyroSphereOrbit(default);
            var positions = orbit.Positions;
            for (int i = 0; i < 5; i++)
            {
                Assert.That(orbit.TryLaunch(out var point), Is.True);
                Assert.That(point.x, Is.EqualTo(positions[i].x)); Assert.That(point.y, Is.EqualTo(positions[i].y));
                Assert.That(orbit.Remaining, Is.EqualTo(4 - i)); Assert.That(orbit.Completed, Is.EqualTo(i == 4));
                Assert.That(orbit.UnlaunchedPositions.Length, Is.EqualTo(4 - i));
            }
            Assert.That(orbit.TryLaunch(out _), Is.False);
        }
        [Test] public void ExpiryOccursOn751AfterFifteenSecondsAndDeathRetiresOnlyUnlaunchedHelpers()
        {
            var orbit = new OriginalPyroSphereOrbit(default);
            for (int i = 0; i < 750; i++) orbit.Tick(default, false);
            Assert.That(orbit.Completed, Is.False);
            orbit.Tick(default, false); Assert.That(orbit.Completed, Is.True);
            var other = new OriginalPyroSphereOrbit(default); other.TryLaunch(out var start);
            var flying = new OriginalPyroFlyingSphere(start, new OriginalPoint(1000, 0), false);
            other.Tick(default, true); Assert.That(other.Remaining, Is.Zero);
            flying.Step(); Assert.That(flying.Completed, Is.False);
        }
        [Test] public void FlyingSphereAdvancesBeforeStrictThirtySixArrivalAndSpellEventsAddAFullStep()
        {
            var sphere = new OriginalPyroFlyingSphere(default, new OriginalPoint(90, 0), false);
            Assert.That(sphere.Step(), Is.False); Assert.That(sphere.Position.x, Is.EqualTo(18));
            Assert.That(sphere.Step(), Is.False); Assert.That(sphere.Position.x, Is.EqualTo(36));
            Assert.That(sphere.Step(), Is.False); // Exactly36 is not less than36.
            Assert.That(sphere.Step(), Is.True); Assert.That(sphere.Position.x, Is.EqualTo(72));
        }
        [Test] public void TargetDeathCapturesCurrentPointAndSubsequentCallbacksDoNotFollowIt()
        {
            var sphere = new OriginalPyroFlyingSphere(default, new OriginalPoint(1000, 0), true);
            sphere.Step(new OriginalPoint(1000, 0), true); Assert.That(sphere.HasLiveTarget, Is.False);
            sphere.Step(new OriginalPoint(-1000, 0)); Assert.That(sphere.Position.x, Is.EqualTo(36));
            Assert.Throws<ArgumentOutOfRangeException>(() => sphere.Step(new OriginalPoint(double.NaN, 0)));
            Assert.That(sphere.Position.x, Is.EqualTo(36));
        }
    }
}

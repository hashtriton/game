using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionArcherVisibilityTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession session, string method, params object[] args) =>
            typeof(OriginalSession).GetMethod(method, Hidden).Invoke(session, args);
        static object Create(string skill, out OriginalSession session, out OriginalWorld world)
        {
            var fixture = typeof(OriginalSessionArcherTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { skill });
            session = (OriginalSession)fixture.GetType().GetField("session", Hidden).GetValue(fixture);
            world = (OriginalWorld)fixture.GetType().GetField("world", Hidden).GetValue(fixture);
            world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 24 }, new OriginalPoint(100, -1000));
            Call(session, "ApplyNativeItemStatus", 1800, Call(session, "NativeItemAction", "I06M"));
            Call(session, "AdvanceItemStatuses", 2.01d);
            world.SetUnitState(1800, paused: true);
            Assert.That(Call(session, "CombatInvisibilityActive", 1800), Is.True);
            return fixture;
        }
        static void Cast(object fixture, string skill) => Assert.That(fixture.GetType().GetMethod("Send", Hidden)
            .Invoke(fixture, new object[] { OriginalSessionCommandKind.CastSkill, skill, 700d, -1000d, 0 }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
        static void Tick(OriginalSession session, OriginalWorld world, double seconds)
        {
            while (seconds > 1e-9)
            {
                double step = Math.Min(.01, seconds); world.Advance(step);
                Call(session, "AdvanceArcherAbilities"); seconds -= step;
            }
        }
        [TestCase(false)]
        [TestCase(true)]
        public void VolleyAcquiresInvisibleEnemyOnlyWhenAnAlliedDetectorSeesIt(bool detected)
        {
            var fixture = Create("A0AS", out var session, out var world);
            if (detected)
                Assert.That(world.TryPublishSummons(new[] { new OriginalWorldSummonSpawn {
                    entityId = 100000091, ownerSlot = 1, sourceHeroEntityId = 1, rawcode = "n0AD",
                    profile = new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 16 },
                    position = new OriginalPoint(200, -1000), health = 1000 } }), Is.True);
            Assert.That(Call(session, "CanSeeForCombat", 1, world.UnitState(1800)), Is.EqualTo(detected));
            Cast(fixture, "A0AS"); Tick(session, world, .3);
            if (detected) Assert.That(world.UnitState(1800).health, Is.LessThan(1000));
            else Assert.That(world.UnitState(1800).health, Is.EqualTo(1000));
        }
        [Test]
        public void PointPowerShotStillDamagesAnUndetectedInvisibleEnemy()
        {
            var fixture = Create("A15W", out var session, out var world);
            Assert.That(Call(session, "CanSeeForCombat", 1, world.UnitState(1800)), Is.False);
            Cast(fixture, "A15W"); Tick(session, world, 1.3);
            Assert.That(world.UnitState(1800).health, Is.LessThan(1000));
            Assert.That(Call(session, "CombatInvisibilityActive", 1800), Is.True);
        }
    }
}

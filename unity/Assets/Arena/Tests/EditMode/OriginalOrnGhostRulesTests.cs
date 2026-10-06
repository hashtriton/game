using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
namespace Arena.Tests
{
    public sealed class OriginalOrnGhostRulesTests
    {
        static object Call(object target, string method, params object[] arguments) => target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { "H008", null };
            var s = (OriginalSession)typeof(OriginalSessionOrnGhostTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[]{ null });
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            return s;
        }
        static void Advance(OriginalSession s, OriginalWorld w, double seconds)
        {
            while (seconds > 1e-9) { double step = Math.Min(.05, seconds); w.Advance(step); Call(s, "AdvanceOrnGhosts"); seconds -= step; }
        }
        static void Enable(OriginalSession s) => Call(s, "OnOrnMatchEvent", new OriginalMatchEvent { kind = OriginalMatchEventKind.BossPause, sourceRule = "final-phases" });
        static void Disable(OriginalSession s) => Call(s, "OnOrnMatchEvent", new OriginalMatchEvent { kind = OriginalMatchEventKind.BossResume, sourceRule = "final-phases" });
        [Test] public void PortalGeometryUsesTwelveSourceAnglesAndInwardFacing()
        {
            for (int i = 1; i <= 12; i++)
            {
                var p = OriginalOrnGhostRules.Portal(i);
                Assert.That(Math.Sqrt(p.x*p.x+(p.y+2700)*(p.y+2700)), Is.EqualTo(800).Within(1e-9));
                Assert.That(OriginalOrnGhostRules.PortalHeading(i), Is.EqualTo((i*30+180)%360));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalOrnGhostRules.Portal(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalOrnGhostRules.Portal(13));
        }
        [Test] public void GhostMovesBeforeSweepAndRemovesBeforeSixtySeventhSweep()
        {
            var rule = new OriginalOrnGhostRules(new OriginalPoint(-800,-2700), 0);
            for (int i=1;i<=66;i++)
            {
                Assert.That(rule.Tick(out var sweep), Is.True);
                Assert.That(sweep.x, Is.EqualTo(-800+i*24).Within(1e-9));
                Assert.That(rule.HealthFraction, Is.EqualTo(i*.015).Within(1e-12));
            }
            var last=rule.Position;
            Assert.That(rule.Tick(out _), Is.False); Assert.That(rule.Completed, Is.True);
            Assert.That(rule.Position.x, Is.EqualTo(last.x)); Assert.That(rule.Tick(out _), Is.False);
        }
        [Test] public void BlockedForcedMovementStillSweepsAndAgesUntilRemoval()
        {
            var rule = new OriginalOrnGhostRules(new OriginalPoint(800,-2700),0);
            Assert.That(OriginalShieldBashRules.AllowsForcedPoint(new OriginalPoint(824,-2700)), Is.False);
            for(int i=1;i<=66;i++)
            { Assert.That(rule.Tick(out var p),Is.True); Assert.That(p.x,Is.EqualTo(800)); Assert.That(rule.HealthFraction,Is.EqualTo(i*.015)); }
            Assert.That(rule.Tick(out _),Is.False);
        }
        [Test] public void GlobalFifteenSecondPhaseSurvivesDisabledTimeAndPendingWarningSurvivesResume()
        {
            var s=Create(out var w); Advance(s,w,14.9); Enable(s);
            Assert.That(s.Snapshot().effects.Count(e=>e.abilityId=="n062"),Is.EqualTo(12));
            Advance(s,w,.1);
            Assert.That(s.Snapshot().effects.Count(e=>e.abilityId=="h04R"),Is.EqualTo(1));
            Disable(s); Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="n062"),Is.False);
            Advance(s,w,1.29); Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="h016"),Is.False);
            Advance(s,w,.01); Assert.That(s.Snapshot().effects.Count(e=>e.abilityId=="h016"),Is.EqualTo(1));
            Advance(s,w,2.01); Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="h016"),Is.False);
            Advance(s,w,11.68); Enable(s); // world29.99, nextglobal30 rather than44.99.
            Advance(s,w,.01); Assert.That(s.Snapshot().effects.Count(e=>e.abilityId=="h04R"),Is.EqualTo(1));
        }
        [Test] public void GhostHitsOnlyOnceAtNewPositionUsingGrowingMaxHealthFractionAndDummyAttribution()
        {
            var s=Create(out var w); Enable(s); Advance(s,w,16.3);
            var ghost=s.Snapshot().effects.Single(e=>e.abilityId=="h016");
            double dx=(ghost.end.x-ghost.position.x)/90,dy=(ghost.end.y-ghost.position.y)/90;
            //164 beyond birth is outside the old radius, inside the moved radius.
            w.ForcePosition(1,new OriginalPoint(ghost.position.x+dx*164,ghost.position.y+dy*164));
            w.SetUnitState(1,invulnerable:false);
            double health=w.UnitState(1).health,maximum=w.UnitState(1).profile.maxHealth;
            Advance(s,w,.03);
            Assert.That(w.UnitState(1).health,Is.EqualTo(health-maximum*.015*.8).Within(1e-6));
            health=w.UnitState(1).health;
            var moved=s.Snapshot().effects.Single(e=>e.abilityId=="h016"); w.ForcePosition(1,moved.position);
            Advance(s,w,.03); Assert.That(w.UnitState(1).health,Is.EqualTo(health).Within(1e-9));
        }
        [Test] public void EnteringOnALaterTickUsesCurrentFractionAndVisualSnapshotsAreDetached()
        {
            var s=Create(out var w); Enable(s); Advance(s,w,16.6);
            var ghost=s.Snapshot().effects.Single(e=>e.abilityId=="h016");
            var p=ghost.position; ghost.position=new OriginalPoint(10000,10000);
            Assert.That(s.Snapshot().effects.Single(e=>e.abilityId=="h016").position.x,Is.EqualTo(p.x));
            w.ForcePosition(1,p); w.SetUnitState(1,invulnerable:false);
            double health=w.UnitState(1).health,maximum=w.UnitState(1).profile.maxHealth;
            Advance(s,w,.03); Assert.That(w.UnitState(1).health,Is.EqualTo(health-maximum*.165*.8).Within(1e-6));
        }
        [Test] public void AllThreeFastKilledFinalSeriesResumeAndPermitFinalVictory()
        {
            var s=Create(out var w);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(s);
            for(int i=0;i<101;i++)s.Advance(.05);
            int boss=OriginalWorld.EnemyEntityId(match.FinalBossEntityId);
            double[] thresholds={.74,.54,.34}; int[] expected={16,2,2};
            for(int stage=0;stage<3;stage++)
            {
                var actor=w.UnitState(boss);
                w.ApplyUnitDamage(boss,actor.health-actor.profile.maxHealth*thresholds[stage]);
                int killed=0;
                for(int i=0;i<1500&&match.FinalStage==stage;i++)
                {
                    s.Advance(.05); Assert.That(s.HaltReason,Is.Null);
                    foreach(var add in s.Snapshot().enemies.Where(e=>e.finalAdd))
                    {w.ForceUnitDeath(OriginalWorld.EnemyEntityId(add.entityId));Assert.That(s.ReportEnemyKilled(add.entityId,false),Is.True);killed++;}
                }
                Assert.That(killed,Is.EqualTo(expected[stage])); Assert.That(match.FinalStage,Is.EqualTo(stage+1));
                Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Combat));
            }
            w.ForceUnitDeath(boss); Assert.That(s.ReportEnemyKilled(match.FinalBossEntityId,false),Is.True);
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Won));
        }
    }
}


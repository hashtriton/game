using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionOrnGhostTests
    {
        static object Call(object target, string method, params object[] arguments) => target.GetType()
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        static OriginalMatch Match(OriginalSession s) => (OriginalMatch)typeof(OriginalSession)
            .GetField("match", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { "H008", null };
            var session = (OriginalSession)typeof(OriginalSessionRegenerationTests)
                .GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            world = (OriginalWorld)args[1]; session.DrainEvents();
            typeof(OriginalMatch).GetProperty("Round").SetValue(Match(session), 30);
            Call(Match(session), "StartCombat"); Call(session, "CollectEvents"); session.DrainEvents();
            world.SetUnitState(1, paused: true, invulnerable: true);
            return session;
        }

        [Test] public void FastKilledFinalSeriesWaitsForWarningDeathThenResumesBeforeGhostBirth()
        {
            var session = Create(out var world);
            for (int i = 0; i < 101; i++) session.Advance(.05);
            int boss = OriginalWorld.EnemyEntityId(Match(session).FinalBossEntityId);
            world.ApplyUnitDamage(boss, world.UnitState(boss).profile.maxHealth * .26);
            bool entered = false, resumed = false; int killed = 0;
            for (int i = 0; i < 1500; i++)
            {
                session.Advance(.05);
                Assert.That(session.HaltReason, Is.Null, "At world time " + world.Clock);
                entered |= session.Snapshot().phase == OriginalMatchPhase.FinalIntermission;
                foreach (var add in session.Snapshot().enemies.Where(e => e.finalAdd))
                {
                    world.ForceUnitDeath(OriginalWorld.EnemyEntityId(add.entityId));
                    Assert.That(session.ReportEnemyKilled(add.entityId, false), Is.True); killed++;
                }
                if (entered && session.Snapshot().phase == OriginalMatchPhase.Combat)
                { resumed = true; break; }
            }
            Assert.That(killed, Is.EqualTo(16)); Assert.That(resumed, Is.True);
            Assert.That(Match(session).FinalStage, Is.EqualTo(1));
            Assert.That(world.UnitState(boss).hidden || world.UnitState(boss).paused, Is.False);
            var effects = session.Snapshot().effects;
            Assert.That(effects.Any(e => e.abilityId == "n062"), Is.False, "Portals were removed synchronously by the death callback.");
            Assert.That(effects.Count(e => e.abilityId == "h016"), Is.EqualTo(1), "The warning callback continues after resume and still creates its ghost.");
        }

        [Test] public void CasterWarningDeathResumesCompletedSeriesBeforeItsEffectActivates()
        {
            var session = Create(out var world);
            for (int i = 0; i < 101; i++) session.Advance(.05);
            int boss = OriginalWorld.EnemyEntityId(Match(session).FinalBossEntityId);
            world.ApplyUnitDamage(boss, world.UnitState(boss).profile.maxHealth * .26);
            world.AddUnit(9001, 0, "n05M", new OriginalWorldUnitProfile { maxHealth = 5000, maxMana = 500,
                collisionRadius = 24 }, new OriginalPoint(-2500, -2500));
            world.SetUnitState(9001, paused: true);
            var catalog = (OriginalCombatCatalog)typeof(OriginalSession)
                .GetField("combatCatalog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            bool startedWarning = false, resumed = false; int killed = 0;
            for (int i = 0; i < 1300; i++)
            {
                session.Advance(.05);
                Assert.That(session.HaltReason, Is.Null);
                foreach (var add in session.Snapshot().enemies.Where(e => e.finalAdd))
                {
                    world.ForceUnitDeath(OriginalWorld.EnemyEntityId(add.entityId));
                    Assert.That(session.ReportEnemyKilled(add.entityId, false), Is.True); killed++;
                }
                if (!startedWarning && world.Clock >= 57)
                {
                    // Trusted SPELL_EFFECT seam: its h04R warning dies after1.6s,
                    // before the next VA portal warning can die at61.3s.
                    Call(session, "BeginCasterEffect", 9001, 0, new OriginalCasterRules(catalog, "A0Z9"),
                        new OriginalPoint(-2500, -2500), world.Clock);
                    startedWarning = true;
                }
                if (startedWarning && session.Snapshot().phase == OriginalMatchPhase.Combat)
                { resumed = true; break; }
                if (world.Clock >= 59) break;
            }
            Assert.That(killed, Is.EqualTo(16));
            Assert.That(resumed, Is.True, "A caster warning KillUnit must reach the same CA listener as an Orn warning.");
            Assert.That(world.Clock, Is.LessThan(59));
            Assert.That(Match(session).FinalStage, Is.EqualTo(1));
            Assert.That(session.Snapshot().effects.Any(e => e.abilityId == "n062" || e.abilityId == "h016"), Is.False);
            Assert.That(session.CasterTelegraphs, Is.Empty, "The activation callback continues after synchronous resume.");
            Assert.That((bool)typeof(OriginalSession).GetField("casterGate", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(session), Is.True);
        }
    }
}

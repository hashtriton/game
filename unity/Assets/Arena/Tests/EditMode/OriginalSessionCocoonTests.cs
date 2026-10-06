using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionCocoonTests
    {
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        [Test] public void CocoonTimerReadsLiveNearbyAlliesAndTracksDeparture()
        {
            var args = new object[] { "H008", null };
            var s = (OriginalSession)typeof(OriginalSessionRegenerationTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            var w = (OriginalWorld)args[1]; s.DrainEvents();
            var m = (OriginalMatch)typeof(OriginalSession).GetField("match", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            typeof(OriginalMatch).GetProperty("Round").SetValue(m, 23);
            Call(m, "StartCombat"); Call(s, "CollectEvents"); s.DrainEvents();
            s.Advance(.55); s.DrainEvents();
            Assert.That(s.HaltReason, Is.Null);
            var cocoon = m.Enemies.Single(e => e.cocoon);
            var body = w.UnitState(OriginalWorld.EnemyEntityId(cocoon.entityId));
            Assert.That(body.paused, Is.True);
            Assert.That(body.invulnerable, Is.False);
            var p = body.position;
            Assert.That(w.TryMove(body.entityId, new OriginalPoint(p.x + 100, p.y)), Is.False);
            var profile = new OriginalWorldUnitProfile { moveSpeed = 0, collisionRadius = 1, maxHealth = 100, maxMana = 0 };
            w.AddUnit(500001, 0, "n067", profile, new OriginalPoint(p.x + 100, p.y));
            w.AddUnit(500002, 0, "n068", profile, new OriginalPoint(p.x - 100, p.y));
            Assert.That(w.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 500002, visible = false } }), Is.True);
            w.AddUnit(500003, 0, "n067", profile, new OriginalPoint(p.x + 326, p.y));
            w.AddUnit(2, 2, "n067", profile, new OriginalPoint(p.x, p.y + 100));
            w.AddUnit(500005, 0, "n068", profile, new OriginalPoint(p.x, p.y - 100));
            w.ForceUnitDeath(500005);
            w.AddUnit(500006, 0, "n066", profile, new OriginalPoint(p.x, p.y + 200));
            Call(s, "SyncCocoonAccelerators");
            Assert.That(cocoon.nearbyAccelerators, Is.EqualTo(2));
            // Call the real referee timer after the session bridge has sampled
            // the world. No simulated weapon/native sparse stats are needed.
            Call(m, "TickCocoon", cocoon.entityId);
            Assert.That(cocoon.cocoonTimer, Is.EqualTo(27));
            w.ForcePosition(500001, new OriginalPoint(p.x + 326, p.y));
            w.ForceUnitDeath(500002);
            Call(s, "SyncCocoonAccelerators");
            Assert.That(cocoon.nearbyAccelerators, Is.Zero);
        }

        [Test] public void OrdinaryUncountedCocoonRawcodeDoesNotGainElvPause()
        {
            var args = new object[] { "H008", null };
            var s = (OriginalSession)typeof(OriginalSessionRegenerationTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            var w = (OriginalWorld)args[1]; s.DrainEvents();
            var m = (OriginalMatch)typeof(OriginalSession).GetField("match", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            typeof(OriginalMatch).GetProperty("Round").SetValue(m, 22);
            var ordinary = (OriginalMatchEnemy)Call(m, "Spawn", "u00L", 0f, 0f, 270f, false, false, false, false, 0, 0);
            Call(s, "CollectEvents"); s.DrainEvents();
            var body = w.UnitState(OriginalWorld.EnemyEntityId(ordinary.entityId));
            Assert.That(s.HaltReason, Is.Null);
            Assert.That(body.paused, Is.False);
            Assert.That(body.invulnerable, Is.False);
            Assert.That(ordinary.cocoon, Is.False);
            Assert.That(ordinary.cocoonTimer, Is.Zero);
        }
    }
}

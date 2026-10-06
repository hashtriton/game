using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionOrnAbilityTests
    {
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, args);
        static object Field(object target, string name) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(target);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { null, false };
            var s = (OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);
            world = (OriginalWorld)Field(s, "world");
            var match = (OriginalMatch)Field(s, "match");
            typeof(OriginalMatch).GetProperty("Phase").SetValue(match, OriginalMatchPhase.Combat);
            world.ForcePosition(1, new OriginalPoint(0, -2700));
            world.AddUnit(1001, 0, "O006", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 30000, maxMana = 2500, moveSpeed = 390 }, new OriginalPoint(-200, -2700));
            return s;
        }
        static void Tick(OriginalSession s, OriginalWorld w, double seconds)
        {
            while (seconds > 1e-9)
            {
                double step = Math.Min(seconds, .01); w.Advance(step); Call(s, "AdvanceNativeWeaponProcs", step); Call(s, "AdvanceOrnAbilities"); seconds -= step;
            }
            Assert.That(s.HaltReason, Is.Null);
        }
        static void Unlock(OriginalSession s, int actor, string ability) => Call(s, "ApplyUnitAbilityOverlay", actor, new[] { ability }, Array.Empty<string>());
        [Test] public void NativeDelayedCastDebitsOnlyAtEffectAndAcceptedStopCancels()
        {
            var s=Create(out var w);Unlock(s,1001,"A0U0");
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A0U0",0),Is.True);
            Tick(s,w,.29);Assert.That(w.UnitState(1001).mana,Is.EqualTo(2500));
            Call(s,"OnAcceptedWorldOrder",1001);Tick(s,w,.02);
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(2500));Assert.That(((IList)Field(s,"ornDecoys")).Count,Is.Zero);
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A0U0",0),Is.True);
            Tick(s,w,.3);Assert.That(w.UnitState(1001).mana,Is.EqualTo(2400));
            Assert.That(((IList)Field(s,"ornDecoys")).Count,Is.EqualTo(1));
        }
        [Test] public void NativeBerserkIsInstantAndDoesNotReplacePendingBolt()
        {
            var s=Create(out var w);Unlock(s,1001,"A10K");Unlock(s,1001,"A0TW");
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A0TW",1),Is.True);
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A10K",0),Is.True);
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(2350));
            Assert.That(((IDictionary)Field(s,"ornCasts")).Count,Is.EqualTo(1));
            Assert.That(((IList)Field(s,"ornSpins")).Count,Is.EqualTo(1));
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A10K",0),Is.False);
            Tick(s,w,.3);Assert.That(w.UnitState(1001).mana,Is.EqualTo(2300));
            Assert.That(w.UnitState(1).paused,Is.True);
        }
        [Test] public void BoltNativeStunOutlivesPausedKickWithoutDamagingTarget()
        {
            var s=Create(out var w);Unlock(s,1001,"A0TW");double hp=w.UnitState(1).health;
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A0TW",1),Is.True);
            Tick(s,w,.3);Assert.That((bool)Call(s,"ActorCastBlocked",1),Is.False);
            Tick(s,w,.01);Assert.That((bool)Call(s,"ActorCastBlocked",1),Is.True);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Tick(s,w,1.19);Assert.That(w.UnitState(1).paused,Is.False);
            Assert.That((bool)Call(s,"ActorCastBlocked",1),Is.True);
            Tick(s,w,1.99);Assert.That((bool)Call(s,"ActorCastBlocked",1),Is.True);
            Tick(s,w,.01);Assert.That((bool)Call(s,"ActorCastBlocked",1),Is.False);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
        }
        [Test] public void DeadTargetBeforeEffectDoesNotConsumeBoltManaOrCooldown()
        {
            var s=Create(out var w);Unlock(s,1001,"A0TW");
            Assert.That((bool)Call(s,"TryStartOrnCast",1001,"A0TW",1),Is.True);
            w.ForceUnitDeath(1);Tick(s,w,.3);
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(2500));
            Assert.That(((IDictionary)Field(s,"ornCooldowns")).Count,Is.Zero);
        }
        [Test] public void FinalResumeUnlocksOnlyItsAuthoredAbility()
        {
            var s = Create(out var w); var match = (OriginalMatch)Field(s, "match");
            var ids = new[] { "A0TW", "A0U0", "A10K" };
            for (int stage = 1; stage <= 3; stage++)
            {
                typeof(OriginalMatch).GetField("finalStage", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(match, stage);
                Call(s, "OnOrnActiveMatchEvent", new OriginalMatchEvent { kind = OriginalMatchEventKind.BossResume, sourceRule = "final-phases", entityId = 1 });
                for (int i = 0; i < 3; i++) Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(1001), ids[i]), Is.EqualTo(i < stage));
            }
        }
        [Test] public void PeriodicCleanseRemovesAllBpseSourcesButPreservesSilence()
        {
            var s = Create(out var w);
            Call(s, "AddTimedNativeStun", 1001, "BPSE", 1, 2d);
            Call(s, "SetActorControl", 1001, "native-bash:1:A05B", OriginalActorControlMask.Move | OriginalActorControlMask.Weapon | OriginalActorControlMask.Cast, 2d, true, true);
            Call(s, "AddNativeSilence", 1001, 1, 7d);
            Call(s, "SelectOrnActiveOrders", w.UnitState(1001));
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1001), Is.False);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1001), Is.True);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1001), Is.True);
        }
        [Test] public void KickMovesPausedVictimAndRestoresAfterStrictFinalCallback()
        {
            var s = Create(out var w); Call(s, "BeginOrnKick", 1001, 1);
            Assert.That(w.UnitState(1).paused && w.UnitState(1).pathingDisabled, Is.True);
            Tick(s, w, 1.16); Assert.That(w.UnitState(1).position.x, Is.EqualTo(580).Within(.01));
            Assert.That(w.UnitState(1).paused, Is.True);
            Tick(s, w, .04); Assert.That(w.UnitState(1).paused || w.UnitState(1).pathingDisabled, Is.False);
        }
        [Test] public void KickKillsIllusionButRetainsDeadPositionForDisplacement()
        {
            var s = Create(out var w);
            w.AddIllusion(1000000000, 1, new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 100 }, new OriginalPoint(80, -2700), 100, 0);
            Call(s, "BeginOrnKick", 1001, 1000000000);
            Assert.That(w.UnitState(1000000000).health, Is.Zero);
            Tick(s, w, 1.2);
            Assert.That(w.UnitState(1000000000).position.x, Is.EqualTo(660).Within(.01));
            Assert.That(w.UnitState(1).health, Is.GreaterThan(0));
            Assert.That(w.UnitState(1000000000).paused, Is.False);
        }
        [Test] public void SpinEmptyAndDeadPartyFinishesWithoutChaseOrLoop()
        {
            foreach (bool remove in new[] { false, true })
            {
                var s = Create(out var w);
                if (remove) w.RemoveUnit(1); else w.ForceUnitDeath(1);
                var before = w.UnitState(1001).position; Call(s, "BeginOrnSpin", 1001); Tick(s, w, 6.1);
                Assert.That(w.UnitState(1001).position.x, Is.EqualTo(before.x));
                Assert.That(w.UnitState(1001).paused || w.UnitState(1001).pathingDisabled, Is.False);
                Assert.That(((IList)Field(s, "ornSpins")).Count, Is.Zero);
                Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(1001), "A077"), Is.False);
            }
        }
        [Test] public void SpinDealsTenUniversalPulsesAndCleansMagicImmunity()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            w.UpdateProfile(1, new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 10000, maxMana = hero.profile.maxMana }, 10000, hero.mana);
            Call(s, "BeginOrnSpin", 1001); Tick(s, w, 1.55);
            Assert.That(w.UnitState(1001).paused, Is.False); Assert.That(w.UnitState(1).health, Is.EqualTo(10000));
            Tick(s, w, .01); Assert.That(w.UnitState(1001).paused, Is.True);
            Assert.That((bool)Call(s, "CasterMagicImmune", w.UnitState(1001)), Is.True);
            Tick(s, w, 4.4);
            Assert.That(w.UnitState(1).health, Is.EqualTo(8200));
            Assert.That(w.UnitState(1001).paused || w.UnitState(1001).pathingDisabled, Is.False);
            Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(1001), "A077"), Is.False);
        }
        [Test] public void DecoysChooseAfterPointSixFiveAndBurstAfterAnotherOnePointTwo()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            w.UpdateProfile(1, new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 10000, maxMana = hero.profile.maxMana }, 10000, hero.mana);
            Call(s, "BeginOrnDecoys", 1001);
            var copies = ((IList)Field(s, "ornDecoys"))[0]; var points = (OriginalPoint[])Field(copies, "positions");
            Assert.That(points.Length, Is.EqualTo(8)); Assert.That(points[0].x, Is.Zero);
            Assert.That(w.UnitState(1001).paused && w.UnitState(1001).invulnerable, Is.True);
            Tick(s, w, .64); Assert.That(w.UnitState(1001).position.x, Is.EqualTo(-200));
            Tick(s, w, .01); int chosen = (int)Field(copies, "chosen");
            Assert.That(w.UnitState(1001).position.x, Is.EqualTo(points[chosen].x).Within(1e-8));
            Tick(s, w, 1.19); Assert.That(w.UnitState(1).health, Is.EqualTo(10000));
            double expected = 0;
            for (int i = 0; i < 8; i++) if (points[i].x * points[i].x + Math.Pow(points[i].y + 2700, 2) <= 170 * 170)
                expected += (i == chosen ? 1500 : 750) * .8;
            Tick(s, w, .01); Assert.That(w.UnitState(1).health, Is.EqualTo(10000 - expected).Within(1e-7));
            Assert.That(w.UnitState(1001).paused || w.UnitState(1001).invulnerable, Is.False);
            Assert.That(((IList)Field(s, "ornDecoys")).Count, Is.Zero);
        }
        [Test] public void DecoysKeepTheRetainedDisconnectedHeroPosition()
        {
            var s=Create(out var w);var player=((IList)Field(s,"players"))[0];
            player.GetType().GetField("connected").SetValue(player,false);
            int slot=(int)player.GetType().GetField("matchSlot").GetValue(player);
            Call(s,"BeginOrnDecoys",1001);
            var points=(OriginalPoint[])Field(((IList)Field(s,"ornDecoys"))[0],"positions");
            Assert.That(points[slot-1].x,Is.EqualTo(w.UnitState(1).position.x));
            Assert.That(points[slot-1].y,Is.EqualTo(w.UnitState(1).position.y));
        }
    }
}

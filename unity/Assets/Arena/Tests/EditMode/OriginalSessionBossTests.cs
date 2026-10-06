using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionBossTests
    {
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        static OriginalMatch Match(OriginalSession s) => (OriginalMatch)typeof(OriginalSession)
            .GetField("match", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
        static OriginalSession StartRound(int round, out OriginalWorld world)
        {
            var args = new object[] { "H008", null };
            var s = (OriginalSession)typeof(OriginalSessionRegenerationTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            world = (OriginalWorld)args[1]; s.DrainEvents();
            // Fixture jumps the round index only. Actual StartCombat owns roster,
            // scaling, phase timers, actor publication and source events.
            typeof(OriginalMatch).GetProperty("Round").SetValue(Match(s), round);
            Call(Match(s), "StartCombat"); Call(s, "CollectEvents"); s.DrainEvents();
            Assert.That(s.HaltReason, Is.Null); return s;
        }
        static void Tick(OriginalSession s, double seconds)
        {
            while (seconds > 1e-9) { double step = Math.Min(seconds, .05); s.Advance(step); s.DrainEvents(); seconds -= step; }
            Assert.That(s.HaltReason, Is.Null);
        }
        static void WorldTick(OriginalWorld w, double seconds)
        {
            while (seconds > 1e-9) { double step = Math.Min(seconds, .05); w.Advance(step); seconds -= step; }
        }
        [Test] public void WindProjectileTurnsBeforeMovingAndReadsChangedPhaseOnEveryTick()
        {
            var wind = new OriginalBossWindRules(new OriginalPoint(0, 0), 0);
            Assert.That(wind.Tick(4, out var sweep), Is.True);
            Assert.That(sweep.x, Is.EqualTo(14).Within(1e-9)); Assert.That(sweep.y, Is.Zero);
            Assert.That(wind.Tick(2, out sweep), Is.True);
            Assert.That(sweep.x, Is.EqualTo(14 + 22 * Math.Cos(.04)).Within(1e-9));
            Assert.That(sweep.y, Is.EqualTo(22 * Math.Sin(.04)).Within(1e-9));
            Assert.That(wind.Tick(1, out sweep), Is.True);
            Assert.That(wind.Travelled, Is.EqualTo(62));
            while (!wind.Completed) Assert.That(wind.Tick(0, out _), Is.True);
            var end = wind.Position; Assert.That(wind.Travelled, Is.GreaterThanOrEqualTo(1050));
            Assert.That(wind.Tick(0, out sweep), Is.False); Assert.That(sweep.x, Is.EqualTo(end.x));
            Assert.That(OriginalBossWindRules.ThresholdReached(3, .600001), Is.False);
            Assert.That(OriginalBossWindRules.ThresholdReached(3, .6), Is.True);
        }
        [Test] public void WindPhasesUnlockWardsAndBindingAtTheAuthoredRemainingCounts()
        {
            var s = StartRound(20, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true); w.ForcePosition(1, new OriginalPoint(2000, 1000));
            for (int phase = 1; phase <= 3; phase++)
            {
                Call(s, "BeginBossWindPhase", boss);
                for (int i = 0; i < 14; i++) { WorldTick(w, 1.5); Call(s, "AdvanceBossWind"); }
                Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(boss), "A0TS"), Is.EqualTo(phase >= 2));
                Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(boss), "A0TU"), Is.EqualTo(phase >= 3));
            }
        }
        [Test] public void BarrageUsesThirtyTwoHertzPhaseClockAndUnlocksOnlyAfterTheFinalCallback()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(1, new OriginalPoint(2000, 1000)); w.SetUnitState(1, invulnerable: true);
            for (int phase = 0; phase < 4; phase++)
            {
                double begin = w.Clock;
                Call(s, "BeginBossBarrage", boss, 0d);
                WorldTick(w, 1.249); Call(s, "AdvanceBossBarrage");
                var bolts = (System.Collections.IList)typeof(OriginalSession).GetField("bossBarrageBolts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
                Assert.That(bolts.Count, Is.Zero);
                w.Advance(.001); Call(s, "AdvanceBossBarrage");
                Assert.That(bolts.Count, Is.EqualTo(phase == 3 ? 2 : 1));
                double left = 20.0625 - (w.Clock - begin) - .001;
                while (left > 1e-9) { double step = Math.Min(.05, left); w.Advance(step); Call(s, "AdvanceBossBarrage"); left -= step; }
                Assert.That(w.UnitState(boss).paused, Is.True);
                w.Advance(.001); Call(s, "AdvanceBossBarrage");
                Assert.That(w.UnitState(boss).paused, Is.False);
                string unlocked = phase == 0 ? "A1D6" : phase == 1 ? "A1D7" : "A1D8";
                Assert.That((bool)Call(s, "HasEffectiveUnitAbility", w.UnitState(boss), unlocked), Is.True);
                WorldTick(w, 1.2); Call(s, "AdvanceBossBarrage");
            }
        }
        [TestCase(0, 0)] [TestCase(45, 60)]
        public void BarrageCrystalStopsAProjectileBeforeAHeroBehindIt(int degrees, int damage)
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "BeginBossBarrage", boss, 0d);
            double angle = degrees * .0174532;
            w.ForcePosition(1, new OriginalPoint(780 * Math.Cos(angle), -2700 + 780 * Math.Sin(angle)));
            double hp = w.UnitState(1).health;
            Call(s, "BeginBossBarrageBolt", boss, 0, new OriginalPoint(0, -2700), (double)degrees, w.Clock);
            WorldTick(w, 1.08); Call(s, "AdvanceBossBarrage");
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - damage).Within(.001));
        }
        [TestCase(1200, false, 99999)] [TestCase(1201, false, 0)] [TestCase(1200, true, 0)]
        public void ProphecyTracksTheBossAndUsesFloatCountdownRadiusAndDeathCancellation(int distance, bool dead, int expectedDamage)
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Call(s, "BeginBossRifts", boss, 0, w.Clock); WorldTick(w, 2); Call(s, "AdvanceBossRifts");
            Assert.That(w.Snapshot().doodads.Length, Is.EqualTo(2));
            var hero = w.UnitState(1); var profile = hero.profile; profile.maxHealth = 200000;
            w.UpdateProfile(1, profile, 200000, hero.mana);
            w.ForcePosition(boss, new OriginalPoint(0, -2700)); w.ForcePosition(1, new OriginalPoint(distance, -2700));
            var type = typeof(OriginalSession).GetNestedType("BossProphecy", BindingFlags.NonPublic);
            var state = Activator.CreateInstance(type, true);
            type.GetField("actor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(state, boss);
            type.GetField("due", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(state, w.Clock + .04);
            var list = (System.Collections.IList)typeof(OriginalSession).GetField("bossProphecies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            list.Add(state);
            if (dead) { w.SetUnitState(boss, invulnerable: false); w.ApplyUnitDamage(boss, 100000000); }
            // Vav subtracts .04 from a JASS real. Float accumulation leaves a
            // positive residue after250 callbacks; callback251 resolves it.
            WorldTick(w, 10); Call(s, "AdvanceBossBarrage"); Assert.That(w.UnitState(1).health, Is.EqualTo(200000));
            w.Advance(.04); Call(s, "AdvanceBossBarrage");
            Assert.That(w.UnitState(1).health, Is.EqualTo(200000 - expectedDamage));
            Assert.That(list.Count, Is.Zero);
            if (!dead) Assert.That(w.Snapshot().doodads.Length, Is.Zero);
        }
        [Test] public void BossRiftsWarnForTwoSecondsThenPublishTwoDistinctInvulnerableObjects()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Call(s, "BeginBossRifts", boss, 0, w.Clock);
            WorldTick(w, 1.99); Call(s, "AdvanceBossRifts");
            Assert.That(w.Snapshot().doodads.Length, Is.Zero);
            w.Advance(.01); Call(s, "AdvanceBossRifts");
            var first = w.Snapshot().doodads;
            Assert.That(first.Length, Is.EqualTo(2));
            foreach (var d in first)
            {
                Assert.That(d.rawcode, Is.EqualTo("B009")); Assert.That(d.dynamic && d.invulnerable, Is.True);
                Assert.That(d.health, Is.EqualTo(9999)); Assert.That(d.scale, Is.EqualTo(1.2));
                Assert.That(w.ApplyDoodadDamage(d.editorId, 10000), Is.False);
            }
            Call(s, "ClearBossRifts", boss); Assert.That(w.Snapshot().doodads.Length, Is.Zero);
            Call(s, "BeginBossRifts", boss, 0, w.Clock); WorldTick(w, 2); Call(s, "AdvanceBossRifts");
            Assert.That(w.Snapshot().doodads.Length, Is.EqualTo(2));
            foreach (var d in w.Snapshot().doodads)
                Assert.That(first.Any(x => x.position.x == d.position.x && x.position.y == d.position.y), Is.False);
        }
        [Test] public void MeteorImpactSpawnsOneTimedMinionEvenWhenEligibleHeroIsInvulnerable()
        {
            var s = StartRound(10, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: true); w.SetUnitState(1, paused: true, invulnerable: true);
            w.ForcePosition(1, new OriginalPoint(200, -2700)); double before = w.UnitState(1).health;
            Call(s, "ResolveBossMeteor", boss, 0, new OriginalPoint(200, -2700), w.Clock);
            var summon = w.Snapshot().units.Single(u => u.rawcode == "n06W");
            Assert.That(summon.profile.maxHealth, Is.EqualTo(1000)); Assert.That(summon.profile.maxMana, Is.Zero);
            Assert.That((int)Call(s, "SourceUnitUserData", summon.entityId), Is.Zero);
            Assert.That(w.UnitState(1).health, Is.EqualTo(before));
            Assert.That(s.Snapshot().enemies.Length, Is.EqualTo(1));
            WorldTick(w, 59.99); Call(s, "AdvanceBossMeteors"); Assert.That(w.UnitState(summon.entityId).health, Is.GreaterThan(0));
            w.Advance(.01); Call(s, "AdvanceBossMeteors"); Assert.That(w.UnitState(summon.entityId).health, Is.Zero);
        }
        [TestCase(1, 0)] [TestCase(0, 0)] [TestCase(1, 360)]
        public void BossSilenceWarnsThenBlocksWeaponsAndSpellsAndUsesMaxManaMinusHalfCurrent(double manaFraction, int distance)
        {
            var s = StartRound(15, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            var hero = w.UnitState(1); double mana = hero.profile.maxMana * manaFraction;
            w.UpdateProfile(1, hero.profile, hero.profile.maxHealth, mana); w.SetUnitState(1, paused: false); w.Stop(1);
            w.ForcePosition(1, new OriginalPoint(distance, 1000));
            Call(s, "BeginBossSilence", boss, 0, new OriginalPoint(0, 1000));
            WorldTick(w, 1.99); Call(s, "AdvanceBossSilences");
            Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.False);
            w.Advance(.01); Call(s, "AdvanceBossSilences");
            double damage = distance == 0 ? (hero.profile.maxMana - mana * .5) * .8 : 0;
            Assert.That(w.UnitState(1).health, Is.EqualTo(hero.profile.maxHealth - damage).Within(.001));
            Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.True);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.False);
            Call(s, "AdvanceActorControls", 6.99d); Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.True);
            Call(s, "AdvanceActorControls", .01d); Assert.That((bool)Call(s, "HasNativeSilence", 1), Is.False);
        }
        [Test] public void BossSilenceDoesNotApplyHeroOnlyManaDamageToNativeMirrorImages()
        {
            var s = StartRound(15, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            var hero = w.UnitState(1); w.ForcePosition(1, new OriginalPoint(2000, 1000));
            w.AddIllusion(1000000000, 1, hero.profile, new OriginalPoint(0, 1000), hero.health, hero.mana);
            Call(s, "BeginBossSilence", boss, 0, new OriginalPoint(0, 1000));
            WorldTick(w, 2); Call(s, "AdvanceBossSilences");
            Assert.That((bool)Call(s, "HasNativeSilence", 1000000000), Is.True);
            Assert.That(w.UnitState(1000000000).health, Is.EqualTo(hero.health));
        }
        [Test] public void BanishBlocksWeaponSlowsAndAmplifiesSpellsThenExpiresInFiveSeconds()
        {
            var s = StartRound(15, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: false, invulnerable: false); w.SetUnitState(1, paused: false);
            Call(s, "BeginBossBanish", boss, 1);
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(62.5));
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.False);
            double health = w.UnitState(1).health;
            Call(s, "ApplyTriggeredHit", boss, 0, w.UnitState(1), 40d, OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health, Is.EqualTo(health - 40 * .8 * 1.66).Within(.001));
            WorldTick(w, 4.99); Call(s, "AdvanceBossBanishes");
            Assert.That((bool)Call(s, "HasBossBanish", 1), Is.True);
            w.Advance(.01); Call(s, "AdvanceBossBanishes");
            Assert.That((bool)Call(s, "HasBossBanish", 1), Is.False);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.False);
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
        }
        [Test] public void BindingNativeCastUsesHalfSecondAndOnlyMovementSlowUntilAuthoredFourSecondCleanup()
        {
            var s = StartRound(20, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: false, invulnerable: false); w.ForcePosition(1, new OriginalPoint(400, -2700));
            Call(s, "ApplyUnitAbilityOverlay", boss, new[] { "A0TU" }, null);
            var profile = w.UnitState(boss).profile; profile.maxMana = 5000; w.UpdateProfile(boss, profile, profile.maxHealth, 5000);
            double rate = (double)Call(s, "WeaponRate", w.UnitState(1));
            Assert.That((bool)Call(s, "TryStartBossBindingCast", boss, 1), Is.True);
            WorldTick(w, .499); Call(s, "AdvanceBossNativeCasts"); Assert.That(w.UnitState(boss).mana, Is.EqualTo(5000));
            w.Advance(.001); Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(boss).mana, Is.EqualTo(4700));
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(25).Within(.0001));
            Assert.That((double)Call(s, "WeaponRate", w.UnitState(1)), Is.EqualTo(rate));
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            WorldTick(w, 3.99); Call(s, "AdvanceBossBindings"); Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(25).Within(.0001));
            w.Advance(.01); Call(s, "AdvanceBossBindings"); Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
        }
        [TestCase(false, 480)] [TestCase(true, 0)]
        public void BurstWarnsForThirtyNineCallbacksThenSharesVictimsAndHealsFixedAmount(bool invulnerable, int damage)
        {
            var s = StartRound(20, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(boss, new OriginalPoint(0, -2700)); w.SetFacing(boss, 0);
            w.SetUnitState(boss, paused: false, invulnerable: false); var actor = w.UnitState(boss);
            w.UpdateProfile(boss, actor.profile, actor.profile.maxHealth - 1000, actor.mana);
            var hero = w.UnitState(1); var profile = hero.profile; profile.maxHealth = 5000;
            w.UpdateProfile(1, profile, 5000, hero.mana); w.ForcePosition(1, new OriginalPoint(70, -2700));
            w.SetUnitState(1, paused: false, invulnerable: invulnerable);
            Assert.That((bool)Call(s, "TryStartBossBurst", boss), Is.True);
            Assert.That(w.UnitState(boss).mana, Is.EqualTo(actor.mana - 100));
            WorldTick(w, 1.559); Call(s, "AdvanceBossBursts");
            Assert.That(s.Snapshot().effects.Count(e => e.abilityId == "A11F" && e.kind == OriginalVisualEffectKind.Ghost), Is.Zero);
            w.Advance(.001); Call(s, "AdvanceBossBursts");
            Assert.That(s.Snapshot().effects.Count(e => e.abilityId == "A11F" && e.kind == OriginalVisualEffectKind.Ghost), Is.EqualTo(6));
            Assert.That(w.UnitState(1).health, Is.EqualTo(5000));
            w.Advance(.04); Call(s, "AdvanceBossBursts");
            Assert.That(w.UnitState(1).health, Is.EqualTo(5000 - damage));
            Assert.That(w.UnitState(boss).health, Is.EqualTo(actor.profile.maxHealth - 700));
            WorldTick(w, .76); Call(s, "AdvanceBossBursts");
            Assert.That(w.UnitState(1).health, Is.EqualTo(5000 - damage));
            Assert.That(w.UnitState(boss).health, Is.EqualTo(actor.profile.maxHealth - 700));
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A11F"), Is.False);
        }
        [Test] public void DoomControlFreezesWithPauseAndNegativeCleanupRemovesItsTimer()
        {
            var s = StartRound(20, out var w); w.SetUnitState(1, paused: false);
            double health = w.UnitState(1).health;
            Call(s, "QueueBossDoom", 0, 1);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.False);
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.False);
            WorldTick(w, 1); Call(s, "AdvanceBossDooms");
            w.SetUnitState(1, paused: true); WorldTick(w, 2); Call(s, "AdvanceBossDooms");
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True);
            w.SetUnitState(1, paused: false); Call(s, "AdvanceBossDooms");
            WorldTick(w, 1.999); Call(s, "AdvanceBossDooms"); Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True);
            w.Advance(.001); Call(s, "AdvanceBossDooms"); Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            Assert.That(w.UnitState(1).health, Is.EqualTo(health));
            Call(s, "QueueBossDoom", 0, 1); Call(s, "ClearNegativeActorControls", 1);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            var states = (System.Collections.IDictionary)typeof(OriginalSession).GetField("bossDooms", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Assert.That(states.Count, Is.Zero);
        }
        [TestCase(false, -2.5)] [TestCase(true, -2.85)]
        public void DreadAuraSubtractsMaximumManaRateForActiveAndPausedHeroWithoutGoingNegative(bool paused, double rate)
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            var origin = w.UnitState(boss).position;
            w.ForcePosition(1, new OriginalPoint(origin.x + 800, origin.y)); w.SetUnitState(1, paused: paused);
            Call(s, "ApplyUnitAbilityOverlay", boss, new[] { "A1D8" }, null);
            Call(s, "AdvanceBossManaAura", 0d); WorldTick(w, .6); Call(s, "AdvanceBossManaAura", .6d);
            var hero = w.UnitState(1); w.UpdateProfile(1, hero.profile, hero.health, 72.5);
            Call(s, "AdvanceRegeneration", 1d);
            Assert.That(w.UnitState(1).mana, Is.EqualTo(72.5 + rate).Within(.0001));
            hero = w.UnitState(1); w.UpdateProfile(1, hero.profile, hero.health, .01);
            Call(s, "AdvanceRegeneration", 1d); Assert.That(w.UnitState(1).mana, Is.Zero);
            Call(s, "ApplyUnitAbilityOverlay", boss, null, new[] { "A1D8" });
            WorldTick(w, 3.099); Call(s, "AdvanceBossManaAura", 0d);
            Assert.That((double)Call(s, "BossManaAuraRate", w.UnitState(1)), Is.EqualTo(-2.9).Within(.0001));
            w.Advance(.001); Call(s, "AdvanceBossManaAura", 0d);
            Assert.That((double)Call(s, "BossManaAuraRate", w.UnitState(1)), Is.Zero);
        }
        [Test] public void StormUsesThreeWarningsThenVisibleFifteenSecondInfernalWithoutTheOtherSpellsLanding()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Call(s, "BeginBossStorm", boss);
            var origin = w.UnitState(boss).position;
            var effects = s.Snapshot().effects.Where(e => e.abilityId == "A1D7").ToArray();
            Assert.That(effects.Length, Is.EqualTo(3));
            foreach (var e in effects)
            { Assert.That(Math.Abs(e.position.x - origin.x), Is.LessThanOrEqualTo(700)); Assert.That(Math.Abs(e.position.y - origin.y), Is.LessThanOrEqualTo(700)); }
            WorldTick(w, .999); Call(s, "AdvanceBossStorms"); Assert.That(w.Snapshot().units.Any(u => u.rawcode == "n025"), Is.False);
            w.Advance(.001); Call(s, "AdvanceBossStorms");
            var summons = w.Snapshot().units.Where(u => u.rawcode == "n025").ToArray();
            Assert.That(summons.Length, Is.EqualTo(3)); Assert.That(summons.All(u => !u.hidden && !u.paused), Is.True);
            Assert.That(s.Snapshot().enemies.Length, Is.EqualTo(1));
            WorldTick(w, 14.99); Call(s, "AdvanceBossMeteors"); Assert.That(w.UnitState(summons[0].entityId).health, Is.GreaterThan(0));
            w.Advance(.01); Call(s, "AdvanceBossMeteors"); Assert.That(summons.All(u => w.UnitState(u.entityId).health == 0), Is.True);
        }
        [Test] public void StormScriptDamagePrecedesNativeMissileStunAndDoesNotRepeatItAtLanding()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            var hero = w.UnitState(1); var profile = hero.profile; profile.maxHealth = 5000;
            w.UpdateProfile(1, profile, 5000, hero.mana); w.ForcePosition(1, new OriginalPoint(200, -2700));
            w.SetUnitState(1, paused: false);
            Call(s, "ResolveBossStorm", boss, 0, new OriginalPoint(200, -2700), w.Clock);
            Assert.That(w.UnitState(1).health, Is.EqualTo(4200)); Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            w.Advance(.004); Call(s, "AdvanceBossStorms"); Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            w.Advance(.001); Call(s, "AdvanceBossStorms");
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True); Assert.That(w.UnitState(1).health, Is.EqualTo(4200));
            Call(s, "AdvanceActorControls", 2d); Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
        }
        [Test] public void DeathFingerRequiresPhaseUnlockThenDebitsAtPointThreeAndUsesMaximumHealth()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: false, invulnerable: false); w.ForcePosition(1, new OriginalPoint(400, -2700));
            var profile = w.UnitState(boss).profile; profile.maxMana = 5000; w.UpdateProfile(boss, profile, profile.maxHealth, 5000);
            Assert.That((bool)Call(s, "TryStartBossDeathFingerCast", boss, 1), Is.False);
            Call(s, "ApplyUnitAbilityOverlay", boss, new[] { "A1D6" }, null);
            var hero = w.UnitState(1);
            Assert.That((bool)Call(s, "TryStartBossDeathFingerCast", boss, 1), Is.True);
            WorldTick(w, .299); Call(s, "AdvanceBossNativeCasts"); Assert.That(w.UnitState(1).health, Is.EqualTo(hero.health));
            w.Advance(.001); Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(1).health, Is.EqualTo(hero.health - hero.profile.maxHealth * .2).Within(.0001));
            Assert.That(w.UnitState(boss).mana, Is.EqualTo(4825));
            Assert.That((bool)Call(s, "TryStartBossDeathFingerCast", boss, 1), Is.False);
            Assert.That(s.Snapshot().effects.Count(e => e.abilityId == "A1D6"), Is.EqualTo(1));
            WorldTick(w, .6); Call(s, "AdvanceBossDeathFinger"); Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A1D6"), Is.False);
        }
        [Test] public void BanishKeepsOneNonHeroMovementBaselineAcrossRefreshAndDispel()
        {
            var s = StartRound(15, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            double speed = w.UnitState(boss).profile.moveSpeed;
            w.SetUnitState(boss, paused: false, invulnerable: false);
            Call(s, "BeginBossBanish", 1, boss);
            Assert.That(w.UnitState(boss).profile.moveSpeed, Is.EqualTo(speed * .25));
            Call(s, "RefreshAbilityMovement", boss); Call(s, "ReleaseAbilityMovementBase", boss);
            Call(s, "RefreshAbilityMovement", boss);
            Assert.That(w.UnitState(boss).profile.moveSpeed, Is.EqualTo(speed * .25));
            Call(s, "RemoveBossBanish", boss, false);
            Assert.That(w.UnitState(boss).profile.moveSpeed, Is.EqualTo(speed));
        }
        [Test] public void BanishedChaosHitNotifiesBindingWithOneButDoesNotRemoveVictimHealth()
        {
            var s = StartRound(15, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, invulnerable: false);
            Call(s, "BeginBossBanish", boss, 1); Call(s, "BeginBossBinding", 1, boss);
            double hp = w.UnitState(1).health, bossHp = w.UnitState(boss).health;
            Call(s, "ApplyTriggeredHit", boss, 0, w.UnitState(1), 40d, OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp));
            Assert.That(w.UnitState(boss).health, Is.EqualTo(bossHp - 1));
            Call(s, "RemoveNegativeAbilityBuffs", 1);
            Assert.That((bool)Call(s, "HasBossBanish", 1), Is.False);
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
        }
        [TestCase(1, 1)] [TestCase(40, 0)]
        public void WardsUseTwoHpPerHitUntilTwoThenOrdinaryDamage(int damage, int expectedLastHealth)
        {
            var s = StartRound(20, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true);
            Call(s, "BeginBossWards", boss);
            var ward = w.Snapshot().units.First(u => u.rawcode == "u00H");
            Call(s, "ApplyResolvedUnitHit", 1, 1, ward, 0d, null);
            Assert.That(w.UnitState(ward.entityId).health, Is.EqualTo(8));
            for (int i = 1; i <= 4; i++)
            {
                Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(ward.entityId), (double)damage, null);
                Assert.That(w.UnitState(ward.entityId).health, Is.EqualTo(i < 4 ? 8 - 2 * i : expectedLastHealth));
                if (i < 4)
                {
                    Assert.That(w.UnitState(ward.entityId).invulnerable, Is.True);
                    // A second hit in the same native timer interval is rejected.
                    Assert.That((bool)Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(ward.entityId), 40d, null), Is.False);
                    w.Advance(.001); Call(s, "AdvanceBossWards");
                    Assert.That(w.UnitState(ward.entityId).invulnerable, Is.False);
                }
            }
        }
        [TestCase(.8, 4)] [TestCase(.3, 6)] [TestCase(.15, 8)]
        public void WardsUseSourceCountThresholdsAndFlyToHealThenRemove(double fraction, int count)
        {
            var s = StartRound(20, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            var actor = w.UnitState(boss); w.ForcePosition(boss, new OriginalPoint(0, -2700));
            double before = actor.profile.maxHealth * fraction;
            w.UpdateProfile(boss, actor.profile, before, actor.mana);
            w.SetUnitState(1, paused: true, invulnerable: true); w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Call(s, "BeginBossWards", boss);
            Assert.That(w.Snapshot().units.Count(u => u.rawcode == "u00H"), Is.EqualTo(count));
            for (int i = 0; i < 220; i++) { w.Advance(.05); Call(s, "AdvanceBossWards"); }
            Assert.That(w.UnitState(boss).health, Is.EqualTo(Math.Min(actor.profile.maxHealth, before + count * 1000)).Within(.001));
            Assert.That(w.Snapshot().units.Any(u => u.rawcode == "u00H"), Is.False);
            Assert.That(s.Snapshot().enemies.Length, Is.EqualTo(1));
        }
        [TestCase(210, true)] [TestCase(1089, false)]
        public void ShockwaveMovesBeforeSweepHitsOnceAndKeepsItsLastSweep(int targetX, bool hitAtFirstStep)
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(boss, new OriginalPoint(0, 1000));
            w.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = boss, setFacing = true, facing = 0 } });
            var profile = w.UnitState(1).profile; profile.maxHealth = 10000;
            w.UpdateProfile(1, profile, 10000, 0); w.SetUnitState(1, paused: true);
            w.ForcePosition(1, new OriginalPoint(targetX, 1000));
            Call(s, "BeginBossShockwave", boss);
            WorldTick(w, .039); Call(s, "AdvanceBossShockwaves"); Assert.That(w.UnitState(1).health, Is.EqualTo(10000));
            w.Advance(.001); Call(s, "AdvanceBossShockwaves"); Assert.That(w.UnitState(1).health, Is.EqualTo(hitAtFirstStep ? 9200 : 10000));
            WorldTick(w, .04); Call(s, "AdvanceBossShockwaves"); Assert.That(w.UnitState(1).health, Is.EqualTo(hitAtFirstStep ? 9200 : 10000));
            WorldTick(w, 1.12); Call(s, "AdvanceBossShockwaves");
            Assert.That(w.UnitState(1).health, Is.EqualTo(9200));
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A1D5"), Is.False);
        }
        [Test] public void ShockwaveBerserkIsImmediateAndReleasedEffectSurvivesCasterDeath()
        {
            var s = StartRound(25, out var w);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: false); double mana = w.UnitState(boss).mana;
            Assert.That((bool)Call(s, "TryStartBossShockwave", boss), Is.True);
            Assert.That(w.UnitState(boss).mana, Is.EqualTo(mana - 150));
            Assert.That((bool)Call(s, "TryStartBossShockwave", boss), Is.False);
            w.ForceUnitDeath(boss);
            var before = s.Snapshot().effects.Single(e => e.abilityId == "A1D5").position;
            WorldTick(w, .04); Call(s, "AdvanceBossShockwaves");
            var after = s.Snapshot().effects.Single(e => e.abilityId == "A1D5").position;
            Assert.That(Math.Abs(after.x - before.x) + Math.Abs(after.y - before.y), Is.GreaterThan(20));
        }
        [Test] public void BindingEchoesActualDamageEventAsUniversalAndExpiresAfterFourSeconds()
        {
            var s = StartRound(20, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: true); w.SetUnitState(1, paused: true, invulnerable: false);
            w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Call(s, "BeginBossBinding", boss, 1);
            double hp = w.UnitState(1).health;
            // The independently supplied native event stage, rather than raw
            // rolled weapon damage or final HP loss, is the source hL argument.
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 50d, (double?)100d);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - 100));
            Call(s, "RemoveNegativeAbilityBuffs", 1);
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 50d, (double?)100d);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - 200));
            WorldTick(w, 4); Call(s, "AdvanceBossBindings");
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 50d, (double?)100d);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - 200));
        }
        [Test] public void BindingDisablesOnlyItsOwnCallbackDuringRecursiveDamage()
        {
            var s = StartRound(20, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: true); w.SetUnitState(1, paused: true, invulnerable: true);
            Call(s, "BeginBossBinding", boss, boss);
            double hp = w.UnitState(boss).health;
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 20d, null);
            Assert.That(w.UnitState(boss).health, Is.EqualTo(hp - 40));
        }
        [Test] public void InfernoWarnsCreatesHiddenSummonThenLandsWithDamageAndIndependentStun()
        {
            var s = StartRound(10, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: true); w.SetUnitState(1, paused: false, invulnerable: false);
            w.ForcePosition(1, new OriginalPoint(2000, 1000)); w.Stop(1);
            double hp = w.UnitState(1).health;
            Call(s, "BeginBossInferno", boss, 0, new OriginalPoint(2000, 1000));
            WorldTick(w, .99); Call(s, "AdvanceBossInfernos");
            Assert.That(w.Snapshot().units.Any(u => u.rawcode == "n025"), Is.False);
            w.Advance(.01); Call(s, "AdvanceBossInfernos");
            var summon = w.Snapshot().units.Single(u => u.rawcode == "n025");
            Assert.That(summon.hidden, Is.True); Assert.That(summon.paused, Is.False);
            Assert.That(summon.profile.maxHealth, Is.EqualTo(3000)); Assert.That(summon.profile.maxMana, Is.Zero);
            Assert.That((int)Call(s, "LivingInfernalCount"), Is.EqualTo(1));
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp));
            WorldTick(w, 1); Call(s, "AdvanceBossInfernos");
            Assert.That(w.UnitState(summon.entityId).hidden, Is.False);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - 160).Within(.001));
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.True);
            Assert.That(w.UnitState(1).paused, Is.False);
            Call(s, "AdvanceActorControls", 1.99d); Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.True);
            Call(s, "AdvanceActorControls", .01d); Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.False);
        }
        [Test] public void InfernoCastSpendsAtNativeEffectAndReleasedWarningSurvivesSilence()
        {
            var s = StartRound(10, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true); w.ForcePosition(1, new OriginalPoint(2000, 1000));
            Assert.That((bool)Call(s, "TryStartBossInfernoCast", boss, w.UnitState(boss).position), Is.True);
            double mana = w.UnitState(boss).mana;
            WorldTick(w, .49); Call(s, "AdvanceBossNativeCasts"); Assert.That(w.UnitState(boss).mana, Is.EqualTo(mana));
            w.Advance(.01); Call(s, "AdvanceBossNativeCasts"); Assert.That(w.UnitState(boss).mana, Is.EqualTo(mana - 150));
            Call(s, "SetActorControl", boss, "test-silence", OriginalActorControlMask.Cast, 5d, true, true);
            WorldTick(w, 1); Call(s, "AdvanceBossInfernos");
            Assert.That(w.Snapshot().units.Single(u => u.rawcode == "n025").hidden, Is.True);
            Assert.That((bool)Call(s, "TryStartBossInfernoCast", boss, w.UnitState(boss).position), Is.False);
        }
        [TestCase(1, 406)] [TestCase(2, 403)] [TestCase(4, 401.5)]
        public void BossRainDividesInitialDamageButBurnsEachTargetForFifty(int count, double expected)
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(boss, paused: true); w.SetUnitState(1, paused: true, invulnerable: true);
            w.ForcePosition(1, new OriginalPoint(2000, 2000));
            for (int i = 0; i < count; i++)
                w.AddUnit(9000 + i, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 420, maxMana = 0, moveSpeed = 0, collisionRadius = 24 },
                    new OriginalPoint(100 + 55 * i, 1000));
            Call(s, "BeginBossRain", boss, 1, new OriginalPoint(100, 1000));
            WorldTick(w, 1.49); Call(s, "AdvanceBossRain"); Assert.That(w.UnitState(9000).health, Is.EqualTo(420));
            WorldTick(w, 8.01); Call(s, "AdvanceBossRain");
            for (int i = 0; i < count; i++) Assert.That(420 - w.UnitState(9000 + i).health, Is.EqualTo(expected).Within(.001));
        }
        [Test] public void GhostHealthThresholdStartsRainAndUnlocksBothNativeOrdersAfterTenSeconds()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true);
            w.ForcePosition(1, new OriginalPoint(2000, 2000));
            w.ApplyUnitDamage(boss, w.UnitState(boss).profile.maxHealth * .36);
            Tick(s, .95);
            Assert.That(w.UnitState(boss).paused, Is.True);
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A0QR"), Is.True);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(boss), "A101"), Is.False);
            Tick(s, 9.99); Assert.That(w.UnitState(boss).paused, Is.True);
            Tick(s, .01); Assert.That(w.UnitState(boss).paused, Is.False);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(boss), "A101"), Is.True);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(boss), "A04V"), Is.True);
        }
        [Test] public void MeteorUsesSixWarningsAtOnePointFiveSecondsAndCleansUpOnSeventhCallback()
        {
            var s = StartRound(10, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true); w.ForcePosition(1, new OriginalPoint(2000, 2000));
            Call(s, "BeginBossMeteorPhase", boss);
            Assert.That(w.UnitState(boss).paused, Is.False);
            int warnings = 0;
            for (int i = 0; i < 7; i++)
            {
                WorldTick(w, 1.5); Call(s, "AdvanceBossMeteors");
                warnings += s.Snapshot().effects.Count(e => e.abilityId == "h02L");
            }
            Assert.That(warnings, Is.EqualTo(6));
            var remaining = (System.Collections.Generic.Dictionary<int, int>)typeof(OriginalSession)
                .GetField("bossMeteorRemaining", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Assert.That(remaining[boss], Is.EqualTo(2));
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "h02L"), Is.False);
        }
        [Test] public void BossShieldHealsBeforeDamageAndRestoresAccumulatedDamageOnlyOnExpiry()
        {
            var s = StartRound(10, out var w); Tick(s, 5.05);
            int boss = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true); w.SetUnitState(boss, paused: true);
            Call(s, "BeginBossShield", boss, w.Clock);
            double maximum = w.UnitState(boss).profile.maxHealth;
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 100d, null);
            Assert.That(w.UnitState(boss).health, Is.EqualTo(maximum - 100));
            Call(s, "ApplyResolvedUnitHit", 1, 1, w.UnitState(boss), 60d, null);
            Assert.That(w.UnitState(boss).health, Is.EqualTo(maximum - 100));
            Assert.That((bool)Call(s, "CasterMagicImmune", w.UnitState(boss)), Is.True);
            WorldTick(w, 6.99); Call(s, "AdvanceBossShields"); Assert.That(w.UnitState(boss).health, Is.EqualTo(maximum - 100));
            w.Advance(.01); Call(s, "AdvanceBossShields"); Assert.That(w.UnitState(boss).health, Is.EqualTo(maximum));
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(boss), "A0V2"), Is.False);
        }
        [Test] public void FifthMegaPhaseAddAppearsInWorldWithoutItsRevivalAuraOrBossScaling()
        {
            var s = StartRound(25, out var w); Tick(s, 5.05);
            int count = s.Snapshot().enemies.Length;
            Call(s, "SpawnBossPhaseAddIfRequired");
            Assert.That(s.HaltReason, Is.Null);
            var add = s.Snapshot().enemies.Single(e => e.rawcode == "n01X");
            var actor = w.UnitState(OriginalWorld.EnemyEntityId(add.entityId));
            Assert.That(s.Snapshot().enemies.Length, Is.EqualTo(count + 1));
            Assert.That(add.sourceUserData, Is.EqualTo(1)); Assert.That(add.counted, Is.False);
            Assert.That((bool)Call(s, "CasterHasAbility", actor, "A11G"), Is.False);
            Assert.That((bool)Call(s, "CasterHasAbility", actor, "A11I"), Is.True);
            Assert.That((double)Call(s, "BossArmorBonus", actor.entityId), Is.Zero);
            Assert.That((double)Call(s, "BossAttackBonus", actor.entityId), Is.Zero);
        }
        [Test] public void QuadrantBoundariesAreInclusiveAndSharedCenterIsInAllFourRegions()
        {
            for (int q = 1; q <= 4; q++)
            {
                Assert.That(OriginalBossQuadrantRules.Contains(q, new OriginalPoint(0, -2720)), Is.True);
                Assert.That(OriginalBossQuadrantRules.Contains(q, OriginalBossQuadrantRules.Minimum(q)), Is.True);
                Assert.That(OriginalBossQuadrantRules.Contains(q, new OriginalPoint(900, -2700)), Is.False);
            }
            Assert.That(OriginalBossQuadrantRules.Contains(1, new OriginalPoint(400, -2100)), Is.True);
            Assert.That(OriginalBossQuadrantRules.Contains(2, new OriginalPoint(400, -2100)), Is.False);
        }
        [Test] public void QuadrantHealingUsesRawDamageIncludingInvulnerableVictimsAndAllDangerousBoundaryRegions()
        {
            var s = StartRound(15, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(id, new OriginalPoint(0, -2700)); w.SetUnitState(id, paused: true);
            w.ForcePosition(1, new OriginalPoint(0, -2720)); w.SetUnitState(1, paused: true, invulnerable: true);
            w.ApplyUnitDamage(id, 1000); double bossBefore = w.UnitState(id).health, heroBefore = w.UnitState(1).health;
            Call(s, "ResolveBossQuadrantPulse", id, 1, .04d);
            Assert.That(w.UnitState(1).health, Is.EqualTo(heroBefore));
            Assert.That(w.UnitState(id).health - bossBefore, Is.EqualTo(w.UnitState(1).profile.maxHealth * .04 * 3 * 1.5).Within(.001));
            w.ForcePosition(1, new OriginalPoint(400, -2100)); bossBefore = w.UnitState(id).health;
            Call(s, "ResolveBossQuadrantPulse", id, 1, .04d);
            Assert.That(w.UnitState(id).health, Is.EqualTo(bossBefore));
            w.ForcePosition(1, new OriginalPoint(-400, -2100)); w.SetUnitState(1, invulnerable: false);
            Call(s, "ResolveBossQuadrantPulse", id, 1, .04d);
            Assert.That(heroBefore - w.UnitState(1).health, Is.EqualTo(w.UnitState(1).profile.maxHealth * .04 * .8).Within(.001));
        }
        [Test] public void QuadrantsGrowForFiftyNineTicksThenExpireAfterElevenPulsesAndOneCleanupTick()
        {
            var s = StartRound(15, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(id, paused: true); w.SetUnitState(1, paused: true, invulnerable: true);
            w.ForcePosition(1, new OriginalPoint(1000, 1000));
            w.ApplyUnitDamage(id, w.UnitState(id).profile.maxHealth * .25); Tick(s, .95);
            Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Rectangle), Is.EqualTo(4));
            var codec = new OriginalUnitySessionCodec(); var snapshot = s.Snapshot();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                acknowledgedSequence = snapshot.players[0].acknowledgedSequence, snapshot = snapshot };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            Assert.That(decoded.snapshot.protocol, Is.EqualTo(OriginalSession.Protocol));
            Assert.That(decoded.snapshot.effects.Count(e => e.kind == OriginalVisualEffectKind.Rectangle), Is.EqualTo(4));
            Tick(s, 1.74); Assert.That(s.Snapshot().effects.First(e => e.kind == OriginalVisualEffectKind.Rectangle).progress, Is.LessThan(1));
            Tick(s, .03); Assert.That(s.Snapshot().effects.First(e => e.kind == OriginalVisualEffectKind.Rectangle).progress, Is.EqualTo(1));
            Tick(s, 11.99); Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Rectangle), Is.EqualTo(4));
            Tick(s, .01); Assert.That(s.Snapshot().effects.Any(e => e.kind == OriginalVisualEffectKind.Rectangle), Is.False);
            var remaining = (System.Collections.Generic.Dictionary<int, int>)typeof(OriginalSession)
                .GetField("bossQuadrantsRemaining", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Assert.That(remaining[id], Is.EqualTo(2));
        }
        [Test] public void OriginalMirrorBossDispelsAtAccumulatedDamageThresholdAndDiscardsExcess()
        {
            var s = StartRound(15, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            void Debuff() { Call(s, "OrderArcherNativeHelper", 1, id, "A166", 1); w.Advance(.02); Call(s, "AdvanceArcherDebuffs"); }
            Debuff(); Assert.That((double)Call(s, "ArcherDebuffArmorDelta", id), Is.LessThan(0));
            Call(s, "ObserveBossNativeDamage", w.UnitState(id), 3499d);
            Assert.That((double)Call(s, "ArcherDebuffArmorDelta", id), Is.LessThan(0));
            Call(s, "ObserveBossNativeDamage", w.UnitState(id), 100d);
            Assert.That((double)Call(s, "ArcherDebuffArmorDelta", id), Is.Zero);
            Debuff(); Call(s, "ObserveBossNativeDamage", w.UnitState(id), 3401d);
            Assert.That((double)Call(s, "ArcherDebuffArmorDelta", id), Is.LessThan(0));
        }
        [Test] public void WindBossAutomaticallyEntersThresholdPhaseDelaysBurstsAndCleansUpAfterFourteenCallbacks()
        {
            var s = StartRound(20, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Assert.That(w.UnitState(id).rawcode, Is.EqualTo("u00G"));
            w.SetUnitState(1, paused: true, invulnerable: true);
            w.ApplyUnitDamage(id, w.UnitState(id).profile.maxHealth * .25);
            Tick(s, .95); Assert.That(w.UnitState(id).paused, Is.True);
            Assert.That(w.UnitState(id).position.y, Is.EqualTo(-2700));
            double entryHealth = w.UnitState(id).health;
            Tick(s, 5.95);
            Assert.That(s.Snapshot().effects.Count(e => e.abilityId == "A0LH"), Is.Zero);
            w.ApplyUnitDamage(id, 20); Tick(s, .05);
            Assert.That(w.UnitState(id).health, Is.EqualTo(entryHealth).Within(.001));
            Assert.That(s.Snapshot().effects.Count(e => e.abilityId == "A0LH"), Is.EqualTo(8));
            Tick(s, 14.95); Assert.That(w.UnitState(id).paused, Is.True);
            Tick(s, .05); Assert.That(w.UnitState(id).paused, Is.False);
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A0LH"), Is.True);
            var remaining = (System.Collections.Generic.Dictionary<int, int>)typeof(OriginalSession)
                .GetField("bossWindRemaining", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Assert.That(remaining[id], Is.EqualTo(3));
            Tick(s, 3.2); Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A0LH"), Is.False);
        }
        [Test] public void MegaScalingUsesPlayerBonusesAndFinalAddsHaveTheirSourceHealthFloor()
        {
            var five = OriginalBossRules.Scaling("n00K", 3, 5, false, 2500);
            Assert.That(five.attack, Is.EqualTo(90)); Assert.That(five.armor, Is.EqualTo(15)); Assert.That(five.health, Is.EqualTo(1050));
            Assert.That(OriginalBossRules.Scaling("n00Z", 2, 25, true, 2000).health, Is.EqualTo(7000));
            Assert.That(OriginalBossRules.Scaling("n009", 1, 30, true, 1300).health, Is.EqualTo(700));
            Assert.That(OriginalBossRules.Scaling("n009", 1, 30, true, 3001).health, Is.EqualTo(1500));
            Assert.Throws<InvalidOperationException>(() => OriginalBossRules.Scaling("n009", 1, 5, false, 1300));
        }
        [Test] public void WindSweepsAfterMovementAndHitsEachVictimOnlyOnce()
        {
            var s = StartRound(20, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true);
            Call(s, "BeginBossWindPhase", id); Tick(s, 6);
            var winds = (System.Collections.IList)typeof(OriginalSession).GetField("bossWinds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Assert.That(winds.Count, Is.EqualTo(8));
            while (winds.Count > 1) winds.RemoveAt(winds.Count - 1); // Isolate one actual emitted projectile.
            var rule = (OriginalBossWindRules)winds[0].GetType().GetField("rule", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(winds[0]);
            var point = rule.Position;
            w.ForcePosition(1, new OriginalPoint(point.x + 125 * Math.Cos(rule.Heading), point.y + 125 * Math.Sin(rule.Heading)));
            w.SetUnitState(1, invulnerable: false); double before = w.UnitState(1).health;
            w.Advance(.04); Call(s, "AdvanceBossWind");
            double after = w.UnitState(1).health;
            Assert.That(before - after, Is.EqualTo(w.UnitState(1).profile.maxHealth * .18 * .8).Within(.001));
            for (int i = 0; i < 4; i++) { w.Advance(.04); Call(s, "AdvanceBossWind"); }
            Assert.That(w.UnitState(1).health, Is.EqualTo(after));
        }
        [Test] public void ActualRoundFiveActorReceivesHealthArmorAndAttackBonuses()
        {
            var s = StartRound(5, out var w); var enemy = s.Snapshot().enemies.Single(); int id = OriginalWorld.EnemyEntityId(enemy.entityId);
            var observed = (OriginalObservedCatalog)typeof(OriginalSession).GetField("observed", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            Assert.That(w.UnitState(id).profile.maxHealth, Is.EqualTo(observed.Unit(enemy.rawcode).RequireMaxHP() + 350));
            Assert.That(w.UnitState(id).health, Is.EqualTo(w.UnitState(id).profile.maxHealth));
            Assert.That((double)Call(s, "BossAttackBonus", id), Is.EqualTo(30));
            Assert.That((double)Call(s, "BossArmorBonus", id), Is.EqualTo(5));
            Assert.That(w.UnitState(id).paused, Is.True); Tick(s, 5.05); Assert.That(w.UnitState(id).paused, Is.False);
        }
        [Test] public void OrnUsesObservedLevelFiftyAttributesBeforePerPlayerScaling()
        {
            var s = StartRound(30, out var w); int id = OriginalWorld.EnemyEntityId(Match(s).FinalBossEntityId);
            var boss = w.UnitState(id);
            Assert.That(boss.profile.maxHealth, Is.EqualTo(33000));
            Assert.That(boss.profile.maxMana, Is.EqualTo(2500));
            Assert.That(boss.profile.moveSpeed, Is.EqualTo(390).Within(.001));
            Assert.That((double)Call(s, "BossPrimaryDamageBonus", boss), Is.EqualTo(375));
            Assert.That((double)Call(s, "BossAgilityAttackSpeedBonus", boss), Is.EqualTo(2.5));
            var catalog = (OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            Assert.That((double)Call(s, "EnemyArmor", catalog.Unit("O006"), id), Is.EqualTo(92));
        }
        [Test] public void ActualBossDamageStartsIntermissionHidesProtectsAndRegeneratesOrn()
        {
            var s = StartRound(30, out var w); Tick(s, 5.05); w.SetUnitState(1, paused: true, invulnerable: true);
            int id = OriginalWorld.EnemyEntityId(Match(s).FinalBossEntityId); var boss = w.UnitState(id);
            w.ApplyUnitDamage(id, boss.profile.maxHealth * .26); Tick(s, 1.05);
            Assert.That(s.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.FinalIntermission));
            boss = w.UnitState(id); Assert.That(boss.hidden && boss.paused && boss.invulnerable, Is.True);
            Assert.That(boss.position.x, Is.Zero); Assert.That(boss.position.y, Is.EqualTo(-2700));
            Assert.That(w.ApplyUnitDamage(id, 100), Is.False);
            double before = boss.health; Tick(s, .5); Assert.That(w.UnitState(id).health, Is.EqualTo(before + 15).Within(1e-7));
        }
        [Test] public void ChargeWorldTimerTelegraphsMovesAndRestoresBossPathing()
        {
            var s = StartRound(5, out var w);
            w.ForcePosition(1, new OriginalPoint(135, 1000)); w.SetUnitState(1, paused: true, invulnerable: true);
            Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(id, new OriginalPoint(0, -2700)); w.Stop(id);
            Call(s, "BeginBossCharge", id);
            Assert.That(w.UnitState(id).paused && w.UnitState(id).pathingDisabled, Is.True);
            Tick(s, 1.8); Assert.That(w.UnitState(id).position.x, Is.Zero);
            Tick(s, .84); Assert.That(w.UnitState(id).position.x, Is.EqualTo(588).Within(.001));
            Assert.That(w.UnitState(id).paused, Is.True);
            Tick(s, .03); Assert.That(w.UnitState(id).paused || w.UnitState(id).pathingDisabled, Is.False);
        }
        [Test] public void AbilityVisualsAreDetachedNetworkSnapshotsAndRejectMalformedDrawingFields()
        {
            var s = StartRound(5, out var w); int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "BeginBossCharge", id);
            var codec = new OriginalUnitySessionCodec();
            OriginalNetworkResponse Response() { var view = s.Snapshot(); return new OriginalNetworkResponse {
                kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1, snapshot = view,
                acknowledgedSequence = view.players[0].acknowledgedSequence }; }
            var response = Response();
            Assert.That(response.snapshot.effects.Single().kind, Is.EqualTo(OriginalVisualEffectKind.Beam));
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            Assert.That(decoded.snapshot.effects[0].abilityId, Is.EqualTo("A101"));
            response.snapshot.effects[0].position = new OriginalPoint(777, 777);
            Assert.That(s.Snapshot().effects[0].position.x, Is.EqualTo(w.UnitState(id).position.x));
            foreach (var mutate in new Action<OriginalVisualEffectView>[] {
                e => e.kind = (OriginalVisualEffectKind)99, e => e.abilityId = "bad",
                e => e.sourceEntityId = -1, e => e.radius = -1, e => e.radius = 4097,
                e => e.progress = 1.01, e => e.variant = 17, e => e.position.x = 1048577,
                e => e.end.y = -1048577 })
            {
                response = Response(); mutate(response.snapshot.effects[0]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
            response = Response(); response.snapshot.effects = new OriginalVisualEffectView[2049];
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }
        [Test] public void BossAiCastsChargeAfterNativeLatencyAndSpendsManaAtEffect()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true);
            // Exercise the real Ehv order selection, then isolate the native
            // effect callback from regeneration and unrelated weapon timers.
            Call(s, "SelectBossOrders");
            double mana = w.UnitState(id).mana;
            Assert.That((bool)Call(s, "BossControlsActor", id), Is.True);
            Assert.That(w.UnitState(id).paused, Is.False);
            for (int i = 0; i < 9; i++) w.Advance(.05);
            w.Advance(.04); Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(id).mana, Is.EqualTo(mana));
            Assert.That(s.Snapshot().effects.Any(e => e.kind == OriginalVisualEffectKind.Beam), Is.False);
            w.Advance(.01); Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(id).mana, Is.EqualTo(mana - 50));
            Assert.That(w.UnitState(id).paused && w.UnitState(id).pathingDisabled, Is.True);
            Assert.That(s.Snapshot().effects.Single().abilityId, Is.EqualTo("A101"));
        }
        [TestCase(5, "TryStartBossChargeCast", "A101")]
        [TestCase(5, "TryStartBossRainCast", "A04V")]
        [TestCase(10, "TryStartBossShieldCast", "A0X9")]
        public void SilenceCancelsPendingBossCastAndRejectsRestart(int round, string method, string ability)
        {
            var s = StartRound(round, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "ApplyUnitAbilityOverlay", id, new[] { ability }, null);
            var args = method == "TryStartBossRainCast" ? new object[] { id, w.UnitState(id).position } : new object[] { id };
            Assert.That((bool)Call(s, method, args), Is.True);
            double mana = w.UnitState(id).mana;
            Call(s, "SetActorControl", id, "test-silence", OriginalActorControlMask.Cast, 5d, true, true);
            Assert.That((bool)Call(s, "BossControlsActor", id), Is.False);
            Assert.That((bool)Call(s, method, args), Is.False);
            WorldTick(w, .5); Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(id).mana, Is.EqualTo(mana));
            Call(s, "ClearActorControl", id, "test-silence");
            Assert.That((bool)Call(s, method, args), Is.True);
        }
        [Test] public void AcceptedOrderCancelsBossCastBeforeDebitAndCooldown()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "SelectBossOrders"); double mana = w.UnitState(id).mana;
            Call(s, "CasterMoveOrder", id, new OriginalPoint(100, -2700));
            Assert.That((bool)Call(s, "BossControlsActor", id), Is.False);
            for (int i = 0; i < 10; i++) w.Advance(.05);
            Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(id).mana, Is.EqualTo(mana));
            Assert.That(w.UnitState(id).paused, Is.False);
            Assert.That((bool)Call(s, "TryStartBossChargeCast", id), Is.True);
        }
        [Test] public void ScriptedBarrelAttackInterruptsNativeBossWindup()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            var options = (OriginalMatchOptions)typeof(OriginalSession).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            options.defensiveBarrels = OriginalDefensiveBarrels.Attackable;
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.ForcePosition(id, new OriginalPoint(0, -2700));
            w.AddDoodad(8765, "LTbr", new OriginalPoint(90, -2700), 100, 100);
            Call(s, "SelectBossOrders"); double mana = w.UnitState(id).mana;
            Assert.That((bool)Call(s, "BossControlsActor", id), Is.True);
            typeof(OriginalSession).GetField("nextBarrelOrder", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(s, w.Clock);
            Call(s, "AdvanceWorldAi");
            Assert.That(w.UnitState(id).targetKind, Is.EqualTo(OriginalWorldTargetKind.Doodad));
            Assert.That((bool)Call(s, "BossControlsActor", id), Is.False);
            for (int i = 0; i < 10; i++) w.Advance(.05);
            Call(s, "AdvanceBossNativeCasts");
            Assert.That(w.UnitState(id).mana, Is.EqualTo(mana));
            Assert.That(w.UnitState(id).paused, Is.False);
        }
        [Test] public void GhostPhaseRestoresCapturedLifeEmitsFourBurstsAndLeavesLastGhostsAfterCleanup()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            w.ForcePosition(1, new OriginalPoint(135, 1000)); w.SetUnitState(1, paused: true, invulnerable: true);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            double captured = w.UnitState(id).health;
            Call(s, "BeginBossGhostPhase", id, 0d);
            Assert.That(w.UnitState(id).paused, Is.True);
            Assert.That((double)Call(s, "BossArmorBonus", id), Is.EqualTo(9005));
            Assert.That(w.UnitState(id).position.y, Is.EqualTo(-2680));
            Tick(s, 1.99); Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Ghost), Is.Zero);
            w.ApplyUnitDamage(id, 100); Tick(s, .01);
            Assert.That(w.UnitState(id).health, Is.EqualTo(captured).Within(1e-7));
            Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Ghost), Is.EqualTo(6));
            Tick(s, 2); Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Ghost), Is.EqualTo(12));
            Tick(s, 6); Assert.That(w.UnitState(id).paused, Is.False);
            Assert.That((double)Call(s, "BossArmorBonus", id), Is.EqualTo(5));
            Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Ghost), Is.EqualTo(6));
            Tick(s, .93); Assert.That(s.Snapshot().effects.Count(e => e.kind == OriginalVisualEffectKind.Ghost), Is.Zero);
        }
        [Test] public void GhostHitsAtOldPositionOnlyOnceAndUsesTargetsMaximumHealth()
        {
            var s = StartRound(5, out var w); Tick(s, 5.05);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            w.SetUnitState(1, paused: true, invulnerable: true);
            Call(s, "BeginBossGhostPhase", id, 0d); Tick(s, 2);
            var ghost = s.Snapshot().effects.First(e => e.kind == OriginalVisualEffectKind.Ghost);
            w.ForcePosition(1, new OriginalPoint(ghost.position.x - 109, ghost.position.y)); w.SetUnitState(1, invulnerable: false);
            double hp = w.UnitState(1).health, maximum = w.UnitState(1).profile.maxHealth;
            // At109 behind spawn, the hero is outside the new position's
            // radius after the22WC movement. The first tick must sweep old position; a
            // paused hero still regenerates, so isolate the callback directly.
            w.Advance(.04); Call(s, "AdvanceBossGhosts");
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - maximum * .15 * .8).Within(.0001));
            hp = w.UnitState(1).health;
            w.ForcePosition(1, s.Snapshot().effects.First(e => e.kind == OriginalVisualEffectKind.Ghost).position);
            w.Advance(.04); Call(s, "AdvanceBossGhosts");
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp).Within(.0001));
        }
        [TestCase(5)] [TestCase(25)]
        public void BossPhaseCleansesPoisonWithoutReapplyingItsSlowedProfile(int round)
        {
            var s = StartRound(round, out var w);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            double speed = w.UnitState(id).profile.moveSpeed;
            var type = typeof(OriginalSession).GetNestedType("Projectile", BindingFlags.NonPublic);
            var shot = Activator.CreateInstance(type, true);
            type.GetField("poisonAbility", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, "A0TC");
            type.GetField("attacker", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, 1);
            type.GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, 1);
            Call(s, "ApplyPoison", shot, w.UnitState(id));
            Assert.That(w.UnitState(id).profile.moveSpeed, Is.LessThan(speed));
            Call(s, round == 5 ? "BeginBossGhostPhase" : "BeginBossBarrage", id, 0d);
            Assert.That(w.UnitState(id).profile.moveSpeed, Is.EqualTo(speed));
            Assert.That((double)Call(s, "PoisonMovementMultiplier", id), Is.EqualTo(1));
        }
        [Test] public void BossPhaseDispelAlsoRemovesTheNativeShieldCripple()
        {
            var s = StartRound(5, out var w); int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "OrderShieldCripple", 1, id);
            Assert.That((double)Call(s, "ApplyCrippleWeaponDamage", id, 100d, 20d), Is.EqualTo(70));
            Call(s, "BeginBossGhostPhase", id, 0d);
            Assert.That((double)Call(s, "ApplyCrippleWeaponDamage", id, 100d, 20d), Is.EqualTo(120));
        }
        [Test] public void GhostPhaseSpellResistanceDoesNotBecomeUniversalImmunity()
        {
            var s = StartRound(5, out var w);
            int id = OriginalWorld.EnemyEntityId(s.Snapshot().enemies.Single().entityId);
            Call(s, "BeginBossGhostPhase", id, 0d);
            double health = w.UnitState(id).health;
            Call(s, "ApplyTriggeredHit", 1, 1, w.UnitState(id), 40d, OriginalTriggeredDamageMode.SpellNormal);
            Call(s, "ApplyTriggeredHit", 1, 1, w.UnitState(id), 40d, OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(id).health, Is.EqualTo(health));
            Call(s, "ApplyTriggeredHit", 1, 1, w.UnitState(id), 40d, OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(w.UnitState(id).health, Is.EqualTo(health - 40).Within(.0001));
        }
        [Test] public void LastAddDeathAfterCompleteSeriesRevealsAndRelocatesBoss()
        {
            var s = StartRound(30, out var w); Tick(s, 5.05); w.SetUnitState(1, paused: true, invulnerable: true);
            int id = OriginalWorld.EnemyEntityId(Match(s).FinalBossEntityId);
            w.ApplyUnitDamage(id, w.UnitState(id).profile.maxHealth * .26); Tick(s, 54);
            Assert.That(s.Snapshot().enemies.Count(e => e.finalAdd), Is.EqualTo(16));
            foreach (var enemy in s.Snapshot().enemies.Where(e => e.finalAdd))
            { w.ForceUnitDeath(OriginalWorld.EnemyEntityId(enemy.entityId)); Assert.That(s.ReportEnemyKilled(enemy.entityId, false), Is.True); s.DrainEvents(); }
            Assert.That(s.HaltReason, Is.Null); Assert.That(Match(s).FinalStage, Is.EqualTo(1));
            Assert.That(s.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.Combat));
            var boss = w.UnitState(id); Assert.That(boss.hidden || boss.invulnerable || boss.paused, Is.False);
            Assert.That(boss.position.x, Is.InRange(-576, 576)); Assert.That(boss.position.y, Is.InRange(-3328, -2112));
            Assert.That(boss.position.x != 0 || boss.position.y != -2700, Is.True);
        }
    }
}

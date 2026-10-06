using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionCasterTests
    {
        static object Call(object target, string name, params object[] args) => target.GetType()
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args = new object[] { "H008", null };
            var session = (OriginalSession)typeof(OriginalSessionRegenerationTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);
            world = (OriginalWorld)args[1];
            world.AddUnit(2001, 0, "n05M", new OriginalWorldUnitProfile { maxHealth = 5000, maxMana = 500, collisionRadius = 24, moveSpeed = 250 }, new OriginalPoint(700, 1000));
            return session;
        }
        static void Begin(OriginalSession session, string id, OriginalPoint point) => Call(session, "BeginCasterEffect", 2001, 0,
            new OriginalCasterRules(Field<OriginalCombatCatalog>(session, "combatCatalog"), id), point, Field<OriginalWorld>(session, "world").Clock);
        static void Tick(OriginalSession session, OriginalWorld world, double seconds)
        {
            while (seconds > 1e-9)
            { double step = Math.Min(.05, seconds); world.Advance(step); Call(session, "AdvanceCasters"); seconds -= step; }
        }

        [Test] public void NativeTypeTokensAreCaseInsensitiveWithoutPromotingUndeclaredTypes()
        {
            var s=Create(out var w);
            Assert.That(Call(s,"CasterHasType",new OriginalWorldUnitView{rawcode="hhou"},"mechanical"),Is.True);
            Assert.That(Call(s,"CasterHasType",new OriginalWorldUnitView{rawcode="hhou"},"structure"),Is.True);
            Assert.That(Call(s,"CasterHasType",new OriginalWorldUnitView{rawcode="hfoo"},"structure"),Is.False);
            Assert.That(Call(s,"CasterHasType",new OriginalWorldUnitView{rawcode="n02K"},"mechanical"),Is.False);
            Assert.That(Call(s,"CasterHasType",new OriginalWorldUnitView{rawcode="hfoo"},"mechanical"),Is.False);
        }
        [Test] public void TotemWarnsThenPulsesFourteenTimesAndRemovesWithoutDeath()
        {
            var s=Create(out var w); var hero=w.UnitState(1);
            w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=10000,maxMana=hero.profile.maxMana,moveSpeed=250,collisionRadius=24},10000,0);
            Begin(s,"A123",hero.position); Tick(s,w,.95);
            Assert.That(w.Snapshot().units.Any(u=>u.rawcode=="n06L"),Is.False);
            Tick(s,w,.05); var totem=w.Snapshot().units.Single(u=>u.rawcode=="n06L");
            Assert.That(totem.health,Is.EqualTo(20)); Assert.That(totem.paused,Is.True);
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            w.ForceUnitDeath(2001); Tick(s,w,14);
            Assert.That(w.UnitState(1).health,Is.EqualTo(7900)); Assert.That(Field<bool>(s,"casterGate"),Is.False);
            Tick(s,w,1); Assert.That(w.UnitState(totem.entityId),Is.Null); Assert.That(Field<bool>(s,"casterGate"),Is.True);
        }

        [Test] public void TotemDamageHookCountsPositiveHitsAndReleasesGateAfterDeath()
        {
            var s=Create(out var w); Begin(s,"A123",w.UnitState(1).position); Tick(s,w,1);
            var totem=w.Snapshot().units.Single(u=>u.rawcode=="n06L");
            Assert.That(Call(s,"ObserveCasterTotemDamage",1,totem,0d),Is.False);
            for(int i=0;i<9;i++)
            {
                Assert.That(Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(totem.entityId),40d,null),Is.True);
                Assert.That(w.UnitState(totem.entityId).health,Is.EqualTo(18-i*2));
                Tick(s,w,.05);
            }
            Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(totem.entityId),40d,null);
            Assert.That(w.UnitState(totem.entityId).health,Is.Zero);
            Tick(s,w,.55); Assert.That(Field<bool>(s,"casterGate"),Is.True);
        }

        [Test] public void CasterAuraLifetimeIsElevenSecondsAndMembershipFollowsPosition()
        {
            var s=Create(out var w); var center=w.UnitState(1).position;
            Begin(s,"A1DC",center); Tick(s,w,1);
            Assert.That(Call(s,"CasterAuraMovementBonus",1),Is.EqualTo(-.5));
            w.ForcePosition(1,new OriginalPoint(center.x+401,center.y)); Tick(s,w,.05);
            Assert.That(Call(s,"CasterAuraMovementBonus",1),Is.EqualTo(0d));
            w.ForcePosition(1,center); Tick(s,w,10.9);
            Assert.That(Call(s,"CasterAuraMovementBonus",1),Is.EqualTo(-.5));
            Tick(s,w,.05); Assert.That(Call(s,"CasterAuraMovementBonus",1),Is.EqualTo(0d));
            Assert.That(Field<bool>(s,"casterGate"),Is.True);
        }

        [Test] public void SpellCursePunishesActualHeroEffectButExcludedNativeOrdersRemainFree()
        {
            var s=Create(out var w); var hero=w.UnitState(1);
            w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=10000,maxMana=hero.profile.maxMana,moveSpeed=250,collisionRadius=24},10000,0);
            Begin(s,"A1DE",hero.position); Tick(s,w,1);
            Call(s,"NotifyNativeSpellEffect",1,"A05M"); Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            Call(s,"NotifyNativeSpellEffect",1,"A0E6"); Assert.That(w.UnitState(1).health,Is.EqualTo(9040));
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A0K4"},Array.Empty<string>());
            Call(s,"NotifyNativeSpellEffect",1,"A0E6"); Assert.That(w.UnitState(1).health,Is.EqualTo(9040));
            Tick(s,w,11); Call(s,"NotifyNativeSpellEffect",1,"A0E6"); Assert.That(w.UnitState(1).health,Is.EqualTo(9040));
        }

        [Test] public void ReverseAuraReflectsOnlyPointMovementAndAttackAroundCurrentHero()
        {
            var s=Create(out var w); var center=w.UnitState(1).position; Begin(s,"A1DF",center); Tick(s,w,1);
            var target=new OriginalPoint(center.x+100,center.y-25);
            var reflected=(OriginalPoint)Call(s,"CasterAuraPointOrder",1,851986,target);
            Assert.That(reflected.x,Is.EqualTo(center.x-100)); Assert.That(reflected.y,Is.EqualTo(center.y+25));
            var cast=(OriginalPoint)Call(s,"CasterAuraPointOrder",1,852123,target);
            Assert.That(cast.x,Is.EqualTo(target.x));
            var creep=(OriginalPoint)Call(s,"CasterAuraPointOrder",2001,851986,target);
            Assert.That(creep.x,Is.EqualTo(target.x));
        }

        [Test] public void FieldSlowRestoresNonheroBaselineAndDoesNotMultiplyAgainOnReentry()
        {
            var s=Create(out var w); var center=w.UnitState(1).position;
            const int summon=100000010;
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=summon,ownerSlot=1,sourceHeroEntityId=1,
                rawcode="hfoo",profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=0,moveSpeed=300,collisionRadius=16},
                health=1000,position=new OriginalPoint(center.x+80,center.y)}}),Is.True);
            Begin(s,"A1DC",center); Tick(s,w,1);
            Assert.That(w.UnitState(summon).profile.moveSpeed,Is.EqualTo(150));
            w.ForcePosition(summon,new OriginalPoint(center.x+450,center.y)); Tick(s,w,.05);
            Assert.That(w.UnitState(summon).profile.moveSpeed,Is.EqualTo(300));
            w.ForcePosition(summon,new OriginalPoint(center.x+80,center.y)); Tick(s,w,.05);
            Assert.That(w.UnitState(summon).profile.moveSpeed,Is.EqualTo(150));
            Tick(s,w,10.9);Assert.That(w.UnitState(summon).profile.moveSpeed,Is.EqualTo(300));
        }

        [Test] public void FieldDeclarationConflictCannotPublishWarningOrSpendCasterMana()
        {
            var s=Create(out var w); var catalog=Field<OriginalCombatCatalog>(s,"combatCatalog");
            catalog.Ability("S002").fields.Single(f=>f.key=="DataA1").number=-.9;
            Assert.Throws<TargetInvocationException>(()=>Begin(s,"A1DC",w.UnitState(1).position));
            Assert.That(s.CasterTelegraphs.Length,Is.Zero);Assert.That(w.UnitState(2001).mana,Is.EqualTo(500));
        }

        [Test] public void EnemyImageWaitsForNativeHelperEffectAndUsesDetachedOwnerAndCombat()
        {
            var s = Create(out var w); var hero = w.UnitState(1); int enemies = Field<OriginalMatch>(s, "match").Enemies.Count;
            Begin(s, "A0ZA", hero.position); Tick(s, w, 1.6);
            Assert.That(w.Snapshot().units.Count(u => u.kind == OriginalWorldUnitKind.Illusion), Is.Zero);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
            Tick(s, w, .05);
            var copy = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            Assert.That(copy.ownerSlot, Is.Zero); Assert.That(copy.copySourceEntityId, Is.EqualTo(1));
            Assert.That(copy.imageFactory, Is.EqualTo(OriginalImageFactory.EnemyWand));
            Assert.That(copy.profile.maxHealth, Is.EqualTo(hero.profile.maxHealth));
            Assert.That(Call(s, "IsNativeHeroPredicate", copy), Is.False);
            Assert.That(Call(s, "ImageOutgoingDamage", copy.entityId, 40d), Is.EqualTo(60d));
            Assert.That(Call(s, "ImageIncomingDamage", copy.entityId, 40d), Is.EqualTo(40d));
            Assert.That(Field<OriginalMatch>(s, "match").Enemies.Count, Is.EqualTo(enemies));
            w.ForceUnitDeath(1);
            Assert.DoesNotThrow(() => Call(s, "CombatStatsFor", w.UnitState(copy.entityId)));
        }

        [Test] public void BossMirrorCapturesNativeBaseAndAppliesArenaScalingOnce()
        {
            var s=Create(out var w); var profile=new OriginalWorldUnitProfile { maxHealth=37000,maxMana=6000,moveSpeed=250,collisionRadius=32 };
            w.AddUnit(3001,0,"n017",profile,new OriginalPoint(0,-2688));
            w.UpdateProfile(3001,profile,18000,4000);
            Assert.That(Call(s,"TryStartBossMirrorCast",3001),Is.True);
            Tick(s,w,.70); Assert.That(w.UnitState(3001).mana,Is.EqualTo(4000));
            Tick(s,w,.05); Assert.That(w.UnitState(3001).hidden,Is.True); Assert.That(w.UnitState(3001).mana,Is.EqualTo(3750));
            Tick(s,w,.5);
            var copies=w.Snapshot().units.Where(u=>u.imageFactory==OriginalImageFactory.BossMirror).ToArray();
            Assert.That(copies.Length,Is.EqualTo(2)); Assert.That(w.UnitState(3001).hidden,Is.False);
            foreach(var image in copies)
            {
                Assert.That(image.profile.maxHealth,Is.EqualTo(37000)); Assert.That(image.health,Is.EqualTo(18000));
                Assert.That(image.mana,Is.EqualTo(3750)); Assert.That(Call(s,"HasBossImageDamageWatch",image.entityId),Is.True);
                object stats=Call(s,"CombatStatsFor",image);
                Assert.That(stats.GetType().GetField("armor",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(stats),Is.EqualTo(90d));
                Assert.That(stats.GetType().GetField("itemAttackDamageBonus",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(stats),Is.EqualTo(100d));
                Assert.That(Call(s,"IsNativeHeroPredicate",image),Is.False);
            }
            Assert.That(Call(s,"TryStartBossMirrorCast",3001),Is.False);
            w.RemoveUnit(3001);
            Assert.DoesNotThrow(()=>Call(s,"CombatStatsFor",w.UnitState(copies[0].entityId)));
        }

        [Test] public void BossMirrorScriptDrainsFixedArenaCenterEvenAfterCasterDeath()
        {
            var s=Create(out var w); w.ForcePosition(1,new OriginalPoint(0,-2688));
            var hero=w.UnitState(1); w.UpdateProfile(1,hero.profile,hero.health,hero.profile.maxMana);
            w.AddUnit(3001,0,"n017",new OriginalWorldUnitProfile{maxHealth=35500,maxMana=6000,moveSpeed=250,collisionRadius=32},new OriginalPoint(1800,-2688));
            Assert.That(Call(s,"TryStartBossMirrorCast",3001),Is.True); Tick(s,w,.75);
            w.ForceUnitDeath(3001); Tick(s,w,.65);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.profile.maxMana));
            Tick(s,w,.05); Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.profile.maxMana*.88).Within(1e-7));
            Assert.That(w.Snapshot().units.Any(u=>u.imageFactory==OriginalImageFactory.BossMirror),Is.False);
        }

        [Test] public void BossImageArenaReentryResetsBonusesAndCopiesManaWithoutAddingLife()
        {
            var s=Create(out var w); var profile=new OriginalWorldUnitProfile{maxHealth=37000,maxMana=6000,moveSpeed=250,collisionRadius=32};
            w.AddUnit(3001,0,"n017",profile,new OriginalPoint(1800,-2688));
            Assert.That(Call(s,"TryStartBossMirrorCast",3001),Is.True); Tick(s,w,1.25);
            var image=w.Snapshot().units.First(u=>u.imageFactory==OriginalImageFactory.BossMirror);
            Assert.That(Call(s,"HasBossImageDamageWatch",image.entityId),Is.False);
            w.UpdateProfile(3001,profile,10000,1234); w.ForcePosition(image.entityId,new OriginalPoint(0,-2688)); Tick(s,w,.05);
            Assert.That(w.UnitState(image.entityId).mana,Is.EqualTo(1234)); Assert.That(w.UnitState(image.entityId).profile.maxHealth,Is.EqualTo(37000));
            Assert.That(Call(s,"HasBossImageDamageWatch",image.entityId),Is.True);
            w.ForcePosition(image.entityId,new OriginalPoint(900,-2688)); Tick(s,w,.05);
            w.UpdateProfile(3001,profile,10000,777); w.ForcePosition(image.entityId,new OriginalPoint(0,-2688)); Tick(s,w,.05);
            var live=w.UnitState(image.entityId); var stats=Call(s,"CombatStatsFor",live);
            Assert.That(live.mana,Is.EqualTo(777)); Assert.That(live.health,Is.EqualTo(37000));
            Assert.That(stats.GetType().GetField("armor",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(stats),Is.EqualTo(90d));
            Assert.That(stats.GetType().GetField("itemAttackDamageBonus",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(stats),Is.EqualTo(100d));
        }

        [Test] public void BossMirrorInterruptBeforeEffectPreservesResourcesAndDoesNotDrainMana()
        {
            var s=Create(out var w); w.ForcePosition(1,new OriginalPoint(0,-2688));
            w.AddUnit(3001,0,"n017",new OriginalWorldUnitProfile{maxHealth=35500,maxMana=6000,moveSpeed=250,collisionRadius=32},new OriginalPoint(100,-2688));
            Assert.That(Call(s,"TryStartBossMirrorCast",3001),Is.True); Tick(s,w,.7);
            Call(s,"CancelBossImageCastOnOrder",3001); double mana=w.UnitState(1).mana; Tick(s,w,1);
            Assert.That(w.UnitState(3001).mana,Is.EqualTo(6000)); Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));
            Assert.That(w.Snapshot().units.Any(u=>u.imageFactory==OriginalImageFactory.BossMirror),Is.False);
        }

        [Test] public void RankTwoEnemyImageUsesQuarterIncomingAndDoesNotCopyAnotherImage()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            Begin(s, "A120", hero.position); Tick(s, w, 1.05);
            var first = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            Assert.That(Call(s, "ImageOutgoingDamage", first.entityId, 40d), Is.EqualTo(70d));
            Assert.That(Call(s, "ImageIncomingDamage", first.entityId, 40d), Is.EqualTo(10d));
            Begin(s, "A0ZA", hero.position); Tick(s, w, 1.65);
            Assert.That(w.Snapshot().units.Count(u => u.kind == OriginalWorldUnitKind.Illusion), Is.EqualTo(2));
            Assert.That(w.Snapshot().units.Where(u => u.kind == OriginalWorldUnitKind.Illusion).All(u => u.copySourceEntityId == 1), Is.True);
        }

        [Test] public void EnemyImageIgnoresImmuneOrDeadDonorAndRejectsConflictingFactoryBeforeWarning()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            Call(s, "ApplyUnitAbilityOverlay", 1, new[] { "Amim" }, Array.Empty<string>());
            Begin(s, "A0ZA", hero.position); Tick(s, w, 1.7);
            Assert.That(w.Snapshot().units.Count(u => u.kind == OriginalWorldUnitKind.Illusion), Is.Zero);
            Call(s, "ApplyUnitAbilityOverlay", 1, Array.Empty<string>(), new[] { "Amim" });
            Begin(s, "A0ZA", hero.position); Tick(s, w, 1.6); w.ForceUnitDeath(1); Tick(s, w, .05);
            Assert.That(w.Snapshot().units.Count(u => u.kind == OriginalWorldUnitKind.Illusion), Is.Zero);
            var catalog = Field<OriginalCombatCatalog>(s, "combatCatalog");
            catalog.Ability("A0LZ").fields.Single(f => f.key == "DataB1").number = 9;
            Assert.Throws<TargetInvocationException>(() => Begin(s, "A0ZA", hero.position));
            Assert.That(s.CasterTelegraphs.Length, Is.Zero);
        }

        [Test] public void EnemyImageLifetimeStopsDuringNativePauseAndExpiresWithoutWaveReward()
        {
            var s = Create(out var w); Begin(s, "A0ZA", w.UnitState(1).position); Tick(s, w, 1.65);
            var copy = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            Call(s, "AdvanceMirrors"); w.SetUnitState(copy.entityId, paused: true);
            Tick(s, w, 2); Call(s, "AdvanceMirrors");
            w.SetUnitState(copy.entityId, paused: false); Tick(s, w, 6.95); Call(s, "AdvanceMirrors");
            Assert.That(w.UnitState(copy.entityId).health, Is.GreaterThan(0));
            Tick(s, w, .05); Call(s, "AdvanceMirrors");
            Assert.That(w.UnitState(copy.entityId).health, Is.Zero);
            Assert.That(w.UnitState(copy.entityId).hidden, Is.True);
            Assert.That(Call(s, "ObserveImageDeath", copy.entityId), Is.False);
        }

        [Test] public void FriendlyMirrorReplacementDoesNotDestroyEnemyWandCopiesOfTheSameHero()
        {
            var s = Create(out var w); Begin(s, "A0ZA", w.UnitState(1).position); Tick(s, w, 1.65);
            var copy = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            var players = Field<System.Collections.IList>(s, "players");
            Assert.That(Call(s, "StartMirror", players[0], 1), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s, w, .3); Call(s, "AdvanceMirrors");
            Assert.That(w.UnitState(copy.entityId).health, Is.GreaterThan(0));
            Assert.That(w.UnitState(copy.entityId).hidden, Is.False);
        }

        [Test] public void SleepWaveMovesBeforeSweepDamagesOnceThenSleepsAndKeepsItsLastSweep()
        {
            var s = Create(out var w); var hero = w.UnitState(1); var profile = hero.profile;
            profile.maxHealth = 4000; w.UpdateProfile(1, profile, 4000, hero.mana);
            w.ForcePosition(2001, new OriginalPoint(0, 1000)); w.ForcePosition(1, new OriginalPoint(477, 1000));
            Begin(s, "A122", new OriginalPoint(2000, 1000)); Tick(s, w, 1.03);
            Assert.That(w.UnitState(1).health, Is.EqualTo(2800).Within(.001));
            Assert.That(Call(s, "HasNativeSleep", 1), Is.True);
            Assert.That(Field<bool>(s, "casterGate"), Is.False);
            Tick(s, w, .03); Assert.That(w.UnitState(1).health, Is.EqualTo(2800).Within(.001));
            w.ForceUnitDeath(2001); Tick(s, w, 1.05);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
            Assert.That(s.Snapshot().effects.Any(e => e.abilityId == "A122"), Is.False);

            var last = Create(out var lastWorld); var p = lastWorld.UnitState(1).profile; p.maxHealth = 4000;
            lastWorld.UpdateProfile(1, p, 4000, 0); lastWorld.ForcePosition(2001, new OriginalPoint(0, 1000));
            lastWorld.ForcePosition(1, new OriginalPoint(1449, 1000));
            Begin(last, "A122", new OriginalPoint(2000, 1000)); Tick(last, lastWorld, 2.08);
            Assert.That(lastWorld.UnitState(1).health, Is.EqualTo(4000));
            Tick(last, lastWorld, .03); Assert.That(lastWorld.UnitState(1).health, Is.EqualTo(2800).Within(.001));
            Assert.That(Call(last, "HasNativeSleep", 1), Is.True);
            Assert.That(Field<bool>(last, "casterGate"), Is.True);
        }

        [Test] public void SleepWaveDeclarationConflictRejectsBeforePublishingWarning()
        {
            var s = Create(out _); var catalog = Field<OriginalCombatCatalog>(s, "combatCatalog");
            catalog.Ability("A124").fields.Single(f => f.key == "DataA1").number = 3;
            Assert.Throws<TargetInvocationException>(() => Begin(s, "A122", new OriginalPoint(135, 1000)));
            Assert.That(s.CasterTelegraphs.Length, Is.Zero);
        }

        [Test] public void PullMovesBeforeDeadCasterCleanupAndRestoresOnlyItsOwnControls()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            Begin(s, "A0Z7", hero.position); Tick(s, w, 1.7);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorMoveBlocked", 1), Is.True);
            Assert.That((bool)Call(s, "ActorCastBlocked", 1), Is.False);
            Assert.That(w.UnitState(1).pathingDisabled, Is.True);
            Call(s, "SetNativeAbun", 1, "independent", true);
            var before = w.UnitState(1).position;
            w.ForceUnitDeath(2001); Tick(s, w, .05);
            Assert.That(w.UnitState(1).position.x, Is.EqualTo(before.x + 8).Within(.001));
            Assert.That(w.UnitState(1).pathingDisabled, Is.False);
            Assert.That((bool)Call(s, "IsNativeRooted", 1), Is.False);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.True);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }

        [Test] public void DragPausesCasterMovesBothAndAppliesFinalUniversalTickBeforeCleanup()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            Begin(s, "A0ZG", hero.position); Tick(s, w, 1.4);
            Assert.That(w.UnitState(2001).paused, Is.True);
            Assert.That(w.UnitState(2001).position.y, Is.EqualTo(hero.position.x));
            double hp = w.UnitState(1).health;
            w.ForceUnitDeath(2001); Tick(s, w, .05);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp - 8).Within(.001));
            Assert.That(w.UnitState(2001).position.x, Is.EqualTo(w.UnitState(1).position.x));
            Assert.That(w.UnitState(2001).paused, Is.False);
            Assert.That(w.UnitState(2001).pathingDisabled, Is.False);
            Assert.That((bool)Call(s, "ActorWeaponBlocked", 1), Is.False);
        }

        [Test] public void RootDeclarationConflictRejectsTetherBeforePublishingWarning()
        {
            var s = Create(out _); var catalog = Field<OriginalCombatCatalog>(s, "combatCatalog");
            catalog.Ability("A0KV").fields.Single(f => f.key == "HeroDur1").number = 9;
            Assert.Throws<TargetInvocationException>(() => Begin(s, "A0Z7", new OriginalPoint(135, 1000)));
            Assert.That(s.CasterTelegraphs.Length, Is.Zero);
        }

        [Test] public void StrikeSummonsHaveIndependentBodiesWithoutAddingWaveEnemies()
        {
            var s = Create(out var w); var match = Field<OriginalMatch>(s, "match");
            int before = match.Enemies.Count; var hero = w.UnitState(1); var profile = hero.profile;
            profile.maxHealth = 2000; w.UpdateProfile(1, profile, 2000, hero.mana);
            Begin(s, "A0Z8", hero.position); Tick(s, w, 1.7);
            var summoned = w.Snapshot().units.Where(u => u.rawcode == "n07C").ToArray();
            Assert.That(summoned.Length, Is.EqualTo(2));
            Assert.That(summoned.Select(u => u.entityId).Distinct().Count(), Is.EqualTo(2));
            foreach (var unit in summoned)
            {
                Assert.That(unit.kind, Is.EqualTo(OriginalWorldUnitKind.Enemy));
                Assert.That(unit.entityId, Is.GreaterThan(1000).And.LessThan(1000000000));
                Assert.That(unit.ownerSlot, Is.Zero);
                Assert.That(unit.profile.maxHealth, Is.EqualTo(840));
                Assert.That(unit.mana, Is.EqualTo(250));
                Assert.That((bool)Call(s, "CasterHasAbility", unit, "A0K4"), Is.True);
                Assert.That((bool)Call(s, "CasterHasAbility", unit, "A0QE"), Is.True);
            }
            Assert.That(match.Enemies.Count, Is.EqualTo(before));
            Assert.That(w.UnitState(1).health, Is.EqualTo(1720).Within(.001));
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void StrikeSummonRequiresLivingNativeHeroInTheWarningCircle()
        {
            var s = Create(out var w); Begin(s, "A0Z8", new OriginalPoint(-2000, -2000));
            Tick(s, w, 1.7);
            Assert.That(w.Snapshot().units.Any(u => u.rawcode == "n07C"), Is.False);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void AbilityOverlayIsPerInstanceDetachedAndFailedChangeIsAtomic()
        {
            var s = Create(out var w);
            var profile = new OriginalWorldUnitProfile { maxHealth = 100, collisionRadius = 24 };
            w.AddUnit(2201, 0, "n01X", profile, new OriginalPoint(1000, 1000));
            w.AddUnit(2202, 0, "n01X", profile, new OriginalPoint(1100, 1000));
            var method = s.GetType().GetMethod("ApplyUnitAbilityOverlay", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Trusted dynamic ability overlay is required.");
            var added = new[] { "A0K4" }; var removed = new[] { "A11G" };
            method.Invoke(s, new object[] { 2201, added, removed });
            added[0] = "A0QE"; removed[0] = "A0K4";
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(2201), "A0K4"), Is.True);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(2201), "A11G"), Is.False);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(2202), "A11G"), Is.True);
            var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(s, new object[] { 2201, new[] { "A0QE", "bad" }, new[] { "A0K4" } }));
            Assert.That(error.InnerException, Is.InstanceOf<ArgumentException>());
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(2201), "A0K4"), Is.True);
            Assert.That((bool)Call(s, "CasterHasAbility", w.UnitState(2201), "A0QE"), Is.False);
            var nativeIds = ((System.Collections.Generic.IEnumerable<OriginalCombatDefinition>)Call(s, "NativeUnitAbilities", w.UnitState(2201))).Select(a => a.id).ToArray();
            Assert.That(nativeIds, Does.Contain("A0K4").And.Not.Contain("A11G"));
        }

        [Test] public void ScriptedDeathIsIdempotentAndCannotConsumeAWaveEnemy()
        {
            var s = Create(out var w); var match = Field<OriginalMatch>(s, "match");
            int before = match.Enemies.Count;
            Begin(s, "A0Z8", w.UnitState(1).position); Tick(s, w, 1.7);
            int id = w.Snapshot().units.First(u => u.rawcode == "n07C").entityId;
            Assert.That((bool)Call(s, "OnScriptedEnemyDied", id, 1), Is.False);
            w.ForceUnitDeath(id);
            Assert.That((bool)Call(s, "OnScriptedEnemyDied", id, 1), Is.True);
            Assert.That((bool)Call(s, "OnScriptedEnemyDied", id, 1), Is.False);
            Assert.That(match.Enemies.Count, Is.EqualTo(before));
            Assert.That(Call(s, "EnemyState", id), Is.Null);
        }

        [Test] public void InvalidScriptedOverlayCannotLeaveAPartialWorldBody()
        {
            var s = Create(out var w); int before = w.Snapshot().units.Length;
            var method = s.GetType().GetMethod("SpawnScriptedEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(s, new object[] {
                "n07C", new OriginalPoint(1000, 1000), 0, new[] { "bad" }, Array.Empty<string>(), null, null }));
            Assert.That(error.InnerException, Is.InstanceOf<ArgumentException>());
            Assert.That(w.Snapshot().units.Length, Is.EqualTo(before));
        }

        [Test] public void SuppliedProfileCannotIntroduceAnUnknownScriptedRawcode()
        {
            var s = Create(out var w); int before = w.Snapshot().units.Length;
            var method = s.GetType().GetMethod("SpawnScriptedEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(s, new object[] {
                "zzzz", new OriginalPoint(1000, 1000), 0, null, null,
                new OriginalWorldUnitProfile { maxHealth = 100, collisionRadius = 24 }, (double?)0 }));
            Assert.That(error.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(w.Snapshot().units.Length, Is.EqualTo(before));
        }

        [Test] public void WarningSnapshotsAreDetachedAndConnectedHumansKeepHoldPosition()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            w.AddIllusion(1000000001, 1, hero.profile, new OriginalPoint(230, 1000), hero.health, hero.mana);
            Assert.That(w.HoldPosition(1), Is.True);
            Begin(s, "A0Z3", new OriginalPoint(135, 1000));
            Assert.That(w.UnitState(1).holding, Is.True);
            Assert.That(w.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(w.UnitState(1000000001).order, Is.EqualTo(OriginalWorldOrder.None));
            var view = s.CasterTelegraphs; Assert.That(view.Length, Is.EqualTo(1));
            view[0].resolvesAt = 999; Assert.That(s.CasterTelegraphs[0].resolvesAt, Is.EqualTo(2));
        }
        [Test] public void SourceAutoDodgeAppliesOnlyToLeftPlayersAndNativeComputerHeroes()
        {
            var s = Create(out var w);
            var player = Field<System.Collections.IList>(s, "players")[0];
            player.GetType().GetField("connected").SetValue(player, false);
            Begin(s, "A0Z3", new OriginalPoint(135, 1000));
            Assert.That(w.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.Move));
            w.AddUnit(2002, 0, "O006", new OriginalWorldUnitProfile { maxHealth = 30000, collisionRadius = 24, moveSpeed = 390 }, new OriginalPoint(400, 1000));
            Begin(s, "A0Z3", new OriginalPoint(135, 1000));
            Assert.That(w.UnitState(2002).order, Is.EqualTo(OriginalWorldOrder.Move));
        }
        [Test] public void RepeatedSafeRingOrdersCannotOverrideConnectedPlayersHold()
        {
            var s = Create(out var w); w.SetUnitState(1, invulnerable: true);
            w.HoldPosition(1); Begin(s, "A0Z6", new OriginalPoint(135, 1000));
            Tick(s, w, 3.5);
            Assert.That(w.UnitState(1).holding, Is.True);
            Assert.That(w.UnitState(1).position.x, Is.EqualTo(135));
            Assert.That(w.UnitState(1).position.y, Is.EqualTo(1000));
        }
        [Test] public void PulseOutlivesDeadCasterAndReopensGlobalGateOnSeventhTimerTick()
        {
            var s = Create(out var w); Begin(s, "A0Z3", new OriginalPoint(-2500, -2500));
            w.ForceUnitDeath(2001); Tick(s, w, 8);
            Assert.That(Field<bool>(s, "casterGate"), Is.False);
            Assert.That(s.CasterTelegraphs, Is.Empty);
            Tick(s, w, 1); Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void FrostPulseDealsSixSourceHitsAndRefreshesNativeFrostAfterEachHit()
        {
            var s = Create(out var w); var hero = w.UnitState(1); var profile = hero.profile;
            profile.maxHealth = 5000; w.UpdateProfile(1, profile, 5000, hero.mana);
            Begin(s, "A1DA", hero.position); w.ForceUnitDeath(2001);
            Tick(s, w, 1.99);
            Assert.That(w.UnitState(1).health, Is.EqualTo(5000));
            Assert.That((double)Call(s, "ArcherDebuffMovementBonus", 1), Is.Zero);
            Tick(s, w, .01);
            Assert.That(w.UnitState(1).health, Is.EqualTo(4760).Within(.001));
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(150).Within(.001));
            Assert.That((double)Call(s, "ArcherDebuffAttackSlow", 1), Is.EqualTo(.25));
            Tick(s, w, 5);
            Assert.That(w.UnitState(1).health, Is.EqualTo(3560).Within(.001));
            Assert.That(Field<bool>(s, "casterGate"), Is.False);
            Tick(s, w, 1);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
            Assert.That(w.UnitState(1).health, Is.EqualTo(3560).Within(.001));
        }
        [Test] public void FrostSecondaryAreaCanReachOutsidePulseButDeadPrimaryGetsNoBuff()
        {
            var s = Create(out var w); w.RemoveUnit(2001); w.ForcePosition(1, new OriginalPoint(-2000, -2000));
            var center = new OriginalPoint(135, 1000);
            var profile = new OriginalWorldUnitProfile { maxHealth = 1000, moveSpeed = 250, collisionRadius = 24 };
            w.AddUnit(2101, 0, "hfoo", profile, new OriginalPoint(525, 1000));
            w.AddUnit(2102, 0, "hfoo", profile, new OriginalPoint(705, 1000));
            w.AddUnit(2103, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 100, moveSpeed = 250, collisionRadius = 24 },
                new OriginalPoint(-255, 1000));
            Call(s, "BeginCasterEffect", 1, 1, new OriginalCasterRules(Field<OriginalCombatCatalog>(s, "combatCatalog"), "A1DA"), center, w.Clock);
            Tick(s, w, 2);
            Assert.That(w.UnitState(2101).health, Is.EqualTo(700).Within(.001));
            Assert.That(w.UnitState(2101).profile.moveSpeed, Is.EqualTo(150).Within(.001));
            Assert.That(w.UnitState(2102).health, Is.EqualTo(1000));
            Assert.That(w.UnitState(2102).profile.moveSpeed, Is.EqualTo(150).Within(.001));
            Assert.That(w.UnitState(2103).health, Is.Zero);
            Assert.That((double)Call(s, "ArcherDebuffMovementBonus", 2103), Is.Zero);
        }
        [Test] public void UnresolvedFrostConstantRejectsBeforeChangingOrderOrMana()
        {
            var s = Create(out var w); w.RemoveUnit(2001);
            w.AddUnit(2001, 0, "n024", new OriginalWorldUnitProfile { maxHealth = 5000, maxMana = 10000,
                collisionRadius = 24 }, new OriginalPoint(700, 1000));
            w.HoldPosition(2001);
            Field<OriginalNativeCatalog>(s, "native").Constant("FrostMoveSpeedDecrease").known = false;
            var error = Assert.Throws<TargetInvocationException>(() => Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)));
            Assert.That(error.InnerException, Is.InstanceOf<InvalidOperationException>());
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(10000));
            Assert.That(w.UnitState(2001).holding, Is.True);
            Assert.That((bool)Call(s, "CasterControlsActor", 2001), Is.False);
            Assert.That(s.CasterTelegraphs, Is.Empty);
        }
        [Test] public void PeriodicRingStopsAtNextTimerCallbackWhenCasterDies()
        {
            var s = Create(out var w); Begin(s, "A0Z6", new OriginalPoint(-2000, -2000));
            Tick(s, w, 2.5); w.ForceUnitDeath(2001); Tick(s, w, 1);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void CompletedEffectReopensSourceBooleanWhileAnotherWarningRemains()
        {
            var s = Create(out var w); Begin(s, "A0Z9", new OriginalPoint(-2500, -2500));
            Begin(s, "A0Z3", new OriginalPoint(-2500, -2500));
            Tick(s, w, 1.6);
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
            Assert.That(s.CasterTelegraphs.Length, Is.EqualTo(1));
        }
        [Test] public void CancelledNativeCastSpendsNoManaAndPublishesNoEffect()
        {
            var s = Create(out var w); var actor = w.UnitState(2001);
            Assert.That((bool)Call(s, "TryStartCaster", actor, new OriginalPoint(135, 1000)), Is.True);
            Assert.That((bool)Call(s, "CasterControlsActor", 2001), Is.True);
            Call(s, "OnCasterAcceptedWorldOrder", 2001); Tick(s, w, 1);
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(actor.mana));
            Assert.That(s.CasterTelegraphs, Is.Empty);
        }
        [Test] public void PersistentSilencePreventsAutomaticCasterRestartUntilCleared()
        {
            var s = Create(out var w); var actor = w.UnitState(2001);
            Assert.That((bool)Call(s, "TryStartCaster", actor, new OriginalPoint(135, 1000)), Is.True);
            Call(s, "SetActorControl", 2001, "test-silence", OriginalActorControlMask.Cast, 0d, true, false);
            Tick(s, w, 1);
            Assert.That((bool)Call(s, "CasterControlsActor", 2001), Is.False);
            Assert.That((bool)Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)), Is.False);
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(actor.mana));
            Assert.That(s.CasterTelegraphs, Is.Empty);
            Call(s, "ClearActorControl", 2001, "test-silence");
            Assert.That((bool)Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)), Is.True);
            Tick(s, w, .5);
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(actor.mana - 150));
            Assert.That(s.CasterTelegraphs.Length, Is.EqualTo(1));
        }
        [Test] public void SuccessfulCastDebitsOnceAtEffectAndHonorsCooldown()
        {
            var s = Create(out var w); var actor = w.UnitState(2001);
            Assert.That((bool)Call(s, "TryStartCaster", actor, new OriginalPoint(-200, 1000)), Is.True);
            Tick(s, w, .49); Assert.That(w.UnitState(2001).mana, Is.EqualTo(500));
            Tick(s, w, .01); Assert.That(w.UnitState(2001).mana, Is.EqualTo(350));
            Assert.That(s.CasterTelegraphs.Length, Is.EqualTo(1));
            Assert.That((bool)Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)), Is.False);
        }
        [Test] public void ConflictingHelperCannotSpendResourcesButMeasuredInstantPulseCan()
        {
            var s = Create(out var w);
            w.RemoveUnit(2001); w.AddUnit(2001, 0, "n06K", new OriginalWorldUnitProfile { maxHealth = 1000, maxMana = 500, collisionRadius = 24 }, new OriginalPoint(700, 1000));
            Field<OriginalCombatCatalog>(s,"combatCatalog").Unit("n06L").fields.Single(f=>f.key=="HP").number=21;
            Assert.Throws<TargetInvocationException>(()=>Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)));
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(500));
            Assert.That(s.CasterGaps.Any(g => g.StartsWith("A123:")), Is.True);
            w.RemoveUnit(2001); w.AddUnit(2001, 0, "n05J", new OriginalWorldUnitProfile { maxHealth = 1000, maxMana = 500, collisionRadius = 24 }, new OriginalPoint(700, 1000));
            Assert.That((bool)Call(s, "TryStartCaster", w.UnitState(2001), new OriginalPoint(135, 1000)), Is.True);
            Assert.That(w.UnitState(2001).mana, Is.EqualTo(350));
            Assert.That(s.CasterTelegraphs.Length, Is.EqualTo(1));
            Assert.That((bool)Call(s, "CasterControlsActor", 2001), Is.False);
        }
        [Test] public void MeasuredMagicPulseDamagesWithoutNumericalArmorAndDoesNotHalt()
        {
            var s = Create(out var w); Assert.That(s.CasterNativeClosureReady, Is.True);
            Begin(s, "A0Z3", new OriginalPoint(135, 1000)); w.Stop(1);
            double before = w.UnitState(1).health;
            Tick(s, w, 3);
            Assert.That(w.UnitState(1).health, Is.EqualTo(before - 40).Within(1e-7));
        }
        [Test] public void AntiHealClampsTheSampledMinimumFor333TicksAndOutlivesCaster()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            w.UpdateProfile(1, hero.profile, 400, hero.mana);
            Begin(s, "A0Z5", hero.position); Tick(s, w, 1.8);
            w.ForceUnitDeath(2001);
            w.UpdateProfile(1, hero.profile, 500, hero.mana); Tick(s, w, .03);
            Assert.That(w.UnitState(1).health, Is.EqualTo(400));
            w.ApplyUnitDamage(1, 100); Tick(s, w, .03);
            w.UpdateProfile(1, hero.profile, 500, hero.mana); Tick(s, w, 9.90);
            Assert.That(w.UnitState(1).health, Is.EqualTo(300));
            Assert.That(Field<bool>(s, "casterGate"), Is.False);
            Tick(s, w, .03); Assert.That(Field<bool>(s, "casterGate"), Is.True);
            w.UpdateProfile(1, hero.profile, 500, hero.mana); Tick(s, w, .03);
            Assert.That(w.UnitState(1).health, Is.EqualTo(500));
        }
        [Test] public void AntiHealCannotSelectAnImageOrMarkTheSameTargetTwice()
        {
            var s = Create(out var w); var hero = w.UnitState(1);
            w.AddIllusion(1000000001, 1, hero.profile, new OriginalPoint(230,1000), hero.health, hero.mana);
            Begin(s, "A0Z5", hero.position); Begin(s, "A0Z5", hero.position); Tick(s, w, 1.8);
            var active = s.Snapshot().effects.Where(e => e.abilityId == "A0Z5").ToArray();
            Assert.That(active.Length, Is.EqualTo(1));
            var image = w.UnitState(1000000001); w.UpdateProfile(image.entityId, image.profile, 300, image.mana);
            Tick(s,w,.03); w.UpdateProfile(image.entityId,image.profile,500,image.mana); Tick(s,w,.03);
            Assert.That(w.UnitState(image.entityId).health,Is.EqualTo(500));
        }
        [Test] public void AllyThrowMarksAllEligibleAlliesButLaunchesOnlyTheLastAndNullRoundsDoNoDamage()
        {
            var s=Create(out var w);
            var profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=0,collisionRadius=24,moveSpeed=0};
            w.AddUnit(2002,0,"n008",profile,new OriginalPoint(650,1000));
            w.AddUnit(2003,0,"n008",profile,new OriginalPoint(600,1000));
            Begin(s,"A0ZD",w.UnitState(1).position); double hp=w.UnitState(1).health;
            Tick(s,w,2.51); Assert.That(w.UnitState(2003).position.x,Is.EqualTo(600));
            Tick(s,w,.04); Assert.That(w.UnitState(2003).position.x,Is.EqualTo(572).Within(.0001));
            Assert.That(w.UnitState(2002).position.x,Is.EqualTo(650));
            Tick(s,w,.64); Assert.That(w.UnitState(1).health,Is.EqualTo(hp-200).Within(.0001));
            Assert.That(Field<bool>(s,"casterGate"),Is.False,"Obv projectile completion does not reopen MC.");
            Tick(s,w,4.37); Assert.That(w.UnitState(1).health,Is.EqualTo(hp-200).Within(.0001));
            Assert.That(w.UnitState(2002).position.x,Is.EqualTo(650));
            Assert.That(Field<bool>(s,"casterGate"),Is.True);
        }
        [Test] public void AllyThrowWithNoEligibleAllyPreservesNativeRejectedNullSourceDamage()
        {
            var s=Create(out var w); double hp=w.UnitState(1).health;
            Begin(s,"A0ZD",w.UnitState(1).position); Tick(s,w,7.56);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp)); Assert.That(Field<bool>(s,"casterGate"),Is.True);
        }
        [Test] public void AllyThrowExcludesSourceTagOneButAllowsTagTwoAndRemovedThrowerCannotHit()
        {
            var s=Create(out var w); var match=Field<OriginalMatch>(s,"match");
            var enemies=Field<System.Collections.Generic.Dictionary<int,OriginalMatchEnemy>>(match,"enemies");
            var profile=new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24};
            foreach(int id in new[]{2002,2003})
            {
                w.AddUnit(id,0,"n008",profile,new OriginalPoint(id==2002?650:600,1000));
                enemies.Add(id-1000,new OriginalMatchEnemy{entityId=id-1000,rawcode="n008",sourceUserData=id==2002?1:2});
            }
            Begin(s,"A0ZD",w.UnitState(1).position); double hp=w.UnitState(1).health;
            Tick(s,w,2.55); Assert.That(w.UnitState(2002).position.x,Is.EqualTo(650));
            Assert.That(w.UnitState(2003).position.x,Is.EqualTo(572).Within(.0001));
            w.RemoveUnit(2003); Tick(s,w,5.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Assert.That(w.UnitState(2002).position.x,Is.EqualTo(650));
        }
        [Test] public void TriggeredFlagsMatchNativeAxesAndRejectUnknownModeBeforeMutation()
        {
            var modes = new[] { OriginalTriggeredDamageMode.SpellNormal, OriginalTriggeredDamageMode.SpellMagic, OriginalTriggeredDamageMode.ChaosUniversal };
            var expected = new[] { 40 * .8 / 1.312, 32d, 40d };
            for (int i = 0; i < modes.Length; i++)
            {
                var s = Create(out var w); var hero = w.UnitState(1);
                Call(s, "ApplyTriggeredHit", 2001, 0, hero, 40d, modes[i]);
                Assert.That(hero.health - w.UnitState(1).health, Is.EqualTo(expected[i]).Within(1e-6), modes[i].ToString());
                var error = Assert.Throws<TargetInvocationException>(() => Call(s, "ApplyTriggeredHit", 2001, 0, w.UnitState(1), 40d, (OriginalTriggeredDamageMode)99));
                Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(hero.health - w.UnitState(1).health, Is.EqualTo(expected[i]).Within(1e-6));
            }
        }
        [Test] public void MovingWaveCarriesCapturedUnitForThirtySevenSweepsAndOutlivesCaster()
        {
            var s = Create(out var w);
            w.SetUnitState(1, paused: true, invulnerable: true);
            Begin(s, "A0ZH", new OriginalPoint(135, 1000)); w.Stop(1);
            Tick(s, w, 1.3); Assert.That(w.UnitState(1).position.x, Is.EqualTo(135));
            Tick(s, w, .03); Assert.That(w.UnitState(1).position.x, Is.EqualTo(162).Within(1e-6));
            w.ForceUnitDeath(2001);
            Tick(s, w, 1.05);
            Assert.That(w.UnitState(1).position.x, Is.EqualTo(1107).Within(1e-6));
            Assert.That(Field<bool>(s, "casterGate"), Is.False);
            Tick(s, w, .03);
            Assert.That(w.UnitState(1).position.x, Is.EqualTo(1134).Within(1e-6));
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void MovingWaveKeepsCapturedCorpseAtItsLastPosition()
        {
            var s = Create(out var w);
            w.SetUnitState(1, paused: true, invulnerable: true);
            Begin(s, "A0ZH", new OriginalPoint(135, 1000)); w.Stop(1);
            Tick(s, w, 1.33); var point = w.UnitState(1).position;
            w.ForceUnitDeath(1); Tick(s, w, 1.08);
            Assert.That(w.UnitState(1).position.x, Is.EqualTo(point.x));
            Assert.That(w.UnitState(1).position.y, Is.EqualTo(point.y));
            Assert.That(Field<bool>(s, "casterGate"), Is.True);
        }
        [Test] public void MovingWaveDoesNotRetryDamageAndRespectsExcludedForceRegion()
        {
            var s = Create(out var w);
            w.ForcePosition(1, new OriginalPoint(400, 2680));
            w.ForcePosition(2001, new OriginalPoint(400, 3000));
            w.SetUnitState(1, paused: true, invulnerable: true);
            Begin(s, "A0ZH", new OriginalPoint(400, 2680)); w.Stop(1);
            Tick(s, w, 1.33);
            Assert.That(w.UnitState(1).position.y, Is.EqualTo(2680));
            // Native invulnerability blocks this hit but not group insertion.
            // A second attempt would reach the still-guarded damage resolver.
            w.SetUnitState(1, invulnerable: false);
            Assert.DoesNotThrow(() => Tick(s, w, .03));
            Assert.That(w.UnitState(1).position.y, Is.EqualTo(2680));
        }
    }
}

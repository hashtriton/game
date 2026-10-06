using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionProgressionTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        sealed class Fixture
        {
            internal OriginalSession session;
            internal OriginalObservedCatalog observed;
            internal long sequence = 3;
            internal OriginalSessionPlayerView Player => session.Snapshot().players[0];
            internal OriginalSessionReplyCode Learn(string skill, long connection = 0) => session.Apply(connection,
                new OriginalSessionCommand { sequence = ++sequence, kind = OriginalSessionCommandKind.LearnSkill, skillId = skill });
            internal void Advance(double seconds)
            {
                while (seconds > 1e-9 && session.HaltReason == null)
                { double step = Math.Min(1, seconds); session.Advance(step); session.DrainEvents(); seconds -= step; }
            }
        }
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static Fixture Create(string hero = "H008", bool world = false)
        {
            var f = new Fixture { observed = Load<OriginalObservedCatalog>("lia39-observed126.json") };
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            f.session.ConfigureProgression(Load<OriginalNativeCatalog>("lia39-native126.json"), f.observed);
            if (world) f.session.ConfigureWorld(new Navigation(), Load<OriginalNativeCatalog>("lia39-native126.json"), Array.Empty<OriginalWorldDoodadView>());
            f.session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = hero });
            f.session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.session.DrainEvents(); return f;
        }
        static void FirstWave(Fixture f)
        {
            f.Advance(2.05);
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = ++f.sequence, kind = OriginalSessionCommandKind.WaveReady }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(3);
            Assert.That(f.session.Snapshot().enemies.Length, Is.EqualTo(42));
        }
        static void GrantTrustedExperience(Fixture fixture, int level)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var player = ((IList)typeof(OriginalSession).GetField("players", flags).GetValue(fixture.session))[0];
            var progressField = player.GetType().GetField("progression");
            var candidate = ((OriginalHeroProgression)progressField.GetValue(player)).Copy();
            candidate.GrantExperience(Load<OriginalNativeCatalog>("lia39-native126.json").heroXp.levels.First(row => row.level == level).cumulative - candidate.Experience);
            var stats = typeof(OriginalSession).GetMethod("CalculateProgressionStats", flags).Invoke(fixture.session, new object[] { fixture.Player.heroId, candidate });
            typeof(OriginalSession).GetMethod("ApplyProgressionProfile", flags).Invoke(fixture.session, new[] { player, stats, candidate });
            progressField.SetValue(player, candidate);
            player.GetType().GetField("stats").SetValue(player, stats);
        }

        [Test] public void AttributeLearningPublishesMeasuredStatsWithBoundedResourceRoundingAndSpendsOnePoint()
        {
            foreach (string heroId in new[] { "H008", "N0A0", "H024" })
            {
                var f = Create(heroId, true); GrantTrustedExperience(f, 12);
                var world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
                var before = world.UnitState(1);
                world.UpdateProfile(1, before.profile, before.profile.maxHealth / 2, before.profile.maxMana / 2);
                int points = f.Player.progression.unspentSkillPoints;
                Assert.That(f.Learn("A001"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                var after = world.UnitState(1); var measured = f.observed.VitalityChange(heroId, "learn-A001");
                Assert.That(after.profile.maxHealth, Is.EqualTo(measured.afterMaxHP));
                Assert.That(after.profile.maxMana, Is.EqualTo(measured.afterMaxMP));
                // This is a declared approximation, not false bit-exact parity.
                Assert.That(after.health, Is.EqualTo(measured.afterHP).Within(1));
                Assert.That(after.mana, Is.EqualTo(measured.afterMP).Within(1));
                Assert.That(f.Player.progression.unspentSkillPoints, Is.EqualTo(points - 1));
                Assert.That(f.Player.progression.attributeBonus.strength, Is.EqualTo(2));
                Assert.That(f.Player.progression.attributeBonus.agility, Is.EqualTo(2));
                Assert.That(f.Player.progression.attributeBonus.intelligence, Is.EqualTo(2));
                Assert.That(measured.policyKnown, Is.False, "Do not rewrite unresolved native rounding as evidence.");
            }
        }

        [Test] public void ExperienceAwardCannotReviveADeadHeroWhenItsMaxHealthIncreases()
        {
            var f = Create(world: true);
            var world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
            world.ForceUnitDeath(1); GrantTrustedExperience(f, 2);
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(655));
            Assert.That(world.UnitState(1).health, Is.Zero);
        }
        [Test] public void StartPublishesFiveSourceSkillsOnePointAndDetachedLearningChoices()
        {
            var f = Create(); var player = f.Player;
            Assert.That(player.hasProgression, Is.True); Assert.That(player.progression.level, Is.EqualTo(1));
            Assert.That(player.progression.unspentSkillPoints, Is.EqualTo(1));
            Assert.That(player.learning.Select(s => s.id), Is.EqualTo(new[] { "A05N", "A05M", "A102", "A0E6", "A001" }));
            player.learning[0].id = "fake"; player.progression.skills[0].rank = 99;
            Assert.That(f.Player.learning[0].id, Is.EqualTo("A05N")); Assert.That(f.Player.progression.skills[0].rank, Is.Zero);
        }
        [Test] public void LearningValidatesConnectionHeroPointsAndSequenceWithoutClientXp()
        {
            var f = Create();
            Assert.That(f.session.Apply(99, new OriginalSessionCommand { sequence = 4,
                kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" }), Is.EqualTo(OriginalSessionReplyCode.UnknownConnection));
            Assert.That(f.Learn("A15X"), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(f.Player.progression.unspentSkillPoints, Is.EqualTo(1));
            Assert.That(f.Learn("A05N"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Player.progression.skills.Single(s => s.id == "A05N").rank, Is.EqualTo(1));
            Assert.That(f.Player.progression.unspentSkillPoints, Is.Zero);
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = f.sequence, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(f.Learn("A05M"), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(f.Player.experience, Is.Zero);
        }
        [Test] public void UnavailableDerivedStatsDoNotSpendThePointOrPublishTheLearnRank()
        {
            var f = Create(); f.observed.Hero("H008", 1).maxHP = double.NaN;
            Assert.That(f.Learn("A05N"), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(f.Player.progression.unspentSkillPoints, Is.EqualTo(1));
            Assert.That(f.Player.progression.skills.Single(s => s.id == "A05N").rank, Is.Zero);
            Assert.That(f.Player.auxiliaryAbilities, Is.Empty);
        }
        [Test] public void ArcherLearningPublishesOrderedSourceHelperStateWithoutPretendingToCast()
        {
            var f = Create("N0A0");
            FirstWave(f);
            foreach (var enemy in f.session.Snapshot().enemies.ToArray()) f.session.ReportEnemyKilled(enemy.entityId, true);
            Assert.That(f.Learn("A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Player.archerAttackHandlerRegistered, Is.True); Assert.That(f.Player.bowElement, Is.EqualTo(1));
            Assert.That(f.Player.auxiliaryAbilities.Single().id, Is.EqualTo("A15Z"));
            Assert.That(f.Player.auxiliaryAbilities.Single().rank, Is.EqualTo(1));
            Assert.That(f.Player.progression.skills.Single(s => s.id == "A15X").rank, Is.EqualTo(1));
        }
        [Test] public void MatchExperienceSyncsOnceAndLevelsUseObservedHeroBaseline()
        {
            var f = Create(); FirstWave(f);
            foreach (var enemy in f.session.Snapshot().enemies.ToArray())
                Assert.That(f.session.ReportEnemyKilled(enemy.entityId, true), Is.True);
            var player = f.Player;
            Assert.That(player.experience, Is.GreaterThanOrEqualTo(200));
            Assert.That(player.progression.experience, Is.EqualTo(player.experience));
            Assert.That(player.progression.matchExperienceWatermark, Is.EqualTo(player.experience));
            Assert.That(player.progression.level, Is.GreaterThan(1));
            Assert.That(player.progression.unspentSkillPoints, Is.EqualTo(player.progression.level));
            for (int i = 0; i < 4; i++) { f.session.DrainEvents(); f.session.Snapshot(); }
            f.Advance(.1);
            Assert.That(f.Player.progression.experience, Is.EqualTo(player.progression.experience));
            Assert.That(f.Player.progression.unspentSkillPoints, Is.EqualTo(player.progression.unspentSkillPoints));
        }
        [Test] public void UnknownLevelStatsLeaveProgressionWatermarkAtomicAndHaltSourceXpConsumption()
        {
            var f = Create(); FirstWave(f);
            f.observed.Hero("H008", 2).maxHP = double.NaN;
            foreach (var enemy in f.session.Snapshot().enemies.ToArray())
            {
                if (f.session.HaltReason != null) break;
                f.session.ReportEnemyKilled(enemy.entityId, true);
            }
            Assert.That(f.session.HaltReason, Does.StartWith("progression-rule-unavailable:"));
            Assert.That(f.Player.progression.level, Is.EqualTo(1));
            Assert.That(f.Player.progression.experience, Is.LessThan(200));
            Assert.That(f.Player.progression.matchExperienceWatermark, Is.EqualTo(f.Player.progression.experience));
        }
        [Test] public void LevelUpUpdatesWorldMaximaAndPreservesMeasuredHealthAndManaDeficits()
        {
            var f = Create(world: true); FirstWave(f);
            var world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
            var hero = world.UnitState(1); Assert.That(world.UpdateProfile(1, hero.profile, 315.5, 72.5), Is.True);
            foreach (var enemy in f.session.Snapshot().enemies)
            {
                f.session.ReportEnemyKilled(enemy.entityId, true);
                if (f.Player.progression.level > 1 || f.session.HaltReason != null) break;
            }
            Assert.That(f.session.HaltReason, Is.Null); Assert.That(f.Player.progression.level, Is.EqualTo(2));
            hero = world.UnitState(1);
            Assert.That(hero.profile.maxHealth, Is.EqualTo(655)); Assert.That(hero.profile.maxMana, Is.EqualTo(165));
            Assert.That(hero.health, Is.EqualTo(339.5).Within(1e-9)); Assert.That(hero.mana, Is.EqualTo(92.5).Within(1e-9));
        }
    }
}

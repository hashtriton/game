using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalMatchTests
    {
        static OriginalMatchCatalog Catalog()
        {
            return JsonUtility.FromJson<OriginalMatchCatalog>(File.ReadAllText(
                Path.Combine(Application.dataPath, "Arena/Data/lia39-match.json")));
        }

        static OriginalMatch Create(int participants = 1, OriginalDifficulty difficulty = OriginalDifficulty.Standard,
            bool? casters = null)
        {
            var options = OriginalMatchOptions.ForDifficulty(difficulty);
            if (casters.HasValue) options.casters = casters.Value;
            return new OriginalMatch(Catalog(), options, participants, 12345);
        }

        static void StartRound(OriginalMatch match)
        {
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            match.Advance(2);
            for (var i = 1; i <= match.Participants; i++) Assert.That(match.SetReady(i), Is.True);
            if (match.Phase == OriginalMatchPhase.BossCountdown) match.Advance(5);
            else match.Advance(2.00001);
        }

        static void KillAll(OriginalMatch match)
        {
            var guard = 0;
            while (match.Enemies.Count > 0 && match.Phase == OriginalMatchPhase.Combat)
            {
                foreach (var enemy in match.Enemies.ToArray()) Assert.That(match.EnemyKilled(enemy.entityId), Is.True);
                Assert.That(++guard, Is.LessThan(20));
            }
        }

        static void ReachRound(OriginalMatch match, int round)
        {
            match.Begin();
            while (match.Round < round)
            {
                if (match.Phase == OriginalMatchPhase.DuelPreparation) match.Advance(25);
                if (match.Phase == OriginalMatchPhase.Duel) { match.CompleteDuelSequence(); continue; }
                StartRound(match);
                KillAll(match);
                if (match.Phase == OriginalMatchPhase.Combat)
                    Assert.Fail("Unexpected incomplete wave " + match.Round);
                match.Advance(match.Round % 5 == 0 ? 1 : 3);
            }
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Preparation));
        }

        [Test]
        public void RoundTwentyFivePhaseAddHasFaMembershipAndExperienceButDoesNotCompleteTheBossRound()
        {
            var match = Create(); ReachRound(match, 25); StartRound(match); match.DrainEvents();
            var entry = typeof(OriginalMatch).GetMethod("SpawnBossPhaseAdd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.That(entry, Is.Not.Null);
            var boss = match.Enemies.Single(); int remaining = match.RemainingCount;
            var add = (OriginalMatchEnemy)entry.Invoke(match, null);
            Assert.That(add.rawcode, Is.EqualTo("n01X"));
            Assert.That(add.finalAdd, Is.True); Assert.That(add.counted, Is.False); Assert.That(add.megaBoss, Is.False);
            Assert.That(add.sourceUserData, Is.EqualTo(1));
            Assert.That(add.x, Is.InRange(-576, 576)); Assert.That(add.y, Is.InRange(-3328, -2112));
            var events = match.DrainEvents();
            Assert.That(events.Count(e => e.kind == OriginalMatchEventKind.Spawn), Is.EqualTo(1));
            Assert.That(events.Any(e => e.kind == OriginalMatchEventKind.FinalAddScaling), Is.False);
            Assert.That(match.EnemyKilled(add.entityId), Is.True);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
            Assert.That(match.RemainingCount, Is.EqualTo(remaining));
            Assert.That(match.Enemies.Single().entityId, Is.EqualTo(boss.entityId));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Experience), Is.True);
        }

        [Test]
        public void CatalogKeepsOriginalRosterAndMarksMissingRawcode()
        {
            var catalog = Catalog(); catalog.Validate();
            Assert.That(catalog.waves.Length, Is.EqualTo(30));
            Assert.That(catalog.Wave(1).regularId, Is.EqualTo("n008"));
            Assert.That(catalog.Wave(21).regularId, Is.EqualTo("n069"));
            Assert.That(catalog.Wave(23).regularId, Is.EqualTo("u00L"));
            Assert.That(catalog.Wave(30).bossId, Is.EqualTo("O006"));
            Assert.That(catalog.Enemy("u00L").HasAbility("A0K4"), Is.True);
            Assert.That(catalog.Enemy("n068").definitionKnown, Is.False);
        }

        [Test]
        public void DescendantsSpawnAtTheDyingUnitsCurrentLocationRatherThanItsOriginalGate()
        {
            foreach (int round in new[] { 21, 23 })
            {
                var match = Create(); ReachRound(match, round); StartRound(match);
                var parent = match.Enemies.First(e => e.rawcode == (round == 21 ? "n069" : "u00L"));
                Assert.That(match.SetEnemyPosition(parent.entityId, 432, 765), Is.True);
                Assert.Throws<ArgumentOutOfRangeException>(() => match.SetEnemyPosition(parent.entityId, float.NaN, 0));
                match.DrainEvents();
                Assert.That(match.EnemyKilled(parent.entityId), Is.True);
                var children = match.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
                Assert.That(children.Length, Is.EqualTo(round == 21 ? 2 : 10));
                foreach (var child in children)
                { Assert.That(child.x, Is.EqualTo(432)); Assert.That(child.y, Is.EqualTo(765)); Assert.That(child.attackGroup, Is.Zero); }
            }
        }

        [Test]
        public void PresetsKeepOriginalCustomizableOptions()
        {
            var easy = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Easy);
            Assert.That(easy.equalGold && easy.acolyteBonus && easy.altars, Is.True);
            Assert.That(easy.casters, Is.False);
            var nightmare = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Nightmare);
            Assert.That(nightmare.curse && nightmare.casters, Is.True);
            Assert.That(nightmare.altars || nightmare.runes || nightmare.explosiveBarrels, Is.False);
            Assert.That(nightmare.defensiveBarrels, Is.EqualTo(OriginalDefensiveBarrels.Destroyed));
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Create(9));
        }

        [Test]
        public void InitialPreparationUsesSourceRoundZeroResources()
        {
            var match = Create();
            Assert.That(match.Begin(), Is.True);
            Assert.That(match.Begin(), Is.False);
            Assert.That(match.Round, Is.EqualTo(1));
            Assert.That(match.Gold(1), Is.EqualTo(130));
            Assert.That(match.Souls(1), Is.EqualTo(4));
            Assert.That(match.RemainingSeconds, Is.EqualTo(90));
            Assert.That(match.SetReady(1), Is.False);
            match.Advance(2);
            Assert.That(match.SetReady(1), Is.True);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
        }

        [Test]
        public void PartialReadinessReducesTimerButCannotRepeatVote()
        {
            var match = Create(8); match.Begin(); match.Advance(2);
            Assert.That(match.SetReady(1), Is.True);
            Assert.That(match.RemainingSeconds, Is.EqualTo(76.75));
            Assert.That(match.SetReady(1), Is.False);
            for (var i = 2; i <= 8; i++) match.SetReady(i);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
        }

        [Test]
        public void FirstWaveCountsAndXpFollowInitialParticipantCount()
        {
            for (var n = 1; n <= 8; n++)
            foreach (var casters in new[] { false, true })
            {
                var match = Create(n, casters: casters); match.Begin(); StartRound(match);
                Assert.That(match.RemainingCount, Is.EqualTo(9 * n + 31 + (casters ? 2 : 0)));
                Assert.That(match.Enemies.Count, Is.EqualTo(match.RemainingCount));
                Assert.That(match.ExperienceBudget, Is.EqualTo(507 / (3 * (3 * n + 11))));
            }
            Assert.That(Create(casters: false).RoundGold(1), Is.EqualTo(264));
            Assert.That(Create(difficulty: OriginalDifficulty.Easy).RoundGold(1), Is.EqualTo(601));
        }

        [Test]
        public void FramePartitionDoesNotChangeSpawnOrderOrPositions()
        {
            var whole = Create(2); whole.Begin(); whole.Advance(90); whole.DrainEvents();
            var partitioned = Create(2); partitioned.Begin(); partitioned.Advance(90); partitioned.DrainEvents();
            whole.Advance(2.1);
            for (var i = 0; i < 21; i++) partitioned.Advance(.1);
            var left = whole.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
            var right = partitioned.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
            Assert.That(right.Length, Is.EqualTo(left.Length));
            for (var i = 0; i < left.Length; i++)
            {
                Assert.That(right[i].rawcode, Is.EqualTo(left[i].rawcode));
                Assert.That(right[i].x, Is.EqualTo(left[i].x));
                Assert.That(right[i].y, Is.EqualTo(left[i].y));
                Assert.That(right[i].time, Is.EqualTo(left[i].time));
            }
        }

        [Test]
        public void NightmareSubstitutesEveryFourthTick()
        {
            var match = Create(difficulty: OriginalDifficulty.Nightmare); match.Begin(); StartRound(match);
            Assert.That(match.Enemies.Count(e => e.rawcode == "n00D"), Is.EqualTo(9));
            Assert.That(match.Enemies.Count(e => e.rawcode == "n008"), Is.EqualTo(30));
        }

        static System.Collections.Generic.IEnumerable<TestCaseData> NightmareCounterCases()
        {
            foreach (int round in new[] { 22, 29 })
                for (int participants = 1; participants <= 8; participants++)
                    foreach (bool casters in new[] { false, true })
                        yield return new TestCaseData(round, participants, casters);
        }

        static OriginalMatch TargetRound(int round, int participants, OriginalDifficulty difficulty, bool casters,
            bool finishSpawns = true)
        {
            var match = Create(participants, difficulty, casters); match.Begin();
            // Focus the round's real ready/start/spawn/death consumers without
            // forcing completion of the preceding twenty-eight rounds.
            typeof(OriginalMatch).GetProperty("Round").SetValue(match, round);
            match.Advance(2);
            for (int slot = 1; slot <= participants; slot++) Assert.That(match.SetReady(slot), Is.True);
            if (finishSpawns) match.Advance(2.00001);
            return match;
        }

        [TestCaseSource(nameof(NightmareCounterCases))]
        public void NightmareInvalidSubstitutionsHaveNoDeathDebt(int round, int participants, bool casters)
        {
            // Hand-derived from NGATE1: 13/16/19/22/25/28/31/34 attempts,
            // with 3 units excluded at each fourth attempt, plus 1/3 bosses.
            int[] budgetsWithoutCasters = { 31, 37, 46, 52, 58, 64, 73, 79 };
            int[] dormantCocoons = { 9, 12, 12, 15, 18, 21, 21, 24 };
            var match = TargetRound(round, participants, OriginalDifficulty.Nightmare, casters);
            int expected = budgetsWithoutCasters[participants - 1] + (casters ? 2 : 0);
            Assert.That(match.RemainingCount, Is.EqualTo(expected));
            Assert.That(match.Enemies.Count(e => e.counted), Is.EqualTo(expected));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
            if (round == 22)
            {
                var cocoons = match.Enemies.Where(e => e.rawcode == "u00L").ToArray();
                Assert.That(cocoons.Length, Is.EqualTo(dormantCocoons[participants - 1]));
                Assert.That(cocoons.All(e => !e.counted && !e.cocoon && e.cocoonTimer == 0), Is.True);
            }
            else
            {
                Assert.That(match.Enemies.Count, Is.EqualTo(expected), "No replacement or placeholder unit is invented for Er[30].");
                Assert.That(match.Enemies.Count(e => e.rawcode == "n0AU"), Is.EqualTo(expected - (casters ? 3 : 1)));
            }
            foreach (var enemy in match.Enemies.Where(e => e.counted).ToArray())
                Assert.That(match.EnemyKilled(enemy.entityId, false), Is.True);
            Assert.That(match.RemainingCount, Is.Zero);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.RoundTransition));
            match.Advance(3);
            Assert.That(match.Phase, Is.EqualTo(round == 29 && participants > 1 ?
                OriginalMatchPhase.DuelPreparation : OriginalMatchPhase.Preparation));
            Assert.That(match.Round, Is.EqualTo(round == 29 && participants > 1 ? round : round + 1));
        }

        [TestCaseSource(nameof(NightmareCounterCases))]
        public void StandardKeepsTheFullDeathBudgetInTheCorrectedNightmareRounds(int round, int participants, bool casters)
        {
            int[] budgetsWithoutCasters = { 40, 49, 58, 67, 76, 85, 94, 103 };
            var match = TargetRound(round, participants, OriginalDifficulty.Standard, casters);
            int expected = budgetsWithoutCasters[participants - 1] + (casters ? 2 : 0);
            Assert.That(match.RemainingCount, Is.EqualTo(expected));
            Assert.That(match.Enemies.Count, Is.EqualTo(expected));
            Assert.That(match.Enemies.All(e => e.counted), Is.True);
            Assert.That(OriginalMatch.CountedEnemies(round, participants, casters), Is.EqualTo(expected));
        }

        [Test]
        public void NightmareTwentyTwoSubstitutedCocoonsDoNotGainAnInventedHatchTimer()
        {
            var match = TargetRound(22, 3, OriginalDifficulty.Nightmare, true);
            match.Advance(31);
            Assert.That(match.Enemies.Count(e => e.rawcode == "u00L"), Is.EqualTo(12));
            Assert.That(match.Enemies.Any(e => e.rawcode == "n065" || e.rawcode == "n066"), Is.False);
            Assert.That(match.RemainingCount, Is.EqualTo(48));
        }

        [TestCase(22, 2, false)] [TestCase(22, 2, true)]
        [TestCase(22, 6, false)] [TestCase(22, 6, true)]
        [TestCase(29, 2, false)] [TestCase(29, 2, true)]
        [TestCase(29, 6, false)] [TestCase(29, 6, true)]
        public void NightmareFastKillsWaitForTheLastInvalidSpawnAttempt(int round, int participants, bool casters)
        {
            var match = TargetRound(round, participants, OriginalDifficulty.Nightmare, casters, false);
            match.Advance(1.999);
            Assert.That(match.Enemies.Any(e => e.sourceUserData == 1), Is.True, "The boss was already created before this final-attempt fixture.");
            foreach (var enemy in match.Enemies.Where(e => e.counted).ToArray())
                Assert.That(match.EnemyKilled(enemy.entityId, false), Is.True);
            Assert.That(match.RemainingCount, Is.Zero);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat), "The pending final source attempt still needs its coordinates and any uncounted unit.");
            match.Advance(.002);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.RoundTransition));
            Assert.That(match.RemainingCount, Is.Zero);
            if (round == 22)
                Assert.That(match.Enemies.Count(e => e.rawcode == "u00L"), Is.EqualTo(participants == 2 ? 12 : 21));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
        }

        [Test]
        public void SkippingMissingNightmareRosterPreservesTheNextValidGateCoordinates()
        {
            var standard = TargetRound(29, 3, OriginalDifficulty.Standard, true, false);
            var nightmare = TargetRound(29, 3, OriginalDifficulty.Nightmare, true, false);
            var spawn = typeof(OriginalMatch).GetMethod("SpawnNormalTick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            standard.DrainEvents(); nightmare.DrainEvents();
            spawn.Invoke(standard, new object[] { 4 }); spawn.Invoke(nightmare, new object[] { 4 });
            var skipped = nightmare.DrainEvents(); standard.DrainEvents();
            Assert.That(skipped.Any(e => e.kind == OriginalMatchEventKind.Spawn || e.kind == OriginalMatchEventKind.Unresolved), Is.False);
            spawn.Invoke(standard, new object[] { 5 }); spawn.Invoke(nightmare, new object[] { 5 });
            var left = standard.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
            var right = nightmare.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
            Assert.That(right.Length, Is.EqualTo(3));
            for (int gate = 0; gate < 3; gate++)
            {
                Assert.That(right[gate].rawcode, Is.EqualTo(left[gate].rawcode));
                Assert.That(right[gate].x, Is.EqualTo(left[gate].x));
                Assert.That(right[gate].y, Is.EqualTo(left[gate].y));
            }
        }

        [Test]
        public void DeathIsIdempotentAndXpIsSharedOnlyWithEligibleLivingHeroes()
        {
            var match = Create(3); match.RegisterHero(3, "H02E"); match.Begin(); StartRound(match);
            match.HeroDied(2);
            var enemy = match.Enemies.First(e => e.rawcode == "n008");
            Assert.That(match.EnemyKilled(enemy.entityId), Is.True);
            Assert.That(match.EnemyKilled(enemy.entityId), Is.False);
            Assert.That(match.Experience(1), Is.EqualTo(match.ExperienceBudget));
            Assert.That(match.Experience(2), Is.Zero);
            Assert.That(match.Experience(3), Is.Zero);
            Assert.That(match.Participants, Is.EqualTo(3));
            var second = match.Enemies.First();
            match.EnemyKilled(second.entityId, eligibleAlliedKill: false);
            Assert.That(match.Experience(1), Is.EqualTo(match.ExperienceBudget));
        }

        [Test]
        public void AllDeadWithoutAltarLosesAndCancelsFutureSpawns()
        {
            var match = Create(2); match.Begin(); match.Advance(90);
            match.HeroDied(1); Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
            match.HeroDied(2); Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Lost));
            match.DrainEvents(); match.Advance(100);
            Assert.That(match.Enemies, Is.Empty);
            Assert.That(match.DrainEvents(), Is.Empty);
        }

        [Test]
        public void AltarRetryPreservesSourceGoldSuppressionAndIndependentSouls()
        {
            var match = Create(difficulty: OriginalDifficulty.Easy); match.Begin(); StartRound(match);
            Assert.That(match.Altars, Is.EqualTo(1));
            match.HeroDied(1);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.AltarRecovery));
            match.Advance(9);
            Assert.That(match.Round, Is.EqualTo(1));
            Assert.That(match.Altars, Is.Zero);
            Assert.That(match.Gold(1), Is.EqualTo(130));
            Assert.That(match.Souls(1), Is.EqualTo(8));
            StartRound(match); KillAll(match); match.Advance(3);
            Assert.That(match.Round, Is.EqualTo(2));
            Assert.That(match.Gold(1), Is.EqualTo(130));
            Assert.That(match.Souls(1), Is.EqualTo(13));
            StartRound(match); KillAll(match); match.Advance(3);
            Assert.That(match.Gold(1), Is.GreaterThan(130));
        }

        [Test]
        public void WaveTwentyOneCountsAllThreeDescendantGenerations()
        {
            foreach (var n in new[] { 1, 5 })
            {
                var match = Create(n); ReachRound(match, 21); StartRound(match);
                var roots = match.Enemies.Where(e => e.rawcode == "n069").ToArray();
                Assert.That(roots.Length, Is.EqualTo(n == 1 ? 6 : 9));
                Assert.That(match.RemainingCount, Is.EqualTo((n == 1 ? 90 : 135) + 3));
                foreach (var root in roots) match.EnemyKilled(root.entityId);
                Assert.That(match.Enemies.Count(e => e.rawcode == "n06A"), Is.EqualTo(roots.Length * 2));
                KillAll(match);
                Assert.That(match.RemainingCount, Is.Zero);
                Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.RoundTransition));
            }
        }

        [Test]
        public void DestroyingCocoonProducesTenWeakWarriorsWithoutConsumingCounter()
        {
            var match = Create(); ReachRound(match, 23); StartRound(match);
            var cocoon = match.Enemies.First(e => e.cocoon);
            var before = match.RemainingCount;
            var experience = match.Experience(1);
            match.EnemyKilled(cocoon.entityId);
            Assert.That(match.RemainingCount, Is.EqualTo(before));
            Assert.That(match.Experience(1), Is.EqualTo(experience));
            Assert.That(match.Enemies.Count(e => e.rawcode == "n066"), Is.EqualTo(10));
            KillAll(match);
            Assert.That(match.RemainingCount, Is.Zero);
        }

        [Test]
        public void AcceleratorsAdvanceCocoonTimerAndProduceStrongWarriors()
        {
            var match = Create(); ReachRound(match, 23); StartRound(match);
            var cocoon = match.Enemies.First(e => e.cocoon);
            match.SetCocoonAccelerators(cocoon.entityId, 2);
            match.Advance(10);
            Assert.That(match.Enemies.Any(e => e.entityId == cocoon.entityId), Is.False);
            Assert.That(match.Enemies.Count(e => e.rawcode == "n065"), Is.EqualTo(10));
            Assert.That(match.RemainingCount, Is.EqualTo(43));
        }

        [Test]
        public void CocoonRetryKeepsGlobalSourceRegionIndexInsteadOfResettingIt()
        {
            var match = Create(difficulty: OriginalDifficulty.Easy); ReachRound(match, 23); StartRound(match);
            var original = match.Enemies.Where(e => e.cocoon).OrderBy(e => e.entityId).First();
            Assert.That(original.x, Is.EqualTo(-1024));
            Assert.That(original.y, Is.EqualTo(0));
            match.HeroDied(1); match.Advance(9); StartRound(match);
            var retried = match.Enemies.Where(e => e.cocoon).OrderBy(e => e.entityId).First();
            Assert.That(retried.x, Is.EqualTo(1728));
            Assert.That(retried.y, Is.EqualTo(1216));
            match.HeroDied(1); match.Advance(9); match.DrainEvents(); StartRound(match);
            Assert.That(match.Enemies.Count(e => e.cocoon), Is.EqualTo(4));
            Assert.That(match.Enemies.Where(e => e.cocoon).All(e => e.x == 0 && e.y == 0), Is.True);
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
            Assert.That(match.RemainingCount, Is.EqualTo(41));
            match.HeroDied(1); match.Advance(9); StartRound(match);
            Assert.That(match.Enemies.Count(e => e.cocoon), Is.EqualTo(4));
            Assert.That(match.Enemies.Where(e => e.cocoon).All(e => e.x == 0 && e.y == 0), Is.True,
                "Later altar retries keep increasing BD rather than cycling the eight original regions.");
            Assert.That((int)typeof(OriginalMatch).GetField("cocoonLocationIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(match), Is.EqualTo(16));
        }

        [TestCase(7, -1984f, 576f)]
        [TestCase(8, 0f, 0f)]
        [TestCase(9, 0f, 0f)]
        public void CocoonIndicesEightNineTenKeepNativeCoordinatesAndFirstCallback(int index, float x, float y)
        {
            var match = Create(); ReachRound(match, 23);
            typeof(OriginalMatch).GetField("cocoonLocationIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(match, index);
            match.Advance(2); Assert.That(match.SetReady(1), Is.True); match.Advance(.5);
            Assert.That(match.Enemies.Count(e => e.cocoon), Is.EqualTo(1));
            var cocoon = match.Enemies.Single(e => e.cocoon);
            Assert.That(cocoon.x, Is.EqualTo(x)); Assert.That(cocoon.y, Is.EqualTo(y));
            Assert.That(cocoon.rawcode, Is.EqualTo("u00L")); Assert.That(cocoon.counted, Is.False);
            Assert.That(cocoon.cocoonTimer, Is.EqualTo(30));
            match.Advance(1);
            Assert.That(cocoon.cocoonTimer, Is.EqualTo(29));
            Assert.That(cocoon.x, Is.EqualTo(x)); Assert.That(cocoon.y, Is.EqualTo(y));
            Assert.That(match.RemainingCount, Is.EqualTo(43));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
        }

        [TestCase(1, false)] [TestCase(1, true)]
        [TestCase(5, false)] [TestCase(5, true)]
        public void CocoonFallbackPreservesTheFullDestroyOrHatchBudget(int participants, bool hatch)
        {
            var match = Create(participants); ReachRound(match, 23);
            typeof(OriginalMatch).GetField("cocoonLocationIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(match, 8);
            StartRound(match);
            int count = participants == 1 ? 4 : 6, budget = participants == 1 ? 43 : 63;
            var cocoons = match.Enemies.Where(e => e.cocoon).ToArray();
            Assert.That(cocoons.Length, Is.EqualTo(count));
            Assert.That(cocoons.All(e => e.x == 0 && e.y == 0), Is.True);
            if (hatch) match.Advance(31);
            else foreach (var cocoon in cocoons) Assert.That(match.EnemyKilled(cocoon.entityId), Is.True);
            var children = match.Enemies.Where(e => e.rawcode == (hatch ? "n065" : "n066")).ToArray();
            Assert.That(children.Length, Is.EqualTo(count * 10));
            Assert.That(children.All(e => e.counted && e.x == 0 && e.y == 0), Is.True);
            Assert.That(match.RemainingCount, Is.EqualTo(budget));
            Assert.That(match.Enemies.Any(e => e.cocoon), Is.False);
            KillAll(match);
            Assert.That(match.RemainingCount, Is.Zero);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.RoundTransition));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
        }

        [Test]
        public void CocoonFallbackContinuesSafelyWhenTheGlobalIndexOverflows()
        {
            var match = Create(); ReachRound(match, 23);
            var index = typeof(OriginalMatch).GetField("cocoonLocationIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            index.SetValue(match, int.MaxValue);
            StartRound(match);
            Assert.That(match.Enemies.Count(e => e.cocoon), Is.EqualTo(4));
            Assert.That(match.Enemies.Where(e => e.cocoon).All(e => e.x == 0 && e.y == 0), Is.True);
            Assert.That((int)index.GetValue(match), Is.EqualTo(unchecked(int.MaxValue + 4)));
            Assert.That(match.RemainingCount, Is.EqualTo(43));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved), Is.False);
        }

        [Test]
        public void MultiplayerBoundaryRequiresPairsThenGladiatorBeforeMega()
        {
            var match = Create(2); ReachRound(match, 4); StartRound(match); KillAll(match); match.Advance(3);
            Assert.That(match.Round, Is.EqualTo(4));
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            match.Advance(25);
            Assert.That(match.DuelKind, Is.EqualTo(OriginalDuelKind.Pairs));
            match.CompleteDuelSequence();
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            match.Advance(25);
            Assert.That(match.DuelKind, Is.EqualTo(OriginalDuelKind.Gladiator));
            match.CompleteDuelSequence();
            Assert.That(match.Round, Is.EqualTo(5));
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Preparation));
        }

        [Test]
        public void StandardMegaAwardsAltarOnlyWhenAllSurvive()
        {
            var survivors = Create(2); ReachRound(survivors, 5); StartRound(survivors); KillAll(survivors);
            Assert.That(survivors.Altars, Is.EqualTo(1));
            var casualty = Create(2); ReachRound(casualty, 5); StartRound(casualty);
            casualty.HeroDied(1); KillAll(casualty);
            Assert.That(casualty.Altars, Is.Zero);
        }

        [Test]
        public void MegaEntryUsesPerPlayerOriginalRegionAndPausedCountdown()
        {
            var match = Create(8); ReachRound(match, 5); match.DrainEvents();
            match.Advance(2);
            for (var slot = 1; slot <= 8; slot++) match.SetReady(slot);
            var emitted = match.DrainEvents();
            var teleports = emitted.Where(e => e.kind == OriginalMatchEventKind.TeleportParty).ToArray();
            Assert.That(teleports.Select(e => e.slot), Is.EquivalentTo(Enumerable.Range(1, 8)));
            foreach (var teleport in teleports)
            {
                Assert.That(teleport.x, Is.InRange(-224, 224));
                Assert.That(teleport.y, Is.InRange(-3360, -3136));
            }
            Assert.That(emitted.Any(e => e.kind == OriginalMatchEventKind.PartyPause && e.enabled), Is.True);
            Assert.That(match.EnemyKilled(match.FinalBossEntityId), Is.False);
            match.Advance(5);
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.PartyPause && !e.enabled), Is.True);
        }

        [Test]
        public void FinalBossRemovesAltarsAndCanDieBeforeFirstPhaseAsSourceAllows()
        {
            var match = Create(); ReachRound(match, 30);
            Assert.That(match.Altars, Is.EqualTo(5));
            StartRound(match);
            Assert.That(match.Altars, Is.Zero);
            Assert.That(match.EnemyKilled(match.FinalBossEntityId), Is.True);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Won));
        }

        [Test]
        public void SourceUserDataIsAssignedByNormalAndMegaSpawnPathsAndCopiedIntoEvents()
        {
            var match = Create(casters: true); match.Begin(); StartRound(match);
            Assert.That(match.Enemies.Count(e => e.sourceUserData == 1), Is.EqualTo(1));
            Assert.That(match.Enemies.Single(e => e.sourceUserData == 1).rawcode, Is.EqualTo("n009"));
            Assert.That(match.Enemies.Where(e => e.rawcode == "n008" || e.rawcode == "n05J").All(e => e.sourceUserData == 0), Is.True);
            foreach (var spawn in match.DrainEvents().Where(e => e.kind == OriginalMatchEventKind.Spawn))
                Assert.That(spawn.sourceUserData, Is.EqualTo(match.Enemies.Single(e => e.entityId == spawn.entityId).sourceUserData));
            var mega = Create(); ReachRound(mega, 5); mega.DrainEvents(); StartRound(mega);
            Assert.That(mega.Enemies.Single().sourceUserData, Is.EqualTo(2));
            Assert.That(mega.DrainEvents().Single(e => e.kind == OriginalMatchEventKind.Spawn).sourceUserData, Is.EqualTo(2));
        }

        [Test]
        public void FinalPhasesAreSequentialAndRequireSpawnCompletionAndDeathCallback()
        {
            var match = Create(); ReachRound(match, 30); StartRound(match);
            Assert.That(match.Enemies.Single(e => e.megaBoss).sourceUserData, Is.EqualTo(2));
            var thresholds = new[] { .75, .55, .35 };
            var counts = new[] { 16, 2, 2 };
            for (var phase = 0; phase < 3; phase++)
            {
                match.SetBossHealthFraction(thresholds[phase]); match.Advance(1);
                Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.FinalIntermission));
                Assert.That(match.EnemyKilled(match.FinalBossEntityId), Is.False);
                match.Advance(60);
                Assert.That(match.Enemies.Count(e => e.finalAdd), Is.EqualTo(counts[phase]));
                Assert.That(match.Enemies.Where(e => e.finalAdd).All(e => e.sourceUserData == (phase == 0 ? 1 : 2)), Is.True);
                foreach (var add in match.Enemies.Where(e => e.finalAdd).ToArray()) match.EnemyKilled(add.entityId);
                Assert.That(match.FinalStage, Is.EqualTo(phase + 1));
                Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
            }
            match.EnemyKilled(match.FinalBossEntityId);
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Won));
        }

        [Test]
        public void FinalSeriesFastKillsDoNotInventAutomaticResumeMissingFromSource()
        {
            var match = Create(); ReachRound(match, 30); StartRound(match);
            match.SetBossHealthFraction(.75); match.Advance(1);
            for (var i = 0; i < 110; i++)
            {
                match.Advance(.5);
                foreach (var add in match.Enemies.Where(e => e.finalAdd).ToArray()) match.EnemyKilled(add.entityId);
            }
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.FinalIntermission));
            Assert.That(match.DrainEvents().Any(e => e.kind == OriginalMatchEventKind.Unresolved &&
                e.sourceRule == "final-series-complete-after-last-death"), Is.False);
            match.ObserveUncountedEnemyDeath();
            Assert.That(match.Phase, Is.EqualTo(OriginalMatchPhase.Combat));
        }
    }
}

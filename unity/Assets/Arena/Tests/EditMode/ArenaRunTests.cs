using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class ArenaRunTests
    {
        [Test]
        public void StartWave_PreparesFirstWaveAndCannotBeRepeated()
        {
            var run = new ArenaRun();

            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
            Assert.That(run.Wave, Is.EqualTo(1));
            Assert.That(run.PendingEnemies, Is.EqualTo(6));
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(run.PendingEnemies, Is.EqualTo(6));
        }

        [Test]
        public void ReadyRun_RejectsCombatAndUpgradeActions()
        {
            var run = new ArenaRun();

            Assert.That(run.SpawnEnemy(), Is.False);
            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.TakeDamage(10f), Is.False);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.False);
            AssertReady(run);
        }

        [Test]
        public void ExplorationPauseMustBeResumedBeforeStartingCombat()
        {
            var run = new ArenaRun();
            run.SetPaused(true);
            Assert.That(run.IsPaused, Is.True);
            Assert.That(run.StartWave(), Is.False);
            run.SetPaused(false);
            Assert.That(run.StartWave(), Is.True);
        }

        [Test]
        public void SpawnEnemy_TransfersPendingEnemyToAliveAndRejectsExtraSpawns()
        {
            var run = BeginRun();

            Assert.That(run.SpawnEnemy(), Is.True);
            Assert.That(run.PendingEnemies, Is.EqualTo(5));
            Assert.That(run.AliveEnemies, Is.EqualTo(1));
            for (var i = 0; i < 5; i++)
                Assert.That(run.SpawnEnemy(), Is.True);

            Assert.That(run.SpawnEnemy(), Is.False);
            Assert.That(run.PendingEnemies, Is.Zero);
            Assert.That(run.AliveEnemies, Is.EqualTo(6));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
        }

        [Test]
        public void EnemyDefeated_RejectsDeathWithoutLivingEnemy()
        {
            var run = BeginRun();

            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.PendingEnemies, Is.EqualTo(6));
            Assert.That(run.Kills, Is.Zero);
        }

        [Test]
        public void LastLivingEnemy_DoesNotEndWaveWhileSpawnsRemain()
        {
            var run = BeginRun();
            Assert.That(run.SpawnEnemy(), Is.True);

            Assert.That(run.EnemyDefeated(), Is.True);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.PendingEnemies, Is.EqualTo(5));
            Assert.That(run.Kills, Is.EqualTo(1));
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.False);
        }

        [Test]
        public void WaveFinishesOnlyAfterAllSpawnedEnemiesDie()
        {
            var run = BeginRun();
            for (var i = 0; i < 6; i++)
                Assert.That(run.SpawnEnemy(), Is.True);
            for (var i = 0; i < 5; i++)
                Assert.That(run.EnemyDefeated(), Is.True);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
            Assert.That(run.AliveEnemies, Is.EqualTo(1));
            Assert.That(run.EnemyDefeated(), Is.True);
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Upgrade));
            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.SpawnEnemy(), Is.False);
            Assert.That(run.Kills, Is.EqualTo(6));
        }

        [TestCase(80f, 85f)]
        [TestCase(10f, 140f)]
        public void WaveBreak_HealsOnceWithoutExceedingMaximum(float damage, float expectedHealth)
        {
            var run = BeginRun();
            Assert.That(run.TakeDamage(damage), Is.True);

            FinishWave(run);

            Assert.That(run.Health, Is.EqualTo(expectedHealth));
            Assert.That(run.TakeDamage(5f), Is.False);
            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.Health, Is.EqualTo(expectedHealth));
        }

        [Test]
        public void PowerChoice_AppliesOnceAndPreparesNextWave()
        {
            var run = BeginRun();
            FinishWave(run);

            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.True);

            Assert.That(run.Damage, Is.EqualTo(32f));
            Assert.That(run.Wave, Is.EqualTo(2));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
            Assert.That(run.PendingEnemies, Is.EqualTo(9));
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.False);
            Assert.That(run.Damage, Is.EqualTo(32f));
            Assert.That(run.PendingEnemies, Is.EqualTo(9));
        }

        [Test]
        public void VitalityChoice_IncreasesCurrentAndMaximumHealth()
        {
            var run = BeginRun();
            Assert.That(run.TakeDamage(80f), Is.True);
            FinishWave(run);

            Assert.That(run.ChooseUpgrade(UpgradeKind.Vitality), Is.True);

            Assert.That(run.MaxHealth, Is.EqualTo(175f));
            Assert.That(run.Health, Is.EqualTo(120f));
            Assert.That(run.TakeDamage(84f), Is.True);
            FinishWave(run);
            Assert.That(run.Health, Is.EqualTo(61f));
        }

        [Test]
        public void FinalVitalityChoice_ImmediatelyHelpsSurviveTheBoss()
        {
            var run = BeginRun();
            for (var i = 0; i < 3; i++)
            {
                FinishWave(run);
                Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.True);
            }
            Assert.That(run.TakeDamage(100f), Is.True);
            FinishWave(run);

            Assert.That(run.ChooseUpgrade(UpgradeKind.Vitality), Is.True);

            Assert.That(run.Wave, Is.EqualTo(5));
            Assert.That(run.MaxHealth, Is.EqualTo(175f));
            Assert.That(run.Health, Is.EqualTo(100f));
            Assert.That(run.TakeDamage(99f), Is.True);
            Assert.That(run.Health, Is.EqualTo(1f));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
        }

        [Test]
        public void HasteChoices_StackAcrossAvailableWaveBreaks()
        {
            var run = BeginRun();
            for (var i = 0; i < 4; i++)
            {
                FinishWave(run);
                Assert.That(run.ChooseUpgrade(UpgradeKind.Haste), Is.True);
            }

            Assert.That(run.MoveSpeed, Is.EqualTo(8.4f).Within(0.0001f));
            Assert.That(run.AttackCooldown, Is.EqualTo(0.2871034f).Within(0.0001f));
            Assert.That(run.AttackCooldown, Is.GreaterThanOrEqualTo(0.22f));
        }

        [TestCase(-1)]
        [TestCase(3)]
        [TestCase(int.MaxValue)]
        public void InvalidUpgrade_DoesNotConsumeTheChoice(int value)
        {
            var run = BeginRun();
            FinishWave(run);

            Assert.That(run.ChooseUpgrade((UpgradeKind)value), Is.False);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Upgrade));
            Assert.That(run.Wave, Is.EqualTo(1));
            Assert.That(run.PendingEnemies, Is.Zero);
            Assert.That(run.Damage, Is.EqualTo(24f));
            Assert.That(run.MaxHealth, Is.EqualTo(140f));
            Assert.That(run.MoveSpeed, Is.EqualTo(6f));
            Assert.That(run.AttackCooldown, Is.EqualTo(0.55f));
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.True);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidDamage_DoesNotCorruptHealth(float amount)
        {
            var run = BeginRun();

            Assert.That(run.TakeDamage(amount), Is.False);

            Assert.That(run.Health, Is.EqualTo(140f));
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Wave));
        }

        [Test]
        public void Pause_BlocksDamageUntilResumed()
        {
            var run = BeginRun();
            run.SetPaused(true);

            Assert.That(run.IsPaused, Is.True);
            Assert.That(run.TakeDamage(20f), Is.False);
            Assert.That(run.Health, Is.EqualTo(140f));
            run.SetPaused(false);
            Assert.That(run.TakeDamage(20f), Is.True);
            Assert.That(run.Health, Is.EqualTo(120f));
        }

        [TestCase(140f)]
        [TestCase(200f)]
        [TestCase(float.MaxValue)]
        public void LethalDamage_ClampsHealthAndMakesLossTerminal(float damage)
        {
            var run = BeginRun();
            Assert.That(run.SpawnEnemy(), Is.True);

            Assert.That(run.TakeDamage(damage), Is.True);

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Lost));
            Assert.That(run.Health, Is.Zero);
            Assert.That(run.TakeDamage(1f), Is.False);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(run.SpawnEnemy(), Is.False);
            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.False);
            run.SetPaused(true);
            Assert.That(run.IsPaused, Is.False);
            Assert.That(run.AliveEnemies, Is.EqualTo(1));
            Assert.That(run.PendingEnemies, Is.EqualTo(5));
            Assert.That(run.Kills, Is.Zero);
        }

        [Test]
        public void FiveCompletedWaves_EndWithVictoryInsteadOfAnotherUpgrade()
        {
            var run = BeginRun();
            var expectedWaves = new[] { 6, 9, 12, 15, 1 };

            for (var i = 0; i < expectedWaves.Length; i++)
            {
                Assert.That(run.PendingEnemies, Is.EqualTo(expectedWaves[i]));
                Assert.That(run.Wave, Is.EqualTo(i + 1));
                FinishWave(run);
                if (i < 4)
                {
                    Assert.That(run.Phase, Is.EqualTo(RunPhase.Upgrade));
                    Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.True);
                }
            }

            Assert.That(run.Phase, Is.EqualTo(RunPhase.Won));
            Assert.That(run.Kills, Is.EqualTo(43));
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.PendingEnemies, Is.Zero);
            Assert.That(run.StartWave(), Is.False);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.False);
            Assert.That(run.TakeDamage(200f), Is.False);
            Assert.That(run.EnemyDefeated(), Is.False);
            Assert.That(run.SpawnEnemy(), Is.False);
            run.SetPaused(true);
            Assert.That(run.IsPaused, Is.False);
        }

        [Test]
        public void Reset_ClearsUpgradesDamageKillsAndPauseForAnotherRun()
        {
            var run = BeginRun();
            FinishWave(run);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Power), Is.True);
            FinishWave(run);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Vitality), Is.True);
            FinishWave(run);
            Assert.That(run.ChooseUpgrade(UpgradeKind.Haste), Is.True);
            Assert.That(run.SpawnEnemy(), Is.True);
            Assert.That(run.TakeDamage(40f), Is.True);
            run.SetPaused(true);

            run.Reset();

            AssertReady(run);
            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.Wave, Is.EqualTo(1));
            Assert.That(run.PendingEnemies, Is.EqualTo(6));
        }

        [Test]
        public void Reset_AfterDeathMakesAnotherRunPlayable()
        {
            var run = BeginRun();
            Assert.That(run.TakeDamage(200f), Is.True);

            run.Reset();

            AssertReady(run);
            Assert.That(run.StartWave(), Is.True);
            Assert.That(run.SpawnEnemy(), Is.True);
            Assert.That(run.TakeDamage(10f), Is.True);
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(6)]
        [TestCase(int.MaxValue)]
        public void InvalidWaveIndex_HasNoSpawnBudget(int wave)
        {
            Assert.That(ArenaRun.EnemiesForWave(wave), Is.Zero);
        }

        private static ArenaRun BeginRun()
        {
            var run = new ArenaRun();
            Assert.That(run.StartWave(), Is.True);
            return run;
        }

        private static void FinishWave(ArenaRun run)
        {
            var pending = run.PendingEnemies;
            Assert.That(pending, Is.InRange(0, 15));
            for (var i = 0; i < pending; i++)
                Assert.That(run.SpawnEnemy(), Is.True);
            var alive = run.AliveEnemies;
            Assert.That(alive, Is.InRange(1, 15));
            for (var i = 0; i < alive; i++)
                Assert.That(run.EnemyDefeated(), Is.True);
        }

        private static void AssertReady(ArenaRun run)
        {
            Assert.That(run.Phase, Is.EqualTo(RunPhase.Ready));
            Assert.That(run.Wave, Is.Zero);
            Assert.That(run.PendingEnemies, Is.Zero);
            Assert.That(run.AliveEnemies, Is.Zero);
            Assert.That(run.Kills, Is.Zero);
            Assert.That(run.Health, Is.EqualTo(140f));
            Assert.That(run.MaxHealth, Is.EqualTo(140f));
            Assert.That(run.Damage, Is.EqualTo(24f));
            Assert.That(run.MoveSpeed, Is.EqualTo(6f));
            Assert.That(run.AttackCooldown, Is.EqualTo(0.55f));
            Assert.That(run.IsPaused, Is.False);
        }
    }
}

using System;

namespace Arena
{
    public enum RunPhase { Ready, Wave, Upgrade, Won, Lost }
    public enum UpgradeKind { Power, Vitality, Haste }

    public sealed class ArenaRun
    {
        public const int TotalWaves = 5;

        public RunPhase Phase { get; private set; }
        public int Wave { get; private set; }
        public int PendingEnemies { get; private set; }
        public int AliveEnemies { get; private set; }
        public int Kills { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Damage { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackCooldown { get; private set; }
        public bool IsPaused { get; private set; }

        public ArenaRun()
        {
            Reset();
        }

        public void Reset()
        {
            Phase = RunPhase.Ready;
            Wave = 0;
            PendingEnemies = 0;
            AliveEnemies = 0;
            Kills = 0;
            Health = 140f;
            MaxHealth = 140f;
            Damage = 24f;
            MoveSpeed = 6f;
            AttackCooldown = 0.55f;
            IsPaused = false;
        }

        public bool StartWave()
        {
            if (Phase != RunPhase.Ready || IsPaused)
                return false;

            PrepareNextWave();
            return true;
        }

        public bool SpawnEnemy()
        {
            if (Phase != RunPhase.Wave || PendingEnemies <= 0)
                return false;

            PendingEnemies--;
            AliveEnemies++;
            return true;
        }

        public bool EnemyDefeated()
        {
            if (Phase != RunPhase.Wave || AliveEnemies <= 0)
                return false;

            AliveEnemies--;
            Kills++;
            if (PendingEnemies == 0 && AliveEnemies == 0)
            {
                if (Wave == TotalWaves)
                {
                    Phase = RunPhase.Won;
                    IsPaused = false;
                }
                else
                {
                    Phase = RunPhase.Upgrade;
                    Health = Math.Min(MaxHealth, Health + 25f);
                }
            }
            return true;
        }

        public bool ChooseUpgrade(UpgradeKind upgrade)
        {
            if (Phase != RunPhase.Upgrade)
                return false;

            switch (upgrade)
            {
                case UpgradeKind.Power:
                    Damage += 8f;
                    break;
                case UpgradeKind.Vitality:
                    MaxHealth += 35f;
                    Health = Math.Min(MaxHealth, Health + 35f);
                    break;
                case UpgradeKind.Haste:
                    MoveSpeed += 0.6f;
                    AttackCooldown = Math.Max(0.22f, AttackCooldown * 0.85f);
                    break;
                default:
                    return false;
            }

            PrepareNextWave();
            return true;
        }

        public bool TakeDamage(float amount)
        {
            if (Phase != RunPhase.Wave || IsPaused || amount <= 0f
                || float.IsNaN(amount) || float.IsInfinity(amount))
                return false;

            Health = Math.Max(0f, Health - amount);
            if (Health == 0f)
            {
                Phase = RunPhase.Lost;
                IsPaused = false;
            }
            return true;
        }

        public void SetPaused(bool paused)
        {
            if (Phase == RunPhase.Ready || Phase == RunPhase.Wave || Phase == RunPhase.Upgrade)
                IsPaused = paused;
        }

        public static int EnemiesForWave(int wave)
        {
            switch (wave)
            {
                case 1: return 6;
                case 2: return 9;
                case 3: return 12;
                case 4: return 15;
                case 5: return 1;
                default: return 0;
            }
        }

        private void PrepareNextWave()
        {
            Wave++;
            PendingEnemies = EnemiesForWave(Wave);
            AliveEnemies = 0;
            Phase = RunPhase.Wave;
        }
    }
}

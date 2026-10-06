using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalMatchPhase
    {
        Configuration, Preparation, Combat, BossCountdown, FinalIntermission,
        RoundTransition, DuelPreparation, Duel, AltarRecovery, Won, Lost
    }

    public enum OriginalDuelKind { Pairs, Gladiator }

    public enum OriginalMatchEventKind
    {
        PhaseChanged, Spawn, Remove, Gold, Souls, Experience, RestoreParty,
        HeroDied, ShopAccess, TeleportParty, PartyPause, BossPause, BossResume, BossRegeneration,
        BossScaling, FinalAddScaling, OrderRefresh, DuelRequired, AltarsChanged,
        Won, Lost, Unresolved
    }

    // Coordinates use the original Warcraft XY plane, not Unity metres.
    [Serializable]
    public sealed class OriginalMatchEvent
    {
        public long sequence;
        public double time;
        public OriginalMatchEventKind kind;
        public int round;
        public int entityId;
        public string rawcode;
        public int slot;
        public int amount;
        public int secondaryAmount;
        public int attackGroup;
        public int sourceUserData;
        public float x;
        public float y;
        public float facing;
        public bool enabled;
        public string sourceRule;
    }

    [Serializable]
    public sealed class OriginalMatchEnemy
    {
        public int entityId;
        public string rawcode;
        public float x;
        public float y;
        public bool counted;
        public bool finalAdd;
        public bool cocoon;
        public bool megaBoss;
        public int cocoonTimer;
        public int nearbyAccelerators;
        // EMv assigns the three gate groups. ETv and descendants use GD (0)
        // regardless of their spawn gate. E5v refreshes each group's point order.
        public int attackGroup;
        // Native SetUnitUserData belongs to the spawn path, not the rawcode:
        // normal boss=1, mega/four mega final adds=2, ordinary units=0.
        public int sourceUserData;
    }

    /// <summary>
    /// Authoritative survival sequencing derived from 3.9c declarations.
    /// Combat, native ability effects, item economy and duel combat are consumers
    /// of these events. This class does not claim to reproduce the Warcraft engine.
    /// </summary>
    public sealed class OriginalMatch
    {
        sealed class Scheduled
        {
            public double time;
            public long order;
            public Action action;
        }

        readonly OriginalMatchCatalog catalog;
        readonly OriginalMatchOptions options;
        readonly List<OriginalMatchEvent> events = new List<OriginalMatchEvent>();
        readonly List<Scheduled> schedule = new List<Scheduled>();
        readonly Dictionary<int, OriginalMatchEnemy> enemies = new Dictionary<int, OriginalMatchEnemy>();
        readonly bool[] alive;
        readonly bool[] ready;
        readonly string[] heroIds;
        readonly int[] gold;
        readonly int[] souls;
        readonly int[] xp;
        uint random;
        long eventSequence;
        long scheduleSequence;
        int entitySequence;
        int generation;
        int readyCount;
        int bossId;
        int cocoonLocationIndex;
        int finalStage;
        int pendingNormalSpawnTicks;
        bool finalSeriesComplete;
        bool rewardsEnabled = true;
        bool duelCompleted;
        bool combatTimerActive;
        int speedBonus;
        double preparationStarted;
        double preparationDeadline;
        double combatStarted;
        double bossFraction = 1;

        public OriginalMatchPhase Phase { get; private set; } = OriginalMatchPhase.Configuration;
        public int Round { get; private set; }
        public int Participants { get; }
        public double Clock { get; private set; }
        public int RemainingCount { get; private set; }
        public int Altars { get; private set; }
        public int ExperienceBudget { get; private set; }
        public int FinalStage => finalStage;
        public int FinalBossEntityId => bossId;
        public OriginalDuelKind DuelKind { get; private set; }
        public IReadOnlyCollection<OriginalMatchEnemy> Enemies => enemies.Values;
        public double RemainingSeconds => Phase == OriginalMatchPhase.Preparation ? Math.Max(0, preparationDeadline - Clock) :
            (schedule.Count == 0 ? 0 : Math.Max(0, schedule[0].time - Clock));

        public OriginalMatch(OriginalMatchCatalog catalog, OriginalMatchOptions options, int participants, int seed)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (participants < 1 || participants > 8) throw new ArgumentOutOfRangeException(nameof(participants));
            catalog.Validate(); options.Validate();
            this.catalog = catalog;
            this.options = options.Copy();
            Participants = participants;
            random = unchecked((uint)seed);
            if (random == 0) random = 0x6D2B79F5;
            alive = new bool[participants]; ready = new bool[participants];
            heroIds = new string[participants]; gold = new int[participants];
            souls = new int[participants]; xp = new int[participants];
            for (var i = 0; i < participants; i++) alive[i] = true;
            Altars = options.difficulty == OriginalDifficulty.Easy ? 1 : 0;
        }

        public void RegisterHero(int slot, string rawcode)
        {
            ValidateSlot(slot);
            if (Phase != OriginalMatchPhase.Configuration) throw new InvalidOperationException("Hero registry is locked after Begin.");
            if (string.IsNullOrEmpty(rawcode) || rawcode.Length != 4) throw new ArgumentException("Expected original hero rawcode.");
            heroIds[slot - 1] = rawcode;
        }

        public int Gold(int slot) { ValidateSlot(slot); return gold[slot - 1]; }
        public int Souls(int slot) { ValidateSlot(slot); return souls[slot - 1]; }
        public int Experience(int slot) { ValidateSlot(slot); return xp[slot - 1]; }
        public bool IsAlive(int slot) { ValidateSlot(slot); return alive[slot - 1]; }
        public bool IsReady(int slot)
        {
            ValidateSlot(slot);
            return Phase == OriginalMatchPhase.Preparation && ready[slot - 1];
        }

        public bool Begin()
        {
            if (Phase != OriginalMatchPhase.Configuration) return false;
            GiveAll(OriginalMatchEventKind.Gold, 100, "participants");
            PrepareNextRound();
            return true;
        }

        // The host calls this once for each simulation tick. Scheduling uses absolute
        // time so different render rates cannot change spawn counts or timer order.
        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            var target = Clock + seconds;
            if (double.IsInfinity(target)) throw new ArgumentOutOfRangeException(nameof(seconds));
            while (schedule.Count > 0 && schedule[0].time <= target)
            {
                var task = schedule[0]; schedule.RemoveAt(0);
                Clock = task.time;
                task.action();
            }
            Clock = target;
        }

        public OriginalMatchEvent[] DrainEvents()
        {
            var result = events.ToArray(); events.Clear(); return result;
        }

        public bool SetReady(int slot)
        {
            ValidateSlot(slot);
            if (Phase != OriginalMatchPhase.Preparation || ready[slot - 1] ||
                Clock < preparationStarted + 2 || Clock > preparationDeadline - .5) return false;
            ready[slot - 1] = true; readyCount++;
            var roundedRemaining = (int)(preparationDeadline - Clock + .5);
            if (roundedRemaining >= 15)
                preparationDeadline = Clock + Math.Max(15, roundedRemaining - PreparationSeconds(Round) / (double)Participants);
            if (readyCount == Participants) StartCombat();
            else SchedulePreparationCheck();
            return true;
        }

        public bool HeroDied(int slot)
        {
            ValidateSlot(slot);
            if (!IsBattlePhase() || !alive[slot - 1]) return false;
            alive[slot - 1] = false;
            Emit(OriginalMatchEventKind.HeroDied, "death-and-wipe", slot: slot);
            foreach (var heroAlive in alive) if (heroAlive) return true;
            ResetSchedule();
            RemoveAllEnemies();
            if (Altars <= 0)
            {
                SetPhase(OriginalMatchPhase.Lost);
                Emit(OriginalMatchEventKind.Lost, "death-and-wipe");
            }
            else
            {
                Altars--;
                Emit(OriginalMatchEventKind.AltarsChanged, "death-and-wipe", amount: Altars);
                SetPhase(OriginalMatchPhase.AltarRecovery);
                Schedule(9, () =>
                {
                    Round--;
                    rewardsEnabled = false;
                    duelCompleted = IsDuelBoundary(Round);
                    PrepareNextRound();
                });
            }
            return true;
        }

        // Source death callbacks spawn descendants at GetUnitX/Y of the dying
        // unit. Its initial gate coordinate is no longer valid after movement.
        public bool SetEnemyPosition(int entityId, float x, float y)
        {
            if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(y) || float.IsInfinity(y) ||
                Math.Abs(x) > 1048576 || Math.Abs(y) > 1048576) throw new ArgumentOutOfRangeException();
            if (!enemies.TryGetValue(entityId, out var enemy)) return false;
            enemy.x = x; enemy.y = y; return true;
        }

        public bool EnemyKilled(int entityId, bool eligibleAlliedKill = true)
        {
            if (!IsBattlePhase() || !enemies.TryGetValue(entityId, out var enemy)) return false;
            if (enemyIdPaused(enemy)) return false;
            enemies.Remove(entityId);
            Emit(OriginalMatchEventKind.Remove, "death-and-wipe", enemy: enemy);
            if (eligibleAlliedKill) AwardExperience(enemy);
            if (enemy.cocoon)
            {
                SpawnCocoonChildren(enemy, false);
                return true;
            }
            if (Round == 21)
            {
                var child = enemy.rawcode == "n069" ? "n06A" : enemy.rawcode == "n06A" ? "n06B" : enemy.rawcode == "n06B" ? "n06C" : null;
                if (child != null)
                    for (var i = 0; i < 2; i++) Spawn(child, enemy.x, enemy.y, 0, true);
            }
            if (enemy.finalAdd)
            {
                TryResumeFinalBoss();
                return true;
            }
            if (enemy.megaBoss)
            {
                if (Round == 30)
                {
                    ResetSchedule(); RemoveAllEnemies(); SetPhase(OriginalMatchPhase.Won);
                    Emit(OriginalMatchEventKind.Won, "final-phases");
                }
                else CompleteMega();
                return true;
            }
            if (enemy.counted)
            {
                RemainingCount--;
                TryFinishOrdinaryRound();
            }
            return true;
        }

        public bool SetCocoonAccelerators(int entityId, int livingNearbyAccelerators)
        {
            if (livingNearbyAccelerators < 0) throw new ArgumentOutOfRangeException(nameof(livingNearbyAccelerators));
            if (!enemies.TryGetValue(entityId, out var enemy) || !enemy.cocoon) return false;
            enemy.nearbyAccelerators = livingNearbyAccelerators;
            return true;
        }

        public void SetBossHealthFraction(double fraction)
        {
            if (double.IsNaN(fraction) || fraction < 0 || fraction > 1) throw new ArgumentOutOfRangeException(nameof(fraction));
            bossFraction = fraction;
        }

        // CA is a global death callback in the source, including deaths not owned
        // by this wave roster. Keep that explicit for phase-series edge cases.
        public void ObserveUncountedEnemyDeath()
        {
            TryResumeFinalBoss();
        }

        // Duel selection, wagers and combat belong to the separate duel controller.
        // No winner or rewards are fabricated while that controller is unavailable.
        public bool CompleteDuelSequence()
        {
            if (Phase != OriginalMatchPhase.Duel) return false;
            if (DuelKind == OriginalDuelKind.Pairs) DuelKind = OriginalDuelKind.Gladiator;
            else duelCompleted = true;
            PrepareNextRound();
            return true;
        }

        public static int PreparationSeconds(int round)
        {
            if (round < 1 || round > 30) throw new ArgumentOutOfRangeException(nameof(round));
            return round == 1 || round == 25 ? 90 : round <= 4 ? 55 : 45;
        }

        public static int SpawnTicks(int participants) => 3 * participants + 10;

        public static int CountedEnemies(int round, int participants, bool casters)
        {
            if (round < 1 || round > 30 || participants < 1 || participants > 8) throw new ArgumentOutOfRangeException();
            if (round % 5 == 0) return 1;
            var extra = casters ? 3 : 1;
            if (round == 21) return (participants <= 4 ? 90 : 135) + extra;
            if (round == 23) return (participants <= 4 ? 40 : 60) + extra;
            return 3 * SpawnTicks(participants) + extra;
        }

        int NormalRosterRound(int tick)
        {
            if (options.difficulty != OriginalDifficulty.Nightmare || tick % 4 != 0 || Round == 24) return Round;
            return Round + (Round == 4 || Round == 9 || Round == 14 || Round == 19 ? 2 : 1);
        }

        int InitialDeathBudget()
        {
            int total = CountedEnemies(Round, Participants, options.casters);
            if (options.difficulty != OriginalDifficulty.Nightmare || Round != 22 && Round != 29) return total;
            // Host correction of Ezv/EMv's original count defect. NGATE1
            // confirms A0K4 deaths are excluded and CreateUnit(rawcode0)
            // creates no death. Keep the public source baseline unchanged.
            for (int tick = 1; tick <= SpawnTicks(Participants); tick++)
            {
                string id = catalog.Wave(NormalRosterRound(tick)).regularId;
                if (string.IsNullOrEmpty(id) || catalog.Enemy(id).HasAbility("A0K4")) total -= 3;
            }
            return total;
        }

        void TryFinishOrdinaryRound()
        {
            // The corrected budget can reach zero before the last invalid
            // attempt (parties2/6). Preserve every planned coordinate draw and
            // the uncounted22 units before entering the normal transition.
            if (RemainingCount == 0 && pendingNormalSpawnTicks == 0 && Phase == OriginalMatchPhase.Combat)
                FinishRound(3);
        }

        public int RoundGold(int completedRound, bool eligible = true)
        {
            if (!eligible) return 0;
            if (completedRound < 0 || completedRound > 30) throw new ArgumentOutOfRangeException(nameof(completedRound));
            if (completedRound == 0) return 30;
            var wave = catalog.Wave(completedRound);
            var ticks = SpawnTicks(Participants);
            var amount = 1.5 * ticks / Participants * wave.rewardRate * GoldFactor();
            if (completedRound % 5 == 0) amount += 50 * completedRound;
            if (options.equalGold) amount += (2.0 * wave.bossBounty + 3.0 * wave.regularBounty * ticks) / Participants;
            return (int)(amount + 30.5);
        }

        public int RoundExperience(int round)
        {
            if (round < 1 || round > 30) throw new ArgumentOutOfRangeException(nameof(round));
            var factor = round <= 2 ? .25 : round <= 20 ? .1 * round : 2 + .05 * (round - 20);
            return (int)(32500 * ExperienceFactor() * factor / 16) / (3 * (SpawnTicks(Participants) + 1));
        }

        void PrepareNextRound()
        {
            if (combatTimerActive)
            {
                var elapsed = (int)(Clock - combatStarted);
                var allowance = Round % 5 == 0 ? 40 + 10 * (Round / 5) : 15;
                speedBonus = elapsed == 0 ? 0 : Math.Max(0, 60 - Math.Max(0, elapsed - allowance));
                combatTimerActive = false;
            }
            ResetSchedule();
            if (IsDuelBoundary(Round) && Participants > 1 && !duelCompleted)
            {
                AwardSpeedBonus();
                SetPhase(OriginalMatchPhase.DuelPreparation);
                Emit(OriginalMatchEventKind.ShopAccess, "preparation", enabled: true);
                RestoreParty();
                Schedule(25, () =>
                {
                    SetPhase(OriginalMatchPhase.Duel);
                    Emit(OriginalMatchEventKind.DuelRequired, "round-income", amount: (int)DuelKind);
                });
                return;
            }
            if (Round > 0 && !IsDuelBoundary(Round)) AwardSpeedBonus();
            if (IsDuelBoundary(Round) && Participants == 1)
            {
                GiveAll(OriginalMatchEventKind.Gold, 200, "round-income");
                GiveAll(OriginalMatchEventKind.Souls, 8, "round-income");
            }
            GiveAll(OriginalMatchEventKind.Gold, RoundGold(Round, rewardsEnabled), "round-income");
            GiveAll(OriginalMatchEventKind.Souls, 4 + Round, "round-income");
            Round++;
            duelCompleted = false;
            DuelKind = OriginalDuelKind.Pairs;
            SetPhase(OriginalMatchPhase.Preparation);
            RemainingCount = 0;
            Array.Clear(ready, 0, ready.Length); readyCount = 0;
            preparationStarted = Clock;
            preparationDeadline = Clock + PreparationSeconds(Round);
            ExperienceBudget = 150;
            RestoreParty();
            Emit(OriginalMatchEventKind.ShopAccess, "preparation", enabled: true);
            SchedulePreparationCheck();
        }

        void SchedulePreparationCheck()
        {
            var deadline = preparationDeadline;
            Schedule(Math.Max(0, deadline - Clock), () =>
            {
                if (Phase == OriginalMatchPhase.Preparation && Clock >= preparationDeadline) StartCombat();
            });
        }

        void StartCombat()
        {
            ResetSchedule();
            combatStarted = Clock;
            combatTimerActive = true;
            pendingNormalSpawnTicks = 0;
            RemainingCount = InitialDeathBudget();
            Emit(OriginalMatchEventKind.ShopAccess, "preparation", enabled: false);
            if (Round % 5 == 0)
            {
                SetPhase(OriginalMatchPhase.BossCountdown);
                for (var slot = 1; slot <= Participants; slot++)
                    Emit(OriginalMatchEventKind.TeleportParty, "mega-transition", slot: slot,
                        x: (float)Range(-224, 224), y: (float)Range(-3360, -3136), facing: 90);
                Emit(OriginalMatchEventKind.RestoreParty, "mega-transition");
                Emit(OriginalMatchEventKind.PartyPause, "mega-transition", enabled: true);
                var boss = Spawn(catalog.Wave(Round).bossId, 0, -2192, 270, true, mega: true, sourceUserData: 2);
                bossId = boss.entityId;
                Emit(OriginalMatchEventKind.BossScaling, "mega-scaling", enemy: boss, amount: Participants);
                Emit(OriginalMatchEventKind.BossPause, "mega-transition", enemy: boss, enabled: true);
                if (Round == 30)
                {
                    Altars = 0; finalStage = 0; bossFraction = 1;
                    Emit(OriginalMatchEventKind.AltarsChanged, "mega-transition", amount: 0);
                }
                Schedule(5, () =>
                {
                    SetPhase(OriginalMatchPhase.Combat);
                    Emit(OriginalMatchEventKind.PartyPause, "mega-transition", enabled: false);
                    Emit(OriginalMatchEventKind.BossResume, "mega-transition", enemy: boss);
                    if (Round == 30) ScheduleFinalCheck();
                });
            }
            else
            {
                ExperienceBudget = RoundExperience(Round);
                SetPhase(OriginalMatchPhase.Combat);
                var ticks = Round == 21 ? (Participants <= 4 ? 2 : 3) :
                    Round == 23 ? (Participants <= 4 ? 4 : 6) : SpawnTicks(Participants);
                pendingNormalSpawnTicks = ticks;
                for (var i = 1; i <= ticks; i++)
                {
                    var tick = i;
                    Schedule(2.0 * i / ticks, () =>
                    {
                        SpawnNormalTick(tick);
                        pendingNormalSpawnTicks--;
                        TryFinishOrdinaryRound();
                    });
                }
                Schedule(Range(4.0 / ticks, 2), SpawnBossAndCasters);
                Schedule(3.5, RefreshOrders);
            }
        }

        void SpawnNormalTick(int tick)
        {
            if (Round == 23)
            {
                var positions = new float[,] { { -1024, 0 }, { 768, 1792 }, { -640, 1568 },
                    { 384, 256 }, { 1728, 1216 }, { -192, -800 }, { 64, 2624 }, { -1984, 576 } };
                // Elv increments global BD; no reset exists on an altar retry.
                var position = cocoonLocationIndex++;
                // COCOON1: BD9/10 read an unset rect. Native center getters
                // return0,0 and Elv still starts the ordinary30-second timer.
                bool hasRegion = (uint)position < (uint)positions.GetLength(0);
                float x = hasRegion ? positions[position, 0] : 0;
                float y = hasRegion ? positions[position, 1] : 0;
                var cocoon = Spawn("u00L", x, y, 270, false, cocoon: true);
                cocoon.cocoonTimer = 30;
                Schedule(1, () => TickCocoon(cocoon.entityId));
                return;
            }
            var rosterRound = NormalRosterRound(tick);
            var id = catalog.Wave(rosterRound).regularId;
            if (string.IsNullOrEmpty(id))
            {
                if (options.difficulty == OriginalDifficulty.Nightmare && Round == 29 && rosterRound == 30)
                {
                    // EMv still draws all three XY coordinates before its
                    // three native null CreateUnit attempts. Invent no roster.
                    Range(-186, 314); Range(2499, 2749);
                    Range(-2109, -1859); Range(324, 824);
                    Range(1609, 1859); Range(974, 1474);
                    return;
                }
                Emit(OriginalMatchEventKind.Unresolved, "nightmare-missing-next-round-roster", amount: rosterRound);
                return;
            }
            if (catalog.Enemy(id).HasAbility("A0K4") &&
                !(options.difficulty == OriginalDifficulty.Nightmare && Round == 22 && rosterRound == 23))
                Emit(OriginalMatchEventKind.Unresolved, "nightmare-substitution-excluded-from-death-counter", amount: rosterRound);
            Spawn(id, (float)Range(-186, 314), (float)Range(2499, 2749), 270, true);
            Spawn(id, (float)Range(-2109, -1859), (float)Range(324, 824), 0, true, attackGroup: 1);
            Spawn(id, (float)Range(1609, 1859), (float)Range(974, 1474), 180, true, attackGroup: 2);
        }

        void SpawnBossAndCasters()
        {
            var gate = (int)Range(0, 3);
            var coordinates = new float[,] { { 64, 2624, 270 }, { -1984, 576, 0 }, { 1734, 1224, 180 } };
            var wave = catalog.Wave(Round);
            for (var i = 0; i < 3; i++)
                if (i == gate || options.casters)
                    Spawn(i == gate ? wave.bossId : wave.casterId,
                        coordinates[i, 0] + (float)Range(-125, 125), coordinates[i, 1] + (float)Range(-125, 125), coordinates[i, 2], true,
                        sourceUserData: i == gate ? 1 : 0);
            Emit(OriginalMatchEventKind.OrderRefresh, "spawn-normal");
        }

        void RefreshOrders()
        {
            if (Phase != OriginalMatchPhase.Combat || Round % 5 == 0) return;
            Emit(OriginalMatchEventKind.OrderRefresh, "spawn-normal");
            Schedule(3.5, RefreshOrders);
        }

        void TickCocoon(int id)
        {
            if (!enemies.TryGetValue(id, out var cocoon)) return;
            cocoon.cocoonTimer -= 1 + cocoon.nearbyAccelerators;
            if (cocoon.cocoonTimer <= 0)
            {
                enemies.Remove(id);
                Emit(OriginalMatchEventKind.Remove, "wave-23-cocoons", enemy: cocoon);
                SpawnCocoonChildren(cocoon, true);
            }
            else Schedule(1, () => TickCocoon(id));
        }

        void SpawnCocoonChildren(OriginalMatchEnemy cocoon, bool strong)
        {
            for (var i = 0; i < 10; i++) Spawn(strong ? "n065" : "n066", cocoon.x, cocoon.y, 270, true);
        }

        void CompleteMega()
        {
            RemainingCount = 0;
            GiveAll(OriginalMatchEventKind.Souls, 5, "mega-transition");
            var allAlive = true;
            foreach (var heroAlive in alive) if (!heroAlive) allAlive = false;
            if (options.altars && (options.difficulty == OriginalDifficulty.Easy ||
                ((options.difficulty == OriginalDifficulty.Standard || options.difficulty == OriginalDifficulty.Custom) && allAlive)))
            {
                Altars++;
                Emit(OriginalMatchEventKind.AltarsChanged, "mega-transition", amount: Altars);
            }
            FinishRound(1);
        }

        void FinishRound(double delay)
        {
            ResetSchedule();
            SetPhase(OriginalMatchPhase.RoundTransition);
            var ordinary = Round % 5 != 0;
            Schedule(delay, () =>
            {
                PrepareNextRound();
                // E9v restores zB only after D4. Xov (mega completion) does not.
                if (ordinary) rewardsEnabled = true;
            });
        }

        void ScheduleFinalCheck()
        {
            Schedule(1, () =>
            {
                if (Phase != OriginalMatchPhase.Combat || Round != 30) return;
                if (finalStage < 3 && bossFraction <= new[] { .75, .55, .35 }[finalStage]) BeginFinalIntermission();
                else ScheduleFinalCheck();
            });
        }

        void BeginFinalIntermission()
        {
            SetPhase(OriginalMatchPhase.FinalIntermission);
            finalSeriesComplete = false;
            Emit(OriginalMatchEventKind.BossPause, "final-phases", entityId: bossId, enabled: true);
            var indices = finalStage == 0 ? new[] { 1, 2, 3, 4, 6, 7, 8, 9, 11, 12, 13, 14, 16, 17, 18, 19 } :
                finalStage == 1 ? new[] { 5, 15 } : new[] { 10, 20 };
            var interval = 3.2 - .2 * Participants + (finalStage == 0 ? 0 : 5);
            var positions = new List<int>();
            for (var i = 1; i <= indices.Length; i++) positions.Add(i);
            for (var i = 0; i < indices.Length; i++)
            {
                var wave = indices[i];
                var selected = finalStage == 0 ? (int)Range(0, positions.Count) : i;
                var position = finalStage == 0 ? positions[selected] : i + 1;
                if (finalStage == 0) { positions[selected] = positions[positions.Count - 1]; positions.RemoveAt(positions.Count - 1); }
                var degrees = position * 360.0 / indices.Length - (finalStage == 0 ? 0 : 30);
                var radius = finalStage == 0 ? 650 : 500;
                var x = (float)(radius * Math.Cos(degrees * Math.PI / 180));
                var y = (float)(-2700 + radius * Math.Sin(degrees * Math.PI / 180));
                Schedule(1 + interval * (i + 1), () =>
                {
                    var id = catalog.Wave(wave).bossId;
                    // f3 first sets1, then changes these four mega variants to2.
                    var tag = id == "n00K" || id == "n00Z" || id == "n017" || id == "u00G" ? 2 : 1;
                    var add = Spawn(id, x, y, 0, false, finalAdd: true, sourceUserData: tag);
                    Emit(OriginalMatchEventKind.FinalAddScaling, "final-phases", enemy: add, amount: Participants);
                });
            }
            Schedule(1 + interval * (indices.Length + 1), () =>
            {
                finalSeriesComplete = true;
                // ENv resumes only on a later death event. The enabled VA
                // warning timer supplies that event even if every add died early.
            });
            Schedule(.5, RegenerateFinalBoss);
        }

        void RegenerateFinalBoss()
        {
            if (Phase != OriginalMatchPhase.FinalIntermission) return;
            // c3 restores 15 HP and 10 mana every half second; health fraction must
            // be updated by the combat owner with its resolved maximum HP.
            Emit(OriginalMatchEventKind.BossRegeneration, "final-phases", entityId: bossId, amount: 15, secondaryAmount: 10);
            Schedule(.5, RegenerateFinalBoss);
        }

        void TryResumeFinalBoss()
        {
            if (Phase != OriginalMatchPhase.FinalIntermission || !finalSeriesComplete || CountFinalAdds() != 0) return;
            ResetSchedule();
            finalStage++;
            SetPhase(OriginalMatchPhase.Combat);
            Emit(OriginalMatchEventKind.BossResume, "final-phases", entityId: bossId,
                x: (float)Range(-576, 576), y: (float)Range(-3328, -2112));
            ScheduleFinalCheck();
        }

        int CountFinalAdds()
        {
            var count = 0;
            foreach (var enemy in enemies.Values) if (enemy.finalAdd) count++;
            return count;
        }

        // D3 (3.9c JASS17766): the fifth mega round's phase helper joins Fa,
        // but receives neither f3 scaling nor the ordinary wave counter.
        // Its native A11G removal belongs to the session's ability overlay.
        internal OriginalMatchEnemy SpawnBossPhaseAdd()
        {
            if (Round != 25 || Phase != OriginalMatchPhase.Combat)
                throw new InvalidOperationException("Boss phase add requires round25 combat.");
            float x = (float)Range(-576, 576), y = (float)Range(-3328, -2112);
            float facing = (float)(Math.Atan2(-2700 - y, -x) * 180 / Math.PI);
            return Spawn("n01X", x, y, facing, false, finalAdd: true, sourceUserData: 1);
        }

        OriginalMatchEnemy Spawn(string id, float x, float y, float facing, bool counted, bool cocoon = false, bool mega = false, bool finalAdd = false, int attackGroup = 0, int sourceUserData = 0)
        {
            var declaration = catalog.Enemy(id);
            if (!declaration.definitionKnown) throw new InvalidOperationException("Unresolved enemy definition: " + id);
            var enemy = new OriginalMatchEnemy { entityId = ++entitySequence, rawcode = id, x = x, y = y,
                counted = counted && !declaration.HasAbility("A0K4"), cocoon = cocoon, megaBoss = mega, finalAdd = finalAdd,
                attackGroup = attackGroup, sourceUserData = sourceUserData };
            enemies.Add(enemy.entityId, enemy);
            Emit(OriginalMatchEventKind.Spawn, "spawn-normal", enemy: enemy, facing: facing);
            return enemy;
        }

        void AwardExperience(OriginalMatchEnemy enemy)
        {
            var unit = catalog.Enemy(enemy.rawcode);
            if (unit.level <= 0 || unit.level >= 49 || unit.HasAbility("A0K4") || unit.HasAbility("A0A9") ||
                unit.id == "U00I" || unit.id == "O006") return;
            for (var i = 0; i < Participants; i++)
                if (alive[i] && heroIds[i] != "H02E") Give(OriginalMatchEventKind.Experience, i + 1, ExperienceBudget, "xp-survival");
        }

        void AwardSpeedBonus()
        {
            if (!rewardsEnabled || Round == 0) return;
            GiveAll(OriginalMatchEventKind.Gold, speedBonus, "M3-m3-speed-bonus");
        }

        void RestoreParty()
        {
            for (var i = 0; i < Participants; i++)
            {
                if (Round > 1 && (options.returnToCenter || !alive[i] || Round == 6 || Round == 11 || Round == 16 || Round == 21 || Round == 26))
                {
                    var angle = (i + 1) * 2 * Math.PI / Participants;
                    Emit(OriginalMatchEventKind.TeleportParty, "preparation", slot: i + 1,
                        x: (float)(-50 + 185 * Math.Cos(angle)), y: (float)(1000 + 185 * Math.Sin(angle)), facing: 270);
                }
                alive[i] = true;
            }
            Emit(OriginalMatchEventKind.RestoreParty, "preparation");
        }

        void RemoveAllEnemies()
        {
            foreach (var enemy in enemies.Values) Emit(OriginalMatchEventKind.Remove, "death-and-wipe", enemy: enemy);
            enemies.Clear(); RemainingCount = 0;
        }

        void GiveAll(OriginalMatchEventKind kind, int amount, string source)
        {
            for (var slot = 1; slot <= Participants; slot++) Give(kind, slot, amount, source);
        }

        void Give(OriginalMatchEventKind kind, int slot, int amount, string source)
        {
            if (amount == 0) return;
            if (kind == OriginalMatchEventKind.Gold) gold[slot - 1] += amount;
            else if (kind == OriginalMatchEventKind.Souls) souls[slot - 1] += amount;
            else if (kind == OriginalMatchEventKind.Experience) xp[slot - 1] += amount;
            Emit(kind, source, slot: slot, amount: amount);
        }

        void SetPhase(OriginalMatchPhase value)
        {
            Phase = value;
            Emit(OriginalMatchEventKind.PhaseChanged, "match-sequence", amount: (int)value);
        }

        bool IsBattlePhase() => Phase == OriginalMatchPhase.Combat || Phase == OriginalMatchPhase.FinalIntermission;
        bool enemyIdPaused(OriginalMatchEnemy enemy) => enemy.entityId == bossId && Phase == OriginalMatchPhase.FinalIntermission;
        static bool IsDuelBoundary(int round) => round > 0 && round % 5 == 4;
        double GoldFactor() => options.difficulty == OriginalDifficulty.Easy ? 1.5 :
            options.difficulty == OriginalDifficulty.Extreme ? .8 : options.difficulty == OriginalDifficulty.Nightmare ? .6 : 1;
        double ExperienceFactor() => options.difficulty == OriginalDifficulty.Easy ? 1.2 :
            options.difficulty == OriginalDifficulty.Extreme ? .8 : options.difficulty == OriginalDifficulty.Nightmare ? .6 : 1;

        void ValidateSlot(int slot)
        {
            if (slot < 1 || slot > Participants) throw new ArgumentOutOfRangeException(nameof(slot));
        }

        void ResetSchedule() { generation++; schedule.Clear(); }

        void Schedule(double delay, Action action)
        {
            var epoch = generation;
            schedule.Add(new Scheduled { time = Clock + delay, order = ++scheduleSequence,
                action = () => { if (epoch == generation) action(); } });
            schedule.Sort((a, b) => a.time == b.time ? a.order.CompareTo(b.order) : a.time.CompareTo(b.time));
        }

        double Range(double minimum, double maximum)
        {
            random ^= random << 13; random ^= random >> 17; random ^= random << 5;
            return minimum + (maximum - minimum) * (random / 4294967296.0);
        }

        void Emit(OriginalMatchEventKind kind, string source, OriginalMatchEnemy enemy = null, int entityId = 0,
            int slot = 0, int amount = 0, float x = 0, float y = 0, float facing = 0, bool enabled = false, int secondaryAmount = 0)
        {
            events.Add(new OriginalMatchEvent { sequence = ++eventSequence, time = Clock, kind = kind,
                round = Round, entityId = enemy?.entityId ?? entityId, rawcode = enemy?.rawcode,
                slot = slot, amount = amount, secondaryAmount = secondaryAmount, x = enemy?.x ?? x, y = enemy?.y ?? y,
                facing = facing, enabled = enabled, sourceRule = source, attackGroup = enemy?.attackGroup ?? 0,
                sourceUserData = enemy?.sourceUserData ?? 0 });
        }
    }
}

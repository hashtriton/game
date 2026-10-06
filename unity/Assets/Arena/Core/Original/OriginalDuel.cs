using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalDuelCatalog
    {
        public int schemaVersion;
        public string sourceSha256, jassSha256, scope;
        public int countdownTicks, pairGold, pairSouls, drawGold, drawSouls, gladiatorBaseSouls, betCap, ringSecondStageTick;
        public double pairCountdownSeconds, gladiatorCountdownSeconds, combatSeconds, transitionSeconds;
        public double betSettlementSeconds, returnSeconds, ringDelaySeconds, ringPeriod;
        public double ringInitialRadius, ringShrinkPerTick, ringMinimumRadius, circleRadius, degreesToRadians;
        public double ringCenterX, ringCenterY, pairLeftX, pairRightX, pairY, cameraX, cameraY;
        public double returnMinX, returnMinY, returnMaxX, returnMaxY;
        public OriginalDuelRule[] rules;
        public OriginalRuleSource[] sources;

        public void Validate()
        {
            if (schemaVersion != 1 || sourceSha256 != "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34")
                throw new ArgumentException("Duels require the verified 3.9c source map.");
            if (countdownTicks != 10 || pairCountdownSeconds != 11 || gladiatorCountdownSeconds != 10 || combatSeconds != 120 ||
                transitionSeconds != 2 || betSettlementSeconds != .5 || returnSeconds != .5 || pairGold != 250 || pairSouls != 10 ||
                drawGold != 125 || drawSouls != 5 || gladiatorBaseSouls != 10 || betCap != 1000 || ringDelaySeconds != 61 ||
                ringPeriod != .04 || ringInitialRadius != 810 || ringShrinkPerTick != .52 || ringMinimumRadius != 325 ||
                ringSecondStageTick != 801 || circleRadius != 525 || degreesToRadians != .0174532 || ringCenterX != 0 ||
                ringCenterY != -2700 || pairLeftX != -416 || pairRightX != 416 || pairY != -2688 ||
                cameraX != 0 || cameraY != -2688 || returnMinX != -384 || returnMinY != 736 || returnMaxX != 256 || returnMaxY != 1248)
                throw new ArgumentException("Duel constants differ from the extracted 3.9c rules.");
        }

        internal OriginalDuelCatalog Copy() { return (OriginalDuelCatalog)MemberwiseClone(); }
    }

    [Serializable]
    public sealed class OriginalDuelRule
    {
        public string id, evidence, summary;
        public string[] unresolved;
    }

    [Serializable]
    public sealed class OriginalDuelParticipant
    {
        public int slot, rating;
        public string heroRawcode;
        public bool mirrorCurse, bloodPoisonCurse;
        internal OriginalDuelParticipant Copy() { return (OriginalDuelParticipant)MemberwiseClone(); }
    }

    public enum OriginalDuelPhase { NotStarted, Countdown, Combat, Resolving, Completed, Unresolved }
    public enum OriginalDuelEventKind
    {
        WorldRule, HeroState, Placement, Alliance, Countdown, CombatStarted, BetOpened, BetChanged, BetLocked,
        Gold, Souls, Win, Loss, Curse, Result, BetSettled, RingStarted, RingStage, RingStopped, Completed, Unresolved
    }

    // slot==0 means all participants for WorldRule/Alliance only. HeroState uses explicit slots.
    // HeroState integer flags are -1=unchanged, 0=false, 1=true. Placement is Warcraft XY, not Unity XYZ.
    // Placement.revive means revive at (x,y), then move there; setFacing=false preserves direction.
    // resetCooldowns requires the Jz I0AE exception from the catalog, not an unconditional item reset.
    [Serializable]
    public sealed class OriginalDuelEvent
    {
        public long sequence;
        public double time;
        public OriginalDuelEventKind kind;
        public int pair, slot, otherSlot, side, amount;
        public string code;
        public double x, y, facing, value;
        public int paused = -1, visible = -1, invulnerable = -1, suspendXp = -1;
        public bool revive, fullLife, fullMana, removeBuffs, resetCooldowns, stop, setFacing;
    }

    [Serializable]
    public sealed class OriginalDuelSnapshot
    {
        public OriginalDuelKind kind;
        public OriginalDuelPhase phase;
        public int completedRound, pair, firstSlot, secondSlot, remaining, carryPrizeGold, ringStage;
        public double time, phaseEndsAt, ringRadius;
        public bool betsOpen;
        public string unresolved;
        public OriginalDuelParticipant[] participants;
        public bool[] alive;
        public int[] betSides, betStakes;
    }

    /// <summary>
    /// Host-only Survival referee. Begin only after Match.DuelRequired. Apply drained Gold/Souls once
    /// through the session inventory ledger; they are deltas, not additional Match rewards. Sequence
    /// numbers are scoped to this instance: ledger identity must also include the duel instance. Completed
    /// permits one Match.CompleteDuelSequence call. CarryPrizeGold must survive both duel kinds/rounds.
    /// World supplies eligible primary-hero deaths and authoritative alive masks; this is not combat AI.
    /// Catalog rules identify world effects and native gaps. A portable seed is not a Warcraft replay seed.
    /// </summary>
    public sealed class OriginalDuel
    {
        sealed class Scheduled
        {
            internal double at;
            internal long order;
            internal Action action;
        }

        readonly OriginalDuelCatalog catalog;
        readonly OriginalDuelParticipant[] participants;
        readonly int completedRound;
        readonly bool[] alive, visible, used, deathSeen;
        readonly int[] betSides, betStakes;
        readonly List<OriginalDuelEvent> events = new List<OriginalDuelEvent>();
        readonly List<Scheduled> scheduled = new List<Scheduled>();
        int first, second, pair, remaining, generation, ringStage;
        long sequence, scheduleOrder;
        uint random;
        double phaseEndsAt, ringStartedAt;
        bool betsOpen, ringActive;
        string unresolved;

        public OriginalDuelPhase Phase { get; private set; }
        public OriginalDuelKind Kind { get; private set; }
        public int CarryPrizeGold { get; private set; }
        public double Time { get; private set; }
        public OriginalDuel(OriginalDuelCatalog catalog, OriginalDuelKind kind, int completedRound,
            OriginalDuelParticipant[] participants, int seed, int carriedPrizeGold = 0)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            catalog.Validate();
            if (kind != OriginalDuelKind.Pairs && kind != OriginalDuelKind.Gladiator)
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (completedRound < 4 || completedRound > 29 || completedRound % 5 != 4)
                throw new ArgumentOutOfRangeException(nameof(completedRound));
            if (participants == null || participants.Length < 2 || participants.Length > 8)
                throw new ArgumentException("Survival duels require 2..8 present human participants.", nameof(participants));
            if (carriedPrizeGold < 0 || carriedPrizeGold > int.MaxValue - 8250)
                throw new ArgumentOutOfRangeException(nameof(carriedPrizeGold));
            this.catalog = catalog.Copy();
            this.participants = new OriginalDuelParticipant[participants.Length];
            foreach (var p in participants)
            {
                if (p == null || p.slot < 1 || p.slot > participants.Length || this.participants[p.slot - 1] != null)
                    throw new ArgumentException("Participant match slots must be unique and contiguous 1..N.", nameof(participants));
                if (p.heroRawcode != "H008" && p.heroRawcode != "N0A0" && p.heroRawcode != "H024")
                    throw new ArgumentException("Only the current three source heroes have a connected world contract.", nameof(participants));
                this.participants[p.slot - 1] = p.Copy();
            }
            this.completedRound = completedRound;
            Kind = kind;
            CarryPrizeGold = carriedPrizeGold;
            alive = new bool[participants.Length + 1]; visible = new bool[alive.Length];
            used = new bool[alive.Length]; deathSeen = new bool[alive.Length];
            betSides = new int[alive.Length]; betStakes = new int[alive.Length];
            for (var slot = 1; slot < alive.Length; slot++) alive[slot] = visible[slot] = true;
            random = unchecked((uint)seed);
            if (random == 0) random = 0x6D2B79F5;
        }

        public bool Begin()
        {
            if (Phase != OriginalDuelPhase.NotStarted) return false;
            World("pair-prepare:pause-nonheroes-except-n03W");
            var camera = World("pair-prepare:camera"); camera.x = catalog.cameraX; camera.y = catalog.cameraY;
            if (Kind == OriginalDuelKind.Pairs)
            {
                for (var slot = 1; slot < alive.Length; slot++) Hero(slot, "pair-prepare:stop-all").stop = true;
                World("pair-prepare:enable-MA-boundary");
                PreparePair(SelectNext(), SelectNext());
            }
            else PrepareGladiator();
            return true;
        }

        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0 || double.IsInfinity(Time + seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Phase == OriginalDuelPhase.Unresolved || Phase == OriginalDuelPhase.Completed || Phase == OriginalDuelPhase.NotStarted) return;
            var target = Time + seconds;
            while (scheduled.Count > 0)
            {
                var next = 0;
                for (var i = 1; i < scheduled.Count; i++)
                    if (scheduled[i].at < scheduled[next].at ||
                        (scheduled[i].at == scheduled[next].at && scheduled[i].order < scheduled[next].order)) next = i;
                var task = scheduled[next];
                if (task.at > target + 1e-9) break;
                scheduled.RemoveAt(next);
                Time = Math.Max(Time, task.at);
                task.action();
                if (Phase == OriginalDuelPhase.Unresolved || Phase == OriginalDuelPhase.Completed) return;
            }
            Time = Math.Max(Time, target);
        }

        /// <summary>
        /// World calls this only for XHv/Xuv-eligible deaths, after updating life/dead flags. The optional
        /// mask has bits 1..N, bit0 clear; it captures simultaneous deaths before reward resolution.
        /// Match.HeroDied must not also process this event during a duel. Killer is an owning match slot,
        /// not a network peer id; summoned-unit kills use their owner's slot.
        /// </summary>
        public bool HeroDied(int slot, int killerOwnerSlot = 0, int aliveMaskAfterEvent = -1)
        {
            ValidateSlot(slot);
            var validMask = (1 << alive.Length) - 2;
            if (aliveMaskAfterEvent != -1 && (aliveMaskAfterEvent < 0 || (aliveMaskAfterEvent & ~validMask) != 0 ||
                (aliveMaskAfterEvent & (1 << slot)) != 0))
                throw new ArgumentOutOfRangeException(nameof(aliveMaskAfterEvent));
            if (Phase != OriginalDuelPhase.Combat || deathSeen[slot] || (Kind == OriginalDuelKind.Pairs && slot != first && slot != second)) return false;
            if (aliveMaskAfterEvent != -1)
                for (var i = 1; i < alive.Length; i++) alive[i] = (aliveMaskAfterEvent & (1 << i)) != 0;
            alive[slot] = false;
            deathSeen[slot] = true;
            if (Kind == OriginalDuelKind.Pairs) ResolvePair(slot, false, false);
            else
            {
                remaining--;
                if (remaining == 1)
                {
                    if (killerOwnerSlot < 1 || killerOwnerSlot >= alive.Length)
                        Halt("gladiator-killer-owner-unknown");
                    else
                    {
                        var winner = killerOwnerSlot;
                        if (!alive[winner])
                            for (var i = 1; i < alive.Length; i++)
                                if (alive[i] && i != slot) winner = i;
                        // If there is no fallback, JASS still awards the valid dead killer's owner.
                        ResolveGladiator(winner, false);
                    }
                }
            }
            return true;
        }

        // Host observation for native revive failures and life-state changes, never a client command.
        public void ObserveHeroAlive(int slot, bool isAlive) { ValidateSlot(slot); alive[slot] = isAlive; }
        public void SetRating(int slot, int rating) { ValidateSlot(slot); participants[slot - 1].rating = rating; }

        /// <summary>Aggregate, fully funded version of original keyboard bet adjustments; Gold events are immediate deltas.</summary>
        public bool TrySetBet(int slot, int side, int stake, int availableGold)
        {
            ValidateSlot(slot);
            if (!betsOpen || Phase != OriginalDuelPhase.Countdown || slot == first || slot == second ||
                side < 0 || side > 2 || stake < 0 || stake > catalog.betCap || availableGold < 0 || stake - betStakes[slot] > availableGold) return false;
            var delta = betStakes[slot] - stake;
            if (delta != 0) Emit(OriginalDuelEventKind.Gold, slot, delta, "bets:adjust");
            if (betSides[slot] != side || betStakes[slot] != stake)
            {
                betSides[slot] = side; betStakes[slot] = stake;
                var changed = Emit(OriginalDuelEventKind.BetChanged, slot, stake, "bets:adjust"); changed.side = side;
            }
            return true;
        }

        public bool DiscardBet(int slot)
        {
            ValidateSlot(slot);
            if (!betsOpen || Phase != OriginalDuelPhase.Countdown || slot == first || slot == second) return false;
            betSides[slot] = betStakes[slot] = 0;
            Emit(OriginalDuelEventKind.BetChanged, slot, 0, "bets:discard-without-refund");
            return true;
        }

        public void ParticipantDisconnected(int slot)
        {
            ValidateSlot(slot);
            // J3 transfers the same hero to computer control. The session owns
            // its orders; pairing, funded bets and death accounting continue.
        }

        public OriginalDuelSnapshot Snapshot()
        {
            var roster = new OriginalDuelParticipant[participants.Length];
            for (var i = 0; i < roster.Length; i++) roster[i] = participants[i].Copy();
            var ringTicks = ringActive ? Math.Max(0, (int)Math.Floor((Time - ringStartedAt + 1e-9) / catalog.ringPeriod)) : 0;
            return new OriginalDuelSnapshot
            {
                kind = Kind, phase = Phase, completedRound = completedRound, pair = pair, firstSlot = first, secondSlot = second,
                remaining = remaining, carryPrizeGold = CarryPrizeGold, time = Time, phaseEndsAt = phaseEndsAt, betsOpen = betsOpen,
                ringStage = ringStage, ringRadius = ringActive ? Math.Max(catalog.ringMinimumRadius,
                    catalog.ringInitialRadius - Math.Max(0, ringTicks - 1) * catalog.ringShrinkPerTick) : 0,
                unresolved = unresolved, participants = roster, alive = (bool[])alive.Clone(),
                betSides = (int[])betSides.Clone(), betStakes = (int[])betStakes.Clone()
            };
        }

        public OriginalDuelEvent[] DrainEvents() { var result = events.ToArray(); events.Clear(); return result; }
        public static int SourceRating(int kills, int level, int additionalA, int additionalB)
        { return checked(kills * 2 + level * 15 + additionalA + additionalB - 15); }

        void PreparePair(int left, int right)
        {
            if (left == 0 || right == 0) { Halt("pair-participant-missing"); return; }
            StopRing();
            first = left; second = right; pair++; remaining = 2; generation++;
            Phase = OriginalDuelPhase.Countdown; phaseEndsAt = Time + catalog.pairCountdownSeconds;
            Array.Clear(betSides, 0, betSides.Length); Array.Clear(betStakes, 0, betStakes.Length);
            World("pair-prepare:hide-pause-shops");
            Hero(first, "pair-prepare:remove-buffs").removeBuffs = true;
            Hero(second, "pair-prepare:remove-buffs").removeBuffs = true;
            World("pair-prepare:delete-unowned-arena-items");
            PrepareHero(first); PrepareHero(second);
            Place(first, catalog.pairLeftX, catalog.pairY, 0, "pair-prepare").revive = true;
            Place(second, catalog.pairRightX, catalog.pairY, 180, "pair-prepare").revive = true;
            Hero(first, "pair-prepare:cooldowns").resetCooldowns = true;
            Hero(second, "pair-prepare:cooldowns").resetCooldowns = true;
            World("pair-prepare:Z2-cleanup"); Schedule(.1, () => World("pair-prepare:Z2-cleanup-repeat"));
            RefillHero(first); RefillHero(second);
            HideSpectators(); Schedule(1.3, HideSpectators);
            Alliance(first, second, false);
            betsOpen = true; Emit(OriginalDuelEventKind.BetOpened, 0, catalog.pairGold + CarryPrizeGold, "bets");
            var token = generation;
            for (var tick = 1; tick <= catalog.countdownTicks; tick++)
            {
                var digit = catalog.countdownTicks - tick + 1;
                Schedule(tick, () => { if (generation == token && Phase == OriginalDuelPhase.Countdown) Emit(OriginalDuelEventKind.Countdown, 0, digit, "pair-countdown"); });
            }
            Schedule(catalog.pairCountdownSeconds, () => { if (generation == token) StartCombat(); });
        }

        void PrepareGladiator()
        {
            generation++; pair = 1; remaining = participants.Length;
            Phase = OriginalDuelPhase.Countdown; phaseEndsAt = Time + catalog.gladiatorCountdownSeconds;
            World("gladiator:disable-AI"); World("pair-prepare:hide-pause-shops");
            World("pair-prepare:delete-unowned-arena-items");
            World("pair-prepare:Z2-cleanup"); Schedule(.1, () => World("pair-prepare:Z2-cleanup-repeat"));
            var order = new int[participants.Length];
            for (var i = 0; i < order.Length; i++) order[i] = i + 1;
            for (var i = order.Length - 1; i > 0; i--)
            {
                var j = RandomInt(0, i); var old = order[i]; order[i] = order[j]; order[j] = old;
            }
            for (var i = 0; i < order.Length; i++)
            {
                var angle = (i + 1) * (360.0 / order.Length);
                PrepareHero(order[i]);
                Place(order[i], catalog.ringCenterX + catalog.circleRadius * Math.Cos(angle * catalog.degreesToRadians),
                    catalog.ringCenterY + catalog.circleRadius * Math.Sin(angle * catalog.degreesToRadians), angle + 180, "gladiator").revive = true;
                Hero(order[i], "gladiator:cooldowns").resetCooldowns = true;
                RefillHero(order[i]);
            }
            Alliance(0, 0, false);
            for (var tick = 1; tick < catalog.countdownTicks; tick++)
            {
                var digit = catalog.countdownTicks - tick;
                Schedule(tick, () => Emit(OriginalDuelEventKind.Countdown, 0, digit, "gladiator-countdown"));
            }
            Schedule(catalog.gladiatorCountdownSeconds, StartCombat);
            Schedule(catalog.gladiatorCountdownSeconds + 1, () => World("gladiator:restore-AI-if-Fn"));
        }

        void PrepareHero(int slot)
        {
            alive[slot] = visible[slot] = true; deathSeen[slot] = false;
            var state = Hero(slot, "pair-prepare");
            state.visible = 1; state.invulnerable = 1;
            state.removeBuffs = Kind == OriginalDuelKind.Gladiator;
            state.stop = Kind == OriginalDuelKind.Gladiator;
        }

        void RefillHero(int slot)
        {
            var state = Hero(slot, "pair-prepare:refill");
            state.fullLife = state.fullMana = true; state.suspendXp = 1;
        }

        void HideSpectators()
        {
            for (var slot = 1; slot < alive.Length; slot++)
                if (slot != first && slot != second && alive[slot] && visible[slot])
                {
                    Place(slot, -64, 580, double.NaN, "pair-prepare:spectator");
                    var state = Hero(slot, "pair-prepare:spectator"); state.visible = 0; state.suspendXp = 1;
                    visible[slot] = false;
                }
        }

        void StartCombat()
        {
            if (Phase != OriginalDuelPhase.Countdown) return;
            Phase = OriginalDuelPhase.Combat; phaseEndsAt = Time + catalog.combatSeconds;
            if (Kind == OriginalDuelKind.Pairs) World("pair-prepare:combat-e3-cleanup");
            for (var slot = 1; slot < alive.Length; slot++)
                if (Kind == OriginalDuelKind.Gladiator || slot == first || slot == second)
                {
                    var state = Hero(slot, "combat-start"); state.paused = 0; state.invulnerable = 0; state.removeBuffs = true;
                    World("combat-start:unpause-side-panel", slot);
                }
            Emit(OriginalDuelEventKind.CombatStarted, 0, 0, "combat-timeout");
            if (Kind == OriginalDuelKind.Pairs && (!alive[first] || !alive[second])) ResolvePair(0, false, true);
            else
            {
                var token = generation;
                Schedule(catalog.combatSeconds, () =>
                {
                    if (generation != token || Phase != OriginalDuelPhase.Combat) return;
                    if (Kind == OriginalDuelKind.Pairs) ResolvePair(0, true, false);
                    else { remaining--; ResolveGladiator(0, true); }
                });
                Schedule(catalog.ringDelaySeconds, () =>
                {
                    if (generation == token && Phase == OriginalDuelPhase.Combat) StartRing();
                });
            }
            if (Kind == OriginalDuelKind.Pairs) { betsOpen = false; Emit(OriginalDuelEventKind.BetLocked, 0, 0, "bets"); }
        }

        void ResolvePair(int dead, bool draw, bool startupFailure)
        {
            Phase = OriginalDuelPhase.Resolving; phaseEndsAt = Time + catalog.transitionSeconds;
            remaining = (alive[first] ? 1 : 0) + (alive[second] ? 1 : 0);
            var side = dead == 0 ? 0 : (dead == first ? 2 : 1);
            var physicalWinner = draw ? first : (dead == 0 ? (!alive[first] ? second : first) : (dead == first ? second : first));
            var loser = draw ? second : (dead == 0 ? (!alive[first] ? first : second) : dead);
            var bothDead = dead != 0 && !alive[physicalWinner];
            if (bothDead) { physicalWinner = second; loser = first; }
            World("pair-result:suppress-global-death-trigger"); Schedule(.2, () => World("pair-result:restore-global-death-trigger"));
            Place(loser, -60, 600, double.NaN, "pair-result");
            Place(physicalWinner, -60, 600, double.NaN, "pair-result");
            var winnerState = Hero(physicalWinner, "pair-result"); winnerState.fullLife = true;
            winnerState.removeBuffs = winnerState.stop = true;
            var loserState = Hero(loser, "pair-result"); loserState.removeBuffs = loserState.stop = true;
            World("pair-result:remove-A10H", physicalWinner); World("pair-result:remove-A10H", loser);
            Alliance(first, second, true);
            var recipient = participants[physicalWinner - 1].mirrorCurse ? loser : physicalWinner;
            if (participants[loser - 1].bloodPoisonCurse && RandomInt(0, 1) == 0)
            {
                participants[recipient - 1].bloodPoisonCurse = true;
                Emit(OriginalDuelEventKind.Curse, recipient, 1, "A19Q");
            }
            if (!bothDead)
            {
                if (draw)
                {
                    Reward(recipient, catalog.drawGold, catalog.drawSouls, "pair-draw");
                    Reward(loser, catalog.drawGold, catalog.drawSouls, "pair-draw");
                }
                else
                {
                    Reward(recipient, checked(catalog.pairGold + CarryPrizeGold), catalog.pairSouls, "pair-victory");
                    CarryPrizeGold = 0;
                    Emit(OriginalDuelEventKind.Win, recipient, 1, "pair-victory");
                    Emit(OriginalDuelEventKind.Loss, loser, 1, "pair-victory");
                }
            }
            var result = Emit(OriginalDuelEventKind.Result, bothDead ? 0 : recipient, 0,
                bothDead ? "both-dead" : draw ? "draw" : startupFailure ? "startup-forfeit" : "winner");
            result.otherSlot = loser; result.side = side;
            World("finish:cleanup-secondary-hero-forms");
            Schedule(catalog.betSettlementSeconds, () => SettleBets(side));
            if (pair < participants.Length / 2)
            {
                var nextFirst = SelectNext(); var nextSecond = SelectNext();
                var state = Hero(recipient, "pair-result:hide-recipient"); state.visible = 0; visible[recipient] = false;
                Schedule(catalog.transitionSeconds, () => PreparePair(nextFirst, nextSecond));
            }
            else FinishSeries();
        }

        void ResolveGladiator(int winner, bool timeout)
        {
            Phase = OriginalDuelPhase.Resolving; phaseEndsAt = Time + catalog.transitionSeconds;
            if (!timeout)
            {
                Reward(winner, 0, catalog.gladiatorBaseSouls + completedRound, "gladiator-victory");
                Emit(OriginalDuelEventKind.Win, winner, 1, "gladiator-victory");
            }
            Emit(OriginalDuelEventKind.Result, winner, 0, timeout ? "no-winner" : "winner");
            World("pair-result:suppress-global-death-trigger"); Schedule(.2, () => World("pair-result:restore-global-death-trigger"));
            for (var slot = 1; slot < alive.Length; slot++)
            {
                var state = Hero(slot, "gladiator-result"); state.fullLife = state.removeBuffs = state.stop = true;
                World("pair-result:remove-A10H", slot);
            }
            StopRing(); World("finish:cleanup-secondary-hero-forms"); FinishSeries();
        }

        void FinishSeries()
        {
            for (var slot = 1; slot < alive.Length; slot++)
            {
                var state = Hero(slot, "finish"); state.visible = 1; state.suspendXp = 0; visible[slot] = true;
            }
            Alliance(0, 0, true); World("finish:unpause-world"); World("finish:disable-MA-boundary");
            var fog = World("finish:restore-fog-after-delay"); fog.value = 6.25;
            Schedule(catalog.returnSeconds, () =>
            {
                for (var slot = 1; slot < alive.Length; slot++)
                {
                    Place(slot, RandomReal(catalog.returnMinX, catalog.returnMaxX), RandomReal(catalog.returnMinY, catalog.returnMaxY), double.NaN, "finish:return");
                    Hero(slot, "finish:return").resetCooldowns = true;
                }
            });
            Schedule(catalog.transitionSeconds, () =>
            {
                StopRing(); Phase = OriginalDuelPhase.Completed; phaseEndsAt = Time;
                Emit(OriginalDuelEventKind.Completed, 0, CarryPrizeGold, "outer-wiring");
                scheduled.Clear();
            });
        }

        void SettleBets(int winningSide)
        {
            if (winningSide == 0)
            {
                for (var slot = 1; slot < betStakes.Length; slot++)
                    if (betStakes[slot] > 0) Emit(OriginalDuelEventKind.Gold, slot, betStakes[slot], "bets:refund");
            }
            else
            {
                var winnerPool = 0; var otherPool = 0;
                for (var slot = 1; slot < betStakes.Length; slot++)
                {
                    if (betSides[slot] == winningSide) winnerPool += betStakes[slot];
                    else if (betSides[slot] != 0) otherPool += betStakes[slot];
                }
                if (winnerPool == 0)
                    for (var slot = 1; slot < betStakes.Length; slot++)
                        if (betSides[slot] == winningSide) { Halt("bet-zero-divisor"); return; }
                var positivePayout = false;
                for (var slot = 1; slot < betStakes.Length; slot++)
                {
                    if (betSides[slot] == winningSide)
                    {
                        var amount = otherPool * betStakes[slot] / winnerPool + betStakes[slot];
                        if (amount > 0) { Emit(OriginalDuelEventKind.Gold, slot, amount, "bets:payout"); positivePayout = true; }
                    }
                    else CarryPrizeGold = checked(CarryPrizeGold + betStakes[slot]);
                }
                if (positivePayout) CarryPrizeGold = 0;
            }
            var settled = Emit(OriginalDuelEventKind.BetSettled, 0, CarryPrizeGold, "bets"); settled.side = winningSide;
        }

        void StartRing()
        {
            ringActive = true; ringStartedAt = Time; ringStage = 1;
            var started = Emit(OriginalDuelEventKind.RingStarted, 0, 1, "ring");
            started.x = catalog.ringCenterX; started.y = catalog.ringCenterY; started.value = catalog.ringInitialRadius;
            var token = generation;
            Schedule(catalog.ringSecondStageTick * catalog.ringPeriod, () =>
            {
                if (ringActive && generation == token) { ringStage = 2; Emit(OriginalDuelEventKind.RingStage, 0, 2, "ring"); }
            });
        }

        void StopRing()
        {
            if (ringActive) Emit(OriginalDuelEventKind.RingStopped, 0, 0, "ring");
            ringActive = false; ringStage = 0;
        }

        int SelectNext()
        {
            var choice = 0;
            for (var slot = 1; slot < used.Length; slot++)
                if (!used[slot] && (choice == 0 || participants[slot - 1].rating > participants[choice - 1].rating)) choice = slot;
            if (choice != 0) used[choice] = true;
            return choice;
        }

        void Reward(int slot, int gold, int souls, string code)
        {
            if (gold != 0) Emit(OriginalDuelEventKind.Gold, slot, gold, code);
            if (souls != 0) Emit(OriginalDuelEventKind.Souls, slot, souls, code);
        }
        void Alliance(int slot, int otherSlot, bool allied)
        {
            var e = Emit(OriginalDuelEventKind.Alliance, slot, allied ? 1 : 0, "alliance"); e.otherSlot = otherSlot;
        }
        OriginalDuelEvent Hero(int slot, string code) { return Emit(OriginalDuelEventKind.HeroState, slot, 0, code); }
        OriginalDuelEvent World(string code, int slot = 0) { return Emit(OriginalDuelEventKind.WorldRule, slot, 0, code); }
        OriginalDuelEvent Place(int slot, double x, double y, double facing, string code)
        {
            var e = Emit(OriginalDuelEventKind.Placement, slot, 0, code); e.x = x; e.y = y;
            e.setFacing = !double.IsNaN(facing); e.facing = e.setFacing ? facing : 0;
            return e;
        }
        OriginalDuelEvent Emit(OriginalDuelEventKind kind, int slot, int amount, string code)
        {
            var e = new OriginalDuelEvent { sequence = ++sequence, time = Time, kind = kind, pair = pair, slot = slot, amount = amount, code = code };
            events.Add(e); return e;
        }
        void Schedule(double delay, Action action) { scheduled.Add(new Scheduled { at = Time + delay, order = ++scheduleOrder, action = action }); }
        void ValidateSlot(int slot) { if (slot < 1 || slot >= alive.Length) throw new ArgumentOutOfRangeException(nameof(slot)); }
        void Halt(string code)
        {
            unresolved = code; Phase = OriginalDuelPhase.Unresolved; betsOpen = false; scheduled.Clear();
            Emit(OriginalDuelEventKind.Unresolved, 0, 0, code);
        }
        uint NextRandom() { random ^= random << 13; random ^= random >> 17; random ^= random << 5; return random; }
        int RandomInt(int minimum, int maximum) { return minimum + (int)((maximum - minimum + 1.0) * (NextRandom() / 4294967296.0)); }
        double RandomReal(double minimum, double maximum) { return minimum + (maximum - minimum) * (NextRandom() / 4294967296.0); }
    }
}

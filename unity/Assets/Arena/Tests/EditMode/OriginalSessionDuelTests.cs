using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionDuelTests
    {
        static readonly string Hash = new string('a', 64);
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name)));
        static OriginalSession NewSession() => new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"),
            Load<OriginalItemCatalog>("lia39-items.json"), Load<OriginalCombatCatalog>("lia39-combat.json"),
            Load<OriginalDuelCatalog>("lia39-duels.json"), Hash, OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);

        static OriginalSessionCommand Command(OriginalSession session, long connection, OriginalSessionCommandKind kind,
            string hero = null, int side = 0, int stake = 0)
        {
            int slot = connection == 0 ? 1 : connection == 10 ? 2 : connection == 20 ? 3 : 2;
            var player = session.Snapshot().players.FirstOrDefault(p => p.slot == slot);
            return new OriginalSessionCommand { protocol = OriginalSession.Protocol, contentHash = Hash,
                sequence = kind == OriginalSessionCommandKind.Hello ? 1 : player.acknowledgedSequence + 1,
                kind = kind, heroId = hero, ready = true, betSide = side, betStake = stake };
        }

        static OriginalSession Start(int count = 3, bool reorderedSlots = false)
        {
            var session = NewSession();
            if (count >= 2) session.Apply(10, Command(session, 10, OriginalSessionCommandKind.Hello));
            if (count >= 3) session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Hello));
            if (reorderedSlots)
            {
                session.Disconnect(10);
                session.Apply(30, Command(session, 30, OriginalSessionCommandKind.Hello));
            }
            long[] connections = count == 2 ? new long[] { 0, 10 } : reorderedSlots ? new long[] { 0, 20, 30 } : new long[] { 0, 10, 20 };
            string[] heroes = { "H008", "N0A0", "H024" };
            for (int i = 0; i < connections.Length; i++)
            {
                Assert.That(session.Apply(connections[i], Command(session, connections[i], OriginalSessionCommandKind.SelectHero, heroes[i])), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                session.Apply(connections[i], Command(session, connections[i], OriginalSessionCommandKind.LobbyReady));
            }
            Assert.That(session.Apply(0, Command(session, 0, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.DrainEvents();
            return session;
        }

        static void Tick(OriginalSession session, double seconds)
        {
            while (seconds > 0) { double step = Math.Min(seconds, 1); session.Advance(step); seconds -= step; }
        }

        static void ReachDuel(OriginalSession session)
        {
            for (int round = 1; round <= 4; round++)
            {
                var before = session.Snapshot();
                Assert.That(before.round, Is.EqualTo(round));
                Tick(session, 2);
                foreach (var p in session.Snapshot().players)
                {
                    long connection = p.slot == 1 ? 0 : p.heroId == "N0A0" && p.slot == 3 ? 20 : p.slot == 3 ? 20 :
                        p.heroId == "H024" ? 30 : 10;
                    Assert.That(session.Apply(connection, Command(session, connection, OriginalSessionCommandKind.WaveReady)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                }
                Tick(session, 3);
                foreach (var enemy in session.Snapshot().enemies)
                    Assert.That(session.ReportEnemyKilled(enemy.entityId), Is.True);
                Tick(session, 3);
                session.DrainEvents();
            }
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Tick(session, 25);
            Assert.That(session.Snapshot().pendingDuel, Is.True);
        }

        static OriginalDuelParticipant[] Roster(OriginalSession session) => session.Snapshot().players.Select(p =>
            new OriginalDuelParticipant { slot = p.slot, heroRawcode = p.heroId, rating = 400 - p.slot * 100 }).ToArray();

        [Test]
        public void DepartedParticipantCanEnterDuelAndActiveDuelSurvivesAnotherDeparture()
        {
            var session = Start(); ReachDuel(session);
            Assert.That(session.Disconnect(10), Is.True);
            Assert.That(session.BeginDuel(Roster(session)), Is.True);
            Assert.That(session.Disconnect(20), Is.True);
            Tick(session, 11);
            Assert.That(session.HaltReason, Is.Null);
            Assert.That(session.Snapshot().duel.phase, Is.EqualTo(OriginalDuelPhase.Combat));
            Assert.That(session.Snapshot().initialParticipants, Is.EqualTo(3));
        }

        [Test]
        public void PendingDuelRequiresWorldRosterAndDoesNotSpendPveTime()
        {
            var session = Start(); ReachDuel(session);
            var before = session.Snapshot();
            Assert.That(before.haltReason, Is.Null);
            Assert.That(before.duel, Is.Null);
            Tick(session, 30);
            Assert.That(session.Snapshot().time, Is.EqualTo(before.time));
            Assert.That(session.Snapshot().pendingDuel, Is.True);
            Assert.That(session.ReportHeroDied(1), Is.False);
            Assert.That(session.BeginDuel(Roster(session)), Is.True);
            Assert.That(session.BeginDuel(Roster(session)), Is.False);
            var active = session.Snapshot();
            Assert.That(active.pendingDuel, Is.False);
            Assert.That(active.duel.kind, Is.EqualTo(OriginalDuelKind.Pairs));
            Assert.That(active.duel.phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            Tick(session, 11);
            Assert.That(session.Snapshot().duel.phase, Is.EqualTo(OriginalDuelPhase.Combat));
            Assert.That(session.Snapshot().time, Is.EqualTo(before.time));
        }

        [Test]
        public void BeginDuelRejectsWrongMissingOrDuplicateRosterWithoutConsumingRequest()
        {
            var session = Start(); ReachDuel(session);
            Assert.Throws<ArgumentException>(() => session.BeginDuel(Roster(session).Take(2).ToArray()));
            var wrong = Roster(session); wrong[1].heroRawcode = "H008";
            Assert.Throws<ArgumentException>(() => session.BeginDuel(wrong));
            var duplicate = Roster(session); duplicate[1].slot = 1;
            Assert.Throws<ArgumentException>(() => session.BeginDuel(duplicate));
            Assert.That(session.Snapshot().pendingDuel, Is.True);
            Assert.That(session.BeginDuel(Roster(session)), Is.True);
        }

        [Test]
        public void LobbySlotsMapToCoreSlotsForRatingsDeathsAndAliveMasks()
        {
            var session = Start(reorderedSlots: true); ReachDuel(session);
            var roster = Roster(session); roster.Single(p => p.slot == 2).rating = 999;
            session.BeginDuel(roster); Tick(session, 11);
            var before = session.Snapshot();
            Assert.That(before.players.Single(p => p.slot == 2).matchSlot, Is.EqualTo(3));
            Assert.That(before.duel.firstSlot, Is.EqualTo(3));
            Assert.That(before.duel.secondSlot, Is.EqualTo(1));
            Assert.That(session.ReportHeroDied(2, 1, (1 << 1) | (1 << 3)), Is.True);
            var after = session.Snapshot();
            Assert.That(after.duel.alive[3], Is.False);
            Assert.That(after.players.Single(p => p.slot == 2).alive, Is.False);
            Assert.That(after.players.Single(p => p.slot == 3).alive, Is.True);
            Assert.That(after.altars, Is.EqualTo(before.altars));
            Assert.That(after.phase, Is.EqualTo(OriginalMatchPhase.Duel));
        }

        [Test]
        public void FundedBetIsBoundToConnectionAndPaidExactlyOnce()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session)); session.DrainEvents();
            long spectatorGold = session.Snapshot().players.Single(p => p.slot == 3).gold;
            var bet = Command(session, 20, OriginalSessionCommandKind.Bet, side: 1, stake: 100);
            Assert.That(session.Apply(20, bet), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Snapshot().players.Single(p => p.slot == 3).gold, Is.EqualTo(spectatorGold - 100));
            Assert.That(session.Apply(20, bet), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Apply(0, Command(session, 0, OriginalSessionCommandKind.Bet, side: 1, stake: 100)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 3, stake: 100)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            var initial = session.DrainEvents();
            Assert.That(initial.Count(e => e.duelEvent?.kind == OriginalDuelEventKind.Gold), Is.EqualTo(1));
            Tick(session, 11);
            long winnerGold = session.Snapshot().players[0].gold;
            Assert.That(session.ReportHeroDied(2, 1), Is.True);
            Assert.That(session.ReportHeroDied(2, 1), Is.False);
            Assert.That(session.Snapshot().players[0].gold, Is.EqualTo(winnerGold + 250));
            Tick(session, .5);
            Assert.That(session.Snapshot().players.Single(p => p.slot == 3).gold, Is.EqualTo(spectatorGold));
            session.DrainEvents(); session.DrainEvents(); session.Snapshot();
            Assert.That(session.Snapshot().players[0].gold, Is.EqualTo(winnerGold + 250));
        }

        [Test]
        public void DiscardKeepsSourceNoRefundAndClosedBetsCannotSpend()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session));
            var gold = session.Snapshot().players[2].gold;
            session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 2, stake: 100));
            Assert.That(session.Apply(20, Command(session, 20, OriginalSessionCommandKind.DiscardBet)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Snapshot().players[2].gold, Is.EqualTo(gold - 100));
            Assert.That(session.Snapshot().duel.betStakes[3], Is.Zero);
            Tick(session, 11);
            Assert.That(session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 1, stake: 1)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(session.Snapshot().players[2].gold, Is.EqualTo(gold - 100));
        }

        [Test]
        public void BothDuelKindsCompleteOnceBeforeBossAndWorldEventsStayOrdered()
        {
            var session = Start(2); ReachDuel(session); session.DrainEvents();
            session.BeginDuel(Roster(session)); Tick(session, 11);
            session.ReportHeroDied(2, 1); Tick(session, 2);
            var afterPairs = session.Snapshot();
            Assert.That(afterPairs.phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(afterPairs.players.Select(p => p.alive), Is.All.True);
            var seriesEvents = session.DrainEvents();
            int complete = Array.FindIndex(seriesEvents, e => e.duelEvent?.kind == OriginalDuelEventKind.Completed);
            int restore = Array.FindIndex(seriesEvents, e => e.matchEvent?.kind == OriginalMatchEventKind.RestoreParty);
            Assert.That(complete, Is.GreaterThanOrEqualTo(0)); Assert.That(restore, Is.GreaterThan(complete));
            Assert.That(seriesEvents.Select(e => e.sequence), Is.Ordered.Ascending);
            Tick(session, 25);
            Assert.That(session.Snapshot().pendingDuel, Is.True);
            Assert.That(session.Snapshot().duelKind, Is.EqualTo(OriginalDuelKind.Gladiator));
            session.BeginDuel(Roster(session));
            Assert.That(session.Snapshot().duelId, Is.EqualTo(afterPairs.duelId + 1));
            Tick(session, 10);
            long souls = session.Snapshot().players[0].souls;
            session.ReportHeroDied(2, 1); Tick(session, 2);
            var final = session.Snapshot();
            Assert.That(final.round, Is.EqualTo(5));
            Assert.That(final.phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            Assert.That(final.players[0].souls, Is.EqualTo(souls + 14 + 8));
            Assert.That(session.BeginDuel(Roster(session)), Is.False);
            Assert.That(session.Snapshot().players[0].souls, Is.EqualTo(final.players[0].souls));
        }

        [Test]
        public void LosingBetCarrySurvivesIntoGladiatorInstance()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session));
            session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 2, stake: 100));
            Tick(session, 11); session.ReportHeroDied(2, 1); Tick(session, 2);
            Assert.That(session.Snapshot().duel.carryPrizeGold, Is.EqualTo(100));
            Tick(session, 25); session.BeginDuel(Roster(session));
            Assert.That(session.Snapshot().duel.carryPrizeGold, Is.EqualTo(100));
        }

        [Test]
        public void CompletionSpendsOnlyTheRemainingStepInTheNextPveTimer()
        {
            var session = Start(2); ReachDuel(session); session.BeginDuel(Roster(session));
            Tick(session, 11); session.ReportHeroDied(2, 1); Tick(session, 1.5);
            double before = session.Snapshot().time;
            session.Advance(1);
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(session.Snapshot().time, Is.EqualTo(before + .5).Within(.000001));
            Assert.That(session.Snapshot().remainingSeconds, Is.EqualTo(24.5).Within(.000001));
        }

        [Test]
        public void MalformedAliveMaskCannotPartiallyApplyDeathOrReward()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session)); Tick(session, 11);
            var before = session.Snapshot();
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ReportHeroDied(2, 1, 1 << 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ReportHeroDied(2, 1, 1 << 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ReportHeroDied(2, 8));
            var after = session.Snapshot();
            Assert.That(after.duel.phase, Is.EqualTo(OriginalDuelPhase.Combat));
            Assert.That(after.players.Select(p => p.gold), Is.EqualTo(before.players.Select(p => p.gold)));
            Assert.That(after.duel.alive, Is.EqualTo(before.duel.alive));
            Assert.That(after.revision, Is.EqualTo(before.revision));
        }

        [Test]
        public void BetAdjustmentRefundsOnlyPriorStakeAndUnfundedBetIsRejected()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session));
            long gold = session.Snapshot().players[2].gold;
            session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 1, stake: 100));
            session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 2, stake: 40));
            Assert.That(session.Snapshot().players[2].gold, Is.EqualTo(gold - 40));
            session.Apply(20, Command(session, 20, OriginalSessionCommandKind.DiscardBet));
            int guard = 0;
            while (session.Snapshot().players[2].gold >= 100)
            {
                Assert.That(++guard, Is.LessThan(64));
                session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 1, stake: 100));
                session.Apply(20, Command(session, 20, OriginalSessionCommandKind.DiscardBet));
            }
            long remaining = session.Snapshot().players[2].gold;
            Assert.That(session.Apply(20, Command(session, 20, OriginalSessionCommandKind.Bet, side: 1, stake: 100)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(session.Snapshot().players[2].gold, Is.EqualTo(remaining));
            Assert.That(session.Snapshot().duel.betStakes[3], Is.Zero);
        }

        [Test]
        public void MissingGladiatorKillerRemainsExplicitAndCannotAdvanceMatch()
        {
            var session = Start(2); ReachDuel(session); session.BeginDuel(Roster(session));
            Tick(session, 11); session.ReportHeroDied(2, 1); Tick(session, 27); session.BeginDuel(Roster(session));
            Tick(session, 10); Assert.That(session.ReportHeroDied(2), Is.True);
            var halted = session.Snapshot();
            Assert.That(halted.haltReason, Is.EqualTo("gladiator-killer-owner-unknown"));
            Tick(session, 30);
            Assert.That(session.Snapshot().round, Is.EqualTo(4));
            Assert.That(session.Snapshot().time, Is.EqualTo(halted.time));
        }

        [Test]
        public void DuelSnapshotIsDetachedAndNativeAliveObservationIsHostOnly()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session));
            var before = session.Snapshot();
            var decoded = JsonUtility.FromJson<OriginalSessionView>(JsonUtility.ToJson(before));
            Assert.That(decoded.duelId, Is.EqualTo(before.duelId));
            Assert.That(decoded.duel.firstSlot, Is.EqualTo(before.duel.firstSlot));
            Assert.That(decoded.duel.participants.Select(p => p.rating), Is.EqualTo(before.duel.participants.Select(p => p.rating)));
            before.duel.participants[0].rating = -999;
            before.duel.alive[1] = false; before.duel.betStakes[1] = 999;
            Assert.That(session.Snapshot().duel.participants[0].rating, Is.Not.EqualTo(-999));
            Assert.That(session.Snapshot().duel.alive[1], Is.True);
            Assert.That(session.Snapshot().duel.betStakes[1], Is.Zero);
            Assert.That(session.ObserveDuelHeroAlive(1, false), Is.True);
            Assert.That(session.Snapshot().players[0].alive, Is.False);
            Assert.That(session.UpdateDuelRating(3, 555), Is.True);
            Assert.That(session.Snapshot().duel.participants.Single(p => p.slot == 3).rating, Is.EqualTo(555));
        }

        [Test]
        public void PveDeathAndKillAreTrustedHostReportsWithNoDuplicateXp()
        {
            var session = Start(2); Tick(session, 2);
            session.Apply(0, Command(session, 0, OriginalSessionCommandKind.WaveReady));
            session.Apply(10, Command(session, 10, OriginalSessionCommandKind.WaveReady)); Tick(session, 3);
            int entity = session.Snapshot().enemies[0].entityId;
            Assert.That(session.ReportEnemyKilled(entity), Is.True);
            int xp = session.Snapshot().players[0].experience;
            Assert.That(xp, Is.GreaterThan(0));
            Assert.That(session.ReportEnemyKilled(entity), Is.False);
            Assert.That(session.Snapshot().players[0].experience, Is.EqualTo(xp));
            Assert.That(session.ReportHeroDied(2), Is.True);
            Assert.That(session.Snapshot().players.Single(p => p.slot == 2).alive, Is.False);
            Assert.That(session.ReportHeroDied(2), Is.False);
        }

        [Test]
        public void DisconnectContinuesWithoutFabricatingDuelDeath()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session));
            Assert.That(session.Disconnect(20), Is.True);
            var before = session.Snapshot(); Tick(session, 30);
            Assert.That(before.haltReason, Is.Null);
            Assert.That(before.duel.phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            Assert.That(session.Snapshot().duel.time, Is.GreaterThan(before.duel.time));
            Assert.That(session.Snapshot().duel.alive, Is.EqualTo(before.duel.alive));
        }

        [Test]
        public void ClientCommandsCannotGrowWorldEventQueueWhileHostIsNotAdvancing()
        {
            var session = Start(); ReachDuel(session); session.BeginDuel(Roster(session)); session.DrainEvents();
            int accepted = 0;
            for (int i = 0; i < 6000; i++)
                if (session.Apply(20, Command(session, 20, OriginalSessionCommandKind.DiscardBet)) == OriginalSessionReplyCode.Accepted) accepted++;
            var stopped = session.Snapshot();
            Assert.That(stopped.haltReason, Is.EqualTo("host-event-consumer-backpressure"));
            Assert.That(accepted, Is.LessThanOrEqualTo(4096));
            Assert.That(session.DrainEvents().Length, Is.LessThanOrEqualTo(4096));
            long acknowledged = stopped.players.Single(p => p.slot == 3).acknowledgedSequence;
            Assert.That(session.Apply(20, Command(session, 20, OriginalSessionCommandKind.DiscardBet)), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(session.Snapshot().players.Single(p => p.slot == 3).acknowledgedSequence, Is.EqualTo(acknowledged + 1));
            Assert.That(session.DrainEvents(), Is.Empty);
        }
    }
}

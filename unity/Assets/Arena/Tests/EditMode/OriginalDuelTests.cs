using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalDuelTests
    {
        static OriginalDuelCatalog Catalog() { return JsonUtility.FromJson<OriginalDuelCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-duels.json"))); }
        static OriginalDuelParticipant[] Roster(int count)
        {
            var ids = new[] { "H008", "N0A0", "H024" };
            return Enumerable.Range(1, count).Select(i => new OriginalDuelParticipant
                { slot = i, heroRawcode = ids[(i - 1) % 3], rating = 100 - i }).ToArray();
        }
        static OriginalDuel Create(int count = 2, OriginalDuelKind kind = OriginalDuelKind.Pairs,
            int carry = 0, int round = 4, OriginalDuelParticipant[] roster = null, int seed = 12345)
        {
            var duel = new OriginalDuel(Catalog(), kind, round, roster ?? Roster(count), seed, carry);
            Assert.That(duel.Begin(), Is.True);
            return duel;
        }
        static int Total(OriginalDuelEvent[] events, OriginalDuelEventKind kind, int slot)
        { return events.Where(e => e.kind == kind && e.slot == slot).Sum(e => e.amount); }

        [Test] public void CatalogHasSourceProvenanceAndRejectsChangedTimings()
        {
            var catalog = Catalog(); catalog.Validate();
            Assert.That(catalog.sources.Any(s => s.function == "Xpv" && s.line >= 32500), Is.True);
            Assert.That(catalog.rules.Single(r => r.id == "bets").unresolved.Length, Is.GreaterThan(0));
            catalog.pairCountdownSeconds = 10;
            Assert.Throws<ArgumentException>(() => catalog.Validate());
        }

        [Test] public void PairSelectionUsesStableDescendingRatingAndExactPlacement()
        {
            var roster = Roster(5); roster[2].rating = 200; roster[4].rating = 200;
            var duel = Create(roster: roster);
            Assert.That(duel.Snapshot().firstSlot, Is.EqualTo(3));
            Assert.That(duel.Snapshot().secondSlot, Is.EqualTo(5));
            var placements = duel.DrainEvents().Where(e => e.kind == OriginalDuelEventKind.Placement).ToArray();
            Assert.That(placements.Single(e => e.slot == 3).x, Is.EqualTo(-416));
            Assert.That(placements.Single(e => e.slot == 5).y, Is.EqualTo(-2688));
            Assert.That(placements.Single(e => e.slot == 5).facing, Is.EqualTo(180));
            Assert.That(OriginalDuel.SourceRating(2, 3, 4, 5), Is.EqualTo(43));
        }

        [Test] public void PairCountdownHasElevenSecondsAndGladiatorHasTen()
        {
            var pairs = Create(); pairs.Advance(10);
            Assert.That(pairs.Phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            Assert.That(pairs.DrainEvents().Where(e => e.kind == OriginalDuelEventKind.Countdown).Select(e => e.amount),
                Is.EqualTo(Enumerable.Range(1, 10).Reverse()));
            pairs.Advance(1); Assert.That(pairs.Phase, Is.EqualTo(OriginalDuelPhase.Combat));
            var glad = Create(kind: OriginalDuelKind.Gladiator); glad.Advance(9);
            Assert.That(glad.Phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            glad.Advance(1); Assert.That(glad.Phase, Is.EqualTo(OriginalDuelPhase.Combat));
        }

        [Test] public void DeathPaysOnceThenCompletesAfterTwoSeconds()
        {
            var duel = Create(carry: 77); duel.Advance(11); duel.DrainEvents();
            Assert.That(duel.HeroDied(2, 1), Is.True);
            Assert.That(duel.HeroDied(2, 1), Is.False);
            var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 1), Is.EqualTo(327));
            Assert.That(Total(events, OriginalDuelEventKind.Souls, 1), Is.EqualTo(10));
            Assert.That(Total(events, OriginalDuelEventKind.Win, 1), Is.EqualTo(1));
            Assert.That(Total(events, OriginalDuelEventKind.Loss, 2), Is.EqualTo(1));
            Assert.That(duel.CarryPrizeGold, Is.Zero);
            duel.Advance(1.99); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Resolving));
            duel.Advance(.01); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Completed));
            Assert.That(duel.DrainEvents().Count(e => e.kind == OriginalDuelEventKind.Completed), Is.EqualTo(1));
            duel.Advance(1000); Assert.That(duel.DrainEvents(), Is.Empty);
        }

        [Test] public void BetsDebitImmediatelyLockAtCombatAndPayOnlyAfterHalfSecond()
        {
            var duel = Create(4); duel.DrainEvents();
            Assert.That(duel.TrySetBet(1, 1, 20, 20), Is.False);
            Assert.That(duel.TrySetBet(3, 1, 100, 99), Is.False);
            Assert.That(duel.TrySetBet(3, 1, 100, 100), Is.True);
            Assert.That(duel.TrySetBet(4, 2, 101, 101), Is.True);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(-100));
            duel.Advance(11);
            Assert.That(duel.TrySetBet(3, 2, 0, 0), Is.False);
            duel.DrainEvents(); duel.HeroDied(2, 1);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.Zero);
            duel.Advance(.499); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.Zero);
            duel.Advance(.001); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(201));
        }

        [Test] public void LosingBetsFundNextPairNotCurrentWinner()
        {
            var duel = Create(4, carry: 50); duel.TrySetBet(3, 2, 70, 70); duel.Advance(11); duel.DrainEvents();
            duel.HeroDied(2, 1);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 1), Is.EqualTo(300));
            duel.Advance(.5); Assert.That(duel.CarryPrizeGold, Is.EqualTo(70));
            duel.Advance(1.5); Assert.That(duel.Snapshot().firstSlot, Is.EqualTo(3));
            duel.Advance(11); duel.DrainEvents(); duel.HeroDied(4, 3);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(320));
        }

        [Test] public void DrawRefundsStakesAndPreservesCarry()
        {
            var duel = Create(3, carry: 40); duel.TrySetBet(3, 0, 90, 90); duel.Advance(11); duel.DrainEvents();
            duel.Advance(120); var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 1), Is.EqualTo(125));
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 2), Is.EqualTo(125));
            Assert.That(events.Any(e => e.kind == OriginalDuelEventKind.Win || e.kind == OriginalDuelEventKind.Loss), Is.False);
            duel.Advance(.5); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(90));
            Assert.That(duel.CarryPrizeGold, Is.EqualTo(40));
        }

        [Test] public void MirrorRedirectsPrizeAndLossToSamePlayerButBetsUsePhysicalWinner()
        {
            var roster = Roster(3); roster[0].mirrorCurse = true;
            var duel = Create(roster: roster); duel.TrySetBet(3, 1, 60, 60); duel.Advance(11); duel.DrainEvents();
            duel.HeroDied(2, 1); var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 1), Is.Zero);
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 2), Is.EqualTo(250));
            Assert.That(Total(events, OriginalDuelEventKind.Win, 2), Is.EqualTo(1));
            Assert.That(Total(events, OriginalDuelEventKind.Loss, 2), Is.EqualTo(1));
            duel.Advance(.5); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(60));
        }

        [Test] public void MirrorOnFirstFighterAlsoRedirectsDrawHalf()
        {
            var roster = Roster(2); roster[0].mirrorCurse = true;
            var duel = Create(roster: roster, carry: 100); duel.DrainEvents(); duel.Advance(131);
            var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 1), Is.Zero);
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 2), Is.EqualTo(250));
            Assert.That(Total(events, OriginalDuelEventKind.Souls, 2), Is.EqualTo(10));
            Assert.That(duel.CarryPrizeGold, Is.EqualTo(100));
        }

        [Test] public void SimultaneousPairDeathSuppressesPrizeButKeepsPhysicalBetSide()
        {
            var duel = Create(3, carry: 60); duel.TrySetBet(3, 2, 50, 50); duel.Advance(11); duel.DrainEvents();
            duel.HeroDied(1, 2, 1 << 3);
            Assert.That(duel.DrainEvents().Any(e => e.kind == OriginalDuelEventKind.Gold || e.kind == OriginalDuelEventKind.Souls), Is.False);
            duel.Advance(.5); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(50));
            Assert.That(duel.CarryPrizeGold, Is.Zero); // A positive bet payout clears the prior carry too.
        }

        [Test] public void EmptyWinningPoolWithSelectedZeroStakeHaltsExplicitly()
        {
            var duel = Create(3); duel.TrySetBet(3, 1, 0, 0); duel.Advance(11); duel.HeroDied(2, 1); duel.Advance(.5);
            Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Unresolved));
            Assert.That(duel.Snapshot().unresolved, Is.EqualTo("bet-zero-divisor"));
            duel.Advance(100); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Unresolved));
        }

        [Test] public void DiscardingBetDoesNotRefundAndAdjustingBetDoes()
        {
            var duel = Create(3); duel.TrySetBet(3, 2, 100, 100); duel.DrainEvents();
            duel.TrySetBet(3, 2, 60, 0);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(40));
            Assert.That(duel.DiscardBet(3), Is.True);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.Zero);
            Assert.That(duel.Snapshot().betStakes[3], Is.Zero);
        }

        [Test] public void GladiatorShufflesAllSlotsOnSourceCircleAndPaysOnlySouls()
        {
            var duel = Create(4, OriginalDuelKind.Gladiator, carry: 70, round: 29);
            var placement = duel.DrainEvents().Where(e => e.kind == OriginalDuelEventKind.Placement).ToArray();
            Assert.That(placement.Select(e => e.slot).OrderBy(x => x), Is.EqualTo(new[] { 1, 2, 3, 4 }));
            foreach (var p in placement) Assert.That(Math.Sqrt(p.x * p.x + (p.y + 2700) * (p.y + 2700)), Is.EqualTo(525).Within(.0001));
            Assert.That(duel.TrySetBet(3, 1, 10, 10), Is.False);
            duel.Advance(10); duel.HeroDied(1, 4); duel.HeroDied(2, 4); duel.DrainEvents(); duel.HeroDied(3, 4);
            var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Souls, 4), Is.EqualTo(39));
            Assert.That(events.Any(e => e.kind == OriginalDuelEventKind.Gold || e.kind == OriginalDuelEventKind.Loss), Is.False);
            Assert.That(duel.CarryPrizeGold, Is.EqualTo(70));
        }

        [Test] public void GladiatorDeadKillerCreditFallsBackToLastLivingHero()
        {
            var duel = Create(3, OriginalDuelKind.Gladiator); duel.Advance(10);
            duel.HeroDied(1, 2); duel.DrainEvents(); duel.HeroDied(2, 1);
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Souls, 3), Is.EqualTo(14));
        }

        [Test] public void GladiatorTimeoutHasNoWinnerAndNoCurrency()
        {
            var duel = Create(3, OriginalDuelKind.Gladiator); duel.Advance(130);
            Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Resolving));
            var events = duel.DrainEvents();
            Assert.That(events.Single(e => e.kind == OriginalDuelEventKind.Result).code, Is.EqualTo("no-winner"));
            Assert.That(events.Any(e => e.kind == OriginalDuelEventKind.Gold || e.kind == OriginalDuelEventKind.Souls), Is.False);
            duel.Advance(2); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Completed));
        }

        [Test] public void RingBeginsAtSixtyOneSecondsAndSecondStageAtTick801()
        {
            var duel = Create(); duel.Advance(11 + 60.999);
            Assert.That(duel.Snapshot().ringStage, Is.Zero);
            duel.Advance(.001); Assert.That(duel.Snapshot().ringStage, Is.EqualTo(1));
            Assert.That(duel.Snapshot().ringRadius, Is.EqualTo(810));
            duel.Advance(32); Assert.That(duel.Snapshot().ringStage, Is.EqualTo(1));
            Assert.That(duel.Snapshot().ringRadius, Is.EqualTo(810 - 799 * .52).Within(.00001));
            duel.Advance(.04); Assert.That(duel.Snapshot().ringStage, Is.EqualTo(2));
            duel.Advance(20); Assert.That(duel.Snapshot().ringRadius, Is.EqualTo(325));
            duel.HeroDied(2, 1); Assert.That(duel.Snapshot().ringStage, Is.EqualTo(2));
            duel.Advance(2); Assert.That(duel.Snapshot().ringStage, Is.Zero);
        }

        [Test] public void FullEightPlayerSeriesHasFourPairsAndOddPlayerGetsNoPrize()
        {
            foreach (var count in new[] { 3, 8 })
            {
                var duel = Create(count); var all = duel.DrainEvents().ToList();
                for (var pair = 0; pair < count / 2; pair++)
                {
                    duel.Advance(11); var snapshot = duel.Snapshot();
                    Assert.That(snapshot.pair, Is.EqualTo(pair + 1));
                    duel.HeroDied(snapshot.secondSlot, snapshot.firstSlot); duel.Advance(2);
                    all.AddRange(duel.DrainEvents());
                }
                Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Completed));
                Assert.That(all.Count(e => e.kind == OriginalDuelEventKind.Win), Is.EqualTo(count / 2));
                if (count == 3) Assert.That(Total(all.ToArray(), OriginalDuelEventKind.Gold, 3), Is.Zero);
            }
        }

        [Test] public void StartupDeadFighterUsesNoDeathContextAndRefundsBets()
        {
            var duel = Create(3); duel.TrySetBet(3, 1, 80, 80); duel.ObserveHeroAlive(1, false); duel.DrainEvents();
            duel.Advance(11); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Resolving));
            Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 2), Is.EqualTo(250));
            duel.Advance(.5); Assert.That(Total(duel.DrainEvents(), OriginalDuelEventKind.Gold, 3), Is.EqualTo(80));
        }

        [Test] public void SourceReturnDoesNotResurrectOrRefillMana()
        {
            var duel = Create(); duel.Advance(11); duel.HeroDied(2, 1); duel.DrainEvents(); duel.Advance(.5);
            var events = duel.DrainEvents();
            Assert.That(events.Where(e => e.kind == OriginalDuelEventKind.Placement).Count(), Is.EqualTo(2));
            foreach (var e in events.Where(e => e.kind == OriginalDuelEventKind.HeroState))
            { Assert.That(e.revive, Is.False); Assert.That(e.fullMana, Is.False); }
            Assert.That(duel.Snapshot().alive[2], Is.False);
        }

        [Test] public void SnapshotAndInputsDoNotMutateRefereeAndDisconnectKeepsPairing()
        {
            var roster = Roster(3); var catalog = Catalog();
            var duel = new OriginalDuel(catalog, OriginalDuelKind.Pairs, 4, roster, 1); duel.Begin();
            roster[0].mirrorCurse = true; catalog.pairGold = 999;
            var snap = duel.Snapshot(); snap.participants[0].mirrorCurse = true; snap.betStakes[3] = 999;
            Assert.That(duel.Snapshot().participants[0].mirrorCurse, Is.False);
            Assert.That(duel.Snapshot().betStakes[3], Is.Zero);
            duel.ParticipantDisconnected(3); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            duel.Advance(11); Assert.That(duel.Phase, Is.EqualTo(OriginalDuelPhase.Combat));
            Assert.That(duel.Begin(), Is.False);
        }

        [Test] public void EveryEventContainsFiniteNetworkValuesAndIncreasingSequence()
        {
            var duel = Create(3); var all = duel.DrainEvents().ToList();
            duel.Advance(11); duel.HeroDied(2, 1); duel.Advance(2); all.AddRange(duel.DrainEvents());
            Assert.That(all.Select(e => e.sequence), Is.EqualTo(Enumerable.Range(1, all.Count).Select(i => (long)i)));
            foreach (var e in all)
                foreach (var value in new[] { e.time, e.x, e.y, e.facing, e.value })
                    Assert.That(double.IsNaN(value) || double.IsInfinity(value), Is.False, e.code);
        }

        [Test] public void PairRefillFollowsCleanupAndRevivalCarriesExplicitTargetCoordinates()
        {
            var duel = Create(); var events = duel.DrainEvents();
            var cleanup = Array.FindIndex(events, e => e.kind == OriginalDuelEventKind.WorldRule && e.code == "pair-prepare:Z2-cleanup");
            var refill = Array.FindIndex(events, e => e.slot == 1 && e.fullLife && e.fullMana);
            Assert.That(refill, Is.GreaterThan(cleanup));
            var revive = events.Single(e => e.slot == 1 && e.revive);
            Assert.That(revive.kind, Is.EqualTo(OriginalDuelEventKind.Placement));
            Assert.That(revive.x, Is.EqualTo(-416)); Assert.That(revive.y, Is.EqualTo(-2688));
            Assert.That(revive.setFacing, Is.True);
        }

        [Test] public void BloodPoisonCurseUsesSourceCoinFlipAndCanInfectRedirectedRecipient()
        {
            var roster = Roster(2); roster[1].bloodPoisonCurse = true;
            var duel = Create(roster: roster, seed: 1); duel.Advance(11); duel.DrainEvents(); duel.HeroDied(2, 1);
            Assert.That(duel.Snapshot().participants[0].bloodPoisonCurse, Is.True);
            Assert.That(duel.DrainEvents().Single(e => e.kind == OriginalDuelEventKind.Curse).code, Is.EqualTo("A19Q"));
            var mirrorRoster = Roster(2); mirrorRoster[0].mirrorCurse = true; mirrorRoster[1].bloodPoisonCurse = true;
            var mirror = Create(roster: mirrorRoster, seed: 1); mirror.Advance(11); mirror.HeroDied(2, 1);
            Assert.That(mirror.DrainEvents().Single(e => e.kind == OriginalDuelEventKind.Curse).slot, Is.EqualTo(2));
        }

        [Test] public void BetRoundingAndUnselectedStakeAreNotRedistributed()
        {
            var duel = Create(6); duel.TrySetBet(3, 1, 1, 1); duel.TrySetBet(4, 1, 2, 2);
            duel.TrySetBet(5, 2, 2, 2); duel.TrySetBet(6, 0, 100, 100);
            duel.Advance(11); duel.HeroDied(2, 1); duel.DrainEvents(); duel.Advance(.5);
            var events = duel.DrainEvents();
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 3), Is.EqualTo(1));
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 4), Is.EqualTo(3));
            Assert.That(Total(events, OriginalDuelEventKind.Gold, 6), Is.Zero);
            Assert.That(duel.CarryPrizeGold, Is.Zero);
        }

        [Test] public void LastPairCarrySurvivesGladiatorAndNextRound()
        {
            var pair = Create(3); pair.TrySetBet(3, 0, 57, 57); pair.Advance(11); pair.HeroDied(2, 1); pair.Advance(2);
            Assert.That(pair.CarryPrizeGold, Is.EqualTo(57));
            var glad = Create(3, OriginalDuelKind.Gladiator, pair.CarryPrizeGold); glad.Advance(132);
            var next = Create(3, carry: glad.CarryPrizeGold, round: 9); next.Advance(11); next.DrainEvents(); next.HeroDied(2, 1);
            Assert.That(Total(next.DrainEvents(), OriginalDuelEventKind.Gold, 1), Is.EqualTo(307));
        }

        [Test] public void AdvanceInSmallStepsHasSameResultAndEventOrderAsOneLargeStep()
        {
            var single = Create(8, OriginalDuelKind.Gladiator, seed: 99);
            var split = Create(8, OriginalDuelKind.Gladiator, seed: 99);
            single.Advance(132);
            for (var i = 0; i < 1320; i++) split.Advance(.1);
            var expected = single.DrainEvents(); var actual = split.DrainEvents();
            Assert.That(actual.Select(e => e.code), Is.EqualTo(expected.Select(e => e.code)));
            Assert.That(actual.Select(e => e.slot), Is.EqualTo(expected.Select(e => e.slot)));
            Assert.That(split.Phase, Is.EqualTo(single.Phase));
            for (var i = 0; i < actual.Length; i++) Assert.That(actual[i].time, Is.EqualTo(expected[i].time).Within(1e-8));
        }

        [Test] public void InvalidParticipantsInputsAndDuplicateCallbacksCannotMintCurrency()
        {
            Assert.Throws<ArgumentException>(() => new OriginalDuel(Catalog(), OriginalDuelKind.Pairs, 4, Roster(1), 1));
            var invalid = Roster(2); invalid[1].slot = 1;
            Assert.Throws<ArgumentException>(() => new OriginalDuel(Catalog(), OriginalDuelKind.Pairs, 4, invalid, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OriginalDuel(Catalog(), OriginalDuelKind.Pairs, 5, Roster(2), 1));
            var duel = Create(3);
            Assert.That(duel.Begin(), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => duel.Advance(double.NaN));
            Assert.That(duel.TrySetBet(3, 1, 1001, 1001), Is.False);
            Assert.That(duel.TrySetBet(3, 1, -1, 100), Is.False);
            Assert.That(duel.HeroDied(2, 1), Is.False);
            duel.Advance(11); duel.DrainEvents();
            Assert.Throws<ArgumentOutOfRangeException>(() => duel.HeroDied(2, 1, 1 << 2));
            Assert.That(duel.HeroDied(3, 1), Is.False);
            Assert.That(duel.DrainEvents(), Is.Empty);
        }
    }
}

using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalDuelRatingLedgerTests
    {
        [Test] public void RegularKillCreditFollowsOwnerAndSourceFilters()
        {
            var ledger = new OriginalDuelRatingLedger(2);
            ledger.RecordEnemyDeath(1, 0, false, false);
            ledger.RecordEnemyDeath(2, 0, false, false); // Same API for a summon owned by slot2.
            ledger.RecordEnemyDeath(1, 0, true, false);
            ledger.RecordEnemyDeath(1, 0, false, true);
            ledger.RecordEnemyDeath(0, 0, false, false);
            ledger.RecordEnemyDeath(1, 2, false, false);
            Assert.That(ledger.Snapshot().regularKills, Is.EqualTo(new[] { 1, 1 }));
            Assert.That(ledger.Rating(1, 3), Is.EqualTo(32));
        }
        [Test] public void BossDamageUsesCappedGlobalPoolAndSettlesWithoutKillOrA0K4Filter()
        {
            var ledger = new OriginalDuelRatingLedger(2);
            ledger.RecordEnemyDamage(1, 1, 100, 30); // BossA: only30 HP remains.
            ledger.RecordEnemyDamage(2, 1, 10, 100); // BossB: same global qH pool.
            ledger.RecordEnemyDamage(1, 0, 900, 900); // Regular damage is not qH.
            ledger.RecordEnemyDamage(0, 1, 999, 999); // Source slot0 excluded from denominator.
            ledger.RecordEnemyDeath(0, 1, true, true);
            Assert.That(ledger.Snapshot().bossDamagePoints, Is.EqualTo(new[] { 30, 10 }));
            Assert.That(ledger.Snapshot().pendingBossDamage, Is.EqualTo(new float[8]));
            Assert.That(ledger.Snapshot().regularKills, Is.EqualTo(new[] { 0, 0 }));
        }
        [Test] public void IndependentRoundedSharesMaySumTo39AndMustNotBeRenormalized()
        {
            var ledger = new OriginalDuelRatingLedger(3);
            for (int slot = 1; slot <= 3; slot++) ledger.RecordEnemyDamage(slot, 1, 1, 1);
            ledger.RecordEnemyDeath(1, 1, false, false);
            Assert.That(ledger.Snapshot().bossDamagePoints, Is.EqualTo(new[] { 13, 13, 13 }));
        }
        [Test] public void ZeroBossDenominatorRemainsUnresolvedWithoutPublishingPoints()
        {
            var ledger = new OriginalDuelRatingLedger(1);
            Assert.Throws<InvalidOperationException>(() => ledger.RecordEnemyDeath(1, 1, false, false));
            Assert.That(ledger.Snapshot().bossDamagePoints, Is.EqualTo(new[] { 0 }));
            Assert.That(ledger.Snapshot().pendingBossDamage, Is.EqualTo(new float[8]));
            Assert.Throws<ArgumentOutOfRangeException>(() => ledger.RecordEnemyDamage(1, 1, double.NaN, 10));
        }
        [Test] public void EvenSecondDeathPenaltyAndEndOfRoundAliveBonusMatchUkMeaning()
        {
            var ledger = new OriginalDuelRatingLedger(2); ledger.BeginCombat();
            ledger.TickCombatSecond(new[] { true, true });
            Assert.That(ledger.Snapshot().remainingSurvivalPoints, Is.EqualTo(new[] { 30, 30 }));
            ledger.TickCombatSecond(new[] { true, false });
            Assert.That(ledger.Snapshot().remainingSurvivalPoints, Is.EqualTo(new[] { 29, 30 }));
            for (int i = 2; i < 70; i++) ledger.TickCombatSecond(new[] { true, true });
            Assert.That(ledger.Snapshot().remainingSurvivalPoints, Is.EqualTo(new[] { 0, 0 }));
            ledger.CompleteRound(new[] { true, false });
            Assert.That(ledger.Snapshot().survivalPoints, Is.EqualTo(new[] { 0, 30 }));
            ledger.BeginCombat(); ledger.CompleteRound(new[] { false, true });
            Assert.That(ledger.Snapshot().survivalPoints, Is.EqualTo(new[] { 30, 60 }));
        }
        [Test] public void SnapshotsAreDetachedAndInvalidLifecycleLeavesStateUnchanged()
        {
            var ledger = new OriginalDuelRatingLedger(1);
            Assert.Throws<InvalidOperationException>(() => ledger.TickCombatSecond(new[] { false }));
            ledger.BeginCombat(); var snapshot = ledger.Snapshot(); snapshot.remainingSurvivalPoints[0] = 999;
            Assert.Throws<ArgumentException>(() => ledger.TickCombatSecond(new bool[2]));
            ledger.TickCombatSecond(new[] { true }); ledger.TickCombatSecond(new[] { true });
            Assert.That(ledger.Snapshot().remainingSurvivalPoints[0], Is.EqualTo(29));
            ledger.RecordEnemyDamage(1, 1, 20, 50);
            ledger.BeginCombat(); // Source Q3 replaces the timer on an altar retry.
            Assert.That(ledger.Snapshot().remainingSurvivalPoints[0], Is.EqualTo(30));
            Assert.That(ledger.Snapshot().combatSeconds, Is.Zero);
            Assert.That(ledger.Snapshot().survivalPoints[0], Is.Zero);
            Assert.That(ledger.Snapshot().pendingBossDamage, Is.EqualTo(new float[8]));
        }
    }
}

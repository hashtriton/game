using System.Linq;
using Arena.Original;
using NUnit.Framework;
using static Arena.Tests.OriginalWorldOptionTestSupport;

namespace Arena.Tests
{
    public sealed class OriginalSessionRoundBonusTests
    {
        [Test] public void AcolyteAddsAllThreeZeroPoolAwardsOnlyAfterItsSourceDelayAndOnce()
        {
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Easy);var s=Create(options);
            var before=s.Snapshot().players[0].combat;
            Call(s,"OnRoundBonusMatchEvent",new OriginalMatchEvent{sequence=100,time=0,kind=OriginalMatchEventKind.ShopAccess,round=2,enabled=true});
            AdvanceWorldOnly(s,.95,"AdvanceRoundBonuses");
            Assert.That(s.Snapshot().players[0].combat.strength,Is.EqualTo(before.strength));
            AdvanceWorldOnly(s,.1,"AdvanceRoundBonuses");var after=s.Snapshot().players[0].combat;
            Assert.That(after.strength-before.strength,Is.EqualTo(2));Assert.That(after.agility-before.agility,Is.EqualTo(2));Assert.That(after.intelligence-before.intelligence,Is.EqualTo(2));
            AdvanceWorldOnly(s,2,"AdvanceRoundBonuses");Assert.That(s.Snapshot().players[0].combat.strength,Is.EqualTo(after.strength));
        }
        [Test] public void DisabledAcolyteDoesNotGrantAttributes()
        {
            var s=Create();var before=s.Snapshot().players[0].combat;
            Call(s,"OnRoundBonusMatchEvent",new OriginalMatchEvent{sequence=100,time=0,kind=OriginalMatchEventKind.ShopAccess,round=2,enabled=true});
            AdvanceWorldOnly(s,2,"AdvanceRoundBonuses");
            Assert.That(s.Snapshot().players[0].combat.strength,Is.EqualTo(before.strength));
        }
        [Test] public void AcolyteWaitsForTheCompletePairAndGladiatorSeriesBeforeAwardingOnce()
        {
            var s=Create(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Easy),2);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Hidden).GetValue(s);
            match.DrainEvents();typeof(OriginalMatch).GetProperty("Round").SetValue(match,4);
            typeof(OriginalMatch).GetProperty("Phase").SetValue(match,OriginalMatchPhase.Combat);
            var before=s.Snapshot().players[0].combat;
            void Consume()
            {
                foreach(var item in match.DrainEvents().Where(e=>e.kind==OriginalMatchEventKind.ShopAccess&&e.enabled))
                    Call(s,"OnRoundBonusMatchEvent",item);
                AdvanceWorldOnly(s,1.05,"AdvanceRoundBonuses");
            }
            typeof(OriginalMatch).GetMethod("PrepareNextRound",Hidden).Invoke(match,null);Consume();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(s.Snapshot().players[0].combat.strength-before.strength,Is.Zero,"D4 returns before OU before pairs");
            match.Advance(25);match.CompleteDuelSequence();Consume();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(s.Snapshot().players[0].combat.strength-before.strength,Is.Zero,"D4 returns before OU before gladiators");
            match.Advance(25);match.CompleteDuelSequence();Consume();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Preparation));Assert.That(match.Round,Is.EqualTo(5));
            var after=s.Snapshot().players[0].combat;
            Assert.That(after.strength-before.strength,Is.EqualTo(2));Assert.That(after.agility-before.agility,Is.EqualTo(2));
            Assert.That(after.intelligence-before.intelligence,Is.EqualTo(2));
        }
    }
}

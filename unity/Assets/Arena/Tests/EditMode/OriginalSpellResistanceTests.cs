using System;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSpellResistanceTests
    {
        [Test] public void LastAddedResistanceWinsAndRemovalRestoresPreviousSource()
        {
            var rules = new OriginalSpellResistanceLedger();
            rules.Set(1, "A0E5", .4);
            rules.Set(1, "item:2", .25);
            rules.Set(1, "item:3", .2);
            Assert.That(rules.Multiplier(1), Is.EqualTo(.8));
            rules.Remove(1, "item:3");
            Assert.That(rules.Multiplier(1), Is.EqualTo(.75));
            rules.Remove(1, "item:2");
            Assert.That(rules.Multiplier(1), Is.EqualTo(.6));
        }
        [Test] public void IdempotentUpkeepDoesNotReorderAndRejectedChangesAreAtomic()
        {
            var rules = new OriginalSpellResistanceLedger();
            rules.Set(1, "A0E5", .4); rules.Set(1, "item:2", .2);
            rules.Set(1, "A0E5", .4);
            Assert.That(rules.Multiplier(1), Is.EqualTo(.8));
            Assert.Throws<InvalidOperationException>(() => rules.Set(1, "A0E5", .5));
            Assert.Throws<ArgumentException>(() => rules.Set(1, "bad", double.NaN));
            Assert.Throws<ArgumentException>(() => rules.Set(0, "bad", .2));
            Assert.That(rules.Multiplier(1), Is.EqualTo(.8));
        }
        [Test] public void CopyKeepsOrderButCandidateChangesDoNotAffectPublishedLedger()
        {
            var rules = new OriginalSpellResistanceLedger();
            rules.Set(1, "item:1", .25); rules.Set(1, "item:2", .2); rules.Set(2, "A0LH", 1);
            var candidate = rules.Copy(); candidate.Remove(1, "item:2"); candidate.Set(1, "item:3", .4);
            candidate.Remove(2, "A0LH");
            Assert.That(rules.Multiplier(1), Is.EqualTo(.8));
            Assert.That(rules.Multiplier(2), Is.Zero);
            Assert.That(candidate.Multiplier(1), Is.EqualTo(.6));
            Assert.That(candidate.Multiplier(2), Is.EqualTo(1));
            Assert.That(rules.Multiplier(3), Is.EqualTo(1));
        }
        [Test] public void IndependentRecipientCandidatesCommitOnlyTheirOwnActor()
        {
            var published = new OriginalSpellResistanceLedger();
            var first = published.Copy(); var second = published.Copy();
            first.Set(1, "item:1", .2); second.Set(2, "item:2", .25);
            published.ReplaceActorFrom(1, first); published.ReplaceActorFrom(2, second);
            second.Remove(2, "item:2");
            Assert.That(published.Multiplier(1), Is.EqualTo(.8));
            Assert.That(published.Multiplier(2), Is.EqualTo(.75));
        }
    }
}

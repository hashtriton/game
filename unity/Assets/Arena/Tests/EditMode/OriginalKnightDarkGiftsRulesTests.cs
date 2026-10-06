using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalKnightDarkGiftsRulesTests
    {
        static OriginalDarkGiftCandidate Candidate(int id = 1, int owner = 1, string code = "H008", double x = 0, bool buff = false, bool resist = false) =>
            new OriginalDarkGiftCandidate(id, owner, code, new OriginalPoint(x, 1000), buff, resist);
        [Test] public void AppliesForThirtyThirtyFiveFortyTicksAndCleansOnTheFollowingTick()
        {
            for (int rank = 1; rank <= 3; rank++)
            {
                var rules = new OriginalKnightDarkGiftsRules(1, rank);
                for (int i = 0; i < 25 + 5 * rank; i++)
                {
                    Assert.That(rules.Tick(new[] { Candidate() }).Select(e => e.kind), Is.EqualTo(new[] {
                        OriginalDarkGiftInstructionKind.CastAcidBomb, OriginalDarkGiftInstructionKind.AddResistance }));
                    Assert.That(rules.Completed, Is.False);
                }
                Assert.That(rules.Tick(new[] { Candidate(buff: true, resist: true) }).Select(e => e.kind), Is.EqualTo(new[] {
                    OriginalDarkGiftInstructionKind.RemoveResistance, OriginalDarkGiftInstructionKind.RemoveAcidBuff }));
                Assert.That(rules.Completed, Is.True); Assert.That(rules.Tick(new[] { Candidate() }), Is.Empty);
            }
        }
        [Test] public void SelectsSameOwnerRawcodeAndPlayableBoundsWithoutInventingHeroKindOrLifeFilter()
        {
            var rules = new OriginalKnightDarkGiftsRules(1, 1);
            var rows = new[] { Candidate(), Candidate(1000000000), Candidate(2, owner: 2), Candidate(1001, owner: 0),
                Candidate(3, code: "H024"), Candidate(4, x: 2432), Candidate(5, x: 2432.01), Candidate(6, x: -2560), Candidate(7, x: -2560.01) };
            Assert.That(rules.Tick(rows).Select(e => e.entityId), Is.EqualTo(new[] { 1,1,1000000000,1000000000,4,4,6,6 }));
        }
        [Test] public void ExistingBuffAndResistanceHaveIndependentChecksAndInvalidInputsDoNotConsumeTime()
        {
            var rules = new OriginalKnightDarkGiftsRules(1, 1);
            Assert.Throws<ArgumentException>(() => rules.Tick(new[] { Candidate(x: double.NaN) }));
            Assert.Throws<ArgumentException>(() => rules.Tick(new[] { Candidate(), Candidate() }));
            Assert.That(rules.Tick(new[] { Candidate(buff: true), Candidate(2, resist: true) }).Select(e => e.kind), Is.EqualTo(new[] {
                OriginalDarkGiftInstructionKind.AddResistance, OriginalDarkGiftInstructionKind.CastAcidBomb }));
            for (int i = 1; i < 30; i++) rules.Tick(Array.Empty<OriginalDarkGiftCandidate>());
            Assert.That(rules.Completed, Is.False); rules.Tick(Array.Empty<OriginalDarkGiftCandidate>());
            Assert.That(rules.Completed, Is.True);
        }
    }
}

using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalHudRelationsTests
    {
        static OriginalSessionView DuelView(OriginalDuelKind kind, OriginalDuelPhase phase)
        {
            return new OriginalSessionView
            {
                started = true, hasDuel = true, phase = OriginalMatchPhase.Duel,
                players = new[]
                {
                    new OriginalSessionPlayerView { slot = 3, matchSlot = 1, connected = true },
                    new OriginalSessionPlayerView { slot = 7, matchSlot = 2, connected = true },
                    new OriginalSessionPlayerView { slot = 8, matchSlot = 3, connected = true }
                },
                duel = new OriginalDuelSnapshot { kind = kind, phase = phase, firstSlot = 1, secondSlot = 2 }
            };
        }

        [TestCase(3, 0, true)]
        [TestCase(0, 3, true)]
        [TestCase(0, 0, false)]
        [TestCase(3, 3, false)]
        [TestCase(7, 7, false)]
        public void OwnUnitsStayAlliedAndSourceEnemiesStayHostile(int localOwner, int targetOwner, bool expected)
        {
            Assert.That(OriginalHudRelations.IsEnemy(null, localOwner, targetOwner), Is.EqualTo(expected));
        }

        [TestCase(OriginalDuelPhase.Countdown, true)]
        [TestCase(OriginalDuelPhase.Combat, true)]
        [TestCase(OriginalDuelPhase.Resolving, false)]
        [TestCase(OriginalDuelPhase.Completed, false)]
        [TestCase(OriginalDuelPhase.Unresolved, false)]
        public void PairUsesMatchSlotsAndLeavesSpectatorsAllied(OriginalDuelPhase phase, bool expected)
        {
            var view = DuelView(OriginalDuelKind.Pairs, phase);
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 7), Is.EqualTo(expected));
            Assert.That(OriginalHudRelations.IsEnemy(view, 7, 3), Is.EqualTo(expected));
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 8), Is.False);
            Assert.That(OriginalHudRelations.IsEnemy(view, 8, 7), Is.False);
        }

        [TestCase(OriginalDuelPhase.Countdown, true)]
        [TestCase(OriginalDuelPhase.Combat, true)]
        [TestCase(OriginalDuelPhase.Resolving, false)]
        [TestCase(OriginalDuelPhase.Completed, false)]
        [TestCase(OriginalDuelPhase.NotStarted, false)]
        public void GladiatorMakesEveryDifferentParticipantHostileUntilResolution(OriginalDuelPhase phase, bool expected)
        {
            var view = DuelView(OriginalDuelKind.Gladiator, phase);
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 7), Is.EqualTo(expected));
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 8), Is.EqualTo(expected));
            Assert.That(OriginalHudRelations.IsEnemy(view, 8, 7), Is.EqualTo(expected));
            Assert.That(OriginalHudRelations.IsEnemy(view, 8, 8), Is.False);
        }

        [TestCase(OriginalWorldUnitKind.Summon)]
        [TestCase(OriginalWorldUnitKind.Illusion)]
        public void OwnedHelpersUseTheirOwnersDuelSide(OriginalWorldUnitKind kind)
        {
            var view = DuelView(OriginalDuelKind.Pairs, OriginalDuelPhase.Combat);
            var opponent = new OriginalWorldUnitView { kind = kind, ownerSlot = 7 };
            var spectator = new OriginalWorldUnitView { kind = kind, ownerSlot = 8 };
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, opponent.ownerSlot), Is.True);
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, spectator.ownerSlot), Is.False);
            Assert.That(OriginalHudRelations.IsEnemy(view, 7, opponent.ownerSlot), Is.False);
        }

        [Test] public void DisconnectedParticipantRetainsTheSameDuelSide()
        {
            var view = DuelView(OriginalDuelKind.Pairs, OriginalDuelPhase.Countdown);
            view.players[1].connected = false;
            view.players[1].alive = false;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 7), Is.True);
            Assert.That(OriginalHudRelations.IsEnemy(view, 7, 3), Is.True);
        }

        [TestCase(OriginalDuelKind.Gladiator, "gladiator-killer-owner-unknown", true)]
        [TestCase(OriginalDuelKind.Gladiator, "other-unresolved-rule", false)]
        [TestCase(OriginalDuelKind.Pairs, "gladiator-killer-owner-unknown", false)]
        [TestCase(OriginalDuelKind.Pairs, "pair-participant-missing", false)]
        [TestCase(OriginalDuelKind.Pairs, "bet-zero-divisor", false)]
        public void OnlyTheUnresolvedGladiatorKillerBranchRetainsHostility(OriginalDuelKind kind, string reason, bool expected)
        {
            var view = DuelView(kind, OriginalDuelPhase.Unresolved);
            view.duel.unresolved = reason;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 7), Is.EqualTo(expected));
        }

        [Test] public void MissingDuelAndUnknownOwnersDoNotInventPlayerHostility()
        {
            var view = DuelView(OriginalDuelKind.Gladiator, OriginalDuelPhase.Combat);
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 6), Is.False);
            Assert.That(OriginalHudRelations.IsEnemy(view, 6, 7), Is.False);
            view.players[1].matchSlot = 0;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 7), Is.False);
            view.hasDuel = false;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 8), Is.False);
            view.hasDuel = true; view.duel = null;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 8), Is.False);
            view.duel = new OriginalDuelSnapshot(); view.players = null;
            Assert.That(OriginalHudRelations.IsEnemy(view, 3, 8), Is.False);
        }
    }
}

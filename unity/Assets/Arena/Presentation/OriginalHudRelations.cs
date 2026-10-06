using System;
using Arena.Original;

namespace Arena
{
    public static class OriginalHudRelations
    {
        // World owners are persistent lobby slots. Duel slots are contiguous
        // match slots, including heroes retained under AI after disconnecting.
        public static bool IsEnemy(OriginalSessionView view, int localOwner, int targetOwner)
        {
            if (localOwner == targetOwner) return false;
            if (localOwner == 0 || targetOwner == 0) return true;
            var duel = view?.duel;
            if (view?.hasDuel != true || duel == null || view.players == null) return false;
            var local = Array.Find(view.players, player => player != null && player.slot == localOwner);
            var target = Array.Find(view.players, player => player != null && player.slot == targetOwner);
            if (local == null || target == null || local.matchSlot <= 0 || target.matchSlot <= 0) return false;

            // OriginalDuel.PreparePair/PrepareGladiator emit Alliance(false)
            // during Countdown; ResolvePair/FinishSeries restore it before
            // publishing Resolving. HeroDied's unknown gladiator killer branch
            // halts before FinishSeries and therefore retains hostile alliances.
            bool active = duel.phase == OriginalDuelPhase.Countdown || duel.phase == OriginalDuelPhase.Combat ||
                duel.kind == OriginalDuelKind.Gladiator && duel.phase == OriginalDuelPhase.Unresolved &&
                duel.unresolved == "gladiator-killer-owner-unknown";
            if (!active) return false;
            return duel.kind == OriginalDuelKind.Gladiator || duel.kind == OriginalDuelKind.Pairs &&
                (local.matchSlot == duel.firstSlot && target.matchSlot == duel.secondSlot ||
                 local.matchSlot == duel.secondSlot && target.matchSlot == duel.firstSlot);
        }
    }
}

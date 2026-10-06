using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Flags]
    public enum OriginalActorControlMask { None = 0, Move = 1, Weapon = 2, Cast = 4, Item = 8 }

    public sealed partial class OriginalSession
    {
        sealed class ActorControl
        {
            internal OriginalActorControlMask mask;
            internal double remaining;
            internal bool timed, negative, freezeWhilePaused;
        }
        readonly Dictionary<int, Dictionary<string, ActorControl>> actorControls = new Dictionary<int, Dictionary<string, ActorControl>>();
        const OriginalActorControlMask AllActorControls = OriginalActorControlMask.Move | OriginalActorControlMask.Weapon | OriginalActorControlMask.Cast | OriginalActorControlMask.Item;

        // Source tokens are independent. A duration of zero is explicitly
        // untimed and needs ClearActorControl; it never means an expired buff.
        // The caller selects native targeting and pause/dispel policy. This
        // storage does not infer them from a buff ID or from a movement speed.
        bool SetActorControl(int actor, string token, OriginalActorControlMask mask, double seconds, bool negative, bool freezeWhilePaused)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length > 128) throw new ArgumentException("Invalid actor control token.", nameof(token));
            if (mask == OriginalActorControlMask.None || (mask & ~AllActorControls) != 0) throw new ArgumentOutOfRangeException(nameof(mask));
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            var unit = world?.UnitState(actor);
            if (unit == null || unit.health <= 0) return false;
            var previous = ActorControlMask(actor);
            if (!actorControls.TryGetValue(actor, out var sources))
            {
                sources = new Dictionary<string, ActorControl>(StringComparer.Ordinal);
                actorControls.Add(actor, sources);
            }
            sources[token] = new ActorControl { mask = mask, remaining = seconds, timed = seconds > 0,
                negative = negative, freezeWhilePaused = freezeWhilePaused };
            var added = ActorControlMask(actor) & ~previous;
            if (added != OriginalActorControlMask.None) OnActorControlsAdded(actor, added);
            return true;
        }

        partial void OnActorControlsAdded(int actor, OriginalActorControlMask newlyBlocked);

        OriginalActorControlMask ActorControlMask(int actor)
        {
            var unit = world?.UnitState(actor);
            if (unit == null || unit.health <= 0 || !actorControls.TryGetValue(actor, out var sources)) return OriginalActorControlMask.None;
            var mask = OriginalActorControlMask.None;
            foreach (var control in sources.Values) mask |= control.mask;
            return mask;
        }
        bool ActorMoveBlocked(int actor) => (ActorControlMask(actor) & OriginalActorControlMask.Move) != 0;
        bool ActorWeaponBlocked(int actor) => (ActorControlMask(actor) & OriginalActorControlMask.Weapon) != 0;
        bool ActorCastBlocked(int actor) => (ActorControlMask(actor) & OriginalActorControlMask.Cast) != 0;

        // CONTROL2 ea850ff36637: exact A0YI(DataA1=15)/BNsi suppresses
        // both weapon starts and A0Z3 spell events for7s; movement continues.
        // Pause policy for silence is reconstructed; only BPSE was paused.
        bool AddNativeSilence(int actor, int source, double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            return SetActorControl(actor, "native-silence:A0YI:" + source,
                OriginalActorControlMask.Weapon | OriginalActorControlMask.Cast, seconds, true, true);
        }
        bool HasNativeSilence(int actor)
        {
            if (!ActorCastBlocked(actor) || !actorControls.TryGetValue(actor, out var sources)) return false;
            foreach (var token in sources.Keys) if (token.StartsWith("native-silence:A0YI:", StringComparison.Ordinal)) return true;
            return false;
        }

        // ABUN1 cache ebcaf63562ba: Abun suppresses weapon starts, while
        // movement and A0Z3 casting continue. It is an added ability rather
        // than a negative buff, so generic negative-buff dispel preserves it.
        bool SetNativeAbun(int actor, string sourceToken, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(sourceToken) || sourceToken.Length > 112)
                throw new ArgumentException("Invalid Abun source token.", nameof(sourceToken));
            string token = "native-abun:" + sourceToken;
            if (enabled) return SetActorControl(actor, token, OriginalActorControlMask.Weapon, 0, false, false);
            ClearActorControl(actor, token); return true;
        }

        // The same experiment measured B08D active throughout blocked motion
        // and successful native casting. Pause freezing remains a host policy
        // for this root, since ABUN1 did not pause an already-rooted target.
        bool AddTimedNativeRoot(int actor, int source, double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            return SetActorControl(actor, "native-root:A0KV:" + source, OriginalActorControlMask.Move, seconds, true, true);
        }
        bool IsNativeRooted(int actor)
        {
            if (!ActorMoveBlocked(actor) || !actorControls.TryGetValue(actor, out var sources)) return false;
            foreach (var token in sources.Keys) if (token.StartsWith("native-root:A0KV:", StringComparison.Ordinal) ||
                token.StartsWith("native-root:A15P:", StringComparison.Ordinal) || token.StartsWith("native-root:A0TL:", StringComparison.Ordinal) || token.StartsWith("native-root:A18L:", StringComparison.Ordinal)) return true;
            return false;
        }
        void ClearNativeRoot(int actor)
        {
            if (!actorControls.TryGetValue(actor, out var sources)) return;
            // Authored UnitRemoveAbility(B08D) clears this native buff, even
            // when more than one root source had targeted the same unit.
            foreach (var token in new List<string>(sources.Keys))
                if (token.StartsWith("native-root:A0KV:", StringComparison.Ordinal)) sources.Remove(token);
            if (sources.Count == 0) actorControls.Remove(actor);
        }

        void ClearActorControl(int actor, string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (!actorControls.TryGetValue(actor, out var sources)) return;
            sources.Remove(token);
            if (sources.Count == 0) actorControls.Remove(actor);
        }
        void ClearNegativeActorControls(int actor)
        {
            itemTaunts.Remove(actor);
            RemoveOrdinaryNegativeBuffs(actor);
            RemoveItemScriptDebuffs(actor);
            RemoveBossBanish(actor);
            RemoveBossBindingDebuff(actor);
            RemoveBossDoom(actor);
            EndNativeSleep(actor, false);
            if (!actorControls.TryGetValue(actor, out var sources)) return;
            foreach (var token in new List<string>(sources.Keys)) if (sources[token].negative) sources.Remove(token);
            if (sources.Count == 0) actorControls.Remove(actor);
        }
        void ForgetActorControls(int actor)
        {
            CancelQueuedCastApproach(actor);
            pendingItemOrders.Remove(actor);
            nativeSleeps.Remove(actor);
            actorControls.Remove(actor);
        }

        // Called once after World advances, before ability/AI/weapon execution.
        // Movement expiry is quantized to the existing <=.05 s host tick. We
        // never restore an old order, alter pause, or rewrite native speed.
        void AdvanceActorControls(double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            foreach (var actor in new List<int>(actorControls.Keys))
            {
                var unit = world?.UnitState(actor);
                if (unit == null || unit.health <= 0) { actorControls.Remove(actor); continue; }
                var sources = actorControls[actor];
                foreach (var token in new List<string>(sources.Keys))
                {
                    var control = sources[token];
                    if (!control.timed || control.freezeWhilePaused && unit.paused) continue;
                    control.remaining -= seconds;
                    if (control.remaining <= 1e-9) sources.Remove(token);
                }
                if (sources.Count == 0) actorControls.Remove(actor);
            }
            AdvanceNativeSleeps(seconds);
        }

        // Inferno's caller resolves eligibility and supplies duration. CONTROL2
        // measures BPSE expiry shifted by the entire2.5s pause, both visible
        // and hidden. This freezes the buff timer without imposing PauseUnit.
        bool AddTimedNativeStun(int target, string buffId, int source, double seconds)
        {
            if (buffId == null || buffId.Length != 4) throw new ArgumentException("Invalid native stun buff.", nameof(buffId));
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            return SetActorControl(target, "native-stun:" + buffId + ":" + source, AllActorControls, seconds, true, true);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossDoom { internal int target, owner; internal double remaining = 3, pulse = .01, updatedAt; internal bool wasPaused; }
        readonly Dictionary<int, BossDoom> bossDooms = new Dictionary<int, BossDoom>();

        void QueueBossDoom(int owner, int targetId)
        {
            var target = world.UnitState(targetId);
            if (target == null || target.health <= .405 || target.hidden || CasterMagicImmune(target) ||
                CasterHasType(target, "mechanical") || CasterHasType(target, "structure")) return;
            // BSPAR2: A0HR on h011 is synchronous. One application zero event
            // precedes B0BN, then zero pulses occur +.01,+1.01,+2.01. Native
            // targets explicitly include vulnerable and invulnerable heroes.
            ApplyResolvedUnitHit(0, owner, target, 0);
            target = world.UnitState(targetId); if (target == null || target.health <= .405) return;
            bossDooms[targetId] = new BossDoom { target = targetId, owner = owner, updatedAt = world.Clock };
            SetActorControl(targetId, "native-doom:B0BN", OriginalActorControlMask.Cast | OriginalActorControlMask.Item, 0, true, false);
        }
        void RemoveBossDoom(int target)
        { bossDooms.Remove(target); ClearActorControl(target, "native-doom:B0BN"); }
        void AdvanceBossDooms()
        {
            foreach (var state in new List<BossDoom>(bossDooms.Values))
            {
                var target = world.UnitState(state.target);
                if (target == null || target.health <= .405)
                { RemoveBossDoom(state.target); continue; }
                double seconds = Math.Max(0, world.Clock - state.updatedAt); state.updatedAt = world.Clock;
                if (target.paused) { state.wasPaused = true; continue; }
                // DOOM1: pause freezes duration, but unpause restarts the
                // periodic1s interval instead of retaining its prior .01s gap.
                if (state.wasPaused) { state.wasPaused = false; state.pulse = 1; }
                state.remaining -= seconds; state.pulse -= seconds;
                while (state.pulse <= 1e-9 && state.remaining - state.pulse >= -1e-9)
                {
                    ApplyResolvedUnitHit(0, state.owner, world.UnitState(state.target), 0); state.pulse += 1;
                }
                if (state.remaining > 1e-9) continue;
                RemoveBossDoom(state.target);
            }
        }
    }
}

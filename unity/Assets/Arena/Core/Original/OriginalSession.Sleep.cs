using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class NativeSleep
        {
            internal int owner,source;
            internal double remaining = 8, protectedRemaining = 2;
        }
        const string NativeSleepToken = "native-sleep:A124";
        readonly Dictionary<int, NativeSleep> nativeSleeps = new Dictionary<int, NativeSleep>();

        void ValidateNativeSleep()
        {
            var ability = combatCatalog.Ability("A124");
            if (ability == null || ability.Text("code") != "AUsl" || ability.Number("levels") != 1 ||
                ability.Text("BuffID1") != "BUsl,BUsp,Bust" || ability.Number("DataA1") != 2 ||
                ability.Number("Dur1") != 8 || ability.Number("HeroDur1") != 8 ||
                ability.Text("targs1") != "air,ground,enemy,organic,neutral" || ability.overrides.Length != 0)
                throw new InvalidOperationException("native-sleep-declaration-conflict");
        }

        bool AddNativeSleep(int target, int sourceOwner)
        {
            ValidateNativeSleep();
            var actor = world.UnitState(target);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.invulnerable ||
                !AreEnemies(sourceOwner, actor.ownerSlot) || CasterMagicImmune(actor) ||
                CasterHasType(actor, "mechanical") || CasterHasType(actor, "structure")) return false;
            // CONTROL2: helper application emits one zero event before BUsl.
            // Actor0 retains helper attribution instead of borrowing the hero.
            ApplyResolvedUnitHit(0, sourceOwner, actor, 0);
            actor = world.UnitState(target);
            if (actor == null || actor.health <= .405) return false;
            // One native BUsl is represented by one state. Latest refresh is
            // host reconstruction; repeated application was not isolated.
            nativeSleeps[target] = new NativeSleep { owner = sourceOwner };
            return SetActorControl(target, NativeSleepToken, AllActorControls, 0, true, true);
        }

        bool HasNativeSleep(int target) => nativeSleeps.ContainsKey(target) && ActorMoveBlocked(target);

        double ResolveNativeSleepDamage(int target, double damage)
        {
            // CONTROL2 measures early CHAOS/NORMAL1 as an admitted zero event,
            // not a rejected call. Extending BUsp protection to other resolved
            // damage axes is a host reconstruction, not additional probe data.
            return nativeSleeps.TryGetValue(target, out var sleep) && sleep.protectedRemaining > 1e-9 ? 0 : damage;
        }

        void NotifyNativeSleepDamage(int target, double damage)
        {
            if (damage > 0 && nativeSleeps.TryGetValue(target, out var sleep) && sleep.protectedRemaining <= 1e-9)
                EndNativeSleep(target, true);
        }

        void EndNativeSleep(int target, bool emitNativeExpiry)
        {
            if (!nativeSleeps.TryGetValue(target, out var sleep)) return;
            nativeSleeps.Remove(target); ClearActorControl(target, NativeSleepToken);
            var actor = world.UnitState(target);
            // Clear first: a positive wake hit is followed by a separate zero
            // helper callback. Dispel/death callbacks were not measured.
            if (emitNativeExpiry && actor != null && actor.health > .405)
                ApplyResolvedUnitHit(sleep.source, sleep.owner, actor, 0);
        }

        void AdvanceNativeSleeps(double seconds)
        {
            foreach (var target in new List<int>(nativeSleeps.Keys))
            {
                var actor = world.UnitState(target);
                if (actor == null || actor.health <= .405) { EndNativeSleep(target, false); continue; }
                // Pause freezing is explicit host policy for AUsl. CONTROL2
                // isolated native pause duration only for BPSE stun.
                if (actor.paused) continue;
                var sleep = nativeSleeps[target];
                sleep.remaining -= seconds; sleep.protectedRemaining -= seconds;
                if (sleep.remaining <= 1e-9) EndNativeSleep(target, true);
            }
        }
    }
}

using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        void BeginCasterSleepWave(CasterEffect effect)
        {
            effect.sleepWave = new OriginalCasterSleepWaveRules(effect.origin, effect.center);
            effect.center = effect.origin;
            effect.captured = new HashSet<int>();
            effect.nextAt += OriginalCasterSleepWaveRules.Interval;
        }

        void TickCasterSleepWave(CasterEffect effect)
        {
            effect.sleepWave.Advance();
            effect.center = effect.sleepWave.Center;
            foreach (var target in CasterUnits())
                if (CasterEligible(effect, target, OriginalCasterSleepWaveRules.Radius, false, true, true) &&
                    effect.captured.Add(target.entityId))
                {
                    // Omv: damage first, then the retained helper orders sleep.
                    // HELPER1 accepted all3 same-callback targets. CONTROL2
                    // closes AUsl status axes; native subframe delay is not
                    // replayed by this host's immediate helper application.
                    CasterScriptDamage(effect, target, OriginalCasterSleepWaveRules.Damage, 2);
                    AddNativeSleep(target.entityId, effect.owner);
                }
            if (effect.sleepWave.Completed)
            {
                // Omv kills its projectile, then removes the sleep helper and
                // finally reopens MC. Existing sleeps survive helper removal.
                ObserveScriptedHelperDeath(); CompleteCasterEffect(effect);
            }
            else effect.nextAt += OriginalCasterSleepWaveRules.Interval;
        }
    }
}

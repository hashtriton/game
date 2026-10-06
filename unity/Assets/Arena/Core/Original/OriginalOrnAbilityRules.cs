using System;

namespace Arena.Original
{
    public static class OriginalOrnAbilityRules
    {
        public const double DecoyChoiceDelay = .65, DecoyBurstDelay = 1.2, DecoyRadius = 170;
        // Eev30930: the chosen copy doubles both the health fraction and flat part.
        public static double DecoyDamage(double maximumHealth, bool chosen)
        {
            if (!OriginalCombatDefinition.IsFinite(maximumHealth) || maximumHealth < 0)
                throw new ArgumentOutOfRangeException(nameof(maximumHealth));
            return maximumHealth * (chosen ? .1 : .05) + (chosen ? 500 : 250);
        }
    }

    // V7v/V5v30853-30920: retained target, fixed bearing, literal angular
    // constants, strict yK<600 and eL. Native cast/buff/flight height are separate.
    public sealed class OriginalOrnKickRules
    {
        public const double Period = .04;
        readonly double bearing;
        int distance;
        public bool Completed { get; private set; }
        public OriginalOrnKickRules(OriginalPoint caster, OriginalPoint target)
        {
            if (!Finite(caster) || !Finite(target)) throw new ArgumentOutOfRangeException();
            bearing = 57.2958 * Math.Atan2(target.y - caster.y, target.x - caster.x);
            if (target.x * target.x + (target.y + 2700) * (target.y + 2700) > 450 * 450) bearing += 180;
        }
        public bool Tick(OriginalPoint current, bool battleActive, out OriginalPoint next)
        {
            next = current;
            if (Completed) return false;
            distance += 20;
            var candidate = new OriginalPoint(current.x + 20 * Math.Cos(bearing * .0174532),
                current.y + 20 * Math.Sin(bearing * .0174532));
            if (!battleActive || distance >= 600 || !OriginalShieldBashRules.AllowsForcedPoint(candidate))
            { Completed = true; return false; }
            next = candidate; return true;
        }
        static bool Finite(OriginalPoint point) => OriginalCombatDefinition.IsFinite(point.x) && OriginalCombatDefinition.IsFinite(point.y);
    }

    // EIv/EOv/EEv/EXv31086-31226. Saved JASS real counters use float, preserving
    // their literal repeated arithmetic. Callback counts are a reconstruction
    // of this source, not a separately observed original-trigger native trace.
    public sealed class OriginalOrnSpinRules
    {
        public const double WarningPeriod = .04, DamagePeriod = .4, ChasePeriod = .04;
        public const double Radius = 300, Damage = 180;
        float warning = 1.5f, damageRemaining = 4, chaseElapsed;
        public bool Started { get; private set; }
        public bool DamageCompleted { get; private set; }
        public bool ChaseCompleted { get; private set; }
        public bool TickWarning()
        {
            if (Started) return true;
            if (warning > 0) { warning -= .04f; return false; }
            Started = true; return true;
        }
        public bool TickDamage(bool alive)
        {
            if (DamageCompleted) return false;
            if (!alive || damageRemaining <= 0) { DamageCompleted = true; return false; }
            damageRemaining -= .4f; return true;
        }
        public OriginalPoint TickChase(OriginalPoint current, OriginalPoint retainedTarget)
        {
            if (ChaseCompleted) return current;
            double dx = retainedTarget.x - current.x, dy = retainedTarget.y - current.y;
            double angle = 57.2958 * Math.Atan2(dy, dx) * .0174532;
            var next = dx * dx + dy * dy >= 100 * 100 ? new OriginalPoint(
                current.x + 11.2 * Math.Cos(angle), current.y + 11.2 * Math.Sin(angle)) : current;
            chaseElapsed += .04f;
            if (chaseElapsed >= 4) ChaseCompleted = true;
            return next;
        }
    }
}

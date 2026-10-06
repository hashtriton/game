using System;

namespace Arena.Original
{
    // LiA3.9c Omv/OMv/Opv34429..34549: source-only geometry. Native A124
    // denial, countdown and waking live in the separate Sleep integration.
    public sealed class OriginalCasterSleepWaveRules
    {
        public const double Interval = .03, Radius = 450, Damage = 1500;
        public OriginalPoint Center { get; private set; }
        public double RemainingDistance { get; private set; } = 950;
        public int Ticks { get; private set; }
        public bool Completed { get; private set; }
        readonly double dx, dy;

        public OriginalCasterSleepWaveRules(OriginalPoint casterAtActivation, OriginalPoint warningCenter)
        {
            Validate(casterAtActivation); Validate(warningCenter);
            double angle = Math.Atan2(warningCenter.y - casterAtActivation.y, warningCenter.x - casterAtActivation.x);
            dx = 27 * Math.Cos(angle); dy = 27 * Math.Sin(angle);
            Center = casterAtActivation;
        }

        // Sweep at Center after Advance, including the completing callback.
        // Source tests the OLD remaining distance after movement and damage.
        public void Advance()
        {
            if (Completed) throw new InvalidOperationException("sleep-wave-already-complete");
            var next = new OriginalPoint(Center.x + dx, Center.y + dy);
            Validate(next);
            Completed = RemainingDistance <= 0;
            RemainingDistance -= 27;
            Center = next; Ticks++;
        }

        static void Validate(OriginalPoint point)
        {
            if (!OriginalCombatDefinition.IsFinite(point.x) || !OriginalCombatDefinition.IsFinite(point.y))
                throw new ArgumentOutOfRangeException(nameof(point));
        }
    }
}

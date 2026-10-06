using System;

namespace Arena.Original
{
    // Own reconstruction of hs/Gs in 3.9c JASS7767..7890. Geometry helpers
    // share the authored eL exclusions and iL/nL clamping with shield bash.
    public sealed class OriginalDuelRingPush
    {
        public const double Period = .03;
        readonly double dx, dy;
        int remaining;
        public bool Completed { get; private set; }
        public int Remaining => remaining;

        public OriginalDuelRingPush(OriginalPoint start, OriginalPoint center)
        {
            double x = start.x - center.x, y = start.y - center.y;
            double distance = Math.Sqrt(x * x + y * y);
            if (!OriginalCombatDefinition.IsFinite(distance) || distance > 1620) throw new ArgumentOutOfRangeException(nameof(start));
            double angle = Math.Atan2(y, x);
            dx = 10 * Math.Cos(angle); dy = 10 * Math.Sin(angle);
            remaining = (int)(distance / 10);
        }

        public bool Tick(OriginalPoint current, out OriginalPoint next)
        {
            next = current;
            if (Completed) return false;
            if (remaining <= 0) { Completed = true; return false; }
            remaining--;
            var requested = new OriginalPoint(current.x - dx, current.y - dy);
            if (!OriginalShieldBashRules.AllowsForcedPoint(requested)) { Completed = true; return false; }
            next = OriginalShieldBashRules.ClampPlayable(requested);
            return true;
        }

        public static bool Outside(OriginalPoint position, OriginalPoint center, double radius)
        {
            double x = position.x - center.x, y = position.y - center.y;
            double square = x * x + y * y;
            return square <= 1620 * 1620 && square > radius * radius;
        }
    }
}

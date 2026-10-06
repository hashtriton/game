using System;

namespace Arena.Original
{
    // Declarative reconstruction of SPv/SKv 64083..64141. The last timer
    // callback moves first and tests evalCount>15 afterward: sixteen steps,
    // not fifteen. Native trigger order and shared global Mf belong to host.
    public sealed class OriginalPyroVacuumPull
    {
        readonly double dx, dy;
        int ticks;
        public bool Completed { get; private set; }
        public OriginalPyroVacuumPull(OriginalPoint center, OriginalPoint initialTarget)
        {
            OriginalPyroEffectRules.Point(center); OriginalPyroEffectRules.Point(initialTarget);
            dx = (center.x - initialTarget.x) / 15; dy = (center.y - initialTarget.y) / 15;
        }
        public bool Tick(OriginalPoint current, bool dead, out OriginalPoint position, out bool dealDamage)
        {
            OriginalPyroEffectRules.Point(current); position = current; dealDamage = false;
            if (Completed) return false;
            if (dead) { Completed = true; return false; }
            position = new OriginalPoint(current.x + dx, current.y + dy);
            ticks++;
            if (ticks > 15) { dealDamage = true; Completed = true; }
            return true;
        }
    }

    // Suv/SSv 64231..64288: ten timer moves from a retained cursor, using
    // Max(275-distance,10)/10. An excluded step leaves that cursor unchanged.
    public sealed class OriginalPyroSpherePush
    {
        readonly double dx, dy;
        OriginalPoint cursor;
        int ticks;
        public bool Completed { get; private set; }
        public OriginalPyroSpherePush(OriginalPoint center, OriginalPoint initialTarget)
        {
            OriginalPyroEffectRules.Point(center); OriginalPyroEffectRules.Point(initialTarget); cursor = initialTarget;
            double x = initialTarget.x - center.x, y = initialTarget.y - center.y;
            double angle = Math.Atan2(y, x), step = Math.Max(275 - Math.Sqrt(x * x + y * y), 10) / 10;
            dx = step * Math.Cos(angle); dy = step * Math.Sin(angle);
        }
        public bool Tick(bool dead, out OriginalPoint position)
        {
            position = cursor;
            if (Completed) return false;
            ticks++;
            if (dead || ticks > 10) { Completed = true; return false; }
            var next = new OriginalPoint(cursor.x + dx, cursor.y + dy);
            if (OriginalShieldBashRules.AllowsForcedPoint(next)) cursor = next;
            position = cursor; return true;
        }
    }

    // tvv64700..64719 creates six native AHfs orders before each selected move.
    // Its native area/DoT is a separate measured dependency, never fake direct
    // damage attached to these positions.
    public sealed class OriginalPyroMeteorTrail
    {
        readonly double dx, dy;
        int ticks;
        public OriginalPoint Position { get; private set; }
        public bool Completed { get; private set; }
        public OriginalPyroMeteorTrail(OriginalPoint impact, double directionRadians)
        {
            OriginalPyroEffectRules.Point(impact);
            if (!OriginalCombatDefinition.IsFinite(directionRadians)) throw new ArgumentOutOfRangeException();
            dx = 10 * Math.Cos(directionRadians); dy = 10 * Math.Sin(directionRadians);
            Position = new OriginalPoint(impact.x + 1.5 * dx, impact.y + 1.5 * dy);
        }
        public bool Tick(out OriginalPoint flameStrikePoint)
        {
            flameStrikePoint = Position;
            if (Completed) return false;
            ticks++;
            if (ticks > 100) { Completed = true; return false; }
            bool cast = ticks == 1 || ticks == 20 || ticks == 40 || ticks == 60 || ticks == 80 || ticks == 100;
            Position = new OriginalPoint(Position.x + dx, Position.y + dy); return cast;
        }
    }

    public static class OriginalPyroEffectRules
    {
        public const double VacuumPullTick = .025, SpherePushTick = .03, PortalTick = .03, MeteorTrailTick = .04;
        public const double MeteorDelay = 1.3;

        // S7v64621..64658: no move at distances<160; only units in175 radius
        // enter the source enumeration. Caller applies enemy/life/type filters.
        public static bool PortalPull(OriginalPoint center, OriginalPoint target, out OriginalPoint result)
        {
            Point(center); Point(target); result = target;
            double x = target.x - center.x, y = target.y - center.y, distance = Math.Sqrt(x * x + y * y);
            if (distance < 160 || distance > 175) return false;
            result = new OriginalPoint(target.x - 15 * x / distance, target.y - 15 * y / distance); return true;
        }
        internal static void Point(OriginalPoint value)
        { if (!OriginalCombatDefinition.IsFinite(value.x) || !OriginalCombatDefinition.IsFinite(value.y)) throw new ArgumentOutOfRangeException(); }
    }
}

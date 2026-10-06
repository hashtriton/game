using System;

namespace Arena.Original
{
    // LiA3.9c bZ14604, VA86410, VFv30306, Vfv30274, VDv30227.
    // Reconstructed rules only. Portal positions use the source trigonometric
    // constant; moving ghosts use Deg2Rad and the source eL point filter.
    public sealed class OriginalOrnGhostRules
    {
        public const double PortalPeriod = 15, WarningSeconds = 1.3, Period = .03;
        public const double StepDistance = 24, Radius = 150, FractionPerStep = .015;
        public const int LastSweepStep = 66, RemovalStep = 67;
        public OriginalPoint Position { get; private set; }
        public double Heading { get; }
        public int Steps { get; private set; }
        public double HealthFraction => Steps * FractionPerStep;
        public bool Completed => Steps >= RemovalStep;

        public OriginalOrnGhostRules(OriginalPoint origin, double heading)
        {
            if (!OriginalCombatDefinition.IsFinite(origin.x) || !OriginalCombatDefinition.IsFinite(origin.y) ||
                !OriginalCombatDefinition.IsFinite(heading)) throw new ArgumentOutOfRangeException();
            Position = origin; Heading = heading;
        }

        public bool Tick(out OriginalPoint sweep)
        {
            sweep = Position;
            if (Completed) return false;
            Steps++;
            // VDv removes before movement and enumeration on callback67.
            if (Completed) return false;
            double radians = Heading * Math.PI / 180;
            var requested = new OriginalPoint(Position.x + StepDistance * Math.Cos(radians),
                Position.y + StepDistance * Math.Sin(radians));
            if (OriginalShieldBashRules.AllowsForcedPoint(requested)) Position = requested;
            sweep = Position; return true;
        }

        public static OriginalPoint Portal(int number)
        {
            if (number < 1 || number > 12) throw new ArgumentOutOfRangeException(nameof(number));
            double angle = number * 30 * .0174532;
            return new OriginalPoint(800 * Math.Cos(angle), -2700 + 800 * Math.Sin(angle));
        }
        public static double PortalHeading(int number)
        {
            if (number < 1 || number > 12) throw new ArgumentOutOfRangeException(nameof(number));
            return (number * 30 + 180) % 360;
        }
    }
}

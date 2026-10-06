using System;

namespace Arena.Original
{
    // LiA3.9c nbv/nBv/ncv/nfv/ngv, JASS28821-29070.
    // Each ghost sweeps at its old position before moving22WC. Its final
    // callback still sweeps and moves, then removes it at virtual range1606.
    public sealed class OriginalBossGhostRules
    {
        public const double Period = .04, Speed = 550, Radius = 110, HealthFraction = .15;
        public const int Ticks = 73;
        public OriginalPoint Position { get; private set; }
        public readonly double heading;
        public int Steps { get; private set; }
        public bool Completed => Steps >= Ticks;

        public OriginalBossGhostRules(OriginalPoint origin, double heading)
        {
            if (!OriginalCombatDefinition.IsFinite(origin.x) || !OriginalCombatDefinition.IsFinite(origin.y) ||
                !OriginalCombatDefinition.IsFinite(heading)) throw new ArgumentOutOfRangeException();
            Position = origin; this.heading = heading;
        }
        public bool Tick(out OriginalPoint sweep)
        {
            sweep = Position; if (Completed) return false;
            double angle = heading * Math.PI / 180;
            Position = new OriginalPoint(Position.x + Speed * Period * Math.Cos(angle),
                Position.y + Speed * Period * Math.Sin(angle));
            Steps++; return true;
        }
        public static OriginalPoint[] Formation(double heading, bool oddFirstSide)
        {
            if (!OriginalCombatDefinition.IsFinite(heading)) throw new ArgumentOutOfRangeException(nameof(heading));
            var output = new OriginalPoint[6]; int index = 0;
            double angle = heading * .0174532;
            var origin = new OriginalPoint(-800 * Math.Cos(angle), -2680 - 800 * Math.Sin(angle));
            foreach (int side in new[] { 1, -1 })
            {
                double perpendicular = (heading + side * 90) * .0174532;
                for (int i = 1; i <= 6; i++)
                {
                    bool odd = (i & 1) == 1;
                    if (odd != (side == 1 ? oddFirstSide : !oddFirstSide)) continue;
                    output[index++] = new OriginalPoint(origin.x + (130 * i - 35) * Math.Cos(perpendicular),
                        origin.y + (130 * i - 35) * Math.Sin(perpendicular));
                }
            }
            return output;
        }
        public static bool ThresholdReached(int remainingPhases, double fraction) =>
            remainingPhases == 2 && fraction <= .65 || remainingPhases == 1 && fraction <= .35;
    }
}

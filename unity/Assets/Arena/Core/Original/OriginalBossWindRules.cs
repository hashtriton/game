using System;

namespace Arena.Original
{
    // n_v/n1v/n4v, 3.9c JASS29661..29800. Speed and turning read the
    // remaining phase counter at every callback, including after cleanup.
    public sealed class OriginalBossWindRules
    {
        public const double Period = .04, Radius = 120, Range = 1050, HealthFraction = .18;
        public OriginalPoint Position { get; private set; }
        public double Heading { get; private set; }
        public double Travelled { get; private set; }
        public bool Completed => Travelled >= Range;
        public OriginalBossWindRules(OriginalPoint position, double degrees)
        {
            if (!OriginalCombatDefinition.IsFinite(position.x) || !OriginalCombatDefinition.IsFinite(position.y) ||
                !OriginalCombatDefinition.IsFinite(degrees)) throw new ArgumentOutOfRangeException();
            Position = position; Heading = degrees * Math.PI / 180;
        }
        public bool Tick(int remainingPhases, out OriginalPoint sweep)
        {
            if (remainingPhases < 0 || remainingPhases > 4) throw new ArgumentOutOfRangeException(nameof(remainingPhases));
            sweep = Position; if (Completed) return false;
            double step = (remainingPhases <= 1 ? 650 : remainingPhases == 2 ? 550 : remainingPhases == 3 ? 450 : 350) * Period;
            if (remainingPhases <= 2) Heading += .04;
            Position = new OriginalPoint(Position.x + step * Math.Cos(Heading), Position.y + step * Math.Sin(Heading));
            Travelled += step; sweep = Position; return true;
        }
        public static bool ThresholdReached(int remainingPhases, double fraction) =>
            remainingPhases >= 1 && remainingPhases <= 4 && fraction <=
                (remainingPhases == 4 ? .8 : remainingPhases == 3 ? .6 : remainingPhases == 2 ? .4 : .2);
    }
}

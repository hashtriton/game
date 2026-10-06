using System;

namespace Arena.Original
{
    // Source zE/ZE/vX/eX84392..84395, nMv/nqv/nQv/nyv/nYv29237..29640.
    public static class OriginalBossQuadrantRules
    {
        public static OriginalPoint Minimum(int quadrant)
        {
            Validate(quadrant);
            return new OriginalPoint(quadrant == 1 || quadrant == 4 ? 0 : -864, quadrant <= 2 ? -2720 : -3584);
        }
        public static OriginalPoint Maximum(int quadrant)
        {
            Validate(quadrant);
            return new OriginalPoint(quadrant == 1 || quadrant == 4 ? 864 : 0, quadrant <= 2 ? -1824 : -2720);
        }
        public static bool Contains(int quadrant, OriginalPoint point)
        {
            var a = Minimum(quadrant); var b = Maximum(quadrant);
            return point.x >= a.x && point.x <= b.x && point.y >= a.y && point.y <= b.y;
        }
        public static bool ThresholdReached(int remaining, double fraction) =>
            remaining == 3 && fraction <= .8 || remaining == 2 && fraction <= .6 || remaining == 1 && fraction <= .4;
        static void Validate(int quadrant) { if (quadrant < 1 || quadrant > 4) throw new ArgumentOutOfRangeException(nameof(quadrant)); }
    }
}

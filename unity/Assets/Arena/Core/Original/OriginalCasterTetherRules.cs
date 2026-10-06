using System;

namespace Arena.Original
{
    // Geometry and callback order from Oav/Ofv/OFv33367/34041/34084 in3.9c.
    // Native Abun/A0KV attack/cast/order behavior is a separate integration gate.
    public sealed class OriginalCasterTetherRules
    {
        public const double Interval = .05;
        public readonly bool drag;
        public readonly OriginalPoint origin, destination;
        public double Travelled { get; private set; }
        public double TargetDistance { get; private set; }
        public double DamagePerTick => drag ? 8 : 0;

        public OriginalCasterTetherRules(bool drag, OriginalPoint origin, OriginalPoint destination)
        {
            Point(origin); Point(destination); this.drag=drag; this.origin=origin; this.destination=destination;
        }
        public static OriginalPoint DragDestination(double xDraw, double yDraw)
        {
            if (!OriginalCombatDefinition.IsFinite(xDraw) || !OriginalCombatDefinition.IsFinite(yDraw) ||
                xDraw<0 || xDraw>=1 || yDraw<0 || yDraw>=1) throw new ArgumentOutOfRangeException(nameof(xDraw));
            return new OriginalPoint(-1400+3000*xDraw,-500+3000*yDraw);
        }
        public static OriginalPoint DragInitialCasterPosition(OriginalPoint warning)
        { Point(warning); return new OriginalPoint(warning.x,warning.x); }

        public OriginalPoint Advance(OriginalPoint currentCaster)
        {
            Point(currentCaster);
            var goal=drag?destination:currentCaster;
            double dx=goal.x-origin.x, dy=goal.y-origin.y, distance=Math.Sqrt(dx*dx+dy*dy);
            double travelled=Travelled+(drag?16:8), angle=Math.Atan2(dy,dx);
            var result=new OriginalPoint(origin.x+travelled*Math.Cos(angle),origin.y+travelled*Math.Sin(angle));
            if (!OriginalCombatDefinition.IsFinite(distance)) throw new ArgumentOutOfRangeException(nameof(currentCaster));
            Point(result); Travelled=travelled; TargetDistance=distance;
            return result;
        }

        // Call after forced movement and, for drag, after the8 damage callback.
        public bool ShouldFinish(double casterHealth, double targetHealth)
        {
            if (!OriginalCombatDefinition.IsFinite(casterHealth) || !OriginalCombatDefinition.IsFinite(targetHealth))
                throw new ArgumentOutOfRangeException(nameof(casterHealth));
            return Travelled>=TargetDistance || casterHealth<=.405 || targetHealth<=.405;
        }
        static void Point(OriginalPoint point)
        {
            if (!OriginalCombatDefinition.IsFinite(point.x) || !OriginalCombatDefinition.IsFinite(point.y))
                throw new ArgumentOutOfRangeException(nameof(point));
        }
    }
}

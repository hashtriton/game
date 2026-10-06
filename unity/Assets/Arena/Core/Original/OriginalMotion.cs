using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public struct OriginalPoint
    {
        public double x, y;
        public OriginalPoint(double x, double y) { this.x = x; this.y = y; }
    }

    // Host-side collision, in Warcraft XY units. The terrain callback includes
    // live destructable footprints. This is a Unity replacement for native unit
    // collision, not a claim that Warcraft uses this particular solver.
    public sealed class OriginalMotion
    {
        sealed class Body { internal int id; internal double x, y, radius; }
        readonly SortedDictionary<int, Body> bodies = new SortedDictionary<int, Body>();
        readonly Func<double, double, double, bool> walkable;
        readonly Func<OriginalPoint, OriginalPoint, double, bool> segmentClear;
        const double Epsilon = 1e-7;

        public OriginalMotion(Func<double, double, double, bool> walkable,
            Func<OriginalPoint, OriginalPoint, double, bool> segmentClear)
        {
            this.walkable = walkable ?? throw new ArgumentNullException(nameof(walkable));
            this.segmentClear = segmentClear ?? throw new ArgumentNullException(nameof(segmentClear));
        }

        public void Add(int id, double x, double y, double radius)
        {
            Coordinates(x, y);
            if (id <= 0 || !Finite(radius) || radius <= 0 || radius > 32768)
                throw new ArgumentOutOfRangeException(nameof(radius));
            if (bodies.ContainsKey(id)) throw new ArgumentException("Duplicate body identity.", nameof(id));
            if (!walkable(x, y, radius)) throw new ArgumentException("Body spawn intersects terrain.");
            foreach (var other in bodies.Values)
                if (Squared(x - other.x, y - other.y) < (radius + other.radius) * (radius + other.radius))
                    throw new ArgumentException("Body spawn intersects another live body.");
            bodies.Add(id, new Body { id = id, x = x, y = y, radius = radius });
        }

        public bool Remove(int id) => bodies.Remove(id);
        // Trusted SetUnitX/Y and SetUnitPathing reconstruction. These source
        // operations do not invoke spawn placement. Keep the physical position
        // even in terrain or overlap; subsequent movement still uses collision.
        internal void SetSourceBody(int id, double x, double y, double radius)
        {
            Coordinates(x, y);
            if (id <= 0 || !Finite(radius) || radius <= 0 || radius > 32768)
                throw new ArgumentOutOfRangeException(nameof(radius));
            bodies[id] = new Body { id = id, x = x, y = y, radius = radius };
        }
        public OriginalPoint Position(int id)
        { var body = bodies[id]; return new OriginalPoint(body.x, body.y); }

        // Bounded simulation movement only. Teleports/revival must find a free
        // destination and replace the body; they must not sweep across the map.
        public OriginalPoint Move(int id, double x, double y, double maximumDistance)
        {
            Coordinates(x, y);
            if (!Finite(maximumDistance) || maximumDistance < 0 || maximumDistance > 4096)
                throw new ArgumentOutOfRangeException(nameof(maximumDistance));
            var body = bodies[id];
            double remaining = maximumDistance;
            while (remaining > Epsilon)
            {
                double dx = x - body.x, dy = y - body.y, distance = Math.Sqrt(Squared(dx, dy));
                if (distance <= Epsilon) break;
                double step = Math.Min(Math.Min(remaining, distance), Math.Min(8, Math.Max(1, body.radius * .5)));
                double oldX = body.x, oldY = body.y;
                MoveDelta(body, dx / distance * step, dy / distance * step, 0);
                remaining -= step;
                if (Squared(body.x - oldX, body.y - oldY) < Epsilon * Epsilon) break;
            }
            return new OriginalPoint(body.x, body.y);
        }

        void MoveDelta(Body body, double dx, double dy, int slideDepth)
        {
            double lengthSquared = Squared(dx, dy);
            if (lengthSquared < Epsilon * Epsilon) return;
            double fraction = 1;
            Body hit = null;
            bool Clear(double x, double y) => segmentClear(new OriginalPoint(body.x, body.y),
                new OriginalPoint(x, y), body.radius);
            if (!Clear(body.x + dx, body.y + dy))
            {
                double low = 0, high = 1;
                for (int i = 0; i < 24; i++)
                {
                    double middle = (low + high) * .5;
                    if (Clear(body.x + dx * middle, body.y + dy * middle)) low = middle;
                    else high = middle;
                }
                fraction = low;
            }
            foreach (var other in bodies.Values)
            {
                if (other == body) continue;
                double ox = body.x - other.x, oy = body.y - other.y;
                double toward = ox * dx + oy * dy;
                if (toward >= -Epsilon) continue;
                double radius = body.radius + other.radius;
                double separation = Squared(ox, oy) - radius * radius;
                double discriminant = toward * toward - lengthSquared * separation;
                if (discriminant < 0) continue;
                double at = Math.Max(0, (-toward - Math.Sqrt(discriminant)) / lengthSquared);
                if (at <= fraction) { fraction = at; hit = other; }
            }
            body.x += dx * fraction; body.y += dy * fraction;
            if (fraction >= 1 - Epsilon || slideDepth >= 2) return;
            dx *= 1 - fraction; dy *= 1 - fraction;
            if (hit != null)
            {
                double nx = body.x - hit.x, ny = body.y - hit.y;
                double normalLength = Math.Sqrt(Squared(nx, ny));
                if (normalLength <= Epsilon) return;
                nx /= normalLength; ny /= normalLength;
                double inward = Math.Min(0, dx * nx + dy * ny);
                MoveDelta(body, dx - nx * inward, dy - ny * inward, slideDepth + 1);
            }
            else
            {
                // Terrain is an axis-aligned pathing grid. Sliding on a wall
                // uses one component only, never both full displacement axes.
                if (Math.Abs(dx) >= Math.Abs(dy) && Clear(body.x + dx, body.y))
                    MoveDelta(body, dx, 0, slideDepth + 1);
                else if (Clear(body.x, body.y + dy))
                    MoveDelta(body, 0, dy, slideDepth + 1);
                else if (Clear(body.x + dx, body.y))
                    MoveDelta(body, dx, 0, slideDepth + 1);
            }
        }

        static double Squared(double x, double y) => x * x + y * y;
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static void Coordinates(double x, double y)
        {
            if (!Finite(x) || !Finite(y) || Math.Abs(x) > 1048576 || Math.Abs(y) > 1048576)
                throw new ArgumentOutOfRangeException(nameof(x));
        }
    }
}

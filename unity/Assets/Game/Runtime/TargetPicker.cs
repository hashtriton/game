using UnityEngine;

namespace Game
{
    /// <summary>
    /// Finds what is under the cursor without physics colliders: every target is a vertical segment
    /// and the nearest one to the camera that the ray passes close enough to wins.
    /// </summary>
    public static class TargetPicker
    {
        /// <summary>Extra slack around a target's body so small or distant targets are easy to click.</summary>
        public const float Tolerance = 0.3f;

        public static Targetable Pick(Ray ray, Unit attacker)
        {
            Targetable best = null;
            var bestRayDistance = float.MaxValue;

            foreach (var unit in Unit.All) Consider(unit);
            foreach (var prop in Destructible.All) Consider(prop);
            return best;

            void Consider(Targetable target)
            {
                if (!target.IsAlive || !target.CanBeAttackedBy(attacker)) return;
                var bottom = target.Position;
                var top = bottom + Vector3.up * target.height;
                var gap = RaySegmentDistance(ray, bottom, top, out var alongRay);
                if (gap > target.radius + Tolerance || alongRay >= bestRayDistance) return;
                best = target;
                bestRayDistance = alongRay;
            }
        }

        /// <summary>Shortest distance between a ray and a segment; <paramref name="alongRay"/> is where on the ray it happens.</summary>
        public static float RaySegmentDistance(Ray ray, Vector3 a, Vector3 b, out float alongRay)
        {
            var u = ray.direction;
            var v = b - a;
            var w = ray.origin - a;
            var uv = Vector3.Dot(u, v);
            var vv = Vector3.Dot(v, v);
            var uw = Vector3.Dot(u, w);
            var vw = Vector3.Dot(v, w);

            var denominator = vv - uv * uv;
            float onSegment;
            if (denominator > 1e-6f) onSegment = Mathf.Clamp01((vw - uv * uw) / denominator);
            else onSegment = vv > 1e-6f ? Mathf.Clamp01(vw / vv) : 0f;

            var point = a + v * onSegment;
            alongRay = Mathf.Max(0f, Vector3.Dot(point - ray.origin, u));
            return Vector3.Distance(ray.origin + u * alongRay, point);
        }
    }
}

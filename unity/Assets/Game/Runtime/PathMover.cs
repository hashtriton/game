using System.Collections.Generic;
using Arena;
using UnityEngine;

namespace Game
{
    /// <summary>Walks a unit along an A* route over the original pathing grid. Shared by the hero and the creeps.</summary>
    public sealed class PathMover
    {
        private const float StuckSeconds = 0.35f;

        private readonly Unit unit;
        private readonly ArenaMap map;
        private readonly List<Vector3> path = new List<Vector3>();
        private int index;
        private float stuckTimer;
        private long navigationRevision;

        public PathMover(Unit unit, ArenaMap map)
        {
            this.unit = unit;
            this.map = map;
        }

        public Vector3 Destination { get; private set; }
        public bool IsMoving => index < path.Count;
        /// <summary>The last position the route leads to; differs from the requested point when that point is blocked.</summary>
        public Vector3 RouteEnd => path.Count > 0 ? path[path.Count - 1] : unit.transform.position;

        /// <summary>Plans a route; the previous one is kept when there is none.</summary>
        public bool SetDestination(Vector3 target)
        {
            var route = map.FindPath(unit.transform.position, target, unit.pathRadius);
            if (route.Count == 0) return false;
            path.Clear();
            path.AddRange(route);
            index = 1;
            // The stuck timer is kept: routes are renewed more often than the timer runs out, and a renewal does not mean progress.
            Destination = route[route.Count - 1];
            navigationRevision = map.NavigationRevision;
            return true;
        }

        public void Clear()
        {
            path.Clear();
            index = 0;
            stuckTimer = 0f;
        }

        /// <summary>Moves one frame along the route. Returns false once the route is finished or abandoned.</summary>
        public bool Step(float dt)
        {
            if (!IsMoving) return false;

            // A barrel was destroyed or revived since the route was built: plan again.
            if (navigationRevision != map.NavigationRevision)
            {
                var goal = Destination;
                navigationRevision = map.NavigationRevision;
                if (!SetDestination(goal) || !IsMoving) return false;
            }

            var position = unit.transform.position;
            var toWaypoint = path[index] - position;
            toWaypoint.y = 0f;
            var step = unit.MoveSpeed * dt;

            if (toWaypoint.magnitude <= step)
            {
                index++;
                if (!IsMoving) return false;
                toWaypoint = path[index] - position;
                toWaypoint.y = 0f;
            }

            var direction = toWaypoint.normalized;
            var moved = map.Move(position, direction * step, unit.pathRadius);
            var travelled = new Vector3(moved.x - position.x, 0f, moved.z - position.z).magnitude;
            unit.transform.position = moved;
            unit.Actor.Face(direction, dt);

            // Pathing grid and body disagree (a body edge on an obstacle corner) or another unit blocks the way:
            // give up instead of jittering on the spot.
            stuckTimer = travelled < step * 0.2f ? stuckTimer + dt : 0f;
            if (stuckTimer > StuckSeconds)
            {
                Clear();
                return false;
            }
            return true;
        }
    }
}

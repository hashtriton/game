using System.Collections.Generic;
using Arena;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Right click moves the hero along an A* path over the original arena pathing grid.
    /// Holding the button keeps steering towards the cursor, like Warcraft III.
    /// </summary>
    [RequireComponent(typeof(ArenaActor))]
    public sealed class HeroController : MonoBehaviour
    {
        public ArenaMap map;
        public Camera viewCamera;
        public GameObject modelPrefab;
        public ClickMarker marker;
        public float height = 1.9f;
        public float radius = 0.4f;
        public float speed = 4.7f;
        // Gap between repaths while the button is held, keeps A* off the hot path.
        public float holdRepathInterval = 0.12f;

        private const float StuckSeconds = 0.35f;

        private ArenaActor actor;
        private readonly List<Vector3> path = new List<Vector3>();
        private int pathIndex;
        private float holdTimer;
        private float stuckTimer;
        private long navigationRevision;
        private Vector3 destination;

        public bool IsMoving => pathIndex < path.Count;

        private void Start()
        {
            actor = GetComponent<ArenaActor>();
            actor.Initialize(modelPrefab, height, radius, true, null);
            var spawn = map.HeroSpawn;
            transform.position = spawn;
            navigationRevision = map.NavigationRevision;
        }

        private void Update()
        {
            ReadOrders();
            Walk(Time.deltaTime);
        }

        private void ReadOrders()
        {
            var mouse = Mouse.current;
            if (mouse == null || viewCamera == null) return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                holdTimer = 0f;
                if (TryGroundPoint(out var point)) Order(point, true);
            }
            else if (mouse.rightButton.isPressed)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdRepathInterval)
                {
                    holdTimer = 0f;
                    if (TryGroundPoint(out var point)) Order(point, false);
                }
            }
        }

        private bool TryGroundPoint(out Vector3 point)
        {
            var ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out var hit, 500f))
            {
                point = hit.point;
                return true;
            }
            // Outside the terrain collider: fall back to the hero's ground plane.
            var plane = new Plane(Vector3.up, transform.position);
            if (plane.Raycast(ray, out var distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = default;
            return false;
        }

        public void Order(Vector3 target, bool showMarker)
        {
            var route = map.FindPath(transform.position, target, radius);
            if (route.Count == 0) return;
            path.Clear();
            path.AddRange(route);
            pathIndex = 1;
            stuckTimer = 0f;
            destination = route[route.Count - 1];
            navigationRevision = map.NavigationRevision;
            if (showMarker && marker != null) marker.Show(destination);
        }

        private void Walk(float dt)
        {
            if (!IsMoving)
            {
                actor.SetMoving(false);
                return;
            }

            // A barrel was destroyed or revived since the route was built: plan again.
            if (navigationRevision != map.NavigationRevision)
            {
                navigationRevision = map.NavigationRevision;
                Order(destination, false);
                if (!IsMoving)
                {
                    actor.SetMoving(false);
                    return;
                }
            }

            var position = transform.position;
            var waypoint = path[pathIndex];
            var toWaypoint = waypoint - position;
            toWaypoint.y = 0f;
            var step = speed * dt;

            if (toWaypoint.magnitude <= step)
            {
                pathIndex++;
                if (!IsMoving)
                {
                    actor.SetMoving(false);
                    return;
                }
                waypoint = path[pathIndex];
                toWaypoint = waypoint - position;
                toWaypoint.y = 0f;
            }

            var direction = toWaypoint.normalized;
            var moved = map.Move(position, direction * step, radius);
            var travelled = new Vector3(moved.x - position.x, 0f, moved.z - position.z).magnitude;
            transform.position = moved;
            actor.Face(direction, dt);
            actor.SetMoving(true);

            // Pathing grid and body disagree (a body edge on an obstacle corner): give up instead of jittering.
            stuckTimer = travelled < step * 0.2f ? stuckTimer + dt : 0f;
            if (stuckTimer > StuckSeconds)
            {
                path.Clear();
                pathIndex = 0;
                actor.SetMoving(false);
            }
        }
    }
}

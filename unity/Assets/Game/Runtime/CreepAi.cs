using Arena;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// A creep goes for the nearest hero it notices, walks up along the pathing grid and hits it until one of them falls.
    /// With <see cref="alwaysHunt"/> it knows where the hero is from the start, which is how wave creeps behave.
    /// </summary>
    [RequireComponent(typeof(Unit))]
    public sealed class CreepAi : MonoBehaviour
    {
        public ArenaMap map;
        public float aggroRange = 8f;
        public bool alwaysHunt;
        /// <summary>A creep that was provoked gives up when the hero gets this far away.</summary>
        public float leashRange = 16f;

        private Unit unit;
        private PathMover mover;
        private Unit target;
        private float repathTimer;
        private float unreachableTimer;

        private void Start()
        {
            unit = GetComponent<Unit>();
            unit.map = map;
            mover = new PathMover(unit, map);
            // Spread the route renewals of a crowd over several frames.
            repathTimer = Random.value * 0.4f;
            unit.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (unit != null) unit.Died -= OnDied;
        }

        private void OnDied(Unit victim, Unit killer)
        {
            target = null;
            mover.Clear();
        }

        private void Update()
        {
            if (!unit.IsAlive) return;
            var dt = Time.deltaTime;
            var actor = unit.Actor;

            if (unit.IsSwinging)
            {
                actor.SetMoving(false);
                if (target != null) actor.Face(target.transform.position - transform.position, dt);
                return;
            }

            if (target == null || !target.IsAlive || TooFar(target)) target = ChooseTarget();
            if (target == null)
            {
                mover.Clear();
                actor.SetMoving(false);
                return;
            }

            if (target.EdgeDistance(transform.position) <= unit.attackRange)
            {
                mover.Clear();
                actor.SetMoving(false);
                actor.Face(target.transform.position - transform.position, dt);
                if (unit.CanAttackNow) unit.BeginAttack(target);
                return;
            }

            repathTimer -= dt;
            if (repathTimer <= 0f)
            {
                repathTimer = 0.45f;
                mover.SetDestination(target.transform.position);
            }

            if (mover.Step(dt))
            {
                actor.SetMoving(true);
                unreachableTimer = 0f;
                return;
            }

            // Boxed in by barrels or by its own pack: wait and try again.
            actor.SetMoving(false);
            unreachableTimer += dt;
            if (unreachableTimer > 0.3f)
            {
                unreachableTimer = 0f;
                repathTimer = 0f;
            }
        }

        private bool TooFar(Unit candidate)
        {
            if (alwaysHunt) return false;
            var away = candidate.transform.position - transform.position;
            away.y = 0f;
            return away.magnitude > leashRange;
        }

        private Unit ChooseTarget()
        {
            Unit best = null;
            var bestDistance = alwaysHunt ? float.MaxValue : aggroRange;
            foreach (var other in Unit.All)
            {
                if (!other.CanBeAttackedBy(unit)) continue;
                var distance = other.EdgeDistance(transform.position);
                if (distance > bestDistance) continue;
                best = other;
                bestDistance = distance;
            }
            return best;
        }
    }
}

using Arena;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Dota-style orders. Right click on the ground walks there (hold to keep steering to the cursor).
    /// Right click on an enemy or a barrel walks up to it and attacks until it falls, however far it was.
    /// A then left click attacks along the way, S stops. An idle hero fights back whoever comes close.
    /// </summary>
    [RequireComponent(typeof(Unit))]
    public sealed class HeroController : MonoBehaviour
    {
        private enum OrderKind { None, Move, Attack, AttackMove }

        public ArenaMap map;
        public Camera viewCamera;
        public ClickMarker marker;
        public ClickMarker attackMarker;
        public TargetHighlight highlight;
        // Gap between repaths while the button is held, keeps A* off the hot path.
        public float holdRepathInterval = 0.12f;
        /// <summary>How far an attack-moving hero notices enemies, measured to their edge.</summary>
        public float acquireRange = 9f;
        /// <summary>Extra distance beyond the weapon reach at which an idle hero starts a fight.</summary>
        public float idleAcquireMargin = 2.8f;
        /// <summary>An enemy picked by the hero itself is dropped when it runs this far away.</summary>
        public float leashRange = 11f;

        private Unit unit;
        private PathMover mover;
        private OrderKind order;
        private Targetable target;
        private bool targetChosenByHero;
        private bool resumeAttackMove;
        private Vector3 attackMoveGoal;
        private bool attackMoveArmed;
        private bool steeringOnGround;
        private float holdTimer;
        private float repathTimer;
        private float unreachableTimer;
        private float scanTimer;

        // Lazy: other components may ask for it in their own Awake, before this one has run.
        public Unit Unit => unit != null ? unit : (unit = GetComponent<Unit>());
        public float radius => unit.radius;
        public bool IsMoving => mover != null && mover.IsMoving;
        public bool IsAttackMoveArmed => attackMoveArmed;
        public Targetable Target => order == OrderKind.Attack ? target : null;

        private void Awake()
        {
            unit = GetComponent<Unit>();
        }

        private void Start()
        {
            unit.map = map;
            mover = new PathMover(unit, map);
            transform.position = map.HeroSpawn;
            unit.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (unit != null) unit.Died -= OnDied;
            GameCursor.Set(CursorKind.Default);
        }

        private void OnDied(Unit victim, Unit killer)
        {
            Stop();
            attackMoveArmed = false;
            GameCursor.Set(CursorKind.Default);
            if (highlight != null) highlight.Hide();
        }

        private void Update()
        {
            if (!unit.IsAlive) return;
            ReadInput();
            RunOrder(Time.deltaTime);
        }

        // ---- input -------------------------------------------------------------------------------------------

        private void ReadInput()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null || viewCamera == null) return;

            var overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            Ray ray = default;
            Targetable hovered = null;
            if (!overUi)
            {
                ray = viewCamera.ScreenPointToRay(mouse.position.ReadValue());
                hovered = TargetPicker.Pick(ray, unit);
            }

            if (keyboard != null)
            {
                if (keyboard.sKey.wasPressedThisFrame) Stop();
                if (keyboard.aKey.wasPressedThisFrame) attackMoveArmed = true;
                if (keyboard.escapeKey.wasPressedThisFrame) attackMoveArmed = false;
            }

            GameCursor.Set(!overUi && (hovered != null || attackMoveArmed) ? CursorKind.Attack : CursorKind.Default);
            if (highlight != null) highlight.Show(hovered);
            if (overUi) return;

            if (attackMoveArmed)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    attackMoveArmed = false;
                    if (hovered != null) OrderAttack(hovered);
                    else if (TryGroundPoint(ray, out var point)) OrderAttackMove(point);
                }
                else if (mouse.rightButton.wasPressedThisFrame) attackMoveArmed = false;
                return;
            }

            if (mouse.rightButton.wasPressedThisFrame)
            {
                holdTimer = 0f;
                steeringOnGround = false;
                if (hovered != null) OrderAttack(hovered);
                else if (TryGroundPoint(ray, out var point))
                {
                    steeringOnGround = true;
                    OrderMove(point, true);
                }
            }
            else if (mouse.rightButton.isPressed && steeringOnGround)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdRepathInterval)
                {
                    holdTimer = 0f;
                    if (TryGroundPoint(ray, out var point)) OrderMove(point, false);
                }
            }
        }

        private bool TryGroundPoint(Ray ray, out Vector3 point)
        {
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

        // ---- orders ------------------------------------------------------------------------------------------

        /// <summary>Walk order, kept under this name for the code that predates attack orders.</summary>
        public void Order(Vector3 point, bool showMarker) => OrderMove(point, showMarker);

        public void OrderMove(Vector3 point, bool showMarker)
        {
            if (!mover.SetDestination(point)) return;
            order = OrderKind.Move;
            target = null;
            resumeAttackMove = false;
            if (showMarker && marker != null) marker.Show(mover.Destination);
        }

        public void OrderAttack(Targetable victim)
        {
            if (victim == null || !victim.CanBeAttackedBy(unit)) return;
            order = OrderKind.Attack;
            target = victim;
            targetChosenByHero = false;
            resumeAttackMove = false;
            repathTimer = 0f;
            unreachableTimer = 0f;
            mover.Clear();
            if (attackMarker != null) attackMarker.Show(victim.Position);
        }

        public void OrderAttackMove(Vector3 point)
        {
            if (!mover.SetDestination(point)) return;
            order = OrderKind.AttackMove;
            target = null;
            resumeAttackMove = false;
            attackMoveGoal = mover.Destination;
            scanTimer = 0f;
            if (attackMarker != null) attackMarker.Show(mover.Destination);
        }

        public void Stop()
        {
            order = OrderKind.None;
            target = null;
            resumeAttackMove = false;
            if (mover != null) mover.Clear();
            if (unit != null && unit.Actor != null) unit.Actor.SetMoving(false);
        }

        // ---- execution ---------------------------------------------------------------------------------------

        private void RunOrder(float dt)
        {
            var actor = unit.Actor;
            // The blow is winding up: stand still and keep facing the victim.
            if (unit.IsSwinging)
            {
                actor.SetMoving(false);
                if (target != null) FaceTarget(dt);
                return;
            }

            switch (order)
            {
                case OrderKind.Move:
                    actor.SetMoving(mover.Step(dt));
                    if (!mover.IsMoving) order = OrderKind.None;
                    break;
                case OrderKind.Attack:
                    RunAttack(dt);
                    break;
                case OrderKind.AttackMove:
                    RunAttackMove(dt);
                    break;
                default:
                    actor.SetMoving(false);
                    RunIdle(dt);
                    break;
            }
        }

        private void RunAttack(float dt)
        {
            if (target == null || !target.IsAlive)
            {
                TargetGone();
                return;
            }

            var actor = unit.Actor;
            if (InReach(target))
            {
                mover.Clear();
                actor.SetMoving(false);
                FaceTarget(dt);
                if (unit.CanAttackNow) unit.BeginAttack(target);
                return;
            }

            var away = target.Position - transform.position;
            away.y = 0f;
            if (targetChosenByHero && away.magnitude > leashRange)
            {
                Stop();
                return;
            }

            // Walk up to it. The victim may move, so the route is renewed a few times a second.
            repathTimer -= dt;
            if (repathTimer <= 0f)
            {
                repathTimer = 0.3f;
                mover.SetDestination(target.Position);
            }

            if (mover.Step(dt))
            {
                actor.SetMoving(true);
                unreachableTimer = 0f;
                return;
            }

            // The route ended short of the reach (a barrel deep in the field, a victim behind a wall): give up.
            actor.SetMoving(false);
            unreachableTimer += dt;
            if (unreachableTimer > 0.8f) Stop();
        }

        private void RunAttackMove(float dt)
        {
            scanTimer -= dt;
            if (scanTimer <= 0f)
            {
                scanTimer = 0.2f;
                var enemy = NearestEnemy(acquireRange);
                if (enemy != null)
                {
                    order = OrderKind.Attack;
                    target = enemy;
                    targetChosenByHero = true;
                    resumeAttackMove = true;
                    unreachableTimer = 0f;
                    repathTimer = 0f;
                    mover.Clear();
                    return;
                }
            }

            var moving = mover.Step(dt);
            unit.Actor.SetMoving(moving);
            if (!moving) order = OrderKind.None;
        }

        private void RunIdle(float dt)
        {
            scanTimer -= dt;
            if (scanTimer > 0f) return;
            scanTimer = 0.25f;
            var enemy = NearestEnemy(unit.attackRange + idleAcquireMargin);
            if (enemy == null) return;
            order = OrderKind.Attack;
            target = enemy;
            targetChosenByHero = true;
            resumeAttackMove = false;
            unreachableTimer = 0f;
            repathTimer = 0f;
        }

        private void TargetGone()
        {
            target = null;
            if (resumeAttackMove && mover.SetDestination(attackMoveGoal))
            {
                order = OrderKind.AttackMove;
                resumeAttackMove = false;
                return;
            }
            order = OrderKind.None;
            resumeAttackMove = false;
            unit.Actor.SetMoving(false);
        }

        private bool InReach(Targetable victim) => victim.EdgeDistance(transform.position) <= unit.attackRange;

        private void FaceTarget(float dt)
        {
            if (target != null) unit.Actor.Face(target.Position - transform.position, dt);
        }

        private Unit NearestEnemy(float range)
        {
            Unit best = null;
            var bestDistance = range;
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

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalWorldTransition
    {
        public int entityId;
        public bool relocate, revive, fullHealth, fullMana, stop, setFacing, kill, remove;
        public OriginalPoint position;
        public double facing;
        public bool? visible, paused, invulnerable;
    }

    public sealed partial class OriginalWorld
    {
        sealed class TransitionCandidate
        {
            internal Unit unit;
            internal OriginalPoint position;
            internal double health, mana, facing;
            internal bool hidden, paused, invulnerable, moving, stop, setFacing, removed, clearTemporaryInvulnerability;
            internal bool Occupies => !removed && health > 0 && !hidden && !unit.pathingDisabled;
        }

        // The duel referee's setup/return batches contain no damage callbacks.
        // Fold their explicit state changes in source order, reserve every final
        // body, then publish once. The bounded fallback is Unity placement, not
        // an assertion about Warcraft's private SetUnitPosition search order.
        public bool TryApplyTransitions(OriginalWorldTransition[] changes, double maxSearch = 512)
        {
            if (changes == null || changes.Length > 4096 || !Finite(maxSearch) || maxSearch < 0 || maxSearch > 4096)
                throw new ArgumentException("Invalid world transition batch.");
            var candidates = new Dictionary<int, TransitionCandidate>();
            var ordered = new List<TransitionCandidate>();
            var pendingEvents = new List<OriginalWorldEvent>();
            foreach (var change in changes)
            {
                if (change == null || !units.TryGetValue(change.entityId, out var unit)) return false;
                if (change.relocate) Coordinates(change.position);
                if (change.setFacing && !Finite(change.facing)) throw new ArgumentOutOfRangeException(nameof(change.facing));
                if (!candidates.TryGetValue(change.entityId, out var candidate))
                {
                    candidate = new TransitionCandidate { unit = unit, position = unit.position, health = unit.health, mana = unit.mana,
                        hidden = unit.hidden, paused = unit.paused, invulnerable = unit.invulnerable };
                    candidates.Add(unit.id, candidate); ordered.Add(candidate);
                }
                if (candidate.removed) return false; // Removal is terminal; no later operation may reuse that identity.
                if (change.relocate) { candidate.position = change.position; candidate.moving = true; }
                if (change.revive)
                {
                    if (candidate.health <= 0) candidate.moving = true;
                    candidate.health = unit.profile.maxHealth;
                }
                if (change.fullHealth && candidate.health > 0) candidate.health = unit.profile.maxHealth;
                if (change.fullMana) candidate.mana = unit.profile.maxMana;
                if (change.visible.HasValue)
                {
                    if (candidate.hidden && change.visible.Value) candidate.moving = true;
                    candidate.hidden = !change.visible.Value;
                }
                if (change.paused.HasValue) candidate.paused = change.paused.Value;
                if (change.invulnerable.HasValue) candidate.invulnerable = change.invulnerable.Value;
                candidate.stop |= change.stop;
                if (change.setFacing) { candidate.setFacing = true; candidate.facing = change.facing; }
                if (change.kill && candidate.health > 0)
                {
                    candidate.clearTemporaryInvulnerability = true;
                    candidate.health = 0; candidate.stop = true;
                    pendingEvents.Add(new OriginalWorldEvent { kind = OriginalWorldEventKind.UnitDied,
                        entityId = unit.id, rawcode = unit.rawcode });
                }
                if (change.remove)
                {
                    candidate.removed = true;
                    pendingEvents.Add(new OriginalWorldEvent { kind = OriginalWorldEventKind.UnitRemoved,
                        entityId = unit.id, rawcode = unit.rawcode });
                }
            }
            // Copied units have detached lineage and may outlive a removed donor.
            var reserved = new List<TransitionCandidate>();
            foreach (var candidate in ordered)
                if (candidate.Occupies && !candidate.moving) reserved.Add(candidate);
            bool FreeFor(OriginalPoint point, TransitionCandidate candidate)
            {
                double radius = candidate.unit.profile.collisionRadius;
                if (Math.Abs(point.x) > 1048576 || Math.Abs(point.y) > 1048576 || !navigation.IsWalkable(point.x, point.y, radius)) return false;
                foreach (var other in units.Values)
                    if (!candidates.ContainsKey(other.id) && other.health > 0 && !other.hidden && !other.pathingDisabled &&
                        Distance(point, other.position) < radius + other.profile.collisionRadius) return false;
                foreach (var other in reserved)
                    if (Distance(point, other.position) < radius + other.unit.profile.collisionRadius) return false;
                return true;
            }
            foreach (var candidate in ordered)
            {
                if (candidate.removed || !candidate.moving) continue;
                if (!candidate.Occupies)
                {
                    if (!candidate.unit.pathingDisabled && !navigation.IsWalkable(candidate.position.x, candidate.position.y, candidate.unit.profile.collisionRadius)) return false;
                    continue;
                }
                var desired = candidate.position;
                bool found = FreeFor(desired, candidate);
                double step = Math.Min(16, maxSearch);
                int rings = step == 0 ? 0 : (int)Math.Ceiling(maxSearch / step);
                bool TryOffset(int x, int y)
                {
                    if ((x * step) * (x * step) + (y * step) * (y * step) > maxSearch * maxSearch) return false;
                    var point = new OriginalPoint(desired.x + x * step, desired.y + y * step);
                    if (!FreeFor(point, candidate)) return false;
                    candidate.position = point; return true;
                }
                for (int ring = 1; ring <= rings && !found; ring++)
                {
                    for (int x = -ring; x <= ring && !found; x++) found = TryOffset(x, -ring) || TryOffset(x, ring);
                    for (int y = -ring + 1; y < ring && !found; y++) found = TryOffset(-ring, y) || TryOffset(ring, y);
                }
                if (!found) return false;
                reserved.Add(candidate);
            }
            // All operations below are infallible for the validated, fixed
            // registry. Explicit kills/removals publish their existing events;
            // ordinary referee returns do not synthesize death or owner credit.
            foreach (var candidate in ordered) motion.Remove(candidate.unit.id);
            foreach (var candidate in ordered)
            {
                var unit = candidate.unit;
                if (!unit.hidden && candidate.hidden) ClearTargets(OriginalWorldTargetKind.Unit, unit.id);
                unit.position = candidate.position; unit.health = candidate.health; unit.mana = candidate.mana;
                unit.hidden = candidate.hidden; unit.paused = candidate.paused; unit.invulnerable = candidate.invulnerable;
                if (candidate.clearTemporaryInvulnerability) unit.temporaryInvulnerability.Clear();
                if (candidate.moving) { unit.path = Array.Empty<OriginalPoint>(); unit.pathIndex = 0; unit.pathRevision = -1; }
                if (candidate.stop) { ClearOrder(unit); unit.holding = false; }
                if (candidate.setFacing && unit.health > 0)
                { unit.hasFacing = true; unit.facingDegrees = (candidate.facing % 360 + 360) % 360; }
                if (candidate.removed) units.Remove(unit.id);
                else if (candidate.Occupies) motion.SetSourceBody(unit.id, unit.position.x, unit.position.y, unit.profile.collisionRadius);
            }
            foreach (var item in pendingEvents)
            {
                ClearTargets(OriginalWorldTargetKind.Unit, item.entityId);
                Emit(item.kind, entityId: item.entityId, rawcode: item.rawcode);
            }
            if (ordered.Count > 0) revision++;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class CastApproachTarget
        {
            internal int actor, unitTarget;
            internal long itemTarget;
            internal OriginalPoint point;
            internal double range;
        }

        sealed class QueuedCastApproach
        {
            internal int actor, owner;
            internal OriginalSessionCommand command;
            internal OriginalPoint target, goal;
            internal long navigationRevision;
        }
        readonly Dictionary<int, QueuedCastApproach> queuedCastApproaches = new Dictionary<int, QueuedCastApproach>();

        static OriginalSessionCommand CopyCastCommand(OriginalSessionCommand command) => new OriginalSessionCommand {
            kind = command.kind, actorEntityId = command.actorEntityId, skillId = command.skillId,
            bag = command.bag, itemSlot = command.itemSlot, itemInstanceId = command.itemInstanceId,
            targetItemInstanceId = command.targetItemInstanceId, targetKind = command.targetKind,
            targetId = command.targetId, x = command.x, y = command.y };

        bool OwnsCastApproachMove(QueuedCastApproach intent, OriginalWorldUnitView actor) => actor != null &&
            actor.order == OriginalWorldOrder.Move && SquaredDistance(actor.destination, intent.goal) <= 1e-8;

        void CancelQueuedCastApproach(int actorId, bool stopMove = false)
        {
            if (!queuedCastApproaches.TryGetValue(actorId, out var intent)) return;
            queuedCastApproaches.Remove(actorId);
            if (stopMove && OwnsCastApproachMove(intent, world?.UnitState(actorId))) world.Stop(actorId);
        }

        OriginalSessionReplyCode PlanCastApproach(Player player, OriginalSessionCommand command, out CastApproachTarget target) =>
            command.kind == OriginalSessionCommandKind.UseItem ? PlanItemCastApproach(player, command, out target) :
            PlanSkillCastApproach(player, command, out target);

        OriginalSessionReplyCode ExecuteCastCommand(Player player, OriginalSessionCommand command) =>
            command.kind == OriginalSessionCommandKind.UseItem ? UseItem(player, command) : ApplyAbilityCommand(player, command);

        OriginalSessionReplyCode ApplyCastCommand(Player player, OriginalSessionCommand command)
        {
            int actorId = command.kind == OriginalSessionCommandKind.UseItem || command.actorEntityId == 0 ?
                OriginalWorld.HeroEntityId(player.slot) : command.actorEntityId;
            queuedCastApproaches.TryGetValue(actorId, out var previous);
            var valid = PlanCastApproach(player, command, out var target);
            // Preserve the existing near/no-target consumer's rejection code
            // and item action reporting. Only pure, valid distant plans queue.
            if (valid != OriginalSessionReplyCode.Accepted) return ExecuteCastCommand(player, command);
            OriginalSessionReplyCode result;
            if (command.kind == OriginalSessionCommandKind.UseItem && ActorItemBlocked(actorId))
            {
                QueueItemOrder(player, command);
                result = OriginalSessionReplyCode.Accepted;
            }
            else if (target != null && SquaredDistance(world.UnitState(actorId).position, target.point) > target.range * target.range)
                result = QueueCastApproach(player, command, target);
            else result = ExecuteCastCommand(player, command);
            if (result == OriginalSessionReplyCode.Accepted && previous != null &&
                queuedCastApproaches.TryGetValue(actorId, out var current) && ReferenceEquals(current, previous))
                CancelQueuedCastApproach(actorId);
            return result;
        }

        bool CastApproachGoal(OriginalWorldUnitView actor, CastApproachTarget target, out OriginalPoint goal)
        {
            goal = default;
            double angle = Math.Atan2(actor.position.y - target.point.y, actor.position.x - target.point.x);
            // Derived host routing: reuse the existing .8 standoff and bounded
            // angular search; the declared cast range itself is unchanged.
            foreach (double fraction in new[] { .8, 1.0 })
                for (int i = 0; i < 16; i++)
                {
                    int offset = i == 0 ? 0 : (i + 1) / 2 * (i % 2 == 0 ? -1 : 1);
                    double direction = angle + offset * Math.PI / 8;
                    // Host numeric stability, not a native range buffer:
                    // remain strictly inside after coordinate rounding so the
                    // existing effect consumer accepts the same destination.
                    // World arrival tolerance is1e-7 WC; a1e-6 WC interior
                    // floor also covers stopping short of the exact goal.
                    double margin = Math.Max(1e-6, Math.Max(target.range, Math.Max(Math.Abs(target.point.x), Math.Abs(target.point.y))) * 1e-12);
                    double radius = target.range * fraction - margin;
                    if (radius <= 0) continue;
                    var candidate = new OriginalPoint(target.point.x + Math.Cos(direction) * radius,
                        target.point.y + Math.Sin(direction) * radius);
                    if (!ValidPoint(candidate.x, candidate.y) || !actor.pathingDisabled &&
                        !navigation.IsWalkable(candidate.x, candidate.y, actor.profile.collisionRadius)) continue;
                    if (!actor.pathingDisabled && !navigation.SegmentClear(actor.position, candidate, actor.profile.collisionRadius))
                    {
                        var path = navigation.FindPath(actor.position, candidate, actor.profile.collisionRadius);
                        if (path == null || path.Length == 0 || SquaredDistance(path[path.Length - 1], candidate) > 1e-8) continue;
                    }
                    goal = candidate; return true;
                }
            return false;
        }

        OriginalSessionReplyCode QueueCastApproach(Player player, OriginalSessionCommand command, CastApproachTarget target)
        {
            var actor = world.UnitState(target.actor);
            if (!CastApproachGoal(actor, target, out var goal)) return OriginalSessionReplyCode.NotReady;
            if (!world.TryMove(target.actor, goal)) return OriginalSessionReplyCode.NotReady;
            OnAcceptedWorldOrder(target.actor);
            // A source taunt may replace this accepted move with its attack.
            if (world.UnitState(target.actor).order != OriginalWorldOrder.Move) return OriginalSessionReplyCode.Accepted;
            queuedCastApproaches[target.actor] = new QueuedCastApproach { actor = target.actor, owner = player.slot,
                command = CopyCastCommand(command), target = target.point, goal = goal,
                navigationRevision = navigation.NavigationRevision };
            return OriginalSessionReplyCode.Accepted;
        }

        void AdvanceQueuedCastApproaches()
        {
            if (world == null || !Started || pendingDuel || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
            {
                foreach (int id in new List<int>(queuedCastApproaches.Keys)) CancelQueuedCastApproach(id, true);
                return;
            }
            foreach (int id in new List<int>(queuedCastApproaches.Keys))
            {
                if (!queuedCastApproaches.TryGetValue(id, out var intent)) continue;
                var actor = world.UnitState(id); var player = players.Find(p => p.slot == intent.owner);
                if (actor == null || actor.health <= 0 || player == null || actor.ownerSlot != intent.owner)
                { CancelQueuedCastApproach(id, true); continue; }
                if (actor.paused || actor.hidden) continue;
                if (intent.command.kind == OriginalSessionCommandKind.UseItem && ActorItemBlocked(id)) continue;
                if (intent.command.kind == OriginalSessionCommandKind.CastSkill && ActorCastBlocked(id))
                { CancelQueuedCastApproach(id, true); continue; }
                if (actor.order != OriginalWorldOrder.None && !OwnsCastApproachMove(intent, actor))
                { CancelQueuedCastApproach(id); continue; }
                var valid = PlanCastApproach(player, intent.command, out var target);
                if (valid != OriginalSessionReplyCode.Accepted || target == null)
                { CancelQueuedCastApproach(id, true); continue; }
                if (SquaredDistance(actor.position, target.point) <= target.range * target.range)
                {
                    CancelQueuedCastApproach(id, true);
                    ExecuteCastCommand(player, intent.command);
                    continue;
                }
                if (SquaredDistance(target.point, intent.target) > 1e-8 || navigation.NavigationRevision != intent.navigationRevision)
                {
                    if (!CastApproachGoal(actor, target, out var goal) || !world.TryMove(id, goal))
                    { CancelQueuedCastApproach(id, true); continue; }
                    intent.goal = goal; intent.target = target.point; intent.navigationRevision = navigation.NavigationRevision;
                }
            }
        }
    }
}

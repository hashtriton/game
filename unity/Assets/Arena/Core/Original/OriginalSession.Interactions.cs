using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class QueuedInteraction
        {
            internal int actor, owner;
            internal OriginalSessionCommandKind kind;
            internal long item;
            internal OriginalPoint target;
        }
        readonly Dictionary<int, QueuedInteraction> queuedInteractions = new Dictionary<int, QueuedInteraction>();

        void CancelQueuedInteraction(int actorId) => queuedInteractions.Remove(actorId);

        OriginalSessionReplyCode AfterAcceptedInteractionInterrupt(Player player, OriginalSessionCommand command, OriginalSessionReplyCode result)
        {
            if (result == OriginalSessionReplyCode.Accepted)
            {
                // Inventory commands operate on the canonical hero even when
                // their unrelated actor field was supplied by a remote peer.
                bool heroItem = command.kind == OriginalSessionCommandKind.UseItem || command.kind == OriginalSessionCommandKind.BuyItem ||
                    command.kind == OriginalSessionCommandKind.SellItem || command.kind == OriginalSessionCommandKind.DropItem ||
                    command.kind == OriginalSessionCommandKind.TransferItem || command.kind == OriginalSessionCommandKind.PickupItem;
                int actor = heroItem || command.actorEntityId == 0 ? OriginalWorld.HeroEntityId(player.slot) : command.actorEntityId;
                CancelQueuedInteraction(actor);
                if (command.kind == OriginalSessionCommandKind.UseWell) CancelQueuedCastApproach(actor);
            }
            return result;
        }

        OriginalSessionReplyCode InteractionTarget(Player player, QueuedInteraction intent, out OriginalWorldUnitView actor,
            out OriginalPoint target, out double range)
        {
            actor = world?.UnitState(intent.actor); target = default; range = 0;
            if (!Started || world == null || pendingDuel || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            if (actor == null || actor.ownerSlot != player.slot) return OriginalSessionReplyCode.InvalidCommand;
            if (actor.health <= 0 || actor.paused || actor.hidden) return OriginalSessionReplyCode.NotReady;
            if (intent.kind == OriginalSessionCommandKind.InteractWell)
            {
                if (!CanInteractHealingWell(player, actor.entityId, out actor)) return OriginalSessionReplyCode.NotReady;
                target = SnapshotHealingWellView().position;
                range = HealingWellActorRange(actor);
                return OriginalSessionReplyCode.Accepted;
            }
            if (intent.kind != OriginalSessionCommandKind.InteractItem || intent.item <= 0)
                return OriginalSessionReplyCode.InvalidCommand;
            if (actor.kind != OriginalWorldUnitKind.Hero || actor.entityId != OriginalWorld.HeroEntityId(player.slot))
                return OriginalSessionReplyCode.InvalidCommand;
            if (itemRules == null || player.inventory == null) return OriginalSessionReplyCode.NotReady;
            if (!groundItems.TryGetValue(intent.item, out var ground) || ground.item == null || ground.item.removed)
                return RejectItem(player, OriginalItemActionCode.InvalidInstance);
            if (!player.inventory.CanPickup(ground.item)) return RejectItem(player, OriginalItemActionCode.WrongOwner);
            var declared = native.Constant("PickupItemRange");
            if (!declared.known) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
            target = ground.position; range = declared.Require();
            return ValidPoint(target.x, target.y) && range > 0 ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.RuleUnavailable;
        }

        OriginalSessionReplyCode ExecuteInteraction(Player player, QueuedInteraction intent) =>
            intent.kind == OriginalSessionCommandKind.InteractWell ? UseHealingWell(player, intent.actor) :
            ApplyItemCommand(player, new OriginalSessionCommand { kind = OriginalSessionCommandKind.PickupItem,
                actorEntityId = intent.actor, itemInstanceId = intent.item, bag = OriginalInventoryBag.Hero });

        OriginalSessionReplyCode MoveForInteraction(Player player, QueuedInteraction intent, OriginalWorldUnitView actor,
            OriginalPoint target, double range)
        {
            double dx = actor.position.x - target.x, dy = actor.position.y - target.y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            var preferred = distance > 0 ? new OriginalPoint(target.x + dx / distance * range * .8,
                target.y + dy / distance * range * .8) : target;
            if (!navigation.IsWalkable(preferred.x, preferred.y, actor.profile.collisionRadius) &&
                !world.TryFindFreeSpawn(target, actor.profile.collisionRadius, range * .8, out preferred))
                return OriginalSessionReplyCode.InvalidCommand;
            var result = ApplyWorldCommand(player, new OriginalSessionCommand { kind = OriginalSessionCommandKind.Move,
                actorEntityId = intent.actor, x = preferred.x, y = preferred.y });
            if (result == OriginalSessionReplyCode.Accepted) { intent.target = target; queuedInteractions[intent.actor] = intent; }
            return result;
        }

        OriginalSessionReplyCode ApplyInteractionCommand(Player player, OriginalSessionCommand command)
        {
            if (command.kind == OriginalSessionCommandKind.InteractWell && command.itemInstanceId != 0)
                return OriginalSessionReplyCode.InvalidCommand;
            var intent = new QueuedInteraction { actor = command.actorEntityId == 0 ? OriginalWorld.HeroEntityId(player.slot) : command.actorEntityId,
                owner = player.slot, kind = command.kind, item = command.itemInstanceId };
            var valid = InteractionTarget(player, intent, out var actor, out var target, out double range);
            if (valid != OriginalSessionReplyCode.Accepted) return valid;
            if (SquaredDistance(actor.position, target) > range * range) return MoveForInteraction(player, intent, actor, target, range);
            var result = ExecuteInteraction(player, intent);
            if (result == OriginalSessionReplyCode.Accepted)
            { CancelQueuedInteraction(intent.actor); world.Stop(intent.actor); OnAcceptedWorldOrder(intent.actor); }
            return result;
        }

        void AdvanceQueuedInteractions()
        {
            if (world == null || !Started || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
            { queuedInteractions.Clear(); return; }
            if (pendingDuel) return;
            foreach (var id in new List<int>(queuedInteractions.Keys))
            {
                var intent = queuedInteractions[id]; var player = players.Find(p => p.slot == intent.owner);
                var current = world.UnitState(id);
                if (player == null || current == null || current.health <= 0)
                { CancelQueuedInteraction(id); continue; }
                if (current.paused || current.hidden) continue;
                var valid = InteractionTarget(player, intent, out var actor, out var target, out double range);
                if (valid != OriginalSessionReplyCode.Accepted)
                { CancelQueuedInteraction(id); world.Stop(id); continue; }
                if (SquaredDistance(actor.position, target) <= range * range)
                {
                    CancelQueuedInteraction(id); ExecuteInteraction(player, intent); world.Stop(id);
                    continue;
                }
                if (SquaredDistance(target, intent.target) > 1e-8 &&
                    MoveForInteraction(player, intent, actor, target, range) != OriginalSessionReplyCode.Accepted)
                { CancelQueuedInteraction(id); world.Stop(id); }
            }
        }
    }
}

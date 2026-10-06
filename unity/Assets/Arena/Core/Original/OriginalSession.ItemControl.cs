using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int, OriginalSessionCommand> pendingItemOrders = new Dictionary<int, OriginalSessionCommand>();
        bool ActorItemBlocked(int actor) => (ActorControlMask(actor) & OriginalActorControlMask.Item) != 0;
        bool ActorItemRejected(int actor) => bossDooms.ContainsKey(actor);

        // ITEMCTL1: stunned I03L accepts an order without immediate healing,
        // spell events or charge debit; it executes when BPSE expires. Sleep
        // accepts but remains dormant through the complete8s observation.
        // Using this order queue for other actives is an explicit host policy.
        void QueueItemOrder(Player player, OriginalSessionCommand command)
        {
            pendingItemOrders[OriginalWorld.HeroEntityId(player.slot)] = new OriginalSessionCommand {
                kind = OriginalSessionCommandKind.UseItem, actorEntityId = command.actorEntityId,
                bag = command.bag, itemSlot = command.itemSlot, itemInstanceId = command.itemInstanceId,
                targetItemInstanceId = command.targetItemInstanceId,
                targetKind = command.targetKind, targetId = command.targetId, x = command.x, y = command.y };
        }
        void AdvanceQueuedItems()
        {
            foreach (var actorId in new List<int>(pendingItemOrders.Keys))
            {
                var actor = world.UnitState(actorId);
                if (actor == null || actor.health <= .405)
                { pendingItemOrders.Remove(actorId); continue; }
                if (actor.paused || actor.hidden || ActorItemBlocked(actorId)) continue;
                var command = pendingItemOrders[actorId]; pendingItemOrders.Remove(actorId);
                var player = players.Find(p => OriginalWorld.HeroEntityId(p.slot) == actorId);
                if (player != null) ApplyCastCommand(player, command);
                // Revalidate the exact instance/slot/resources on execution.
                // Inventory changes can invalidate an order without any debit.
            }
        }
    }
}

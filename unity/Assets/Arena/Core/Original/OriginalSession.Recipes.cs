using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        string ChargedRecipeOffer(string id)
        {
            var definition = itemCatalog?.Item(id);
            if (definition == null || definition.classId != "Charged") return null;
            var link = itemCatalog.QuickBuy(id)?.linkedResultId;
            return link != null && itemCatalog.QuickBuy(link)?.linkedResultId != null ? link : null;
        }

        OriginalSessionReplyCode UseChargedRecipe(Player player, OriginalSessionCommand command, string offer)
        {
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            var candidate = player.inventory.Copy();
            // FS8472 performs the same price/ingredient operation regardless
            // of Ix's quick-buy toggle. These recipe objects have zero native
            // charges and survive activation; the script removes ingredients.
            var action = candidate.TryQuickBuy(offer, command.bag, fromChargedItemUse: true);
            if (!action.Applied) return RejectItem(player, action.Code);
            if (action.Code == OriginalItemActionCode.Grounded && groundItems.Count >= 8192)
                return RejectItem(player, OriginalItemActionCode.NoSpace);
            if (!ApplyEquipmentProfile(player, candidate, actor)) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
            player.inventory = candidate;
            if (action.Code == OriginalItemActionCode.Grounded)
                groundItems.Add(action.Item.instanceId, new OriginalGroundItemView { item = CopyItem(action.Item), position = actor.position });
            SyncItemScriptInventory(player);
            player.lastItemAction = action.Code;
            // Charged recipe's A0KD is explicitly excluded by Kz from curse.
            NotifyNativeSpellEffect(actor.entityId, "A0KD");
            return OriginalSessionReplyCode.Accepted;
        }
    }
}

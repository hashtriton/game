using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // CASTAPPROACH1 observes the two wards and I06J walking before effect
        // and debit. Other supported item families retain their own existing
        // target rules; this preflight skips only distance and commits nothing.
        OriginalSessionReplyCode PlanItemCastApproach(Player player, OriginalSessionCommand command,
            out CastApproachTarget target)
        {
            target = null;
            if (player == null || command == null || command.kind != OriginalSessionCommandKind.UseItem ||
                !Enum.IsDefined(typeof(OriginalInventoryBag), command.bag) || command.itemSlot < 0 || command.itemSlot >= 6 ||
                command.itemInstanceId <= 0 || command.targetItemInstanceId < 0 ||
                !Enum.IsDefined(typeof(OriginalWorldTargetKind), command.targetKind) ||
                command.actorEntityId != 0 && command.actorEntityId != OriginalWorld.HeroEntityId(player.slot))
                return OriginalSessionReplyCode.InvalidCommand;
            if (!Started || world == null || player.inventory == null || pendingDuel ||
                match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            int actorId = OriginalWorld.HeroEntityId(player.slot);
            var actor = world.UnitState(actorId);
            if (actor == null || actor.kind != OriginalWorldUnitKind.Hero || actor.ownerSlot != player.slot)
                return OriginalSessionReplyCode.InvalidCommand;
            var slots = command.bag == OriginalInventoryBag.Hero ? player.inventory.HeroSlots : player.inventory.ServantSlots;
            var held = slots[command.itemSlot];
            if (held == null || held.instanceId != command.itemInstanceId || held.ownerId != 0 && held.ownerId != player.slot)
                return OriginalSessionReplyCode.InvalidCommand;
            try
            {
                var view = Array.Find(ItemUseViews(player), item => item.bag == command.bag && item.slot == command.itemSlot &&
                    item.instanceId == command.itemInstanceId);
                if (view == null) return OriginalSessionReplyCode.InvalidCommand;
                if (view.code == OriginalItemUseCode.RuleUnavailable || view.code == OriginalItemUseCode.Unsupported)
                    return OriginalSessionReplyCode.RuleUnavailable;
                if (view.code != OriginalItemUseCode.Ready) return OriginalSessionReplyCode.NotReady;
                if (!view.implemented) return OriginalSessionReplyCode.RuleUnavailable;
                var summon = activeItemRules?.Summon(view.itemId);
                if (command.targetItemInstanceId != 0 && view.itemId != "I0B7" && (summon == null || !summon.pointTarget))
                    return OriginalSessionReplyCode.InvalidCommand;
                if (summon != null)
                {
                    var code = ResolveSummonItemTarget(player, command, summon, out var point, false);
                    if (code != OriginalSessionReplyCode.Accepted) return code;
                    if (!summon.pointTarget || command.targetItemInstanceId != 0) return OriginalSessionReplyCode.Accepted;
                    target = new CastApproachTarget { actor = actorId, point = point, range = summon.castRange,
                        unitTarget = command.targetKind == OriginalWorldTargetKind.Unit ? command.targetId : 0 };
                }
                else if (view.itemId == "I0B7")
                {
                    if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0)
                        return OriginalSessionReplyCode.InvalidCommand;
                    if (command.targetItemInstanceId == 0 || Array.Exists(player.inventory.HeroSlots,
                        item => item != null && item.instanceId == command.targetItemInstanceId))
                        return OriginalSessionReplyCode.Accepted;
                    if (!groundItems.TryGetValue(command.targetItemInstanceId, out var ground) || ground.item == null ||
                        !ValidPoint(ground.position.x, ground.position.y)) return OriginalSessionReplyCode.InvalidCommand;
                    // uq accepts ground items without an ownership check. Its
                    // existing DisassembleItem consumer enforces source range700.
                    target = new CastApproachTarget { actor = actorId, itemTarget = command.targetItemInstanceId,
                        point = ground.position, range = 700 };
                }
                else
                {
                    OriginalSessionReplyCode code;
                    double range;
                    OriginalAbilityTargetMode mode;
                    var channel = ItemChannelRule(view.itemId);
                    var image = ItemImageRule(view.itemId);
                    var native = NativeItemAction(view.itemId);
                    var script = ItemSummonScriptRule(view.itemId);
                    if (channel != null)
                    { code = ValidateItemChannelTarget(player, command, channel, false); range = channel.range; mode = channel.targetMode; }
                    else if (image != null)
                    { code = ValidateItemImageTarget(player, command, image, false); range = image.range; mode = OriginalAbilityTargetMode.Unit; }
                    else if (native != null)
                    { code = ValidateNativeItemTarget(player, command, native, false); range = native.range; mode = native.targetMode; }
                    else if (script != null)
                    { code = ValidateItemSummonScriptTarget(player, command, script, false); range = script.range; mode = script.targetMode; }
                    else
                        return command.targetKind == OriginalWorldTargetKind.None && command.targetId == 0 && command.targetItemInstanceId == 0
                            ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.InvalidCommand;
                    if (code != OriginalSessionReplyCode.Accepted) return code;
                    if (mode == OriginalAbilityTargetMode.None) return OriginalSessionReplyCode.Accepted;
                    var victim = mode == OriginalAbilityTargetMode.Unit ? world.UnitState(command.targetId) : null;
                    target = new CastApproachTarget { actor = actorId, unitTarget = victim == null ? 0 : victim.entityId,
                        point = victim == null ? new OriginalPoint(command.x, command.y) : victim.position, range = range };
                }
                if (!OriginalCombatDefinition.IsFinite(target.range) || target.range <= 0)
                { target = null; return OriginalSessionReplyCode.RuleUnavailable; }
                return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { target = null; return OriginalSessionReplyCode.RuleUnavailable; }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemUseRules consumableRules;
        readonly Dictionary<string, double> itemCooldowns = new Dictionary<string, double>(StringComparer.Ordinal);

        void ConfigureItemUse(OriginalObservedItemUse observations)
        {
            if (Started) throw new InvalidOperationException("Configure item use before starting a match.");
            consumableRules = observations == null ? null : new OriginalItemUseRules(itemCatalog, combatCatalog, observations);
        }

        OriginalItemUseView[] ItemUseViews(Player player)
        {
            if (player.inventory == null) return Array.Empty<OriginalItemUseView>();
            var result = new List<OriginalItemUseView>();
            var actor = world?.UnitState(OriginalWorld.HeroEntityId(player.slot));
            var inventory = player.inventory.Snapshot();
            for (int bag = 0; bag < 2; bag++)
            {
                var slots = bag == 0 ? inventory.heroSlots : inventory.servantSlots;
                for (int slot = 0; slot < slots.Length; slot++)
                {
                    var item = slots[slot]; if (item == null) continue;
                    var rule = consumableRules?.Rule(item.itemId);
                    var armor = activeItemRules?.Armor(item.itemId);
                    var summon = activeItemRules?.Summon(item.itemId);
                    var script = scriptItemRules?.Rule(item.itemId);
                    var scriptedAct = scriptActRules?.Rule(item.itemId);
                    var nativeAction = NativeItemAction(item.itemId);
                    var channel = ItemChannelRule(item.itemId);
                    var image = ItemImageRule(item.itemId);
                    var fortitude = ItemFortitudeRule(item.itemId);
                    var exchange = ItemExchangeRule(item.itemId);
                    var summonScript = ItemSummonScriptRule(item.itemId);
                    bool modeItem = IsItemMode(item.itemId);
                    string recipe = ChargedRecipeOffer(item.itemId);
                    var view = new OriginalItemUseView { bag = (OriginalInventoryBag)bag, slot = slot,
                        instanceId = item.instanceId, itemId = item.itemId, implemented = rule != null && rule.known,
                        code = rule == null ? OriginalItemUseCode.Unsupported : rule.known ? OriginalItemUseCode.Ready : OriginalItemUseCode.RuleUnavailable };
                    if (armor != null) { view.implemented = armor.known; view.code = armor.known ? OriginalItemUseCode.Ready : OriginalItemUseCode.RuleUnavailable; }
                    if (summon != null) { view.implemented = summon.known; view.code = summon.known ? OriginalItemUseCode.Ready : OriginalItemUseCode.RuleUnavailable;
                        view.targetMode = summon.pointTarget ? OriginalAbilityTargetMode.UnitOrPoint : OriginalAbilityTargetMode.None; }
                    if (script != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    if (scriptedAct != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    if (nativeAction != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; view.targetMode = nativeAction.targetMode; }
                    if (channel != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; view.targetMode = channel.targetMode; }
                    if (image != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; view.targetMode = OriginalAbilityTargetMode.Unit; }
                    if (fortitude != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    if (exchange != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    if (summonScript != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; view.targetMode = summonScript.targetMode; }
                    if (modeItem) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    if (recipe != null) { view.implemented = true; view.code = OriginalItemUseCode.Ready; }
                    view.requiresCharge = (nativeAction == null || nativeAction.requiresCharge) && summonScript == null && exchange == null && fortitude == null && !modeItem && image == null && channel == null && scriptedAct == null && recipe == null &&
                        (armor == null || armor.retired) && (summon == null || summon.retired) && (script == null || script.retired);
                    if (rule != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, rule.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (armor != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, armor.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (summon != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, summon.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (script != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, script.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (scriptedAct != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, scriptedAct.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (nativeAction != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, nativeAction.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (channel != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, channel.cooldownGroup), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    else if (image != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, OriginalItemImageRules.AbilityId), out var end);
                        view.cooldownRemaining = Math.Max(0, end - ItemClock);
                    }
                    if (fortitude != null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot, fortitude.abilityId), out var fortitudeEnd);
                        view.cooldownRemaining = Math.Max(0, fortitudeEnd - ItemClock);
                    }
                    if(exchange!=null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot,exchange.abilityId),out var exchangeEnd);
                        view.cooldownRemaining=Math.Max(0,exchangeEnd-ItemClock);
                    }
                    if(summonScript!=null)
                    {
                        itemCooldowns.TryGetValue(ItemCooldownKey(player.slot,summonScript.cooldownGroup),out var summonScriptEnd);
                        view.cooldownRemaining=Math.Max(0,summonScriptEnd-ItemClock);
                    }
                    if (actor == null || !Started || pendingDuel || bag != 0 || actor.hidden ||
                        match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost) view.code = OriginalItemUseCode.NotReady;
                    else if (actor.health <= 0) view.code = OriginalItemUseCode.Dead;
                    else if (actor.paused) view.code = OriginalItemUseCode.Paused;
                    else if (ActorItemRejected(actor.entityId)) view.code = OriginalItemUseCode.NotReady;
                    else if (modeItem && OriginalItemModeRules.Family(item.itemId) == 2 && !warpathTriggersEnabled) view.code = OriginalItemUseCode.NotReady;
                    else if (!item.chargesKnown) view.code = OriginalItemUseCode.RuleUnavailable;
                    else if (item.charges <= 0 && view.requiresCharge) view.code = OriginalItemUseCode.NoCharges;
                    else if (armor != null && actor.mana < armor.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (summon != null && actor.mana < summon.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (script != null && actor.mana < script.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (scriptedAct != null && actor.mana < scriptedAct.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (channel != null && actor.mana < channel.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (image != null && actor.mana < image.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (nativeAction != null && actor.mana < nativeAction.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (exchange != null && actor.mana < exchange.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (summonScript != null && actor.mana < summonScript.manaCost) view.code = OriginalItemUseCode.NotReady;
                    else if (view.implemented && view.cooldownRemaining > 0) view.code = OriginalItemUseCode.Cooldown;
                    else if (view.implemented && rule != null && (rule.resource == "health" ? actor.health >= actor.profile.maxHealth : actor.mana >= actor.profile.maxMana))
                        view.code = OriginalItemUseCode.FullResource;
                    result.Add(view);
                }
            }
            return result.ToArray();
        }

        OriginalSessionReplyCode UseItem(Player player, OriginalSessionCommand command)
        {
            if (!Enum.IsDefined(typeof(OriginalInventoryBag), command.bag) || command.itemSlot < 0 || command.itemSlot >= 6 ||
                command.itemInstanceId <= 0 || command.targetItemInstanceId < 0 || !Enum.IsDefined(typeof(OriginalWorldTargetKind), command.targetKind) ||
                command.actorEntityId != 0 && command.actorEntityId != OriginalWorld.HeroEntityId(player.slot))
                return OriginalSessionReplyCode.InvalidCommand;
            var view = Array.Find(ItemUseViews(player), item => item.bag == command.bag && item.slot == command.itemSlot && item.instanceId == command.itemInstanceId);
            if (view == null) return RejectItem(player, OriginalItemActionCode.InvalidInstance);
            var channel = ItemChannelRule(view.itemId);
            var image = ItemImageRule(view.itemId);
            var nativeAction = NativeItemAction(view.itemId);
            var summonScript = ItemSummonScriptRule(view.itemId);
            var summon = activeItemRules?.Summon(view.itemId);
            if (channel == null && image == null && nativeAction == null && summonScript == null && (summon == null || !summon.pointTarget) && (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0)) return OriginalSessionReplyCode.InvalidCommand;
            if(summonScript!=null)
            {
                var targetCode=ValidateItemSummonScriptTarget(player,command,summonScript);
                if(targetCode!=OriginalSessionReplyCode.Accepted)return targetCode;
            }
            if(nativeAction!=null)
            {
                var targetCode=ValidateNativeItemTarget(player,command,nativeAction);
                if(targetCode!=OriginalSessionReplyCode.Accepted)return targetCode;
            }
            if (channel != null)
            {
                var targetCode = ValidateItemChannelTarget(player, command, channel);
                if (targetCode != OriginalSessionReplyCode.Accepted) return targetCode;
            }
            if (image != null)
            {
                var targetCode = ValidateItemImageTarget(player, command, image);
                if (targetCode != OriginalSessionReplyCode.Accepted) return targetCode;
            }
            if (summon != null)
            {
                var targetCode = ValidateSummonItemTarget(player, command, summon);
                if (targetCode != OriginalSessionReplyCode.Accepted) return targetCode;
            }
            if (command.targetItemInstanceId != 0 && view.itemId != "I0B7" && (summon == null || !summon.pointTarget)) return OriginalSessionReplyCode.InvalidCommand;
            if (view.code == OriginalItemUseCode.RuleUnavailable || view.code == OriginalItemUseCode.Unsupported)
                return RejectItem(player, OriginalItemActionCode.UnimplementedEffect);
            if (view.code != OriginalItemUseCode.Ready) return OriginalSessionReplyCode.NotReady;
            if (ActorItemBlocked(OriginalWorld.HeroEntityId(player.slot)))
            { QueueItemOrder(player, command); return OriginalSessionReplyCode.Accepted; }
            pendingItemOrders.Remove(OriginalWorld.HeroEntityId(player.slot));
            if (channel != null) return UseItemChannel(player, command, channel);
            if (image != null) return UseItemImage(player, command, image);
            if (summonScript != null) return UseItemSummonScript(player,command,summonScript);
            if (IsItemMode(view.itemId)) return UseItemMode(player,command,view.itemId);
            var fortitude = ItemFortitudeRule(view.itemId);
            if (fortitude != null) return UseItemFortitude(player,fortitude);
            var exchange=ItemExchangeRule(view.itemId);
            if(exchange!=null)return UseItemExchange(player,exchange);
            string recipe = ChargedRecipeOffer(view.itemId);
            if (recipe != null) return UseChargedRecipe(player, command, recipe);
            var script = scriptItemRules?.Rule(view.itemId);
            if (script != null) return UseScriptItem(player, command, view, script);
            var scriptedAct = scriptActRules?.Rule(view.itemId);
            if (scriptedAct != null) return UseScriptActItem(player, scriptedAct);
            var armor = activeItemRules?.Armor(view.itemId);
            if (armor != null) return UseArmorItem(player, command, view, armor);
            if (summon != null) return UseSummonItem(player, command, view, summon);
            if (nativeAction != null) return UseNativeItemAction(player, command, view, nativeAction);
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            var candidate = player.inventory.Copy();
            var action = candidate.ConsumeCharge(command.bag, command.itemSlot, command.itemInstanceId);
            if (!action.Applied) return RejectItem(player, action.Code);
            var rule = consumableRules.Rule(view.itemId);
            try
            {
                var prepared = PrepareEquipmentProfile(player, candidate, actor);
                if (rule.resource == "health") prepared.health = Math.Min(prepared.profile.maxHealth, prepared.health + rule.amount);
                else prepared.mana = Math.Min(prepared.profile.maxMana, prepared.mana + rule.amount);
                if (!CommitEquipmentProfile(prepared)) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                player.inventory = candidate;
                itemCooldowns[ItemCooldownKey(player.slot, rule.cooldownGroup)] = ItemClock + rule.cooldown;
                player.lastItemAction = OriginalItemActionCode.Success;
            }
            catch (InvalidOperationException) { return RejectItem(player, OriginalItemActionCode.UnresolvedRule); }
            // ITEMFAM3 H024/I03L/I03M: accepted native use emits SPELL_EFFECT
            // in the same callback. Full/cooldown rejection emits none. Szv
            // advances already flying spheres exactly once for that event.
            // Dispatch after the committed item state, outside rejection catch.
            NotifyNativeSpellEffect(actor.entityId, rule.abilityId);
            return OriginalSessionReplyCode.Accepted;
        }

        static string ItemCooldownKey(int slot, string group) => slot + ":" + group;
    }
}


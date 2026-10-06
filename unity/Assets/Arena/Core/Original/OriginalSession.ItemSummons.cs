using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemSummonState { internal double remaining; internal bool deathReported; }
        readonly Dictionary<int, ItemSummonState> itemSummons = new Dictionary<int, ItemSummonState>();
        int nextItemSummonId = OriginalWorld.FirstSummonEntityId;

        OriginalSessionReplyCode ValidateSummonItemTarget(Player player, OriginalSessionCommand command, OriginalItemSummonRule rule, bool enforceRange = true) =>
            ResolveSummonItemTarget(player, command, rule, out _, enforceRange);

        OriginalSessionReplyCode ResolveSummonItemTarget(Player player, OriginalSessionCommand command, OriginalItemSummonRule rule,
            out OriginalPoint point, bool enforceRange = true)
        {
            point = default;
            if (!rule.pointTarget)
                return command.targetKind == OriginalWorldTargetKind.None && command.targetId == 0 && command.targetItemInstanceId == 0
                    ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.InvalidCommand;
            var actor = world?.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null) return OriginalSessionReplyCode.NotReady;
            if (command.targetItemInstanceId != 0)
            {
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0 || !ValidPoint(command.x, command.y))
                    return OriginalSessionReplyCode.InvalidCommand;
                // Acv tests the matching target-item type. Restrict item
                // references to the owner's inventory, as a host policy.
                var inventory = player.inventory.Snapshot();
                foreach (var bag in new[] { inventory.heroSlots, inventory.servantSlots })
                    foreach (var item in bag)
                        if (item != null && item.instanceId == command.targetItemInstanceId &&
                            (item.ownerId == 0 || item.ownerId == player.slot) && item.itemId == rule.itemId)
                        { point = actor.position; return OriginalSessionReplyCode.Accepted; }
                return OriginalSessionReplyCode.InvalidCommand;
            }
            if (command.targetKind == OriginalWorldTargetKind.Unit)
            {
                if (command.targetId <= 0) return OriginalSessionReplyCode.InvalidCommand;
                var victim = world.UnitState(command.targetId);
                // CASTAPPROACH1 accepts allied/enemy Unit targets and supplies
                // GetSpellTargetX/Y. Visibility/liveness is host admission.
                if (victim == null || victim.health <= .405 || !CanSeeForCombat(player.slot, victim))
                    return OriginalSessionReplyCode.NotReady;
                point = victim.position;
            }
            else
            {
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0 || !ValidPoint(command.x, command.y))
                    return OriginalSessionReplyCode.InvalidCommand;
                point = new OriginalPoint(command.x, command.y);
            }
            return !enforceRange || SquaredDistance(actor.position, point) <= rule.castRange * rule.castRange
                ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.NotReady;
        }

        OriginalSessionReplyCode UseSummonItem(Player player, OriginalSessionCommand command, OriginalItemUseView view, OriginalItemSummonRule rule)
        {
            if (!rule.known) return RejectItem(player, OriginalItemActionCode.UnimplementedEffect);
            var targetCode = ResolveSummonItemTarget(player, command, rule, out var center);
            if (targetCode != OriginalSessionReplyCode.Accepted) return targetCode;
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor.mana < rule.manaCost || actor.health <= .405) return OriginalSessionReplyCode.NotReady;
            var candidate = player.inventory.Copy();
            if (rule.retired)
            {
                var consumed = candidate.ConsumeCharge(command.bag, command.itemSlot, command.itemInstanceId);
                if (!consumed.Applied) return RejectItem(player, consumed.Code);
            }
            PreparedEquipmentProfile prepared;
            OriginalWorldSummonSpawn[] rows;
            if (!rule.pointTarget) center = actor.position;
            try
            {
                prepared = PrepareEquipmentProfile(player, candidate, actor);
                if (prepared.mana < rule.manaCost) return OriginalSessionReplyCode.NotReady;
                prepared.mana -= rule.manaCost;
                rows = new OriginalWorldSummonSpawn[rule.units.Length];
                if (nextItemSummonId > OriginalWorld.LastSummonEntityId - rows.Length + 1)
                    return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                for (int i = 0; i < rows.Length; i++)
                {
                    var profile = rule.profiles[i].Copy();
                    if (!FindItemSummonPoint(center, profile.collisionRadius, rows, i, out var point))
                        return OriginalSessionReplyCode.NotReady;
                    rows[i] = new OriginalWorldSummonSpawn { entityId = nextItemSummonId + i, ownerSlot = player.slot,
                        sourceHeroEntityId = actor.entityId, rawcode = rule.units[i], profile = profile, position = point,
                        health = profile.maxHealth, mana = profile.maxMana,
                        invulnerable = Array.IndexOf(combatCatalog.Unit(rule.units[i]).Text("abilList").Split(','), "Avul") >= 0 };
                }
                if (!world.TryPublishSummons(rows, new OriginalWorldProfileUpdate { entityId = actor.entityId,
                    profile = prepared.profile, health = prepared.health, mana = prepared.mana }))
                    return OriginalSessionReplyCode.NotReady;
            }
            catch (InvalidOperationException) { return RejectItem(player, OriginalItemActionCode.UnresolvedRule); }
            // The world and hero profile are already committed atomically.
            // Only prevalidated private metadata follows, without callbacks.
            CommitEquipmentProfile(prepared, profileAlreadyPublished: true);
            foreach (var row in rows) itemSummons.Add(row.entityId, new ItemSummonState { remaining = rule.duration });
            nextItemSummonId += rows.Length;
            player.inventory = candidate;
            itemCooldowns[ItemCooldownKey(player.slot, rule.cooldownGroup)] = ItemClock + rule.cooldown;
            player.lastItemAction = OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(actor.entityId, rule.abilityId);
            return OriginalSessionReplyCode.Accepted;
        }

        bool FindItemSummonPoint(OriginalPoint center, double radius, OriginalWorldSummonSpawn[] staged, int count, out OriginalPoint point)
        {
            // Native private placement is not exported by ITEMACT2. Bounded
            // deterministic rings preserve collision and all-or-none spawning.
            for (int ring = 0; ring <= 32; ring++)
                for (int step = 0; step < (ring == 0 ? 1 : 16); step++)
                {
                    double angle = step * Math.PI / 8;
                    var wanted = new OriginalPoint(center.x + ring * 16 * Math.Cos(angle), center.y + ring * 16 * Math.Sin(angle));
                    if (!world.TryFindFreeSpawn(wanted, radius, 0, out point)) continue;
                    bool occupied = false;
                    for (int i = 0; i < count; i++)
                    {
                        double combined = radius + staged[i].profile.collisionRadius;
                        if (SquaredDistance(point, staged[i].position) < combined * combined) { occupied = true; break; }
                    }
                    if (!occupied) return true;
                }
            point = center; return false;
        }

        void AdvanceItemSummons(double seconds)
        {
            foreach (var entry in new List<KeyValuePair<int, ItemSummonState>>(itemSummons))
            {
                var unit = world.UnitState(entry.Key);
                if (unit == null) { itemSummons.Remove(entry.Key); continue; }
                if (unit.health <= 0) { ObserveItemSummonDeath(entry.Key); continue; }
                if (unit.paused) continue;
                entry.Value.remaining -= seconds;
                if (entry.Value.remaining > 1e-9) continue;
                if (world.ForceUnitDeath(entry.Key)) { OnPyroUnitDied(entry.Key); ObserveItemSummonDeath(entry.Key); }
            }
        }
        void ObserveItemSummonDeath(int id)
        {
            if (!itemSummons.TryGetValue(id, out var state) || state.deathReported) return;
            state.deathReported = true;
            // Native timed-life and ordinary summon deaths reach global CA,
            // but are neither counted wave victims nor hero bounty/XP awards.
            ObserveScriptedHelperDeath();
        }
        void ObserveItemSummonWorldEvent(OriginalWorldEvent item)
        {
            if (item.kind == OriginalWorldEventKind.UnitDied) ObserveItemSummonDeath(item.entityId);
            else if (item.kind == OriginalWorldEventKind.UnitRemoved) itemSummons.Remove(item.entityId);
        }
    }
}

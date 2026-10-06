using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PickupPlayerChange
        {
            internal Player player;
            internal OriginalInventory originalInventory, inventory;
            internal OriginalHeroProgression originalProgression, progression;
            internal OriginalHeroStatsSnapshot stats;
            internal PreparedEquipmentProfile profile;
            internal bool inventoryChanged, experienceChanged;
            internal PermanentItemAttributes permanentAttributes;
        }
        sealed class PickupTransaction
        {
            internal bool handled, committed;
            internal OriginalItemActionCode code;
            internal uint randomBefore, randomAfter;
            internal long worldRevision;
            internal readonly List<PickupPlayerChange> players = new List<PickupPlayerChange>();
            internal readonly List<OriginalGroundItemView> ground = new List<OriginalGroundItemView>();
            internal readonly List<PreparedEquipmentProfile> resources = new List<PreparedEquipmentProfile>();
            internal readonly List<int> hasteRecipients = new List<int>();
            internal double hasteDuration,hasteBonus;
            internal int runeVampireActor;
            internal readonly List<int> runeDispelTargets=new List<int>();
            internal readonly List<int> runeDamageTargets=new List<int>();
        }
        uint pickupRandom;

        // Called after the command handler validates proximity, ownership and
        // stock, and debits a detached buyer candidate. C6 handles powerups
        // before ordinary inventory insertion. E6 removes the used powerup;
        // no powerup is retained as a seventh inventory item here.
        PickupTransaction PreparePowerupPickup(Player player, OriginalWorldUnitView actor, OriginalInventory candidate, string itemId)
        {
            var result = new PickupTransaction { code = OriginalItemActionCode.Success, randomBefore = pickupRandom,
                randomAfter = pickupRandom, worldRevision = world.Snapshot().revision };
            if (actor == null || actor.kind != OriginalWorldUnitKind.Hero || actor.ownerSlot != player.slot ||
                actor.entityId != OriginalWorld.HeroEntityId(player.slot) || candidate == null || candidate.OwnerId != player.slot)
                throw new ArgumentException("Powerup actor/candidate ownership mismatch.");
            if (PrepareNativePowerupPickup(result, player, actor, candidate, itemId)) return result;
            var context = new OriginalItemPickupContext { actor = PickupActor(actor, actor.ownerSlot), wave = match.Round };
            foreach (var item in candidate.HeroSlots) if (item != null) context.actorInventoryCount++;
            var nearby = new List<OriginalItemPickupActor>();
            foreach (var unit in world.Snapshot().units) nearby.Add(PickupActor(unit, actor.ownerSlot));
            context.nearbyUnits = nearby.ToArray();
            int? draw = null;
            if (itemId == "I05F" || itemId == "I07W" || itemId == "I0AI" || itemId == "I05W" || itemId == "I0AT")
            {
                int minimum = itemId == "I05F" ? 200 : itemId == "I07W" ? 20 : itemId == "I0AI" ? 2 : 1;
                int maximum = itemId == "I05F" ? 700 : itemId == "I07W" ? 70 : itemId == "I0AT" ? OriginalItemFatePool.TotalWeight : 7;
                uint state = pickupRandom == 0 ? unchecked((uint)seed) ^ 0x5049434bu : pickupRandom;
                if (state == 0) state = 0x6D2B79F5u;
                // Host deterministic stream, not a claim to reproduce Warcraft's
                // private RNG sequence. Rejection sampling avoids modulo bias.
                uint size = (uint)(maximum - minimum + 1), threshold = unchecked(0u - size) % size;
                do { state ^= state << 13; state ^= state >> 17; state ^= state << 5; } while (state < threshold);
                draw = minimum + (int)(state % size); result.randomAfter = state;
            }
            var plan = OriginalItemPickupEffects.Plan(itemId, context, draw);
            result.handled = plan.handled;
            if (!plan.handled) return result;
            if (!plan.complete) { result.code = OriginalItemActionCode.UnimplementedEffect; return result; }
            var changes = new SortedDictionary<int, PickupPlayerChange>();
            PickupPlayerChange Change(int slot)
            {
                if (changes.TryGetValue(slot, out var found)) return found;
                var recipient = players.Find(p => p.slot == slot);
                if (recipient == null || recipient.inventory == null) throw new InvalidOperationException("Pickup recipient has no inventory.");
                found = new PickupPlayerChange { player = recipient, originalInventory = recipient.inventory,
                    inventory = slot == player.slot ? candidate : recipient.inventory.Copy(),
                    originalProgression = recipient.progression, stats = recipient.stats };
                changes.Add(slot, found); return found;
            }
            try
            {
                Change(player.slot); // Retain an already validated purchase debit.
                foreach (var mutation in plan.mutations)
                {
                    var change = Change(mutation.ownerSlot);
                    switch (mutation.kind)
                    {
                        case OriginalPickupMutationKind.Gold: change.inventory.GrantResources(mutation.amount, 0); break;
                        case OriginalPickupMutationKind.Lumber: change.inventory.GrantResources(0, mutation.amount); break;
                        case OriginalPickupMutationKind.Experience:
                            if (mutation.entityId != OriginalWorld.HeroEntityId(mutation.ownerSlot) || change.originalProgression == null)
                                throw new InvalidOperationException("Pickup experience recipient has no canonical progression.");
                            if (change.progression == null) change.progression = change.originalProgression.Copy();
                            change.progression.GrantExperience(mutation.amount); change.experienceChanged = true; break;
                        case OriginalPickupMutationKind.CreateGroundItem:
                        case OriginalPickupMutationKind.CreateInventoryItem:
                            var item = change.inventory.CreateInstance(mutation.itemId, mutation.ownerSlot);
                            if (mutation.kind == OriginalPickupMutationKind.CreateInventoryItem)
                            {
                                var action = change.inventory.TryPickup(item);
                                if (!action.Applied) { result.code = action.Code; return result; }
                                change.inventoryChanged = true;
                                if (action.Code != OriginalItemActionCode.Grounded) break;
                                item = action.Item;
                            }
                            result.ground.Add(new OriginalGroundItemView { item = CopyItem(item), position = new OriginalPoint(mutation.x, mutation.y) });
                            break;
                        default: throw new InvalidOperationException("Unsupported pickup mutation.");
                    }
                }
                if (groundItems.Count + result.ground.Count > 8192) { result.code = OriginalItemActionCode.NoSpace; return result; }
                foreach (var change in changes.Values)
                {
                    var unit = world.UnitState(OriginalWorld.HeroEntityId(change.player.slot));
                    if (unit == null) throw new InvalidOperationException("Pickup recipient world actor is missing.");
                    if (change.inventoryChanged) change.profile = PrepareEquipmentProfile(change.player, change.inventory, unit);
                    if (change.experienceChanged)
                    {
                        change.stats = CalculateProgressionStats(change.player.hero, change.progression);
                        var stats = ComposeEquipment(change.stats, change.inventory);
                        var profile = unit.profile.Copy();
                        profile.maxHealth = stats.maxHealth.Require(); profile.maxMana = stats.maxMana.Require();
                        profile.moveSpeed = ResolveAbilityMoveSpeed(change.player.slot, stats.baseMoveSpeed, change.progression);
                        string policy = null;
                        if (unit.profile.maxHealth != profile.maxHealth || unit.profile.maxMana != profile.maxMana)
                        {
                            var observed = progressionObserved.VitalityChange(change.player.hero, "level-up");
                            if (!observed.policyKnown) throw new InvalidOperationException(observed.policy);
                            policy = observed.policy;
                        }
                        change.profile = new PreparedEquipmentProfile { entityId = unit.entityId, profile = profile,
                            health = UpdatedVitality(unit.health, unit.profile.maxHealth, profile.maxHealth, policy, true),
                            mana = UpdatedVitality(unit.mana, unit.profile.maxMana, profile.maxMana, policy, false) };
                    }
                    if (change.profile != null && ((unit.health > 0) != (change.profile.health > 0) ||
                        unit.profile.collisionRadius != change.profile.profile.collisionRadius))
                        throw new InvalidOperationException("Pickup cannot change residency or life state through a profile update.");
                    result.players.Add(change);
                }
            }
            catch (InvalidOperationException) { result.code = OriginalItemActionCode.UnresolvedRule; }
            catch (OverflowException) { result.code = OriginalItemActionCode.NotAllowed; }
            return result;
        }

        OriginalItemPickupActor PickupActor(OriginalWorldUnitView unit, int picker) => new OriginalItemPickupActor
        {
            entityId = unit.entityId, ownerSlot = unit.ownerSlot, rawcode = unit.rawcode,
            hero = unit.kind == OriginalWorldUnitKind.Hero || unit.kind == OriginalWorldUnitKind.Illusion,
            illusion = unit.kind == OriginalWorldUnitKind.Illusion, alliedToActor = unit.ownerSlot > 0 && !AreEnemies(picker, unit.ownerSlot),
            health = unit.health, x = unit.position.x, y = unit.position.y
        };

        void CommitPowerupPickup(PickupTransaction transaction)
        {
            if (transaction == null || !transaction.handled || transaction.committed || transaction.code != OriginalItemActionCode.Success ||
                pickupRandom != transaction.randomBefore || world.Snapshot().revision != transaction.worldRevision)
                throw new InvalidOperationException("Powerup transaction is stale, incomplete or already committed.");
            foreach (var change in transaction.players)
                if (!ReferenceEquals(change.player.inventory, change.originalInventory) ||
                    !ReferenceEquals(change.player.progression, change.originalProgression))
                    throw new InvalidOperationException("Powerup recipient changed before commit.");
            foreach (var ground in transaction.ground)
                if (groundItems.ContainsKey(ground.item.instanceId)) throw new InvalidOperationException("Duplicate spawned item identity.");
            // A synchronous host transaction has prevalidated each actor and
            // unchanged collision radius. These updates cannot need placement.
            foreach (var change in transaction.players)
                if (change.profile != null && !CommitEquipmentProfile(change.profile))
                    throw new InvalidOperationException("Prevalidated pickup profile commit failed.");
            foreach (var profile in transaction.resources)
                if (!CommitEquipmentProfile(profile)) throw new InvalidOperationException("Prevalidated rune resource commit failed.");
            foreach (var change in transaction.players)
            {
                change.player.inventory = change.inventory;
                if (change.experienceChanged) { change.player.progression = change.progression; change.player.stats = change.stats; }
                if (change.permanentAttributes != null) itemPermanentAttributes[change.player.slot] = change.permanentAttributes;
            }
            foreach (var ground in transaction.ground) groundItems.Add(ground.item.instanceId, ground);
            foreach (int actor in transaction.hasteRecipients) ApplyItemHaste(actor,transaction.hasteDuration,transaction.hasteBonus);
            pickupRandom = transaction.randomAfter; transaction.committed = true;
            CommitRunePowerupPickup(transaction);
        }
    }
}

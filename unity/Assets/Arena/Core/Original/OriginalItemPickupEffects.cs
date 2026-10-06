using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalPickupMutationKind { Experience, Gold, Lumber, CreateInventoryItem, CreateGroundItem }

    public sealed class OriginalItemPickupActor
    {
        public int entityId, ownerSlot;
        public string rawcode;
        public bool hero, illusion, alliedToActor;
        public double health, x, y;
    }

    public sealed class OriginalItemPickupContext
    {
        public OriginalItemPickupActor actor;
        // The host supplies the complete enumeration, in its deterministic order.
        public OriginalItemPickupActor[] nearbyUnits = Array.Empty<OriginalItemPickupActor>();
        public int wave, actorInventoryCount;
    }

    public sealed class OriginalPickupMutation
    {
        public OriginalPickupMutationKind kind;
        public int entityId, ownerSlot, amount;
        public string itemId;
        public double x, y;
    }

    public sealed class OriginalItemPickupPlan
    {
        public bool handled, complete;
        public string unresolved;
        public OriginalPickupMutation[] mutations = Array.Empty<OriginalPickupMutation>();
        public string source = "scripts/war3map.j:C6:20693-20786; pickup registration:85415-85419";
    }

    // Only C6's pickup-trigger mutations. Native item consumption, another
    // trigger, inventory conversion/crafting and charge behavior are separate.
    // The optional draw is sampled by the host's candidate RNG, not the client.
    // Planning never advances RNG or publishes partial currency/XP/item state.
    public static class OriginalItemPickupEffects
    {
        static readonly string[] RandomItems = { "I069", "I095", "I06K", "I06H", "I07F", "I07L", "I01C" };

        public static OriginalItemPickupPlan Plan(string itemId, OriginalItemPickupContext context, int? randomDraw = null)
        {
            if (!Rawcode(itemId)) throw new ArgumentException("Invalid item rawcode.");
            Validate(context);
            var result = new OriginalItemPickupPlan();
            bool random = itemId == "I05F" || itemId == "I07W" || itemId == "I0AI" || itemId == "I05W" || itemId == "I0AT";
            if (!random && itemId != "I05T" && itemId != "I05U" && itemId != "I0AT") return result;
            result.handled = true;
            if (itemId == "I05F" && !context.actor.hero)
            { result.unresolved = "native-AddHeroXP-on-nonhero-unresolved"; return result; }
            int roll = 0;
            if (random)
            {
                if (!randomDraw.HasValue) { result.unresolved = "host-random-draw-required"; return result; }
                int min = itemId == "I05F" ? 200 : itemId == "I07W" ? 20 : itemId == "I0AI" ? 2 : 1;
                int max = itemId == "I05F" ? 700 : itemId == "I07W" ? 70 : itemId == "I0AT" ? OriginalItemFatePool.TotalWeight : 7;
                roll = randomDraw.Value;
                if (roll < min || roll > max) throw new ArgumentOutOfRangeException(nameof(randomDraw));
            }
            var actor = context.actor;
            var mutations = new List<OriginalPickupMutation>();
            if (itemId == "I05T" || itemId == "I05U")
            {
                foreach (var target in context.nearbyUnits)
                {
                    double dx = target.x - actor.x, dy = target.y - actor.y;
                    if (dx * dx + dy * dy > 900 * 900 || target.health <= .405 || !target.hero || !target.alliedToActor || target.illusion ||
                        target.rawcode == "U00T" || target.rawcode == "E00E" || target.rawcode == "E00J" || target.rawcode == "O00D") continue;
                    if (target.ownerSlot == 0)
                    { result.unresolved = "eligible-recipient-owner-not-in-session"; return result; }
                    mutations.Add(new OriginalPickupMutation { kind = itemId == "I05T" ? OriginalPickupMutationKind.Experience : OriginalPickupMutationKind.Gold,
                        entityId = target.entityId, ownerSlot = target.ownerSlot, amount = (itemId == "I05T" ? 25 : 10) * context.wave });
                }
            }
            else if (itemId == "I05W" || itemId == "I0AT")
                mutations.Add(new OriginalPickupMutation { kind = context.actorInventoryCount < 6 ? OriginalPickupMutationKind.CreateInventoryItem : OriginalPickupMutationKind.CreateGroundItem,
                    entityId = actor.entityId, ownerSlot = actor.ownerSlot, itemId = itemId == "I0AT" ? OriginalItemFatePool.At(roll) : RandomItems[roll - 1], x = actor.x, y = actor.y });
            else
                mutations.Add(new OriginalPickupMutation { kind = itemId == "I05F" ? OriginalPickupMutationKind.Experience :
                    itemId == "I07W" ? OriginalPickupMutationKind.Gold : OriginalPickupMutationKind.Lumber,
                    entityId = actor.entityId, ownerSlot = actor.ownerSlot, amount = roll });
            result.complete = true;
            result.mutations = mutations.ToArray();
            return result;
        }

        static void Validate(OriginalItemPickupContext context)
        {
            if (context == null || context.wave < 0 || context.wave > 30 || context.actorInventoryCount < 0 || context.actorInventoryCount > 6 || context.nearbyUnits == null)
                throw new ArgumentException("Invalid pickup context.");
            Actor(context.actor, true);
            var ids = new HashSet<int>();
            foreach (var target in context.nearbyUnits)
            {
                Actor(target, false);
                if (!ids.Add(target.entityId)) throw new ArgumentException("Duplicate pickup recipient.");
            }
        }

        static void Actor(OriginalItemPickupActor value, bool picker)
        {
            if (value == null || value.entityId <= 0 || value.ownerSlot < (picker ? 1 : 0) || value.ownerSlot > 8 || !Rawcode(value.rawcode) ||
                !Finite(value.health) || value.health < 0 || !Finite(value.x) || !Finite(value.y) || Math.Abs(value.x) > 1048576 || Math.Abs(value.y) > 1048576)
                throw new ArgumentException("Invalid pickup actor.");
        }
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static bool Rawcode(string value)
        {
            if (value == null || value.Length != 4) return false;
            foreach (var c in value) if (c < 32 || c > 126) return false;
            return true;
        }
    }
}

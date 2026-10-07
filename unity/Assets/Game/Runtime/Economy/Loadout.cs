using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Game
{
    public enum BuyOutcome { Bought, NotEnoughGold, NotEnoughSouls, NoSpace, Unavailable }

    /// <summary>
    /// The hero's purse and bag. Purchases, recipes and sales run through the inventory rules of the original map;
    /// what the bag holds is turned into the hero's stat bonuses after every change.
    /// </summary>
    public sealed class Loadout
    {
        private readonly ItemBook book;
        private readonly Unit hero;
        private OriginalInventory inventory;
        private bool warnedAboutEffects;

        public event Action Changed;

        public Loadout(ItemBook book, Unit hero, long gold, long souls)
        {
            this.book = book;
            this.hero = hero;
            inventory = new OriginalInventory(book.Items, 1, gold, souls, null, book.Rules);
            ApplyStats();
        }

        public long Gold => inventory.Gold;
        public long Souls => inventory.Lumber;
        public int SlotCount => OriginalInventory.SlotsPerBag;

        /// <summary>The six hero slots, empty ones are null. A copy: changing it changes nothing.</summary>
        public OriginalItemInstance[] Slots => inventory.HeroSlots;

        public int FreeSlots
        {
            get
            {
                var free = 0;
                foreach (var slot in inventory.HeroSlots)
                    if (slot == null) free++;
                return free;
            }
        }

        public List<string> OwnedIds()
        {
            var ids = new List<string>();
            foreach (var slot in inventory.HeroSlots)
                if (slot != null) ids.Add(slot.itemId);
            return ids;
        }

        public void Grant(long gold, long souls = 0)
        {
            inventory.GrantResources(gold, souls);
            Changed?.Invoke();
        }

        public PurchasePlan PlanFor(string finalId, bool another = false) => ShoppingPlanner.Plan(book, inventory.HeroSlots, finalId, another);

        /// <summary>Buys one shelf item. Nothing changes unless the purchase goes through completely.</summary>
        public BuyOutcome Buy(ShopEntry entry)
        {
            // Work on a copy: with a full bag the original rules drop the purchase on the ground, which the shop must not do.
            var candidate = inventory.Copy();
            var outcome = BuyOn(candidate, entry);
            if (outcome != BuyOutcome.Bought) return outcome;

            inventory = candidate;
            ApplyStats();
            Changed?.Invoke();
            return BuyOutcome.Bought;
        }

        /// <summary>
        /// Carries out a plan step by step. Running out of gold or souls keeps what was bought so far; running out of room
        /// buys nothing at all, so no gold is left sitting in loose parts.
        /// </summary>
        public BuyOutcome BuyPlan(PurchasePlan plan, out int bought)
        {
            bought = 0;
            if (!plan.IsPossible) return BuyOutcome.Unavailable;

            var work = inventory.Copy();
            var outcome = BuyOutcome.Bought;
            var done = 0;
            foreach (var step in plan.steps)
            {
                var before = work.Copy();
                var result = BuyOn(work, step);
                if (result != BuyOutcome.Bought)
                {
                    work = before;
                    outcome = result;
                    break;
                }
                done++;
            }

            if (outcome == BuyOutcome.NoSpace || outcome == BuyOutcome.Unavailable && done == 0) return outcome;
            if (done > 0)
            {
                inventory = work;
                ApplyStats();
                Changed?.Invoke();
            }
            bought = done;
            return outcome;
        }

        // Buys into the given inventory (a working copy); the caller commits it or throws it away.
        private static BuyOutcome BuyOn(OriginalInventory target, ShopEntry entry)
        {
            var quote = target.QuoteBuy(entry.shopId, entry.offerId);
            switch (quote.Code)
            {
                case OriginalItemActionCode.Success:
                    break;
                case OriginalItemActionCode.NotEnoughResources:
                    return quote.GoldCost > target.Gold ? BuyOutcome.NotEnoughGold : BuyOutcome.NotEnoughSouls;
                default:
                    return BuyOutcome.Unavailable;
            }

            var servantBefore = Occupied(target.ServantSlots);
            var result = target.TryBuy(entry.shopId, entry.offerId);
            if (result.Code == OriginalItemActionCode.Grounded) return BuyOutcome.NoSpace;
            if (!result.Applied) return BuyOutcome.Unavailable;
            // A full hero bag spills into the servant's bag, which this interface does not show: refuse instead.
            return Occupied(target.ServantSlots) > servantBefore ? BuyOutcome.NoSpace : BuyOutcome.Bought;
        }

        public bool Sell(int slot)
        {
            var candidate = inventory.Copy();
            var result = candidate.TrySell(OriginalInventoryBag.Hero, slot);
            if (!result.Applied) return false;
            inventory = candidate;
            ApplyStats();
            Changed?.Invoke();
            return true;
        }

        /// <summary>What selling the item in a slot would bring, or -1 when it cannot be sold.</summary>
        public int SellValue(int slot)
        {
            var candidate = inventory.Copy();
            var result = candidate.TrySell(OriginalInventoryBag.Hero, slot);
            return result.Applied ? (int)-result.GoldCost : -1;
        }

        private static int Occupied(OriginalItemInstance[] slots)
        {
            var count = 0;
            foreach (var slot in slots)
                if (slot != null) count++;
            return count;
        }

        private void ApplyStats()
        {
            if (hero == null) return;
            try
            {
                var plan = book.Effects.Plan(inventory.Snapshot());
                var profile = plan.NativeProfile;
                if (profile == null || !profile.known)
                {
                    hero.SetBonus(default);
                    return;
                }
                hero.SetBonus(new StatBonus
                {
                    strength = (float)profile.strength,
                    agility = (float)profile.agility,
                    intelligence = (float)profile.intelligence,
                    health = (float)profile.maxHealthFlat,
                    mana = (float)profile.maxManaFlat,
                    damage = (float)profile.attackDamage,
                    armor = (float)profile.armor,
                    attackSpeed = (float)profile.attackSpeedFraction,
                    // The data counts speed in map units per second, 64 of them to a metre.
                    moveSpeed = (float)profile.moveSpeedFlat / 64f,
                    healthRegen = (float)profile.healthRegenPerSecond
                });
            }
            catch (Exception exception)
            {
                // An item whose effect the data cannot describe must not break buying; the bonuses just stay as they were.
                if (warnedAboutEffects) return;
                warnedAboutEffects = true;
                Debug.LogWarning("Item bonuses could not be applied: " + exception.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PermanentItemAttributes
        {
            internal int strength, agility, intelligence;
            internal PermanentItemAttributes Copy() => new PermanentItemAttributes
                { strength = strength, agility = agility, intelligence = intelligence };
        }
        readonly Dictionary<int, PermanentItemAttributes> itemPermanentAttributes = new Dictionary<int, PermanentItemAttributes>();

        // bIv 38337..38378 redirects the three tomes to the living canonical
        // hero and increments its base attribute. Other branches below use
        // the exact declared native powerup amounts, not inventory bonuses.
        bool PrepareNativePowerupPickup(PickupTransaction result, Player player,
            OriginalWorldUnitView actor, OriginalInventory candidate, string itemId)
        {
            if(PrepareAdditionalRunePickup(result,player,actor,candidate,itemId))return true;
            bool tome = itemId == "tstr" || itemId == "tdex" || itemId == "tint";
            string abilityId = itemId == "gold" ? "AIgo" : itemId == "lmbr" ? "AIlu" :
                itemId == "rhe2" ? "APh2" : itemId == "rres" ? "APra" : itemId == "rspd" ? "APsa" : null;
            if (!tome && abilityId == null) return false;
            result.handled = true;
            if (actor.health <= .405) { result.code = OriginalItemActionCode.NotAllowed; return true; }
            var change = new PickupPlayerChange { player = player, originalInventory = player.inventory,
                inventory = candidate, originalProgression = player.progression, stats = player.stats };
            try
            {
                if (tome)
                {
                    var attributes = itemPermanentAttributes.TryGetValue(player.slot, out var existing) ? existing.Copy() : new PermanentItemAttributes();
                    checked
                    {
                        if (itemId == "tstr") attributes.strength++;
                        if (itemId == "tdex") attributes.agility++;
                        if (itemId == "tint") attributes.intelligence++;
                    }
                    var stats = ComposeEquipment(player.stats, candidate, attributes);
                    var profile = actor.profile.Copy();
                    profile.maxHealth = stats.maxHealth.Require(); profile.maxMana = stats.maxMana.Require();
                    // Native per-attribute half-tie arithmetic remains a
                    // labelled approximation, shared with learned attributes.
                    change.profile = new PreparedEquipmentProfile { entityId = actor.entityId, profile = profile,
                        health = UpdatedVitality(actor.health, actor.profile.maxHealth, profile.maxHealth, "ratio-nearest-approximation", true),
                        mana = UpdatedVitality(actor.mana, actor.profile.maxMana, profile.maxMana, "ratio-nearest-approximation", false) };
                    change.permanentAttributes = attributes;
                }
                else
                {
                    var item = itemCatalog.Item(itemId); var ability = combatCatalog.Ability(abilityId);
                    if (Array.IndexOf(item.abilityIds, abilityId) < 0 || !ability.TryNumber("DataA1", out double a, out _) || a < 0)
                        throw new InvalidOperationException("Native powerup declaration is unavailable.");
                    string expected = itemId == "gold" ? "AIgo" : itemId == "lmbr" ? "AIlu" : itemId == "rhe2" ? "AIha" : itemId == "rspd" ? "AIsa" : "AIra";
                    if (ability.Text("code") != expected) throw new InvalidOperationException("Native powerup family changed.");
                    if (itemId == "gold" || itemId == "lmbr")
                    {
                        if (a != Math.Truncate(a) || a > int.MaxValue) throw new InvalidOperationException("Invalid resource amount.");
                        candidate.GrantResources(itemId == "gold" ? (int)a : 0, itemId == "lmbr" ? (int)a : 0);
                    }
                    else if(itemId=="rspd")
                    {
                        if(ability.Text("BuffID1")!="Bspe"||a!=2||ability.Number("Dur1")!=15||
                            ability.Number("HeroDur1")!=15||ability.Number("Area1")!=2000)
                            throw new InvalidOperationException("Speed rune conflicts with native ITEMSTAT2.");
                        // ITEMSTAT2 measures the two heroes at522 for15s.
                        // Applying authored +200% to other base speeds and
                        // combining modifiers uses the shared derived host policy.
                        result.hasteDuration=15;result.hasteBonus=a;
                        foreach(var unit in world.Snapshot().units)
                            if(ItemAuraRecipient(actor,unit,ability,1)) result.hasteRecipients.Add(unit.entityId);
                    }
                    else
                    {
                        double mana = 0;
                        if (itemId == "rres" && (!ability.TryNumber("DataB1", out mana, out _) || mana < 0))
                            throw new InvalidOperationException("Native rune mana amount is unavailable.");
                        if (!ability.TryNumber("Area1", out double radius, out _) || radius < 0 || ability.Text("targs1") == null)
                            throw new InvalidOperationException("Native rune recipient declaration is unavailable.");
                        foreach (var unit in world.Snapshot().units)
                            if (ItemAuraRecipient(actor, unit, ability, 1))
                                result.resources.Add(new PreparedEquipmentProfile { entityId = unit.entityId, profile = unit.profile.Copy(),
                                    health = Math.Min(unit.profile.maxHealth, unit.health + a), mana = Math.Min(unit.profile.maxMana, unit.mana + mana) });
                    }
                }
                result.players.Add(change);
            }
            catch (InvalidOperationException) { result.code = OriginalItemActionCode.UnresolvedRule; }
            catch (OverflowException) { result.code = OriginalItemActionCode.NotAllowed; }
            return true;
        }
    }
}

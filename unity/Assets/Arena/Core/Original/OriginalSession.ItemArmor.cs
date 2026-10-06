using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemActiveRules activeItemRules;
        sealed class ItemArmorState { internal double amount, remaining; }
        readonly Dictionary<(int actor, string buff), ItemArmorState> itemArmor = new Dictionary<(int, string), ItemArmorState>();

        void ConfigureItemActives(OriginalObservedItemActives observations)
        {
            if (Started) throw new InvalidOperationException("Configure active items before starting a match.");
            activeItemRules = new OriginalItemActiveRules(itemCatalog, combatCatalog, observations);
            scriptItemRules = new OriginalItemScriptRules(itemCatalog, combatCatalog, observations);
            scriptActRules = new OriginalItemScriptActRules(itemCatalog, combatCatalog);
        }
        double ItemArmorBonus(int actorId)
        {
            double result = WidowArmorBonus(actorId);
            foreach (var pair in itemArmor) if (pair.Key.actor == actorId) result += pair.Value.amount;
            return result;
        }
        void AdvanceItemArmor(double elapsed)
        {
            foreach (var key in new List<(int actor, string buff)>(itemArmor.Keys))
            {
                var actor = world.UnitState(key.actor);
                if (actor == null || actor.health <= 0) { itemArmor.Remove(key); continue; }
                if (actor.paused) continue;
                itemArmor[key].remaining -= elapsed;
                if (itemArmor[key].remaining <= 1e-9) itemArmor.Remove(key);
            }
        }
        OriginalSessionReplyCode UseArmorItem(Player player, OriginalSessionCommand command, OriginalItemUseView view, OriginalItemArmorRule rule)
        {
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor.mana < rule.manaCost) return OriginalSessionReplyCode.NotReady;
            var recipients = new List<int>();
            // Authored friend/self/air/ground/invulnerable/vulnerable targets.
            // Center-radius filtering and same-Bdef refresh are explicit port
            // reconstruction; ITEMACT2 measured the caster and one ally200WC.
            foreach (var target in world.Snapshot().units)
                if (ItemAuraRecipient(actor, target, combatCatalog.Ability(rule.abilityId), 1))
                    recipients.Add(target.entityId);
            var candidate = player.inventory.Copy();
            if (rule.retired)
            {
                var consumed = candidate.ConsumeCharge(command.bag, command.itemSlot, command.itemInstanceId);
                if (!consumed.Applied) return RejectItem(player, consumed.Code);
            }
            try
            {
                var prepared = PrepareEquipmentProfile(player, candidate, actor);
                if (prepared.mana < rule.manaCost) return OriginalSessionReplyCode.NotReady;
                prepared.mana -= rule.manaCost;
                if (!CommitEquipmentProfile(prepared)) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                player.inventory = candidate;
                // Same-buff replacement, different-buff addition is an explicit
                // host transfer; mixed B011/Bdef native stacking is not measured.
                foreach (int id in recipients) itemArmor[(id, rule.buffId)] = new ItemArmorState { amount = rule.armor, remaining = rule.duration };
                itemCooldowns[ItemCooldownKey(player.slot, rule.cooldownGroup)] = ItemClock + rule.cooldown;
                player.lastItemAction = OriginalItemActionCode.Success;
            }
            catch (InvalidOperationException) { return RejectItem(player, OriginalItemActionCode.UnresolvedRule); }
            NotifyNativeSpellEffect(actor.entityId, rule.abilityId);
            if (rule.restoration > 0) ApplyUnitySwordRestoration(actor.entityId, rule.restoration);
            return OriginalSessionReplyCode.Accepted;
        }

        void ApplyUnitySwordRestoration(int actorId, double amount)
        {
            var actor = world.UnitState(actorId); if (actor == null) return;
            // S8's script recipient filter differs from the native armor mask.
            foreach (var target in world.Snapshot().units)
            {
                if (!DuelCleanupEligible(target.rawcode) || target.health <= .405 || AreEnemies(actor.ownerSlot, target.ownerSlot) ||
                    CasterHasType(target, "mechanical") || CasterHasType(target, "structure") || HasEffectiveUnitAbility(target, "A0K4") ||
                    SquaredDistance(actor.position, target.position) > 800 * 800) continue;
                ApplySourceHealing(actorId, target.entityId, amount);
                var current = world.UnitState(target.entityId);
                if (current != null) world.UpdateProfile(current.entityId, current.profile, current.health, Math.Min(current.profile.maxMana, current.mana + amount));
            }
        }

        void ApplySourceHealing(int sourceId, int targetId, double amount)
        {
            var source = world.UnitState(sourceId); var target = world.UnitState(targetId);
            if (source == null || target == null || target.health <= .405) return;
            // fL2934..2950: healer's ac flag, healer's debuffs, recipient curse.
            if (source.kind == OriginalWorldUnitKind.Hero)
                foreach (var item in PlayerAt(source.ownerSlot).inventory.HeroSlots)
                    if (item?.itemId == "I05Y") { amount *= 1.2; break; }
            if (HasWaveDarkBuff(sourceId) || HasEffectiveUnitAbility(source, "B0B2")) amount *= .5;
            bool inverted = HasEffectiveUnitAbility(target, "A19P") || target.kind == OriginalWorldUnitKind.Hero &&
                PlayerAt(target.ownerSlot).auxiliaryAbilities.ContainsKey("A19P");
            if (inverted)
                ApplyNativeTriggeredHit(targetId, target.ownerSlot, target, amount, OriginalTriggeredDamageMode.ChaosUniversal);
            else world.UpdateProfile(targetId, target.profile, Math.Min(target.profile.maxHealth, target.health + amount), target.mana);
            // H02E-specific healing XP is outside the three selected heroes.
        }
    }
}

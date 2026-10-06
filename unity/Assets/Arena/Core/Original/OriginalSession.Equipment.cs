using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalNativeItemProfile ItemNativeProfile(OriginalInventory inventory, string primary = null)
        {
            var equipment=inventory?.Snapshot();
            if(equipment!=null)
                for(int i=0;i<equipment.heroSlots.Length;i++)if(equipment.heroSlots[i]?.itemId=="I09D")equipment.heroSlots[i]=null;
            var profile = itemEffects == null || inventory == null ? new OriginalNativeItemProfile { known = true } :
                itemEffects.Plan(equipment).NativeProfile.Require();
            if (primary != null && inventory != null)
                foreach (var item in inventory.HeroSlots)
                {
                    if (item == null || item.itemId != "I05P") continue;
                    // Source YU keeps exactly the owner's primary attribute.
                    if (primary != "STR") profile.strength -= SpaceBootAttribute("A11P", "DataC1");
                    if (primary != "AGI") profile.agility -= SpaceBootAttribute("A11Q", "DataA1");
                    if (primary != "INT") profile.intelligence -= SpaceBootAttribute("A11R", "DataB1");
                }
            return profile;
        }

        double SpaceBootAttribute(string id, string field)
        {
            var ability = combatCatalog.Ability(id);
            if (ability.Text("code") != "AIab" || !ability.TryNumber(field, out double amount, out _) || amount < 0)
                throw new InvalidOperationException("Space boot primary attribute declaration is unavailable.");
            return amount;
        }

        sealed class PreparedEquipmentProfile
        {
            internal int entityId;
            internal OriginalWorldUnitProfile profile;
            internal double health, mana;
            internal OriginalSpellResistanceLedger resistance;
            internal Player player;
            internal System.Collections.Generic.SortedDictionary<string, int> auxiliaryAbilities;
            internal string[] removedNativeAbilities = Array.Empty<string>();
            internal OriginalInventory modeInventory;
            internal ItemMutability mutability;
        }

        bool ApplyEquipmentProfile(Player player, OriginalInventory candidate, OriginalWorldUnitView actor)
        {
            try
            {
                return CommitEquipmentProfile(PrepareEquipmentProfile(player, candidate, actor));
            }
            catch (InvalidOperationException) { return false; }
        }

        PreparedEquipmentProfile PrepareEquipmentProfile(Player player, OriginalInventory candidate, OriginalWorldUnitView actor)
        {
                if (itemEffects == null) return new PreparedEquipmentProfile { entityId = actor.entityId,
                    profile = actor.profile.Copy(), health = actor.health, mana = actor.mana };
                var stats = ComposeEquipment(player.stats ?? OriginalHeroStats.Calculate(combatCatalog, player.hero, 1, progressionObserved), candidate);
                var mutability = CopyItemMutability(actor.entityId);
                var vitality = new OriginalItemVitalityResult { known = true, health = actor.health,
                    maxHealth = actor.profile.maxHealth, mana = actor.mana, maxMana = actor.profile.maxMana };
                var before = player.inventory.HeroSlots;
                var after = candidate.HeroSlots;
                var resistance = nativeSpellResistance.Copy();
                bool Retained(OriginalItemInstance item, OriginalItemInstance[] slots) =>
                    Array.Exists(slots, other => other != null && other.instanceId == item.instanceId && other.itemId == item.itemId);
                void Change(OriginalItemInstance item, bool adding)
                {
                    if(item.itemId=="I09D")return;
                    // ITEMEX2: DROP runs while native attributes remain; PICKUP
                    // runs after their addition. Tu therefore undoes first.
                    if(item.itemId=="I05E"&&!adding)ChangeItemMutability(mutability,vitality,-1);
                    vitality = itemEffects.ChangeVitality(item.itemId, adding, vitality.health, vitality.maxHealth,
                        vitality.mana, vitality.maxMana, ProgressionConstant("StrHitPointBonus"), ProgressionConstant("IntManaBonus"), stats.primaryAttribute).Require();
                    if(item.itemId=="I05E"&&adding)ChangeItemMutability(mutability,vitality,1);
                    var sourcePlan = itemEffects.Plan((adding ? candidate : player.inventory).Snapshot());
                    foreach (var modifier in sourcePlan.DeclaredModifiers)
                    {
                        if (modifier.ItemInstanceId != item.instanceId || modifier.Stat != "spellResistanceFraction") continue;
                        if (modifier.Operation != "last-added") throw new InvalidOperationException("Unknown item resistance operation.");
                        string key = "item:" + item.instanceId + ":" + modifier.AbilityId + ":" + modifier.Occurrence;
                        if (adding) resistance.Set(actor.entityId, key, modifier.Value);
                        else resistance.Remove(actor.entityId, key);
                    }
                }
                // aM/EM residency determines which items actually affect the
                // hero. Servant-only moves never touch the hero's resources.
                foreach (var item in before) if (item != null && !Retained(item, after)) Change(item, false);
                foreach (var item in after) if (item != null && !Retained(item, before)) Change(item, true);
                stats=ComposeEquipment(player.stats ?? OriginalHeroStats.Calculate(combatCatalog,player.hero,1,progressionObserved),candidate,null,mutability);
                var remaining = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                foreach (var ability in itemEffects.CombatAbilities(candidate.Snapshot())) remaining.Add(ability.abilityId);
                var removed = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                foreach (var ability in itemEffects.CombatAbilities(player.inventory.Snapshot()))
                    if (!remaining.Contains(ability.abilityId)) removed.Add(ability.abilityId);
                if (Math.Abs(vitality.maxHealth - stats.maxHealth.Require()) > 1e-6 ||
                    Math.Abs(vitality.maxMana - stats.maxMana.Require()) > 1e-6)
                    throw new InvalidOperationException("Item vitality and composed maxima differ.");
                var profile = actor.profile.Copy();
                profile.maxHealth = stats.maxHealth.Require(); profile.maxMana = stats.maxMana.Require();
                profile.moveSpeed = ResolveAbilityMoveSpeed(player.slot, stats.baseMoveSpeed);
                if (IsRingForcedActor(actor.entityId) || IsShieldForceActive(actor.entityId)) profile.moveSpeed = 0;
                return new PreparedEquipmentProfile { entityId = actor.entityId, profile = profile,
                    health = vitality.health, mana = vitality.mana, resistance = resistance, player = player,
                    auxiliaryAbilities = EquipmentAuxiliaries(player, candidate, player.progression),
                    modeInventory = candidate,
                    mutability = mutability,
                    removedNativeAbilities = new System.Collections.Generic.List<string>(removed).ToArray() };
        }

        bool CommitEquipmentProfile(PreparedEquipmentProfile prepared, bool profileAlreadyPublished = false)
        {
            if (!profileAlreadyPublished && !world.UpdateProfile(prepared.entityId, prepared.profile, prepared.health, prepared.mana)) return false;
            if (prepared.resistance != null) nativeSpellResistance.ReplaceActorFrom(prepared.entityId, prepared.resistance);
            if(prepared.mutability!=null)itemMutability[prepared.entityId]=prepared.mutability;
            if (prepared.auxiliaryAbilities != null) prepared.player.auxiliaryAbilities = prepared.auxiliaryAbilities;
            foreach (string ability in prepared.removedNativeAbilities) ForgetWaveTraitItemAbility(prepared.entityId, ability);
            if (prepared.modeInventory != null)
            { SyncItemModeInventory(prepared.entityId,prepared.modeInventory); SyncAstralGuardian(prepared.entityId,prepared.modeInventory);
                SyncChargeBladeInventory(prepared.entityId,prepared.modeInventory); SyncItemAxeInventory(prepared.entityId,prepared.modeInventory); }
            return true;
        }

        // Level and learned attributes remain the unitemized baseline. Rebuild
        // from a detached inventory candidate so repeated XP synchronization,
        // transfers and removals cannot accumulate bonuses a second time.
        OriginalHeroStatsSnapshot ComposeEquipment(OriginalHeroStatsSnapshot baseline, OriginalInventory inventory, PermanentItemAttributes permanent = null, ItemMutability mutability = null)
        {
            var profile = ItemNativeProfile(inventory, baseline.primaryAttribute);
            if (permanent == null && inventory != null) itemPermanentAttributes.TryGetValue(inventory.OwnerId, out permanent);
            if (permanent != null)
            {
                profile.strength += permanent.strength;
                profile.agility += permanent.agility;
                profile.intelligence += permanent.intelligence;
            }
            var result = ComposeSoulUpgrades(baseline, inventory?.OwnerId ?? 0);
            AddItemScriptAttributes(inventory?.OwnerId ?? 0, profile);
            result.strength = Plus(result.strength, profile.strength);
            result.agility = Plus(result.agility, profile.agility);
            result.intelligence = Plus(result.intelligence, profile.intelligence);
            result.primary = result.primaryAttribute == "STR" ? result.strength :
                result.primaryAttribute == "AGI" ? result.agility : result.intelligence;
            double primaryBonus = result.primaryAttribute == "STR" ? profile.strength :
                result.primaryAttribute == "AGI" ? profile.agility : profile.intelligence;
            double attributeDamage = primaryBonus * ProgressionConstant("StrAttackBonus");
            result.primaryDamageBonus = Plus(result.primaryDamageBonus, attributeDamage);
            result.attackMinimum = Plus(result.attackMinimum, attributeDamage + profile.attackDamage);
            result.attackMaximum = Plus(result.attackMaximum, attributeDamage + profile.attackDamage);
            result.maxHealth = Plus(result.maxHealth, profile.maxHealthFlat + profile.strength * ProgressionConstant("StrHitPointBonus"));
            result.maxMana = Plus(result.maxMana, profile.maxManaFlat + profile.intelligence * ProgressionConstant("IntManaBonus"));
            result.armor = Plus(result.armor, profile.armor + profile.agility * ProgressionConstant("AgiDefenseBonus"));
            result.agilityAttackSpeedBonus = Plus(result.agilityAttackSpeedBonus, profile.agility * ProgressionConstant("AgiAttackSpeedBonus"));
            result.itemAttackDamageBonus = profile.attackDamage;
            result.itemAttackSpeedBonus = profile.attackSpeedFraction;
            result.itemHealthRegen = profile.healthRegenPerSecond;
            result.itemManaRegenFraction = profile.manaRegenBaseFraction;
            result.baseMoveSpeed += profile.moveSpeedFlat;
            ComposeItemModeStats(result,inventory);
            ComposeItemMutability(result,inventory?.OwnerId??0,mutability);
            return result;
        }
    }
}


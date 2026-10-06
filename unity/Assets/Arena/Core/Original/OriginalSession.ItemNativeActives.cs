using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class NativeItemActionRule
        {
            internal string abilityId, cooldownGroup, status, sourceEffect;
            internal double health, mana, cooldown, duration, fade, manaCost, movement, strike, nativeDamage, range;
            internal bool requiresCharge=true;
            internal OriginalAbilityTargetMode targetMode;
        }

        NativeItemActionRule NativeItemAction(string itemId)
        {
            if(itemId=="I0AJ")return RegenerationItemRule();
            if(itemId=="I09X")return ChainRootItemRule();
            if(itemId=="I0B2")return FlameBootItemRule();
            if(itemId=="I07C")return ItemAxeRule();
            if(itemId=="I01Y"||itemId=="I01B"||itemId=="I07M")return ShellItemRule(itemId);
            var sourceRule=SourceNativeItemRule(itemId);if(sourceRule!=null)return sourceRule;
            if(itemId=="I013"||itemId=="I0AP")return WindWalkItemRule(itemId);
            if(itemId=="I026")
            {
                var source=itemCatalog.Item(itemId);var a=combatCatalog.Ability("A088");
                if(source==null||Array.IndexOf(source.abilityIds,"A088")<0||source.cooldownId!="A088"||a?.Text("code")!="ANfd"||
                    a.Number("DataC1")!=275||a.Number("Cost1")!=80||a.Number("Cool1")!=12||a.Number("Rng1")!=500||
                    a.Text("targs1")!="air,ground,enemy,organic,neutral")throw new InvalidOperationException("Finger item declaration changed.");
                // ANfd Nfd1/2 are visual delay/duration, not mechanical delay.
                // Instant activation and SPELLS/MAGIC damage transfer use the
                // host native-spell family policy; exact alias engine axes are
                // not promoted to measured proof by these declarations.
                return new NativeItemActionRule{abilityId="A088",cooldownGroup="A088",requiresCharge=false,
                    targetMode=OriginalAbilityTargetMode.Unit,nativeDamage=275,range=500,manaCost=80,cooldown=12};
            }
            if(itemId=="I06M" || itemId=="I06O")
            {
                bool invis=itemId=="I06M";string id=invis?"AIv1":"AIdv",code=invis?"AIvi":"AHds",buff=invis?"B034":"B035";
                var source=itemCatalog.Item(itemId);var a=combatCatalog.Ability(id);double duration=invis?7:3,cooldown=invis?13:12;
                if(source==null||source.abilityIds.Length!=1||source.abilityIds[0]!=id||source.cooldownId!=id||
                    a==null||a.Text("code")!=code||a.Text("BuffID1")!=buff||a.Number("Dur1")!=duration||
                    a.Number("HeroDur1")!=duration||a.Number("Cool1")!=cooldown)
                    throw new InvalidOperationException("Native item status identity changed.");
                bool present=Array.Exists(a.fields,f=>f.key=="Cost1")||Array.Exists(a.overrides,f=>f.field=="amcs"&&f.level==1);
                if(present&&(!a.TryNumber("Cost1",out double value,out _)||value!=0))
                    throw new InvalidOperationException("Native item status mana conflicts with ITEMSTAT2.");
                // ITEMSTAT2 campaign8673413b357d: instant cost0, B0347s with
                //2s fade and B0353s. Alias-specific, not base AHds Cost25.
                return new NativeItemActionRule{abilityId=id,cooldownGroup=id,status=buff,duration=duration,fade=invis?2:0,cooldown=cooldown};
            }
            if (itemId != "I01L") return null;
            var item = itemCatalog.Item(itemId); var ability = combatCatalog.Ability("A05X");
            if (item == null || item.abilityIds.Length != 1 || item.abilityIds[0] != "A05X" || item.cooldownId != "AIra" ||
                ability.Text("code") != "AIra" || ability.Number("DataA1") != 500 || ability.Number("DataB1") != 250 ||
                ability.Number("Area1") != 1000 || ability.Number("Cool1") != 25 ||
                ability.Text("targs1") != "ground,air,friend,self,organic,vuln,invu")
                throw new InvalidOperationException("Native area-restoration declaration differs.");
            // War3Patch AbilityData.slk AIra Cost1=0, row58879. Transfer of
            // this sparse field and immediate family activation is derived;
            // the per-item callback/cost control is a separate native probe.
            bool declared = Array.Exists(ability.fields, f => f.key == "Cost1") ||
                Array.Exists(ability.overrides, f => f.field == "amcs" && f.level == 1);
            if (declared && (!ability.TryNumber("Cost1", out double cost, out _) || cost != 0))
                throw new InvalidOperationException("Native area-restoration mana conflict.");
            return new NativeItemActionRule { abilityId = ability.id, cooldownGroup = item.cooldownId,
                health = 500, mana = 250, cooldown = 25 };
        }

        OriginalSessionReplyCode UseNativeItemAction(Player player, OriginalSessionCommand command,
            OriginalItemUseView view, NativeItemActionRule rule)
        {
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if(actor.mana<rule.manaCost)return OriginalSessionReplyCode.NotReady;
            // Preserve a charge at full MP; exact native full-resource refusal
            // is not covered by the partial-resource ITEMREGEN1 measurement.
            if(rule.status=="B0B1"&&actor.mana>=actor.profile.maxMana)return OriginalSessionReplyCode.NotReady;
            var ability = combatCatalog.Ability(rule.abilityId);
            var recipients = new List<OriginalWorldUnitView>();
            bool needed = false;
            foreach (var target in world.Snapshot().units)
                if ((rule.health > 0 || rule.mana > 0) && ItemAuraRecipient(actor, target, ability, 1))
                {
                    recipients.Add(target);
                    needed |= target.health < target.profile.maxHealth || target.mana < target.profile.maxMana;
                }
            // Native area restoration's exact all-full refusal is not measured.
            // The host preserves a charge when nobody can benefit.
            if (!needed && rule.status==null && rule.nativeDamage<=0 && rule.sourceEffect==null) return OriginalSessionReplyCode.NotReady;
            var candidate = player.inventory.Copy();
            if(rule.requiresCharge)
            {
                var consumed = candidate.ConsumeCharge(command.bag, command.itemSlot, command.itemInstanceId);
                if (!consumed.Applied) return RejectItem(player, consumed.Code);
            }
            try
            {
                var prepared = PrepareEquipmentProfile(player, candidate, actor);
                prepared.mana-=rule.manaCost;
                foreach (var target in recipients)
                    if (target.entityId == actor.entityId)
                    {
                        prepared.health = Math.Min(prepared.profile.maxHealth, prepared.health + rule.health);
                        prepared.mana = Math.Min(prepared.profile.maxMana, prepared.mana + rule.mana);
                    }
                if (!CommitEquipmentProfile(prepared)) return RejectItem(player, OriginalItemActionCode.UnresolvedRule);
                // Other recipients retain their exact profiles/body radii and
                // are living throughout this synchronous host transaction.
                // No callbacks run until every resource update is published.
                foreach (var target in recipients)
                    if (target.entityId != actor.entityId)
                        world.UpdateProfile(target.entityId, target.profile,
                            Math.Min(target.profile.maxHealth, target.health + rule.health),
                            Math.Min(target.profile.maxMana, target.mana + rule.mana));
                player.inventory = candidate;
                itemCooldowns[ItemCooldownKey(player.slot, rule.cooldownGroup)] = ItemClock + rule.cooldown;
                player.lastItemAction = OriginalItemActionCode.Success;
                if(rule.status!=null)
                {
                    world.Stop(actor.entityId);
                    ApplyNativeItemStatus(actor.entityId,rule);
                    if(rule.abilityId=="A01X")BeginWidowSweep(actor.entityId);
                }
            }
            catch (InvalidOperationException) { return RejectItem(player, OriginalItemActionCode.UnresolvedRule); }
            NotifyNativeSpellEffect(actor.entityId, rule.abilityId);
            if(rule.sourceEffect=="anti-magic-shell"||rule.sourceEffect=="lightning-shell")BeginItemShell(actor,command.targetId,rule);
            else if(rule.sourceEffect=="chain-root")BeginItemEnsnare(actor,command.targetId);
            else if(rule.sourceEffect=="flame-boots")BeginFlameBoots(actor.entityId);
            else if(rule.sourceEffect=="axe-toggle")ApplyItemAxeToggle(actor.entityId);
            else if(rule.sourceEffect!=null)ApplySourceNativeItemAction(actor,command,rule);
            if(rule.nativeDamage>0)
                ApplyNativeTriggeredHit(actor.entityId,actor.ownerSlot,world.UnitState(command.targetId),rule.nativeDamage,OriginalTriggeredDamageMode.SpellMagic);
            return OriginalSessionReplyCode.Accepted;
        }

        OriginalSessionReplyCode ValidateNativeItemTarget(Player player,OriginalSessionCommand command,NativeItemActionRule rule,bool enforceRange=true)
        {
            if(rule.sourceEffect=="chain-root")return ValidateItemChainRoot(player,command,enforceRange);
            if(rule.sourceEffect=="lightning-shell")return ValidateLightningItemTarget(player,command,enforceRange);
            if(rule.targetMode==OriginalAbilityTargetMode.None)
                return command.targetKind==OriginalWorldTargetKind.None&&command.targetId==0?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.InvalidCommand;
            var actor=world?.UnitState(OriginalWorld.HeroEntityId(player.slot));var target=world?.UnitState(command.targetId);
            if(command.targetKind!=OriginalWorldTargetKind.Unit||actor==null||target==null||target.health<=.405||target.invulnerable||
                !AreEnemies(actor.ownerSlot,target.ownerSlot)||!CanSeeForCombat(actor.ownerSlot,target)||CasterMagicImmune(target)||
                CasterHasType(target,"mechanical")||
                !WeaponTargetTypeAllowed(combatCatalog.Ability(rule.abilityId).Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType"))||
                enforceRange&&SquaredDistance(actor.position,target.position)>rule.range*rule.range)return OriginalSessionReplyCode.InvalidCommand;
            return OriginalSessionReplyCode.Accepted;
        }
    }
}

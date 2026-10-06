using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemChannelRules itemChannelRules;
        sealed class ItemChannelEffect
        {
            internal string ability,visualAbility;
            internal int actor, owner, target, ticks;
            internal double due, period, amount, travel;
            internal bool critical;
            internal OriginalPoint point, destination;
            internal readonly HashSet<int> visited = new HashSet<int>();
        }
        readonly List<ItemChannelEffect> itemChannelEffects = new List<ItemChannelEffect>();
        readonly Dictionary<int, ItemChannelEffect> itemSpitPoison = new Dictionary<int, ItemChannelEffect>();
        OriginalItemChannelRule ItemChannelRule(string id)
        {
            if (itemChannelRules == null) itemChannelRules = new OriginalItemChannelRules(itemCatalog, combatCatalog);
            return itemChannelRules.Rule(id);
        }

        OriginalSessionReplyCode ValidateItemChannelTarget(Player player, OriginalSessionCommand command, OriginalItemChannelRule rule, bool enforceRange = true)
        {
            var actor = world.UnitState(player.slot);
            if (actor == null) return OriginalSessionReplyCode.NotReady;
            if (rule.targetMode == OriginalAbilityTargetMode.Unit)
            {
                if (command.targetKind != OriginalWorldTargetKind.Unit || command.targetId <= 0) return OriginalSessionReplyCode.InvalidCommand;
                var target = world.UnitState(command.targetId);
                if (target == null || target.health <= .405 || !CanSeeForCombat(actor.ownerSlot,target)) return OriginalSessionReplyCode.NotReady;
                if(rule.nativeCode=="Aste")
                {
                    // ITEMTARGET1 a7dc24a26973: both Aste aliases accept an
                    // allied hero. A0FI's textual "enemies" is not its native filter.
                    if(target.entityId==actor.entityId||rule.abilityId=="A0FI"&&target.invulnerable)return OriginalSessionReplyCode.NotReady;
                }
                else if(rule.nativeCode=="Auhf")
                {
                    if(target.kind!=OriginalWorldUnitKind.Hero||AreEnemies(actor.ownerSlot,target.ownerSlot)||target.invulnerable)
                        return OriginalSessionReplyCode.NotReady;
                }
                else if(CasterHasType(target,"structure")||CasterHasType(target,"mechanical"))return OriginalSessionReplyCode.NotReady;
                bool healing = rule.abilityId == "A12Y" || rule.abilityId == "A12Z";
                if ((rule.nativeCode=="ANcl"||rule.nativeCode=="AOcl") && rule.abilityId != "A0KP" && (AreEnemies(actor.ownerSlot, target.ownerSlot) == healing || !healing && (target.invulnerable || CasterMagicImmune(target))))
                    return OriginalSessionReplyCode.NotReady;
                if (enforceRange && SquaredDistance(actor.position, target.position) > rule.range * rule.range) return OriginalSessionReplyCode.NotReady;
            }
            else
            {
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0) return OriginalSessionReplyCode.InvalidCommand;
                if (rule.targetMode == OriginalAbilityTargetMode.Point && (!ValidPoint(command.x, command.y) ||
                    enforceRange && SquaredDistance(actor.position, new OriginalPoint(command.x, command.y)) > rule.range * rule.range))
                    return OriginalSessionReplyCode.NotReady;
            }
            return OriginalSessionReplyCode.Accepted;
        }

        OriginalSessionReplyCode UseItemChannel(Player player, OriginalSessionCommand command, OriginalItemChannelRule rule)
        {
            var valid = ValidateItemChannelTarget(player, command, rule);
            if (valid != OriginalSessionReplyCode.Accepted) return valid;
            if (!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic) ||
                !TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.ChaosUniversal)) return OriginalSessionReplyCode.RuleUnavailable;
            // Preflight stats before any mana/cooldown mutation.
            if (rule.abilityId == "A17V")
            {
                ItemChannelAttributeSum(player.slot);
                if (combatCatalog.Ability("A17U")?.Text("BuffID1") != "B0AY") return OriginalSessionReplyCode.RuleUnavailable;
            }
            if (!PreflightItemChannelExtra(player, rule.abilityId)) return OriginalSessionReplyCode.RuleUnavailable;
            if (!world.TrySpendMana(player.slot, rule.manaCost)) return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot, rule.cooldownGroup)] = ItemClock + rule.cooldown;
            player.lastItemAction = OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(player.slot, rule.abilityId);
            BeginItemChannel(player.slot, rule.abilityId, command.targetId, new OriginalPoint(command.x, command.y));
            var target=world.UnitState(command.targetId);
            // ITEMTARGET1 observed one native zero callback after EFFECT for
            // enemy Aste casts; allied casts had none. Dispatch outside hL.
            if(rule.nativeCode=="Aste"&&target!=null&&AreEnemies(player.slot,target.ownerSlot))
                ApplyNativeTriggeredHit(player.slot,player.slot,target,0,OriginalTriggeredDamageMode.SpellMagic);
            if(rule.nativeCode=="AOsh")ApplyItemShockNative(player.slot,new OriginalPoint(command.x,command.y));
            return OriginalSessionReplyCode.Accepted;
        }

        double ItemChannelAttributeSum(int actor)
        {
            var stats = HeroCombatStats(world.UnitState(actor).ownerSlot);
            return stats.strength.Require() + stats.agility.Require() + stats.intelligence.Require();
        }
        void BeginItemChannel(int actorId, string ability, int targetId, OriginalPoint point)
        {
            if(BeginItemOffense(actorId,ability,targetId,point))return;
            var actor = world.UnitState(actorId);
            var effect = new ItemChannelEffect { actor = actorId, owner = actor.ownerSlot, ability = ability,
                target = targetId, point = actor.position, destination = point };
            switch (ability)
            {
                case "A12Y": case "A12Z":
                    effect.amount = ability == "A12Y" ? 400 : 700; effect.period = .3;
                    effect.visited.Add(targetId); ApplySourceHealing(actorId, targetId, effect.amount); break;
                case "A0CC": effect.period = .5; effect.ticks = 16; break;
                case "A17V": effect.period = .03; break;
                case "A0TX":
                    // OL blocks the source handler after native activation.
                    if (HasEffectiveUnitAbility(world.UnitState(targetId), "B06X")) return;
                    effect.period = .04; effect.visited.Add(targetId); break;
                default: BeginItemChannelExtra(actorId, ability, targetId, point); return;
            }
            effect.due = world.Clock + effect.period; itemChannelEffects.Add(effect);
        }

        void AdvanceItemChannels()
        {
            AdvanceItemChannelExtras();
            foreach (var effect in itemChannelEffects.ToArray())
                while (itemChannelEffects.Contains(effect) && effect.due <= world.Clock + 1e-9)
                {
                    effect.due += effect.period;
                    if (!TickItemChannel(effect)) itemChannelEffects.Remove(effect);
                }
            foreach (var pair in new List<KeyValuePair<int, ItemChannelEffect>>(itemSpitPoison))
            {
                var effect = pair.Value;
                while (itemSpitPoison.ContainsKey(pair.Key) && effect.due <= world.Clock + 1e-9)
                {
                    effect.due += 1;
                    var target = world.UnitState(pair.Key);
                    if (effect.ticks <= 0 || target == null || target.health <= .405)
                    { itemSpitPoison.Remove(pair.Key); if (target != null) ApplyUnitAbilityOverlay(pair.Key, null, new[] { "A17U" }); break; }
                    effect.ticks--;
                    ApplyTriggeredHit(effect.actor, effect.owner, target, ItemChannelAttributeSum(effect.actor) * .2,
                        OriginalTriggeredDamageMode.SpellMagic);
                }
            }
        }

        bool TickItemChannel(ItemChannelEffect effect)
        {
            var actor = world.UnitState(effect.actor); if (actor == null) return false;
            if (effect.ability == "A12Y" || effect.ability == "A12Z" || effect.ability == "chain_heal" || effect.ability == "chain_damage")
            {
                bool heal = effect.ability != "chain_damage";
                if (effect.ticks == (heal ? 9 : 7)) return false;
                var previous = world.UnitState(effect.target); if (previous == null) return false;
                OriginalWorldUnitView nearest = null; double distance = heal ? 1000 * 1000 : 700 * 700;
                foreach (var target in CasterUnits())
                {
                    if (effect.visited.Contains(target.entityId) || target.health <= .405 || AreEnemies(effect.owner, target.ownerSlot) == heal ||
                        CasterHasType(target, "structure") || CasterHasType(target, "mechanical") ||
                        (heal ? HasEffectiveUnitAbility(target, "A0K4") : !CanSeeForCombat(effect.owner,target) || CasterMagicImmune(target))) continue;
                    double candidate = SquaredDistance(previous.position, target.position);
                    if (candidate < distance) { distance = candidate; nearest = target; }
                }
                if (nearest == null) return false;
                effect.point = previous.position;
                effect.target = nearest.entityId; effect.visited.Add(nearest.entityId); effect.ticks++;
                ApplyItemChainHit(effect, nearest, heal); return true;
            }
            if (effect.ability == "A0CC")
            {
                if (effect.ticks <= 0 || actor.health <= .405) { ObserveScriptedHelperDeath(); return false; }
                double distance = Math.Sqrt(SquaredDistance(effect.point, actor.position));
                effect.point = distance > 600 ? actor.position : StepItemPoint(effect.point, actor.position, 30 + distance * .15, true);
                if (effect.ticks % 2 == 0)
                    foreach (var target in CasterUnits())
                        if (target.health > .405 && !AreEnemies(effect.owner, target.ownerSlot) && !CasterHasType(target, "structure") &&
                            SquaredDistance(effect.point, target.position) <= 600 * 600)
                            world.UpdateProfile(target.entityId, target.profile, Math.Min(target.profile.maxHealth, target.health + target.profile.maxHealth * .03), target.mana);
                effect.ticks--; return true;
            }
            if (effect.ability == "A17V")
            {
                effect.travel += 21;
                if (effect.travel < Math.Sqrt(SquaredDistance(effect.point, effect.destination))) return true;
                double damage = ItemChannelAttributeSum(effect.actor) * 1.5;
                foreach (var target in CasterUnits())
                    if (target.health > .405 && AreEnemies(effect.owner, target.ownerSlot) && !CasterMagicImmune(target) &&
                        !CasterHasType(target, "structure") && SquaredDistance(effect.destination, target.position) <= 200 * 200)
                    {
                        ApplyTriggeredHit(effect.actor, effect.owner, target, damage, OriginalTriggeredDamageMode.SpellMagic);
                        if (itemSpitPoison.TryGetValue(target.entityId, out var existing)) existing.ticks = 5;
                        else if (!HasEffectiveUnitAbility(world.UnitState(target.entityId), "A17U"))
                        {
                            ApplyUnitAbilityOverlay(target.entityId, new[] { "A17U" }, null);
                            itemSpitPoison.Add(target.entityId, new ItemChannelEffect { actor = effect.actor, owner = effect.owner,
                                ticks = 5, due = world.Clock + 1 });
                        }
                    }
                return false;
            }
            // x8: flying transfusion, nine victims, then return at1.25 speed.
            var victim = world.UnitState(effect.target); if (victim == null) return false;
            effect.point = StepItemPoint(effect.point, victim.position, effect.target == effect.actor ? 30 : 24, false);
            if (SquaredDistance(effect.point, victim.position) >= 60 * 60) return true;
            effect.point = victim.position;
            if (effect.target == effect.actor) { ApplySourceHealing(effect.actor, effect.actor, effect.amount * .5); ObserveScriptedHelperDeath(); return false; }
            if (victim.health > .405 && !victim.hidden)
            {
                double damage = Math.Min(600, victim.health * .09) + 300;
                ApplyTriggeredHit(effect.actor, effect.owner, victim, damage, OriginalTriggeredDamageMode.ChaosUniversal);
                effect.amount += damage; effect.ticks++;
            }
            OriginalWorldUnitView next = null; double nearestDistance = 500 * 500;
            foreach (var target in CasterUnits())
            {
                if (target.health <= .405 || target.hidden || effect.visited.Contains(target.entityId) ||
                    AreEnemies(victim.ownerSlot, target.ownerSlot) || CasterHasType(target, "structure")) continue;
                double distance = SquaredDistance(victim.position, target.position);
                if (distance < nearestDistance) { next = target; nearestDistance = distance; }
            }
            effect.target = next == null || effect.ticks == 9 ? effect.actor : next.entityId;
            effect.visited.Add(effect.target); return true;
        }

        void BeginItemSourceChain(int actorId, int targetId, double amount, bool healing, bool critical = false,string visualAbility=null)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null) return;
            var effect = new ItemChannelEffect { actor=actorId,owner=actor.ownerSlot,target=targetId,amount=amount,
                ability=healing?"chain_heal":"chain_damage",visualAbility=visualAbility,point=actor.position,critical=critical,period=.3,due=world.Clock+.3 };
            effect.visited.Add(targetId); ApplyItemChainHit(effect,target,healing); itemChannelEffects.Add(effect);
        }
        void ApplyItemChainHit(ItemChannelEffect effect, OriginalWorldUnitView target, bool healing)
        {
            if (healing) ApplySourceHealing(effect.actor,target.entityId,effect.amount);
            else
            {
                bool previous=sourceChainDamageRunning;sourceChainDamageRunning=true;
                try{ApplyTriggeredHit(effect.actor,effect.owner,target,
                    effect.amount*(effect.critical&&ItemActRandomIndex(99)<25?2:1),OriginalTriggeredDamageMode.SpellMagic);}
                finally{sourceChainDamageRunning=previous;}
            }
        }

        static OriginalPoint StepItemPoint(OriginalPoint from, OriginalPoint target, double step, bool clamp)
        {
            double distance = Math.Sqrt(SquaredDistance(from, target));
            if (distance <= 1e-9 || clamp && distance <= step) return target;
            return new OriginalPoint(from.x + (target.x - from.x) / distance * step, from.y + (target.y - from.y) / distance * step);
        }
    }
}

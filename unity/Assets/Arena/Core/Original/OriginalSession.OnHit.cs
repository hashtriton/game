using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PoisonState
        {
            internal int target, source, owner;
            internal double expires, nextTick, baseMoveSpeed, damagePerTick;
            internal string payloadBuffId;
            internal OriginalPoisonRules rules;
        }
        readonly Dictionary<int, PoisonState> poisons = new Dictionary<int, PoisonState>();

        IEnumerable<OriginalCombatDefinition> NativeUnitAbilities(OriginalWorldUnitView actor)
        {
            foreach (var originalId in EffectiveUnitAbilityIds(actor))
            {
                string id = originalId;
                var definition = combatCatalog.Ability(id);
                if (definition != null) yield return definition;
            }
        }
        string WeaponPoison(OriginalWorldUnitView source)
        {
            if (source.kind == OriginalWorldUnitKind.Illusion) return null; // Native passive inheritance is a separate measured closure.
            foreach (var ability in NativeUnitAbilities(source))
                // Dispatch the four individually measured native rank1 rows.
                // Shared-buff mixtures use the native MIXPOIS2 rules below.
                if (ability.Text("code") == "Aven" && (ability.id == "A0TC" || ability.id == "A0TD" || ability.id == "A0TE" || ability.id == "A0TF"))
                { _ = new OriginalPoisonRules(combatCatalog, ability.id, 1); return ability.id; }
            return null;
        }
        void ApplyMeleeReflection(Projectile shot, OriginalWorldUnitView defender)
        {
            if (!shot.melee || shot.damage <= 0 || defender.kind == OriginalWorldUnitKind.Illusion) return;
            var attacker = world.UnitState(shot.attacker);
            if (attacker == null || attacker.health <= 0 || attacker.hidden || attacker.invulnerable) return;
            double fraction = NativeThornsFraction(defender);
            if (fraction <= 0) return;
            double damage = shot.damage * fraction * OriginalAttackRules.DamageTypeMultiplier(native,"spells",combatCatalog.Unit(attacker.rawcode).Text("defType"));
            // Native numeric armor is excluded. Image and shield incoming
            // modifiers retain their measured type-specific pipeline.
            damage = IncomingMagicDamage(attacker, damage);
            ApplyResolvedUnitHit(defender.entityId, defender.ownerSlot, attacker, damage);
        }
        double IncomingMagicDamage(OriginalWorldUnitView target, double damage)
        {
            if (target.kind == OriginalWorldUnitKind.Hero && defendingHeroes.Contains(target.ownerSlot))
                damage *= new OriginalDefendRules(combatCatalog, SkillRank(players.Find(p => p.slot == target.ownerSlot), "A05M")).magicMultiplier;
            return ImageIncomingDamage(target.entityId, damage) * ArcherIncomingDamageMultiplier(target.entityId) * NativeSpellResistanceMultiplier(target.entityId) * NativeDefendItemSpellFactor(target);
        }
        void ApplyPoison(Projectile shot, OriginalWorldUnitView target)
        {
            if (shot.poisonAbility == null) return;
            // Aven organic targets: explicit mechanical classification excludes
            // mechanical units; world has no structures in this weapon path.
            string types = combatCatalog.Unit(target.rawcode).Text("type") ?? "";
            if (Array.IndexOf(types.Split(','), "mechanical") >= 0) return;
            var rules = new OriginalPoisonRules(combatCatalog, shot.poisonAbility, 1);
            poisons.TryGetValue(target.entityId, out var previous);
            if(previous!=null && previous.expires<=world.Clock+1e-9)
            {RemovePoison(target.entityId);previous=null;}
            bool weakerOtherBuff = previous != null && previous.payloadBuffId != rules.buffId && rules.damagePerTick < previous.damagePerTick;
            CaptureAbilityMovementBase(target.entityId);
            var state = previous ?? new PoisonState { target = target.entityId, rules = rules,
                nextTick = world.Clock + OriginalPoisonRules.InitialTickDelay, baseMoveSpeed = UnmodifiedNonHeroMovement(target) };
            // MIXPOIS2 rank1, TC/TD and TE/TF in both actual hit orders:
            // latest slow, strongest periodic damage/owner, maximum expiry.
            // Equal damage changes owner; a live buff retains its tick phase.
            if(previous==null || rules.damagePerTick>=state.damagePerTick)
            {state.damagePerTick=rules.damagePerTick;state.source=shot.attacker;state.owner=shot.owner;state.payloadBuffId=rules.buffId;}
            state.rules=rules;
            // CROSSP1 proves one Aven bucket across B06K/B06L, not independent
            // stacking. A weaker different buff changes slow but neither its
            // damage/buff owner nor expiry (TD->TF ends at the original TD+4).
            // Equal/stronger shorter-duration cases retain the existing maximum
            // endpoint policy; that unmeasured axis remains reconstruction.
            if (!weakerOtherBuff)
                state.expires = Math.Max(state.expires,world.Clock + (IsNativeHeroPredicate(target) ? rules.heroDuration : rules.duration));
            poisons[target.entityId] = state;
            RefreshAbilityMovement(target.entityId);
            RescaleWeaponRate(target.entityId, world.Clock);
        }
        double PoisonMovementMultiplier(int id) => poisons.TryGetValue(id, out var poison) ? 1 - poison.rules.moveSlow : 1;
        double PoisonAttackSlow(int id) => poisons.TryGetValue(id, out var poison) ? poison.rules.attackSlow : 0;
        void RefreshAbilityMovement(int id)
        {
            var actor = world.UnitState(id); if (actor == null) return;
            var profile = actor.profile.Copy();
            if (actor.kind == OriginalWorldUnitKind.Hero)
                profile.moveSpeed = ResolveAbilityMoveSpeed(actor.ownerSlot, HeroCombatStats(actor.ownerSlot).baseMoveSpeed);
            else if (actor.kind == OriginalWorldUnitKind.Illusion)
                profile.moveSpeed = ClampAbilityMoveSpeed(combatCatalog.Unit(actor.rawcode).Number("spd") *
                    (PoisonMovementMultiplier(id) + KnightMovementBonus(id) + PyroMovementBonus(id) + ArcherDebuffMovementBonus(id) + BossBanishMovementBonus(id) + BossBindingMovementBonus(id) + CasterAuraMovementBonus(id) + ItemAuraMovementBonus(id) + SummonAbilityMovementBonus(id) + ItemStatusMovementBonus(id) + OrdinaryMovementBonus(id) + ItemScriptDebuffMovementBonus(id)));
            else profile.moveSpeed = ClampAbilityMoveSpeed(UnmodifiedNonHeroMovement(actor) *
                    (PoisonMovementMultiplier(id) + PyroMovementBonus(id) + ArcherDebuffMovementBonus(id) + BossBanishMovementBonus(id) + BossBindingMovementBonus(id) + CasterAuraMovementBonus(id) + ItemAuraMovementBonus(id) + SummonAbilityMovementBonus(id) + ItemStatusMovementBonus(id) + OrdinaryMovementBonus(id) + ItemScriptDebuffMovementBonus(id)));
            if (IsRingForcedActor(id) || IsShieldForceActive(id) || OrdinaryEntangled(id)) profile.moveSpeed = 0;
            if (!world.UpdateProfile(id, profile, actor.health, actor.mana)) throw new InvalidOperationException("poison-profile-unavailable:" + id);
        }
        void RemovePoison(int id)
        {
            if (!poisons.TryGetValue(id, out var poison)) return;
            poisons.Remove(id);
            RefreshAbilityMovement(id);
            RescaleWeaponRate(id, world.Clock);
            ReleaseAbilityMovementBase(id);
        }
        void AdvancePoisons()
        {
            double now = world.Clock;
            foreach (var poison in new List<PoisonState>(poisons.Values))
            {
                var target = world.UnitState(poison.target);
                if (target == null || target.health <= 0) { RemovePoison(poison.target); continue; }
                while (poison.nextTick <= now + 1e-9 && poison.nextTick < poison.expires - 1e-9)
                {
                    poison.nextTick += OriginalPoisonRules.TickInterval;
                    if (target.hidden || target.invulnerable) continue;
                    double damage = poison.damagePerTick * OriginalAttackRules.DamageTypeMultiplier(native, "spells", combatCatalog.Unit(target.rawcode).Text("defType"));
                    damage = IncomingMagicDamage(target, damage);
                    ApplyResolvedUnitHit(poison.source, poison.owner, target, damage);
                    target = world.UnitState(poison.target);
                    if (target == null || target.health <= 0) break;
                }
                if (now + 1e-9 >= poison.expires || target == null || target.health <= 0) RemovePoison(poison.target);
            }
        }
        double WeaponRate(OriginalWorldUnitView actor)
        {
            double rate = 1 - ItemScriptDebuffAttackSlow(actor.entityId) - OrdinaryAttackSlow(actor.entityId) - PoisonAttackSlow(actor.entityId) - ArcherDebuffAttackSlow(actor.entityId) + ArcherAttackSpeedBonus(actor.entityId) + KnightAttackSpeedBonus(actor.entityId);
            if (UsesHeroCombatStats(actor)) { var stats = CombatStatsFor(actor); rate += stats.agilityAttackSpeedBonus + stats.itemAttackSpeedBonus + stats.upgradeAttackSpeedBonus; }
            rate += BossAgilityAttackSpeedBonus(actor) + NativeEnduranceAttackSpeed(actor) + ItemAuraAttackSpeedBonus(actor.entityId) + WaveBloodlustAttackSpeed(actor.entityId) + SummonAbilityAttackSpeedBonus(actor.entityId);
            if (!OriginalCombatDefinition.IsFinite(rate)) throw new InvalidOperationException("nonfinite-weapon-rate");
            // CAPS3 complete positive cycles: H0081.85/9.249878=.2 and
            // hfoo1.35/6.74994=.2; AGI600/1000 both1.85/.369995=5.
            return Math.Max(.2, Math.Min(5,rate));
        }
        void RescaleWeaponRate(int id, double now)
        {
            var actor = world.UnitState(id);
            if (actor == null || !weaponCycles.TryGetValue(id, out var cycle) || cycle.rate <= 0) return;
            double next = WeaponRate(actor);
            if (next == cycle.rate) return;
            // Native THORNS_SLOW mid-cycle change preserves the completed
            // fraction, not the old release time or a new full cooldown.
            if (cycle.nextAttack > now) cycle.nextAttack = now + (cycle.nextAttack - now) * cycle.rate / next;
            if (cycle.winding && cycle.releaseAt > now) cycle.releaseAt = now + (cycle.releaseAt - now) * cycle.rate / next;
            cycle.rate = next;
        }
    }
}

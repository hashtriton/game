using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // hL2971..3003 exact flags, verified by CAST1 under1.26.0.6401.
        // Cache adc213f73a9b02fecdd8adc0e973176aa927212f82836bd9c0be8d031aa89848;
        // tools/arena/extract_observed_caster.py preserves all twelve hits.
        bool TriggeredDamageModeAvailable(OriginalTriggeredDamageMode mode) =>
            mode == OriginalTriggeredDamageMode.SpellNormal || mode == OriginalTriggeredDamageMode.SpellMagic ||
            mode == OriginalTriggeredDamageMode.ChaosUniversal;

        void ApplyTriggeredHit(int attacker, int owner, OriginalWorldUnitView target, double rawDamage, OriginalTriggeredDamageMode mode)
        {
            if (!OriginalCombatDefinition.IsFinite(rawDamage) || rawDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            if (!TriggeredDamageModeAvailable(mode)) throw new InvalidOperationException("native-triggered-damage-mode-unresolved:" + mode);
            var effect=PrepareItemSpellEffect(attacker,rawDamage);
            ApplyNativeTriggeredHit(attacker,owner,target,effect.damage,mode);
            CompleteItemSpellEffect(attacker,owner,effect);
        }

        void ApplyNativeTriggeredHit(int attacker, int owner, OriginalWorldUnitView target, double rawDamage, OriginalTriggeredDamageMode mode)
        {
            if (!OriginalCombatDefinition.IsFinite(rawDamage) || rawDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            if (!TriggeredDamageModeAvailable(mode)) throw new InvalidOperationException("native-triggered-damage-mode-unresolved:" + mode);
            target = target == null ? null : world.UnitState(target.entityId);
            if (target == null || target.health <= 0 || target.hidden || target.invulnerable) return;
            // ITEMFAM3 edry/hspt Amim intact/remove controls: MAGIC is
            // rejected without EVENT_UNIT_DAMAGED, not a zero-damage event.
            // The measured NORMAL and UNIVERSAL paths remain eligible.
            // ITEMSHELL2: AIxs blocks40/200 MAGIC with no damage callback.
            // Its native type flag and hostile control immunity were not tested.
            if (mode == OriginalTriggeredDamageMode.SpellMagic && (CasterMagicImmune(target)||ItemAntiMagicShellActive(target.entityId))) return;
            if(mode==OriginalTriggeredDamageMode.SpellNormal)
            {
                ReflectNativeCarapace(attacker,target,rawDamage);
                target=world.UnitState(target.entityId);
                if(target==null || target.health<=0 || target.hidden || target.invulnerable)return;
            }
            var definition = combatCatalog.Unit(target.rawcode);
            double damage;
            if (mode == OriginalTriggeredDamageMode.ChaosUniversal)
            {
                damage = rawDamage * OriginalAttackRules.DamageTypeMultiplier(native, "chaos", definition.Text("defType"));
                // AOmi DataC is retained as its declared general incoming
                // factor. Image UNIVERSAL interaction is not part of CAST1.
                damage = ImageIncomingDamage(target.entityId, damage) * ArcherIncomingDamageMultiplier(target.entityId);
            }
            else
            {
                damage = rawDamage * OriginalAttackRules.DamageTypeMultiplier(native, "spells", definition.Text("defType"));
                if (mode == OriginalTriggeredDamageMode.SpellNormal)
                {
                    double armor = UsesHeroCombatStats(target) ? CombatStatsFor(target).armor : EnemyArmor(definition, target.entityId);
                    damage *= OriginalAttackRules.ArmorMultiplier(native, armor);
                    // INDEF1 paired A15F: SPELLS/NORMAL40 on hfoo yields
                    // 7.142857 versus stripped35.714283. MAGIC/UNIVERSAL do
                    // not enter this physical-native factor.
                    damage = IncomingInnateWeaponDamage(target, damage);
                }
                damage = IncomingMagicDamage(target, damage);
            }
            if (HasBossBanish(target.entityId))
            {
                // BAND3: both40 CHAOS axes notify observers with1 but lose
                // zero life. The zero-health rule agrees with the declared
                // EtherealDamageBonus. Other positive magnitudes are derived.
                if (mode == OriginalTriggeredDamageMode.ChaosUniversal)
                { ApplyResolvedUnitHit(attacker, owner, target, 0, rawDamage > 0 ? 1 : 0); return; }
                damage *= 1.66;
            }
            ApplyResolvedUnitHit(attacker, owner, target, BossIncomingTriggeredDamage(target, mode, damage));
        }

        void ApplyTriggeredNormalHit(int attacker, int owner, OriginalWorldUnitView target, double rawDamage) =>
            ApplyTriggeredHit(attacker, owner, target, rawDamage, OriginalTriggeredDamageMode.SpellNormal);
    }
}

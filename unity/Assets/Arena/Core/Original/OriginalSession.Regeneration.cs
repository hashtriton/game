using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // Source regeneration rates are integrated in the host's bounded step.
        // Native sub-frame cadence is not inferred from a rate declaration.
        // REGENL1 cache cbd941979144c921ea546fdbe7316b80c26591765f3e3098956765a0b14f6520;
        // checked by tools/arena/extract_observed_regeneration.py.
        void AdvanceRegeneration(double seconds)
        {
            AdvanceBossManaAura(seconds);
            AdvanceNativeHealthAuras();
            foreach (var unit in world.Snapshot().units)
            {
                // REGENL1 separates base regeneration from attribute bonuses:
                // PauseUnit retains the former and suppresses the latter, even
                // after native unpaused updates. It does not stop all healing.
                if (unit.health <= 0) continue;
                var definition = combatCatalog.Unit(unit.rawcode);
                double healthRate = 0, manaRate = 0;
                if (definition.Text("regenType") == "always" && definition.TryNumber("regenHP", out double declaredHealth, out _)) healthRate = declaredHealth;
                if (definition.TryNumber("regenMana", out double declaredMana, out _)) manaRate = declaredMana;
                if (UsesHeroCombatStats(unit))
                {
                    double research = CombatStatsFor(unit).upgradeRegenPerSecond;
                    healthRate += research; manaRate += research;
                }
                var boss = BossIntrinsic(unit.rawcode);
                if (boss != null)
                {
                    // O006 L50 native: paused0/0, unpaused12.5/12.5. The sparse
                    // declaration regenMana=1 and regenType=none do not describe
                    // those actual rates. Use the measured base0/attribute split.
                    healthRate = manaRate = 0;
                    if (!unit.paused)
                    {
                        healthRate += boss.RequireStrength() * native.Constant("StrRegenBonus").Require();
                        manaRate += boss.RequireIntelligence() * native.Constant("IntRegenBonus").Require();
                    }
                }
                // LiAOmi1: while the real caster is hidden its attribute rates
                // vanish (base1.3/.05 remains). Images retain captured rates.
                if (UsesHeroCombatStats(unit) && !MirrorHiddenRegeneration(unit.entityId))
                {
                    var stats = CombatStatsFor(unit);
                    if (!unit.paused)
                    {
                        // ITEMCTL1 isolates H008 strength regeneration during
                        // B0BN: only the declared1.3HP/s base remains. Mana was
                        // full in this experiment, so its rate is not inferred.
                        if (!bossDooms.ContainsKey(unit.entityId)) healthRate += stats.strength * native.Constant("StrRegenBonus").Require();
                        manaRate += stats.intelligence * native.Constant("IntRegenBonus").Require();
                    }
                    healthRate += stats.itemHealthRegen;
                    // AImr adds a fraction of natural regeneration. Flat aura
                    // contributions are separate and must not be multiplied.
                    manaRate *= 1 + stats.itemManaRegenFraction;
                }
                // Sparse or conditional nonhero regeneration is outside this
                // implementation. A known negative rate must not silently turn
                // into zero before its source death attribution is integrated.
                if (healthRate < 0 || manaRate < 0) throw new InvalidOperationException("negative-regeneration-policy-unresolved:" + unit.rawcode);
                // MANAAURA2: A1D8 subtracts2% maximum mana per second,
                // independently of attribute/item regeneration and target pause.
                // Validate the ordinary rates before applying this known drain.
                manaRate += BossManaAuraRate(unit);
                healthRate += ItemAuraHealthRegen(unit.entityId, unit.profile.maxHealth);
                healthRate += NativeHealthAuraRate(unit);
                manaRate += ItemAuraManaRegen(unit.entityId, unit.profile.maxMana);
                double health = Math.Min(unit.profile.maxHealth, unit.health + healthRate * seconds);
                double mana = Math.Max(0, Math.Min(unit.profile.maxMana, unit.mana + manaRate * seconds));
                if (health != unit.health || mana != unit.mana) world.UpdateProfile(unit.entityId, unit.profile, health, mana);
            }
        }
    }
}

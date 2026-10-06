using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalItemVitalityStep
    {
        public string abilityId;
        public int occurrence;
        public double healthMaximumDelta, manaMaximumDelta;
        public double health, maxHealth, mana, maxMana;
    }

    public sealed class OriginalItemVitalityResult
    {
        public bool known;
        public bool approximate;
        public string unresolved;
        public double health, maxHealth, mana, maxMana;
        public OriginalItemVitalityStep[] steps = Array.Empty<OriginalItemVitalityStep>();
        public string evidence = "Derived sequential occurrence float-delta ratio; LiAEquip1 344/344 and LiAItemR1 23/23 item changes match. General native arithmetic and mixed-set ordering remain approximate; A001 learning is a different operation.";
        public OriginalItemVitalityResult Require()
        {
            if (!known) throw new InvalidOperationException(unresolved ?? "Item vitality transition is unresolved.");
            return this;
        }
    }

    public sealed partial class OriginalInventoryEffects
    {
        sealed class VitalityContribution
        {
            internal string ability;
            internal int occurrence;
            internal double strength, intelligence, health, mana;
        }
        readonly Dictionary<string, VitalityContribution[]> vitalitySteps = new Dictionary<string, VitalityContribution[]>(StringComparer.Ordinal);

        void SaveVitalitySteps(OriginalItemEffectDefinition item, IList<OriginalStatModifier> modifiers)
        {
            var steps = new List<VitalityContribution>();
            var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var ability in item.abilityIds)
            {
                occurrences.TryGetValue(ability, out var occurrence); occurrences[ability] = occurrence + 1;
                var step = new VitalityContribution { ability = ability, occurrence = occurrence };
                foreach (var modifier in modifiers)
                {
                    if (modifier.AbilityId != ability || modifier.Occurrence != occurrence) continue;
                    switch (modifier.Stat)
                    {
                        case "strength": step.strength += modifier.Value; break;
                        case "intelligence": step.intelligence += modifier.Value; break;
                        case "maxHealth": step.health += modifier.Value; break;
                        case "maxMana": step.mana += modifier.Value; break;
                    }
                }
                steps.Add(step);
            }
            vitalitySteps.Add(item.id, steps.ToArray());
        }

        // The observed sequence changes current resources once per ability
        // occurrence, not once for the final inventory maximum. Removal uses
        // declaration order: both orders fit all current observations, so this
        // order and generalization to mixed sets are explicitly derived.
        // Unresolved transitions return the original state, never a prefix.
        public OriginalItemVitalityResult ChangeVitality(string itemId, bool adding,
            double health, double maxHealth, double mana, double maxMana, double strHealth, double intMana, string primaryAttribute = null)
        {
            if (!Rawcode(itemId) || !Finite(health) || !Finite(maxHealth) || !Finite(mana) || !Finite(maxMana) ||
                health < 0 || maxHealth <= 0 || health > maxHealth || mana < 0 || maxMana < 0 || mana > maxMana ||
                !Finite(strHealth) || strHealth < 0 || !Finite(intMana) || intMana < 0)
                throw new ArgumentException("Invalid item vitality state or attribute coefficients.");
            var result = new OriginalItemVitalityResult { health = health, maxHealth = maxHealth, mana = mana, maxMana = maxMana };
            if (!vitalitySteps.TryGetValue(itemId, out var source))
            {
                result.unresolved = "item-native-profile-unresolved:" + itemId;
                return result;
            }
            var outputs = new List<OriginalItemVitalityStep>();
            var hp = health; var hpMax = maxHealth; var mp = mana; var mpMax = maxMana;
            foreach (var step in source)
            {
                // YU 11825..11836 removes both non-primary attribute abilities.
                // Applying only the surviving intrinsic contributions avoids
                // false maxima. Intermediate add/remove rounding is derived,
                // not measured by the original-handler-free equip probe.
                if (itemId == "I05P" && primaryAttribute != null &&
                    (step.ability == "A11P" && primaryAttribute != "STR" ||
                     step.ability == "A11Q" && primaryAttribute != "AGI" ||
                     step.ability == "A11R" && primaryAttribute != "INT")) continue;
                var sign = adding ? 1 : -1;
                var hpDelta = sign * (step.health + step.strength * strHealth);
                var mpDelta = sign * (step.mana + step.intelligence * intMana);
                result.approximate |= hpDelta != 0 || mpDelta != 0;
                if (!Transition(hp, hpMax, hpDelta, true, out var nextHp, out var nextHpMax, out var reason) ||
                    !Transition(mp, mpMax, mpDelta, false, out var nextMp, out var nextMpMax, out reason))
                {
                    result.unresolved = reason + ":" + itemId + ":" + step.ability + ":" + step.occurrence;
                    return result;
                }
                hp = nextHp; hpMax = nextHpMax; mp = nextMp; mpMax = nextMpMax;
                outputs.Add(new OriginalItemVitalityStep { abilityId = step.ability, occurrence = step.occurrence,
                    healthMaximumDelta = hpDelta, manaMaximumDelta = mpDelta,
                    health = hp, maxHealth = hpMax, mana = mp, maxMana = mpMax });
            }
            result.known = true; result.health = hp; result.maxHealth = hpMax; result.mana = mp; result.maxMana = mpMax;
            result.steps = outputs.ToArray();
            return result;
        }

        static bool Transition(double current, double maximum, double delta, bool health,
            out double next, out double nextMaximum, out string unresolved)
        {
            next = current; nextMaximum = maximum; unresolved = null;
            // Native ordinary damage/armor/IAS items do not round resources.
            if (delta == 0) return true;
            nextMaximum = maximum + delta;
            if (!Finite(delta) || !Finite(nextMaximum) || maximum <= 0 || nextMaximum <= 0)
            {
                unresolved = "item-vitality-zero-or-invalid-maximum-unresolved"; return false;
            }
            // A data-selected approximation, not recovered engine code. The
            // float32 proportional increment reproduces both measured 822
            // half-ties and the 1046 controls, without a per-state lookup.
            var scaled = current + current * (double)(float)(delta / maximum);
            next = Math.Floor(scaled + .5);
            // LiAItemR1 11/11 native add/remove observations keep a living
            // unit at one HP when proportional rounding would reach zero.
            // The probe starts above Warcraft's .405 alive threshold; it
            // does not authorize reviving an already dead native unit.
            if (health && current > .405) next = Math.Max(1, next);
            if (!Finite(scaled) || !Finite(next) || next > nextMaximum || (health && (current <= .405 || next <= 0)))
            {
                unresolved = "item-vitality-zero-or-cap-rounding-unresolved"; return false;
            }
            return true;
        }
    }
}

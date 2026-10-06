using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalSessionSkillView
    {
        public string id, name;
        public int rank, maximumRank, requiredLevel;
        public bool requiredLevelKnown;
        public OriginalLearnCode code;
    }
    [Serializable] public sealed class OriginalLearnedAbilityView { public string id; public int rank; }

    public sealed partial class OriginalSession
    {
        OriginalNativeCatalog progressionNative;
        OriginalObservedCatalog progressionObserved;
        sealed class StartingProgression
        {
            internal OriginalHeroProgression progression;
            internal OriginalHeroStatsSnapshot stats;
        }

        public void ConfigureProgression(OriginalNativeCatalog nativeCatalog, OriginalObservedCatalog observedCatalog)
        {
            if (Started) throw new InvalidOperationException("Configure progression before starting.");
            if (nativeCatalog == null || observedCatalog == null) throw new ArgumentNullException();
            nativeCatalog.BuildIndexes(); observedCatalog.BuildIndexes();
            progressionNative = nativeCatalog; progressionObserved = observedCatalog;
        }

        StartingProgression[] CreateStartingProgressions()
        {
            var result = new StartingProgression[players.Count];
            if (progressionNative == null) return result;
            for (int i = 0; i < result.Length; i++)
            {
                var progression = new OriginalHeroProgression(combatCatalog, progressionNative, players[i].hero, progressionObserved);
                result[i] = new StartingProgression { progression = progression, stats = CalculateProgressionStats(players[i].hero, progression) };
            }
            return result;
        }

        OriginalSessionReplyCode LearnSkill(Player player, string skillId)
        {
            if (!Started || player.progression == null || pendingDuel ||
                !(DuelActive ? duel.Snapshot().alive[player.matchSlot] : match.IsAlive(player.matchSlot)) ||
                match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            if (skillId == null || skillId.Length != 4) return OriginalSessionReplyCode.InvalidCommand;
            var candidate = player.progression.Copy();
            var learned = candidate.TryLearn(skillId);
            if (learned.code != OriginalLearnCode.Learned)
                return learned.code == OriginalLearnCode.UnknownSkill ? OriginalSessionReplyCode.InvalidCommand :
                    learned.code == OriginalLearnCode.RuleUnavailable ? OriginalSessionReplyCode.RuleUnavailable : OriginalSessionReplyCode.NotReady;
            try
            {
                var stats = CalculateProgressionStats(player.hero, candidate);
                var abilities = new SortedDictionary<string, int>(player.auxiliaryAbilities, StringComparer.Ordinal);
                bool handler = player.archerAttackHandlerRegistered; int element = player.bowElement;
                foreach (var change in learned.mutations)
                    switch (change.kind)
                    {
                        case OriginalLearnMutationKind.AddAbility: abilities[change.abilityId] = change.value; break;
                        case OriginalLearnMutationKind.SetExistingAbilityRank:
                            if (abilities.ContainsKey(change.abilityId)) abilities[change.abilityId] = change.value;
                            break;
                        case OriginalLearnMutationKind.RegisterArcherAttackHandler: handler = true; break;
                        case OriginalLearnMutationKind.SetBowElement: element = change.value; break;
                        default: throw new InvalidOperationException("Unknown source learning mutation.");
                    }
                abilities = EquipmentAuxiliaries(player, player.inventory, candidate, abilities);
                ApplyProgressionProfile(player, stats, candidate);
                player.progression = candidate; player.stats = stats; player.auxiliaryAbilities = abilities;
                player.archerAttackHandlerRegistered = handler; player.bowElement = element;
                return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { return OriginalSessionReplyCode.RuleUnavailable; }
        }

        void SyncProgressionExperience(Player player)
        {
            if (player.progression == null || HaltReason != null) return;
            try
            {
                var candidate = player.progression.Copy();
                int previousLevel = candidate.Level;
                candidate.SyncMatchExperience(match.Experience(player.matchSlot));
                var stats = CalculateProgressionStats(player.hero, candidate);
                ApplyProgressionProfile(player, stats, candidate);
                player.progression = candidate; player.stats = stats;
                if (candidate.Level != previousLevel) LearnDisconnectedSkill(player);
            }
            catch (InvalidOperationException exception) { HaltReason = "progression-rule-unavailable:" + exception.Message; }
        }

        void ApplyProgressionProfile(Player player, OriginalHeroStatsSnapshot stats, OriginalHeroProgression candidate = null)
        {
            if (world == null) return;
            stats = ComposeEquipment(stats, player.inventory);
            int id = OriginalWorld.HeroEntityId(player.slot);
            var unit = world.UnitState(id);
            if (unit == null) throw new InvalidOperationException("Hero world entity is missing.");
            var profile = new OriginalWorldUnitProfile { collisionRadius = unit.profile.collisionRadius,
                moveSpeed = ResolveAbilityMoveSpeed(player.slot, stats.baseMoveSpeed, candidate),
                maxHealth = stats.maxHealth.Require(), maxMana = stats.maxMana.Require() };
            string policy = null;
            if (unit.profile.maxHealth != profile.maxHealth || unit.profile.maxMana != profile.maxMana)
            {
                bool levelChanged = stats.level != player.progression.Level;
                var observation = progressionObserved.VitalityChange(player.hero, levelChanged ? "level-up" : "learn-A001");
                if (observation.policyKnown) policy = observation.policy;
                else if (!levelChanged && candidate != null && observation.policy == "ratio-rounding-unresolved")
                    // Native A001 preserves the resource fraction. Half-tie
                    // arithmetic differs by one point across measured cases;
                    // keep this explicit approximation separate from evidence.
                    policy = "ratio-nearest-approximation";
                else throw new InvalidOperationException(observation.policy);
            }
            double health = UpdatedVitality(unit.health, unit.profile.maxHealth, profile.maxHealth, policy, true);
            double mana = UpdatedVitality(unit.mana, unit.profile.maxMana, profile.maxMana, policy, false);
            if (!world.UpdateProfile(id, profile, health, mana)) throw new InvalidOperationException("Hero profile placement is unavailable.");
        }

        static double UpdatedVitality(double current, double beforeMaximum, double afterMaximum, string policy, bool isHealth)
        {
            if (beforeMaximum == afterMaximum) return current;
            // Gaining a level cannot revive a dead actor. Resurrection belongs
            // to the altar/duel referee and has its own explicit world operation.
            if (isHealth && current <= 0) return 0;
            if (policy == "ratio-nearest-approximation")
            {
                double scaled = beforeMaximum > 0 ? current / beforeMaximum * afterMaximum : 0;
                return Math.Min(afterMaximum, Math.Max(isHealth ? 1 : 0, Math.Floor(scaled + .5)));
            }
            // Native level-up measurements preserve the absolute deficit.
            if (policy != "preserve-deficit") throw new InvalidOperationException("native-vitality-change-policy-unresolved:" + policy);
            return Math.Max(0, Math.Min(afterMaximum, current + afterMaximum - beforeMaximum));
        }

        // The weapon consumer is another host-owned Session partial. The value
        // never escapes into a client command or the presentation's catalogs.
        OriginalHeroStatsSnapshot HeroCombatStats(int lobbySlot)
        {
            var player = players.Find(p => p.slot == lobbySlot);
            if (player == null) throw new InvalidOperationException("Hero slot is missing.");
            return ApplyRunePowerupStats(lobbySlot,ApplyArcherStats(lobbySlot, ApplyDuelPressureStats(lobbySlot, ComposeEquipment(player.stats ?? OriginalHeroStats.Calculate(combatCatalog, player.hero, 1, progressionObserved), player.inventory))));
        }

        OriginalHeroStatsSnapshot CalculateProgressionStats(string heroId, OriginalHeroProgression progression)
        {
            var state = progression.Snapshot();
            var result = OriginalHeroStats.Calculate(combatCatalog, heroId, state.level, progressionObserved);
            int rank = 0;
            foreach (var skill in state.skills) if (skill.id == "A001") rank = skill.rank;
            if (rank > 0)
            {
                if (!state.attributeBonus.known) throw new InvalidOperationException(state.attributeBonus.unresolved);
                var bonus = state.attributeBonus;
                var effect = Array.Find(progressionObserved.Skill(heroId, "A001").rankEffects, e => e.rank == rank && e.known);
                if (effect == null) throw new InvalidOperationException("A001 rank effect not observed.");
                result.strength = Plus(result.strength, bonus.strength);
                result.agility = Plus(result.agility, bonus.agility);
                result.intelligence = Plus(result.intelligence, bonus.intelligence);
                result.primary = result.primaryAttribute == "STR" ? result.strength : result.primaryAttribute == "AGI" ? result.agility : result.intelligence;
                result.maxHealth = Plus(result.maxHealth, effect.maxHPBonus); result.maxMana = Plus(result.maxMana, effect.maxMPBonus);
                result.armor = Plus(result.armor, bonus.agility * ProgressionConstant("AgiDefenseBonus"));
                result.agilityAttackSpeedBonus = Plus(result.agilityAttackSpeedBonus, bonus.agility * ProgressionConstant("AgiAttackSpeedBonus"));
                double primaryBonus = result.primaryAttribute == "STR" ? bonus.strength : result.primaryAttribute == "AGI" ? bonus.agility : bonus.intelligence;
                double damageBonus = primaryBonus * ProgressionConstant("StrAttackBonus");
                result.primaryDamageBonus = Plus(result.primaryDamageBonus, damageBonus);
                result.attackMinimum = Plus(result.attackMinimum, damageBonus); result.attackMaximum = Plus(result.attackMaximum, damageBonus);
            }
            // Unknown derived stats must fail before XP/skill-point publication.
            result.maxHealth.Require(); result.maxMana.Require(); result.armor.Require();
            result.primaryDamageBonus.Require(); result.agilityAttackSpeedBonus.Require();
            return result;
        }

        double ProgressionConstant(string key)
        {
            OriginalCombatConstant found = null;
            foreach (var constant in combatCatalog.constants)
                if (constant.key == key)
                {
                    if (found != null) throw new InvalidOperationException("Duplicate progression constant " + key);
                    found = constant;
                }
            if (found == null || found.values == null || found.values.Length != 1 || !OriginalCombatDefinition.IsFinite(found.values[0]))
                throw new InvalidOperationException("Unknown progression constant " + key);
            return found.values[0];
        }
        static OriginalHeroStatValue Plus(OriginalHeroStatValue value, double extra)
        {
            double result = value.Require() + extra;
            if (!OriginalCombatDefinition.IsFinite(result)) throw new InvalidOperationException("Non-finite learned hero stat.");
            return new OriginalHeroStatValue { known = true, value = result };
        }

        OriginalSessionSkillView[] LearningView(Player player)
        {
            if (player.progression == null) return Array.Empty<OriginalSessionSkillView>();
            var skills = player.progression.Snapshot().skills;
            var result = new OriginalSessionSkillView[skills.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                var choice = player.progression.CanLearn(skills[i].id);
                result[i] = new OriginalSessionSkillView { id = skills[i].id, name = combatCatalog.Ability(ActualPyroAbility(player, skills[i].id)).name,
                    rank = skills[i].rank, maximumRank = skills[i].maximumRank, code = choice.code,
                    requiredLevelKnown = choice.requiredLevelKnown, requiredLevel = choice.requiredLevel };
            }
            return result;
        }
        static OriginalLearnedAbilityView[] AuxiliaryView(Player player)
        {
            var result = new OriginalLearnedAbilityView[player.auxiliaryAbilities.Count]; int i = 0;
            foreach (var ability in player.auxiliaryAbilities) result[i++] = new OriginalLearnedAbilityView { id = ability.Key, rank = ability.Value };
            return result;
        }
    }
}

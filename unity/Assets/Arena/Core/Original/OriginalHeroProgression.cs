using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalLearnCode { Learned, Available, UnknownSkill, MaximumRank, NoSkillPoints, HeroLevelTooLow, RuleUnavailable }
    public enum OriginalLearnMutationKind { AddAbility, SetExistingAbilityRank, RegisterArcherAttackHandler, SetBowElement }
    [Serializable] public sealed class OriginalLearnMutation { public OriginalLearnMutationKind kind; public string abilityId; public int value; public int sourceLine; }
    [Serializable] public sealed class OriginalLearnResult
    {
        public OriginalLearnCode code; public string skillId; public int rank; public bool requiredLevelKnown; public int requiredLevel;
        public OriginalLearnMutation[] mutations = Array.Empty<OriginalLearnMutation>();
    }
    [Serializable] public sealed class OriginalSkillRank { public string id; public int rank; public int maximumRank; }
    [Serializable] public sealed class OriginalAttributeBonus { public bool known; public double strength; public double agility; public double intelligence; public string unresolved; }
    [Serializable] public sealed class OriginalHeroProgressionSnapshot
    {
        public string heroId; public int experience; public int level; public int unspentSkillPoints; public long matchExperienceWatermark;
        public OriginalSkillRank[] skills; public OriginalAttributeBonus attributeBonus;
    }
    public sealed class OriginalHeroProgression
    {
        sealed class Rule { public string id; public int maximum, first, skip, rank; public bool skipKnown; public int[] observedLevels; }
        readonly string heroId;
        readonly Rule[] rules;
        readonly Dictionary<string, Rule> byId = new Dictionary<string, Rule>(StringComparer.Ordinal);
        readonly int[] thresholds;
        readonly OriginalAttributeBonus[] attributeBonuses;
        long matchExperienceWatermark;
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int UnspentSkillPoints { get; private set; } = 1;

        // Stage a host transaction on independent state. No catalog re-read can
        // change the candidate's already validated rules or measured thresholds.
        public OriginalHeroProgression Copy() => new OriginalHeroProgression(this);

        private OriginalHeroProgression(OriginalHeroProgression original)
        {
            heroId = original.heroId;
            Level = original.Level; Experience = original.Experience;
            UnspentSkillPoints = original.UnspentSkillPoints;
            matchExperienceWatermark = original.matchExperienceWatermark;
            thresholds = (int[])original.thresholds.Clone();
            rules = new Rule[original.rules.Length];
            for (int i = 0; i < rules.Length; i++)
            {
                var before = original.rules[i];
                var after = new Rule { id = before.id, maximum = before.maximum, first = before.first,
                    skip = before.skip, rank = before.rank, skipKnown = before.skipKnown,
                    observedLevels = before.observedLevels == null ? null : (int[])before.observedLevels.Clone() };
                rules[i] = after; byId.Add(after.id, after);
            }
            attributeBonuses = new OriginalAttributeBonus[original.attributeBonuses.Length];
            for (int i = 0; i < attributeBonuses.Length; i++)
            {
                var before = original.attributeBonuses[i];
                attributeBonuses[i] = new OriginalAttributeBonus { known = before.known, strength = before.strength,
                    agility = before.agility, intelligence = before.intelligence, unresolved = before.unresolved };
            }
        }

        // Native skill points: classic.battle.net/war3/basics/heroes.shtml,
        // Ability Tree and Levels. Map learning limits supersede stock limits.
        // This object owns progression only. The host validates actor, phase,
        // life state and the source of each XP award before calling it.
        public OriginalHeroProgression(OriginalCombatCatalog combat, OriginalNativeCatalog native, string heroId, OriginalObservedCatalog observed = null)
        {
            if (combat == null || native == null) throw new ArgumentNullException();
            if (heroId != "H008" && heroId != "N0A0" && heroId != "H024") throw new ArgumentException("Unsupported hero.");
            combat.BuildIndexes(); native.BuildIndexes();
            observed?.BuildIndexes();
            if (combat.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected combat source.");
            this.heroId = heroId;
            thresholds = new int[native.heroXp.levels.Length];
            for (var i = 0; i < thresholds.Length; i++) thresholds[i] = native.heroXp.levels[i].cumulative;
            OriginalHeroDefinition hero = null;
            foreach (var candidate in combat.selectedHeroes) if (candidate.id == heroId) hero = candidate;
            if (hero == null || hero.skills.Length != 5) throw new ArgumentException("Expected five source skills.");
            rules = new Rule[5];
            for (var i = 0; i < rules.Length; i++)
            {
                var id = hero.skills[i]; var skill = combat.Ability(id);
                // Restrict helper dispatch to the selected source skill IDs.
                if (!OriginalLearnEffects.IsSelectedSkill(id)) throw new ArgumentException("Unexpected hero skill.");
                var rule = new Rule { id = id, maximum = Integer(skill.Number("levels"), 1, 50), first = Integer(skill.Number("reqLevel"), 1, 50) };
                if (skill.TryNumber("levelSkip", out var skip, out _))
                {
                    rule.skip = Integer(skip, 0, 50);
                    // A zero does not establish the effective engine interval.
                    // Until measured it remains unknown, like an absent cell.
                    rule.skipKnown = rule.skip > 0;
                }
                foreach (var field in skill.fields)
                    if (field.key == "levelSkip" && (field.conflict || !field.isNumber))
                        throw new InvalidOperationException("Conflicting levelSkip: " + id);
                if (observed != null)
                {
                    var measured = observed.Skill(heroId, id);
                    if (!measured.known || measured.maximumRank != rule.maximum) throw new InvalidOperationException("Incompatible measured skill: " + id);
                    rule.observedLevels = (int[])measured.minimumHeroLevels.Clone();
                }
                rules[i] = rule; byId.Add(id, rule);
            }
            if (!byId.ContainsKey("A001")) throw new ArgumentException("Missing attribute skill.");
            var attribute = combat.Ability("A001");
            attributeBonuses = new OriginalAttributeBonus[byId["A001"].maximum + 1];
            attributeBonuses[0] = new OriginalAttributeBonus { known = true };
            for (var rank = 1; rank < attributeBonuses.Length; rank++)
            {
                var bonus = new OriginalAttributeBonus();
                // Aamk metadata: Iagi=DataA, Iint=DataB, Istr=DataC.
                bonus.known = attribute.TryNumber("DataA" + rank, out bonus.agility, out _) &
                    attribute.TryNumber("DataB" + rank, out bonus.intelligence, out _) &
                    attribute.TryNumber("DataC" + rank, out bonus.strength, out _);
                if (!bonus.known) bonus.unresolved = "A001 rank " + rank + " attribute data is unresolved.";
                if (observed != null)
                {
                    var measured = Array.Find(observed.Skill(heroId, "A001").rankEffects, effect => effect.rank == rank && effect.known);
                    if (measured != null) bonus = new OriginalAttributeBonus { known = true, strength = measured.strengthBonus,
                        agility = measured.agilityBonus, intelligence = measured.intelligenceBonus };
                }
                attributeBonuses[rank] = bonus;
            }
        }

        static int Integer(double value, int minimum, int maximum)
        {
            if (!OriginalCombatDefinition.IsFinite(value) || value != Math.Floor(value) || value < minimum || value > maximum)
                throw new ArgumentException("Invalid progression integer.");
            return (int)value;
        }

        public int GrantExperience(int awardedXp)
        {
            if (awardedXp < 0) throw new ArgumentOutOfRangeException(nameof(awardedXp));
            return Grant(awardedXp);
        }

        // Match.Experience(slot) is a cumulative award ledger, not current XP.
        // Call only on the host, instead of also granting Match Experience
        // events. Item/script awards use GrantExperience independently.
        public int SyncMatchExperience(long cumulative)
        {
            if (cumulative < matchExperienceWatermark) throw new ArgumentOutOfRangeException(nameof(cumulative));
            var accepted = Grant(cumulative - matchExperienceWatermark);
            matchExperienceWatermark = cumulative;
            return accepted;
        }

        int Grant(long awarded)
        {
            var accepted = (int)Math.Min(awarded, thresholds[thresholds.Length - 1] - Experience);
            Experience += accepted;
            var before = Level;
            while (Level < thresholds.Length && Experience >= thresholds[Level]) Level++;
            UnspentSkillPoints += Level - before;
            return accepted;
        }

        public OriginalLearnResult CanLearn(string skillId)
        {
            var result = new OriginalLearnResult { skillId = skillId };
            if (skillId == null || !byId.TryGetValue(skillId, out var rule)) { result.code = OriginalLearnCode.UnknownSkill; return result; }
            result.rank = rule.rank + 1;
            result.requiredLevelKnown = rule.rank == 0 || rule.skipKnown || rule.observedLevels != null;
            if (result.requiredLevelKnown) result.requiredLevel = rule.observedLevels != null && rule.rank < rule.maximum ?
                rule.observedLevels[rule.rank] : rule.first + rule.rank * rule.skip;
            result.code = rule.rank >= rule.maximum ? OriginalLearnCode.MaximumRank :
                UnspentSkillPoints == 0 ? OriginalLearnCode.NoSkillPoints :
                !result.requiredLevelKnown ? OriginalLearnCode.RuleUnavailable :
                Level < result.requiredLevel ? OriginalLearnCode.HeroLevelTooLow : OriginalLearnCode.Available;
            return result;
        }

        public OriginalLearnResult TryLearn(string skillId)
        {
            var result = CanLearn(skillId);
            if (result.code != OriginalLearnCode.Available) return result;
            result.mutations = OriginalLearnEffects.For(skillId, result.rank);
            byId[skillId].rank = result.rank;
            UnspentSkillPoints--;
            result.code = OriginalLearnCode.Learned;
            return result;
        }

        public OriginalHeroProgressionSnapshot Snapshot()
        {
            var skills = new OriginalSkillRank[rules.Length];
            for (var i = 0; i < skills.Length; i++) skills[i] = new OriginalSkillRank { id = rules[i].id, rank = rules[i].rank, maximumRank = rules[i].maximum };
            var bonus = attributeBonuses[byId["A001"].rank];
            return new OriginalHeroProgressionSnapshot { heroId = heroId, experience = Experience, level = Level,
                unspentSkillPoints = UnspentSkillPoints, matchExperienceWatermark = matchExperienceWatermark, skills = skills,
                attributeBonus = new OriginalAttributeBonus { known = bonus.known, strength = bonus.strength, agility = bonus.agility, intelligence = bonus.intelligence, unresolved = bonus.unresolved } };
        }
    }
    public static class OriginalLearnEffects
    {
        internal static bool IsSelectedSkill(string id) => id == "A001" || id == "A05N" || id == "A05M" || id == "A102" || id == "A0E6" ||
            id == "A15W" || id == "A0AS" || id == "A15X" || id == "A0AC" || id == "A0SJ" || id == "A0SP" || id == "A0AE" || id == "A0SM";

        // aFe/aEe/aDe:83481-83489,82992-83017,83459-83468.
        // These are ordered trigger requests, not native spell effects. The
        // consumer must treat SetExistingAbilityRank as a no-op if absent.
        public static OriginalLearnMutation[] For(string skillId, int rank, bool illusion = false)
        {
            if (!IsSelectedSkill(skillId)) throw new ArgumentException("Unsupported source skill.");
            if (rank < 1 || rank > (skillId == "A001" ? 15 : 3)) throw new ArgumentOutOfRangeException(nameof(rank));
            if (illusion) return Array.Empty<OriginalLearnMutation>();
            if (skillId == "A0AC") return new[] { Mutation(rank == 1 ? OriginalLearnMutationKind.AddAbility : OriginalLearnMutationKind.SetExistingAbilityRank, "A0N6", rank, rank == 1 ? 83463 : 83465) };
            if (skillId != "A15X") return Array.Empty<OriginalLearnMutation>();
            if (rank == 1) return new[] {
                Mutation(OriginalLearnMutationKind.AddAbility, "A15Z", 1, 83000),
                Mutation(OriginalLearnMutationKind.RegisterArcherAttackHandler, null, 1, 83002),
                Mutation(OriginalLearnMutationKind.SetBowElement, null, 1, 83005) };
            var ids = new[] { "A15Z", "A160", "A161", "A162", "A17M" };
            var result = new OriginalLearnMutation[ids.Length];
            for (var i = 0; i < ids.Length; i++) result[i] = Mutation(OriginalLearnMutationKind.SetExistingAbilityRank, ids[i], rank, 83007 + i);
            return result;
        }
        static OriginalLearnMutation Mutation(OriginalLearnMutationKind kind, string id, int value, int line) =>
            new OriginalLearnMutation { kind = kind, abilityId = id, value = value, sourceLine = line };
    }
}

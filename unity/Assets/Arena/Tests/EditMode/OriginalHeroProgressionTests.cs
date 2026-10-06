using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalHeroProgressionTests
    {
        static OriginalCombatCatalog Combat() => JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));
        static OriginalNativeCatalog Native() => JsonUtility.FromJson<OriginalNativeCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-native126.json")));
        static OriginalHeroProgression Hero(string id = "H008") => new OriginalHeroProgression(Combat(), Native(), id);
        [Test] public void CopyPreservesObservedRulesRanksBonusesAndXpWatermarkWhileChangesStayIndependent()
        {
            var observed = JsonUtility.FromJson<OriginalObservedCatalog>(File.ReadAllText(
                Path.Combine(Application.dataPath, "Arena/Data/lia39-observed126.json")));
            var original = new OriginalHeroProgression(Combat(), Native(), "H008", observed);
            original.SyncMatchExperience(17000); original.GrantExperience(55);
            for (int i = 0; i < 6; i++) Assert.That(original.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(original.TryLearn("A05N").code, Is.EqualTo(OriginalLearnCode.Learned));
            var candidate = original.Copy();
            Assert.That(candidate.Level, Is.EqualTo(original.Level));
            Assert.That(candidate.UnspentSkillPoints, Is.EqualTo(original.UnspentSkillPoints));
            Assert.That(candidate.Snapshot().attributeBonus.strength, Is.EqualTo(12));
            Assert.That(candidate.Snapshot().attributeBonus.known, Is.True);
            Assert.That(candidate.SyncMatchExperience(17000), Is.Zero);
            Assert.That(candidate.Experience, Is.EqualTo(17055));
            Assert.That(candidate.CanLearn("A05N").requiredLevelKnown, Is.True);
            Assert.That(candidate.CanLearn("A05N").requiredLevel, Is.EqualTo(3));
            Assert.That(candidate.TryLearn("A05N").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(candidate.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(candidate.SyncMatchExperience(17500), Is.EqualTo(500));
            Assert.That(candidate.Snapshot().attributeBonus.strength, Is.EqualTo(14));
            Assert.That(original.Experience, Is.EqualTo(17055));
            Assert.That(original.Snapshot().matchExperienceWatermark, Is.EqualTo(17000));
            Assert.That(original.Snapshot().skills.Single(x => x.id == "A05N").rank, Is.EqualTo(1));
            Assert.That(original.Snapshot().attributeBonus.strength, Is.EqualTo(12));
            Assert.That(original.UnspentSkillPoints, Is.EqualTo(11));
            Assert.That(original.SyncMatchExperience(17500), Is.EqualTo(500));
            Assert.That(candidate.SyncMatchExperience(17500), Is.Zero);
        }
        [Test] public void EverySelectedHeroStartsWithOnePointAndFiveUnlearnedSkills()
        {
            foreach (var id in new[] { "H008", "N0A0", "H024" })
            {
                var h = Hero(id); var s = h.Snapshot();
                Assert.That(s.heroId, Is.EqualTo(id)); Assert.That(h.Level, Is.EqualTo(1));
                Assert.That(h.Experience, Is.Zero); Assert.That(h.UnspentSkillPoints, Is.EqualTo(1));
                Assert.That(s.skills.Length, Is.EqualTo(5)); Assert.That(s.skills.All(x => x.rank == 0), Is.True);
                Assert.That(s.attributeBonus.known, Is.True); Assert.That(s.attributeBonus.strength, Is.Zero);
            }
        }
        [Test] public void XpBoundariesAwardEveryCrossedLevelAndStopAtMapMaximum()
        {
            var h = Hero(); Assert.That(h.GrantExperience(199), Is.EqualTo(199)); Assert.That(h.Level, Is.EqualTo(1));
            h.GrantExperience(1); Assert.That(h.Level, Is.EqualTo(2)); Assert.That(h.UnspentSkillPoints, Is.EqualTo(2));
            h.GrantExperience(340); Assert.That(h.Level, Is.EqualTo(3));
            Assert.That(h.GrantExperience(int.MaxValue), Is.EqualTo(127400 - 540));
            Assert.That(h.Level, Is.EqualTo(50)); Assert.That(h.UnspentSkillPoints, Is.EqualTo(50));
            Assert.That(h.GrantExperience(int.MaxValue), Is.Zero); Assert.That(h.Experience, Is.EqualTo(127400));
            Assert.Throws<ArgumentOutOfRangeException>(() => h.GrantExperience(-1));
        }
        [Test] public void MatchCumulativeExperienceIsIdempotentAndIndependentOfDirectAwards()
        {
            var h = Hero(); h.SyncMatchExperience(150); h.GrantExperience(200); h.SyncMatchExperience(150);
            Assert.That(h.Experience, Is.EqualTo(350)); h.SyncMatchExperience(300); Assert.That(h.Experience, Is.EqualTo(500));
            Assert.Throws<ArgumentOutOfRangeException>(() => h.SyncMatchExperience(299));
            Assert.That(h.Snapshot().matchExperienceWatermark, Is.EqualTo(300));
            h.SyncMatchExperience(long.MaxValue); Assert.That(h.Experience, Is.EqualTo(127400));
            Assert.That(h.SyncMatchExperience(long.MaxValue), Is.Zero);
        }
        [Test] public void FirstRankUsesMapRequiredLevelAndSpendsOnePointAtomically()
        {
            var h = Hero(); Assert.That(h.TryLearn("A05M").code, Is.EqualTo(OriginalLearnCode.HeroLevelTooLow));
            Assert.That(h.TryLearn("A15W").code, Is.EqualTo(OriginalLearnCode.UnknownSkill));
            Assert.That(h.TryLearn("A05N").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(h.UnspentSkillPoints, Is.Zero);
            Assert.That(h.TryLearn("A05M").code, Is.EqualTo(OriginalLearnCode.NoSkillPoints));
            h.GrantExperience(200); Assert.That(h.TryLearn("A05M").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(h.Snapshot().skills.Single(x => x.id == "A05M").rank, Is.EqualTo(1));
        }
        [Test] public void MissingLevelSkipCannotBeGuessedFromNativeBaseAbility()
        {
            var h = Hero(); h.TryLearn("A05N"); h.GrantExperience(127400);
            var r = h.TryLearn("A05N"); Assert.That(r.code, Is.EqualTo(OriginalLearnCode.RuleUnavailable));
            Assert.That(r.requiredLevelKnown, Is.False); Assert.That(h.UnspentSkillPoints, Is.EqualTo(49));
        }
        [Test] public void UltimateRanksRequireLevelsFiveNineThirteen()
        {
            var h = Hero(); h.GrantExperience(1400); Assert.That(h.TryLearn("A0E6").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(h.CanLearn("A0E6").requiredLevel, Is.EqualTo(9));
            Assert.That(h.TryLearn("A0E6").code, Is.EqualTo(OriginalLearnCode.HeroLevelTooLow));
            h.GrantExperience(3000); Assert.That(h.TryLearn("A0E6").code, Is.EqualTo(OriginalLearnCode.Learned));
            h.GrantExperience(4600); Assert.That(h.TryLearn("A0E6").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(h.TryLearn("A0E6").code, Is.EqualTo(OriginalLearnCode.MaximumRank));
            Assert.That(h.UnspentSkillPoints, Is.EqualTo(10));
        }
        [Test] public void AttributeBonusUsesBinaryOverridesAndRetainsMissingSixthRank()
        {
            var h = Hero(); h.GrantExperience(127400);
            for (var rank = 1; rank <= 15; rank++)
            {
                Assert.That(h.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.Learned)); var b = h.Snapshot().attributeBonus;
                Assert.That(b.known, Is.EqualTo(rank != 6));
                if (rank != 6) { Assert.That(b.strength, Is.EqualTo(2 * rank)); Assert.That(b.agility, Is.EqualTo(2 * rank)); Assert.That(b.intelligence, Is.EqualTo(2 * rank)); }
                else Assert.That(b.unresolved, Does.Contain("6"));
            }
            Assert.That(h.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.MaximumRank));
        }
        [Test] public void AttributesBecomeAvailableAtTwelveThenEveryLevel()
        {
            var h = Hero(); h.GrantExperience(6500); Assert.That(h.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.HeroLevelTooLow));
            h.GrantExperience(1200); Assert.That(h.TryLearn("A001").code, Is.EqualTo(OriginalLearnCode.Learned));
            Assert.That(h.CanLearn("A001").requiredLevel, Is.EqualTo(13));
        }
        [Test] public void ReturnedSnapshotsAndCatalogMutationCannotModifyProgression()
        {
            var combat = Combat(); var native = Native(); var h = new OriginalHeroProgression(combat, native, "H008");
            combat.Ability("A0E6").fields.Single(f => f.key == "reqLevel").number = 1;
            native.heroXp.levels[1].cumulative = 1;
            var s = h.Snapshot(); s.skills[0].rank = 99; s.skills[0].id = "A0E6"; s.attributeBonus.strength = 500;
            Assert.That(h.CanLearn("A0E6").requiredLevel, Is.EqualTo(5)); h.GrantExperience(1); Assert.That(h.Level, Is.EqualTo(1));
            Assert.That(h.Snapshot().skills[0].rank, Is.Zero); Assert.That(h.Snapshot().attributeBonus.strength, Is.Zero);
        }
        [Test] public void ArcherLearnEffectsKeepSourceOrderAndDoNotAddAbsentHelpersOnHigherRanks()
        {
            var first = OriginalLearnEffects.For("A15X", 1);
            Assert.That(first.Select(x => x.kind), Is.EqualTo(new[] { OriginalLearnMutationKind.AddAbility, OriginalLearnMutationKind.RegisterArcherAttackHandler, OriginalLearnMutationKind.SetBowElement }));
            Assert.That(first[0].abilityId, Is.EqualTo("A15Z")); Assert.That(first[2].value, Is.EqualTo(1));
            var second = OriginalLearnEffects.For("A15X", 2);
            Assert.That(second.Select(x => x.abilityId), Is.EqualTo(new[] { "A15Z", "A160", "A161", "A162", "A17M" }));
            Assert.That(second.All(x => x.kind == OriginalLearnMutationKind.SetExistingAbilityRank && x.value == 2), Is.True);
            Assert.That(OriginalLearnEffects.For("A0AC", 1)[0].kind, Is.EqualTo(OriginalLearnMutationKind.AddAbility));
            Assert.That(OriginalLearnEffects.For("A0AC", 3)[0].abilityId, Is.EqualTo("A0N6"));
            Assert.That(OriginalLearnEffects.For("A0AC", 3)[0].value, Is.EqualTo(3));
        }
        [Test] public void LearnTriggersIgnoreIllusionsAndOtherSelectedSkills()
        {
            Assert.That(OriginalLearnEffects.For("A15X", 1, true), Is.Empty);
            Assert.That(OriginalLearnEffects.For("A0AC", 1, true), Is.Empty);
            Assert.That(OriginalLearnEffects.For("A05N", 1), Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalLearnEffects.For("A15X", 4));
            Assert.Throws<ArgumentException>(() => OriginalLearnEffects.For("xxxx", 1));
        }
        [Test] public void InvalidAndConflictingRulesAreRejectedBeforeStateExists()
        {
            Assert.Throws<ArgumentException>(() => Hero("Hfoo"));
            var combat = Combat(); combat.Ability("A001").fields.Single(x => x.key == "reqLevel").conflict = true;
            Assert.Throws<InvalidOperationException>(() => new OriginalHeroProgression(combat, Native(), "H008"));
            combat = Combat(); combat.Ability("A0E6").fields.Single(x => x.key == "levelSkip").number = -1;
            Assert.Throws<ArgumentException>(() => new OriginalHeroProgression(combat, Native(), "H008"));
        }
    }
}

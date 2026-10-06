using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalInventoryEffectsTests
    {
        private static OriginalItemPassiveCatalog LoadObserved() => JsonUtility.FromJson<OriginalItemPassiveCatalog>(
            File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-item-passives.json")));
        private static OriginalItemPassiveCatalog Load()
        {
            var result = LoadObserved(); result.observedEquip = null;
            result.observedAdditionalEquip = null; return result;
        }

        private static OriginalInventorySnapshot Bag(params string[] ids)
        {
            var snapshot = new OriginalInventorySnapshot { ownerId = 1, heroSlots = new OriginalItemInstance[6], servantSlots = new OriginalItemInstance[6] };
            for (var i = 0; i < ids.Length; i++) snapshot.heroSlots[i] = new OriginalItemInstance { instanceId = i + 1, itemId = ids[i], ownerId = 1 };
            return snapshot;
        }

        [Test]
        public void EmptyHeroHasKnownEmptyPlan_AndServantDoesNotBuffHero()
        {
            var snapshot = Bag();
            snapshot.servantSlots[0] = new OriginalItemInstance { instanceId = 9, itemId = "I000", ownerId = 1 };
            var plan = new OriginalInventoryEffects(Load()).Plan(snapshot);
            Assert.That(plan.CanApplyProfile, Is.True);
            Assert.That(plan.DeclaredModifiers, Is.Empty);
            Assert.That(plan.CmContributions, Is.Empty);
            Assert.That(plan.ServantItemCount, Is.EqualTo(1));
        }

        [Test]
        public void NativeDamageAndScriptAttackProxyAreSeparate_UnknownStackingIsExplicit()
        {
            var plan = new OriginalInventoryEffects(Load()).Plan(Bag("I000", "I000"),
                (instance, ability) => ability == "A00A" ? (int?)1 : null);
            Assert.That(plan.DeclaredModifiers.Length, Is.EqualTo(2));
            Assert.That(plan.DeclaredModifiers.All(x => x.Stat == "attackDamage" && x.Value == 16), Is.True);
            Assert.That(plan.CmContributions.Select(x => x.Value), Is.EqualTo(new double[] { 16, 16 }));
            Assert.That(plan.CanApplyProfile, Is.False);
            Assert.That(plan.Gaps.Any(x => x.Reason.Contains("stacking")), Is.True);
        }

        [Test]
        public void MissingRankDoesNotGuessOne_ButRetainsIndependentCmDeclaration()
        {
            var plan = new OriginalInventoryEffects(Load()).Plan(Bag("I000"));
            Assert.That(plan.DeclaredModifiers, Is.Empty);
            Assert.That(plan.CmContributions.Single().Value, Is.EqualTo(16));
            Assert.That(plan.Gaps.Any(x => x.AbilityId == "A00A" && x.Reason == "native-current-level-unresolved"), Is.True);
        }

        [Test]
        public void UnknownEffectInOneItemDoesNotEraseOtherKnownContributions()
        {
            var plan = new OriginalInventoryEffects(Load()).Plan(Bag("I000", "I05F"), (i, a) => 1);
            Assert.That(plan.DeclaredModifiers.Single().Value, Is.EqualTo(16));
            Assert.That(plan.Items.Single(x => x.ItemId == "I05F").ScriptReferences.Any(x => x.functionName == "C6"), Is.True);
            Assert.That(plan.Gaps.Any(x => x.ItemInstanceId == 2 && x.Reason == "item-script-lifecycle-unimplemented"), Is.True);
        }

        [Test]
        public void PlannerAndReturnedPlansDoNotAliasSourceOrEachOther()
        {
            var source = Load();
            var planner = new OriginalInventoryEffects(source);
            source.abilities.Single(x => x.id == "A00A").levels[0].modifiers[0].value = 999;
            source.items.Single(x => x.id == "I000").cmBonus = 999;
            var first = planner.Plan(Bag("I000"), (i, a) => 1);
            first.DeclaredModifiers[0].Value = 444;
            first.DeclaredModifiers[0].Sources[0] = "corrupt";
            first.Items[0].ScriptReferences[0].functionName = "corrupt";
            var second = planner.Plan(Bag("I000"), (i, a) => 1);
            Assert.That(second.DeclaredModifiers.Single().Value, Is.EqualTo(16));
            Assert.That(second.CmContributions.Single().Value, Is.EqualTo(16));
            Assert.That(second.DeclaredModifiers[0].Sources[0], Is.Not.EqualTo("corrupt"));
            Assert.That(second.Items[0].ScriptReferences[0].functionName, Is.Not.EqualTo("corrupt"));
        }

        [Test]
        public void InvalidOrDuplicateResidencyFailsBeforeCallingRankResolver()
        {
            var planner = new OriginalInventoryEffects(Load());
            var snapshot = Bag("I000");
            snapshot.servantSlots[0] = snapshot.heroSlots[0];
            var calls = 0;
            Assert.Throws<ArgumentException>(() => planner.Plan(snapshot, (i, a) => { calls++; return 1; }));
            Assert.That(calls, Is.Zero);
            snapshot.servantSlots[0] = null;
            snapshot.heroSlots[0].removed = true;
            Assert.Throws<ArgumentException>(() => planner.Plan(snapshot));
        }

        [Test]
        public void AllSourceItemsHaveIndependentCoverage_UnknownNativeFamilyStaysVisible()
        {
            var source = Load();
            var planner = new OriginalInventoryEffects(source);
            Assert.That(planner.ItemCount, Is.EqualTo(440));
            foreach (var item in source.items)
            {
                var plan = planner.Plan(Bag(item.id), (i, a) => 1);
                Assert.That(plan.Items.Single().ItemId, Is.EqualTo(item.id));
                Assert.That(plan.CanApplyProfile, Is.False, item.id);
            }
        }

        [Test]
        public void SourceDuplicateAbilityKeepsSeparateOccurrencesAndObservedI050Strength()
        {
            var plan = new OriginalInventoryEffects(Load()).Plan(Bag("I050"), (i, a) => 1);
            Assert.That(plan.Items.Single().DirectAbilities.Count(x => x == "A00T"), Is.EqualTo(2));
            Assert.That(plan.DeclaredModifiers.Count(x => x.AbilityId == "A00T"), Is.EqualTo(2));
            Assert.That(plan.DeclaredModifiers.Where(x => x.AbilityId == "A00T").Select(x => x.Occurrence), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(plan.DeclaredModifiers.Where(x => x.Stat == "strength").Sum(x => x.Value), Is.EqualTo(36));
        }

        [Test]
        public void ObservedNativeProfileAppliesKnownDamageArmorAndAttributesWithoutClaimingAllEffects()
        {
            var planner = new OriginalInventoryEffects(LoadObserved());
            var plan = planner.Plan(Bag("I000", "I050", "I00C"));
            Assert.That(plan.CanApplyProfile, Is.True);
            Assert.That(plan.EffectsComplete, Is.False);
            Assert.That(plan.NativeProfile.Require().attackDamage, Is.EqualTo(76));
            Assert.That(plan.NativeProfile.strength, Is.EqualTo(48));
            Assert.That(plan.NativeProfile.maxHealthFlat, Is.Zero);
            Assert.That(plan.NativeProfile.maxManaFlat, Is.Zero);
            Assert.That(plan.NativeProfile.agility, Is.Zero);
        }

        [Test]
        public void FortyThreeObservedItemsHaveNativeProfilesButUnknownItemsAndWrongRanksDoNot()
        {
            var source = LoadObserved(); var planner = new OriginalInventoryEffects(source);
            Assert.That(source.observedEquip.items.Length, Is.EqualTo(43));
            foreach (var item in source.observedEquip.items)
                Assert.That(planner.Plan(Bag(item.id)).CanApplyProfile, Is.True, item.id);
            Assert.That(planner.Plan(Bag("I000", "I05F")).CanApplyProfile, Is.False);
            Assert.That(planner.Plan(Bag("I000"), (i, a) => 0).CanApplyProfile, Is.False);
            Assert.Throws<InvalidOperationException>(() => planner.Plan(Bag("I05F")).NativeProfile.Require());
        }

        [Test]
        public void NativeProfileKeepsFlatVitalitySeparateFromAttributeDerivedVitalityAndCopiesEvidence()
        {
            var source = LoadObserved(); var planner = new OriginalInventoryEffects(source);
            var first = planner.Plan(Bag("I04F"));
            Assert.That(first.NativeProfile.intelligence, Is.EqualTo(20));
            Assert.That(first.NativeProfile.maxHealthFlat, Is.EqualTo(300));
            Assert.That(first.NativeProfile.maxManaFlat, Is.EqualTo(300));
            first.NativeProfile.intelligence = 999;
            source.observedEquip.items[0].snapshots[1].strength = 999;
            Assert.That(planner.Plan(Bag("I04F")).NativeProfile.intelligence, Is.EqualTo(20));
            var six = planner.Plan(Bag("I00C", "I00C", "I00C", "I00C", "I00C", "I00C"));
            Assert.That(six.NativeProfile.Require().strength, Is.EqualTo(72));
            Assert.That(six.NativeProfile.mixedOrMoreThanTwoIsDerived, Is.True);
        }

        [Test]
        public void CorruptEquipDataCannotSilentlyResolveNativeStats()
        {
            var source = LoadObserved(); source.observedEquip.items[0].snapshots[1].strength = 999;
            Assert.Throws<ArgumentException>(() => new OriginalInventoryEffects(source));
            source = LoadObserved(); source.observedEquip.source.complete = false;
            Assert.Throws<ArgumentException>(() => new OriginalInventoryEffects(source));
        }
    }
}

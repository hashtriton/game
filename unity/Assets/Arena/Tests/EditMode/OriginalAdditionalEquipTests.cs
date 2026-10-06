using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalAdditionalEquipTests
    {
        [Test] public void NativeMeasuredBootsUseMaximumBonusAndRetainActiveCoverageGaps()
        {
            var effects = new OriginalInventoryEffects(Load<OriginalItemPassiveCatalog>("lia39-item-passives.json"));
            var bag = Bag("I00Y");
            bag.heroSlots[1] = new OriginalItemInstance { ownerId = 1, instanceId = 2, itemId = "I013" };
            bag.heroSlots[2] = new OriginalItemInstance { ownerId = 1, instanceId = 3, itemId = "I08U" };
            var plan = effects.Plan(bag);
            Assert.That(plan.CanApplyProfile, Is.True);
            Assert.That(plan.NativeProfile.moveSpeedFlat, Is.EqualTo(80));
            Assert.That(plan.NativeProfile.attackDamage, Is.EqualTo(60));
            Assert.That(plan.NativeProfile.maxHealthFlat, Is.EqualTo(400));
            Assert.That(plan.EffectsComplete, Is.False, "Measured passive profile does not implement Wind Walk or item scripts.");
            bag.heroSlots[2] = null;
            Assert.That(effects.Plan(bag).NativeProfile.moveSpeedFlat, Is.EqualTo(60));
        }

        [Test] public void NativeMeasuredResistanceItemsKeepTheirPerInstanceContributions()
        {
            var effects = new OriginalInventoryEffects(Load<OriginalItemPassiveCatalog>("lia39-item-passives.json"));
            var bag = Bag("I024");
            bag.heroSlots[1] = new OriginalItemInstance { ownerId = 1, instanceId = 2, itemId = "I026" };
            var plan = effects.Plan(bag);
            Assert.That(plan.CanApplyProfile, Is.True);
            Assert.That(plan.NativeProfile.maxHealthFlat, Is.EqualTo(200));
            Assert.That(plan.DeclaredModifiers.Where(m => m.Stat == "spellResistanceFraction").Select(m => m.Value), Is.EqualTo(new[] { .2, .25 }));
            Assert.That(plan.EffectsComplete, Is.False, "ANfd cleave remains a separate executor.");
        }
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static OriginalItemPassiveCatalog Fixture()
        {
            var source = Load<OriginalItemPassiveCatalog>("lia39-item-passives.json");
            var items = Load<OriginalItemCatalog>("lia39-items.json");
            var native = Load<OriginalObservedItemCatalog>("lia39-observed-items126.json");
            var prior = new HashSet<string>(source.observedEquip.items.Select(r => r.id));
            var rows = new List<OriginalEquipItem>();
            foreach (var item in items.items.OrderBy(r => r.id))
            {
                if (item.classId == "PowerUp" || prior.Contains(item.id) || native.items.Single(r => r.id == item.id).powerup) continue;
                var queries = new List<string>();
                void Add(string id)
                {
                    if (queries.Contains(id)) return;
                    queries.Add(id);
                    foreach (var level in source.abilities.Single(a => a.id == id).levels)
                        foreach (var child in level.spellbookAbilityIds) Add(child);
                }
                foreach (string ability in item.abilityIds) Add(ability);
                rows.Add(new OriginalEquipItem { id = item.id, sourceKey = "item_" + item.id, error = "synthetic unmeasured row",
                    directAbilityIds = item.abilityIds.ToArray(), abilityIds = queries.ToArray() });
            }
            Assert.That(rows.Count, Is.EqualTo(221));
            source.observedAdditionalEquip = new[] { new OriginalObservedItemEquip {
                schemaVersion = 3, mapSha256 = source.mapSha256, engineVersion = "1.26.0.6401", heroId = "H008", heroLevel = 1,
                source = new OriginalEquipSource { cacheName = "LiAEq2.w3v", cacheSha256 = new string('a', 64), records = 221, complete = true,
                    capturedUtc = "synthetic test fixture",
                    probeMapSha256 = "980448f918e4b9c4434070f48622b1a6029d21cb01a87a9cd4ac93a76a28d831",
                    probeScriptSha256 = "fae3aacc7c0e764c79473aaf6ed02046923c861605d1e4935a892c0c71d8c3b2" }, items = rows.ToArray() } };
            KnownZero(source, "I003");
            return source;
        }
        static void KnownZero(OriginalItemPassiveCatalog source, string id)
        {
            var row = source.observedAdditionalEquip[0].items.Single(r => r.id == id);
            row.known = true; row.error = null;
            string[] phases = { "baseline", "one_immediate", "one_delayed", "two_immediate", "two_delayed",
                "remove_first_immediate", "remove_first_delayed", "empty_immediate", "empty_delayed" };
            int[] counts = { 0,1,1,2,2,1,1,0,0 };
            row.snapshots = phases.Select((phase,i) => new OriginalEquipSnapshot { phase = phase, copies = counts[i], level = 1, xp = 0, time = .1*i,
                baseStrength = 22, baseAgility = 6, baseIntelligence = 7, strength = 22, agility = 6, intelligence = 7,
                health = 300, maxHealth = 631, mana = 70, maxMana = 145, moveSpeed = 250,
                abilityRanks = row.abilityIds.Select(a => counts[i] == 0 ? 0 : 1).ToArray() }).ToArray();
        }
        static OriginalInventorySnapshot Bag(string id) => new OriginalInventorySnapshot { ownerId = 1,
            heroSlots = new[] { new OriginalItemInstance { ownerId = 1, instanceId = 1, itemId = id }, null, null, null, null, null },
            servantSlots = new OriginalItemInstance[6] };

        [Test] public void MeasuredChannelRecipeHasZeroPassiveProfileWhileScriptCoverageRemainsExplicit()
        {
            var source = Fixture(); var effects = new OriginalInventoryEffects(source);
            var plan = effects.Plan(Bag("I003"));
            Assert.That(plan.CanApplyProfile, Is.True); Assert.That(plan.EffectsComplete, Is.False);
            Assert.That(plan.NativeProfile.attackDamage, Is.Zero); Assert.That(plan.NativeProfile.maxHealthFlat, Is.Zero);
            var transition = effects.ChangeVitality("I003", true, 100.25, 631, 70.75, 145, 8, 10);
            Assert.That(transition.known, Is.True); Assert.That(transition.health, Is.EqualTo(100.25));
            Assert.That(transition.mana, Is.EqualTo(70.75)); Assert.That(transition.approximate, Is.False);
        }
        [Test] public void UnsupportedItemDoesNotInvalidateKnownRecipesOrOldFortyThreeProfiles()
        {
            var source = Fixture(); var effects = new OriginalInventoryEffects(source);
            Assert.That(effects.Plan(Bag("I003")).CanApplyProfile, Is.True);
            Assert.That(effects.Plan(Bag("I00Y")).CanApplyProfile, Is.False);
            foreach (var old in source.observedEquip.items) Assert.That(effects.Plan(Bag(old.id)).CanApplyProfile, Is.True, old.id);
        }
        [Test] public void EvidenceMutationsAndWrongCurrentRankCannotAlterFrozenProfile()
        {
            var source = Fixture(); var effects = new OriginalInventoryEffects(source);
            var row = source.observedAdditionalEquip[0].items.Single(r => r.id == "I003");
            row.snapshots[1].maxHealth = 999; row.abilityIds[0] = "evil";
            Assert.That(effects.Plan(Bag("I003")).CanApplyProfile, Is.True);
            Assert.That(effects.Plan(Bag("I003"), (i,a) => 2).CanApplyProfile, Is.False);
            var plan = effects.Plan(Bag("I003")); plan.NativeProfile.maxHealthFlat = 100;
            Assert.That(effects.Plan(Bag("I003")).NativeProfile.maxHealthFlat, Is.Zero);
        }
        [Test] public void WrongQueryClosureOrIncompleteNativeStateRejectsTheWholeEvidenceBatch()
        {
            var source = Fixture(); source.observedAdditionalEquip[0].items.Single(r => r.id == "I003").abilityIds = new string[0];
            Assert.Throws<ArgumentException>(() => new OriginalInventoryEffects(source));
            source = Fixture(); source.observedAdditionalEquip[0].items.Single(r => r.id == "I003").snapshots[1].baseStrength = 23;
            Assert.Throws<ArgumentException>(() => new OriginalInventoryEffects(source));
        }
        [Test] public void CapturedCompactBatchRetainsAll221RowsAndOriginalProfiles_WithPerItemEvidence()
        {
            var source = Load<OriginalItemPassiveCatalog>("lia39-item-passives.json");
            var batch = source.observedAdditionalEquip.Single(r => r.schemaVersion == 3);
            Assert.That(batch.source.cacheSha256, Is.EqualTo("59bd92d5e0a681b6a4691c130af8eb59ce5c202b837b4137261fa036ff77bd6b"));
            Assert.That(batch.items.Length, Is.EqualTo(221));
            Assert.That(batch.items.All(r => r.known && r.snapshots.Length == 9), Is.True);
            var effects = new OriginalInventoryEffects(source);
            foreach (var prior in source.observedEquip.items)
                Assert.That(effects.Plan(Bag(prior.id)).CanApplyProfile, Is.True, prior.id);
            var recipe = effects.Plan(Bag("I003"));
            Assert.That(recipe.CanApplyProfile, Is.True); Assert.That(recipe.EffectsComplete, Is.False);
            Assert.That(recipe.NativeProfile.evidence.Any(e => e.Contains(batch.source.cacheSha256) && e.Contains("item_I003")), Is.True);
            Assert.That(effects.ChangeVitality("I003", true, 100.25, 631, 70.75, 145, 8, 10).Require().health, Is.EqualTo(100.25));
            Assert.That(effects.Plan(Bag("I00Y")).NativeProfile.moveSpeedFlat, Is.EqualTo(50), "LiAItemFam3 closes the native boot contribution.");
        }
        [Test] public void CapturedMatricesCoverEveryNativeNonPowerupOnce_WithoutPromotingUnsupportedFamilies()
        {
            var source = Load<OriginalItemPassiveCatalog>("lia39-item-passives.json");
            var flags = Load<OriginalObservedItemCatalog>("lia39-observed-items126.json");
            var extra = source.observedAdditionalEquip.Single(r => r.schemaVersion == 4);
            Assert.That(extra.source.cacheSha256, Is.EqualTo("d1fcc878dec24bb180dab4558b56238028a0281f58ab23f08fe431430a7dd7d1"));
            Assert.That(extra.items.Length, Is.EqualTo(8));
            Assert.That(extra.items.All(r => r.known && r.snapshots.Length == 9), Is.True);
            var observed = source.observedEquip.items.Select(r => r.id).Concat(
                source.observedAdditionalEquip.SelectMany(b => b.items.Select(r => r.id))).ToArray();
            Assert.That(observed.Length, Is.EqualTo(272)); Assert.That(observed.Distinct().Count(), Is.EqualTo(272));
            Assert.That(observed.OrderBy(id => id), Is.EqualTo(flags.items.Where(r => !r.powerup).Select(r => r.id).OrderBy(id => id)));
            var effects = new OriginalInventoryEffects(source);
            Assert.That(source.items.Count(r => effects.Plan(Bag(r.id)).CanApplyProfile), Is.GreaterThanOrEqualTo(143),
                "Later source-backed family executors may expand coverage without losing the earlier profiles.");
            var imageItem = effects.Plan(Bag("I05G"));
            Assert.That(imageItem.CanApplyProfile, Is.True, "The reversible intrinsic profile is independent of the active image executor.");
            Assert.That(imageItem.EffectsComplete, Is.False, "An intrinsic profile does not close the active image spell or its source handlers.");
            Assert.That(effects.Plan(Bag("I003")).EffectsComplete, Is.False);
        }
    }
}

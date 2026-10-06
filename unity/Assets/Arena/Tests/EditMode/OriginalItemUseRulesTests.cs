using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemUseRulesTests
    {
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + file + ".json")));
        [Test] public void DeclarationsAloneDoNotInventMissingCostOrChargeBehavior()
        {
            var rules = new OriginalItemUseRules(Load<OriginalItemCatalog>("items"), Load<OriginalCombatCatalog>("combat"));
            Assert.That(rules.Rule("I03L").known, Is.False);
            Assert.That(rules.Rule("I03L").amount, Is.EqualTo(300));
            Assert.That(rules.Rule("I03M").amount, Is.EqualTo(200));
            Assert.That(rules.Rule("I022").amount, Is.EqualTo(800));
            Assert.That(rules.Rule("I023").amount, Is.EqualTo(600));
            Assert.That(rules.Rule("I022").cooldown, Is.EqualTo(40));
            rules.Rule("I03L").known = true;
            Assert.That(rules.Rule("I03L").known, Is.False);
            Assert.That(rules.Rule("I000"), Is.Null);
        }
        [Test] public void IncompleteRuntimeEvidenceCannotEnableConsumables()
        {
            Assert.Throws<ArgumentException>(() => new OriginalItemUseRules(Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), new OriginalObservedItemUse()));
        }
        [Test] public void NativeCaptureEnablesUseAndKnownPassiveProfilesWithoutAliasingEvidence()
        {
            var observed = Load<OriginalObservedItemCatalog>("observed-items126");
            var rules = new OriginalItemUseRules(Load<OriginalItemCatalog>("items"), Load<OriginalCombatCatalog>("combat"), observed.itemUse);
            observed.itemUse.items[0].amount = 999;
            Assert.That(rules.Rule("I03L").amount, Is.EqualTo(300));
            Assert.That(rules.Rule("I03L").known, Is.True);
            var effects = new OriginalInventoryEffects(Load<OriginalItemPassiveCatalog>("item-passives"), rules);
            var inventory = new OriginalInventory(Load<OriginalItemCatalog>("items"), 1);
            Assert.That(inventory.TryPickup(inventory.CreateInstance("I022")).Applied, Is.True);
            Assert.That(inventory.TryPickup(inventory.CreateInstance("I023")).Applied, Is.True);
            var plan = effects.Plan(inventory.Snapshot());
            Assert.That(plan.CanApplyProfile, Is.True);
            Assert.That(plan.NativeProfile.healthRegenPerSecond, Is.EqualTo(10));
            Assert.That(plan.NativeProfile.manaRegenBaseFraction, Is.EqualTo(.5));
            Assert.That(plan.NativeProfile.maxHealthFlat, Is.Zero);
            Assert.That(effects.ChangeVitality("I022", false, 12.5, 631, 20.5, 145, 8, 10).Require().health, Is.EqualTo(12.5));
        }
    }
}

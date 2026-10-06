using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalItemPickupEffectsTests
    {
        static OriginalItemPickupActor Actor(int id = 1, int owner = 1, string rawcode = "H008", double x = 0) =>
            new OriginalItemPickupActor { entityId = id, ownerSlot = owner, rawcode = rawcode, hero = true, alliedToActor = true, health = 100, x = x };
        static OriginalItemPickupContext Context() => new OriginalItemPickupContext
            { actor = Actor(), nearbyUnits = new[] { Actor() }, wave = 3, actorInventoryCount = 2 };

        [Test] public void RandomCurrencyAndExperienceUseSourceInclusiveBoundsWithoutRngMutation()
        {
            Assert.That(OriginalItemPickupEffects.Plan("I05F", Context(), 200).mutations.Single().amount, Is.EqualTo(200));
            Assert.That(OriginalItemPickupEffects.Plan("I05F", Context(), 700).mutations.Single().kind, Is.EqualTo(OriginalPickupMutationKind.Experience));
            Assert.That(OriginalItemPickupEffects.Plan("I07W", Context(), 70).mutations.Single().kind, Is.EqualTo(OriginalPickupMutationKind.Gold));
            Assert.That(OriginalItemPickupEffects.Plan("I0AI", Context(), 2).mutations.Single().kind, Is.EqualTo(OriginalPickupMutationKind.Lumber));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalItemPickupEffects.Plan("I05F", Context(), 199));
            Assert.That(OriginalItemPickupEffects.Plan("I05F", Context()).complete, Is.False);
        }

        [Test] public void RadiusIncludesBoundaryButExcludesDeadIllusionsEnemiesAndSourceProxyHeroes()
        {
            var context = Context();
            var boundary = Actor(2, 2, "H024", 900);
            var outside = Actor(3, 3, "N0A0", 900.001);
            var dead = Actor(4, 4); dead.health = .405;
            var image = Actor(5, 5); image.illusion = true;
            var enemy = Actor(6, 6); enemy.alliedToActor = false;
            var proxy = Actor(7, 7, "U00T");
            context.nearbyUnits = new[] { context.actor, boundary, outside, dead, image, enemy, proxy };
            var plan = OriginalItemPickupEffects.Plan("I05T", context);
            Assert.That(plan.complete, Is.True);
            Assert.That(plan.mutations.Select(x => x.entityId), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(plan.mutations.All(x => x.amount == 75), Is.True);
        }

        [Test] public void GoldTargetsEachEligibleHeroOwnerAndDoesNotDeduplicateOwners()
        {
            var context = Context(); context.nearbyUnits = new[] { context.actor, Actor(2, 1, "H024", 50) };
            var plan = OriginalItemPickupEffects.Plan("I05U", context);
            Assert.That(plan.mutations.Length, Is.EqualTo(2));
            Assert.That(plan.mutations.All(x => x.ownerSlot == 1 && x.amount == 30), Is.True);
        }

        [Test] public void RandomItemBagUsesTheSevenDeclaredOutcomesAndFullInventoryGroundOwnership()
        {
            var context = Context(); context.actorInventoryCount = 6;
            var ground = OriginalItemPickupEffects.Plan("I05W", context, 7).mutations.Single();
            Assert.That(ground.kind, Is.EqualTo(OriginalPickupMutationKind.CreateGroundItem));
            Assert.That(ground.itemId, Is.EqualTo("I01C")); Assert.That(ground.ownerSlot, Is.EqualTo(1));
            context.actorInventoryCount = 5;
            Assert.That(OriginalItemPickupEffects.Plan("I05W", context, 1).mutations.Single().kind, Is.EqualTo(OriginalPickupMutationKind.CreateInventoryItem));
            Assert.Throws<ArgumentOutOfRangeException>(() => OriginalItemPickupEffects.Plan("I05W", context, 8));
        }

        [Test] public void WeightedChestRequiresAHostDrawAndNonHeroXpRemainsUnsupported()
        {
            Assert.That(OriginalItemPickupEffects.Plan("I0AT", Context()).complete, Is.False);
            var context = Context(); context.actor.hero = false;
            Assert.That(OriginalItemPickupEffects.Plan("I05F", context, 200).complete, Is.False);
            Assert.That(OriginalItemPickupEffects.Plan("I000", Context()).handled, Is.False);
        }

        [Test] public void FateChestPreservesEveryAuthoredWeightAndGroundFallback()
        {
            var counts=Enumerable.Range(1,OriginalItemFatePool.TotalWeight).GroupBy(OriginalItemFatePool.At).ToDictionary(g=>g.Key,g=>g.Count());
            Assert.That(counts.Count,Is.EqualTo(139));Assert.That(counts["I01Y"],Is.EqualTo(10));
            Assert.That(counts["I01R"],Is.EqualTo(8));Assert.That(counts["I060"],Is.EqualTo(5));
            Assert.That(OriginalItemFatePool.At(1308),Is.EqualTo("I08I"));
            var context=Context();context.actorInventoryCount=6;
            var plan=OriginalItemPickupEffects.Plan("I0AT",context,1);
            Assert.That(plan.complete,Is.True);Assert.That(plan.mutations.Single().itemId,Is.EqualTo("I01Y"));
            Assert.That(plan.mutations.Single().kind,Is.EqualTo(OriginalPickupMutationKind.CreateGroundItem));
            Assert.Throws<ArgumentOutOfRangeException>(()=>OriginalItemFatePool.At(1309));
        }

        [Test] public void InvalidContextFailsBeforeAnyPlanAndReturnedPlansAreDetached()
        {
            var context = Context(); context.nearbyUnits = new[] { context.actor, context.actor };
            Assert.Throws<ArgumentException>(() => OriginalItemPickupEffects.Plan("I05T", context));
            context = Context(); context.actor.health = double.NaN;
            Assert.Throws<ArgumentException>(() => OriginalItemPickupEffects.Plan("I05F", context, 200));
            context = Context(); var plan = OriginalItemPickupEffects.Plan("I05T", context);
            plan.mutations[0].amount = 999; context.nearbyUnits[0].ownerSlot = 2;
            Assert.That(OriginalItemPickupEffects.Plan("I05T", Context()).mutations[0].amount, Is.EqualTo(75));
        }

        [Test] public void FullWorldEnumerationMayContainEnemyOwnerZeroWithoutInvalidatingAlliedRewards()
        {
            var context = Context();
            var enemy = Actor(1001, 0, "n008", 100); enemy.hero = false; enemy.alliedToActor = false;
            context.nearbyUnits = new[] { context.actor, enemy };
            var plan = OriginalItemPickupEffects.Plan("I05T", context);
            Assert.That(plan.complete, Is.True);
            Assert.That(plan.mutations.Single().entityId, Is.EqualTo(1));
            enemy.hero = true; enemy.alliedToActor = true;
            Assert.That(OriginalItemPickupEffects.Plan("I05T", context).complete, Is.False);
            Assert.That(OriginalItemPickupEffects.Plan("I05T", context).mutations, Is.Empty);
        }
    }
}

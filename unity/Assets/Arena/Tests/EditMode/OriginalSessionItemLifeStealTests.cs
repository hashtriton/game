using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemLifeStealTests
    {
        static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s, string name, params object[] args)
        {
            var method = typeof(OriginalSession).GetMethod(name, Hidden);
            Assert.That(method, Is.Not.Null, "Missing item lifesteal boundary.");
            return method.Invoke(s, args);
        }
        static OriginalSession Create(out OriginalWorld world, out OriginalInventory inventory)
        {
            var s = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Hidden).GetValue(s);
            var p = ((IList)typeof(OriginalSession).GetField("players", Hidden).GetValue(s))[0];
            inventory = (OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
            world.AddUnit(1001, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 16 }, new OriginalPoint(500, 1000));
            var hero = world.UnitState(1); world.UpdateProfile(1, hero.profile, 100, hero.mana);
            return s;
        }
        [Test] public void ItemLifeStealUsesActualDamageAndHighestResidentOrbWithoutStackingCopies()
        {
            var s = Create(out var w, out var inventory);
            inventory.TryPickup(inventory.CreateInstance("I08M"), OriginalInventoryBag.Hero);
            inventory.TryPickup(inventory.CreateInstance("I00H"), OriginalInventoryBag.Hero);
            Call(s, "ApplyItemWeaponLifeSteal", 1, 1001, 40.0);
            Assert.That(w.UnitState(1).health, Is.EqualTo(104).Within(1e-8));
            inventory.Transfer(OriginalInventoryBag.Hero, 0);
            Call(s, "ApplyItemWeaponLifeSteal", 1, 1001, 40.0);
            Assert.That(w.UnitState(1).health, Is.EqualTo(110.8).Within(1e-8));
            Call(s, "ApplyItemWeaponLifeSteal", 1, 1001, 0.0);
            Assert.That(w.UnitState(1).health, Is.EqualTo(110.8).Within(1e-8));
        }
        [Test] public void ServantAndRemovedSourceDoNotHealAndHealingNeverRevives()
        {
            var s = Create(out var w, out var inventory);
            inventory.TryPickup(inventory.CreateInstance("I08M"), OriginalInventoryBag.Servant);
            Call(s, "ApplyItemWeaponLifeSteal", 1, 1001, 40.0);
            Assert.That(w.UnitState(1).health, Is.EqualTo(100));
            inventory.Transfer(OriginalInventoryBag.Servant, 0);
            w.ForceUnitDeath(1);
            Call(s, "ApplyItemWeaponLifeSteal", 1, 1001, 40.0);
            Assert.That(w.UnitState(1).health, Is.Zero);
        }
        [Test] public void MechanicalVictimsUseTheNativeCaseInsensitiveClassification()
        {
            var s=Create(out var w,out var inventory);
            inventory.TryPickup(inventory.CreateInstance("I08M"),OriginalInventoryBag.Hero);
            w.AddUnit(1002,0,"hmtt",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(700,1000));
            Call(s,"ApplyItemWeaponLifeSteal",1,1002,40.0);
            Assert.That(w.UnitState(1).health,Is.EqualTo(100));
        }
        [Test] public void VampiricAuraAffectsNearbyAlliedMeleeButNotAnEnemyOrRangedAttacker()
        {
            var s = Create(out var w, out var inventory);
            inventory.TryPickup(inventory.CreateInstance("I04B"), OriginalInventoryBag.Hero);
            var row = new OriginalWorldSummonSpawn { entityId = 100000000, ownerSlot = 1, sourceHeroEntityId = 1, rawcode = "hfoo",
                position = new OriginalPoint(300, 1000), profile = new OriginalWorldUnitProfile { maxHealth = 500, collisionRadius = 16 }, health = 100 };
            Assert.That(w.TryPublishSummons(new[] { row }), Is.True);
            Call(s, "ApplyItemWeaponLifeSteal", row.entityId, 1001, 100.0);
            Assert.That(w.UnitState(row.entityId).health, Is.EqualTo(115));
            row.entityId++; row.rawcode = "n01V"; row.position = new OriginalPoint(400, 1000);
            Assert.That(w.TryPublishSummons(new[] { row }), Is.True);
            Call(s, "ApplyItemWeaponLifeSteal", row.entityId, 1001, 100.0);
            Assert.That(w.UnitState(row.entityId).health, Is.EqualTo(100));
            Call(s, "ApplyItemWeaponLifeSteal", 1001, 1, 100.0);
            Assert.That(w.UnitState(1001).health, Is.EqualTo(1000));
        }
        [Test] public void NativeSelfAuraDoesNotRequireGroundFlagAndNonheroAuraExcludesNativeHeroes()
        {
            var s = Create(out var w, out _);
            foreach (var pair in new[] { (1002, "O006", 600.0), (1003, "n00Q", 700.0), (1004, "n008", 800.0) })
                w.AddUnit(pair.Item1, 0, pair.Item2, new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 16 }, new OriginalPoint(pair.Item3, 1000));
            foreach (var id in new[] { 1002, 1004 })
            { var unit = w.UnitState(id); w.UpdateProfile(id, unit.profile, 100, unit.mana); }
            Call(s, "ApplyItemWeaponLifeSteal", 1002, 1, 100.0);
            Assert.That(w.UnitState(1002).health, Is.EqualTo(130), "ACvp self-only applies; A06Y nonhero excludes O006");
            Call(s, "ApplyItemWeaponLifeSteal", 1004, 1, 100.0);
            Assert.That(w.UnitState(1004).health, Is.EqualTo(175), "A06Y includes nearby allied nonheroes");
        }
    }
}


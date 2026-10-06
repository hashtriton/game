using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemProtectionTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld w,out OriginalInventory inv)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];inv=(OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(300,1000));return s;
        }
        static void Hit(OriginalSession s,double damage,bool melee)
        {
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("attacker",1001),("owner",0),("target",1),("kind",OriginalWorldTargetKind.Unit),("attackType","chaos"),("damage",damage),("melee",melee)})
                type.GetField(pair.Item1,Hidden).SetValue(shot,pair.Item2);
            Call(s,"ApplyWeaponHit",shot);
        }
        [Test] public void HardenedSkinUsesBothWeaponKindsAndDoesNotBlockScriptedMagicOrServantEquipment()
        {
            foreach(bool melee in new[]{true,false})
            {
                var s=Create(out var w,out var inv);var item=inv.CreateInstance("I06R");inv.TryPickup(item,OriginalInventoryBag.Hero);
                double before=w.UnitState(1).health;Hit(s,40,melee);
                Assert.That(before-w.UnitState(1).health,Is.EqualTo(1/(1+.06*s.Snapshot().players[0].combat.armor)).Within(1e-7));
                before=w.UnitState(1).health;Call(s,"ApplyNativeTriggeredHit",1001,0,w.UnitState(1),40.0,OriginalTriggeredDamageMode.SpellMagic);
                Assert.That(before-w.UnitState(1).health,Is.EqualTo(32).Within(1e-7));
                inv.Transfer(OriginalInventoryBag.Hero,0);before=w.UnitState(1).health;Hit(s,40,melee);
                Assert.That(before-w.UnitState(1).health,Is.GreaterThan(1));
            }
        }
        [Test] public void HardenedSkinSubtractsBeforeArmorAsMeasuredBySettledItemRoarWeaponHits()
        {
            var s=Create(out var w,out var inv);
            var player=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            var combat=(OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Hidden).GetValue(s);
            var observations=(OriginalObservedCatalog)typeof(OriginalSession).GetField("progressionObserved",Hidden).GetValue(s);
            var stats=OriginalHeroStats.Calculate(combat,"H008",50,observations);
            player.GetType().GetField("stats").SetValue(player,stats);
            var old=w.UnitState(1);w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=stats.maxHealth.Require(),maxMana=stats.maxMana.Require(),
                collisionRadius=old.profile.collisionRadius,moveSpeed=old.profile.moveSpeed},stats.maxHealth.Require(),stats.maxMana.Require());
            inv.TryPickup(inv.CreateInstance("I06R"),OriginalInventoryBag.Hero);
            Assert.That(s.Snapshot().players[0].combat.armor,Is.EqualTo(40.8).Within(1e-7));
            double before=w.UnitState(1).health;Hit(s,269,true);
            // ITEMROAR1 settled native hit: (269-45)/(1+.06*40.8).
            // The previous post-armor policy yielded33.016 instead of64.965.
            Assert.That(before-w.UnitState(1).health,Is.EqualTo(64.96517944335938).Within(.0001));
        }
        [Test] public void CarapaceItemReflectionUsesDeclaredFractionAndStopsWithLastResidentCopy()
        {
            var s=Create(out var w,out var inv);inv.TryPickup(inv.CreateInstance("I076"),OriginalInventoryBag.Hero);
            Call(s,"ReflectNativeCarapace",1001,w.UnitState(1),100.0);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(978).Within(1e-7));
            Assert.That(Call(s,"IncomingInnateWeaponDamage",w.UnitState(1),100.0),Is.EqualTo(100));
            inv.Transfer(OriginalInventoryBag.Hero,0);Call(s,"ReflectNativeCarapace",1001,w.UnitState(1),100.0);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(978).Within(1e-7));
        }
        [Test] public void HighestCorruptionOrbChangesTheFirstWeaponHitAndKeepsItsSixSecondDebuffAfterDrop()
        {
            var s=Create(out var w,out var inv);inv.TryPickup(inv.CreateInstance("I06B"),OriginalInventoryBag.Hero);
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("attacker",1),("owner",1),("target",1001),("kind",OriginalWorldTargetKind.Unit),("attackType","chaos"),("damage",40d),("melee",true)})
                type.GetField(pair.Item1,Hidden).SetValue(shot,pair.Item2);
            Call(s,"ApplyWeaponHit",shot);
            Assert.That(1000-w.UnitState(1001).health,Is.EqualTo(40*(2-Math.Pow(.94,2))).Within(1e-7));
            inv.Transfer(OriginalInventoryBag.Hero,0);for(int i=0;i<119;i++)w.Advance(.05);w.Advance(.04);
            Assert.That(Call(s,"NativeCorruptionArmorDelta",1001),Is.EqualTo(-4));
            w.Advance(.02);Assert.That(Call(s,"NativeCorruptionArmorDelta",1001),Is.Zero);
            var t=Create(out var w2,out var inv2);inv2.TryPickup(inv2.CreateInstance("I08M"),OriginalInventoryBag.Hero);
            inv2.TryPickup(inv2.CreateInstance("I06B"),OriginalInventoryBag.Hero);
            Call(t,"ApplyNativeCorruption",w2.UnitState(1),w2.UnitState(1001));
            Assert.That(Call(t,"NativeCorruptionArmorDelta",1001),Is.Zero,"Higher physical-slot lifesteal orb suppresses corruption.");
        }
        [Test] public void CorruptionTargetMaskIncludesWardsAndExcludesStructures()
        {
            var s=Create(out var w,out var inv);inv.TryPickup(inv.CreateInstance("I06B"),OriginalInventoryBag.Hero);
            w.AddUnit(1002,0,"hhou",new OriginalWorldUnitProfile{maxHealth=500,collisionRadius=16},new OriginalPoint(450,1000));
            w.AddUnit(1003,0,"ohwd",new OriginalWorldUnitProfile{maxHealth=5,collisionRadius=16},new OriginalPoint(550,1000));
            Call(s,"ApplyNativeCorruption",w.UnitState(1),w.UnitState(1002));
            Call(s,"ApplyNativeCorruption",w.UnitState(1),w.UnitState(1003));
            Assert.That(Call(s,"NativeCorruptionArmorDelta",1002),Is.Zero);
            Assert.That(Call(s,"NativeCorruptionArmorDelta",1003),Is.EqualTo(-4));
        }
    }
}

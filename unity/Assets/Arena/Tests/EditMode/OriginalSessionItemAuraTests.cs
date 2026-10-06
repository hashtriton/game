using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
namespace Arena.Tests
{
    public sealed class OriginalSessionItemAuraTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s, string name, params object[] args)
        { var m = typeof(OriginalSession).GetMethod(name, Hidden); Assert.That(m, Is.Not.Null, name); return m.Invoke(s,args); }
        static OriginalSession Create(out OriginalWorld world, out OriginalInventory inventory)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            inventory=(OriginalInventory)p.GetType().GetField("inventory").GetValue(p); return s;
        }
        [Test] public void AuraRegenerationUsesAuthoredFlatValuesAndSameBuffCopiesDoNotMultiply()
        {
            var s=Create(out var w,out var bag);
            foreach(var id in new[]{"I00T","I00W","I00W","I00Z"}) bag.TryPickup(bag.CreateInstance(id),OriginalInventoryBag.Hero);
            Call(s,"AdvanceItemAuras");
            Assert.That(Call(s,"ItemAuraArmorFlat",1),Is.EqualTo(7));
            Assert.That(Call(s,"ItemAuraManaRegen",1,145.0),Is.EqualTo(4));
            Assert.That(Call(s,"ItemAuraHealthRegen",1,631.0),Is.EqualTo(1.5));
            w.ForceUnitDeath(1);for(int i=0;i<11;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(Call(s,"ItemAuraManaRegen",1,145.0),Is.EqualTo(0));
        }
        [Test] public void EnemyAuraTargetsGroundClassAndRestoresRawMovementAfterTransfer()
        {
            var s=Create(out var w,out var bag);
            w.AddUnit(1001,0,"n008",new OriginalWorldUnitProfile{maxHealth=1000,moveSpeed=300,collisionRadius=16},new OriginalPoint(300,1000));
            w.AddUnit(1002,0,"n06L",new OriginalWorldUnitProfile{maxHealth=20,moveSpeed=1,collisionRadius=1},new OriginalPoint(350,1000));
            bag.TryPickup(bag.CreateInstance("I03N"),OriginalInventoryBag.Hero);
            Call(s,"AdvanceItemAuras");
            Assert.That(Call(s,"ItemAuraMovementBonus",1001),Is.EqualTo(-.1));
            Assert.That(Call(s,"ItemAuraAttackSpeedBonus",1001),Is.EqualTo(-.15));
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
            Assert.That(Call(s,"ItemAuraMovementBonus",1002),Is.EqualTo(0),"Ward is not a ground target");
            bag.Transfer(OriginalInventoryBag.Hero,0);for(int i=0;i<11;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(300));
        }
        [Test] public void DawnBootsPulseBothResourcesOncePerSecondAndCopiesDoNotMultiply()
        {
            var s=Create(out var w,out var bag);
            bag.TryPickup(bag.CreateInstance("I0AZ"),OriginalInventoryBag.Hero);
            bag.TryPickup(bag.CreateInstance("I0AZ"),OriginalInventoryBag.Hero);
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,50);w.SetUnitState(1,paused:true);
            Call(s,"AdvanceItemAuras");
            for(int i=0;i<10;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            for(int i=0;i<10;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(w.UnitState(1).health,Is.EqualTo(100+hero.profile.maxHealth*.005).Within(1e-8));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(50+hero.profile.maxMana*.005).Within(1e-8));
            bag.Transfer(OriginalInventoryBag.Hero,0);bag.Transfer(OriginalInventoryBag.Hero,1);
            for(int i=0;i<20;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(w.UnitState(1).health,Is.EqualTo(100+hero.profile.maxHealth*.005).Within(1e-8));
            bag.Transfer(OriginalInventoryBag.Servant,0);
            w.ForceUnitDeath(1);
            for(int i=0;i<20;i++)w.Advance(.05);Call(s,"AdvanceItemAuras");
            Assert.That(w.UnitState(1).health,Is.Zero,"Source threshold must not revive a corpse");
        }
    }
}

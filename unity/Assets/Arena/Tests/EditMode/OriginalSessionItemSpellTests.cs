using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
namespace Arena.Tests
{
    public sealed class OriginalSessionItemSpellTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world,out OriginalInventory bag)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];bag=(OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
            world.AddUnit(1001,0,"edry",new OriginalWorldUnitProfile{maxHealth=500,collisionRadius=16},new OriginalPoint(300,1000));return s;
        }
        [Test] public void NemesisCopiesAmplifyScriptDamageAndMagicVampEvenWhenNativeMagicIsRejected()
        {
            var s=Create(out var w,out var bag);for(int i=0;i<2;i++)bag.TryPickup(bag.CreateInstance("I0AE"),OriginalInventoryBag.Hero);
            Call(s,"AdvanceItemScripts");var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,0);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(500));
            Assert.That(w.UnitState(1).health,Is.EqualTo(106.36).Within(1e-8));
            hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,hero.profile.maxHealth,0);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(3.18).Within(1e-8));
        }
        [Test] public void NativeHelperDamageCannotBorrowItsOwnersSpellItemsAndServantIsExcluded()
        {
            var s=Create(out var w,out var bag);bag.TryPickup(bag.CreateInstance("I05K"),OriginalInventoryBag.Hero);
            bag.TryPickup(bag.CreateInstance("I0AE"),OriginalInventoryBag.Servant);Call(s,"AdvanceItemScripts");
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,0);
            Call(s,"ApplyTriggeredHit",0,1,w.UnitState(1001),100.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100.0,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(110));
        }
        [Test] public void DarkWaveHalvesMagicVampHealthManaAndCurseDamageBeforeTheSourceBranch()
        {
            var s=Create(out var w,out var bag);bag.TryPickup(bag.CreateInstance("I05K"),OriginalInventoryBag.Hero);
            Call(s,"BeginWaveTraits",4);var hero=w.UnitState(1);
            w.UpdateProfile(1,hero.profile,100,0);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(105));
            w.UpdateProfile(1,hero.profile,hero.profile.maxHealth,0);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(2.5));
            var player=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            ((IDictionary)player.GetType().GetField("auxiliaryAbilities").GetValue(player))["A19P"]=1;
            w.UpdateProfile(1,hero.profile,100,0);
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(95));
            Call(s,"EndWaveTraits");
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),100d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(85));
        }
    }
}

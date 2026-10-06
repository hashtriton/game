using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemOrbSecondaryTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(string item,out OriginalWorld w,out OriginalInventory inv,string hero="H008")
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{hero});
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            inv=(OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
            Assert.That(inv.TryPickup(inv.CreateInstance(item),OriginalInventoryBag.Hero).Applied,Is.True);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16,moveSpeed=270},new OriginalPoint(235,1000));
            return s;
        }
        static void FirstHit(OriginalSession s,OriginalWorld w)
        {
            Assert.That(w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001),Is.True);
            Call(s,"AdvanceWeapons");
            for(int i=0;i<12;i++){w.Advance(.05);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(1001).health,Is.LessThan(10000),"The primary hit must actually land.");
        }
        [Test] public void FrostOrbSlowsTheActualPrimaryWeaponVictim()
        {
            foreach(string item in new[]{"I01N","I03N","I0B0"})
            {
                var s=Create(item,out var w,out _);FirstHit(s,w);
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(162).Within(1e-7),item);
                Assert.That(Call(s,"ArcherDebuffAttackSlow",1001),Is.EqualTo(.25),item);
            }
        }
        [Test] public void FireOrbsUseMeasuredFixedDamageWithImmunityHeroAllyAndRadiusControls()
        {
            foreach(var pair in new[]{("I02A",15d),("I02C",75d),("I0A6",75d)})
            {
                var s=Create(pair.Item1,out var w,out _);
                foreach(var body in new[]{(1002,"hfoo",335d,1000d),(1003,"edry",235d,900d),
                    (1004,"H008",235d,1100d),(1005,"hhou",335d,1100d),(1006,"hfoo",455d,1000d)})
                    w.AddUnit(body.Item1,0,body.Item2,new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(body.Item3,body.Item4));
                Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=100000201,ownerSlot=1,sourceHeroEntityId=1,
                    rawcode="hfoo",profile=new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},health=10000,position=new OriginalPoint(135,1100)}}),Is.True);
                FirstHit(s,w);
                Assert.That(10000-w.UnitState(1002).health,Is.EqualTo(pair.Item2).Within(1e-7),pair.Item1);
                Assert.That(10000-w.UnitState(1003).health,Is.EqualTo(pair.Item2).Within(1e-7),"Amim admits this native splash.");
                Assert.That(10000-w.UnitState(1004).health,Is.EqualTo(pair.Item2*.8).Within(1e-7));
                Assert.That(w.UnitState(1005).health,Is.EqualTo(10000),"Declared target mask excludes structures.");
                Assert.That(w.UnitState(1006).health,Is.EqualTo(10000));
                Assert.That(w.UnitState(100000201).health,Is.EqualTo(10000));
            }
        }
        [Test] public void ReleasedFireMissileRetainsItsOrbAfterDropAndDoesNotProcAgainAfterRemoval()
        {
            var s=Create("I02C",out var w,out var inv,"H024");
            w.AddUnit(1002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(335,1000));
            var shots=(IList)typeof(OriginalSession).GetField("projectiles",Hidden).GetValue(s);
            w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001);
            for(int i=0;i<100&&shots.Count==0;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
            Assert.That(shots.Count,Is.EqualTo(1));
            Assert.That(inv.Transfer(OriginalInventoryBag.Hero,0).Applied,Is.True);
            w.Stop(1);
            for(int i=0;i<100;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
            Assert.That(10000-w.UnitState(1002).health,Is.EqualTo(75));
            Assert.That(Call(s,"CaptureItemOrbSecondary",1),Is.Null);
        }
        [Test] public void HighestPhysicalSlotSuppressesOtherOrbsAndServantItemsDoNotProc()
        {
            foreach(bool servant in new[]{false,true})
            {
                var s=Create(servant?"I01N":"I08M",out var w,out var inv);
                if(servant)inv.Transfer(OriginalInventoryBag.Hero,0);
                else inv.TryPickup(inv.CreateInstance("I01N"),OriginalInventoryBag.Hero);
                FirstHit(s,w);Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
            }
        }
        static object Shot(OriginalSession s,int target=1001,bool missed=false)
        {
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("attacker",1),("owner",1),("target",target),("kind",OriginalWorldTargetKind.Unit),
                ("attackType","hero"),("damage",40d),("melee",true),("missed",missed),("itemOrb",Call(s,"CaptureItemOrbSecondary",1))})
                type.GetField(pair.Item1,Hidden).SetValue(shot,pair.Item2);
            return shot;
        }
        [Test] public void MissedShotHasNoOrbEffectsAndSecondaryDamageDoesNotRecursivelySplash()
        {
            var s=Create("I02A",out var w,out _);
            w.AddUnit(1002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(385,1000));
            w.AddUnit(1003,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(535,1000));
            Call(s,"ApplyWeaponHit",Shot(s,missed:true));
            Assert.That(w.UnitState(1001).health,Is.EqualTo(10000));Assert.That(w.UnitState(1002).health,Is.EqualTo(10000));
            w.UpdateProfile(1001,w.UnitState(1001).profile,1,0);
            Call(s,"ApplyWeaponHit",Shot(s));
            Assert.That(w.UnitState(1001).health,Is.Zero);
            Assert.That(w.UnitState(1002).health,Is.EqualTo(9985));
            Assert.That(w.UnitState(1003).health,Is.EqualTo(10000));
        }
        static void BuffTick(OriginalSession s,OriginalWorld w,double seconds)
        {
            while(seconds>1e-9){double dt=Math.Min(.05,seconds);w.Advance(dt);Call(s,"AdvanceArcherDebuffs");seconds-=dt;}
        }
        [Test] public void FrostNormalDurationPausesRefreshesAndCleansesWithoutRequiringTheItemToRemain()
        {
            var s=Create("I01N",out var w,out var inv);Call(s,"ApplyWeaponHit",Shot(s));
            inv.Transfer(OriginalInventoryBag.Hero,0);BuffTick(s,w,1);
            w.SetUnitState(1001,paused:true);BuffTick(s,w,5);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(162).Within(1e-7));
            w.SetUnitState(1001,paused:false);BuffTick(s,w,1.9);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(162).Within(1e-7));
            BuffTick(s,w,.11);Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
            inv.Transfer(OriginalInventoryBag.Servant,0);Call(s,"ApplyWeaponHit",Shot(s));
            BuffTick(s,w,2.9);Call(s,"ApplyWeaponHit",Shot(s));BuffTick(s,w,.2);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(162).Within(1e-7));
            Call(s,"RemoveArcherDebuffs",1001);Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
        }
        [Test] public void NativeHeroGetsOneSecondButItsNonheroImageGetsThreeSeconds()
        {
            var s=Create("I01N",out var w,out _);
            w.AddUnit(1002,0,"H008",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16,moveSpeed=250},new OriginalPoint(335,1000));
            Assert.That(w.TryPublishImages(1,0,OriginalImageFactory.EnemyWand,new[]{new OriginalIllusionSpawn{entityId=1000000001,
                profile=w.UnitState(1).profile,position=new OriginalPoint(435,1000),health=w.UnitState(1).health,mana=w.UnitState(1).mana}}),Is.True);
            // The native predicate itself is independent of captured combat stats.
            Assert.That(Call(s,"IsNativeHeroPredicate",w.UnitState(1002)),Is.True);
            Assert.That(Call(s,"IsNativeHeroPredicate",w.UnitState(1000000001)),Is.False);
            var catalog=(OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Hidden).GetValue(s);
            var native=(OriginalNativeCatalog)typeof(OriginalSession).GetField("native",Hidden).GetValue(s);
            var rules=new OriginalArcherDebuffRules(catalog,native,"A062",1);
            Call(s,"ApplyArcherNativeBuff",1002,rules);
            var statsType=typeof(OriginalSession).GetNestedType("ActorCombatStats",BindingFlags.NonPublic);
            var constructor=statsType.GetConstructor(Hidden,null,new[]{typeof(OriginalHeroStatsSnapshot),typeof(bool),typeof(double)},null);
            var captured=constructor.Invoke(new[]{Call(s,"HeroCombatStats",1),(object)true,0d});
            var captures=(IDictionary)typeof(OriginalSession).GetField("illusionCombatStats",Hidden).GetValue(s);
            captures.Add(1000000001,captured);
            Call(s,"ApplyArcherNativeBuff",1000000001,rules);
            Assert.That(w.UnitState(1002).profile.moveSpeed,Is.EqualTo(150).Within(1e-7));
            BuffTick(s,w,1.01);Assert.That(w.UnitState(1002).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(Call(s,"ArcherDebuffAttackSlow",1000000001),Is.EqualTo(.25));
            BuffTick(s,w,2.0);Assert.That(Call(s,"ArcherDebuffAttackSlow",1000000001),Is.Zero);

        }
        [Test] public void NativeEvasionRejectsBothFrostAndFireSecondaries()
        {
            foreach(string item in new[]{"I01N","I02A"})
            {
                var s=Create(item,out var w,out _);
                w.AddUnit(1002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(335,1000));
                Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15G"},Array.Empty<string>());
                typeof(OriginalSession).GetField("weaponRandom",Hidden).SetValue(s,1u);
                Call(s,"ApplyWeaponHit",Shot(s));
                Assert.That(w.UnitState(1001).health,Is.EqualTo(10000));
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
                Assert.That(w.UnitState(1002).health,Is.EqualTo(10000));
            }
        }

    }
}

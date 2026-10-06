using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionWeaponProcTests
    {
        static readonly BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args=new object[]{null,false};
            var session=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(session);
            world.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,maxMana=145,collisionRadius=31},new OriginalPoint(1000,1000));
            return session;
        }
        static Array Capture(OriginalSession s,OriginalWorld w,string ability)
        {
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{ability},Array.Empty<string>());
            for(int i=0;i<256;i++)
            {
                var procs=(Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1));
                if(procs.Length>0)return procs;
            }
            Assert.Fail("Authored nonzero proc never sampled: "+ability);return null;
        }
        static void Hit(OriginalSession s,Array procs,double damage=40,bool missed=false)
        {
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);
            var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("attacker",1),("owner",1),("target",1001),("kind",OriginalWorldTargetKind.Unit),
                ("attackType","chaos"),("damage",damage),("nativeProcs",procs),("missed",missed)})
                type.GetField(pair.Item1,Hidden).SetValue(shot,pair.Item2);
            Call(s,"ApplyWeaponHit",shot);
        }
        static OriginalItemInstance[] Equip(OriginalSession s,params string[] items)
        {
            var effects=JsonUtility.FromJson<OriginalItemPassiveCatalog>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-item-passives.json")));
            typeof(OriginalSession).GetField("itemEffects",Hidden).SetValue(s,new OriginalInventoryEffects(effects));
            var player=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            var inventory=(OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            var slots=(OriginalItemInstance[])typeof(OriginalInventory).GetField("heroSlots",Hidden).GetValue(inventory);
            for(int i=0;i<items.Length;i++)slots[i]=new OriginalItemInstance{ownerId=1,instanceId=i+1,itemId=items[i]};
            return slots;
        }
        static Array SampleAll(OriginalSession s,OriginalWorld w,int count)
        {
            for(int i=0;i<10000;i++)
            {
                var result=(Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1));
                if(result.Length==count)return result;
            }
            Assert.Fail("Expected ordered item procs never sampled.");return null;
        }
        [Test] public void EquippedCriticalDuplicatesAreCapturedAndRemovedItemsDoNotChangeReleasedShot()
        {
            var s=Create(out var w);var slots=Equip(s,"I008","I009");
            var procs=SampleAll(s,w,2);slots[0]=slots[1]=null;
            Assert.That(((Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1))).Length,Is.Zero);
            Hit(s,procs);Assert.That(w.UnitState(1001).health,Is.EqualTo(10000-40*1.5*1.7/1.12).Within(1e-7));
        }
        [Test] public void ItemBashKeepsItsOwnBuffIdentityAndOrnDoesNotDispelItAsBpse()
        {
            var s=Create(out var w);Equip(s,"I02L");var procs=SampleAll(s,w,1);Hit(s,procs);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(10000-175-40/1.12).Within(1e-7));
            Call(s,"ClearOrnNativeStun",1001);
            Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.True);
            Call(s,"AdvanceNativeWeaponProcs",1.5);Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.False);
        }
        [Test] public void ItemCopiesAreExcludedFromIllusionWeaponProcs()
        {
            var s=Create(out var w);Equip(s,"I008","I00F");
            w.AddIllusion(1000000000,1,new OriginalWorldUnitProfile{maxHealth=100,collisionRadius=24},new OriginalPoint(500,1000),100,0);
            Assert.That(((Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1000000000))).Length,Is.Zero);
        }
        [Test] public void PrimaryWeaponLifestealUsesOnlyActualPrimaryLifeLossAndNotNestedMagicOrMisses()
        {
            var s=Create(out var w);Equip(s,"I08M");
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            var target=w.UnitState(1001);w.UpdateProfile(1001,target.profile,10,target.mana);
            Hit(s,Array.CreateInstance(typeof(OriginalSession).GetNestedType("NativeWeaponProc",BindingFlags.NonPublic),0));
            Assert.That(w.UnitState(1001).health,Is.Zero);
            Assert.That(w.UnitState(1).health,Is.EqualTo(101).Within(1e-7),"Overkill must heal from 10 life lost, not the full rolled hit.");
            s=Create(out w);Equip(s,"I08M");hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            var procs=Capture(s,w,"A0QV");target=w.UnitState(1001);w.UpdateProfile(1001,target.profile,10,target.mana);
            Hit(s,procs);Assert.That(w.UnitState(1001).health,Is.Zero);
            Assert.That(w.UnitState(1).health,Is.EqualTo(100),"Nested magic killed the target before the physical hit.");
            s=Create(out w);Equip(s,"I08M");hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            Hit(s,null,40,true);Assert.That(w.UnitState(1).health,Is.EqualTo(100));
        }
        [Test] public void CriticalMultiplierProducesOnePhysicalHitAndSnapshotSurvivesAbilityRemoval()
        {
            var s=Create(out var w);var procs=Capture(s,w,"A05C");
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A05C"});w.DrainEvents();
            Hit(s,procs);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(10000-80/1.12).Within(1e-7));
            Assert.That(w.DrainEvents().Count(e=>e.kind==OriginalWorldEventKind.UnitDamaged),Is.EqualTo(1));
        }
        [Test] public void FlatCriticalMagicEventPrecedesUnmodifiedPhysicalWeapon()
        {
            var s=Create(out var w);var procs=Capture(s,w,"A0QV");w.DrainEvents();Hit(s,procs);
            var hits=w.DrainEvents().Where(e=>e.kind==OriginalWorldEventKind.UnitDamaged).ToArray();
            Assert.That(hits.Length,Is.EqualTo(2));
            Assert.That(hits[0].health,Is.EqualTo(9980).Within(1e-7));
            Assert.That(hits[1].health,Is.EqualTo(9980-40/1.12).Within(1e-7));
        }
        [Test] public void BashBlocksBeforeMagicAndWeaponDamageAndExpiresWithoutClearingOtherControl()
        {
            var s=Create(out var w);var procs=Capture(s,w,"A0AZ");w.DrainEvents();Hit(s,procs);
            Assert.That((bool)Call(s,"ActorCastBlocked",1001),Is.True);
            var hits=w.DrainEvents().Where(e=>e.kind==OriginalWorldEventKind.UnitDamaged).ToArray();
            Assert.That(hits.Select(e=>e.health),Is.EqualTo(new[]{9900.0,9900-40/1.12}).Within(1e-7));
            Call(s,"SetActorControl",1001,"independent",OriginalActorControlMask.Move,0.0,false,false);
            Call(s,"AdvanceNativeWeaponProcs",.49);Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.True);
            Call(s,"AdvanceNativeWeaponProcs",.01);Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.False);
            Assert.That((bool)Call(s,"ActorMoveBlocked",1001),Is.True);
        }
        [Test] public void FeedbackBurnsManaAndAddsPhysicalDamageWithoutSeparatePositiveSpellHit()
        {
            var s=Create(out var w);var procs=Capture(s,w,"A0RS");w.DrainEvents();Hit(s,procs);
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(115));
            Assert.That(w.UnitState(1001).health,Is.EqualTo(10000-70/1.12).Within(1e-7));
            Assert.That(w.DrainEvents().Count(e=>e.kind==OriginalWorldEventKind.UnitDamaged),Is.EqualTo(1));
            var target=w.UnitState(1001);w.UpdateProfile(1001,target.profile,target.health,12);Hit(s,procs);
            Assert.That(w.UnitState(1001).mana,Is.Zero);
            Assert.That(target.health-w.UnitState(1001).health,Is.EqualTo(52/1.12).Within(1e-7));
        }
        [Test] public void MissesDoNotBurnManaAndCleansingOrPausingBashDoesNotCreateStaleControl()
        {
            var s=Create(out var w);var burn=Capture(s,w,"A0RS");Hit(s,burn,missed:true);
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(145));
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A0RS"});var bash=Capture(s,w,"A0AZ");Hit(s,bash);
            w.SetUnitState(1001,paused:true);Call(s,"AdvanceNativeWeaponProcs",2.0);
            Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.True);
            Call(s,"ClearNegativeActorControls",1001);w.SetUnitState(1001,paused:false);Call(s,"AdvanceNativeWeaponProcs",.1);
            Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.False);
        }
        [Test] public void RealFinalBossCapturesFeedbackTogetherWithPossibleBashWithoutProcCooldown()
        {
            var s=Create(out var w);
            w.AddUnit(1002,0,"O006",new OriginalWorldUnitProfile{maxHealth=30000,maxMana=2500,collisionRadius=32},new OriginalPoint(1500,1000));
            bool combined=false;
            for(int i=0;i<256;i++)
                if(((Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1002))).Length==2){combined=true;break;}
            Assert.That(combined,Is.True);Assert.That(s.HaltReason,Is.Null);
        }
        [Test] public void ActualWeaponReleasePublishesBashAndAbilityClockExpiresItsOwnToken()
        {
            var s=Create(out var w);Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A0AZ"},Array.Empty<string>());
            w.SetUnitState(1,paused:false);Assert.That(w.Relocate(1,new OriginalPoint(930,1000)),Is.True);
            Assert.That(w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001),Is.True);
            for(int i=0;i<12;i++){w.Advance(.05);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(1001).health,Is.LessThan(9900));
            Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.True);
            Call(s,"AdvanceAbilities",.5);Assert.That((bool)Call(s,"ActorWeaponBlocked",1001),Is.False);
        }
    }
}

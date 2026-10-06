using System;
using System.Reflection;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionInnateTests
    {
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld w)
        {
            var args=new object[]{null,false};var s=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);
            typeof(OriginalSession).GetField("observed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(s,
                JsonUtility.FromJson<OriginalObservedCatalog>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-observed126.json"))));
            return s;
        }
        static void Hit(OriginalSession s,int target,double damage,bool melee=false)
        {
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new (string,object)[]{("attacker",1),("owner",1),("target",target),("kind",OriginalWorldTargetKind.Unit),("attackType","chaos"),("damage",damage),("melee",melee)})
                type.GetField(pair.Item1,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(shot,pair.Item2);
            Call(s,"ApplyWeaponHit",shot);
        }
        [Test] public void GalvanizationAuraHealsUndeadWithFlatRateIndependentOfMaximumLife()
        {
            var s=Create(out var w);
            foreach(var row in new[]{(1001,"n01X",5600,1000),(1002,"ugho",330,1100),(1003,"uabo",1080,1150)})
            {
                w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=row.Item3,collisionRadius=8},new OriginalPoint(row.Item4,1000));
                var unit=w.UnitState(row.Item1);w.UpdateProfile(unit.entityId,unit.profile,unit.profile.maxHealth*.25,0);
            }
            // Scan phase is a labelled host policy. After admission, integrate
            // an exact second of the measured flat rate through the real loop.
            for(int i=0;i<10;i++)w.Advance(.05);
            Call(s,"AdvanceNativeHealthAuras");
            for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceRegeneration",.05);}
            Assert.That(w.UnitState(1002).health,Is.EqualTo(82.504).Within(1e-7));
            Assert.That(w.UnitState(1003).health,Is.EqualTo(270.004).Within(1e-7));
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1001)),Is.Zero,"Authored notself.");
        }
        [Test] public void GalvanizationAuraExcludesNonUndeadOutsideAndRemovedOverlayAfterLinger()
        {
            var s=Create(out var w);
            foreach(var row in new[]{(1001,"n01X",1000),(1002,"ugho",1100),(1003,"hfoo",1150),(1004,"ugho",1300)})
                w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(row.Item3,1000));
            for(int i=0;i<10;i++)w.Advance(.05);Call(s,"AdvanceNativeHealthAuras");
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1002)),Is.EqualTo(.004));
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1003)),Is.Zero);
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1004)),Is.Zero);
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"A11G"});
            for(int i=0;i<50;i++){w.Advance(.05);Call(s,"AdvanceNativeHealthAuras");}
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1002)),Is.EqualTo(.004));
            for(int i=0;i<11;i++){w.Advance(.05);Call(s,"AdvanceNativeHealthAuras");}
            Assert.That((double)Call(s,"NativeHealthAuraRate",w.UnitState(1002)),Is.Zero);
        }
        [Test] public void DeclaredCleaveHitsOnlySecondaryHostileGroundTargetsWithinItsArea()
        {
            foreach(bool melee in new[]{false,true})
            {
                var s=Create(out var w);Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A15J"},Array.Empty<string>());
                foreach(var row in new[]{(1001,"hfoo",1000),(1002,"hfoo",1100),(1003,"hdhw",1150),(1004,"hfoo",1210)})
                    w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(row.Item3,1000));
                Hit(s,1001,40,melee);
                Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-40/1.12).Within(1e-7));
                Assert.That(w.UnitState(1002).health,Is.EqualTo(melee?980:1000).Within(1e-7));
                Assert.That(w.UnitState(1003).health,Is.EqualTo(1000));Assert.That(w.UnitState(1004).health,Is.EqualTo(1000));
            }
        }
        [Test] public void ThornsAuraProtectsNearbyAlliesAndStopsAtRemovedEmitter()
        {
            var s=Create(out var w);w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(1000,1000));
            w.AddUnit(1002,0,"n00E",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(1100,1000));
            double before=w.UnitState(1).health;Hit(s,1001,40,true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(before-40*.2*.8).Within(1e-7));
            w.RemoveUnit(1002);Hit(s,1001,40,true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(before-40*.2*.8).Within(1e-7));
        }
        [Test] public void CommandAuraUsesAuthoredNonHeroAndMeleeRangedFlagsWithoutStackingEmitters()
        {
            var s=Create(out var w);
            foreach(var row in new[]{(1001,"hfoo",1000),(1002,"n01C",1100),(1003,"n01C",1200),(1004,"O006",1300)})
                w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(row.Item3,1000));
            Assert.That((double)Call(s,"NativeCommandDamageBonus",w.UnitState(1001),1,100.0),Is.EqualTo(40));
            Assert.That((double)Call(s,"NativeCommandDamageBonus",w.UnitState(1004),1,100.0),Is.Zero);
            Assert.That((double)Call(s,"NativeCommandDamageBonus",w.UnitState(1),1,100.0),Is.Zero);
            w.RemoveUnit(1002);w.RemoveUnit(1003);
            Assert.That((double)Call(s,"NativeCommandDamageBonus",w.UnitState(1001),1,100.0),Is.Zero);
        }
        [Test] public void NativeCarapaceReducesPhysicalWeaponDamageBeforeTheSingleAuthorityHit()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"n00D",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(1000,1000));
            Hit(s,1001,40);Assert.That(w.UnitState(1001).health,Is.EqualTo(992).Within(1e-8));
            Assert.That(w.UnitState(1001).mana,Is.Zero);Assert.That(s.HaltReason,Is.Null);
            // Per-instance removal restores the measured stripped40 control.
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"A15F"});
            Hit(s,1001,40);Assert.That(w.UnitState(1001).health,Is.EqualTo(952).Within(1e-8));
        }
        [Test] public void NativeCarapaceDoesNotInterceptMeasuredMagicAndUniversalModes()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"n00D",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(1000,1000));
            foreach(var mode in new[]{OriginalTriggeredDamageMode.SpellMagic,OriginalTriggeredDamageMode.ChaosUniversal})
                Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,mode);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(920).Within(1e-8));
        }
        [Test] public void NativeCarapaceReflectionLosesOneActualSourceHealthWithoutArmorOrRecursiveReflection()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(1000,1000));
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15F"},Array.Empty<string>());
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A15F"},Array.Empty<string>());
            double hp=w.UnitState(1).health;w.DrainEvents();Hit(s,1001,40,melee:true);
            var hits=Array.FindAll(w.DrainEvents(),e=>e.kind==OriginalWorldEventKind.UnitDamaged);
            Assert.That(hits.Length,Is.EqualTo(2));Assert.That(hits[0].entityId,Is.EqualTo(1));
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-1));
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-40/1.12*.2).Within(1e-7));
            hp=w.UnitState(1).health;Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-1));
        }
        [Test] public void NativeCarapaceReflectionSkipsZeroMagicUniversalAndRemovedAbility()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(1000,1000));
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15F"},Array.Empty<string>());double hp=w.UnitState(1).health;
            foreach(var mode in new[]{OriginalTriggeredDamageMode.SpellNormal,OriginalTriggeredDamageMode.SpellMagic,OriginalTriggeredDamageMode.ChaosUniversal})
                Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),mode==OriginalTriggeredDamageMode.SpellNormal?0.0:40.0,mode);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"A15F"});Hit(s,1001,40,melee:true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
        }
        [Test] public void NativeCarapaceReducesSpellsNormalButRemovingAbilityRestoresFullDamage()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(1000,1000));
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15F"},Array.Empty<string>());
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-40/1.12*.2).Within(1e-7));
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"A15F"});
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-40/1.12*1.2).Within(1e-7));
        }
        [Test] public void CorruptionAppliesBeforeFirstWeaponHitAndOnlyLastsDeclaredPointZeroOne()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(1000,1000));
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A0QZ"},Array.Empty<string>());
            Hit(s,1001,152);
            double expected=152*(2-Math.Pow(.94,8));
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-expected).Within(1e-7));
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000-expected-40*(2-Math.Pow(.94,8))).Within(1e-7));
            w.Advance(.01);
            Assert.That((double)Call(s,"NativeCorruptionArmorDelta",1001),Is.Zero);
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A0QZ"});
            double before=w.UnitState(1001).health;Hit(s,1001,152);
            Assert.That(before-w.UnitState(1001).health,Is.EqualTo(152/1.12).Within(1e-7));
        }
        [Test] public void RealSummonCorruptionChangesHeroArmorAndCleanseRestoresCapturedBaseline()
        {
            var s=Create(out var w);Infernal(w,"n07C");
            var source=w.UnitState(1001);var target=w.UnitState(1);
            Call(s,"ApplyNativeCorruption",source,target);
            Assert.That((double)Call(s,"NativeCorruptionArmorDelta",1),Is.EqualTo(-10));
            Call(s,"RemoveNativeCorruption",1);
            Assert.That((double)Call(s,"NativeCorruptionArmorDelta",1),Is.Zero);
        }
        [Test] public void OrdinaryCreepCorruptionUsesItsAuthoredFiveArmorRatherThanSummonTen()
        {
            foreach(string rawcode in new[]{"n00N","n00O"})
            {
                var s=Create(out var w);Infernal(w,rawcode);
                Call(s,"ApplyNativeCorruption",w.UnitState(1001),w.UnitState(1));
                Assert.That((double)Call(s,"NativeCorruptionArmorDelta",1),Is.EqualTo(-5));
                w.Advance(.01);
                Assert.That((double)Call(s,"NativeCorruptionArmorDelta",1),Is.Zero);
            }
        }
        [Test] public void FinalBossEnduranceAddsDeclaredAttackSpeedWithoutRepeatingObservedMovementBonus()
        {
            var s=Create(out var w);
            w.AddUnit(1001,0,"O006",new OriginalWorldUnitProfile{maxHealth=30000,maxMana=2500,moveSpeed=390,collisionRadius=32},new OriginalPoint(1000,1000));
            Assert.That((double)Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(4.25));
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(390));
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"SCae"});
            Assert.That((double)Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(3.5));
        }
        [Test] public void DeclaredEvasionSkipsWeaponCallbacksButPausedTargetsAndSpellsRemainHittable()
        {
            foreach(string ability in new[]{"AEev","A15G"})
            {
                var s=Create(out var w);
                w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=100000,collisionRadius=31},new OriginalPoint(1000,1000));
                Call(s,"ApplyUnitAbilityOverlay",1001,new[]{ability},Array.Empty<string>());
                int misses=0,hits=0;
                for(int i=0;i<128;i++)
                {
                    double hp=w.UnitState(1001).health;Hit(s,1001,40);
                    if(w.UnitState(1001).health==hp)misses++;else hits++;
                }
                Assert.That(misses,Is.GreaterThan(0));Assert.That(hits,Is.GreaterThan(0));
                w.SetUnitState(1001,paused:true);double before=w.UnitState(1001).health;
                for(int i=0;i<8;i++)Hit(s,1001,40);
                Assert.That(before-w.UnitState(1001).health,Is.EqualTo(8*40/1.12).Within(1e-6));
                w.SetUnitState(1001,paused:false);before=w.UnitState(1001).health;
                Call(s,"ApplyTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
                Assert.That(before-w.UnitState(1001).health,Is.EqualTo(40/1.12).Within(1e-6));
            }
        }

        static void StepInnates(OriginalSession s, OriginalWorld w, double seconds)
        {
            while(seconds>1e-8)
            {
                double step=Math.Min(.05,seconds); w.Advance(step);
                Call(s,"AdvanceAbilities",step); seconds-=step;
            }
        }
        static void Infernal(OriginalWorld w,string id="n06W")
        {
            var hero=w.UnitState(1);
            w.AddUnit(1001,0,id,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},
                new OriginalPoint(hero.position.x+100,hero.position.y));
        }
        [Test] public void InfernalPermanentImmolationUsesDeclaredTenMagicDamageEachSecondWithoutNumericArmor()
        {
            foreach(var id in new[]{"n06W","n025"})
            {
                var s=Create(out var w); Infernal(w,id); double initial=w.UnitState(1).health;
                StepInnates(s,w,.95);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
                StepInnates(s,w,.05);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
                StepInnates(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-16).Within(1e-7));
                Assert.That(s.HaltReason,Is.Null);
            }
        }
        [Test] public void ImmolationChecksLiveRangeImmunityAndAbilityRemovalAtEveryPulse()
        {
            var s=Create(out var w);Infernal(w);double initial=w.UnitState(1).health;
            var origin=w.UnitState(1).position;
            Assert.That(w.Relocate(1,new OriginalPoint(origin.x-121,origin.y)),Is.True);
            StepInnates(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            Assert.That(w.Relocate(1,origin),Is.True);
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"ACmi"},Array.Empty<string>());
            StepInnates(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"ACmi"});
            StepInnates(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"ANpi"});
            StepInnates(s,w,2);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
        }
        [Test] public void MeasuredInfernalLandingSeedsFirstPulseAtPointZeroOneThenOneSecondPhase()
        {
            var s=Create(out var w);Infernal(w,"n025");double initial=w.UnitState(1).health;
            Assert.That(w.SetVisibility(1001,false),Is.True);StepInnates(s,w,1);
            Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            Assert.That(w.SetVisibility(1001,true),Is.True);Call(s,"SeedNativeImmolationLanding",1001);
            Call(s,"AdvancePermanentImmolations",.05);
            Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            StepInnates(s,w,.005);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            StepInnates(s,w,.005);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
            StepInnates(s,w,.999);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
            StepInnates(s,w,.001);Assert.That(w.UnitState(1).health,Is.EqualTo(initial-16).Within(1e-7));
        }
        [Test] public void ImmolationHostSchedulerFreezesHiddenOrPausedEmittersAndStopsAfterDeath()
        {
            var s=Create(out var w);Infernal(w);double initial=w.UnitState(1).health;
            StepInnates(s,w,.5);w.SetUnitState(1001,paused:true);
            StepInnates(s,w,2);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            w.SetUnitState(1001,paused:false);Assert.That(w.SetVisibility(1001,false),Is.True);
            StepInnates(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(initial));
            Assert.That(w.SetVisibility(1001,true),Is.True);StepInnates(s,w,.5);
            Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
            w.ForceUnitDeath(1001);StepInnates(s,w,2);
            Assert.That(w.UnitState(1).health,Is.EqualTo(initial-8).Within(1e-7));
        }
    }
}

using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionOrdinarySpellTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)
        {var m=typeof(OriginalSession).GetMethod(name,Hidden);Assert.That(m,Is.Not.Null,name);return m.Invoke(s,args);}
        static OriginalSession Create(out OriginalWorld w,string raw)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var h=w.UnitState(1);w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=10000,maxMana=h.profile.maxMana,
                moveSpeed=250,collisionRadius=h.profile.collisionRadius},10000,h.mana);
            w.AddUnit(9001,0,raw,new OriginalWorldUnitProfile{maxHealth=1000,maxMana=5000,moveSpeed=300,collisionRadius=16},new OriginalPoint(300,1000));return s;
        }
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double d=Math.Min(.01,seconds);w.Advance(d);Call(s,"AdvanceOrdinaryNativeSpells");seconds-=d;}}
        static void Invisible(OriginalSession s)
        {
            // Combat fixture has an intentionally enlarged HP profile. Public
            // inventory/profile transactions have their own ItemUse coverage.
            Call(s,"ApplyNativeItemStatus",1,Call(s,"NativeItemAction","I06M"));
            Call(s,"AdvanceItemStatuses",2.01d);
        }
        [Test] public void InvisibleTargetCannotBeAcquiredOrTargetCastButAreaDamageStillHits()
        {
            var s=Create(out var w,"n00M");Invisible(s);
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A064",1),Is.False);
            Call(s,"AdvanceWorldAi");Assert.That(w.UnitState(9001).order,Is.Not.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(w.TryAttackTarget(9001,OriginalWorldTargetKind.Unit,1),Is.True);
            Call(s,"AdvanceWeapons");Assert.That(w.UnitState(9001).attackSequence,Is.Zero);
            Assert.That(w.UnitState(9001).order,Is.EqualTo(OriginalWorldOrder.None));
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A046",1);
            Assert.That(w.UnitState(1).health,Is.EqualTo(9880).Within(.001));
        }
        [Test] public void ReleasedMissileHitsNewlyInvisibleTargetAndActualAttackRevealsItsSource()
        {
            var s=Create(out var w,"n01D");w.TryAttackTarget(9001,OriginalWorldTargetKind.Unit,1);
            Call(s,"AdvanceWeapons");
            for(int i=0;i<50;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000));Invisible(s);
            for(int i=0;i<15;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(1).health,Is.LessThan(10000));
            Assert.That(Call(s,"ItemInvisibilityActive",1),Is.True);
            w.ForcePosition(1,new OriginalPoint(200,1000));w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,9001);
            Call(s,"AdvanceWeapons");Assert.That(Call(s,"ItemInvisibilityActive",1),Is.False);
        }
        [Test] public void PermanentInvisibilityFadesWhileMovingAndRestartsAfterAttackReveal()
        {
            var s=Create(out var w,"n00M");Call(s,"AdvanceNativeInvisibility");
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.True);
            for(int i=0;i<200;i++){w.Advance(.01);Call(s,"AdvanceNativeInvisibility");}
            Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.True);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.False);
            w.TryMove(9001,new OriginalPoint(320,1000));w.Advance(.01);Call(s,"AdvanceNativeInvisibility");
            Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.True);
            Call(s,"RevealItemInvisibility",9001);Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.False);
            for(int i=0;i<200;i++){w.Advance(.01);Call(s,"AdvanceNativeInvisibility");}
            Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.True);
        }
        [Test] public void AlliedDetectorRevealsWithinItsDeclaredRadiusAndRemovalRestoresConcealment()
        {
            var s=Create(out var w,"n00M");Call(s,"AdvanceNativeInvisibility");
            for(int i=0;i<201;i++){w.Advance(.01);Call(s,"AdvanceNativeInvisibility");}
            var row=new OriginalWorldSummonSpawn{entityId=100000091,ownerSlot=1,sourceHeroEntityId=1,rawcode="n0AD",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},position=new OriginalPoint(799,1000),health=1000};
            Assert.That(w.TryPublishSummons(new[]{row}),Is.True);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.True);
            w.ForcePosition(row.entityId,new OriginalPoint(801,1000));
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.False);
            w.ForcePosition(row.entityId,new OriginalPoint(799,1000));w.ForceUnitDeath(row.entityId);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.False);
        }
        [Test] public void CrippleDebitsAtEffectAndCombinesWhiteDamageMovementAndAttackSlow()
        {
            var s=Create(out var w,"n00M");double rate=(double)Call(s,"WeaponRate",w.UnitState(1));
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A064",1),Is.True);
            Tick(s,w,.49);Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));Tick(s,w,.01);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4900));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(100).Within(1e-7));
            Assert.That(Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(rate-.6).Within(1e-7));
            Assert.That(Call(s,"ApplyCrippleWeaponDamage",1,101d,12d),Is.EqualTo(82));
            Tick(s,w,10.01);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void FaerieArmorAndHowlDamageUseSeparateDeclaredModifiersAndDispelRestoresBoth()
        {
            var s=Create(out var w,"n02D");var before=(double)Call(s,"EnemyArmor",((OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Hidden).GetValue(s)).Unit("n02D"),9001);
            Call(s,"ResolveOrdinaryNativeSpell",1,"A0B0",9001);
            var after=(double)Call(s,"EnemyArmor",((OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Hidden).GetValue(s)).Unit("n02D"),9001);
            Assert.That(after,Is.EqualTo(before-30));
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A0GQ",1);
            Assert.That(Call(s,"ApplyCrippleWeaponDamage",1,101d,12d),Is.EqualTo(37));
            Call(s,"RemoveOrdinaryNativeBuffs",1);Assert.That(Call(s,"ApplyCrippleWeaponDamage",1,101d,12d),Is.EqualTo(113));
        }
        [Test] public void RejuvenationHealsDeclaredTotalAndDoesNotReviveAfterTargetDeath()
        {
            var s=Create(out var w,"n00G");var a=w.UnitState(9001);w.UpdateProfile(9001,a.profile,100,a.mana);
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A04E",9001),Is.True);Tick(s,w,.5);Tick(s,w,6);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(700).Within(.0001));
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A04E",9001);w.ForceUnitDeath(9001);Tick(s,w,2);
            Assert.That(w.UnitState(9001).health,Is.Zero);
        }
        [Test] public void ReleasedBoltSurvivesSourceDeathAndRespectsAuthoredMissileSpeed()
        {
            var s=Create(out var w,"n00Q");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A06Z",1),Is.True);
            Tick(s,w,.3);w.ForceUnitDeath(9001);Tick(s,w,.23);Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            Tick(s,w,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(9680).Within(1e-7));Assert.That(Call(s,"ActorCastBlocked",1),Is.True);
        }
        [Test] public void ThunderclapAppliesDeclaredDamageAndSlowButDoesNotHitFlyingTargets()
        {
            var s=Create(out var w,"n009");
            var air=new OriginalWorldSummonSpawn{entityId=100000000,ownerSlot=1,sourceHeroEntityId=1,rawcode="hgyr",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},position=new OriginalPoint(350,1000),health=1000};
            Assert.That(w.TryPublishSummons(new[]{air}),Is.True);
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A046",1);
            Assert.That(w.UnitState(1).health,Is.EqualTo(9880).Within(1e-7));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(150).Within(1e-7));
            Assert.That(Call(s,"OrdinarySpellTarget",w.UnitState(9001),w.UnitState(1),"A046"),Is.True);
            Assert.That(w.UnitState(air.entityId).health,Is.EqualTo(1000));
        }
        [Test] public void InterruptedOrControlledCastDoesNotSpendAndUnsupportedFamilyDoesNotHalt()
        {
            var s=Create(out var w,"n00M");Call(s,"TryStartOrdinaryNativeSpell",9001,"A064",1);
            w.Stop(9001);Call(s,"OnAcceptedWorldOrder",9001);Tick(s,w,.7);Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));
            Call(s,"AddTimedNativeStun",9001,"BPSE",1,2d);Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A064",1),Is.False);
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A0AN",1),Is.False);Assert.That(s.HaltReason,Is.Null);
        }
        [Test] public void AiStartsNativeCastAndNegativeCleanseKeepsRejuvenation()
        {
            var s=Create(out var w,"n00M");
            Call(s,"SelectOrdinaryNativeSpellOrders");Tick(s,w,.5);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4900));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(100));
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            Call(s,"ResolveOrdinaryNativeSpell",1,"A04E",1);
            Call(s,"ClearNegativeActorControls",1);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Tick(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(200).Within(.0001));
        }
        [Test] public void DeathCoilDealsHalfToLivingEnemyAndHealsFriendlyUndead()
        {
            var s=Create(out var w,"n00J");
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A04T",1),Is.True);
            Tick(s,w,.65);Assert.That(w.UnitState(1).health,Is.EqualTo(9800).Within(.0001));
            w.AddUnit(9002,0,"ugho",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(400,1000));
            var ally=w.UnitState(9002);w.UpdateProfile(9002,ally.profile,100,0);
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A04T",9002);Tick(s,w,.1);
            Assert.That(w.UnitState(9002).health,Is.EqualTo(600));
            Assert.That(Call(s,"OrdinarySpellTarget",w.UnitState(9001),w.UnitState(9001),"A04T"),Is.False);
        }
        [Test] public void ManaBurnUsesCurrentManaAtDeclaredBoltDelayAndNeverGoesNegative()
        {
            var s=Create(out var w,"n027");var h=w.UnitState(1);w.UpdateProfile(1,h.profile,h.health,75);
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A0AN",1),Is.True);
            Tick(s,w,.74);Assert.That(w.UnitState(1).mana,Is.EqualTo(75));Tick(s,w,.01);
            Assert.That(w.UnitState(1).mana,Is.Zero);Assert.That(w.UnitState(1).health,Is.EqualTo(9940).Within(.0001));
            Tick(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(9940).Within(.0001));
        }
        [Test] public void FrostNovaHitsPrimaryAndAreaOnceAndUsesSharedBfro()
        {
            var s=Create(out var w,"n00V");
            var summon=new OriginalWorldSummonSpawn{entityId=100000000,ownerSlot=1,sourceHeroEntityId=1,rawcode="hfoo",
                profile=new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=16,moveSpeed=300},position=new OriginalPoint(135,1150),health=2000};
            Assert.That(w.TryPublishSummons(new[]{summon}),Is.True);
            Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A09S",1),Is.True);Tick(s,w,.3);
            Assert.That(w.UnitState(1).health,Is.EqualTo(9360).Within(.0001));
            Assert.That(w.UnitState(summon.entityId).health,Is.EqualTo(1600).Within(.0001));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(150));
            Call(s,"OrderArcherNativeHelper",0,1,"A168",1);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(150));
        }
        [Test] public void ItemScriptSlowUsesSharedRateMovementAndNegativeCleanse()
        {
            var s=Create(out var w,"hfoo");double rate=(double)Call(s,"WeaponRate",w.UnitState(1));
            Call(s,"ApplyItemScriptDebuff",w.UnitState(1),"A0OR");
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(175));
            Assert.That(Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(rate-.3).Within(.000001));
            Call(s,"ClearNegativeActorControls",1);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(rate));
        }
        [Test] public void MeasuredSparseCreepsCastImmediatelyWithDifferentAttackRecovery()
        {
            foreach(var row in new[]{("n009","A046",200d,120d,true),("n019","A073",100d,200d,false)})
            {
                var s=Create(out var w,row.Item1);
                Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,row.Item2,1),Is.True);
                Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000-row.Item3));
                Assert.That(w.UnitState(1).health,Is.EqualTo(10000-row.Item4));
                Assert.That(Call(s,"AutomaticAttackRecovery",9001),Is.EqualTo(row.Item5));
                Call(s,"OnAcceptedWorldOrder",9001);Assert.That(Call(s,"AutomaticAttackRecovery",9001),Is.False);
                Assert.That(w.UnitState(1).health,Is.EqualTo(10000-row.Item4),"Post-effect order does not refund a completed instant spell.");
            }
        }
        [Test] public void RootsBlockMovementAndWeaponButPermitCastingAndPersistAfterCasterDeath()
        {
            var s=Create(out var w,"n01A");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A074",1),Is.True);
            Tick(s,w,.5);Assert.That(w.UnitState(9001).mana,Is.EqualTo(4900));
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.True);Assert.That(Call(s,"ActorWeaponBlocked",1),Is.True);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);Assert.That(w.UnitState(1).profile.moveSpeed,Is.Zero);
            w.ForceUnitDeath(9001);Tick(s,w,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(9928));
            Tick(s,w,4.99);Assert.That(w.UnitState(1).health,Is.EqualTo(9640));
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.False);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void ShadowStrikeDecaysEachSecondAndHasAnIndependentThreeSecondDamagePulse()
        {
            var s=Create(out var w,"n01D");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A07A",1),Is.True);
            Tick(s,w,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(9920));
            foreach(double speed in new[]{50d,122d,178d,218d,242d})
            {Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(speed).Within(.0001));Tick(s,w,1);}
            Assert.That(w.UnitState(1).health,Is.EqualTo(9840));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void PurgeHasFiveMovementStepsWithoutWeaponOrCastBlockAndDispelStopsItsTimer()
        {
            var s=Create(out var w,"n029");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"ACpu",1),Is.True);
            Tick(s,w,.55);
            foreach(double speed in new[]{1d,50d,100d,150d,200d})
            {Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(speed).Within(.0001));Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
                Assert.That(Call(s,"ActorWeaponBlocked",1),Is.False);Tick(s,w,1);}
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Call(s,"ResolveOrdinaryNativeSpell",9001,"ACpu",1);Call(s,"ClearNegativeActorControls",1);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));Tick(s,w,1);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void PolymorphAffectsHeroAndSummonWithAbsoluteHundredSpeedAndSixSecondControl()
        {
            foreach(bool summoned in new[]{false,true})
            {
                var s=Create(out var w,"n01U");int target=1;
                if(summoned){target=100000020;Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=target,ownerSlot=1,
                    sourceHeroEntityId=1,rawcode="hfoo",profile=new OriginalWorldUnitProfile{maxHealth=420,moveSpeed=270,collisionRadius=16},
                    position=new OriginalPoint(400,1000),health=420}}),Is.True);}
                Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A0B1",target),Is.True);
                Assert.That(w.UnitState(9001).mana,Is.EqualTo(4800));Assert.That(w.UnitState(target).profile.moveSpeed,Is.EqualTo(100).Within(.0001));
                Assert.That(Call(s,"ActorMoveBlocked",target),Is.False);Assert.That(Call(s,"ActorWeaponBlocked",target),Is.True);
                Assert.That(Call(s,"ActorCastBlocked",target),Is.True);Tick(s,w,6);
                Assert.That(Call(s,"ActorCastBlocked",target),Is.False);Assert.That(w.UnitState(target).profile.moveSpeed,Is.EqualTo(summoned?270:250));
            }
        }
        [Test] public void JetHexUsesMeasuredHundredSpeedAndTwoSecondControlsWithoutExtraDamageReduction()
        {
            foreach(bool summoned in new[]{false,true})
            {
                var s=Create(out var w,"hfoo");int target=1;
                if(summoned){target=100000020;Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=target,ownerSlot=1,
                    sourceHeroEntityId=1,rawcode="hfoo",profile=new OriginalWorldUnitProfile{maxHealth=420,moveSpeed=270,collisionRadius=16},
                    position=new OriginalPoint(400,1000),health=420}}),Is.True);}
                double hp=w.UnitState(target).health;
                Call(s,"ApplyNativeTriggeredHit",9001,0,w.UnitState(target),40d,OriginalTriggeredDamageMode.SpellMagic);
                double baseline=hp-w.UnitState(target).health;
                Call(s,"ApplyItemJetHex",w.UnitState(target),9001);
                Assert.That(w.UnitState(target).profile.moveSpeed,Is.EqualTo(100).Within(.0001));
                Assert.That(Call(s,"ActorMoveBlocked",target),Is.False);Assert.That(Call(s,"ActorWeaponBlocked",target),Is.True);
                Assert.That(Call(s,"ActorCastBlocked",target),Is.True);
                hp=w.UnitState(target).health;Call(s,"ApplyNativeTriggeredHit",9001,0,w.UnitState(target),40d,OriginalTriggeredDamageMode.SpellMagic);
                Assert.That(hp-w.UnitState(target).health,Is.EqualTo(baseline).Within(.0001));
                Tick(s,w,1.99);Assert.That(Call(s,"ActorCastBlocked",target),Is.True);
                Tick(s,w,.02);Assert.That(Call(s,"ActorCastBlocked",target),Is.False);
                Assert.That(w.UnitState(target).profile.moveSpeed,Is.EqualTo(summoned?270:250).Within(.0001));
            }
        }
        [Test] public void HexDispelPauseAndPolymorphOverlapRestoreTheOriginalMovementBase()
        {
            var s=Create(out var w,"n01U");Call(s,"ResolveOrdinaryNativeSpell",9001,"A0B1",1);
            Call(s,"ApplyItemJetHex",w.UnitState(1),9001);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(100).Within(.0001),"Two absolute transformations share one host movement cap.");
            Tick(s,w,2.01);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(100).Within(.0001));
            Call(s,"ClearNegativeActorControls",1);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Call(s,"ApplyItemJetHex",w.UnitState(1),9001);w.SetUnitState(1,paused:true);Tick(s,w,3);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True,"Pause freezing is an explicit host policy for hex.");
            w.SetUnitState(1,paused:false);Tick(s,w,2.01);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void ImmolationIsInstantDrainsManaAndUsesLiveAreaMembershipWithoutRepeatedCastDebit()
        {
            var s=Create(out var w,"n015");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A05Y",1),Is.True);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4975));Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A05Y",1),Is.False);
            Tick(s,w,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(9952));
            w.ForcePosition(1,new OriginalPoint(-1000,1000));Tick(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(9952));
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4975-25*1.01).Within(.0001));
            w.ForcePosition(1,new OriginalPoint(135,1000));Tick(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(9904));
            w.ForceUnitDeath(9001);Tick(s,w,2);Assert.That(w.UnitState(1).health,Is.EqualTo(9904));
        }
        [Test] public void CarrionSwarmNativeZeroAndSourcePercentWaveAreSeparateAndSourceDeathDoesNotCancel()
        {
            var s=Create(out var w,"n00E");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A0RA",1),Is.True);
            Tick(s,w,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(10000));Assert.That(w.UnitState(9001).mana,Is.EqualTo(4900));
            w.ForceUnitDeath(9001);Tick(s,w,.03);Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            Tick(s,w,.03);Assert.That(w.UnitState(1).health,Is.EqualTo(8400));
            Tick(s,w,1);Assert.That(w.UnitState(1).health,Is.EqualTo(8400),"Retained source group permits only one percent hit.");
        }
        [Test] public void RejuvenationSourceCompanionHealsNearbyAllySeparatelyFromMainTarget()
        {
            var s=Create(out var w,"n00G");w.AddUnit(9002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(350,1000));
            var ally=w.UnitState(9002);w.UpdateProfile(9002,ally.profile,100,0);
            Call(s,"ResolveOrdinaryNativeSpell",9001,"A04E",9001);Tick(s,w,6);
            Assert.That(w.UnitState(9002).health,Is.EqualTo(700).Within(.0001));
        }
        [Test] public void CarrionSourceRetainsAnInvulnerableRecipientBeforeItsProtectionExpires()
        {
            var s=Create(out var w,"n00E");Assert.That(Call(s,"TryStartOrdinaryNativeSpell",9001,"A0RA",1),Is.True);
            Tick(s,w,.5);w.SetUnitState(1,invulnerable:true);Tick(s,w,.06);
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            w.SetUnitState(1,invulnerable:false);Tick(s,w,.2);
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000),"ILv adds the recipient to its retained group even when native damage is rejected.");
        }
    }
}

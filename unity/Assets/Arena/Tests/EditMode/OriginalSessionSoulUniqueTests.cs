using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionSoulUniqueTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)
        {
            var method=typeof(OriginalSession).GetMethod(name,Hidden);
            if(method==null)Assert.Fail("Unique soul effect is not implemented: "+name);
            return method.Invoke(s,args);
        }
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
        static OriginalWorld World(OriginalSession s)=>(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
        static OriginalSession Create(string id="H008")=>(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{id});
        static void Grant(OriginalSession s,string id)
        {Assert.That(Call(s,"CanApplyUniqueSoulUpgrade",Player(s),id),Is.True);Call(s,"ApplyUniqueSoulUpgrade",Player(s),id);}
        static void Advance(OriginalSession s,double seconds)
        {
            while(seconds>1e-9){double step=Math.Min(.05,seconds);World(s).Advance(step);Call(s,"AdvanceUniqueSoulEffects");seconds-=step;}
        }
        [Test] public void CataclysmChangesOnlyPrimaryAndAppliesSourceVitalityBranchesOnce()
        {
            foreach(string id in new[]{"H008","N0A0","H024"})
            {
                var s=Create(id);var w=World(s);var before=w.UnitState(1);w.UpdateProfile(1,before.profile,100,20);
                var stats=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);Grant(s,"R00G");
                var after=w.UnitState(1);var changed=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
                Assert.That(changed.primary.Require(),Is.EqualTo(stats.primary.Require()+50));
                Assert.That(changed.strength.Require()-stats.strength.Require(),Is.EqualTo(id=="H008"?50:0));
                Assert.That(changed.agility.Require()-stats.agility.Require(),Is.EqualTo(id=="N0A0"?50:0));
                Assert.That(changed.intelligence.Require()-stats.intelligence.Require(),Is.EqualTo(id=="H024"?50:0));
                Assert.That(after.health,Is.EqualTo(id=="H008"?after.profile.maxHealth:100));
                Assert.That(after.mana,Is.EqualTo(id=="H024"?after.profile.maxMana:20));
                Grant(s,"R00G");Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(after.profile.maxHealth));
                Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).primary.Require(),Is.EqualTo(changed.primary.Require()));
            }
        }
        [Test] public void SoulPathingGrantRetainsOrdersAndPublishesExactEvasionChild()
        {
            var s=Create();var w=World(s);w.SetUnitState(1,paused:false);w.TryMove(1,new OriginalPoint(700,1000));
            Grant(s,"R00H");var actor=w.UnitState(1);
            Assert.That(actor.pathingDisabled,Is.True);Assert.That(actor.order,Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(Call(s,"HasEffectiveUnitAbility",actor,"A1DS"),Is.True);
            bool miss=false,hit=false;
            for(int i=0;i<100;i++){bool result=(bool)Call(s,"RollNativeEvasion",actor);miss|=result;hit|=!result;}
            Assert.That(miss&&hit,Is.True,"Declared15% feeds the common evasion resolver, not a frequency proof.");
        }
        [Test] public void RoundRestoreReappliesSoulPathingAfterTemporaryForceEnabledIt()
        {
            var s=Create();var w=World(s);Grant(s,"R00H");
            // Force effects explicitly enable pathing when they end. A3 must
            // restore the persistent grant on the next party restoration.
            w.SetPathingEnabled(1,true);Assert.That(w.UnitState(1).pathingDisabled,Is.False);
            Call(s,"ApplyWorldEvent",new OriginalMatchEvent{kind=OriginalMatchEventKind.RestoreParty});
            Assert.That(w.UnitState(1).pathingDisabled,Is.True);
            var ordinary=Create();Call(ordinary,"ApplyWorldEvent",new OriginalMatchEvent{kind=OriginalMatchEventKind.RestoreParty});
            Assert.That(World(ordinary).UnitState(1).pathingDisabled,Is.False);
        }
        [Test] public void TitanPulseUsesStrengthWhilePausedAndSkipsHiddenWithoutAccumulating()
        {
            var s=Create();var w=World(s);var hero=w.UnitState(1);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(hero.position.x+100,hero.position.y));
            w.SetUnitState(9001,paused:true);Grant(s,"R00F");
            Advance(s,1.95);Assert.That(w.UnitState(9001).health,Is.EqualTo(1000));
            Advance(s,.05);Assert.That(w.UnitState(9001).health,Is.LessThan(1000));
            double after=w.UnitState(9001).health;w.SetVisibility(1,false);Advance(s,2);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(after));w.SetVisibility(1,true);Call(s,"AdvanceUniqueSoulEffects");
            Assert.That(w.UnitState(9001).health,Is.EqualTo(after));
        }
        [Test] public void AbyssAuraAffectsEnemyBaseArmorAndStopsWhenEmitterHidden()
        {
            var s=Create();var w=World(s);var hero=w.UnitState(1);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(hero.position.x+100,hero.position.y));
            Grant(s,"R00E");Assert.That(Call(s,"UniqueSoulArmorFraction",9001),Is.EqualTo(-.15));
            var combat=(OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Hidden).GetValue(s);
            Assert.That(Call(s,"EnemyArmor",combat.Unit("hfoo"),9001),Is.EqualTo(1.7).Within(.000001));
            Assert.That(Call(s,"UniqueSoulArmorFraction",1),Is.EqualTo(0));w.SetVisibility(1,false);
            Assert.That(Call(s,"UniqueSoulArmorFraction",9001),Is.EqualTo(0));
        }
        [Test] public void BladeWavesUseThreeIndependentHitSetsAndSourceExpandingRadius()
        {
            var s=Create();var w=World(s);var hero=w.UnitState(1);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=8},new OriginalPoint(hero.position.x+100,hero.position.y));
            w.SetUnitState(9001,paused:true);Grant(s,"R00J");
            Call(s,"BeginUniqueSoulBladeWaves",hero,w.UnitState(9001));
            w.Advance(.03);Call(s,"AdvanceUniqueSoulEffects");double hit=10000-w.UnitState(9001).health;
            double cm=(double)Call(s,"ScriptedAbilityAttack",Player(s));Assert.That(hit,Is.EqualTo(cm*.33*3).Within(.0001));
            Advance(s,.78);Assert.That(10000-w.UnitState(9001).health,Is.EqualTo(hit).Within(.0001));
        }
        [Test] public void PhaseShiftHealingClampsBeforeDamageAndDoesNotBecomeInvulnerability()
        {
            var s=Create();var w=World(s);Grant(s,"R00I");var hero=w.UnitState(1);
            Call(s,"ApplyUniqueSoulPhaseShift",hero,100d,1);Assert.That(w.UnitState(1).health,Is.EqualTo(hero.health));
            w.ApplyUnitDamage(1,100);double injured=w.UnitState(1).health;
            Call(s,"ApplyUniqueSoulPhaseShift",w.UnitState(1),100d,1);w.ApplyUnitDamage(1,100);
            Assert.That(w.UnitState(1).health,Is.EqualTo(injured));
            Call(s,"ApplyUniqueSoulPhaseShift",w.UnitState(1),10d,1);Assert.That(w.UnitState(1).health,Is.EqualTo(injured));
            Call(s,"ApplyUniqueSoulPhaseShift",w.UnitState(1),100d,16);Assert.That(w.UnitState(1).health,Is.EqualTo(injured));
        }
    }
}

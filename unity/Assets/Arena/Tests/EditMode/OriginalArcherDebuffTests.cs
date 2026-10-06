using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalArcherDebuffTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/"+name+".json")));
        static object Call(OriginalSession session,string name,params object[] args) => typeof(OriginalSession).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(session,args);
        static (OriginalSession session,OriginalWorld world,object fixture) Create()
        {
            var fixture=typeof(OriginalSessionArcherTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{"A15X"});
            return ((OriginalSession)fixture.GetType().GetField("session",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(fixture),
                (OriginalWorld)fixture.GetType().GetField("world",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(fixture),fixture);
        }
        static void Advance(object fixture,double seconds) => fixture.GetType().GetMethod("Advance",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(fixture,new object[]{seconds});
        static void Enemy(OriginalWorld w,int id,double x=100,string rawcode="hfoo")
        {
            w.AddUnit(id,0,rawcode,new OriginalWorldUnitProfile{maxHealth=10000,moveSpeed=270,collisionRadius=1},new OriginalPoint(x,-1000));
            w.SetUnitState(id,paused:true);
        }

        [Test] public void SourceRanksAndMeasuredFrostConstantsRemainSeparateFromUnknownHazeMask()
        {
            var c=Load<OriginalCombatCatalog>("lia39-combat");var n=Load<OriginalNativeCatalog>("lia39-native126");
            for(int rank=1;rank<=3;rank++)
            {
                var acid=new OriginalArcherDebuffRules(c,n,"A166",rank);Assert.That(acid.armorReduction,Is.EqualTo(5*rank));
                var frost=new OriginalArcherDebuffRules(c,n,"A168",rank);Assert.That(frost.movementSlow,Is.EqualTo(.4));Assert.That(frost.attackSlow,Is.EqualTo(.25));
                Assert.That(frost.heroDuration,Is.EqualTo(1.5));Assert.That(frost.duration,Is.EqualTo(3));
                var haze=new OriginalArcherDebuffRules(c,n,"A165",rank);Assert.That(haze.attackSlow,Is.EqualTo(.2+.15*rank).Within(1e-12));
                Assert.That(haze.missChance,Is.EqualTo(haze.attackSlow));
            }
            var a=c.abilities.Single(v=>v.id=="A168");a.fields=a.fields.Concat(new[]{new OriginalCombatField{key="DataA1",number=50,isNumber=true}}).ToArray();c.BuildIndexes();
            Assert.Throws<InvalidOperationException>(()=>new OriginalArcherDebuffRules(c,n,"A168",1));
        }
        [Test] public void AcidOrderDoesNotPrecedeTheImmediateScriptedHitAndArmorReturnsOnDispel()
        {
            var f=Create();Enemy(f.world,1800);
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A166",2);
            Assert.That(Call(f.session,"ArcherDebuffArmorDelta",1800),Is.EqualTo(0d));
            Advance(f.fixture,.01);Assert.That(Call(f.session,"ArcherDebuffArmorDelta",1800),Is.EqualTo(-10d));
            Assert.That(f.world.UnitState(1800).health,Is.EqualTo(10000));
            Call(f.session,"RemoveArcherDebuffs",1800);Assert.That(Call(f.session,"ArcherDebuffArmorDelta",1800),Is.EqualTo(0d));
        }
        [Test] public void FrostAffectsOrganicEnemiesInItsNativeAreaAndRestoresOneUnmodifiedBaseline()
        {
            var f=Create();Enemy(f.world,1800,100);Enemy(f.world,1801,299);Enemy(f.world,1802,301);
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A168",1);
            Assert.That(f.world.UnitState(1800).profile.moveSpeed,Is.EqualTo(162).Within(1e-9));
            Assert.That(f.world.UnitState(1801).profile.moveSpeed,Is.EqualTo(162).Within(1e-9));
            Assert.That(f.world.UnitState(1802).profile.moveSpeed,Is.EqualTo(270));
            Call(f.session,"AddPyroChainBuff",f.world.UnitState(1800),f.world.Clock+30);
            Assert.That(f.world.UnitState(1800).profile.moveSpeed,Is.EqualTo(94.5).Within(1e-9));
            Call(f.session,"RemoveArcherNativeBuff",1800,"Bfro");
            Assert.That(f.world.UnitState(1800).profile.moveSpeed,Is.EqualTo(202.5).Within(1e-9));
            Call(f.session,"RemovePyroChainBuff",1800);
            Assert.That(f.world.UnitState(1800).profile.moveSpeed,Is.EqualTo(270));
        }
        [Test] public void PausedBuffTimeIsFrozenAndHazeKeepsItsOwnMissAndAttackSlow()
        {
            var f=Create();Enemy(f.world,1800);
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A165",3);Advance(f.fixture,.01);
            Assert.That(Call(f.session,"ArcherDebuffMissChance",1800),Is.EqualTo(.65));
            Assert.That(Call(f.session,"ArcherDebuffAttackSlow",1800),Is.EqualTo(.65));
            Advance(f.fixture,5);Assert.That(Call(f.session,"ArcherDebuffMissChance",1800),Is.EqualTo(.65));
            f.world.SetUnitState(1800,paused:false);f.world.HoldPosition(1800);Advance(f.fixture,4.01);
            Assert.That(Call(f.session,"ArcherDebuffMissChance",1800),Is.EqualTo(0d));
            Assert.That(f.session.HaltReason,Is.Null);
        }
        [Test] public void SourceOrbReactionRemovesOnlyFrostBeforeApplyingTheNextElement()
        {
            var f=Create();Enemy(f.world,1800);
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A168",1);
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A165",1);Advance(f.fixture,.01);
            // Source aie removes Bfro even on the last, zero-count orb watch.
            var state=Call(f.session,"Archer",1);state.GetType().GetField("active",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(state,OriginalBowElement.Fire);
            state.GetType().GetField("charges",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(state,0);
            Call(f.session,"OnArcherAttackStarted",f.world.UnitState(1),f.world.UnitState(1800));
            Call(f.session,"ObserveArcherDamage",1,1800);
            Assert.That(Call(f.session,"ArcherDebuffMovementBonus",1800),Is.EqualTo(0d));
            Assert.That(Call(f.session,"ArcherDebuffMissChance",1800),Is.EqualTo(.35));
        }
        [Test] public void NativeFrostHelperZeroEventsKeepTheOwnersArcherWatchUntilThatHeroHits()
        {
            var f=Create();Enemy(f.world,1800);
            var state=Call(f.session,"Archer",1);
            state.GetType().GetField("active",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(state,OriginalBowElement.Fire);
            state.GetType().GetField("charges",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(state,1);
            Call(f.session,"OnArcherAttackStarted",f.world.UnitState(1),f.world.UnitState(1800));
            Call(f.session,"OrderArcherNativeHelper",1,1800,"A168",1);
            Assert.That(state.GetType().GetField("charges",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(state),Is.EqualTo(1));
            Assert.That(f.world.UnitState(1800).health,Is.EqualTo(10000));
            Assert.That(Call(f.session,"ArcherDebuffMovementBonus",1800),Is.EqualTo(-.4));
            Call(f.session,"ApplyResolvedUnitHit",1,1,f.world.UnitState(1800),0d,null);
            Assert.That(state.GetType().GetField("charges",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(state),Is.EqualTo(0));
            Assert.That(Call(f.session,"ArcherDebuffMovementBonus",1800),Is.EqualTo(0d));
            Assert.That(f.world.UnitState(1800).health,Is.LessThan(10000));
            Assert.That(f.session.HaltReason,Is.Null);
        }
    }
}

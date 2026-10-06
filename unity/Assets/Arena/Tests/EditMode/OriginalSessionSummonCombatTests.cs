using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionSummonCombatTests
    {
        const int Summon=OriginalWorld.FirstSummonEntityId;
        static readonly BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var args=new object[]{null,false};
            var s=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            Assert.That(world.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=Summon,ownerSlot=1,sourceHeroEntityId=1,rawcode="n01V",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=100,moveSpeed=290,collisionRadius=24},health=500,mana=50,position=new OriginalPoint(930,1000)}}),Is.True);
            return s;
        }
        [Test] public void OwnedSummonUsesOwnRawWeaponArmorAndRegenerationWithoutHeroAttributes()
        {
            var s=Create(out var w);Assert.That((double)Call(s,"WeaponRate",w.UnitState(Summon)),Is.EqualTo(1));
            Call(s,"AdvanceRegeneration",1.0);
            Assert.That(w.UnitState(Summon).health,Is.EqualTo(500.5));Assert.That(w.UnitState(Summon).mana,Is.EqualTo(50.5));
            Call(s,"ApplyTriggeredHit",1,1,w.UnitState(Summon),40.0,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(Summon).health,Is.EqualTo(500.5-40/1.9).Within(1e-7));
        }
        [Test] public void SummonMovementDebuffRefreshAndExpiryRestoreOneCapturedRawBaseline()
        {
            var s=Create(out var w);
            Call(s,"AddPyroChainBuff",w.UnitState(Summon),2.0);
            Assert.That(w.UnitState(Summon).profile.moveSpeed,Is.EqualTo(217.5));
            Call(s,"AddPyroChainBuff",w.UnitState(Summon),2.0);
            Assert.That(w.UnitState(Summon).profile.moveSpeed,Is.EqualTo(217.5));
            Call(s,"RemovePyroChainBuff",Summon);
            Assert.That(w.UnitState(Summon).profile.moveSpeed,Is.EqualTo(290));
        }
        [Test] public void SummonReleasedMissileSurvivesDeadOwnerAndRemovedShooter()
        {
            var s=Create(out var w);w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=31},new OriginalPoint(1000,1000));
            w.ForceUnitDeath(1);Assert.That(w.TryAttackTarget(Summon,OriginalWorldTargetKind.Unit,1001),Is.True);
            for(int i=0;i<8;i++){w.Advance(.05);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(1001).health,Is.EqualTo(1000));
            Assert.That(w.RemoveUnit(Summon),Is.True);w.Advance(.05);Call(s,"AdvanceWeapons");
            Assert.That(w.UnitState(1001).health,Is.LessThan(1000));Assert.That(s.HaltReason,Is.Null);
        }
    }
}

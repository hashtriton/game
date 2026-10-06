using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionCrippleTests
    {
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(s,args);
        static void Step(OriginalSession s,double seconds)
        {
            while(seconds>1e-9){double d=Math.Min(.01,seconds);s.Advance(d);s.DrainEvents();seconds-=d;}
            Assert.That(s.HaltReason,Is.Null);
        }
        static OriginalSession Create(out OriginalWorld w)
        {
            var args=new object[]{null,false};
            var s=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);
            Step(s,2.05); Assert.That(w.Relocate(1,new OriginalPoint(0,1000)),Is.True);w.SetFacing(1,0);w.HoldPosition(1);
            var player=((IList)typeof(OriginalSession).GetField("players",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s))[0];
            var field=player.GetType().GetField("progression");var candidate=((OriginalHeroProgression)field.GetValue(player)).Copy();candidate.GrantExperience(10000);
            var stats=(OriginalHeroStatsSnapshot)Call(s,"CalculateProgressionStats","H008",candidate);
            Call(s,"ApplyProgressionProfile",player,stats,candidate);field.SetValue(player,candidate);player.GetType().GetField("stats").SetValue(player,stats);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=5,kind=OriginalSessionCommandKind.LearnSkill,skillId="A102"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return s;
        }
        static void Add(OriginalWorld w,int id,double x,string raw="hfoo")
        {
            w.AddUnit(id,0,raw,new OriginalWorldUnitProfile{maxHealth=10000,moveSpeed=270,collisionRadius=31},new OriginalPoint(x,1000));
            w.SetUnitState(id,paused:true);
        }
        [Test] public void PublicShieldCommitsAtNativePointThreeAndOrdersCrippleForEverySurvivingConeTarget()
        {
            var s=Create(out var w);Add(w,1001,100);Add(w,1002,180);Add(w,1003,-100);
            double mana=w.UnitState(1).mana;
            Assert.That(s.Snapshot().players[0].abilities.Single(a=>a.id=="A102").code,Is.EqualTo(OriginalAbilityUseCode.Ready));
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.CastSkill,skillId="A102"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Step(s,.29);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));Assert.That(w.UnitState(1001).health,Is.EqualTo(10000));
            Step(s,.01);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana-30).Within(1e-8));
            foreach(int id in new[]{1001,1002})
            {
                Assert.That(w.UnitState(id).health,Is.EqualTo(10000-110/1.12).Within(1e-8));
                Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",id,13.0,12.0),Is.EqualTo(18));
            }
            Assert.That(w.UnitState(1003).health,Is.EqualTo(10000));
            Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1003,13.0,12.0),Is.EqualTo(25));
            Assert.That(s.Snapshot().players[0].abilities.Single(a=>a.id=="A102").cooldownRemaining,Is.EqualTo(11).Within(1e-8));
        }
        [Test] public void PublicStopBeforeEffectSpendsNothingAndStartsNeitherDisplacementNorCripple()
        {
            var s=Create(out var w);Add(w,1001,100);double mana=w.UnitState(1).mana;
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.CastSkill,skillId="A102"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Step(s,.1);Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=7,kind=OriginalSessionCommandKind.Stop}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Step(s,.3);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));Assert.That(w.UnitState(1001).pathingDisabled,Is.False);
            Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1001,13.0,0.0),Is.EqualTo(13));
            Assert.That(s.Snapshot().players[0].abilities.Single(a=>a.id=="A102").cooldownRemaining,Is.Zero);
        }
        [Test] public void CripplePausesItsSixSecondDurationWithoutChangingRateOrSpeedAndDispelRemovesIt()
        {
            var s=Create(out var w);Add(w,1001,100);double rate=(double)Call(s,"WeaponRate",w.UnitState(1001));
            Call(s,"OrderShieldCripple",1,1001);Step(s,8);
            Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1001,13.0,0.0),Is.EqualTo(6));
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
            Assert.That((double)Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(rate));
            w.SetUnitState(1001,paused:false);w.HoldPosition(1001);w.SetUnitState(1,paused:true);
            Step(s,6);Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1001,13.0,0.0),Is.EqualTo(13));
            Call(s,"OrderShieldCripple",0,1);Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1,56.0,12.0),Is.EqualTo(40));
            Call(s,"RemoveDispellableAbilityBuffs",1);Assert.That((double)Call(s,"ApplyCrippleWeaponDamage",1,56.0,12.0),Is.EqualTo(68));
        }
        [Test] public void ActualWeaponUsesCurrentCrippleAtReleaseAfterUnchangedWindup()
        {
            double[] damage=new double[2],started=new double[2];double armorFactor=0;
            for(int pass=0;pass<2;pass++)
            {
                var s=Create(out var w);Add(w,1001,100);w.SetUnitState(1001,paused:false);w.SetUnitState(1,paused:true);
                armorFactor=1+.06*((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).armor.Require();
                Assert.That(w.TryAttackTarget(1001,OriginalWorldTargetKind.Unit,1),Is.True);
                double hp=w.UnitState(1).health;
                w.Advance(.01);Call(s,"AdvanceWeapons");started[pass]=w.UnitState(1001).attackSequence;
                if(pass==1)Call(s,"OrderShieldCripple",1,1001);
                for(int i=0;i<51;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
                damage[pass]=hp-w.UnitState(1).health;
            }
            Assert.That(started[0],Is.EqualTo(1));Assert.That(started[1],Is.EqualTo(1));Assert.That(damage[0],Is.GreaterThan(0));
            // This host seed rolls13. Reduction floors the white result to6
            // before the identical target's numeric armor is applied.
            Assert.That(damage[0]*armorFactor,Is.EqualTo(13).Within(1e-8));
            Assert.That(damage[1]*armorFactor,Is.EqualTo(6).Within(1e-8));
        }
    }
}

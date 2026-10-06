using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionPyroFieldTests
    {
        const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var fixture=typeof(OriginalSessionPyroTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{false});
            var s=(OriginalSession)fixture.GetType().GetField("session",Private).GetValue(fixture);
            world=(OriginalWorld)fixture.GetType().GetField("world",Private).GetValue(fixture);return s;
        }
        static void Enemy(OriginalWorld w,int id,double x)=>w.AddUnit(id,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=31},new OriginalPoint(x,1000));
        static void Cast(OriginalSession s,OriginalWorld w,string ability,int rank,double x)=>Call(s,"IssuePyroFlameStrike",1,1,ability,rank,new OriginalPoint(x,1000),w.Clock);
        static void Step(OriginalSession s,OriginalWorld w,double duration)
        {while(duration>1e-8){double step=Math.Min(.01,duration);w.Advance(step);Call(s,"AdvancePyroMeteors");duration-=step;}Assert.That(s.HaltReason,Is.Null);}
        [Test] public void NativeFieldKeepsGlobalPulseClockAndTrue210TargetWhile240IsOutside()
        {
            var s=Create(out var w);Enemy(w,1001,500);Enemy(w,1002,1300);Enemy(w,1003,710);Enemy(w,1004,260);
            Cast(s,w,"A0SQ",1,500);Step(s,w,1.4);w.SetPathingEnabled(1002,false);w.ForcePosition(1002,new OriginalPoint(500,1000));
            Assert.That(w.UnitState(1002).health,Is.EqualTo(10000));Step(s,w,1);w.ForcePosition(1002,new OriginalPoint(1300,1000));
            Step(s,w,1);w.ForcePosition(1002,new OriginalPoint(500,1000));Step(s,w,3);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9880));Assert.That(w.UnitState(1003).health,Is.EqualTo(9880));
            Assert.That(w.UnitState(1004).health,Is.EqualTo(10000));Assert.That(w.UnitState(1002).health,Is.EqualTo(9940));
        }
        [Test] public void EqualOverlappingFieldDoesNotRefreshOrAddPulsesButShiftedFieldIsIndependent()
        {
            var s=Create(out var w);Enemy(w,1001,500);Enemy(w,1002,1300);
            Cast(s,w,"A0SQ",1,500);Step(s,w,.82);Cast(s,w,"A0SQ",1,500);Cast(s,w,"A0SQ",1,1300);Step(s,w,7);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9880));Assert.That(w.UnitState(1002).health,Is.EqualTo(9880));
        }
        [Test] public void StrongerReplacementSkipsImmediateDamageAndOwnsEightRemainingPulses()
        {
            var s=Create(out var w);Enemy(w,1001,500);Cast(s,w,"A0SQ",1,500);Step(s,w,.82);Cast(s,w,"A0ST",3,500);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9980));Step(s,w,.99);Assert.That(w.UnitState(1001).health,Is.EqualTo(9980));
            Step(s,w,.01);Assert.That(w.UnitState(1001).health,Is.EqualTo(9880));Step(s,w,8);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9180));
        }
        [Test] public void FlameFieldUsesSharedMagicImmunityAndSurvivesOriginalHeroDeath()
        {
            var s=Create(out var w);Enemy(w,1001,500);Cast(s,w,"A0SQ",1,500);
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"ACmi"},Array.Empty<string>());w.ForceUnitDeath(1);Step(s,w,1);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9980));Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"ACmi"});
            Step(s,w,1);Assert.That(w.UnitState(1001).health,Is.EqualTo(9960));
        }
        [Test] public void PublicMeteorLearnCastImpactAndNativeTrailArePlayable()
        {
            var s=Create(out var w);Enemy(w,1001,700);
            var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];var field=player.GetType().GetField("progression");
            var candidate=((OriginalHeroProgression)field.GetValue(player)).Copy();candidate.GrantExperience(1200);
            var stats=(OriginalHeroStatsSnapshot)Call(s,"CalculateProgressionStats","H024",candidate);
            Call(s,"ApplyProgressionProfile",player,stats,candidate);field.SetValue(player,candidate);player.GetType().GetField("stats").SetValue(player,stats);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.LearnSkill,skillId="A0SM"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var view=s.Snapshot().players[0].abilities.Single(a=>a.id=="A0SM");Assert.That(view.code,Is.EqualTo(OriginalAbilityUseCode.Ready));
            double mana=w.UnitState(1).mana;
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=7,kind=OriginalSessionCommandKind.CastSkill,skillId="A0SM",x=700,y=1000}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            for(int i=0;i<9;i++)s.Advance(.01);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));
            s.Advance(.01);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana-view.manaCost));
            for(int i=0;i<134;i++)s.Advance(.01);
            Assert.That(s.HaltReason,Is.Null);Assert.That(w.UnitState(1001).health,Is.EqualTo(9880).Within(.0001));
            Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="A0SQ" && e.kind==OriginalVisualEffectKind.ActiveCircle),Is.True);
        }
    }
}

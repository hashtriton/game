using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionDarkGiftsTests
    {
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(s,args);
        static void Advance(OriginalSession s,double seconds)
        {
            while(seconds>1e-9){double step=Math.Min(.01,seconds);s.Advance(step);s.DrainEvents();seconds-=step;}
            Assert.That(s.HaltReason,Is.Null);
        }
        static OriginalSession Create(out OriginalWorld world)
        {
            var args=new object[]{null,false};
            var s=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s);
            Advance(s,2.05); world.HoldPosition(1);
            var player=((IList)typeof(OriginalSession).GetField("players",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s))[0];
            var field=player.GetType().GetField("progression");var candidate=((OriginalHeroProgression)field.GetValue(player)).Copy();candidate.GrantExperience(10000);
            var stats=(OriginalHeroStatsSnapshot)Call(s,"CalculateProgressionStats","H008",candidate);
            Call(s,"ApplyProgressionProfile",player,stats,candidate);field.SetValue(player,candidate);player.GetType().GetField("stats").SetValue(player,stats);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=5,kind=OriginalSessionCommandKind.LearnSkill,skillId="A0E6"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return s;
        }
        static double Armor(OriginalSession s,OriginalWorld w,int id)
        {
            var stats=Call(s,"CombatStatsFor",w.UnitState(id));
            return (double)stats.GetType().GetField("armor",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(stats);
        }
        [Test] public void NativeStompCommitsAtPointThreeThenSourceWaitsPointFiveBeforeApplyingGifts()
        {
            var s=Create(out var w);double mana=w.UnitState(1).mana,armor=Armor(s,w,1);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.CastSkill,skillId="A0E6"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,.29);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));
            Advance(s,.01);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana-100).Within(1e-8));
            Advance(s,.49);Assert.That(Armor(s,w,1),Is.EqualTo(armor));
            Advance(s,.01);Assert.That(Armor(s,w,1),Is.EqualTo(armor+40));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(262.5).Within(1e-8));
            Assert.That(s.Snapshot().players[0].abilities.Single(a=>a.id=="A0E6").cooldownRemaining,Is.EqualTo(109.5).Within(1e-8));
            Advance(s,15);Assert.That(Armor(s,w,1),Is.EqualTo(armor));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void StopBeforeEffectCancelsManaCooldownAndUpkeep()
        {
            var s=Create(out var w);double mana=w.UnitState(1).mana,armor=Armor(s,w,1);
            s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.CastSkill,skillId="A0E6"});Advance(s,.2);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=7,kind=OriginalSessionCommandKind.Stop}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,1);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));Assert.That(Armor(s,w,1),Is.EqualTo(armor));
            Assert.That(s.Snapshot().players[0].abilities.Single(a=>a.id=="A0E6").cooldownRemaining,Is.Zero);
        }
        [Test] public void EveryMeasuredRankMatchesArmorMovementAndIncomingFlagsWithoutChangingBaseline()
        {
            for(int rank=1;rank<=3;rank++)
            {
                var s=Create(out var w);double armor=Armor(s,w,1),rate=(double)Call(s,"WeaponRate",w.UnitState(1));
                Call(s,"BeginDarkGifts",1,rank);Advance(s,.5);
                Assert.That(Armor(s,w,1),Is.EqualTo(armor+40*rank));
                Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250*(1+.05*rank)).Within(1e-8));
                Assert.That((double)Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(rate+.2+.1*rank).Within(1e-8));
                foreach(var mode in new[]{OriginalTriggeredDamageMode.SpellNormal,OriginalTriggeredDamageMode.SpellMagic,OriginalTriggeredDamageMode.ChaosUniversal})
                {
                    var before=w.UnitState(1);Call(s,"ApplyTriggeredHit",0,0,before,40.0,mode);double damage=before.health-w.UnitState(1).health;
                    double expected=mode==OriginalTriggeredDamageMode.ChaosUniversal?40:32*.6;
                    if(mode==OriginalTriggeredDamageMode.SpellNormal)expected/=1+.06*(armor+40*rank);
                    Assert.That(damage,Is.EqualTo(expected).Within(1e-7));
                }
                Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).armor.Require(),Is.EqualTo(armor));
            }
        }
        [Test] public void DispelRemovesAcidButUpkeepReappliesAndAbilityResistanceIsIndependent()
        {
            var s=Create(out var w);double armor=Armor(s,w,1);Call(s,"BeginDarkGifts",1,1);Advance(s,.5);
            Call(s,"RemoveDispellableAbilityBuffs",1);Assert.That(Armor(s,w,1),Is.EqualTo(armor));
            Assert.That((double)Call(s,"KnightSpellResistance",1),Is.EqualTo(.6));
            Advance(s,.5);Assert.That(Armor(s,w,1),Is.EqualTo(armor+40));
        }
        [Test] public void NativeResistanceUsesAdditionOrderAndUpkeepDoesNotReorderKnightSource()
        {
            var s=Create(out var w);Call(s,"BeginDarkGifts",1,1);Advance(s,.5);
            Assert.That((double)Call(s,"IncomingMagicDamage",w.UnitState(1),40.0),Is.EqualTo(24));
            Call(s,"SetNativeSpellResistance",1,"test-item-AIsr",.2);
            Assert.That((double)Call(s,"IncomingMagicDamage",w.UnitState(1),40.0),Is.EqualTo(32));
            Advance(s,1);
            Assert.That((double)Call(s,"IncomingMagicDamage",w.UnitState(1),40.0),Is.EqualTo(32));
            Call(s,"RemoveNativeSpellResistance",1,"test-item-AIsr");
            Assert.That((double)Call(s,"IncomingMagicDamage",w.UnitState(1),40.0),Is.EqualTo(24));
            Advance(s,14);
            Assert.That((double)Call(s,"IncomingMagicDamage",w.UnitState(1),40.0),Is.EqualTo(40));
        }
        [Test] public void NativeBackswingDelaysAutomaticAttackButAcceptedAttackOrderCancelsOnlyRecovery()
        {
            foreach(bool explicitOrder in new[]{false,true})
            {
                var s=Create(out var w);var point=w.UnitState(1).position;
                w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=31},new OriginalPoint(point.x+100,point.y));
                w.SetUnitState(1001,paused:true);
                s.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.CastSkill,skillId="A0E6"});
                Advance(s,.4);Call(s,"AdvanceWorldAi");Assert.That(w.UnitState(1).attackSequence,Is.Zero);
                if(explicitOrder)
                    Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=7,kind=OriginalSessionCommandKind.AttackTarget,targetKind=OriginalWorldTargetKind.Unit,targetId=1001}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Advance(s,.4);Assert.That(w.UnitState(1).attackSequence,explicitOrder?Is.EqualTo(1):Is.Zero);
                Advance(s,.02);Assert.That((bool)Call(s,"AutomaticAttackRecovery",1),Is.False);
                Advance(s,.2);Call(s,"AdvanceWorldAi");Advance(s,.01);Assert.That(w.UnitState(1).attackSequence,Is.GreaterThanOrEqualTo(1));
                Assert.That((double)Call(s,"KnightArmorBonus",1),Is.EqualTo(40));
            }
        }
    }
}

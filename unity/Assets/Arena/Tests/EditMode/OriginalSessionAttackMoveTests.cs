using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionAttackMoveTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static OriginalSession Create(out OriginalWorld world)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            world.AddUnit(1900,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=100,collisionRadius=24},new OriginalPoint(335,1000));
            return s;
        }
        static OriginalSessionReplyCode Send(OriginalSession s,OriginalSessionCommandKind kind,double x=1000)=>s.Apply(0,new OriginalSessionCommand {
            kind=kind,sequence=s.Snapshot().players[0].acknowledgedSequence+1,x=x,y=1000});
        static void Ai(OriginalSession s,OriginalWorld w)
        { for(int i=0;i<5;i++)w.Advance(.05);typeof(OriginalSession).GetMethod("AdvanceWorldAi",Private).Invoke(s,null); }
        [Test] public void MoveIgnoresAnEnemyButAttackMoveAcquiresIt()
        {
            var s=Create(out var w);
            Assert.That(Send(s,OriginalSessionCommandKind.Move),Is.EqualTo(OriginalSessionReplyCode.Accepted));Ai(s,w);
            Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(Send(s,OriginalSessionCommandKind.AttackMove),Is.EqualTo(OriginalSessionReplyCode.Accepted));Ai(s,w);
            Assert.That(w.UnitState(1).targetId,Is.EqualTo(1900));
        }
        [Test] public void AttackMoveResumesAfterTargetRemovalAndStopCancelsTheDestination()
        {
            var s=Create(out var w);Send(s,OriginalSessionCommandKind.AttackMove);Ai(s,w);
            Assert.That(w.UnitState(1).targetId,Is.EqualTo(1900));w.RemoveUnit(1900);
            typeof(OriginalSession).GetMethod("AdvanceWeapons",Private).Invoke(s,null);Ai(s,w);
            Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(Send(s,OriginalSessionCommandKind.Stop),Is.EqualTo(OriginalSessionReplyCode.Accepted));var point=w.UnitState(1).position;
            Ai(s,w);Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(point.x));
        }
        [Test] public void InvalidAttackMoveCannotReplaceAcceptedDestination()
        {
            var s=Create(out var w);Send(s,OriginalSessionCommandKind.AttackMove);
            Assert.That(Send(s,OriginalSessionCommandKind.AttackMove,double.NaN),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Ai(s,w);Assert.That(w.UnitState(1).targetId,Is.EqualTo(1900));
            var codec=new OriginalUnitySessionCodec();
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(new OriginalSessionCommand{sequence=1,kind=OriginalSessionCommandKind.AttackMove,x=double.MaxValue}),out _),Is.False);
        }
        [Test] public void AcceptedAbilityReplacesAttackMoveWhileRejectedAbilityPreservesIt()
        {
            var s=Create(out var w);w.RemoveUnit(1900);
            Send(s,OriginalSessionCommandKind.AttackMove);
            OriginalSessionReplyCode Skill(OriginalSessionCommandKind kind)=>s.Apply(0,new OriginalSessionCommand {
                kind=kind,sequence=s.Snapshot().players[0].acknowledgedSequence+1,skillId="A05N"});
            Assert.That(Skill(OriginalSessionCommandKind.CastSkill),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));
            Assert.That(Skill(OriginalSessionCommandKind.LearnSkill),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Skill(OriginalSessionCommandKind.CastSkill),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            for(int i=0;i<30;i++)
            {
                w.Advance(.05);
                typeof(OriginalSession).GetMethod("AdvanceMirrors",Private).Invoke(s,null);
            }
            Ai(s,w);
            Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.None));
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionSummonAbilityTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string method,params object[] args)
        {var m=typeof(OriginalSession).GetMethod(method,Private);var p=m.GetParameters();return m.Invoke(s,p.Select((parameter,i)=>i<args.Length?args[i]:parameter.DefaultValue).ToArray());}
        static OriginalSession Create(out OriginalWorld world)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);return s;
        }
        static int Add(OriginalWorld w,string raw,int offset=0,double x=335,double hp=1000,double mana=1000)
        {
            int id=OriginalWorld.FirstSummonEntityId+offset;
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=id,ownerSlot=1,sourceHeroEntityId=1,rawcode=raw,
                profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=1000,moveSpeed=250,collisionRadius=16},
                health=hp,mana=mana,position=new OriginalPoint(x,1000)}}),Is.True);return id;
        }
        static void Enemy(OriginalWorld w,int id=9001,double x=600)
        {w.AddUnit(id,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=2000,maxMana=500,moveSpeed=250,collisionRadius=16},new OriginalPoint(x,1000));}
        static OriginalSessionReplyCode Cast(OriginalSession s,int actor,string ability,int target=0,double x=0,double y=0)
        {
            var command=new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,actorEntityId=actor,skillId=ability,
                sequence=s.Snapshot().players[0].acknowledgedSequence+1,targetKind=target==0?OriginalWorldTargetKind.None:OriginalWorldTargetKind.Unit,targetId=target,x=x,y=y};
            var codec=new OriginalUnitySessionCodec();Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out var decoded),Is.True);
            return s.Apply(0,decoded);
        }
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double step=Math.Min(.05,seconds);w.Advance(step);Call(s,"AdvanceActorControls",step);Call(s,"AdvanceSummonAbilities");Call(s,"AdvanceItemScriptActs");seconds-=step;}}
        static bool Wire(OriginalSession s,Action<OriginalSessionView> mutate=null)
        {
            var snapshot=s.Snapshot();mutate?.Invoke(snapshot);var codec=new OriginalUnitySessionCodec();
            return codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,
                assignedSlot=1,snapshot=snapshot,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _);
        }
        [Test] public void SummonUnitSpellUsesSelectedActorManaAndRoundTripsDetachedAbilityViews()
        {
            var s=Create(out var w);int id=Add(w,"n02K");Enemy(w);
            Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(w.UnitState(id).mana,Is.EqualTo(1000));
            Tick(s,w,.5);Assert.That(w.UnitState(id).mana,Is.EqualTo(825));Assert.That(w.UnitState(9001).profile.moveSpeed,Is.EqualTo(100));
            Assert.That(Wire(s),Is.True);Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Wire(s,v=>v.unitAbilities[0].entityId=1),Is.False);
            Assert.That(Wire(s,v=>v.unitAbilities[0].abilities[0].rank=2),Is.False);
            Assert.That(Wire(s,v=>v.unitAbilities[0].abilities[0].manaCost=-1),Is.False);
            var copy=s.Snapshot();copy.unitAbilities[0].abilities[0].id="ZZZZ";
            Assert.That(s.Snapshot().unitAbilities[0].abilities[0].id,Is.Not.EqualTo("ZZZZ"));
        }
        [Test] public void SummonWrongActorTargetManaAndAcceptedOrderInterruptDoNotDebit()
        {
            var s=Create(out var w);int id=Add(w,"n02K",mana:0);Enemy(w);
            Assert.That(Cast(s,9001,"A08U",1),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            var unit=w.UnitState(id);w.UpdateProfile(id,unit.profile,unit.health,1000);
            Assert.That(Cast(s,id,"A08U",1),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(s,"OnAcceptedWorldOrder",id);Tick(s,w,.6);
            Assert.That(w.UnitState(id).mana,Is.EqualTo(1000));Assert.That(w.UnitState(9001).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(s,"AddNativeSilence",id,9001,1d);Tick(s,w,.6);Assert.That(w.UnitState(id).mana,Is.EqualTo(1000));
        }
        [Test] public void StompCanBeCastWithoutAUnitTargetOrAnyNearbyEnemy()
        {
            var s=Create(out var w);int id=Add(w,"n02K");
            Assert.That(Cast(s,id,"A08T"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.5);
            Assert.That(w.UnitState(id).mana,Is.EqualTo(900));Assert.That(Wire(s),Is.True);
        }
        [Test] public void ImmolationToggleOffIsAvailableWithoutManaAndWaveSpellsUseTheirOwnCooldownRegistry()
        {
            var s=Create(out var w);int id=Add(w,"n015");
            Assert.That(Cast(s,id,"A05Y"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().unitAbilities[0].abilities.Single(a=>a.id=="A05Y").toggledOn,Is.True);
            var actor=w.UnitState(id);w.UpdateProfile(id,actor.profile,actor.health,0);
            Assert.That(Cast(s,id,"A05Y"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().unitAbilities[0].abilities.Single(a=>a.id=="A05Y").toggledOn,Is.False);
            w.UpdateProfile(id,actor.profile,actor.health,1000);Call(s,"ApplyUnitAbilityOverlay",id,new[]{"A0TR"},null);
            Assert.That(Cast(s,id,"A0TR"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(id).mana,Is.EqualTo(850));
            Assert.That(Cast(s,id,"A0TR"),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(s.Snapshot().unitAbilities[0].abilities.Single(a=>a.id=="A0TR").cooldownRemaining,Is.GreaterThan(0));
            Assert.That(Wire(s),Is.True);
        }
        [Test] public void HealerRunsTargetMaxHealthSourceAreaBeforeNativeFiftyHeal()
        {
            var s=Create(out var w);int id=Add(w,"n01V");int recipient=Add(w,"hfoo",1,500,100);int structure=Add(w,"hhou",2,600,100);
            Assert.That(Cast(s,id,"A0FD",recipient),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.5);
            Assert.That(w.UnitState(recipient).health,Is.EqualTo(200));Assert.That(w.UnitState(structure).health,Is.EqualTo(150));
            Assert.That(w.UnitState(id).mana,Is.EqualTo(995));Assert.That(Wire(s),Is.True);
        }
        [Test] public void BloodlustComposesMovementAndSleepUsesItsOwnDurationAndCasterIdentity()
        {
            var s=Create(out var w);int id=Add(w,"n01R");Enemy(w);
            double baseRate=(double)Call(s,"WeaponRate",w.UnitState(id));
            Assert.That(Cast(s,id,"A030",id),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.5);
            Assert.That(w.UnitState(id).profile.moveSpeed,Is.EqualTo(275));Assert.That(Call(s,"SummonAbilityAttackSpeedBonus",id),Is.EqualTo(.5));
            Assert.That(Call(s,"WeaponRate",w.UnitState(id)),Is.EqualTo(baseRate+.5).Within(.000001));
            Assert.That(Cast(s,id,"A0A2",9001),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.5);
            Assert.That(Call(s,"HasNativeSleep",9001),Is.True);double hp=w.UnitState(9001).health;
            Call(s,"ApplyResolvedUnitHit",id,1,w.UnitState(9001),40d);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(hp));Tick(s,w,2.01);
            Call(s,"ApplyResolvedUnitHit",id,1,w.UnitState(9001),40d);
            Assert.That(Call(s,"HasNativeSleep",9001),Is.False);Assert.That(w.UnitState(9001).health,Is.EqualTo(hp-40));
            Tick(s,w,20);
            Assert.That(w.UnitState(id).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(Call(s,"WeaponRate",w.UnitState(id)),Is.EqualTo(baseRate).Within(.000001));
        }
        [Test] public void RevealExposesAnInvisibleEnemyForEightSecondsThroughTheSharedVisibilityPredicate()
        {
            var s=Create(out var w);int id=Add(w,"n01R");
            Call(s,"ApplyUnitAbilityOverlay",id,new[]{"A0WH"},null);
            w.AddUnit(9001,0,"n00M",new OriginalWorldUnitProfile{maxHealth=2000,maxMana=500,moveSpeed=250,collisionRadius=16},new OriginalPoint(700,1000));
            Call(s,"AdvanceNativeInvisibility");Tick(s,w,2.1);Call(s,"AdvanceNativeInvisibility");
            Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.True);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.False);
            Assert.That(Cast(s,id,"A0WH"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.5);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.True);
            Tick(s,w,8.01);Assert.That(Call(s,"CombatInvisibilityActive",9001),Is.True);
            Assert.That(Call(s,"CanSeeForCombat",1,w.UnitState(9001)),Is.False);
        }
        [Test] public void AnotherPlayersSummonCannotBeOrderedAndTargetRemovalCancelsWithoutDebit()
        {
            var s=Create(out var w);int id=Add(w,"n02K");Enemy(w);
            var actor=w.UnitState(id);
            // A real second canonical lineage creates a valid owned summon;
            // the session sender remains player1 throughout this regression.
            w.AddUnit(2,2,"H008",actor.profile,new OriginalPoint(900,1000));
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=id+1,ownerSlot=2,sourceHeroEntityId=2,rawcode="n02K",
                profile=actor.profile,health=1000,mana=1000,position=new OriginalPoint(1000,1000)}}),Is.True);
            Assert.That(Cast(s,id+1,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(w.UnitState(id+1).mana,Is.EqualTo(1000));
            Assert.That(Cast(s,id,"A08U",9001),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            w.ForceUnitDeath(9001);Tick(s,w,.6);Assert.That(w.UnitState(id).mana,Is.EqualTo(1000));
        }
        [Test] public void SummonNetTravelsThenRootsWithoutBlockingCastAndSurvivesCasterDeath()
        {
            var s=Create(out var w);int id=Add(w,"n0AC");Enemy(w,9001,535);
            Assert.That(Cast(s,id,"A18J",x:535,y:1000),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.55);
            Assert.That(w.UnitState(id).mana,Is.EqualTo(950));w.ForceUnitDeath(id);Tick(s,w,.5);
            Assert.That(Call(s,"IsNativeRooted",9001),Is.True);Assert.That(Call(s,"ActorCastBlocked",9001),Is.False);
            Tick(s,w,3.1);Assert.That(Call(s,"IsNativeRooted",9001),Is.False);
        }
        [Test] public void ShadowShieldKeepsLiteralReservoirAndHealsHalfRecordedDamageAfterEightSeconds()
        {
            var s=Create(out var w);int id=Add(w,"n0AD");int recipient=Add(w,"hfoo",1,500,500);int ally=Add(w,"hfoo",2,600,100);
            Assert.That(Cast(s,id,"A18I",recipient),Is.EqualTo(OriginalSessionReplyCode.Accepted));Tick(s,w,.75);
            Call(s,"ApplyResolvedUnitHit",0,0,w.UnitState(recipient),40d);Assert.That(w.UnitState(recipient).health,Is.EqualTo(460));
            Tick(s,w,.05);Assert.That(w.UnitState(recipient).health,Is.EqualTo(500));w.ForceUnitDeath(id);
            Tick(s,w,7.95);Assert.That(w.UnitState(ally).health,Is.EqualTo(120));Assert.That(w.UnitState(recipient).health,Is.EqualTo(520));
        }
    }
}

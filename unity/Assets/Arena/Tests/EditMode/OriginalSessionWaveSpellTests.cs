using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionWaveSpellTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)
        {
            var method=typeof(OriginalSession).GetMethod(name,Hidden);
            Assert.That(method,Is.Not.Null,"Missing wave spell runtime: "+name);return method.Invoke(s,args);
        }
        static OriginalSession Create(out OriginalWorld w,string raw="n02A",double x=300)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var hero=w.UnitState(1);w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=10000,maxMana=hero.profile.maxMana,
                moveSpeed=0,collisionRadius=hero.profile.collisionRadius},10000,hero.mana);
            w.AddUnit(9001,0,raw,new OriginalWorldUnitProfile{maxHealth=10000,maxMana=5000,collisionRadius=16},new OriginalPoint(x,1000));
            return s;
        }
        static void Begin(OriginalSession s,string id)=>Assert.That(Call(s,"BeginWaveSpellEffect",9001,id),Is.True);
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double step=Math.Min(.01,seconds);w.Advance(step);Call(s,"AdvanceWaveSpells");seconds-=step;}}
        [Test] public void LeapLandsOnThe106thCallbackAndKeepsItsSourceDamageAfterCasterDeath()
        {
            var s=Create(out var w);Begin(s,"A15T");Assert.That(w.UnitState(9001).pathingDisabled,Is.True);
            w.ForceUnitDeath(9001);Tick(s,w,2.10);Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            Tick(s,w,.02);Assert.That(w.UnitState(1).health,Is.EqualTo(9400).Within(.0001));
            Assert.That(w.UnitState(9001).pathingDisabled,Is.False);
        }
        [Test] public void PullFieldMovesEightPerCallbackAndSurvivesItsCaster()
        {
            var s=Create(out var w,"n06D",635);Begin(s,"A0TR");w.ForceUnitDeath(9001);
            Tick(s,w,.03);Assert.That(w.UnitState(1).position.x,Is.EqualTo(143).Within(.0001));
            Tick(s,w,3.9);Assert.That(w.UnitState(1).position.x,Is.EqualTo(623).Within(.0001));
            w.ForcePosition(1,new OriginalPoint(135,1000));Tick(s,w,.3);
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(135));
        }
        [Test] public void EscapeChargeChoosesTheAuthoredFirstArenaRegionAndRestoresPathing()
        {
            var s=Create(out var w,"n067");Begin(s,"A11L");Tick(s,w,5);
            var actor=w.UnitState(9001);double dx=actor.position.x+1024,dy=actor.position.y;
            Assert.That(Math.Sqrt(dx*dx+dy*dy),Is.LessThan(20.01));Assert.That(actor.pathingDisabled,Is.False);
            Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
        }
        [Test] public void HeavyChargeWaitsNineWindupCallbacksThenHitsEachVictimOnlyOnce()
        {
            var s=Create(out var w,"n0AR");Begin(s,"A1DB");Assert.That(w.UnitState(9001).paused,Is.True);
            Tick(s,w,1.8);Assert.That(w.UnitState(9001).position.x,Is.EqualTo(300));
            Tick(s,w,.96);var actor=w.UnitState(9001);
            Assert.That(actor.position.x,Is.EqualTo(-490.5).Within(.002));Assert.That(actor.paused||actor.pathingDisabled,Is.False);
            Assert.That(w.UnitState(1).health,Is.EqualTo(9680).Within(.0001));
        }
        [Test] public void AnchorTetherUsesInitialHeroPointAndBreaksOnlyBeyond900()
        {
            var s=Create(out var w,"n0AT");Begin(s,"A1DD");w.ForcePosition(1,new OriginalPoint(1035,1000));
            Tick(s,w,.1);Assert.That(w.UnitState(1).health,Is.EqualTo(10000));
            w.ForcePosition(1,new OriginalPoint(1036,1000));Tick(s,w,.1);
            Assert.That(w.UnitState(1).health,Is.EqualTo(9300));Tick(s,w,.02);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True,"Authored A0O1 stun3s is retained.");
        }
        [Test] public void ExactTetherBoltEmitsTwoZeroDamageCallbacksWithoutInventingDamage()
        {
            var s=Create(out var w);var reference=Create(out _);
            typeof(OriginalSession).GetField("warpathTriggersEnabled",Hidden).SetValue(s,true);
            var random=typeof(OriginalSession).GetField("weaponRandom",Hidden);random.SetValue(s,123u);random.SetValue(reference,123u);
            double hp=w.UnitState(1).health;Call(s,"QueueWaveSpellStun",1);
            Tick(s,w,.004);Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
            Tick(s,w,.002);for(int i=0;i<2;i++)Call(reference,"RollWeapon",20);
            Assert.That(random.GetValue(s),Is.EqualTo(random.GetValue(reference)),"Both native zero events reach the central damage watch.");
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Assert.That(Call(s,"ActorCastBlocked",1),Is.True);
            Tick(s,w,1.1);Assert.That(Call(s,"ActorCastBlocked",1),Is.True,"Helper timed-life death does not remove BPSE.");
            Call(s,"AdvanceActorControls",2.99);Assert.That(Call(s,"ActorCastBlocked",1),Is.True);
            Call(s,"AdvanceActorControls",.02);Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
            Assert.That(random.GetValue(s),Is.EqualTo(random.GetValue(reference)),"The measured window has no extra expiry damage event.");
        }
        [Test] public void LoneHeroResonancePullsTheCasterAndDamagesBothOnContact()
        {
            var s=Create(out var w,"n0AV");Begin(s,"A1DI");Tick(s,w,.1);
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(143).Within(.0001));
            Assert.That(w.UnitState(9001).position.x,Is.EqualTo(292).Within(.0001));
            Tick(s,w,.1);Assert.That(w.UnitState(1).health,Is.EqualTo(9100));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9100),"Source h2=lZ fallback damages the caster too.");
        }
        [Test] public void MissingLivingPartyMakesBindingsFinishWithoutAnEndlessSelectionLoop()
        {
            var s=Create(out var w,"n0AT");w.ForceUnitDeath(1);
            Assert.That(Call(s,"BeginWaveSpellEffect",9001,"A1DD"),Is.False);
            Assert.That(Call(s,"BeginWaveSpellEffect",9001,"A1DI"),Is.False);
            Assert.That(Call(s,"BeginWaveSpellEffect",9001,"A1DB"),Is.False);
            Tick(s,w,10);Assert.That(w.UnitState(9001).paused||w.UnitState(9001).pathingDisabled,Is.False);
        }
        [Test] public void InstantBerserkFamilyDebitsOnceKeepsAttackAndRejectsCooldown()
        {
            var s=Create(out var w);w.TryAttackTarget(9001,OriginalWorldTargetKind.Unit,1);
            Assert.That(Call(s,"TryStartWaveSpell",9001,"A15T"),Is.True);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4800));
            Assert.That(w.UnitState(9001).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(Call(s,"TryStartWaveSpell",9001,"A15T"),Is.False);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4800));
        }
        [Test] public void RoarCastCanBeInterruptedBeforeEffectAndRemovedAbilityIsRechecked()
        {
            var s=Create(out var w,"n0AR");Assert.That(Call(s,"TryStartWaveSpell",9001,"A1DB"),Is.True);
            Tick(s,w,.7);Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));
            w.Stop(9001);Call(s,"OnAcceptedWorldOrder",9001);Tick(s,w,.1);
            Assert.That(w.UnitState(9001).paused,Is.False);Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));
            Assert.That(Call(s,"TryStartWaveSpell",9001,"A1DB"),Is.True);
            Call(s,"ApplyUnitAbilityOverlay",9001,Array.Empty<string>(),new[]{"A1DB"});Tick(s,w,.75);
            Assert.That(w.UnitState(9001).paused,Is.False);Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));
        }
        [Test] public void CastControlPreventsNativeAiAndDoesNotCancelAnAlreadyLaunchedField()
        {
            var s=Create(out var w,"n06D");Call(s,"AddTimedNativeStun",9001,"BPSE",1,1d);
            Call(s,"SelectWaveSpellOrders");Assert.That(w.UnitState(9001).mana,Is.EqualTo(5000));
            Call(s,"ClearNegativeActorControls",9001);Call(s,"SelectWaveSpellOrders");
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4850));
            Call(s,"AddTimedNativeStun",9001,"BPSE",1,1d);Tick(s,w,.03);
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(143).Within(.0001));
        }
        [Test] public void CompletedRoarDebitsAtEffectAndPublishesDetachedTelegraph()
        {
            var s=Create(out var w,"n0AR");Call(s,"TryStartWaveSpell",9001,"A1DB");Tick(s,w,.75);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(4950));Assert.That(w.UnitState(9001).paused,Is.True);
            var rows=s.Snapshot().effects;Assert.That(Array.Exists(rows,e=>e.abilityId=="A1DB"),Is.True);
            rows[0].radius=1234;Assert.That(s.Snapshot().effects[0].radius,Is.Not.EqualTo(1234));
        }
    }
}

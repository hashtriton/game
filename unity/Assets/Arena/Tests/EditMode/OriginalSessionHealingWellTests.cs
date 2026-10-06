using System.Reflection;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using static Arena.Tests.OriginalWorldOptionTestSupport;

namespace Arena.Tests
{
    public sealed class OriginalSessionHealingWellTests
    {
        static object View(OriginalSession s)=>Call(s,"SnapshotHealingWellView");
        static T Field<T>(object value,string name)=>(T)value.GetType().GetField(name).GetValue(value);
        static void Spawn(OriginalSession s)
        {
            Call(s,"OnHealingWellMatchEvent",new OriginalMatchEvent{sequence=100,time=0,kind=OriginalMatchEventKind.ShopAccess,round=2,enabled=true});
            AdvanceWorldOnly(s,1.05,"AdvanceHealingWell");
        }
        [Test] public void WellSpawnsAfterFirstCompletedRoundWithSourceDelayAndNativeMaximum()
        {
            var s=Create();Assert.That(Field<bool>(View(s),"present"),Is.False);
            Call(s,"OnHealingWellMatchEvent",new OriginalMatchEvent{sequence=100,time=0,kind=OriginalMatchEventKind.ShopAccess,round=2,enabled=true});
            AdvanceWorldOnly(s,.95,"AdvanceHealingWell");Assert.That(Field<bool>(View(s),"present"),Is.False);
            AdvanceWorldOnly(s,.1,"AdvanceHealingWell");var view=View(s);
            Assert.That(Field<bool>(view,"present"),Is.True);Assert.That(Field<double>(view,"mana"),Is.EqualTo(2000));
            Assert.That(Field<OriginalPoint>(view,"position").x,Is.EqualTo(-60));Assert.That(Field<OriginalPoint>(view,"position").y,Is.EqualTo(-380));
        }
        [Test] public void NativeBothResourceDeficitsSplitLimitedWellManaAndRejectFarActor()
        {
            var s=Create();Spawn(s);var w=World(s);var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,actor.profile.maxHealth-200,actor.profile.maxMana-60);
            typeof(OriginalSession).GetField("healingWellMana",Hidden).SetValue(s,20d);
            Assert.That(Call(s,"UseHealingWell",Player(s,1),1),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Field<double>(View(s),"mana"),Is.EqualTo(20));
            Assert.That(w.Relocate(1,new OriginalPoint(-60,-380)),Is.True);actor=w.UnitState(1);
            double hp=actor.health,mp=actor.mana;
            Assert.That(Call(s,"UseHealingWell",Player(s,1),1),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).health-hp,Is.EqualTo(10));Assert.That(w.UnitState(1).mana-mp,Is.EqualTo(10));
            Assert.That(Field<double>(View(s),"mana"),Is.EqualTo(0));
            Assert.That(Call(s,"UseHealingWell",Player(s,1),1),Is.EqualTo(OriginalSessionReplyCode.NotReady));
        }
        [Test] public void WellRedirectsUnusedHalfAndPreservesExcessManaWhenBothAxesFill()
        {
            foreach(var deficits in new[]{new[]{2d,60d,2d,18d,0d},new[]{200d,2d,18d,2d,0d},new[]{2d,2d,2d,2d,16d}})
            {
                var s=Create();Spawn(s);var w=World(s);w.Relocate(1,new OriginalPoint(-60,-380));var actor=w.UnitState(1);
                w.UpdateProfile(1,actor.profile,actor.profile.maxHealth-deficits[0],actor.profile.maxMana-deficits[1]);
                typeof(OriginalSession).GetField("healingWellMana",Hidden).SetValue(s,20d);actor=w.UnitState(1);
                Assert.That(Call(s,"UseHealingWell",Player(s,1),1),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(w.UnitState(1).health-actor.health,Is.EqualTo(deficits[2]));Assert.That(w.UnitState(1).mana-actor.mana,Is.EqualTo(deficits[3]));
                Assert.That(Field<double>(View(s),"mana"),Is.EqualTo(deficits[4]));
            }
        }
        [Test] public void OriginalWellDoesNotInheritStockNightRegenerationAndRejectsInvalidWireMana()
        {
            var s=Create();Spawn(s);typeof(OriginalSession).GetField("healingWellMana",Hidden).SetValue(s,100d);
            AdvanceWorldOnly(s,480,"AdvanceHealingWell");Assert.That(Field<double>(View(s),"mana"),Is.EqualTo(100));
            var codec=new OriginalUnitySessionCodec();var reply=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,code=OriginalSessionReplyCode.Accepted,assignedSlot=1,acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence,snapshot=s.Snapshot()};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out _),Is.True);
            reply.snapshot.well.mana=2001;Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out _),Is.False);
            reply.snapshot.well.mana=100;reply.snapshot.well.present=false;Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out _),Is.False);
        }
        [Test] public void EasyWellRefillsOnlyAfterBothDuelsAndNeverAfterCompletedRoundThirty()
        {
            var s=Create(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Easy),2);Spawn(s);
            typeof(OriginalSession).GetField("healingWellMana",Hidden).SetValue(s,100d);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Hidden).GetValue(s);
            match.DrainEvents();typeof(OriginalMatch).GetProperty("Round").SetValue(match,4);
            typeof(OriginalMatch).GetProperty("Phase").SetValue(match,OriginalMatchPhase.Combat);
            void Consume()
            {
                foreach(var item in match.DrainEvents().Where(e=>e.kind==OriginalMatchEventKind.ShopAccess&&e.enabled))
                {item.sequence+=100;Call(s,"OnHealingWellMatchEvent",item);}
                AdvanceWorldOnly(s,1.05,"AdvanceHealingWell");
            }
            typeof(OriginalMatch).GetMethod("PrepareNextRound",Hidden).Invoke(match,null);Consume();
            Assert.That(s.Snapshot().well.mana,Is.EqualTo(100),"The pre-pair shop opening is not OU");
            match.Advance(25);match.CompleteDuelSequence();Consume();
            Assert.That(s.Snapshot().well.mana,Is.EqualTo(100),"The pre-gladiator shop opening is not OU");
            match.Advance(25);match.CompleteDuelSequence();Consume();Assert.That(s.Snapshot().well.mana,Is.EqualTo(2000));
            typeof(OriginalSession).GetField("healingWellMana",Hidden).SetValue(s,100d);
            Call(s,"OnHealingWellMatchEvent",new OriginalMatchEvent{sequence=1000,time=match.Clock,kind=OriginalMatchEventKind.ShopAccess,round=31,enabled=true});
            AdvanceWorldOnly(s,1.05,"AdvanceHealingWell");Assert.That(s.Snapshot().well.mana,Is.EqualTo(100));
        }
        [Test] public void PublicUseWellMatchesAllThreeObservedHeroBoundariesAndOwnership()
        {
            var s=Create(count:3);Spawn(s);var w=World(s);
            for(int slot=1;slot<=3;slot++)
            {
                long connection=slot==1?0:100+slot-1; // Create uses101/102 for lobby slots2/3.
                var actor=w.UnitState(slot);w.UpdateProfile(slot,actor.profile,actor.profile.maxHealth-20,actor.profile.maxMana-20);
                Assert.That(w.Relocate(slot,new OriginalPoint(365,-380)),Is.True);
                Assert.That(s.Apply(connection,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseWell,sequence=4,actorEntityId=slot}),Is.EqualTo(OriginalSessionReplyCode.Accepted),actor.rawcode+"425");
                actor=w.UnitState(slot);w.UpdateProfile(slot,actor.profile,actor.profile.maxHealth-20,actor.profile.maxMana-20);
                Assert.That(w.Relocate(slot,new OriginalPoint(366,-380)),Is.True);double before=s.Snapshot().well.mana;
                Assert.That(s.Apply(connection,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseWell,sequence=5,actorEntityId=slot}),Is.EqualTo(OriginalSessionReplyCode.NotReady),actor.rawcode+"426");
                Assert.That(s.Snapshot().well.mana,Is.EqualTo(before));
                Assert.That(w.Relocate(slot,new OriginalPoint(-60,-1380-slot*100)),Is.True);
            }
            Assert.That(w.Relocate(2,new OriginalPoint(-60,-380)),Is.True);double mana=s.Snapshot().well.mana;
            Assert.That(s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseWell,sequence=6,actorEntityId=2}),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(s.Snapshot().well.mana,Is.EqualTo(mana));
        }
    }
}

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalCastApproachTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        sealed class Navigation:IOriginalWorldNavigation
        {
            internal int mode;
            public OriginalPoint HeroSpawn=>new OriginalPoint(0,0);
            public long NavigationRevision=>0;
            public bool IsWalkable(double x,double y,double radius)=>mode==4?(x-1600.00000005)*(x-1600.00000005)+y*y>=760*760:mode==3?(x-1600)*(x-1600)+(y-6)*(y-6)>=760*760:mode==2?Math.Abs(x)<=200:
                mode!=1||x<750||x>850||Math.Abs(y)>200;
            public bool SegmentClear(OriginalPoint a,OriginalPoint b,double radius)=>mode!=2||IsWalkable(b.x,b.y,radius);
            public OriginalPoint[] FindPath(OriginalPoint a,OriginalPoint b,double radius)=>IsWalkable(b.x,b.y,radius)?new[]{b}:Array.Empty<OriginalPoint>();
            public bool SetDoodadAlive(int id,bool alive)=>true;
        }
        static T Load<T>(string name)=>JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-"+name+".json")));
        static OriginalSession Create(int navigationMode=0,string hero="H008")
        {
            var native=Load<OriginalNativeCatalog>("native126");var measured=Load<OriginalObservedCatalog>("observed126");
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);options.curse=false;
            var s=new OriginalSession(Load<OriginalMatchCatalog>("match"),Load<OriginalItemCatalog>("items"),Load<OriginalCombatCatalog>("combat"),
                Load<OriginalDuelCatalog>("duels"),new string('a',64),options,123);
            s.ConfigureProgression(native,measured);s.ConfigureWorld(new Navigation{mode=navigationMode},native,Array.Empty<OriginalWorldDoodadView>(),measured);
            s.ConfigureItems(native,Load<OriginalObservedItemCatalog>("observed-items126"),Load<OriginalItemPassiveCatalog>("item-passives"));
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=1,kind=OriginalSessionCommandKind.SelectHero,heroId=hero}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=2,kind=OriginalSessionCommandKind.LobbyReady,ready=true}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=3,kind=OriginalSessionCommandKind.Start}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return s;
        }
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalWorld World(OriginalSession s)=>(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
        static OriginalItemInstance Add(OriginalSession s,string id,int charges=1)
        {var item=Inventory(s).CreateInstance(id);item.chargesKnown=true;item.charges=charges;Assert.That(Inventory(s).TryPickup(item).Applied,Is.True);return item;}
        static OriginalSessionCommand Command(OriginalSession s,OriginalItemInstance item)=>new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemSlot=0,itemInstanceId=item.instanceId,x=1000,y=0};
        static void Advance(OriginalSession s,double seconds)
        {for(double left=seconds;left>1e-8;left-=.05){s.Advance(Math.Min(.05,left));Assert.That(s.HaltReason,Is.Null);}}
        static OriginalWorldUnitView Ward(OriginalSession s)=>World(s).Snapshot().units.SingleOrDefault(x=>x.kind==OriginalWorldUnitKind.Summon);
        static OriginalSessionReplyCode Send(OriginalSession s,OriginalSessionCommand c)
        {c.sequence=s.Snapshot().players[0].acknowledgedSequence+1;return s.Apply(0,c);}
        static int Queued(OriginalSession s)=>((IDictionary)typeof(OriginalSession).GetField("queuedCastApproaches",Private).GetValue(s)).Count;
        static void Control(OriginalSession s,OriginalActorControlMask mask,double seconds=.5)
        {Assert.That(typeof(OriginalSession).GetMethod("SetActorControl",Private).Invoke(s,new object[]{1,"approach-test",mask,seconds,true,false}),Is.True);}
        static void Target(OriginalWorld w)=>w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=3000,maxMana=1000,collisionRadius=8},new OriginalPoint(1000,0));

        [TestCase(OriginalSessionCommandKind.Move),TestCase(OriginalSessionCommandKind.HoldPosition),TestCase(OriginalSessionCommandKind.AttackMove)]
        public void AcceptedMovementOrdersCancelTheOldCastIntent(OriginalSessionCommandKind kind)
        {
            var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(Send(s,new OriginalSessionCommand{kind=kind,x=300,y=300}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Queued(s),Is.Zero);Advance(s,5);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void InvalidReplacementPreservesTheAcceptedIntentAndItsCopiedCoordinates()
        {
            var s=Create();var item=Add(s,"I021");var c=Command(s,item);
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));c.x=900000;c.y=900000;
            var invalid=Command(s,item);invalid.x=double.NaN;
            Assert.That(s.Apply(0,invalid),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Queued(s),Is.EqualTo(1));Advance(s,5);Assert.That(Ward(s).position.x,Is.EqualTo(1000));Assert.That(Ward(s).position.y,Is.Zero);
        }
        [Test] public void ANewAcceptedFarCastReplacesThePreviousPointExactlyOnce()
        {
            var s=Create();var item=Add(s,"I021",2);Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,.2);var c=Command(s,item);c.x=0;c.y=1000;Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Queued(s),Is.EqualTo(1));Advance(s,6);Assert.That(Ward(s).position.x,Is.Zero);Assert.That(Ward(s).position.y,Is.EqualTo(1000));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));Assert.That(Queued(s),Is.Zero);
        }
        [Test] public void DroppingTheExactItemInvalidatesItsIntentWithoutConsumingTheGroundInstance()
        {
            var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.DropItem,itemSlot=0,itemInstanceId=item.instanceId}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,5);Assert.That(Ward(s),Is.Null);Assert.That(Queued(s),Is.Zero);
            Assert.That(s.Snapshot().groundItems.Single().item.instanceId,Is.EqualTo(item.instanceId));Assert.That(s.Snapshot().groundItems.Single().item.charges,Is.EqualTo(1));
        }
        [TestCase(false),TestCase(true)] public void LostUnitTargetCancelsWithoutItemDebit(bool hide)
        {
            var s=Create();var item=Add(s,"I021");var w=World(s);Target(w);var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            if(hide)Assert.That(w.SetVisibility(9001,false),Is.True);else Assert.That(w.ForceUnitDeath(9001),Is.True);
            Advance(s,5);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));Assert.That(Queued(s),Is.Zero);
        }
        [Test] public void ManaLossDuringApproachCannotCommitTheItemEffectOrCooldown()
        {
            var s=Create();var item=Add(s,"I06J",0);var w=World(s);Target(w);var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            var actor=w.UnitState(1);Assert.That(w.UpdateProfile(1,actor.profile,actor.health,0),Is.True);Advance(s,.05);
            Assert.That(Queued(s),Is.Zero);Assert.That(s.Snapshot().players[0].itemUses.Single().cooldownRemaining,Is.Zero);
            Assert.That(w.UnitState(9001).mana,Is.EqualTo(1000));
        }
        [Test] public void PauseWaitsButDeathAndRestoreCannotResumeAnOldItemIntent()
        {
            var s=Create();var item=Add(s,"I021");var w=World(s);Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(w.SetUnitState(1,paused:true),Is.True);var pos=w.UnitState(1).position;Advance(s,.5);
            Assert.That(Queued(s),Is.EqualTo(1));Assert.That(w.UnitState(1).position.x,Is.EqualTo(pos.x));Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(w.SetUnitState(1,paused:false),Is.True);Assert.That(w.ForceUnitDeath(1),Is.True);Advance(s,.05);
            Assert.That(Queued(s),Is.Zero);Assert.That(w.RestoreUnit(1,pos),Is.True);Advance(s,5);Assert.That(Ward(s),Is.Null);
        }
        [Test] public void ItemControlWaitsAndCastOnlyControlDoesNotBlockItems()
        {
            var s=Create();var item=Add(s,"I021");Control(s,OriginalActorControlMask.Item|OriginalActorControlMask.Move);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(World(s).UnitState(1).position.x,Is.Zero);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Advance(s,.4);Control(s,OriginalActorControlMask.Cast,5);Advance(s,5);Assert.That(Ward(s),Is.Not.Null);
        }
        [Test] public void SourceStopBoundaryCancelsAnIntentBeforeDuelReturn()
        {
            var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(World(s).Stop(1),Is.True);typeof(OriginalSession).GetMethod("OnAcceptedWorldOrder",Private).Invoke(s,new object[]{1});
            Advance(s,5);Assert.That(Queued(s),Is.Zero);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [TestCase("N0A0","A15W"),TestCase("H024","A0SJ")]
        public void HeroPointSkillApproachesWithoutEarlyDebitAndAcceptsItsActualAlias(string hero,string learn)
        {
            var s=Create(hero:hero);Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LearnSkill,skillId=learn}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var ability=s.Snapshot().players[0].abilities.Single(x=>x.id==learn);double mana=World(s).UnitState(1).mana;
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId=ability.castAbilityId,x=1600,y=0}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(s).UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));Assert.That(World(s).UnitState(1).mana,Is.EqualTo(mana));Assert.That(Queued(s),Is.EqualTo(1));
            Advance(s,.2);Assert.That(World(s).UnitState(1).position.x,Is.GreaterThan(0));
            Advance(s,6);Assert.That(Queued(s),Is.Zero);Assert.That(World(s).UnitState(1).castSequence,Is.GreaterThan(0));
            Assert.That(s.Snapshot().players[0].abilities.Single(x=>x.id==learn).cooldownRemaining,Is.GreaterThan(0));
        }
        [Test] public void CastControlRejectsNewSkillsAndCancelsPendingSkillsWithoutSpending()
        {
            var s=Create(hero:"N0A0");Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LearnSkill,skillId="A15W"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            double mana=World(s).UnitState(1).mana;
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId="A15W",x=1600}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Control(s,OriginalActorControlMask.Cast,5);Advance(s,.05);Assert.That(Queued(s),Is.Zero);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId="A15W",x=1600}),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(mana));Assert.That(World(s).UnitState(1).castSequence,Is.Zero);
        }
        [Test] public void OwnedSummonUsesOnlyItsOwnMana()
        {
            var s=Create();var w=World(s);Target(w);int summon=OriginalWorld.FirstSummonEntityId;
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=summon,ownerSlot=1,sourceHeroEntityId=1,rawcode="n02K",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=1000,moveSpeed=250,collisionRadius=16},health=1000,mana=1000,position=new OriginalPoint(0,100)}}),Is.True);
            double mana=w.UnitState(summon).mana,heroMana=w.UnitState(1).mana;
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,actorEntityId=summon,skillId="A08U",targetKind=OriginalWorldTargetKind.Unit,targetId=9001}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(summon).mana,Is.EqualTo(mana));Assert.That(w.UnitState(1).mana,Is.EqualTo(heroMana));
            Advance(s,6);Assert.That(Queued(s),Is.Zero);Assert.That(w.UnitState(summon).castSequence,Is.GreaterThan(0));Assert.That(w.UnitState(summon).mana,Is.LessThan(mana));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(heroMana));
        }
        [TestCase(-1e-15,0),TestCase(0,0),TestCase(360,0),TestCase(1080,0),TestCase(-1081,359)]
        public void NativeFacingAlwaysStaysInsideTheHalfOpenDegreeInterval(double input,double expected)
        {
            var s=Create();Assert.That(World(s).SetFacing(1,input),Is.True);
            Assert.That(World(s).UnitState(1).facingDegrees,Is.EqualTo(expected));
            Assert.That(World(s).UnitState(1).facingDegrees,Is.GreaterThanOrEqualTo(0).And.LessThan(360));
        }
        [Test] public void AcceptedBerserkCancelsTheOldIntentAndRetainsItsExistingOrderPolicy()
        {
            var s=Create(hero:"N0A0");
            ((OriginalHeroProgression)Player(s).GetType().GetField("progression").GetValue(Player(s))).SyncMatchExperience(10000);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LearnSkill,skillId="A0AS"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            var before=World(s).UnitState(1);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId="A0AS"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Queued(s),Is.Zero);
            // ARCHER_NATIVE proves order0 for current Absk; keep its actual
            // consumer policy rather than introducing a blanket Stop hook.
            var rules=new OriginalArcherCastRules(Load<OriginalCombatCatalog>("combat"),"A0AS",1);
            Assert.That(World(s).UnitState(1).order,Is.EqualTo(rules.preservesAttackOrder?before.order:OriginalWorldOrder.None));
            Advance(s,5);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void ASourceTauntReplacesTheApproachWithoutRecursionOrResources()
        {
            var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            var w=World(s);w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=3000,collisionRadius=8},new OriginalPoint(100,100));
            typeof(OriginalSession).GetMethod("BeginItemTaunt",Private).Invoke(s,new object[]{9001,"A0OO"});Advance(s,.05);
            Assert.That(Queued(s),Is.Zero);Assert.That(w.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));Assert.That(w.UnitState(1).targetId,Is.EqualTo(9001));
            Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void TransferAndReplacementInvalidateOnlyTheExactPendingInstance()
        {
            foreach(bool replace in new[]{false,true})
            {
                var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
                Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.TransferItem,itemSlot=0,itemInstanceId=item.instanceId}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                if(replace)
                {
                    Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.DropItem,bag=OriginalInventoryBag.Servant,itemSlot=0,itemInstanceId=item.instanceId}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                    var next=Add(s,"I021");Assert.That(next.instanceId,Is.Not.EqualTo(item.instanceId));
                }
                Advance(s,.05);Assert.That(Queued(s),Is.Zero);Advance(s,5);Assert.That(Ward(s),Is.Null);
                if(replace){Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));Assert.That(s.Snapshot().groundItems.Single().item.charges,Is.EqualTo(1));}
                else {Assert.That(Inventory(s).ServantSlots[0].instanceId,Is.EqualTo(item.instanceId));Assert.That(Inventory(s).ServantSlots[0].charges,Is.EqualTo(1));}
            }
        }
        [Test] public void AnUnrelatedInventoryTransferDoesNotCancelAValidCast()
        {
            var s=Create();var item=Add(s,"I021");var other=Add(s,"I01Y");
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.TransferItem,itemSlot=1,itemInstanceId=other.instanceId}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,.05);Assert.That(Queued(s),Is.EqualTo(1));Advance(s,5);Assert.That(Ward(s),Is.Not.Null);
            Assert.That(Inventory(s).ServantSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void CooldownAppearingBeforeArrivalCancelsWithoutManaDebit()
        {
            var s=Create();var item=Add(s,"I06J",0);Target(World(s));var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            ((IDictionary)typeof(OriginalSession).GetField("itemCooldowns",Private).GetValue(s))["1:A0FI"]=World(s).Clock+20;
            Advance(s,.05);Assert.That(Queued(s),Is.Zero);Assert.That(World(s).UnitState(1).mana,Is.EqualTo(World(s).UnitState(1).profile.maxMana));
            Assert.That(World(s).UnitState(9001).mana,Is.EqualTo(1000));
        }
        [Test] public void ApproachMotionIsNotReflectedByTheSourcePointOrderCurse()
        {
            var s=Create();var w=World(s);var center=w.UnitState(1).position;
            typeof(OriginalSession).GetMethod("BeginCasterEffect",Private).Invoke(s,new object[]{9001,0,new OriginalCasterRules(Load<OriginalCombatCatalog>("combat"),"A1DF"),center,w.Clock});
            for(int i=0;i<20;i++){w.Advance(.05);typeof(OriginalSession).GetMethod("AdvanceCasters",Private).Invoke(s,null);}
            var reflected=(OriginalPoint)typeof(OriginalSession).GetMethod("CasterAuraPointOrder",Private).Invoke(s,new object[]{1,851986,new OriginalPoint(100,0)});
            Assert.That(reflected.x,Is.EqualTo(-100));
            var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).destination.x,Is.GreaterThan(0));Assert.That(Queued(s),Is.EqualTo(1));
        }
        [Test] public void WardUnitOrPointMetadataIsStrictAndOldProtocolCannotJoin()
        {
            var s=Create();Add(s,"I021");var codec=new OriginalUnitySessionCodec();var view=s.Snapshot();
            var response=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,assignedSlot=1,
                acknowledgedSequence=view.players[0].acknowledgedSequence,code=OriginalSessionReplyCode.Accepted,snapshot=view};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True);
            view.players[0].itemUses[0].targetMode=OriginalAbilityTargetMode.Point;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
            var lobby=new OriginalSession(Load<OriginalMatchCatalog>("match"),Load<OriginalItemCatalog>("items"),Load<OriginalCombatCatalog>("combat"),Load<OriginalDuelCatalog>("duels"),view.contentHash,OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard),123);
            Assert.That(lobby.Apply(123,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Hello,sequence=1,protocol=OriginalSession.Protocol-1,contentHash=view.contentHash}),Is.EqualTo(OriginalSessionReplyCode.VersionMismatch));
            var s2=Create();Add(s2,"I06J",0);response.snapshot=s2.Snapshot();response.acknowledgedSequence=response.snapshot.players[0].acknowledgedSequence;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True);
            response.snapshot.players[0].itemUses[0].targetMode=OriginalAbilityTargetMode.UnitOrPoint;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
            response.snapshot=s2.Snapshot();response.snapshot.players[0].abilities[0].targetMode=OriginalAbilityTargetMode.UnitOrPoint;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
        }

        [TestCase("I021"),TestCase("I094")]
        public void DistantWardMovesWithoutDebitThenPublishesOneWard(string id)
        {
            var s=Create();var item=Add(s,id,2);var before=World(s).UnitState(1);var c=Command(s,item);
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(2));Assert.That(Ward(s),Is.Null);
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(before.mana));
            Assert.That(World(s).UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));
            Advance(s,.2);Assert.That(World(s).UnitState(1).position.x,Is.GreaterThan(0));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(2));
            Advance(s,4);var ward=Ward(s);Assert.That(ward,Is.Not.Null);
            Assert.That(ward.position.x,Is.EqualTo(1000));Assert.That(ward.position.y,Is.Zero);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Advance(s,2);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(World(s).Snapshot().units.Count(x=>x.kind==OriginalWorldUnitKind.Summon),Is.EqualTo(1));
        }

        [TestCase("I021"),TestCase("I094")]
        public void StopDuringWardApproachCancelsWithoutAChargeOrEffect(string id)
        {
            var s=Create();var item=Add(s,id);Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,.2);var position=World(s).UnitState(1).position;
            Assert.That(s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Stop,
                sequence=s.Snapshot().players[0].acknowledgedSequence+1}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,6);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(World(s).UnitState(1).position.x,Is.EqualTo(position.x));
            Assert.That(World(s).UnitState(1).position.y,Is.EqualTo(position.y));
        }

        [Test] public void DistantManaItemApproachesBeforeItsMeasuredSeventyDebit()
        {
            var s=Create();var item=Add(s,"I06J",0);var w=World(s);var hero=w.UnitState(1);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=3000,maxMana=1000,collisionRadius=8},new OriginalPoint(1000,0));
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana));
            Assert.That(s.Snapshot().players[0].itemUses.Single().cooldownRemaining,Is.Zero);
            Advance(s,.2);Assert.That(w.UnitState(1).position.x,Is.GreaterThan(0));Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana));
            for(int i=0;i<80&&s.Snapshot().players[0].itemUses.Single().cooldownRemaining==0;i++)Advance(s,.05);
            Assert.That(s.Snapshot().players[0].itemUses.Single().cooldownRemaining,Is.GreaterThan(0));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-70).Within(.1));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
        }

        [Test] public void WardApproachUsesTheMovingUnitCoordinatesAtExecution()
        {
            var s=Create();var item=Add(s,"I021");var w=World(s);
            w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=3000,collisionRadius=8},new OriginalPoint(1000,0));
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(w.Relocate(9001,new OriginalPoint(1100,200)),Is.True);
            Advance(s,5);var ward=Ward(s);Assert.That(ward,Is.Not.Null);
            double dx=ward.position.x-1100,dy=ward.position.y-200;
            Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(64*64));Assert.That(Inventory(s).HeroSlots[0],Is.Null);
        }

        [Test] public void WardBehindAnObstacleCanCastFromAReachableStandoff()
        {
            var s=Create(1);var item=Add(s,"I021");
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,5);
            Assert.That(Ward(s),Is.Not.Null);Assert.That(World(s).UnitState(1).position.x,Is.LessThan(750));
            Assert.That(Ward(s).position.x,Is.EqualTo(1000));
        }
        [Test] public void ABlockedTargetPointDoesNotNeedToBeTheActorsMoveDestination()
        {
            var s=Create(1);var item=Add(s,"I021");var c=Command(s,item);c.x=800;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(s).UnitState(1).destination.x,Is.LessThan(750));Advance(s,5);
            var ward=Ward(s);Assert.That(ward,Is.Not.Null);double dx=ward.position.x-800,dy=ward.position.y;
            Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(64*64));Assert.That(Inventory(s).HeroSlots[0],Is.Null);
        }

        [Test] public void AnUnreachableReplacementDoesNotEraseAnAcceptedMovementOrder()
        {
            var s=Create(2);var item=Add(s,"I021");
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Move,x=150}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(World(s).UnitState(1).order,Is.EqualTo(OriginalWorldOrder.Move));Assert.That(World(s).UnitState(1).destination.x,Is.EqualTo(150));
            Advance(s,2);Assert.That(Ward(s),Is.Null);Assert.That(World(s).UnitState(1).position.x,Is.EqualTo(150));Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }
        [Test] public void BoundaryStandoffInsideTheDeclaredRangeExecutesOnceWithoutStalling()
        {
            // The walkable annulus begins at .95 of source A15W range800.
            // It rejects the preferred .8 goal and exercises outer fallback.
            var s=Create(3,"N0A0");Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LearnSkill,skillId="A15W"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var before=World(s).UnitState(1);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId="A15W",x=1600,y=6}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(before.mana));Assert.That(World(s).UnitState(1).castSequence,Is.Zero);
            Advance(s,8);Assert.That(Queued(s),Is.Zero);Assert.That(World(s).UnitState(1).castSequence,Is.EqualTo(1));
            Assert.That(s.Snapshot().players[0].abilities.Single(x=>x.id=="A15W").cooldownRemaining,Is.GreaterThan(0));
        }
        [Test] public void WorldArrivalToleranceCannotRetainAnIntentOutsideTheStrictCastRange()
        {
            var s=Create(4,"N0A0");Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.LearnSkill,skillId="A15W"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.CastSkill,skillId="A15W",x=1600.00000005,y=0}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s,8);Assert.That(Queued(s),Is.Zero);Assert.That(World(s).UnitState(1).castSequence,Is.EqualTo(1));
            Assert.That(s.Snapshot().players[0].abilities.Single(x=>x.id=="A15W").cooldownRemaining,Is.GreaterThan(0));
        }
        [TestCase(true),TestCase(false)] public void AcceptedDirectWellReplacesCastButRejectedWellPreservesIt(bool accepted)
        {
            var s=Create();var item=Add(s,"I021");Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Advance(s,.2);
            Assert.That(Queued(s),Is.EqualTo(1));
            // Trusted well availability isolates order replacement from OU.
            typeof(OriginalSession).GetField("healingWellPresent",Private).SetValue(s,true);
            typeof(OriginalSession).GetField("healingWellMana",Private).SetValue(s,20d);
            var actor=World(s).UnitState(1);if(accepted)Assert.That(World(s).UpdateProfile(1,actor.profile,actor.health-10,actor.mana),Is.True);
            Assert.That(Send(s,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseWell}),Is.EqualTo(accepted?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.NotReady));
            Assert.That(Queued(s),Is.EqualTo(accepted?0:1));Advance(s,5);Assert.That(Ward(s)==null,Is.EqualTo(accepted));
        }

        [Test] public void AnUnreachableCastStandoffCannotSpendOrMove()
        {
            var s=Create(2);var item=Add(s,"I021");var before=World(s).UnitState(1);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Advance(s,5);Assert.That(Ward(s),Is.Null);Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(World(s).UnitState(1).position.x,Is.EqualTo(before.position.x));
        }
    }
}

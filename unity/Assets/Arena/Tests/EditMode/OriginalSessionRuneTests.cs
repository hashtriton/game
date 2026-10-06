using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionRuneTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        sealed class Navigation:IOriginalWorldNavigation
        {
            internal readonly Dictionary<int,bool> alive=new Dictionary<int,bool>();
            public OriginalPoint HeroSpawn=>new OriginalPoint(135,1000);
            public long NavigationRevision{get;private set;}
            public bool IsWalkable(double x,double y,double radius)=>Math.Abs(x)<6000&&Math.Abs(y)<6000;
            public bool SegmentClear(OriginalPoint a,OriginalPoint b,double r)=>true;
            public OriginalPoint[] FindPath(OriginalPoint a,OriginalPoint b,double r)=>new[]{b};
            public bool SetDoodadAlive(int id,bool value){alive[id]=value;NavigationRevision++;return true;}
        }
        static T Load<T>(string suffix)=>JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-"+suffix+".json")));
        static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Hidden|BindingFlags.Public).GetValue(obj);
        static object Call(object obj,string name,params object[] args)
        {
            var method=obj.GetType().GetMethod(name,Hidden|BindingFlags.Static);Assert.That(method,Is.Not.Null,"Required source consumer: "+name);
            return method.Invoke(obj,args);
        }
        static OriginalSession Create(out OriginalWorld world,out Navigation nav,bool runeOn=true,bool barrels=true,int participants=1,string firstHero="H008")
        {
            var native=Load<OriginalNativeCatalog>("native126");
            foreach(var item in native.items)foreach(var f in item.fields)if(f.field=="stockStart")
            {f.known=true;f.kind="number";f.number=0;f.state="map-declaration";f.sources=new[]{new OriginalNativeEvidence{source=0,line=1}};}
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);options.runes=runeOn;options.explosiveBarrels=barrels;
            var s=new OriginalSession(Load<OriginalMatchCatalog>("match"),Load<OriginalItemCatalog>("items"),Load<OriginalCombatCatalog>("combat"),
                Load<OriginalDuelCatalog>("duels"),new string('a',64),options,123);
            var observed=Load<OriginalObservedCatalog>("observed126");s.ConfigureProgression(native,observed);
            nav=new Navigation();s.ConfigureWorld(nav,native,new[]{
                new OriginalWorldDoodadView{editorId=125,rawcode="LTex",position=new OriginalPoint(-1408,-384),maxHealth=20,health=20},
                new OriginalWorldDoodadView{editorId=126,rawcode="LTbr",position=new OriginalPoint(2048,2048),maxHealth=20,health=20}},observed);
            s.ConfigureItems(native,Load<OriginalObservedItemCatalog>("observed-items126"),Load<OriginalItemPassiveCatalog>("item-passives"));
            string[] heroes={firstHero,firstHero=="N0A0"?"H008":"N0A0","H024"};
            for(int i=0;i<participants;i++)
            {
                long connection=i==0?0:i;long sequence=1;
                if(i>0){Assert.That(s.Apply(connection,new OriginalSessionCommand{sequence=1,kind=OriginalSessionCommandKind.Hello,protocol=OriginalSession.Protocol,contentHash=new string('a',64)}),Is.EqualTo(OriginalSessionReplyCode.Accepted));sequence++;}
                Assert.That(s.Apply(connection,new OriginalSessionCommand{sequence=sequence++,kind=OriginalSessionCommandKind.SelectHero,heroId=heroes[i]}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(s.Apply(connection,new OriginalSessionCommand{sequence=sequence,kind=OriginalSessionCommandKind.LobbyReady,ready=true}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=3,kind=OriginalSessionCommandKind.Start}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            world=Field<OriginalWorld>(s,"world");return s;
        }
        static object Player(OriginalSession s)=>Field<IList>(s,"players")[0];
        static OriginalInventory Inventory(OriginalSession s)=>Field<OriginalInventory>(Player(s),"inventory");
        static SortedDictionary<long,OriginalGroundItemView> Ground(OriginalSession s)=>Field<SortedDictionary<long,OriginalGroundItemView>>(s,"groundItems");
        static long AddGround(OriginalSession s,OriginalWorld w,string id,OriginalPoint? at=null)
        {var item=Inventory(s).CreateInstance(id);Ground(s).Add(item.instanceId,new OriginalGroundItemView{item=item,position=at??w.UnitState(1).position});return item.instanceId;}
        static void Pickup(OriginalSession s,long id)
        {Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=s.Snapshot().players[0].acknowledgedSequence+1,kind=OriginalSessionCommandKind.PickupItem,itemInstanceId=id,bag=OriginalInventoryBag.Hero}),Is.EqualTo(OriginalSessionReplyCode.Accepted));}
        static void Phase(OriginalSession s,int round,OriginalMatchPhase phase)
        {
            var match=Field<OriginalMatch>(s,"match");typeof(OriginalMatch).GetProperty("Round").SetValue(match,round);
            typeof(OriginalMatch).GetProperty("Phase").SetValue(match,phase);
            Call(s,"OnRuneMatchEvent",new OriginalMatchEvent{kind=OriginalMatchEventKind.PhaseChanged,round=round,amount=(int)phase});
        }
        static void RuneClock(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-8){double step=Math.Min(.05,seconds);w.Advance(step);Call(s,"AdvanceRuneAndBarrelWorld");seconds-=step;}}
        static void StatusClock(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-8){double step=Math.Min(.05,seconds);w.Advance(step);Call(s,"AdvanceRunePowerups",step);seconds-=step;}}

        [Test] public void DisabledExplosiveBarrelsAreRemovedWithoutBreakingOrdinaryPassageBarrels()
        {
            var s=Create(out var w,out var nav,barrels:false);
            Assert.That(s.Snapshot().world.doodads.Select(d=>d.rawcode),Is.EqualTo(new[]{"LTbr"}));
            Assert.That(nav.alive[125],Is.False);Assert.That(nav.alive[126],Is.True);
            Assert.That(w.ApplyDoodadDamage(125,20),Is.False);
        }
        [Test] public void ExplosiveProtectionTracksThreeTwoThreeLivingCanonicalHeroes()
        {
            var s=Create(out var w,out _,participants:3);
            Assert.That(s.Snapshot().world.doodads.Single(d=>d.editorId==125).invulnerable,Is.True);
            Assert.That(w.ApplyDoodadDamage(125,1),Is.False);
            Assert.That(w.ForceUnitDeath(3),Is.True);RuneClock(s,w,.05);
            Assert.That(s.Snapshot().world.doodads.Single(d=>d.editorId==125).invulnerable,Is.False);
            Assert.That(w.RestoreUnit(3,new OriginalPoint(1000,1000)),Is.True);RuneClock(s,w,.05);
            Assert.That(s.Snapshot().world.doodads.Single(d=>d.editorId==125).invulnerable,Is.True);
            Assert.That(w.ApplyDoodadDamage(126,1),Is.True,"Protection only belongs to LTex.");
        }
        [Test] public void OrdinaryFortyFiveSecondRuneClockPersistsAcrossPreparationAndCleansChosenRect()
        {
            var s=Create(out var w,out _);Phase(s,1,OriginalMatchPhase.Combat);RuneClock(s,w,20);
            Phase(s,2,OriginalMatchPhase.Preparation);RuneClock(s,w,20);Assert.That(Ground(s),Is.Empty);
            Phase(s,2,OriginalMatchPhase.Combat);RuneClock(s,w,24);Assert.That(Ground(s),Is.Empty);
            long a=AddGround(s,w,"I007",new OriginalPoint(-1156,1156)),b=AddGround(s,w,"I007",new OriginalPoint(1156,400));
            RuneClock(s,w,1);Assert.That(Ground(s).Count,Is.EqualTo(2),"One chosen rectangle cleans its old item and creates one rune.");
            Assert.That(Ground(s).ContainsKey(a)^Ground(s).ContainsKey(b),Is.True);
            var rune=Ground(s).Values.Single(g=>g.item.itemId!="I007");Assert.That(rune.item.ownerId,Is.Zero);
            Assert.That(rune.position.x,Is.EqualTo(-1156d).Or.EqualTo(1156d));
        }
        [Test] public void RuneOffStillAllowsTwentyFiveSecondMegaBossRunesAndPreservesCounter()
        {
            var s=Create(out var w,out _,runeOn:false);Phase(s,1,OriginalMatchPhase.Combat);RuneClock(s,w,90);Assert.That(Ground(s),Is.Empty);
            Phase(s,5,OriginalMatchPhase.BossCountdown);RuneClock(s,w,5);Assert.That(Ground(s),Is.Empty);
            Phase(s,5,OriginalMatchPhase.Combat);RuneClock(s,w,20);Assert.That(Ground(s),Is.Empty);
            Phase(s,6,OriginalMatchPhase.Preparation);RuneClock(s,w,30);Assert.That(Ground(s),Is.Empty);
            Phase(s,10,OriginalMatchPhase.Combat);RuneClock(s,w,5);
            Assert.That(Ground(s).Count,Is.EqualTo(1));Assert.That(Ground(s).Values.Single().position.x,Is.EqualTo(-2));
            Assert.That(Ground(s).Values.Single().position.y,Is.EqualTo(-1920));
        }
        [Test] public void ManaRuneCapsOrganicAlliesAndDoesNotRestoreEnemiesOrMechanicalUnits()
        {
            var s=Create(out var w,out _);var hero=w.UnitState(1);var p=new OriginalWorldUnitProfile{maxHealth=hero.profile.maxHealth,
                maxMana=1000,moveSpeed=hero.profile.moveSpeed,collisionRadius=hero.profile.collisionRadius};w.UpdateProfile(1,p,hero.health,100);
            var rows=new[]{(100000201,"hfoo",235d),(100000202,"hmtt",335d),(100000203,"hfoo",2235d)};
            foreach(var row in rows)Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=row.Item1,ownerSlot=1,sourceHeroEntityId=1,rawcode=row.Item2,
                position=new OriginalPoint(row.Item3,1000),profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=1000,collisionRadius=16},health=1000,mana=900}}),Is.True);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,maxMana=1000,collisionRadius=16},new OriginalPoint(435,1000));
            w.UpdateProfile(1001,w.UnitState(1001).profile,1000,100);Pickup(s,AddGround(s,w,"rman"));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(350));Assert.That(w.UnitState(100000201).mana,Is.EqualTo(1000));
            Assert.That(w.UnitState(100000202).mana,Is.EqualTo(900));Assert.That(w.UnitState(100000203).mana,Is.EqualTo(900));
            Assert.That(w.UnitState(1001).mana,Is.EqualTo(100));Assert.That(Inventory(s).HeroSlots.All(i=>i==null),Is.True);
        }
        [Test] public void VampireRuneAddsFiftyWeaponDamageAndHealsActualPrimaryLossThenExpires()
        {
            var s=Create(out var w,out _);var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,hero.health-200,hero.mana);
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(235,1000));
            double before=s.Snapshot().players[0].combat.attackMinimum;Pickup(s,AddGround(s,w,"vamp"));
            Assert.That(s.Snapshot().players[0].combat.attackMinimum-before,Is.EqualTo(50).Within(1e-7));
            double life=w.UnitState(1).health;w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001);Call(s,"AdvanceWeapons");
            for(int i=0;i<80&&w.UnitState(1001).health==10000;i++){w.Advance(.01);Call(s,"AdvanceWeapons");}
            double loss=10000-w.UnitState(1001).health;Assert.That(loss,Is.GreaterThan(0));Assert.That(w.UnitState(1).health-life,Is.EqualTo(loss).Within(1e-7));
            w.Stop(1);StatusClock(s,w,15.01);Assert.That(s.Snapshot().players[0].combat.attackMinimum,Is.EqualTo(before).Within(1e-7));
        }
        [Test] public void ItemRunePreservesSourceEmptyDrawsAndFullInventoryRewardOnGround()
        {
            var s=Create(out var w,out _);foreach(int empty in new[]{7,9,10})
                Assert.That(Call(s,"RuneItemReward",empty),Is.Null,"Source bOv has no mapping for this draw.");
            foreach(int draw in new[]{1,2,3,4,5,6,8})Assert.That(new[]{"I066","I06P","I068","I06E","I06N","I06G","I0AK"},Does.Contain(Call(s,"RuneItemReward",draw)));
            // aM4493/4545 checks the servant too before its full-bag drop.
            for(int i=0;i<12;i++)Assert.That(Inventory(s).TryPickup(Inventory(s).CreateInstance("I007")).Applied,Is.True);
            // Force an independently computed xorshift seed whose first1..10 draw is1.
            typeof(OriginalSession).GetField("pickupRandom",Hidden).SetValue(s,17u);
            long rune=AddGround(s,w,"rspl");Pickup(s,rune);
            Assert.That(Inventory(s).HeroSlots.Count(i=>i!=null),Is.EqualTo(6));
            Assert.That(Ground(s).Values.Any(g=>g.item.itemId=="I066"&&g.item.ownerId==1),Is.True);
        }
        [Test] public void DispelRuneClearsNegativeStatusesAndDamagesOnlySourceEligibleEnemies()
        {
            var s=Create(out var w,out _);var c=Load<OriginalCombatCatalog>("combat");var n=Load<OriginalNativeCatalog>("native126");
            Call(s,"AddOrdinaryBuff",w.UnitState(1),c.Ability("A071"),10d);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.LessThan(250),"APDI3 exact Bslo is a dispellable magical slow.");
            foreach(var row in new[]{(1001,"hfoo",235d),(1002,"edry",335d),(1003,"hfoo",735d)})
                w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(row.Item3,1000));
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(w.UnitState(1001).health,Is.EqualTo(750));Assert.That(w.UnitState(1002).health,Is.EqualTo(1000));
            Assert.That(w.UnitState(1003).health,Is.EqualTo(1000));
        }
        [Test] public void SpellShieldRuneIsConsumedWithoutInventingImmunityFromFailedPositiveObservation()
        {
            var s=Create(out var w,out _);double health=w.UnitState(1).health;long rune=AddGround(s,w,"rsps");Pickup(s,rune);
            Assert.That(Ground(s).ContainsKey(rune),Is.False);Assert.That(Inventory(s).HeroSlots.All(i=>i==null),Is.True);
            Assert.That(w.ApplyUnitDamage(1,40),Is.True);Assert.That(w.UnitState(1).health,Is.EqualTo(health-40));
        }
        [Test] public void DispelRunePreservesPhysicalEnsnareWhileClearingAlliedMagicBuffs()
        {
            var s=Create(out var w,out _);
            Call(s,"SetActorControl",1,"curse-cold:A19U",OriginalActorControlMask.Move,5d,true,true);
            Call(s,"SetActorControl",1,"native-root:A0KV:control",OriginalActorControlMask.Move,5d,true,true);
            Call(s,"ApplyItemHaste",1,15d,1d);
            Pickup(s,AddGround(s,w,"vamp"));
            Assert.That((bool)Call(s,"ActorMoveBlocked",1),Is.True);
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That((bool)Call(s,"ActorMoveBlocked",1),Is.True,"APDI1 B0BL stays at rank1 on allied and hostile recipients.");
            var controls=Field<IDictionary>(s,"actorControls");var tokens=(IDictionary)controls[1];
            Assert.That(tokens.Contains("curse-cold:A19U"),Is.True);
            Assert.That(tokens.Contains("native-root:A0KV:control"),Is.True,"Aens alias transfer preserves physical ensnare.");
            Assert.That(Field<IDictionary>(s,"itemHaste").Contains(1),Is.False);
            Assert.That(Field<IDictionary>(s,"runeVampires").Contains(1),Is.False);
        }
        [Test] public void DispelClearsLightningCarrierBuffWithoutStoppingItsSourcePulseTimerOrDefend()
        {
            var s=Create(out var w,out _);var actor=w.UnitState(1);
            var rule=Call(s,"ShellItemRule","I01B");Call(s,"BeginItemShell",actor,1,rule);
            var progression=Field<OriginalHeroProgression>(Player(s),"progression");progression.GrantExperience(1000);
            Assert.That(progression.TryLearn("A05M").code,Is.EqualTo(OriginalLearnCode.Learned));
            Field<HashSet<int>>(s,"defendingHeroes").Add(1);
            Assert.That((bool)Call(s,"HasItemLightningBuff",1,"B00O"),Is.True);
            int timers=Field<IList>(s,"itemLightningPulses").Count;
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That((bool)Call(s,"HasItemLightningBuff",1,"B00O"),Is.False,"Alsh carrier is a magic-buff transfer, distinct from authored U8.");
            Assert.That(Field<IList>(s,"itemLightningPulses").Count,Is.EqualTo(timers));
            Assert.That(Field<HashSet<int>>(s,"defendingHeroes").Contains(1),Is.True,"Adef is an activated stance, not this magic buff.");
        }
        [Test] public void DispelNativeSummonDamageOnlyHitsNonimmuneEnemiesAndSourceDamageRemainsSeparate()
        {
            var s=Create(out var w,out _);
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=100000501,ownerSlot=1,sourceHeroEntityId=1,
                rawcode="n01R",position=new OriginalPoint(235,1000),profile=new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},health=1000}}),Is.True);
            w.AddUnit(1001,0,"n01R",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(335,1000));
            w.AddUnit(1002,0,"n01R",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(435,1000));
            Call(s,"ApplyUnitAbilityOverlay",1002,new[]{"Amim"},null);
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(w.UnitState(100000501).health,Is.EqualTo(1000),"APDI3 allied actualAsum receives no native250.");
            Assert.That(w.UnitState(1001).health,Is.EqualTo(500),"NativeAPdi250 plus separate I6hL250 on sourceowner11.");
            Assert.That(w.UnitState(1002).health,Is.EqualTo(1000),"NativeAmim blocks APdi and sourceI6 excludes immune.");
        }
        [Test] public void DispelNativeZeroEventCanConsumeArcherWatchWhenSourceDamageDoesNotApply()
        {
            var s=Create(out var w,out _,participants:2,firstHero:"N0A0");
            Field<OriginalHeroProgression>(Player(s),"progression").GrantExperience(1000);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=4,kind=OriginalSessionCommandKind.LearnSkill,skillId="A15X"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=5,kind=OriginalSessionCommandKind.CastSkill,skillId="A15X",targetKind=OriginalWorldTargetKind.None}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            s.Advance(.31);Assert.That(s.HaltReason,Is.Null);
            var archer=Call(s,"Archer",1);Assert.That(Field<int>(archer,"charges"),Is.EqualTo(5));
            Field<HashSet<int>>(s,"hostilePlayerPairs").Add(18);
            Call(s,"OnArcherAttackStarted",w.UnitState(1),w.UnitState(2));
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(Field<int>(archer,"charges"),Is.EqualTo(4),"APdi hostileordinary0 event reaches exacthero-source aie watcher.");
        }
        [Test] public void DispelLeavesNativeRegenerationCarrierAndItsNextResourcePulse()
        {
            var s=Create(out var w,out _);var actor=w.UnitState(1);w.UpdateProfile(1,actor.profile,actor.health,0);
            Call(s,"ApplyNativeItemStatus",1,Call(s,"RegenerationItemRule"));
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(Field<IDictionary>(s,"itemRegeneration").Contains(1),Is.True,"APDI3 positive B0B1 stays after APdi on both teams.");
            for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceItemStatuses",.05);}
            Assert.That(w.UnitState(1).mana,Is.EqualTo(.1).Within(1e-8));
        }
        [Test] public void DispelPreservesAvenPoisonAndAprgMovementStateWithoutResettingThem()
        {
            var s=Create(out var w,out _);var c=Load<OriginalCombatCatalog>("combat");
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=5000,collisionRadius=16},new OriginalPoint(335,1000));
            Assert.That((bool)Call(s,"ResolveOrdinaryNativeStatus",w.UnitState(1001),w.UnitState(1),c.Ability("ACpu")),Is.True);
            var projectileType=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);
            var shot=Activator.CreateInstance(projectileType,true);
            projectileType.GetField("poisonAbility",Hidden).SetValue(shot,"A0TC");
            projectileType.GetField("attacker",Hidden).SetValue(shot,1001);projectileType.GetField("owner",Hidden).SetValue(shot,0);
            Call(s,"ApplyPoison",shot,w.UnitState(1));
            var poison=Field<IDictionary>(s,"poisons")[1];var statuses=(IDictionary)Field<IDictionary>(s,"ordinaryStatuses")[1];
            var purge=statuses["Bprg"];Assert.That(poison,Is.Not.Null);Assert.That(purge,Is.Not.Null);
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(Field<IDictionary>(s,"poisons")[1],Is.SameAs(poison),"Native Aven physicalpoison retains original expiry/tick phase.");
            Assert.That(((IDictionary)Field<IDictionary>(s,"ordinaryStatuses")[1])["Bprg"],Is.SameAs(purge),"Aprg movement cannot be dispelled; retain its age and timer.");
        }
        [Test] public void DispelPreservesNativeDoomAndItsAbilityItemBlocksOnAnotherHero()
        {
            var s=Create(out var w,out _,participants:2);w.ForcePosition(2,new OriginalPoint(235,1000));
            Call(s,"QueueBossDoom",0,2);var state=Field<IDictionary>(s,"bossDooms")[2];Assert.That(state,Is.Not.Null);
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That(Field<IDictionary>(s,"bossDooms")[2],Is.SameAs(state),"Map A0HR inherits ANdo, whose Doom cannot be dispelled.");
            Assert.That((bool)Call(s,"ActorCastBlocked",2),Is.True);
            var controls=(IDictionary)Field<IDictionary>(s,"actorControls")[2];Assert.That(controls.Contains("native-doom:B0BN"),Is.True);
        }
        [Test] public void DispelPreservesBothNativeFrostOrbAliasesAndTheirExistingExpiryObjects()
        {
            foreach(string ability in new[]{"A062","A0BJ"})
            {
                var s=Create(out var w,out _,participants:2);w.ForcePosition(2,new OriginalPoint(235,1000));
                var rules=new OriginalArcherDebuffRules(Load<OriginalCombatCatalog>("combat"),Load<OriginalNativeCatalog>("native126"),ability,1);
                Call(s,"ApplyArcherNativeBuff",2,rules);
                var state=((IDictionary)Field<IDictionary>(s,"archerDebuffs")[2])[rules.buffId];double speed=w.UnitState(2).profile.moveSpeed;
                Pickup(s,AddGround(s,w,"rdis"));
                Assert.That(Field<IDictionary>(s,"archerDebuffs").Contains(2),Is.True,"APDI4 retains both physical frost recipients.");
                Assert.That(((IDictionary)Field<IDictionary>(s,"archerDebuffs")[2])[rules.buffId],Is.SameAs(state),"APDI4 retains exact AIob buff after actual rdis activation.");
                Assert.That(w.UnitState(2).profile.moveSpeed,Is.EqualTo(speed));
                for(int i=0;i<21;i++){w.Advance(.05);Call(s,"AdvanceArcherDebuffs");}
                Assert.That(Field<IDictionary>(s,"archerDebuffs").Contains(2),Is.False,"The retained nativehero1s duration is not refreshed.");
            }
        }
        [Test] public void DispelPreservesWeaponBashAndSpellStunControlsThenLetsOriginalTimersExpire()
        {
            var s=Create(out var w,out _,participants:2);w.ForcePosition(2,new OriginalPoint(235,1000));
            w.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=5000,collisionRadius=16},new OriginalPoint(335,1000));
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A0H5"},null);
            var type=typeof(OriginalSession).GetNestedType("Projectile",BindingFlags.NonPublic);var shot=Activator.CreateInstance(type,true);
            foreach(var pair in new[]{("attacker",1001),("owner",0),("target",2)})type.GetField(pair.Item1,Hidden).SetValue(shot,pair.Item2);
            type.GetField("nativeProcs",Hidden).SetValue(shot,Call(s,"CaptureNativeWeaponProcs",w.UnitState(1001)));
            Call(s,"ResolveNativeWeaponProcs",shot,w.UnitState(2));
            var bash=Field<IList>(s,"nativeBashes")[0];Assert.That(bash,Is.Not.Null);
            Call(s,"AddTimedNativeStun",2,"BPSE",1001,3d);
            var controls=(IDictionary)Field<IDictionary>(s,"actorControls")[2];var spell=controls["native-stun:BPSE:1001"];
            Pickup(s,AddGround(s,w,"rdis"));
            Assert.That((bool)Call(s,"ActorMoveBlocked",2),Is.True,"APDI4 A0H5/AHtb BPSE both survive dispel.");
            Assert.That(((IDictionary)Field<IDictionary>(s,"actorControls")[2])["native-stun:BPSE:1001"],Is.SameAs(spell));
            Assert.That(Field<IList>(s,"nativeBashes")[0],Is.SameAs(bash));
            Call(s,"AdvanceNativeWeaponProcs",1.01d);Assert.That(Field<IList>(s,"nativeBashes"),Is.Empty);
            Assert.That((bool)Call(s,"ActorMoveBlocked",2),Is.True,"The independently retained spellstun still controls the actor.");
            Call(s,"AdvanceActorControls",3.01d);Assert.That((bool)Call(s,"ActorMoveBlocked",2),Is.False);
        }
        [Test] public void EveryNonemptyItemRuneRewardUsesItsRealConversionAndEmptyDrawsStayEmpty()
        {
            // Independent xorshift seed search: draws1..10 use these literals.
            uint[] seeds={17,9,8,7,6,5,4,3,2,1};
            string[] inventory={"I02H","I06O","I01Y","I01L","I06M","I03M",null,"I0AJ",null,null};
            for(int i=0;i<10;i++)
            {
                var s=Create(out var w,out _);typeof(OriginalSession).GetField("pickupRandom",Hidden).SetValue(s,seeds[i]);
                Pickup(s,AddGround(s,w,"rspl"));
                var found=Inventory(s).HeroSlots.FirstOrDefault(x=>x!=null);
                Assert.That(found?.itemId,Is.EqualTo(inventory[i]),"Source draw"+(i+1));Assert.That(Ground(s),Is.Empty);
            }
        }
        [Test] public void UncommittedItemRunePreparationDoesNotAdvanceRngOrPublishReward()
        {
            var s=Create(out var w,out _);typeof(OriginalSession).GetField("pickupRandom",Hidden).SetValue(s,17u);
            var first=Call(s,"PreparePowerupPickup",Player(s),w.UnitState(1),Inventory(s).Copy(),"rspl");
            Assert.That(Field<OriginalItemActionCode>(first,"code"),Is.EqualTo(OriginalItemActionCode.Success));
            Assert.That(Field<uint>(s,"pickupRandom"),Is.EqualTo(17u));Assert.That(Inventory(s).HeroSlots.All(x=>x==null),Is.True);
            var second=Call(s,"PreparePowerupPickup",Player(s),w.UnitState(1),Inventory(s).Copy(),"rspl");
            Call(s,"CommitPowerupPickup",second);Assert.That(Inventory(s).HeroSlots.First(x=>x!=null).itemId,Is.EqualTo("I02H"));
            Assert.Throws<TargetInvocationException>(()=>Call(s,"CommitPowerupPickup",first),"A stale second transaction cannot duplicate the reward.");
        }
        [Test] public void VampireRunePausesAndRefreshesWithoutDoubleStacking()
        {
            var s=Create(out var w,out _);double damage=s.Snapshot().players[0].combat.attackMinimum;
            Pickup(s,AddGround(s,w,"I07G"));StatusClock(s,w,14);
            w.SetUnitState(1,paused:true);StatusClock(s,w,5);Assert.That(s.Snapshot().players[0].combat.attackMinimum-damage,Is.EqualTo(50));
            w.SetUnitState(1,paused:false);Pickup(s,AddGround(s,w,"vamp"));StatusClock(s,w,1.1);
            Assert.That(s.Snapshot().players[0].combat.attackMinimum-damage,Is.EqualTo(50));
            StatusClock(s,w,14);Assert.That(s.Snapshot().players[0].combat.attackMinimum,Is.EqualTo(damage));
        }
        [Test] public void VampirePowerupAutoAppliesWithBothInventoryBagsFull()
        {
            var s=Create(out var w,out _);for(int i=0;i<12;i++)Assert.That(Inventory(s).TryPickup(Inventory(s).CreateInstance("I007")).Applied,Is.True);
            double before=s.Snapshot().players[0].combat.attackMinimum;long id=AddGround(s,w,"vamp");Pickup(s,id);
            Assert.That(Inventory(s).HeroSlots.Count(x=>x!=null),Is.EqualTo(6));Assert.That(Inventory(s).ServantSlots.Count(x=>x!=null),Is.EqualTo(6));
            Assert.That(s.Snapshot().players[0].combat.attackMinimum-before,Is.EqualTo(50));Assert.That(Ground(s).ContainsKey(id),Is.False);
        }
        [Test] public void PublicHostAdvancePublishesUnownedRuneThroughCurrentWireCodec()
        {
            var s=Create(out var w,out _);for(int i=0;i<42;i++){s.Advance(.05);s.DrainEvents();}
            Phase(s,1,OriginalMatchPhase.Combat);
            for(int i=0;i<900;i++){s.Advance(.05);s.DrainEvents();}
            Assert.That(s.HaltReason,Is.Null);Assert.That(s.Snapshot().groundItems.Length,Is.EqualTo(1));
            var codec=new Arena.OriginalUnitySessionCodec();var view=s.Snapshot();
            byte[] message=codec.EncodeResponse(new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,assignedSlot=1,
                code=OriginalSessionReplyCode.Accepted,acknowledgedSequence=view.players[0].acknowledgedSequence,snapshot=view});
            Assert.That(codec.TryDecodeResponse(message,out var decoded),Is.True);Assert.That(decoded.assignedSlot,Is.EqualTo(1));
            Assert.That(decoded.snapshot.groundItems[0].item.ownerId,Is.Zero);
        }
    }
}

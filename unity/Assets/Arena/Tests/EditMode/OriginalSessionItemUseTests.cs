using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemUseTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        static OriginalWorld World(OriginalSession session) => (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
        static OriginalInventory Inventory(OriginalSession session)
        {
            var player = ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
            return (OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
        }
        static OriginalItemInstance Add(OriginalSession session, string id, int charges = 1, OriginalInventoryBag bag = OriginalInventoryBag.Hero)
        {
            var item = Inventory(session).CreateInstance(id); item.chargesKnown = true; item.charges = charges;
            Assert.That(Inventory(session).TryPickup(item, bag).Applied, Is.True); return item;
        }
        static OriginalSessionCommand Command(OriginalSession session, OriginalItemInstance item, int slot = 0, OriginalInventoryBag bag = OriginalInventoryBag.Hero) =>
            new OriginalSessionCommand { kind = OriginalSessionCommandKind.UseItem,
                sequence = session.Snapshot().players[0].acknowledgedSequence + 1, itemInstanceId = item.instanceId, itemSlot = slot, bag = bag };
        static void Resources(OriginalSession session, double health, double mana)
        { var hero = World(session).UnitState(1); World(session).UpdateProfile(1, hero.profile, health, mana); }

        static OriginalItemInstance EquipReusable(OriginalSession s,string id)
        {
            var candidate=Inventory(s).Copy();var item=candidate.CreateInstance(id);item.chargesKnown=true;item.charges=0;
            Assert.That(candidate.TryPickup(item).Applied,Is.True);
            var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
            Assert.That(typeof(OriginalSession).GetMethod("ApplyEquipmentProfile",Private).Invoke(s,new object[]{player,candidate,World(s).UnitState(1)}),Is.True);
            player.GetType().GetField("inventory").SetValue(player,candidate);return item;
        }

        [Test] public void MeasuredAntiMagicShellRejectsMagicWithoutBecomingUniversalOrPhysicalInvulnerability()
        {
            var s=Create();var w=World(s);var item=Add(s,"I01Y");Resources(s,500,100);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Inventory(s).HeroSlots[0],Is.Null);
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("ApplyNativeTriggeredHit",0,0,w.UnitState(1),200d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(500));
            Call("ApplyNativeTriggeredHit",0,0,w.UnitState(1),40d,OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(w.UnitState(1).health,Is.EqualTo(460));
            Call("ApplyNativeTriggeredHit",0,0,w.UnitState(1),40d,OriginalTriggeredDamageMode.SpellNormal);
            Assert.That(w.UnitState(1).health,Is.LessThan(460));
            Call("AdvanceItemShells",6.01d);double before=w.UnitState(1).health;
            Call("ApplyNativeTriggeredHit",0,0,w.UnitState(1),40d,OriginalTriggeredDamageMode.SpellMagic);
            Assert.That(w.UnitState(1).health,Is.EqualTo(before-32).Within(1e-6));
        }

        [Test] public void LightningItemAcceptsAlliedCarrierAndRunsEightSourcePulsesAfterCasterDeath()
        {
            var s=Create();var w=World(s);var item=EquipReusable(s,"I01B");var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=3000,collisionRadius=8};
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=100000044,ownerSlot=1,sourceHeroEntityId=1,
                rawcode="hfoo",profile=profile,health=3000,position=new OriginalPoint(hero.position.x+150,hero.position.y)}}),Is.True);
            w.AddUnit(9441,0,"hfoo",profile,new OriginalPoint(hero.position.x+300,hero.position.y));
            w.AddUnit(9442,0,"hhou",profile,new OriginalPoint(hero.position.x+350,hero.position.y));
            w.AddUnit(9443,0,"hfoo",profile,new OriginalPoint(hero.position.x+426,hero.position.y));
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=100000044;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-350));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("AdvanceItemShells",1d);Assert.That(w.UnitState(9441).health,Is.EqualTo(2850));
            w.ForceUnitDeath(1);Call("AdvanceItemShells",8.01d);
            Assert.That(w.UnitState(9441).health,Is.EqualTo(1800));Assert.That(w.UnitState(100000044).health,Is.EqualTo(3000));
            Assert.That(w.UnitState(9442).health,Is.EqualTo(3000));Assert.That(w.UnitState(9443).health,Is.EqualTo(3000));
        }

        [Test] public void UpgradedLightningSpendsFourHundredFiftyAndExcludesItsEnemyCarrier()
        {
            var s=Create();var w=World(s);var item=EquipReusable(s,"I07M");var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=3000,collisionRadius=8};
            w.AddUnit(9450,0,"hfoo",profile,new OriginalPoint(hero.position.x+100,hero.position.y));
            w.AddUnit(9451,0,"hfoo",profile,new OriginalPoint(hero.position.x+200,hero.position.y));
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9450;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-450));Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            typeof(OriginalSession).GetMethod("AdvanceItemShells",Private).Invoke(s,new object[]{8.01d});
            Assert.That(w.UnitState(9450).health,Is.EqualTo(3000));Assert.That(w.UnitState(9451).health,Is.EqualTo(1400));
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
        }

        [Test] public void MeasuredRegenPotionAddsTenSmallManaPulsesAndDamageCancelsTheRemainder()
        {
            var s=Create();var w=World(s);var item=Add(s,"I0AJ");Resources(s,500,10);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);Assert.That(w.UnitState(1).mana,Is.EqualTo(10));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("AdvanceItemRegeneration",.99d);Assert.That(w.UnitState(1).mana,Is.EqualTo(10));
            Call("AdvanceItemRegeneration",9.01d);Assert.That(w.UnitState(1).mana,Is.EqualTo(11).Within(1e-9));
            Assert.That(w.UnitState(1).health,Is.EqualTo(500));
            Call("AdvanceItemRegeneration",5d);Assert.That(w.UnitState(1).mana,Is.EqualTo(11).Within(1e-9));
            var second=Add(s,"I0AJ");Assert.That(s.Apply(0,Command(s,second)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call("AdvanceItemRegeneration",1d);Assert.That(w.UnitState(1).mana,Is.EqualTo(11.1).Within(1e-9));
            Call("ApplyResolvedUnitHit",0,0,w.UnitState(1),1d,null);Call("AdvanceItemRegeneration",10d);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(11.1).Within(1e-9));Assert.That(w.UnitState(1).health,Is.EqualTo(499));
        }

        [Test] public void ChainRootVisitsNineDistinctEnemiesAndKeepsRunningAfterCasterDeath()
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"H024"});
            var w=World(s);var item=EquipReusable(s,"I09X");var hero=w.UnitState(1);
            for(int i=0;i<10;i++)w.AddUnit(9400+i,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(hero.position.x+150+i*90,hero.position.y));
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9400;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-400));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("AdvanceItemEnsnare",.15d);Assert.That(Call("ActorMoveBlocked",9400),Is.True);
            w.ApplyUnitDamage(1,hero.health+1);
            Call("AdvanceItemEnsnare",3d);
            for(int i=0;i<9;i++){Assert.That(Call("ActorMoveBlocked",9400+i),Is.True);Assert.That(Call("ActorCastBlocked",9400+i),Is.False);}
            Assert.That(Call("ActorMoveBlocked",9409),Is.False);
            Call("AdvanceActorControls",5.01d);Assert.That(Call("ActorMoveBlocked",9400),Is.False);
        }

        [Test] public void ChainHelperTimedLifeResumesFinalSeriesAfterCasterDeath()
        {
            var s=Create();var w=World(s);var hero=w.UnitState(1);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Private).GetValue(s);
            object Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Private).Invoke(target,args);
            typeof(OriginalMatch).GetProperty("Round").SetValue(match,30);Call(match,"StartCombat");Call(s,"CollectEvents");
            w.SetUnitState(1,paused:true,invulnerable:true);
            for(int i=0;i<101;i++)s.Advance(.05);
            int boss=OriginalWorld.EnemyEntityId(match.FinalBossEntityId);w.ApplyUnitDamage(boss,w.UnitState(boss).profile.maxHealth*.26);
            bool started=false,resumed=false;
            for(int i=0;i<1300;i++)
            {
                s.Advance(.05);Assert.That(s.HaltReason,Is.Null);
                foreach(var add in s.Snapshot().enemies.Where(e=>e.finalAdd))
                {w.ForceUnitDeath(OriginalWorld.EnemyEntityId(add.entityId));Assert.That(s.ReportEnemyKilled(add.entityId,false),Is.True);}
                if(!started&&w.Clock>=57)
                {
                    w.AddUnit(9470,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(-2500,-2500));
                    // Trusted effect seam isolates helper lifetime from command mana/range.
                    Call(s,"BeginItemEnsnare",w.UnitState(1),9470);w.ForceUnitDeath(1);started=true;
                }
                if(started&&match.Phase==OriginalMatchPhase.Combat){resumed=true;break;}
                if(w.Clock>=59)break;
            }
            Assert.That(resumed,Is.True);Assert.That(w.Clock,Is.LessThan(59));Assert.That(match.FinalStage,Is.EqualTo(1));
            Assert.That(s.Snapshot().effects.Any(e=>e.abilityId=="n062"||e.abilityId=="h016"),Is.False);
        }

        [Test] public void FlameBootsKeepTheSourceFirstGroupHandleStampAndExpireHasteAtFiveSeconds()
        {
            var s=Create();var w=World(s);var item=EquipReusable(s,"I0B2");var hero=w.UnitState(1);
            for(int i=0;i<2;i++)w.AddUnit(9410+i,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=8},new OriginalPoint(hero.position.x+100,hero.position.y+i*40));
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Assert.That(Call("FlameBootMovementBonus",1),Is.EqualTo(.2));
            Call("AdvanceFlameBoots",.2d);Call("AdvanceFlameBoots",.5d);
            Assert.That(w.UnitState(9410).health,Is.EqualTo(977.5));Assert.That(w.UnitState(9411).health,Is.EqualTo(1000));
            w.ApplyUnitDamage(1,hero.health+1);Call("AdvanceFlameBoots",.5d);
            Assert.That(w.UnitState(9410).health,Is.EqualTo(955));
            Call("AdvanceFlameBoots",4d);Assert.That(Call("FlameBootMovementBonus",1),Is.EqualTo(0));
            Assert.That(w.UnitState(9410).health,Is.EqualTo(865));Assert.That(w.UnitState(9411).health,Is.EqualTo(1000));
        }

        [Test] public void VoidItemBurnsCurrentManaBeforeUniversalDamageAndSilencesCastingOnly()
        {
            var s=Create();var w=World(s);var item=EquipReusable(s,"I0AL");var hero=w.UnitState(1);
            w.AddUnit(9271,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=2000,maxMana=500,collisionRadius=16},new OriginalPoint(hero.position.x+200,hero.position.y));
            var enemy=w.UnitState(9271);w.UpdateProfile(9271,enemy.profile,2000,120);
            var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9271;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(9271).mana,Is.Zero);Assert.That(w.UnitState(9271).health,Is.EqualTo(1820));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-125));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Assert.That(Call("ActorCastBlocked",9271),Is.True);Assert.That(Call("ActorWeaponBlocked",9271),Is.False);
            Call("AdvanceActorControls",3.01d);Assert.That(Call("ActorCastBlocked",9271),Is.False);
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
        }

        [Test] public void SilenceItemTargetsOrganicEnemiesInAuthoredRadiusAndLeavesAlliesAndMechanicalFree()
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"H024"});
            var w=World(s);var item=EquipReusable(s,"I04B");var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=16};
            w.AddUnit(9271,0,"hfoo",profile,new OriginalPoint(hero.position.x+200,hero.position.y));
            w.AddUnit(9272,0,"hmtt",profile,new OriginalPoint(hero.position.x+280,hero.position.y));
            w.AddUnit(9273,0,"hfoo",profile,new OriginalPoint(hero.position.x+400,hero.position.y));
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Assert.That(Call("ActorCastBlocked",9271),Is.True);Assert.That(Call("ActorWeaponBlocked",9271),Is.True);
            foreach(int id in new[]{1,9272,9273})Assert.That(Call("ActorCastBlocked",id),Is.False);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-250));
            Call("AdvanceActorControls",4.01d);Assert.That(Call("ActorCastBlocked",9271),Is.False);
        }

        [Test] public void FingerItemRejectsInvalidTargetsBeforeDebitAndPublishesNativeDamageOnce()
        {
            var s=Create();var w=World(s);var candidate=Inventory(s).Copy();var item=candidate.CreateInstance("I026");
            item.chargesKnown=true;item.charges=0;Assert.That(candidate.TryPickup(item).Applied,Is.True);
            var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
            Assert.That(typeof(OriginalSession).GetMethod("ApplyEquipmentProfile",Private).Invoke(s,new object[]{player,candidate,w.UnitState(1)}),Is.True);
            player.GetType().GetField("inventory").SetValue(player,candidate);var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=16};
            w.AddUnit(9201,0,"hfoo",profile,new OriginalPoint(hero.position.x+300,hero.position.y));
            w.AddUnit(9202,0,"hmtt",profile,new OriginalPoint(hero.position.x+400,hero.position.y));
            w.AddUnit(9203,0,"hfoo",profile,new OriginalPoint(hero.position.x+700,hero.position.y));
            OriginalSessionCommand Use(int target){var c=Command(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=target;return c;}
            Assert.That(s.Snapshot().players[0].itemUses[0].targetMode,Is.EqualTo(OriginalAbilityTargetMode.Unit));
            foreach(int invalid in new[]{1,9202,999999})
            {Assert.That(s.Apply(0,Use(invalid)),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana));}
            Assert.That(s.Apply(0,Use(9203)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana));Assert.That(w.UnitState(9203).health,Is.EqualTo(2000));
            var accepted=Use(9201);Assert.That(s.Apply(0,accepted),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(9201).health,Is.EqualTo(1725));Assert.That(w.UnitState(1).mana,Is.EqualTo(hero.mana-80));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            Assert.That(s.Apply(0,accepted),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(w.UnitState(9201).health,Is.EqualTo(1725));
            var codec=new OriginalUnitySessionCodec();var snapshot=s.Snapshot();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,
                assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _),Is.True);
        }

        [Test] public void ReusableWindWalkDebitsManaFadesThenArmsOneReleasedStrike()
        {
            var s=Create();var w=World(s);var item=Add(s,"I0AP",0);double mana=w.UnitState(1).mana;
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Assert.That(s.Snapshot().players[0].itemUses[0].requiresCharge,Is.False);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(mana-70));
            Assert.That(Call("ItemStatusMovementBonus",1),Is.EqualTo(.1));
            Call("AdvanceItemStatuses",.29d);Assert.That(Call("ItemInvisibilityActive",1),Is.False);
            Call("AdvanceItemStatuses",.02d);Assert.That(Call("ItemInvisibilityActive",1),Is.True);
            Call("ArmItemWindWalkStrike",1);Call("RevealItemInvisibility",1);
            Assert.That(Call("ItemStatusMovementBonus",1),Is.Zero);
            Assert.That(Call("ConsumeItemWindWalkStrike",1),Is.EqualTo(50));
            Assert.That(Call("ConsumeItemWindWalkStrike",1),Is.Zero);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void WindWalkManaRejectionPreservesItemAndNeverCreatesAStatus()
        {
            var s=Create();var item=Add(s,"I013",0);Resources(s,100,69);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(69));
            Assert.That(typeof(OriginalSession).GetMethod("ItemInvisibilityActive",Private).Invoke(s,new object[]{1}),Is.False);
        }

        [Test] public void WidowSourceSweepVisitsEachVictimOnceAndWeaponRefreshKeepsTimerPhase()
        {
            var s=Create();var w=World(s);var item=Add(s,"I013",0);var hero=w.UnitState(1);
            w.AddUnit(9101,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=8},new OriginalPoint(hero.position.x+80,hero.position.y));
            w.AddUnit(9102,0,"hhou",new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=8},new OriginalPoint(hero.position.x-80,hero.position.y));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            void Tick(double duration){while(duration>1e-9){double step=Math.Min(.05,duration);w.Advance(step);Call("AdvanceItemStatuses",step);duration-=step;}}
            Tick(.1);
            Assert.That(Call("ItemArmorBonus",9101),Is.EqualTo(-4));Assert.That(Call("ItemArmorBonus",9102),Is.Zero);
            Tick(.5);Assert.That(Call("ItemArmorBonus",9101),Is.EqualTo(-4));
            Call("ObserveWidowWeapon",1,w.UnitState(9101),10d,true);Assert.That(Call("ItemArmorBonus",9101),Is.EqualTo(-8));
            Tick(6.49);Assert.That(Call("ItemArmorBonus",9101),Is.EqualTo(-8));
            Tick(.02);Assert.That(Call("ItemArmorBonus",9101),Is.Zero);
            Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void AreaRestoreConsumesOnlyOnBenefitAndExcludesMechanicalEnemiesAndDistantAllies()
        {
            var s=Create();var w=World(s);var item=Add(s,"I01L",2);var hero=w.UnitState(1);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(2));
            var profile=new OriginalWorldUnitProfile{maxHealth=2000,maxMana=1000,collisionRadius=16};
            foreach(var row in new[]{(100000071,"hfoo",200d),(100000072,"hmtt",300d),(100000073,"hfoo",1100d)})
                Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=row.Item1,ownerSlot=1,sourceHeroEntityId=1,
                    rawcode=row.Item2,profile=profile,position=new OriginalPoint(hero.position.x+row.Item3,hero.position.y),health=100,mana=0}}),Is.True);
            w.AddUnit(1001,0,"hfoo",profile,new OriginalPoint(hero.position.x+400,hero.position.y));w.UpdateProfile(1001,profile,100,0);
            var command=Command(s,item);
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(100000071).health,Is.EqualTo(600));Assert.That(w.UnitState(100000071).mana,Is.EqualTo(250));
            foreach(int id in new[]{100000072,100000073,1001})
            { Assert.That(w.UnitState(id).health,Is.EqualTo(100),id.ToString());Assert.That(w.UnitState(id).mana,Is.Zero); }
            Assert.That(w.UnitState(1).health,Is.EqualTo(hero.health));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));Assert.That(s.HaltReason,Is.Null);
        }
        [Test] public void NativeInvulnerabilityPotionExpiresWithoutClearingIndependentBaseProtection()
        {
            var s=Create();var w=World(s);var item=Add(s,"I06O");
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).invulnerable,Is.True);Assert.That(Inventory(s).HeroSlots[0],Is.Null);
            w.SetUnitState(1,paused:true);
            typeof(OriginalSession).GetMethod("AdvanceItemStatuses",Private).Invoke(s,new object[]{5d});
            Assert.That(w.UnitState(1).invulnerable,Is.True,"Pause freezes the native buff clock by host policy");
            w.SetUnitState(1,paused:false,invulnerable:true);
            typeof(OriginalSession).GetMethod("AdvanceItemStatuses",Private).Invoke(s,new object[]{3.01d});
            Assert.That(w.UnitState(1).invulnerable,Is.True,"Base/duel protection must survive item expiry");
            w.SetUnitState(1,invulnerable:false);Assert.That(w.UnitState(1).invulnerable,Is.False);
        }
        [Test] public void NativeInvisibilityFadesForTwoSecondsAndPreservesFriendlyVisibility()
        {
            var s=Create();var w=World(s);var item=Add(s,"I06M");
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("AdvanceItemStatuses",1.99d);Assert.That(Call("ItemInvisibilityActive",1),Is.False);
            Call("AdvanceItemStatuses",.02d);Assert.That(Call("ItemInvisibilityActive",1),Is.True);
            Assert.That(Call("CanSeeForCombat",1,w.UnitState(1)),Is.True);Assert.That(Call("CanSeeForCombat",0,w.UnitState(1)),Is.False);
            Assert.That(w.UnitState(1).hidden,Is.False);Assert.That(w.TryMove(1,new OriginalPoint(500,1000)),Is.True);
            Assert.That(Call("ItemInvisibilityActive",1),Is.True,"Movement does not reveal");
            Call("RevealItemInvisibility",1);Assert.That(Call("ItemInvisibilityActive",1),Is.False);
            Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void PublicOrdersRevealInvisibilityOnlyAfterAnAcceptedAttackAndRejectUnseenTargets()
        {
            var s=Create();var w=World(s);var item=Add(s,"I06M");
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
            Call("AdvanceItemStatuses",2.01d);
            var hero=w.UnitState(1);
            var profile=new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16};
            w.AddUnit(1001,0,"hfoo",profile,new OriginalPoint(hero.position.x+150,hero.position.y));
            OriginalSessionReplyCode Send(OriginalSessionCommandKind kind,int target=0)=>s.Apply(0,new OriginalSessionCommand{
                kind=kind,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
                targetKind=target==0?OriginalWorldTargetKind.None:OriginalWorldTargetKind.Unit,targetId=target,
                x=hero.position.x+30,y=hero.position.y});
            Assert.That(Send(OriginalSessionCommandKind.Move),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Call("ItemInvisibilityActive",1),Is.True);
            Assert.That(Send(OriginalSessionCommandKind.AttackTarget,999999),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Call("ItemInvisibilityActive",1),Is.True,"Rejected attack must not reveal its source");
            var invisRule=Call("NativeItemAction","I06M");Call("ApplyNativeItemStatus",1001,invisRule);Call("AdvanceItemStatuses",2.01d);
            Assert.That(Send(OriginalSessionCommandKind.AttackTarget,1001),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Call("ItemInvisibilityActive",1),Is.True);
            Call("RevealItemInvisibility",1001);
            Assert.That(Send(OriginalSessionCommandKind.AttackTarget,1001),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Call("ItemInvisibilityActive",1),Is.False);Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void UnitySwordsRestoreBothResourcesWithReusableZeroChargeAndExactAllyFilter()
        {
            foreach (string id in new[]{"I02E","I03Z"})
            {
                var s=Create();var item=Add(s,id,0);var w=World(s);
                var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
                Assert.That(typeof(OriginalSession).GetMethod("ApplyEquipmentProfile",Private).Invoke(s,new object[]{player,Inventory(s),w.UnitState(1)}),Is.True);
                Resources(s,100,0);var hero=w.UnitState(1);
                var profile=new OriginalWorldUnitProfile{maxHealth=2000,maxMana=1000,collisionRadius=16};
                foreach(var row in new[]{(100000051,200d),(100000052,801d)})
                    Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=row.Item1,ownerSlot=1,sourceHeroEntityId=1,
                        rawcode="hfoo",profile=profile,position=new OriginalPoint(hero.position.x+row.Item2,hero.position.y),health=100,mana=0}}),Is.True);
                w.AddUnit(1003,0,"hfoo",profile,new OriginalPoint(hero.position.x+300,hero.position.y));w.UpdateProfile(1003,profile,100,0);
                Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                double amount=id=="I02E"?275:550;
                Assert.That(w.UnitState(1).health,Is.EqualTo(Math.Min(hero.profile.maxHealth,100+amount)));
                Assert.That(w.UnitState(1).mana,Is.EqualTo(Math.Min(hero.profile.maxMana,amount)));
                Assert.That(w.UnitState(100000051).health,Is.EqualTo(100+amount));Assert.That(w.UnitState(100000051).mana,Is.EqualTo(amount));
                Assert.That(w.UnitState(100000052).health,Is.EqualTo(100));Assert.That(w.UnitState(1003).health,Is.EqualTo(100));
                Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
                Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
                Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
                Assert.That((double)typeof(OriginalSession).GetMethod("ItemArmorBonus",Private).Invoke(s,new object[]{1}),Is.EqualTo(id=="I02E"?14:24));
            }
        }

        [Test] public void DistinctArmorBuffsCoexistWhileTheSameSwordBuffReplacesAndExpires()
        {
            var s=Create();var sword=Add(s,"I02E",0);var upgraded=Add(s,"I03Z",0);var scroll=Add(s,"I02H");
            Assert.That(s.Apply(0,Command(s,sword)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0,Command(s,scroll,2)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            double Armor()=>(double)typeof(OriginalSession).GetMethod("ItemArmorBonus",Private).Invoke(s,new object[]{1});
            Assert.That(Armor(),Is.EqualTo(114));
            Assert.That(s.Apply(0,Command(s,upgraded,1)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Armor(),Is.EqualTo(124));
            typeof(OriginalSession).GetMethod("AdvanceItemArmor",Private).Invoke(s,new object[]{6d});
            Assert.That(Armor(),Is.EqualTo(24));
            typeof(OriginalSession).GetMethod("AdvanceItemArmor",Private).Invoke(s,new object[]{10d});
            Assert.That(Armor(),Is.Zero);
        }

        [Test] public void SourceHealingCurseReversesHealthButDoesNotReverseSwordManaRestoration()
        {
            var s=Create();var item=Add(s,"I02E",0);var w=World(s);
            var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
            Assert.That(typeof(OriginalSession).GetMethod("ApplyEquipmentProfile",Private).Invoke(s,new object[]{player,Inventory(s),w.UnitState(1)}),Is.True);
            ((IDictionary)player.GetType().GetField("auxiliaryAbilities").GetValue(player))["A19P"]=1;
            Resources(s,600,0);
            Assert.That(s.Apply(0,Command(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).health,Is.EqualTo(325));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(Math.Min(275,w.UnitState(1).profile.maxMana)));
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardPointMetadataAndCoordinatesReachTheProductionCodec(string id)
        {
            var s=Create();var item=Add(s,id);var view=s.Snapshot();
            Assert.That(view.players[0].itemUses.Single().targetMode,Is.EqualTo(OriginalAbilityTargetMode.UnitOrPoint));
            var codec=new OriginalUnitySessionCodec();
            var response=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,assignedSlot=1,
                acknowledgedSequence=view.players[0].acknowledgedSequence,code=OriginalSessionReplyCode.Accepted,snapshot=view};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out var decoded),Is.True);
            Assert.That(decoded.snapshot.players[0].itemUses.Single().targetMode,Is.EqualTo(OriginalAbilityTargetMode.UnitOrPoint));
            var command=Command(s,item);command.x=300;command.y=1000;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out var copy),Is.True);
            Assert.That(copy.x,Is.EqualTo(300));Assert.That(copy.y,Is.EqualTo(1000));
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardPointUseDebitsOneChargeAtTheSelectedGroundPoint(string id)
        {
            var s=Create();var item=Add(s,id,2);var actor=World(s).UnitState(1);
            var c=Command(s,item);c.x=actor.position.x+250;c.y=actor.position.y-100;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var ward=World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
            Assert.That(ward.position.x,Is.EqualTo(c.x));Assert.That(ward.position.y,Is.EqualTo(c.y));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(ward.rawcode,Is.EqualTo(id=="I021"?"ohwd":"o00J"));
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(actor.mana));
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(World(s).Snapshot().units.Count(x=>x.kind==OriginalWorldUnitKind.Summon),Is.EqualTo(1));
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardZeroCoordinatesRemainAnExplicitGroundPoint(string id)
        {
            var s=Create();var item=Add(s,id);Assert.That(World(s).Relocate(1,new OriginalPoint(250,0)),Is.True);
            var c=Command(s,item);c.x=0;c.y=0;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var ward=World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
            Assert.That(ward.position.x,Is.Zero);Assert.That(ward.position.y,Is.Zero);
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardMatchingOwnedItemTargetSelectsTheSourceSelfBranch(string id)
        {
            var s=Create();var item=Add(s,id);var actor=World(s).UnitState(1);
            var c=Command(s,item);c.targetItemInstanceId=item.instanceId;c.x=900000;c.y=900000;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var ward=World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
            double dx=ward.position.x-actor.position.x,dy=ward.position.y-actor.position.y;
            Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(64*64));
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardFarPointWaitsForItemControlThenApproachesWithoutEarlySpending(string id)
        {
            var s=Create();var item=Add(s,id);var actor=World(s).UnitState(1);
            Assert.That(typeof(OriginalSession).GetMethod("SetActorControl",Private).Invoke(s,
                new object[]{1,"ward-test",OriginalActorControlMask.Item,.5,true,false}),Is.True);
            var c=Command(s,item);c.x=actor.position.x+501;c.y=actor.position.y;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(((IDictionary)typeof(OriginalSession).GetField("pendingItemOrders",Private).GetValue(s)).Count,Is.EqualTo(1));
            Assert.That(World(s).Snapshot().units.Any(x=>x.kind==OriginalWorldUnitKind.Summon),Is.False);
            Assert.That(World(s).UnitState(1).mana,Is.EqualTo(actor.mana));
            Assert.That(s.Snapshot().players[0].itemUses.Single().cooldownRemaining,Is.Zero);
            for(int i=0;i<10;i++)s.Advance(.05);
            Assert.That(((IDictionary)typeof(OriginalSession).GetField("pendingItemOrders",Private).GetValue(s)).Count,Is.Zero);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            for(int i=0;i<60;i++)s.Advance(.05);
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);
            Assert.That(World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon).position.x,Is.EqualTo(c.x));
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardNonfiniteDirectHostPointRejectsBeforeMutation(string id)
        {
            var s=Create();var item=Add(s,id);var c=Command(s,item);c.x=double.NaN;c.y=1000;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Assert.That(World(s).Snapshot().units.Any(x=>x.kind==OriginalWorldUnitKind.Summon),Is.False);
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardWrongOrForeignItemTargetCannotConsumeACharge(string id)
        {
            var s=Create();var item=Add(s,id);var wrong=Add(s,"I04J");
            var ground=(IDictionary)typeof(OriginalSession).GetField("groundItems",Private).GetValue(s);
            ground.Add(999999L,new OriginalGroundItemView{item=new OriginalItemInstance{instanceId=999999L,
                itemId=id,ownerId=2,chargesKnown=true,charges=1},position=World(s).UnitState(1).position});
            foreach(long target in new[]{wrong.instanceId,999999L})
            {
                var c=Command(s,item);c.targetItemInstanceId=target;
                Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
                Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
                Assert.That(World(s).Snapshot().units.Any(x=>x.kind==OriginalWorldUnitKind.Summon),Is.False);
            }
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardBoundaryPointAndQueuedCopyKeepTheOriginalCoordinates(string id)
        {
            var s=Create();var item=Add(s,id);var actor=World(s).UnitState(1);
            Assert.That(typeof(OriginalSession).GetMethod("SetActorControl",Private).Invoke(s,
                new object[]{1,"ward-test",OriginalActorControlMask.Item,.5,true,false}),Is.True);
            var c=Command(s,item);c.x=actor.position.x+500;c.y=actor.position.y;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            c.x=-900000;c.y=-900000;
            typeof(OriginalSession).GetMethod("AdvanceActorControls",Private).Invoke(s,new object[]{.51d});
            typeof(OriginalSession).GetMethod("AdvanceQueuedItems",Private).Invoke(s,null);
            var ward=World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
            Assert.That(ward.position.x,Is.EqualTo(actor.position.x+500));Assert.That(ward.position.y,Is.EqualTo(actor.position.y));
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardQueuedMatchingItemTargetKeepsItsSelfOverride(string id)
        {
            var s=Create();var item=Add(s,id);var actor=World(s).UnitState(1);
            Assert.That(typeof(OriginalSession).GetMethod("SetActorControl",Private).Invoke(s,
                new object[]{1,"ward-test",OriginalActorControlMask.Item,.5,true,false}),Is.True);
            var c=Command(s,item);c.targetItemInstanceId=item.instanceId;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            c.targetItemInstanceId=0;c.x=-900000;c.y=-900000;
            typeof(OriginalSession).GetMethod("AdvanceActorControls",Private).Invoke(s,new object[]{.51d});
            typeof(OriginalSession).GetMethod("AdvanceQueuedItems",Private).Invoke(s,null);
            var ward=World(s).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
            double dx=ward.position.x-actor.position.x,dy=ward.position.y-actor.position.y;
            Assert.That(dx*dx+dy*dy,Is.LessThanOrEqualTo(64*64));
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardMatchingOwnedSiblingItemAlsoUsesSelfWithoutConsumingTheTarget(string id)
        {
            var s=Create();var activated=Add(s,id);var target=Add(s,id,1,OriginalInventoryBag.Servant);
            var c=Command(s,activated);c.targetItemInstanceId=target.instanceId;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0],Is.Null);
            Assert.That(Inventory(s).ServantSlots[0].instanceId,Is.EqualTo(target.instanceId));
            Assert.That(Inventory(s).ServantSlots[0].charges,Is.EqualTo(1));
        }

        [TestCase("I021"), TestCase("I094")]
        public void WardUseWithoutTheOptionalWorldReturnsNotReadyWithoutMutation(string id)
        {
            var s=Create();var item=Add(s,id);
            typeof(OriginalSession).GetField("world",Private).SetValue(s,null);
            var c=Command(s,item);c.x=250;c.y=1000;
            Assert.That(s.Apply(0,c),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
        }

        [Test] public void WardItemsConsumeOnceHealOnlyNearbyAlliesAndExpireIndependently()
        {
            foreach (string id in new[] { "I021", "I094" })
            {
                var session=Create();var item=Add(session,id);Resources(session,100,0);
                var command=Command(session,item);
                command.targetItemInstanceId=item.instanceId;
                Assert.That(session.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                var ward=World(session).Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Summon);
                Assert.That(ward.rawcode,Is.EqualTo(id=="I021"?"ohwd":"o00J"));
                Assert.That(ward.health,Is.EqualTo(5));Assert.That(ward.profile.moveSpeed,Is.Zero);
                Assert.That(Inventory(session).HeroSlots[0],Is.Null);
                Assert.That(session.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
                var profile=new OriginalWorldUnitProfile{maxHealth=420,maxMana=100,moveSpeed=0,collisionRadius=16};
                World(session).AddUnit(1001,0,"hfoo",profile,new OriginalPoint(ward.position.x+100,ward.position.y));
                Assert.That(World(session).TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=100000050,
                    ownerSlot=1,sourceHeroEntityId=1,rawcode="hfoo",profile=profile,
                    position=new OriginalPoint(ward.position.x+600,ward.position.y),health=100,mana=0}}),Is.True);
                typeof(OriginalSession).GetMethod("AdvanceItemAuras",Private).Invoke(session,null);
                string method=id=="I021"?"ItemAuraHealthRegen":"ItemAuraManaRegen";
                double maximum=id=="I021"?631:145;
                Assert.That((double)typeof(OriginalSession).GetMethod(method,Private).Invoke(session,new object[]{1,maximum}),
                    Is.EqualTo(maximum*.03).Within(.000001));
                foreach(int excluded in new[]{1001,100000050})
                    Assert.That((double)typeof(OriginalSession).GetMethod(method,Private).Invoke(session,new object[]{excluded,100.0}),Is.Zero);
                var definition=((OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Private).GetValue(session)).Unit(ward.rawcode);
                Assert.That((double)typeof(OriginalSession).GetMethod("EnemyArmor",Private).Invoke(session,new object[]{definition,ward.entityId}),Is.Zero);
                World(session).ForceUnitDeath(1);
                typeof(OriginalSession).GetMethod("AdvanceItemSummons",Private).Invoke(session,new object[]{id=="I021"?30.0:15.0});
                Assert.That(World(session).UnitState(ward.entityId).health,Is.Zero);
                Assert.That(session.HaltReason,Is.Null);
            }
        }

        [Test] public void NativeSummonUsePublishesDetachedOwnedUnitAndReplayCannotDuplicate()
        {
            foreach (string id in new[] { "I01A", "I01M", "I07E", "I07K" })
            {
                var session = Create(); var item = Add(session, id); var command = Command(session, item);
                Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted), id);
                var summon = World(session).Snapshot().units.Single(x => x.kind == OriginalWorldUnitKind.Summon);
                Assert.That(summon.ownerSlot, Is.EqualTo(1)); Assert.That(summon.sourceHeroEntityId, Is.EqualTo(1));
                Assert.That(summon.health, Is.EqualTo(summon.profile.maxHealth));
                Assert.That(Inventory(session).HeroSlots[0], Is.Null);
                Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
                Assert.That(World(session).Snapshot().units.Count(x => x.kind == OriginalWorldUnitKind.Summon), Is.EqualTo(1));
                var snapshot = session.Snapshot();
                var codec = new OriginalUnitySessionCodec();
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot,
                    snapshot = snapshot, assignedSlot = 1, code = OriginalSessionReplyCode.Accepted,
                    acknowledgedSequence = snapshot.players[0].acknowledgedSequence }), out _), Is.True);
                World(session).ForceUnitDeath(1);
                Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = command.sequence + 1, kind = OriginalSessionCommandKind.Move,
                    actorEntityId = summon.entityId, x = summon.position.x + 100, y = summon.position.y }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(session.HaltReason, Is.Null);
            }
        }

        [Test] public void NativeSummonLifetimeHonorsPauseAndExpiresWithoutHeroReward()
        {
            var session = Create(); var item = Add(session, "I01A");
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var summon = World(session).Snapshot().units.Single(x => x.kind == OriginalWorldUnitKind.Summon);
            var advance = typeof(OriginalSession).GetMethod("AdvanceItemSummons", Private);
            World(session).SetUnitState(summon.entityId, paused: true);
            advance.Invoke(session, new object[] { 50.0 }); Assert.That(World(session).UnitState(summon.entityId).health, Is.GreaterThan(0));
            World(session).SetUnitState(summon.entityId, paused: false);
            var gold = Inventory(session).Gold; var xp = session.Snapshot().players[0].experience;
            advance.Invoke(session, new object[] { 45.0 });
            Assert.That(World(session).UnitState(summon.entityId).health, Is.Zero);
            Assert.That(Inventory(session).Gold, Is.EqualTo(gold)); Assert.That(session.Snapshot().players[0].experience, Is.EqualTo(xp));
            advance.Invoke(session, new object[] { 45.0 }); Assert.That(session.HaltReason, Is.Null);
        }

        [Test] public void MeasuredSparseSummonsPublishAllUnitsAndReusableBookKeepsItsStats()
        {
            foreach (var id in new[] { "I01D", "I01J", "I02G" })
            {
                var session = Create(); var world = World(session);
                var player = ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
                var candidate = Inventory(session).Copy(); var item = candidate.CreateInstance(id);
                Assert.That(candidate.TryPickup(item).Applied, Is.True);
                Assert.That((bool)typeof(OriginalSession).GetMethod("ApplyEquipmentProfile", Private).Invoke(session,
                    new[] { player, candidate, (object)world.UnitState(1) }), Is.True, id);
                player.GetType().GetField("inventory").SetValue(player, candidate);
                var before = world.UnitState(1);
                Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted), id);
                var summons = world.Snapshot().units.Where(x => x.kind == OriginalWorldUnitKind.Summon).ToArray();
                Assert.That(summons.Length, Is.EqualTo(id == "I01J" ? 4 : 1));
                Assert.That(summons.All(x => x.health == x.profile.maxHealth && x.health > 0), Is.True);
                if (id == "I02G")
                {
                    Assert.That(Inventory(session).HeroSlots[0].itemId, Is.EqualTo(id));
                    Assert.That(world.UnitState(1).mana, Is.EqualTo(before.mana - 125));
                    Assert.That(world.UnitState(1).profile.maxMana, Is.EqualTo(before.profile.maxMana));
                }
                else Assert.That(Inventory(session).HeroSlots[0], Is.Null);
                Assert.That(session.HaltReason, Is.Null);
            }
        }

        [Test] public void SummonRejectedPreflightPreservesChargeVitalsCooldownAndIdentityCounter()
        {
            var session = Create(); var item = Add(session, "I01A", 99); var other = Add(session, "I01A");
            var counter = typeof(OriginalSession).GetField("nextItemSummonId", Private);
            counter.SetValue(session, OriginalWorld.LastSummonEntityId + 1);
            var before = World(session).Snapshot(); var hero = World(session).UnitState(1);
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(99));
            Assert.That(World(session).Snapshot().revision, Is.EqualTo(before.revision));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(hero.health));
            Assert.That(World(session).UnitState(1).mana, Is.EqualTo(hero.mana));
            Assert.That(session.Snapshot().players[0].itemUses[0].cooldownRemaining, Is.Zero);
            Assert.That(counter.GetValue(session), Is.EqualTo(OriginalWorld.LastSummonEntityId + 1));
            counter.SetValue(session, OriginalWorld.FirstSummonEntityId);
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(0, Command(session, other, 1)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(98));
            Assert.That(Inventory(session).HeroSlots[1].charges, Is.EqualTo(1));
            Assert.That(World(session).Snapshot().units.Single(x => x.kind == OriginalWorldUnitKind.Summon).entityId,
                Is.EqualTo(OriginalWorld.FirstSummonEntityId));
        }

        [Test] public void ArmorScrollAppliesAtFullVitalsOnceWithoutHealingAndExpiresReversibly()
        {
            var session = Create(); var item = Add(session, "I02H");
            var before = World(session).UnitState(1);
            var method = typeof(OriginalSession).GetMethod("ItemArmorBonus", Private);
            double Bonus() => (double)method.Invoke(session, new object[] { 1 });
            Assert.That(session.Snapshot().players[0].itemUses.Single().code, Is.EqualTo(OriginalItemUseCode.Ready));
            var command = Command(session, item);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Bonus(), Is.EqualTo(100));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(before.health));
            Assert.That(World(session).UnitState(1).mana, Is.EqualTo(before.mana));
            Assert.That(Inventory(session).HeroSlots[0], Is.Null);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            var advance = typeof(OriginalSession).GetMethod("AdvanceItemArmor", Private);
            advance.Invoke(session, new object[] { 5.9 }); Assert.That(Bonus(), Is.EqualTo(100));
            advance.Invoke(session, new object[] { .1 }); Assert.That(Bonus(), Is.Zero);
            Assert.That(session.HaltReason, Is.Null);
        }

        [Test] public void ArmorScrollSecondCopyCooldownAndInvalidActorDoNotConsume()
        {
            var session = Create(); var item = Add(session, "I02H", 99); var second = Add(session, "I02H");
            var wrong = Command(session, item); wrong.actorEntityId = 2;
            Assert.That(session.Apply(0, wrong), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(99));
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(0, Command(session, second, 1)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(98));
            Assert.That(Inventory(session).HeroSlots[1].charges, Is.EqualTo(1));
        }

        [Test] public void NativePotionUseRestoresResourceRetiresItemAndReplayCannotRestoreAgain()
        {
            var session = Create(); var item = Add(session, "I03L"); Resources(session, 100, 0);
            var view = session.Snapshot().players[0].itemUses.Single();
            Assert.That(view.code, Is.EqualTo(OriginalItemUseCode.Ready));
            var command = Command(session, item);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(400));
            Assert.That(World(session).UnitState(1).mana, Is.Zero);
            Assert.That(Inventory(session).HeroSlots[0], Is.Null);
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(400));
            Assert.That(session.HaltReason, Is.Null);
        }
        [Test] public void AcceptedPyroPotionAdvancesExistingSphereOnceAndRejectedOrReplayedUseDoesNot()
        {
            foreach (string id in new[] { "I03L", "I03M" })
            {
                var session = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "H024" });
                object Call(string name, params object[] arguments) => typeof(OriginalSession).GetMethod(name, Private).Invoke(session, arguments);
                Call("BeginPyroSpheres", 1); Call("LaunchPyroSphere", 1, new OriginalPoint(2000, 1000), 0);
                var spheres = (IList)typeof(OriginalSession).GetField("pyroSpheres", Private).GetValue(session);
                var rule = (OriginalPyroFlyingSphere)spheres[0].GetType().GetField("rule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(spheres[0]);
                var item = Add(session, id, 2); var start = rule.Position;
                Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
                Assert.That(rule.Position.x, Is.EqualTo(start.x));
                Resources(session, 100, 0); var command = Command(session, item);
                Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                var moved = rule.Position; double dx = moved.x - start.x, dy = moved.y - start.y;
                Assert.That(Math.Sqrt(dx * dx + dy * dy), Is.EqualTo(18).Within(.000001));
                Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
                Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
                Assert.That(rule.Position.x, Is.EqualTo(moved.x)); Assert.That(rule.Position.y, Is.EqualTo(moved.y));
                Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(1));
            }
        }

        [Test] public void FullResourceRejectedBeforeChargeAndSameInstanceCanSucceedAfterDamage()
        {
            var session = Create(); var item = Add(session, "I03L");
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(1));
            Resources(session, 600, 145);
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(631));
        }

        [Test] public void IdenticalCopiesShareCooldownAndRetryConsumesOnlyRequestedCopy()
        {
            // Source cap99 prevents auto-merging the second physical copy.
            var session = Create(); var first = Add(session, "I03L", 99); var second = Add(session, "I03L");
            Resources(session, 10, 0);
            Assert.That(session.Apply(0, Command(session, first)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            int firstCharges = Inventory(session).HeroSlots[0].charges;
            Resources(session, 10, 0);
            Assert.That(session.Apply(0, Command(session, second, 1)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(firstCharges));
            Assert.That(Inventory(session).HeroSlots[1].charges, Is.EqualTo(1));
            for (int i = 0; i < 420; i++) { session.Advance(.05); session.DrainEvents(); }
            Assert.That(session.Apply(0, Command(session, second, 1)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(session).HeroSlots[1], Is.Null);
            Assert.That(Inventory(session).HeroSlots[0].charges, Is.EqualTo(firstCharges));
        }

        [Test] public void ManaStoneUsesAuthoredAmountAndRemovesItsPassiveContribution()
        {
            var session = Create(); var item = Add(session, "I023"); Resources(session, 100, 0);
            Assert.That(session.Apply(0, Command(session, item)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(World(session).UnitState(1).mana, Is.EqualTo(145));
            Assert.That(World(session).UnitState(1).health, Is.EqualTo(100));
            Assert.That(session.Snapshot().players[0].itemUses, Is.Empty);
        }

        [Test] public void StaleIdentityForeignActorAndServantCannotConsumeHeroPotion()
        {
            var session = Create(); var item = Add(session, "I03M"); Resources(session, 100, 0);
            var wrong = Command(session, item); wrong.itemInstanceId++;
            Assert.That(session.Apply(0, wrong), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            wrong = Command(session, item); wrong.actorEntityId = 2;
            Assert.That(session.Apply(0, wrong), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Inventory(session).Transfer(OriginalInventoryBag.Hero, 0).Applied, Is.True);
            Assert.That(session.Apply(0, Command(session, item, bag: OriginalInventoryBag.Servant)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Inventory(session).ServantSlots[0].charges, Is.EqualTo(1));
            Assert.That(World(session).UnitState(1).mana, Is.Zero);
        }

        [Test] public void WireRejectsInvalidUseTargetsAndForgedUseSnapshotIdentity()
        {
            var session = Create(); var item = Add(session, "I03M"); Resources(session, 100, 0);
            var codec = new OriginalUnitySessionCodec(); var command = Command(session, item);
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.True);
            command.itemSlot = 6;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            command.itemSlot = 0; command.targetKind = OriginalWorldTargetKind.Unit; command.targetId = 1;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.True,"Targeted item families share this wire shape");
            Assert.That(session.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand),"A self potion still rejects a unit target at authority");
            Assert.That(Inventory(session).HeroSlots[0].charges,Is.EqualTo(1));
            var view = session.Snapshot();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1, acknowledgedSequence = view.players[0].acknowledgedSequence,
                code = OriginalSessionReplyCode.Accepted, snapshot = view };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.True);
            view.players[0].itemUses[0].instanceId++;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemSummonScriptTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);return s;
        }
        static void Enemy(OriginalWorld w,int id=9001,string rawcode="n008",double x=335)
        {w.AddUnit(id,0,rawcode,new OriginalWorldUnitProfile{maxHealth=9000,maxMana=500,moveSpeed=400,collisionRadius=24},new OriginalPoint(x,1000));}
        static OriginalWorldUnitView Summon(OriginalWorld w)=>w.Snapshot().units.Single(u=>u.kind==OriginalWorldUnitKind.Summon);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalItemInstance Orb(OriginalSession s,OriginalWorld w,string itemId="I00Z")
        {
            var p=Player(s);var inventory=((OriginalInventory)p.GetType().GetField("inventory").GetValue(p)).Copy();
            var item=inventory.CreateInstance(itemId);Assert.That(inventory.TryPickup(item).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",p,inventory,w.UnitState(1)),Is.True);p.GetType().GetField("inventory").SetValue(p,inventory);
            var actor=w.UnitState(1);var profile=actor.profile;profile.maxMana=1000;w.UpdateProfile(1,profile,actor.health,1000);return item;
        }
        static OriginalSessionCommand OrbCommand(OriginalSession s,OriginalItemInstance item)=>new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemInstanceId=item.instanceId,itemSlot=0,x=335,y=1000};
        [Test] public void FingerPublicUsePreflightsIdentityBeforeDebitThenCreatesOneOwnedSummon()
        {
            var s=Create(out var w);var item=Orb(s,w,"I049");Enemy(w);
            OriginalSessionCommand Command(){var c=OrbCommand(s,item);c.targetKind=OriginalWorldTargetKind.Unit;c.targetId=9001;return c;}
            Assert.That(s.Snapshot().players[0].itemUses[0].targetMode,Is.EqualTo(OriginalAbilityTargetMode.Unit));
            typeof(OriginalSession).GetField("nextItemSummonId",Private).SetValue(s,OriginalWorld.LastSummonEntityId+1);
            Assert.That(s.Apply(0,Command()),Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));
            Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
            typeof(OriginalSession).GetField("nextItemSummonId",Private).SetValue(s,OriginalWorld.FirstSummonEntityId);
            var codec=new OriginalUnitySessionCodec();var command=Command();
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out var decoded),Is.True);
            Assert.That(s.Apply(0,decoded),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(900));Assert.That(w.UnitState(9001).health,Is.Zero);
            Assert.That(Summon(w).profile.maxHealth,Is.EqualTo(130));
            Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(30));
            Assert.That(s.Snapshot().players[0].itemUses[0].requiresCharge,Is.False);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,
                assignedSlot=1,snapshot=s.Snapshot(),acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence}),out _),Is.True);
            Assert.That(s.Apply(0,decoded),Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(900));
        }
        [Test] public void FingerNativeHeroTargetProducesNoCloneAndInvalidStructureCostsNothing()
        {
            var s=Create(out var w);var item=Orb(s,w,"I049");Enemy(w,9001,"H008");Enemy(w,9002,"hhou",500);
            var command=OrbCommand(s,item);command.targetKind=OriginalWorldTargetKind.Unit;command.targetId=9002;
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));
            command=OrbCommand(s,item);command.targetKind=OriginalWorldTargetKind.Unit;command.targetId=9001;
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(900));Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));
            Assert.That(w.Snapshot().units.Any(u=>u.kind==OriginalWorldUnitKind.Summon),Is.False);
        }
        [Test] public void OrbPublicUseRetainsItemPublishesHiddenSummonAndSeparatesSourceDamageFromNativeStun()
        {
            var s=Create(out var w);var item=Orb(s,w);Enemy(w);
            var view=s.Snapshot().players[0].itemUses[0];Assert.That(view.code,Is.EqualTo(OriginalItemUseCode.Ready));
            Assert.That(view.targetMode,Is.EqualTo(OriginalAbilityTargetMode.Point));Assert.That(view.requiresCharge,Is.False);
            Assert.That(s.Apply(0,OrbCommand(s,item)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var summon=Summon(w);Assert.That(summon.rawcode,Is.EqualTo("n01S"));Assert.That(summon.hidden,Is.True);
            Assert.That(summon.health,Is.EqualTo(1800));Assert.That(summon.mana,Is.Zero);Assert.That(summon.profile.maxMana,Is.Zero);
            Assert.That(w.UnitState(1).mana,Is.EqualTo(750));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(55));
            var codec=new OriginalUnitySessionCodec();Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
                kind=OriginalNetworkResponseKind.Snapshot,snapshot=s.Snapshot(),assignedSlot=1,
                acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence}),out _),Is.True);
            for(int i=0;i<19;i++){w.Advance(.05);Call(s,"AdvanceItemSummonScripts");}
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));Assert.That(w.UnitState(summon.entityId).hidden,Is.True);
            w.Advance(.05);Call(s,"AdvanceItemSummonScripts");
            Assert.That(w.UnitState(9001).health,Is.EqualTo(8700));Assert.That(w.UnitState(summon.entityId).hidden,Is.False);
            Assert.That(Call(s,"ActorCastBlocked",9001),Is.True);
            for(int i=0;i<54;i++)Call(s,"AdvanceNativeWeaponProcs",.05d);
            Assert.That(Call(s,"ActorCastBlocked",9001),Is.False);Assert.That(w.UnitState(9001).health,Is.EqualTo(8700));
            Assert.That(s.Apply(0,OrbCommand(s,item)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
        }
        [Test] public void OrbInvalidPointAndExhaustedIdentityDoNotSpendManaOrCooldown()
        {
            // A finite distant point now starts an approach. This case checks
            // a genuinely invalid point independently of the cast distance.
            var s=Create(out var w);var item=Orb(s,w);var command=OrbCommand(s,item);command.x=double.NaN;
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            typeof(OriginalSession).GetField("nextItemSummonId",Private).SetValue(s,OriginalWorld.LastSummonEntityId+1);
            Assert.That(s.Apply(0,OrbCommand(s,item)),Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
            Assert.That(w.Snapshot().units.Any(u=>u.kind==OriginalWorldUnitKind.Summon),Is.False);
        }
        [Test] public void AssimilationCreatesFreshRawcodeWithLevelBonusesAndInstanceAbilities()
        {
            var s=Create(out var w);Enemy(w);
            ((Dictionary<int,OriginalBossScaling>)typeof(OriginalSession).GetField("bossScaling",Private).GetValue(s)).Add(9001,new OriginalBossScaling(300,90,0));
            int id=(int)Call(s,"BeginItemAssimilation",1,9001);var summon=Summon(w);
            Assert.That(id,Is.EqualTo(summon.entityId));Assert.That(w.UnitState(9001).health,Is.Zero);
            Assert.That(summon.rawcode,Is.EqualTo("n008"));Assert.That(summon.ownerSlot,Is.EqualTo(1));Assert.That(summon.sourceHeroEntityId,Is.EqualTo(1));
            Assert.That(summon.profile.maxHealth,Is.EqualTo(130));Assert.That(summon.health,Is.EqualTo(130));
            Assert.That(summon.profile.maxMana,Is.Zero);Assert.That(summon.profile.moveSpeed,Is.EqualTo(300));
            Assert.That(Call(s,"SummonScriptAttackBonus",id),Is.EqualTo(5d));Assert.That(Call(s,"SummonScriptArmorBonus",id),Is.EqualTo(1d));
            foreach(string ability in new[]{"A0YP","A08U","A08T","ACmi"})Assert.That(Call(s,"HasEffectiveUnitAbility",summon,ability),Is.True);
            Assert.That(Call(s,"SourceUnitUserData",id),Is.Zero);Assert.That(s.Snapshot().enemies.Any(e=>e.entityId==id),Is.False);
        }
        [Test] public void AssimilationStillCreatesWhenVictimSurvivesRejectedDamageAndSourceIsDead()
        {
            var s=Create(out var w);Enemy(w);w.SetUnitState(9001,invulnerable:true);w.ForceUnitDeath(1);
            Assert.That((int)Call(s,"BeginItemAssimilation",1,9001),Is.GreaterThan(0));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));Assert.That(Summon(w).health,Is.EqualTo(130));
        }
        [Test] public void AssimilationExcludesHeroStructureIllusionAndEffectiveSpecialMarker()
        {
            var s=Create(out var w);Enemy(w);Enemy(w,9002,"hhou",600);Enemy(w,9003,"n024",800);
            Assert.That(Call(s,"ItemAssimilationEligible",w.UnitState(1)),Is.False);
            Assert.That(Call(s,"ItemAssimilationEligible",w.UnitState(9002)),Is.False);
            Assert.That(Call(s,"ItemAssimilationEligible",w.UnitState(9003)),Is.False,"ConvertUnitType(9) means Giant.");
            var hero=w.UnitState(1);w.AddIllusion(1000000007,1,hero.profile,new OriginalPoint(1000,1000),hero.health,hero.mana);
            Assert.That(Call(s,"ItemAssimilationEligible",w.UnitState(1000000007)),Is.False);
            Call(s,"ApplyUnitAbilityOverlay",9001,new[]{"A0K4"},Array.Empty<string>());
            Assert.That((int)Call(s,"BeginItemAssimilation",1,9001),Is.Zero);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));Assert.That(w.Snapshot().units.Any(u=>u.kind==OriginalWorldUnitKind.Summon),Is.False);
        }
        [Test] public void AssimilationBonusesAffectActualWeaponAndIncomingArmorExactlyOnce()
        {
            double[] hits=new double[2];
            for(int variant=0;variant<2;variant++)
            {
                var s=Create(out var w);Enemy(w);int id=(int)Call(s,"BeginItemAssimilation",1,9001);var summon=Summon(w);
                if(variant==0)((IDictionary)typeof(OriginalSession).GetField("itemSummonBonuses",Private).GetValue(s)).Clear();
                w.AddUnit(9002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},
                    new OriginalPoint(summon.position.x+70,summon.position.y));
                Assert.That(w.TryAttackTarget(id,OriginalWorldTargetKind.Unit,9002),Is.True);
                for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceWeapons");}
                Assert.That(w.UnitState(id).attackSequence,Is.EqualTo(1));hits[variant]=10000-w.UnitState(9002).health;
                if(variant==1)
                {
                    double hp=w.UnitState(id).health;
                    Call(s,"ApplyNativeTriggeredHit",0,0,w.UnitState(id),40d,OriginalTriggeredDamageMode.SpellNormal);
                    Assert.That(hp-w.UnitState(id).health,Is.EqualTo(40/1.06).Within(.00001));
                }
            }
            Assert.That(hits[1]-hits[0],Is.EqualTo(5/1.12).Within(.00001),"Flat source bonus joins white damage before defense exactly once.");
        }
        [Test] public void AssimilationIdentityBudgetFailurePrecedesDamageOrRegistryMutation()
        {
            var s=Create(out var w);Enemy(w);
            typeof(OriginalSession).GetField("nextItemSummonId",Private).SetValue(s,OriginalWorld.LastSummonEntityId+1);
            Assert.Throws<TargetInvocationException>(()=>Call(s,"BeginItemAssimilation",1,9001));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));Assert.That(w.Snapshot().units.Any(u=>u.kind==OriginalWorldUnitKind.Summon),Is.False);
        }
        [Test] public void OrbSourceTimerUsesFixedPointAndSurvivesCasterDeath()
        {
            var s=Create(out var w);Enemy(w);Enemy(w,9002,"hhou",500);Enemy(w,9003,"n008",800);
            Call(s,"BeginItemOrbSource",1,new OriginalPoint(335,1000));w.ForceUnitDeath(1);
            for(int i=0;i<19;i++){w.Advance(.05);Call(s,"AdvanceItemSummonScripts");}
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9000));w.Advance(.05);Call(s,"AdvanceItemSummonScripts");
            Assert.That(w.UnitState(9001).health,Is.EqualTo(8700));Assert.That(w.UnitState(9002).health,Is.EqualTo(9000));Assert.That(w.UnitState(9003).health,Is.EqualTo(9000));
            w.Advance(.05);Call(s,"AdvanceItemSummonScripts");Assert.That(w.UnitState(9001).health,Is.EqualTo(8700));
        }
        [Test] public void AssimilatedCasterUsesItsOwnManaCooldownAndEffectiveGrantedSpells()
        {
            var s=Create(out var w);Enemy(w,9001,"n009");int id=(int)Call(s,"BeginItemAssimilation",1,9001);
            var summon=w.UnitState(id);var profile=summon.profile;profile.maxMana=400;
            w.UpdateProfile(id,profile,summon.health,0);
            w.AddUnit(9002,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=1000,moveSpeed=250,collisionRadius=16},
                new OriginalPoint(summon.position.x+300,summon.position.y));
            Call(s,"AdvanceAssimilatedOrders");Assert.That(w.UnitState(9002).profile.moveSpeed,Is.EqualTo(250));
            w.UpdateProfile(id,profile,summon.health,400);for(int i=0;i<4;i++)w.Advance(.05);Call(s,"AdvanceAssimilatedOrders");
            Assert.That(w.UnitState(id).mana,Is.EqualTo(225));Assert.That(w.UnitState(9002).profile.moveSpeed,Is.EqualTo(100));
            for(int i=0;i<4;i++)w.Advance(.05);Call(s,"AdvanceAssimilatedOrders");Assert.That(w.UnitState(id).mana,Is.EqualTo(225),"Cooldown and stomp radius still apply.");
            Call(s,"ApplyUnitAbilityOverlay",id,Array.Empty<string>(),new[]{"A08U","A08T"});
            for(int i=0;i<220;i++)w.Advance(.05);Call(s,"AdvanceAssimilatedOrders");Assert.That(w.UnitState(id).mana,Is.EqualTo(225));
        }
    }
}

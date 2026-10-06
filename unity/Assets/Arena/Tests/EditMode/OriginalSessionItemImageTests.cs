using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemImageTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Private).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create(out OriginalWorld w,out OriginalItemInstance item)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            var candidate=Inventory(s).Copy();item=candidate.CreateInstance("I048");Assert.That(candidate.TryPickup(item).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.True);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
            var hero=w.UnitState(1);var p=hero.profile;p.maxMana=1000;w.UpdateProfile(1,p,hero.health,1000);return s;
        }
        static OriginalSessionCommand Command(OriginalSession s,OriginalItemInstance item,int target)=>new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemInstanceId=item.instanceId,itemSlot=0,targetKind=OriginalWorldTargetKind.Unit,targetId=target};
        static bool Wire(OriginalSessionView snapshot)
        {var codec=new OriginalUnitySessionCodec();return codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
            kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _);}
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double step=Math.Min(.05,seconds);w.Advance(step);Call(s,"AdvanceMirrors");seconds-=step;}}

        [Test] public void WandPublicUseCopiesCanonicalHeroAndCommitsManaCooldownWithoutConsumingItem()
        {
            var s=Create(out var w,out var item);var use=s.Snapshot().players[0].itemUses[0];
            Assert.That(use.code,Is.EqualTo(OriginalItemUseCode.Ready));Assert.That(use.requiresCharge,Is.False);
            Assert.That(use.targetMode,Is.EqualTo(OriginalAbilityTargetMode.Unit));
            var command=Command(s,item,1);Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var image=w.Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Illusion);
            Assert.That(image.imageFactory.ToString(),Is.EqualTo("ItemWand"));Assert.That(image.ownerSlot,Is.EqualTo(1));
            Assert.That(image.copySourceEntityId,Is.EqualTo(1));Assert.That(image.sourceHeroEntityId,Is.EqualTo(1));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(760));Assert.That(image.mana,Is.EqualTo(760));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(18));Assert.That(Wire(s.Snapshot()),Is.True);
            s.Apply(0,command);Assert.That(w.Snapshot().units.Count(x=>x.kind==OriginalWorldUnitKind.Illusion),Is.EqualTo(1));
            Assert.That(s.Apply(0,Command(s,item,1)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
        }
        [Test] public void WandEnemyCopyKeepsItsOwnFrozenStatsAndSurvivesDonorRemovalOnWire()
        {
            var s=Create(out var w,out var item);w.AddUnit(9001,0,"hfoo",new OriginalWorldUnitProfile{
                maxHealth=800,maxMana=0,moveSpeed=200,collisionRadius=16},new OriginalPoint(300,1000));
            ((Dictionary<int,OriginalBossScaling>)typeof(OriginalSession).GetField("bossScaling",Private).GetValue(s)).Add(9001,new OriginalBossScaling(30,5,0));
            Assert.That(s.Apply(0,Command(s,item,9001)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var image=w.Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Illusion);
            var stats=Call(s,"CombatStatsFor",image);double Value(string key)=>(double)stats.GetType().GetField(key,Private).GetValue(stats);
            Assert.That(Value("armor"),Is.EqualTo(7));Assert.That(Value("itemAttackDamageBonus"),Is.EqualTo(30));
            Assert.That(Value("primaryDamageBonus"),Is.Zero);Assert.That(image.sourceHeroEntityId,Is.Zero);
            Assert.That(Call(s,"IsNativeHeroPredicate",image),Is.False);Assert.That(Wire(s.Snapshot()),Is.True);
            w.RemoveUnit(9001);Assert.That(Wire(s.Snapshot()),Is.True);
            var broken=s.Snapshot();broken.world.units.Single(x=>x.entityId==image.entityId).copySourceEntityId=100000001;Assert.That(Wire(broken),Is.False);
            broken=s.Snapshot();broken.world.units.Single(x=>x.entityId==image.entityId).ownerSlot=0;Assert.That(Wire(broken),Is.False);
            broken=s.Snapshot();broken.world.units.Single(x=>x.entityId==image.entityId).sourceHeroEntityId=1;Assert.That(Wire(broken),Is.False);
        }
        [Test] public void WandSourceA04UAndIdentityBudgetFailuresDoNotSpendManaOrCooldown()
        {
            var s=Create(out var w,out var item);Call(s,"ApplyUnitAbilityOverlay",1, new[]{"A04U"},Array.Empty<string>());
            Assert.That(s.Apply(0,Command(s,item,1)),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A04U"});
            typeof(OriginalSession).GetField("nextIllusionId",Private).SetValue(s,int.MaxValue);
            Assert.That(s.Apply(0,Command(s,item,1)),Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));Assert.That(w.Snapshot().units.Any(x=>x.kind==OriginalWorldUnitKind.Illusion),Is.False);
        }
        [Test] public void WandImageUsesExistingPauseLifetimeAndDoesNotBecomeAHeroDeath()
        {
            var s=Create(out var w,out var item);Assert.That(s.Apply(0,Command(s,item,1)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            int id=w.Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Illusion).entityId;
            w.SetUnitState(id,paused:true);Tick(s,w,2);Assert.That(w.UnitState(id).health,Is.GreaterThan(0));
            w.SetUnitState(id,paused:false);Tick(s,w,9.9);Assert.That(w.UnitState(id).health,Is.GreaterThan(0));
            Tick(s,w,.11);Assert.That(w.UnitState(id).health,Is.Zero);
            Assert.That(w.UnitState(1).health,Is.GreaterThan(0));Assert.That(s.HaltReason,Is.Null);
        }
        [Test] public void WandOrdinaryImageActuallyAttacksWithRawWeaponAndReceivesDoubleIncomingDamage()
        {
            var s=Create(out var w,out var item);var profile=new OriginalWorldUnitProfile{maxHealth=2000,maxMana=0,moveSpeed=270,collisionRadius=16};
            w.AddUnit(9001,0,"hfoo",profile,new OriginalPoint(300,1000));
            Assert.That(s.Apply(0,Command(s,item,9001)),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var image=w.Snapshot().units.Single(x=>x.kind==OriginalWorldUnitKind.Illusion);
            Assert.That(((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",image)).Any(),Is.False,"No invented native passive inheritance");
            Assert.That(w.TryAttackTarget(image.entityId,OriginalWorldTargetKind.Unit,9001),Is.True);
            for(int i=0;i<60;i++){w.Advance(.05);Call(s,"AdvanceWeapons");}
            Assert.That(w.UnitState(image.entityId).attackSequence,Is.GreaterThan(0));Assert.That(w.UnitState(9001).health,Is.LessThan(2000));
            double before=w.UnitState(image.entityId).health;
            Call(s,"ApplyNativeTriggeredHit",9001,0,w.UnitState(image.entityId),40d,OriginalTriggeredDamageMode.ChaosUniversal);
            Assert.That(w.UnitState(image.entityId).health,Is.EqualTo(before-80).Within(.0001));Assert.That(s.HaltReason,Is.Null);
        }
        [Test] public void WandRejectsUnsupportedAncestryAndWrongTargetShapeBeforeAnyCost()
        {
            var s=Create(out var w,out var item);var wrong=Command(s,item,1);wrong.targetKind=OriginalWorldTargetKind.None;
            Assert.That(s.Apply(0,wrong),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            var hero=w.UnitState(1);w.AddIllusion(1000000007,1,hero.profile,new OriginalPoint(300,1000),hero.health,hero.mana);
            Assert.That(s.Apply(0,Command(s,item,1000000007)),Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(1000));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
        }
    }
}

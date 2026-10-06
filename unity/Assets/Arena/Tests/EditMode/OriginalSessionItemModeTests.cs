using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemModeTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string method,params object[] args)=>typeof(OriginalSession).GetMethod(method,Private).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalHeroStatsSnapshot Stats(OriginalSession s)=>(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
        static OriginalSession Create(string itemId,out OriginalWorld w)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            var candidate=Inventory(s).Copy();Assert.That(candidate.TryPickup(candidate.CreateInstance(itemId)).Applied,Is.True);
            Publish(s,w,candidate);return s;
        }
        static void Publish(OriginalSession s,OriginalWorld w,OriginalInventory candidate)
        {
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.True);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
        }
        static OriginalSessionReplyCode Toggle(OriginalSession s)
        {
            var item=Inventory(s).HeroSlots[0];return s.Apply(0,new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,
                sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemInstanceId=item.instanceId,itemSlot=0});
        }
        static void Tick(OriginalSession s,OriginalWorld w,double time)
        {while(time>1e-9){double step=Math.Min(.05,time);w.Advance(step);Call(s,"AdvanceItemModes");time-=step;}}
        static bool Wire(OriginalSession s)
        {var codec=new OriginalUnitySessionCodec();var snapshot=s.Snapshot();return codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
            kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _);}
        static void Add(OriginalWorld w,int id,int owner,double dx,double hp=1000)
        {var profile=new OriginalWorldUnitProfile{maxHealth=hp,maxMana=1000,moveSpeed=270,collisionRadius=8}; var point=new OriginalPoint(135+dx,1000);
            if(owner==0)w.AddUnit(id,0,"hfoo",profile,point); else Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=id,ownerSlot=owner,sourceHeroEntityId=owner,rawcode="hfoo",profile=profile,position=point,health=hp,mana=1000}}),Is.True);}

        [Test] public void SealReplacesTheItemAndChangesOneGlobalTimedBonusWithoutAccumulation()
        {
            var s=Create("I082",out var w);var baseline=Stats(s);long original=Inventory(s).HeroSlots[0].instanceId;
            Assert.That(s.Snapshot().players[0].itemUses[0].requiresCharge,Is.False);Assert.That(Wire(s),Is.True);
            Tick(s,w,1);double bonus=Math.Min(30,(int)((double)Call(s,"ScriptedAbilityAttack",Player(s))/25));
            Assert.That(Stats(s).armor.Require(),Is.EqualTo(baseline.armor.Require()+bonus));
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].itemId,Is.EqualTo("I083"));Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.Not.EqualTo(original));
            Assert.That(Stats(s).armor.Require(),Is.EqualTo(baseline.armor.Require()));
            Assert.That(Stats(s).attackMinimum.Require(),Is.EqualTo(baseline.attackMinimum.Require()));
            double hp=w.UnitState(1).health;Tick(s,w,1);
            double attackBonus=Math.Min(200,(int)(baseline.armor.Require()*2));
            Assert.That(Stats(s).attackMinimum.Require(),Is.EqualTo(baseline.attackMinimum.Require()+attackBonus));
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp).Within(.0001),"yP restores the initial life after its observable self hit");
            Tick(s,w,2);Assert.That(Stats(s).attackMinimum.Require(),Is.EqualTo(baseline.attackMinimum.Require()+attackBonus));
            Assert.That(Wire(s),Is.True);
            var candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Hero,0);Publish(s,w,candidate);
            Assert.That(Stats(s).itemAttackDamageBonus,Is.Zero);
        }

        [Test] public void ModeDuplicatesAreGroundedAndWarpathCyclesFreshIdentitiesWithExactAttributeModes()
        {
            var s=Create("I09L",out var w);var inv=Inventory(s);var duplicate=inv.CreateInstance("I09N");
            Assert.That(inv.TryPickup(duplicate).Code,Is.EqualTo(OriginalItemActionCode.Grounded));
            Assert.That(inv.HeroSlots.Count(x=>x!=null),Is.EqualTo(1));
            var strength=Stats(s);long first=inv.HeroSlots[0].instanceId;
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));var agility=Stats(s);
            Assert.That(Inventory(s).HeroSlots[0].itemId,Is.EqualTo("I09M"));Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.Not.EqualTo(first));
            Assert.That(agility.strength.Require(),Is.EqualTo(strength.strength.Require()-10));
            Assert.That(agility.agility.Require(),Is.EqualTo(strength.agility.Require()+10));
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));var intelligence=Stats(s);
            Assert.That(intelligence.intelligence.Require(),Is.EqualTo(strength.intelligence.Require()+10));
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(Stats(s).strength.Require(),Is.EqualTo(strength.strength.Require()));
            Assert.That(Wire(s),Is.True);
        }

        [Test] public void WarpathKillHealsNearbyAlliesAndSpellModeRestoresManaOnlyForEligibleSpells()
        {
            var s=Create("I09L",out var w);Add(w,100000001,1,80);Add(w,100000002,1,750);Add(w,9003,0,150);
            foreach(int id in new[]{1,100000001,100000002,9003}){var u=w.UnitState(id);w.UpdateProfile(id,u.profile,100,0);}
            double heal=Stats(s).strength.Require()*.25;
            Assert.That(Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(9003),100d,null),Is.True);
            Assert.That(w.UnitState(100000001).health,Is.EqualTo(100+heal));Assert.That(w.UnitState(100000002).health,Is.EqualTo(100));
            Assert.That(w.UnitState(1).health,Is.EqualTo(100+heal));Assert.That(w.UnitState(9003).health,Is.Zero);
            Toggle(s);Toggle(s);double mana=Stats(s).intelligence.Require()*.25;
            foreach(int id in new[]{1,100000001,100000002}){var u=w.UnitState(id);w.UpdateProfile(id,u.profile,u.health,0);}
            Call(s,"NotifyNativeSpellEffect",1,"A0SW");Assert.That(w.UnitState(100000001).mana,Is.Zero,"hK excludes the mask activation");
            Call(s,"NotifyNativeSpellEffect",1,"A0Z3");Assert.That(w.UnitState(100000001).mana,Is.EqualTo(mana));
            Assert.That(w.UnitState(100000002).mana,Is.Zero);Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));
        }

        [Test] public void WarpathAgilityRollUsesCurrentAgilityAndIntelligenceExecutionExcludesSpecialTags()
        {
            var s=Create("I09L",out var w);Toggle(s);Add(w,9001,0,100);Add(w,9002,0,380);Add(w,9003,0,600);
            double amount=Stats(s).agility.Require()*.25;
            Call(s,"ResolveWarpathDamage",1,w.UnitState(9001),2d,3);Assert.That(w.UnitState(9001).health,Is.EqualTo(1000));
            Call(s,"ResolveWarpathDamage",1,w.UnitState(9001),2d,2);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(1000-amount).Within(.0001));
            Assert.That(w.UnitState(9002).health,Is.EqualTo(1000-amount).Within(.0001));Assert.That(w.UnitState(9003).health,Is.EqualTo(1000));
            Toggle(s);Call(s,"ResolveWarpathDamage",1,w.UnitState(9001),21d,2);Assert.That(w.UnitState(9001).health,Is.Zero);
            Call(s,"ApplyUnitAbilityOverlay",9002,new[]{"Amim"},Array.Empty<string>());
            Call(s,"ResolveWarpathDamage",1,w.UnitState(9002),21d,2);Assert.That(w.UnitState(9002).health,Is.GreaterThan(0));
            foreach(int tag in new[]{1,2})
            {
                int id=(int)Call(s,"SpawnScriptedEnemy","hfoo",new OriginalPoint(400+tag*50,900),tag,Array.Empty<string>(),Array.Empty<string>(),
                    new OriginalWorldUnitProfile{maxHealth=1000,maxMana=0,moveSpeed=270,collisionRadius=8},(double?)0);
                Call(s,"ResolveWarpathDamage",1,w.UnitState(id),21d,2);Assert.That(w.UnitState(id).health,Is.EqualTo(1000),"Fa/source tags are excluded: "+tag);
            }
            Assert.That(s.HaltReason,Is.Null);
        }

        [Test] public void SealArmorProbeHonorsInvulnerabilityAndRestoresLowLife()
        {
            var s=Create("I083",out var w);var u=w.UnitState(1);w.UpdateProfile(1,u.profile,5,u.mana);
            double value=(double)Call(s,"ProbeSealArmor",Player(s));
            Assert.That(value,Is.EqualTo(Stats(s).armor.Require()).Within(.0001));Assert.That(w.UnitState(1).health,Is.EqualTo(5));
            w.SetUnitState(1,invulnerable:true);Tick(s,w,1);
            Assert.That(Stats(s).itemAttackDamageBonus,Is.EqualTo(250),"50 intrinsic plus the source cap of200");
            Assert.That(w.UnitState(1).health,Is.EqualTo(5));
        }

        [Test] public void RawNonInitialWarpathModeCannotEnableGlobalTriggersBeforePhysicalStrengthMaskPickup()
        {
            var s=Create("I09M",out var w);Assert.That(s.Snapshot().players[0].itemUses[0].code,Is.EqualTo(OriginalItemUseCode.NotReady));
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            var candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Hero,0);candidate.TryPickup(candidate.CreateInstance("I09L"));Publish(s,w,candidate);
            Assert.That(Toggle(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(typeof(OriginalSession).GetField("warpathTriggersEnabled",Private).GetValue(s),Is.True);
        }

        [Test] public void WarpathRangedReflectionUsesAttackersOwnerModeAndFollowsTheRetainedTarget()
        {
            var s=Create("I09L",out var w);
            const int id=100000010;
            Assert.That(w.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=id,ownerSlot=1,sourceHeroEntityId=1,rawcode="hrif",
                profile=new OriginalWorldUnitProfile{maxHealth=1000,maxMana=0,moveSpeed=270,collisionRadius=8},
                position=new OriginalPoint(375,1000),health=1000,mana=0}}),Is.True);
            // KT reads wx from the ranged attacker's OWNER, but I09L and
            // B0A7 from the defender. The attacker need not hold an item.
            Call(s,"ResolveWarpathDamage",id,w.UnitState(1),40d,2);
            var missiles=(IList)typeof(OriginalSession).GetField("warpathMissiles",Private).GetValue(s);
            Assert.That(missiles.Count,Is.EqualTo(1));Tick(s,w,.29);Assert.That(w.UnitState(id).health,Is.EqualTo(1000));
            Tick(s,w,.02);Assert.That(w.UnitState(id).health,Is.EqualTo(960));Assert.That(missiles.Count,Is.Zero);
            Toggle(s);Call(s,"ResolveWarpathDamage",id,w.UnitState(1),40d,2);
            Assert.That(missiles.Count,Is.Zero,"Switching the attacker's owner out of mode0 suppresses the reflection");
        }
    }
}


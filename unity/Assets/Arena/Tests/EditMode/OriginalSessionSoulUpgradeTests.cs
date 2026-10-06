using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionSoulUpgradeTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,null);
        static OriginalSession CreateHero(string id) => (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,new object[]{id});
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
        static object Call(OriginalSession s,string method,params object[] args) => typeof(OriginalSession).GetMethod(method,Private).Invoke(s,args);
        static OriginalHeroStatsSnapshot Stats(OriginalSession s) => (OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
        static OriginalSessionReplyCode Buy(OriginalSession s,string id) => s.Apply(0,new OriginalSessionCommand {
            kind=OriginalSessionCommandKind.BuySoulUpgrade,skillId=id,sequence=s.Snapshot().players[0].acknowledgedSequence+1});
        static void Finish(OriginalSession s)
        { for(int i=0;i<20;i++)World(s).Advance(.05);Call(s,"AdvanceSoulUpgrades"); }
        static OriginalSoulUpgradeView Upgrade(OriginalSession s,string id)=>s.Snapshot().players[0].soulUpgrades.Single(x=>x.id==id);

        [Test] public void ResearchDebitsOnceAndAppliesOnlyAfterOneSecond()
        {
            var s=Create();var w=World(s);var actor=w.UnitState(1);long souls=s.Snapshot().players[0].souls;
            w.UpdateProfile(1,actor.profile,233.47,62.35);
            Assert.That(Buy(s,"R002"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().players[0].souls,Is.EqualTo(souls-2));
            Assert.That(Upgrade(s,"R002").rank,Is.Zero);Assert.That(Upgrade(s,"R002").researching,Is.True);
            Assert.That(Buy(s,"R003"),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(631));
            Finish(s);
            Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(711));
            Assert.That(w.UnitState(1).health,Is.EqualTo(263));Assert.That(w.UnitState(1).mana,Is.EqualTo(62.35));
            Assert.That(Upgrade(s,"R002").rank,Is.EqualTo(1));Assert.That(Upgrade(s,"R002").researching,Is.False);
            Call(s,"AdvanceSoulUpgrades");Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(711));
            Assert.That(Buy(s,"R003"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);
            Assert.That(w.UnitState(1).profile.maxMana,Is.EqualTo(195));Assert.That(w.UnitState(1).mana,Is.EqualTo(84));
        }

        [Test] public void AllBasicCombatBonusesSurviveRepeatedCompositionWithoutAccumulation()
        {
            var s=Create();var before=Stats(s);
            foreach(string id in OriginalSoulUpgradeRules.BasicIds){Assert.That(Buy(s,id),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);}
            var after=Stats(s);
            Assert.That(after.maxHealth.Require()-before.maxHealth.Require(),Is.EqualTo(80));
            Assert.That(after.maxMana.Require()-before.maxMana.Require(),Is.EqualTo(50));
            Assert.That(after.armor.Require()-before.armor.Require(),Is.EqualTo(1));
            Assert.That(after.attackMinimum.Require()-before.attackMinimum.Require(),Is.EqualTo(5));
            Assert.That(after.upgradeAttackDamageBonus,Is.EqualTo(5));Assert.That(after.upgradeAttackSpeedBonus,Is.EqualTo(.05));
            Assert.That(after.baseMoveSpeed-before.baseMoveSpeed,Is.EqualTo(4));
            for(int i=0;i<10;i++)Assert.That(Stats(s).maxHealth.Require(),Is.EqualTo(after.maxHealth.Require()));
            var w=World(s);var actor=w.UnitState(1);w.UpdateProfile(1,actor.profile,100,50);w.SetUnitState(1,paused:true);
            Call(s,"AdvanceRegeneration",2d);
            Assert.That(w.UnitState(1).health,Is.EqualTo(102.9).Within(.001));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(50.4).Within(.001));
        }

        [Test] public void ResearchCapsCostsAndUniqueLockCannotConsumeResources()
        {
            var s=Create();long original=s.Snapshot().players[0].souls;
            Assert.That(Buy(s,"R00G"),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Buy(s,"R999"),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(s.Snapshot().players[0].souls,Is.EqualTo(original));
            for(int i=0;i<10;i++){Assert.That(Upgrade(s,"R004").soulCost,Is.EqualTo(2+i));Assert.That(Buy(s,"R004"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);}
            Assert.That(s.Snapshot().players[0].souls,Is.EqualTo(original-65));
            Assert.That(Buy(s,"R004"),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Upgrade(s,"R004").rank,Is.EqualTo(10));Assert.That(Stats(s).upgradeRegenPerSecond,Is.EqualTo(1.5));
        }

        [Test] public void ProtocolCarriesResearchAndRejectsForgedRanks()
        {
            var s=Create();Buy(s,"R002");var codec=new OriginalUnitySessionCodec();
            var response=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,code=OriginalSessionReplyCode.Accepted,
                assignedSlot=1,acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence,snapshot=s.Snapshot()};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out var decoded),Is.True);
            Assert.That(decoded.snapshot.players[0].soulUpgrades.Single(x=>x.id=="R002").researching,Is.True);
            response.snapshot.players[0].soulUpgrades[0].rank=11;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False);
        }

        [Test] public void AllThreeHeroesMatchNativeSoulTwoFirstRankVitality()
        {
            string[] heroes={"H008","N0A0","H024"};
            double[] expectedHealth={263,189,248},expectedMana={84,116,140};
            for(int i=0;i<heroes.Length;i++)
            {
                var s=CreateHero(heroes[i]);var w=World(s);var actor=w.UnitState(1);
                w.UpdateProfile(1,actor.profile,actor.profile.maxHealth*.37,actor.profile.maxMana*.43);
                Assert.That(Buy(s,"R002"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);
                Assert.That(w.UnitState(1).health,Is.EqualTo(expectedHealth[i]));
                Assert.That(Buy(s,"R003"),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);
                Assert.That(w.UnitState(1).mana,Is.EqualTo(expectedMana[i]));
            }
        }

        [Test] public void CompletingSixtyBasicsUnlocksAndAppliesEveryUniquePurchaseExactlyOnce()
        {
            foreach(string hero in new[]{"H008","N0A0","H024"})
            {
                var s=CreateHero(hero);var w=World(s);
                var player=((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
                var inventory=(OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
                inventory.GrantResources(0,1000);long before=s.Snapshot().players[0].souls;
                foreach(string id in OriginalSoulUpgradeRules.BasicIds)
                    for(int rank=0;rank<10;rank++){Assert.That(Buy(s,id),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);}
                Assert.That(s.Snapshot().players[0].souls,Is.EqualTo(before-390));
                var actor=w.UnitState(1);w.UpdateProfile(1,actor.profile,100,20);
                double primary=Stats(s).primary.Require();
                foreach(string id in OriginalSoulUpgradeRules.UniqueIds)
                {
                    Assert.That(Upgrade(s,id).unlocked,Is.True);
                    Assert.That(Buy(s,id),Is.EqualTo(OriginalSessionReplyCode.Accepted));Finish(s);
                    Assert.That(Upgrade(s,id).rank,Is.EqualTo(1));
                    Assert.That(Buy(s,id),Is.EqualTo(OriginalSessionReplyCode.NotReady));
                }
                Assert.That(s.Snapshot().players[0].souls,Is.EqualTo(before-390-6*90));
                Assert.That(Stats(s).primary.Require(),Is.EqualTo(primary+50));
                Assert.That(w.UnitState(1).health,Is.EqualTo(hero=="H008"?w.UnitState(1).profile.maxHealth:100));
                Assert.That(w.UnitState(1).mana,Is.EqualTo(hero=="H024"?w.UnitState(1).profile.maxMana:20));
                Assert.That(w.UnitState(1).pathingDisabled,Is.True);
            }
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemScriptActTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)
        {var m=typeof(OriginalSession).GetMethod(name,Private);Assert.That(m,Is.Not.Null,name);return m.Invoke(s,args);}
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create(out OriginalWorld w)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);return s;
        }
        static OriginalItemInstance Equip(OriginalSession s,string id)
        {
            var candidate=Inventory(s).Copy();var item=candidate.CreateInstance(id);Assert.That(candidate.TryPickup(item).Applied,Is.True);
            var w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.True,id);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);return item;
        }
        static OriginalSessionReplyCode Use(OriginalSession s,OriginalItemInstance item)=>s.Apply(0,new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemInstanceId=item.instanceId,itemSlot=Array.FindIndex(Inventory(s).HeroSlots,x=>x?.instanceId==item.instanceId)});
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        {while(seconds>1e-9){double step=Math.Min(.01,seconds);w.Advance(step);Call(s,"AdvanceItemScriptActs");seconds-=step;}}
        static void Begin(OriginalSession s,string id)=>Call(s,"BeginItemScriptAct",1,id);
        static void Enemy(OriginalWorld w,int id,double x,double hp=10000)=>w.AddUnit(id,0,"hfoo",
            new OriginalWorldUnitProfile{maxHealth=hp,maxMana=100,collisionRadius=16},new OriginalPoint(x,1000));

        [Test] public void GodArmorPublicUseSwapsCapturedPercentagesOnCallback32AndKeepsTheItem()
        {
            var s=Create(out var w);var item=Equip(s,"I01R");var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,actor.profile.maxHealth*.25,actor.profile.maxMana*.75);
            Assert.That(Use(s,item),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Use(s,item),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Tick(s,w,.95);Assert.That(w.UnitState(1).health,Is.EqualTo(actor.profile.maxHealth*.25).Within(.001));
            Tick(s,w,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(actor.profile.maxHealth*.75).Within(.001));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(actor.profile.maxMana*.25).Within(.001));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
        }
        [Test] public void BloodSwordUsesCappedCurrentHealthAndHealsItsRawSumDespiteArmor()
        {
            var s=Create(out var w);Enemy(w,9001,200);Enemy(w,9002,700);
            var actor=w.UnitState(1);w.UpdateProfile(1,actor.profile,50,actor.mana);Begin(s,"A0BI");
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9700));Assert.That(w.UnitState(9002).health,Is.EqualTo(10000));
            Assert.That(w.UnitState(1).health,Is.EqualTo(350));
        }
        [Test] public void DeathAegisRestoresAfterDamageButDoesNotPreventLethalDamage()
        {
            var s=Create(out var w);Begin(s,"A0BP");var actor=w.UnitState(1);
            Call(s,"ObserveItemScriptActDamage",actor,200d);w.ApplyUnitDamage(1,200);Tick(s,w,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(actor.health));
            Call(s,"ObserveItemScriptActDamage",w.UnitState(1),200d);w.ApplyUnitDamage(1,200);Tick(s,w,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(actor.health-100));
            Begin(s,"A0BP");w.UpdateProfile(1,actor.profile,50,actor.mana);
            Call(s,"ObserveItemScriptActDamage",w.UnitState(1),100d);w.ApplyUnitDamage(1,100);Tick(s,w,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(0));
        }
        [Test] public void DragonShieldCountsEventsAndSweepsEachTargetOnlyOnceAfterDeath()
        {
            var s=Create(out var w);Enemy(w,9001,200);Begin(s,"A16O");
            Call(s,"ObserveItemScriptActDamage",w.UnitState(1),100d);w.ForceUnitDeath(1);Call(s,"ObserveItemScriptActDeath",1);
            Tick(s,w,.14);Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
            Tick(s,w,.01);Assert.That(w.UnitState(9001).health,Is.EqualTo(9380));
            Tick(s,w,2);Assert.That(w.UnitState(9001).health,Is.EqualTo(9380));
        }
        [Test] public void GravityBootsCaptureTargetsThenPullTowardCurrentCasterForFifteenSteps()
        {
            var s=Create(out var w);Enemy(w,9001,350);Enemy(w,9002,800);Begin(s,"A1CV");
            w.ForcePosition(1,new OriginalPoint(600,1000));Tick(s,w,.03);
            Assert.That(w.UnitState(9001).position.x,Is.EqualTo(365));Tick(s,w,.42);
            Assert.That(w.UnitState(9001).position.x,Is.EqualTo(560));
            Assert.That(w.UnitState(9002).position.x,Is.EqualTo(800));
        }
        [Test] public void LightningBowKeepsItsFinalSeventeenthPulseAndStopsOnCasterDeath()
        {
            var s=Create(out var w);Enemy(w,9001,200,100000);Begin(s,"A0BM");Tick(s,w,8);
            Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A0BM"),Is.EqualTo(16));Tick(s,w,.5);
            Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A0BM"),Is.EqualTo(17));Tick(s,w,1);
            Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A0BM"),Is.EqualTo(17));
            var deadSession=Create(out var deadWorld);Enemy(deadWorld,9001,200,100000);Begin(deadSession,"A0BM");
            Tick(deadSession,deadWorld,.5);deadWorld.ForceUnitDeath(1);Tick(deadSession,deadWorld,.5);
            Assert.That((int)Call(deadSession,"ItemScriptActPulseCount",1,"A0BM"),Is.EqualTo(2),"source checks death after its final pulse");
            Tick(deadSession,deadWorld,2);Assert.That((int)Call(deadSession,"ItemScriptActPulseCount",1,"A0BM"),Is.EqualTo(2));
        }
        [Test] public void FractionalShieldPreservesSourceLastPoolBranchAndInfiniteDuration()
        {
            var s=Create(out var w);var actor=w.UnitState(1);
            Call(s,"AddItemSourceShield",1,100d,0d,.6,"A0MB");
            Call(s,"ObserveItemScriptActDamage",actor,100d);w.ApplyUnitDamage(1,100);Tick(s,w,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(actor.health),"100 damage spends60 pool, restores100");
            Tick(s,w,20);Call(s,"ObserveItemScriptActDamage",w.UnitState(1),100d);w.ApplyUnitDamage(1,100);Tick(s,w,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(actor.health-60),"last40 pool restores40, not40/.6");
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0MB"),Is.False);
        }
        [Test] public void WrathCapturesBaseAttributeExcludingEquipmentThenExpiresWithoutLosingLaterEquipment()
        {
            var s=Create(out var w);Equip(s,"I050");var before=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Begin(s,"A0HZ");var active=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Assert.That(active.strength.value-before.strength.value,Is.EqualTo(16),"floor22*.75, excluding36 itemSTR");
            var candidate=Inventory(s).Copy();var extra=candidate.CreateInstance("I00C");Assert.That(candidate.TryPickup(extra).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.True);
            Player(s).GetType().GetField("inventory").SetValue(Player(s),candidate);
            Tick(s,w,5);var expired=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Assert.That(expired.strength.value,Is.EqualTo(before.strength.value+12));
            Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(expired.maxHealth.value));
        }
        [Test] public void KnightCuirassRaisesOnlyItsNativeRankUntilCallbackFifteenAndDroppingEndsIt()
        {
            var s=Create(out var w);Equip(s,"I076");Begin(s,"A0FX");
            Assert.That(Call(s,"ItemScriptAbilityRank",1,"A0FT",1),Is.EqualTo(2));
            Tick(s,w,7);Assert.That(Call(s,"ItemScriptAbilityRank",1,"A0FT",1),Is.EqualTo(2));
            Tick(s,w,.5);Assert.That(Call(s,"ItemScriptAbilityRank",1,"A0FT",1),Is.EqualTo(1));
            Begin(s,"A0FX");Inventory(s).Transfer(OriginalInventoryBag.Hero,0);Tick(s,w,.5);
            Assert.That(Call(s,"ItemScriptAbilityRank",1,"A0FT",1),Is.EqualTo(1));
        }
        [Test] public void TitanOrbitHitsOnceFromHelperAndCleansOnTheCallbackAfterFinalSweep()
        {
            var s=Create(out var w);Enemy(w,9001,320);double amount=(double)Call(s,"ScriptedAbilityAttack",Player(s))*1.5;
            Begin(s,"A1BM");Tick(s,w,.03);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000-amount/1.12).Within(.001));
            Assert.That(Call(s,"ItemScriptDebuffMovementBonus",9001),Is.EqualTo(-.3));
            Tick(s,w,1.23);Assert.That(w.UnitState(9001).health,Is.EqualTo(10000-amount/1.12).Within(.001));
            Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A1BM"),Is.EqualTo(42));
            Tick(s,w,.03);Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A1BM"),Is.EqualTo(42));
        }
        [Test] public void MistFollowsDeadCasterForTenRecastsAndOnlyDeclaredMissAxisIsApplied()
        {
            var s=Create(out var w);Enemy(w,9001,200);Enemy(w,9002,900);Begin(s,"A0QQ");
            Assert.That(Call(s,"ItemScriptDebuffMissChance",9001),Is.EqualTo(.3));
            Assert.That(Call(s,"ActorCastBlocked",9001),Is.False);
            w.ForcePosition(1,new OriginalPoint(850,1000));w.ForceUnitDeath(1);Tick(s,w,.5);
            Assert.That(Call(s,"ItemScriptDebuffMissChance",9002),Is.EqualTo(.3));
            Tick(s,w,.21);Assert.That(Call(s,"ItemScriptDebuffMissChance",9001),Is.Zero);
            Tick(s,w,4.29);Assert.That((int)Call(s,"ItemScriptActPulseCount",1,"A0QQ"),Is.EqualTo(11));
            Tick(s,w,.71);Assert.That(Call(s,"ItemScriptDebuffMissChance",9002),Is.Zero);
        }
        [Test] public void DawnHealsAlliesAndFrostHasFivePulsesWithSlowLingeringAfterLastPulse()
        {
            var s=Create(out var w);Enemy(w,9001,200);var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            Begin(s,"A1CT");Assert.That(w.UnitState(1).health,Is.EqualTo(350));
            Assert.That(Call(s,"ItemScriptDebuffMissChance",9001),Is.EqualTo(.6));
            Begin(s,"A1CU");Tick(s,w,5);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9800).Within(.001));
            Assert.That(Call(s,"ItemScriptDebuffMovementBonus",9001),Is.EqualTo(-.4));
            Tick(s,w,1.99);Assert.That(Call(s,"ItemScriptDebuffMovementBonus",9001),Is.EqualTo(-.4));
            Tick(s,w,.02);Assert.That(Call(s,"ItemScriptDebuffMovementBonus",9001),Is.Zero);
        }
        [Test] public void CrownAveragesBothSidesThenDamagesEnemiesAndRestoresBookChildrenAfterEightSeconds()
        {
            var s=Create(out var w);Enemy(w,9001,200,2000);var hero=w.UnitState(1);
            w.UpdateProfile(1,hero.profile,hero.profile.maxHealth*.25,hero.mana);var enemy=w.UnitState(9001);
            w.UpdateProfile(9001,enemy.profile,1500,enemy.mana);Begin(s,"A0KQ");
            Assert.That(w.UnitState(1).health,Is.EqualTo(hero.profile.maxHealth*.5).Within(.001));
            Assert.That(w.UnitState(9001).health,Is.LessThan(1000));
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0X4"),Is.True);
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(9001),"A0X3"),Is.True);
            Tick(s,w,8);Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0X4"),Is.False);
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(9001),"A0X3"),Is.False);
        }
        [Test] public void DaedalusRangedActiveLaunchesThreeHelperProjectilesAndNoSecondaryRecursion()
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"N0A0"});
            var w=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);
            Equip(s,"I070");for(int i=0;i<4;i++)Enemy(w,9001+i,200+i*80);Begin(s,"A0T5");
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(9001),1d,null,true);Tick(s,w,1);
            Assert.That(w.UnitState(9001).health,Is.LessThan(9999));Assert.That(w.UnitState(9002).health,Is.LessThan(10000));
            Assert.That(w.UnitState(9003).health,Is.LessThan(10000));Assert.That(w.UnitState(9004).health,Is.EqualTo(10000));
            var after=w.UnitState(9001).health;Tick(s,w,1);Assert.That(w.UnitState(9001).health,Is.EqualTo(after));
        }
        [Test] public void ConflictingHelperRejectsPublicUseWithoutManaCooldownOrItemMutation()
        {
            var s=Create(out var w);var item=Equip(s,"I03N");
            var original=(OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Private).GetValue(s);
            var changed=UnityEngine.JsonUtility.FromJson<OriginalCombatCatalog>(UnityEngine.JsonUtility.ToJson(original));
            Array.Find(changed.Ability("A03W").fields,f=>f.key=="HeroDur1").number=99;
            var items=(OriginalItemCatalog)typeof(OriginalSession).GetField("itemCatalog",Private).GetValue(s);
            var guarded=new OriginalItemScriptActRules(items,changed);Assert.That(guarded.Rule("I03N"),Is.Null);
            Assert.That(guarded.Rule("I01R"),Is.Not.Null,"unrelated source action remains available");
            typeof(OriginalSession).GetField("scriptActRules",Private).SetValue(s,guarded);
            double mana=w.UnitState(1).mana,health=w.UnitState(1).health;
            Assert.That(Use(s,item),Is.Not.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(mana));Assert.That(w.UnitState(1).health,Is.EqualTo(health));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            var cooldowns=(IDictionary)typeof(OriginalSession).GetField("itemCooldowns",Private).GetValue(s);Assert.That(cooldowns.Count,Is.Zero);
        }
        [Test] public void AllSeventeenSourceActivesAreReachableByPublicItemCommandAndKeepTheirInstance()
        {
            foreach(string id in new[]{"I01R","I03N","I03S","I03T","I03Y","I076","I07O","I07T","I087","I07U","I09A","I0A6","I054","I070","I0AZ","I0B0","I0B1"})
            {
                var s=Create(out var w);var item=Equip(s,id);var actor=w.UnitState(1);
                actor.profile.maxMana=5000;w.UpdateProfile(1,actor.profile,actor.health,5000);
                Assert.That(Use(s,item),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId),id);
                Assert.That(s.HaltReason,Is.Null,id);
            }
        }
        [Test] public void SpaceBootWrathUsesSixSecondsWithoutChangingTheFiveSecondItemRule()
        {
            var s=Create(out var w);double before=((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.value;
            Call(s,"BeginItemWrath",1,"A0HZ",6d);Tick(s,w,5);
            Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.value,Is.EqualTo(before+16));
            Tick(s,w,1);Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.value,Is.EqualTo(before));
        }
    }
}

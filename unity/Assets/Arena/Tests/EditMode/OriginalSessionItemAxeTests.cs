using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemAxeTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld world,out OriginalItemInstance[] slots,bool withItems=false)
        {
            var args=new object[]{null,withItems};
            var s=(OriginalSession)typeof(OriginalSessionMirrorTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,args);
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            var player=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            var inventory=(OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            slots=(OriginalItemInstance[])typeof(OriginalInventory).GetField("heroSlots",Hidden).GetValue(inventory);
            slots[0]=new OriginalItemInstance{ownerId=1,instanceId=1,itemId="I07C",chargesKnown=true,charges=0};
            world.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,maxMana=0,collisionRadius=24},new OriginalPoint(1000,1000));
            return s;
        }
        [Test] public void AxeRepeatsResolvedEventThroughArmorWithoutRecursiveWeaponCallbacks()
        {
            var s=Create(out var w,out _);var before=w.UnitState(1);
            Call(s,"BeginItemAxeBuff",1);
            double first=40/1.12;
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(1001),first,null,true);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(10000-first-first/1.12).Within(1e-7));
            Assert.That(w.UnitState(1).health,Is.EqualTo(before.health-before.profile.maxHealth*.12).Within(1e-7));
            Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(1001),first,null);
            Assert.That(w.UnitState(1).health,Is.EqualTo(before.health-before.profile.maxHealth*.12).Within(1e-7));
        }
        [Test] public void AxeQuarterLifeRemovesBuffAndDoesNotConsumeHealthOrRepeatDamage()
        {
            var s=Create(out var w,out _);var hero=w.UnitState(1);
            w.UpdateProfile(1,hero.profile,hero.profile.maxHealth*.25,hero.mana);Call(s,"BeginItemAxeBuff",1);
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(1001),40,null,true);
            Assert.That(w.UnitState(1001).health,Is.EqualTo(9960));
            Assert.That(w.UnitState(1).health,Is.EqualTo(hero.profile.maxHealth*.25));
            Assert.That((int)Call(s,"ItemAxeAbilityRank",1,"A0JR",2),Is.EqualTo(1));
            Assert.That((int)Call(s,"ItemAxeAbilityRank",1,"A0JR",1),Is.EqualTo(1));
        }
        [Test] public void AxeDropStopsWeaponEffectButItsLowLifeTimerContinuesWhilePaused()
        {
            var s=Create(out var w,out var slots);var hero=w.UnitState(1);Call(s,"BeginItemAxeBuff",1);
            slots[0]=null;Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(1001),40,null,true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hero.health));
            w.UpdateProfile(1,hero.profile,hero.profile.maxHealth*.2,hero.mana);w.SetUnitState(1,paused:true);
            for(int i=0;i<20;i++)w.Advance(.05);
            Call(s,"AdvanceItemAxes");
            var states=(IDictionary)typeof(OriginalSession).GetField("itemAxes",Hidden).GetValue(s);
            Assert.That(states.Contains(1),Is.False);
            Assert.That((int)Call(s,"ItemAxeAbilityRank",1,"A0JR",2),Is.EqualTo(2),"Missing ability cannot receive SetUnitAbilityLevel.");
        }
        [Test] public void AxeNestedLethalRepeatDoesNotApplyOriginalHitAgainAndSpellPowerAffectsOnlySelfHl()
        {
            var s=Create(out var w,out var slots);slots[1]=new OriginalItemInstance{ownerId=1,instanceId=2,itemId="I085"};
            var hero=w.UnitState(1);var target=w.UnitState(1001);w.UpdateProfile(1001,target.profile,20,0);
            Call(s,"BeginItemAxeBuff",1);w.DrainEvents();
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(1001),40.0,null,true);
            Assert.That(w.UnitState(1001).health,Is.Zero);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hero.health-hero.profile.maxHealth*.12*1.2).Within(1e-7));
            var damage=Array.FindAll(w.DrainEvents(),e=>e.kind==OriginalWorldEventKind.UnitDamaged&&e.entityId==1001);
            Assert.That(damage.Length,Is.EqualTo(1),"The nested lethal repeat ends the original pending hit.");
        }
        [Test] public void PublicAxeToggleIsFreeReusableAndNativeBuffDoesNotPreventItsOwnOffSwitch()
        {
            var s=Create(out var w,out var slots,true);
            var player=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            var inventory=(OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            Assert.That(Call(s,"ApplyEquipmentProfile",player,inventory,w.UnitState(1)),Is.True);
            var before=w.UnitState(1);double rate=(double)Call(s,"WeaponRate",before);
            OriginalSessionCommand Command()=>new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,
                sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemInstanceId=1,itemSlot=0,bag=OriginalInventoryBag.Hero};
            Assert.That(s.Apply(0,Command()),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(before.mana));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(before.profile.moveSpeed*1.2).Within(1e-7));
            Assert.That((double)Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(Math.Min(5,rate+2.75)).Within(1e-7));
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True);Assert.That(Call(s,"ActorItemBlocked",1),Is.False);
            Assert.That(s.Apply(0,Command()),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            for(int i=0;i<121;i++)w.Advance(.05);
            Assert.That(s.Apply(0,Command()),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(before.profile.moveSpeed).Within(1e-7));
            Assert.That(inventory.HeroSlots[0].itemId,Is.EqualTo("I07C"));
        }
        [Test] public void AxeNativeBuffCleanseRestoresModifiersBeforeSourceTimerAndKeepsOtherSoulBurn()
        {
            var s=Create(out var w,out _);var hero=w.UnitState(1);
            Call(s,"BeginItemSoulBurn",1,1);Call(s,"BeginItemAxeBuff",1);
            Assert.That((double)Call(s,"ItemScriptDebuffMovementBonus",1),Is.EqualTo(.4));
            Call(s,"RemoveItemAxeBuff",1);
            Assert.That((double)Call(s,"ItemScriptDebuffMovementBonus",1),Is.EqualTo(.2));
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True,"Other native Soul Burn keeps its own control.");
            Call(s,"BeginItemAxeBuff",1);Call(s,"RemoveItemScriptDebuffs",1);
            double life=w.UnitState(1).health;
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(1001),40d,null,true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(life),"Cleansed B0A1 cannot run Bt before the next At timer.");
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
            for(int i=0;i<20;i++)w.Advance(.05);Call(s,"AdvanceItemAxes");
            Assert.That((int)Call(s,"ItemAxeAbilityRank",1,"A0JR",2),Is.EqualTo(1));
        }
        [Test] public void AxeNativeZeroPulsesReachDamageWatchWithoutSelfHealthLossAndStopOnToggleOff()
        {
            var s=Create(out var w,out _);var reference=Create(out _,out _);
            typeof(OriginalSession).GetField("warpathTriggersEnabled",Hidden).SetValue(s,true);
            var random=typeof(OriginalSession).GetField("weaponRandom",Hidden);random.SetValue(s,123u);random.SetValue(reference,123u);
            double hp=w.UnitState(1).health;Call(s,"BeginItemAxeBuff",1);
            for(int i=0;i<101;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}
            for(int i=0;i<6;i++)Call(reference,"RollWeapon",20);
            Assert.That(random.GetValue(s),Is.EqualTo(random.GetValue(reference)));
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Call(s,"RemoveItemAxeBuff",1);
            for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}
            Assert.That(random.GetValue(s),Is.EqualTo(random.GetValue(reference)));
        }
    }
}

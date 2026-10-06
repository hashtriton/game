using System;
using System.Collections;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemMutabilityTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Call(OriginalSession s,string method,params object[] args)=>typeof(OriginalSession).GetMethod(method,Private).Invoke(s,args);
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create(string hero,out OriginalWorld world)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{hero});
            world=(OriginalWorld)typeof(OriginalSession).GetField("world",Private).GetValue(s);return s;
        }
        static void Publish(OriginalSession s,OriginalWorld w,OriginalInventory inventory)
        {Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),inventory,w.UnitState(1)),Is.True);Player(s).GetType().GetField("inventory").SetValue(Player(s),inventory);}
        static void Pickup(OriginalSession s,OriginalWorld w)
        {var candidate=Inventory(s).Copy();Assert.That(candidate.TryPickup(candidate.CreateInstance("I05E")).Applied,Is.True);Publish(s,w,candidate);}
        static void Maxima(OriginalWorld w,double hp,double mp)
        {Assert.That(w.UnitState(1).profile.maxHealth,Is.EqualTo(hp));Assert.That(w.UnitState(1).profile.maxMana,Is.EqualTo(mp));}
        static OriginalSessionReplyCode Use(OriginalSession s)=>s.Apply(0,new OriginalSessionCommand{
            kind=OriginalSessionCommandKind.UseItem,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
            itemSlot=0,itemInstanceId=Inventory(s).HeroSlots[0].instanceId});
        static void Wire(OriginalSession s)
        {var snapshot=s.Snapshot();var codec=new OriginalUnitySessionCodec();Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
            kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence}),out _),Is.True);}

        [Test] public void PublicMutabilityUsesNativeZeroCostAndPreflightsBeforeCooldownPublication()
        {
            var s=Create("H008",out var w);Pickup(s,w);var before=w.UnitState(1);double mana=before.mana;
            Assert.That(s.Snapshot().players[0].itemUses[0].requiresCharge,Is.False);
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).profile.maxHealth,Is.LessThan(before.profile.maxHealth));
            Assert.That(w.UnitState(1).mana,Is.GreaterThan(mana));
            Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(6));
            Assert.That(Inventory(s).HeroSlots[0],Is.Not.Null);Wire(s);
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            s=Create("H008",out w);Pickup(s,w);before=w.UnitState(1);var profile=before.profile;profile.maxHealth=1;
            w.UpdateProfile(1,profile,1,before.mana);long revision=w.Snapshot().revision;
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.Snapshot().revision,Is.EqualTo(revision));Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
        }

        [Test] public void DropUndoRunsBeforeNativeThirtyAttributeRemovalForPartialResources()
        {
            var s=Create("H008",out var w);Pickup(s,w);var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,actor.profile.maxHealth*.37,actor.profile.maxMana*.23);
            var before=w.UnitState(1);var candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Hero,0);
            var state=Call(s,"CopyItemMutability",1);var vitality=new OriginalItemVitalityResult{known=true,
                health=before.health,maxHealth=before.profile.maxHealth,mana=before.mana,maxMana=before.profile.maxMana};
            Call(s,"ChangeItemMutability",state,vitality,-1);
            var effects=(OriginalInventoryEffects)typeof(OriginalSession).GetField("itemEffects",Private).GetValue(s);
            var expected=effects.ChangeVitality("I05E",false,vitality.health,vitality.maxHealth,vitality.mana,vitality.maxMana,8,10,"STR").Require();
            Publish(s,w,candidate);Assert.That(w.UnitState(1).health,Is.EqualTo(expected.health));Assert.That(w.UnitState(1).mana,Is.EqualTo(expected.mana));
        }

        [Test] public void EternalPowerUsesMeasuredSoulBurnAxesAndCleansUpItsOwnControlOnly()
        {
            var s=Create("H008",out var w);var candidate=Inventory(s).Copy();candidate.TryPickup(candidate.CreateInstance("I017"));Publish(s,w,candidate);
            var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,hero.health,hero.profile.maxMana);double mana=w.UnitState(1).mana;
            Assert.That(Use(s),Is.EqualTo(OriginalSessionReplyCode.Accepted));Assert.That(w.UnitState(1).mana,Is.EqualTo(mana-90));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(300));Assert.That(Call(s,"ActorCastBlocked",1),Is.True);
            Assert.That(Call(s,"ActorWeaponBlocked",1),Is.False);Assert.That(Call(s,"ItemScriptDebuffAttackSlow",1),Is.EqualTo(-2));Wire(s);
            Call(s,"SetActorControl",1,"fixture-independent",OriginalActorControlMask.Cast,0d,false,false);
            for(int i=0;i<160;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));Assert.That(Call(s,"ItemScriptDebuffAttackSlow",1),Is.Zero);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True);Call(s,"ClearActorControl",1,"fixture-independent");Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
        }
        [Test] public void SoulBurnPublishesEightZeroDamageEventsWithPauseAndDispelBoundaries()
        {
            var s=Create("H008",out var w);var reference=Create("H008",out _);
            // KT consumes one shared draw on EVERY admitted event. This is
            // observable evidence that damage0 reaches the central pipeline.
            typeof(OriginalSession).GetField("warpathTriggersEnabled",Private).SetValue(s,true);
            var random=typeof(OriginalSession).GetField("weaponRandom",Private);random.SetValue(s,123u);random.SetValue(reference,123u);
            Call(s,"BeginItemSoulBurn",1,1);double hp=w.UnitState(1).health;
            w.SetUnitState(1,paused:true);for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}Assert.That(random.GetValue(s),Is.EqualTo(123u));
            w.SetUnitState(1,paused:false);
            for(int i=0;i<160;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}
            for(int i=0;i<8;i++)Call(reference,"RollWeapon",20);
            Assert.That(random.GetValue(s),Is.EqualTo(random.GetValue(reference)));Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Call(s,"BeginItemSoulBurn",1,1);Call(s,"ClearNegativeActorControls",1);var before=random.GetValue(s);
            for(int i=0;i<20;i++){w.Advance(.05);Call(s,"AdvanceItemScriptDebuffs");}Assert.That(random.GetValue(s),Is.EqualTo(before));Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
        }

        [Test] public void FirstPickupExchangesTwentyPercentOfManaAfterTheThirtyAttributeBonus()
        {
            foreach(string hero in new[]{"H008","H024","N0A0"})
            {
                var s=Create(hero,out var w);var before=w.UnitState(1);
                double hp=before.profile.maxHealth+240,mp=before.profile.maxMana+300;
                int exchange=(int)(mp*.2);Pickup(s,w);Maxima(w,hp+exchange,mp-exchange);
                // Recomposition must not replay Tu or lose its permanent max changes.
                Publish(s,w,Inventory(s).Copy());Maxima(w,hp+exchange,mp-exchange);
                var removed=Inventory(s).Copy();removed.Transfer(OriginalInventoryBag.Hero,0);Publish(s,w,removed);
                Maxima(w,before.profile.maxHealth,before.profile.maxMana);
                // Tu keeps pH=2 after drop, so another pickup chooses the other direction.
                removed=removed.Copy();removed.Transfer(OriginalInventoryBag.Servant,0);Publish(s,w,removed);
                exchange=(int)(hp*.2);Maxima(w,hp-exchange,mp+exchange);
            }
        }
        [Test] public void SpellEffectUndoesTheSavedExchangeBeforeChoosingTheOtherMaximum()
        {
            var s=Create("H008",out var w);var before=w.UnitState(1);Pickup(s,w);
            double hp=before.profile.maxHealth+240,mp=before.profile.maxMana+300;
            Call(s,"ApplyItemMutabilityEffect",1);int amount=(int)(hp*.2);Maxima(w,hp-amount,mp+amount);
            var stats=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Assert.That(stats.maxHealth.Require(),Is.EqualTo(hp-amount));Assert.That(stats.maxMana.Require(),Is.EqualTo(mp+amount));
            Call(s,"ApplyItemMutabilityEffect",1);amount=(int)(mp*.2);Maxima(w,hp+amount,mp-amount);
        }
        [Test] public void DuplicatePickupDoesNotUndoTheFirstExchangeAndDropsReuseTheLastSavedAmount()
        {
            var s=Create("H008",out var w);var before=w.UnitState(1);
            int first=(int)((before.profile.maxMana+300)*.2);Pickup(s,w);
            int second=(int)((before.profile.maxHealth+480+first)*.2);Pickup(s,w);
            Maxima(w,before.profile.maxHealth+480+first-second,before.profile.maxMana+600-first+second);
            var candidate=Inventory(s).Copy();candidate.Transfer(OriginalInventoryBag.Hero,1);Publish(s,w,candidate);
            Maxima(w,before.profile.maxHealth+240+first,before.profile.maxMana+300-first);
            // Source reuses the saved second amount on another drop. Here it
            // would create a negative maximum mana; that unmeasured native
            // clamp is fail-closed, without consuming the remaining item.
            candidate=candidate.Copy();candidate.Transfer(OriginalInventoryBag.Hero,0);
            long revision=w.Snapshot().revision;
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.False);
            Assert.That(w.Snapshot().revision,Is.EqualTo(revision));Assert.That(Inventory(s).HeroSlots[0],Is.Not.Null);
        }
        [Test] public void RejectedLateProfileDoesNotPublishMutabilityStateOrInventory()
        {
            var s=Create("H008",out var w);var actor=w.UnitState(1);
            // A foreign invalid base profile makes final composition disagree;
            // preparation must leave the pH=1 state and world untouched.
            w.UpdateProfile(1,new OriginalWorldUnitProfile{maxHealth=actor.profile.maxHealth+1,maxMana=actor.profile.maxMana,
                moveSpeed=actor.profile.moveSpeed,collisionRadius=actor.profile.collisionRadius},actor.health,actor.mana);
            long revision=w.Snapshot().revision;var candidate=Inventory(s).Copy();candidate.TryPickup(candidate.CreateInstance("I05E"));
            Assert.That(Call(s,"ApplyEquipmentProfile",Player(s),candidate,w.UnitState(1)),Is.False);
            Assert.That(w.Snapshot().revision,Is.EqualTo(revision));Assert.That(Inventory(s).HeroSlots[0],Is.Null);
            w.UpdateProfile(1,actor.profile,actor.health,actor.mana);Pickup(s,w);
            int exchange=(int)((actor.profile.maxMana+300)*.2);
            Maxima(w,actor.profile.maxHealth+240+exchange,actor.profile.maxMana+300-exchange);
        }
    }
}



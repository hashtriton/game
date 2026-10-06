using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemChannelTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession session, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, Private).Invoke(session, args);
        static object Player(OriginalSession session) => ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
        static OriginalInventory Inventory(OriginalSession session) => (OriginalInventory)Player(session).GetType().GetField("inventory").GetValue(Player(session));
        static OriginalSession Create(out OriginalWorld world)
        {
            var session = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session); return session;
        }
        static OriginalItemInstance Equip(OriginalSession session, OriginalWorld world, string id)
        {
            var candidate = Inventory(session).Copy(); var item = candidate.CreateInstance(id);
            Assert.That(candidate.TryPickup(item).Applied, Is.True);
            Assert.That(Call(session, "ApplyEquipmentProfile", Player(session), candidate, world.UnitState(1)), Is.True);
            Player(session).GetType().GetField("inventory").SetValue(Player(session), candidate); return item;
        }
        static void Tick(OriginalSession session, OriginalWorld world, double seconds)
        {
            while (seconds > 1e-9) { double step = Math.Min(.01, seconds); world.Advance(step); Call(session, "AdvanceItemScriptActs"); Call(session, "AdvanceItemChannels"); seconds -= step; }
        }
        static void Ally(OriginalWorld world, int id, double x, double health = 100)
        {
            Assert.That(world.TryPublishSummons(new[] {new OriginalWorldSummonSpawn {entityId = id, ownerSlot = 1, sourceHeroEntityId = 1,
                rawcode = "hfoo", profile = new OriginalWorldUnitProfile {maxHealth = 2000, maxMana = 100, collisionRadius = 8},
                position = new OriginalPoint(x,1000), health = health, mana = 100}}), Is.True);
        }
        static void Enemy(OriginalWorld world, int id, double x) => world.AddUnit(id, 0, "hfoo",
            new OriginalWorldUnitProfile {maxHealth = 10000, maxMana = 100, collisionRadius = 8}, new OriginalPoint(x, 1000));

        [Test] public void HealingWaveHitsInitialThenNineDistinctNearestAlliesAndNeverAnEnemy()
        {
            var s = Create(out var w);
            for (int i = 0; i < 11; i++) Ally(w, 100000051+i, 200+i*40);
            Enemy(w, 9001, 700);
            Call(s, "BeginItemChannel", 1, "A12Y", 100000051, default(OriginalPoint));
            Assert.That(w.UnitState(100000051).health, Is.EqualTo(500));
            Tick(s,w,3.3);
            Assert.That(w.Snapshot().units.Count(u => u.kind == OriginalWorldUnitKind.Summon && u.health == 500), Is.EqualTo(10));
            Assert.That(w.UnitState(100000061).health, Is.EqualTo(100), "Eleventh recipient is beyond the nine-bounce cap.");
            Assert.That(w.UnitState(9001).health, Is.EqualTo(10000));
        }
        [Test] public void RegenerationOrbFollowsAndHealsEightTimesWithoutCallingModifiedSourceHealing()
        {
            var s=Create(out var w); Ally(w,100000051,200);
            Call(s,"BeginItemChannel",1,"A0CC",0,default(OriginalPoint)); Tick(s,w,.5);
            Assert.That(w.UnitState(100000051).health,Is.EqualTo(160));
            Tick(s,w,8); Assert.That(w.UnitState(100000051).health,Is.EqualTo(580));
        }
        [Test] public void SpitTravelsBeforeDamageThenDealsFiveCurrentAttributeTicks()
        {
            var s=Create(out var w); Enemy(w,9001,555);
            Call(s,"BeginItemChannel",1,"A17V",0,new OriginalPoint(555,1000)); Tick(s,w,.59);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
            Tick(s,w,.01); Assert.That(w.UnitState(9001).health,Is.EqualTo(10000-35*1.5));
            Tick(s,w,5); Assert.That(w.UnitState(9001).health,Is.EqualTo(10000-35*2.5).Within(.001));
            Tick(s,w,2); Assert.That(w.UnitState(9001).health,Is.EqualTo(10000-35*2.5).Within(.001));
        }
        [Test] public void TransfusionReturnsHalfItsRawSumAfterHittingAndDoesNotHealEarly()
        {
            var s=Create(out var w); Enemy(w,9001,335); var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,50,actor.mana);
            Call(s,"BeginItemChannel",1,"A0TX",9001,default(OriginalPoint)); Tick(s,w,.24);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9100)); Assert.That(w.UnitState(1).health,Is.EqualTo(50));
            Tick(s,w,.4); Assert.That(w.UnitState(1).health,Is.EqualTo(500));
        }
        [Test] public void TargetedUseValidatesBeforeDebitAndProductionCodecRetainsItsTarget()
        {
            var s=Create(out var w); var item=Equip(s,w,"I05A"); Enemy(w,9001,335);
            var command=new OriginalSessionCommand {kind=OriginalSessionCommandKind.UseItem, sequence=4,
                itemSlot=0,itemInstanceId=item.instanceId,targetKind=OriginalWorldTargetKind.Unit,targetId=9001};
            var codec=new OriginalUnitySessionCodec();
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command),out var decoded),Is.True);
            var before=w.UnitState(1); command.sequence=s.Snapshot().players[0].acknowledgedSequence+1; command.targetId=1;
            Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(w.UnitState(1).mana,Is.EqualTo(before.mana));
            Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.Zero);
            decoded.sequence=s.Snapshot().players[0].acknowledgedSequence+1;
            Assert.That(s.Apply(0,decoded),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().players[0].itemUses[0].targetMode,Is.EqualTo(OriginalAbilityTargetMode.Unit));
        }
        [Test] public void ReusableReadyItemsWithZeroChargesSurviveProductionSnapshotCodec()
        {
            foreach(string id in new[]{"I01R","I02E","I03Z","I05A"})
            {
                var s=Create(out var w); Equip(s,w,id); var snapshot=s.Snapshot(); var view=snapshot.players[0].itemUses[0];
                Assert.That(view.code,Is.EqualTo(OriginalItemUseCode.Ready),id); Assert.That(view.requiresCharge,Is.False,id);
                var codec=new OriginalUnitySessionCodec();
                var response=new OriginalNetworkResponse {kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,
                    assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence};
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True,id);
                view.requiresCharge=true;
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.False,id);
            }
        }

        [Test] public void EveryChannelPublishesAUsablePublicCommandWithItsDeclaredTargetMode()
        {
            foreach(string id in new[]{"I00T","I00V","I04D","I08I","I05A","I045","I096","I08Q","I09P","I05P","I060",
                "I06J","I08U","I07P","I08D","I06R","I090","I05D","I015","I07Y","I02C"})
            {
                var s=Create(out var w);var item=Equip(s,w,id);Enemy(w,9001,335);
                var hero=w.UnitState(1);var profile=hero.profile;profile.maxMana=10000;w.UpdateProfile(1,profile,hero.health,10000);
                var view=s.Snapshot().players[0].itemUses.Single(x=>x.instanceId==item.instanceId);
                Assert.That(view.code,Is.EqualTo(OriginalItemUseCode.Ready),id);
                var command=new OriginalSessionCommand {kind=OriginalSessionCommandKind.UseItem,
                    sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemSlot=0,itemInstanceId=item.instanceId,x=335,y=1000};
                if(view.targetMode==OriginalAbilityTargetMode.Unit)
                {command.targetKind=OriginalWorldTargetKind.Unit;command.targetId=id=="I00T"||id=="I00V"||id=="I07P"||id=="I08D"?1:9001;}
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId),id);
                Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.GreaterThan(0),id);
            }
        }

        [Test] public void ProtectionStaffHealsBeforeDamageAndItsShieldThenHealsOnTheLaterCallback()
        {
            var s=Create(out var w);var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            Call(s,"BeginItemChannel",1,"A0C5",0,hero.position);
            Call(s,"ApplyResolvedUnitHit",9001,0,w.UnitState(1),80d,null);
            Assert.That(w.UnitState(1).health,Is.EqualTo(100));
            Tick(s,w,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(180));
            Tick(s,w,10);Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A19J"),Is.False);
        }

        [Test] public void LightTotemSelectsLowHealthHeroAndRemovesItsShieldOutsideRange()
        {
            var s=Create(out var w);var hero=w.UnitState(1);w.UpdateProfile(1,hero.profile,100,hero.mana);
            Call(s,"BeginItemChannel",1,"A0M9",0,hero.position);Tick(s,w,.2);
            Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0MB"),Is.True);
            w.ForcePosition(1,new OriginalPoint(hero.position.x+601,hero.position.y));Tick(s,w,.2);
            Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0MB"),Is.False);
            Tick(s,w,6);Assert.That(((IList)typeof(OriginalSession).GetField("itemProtectionFields",Private).GetValue(s)).Count,Is.Zero);
        }

        [Test] public void CrossbowPushesBothActorsApartThenRestoresTheirPathing()
        {
            var s=Create(out var w);Enemy(w,9001,335);
            Call(s,"BeginItemChannel",1,"A104",9001,default(OriginalPoint));Tick(s,w,.02);
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(115));Assert.That(w.UnitState(9001).position.x,Is.EqualTo(355));
            Assert.That(w.UnitState(1).pathingDisabled,Is.True);Tick(s,w,.38);
            Assert.That(w.UnitState(1).pathingDisabled,Is.False);Assert.That(w.UnitState(9001).pathingDisabled,Is.False);
        }

        [Test] public void CrossbowRestoresSurvivingParticipantWhenTheOtherIsRemovedDuringPush()
        {
            foreach(int removed in new[]{1,9001})
            {
                var s=Create(out var w);Enemy(w,9001,335);
                Call(s,"BeginItemChannel",1,"A104",9001,default(OriginalPoint));Tick(s,w,.02);
                int survivor=removed==1?9001:1;
                Assert.That(w.UnitState(survivor).pathingDisabled,Is.True);
                Assert.That(w.RemoveUnit(removed),Is.True);Tick(s,w,.02);
                Assert.That(w.UnitState(survivor).pathingDisabled,Is.False);
            }
        }

        [Test] public void ChannelRenderStateUsesWireSafeIdsAndTravelsWithoutAdvancingGameplay()
        {
            var s=Create(out var w);Enemy(w,9001,555);
            Call(s,"BeginItemChannel",1,"A17V",0,new OriginalPoint(555,1000));Tick(s,w,.3);
            var snapshot=s.Snapshot();var effect=snapshot.effects.Single(x=>x.abilityId=="A17V");
            Assert.That(effect.position.x,Is.EqualTo(345).Within(.001));
            Assert.That(effect.kind,Is.EqualTo(OriginalVisualEffectKind.Orb));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
            var codec=new OriginalUnitySessionCodec();
            var response=new OriginalNetworkResponse {kind=OriginalNetworkResponseKind.Snapshot,snapshot=snapshot,
                assignedSlot=1,acknowledgedSequence=snapshot.players[0].acknowledgedSequence};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out _),Is.True);
            Assert.That(s.Snapshot().effects.Single(x=>x.abilityId=="A17V").position.x,Is.EqualTo(effect.position.x));
        }

        [Test] public void SpaceBootsCollectMovementChargeAndTheirPrimaryWrathExpiresAfterSixSeconds()
        {
            var s=Create(out var w);Equip(s,w,"I05P");Call(s,"AdvanceItemChannels");
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(50));
            w.ForcePosition(1,new OriginalPoint(235,1000));Tick(s,w,.2);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(65));
            var before=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Call(s,"BeginItemChannel",1,"A11S",0,new OriginalPoint(735,1000));Tick(s,w,.12);
            Assert.That(w.UnitState(1).position.x,Is.EqualTo(289));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(50));
            Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.value,Is.EqualTo(before.strength.value+16));
            Tick(s,w,5.88);Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.value,Is.EqualTo(before.strength.value));
        }

        [Test] public void ItemHelperDeathResumesFinalIntermissionOnlyWhenTheHelperActuallyDies()
        {
            foreach(string ability in new[]{"A0CC","A0TX"})
            {
                var s=Create(out var w);var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Private).GetValue(s);
                Call(s,"BeginItemChannel",1,ability,1,w.UnitState(1).position);
                typeof(OriginalMatch).GetProperty("Phase").SetValue(match,OriginalMatchPhase.FinalIntermission);
                typeof(OriginalMatch).GetField("finalSeriesComplete",Private).SetValue(match,true);
                typeof(OriginalMatch).GetField("bossId",Private).SetValue(match,1);
                double duration=ability=="A0CC"?8.5:.04;Tick(s,w,duration-.01);
                Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.FinalIntermission));
                Tick(s,w,.01);Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Combat));
                Assert.That(match.FinalStage,Is.EqualTo(1));Tick(s,w,.1);Assert.That(match.FinalStage,Is.EqualTo(1));
            }
        }

        [Test] public void JetBootsReachTheMovingTargetAndExchangeBothPositionsWithoutDoubleAreaDamage()
        {
            foreach(bool exchange in new[]{false,true})
            {
                var s=Create(out var w);Enemy(w,9001,335);
                Call(s,"BeginItemChannel",1,exchange?"A0NA":"A0FI",9001,default(OriginalPoint));Tick(s,w,.21);
                Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
                Tick(s,w,.03);
                Assert.That(w.UnitState(1).position.x,Is.EqualTo(335));
                Assert.That(w.UnitState(9001).position.x,Is.EqualTo(exchange?135:335));
                Assert.That(w.UnitState(9001).health,Is.EqualTo(exchange?9700:9850));
                Assert.That(w.UnitState(1).pathingDisabled,Is.False);
                Assert.That((bool)Call(s,"ActorWeaponBlocked",9001),Is.True);
            }
        }
        [Test] public void NativeJetTargetingAcceptsAnAlliedHeroAndRejectsSelfBeforeDebit()
        {
            foreach(string id in new[]{"I06J","I08U"})
            {
                var s=Create(out var w);var item=Equip(s,w,id);
                w.AddUnit(2,2,"H024",new OriginalWorldUnitProfile{maxHealth=1000,maxMana=100,collisionRadius=8},new OriginalPoint(135,1200));
                var actor=w.UnitState(1);var profile=actor.profile;profile.maxMana=10000;w.UpdateProfile(1,profile,actor.health,10000);
                var command=new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,
                    sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemSlot=0,itemInstanceId=item.instanceId,
                    targetKind=OriginalWorldTargetKind.Unit,targetId=1};
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.NotReady),id);
                Assert.That(w.UnitState(1).mana,Is.EqualTo(10000));
                command.sequence=s.Snapshot().players[0].acknowledgedSequence+1;command.targetId=2;
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                Assert.That(w.UnitState(1).mana,Is.EqualTo(10000-(id=="I06J"?70:90)));
            }
        }
        [Test] public void MoonNecklaceAddsCappedBaseAttributesForSixSecondsAndCleansItsResistanceMarker()
        {
            var s=Create(out var w);Equip(s,w,"I08D");
            var before=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Call(s,"BeginItemChannel",1,"A0JE",1,default(OriginalPoint));
            var during=(OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1);
            Assert.That(during.strength.Require()-before.strength.Require(),Is.EqualTo(17));
            Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0I4"),Is.True);
            Tick(s,w,6);
            Assert.That(((OriginalHeroStatsSnapshot)Call(s,"HeroCombatStats",1)).strength.Require(),Is.EqualTo(before.strength.Require()));
            Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0I4"),Is.False);
        }
        [Test] public void ErosChargesOnlyEligibleSpellsAndReflectsAfterDamageWithoutAbsorbingIt()
        {
            var s=Create(out var w);Equip(s,w,"I05D");Enemy(w,9001,335);
            Call(s,"NotifyNativeSpellEffect",1,"A0YK");Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
            Call(s,"NotifyNativeSpellEffect",1,"A0CC");Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(1));
            Call(s,"BeginItemChannel",1,"A0YK",0,default(OriginalPoint));
            double health=w.UnitState(1).health;
            Call(s,"ApplyResolvedUnitHit",9001,0,w.UnitState(1),80d,null);
            Assert.That(w.UnitState(1).health,Is.EqualTo(health-80));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9920));
            Call(s,"ApplyResolvedUnitHit",9001,0,w.UnitState(1),170d,null);Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9920),"The equal-pool final hit does not reflect.");
            Assert.That((bool)Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A0YN"),Is.False);
        }
        [Test] public void RedMistTauntsEnemyDealsFourHundredAndRestoresExpiredOrderFreedom()
        {
            var s=Create(out var w);Enemy(w,9001,335);
            Call(s,"BeginItemChannel",1,"A0OS",0,default(OriginalPoint));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000),"wT waits .01 before its damage.");
            Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9600));
            Assert.That(w.UnitState(9001).targetId,Is.EqualTo(1));
            w.Stop(9001);Call(s,"AdvanceItemTauntOrders");Assert.That(w.UnitState(9001).targetId,Is.EqualTo(1));
            Tick(s,w,6.01);Call(s,"AdvanceItemTauntOrders");
            w.Stop(9001);Call(s,"AdvanceItemTauntOrders");Assert.That(w.UnitState(9001).order,Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void DispelRemovesTauntOrderInterceptionAsWellAsItsCastControl()
        {
            var s=Create(out var w);Enemy(w,9001,335);
            Call(s,"BeginItemChannel",1,"A0OU",0,default(OriginalPoint));
            Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.True);
            Call(s,"ClearNegativeActorControls",9001);w.Stop(9001);Call(s,"AdvanceItemTauntOrders");
            Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.False);
            Assert.That(w.UnitState(9001).order,Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void RedMistStillDamagesB03NButDoesNotForceAnOrderOrBlockCasting()
        {
            var s=Create(out var w);Enemy(w,9001,335);
            Call(s,"BeginItemChannel",1,"A0OU",0,default(OriginalPoint));
            Tick(s,w,.01);
            Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.True);w.Stop(9001);
            // The current three heroes cannot produce Siren's B03N. Supply
            // that native predicate in this fixture without inventing an ability grant.
            var catalog=(OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog",Private).GetValue(s);
            catalog.Unit("hfoo").fields.Single(x=>x.key=="abilList").text+=",B03N";
            Call(s,"BeginItemChannel",1,"A0OS",0,default(OriginalPoint));
            Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9600));
            Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.False);
            Assert.That(w.UnitState(9001).order,Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test] public void TauntSourceFilterDistinguishesStunBuffFromOtherWeaponRestrictions()
        {
            foreach(bool stun in new[]{false,true})
            {
                var s=Create(out var w);Enemy(w,9001,335);
                Call(s,"SetActorControl",9001,stun?"native-stun:BPSE:1":"native-abun:test",
                    OriginalActorControlMask.Weapon,5d,true,true);
                Call(s,"BeginItemChannel",1,"A0OS",0,default(OriginalPoint));
                Tick(s,w,.01);
                Assert.That(w.UnitState(9001).health,Is.EqualTo(stun?10000:9600));
                Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.True,"Native buff still intercepts orders; ST excludes only the delayed damage/initial attack.");
            }
        }
        [Test] public void LightningItemHitsExactlyEightDistinctEnemiesFor350()
        {
            var s=Create(out var w);
            for(int i=0;i<9;i++)Enemy(w,9001+i,335+i*70);
            Call(s,"BeginItemChannel",1,"A028",9001,default(OriginalPoint));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9650));Tick(s,w,2.41);
            Assert.That(w.Snapshot().units.Count(u=>u.ownerSlot==0&&u.health==9650),Is.EqualTo(8));
            Assert.That(w.UnitState(9009).health,Is.EqualTo(10000));
        }
        [Test] public void ChargeBladeAccumulatesMovementCapsAndOnlyWeaponHitReleasesTheCharge()
        {
            var s=Create(out var w);Equip(s,w,"I07Y");
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
            for(int i=1;i<=3;i++){w.ForcePosition(1,new OriginalPoint(135+i*1000,1000));Tick(s,w,.2);}
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(350));
            Enemy(w,9001,3335);
            Call(s,"ObserveChargeBladeWeapon",1,w.UnitState(9001),10d,false);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(350));
            Call(s,"ApplyResolvedWeaponOrSpellHit",1,1,w.UnitState(9001),10d,null,true);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
            Assert.That(w.UnitState(9001).health,Is.InRange(9290d,9640d));
            Assert.That(Inventory(s).HeroSlots[0].removed,Is.False);
        }
        [Test] public void ChargeBladeIgnoresLargeTeleportsAndManualCastUsesCurrentCharge()
        {
            var s=Create(out var w);Equip(s,w,"I07Y");
            w.ForcePosition(1,new OriginalPoint(1335,1000));Tick(s,w,.2);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero,"180 or more per sampling step is excluded.");
            w.ForcePosition(1,new OriginalPoint(1435,1000));Tick(s,w,.2);
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.EqualTo(15));Enemy(w,9001,1635);
            Call(s,"BeginItemChannel",1,"A028",9001,default(OriginalPoint));
            Assert.That(w.UnitState(9001).health,Is.InRange(9970d,9985d));
            Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
        }
        [Test] public void PhysicalOffenseItemsUseExactManaAndCooldownAndKeepTheirZeroChargeInstance()
        {
            foreach(var id in new[]{"I015","I07Y","I02C"})
            {
                var s=Create(out var w);var item=Equip(s,w,id);Enemy(w,9001,335);
                var hero=w.UnitState(1);var profile=hero.profile;profile.maxMana=10000;w.UpdateProfile(1,profile,hero.health,10000);
                bool wave=id=="I02C";
                var command=new OriginalSessionCommand{kind=OriginalSessionCommandKind.UseItem,
                    sequence=s.Snapshot().players[0].acknowledgedSequence+1,itemSlot=0,itemInstanceId=item.instanceId,
                    targetKind=wave?OriginalWorldTargetKind.None:OriginalWorldTargetKind.Unit,targetId=wave?0:9001,x=335,y=1000};
                Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.Accepted),id);
                Assert.That(w.UnitState(1).mana,Is.EqualTo(10000-(wave?400:100)),id);
                Assert.That(s.Snapshot().players[0].itemUses[0].cooldownRemaining,Is.EqualTo(wave?18:12),id);
                Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
                Assert.That(Inventory(s).HeroSlots[0].charges,Is.Zero);
                Assert.That(w.UnitState(9001).health,Is.EqualTo(id=="I015"?9650:10000),id);
                command.sequence++;Assert.That(s.Apply(0,command),Is.EqualTo(OriginalSessionReplyCode.NotReady));
                Assert.That(w.UnitState(1).mana,Is.EqualTo(10000-(wave?400:100)),id);
            }
        }
        [Test] public void NativeShockZeroUsesSeparateSweptAreaAndNeverSubtractsHealth()
        {
            var s=Create(out var w);Enemy(w,9001,335);Enemy(w,9002,1020);Enemy(w,9003,1050);
            Ally(w,100000051,500);
            int hits=(int)Call(s,"ApplyItemShockNative",1,new OriginalPoint(335,1000));
            Assert.That(hits,Is.EqualTo(2));
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
            Assert.That(w.UnitState(9002).health,Is.EqualTo(10000));
            Assert.That(w.UnitState(100000051).health,Is.EqualTo(100));
        }

        [Test] public void FireWaveMovesBeforeHitAndRetainsOriginalLastStepOvershoot()
        {
            var s=Create(out var w);Enemy(w,9001,335);Enemy(w,9002,980);Enemy(w,9003,1000);
            Call(s,"BeginItemChannel",1,"A08A",0,new OriginalPoint(735,1000));Tick(s,w,.05);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(9400));Tick(s,w,.66);
            Assert.That(w.UnitState(9002).health,Is.EqualTo(9400));
            Assert.That(w.UnitState(9003).health,Is.EqualTo(10000));
            Tick(s,w,1);Assert.That(w.UnitState(9001).health,Is.EqualTo(9400),"Each victim is hit once.");
        }
        [Test] public void PausedTauntRecipientKeepsNativeBuffButSkipsDelayedMistDamage()
        {
            var s=Create(out var w);Enemy(w,9001,335);
            w.SetUnitState(9001,paused:true);
            Call(s,"BeginItemChannel",1,"A0OS",0,default(OriginalPoint));Tick(s,w,.01);
            Assert.That(w.UnitState(9001).health,Is.EqualTo(10000));
            Assert.That((bool)Call(s,"ActorCastBlocked",9001),Is.True);
            w.SetUnitState(9001,paused:false);Call(s,"AdvanceItemTauntOrders");
            Assert.That(w.UnitState(9001).targetId,Is.EqualTo(1));
        }
    }
}

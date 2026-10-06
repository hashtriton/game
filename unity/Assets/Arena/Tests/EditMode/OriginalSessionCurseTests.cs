using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using static Arena.Tests.OriginalWorldOptionTestSupport;

namespace Arena.Tests
{
    public sealed class OriginalSessionCurseTests
    {
        static bool CurseId(string id)=>id!=null&&id.Length==4&&string.CompareOrdinal(id,"A19L")>=0&&string.CompareOrdinal(id,"A19T")<=0;
        static void Grant(OriginalSession s,int slot,string id)=>Call(s,"GrantWorldCurse",Player(s,slot),id);
        [Test] public void CurseOptionAssignsUniqueMarkersAndReservesDeathBondForLargeParties()
        {
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);options.curse=true;
            var three=Create(options,3).Snapshot();
            var ids=three.players.Select(p=>p.auxiliaryAbilities.Where(a=>CurseId(a.id)).Single().id).ToArray();
            Assert.That(ids.Distinct().Count(),Is.EqualTo(3));Assert.That(ids,Does.Not.Contain("A19T"));
            var six=Create(options,6).Snapshot();
            ids=six.players.Select(p=>p.auxiliaryAbilities.Where(a=>CurseId(a.id)).Single().id).ToArray();
            Assert.That(ids.Count(i=>i=="A19T"),Is.EqualTo(2));Assert.That(ids.Where(i=>i!="A19T").Distinct().Count(),Is.EqualTo(4));
        }
        [Test] public void HeavySoulClampsNativeMovementAndGrantsItsDeclaredArmorCompanion()
        {
            var s=Create();Grant(s,1,"A19L");var actor=World(s).UnitState(1);
            var profile=actor.profile;profile.moveSpeed=400;World(s).UpdateProfile(1,profile,actor.health,actor.mana);
            AdvanceWorldOnly(s,1.05,"AdvanceWorldCurses");
            Assert.That(World(s).UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Assert.That(Call(s,"CurseArmorFraction",1),Is.EqualTo(.3));
        }
        [Test] public void HeavySoulRecomputesSpeedAfterRemovingHasteBeforeDecidingItsClamp()
        {
            var s=Create();Grant(s,1,"A19L");
            Call(s,"AddPyroChainBuff",World(s).UnitState(1),10d);
            Assert.That(World(s).UnitState(1).profile.moveSpeed,Is.EqualTo(187.5));
            Call(s,"ApplyItemHaste",1,10d,.5d);
            Assert.That(World(s).UnitState(1).profile.moveSpeed,Is.EqualTo(312.5));
            AdvanceWorldOnly(s,1.05,"AdvanceWorldCurses");
            Assert.That(World(s).UnitState(1).profile.moveSpeed,Is.EqualTo(187.5));
            Call(s,"RefreshAbilityMovement",1);
            Assert.That(World(s).UnitState(1).profile.moveSpeed,Is.EqualTo(187.5));
        }
        [Test] public void ContagiousPoisonDrainsEachEligibleAllyOnceWithoutAWeaponDamageEvent()
        {
            var s=Create(count:3);Grant(s,1,"A19N");var w=World(s);var a=w.UnitState(2);
            w.UpdateProfile(2,a.profile,100,a.mana);w.DrainEvents();
            AdvanceWorldOnly(s,1.05,"AdvanceWorldCurses");
            Assert.That(w.UnitState(2).health,Is.EqualTo(100-.02*a.profile.maxHealth).Within(1e-6));
            Assert.That(w.UnitState(1).health,Is.GreaterThan(100));
            Assert.That(w.DrainEvents().Any(e=>e.kind==OriginalWorldEventKind.UnitDied),Is.False);
        }
        [Test] public void ColdCurseRootsAQualifiedCasterAndReturnsFourManaPulses()
        {
            var s=Create();Grant(s,1,"A19M");var w=World(s);var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,actor.health,20);
            Call(s,"NotifyNativeSpellEffect",1,"A102");
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.True);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);Assert.That(Call(s,"ActorItemBlocked",1),Is.False);
            AdvanceWorldOnly(s,5.1,"AdvanceWorldCurses");
            Assert.That(w.UnitState(1).mana,Is.EqualTo(20+actor.profile.maxMana*.03*4).Within(1e-6));
        }
        [Test] public void ColdCurseDoesNotTriggerInsideOriginalExcludedSector()
        {
            var s=Create();Grant(s,1,"A19M");World(s).Relocate(1,new OriginalPoint(200,700));
            Call(s,"NotifyNativeSpellEffect",1,"A102");Assert.That(Call(s,"ActorMoveBlocked",1),Is.False);
        }
        [Test] public void ColdEnsnareRejectsInvulnerableRecipientButItsScriptManaTimerStillRuns()
        {
            var s=Create();Grant(s,1,"A19M");var w=World(s);var actor=w.UnitState(1);
            w.UpdateProfile(1,actor.profile,actor.health,20);w.SetTemporaryInvulnerability(1,"test:AIvu",true);
            Call(s,"NotifyNativeSpellEffect",1,"A102");
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.False);
            AdvanceWorldOnly(s,5.1,"AdvanceWorldCurses");
            Assert.That(w.UnitState(1).mana,Is.EqualTo(20+actor.profile.maxMana*.03*4).Within(1e-6));
        }
        [Test] public void StoneSkinBlocksFourPhysicalSlotsAndOneMegaDeathFreesOnlyOne()
        {
            var s=Create();Grant(s,1,"A19R");var inventory=Inventory(s,1);
            Assert.That(inventory.HeroSlots.Count(i=>i?.itemId=="I09D"),Is.EqualTo(4));
            Assert.That(inventory.Drop(OriginalInventoryBag.Hero,2).Code,Is.EqualTo(OriginalItemActionCode.NotAllowed));
            Assert.That(inventory.Transfer(OriginalInventoryBag.Hero,2).Code,Is.EqualTo(OriginalItemActionCode.NotAllowed));
            Assert.That(Call(s,"CasterMagicImmune",World(s).UnitState(1)),Is.True);
            Call(s,"ObserveCurseMegaDeath",2);
            Assert.That(Inventory(s,1).HeroSlots.Count(i=>i?.itemId=="I09D"),Is.EqualTo(3));
            Assert.That(Call(s,"HeroCombatStats",1),Is.Not.Null,"Slot blocks have no invented equipment stats.");
        }
        [Test] public void DarkSealUsesNativeMaskEightToBlockItemsWhileCastingRemainsAllowed()
        {
            var s=Create();Grant(s,1,"A19S");AdvanceWorldOnly(s,1.05,"AdvanceWorldCurses");
            Assert.That(Call(s,"ActorItemBlocked",1),Is.True);Assert.That(Call(s,"ActorCastBlocked",1),Is.False);
        }
        [Test] public void BloodPoisonUsesSourceDayTriangleAndNativeDayLength()
        {
            var s=Create();Grant(s,1,"A19Q");var w=World(s);var actor=w.UnitState(1);w.UpdateProfile(1,actor.profile,300,actor.mana);
            // Source time starts at6; at60s it is9, losing1%maxHP per1s.
            Call(s,"ApplyBloodPoisonSecond",60d);Assert.That(w.UnitState(1).health,Is.EqualTo(300-.01*actor.profile.maxHealth).Within(1e-6));
            Call(s,"ApplyBloodPoisonSecond",180d);Assert.That(w.UnitState(1).health,Is.EqualTo(300).Within(1e-6));
            Call(s,"ApplyBloodPoisonSecond",360d);Assert.That(w.UnitState(1).health,Is.EqualTo(300).Within(1e-6));
        }
        [Test] public void DeathBondKillsOnlyOtherLivingCursedHeroes()
        {
            var s=Create(count:3);Grant(s,1,"A19T");Grant(s,2,"A19T");var w=World(s);
            Assert.That(w.ForceUnitDeath(1),Is.True);Call(s,"ObserveDeathBonds",1);
            Assert.That(w.UnitState(2).health,Is.EqualTo(0));Assert.That(w.UnitState(3).health,Is.GreaterThan(.405));
            Call(s,"ObserveDeathBonds",1);Assert.That(w.UnitState(2).health,Is.EqualTo(0));
        }
        [Test] public void StoneBlocksDoNotPreventEquipmentTransactionsOrAuthoritativeWire()
        {
            var s=Create();Grant(s,1,"A19R");var player=Player(s,1);var before=Inventory(s,1);var candidate=before.Copy();
            Assert.That(candidate.TryPickup(candidate.CreateInstance("I04P")).Applied,Is.True);
            Assert.That(Call(s,"ApplyEquipmentProfile",player,candidate,World(s).UnitState(1)),Is.True);
            player.GetType().GetField("inventory").SetValue(player,candidate);
            Assert.That(s.Snapshot().players[0].inventory.heroSlots.Count(i=>i?.itemId=="I09D"),Is.EqualTo(4));
            var codec=new OriginalUnitySessionCodec();var reply=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,code=OriginalSessionReplyCode.Accepted,assignedSlot=1,acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence,snapshot=s.Snapshot()};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out _),Is.True);
        }
        [Test] public void FireScarAmplifiesSourceDamageAndAppliesItsOwnUniversalBacklash()
        {
            var s=Create();Grant(s,1,"A19O");var actor=World(s).UnitState(1);
            var effect=Call(s,"PrepareItemSpellEffect",1,100d);
            Assert.That((double)effect.GetType().GetField("damage",Hidden).GetValue(effect),Is.EqualTo(120));
            Call(s,"CompleteItemSpellEffect",1,1,effect);
            Assert.That(actor.health-World(s).UnitState(1).health,Is.EqualTo(12).Within(1e-6));
        }
        [Test] public void MirrorInvertsSourceVampHealing()
        {
            var s=Create();Grant(s,1,"A19P");var actor=World(s).UnitState(1);World(s).UpdateProfile(1,actor.profile,300,actor.mana);
            var effect=Call(s,"PrepareItemSpellEffect",1,100d);
            effect.GetType().GetField("vamp",Hidden).SetValue(effect,.1);
            Call(s,"CompleteItemSpellEffect",1,1,effect);
            Assert.That(World(s).UnitState(1).health,Is.EqualTo(290).Within(1e-6));
        }
        [Test] public void AllAssignedCursesUseTheProductionCodecWithoutRelaxingRanks()
        {
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Nightmare);var s=Create(options,8);
            var codec=new OriginalUnitySessionCodec();var reply=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,code=OriginalSessionReplyCode.Accepted,assignedSlot=1,acknowledgedSequence=s.Snapshot().players[0].acknowledgedSequence,snapshot=s.Snapshot()};
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out var decoded),Is.True);
            foreach(var player in decoded.snapshot.players)Assert.That(player.auxiliaryAbilities.Count(a=>CurseId(a.id)),Is.EqualTo(1));
            var curse=reply.snapshot.players[0].auxiliaryAbilities.Single(a=>CurseId(a.id));curse.rank=2;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(reply),out _),Is.False);
        }
        [Test] public void DuelCurseChangesRemoveInactiveMarkersAndAddTheNewRecipientOverlay()
        {
            var s=Create(count:3);Grant(s,1,"A19Q");var w=World(s);
            Assert.That(Call(s,"ApplyDuelWorldBatch",new object[]{new[]{
                new OriginalDuelEvent{kind=OriginalDuelEventKind.Curse,slot=1,amount=0,code="A19Q"},
                new OriginalDuelEvent{kind=OriginalDuelEventKind.Curse,slot=2,amount=1,code="A19Q"}}}),Is.True);
            Assert.That(Call(s,"HasWorldCurse",1,"A19Q"),Is.False);
            Assert.That(s.Snapshot().players[0].auxiliaryAbilities.Any(a=>a.id=="A19Q"),Is.False);
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(1),"A19Q"),Is.False);
            Assert.That(Call(s,"HasEffectiveUnitAbility",w.UnitState(2),"A19Q"),Is.True);
            var first=w.UnitState(1);var second=w.UnitState(2);w.UpdateProfile(1,first.profile,300,first.mana);w.UpdateProfile(2,second.profile,300,second.mana);
            Call(s,"ApplyBloodPoisonSecond",60d);
            Assert.That(w.UnitState(1).health,Is.EqualTo(300));Assert.That(w.UnitState(2).health,Is.EqualTo(300-.01*second.profile.maxHealth).Within(1e-6));
        }
    }
}

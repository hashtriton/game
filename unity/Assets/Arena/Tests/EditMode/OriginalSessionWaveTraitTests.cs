using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionWaveTraitTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s,string name,params object[] args) => typeof(OriginalSession).GetMethod(name,Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld w)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);return s;
        }
        static void Spawn(OriginalWorld w,int id,string rawcode="hfoo") => w.AddUnit(id,0,rawcode,
            new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(300+(id-1001)*100,1000));
        static void Tick(OriginalSession s,OriginalWorld w,double seconds)
        { while(seconds>1e-9) { double step=Math.Min(.05,seconds); w.Advance(step);Call(s,"AdvanceWaveTraits",step);seconds-=step; } }
        static OriginalInventory Inventory(OriginalSession s)
        {
            var p=((IList)typeof(OriginalSession).GetField("players",Hidden).GetValue(s))[0];
            return (OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
        }
        [Test] public void RandomWaveTraitsAreDistinctAndApplyOnlyToOrdinarySpawnInstances()
        {
            var s=Create(out var w);Call(s,"BeginWaveTraits",24);Spawn(w,1001);Spawn(w,1002);
            Call(s,"ApplyWaveSpawnAbilities",w.UnitState(1001));Call(s,"ApplyWaveSpawnAbilities",w.UnitState(1002));
            var a=((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(1001))).Where(x=>x.StartsWith("A15")).ToArray();
            Assert.That(a.Length,Is.EqualTo(3));Assert.That(a.Distinct().Count(),Is.EqualTo(3));
            Assert.That((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(1002)),Is.EquivalentTo((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(1001))));
            Call(s,"BeginWaveTraits",29);Spawn(w,1003);Call(s,"ApplyWaveSpawnAbilities",w.UnitState(1003));
            Assert.That(((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(1003))).Count(x=>x.StartsWith("A15")),Is.EqualTo(4));
            Assert.That(((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(1))).Any(x=>x.StartsWith("A15")),Is.False);
        }
        [Test] public void DeathExplosionIsDelayedOnceAndRemovedWaveHelperCancelsQueuedDamage()
        {
            var s=Create(out var w);Call(s,"BeginWaveTraits",7);Spawn(w,1001);Spawn(w,1002);
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15K"},Array.Empty<string>());
            Call(s,"ApplyUnitAbilityOverlay",1002,new[]{"A15K"},Array.Empty<string>());
            double hp=w.UnitState(1).health;
            Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(1001),1000.0,null);
            Call(s,"ObserveWaveTraitDeath",w.UnitState(1001),1);
            Tick(s,w,1.49);Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Tick(s,w,.02);Assert.That(w.UnitState(1).health,Is.EqualTo(hp-80).Within(1e-6));
            w.ForcePosition(1002,new OriginalPoint(335,1000));
            Call(s,"ApplyResolvedUnitHit",1,1,w.UnitState(1002),1000.0,null);
            Call(s,"EndWaveTraits");Tick(s,w,2.0);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-80).Within(1e-6));
        }
        [Test] public void DarkRanksFollowSourceRegionAndResetDeadHeroesWithoutFixingSourceTypo()
        {
            var s=Create(out var w);var inventory=Inventory(s);
            inventory.TryPickup(inventory.CreateInstance("I00H"),OriginalInventoryBag.Hero);
            inventory.TryPickup(inventory.CreateInstance("I08M"),OriginalInventoryBag.Hero);
            Call(s,"BeginWaveTraits",4);Tick(s,w,2.01);
            Assert.That(Call(s,"WaveTraitItemAbilityRank",1,"A00W",1),Is.EqualTo(2));
            Assert.That(Call(s,"WaveTraitItemAbilityRank",1,"A0M1",1),Is.EqualTo(1),"Source literal I0M1 is absent; do not replace it with A0M1.");
            w.ForceUnitDeath(1);Call(s,"EndWaveTraits");
            Assert.That(Call(s,"WaveTraitItemAbilityRank",1,"A00W",1),Is.EqualTo(1),"E8v has no living-hero filter.");
        }
        [Test] public void DarkAuraMarkerIncludesInvulnerableEnemiesAndEndsWithItsHelper()
        {
            var s=Create(out var w);Spawn(w,1001);Call(s,"BeginWaveTraits",4);
            Assert.That(Call(s,"HasWaveDarkBuff",1),Is.True);Assert.That(Call(s,"HasWaveDarkBuff",1001),Is.False);
            w.SetUnitState(1,invulnerable:true);Assert.That(Call(s,"HasWaveDarkBuff",1),Is.True);
            w.SetVisibility(1,false);Assert.That(Call(s,"HasWaveDarkBuff",1),Is.False);
            w.SetVisibility(1,true);Call(s,"EndWaveTraits");Assert.That(Call(s,"HasWaveDarkBuff",1),Is.False);
        }
        [Test] public void TemporaryItemDefendCanAmplifyMagicAndRestoresAfterRemoval()
        {
            var s=Create(out var w);
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A0X3"},Array.Empty<string>());
            Assert.That(Call(s,"NativeDefendItemSpellFactor",w.UnitState(1)),Is.EqualTo(1.3));
            Call(s,"ApplyUnitAbilityOverlay",1,new[]{"A0X4"},Array.Empty<string>());
            Assert.That(Call(s,"NativeDefendItemSpellFactor",w.UnitState(1)),Is.EqualTo(.7));
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A0X4"});
            Assert.That(Call(s,"NativeDefendItemSpellFactor",w.UnitState(1)),Is.EqualTo(1.3));
            Call(s,"ApplyUnitAbilityOverlay",1,Array.Empty<string>(),new[]{"A0X3"});
            Assert.That(Call(s,"NativeDefendItemSpellFactor",w.UnitState(1)),Is.EqualTo(1));
        }
        [Test] public void NativeBloodlustAddsAttackSpeedWithoutMovementAndExpiresAfterHelperRemoval()
        {
            var s=Create(out var w);Spawn(w,1001);Spawn(w,1002);
            Call(s,"ApplyUnitAbilityOverlay",1001,new[]{"A15E"},Array.Empty<string>());
            double before=(double)Call(s,"WeaponRate",w.UnitState(1001));
            Call(s,"BeginWaveTraits",1);Tick(s,w,6.01);
            Assert.That(Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(before+1));
            Assert.That(Call(s,"WeaponRate",w.UnitState(1002)),Is.EqualTo(before));
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.Zero);
            Call(s,"EndWaveTraits");Tick(s,w,2.01);
            Assert.That(Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(before));
        }
        [Test] public void NativeWaveSilenceBlocksOnlyCastsAndDeathRootArrivesAfterMissileTravel()
        {
            var s=Create(out var w);Call(s,"BeginWaveTraits",9);Tick(s,w,8.01);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.True);Assert.That(Call(s,"ActorWeaponBlocked",1),Is.False);
            Assert.That(Call(s,"ActorMoveBlocked",1),Is.False);
            Call(s,"ClearNegativeActorControls",1);
            Call(s,"ApplyWaveRoot",1);
            Assert.That(Call(s,"IsNativeRooted",1),Is.False,"Aens missile is not an immediate root.");
            Tick(s,w,.3);Assert.That(Call(s,"IsNativeRooted",1),Is.True);
            Assert.That(Call(s,"ActorCastBlocked",1),Is.False);Assert.That(Call(s,"ActorWeaponBlocked",1),Is.False);
            Tick(s,w,2.01);Call(s,"AdvanceActorControls",2.01);
            Assert.That(Call(s,"IsNativeRooted",1),Is.False);
        }
        [Test] public void AIddUsesMeasuredSpellFactorsWithoutChangingPhysicalOrUniversalDamage()
        {
            foreach(var row in new[]{("A15I",.2),("A09A",.5),("A0RH",.25)})
            {
                var s=Create(out var w);Spawn(w,1001);
                Call(s,"ApplyUnitAbilityOverlay",1001,new[]{row.Item1},Array.Empty<string>());
                double hp=w.UnitState(1001).health;
                Call(s,"ApplyNativeTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellMagic);
                Assert.That(hp-w.UnitState(1001).health,Is.EqualTo(40*row.Item2).Within(1e-7));
                hp=w.UnitState(1001).health;
                Call(s,"ApplyNativeTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.SpellNormal);
                Assert.That(hp-w.UnitState(1001).health,Is.EqualTo(40/1.12*row.Item2).Within(1e-7));
                hp=w.UnitState(1001).health;
                Call(s,"ApplyNativeTriggeredHit",1,1,w.UnitState(1001),40.0,OriginalTriggeredDamageMode.ChaosUniversal);
                Assert.That(hp-w.UnitState(1001).health,Is.EqualTo(40).Within(1e-7));
            }
        }
    }
}

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;
namespace Arena.Tests {
 public sealed class OriginalItemNativeAbilityTests {
  static OriginalItemPassiveCatalog Source()=>JsonUtility.FromJson<OriginalItemPassiveCatalog>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-item-passives.json")));
  static OriginalInventorySnapshot Bag()=>new OriginalInventorySnapshot {ownerId=1,heroSlots=new OriginalItemInstance[6],servantSlots=new OriginalItemInstance[6]};
  static OriginalItemInstance Item(string id,long instance)=>new OriginalItemInstance{ownerId=1,itemId=id,instanceId=instance};
  [Test] public void OrderedItemAbilitiesPreserveCopiesExcludeServantAndDetachEvidence() {
   var source=Source();var effects=new OriginalInventoryEffects(source);var bag=Bag();
   bag.heroSlots[0]=Item("I008",2);bag.heroSlots[3]=Item("I008",3);bag.servantSlots[0]=Item("I008",4);
   var method=typeof(OriginalInventoryEffects).GetMethod("CombatAbilities");Assert.That(method,Is.Not.Null,"Missing item-native ability dispatch boundary.");
   var rows=(IList)method.Invoke(effects,new object[]{bag});Assert.That(rows.Count,Is.EqualTo(2));
   object Read(object row,string field)=>row.GetType().GetField(field).GetValue(row);
   Assert.That(Read(rows[0],"instanceId"),Is.EqualTo(2L));Assert.That(Read(rows[1],"instanceId"),Is.EqualTo(3L));
   Assert.That(Read(rows[0],"abilityId"),Is.EqualTo("A00I"));Assert.That(Read(rows[0],"rank"),Is.EqualTo(1));
   rows[0].GetType().GetField("abilityId").SetValue(rows[0],"evil");source.items.Single(x=>x.id=="I008").abilityIds[0]="evil";
   rows=(IList)method.Invoke(effects,new object[]{bag});Assert.That(Read(rows[0],"abilityId"),Is.EqualTo("A00I"));
  }
  [Test] public void MeasuredCriticalItemHasZeroIntrinsicStatsWithoutPretendingItsProcIsAFlatBonus() {
   var effects=new OriginalInventoryEffects(Source());var bag=Bag();bag.heroSlots[0]=Item("I008",1);
   var plan=effects.Plan(bag);Assert.That(plan.CanApplyProfile,Is.True);
   Assert.That(plan.NativeProfile.attackDamage,Is.Zero);Assert.That(plan.NativeProfile.strength,Is.Zero);
   Assert.That(plan.NativeProfile.maxHealthFlat,Is.Zero);Assert.That(plan.EffectsComplete,Is.False);
   var vitality=effects.ChangeVitality("I008",true,100.25,631,50.5,145,8,10);
   Assert.That(vitality.known,Is.True);Assert.That(vitality.health,Is.EqualTo(100.25));Assert.That(vitality.mana,Is.EqualTo(50.5));
  }
  [Test] public void LifestealAndScriptActivesCanEquipTheirMeasuredIntrinsicProfiles() {
   var effects=new OriginalInventoryEffects(Source());
   foreach(var id in new[]{"I08M","I00H","I03S","I0AE","I0B7","I06J","I00T","I00W","I00Z","I09A"}) {
    var bag=Bag();bag.heroSlots[0]=Item(id,1);var plan=effects.Plan(bag);
    Assert.That(plan.CanApplyProfile,Is.True,id+" still rejects its measured intrinsic profile");
    Assert.That(plan.EffectsComplete,Is.False,"Script/native coverage remains explicit");
   }
  }
  [Test] public void DeclaredOrbDamageIsAddedAndSpellbookChildRankCannotReuseMeasuredProfile() {
   var source=Source();var effects=new OriginalInventoryEffects(source);var bag=Bag();bag.heroSlots[0]=Item("I02A",1);
   var plan=effects.Plan(bag);Assert.That(plan.CanApplyProfile,Is.True,"Orb intrinsic profile blocked");
   Assert.That(plan.NativeProfile.attackDamage,Is.EqualTo(15));
   Assert.That(plan.DeclaredModifiers.Any(m=>m.AbilityId=="A07K" && m.NativeField=="Idam" && m.Value==15),Is.True);
   Assert.That(plan.EffectsComplete,Is.False,"Orb on-hit semantics must remain separately accounted");
   bag.heroSlots[0]=Item("I05A",1);var row=source.observedAdditionalEquip.SelectMany(b=>b.items).Single(x=>x.id=="I05A");
   var measured=row.snapshots[1];var direct=source.items.Single(x=>x.id=="I05A").abilityIds;
   string child=row.abilityIds.First(x=>!direct.Contains(x));
   int? Rank(long id,string ability)=>ability==child?2:measured.abilityRanks[Array.IndexOf(row.abilityIds,ability)];
   Assert.That(effects.Plan(bag,Rank).CanApplyProfile,Is.False,"Changed spellbook child retained baseline profile");
  }

  [Test] public void MeasuredIntrinsicProfilesDoNotDependOnUnrelatedActivationHandlers() {
   var effects=new OriginalInventoryEffects(Source());
   foreach(string id in new[]{"I015","I017","I01B","I02C","I03N","I03Y","I04B","I054","I05E","I07N","I07P","I07Y","I082","I08D","I0AL","I0AP","I06B","I01L","I06M","I06O","I0AJ"}) {
    var bag=Bag();bag.heroSlots[0]=Item(id,1);var p=effects.Plan(bag);
    Assert.That(p.CanApplyProfile,Is.True,id);Assert.That(p.EffectsComplete,Is.False,id);
   }
  }
  [Test] public void MeasuredSparseCarapaceAddsNoArmorBeyondTheDeclaredOtherModifiers() {
   var effects=new OriginalInventoryEffects(Source());
   foreach(var row in new[]{("I029",5d,200d),("I05D",8d,300d)}) {
    var bag=Bag();bag.heroSlots[0]=Item(row.Item1,1);var p=effects.Plan(bag);
    Assert.That(p.CanApplyProfile,Is.True,row.Item1);Assert.That(p.NativeProfile.armor,Is.EqualTo(row.Item2));
    Assert.That(p.NativeProfile.maxHealthFlat,Is.EqualTo(row.Item3));Assert.That(p.EffectsComplete,Is.False);
   }
  }

 }
}

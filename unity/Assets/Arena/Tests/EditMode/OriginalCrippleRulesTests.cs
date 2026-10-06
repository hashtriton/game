using System;
using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalCrippleRulesTests
    {
        static OriginalCombatCatalog Catalog() => JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(Path.Combine(Application.dataPath,"Arena/Data/lia39-combat.json")));
        [Test] public void NativeFiveControlledWhiteAndItemDamagePairsMatchAfterArmor()
        {
            var rule=new OriginalCrippleRules(Catalog());
            double[] white={56,86,56,12,13}, flat={0,0,12,0,0};
            double[] native={24.999998092651367,38.39285659790039,35.71428298950195,5.357142448425293,5.357142448425293};
            for(int i=0;i<white.Length;i++) Assert.That(rule.WeaponDamage(white[i],flat[i])/1.12,Is.EqualTo(native[i]).Within(.00001));
            Assert.That(rule.duration,Is.EqualTo(6));Assert.That(rule.heroDuration,Is.EqualTo(6));
        }
        [Test] public void ItemFlatBonusIsNotMistakenForPrimaryAttributeDamage()
        {
            var rule=new OriginalCrippleRules(Catalog());
            Assert.That(rule.WeaponDamage(56+30,0)-rule.WeaponDamage(56,0),Is.EqualTo(15));
            Assert.That(rule.WeaponDamage(56,12)-rule.WeaponDamage(56,0),Is.EqualTo(12));
        }
        [Test] public void ConflictingSourceDefinitionAndNonFiniteInputAreRejected()
        {
            var catalog=Catalog();var field=Array.Find(catalog.Ability("A103").fields,f=>f.key=="DataC1");field.number=.3;
            Assert.Throws<InvalidOperationException>(()=>new OriginalCrippleRules(catalog));
            var rule=new OriginalCrippleRules(Catalog());
            Assert.Throws<ArgumentOutOfRangeException>(()=>rule.WeaponDamage(double.NaN,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>rule.WeaponDamage(5,double.PositiveInfinity));
        }
    }
}

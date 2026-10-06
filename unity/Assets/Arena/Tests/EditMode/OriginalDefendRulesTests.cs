using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalDefendRulesTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        [Test] public void DirectDamageMatchesNativeFortyDamageProbeAcrossAllThreeRanksAndFourTypes()
        {
            var combat = Load<OriginalCombatCatalog>("lia39-combat"); var native = Load<OriginalNativeCatalog>("lia39-native126");
            string[] types = { "spells", "normal", "pierce", "magic" };
            // Immediate HP deltas in LiADef1.w3v direct{0,1,2,4}_r{1,2,3}_on.
            double[,] expected = { {16.75396728515625,26.17803955078125,9.162353515625,16.75396728515625},
                {12.56549072265625,26.17803955078125,5.4974365234375,12.56549072265625},
                {8.37701416015625,26.17803955078125,1.83251953125,8.37701416015625} };
            for (int rank = 1; rank <= 3; rank++)
                for (int type = 0; type < types.Length; type++)
                {
                    var rules = new OriginalDefendRules(combat, rank);
                    double damage = 40 * OriginalAttackRules.DamageTypeMultiplier(native, types[type], "hero") *
                        OriginalAttackRules.ArmorMultiplier(native, 8.8) * rules.IncomingMultiplier(types[type]);
                    Assert.That(damage, Is.EqualTo(expected[rank - 1, type]).Within(.0001));
                }
        }
        [Test] public void MovementMultipliersMatchNativeSpeedGetterAndActualMotionFamily()
        {
            var combat = Load<OriginalCombatCatalog>("lia39-combat");
            double[] measured = { 174.99998474121094, 187.5, 199.99998474121094 };
            for (int rank = 1; rank <= 3; rank++)
                Assert.That(250 * new OriginalDefendRules(combat, rank).movementMultiplier, Is.EqualTo(measured[rank - 1]).Within(.0001));
        }
        [Test] public void ActualPierceProjectileFloorIsOneWithoutConvertingZeroDamageToAHit()
        {
            var combat = Load<OriginalCombatCatalog>("lia39-combat");
            double[] ordinary = {6.413612365722656,6.871727466583252,5.955496788024902,5.039266586303711,5.497382164001465};
            foreach (double damage in ordinary)
            {
                Assert.That(new OriginalDefendRules(combat, 1).IncomingWeaponDamage(damage, "pierce"), Is.EqualTo(damage * .5));
                Assert.That(new OriginalDefendRules(combat, 3).IncomingWeaponDamage(damage, "pierce"), Is.EqualTo(1));
            }
            Assert.That(new OriginalDefendRules(combat, 3).IncomingWeaponDamage(0, "pierce"), Is.Zero);
            Assert.Throws<ArgumentException>(() => new OriginalDefendRules(combat, 1).IncomingWeaponDamage(10, "spells"));
        }
        [Test] public void MissingConflictedAndNonfiniteCoefficientsCannotSilentlyBecomeShieldDefaults()
        {
            var combat = Load<OriginalCombatCatalog>("lia39-combat"); var coefficient = combat.Ability("A05M").fields.Single(f => f.key == "DataC1");
            coefficient.number = double.NaN; Assert.Throws<InvalidOperationException>(() => new OriginalDefendRules(combat, 1));
            coefficient.number = .3; coefficient.conflict = true; Assert.Throws<InvalidOperationException>(() => new OriginalDefendRules(combat, 1));
            coefficient.conflict = false; coefficient.number = 1.01; Assert.Throws<InvalidOperationException>(() => new OriginalDefendRules(combat, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OriginalDefendRules(combat, 4));
        }
    }
}

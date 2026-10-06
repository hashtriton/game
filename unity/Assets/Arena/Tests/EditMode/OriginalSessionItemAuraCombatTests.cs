using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemAuraCombatTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, Hidden).Invoke(s, args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var s = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Hidden).GetValue(s);
            world.AddUnit(1001, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 10000, collisionRadius = 31 }, new OriginalPoint(235, 1000));
            return s;
        }
        static void Axes(OriginalSession s, int id, Dictionary<string, double> axes) =>
            ((IDictionary)typeof(OriginalSession).GetField("itemAuraAmounts", Hidden).GetValue(s))[id] = axes;
        static double Armor(OriginalSession s, OriginalWorld world, int id)
        {
            if (id == 1)
            {
                object stats = Call(s, "CombatStatsFor", world.UnitState(id));
                return (double)stats.GetType().GetField("armor", Hidden).GetValue(stats);
            }
            var catalog = (OriginalCombatCatalog)typeof(OriginalSession).GetField("combatCatalog", Hidden).GetValue(s);
            return (double)Call(s, "EnemyArmor", catalog.Unit("hfoo"), id);
        }
        [Test] public void AuraArmorPercentageUsesBaselineOnceAndDoesNotMultiplyTemporaryArmor()
        {
            var s = Create(out var world);
            foreach (int id in new[] { 1, 1001 })
            {
                double baseline = Armor(s, world, id);
                var stateType = typeof(OriginalSession).GetNestedType("ItemArmorState", BindingFlags.NonPublic);
                object state = Activator.CreateInstance(stateType, true);
                stateType.GetField("amount", Hidden).SetValue(state, 7.0);
                ((IDictionary)typeof(OriginalSession).GetField("itemArmor", Hidden).GetValue(s))[(id,"Bdef")] = state;
                Axes(s, id, new Dictionary<string, double> { { "armor", 3 }, { "armorPercent", .5 } });
                Assert.That(Armor(s, world, id), Is.EqualTo(baseline * 1.5 + 10).Within(1e-9));
                Assert.That(Armor(s, world, id), Is.EqualTo(baseline * 1.5 + 10).Within(1e-9), "Reading stats cannot compound aura armor.");
            }
        }
        static double Strike(bool aura, bool cripple)
        {
            var s = Create(out var world);
            if (aura) Axes(s, 1, new Dictionary<string, double> { { "damageMelee", .4 }, { "attackSpeed", .5 } });
            double baselineRate = 1 + ((OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1)).agilityAttackSpeedBonus.Require();
            Assert.That(Call(s, "WeaponRate", world.UnitState(1)), Is.EqualTo(baselineRate + (aura ? .5 : 0)).Within(1e-9));
            if (cripple) Call(s, "OrderShieldCripple", 0, 1);
            Assert.That(world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001), Is.True);
            Call(s, "AdvanceWeapons");
            for (int i = 0; i < 12; i++) { world.Advance(.05); Call(s, "AdvanceWeapons"); }
            return (10000 - world.UnitState(1001).health) * 1.12;
        }
        [Test] public void ReleasedWeaponKeepsAuraGreenDamageOutsideCrippleWhiteReduction()
        {
            double white = Strike(false, false);
            Assert.That(Strike(true, false), Is.EqualTo(white * 1.4).Within(1e-7));
            Assert.That(Strike(true, true), Is.EqualTo(Math.Floor(white * .5) + white * .4).Within(1e-7));
        }
    }
}

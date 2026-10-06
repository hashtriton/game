using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionOrdinaryAiRegressionTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession session, string method, params object[] arguments) =>
            typeof(OriginalSession).GetMethod(method, Hidden).Invoke(session, arguments);
        static OriginalSession Create(string rawcode, out OriginalWorld world)
        {
            object[] arguments = { null, rawcode };
            var session = (OriginalSession)typeof(OriginalSessionOrdinarySpellTests)
                .GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, arguments);
            world = (OriginalWorld)arguments[0];
            Call(session, "ResolveOrdinaryNativeSpell", 9001, "A064", 1);
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(100).Within(.001));
            return session;
        }
        static void AdvanceSpell(OriginalSession session, OriginalWorld world)
        {
            for (int i = 0; i < 100; i++)
            { world.Advance(.01); Call(session, "AdvanceOrdinaryNativeSpells"); }
        }
        [Test] public void ManaBurnAiCastsOnTargetWithExistingUnrelatedBuff()
        {
            var session = Create("n027", out var world);
            double mana = world.UnitState(1).mana;
            Assert.DoesNotThrow(() => Call(session, "AdvanceWorldAi"));
            Assert.That(world.UnitState(9001).castSequence, Is.EqualTo(1));
            AdvanceSpell(session, world);
            Assert.That(world.UnitState(9001).mana, Is.EqualTo(4910));
            Assert.That(world.UnitState(1).mana, Is.LessThan(mana));
        }
        [Test] public void DeathCoilAiCastsOnTargetWithExistingUnrelatedBuff()
        {
            var session = Create("n00J", out var world);
            double health = world.UnitState(1).health;
            Assert.DoesNotThrow(() => Call(session, "AdvanceWorldAi"));
            Assert.That(world.UnitState(9001).castSequence, Is.EqualTo(1));
            AdvanceSpell(session, world);
            Assert.That(world.UnitState(9001).mana, Is.EqualTo(4925));
            Assert.That(world.UnitState(1).health, Is.LessThan(health));
        }
        [Test] public void BuffSpellAiStillSkipsTargetAlreadyAffectedByItsOwnBuff()
        {
            var session = Create("n00M", out var world);
            Assert.DoesNotThrow(() => Call(session, "AdvanceWorldAi"));
            Assert.That(world.UnitState(9001).castSequence, Is.Zero);
            Assert.That(world.UnitState(9001).mana, Is.EqualTo(5000));
            Call(session, "RemoveOrdinaryNativeBuffs", 1);
            Call(session, "SelectOrdinaryNativeSpellOrders");
            Assert.That(world.UnitState(9001).castSequence, Is.EqualTo(1));
        }
    }
}

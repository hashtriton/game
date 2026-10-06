using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionVisibilityCodecTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession session, string name, params object[] args) =>
            typeof(OriginalSession).GetMethod(name, Private).Invoke(session, args);
        static OriginalSession Create(out OriginalWorld world)
        {
            var session = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
            return session;
        }
        static FieldInfo Field(string name)
        {
            var field = typeof(OriginalWorldUnitView).GetField(name);
            Assert.That(field, Is.Not.Null, "Missing visibility wire field " + name);
            return field;
        }
        static int Mask(OriginalWorldUnitView unit) => (int)Field("visibleToOwners").GetValue(unit);
        static bool Invisible(OriginalWorldUnitView unit) => (bool)Field("invisible").GetValue(unit);
        static bool Wire(OriginalSessionView snapshot)
        {
            var codec = new OriginalUnitySessionCodec();
            return codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse {
                kind = OriginalNetworkResponseKind.Snapshot, snapshot = snapshot, assignedSlot = 1,
                acknowledgedSequence = snapshot.players[0].acknowledgedSequence }), out _);
        }
        static void InvisibleStatus(OriginalSession session, int actor)
        {
            var type = typeof(OriginalSession).GetMethod("ApplyNativeItemStatus", Private).GetParameters()[1].ParameterType;
            var rule = Activator.CreateInstance(type);
            type.GetField("status", Private).SetValue(rule, "B034");
            type.GetField("duration", Private).SetValue(rule, 7d);
            type.GetField("fade", Private).SetValue(rule, 2d);
            Call(session, "ApplyNativeItemStatus", actor, rule);
        }

        [Test] public void InvisibilityPublicationUsesHostObserverPredicateAfterFadeAndKeepsWorldBodies()
        {
            var session = Create(out var world);
            ((HashSet<int>)typeof(OriginalSession).GetField("hostilePlayerPairs", Private).GetValue(session)).Add(2 * 16 + 1);
            InvisibleStatus(session, 1);
            var before = session.Snapshot().world.units.Single(u => u.entityId == 1);
            Assert.That(Invisible(before), Is.False); Assert.That(Mask(before), Is.EqualTo(255));
            Call(session, "AdvanceItemStatuses", 2d);
            var snapshot = session.Snapshot(); var hero = snapshot.world.units.Single(u => u.entityId == 1);
            Assert.That(Invisible(hero), Is.True); Assert.That(hero.hidden, Is.False);
            Assert.That(Mask(hero), Is.EqualTo(253), "Hostile owner2 cannot see; self and allied owners can.");
            for (int owner = 1; owner <= 8; owner++)
                Assert.That((Mask(hero) & (1 << (owner - 1))) != 0,
                    Is.EqualTo((bool)Call(session, "CanSeeForCombat", owner, world.UnitState(1))));
            Assert.That(Wire(snapshot), Is.True);
            Field("visibleToOwners").SetValue(hero, 0);
            Assert.That(Mask(session.Snapshot().world.units.Single(u => u.entityId == 1)), Is.EqualTo(253));
            Assert.That(world.UnitState(1).hidden, Is.False); Assert.That(Invisible(world.UnitState(1)), Is.False);
            Call(session, "AdvanceItemStatuses", 5d);
            Assert.That(Invisible(session.Snapshot().world.units.Single(u => u.entityId == 1)), Is.False);
        }

        [Test] public void EnemyInvisibilityHasNoPartyBitsAndHiddenStateAlwaysHasZeroMask()
        {
            var session = Create(out var world);
            world.AddUnit(9001, 0, "hfoo", new OriginalWorldUnitProfile {
                collisionRadius = 8, maxHealth = 100, maxMana = 0 }, new OriginalPoint(300, 1000));
            InvisibleStatus(session, 9001); Call(session, "AdvanceItemStatuses", 2d);
            var enemy = session.Snapshot().world.units.Single(u => u.entityId == 9001);
            Assert.That(Invisible(enemy), Is.True); Assert.That(Mask(enemy), Is.Zero);
            Assert.That(Wire(session.Snapshot()), Is.True);
            world.SetVisibility(1, false);
            Assert.That(Mask(world.UnitState(1)), Is.Zero);
            Assert.That(Mask(session.Snapshot().world.units.Single(u => u.entityId == 1)), Is.Zero);
            Assert.That(Wire(session.Snapshot()), Is.True);
        }

        [Test] public void VisibilityCodecRejectsOutOfRangeAndContradictoryMasksAndOldProtocol()
        {
            var session = Create(out _);
            foreach (int mask in new[] { -1, 256, int.MaxValue, 0 })
            {
                var snapshot = session.Snapshot();
                Field("visibleToOwners").SetValue(snapshot.world.units[0], mask);
                Assert.That(Wire(snapshot), Is.False, "Visible ordinary actor with mask " + mask);
            }
            var hidden = session.Snapshot(); hidden.world.units[0].hidden = true;
            Assert.That(Wire(hidden), Is.False);
            var old = session.Snapshot(); old.protocol = OriginalSession.Protocol - 1;
            Assert.That(Wire(old), Is.False);
        }
    }
}

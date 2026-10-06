using System;
using System.Collections;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace Arena.Tests
{
    public sealed partial class OriginalItemInputTests
    {
        int HeldCharges(long instance) => session.Snapshot().players[0].inventory.heroSlots
            .Where(item => item != null && item.instanceId == instance).Sum(item => item.charges);

        Vector3 ApproachTerrainPoint(OriginalPoint point)
        {
            var position = new Vector3((float)(point.x / runtime.map.unitsPerMeter), 0,
                (float)(point.y / runtime.map.unitsPerMeter));
            var terrain = UnityEngine.Object.FindAnyObjectByType<Terrain>();
            position.y = terrain.SampleHeight(position) + terrain.transform.position.y;
            return position;
        }

        OriginalPoint VisibleDistantApproachPoint(double extraRadius = 0)
        {
            var origin = world.UnitState(1).position;
            var nav = (IOriginalWorldNavigation)typeof(OriginalWorld).GetField("navigation", Private).GetValue(world);
            for (int step = 0; step < 16; step++)
            {
                double angle = step * Math.PI / 8;
                var target = new OriginalPoint(origin.x + 850 * Math.Cos(angle), origin.y + 850 * Math.Sin(angle));
                if (!nav.IsWalkable(target.x, target.y, Math.Max(extraRadius, world.UnitState(1).profile.collisionRadius))) continue;
                var path = nav.FindPath(origin, target, world.UnitState(1).profile.collisionRadius);
                if (path == null || path.Length == 0) continue;
                var pixel = runtime.viewCamera.WorldToScreenPoint(ApproachTerrainPoint(target));
                if (pixel.z > 0 && pixel.x > Screen.width * .2f && pixel.x < Screen.width * .8f &&
                    pixel.y > Screen.height * .35f && pixel.y < Screen.height * .82f) return target;
            }
            Assert.Fail("Fixture needs a visible reachable target outside the ward's 500 range.");
            return default;
        }

        [UnityTest] public IEnumerator DistantWardMouseMovesBeforeDebitAndStopCancelsTheActualIntent()
        {
            long instance = Equip("I021");
            runtime.network.SendCommand(OriginalSessionCommandKind.Stop);
            yield return null; yield return null;
            var target = VisibleDistantApproachPoint();
            var before = world.UnitState(1);
            int charges = HeldCharges(instance);
            int wards = world.Snapshot().units.Count(unit => unit.rawcode == "ohwd");
            Assert.That(Use(instance).targetMode, Is.EqualTo(OriginalAbilityTargetMode.UnitOrPoint));
            Assert.That(runtime.RequestItemUse(instance), Is.True);
            Point(ApproachTerrainPoint(target), MouseButton.Left);
            yield return null; yield return null;
            Point(ApproachTerrainPoint(target));
            float deadline = Time.realtimeSinceStartup + 1;
            while (world.UnitState(1).order != OriginalWorldOrder.Move && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(world.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.Move), runtime.Notice);
            Assert.That(HeldCharges(instance), Is.EqualTo(charges));
            Assert.That(world.UnitState(1).mana, Is.GreaterThanOrEqualTo(before.mana - .001));
            Assert.That(world.Snapshot().units.Count(unit => unit.rawcode == "ohwd"), Is.EqualTo(wards));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return new WaitForSecondsRealtime(1);
            Assert.That(world.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(HeldCharges(instance), Is.EqualTo(charges));
            Assert.That(world.Snapshot().units.Count(unit => unit.rawcode == "ohwd"), Is.EqualTo(wards));
            Assert.That(runtime.View.haltReason, Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator WardUnitOrPointMouseSelectsAnOwnedSummonAndPlacesAtItsCurrentPosition()
        {
            long summonItem = Equip("I01D"), wardItem = Equip("I094");
            float deadline = Time.realtimeSinceStartup + 2;
            // Equip changes the authority fixture immediately; the rendered
            // client receives it at the ordinary snapshot cadence.
            while (!runtime.View.players[0].itemUses.Any(item => item.instanceId == summonItem &&
                item.code == OriginalItemUseCode.Ready) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(runtime.RequestItemUse(summonItem), Is.True, runtime.Notice);
            deadline = Time.realtimeSinceStartup + 2;
            while (!world.Snapshot().units.Any(unit => unit.kind == OriginalWorldUnitKind.Summon && unit.ownerSlot == 1) &&
                Time.realtimeSinceStartup < deadline) yield return null;
            var summoned = world.Snapshot().units.First(unit => unit.kind == OriginalWorldUnitKind.Summon && unit.ownerSlot == 1);
            var target = VisibleDistantApproachPoint(summoned.profile.collisionRadius);
            // Fixture placement isolates the real UI unit picker and host cast
            // route. This is an integration test, not an organic battle claim.
            Assert.That(world.ForcePosition(summoned.entityId, target), Is.True);
            world.Stop(summoned.entityId);
            yield return new WaitForSecondsRealtime(.35f);
            int charges = HeldCharges(wardItem);
            Assert.That(runtime.RequestItemUse(wardItem), Is.True);
            Point(ApproachTerrainPoint(target) + Vector3.up * .8f, MouseButton.Left);
            yield return null; yield return null;
            Point(ApproachTerrainPoint(target));
            bool relocated = false;
            for (int step = 0; step < 16; step++)
            {
                double angle = step * Math.PI / 8;
                var next = new OriginalPoint(target.x + 220 * Math.Cos(angle), target.y + 220 * Math.Sin(angle));
                if (!world.ForcePosition(summoned.entityId, next)) continue;
                world.Stop(summoned.entityId);
                relocated = true; break;
            }
            Assert.That(relocated, Is.True, "Fixture needs a second free unit position.");
            deadline = Time.realtimeSinceStartup + 8;
            while (!world.Snapshot().units.Any(unit => unit.rawcode == "o00J") && Time.realtimeSinceStartup < deadline) yield return null;
            var ward = world.Snapshot().units.Single(unit => unit.rawcode == "o00J");
            var current = world.UnitState(summoned.entityId).position;
            Assert.That(Math.Sqrt(Math.Pow(ward.position.x - current.x, 2) + Math.Pow(ward.position.y - current.y, 2)),
                Is.LessThanOrEqualTo(128), "Existing free-placement policy must stay around the selected unit.");
            Assert.That(HeldCharges(wardItem), Is.EqualTo(charges - 1));
            Assert.That(runtime.ArmedItemInstance, Is.Zero);
            Assert.That(runtime.View.haltReason, Is.Null.Or.Empty);
        }
    }
}

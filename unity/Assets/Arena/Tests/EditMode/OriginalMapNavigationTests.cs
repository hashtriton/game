using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalMapNavigationTests
    {
        GameObject root;
        OriginalMapNavigation navigation;

        [SetUp] public void Setup()
        {
            root = new GameObject("Original navigation regression");
            var map = root.AddComponent<ArenaMap>();
            map.layoutJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-layout.json");
            navigation = new OriginalMapNavigation(map);
            foreach (int id in new[] { 686, 740, 744 }) navigation.SetDoodadAlive(id, false);
        }
        [TearDown] public void Cleanup() => UnityEngine.Object.DestroyImmediate(root);

        [Test] public void FloatMapConversionDoesNotCreateANewPhysicalStartWaypoint()
        {
            // Actual host snapshot from the first-wave choke, 2026-10-06.
            var from = new OriginalPoint(1366.4429849502908, 2062.300002937159);
            var to = new OriginalPoint(1248, 2464);
            var path = navigation.FindPath(from, to, 24);
            Assert.That(path, Is.Not.Empty);
            Assert.That(path[0].x, Is.EqualTo(from.x));
            Assert.That(path[0].y, Is.EqualTo(from.y));
            Assert.That(path[path.Length - 1].x, Is.EqualTo(to.x));
            Assert.That(path[path.Length - 1].y, Is.EqualTo(to.y));
        }

        [Test] public void TouchingCreepsCanFollowTheRealOpenedChokeWithoutBackingIntoEachOther()
        {
            var world = new OriginalWorld(navigation);
            var profile = new OriginalWorldUnitProfile { moveSpeed = 300, collisionRadius = 24, maxHealth = 80 };
            var goal = new OriginalPoint(1248, 2464);
            world.AddUnit(1, 1, "H008", profile, goal);
            // Observed 1022/1040 positions. The extra 1e-6 WC removes only a
            // sub-ULP negative contact gap in the serialized host snapshot.
            world.AddUnit(1022, 0, "n008", profile, new OriginalPoint(1366.4429849502908, 2062.300002937159));
            world.AddUnit(1040, 0, "n008", profile, new OriginalPoint(1395.4474138132376, 2100.5458283870444));
            world.TryMove(1022, goal); world.TryMove(1040, goal);
            for (int tick = 0; tick < 120; tick++)
            {
                var a = world.UnitState(1022).position; var b = world.UnitState(1040).position;
                world.Advance(.05);
                var afterA = world.UnitState(1022).position; var afterB = world.UnitState(1040).position;
                Assert.That(Distance(afterA, afterB), Is.GreaterThanOrEqualTo(48 - 1e-6));
                Assert.That(Distance(a, afterA), Is.LessThanOrEqualTo(15 + 1e-6));
                Assert.That(Distance(b, afterB), Is.LessThanOrEqualTo(15 + 1e-6));
                Assert.That(navigation.IsWalkable(afterA.x, afterA.y, 24), Is.True);
                Assert.That(navigation.IsWalkable(afterB.x, afterB.y, 24), Is.True);
            }
            Assert.That(Distance(world.UnitState(1040).position, goal), Is.LessThanOrEqualTo(120));
            Assert.That(Distance(world.UnitState(1022).position, goal), Is.LessThanOrEqualTo(168));
        }

        [Test] public void ProjectedBlockedEndpointsDoNotBecomeTheOriginalBlockedCoordinates()
        {
            var blocked = new OriginalPoint(1376, 2400);
            var open = new OriginalPoint(1248, 2464);
            Assert.That(navigation.IsWalkable(blocked.x, blocked.y, 24), Is.False);
            var fromBlocked = navigation.FindPath(blocked, open, 24);
            var toBlocked = navigation.FindPath(open, blocked, 24);
            Assert.That(fromBlocked, Is.Not.Empty); Assert.That(toBlocked, Is.Not.Empty);
            var start = fromBlocked[0]; var end = toBlocked[toBlocked.Length - 1];
            Assert.That(navigation.IsWalkable(start.x, start.y, 24), Is.True);
            Assert.That(navigation.IsWalkable(end.x, end.y, 24), Is.True);
        }

        [Test] public void AReachableForwardRouteDoesNotRequireEnteringABodyBehindTheActor()
        {
            var world = new OriginalWorld(navigation);
            var profile = new OriginalWorldUnitProfile { moveSpeed = 300, collisionRadius = 24, maxHealth = 80 };
            var goal = new OriginalPoint(1248, 2464);
            world.AddUnit(1, 1, "H008", profile, goal);
            world.AddUnit(1022, 0, "n008", profile, new OriginalPoint(1360, 2064));
            world.AddUnit(1040, 0, "n008", profile, new OriginalPoint(1395.4474138132376, 2100.5458283870444));
            world.TryMove(1040, goal);
            for (int tick = 0; tick < 120; tick++) world.Advance(.05);
            Assert.That(Distance(world.UnitState(1040).position, goal), Is.LessThanOrEqualTo(120));
            Assert.That(world.UnitState(1022).position.x, Is.EqualTo(1360));
            Assert.That(world.UnitState(1022).position.y, Is.EqualTo(2064));
        }

        [Test] public void StationaryFrontCreepsCanAttackAcrossTheRealChokeAt131And126Units()
        {
            T Load<T>(string name) => JsonUtility.FromJson<T>(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/" + name).text);
            var session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureWorld(navigation, Load<OriginalNativeCatalog>("lia39-native126.json"), Array.Empty<OriginalWorldDoodadView>(),
                Load<OriginalObservedCatalog>("lia39-observed126.json"));
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            Assert.That(world.Relocate(1, new OriginalPoint(1248, 2464)), Is.True); world.HoldPosition(1);
            // Actual stalled pair 1003/1012, rounded outward by less than .25
            // WC so a replay does not introduce sub-ULP terrain/body overlap.
            var a = new OriginalPoint(1379, 2456); var b = new OriginalPoint(1368, 2503);
            var profile = new OriginalWorldUnitProfile { moveSpeed = 300, collisionRadius = 24, maxHealth = 1000 };
            world.AddUnit(1003, 0, "n008", profile, a); world.AddUnit(1012, 0, "n008", profile, b);
            world.TryAttackTarget(1003, OriginalWorldTargetKind.Unit, 1); world.TryAttackTarget(1012, OriginalWorldTargetKind.Unit, 1);
            for (int tick = 0; tick < 40; tick++) { session.Advance(.05); session.DrainEvents(); }
            Assert.That(session.HaltReason, Is.Null);
            Assert.That(world.UnitState(1003).attackSequence, Is.GreaterThan(0));
            Assert.That(world.UnitState(1012).attackSequence, Is.GreaterThan(0));
            Assert.That(world.UnitState(1).health, Is.LessThan(631));
            Assert.That(Distance(world.UnitState(1003).position, a), Is.Zero);
            Assert.That(Distance(world.UnitState(1012).position, b), Is.Zero);
            Assert.That(Distance(world.UnitState(1).position, new OriginalPoint(1248, 2464)), Is.Zero);
        }

        static double Distance(OriginalPoint a, OriginalPoint b) => Math.Sqrt((a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y));
    }
}

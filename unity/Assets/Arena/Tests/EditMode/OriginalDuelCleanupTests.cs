using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalDuelCleanupTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => x < 9000;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => IsWalkable(b.x, b.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalWorld World(OriginalSession session) => (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
        static SortedDictionary<long, OriginalGroundItemView> Ground(OriginalSession session) =>
            (SortedDictionary<long, OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
        static bool Apply(OriginalSession session, params OriginalDuelEvent[] rows) => (bool)typeof(OriginalSession)
            .GetMethod("ApplyDuelWorldBatch", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(session, new object[] { rows });
        static OriginalDuelEvent Rule(string code) => new OriginalDuelEvent { kind = OriginalDuelEventKind.WorldRule, code = code };
        static OriginalDuelEvent Place(double x, double y) => new OriginalDuelEvent { kind = OriginalDuelEventKind.Placement, slot = 1, x = x, y = y };
        static OriginalSession Start()
        {
            var native = Load<OriginalNativeCatalog>("native126"); var observed = Load<OriginalObservedCatalog>("observed126");
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            int sequence = 1;
            foreach (var kind in new[] { OriginalSessionCommandKind.SelectHero, OriginalSessionCommandKind.LobbyReady, OriginalSessionCommandKind.Start })
                Assert.That(session.Apply(0, new OriginalSessionCommand { kind = kind, sequence = sequence++, protocol = OriginalSession.Protocol,
                    contentHash = new string('a', 64), heroId = "H008", ready = true }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            World(session).DrainEvents(); return session;
        }
        static void Enemy(OriginalWorld world, int id, string code, double x, double y) => world.AddUnit(id, 0, code,
            new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 100, maxMana = 10, moveSpeed = 200 }, new OriginalPoint(x, y));

        [Test] public void CleanupBeforePlacementFreesExactDestinationWithoutFallback()
        {
            var session = Start(); var world = World(session);
            Enemy(world, 1200, "n008", -416, -2688); world.DrainEvents();
            Assert.That(Apply(session, Rule("pair-prepare:Z2-cleanup"), Place(-416, -2688)), Is.True);
            Assert.That(world.UnitState(1200), Is.Null);
            Assert.That(world.UnitState(1).position.x, Is.EqualTo(-416));
            Assert.That(world.UnitState(1).position.y, Is.EqualTo(-2688));
            Assert.That(world.DrainEvents().Select(e => e.kind), Is.EqualTo(new[] { OriginalWorldEventKind.UnitDied, OriginalWorldEventKind.UnitRemoved }));
        }

        [Test] public void RegionCleanupUsesPositionAtItsStepBeforeLaterHeroRelocation()
        {
            var session = Start(); var world = World(session);
            Assert.That(Apply(session, Rule("pair-prepare:Z2-cleanup"), Place(-416, -2688)), Is.True);
            Assert.That(world.UnitState(1).paused, Is.False, "The hero was outside Z2 when cleanup ran.");
            var other = Start();
            Assert.That(Apply(other, Place(-416, -2688), Rule("pair-prepare:Z2-cleanup")), Is.True);
            Assert.That(World(other).UnitState(1).paused, Is.True, "The later cleanup sees the relocated hero.");
        }

        [Test] public void LateBlockedPlacementDoesNotPublishCleanupPauseDeathOrGroundDeletion()
        {
            var session = Start(); var world = World(session);
            Enemy(world, 1200, "n008", -416, -2688);
            Enemy(world, 1201, "E00J", 500, 1000);
            Ground(session).Add(7, new OriginalGroundItemView { item = new OriginalItemInstance { instanceId = 7, itemId = "I000", ownerId = 0 }, position = new OriginalPoint(-416, -2688) });
            world.DrainEvents(); var before = world.Snapshot();
            Assert.That(Apply(session, Rule("pair-prepare:delete-unowned-arena-items"), Rule("pair-prepare:Z2-cleanup"),
                new OriginalDuelEvent { kind = OriginalDuelEventKind.HeroState, slot = 1, paused = 1 }, Place(10000, -2688)), Is.False);
            Assert.That(world.Snapshot().revision, Is.EqualTo(before.revision));
            Assert.That(world.UnitState(1200).health, Is.EqualTo(100));
            Assert.That(world.UnitState(1201).health, Is.EqualTo(100));
            Assert.That(world.UnitState(1).paused, Is.False);
            Assert.That(Ground(session).ContainsKey(7), Is.True);
            Assert.That(world.DrainEvents(), Is.Empty);
        }

        [Test] public void KillRetainsDeadEntryWhileRemoveDoesNotInventADeath()
        {
            var world = World(Start());
            Enemy(world, 1200, "n008", 300, 1000); Enemy(world, 1201, "n008", 400, 1000);
            world.DrainEvents();
            Assert.That(world.TryApplyTransitions(new[] {
                new OriginalWorldTransition { entityId = 1200, kill = true },
                new OriginalWorldTransition { entityId = 1200, kill = true },
                new OriginalWorldTransition { entityId = 1201, remove = true }
            }), Is.True);
            Assert.That(world.UnitState(1200).health, Is.Zero);
            Assert.That(world.UnitState(1201), Is.Null);
            Assert.That(world.DrainEvents().Select(e => e.kind), Is.EqualTo(new[] { OriginalWorldEventKind.UnitDied, OriginalWorldEventKind.UnitRemoved }));
            Assert.That(world.Relocate(1, new OriginalPoint(300, 1000)), Is.True, "The corpse no longer reserves its old body.");
        }

        [Test] public void ExplicitReviveAfterKillPreservesDeathEventAndClearsAttackers()
        {
            var world = World(Start()); Enemy(world, 1200, "n008", 300, 1000);
            Assert.That(world.TryAttackTarget(1200, OriginalWorldTargetKind.Unit, 1), Is.True);
            world.DrainEvents();
            Assert.That(world.TryApplyTransitions(new[] {
                new OriginalWorldTransition { entityId = 1, kill = true },
                new OriginalWorldTransition { entityId = 1, revive = true, relocate = true, position = new OriginalPoint(600, 1000) }
            }), Is.True);
            Assert.That(world.UnitState(1).health, Is.EqualTo(world.UnitState(1).profile.maxHealth));
            Assert.That(world.UnitState(1200).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(world.DrainEvents().Select(e => e.kind), Is.EqualTo(new[] { OriginalWorldEventKind.UnitDied }));
        }

        [Test] public void RemovingSourceKeepsDetachedImagesAndLaterCleanupRemovesThem()
        {
            var world = World(Start()); var hero = world.UnitState(1);
            world.AddIllusion(1000000000, 1, hero.profile, new OriginalPoint(300, 1000), hero.health, hero.mana);
            world.DrainEvents(); long before = world.Snapshot().revision;
            Assert.That(world.TryApplyTransitions(new[] { new OriginalWorldTransition { entityId = 1, remove = true } }), Is.True);
            Assert.That(world.Snapshot().revision, Is.GreaterThan(before));
            Assert.That(world.UnitState(1), Is.Null);
            Assert.That(world.UnitState(1000000000).copySourceEntityId, Is.EqualTo(1));
            Assert.That(world.UnitState(1000000000).health, Is.EqualTo(hero.health));
            Assert.That(world.DrainEvents().Single().entityId, Is.EqualTo(1));
            Assert.That(world.TryApplyTransitions(new[] {
                new OriginalWorldTransition { entityId = 1000000000, remove = true }
            }), Is.True);
            Assert.That(world.Snapshot().units, Is.Empty);
            Assert.That(world.DrainEvents().All(e => e.kind == OriginalWorldEventKind.UnitRemoved), Is.True);
        }

        [Test] public void ReusingRemovedIdentityRejectsWholeBatch()
        {
            var world = World(Start()); Enemy(world, 1200, "n008", 300, 1000); world.DrainEvents();
            long before = world.Snapshot().revision;
            Assert.That(world.TryApplyTransitions(new[] {
                new OriginalWorldTransition { entityId = 1200, kill = true, remove = true },
                new OriginalWorldTransition { entityId = 1200, revive = true }
            }), Is.False);
            Assert.That(world.UnitState(1200).health, Is.EqualTo(100));
            Assert.That(world.Snapshot().revision, Is.EqualTo(before)); Assert.That(world.DrainEvents(), Is.Empty);
        }
    }
}

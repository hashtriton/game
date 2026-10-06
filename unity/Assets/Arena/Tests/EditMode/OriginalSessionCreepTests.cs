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
    public sealed class OriginalSessionCreepTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public bool blocked;
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision { get; private set; }
            public bool IsWalkable(double x, double y, double radius) => !blocked && Math.Abs(x) <= 4096 && Math.Abs(y) <= 4096;
            public bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius) => IsWalkable(to.x, to.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius) => new[] { to };
            public bool SetDoodadAlive(int editorId, bool alive) { NavigationRevision++; return true; }
        }

        sealed class Fixture
        {
            internal OriginalSession session;
            internal Navigation navigation;
            internal readonly List<OriginalMatchEvent> events = new List<OriginalMatchEvent>();
            internal OriginalSessionView View => session.Snapshot();
            internal void Collect()
            {
                foreach (var item in session.DrainEvents()) if (item.matchEvent != null) events.Add(item.matchEvent);
            }
            internal void Advance(double seconds, double step = .05)
            {
                while (seconds > 1e-9 && session.HaltReason == null)
                {
                    double duration = Math.Min(step, seconds);
                    session.Advance(duration); Collect(); seconds -= duration;
                }
            }
        }

        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static Fixture Create()
        {
            var f = new Fixture { navigation = new Navigation() };
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            f.session.ConfigureWorld(f.navigation, Load<OriginalNativeCatalog>("lia39-native126.json"),
                Array.Empty<OriginalWorldDoodadView>(), Load<OriginalObservedCatalog>("lia39-observed126.json"));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Collect(); return f;
        }

        static void EnterCombat(Fixture f)
        {
            f.Advance(2.05);
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.WaveReady }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Collect(); Assert.That(f.View.phase, Is.EqualTo(OriginalMatchPhase.Combat));
        }

        [Test] public void PreparationContainsHeroesButNoPrematureCreeps()
        {
            var f = Create(); f.Advance(1.95);
            Assert.That(f.View.phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            Assert.That(f.View.world.units.Select(u => u.ownerSlot), Is.EqualTo(new[] { 1 }));
            Assert.That(f.View.enemies, Is.Empty);
            Assert.That(f.events.Count(e => e.kind == OriginalMatchEventKind.Spawn), Is.Zero);
        }

        [Test] public void FirstWaveKeepsThirteenRegularsPerGroupAndAllBossCastersInGd()
        {
            var f = Create(); EnterCombat(f); f.Advance(3);
            Assert.That(f.session.HaltReason, Is.Null);
            var enemies = f.View.enemies;
            Assert.That(enemies.Length, Is.EqualTo(42));
            Assert.That(enemies.Count(e => e.rawcode == "n008"), Is.EqualTo(39));
            for (int group = 0; group < 3; group++)
                Assert.That(enemies.Count(e => e.rawcode == "n008" && e.attackGroup == group), Is.EqualTo(13));
            Assert.That(enemies.Count(e => e.rawcode == "n009" && e.attackGroup == 0), Is.EqualTo(1));
            Assert.That(enemies.Count(e => e.rawcode == "n05J" && e.attackGroup == 0), Is.EqualTo(2));
            var spawns = f.events.Where(e => e.kind == OriginalMatchEventKind.Spawn).ToArray();
            Assert.That(spawns.Length, Is.EqualTo(42));
            foreach (var enemy in enemies)
                Assert.That(spawns.Single(e => e.entityId == enemy.entityId).attackGroup, Is.EqualTo(enemy.attackGroup));
            enemies[0].attackGroup = (enemies[0].attackGroup + 1) % 3;
            Assert.That(f.View.enemies[0].attackGroup, Is.EqualTo(spawns.Single(e => e.entityId == enemies[0].entityId).attackGroup));
        }

        [Test] public void WorldSpawnsSourceIdentitiesAndObservedHealthManaSpeedWithoutGuessingSparseFields()
        {
            var f = Create(); EnterCombat(f); f.Advance(3);
            Assert.That(f.session.HaltReason, Is.Null);
            var view = f.View; Assert.That(view.world.units.Length, Is.EqualTo(43));
            foreach (var enemy in view.enemies)
            {
                var unit = view.world.units.Single(u => u.entityId == OriginalWorld.EnemyEntityId(enemy.entityId));
                Assert.That(unit.ownerSlot, Is.Zero); Assert.That(unit.rawcode, Is.EqualTo(enemy.rawcode));
                double hp = enemy.rawcode == "n008" ? 80 : enemy.rawcode == "n009" ? 200 : 300;
                double mana = enemy.rawcode == "n008" ? 0 : enemy.rawcode == "n009" ? 300 : 250;
                Assert.That(unit.profile.maxHealth, Is.EqualTo(hp)); Assert.That(unit.health, Is.EqualTo(hp));
                Assert.That(unit.profile.maxMana, Is.EqualTo(mana)); Assert.That(unit.mana, Is.EqualTo(mana));
                Assert.That(unit.profile.moveSpeed, Is.EqualTo(300)); Assert.That(unit.profile.collisionRadius, Is.EqualTo(24));
            }
        }

        static void AssertSeparateBodies(OriginalWorldSnapshot world)
        {
            var units = world.units.Where(u => u.health > 0).ToArray();
            for (int i = 0; i < units.Length; i++)
                for (int j = i + 1; j < units.Length; j++)
                {
                    var a = units[i]; var b = units[j];
                    double dx = a.position.x - b.position.x, dy = a.position.y - b.position.y;
                    double radius = a.profile.collisionRadius + b.profile.collisionRadius;
                    Assert.That(dx * dx + dy * dy, Is.GreaterThanOrEqualTo(radius * radius - 1e-5), a.entityId + ":" + b.entityId);
                }
        }

        [Test] public void ClusteredWaveSpawnsAndApproachingUnitsNeverDisableBodyCollision()
        {
            var f = Create(); EnterCombat(f);
            for (int i = 0; i < 60; i++)
            {
                f.Advance(.05); Assert.That(f.session.HaltReason, Is.Null);
                AssertSeparateBodies(f.View.world);
            }
            Assert.That(f.View.world.units.Count(u => u.ownerSlot == 0), Is.EqualTo(42));
        }

        [Test] public void NativeFailedN068CreationHaltsWithoutInventingAnEntityOrAdvancingTime()
        {
            var f = Create(); int before = f.View.world.units.Length;
            // A trusted source event seam isolates the missing native rawcode.
            // No client command can publish a spawn or supply unit stats.
            var apply = typeof(OriginalSession).GetMethod("ApplyWorldEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            apply.Invoke(f.session, new object[] { new OriginalMatchEvent {
                kind = OriginalMatchEventKind.Spawn, entityId = 900, rawcode = "n068", x = 0, y = 2000, round = 23 } });
            Assert.That(f.session.HaltReason, Does.StartWith("world-spawn-rule-unavailable:n068"));
            Assert.That(f.View.world.units.Length, Is.EqualTo(before));
            Assert.That(f.View.world.units.Any(u => u.entityId == 1900), Is.False);
            double time = f.View.time; f.session.Advance(1);
            Assert.That(f.View.time, Is.EqualTo(time));
        }

        [Test] public void UnplaceableWaveSpawnFailsExplicitlyInsteadOfStackingOrDisablingCollision()
        {
            var f = Create(); EnterCombat(f); f.navigation.blocked = true; f.Advance(1);
            Assert.That(f.session.HaltReason, Does.StartWith("world-spawn-"));
            Assert.That(f.View.world.units.Length, Is.EqualTo(1));
            double time = f.View.time; f.session.Advance(1);
            Assert.That(f.View.time, Is.EqualTo(time));
        }

        [Test] public void HostAdvanceChunkSizeCannotChangeSpawnIdentitiesOrBodyPositions()
        {
            var small = Create(); var large = Create(); EnterCombat(small); EnterCombat(large);
            small.Advance(3, .05); large.Advance(3, 1);
            Assert.That(small.session.HaltReason, Is.Null); Assert.That(large.session.HaltReason, Is.Null);
            var a = small.View.world.units; var b = large.View.world.units;
            Assert.That(a.Length, Is.EqualTo(43)); Assert.That(b.Length, Is.EqualTo(a.Length));
            for (int i = 0; i < a.Length; i++)
            {
                Assert.That(b[i].entityId, Is.EqualTo(a[i].entityId)); Assert.That(b[i].rawcode, Is.EqualTo(a[i].rawcode));
                Assert.That(b[i].position.x, Is.EqualTo(a[i].position.x).Within(1e-6));
                Assert.That(b[i].position.y, Is.EqualTo(a[i].position.y).Within(1e-6));
            }
        }

        [Test] public void NormalWaveOrdersExcludeHeroesOutsideTheSourceArenaRectangle()
        {
            var f = Create(); EnterCombat(f);
            var world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
            Assert.That(world.Relocate(1, new OriginalPoint(0, -3800)), Is.True);
            f.Advance(3);
            Assert.That(f.session.HaltReason, Is.Null);
            Assert.That(f.View.world.units.Count(u => u.ownerSlot == 0), Is.EqualTo(42));
            foreach (var creep in f.View.world.units.Where(u => u.ownerSlot == 0))
                Assert.That(creep.order, Is.EqualTo(OriginalWorldOrder.None),
                    "E5v enumerates heroes in vV (-2560,-1536,2304,3200); a hero in the boss area cannot supply its point order.");
        }
    }
}

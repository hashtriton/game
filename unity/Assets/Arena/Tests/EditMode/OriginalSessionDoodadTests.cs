using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionDoodadTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision { get; private set; }
            public bool IsWalkable(double x, double y, double radius) => Math.Abs(x) <= 4096 && Math.Abs(y) <= 4096;
            public bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius) => IsWalkable(to.x, to.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius) => new[] { to };
            public bool SetDoodadAlive(int id, bool alive) { NavigationRevision++; return true; }
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        static T Private<T>(OriginalSession session, string name) => (T)typeof(OriginalSession).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
        static OriginalSession Create(string barrel = "LTex")
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureWorld(new Navigation(), Load<OriginalNativeCatalog>("lia39-native126"), new[] {
                new OriginalWorldDoodadView { editorId = 125, rawcode = barrel, position = new OriginalPoint(0, 0), maxHealth = 20, health = 20 } },
                Load<OriginalObservedCatalog>("lia39-observed126"));
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(session, 2.05);
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.WaveReady }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(session, 2.1);
            Assert.That(session.HaltReason, Is.Null);
            return session;
        }
        static void Advance(OriginalSession session, double seconds)
        {
            while (seconds > 1e-8) { double step = Math.Min(.05, seconds); session.Advance(step); session.DrainEvents(); seconds -= step; }
        }
        static void DispatchDestruction(OriginalSession session)
        {
            var world = Private<OriginalWorld>(session, "world");
            Assert.That(world.ApplyDoodadDamage(125, 20), Is.True);
            typeof(OriginalSession).GetMethod("ProcessWorldEvents", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(session, null);
        }

        [Test] public void ExplosiveBarrelKillsEligibleCreepsWithin500WithoutGivingAlliedKillXp()
        {
            var session = Create(); var world = Private<OriginalWorld>(session, "world");
            var enemies = session.Snapshot().enemies.Where(e => e.rawcode == "n008").Take(3).ToArray();
            var positions = new[] { new OriginalPoint(0, 100), new OriginalPoint(500, 0), new OriginalPoint(550, 0) };
            for (int i = 0; i < enemies.Length; i++) Assert.That(world.Relocate(1000 + enemies[i].entityId, positions[i]), Is.True);
            int before = session.Snapshot().remainingEnemies;
            DispatchDestruction(session);
            Assert.That(world.UnitState(1000 + enemies[0].entityId), Is.Null);
            Assert.That(world.UnitState(1000 + enemies[1].entityId), Is.Null);
            Assert.That(world.UnitState(1000 + enemies[2].entityId).health, Is.EqualTo(80));
            Assert.That(session.Snapshot().remainingEnemies, Is.EqualTo(before - 2));
            Assert.That(session.Snapshot().players[0].experience, Is.Zero);
            Assert.That(world.UnitState(1).health, Is.EqualTo(631));
        }

        [Test] public void ExplosionUsesKillUnitAndExcludesSourceMegaBossUserDataTwo()
        {
            var session = Create(); var world = Private<OriginalWorld>(session, "world"); var match = Private<OriginalMatch>(session, "match");
            var targets = match.Enemies.Where(e => e.rawcode == "n008").Take(3).ToArray();
            Assert.That(world.Relocate(1000 + targets[0].entityId, new OriginalPoint(0, 100)), Is.True);
            Assert.That(world.Relocate(1000 + targets[1].entityId, new OriginalPoint(100, 100)), Is.True);
            Assert.That(world.Relocate(1000 + targets[2].entityId, new OriginalPoint(200, 100)), Is.True);
            world.SetUnitState(1000 + targets[0].entityId, invulnerable: true);
            targets[1].megaBoss = true;
            targets[1].sourceUserData = 2;
            targets[2].megaBoss = true; // The host label alone is not the source GetUnitUserData filter.
            DispatchDestruction(session);
            Assert.That(world.UnitState(1000 + targets[0].entityId), Is.Null, "KillUnit is distinct from damage and bypasses invulnerability.");
            Assert.That(world.UnitState(1000 + targets[1].entityId).health, Is.EqualTo(80));
            Assert.That(world.UnitState(1000 + targets[2].entityId), Is.Null);
        }

        [Test] public void OrdinaryBarrelDestructionDoesNotTriggerExplosiveKills()
        {
            var session = Create("LTbr"); var world = Private<OriginalWorld>(session, "world");
            int id = 1000 + session.Snapshot().enemies.First(e => e.rawcode == "n008").entityId;
            Assert.That(world.Relocate(id, new OriginalPoint(0, 100)), Is.True);
            int before = session.Snapshot().remainingEnemies;
            DispatchDestruction(session);
            Assert.That(world.UnitState(id).health, Is.EqualTo(80));
            Assert.That(session.Snapshot().remainingEnemies, Is.EqualTo(before));
        }
    }
}

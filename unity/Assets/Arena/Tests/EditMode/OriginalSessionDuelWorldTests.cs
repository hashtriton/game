using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionDuelWorldTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalWorld World(OriginalSession session) => (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
        static OriginalSessionReplyCode Send(OriginalSession session, int slot, OriginalSessionCommandKind kind, string hero = null, int target = 0)
        {
            var player = session.Snapshot().players.FirstOrDefault(p => p.slot == slot);
            return session.Apply(slot == 1 ? 0 : slot * 10, new OriginalSessionCommand { kind = kind,
                sequence = kind == OriginalSessionCommandKind.Hello ? 1 : player.acknowledgedSequence + 1,
                protocol = OriginalSession.Protocol, contentHash = new string('a', 64), heroId = hero, ready = true,
                targetKind = target == 0 ? OriginalWorldTargetKind.None : OriginalWorldTargetKind.Unit, targetId = target });
        }
        static void Tick(OriginalSession session, double seconds)
        {
            for (double left = seconds; left > 1e-9; left -= Math.Min(left, .05))
            {
                session.Advance(Math.Min(left, .05)); session.DrainEvents();
                Assert.That(session.HaltReason, Is.Null);
            }
        }
        static OriginalSession Start()
        {
            var native = Load<OriginalNativeCatalog>("native126"); var observed = Load<OriginalObservedCatalog>("observed126");
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureProgression(native, observed);
            session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            foreach (int slot in new[] { 2, 3 }) Assert.That(Send(session, slot, OriginalSessionCommandKind.Hello), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            string[] heroes = { "H008", "N0A0", "H024" };
            for (int i = 0; i < heroes.Length; i++)
            {
                Send(session, i + 1, OriginalSessionCommandKind.SelectHero, heroes[i]);
                Send(session, i + 1, OriginalSessionCommandKind.LobbyReady);
            }
            Assert.That(Send(session, 1, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.DrainEvents(); return session;
        }
        static void ReachWorldDuel(OriginalSession session)
        {
            var world = World(session);
            // Test harness bypasses PvE victory, while the real match referee
            // owns all round timers, setup events, ratings and duel scheduling.
            foreach (var unit in world.Snapshot().units) world.SetUnitState(unit.entityId, paused: true, invulnerable: true);
            for (int round = 1; round <= 4; round++)
            {
                Assert.That(session.Snapshot().round, Is.EqualTo(round)); Tick(session, 2.05);
                foreach (var player in session.Snapshot().players)
                    Assert.That(Send(session, player.slot, OriginalSessionCommandKind.WaveReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Tick(session, 3.1);
                foreach (var enemy in session.Snapshot().enemies)
                { world.ForceUnitDeath(OriginalWorld.EnemyEntityId(enemy.entityId)); Assert.That(session.ReportEnemyKilled(enemy.entityId, false), Is.True); }
                Tick(session, 3.1);
            }
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation)); Tick(session, 25.1);
        }

        [Test] public void SourceDuelStartsFromWorldRatingsAllowsPvpAndReturnsToNextWave()
        {
            var session = Start(); var world = World(session);
            Assert.That(Send(session, 1, OriginalSessionCommandKind.AttackTarget, target: 2), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            ReachWorldDuel(session);
            var state = session.Snapshot();
            Assert.That(state.pendingDuel, Is.False);
            Assert.That(state.duel.phase, Is.EqualTo(OriginalDuelPhase.Countdown));
            int first = state.players.Single(p => p.matchSlot == state.duel.firstSlot).slot;
            int second = state.players.Single(p => p.matchSlot == state.duel.secondSlot).slot;
            int spectator = state.players.Single(p => p.slot != first && p.slot != second).slot;
            Assert.That(world.UnitState(first).position.x, Is.EqualTo(-416));
            Assert.That(world.UnitState(second).position.x, Is.EqualTo(416));
            Assert.That(world.UnitState(first).position.y, Is.EqualTo(-2688));
            Assert.That(world.UnitState(spectator).hidden, Is.True);
            Tick(session, 11);
            Assert.That(world.UnitState(first).invulnerable || world.UnitState(first).paused, Is.False);
            Assert.That(Send(session, first, OriginalSessionCommandKind.AttackTarget, target: second), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            double beforeHealth = world.UnitState(second).health;
            Tick(session, 5);
            Assert.That(world.UnitState(second).health, Is.LessThan(beforeHealth), "The world resolves actual hostile weapon hits.");
            int altars = session.Snapshot().altars;
            typeof(OriginalSession).GetMethod("ApplyResolvedUnitHit", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(session, new object[] { first, first, world.UnitState(second), 100000d, null });
            Assert.That(session.Snapshot().altars, Is.EqualTo(altars));
            Assert.That(session.Snapshot().duel.phase, Is.EqualTo(OriginalDuelPhase.Resolving));
            Tick(session, 2.1);
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(Send(session, first, OriginalSessionCommandKind.AttackTarget, target: second), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Tick(session, 25.1);
            Assert.That(session.Snapshot().duel.kind, Is.EqualTo(OriginalDuelKind.Gladiator));
            Tick(session, 10.1);
            foreach (var opponent in session.Snapshot().players.Where(p => p.slot != first))
                typeof(OriginalSession).GetMethod("ApplyResolvedUnitHit", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(session, new object[] { first, first, world.UnitState(opponent.slot), 100000d, null });
            Tick(session, 2.1);
            Assert.That(session.Snapshot().round, Is.EqualTo(5));
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            Assert.That(world.Snapshot().units.Where(u => u.kind == OriginalWorldUnitKind.Hero).All(u => !u.hidden), Is.True);
            Assert.That(Send(session, first, OriginalSessionCommandKind.AttackTarget, target: second), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
        }
    }
}

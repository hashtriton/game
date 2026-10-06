using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionWorldTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public bool blocked;
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision { get; private set; }
            public bool IsWalkable(double x, double y, double r) => !blocked && Math.Abs(x) <= 4096 && Math.Abs(y) <= 4096;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) { NavigationRevision++; return true; }
        }
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        static readonly string Hash = new string('a', 64);
        static OriginalSession Create(Navigation navigation = null, OriginalDifficulty difficulty = OriginalDifficulty.Standard)
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                Load<OriginalCombatCatalog>("lia39-combat.json"), Load<OriginalDuelCatalog>("lia39-duels.json"), Hash,
                OriginalMatchOptions.ForDifficulty(difficulty), 123);
            session.ConfigureWorld(navigation ?? new Navigation(), Load<OriginalNativeCatalog>("lia39-native126.json"),
                new[] { new OriginalWorldDoodadView { editorId = 686, rawcode = "LTbr", position = new OriginalPoint(1376, 2464), health = 10, maxHealth = 10 } });
            return session;
        }
        static OriginalSessionCommand Command(long sequence, OriginalSessionCommandKind kind, string hero = null) =>
            new OriginalSessionCommand { sequence = sequence, kind = kind, protocol = OriginalSession.Protocol, contentHash = Hash, heroId = hero, ready = true };
        static void Start(OriginalSession session)
        {
            session.Apply(0, Command(1, OriginalSessionCommandKind.SelectHero, "H008"));
            session.Apply(0, Command(2, OriginalSessionCommandKind.LobbyReady));
            Assert.That(session.Apply(0, Command(3, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.DrainEvents();
        }
        [Test]
        public void WorldAppearsAtStartWithSourceHeroStatsAndBarrels()
        {
            var session = Create(); Assert.That(session.Snapshot().hasWorld, Is.False);
            Start(session); var view = session.Snapshot();
            Assert.That(view.hasWorld, Is.True); Assert.That(view.world.units.Length, Is.EqualTo(1));
            var hero = view.world.units[0];
            Assert.That(hero.ownerSlot, Is.EqualTo(1)); Assert.That(hero.health, Is.EqualTo(631));
            Assert.That(hero.mana, Is.EqualTo(145)); Assert.That(hero.profile.collisionRadius, Is.EqualTo(24));
            Assert.That(view.world.doodads.Single().editorId, Is.EqualTo(686));
            view.world.units[0].health = 1; view.world.doodads[0].health = 0;
            Assert.That(session.Snapshot().world.units[0].health, Is.EqualTo(631));
            Assert.That(session.Snapshot().world.doodads[0].health, Is.EqualTo(10));
        }
        [Test]
        public void MoveIsAnIntentWithHostSpeedAndStopCancelsIt()
        {
            var session = Create(); Start(session);
            var before = session.Snapshot().world.units[0].position;
            var command = Command(4, OriginalSessionCommandKind.Move); command.x = before.x + 1000; command.y = before.y;
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Snapshot().world.units[0].position.x, Is.EqualTo(before.x));
            session.Advance(1);
            Assert.That(session.Snapshot().world.units[0].position.x - before.x, Is.EqualTo(250).Within(.00001));
            Assert.That(session.Apply(0, Command(5, OriginalSessionCommandKind.Stop)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.Advance(1);
            Assert.That(session.Snapshot().world.units[0].position.x - before.x, Is.EqualTo(250).Within(.00001));
        }
        [Test]
        public void InvalidMoveCannotPoisonSnapshotOrReplayLater()
        {
            var session = Create(); Start(session);
            var command = Command(4, OriginalSessionCommandKind.Move); command.x = double.NaN;
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            command.x = 500; command.y = 1000;
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Snapshot().world.units[0].order, Is.EqualTo(OriginalWorldOrder.None));
        }
        [Test]
        public void AttackRequiresAnExistingTargetAndCannotBeAClientDamageReport()
        {
            var session = Create(); Start(session);
            var command = Command(4, OriginalSessionCommandKind.AttackTarget); command.targetKind = OriginalWorldTargetKind.Doodad; command.targetId = 999;
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            command.sequence = 5; command.targetId = 686;
            Assert.That(session.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var snapshot = session.Snapshot().world;
            Assert.That(snapshot.units[0].targetId, Is.EqualTo(686)); Assert.That(snapshot.doodads[0].health, Is.EqualTo(10));
        }

        [Test]
        public void FailedHeroPlacementKeepsLobbyAtomicAndAllowsANewStartCommand()
        {
            var navigation = new Navigation { blocked = true }; var session = Create(navigation);
            session.Apply(0, Command(1, OriginalSessionCommandKind.SelectHero, "H008"));
            session.Apply(0, Command(2, OriginalSessionCommandKind.LobbyReady));
            Assert.That(session.Apply(0, Command(3, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            var failed = session.Snapshot();
            Assert.That(failed.started, Is.False); Assert.That(failed.hasWorld, Is.False);
            Assert.That(failed.players[0].matchSlot, Is.Zero); Assert.That(failed.players[0].gold, Is.Zero);
            Assert.That(session.DrainEvents(), Is.Empty);
            navigation.blocked = false;
            Assert.That(session.Apply(0, Command(3, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Apply(0, Command(4, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Snapshot().hasWorld, Is.True);
            Assert.That(session.Snapshot().world.units.Single(u => u.ownerSlot == 1).health, Is.EqualTo(631));
        }

        // Death is a trusted world notification, never a client command. This
        // seam supplies its physical state without depending on combat AI.
        static OriginalWorld TrustedWorld(OriginalSession session) => (OriginalWorld)typeof(OriginalSession)
            .GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);

        static void EnterCombat(OriginalSession session)
        {
            for (int tick = 0; tick < 120 && session.Snapshot().phase != OriginalMatchPhase.Combat; tick++)
            { session.Advance(1); session.DrainEvents(); }
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.Combat));
        }

        [Test]
        public void AltarReviveFindsAFreePointWhenTheCorpsePositionIsOccupied()
        {
            var session = Create(difficulty: OriginalDifficulty.Easy); Start(session); EnterCombat(session);
            var world = TrustedWorld(session); var dead = world.Snapshot().units.Single(u => u.ownerSlot == 1);
            Assert.That(world.ApplyUnitDamage(dead.entityId, dead.health), Is.True);
            world.AddUnit(900000, 0, "hfoo", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 100 }, dead.position);
            Assert.That(session.ReportHeroDied(1), Is.True);
            for (int tick = 0; tick < 10; tick++) { session.Advance(1); session.DrainEvents(); }
            var revived = session.Snapshot().world.units.Single(u => u.ownerSlot == 1);
            double dx = revived.position.x - dead.position.x, dy = revived.position.y - dead.position.y;
            Assert.That(revived.health, Is.EqualTo(631)); Assert.That(revived.mana, Is.EqualTo(145));
            Assert.That(dx * dx + dy * dy, Is.GreaterThanOrEqualTo(48 * 48));
            Assert.That(session.Snapshot().players[0].alive, Is.True); Assert.That(session.HaltReason, Is.Null);
        }

        [Test]
        public void AltarReviveHaltsExplicitlyWhenNoValidPlacementExists()
        {
            var navigation = new Navigation(); var session = Create(navigation, OriginalDifficulty.Easy);
            Start(session); EnterCombat(session);
            var world = TrustedWorld(session); var hero = world.Snapshot().units.Single(u => u.ownerSlot == 1);
            world.ApplyUnitDamage(hero.entityId, hero.health);
            Assert.That(session.ReportHeroDied(1), Is.True); navigation.blocked = true;
            for (int tick = 0; tick < 10 && session.HaltReason == null; tick++) { session.Advance(1); session.DrainEvents(); }
            Assert.That(session.HaltReason, Does.StartWith("world-restore-placement-unavailable:"));
            Assert.That(session.Snapshot().world.units.Single(u => u.ownerSlot == 1).health, Is.Zero);
            double before = session.Snapshot().time; session.Advance(1);
            Assert.That(session.Snapshot().time, Is.EqualTo(before));
        }

        [Test]
        public void ReusedLobbySlotCannotRedirectAConnectionToAnotherHero()
        {
            var session = Create();
            session.Apply(10, Command(1, OriginalSessionCommandKind.Hello));
            session.Apply(20, Command(1, OriginalSessionCommandKind.Hello));
            Assert.That(session.Disconnect(10), Is.True);
            session.Apply(30, Command(1, OriginalSessionCommandKind.Hello));
            session.Apply(0, Command(1, OriginalSessionCommandKind.SelectHero, "H008"));
            session.Apply(20, Command(2, OriginalSessionCommandKind.SelectHero, "H024"));
            session.Apply(30, Command(2, OriginalSessionCommandKind.SelectHero, "N0A0"));
            session.Apply(0, Command(2, OriginalSessionCommandKind.LobbyReady));
            session.Apply(20, Command(3, OriginalSessionCommandKind.LobbyReady));
            session.Apply(30, Command(3, OriginalSessionCommandKind.LobbyReady));
            Assert.That(session.Apply(0, Command(3, OriginalSessionCommandKind.Start)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.DrainEvents(); var before = session.Snapshot();
            Assert.That(before.players.Select(p => p.slot), Is.EqualTo(new[] { 1, 3, 2 }));
            Assert.That(before.players.Select(p => p.matchSlot), Is.EqualTo(new[] { 1, 2, 3 }));
            var moving = before.world.units.Single(u => u.ownerSlot == 3);
            var move = Command(4, OriginalSessionCommandKind.Move); move.x = moving.position.x; move.y = moving.position.y - 500;
            Assert.That(session.Apply(20, move), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(10, move), Is.EqualTo(OriginalSessionReplyCode.UnknownConnection));
            session.Advance(.1); var after = session.Snapshot();
            Assert.That(after.world.units.Single(u => u.ownerSlot == 3).entityId, Is.EqualTo(3));
            Assert.That(after.world.units.Single(u => u.ownerSlot == 3).position.y, Is.LessThan(moving.position.y));
            foreach (int slot in new[] { 1, 2 })
            {
                var first = before.world.units.Single(u => u.ownerSlot == slot);
                var last = after.world.units.Single(u => u.ownerSlot == slot);
                Assert.That(last.position.x, Is.EqualTo(first.position.x)); Assert.That(last.position.y, Is.EqualTo(first.position.y));
                Assert.That(last.order, Is.EqualTo(OriginalWorldOrder.None));
            }
        }
    }
}

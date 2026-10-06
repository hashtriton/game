using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionHeroSelectionTests
    {
        static readonly string Hash = new string('a', 64);
        static readonly string[] Pool = { "H008", "H024", "N0A0" };
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(
            Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));

        sealed class Navigation : IOriginalWorldNavigation
        {
            public bool blocked;
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => !blocked && Math.Abs(x) < 4096 && Math.Abs(y) < 4096;
            public bool SegmentClear(OriginalPoint first, OriginalPoint second, double radius) => IsWalkable(second.x, second.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint first, OriginalPoint second, double radius) => new[] { second };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }

        sealed class Fixture
        {
            public readonly OriginalSession session;
            public readonly Navigation navigation;
            public readonly Dictionary<long, long> sequences = new Dictionary<long, long> { { 0, 0 } };
            public readonly long[] connections;
            public Fixture(OriginalHeroSelection mode, int count = 1, int seed = 123, bool blocked = false)
            {
                var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
                options.heroSelection = mode;
                session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                    Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), Hash, options, seed);
                var native = Load<OriginalNativeCatalog>("native126");
                var observed = Load<OriginalObservedCatalog>("observed126");
                navigation = new Navigation { blocked = blocked };
                session.ConfigureWorld(navigation, native, Array.Empty<OriginalWorldDoodadView>(), observed);
                session.ConfigureProgression(native, observed);
                connections = Enumerable.Range(0, count).Select(i => (long)i * 10).ToArray();
                foreach (long connection in connections.Skip(1))
                    Assert.That(Send(connection, OriginalSessionCommandKind.Hello), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            public OriginalSessionReplyCode Send(long connection, OriginalSessionCommandKind kind, string hero = null, bool ready = true)
            {
                sequences.TryGetValue(connection, out long previous);
                long next = previous + 1; sequences[connection] = next;
                return session.Apply(connection, new OriginalSessionCommand { sequence = next, kind = kind,
                    protocol = OriginalSession.Protocol, contentHash = Hash, heroId = hero, ready = ready });
            }
            public void ReadyAll()
            {
                foreach (long connection in connections)
                    Assert.That(Send(connection, OriginalSessionCommandKind.LobbyReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            public string[] Heroes() => session.Snapshot().players.Select(p => p.heroId).ToArray();
        }

        static void AssertWire(OriginalSession session)
        {
            var view = session.Snapshot();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                code = OriginalSessionReplyCode.Accepted, acknowledgedSequence = view.players[0].acknowledgedSequence, snapshot = view };
            var codec = new OriginalUnitySessionCodec();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.True);
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void RandomLobbyCanReadyWithoutRevealingHeroesAndRoundTrips(OriginalHeroSelection mode)
        {
            var f = new Fixture(mode, 3); f.ReadyAll();
            Assert.That(f.Heroes(), Is.All.Null);
            Assert.That(f.session.Snapshot().players.Select(p => p.lobbyReady), Is.All.True);
            AssertWire(f.session);
            var tampered = f.session.Snapshot(); tampered.options.heroSelection = OriginalHeroSelection.Free;
            var codec = new OriginalUnitySessionCodec();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse
            { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1, code = OriginalSessionReplyCode.Accepted,
                acknowledgedSequence = tampered.players[0].acknowledgedSequence, snapshot = tampered }), out _), Is.False);
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void RandomModeRejectsManualHeroSelection(OriginalHeroSelection mode)
        {
            var f = new Fixture(mode);
            Assert.That(f.Send(0, OriginalSessionCommandKind.SelectHero, "H008"), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(f.Send(0, OriginalSessionCommandKind.SelectHero, "H999"), Is.EqualTo(OriginalSessionReplyCode.InvalidHero));
            Assert.That(f.Heroes(), Is.All.Null);
        }

        [Test] public void RandomThreeReceivesTheCurrentPoolWithoutDuplicatesAndCorrectWorldProfiles()
        {
            var f = new Fixture(OriginalHeroSelection.Random, 3); f.ReadyAll();
            Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Heroes(), Is.EquivalentTo(Pool));
            var view = f.session.Snapshot();
            foreach (var player in view.players)
            {
                Assert.That(player.progression.heroId, Is.EqualTo(player.heroId));
                Assert.That(view.world.units.Single(u => u.ownerSlot == player.slot).rawcode, Is.EqualTo(player.heroId));
                Assert.That(player.inventory.ownerId, Is.EqualTo(player.slot));
                Assert.That(player.gold, Is.EqualTo(130));
            }
            AssertWire(f.session);
        }

        [Test] public void SameRandomSupportsEightOwnersWithOneSharedHeroType()
        {
            var f = new Fixture(OriginalHeroSelection.SameRandom, 8); f.ReadyAll();
            Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Heroes().Distinct().Count(), Is.EqualTo(1));
            Assert.That(Pool, Does.Contain(f.Heroes()[0]));
            Assert.That(f.session.Snapshot().world.units.Length, Is.EqualTo(8));
            AssertWire(f.session);
        }

        [TestCase(OriginalHeroSelection.Free)]
        [TestCase(OriginalHeroSelection.Random)]
        public void UniqueModesCannotAdmitMorePlayersThanTheirHeroPool(OriginalHeroSelection mode)
        {
            var f = new Fixture(mode, 3);
            Assert.That(f.Send(30, OriginalSessionCommandKind.Hello), Is.EqualTo(OriginalSessionReplyCode.Full));
            Assert.That(f.session.Snapshot().players.Length, Is.EqualTo(3));
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void UnreadyAndNonHostStartCannotAssignHeroes(OriginalHeroSelection mode)
        {
            var f = new Fixture(mode, 2);
            Assert.That(f.Send(0, OriginalSessionCommandKind.LobbyReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(f.Heroes(), Is.All.Null);
            Assert.That(f.Send(10, OriginalSessionCommandKind.LobbyReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Send(10, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.NotHost));
            Assert.That(f.Heroes(), Is.All.Null);
            Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void SameSeedAndRosterProduceTheSameAssignmentsWithoutPostStartRerolls(OriginalHeroSelection mode)
        {
            var first = new Fixture(mode, 3, 6789); var second = new Fixture(mode, 3, 6789);
            first.ReadyAll(); second.ReadyAll();
            Assert.That(first.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(second.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var heroes = first.Heroes(); Assert.That(second.Heroes(), Is.EqualTo(heroes));
            Assert.That(first.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.AlreadyStarted));
            Assert.That(first.Send(0, OriginalSessionCommandKind.SelectHero, "H008"), Is.EqualTo(OriginalSessionReplyCode.AlreadyStarted));
            Assert.That(first.Heroes(), Is.EqualTo(heroes));
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void FailedPlacementRollsBackHeroesAndKeepsTheSameRetryResult(OriginalHeroSelection mode)
        {
            var failed = new Fixture(mode, 3, 45, blocked: true); failed.ReadyAll();
            Assert.That(failed.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(failed.Heroes(), Is.All.Null);
            Assert.That(failed.session.Snapshot().started, Is.False);
            Assert.That(failed.session.Snapshot().players.Select(p => p.gold), Is.All.EqualTo(0));
            AssertWire(failed.session);
            failed.navigation.blocked = false;
            Assert.That(failed.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var clean = new Fixture(mode, 3, 45); clean.ReadyAll(); clean.Send(0, OriginalSessionCommandKind.Start);
            Assert.That(failed.Heroes(), Is.EqualTo(clean.Heroes()));
        }

        [Test] public void OversizedRosterCannotSwitchToUniqueRandom()
        {
            var f = new Fixture(OriginalHeroSelection.Duplicates, 4);
            var next = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            next.heroSelection = OriginalHeroSelection.Random;
            Assert.That(f.session.SetOptions(next), Is.False);
            Assert.That(f.session.Snapshot().options.heroSelection, Is.EqualTo(OriginalHeroSelection.Duplicates));
        }

        [TestCase(OriginalHeroSelection.Random)]
        [TestCase(OriginalHeroSelection.SameRandom)]
        public void DifferentSeedsCanAssignEachOfTheThreeApprovedHeroes(OriginalHeroSelection mode)
        {
            var seen = new HashSet<string>();
            foreach (int seed in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 123, 456, 6789, 100000, -1, -37, int.MinValue, int.MaxValue })
            {
                var f = new Fixture(mode, seed: seed); f.ReadyAll();
                Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                seen.Add(f.Heroes()[0]);
            }
            Assert.That(seen, Is.EquivalentTo(Pool));
        }

        [TestCase(OriginalHeroSelection.Free)]
        [TestCase(OriginalHeroSelection.Duplicates)]
        public void ManualModesStillRequireASelectedHero(OriginalHeroSelection mode)
        {
            var f = new Fixture(mode);
            Assert.That(f.Send(0, OriginalSessionCommandKind.LobbyReady), Is.EqualTo(OriginalSessionReplyCode.InvalidHero));
            Assert.That(f.Send(0, OriginalSessionCommandKind.SelectHero, "H008"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Send(0, OriginalSessionCommandKind.LobbyReady), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Send(0, OriginalSessionCommandKind.Start), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.Heroes(), Is.EqualTo(new[] { "H008" }));
        }
    }
}

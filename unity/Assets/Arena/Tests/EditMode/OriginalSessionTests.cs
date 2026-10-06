using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionTests
    {
        private static readonly string Hash = new string('a', 64);
        private static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));
        private static OriginalSession Session() => new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"),
            Load<OriginalItemCatalog>("lia39-items.json"), Load<OriginalCombatCatalog>("lia39-combat.json"),
            Load<OriginalDuelCatalog>("lia39-duels.json"), Hash,
            OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);

        private static OriginalSessionCommand Command(OriginalSessionCommandKind kind, long sequence, string hero = null) =>
            new OriginalSessionCommand { kind = kind, sequence = sequence, protocol = OriginalSession.Protocol,
                contentHash = Hash, heroId = hero, ready = true };

        [Test]
        public void AlteredRosterIsRejectedBeforeAPlayerCanSelectIt()
        {
            var catalog = Load<OriginalCombatCatalog>("lia39-combat.json");
            catalog.selectedHeroes[0].id = "BAD";
            catalog.units.First(x => x.id == "H008").id = "BAD";
            Assert.Throws<System.ArgumentException>(() => catalog.BuildIndexes());
        }

        [Test]
        public void Handshake_RejectsDifferentContentAndUnknownConnections()
        {
            var session = Session();
            var hello = Command(OriginalSessionCommandKind.Hello, 1);
            hello.contentHash = new string('b', 64);
            Assert.That(session.Apply(10, hello), Is.EqualTo(OriginalSessionReplyCode.VersionMismatch));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.SelectHero, 2, "H008")), Is.EqualTo(OriginalSessionReplyCode.UnknownConnection));
            Assert.That(session.Snapshot().players.Length, Is.EqualTo(1));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.Hello, 1)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.Hello, 1)), Is.EqualTo(OriginalSessionReplyCode.AlreadyJoined));
        }

        [Test]
        public void Lobby_RestrictsRosterAndHostAuthorityAndConsumesRejectedSequences()
        {
            var session = Session();
            session.Apply(10, Command(OriginalSessionCommandKind.Hello, 1));
            Assert.That(session.Apply(0, Command(OriginalSessionCommandKind.SelectHero, 1, "H008")), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.SelectHero, 2, "H008")), Is.EqualTo(OriginalSessionReplyCode.HeroTaken));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.SelectHero, 2, "H024")), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.SelectHero, 3, "H999")), Is.EqualTo(OriginalSessionReplyCode.InvalidHero));
            Assert.That(session.Apply(10, Command(OriginalSessionCommandKind.Start, 4)), Is.EqualTo(OriginalSessionReplyCode.NotHost));
            Assert.That(session.Apply(0, Command(OriginalSessionCommandKind.Start, 2)), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(session.Snapshot().started, Is.False);
        }

        [Test]
        public void ThreePlayers_StartWithOriginalResourcesAndImmutableInitialPopulation()
        {
            var session = Session();
            session.Apply(10, Command(OriginalSessionCommandKind.Hello, 1));
            session.Apply(20, Command(OriginalSessionCommandKind.Hello, 1));
            session.Apply(0, Command(OriginalSessionCommandKind.SelectHero, 1, "H008"));
            session.Apply(10, Command(OriginalSessionCommandKind.SelectHero, 2, "N0A0"));
            session.Apply(20, Command(OriginalSessionCommandKind.SelectHero, 2, "H024"));
            session.Apply(0, Command(OriginalSessionCommandKind.LobbyReady, 2));
            session.Apply(10, Command(OriginalSessionCommandKind.LobbyReady, 3));
            session.Apply(20, Command(OriginalSessionCommandKind.LobbyReady, 3));
            Assert.That(session.Apply(0, Command(OriginalSessionCommandKind.Start, 3)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var before = session.Snapshot();
            Assert.That(before.initialParticipants, Is.EqualTo(3));
            Assert.That(before.players.Select(x => x.gold), Is.All.EqualTo(130));
            Assert.That(before.players.Select(x => x.souls), Is.All.EqualTo(4));
            Assert.That(before.players.Select(x => x.heroId), Is.EqualTo(new[] { "H008", "N0A0", "H024" }));
            Assert.That(before.players.Select(x => x.lobbyReady), Is.All.True);
            Assert.That(before.players.Select(x => x.waveReady), Is.All.False);
            session.Advance(1); session.Advance(1);
            session.Apply(10, Command(OriginalSessionCommandKind.WaveReady, 4));
            var voted = session.Snapshot();
            Assert.That(voted.players.Single(x => x.heroId == "N0A0").waveReady, Is.True);
            Assert.That(voted.players.Where(x => x.heroId != "N0A0").Select(x => x.waveReady), Is.All.False);
            Assert.That(session.Apply(30, Command(OriginalSessionCommandKind.Hello, 1)), Is.EqualTo(OriginalSessionReplyCode.AlreadyStarted));
            Assert.That(session.Disconnect(10), Is.True);
            var after = session.Snapshot();
            Assert.That(after.initialParticipants, Is.EqualTo(3));
            Assert.That(after.players.Length, Is.EqualTo(3));
            Assert.That(after.players.Single(x => x.heroId == "N0A0").connected, Is.False);
            Assert.That(after.haltReason, Is.Null);
            session.Advance(1);
            Assert.That(session.Snapshot().time, Is.GreaterThan(after.time));
        }

        [Test]
        public void Snapshots_CannotMutateAuthorityAndRoundTripThroughUnityJson()
        {
            var session = Session();
            session.Apply(0, Command(OriginalSessionCommandKind.SelectHero, 1, "H008"));
            session.Apply(0, Command(OriginalSessionCommandKind.LobbyReady, 2));
            session.Apply(0, Command(OriginalSessionCommandKind.Start, 3));
            for (var i = 0; i < 2; i++) session.Advance(1);
            Assert.That(session.Apply(0, Command(OriginalSessionCommandKind.WaveReady, 4)), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.Advance(1);
            var snapshot = session.Snapshot();
            Assert.That(snapshot.enemies.Length, Is.GreaterThan(0));
            var roundTrip = JsonUtility.FromJson<OriginalSessionView>(JsonUtility.ToJson(snapshot));
            Assert.That(roundTrip.enemies[0].rawcode, Is.EqualTo(snapshot.enemies[0].rawcode));
            Assert.That(roundTrip.options.difficulty, Is.EqualTo(OriginalDifficulty.Standard));
            Assert.That(roundTrip.options.casters, Is.True);
            snapshot.options.casters = false;
            snapshot.players[0].gold = 999999;
            snapshot.enemies[0].rawcode = "FAKE";
            snapshot.enemies[0].entityId = -1;
            var authoritative = session.Snapshot();
            Assert.That(authoritative.players[0].gold, Is.EqualTo(130));
            Assert.That(authoritative.enemies[0].rawcode, Is.Not.EqualTo("FAKE"));
            Assert.That(authoritative.enemies[0].entityId, Is.GreaterThan(0));
            Assert.That(authoritative.options.casters, Is.True);
            Assert.That(session.Apply(0, Command(OriginalSessionCommandKind.WaveReady, 4)), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
        }
    }
}

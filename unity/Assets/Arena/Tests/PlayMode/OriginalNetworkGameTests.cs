#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Arena.Tests
{
    public sealed class OriginalNetworkGameTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private TextAsset[] assets;
        private OriginalGameCatalogs catalogs;
        private float previousTimeScale;

        [SetUp]
        public void Setup()
        {
            assets = new[] { "layout", "match", "combat", "items", "item-passives", "duels", "native126", "observed126", "observed-items126" }.Select(name =>
                AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-" + name + ".json")).ToArray();
            catalogs = OriginalGameCatalogs.Load(assets);
            previousTimeScale = Time.timeScale;
        }

        private OriginalNetworkGame Player(string name, OriginalGameCatalogs data = null)
        {
            var gameObject = new GameObject(name);
            objects.Add(gameObject);
            var result = gameObject.AddComponent<OriginalNetworkGame>();
            result.Configure(data ?? catalogs);
            return result;
        }

        private static IEnumerator Wait(Func<bool> predicate, string message)
        {
            var until = Time.realtimeSinceStartupAsDouble + 8;
            while (!predicate() && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(predicate(), Is.True, message);
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            foreach (var value in objects) if (value) UnityEngine.Object.Destroy(value);
            objects.Clear();
            Time.timeScale = previousTimeScale;
            yield return null;
        }

        [UnityTest]
        public IEnumerator HostAndTwoClientsChooseThreeHeroesStartAndReplicateWhileTimeScaleIsZero()
        {
            Time.timeScale = 0;
            var host = Player("Test host");
            var mapObject = new GameObject("Three-player source map");objects.Add(mapObject);
            var map=mapObject.AddComponent<ArenaMap>();map.layoutJson=assets.First(x=>x.name=="lia39-layout");
            host.ConfigureWorld(new OriginalMapNavigation(map));
            var archer = Player("Test archer client");
            var pyro = Player("Test pyro client");
            Assert.That(host.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 77), Is.True, host.Error);
            var port = host.Port;
            Assert.That(archer.Join("127.0.0.1", port), Is.True);
            Assert.That(pyro.Join("127.0.0.1", port), Is.True);
            yield return Wait(() => archer.State == OriginalConnectionState.Lobby && pyro.State == OriginalConnectionState.Lobby,
                "Both clients must complete the JSON/wire/TCP handshake through Update.");
            Assert.That(archer.LocalSlot, Is.InRange(2, 3));
            Assert.That(pyro.LocalSlot, Is.Not.EqualTo(archer.LocalSlot));
            host.SendCommand(OriginalSessionCommandKind.SelectHero, "H008");
            archer.SendCommand(OriginalSessionCommandKind.SelectHero, "N0A0");
            pyro.SendCommand(OriginalSessionCommandKind.SelectHero, "H024");
            archer.SendCommand(OriginalSessionCommandKind.Start);
            yield return Wait(() => archer.LastReply?.code == OriginalSessionReplyCode.NotHost, "A client cannot start the match.");
            host.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            archer.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            pyro.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            yield return Wait(() => host.View.players.Length == 3 && host.View.players.All(x => x.lobbyReady), "All source heroes are ready.");
            host.SendCommand(OriginalSessionCommandKind.Start);
            yield return Wait(() => archer.View.started && pyro.View.started, "Both clients see the same started session.");
            Assert.That(archer.View.hasWorld&&pyro.View.hasWorld,Is.True);
            Assert.That(pyro.View.world.doodads.Where(x=>x.rawcode=="LTex").Select(x=>x.invulnerable),Is.Not.Empty.And.All.True);
            Assert.That(archer.View.players.Select(x => x.heroId), Is.EquivalentTo(new[] { "H008", "N0A0", "H024" }));
            Assert.That(pyro.View.players.Select(x => x.gold), Is.All.EqualTo(130));
            Assert.That(pyro.View.players.Select(x => x.souls), Is.All.EqualTo(4));
            host.AdvanceHost(0.5);
            host.DrainHostEvents();
            yield return Wait(() => host.View.time >= .5-1e-8 && archer.View.time >= .5-1e-8 && pyro.View.time >= .5-1e-8,
                "Authoritative time reaches the host view and both clients after the network pump.");
            Assert.That(host.View.time,Is.EqualTo(.5).Within(1e-8));
            var disconnectedSlot = archer.LocalSlot;
            archer.Disconnect();
            yield return Wait(() => !pyro.View.players.Single(x => x.slot == disconnectedSlot).connected, "Disconnect reaches the remaining client explicitly.");
            Assert.That(pyro.View.players.Single(x => x.slot == disconnectedSlot).connected, Is.False);
            Assert.That(pyro.View.initialParticipants, Is.EqualTo(3));
            Assert.That(pyro.View.haltReason, Is.Null.Or.Empty);
            host.AdvanceHost(.5); host.DrainHostEvents();
            yield return Wait(() => pyro.View.time >= 1-1e-8, "The host keeps advancing after a teammate leaves.");
            Assert.That(pyro.State, Is.EqualTo(OriginalConnectionState.Playing));
            host.Disconnect();
            yield return Wait(() => pyro.State == OriginalConnectionState.Faulted, "Host shutdown is visible to the client.");
        }

        [UnityTest]
        public IEnumerator DifferentDataIsRejectedAndTheRuntimeCanReconnectAfterDisconnect()
        {
            var host = Player("Compatibility host");
            var changed = new TextAsset(assets[0].text + "\n") { name = assets[0].name };
            OriginalGameCatalogs different;
            try { different = OriginalGameCatalogs.Load(new[] { changed }.Concat(assets.Skip(1))); }
            finally { UnityEngine.Object.Destroy(changed); }
            var client = Player("Different build", different);
            Assert.That(host.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 11), Is.True);
            Assert.That(client.Join("127.0.0.1", host.Port), Is.True);
            yield return Wait(() => client.State == OriginalConnectionState.Faulted, "Fingerprint mismatch must reject joining.");
            Assert.That(client.LastReply?.code, Is.EqualTo(OriginalSessionReplyCode.VersionMismatch));
            Assert.That(host.View.players.Length, Is.EqualTo(1));
            client.Disconnect();
            client.Configure(catalogs);
            Assert.That(client.Join("127.0.0.1", host.Port), Is.True);
            yield return Wait(() => client.State == OriginalConnectionState.Lobby, "The same component reconnects with valid data.");
            client.Disconnect();
            yield return Wait(() => host.View.players.Length == 1, "Lobby departure frees the slot.");
            host.Disconnect();
            Assert.That(host.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 12), Is.True);
            Assert.That(host.LocalSlot, Is.EqualTo(1));
            Assert.That(host.SendCommand(OriginalSessionCommandKind.SelectHero, "H008"), Is.True);
            Assert.That(host.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }

        [UnityTest]
        public IEnumerator RemoteCommandsAndFirstWaveCombatReplicateTheAuthoritativeWorld()
        {
            Time.timeScale = 0;
            var mapObject = new GameObject("Network combat source map"); objects.Add(mapObject);
            var map = mapObject.AddComponent<ArenaMap>();
            map.layoutJson = assets.First(x => x.name == "lia39-layout");
            var navigation = new OriginalMapNavigation(map);
            var host = Player("Combat host"); var client = Player("Combat archer");
            host.ConfigureWorld(navigation);
            Assert.That(host.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 77), Is.True, host.Error);
            Assert.That(client.Join("127.0.0.1", host.Port), Is.True);
            yield return Wait(() => client.State == OriginalConnectionState.Lobby, "Combat client joins over TCP.");
            host.SendCommand(OriginalSessionCommandKind.SelectHero, "H008");
            client.SendCommand(OriginalSessionCommandKind.SelectHero, "N0A0");
            host.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            client.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            yield return Wait(() => host.View.players.Length == 2 && host.View.players.All(x => x.lobbyReady), "Both heroes ready.");
            host.SendCommand(OriginalSessionCommandKind.Start);
            yield return Wait(() => client.View.started && client.View.hasWorld, "World is replicated to the remote peer.");
            var ownId = OriginalWorld.HeroEntityId(client.LocalSlot);
            var initial = client.View.world.units.Single(x => x.entityId == ownId).position;
            var destination = new OriginalPoint(initial.x + 128, initial.y);
            Assert.That(navigation.IsWalkable(destination.x, destination.y, 24), Is.True);
            client.SendCommand(OriginalSessionCommandKind.Move, x: destination.x, y: destination.y, actorEntityId: 1);
            yield return Wait(() => client.LastReply?.commandSequence == 4, "Foreign actor rejection arrives through the wire.");
            Assert.That(client.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            client.SendCommand(OriginalSessionCommandKind.Move, x: destination.x, y: destination.y);
            yield return Wait(() => client.LastReply?.commandSequence == 5, "Own movement intent reaches the host.");
            Assert.That(client.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
            for (int i = 0; i < 3; i++) { host.AdvanceHost(1); host.DrainHostEvents(); yield return null; }
            // Sixty 0.05s steps need not sum to exactly three in binary floating
            // point. Compare replication at the actual authoritative revision.
            yield return Wait(() => host.View.world.time >= 3 - 1e-8 && client.View.revision == host.View.revision,
                "The client receives the current host simulation revision.");
            Assert.That(client.View.world.time, Is.EqualTo(3).Within(1e-8));
            Assert.That(client.View.world.units.Single(x => x.entityId == ownId).position.x,
                Is.EqualTo(host.View.world.units.Single(x => x.entityId == ownId).position.x).Within(1e-8));
            Assert.That(client.View.world.units.Single(x => x.entityId == ownId).position.x, Is.Not.EqualTo(initial.x));
            var learn = client.View.players.Single(x => x.slot == client.LocalSlot).learning.First(x => x.code == OriginalLearnCode.Available);
            client.SendCommand(OriginalSessionCommandKind.LearnSkill, skillId: learn.id);
            yield return Wait(() => client.LastReply?.commandSequence == 6, "Learning is acknowledged by the authority.");
            Assert.That(client.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
            host.SendCommand(OriginalSessionCommandKind.WaveReady);
            client.SendCommand(OriginalSessionCommandKind.WaveReady);
            yield return Wait(() => client.LastReply?.commandSequence == 7, "Both readiness intents arrive before advancing.");
            Assert.That(client.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
            for (int i = 0; i < 28; i++)
            {
                host.AdvanceHost(1); host.DrainHostEvents();
                Assert.That(host.View.haltReason, Is.Null.Or.Empty, "Source rule resolution must not halt combat.");
                yield return null;
            }
            // AdvanceHost intentionally leaves publication to Update's 10Hz
            // cadence. A final command publishes the settled session now, so
            // this assertion cannot wait for an already superseded snapshot.
            host.SendCommand(OriginalSessionCommandKind.Stop);
            long revision = host.View.revision;
            yield return Wait(() => client.View.revision == revision, "Final combat snapshot arrives unchanged.");
            Assert.That(host.View.world.units.Any(x => x.attackSequence > 0), Is.True, "Real weapon cycles must run.");
            Assert.That(host.View.world.units.Any(x => x.health < x.profile.maxHealth), Is.True, "Real damage must be applied.");
            Assert.That(client.View.remainingEnemies, Is.EqualTo(host.View.remainingEnemies));
            Assert.That(client.View.players.Select(x => x.experience), Is.EqualTo(host.View.players.Select(x => x.experience)));
            foreach (var authoritative in host.View.world.units)
            {
                var remote = client.View.world.units.Single(x => x.entityId == authoritative.entityId);
                // JsonUtility's decimal encoding can round the last binary
                // digit. This tolerance is far below a Warcraft pathing unit.
                Assert.That(remote.health, Is.EqualTo(authoritative.health).Within(1e-8));
                Assert.That(remote.position.x, Is.EqualTo(authoritative.position.x).Within(1e-8));
                Assert.That(remote.position.y, Is.EqualTo(authoritative.position.y).Within(1e-8));
                Assert.That(remote.attackSequence, Is.EqualTo(authoritative.attackSequence));
            }
        }

        [UnityTest]
        public IEnumerator ClosingPendingConnectionDisposesItAndPublicBindIsRejected()
        {
            var game = Player("Cancelled connection");
            Assert.That(game.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 0, bindAddress: "0.0.0.0"), Is.False);
            Assert.That(game.State, Is.EqualTo(OriginalConnectionState.Disconnected));
            // Reserve then close a loopback port, without contacting any external host.
            int closedPort;
            using (var temporary = new OriginalTcpHost()) closedPort = temporary.Port;
            Assert.That(game.Join("127.0.0.1", closedPort), Is.True);
            game.Disconnect();
            yield return null;
            Assert.That(game.State, Is.EqualTo(OriginalConnectionState.Disconnected));
            Assert.That(game.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 0), Is.True);
            Assert.That(game.SendCommand((OriginalSessionCommandKind)999), Is.False);
            Assert.That(game.SendCommand(OriginalSessionCommandKind.SelectHero, "H008"), Is.True);
            Assert.That(game.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }

        [Test]
        public void HostSnapshotCallbackCanDisconnectWithoutPublishingAnOldReply()
        {
            var game = Player("Disconnecting host");
            Assert.That(game.Host(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 1), Is.True);
            game.SnapshotChanged += _ => game.Disconnect();
            Assert.DoesNotThrow(() => game.SendCommand(OriginalSessionCommandKind.SelectHero, "H008"));
            Assert.That(game.State, Is.EqualTo(OriginalConnectionState.Disconnected));
            Assert.That(game.View, Is.Null);
            Assert.That(game.LastReply, Is.Null);
        }

        [Test]
        public void RehostingInsideSnapshotCallbackStopsOldNotificationsAndKeepsTheNewSequence()
        {
            var game = Player("Replacing host");
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            Assert.That(game.Host(options, 1), Is.True);
            Action<OriginalSessionView> replace = null;
            replace = _ =>
            {
                game.SnapshotChanged -= replace;
                game.Disconnect();
                Assert.That(game.Host(options, 2), Is.True);
            };
            game.SnapshotChanged += replace;
            var staleNotifications = 0;
            game.SnapshotChanged += view => { if (view.revision > 0) staleNotifications++; };
            Assert.DoesNotThrow(() => game.SendCommand(OriginalSessionCommandKind.SelectHero, "H008"));
            Assert.That(staleNotifications, Is.Zero, "The old event dispatch must stop after replacing its session.");
            Assert.That(game.LastReply, Is.Null, "The old command must not overwrite the new session's reply.");
            Assert.That(game.View.players.Single().heroId, Is.Null.Or.Empty);
            Assert.That(game.SendCommand(OriginalSessionCommandKind.SelectHero, "H024"), Is.True);
            Assert.That(game.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(game.LastReply.acknowledgedSequence, Is.EqualTo(1));
        }

        [Test]
        public void AnOldHostCallbackFailureCannotCloseItsReplacement()
        {
            var game = Player("Replacing host during creation");
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            Action<OriginalSessionView> replace = null;
            replace = _ =>
            {
                game.SnapshotChanged -= replace;
                game.Disconnect();
                Assert.That(game.Host(options, 2), Is.True);
                throw new InvalidOperationException("Observer failed after replacing the session.");
            };
            game.SnapshotChanged += replace;
            Assert.That(game.Host(options, 1), Is.False);
            Assert.That(game.State, Is.EqualTo(OriginalConnectionState.Lobby));
            Assert.That(game.IsHost, Is.True);
            Assert.That(game.Error, Is.Null);
            Assert.That(game.SendCommand(OriginalSessionCommandKind.SelectHero, "H008"), Is.True);
            Assert.That(game.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }

        [UnityTest]
        public IEnumerator ClientReplyCallbackCanJoinAnotherHostWithoutConsumingOldResponses()
        {
            var first = Player("First host");
            var second = Player("Second host");
            var game = Player("Reconnecting client");
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            Assert.That(first.Host(options, 1), Is.True);
            Assert.That(second.Host(options, 2), Is.True);
            Assert.That(game.Join("127.0.0.1", first.Port), Is.True);
            yield return Wait(() => game.State == OriginalConnectionState.Lobby, "Initial join completes.");
            var replaced = false;
            Action<OriginalNetworkResponse> replace = null;
            replace = reply =>
            {
                if (reply.commandSequence != 2) return;
                game.CommandReplied -= replace;
                game.Disconnect();
                Assert.That(game.Join("127.0.0.1", second.Port), Is.True);
                replaced = true;
            };
            game.CommandReplied += replace;
            game.SendCommand(OriginalSessionCommandKind.SelectHero, "H024");
            yield return Wait(() => replaced && game.State == OriginalConnectionState.Lobby,
                "The replacement join must complete without processing the old snapshot.");
            Assert.That(game.View.players.All(x => string.IsNullOrEmpty(x.heroId)), Is.True);
            game.SendCommand(OriginalSessionCommandKind.SelectHero, "N0A0");
            yield return Wait(() => game.LastReply?.commandSequence == 2, "The new connection owns its sequence.");
            Assert.That(game.LastReply.code, Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }

        [UnityTest]
        public IEnumerator ClientSnapshotCallbackCanBecomeAHost()
        {
            var host = Player("Original host");
            var game = Player("Client becoming host");
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            Assert.That(host.Host(options, 1), Is.True);
            Assert.That(game.Join("127.0.0.1", host.Port), Is.True);
            yield return Wait(() => game.State == OriginalConnectionState.Lobby, "Initial join completes.");
            Action<OriginalSessionView> replace = null;
            replace = view =>
            {
                if (!view.players.Any(x => x.heroId == "H024")) return;
                game.SnapshotChanged -= replace;
                game.Disconnect();
                Assert.That(game.Host(options, 2), Is.True);
            };
            game.SnapshotChanged += replace;
            game.SendCommand(OriginalSessionCommandKind.SelectHero, "H024");
            yield return Wait(() => game.IsHost, "The callback replaces the client with a host.");
            Assert.That(game.State, Is.EqualTo(OriginalConnectionState.Lobby));
            Assert.That(game.View.players.Length, Is.EqualTo(1));
            Assert.That(game.LastReply, Is.Null);
            game.SendCommand(OriginalSessionCommandKind.SelectHero, "H008");
            Assert.That(game.LastReply.acknowledgedSequence, Is.EqualTo(1));
        }
    }
}
#endif

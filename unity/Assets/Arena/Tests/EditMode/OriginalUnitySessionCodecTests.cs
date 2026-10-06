using System;
using System.Linq;
using System.IO;
using System.Text;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalUnitySessionCodecTests
    {
        private readonly OriginalUnitySessionCodec codec = new OriginalUnitySessionCodec();

        [Test]
        public void CommandRoundTripPreservesSequenceAndRejectsMalformedOrUnboundedInput()
        {
            var command = new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero,
                sequence = 1234567890123, protocol = OriginalSession.Protocol, heroId = "N0A0", contentHash = new string('a', 64) };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out var read), Is.True);
            Assert.That(read.sequence, Is.EqualTo(command.sequence));
            Assert.That(read.heroId, Is.EqualTo("N0A0"));
            foreach (var bad in new[] { "", "{}", "null", "[]", "{", "{}{}", "{\"kind\":999,\"sequence\":1}",
                "{\"kind\":2,\"sequence\":0}", "{\"kind\":2,\"sequence\":1,\"heroId\":\"H008extra\"}",
                "{\"nested\":" + new string('[', 25) + "1" + new string(']', 25) + "}" })
                Assert.That(codec.TryDecodeCommand(Encoding.UTF8.GetBytes(bad), out _), Is.False, bad);
            Assert.That(codec.TryDecodeCommand(new byte[] { 123, 0xc0, 0xaf, 125 }, out _), Is.False);
            Assert.That(codec.TryDecodeCommand(Enumerable.Repeat((byte)32, 4097).ToArray(), out _), Is.False);
        }

        [Test]
        public void SnapshotRoundTripRetainsAuthoritativeOptionsAndRejectsBadProtocol()
        {
            var session = OriginalGameCatalogs.Load(OriginalGameCatalogsTests.Assets()).CreateSession(
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 11);
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                snapshot = session.Snapshot() };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var read), Is.True);
            Assert.That(read.snapshot.options.difficulty, Is.EqualTo(OriginalDifficulty.Standard));
            response.snapshot.protocol++;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            response.snapshot.protocol--;
            response.snapshot.time = double.NaN;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }

        private static OriginalSession Session() => OriginalGameCatalogs.Load(OriginalGameCatalogsTests.Assets())
            .CreateSession(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 11);

        [Test]
        public void LobbySnapshotsCannotSmuggleAnActiveOrPendingDuel()
        {
            var session = Session();
            foreach (var mutate in new Action<OriginalSessionView>[] {
                v => v.pendingDuel = true, v => v.duelId = -1, v => v.duelId = 1,
                v => v.duelKind = (OriginalDuelKind)99,
                v => v.hasDuel = true, v => v.duel = new OriginalDuelSnapshot { completedRound = 4 } })
            {
                var response = Snapshot(session); mutate(response.snapshot);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }

        [Test]
        public void BetCommandsRetainAmountAndRejectValuesBeyondOriginalBounds()
        {
            var command = new OriginalSessionCommand { kind = OriginalSessionCommandKind.Bet, sequence = 5,
                protocol = OriginalSession.Protocol, betSide = 2, betStake = 1000 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out var read), Is.True);
            Assert.That(read.betStake, Is.EqualTo(1000)); Assert.That(read.betSide, Is.EqualTo(2));
            command.betStake = 0;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.True, "Reducing a wager to zero is distinct from forfeiting it.");
            command.betStake = 1001;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            command.betStake = 100; command.betSide = -1;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
        }

        [Test]
        public void RealDuelSnapshotsRoundTripAndRejectBrokenParticipantOrBetArrays()
        {
            var session = Session();
            long hostSequence = 0, guestSequence = 0;
            void Command(long connection, OriginalSessionCommandKind kind, string hero = null)
            {
                long sequence = connection == 0 ? ++hostSequence : ++guestSequence;
                Assert.That(session.Apply(connection, new OriginalSessionCommand { kind = kind, sequence = sequence,
                    protocol = OriginalSession.Protocol, contentHash = session.Snapshot().contentHash, heroId = hero, ready = true }),
                    Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            Command(10, OriginalSessionCommandKind.Hello);
            Command(0, OriginalSessionCommandKind.SelectHero, "H008");
            Command(10, OriginalSessionCommandKind.SelectHero, "H024");
            Command(0, OriginalSessionCommandKind.LobbyReady); Command(10, OriginalSessionCommandKind.LobbyReady);
            Command(0, OriginalSessionCommandKind.Start);
            for (int tick = 0; tick < 1600 && !session.Snapshot().pendingDuel; tick++)
            {
                session.DrainEvents(); session.Advance(1);
                foreach (var enemy in session.Snapshot().enemies) session.ReportEnemyKilled(enemy.entityId);
            }
            Assert.That(session.Snapshot().pendingDuel, Is.True);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(session)), out _), Is.True);
            Assert.That(session.BeginDuel(new[] {
                new OriginalDuelParticipant { slot = 1, heroRawcode = "H008", rating = 12 },
                new OriginalDuelParticipant { slot = 2, heroRawcode = "H024", rating = 14 } }), Is.True);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(session)), out var read), Is.True);
            Assert.That(read.snapshot.duel.participants[1].heroRawcode, Is.EqualTo("H024"));
            foreach (var mutate in new Action<OriginalDuelSnapshot>[] {
                d => d.alive = new bool[2], d => d.participants[1].slot = 1,
                d => d.participants[0].heroRawcode = "N0A0", d => d.betStakes[1] = -1,
                d => d.betSides[2] = 3, d => d.ringRadius = double.NaN,
                d => d.completedRound = 5, d => d.phase = (OriginalDuelPhase)99 })
            {
                var response = Snapshot(session); mutate(response.snapshot.duel);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            }
        }

        private static OriginalNetworkResponse Snapshot(OriginalSession session, int slot = 1)
        {
            var view = session.Snapshot();
            return new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = slot,
                acknowledgedSequence = view.players.Single(x => x.slot == slot).acknowledgedSequence, snapshot = view };
        }

        sealed class WorldNavigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => Math.Abs(x) < 4096 && Math.Abs(y) < 4096;
            public bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius) => IsWalkable(to.x, to.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius) => new[] { to };
            public bool SetDoodadAlive(int editorId, bool alive) => true;
        }

        static OriginalSession WorldSession()
        {
            var session = Session();
            var native = UnityEngine.JsonUtility.FromJson<OriginalNativeCatalog>(File.ReadAllText(
                Path.Combine(UnityEngine.Application.dataPath, "Arena/Data/lia39-native126.json")));
            session.ConfigureWorld(new WorldNavigation(), native, new[] { new OriginalWorldDoodadView {
                editorId = 0, rawcode = "LTbr", position = new OriginalPoint(600, 1000), maxHealth = 10, health = 10 } });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" }),
                Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true }),
                Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }),
                Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return session;
        }

        [Test]
        public void WorldPresenceRoundTripsActualUnityNullAndSourceHeroState()
        {
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(Session())), out var lobby), Is.True);
            Assert.That(lobby.snapshot.hasWorld, Is.False); Assert.That(lobby.snapshot.world, Is.Null,
                "Unity materializes inline null objects; hasWorld must normalize the decoded absence.");
            var session = WorldSession();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(session)), out var game), Is.True);
            Assert.That(game.snapshot.hasWorld, Is.True);
            Assert.That(game.snapshot.world.units.Single().ownerSlot, Is.EqualTo(1));
            Assert.That(game.snapshot.world.units.Single().health, Is.EqualTo(631));
            Assert.That(game.snapshot.world.doodads.Single().editorId, Is.Zero, "Editor identity zero is valid.");
            var response = Snapshot(session); response.snapshot.hasWorld = false;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            response = Snapshot(Session()); response.snapshot.hasWorld = true;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }

        [Test]
        public void WorldSnapshotsRejectInvalidOwnersGeometryHealthAndDanglingOrders()
        {
            var session = WorldSession();
            var mutations = new Action<OriginalWorldSnapshot>[]
            {
                w => w.revision = -1,
                w => w.navigationRevision = -1,
                w => w.time = double.NaN,
                w => w.units = Array.Empty<OriginalWorldUnitView>(),
                w => w.units = new[] { w.units[0], w.units[0] },
                w => w.units[0].ownerSlot = 2,
                w => w.units[0].ownerSlot = 0,
                w => w.units[0].entityId = 2,
                w => w.units[0].rawcode = "H024",
                w => w.units[0].position.x = double.NaN,
                w => w.units[0].destination.y = double.PositiveInfinity,
                w => w.units[0].profile = null,
                w => w.units[0].profile.moveSpeed = double.NaN,
                w => w.units[0].profile.moveSpeed = 81921,
                w => w.units[0].profile.collisionRadius = 0,
                w => w.units[0].profile.maxHealth = double.PositiveInfinity,
                w => w.units[0].profile.maxMana = -1,
                w => w.units[0].health = 632,
                w => w.units[0].health = double.NaN,
                w => w.units[0].mana = 146,
                w => w.units[0].attackSequence = -1,
                w => w.units[0].order = (OriginalWorldOrder)99,
                w => w.units[0].targetKind = OriginalWorldTargetKind.Unit,
                w => w.units[0].approaching = true,
                w => { w.units[0].order = OriginalWorldOrder.AttackTarget; w.units[0].targetKind = OriginalWorldTargetKind.Unit; w.units[0].targetId = 1; },
                w => { w.units[0].order = OriginalWorldOrder.AttackTarget; w.units[0].targetKind = OriginalWorldTargetKind.Doodad; w.units[0].targetId = 999; },
                w => w.doodads = new[] { w.doodads[0], w.doodads[0] },
                w => w.doodads[0].position.x = double.PositiveInfinity,
                w => w.doodads[0].maxHealth = 0,
                w => w.doodads[0].health = 11
            };
            for (int i = 0; i < mutations.Length; i++)
            {
                var response = Snapshot(session); mutations[i](response.snapshot.world);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False, "world mutation " + i);
            }
        }

        [Test]
        public void MovementAndTargetCommandsRejectNonFiniteCoordinatesAndInvalidIdentities()
        {
            var move = new OriginalSessionCommand { kind = OriginalSessionCommandKind.Move, sequence = 4, x = 135, y = 1000 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(move), out var copy), Is.True);
            Assert.That(copy.y, Is.EqualTo(1000));
            foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 1048577.0 })
            {
                move.x = value;
                Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(move), out _), Is.False);
            }
            var attack = new OriginalSessionCommand { kind = OriginalSessionCommandKind.AttackTarget, sequence = 5,
                targetKind = OriginalWorldTargetKind.Doodad, targetId = 0 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(attack), out copy), Is.True);
            Assert.That(copy.targetId, Is.Zero);
            attack.targetKind = OriginalWorldTargetKind.Unit;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(attack), out _), Is.False);
            attack.targetId = 1;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(attack), out _), Is.True);
            attack.targetKind = (OriginalWorldTargetKind)99;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(attack), out _), Is.False);
        }

        [Test]
        public void IllusionsKeepTheirOwnIdentityAndCanOutliveTheirCanonicalHero()
        {
            var session = WorldSession();
            OriginalNetworkResponse WithIllusion()
            {
                var response = Snapshot(session); var hero = response.snapshot.world.units[0];
                var illusion = UnityEngine.JsonUtility.FromJson<OriginalWorldUnitView>(UnityEngine.JsonUtility.ToJson(hero));
                illusion.entityId = 1000000000; illusion.kind = OriginalWorldUnitKind.Illusion;
                illusion.sourceHeroEntityId = hero.entityId; illusion.copySourceEntityId = hero.entityId;
                illusion.imageFactory = OriginalImageFactory.KnightMirror; illusion.position.x += 100;
                illusion.hasFacing = true; illusion.facingDegrees = 180; illusion.castSequence = 1;
                response.snapshot.world.units = new[] { hero, illusion }; return response;
            }
            var valid = WithIllusion();
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(valid), out var decoded), Is.True);
            Assert.That(decoded.snapshot.world.units[1].sourceHeroEntityId, Is.EqualTo(1));
            valid.snapshot.world.units[0].health = 0; valid.snapshot.players[0].alive = false;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(valid), out _), Is.True);
            var mutations = new Action<OriginalWorldUnitView>[]
            {
                u => u.kind = OriginalWorldUnitKind.Hero, u => u.kind = (OriginalWorldUnitKind)99,
                u => u.ownerSlot = 0, u => u.sourceHeroEntityId = 2, u => u.rawcode = "H024",
                u => u.entityId = 1002, u => u.facingDegrees = double.NaN,
                u => u.facingDegrees = 360, u => u.hasFacing = false, u => u.castSequence = -1
            };
            for (int i = 0; i < mutations.Length; i++)
            {
                var response = WithIllusion(); mutations[i](response.snapshot.world.units[1]);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False, "illusion mutation " + i);
            }
        }

        [Test]
        public void CastAndOwnedActorCommandsPreserveIntentAndRejectMalformedTargets()
        {
            var command = new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.Move,
                actorEntityId = 1000000000, x = 123, y = 456 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out var copy), Is.True);
            Assert.That(copy.actorEntityId, Is.EqualTo(1000000000));
            command.actorEntityId = -1;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            command.actorEntityId = 0; command.kind = OriginalSessionCommandKind.CastSkill; command.skillId = "A05N";
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.True);
            command.x = double.NaN;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            command.x = 0; command.targetKind = OriginalWorldTargetKind.Unit;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.False);
            command.targetId = 1001;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(command), out _), Is.True);
        }

        [Test]
        public void SnapshotsRejectMissingDuplicateOrUnownedPlayerSlots()
        {
            var session = Session();
            var mutations = new Action<OriginalNetworkResponse>[]
            {
                x => x.assignedSlot = 0,
                x => x.assignedSlot = 2,
                x => x.snapshot.players = Array.Empty<OriginalSessionPlayerView>(),
                x => x.snapshot.players = new OriginalSessionPlayerView[] { null },
                x => x.snapshot.players = new[] { x.snapshot.players[0], x.snapshot.players[0] },
                x => x.snapshot.players[0].slot = 0,
                x => x.snapshot.players[0].connected = false,
                x => x.snapshot.players[0].acknowledgedSequence = -1,
                x => x.acknowledgedSequence = 1,
                x => x.snapshot.players[0].lobbyReady = true,
                x => x.snapshot.players[0].heroId = "BAD"
            };
            for (var i = 0; i < mutations.Length; i++)
            {
                var response = Snapshot(session);
                mutations[i](response);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False, "roster case " + i);
            }
        }

        [Test]
        public void SnapshotsRejectInvalidOptionsAndInconsistentMatchState()
        {
            var session = Session();
            var mutations = new Action<OriginalNetworkResponse>[]
            {
                x => x.snapshot.options.difficulty = (OriginalDifficulty)99,
                x => x.snapshot.options.heroSelection = (OriginalHeroSelection)99,
                x => x.snapshot.options.defensiveBarrels = (OriginalDefensiveBarrels)0,
                x => x.snapshot.phase = (OriginalMatchPhase)99,
                x => x.snapshot.revision = -1,
                x => x.snapshot.contentHash = new string('z', 64),
                x => x.snapshot.time = -1,
                x => x.snapshot.remainingSeconds = -1,
                x => x.snapshot.remainingEnemies = -1,
                x => x.snapshot.altars = -1,
                x => x.snapshot.started = true,
                x => x.snapshot.initialParticipants = 1,
                x => x.snapshot.players[0].matchSlot = 1,
                x => x.snapshot.players[0].gold = -1
            };
            for (var i = 0; i < mutations.Length; i++)
            {
                var response = Snapshot(session);
                mutations[i](response);
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False, "state case " + i);
            }
        }

        [Test]
        public void StartedSnapshotsPreserveReusedSlotsAndDisconnectedOtherParticipants()
        {
            var session = Session();
            void Accept(long connection, OriginalSessionCommandKind kind, long sequence, string hero = null) =>
                Assert.That(session.Apply(connection, new OriginalSessionCommand { protocol = OriginalSession.Protocol,
                    contentHash = session.Snapshot().contentHash, kind = kind, sequence = sequence, heroId = hero, ready = true }),
                    Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Accept(10, OriginalSessionCommandKind.Hello, 1);
            Accept(20, OriginalSessionCommandKind.Hello, 1);
            session.Disconnect(10);
            Accept(30, OriginalSessionCommandKind.Hello, 1);
            Accept(0, OriginalSessionCommandKind.SelectHero, 1, "H008");
            Accept(20, OriginalSessionCommandKind.SelectHero, 2, "H024");
            Accept(30, OriginalSessionCommandKind.SelectHero, 2, "N0A0");
            Accept(0, OriginalSessionCommandKind.LobbyReady, 2);
            Accept(20, OriginalSessionCommandKind.LobbyReady, 3);
            Accept(30, OriginalSessionCommandKind.LobbyReady, 3);
            Accept(0, OriginalSessionCommandKind.Start, 3);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(session, 2)), out _), Is.True);
            session.Advance(1); session.Advance(1);
            Accept(0, OriginalSessionCommandKind.WaveReady, 4);
            Accept(20, OriginalSessionCommandKind.WaveReady, 4);
            Accept(30, OriginalSessionCommandKind.WaveReady, 4);
            session.Advance(1); session.Advance(1);
            Assert.That(session.Snapshot().enemies, Is.Not.Empty);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(Snapshot(session, 2)), out _), Is.True);
            session.Disconnect(20);
            var response = Snapshot(session, 2);
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            Assert.That(decoded.snapshot.players.Single(x => x.slot == 3).connected, Is.False);
            response.snapshot.players[2].matchSlot = response.snapshot.players[0].matchSlot;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
            response = Snapshot(session, 2);
            response.snapshot.enemies[0].x = float.NaN;
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }
    }
}

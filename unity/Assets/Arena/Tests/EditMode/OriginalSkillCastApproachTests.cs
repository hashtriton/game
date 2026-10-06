using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSkillCastApproachTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalSession Create(string hero, out OriginalWorld world)
        {
            var native = Load<OriginalNativeCatalog>("native126");
            var observed = Load<OriginalObservedCatalog>("observed126");
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureProgression(native, observed);
            session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            session.ConfigureItems(native, Load<OriginalObservedItemCatalog>("observed-items126"), Load<OriginalItemPassiveCatalog>("item-passives"));
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero, heroId = hero });
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
            return session;
        }
        static object Player(OriginalSession session) => ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
        static OriginalSessionReplyCode Send(OriginalSession session, OriginalSessionCommand command)
        {
            command.sequence = session.Snapshot().players[0].acknowledgedSequence + 1;
            return session.Apply(0, command);
        }
        static object Call(OriginalSession session, string method, params object[] args) => typeof(OriginalSession).GetMethod(method, Private).Invoke(session, args);
        static OriginalSessionReplyCode Plan(OriginalSession session, OriginalSessionCommand command, out object target)
        {
            var method = typeof(OriginalSession).GetMethod("PlanSkillCastApproach", Private);
            Assert.That(method, Is.Not.Null, "The production skill approach preflight is absent.");
            object[] args = { Player(session), command, null };
            var before = JsonUtility.ToJson(session.Snapshot());
            var result = (OriginalSessionReplyCode)method.Invoke(session, args);
            target = args[2];
            Assert.That(JsonUtility.ToJson(session.Snapshot()), Is.EqualTo(before), "Preflight must not move, cast, debit, or change cooldowns.");
            return result;
        }
        static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        static int Summon(OriginalWorld world, string rawcode, int owner = 1, int offset = 0, double mana = 1000)
        {
            int id = OriginalWorld.FirstSummonEntityId + offset;
            var profile = new OriginalWorldUnitProfile { maxHealth = 1000, maxMana = 1000, moveSpeed = 250, collisionRadius = 16 };
            if (owner != 1 && world.UnitState(owner) == null) world.AddUnit(owner, owner, "H008", profile, new OriginalPoint(700, 1000));
            Assert.That(world.TryPublishSummons(new[] { new OriginalWorldSummonSpawn { entityId = id, ownerSlot = owner,
                sourceHeroEntityId = owner, rawcode = rawcode, profile = profile, health = 1000, mana = mana,
                position = new OriginalPoint(335 + offset * 60, 1000) } }), Is.True);
            return id;
        }
        static void Enemy(OriginalWorld world) => world.AddUnit(9001, 0, "hfoo", new OriginalWorldUnitProfile {
            maxHealth = 2000, maxMana = 500, moveSpeed = 250, collisionRadius = 16 }, new OriginalPoint(1335, 1000));

        [Test] public void ArcherFarPointPreflightUsesItsDeclaredRangeWithoutTakingManaOrStartingCast()
        {
            var session = Create("N0A0", out _);
            Assert.That(Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A15W" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A15W", x = 1335, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Field<int>(target, "actor"), Is.EqualTo(1));
            Assert.That(Field<int>(target, "unitTarget"), Is.EqualTo(0));
            Assert.That(Field<double>(target, "range"), Is.EqualTo(800));
            Assert.That(Field<OriginalPoint>(target, "point").x, Is.EqualTo(1335));
        }
        [Test] public void PyroFarVacuumRejectsUnitShapeAndUnknownAliasBeforeAnyApproach()
        {
            var session = Create("H024", out _);
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A0SJ" });
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A0SJ", x = 1335, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Field<double>(target, "range"), Is.EqualTo(900));
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A0SJ", targetKind = OriginalWorldTargetKind.Unit, targetId = 1 }, out target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(target, Is.Null);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A0SO", x = 1335, y = 1000 }, out target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
        }
        [Test] public void PyroSecondarySphereUsesNativeRankOneEvenWhenTheLearnedSkillHasRankThree()
        {
            var session = Create("H024", out _);
            var player = Player(session);
            ((OriginalHeroProgression)player.GetType().GetField("progression").GetValue(player)).SyncMatchExperience(10000);
            for (int i = 0; i < 3; i++) Assert.That(Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A0SP" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Call(session, "BeginPyroSpheres", 1);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A0SO", x = 1335, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Field<double>(target, "range"), Is.EqualTo(900), "A0SO rank2/3 ranges1252/1496 do not belong to the temporary native helper.");
        }
        [Test] public void NoTargetKnightOrderReturnsNoMovementDescriptorAndRejectsAUnitTarget()
        {
            var session = Create("H008", out _);
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" });
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N" }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(target, Is.Null);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N", targetKind = OriginalWorldTargetKind.Unit, targetId = 1 }, out target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
        }
        [Test] public void InsufficientManaAndMalformedPointDoNotCreateHeroMovementIntent()
        {
            var session = Create("N0A0", out var world);
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A15W" });
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A15W", x = double.NaN, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            var actor = world.UnitState(1); world.UpdateProfile(1, actor.profile, actor.health, 0);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A15W", x = 1335, y = 1000 }, out target), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(target, Is.Null);
        }
        [Test] public void SummonFarUnitPreflightRetainsLiveTargetIdentityAndItsOwnMana()
        {
            var session = Create("H008", out var world); int actor = Summon(world, "n02K"); Enemy(world);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, actorEntityId = actor,
                skillId = "A08U", targetKind = OriginalWorldTargetKind.Unit, targetId = 9001 }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Field<int>(target, "actor"), Is.EqualTo(actor));
            Assert.That(Field<int>(target, "unitTarget"), Is.EqualTo(9001));
            Assert.That(Field<double>(target, "range"), Is.EqualTo(600));
            Assert.That(world.UnitState(actor).mana, Is.EqualTo(1000));
            Assert.That(world.UnitState(1).mana, Is.EqualTo(145));
        }
        [Test] public void SummonFarPointNetUsesItsSourceRangeWithoutSpawningHelpers()
        {
            var session = Create("H008", out var world); int actor = Summon(world, "n0AC");
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, actorEntityId = actor,
                skillId = "A18J", x = 1335, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Field<double>(target, "range"), Is.EqualTo(700));
            Assert.That(Field<int>(target, "unitTarget"), Is.EqualTo(0));
        }
        [Test] public void SummonForeignActorWrongAllegianceAndDeadTargetNeverCreateApproach()
        {
            var session = Create("H008", out var world); int actor = Summon(world, "n02K"); Enemy(world);
            int foreign = Summon(world, "n02K", owner: 2, offset: 1);
            var command = new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, actorEntityId = foreign,
                skillId = "A08U", targetKind = OriginalWorldTargetKind.Unit, targetId = 9001 };
            Assert.That(Plan(session, command, out var target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            command.actorEntityId = actor; command.targetId = 1;
            Assert.That(Plan(session, command, out target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            command.targetId = 9001; world.ForceUnitDeath(9001);
            Assert.That(Plan(session, command, out target), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(target, Is.Null);
        }
        [Test] public void BlockedOrUnlearnedSkillIsNotAnOutOfRangeReason()
        {
            var session = Create("H024", out _);
            var command = new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill, skillId = "A0SJ", x = 1335, y = 1000 };
            Assert.That(Plan(session, command, out var target), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Send(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LearnSkill, skillId = "A0SJ" });
            Call(session, "AddNativeSilence", 1, 9001, 1d);
            Assert.That(Plan(session, command, out target), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(target, Is.Null);
        }
        [Test] public void PassiveSkillKeepsTheExistingNotReadyRefusalWithoutMovement()
        {
            var session = Create("H008", out _);
            Assert.That(Plan(session, new OriginalSessionCommand { kind = OriginalSessionCommandKind.CastSkill,
                skillId = "A001", x = 1335, y = 1000 }, out var target), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(target, Is.Null);
        }
    }
}

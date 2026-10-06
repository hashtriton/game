using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionAbilityTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        static OriginalSession Create()
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureProgression(Load<OriginalNativeCatalog>("lia39-native126"), Load<OriginalObservedCatalog>("lia39-observed126"));
            session.ConfigureWorld(new Navigation(), Load<OriginalNativeCatalog>("lia39-native126"), Array.Empty<OriginalWorldDoodadView>());
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.DrainEvents(); return session;
        }
        [Test] public void AbilityViewsKeepUnknownNativeCostsDistinctFromDeclaredCostsAndAreDetached()
        {
            var session = Create(); var views = session.Snapshot().players[0].abilities;
            Assert.That(views.Select(v => v.id), Is.EqualTo(new[] { "A05N", "A05M", "A102", "A0E6", "A001" }));
            var mirror = views[0]; Assert.That(mirror.manaCostKnown, Is.True); Assert.That(mirror.manaCost, Is.EqualTo(60));
            Assert.That(mirror.code, Is.EqualTo(OriginalAbilityUseCode.NotLearned));
            Assert.That(views[1].manaCostKnown, Is.True); Assert.That(views[1].manaCost, Is.Zero);
            Assert.That(views[4].manaCostKnown, Is.False);
            Assert.That(views[4].code, Is.EqualTo(OriginalAbilityUseCode.Passive));
            mirror.rank = 3; mirror.manaCost = 0;
            Assert.That(session.Snapshot().players[0].abilities[0].rank, Is.Zero);
            Assert.That(session.Snapshot().players[0].abilities[0].manaCost, Is.EqualTo(60));
        }
        [Test] public void MirrorWithInvalidTargetCannotSpendManaOrPublishACast()
        {
            var session = Create();
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var before = session.Snapshot().world.units[0];
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N", targetKind = OriginalWorldTargetKind.Unit, targetId = 1 }), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            var after = session.Snapshot().world.units[0];
            Assert.That(after.mana, Is.EqualTo(before.mana)); Assert.That(after.castSequence, Is.EqualTo(before.castSequence));
            Assert.That(session.Snapshot().world.units.Length, Is.EqualTo(1));
            Assert.That(session.Snapshot().players[0].abilities[0].implemented, Is.True);
        }
        [Test] public void CastingCannotSelectAnotherHeroesSkillOrRedirectToAnotherActor()
        {
            var session = Create();
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.CastSkill, skillId = "A15W" }), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N", actorEntityId = 2 }), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 6, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N" }), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(session.Snapshot().world.units[0].mana, Is.EqualTo(145));
        }
        static OriginalWorld World(OriginalSession session) => (OriginalWorld)typeof(OriginalSession)
            .GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);

        [Test] public void MeasuredShieldToggleChangesSpeedWithoutManaOrFakeSpellEventsAndCanBeTurnedOff()
        {
            var session = Create();
            void Advance(double seconds) { while (seconds > 1e-9) { double step = Math.Min(.05, seconds); session.Advance(step); session.DrainEvents(); seconds -= step; } }
            Advance(2.05);
            session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.WaveReady });
            Advance(3);
            foreach (var enemy in session.Snapshot().enemies.ToArray())
            {
                session.ReportEnemyKilled(enemy.entityId, true);
                if (session.Snapshot().players[0].progression.level >= 2) break;
            }
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var world = World(session); var hero = world.UnitState(1);
            Assert.That(world.UpdateProfile(1, hero.profile, hero.health, 0), Is.True);
            world.AddUnit(1999,0,"hfoo",hero.profile,new OriginalPoint(100,-1400));
            Assert.That(world.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1999),Is.True);
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 6, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(175).Within(1e-9));
            Assert.That(world.UnitState(1).mana, Is.Zero); Assert.That(world.UnitState(1).castSequence, Is.Zero);
            var view = session.Snapshot().players[0].abilities.Single(a => a.id == "A05M");
            Assert.That(view.toggledOn, Is.True); Assert.That(view.implemented, Is.True); Assert.That(view.code, Is.EqualTo(OriginalAbilityUseCode.Ready));
            world.SetUnitState(1, paused: true);
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 7, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(175).Within(1e-9));
            world.SetUnitState(1, paused: false);
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 8, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
            Assert.That(session.Snapshot().players[0].abilities.Single(a => a.id == "A05M").toggledOn, Is.False);
        }
        [Test] public void OwnedImageMovementUsesActorIdentityAfterTheCanonicalHeroDies()
        {
            var session = Create(); var world = World(session); var hero = world.UnitState(1);
            world.AddIllusion(1000000000, 1, hero.profile, new OriginalPoint(64, -1400), 300, 60);
            world.ForceUnitDeath(1);
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.Move,
                actorEntityId = 1000000000, x = 128, y = -1400 }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            session.Advance(.05);
            Assert.That(session.HaltReason, Is.Null);
            Assert.That(world.UnitState(1000000000).position.x, Is.GreaterThan(64));
            Assert.That(world.UnitState(1).health, Is.Zero);
            Assert.That(world.UnitState(1).position.x, Is.Zero);
        }
        [Test] public void ActorCommandsCannotControlAnEnemyOrAnotherPlayersHero()
        {
            var session = Create(); var world = World(session); var profile = world.UnitState(1).profile;
            world.AddUnit(2, 2, "H024", profile, new OriginalPoint(64, -1400));
            world.AddUnit(1001, 0, "n008", profile, new OriginalPoint(128, -1400));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.Move,
                actorEntityId = 2, x = 300, y = -1400 }), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.Stop,
                actorEntityId = 1001 }), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(world.UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(world.UnitState(2).order, Is.EqualTo(OriginalWorldOrder.None));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionDisconnectedAiTests
    {
        static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalSession Start(out OriginalWorld world, string remoteHero = "H008")
        {
            var s = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 27);
            var native = Load<OriginalNativeCatalog>("native126"); var observed = Load<OriginalObservedCatalog>("observed126");
            s.ConfigureProgression(native, observed); s.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            Assert.That(s.Apply(10, new OriginalSessionCommand { kind = OriginalSessionCommandKind.Hello, sequence = 1, protocol = OriginalSession.Protocol, contentHash = new string('a',64) }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            s.Apply(0, new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero, sequence = 1, heroId = remoteHero == "H024" ? "H008" : "H024" });
            s.Apply(10, new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero, sequence = 2, heroId = remoteHero });
            s.Apply(0, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LobbyReady, sequence = 2, ready = true });
            s.Apply(10, new OriginalSessionCommand { kind = OriginalSessionCommandKind.LobbyReady, sequence = 3, ready = true });
            Assert.That(s.Apply(0, new OriginalSessionCommand { kind = OriginalSessionCommandKind.Start, sequence = 3 }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", Hidden).GetValue(s);
            return s;
        }
        [Test] public void DeparturePreservesPartyAndProgressWhileRejectingOldConnection()
        {
            var s = Start(out var w); var before = s.Snapshot();
            Assert.That(s.Disconnect(10), Is.True); s.Advance(.5);
            Assert.That(s.HaltReason, Is.Null); Assert.That(s.Snapshot().time, Is.GreaterThan(before.time));
            Assert.That(s.Snapshot().initialParticipants, Is.EqualTo(2)); Assert.That(w.UnitState(2), Is.Not.Null);
            Assert.That(s.Snapshot().players.Single(x => x.slot == 2).connected, Is.False);
            Assert.That(s.Apply(10, new OriginalSessionCommand { kind = OriginalSessionCommandKind.Stop, sequence = 4 }), Is.EqualTo(OriginalSessionReplyCode.UnknownConnection));
            Assert.That(s.Disconnect(10), Is.False);
        }
        [Test] public void DepartureLearnsSourceLevelOneChoiceWithoutChangingHumanSkills()
        {
            var s = Start(out _); s.Disconnect(10);
            Assert.That(s.Snapshot().players.Single(x => x.slot == 2).learning.Single(x => x.id == "A05N").rank, Is.EqualTo(1));
            Assert.That(s.Snapshot().players.Single(x => x.slot == 1).learning.All(x => x.rank == 0), Is.True);
        }
        [Test] public void AiQuarterSecondRosterScanCastsMirrorAndContinuesWeaponCombat()
        {
            var s = Start(out var w); s.Disconnect(10);
            var match = (OriginalMatch)typeof(OriginalSession).GetField("match", Hidden).GetValue(s);
            for (int i = 0; i < 40; i++) { s.Advance(.05); s.DrainEvents(); }
            match.SetReady(1); match.SetReady(2); s.Advance(.05); s.DrainEvents();
            var hero = w.UnitState(2); w.AddUnit(6001, 0, "n008", new OriginalWorldUnitProfile { maxHealth = 5000, maxMana = 0, moveSpeed = 0, collisionRadius = 16 }, new OriginalPoint(hero.position.x + 100, hero.position.y));
            for (int i = 0; i < 60; i++) { s.Advance(.05); s.DrainEvents(); }
            Assert.That(s.HaltReason, Is.Null);
            Assert.That(w.Snapshot().units.Any(x => x.kind == OriginalWorldUnitKind.Illusion && x.ownerSlot == 2), Is.True);
            Assert.That(w.UnitState(2).attackSequence, Is.GreaterThan(0));
        }
        [Test] public void PrimaryVacuumOrderCannotBecomeTheSecondaryDetonation()
        {
            var s = Start(out var w, "H024"); s.Disconnect(10); w.SetUnitState(2, paused: false);
            typeof(OriginalSession).GetMethod("BeginPyroVacuum", Hidden).Invoke(s, new object[] { 2, 1, new OriginalPoint(900, 1000) });
            var players = (System.Collections.IList)typeof(OriginalSession).GetField("players", Hidden).GetValue(s);
            var before = w.UnitState(2).castSequence;
            var method = typeof(OriginalSession).GetMethod("DisconnectedAiCast", Hidden);
            Assert.That(method.Invoke(s, new object[] { players[1], "A0SJ", new OriginalPoint(850,1000), new[] { "A0SJ" } }), Is.False);
            Assert.That(w.UnitState(2).castSequence, Is.EqualTo(before));
            Assert.That(method.Invoke(s, new object[] { players[1], "A0SJ", new OriginalPoint(750,1000), new[] { "A0SN" } }), Is.True);
            Assert.That(w.UnitState(2).castSequence, Is.GreaterThan(before));
        }
        static OriginalSession StartRefreshRepro(out OriginalWorld world,out int enemy)
        {
            var s=Start(out world);var match=(OriginalMatch)typeof(OriginalSession).GetField("match",Hidden).GetValue(s);
            for(int i=0;i<40;i++){s.Advance(.05);s.DrainEvents();}
            match.SetReady(1);match.SetReady(2);
            for(int i=0;i<100&&!match.Enemies.Any(x=>x.rawcode=="n008"&&x.attackGroup==0);i++)
            {s.Advance(.05);s.DrainEvents();}
            enemy=OriginalWorld.EnemyEntityId(match.Enemies.First(x=>x.rawcode=="n008"&&x.attackGroup==0).entityId);
            foreach(var unit in world.Snapshot().units)world.SetUnitState(unit.entityId,paused:true);
            world.ForcePosition(1,new OriginalPoint(0,1000));world.ForcePosition(2,new OriginalPoint(2500,1000));
            world.ForcePosition(enemy,new OriginalPoint(140,1000));world.SetUnitState(enemy,paused:false);
            return s;
        }
        [Test] public void SourceAttackPointRefreshDoesNotWalkEngagedCreepTowardHeroCenter()
        {
            var s=StartRefreshRepro(out var w,out int enemy);
            Assert.That(w.TryAttackTarget(enemy,OriginalWorldTargetKind.Unit,1),Is.True);
            typeof(OriginalSession).GetMethod("AdvanceWeapons",Hidden).Invoke(s,null);
            var before=w.UnitState(enemy);
            typeof(OriginalSession).GetMethod("RefreshWorldOrders",Hidden).Invoke(s,null);
            Assert.That(w.UnitState(enemy).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));
            Assert.That(w.UnitState(enemy).targetId,Is.EqualTo(1));
            for(int i=0;i<4;i++)w.Advance(.05);
            Assert.That(w.UnitState(enemy).position.x,Is.EqualTo(before.position.x).Within(1e-8));
            // The source FA repeats at3.5s. An unchanged refresh cannot ratchet
            // a stationary engaged attacker farther into the same queue.
            for(int refresh=0;refresh<3;refresh++)
            {
                for(int i=0;i<70;i++)w.Advance(.05);
                typeof(OriginalSession).GetMethod("RefreshWorldOrders",Hidden).Invoke(s,null);
                Assert.That(w.UnitState(enemy).position.x,Is.EqualTo(before.position.x).Within(1e-8));
            }
        }
        [Test] public void SourceAttackPointRefreshAcquiresNearbyBodyBeforeFirstMovementTick()
        {
            var s=StartRefreshRepro(out var w,out int enemy);
            w.TryMove(enemy,w.UnitState(1).position);
            typeof(OriginalSession).GetMethod("RefreshWorldOrders",Hidden).Invoke(s,null);
            Assert.That(w.UnitState(enemy).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));
            w.Advance(.05);
            Assert.That(w.UnitState(enemy).position.x,Is.EqualTo(140).Within(1e-8));
        }
    }
}

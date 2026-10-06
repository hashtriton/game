using System;
using System.IO;
using System.Linq;
using WorldSupport = Arena.Tests.OriginalWorldOptionTestSupport;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalAcolyteShopTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalSession Create()
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureItems(Load<OriginalNativeCatalog>("native126"), Load<OriginalObservedItemCatalog>("observed-items126"));
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return session;
        }
        static OriginalShopView Acolyte(OriginalSession session) => session.Snapshot().shops.SingleOrDefault(s => s.unitId == "u00E");
        static double ReadyAge
        {
            get
            {
                var stock = Load<OriginalObservedItemCatalog>("observed-items126").readyStock;
                stock.BuildIndexes(Load<OriginalItemCatalog>("items"));
                return new[] { "I07W", "I0AI", "I05F", "I0AT", "I0B8" }.Max(id => stock.ReadyAgeSeconds("u00E", id));
            }
        }
        static void Advance(OriginalSession session, double seconds)
        {
            while (seconds > 1e-9) { double dt = Math.Min(.05, seconds); session.Advance(dt); seconds -= dt; }
            Assert.That(session.HaltReason, Is.Null);
        }
        static void Ready(OriginalSession session) => Assert.That(session.Apply(0, new OriginalSessionCommand {
            sequence = session.Snapshot().players[0].acknowledgedSequence + 1, kind = OriginalSessionCommandKind.WaveReady }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
        static void FinishWave(OriginalSession session)
        {
            foreach (var enemy in session.Snapshot().enemies) Assert.That(session.ReportEnemyKilled(enemy.entityId), Is.True);
            Advance(session, 3.01);
        }

        [Test] public void AcolyteAppearsAfterFirstCompletedWaveThenFreshStockWaitsForMeasuredReadyAge()
        {
            var session = Create(); Assert.That(Acolyte(session), Is.Null);
            Advance(session, 2.01); Assert.That(Acolyte(session), Is.Null); Ready(session);
            Advance(session, 2.01); Assert.That(Acolyte(session), Is.Null); FinishWave(session);
            Assert.That(session.Snapshot().round, Is.EqualTo(2)); Assert.That(Acolyte(session), Is.Null);
            Advance(session, .95); Assert.That(Acolyte(session), Is.Null);
            Advance(session, .1); var early = Acolyte(session);
            Assert.That(early, Is.Not.Null); Assert.That(early.open, Is.True);
            Assert.That(early.position.x, Is.EqualTo(-416)); Assert.That(early.position.y, Is.EqualTo(608));
            Assert.That(early.stock.All(s => !s.known), Is.True);
            Assert.That(ReadyAge, Is.LessThan(2.201));
            Advance(session, ReadyAge + .1); var ready = Acolyte(session);
            Assert.That(ready.stock.All(s => s.known && s.available > 0), Is.True);
            Ready(session); Assert.That(Acolyte(session), Is.Null);
        }
        [Test] public void RecreatedAcolyteDoesNotReusePriorReadyStockAtSpawnTime()
        {
            var session = Create(); Advance(session, 2.01); Ready(session); Advance(session, 2.01); FinishWave(session);
            Advance(session, 1 + ReadyAge + .1); Assert.That(Acolyte(session).stock.All(s => s.known), Is.True);
            Ready(session); Advance(session, 2.01); FinishWave(session);
            Advance(session, 1.1); Assert.That(Acolyte(session).stock.All(s => !s.known), Is.True);
            Advance(session, ReadyAge + .1); Assert.That(Acolyte(session).stock.All(s => s.known), Is.True);
        }
        [Test] public void AcolyteShopWaitsForTheWholeDuelSeriesWhileOrdinaryShopsStayOpen()
        {
            var session=WorldSupport.Create(count:2);
            var match=(OriginalMatch)typeof(OriginalSession).GetField("match",WorldSupport.Hidden).GetValue(session);
            match.DrainEvents();typeof(OriginalMatch).GetProperty("Round").SetValue(match,4);
            typeof(OriginalMatch).GetProperty("Phase").SetValue(match,OriginalMatchPhase.Combat);
            void OpenPreparation()
            {
                foreach(var item in match.DrainEvents().Where(e=>e.kind==OriginalMatchEventKind.ShopAccess))
                    WorldSupport.Call(session,"ApplyShopAccessEvent",item);
                WorldSupport.AdvanceWorldOnly(session,1.05,"RefreshAcolyteStock");
            }
            typeof(OriginalMatch).GetMethod("PrepareNextRound",WorldSupport.Hidden).Invoke(match,null);OpenPreparation();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(Acolyte(session),Is.Null,"D4 returns before OU before pair duels");
            Assert.That(session.Snapshot().shops.All(s=>s.open),Is.True,"Ordinary central shops stay open during preparation");
            match.Advance(25);match.CompleteDuelSequence();OpenPreparation();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.DuelPreparation));
            Assert.That(Acolyte(session),Is.Null,"The gladiator preparation also precedes OU");
            Assert.That(session.Snapshot().shops.All(s=>s.open),Is.True);
            match.Advance(25);match.CompleteDuelSequence();OpenPreparation();
            Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Preparation));Assert.That(match.Round,Is.EqualTo(5));
            var fresh=Acolyte(session);Assert.That(fresh,Is.Not.Null);Assert.That(fresh.stock.All(s=>!s.known),Is.True);
            WorldSupport.AdvanceWorldOnly(session,ReadyAge+.1,"RefreshAcolyteStock");
            Assert.That(Acolyte(session).stock.All(s=>s.known&&s.available>0),Is.True);
        }
    }
}



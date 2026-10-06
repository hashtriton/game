using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalNativeBountyTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalObservedBountyCatalog Measured() => Load<OriginalObservedCatalog>("observed126").bounty;

        [Test] public void RealCaptureMatchesFirstWaveAndOnlyNamedBlankRowsResolveToZero()
        {
            var rules = new OriginalNativeBountyRules(Load<OriginalCombatCatalog>("combat"), Measured());
            Assert.That(rules.Resolve("n008").amount, Is.EqualTo(4));
            Assert.That(rules.Resolve("n009").amount, Is.EqualTo(32));
            Assert.That(rules.Resolve("n05J").amount, Is.EqualTo(16));
            foreach (string id in new[] { "O006", "n00K", "n00Z", "n017", "n0AW", "u00G", "u00L" })
                Assert.That(rules.Resolve(id).amount, Is.Zero);
            Assert.That(rules.Resolve("n008").distributionKnown, Is.False);
            Assert.That(rules.Resolve("n067").amount, Is.EqualTo(90)); // dice1 exists, sides remains unknown.
            Assert.Throws<InvalidOperationException>(() => rules.Resolve("n068"));
            Assert.Throws<InvalidOperationException>(() => rules.Resolve("hfoo"));
        }
        [Test] public void MalformedSamplesControlsAndSourceConflictsFailClosed()
        {
            var measured = Measured(); measured.controls[1].gold = 1;
            Assert.Throws<ArgumentException>(() => new OriginalNativeBountyRules(Load<OriginalCombatCatalog>("combat"), measured));
            measured = Measured(); Array.Find(measured.units, u => u.id == "n008").samples[2].gold = 5;
            Assert.Throws<ArgumentException>(() => new OriginalNativeBountyRules(Load<OriginalCombatCatalog>("combat"), measured));
            measured = Measured(); Array.Find(measured.units, u => u.id == "n068").samples[0].gold = 1;
            Assert.Throws<ArgumentException>(() => new OriginalNativeBountyRules(Load<OriginalCombatCatalog>("combat"), measured));
            var combat = Load<OriginalCombatCatalog>("combat"); combat.BuildIndexes();
            Array.Find(combat.Unit("n008").fields, f => f.key == "bountyplus").conflict = true;
            Assert.Throws<ArgumentException>(() => new OriginalNativeBountyRules(combat, Measured()));
        }
        [Test] public void RulesFreezeParsedDtoAndCompleteDeclaredDiceUseHostDraws()
        {
            var measured = Measured(); var combat = Load<OriginalCombatCatalog>("combat"); combat.BuildIndexes();
            var rules = new OriginalNativeBountyRules(combat, measured);
            Array.Find(measured.units, u => u.id == "n008").observedMin = 100;
            Array.Find(combat.Unit("n008").fields, f => f.key == "bountyplus").number = 100;
            Assert.That(rules.Resolve("n008").amount, Is.EqualTo(4));
            // Synthetic complete declaration tests model mechanics only; the
            // real sparse wave catalog does not establish these dice defaults.
            combat = Load<OriginalCombatCatalog>("combat"); combat.BuildIndexes();
            var definition = combat.Unit("n008"); var fields = new System.Collections.Generic.List<OriginalCombatField>(definition.fields);
            fields.Add(new OriginalCombatField { key = "bountydice", isNumber = true, number = 2 });
            fields.Add(new OriginalCombatField { key = "bountysides", isNumber = true, number = 3 });
            definition.fields = fields.ToArray(); rules = new OriginalNativeBountyRules(combat, Measured());
            int calls = 0; var reward = rules.Resolve("n008", sides => { Assert.That(sides, Is.EqualTo(3)); return ++calls; });
            Assert.That(reward.amount, Is.EqualTo(7)); Assert.That(calls, Is.EqualTo(2));
            Assert.That(reward.distributionKnown, Is.True);
            Assert.Throws<InvalidOperationException>(() => rules.Resolve("n008", sides => 0));
        }

        static OriginalSession Session(bool equalGold = false)
        {
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard); options.equalGold = equalGold;
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64), options, 123);
            session.ConfigureBounty(Measured());
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return session;
        }
        static int Award(OriginalSession session, int owner, int id, string rawcode = "n008")
        {
            try { return (int)typeof(OriginalSession).GetMethod("ApplyNativeBounty", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(session, new object[] { owner, new OriginalMatchEnemy { entityId = id, rawcode = rawcode } }); }
            catch (TargetInvocationException error) { throw error.InnerException; }
        }
        static OriginalInventory Inventory(OriginalSession session)
        {
            var players = (IList)typeof(OriginalSession).GetField("players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            return (OriginalInventory)players[0].GetType().GetField("inventory").GetValue(players[0]);
        }
        [Test] public void SessionCreditsOwnerExactlyOnceWithoutNativeXpAndHonorsSharedGold()
        {
            var session = Session(); var before = session.Snapshot().players[0];
            Assert.That(Award(session, 0, 1), Is.Zero);
            Assert.That(Award(session, 1, 1), Is.EqualTo(4));
            Assert.That(Award(session, 1, 1), Is.Zero);
            var after = session.Snapshot().players[0];
            Assert.That(after.gold, Is.EqualTo(before.gold + 4)); Assert.That(after.experience, Is.EqualTo(before.experience));
            var shared = Session(true); long gold = shared.Snapshot().players[0].gold;
            Assert.That(Award(shared, 1, 1), Is.Zero); Assert.That(shared.Snapshot().players[0].gold, Is.EqualTo(gold));
        }
        [Test] public void UnresolvedBountyAndOverflowDoNotPublishBalanceOrConsumeDeathIdentity()
        {
            var session = Session(); long before = Inventory(session).Gold;
            Assert.Throws<InvalidOperationException>(() => Award(session, 1, 77, "n068"));
            Assert.That(Inventory(session).Gold, Is.EqualTo(before));
            Inventory(session).GrantResources(long.MaxValue - before, 0);
            Assert.Throws<OverflowException>(() => Award(session, 1, 77));
            Assert.That(Inventory(session).Gold, Is.EqualTo(long.MaxValue));
            Assert.That(Inventory(session).TrySpendResources(10, 0), Is.True);
            Assert.That(Award(session, 1, 77), Is.EqualTo(4));
            Assert.That(Inventory(session).Gold, Is.EqualTo(long.MaxValue - 6));
            Assert.Throws<InvalidOperationException>(() => session.ConfigureBounty(Measured()));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionItemTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => Math.Abs(x) < 5000 && Math.Abs(y) < 5000;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalSession Create(bool testStockKnown = true)
        {
            var native = Load<OriginalNativeCatalog>("native126");
            // Synthetic fixture inputs test transactions; these are explicitly
            // not evidence for absent native costs, charges or starting stock.
            foreach (var item in native.items)
                foreach (var field in item.fields)
                    if (field.field == "lumbercost" || field.field == "uses" || testStockKnown && field.field == "stockStart")
                    { field.known = true; field.kind = "number"; field.state = "map-declaration"; field.number = 0; field.sources = new[] { new OriginalNativeEvidence { source = 0, line = 1 } }; }
            native.BuildIndexes();
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>());
            session.ConfigureItems(native);
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return session;
        }
        static OriginalSessionReplyCode Send(OriginalSession s, OriginalSessionCommandKind kind, int slot = 0, long item = 0, int shop = 2, string offer = "I04J") =>
            s.Apply(0, new OriginalSessionCommand { sequence = s.Snapshot().players[0].acknowledgedSequence + 1, kind = kind,
                itemId = offer, shopInstanceId = shop, itemSlot = slot, itemInstanceId = item });

        [Test] public void OrdinaryBuyConvertsOfferDebitsOnceAndRespectsSharedStock()
        {
            var s = Create();
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var player = s.Snapshot().players[0];
            Assert.That(player.gold, Is.EqualTo(65));
            Assert.That(player.inventory.heroSlots[0].itemId, Is.EqualTo("I000"));
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(s.Snapshot().players[0].gold, Is.EqualTo(65));
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.BuyItem, itemId = "I04J", shopInstanceId = 2 }), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            s.Advance(1); s.Advance(.1);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().players[0].gold, Is.Zero);
        }

        [Test] public void UnknownStartingStockAndWrongOffersDoNotSpendOrGrant()
        {
            var s = Create(false);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, offer: "I0AT"), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(s.Snapshot().players[0].gold, Is.EqualTo(130));
            Assert.That(s.Snapshot().players[0].inventory.heroSlots.All(x => x == null), Is.True);
        }

        [Test] public void DropPickupAndTransferKeepExactlyOneResidencyAndSnapshotsAreDetached()
        {
            var s = Create();
            Send(s, OriginalSessionCommandKind.BuyItem);
            long id = s.Snapshot().players[0].inventory.heroSlots[0].instanceId;
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().players[0].inventory.heroSlots[0], Is.Null);
            Assert.That(s.Snapshot().groundItems.Single().item.itemId, Is.EqualTo("I04J"));
            var leaked = s.Snapshot(); leaked.groundItems[0].item.removed = true;
            Assert.That(Send(s, OriginalSessionCommandKind.PickupItem, item: id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().groundItems, Is.Empty);
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var bags = s.Snapshot().players[0].inventory;
            Assert.That(bags.heroSlots[0], Is.Null);
            Assert.That(bags.servantSlots[0].instanceId, Is.EqualTo(id));
            bags.servantSlots[0].charges = 987;
            Assert.That(s.Snapshot().players[0].inventory.servantSlots[0].charges, Is.Zero);
            Assert.That(Send(s, OriginalSessionCommandKind.PickupItem, item: id), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
        }

        [Test] public void TwelveFullSlotsLeavePaidThirteenthItemOnGround()
        {
            var s = Create();
            var players = (System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
            var player = players[0];
            var inventory = (OriginalInventory)player.GetType().GetField("inventory", BindingFlags.Instance | BindingFlags.Public).GetValue(player);
            for (int i = 0; i < 12; i++) inventory.TryPickup(inventory.CreateInstance("I000"));
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var v = s.Snapshot();
            Assert.That(v.players[0].gold, Is.EqualTo(65));
            Assert.That(v.groundItems.Length, Is.EqualTo(1));
            Assert.That(v.groundItems[0].item.removed, Is.False);
            var all = v.players[0].inventory.heroSlots.Concat(v.players[0].inventory.servantSlots).Select(i => i.instanceId).Append(v.groundItems[0].item.instanceId);
            Assert.That(all.Distinct().Count(), Is.EqualTo(13));
        }

        [Test] public void NetworkRoundTripKeepsSixEmptySlotsAndRejectsDuplicateItemResidency()
        {
            var s = Create(); Send(s, OriginalSessionCommandKind.BuyItem);
            var codec = new OriginalUnitySessionCodec();
            var snapshot = s.Snapshot();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, code = OriginalSessionReplyCode.Accepted,
                assignedSlot = 1, acknowledgedSequence = snapshot.players[0].acknowledgedSequence, snapshot = snapshot };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            Assert.That(decoded.snapshot.players[0].inventory.servantSlots.Length, Is.EqualTo(6));
            Assert.That(decoded.snapshot.players[0].inventory.servantSlots.All(i => i == null), Is.True);
            snapshot.groundItems = new[] { new OriginalGroundItemView { item = snapshot.players[0].inventory.heroSlots[0], position = new OriginalPoint(135, 1000) } };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out _), Is.False);
        }
    }
}

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionPawnTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        static OriginalInventory Inventory(OriginalSession session)
        {
            var player = ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(session))[0];
            return (OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
        }
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(s);
        static OriginalSessionReplyCode Send(OriginalSession s, OriginalSessionCommand c)
        { c.sequence = s.Snapshot().players[0].acknowledgedSequence + 1; return s.Apply(0, c); }

        [Test] public void PawnMatchesSixNativeGoldAndChargeCases()
        {
            string[] ids = { "I000", "I003", "I03L", "I03L", "I03L", "I0AE" };
            int[] charges = { 0, 0, 1, 3, 0, 2 }, credits = { 32, 50, 3, 9, 0, 1340 };
            for (int i = 0; i < ids.Length; i++)
            {
                var s = Create(); var inventory = Inventory(s);
                var item = inventory.CreateInstance(ids[i]); item.charges = charges[i];
                Assert.That(inventory.TryPickup(item).Applied, Is.True);
                long gold = inventory.Gold;
                Assert.That(inventory.TrySell(OriginalInventoryBag.Hero, 0).Applied, Is.True, ids[i]);
                Assert.That(inventory.Gold - gold, Is.EqualTo(credits[i]), ids[i]);
                Assert.That(inventory.HeroSlots[0], Is.Null);
                Assert.That(inventory.TrySell(OriginalInventoryBag.Hero, 0).Code, Is.EqualTo(OriginalItemActionCode.InvalidSlot));
            }
        }

        [Test] public void SaleRequiresNearbyShopAndExactIdentityThenRebuildsEquipment()
        {
            var s = Create(); var w = World(s); double originalMax = w.UnitState(1).profile.maxHealth;
            var shop = s.Snapshot().shops.First(x => x.stock.Any(y => y.itemId == "I04P"));
            Assert.That(Send(s, new OriginalSessionCommand { kind = OriginalSessionCommandKind.BuyItem, itemId = "I04P", shopInstanceId = shop.instanceId }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var inventory = s.Snapshot().players[0].inventory; var item = inventory.heroSlots[0];
            Assert.That(w.UnitState(1).profile.maxHealth, Is.GreaterThan(originalMax));
            var command = new OriginalSessionCommand { kind = OriginalSessionCommandKind.SellItem, shopInstanceId = shop.instanceId, itemSlot = 0, itemInstanceId = item.instanceId };
            w.Relocate(1, new OriginalPoint(shop.position.x + 451, shop.position.y));
            Assert.That(Send(s, command), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(Inventory(s).Gold, Is.EqualTo(inventory.gold));
            Assert.That(w.UnitState(1).profile.maxHealth, Is.GreaterThan(originalMax));
            w.Relocate(1, shop.position); command.itemInstanceId++;
            Assert.That(Send(s, command), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            command.itemInstanceId--;
            Assert.That(Send(s, command), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).profile.maxHealth, Is.EqualTo(originalMax));
            Assert.That(Inventory(s).Gold, Is.GreaterThan(inventory.gold));
            long credited = Inventory(s).Gold;
            Assert.That(s.Apply(0, command), Is.EqualTo(OriginalSessionReplyCode.InvalidSequence));
            Assert.That(Inventory(s).Gold, Is.EqualTo(credited));
        }

        [Test] public void SaleCodecRejectsUnknownBagMissingIdentityAndShop()
        {
            var codec = new OriginalUnitySessionCodec();
            var c = new OriginalSessionCommand { kind = OriginalSessionCommandKind.SellItem, sequence = 1, shopInstanceId = 1, itemInstanceId = 1 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(c), out _), Is.True);
            c.bag = (OriginalInventoryBag)2;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(c), out _), Is.False);
            c.bag = OriginalInventoryBag.Hero; c.itemInstanceId = 0;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(c), out _), Is.False);
            c.itemInstanceId = 1; c.shopInstanceId = 0;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(c), out _), Is.False);
        }
        [Test] public void SoulPriceFloorsTheWholeStackAndIgnoresZeroDefaultCharges()
        {
            string[] ids={"I01D","I01D","I01D","I021","I0AE","I003"};
            int[] charges={1,3,0,1,8,3},gold={0,0,0,0,1340,50},souls={4,13,0,1,0,0};
            for(int i=0;i<ids.Length;i++)
            {
                var inventory=Inventory(Create());var item=inventory.CreateInstance(ids[i]);item.charges=charges[i];
                Assert.That(inventory.TryPickup(item).Applied,Is.True);
                long previousGold=inventory.Gold,previousSouls=inventory.Lumber;
                Assert.That(inventory.TrySell(OriginalInventoryBag.Hero,0).Applied,Is.True);
                Assert.That(inventory.Gold-previousGold,Is.EqualTo(gold[i]));
                Assert.That(inventory.Lumber-previousSouls,Is.EqualTo(souls[i]));
            }
        }
        [Test] public void CreditOverflowCannotConsumeAnItemOrPartiallyChangeBalances()
        {
            foreach(string id in new[]{"I000","I01D"})
            {
                var inventory=Inventory(Create());var item=inventory.CreateInstance(id);
                Assert.That(inventory.TryPickup(item).Applied,Is.True);
                inventory.GrantResources(long.MaxValue-inventory.Gold,long.MaxValue-inventory.Lumber);
                Assert.That(inventory.TrySell(OriginalInventoryBag.Hero,0).Code,Is.EqualTo(OriginalItemActionCode.NotAllowed));
                Assert.That(inventory.Gold,Is.EqualTo(long.MaxValue));Assert.That(inventory.Lumber,Is.EqualTo(long.MaxValue));
                Assert.That(inventory.HeroSlots[0].instanceId,Is.EqualTo(item.instanceId));
            }
        }
    }
}

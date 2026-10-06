using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalGameCatalogsTests
    {
        internal static TextAsset[] Assets() => new[] { "layout", "match", "combat", "items", "item-passives", "duels", "native126", "observed126", "observed-items126" }
            .Select(name => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-" + name + ".json")).ToArray();

        [Test]
        public void PlayerDataLoadsFromReferencedAssetsAndHashesActualBytes()
        {
            var assets = Assets();
            var data = OriginalGameCatalogs.Load(assets);
            Assert.That(data.Combat.selectedHeroes.Select(x => x.id), Is.EqualTo(new[] { "H008", "N0A0", "H024" }));
            Assert.That(data.Fingerprint, Has.Length.EqualTo(64));
            Assert.That(data.ObservedItems.Field("I000", "goldcost").value, Is.EqualTo(65));
            Assert.That(data.ObservedItems.Field("I000", "lumbercost").known, Is.True);
            Assert.That(OriginalGameCatalogs.Load(assets.Reverse()).Fingerprint, Is.EqualTo(data.Fingerprint));
            var changed = new TextAsset(assets[0].text + "\n") { name = assets[0].name };
            try
            {
                Assert.That(OriginalGameCatalogs.Load(new[] { changed }.Concat(assets.Skip(1))).Fingerprint,
                    Is.Not.EqualTo(data.Fingerprint), "Text assets must not be parsed and reserialized before hashing.");
            }
            finally { UnityEngine.Object.DestroyImmediate(changed); }
        }

        [Test]
        public void MissingDuplicateOrDifferentMapCannotEnterLobby()
        {
            var assets = Assets();
            Assert.Throws<ArgumentException>(() => OriginalGameCatalogs.Load(assets.Skip(1)));
            Assert.Throws<ArgumentException>(() => OriginalGameCatalogs.Load(assets.Take(assets.Length - 1)));
            Assert.Throws<ArgumentException>(() => OriginalGameCatalogs.Load(assets.Concat(new[] { assets[0] })));
            var changed = new TextAsset(assets[0].text.Replace(OriginalGameCatalogs.MapSha256, new string('0', 64)))
                { name = assets[0].name };
            try { Assert.Throws<ArgumentException>(() => OriginalGameCatalogs.Load(new[] { changed }.Concat(assets.Skip(1)))); }
            finally { UnityEngine.Object.DestroyImmediate(changed); }
        }

        [Test]
        public void ValidLayoutHeaderWithoutPlayableGridsCannotEnterLobby()
        {
            var assets = Assets();
            var layout = new TextAsset("{\"schemaVersion\":1,\"version\":\"3.9c\",\"wcUnitsPerUnityUnit\":64," +
                "\"provenance\":{\"mapSha256\":\"" + OriginalGameCatalogs.MapSha256 + "\"}}") { name = assets[0].name };
            try { Assert.Throws<InvalidOperationException>(() => OriginalGameCatalogs.Load(new[] { layout }.Concat(assets.Skip(1)))); }
            finally { UnityEngine.Object.DestroyImmediate(layout); }
        }

        [Test]
        public void ReadyShopsPermitARealPurchaseWithoutInventingNativeStockStart()
        {
            var assets = Assets(); var data = OriginalGameCatalogs.Load(assets);
            var mapObject = new GameObject("Source shop purchase");
            try
            {
                var map = mapObject.AddComponent<ArenaMap>();
                map.layoutJson = assets.First(a => a.name == "lia39-layout");
                var session = data.CreateSession(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123,
                    new OriginalMapNavigation(map));
                Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                var before = session.Snapshot();
                var shop = before.shops.First(s => s.open && s.stock.Any(i => i.itemId == "I04J"));
                var stock = shop.stock.First(i => i.itemId == "I04J");
                Assert.That(stock.known && stock.available > 0, Is.True);
                var rules = new OriginalItemRules(data.Items, data.Native, data.ObservedItems);
                Assert.That(rules.Field("I04J", "stockStart").known, Is.False);
                Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.BuyItem,
                    shopInstanceId = shop.instanceId, itemId = "I04J", bag = OriginalInventoryBag.Hero }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                var after = session.Snapshot();
                Assert.That(after.players[0].inventory.heroSlots[0].itemId, Is.EqualTo("I000"));
                Assert.That(after.players[0].gold, Is.LessThan(before.players[0].gold));
                Assert.That(after.shops.First(s => s.instanceId == shop.instanceId).stock.First(i => i.itemId == "I04J").available,
                    Is.EqualTo(stock.available - 1));
            }
            finally { UnityEngine.Object.DestroyImmediate(mapObject); }
        }

        [Test]
        public void PresentationChangesCannotReplaceTheAuthoritativeRulesUnderTheSameHash()
        {
            var data = OriginalGameCatalogs.Load(Assets());
            var hash = data.Fingerprint;
            data.Combat.selectedHeroes[0].id = "ZZZZ";
            data.Items.items = Array.Empty<OriginalItemDefinition>();
            data.ObservedItems.items = Array.Empty<OriginalObservedItem>();
            data.Passives.items = Array.Empty<OriginalItemEffectDefinition>();
            var session = data.CreateSession(OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 4);
            Assert.That(session.Apply(0, new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero,
                sequence = 1, heroId = "ZZZZ" }), Is.EqualTo(OriginalSessionReplyCode.InvalidHero));
            Assert.That(session.Apply(0, new OriginalSessionCommand { kind = OriginalSessionCommandKind.SelectHero,
                sequence = 2, heroId = "H008" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(session.Snapshot().contentHash, Is.EqualTo(hash));
        }
    }
}

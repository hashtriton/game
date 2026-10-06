using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalShopPlacementTests
    {
        [Test] public void StandardLayoutUsesThirteenCentralShopsAndTwoIndependentElixirShops()
        {
            var shops = OriginalShops.Placements(false);
            Assert.That(shops.Length, Is.EqualTo(16));
            Assert.That(shops.Select(s => s.instanceId).Distinct().Count(), Is.EqualTo(16));
            var north = shops.Single(s => s.instanceId == 201);
            Assert.That(north.unitId, Is.EqualTo("n0AL"));
            Assert.That(north.position.x, Is.EqualTo(-1945));
            Assert.That(north.position.y, Is.EqualTo(2605));
            Assert.That(shops.Single(s => s.unitId == "n05V").position.x, Is.EqualTo(326));
        }

        [Test] public void CompactLayoutRetainsTheOriginalSacrificialAndElixirShops()
        {
            var shops = OriginalShops.Placements(true);
            Assert.That(shops.Length, Is.EqualTo(18));
            Assert.That(shops.Count(s => s.instanceId >= 102 && s.instanceId <= 114), Is.EqualTo(13));
            Assert.That(shops.Single(s => s.unitId == "n05Z").position.x, Is.EqualTo(-450));
            Assert.That(shops.Single(s => s.unitId == "n05Y").position.y, Is.EqualTo(836));
            Assert.That(shops.Any(s => s.unitId == "n05V"), Is.False);
            Assert.That(shops.Single(s => s.unitId == "n004").position.x, Is.EqualTo(200));
        }

        [Test] public void CombatHidesCentralShopsButKeepsBothExternalElixirShops()
        {
            foreach (bool compact in new[] { false, true })
                foreach (var shop in OriginalShops.Placements(compact))
                {
                    if (shop.instanceId == OriginalShops.AcolyteInstanceId) continue;
                    Assert.That(OriginalShops.IsOpen(shop.instanceId, true), Is.True);
                    Assert.That(OriginalShops.IsOpen(shop.instanceId, false), Is.EqualTo(shop.instanceId >= 201));
                }
        }
        [Test] public void AcolyteHasSourceCenterButRequiresExplicitPresence()
        {
            foreach (bool compact in new[] { false, true })
            {
                var shop = OriginalShops.Placements(compact).Single(s => s.instanceId == OriginalShops.AcolyteInstanceId);
                Assert.That(shop.unitId, Is.EqualTo("u00E")); Assert.That(shop.position.x, Is.EqualTo(-416)); Assert.That(shop.position.y, Is.EqualTo(608));
                Assert.That(OriginalShops.IsOpen(shop.instanceId, true), Is.False);
                Assert.That(OriginalShops.IsOpen(shop.instanceId, true, true), Is.True);
            }
        }
    }
}

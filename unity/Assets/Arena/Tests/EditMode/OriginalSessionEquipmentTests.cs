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
    public sealed class OriginalSessionEquipmentTests
    {
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
        static object Player(OriginalSession s) => ((IList)typeof(OriginalSession).GetField("players", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s) => (OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
        static OriginalHeroStatsSnapshot Stats(OriginalSession s) => (OriginalHeroStatsSnapshot)typeof(OriginalSession).GetMethod("HeroCombatStats", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, new object[] { 1 });
        static OriginalSession Create() => CreateHero("H008");
        static OriginalSession CreateHero(string heroId)
        {
            var native = Load<OriginalNativeCatalog>("native126");
            // Synthetic stock availability isolates equipment transactions.
            foreach (var item in native.items)
                foreach (var field in item.fields)
                    if (field.field == "stockStart")
                    { field.known = true; field.kind = "number"; field.number = 0; field.state = "map-declaration"; field.sources = new[] { new OriginalNativeEvidence { source = 0, line = 1 } }; }
            var s = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"), Load<OriginalCombatCatalog>("combat"),
                Load<OriginalDuelCatalog>("duels"), new string('a', 64), OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            var observed = Load<OriginalObservedCatalog>("observed126");
            s.ConfigureProgression(native, observed);
            s.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            s.ConfigureItems(native, Load<OriginalObservedItemCatalog>("observed-items126"), Load<OriginalItemPassiveCatalog>("item-passives"));
            s.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = heroId });
            s.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Inventory(s).GrantResources(10000, 100);
            return s;
        }
        static OriginalSessionReplyCode Send(OriginalSession s, OriginalSessionCommandKind kind, string offer = "I04P", OriginalInventoryBag bag = OriginalInventoryBag.Hero, int slot = 0)
        {
            var shop = s.Snapshot().shops.First(x => x.stock.Any(stock => stock.itemId == offer));
            return s.Apply(0, new OriginalSessionCommand { sequence = s.Snapshot().players[0].acknowledgedSequence + 1, kind = kind,
                itemId = offer, shopInstanceId = shop.instanceId, itemSlot = slot, bag = bag });
        }

        [Test] public void SpaceBootsKeepOnlyPrimaryAttributesAndRemoveWithoutLosingBaseStats()
        {
            foreach (string hero in new[] { "H008", "N0A0", "H024" })
            {
                var s = CreateHero(hero); var p = Player(s); var baseline = Stats(s); var old = Inventory(s);
                var candidate = old.Copy(); candidate.TryPickup(candidate.CreateInstance("I05P"));
                var apply = typeof(OriginalSession).GetMethod("ApplyEquipmentProfile", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(apply.Invoke(s, new object[] { p, candidate, World(s).UnitState(1) }), Is.True, hero);
                p.GetType().GetField("inventory").SetValue(p, candidate);
                var equipped = Stats(s);
                Assert.That(equipped.strength.Require() - baseline.strength.Require(), Is.EqualTo(baseline.primaryAttribute == "STR" ? 30 : 0), hero);
                Assert.That(equipped.agility.Require() - baseline.agility.Require(), Is.EqualTo(baseline.primaryAttribute == "AGI" ? 30 : 0), hero);
                Assert.That(equipped.intelligence.Require() - baseline.intelligence.Require(), Is.EqualTo(baseline.primaryAttribute == "INT" ? 30 : 0), hero);
                candidate = candidate.Copy(); candidate.Transfer(OriginalInventoryBag.Hero, 0);
                Assert.That(apply.Invoke(s, new object[] { p, candidate, World(s).UnitState(1) }), Is.True, hero);
                p.GetType().GetField("inventory").SetValue(p, candidate);
                Assert.That(Stats(s).primary.Require(), Is.EqualTo(baseline.primary.Require()), hero);
                Assert.That(World(s).UnitState(1).profile.maxHealth, Is.EqualTo(baseline.maxHealth.Require()), hero);
            }
        }

        [Test] public void HostCombatViewTracksPurchasesAndRoundTripsWithoutChangingPreviousSnapshot()
        {
            var codec=new OriginalUnitySessionCodec();
            foreach(string hero in new[]{"H008","N0A0","H024"})
            {
                var s=CreateHero(hero);var before=s.Snapshot().players[0].combat;
                Assert.That(before.known,Is.True,hero);Assert.That(before.attackInterval,Is.GreaterThan(0));
                Assert.That(Send(s,OriginalSessionCommandKind.BuyItem,"I04J"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                var equipped=s.Snapshot();var combat=equipped.players[0].combat;
                Assert.That(combat.attackMinimum-before.attackMinimum,Is.EqualTo(16).Within(.00001));
                Assert.That(combat.attackMaximum-before.attackMaximum,Is.EqualTo(16).Within(.00001));
                var response=new OriginalNetworkResponse{kind=OriginalNetworkResponseKind.Snapshot,assignedSlot=1,
                    acknowledgedSequence=equipped.players[0].acknowledgedSequence,code=OriginalSessionReplyCode.Accepted,snapshot=equipped};
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response),out var copy),Is.True,hero);
                Assert.That(copy.snapshot.players[0].combat.attackMaximum,Is.EqualTo(combat.attackMaximum));
                Assert.That(Send(s,OriginalSessionCommandKind.TransferItem,"I04J"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(s.Snapshot().players[0].combat.attackMinimum,Is.EqualTo(before.attackMinimum));
                Assert.That(equipped.players[0].combat.attackMinimum,Is.EqualTo(combat.attackMinimum));
            }
        }

        [Test] public void CombatViewCodecRejectsInvalidRangesAndFalseUnknownValues()
        {
            var s=Create();var codec=new OriginalUnitySessionCodec();
            foreach(int invalid in new[]{0,1,2,3,4})
            {
                var view=s.Snapshot();var stats=view.players[0].combat;
                switch(invalid)
                {
                    case 0:stats.attackMaximum=stats.attackMinimum-1;break;
                    case 1:stats.attackInterval=0;break;
                    case 2:stats.primaryAttribute="INVALID";break;
                    case 3:stats.armor=1000000001;break;
                    case 4:stats.known=false;break;
                }
                view.players[0].combat=stats;
                Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(new OriginalNetworkResponse{
                    kind=OriginalNetworkResponseKind.Snapshot,assignedSlot=1,acknowledgedSequence=view.players[0].acknowledgedSequence,
                    code=OriginalSessionReplyCode.Accepted,snapshot=view}),out _),Is.False,"Invalid case "+invalid);
            }
        }

        [Test] public void DarkWaveItemRankSurvivesOneCopyAndClearsWhenLastAbilityLeavesHero()
        {
            var s = Create(); var p = Player(s); var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var apply = typeof(OriginalSession).GetMethod("ApplyEquipmentProfile", flags);
            var abilities = typeof(OriginalSession).GetMethod("ItemNativeCombatAbilities", flags);
            void Publish(OriginalInventory candidate)
            {
                Assert.That(apply.Invoke(s, new object[] { p, candidate, World(s).UnitState(1) }), Is.True);
                p.GetType().GetField("inventory").SetValue(p, candidate);
            }
            var next = Inventory(s).Copy(); next.TryPickup(next.CreateInstance("I00H")); next.TryPickup(next.CreateInstance("I00H")); Publish(next);
            var ranks = (System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<string>>)typeof(OriginalSession).GetField("waveDarkRanks", flags).GetValue(s);
            ranks[1] = new System.Collections.Generic.HashSet<string> { "A00W" };
            var rows = (OriginalItemNativeCombatAbility[])abilities.Invoke(s, new object[] { 1 });
            Assert.That(rows.Where(x => x.abilityId == "A00W").Select(x => x.rank), Is.EqualTo(new[] { 2, 2 }));
            next = Inventory(s).Copy(); next.Transfer(OriginalInventoryBag.Hero, 0); Publish(next);
            Assert.That(((OriginalItemNativeCombatAbility[])abilities.Invoke(s, new object[] { 1 })).Single(x => x.abilityId == "A00W").rank, Is.EqualTo(2));
            next = Inventory(s).Copy(); next.Transfer(OriginalInventoryBag.Hero, 1); Publish(next);
            next = Inventory(s).Copy(); next.Transfer(OriginalInventoryBag.Servant, 0); Publish(next);
            Assert.That(((OriginalItemNativeCombatAbility[])abilities.Invoke(s, new object[] { 1 })).Single(x => x.abilityId == "A00W").rank, Is.EqualTo(1));
            Assert.That(rows.Where(x => x.abilityId == "A00W").All(x => x.rank == 2), Is.True, "Previously published rows must remain detached.");
        }

        [Test] public void StrengthPurchaseUpdatesVitalsAndTransferRemovesOnlyHeroBonuses()
        {
            var s = Create(); var world = World(s); var before = world.UnitState(1);
            // Actual LiAEquip1 I00C before-state and immediate result.
            world.UpdateProfile(1, before.profile, 315.7397155761719, before.profile.maxMana * .5);
            var baseline = Stats(s);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var after = world.UnitState(1);
            Assert.That(after.profile.maxHealth, Is.EqualTo(before.profile.maxHealth + 96));
            Assert.That(after.health, Is.EqualTo(364));
            Assert.That(Stats(s).strength.Require(), Is.EqualTo(baseline.strength.Require() + 12));
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(before.profile.maxHealth));
            Assert.That(Stats(s).strength.Require(), Is.EqualTo(baseline.strength.Require()));
            Assert.That(s.Snapshot().players[0].inventory.servantSlots[0].itemId, Is.EqualTo("I00C"));
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(before.profile.maxHealth + 96));
        }

        [Test] public void DamageBonusIsSeparateFromPrimaryAndLearningDoesNotEraseEquipment()
        {
            var s = Create(); var before = Stats(s);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, "I04J"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var after = Stats(s);
            Assert.That(after.itemAttackDamageBonus, Is.EqualTo(16));
            Assert.That(after.primaryDamageBonus.Require(), Is.EqualTo(before.primaryDamageBonus.Require()));
            Assert.That(after.attackMinimum.Require(), Is.EqualTo(before.attackMinimum.Require() + 16));
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Stats(s).itemAttackDamageBonus, Is.EqualTo(16));
            after.itemAttackDamageBonus = 999;
            Assert.That(Stats(s).itemAttackDamageBonus, Is.EqualTo(16));
        }

        [Test] public void LowLifeRemovalClampsToOneAndCommitsWithoutKillingHero()
        {
            var s = Create(); var world = World(s); var before = world.UnitState(1);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var equipped = world.UnitState(1);
            // LiAItemR1 now measures eleven low-life add/remove cases: the
            // native item operation clamps a living unit to one HP.
            world.UpdateProfile(1, equipped.profile, .45, equipped.mana);
            var gold = Inventory(s).Gold;
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).Gold, Is.EqualTo(gold));
            Assert.That(Inventory(s).HeroSlots.All(x => x == null), Is.True);
            Assert.That(Inventory(s).ServantSlots[0].itemId, Is.EqualTo("I00C"));
            Assert.That(world.UnitState(1).health, Is.EqualTo(1));
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(before.profile.maxHealth));
        }

        static void RemoveSyntheticProfile(OriginalSession session,string id)
        {
            // Keep rejection tests independent of expanding real content coverage.
            var effects = (OriginalInventoryEffects)typeof(OriginalSession).GetField("itemEffects", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            var profiles = (IDictionary)typeof(OriginalInventoryEffects).GetField("nativeProfiles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(effects);
            profiles.Remove(id);
        }
        [Test] public void UnsupportedPurchasedProfileRollsBackStockGoldAndInventory()
        {
            var s = Create();
            RemoveSyntheticProfile(s,"I02A");
            var before = s.Snapshot();
            var stock = before.shops.First(x => x.stock.Any(y => y.itemId == "I04W"));
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, "I04W"), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            var after = s.Snapshot();
            Assert.That(after.players[0].gold, Is.EqualTo(before.players[0].gold));
            Assert.That(after.players[0].inventory.heroSlots.All(x => x == null), Is.True);
            Assert.That(after.shops.First(x => x.instanceId == stock.instanceId).stock.First(x => x.itemId == "I04W").available,
                Is.EqualTo(stock.stock.First(x => x.itemId == "I04W").available));
            Assert.That(World(s).UnitState(1).profile.maxHealth, Is.EqualTo(631));
        }

        [Test] public void RepeatedProgressionSynchronizationKeepsItemBonusesExactlyOnce()
        {
            var s = Create();
            double baselineStrength = Stats(s).strength.Require();
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var before = World(s).UnitState(1);
            var synchronize = typeof(OriginalSession).GetMethod("SyncProgressionExperience", BindingFlags.Instance | BindingFlags.NonPublic);
            for (int i = 0; i < 4; i++) synchronize.Invoke(s, new[] { Player(s) });
            var after = World(s).UnitState(1);
            Assert.That(s.HaltReason, Is.Null);
            Assert.That(after.profile.maxHealth, Is.EqualTo(before.profile.maxHealth));
            Assert.That(after.health, Is.EqualTo(before.health));
            Assert.That(Stats(s).strength.Require(), Is.EqualTo(baselineStrength + 12));
        }

        [Test] public void MeasuredBootPurchaseAndTransferRebuildMovementWithoutAccumulation()
        {
            var s = Create(); var world = World(s);
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, "I04U"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(300));
            Assert.That(Stats(s).baseMoveSpeed, Is.EqualTo(300));
            var second = Inventory(s).CreateInstance("I08U");
            Assert.That(Inventory(s).TryPickup(second, OriginalInventoryBag.Servant).Applied, Is.True);
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(330));
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(1031));
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, slot: 1), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(300));
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(world.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
            Assert.That(world.UnitState(1).profile.maxHealth, Is.EqualTo(631));
        }

        [Test] public void ItemResistanceTransactionsPreserveLastAddedAndRejectedCandidateCannotReorder()
        {
            var s = Create();
            double Resistance() => (double)typeof(OriginalSession).GetMethod("NativeSpellResistanceMultiplier", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, new object[] { 1 });
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, "I04T"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Resistance(), Is.EqualTo(.8));
            var second = Inventory(s).CreateInstance("I026");
            Assert.That(Inventory(s).TryPickup(second, OriginalInventoryBag.Servant).Applied, Is.True);
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Resistance(), Is.EqualTo(.75));
            RemoveSyntheticProfile(s,"I02A");
            long gold = Inventory(s).Gold;
            Assert.That(Send(s, OriginalSessionCommandKind.BuyItem, "I04W"), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(Inventory(s).Gold, Is.EqualTo(gold)); Assert.That(Resistance(), Is.EqualTo(.75));
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, slot: 1), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Resistance(), Is.EqualTo(.8));
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Resistance(), Is.EqualTo(.75));
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem, slot: 1), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Resistance(), Is.EqualTo(1)); Assert.That(s.HaltReason, Is.Null);
        }
    }
}

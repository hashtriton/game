using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionInteractionTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        const OriginalSessionCommandKind ItemInteract = OriginalSessionCommandKind.InteractItem;
        const OriginalSessionCommandKind WellInteract = OriginalSessionCommandKind.InteractWell;
        static OriginalSession Create() => (OriginalSession)typeof(OriginalSessionEquipmentTests)
            .GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        static T Load<T>(string name) => UnityEngine.JsonUtility.FromJson<T>(File.ReadAllText(
            Path.Combine(UnityEngine.Application.dataPath, "Arena/Data/lia39-" + name + ".json")));
        static OriginalSession CreateTwoPlayers()
        {
            var native = Load<OriginalNativeCatalog>("native126"); var observed = Load<OriginalObservedCatalog>("observed126");
            var s = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a',64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            var navigation = (IOriginalWorldNavigation)Activator.CreateInstance(
                typeof(OriginalSessionEquipmentTests).GetNestedType("Navigation", BindingFlags.NonPublic), true);
            s.ConfigureProgression(native, observed); s.ConfigureWorld(navigation, native, Array.Empty<OriginalWorldDoodadView>(), observed);
            s.ConfigureItems(native, Load<OriginalObservedItemCatalog>("observed-items126"), Load<OriginalItemPassiveCatalog>("item-passives"));
            Assert.That(s.Apply(10, new OriginalSessionCommand { kind=OriginalSessionCommandKind.Hello, sequence=1,
                protocol=OriginalSession.Protocol, contentHash=new string('a',64) }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            foreach(var step in new[] { (0L,1L,OriginalSessionCommandKind.SelectHero,"H008"),
                (10L,2L,OriginalSessionCommandKind.SelectHero,"H024"), (0L,2L,OriginalSessionCommandKind.LobbyReady,(string)null),
                (10L,3L,OriginalSessionCommandKind.LobbyReady,(string)null), (0L,3L,OriginalSessionCommandKind.Start,(string)null) })
                Assert.That(s.Apply(step.Item1,new OriginalSessionCommand { sequence=step.Item2,kind=step.Item3,heroId=step.Item4,ready=true }),
                    Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return s;
        }
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(s);
        static OriginalInventory Inventory(OriginalSession s)
        {
            var p = ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(s))[0];
            return (OriginalInventory)p.GetType().GetField("inventory").GetValue(p);
        }
        static SortedDictionary<long, OriginalGroundItemView> Ground(OriginalSession s) =>
            (SortedDictionary<long, OriginalGroundItemView>)typeof(OriginalSession).GetField("groundItems", Private).GetValue(s);
        static long GroundItem(OriginalSession s, double offset, string id = "I04J", int owner = 0)
        {
            var item = Inventory(s).CreateInstance(id, owner);
            var p = World(s).UnitState(1).position;
            Ground(s).Add(item.instanceId, new OriginalGroundItemView { item = item, position = new OriginalPoint(p.x + offset, p.y) });
            return item.instanceId;
        }
        static OriginalSessionReplyCode Send(OriginalSession s, OriginalSessionCommandKind kind, long item = 0, int actor = 1,
            double x = 0, double y = 0, string skill = null, int slot = 0) => s.Apply(0,
                new OriginalSessionCommand { kind = kind, sequence = s.Snapshot().players[0].acknowledgedSequence + 1,
                    actorEntityId = actor, itemInstanceId = item, itemSlot = slot, x = x, y = y, skillId = skill });
        static void Tick(OriginalSession s, int count = 100)
        { for (int i = 0; i < count; i++) { s.Advance(.05); s.DrainEvents(); } }
        static bool Has(OriginalSession s, long id) => Inventory(s).HeroSlots.Concat(Inventory(s).ServantSlots).Any(i => i?.instanceId == id);
        static void Well(OriginalSession s, double mana = 2000)
        {
            typeof(OriginalSession).GetField("healingWellPresent", Private).SetValue(s, true);
            typeof(OriginalSession).GetField("healingWellMana", Private).SetValue(s, mana);
            var actor = World(s).UnitState(1);
            Assert.That(World(s).UpdateProfile(1, actor.profile, 500, 100), Is.True);
        }

        [Test] public void DistantPickupMovesThroughAuthorityBeforeReceivingTheItem()
        {
            var s = Create(); long id = GroundItem(s, 900); var start = World(s).UnitState(1).position;
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Has(s, id), Is.False); Assert.That(Ground(s).ContainsKey(id), Is.True);
            Assert.That(World(s).UnitState(1).position.x, Is.EqualTo(start.x));
            Tick(s, 20); Assert.That(World(s).UnitState(1).position.x, Is.GreaterThan(start.x + 100));
            Assert.That(Has(s, id), Is.False);
            Tick(s); Assert.That(Has(s, id), Is.True); Assert.That(Ground(s).ContainsKey(id), Is.False);
            Assert.That(World(s).UnitState(1).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
        }

        [Test] public void NearbyPickupUsesTheExistingAtomicTransactionImmediately()
        {
            var s = Create(); long id = GroundItem(s, 20);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Has(s, id), Is.True); Assert.That(Ground(s).ContainsKey(id), Is.False);
        }

        [TestCase(OriginalSessionCommandKind.Stop)]
        [TestCase(OriginalSessionCommandKind.HoldPosition)]
        [TestCase(OriginalSessionCommandKind.Move)]
        public void AcceptedExplicitMotionOrderCancelsPendingPickup(OriginalSessionCommandKind kind)
        {
            var s = Create(); long id = GroundItem(s, 900); var start = World(s).UnitState(1).position;
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, kind, x: start.x, y: start.y - 250), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s, 100); Assert.That(Has(s, id), Is.False); Assert.That(Ground(s).ContainsKey(id), Is.True);
        }

        [Test] public void RejectedMoveAndCastLeaveTheAcceptedPickupIntentIntact()
        {
            var s = Create(); long id = GroundItem(s, 900);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, OriginalSessionCommandKind.Move, x: double.NaN), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Send(s, OriginalSessionCommandKind.CastSkill, skill: "BAD!"), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Tick(s); Assert.That(Has(s, id), Is.True);
        }

        [Test] public void AcceptedHeroCastCancelsThePendingPickup()
        {
            var s = Create(); long id = GroundItem(s, 900);
            Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, skill: "A05N"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, OriginalSessionCommandKind.CastSkill, skill: "A05N"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s, 40);
            Assert.That(World(s).Relocate(1, Ground(s)[id].position), Is.True);
            Tick(s, 2); Assert.That(Has(s, id), Is.False); Assert.That(Ground(s).ContainsKey(id), Is.True);
        }

        [Test] public void AcceptedUseItemAndCanonicalDropCancelThePendingPickup()
        {
            foreach (bool drop in new[] { false, true })
            {
                var s = Create(); var potion = Inventory(s).CreateInstance("I03L");
                Assert.That(Inventory(s).TryPickup(potion).Applied, Is.True);
                var actor = World(s).UnitState(1); World(s).UpdateProfile(1, actor.profile, 500, actor.mana);
                long id = GroundItem(s, 900);
                Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(Send(s, drop ? OriginalSessionCommandKind.DropItem : OriginalSessionCommandKind.UseItem,
                    potion.instanceId, actor: drop ? 999999 : 1), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Tick(s, 100); Assert.That(Has(s, id), Is.False); Assert.That(Ground(s).ContainsKey(id), Is.True);
            }
        }

        [Test] public void RejectedUseItemLeavesThePendingPickupIntact()
        {
            var s = Create(); long id = GroundItem(s, 900);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, OriginalSessionCommandKind.UseItem), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Tick(s); Assert.That(Has(s, id), Is.True);
        }

        [Test] public void ANewInteractionReplacesTheEarlierTarget()
        {
            var s = Create(); long first = GroundItem(s, 900), second = GroundItem(s, -700);
            Assert.That(Send(s, ItemInteract, first), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, ItemInteract, second), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s); Assert.That(Has(s, first), Is.False); Assert.That(Has(s, second), Is.True);
            Assert.That(Ground(s).ContainsKey(first), Is.True);
        }

        [Test] public void VanishedTargetAndDeadActorCannotReceiveADeferredPickup()
        {
            var s = Create(); long id = GroundItem(s, 900);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Ground(s).Remove(id); Tick(s);
            Assert.That(Has(s, id), Is.False);
            var dead = Create(); long other = GroundItem(dead, 900);
            Assert.That(Send(dead, ItemInteract, other), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            World(dead).ForceUnitDeath(1); Tick(dead, 2);
            Assert.That(Has(dead, other), Is.False); Assert.That(Ground(dead).ContainsKey(other), Is.True);
        }

        [Test] public void TwoPlayersChasingOneInstanceCannotDuplicateItsResidency()
        {
            var s = CreateTwoPlayers(); long id = GroundItem(s,900);
            Assert.That(Send(s, ItemInteract,id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            long sequence = s.Snapshot().players.Single(p=>p.slot==2).acknowledgedSequence+1;
            Assert.That(s.Apply(10,new OriginalSessionCommand {kind=ItemInteract,sequence=sequence,itemInstanceId=id,actorEntityId=2}),
                Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, WellInteract,actor:2), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Tick(s,160); var view=s.Snapshot();
            Assert.That(view.players.Sum(p=>p.inventory.heroSlots.Concat(p.inventory.servantSlots).Count(i=>i?.instanceId==id)), Is.EqualTo(1));
            Assert.That(Ground(s).ContainsKey(id), Is.False);
            Assert.That(s.HaltReason, Is.Null.Or.Empty);
        }

        [Test] public void OwnedSummonsAndIllusionsCannotUseTheHeroesInventoryPickup()
        {
            var s = Create(); var w = World(s); long id = GroundItem(s, 900);
            var hero = w.UnitState(1); var p = new OriginalWorldUnitProfile { collisionRadius = 8, maxHealth = 100, moveSpeed = 100 };
            Assert.That(w.TryPublishSummons(new[] { new OriginalWorldSummonSpawn { entityId = 100000044, ownerSlot = 1,
                sourceHeroEntityId = 1, rawcode = "hfoo", profile = p, health = 100,
                position = new OriginalPoint(hero.position.x + 100, hero.position.y) } }), Is.True);
            w.AddIllusion(1000000044, 1, hero.profile, new OriginalPoint(hero.position.x - 100, hero.position.y), hero.health, hero.mana);
            Assert.That(Send(s, ItemInteract, id, 100000044), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Send(s, ItemInteract, id, 1000000044), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Ground(s).ContainsKey(id), Is.True);
        }

        [Test] public void FullBagsKeepOneGroundResidencyThroughTheExistingGroundedFallback()
        {
            var s = Create(); var inventory = Inventory(s);
            long initial = GroundItem(s, 20);
            Assert.That(Send(s, OriginalSessionCommandKind.PickupItem, initial), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            string inventoryId = Inventory(s).HeroSlots.Single(i => i != null).itemId;
            inventory = Inventory(s);
            foreach (string field in new[] { "heroSlots", "servantSlots" })
            {
                var slots = (OriginalItemInstance[])typeof(OriginalInventory).GetField(field, Private).GetValue(inventory);
                for (int i = 0; i < slots.Length; i++) slots[i] = inventory.CreateInstance(inventoryId);
            }
            long id = GroundItem(s, 900);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s); Assert.That(Has(s, id), Is.False); Assert.That(Ground(s).Count, Is.EqualTo(1));
            Assert.That(Inventory(s).HeroSlots.Concat(Inventory(s).ServantSlots).Count(i => i != null), Is.EqualTo(12));
        }

        [Test] public void WellApproachHealsOnlyAfterTheActorReallyEntersItsRange()
        {
            var s = Create(); Well(s); var start = World(s).UnitState(1).position;
            Assert.That(Send(s, WellInteract), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().well.mana, Is.EqualTo(2000));
            Assert.That(World(s).UnitState(1).position.y, Is.EqualTo(start.y));
            Tick(s, 10); Assert.That(s.Snapshot().well.mana, Is.EqualTo(2000));
            Tick(s, 130); var actor = World(s).UnitState(1);
            Assert.That(actor.health, Is.EqualTo(actor.profile.maxHealth).Within(1e-6));
            Assert.That(actor.mana, Is.EqualTo(actor.profile.maxMana).Within(1e-6));
            Assert.That(s.Snapshot().well.mana, Is.LessThan(2000));
            Assert.That(actor.order, Is.EqualTo(OriginalWorldOrder.None));
        }

        [Test] public void OrganicOwnedSummonCanApproachTheWellButIllusionsAndEnemiesCannot()
        {
            var s = Create(); Well(s); var w = World(s); var hero = w.UnitState(1);
            var profile = new OriginalWorldUnitProfile { collisionRadius = 8, maxHealth = 100, moveSpeed = 200 };
            Assert.That(w.TryPublishSummons(new[] { new OriginalWorldSummonSpawn { entityId = 100000044, ownerSlot = 1,
                sourceHeroEntityId = 1, rawcode = "hfoo", profile = profile, health = 50,
                position = new OriginalPoint(hero.position.x + 100, hero.position.y) } }), Is.True);
            w.AddIllusion(1000000044, 1, hero.profile, new OriginalPoint(hero.position.x - 100, hero.position.y), hero.health, hero.mana);
            w.AddUnit(9442, 0, "hfoo", profile, new OriginalPoint(hero.position.x, hero.position.y + 1800));
            Assert.That(Send(s, WellInteract, actor: 1000000044), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Send(s, WellInteract, actor: 9442), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            // This rejection fixture has no mirror-combat profile; remove it
            // before advancing the unrelated organic summon approach.
            w.RemoveUnit(1000000044);
            Assert.That(Send(s, WellInteract, actor: 100000044), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Tick(s, 200); var recipient = w.UnitState(100000044);
            Assert.That(recipient.health, Is.EqualTo(100).Within(1e-6),
                "position=" + recipient.position.x + "," + recipient.position.y + " order=" + recipient.order +
                " mana=" + s.Snapshot().well.mana + " halt=" + s.HaltReason);
            Assert.That(s.Snapshot().well.mana, Is.LessThan(2000));
        }

        [Test] public void AbsentWellAndInvalidActorDoNotReplaceAnExistingPickup()
        {
            var s = Create(); long id = GroundItem(s, 900);
            Assert.That(Send(s, ItemInteract, id), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s, WellInteract), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(Send(s, ItemInteract, id, actor: 999999), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Tick(s); Assert.That(Has(s, id), Is.True);
        }

        [Test] public void WireRejectsInvalidItemIdentityAndWellItemPayload()
        {
            var codec = new OriginalUnitySessionCodec();
            var item = new OriginalSessionCommand { kind = ItemInteract, sequence = 1, actorEntityId = 1 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(item), out _), Is.False);
            item.itemInstanceId = 123;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(item), out _), Is.True);
            var well = new OriginalSessionCommand { kind = WellInteract, sequence = 1, actorEntityId = 1, itemInstanceId = 123 };
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(well), out _), Is.False);
            well.itemInstanceId = 0;
            Assert.That(codec.TryDecodeCommand(codec.EncodeCommand(well), out _), Is.True);
        }
    }
}


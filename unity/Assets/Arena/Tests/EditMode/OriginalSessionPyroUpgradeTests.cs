using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionPyroUpgradeTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Player(OriginalSession s) => ((IList)typeof(OriginalSession).GetField("players", Private).GetValue(s))[0];
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, Private).Invoke(s, args);
        static OriginalInventory Inventory(OriginalSession s) => (OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(s);
        static OriginalSessionReplyCode Send(OriginalSession s, OriginalSessionCommandKind kind, string id = null, int slot = 0, OriginalInventoryBag bag = OriginalInventoryBag.Hero) =>
            s.Apply(0, new OriginalSessionCommand { sequence = s.Snapshot().players[0].acknowledgedSequence + 1, kind = kind,
                skillId = id, itemSlot = slot, bag = bag, x = 700, y = 1000 });
        static void Advance(OriginalSession s, double seconds)
        { while (seconds > 1e-9) { double d = Math.Min(.01, seconds); s.Advance(d); s.DrainEvents(); seconds -= d; } Assert.That(s.HaltReason, Is.Null); }
        static OriginalSession Create(bool learned = true)
        {
            var s = (OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("CreateHero", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { "H024" });
            var p = Player(s); var field = p.GetType().GetField("progression");
            var candidate = ((OriginalHeroProgression)field.GetValue(p)).Copy(); candidate.GrantExperience(12000);
            var stats = (OriginalHeroStatsSnapshot)Call(s, "CalculateProgressionStats", "H024", candidate);
            Call(s, "ApplyProgressionProfile", p, stats, candidate); field.SetValue(p, candidate); p.GetType().GetField("stats").SetValue(p, stats);
            if (learned)
            {
                Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, "A0SP"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, "A0SM"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            }
            return s;
        }
        static void AddServant(OriginalSession s, string id)
        { Assert.That(Inventory(s).TryPickup(Inventory(s).CreateInstance(id), OriginalInventoryBag.Servant).Applied, Is.True); }
        static void Alias(OriginalSession s, bool upgraded, int rank = 1)
        {
            var p = Wire(s).players[0];
            Assert.That(p.progression.skills.Single(x => x.id == "A0SP").rank, Is.EqualTo(rank));
            Assert.That(p.abilities.Single(x => x.id == "A0SP").castAbilityId, Is.EqualTo(upgraded ? "A0SR" : "A0SP"));
            Assert.That(p.abilities.Single(x => x.id == "A0SM").castAbilityId, Is.EqualTo(upgraded ? "A0SS" : "A0SM"));
            Assert.That(p.auxiliaryAbilities.Any(x => x.id == "A0SU" && x.rank == 1), Is.EqualTo(upgraded));
            Assert.That(p.auxiliaryAbilities.Any(x => x.id == "A0SR" && x.rank == rank), Is.EqualTo(upgraded));
        }
        static OriginalSessionView Wire(OriginalSession s)
        {
            var view = s.Snapshot(); var codec = new OriginalUnitySessionCodec();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot, assignedSlot = 1,
                acknowledgedSequence = view.players[0].acknowledgedSequence, snapshot = view };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var decoded), Is.True);
            return decoded.snapshot;
        }
        [Test] public void CraftedStaffUpgradeDuplicateRemovalAndCanonicalLearningPreserveRanksAndPoints()
        {
            var s = Create();
            foreach (string id in new[] { "I010", "I02G", "I00S" }) AddServant(s, id);
            Assert.That(Inventory(s).ServantSlots[0].itemId, Is.EqualTo("I00Z"));
            int before = s.Snapshot().players[0].progression.unspentSkillPoints;
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Alias(s, true);
            AddServant(s, "I00Z");
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Alias(s, true);
            Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, "A0SR"), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, "A0SP"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Alias(s, true, 2);
            Assert.That(s.Snapshot().players[0].progression.unspentSkillPoints, Is.EqualTo(before - 1));
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Alias(s, true, 2);
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, slot: 1), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Alias(s, false, 2);
            Assert.That(s.Snapshot().players[0].progression.unspentSkillPoints, Is.EqualTo(before - 1));
        }
        [Test] public void UpgradeUsesActualCostAndDropCannotResetCanonicalMeteorCooldown()
        {
            var s = Create(); AddServant(s, "I00Z");
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var hero = World(s).UnitState(1); World(s).UpdateProfile(1, hero.profile, hero.health, hero.profile.maxMana);
            Assert.That(s.Snapshot().players[0].abilities.Single(x => x.id == "A0SM").manaCost, Is.EqualTo(300));
            Assert.That(Send(s, OriginalSessionCommandKind.CastSkill, "A0SS"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .11);
            Assert.That(World(s).UnitState(1).mana, Is.LessThan(hero.profile.maxMana - 299));
            Assert.That(Send(s, OriginalSessionCommandKind.DropItem), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var ordinary = s.Snapshot().players[0].abilities.Single(x => x.id == "A0SM");
            Assert.That(ordinary.castAbilityId, Is.EqualTo("A0SM")); Assert.That(ordinary.cooldownRemaining, Is.GreaterThan(79));
            Assert.That(Send(s, OriginalSessionCommandKind.CastSkill, "A0SM"), Is.EqualTo(OriginalSessionReplyCode.NotReady));
        }
        [Test] public void UnlearnedStaffPublishesOnlyEngineeringThenCanonicalLearnCreatesOneAlias()
        {
            var s = Create(false); AddServant(s, "I00Z");
            int points = s.Snapshot().players[0].progression.unspentSkillPoints;
            Assert.That(Send(s, OriginalSessionCommandKind.TransferItem, bag: OriginalInventoryBag.Servant), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var before = Wire(s).players[0];
            Assert.That(before.auxiliaryAbilities.Select(x => x.id), Is.EqualTo(new[] { "A0SU" }));
            Assert.That(before.abilities.Single(x => x.id == "A0SP").castAbilityId, Is.EqualTo("A0SP"));
            Assert.That(Send(s, OriginalSessionCommandKind.CastSkill, "A0SR"), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Send(s, OriginalSessionCommandKind.LearnSkill, "A0SP"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var after = Wire(s).players[0];
            Assert.That(after.auxiliaryAbilities.Select(x => x.id), Is.EqualTo(new[] { "A0SR", "A0SU" }));
            Assert.That(after.progression.unspentSkillPoints, Is.EqualTo(points - 1));
            Assert.That(after.abilities.Single(x => x.id == "A0SP").castAbilityId, Is.EqualTo("A0SR"));
        }
    }
}

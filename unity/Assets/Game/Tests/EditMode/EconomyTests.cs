using System.Linq;
using Arena;
using Arena.Original;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    public sealed class EconomyTests
    {
        private static readonly string[] DataNames =
        {
            "lia39-match", "lia39-items", "lia39-combat", "lia39-item-passives", "lia39-duels",
            "lia39-native126", "lia39-observed126", "lia39-observed-items126", "lia39-layout"
        };

        private static ItemBook book;
        private static GuideFile guides;

        [OneTimeSetUp]
        public void LoadData()
        {
            var assets = DataNames.Select(name => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/" + name + ".json")).ToArray();
            Assert.IsTrue(assets.All(a => a != null), "the catalogs of the original map must be importable");
            book = new ItemBook(OriginalGameCatalogs.Load(assets));
            var guideAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Game/Data/guides.json");
            Assert.IsNotNull(guideAsset);
            guides = JsonUtility.FromJson<GuideFile>(guideAsset.text);
        }

        private static Loadout NewLoadout(long gold, long souls = 0) => new Loadout(book, null, gold, souls);

        [Test]
        public void Homoglyphs_turn_latin_lookalikes_into_cyrillic()
        {
            Assert.AreEqual("Когти", Homoglyphs.Clean("Кoгти"));
            Assert.AreEqual("Cutlass", Homoglyphs.Clean("Cutlass"), "text without Cyrillic is left alone");
            Assert.AreEqual("", Homoglyphs.Clean(null));
        }

        [Test]
        public void Every_shelf_has_items_and_every_name_is_clean()
        {
            Assert.GreaterOrEqual(book.Tabs.Count, 10);
            foreach (var tab in book.Tabs)
            {
                Assert.IsNotEmpty(tab.entries, tab.title);
                foreach (var entry in tab.entries)
                {
                    var name = book.Name(entry.displayId);
                    Assert.IsNotEmpty(name);
                    var mixed = name.Any(c => "aeopcxy".IndexOf(c) >= 0) && name.Any(c => c >= 'А' && c <= 'я');
                    Assert.IsFalse(mixed, "Latin lookalikes left in " + name);
                }
            }
            var basics = book.Tabs.First(t => t.id == "base");
            Assert.AreEqual(26, basics.entries.Count, "the three basic shops sell 26 different items");
        }

        [Test]
        public void Plan_for_the_huge_axe_buys_the_parts_first_and_the_recipe_last()
        {
            var plan = ShoppingPlanner.Plan(book, new OriginalItemInstance[0], "I005");
            Assert.IsTrue(plan.IsPossible, plan.problem);
            CollectionAssert.AreEquivalent(new[] { "I000", "I004", "I02A", "I006" }, plan.steps.Select(s => s.itemId).ToArray());
            Assert.AreEqual("I006", plan.steps.Last().itemId, "the scroll completes the item, so it goes last");
            Assert.AreEqual(book.TotalCost("I005"), plan.gold, "parts plus recipe add up to the item's value");
        }

        [Test]
        public void Plan_counts_the_parts_the_hero_already_carries()
        {
            var owned = new[]
            {
                new OriginalItemInstance { instanceId = 1, itemId = "I000" },
                new OriginalItemInstance { instanceId = 2, itemId = "I004" }
            };
            var plan = ShoppingPlanner.Plan(book, owned, "I005");
            Assert.AreEqual(book.TotalCost("I005") - 65 - 85, plan.gold);
            Assert.AreEqual(2, plan.steps.Count);
        }

        [Test]
        public void Plan_for_an_item_already_owned_is_empty()
        {
            var plan = ShoppingPlanner.Plan(book, new[] { new OriginalItemInstance { instanceId = 1, itemId = "I005" } }, "I005");
            Assert.IsTrue(plan.alreadyOwned);
            Assert.IsFalse(plan.IsPossible);
        }

        [Test]
        public void Buying_the_whole_plan_leaves_one_finished_item_and_the_right_change()
        {
            var loadout = NewLoadout(1000);
            var plan = loadout.PlanFor("I005");
            var outcome = loadout.BuyPlan(plan, out var bought);

            Assert.AreEqual(BuyOutcome.Bought, outcome);
            Assert.AreEqual(4, bought);
            Assert.AreEqual(1000 - plan.gold, loadout.Gold);
            CollectionAssert.AreEqual(new[] { "I005" }, loadout.OwnedIds(), "the recipe consumed its parts");
        }

        [Test]
        public void A_chain_of_two_recipes_ends_in_one_item()
        {
            // The Phantom Blade needs a Battle Axe and a Panthilus Sword, each built from its own parts.
            var loadout = NewLoadout(5000);
            var plan = loadout.PlanFor("I02L");
            Assert.IsTrue(plan.IsPossible, plan.problem);
            Assert.AreEqual(BuyOutcome.Bought, loadout.BuyPlan(plan, out _));
            CollectionAssert.AreEqual(new[] { "I02L" }, loadout.OwnedIds());
        }

        [Test]
        public void Running_out_of_gold_midway_keeps_what_was_bought_and_reports_it()
        {
            var loadout = NewLoadout(100);
            var plan = loadout.PlanFor("I005");
            var outcome = loadout.BuyPlan(plan, out var bought);

            Assert.AreEqual(BuyOutcome.NotEnoughGold, outcome);
            Assert.Greater(bought, 0);
            Assert.Less(bought, plan.steps.Count);
            Assert.GreaterOrEqual(loadout.Gold, 0);
            Assert.AreEqual(bought, loadout.OwnedIds().Count);
        }

        [Test]
        public void A_full_bag_refuses_a_purchase_instead_of_dropping_it_on_the_ground()
        {
            var loadout = NewLoadout(5000);
            foreach (var id in new[] { "I000", "I004", "I00I", "I002", "I007", "I00K" })
                Assert.AreEqual(BuyOutcome.Bought, loadout.Buy(book.EntryFor(id)), id);
            Assert.AreEqual(0, loadout.FreeSlots);

            var before = loadout.Gold;
            Assert.AreEqual(BuyOutcome.NoSpace, loadout.Buy(book.EntryFor("I00E")));
            Assert.AreEqual(before, loadout.Gold, "a refused purchase costs nothing");
        }

        [Test]
        public void Selling_returns_half_the_price()
        {
            var loadout = NewLoadout(500);
            Assert.AreEqual(BuyOutcome.Bought, loadout.Buy(book.EntryFor("I004")));
            var slot = System.Array.FindIndex(loadout.Slots, s => s != null);
            Assert.AreEqual(85 / 2, loadout.SellValue(slot));
            var gold = loadout.Gold;
            Assert.IsTrue(loadout.Sell(slot));
            Assert.AreEqual(gold + 85 / 2, loadout.Gold);
            Assert.AreEqual(6, loadout.FreeSlots);
        }

        [Test]
        public void Every_item_of_every_guide_can_be_bought_from_an_empty_bag()
        {
            Assert.GreaterOrEqual(guides.guides.Length, 5);
            foreach (var guide in guides.guides)
            {
                Assert.IsNotEmpty(guide.title);
                Assert.IsNotEmpty(guide.source, guide.id + " must name where it comes from");
                foreach (var step in guide.steps)
                {
                    foreach (var id in step.items)
                    {
                        Assert.IsNotNull(book.Items.Item(id), guide.id + ": unknown item " + id);
                        var plan = ShoppingPlanner.Plan(book, new OriginalItemInstance[0], id);
                        Assert.IsTrue(plan.IsPossible, guide.id + " / " + book.Name(id) + ": " + plan.problem);
                    }
                }
            }
        }

        [Test]
        public void Guide_progress_marks_what_is_owned_and_points_at_the_first_missing_item()
        {
            var guide = guides.guides.First(g => g.id == "knight-melee");
            var marks = GuideProgress.Evaluate(book, new[] { "I000", "I022" }, guide);
            Assert.AreEqual(ItemMark.Done, marks[0][0]);
            Assert.AreEqual(ItemMark.Done, marks[0][1]);
            Assert.AreEqual(ItemMark.Next, marks[1][0], "the first missing item is the next one to buy");
            Assert.AreEqual(ItemMark.Todo, marks[1][1]);
        }

        [Test]
        public void An_item_built_into_a_better_one_still_counts_as_done()
        {
            var guide = guides.guides.First(g => g.id == "strong-splash");
            // The huge axe is the second step; a Blood Moon contains it.
            var marks = GuideProgress.Evaluate(book, new[] { "I01P" }, guide);
            Assert.AreEqual(ItemMark.Done, marks[1][0]);
            Assert.IsTrue(GuideProgress.BuiltFrom(book, "I01P", "I005", 0));
            Assert.IsFalse(GuideProgress.BuiltFrom(book, "I005", "I01P", 0));
        }

        [Test]
        public void Every_priced_shelf_item_can_actually_be_bought()
        {
            // The shop shows a price only when the inventory rules will accept the purchase.
            var refused = new System.Collections.Generic.List<string>();
            foreach (var tab in book.Tabs)
            {
                foreach (var entry in tab.entries)
                {
                    if (!entry.PriceKnown) continue;
                    var outcome = NewLoadout(1000000, 1000000).Buy(entry);
                    if (outcome != BuyOutcome.Bought) refused.Add(tab.title + ": " + book.Name(entry.itemId) + " (" + outcome + ")");
                }
            }
            Assert.IsEmpty(refused, string.Join("; ", refused));
        }

        [Test]
        public void A_held_item_counts_for_one_copy_of_a_part_not_for_every_copy()
        {
            var guide = guides.guides.First(g => g.id == "strong-splash");
            var marks = GuideProgress.Evaluate(book, new[] { "I01P" }, guide);
            Assert.AreEqual(ItemMark.Done, marks[1][0], "the Blood Moon contains the first huge axe");
            Assert.AreNotEqual(ItemMark.Done, marks[2][0], "the second huge axe is still to be bought");
        }

        [Test]
        public void An_item_with_several_recipes_uses_the_one_the_bag_already_leans_towards()
        {
            Assert.GreaterOrEqual(book.RecipesFor("I05P").Count, 2, "the boots of space have a recipe per attribute");
            var empty = ShoppingPlanner.Plan(book, new OriginalItemInstance[0], "I05P");
            var withSword = ShoppingPlanner.Plan(book, new[] { new OriginalItemInstance { instanceId = 1, itemId = "I087" } }, "I05P");
            Assert.IsTrue(empty.IsPossible, empty.problem);
            Assert.IsTrue(withSword.IsPossible, withSword.problem);
            Assert.Less(withSword.gold, empty.gold, "carrying the Wrath Sword of one variant makes that variant cheaper");
            Assert.IsTrue(GuideProgress.BuiltFrom(book, "I05P", "I087", 0), "every variant counts as a way to build it");
        }

        [Test]
        public void A_plan_that_does_not_fit_the_bag_buys_nothing_at_all()
        {
            var loadout = NewLoadout(5000);
            foreach (var id in new[] { "I007", "I00K", "I00E", "I00R", "I00N" })
                Assert.AreEqual(BuyOutcome.Bought, loadout.Buy(book.EntryFor(id)), id);
            Assert.AreEqual(1, loadout.FreeSlots);

            var before = loadout.Gold;
            var plan = loadout.PlanFor("I005");
            Assert.AreEqual(BuyOutcome.NoSpace, loadout.BuyPlan(plan, out var bought));
            Assert.AreEqual(0, bought);
            Assert.AreEqual(before, loadout.Gold, "no gold may be left sitting in parts that do not fit");
            Assert.AreEqual(5, loadout.OwnedIds().Count);
        }

        [Test]
        public void Item_text_for_a_finished_item_names_it_and_gives_the_price()
        {
            var text = book.Text.Describe("I005");
            StringAssert.Contains("Огромный Топор", Homoglyphs.Clean(text));
            StringAssert.Contains("Цена", text);
        }
    }
}

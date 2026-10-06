using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionMirrorTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public bool blocked;
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => !blocked;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => !blocked;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        static OriginalSession Create(out Navigation nav, bool withItems = false)
        {
            var s = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            s.ConfigureProgression(Load<OriginalNativeCatalog>("lia39-native126"), Load<OriginalObservedCatalog>("lia39-observed126"));
            nav = new Navigation(); s.ConfigureWorld(nav, Load<OriginalNativeCatalog>("lia39-native126"), Array.Empty<OriginalWorldDoodadView>());
            if (withItems) s.ConfigureItems(Load<OriginalNativeCatalog>("lia39-native126"), Load<OriginalObservedItemCatalog>("lia39-observed-items126"), Load<OriginalItemPassiveCatalog>("lia39-item-passives"));
            s.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            s.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05N" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            s.DrainEvents(); return s;
        }
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
        static void Advance(OriginalSession s, double seconds)
        {
            while (seconds > 1e-8) { double step = Math.Min(.05, seconds); s.Advance(step); s.DrainEvents(); seconds -= step; }
            Assert.That(s.HaltReason, Is.Null);
        }
        static OriginalSessionReplyCode Cast(OriginalSession s, long seq) => s.Apply(0,
            new OriginalSessionCommand { sequence = seq, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05N" });
        static object Player(OriginalSession s) => ((System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s))[0];
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, args);

        [Test] public void NativeItemFlatDamageDoesNotEnterThePublishedImageCombatCapture()
        {
            var s = Create(out _, withItems: true); var player = Player(s);
            var inventory = (OriginalInventory)player.GetType().GetField("inventory").GetValue(player);
            Assert.That(inventory.TryPickup(inventory.CreateInstance("I007")).Applied, Is.True);
            var heroStats = (OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1);
            Assert.That(heroStats.itemAttackDamageBonus, Is.EqualTo(12));
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .8);
            var image = World(s).Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            var captured = Call(s, "CombatStatsFor", image);
            Assert.That((double)captured.GetType().GetField("itemAttackDamageBonus", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(captured), Is.Zero);
            Assert.That((double)captured.GetType().GetField("primaryDamageBonus", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(captured), Is.EqualTo(heroStats.primaryDamageBonus.Require()));
            Assert.That(((OriginalHeroStatsSnapshot)Call(s, "HeroCombatStats", 1)).itemAttackDamageBonus, Is.EqualTo(12));
        }

        [Test] public void ImageDoesNotCopyActiveDefendButCasterKeepsItsToggle()
        {
            var s = Create(out _); Advance(s, 2.05);
            s.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.WaveReady }); Advance(s, 3);
            foreach (var enemy in s.Snapshot().enemies.ToArray())
            { s.ReportEnemyKilled(enemy.entityId, true); if (s.Snapshot().players[0].progression.level >= 2) break; }
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 6, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 7, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Cast(s, 8), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .8);
            Assert.That(World(s).UnitState(1).profile.moveSpeed, Is.EqualTo(175).Within(1e-9));
            Assert.That(World(s).Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion).profile.moveSpeed, Is.EqualTo(250));
            Assert.That(s.Snapshot().players[0].abilities.Single(a => a.id == "A05M").toggledOn, Is.True);
        }

        [Test] public void MirrorEffectDispelsPoisonBeforeCreationWithoutClearingItAtOrderTime()
        {
            var s = Create(out _); var w = World(s);
            var projectileType = typeof(OriginalSession).GetNestedType("Projectile", BindingFlags.NonPublic);
            var shot = Activator.CreateInstance(projectileType, true);
            projectileType.GetField("attacker", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, 1001);
            projectileType.GetField("owner", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, 0);
            projectileType.GetField("poisonAbility", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, "A0TC");
            Call(s, "ApplyPoison", shot, w.UnitState(1));
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(225));
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .25);
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(225));
            Advance(s, .05); Assert.That(w.UnitState(1).hidden, Is.True);
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
            Advance(s, .5);
            Assert.That(w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion).profile.moveSpeed, Is.EqualTo(250));
        }
        [Test] public void MirrorSpendsAtEffectCreatesOneImageAndStartsCooldownAtEffect()
        {
            var s = Create(out _); var w = World(s);
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(w.UnitState(1).mana, Is.EqualTo(145));
            Advance(s, .25); Assert.That(w.Snapshot().units.Length, Is.EqualTo(1)); Assert.That(w.UnitState(1).mana, Is.EqualTo(145));
            Advance(s, .05); Assert.That(w.UnitState(1).mana, Is.EqualTo(85)); Assert.That(w.UnitState(1).hidden, Is.True);
            Advance(s, .5);
            var image = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            Assert.That(image.sourceHeroEntityId, Is.EqualTo(1)); Assert.That(image.ownerSlot, Is.EqualTo(1));
            Assert.That(image.health, Is.EqualTo(w.UnitState(1).health)); Assert.That(image.mana, Is.EqualTo(85));
            Assert.That(w.UnitState(1).hidden, Is.False);
            Assert.That(s.Snapshot().players[0].abilities.Single(a => a.id == "A05N").cooldownRemaining, Is.EqualTo(15.5).Within(1e-7));
        }
        [Test] public void AcceptedStopInterruptsBeforeManaAndCooldownAreCommitted()
        {
            var s = Create(out _); Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .1);
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 6, kind = OriginalSessionCommandKind.Stop }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Advance(s, 1); Assert.That(World(s).UnitState(1).mana, Is.EqualTo(145)); Assert.That(World(s).Snapshot().units.Length, Is.EqualTo(1));
            Assert.That(Cast(s, 7), Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }
        [Test] public void MirrorHelperUsesScriptedProxyAndNumericArmorInsteadOfWeaponRoll()
        {
            var s = Create(out _); var w = World(s);
            w.AddUnit(1001, 0, "hfoo", new OriginalWorldUnitProfile { collisionRadius = 31, moveSpeed = 270, maxHealth = 1000 }, new OriginalPoint(300, -1400));
            w.SetUnitState(1001, paused: true);
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, 1);
            Assert.That(w.UnitState(1001).health, Is.EqualTo(1000 - 67.4 / 1.12).Within(1e-6));
        }
        [Test] public void ExpiredImageDiesOnceThenIsRemovedWithoutHeroDeathOrReward()
        {
            var s = Create(out _); Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .8);
            int id = World(s).Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion).entityId;
            var before = s.Snapshot().players[0]; Advance(s, 30);
            Assert.That(World(s).UnitState(id).health, Is.Zero); Assert.That(World(s).UnitState(id).hidden, Is.True);
            Assert.That(World(s).UnitState(1).health, Is.GreaterThan(0));
            Assert.That(s.Snapshot().players[0].gold, Is.EqualTo(before.gold)); Assert.That(s.Snapshot().players[0].experience, Is.EqualTo(before.experience));
            Advance(s, 1.5); Assert.That(World(s).UnitState(id), Is.Null);
        }
        [Test] public void RecastReplacesOldGenerationAtEffectAndNeverReusesIdentity()
        {
            var s = Create(out _); var w = World(s); Cast(s, 5); Advance(s, 16.3);
            int oldId = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion).entityId;
            Assert.That(Cast(s, 6), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .3);
            Assert.That(w.UnitState(oldId).health, Is.Zero); Assert.That(w.UnitState(oldId).hidden, Is.True);
            Advance(s, .5); var fresh = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion && u.health > 0);
            Assert.That(fresh.entityId, Is.GreaterThan(oldId)); Assert.That(w.UnitState(1).mana, Is.EqualTo(25));
        }
        [Test] public void NoManaAndUnavailablePlacementDoNotPublishOrConsumeResources()
        {
            var s = Create(out var nav); var w = World(s); var hero = w.UnitState(1);
            w.UpdateProfile(1, hero.profile, hero.health, 59);
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            w.UpdateProfile(1, hero.profile, hero.health, 145); nav.blocked = true;
            Assert.That(Cast(s, 6), Is.EqualTo(OriginalSessionReplyCode.RuleUnavailable));
            Assert.That(w.UnitState(1).mana, Is.EqualTo(145)); Assert.That(w.UnitState(1).castSequence, Is.Zero);
        }
        [Test] public void NativeMirrorHiddenPhaseUsesBaseRegenAndImagesKeepCapturedAttributeRates()
        {
            var s = Create(out _); var w = World(s);
            var observed = Load<OriginalObservedCatalog>("lia39-observed126"); observed.BuildIndexes();
            typeof(OriginalSession).GetField("observed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(s, observed);
            var hero = w.UnitState(1); w.UpdateProfile(1, hero.profile, 300, 100);
            Assert.That(Cast(s, 5), Is.EqualTo(OriginalSessionReplyCode.Accepted)); Advance(s, .8);
            var image = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            Assert.That(w.UnitState(1).health, Is.EqualTo(300 + .3 * 2.4 + .5 * 1.3).Within(1e-7));
            Assert.That(w.UnitState(1).mana, Is.EqualTo(100 + .3 * .4 - 60 + .5 * .05).Within(1e-7));
            double before = image.health; Advance(s, .2);
            Assert.That(w.UnitState(image.entityId).health, Is.EqualTo(before + .2 * 2.4).Within(1e-7));
        }
        [Test] public void LivingImageRetainsCapturedCombatAfterOwnerDeathAndReceivesItsAuthoredMultiplier()
        {
            var s = Create(out _); var w = World(s); Cast(s, 5); Advance(s, 1);
            var image = w.Snapshot().units.Single(u => u.kind == OriginalWorldUnitKind.Illusion);
            w.ForceUnitDeath(1);
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 6, kind = OriginalSessionCommandKind.Move,
                actorEntityId = image.entityId, x = 600, y = -1400 }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            typeof(OriginalSession).GetMethod("ApplyTriggeredNormalHit", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(s, new object[] { 1001, 0, image, 100.0 });
            Assert.That(w.UnitState(image.entityId).health, Is.EqualTo(image.health - 100 * .8 / 1.312 * 1.75).Within(1e-6));
            Advance(s, .05); Assert.That(w.UnitState(image.entityId).position.x, Is.GreaterThan(image.position.x));
        }
        [Test] public void SourceHelperTimerStillFiresWhenTheCasterDiesAfterEffect()
        {
            var s = Create(out _); var w = World(s);
            w.AddUnit(1001, 0, "hfoo", new OriginalWorldUnitProfile { collisionRadius = 31, moveSpeed = 270, maxHealth = 1000 }, new OriginalPoint(300, -1400));
            w.SetUnitState(1001, paused: true); Cast(s, 5); Advance(s, .8); w.ForceUnitDeath(1); Advance(s, .2);
            Assert.That(w.UnitState(1001).health, Is.EqualTo(1000 - 67.4 / 1.12).Within(1e-6));
            Assert.That(w.UnitState(1).health, Is.Zero);
        }
    }
}

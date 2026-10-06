using System;
using System.IO;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionRegenerationTests
    {
        sealed class Navigation : IOriginalWorldNavigation, IOriginalWorldDynamicNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => Math.Abs(x) <= 4096 && Math.Abs(y) <= 4096;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => IsWalkable(b.x, b.y, r);
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
            readonly System.Collections.Generic.HashSet<int> dynamicIds = new System.Collections.Generic.HashSet<int>();
            // Open arena fixture: exercise publication/lifecycle only. Actual
            // footprint collision is covered by ArenaDynamicPathingTests.
            public bool TryAddDynamicDoodad(int id, string rawcode, OriginalPoint point, double facing, double scale) => dynamicIds.Add(id);
            public bool RemoveDynamicDoodad(int id) => dynamicIds.Remove(id);
        }
        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-" + file + ".json")));
        static OriginalSession Create(string hero, out OriginalWorld world)
        {
            var session = new OriginalSession(Load<OriginalMatchCatalog>("match"), Load<OriginalItemCatalog>("items"),
                Load<OriginalCombatCatalog>("combat"), Load<OriginalDuelCatalog>("duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            var native = Load<OriginalNativeCatalog>("native126"); var observed = Load<OriginalObservedCatalog>("observed126");
            session.ConfigureProgression(native, observed);
            session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(), observed);
            session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = hero });
            session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
            return session;
        }
        [Test] public void AllThreeHeroesRecoverTheDeclaredHealthAndManaRates()
        {
            var heroes = new[] { "H008", "N0A0", "H024" };
            var healthRates = new[] { 2.4, 1.25, 1.55 }; var manaRates = new[] { .4, .3, 1.01 };
            for (int i = 0; i < heroes.Length; i++)
            {
                var session = Create(heroes[i], out var world); var unit = world.UnitState(1);
                world.UpdateProfile(1, unit.profile, unit.health - 100, unit.mana - 100);
                session.Advance(1);
                Assert.That(session.HaltReason, Is.Null);
                Assert.That(world.UnitState(1).health, Is.EqualTo(unit.health - 100 + healthRates[i]).Within(1e-6));
                Assert.That(world.UnitState(1).mana, Is.EqualTo(unit.mana - 100 + manaRates[i]).Within(1e-6));
            }
        }
        [Test] public void PausingKeepsBaseRegenerationButSuppressesAttributeRatesBeforeAndAfterWarmup()
        {
            var session = Create("H008", out var world); var initial = world.UnitState(1);
            world.UpdateProfile(1, initial.profile, initial.health - 100, initial.mana - 100);
            foreach (bool paused in new[] { true, false, true })
            {
                world.SetUnitState(1, paused: paused); var before = world.UnitState(1);
                session.Advance(1);
                Assert.That(session.HaltReason, Is.Null);
                Assert.That(world.UnitState(1).paused, Is.EqualTo(paused));
                Assert.That(world.UnitState(1).health - before.health, Is.EqualTo(paused ? 1.3 : 2.4).Within(1e-6));
                Assert.That(world.UnitState(1).mana - before.mana, Is.EqualTo(paused ? .05 : .4).Within(1e-6));
            }
        }
        [Test] public void RegenerationCapsAtMaximaAndCannotReviveADeadBody()
        {
            var session = Create("H008", out var world); var unit = world.UnitState(1);
            world.UpdateProfile(1, unit.profile, unit.health - .01, unit.mana - .01); session.Advance(1);
            Assert.That(world.UnitState(1).health, Is.EqualTo(unit.profile.maxHealth));
            Assert.That(world.UnitState(1).mana, Is.EqualTo(unit.profile.maxMana));
            world.ForceUnitDeath(1); session.Advance(1);
            Assert.That(world.UnitState(1).health, Is.Zero);
        }
        [Test] public void NativeOrnHasAttributeRegenerationOnlyAndLosesItWhilePaused()
        {
            var session = Create("H008", out var world);
            var profile = new OriginalWorldUnitProfile { maxHealth = 30000, maxMana = 2500, moveSpeed = 390, collisionRadius = 24 };
            world.AddUnit(6001, 0, "O006", profile, new OriginalPoint(0, 0));
            world.UpdateProfile(6001, profile, 29900, 2400);
            foreach (bool paused in new[] { true, false, true })
            {
                world.SetUnitState(6001, paused: paused); var before = world.UnitState(6001);
                typeof(OriginalSession).GetMethod("AdvanceRegeneration", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(session, new object[] { 1d });
                Assert.That(world.UnitState(6001).health - before.health, Is.EqualTo(paused ? 0 : 12.5).Within(1e-6));
                Assert.That(world.UnitState(6001).mana - before.mana, Is.EqualTo(paused ? 0 : 12.5).Within(1e-6));
            }
        }
    }
}

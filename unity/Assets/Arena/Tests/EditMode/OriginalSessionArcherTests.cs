using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionArcherTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => true;
        }
        sealed class Fixture
        {
            internal OriginalSession session; internal OriginalWorld world; internal long sequence = 3;
            internal OriginalSessionReplyCode Send(OriginalSessionCommandKind kind, string skill = null, double x = 0, double y = 0, int actor = 0)
                => session.Apply(0, new OriginalSessionCommand { kind = kind, sequence = ++sequence, skillId = skill, x = x, y = y, actorEntityId = actor });
            internal void Advance(double seconds)
            { while (seconds > 1e-9) { double step = Math.Min(.01, seconds); session.Advance(step); session.DrainEvents(); seconds -= step; } }
            internal OriginalAbilityView Ability(string id) => session.Snapshot().players[0].abilities.Single(a => a.id == id);
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        static Fixture Create(string skill = "A15W")
        {
            var f = new Fixture(); var native = Load<OriginalNativeCatalog>("lia39-native126"); var observed = Load<OriginalObservedCatalog>("lia39-observed126");
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            f.session.ConfigureProgression(native, observed);
            f.session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>());
            f.session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "N0A0" });
            f.session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(f.session);
            f.Advance(2.05); // Source party unpause, without a wave/AI fixture.
            if (skill == "A15X" || skill == "A0AS")
            {
                // Trusted XP setup reaches the source required hero level2;
                // learning and casting still use actual public commands.
                var player = ((System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(f.session))[0];
                var field = player.GetType().GetField("progression");
                var candidate = ((OriginalHeroProgression)field.GetValue(player)).Copy(); candidate.GrantExperience(200);
                var stats = (OriginalHeroStatsSnapshot)Call(f.session, "CalculateProgressionStats", "N0A0", candidate);
                Call(f.session, "ApplyProgressionProfile", player, stats, candidate);
                field.SetValue(player, candidate); player.GetType().GetField("stats").SetValue(player, stats);
            }
            Assert.That(f.Send(OriginalSessionCommandKind.LearnSkill, skill), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return f;
        }
        [Test] public void PointCastRejectsAnotherActorWithoutSpendingOrPublishing()
        {
            var f = Create(); var before = f.world.UnitState(1);
            Assert.That(f.Ability("A15W").targetMode, Is.EqualTo(OriginalAbilityTargetMode.Point));
            Assert.That(f.Ability("A15W").implemented, Is.True);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15W", 700, -1000, 2), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana));
            Assert.That(f.world.UnitState(1).castSequence, Is.EqualTo(before.castSequence));
        }
        [Test] public void DynamicSpecialAbilityExcludesSummonAndRemovalRestoresCandidate()
        {
            var f = Create();
            f.world.AddUnit(1800, 0, "n07C", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 16, moveSpeed = 0 }, new OriginalPoint(100, -1000));
            OriginalArcherCandidate Candidate() => ((OriginalArcherCandidate[])Call(f.session, "ArcherCandidates", 1)).Single(x => x.entityId == 1800);
            Assert.That(Candidate().excludedAbility, Is.False);
            Call(f.session, "ApplyUnitAbilityOverlay", 1800, new[] { "A0K4" }, Array.Empty<string>());
            Assert.That(Candidate().excludedAbility, Is.True);
            Call(f.session, "ApplyUnitAbilityOverlay", 1800, Array.Empty<string>(), new[] { "A0K4" });
            Assert.That(Candidate().excludedAbility, Is.False);
        }
        [Test] public void StopBeforePointEffectCancelsCostCooldownAndSourceTimers()
        {
            var f = Create(); var before = f.world.UnitState(1);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15W", 700, -1000), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.1); Assert.That(f.Send(OriginalSessionCommandKind.Stop), Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(1.2);
            Assert.That(f.session.HaltReason, Is.Null);
            Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana));
            Assert.That(f.world.UnitState(1).position.x, Is.EqualTo(before.position.x));
            Assert.That(f.Ability("A15W").cooldownRemaining, Is.Zero);
        }
        [Test] public void PowerShotConsumesOnceThenBackstepsAndDamagesRetainedTargetAtFlightEnd()
        {
            var f = Create(); var before = f.world.UnitState(1);
            f.world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 24, moveSpeed = 0 }, new OriginalPoint(100, -1000));
            f.world.SetUnitState(1800, paused: true);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15W", 700, -1000), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.29); Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana));
            f.Advance(.02); Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana - 50));
            Assert.That(f.Ability("A15W").cooldownRemaining, Is.GreaterThan(15.9));
            f.Advance(.56); Assert.That(f.world.UnitState(1800).health, Is.EqualTo(1000));
            f.Advance(.4); Assert.That(f.world.UnitState(1800).health, Is.LessThan(1000));
            Assert.That(f.world.UnitState(1).position.x, Is.EqualTo(-288).Within(.001));
            Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana - 50)); Assert.That(f.session.HaltReason, Is.Null);
        }
        static object Call(OriginalSession session, string method, params object[] arguments) => typeof(OriginalSession)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(session, arguments);
        static OriginalSessionView WireSnapshot(OriginalSession session)
        {
            var codec = new OriginalUnitySessionCodec();
            var snapshot = session.Snapshot();
            var response = new OriginalNetworkResponse { kind = OriginalNetworkResponseKind.Snapshot,
                assignedSlot = 1, acknowledgedSequence = snapshot.players[0].acknowledgedSequence, snapshot = snapshot };
            Assert.That(codec.TryDecodeResponse(codec.EncodeResponse(response), out var read), Is.True);
            return read.snapshot;
        }
        static int Charges(OriginalSession session)
        {
            var state = Call(session, "Archer", 1);
            return (int)state.GetType().GetField("charges", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(state);
        }
        [Test] public void NativeHelperDamageDoesNotConsumeHeroOrbWatchButCanonicalDamageConsumesExactlyOnce()
        {
            var f = Create("A15X");
            f.world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 24 }, new OriginalPoint(100, -1000));
            f.world.SetUnitState(1800, paused: true);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.31);
            Assert.That(Charges(f.session), Is.EqualTo(5));
            Call(f.session, "OnArcherAttackStarted", f.world.UnitState(1), f.world.UnitState(1800));
            Call(f.session, "ApplyArcherEffect", 1, 1, new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.Damage, entityId = 1800, damage = 10 });
            Assert.That(Charges(f.session), Is.EqualTo(5));
            double before = f.world.UnitState(1800).health;
            Call(f.session, "ObserveArcherDamage", 1, 1800);
            Assert.That(Charges(f.session), Is.EqualTo(4)); Assert.That(f.world.UnitState(1800).health, Is.LessThan(before));
            Call(f.session, "ObserveArcherDamage", 1, 1800);
            Assert.That(Charges(f.session), Is.EqualTo(4));
        }
        [Test] public void ExpiredAttackWatchCannotConsumeAnOrbChargeOnLateDamage()
        {
            var f = Create("A15X");
            f.world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 24 }, new OriginalPoint(100, -1000));
            f.world.SetUnitState(1800, paused: true);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.31);
            Call(f.session, "OnArcherAttackStarted", f.world.UnitState(1), f.world.UnitState(1800)); f.Advance(1.01);
            Call(f.session, "ObserveArcherDamage", 1, 1800);
            Assert.That(Charges(f.session), Is.EqualTo(5));
        }
        [Test] public void ZeroDamageNativeEventConsumesOneOrbWithoutApplyingAnOuterHit()
        {
            var f = Create("A15X");
            f.world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1000, collisionRadius = 24 }, new OriginalPoint(100, -1000));
            f.world.SetUnitState(1800, paused: true);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.31);
            Call(f.session, "OnArcherAttackStarted", f.world.UnitState(1), f.world.UnitState(1800));
            f.world.DrainEvents();
            Assert.That((bool)Call(f.session, "ApplyResolvedUnitHit", 1, 1, f.world.UnitState(1800), 0d, null), Is.True);
            Assert.That(Charges(f.session), Is.EqualTo(4));
            double afterProc = f.world.UnitState(1800).health;
            Assert.That(afterProc, Is.LessThan(1000));
            Assert.That((bool)Call(f.session, "ApplyResolvedUnitHit", 1, 1, f.world.UnitState(1800), 0d, null), Is.True);
            Assert.That(f.world.UnitState(1800).health, Is.EqualTo(afterProc));
            Assert.That(Charges(f.session), Is.EqualTo(4));
            Assert.That(f.world.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.UnitDied), Is.Zero);
        }
        [TestCase(0d)] [TestCase(10d)] public void LethalNestedOrbProcDoesNotApplyTheOuterHitOrPublishAnotherDeath(double outerDamage)
        {
            var f = Create("A15X");
            f.world.AddUnit(1800, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = 1, collisionRadius = 24 }, new OriginalPoint(100, -1000));
            f.world.SetUnitState(1800, paused: true);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.31);
            Call(f.session, "OnArcherAttackStarted", f.world.UnitState(1), f.world.UnitState(1800));
            f.world.DrainEvents();
            Assert.That((bool)Call(f.session, "ApplyResolvedUnitHit", 1, 1, f.world.UnitState(1800), outerDamage, null), Is.False);
            Assert.That(f.world.UnitState(1800).health, Is.Zero); Assert.That(Charges(f.session), Is.EqualTo(4));
            Assert.That(f.world.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.UnitDied), Is.EqualTo(1));
        }
        [Test] public void ElementCyclePublishesTheNextHelperWithoutChangingTheActiveOrb()
        {
            var f = Create("A15X");
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.31);
            var before = f.session.Snapshot().players[0];
            Assert.That(before.bowElement, Is.EqualTo(1));
            Assert.That(before.auxiliaryAbilities.Single(a => a.id == "A15Z").rank, Is.EqualTo(1));
            f.Advance(18.9);
            var after = WireSnapshot(f.session).players[0];
            Assert.That(after.bowElement, Is.EqualTo(2));
            Assert.That(after.auxiliaryAbilities.Any(a => a.id == "A15Z"), Is.False);
            Assert.That(after.auxiliaryAbilities.Single(a => a.id == "A160").rank, Is.EqualTo(1));
            Assert.That(Charges(f.session), Is.EqualTo(5));
            Assert.That(f.Ability("A15X").toggledOn, Is.True);
            Assert.That(before.bowElement, Is.EqualTo(1)); // Previously published snapshot is detached.
        }
        [Test] public void LearningAnotherBowRankPreservesTheCycledHelperAndChangesItsRank()
        {
            var f = Create("A15X");
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(19.21);
            var player = ((System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(f.session))[0];
            var field = player.GetType().GetField("progression");
            var candidate = ((OriginalHeroProgression)field.GetValue(player)).Copy(); candidate.GrantExperience(700);
            var stats = (OriginalHeroStatsSnapshot)Call(f.session, "CalculateProgressionStats", "N0A0", candidate);
            Call(f.session, "ApplyProgressionProfile", player, stats, candidate);
            field.SetValue(player, candidate); player.GetType().GetField("stats").SetValue(player, stats);
            Assert.That(f.Send(OriginalSessionCommandKind.LearnSkill, "A15X"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var view = WireSnapshot(f.session).players[0];
            Assert.That(view.bowElement, Is.EqualTo(2));
            Assert.That(view.auxiliaryAbilities.Any(a => a.id == "A15Z"), Is.False);
            Assert.That(view.auxiliaryAbilities.Single(a => a.id == "A160").rank, Is.EqualTo(2));
            var state = Call(f.session, "Archer", 1);
            Assert.That((OriginalBowElement)state.GetType().GetField("next", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(state), Is.EqualTo(OriginalBowElement.Ice));
            Assert.That(f.Ability("A15X").rank, Is.EqualTo(2));
        }

        [Test] public void BerserkCommitsNativeInstantCostAndCooldownThroughThePublicCommand()
        {
            var f = Create("A0AS"); var before = f.world.UnitState(1);
            Assert.That(f.Ability("A0AS").implemented, Is.True);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A0AS"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana - 40));
            Assert.That(f.Ability("A0AS").toggledOn, Is.True);
            Assert.That(f.Ability("A0AS").cooldownRemaining, Is.EqualTo(16).Within(1e-9));
            Assert.That(WireSnapshot(f.session).players[0].abilities.Single(a => a.id == "A0AS").toggledOn, Is.True);
            Assert.That(f.Send(OriginalSessionCommandKind.Stop), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.world.UnitState(1).mana, Is.EqualTo(before.mana - 40));
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A0AS"), Is.EqualTo(OriginalSessionReplyCode.NotReady));
            Assert.That(f.session.HaltReason, Is.Null);
        }
        [Test] public void BerserkObservedNeutralDefaultsCoverThreeRanksButRejectConflictingDeclarations()
        {
            var catalog = Load<OriginalCombatCatalog>("lia39-combat");
            for (int rank = 1; rank <= 3; rank++)
            {
                var rules = new OriginalArcherCastRules(catalog, "A0AS", rank);
                Assert.That(rules.castPoint, Is.Zero);
                Assert.That(rules.attackSpeedBonus, Is.EqualTo(.5 * rank));
                Assert.That(rules.movementMultiplier, Is.EqualTo(1));
                Assert.That(rules.incomingMultiplier, Is.EqualTo(1));
                Assert.That(rules.manaCost, Is.EqualTo(25 + 15 * rank));
            }
            var ability = catalog.Ability("A0AS");
            ability.fields = ability.fields.Concat(new[] { new OriginalCombatField {
                key = "DataA1", isNumber = true, number = .1, sources = new[] { "synthetic conflicting declaration" } } }).ToArray();
            Assert.Throws<InvalidOperationException>(() => new OriginalArcherCastRules(catalog, "A0AS", 1));
        }
        [Test] public void BerserkAddsMeasuredAttackSpeedForFiveSecondsWithoutMovementPenalty()
        {
            var f = Create("A0AS"); var actor = f.world.UnitState(1);
            double baseline = (double)Call(f.session, "WeaponRate", actor);
            Assert.That(f.Send(OriginalSessionCommandKind.CastSkill, "A0AS"), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That((double)Call(f.session, "WeaponRate", f.world.UnitState(1)), Is.EqualTo(baseline + .5));
            Assert.That(f.world.UnitState(1).profile.moveSpeed, Is.EqualTo(actor.profile.moveSpeed));
            Assert.That((double)Call(f.session, "ArcherIncomingDamageMultiplier", 1), Is.EqualTo(1));
            f.Advance(4.99); Assert.That(f.Ability("A0AS").toggledOn, Is.True);
            f.Advance(.02); Assert.That(f.Ability("A0AS").toggledOn, Is.False);
            Assert.That((double)Call(f.session, "WeaponRate", f.world.UnitState(1)), Is.EqualTo(baseline));
            Assert.That(f.session.HaltReason, Is.Null);
        }
    }
}

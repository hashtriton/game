using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalNativeOnHitTests
    {
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, -1400);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double radius) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double radius) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double radius) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        static OriginalSession Create(OriginalDifficulty difficulty = OriginalDifficulty.Standard)
        {
            var s = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(difficulty), 123);
            s.ConfigureProgression(Load<OriginalNativeCatalog>("lia39-native126"), Load<OriginalObservedCatalog>("lia39-observed126"));
            s.ConfigureWorld(new Navigation(), Load<OriginalNativeCatalog>("lia39-native126"), Array.Empty<OriginalWorldDoodadView>());
            s.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = "H008" });
            s.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true });
            Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            s.DrainEvents(); return s;
        }
        static OriginalWorld World(OriginalSession s) => (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
        static void Advance(OriginalSession s, double seconds)
        { while (seconds > 1e-9) { double step = Math.Min(.01, seconds); s.Advance(step); s.DrainEvents(); seconds -= step; } Assert.That(s.HaltReason, Is.Null); }
        static object Call(OriginalSession s, string method, params object[] args) =>
            typeof(OriginalSession).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, args);
        static void ArcherBuff(OriginalSession s, int target, string ability, int rank = 1) => Call(s, "ApplyArcherNativeBuff", target,
            new OriginalArcherDebuffRules(Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalNativeCatalog>("lia39-native126"), ability, rank));
        static void Hit(OriginalSession s, int source, int target, double damage, bool melee = true, string poison = null, bool missed = false)
        {
            var type = typeof(OriginalSession).GetNestedType("Projectile", BindingFlags.NonPublic);
            var shot = Activator.CreateInstance(type, true);
            void Set(string key, object value) => type.GetField(key, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(shot, value);
            var actor = World(s).UnitState(source);
            Set("attacker", source); Set("owner", actor.ownerSlot); Set("target", target); Set("kind", OriginalWorldTargetKind.Unit);
            Set("attackType", "normal"); Set("damage", damage); Set("melee", melee); Set("poisonAbility", poison);
            Set("missed", missed);
            typeof(OriginalSession).GetMethod("ApplyWeaponHit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new[] { shot });
        }
        static void Enemy(OriginalSession s, string rawcode = "n008", double hp = 1000)
        {
            var w = World(s); w.AddUnit(1001, 0, rawcode, new OriginalWorldUnitProfile { maxHealth = hp, moveSpeed = 270, collisionRadius = 24 }, new OriginalPoint(100, -1400));
            w.SetUnitState(1001, paused: true);
        }
        [Test] public void ThornsMatchesActualNormalAndHeroReflectionWithoutNumericArmor()
        {
            var c = Load<OriginalCombatCatalog>("lia39-combat"); var n = Load<OriginalNativeCatalog>("lia39-native126");
            var rules = new OriginalThornsRules(c, "A05T", 1);
            Assert.That(rules.ReflectedDamage(n, 13, "small"), Is.EqualTo(3.25));
            Assert.That(rules.ReflectedDamage(n, 12, "large"), Is.EqualTo(3));
            Assert.That(rules.ReflectedDamage(n, 57, "hero"), Is.EqualTo(11.4).Within(1e-10));
        }
        [Test] public void MissingNormalPoisonDamageIsResolvedByExactNativeObservationOnly()
        {
            var c = Load<OriginalCombatCatalog>("lia39-combat");
            var normal = new OriginalPoisonRules(c, "A0TC", 1); var hard = new OriginalPoisonRules(c, "A0TD", 1);
            Assert.That(normal.damagePerTick, Is.Zero); Assert.That(normal.duration, Is.EqualTo(2)); Assert.That(normal.moveSlow, Is.EqualTo(.1));
            Assert.That(hard.damagePerTick, Is.EqualTo(10)); Assert.That(hard.heroDuration, Is.EqualTo(4)); Assert.That(hard.attackSlow, Is.EqualTo(.2));
            var field = c.Ability("A0TC").fields.First(f => f.key == "DataB1"); field.conflict = true;
            Assert.Throws<InvalidOperationException>(() => new OriginalPoisonRules(c, "A0TC", 1));
        }
        [Test] public void DefendAndPoisonSubtractFromTheSameBaseSpeedAsNativeLateOrderControls()
        {
            foreach (string poison in new[] { "A0TC", "A0TD" })
            foreach (bool shieldFirst in new[] { false, true })
            {
                var s = Create(); Enemy(s); Advance(s, 2.05); var w = World(s);
                var player = ((System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s))[0];
                var progressionField = player.GetType().GetField("progression");
                var candidate = ((OriginalHeroProgression)progressionField.GetValue(player)).Copy(); candidate.GrantExperience(200);
                var stats = typeof(OriginalSession).GetMethod("CalculateProgressionStats", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { "H008", candidate });
                typeof(OriginalSession).GetMethod("ApplyProgressionProfile", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { player, stats, candidate });
                progressionField.SetValue(player, candidate); player.GetType().GetField("stats").SetValue(player, stats);
                Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 4, kind = OriginalSessionCommandKind.LearnSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                if (!shieldFirst) Hit(s, 1001, 1, 1, false, poison);
                Assert.That(s.Apply(0, new OriginalSessionCommand { sequence = 5, kind = OriginalSessionCommandKind.CastSkill, skillId = "A05M" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
                if (shieldFirst) Hit(s, 1001, 1, 1, false, poison);
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(poison == "A0TC" ? 150 : 125).Within(1e-8));
                typeof(OriginalSession).GetMethod("AddPyroChainBuff", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { w.UnitState(1), w.Clock + 3 });
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(poison == "A0TC" ? 87.5 : 62.5).Within(1e-8));
                ArcherBuff(s,1,"A168");
                Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(1),"CAPS3 measured native combined-slow minimum.");
                Call(s,"RemoveArcherDebuffs",1);
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(poison == "A0TC" ? 87.5 : 62.5).Within(1e-8));
                typeof(OriginalSession).GetMethod("RemovePoison", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { 1 });
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(112.5).Within(1e-8));
                typeof(OriginalSession).GetMethod("RemovePyroChainBuff", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { 1 });
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(175).Within(1e-8));
            }
        }
        [Test] public void SparseB06LPoisonsUseMeasuredZeroDamageAndTheirOwnDurations()
        {
            var c = Load<OriginalCombatCatalog>("lia39-combat");
            foreach (string id in new[] { "A0TE", "A0TF" })
            {
                var rule = new OriginalPoisonRules(c, id, 1);
                Assert.That(rule.buffId, Is.EqualTo("B06L")); Assert.That(rule.damagePerTick, Is.Zero);
                Assert.That(rule.duration, Is.EqualTo(id == "A0TE" ? 1.5 : 3));
                var s = Create(); Enemy(s); Hit(s, 1001, 1, 1, false, id); var w = World(s);
                double before = w.UnitState(1).health;
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(id == "A0TE" ? 200 : 212.5));
                Advance(s, 1.01); Assert.That(w.UnitState(1).health, Is.EqualTo(before));
            }
        }
        [Test] public void EnemyAuraAndPoisonComposeFromOneBaselineRegardlessOfApplyAndRemovalOrder()
        {
            foreach(bool auraFirst in new[]{false,true})
            foreach(bool removeAuraFirst in new[]{false,true})
            {
                var s=Create(); Enemy(s,"hfoo");Advance(s,2.05);var w=World(s);
                void Aura()=>typeof(OriginalSession).GetMethod("AddPyroChainBuff",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(s,new object[]{w.UnitState(1001),w.Clock+3});
                void Remove(string name)=>typeof(OriginalSession).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(s,new object[]{1001});
                if(auraFirst)Aura();
                Hit(s,1,1001,1,false,"A0TC");
                if(!auraFirst)Aura();
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270*.65).Within(1e-8));
                Remove(removeAuraFirst?"RemovePyroChainBuff":"RemovePoison");
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270*(removeAuraFirst?.9:.75)).Within(1e-8));
                Remove(removeAuraFirst?"RemovePoison":"RemovePyroChainBuff");
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270).Within(1e-8));
            }
        }
        [Test] public void ThreeMovementDebuffsRestoreOneBaselineForEveryApplicationAndRemovalOrder()
        {
            int[][] orders = {new[]{0,1,2},new[]{0,2,1},new[]{1,0,2},new[]{1,2,0},new[]{2,0,1},new[]{2,1,0}};
            foreach(var apply in orders)
            foreach(var remove in orders)
            {
                var s=Create(); Enemy(s,"hfoo"); var w=World(s); var active=new bool[3];
                foreach(int index in apply)
                {
                    if(index==0) Hit(s,1,1001,1,false,"A0TC");
                    else if(index==1) Call(s,"AddPyroChainBuff",w.UnitState(1001),w.Clock+3);
                    else ArcherBuff(s,1001,"A168");
                    active[index]=true;
                }
                Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270*.25).Within(1e-8));
                foreach(int index in remove)
                {
                    Call(s,index==0?"RemovePoison":index==1?"RemovePyroChainBuff":"RemoveArcherDebuffs",1001);
                    active[index]=false;
                    double factor=1-(active[0]?.1:0)-(active[1]?.25:0)-(active[2]?.4:0);
                    Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270*factor).Within(1e-8));
                }
            }
        }
        [Test] public void AllStatusesExpiringDuringRingForceRestoreTheCapturedNonzeroBaseline()
        {
            var s=Create(); Enemy(s,"hfoo");var w=World(s);
            Call(s,"CaptureAbilityMovementBase",1001);
            var ring=typeof(OriginalSession).GetNestedType("RingForce",BindingFlags.NonPublic);
            var forces=(System.Collections.IDictionary)typeof(OriginalSession).GetField("ringForces",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(s);
            forces.Add(1001,Activator.CreateInstance(ring,true));
            var actor=w.UnitState(1001);var profile=actor.profile;profile.moveSpeed=0;
            Assert.That(w.UpdateProfile(1001,profile,actor.health,actor.mana),Is.True);
            Hit(s,1,1001,1,false,"A0TC");ArcherBuff(s,1001,"A168");Call(s,"AddPyroChainBuff",w.UnitState(1001),w.Clock+3);
            Call(s,"RemoveArcherDebuffs",1001);Call(s,"RemovePoison",1001);Call(s,"RemovePyroChainBuff",1001);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.Zero);
            Call(s,"FinishRingForce",1001);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
            ArcherBuff(s,1001,"A168");Call(s,"RemoveArcherDebuffs",1001);
            Assert.That(w.UnitState(1001).profile.moveSpeed,Is.EqualTo(270));
        }
        [Test] public void StackedAuthoredAttackSlowsUseNativeMeasuredPointTwoFloor()
        {
            var s=Create();Enemy(s,"hfoo");var w=World(s);
            Hit(s,1,1001,1,false,"A0TD");ArcherBuff(s,1001,"A168");ArcherBuff(s,1001,"A165",3);
            Assert.That((double)Call(s,"WeaponRate",w.UnitState(1001)),Is.EqualTo(.2));
            w.SetUnitState(1001,paused:false);w.TryAttackTarget(1001,OriginalWorldTargetKind.Unit,1);
            Advance(s,.05);Assert.That(w.UnitState(1001).attackSequence,Is.EqualTo(1));
            Assert.That(w.UnitState(1).health,Is.EqualTo(631));
        }
        [Test] public void NativeHighAgilityCapControlsBothCycleAndActualWeaponWindup()
        {
            foreach(double bonus in new[]{6.0,10.0})
            {
                var s=Create();Advance(s,2.05);Enemy(s,"hfoo",10000);var w=World(s);
                var player=((System.Collections.IList)typeof(OriginalSession).GetField("players",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(s))[0];
                var stats=(OriginalHeroStatsSnapshot)player.GetType().GetField("stats").GetValue(player);
                stats.agilityAttackSpeedBonus=new OriginalHeroStatValue{known=true,value=bonus};
                Assert.That((double)Call(s,"WeaponRate",w.UnitState(1)),Is.EqualTo(5));
                w.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001);Advance(s,.12);
                Assert.That(w.UnitState(1001).health,Is.LessThan(10000));Assert.That(w.UnitState(1).attackSequence,Is.EqualTo(1));
                Advance(s,.28);Assert.That(w.UnitState(1).attackSequence,Is.EqualTo(2));
            }
        }
        [Test] public void CapturedMissBypassesDamageReflectionPoisonAndResourceEvents()
        {
            var s=Create();Enemy(s);var w=World(s);s.DrainEvents();w.DrainEvents();
            Hit(s,1001,1,13,true,"A0TD",missed:true);
            Assert.That(w.UnitState(1).health,Is.EqualTo(631));Assert.That(w.UnitState(1001).health,Is.EqualTo(1000));
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));Assert.That(w.DrainEvents(),Is.Empty);Assert.That(s.DrainEvents(),Is.Empty);
            var field=typeof(OriginalSession).GetField("weaponRandom",BindingFlags.NonPublic|BindingFlags.Instance);
            field.SetValue(s,123u);Assert.That((bool)Call(s,"RollWeaponMiss",1),Is.False);Assert.That(field.GetValue(s),Is.EqualTo(123u));
        }
        [Test] public void MeleeHitReflectsBeforeIncomingWhileRangedHitNeverReflects()
        {
            var s = Create(); Enemy(s); var w = World(s); Hit(s, 1001, 1, 13);
            Assert.That(w.UnitState(1001).health, Is.EqualTo(996.75));
            Assert.That(w.UnitState(1).health, Is.EqualTo(631 - 13 / 1.312).Within(1e-8));
            Hit(s, 1001, 1, 13, false);
            Assert.That(w.UnitState(1001).health, Is.EqualTo(996.75));
        }
        [Test] public void ReflectedLethalHitDoesNotCancelAlreadyReleasedAttackOrRecurse()
        {
            var s = Create(); Enemy(s, hp: 2); var w = World(s); Hit(s, 1001, 1, 13);
            Assert.That(w.UnitState(1001).health, Is.Zero);
            Assert.That(w.UnitState(1).health, Is.EqualTo(631 - 13 / 1.312).Within(1e-8));
            Assert.That(w.DrainEvents().Count(e => e.kind == OriginalWorldEventKind.UnitDied && e.entityId == 1001), Is.EqualTo(1));
        }
        [Test] public void PoisonRefreshPreservesTickPhaseAndExpiryRestoresBaseline()
        {
            var s = Create(OriginalDifficulty.Nightmare); Enemy(s); var w = World(s);
            Hit(s, 1001, 1, 1, false, "A0TD"); double start = w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(200)); Advance(s, .01);
            Assert.That(w.UnitState(1).health, Is.EqualTo(start - 8).Within(1e-8));
            Advance(s, .49); Hit(s, 1001, 1, 1, false, "A0TD"); double refreshed = w.UnitState(1).health;
            Advance(s, .50); Assert.That(w.UnitState(1).health, Is.EqualTo(refreshed));
            Advance(s, .01); Assert.That(w.UnitState(1).health, Is.EqualTo(refreshed - 8).Within(1e-8));
            Advance(s, 3.49); Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
        }
        [Test] public void WeakerLatestPoisonChangesSlowButKeepsStrongDamageAndLongestExpiry()
        {
            var s=Create();Enemy(s);var w=World(s);
            Hit(s,1001,1,1,false,"A0TD");Advance(s,.5);
            Hit(s,1001,1,1,false,"A0TC");double hp=w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(225));
            Advance(s,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(hp));
            Advance(s,.01);Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-8));
            Advance(s,1.99);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(225));
            Advance(s,1);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void StrongerLatestPoisonKeepsOriginalPhaseAndStartsFreshOnlyAfterExpiry()
        {
            var s=Create();Enemy(s);var w=World(s);
            Hit(s,1001,1,1,false,"A0TC");Advance(s,.5);Hit(s,1001,1,1,false,"A0TD");double hp=w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(200));
            Advance(s,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Advance(s,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-8));Advance(s,3.49);
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
            Hit(s,1001,1,1,false,"A0TD");hp=w.UnitState(1).health;Advance(s,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-8));
        }
        [Test] public void SparseSharedBuffUsesLatestSlowWithoutShorteningEarlierLongerDuration()
        {
            var s=Create();Enemy(s);var w=World(s);
            Hit(s,1001,1,1,false,"A0TF");Advance(s,.5);Hit(s,1001,1,1,false,"A0TE");double hp=w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(200));Advance(s,2);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(200));
            Advance(s,.5);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void NormalPoisonSlowsWithoutInventingDamageAndDispelClearsMovementPenalty()
        {
            var s = Create(); Enemy(s); var w = World(s); Hit(s, 1001, 1, 1, false, "A0TC"); double hp = w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(225)); Advance(s, 1);
            Assert.That(w.UnitState(1).health, Is.EqualTo(hp));
            typeof(OriginalSession).GetMethod("RemoveDispellableAbilityBuffs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { 1 });
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(250));
        }
        [Test] public void RealEnemyWeaponAppliesTheDifficultySelectedAvenRule()
        {
            foreach (var difficulty in new[] { OriginalDifficulty.Standard, OriginalDifficulty.Extreme, OriginalDifficulty.Nightmare })
            {
                var s = Create(difficulty); Enemy(s); var w = World(s); Call(s, "ApplyEnemyDifficultyAbilities", w.UnitState(1001)); w.SetUnitState(1001, paused: false);
                w.TryAttackTarget(1001, OriginalWorldTargetKind.Unit, 1); Advance(s, .60);
                Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(difficulty == OriginalDifficulty.Standard ? 225 : 200));
                Assert.That(w.UnitState(1001).health, Is.LessThan(1000));
            }
        }
        [Test] public void PoisonChangesRemainingAttackFractionAndSubsequentCadence()
        {
            var s = Create(); Enemy(s, "hfoo", 10000); var w = World(s); w.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001);
            Advance(s, .20); Hit(s, 1001, 1, 1, false, "A0TC");
            var cycles = (System.Collections.IDictionary)typeof(OriginalSession).GetField("weaponCycles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s);
            Advance(s, .01); var cycle = cycles[1]; double next = (double)cycle.GetType().GetField("nextAttack", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cycle);
            // First attack began at .01. Poison arrives at .20; only the remaining fraction is rescaled.
            Assert.That(next, Is.EqualTo(.20 + (.01 + 1.85 / 1.06 - .20) * 1.06 / .96).Within(1e-8));
            Assert.That(w.UnitState(1).profile.moveSpeed, Is.EqualTo(225));
        }
        [Test] public void LaterWaveMeasuredPoisonFamiliesDoNotBreakTheirExistingBasicWeapon()
        {
            foreach (string rawcode in new[] { "n03B", "n0AC", "n03C" })
            {
                var s = Create(); Enemy(s, rawcode); var w = World(s); w.SetUnitState(1001, paused: false);
                w.TryAttackTarget(1001, OriginalWorldTargetKind.Unit, 1); Advance(s, 2);
                Assert.That(w.UnitState(1).health, Is.LessThan(631), rawcode);
                Assert.That(w.UnitState(1001).attackSequence, Is.GreaterThan(0), rawcode);
            }
        }
        [Test] public void AllyThornsAuraIncludesItsAuthoredSelfTarget()
        {
            var s = Create(); Enemy(s, "n00E"); var w = World(s);
            Call(s,"ApplyUnitAbilityOverlay",1001,Array.Empty<string>(),new[]{"A15F"}); Hit(s, 1, 1001, 13);
            Assert.That(w.UnitState(1001).health, Is.LessThan(1000));
            Assert.That(w.UnitState(1).health, Is.EqualTo(631-13*.2*.8).Within(1e-7));
        }
        [Test] public void WeakerCrossBuffPoisonChangesSlowButCannotExtendStrongPayloadExpiry()
        {
            var s=Create();Enemy(s);var w=World(s);
            Hit(s,1001,1,1,false,"A0TD");Advance(s,1.5);Hit(s,1001,1,1,false,"A0TF");double hp=w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(212.5));
            Advance(s,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Advance(s,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-7));
            Advance(s,1.99);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250),"TF must not extend TD to4.5s.");
        }
        [Test] public void StrongerCrossBuffPoisonReplacesPayloadWhileRetainingItsOriginalTickPhase()
        {
            var s=Create();Enemy(s);var w=World(s);
            Hit(s,1001,1,1,false,"A0TF");Advance(s,1.5);Hit(s,1001,1,1,false,"A0TD");double hp=w.UnitState(1).health;
            Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(200));
            Advance(s,.5);Assert.That(w.UnitState(1).health,Is.EqualTo(hp));Advance(s,.01);
            Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-7));
            Advance(s,3.49);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
        }
        [Test] public void EqualCrossBuffPoisonRemainsOneSlowAndReapplyAfterExpiryStartsANewPhase()
        {
            foreach(bool reverse in new[]{false,true})
            {
                var s=Create();Enemy(s);var w=World(s);
                Hit(s,1001,1,1,false,reverse?"A0TE":"A0TC");Advance(s,.5);
                Hit(s,1001,1,1,false,reverse?"A0TC":"A0TE");
                Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(reverse?225:200));
                Advance(s,2);Assert.That(w.UnitState(1).profile.moveSpeed,Is.EqualTo(250));
                Hit(s,1001,1,1,false,"A0TD");double hp=w.UnitState(1).health;Advance(s,.01);
                Assert.That(w.UnitState(1).health,Is.EqualTo(hp-8).Within(1e-7));
            }
        }
    }
}

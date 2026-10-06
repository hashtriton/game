using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionWeaponTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(135, 1000);
            public long NavigationRevision { get; private set; }
            public int destructionCalls;
            public bool IsWalkable(double x, double y, double radius) => Math.Abs(x) <= 4096 && Math.Abs(y) <= 4096;
            public bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius) => IsWalkable(to.x, to.y, radius);
            public OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius) => new[] { to };
            public bool SetDoodadAlive(int editorId, bool alive)
            { NavigationRevision++; if (!alive) destructionCalls++; return true; }
        }

        sealed class Fixture
        {
            internal OriginalSession session;
            internal OriginalCombatCatalog combat;
            internal Navigation navigation;
            internal OriginalWorld world;
            internal long sequence = 3;
            internal OriginalWorldSnapshot Snapshot => session.Snapshot().world;
            internal OriginalWorldUnitView Hero => Snapshot.units.Single(u => u.ownerSlot == 1);
            internal double BarrelHealth => Snapshot.doodads.Single().health;
            internal OriginalSessionReplyCode Command(OriginalSessionCommandKind kind, OriginalWorldTargetKind targetKind = OriginalWorldTargetKind.None, int target = 0) =>
                session.Apply(0, new OriginalSessionCommand { sequence = ++sequence, kind = kind, targetKind = targetKind, targetId = target, x = 700, y = 1000 });
            internal void Advance(double seconds)
            {
                while (seconds > 1e-9)
                {
                    double step = Math.Min(.05, seconds); session.Advance(step); session.DrainEvents(); seconds -= step;
                }
            }
        }

        static T Load<T>(string file) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + file)));

        [Test] public void FinalBossLevelFiftyAttributesAffectRealWeaponWindupAndReleasedDamage()
        {
            var f = Create(distance: 1000, observed: true);
            var hero = f.world.UnitState(1);
            var profile = new OriginalWorldUnitProfile { maxHealth = 10000, maxMana = hero.profile.maxMana,
                moveSpeed = hero.profile.moveSpeed, collisionRadius = hero.profile.collisionRadius };
            Assert.That(f.world.UpdateProfile(1, profile, 10000, hero.mana), Is.True);
            f.world.SetUnitState(1, paused: true);
            f.world.AddUnit(1900, 0, "O006", new OriginalWorldUnitProfile {
                maxHealth = 30000, maxMana = 2500, moveSpeed = 0, collisionRadius = 32 }, new OriginalPoint(235, 1000));
            // Isolate the attribute/IAS contract; native bash and feedback
            // now have their own complete release/impact regression suite.
            typeof(OriginalSession).GetMethod("ApplyUnitAbilityOverlay",BindingFlags.NonPublic|BindingFlags.Instance)
                .Invoke(f.session,new object[]{1900,Array.Empty<string>(),new[]{"A0EW","A0EX","A0Y9"}});
            Assert.That(f.world.TryAttackTarget(1900, OriginalWorldTargetKind.Unit, 1), Is.True);
            f.Advance(.1);
            Assert.That(f.session.HaltReason, Is.Null);
            Assert.That(f.world.UnitState(1900).attackSequence, Is.EqualTo(1));
            Assert.That(f.world.UnitState(1).health, Is.EqualTo(10000));
            f.Advance(.05);
            Assert.That(f.session.HaltReason, Is.Null);
            // Native SPARSE20 O006L50 attributes250 plus authored SCae .75:
            // IAS+2.5+.75 gives .33/4.25
            // windup; primary AGI damage adds250*mapStrAttackBonus1.5=375.
            var d = f.combat.Unit("O006");
            double min = d.Number("dmgplus1") + d.Number("dice1") + 375;
            double max = d.Number("dmgplus1") + d.Number("dice1") * d.Number("sides1") + 375;
            var native = Load<OriginalNativeCatalog>("lia39-native126.json");
            double multiplier = OriginalAttackRules.DamageTypeMultiplier(native, "hero", f.combat.Unit("H008").Text("defType")) *
                OriginalAttackRules.ArmorMultiplier(native, OriginalHeroStats.Calculate(f.combat, "H008", 1).armor.Require());
            double actual = 10000 - f.world.UnitState(1).health;
            Assert.That(actual, Is.InRange(min * multiplier, max * multiplier));
        }

        [Test] public void CocoonAcquisitionDoesNotInventAWeaponOrAttackNearbyBarrel()
        {
            var f = Create(distance: 100);
            f.world.SetUnitState(1, paused: true, invulnerable: false);
            Assert.That(f.combat.Unit("u00L").Number("acquire"), Is.EqualTo(500));
            Assert.That(f.combat.Unit("u00L").Text("weapTp1"), Is.EqualTo("_"));
            f.world.AddUnit(1900, 0, "u00L", new OriginalWorldUnitProfile { maxHealth = 100, maxMana = 0, moveSpeed = 0, collisionRadius = 16 }, new OriginalPoint(200, 1000));
            f.Advance(7.1);
            Assert.That(f.session.HaltReason, Is.Null);
            Assert.That(f.world.UnitState(1900).order, Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(f.world.UnitState(1900).attackSequence, Is.Zero);
            Assert.That(f.BarrelHealth, Is.EqualTo(10));
            // A trusted external order cannot make an absent native weapon fire.
            f.world.TryAttackTarget(1900, OriginalWorldTargetKind.Unit, 1);
            f.Advance(.5);
            Assert.That(f.session.HaltReason, Is.Null);
            Assert.That(f.world.UnitState(1900).attackSequence, Is.Zero);
        }
        [Test] public void ExplicitSecondWeaponUsesItsOwnRangeWindupDamageAndCooldown()
        {
            foreach(double distance in new[]{65d,300d})
            {
                var f=Create(distance:1000);f.world.SetUnitState(1,paused:true);
                f.world.AddUnit(1900,0,"n01R",new OriginalWorldUnitProfile{maxHealth=900,maxMana=300,collisionRadius=24},new OriginalPoint(135+distance,1000));
                f.world.HoldPosition(1900);f.world.TryAttackTarget(1900,OriginalWorldTargetKind.Unit,1,preserveHolding:true);
                double hp=f.world.UnitState(1).health;f.Advance(.3);
                Assert.That(f.session.HaltReason,Is.Null);Assert.That(f.world.UnitState(1).health,Is.EqualTo(hp));
                if(distance==300){Assert.That(f.world.UnitState(1900).attackSequence,Is.Zero);continue;}
                f.Advance(.05);double damage=hp-f.world.UnitState(1).health;
                var native=Load<OriginalNativeCatalog>("lia39-native126.json");
                double armor=OriginalHeroStats.Calculate(f.combat,"H008",1).armor.Require();
                double factor=OriginalAttackRules.ArmorMultiplier(native,armor);
                Assert.That(damage,Is.InRange(61*factor,66*factor));
                Assert.That(f.world.UnitState(1900).attackSequence,Is.EqualTo(1));
                f.Advance(.6);Assert.That(f.world.UnitState(1900).attackSequence,Is.EqualTo(2));
            }
        }
        [Test] public void NativeSparseSummonArmorZeroStillComposesTemporaryArmor()
        {
            var f=Create();f.world.AddUnit(1900,0,"n026",new OriginalWorldUnitProfile{maxHealth=210,collisionRadius=24},new OriginalPoint(500,1000));
            var method=typeof(OriginalSession).GetMethod("EnemyArmor",BindingFlags.NonPublic|BindingFlags.Instance);
            Assert.That(method.Invoke(f.session,new object[]{f.combat.Unit("n026"),1900}),Is.EqualTo(0d));
            // Source-derived AIcb is independently tested. The sparse baseline
            // must not erase its current per-entity contribution.
            typeof(OriginalSession).GetMethod("ApplyUnitAbilityOverlay",BindingFlags.NonPublic|BindingFlags.Instance)
                .Invoke(f.session,new object[]{1,new[]{"A0QZ"},Array.Empty<string>()});
            typeof(OriginalSession).GetMethod("ApplyNativeCorruption",BindingFlags.NonPublic|BindingFlags.Instance)
                .Invoke(f.session,new object[]{f.world.UnitState(1),f.world.UnitState(1900)});
            Assert.That(method.Invoke(f.session,new object[]{f.combat.Unit("n026"),1900}),Is.EqualTo(-10d));
        }
        [Test] public void DualWeaponSelectsGroundMeleeAndAirSplashWithDeclaredRings()
        {
            foreach(bool air in new[]{false,true})
            {
                var f=Create(distance:1000);f.world.SetUnitState(1,paused:true);
                const int attacker=OriginalWorld.FirstSummonEntityId;
                Assert.That(f.world.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=attacker,ownerSlot=1,sourceHeroEntityId=1,rawcode="n02K",
                    profile=new OriginalWorldUnitProfile{maxHealth=2500,collisionRadius=24},health=2500,position=new OriginalPoint(1000,1000)}}),Is.True);
                f.world.AddUnit(1901,0,air?"hdhw":"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=16},new OriginalPoint(1100,1000));
                foreach(int id in new[]{1902,1903,1904,1905})
                    f.world.AddUnit(id,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=8},new OriginalPoint(1100,1000+(id==1902?40:id==1903?100:id==1904?200:260)));
                foreach(int id in new[]{1901,1902,1903,1904,1905})f.world.SetUnitState(id,paused:true);
                f.world.TryAttackTarget(attacker,OriginalWorldTargetKind.Unit,1901);
                f.Advance(.6);
                if(air)Assert.That(f.world.UnitState(1901).health,Is.EqualTo(10000),"Projectile should still be in flight.");
                f.Advance(.1);Assert.That(f.session.HaltReason,Is.Null);
                double primary=10000-f.world.UnitState(1901).health;
                double raw=primary*(air?1.06:1.12);
                Assert.That(raw,Is.InRange(air?141:131,air?148:138));
                foreach(int id in new[]{1902,1903,1904,1905})
                {
                    double factor=!air||id==1905?0:id==1902?1:id==1903?.5:.25;
                    Assert.That(10000-f.world.UnitState(id).health,Is.EqualTo(raw*factor/1.12).Within(.0001));
                }
            }
        }
        [Test] public void ArtilleryHitsItsReleasedPointAfterTargetMovesWithDeclaredSplashRings()
        {
            var f=Create(distance:1000);f.world.SetUnitState(1,paused:true);
            const int attacker=OriginalWorld.FirstSummonEntityId;
            Assert.That(f.world.TryPublishSummons(new[]{new OriginalWorldSummonSpawn{entityId=attacker,ownerSlot=1,sourceHeroEntityId=1,rawcode="o00C",
                profile=new OriginalWorldUnitProfile{maxHealth=2500,collisionRadius=24},health=2500,position=new OriginalPoint(1000,1000)}}),Is.True);
            foreach(var row in new[]{(1901,"hfoo",1300),(1902,"hfoo",1340),(1903,"hfoo",1420),(1904,"hdhw",1310)})
            {
                f.world.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=10000,collisionRadius=4},new OriginalPoint(row.Item3,1000));
                f.world.SetUnitState(row.Item1,paused:true);
            }
            f.world.TryAttackTarget(attacker,OriginalWorldTargetKind.Unit,1901);f.Advance(.2);
            Assert.That(f.world.UnitState(1901).health,Is.EqualTo(10000));
            Assert.That(f.world.Relocate(1901,new OriginalPoint(1700,1000)),Is.True);
            f.Advance(.3);Assert.That(f.session.HaltReason,Is.Null);
            Assert.That(f.world.UnitState(1901).health,Is.EqualTo(10000));
            Assert.That(f.world.UnitState(1904).health,Is.EqualTo(10000));
            double half=10000-f.world.UnitState(1902).health,quarter=10000-f.world.UnitState(1903).health;
            Assert.That(half,Is.GreaterThan(0));Assert.That(quarter/half,Is.EqualTo(.25/.4).Within(1e-7));
        }
        static Fixture Create(string hero = "H008", double distance = 100, bool observed = false)
        {
            var f = new Fixture { combat = Load<OriginalCombatCatalog>("lia39-combat.json"), navigation = new Navigation() };
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match.json"), Load<OriginalItemCatalog>("lia39-items.json"),
                f.combat, Load<OriginalDuelCatalog>("lia39-duels.json"), new string('a', 64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            f.session.ConfigureWorld(f.navigation, Load<OriginalNativeCatalog>("lia39-native126.json"), new[] {
                new OriginalWorldDoodadView { editorId = 686, rawcode = "LTbr", position = new OriginalPoint(135 + distance, 1000), maxHealth = 10, health = 10 } },
                observed ? Load<OriginalObservedCatalog>("lia39-observed126.json") : null);
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 1, kind = OriginalSessionCommandKind.SelectHero, heroId = hero }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 2, kind = OriginalSessionCommandKind.LobbyReady, ready = true }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence = 3, kind = OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.session.DrainEvents();
            // Test seam for trusted world observations. Network clients have no
            // route to this object or to direct damage/state mutators.
            f.world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.session);
            return f;
        }

        static double Windup(Fixture f) => f.combat.Unit(f.Hero.rawcode).Number("dmgpt1") /
            (1 + OriginalHeroStats.Calculate(f.combat, f.Hero.rawcode, 1).agilityAttackSpeedBonus.Require());

        [Test] public void WindWalkArmsAtAttackAndCapturesOneFlatBonusInTheReleasedMissile()
        {
            foreach(bool explicitOrder in new[]{false,true})
            {
                var released=new double[2];var losses=new double[2];
                for(int treatment=0;treatment<2;treatment++)
                {
                    var f=Create("H024",1000);var flags=BindingFlags.Instance|BindingFlags.NonPublic;
                    object Call(string name,params object[] args)=>typeof(OriginalSession).GetMethod(name,flags).Invoke(f.session,args);
                    f.world.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{maxHealth=2000,collisionRadius=24},new OriginalPoint(435,1000));
                    f.world.SetUnitState(1001,paused:true);
                    Call("OrderShieldCripple",0,1);
                    if(treatment==1)
                    {
                        var rule=Call("WindWalkItemRule","I0AP");Call("ApplyNativeItemStatus",1,rule);Call("AdvanceItemStatuses",.31d);
                        Assert.That(Call("ItemInvisibilityActive",1),Is.True);
                    }
                    if(explicitOrder)Assert.That(f.Command(OriginalSessionCommandKind.AttackTarget,OriginalWorldTargetKind.Unit,1001),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                    else Assert.That(f.world.TryAttackTarget(1,OriginalWorldTargetKind.Unit,1001),Is.True);
                    var shots=(System.Collections.IList)typeof(OriginalSession).GetField("projectiles",flags).GetValue(f.session);
                    for(int i=0;i<200&&shots.Count==0;i++)f.Advance(.01);
                    Assert.That(f.session.HaltReason,Is.Null);Assert.That(shots.Count,Is.EqualTo(1));
                    released[treatment]=(double)shots[0].GetType().GetField("damage",flags).GetValue(shots[0]);
                    Assert.That(Call("ItemInvisibilityActive",1),Is.False);
                    Assert.That(Call("ConsumeItemWindWalkStrike",1),Is.Zero,"The released shot consumes the armed bonus exactly once.");
                    f.Command(OriginalSessionCommandKind.Stop);f.world.SetUnitState(1,paused:true);f.Advance(1);
                    losses[treatment]=2000-f.world.UnitState(1001).health;
                }
                Assert.That(released[1]-released[0],Is.EqualTo(50).Within(.000001),"Cripple does not halve the Wind Walk flat bonus.");
                var native=Load<OriginalNativeCatalog>("lia39-native126.json");
                double factor=OriginalAttackRules.ArmorMultiplier(native,2)*OriginalAttackRules.DamageTypeMultiplier(native,"hero","large");
                Assert.That(losses[1]-losses[0],Is.EqualTo(50*factor).Within(.000001),"Stop after release preserves the captured bonus through impact.");
            }
        }

        [Test] public void NativeEnsnareRaisesOnlyMeleeRangeToItsDeclared128Floor()
        {
            foreach(bool rooted in new[]{false,true})
            foreach(double distance in new[]{175.9,176.1})
            {
                var f=Create(distance:1000,observed:true);
                f.world.SetUnitState(1,paused:true,invulnerable:false);
                f.world.AddUnit(1001,0,"n008",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=24},new OriginalPoint(135+distance,1000));
                if(rooted) typeof(OriginalSession).GetMethod("AddTimedNativeRoot",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(f.session,new object[]{1001,1,5.0});
                f.world.HoldPosition(1001);
                f.world.TryAttackTarget(1001,OriginalWorldTargetKind.Unit,1,preserveHolding:true);
                double before=f.world.UnitState(1).health;f.Advance(1);
                Assert.That(f.session.HaltReason,Is.Null);
                Assert.That(f.world.UnitState(1001).position.x,Is.EqualTo(135+distance));
                Assert.That(f.world.UnitState(1).health,rooted&&distance<176?Is.LessThan(before):Is.EqualTo(before));
            }
        }

        [Test]
        public void UnitAttackUsesBothCollisionRadiiAtTheMeasuredNativeBoundary()
        {
            // RANGE_BOUNDARY2: H008->n008 hits at 167.8999; n008->H008
            // hits at 147.8999. The +.1 row moves before hitting, so the
            // stationary +.1 rejection below tests the inferred boundary.
            foreach (bool enemyAttacks in new[] { false, true })
                foreach (double offset in new[] { -.1, .1 })
                {
                    var f = Create(distance: 1000, observed: true);
                    double range = f.combat.Unit(enemyAttacks ? "n008" : "H008").Number("rangeN1") + 48;
                    f.world.AddUnit(1001, 0, "n008", new OriginalWorldUnitProfile {
                        collisionRadius = 24, maxHealth = 1000, moveSpeed = 0 }, new OriginalPoint(135 + range + offset, 1000));
                    int attacker = enemyAttacks ? 1001 : 1, victim = enemyAttacks ? 1 : 1001;
                    f.world.SetUnitState(victim, paused: true);
                    if (!enemyAttacks) f.world.HoldPosition(attacker);
                    Assert.That(f.world.TryAttackTarget(attacker, OriginalWorldTargetKind.Unit, victim, preserveHolding: true), Is.True);
                    double health = f.world.UnitState(victim).health;
                    f.Advance(1);
                    Assert.That(f.session.HaltReason, Is.Null);
                    if (offset < 0)
                    {
                        Assert.That(f.world.UnitState(attacker).attackSequence, Is.GreaterThan(0));
                        Assert.That(f.world.UnitState(victim).health, Is.LessThan(health));
                    }
                    else Assert.That(f.world.UnitState(attacker).attackSequence, Is.Zero);
                    Assert.That(f.world.UnitState(attacker).position.x, Is.EqualTo(enemyAttacks ? 135 + range + offset : 135));
                }
        }

        [Test]
        public void UnitTargetLeavingCollisionExtendedRangeCancelsThePendingRelease()
        {
            var f = Create(distance: 1000, observed: true);
            f.world.AddUnit(1001, 0, "n008", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 1000 }, new OriginalPoint(302.9, 1000));
            f.world.SetUnitState(1001, paused: true); f.world.HoldPosition(1);
            f.world.TryAttackTarget(1, OriginalWorldTargetKind.Unit, 1001, preserveHolding: true);
            f.Advance(.05); Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            Assert.That(f.world.Relocate(1001, new OriginalPoint(303.1, 1000)), Is.True);
            f.Advance(.6);
            Assert.That(f.world.UnitState(1001).health, Is.EqualTo(1000));
            Assert.That(f.Hero.holding, Is.True); Assert.That(f.Hero.approaching, Is.False);
        }

        [Test]
        public void HiddenAttackerCannotReleaseAnExistingWindup()
        {
            var f = Create(); f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686);
            f.Advance(.05); Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            Assert.That(f.world.SetVisibility(1, false), Is.True); f.Advance(1);
            Assert.That(f.Hero.hidden, Is.True); Assert.That(f.BarrelHealth, Is.EqualTo(10));
            Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
        }

        [Test]
        public void ReleasedHazeMissSurvivesDispelBeforeProjectileImpact()
        {
            var f=Create("H024",1000);
            f.world.AddUnit(1001,0,"hfoo",new OriginalWorldUnitProfile{collisionRadius=24,maxHealth=1000},new OriginalPoint(435,1000));
            f.world.SetUnitState(1001,paused:true);
            var flags=BindingFlags.NonPublic|BindingFlags.Instance;
            var rules=new OriginalArcherDebuffRules(f.combat,Load<OriginalNativeCatalog>("lia39-native126.json"),"A165",3);
            typeof(OriginalSession).GetMethod("ApplyArcherNativeBuff",flags).Invoke(f.session,new object[]{1,rules});
            // This seed is a host regression fixture, not Warcraft RNG parity.
            typeof(OriginalSession).GetField("weaponRandom",flags).SetValue(f.session,1u);
            f.Command(OriginalSessionCommandKind.AttackTarget,OriginalWorldTargetKind.Unit,1001);
            var shots=(System.Collections.IList)typeof(OriginalSession).GetField("projectiles",flags).GetValue(f.session);
            for(int i=0;i<300 && shots.Count==0;i++)f.Advance(.01);
            Assert.That(f.session.HaltReason,Is.Null);Assert.That(shots.Count,Is.EqualTo(1));
            Assert.That(shots[0].GetType().GetField("missed",flags).GetValue(shots[0]),Is.EqualTo(true));
            typeof(OriginalSession).GetMethod("RemoveArcherDebuffs",flags).Invoke(f.session,new object[]{1});
            f.Command(OriginalSessionCommandKind.Stop);f.world.SetUnitState(1,paused:true);f.Advance(1);
            Assert.That(shots.Count,Is.Zero);Assert.That(f.world.UnitState(1001).health,Is.EqualTo(1000));
        }

        [Test]
        public void AReleasedProjectileCannotDamageATargetThatBecameHidden()
        {
            var f = Create("H024", 1000);
            f.world.AddUnit(1001, 0, "H008", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 1000 }, new OriginalPoint(435, 1000));
            f.world.SetUnitState(1001, paused: true);
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Unit, 1001);
            f.Advance(.05 + Windup(f) + .01);
            Assert.That(f.world.UnitState(1001).health, Is.EqualTo(1000), "The projectile is in flight.");
            Assert.That(f.world.SetVisibility(1001, false), Is.True); f.Advance(1);
            Assert.That(f.world.UnitState(1001).health, Is.EqualTo(1000));
        }

        [Test]
        public void FarBarrelRequiresApproachAndSourceWindupAndDiesExactlyOnce()
        {
            var f = Create(distance: 600); var before = f.Hero.position;
            long initialNavigationRevision = f.navigation.NavigationRevision;
            Assert.That(f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.BarrelHealth, Is.EqualTo(10)); Assert.That(f.Hero.attackSequence, Is.Zero);
            for (int tick = 0; tick < 100 && f.Hero.attackSequence == 0; tick++)
            { f.Advance(.05); Assert.That(f.BarrelHealth, Is.EqualTo(10)); }
            Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            Assert.That(f.Hero.position.x, Is.GreaterThan(before.x));
            double distance = 735 - f.Hero.position.x;
            Assert.That(distance, Is.LessThanOrEqualTo(f.combat.Unit("H008").Number("rangeN1")));
            f.Advance(Windup(f) - .01); Assert.That(f.BarrelHealth, Is.EqualTo(10));
            f.Advance(.02); Assert.That(f.BarrelHealth, Is.Zero);
            Assert.That(f.navigation.NavigationRevision, Is.EqualTo(initialNavigationRevision + 1));
            Assert.That(f.navigation.destructionCalls, Is.EqualTo(1));
            f.Advance(3);
            Assert.That(f.navigation.destructionCalls, Is.EqualTo(1));
            Assert.That(f.Hero.order, Is.EqualTo(OriginalWorldOrder.None));
        }

        [Test]
        public void StopBeforeReleaseCancelsThePendingMeleeHit()
        {
            var f = Create();
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686);
            f.Advance(.10); Assert.That(f.Hero.attackSequence, Is.EqualTo(1)); Assert.That(f.BarrelHealth, Is.EqualTo(10));
            Assert.That(f.Command(OriginalSessionCommandKind.Stop), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(1);
            Assert.That(f.BarrelHealth, Is.EqualTo(10)); Assert.That(f.navigation.destructionCalls, Is.Zero);
        }

        [Test]
        public void RangedHitWaitsForSourceTravelTimeAndStopDoesNotRecallReleasedShot()
        {
            var f = Create("H024", 300);
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686);
            f.Advance(.05); Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            f.Advance(Windup(f) - .01); Assert.That(f.BarrelHealth, Is.EqualTo(10));
            f.Advance(.02); Assert.That(f.BarrelHealth, Is.EqualTo(10), "The missile has left the weapon but has not arrived.");
            Assert.That(f.Command(OriginalSessionCommandKind.Stop), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            double travel = 300 / f.combat.Unit("H024").Number("Missilespeed1");
            f.Advance(travel - .01); Assert.That(f.BarrelHealth, Is.EqualTo(10));
            f.Advance(.02); Assert.That(f.BarrelHealth, Is.Zero);
            Assert.That(f.navigation.destructionCalls, Is.EqualTo(1));
        }

        [Test]
        public void PausedHeroCannotAcceptMovementOrAttackOrReleaseAnExistingWindup()
        {
            var f = Create(); var position = f.Hero.position;
            f.world.SetUnitState(1, paused: true);
            Assert.That(f.Command(OriginalSessionCommandKind.Move), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            f.Advance(.5); Assert.That(f.Hero.attackSequence, Is.Zero);
            f.world.SetUnitState(1, paused: false);
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686); f.Advance(.05);
            Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            f.world.SetUnitState(1, paused: true); f.Advance(1);
            Assert.That(f.BarrelHealth, Is.EqualTo(10)); Assert.That(f.Hero.position.x, Is.EqualTo(position.x));
            Assert.That(f.Hero.position.y, Is.EqualTo(position.y));
        }

        [Test]
        public void InvulnerableTargetRejectsOrdersAndCannotTakeAPreviouslyQueuedHit()
        {
            var f = Create(distance: 1000);
            f.world.AddUnit(1001, 0, "H008", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 100 }, new OriginalPoint(235, 1000));
            f.world.SetUnitState(1001, invulnerable: true);
            Assert.That(f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Unit, 1001), Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(f.world.ApplyUnitDamage(1001, 100), Is.False);
            f.world.SetUnitState(1001, invulnerable: false);
            Assert.That(f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Unit, 1001), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.05); Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            f.world.SetUnitState(1001, invulnerable: true); f.Advance(1);
            Assert.That(f.Snapshot.units.Single(u => u.entityId == 1001).health, Is.EqualTo(100));
        }

        [Test]
        public void LethalEarlierHitPreventsVictimFromReleasingItsWeaponInTheSameTick()
        {
            var f = Create(distance: 1000);
            f.world.AddUnit(1001, 0, "hfoo", new OriginalWorldUnitProfile { collisionRadius = 24, maxHealth = 1 }, new OriginalPoint(235, 1000));
            f.world.TryAttackTarget(1001, OriginalWorldTargetKind.Unit, 1);
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Unit, 1001);
            // A non-thorns hfoo isolates the same-tick stale-snapshot regression.
            // The hero's agility makes its damage point earlier than the enemy's.
            f.Advance(.55);
            Assert.That(f.Snapshot.units.Single(u => u.entityId == 1001).health, Is.Zero);
            Assert.That(f.Hero.health, Is.EqualTo(631), "A snapshot taken before the lethal hit must not resurrect a queued attack.");
        }

        [Test]
        public void StopAndReattackBeforeTheNextTickCannotReuseTheCancelledWindup()
        {
            var f = Create();
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686);
            f.Advance(.4); Assert.That(f.Hero.attackSequence, Is.EqualTo(1));
            f.Command(OriginalSessionCommandKind.Stop);
            f.Command(OriginalSessionCommandKind.AttackTarget, OriginalWorldTargetKind.Doodad, 686);
            f.Advance(.15);
            Assert.That(f.BarrelHealth, Is.EqualTo(10), "Both commands can arrive in one network pump; cancellation still has to invalidate the old release.");
        }
    }
}

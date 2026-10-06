using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSessionPyroTests
    {
        sealed class Navigation : IOriginalWorldNavigation
        {
            public OriginalPoint HeroSpawn => new OriginalPoint(0, 1000);
            public long NavigationRevision => 0;
            public bool IsWalkable(double x, double y, double r) => true;
            public bool SegmentClear(OriginalPoint a, OriginalPoint b, double r) => true;
            public OriginalPoint[] FindPath(OriginalPoint a, OriginalPoint b, double r) => new[] { b };
            public bool SetDoodadAlive(int id, bool alive) => false;
        }
        sealed class Fixture
        {
            internal OriginalSession session; internal OriginalWorld world;
            internal void Advance(double seconds)
            {
                while (seconds > 1e-9) { double d = Math.Min(.01, seconds); session.Advance(d); session.DrainEvents(); seconds -= d; }
                Assert.That(session.HaltReason, Is.Null);
            }
            internal void Enemy(int id, double x, double hp = 1000, double speed = 0)
            {
                world.AddUnit(id, 0, "hfoo", new OriginalWorldUnitProfile { maxHealth = hp, collisionRadius = 31, moveSpeed = speed }, new OriginalPoint(x, 1000));
                world.SetUnitState(id, paused: true);
            }
        }
        static T Load<T>(string name) => JsonUtility.FromJson<T>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/" + name + ".json")));
        static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, args);
        static Fixture Create(bool realMatchEnemies = false)
        {
            var f = new Fixture(); var native = Load<OriginalNativeCatalog>("lia39-native126");
            f.session = new OriginalSession(Load<OriginalMatchCatalog>("lia39-match"), Load<OriginalItemCatalog>("lia39-items"),
                Load<OriginalCombatCatalog>("lia39-combat"), Load<OriginalDuelCatalog>("lia39-duels"), new string('a',64),
                OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard), 123);
            var observed=Load<OriginalObservedCatalog>("lia39-observed126");
            f.session.ConfigureProgression(native, observed);
            if(realMatchEnemies) f.session.ConfigureBounty(observed.bounty);
            f.session.ConfigureWorld(new Navigation(), native, Array.Empty<OriginalWorldDoodadView>(),realMatchEnemies?observed:null);
            f.session.Apply(0, new OriginalSessionCommand { sequence=1, kind=OriginalSessionCommandKind.SelectHero, heroId="H024" });
            f.session.Apply(0, new OriginalSessionCommand { sequence=2, kind=OriginalSessionCommandKind.LobbyReady, ready=true });
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence=3, kind=OriginalSessionCommandKind.Start }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.world = (OriginalWorld)typeof(OriginalSession).GetField("world", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f.session);
            f.Advance(2.05); f.world.HoldPosition(1);
            var player = ((System.Collections.IList)typeof(OriginalSession).GetField("players", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f.session))[0];
            var field = player.GetType().GetField("progression"); var candidate = ((OriginalHeroProgression)field.GetValue(player)).Copy(); candidate.GrantExperience(200);
            var stats = (OriginalHeroStatsSnapshot)Call(f.session, "CalculateProgressionStats", "H024", candidate);
            Call(f.session, "ApplyProgressionProfile", player, stats, candidate);
            field.SetValue(player, candidate); player.GetType().GetField("stats").SetValue(player, stats);
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence=4,kind=OriginalSessionCommandKind.LearnSkill,skillId="A0SJ" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.session.Apply(0, new OriginalSessionCommand { sequence=5,kind=OriginalSessionCommandKind.LearnSkill,skillId="A0SP" }), Is.EqualTo(OriginalSessionReplyCode.Accepted));
            return f;
        }
        static OriginalSessionReplyCode Cast(Fixture f, long sequence, string id, double x=900) =>
            f.session.Apply(0,new OriginalSessionCommand {sequence=sequence,kind=OriginalSessionCommandKind.CastSkill,skillId=id,x=x,y=1000});
        static void LearnPortal(Fixture f)
        {
            var player=((System.Collections.IList)typeof(OriginalSession).GetField("players",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f.session))[0];
            var field=player.GetType().GetField("progression"); var candidate=((OriginalHeroProgression)field.GetValue(player)).Copy(); candidate.GrantExperience(300);
            var stats=(OriginalHeroStatsSnapshot)Call(f.session,"CalculateProgressionStats","H024",candidate);
            Call(f.session,"ApplyProgressionProfile",player,stats,candidate);field.SetValue(player,candidate);player.GetType().GetField("stats").SetValue(player,stats);
            Assert.That(f.session.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.LearnSkill,skillId="A0AE"}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
        }
        [Test] public void PublicVacuumCommitsManaAtNativeCastPointAndKeepsMainCooldownThroughZeroCostDetonation()
        {
            var f=Create(); var hero=f.world.UnitState(1); double initial=hero.mana;
            Assert.That(Cast(f,6,"A0SN"),Is.EqualTo(OriginalSessionReplyCode.InvalidCommand));
            Assert.That(Cast(f,7,"A0SJ"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.09); Assert.That(f.world.UnitState(1).mana,Is.EqualTo(initial));
            f.Advance(.01); Assert.That(f.world.UnitState(1).mana,Is.EqualTo(initial-100).Within(.0001));
            var secondary=f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SJ");
            Assert.That(secondary.castAbilityId,Is.EqualTo("A0SN")); Assert.That(secondary.manaCostKnown,Is.True); Assert.That(secondary.manaCost,Is.Zero);
            Assert.That(Cast(f,8,"A0SN",0),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.world.UnitState(1).mana,Is.EqualTo(initial-100).Within(.0001));
            f.Advance(.04); var main=f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SJ");
            Assert.That(main.castAbilityId,Is.EqualTo("A0SJ")); Assert.That(main.cooldownRemaining,Is.EqualTo(11.96).Within(.0001));
            Assert.That(Cast(f,9,"A0SJ"),Is.EqualTo(OriginalSessionReplyCode.NotReady));
        }
        [Test] public void PublicVacuumCancelledBeforeEffectSpendsNoManaAndCreatesNoProjectile()
        {
            var f=Create(); double initial=f.world.UnitState(1).mana;
            Assert.That(Cast(f,6,"A0SJ"),Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.05);
            Assert.That(f.session.Apply(0,new OriginalSessionCommand {sequence=7,kind=OriginalSessionCommandKind.Stop}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.1); Assert.That(f.world.UnitState(1).mana,Is.EqualTo(initial));
            Assert.That(f.session.Snapshot().effects.Any(e=>e.abilityId=="A0SJ"),Is.False);
            Assert.That(f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SJ").cooldownRemaining,Is.Zero);
        }
        [Test] public void InstantDetonationClearsAnExistingAttackOrderWithoutAdditionalMana()
        {
            var f=Create(); f.Enemy(1001,900);
            Assert.That(Cast(f,6,"A0SJ"),Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.1);
            Assert.That(f.session.Apply(0,new OriginalSessionCommand { sequence=7,kind=OriginalSessionCommandKind.AttackTarget,
                targetKind=OriginalWorldTargetKind.Unit,targetId=1001 }),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.world.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.AttackTarget));
            double mana=f.world.UnitState(1).mana;
            Assert.That(Cast(f,8,"A0SN",0),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(f.world.UnitState(1).order,Is.EqualTo(OriginalWorldOrder.None));
            Assert.That(f.world.UnitState(1).mana,Is.EqualTo(mana));
        }
        [Test] public void FullVacuumWaitsForThirtySixFlightTicksAndSixteenPullTicksBeforeModeTwoDamage()
        {
            var f=Create(); f.Enemy(1001,930);
            Call(f.session,"BeginPyroVacuum",1,1,new OriginalPoint(900,1000));
            Assert.That(f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SJ").castAbilityId,Is.EqualTo("A0SN"));
            f.Advance(1.83); Assert.That(f.world.UnitState(1001).health,Is.EqualTo(1000));
            f.Advance(.01); Assert.That(f.world.UnitState(1001).health,Is.EqualTo(900).Within(.0001));
            Assert.That(f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SJ").castAbilityId,Is.EqualTo("A0SJ"));
        }
        [Test] public void PublicSpheresSpendMainManaOnceAndSecondaryRecastsDoNotWaitForChannelFinish()
        {
            var f=Create(); double mana=f.world.UnitState(1).mana;
            Assert.That(Cast(f,6,"A0SP"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.1); Assert.That(f.world.UnitState(1).mana,Is.EqualTo(mana-125));
            Assert.That(f.session.Snapshot().effects.Count(e=>e.abilityId=="A0SP"),Is.EqualTo(5));
            for(int i=0;i<5;i++)
            {
                Assert.That(Cast(f,7+i,"A0SO",900),Is.EqualTo(OriginalSessionReplyCode.Accepted));
                f.Advance(.2);
                Assert.That(f.world.UnitState(1).mana,Is.EqualTo(mana-125));
                Assert.That(f.session.Snapshot().effects.Count(e=>e.abilityId=="A0SP"),Is.EqualTo(4-i));
            }
            Assert.That(f.session.Snapshot().players[0].abilities.Single(a=>a.id=="A0SP").castAbilityId,Is.EqualTo("A0SP"));
        }
        [Test] public void SecondarySphereStopBeforeEffectPreservesItsOrbAndStopAfterEffectPreservesFlight()
        {
            var f=Create(); Assert.That(Cast(f,6,"A0SP"),Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.1);
            Assert.That(Cast(f,7,"A0SO"),Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.04);
            Assert.That(f.session.Apply(0,new OriginalSessionCommand{sequence=8,kind=OriginalSessionCommandKind.Stop}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(.1); Assert.That(f.session.Snapshot().effects.Count(e=>e.abilityId=="A0SP"),Is.EqualTo(5));
            Assert.That(Cast(f,9,"A0SO"),Is.EqualTo(OriginalSessionReplyCode.Accepted)); f.Advance(.1);
            Assert.That(Call(f.session,"AutomaticAttackRecovery",1),Is.EqualTo(true));
            Assert.That(f.session.Apply(0,new OriginalSessionCommand{sequence=10,kind=OriginalSessionCommandKind.Stop}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Call(f.session,"AutomaticAttackRecovery",1),Is.EqualTo(false));
            Assert.That(f.session.Snapshot().effects.Count(e=>e.abilityId=="A0SO"),Is.EqualTo(1));
        }
        [Test] public void LethalSphereImpactAndNestedTargetDeathDoNotDuplicateDamageOrDeathEvents()
        {
            var f=Create(); f.Enemy(1001,200,20); f.Enemy(1002,270,20);
            Call(f.session,"BeginPyroSpheres",1); Call(f.session,"LaunchPyroSphere",1,new OriginalPoint(200,1000),1001);
            Call(f.session,"LaunchPyroSphere",1,new OriginalPoint(270,1000),1002);
            f.world.DrainEvents(); Call(f.session,"OnPyroSpellEffect",1);
            var deaths=f.world.DrainEvents().Where(e=>e.kind==OriginalWorldEventKind.UnitDied).ToArray();
            Assert.That(deaths.Select(e=>e.entityId),Is.EquivalentTo(new[]{1001,1002}));
            f.Advance(2);
            Assert.That(f.world.UnitState(1001).health,Is.Zero); Assert.That(f.world.UnitState(1002).health,Is.Zero);
            Assert.That(f.session.HaltReason,Is.Null);
            Assert.That(f.session.Snapshot().effects.Count(e=>e.abilityId=="A0SO"),Is.Zero);
        }
        [Test] public void NestedSphereDeathsCreditEachActualMatchEnemyAndNativeBountyExactlyOnce()
        {
            var f=Create(realMatchEnemies:true);
            f.world.SetUnitState(1,paused:true,invulnerable:true);
            Assert.That(f.session.Apply(0,new OriginalSessionCommand{sequence=6,kind=OriginalSessionCommandKind.WaveReady}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            f.Advance(3);
            var before=f.session.Snapshot();Assert.That(before.enemies.Length,Is.EqualTo(42));
            foreach(var unit in before.world.units.Where(u=>u.ownerSlot==0)) f.world.SetUnitState(unit.entityId,paused:true);
            var victims=before.enemies.Where(e=>e.rawcode=="n008").Take(2).ToArray();
            int a=OriginalWorld.EnemyEntityId(victims[0].entityId),b=OriginalWorld.EnemyEntityId(victims[1].entityId);
            foreach(var id in new[]{a,b})
            {
                var unit=f.world.UnitState(id);Assert.That(f.world.UpdateProfile(id,unit.profile,20,unit.mana),Is.True);
                f.world.ForcePosition(id,new OriginalPoint(id==a?200:270,1000));
            }
            Call(f.session,"BeginPyroSpheres",1);Call(f.session,"LaunchPyroSphere",1,new OriginalPoint(200,1000),a);
            Call(f.session,"LaunchPyroSphere",1,new OriginalPoint(270,1000),b);f.session.DrainEvents();
            Call(f.session,"OnPyroSpellEffect",1);
            var events=f.session.DrainEvents().Where(e=>e.matchEvent!=null).Select(e=>e.matchEvent).ToArray();
            Assert.That(events.Where(e=>e.kind==OriginalMatchEventKind.Remove).Select(e=>e.entityId),Is.EquivalentTo(victims.Select(v=>v.entityId)));
            Assert.That(events.Where(e=>e.kind==OriginalMatchEventKind.Experience).Select(e=>e.amount),Is.EqualTo(new[]{12d,12d}));
            var after=f.session.Snapshot();Assert.That(after.enemies.Length,Is.EqualTo(40));
            Assert.That(after.players[0].gold-before.players[0].gold,Is.EqualTo(8));
            Assert.That(after.players[0].experience-before.players[0].experience,Is.EqualTo(24));
            foreach(var victim in victims)Assert.That(f.session.ReportEnemyKilled(victim.entityId),Is.False);
            Call(f.session,"OnPyroUnitDied",a);Call(f.session,"OnPyroUnitDied",b);f.Advance(2);
            Assert.That(f.session.Snapshot().players[0].gold,Is.EqualTo(after.players[0].gold));
            Assert.That(f.session.Snapshot().players[0].experience,Is.EqualTo(after.players[0].experience));
        }
        [Test] public void ManualDetonationIncrementsCounterWithoutAnotherFlightMoveAndUsesCapturedPullVector()
        {
            var f=Create(); f.Enemy(1001,65);
            Call(f.session,"BeginPyroVacuum",1,1,new OriginalPoint(900,1000)); f.Advance(.04);
            Call(f.session,"DetonatePyroVacuum",1); f.Advance(.43);
            Assert.That(f.world.UnitState(1001).health,Is.EqualTo(1000));
            f.Advance(.01);
            Assert.That(f.world.UnitState(1001).health,Is.EqualTo(1000-50*(1+50.0/900)).Within(.0001));
            Assert.That(f.world.UnitState(1001).position.x,Is.EqualTo(65-16*10.0/15).Within(.0001));
        }
        [Test] public void HomingTargetDeathGetsExactlyOneImmediateStepAndKeepsItsLastPosition()
        {
            var f=Create(); f.Enemy(1001,1000);
            Call(f.session,"BeginPyroSpheres",1); Call(f.session,"LaunchPyroSphere",1,new OriginalPoint(1000,1000),1001);
            OriginalPoint Position()=>f.session.Snapshot().effects.Single(e=>e.abilityId=="A0SO").position;
            var before=Position(); f.world.ForceUnitDeath(1001); Call(f.session,"OnPyroUnitDied",1001); var after=Position();
            Assert.That(after.x-before.x,Is.EqualTo(18).Within(.0001));
            Call(f.session,"OnPyroUnitDied",1001); Assert.That(Position().x,Is.EqualTo(after.x));
            f.world.ForcePosition(1001,new OriginalPoint(-1000,1000)); f.Advance(.02);
            Assert.That(Position().x-after.x,Is.EqualTo(18).Within(.0001));
        }
        [Test] public void PortalPullUsesTheInclusiveOuterRingAndVisualSnapshotsAreDetached()
        {
            var f=Create(); f.Enemy(1001,165); f.Enemy(1002,300);
            Call(f.session,"BeginPyroPortal",1,1,new OriginalPoint(0,1000));
            var snapshot=f.session.Snapshot(); var portal=snapshot.effects.Single(e=>e.abilityId=="A0AE");
            Assert.That(portal.radius,Is.EqualTo(175)); portal.position=new OriginalPoint(999,999);
            Assert.That(f.session.Snapshot().effects.Single(e=>e.abilityId=="A0AE").position.x,Is.Zero);
            f.Advance(.02); Assert.That(f.world.UnitState(1001).position.x,Is.EqualTo(165));
            f.Advance(.01); Assert.That(f.world.UnitState(1001).position.x,Is.EqualTo(150));
            Assert.That(f.world.UnitState(1002).position.x,Is.EqualTo(300));
        }
        [Test] public void PublicPortalPublishesMeasuredMovementSlowWithoutInventingAttackSlowAndDecaysAfterEmitter()
        {
            var f=Create(); f.Enemy(1001,100,1000,100);
            LearnPortal(f);
            double mana=f.world.UnitState(1).mana,rate=(double)Call(f.session,"WeaponRate",f.world.UnitState(1001));
            Assert.That(Cast(f,7,"A0AE",100),Is.EqualTo(OriginalSessionReplyCode.Accepted));f.Advance(.1);
            Assert.That(f.world.UnitState(1).mana,Is.EqualTo(mana-125));
            f.Advance(.49);Assert.That(f.world.UnitState(1001).profile.moveSpeed,Is.EqualTo(100));
            f.Advance(.01);Assert.That(f.world.UnitState(1001).profile.moveSpeed,Is.EqualTo(75));
            Assert.That((double)Call(f.session,"WeaponRate",f.world.UnitState(1001)),Is.EqualTo(rate));
            f.Advance(5.49);Assert.That((bool)Call(f.session,"PyroHasChainBuff",1001),Is.True);
            f.Advance(.02);Assert.That(f.world.UnitState(1001).profile.moveSpeed,Is.EqualTo(100));
            Assert.That((bool)Call(f.session,"PyroHasChainBuff",1001),Is.False);
        }
        [Test] public void ChainBuffFeedsVacuumAndRepeatedSphereAreaMultiplicationAtTheirSourceCallsites()
        {
            var f=Create(); LearnPortal(f);f.Enemy(1001,930);
            Call(f.session,"AddPyroChainBuff",f.world.UnitState(1001),f.world.Clock+10);
            Call(f.session,"BeginPyroVacuum",1,1,new OriginalPoint(900,1000));f.Advance(1.84);
            Assert.That(f.world.UnitState(1001).health,Is.EqualTo(875).Within(1e-7));
            var g=Create();LearnPortal(g);g.Enemy(1001,200);g.Enemy(1002,270);
            foreach(int id in new[]{1001,1002})Call(g.session,"AddPyroChainBuff",g.world.UnitState(id),g.world.Clock+10);
            Call(g.session,"BeginPyroSpheres",1);Call(g.session,"LaunchPyroSphere",1,new OriginalPoint(200,1000),1001);g.Advance(.02);
            Assert.That(g.world.UnitState(1001).health,Is.EqualTo(1000-30*1.25).Within(1e-7));
            Assert.That(g.world.UnitState(1002).health,Is.EqualTo(1000-30*1.25*1.25).Within(1e-7));
        }
    }
}

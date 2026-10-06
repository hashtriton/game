using System;
using System.Linq;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalArcherRulesTests
    {
        [Test] public void LearnedUltimateAddsNativeWeaponBonusOnceWithoutChangingScriptedPrimaryOrInput()
        {
            var catalog=UnityEngine.JsonUtility.FromJson<OriginalCombatCatalog>(System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"Arena/Data/lia39-combat.json")));
            var input=OriginalHeroStats.Calculate(catalog,"N0A0",1);
            var output=OriginalArcherWeaponBonus.Apply(catalog,input,2);
            Assert.That(output.itemAttackDamageBonus,Is.EqualTo(input.itemAttackDamageBonus+40));
            Assert.That(output.attackMinimum.Require(),Is.EqualTo(input.attackMinimum.Require()+40));
            Assert.That(output.attackMaximum.Require(),Is.EqualTo(input.attackMaximum.Require()+40));
            Assert.That(output.primaryDamageBonus.Require(),Is.EqualTo(input.primaryDamageBonus.Require()));
            Assert.That(input.itemAttackDamageBonus,Is.Zero);Assert.That(output,Is.Not.SameAs(input));
            Assert.That(OriginalArcherWeaponBonus.Apply(catalog,input,0).attackMinimum.Require(),Is.EqualTo(input.attackMinimum.Require()));
        }
        static OriginalArcherCandidate C(int id, double x, double y=0, double hp=100, bool enemy=true,
            bool invisible=false,bool structure=false,bool mechanical=false,bool dead=false,bool excluded=false) =>
            new OriginalArcherCandidate(id,new OriginalPoint(x,y),hp,enemy,invisible,structure,mechanical,dead,excluded);
        static OriginalPoint Position(OriginalPoint current, OriginalArcherEffectEvent[] events)
        { foreach (var e in events) if(e.kind==OriginalArcherEventKind.ForcedPosition)current=e.position;return current; }
        [Test] public void VolleyRetainsFirstSixEligibleHostCandidatesAndUsesLiteralUkBoundary()
        {
            var candidates = new[] { C(100,0,enemy:false), C(101,0,invisible:true), C(102,0,structure:true), C(103,0,hp:.404),
                C(104,0,dead:true), C(105,0,excluded:true), C(106,901), C(8,900,hp:.405), C(7,0,mechanical:true), C(6,0),C(5,0),C(4,0),C(3,0),C(2,0) };
            var volley=new OriginalArcherVolleyRules(new OriginalPoint(0,0),100,OriginalBowElement.Fire,candidates);
            Assert.That(volley.Targets,Is.EqualTo(new[]{8,7,6,5,4,3}));
            var copy=volley.Targets;copy[0]=123;Assert.That(volley.Targets[0],Is.EqualTo(8));
        }
        [Test] public void VolleyTracksDistanceFromFixedOriginAndKeepsCapturedElementForLiveComboAtImpact()
        {
            var volley=new OriginalArcherVolleyRules(new OriginalPoint(0,0),100,OriginalBowElement.Ice,new[]{C(2,49)});
            Assert.That(volley.Tick(_=>new OriginalPoint(49,0),5,3),Is.Empty);
            Assert.That(volley.Tick(_=>new OriginalPoint(72,0),0,3),Is.Empty);
            var result=volley.Tick(_=>new OriginalPoint(60,0),5,3);
            Assert.That(result[0].kind,Is.EqualTo(OriginalArcherEventKind.Damage));Assert.That(result[0].damage,Is.EqualTo(60));
            Assert.That(result[0].applyElement,Is.True);Assert.That(result[0].element,Is.EqualTo(OriginalBowElement.Ice));
            Assert.That(volley.Completed,Is.True); Assert.That(volley.Tick(_=>null,5,3),Is.Empty);
        }
        [Test] public void PowerShotMovesEightStepsLaunchesNinthAndHitsOnlyAfterTwentyFlightSteps()
        {
            var shot=new OriginalArcherPowerShotRules(1,2,100,0,new OriginalPoint(700,0));
            var position=new OriginalPoint(0,0);var targets=new[]{C(2,0)};
            for(int i=0;i<8;i++)
            {
                var events=shot.Tick(position,targets,0,0,OriginalBowElement.None);
                Assert.That(events[0].kind,Is.EqualTo(OriginalArcherEventKind.DestructableSweep));
                Assert.That(events[0].position.x,Is.EqualTo(position.x));position=Position(position,events);
            }
            Assert.That(position.x,Is.EqualTo(-288));Assert.That(shot.Launched,Is.False);
            var launch=shot.Tick(position,targets,0,0,OriginalBowElement.None);
            Assert.That(launch.Count(e=>e.kind==OriginalArcherEventKind.ArrowLaunched),Is.EqualTo(1));
            Assert.That(shot.ArrowPositions[0].x,Is.EqualTo(-258));
            for(int i=0;i<19;i++)Assert.That(shot.Tick(position,targets,0,0,OriginalBowElement.None).Any(e=>e.kind==OriginalArcherEventKind.Damage),Is.False);
            var final=shot.Tick(position,Array.Empty<OriginalArcherCandidate>(),0,0,OriginalBowElement.None);
            Assert.That(final.Single(e=>e.kind==OriginalArcherEventKind.Damage).damage,Is.EqualTo(150));
            Assert.That(shot.ArrowPositions[0].x,Is.EqualTo(462));Assert.That(shot.Completed,Is.True);
            Assert.That(shot.Tick(position,targets,0,0,OriginalBowElement.None),Is.Empty);
        }
        [Test] public void SharedHitGroupDeduplicatesThreeSplitArrowsAndRetainsOnlyOrganicLivingEnemies()
        {
            var shot=new OriginalArcherPowerShotRules(1,1,100,0,new OriginalPoint(700,0));var position=new OriginalPoint(0,0);
            for(int i=0;i<9;i++)position=Position(position,shot.Tick(position,Array.Empty<OriginalArcherCandidate>(),3,1,OriginalBowElement.Venom));
            Assert.That(shot.ArrowPositions.Length,Is.EqualTo(3));
            var candidates=new[]{C(2,-200),C(3,-200,mechanical:true),C(4,-200,structure:true),C(5,-200,hp:.405),C(6,-200,enemy:false),C(7,-200,invisible:true)};
            OriginalArcherEffectEvent[] last=null;
            for(int i=0;i<20;i++)last=shot.Tick(position,candidates,4,2,OriginalBowElement.Lightning);
            Assert.That(last.Where(e=>e.kind==OriginalArcherEventKind.Damage).Select(e=>e.entityId),Is.EqualTo(new[]{2,7}));
            Assert.That(last.Where(e=>e.kind==OriginalArcherEventKind.Damage).All(e=>e.applyElement && e.element==OriginalBowElement.Lightning),Is.True);
        }
        [Test] public void ExcludedBackstepStillSweepsAndConsumesItsTickWithoutInventingAnotherMove()
        {
            var shot=new OriginalArcherPowerShotRules(1,1,100,0,new OriginalPoint(0,3000));var position=new OriginalPoint(0,3000);
            for(int i=0;i<8;i++)
            {
                var events=shot.Tick(position,Array.Empty<OriginalArcherCandidate>(),0,0,OriginalBowElement.None);
                Assert.That(events.Length,Is.EqualTo(1));Assert.That(events[0].kind,Is.EqualTo(OriginalArcherEventKind.DestructableSweep));
            }
            Assert.That(shot.Tick(position,Array.Empty<OriginalArcherCandidate>(),0,0,OriginalBowElement.None)[0].kind,Is.EqualTo(OriginalArcherEventKind.ArrowLaunched));
        }
        [Test] public void BadObservationDoesNotAdvanceShotAndRemovedVolleyTargetIsRetired()
        {
            var shot=new OriginalArcherPowerShotRules(1,1,0,0,new OriginalPoint(1,0));
            Assert.Throws<ArgumentException>(()=>shot.Tick(new OriginalPoint(0,0),new[]{C(2,0),C(2,1)},0,0,OriginalBowElement.None));
            var events=shot.Tick(new OriginalPoint(0,0),Array.Empty<OriginalArcherCandidate>(),0,0,OriginalBowElement.None);
            Assert.That(events.Last().position.x,Is.EqualTo(-36));
            var volley=new OriginalArcherVolleyRules(new OriginalPoint(0,0),100,OriginalBowElement.None,new[]{C(2,100)});
            Assert.That(volley.Tick(_=>null,0,0).Any(e=>e.kind==OriginalArcherEventKind.Damage),Is.False);
            Assert.That(volley.Completed,Is.True); Assert.That(volley.Tick(_=>null,5,3),Is.Empty);
        }
    }
}

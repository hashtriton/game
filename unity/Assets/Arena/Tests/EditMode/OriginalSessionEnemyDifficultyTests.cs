using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionEnemyDifficultyTests
    {
        const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
        static object Call(OriginalSession s, string name, params object[] args) => typeof(OriginalSession).GetMethod(name, Hidden).Invoke(s,args);
        static OriginalSession Create(out OriginalWorld w, OriginalDifficulty difficulty = OriginalDifficulty.Extreme)
        {
            var s=(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
            w=(OriginalWorld)typeof(OriginalSession).GetField("world",Hidden).GetValue(s);
            ((OriginalMatchOptions)typeof(OriginalSession).GetField("options",Hidden).GetValue(s)).difficulty=difficulty;
            return s;
        }
        static string[] Ids(OriginalSession s, OriginalWorld w, int id) => ((IEnumerable<string>)Call(s,"EffectiveUnitAbilityIds",w.UnitState(id))).ToArray();
        [Test] public void DifficultyOverlayReplacesAbilitiesOnceAndLaterRemovalStaysAuthoritative()
        {
            var s=Create(out var w);
            foreach(var row in new[]{(1001,"n009","A0TD","A0TC"),(1002,"n00N","A0QZ","A0VP"),(1003,"n02C","A0RH","A09A")})
            {
                w.AddUnit(row.Item1,0,row.Item2,new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(1000+(row.Item1-1001)*100,1000));
                Call(s,"ApplyEnemyDifficultyAbilities",w.UnitState(row.Item1));
                Call(s,"ApplyEnemyDifficultyAbilities",w.UnitState(row.Item1));
                Assert.That(Ids(s,w,row.Item1).Count(x=>x==row.Item3),Is.EqualTo(1));
                Assert.That(Ids(s,w,row.Item1),Does.Not.Contain(row.Item4));
                Call(s,"ApplyUnitAbilityOverlay",row.Item1,Array.Empty<string>(),new[]{row.Item3});
                Assert.That(((IEnumerable<OriginalCombatDefinition>)Call(s,"NativeUnitAbilities",w.UnitState(row.Item1))).Any(a=>a.id==row.Item3),Is.False);
            }
            Assert.That(Ids(s,w,1),Does.Not.Contain("A077"),"Allied heroes are not an Ekv spawn path.");
        }
        [Test] public void ActualFirstWavePublicationAppliesTheInstanceOverlayBeforeOrders()
        {
            var s=Create(out var w);
            for(int i=0;i<41;i++)s.Advance(.05);
            Assert.That(s.Apply(0,new OriginalSessionCommand{sequence=4,kind=OriginalSessionCommandKind.WaveReady}),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            for(int i=0;i<42;i++)s.Advance(.05);
            Assert.That(s.HaltReason,Is.Null);
            var spiders=w.Snapshot().units.Where(u=>u.rawcode=="n008" || u.rawcode=="n009").ToArray();
            Assert.That(spiders.Length,Is.EqualTo(40));
            foreach(var spider in spiders)
            { Assert.That(Ids(s,w,spider.entityId),Does.Contain("A0TD"));Assert.That(Ids(s,w,spider.entityId),Does.Not.Contain("A0TC")); }
        }
        [Test] public void BrawlerCriticalAndEvasionTransferExplicitFieldsWithoutAPausedMiss()
        {
            var s=Create(out var w,OriginalDifficulty.Standard);
            w.AddUnit(1001,0,"n00L",new OriginalWorldUnitProfile{maxHealth=1000,collisionRadius=16},new OriginalPoint(1000,1000));
            bool critical=false,normal=false,evaded=false,hit=false;
            for(int i=0;i<256;i++)
            {
                var procs=(Array)Call(s,"CaptureNativeWeaponProcs",w.UnitState(1001));
                if(procs.Length==0)normal=true;
                else { critical=true;Assert.That(procs.GetValue(0).GetType().GetField("multiplier",Hidden).GetValue(procs.GetValue(0)),Is.EqualTo(1.5)); }
                bool miss=(bool)Call(s,"RollNativeEvasion",w.UnitState(1001));evaded|=miss;hit|=!miss;
            }
            Assert.That(critical&&normal&&evaded&&hit,Is.True,"Distribution is not an exact native probability claim.");
            w.SetUnitState(1001,paused:true);
            Assert.That(Call(s,"RollNativeEvasion",w.UnitState(1001)),Is.False);
        }
    }
}


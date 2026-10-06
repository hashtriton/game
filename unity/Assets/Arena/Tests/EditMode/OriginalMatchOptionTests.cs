using System;
using System.IO;
using System.Linq;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalMatchOptionTests
    {
        static OriginalMatchCatalog Catalog() => JsonUtility.FromJson<OriginalMatchCatalog>(File.ReadAllText(
            Path.Combine(Application.dataPath,"Arena/Data/lia39-match.json")));

        [Test]
        public void CustomUsesStandardIncomeExperienceAndAllAliveMegaAltar()
        {
            var options=OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            options.difficulty=(OriginalDifficulty)0;
            options.runes=false;
            var match=new OriginalMatch(Catalog(),options,1,12345);
            Assert.That(match.RoundGold(1),Is.EqualTo(264));
            Assert.That(match.RoundExperience(1),Is.EqualTo(12));
            match.RegisterHero(1,"H008");match.Begin();
            while(match.Round<=5)
            {
                Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.Preparation));
                match.Advance(2);Assert.That(match.SetReady(1),Is.True);
                match.Advance(match.Phase==OriginalMatchPhase.BossCountdown?5:2.00001);
                for(int guard=0;guard<20&&match.Phase==OriginalMatchPhase.Combat;guard++)
                    foreach(var enemy in match.Enemies.ToArray())Assert.That(match.EnemyKilled(enemy.entityId),Is.True);
                if(match.Phase==OriginalMatchPhase.DuelPreparation)break;
                Assert.That(match.Phase,Is.EqualTo(OriginalMatchPhase.RoundTransition));
                match.Advance(match.Round%5==0?1:3);
                if(match.Phase==OriginalMatchPhase.DuelPreparation)break;
            }
            Assert.That(match.Altars,Is.EqualTo(1),"Custom Qc=0 uses the Standard all-alive altar branch.");
        }
    }
}

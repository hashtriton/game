using System;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalSummonSkillTextTests
    {
        static OriginalGameCatalogs Catalogs()=>(OriginalGameCatalogs)typeof(OriginalSkillTextTests)
            .GetMethod("Catalogs",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        [Test] public void SourceDescriptionsDistinguishHealingShieldAndForcedMovementWithoutMutatingCatalogs()
        {
            var c=Catalogs();string before=JsonUtility.ToJson(c.Combat);
            Assert.That(OriginalSummonSkillText.Describe(c,"A0FD"),Does.Contain("50 здоровья").And.Contain("5%"));
            Assert.That(OriginalSummonSkillText.Describe(c,"A18I"),Does.Contain("задержкой").And.Contain("половину"));
            Assert.That(OriginalSummonSkillText.Describe(c,"A0TR"),Does.Contain("притягивает"));
            Assert.That(OriginalSummonSkillText.Describe(c,"A11L"),Does.Contain("точке арены").And.Not.Contain("атак"));
            Assert.That(OriginalSummonSkillText.Describe(c,"A05Y"),Does.Contain("без расхода маны"));
            Assert.That(JsonUtility.ToJson(c.Combat),Is.EqualTo(before));
        }
        [Test] public void NoUnknownAbilityOrRankGetsAnInventedDescription()
        {
            var c=Catalogs();Assert.That(OriginalSummonSkillText.Describe(c,"ZZZZ"),Is.Empty);
            Assert.That(OriginalSummonSkillText.Describe(c,"A0WH",2),Is.Empty);
            foreach(string id in new[]{"A0FD","A030","A0A2","A0WH","A18I","A18J","A15T","A0TR","A11L","A1DB","A1DD","A1DI"})
                Assert.That(OriginalSummonSkillText.Describe(c,id).Length,Is.GreaterThan(30),id);
        }
    }
}

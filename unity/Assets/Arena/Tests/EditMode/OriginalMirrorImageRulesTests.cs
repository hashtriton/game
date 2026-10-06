using System.IO;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalMirrorImageRulesTests
    {
        [Test] public void EveryRankUsesExactMapFieldsAndMeasuredCastPoint()
        {
            var catalog = JsonUtility.FromJson<OriginalCombatCatalog>(File.ReadAllText(Path.Combine(Application.dataPath, "Arena/Data/lia39-combat.json")));
            for (int rank = 1; rank <= 3; rank++)
            {
                var rules = new OriginalMirrorImageRules(catalog, rank);
                Assert.That(rules.count, Is.EqualTo(rank)); Assert.That(rules.manaCost, Is.EqualTo(40 + 20 * rank));
                Assert.That(rules.cooldown, Is.EqualTo(17 - rank)); Assert.That(rules.creationDelay, Is.EqualTo(.5));
                Assert.That(rules.castPoint, Is.EqualTo(.3)); Assert.That(rules.lifetime, Is.EqualTo(30));
                Assert.That(rules.outgoing, Is.EqualTo(.15 * rank).Within(1e-10)); Assert.That(rules.incoming, Is.EqualTo(1.85 - .1 * rank).Within(1e-10));
            }
        }
        [Test] public void ScriptedDamageRegistryPreservesPercentFlatAndRankModeDistinction()
        {
            Assert.That(OriginalScriptedAttackRules.AbilityBonus("B08M", 1), Is.EqualTo(-.5));
            Assert.That(OriginalScriptedAttackRules.AbilityBonus("A0EJ", 3), Is.EqualTo(160));
            Assert.That(OriginalScriptedAttackRules.AbilityBonus("BNso", 3), Is.EqualTo(-.5));
            Assert.That(OriginalScriptedAttackRules.AbilityBonus("BNso", 4), Is.Zero);
            Assert.That(OriginalScriptedAttackRules.AbilityBonus("A05N", 3), Is.Zero);
            Assert.That(OriginalHeroRules.AbilityAttack(22, 0, new[] { -.5, 160.0 }), Is.EqualTo(189));
        }
        [Test] public void MirrorReturnRectangleUsesSourceInclusiveBounds()
        {
            Assert.That(OriginalMirrorImageRules.ReturnsToOrigin(new OriginalPoint(1248, 2464)), Is.True);
            Assert.That(OriginalMirrorImageRules.ReturnsToOrigin(new OriginalPoint(1408, 2720)), Is.True);
            Assert.That(OriginalMirrorImageRules.ReturnsToOrigin(new OriginalPoint(1408.01, 2720)), Is.False);
        }
    }
}

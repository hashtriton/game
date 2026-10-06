using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Arena.Tests
{
    public sealed class OriginalItemTextTests
    {
        OriginalGameCatalogs Catalogs()=>OriginalGameCatalogs.Load(new[]{"match","items","combat","item-passives","duels","native126","observed126","observed-items126","layout"}
            .Select(name=>AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/lia39-"+name+".json")));

        [Test] public void EveryCatalogItemCanBeInspectedWithoutChangingSourceData()
        {
            var catalogs=Catalogs();string before=JsonUtility.ToJson(catalogs.Items)+JsonUtility.ToJson(catalogs.Passives);
            var descriptions=new OriginalItemText(catalogs);
            foreach(var item in catalogs.Items.items)
                Assert.That(descriptions.Describe(item.id),Is.Not.Null.And.Not.Empty,item.id);
            Assert.That(JsonUtility.ToJson(catalogs.Items)+JsonUtility.ToJson(catalogs.Passives),Is.EqualTo(before));
        }
        [Test] public void InspectionExplainsVerifiedBonusesAndTheEntireRecipe()
        {
            var catalogs=Catalogs();var descriptions=new OriginalItemText(catalogs);
            Assert.That(descriptions.Describe("I04J"),Does.Contain("+16 к урону"));
            Assert.That(descriptions.Describe("I05P"),Does.Contain("к основной характеристике").And.Not.Contain("к ловкости"));
            string recipe=descriptions.Describe("I001");
            foreach(var ingredient in new[]{"I003","I000","I002"})
                Assert.That(recipe,Does.Contain("1 × "+catalogs.Items.Item(ingredient).displayName));
        }
        [Test] public void InspectionExplainsActiveEffectsAndKeepsPassiveItemsWithoutAnActivationHeading()
        {
            var descriptions=new OriginalItemText(Catalogs());
            Assert.That(descriptions.Describe("I07Y"),Does.Contain("Активное применение").And.Contain("350").And.Contain("25%"));
            Assert.That(descriptions.Describe("I08M"),Does.Contain("Вампиризм").And.Not.Contain("Активное применение"));
            Assert.That(descriptions.Describe("I04J"),Does.Not.Contain("Активное применение"));
        }
    }
}

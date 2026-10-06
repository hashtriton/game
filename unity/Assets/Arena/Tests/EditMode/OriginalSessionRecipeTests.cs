using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;

namespace Arena.Tests
{
    public sealed class OriginalSessionRecipeTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static object Player(OriginalSession s)=>((IList)typeof(OriginalSession).GetField("players",Private).GetValue(s))[0];
        static OriginalInventory Inventory(OriginalSession s)=>(OriginalInventory)Player(s).GetType().GetField("inventory").GetValue(Player(s));
        static OriginalSession Create()=>(OriginalSession)typeof(OriginalSessionEquipmentTests).GetMethod("Create",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        static OriginalSessionReplyCode Send(OriginalSession s,OriginalSessionCommandKind kind,string offer=null,long instance=0,bool ready=false)
        {
            int shop=offer==null?0:s.Snapshot().shops.First(x=>x.stock.Any(y=>y.itemId==offer)).instanceId;
            return s.Apply(0,new OriginalSessionCommand{kind=kind,sequence=s.Snapshot().players[0].acknowledgedSequence+1,
                itemId=offer,shopInstanceId=shop,itemInstanceId=instance,ready=ready});
        }
        [Test] public void ToggleThenPurchaseRecipeCompletesAtSourcePrice()
        {
            var s=Create();long initial=Inventory(s).Gold;
            Assert.That(Send(s,OriginalSessionCommandKind.SetQuickBuy,ready:true),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(s.Snapshot().players[0].inventory.quickBuyEnabled,Is.True);
            Assert.That(Send(s,OriginalSessionCommandKind.BuyItem,"I032"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots.Where(x=>x!=null).Select(x=>x.itemId),Is.EqualTo(new[]{"I001"}));
            Assert.That(initial-Inventory(s).Gold,Is.EqualTo(220));
        }
        [Test] public void ChargedRecipeWorksWithToggleOffAndZeroNativeCharges()
        {
            var s=Create();Assert.That(Send(s,OriginalSessionCommandKind.BuyItem,"I032"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var recipe=Inventory(s).HeroSlots[0];Assert.That(recipe.itemId,Is.EqualTo("I003"));Assert.That(recipe.charges,Is.EqualTo(0));
            long before=Inventory(s).Gold;
            Assert.That(Send(s,OriginalSessionCommandKind.UseItem,instance:recipe.instanceId),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).HeroSlots[0].itemId,Is.EqualTo("I001"));Assert.That(before-Inventory(s).Gold,Is.EqualTo(120));
        }
        [Test] public void InsufficientGoldLeavesBoughtRecipeAndDoesNotDuplicateOnReplay()
        {
            var s=Create();Inventory(s).TrySpendResources(Inventory(s).Gold-100,0);
            Assert.That(Send(s,OriginalSessionCommandKind.SetQuickBuy,ready:true),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s,OriginalSessionCommandKind.BuyItem,"I032"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            var recipe=Inventory(s).HeroSlots[0];Assert.That(recipe.itemId,Is.EqualTo("I003"));Assert.That(Inventory(s).Gold,Is.EqualTo(0));
            Assert.That(Send(s,OriginalSessionCommandKind.UseItem,instance:recipe.instanceId),Is.EqualTo(OriginalSessionReplyCode.ItemRejected));
            Assert.That(Inventory(s).HeroSlots[0].instanceId,Is.EqualTo(recipe.instanceId));Assert.That(Inventory(s).Gold,Is.EqualTo(0));
        }
        [Test] public void SourceUsesOnlyActingBagForQuickPurchaseIngredientDiscount()
        {
            var s=Create();var inventory=Inventory(s);var ingredient=inventory.CreateInstance("I000");
            Assert.That(inventory.TryPickup(ingredient,OriginalInventoryBag.Servant).Applied,Is.True);
            long before=inventory.Gold;Assert.That(Send(s,OriginalSessionCommandKind.SetQuickBuy,ready:true),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Send(s,OriginalSessionCommandKind.BuyItem,"I032"),Is.EqualTo(OriginalSessionReplyCode.Accepted));
            Assert.That(Inventory(s).ServantSlots[0].instanceId,Is.EqualTo(ingredient.instanceId));
            Assert.That(before-Inventory(s).Gold,Is.EqualTo(220));
        }
    }
}

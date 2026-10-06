using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class DisconnectedShopping
        {
            internal int index=1,soulIndex=1;
            internal bool inside;
            internal double purchase=double.PositiveInfinity,supplies=double.PositiveInfinity,souls=double.PositiveInfinity;
            internal readonly List<double> starts=new List<double>(),departures=new List<double>();
            internal string unresolved;
        }
        readonly Dictionary<int,DisconnectedShopping> disconnectedShopping=new Dictionary<int,DisconnectedShopping>();
        uint disconnectedShoppingRandom;
        // Stable host RNG substitutes GetRandomReal/GetRandomInt. The authored
        // ranges and list order are preserved; the Warcraft stream is not.
        double AiShoppingRandom()
        {
            if(disconnectedShoppingRandom==0)disconnectedShoppingRandom=unchecked((uint)seed)^0x41534931u;
            if(disconnectedShoppingRandom==0)disconnectedShoppingRandom=1;
            disconnectedShoppingRandom^=disconnectedShoppingRandom<<13;disconnectedShoppingRandom^=disconnectedShoppingRandom>>17;
            disconnectedShoppingRandom^=disconnectedShoppingRandom<<5;
            return disconnectedShoppingRandom/4294967296d;
        }
        long AiScriptPrice(string id)
        {
            var row=itemCatalog?.QuickBuy(id);
            if(row==null||row.scriptGoldValue<0)throw new InvalidOperationException("ai-script-price-unresolved:"+id);
            return row.scriptGoldValue;
        }
        long RefundAiItem(OriginalInventory candidate,string id)
        {
            if(id==null)return 0;
            long value=AiScriptPrice(id);if(value<=0)return 0;
            var slots=candidate.HeroSlots;
            for(int i=0;i<slots.Length;i++)if(slots[i]!=null&&slots[i].itemId==id)
            {
                if(!candidate.RemoveForScript(OriginalInventoryBag.Hero,i,slots[i].instanceId).Applied)
                    throw new InvalidOperationException("ai-refund-instance-rejected");
                candidate.GrantResources(value,0);return value;
            }
            return 0;
        }
        bool CommitAiInventory(Player player,OriginalInventory candidate,OriginalWorldUnitView actor,OriginalItemAction action=null)
        {
            if(action!=null&&!action.Applied)return false;
            if(action?.Code==OriginalItemActionCode.Grounded&&groundItems.Count>=8192)return false;
            if(!ApplyEquipmentProfile(player,candidate,actor))return false;
            player.inventory=candidate;
            if(action?.Code==OriginalItemActionCode.Grounded)
                groundItems.Add(action.Item.instanceId,new OriginalGroundItemView{item=CopyItem(action.Item),position=actor.position});
            SyncItemScriptInventory(player);return true;
        }
        bool TryDisconnectedShoppingPurchase(Player player,int index)
        {
            var actor=world.UnitState(player.slot);if(actor==null||actor.health<=.405||player.inventory==null)return false;
            var candidate=player.inventory.Copy();long before=candidate.Gold,refund=0;
            // eOv captures Ls BEFORE env refunds. A successful purchase then
            // overwrites gold with old Ls-price; a failed one retains refunds.
            if(match.Round<=5){refund+=RefundAiItem(candidate,"I03L");refund+=RefundAiItem(candidate,"I03M");}
            else if(match.Round<=20){refund+=RefundAiItem(candidate,"I022");refund+=RefundAiItem(candidate,"I023");}
            string id=OriginalDisconnectedShoppingRules.Item(player.hero,index);
            long price=id==null?0:AiScriptPrice(id);
            if(id==null||before<price)
            {if(refund>0)CommitAiInventory(player,candidate,actor);return false;}
            refund+=RefundAiItem(candidate,OriginalDisconnectedShoppingRules.ReplacedItem(player.hero,index));
            if(!candidate.TrySpendResources(checked(price+refund),0))return false;
            var action=candidate.TryPickup(candidate.CreateInstance(id,player.slot));
            return CommitAiInventory(player,candidate,actor,action);
        }
        void AiSupplyItem(Player player,string id,bool lumber=false)
        {
            var actor=world.UnitState(player.slot);if(actor==null||actor.health<=.405)return;
            long price=AiScriptPrice(id);var candidate=player.inventory.Copy();
            if(!candidate.TrySpendResources(lumber?0:price,lumber?price:0))return;
            var powerup=PreparePowerupPickup(player,actor,candidate,id);
            if(powerup.handled)
            {if(powerup.code==OriginalItemActionCode.Success)CommitPowerupPickup(powerup);return;}
            var action=candidate.TryPickup(candidate.CreateInstance(id,player.slot));
            CommitAiInventory(player,candidate,actor,action);
        }
        static int AiCharges(OriginalInventory inventory,string id)
        {foreach(var item in inventory.HeroSlots)if(item!=null&&item.itemId==id)return item.chargesKnown?item.charges:int.MaxValue;return 0;}
        void BuyDisconnectedSupplies(Player player,DisconnectedShopping state)
        {
            // Na is the source's separate PvP game mode, not the temporary
            // inter-round duel flag. This session implements cooperative LiA.
            if(state.index>=18)return;
            int occupied=0;foreach(var item in player.inventory.HeroSlots)if(item!=null)occupied++;
            if(match.Round<4)
            {
                if(occupied<6&&AiCharges(player.inventory,"I03L")<2)AiSupplyItem(player,"I06F");
                if(occupied<5&&AiCharges(player.inventory,"I03M")<2)AiSupplyItem(player,"I06G");
            }
            else
            {
                int round=match.Round;
                if(!DuelActive&&occupied<6&&(round==4||round==5||round==9||round==10||round==15||round==16||round==20))
                    AiSupplyItem(player,new[]{"I069","I07F","I06H","I01C"}[(int)(AiShoppingRandom()*4)],true);
                if(round<=20)
                {
                    if(occupied<6&&AiCharges(player.inventory,"I022")<2)AiSupplyItem(player,"I06C");
                    if(occupied<5&&AiCharges(player.inventory,"I023")<2)AiSupplyItem(player,"I06D");
                }
            }
        }
        void AdvanceDisconnectedShopping()
        {
            if(world==null||itemRules==null)return;
            foreach(var player in players)
            {
                if(player.connected)continue;
                var actor=world.UnitState(player.slot);if(actor==null)continue;
                bool inside=OriginalDisconnectedShoppingRules.InShopRegion(actor.position);
                if(!disconnectedShopping.TryGetValue(player.slot,out var state))
                {
                    // Enabling RI on disconnect does not emit a native enter
                    // event for a hero already inside GX.
                    disconnectedShopping[player.slot]=new DisconnectedShopping{inside=inside};continue;
                }
                if(inside&&!state.inside&&!DuelActive&&!pendingDuel)
                {
                    state.starts.Add(world.Clock+2+AiShoppingRandom());state.departures.Add(world.Clock+9+AiShoppingRandom());
                }
                state.inside=inside;
                for(int i=state.starts.Count-1;i>=0;i--)if(state.starts[i]<=world.Clock+1e-9)
                {
                    double at=state.starts[i];state.starts.RemoveAt(i);
                    if(double.IsPositiveInfinity(state.purchase)&&player.inventory.Gold>0)state.purchase=at+.3;
                    if(double.IsPositiveInfinity(state.souls)&&player.inventory.Lumber>0)state.souls=at+2;
                }
                while(state.purchase<=world.Clock+1e-9)
                {
                    if(TryDisconnectedShoppingPurchase(player,state.index)){state.index++;state.purchase+=.3;}
                    else{state.supplies=state.purchase+.1;state.purchase=double.PositiveInfinity;}
                }
                if(state.supplies<=world.Clock+1e-9)
                {state.supplies=double.PositiveInfinity;BuyDisconnectedSupplies(player,state);}
                while(state.souls<=world.Clock+1e-9)
                {
                    if(player.inventory.Lumber<OriginalDisconnectedShoppingRules.SoulThreshold(state.soulIndex))
                    {state.souls=double.PositiveInfinity;break;}
                    string research=OriginalDisconnectedShoppingRules.SoulUpgrade(state.soulIndex,
                        state.soulIndex>=61?1+(int)(AiShoppingRandom()*6):1);
                    var result=BuySoulUpgrade(player,research);
                    state.unresolved=result==OriginalSessionReplyCode.RuleUnavailable?"ai-soul-rule-unavailable:"+research:null;
                    // v7v advances after IssueImmediateOrder even when native
                    // research is rejected (for example an already-owned unique).
                    state.soulIndex=state.soulIndex>=61?62:state.soulIndex+1;state.souls+=2;
                }
                for(int i=state.departures.Count-1;i>=0;i--)if(state.departures[i]<=world.Clock+1e-9)
                {
                    state.departures.RemoveAt(i);
                    if(DuelActive||pendingDuel||match.Round==5||match.Round==10||match.Round==15||match.Round==20||match.Round==25)continue;
                    var point=OriginalDisconnectedShoppingRules.DeparturePoint(player.slot,AiShoppingRandom(),AiShoppingRandom());
                    ApplyWorldCommand(player,new OriginalSessionCommand{kind=OriginalSessionCommandKind.Move,x=point.x,y=point.y});
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemScriptRules scriptItemRules;
        sealed class NemesisState
        {
            internal int pool=1, steps, published=1;
            internal bool historicalMember;
            internal readonly HashSet<long> instances=new HashSet<long>();
        }
        readonly Dictionary<int,NemesisState> nemesisItems=new Dictionary<int,NemesisState>();
        double nextNemesisTick=2;

        NemesisState Nemesis(int slot)
        {
            if(!nemesisItems.TryGetValue(slot,out var state)) nemesisItems.Add(slot,state=new NemesisState());
            return state;
        }
        void SyncItemScriptInventory(Player player)
        {
            if(player.inventory==null) return;
            var state=Nemesis(player.slot); var current=new HashSet<long>(); bool added=false;
            foreach(var item in player.inventory.HeroSlots)
                if(item!=null && item.itemId=="I0AE") { current.Add(item.instanceId); added|=!state.instances.Contains(item.instanceId); }
            state.instances.Clear(); state.instances.UnionWith(current);
            if(added) { state.historicalMember=true; PublishNemesis(player,state); }
        }
        void PublishNemesis(Player player,NemesisState state)
        {
            var items=player.inventory.HeroSlots;
            for(int i=0;i<items.Length;i++)
                if(items[i]!=null && items[i].itemId=="I0AE")
                    player.inventory.SetScriptCharges(OriginalInventoryBag.Hero,i,items[i].instanceId,state.pool);
            state.published=state.pool;
        }
        // hU stores one value on the hero; V7 adds it separately for each copy.
        double NemesisItemSpellPower(int actor) => nemesisItems.TryGetValue(actor,out var state) ? state.published*.03 : 0;
        double NemesisItemMagicVamp(int actor) => NemesisItemSpellPower(actor);

        void AdvanceItemScripts()
        {
            int held=0;
            foreach(var player in players) { SyncItemScriptInventory(player); held+=Nemesis(player.slot).instances.Count; }
            // Ho has a global2s phase. Its exact alignment to lobby completion
            // is unavailable; match time0 is the explicit port epoch.
            while(ItemClock+1e-9>=nextNemesisTick)
            {
                nextNemesisTick+=2;
                if(held==0) continue;
                foreach(var player in players)
                {
                    var state=Nemesis(player.slot); if(!state.historicalMember) continue;
                    // jU increments0..60, then grants on callback61 (122s).
                    // Jo retains a hero after dropping the last copy.
                    if(state.steps<60) state.steps++;
                    else { state.steps=0; if(state.pool<8) {state.pool++; PublishNemesis(player,state);} }
                }
            }
        }

        OriginalSessionReplyCode UseScriptItem(Player player,OriginalSessionCommand command,OriginalItemUseView view,OriginalItemScriptRule rule)
        {
            var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if(actor.mana<rule.manaCost) return OriginalSessionReplyCode.NotReady;
            if(view.itemId=="I0B7") return DisassembleItem(player,command,rule,actor);
            if(command.targetItemInstanceId!=0) return OriginalSessionReplyCode.InvalidCommand;
            // A0WW retains the same item and all equipment. Debit only the
            // native mana cost; rebuilding equipment is unnecessary here.
            if(!world.TrySpendMana(actor.entityId,rule.manaCost)) return OriginalSessionReplyCode.NotReady;
            SyncItemScriptInventory(player);
            itemCooldowns[ItemCooldownKey(player.slot,rule.cooldownGroup)]=ItemClock+rule.cooldown;
            var state=Nemesis(player.slot);
            if(state.pool>0)
            {
                ResetAbilityCooldowns(player.slot,false);
                // HU publishes before subtracting: visible charges and the
                // spell bonuses intentionally lag until the next hU refresh.
                PublishNemesis(player,state); state.pool--;
            }
            player.lastItemAction=OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(actor.entityId,rule.abilityId);
            return OriginalSessionReplyCode.Accepted;
        }

        OriginalSessionReplyCode DisassembleItem(Player player,OriginalSessionCommand command,OriginalItemScriptRule rule,OriginalWorldUnitView actor)
        {
            var candidate=player.inventory.Copy(); var slots=candidate.HeroSlots;
            int targetSlot=-1; OriginalGroundItemView ground=null; OriginalItemInstance target=null;
            if(command.targetItemInstanceId>0)
            {
                targetSlot=Array.FindIndex(slots,item=>item!=null && item.instanceId==command.targetItemInstanceId);
                if(targetSlot>=0) target=slots[targetSlot];
                else if(groundItems.TryGetValue(command.targetItemInstanceId,out ground))
                {
                    // uq has no ownership check; retain the native item target
                    // range700. World/shop IDs absent from Yq refund normally.
                    if(SquaredDistance(actor.position,ground.position)>700*700) return OriginalSessionReplyCode.NotReady;
                    target=ground.item;
                }
                else return RejectItem(player,OriginalItemActionCode.InvalidInstance);
            }
            else
            {
                targetSlot=Array.FindIndex(slots,item=>item!=null && OriginalItemScriptRules.Disassembly(item.itemId)!=null);
                if(targetSlot>=0) target=slots[targetSlot];
            }
            var parts=OriginalItemScriptRules.Disassembly(target?.itemId);
            var consume=candidate.ConsumeCharge(command.bag,command.itemSlot,command.itemInstanceId);
            if(!consume.Applied) return RejectItem(player,consume.Code);
            var outputs=new List<OriginalItemInstance>();
            if(parts==null)
            {
                var refund=candidate.TryPickup(candidate.CreateInstance("I0B8",player.slot));
                if(!refund.Applied) return RejectItem(player,refund.Code);
                if(refund.Code==OriginalItemActionCode.Grounded) outputs.Add(refund.Item);
            }
            else
            {
                if(targetSlot>=0)
                {
                    var removed=candidate.RemoveForScript(OriginalInventoryBag.Hero,targetSlot,target.instanceId);
                    if(!removed.Applied) return RejectItem(player,removed.Code);
                }
                var added=candidate.TryPickup(candidate.CreateInstance(parts[0],player.slot));
                if(!added.Applied) return RejectItem(player,added.Code);
                if(added.Code==OriginalItemActionCode.Grounded) outputs.Add(added.Item);
                outputs.Add(candidate.CreateInstance(parts[1],player.slot));
            }
            if(groundItems.Count-(parts!=null && ground!=null?1:0)+outputs.Count>8192)
                return RejectItem(player,OriginalItemActionCode.NoSpace);
            if(!ApplyEquipmentProfile(player,candidate,actor)) return RejectItem(player,OriginalItemActionCode.UnresolvedRule);
            player.inventory=candidate;
            if(parts!=null && ground!=null) groundItems.Remove(target.instanceId);
            foreach(var item in outputs) groundItems.Add(item.instanceId,new OriginalGroundItemView{item=CopyItem(item),position=actor.position});
            SyncItemScriptInventory(player);
            itemCooldowns[ItemCooldownKey(player.slot,rule.cooldownGroup)]=ItemClock+rule.cooldown;
            player.lastItemAction=parts==null?OriginalItemActionCode.NoRecipe:OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(actor.entityId,rule.abilityId);
            return OriginalSessionReplyCode.Accepted;
        }

        void ResetItemCooldowns(int slot,bool duelReset)
        {
            var player=PlayerAt(slot); bool preserve=false;
            if(duelReset && player.inventory!=null)
            {
                var slots=player.inventory.HeroSlots;
                // Jz13390 protects only slots0..4. Slot5 really is omitted.
                for(int i=0;i<5;i++) preserve|=slots[i]!=null && slots[i].itemId=="I0AE";
            }
            string prefix=slot+":";
            foreach(var key in new List<string>(itemCooldowns.Keys))
                if(key.StartsWith(prefix,StringComparison.Ordinal) && !(preserve && key==ItemCooldownKey(slot,"A0WW"))) itemCooldowns.Remove(key);
        }
    }
}

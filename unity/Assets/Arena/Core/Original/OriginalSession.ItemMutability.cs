using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemMutability
        {
            internal int mode=1,saved;
            internal double health,mana;
            internal ItemMutability Copy()=>new ItemMutability{mode=mode,saved=saved,health=health,mana=mana};
        }
        readonly Dictionary<int,ItemMutability> itemMutability=new Dictionary<int,ItemMutability>();
        readonly Dictionary<string,OriginalItemExchangeRules> itemExchangeRules=new Dictionary<string,OriginalItemExchangeRules>();
        OriginalItemExchangeRules ItemExchangeRule(string id)
        {
            if(id!="I017"&&id!="I05E")return null;
            if(!itemExchangeRules.TryGetValue(id,out var rule))itemExchangeRules[id]=rule=new OriginalItemExchangeRules(itemCatalog,combatCatalog,id);
            return rule;
        }
        OriginalSessionReplyCode UseItemExchange(Player player,OriginalItemExchangeRules rule)
        {
            var actor=world.UnitState(player.slot);if(actor==null||actor.health<=.405||actor.mana<rule.manaCost)return OriginalSessionReplyCode.NotReady;
            if(rule.itemId=="I05E")
            {
                // Finish every potentially rejecting maximum calculation before
                // mana/CD publication. The native item itself costs zero.
                try{ApplyItemMutabilityEffect(actor.entityId);}
                catch(InvalidOperationException){return OriginalSessionReplyCode.RuleUnavailable;}
            }
            else if(!world.TrySpendMana(actor.entityId,rule.manaCost))return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot,rule.abilityId)]=ItemClock+rule.cooldown;
            player.lastItemAction=OriginalItemActionCode.Success;NotifyNativeSpellEffect(actor.entityId,rule.abilityId);
            if(rule.itemId=="I017")BeginItemSoulBurn(actor.entityId,player.slot);
            return OriginalSessionReplyCode.Accepted;
        }
        static bool MutabilityFinite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
        ItemMutability CopyItemMutability(int actor)=>itemMutability.TryGetValue(actor,out var value)?value.Copy():new ItemMutability();
        void ComposeItemMutability(OriginalHeroStatsSnapshot stats,int actor,ItemMutability candidate)
        {
            if(candidate==null&&!itemMutability.TryGetValue(actor,out candidate))return;
            stats.maxHealth=Plus(stats.maxHealth,candidate.health);stats.maxMana=Plus(stats.maxMana,candidate.mana);
        }
        void ValidateMutability()
        {
            var item=itemCatalog.Item("I05E");var ability=combatCatalog.Ability("A15B");
            if(itemCatalog.mapSha256!=OriginalNativeCatalog.ExpectedMapSha256||combatCatalog.sourceSha256!=itemCatalog.mapSha256||
                item.cooldownId!="A15B"||Array.IndexOf(item.abilityIds,"A15B")<0||ability.Text("code")!="ACtc"||
                ability.Number("Cool1")!=6||ability.Text("targs1")!="none")
                throw new InvalidOperationException("Mutability item source identity changed.");
        }
        // Tu11009/wu11053 use one player mode and one last saved amount,
        // not one reversible modifier per item. In particular another pickup
        // does NOT undo the previous exchange, and a drop retains the mode.
        void ChangeItemMutability(ItemMutability state,OriginalItemVitalityResult vitality,int operation)
        {
            ValidateMutability();
            if(operation<=0)
            {
                int undo=state.mode==1?-state.saved:state.saved;
                MutabilityMaximum(vitality,false,undo);MutabilityMaximum(vitality,true,-undo);
                state.mana+=undo;state.health-=undo;
            }
            if(operation<0)return;
            state.mode=state.mode==1?2:1;
            double raw=(state.mode==2?vitality.maxMana:vitality.maxHealth)*.2;
            if(!MutabilityFinite(raw)||raw<0||raw>int.MaxValue)throw new InvalidOperationException("Mutability exchange amount is invalid.");
            state.saved=(int)raw;
            int amount=state.mode==2?-state.saved:state.saved;
            MutabilityMaximum(vitality,false,amount);MutabilityMaximum(vitality,true,-amount);
            state.mana+=amount;state.health-=amount;
        }
        static void MutabilityMaximum(OriginalItemVitalityResult vitality,bool health,int delta)
        {
            // zl2657 adds/removes A0HG/A15A in100/10/1 chunks. The exact
            // maxima and chunk order are source-backed; current resource
            // rounding is corroborated by all104 ITEMEX2 full/partial chunks.
            // Other extreme maxima and cross-engine precision remain outside
            // that measured matrix.
            int remaining=Math.Abs(delta),sign=Math.Sign(delta);
            while(remaining>0)
            {
                int step=Math.Min(remaining>=100?100:remaining>=10?10:1,remaining)*sign;remaining-=Math.Abs(step);
                double maximum=health?vitality.maxHealth:vitality.maxMana,current=health?vitality.health:vitality.mana;
                double nextMaximum=maximum+step;
                if(!MutabilityFinite(nextMaximum)||nextMaximum<=0)throw new InvalidOperationException("Mutability maximum is not positive.");
                double next=Math.Floor(current+current*(double)(float)(step/maximum)+.5);
                if(health&&current>.405)next=Math.Max(1,next);
                next=Math.Min(nextMaximum,next);
                if(!MutabilityFinite(next)||next<0)throw new InvalidOperationException("Mutability resource is invalid.");
                if(health){vitality.maxHealth=nextMaximum;vitality.health=next;}
                else{vitality.maxMana=nextMaximum;vitality.mana=next;}
            }
        }
        void ApplyItemMutabilityEffect(int actorId)
        {
            var actor=world.UnitState(actorId);if(actor==null)return;
            var state=CopyItemMutability(actorId);
            var vitality=new OriginalItemVitalityResult{known=true,health=actor.health,maxHealth=actor.profile.maxHealth,
                mana=actor.mana,maxMana=actor.profile.maxMana};
            ChangeItemMutability(state,vitality,0);var profile=actor.profile.Copy();
            profile.maxHealth=vitality.maxHealth;profile.maxMana=vitality.maxMana;
            if(!world.UpdateProfile(actorId,profile,vitality.health,vitality.mana))throw new InvalidOperationException("Mutability profile publication failed.");
            itemMutability[actorId]=state;
        }
    }
}

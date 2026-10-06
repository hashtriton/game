using System;
using Arena.Original;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        GameObject quickItems;
        readonly Button[] quickItemButtons = new Button[6];
        readonly Text[] quickItemLabels = new Text[6];
        static readonly Key[] itemKeys = {Key.Digit1,Key.Digit2,Key.Digit3,Key.Digit4,Key.Digit5,Key.Digit6};

        void BuildQuickItems(Transform root)
        {
            var body = Box(root, "Quick item slots", new Vector2(.70f,.02f), new Vector2(.982f,.215f), panel);
            quickItems = body.gameObject;
            for (int i=0;i<quickItemButtons.Length;i++)
            {
                int slot=i;
                int column=i%3,row=i/3;
                var button=MakeButton(body, "", new Vector2(.025f+column*.325f,.53f-row*.475f), new Vector2(.325f+column*.325f,.955f-row*.475f),
                    ()=>UseQuickItem(slot), 16);
                button.gameObject.name="Quick item "+(i+1);
                quickItemButtons[i]=button; quickItemLabels[i]=button.GetComponentInChildren<Text>();
            }
        }

        void EnsureMarketCatalogs()
        {
            if(marketCatalogs!=null)return;
            marketCatalogs=OriginalGameCatalogs.Load(Network.dataAssets);
            marketRules=new OriginalItemRules(marketCatalogs.Items,marketCatalogs.Native,marketCatalogs.ObservedItems);
        }

        void UseQuickItem(int slot)
        {
            var player=LocalPlayer(Network.View);
            if(player?.inventory?.heroSlots==null||slot<0||slot>=player.inventory.heroSlots.Length)return;
            var item=player.inventory.heroSlots[slot];
            if(item==null)return;
            selectedSlot=slot;
            InventoryCommand(OriginalSessionCommandKind.UseItem);
            if(hammerInstance!=0)marketOpen=true;
        }

        static bool TypingInHud()
        {
            var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            var input=selected?selected.GetComponent<InputField>():null;
            return input&&input.isActiveAndEnabled&&input.isFocused;
        }

        void RefreshQuickItems(bool playing,OriginalSessionView view,OriginalSessionPlayerView player)
        {
            quickItems.SetActive(playing&&!marketOpen);
            if(!playing)return;
            EnsureMarketCatalogs();
            var keys=Keyboard.current;
            for(int i=0;i<quickItemButtons.Length;i++)
            {
                var item=player?.inventory?.heroSlots==null?null:player.inventory.heroSlots[i];
                var use=item==null||player.itemUses==null?null:Array.Find(player.itemUses,x=>x.instanceId==item.instanceId);
                bool enabled=use!=null&&use.code==OriginalItemUseCode.Ready&&string.IsNullOrEmpty(view.haltReason);
                quickItemButtons[i].interactable=enabled;
                string name=item==null?"ПУСТО":marketCatalogs.Items.Item(item.itemId)?.displayName??"ПРЕДМЕТ";
                string state=use!=null&&use.cooldownRemaining>0?"  "+Math.Ceiling(use.cooldownRemaining)+" С":
                    item!=null&&item.chargesKnown&&item.charges>0?"  ×"+item.charges:"";
                Write(quickItemLabels[i],(i+1)+"  "+name+"\n"+state);
                quickItemButtons[i].image.color=item!=null&&runtime.ArmedItemInstance==item.instanceId?new Color(.24f,.21f,.13f):inset;
                if(enabled&&!optionsOpen&&keys!=null&&keys[itemKeys[i]].wasPressedThisFrame&&!TypingInHud())UseQuickItem(i);
            }
        }

        string SelectedUnitTitle(OriginalSessionPlayerView player,OriginalWorldUnitView unit)
        {
            if(player==null)return "ВЫБЕРИТЕ ГЕРОЯ";
            if(unit?.kind==OriginalWorldUnitKind.Summon)
            {
                EnsureMarketCatalogs();
                return (marketCatalogs.Combat.Unit(unit.rawcode)?.name??"ПРИЗВАННЫЙ ЮНИТ").ToUpperInvariant();
            }
            return (unit?.kind==OriginalWorldUnitKind.Illusion?"ИЛЛЮЗИЯ / ":"")+HeroName(player.heroId)+
                (player.hasProgression?"  /  УР. "+player.progression.level:"");
        }
    }
}

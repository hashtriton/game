using System;
using System.Collections.Generic;
using System.Text;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        RectTransform tooltipPanel;
        Text tooltipText,tooltipScrollHint;
        RectTransform tooltipViewport,tooltipScrollThumb;
        float tooltipScroll;
        OriginalHudTooltipTarget tooltipTarget;
        OriginalItemText itemText;
        readonly Dictionary<string,string> skillDescriptions=new Dictionary<string,string>();

        void BuildTooltips(Transform root)
        {
            tooltipPanel=Box(root,"Tooltip",new Vector2(.5f,.5f),new Vector2(.5f,.5f),panel,false);
            tooltipPanel.pivot=new Vector2(0,1);
            tooltipPanel.sizeDelta=new Vector2(430,300);
            tooltipViewport=Rect(tooltipPanel,"Tooltip viewport",Vector2.zero,Vector2.one);
            tooltipViewport.offsetMin=new Vector2(16,32);tooltipViewport.offsetMax=new Vector2(-24,-14);
            tooltipViewport.gameObject.AddComponent<RectMask2D>();
            tooltipText=Label(tooltipViewport,"",16,pale,new Vector2(0,1),Vector2.one,TextAnchor.UpperLeft);
            tooltipText.supportRichText=false;
            tooltipText.rectTransform.pivot=new Vector2(.5f,1);
            tooltipText.raycastTarget=false;
            tooltipScrollHint=Label(tooltipPanel,"Колесо мыши: прокрутка",15,muted,Vector2.zero,new Vector2(1,0));
            tooltipScrollHint.rectTransform.offsetMin=new Vector2(16,6);tooltipScrollHint.rectTransform.offsetMax=new Vector2(-16,28);
            tooltipScrollThumb=Box(tooltipPanel,"Scroll position",new Vector2(1,1),Vector2.one,gold,false);
            tooltipScrollThumb.pivot=new Vector2(1,1);
            tooltipPanel.gameObject.SetActive(false);
            for(int i=0;i<6;i++)
            {int index=i;BindTooltip(quickItemButtons[i],()=>InventoryTooltip(index));}
            for(int i=0;i<12;i++)
            {
                int index=i;
                BindTooltip(inventoryButtons[i],()=>InventoryTooltip(index));
                BindTooltip(offerButtons[i],()=>OfferTooltip(index));
            }
            for(int i=0;i<castButtons.Length;i++)
            {int index=i;BindTooltip(castButtons[i],()=>SkillTooltip(index));BindTooltip(skillButtons[i],()=>SkillTooltip(index));}
        }
        void BindTooltip(Button button,Func<string> description)
        {
            var target=button.gameObject.AddComponent<OriginalHudTooltipTarget>();
            target.owner=this;target.description=description;
        }
        internal void ShowTooltip(OriginalHudTooltipTarget target)
        {if(tooltipTarget!=target)tooltipScroll=0;tooltipTarget=target;}
        internal void HideTooltip(OriginalHudTooltipTarget target)
        {if(tooltipTarget==target){tooltipTarget=null;if(tooltipPanel)tooltipPanel.gameObject.SetActive(false);}}

        void RefreshTooltip()
        {
            if(!tooltipPanel)return;
            string body=tooltipTarget&&tooltipTarget.isActiveAndEnabled?tooltipTarget.description():"";
            if(string.IsNullOrEmpty(body)&&!optionsOpen&&!matchOptionsOpen)body=WorldItemTooltip();
            tooltipPanel.gameObject.SetActive(!string.IsNullOrEmpty(body));
            if(string.IsNullOrEmpty(body))return;
            Write(tooltipText,body);
            float width=Mathf.Min(430,canvasRect.rect.width-32);
            tooltipPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
            float contentHeight=tooltipText.preferredHeight;
            float height=Mathf.Min(canvasRect.rect.height-32,contentHeight+46);
            tooltipPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
            float available=height-46,overflow=Mathf.Max(0,contentHeight-available);
            if(Mouse.current!=null&&overflow>0)tooltipScroll-=Mouse.current.scroll.ReadValue().y*.3f;
            tooltipScroll=Mathf.Clamp(tooltipScroll,0,overflow);
            tooltipText.rectTransform.sizeDelta=new Vector2(0,contentHeight);
            tooltipText.rectTransform.anchoredPosition=new Vector2(0,tooltipScroll);
            tooltipScrollHint.gameObject.SetActive(overflow>0);
            tooltipScrollThumb.gameObject.SetActive(overflow>0);
            if(overflow>0)
            {
                float thumb=Mathf.Max(24,available*available/contentHeight);
                tooltipScrollThumb.sizeDelta=new Vector2(3,thumb);
                tooltipScrollThumb.anchoredPosition=new Vector2(-10,-14-(available-thumb)*tooltipScroll/overflow);
            }
            Vector2 screen=Mouse.current!=null?Mouse.current.position.ReadValue():Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,screen,null,out var point);
            var bounds=canvasRect.rect;
            point.x=Mathf.Clamp(point.x+18,bounds.xMin+16,bounds.xMax-width-16);
            point.y=Mathf.Clamp(point.y-18,bounds.yMin+height+16,bounds.yMax-16);
            tooltipPanel.anchoredPosition=point;
            tooltipPanel.SetAsLastSibling();
        }
        string ItemDescription(string id)
        {EnsureMarketCatalogs();if(itemText==null)itemText=new OriginalItemText(marketCatalogs);return itemText.Describe(id);}
        string InventoryTooltip(int index)
        {
            var player=LocalPlayer(Network.View);var inventory=player?.inventory;
            var item=inventory==null?null:index<6?inventory.heroSlots[index]:inventory.servantSlots[index-6];
            if(item==null)return "";
            string body=ItemDescription(item.itemId);
            var use=player.itemUses==null?null:Array.Find(player.itemUses,row=>row.instanceId==item.instanceId);
            if(item.chargesKnown&&item.charges>0)body+="\n\nЗаряд: "+item.charges;
            if(use!=null&&use.cooldownRemaining>0)body+="\nДо готовности: "+Math.Ceiling(use.cooldownRemaining)+" с";
            else if(use?.code==OriginalItemUseCode.Ready)
            {
                body+="\n\nНажмите "+(index<6?(index+1).ToString():"«Использовать»")+" для применения.";
                if(use.targetMode==OriginalAbilityTargetMode.UnitOrPoint)body+=" Выберите юнита или точку левой кнопкой мыши. Повторное нажатие выбирает героя.";
                else if(use.targetMode==OriginalAbilityTargetMode.Unit)body+=" Выберите юнита левой кнопкой мыши.";
                else if(use.targetMode==OriginalAbilityTargetMode.Point)body+=" Укажите точку левой кнопкой мыши.";
                if(use.targetMode!=OriginalAbilityTargetMode.None)body+=" К дальней цели герой подойдёт перед применением.";
            }
            return body;
        }
        string OfferTooltip(int index)
        {
            if(soulMode)return "";
            if(groundMode){int slot=groundPage*12+index;return slot<nearbyItems.Count?ItemDescription(nearbyItems[slot].item.itemId):"";}
            var shop=listedShops.Find(row=>row.instanceId==selectedShop);
            var offers=shop==null?null:marketCatalogs?.Items.Shop(shop.unitId)?.offerIds;
            return offers!=null&&index<offers.Length?ItemDescription(offers[index]):"";
        }
        string SkillTooltip(int index)
        {
            if(runtime.SelectedUnit?.kind!=OriginalWorldUnitKind.Hero)
            {
                var abilities=runtime.SelectedUnitAbilities;
                if(index<0||index>=abilities.Length)return "";
                var selected=abilities[index];
                EnsureMarketCatalogs();
                string summonKey="summon|"+selected.id+"|"+selected.rank;
                if(!skillDescriptions.TryGetValue(summonKey,out string summonDescription))
                    skillDescriptions[summonKey]=summonDescription=OriginalSummonSkillText.Describe(marketCatalogs,selected.id,selected.rank);
                var body=new StringBuilder(runtime.SelectedAbilityName(index)).Append("\n\n").Append(summonDescription);
                if(selected.manaCostKnown)body.Append("\nМана сейчас: ").Append(selected.manaCost.ToString("0.##"));
                if(selected.cooldownRemaining>0)body.Append("\nДо готовности: ").Append(Math.Ceiling(selected.cooldownRemaining)).Append(" с");
                body.Append("\n\n").Append(selected.targetMode==OriginalAbilityTargetMode.Unit?"Выберите юнита левой кнопкой мыши.":
                    selected.targetMode==OriginalAbilityTargetMode.Point?"Укажите точку на арене левой кнопкой мыши.":"Применяется без выбора цели.");
                body.Append("\nРасходует ману выбранного существа.");
                return body.ToString();
            }
            var player=LocalPlayer(Network.View);
            if(player?.learning==null||index>=player.learning.Length)return "";
            var skill=player.learning[index];var ability=player.abilities!=null&&index<player.abilities.Length?player.abilities[index]:null;
            EnsureMarketCatalogs();
            string id=player.heroId=="H024"&&!string.IsNullOrEmpty(ability?.castAbilityId)?ability.castAbilityId:skill.id;
            string key=player.heroId+"|"+id+"|"+skill.rank;
            if(!skillDescriptions.TryGetValue(key,out string description))
                skillDescriptions[key]=description=OriginalSkillText.Describe(marketCatalogs,player.heroId,id,skill.rank);
            var text=new StringBuilder(description);
            if(ability==null)return text.ToString();
            if(ability.cooldownRemaining>0)text.Append("\nДо готовности: ").Append(Math.Ceiling(ability.cooldownRemaining)).Append(" с");
            text.Append("\n\n").Append(ability.code==OriginalAbilityUseCode.Passive?"Пассивный навык действует автоматически.":
                ability.targetMode==OriginalAbilityTargetMode.Unit?"Выберите юнита левой кнопкой мыши.":
                ability.targetMode==OriginalAbilityTargetMode.Point?"Укажите точку на арене левой кнопкой мыши.":"Применяется без выбора цели.");
            if(ability.code!=OriginalAbilityUseCode.Passive)text.Append("\nEsc или правая кнопка мыши отменяют выбор цели.");
            return text.ToString();
        }
    }
}

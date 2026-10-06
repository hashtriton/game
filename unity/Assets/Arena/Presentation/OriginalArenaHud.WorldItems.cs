using System;
using Arena.Original;
using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        Text wellLabel;
        string ActiveCurseName(OriginalSessionPlayerView player)
        {
            if(player?.auxiliaryAbilities==null)return "";
            var active=new System.Collections.Generic.List<string>();
            foreach(var ability in player.auxiliaryAbilities)
                if(ability.rank>0&&OriginalCurseRules.IsCurse(ability.id))active.Add(ability.id);
            active.Sort(StringComparer.Ordinal);
            return active.Count==0?"":(active.Count==1?"Проклятие: ":"Проклятия: ")+
                string.Join(", ",active.ConvertAll(OriginalCurseRules.Name));
        }

        string WorldItemTooltip()
        {
            if(runtime.InputBlocked)return "";
            if(runtime.HoveredWell)
            {
                var well=Network.View?.well;
                return well?.present==true?"Колодец восстановления\nЗапас: "+Number(well.mana)+" / "+Number(well.maxMana)+
                    "\nПКМ: подойти и восстановить здоровье и ману.":"";
            }
            long id=runtime.HoveredGroundItem;
            var ground=id==0||Network.View?.groundItems==null?null:Array.Find(Network.View.groundItems,g=>g.item.instanceId==id);
            if(ground==null)return "";
            EnsureMarketCatalogs();
            var item=marketCatalogs.Items.Item(ground.item.itemId);
            string owner=ground.item.ownerId==0?"":ground.item.ownerId==Network.LocalSlot?"\nВаш предмет.":"\nПринадлежит другому игроку.";
            return (item?.displayName??ground.item.itemId)+owner+"\nПКМ: подойти и подобрать.\nУправление предметами: B.";
        }

        void RefreshWellLabel(bool playing)
        {
            if(!wellLabel)
            {
                wellLabel=Label(barLayer,"",17,gold,new Vector2(.5f,.5f),new Vector2(.5f,.5f),TextAnchor.MiddleCenter);
                wellLabel.gameObject.name="Well resource label";
                wellLabel.rectTransform.sizeDelta=new Vector2(240,42);
            }
            var well=Network.View?.well;
            var point=runtime.WellScreenPoint;
            bool visible=playing&&well?.present==true&&point.z>0&&point.x>=0&&point.x<=Screen.width&&point.y>=0&&point.y<=Screen.height;
            wellLabel.gameObject.SetActive(visible);
            if(!visible)return;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,point,null,out var p))wellLabel.rectTransform.anchoredPosition=p;
            Write(wellLabel,"Колодец  "+Number(well.mana)+" / "+Number(well.maxMana));
        }
    }
}

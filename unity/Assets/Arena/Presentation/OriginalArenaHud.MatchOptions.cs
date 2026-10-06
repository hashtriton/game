using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        OriginalMatchOptions configuredRules = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
        GameObject matchOptionsPanel;
        Button matchRulesControl;
        readonly Text[] matchRuleLabels = new Text[10];
        Text matchRulesHeading;
        bool matchOptionsOpen;
        readonly Dictionary<Selectable,bool> matchModalStates=new Dictionary<Selectable,bool>();

        void ChooseDifficulty(int level)
        {
            var next = OriginalMatchOptions.ForDifficulty((OriginalDifficulty)level);
            next.returnToCenter = configuredRules.returnToCenter;
            next.compactShops = configuredRules.compactShops;
            configuredRules = next; chosenDifficulty = level;
            RefreshMatchRuleLabels();
        }

        static OriginalMatchOptions CopyMatchOptions(OriginalMatchOptions source) => new OriginalMatchOptions
        {
            difficulty=source.difficulty, heroSelection=source.heroSelection,
            altars=source.altars, equalGold=source.equalGold, casters=source.casters, runes=source.runes,
            explosiveBarrels=source.explosiveBarrels, defensiveBarrels=source.defensiveBarrels,
            curse=source.curse, acolyteBonus=source.acolyteBonus,
            returnToCenter=source.returnToCenter, compactShops=source.compactShops
        };

        // wZ15155..15173 compares these eight battle settings. Hero selection,
        // return-to-center and shop arrangement do not change source Qc.
        void ReclassifyMatchRules()
        {
            configuredRules.difficulty = configuredRules.ClassifyDifficulty();
            chosenDifficulty=(int)configuredRules.difficulty;
        }

        void ToggleMatchRule(int index)
        {
            if (Network.State!=OriginalConnectionState.Disconnected) return;
            switch(index)
            {
                case 0: configuredRules.altars=!configuredRules.altars; break;
                case 1: configuredRules.equalGold=!configuredRules.equalGold; break;
                case 2: configuredRules.casters=!configuredRules.casters; break;
                case 3: configuredRules.runes=!configuredRules.runes; break;
                case 4: configuredRules.explosiveBarrels=!configuredRules.explosiveBarrels; break;
                case 5: configuredRules.defensiveBarrels=(OriginalDefensiveBarrels)((int)configuredRules.defensiveBarrels%3+1); break;
                case 6: configuredRules.curse=!configuredRules.curse; break;
                case 7: configuredRules.acolyteBonus=!configuredRules.acolyteBonus; break;
                case 8: configuredRules.returnToCenter=!configuredRules.returnToCenter; break;
                case 9: configuredRules.compactShops=!configuredRules.compactShops; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
            ReclassifyMatchRules(); RefreshMatchRuleLabels();
        }

        void BuildMatchOptions(Transform root)
        {
            matchOptionsPanel=Box(root,"Match rules",Vector2.zero,Vector2.one,new Color(.012f,.02f,.028f,.95f)).gameObject;
            var body=Box(matchOptionsPanel.transform,"Match rules body",new Vector2(.16f,.08f),new Vector2(.84f,.92f),panel);
            matchRulesHeading=Label(body,"",30,gold,new Vector2(.045f,.865f),new Vector2(.95f,.97f));
            Label(body,"Выберите готовую сложность в лобби или настройте её правила здесь.",18,muted,
                new Vector2(.045f,.79f),new Vector2(.95f,.87f));
            for(int i=0;i<matchRuleLabels.Length;i++)
            {
                int option=i, column=i%2, row=i/2;
                float left=.045f+column*.465f, top=.76f-row*.118f;
                var control=MakeButton(body,"",new Vector2(left,top-.097f),new Vector2(left+.435f,top),()=>ToggleMatchRule(option),19);
                control.gameObject.name="Match rule "+i;
                matchRuleLabels[i]=control.GetComponentInChildren<Text>();
            }
            Label(body,"Герой может разрушать защитные бочки в обоих режимах атаки крипов. Мегабоссы используют свои руны.",17,muted,
                new Vector2(.045f,.105f),new Vector2(.95f,.19f));
            MakeButton(body,"ВЕРНУТЬСЯ В ЛОББИ",new Vector2(.045f,.035f),new Vector2(.95f,.105f),()=>SetMatchOptions(false),20);
            RefreshMatchRuleLabels();matchOptionsPanel.SetActive(false);
        }

        void RefreshMatchRuleLabels()
        {
            if(!matchRulesHeading)return;
            Write(matchRulesHeading,chosenDifficulty==0?"СВОИ ПРАВИЛА": "ПРАВИЛА / "+difficulties[chosenDifficulty-1]);
            string[] names={"Алтари возрождения","Равное золото","Крипы-заклинатели","Руны волн","Взрывающиеся бочки",
                "Защитные бочки","Проклятие","Награды аколита","Возврат в центр","Лавки рядом"};
            bool[] flags={configuredRules.altars,configuredRules.equalGold,configuredRules.casters,configuredRules.runes,
                configuredRules.explosiveBarrels,false,configuredRules.curse,configuredRules.acolyteBonus,
                configuredRules.returnToCenter,configuredRules.compactShops};
            for(int i=0;i<matchRuleLabels.Length;i++)
            {
                string state=i==5?configuredRules.defensiveBarrels==OriginalDefensiveBarrels.Attackable?"КРИПЫ АТАКУЮТ":
                    configuredRules.defensiveBarrels==OriginalDefensiveBarrels.Protected?"КРИПЫ НЕ АТАКУЮТ":"УДАЛЕНЫ":flags[i]?"ВКЛ":"ВЫКЛ";
                Write(matchRuleLabels[i],names[i]+"\n"+state);
            }
        }

        void SetMatchOptions(bool value)
        {
            bool next=value&&Network.State==OriginalConnectionState.Disconnected;
            if(next==matchOptionsOpen)return;
            matchOptionsOpen=next;
            matchOptionsPanel.SetActive(matchOptionsOpen);
            if(matchOptionsOpen)
            {
                tooltipTarget=null;matchOptionsPanel.transform.SetAsLastSibling();
                matchModalStates.Clear();
                foreach(var control in canvas.GetComponentsInChildren<Selectable>(true))
                    if(!control.transform.IsChildOf(matchOptionsPanel.transform))matchModalStates.Add(control,control.interactable);
                BlockMatchUnderlay();
                if(UnityEngine.EventSystems.EventSystem.current)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(matchRuleLabels[0].transform.parent.gameObject);
            }
            else
            {
                foreach(var entry in matchModalStates)if(entry.Key)entry.Key.interactable=entry.Value;
                matchModalStates.Clear();
                if(UnityEngine.EventSystems.EventSystem.current)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(matchRulesControl.gameObject);
            }
        }

        void BlockMatchUnderlay()
        {foreach(var entry in matchModalStates)if(entry.Key)entry.Key.interactable=false;}

        void RefreshMatchOptions(bool disconnected)
        {
            matchRulesControl.interactable=disconnected;
            if(!disconnected)SetMatchOptions(false);
            if(!matchOptionsOpen)return;
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)SetMatchOptions(false);
            if(matchOptionsOpen){BlockMatchUnderlay();matchOptionsPanel.transform.SetAsLastSibling();}
        }
    }
}

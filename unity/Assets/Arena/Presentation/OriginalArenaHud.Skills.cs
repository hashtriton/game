using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        void RefreshSkills(bool playing,OriginalSessionView view,OriginalSessionPlayerView player,OriginalWorldUnitView unit)
        {
            bool show=playing&&player!=null&&unit!=null;
            skillPanel.SetActive(show);if(!show)return;
            bool hero=unit.kind==OriginalWorldUnitKind.Hero&&player.hasProgression;
            var abilities=runtime.SelectedUnitAbilities;
            int columns=Mathf.Max(5,abilities.Length);
            Write(skillHeading,hero?"Навыки"+(player.progression.unspentSkillPoints>0?"  /  +"+player.progression.unspentSkillPoints:""):
                abilities.Length>0?"Способности выбранного юнита":"У этого юнита нет активных способностей");
            for(int i=0;i<castButtons.Length;i++)
            {
                var choice=hero&&player.learning!=null&&i<player.learning.Length?player.learning[i]:null;
                var ability=i<abilities.Length?abilities[i]:null;
                float left=.012f+i*.99f/columns,right=left+.99f/columns-.011f;
                var upgrade=(RectTransform)skillButtons[i].transform;
                upgrade.anchorMin=new Vector2(left,.62f);upgrade.anchorMax=new Vector2(right,.80f);
                skillButtons[i].gameObject.SetActive(choice!=null);
                if(choice!=null)
                {
                    skillButtons[i].interactable=choice.code==OriginalLearnCode.Available&&player.alive&&
                        !view.pendingDuel&&string.IsNullOrEmpty(view.haltReason)&&!optionsOpen&&view.phase!=OriginalMatchPhase.Won&&view.phase!=OriginalMatchPhase.Lost;
                    string available=choice.code==OriginalLearnCode.Available?"  +":choice.code==OriginalLearnCode.HeroLevelTooLow?"  ур."+choice.requiredLevel:"";
                    Write(skillLabels[i],choice.rank+"/"+choice.maximumRank+available);
                }
                castButtons[i].gameObject.SetActive(ability!=null||choice!=null);
                if(ability==null&&choice==null)continue;
                var rect=(RectTransform)castButtons[i].transform;
                rect.anchorMin=new Vector2(left,.05f);rect.anchorMax=new Vector2(right,hero?.58f:.80f);
                castButtons[i].interactable=ability?.code==OriginalAbilityUseCode.Ready&&
                    !view.pendingDuel&&string.IsNullOrEmpty(view.haltReason)&&!optionsOpen&&
                    view.phase!=OriginalMatchPhase.Won&&view.phase!=OriginalMatchPhase.Lost;
                string name=choice?.name??runtime.SelectedAbilityName(i);
                if(hero)
                    name=i==4?"Атрибуты":ability?.castAbilityId=="A0SN"?"ВЗОРВАТЬ СГУСТОК":
                        ability?.castAbilityId=="A0SO"?"ВЫПУСТИТЬ СФЕРЫ":
                        ability?.castAbilityId=="A0SR"||ability?.castAbilityId=="A0SS"?name+" (усилено)":name;
                Write(castKeys[i],i<4&&ability!=null&&ability.code!=OriginalAbilityUseCode.Passive?"QWER"[i].ToString():"");
                Write(castLabels[i],name+"\n"+(ability==null?"Недоступно":AbilityLabel(ability)));
                castGlyphs[i].Set(!hero?OriginalHudSymbol.Sun:i==4?OriginalHudSymbol.Attributes:player.heroId=="H024"?OriginalHudSymbol.Flame:
                    player.heroId=="N0A0"?(i==1?OriginalHudSymbol.Sun:OriginalHudSymbol.Arrow):
                    i==0?OriginalHudSymbol.Copies:i==1?OriginalHudSymbol.Shield:i==2?OriginalHudSymbol.Blade:OriginalHudSymbol.Sun);
                castButtons[i].image.color=ability!=null&&ability.toggledOn?new Color(.16f,.32f,.25f):inset;
            }
        }
    }
}

using System;
using Arena.Original;
using UnityEngine;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        GameObject duelPanel,betControls,matchResult;
        Text duelTitle,duelContestants,betStatus,resultTitle,resultDetail;
        Button betLeft,betRight,betLess,betMore,betDiscard;

        void BuildDuel(Transform root)
        {
            var panelRect=Box(root,"Duel scoreboard",new Vector2(.235f,.66f),new Vector2(.765f,.83f),panel);
            duelPanel=panelRect.gameObject;
            duelTitle=Label(panelRect,"",16,gold,new Vector2(.025f,.76f),new Vector2(.975f,.98f),TextAnchor.MiddleCenter);
            duelContestants=Label(panelRect,"",16,pale,new Vector2(.025f,.53f),new Vector2(.975f,.77f),TextAnchor.MiddleCenter);
            betStatus=Label(panelRect,"",11,muted,new Vector2(.025f,.31f),new Vector2(.975f,.54f),TextAnchor.MiddleCenter);
            betControls=Rect(panelRect,"Duel betting",new Vector2(.025f,.045f),new Vector2(.975f,.31f)).gameObject;
            betLeft=MakeButton(betControls.transform,"НА ЛЕВОГО",new Vector2(0,0),new Vector2(.20f,1),()=>PlaceDuelBet(1),11);
            betRight=MakeButton(betControls.transform,"НА ПРАВОГО",new Vector2(.21f,0),new Vector2(.41f,1),()=>PlaceDuelBet(2),11);
            betLess=MakeButton(betControls.transform,"-100",new Vector2(.43f,0),new Vector2(.54f,1),()=>AdjustDuelBet(-100),11);
            betMore=MakeButton(betControls.transform,"+100",new Vector2(.55f,0),new Vector2(.66f,1),()=>AdjustDuelBet(100),11);
            betDiscard=MakeButton(betControls.transform,"СБРОС БЕЗ ВОЗВРАТА",new Vector2(.68f,0),new Vector2(1,1),
                ()=>Network.SendCommand(OriginalSessionCommandKind.DiscardBet),10);
            duelPanel.SetActive(false);
            var result=Box(root,"Match result",new Vector2(.3f,.47f),new Vector2(.7f,.70f),panel);
            matchResult=result.gameObject;
            resultTitle=Label(result,"",34,gold,new Vector2(.08f,.58f),new Vector2(.92f,.91f),TextAnchor.MiddleCenter);
            resultDetail=Label(result,"",15,pale,new Vector2(.08f,.35f),new Vector2(.92f,.62f),TextAnchor.MiddleCenter);
            MakeButton(result,"ВЕРНУТЬСЯ В ЛОББИ",new Vector2(.2f,.11f),new Vector2(.8f,.31f),Leave,14);
            matchResult.SetActive(false);
        }

        bool CanBet(OriginalSessionView view,OriginalSessionPlayerView player)
        {
            var duel=view?.duel;
            return player!=null&&duel!=null&&view.hasDuel&&duel.kind==OriginalDuelKind.Pairs&&duel.betsOpen&&
                duel.phase==OriginalDuelPhase.Countdown&&player.matchSlot!=duel.firstSlot&&player.matchSlot!=duel.secondSlot&&
                string.IsNullOrEmpty(view.haltReason);
        }
        void PlaceDuelBet(int side)
        {
            var view=Network.View;var player=LocalPlayer(view);if(!CanBet(view,player))return;
            int current=view.duel.betStakes[player.matchSlot];
            int stake=current>0?current:(int)Math.Min(100,player.gold);
            if(stake>0)Network.SendCommand(OriginalSessionCommandKind.Bet,betSide:side,betStake:stake);
        }
        void AdjustDuelBet(int delta)
        {
            var view=Network.View;var player=LocalPlayer(view);if(!CanBet(view,player))return;
            int slot=player.matchSlot,side=view.duel.betSides[slot],current=view.duel.betStakes[slot];
            if(side==0)return;
            EnsureMarketCatalogs();
            int stake=(int)Math.Max(0,Math.Min(marketCatalogs.Duels.betCap,Math.Min(current+(long)delta,current+player.gold)));
            Network.SendCommand(OriginalSessionCommandKind.Bet,betSide:stake==0?0:side,betStake:stake);
        }

        void RefreshDuel(bool playing,OriginalSessionView view,OriginalSessionPlayerView player)
        {
            bool active=playing&&view.hasDuel&&view.duel!=null&&view.duel.phase!=OriginalDuelPhase.Completed;
            duelPanel.SetActive(active&&!marketOpen);
            if(active)
            {
                var duel=view.duel;
                string time=Number(Math.Max(0,duel.phaseEndsAt-duel.time))+" С";
                Write(duelTitle,(duel.kind==OriginalDuelKind.Pairs?"ДУЭЛЬ  "+duel.pair:"ГЛАДИАТОР")+"  /  "+time);
                string Name(int slot)
                {
                    var fighter=Array.Find(duel.participants,p=>p.slot==slot);
                    return fighter==null?"?":HeroName(fighter.heroRawcode)+" #"+slot;
                }
                Write(duelContestants,duel.kind==OriginalDuelKind.Pairs?Name(duel.firstSlot)+"    VS    "+Name(duel.secondSlot):
                    "ОСТАЛОСЬ ГЕРОЕВ: "+duel.remaining);
                bool canBet=CanBet(view,player);int slot=player?.matchSlot??0;
                int stake=slot>0?duel.betStakes[slot]:0,side=slot>0?duel.betSides[slot]:0;
                betControls.SetActive(canBet);
                if(canBet)
                {
                    EnsureMarketCatalogs();
                    betLeft.interactable=betRight.interactable=stake>0||player.gold>0;
                    betMore.interactable=side>0&&stake<marketCatalogs.Duels.betCap&&player.gold>0;
                    betLess.interactable=betDiscard.interactable=stake>0;
                    Write(betStatus,stake>0?"ВАША СТАВКА: "+stake+" ЗОЛОТА  /  "+(side==1?"ЛЕВЫЙ":"ПРАВЫЙ"):
                        "ВЫБЕРИТЕ ПОБЕДИТЕЛЯ. ПЕРВАЯ СТАВКА - ДО 100 ЗОЛОТА.");
                }
                else Write(betStatus,stake>0?"СТАВКА ЗАКРЫТА: "+stake+" ЗОЛОТА":
                    duel.phase==OriginalDuelPhase.Countdown?"ПОДГОТОВЬТЕСЬ К БОЮ":duel.ringStage>0?"КОЛЬЦО СУЖАЕТСЯ":"БОЙ ИДЁТ");
            }
            bool finished=playing&&(view.phase==OriginalMatchPhase.Won||view.phase==OriginalMatchPhase.Lost);
            matchResult.SetActive(finished);
            if(finished)
            {
                Write(resultTitle,view.phase==OriginalMatchPhase.Won?"ПОБЕДА":"ПОРАЖЕНИЕ");
                Write(resultDetail,view.phase==OriginalMatchPhase.Won?"Арена пройдена. Все 30 этапов завершены.":"Команда продержалась до этапа "+view.round+" из 30.");
            }
        }
    }
}

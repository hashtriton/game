using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        GameObject optionsPanel;
        bool optionsOpen;
        readonly Dictionary<Selectable,bool> optionsModalStates=new Dictionary<Selectable,bool>();
        Text volumeLabel,qualityLabel,displayLabel;
        const string VolumePreference="Arena.MasterVolume";
        const string QualityPreference="Arena.Quality";

        void BuildOptions(Transform root)
        {
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePreference,.65f));
            if(PlayerPrefs.HasKey(QualityPreference))QualitySettings.SetQualityLevel(
                Mathf.Clamp(PlayerPrefs.GetInt(QualityPreference),0,QualitySettings.names.Length-1));
            optionsPanel=Box(root,"Game menu",Vector2.zero,Vector2.one,new Color(.025f,.043f,.06f,.93f)).gameObject;
            var body=Box(optionsPanel.transform,"Menu body",new Vector2(.22f,.12f),new Vector2(.78f,.88f),panel);
            Label(body,"Меню арены",30,gold,new Vector2(.055f,.865f),new Vector2(.94f,.965f));
            Label(body,"Матч продолжается, пока открыто меню.",16,muted,new Vector2(.055f,.82f),new Vector2(.94f,.87f));
            var volumeButton=MakeButton(body,"",new Vector2(.055f,.725f),new Vector2(.485f,.80f),()=>
            {
                AudioListener.volume=AudioListener.volume>=.99f?0:Mathf.Min(1,AudioListener.volume+.2f);
                PlayerPrefs.SetFloat(VolumePreference,AudioListener.volume);PlayerPrefs.Save();RefreshOptionsLabels();
            },17);
            volumeLabel=volumeButton.GetComponentInChildren<Text>();
            var qualityButton=MakeButton(body,"",new Vector2(.515f,.725f),new Vector2(.945f,.80f),()=>
            {
                QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel()+1)%QualitySettings.names.Length);
                PlayerPrefs.SetInt(QualityPreference,QualitySettings.GetQualityLevel());PlayerPrefs.Save();RefreshOptionsLabels();
            },17);
            qualityLabel=qualityButton.GetComponentInChildren<Text>();
            var displayButton=MakeButton(body,"",new Vector2(.055f,.625f),new Vector2(.485f,.70f),()=>
            {Screen.fullScreen=!Screen.fullScreen;RefreshOptionsLabels();},17);
            displayLabel=displayButton.GetComponentInChildren<Text>();
            Label(body,"Управление",22,pale,new Vector2(.055f,.535f),new Vector2(.94f,.60f));
            Label(body,"ПКМ   Движение / атака\nЛКМ   Выбор своего юнита\nA   Атаковать цель\nS   Стоп     H   Удерживать\nSpace / F1   К герою\nМиникарта: ЛКМ камера, ПКМ идти",17,pale,new Vector2(.055f,.285f),new Vector2(.485f,.535f),TextAnchor.UpperLeft);
            Label(body,"Q W E R   Навыки\n1-6   Предметы\nB   Лавка и инвентарь\nСтрелки / колесо   Камера\nTab   Вся карта     F10   Меню",17,pale,new Vector2(.515f,.285f),new Vector2(.945f,.535f),TextAnchor.UpperLeft);
            Label(body,"Наводите курсор на предметы и навыки, чтобы узнать их свойства. Esc отменяет выбор цели.\nПокупайте снаряжение между волнами или во время боя. Разрушайте бочки атакой, чтобы освободить проход.",16,muted,new Vector2(.055f,.15f),new Vector2(.945f,.29f));
            MakeButton(body,"ПРОДОЛЖИТЬ",new Vector2(.055f,.05f),new Vector2(.34f,.13f),()=>SetOptions(false),17);
            MakeButton(body,"В ЛОББИ",new Vector2(.36f,.05f),new Vector2(.645f,.13f),()=>{Leave();SetOptions(false);},17);
            MakeButton(body,"ВЫЙТИ",new Vector2(.665f,.05f),new Vector2(.945f,.13f),()=>{Leave();Application.Quit();},17);
            RefreshOptionsLabels();optionsPanel.SetActive(false);
        }
        void RefreshOptionsLabels()
        {
            Write(volumeLabel,"Звук: "+Mathf.RoundToInt(AudioListener.volume*100)+"%");
            Write(qualityLabel,"Графика: "+QualitySettings.names[QualitySettings.GetQualityLevel()]);
            Write(displayLabel,Screen.fullScreen?"Полный экран":"В окне");
        }
        void SetOptions(bool value)
        {
            if(value&&matchOptionsPanel)SetMatchOptions(false);
            if(value==optionsOpen)return;
            optionsOpen=value;optionsPanel.SetActive(value);
            if(value)
            {
                marketOpen=false;tooltipTarget=null;RefreshOptionsLabels();optionsPanel.transform.SetAsLastSibling();
                optionsModalStates.Clear();
                foreach(var control in canvas.GetComponentsInChildren<Selectable>(true))
                    if(!control.transform.IsChildOf(optionsPanel.transform))optionsModalStates.Add(control,control.interactable);
                BlockOptionsUnderlay();
                if(UnityEngine.EventSystems.EventSystem.current)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(volumeLabel.transform.parent.gameObject);
            }
            else
            {
                foreach(var entry in optionsModalStates)if(entry.Key)entry.Key.interactable=entry.Value;
                optionsModalStates.Clear();
                if(UnityEngine.EventSystems.EventSystem.current&&menuControl)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(menuControl.gameObject);
            }
            var view=Network?.View;
            if(runtime)runtime.InputBlocked=value||view?.started!=true||
                view.phase==Arena.Original.OriginalMatchPhase.Won||view.phase==Arena.Original.OriginalMatchPhase.Lost;
        }
        void BlockOptionsUnderlay()
        {foreach(var entry in optionsModalStates)if(entry.Key)entry.Key.interactable=false;}
        void RefreshOptions()
        {
            var keyboard=Keyboard.current;
            if(keyboard!=null&&!TypingInHud()&&(keyboard.f10Key.wasPressedThisFrame||optionsOpen&&keyboard.escapeKey.wasPressedThisFrame))
                SetOptions(!optionsOpen);
            if(optionsOpen)optionsPanel.transform.SetAsLastSibling();
        }
    }
}

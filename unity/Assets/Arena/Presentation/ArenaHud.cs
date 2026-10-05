using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Arena
{
    /// <summary>Screen-space presentation. All run mutations go through ArenaGame.</summary>
    public sealed class ArenaHud : MonoBehaviour
    {
        public ArenaGame game;
        public Texture2D mapTexture;
        readonly Color ink = new Color(.035f, .055f, .085f, .96f);
        readonly Color pale = new Color(.91f, .9f, .83f);
        readonly Color gold = new Color(.94f, .7f, .32f);
        Font font;
        Canvas canvas;
        Text wave, count, health, objective, attack, skill, dodge, modalTitle, modalBody, startLabel;
        Image healthFill;
        GameObject modal, start, choices, resume;
        RunPhase lastPhase = (RunPhase)(-1);
        bool lastPaused;
        RectTransform minimapMarker;
        GameObject beginButton;

        void Awake()
        {
            if (!game) game = GetComponent<ArenaGame>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("Arena HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = .5f;
            if (!FindAnyObjectByType<EventSystem>())
            {
                var events = new GameObject("Arena Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
            }

            var top = Box(root.transform, "Header", new Vector2(.025f, .885f), new Vector2(.975f, .975f), ink);
            Label(top, "АРЕНА 3.9c", 25, gold, new Vector2(.025f, .44f), new Vector2(.37f, .88f));
            Label(top, "TAB - ВСЯ КАРТА  /  КОЛЕСО - МАСШТАБ", 12, new Color(.55f,.66f,.69f), new Vector2(.027f,.13f), new Vector2(.4f,.42f));
            wave = Label(top, "ВОЛНА 1 / 5", 24, pale, new Vector2(.42f,.43f), new Vector2(.69f,.9f));
            count = Label(top, "", 14, pale, new Vector2(.42f,.1f), new Vector2(.74f,.43f));
            Button(top, "ПАУЗА  [ESC]", new Vector2(.84f,.2f), new Vector2(.98f,.8f), () => game.TogglePause());
            objective = Label(root.transform, "", 18, pale, new Vector2(.24f,.815f), new Vector2(.76f,.868f), TextAnchor.MiddleCenter);
            beginButton = Button(top,"ПРОБНЫЙ БОЙ  [ENTER]",new Vector2(.69f,.2f),new Vector2(.835f,.8f),OnStart).gameObject;
            if (mapTexture)
            {
                var mini = new GameObject("Map",typeof(RectTransform),typeof(RawImage));
                mini.transform.SetParent(root.transform,false);
                var rect=mini.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.975f,.17f);
                rect.pivot=new Vector2(1,0);rect.sizeDelta=new Vector2(205,205);
                mini.GetComponent<RawImage>().texture=mapTexture;
                minimapMarker=Box(rect,"Hero marker",Vector2.zero,Vector2.zero,gold);
                minimapMarker.sizeDelta=new Vector2(7,7);minimapMarker.GetComponent<Image>().raycastTarget=false;
            }

            var bottom = Box(root.transform, "Status", new Vector2(.025f,.035f), new Vector2(.975f,.15f), ink);
            Label(bottom, "СТРАЖ", 14, gold, new Vector2(.025f,.6f), new Vector2(.22f,.9f));
            health = Label(bottom, "140 / 140", 20, pale, new Vector2(.19f,.58f), new Vector2(.34f,.92f));
            var hpBack = Box(bottom, "Health track", new Vector2(.025f,.27f), new Vector2(.33f,.47f), new Color(.19f,.24f,.27f));
            healthFill = Box(hpBack, "Health", Vector2.zero, Vector2.one, new Color(.28f,.79f,.62f)).GetComponent<Image>();
            Label(bottom, "WASD / ПКМ - движение", 13, new Color(.57f,.68f,.72f), new Vector2(.025f,.01f), new Vector2(.35f,.23f));
            attack = Ability(bottom, "ЛКМ", "УДАР", .39f);
            skill = Ability(bottom, "Q", "КРУГОВОЙ УДАР", .59f);
            dodge = Ability(bottom, "SPACE", "РЫВОК", .79f);

            modal = Box(root.transform, "Modal backdrop", Vector2.zero, Vector2.one, new Color(.015f,.025f,.04f,.74f)).gameObject;
            var panel = Box(modal.transform, "Run panel", new Vector2(.2f,.23f), new Vector2(.8f,.78f), ink);
            Box(panel, "Gold rule", new Vector2(.065f,.915f), new Vector2(.18f,.923f), gold);
            Label(panel, "АРЕНА 3.9c / ПРОБНЫЙ БОЙ", 14, gold, new Vector2(.065f,.79f), new Vector2(.94f,.89f));
            modalTitle = Label(panel, "ВЫСТОЙ ПЯТЬ ВОЛН", 36, pale, new Vector2(.065f,.62f), new Vector2(.94f,.8f));
            modalBody = Label(panel, "", 20, new Color(.66f,.75f,.78f), new Vector2(.065f,.3f), new Vector2(.94f,.63f));
            start = Button(panel, "ВОЙТИ НА АРЕНУ", new Vector2(.065f,.1f), new Vector2(.53f,.245f), OnStart).gameObject;
            startLabel = start.GetComponentInChildren<Text>();
            resume = Button(panel, "ПРОДОЛЖИТЬ", new Vector2(.065f,.1f), new Vector2(.53f,.245f), () => game.TogglePause()).gameObject;
            var quit = Button(panel, "ВЫХОД", new Vector2(.71f,.1f), new Vector2(.94f,.245f), () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
            choices = new GameObject("Upgrade choices", typeof(RectTransform));
            choices.transform.SetParent(panel, false);
            Anchors(choices.GetComponent<RectTransform>(), new Vector2(.055f,.06f), new Vector2(.945f,.36f));
            string[] titles = { "1  /  СИЛА\n+8 к урону", "2  /  СТОЙКОСТЬ\n+35 к здоровью и максимуму", "3  /  ТЕМП\nБыстрее бег и атака" };
            for (int i=0; i<3; i++)
            {
                int index = i;
                Button(choices.transform, titles[i], new Vector2(i/3f+.012f,.05f), new Vector2((i+1)/3f-.012f,.95f), () => game.ChooseUpgrade(index));
            }
            quit.gameObject.name = "Quit";
        }

        Text Ability(Transform parent, string key, string title, float x)
        {
            var box = Box(parent, title, new Vector2(x,.13f), new Vector2(x+.185f,.87f), new Color(.075f,.11f,.15f));
            Label(box, key, 16, gold, new Vector2(.075f,.47f), new Vector2(.93f,.92f));
            return Label(box, title, 14, pale, new Vector2(.075f,.04f), new Vector2(.98f,.51f));
        }

        void Update()
        {
            if (!game || game.Run == null) return;
            var run = game.Run;
            wave.text = run.Phase == RunPhase.Ready ? "ОСМОТР КАРТЫ" : $"ВОЛНА {run.Wave} / {ArenaRun.TotalWaves}";
            count.text = run.Phase == RunPhase.Ready ? "Свободное перемещение" : run.Phase == RunPhase.Wave ? $"Врагов: {run.AliveEnemies + run.PendingEnemies}     Побеждено: {run.Kills}" : $"Побеждено: {run.Kills}";
            beginButton.SetActive(run.Phase==RunPhase.Ready);
            if(minimapMarker && game.Hero && game.arenaMap)
            {
                var bounds=game.arenaMap.WorldBounds;var p=game.Hero.position;
                minimapMarker.anchorMin=minimapMarker.anchorMax=new Vector2(Mathf.InverseLerp(bounds.min.x,bounds.max.x,p.x),Mathf.InverseLerp(bounds.min.z,bounds.max.z,p.z));
            }
            health.text = $"{Mathf.CeilToInt(run.Health)} / {Mathf.CeilToInt(run.MaxHealth)}";
            healthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(run.Health/run.MaxHealth),1);
            healthFill.color = run.Health < run.MaxHealth*.3f ? new Color(.95f,.3f,.25f) : new Color(.28f,.79f,.62f);
            attack.text = Cooldown("УДАР", game.AttackRemaining);
            skill.text = Cooldown("КРУГОВОЙ УДАР", game.SkillRemaining);
            dodge.text = Cooldown("РЫВОК", game.DodgeRemaining);
            objective.text = run.Phase == RunPhase.Wave ? (run.Wave == 5 ? "ХРАНИТЕЛЬ ВОРОТ   /   уходи из красного круга" : "Уничтожь волну. Двигайся, чтобы избегать атак.") : "";

            if (run.Phase != lastPhase || run.IsPaused != lastPaused)
            {
                RefreshModal();
                lastPhase = run.Phase;
                lastPaused = run.IsPaused;
            }
            var keyboard = Keyboard.current;
            if (keyboard == null || run.IsPaused) return;
            if (run.Phase == RunPhase.Ready && keyboard.enterKey.wasPressedThisFrame) OnStart();
            else if (run.Phase == RunPhase.Upgrade)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) game.ChooseUpgrade(0);
                else if (keyboard.digit2Key.wasPressedThisFrame) game.ChooseUpgrade(1);
                else if (keyboard.digit3Key.wasPressedThisFrame) game.ChooseUpgrade(2);
            }
            else if ((run.Phase == RunPhase.Won || run.Phase == RunPhase.Lost) && keyboard.enterKey.wasPressedThisFrame) OnStart();
        }

        string Cooldown(string label, float time) => time > .05f ? $"{label}   {time:0.0}с" : label + "   ГОТОВ";

        void OnStart()
        {
            if (game.Run.Phase == RunPhase.Ready) game.StartRun();
            else game.RestartRun();
        }

        void RefreshModal()
        {
            var run = game.Run;
            modal.SetActive((run.Phase != RunPhase.Wave && run.Phase != RunPhase.Ready) || run.IsPaused);
            start.SetActive(!run.IsPaused && run.Phase != RunPhase.Upgrade);
            choices.SetActive(!run.IsPaused && run.Phase == RunPhase.Upgrade);
            resume.SetActive(run.IsPaused);
            modal.transform.Find("Run panel/Quit").gameObject.SetActive(run.Phase != RunPhase.Upgrade || run.IsPaused);
            startLabel.text = run.Phase == RunPhase.Ready ? "ВОЙТИ НА АРЕНУ  [ENTER]" : "НОВЫЙ ЗАБЕГ  [ENTER]";
            if (run.IsPaused)
            {
                modalTitle.text = "ПЕРЕДЫШКА";
                modalBody.text = "Забег приостановлен.\n\nEsc - вернуться к бою.";
            }
            else switch(run.Phase)
            {
                case RunPhase.Ready:
                    modalTitle.text = "ВЫСТОЙ ПЯТЬ ВОЛН";
                    modalBody.text = "Сражайся, усиливай героя и одолей хранителя.\n\nWASD / ПКМ - движение   ·   ЛКМ - удар\nQ - круговой удар   ·   Space - рывок";
                    break;
                case RunPhase.Upgrade:
                    modalTitle.text = "ВОЛНА ПРОЙДЕНА";
                    modalBody.text = "Передышка восстановила до 25 здоровья.\nВыбери усиление на остаток забега:";
                    break;
                case RunPhase.Won:
                    modalTitle.text = "АРЕНА ПОКОРЕНА";
                    modalBody.text = $"Все пять волн пройдены. Хранитель повержен.\n\nПобеждено врагов: {run.Kills}\nПопробуй другой набор усилений.";
                    break;
                case RunPhase.Lost:
                    modalTitle.text = "СТРАЖ ПАЛ";
                    modalBody.text = $"Волна {run.Wave} из 5. Побеждено врагов: {run.Kills}.\n\nИспользуй рывок и держись в движении.\nНовый забег сбросит все усиления.";
                    break;
            }
        }

        RectTransform Box(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            Anchors(rect,min,max);
            go.GetComponent<Image>().color = color;
            // Panels deliberately block pointer events; text and decoration do not.
            return rect;
        }

        Text Label(Transform parent, string text, int size, Color color, Vector2 min, Vector2 max, TextAnchor align=TextAnchor.MiddleLeft)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent,false);
            Anchors(go.GetComponent<RectTransform>(),min,max);
            var label = go.GetComponent<Text>();
            label.font=font; label.text=text; label.fontSize=size; label.color=color; label.alignment=align;
            label.raycastTarget=false;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;
            label.verticalOverflow=VerticalWrapMode.Truncate;
            return label;
        }

        Button Button(Transform parent, string text, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
        {
            var rect = Box(parent,text,min,max,new Color(.17f,.23f,.27f));
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(.95f,.78f,.49f);
            colors.pressedColor = new Color(.6f,.8f,.8f);
            button.colors=colors;
            button.onClick.AddListener(action);
            Label(rect,text,17,pale,new Vector2(.04f,.03f),new Vector2(.96f,.97f),TextAnchor.MiddleCenter);
            return button;
        }

        static void Anchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=Vector2.zero; rect.offsetMax=Vector2.zero;
        }
    }
}

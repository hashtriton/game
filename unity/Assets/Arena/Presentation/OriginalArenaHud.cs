using System.Collections.Generic;
using System.Text;
using Arena.Original;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Arena
{
    [DisallowMultipleComponent]
    public sealed partial class OriginalArenaHud : MonoBehaviour
    {
        public OriginalArenaRuntime runtime;
        public Texture2D mapTexture;
        readonly Color panel = new Color(.094f, .149f, .196f, .97f);
        readonly Color inset = new Color(.149f, .235f, .286f, 1);
        readonly Color pale = new Color(.941f, .914f, .855f);
        readonly Color muted = new Color(.702f, .765f, .788f);
        readonly Color gold = new Color(.816f, .675f, .463f);
        readonly Color green = new Color(.451f, .753f, .620f);
        readonly string[] heroIds = { "H008", "H024", "N0A0" };
        readonly string[] heroNames = { "РЫЦАРЬ", "ПИРОМАНТ", "ЛУЧНИЦА" };
        readonly string[] difficulties = { "ЛЕГКО", "ОБЫЧНО", "ЭКСТРЕМАЛЬНО", "КОШМАР" };
        readonly Dictionary<int, Image> bars = new Dictionary<int, Image>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly List<int> removed = new List<int>();
        readonly StringBuilder rosterText = new StringBuilder();
        Font font,headingFont;
        Canvas canvas;
        RectTransform canvasRect, barLayer, minimapMarker;
        GameObject lobby, connectionPanel, rosterPanel, difficultyPanel, readyButton, startButton, leaveButton, waveReady;
        Text phase, counter, heroName, health, mana, resources, status, lobbyStatus, lobbyTitle, roster, readyLabel, hint, selectionLabel,combatStats;
        Image hpFill, mpFill;
        Button solo, host, join, readyControl, startControl, selectionControl, menuControl;
        readonly Button[] heroButtons = new Button[3];
        readonly Button[] difficultyButtons = new Button[4];
        readonly Button[] skillButtons = new Button[6];
        readonly Text[] skillLabels = new Text[6];
        readonly Button[] castButtons = new Button[6];
        readonly Text[] castLabels = new Text[6];
        readonly Text[] castKeys = new Text[6];
        readonly OriginalHudGlyph[] castGlyphs = new OriginalHudGlyph[6];
        GameObject skillPanel;
        Text skillHeading;
        InputField address, port;
        int chosenHero, chosenDifficulty = 2;
        OriginalHeroSelection chosenSelection = OriginalHeroSelection.Free;
        string localError;
        GameObject ownedEvents;
        OriginalArenaAudio sound;
        OriginalNetworkGame Network => runtime ? runtime.network : null;

        void Awake() => BuildUi();

        void OnEnable()
        {
            // A script reload can retain the old generated Canvas while a new
            // control field is still null. Rebuild the presentation together.
            if (!canvas || !selectionControl || !skillPanel || !marketPanel || !quickItems || !duelPanel || !castButtons[0] || !matchOptionsPanel || !matchRulesControl)
            {
                if (canvas) { canvas.gameObject.SetActive(false); Destroy(canvas.gameObject); }
                if (ownedEvents) { ownedEvents.SetActive(false); Destroy(ownedEvents); }
                bars.Clear();mapMarkers.Clear(); BuildUi();
            }
            canvas.gameObject.SetActive(true);
        }

        void OnDisable()
        {
            if (canvas) canvas.gameObject.SetActive(false);
        }

        void BuildUi()
        {
            if (!runtime) runtime = GetComponent<OriginalArenaRuntime>();
            sound=GetComponent<OriginalArenaAudio>();if(!sound)sound=gameObject.AddComponent<OriginalArenaAudio>();
            font = Resources.Load<Font>("ArenaUi/PTSans") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            headingFont = Resources.Load<Font>("ArenaUi/RussoOne") ?? font;
            var root = new GameObject("Original arena HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            canvasRect = root.GetComponent<RectTransform>();
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            if (!FindAnyObjectByType<EventSystem>())
            {
                ownedEvents = new GameObject("Original arena input", typeof(EventSystem), typeof(InputSystemUIInputModule));
                ownedEvents.transform.SetParent(transform, false);
            }
            barLayer = Rect(root.transform, "Unit health bars", Vector2.zero, Vector2.one);
            BuildHeader(root.transform); BuildStatus(root.transform); BuildSkills(root.transform); BuildMinimap(root.transform);
            BuildQuickItems(root.transform); BuildMarket(root.transform); BuildDuel(root.transform); BuildLobby(root.transform); BuildTooltips(root.transform); BuildOptions(root.transform); BuildMatchOptions(root.transform);
        }

        void BuildHeader(Transform root)
        {
            var wealth = Box(root,"Resources",new Vector2(.018f,.913f),new Vector2(.355f,.98f),panel);
            resources=Label(wealth,"",20,gold,new Vector2(.04f,.12f),new Vector2(.96f,.88f));
            var top = Box(root, "Match header", new Vector2(.375f, .902f), new Vector2(.685f, .98f), panel);
            Box(top,"Bronze rule",new Vector2(.15f,.985f),new Vector2(.85f,1),gold,false);
            phase = Label(top, "Подготовка", 24, pale, new Vector2(.03f, .40f), new Vector2(.97f, .94f),TextAnchor.MiddleCenter);
            counter = Label(top, "", 16, muted, new Vector2(.03f, .04f), new Vector2(.97f, .42f),TextAnchor.MiddleCenter);
            waveReady = MakeButton(root, "ГОТОВ К ВОЛНЕ", new Vector2(.708f, .922f), new Vector2(.88f, .98f), () =>
                Network.SendCommand(OriginalSessionCommandKind.WaveReady)).gameObject;
            menuControl = MakeButton(root, "F10  МЕНЮ", new Vector2(.892f, .922f), new Vector2(.982f, .98f), ()=>SetOptions(true));
            status = Label(root, "", 18, gold, new Vector2(.20f, .843f), new Vector2(.80f, .892f), TextAnchor.MiddleCenter);
        }

        void BuildStatus(Transform root)
        {
            var bottom = Box(root, "Hero status", new Vector2(.175f, .02f), new Vector2(.36f, .215f), panel);
            heroName = Label(bottom, "Выберите героя", 19, gold, new Vector2(.06f, .79f), new Vector2(.94f, .96f));
            health = Label(bottom, "", 17, pale, new Vector2(.06f, .65f), new Vector2(.94f, .79f));
            mana = Label(bottom, "", 17, pale, new Vector2(.06f, .43f), new Vector2(.94f, .58f));
            var hp = Box(bottom, "Health track", new Vector2(.06f, .60f), new Vector2(.94f, .65f), inset, false);
            hpFill = Box(hp, "Health", Vector2.zero, Vector2.one, green, false).GetComponent<Image>();
            var mp = Box(bottom, "Mana track", new Vector2(.06f, .39f), new Vector2(.94f, .44f), inset, false);
            mpFill = Box(mp, "Mana", Vector2.zero, Vector2.one, new Color(.42f, .737f, .792f), false).GetComponent<Image>();
            combatStats=Label(bottom, "", 15, pale,new Vector2(.06f, .04f), new Vector2(.94f, .35f));
            hint = Label(root, "", 16, pale,new Vector2(.20f, .268f), new Vector2(.96f, .31f),TextAnchor.MiddleCenter);
        }

        void BuildMinimap(Transform root)
        {
            if (!mapTexture) return;
            var frame = Box(root, "Minimap frame", new Vector2(.018f, .02f), new Vector2(.162f, .02f), panel);
            frame.sizeDelta = new Vector2(0, 224); frame.pivot = new Vector2(.5f, 0);
            var mini = Rect(frame, "Measured map", new Vector2(.04f, .04f), new Vector2(.96f, .96f));
            var image = mini.gameObject.AddComponent<RawImage>(); image.texture = mapTexture;
            minimapSurface=mini;mini.gameObject.AddComponent<OriginalMinimapInput>().runtime=runtime;
            minimapMarker = Box(mini, "Owned hero", Vector2.zero, Vector2.zero, gold, false);
            minimapMarker.sizeDelta = new Vector2(7, 7);
        }

        void BuildSkills(Transform root)
        {
            var body = Box(root, "Skills", new Vector2(.375f, .02f), new Vector2(.685f, .215f), panel);
            skillPanel = body.gameObject;
            skillHeading = Label(body, "Навыки", 15, gold, new Vector2(.026f, .82f), new Vector2(.98f, .98f));
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                float left = .012f + i * .198f;
                skillButtons[i] = MakeButton(body, "", new Vector2(left, .62f), new Vector2(left + .187f, .80f), () =>
                {
                    var player = LocalPlayer(Network.View);
                    if (player != null && player.hasProgression && player.learning != null && index < player.learning.Length)
                        Network.SendCommand(OriginalSessionCommandKind.LearnSkill, skillId: player.learning[index].id);
                }, 11);
                skillLabels[i] = skillButtons[i].GetComponentInChildren<Text>();
                castButtons[i] = MakeButton(body, "", new Vector2(left, .05f), new Vector2(left + .187f, .58f), () => runtime.RequestSkill(index), 16);
                castLabels[i] = castButtons[i].GetComponentInChildren<Text>();
                castLabels[i].fontSize=15;
                castLabels[i].rectTransform.anchorMax=new Vector2(.96f,.65f);
                castKeys[i]=Label(castButtons[i].transform,"",15,gold,new Vector2(.07f,.73f),new Vector2(.27f,.95f),TextAnchor.UpperLeft);
                castGlyphs[i]=Rect(castButtons[i].transform,"Skill symbol",new Vector2(.35f,.68f),new Vector2(.72f,.97f)).gameObject.AddComponent<OriginalHudGlyph>();
                castGlyphs[i].color=new Color(.42f,.737f,.792f);castGlyphs[i].raycastTarget=false;
            }
        }

        void BuildLobby(Transform root)
        {
            lobby = Box(root, "Lobby backdrop", Vector2.zero, Vector2.one, new Color(.012f, .02f, .028f, .86f)).gameObject;
            var body = Box(lobby.transform, "Lobby", new Vector2(.12f, .07f), new Vector2(.88f, .93f), panel);
            Box(body, "Gold rule", new Vector2(.045f, .935f), new Vector2(.14f, .939f), gold, false);
            Label(body, "Арена / Совместное выживание", 24, gold, new Vector2(.045f, .875f), new Vector2(.95f, .93f));
            lobbyTitle = Label(body, "ВЫБЕРИТЕ ГЕРОЯ", 32, pale, new Vector2(.045f, .79f), new Vector2(.95f, .88f));
            string[] roles = { "СИЛА", "ИНТЕЛЛЕКТ", "ЛОВКОСТЬ" };
            string[] ranges = { "БЛИЖНИЙ БОЙ", "МАГИЯ", "ДАЛЬНИЙ БОЙ" };
            OriginalHudSymbol[] symbols = { OriginalHudSymbol.Shield, OriginalHudSymbol.Flame, OriginalHudSymbol.Arrow };
            for (int i = 0; i < 3; i++)
            {
                int hero = i;
                float left = .045f + i * .308f;
                heroButtons[i] = MakeButton(body, heroNames[i] + "\n" + roles[i], new Vector2(left, .626f), new Vector2(left + .294f, .773f), () => SelectHero(hero));
                var title=heroButtons[i].GetComponentInChildren<Text>();title.text=heroNames[i];title.font=headingFont;title.fontSize=24;
                title.alignment=TextAnchor.MiddleLeft;title.rectTransform.anchorMin=new Vector2(.32f,.48f);title.rectTransform.anchorMax=new Vector2(.95f,.89f);
                Label(heroButtons[i].transform,roles[i]+"\n"+ranges[i],15,muted,new Vector2(.32f,.1f),new Vector2(.95f,.47f));
                var symbol=Rect(heroButtons[i].transform,"Hero symbol",new Vector2(.05f,.24f),new Vector2(.26f,.78f)).gameObject.AddComponent<OriginalHudGlyph>();
                symbol.Set(symbols[i]);symbol.color=i==1?new Color(.84f,.48f,.30f):i==2?new Color(.42f,.737f,.792f):gold;symbol.raycastTarget=false;
            }
            difficultyPanel = Rect(body, "Difficulty", new Vector2(.045f, .50f), new Vector2(.956f, .605f)).gameObject;
            matchRulesControl = MakeButton(difficultyPanel.transform, "ПРАВИЛА МАТЧА", new Vector2(0, .64f), new Vector2(.44f, 1), () => SetMatchOptions(true), 15);
            selectionControl = MakeButton(difficultyPanel.transform, "", new Vector2(.47f, .64f), new Vector2(1, 1),
                () => chosenSelection = (OriginalHeroSelection)((int)chosenSelection % 4 + 1), 11);
            selectionLabel = selectionControl.GetComponentInChildren<Text>();
            for (int i = 0; i < 4; i++)
            {
                int level = i + 1;
                difficultyButtons[i] = MakeButton(difficultyPanel.transform, difficulties[i], new Vector2(i * .25f, 0), new Vector2(i * .25f + .24f, .62f), () => ChooseDifficulty(level), 12);
            }
            connectionPanel = Rect(body, "Connection choices", new Vector2(.045f, .17f), new Vector2(.956f, .475f)).gameObject;
            Label(connectionPanel.transform, "IP ВЕДУЩЕГО / ЛОКАЛЬНЫЙ IP ДЛЯ СЕРВЕРА", 12, muted, new Vector2(0, .77f), new Vector2(.69f, .98f));
            Label(connectionPanel.transform, "ПОРТ", 12, muted, new Vector2(.72f, .77f), new Vector2(1, .98f));
            address = Input(connectionPanel.transform, "127.0.0.1", new Vector2(0, .52f), new Vector2(.69f, .79f));
            port = Input(connectionPanel.transform, "17739", new Vector2(.72f, .52f), new Vector2(1, .79f));
            port.contentType = InputField.ContentType.IntegerNumber; port.characterLimit = 5;
            solo = MakeButton(connectionPanel.transform, "ИГРАТЬ ОДНОМУ", new Vector2(0, .17f), new Vector2(.322f, .46f), Solo, 15);
            host = MakeButton(connectionPanel.transform, "СОЗДАТЬ СЕРВЕР", new Vector2(.338f, .17f), new Vector2(.66f, .46f), Host, 15);
            join = MakeButton(connectionPanel.transform, "ПОДКЛЮЧИТЬСЯ", new Vector2(.677f, .17f), new Vector2(1, .46f), Join, 15);
            Label(connectionPanel.transform, "Для LAN ведущий указывает свой локальный IP. Игроки вводят тот же IP и порт.", 11, muted,
                new Vector2(0, -.05f), new Vector2(1, .14f));
            rosterPanel = Rect(body, "Lobby players", new Vector2(.045f, .17f), new Vector2(.956f, .60f)).gameObject;
            roster = Label(rosterPanel.transform, "", 16, pale, new Vector2(0, .20f), new Vector2(.94f, .98f), TextAnchor.UpperLeft);
            readyControl = MakeButton(rosterPanel.transform, "ГОТОВ", new Vector2(0, -.02f), new Vector2(.42f, .17f), ToggleReady);
            readyButton = readyControl.gameObject; readyLabel = readyButton.GetComponentInChildren<Text>();
            startControl = MakeButton(rosterPanel.transform, "НАЧАТЬ МАТЧ", new Vector2(.46f, -.02f), new Vector2(1, .17f), () => Network.SendCommand(OriginalSessionCommandKind.Start));
            startButton = startControl.gameObject;
            lobbyStatus = Label(body, "", 13, gold, new Vector2(.045f, .068f), new Vector2(.956f, .14f));
            leaveButton = MakeButton(body, "НАЗАД / ОТКЛЮЧИТЬСЯ", new Vector2(.60f, .015f), new Vector2(.956f, .065f), Leave, 11).gameObject;
            Label(body, "30 волн. Три героя. Один общий бой.", 15, muted,
                new Vector2(.045f, .012f), new Vector2(.59f, .062f));
        }

        void SelectHero(int index)
        {
            chosenHero = index; localError = null;
            if (Network && Network.State == OriginalConnectionState.Lobby)
                Network.SendCommand(OriginalSessionCommandKind.SelectHero, heroIds[index]);
        }

        bool TryPort(out int value)
        {
            if (int.TryParse(port.text, out value) && value >= 1 && value <= 65535) return true;
            localError = "Порт должен быть от 1 до 65535."; return false;
        }

        void Solo()
        {
            localError = null;
            if (!runtime.Host(SelectedOptions(), 0, "127.0.0.1")) return;
            if(chosenSelection==OriginalHeroSelection.Free||chosenSelection==OriginalHeroSelection.Duplicates)
                Network.SendCommand(OriginalSessionCommandKind.SelectHero, heroIds[chosenHero]);
            Network.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            Network.SendCommand(OriginalSessionCommandKind.Start);
        }

        void Host()
        {
            localError = null;
            if (!TryPort(out int value)) return;
            if (runtime.Host(SelectedOptions(), value, address.text.Trim())&&
                (chosenSelection==OriginalHeroSelection.Free||chosenSelection==OriginalHeroSelection.Duplicates))
                Network.SendCommand(OriginalSessionCommandKind.SelectHero, heroIds[chosenHero]);
        }

        OriginalMatchOptions SelectedOptions()
        {
            var options = CopyMatchOptions(configuredRules);
            options.heroSelection = chosenSelection;
            return options;
        }

        void Join()
        {
            localError = null;
            if (TryPort(out int value)) Network.Join(address.text.Trim(), value);
        }

        void Leave()
        {
            if (runtime) runtime.Disconnect();
            localError = null;
        }

        void ToggleReady()
        {
            var local = LocalPlayer(Network.View);
            if (local != null) Network.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: !local.lobbyReady);
        }

        void Update()
        {
            if (!runtime || !Network) return;
            var view = Network.View;
            bool connected = Network.State == OriginalConnectionState.Lobby || Network.State == OriginalConnectionState.Playing;
            bool playing = connected && view != null && view.started;
            bool finished = playing && (view.phase == OriginalMatchPhase.Won || view.phase == OriginalMatchPhase.Lost);
            bool disconnected = Network.State == OriginalConnectionState.Disconnected;
            RefreshOptions();
            lobby.SetActive(!playing); runtime.InputBlocked = !playing||optionsOpen||finished;
            if (finished) tooltipTarget = null;
            connectionPanel.SetActive(!connected); difficultyPanel.SetActive(!connected);
            rosterPanel.SetActive(connected && !playing);
            solo.interactable = host.interactable = join.interactable = disconnected;
            address.interactable = port.interactable = disconnected;
            selectionControl.interactable = disconnected;
            Write(selectionLabel, SelectionLabel(chosenSelection));
            leaveButton.SetActive(!disconnected);
            var player = LocalPlayer(view);
            var selection = connected && view != null ? view.options.heroSelection : chosenSelection;
            bool manualSelection = selection == OriginalHeroSelection.Free || selection == OriginalHeroSelection.Duplicates;
            for (int i = 0; i < heroButtons.Length; i++)
            {
                bool taken = false;
                if (view != null && view.players != null && view.options.heroSelection == OriginalHeroSelection.Free)
                    foreach (var other in view.players)
                        if (other.slot != Network.LocalSlot && other.heroId == heroIds[i]) taken = true;
                heroButtons[i].interactable = manualSelection && (disconnected || Network.State == OriginalConnectionState.Lobby) && !taken;
                bool selected = player != null ? player.heroId == heroIds[i] : manualSelection && chosenHero == i;
                heroButtons[i].GetComponent<Image>().color = selected ? new Color(.24f, .20f, .12f) : inset;
            }
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                difficultyButtons[i].interactable = disconnected;
                difficultyButtons[i].GetComponent<Image>().color = chosenDifficulty == i + 1 ? new Color(.24f, .20f, .12f) : inset;
            }
            Write(lobbyTitle, connected ? "СОБЕРИТЕ КОМАНДУ" : manualSelection ? "ВЫБЕРИТЕ ГЕРОЯ" :
                selection==OriginalHeroSelection.SameRandom ? "ОДИН СЛУЧАЙНЫЙ ГЕРОЙ ДЛЯ КОМАНДЫ" : "ГЕРОЙ ВЫБЕРЕТСЯ СЛУЧАЙНО");
            string networkStatus = Network.State == OriginalConnectionState.Connecting || Network.State == OriginalConnectionState.Joining ? "Подключение к ведущему..." :
                connected ? (Network.IsHost ? "Ведущий. Порт " + Network.Port : "Подключено. Игрок " + Network.LocalSlot) : "";
            Write(lobbyStatus, localError ?? Network.Error ?? runtime.Notice ?? networkStatus);
            if (connected && !playing) RefreshRoster(view, player);
            string detail = playing ? "ЭТАП " + view.round + " / 30" : "ПОДГОТОВКА К ИГРЕ";
            Write(phase, detail);
            Write(counter, playing ? PhaseName(view.phase) + (view.remainingSeconds > 0 ? "  ·  " + Mathf.CeilToInt((float)view.remainingSeconds) + " c" : "") +
                "    Врагов: " + view.remainingEnemies + "    Алтарей: " + view.altars : "");
            bool canReady = playing && view.phase == OriginalMatchPhase.Preparation && !view.pendingDuel && player != null && !player.waveReady;
            waveReady.SetActive(canReady && string.IsNullOrEmpty(view.haltReason));
            string notice = playing && !string.IsNullOrEmpty(view.haltReason) ? "МАТЧ ОСТАНОВЛЕН: " + view.haltReason :
                playing && view.pendingDuel ? "Подготовка дуэли: ожидание подтверждённых данных боя." : runtime.Notice ?? "";
            if(playing&&string.IsNullOrEmpty(notice))notice=ActiveCurseName(player);
            Write(status, notice);
            var unit = runtime.SelectedUnit ?? runtime.LocalUnit;
            Write(heroName, SelectedUnitTitle(player, unit));
            Write(resources, player == null ? "Золото  -     Души  -" : "Золото  " + player.gold + "      Души  " + player.souls + "      Опыт  " + player.experience);
            Write(health, unit == null ? "Здоровье  -" : "Здоровье  " + Number(unit.health) + " / " + Number(unit.profile.maxHealth));
            Write(mana, unit == null ? "Мана  -" : "Мана  " + Number(unit.mana) + " / " + Number(unit.profile.maxMana));
            Fill(hpFill, unit == null ? 0 : unit.health / unit.profile.maxHealth);
            Fill(mpFill, unit == null || unit.profile.maxMana <= 0 ? 0 : unit.mana / unit.profile.maxMana);
            var stats=player?.combat??default;
            Write(combatStats,stats.known&&unit?.kind==OriginalWorldUnitKind.Hero?
                "Урон "+Number(stats.attackMinimum)+"-"+Number(stats.attackMaximum)+"   Броня "+stats.armor.ToString("0.#")+
                "\nСил "+Number(stats.strength)+"   Лов "+Number(stats.agility)+"   Инт "+Number(stats.intelligence):
                "ПКМ  идти / атаковать\nS  стоп    H  удерживать");
            Write(hint, runtime.ArmedItemInstance != 0 ? "УКАЖИТЕ ЦЕЛЬ ПРЕДМЕТА. ESC / ПКМ - ОТМЕНА." :
                runtime.ArmedSkillId != null ? "УКАЖИТЕ ЦЕЛЬ НАВЫКА. ESC / ПКМ - ОТМЕНА." :
                runtime.AttackTargetArmed ? "ВЫБЕРИТЕ ЦЕЛЬ ЛЕВОЙ КНОПКОЙ МЫШИ. ESC - ОТМЕНА." :
                runtime.SelectedUnit?.kind == OriginalWorldUnitKind.Illusion ? "ВЫБРАНА ИЛЛЮЗИЯ. ЛКМ - ВЫБОР СВОЕГО ЮНИТА. F1 - ГЕРОЙ." :
                unit != null && unit.holding ? "УДЕРЖАНИЕ ПОЗИЦИИ: АТАКА В РАДИУСЕ ОРУЖИЯ, БЕЗ ПРЕСЛЕДОВАНИЯ." :
                "");
            RefreshSkills(playing,view,player,unit);
            if (minimapMarker)
            {
                minimapMarker.gameObject.SetActive(unit != null);
                if (unit != null)
                {
                    var bounds = runtime.map.WorldBounds; var p = runtime.WorldPoint(unit.position);
                    minimapMarker.anchorMin = minimapMarker.anchorMax = new Vector2(Mathf.InverseLerp(bounds.min.x, bounds.max.x, p.x), Mathf.InverseLerp(bounds.min.z, bounds.max.z, p.z));
                }
            }
            RefreshBars(playing && view.hasWorld ? view.world : null);
            RefreshWellLabel(playing);
            RefreshMinimap(playing && view.hasWorld ? view.world : null);
            RefreshMarket(playing && !finished, view, player);
            RefreshQuickItems(playing && !finished, view, player);
            RefreshDuel(playing, view, player);
            RefreshTooltip();
            RefreshMatchOptions(disconnected);
            if(optionsOpen)BlockOptionsUnderlay();
        }

        void RefreshRoster(OriginalSessionView view, OriginalSessionPlayerView local)
        {
            rosterText.Clear(); bool allReady = true;
            bool random = view.options.heroSelection == OriginalHeroSelection.Random || view.options.heroSelection == OriginalHeroSelection.SameRandom;
            foreach (var player in view.players)
            {
                rosterText.Append(player.slot.ToString("00")).Append("   ").Append(random && string.IsNullOrEmpty(player.heroId) ? "Случайный герой" : HeroName(player.heroId));
                if (player.slot == Network.LocalSlot) rosterText.Append("  (вы)");
                rosterText.Append(player.connected ? player.lobbyReady ? "   ·   ГОТОВ" : "   ·   выбор героя" : "   ·   отключён").Append('\n');
                if (!player.connected || !player.lobbyReady || !random && string.IsNullOrEmpty(player.heroId)) allReady = false;
            }
            if ((view.options.heroSelection == OriginalHeroSelection.Free || view.options.heroSelection == OriginalHeroSelection.Random) && view.players.Length > heroIds.Length) allReady = false;
            Write(roster, rosterText.ToString());
            readyControl.interactable = local != null && (random || !string.IsNullOrEmpty(local.heroId));
            Write(readyLabel, local != null && local.lobbyReady ? "ОТМЕНИТЬ ГОТОВНОСТЬ" : "ГОТОВ");
            startButton.SetActive(Network.IsHost); startControl.interactable = allReady;
        }

        static string SelectionLabel(OriginalHeroSelection mode) => mode == OriginalHeroSelection.Random ? "ГЕРОИ: СЛУЧАЙНЫЕ (ДО 3)" :
            mode == OriginalHeroSelection.SameRandom ? "ОДИН СЛУЧАЙНЫЙ ГЕРОЙ (ДО 8)" :
            mode == OriginalHeroSelection.Duplicates ? "ГЕРОИ: С ПОВТОРАМИ (ДО 8)" : "ГЕРОИ: БЕЗ ПОВТОРОВ (ДО 3)";

        void RefreshBars(OriginalWorldSnapshot world)
        {
            seen.Clear();
            if (world != null) foreach (var unit in world.units)
            {
                if (unit.health <= 0 || !runtime.UnitVisible(unit)) continue;
                seen.Add(unit.entityId);
                if (!bars.TryGetValue(unit.entityId, out var fill))
                {
                    var track = Box(barLayer, "Health " + unit.entityId, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Color(.018f, .025f, .03f, .92f), false);
                    track.sizeDelta = new Vector2(unit.ownerSlot != 0 ? 66 : 46, 6);
                    fill = Box(track, "Fill", Vector2.zero, Vector2.one, unit.ownerSlot == 0 ? new Color(.88f, .30f, .26f) : green, false).GetComponent<Image>();
                    bars.Add(unit.entityId, fill);
                }
                var point = runtime.UnitScreenPoint(unit.entityId); var rect = (RectTransform)fill.transform.parent;
                fill.color = OriginalHudRelations.IsEnemy(Network.View, Network.LocalSlot, unit.ownerSlot) ? new Color(.88f, .30f, .26f) : green;
                rect.gameObject.SetActive(point.z > 0 && point.x >= 0 && point.x <= Screen.width && point.y >= 0 && point.y <= Screen.height);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, point, null, out var local)) rect.anchoredPosition = local;
                Fill(fill, unit.health / unit.profile.maxHealth);
            }
            removed.Clear(); foreach (var pair in bars) if (!seen.Contains(pair.Key)) removed.Add(pair.Key);
            foreach (int id in removed) { Destroy(bars[id].transform.parent.gameObject); bars.Remove(id); }
        }

        OriginalSessionPlayerView LocalPlayer(OriginalSessionView view)
        {
            if (view != null && view.players != null) foreach (var player in view.players) if (player.slot == Network.LocalSlot) return player;
            return null;
        }
        string HeroName(string id)
        {
            for (int i = 0; i < heroIds.Length; i++) if (heroIds[i] == id) return heroNames[i];
            return string.IsNullOrEmpty(id) ? "Герой не выбран" : id;
        }
        static string Number(double value) => System.Math.Ceiling(value).ToString("0");
        static string AbilityLabel(OriginalAbilityView ability)
        {
            switch (ability.code)
            {
                case OriginalAbilityUseCode.Passive: return "Пассивный";
                case OriginalAbilityUseCode.NotLearned: return "Не изучен";
                case OriginalAbilityUseCode.Cooldown: return Number(ability.cooldownRemaining) + " с";
                case OriginalAbilityUseCode.NoMana: return "Нет маны";
                case OriginalAbilityUseCode.Busy: return "Применяется";
                case OriginalAbilityUseCode.Dead: return "Погиб";
                case OriginalAbilityUseCode.Paused: return "Ожидание";
                case OriginalAbilityUseCode.RuleUnavailable: return "Недоступно";
                default: return ability.toggledOn ? "Включён" : ability.manaCostKnown ? Number(ability.manaCost)+" маны" : "Мана ?";
            }
        }
        static void Write(Text label, string value) { if (label.text != value) label.text = value; }
        static void Fill(Image image, double fraction) => image.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)fraction), 1);
        static string PhaseName(OriginalMatchPhase phase)
        {
            switch (phase)
            {
                case OriginalMatchPhase.Preparation: return "Подготовка";
                case OriginalMatchPhase.Combat: return "Бой";
                case OriginalMatchPhase.BossCountdown: return "До выхода босса";
                case OriginalMatchPhase.AltarRecovery: return "Возрождение у алтаря";
                case OriginalMatchPhase.DuelPreparation: return "Подготовка дуэли";
                case OriginalMatchPhase.Duel: return "Дуэль";
                case OriginalMatchPhase.Won: return "Победа";
                case OriginalMatchPhase.Lost: return "Поражение";
                default: return "Переход этапа";
            }
        }

        RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        RectTransform Box(Transform parent, string name, Vector2 min, Vector2 max, Color color, bool blocks = true)
        {
            var rect = Rect(parent, name, min, max); var image = rect.gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = blocks; return rect;
        }
        Text Label(Transform parent, string text, int size, Color color, Vector2 min, Vector2 max, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = Rect(parent, "Text", min, max).gameObject.AddComponent<Text>();
            label.font = size>=22?headingFont:font; label.fontSize = Mathf.Max(15,size); label.color = color; label.text = text; label.alignment = alignment;
            label.raycastTarget = false; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }
        Button MakeButton(Transform parent, string text, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action, int size = 15)
        {
            var rect = Box(parent, text, min, max, inset); var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors; colors.highlightedColor = new Color(1.32f, 1.26f, 1.08f); colors.pressedColor = new Color(.8f, .88f, .82f);
            colors.disabledColor = new Color(.66f, .72f, .75f, 1); colors.fadeDuration=.1f; button.colors = colors;
            button.onClick.AddListener(action);
            button.onClick.AddListener(()=>{if(sound)sound.Click();});
            Label(rect, text, size, pale, new Vector2(.04f, .04f), new Vector2(.96f, .96f), TextAnchor.MiddleCenter);
            return button;
        }
        InputField Input(Transform parent, string value, Vector2 min, Vector2 max)
        {
            var rect = Box(parent, "Address input", min, max, inset);
            var input = rect.gameObject.AddComponent<InputField>();
            input.textComponent = Label(rect, "", 17, pale, new Vector2(.035f, .05f), new Vector2(.965f, .95f));
            input.textComponent.supportRichText = false; input.text = value; input.characterLimit = 64;
            return input;
        }
        void OnDestroy()
        {
            if (canvas) Destroy(canvas.gameObject);
            if (ownedEvents) Destroy(ownedEvents);
        }
    }
}

using System.Collections;
using System.Linq;
using System.Reflection;
using Arena.Original;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Arena.Tests
{
    // The shipped scene now uses the original session. The retired five-wave
    // ArenaGame prototype is not the gameplay contract of Lia39Arena.
    public sealed class OriginalArenaGameplayTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        OriginalArenaRuntime runtime;
        Keyboard keyboard;
        Mouse mouse;
        InputSettings.BackgroundBehavior previousBackground;

        [UnitySetUp] public IEnumerator Setup()
        {
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            keyboard.MakeCurrent(); mouse.MakeCurrent();
            yield return SceneManager.LoadSceneAsync("Lia39Arena"); yield return null;
            runtime = Object.FindFirstObjectByType<OriginalArenaRuntime>();
            Assert.That(runtime, Is.Not.Null);
            StartMatch(); yield return null; yield return null;
            Assert.That(runtime.View.phase, Is.EqualTo(OriginalMatchPhase.Preparation));
        }

        void StartMatch()
        {
            var options = OriginalMatchOptions.ForDifficulty(OriginalDifficulty.Standard);
            options.altars = false;
            Assert.That(runtime.Host(options, 0, "127.0.0.1"), Is.True);
            runtime.network.SendCommand(OriginalSessionCommandKind.SelectHero, heroId: "H008");
            runtime.network.SendCommand(OriginalSessionCommandKind.LobbyReady, ready: true);
            runtime.network.SendCommand(OriginalSessionCommandKind.Start);
        }

        [UnityTearDown] public IEnumerator Teardown()
        {
            if (runtime) runtime.Disconnect();
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = previousBackground;
            yield return null;
        }

        IEnumerator Keypress(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key)); yield return null; yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
        }

        IEnumerator ClickUi(UnityEngine.UI.Button button)
        {
            Assert.That(button.interactable, Is.True, button.name);
            var rect=(RectTransform)button.transform;
            var point=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=point});yield return null;yield return null;
        }

        void Pointer(Vector3 world, bool right = false)
        {
            var point = runtime.viewCamera.WorldToScreenPoint(world);
            var state = new MouseState { position = new Vector2(point.x, point.y) };
            if (right) state = state.WithButton(MouseButton.Right);
            InputSystem.QueueStateEvent(mouse, state);
        }

        [UnityTest] public IEnumerator HudSymbolsRenderGeometryAfterCanvasBuild()
        {
            Canvas.ForceUpdateCanvases();yield return null;
            var glyphs=Object.FindAnyObjectByType<OriginalArenaHud>().GetComponentsInChildren<OriginalHudGlyph>();
            Assert.That(glyphs.Length,Is.GreaterThanOrEqualTo(5));
            foreach(var glyph in glyphs)
            {
                var renderer=glyph.GetComponent<CanvasRenderer>();
                Assert.That(renderer,Is.Not.Null,glyph.name+" requires an actual canvas renderer.");
                Assert.That(renderer.GetMesh()?.vertexCount??0,Is.GreaterThan(0),glyph.name+" must produce visible geometry.");
            }
        }

        [UnityTest] public IEnumerator AnimatedModelRemainsVisibleAndCameraFramesHero()
        {
            yield return new WaitForSecondsRealtime(.4f);
            var renderers = runtime.Hero.GetComponentsInChildren<SkinnedMeshRenderer>();
            Assert.That(renderers, Is.Not.Empty);
            var bounds = new Bounds(); bool any = false;
            foreach (var renderer in renderers)
            {
                var mesh = new Mesh(); renderer.BakeMesh(mesh, true);
                foreach (var vertex in mesh.vertices)
                {
                    var point = renderer.transform.TransformPoint(vertex);
                    if (!any) { bounds = new Bounds(point, Vector3.zero); any = true; }
                    else bounds.Encapsulate(point);
                }
                Object.Destroy(mesh);
                Assert.That(renderer.sharedMaterials.All(m => m && m.shader.isSupported), Is.True);
            }
            Assert.That(bounds.size.y, Is.InRange(1.5f, 2.3f));
            Assert.That(renderers[0].bounds.size.y, Is.InRange(1.5f, 2.5f));
            Assert.That(bounds.min.y - runtime.Hero.position.y, Is.InRange(-.15f, .25f));
            var visible = runtime.viewCamera.WorldToViewportPoint(runtime.Hero.position);
            Assert.That(visible.y, Is.InRange(.18f, .81f));
            Assert.That(visible.x, Is.InRange(.2f, .8f));
            Assert.That(runtime.Hero.GetComponentInChildren<Animation>().IsPlaying("Idle"), Is.True);
        }

        [UnityTest] public IEnumerator PreparationAllowsMovementOverviewAndMenuWithoutPausingMatch()
        {
            yield return new WaitForSecondsRealtime(.3f);
            var start = runtime.Hero.position;
            var target = runtime.map.FindNearestWalkable(start + Vector3.back * 3);
            Pointer(target, true); yield return null; Pointer(target);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Vector3.Distance(start, runtime.Hero.position), Is.GreaterThan(.5f));
            Assert.That(runtime.View.phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            Assert.That(runtime.View.world.units.Count(u => u.ownerSlot == 0), Is.Zero);
            yield return Keypress(Key.S);
            runtime.network.SendCommand(OriginalSessionCommandKind.LearnSkill, skillId:runtime.View.players[0].learning[0].id);
            yield return null; yield return null;
            Assert.That(runtime.SelectedUnitAbilities[0].code, Is.EqualTo(OriginalAbilityUseCode.Ready));
            double beforeMenu = runtime.View.time;
            yield return Keypress(Key.F10);
            Assert.That(runtime.InputBlocked, Is.True);
            Assert.That(runtime.RequestSkill(0), Is.False, "Direct skill buttons must respect the menu input gate.");
            Assert.That(runtime.ArmedSkillId, Is.Null);
            var stopped = runtime.LocalUnit.position;
            Pointer(start, true); yield return null; Pointer(start);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(runtime.LocalUnit.position.x, Is.EqualTo(stopped.x).Within(.01));
            Assert.That(runtime.LocalUnit.position.y, Is.EqualTo(stopped.y).Within(.01));
            Assert.That(runtime.View.time, Is.GreaterThan(beforeMenu), "A co-op match keeps running in the menu.");
            yield return Keypress(Key.F10);
            Assert.That(runtime.InputBlocked, Is.False);
            yield return Keypress(Key.Tab);
            Assert.That(runtime.cameraControl.Overview, Is.True);
            yield return Keypress(Key.Tab);
            Assert.That(runtime.cameraControl.Overview, Is.False);
            yield return Keypress(Key.Space);
            yield return new WaitForSecondsRealtime(.4f);
            var visible = runtime.viewCamera.WorldToViewportPoint(runtime.Hero.position);
            Assert.That(visible.y, Is.InRange(.18f, .81f));
            Assert.That(runtime.View.haltReason, Is.Null.Or.Empty);
        }

        [UnityTest] public IEnumerator LobbyRulesButtonsReclassifyAndPresetsRestoreSourceSettings()
        {
            runtime.Disconnect(); yield return null; yield return null;
            var hud=Object.FindFirstObjectByType<OriginalArenaHud>();
            var open=(UnityEngine.UI.Button)typeof(OriginalArenaHud).GetField("matchRulesControl",Private).GetValue(hud);
            yield return ClickUi(open);
            var panel=(GameObject)typeof(OriginalArenaHud).GetField("matchOptionsPanel",Private).GetValue(hud);
            Assert.That(panel.activeSelf, Is.True);
            Assert.That(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.IsChildOf(panel.transform), Is.True);
            yield return Keypress(Key.RightArrow);
            Assert.That(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.IsChildOf(panel.transform), Is.True);
            Assert.That(runtime.network.State, Is.EqualTo(OriginalConnectionState.Disconnected));
            var rules=panel.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.name.StartsWith("Match rule ")).ToArray();
            Assert.That(rules.Length, Is.EqualTo(10));
            var select=typeof(OriginalArenaHud).GetMethod("SelectedOptions",Private);
            yield return ClickUi(rules.Single(b=>b.name=="Match rule 0"));
            var chosen=(OriginalMatchOptions)select.Invoke(hud,null);
            Assert.That(chosen.altars, Is.False);
            Assert.That((int)chosen.difficulty, Is.Zero);
            yield return ClickUi(rules.Single(b=>b.name=="Match rule 0"));
            Assert.That(((OriginalMatchOptions)select.Invoke(hud,null)).difficulty, Is.EqualTo(OriginalDifficulty.Standard));
            yield return ClickUi(rules.Single(b=>b.name=="Match rule 8"));
            chosen=(OriginalMatchOptions)select.Invoke(hud,null);
            Assert.That(chosen.returnToCenter, Is.False);
            Assert.That(chosen.difficulty, Is.EqualTo(OriginalDifficulty.Standard));
            yield return ClickUi(panel.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b=>b.name=="ВЕРНУТЬСЯ В ЛОББИ"));
            Assert.That(panel.activeSelf, Is.False);
            var presets=(UnityEngine.UI.Button[])typeof(OriginalArenaHud).GetField("difficultyButtons",Private).GetValue(hud);
            yield return ClickUi(presets[2]);
            chosen=(OriginalMatchOptions)select.Invoke(hud,null);
            Assert.That(chosen.difficulty, Is.EqualTo(OriginalDifficulty.Extreme));
            Assert.That(chosen.altars, Is.False); Assert.That(chosen.runes, Is.False);
            Assert.That(chosen.explosiveBarrels, Is.False);
            Assert.That(chosen.defensiveBarrels, Is.EqualTo(OriginalDefensiveBarrels.Attackable));
            Assert.That(chosen.returnToCenter, Is.False, "World layout choices are independent of the eight preset switches.");
            chosen.runes=true;
            Assert.That(((OriginalMatchOptions)select.Invoke(hud,null)).runes, Is.False, "A new host gets an independent options copy.");
        }

        [UnityTest] public IEnumerator AuthoritativeDeathAndNewMatchRemoveStaleActors()
        {
            var hud = Object.FindFirstObjectByType<OriginalArenaHud>();
            yield return Keypress(Key.B);
            var market = (GameObject)typeof(OriginalArenaHud).GetField("marketPanel", Private).GetValue(hud);
            Assert.That(market.activeSelf, Is.True);
            var session = (OriginalSession)typeof(OriginalNetworkGame).GetField("session", Private).GetValue(runtime.network);
            var world = (OriginalWorld)typeof(OriginalSession).GetField("world", Private).GetValue(session);
            runtime.network.SendCommand(OriginalSessionCommandKind.WaveReady, ready: true);
            // Controlled transition/death fixture, not a claim about player survival.
            for (int i = 0; i < 120 && session.Snapshot().phase != OriginalMatchPhase.Combat; i++)
                runtime.network.AdvanceHost(1);
            Assert.That(session.Snapshot().phase, Is.EqualTo(OriginalMatchPhase.Combat));
            yield return null; yield return null;
            Assert.That(runtime.View.world.units.Any(u => u.ownerSlot == 0), Is.True);
            var hero = world.UnitState(1);
            Assert.That(world.ApplyUnitDamage(hero.entityId, hero.health), Is.True);
            Assert.That(session.ReportHeroDied(1), Is.True);
            float deadline = Time.realtimeSinceStartup + 3;
            while (runtime.View.phase != OriginalMatchPhase.Lost && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(runtime.View.phase, Is.EqualTo(OriginalMatchPhase.Lost));
            yield return null; yield return null;
            Assert.That(runtime.LocalUnit.health, Is.Zero);
            Assert.That(market.activeSelf, Is.False, "The shop must not cover the match result.");
            Assert.That(runtime.InputBlocked, Is.True);
            runtime.Disconnect(); yield return null; yield return null;
            Assert.That(Object.FindObjectsByType<ArenaActor>(FindObjectsSortMode.None), Is.Empty);
            StartMatch(); yield return null; yield return null;
            Assert.That(runtime.View.phase, Is.EqualTo(OriginalMatchPhase.Preparation));
            Assert.That(runtime.LocalUnit.health, Is.EqualTo(631));
            Assert.That(runtime.View.world.units.Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<ArenaActor>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(runtime.View.haltReason, Is.Null.Or.Empty);
        }
    }
}

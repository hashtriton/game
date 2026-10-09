using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
    public static class HeroProbeCapture
    {
        private const string Evidence = "../.local/codex-tasks/hero-rig/shots-p4/";
        private static string ActiveEvidence => SessionState.GetBool("HeroP5Capture", false) ? "../.local/codex-tasks/hero-p5/shots/" : Evidence;
        private static readonly List<object> motionFrames = new List<object>();
        private static bool motionDone;
        private static string shot;
        private static string clipName;
        private static HeroController controller;
        private static Unit hero;
        private static Animation animation;
        private static int armedFrame;
        private static int frozenFrame;
        private static float startTime;
        private static double deadline;
        private static Vector3 startPosition;
        private static string failure;
        private static bool ready;

        public static object PrepareArena(string variant)
        {
            if (!new[] { "P5", "P4", "Placeholder" }.Contains(variant)) throw new ArgumentException(variant);
            var result = PrepareArena(variant == "P4");
            SessionState.SetBool("HeroP5Capture", true);
            SessionState.SetString("HeroP4CaptureVariant", variant);
            if (variant == "P5")
            {
                var unit = UnityEngine.Object.FindAnyObjectByType<HeroController>().GetComponent<Unit>();
                unit.modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/P5/HeroProbeP5.prefab");
                if (unit.modelPrefab == null) throw new InvalidOperationException("Missing P5 probe.");
            }
            return new { variant, preparation = result };
        }

        public static object BeginMotion(string name)
        {
            if (name != "Run" && name != "Attack") throw new ArgumentException(name);
            Arm(name);
            EditorApplication.update -= FreezeWhenReady;
            controller.enabled = false;
            hero.enabled = false;
            hero.Actor.enabled = false;
            animation.enabled = true;
            animation.Play(name);
            animation[name].time = 0;
            motionDone = false;
            motionFrames.Clear();
            EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            controller.StartCoroutine(CaptureMotionFrames(name));
            return new { clip = name, samples = 12, playbackSpeed = .1f };
        }

        private static System.Collections.IEnumerator CaptureMotionFrames(string name)
        {
            var state = animation[name];
            var previousSpeed = state.speed;
            state.speed = .1f;
            var folder = "../.local/codex-tasks/hero-p5/motion/" + name + "/";
            Directory.CreateDirectory(folder);
            for (var index = 0; index < 12; index++)
            {
                var targetPhase = .01f + index * .97f / 11f;
                do { yield return new WaitForEndOfFrame(); } while (state.time / state.length < targetPhase);
                var image = ScreenCapture.CaptureScreenshotAsTexture();
                try { File.WriteAllBytes(folder + index.ToString("D2") + ".png", image.EncodeToPNG()); }
                finally { UnityEngine.Object.Destroy(image); }
                var center = controller.viewCamera.WorldToScreenPoint(hero.transform.position + Vector3.up * 1.2f);
                motionFrames.Add(new { index, targetPhase, actualPhase = state.time / state.length, frame = Time.frameCount,
                    center = new[] { center.x, Screen.height - center.y }, width = Screen.width, height = Screen.height,
                    playing = animation.IsPlaying(name), speed = state.speed });
            }
            state.speed = previousSpeed;
            File.WriteAllText(folder + "facts.json", Newtonsoft.Json.JsonConvert.SerializeObject(motionFrames, Newtonsoft.Json.Formatting.Indented));
            motionDone = true;
        }

        public static object MotionStatus() => new { done = motionDone, frames = motionFrames.Count, frame = Time.frameCount };

        public static object PrepareArena(bool p4)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before preparing the arena.");
            if (EditorSceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Preparation requires a clean scene.");
            EditorSceneManager.OpenScene("Assets/Game/Scenes/Arena.unity", OpenSceneMode.Single);
            var unit = UnityEngine.Object.FindAnyObjectByType<HeroController>().GetComponent<Unit>();
            var original = AssetDatabase.GetAssetPath(unit.modelPrefab);
            if (p4) unit.modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/HeroProbe/P4/HeroProbeP4.prefab");
            if (unit.modelPrefab == null) throw new InvalidOperationException("Missing preview prefab.");
            SessionState.SetString("HeroP4CaptureVariant", p4 ? "P4" : "Placeholder");
            SetPreset(3);
            return new { variant = p4 ? "P4" : "Placeholder", originalPrefab = original, previewPrefab = AssetDatabase.GetAssetPath(unit.modelPrefab), scene = EditorSceneManager.GetActiveScene().path };
        }

        public static object Arm(string name)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Capture requires Play Mode.");
            if (!new[] { "Idle", "Run", "Attack", "WideArena", "WideBarrels", "Death" }.Contains(name)) throw new ArgumentException(name);
            EditorApplication.update -= FreezeWhenReady;
            shot = name;
            ready = false;
            failure = null;
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            if (hud == null || !hud.session.Ready) throw new InvalidOperationException("Wait for the real HUD and item data.");
            ArenaVisualPreview.Frame(name == "WideBarrels" ? "corner" : "arena");
            controller = hud.hero;
            hero = controller.Unit;
            if (!hero.IsAlive) throw new InvalidOperationException("Death must be the final shot of the session.");
            var map = controller.map;
            var origin = name == "WideBarrels" ? map.FindNearestWalkable(map.WcToWorld(1408f, 2112f), hero.pathRadius) : map.HeroSpawn;
            hero.transform.position = origin;
            hero.Actor.Face(new Vector3(.57f, 0, -.82f), 1f);
            hero.enabled = true;
            hero.Actor.enabled = true;
            animation = hero.GetComponentInChildren<Animation>();
            animation.enabled = true;
            var creeps = UnityEngine.Object.FindObjectsByType<Unit>().Where(u => u.faction == Faction.Creep).OrderBy(u => u.name).ToArray();
            var offsets = name == "Attack" ? new[] { new Vector3(1.5f, 0, -1f), new Vector3(-4, 0, 4), new Vector3(4, 0, 4) }
                : new[] { new Vector3(4.5f, 0, 3.5f), new Vector3(-4, 0, 5.5f), new Vector3(3.5f, 0, -2.5f) };
            for (var i = 0; i < creeps.Length; i++)
            {
                var creep = creeps[i];
                creep.transform.position = map.FindNearestWalkable(origin + offsets[i % offsets.Length], creep.pathRadius);
                creep.enabled = false;
                creep.Actor.enabled = false;
                var playback = creep.GetComponentInChildren<Animation>();
                if (playback != null)
                {
                    playback.enabled = false;
                    playback.GetClip("Idle").SampleAnimation(playback.gameObject, .25f);
                }
            }
            controller.Stop();
            controller.enabled = false;
            var camera = controller.viewCamera;
            var cameraControl = camera.GetComponent<RtsCamera>();
            cameraControl.enabled = false;
            var wide = name.StartsWith("Wide");
            var focus = origin + (name == "WideArena" ? Vector3.forward * 3.5f : Vector3.zero);
            focus.y = map.SampleHeight(focus);
            var rotation = Quaternion.Euler(cameraControl.pitch, 0, 0);
            camera.transform.SetPositionAndRotation(focus - rotation * Vector3.forward * (wide ? name == "WideArena" ? 28f : 20f : 19f), rotation);
            hud.transform.Find("Shop window").gameObject.SetActive(false);
            hud.Say("", 0f);
            hero.health = hero.MaxHealth;
            hero.mana = hero.MaxMana;
            hero.healthRegen = hero.manaRegen = 0f;
            clipName = name == "Run" || name == "Attack" || name == "Death" ? name : "Idle";
            hero.Actor.SetMoving(false);
            animation.Play("Idle");
            animation["Idle"].time = 0;
            animation.Sample();
            Time.timeScale = 1f;
            startPosition = hero.transform.position;
            if (name == "Run")
            {
                controller.enabled = true;
                controller.OrderMove(map.FindNearestWalkable(origin + new Vector3(3, 0, 5), hero.pathRadius), false);
            }
            else if (name == "Attack")
            {
                hero.Actor.Face(creeps[0].transform.position - hero.transform.position, 1f);
                hero.BeginAttack(creeps[0]);
            }
            else if (name == "Death") hero.ApplyDamage(100000f, null);
            startTime = Time.time;
            armedFrame = Time.frameCount;
            deadline = EditorApplication.timeSinceStartup + 20;
            EditorApplication.update += FreezeWhenReady;
            EditorApplication.QueuePlayerLoopUpdate();
            return new { shot, clipName, armedFrame, startTime, startPosition = Vector(startPosition) };
        }

        private static void FreezeWhenReady()
        {
            if (!EditorApplication.isPlaying || hero == null || EditorApplication.timeSinceStartup > deadline)
            {
                failure = "Capture did not reach the requested live animation phase.";
                EditorApplication.update -= FreezeWhenReady;
                return;
            }
            var state = animation[clipName];
            var phase = clipName == "Death" ? .9f : clipName == "Run" || clipName == "Attack" ? .25f : .01f;
            if (Time.frameCount <= armedFrame + 2 || !animation.IsPlaying(clipName) || state.time < state.length * phase) return;
            controller.enabled = false;
            hero.enabled = false;
            hero.Actor.enabled = false;
            animation.enabled = false;
            Time.timeScale = 0;
            // Freeze at a common phase after proving live playback, then let GPU skinning catch up.
            animation.GetClip(clipName).SampleAnimation(animation.gameObject, state.length * phase);
            frozenFrame = Time.frameCount;
            ready = true;
            EditorApplication.update -= FreezeWhenReady;
            Canvas.ForceUpdateCanvases();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        public static object Status()
        {
            return new { shot, ready, failure, armedFrame, frozenFrame, frame = Time.frameCount, clipName,
                elapsed = Time.time - startTime, position = hero == null ? null : Vector(hero.transform.position) };
        }

        public static object Record()
        {
            if (!ready || failure != null || Time.frameCount <= frozenFrame) throw new InvalidOperationException("Wait for a rendered player frame after the frozen pose.");
            var camera = controller.viewCamera;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var feetMin = float.MaxValue;
            var allMin = float.MaxValue;
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            try
            {
                foreach (var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    mesh.Clear();
                    renderer.BakeMesh(mesh, true);
                    mesh.GetVertices(vertices);
                    foreach (var vertex in vertices)
                    {
                        var world = renderer.transform.TransformPoint(vertex);
                        var screen = camera.WorldToScreenPoint(world);
                        min = Vector2.Min(min, new Vector2(screen.x, Screen.height - screen.y));
                        max = Vector2.Max(max, new Vector2(screen.x, Screen.height - screen.y));
                        allMin = Mathf.Min(allMin, world.y);
                        if (renderer.name.StartsWith("HB_Boot_") || renderer.name.StartsWith("HB_Sabaton_")) feetMin = Mathf.Min(feetMin, world.y);
                    }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(mesh); }
            var variant = SessionState.GetString("HeroP4CaptureVariant", "unknown");
            var result = new { variant, shot, clipName, armedFrame, frozenFrame, capturedFrame = Time.frameCount,
                liveElapsed = Time.time - startTime, liveDisplacement = Vector3.Distance(startPosition, hero.transform.position),
                width = Screen.width, height = Screen.height, cameraFov = camera.fieldOfView,
                cameraPosition = Vector(camera.transform.position), cameraRotation = Vector(camera.transform.eulerAngles),
                heroPosition = Vector(hero.transform.position), heroRotation = Vector(hero.transform.eulerAngles),
                heroBox = new[] { min.x, min.y, max.x, max.y }, allMin,
                feetMin = feetMin == float.MaxValue ? (float?)null : feetMin,
                floor = controller.map.SampleHeight(hero.transform.position), realHud = UnityEngine.Object.FindAnyObjectByType<GameHud>() != null,
                isDead = hero.Actor.IsDead, modelPrefab = AssetDatabase.GetAssetPath(hero.modelPrefab) };
            Directory.CreateDirectory(ActiveEvidence);
            File.WriteAllText(ActiveEvidence + variant + "-" + shot + ".json", Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented));
            return result;
        }

        public static object Restore()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before restoring the arena.");
            EditorApplication.update -= FreezeWhenReady;
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/Arena.unity", OpenSceneMode.Single);
            var preset = SessionState.GetInt("HeroP4OriginalPreset", 0);
            SetPreset(preset);
            return new { scene = scene.path, dirty = scene.isDirty, preset };
        }

        public static object RestoreP5()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before restoring.");
            EditorApplication.update -= FreezeWhenReady;
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/Arena.unity", OpenSceneMode.Single);
            var preset = SessionState.GetInt("HeroP5OriginalPreset", 0);
            SetPreset(preset);
            SessionState.SetBool("HeroP5Capture", false);
            return new { scene = scene.path, dirty = scene.isDirty, preset };
        }

        private static float[] Vector(Vector3 value) => new[] { value.x, value.y, value.z };

        private static void SetPreset(int index)
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var view = EditorWindow.GetWindow(type);
            type.GetProperty("selectedSizeIndex", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SetValue(view, index);
        }
    }
}

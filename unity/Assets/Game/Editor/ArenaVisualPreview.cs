using System;
using System.Linq;
using Arena;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    // Framing changes live only in Play Mode and are discarded when Play ends.
    public static class ArenaVisualPreview
    {
        public static void Frame(string view)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Preview requires Play Mode.");
            if (view != "arena" && view != "closeup" && view != "corner")
                throw new ArgumentException("Unknown preview view: " + view, nameof(view));
            var map = UnityEngine.Object.FindAnyObjectByType<ArenaMap>();
            var hero = UnityEngine.Object.FindAnyObjectByType<HeroController>();
            if (map == null || hero == null) throw new InvalidOperationException("Preview requires an arena map and hero.");
            var heroUnit = hero.Unit;
            if (heroUnit == null) throw new InvalidOperationException("Preview hero requires a Unit.");
            var camera = hero.viewCamera;
            if (camera == null) throw new InvalidOperationException("Preview hero requires a view camera.");
            var cameraControl = camera.GetComponent<RtsCamera>();
            if (cameraControl == null) throw new InvalidOperationException("Preview camera requires an RtsCamera.");
            if (hero.map != map || cameraControl.map != map)
                throw new InvalidOperationException("Preview hero and camera must reference the arena map.");

            var origin = map.HeroSpawn;
            var focus = origin + Vector3.forward * 3.5f;
            var distance = 28f;
            if (view == "closeup")
            {
                focus = origin + new Vector3(0.5f, 0f, 1.5f);
                distance = 15.5f;
            }
            else if (view == "corner")
            {
                focus = map.WcToWorld(1408f, 2112f);
                distance = 20f;
            }
            focus.y = map.SampleHeight(focus);
            var rotation = Quaternion.Euler(cameraControl.pitch, 0f, 0f);
            var units = UnityEngine.Object.FindObjectsByType<Unit>();
            cameraControl.enabled = false;
            hero.enabled = false;
            foreach (var ai in UnityEngine.Object.FindObjectsByType<CreepAi>()) ai.enabled = false;
            hero.transform.position = origin;
            hero.transform.rotation = Quaternion.Euler(0f, 35f, 0f);
            var creeps = units.Where(u => u.faction == Faction.Creep).OrderBy(u => u.name).ToArray();
            var offsets = new[] { new Vector3(4.5f, 0f, 3.5f), new Vector3(-4f, 0f, 5.5f), new Vector3(3.5f, 0f, -2.5f) };
            for (var i = 0; i < creeps.Length; i++)
            {
                var creep = creeps[i];
                creep.transform.position = map.FindNearestWalkable(origin + offsets[i % offsets.Length], creep.pathRadius);
                creep.transform.rotation = Quaternion.LookRotation(origin - creep.transform.position);
                SamplePose(creep, "Idle", 0.25f);
            }
            SamplePose(heroUnit, "Attack", 0.3f);
            camera.transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
            Time.timeScale = 0f;
            Canvas.ForceUpdateCanvases();
        }

        static void SamplePose(Unit unit, string clip, float normalizedTime)
        {
            var animation = unit.GetComponentInChildren<Animation>();
            if (animation == null || animation[clip] == null) return;
            animation.Play(clip);
            animation[clip].normalizedTime = normalizedTime;
            animation.Sample();
            animation.enabled = false;
        }
    }
}

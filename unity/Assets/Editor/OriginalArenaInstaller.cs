using System;
using System.Collections.Generic;
using System.Linq;
using Arena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Converts the currently loaded measured arena. Never rebuilds terrain or art.
public static class OriginalArenaInstaller
{
    const string ScenePath = "Assets/Arena/Scenes/Lia39Arena.unity";
    static readonly string[] DataNames =
    {
        "lia39-match", "lia39-items", "lia39-combat", "lia39-item-passives",
        "lia39-duels", "lia39-native126", "lia39-layout", "lia39-observed126", "lia39-observed-items126"
    };

    [MenuItem("Game/Install original arena runtime")]
    public static void InstallCurrentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode before installing the original runtime.");
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
            throw new InvalidOperationException("Open and activate " + ScenePath + " before installing.");

        // Resolve and validate every required reference before removing legacy
        // components. The original scene geometry and prefab assets stay intact.
        var oldGames = Components<ArenaGame>(scene);
        var runtimes = Components<OriginalArenaRuntime>(scene);
        if (oldGames.Length > 1 || runtimes.Length > 1)
            throw new InvalidOperationException("Expected one arena game system in the active scene.");
        ArenaGame old = oldGames.FirstOrDefault();
        OriginalArenaRuntime existing = runtimes.FirstOrDefault();
        if (!old && !existing) throw new InvalidOperationException("The current scene has no arena game references to preserve.");
        ArenaMap map = old ? old.arenaMap : existing.map;
        Camera camera = old ? old.arenaCamera : existing.viewCamera;
        GameObject hero = old ? old.heroPrefab : existing.heroPrefab;
        GameObject melee = old ? old.meleePrefab : existing.meleePrefab;
        Material effects = old ? old.effectMaterial : existing.effectMaterial;
        var oldHud = Components<ArenaHud>(scene).FirstOrDefault();
        var currentHud = Components<OriginalArenaHud>(scene).FirstOrDefault();
        Texture2D minimap = oldHud ? oldHud.mapTexture : currentHud ? currentHud.mapTexture : null;
        if (!minimap) minimap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Arena/Generated/Lia39Minimap.png");
        if (!map || !camera || !hero || !melee || !minimap || !map.layoutJson)
            throw new InvalidOperationException("The arena needs its existing map, camera, hero, melee prefab and minimap.");
        if (map.gameObject.scene != scene || camera.gameObject.scene != scene)
            throw new InvalidOperationException("Map and camera references must belong to the current arena scene.");
        var data = new TextAsset[DataNames.Length];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/" + DataNames[i] + ".json");
            if (!data[i]) throw new InvalidOperationException("Missing game data: " + DataNames[i]);
        }
        OriginalGameCatalogs.Load(data);

        Undo.IncrementCurrentGroup(); int undo = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Install original arena runtime");
        GameObject systems = old ? old.gameObject : existing.gameObject;
        foreach (var component in oldGames) Undo.DestroyObjectImmediate(component);
        foreach (var component in Components<ArenaHud>(scene)) Undo.DestroyObjectImmediate(component);
        foreach (var component in Components<ArenaMapCamera>(scene)) Undo.DestroyObjectImmediate(component);
        var network = systems.GetComponent<OriginalNetworkGame>();
        if (!network) network = Undo.AddComponent<OriginalNetworkGame>(systems);
        Undo.RecordObject(network, "Configure original catalogs"); network.dataAssets = data; network.enabled = true;
        var runtime = existing ? existing : Undo.AddComponent<OriginalArenaRuntime>(systems);
        Undo.RecordObject(runtime, "Configure original presenter");
        runtime.network = network; runtime.map = map; runtime.viewCamera = camera;
        runtime.heroPrefab = hero; runtime.meleePrefab = melee; runtime.effectMaterial = effects; runtime.enabled = true;
        var cameraControl = camera.GetComponent<OriginalArenaCamera>();
        if (!cameraControl) cameraControl = Undo.AddComponent<OriginalArenaCamera>(camera.gameObject);
        Undo.RecordObject(cameraControl, "Configure original camera");
        cameraControl.map = map; cameraControl.hero = null; cameraControl.inputBlocked = false; cameraControl.enabled = true;
        runtime.cameraControl = cameraControl;
        var hud = currentHud ? currentHud : Undo.AddComponent<OriginalArenaHud>(systems);
        Undo.RecordObject(hud, "Configure original HUD"); hud.runtime = runtime; hud.mapTexture = minimap; hud.enabled = true;
        EditorUtility.SetDirty(network); EditorUtility.SetDirty(runtime); EditorUtility.SetDirty(cameraControl); EditorUtility.SetDirty(hud);
        Undo.CollapseUndoOperations(undo);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the installed arena scene.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        Debug.Log("ORIGINAL_ARENA_INSTALLED scene=" + scene.path + " preservedHero=" + hero.name +
            " preservedCreep=" + melee.name + " map=" + map.name + " dataAssets=" + data.Length);
    }

    static T[] Components<T>(Scene scene) where T : Component
    {
        var result = new List<T>();
        foreach (var root in scene.GetRootGameObjects()) result.AddRange(root.GetComponentsInChildren<T>(true));
        return result.ToArray();
    }
}

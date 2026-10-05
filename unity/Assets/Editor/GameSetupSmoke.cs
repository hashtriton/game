using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameSetupSmoke
{
    public static void Run()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "GameSetupSmokeCube";
        cube.transform.position = new Vector3(0, 1, 0);
        var cameraObject = new GameObject("GameSetupSmokeCamera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.transform.position = new Vector3(6, 6, -6);
        cameraObject.transform.LookAt(cube.transform);
        var lightObject = new GameObject("GameSetupSmokeLight");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
        Directory.CreateDirectory("Assets/Smoke");
        if (!EditorSceneManager.SaveScene(scene, "Assets/Smoke/SetupSmoke.unity"))
            throw new InvalidOperationException("Smoke scene save failed");
        AssetDatabase.SaveAssets();
        var report = "{\"test\":\"unity_batch_compile_scene_save\",\"passed\":true,\"unity_version\":\""
            + Application.unityVersion + "\",\"scene\":\"Assets/Smoke/SetupSmoke.unity\",\"root_objects\":"
            + scene.rootCount + ",\"mcp_tested\":false}";
        File.WriteAllText(Path.GetFullPath("../verification/unity-batch-smoke.json"), report + Environment.NewLine);
        Debug.Log("GAME_SETUP_SMOKE_PASSED " + report);
    }
}

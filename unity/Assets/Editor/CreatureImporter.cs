using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Imports Blender creature exports (tools/creatures/export.py) into URP materials and legacy-animated prefabs.</summary>
public static class CreatureImporter
{
    public const string Root = "Assets/Creatures";
    public const string ShowcasePath = Root + "/CreatureShowcase.unity";
    public static readonly string[] Clips = { "Idle", "Run", "Attack", "Death" };

    [Serializable] class Settings { public string title; public float alpha = 1; public float emission = 2; }

    public static string[] ImportAll()
    {
        var names = Directory.GetDirectories(Root).Select(Path.GetFileName)
            .Where(n => File.Exists($"{Root}/{n}/{n}.fbx") && File.Exists($"{Root}/{n}/{n}.creature.json")).OrderBy(n => n).ToArray();
        foreach (var n in names) Import(n);
        AssetDatabase.SaveAssets();
        return names;
    }

    public static GameObject Import(string name)
    {
        string dir = Root + "/" + name, fbx = dir + "/" + name + ".fbx";
        var settings = JsonUtility.FromJson<Settings>(File.ReadAllText(dir + "/" + name + ".creature.json"));
        ConfigureTexture(dir + "/" + name + "_Albedo.png", TextureImporterType.Default, true);
        ConfigureTexture(dir + "/" + name + "_Emission.png", TextureImporterType.Default, true);
        ConfigureTexture(dir + "/" + name + "_Normal.png", TextureImporterType.NormalMap, false);
        ConfigureTexture(dir + "/" + name + "_Mask.png", TextureImporterType.Default, false);

        var importer = (ModelImporter)AssetImporter.GetAtPath(fbx) ?? throw new FileNotFoundException(fbx);
        importer.animationType = ModelImporterAnimationType.Legacy;
        importer.importAnimation = true;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.importCameras = false;
        importer.importLights = false;
        importer.bakeAxisConversion = true;
        importer.globalScale = 1;
        importer.useFileScale = true;
        importer.SaveAndReimport();

        var material = BuildMaterial(dir, name, settings);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        var instance = UnityEngine.Object.Instantiate(model);
        instance.name = name;
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
            r.sharedMaterials = r.sharedMaterials.Select(_ => material).ToArray();
        foreach (var animator in instance.GetComponentsInChildren<Animator>()) UnityEngine.Object.DestroyImmediate(animator);
        var animation = instance.GetComponent<Animation>() ?? instance.AddComponent<Animation>();
        foreach (AnimationState state in animation) animation.RemoveClip(state.clip);
        var sources = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__")).ToArray();
        for (int i = 0; i < Clips.Length; i++)
        {
            var original = sources.FirstOrDefault(c => c.name.Split('|').Last() == Clips[i])
                ?? throw new InvalidOperationException(name + " missing animation " + Clips[i]);
            string clipPath = dir + "/" + name + "_" + Clips[i] + ".anim";
            var copy = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (!copy) { copy = UnityEngine.Object.Instantiate(original); AssetDatabase.CreateAsset(copy, clipPath); }
            else EditorUtility.CopySerialized(original, copy);
            copy.name = Clips[i]; copy.legacy = true;
            copy.wrapMode = i < 2 ? WrapMode.Loop : i == 3 ? WrapMode.ClampForever : WrapMode.Once;
            animation.AddClip(copy, Clips[i]); EditorUtility.SetDirty(copy);
            if (i == 0) animation.clip = copy;
        }
        animation.playAutomatically = true;
        animation.cullingType = AnimationCullingType.AlwaysAnimate;
        foreach (var smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>()) smr.updateWhenOffscreen = true;
        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, dir + "/" + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(instance);
        return prefab;
    }

    static void ConfigureTexture(string path, TextureImporterType type, bool srgb)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(path) ?? throw new FileNotFoundException(path);
        ti.textureType = type; ti.sRGBTexture = srgb; ti.mipmapEnabled = true;
        ti.alphaSource = path.EndsWith("_Mask.png") ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
        ti.maxTextureSize = 2048; ti.textureCompression = TextureImporterCompression.CompressedHQ;
        ti.SaveAndReimport();
    }

    static Material BuildMaterial(string dir, string name, Settings s)
    {
        string path = dir + "/" + name + ".mat";
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? throw new InvalidOperationException("URP Lit missing");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
        mat.shader = shader;
        Texture2D Tex(string k) => AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/" + name + "_" + k + ".png");
        mat.SetTexture("_BaseMap", Tex("Albedo"));
        mat.SetColor("_BaseColor", new Color(1, 1, 1, s.alpha));
        mat.SetTexture("_BumpMap", Tex("Normal")); mat.SetFloat("_BumpScale", 1); mat.EnableKeyword("_NORMALMAP");
        mat.SetTexture("_MetallicGlossMap", Tex("Mask")); mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        mat.SetFloat("_Smoothness", 1); mat.SetFloat("_SmoothnessTextureChannel", 0);
        mat.SetTexture("_EmissionMap", Tex("Emission"));
        mat.SetColor("_EmissionColor", Color.white * s.emission); mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        bool transparent = s.alpha < .999f;
        mat.SetFloat("_Surface", transparent ? 1 : 0);
        mat.SetFloat("_Blend", 0);
        mat.SetFloat("_SrcBlend", (float)(transparent ? BlendMode.SrcAlpha : BlendMode.One));
        mat.SetFloat("_DstBlend", (float)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
        mat.SetFloat("_ZWrite", transparent ? 0 : 1);
        mat.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
        if (transparent) mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); else mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = transparent ? (int)RenderQueue.Transparent : -1;
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    /// <summary>Builds the 3x3 showcase as an additive scene so the currently open scene stays untouched.</summary>
    public static string BuildShowcase()
    {
        var names = ImportedNames();
        var active = SceneManager.GetActiveScene();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            var lightGo = new GameObject("Sun");
            SceneManager.MoveGameObjectToScene(lightGo, scene);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.6f;
            light.shadows = LightShadows.Soft; lightGo.transform.rotation = Quaternion.Euler(50, 150, 0);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Ground";
            SceneManager.MoveGameObjectToScene(ground, scene);
            ground.transform.localScale = new Vector3(4, 1, 4);
            ground.GetComponent<Renderer>().sharedMaterial = GroundMaterial();
            for (int i = 0; i < names.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{names[i]}/{names[i]}.prefab");
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.transform.position = Slot(i);
            }
            EditorSceneManager.SaveScene(scene, ShowcasePath);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
            if (active.IsValid()) SceneManager.SetActiveScene(active);
        }
        return ShowcasePath;
    }

    public static Vector3 Slot(int i) => new Vector3((i % 3 - 1) * 7f, 0, (1 - i / 3) * 7f);

    static Material GroundMaterial()
    {
        string path = Root + "/Showcase ground.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
        mat.SetColor("_BaseColor", new Color(.16f, .16f, .17f)); mat.SetFloat("_Smoothness", .1f);
        return mat;
    }

    public static string[] ImportedNames() => Directory.GetDirectories(Root).Select(Path.GetFileName)
        .Where(n => File.Exists($"{Root}/{n}/{n}.prefab") && File.Exists($"{Root}/{n}/{n}.creature.json")).OrderBy(n => n).ToArray();

    /// <summary>Renders creatures in an isolated preview scene, every one sampled at the same clip time.</summary>
    public static string Capture(string clip, float normalizedTime, string outPath, float azimuth = 20, float elevation = 28, string only = null)
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var rt = new RenderTexture(1920, 1080, 24) { antiAliasing = 4 };
        try
        {
            var names = ImportedNames();
            var creatures = new List<GameObject>();
            for (int i = 0; i < names.Length; i++)
            {
                if (only != null && names[i] != only) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/{names[i]}/{names[i]}.prefab");
                var go = UnityEngine.Object.Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(go, scene);
                go.transform.position = only != null ? Vector3.zero : Slot(i);
                creatures.Add(go);
            }
            var bounds = new Bounds(creatures[0].transform.position, Vector3.zero);
            foreach (var g in creatures)
            {
                var c = g.GetComponent<Animation>().GetClip(clip);
                c.SampleAnimation(g, c.length * normalizedTime);
                foreach (var r in g.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            }
            var lightGo = new GameObject("Sun");
            SceneManager.MoveGameObjectToScene(lightGo, scene);
            var light = lightGo.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.6f;
            light.shadows = LightShadows.Soft; lightGo.transform.rotation = Quaternion.Euler(45, 160, 0);
            var rimGo = new GameObject("Rim");
            SceneManager.MoveGameObjectToScene(rimGo, scene);
            var rim = rimGo.AddComponent<Light>(); rim.type = LightType.Directional; rim.intensity = .8f;
            rim.color = new Color(.75f, .85f, 1f); rimGo.transform.rotation = Quaternion.Euler(30, -20, 0);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            SceneManager.MoveGameObjectToScene(ground, scene);
            ground.transform.localScale = new Vector3(5, 1, 5);
            ground.GetComponent<Renderer>().sharedMaterial = GroundMaterial();
            var camGo = new GameObject("CaptureCamera");
            SceneManager.MoveGameObjectToScene(camGo, scene);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 30; cam.targetTexture = rt; cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.05f, .055f, .065f); cam.scene = scene;
            var dir = Quaternion.Euler(-elevation, azimuth, 0) * Vector3.back;
            float dist = bounds.extents.magnitude / Mathf.Sin(cam.fieldOfView * .5f * Mathf.Deg2Rad) * 1.05f;
            camGo.transform.position = bounds.center - dir * dist;
            camGo.transform.LookAt(bounds.center);
            cam.nearClipPlane = .1f; cam.farClipPlane = dist * 3;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
            RenderTexture.active = null;
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            return outPath;
        }
        finally
        {
            rt.Release();
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class HeroProbeBuilder
    {
        private const string Root = "Assets/Game/HeroProbe/";

        [MenuItem("Game/Probe/Build hero probe prefab P5")]
        public static void BuildP5()
        {
            const string folder = Root + "P5/";
            const string modelPath = folder + "Breakwater_P5_rig.fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException(modelPath);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.optimizeGameObjects = false;
            importer.isReadable = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();

            var slots = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("../art/heroes/breakwater/rig/Breakwater_P5_rig.json"))["part_materials"];
            var materials = new System.Collections.Generic.Dictionary<string, Material>
            {
                ["HB_WhitePaintedSteel"] = P5Material("WhitePaintedSteel", new Color(.89f, .88f, .84f), .5f, .45f),
                ["HB_BrushedSteel"] = P5Material("BrushedSteel", new Color(.63f, .69f, .75f), .8f, .55f),
                ["HB_NavyClothLeather"] = P5Material("NavyClothLeather", new Color(.15f, .22f, .30f), .1f, .25f),
                ["HB_DarkJointSteel"] = P5Material("DarkJointSteel", new Color(.18f, .21f, .24f), .65f, .4f),
                ["HB_CyanAccent"] = P5Material("CyanAccent", new Color(.05f, .70f, .82f), .1f, .4f, true),
                ["HB_RestrainedBronze"] = P5Material("RestrainedBronze", new Color(.56f, .43f, .27f), .7f, .45f)
            };
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            instance.name = "HeroProbeP5";
            try
            {
                foreach (var animator in instance.GetComponentsInChildren<Animator>()) UnityEngine.Object.DestroyImmediate(animator);
                foreach (var old in instance.GetComponentsInChildren<Animation>()) UnityEngine.Object.DestroyImmediate(old);
                var animation = instance.AddComponent<Animation>();
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var names = slots[renderer.name]?.Values<string>().ToArray();
                    if (names == null || names.Length != renderer.sharedMaterials.Length)
                        throw new InvalidOperationException("P5 material slots mismatch: " + renderer.name);
                    renderer.sharedMaterials = names.Select(n => materials[n]).ToArray();
                }
                var paths = new System.Collections.Generic.List<object>();
                foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
                {
                    var original = clips.FirstOrDefault(c => c.name.Split('|').Last() == name);
                    if (original == null) throw new InvalidOperationException("Missing baked action " + name);
                    var path = folder + name + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip == null)
                    {
                        clip = UnityEngine.Object.Instantiate(original);
                        AssetDatabase.CreateAsset(clip, path);
                    }
                    else EditorUtility.CopySerialized(original, clip);
                    clip.name = name;
                    clip.legacy = true;
                    clip.wrapMode = name == "Death" ? WrapMode.ClampForever : name == "Attack" ? WrapMode.Once : WrapMode.Loop;
                    EditorUtility.SetDirty(clip);
                    animation.AddClip(clip, name);
                    if (name == "Idle") animation.clip = clip;
                    var missing = AnimationUtility.GetCurveBindings(clip).Select(b => b.path).Distinct()
                        .Where(p => p.Length > 0 && instance.transform.Find(p) == null).ToArray();
                    paths.Add(new { name, length = clip.length, missingCount = missing.Length, missing });
                    if (missing.Length > 0) throw new InvalidOperationException(name + " missing paths: " + string.Join(", ", missing));
                }
                animation.playAutomatically = true;
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
                AssetDatabase.SaveAssets();
                PrefabUtility.SaveAsPrefabAsset(instance, folder + "HeroProbeP5.prefab");
                File.WriteAllText("../.local/codex-tasks/hero-p5/p5-import-facts.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paths, Newtonsoft.Json.Formatting.Indented));
                Debug.Log("HERO_PROBE_P5_BUILT clips=4 missingPaths=0 materials=6");
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static Material P5Material(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            var path = Root + "P5/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emission ? color * .12f : Color.black);
            if (emission) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        [MenuItem("Game/Probe/Build hero probe prefab P4")]
        public static void BuildP4()
        {
            const string folder = Root + "P4/";
            const string modelPath = folder + "Breakwater_P4_rig.fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException(modelPath);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.optimizeGameObjects = false;
            importer.isReadable = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();

            var slots = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText("../art/heroes/breakwater/rig/Breakwater_P4_rig.json"))["part_materials"];
            var materials = new System.Collections.Generic.Dictionary<string, Material>
            {
                ["HB_WhitePaintedSteel"] = P4Material("WhitePaintedSteel", new Color(.89f, .88f, .84f), .5f, .45f),
                ["HB_BrushedSteel"] = P4Material("BrushedSteel", new Color(.63f, .69f, .75f), .8f, .55f),
                ["HB_NavyClothLeather"] = P4Material("NavyClothLeather", new Color(.15f, .22f, .30f), 0f, .25f),
                ["HB_DarkJointSteel"] = P4Material("DarkJointSteel", new Color(.18f, .21f, .24f), .65f, .4f),
                ["HB_CyanAccent"] = P4Material("CyanAccent", new Color(.05f, .70f, .82f), .1f, .4f, true),
                ["HB_RestrainedBronze"] = P4Material("RestrainedBronze", new Color(.56f, .43f, .27f), .7f, .45f)
            };
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            instance.name = "HeroProbeP4";
            try
            {
                foreach (var animator in instance.GetComponentsInChildren<Animator>()) UnityEngine.Object.DestroyImmediate(animator);
                foreach (var old in instance.GetComponentsInChildren<Animation>()) UnityEngine.Object.DestroyImmediate(old);
                var animation = instance.AddComponent<Animation>();
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var names = slots[renderer.name]?.Values<string>().ToArray();
                    if (names == null || names.Length != renderer.sharedMaterials.Length)
                        throw new InvalidOperationException("P4 material slots mismatch: " + renderer.name);
                    renderer.sharedMaterials = names.Select(n => materials[n]).ToArray();
                }
                var paths = new System.Collections.Generic.List<object>();
                foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
                {
                    var original = clips.FirstOrDefault(c => c.name.Split('|').Last() == name);
                    if (original == null) throw new InvalidOperationException("Missing baked action " + name);
                    var path = folder + name + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip == null)
                    {
                        clip = UnityEngine.Object.Instantiate(original);
                        AssetDatabase.CreateAsset(clip, path);
                    }
                    else EditorUtility.CopySerialized(original, clip);
                    clip.name = name;
                    clip.legacy = true;
                    clip.wrapMode = name == "Death" ? WrapMode.ClampForever : name == "Attack" ? WrapMode.Once : WrapMode.Loop;
                    EditorUtility.SetDirty(clip);
                    animation.AddClip(clip, name);
                    if (name == "Idle") animation.clip = clip;
                    var missing = AnimationUtility.GetCurveBindings(clip).Select(b => b.path).Distinct()
                        .Where(p => p.Length > 0 && instance.transform.Find(p) == null).ToArray();
                    paths.Add(new { name, length = clip.length, missingCount = missing.Length, missing });
                    if (missing.Length > 0) throw new InvalidOperationException(name + " missing paths: " + string.Join(", ", missing));
                }
                animation.playAutomatically = true;
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
                AssetDatabase.SaveAssets();
                PrefabUtility.SaveAsPrefabAsset(instance, folder + "HeroProbeP4.prefab");
                File.WriteAllText("../.local/codex-tasks/hero-rig/p4-import-facts.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paths, Newtonsoft.Json.Formatting.Indented));
                Debug.Log("HERO_PROBE_P4_BUILT clips=4 missingPaths=0 materials=6");
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static Material P4Material(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            var path = Root + "P4/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", emission ? color * .12f : Color.black);
            if (emission) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        [MenuItem("Game/Probe/Build hero probe prefab")]
        public static void Build()
        {
            const string modelPath = Root + "Breakwater_P3_rig.fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException(modelPath);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.optimizeGameObjects = false;
            importer.isReadable = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var instance = UnityEngine.Object.Instantiate(source);
            instance.name = "HeroProbe";
            try
            {
                foreach (var animator in instance.GetComponentsInChildren<Animator>())
                    UnityEngine.Object.DestroyImmediate(animator);
                foreach (var old in instance.GetComponentsInChildren<Animation>())
                    UnityEngine.Object.DestroyImmediate(old);
                var animation = instance.AddComponent<Animation>();
                var gray = Material("Gray", new Color(0.48f, 0.52f, 0.57f));
                var cyan = Material("Cyan", new Color(0.06f, 0.64f, 0.76f));
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(_ =>
                        renderer.name.Contains("Stripe") || renderer.name.Contains("Visor") ? cyan : gray).ToArray();

                var paths = new System.Collections.Generic.List<object>();
                foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
                {
                    var original = clips.FirstOrDefault(c => c.name.Split('|').Last() == name);
                    if (original == null) throw new InvalidOperationException("Missing baked action " + name);
                    var path = Root + name + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip == null)
                    {
                        clip = UnityEngine.Object.Instantiate(original);
                        AssetDatabase.CreateAsset(clip, path);
                    }
                    else EditorUtility.CopySerialized(original, clip);
                    clip.name = name;
                    clip.legacy = true;
                    clip.wrapMode = name == "Death" ? WrapMode.ClampForever : name == "Attack" ? WrapMode.Once : WrapMode.Loop;
                    EditorUtility.SetDirty(clip);
                    animation.AddClip(clip, name);
                    if (name == "Idle") animation.clip = clip;
                    var missing = AnimationUtility.GetCurveBindings(clip).Select(b => b.path).Distinct()
                        .Where(p => p.Length > 0 && instance.transform.Find(p) == null).ToArray();
                    paths.Add(new { name, length = clip.length, missingCount = missing.Length, missing });
                    if (missing.Length > 0) throw new InvalidOperationException(name + " missing paths: " + string.Join(", ", missing));
                }
                animation.playAutomatically = true;
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
                AssetDatabase.SaveAssets();
                PrefabUtility.SaveAsPrefabAsset(instance, Root + "HeroProbe.prefab");
                Directory.CreateDirectory("../.local/codex-tasks/hero-rig");
                File.WriteAllText("../.local/codex-tasks/hero-rig/import-facts.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paths, Newtonsoft.Json.Formatting.Indented));
                Debug.Log("HERO_PROBE_BUILT clips=4 missingPaths=0");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static Material Material(string name, Color color)
        {
            var path = Root + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", 0.18f);
            material.SetFloat("_Smoothness", 0.32f);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

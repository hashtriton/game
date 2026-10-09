using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class HeroBreakwaterBuilder
    {
        public const string PrefabPath = "Assets/Game/Heroes/Breakwater/Breakwater.prefab";
        private const string Folder = "Assets/Game/Heroes/Breakwater/";

        [MenuItem("Game/Hero/Build Breakwater prefab")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            const string modelPath = Folder + "Breakwater_P5_merged.fbx";
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) throw new FileNotFoundException(modelPath);
            importer.animationType = ModelImporterAnimationType.Legacy;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.optimizeGameObjects = false;
            importer.isReadable = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.SaveAndReimport();

            var materials = new[]
            {
                Material("BrushedSteel", new Color(.63f, .69f, .75f), .8f, .55f),
                Material("CyanAccent", new Color(.05f, .70f, .82f), .1f, .4f, true),
                Material("DarkJointSteel", new Color(.18f, .21f, .24f), .65f, .4f),
                Material("NavyClothLeather", new Color(.15f, .22f, .30f), .1f, .25f),
                Material("RestrainedBronze", new Color(.56f, .43f, .27f), .7f, .45f),
                Material("WhitePaintedSteel", new Color(.89f, .88f, .84f), .5f, .45f)
            };
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(modelPath));
            instance.name = "Breakwater";
            try
            {
                foreach (var animator in instance.GetComponentsInChildren<Animator>()) UnityEngine.Object.DestroyImmediate(animator);
                foreach (var old in instance.GetComponentsInChildren<Animation>()) UnityEngine.Object.DestroyImmediate(old);
                var renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length != 1 || !(renderers[0] is SkinnedMeshRenderer skinned) || skinned.sharedMesh.subMeshCount != 6)
                    throw new InvalidOperationException("Breakwater requires one skinned mesh with six material slots.");
                skinned.sharedMaterials = skinned.sharedMaterials.Select(source =>
                    materials.Single(material => "HB_" + material.name == source.name)).ToArray();
                var animation = instance.AddComponent<Animation>();
                foreach (var name in new[] { "Idle", "Run", "Attack", "Death" })
                {
                    var original = clips.SingleOrDefault(c => c.name.Split('|').Last() == name);
                    if (original == null) throw new InvalidOperationException("Missing baked action " + name);
                    var path = Folder + name + ".anim";
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
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                        if (binding.path.Length > 0 && instance.transform.Find(binding.path) == null)
                            throw new InvalidOperationException(name + " missing path: " + binding.path);
                    EditorUtility.SetDirty(clip);
                    animation.AddClip(clip, name);
                    if (name == "Idle") animation.clip = clip;
                }
                animation.playAutomatically = true;
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
                AssetDatabase.SaveAssets();
                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static Material Material(string name, Color color, float metallic, float smoothness, bool emission = false)
        {
            var path = Folder + name + ".mat";
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
    }
}

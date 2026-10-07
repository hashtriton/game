using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static partial class GameArenaBuilder
    {
        const string TexRoot = "Assets/Game/Art/Textures/";
        const string GenRoot = "Assets/Game/Generated/";
        const string MatRoot = "Assets/Game/Generated/Materials/";
        const string MeshRoot = "Assets/Game/Generated/Meshes/";

        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        // Texture import rules by file name suffix. Normal maps and roughness must not be treated as colour.
        static void ImportTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexRoot.TrimEnd('/') }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null) continue;

                var name = Path.GetFileNameWithoutExtension(path);
                var isNormal = name.EndsWith("_n");
                var isData = name.EndsWith("_r") || name == "macro" || name == "cloth_alpha";
                var type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                var srgb = !isNormal && !isData;
                var alpha = name == "flame" || name == "blood" || name == "scorch" || name == "mist";

                if (importer.textureType == type && importer.sRGBTexture == srgb && importer.anisoLevel == 8 &&
                    importer.alphaIsTransparency == alpha) continue;

                importer.textureType = type;
                importer.sRGBTexture = srgb;
                importer.anisoLevel = 8;
                importer.mipmapEnabled = true;
                importer.alphaIsTransparency = alpha;
                importer.alphaSource = alpha ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        // URP Lit and Terrain Lit read smoothness from the alpha of a "mask" texture (R metallic, A smoothness).
        // The generated textures ship roughness, so pack 1 - roughness into a mask once.
        static Texture2D PackedMask(string baseName)
        {
            var path = GenRoot + baseName + "_mask.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            source.LoadImage(File.ReadAllBytes(TexRoot + baseName + "_r.jpg"));
            var pixels = source.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 255, 128, (byte)(255 - pixels[i].r));
            var packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
            packed.SetPixels32(pixels);
            File.WriteAllBytes(path, packed.EncodeToPNG());
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(packed);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D Tex(string file) => AssetDatabase.LoadAssetAtPath<Texture2D>(TexRoot + file);

        static Material NewMaterial(string name, string shaderName)
        {
            var path = MatRoot + name + ".mat";
            var material = new Material(Shader.Find(shaderName)) { name = name, enableInstancing = true };
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                // Overwrite the asset in place so its GUID, and every prefab and scene pointing at it, stays valid.
                EditorUtility.CopySerialized(material, existing);
                existing.name = name;
                Object.DestroyImmediate(material);
                material = existing;
                EditorUtility.SetDirty(material);
            }
            else AssetDatabase.CreateAsset(material, path);
            materials[name] = material;
            return material;
        }

        // Full PBR material from a generated texture set: <set>_a (albedo), <set>_n (normal), <set>_r (roughness).
        static Material Pbr(string name, string set, float smoothness, Color? tint = null, float normalScale = 1f)
        {
            var material = NewMaterial(name, "Universal Render Pipeline/Lit");
            material.SetTexture("_BaseMap", Tex(set + "_a.jpg"));
            material.SetColor("_BaseColor", tint ?? Color.white);
            material.SetTexture("_BumpMap", Tex(set + "_n.jpg"));
            material.SetFloat("_BumpScale", normalScale);
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_MetallicGlossMap", PackedMask(set));
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Plain(string name, Color color, float smoothness = 0.2f, float metallic = 0f)
        {
            var material = NewMaterial(name, "Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material Emissive(string name, Color baseColor, Color emission, float intensity)
        {
            var material = Plain(name, baseColor, 0.35f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.SetColor("_EmissionColor", emission * intensity);
            EditorUtility.SetDirty(material);
            return material;
        }

        // Particle-style unlit material, additive or alpha blended, depth-tested without writing depth.
        static Material Blended(string name, Texture2D texture, Color color, bool additive)
        {
            var material = NewMaterial(name, "Universal Render Pipeline/Particles/Unlit");
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            EditorUtility.SetDirty(material);
            return material;
        }

        static Texture2D GeneratedTexture(string file, int size, System.Func<float, float, float> alphaAt)
        {
            var path = GenRoot + file;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var a = Mathf.Clamp01(alphaAt((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            texture.SetPixels32(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static void BuildMaterials()
        {
            materials.Clear();
            Pbr("Stone", "stone", 0.55f, new Color(0.9f, 0.9f, 0.95f));
            Pbr("StoneWarm", "stone", 0.5f, new Color(1f, 0.92f, 0.82f));
            Pbr("StoneCold", "stone", 0.6f, new Color(0.78f, 0.82f, 0.92f));
            Pbr("Cobble", "cobble", 0.55f);
            Pbr("Barrel", "barrel", 0.8f);
            Pbr("BarrelExplosive", "barrel", 0.8f, new Color(1f, 0.5f, 0.4f));
            Pbr("Crate", "crate", 0.7f);
            Pbr("Rust", "rust", 0.9f, new Color(0.8f, 0.78f, 0.75f));
            Plain("Foliage", new Color(0.045f, 0.085f, 0.06f), 0.15f);
            Emissive("MushroomTeal", new Color(0.05f, 0.25f, 0.22f), new Color(0.15f, 1f, 0.8f), 2.2f);
            Emissive("MushroomViolet", new Color(0.18f, 0.08f, 0.25f), new Color(0.65f, 0.3f, 1f), 2.0f);
            Emissive("Crystal", new Color(0.05f, 0.3f, 0.4f), new Color(0.2f, 0.85f, 1f), 2.4f);
            Emissive("Water", new Color(0.02f, 0.12f, 0.14f), new Color(0.1f, 0.9f, 0.85f), 1.6f);

            var soft = GeneratedTexture("soft_dot.png", 64, (x, y) => Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(x * x + y * y)), 2f));
            var ring = GeneratedTexture("ring.png", 256, (x, y) =>
            {
                var r = Mathf.Sqrt(x * x + y * y);
                return Mathf.Clamp01(1f - Mathf.Abs(r - 0.82f) / 0.07f) * Mathf.Clamp01((1f - r) / 0.05f);
            });
            Blended("Flame", Tex("flame.png"), Color.white, true);
            Blended("Ember", soft, new Color(1f, 0.55f, 0.2f, 1f), true);
            Blended("Ring", ring, new Color(0.45f, 1f, 0.75f, 1f), true);
            // Radial falloff modulated by noise: soft-edged cloud blobs, no visible quad borders.
            var cloud = GeneratedTexture("mist_blob.png", 128, (x, y) =>
                Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(x * x + y * y)), 1.4f) * (0.5f + 0.5f * Mathf.PerlinNoise(x * 2.5f + 11f, y * 2.5f + 7f)));
            Blended("Mist", cloud, new Color(0.5f, 0.62f, 0.78f, 0.16f), false);
            Blended("Ash", soft, new Color(0.9f, 0.78f, 0.62f, 0.55f), true);
            Blended("Dust", soft, new Color(0.55f, 0.5f, 0.45f, 0.35f), false);
            Blended("Blood", Tex("blood.png"), new Color(0.5f, 0.07f, 0.05f, 0.85f), false);
            Blended("Scorch", Tex("scorch.png"), new Color(0.03f, 0.03f, 0.03f, 0.9f), false);
            AssetDatabase.SaveAssets();
        }
    }
}

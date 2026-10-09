using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static class HudArtBuilder
    {
        const string Root = "Assets/Game/Art/Ui/Hud/Resources/";

        [MenuItem("Game/HUD/Import skin")]
        public static void Import()
        {
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(Root + "Sprites", "*.png"))
            {
                var path = file.Replace('\\', '/');
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var border = file.EndsWith("shopWindow.png", StringComparison.Ordinal) || file.EndsWith("shopTooltip.png", StringComparison.Ordinal)
                    ? new Vector4(40f, 40f, 40f, 40f) : Vector4.zero;
                if (importer.textureType == TextureImporterType.Sprite && !importer.mipmapEnabled &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed && importer.spriteBorder == border) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = border;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var pathToSkin = Root + "HudSkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<HudSkin>(pathToSkin);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<HudSkin>();
                AssetDatabase.CreateAsset(skin, pathToSkin);
            }
            foreach (var field in typeof(HudSkin).GetFields())
            {
                if (field.FieldType != typeof(Sprite)) continue;
                var fileName = field.Name == "portraitMask" ? "portrait_mask" : field.Name == "portraitFrame" ? "portrait_frame" : field.Name;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Sprites/" + fileName + ".png");
                if (sprite == null) throw new FileNotFoundException("HUD sprite: " + fileName);
                field.SetValue(skin, sprite);
            }
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Game/HUD/Render hero portrait")]
        public static void RenderPortrait()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before rendering portrait.");
            var hero = UnityEngine.Object.FindAnyObjectByType<HeroController>();
            if (hero == null || hero.GetComponent<Unit>().modelPrefab == null)
                throw new InvalidOperationException("Open Arena scene with the current hero model.");
            var preview = new PreviewRenderUtility();
            Texture2D texture = null;
            try
            {
                var instance = UnityEngine.Object.Instantiate(hero.GetComponent<Unit>().modelPrefab);
                preview.AddSingleGO(instance);
                instance.transform.rotation = Quaternion.Euler(0f, 165f, 0f);
                var animation = instance.GetComponentInChildren<Animation>();
                if (animation != null && animation["Idle"] != null)
                {
                    animation.Play("Idle");
                    animation["Idle"].normalizedTime = 0.2f;
                    animation.Sample();
                }
                // Imported skinned bounds may contain the pre-normalization scale. Measure the actual posed mesh.
                var bounds = new Bounds();
                var found = false;
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    Mesh mesh;
                    var skinned = renderer as SkinnedMeshRenderer;
                    if (skinned != null)
                    {
                        mesh = new Mesh();
                        skinned.BakeMesh(mesh, true);
                        skinned.updateWhenOffscreen = true;
                    }
                    else mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                    if (mesh == null) continue;
                    foreach (var vertex in mesh.vertices)
                    {
                        var point = renderer.transform.TransformPoint(vertex);
                        if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                        else bounds.Encapsulate(point);
                    }
                    if (skinned != null) UnityEngine.Object.DestroyImmediate(mesh);
                }
                if (!found) throw new InvalidOperationException("Hero has no renderable geometry.");
                var focus = bounds.center + Vector3.up * bounds.size.y * 0.23f;
                var camera = preview.camera;
                camera.orthographic = true;
                camera.orthographicSize = bounds.size.y * 0.32f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.03f, 0.07f, 0.12f, 0f);
                camera.transform.position = focus + new Vector3(0f, 0.08f, -bounds.size.y * 3f);
                camera.transform.LookAt(focus);
                preview.lights[0].intensity = 1.8f;
                preview.lights[0].color = new Color(1f, 0.9f, 0.78f);
                preview.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                preview.lights[1].intensity = 1.2f;
                preview.lights[1].color = new Color(0.45f, 0.7f, 1f);
                preview.ambientColor = new Color(0.32f, 0.37f, 0.44f);
                preview.BeginStaticPreview(new Rect(0f, 0f, 448f, 560f));
                // CLI calls have no reliable GUI repaint viewport, especially at Windows display scaling.
                camera.pixelRect = new Rect(0f, 0f, camera.targetTexture.width, camera.targetTexture.height);
                camera.aspect = 448f / 560f;
                preview.Render(true, false);
                texture = preview.EndStaticPreview();
                Directory.CreateDirectory(Root + "Sprites");
                File.WriteAllBytes(Root + "Sprites/portrait.png", texture.EncodeToPNG());
            }
            finally
            {
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                preview.Cleanup();
            }
            Import();
        }
    }
}

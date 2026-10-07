using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static partial class GameArenaBuilder
    {
        const string UiRoot = "Assets/Game/Art/Ui/";
        const string IconRoot = "Assets/Game/Art/Icons/";
        const string HudAssetsPath = GenRoot + "HudAssets.asset";
        const string GuidesPath = "Assets/Game/Data/guides.json";
        const string BodyFontPath = "Assets/Arena/Resources/ArenaUi/PTSans.ttf";
        const string TitleFontPath = "Assets/Arena/Resources/ArenaUi/RussoOne.ttf";

        // The catalogs OriginalGameCatalogs.Load expects, by file name without extension.
        static readonly string[] CatalogNames =
        {
            "lia39-match", "lia39-items", "lia39-combat", "lia39-item-passives", "lia39-duels",
            "lia39-native126", "lia39-observed126", "lia39-observed-items126", "lia39-layout"
        };

        // Corners of the sliced skin images, as (left, bottom, right, top). Keep in step with SLICES in tools/art/gen_ui_skin.py.
        static readonly Dictionary<string, Vector4> SkinSlices = new Dictionary<string, Vector4>
        {
            { "panel", new Vector4(22f, 22f, 22f, 22f) },
            { "tooltip", new Vector4(14f, 14f, 14f, 14f) },
            { "slot", new Vector4(6f, 6f, 6f, 6f) },
            { "button", new Vector4(10f, 10f, 10f, 10f) },
            { "frame", new Vector4(6f, 6f, 6f, 6f) },
            { "tab", new Vector4(8f, 8f, 8f, 8f) }
        };

        static int iconCount;

        static void ImportSprites(string folder, System.Func<string, Vector4> border)
        {
            if (!Directory.Exists(folder.TrimEnd('/'))) return;
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(folder.TrimEnd('/'), "*.png"))
            {
                var path = file.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                var slice = border(name);
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == slice && !importer.mipmapEnabled &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = slice;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        static Sprite SkinSprite(string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + name + ".png");
            if (sprite == null) throw new FileNotFoundException("Missing UI skin image " + name + " (run tools/art/gen_ui_skin.py)");
            return sprite;
        }

        // Runs at the start of the build: importing refreshes the asset database, which must not happen
        // once the build has created materials and prefabs that are still only in memory.
        static void ImportInterfaceImages()
        {
            ImportSprites(UiRoot, name => SkinSlices.TryGetValue(name, out var slice) ? slice : Vector4.zero);
            ImportSprites(IconRoot, name => Vector4.zero);
        }

        static HudAssets BuildHudAssets()
        {
            var assets = AssetDatabase.LoadAssetAtPath<HudAssets>(HudAssetsPath);
            if (assets == null)
            {
                assets = ScriptableObject.CreateInstance<HudAssets>();
                AssetDatabase.CreateAsset(assets, HudAssetsPath);
            }
            assets.bodyFont = AssetDatabase.LoadAssetAtPath<Font>(BodyFontPath);
            assets.titleFont = AssetDatabase.LoadAssetAtPath<Font>(TitleFontPath);
            assets.panel = SkinSprite("panel");
            assets.tooltip = SkinSprite("tooltip");
            assets.slot = SkinSprite("slot");
            assets.frame = SkinSprite("frame");
            assets.button = SkinSprite("button");
            assets.tab = SkinSprite("tab");
            assets.barFill = SkinSprite("bar_fill");
            assets.divider = SkinSprite("divider");
            assets.coin = SkinSprite("coin");
            assets.soul = SkinSprite("soul");

            var ids = new List<string>();
            var sprites = new List<Sprite>();
            if (Directory.Exists(IconRoot.TrimEnd('/')))
            {
                foreach (var file in Directory.GetFiles(IconRoot.TrimEnd('/'), "*.png").OrderBy(f => f, System.StringComparer.Ordinal))
                {
                    var path = file.Replace('\\', '/');
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) continue;
                    ids.Add(Path.GetFileNameWithoutExtension(path));
                    sprites.Add(sprite);
                }
            }
            assets.iconIds = ids.ToArray();
            assets.icons = sprites.ToArray();
            iconCount = ids.Count;
            EditorUtility.SetDirty(assets);
            return assets;
        }

        static void BuildHud(Camera camera, HeroController controller)
        {
            var assets = BuildHudAssets();

            var sessionObject = new GameObject("Game session");
            var session = sessionObject.AddComponent<GameSession>();
            session.hero = controller.GetComponent<Unit>();
            session.dataAssets = CatalogNames
                .Select(name => AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Arena/Data/" + name + ".json"))
                .ToArray();
            if (session.dataAssets.Any(a => a == null)) throw new FileNotFoundException("A catalog of the original map is missing in Assets/Arena/Data.");
            session.guidesJson = AssetDatabase.LoadAssetAtPath<TextAsset>(GuidesPath);
            if (session.guidesJson == null) throw new FileNotFoundException(GuidesPath);

            var hudObject = new GameObject("HUD");
            var hud = hudObject.AddComponent<GameHud>();
            hud.session = session;
            hud.assets = assets;
            hud.view = camera;
            hud.hero = controller;
        }
    }
}

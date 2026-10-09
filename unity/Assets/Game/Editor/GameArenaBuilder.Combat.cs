using System.Collections.Generic;
using System.IO;
using Arena;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static partial class GameArenaBuilder
    {
        const string LayoutOutPath = GenRoot + "arena-layout.json";
        const string CreepPrefabPath = "Assets/Creatures/CrystalArachnid/CrystalArachnid.prefab";
        const string BreakEffectPath = GenRoot + "BarrelBreak.prefab";

        // The north-east corner of the arena, where the original has its field of barrels. The layout ships the barrels
        // only up to x = 1350; the free strip between them and the east wall is filled here on the same grid.
        const float CornerMinX = 1376f, CornerMaxX = 1760f, CornerMinY = 2016f, CornerMaxY = 2688f;

        static int cornerBarrels;

        /// <summary>
        /// Writes the layout the arena runs on: the measured 3.9c layout plus the barrels that complete the corner.
        /// The source layout asset stays untouched; both the pathing grid and the scene read the written copy.
        /// </summary>
        static TextAsset BuildLayout()
        {
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath);
            if (source == null) throw new FileNotFoundException(LayoutPath);
            var layout = JsonUtility.FromJson<ArenaMapLayout>(source.text);

            var probeObject = new GameObject("Layout probe");
            var probe = probeObject.AddComponent<ArenaMap>();
            probe.layoutJson = source;
            probe.unitsPerMeter = 64f;
            var added = CornerBarrelRecords(layout, probe);
            Object.DestroyImmediate(probeObject);

            var doodads = new List<MapDoodad>(layout.doodads);
            doodads.AddRange(added);
            layout.doodads = doodads.ToArray();
            cornerBarrels = added.Count;

            File.WriteAllText(LayoutOutPath, JsonUtility.ToJson(layout));
            AssetDatabase.ImportAsset(LayoutOutPath);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutOutPath);
        }

        static List<MapDoodad> CornerBarrelRecords(ArenaMapLayout layout, ArenaMap probe)
        {
            MapDoodad template = null;
            var nextId = 0;
            foreach (var d in layout.doodads)
            {
                nextId = Mathf.Max(nextId, d.editorId);
                if (template == null && d.id == "LTbr" && d.life != 0 && !string.IsNullOrEmpty(d.pathingTexture)) template = d;
            }
            if (template == null) throw new System.InvalidOperationException("No barrel record to copy the footprint from.");
            var templateJson = JsonUtility.ToJson(template);

            var result = new List<MapDoodad>();
            // Barrels of the layout sit on a 64 unit lattice offset by half a step.
            for (var y = 32f + 64f * Mathf.Ceil((CornerMinY - 32f) / 64f); y <= CornerMaxY; y += 64f)
            {
                for (var x = 32f + 64f * Mathf.Ceil((CornerMinX - 32f) / 64f); x <= CornerMaxX; x += 64f)
                {
                    if (!FootprintFree(probe, x, y) || NearScenery(layout, x, y)) continue;

                    var barrel = JsonUtility.FromJson<MapDoodad>(templateJson);
                    barrel.offset = -1;
                    barrel.editorId = ++nextId;
                    barrel.x = x;
                    barrel.y = y;
                    barrel.z = probe.SampleHeight(new Vector3(x / 64f, 0f, y / 64f)) * 64f;
                    var seed = barrel.editorId;
                    barrel.rotationRadians = Hash01(seed, 91) * Mathf.PI * 2f;
                    var scale = 0.95f + Hash01(seed, 92) * 0.15f;
                    barrel.scale = new[] { scale, scale, scale };
                    result.Add(barrel);
                }
            }
            return result;
        }

        // A barrel blocks one lattice cell of 64 x 64 units, so the whole cell has to be open ground.
        static bool FootprintFree(ArenaMap probe, float x, float y)
        {
            for (var dy = -28f; dy <= 28f; dy += 14f)
                for (var dx = -28f; dx <= 28f; dx += 14f)
                    if (!probe.IsWalkable(new Vector3((x + dx) / 64f, 0f, (y + dy) / 64f))) return false;
            return true;
        }

        // Scenery without a pathing footprint (rocks, statues, ruins) must not end up inside a barrel.
        static bool NearScenery(ArenaMapLayout layout, float x, float y)
        {
            foreach (var d in layout.doodads)
            {
                switch (d.category)
                {
                    case "rock": case "statue": case "pillar": case "wall": case "building": case "unresolved": case "decoration": case "floor":
                        break;
                    default:
                        continue;
                }
                if (Mathf.Abs(d.x - x) < 60f && Mathf.Abs(d.y - y) < 60f) return true;
            }
            return false;
        }

        // Wood chips and a puff of dust for a breaking barrel.
        static GameObject BuildBreakEffect()
        {
            var root = new GameObject("Barrel break");

            var chips = root.AddComponent<ParticleSystem>();
            var main = chips.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.gravityModifier = 1.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.None;
            var emission = chips.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 16, 20) });
            var shape = chips.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.25f;
            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            renderer.sharedMaterial = materials["Barrel"];

            var dustObject = new GameObject("Dust");
            dustObject.transform.SetParent(root.transform, false);
            var dust = dustObject.AddComponent<ParticleSystem>();
            var dustMain = dust.main;
            dustMain.duration = 1f;
            dustMain.loop = false;
            dustMain.startLifetime = 0.9f;
            dustMain.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.1f);
            dustMain.startSize = new ParticleSystem.MinMaxCurve(0.9f, 1.5f);
            dustMain.simulationSpace = ParticleSystemSimulationSpace.World;
            dustMain.stopAction = ParticleSystemStopAction.None;
            var dustEmission = dust.emission;
            dustEmission.rateOverTime = 0f;
            dustEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6, 8) });
            var dustShape = dust.shape;
            dustShape.shapeType = ParticleSystemShapeType.Sphere;
            dustShape.radius = 0.3f;
            var fade = dust.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var grow = dust.sizeOverLifetime;
            grow.enabled = true;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.3f));
            dustObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = materials["Dust"];

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, BreakEffectPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // Three dummies to try the orders on: far enough that the hero has to walk up, quiet until he comes close.
        static void BuildTrainingCreeps(ArenaMap map)
        {
            var prefab = BuildCreepModel();
            var root = new GameObject("Training creeps").transform;
            var offsets = new[] { new Vector3(11f, 0f, 5f), new Vector3(7f, 0f, 12f), new Vector3(-9f, 0f, 9f) };
            for (var i = 0; i < offsets.Length; i++)
            {
                var creep = new GameObject("Training creep " + (i + 1));
                creep.transform.SetParent(root, false);
                creep.transform.position = map.FindNearestWalkable(map.HeroSpawn + offsets[i]);
                creep.AddComponent<ArenaActor>();
                var unit = creep.AddComponent<Unit>();
                unit.faction = Faction.Creep;
                unit.displayName = "Хрустальный арахнид";
                unit.modelPrefab = prefab;
                unit.height = 1.5f;
                unit.radius = 0.55f;
                unit.baseMaxHealth = 220f;
                unit.baseArmor = 1f;
                unit.baseDamageMin = 9f;
                unit.baseDamageMax = 13f;
                unit.baseAttackInterval = 1.6f;
                unit.baseMoveSpeed = 3.6f;
                unit.bounty = 8;
                var ai = creep.AddComponent<CreepAi>();
                ai.map = map;
                ai.aggroRange = 7f;
            }
        }

        static GameObject BuildCreepModel()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(CreepPrefabPath);
            if (source == null) throw new FileNotFoundException(CreepPrefabPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                var albedo = BuildCreepPalette("Albedo");
                var glowMask = BuildCreepPalette("Emission");
                var copies = new Dictionary<Material, Material>();
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var slots = renderer.sharedMaterials;
                    for (var i = 0; i < slots.Length; i++)
                    {
                        var original = slots[i];
                        if (original == null) throw new System.InvalidOperationException("Creep has an empty material slot.");
                        if (!copies.TryGetValue(original, out var material))
                        {
                            material = NewMaterial("Creep_" + original.name, original.shader.name);
                            material.CopyPropertiesFromMaterial(original);
                            material.SetColor("_BaseColor", new Color(0.98f, 0.94f, 0.88f));
                            material.SetFloat("_Smoothness", 0.3f);
                            material.SetFloat("_BumpScale", 0.65f);
                            material.SetTexture("_BaseMap", albedo);
                            material.SetTexture("_EmissionMap", glowMask);
                            material.SetColor("_EmissionColor", new Color(1.5f, 0.34f, 0.045f));
                            EditorUtility.SetDirty(material);
                            copies.Add(original, material);
                        }
                        slots[i] = material;
                    }
                    renderer.sharedMaterials = slots;
                }
                AssetDatabase.SaveAssets();
                return PrefabUtility.SaveAsPrefabAsset(instance, GenRoot + "CreepModel.prefab");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        // Recolour the existing cyan mineral patches; geometry and all animation bindings stay intact.
        static Texture2D BuildCreepPalette(string channel)
        {
            var path = GenRoot + "Creep" + channel + ".png";
            var sourcePath = "Assets/Creatures/CrystalArachnid/CrystalArachnid_" + channel + ".png";
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(sourcePath)))
                    throw new InvalidDataException("Unable to decode creep texture: " + sourcePath);
                var pixels = texture.GetPixels32();
                for (var i = 0; i < pixels.Length; i++)
                {
                    if (channel == "Emission")
                    {
                        var value = System.Math.Max(pixels[i].r, System.Math.Max(pixels[i].g, pixels[i].b));
                        pixels[i] = new Color32(value, value, value, 255);
                    }
                    else
                    {
                        Color.RGBToHSV(pixels[i], out var hue, out var saturation, out var value);
                        if (hue > 0.38f && hue < 0.65f && saturation > 0.25f)
                            pixels[i] = Color.HSVToRGB(0.055f, saturation * 0.8f, value);
                    }
                }
                texture.SetPixels32(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}

using System;
using System.IO;
using Arena;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.EditorTools
{
    /// <summary>
    /// Builds the sunlit arena scene from the measured Life in Arena 3.9c layout:
    /// terrain, props, light, post processing, camera and a hero that runs on the original pathing grid.
    /// </summary>
    public static partial class GameArenaBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/Arena.unity";
        const string LayoutPath = "Assets/Arena/Data/lia39-layout.json";
        const string HeroPrefabPath = "Assets/Arena/Generated/Warrior.prefab";
        static readonly bool UseBreakwaterHero = true;

        [MenuItem("Game/Build Arena scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");

            EnsureFolders();
            ImportTextures();
            ImportInterfaceImages();
            BuildMaterials();
            ConfigurePipeline();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var layoutAsset = BuildLayout();
            breakEffectPrefab = BuildBreakEffect();

            var mapObject = new GameObject("Arena Map");
            var map = mapObject.AddComponent<ArenaMap>();
            map.layoutJson = layoutAsset;
            map.unitsPerMeter = 64f;

            // Terrain layers need their packed masks imported before batching starts.
            PackedMask("ground");
            AssetDatabase.StartAssetEditing();
            try
            {
                BuildTerrain(map);
                PlaceDoodads(map, new GameObject("Arena props").transform);
                PlaceLandmarks(map);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            BuildAtmosphere();
            var hero = BuildHero(map);
            var camera = BuildCamera(map, hero.transform);
            BuildAir(map, hero.transform);
            BuildTrainingCreeps(map);
            BuildHud(camera, hero.GetComponent<HeroController>());

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Arena scene failed to save.");
            AssetDatabase.SaveAssets();

            var summary = new System.Text.StringBuilder("ARENA_BUILT");
            foreach (var pair in placed) summary.Append(' ').Append(pair.Key).Append('=').Append(pair.Value);
            summary.Append(" icons=").Append(iconCount).Append(" cornerBarrels=").Append(cornerBarrels).Append(" meshes=").Append(meshes.Count);
            Debug.Log(summary.ToString());
        }

        static void EnsureFolders()
        {
            foreach (var path in new[] { "Assets/Game/Scenes", "Assets/Game/Generated" })
                Directory.CreateDirectory(path);
            if (!AssetDatabase.IsValidFolder(MeshRoot.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/Game/Generated", "Meshes");
            // Materials are updated in place, never deleted: prefabs refer to them by GUID, and a prefab that is
            // imported while its material is being recreated keeps an empty slot (a pink hero).
            if (!AssetDatabase.IsValidFolder(MatRoot.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets/Game/Generated", "Materials");
            meshes.Clear();
            AssetDatabase.Refresh();
        }

        static void ConfigurePipeline()
        {
            const string rendererPath = GenRoot + "GameRenderer.asset";
            const string pipelinePath = GenRoot + "GameURP.asset";

            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }
            // Forward+ lifts the per-object limit on additional lights: the arena has dozens of torches.
            renderer.renderingMode = RenderingMode.ForwardPlus;
            if (!renderer.TryGetRendererFeature<ScreenSpaceAmbientOcclusion>(out var occlusion))
            {
                occlusion = ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();
                occlusion.name = "Courtyard ambient occlusion";
                AssetDatabase.AddObjectToAsset(occlusion, renderer);
                renderer.rendererFeatures.Add(occlusion);
            }
            var ao = new SerializedObject(occlusion);
            var settings = ao.FindProperty("m_Settings");
            settings.FindPropertyRelative("Source").enumValueIndex = 1;
            settings.FindPropertyRelative("Intensity").floatValue = 0.55f;
            settings.FindPropertyRelative("Radius").floatValue = 0.35f;
            settings.FindPropertyRelative("DirectLightingStrength").floatValue = 0.12f;
            settings.FindPropertyRelative("Downsample").boolValue = false;
            settings.FindPropertyRelative("AfterOpaque").boolValue = false;
            ao.ApplyModifiedPropertiesWithoutUndo();
            occlusion.SetActive(true);
            renderer.SetDirty();
            EditorUtility.SetDirty(renderer);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }
            pipeline.supportsHDR = true;
            pipeline.msaaSampleCount = 4;
            pipeline.supportsCameraDepthTexture = true;
            pipeline.mainLightShadowmapResolution = 4096;
            pipeline.shadowDistance = 90f;
            pipeline.shadowCascadeCount = 4;
            pipeline.colorGradingMode = ColorGradingMode.HighDynamicRange;
            // Soft shadows have no public setter.
            var serialized = new SerializedObject(pipeline);
            serialized.FindProperty("m_SoftShadowsSupported").boolValue = true;
            serialized.FindProperty("m_SoftShadowQuality").enumValueIndex = (int)SoftShadowQuality.High;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            var current = QualitySettings.GetQualityLevel();
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);
        }

        static void BuildTerrain(ArenaMap map)
        {
            var source = map.Layout.terrain;
            var unit = map.unitsPerMeter;
            var terrainData = new TerrainData { heightmapResolution = source.width };
            terrainData.size = new Vector3((source.width - 1) * source.cellSize / unit, 20f, (source.height - 1) * source.cellSize / unit);
            var heights = new float[source.height, source.width];
            for (var z = 0; z < source.height; z++)
                for (var x = 0; x < source.width; x++)
                    heights[z, x] = (source.vertices[z * source.width + x].height / unit + 10f) / 20f;
            terrainData.SetHeights(0, 0, heights);

            var layers = new[]
            {
                TerrainLayerFor("Dirt", "ground", 8f, Color.white, 0.42f),
                TerrainLayerFor("Cobble", "cobble", 8f, Color.white, 0.42f),
                TerrainLayerFor("Paving", "cobble", 8f, new Color(0.95f, 0.98f, 1f), 0.42f),
                TerrainLayerFor("DeadGrass", "ground", 8f, new Color(0.76f, 0.84f, 0.65f), 0.3f),
                TerrainLayerFor("DirtDamp", "ground", 8f, new Color(0.85f, 0.90f, 0.94f), 0.55f),
                TerrainLayerFor("CobbleDamp", "cobble", 8f, new Color(0.85f, 0.90f, 0.94f), 0.55f),
                TerrainLayerFor("PavingDamp", "cobble", 8f, new Color(0.85f, 0.90f, 0.94f), 0.55f),
                TerrainLayerFor("GrassDamp", "ground", 8f, new Color(0.58f, 0.69f, 0.53f), 0.42f)
            };
            terrainData.terrainLayers = layers;

            const int resolution = 256;
            terrainData.alphamapResolution = resolution;
            var weights = new float[resolution, resolution, layers.Length];
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var gx = x / (resolution - 1f) * (source.width - 1);
                    var gz = z / (resolution - 1f) * (source.height - 1);
                    var vx = Mathf.Min(source.width - 2, Mathf.FloorToInt(gx));
                    var vz = Mathf.Min(source.height - 2, Mathf.FloorToInt(gz));
                    var fx = gx - vx;
                    var fz = gz - vz;
                    // World-scale staining breaks the repeat without moving texture joints or terrain vertices.
                    var worldX = (source.origin[0] + gx * source.cellSize) / unit;
                    var worldZ = (source.origin[1] + gz * source.cellSize) / unit;
                    var patch = Mathf.PerlinNoise(worldX * 0.25f + 37f, worldZ * 0.10f + 91f);
                    var broad = Mathf.PerlinNoise(worldX * 0.025f + 17f, worldZ * 0.025f + 8f);
                    var wornLane = Mathf.Exp(-Mathf.Pow((worldX - map.HeroSpawn.x) / 3.5f, 2f));
                    var damp = Mathf.Clamp01((patch * 0.85f + broad * 0.15f - 0.52f) / 0.30f) * 0.22f * (1f - wornLane * 0.7f);
                    for (var dz = 0; dz < 2; dz++)
                    {
                        for (var dx = 0; dx < 2; dx++)
                        {
                            var vertex = source.vertices[(vz + dz) * source.width + vx + dx];
                            var layer = LayerIndex(source.groundTextures[vertex.groundTextureIndex]);
                            var weight = (dx == 0 ? 1f - fx : fx) * (dz == 0 ? 1f - fz : fz);
                            weights[z, x, layer] += weight * (1f - damp);
                            weights[z, x, layer + 4] += weight * damp;
                        }
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, weights);
            AssetDatabase.CreateAsset(terrainData, GenRoot + "ArenaTerrain.asset");

            var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "Terrain";
            terrainObject.transform.position = new Vector3(source.origin[0] / unit, -10f, source.origin[1] / unit);
            var terrain = terrainObject.GetComponent<Terrain>();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 3f;
            var material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit")) { name = "TerrainLit" };
            AssetDatabase.CreateAsset(material, MatRoot + "TerrainLit.mat");
            terrain.materialTemplate = material;
            Prop("Outer ground", null, OuterGroundMesh(map), materials["OuterGround"], Vector3.zero, Quaternion.identity, Vector3.one, true);
        }

        // Original ground tile rawcodes folded into the four looks the arena needs.
        static int LayerIndex(string rawcode)
        {
            switch (rawcode)
            {
                case "Qcbp": return 1;
                case "Qstp": return 2;
                case "Qgrs":
                case "Qgrt": return 3;
                default: return 0;
            }
        }

        static TerrainLayer TerrainLayerFor(string name, string set, float tileMeters, Color tint, float smoothness)
        {
            var layer = new TerrainLayer
            {
                name = name,
                diffuseTexture = Tex(set + "_a.jpg"),
                normalMapTexture = Tex(set + "_n.jpg"),
                maskMapTexture = PackedMask(set),
                tileSize = new Vector2(tileMeters, tileMeters),
                diffuseRemapMin = Vector4.zero,
                diffuseRemapMax = new Vector4(tint.r, tint.g, tint.b, 1f),
                maskMapRemapMin = Vector4.zero,
                maskMapRemapMax = new Vector4(1f, 1f, 1f, smoothness)
            };
            AssetDatabase.CreateAsset(layer, GenRoot + "Layer_" + name + ".terrainlayer");
            return layer;
        }

        // Service points from the original map: the healing well sits where it did in the source layout.
        static void PlaceLandmarks(ArenaMap map)
        {
            var root = new GameObject("Landmarks").transform;
            foreach (var unit in map.Layout.staticUnits)
            {
                if (unit.id != "e00M") continue;
                var point = map.WcToWorld(unit.x, unit.y);
                BuildWell(root, point);
            }
        }

        static void BuildAtmosphere()
        {
            var sun = new GameObject("Afternoon sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.9f, 0.76f);
            sun.intensity = 2.9f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.95f;
            // Camera yaw is zero: +X and -Z project toward the lower-right of the frame.
            sun.transform.rotation = Quaternion.Euler(42f, 135f, 0f);
            RenderSettings.sun = sun;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.4f, 0.5f, 0.64f);
            RenderSettings.ambientEquatorColor = new Color(0.3f, 0.37f, 0.47f);
            RenderSettings.ambientGroundColor = new Color(0.46f, 0.39f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.65f, 0.75f, 0.82f);
            RenderSettings.fogDensity = 0.0018f;
            RenderSettings.skybox = null;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, GenRoot + "GamePost.asset");

            var tonemapping = AddEffect<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);
            var bloom = AddEffect<Bloom>(profile);
            bloom.threshold.Override(1.2f);
            bloom.intensity.Override(0.22f);
            bloom.scatter.Override(0.55f);
            bloom.highQualityFiltering.Override(true);
            var vignette = AddEffect<Vignette>(profile);
            vignette.intensity.Override(0.08f);
            vignette.smoothness.Override(0.5f);
            var adjustments = AddEffect<ColorAdjustments>(profile);
            adjustments.contrast.Override(8f);
            adjustments.saturation.Override(14f);
            adjustments.colorFilter.Override(Color.white);
            EditorUtility.SetDirty(profile);

            var volume = new GameObject("Post processing").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        static T AddEffect<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var effect = profile.Add<T>(true);
            AssetDatabase.AddObjectToAsset(effect, profile);
            return effect;
        }

        // Hero height in metres. A barrel is 0.95 m and a wall 3 to 4 m, so this reads as a heavy fighter next to both.
        const float HeroHeight = 2.4f;

        static GameObject BuildHero(ArenaMap map)
        {
            var hero = new GameObject("Hero");
            var ring = Prop("Selection ring", hero.transform, FlatQuadMesh("quad_flat"), materials["Ring"],
                Vector3.zero, Quaternion.identity, new Vector3(2.2f, 1f, 2.2f), false, false);
            ring.transform.localPosition = new Vector3(0f, 0.07f, 0f);

            var marker = new GameObject("Move marker");
            Prop("Ring", marker.transform, FlatQuadMesh("quad_flat"), materials["Ring"], Vector3.zero, Quaternion.identity, Vector3.one, false, false);
            var clickMarker = marker.AddComponent<ClickMarker>();

            var attackFlash = new GameObject("Attack marker");
            Prop("Ring", attackFlash.transform, FlatQuadMesh("quad_flat"), materials["Ring"], Vector3.zero, Quaternion.identity, Vector3.one, false, false);
            var attackMarker = attackFlash.AddComponent<ClickMarker>();
            attackMarker.color = new Color(1f, 0.22f, 0.16f, 1f);

            var highlightObject = new GameObject("Target highlight");
            Prop("Ring", highlightObject.transform, FlatQuadMesh("quad_flat"), materials["Ring"], Vector3.zero, Quaternion.identity, Vector3.one, false, false);
            var highlight = highlightObject.AddComponent<TargetHighlight>();

            // Gentle warm bounce on the placeholder armour complements the daylight key.
            var glow = new GameObject("Hero light").AddComponent<Light>();
            glow.transform.SetParent(hero.transform, false);
            glow.transform.localPosition = new Vector3(0f, HeroHeight + 0.7f, -0.8f);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.86f, 0.68f);
            glow.intensity = 0.55f;
            glow.range = 7.5f;
            glow.shadows = LightShadows.None;

            hero.AddComponent<ArenaActor>();
            var unit = hero.AddComponent<Unit>();
            unit.faction = Faction.Hero;
            unit.displayName = "Герой";
            unit.modelPrefab = BuildHeroModel();
            unit.height = HeroHeight;
            unit.radius = 0.4f;
            unit.baseMaxHealth = 700f;
            unit.baseMaxMana = 240f;
            unit.baseArmor = 3f;
            unit.baseDamageMin = 26f;
            unit.baseDamageMax = 34f;
            unit.attackRange = 1.1f;
            unit.baseAttackInterval = 1.35f;
            unit.baseMoveSpeed = 5f;
            unit.healthRegen = 1.2f;
            unit.manaRegen = 0.8f;

            var controller = hero.AddComponent<HeroController>();
            controller.map = map;
            controller.marker = clickMarker;
            controller.attackMarker = attackMarker;
            controller.highlight = highlight;
            // The map spawn is the hero's start; Start() snaps the transform there at runtime, set it here for the editor view.
            hero.transform.position = map.HeroSpawn;
            return hero;
        }

        // Existing CC0 placeholder, with restrained metal response and its original texture and animations.
        static GameObject BuildHeroModel()
        {
            if (UseBreakwaterHero)
            {
                var breakwater = AssetDatabase.LoadAssetAtPath<GameObject>(HeroBreakwaterBuilder.PrefabPath);
                if (breakwater == null) throw new FileNotFoundException(HeroBreakwaterBuilder.PrefabPath);
                return breakwater;
            }
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
            if (source == null) throw new FileNotFoundException(HeroPrefabPath);

            var body = NewMaterial("HeroBody", "Universal Render Pipeline/Lit");
            body.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/Warrior_Texture.png"));
            body.SetColor("_BaseColor", new Color(0.88f, 0.9f, 0.98f));
            body.SetFloat("_Metallic", 0.18f);
            body.SetFloat("_Smoothness", 0.32f);
            var blade = NewMaterial("HeroBlade", "Universal Render Pipeline/Lit");
            blade.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/Warrior_Sword_Texture.png"));
            blade.SetColor("_BaseColor", new Color(0.92f, 0.97f, 1f));
            blade.SetFloat("_Metallic", 0.6f);
            blade.SetFloat("_Smoothness", 0.55f);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            // Unpacked, so the new prefab holds its materials directly instead of overrides aimed at the source prefab.
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                var materialsOnRenderer = renderer.sharedMaterials;
                for (var i = 0; i < materialsOnRenderer.Length; i++)
                    materialsOnRenderer[i] = materialsOnRenderer[i] != null && materialsOnRenderer[i].name.ToLowerInvariant().Contains("sword") ? blade : body;
                renderer.sharedMaterials = materialsOnRenderer;
            }
            // The materials above only exist in memory until saved; the prefab must point at files, not at objects.
            AssetDatabase.SaveAssets();
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, GenRoot + "HeroModel.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        static Camera BuildCamera(ArenaMap map, Transform hero)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var camera = go.GetComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 300f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.65f, 0.78f, 0.86f);
            camera.allowHDR = true;
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;

            var rts = go.AddComponent<RtsCamera>();
            rts.follow = hero;
            rts.map = map;
            go.AddComponent<ControlsHint>();
            var arena = map.ArenaBounds;
            rts.limits = new UnityEngine.Bounds(arena.center, arena.size + new Vector3(8f, 0f, 8f));
            hero.GetComponent<HeroController>().viewCamera = camera;
            go.transform.position = map.HeroSpawn + new Vector3(0f, 22f, -16f);
            go.transform.rotation = Quaternion.Euler(rts.pitch, 0f, 0f);
            return camera;
        }
    }
}

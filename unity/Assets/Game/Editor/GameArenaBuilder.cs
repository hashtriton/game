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
    /// Builds the grim night arena scene from the measured Life in Arena 3.9c layout:
    /// terrain, props, light, post processing, camera and a hero that runs on the original pathing grid.
    /// </summary>
    public static partial class GameArenaBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/Arena.unity";
        const string LayoutPath = "Assets/Arena/Data/lia39-layout.json";
        const string HeroPrefabPath = "Assets/Arena/Generated/Warrior.prefab";

        [MenuItem("Game/Build Arena scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");

            EnsureFolders();
            ImportTextures();
            BuildMaterials();
            ConfigurePipeline();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var mapObject = new GameObject("Arena Map");
            var map = mapObject.AddComponent<ArenaMap>();
            map.layoutJson = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutPath);
            if (map.layoutJson == null) throw new FileNotFoundException(LayoutPath);
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
            BuildCamera(map, hero.transform);
            BuildAir(map, hero.transform);

            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Arena scene failed to save.");
            AssetDatabase.SaveAssets();

            var summary = new System.Text.StringBuilder("ARENA_BUILT");
            foreach (var pair in placed) summary.Append(' ').Append(pair.Key).Append('=').Append(pair.Value);
            summary.Append(" meshes=").Append(meshes.Count);
            Debug.Log(summary.ToString());
        }

        static void EnsureFolders()
        {
            foreach (var path in new[] { "Assets/Game/Scenes", "Assets/Game/Generated" })
                Directory.CreateDirectory(path);
            AssetDatabase.DeleteAsset(MeshRoot.TrimEnd('/'));
            AssetDatabase.DeleteAsset(MatRoot.TrimEnd('/'));
            AssetDatabase.CreateFolder("Assets/Game/Generated", "Meshes");
            AssetDatabase.CreateFolder("Assets/Game/Generated", "Materials");
            meshes.Clear();
            // Packed masks derive from the roughness textures; regenerate them in case those changed.
            foreach (var mask in Directory.GetFiles(GenRoot.TrimEnd('/'), "*_mask.png"))
                AssetDatabase.DeleteAsset(mask.Replace('\\', '/'));
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
                TerrainLayerFor("Dirt", "ground", 12f, new Color(1f, 0.93f, 0.84f), 0.45f),
                TerrainLayerFor("Cobble", "cobble", 4f, new Color(0.9f, 0.9f, 0.95f), 0.55f),
                TerrainLayerFor("Paving", "stone", 3.5f, new Color(0.82f, 0.84f, 0.9f), 0.6f),
                TerrainLayerFor("DeadGrass", "ground", 6f, new Color(0.5f, 0.56f, 0.36f), 0.3f)
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
                    for (var dz = 0; dz < 2; dz++)
                    {
                        for (var dx = 0; dx < 2; dx++)
                        {
                            var vertex = source.vertices[(vz + dz) * source.width + vx + dx];
                            var layer = LayerIndex(source.groundTextures[vertex.groundTextureIndex]);
                            weights[z, x, layer] += (dx == 0 ? 1f - fx : fx) * (dz == 0 ? 1f - fz : fz);
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
            var moon = new GameObject("Moon").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.62f, 0.7f, 0.92f);
            moon.intensity = 1.9f;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = 0.9f;
            moon.transform.rotation = Quaternion.Euler(48f, -38f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.29f, 0.45f);
            RenderSettings.ambientEquatorColor = new Color(0.17f, 0.21f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.11f, 0.14f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.10f);
            RenderSettings.fogDensity = 0.014f;
            RenderSettings.skybox = null;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, GenRoot + "GamePost.asset");

            var tonemapping = AddEffect<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);
            var bloom = AddEffect<Bloom>(profile);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(0.8f);
            bloom.scatter.Override(0.7f);
            bloom.highQualityFiltering.Override(true);
            var vignette = AddEffect<Vignette>(profile);
            vignette.intensity.Override(0.42f);
            vignette.smoothness.Override(0.5f);
            var adjustments = AddEffect<ColorAdjustments>(profile);
            adjustments.contrast.Override(16f);
            adjustments.saturation.Override(-16f);
            adjustments.colorFilter.Override(new Color(0.92f, 0.98f, 1f));
            var lift = AddEffect<LiftGammaGain>(profile);
            lift.lift.Override(new Vector4(0.96f, 0.99f, 1.06f, 0f));
            var grain = AddEffect<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.3f);
            grain.response.Override(0.8f);
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

        static GameObject BuildHero(ArenaMap map)
        {
            var hero = new GameObject("Hero");
            var ring = Prop("Selection ring", hero.transform, FlatQuadMesh("quad_flat"), materials["Ring"],
                Vector3.zero, Quaternion.identity, new Vector3(1.9f, 1f, 1.9f), false, false);
            ring.transform.localPosition = new Vector3(0f, 0.07f, 0f);

            var marker = new GameObject("Move marker");
            Prop("Ring", marker.transform, FlatQuadMesh("quad_flat"), materials["Ring"], Vector3.zero, Quaternion.identity, Vector3.one, false, false);
            var clickMarker = marker.AddComponent<ClickMarker>();

            // A faint personal light keeps the hero readable in the dark without flattening the scene.
            var glow = new GameObject("Hero light").AddComponent<Light>();
            glow.transform.SetParent(hero.transform, false);
            glow.transform.localPosition = new Vector3(0f, 2.6f, -0.6f);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.86f, 0.68f);
            glow.intensity = 2.6f;
            glow.range = 7.5f;
            glow.shadows = LightShadows.None;

            hero.AddComponent<ArenaActor>();
            var controller = hero.AddComponent<HeroController>();
            controller.map = map;
            controller.modelPrefab = BuildHeroModel();
            controller.marker = clickMarker;
            // The map spawn is the hero's start; Start() snaps the transform there at runtime, set it here for the editor view.
            hero.transform.position = map.HeroSpawn;
            return hero;
        }

        // Placeholder hero: the existing CC0 Warrior with its animation clips, re-skinned with duller, darker materials
        // so it does not read as a toy under torchlight. Own prefab, the shared source assets stay untouched.
        static GameObject BuildHeroModel()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
            if (source == null) throw new FileNotFoundException(HeroPrefabPath);

            var body = NewMaterial("HeroBody", "Universal Render Pipeline/Lit");
            body.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/Warrior_Texture.png"));
            body.SetColor("_BaseColor", new Color(0.55f, 0.53f, 0.58f));
            body.SetFloat("_Metallic", 0.25f);
            body.SetFloat("_Smoothness", 0.35f);
            var blade = NewMaterial("HeroBlade", "Universal Render Pipeline/Lit");
            blade.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/Warrior_Sword_Texture.png"));
            blade.SetColor("_BaseColor", new Color(0.7f, 0.72f, 0.78f));
            blade.SetFloat("_Metallic", 0.6f);
            blade.SetFloat("_Smoothness", 0.55f);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                var materialsOnRenderer = renderer.sharedMaterials;
                for (var i = 0; i < materialsOnRenderer.Length; i++)
                    materialsOnRenderer[i] = materialsOnRenderer[i] != null && materialsOnRenderer[i].name.ToLowerInvariant().Contains("sword") ? blade : body;
                renderer.sharedMaterials = materialsOnRenderer;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, GenRoot + "HeroModel.prefab");
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        static void BuildCamera(ArenaMap map, Transform hero)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            go.tag = "MainCamera";
            var camera = go.GetComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 300f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f);
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
        }
    }
}

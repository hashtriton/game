using System.Collections.Generic;
using Arena;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static partial class GameArenaBuilder
    {
        enum Kind
        {
            None, Wall, Pillar, Lattice, Slab, Barrel, BarrelExplosive, Crate, Rock, Debris,
            Crystal, Mushroom, Statue, Tree, Brazier, OmniLight, Blood, Scorch
        }

        static readonly Dictionary<Kind, int> placed = new Dictionary<Kind, int>();
        static GameObject breakEffectPrefab;

        static Kind Classify(MapDoodad d)
        {
            var name = d.name ?? string.Empty;
            switch (d.id)
            {
                case "LTbr": case "LTbs": case "D01F": case "D01G": return Kind.Barrel;
                case "LTex": return Kind.BarrelExplosive;
                case "LTba": return Kind.Crate;
                case "D025": return Kind.Brazier;
                case "D02D": return Kind.OmniLight;
                case "D003": return Kind.Lattice;
                case "D01Q": return Kind.None; // tree roots
            }
            if (name.StartsWith("blood")) return Kind.Blood;
            if (name.StartsWith("DemonFoot")) return Kind.Scorch;
            if (name.StartsWith("Handcuffs") || name == "Chain") return Kind.None;

            switch (d.category)
            {
                case "pillar": return Kind.Pillar;
                case "statue": return Kind.Statue;
                case "wall": return Kind.Wall;
                case "floor": return Kind.Slab;
                case "plant": return Kind.Mushroom;
                case "tree": return Kind.Tree;
                case "rock":
                    if (name.StartsWith("Icecrown_Crystal")) return Kind.Crystal;
                    return name.StartsWith("R1_debris") ? Kind.Debris : Kind.Rock;
                case "unresolved":
                    if (!HasBounds(d)) return Kind.None;
                    if (name.StartsWith("R1_ring")) return Kind.Slab;
                    return Kind.Wall;
            }
            return Kind.None;
        }

        static bool HasBounds(MapDoodad d) =>
            d.geometryBoundsAvailable && d.geometryBoundsMin != null && d.geometryBoundsMin.Length == 3 &&
            d.geometryBoundsMax != null && d.geometryBoundsMax.Length == 3;

        // Original model axes are (X, Y, Z-up). Unity local axes are (X, Z-up as Y, Y as Z).
        static void ReadBounds(MapDoodad d, Vector3 scale, float unitsPerMeter, out Vector3 size, out Vector3 center)
        {
            var min = d.geometryBoundsMin;
            var max = d.geometryBoundsMax;
            size = Vector3.Scale(new Vector3(max[0] - min[0], max[2] - min[2], max[1] - min[1]), scale) / unitsPerMeter;
            center = Vector3.Scale(new Vector3((min[0] + max[0]) * 0.5f, (min[2] + max[2]) * 0.5f, (min[1] + max[1]) * 0.5f), scale) / unitsPerMeter;
        }

        static float Hash01(int seed, int salt)
        {
            unchecked
            {
                var h = (uint)(seed * 73856093) ^ (uint)(salt * 19349663);
                h ^= h >> 13; h *= 1274126177u; h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static GameObject Prop(string name, Transform parent, Mesh mesh, Material material, Vector3 position, Quaternion rotation,
            Vector3 scale, bool isStatic, bool castShadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = castShadows;
            if (isStatic) GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
            return go;
        }

        static void PlaceDoodads(ArenaMap map, Transform root)
        {
            placed.Clear();
            var walls = Group(root, "Walls and ruins");
            var barrels = Group(root, "Barrels and crates");
            var nature = Group(root, "Rocks, trees, growth");
            var lights = Group(root, "Fire and light");
            var decals = Group(root, "Ground decals");
            var scale = map.unitsPerMeter;

            var stones = new[] { materials["Stone"], materials["StoneWarm"], materials["StoneCold"] };
            var barrelMesh = BarrelMesh();
            var rocks = new[] { RockMesh(1), RockMesh(2), RockMesh(3), RockMesh(4), RockMesh(5) };

            foreach (var d in map.Layout.doodads)
            {
                var kind = Classify(d);
                if (kind == Kind.None) continue;
                placed[kind] = placed.TryGetValue(kind, out var count) ? count + 1 : 1;

                var position = new Vector3(d.x, d.z, d.y) / scale;
                var yaw = Quaternion.Euler(0f, -d.rotationRadians * Mathf.Rad2Deg, 0f);
                var factor = d.scale != null && d.scale.Length == 3 ? new Vector3(d.scale[0], d.scale[2], d.scale[1]) : Vector3.one;
                var stone = stones[d.editorId % stones.Length];
                var label = d.id + "#" + d.editorId;

                switch (kind)
                {
                    case Kind.Wall:
                    case Kind.Slab:
                    case Kind.Pillar:
                    case Kind.Lattice:
                    {
                        if (!HasBounds(d)) break;
                        ReadBounds(d, factor, scale, out var size, out var center);
                        if (kind == Kind.Slab) size.y = Mathf.Max(size.y, 0.24f);
                        if (kind == Kind.Wall && size.y > Mathf.Max(size.x, size.z) * 1.3f) kind = Kind.Pillar;
                        Mesh mesh;
                        Material material;
                        switch (kind)
                        {
                            case Kind.Pillar: mesh = PillarMesh(size, 3f); material = stone; break;
                            case Kind.Lattice: mesh = LatticeMesh(size, 3f); material = materials["Rust"]; break;
                            case Kind.Slab: mesh = BoxMesh(size, 8f); material = materials["Cobble"]; break;
                            default: mesh = WallMesh(size, 3f); material = stone; break;
                        }
                        var masonry = Prop(label, walls, mesh, material, position + yaw * center, yaw, Vector3.one, true);
                        if (kind == Kind.Slab && Mathf.Min(size.x, size.z) >= 1.5f)
                            Prop("Stone curb", masonry.transform, SlabCurbMesh(size), materials["StoneWarm"], masonry.transform.position, yaw, Vector3.one, true);
                        if (kind == Kind.Wall || kind == Kind.Pillar) DecorateStone(masonry.transform, size, d.editorId, kind == Kind.Pillar, map);
                        break;
                    }
                    case Kind.Barrel:
                    case Kind.BarrelExplosive:
                    {
                        var turn = Quaternion.Euler(0f, Hash01(d.editorId, 1) * 360f, 0f);
                        var s = new Vector3(1f, 0.96f + Hash01(d.editorId, 2) * 0.1f, 1f);
                        var material = kind == Kind.Barrel ? materials["Barrel"] : materials["BarrelExplosive"];
                        var barrel = Prop("Barrel#" + d.editorId, barrels, barrelMesh, material, position, turn, s, false);
                        Prop("Iron hoops", barrel.transform, BarrelHoopsMesh(), materials["Rust"], barrel.transform.position, turn, Vector3.one, false);
                        // Only barrels that block the pathing grid can be cut through; stacked-barrel scenery stays as it is.
                        if (string.IsNullOrEmpty(d.pathingTexture)) break;
                        var breakable = barrel.AddComponent<Destructible>();
                        // User rule, 2026-10-09: one hero hit opens a passage, even without item bonuses.
                        breakable.maxHealth = 1f;
                        breakable.map = map;
                        breakable.editorId = d.editorId;
                        breakable.radius = 0.42f;
                        breakable.height = 0.95f;
                        breakable.explosive = kind == Kind.BarrelExplosive;
                        breakable.breakEffect = breakEffectPrefab;
                        break;
                    }
                    case Kind.Crate:
                    {
                        var size = HasBounds(d) ? BoundsSize(d, factor, scale) : Vector3.one * 1.1f;
                        var crate = Group(barrels, "Crate#" + d.editorId);
                        crate.SetPositionAndRotation(position + yaw * (Vector3.up * size.y * 0.5f), yaw);
                        var turn = yaw * Quaternion.Euler(0f, (Hash01(d.editorId, 131) - 0.5f) * 8f, 0f);
                        var visualScale = Vector3.one * (0.96f + Hash01(d.editorId, 132) * 0.06f);
                        Prop("Planks and frame", crate, CrateMesh(size, false), materials["Crate"], crate.position, turn, visualScale, false);
                        Prop("Corner brackets", crate, CrateMesh(size, true), materials["Rust"], crate.position, turn, visualScale, false);
                        if (d.editorId % 5 == 0)
                        {
                            var plankPoint = position + yaw * new Vector3(size.x * 0.8f, 0f, size.z * 0.4f);
                            plankPoint.y = map.SampleHeight(plankPoint) + 0.028f;
                            Prop("Broken plank", crate, BoxMesh(new Vector3(0.17f, 0.04f, 0.65f), 1.1f), materials["Crate"], plankPoint,
                                turn * Quaternion.Euler(0f, 28f, 0f), Vector3.one, true);
                        }
                        break;
                    }
                    case Kind.Rock:
                    {
                        var s = 1.1f + Hash01(d.editorId, 3) * 1.1f;
                        var turn = Quaternion.Euler(0f, Hash01(d.editorId, 4) * 360f, 0f);
                        Prop(label, nature, rocks[d.editorId % rocks.Length], stone, position, turn,
                            new Vector3(s, s * (0.7f + Hash01(d.editorId, 5) * 0.5f), s), true);
                        break;
                    }
                    case Kind.Debris:
                    {
                        ReadBounds(d, factor, scale, out var size, out var center);
                        var s = new Vector3(size.x / 0.9f, Mathf.Max(size.y, 0.18f) / 0.55f, size.z / 0.9f);
                        Prop(label, nature, rocks[d.editorId % rocks.Length], stone, position + yaw * center, yaw, s, true);
                        break;
                    }
                    case Kind.Crystal:
                    {
                        var h = 1.0f + Hash01(d.editorId, 6) * 1.4f;
                        var tilt = Quaternion.Euler((Hash01(d.editorId, 7) - 0.5f) * 24f, Hash01(d.editorId, 8) * 360f, (Hash01(d.editorId, 9) - 0.5f) * 24f);
                        var crystal = Prop(label, nature, CrystalMesh(), materials["Crystal"], position, tilt, new Vector3(h * 0.7f, h, h * 0.7f), false);
                        Prop("Crystal shard", crystal.transform, CrystalMesh(), materials["Crystal"], crystal.transform.TransformPoint(new Vector3(0.24f, 0.02f, 0.03f)),
                            tilt * Quaternion.Euler(0f, 24f, -22f), Vector3.one * 0.58f, false);
                        var spill = new GameObject("Crystal light").AddComponent<Light>();
                        spill.transform.SetParent(crystal.transform, false);
                        spill.transform.localPosition = Vector3.up * 0.65f;
                        spill.type = LightType.Point;
                        spill.color = new Color(0.08f, 0.35f, 1f);
                        spill.intensity = 0.8f;
                        spill.range = 3f;
                        spill.shadows = LightShadows.None;
                        break;
                    }
                    case Kind.Mushroom:
                    {
                        var material = materials[Hash01(d.editorId, 10) > 0.5f ? "MushroomTeal" : "MushroomViolet"];
                        for (var i = 0; i < 3; i++)
                        {
                            var offset = new Vector3((Hash01(d.editorId, 20 + i) - 0.5f) * 0.5f, 0f, (Hash01(d.editorId, 30 + i) - 0.5f) * 0.5f);
                            var s = 0.8f + Hash01(d.editorId, 40 + i) * 1.1f;
                            Prop(label, nature, MushroomMesh(), material, position + offset, Quaternion.identity, Vector3.one * s, false, false);
                        }
                        break;
                    }
                    case Kind.Statue:
                        Prop(label, walls, StatueMesh(), stone, position, yaw, Vector3.one, true);
                        break;
                    case Kind.Tree:
                    {
                        var h = (d.id == "D00B" ? 3.4f : 4.4f) + Hash01(d.editorId, 11) * 1.6f;
                        var turn = Quaternion.Euler(0f, Hash01(d.editorId, 12) * 360f, 0f);
                        var foliage = materials[d.editorId % 4 == 0 ? "LeavesSun" : "Leaves"];
                        var tree = Prop(label, nature, ConiferMesh(d.editorId % 4), foliage, position, turn, new Vector3(h * 0.65f, h, h * 0.65f), false);
                        Prop("Trunk", tree.transform, TreeTrunkMesh(), materials["Crate"], position, turn, Vector3.one, false);
                        break;
                    }
                    case Kind.Blood:
                    case Kind.Scorch:
                    {
                        var size = HasBounds(d) ? Mathf.Max(BoundsSize(d, factor, scale).x, 2.2f) : 2.6f;
                        var material = materials[kind == Kind.Blood ? "Blood" : "Scorch"];
                        Prop(label, decals, FlatQuadMesh("quad_flat"), material, position + Vector3.up * 0.035f,
                            Quaternion.Euler(0f, Hash01(d.editorId, 13) * 360f, 0f), new Vector3(size, 1f, size), true, false);
                        break;
                    }
                    case Kind.Brazier:
                        BuildBrazier(label, lights, position);
                        break;
                    case Kind.OmniLight:
                        BuildOmniLight(label, lights, position + Vector3.up * 1.4f);
                        break;
                }
            }
        }

        static Vector3 BoundsSize(MapDoodad d, Vector3 factor, float unitsPerMeter)
        {
            ReadBounds(d, factor, unitsPerMeter, out var size, out _);
            return size;
        }

        static void DecorateStone(Transform root, Vector3 size, int seed, bool pillar, ArenaMap map)
        {
            if (size.y < 1.3f) return;
            var inward = root.InverseTransformDirection(map.ArenaBounds.center - root.position);
            var faceX = pillar ? Mathf.Abs(inward.x) > Mathf.Abs(inward.z) : size.x < size.z;
            var normal = faceX ? new Vector3(inward.x < 0f ? -1f : 1f, 0f, 0f) : new Vector3(0f, 0f, inward.z < 0f ? -1f : 1f);
            var width = faceX ? size.z : size.x;
            var face = Vector3.Scale(normal, size) * (pillar ? 0.4f : 0.5f) + normal * 0.035f;
            var facing = root.rotation * Quaternion.LookRotation(normal);
            if (seed % 3 == 0)
            {
                var basePoint = face - Vector3.up * (size.y * 0.5f - 0.015f) + normal * 0.1f;
                var worldPoint = root.TransformPoint(basePoint);
                worldPoint.y = Mathf.Max(worldPoint.y, map.SampleHeight(worldPoint) + 0.015f);
                Prop("Joint grass", root, GrassTuftMesh(Mathf.Min(width, 3f) * 0.27f), materials["FoliageLight"], worldPoint,
                    facing, Vector3.one, true);
            }
            if (seed % 5 == 0)
            {
                var basePoint = root.TransformPoint(face - Vector3.up * (size.y * 0.5f - 0.05f) + normal * 0.16f);
                basePoint.y = Mathf.Max(basePoint.y, map.SampleHeight(basePoint) + 0.015f);
                Prop("Wall growth", root, BushMesh(seed % 3), materials["Leaves"], basePoint, facing,
                    Vector3.one * (0.65f + Hash01(seed, 99) * 0.5f), true);
            }
            if (seed % 7 == 0 && size.y > 2f)
            {
                var basePoint = root.position;
                basePoint.y = map.SampleHeight(basePoint) + 0.12f;
                Prop("Weathered footing", root, BoxMesh(new Vector3(size.x * 1.015f, 0.24f, size.z * 1.015f), 3f), materials["StoneBase"], basePoint, root.rotation, Vector3.one, true);
            }
            if (seed % 5 == 0)
            {
                var point = root.TransformPoint(face + normal * 0.28f);
                point.y = map.SampleHeight(point) + 0.1f;
                Prop("Fallen stone", root, BoxMesh(new Vector3(0.36f, 0.24f, 0.49f), 3f), materials["StoneWarm"], point,
                    root.rotation * Quaternion.Euler(12f, Hash01(seed, 291) * 180f, -8f), Vector3.one, true);
            }
            if (pillar && size.y > 2.6f && width > 0.9f)
            {
                var top = root.TransformPoint(face + normal * 0.09f + Vector3.up * (size.y * 0.5f - 0.26f));
                var bannerWidth = Mathf.Min(width * 0.82f, 1.55f);
                var bannerHeight = Mathf.Min(size.y * 0.56f, 2.6f);
                Prop("Banner", root, BannerMesh(), materials[Hash01(seed, 308) > 0.5f ? "ClothOrange" : "ClothBlue"], top, facing, new Vector3(bannerWidth, bannerHeight, bannerWidth), true);
                Prop("Banner rail", root, BoxMesh(new Vector3(bannerWidth + 0.16f, 0.055f, 0.065f), 1f), materials["Rust"], top, facing, Vector3.one, true);
                if (seed % 3 == 0 && Vector2.Distance(new Vector2(root.position.x, root.position.z),
                    new Vector2(map.ArenaBounds.center.x, map.ArenaBounds.center.z)) < 28f)
                {
                    var torchPoint = root.TransformPoint(face + normal * 0.25f + (Quaternion.LookRotation(normal) * Vector3.right) * (width * 0.3f));
                    torchPoint.y = Mathf.Max(torchPoint.y, map.SampleHeight(torchPoint) + 1.4f);
                    BuildBrazier("Pillar torch", root, torchPoint, 0.55f, facing);
                }
            }
            if (seed % 3 != 0 || width < 0.5f) return;
            var ivyPosition = face + Vector3.up * (size.y * 0.5f - 0.13f);
            var tangent = Quaternion.LookRotation(normal) * Vector3.right;
            ivyPosition += tangent * (Mathf.Min(width, 2.2f) * 0.24f);
            Prop("Ivy", root, IvyMesh(), materials[seed % 2 == 0 ? "Leaves" : "LeavesSun"], root.TransformPoint(ivyPosition), facing,
                new Vector3(Mathf.Min(width, 1.8f), Mathf.Min(size.y * 0.55f, 1.8f), 1f), true);
        }

        static void BuildBrazier(string label, Transform parent, Vector3 position, float scale = 1f, Quaternion? rotation = null)
        {
            var root = new GameObject("Brazier " + label).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.identity);
            Prop("Bowl", root, BrazierMesh(), materials["Rust"], position, Quaternion.identity, Vector3.one * 1.15f, true);
            Prop("Sconce plate", root, BoxMesh(new Vector3(0.32f, 0.66f, 0.09f), 1f), materials["Rust"], position + new Vector3(0f, 0.58f, 0.39f), Quaternion.identity, Vector3.one, true);
            Prop("Sconce bracket", root, BoxMesh(new Vector3(0.10f, 0.10f, 0.39f), 1f), materials["Rust"], position + new Vector3(0f, 0.51f, 0.19f), Quaternion.identity, Vector3.one, true);
            BuildFire(root, new Vector3(0f, 0.95f, 0f));

            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.65f, 0.32f);
            light.intensity = 2.2f;
            light.range = 5f;
            light.shadows = LightShadows.None;
            lightObject.AddComponent<FlickerLight>();
            root.localScale = Vector3.one * scale;
            if (rotation.HasValue) root.rotation = rotation.Value;
        }

        static void BuildOmniLight(string label, Transform parent, Vector3 position)
        {
            var go = new GameObject("Glow " + label);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.74f, 0.42f);
            light.intensity = 3.2f;
            light.range = 8f;
            light.shadows = LightShadows.None;
            go.AddComponent<FlickerLight>().amplitude = 0.12f;
        }

        static void BuildFire(Transform parent, Vector3 localPosition)
        {
            var flame = new GameObject("Flame");
            flame.transform.SetParent(parent, false);
            flame.transform.localPosition = localPosition;
            var ps = flame.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.34f, 0.48f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.85f, 1.15f);
            main.startSizeZ = 1f;
            main.maxParticles = 12;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.gravityModifier = -0.05f;
            var emission = ps.emission;
            emission.rateOverTime = 9f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 4f;
            shape.radius = 0.055f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Grid;
            sheet.numTilesX = 4;
            sheet.numTilesY = 2;
            sheet.animation = ParticleSystemAnimationType.WholeSheet;
            sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
            sheet.cycleCount = 1;
            var color = ps.colorOverLifetime;
            color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.7f, 0.7f), new GradientAlphaKey(0f, 1f) });
            color.color = fade;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.45f));
            var flameRenderer = flame.GetComponent<ParticleSystemRenderer>();
            flameRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            flameRenderer.sharedMaterial = materials["Flame"];
            flameRenderer.pivot = new Vector3(0f, 0.5f, 0f);
            flameRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            flameRenderer.receiveShadows = false;

            for (var layer = 1; layer <= 2; layer++)
            {
                var inner = Object.Instantiate(flame, parent, false);
                inner.name = layer == 1 ? "Flame mid" : "Flame core";
                var system = inner.GetComponent<ParticleSystem>();
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var innerMain = system.main;
                innerMain.startSizeX = layer == 1 ? 0.3f : 0.16f;
                innerMain.startSizeY = layer == 1 ? 0.85f : 0.65f;
                innerMain.startLifetime = layer == 1 ? 0.65f : 0.4f;
                innerMain.maxParticles = 6;
                var innerEmission = system.emission;
                innerEmission.rateOverTime = 7f;
                inner.GetComponent<ParticleSystemRenderer>().sharedMaterial = materials[layer == 1 ? "FlameMid" : "FlameCore"];
            }

            var sparks = new GameObject("Embers");
            sparks.transform.SetParent(parent, false);
            sparks.transform.localPosition = localPosition;
            var sparkSystem = sparks.AddComponent<ParticleSystem>();
            var sparkMain = sparkSystem.main;
            sparkMain.loop = true;
            sparkMain.prewarm = true;
            sparkMain.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 3.2f);
            sparkMain.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            sparkMain.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
            sparkMain.maxParticles = 30;
            sparkMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sparkMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
            sparkMain.gravityModifier = -0.02f;
            var sparkEmission = sparkSystem.emission;
            sparkEmission.rateOverTime = 6f;
            var sparkShape = sparkSystem.shape;
            sparkShape.shapeType = ParticleSystemShapeType.Cone;
            sparkShape.angle = 14f;
            sparkShape.radius = 0.22f;
            sparkShape.rotation = new Vector3(-90f, 0f, 0f);
            var noise = sparkSystem.noise;
            noise.enabled = true;
            noise.strength = 0.35f;
            noise.frequency = 0.6f;
            var sparkColor = sparkSystem.colorOverLifetime;
            sparkColor.enabled = true;
            var sparkFade = new Gradient();
            sparkFade.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.7f, 0.3f), 0f), new GradientColorKey(new Color(1f, 0.25f, 0.05f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
            sparkColor.color = sparkFade;
            var sparkRenderer = sparks.GetComponent<ParticleSystemRenderer>();
            sparkRenderer.sharedMaterial = materials["Ember"];
            sparkRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sparkRenderer.receiveShadows = false;
        }

        // Low flat mist sheets over the whole arena and drifting ash around the hero.
        static void BuildAir(ArenaMap map, Transform hero)
        {
            var arena = map.ArenaBounds;

            var mist = new GameObject("Ground mist");
            mist.transform.position = new Vector3(arena.center.x, 0.9f, arena.center.z);
            var mistSystem = mist.AddComponent<ParticleSystem>();
            var mistMain = mistSystem.main;
            mistMain.loop = true;
            mistMain.prewarm = true;
            mistMain.duration = 30f;
            mistMain.startLifetime = new ParticleSystem.MinMaxCurve(22f, 30f);
            mistMain.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            mistMain.startSize = new ParticleSystem.MinMaxCurve(9f, 16f);
            mistMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            mistMain.maxParticles = 80;
            mistMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var mistEmission = mistSystem.emission;
            mistEmission.rateOverTime = 3.2f;
            var mistShape = mistSystem.shape;
            mistShape.shapeType = ParticleSystemShapeType.Box;
            mistShape.scale = new Vector3(arena.size.x, 1.2f, arena.size.z);
            mistShape.randomDirectionAmount = 1f;
            var mistColor = mistSystem.colorOverLifetime;
            mistColor.enabled = true;
            var mistFade = new Gradient();
            mistFade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            mistColor.color = mistFade;
            var mistRenderer = mist.GetComponent<ParticleSystemRenderer>();
            mistRenderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            mistRenderer.sharedMaterial = materials["Mist"];
            mistRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mistRenderer.receiveShadows = false;

            var ash = new GameObject("Drifting ash");
            ash.transform.SetParent(hero, false);
            ash.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            var ashSystem = ash.AddComponent<ParticleSystem>();
            var ashMain = ashSystem.main;
            ashMain.loop = true;
            ashMain.prewarm = true;
            ashMain.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
            ashMain.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            ashMain.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
            ashMain.maxParticles = 600;
            ashMain.simulationSpace = ParticleSystemSimulationSpace.World;
            var ashEmission = ashSystem.emission;
            ashEmission.rateOverTime = 55f;
            var ashShape = ashSystem.shape;
            ashShape.shapeType = ParticleSystemShapeType.Box;
            ashShape.scale = new Vector3(36f, 7f, 36f);
            ashShape.randomDirectionAmount = 1f;
            var ashNoise = ashSystem.noise;
            ashNoise.enabled = true;
            ashNoise.strength = 0.6f;
            ashNoise.frequency = 0.25f;
            var ashColor = ashSystem.colorOverLifetime;
            ashColor.enabled = true;
            ashColor.color = mistFade;
            var ashRenderer = ash.GetComponent<ParticleSystemRenderer>();
            ashRenderer.sharedMaterial = materials["Ash"];
            ashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ashRenderer.receiveShadows = false;
        }

        // The central healing well: stone rim, glowing water and a cold light so it reads from across the arena.
        static void BuildWell(Transform parent, Vector3 position)
        {
            var root = new GameObject("Well").transform;
            root.SetParent(parent, false);
            root.position = position;
            Prop("Rim", root, WellMesh(), materials["StoneCold"], position, Quaternion.identity, Vector3.one, true);
            Prop("Water", root, DiscMesh("disc_water", 1.45f, 24), materials["Water"], position + Vector3.up * 0.62f, Quaternion.identity, Vector3.one, false, false);

            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.25f, 0.95f, 0.85f);
            light.intensity = 7f;
            light.range = 11f;
            light.shadows = LightShadows.None;
        }
    }
}

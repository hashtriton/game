using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    public static partial class GameArenaBuilder
    {
        // Meshes are generated, deduplicated by a quantised key and stored as assets so the scene only references them.
        static readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        sealed class MeshData
        {
            readonly List<Vector3> vertices = new List<Vector3>();
            readonly List<Vector3> normals = new List<Vector3>();
            readonly List<Vector2> uvs = new List<Vector2>();
            readonly List<int> triangles = new List<int>();

            // Corners are bottom-left, bottom-right, top-right, top-left as seen from outside; Unity front faces are clockwise.
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 na, Vector3 nb, Vector3 nc, Vector3 nd,
                Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
            {
                var i = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
                normals.Add(na); normals.Add(nb); normals.Add(nc); normals.Add(nd);
                uvs.Add(ua); uvs.Add(ub); uvs.Add(uc); uvs.Add(ud);
                triangles.AddRange(new[] { i, i + 3, i + 2, i, i + 2, i + 1 });
            }

            public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 n, Vector2 ua, Vector2 ub, Vector2 uc)
            {
                Tri(a, b, c, n, n, n, ua, ub, uc);
            }

            public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 na, Vector3 nb, Vector3 nc, Vector2 ua, Vector2 ub, Vector2 uc)
            {
                var i = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                normals.Add(na); normals.Add(nb); normals.Add(nc);
                uvs.Add(ua); uvs.Add(ub); uvs.Add(uc);
                triangles.AddRange(new[] { i, i + 1, i + 2 });
            }

            public void Append(Mesh mesh, Vector3 offset)
            {
                var start = vertices.Count;
                foreach (var vertex in mesh.vertices) vertices.Add(vertex + offset);
                normals.AddRange(mesh.normals);
                uvs.AddRange(mesh.uv);
                foreach (var index in mesh.triangles) triangles.Add(start + index);
            }

            public Mesh ToMesh()
            {
                var mesh = new Mesh();
                if (vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }
        }

        static string Q(float value) => Mathf.RoundToInt(value * 20f).ToString();

        static Mesh Store(string key, MeshData data)
        {
            var mesh = data.ToMesh();
            mesh.name = key;
            var path = MeshRoot + key + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                // Mesh topology changes need the native buffers refreshed, as well as the saved asset.
                existing.Clear();
                existing.indexFormat = mesh.indexFormat;
                existing.vertices = mesh.vertices;
                existing.normals = mesh.normals;
                existing.uv = mesh.uv;
                existing.triangles = mesh.triangles;
                existing.tangents = mesh.tangents;
                existing.bounds = mesh.bounds;
                Object.DestroyImmediate(mesh);
                mesh = existing;
                EditorUtility.SetDirty(mesh);
            }
            else AssetDatabase.CreateAsset(mesh, path);
            meshes[key] = mesh;
            return mesh;
        }

        static void AddBoxFace(MeshData data, Vector3 origin, Vector3 u, Vector3 v, float uLength, float vLength, Vector3 normal, float tile)
        {
            var du = u * uLength;
            var dv = v * vLength;
            data.Quad(origin, origin + du, origin + du + dv, origin + dv, normal, normal, normal, normal,
                Vector2.zero, new Vector2(uLength / tile, 0f), new Vector2(uLength / tile, vLength / tile), new Vector2(0f, vLength / tile));
        }

        // Open-bottom box. UVs are in metres divided by the texture tile, so tiling stays uniform at any size.
        static void AddBox(MeshData data, Vector3 center, Vector3 size, float tile)
        {
            var h = size * 0.5f;
            AddBoxFace(data, center + new Vector3(-h.x, -h.y, -h.z), Vector3.right, Vector3.up, size.x, size.y, Vector3.back, tile);
            AddBoxFace(data, center + new Vector3(h.x, -h.y, h.z), Vector3.left, Vector3.up, size.x, size.y, Vector3.forward, tile);
            AddBoxFace(data, center + new Vector3(h.x, -h.y, -h.z), Vector3.forward, Vector3.up, size.z, size.y, Vector3.right, tile);
            AddBoxFace(data, center + new Vector3(-h.x, -h.y, h.z), Vector3.back, Vector3.up, size.z, size.y, Vector3.left, tile);
            AddBoxFace(data, center + new Vector3(-h.x, h.y, -h.z), Vector3.right, Vector3.forward, size.x, size.z, Vector3.up, tile);
        }

        static Mesh BoxMesh(Vector3 size, float tile)
        {
            var key = $"box_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}_{Q(tile)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            AddRoundedBox(data, Vector3.zero, size, tile, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.12f);
            return Store(key, data);
        }

        // Keep the measured outer bounds while softening the edges that catch the daylight key.
        static void AddRoundedBox(MeshData data, Vector3 center, Vector3 size, float tile, float bevel, int weatherSeed = -1)
        {
            var h = size * 0.5f;
            bevel = Mathf.Min(bevel, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * 0.45f);
            var inner = h - Vector3.one * bevel;
            const int steps = 6;

            float Coord(float half, int i) => i < 3 ? -half + bevel * i * 0.5f : half - bevel * (5 - i) * 0.5f;
            Vector3 Round(Vector3 p, out Vector3 normal)
            {
                var q = new Vector3(Mathf.Clamp(p.x, -inner.x, inner.x), Mathf.Clamp(p.y, -inner.y, inner.y), Mathf.Clamp(p.z, -inner.z, inner.z));
                normal = (p - q).normalized;
                var chip = weatherSeed < 0 ? 0f : Mathf.Pow(Mathf.PerlinNoise(p.x * 6f + weatherSeed * 0.31f, p.y * 7f + p.z * 4f + weatherSeed), 3f) * bevel * 0.48f;
                return center + q + normal * (bevel - chip);
            }

            void Face(Vector3 normal, Vector3 u, Vector3 v, float hu, float hv)
            {
                var origin = Vector3.Scale(normal, h);
                var textureOffset = weatherSeed < 0 ? Vector2.zero : new Vector2(Hash01(weatherSeed, 201), Hash01(weatherSeed, 202));
                for (var y = 0; y < steps - 1; y++)
                {
                    for (var x = 0; x < steps - 1; x++)
                    {
                        var ux0 = Coord(hu, x);
                        var ux1 = Coord(hu, x + 1);
                        var vy0 = Coord(hv, y);
                        var vy1 = Coord(hv, y + 1);
                        var a = Round(origin + u * ux0 + v * vy0, out var na);
                        var b = Round(origin + u * ux1 + v * vy0, out var nb);
                        var c = Round(origin + u * ux1 + v * vy1, out var nc);
                        var d = Round(origin + u * ux0 + v * vy1, out var nd);
                        data.Quad(a, b, c, d, na, nb, nc, nd,
                            new Vector2(ux0 / tile, vy0 / tile) + textureOffset, new Vector2(ux1 / tile, vy0 / tile) + textureOffset,
                            new Vector2(ux1 / tile, vy1 / tile) + textureOffset, new Vector2(ux0 / tile, vy1 / tile) + textureOffset);
                    }
                }
            }

            Face(Vector3.back, Vector3.right, Vector3.up, h.x, h.y);
            Face(Vector3.forward, Vector3.left, Vector3.up, h.x, h.y);
            Face(Vector3.right, Vector3.forward, Vector3.up, h.z, h.y);
            Face(Vector3.left, Vector3.back, Vector3.up, h.z, h.y);
            Face(Vector3.up, Vector3.right, Vector3.forward, h.x, h.z);
            Face(Vector3.down, Vector3.right, Vector3.back, h.x, h.z);
        }

        static Mesh WallMesh(Vector3 size, float tile)
        {
            var key = $"wall_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}_{Q(tile)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var alongX = size.x >= size.z;
            var length = alongX ? size.x : size.z;
            var rows = Mathf.Clamp(Mathf.RoundToInt(size.y / 0.65f), 1, 8);
            var columns = Mathf.Clamp(Mathf.RoundToInt(length / 1.5f), 1, 10);
            var blockWidth = length / columns;
            const float joint = 0.035f;
            var seed = Mathf.RoundToInt(size.x * 100f + size.z * 37f);
            var heights = new float[rows];
            var total = 0f;
            for (var row = 0; row < rows; row++) { heights[row] = 0.8f + Hash01(seed, row + 210) * 0.4f; total += heights[row]; }
            var bottom = -size.y * 0.5f;
            for (var row = 0; row < rows; row++)
            {
                var rowHeight = heights[row] / total * size.y;
                var start = -length * 0.5f;
                var offset = row % 2 == 1 && columns > 1;
                var index = 0;
                while (start < length * 0.5f - 0.001f)
                {
                    var width = blockWidth * (0.75f + Hash01(seed + row, index + 230) * 0.5f) * (offset ? 0.5f : 1f);
                    offset = false;
                    var end = Mathf.Min(start + width, length * 0.5f);
                    if (length * 0.5f - end < blockWidth * 0.25f) end = length * 0.5f;
                    var insetStart = start + (start > -length * 0.5f + 0.001f ? joint * 0.5f : 0f);
                    var insetEnd = end - (end < length * 0.5f - 0.001f ? joint * 0.5f : 0f);
                    var block = new Vector3(alongX ? insetEnd - insetStart : size.x, rowHeight - (rows > 1 ? joint : 0f), alongX ? size.z : insetEnd - insetStart);
                    var center = new Vector3(alongX ? (insetStart + insetEnd) * 0.5f : 0f, bottom + rowHeight * 0.5f, alongX ? 0f : (insetStart + insetEnd) * 0.5f);
                    if (rows > 1 && row == 0) { block.y += joint * 0.5f; center.y -= joint * 0.25f; }
                    if (rows > 1 && row == rows - 1) { block.y += joint * 0.5f; center.y += joint * 0.25f; }
                    if (row == rows - 1 && index % 3 == 1)
                    {
                        var loss = rowHeight * (0.1f + Hash01(seed, index + 245) * 0.23f);
                        block.y -= loss; center.y -= loss * 0.5f;
                    }
                    AddRoundedBox(data, center, block, tile, Mathf.Min(block.x, Mathf.Min(block.y, block.z)) * 0.14f, seed + row * 17 + index);
                    start = end;
                    index++;
                }
                bottom += rowHeight;
            }
            return Store(key, data);
        }

        static Mesh PillarMesh(Vector3 size, float tile)
        {
            var key = $"pillar_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}_{Q(tile)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var foot = Mathf.Min(0.45f, size.y * 0.12f);
            var cap = foot * 0.9f;
            var seed = Mathf.RoundToInt(size.x * 79f + size.z * 51f);
            AddRoundedBox(data, new Vector3(0f, -size.y * 0.5f + foot * 0.5f, 0f), new Vector3(size.x, foot, size.z), tile, foot * 0.2f, seed);
            var shaft = size.y - foot - cap;
            var courses = Mathf.Max(1, Mathf.RoundToInt(shaft / 0.78f));
            for (var course = 0; course < courses; course++)
            {
                var height = shaft / courses;
                var center = new Vector3((Hash01(seed, course + 260) - 0.5f) * size.x * 0.025f,
                    -size.y * 0.5f + foot + height * (course + 0.5f), (Hash01(seed, course + 270) - 0.5f) * size.z * 0.025f);
                var width = 0.76f + Hash01(seed, course + 280) * 0.045f;
                AddRoundedBox(data, center, new Vector3(size.x * width, height - 0.028f, size.z * width), tile, Mathf.Min(size.x, size.z) * 0.065f, seed + course);
            }
            for (var part = 0; part < 4; part++)
                AddRoundedBox(data, new Vector3((part % 2 - 0.5f) * size.x * 0.48f, size.y * 0.5f - cap * 0.5f - part * cap * 0.04f, (part / 2 - 0.5f) * size.z * 0.48f),
                    new Vector3(size.x * 0.46f, cap, size.z * 0.46f), tile, cap * 0.22f, seed + part + 7);
            return Store(key, data);
        }

        static Mesh SlabCurbMesh(Vector3 size)
        {
            var key = $"curb_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var width = Mathf.Min(0.18f, Mathf.Min(size.x, size.z) * 0.12f);
            foreach (var sign in new[] { -1f, 1f })
            {
                AddRoundedBox(data, new Vector3(0f, 0.025f, sign * (size.z - width) * 0.5f), new Vector3(size.x, size.y + 0.05f, width), 3f, 0.035f, 31);
                AddRoundedBox(data, new Vector3(sign * (size.x - width) * 0.5f, 0.025f, 0f), new Vector3(width, size.y + 0.05f, Mathf.Max(0.01f, size.z - width * 2f)), 3f, 0.035f, 37);
            }
            return Store(key, data);
        }

        // Iron fence panel: evenly spaced bars between three rails. The thin axis is X, length runs along Z.
        static Mesh LatticeMesh(Vector3 size, float tile)
        {
            var key = $"lattice_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}_{Q(tile)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var thin = Mathf.Min(size.x, 0.12f);
            var barCount = Mathf.Max(2, Mathf.RoundToInt(size.z / 0.26f));
            for (var i = 0; i < barCount; i++)
            {
                var z = -size.z * 0.5f + 0.06f + (size.z - 0.12f) * i / (barCount - 1);
                AddBox(data, new Vector3(0f, 0f, z), new Vector3(thin * 0.6f, size.y, 0.06f), tile);
            }
            foreach (var fraction in new[] { -0.46f, 0f, 0.46f })
                AddBox(data, new Vector3(0f, size.y * fraction, 0f), new Vector3(thin, 0.12f, size.z), tile);
            return Store(key, data);
        }

        // Surface of revolution. profile is (radius, height) from bottom to top; smooth normals come from the profile slope.
        static Mesh LatheMesh(string key, Vector2[] profile, int segments, bool hard, float uRepeat, bool topCap, bool bottomCap)
        {
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var height = 0f;
            foreach (var p in profile) height = Mathf.Max(height, p.y);

            Vector3 Ring(float radius, float y, float angle) => new Vector3(radius * Mathf.Cos(angle), y, radius * Mathf.Sin(angle));
            Vector2 SlopeNormal(int k)
            {
                var tangent = profile[Mathf.Min(k + 1, profile.Length - 1)] - profile[Mathf.Max(k - 1, 0)];
                return new Vector2(tangent.y, -tangent.x).normalized;
            }

            for (var k = 0; k < profile.Length - 1; k++)
            {
                var p0 = profile[k];
                var p1 = profile[k + 1];
                var n0 = SlopeNormal(k);
                var n1 = SlopeNormal(k + 1);
                for (var i = 0; i < segments; i++)
                {
                    var a0 = Mathf.PI * 2f * i / segments;
                    var a1 = Mathf.PI * 2f * (i + 1) / segments;
                    var b0 = Ring(p0.x, p0.y, a0);
                    var b1 = Ring(p0.x, p0.y, a1);
                    var t0 = Ring(p1.x, p1.y, a0);
                    var t1 = Ring(p1.x, p1.y, a1);
                    Vector3 nb0, nb1, nt0, nt1;
                    if (hard)
                    {
                        var face = Vector3.Cross(t0 - b0, b1 - b0).normalized;
                        nb0 = nb1 = nt0 = nt1 = face;
                    }
                    else
                    {
                        nb0 = new Vector3(n0.x * Mathf.Cos(a0), n0.y, n0.x * Mathf.Sin(a0));
                        nb1 = new Vector3(n0.x * Mathf.Cos(a1), n0.y, n0.x * Mathf.Sin(a1));
                        nt0 = new Vector3(n1.x * Mathf.Cos(a0), n1.y, n1.x * Mathf.Sin(a0));
                        nt1 = new Vector3(n1.x * Mathf.Cos(a1), n1.y, n1.x * Mathf.Sin(a1));
                    }
                    var u0 = (float)i / segments * uRepeat;
                    var u1 = (float)(i + 1) / segments * uRepeat;
                    var v0 = p0.y / height;
                    var v1 = p1.y / height;
                    data.Quad(b0, b1, t1, t0, nb0, nb1, nt1, nt0,
                        new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1), new Vector2(u0, v1));
                }
            }

            if (topCap) AddDisc(data, profile[profile.Length - 1], segments, true);
            if (bottomCap) AddDisc(data, profile[0], segments, false);
            return Store(key, data);
        }

        // Flat cap sampled from the middle of the texture so the barrel top shows staves, not the hoops.
        static void AddDisc(MeshData data, Vector2 edge, int segments, bool up)
        {
            var normal = up ? Vector3.up : Vector3.down;
            var center = new Vector3(0f, edge.y, 0f);
            var k = edge.x > 0.001f ? 0.25f / edge.x : 0f;
            for (var i = 0; i < segments; i++)
            {
                var a0 = Mathf.PI * 2f * i / segments;
                var a1 = Mathf.PI * 2f * (i + 1) / segments;
                var p0 = new Vector3(edge.x * Mathf.Cos(a0), edge.y, edge.x * Mathf.Sin(a0));
                var p1 = new Vector3(edge.x * Mathf.Cos(a1), edge.y, edge.x * Mathf.Sin(a1));
                var uvC = new Vector2(0.5f, 0.5f);
                var uv0 = uvC + new Vector2(p0.x, p0.z) * k;
                var uv1 = uvC + new Vector2(p1.x, p1.z) * k;
                if (up) data.Tri(center, p1, p0, normal, uvC, uv1, uv0);
                else data.Tri(center, p0, p1, normal, uvC, uv0, uv1);
            }
        }

        static Mesh DiscMesh(string key, float radius, int segments)
        {
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            AddDisc(data, new Vector2(radius, 0f), segments, true);
            return Store(key, data);
        }

        static Mesh BarrelMesh()
        {
            var profile = new[]
            {
                new Vector2(0.28f, 0f), new Vector2(0.33f, 0.12f), new Vector2(0.36f, 0.30f), new Vector2(0.37f, 0.475f),
                new Vector2(0.36f, 0.65f), new Vector2(0.33f, 0.83f), new Vector2(0.28f, 0.95f)
            };
            return LatheMesh("barrel", profile, 28, false, 1f, true, false);
        }

        static Mesh CrateMesh(Vector3 size, bool metal)
        {
            var key = $"crate_{(metal ? "iron" : "wood")}_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var half = size * 0.5f;
            var batten = Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.11f;
            if (metal)
            {
                foreach (var x in new[] { -1f, 1f })
                    foreach (var z in new[] { -1f, 1f })
                        foreach (var y in new[] { -1f, 1f })
                        {
                            var center = new Vector3(x * (half.x - batten * 0.5f), y * (half.y - batten * 0.72f), z * (half.z - batten * 0.5f));
                            AddBox(data, center + Vector3.forward * z * batten * 0.52f, new Vector3(batten * 1.9f, batten * 1.35f, 0.026f), 0.7f);
                            AddBox(data, center + Vector3.right * x * batten * 0.52f, new Vector3(0.026f, batten * 1.35f, batten * 1.9f), 0.7f);
                        }
            }
            else
            {
                AddRoundedBox(data, Vector3.zero, size * 0.81f, 1.1f, batten * 0.18f);
                for (var i = 0; i < 5; i++)
                {
                    var x = (i - 2) * (size.x - batten * 2f) / 5f;
                    var z = (i - 2) * (size.z - batten * 2f) / 5f;
                    foreach (var sign in new[] { -1f, 1f })
                    {
                        AddBox(data, new Vector3(x, 0f, sign * (half.z - batten * 0.45f)),
                            new Vector3((size.x - batten * 2f) / 5f - 0.012f, size.y - batten * 1.7f, batten * 0.32f), 1.1f);
                        AddBox(data, new Vector3(sign * (half.x - batten * 0.45f), 0f, z),
                            new Vector3(batten * 0.32f, size.y - batten * 1.7f, (size.z - batten * 2f) / 5f - 0.012f), 1.1f);
                    }
                    AddBox(data, new Vector3(x, half.y - batten * 0.3f, 0f),
                        new Vector3((size.x - batten * 2f) / 5f - 0.012f, batten * 0.3f, size.z - batten * 1.7f), 1.1f);
                }
                foreach (var x in new[] { -1f, 1f })
                    foreach (var z in new[] { -1f, 1f })
                        AddRoundedBox(data, new Vector3(x * (half.x - batten * 0.5f), 0f, z * (half.z - batten * 0.5f)),
                            new Vector3(batten, size.y, batten), 1.1f, batten * 0.1f);
                foreach (var y in new[] { -1f, 1f })
                {
                    foreach (var z in new[] { -1f, 1f })
                        AddBox(data, new Vector3(0f, y * (half.y - batten * 0.5f), z * (half.z - batten * 0.5f)), new Vector3(size.x, batten, batten), 1.1f);
                    foreach (var x in new[] { -1f, 1f })
                        AddBox(data, new Vector3(x * (half.x - batten * 0.5f), y * (half.y - batten * 0.5f), 0f), new Vector3(batten, batten, size.z), 1.1f);
                }
            }
            return Store(key, data);
        }

        static Mesh BarrelHoopMesh(float radius)
        {
            var profile = new[]
            {
                new Vector2(radius - 0.008f, 0f), new Vector2(radius, 0.012f), new Vector2(radius, 0.058f),
                new Vector2(radius - 0.008f, 0.070f), new Vector2(radius - 0.022f, 0.070f),
                new Vector2(radius - 0.022f, 0f), new Vector2(radius - 0.008f, 0f)
            };
            return LatheMesh("barrel_hoop_" + Mathf.RoundToInt(radius * 1000f), profile, 28, false, 3f, false, false);
        }

        static Mesh BarrelHoopsMesh()
        {
            const string key = "barrel_hoops";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            foreach (var y in new[] { 0.095f, 0.44f, 0.79f })
                data.Append(BarrelHoopMesh(y == 0.44f ? 0.382f : 0.342f), new Vector3(0f, y, 0f));
            return Store(key, data);
        }

        static Mesh BrazierMesh()
        {
            var profile = new[]
            {
                new Vector2(0.32f, 0f), new Vector2(0.32f, 0.05f), new Vector2(0.10f, 0.12f), new Vector2(0.07f, 0.50f),
                new Vector2(0.20f, 0.58f), new Vector2(0.40f, 0.82f), new Vector2(0.36f, 0.84f), new Vector2(0.001f, 0.64f)
            };
            return LatheMesh("brazier", profile, 24, false, 2f, false, false);
        }

        static Mesh MushroomMesh()
        {
            var profile = new[]
            {
                new Vector2(0.03f, 0f), new Vector2(0.03f, 0.12f), new Vector2(0.12f, 0.13f), new Vector2(0.14f, 0.17f),
                new Vector2(0.10f, 0.22f), new Vector2(0.03f, 0.25f), new Vector2(0.001f, 0.26f)
            };
            return LatheMesh("mushroom", profile, 8, false, 1f, false, false);
        }

        static Mesh CrystalMesh()
        {
            var profile = new[] { new Vector2(0.18f, 0f), new Vector2(0.24f, 0.18f), new Vector2(0.20f, 0.70f), new Vector2(0.001f, 1f) };
            return LatheMesh("crystal", profile, 6, true, 1f, false, false);
        }

        static Mesh WellMesh()
        {
            var profile = new[]
            {
                new Vector2(1.75f, 0f), new Vector2(1.75f, 0.85f), new Vector2(1.55f, 0.92f), new Vector2(1.40f, 0.85f), new Vector2(1.40f, 0.45f)
            };
            return LatheMesh("well", profile, 20, false, 6f, false, false);
        }

        // Hooded statue: bell-shaped robe with a rounded head, enough to read as a figure from the camera.
        static Mesh StatueMesh()
        {
            var profile = new[]
            {
                new Vector2(0.45f, 0f), new Vector2(0.45f, 0.30f), new Vector2(0.30f, 0.34f), new Vector2(0.26f, 1.0f),
                new Vector2(0.20f, 1.55f), new Vector2(0.16f, 1.75f), new Vector2(0.20f, 1.9f), new Vector2(0.14f, 2.1f), new Vector2(0.001f, 2.2f)
            };
            return LatheMesh("statue", profile, 10, false, 2f, false, false);
        }

        // Low rock: subdivided icosahedron, noise-displaced and flat shaded, box-projected UVs.
        static Mesh RockMesh(int seed)
        {
            var key = "rock_" + seed;
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            var corners = new[]
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            for (var i = 0; i < corners.Length; i++) corners[i].Normalize();
            var faces = new[]
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            };

            Vector3 Displace(Vector3 unit)
            {
                var noise = Mathf.PerlinNoise(unit.x * 1.9f + seed * 7.3f, unit.y * 1.7f + unit.z * 1.3f + seed);
                var p = unit * (0.34f + noise * 0.22f);
                p.y = Mathf.Max(p.y * 0.8f, -0.12f);
                return p;
            }

            var data = new MeshData();
            for (var f = 0; f < faces.Length; f += 3)
            {
                var a = corners[faces[f]];
                var b = corners[faces[f + 1]];
                var c = corners[faces[f + 2]];
                var ab = ((a + b) * 0.5f).normalized;
                var bc = ((b + c) * 0.5f).normalized;
                var ca = ((c + a) * 0.5f).normalized;
                AddRockTriangle(data, Displace(a), Displace(ab), Displace(ca));
                AddRockTriangle(data, Displace(ab), Displace(b), Displace(bc));
                AddRockTriangle(data, Displace(ca), Displace(bc), Displace(c));
                AddRockTriangle(data, Displace(ab), Displace(bc), Displace(ca));
            }
            return Store(key, data);
        }

        static void AddRockTriangle(MeshData data, Vector3 a, Vector3 b, Vector3 c)
        {
            var normal = Vector3.Cross(b - a, c - a);
            // Clockwise-from-outside winding has the cross product pointing outwards.
            if (Vector3.Dot(normal, (a + b + c) / 3f) < 0f)
            {
                var swap = b; b = c; c = swap;
                normal = -normal;
            }
            normal.Normalize();
            var ax = Mathf.Abs(normal.x);
            var ay = Mathf.Abs(normal.y);
            var az = Mathf.Abs(normal.z);
            Vector2 Project(Vector3 p) => ax >= ay && ax >= az ? new Vector2(p.z, p.y) : ay >= az ? new Vector2(p.x, p.z) : new Vector2(p.x, p.y);
            data.Tri(a, b, c, normal, Project(a) * 1.4f, Project(b) * 1.4f, Project(c) * 1.4f);
        }

        static void AddLeafCard(MeshData data, Vector3 center, Quaternion rotation, float width, float height, Vector3 outward)
        {
            Vector3 Sample(float u, float v) => center + rotation * new Vector3((u - 0.5f) * width, (v - 0.5f) * height,
                Mathf.Sin(u * Mathf.PI) * Mathf.Sin(v * Mathf.PI) * width * 0.12f);
            Vector3 Normal(float u, float v) => (outward + rotation * new Vector3((u - 0.5f) * 0.5f, (v - 0.5f) * 0.35f, 0.7f)).normalized;
            const int divisions = 2;
            for (var y = 0; y < divisions; y++)
                for (var x = 0; x < divisions; x++)
                {
                    var u0 = x / (float)divisions; var u1 = (x + 1) / (float)divisions;
                    var v0 = y / (float)divisions; var v1 = (y + 1) / (float)divisions;
                    data.Quad(Sample(u0, v0), Sample(u0, v1), Sample(u1, v1), Sample(u1, v0),
                        Normal(u0, v0), Normal(u0, v1), Normal(u1, v1), Normal(u1, v0),
                        new Vector2(u0, v0), new Vector2(u0, v1), new Vector2(u1, v1), new Vector2(u1, v0));
                }
        }

        // All leaf cards of a crown share one mesh and cutout material.
        static Mesh ConiferMesh(int variation)
        {
            var key = variation == 0 ? "conifer" : "conifer_" + variation;
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            for (var cluster = 0; cluster < 9; cluster++)
            {
                var angle = cluster * 2.4f + variation * 1.7f;
                var radius = cluster == 8 ? 0f : 0.19f + Hash01(cluster, variation + 82) * 0.1f;
                var origin = new Vector3(Mathf.Cos(angle) * radius, 0.48f + Hash01(cluster, variation + 84) * 0.36f, Mathf.Sin(angle) * radius);
                for (var leaf = 0; leaf < 22; leaf++)
                {
                    var seed = cluster * 31 + leaf + variation * 307;
                    var phi = Hash01(seed, 85) * Mathf.PI * 2f;
                    var y = Hash01(seed, 86) * 2f - 1f;
                    var outward = new Vector3(Mathf.Cos(phi) * Mathf.Sqrt(1f - y * y), y, Mathf.Sin(phi) * Mathf.Sqrt(1f - y * y));
                    var center = origin + Vector3.Scale(outward, new Vector3(0.17f, 0.13f, 0.18f)) * (0.45f + Hash01(seed, 87) * 0.55f);
                    var normal = (outward + Vector3.up * 0.55f).normalized;
                    var rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(0f, 0f, Hash01(seed, 88) * 360f);
                    var size = 0.105f + Hash01(seed, 89) * 0.085f;
                    AddLeafCard(data, center, rotation, size, size * 1.15f, normal);
                }
            }
            return Store(key, data);
        }

        static Mesh BushMesh(int variation)
        {
            var key = "bush_" + variation;
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            for (var i = 0; i < 24; i++)
            {
                var angle = Hash01(i, variation + 101) * Mathf.PI * 2f;
                var outward = new Vector3(Mathf.Cos(angle), 0.45f + Hash01(i, 103), Mathf.Sin(angle)).normalized;
                var center = Vector3.Scale(outward, new Vector3(0.34f, 0.28f, 0.34f));
                var rotation = Quaternion.LookRotation(outward) * Quaternion.Euler(0f, 0f, Hash01(i, 104) * 360f);
                AddLeafCard(data, center, rotation, 0.3f + Hash01(i, 105) * 0.18f, 0.45f, outward);
            }
            return Store(key, data);
        }

        static Mesh GrassTuftMesh(float spread)
        {
            var key = "grass_patch_" + Q(spread);
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            for (var i = 0; i < 42; i++)
            {
                var turn = Quaternion.Euler(0f, Hash01(i, 114) * 360f, 0f);
                var height = 0.19f + Hash01(i, 115) * 0.23f;
                var origin = new Vector3((i / 14 - 1) * spread + (Hash01(i, 116) - 0.5f) * 0.16f, 0f, (Hash01(i, 117) - 0.5f) * 0.16f);
                Vector3 Point(float t, float side) => origin + turn * new Vector3(side * 0.028f * (1f - t * 0.95f), t * height, t * t * height * 0.38f);
                var normal = turn * new Vector3(0f, 0.5f, -0.86f);
                for (var j = 0; j < 3; j++)
                {
                    var a = j / 3f; var b = (j + 1) / 3f;
                    data.Quad(Point(a, -1f), Point(a, 1f), Point(b, 1f), Point(b, -1f), normal, normal, normal, normal,
                        new Vector2(0f, a), new Vector2(1f, a), new Vector2(1f, b), new Vector2(0f, b));
                    data.Quad(Point(a, 1f), Point(a, -1f), Point(b, -1f), Point(b, 1f), -normal, -normal, -normal, -normal,
                        new Vector2(1f, a), new Vector2(0f, a), new Vector2(0f, b), new Vector2(1f, b));
                }
            }
            return Store(key, data);
        }

        static Mesh TreeTrunkMesh()
        {
            var profile = new[] { new Vector2(0.09f, 0f), new Vector2(0.07f, 0.10f), new Vector2(0.045f, 0.39f), new Vector2(0.027f, 0.62f) };
            return LatheMesh("tree_trunk", profile, 12, false, 1f, true, false);
        }

        static Mesh IvyMesh()
        {
            const string key = "ivy";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            for (var i = 0; i < 15; i++)
            {
                var center = new Vector3(Mathf.Sin(i * 1.9f) * 0.16f, -0.08f - i * 0.09f, 0.035f);
                var rotation = Quaternion.Euler(-8f, (Hash01(i, 62) - 0.5f) * 24f, (i % 2 == 0 ? 1f : -1f) * 25f);
                AddLeafCard(data, center, rotation, 0.28f, 0.3f, Vector3.forward);
            }
            return Store(key, data);
        }

        static Mesh BannerMesh()
        {
            const string key = "banner";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            const int columns = 12;
            const int rows = 12;
            Vector3 Sample(float u, float v) => new Vector3(u - 0.5f, -v * (1f - Mathf.Abs(u - 0.5f) * 0.22f),
                Mathf.Sin(u * Mathf.PI * 4f + v * 1.7f) * 0.045f + Mathf.Sin(v * 5f) * 0.03f);
            Vector3 Normal(float u, float v)
            {
                const float step = 0.001f;
                var tangentU = (Sample(u + step, v) - Sample(u - step, v)) / (2f * step);
                var tangentV = (Sample(u, v + step) - Sample(u, v - step)) / (2f * step);
                return Vector3.Cross(tangentV, tangentU).normalized;
            }
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < columns; x++)
                {
                    var u0 = x / (float)columns;
                    var u1 = (x + 1) / (float)columns;
                    var v0 = y / (float)rows;
                    var v1 = (y + 1) / (float)rows;
                    var a = Sample(u0, v1); var b = Sample(u1, v1); var c = Sample(u1, v0); var d = Sample(u0, v0);
                    var na = Normal(u0, v1); var nb = Normal(u1, v1); var nc = Normal(u1, v0); var nd = Normal(u0, v0);
                    data.Quad(a, b, c, d, -na, -nb, -nc, -nd, new Vector2(u0, v1), new Vector2(u1, v1), new Vector2(u1, v0), new Vector2(u0, v0));
                    data.Quad(b, a, d, c, nb, na, nd, nc, new Vector2(u1, v1), new Vector2(u0, v1), new Vector2(u0, v0), new Vector2(u1, v0));
                }
            }
            return Store(key, data);
        }
        static Mesh FlatQuadMesh(string key)
        {
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            const float h = 0.5f;
            // Faces up. Seen from above, +X is right and +Z is up on screen.
            data.Quad(new Vector3(-h, 0f, -h), new Vector3(h, 0f, -h), new Vector3(h, 0f, h), new Vector3(-h, 0f, h),
                Vector3.up, Vector3.up, Vector3.up, Vector3.up,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
            return Store(key, data);
        }

        static Mesh OuterGroundMesh(Arena.ArenaMap map)
        {
            const string key = "outer_ground";
            var data = new MeshData();
            var source = map.Layout.terrain;
            var min = new Vector2(source.origin[0], source.origin[1]) / map.unitsPerMeter;
            var step = source.cellSize / map.unitsPerMeter;
            var max = min + new Vector2(source.width - 1, source.height - 1) * step;
            var xs = new List<float>(); var zs = new List<float>();
            foreach (var margin in new[] { 128f, 64f, 32f, 16f, 4f }) { xs.Add(min.x - margin); zs.Add(min.y - margin); }
            for (var i = 0; i < source.width; i++) xs.Add(min.x + i * step);
            for (var i = 0; i < source.height; i++) zs.Add(min.y + i * step);
            foreach (var margin in new[] { 4f, 16f, 32f, 64f, 128f }) { xs.Add(max.x + margin); zs.Add(max.y + margin); }
            Vector3 Point(float x, float z)
            {
                var p = new Vector3(x, 0f, z);
                p.y = map.SampleHeight(p) - 0.025f;
                return p;
            }
            Vector2 Uv(Vector3 p) => new Vector2((p.x - min.x) / 8f, (p.z - min.y) / 8f);
            for (var z = 0; z < zs.Count - 1; z++)
                for (var x = 0; x < xs.Count - 1; x++)
                {
                    if (xs[x] >= min.x && xs[x + 1] <= max.x && zs[z] >= min.y && zs[z + 1] <= max.y) continue;
                    var a = Point(xs[x], zs[z]); var b = Point(xs[x + 1], zs[z]);
                    var c = Point(xs[x + 1], zs[z + 1]); var d = Point(xs[x], zs[z + 1]);
                    var normal = Vector3.Cross(d - a, b - a).normalized;
                    data.Quad(a, b, c, d, normal, normal, normal, normal, Uv(a), Uv(b), Uv(c), Uv(d));
                }
            return Store(key, data);
        }
    }
}

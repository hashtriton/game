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
                var i = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                normals.Add(n); normals.Add(n); normals.Add(n);
                uvs.Add(ua); uvs.Add(ub); uvs.Add(uc);
                triangles.AddRange(new[] { i, i + 1, i + 2 });
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
            AssetDatabase.CreateAsset(mesh, MeshRoot + key + ".asset");
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
            AddBox(data, Vector3.zero, size, tile);
            return Store(key, data);
        }

        static Mesh PillarMesh(Vector3 size, float tile)
        {
            var key = $"pillar_{Q(size.x)}_{Q(size.y)}_{Q(size.z)}_{Q(tile)}";
            if (meshes.TryGetValue(key, out var cached)) return cached;
            var data = new MeshData();
            var foot = Mathf.Min(0.45f, size.y * 0.12f);
            var cap = foot * 0.9f;
            AddBox(data, new Vector3(0f, -size.y * 0.5f + foot * 0.5f, 0f), new Vector3(size.x, foot, size.z), tile);
            AddBox(data, new Vector3(0f, (foot - cap) * 0.5f, 0f), new Vector3(size.x * 0.78f, size.y - foot - cap, size.z * 0.78f), tile);
            AddBox(data, new Vector3(0f, size.y * 0.5f - cap * 0.5f, 0f), new Vector3(size.x * 0.96f, cap, size.z * 0.96f), tile);
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
            return LatheMesh("barrel", profile, 16, false, 1f, true, false);
        }

        static Mesh BrazierMesh()
        {
            var profile = new[]
            {
                new Vector2(0.32f, 0f), new Vector2(0.32f, 0.05f), new Vector2(0.10f, 0.12f), new Vector2(0.07f, 0.50f),
                new Vector2(0.20f, 0.58f), new Vector2(0.40f, 0.82f), new Vector2(0.36f, 0.84f), new Vector2(0.001f, 0.64f)
            };
            return LatheMesh("brazier", profile, 12, false, 2f, false, false);
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

        // Faceted spruce: trunk plus four overlapping skirts, 1 unit tall and 1 unit wide at the lowest tier.
        static Mesh ConiferMesh()
        {
            var profile = new[]
            {
                new Vector2(0.05f, 0f), new Vector2(0.05f, 0.17f), new Vector2(0.50f, 0.20f), new Vector2(0.12f, 0.50f),
                new Vector2(0.40f, 0.42f), new Vector2(0.10f, 0.70f), new Vector2(0.30f, 0.60f), new Vector2(0.07f, 0.86f),
                new Vector2(0.20f, 0.78f), new Vector2(0.001f, 1f)
            };
            return LatheMesh("conifer", profile, 10, true, 3f, false, false);
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
    }
}

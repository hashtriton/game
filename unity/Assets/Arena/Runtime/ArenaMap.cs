using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arena
{
    [Serializable]
    public sealed class ArenaMapLayout
    {
        public int schemaVersion;
        public string version;
        public float wcUnitsPerUnityUnit;
        public MapProvenance provenance;
        public TerrainLayout terrain;
        public MapInfo mapInfo;
        public PathingLayout pathing;
        public MapDoodad[] doodads;
        public MapRegion[] regions;
        public MapUnit[] staticUnits;
        public MapDestructable[] scriptDestructables;
        public string[] limits;
    }

    [Serializable]
    public sealed class TerrainLayout
    {
        public int formatVersion;
        public string tileset;
        public int customTileset;
        public int width;
        public int height;
        public float cellSize;
        public float[] origin;
        public string[] groundTextures;
        public string[] cliffTextures;
        public TerrainVertex[] vertices;
        public string heightFormula;
        public int consumedBytes;
    }

    [Serializable]
    public sealed class TerrainVertex
    {
        public int offset;
        public float x;
        public float y;
        public float height;
        public int groundHeightRaw;
        public int waterRaw;
        public float waterHeight;
        public int flags;
        public int groundTextureIndex;
        public int groundVariation;
        public int cliffVariation;
        public int cliffTextureIndex;
        public int layerHeight;
        public bool mapEdge;
    }

    [Serializable]
    public sealed class PathingLayout
    {
        public int formatVersion;
        public int width;
        public int height;
        public float cellSize;
        public float[] origin;
        public int[] flags;
        public int blockedMask;
        public int dataOffset;
        public string scope;
    }

    [Serializable]
    public sealed class MapDoodad
    {
        public int offset;
        public string id;
        public string name;
        public string category;
        public string categoryEvidence;
        public string modelPathReference;
        public string pathingTexture;
        public int variation;
        public float x;
        public float y;
        public float z;
        public float rotationRadians;
        public float[] scale;
        public int flags;
        public int life;
        public int editorId;
        public MapDefinitionSource[] definitionSources;
        public bool modelBoundsAvailable;
        public float[] modelBoundsMin;
        public float[] modelBoundsMax;
        public float modelBoundsRadius;
        public string modelBoundsSource;
        public int modelBoundsOffset;
        public string modelBoundsSourceSha256;
        public string modelBoundsEvidence;
        public bool geometryBoundsAvailable;
        public float[] geometryBoundsMin;
        public float[] geometryBoundsMax;
        public int geometryVertexCount;
        public int[] geometrySourceOffsets;
        public string geometryBoundsEvidence;
    }

    [Serializable]
    public sealed class MapDefinitionSource
    {
        public string field;
        public string source;
        public int offset;
    }

    [Serializable]
    public sealed class MapRegion
    {
        public string id;
        public string name;
        public float[] bounds;
        public int sourceLine;
    }

    [Serializable]
    public sealed class MapUnit
    {
        public string id;
        public string name;
        public string @function;
        public string ownerExpression;
        public float x;
        public float y;
        public float facing;
        public int sourceLine;
        public int[] coordinateSourceLines;
        public string scope;
    }

    [Serializable]
    public sealed class MapDestructable
    {
        public string id;
        public float x;
        public float y;
        public float facingDegrees;
        public float scale;
        public int variation;
        public string @function;
        public int sourceLine;
    }

    [Serializable]
    public sealed class MapInfo
    {
        public int formatVersion;
        public int editorVersion;
        public int saveCount;
        public string name;
        public string author;
        public int cameraBoundsOffset;
        public float[] cameraBounds;
        public int[] margins;
        public int[] playableTiles;
        public int flags;
        public string tileset;
        public int parsedPrefixBytes;
        public int totalBytes;
    }

    [Serializable]
    public sealed class MapProvenance
    {
        public string mapName;
        public string mapSha256;
        public MapSource[] sources;
        public string evidence;
        public string[] formatReferences;
    }

    [Serializable]
    public sealed class MapSource
    {
        public string path;
        public int bytes;
        public string sha256;
    }

    [DisallowMultipleComponent]
    public sealed class ArenaMap : MonoBehaviour
    {
        public TextAsset layoutJson;
        public float unitsPerMeter = 64f;

        private ArenaMapLayout layout;
        private TextAsset loadedAsset;
        private float loadedScale;
        private Bounds worldBounds;
        private Bounds arenaBounds;
        private bool[] blocked;
        private bool[] clearance;
        private float clearanceRadius = -1f;
        private int width;
        private int height;
        private float cell;
        private float originX;
        private float originZ;
        private byte[] searchState;
        private int[] cost;
        private int[] priority;
        private int[] parent;
        private int[] heap;
        private int[] heapPosition;
        private int heapCount;
        private Vector3[] spawnPoints;
        private Vector3 heroSpawn;

        public ArenaMapLayout Layout { get { EnsureInitialized(); return layout; } }
        public Bounds WorldBounds { get { EnsureInitialized(); return worldBounds; } }
        public Bounds ArenaBounds { get { EnsureInitialized(); return arenaBounds; } }
        public Vector3 HeroSpawn { get { EnsureInitialized(); return heroSpawn; } }
        public Vector3[] SpawnPoints { get { EnsureInitialized(); return (Vector3[])spawnPoints.Clone(); } }

        public void Initialize()
        {
            if (layoutJson == null) throw new InvalidOperationException("ArenaMap requires a layout JSON asset.");
            if (!Finite(unitsPerMeter) || unitsPerMeter <= 0f)
                throw new InvalidOperationException("ArenaMap unitsPerMeter must be positive and finite.");

            ArenaMapLayout parsed = JsonUtility.FromJson<ArenaMapLayout>(layoutJson.text);
            Validate(parsed);
            layout = parsed;
            loadedAsset = layoutJson;
            loadedScale = unitsPerMeter;
            TerrainLayout terrain = layout.terrain;
            PathingLayout pathing = layout.pathing;
            width = pathing.width;
            height = pathing.height;
            cell = pathing.cellSize / unitsPerMeter;
            originX = pathing.origin[0] / unitsPerMeter;
            originZ = pathing.origin[1] / unitsPerMeter;
            int count = width * height;
            blocked = new bool[count];
            clearance = new bool[count];
            searchState = new byte[count];
            cost = new int[count];
            priority = new int[count];
            parent = new int[count];
            heap = new int[count];
            heapPosition = new int[count];
            clearanceRadius = -1f;
            int mask = pathing.blockedMask == 0 ? 2 : pathing.blockedMask;
            for (int i = 0; i < count; i++) blocked[i] = (pathing.flags[i] & mask) != 0;
            AddApproximateObjectFootprints();

            float minHeight = float.PositiveInfinity;
            float maxHeight = float.NegativeInfinity;
            for (int i = 0; i < terrain.vertices.Length; i++)
            {
                float value = terrain.vertices[i].height / unitsPerMeter;
                minHeight = Mathf.Min(minHeight, value);
                maxHeight = Mathf.Max(maxHeight, value);
            }
            worldBounds = new Bounds();
            worldBounds.SetMinMax(new Vector3(terrain.origin[0] / unitsPerMeter, minHeight, terrain.origin[1] / unitsPerMeter),
                new Vector3((terrain.origin[0] + (terrain.width - 1) * terrain.cellSize) / unitsPerMeter, maxHeight,
                    (terrain.origin[1] + (terrain.height - 1) * terrain.cellSize) / unitsPerMeter));
            float[] arena = { -2560f, -1536f, 2304f, 3200f };
            if (layout.regions != null)
            {
                foreach (MapRegion region in layout.regions)
                    if (region != null && region.id == "vV" && region.bounds != null && region.bounds.Length == 4)
                    { arena = region.bounds; break; }
            }
            arenaBounds = new Bounds();
            arenaBounds.SetMinMax(new Vector3(arena[0] / unitsPerMeter, minHeight, arena[1] / unitsPerMeter),
                new Vector3(arena[2] / unitsPerMeter, maxHeight, arena[3] / unitsPerMeter));
            heroSpawn = FindNearestWalkable(WcToWorld(135f, 1000f));
            spawnPoints = new[]
            {
                FindNearestWalkable(WcToWorld(64f, 2624f)),
                FindNearestWalkable(WcToWorld(-1984f, 574f)),
                FindNearestWalkable(WcToWorld(1734f, 1224f))
            };
        }

        public Vector3 WcToWorld(float x, float y)
        {
            EnsureInitialized();
            return Ground(new Vector3(x / unitsPerMeter, 0f, y / unitsPerMeter));
        }

        public Vector3 WcToWorld(float x, float y, float terrainHeight)
        {
            EnsureInitialized();
            return new Vector3(x / unitsPerMeter, terrainHeight / unitsPerMeter, y / unitsPerMeter);
        }

        public float SampleHeight(Vector3 point)
        {
            EnsureInitialized();
            if (!Finite(point.x) || !Finite(point.z)) throw new ArgumentException("Map position must be finite.");
            TerrainLayout t = layout.terrain;
            float gx = Mathf.Clamp((point.x * unitsPerMeter - t.origin[0]) / t.cellSize, 0f, t.width - 1f);
            float gy = Mathf.Clamp((point.z * unitsPerMeter - t.origin[1]) / t.cellSize, 0f, t.height - 1f);
            int x = Mathf.Min(Mathf.FloorToInt(gx), t.width - 2);
            int y = Mathf.Min(Mathf.FloorToInt(gy), t.height - 2);
            float tx = gx - x;
            float ty = gy - y;
            float bottom = Mathf.Lerp(t.vertices[y * t.width + x].height, t.vertices[y * t.width + x + 1].height, tx);
            float top = Mathf.Lerp(t.vertices[(y + 1) * t.width + x].height, t.vertices[(y + 1) * t.width + x + 1].height, tx);
            return Mathf.Lerp(bottom, top, ty) / unitsPerMeter;
        }

        public bool IsWalkable(Vector3 point)
        {
            EnsureInitialized();
            return CanOccupy(point, 0f);
        }

        public bool IsWalkable(Vector3 point, float radius)
        {
            EnsureInitialized();
            return Finite(radius) && radius >= 0f && CanOccupy(point, radius);
        }

        // An unsuccessful projection returns the grounded input; IsWalkable exposes that failure.
        public Vector3 FindNearestWalkable(Vector3 point, float maxDistance = 10f)
        {
            EnsureInitialized();
            if (!Finite(point.x) || !Finite(point.z)) throw new ArgumentException("Map position must be finite.");
            if (CanOccupy(point, 0f)) return Ground(point);
            if (TryNearestNode(point, 0f, maxDistance, false, out int index)) return CellPoint(index);
            return Ground(point);
        }

        public Vector3 Move(Vector3 from, Vector3 displacement, float radius)
        {
            EnsureInitialized();
            if (!Finite(from.x) || !Finite(from.z)) throw new ArgumentException("Map position must be finite.");
            if (!Finite(radius) || radius < 0f || !Finite(displacement.x) || !Finite(displacement.z)) return Ground(from);
            Vector3 position = Ground(from);
            if (!CanOccupy(position, radius))
            {
                if (!TryNearestNode(position, radius, 10f, false, out int index)) return position;
                position = CellPoint(index);
            }
            displacement.y = 0f;
            float distance = displacement.magnitude;
            if (distance <= .00001f) return position;
            // Work stays bounded even if a caller supplies an unusually large displacement.
            float maximumTravel = Mathf.Sqrt(width * width + height * height) * cell;
            if (distance > maximumTravel) { displacement *= maximumTravel / distance; distance = maximumTravel; }
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / (cell * .25f)));
            Vector3 step = displacement / steps;
            for (int i = 0; i < steps; i++)
            {
                Vector3 next = position + step;
                if (SegmentClear(position, next, radius)) position = next;
                else
                {
                    // Resolve the larger axis first to keep wall sliding deterministic.
                    Vector3 first = Mathf.Abs(step.x) >= Mathf.Abs(step.z)
                        ? new Vector3(step.x, 0f, 0f) : new Vector3(0f, 0f, step.z);
                    Vector3 second = step - first;
                    if (SegmentClear(position, position + first, radius)) position += first;
                    if (SegmentClear(position, position + second, radius)) position += second;
                }
            }
            return Ground(position);
        }

        // Empty means no route, including endpoints that cannot be projected within ten metres.
        public List<Vector3> FindPath(Vector3 from, Vector3 to, float radius = .45f)
        {
            EnsureInitialized();
            if (!Finite(radius) || radius < 0f || !Finite(from.x) || !Finite(from.z) || !Finite(to.x) || !Finite(to.z))
                return new List<Vector3>();
            bool validStart = CanOccupy(from, radius);
            bool validEnd = CanOccupy(to, radius);
            if (!TryNearestNode(from, radius, 10f, validStart, out int start) ||
                !TryNearestNode(to, radius, 10f, validEnd, out int goal)) return new List<Vector3>();
            Vector3 first = validStart ? Ground(from) : CellPoint(start);
            Vector3 last = validEnd ? Ground(to) : CellPoint(goal);
            EnsureClearance(radius);
            Array.Clear(searchState, 0, searchState.Length);
            heapCount = 0;
            cost[start] = 0;
            parent[start] = -1;
            priority[start] = Heuristic(start, goal);
            searchState[start] = 1;
            Push(start);
            while (heapCount > 0)
            {
                int current = Pop();
                if (current == goal) return BuildPath(start, goal, first, last);
                searchState[current] = 2;
                int cx = current % width;
                int cy = current / width;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx;
                        int ny = cy + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                        int next = ny * width + nx;
                        if (!clearance[next] || searchState[next] == 2) continue;
                        bool diagonal = dx != 0 && dy != 0;
                        if (diagonal && (!clearance[cy * width + nx] || !clearance[ny * width + cx])) continue;
                        if (!SegmentClear(CellPoint(current), CellPoint(next), radius)) continue;
                        int candidate = cost[current] + (diagonal ? 14 : 10);
                        if (searchState[next] != 0 && candidate >= cost[next]) continue;
                        parent[next] = current;
                        cost[next] = candidate;
                        priority[next] = candidate + Heuristic(next, goal);
                        if (searchState[next] == 0) { searchState[next] = 1; Push(next); }
                        else SiftUp(heapPosition[next]);
                    }
                }
            }
            return new List<Vector3>();
        }

        private void EnsureInitialized()
        {
            if (layout == null || loadedAsset != layoutJson || loadedScale != unitsPerMeter) Initialize();
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void Validate(ArenaMapLayout data)
        {
            if (data == null || data.terrain == null || data.pathing == null)
                throw new InvalidOperationException("ArenaMap layout needs terrain and pathing.");
            TerrainLayout t = data.terrain;
            PathingLayout p = data.pathing;
            if (t.width < 2 || t.height < 2 || t.width > 1024 || t.height > 1024 ||
                t.vertices == null || t.vertices.Length != t.width * t.height ||
                t.origin == null || t.origin.Length < 2 || !Finite(t.origin[0]) || !Finite(t.origin[1]) ||
                !Finite(t.cellSize) || t.cellSize <= 0f)
                throw new InvalidOperationException("ArenaMap terrain grid is malformed.");
            if (p.width < 1 || p.height < 1 || p.width > 1024 || p.height > 1024 ||
                p.flags == null || p.flags.Length != p.width * p.height ||
                p.origin == null || p.origin.Length < 2 || !Finite(p.origin[0]) || !Finite(p.origin[1]) ||
                !Finite(p.cellSize) || p.cellSize <= 0f)
                throw new InvalidOperationException("ArenaMap pathing grid is malformed.");
            for (int i = 0; i < t.vertices.Length; i++)
                if (t.vertices[i] == null || !Finite(t.vertices[i].height))
                    throw new InvalidOperationException("ArenaMap terrain contains an invalid height.");
        }

        private Vector3 Ground(Vector3 point)
        {
            point.y = SampleHeight(point);
            return point;
        }

        private Vector3 CellPoint(int index)
        {
            return Ground(new Vector3(originX + (index % width + .5f) * cell, 0f,
                originZ + (index / width + .5f) * cell));
        }

        private bool CanOccupy(Vector3 point, float radius)
        {
            float x = point.x - originX;
            float z = point.z - originZ;
            if (!Finite(x) || !Finite(z) || x - radius < 0f || z - radius < 0f ||
                x + radius >= width * cell || z + radius >= height * cell) return false;
            if (radius <= 0f) return !blocked[Mathf.FloorToInt(z / cell) * width + Mathf.FloorToInt(x / cell)];
            int minX = Mathf.Max(0, Mathf.FloorToInt((x - radius) / cell));
            int maxX = Mathf.Min(width - 1, Mathf.FloorToInt((x + radius) / cell));
            int minY = Mathf.Max(0, Mathf.FloorToInt((z - radius) / cell));
            int maxY = Mathf.Min(height - 1, Mathf.FloorToInt((z + radius) / cell));
            float squaredRadius = radius * radius;
            for (int yy = minY; yy <= maxY; yy++)
            {
                for (int xx = minX; xx <= maxX; xx++)
                {
                    if (!blocked[yy * width + xx]) continue;
                    float dx = x - Mathf.Clamp(x, xx * cell, (xx + 1) * cell);
                    float dz = z - Mathf.Clamp(z, yy * cell, (yy + 1) * cell);
                    if (dx * dx + dz * dz < squaredRadius) return false;
                }
            }
            return true;
        }

        private bool TryNearestNode(Vector3 point, float radius, float maxDistance, bool connected, out int index)
        {
            index = -1;
            if (!Finite(maxDistance) || maxDistance < 0f) return false;
            float localX = point.x - originX;
            float localZ = point.z - originZ;
            int minX = Mathf.Clamp(Mathf.FloorToInt((localX - maxDistance) / cell), 0, width - 1);
            int maxX = Mathf.Clamp(Mathf.FloorToInt((localX + maxDistance) / cell), 0, width - 1);
            int minY = Mathf.Clamp(Mathf.FloorToInt((localZ - maxDistance) / cell), 0, height - 1);
            int maxY = Mathf.Clamp(Mathf.FloorToInt((localZ + maxDistance) / cell), 0, height - 1);
            float best = maxDistance * maxDistance;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int candidate = y * width + x;
                    if (blocked[candidate]) continue;
                    Vector3 position = new Vector3(originX + (x + .5f) * cell, 0f, originZ + (y + .5f) * cell);
                    float dx = position.x - point.x;
                    float dz = position.z - point.z;
                    float distance = dx * dx + dz * dz;
                    if (distance > best || !CanOccupy(position, radius)) continue;
                    if (connected && !SegmentClear(point, position, radius)) continue;
                    best = distance;
                    index = candidate;
                }
            }
            return index >= 0;
        }

        private bool SegmentClear(Vector3 a, Vector3 b, float radius)
        {
            if (!CanOccupy(a, radius) || !CanOccupy(b, radius)) return false;
            float ax = a.x - originX;
            float ay = a.z - originZ;
            float bx = b.x - originX;
            float by = b.z - originZ;
            int minX = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(ax, bx) - radius) / cell));
            int maxX = Mathf.Min(width - 1, Mathf.FloorToInt((Mathf.Max(ax, bx) + radius) / cell));
            int minY = Mathf.Max(0, Mathf.FloorToInt((Mathf.Min(ay, by) - radius) / cell));
            int maxY = Mathf.Min(height - 1, Mathf.FloorToInt((Mathf.Max(ay, by) + radius) / cell));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!blocked[y * width + x]) continue;
                    float left = x * cell;
                    float bottom = y * cell;
                    float right = left + cell;
                    float top = bottom + cell;
                    float enter = 0f;
                    float exit = 1f;
                    if (ClipAxis(ax, bx - ax, left, right, ref enter, ref exit) &&
                        ClipAxis(ay, by - ay, bottom, top, ref enter, ref exit)) return false;
                    if (radius <= 0f) continue;
                    float distance = Mathf.Min(PointRectangleDistance(ax, ay, left, bottom, right, top),
                        PointRectangleDistance(bx, by, left, bottom, right, top));
                    distance = Mathf.Min(distance, PointSegmentDistance(left, bottom, ax, ay, bx, by));
                    distance = Mathf.Min(distance, PointSegmentDistance(left, top, ax, ay, bx, by));
                    distance = Mathf.Min(distance, PointSegmentDistance(right, bottom, ax, ay, bx, by));
                    distance = Mathf.Min(distance, PointSegmentDistance(right, top, ax, ay, bx, by));
                    if (distance < radius * radius) return false;
                }
            }
            return true;
        }

        private static bool ClipAxis(float start, float delta, float min, float max, ref float enter, ref float exit)
        {
            if (Mathf.Abs(delta) < .0000001f) return start >= min && start <= max;
            float first = (min - start) / delta;
            float last = (max - start) / delta;
            if (first > last) { float temporary = first; first = last; last = temporary; }
            enter = Mathf.Max(enter, first);
            exit = Mathf.Min(exit, last);
            return enter <= exit;
        }

        private static float PointRectangleDistance(float x, float y, float left, float bottom, float right, float top)
        {
            float dx = x - Mathf.Clamp(x, left, right);
            float dy = y - Mathf.Clamp(y, bottom, top);
            return dx * dx + dy * dy;
        }

        private static float PointSegmentDistance(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float length = dx * dx + dy * dy;
            float t = length > .00000001f ? Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / length) : 0f;
            float ex = x - ax - t * dx;
            float ey = y - ay - t * dy;
            return ex * ex + ey * ey;
        }

        private void EnsureClearance(float radius)
        {
            if (clearanceRadius == radius) return;
            for (int i = 0; i < clearance.Length; i++)
                clearance[i] = !blocked[i] && CanOccupy(new Vector3(originX + (i % width + .5f) * cell, 0f,
                    originZ + (i / width + .5f) * cell), radius);
            clearanceRadius = radius;
        }

        private List<Vector3> BuildPath(int start, int goal, Vector3 first, Vector3 last)
        {
            var route = new List<int>();
            int node = goal;
            while (node >= 0)
            {
                route.Add(node);
                if (node == start) break;
                node = parent[node];
            }
            route.Reverse();
            var result = new List<Vector3> { first };
            AddDistinct(result, CellPoint(start));
            // Only remove collinear cell centres. Arbitrary shortcuts can cut blocked corners.
            for (int i = 1; i < route.Count - 1; i++)
            {
                int before = route[i - 1];
                int current = route[i];
                int after = route[i + 1];
                if (current % width - before % width != after % width - current % width ||
                    current / width - before / width != after / width - current / width)
                    AddDistinct(result, CellPoint(current));
            }
            AddDistinct(result, CellPoint(goal));
            AddDistinct(result, last);
            return result;
        }

        private static void AddDistinct(List<Vector3> list, Vector3 point)
        {
            if ((list[list.Count - 1] - point).sqrMagnitude > .000001f) list.Add(point);
        }

        private int Heuristic(int a, int b)
        {
            int x = Mathf.Abs(a % width - b % width);
            int y = Mathf.Abs(a / width - b / width);
            return 10 * Mathf.Max(x, y) + 4 * Mathf.Min(x, y);
        }

        private void Push(int node)
        {
            int position = heapCount++;
            heap[position] = node;
            heapPosition[node] = position;
            SiftUp(position);
        }

        private void SiftUp(int position)
        {
            while (position > 0)
            {
                int above = (position - 1) / 2;
                if (priority[heap[above]] <= priority[heap[position]]) break;
                Swap(position, above);
                position = above;
            }
        }

        private int Pop()
        {
            int result = heap[0];
            heapCount--;
            if (heapCount == 0) return result;
            heap[0] = heap[heapCount];
            heapPosition[heap[0]] = 0;
            int position = 0;
            while (true)
            {
                int left = position * 2 + 1;
                if (left >= heapCount) break;
                int right = left + 1;
                int best = right < heapCount && priority[heap[right]] < priority[heap[left]] ? right : left;
                if (priority[heap[position]] <= priority[heap[best]]) break;
                Swap(position, best);
                position = best;
            }
            return result;
        }

        private void Swap(int a, int b)
        {
            int value = heap[a];
            heap[a] = heap[b];
            heap[b] = value;
            heapPosition[heap[a]] = a;
            heapPosition[heap[b]] = b;
        }

        private void AddApproximateObjectFootprints()
        {
            if (layout.doodads == null) return;
            foreach (MapDoodad doodad in layout.doodads)
            {
                if (doodad == null || doodad.life == 0 || string.IsNullOrEmpty(doodad.pathingTexture)) continue;
                int size = FootprintSize(doodad.pathingTexture);
                if (size == 0) continue;
                // WPM excludes destructable footprints. The named texture dimensions supply
                // a conservative solid square, not decoded pixels or Warcraft runtime pathing.
                // Visual mesh scale/GEOS bounds do not define that footprint. Destruction and
                // script changes are intentionally outside this static navigation snapshot.
                float half = size * 32f * .5f / unitsPerMeter;
                float x = doodad.x / unitsPerMeter - originX;
                float z = doodad.y / unitsPerMeter - originZ;
                int minX = Mathf.Max(0, Mathf.FloorToInt((x - half) / cell));
                int maxX = Mathf.Min(width - 1, Mathf.CeilToInt((x + half) / cell) - 1);
                int minY = Mathf.Max(0, Mathf.FloorToInt((z - half) / cell));
                int maxY = Mathf.Min(height - 1, Mathf.CeilToInt((z + half) / cell) - 1);
                for (int yy = minY; yy <= maxY; yy++)
                    for (int xx = minX; xx <= maxX; xx++) blocked[yy * width + xx] = true;
            }
        }

        private static int FootprintSize(string texture)
        {
            if (texture.EndsWith("2x2Unflyable.tga", StringComparison.OrdinalIgnoreCase)) return 2;
            if (texture.EndsWith("4x4Unflyable.tga", StringComparison.OrdinalIgnoreCase)) return 4;
            if (texture.EndsWith("8x8Unflyable.tga", StringComparison.OrdinalIgnoreCase)) return 8;
            return 0;
        }
    }
}

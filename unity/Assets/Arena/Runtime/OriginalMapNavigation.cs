using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    // The authority asks Unity's measured terrain/pathing adapter in Warcraft
    // coordinates. Clients render authoritative positions and do not run this
    // solver to decide outcomes.
    public sealed class OriginalMapNavigation : IOriginalWorldNavigation, IOriginalWorldDynamicNavigation
    {
        readonly ArenaMap map;
        readonly float scale;
        public OriginalMapNavigation(ArenaMap map)
        {
            this.map = map ? map : throw new ArgumentNullException(nameof(map));
            map.Initialize(); scale = map.unitsPerMeter;
        }
        public OriginalPoint HeroSpawn => Wc(map.HeroSpawn);
        public long NavigationRevision => map.NavigationRevision;
        public bool IsWalkable(double x, double y, double radius) => map.IsWalkable(World(x, y), (float)(radius / scale));
        public bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius) =>
            map.SegmentClear(World(from.x, from.y), World(to.x, to.y), (float)(radius / scale));
        public OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius)
        {
            var start = World(from.x, from.y); var end = World(to.x, to.y);
            var path = map.FindPath(start, end, (float)(radius / scale));
            var result = new OriginalPoint[path.Count];
            for (int i = 0; i < result.Length; i++) result[i] = Wc(path[i]);
            // The map searches in floats, while authoritative body contacts use
            // doubles. Its unchanged endpoint must not become a different,
            // rounded physical waypoint inside a touching body. Projected
            // endpoints remain projected; do not snap actors or widen collision.
            if (path.Count > 0 && path[0].x == start.x && path[0].z == start.z) result[0] = from;
            if (path.Count > 1 && path[path.Count - 1].x == end.x && path[path.Count - 1].z == end.z)
                result[result.Length - 1] = to;
            return result;
        }
        public bool SetDoodadAlive(int editorId, bool alive) => map.SetDoodadAlive(editorId, alive);
        public bool TryAddDynamicDoodad(int editorId, string rawcode, OriginalPoint position, double facingDegrees, double visualScale) =>
            map.TryAddDynamicDoodad(editorId, rawcode, World(position.x, position.y), (float)facingDegrees, (float)visualScale);
        public bool RemoveDynamicDoodad(int editorId) => map.RemoveDynamicDoodad(editorId);
        public OriginalWorldDoodadView[] Destructables(OriginalNativeCatalog native)
        {
            var result = new List<OriginalWorldDoodadView>();
            foreach (var doodad in map.Layout.doodads)
            {
                if (doodad.id != "LTbr" && doodad.id != "LTbs" && doodad.id != "LTex") continue;
                double maximum = native.DestructableHP(doodad.id).Require();
                result.Add(new OriginalWorldDoodadView { editorId = doodad.editorId, rawcode = doodad.id,
                    position = new OriginalPoint(doodad.x, doodad.y), maxHealth = maximum,
                    health = maximum * doodad.life / 100.0 });
            }
            return result.ToArray();
        }
        Vector3 World(double x, double y) => new Vector3((float)(x / scale), 0, (float)(y / scale));
        OriginalPoint Wc(Vector3 point) => new OriginalPoint(point.x * scale, point.z * scale);
    }
}

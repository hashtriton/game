using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        readonly HashSet<int> dynamicScenery = new HashSet<int>();

        void ApplyDynamicDoodadSnapshot(OriginalWorldSnapshot snapshot)
        {
            var present = new HashSet<int>();
            foreach (var doodad in snapshot.doodads)
            {
                if (!doodad.dynamic) continue;
                present.Add(doodad.editorId);
                if (!dynamicScenery.Contains(doodad.editorId))
                {
                    // Own procedural landmark, not the map's original model.
                    // Geometry is visual only; host pathing owns the footprint.
                    var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    root.name = "B009 dynamic " + doodad.editorId;
                    root.transform.SetParent(actorRoot, false);
                    var collider = root.GetComponent<Collider>();
                    if (collider) { collider.enabled = false; Destroy(collider); }
                    var renderer = root.GetComponent<Renderer>();
                    if (effectMaterial) renderer.sharedMaterial = effectMaterial;
                    var properties = new MaterialPropertyBlock();
                    var color = new Color(.32f, .08f, .65f, 1);
                    properties.SetColor("_BaseColor", color); properties.SetColor("_Color", color);
                    renderer.SetPropertyBlock(properties);
                    scenery.Add(doodad.editorId, new Scenery { root = root, initiallyActive = true });
                    dynamicScenery.Add(doodad.editorId);
                }
                var prop = scenery[doodad.editorId];
                float visualScale = (float)doodad.scale;
                prop.root.transform.position = WorldPoint(doodad.position) + Vector3.up * visualScale;
                prop.root.transform.rotation = Quaternion.Euler(0, (float)doodad.facingDegrees, 0);
                prop.root.transform.localScale = new Vector3(3 * visualScale, visualScale, 3 * visualScale);
                prop.root.SetActive(doodad.health > 0);
                prop.bounds = prop.root.GetComponent<Renderer>().bounds;
            }
            foreach (int id in new List<int>(dynamicScenery))
                if (!present.Contains(id)) RemoveDynamicScenery(id);
        }

        void RemoveDynamicScenery(int id)
        {
            if (scenery.TryGetValue(id, out var prop))
            { if (prop.root) Destroy(prop.root); scenery.Remove(id); }
            dynamicScenery.Remove(id);
        }
        void ClearDynamicScenery()
        {
            foreach (int id in new List<int>(dynamicScenery)) RemoveDynamicScenery(id);
        }
    }
}

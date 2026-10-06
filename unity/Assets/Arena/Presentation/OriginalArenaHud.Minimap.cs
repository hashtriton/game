using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaHud
    {
        RectTransform minimapSurface;
        readonly Dictionary<int,RectTransform> mapMarkers=new Dictionary<int,RectTransform>();
        readonly HashSet<int> mapSeen=new HashSet<int>();
        readonly List<int> mapRemoved=new List<int>();
        void RefreshMinimap(OriginalWorldSnapshot world)
        {
            if(!minimapSurface)return;
            mapSeen.Clear();
            if(world!=null)foreach(var unit in world.units)
            {
                if(unit.health<=0||!runtime.UnitVisible(unit))continue;
                mapSeen.Add(unit.entityId);
                if(!mapMarkers.TryGetValue(unit.entityId,out var marker))
                {
                    marker=Box(minimapSurface,"Unit marker "+unit.entityId,Vector2.zero,Vector2.zero,
                        unit.ownerSlot==0?new Color(.95f,.42f,.35f):unit.ownerSlot==Network.LocalSlot?gold:green,false);
                    marker.sizeDelta=Vector2.one*(unit.kind==OriginalWorldUnitKind.Hero?5:3);
                    mapMarkers.Add(unit.entityId,marker);
                }
                var bounds=runtime.map.WorldBounds;var p=runtime.WorldPoint(unit.position);
                marker.GetComponent<UnityEngine.UI.Image>().color=unit.ownerSlot==Network.LocalSlot?gold:
                    OriginalHudRelations.IsEnemy(Network.View,Network.LocalSlot,unit.ownerSlot)?new Color(.95f,.42f,.35f):green;
                marker.anchorMin=marker.anchorMax=new Vector2(Mathf.InverseLerp(bounds.min.x,bounds.max.x,p.x),Mathf.InverseLerp(bounds.min.z,bounds.max.z,p.z));
            }
            mapRemoved.Clear();foreach(var pair in mapMarkers)if(!mapSeen.Contains(pair.Key))mapRemoved.Add(pair.Key);
            foreach(int id in mapRemoved){Destroy(mapMarkers[id].gameObject);mapMarkers.Remove(id);}
            if(minimapMarker)minimapMarker.SetAsLastSibling();
        }
    }
}

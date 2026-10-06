using System.Collections.Generic;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        sealed class GroundVisual
        {
            internal GameObject root;
            internal OriginalGroundItemView state;
            internal Bounds bounds;
            internal readonly MaterialPropertyBlock color=new MaterialPropertyBlock();
        }
        readonly Dictionary<long,GroundVisual> groundVisuals=new Dictionary<long,GroundVisual>();
        readonly HashSet<long> seenGround=new HashSet<long>();
        readonly List<long> removedGroundVisuals=new List<long>();
        public long HoveredGroundItem { get; private set; }
        public bool HoveredWell { get; private set; }
        GameObject wellVisual;
        Bounds wellBounds;

        void UpdateWorldItems()
        {
            seenGround.Clear();
            if(View?.started==true&&View.groundItems!=null)foreach(var ground in View.groundItems)
            {
                if(ground?.item==null||ground.item.removed)continue;
                long id=ground.item.instanceId;seenGround.Add(id);
                if(!groundVisuals.TryGetValue(id,out var visual))
                {
                    visual=new GroundVisual{root=ArenaEffects.Orb(actorRoot,effectMaterial,Vector3.zero,.5f,GroundColor(ground.item))};
                    visual.root.name="Ground item "+id;
                    groundVisuals.Add(id,visual);
                }
                visual.state=ground;
                ApplyEffectColor(visual.root.GetComponent<Renderer>(),visual.color,GroundColor(ground.item));
                visual.root.transform.position=WorldPoint(ground.position)+Vector3.up*.35f;
                visual.bounds=new Bounds(visual.root.transform.position,Vector3.one*.7f);
            }
            removedGroundVisuals.Clear();foreach(var pair in groundVisuals)if(!seenGround.Contains(pair.Key))removedGroundVisuals.Add(pair.Key);
            foreach(long id in removedGroundVisuals){if(groundVisuals[id].root)Destroy(groundVisuals[id].root);groundVisuals.Remove(id);}
            UpdateWellVisual();
            HoveredGroundItem=0;HoveredWell=false;
            if(InputBlocked||Mouse.current==null||PointerOverUi()||!viewCamera)return;
            var ray=viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            FindPointerCombatTarget(ray,out _,out _,out float combatDistance);
            if(!FindWorldInteraction(ray,out long item,out bool well,out float distance)||distance>=combatDistance)return;
            HoveredGroundItem=item;HoveredWell=well;
        }

        Color GroundColor(OriginalItemInstance item)
        {
            if(item.ownerId!=0)return item.ownerId==network.LocalSlot?new Color(1,.74f,.25f):new Color(.68f,.46f,.37f);
            if(item.itemId=="rhe2"||item.itemId=="rres")return new Color(.32f,1,.52f);
            if(item.itemId=="rman")return new Color(.30f,.75f,1);
            if(item.itemId=="vamp")return new Color(.92f,.27f,.44f);
            return new Color(.88f,.79f,.41f);
        }

        void UpdateWellVisual()
        {
            var well=View?.well;
            bool visible=View?.started==true&&well?.present==true;
            if(!visible){if(wellVisual)wellVisual.SetActive(false);return;}
            if(!wellVisual)
            {
                if(wellPrefab)
                {
                    wellVisual=Instantiate(wellPrefab,actorRoot);wellVisual.name="Healing well";
                    foreach(var collider in wellVisual.GetComponentsInChildren<Collider>(true))
                    {collider.enabled=false;Destroy(collider);}
                    var renderers=wellVisual.GetComponentsInChildren<Renderer>(true);
                    if(renderers.Length>0)
                    {
                        var bounds=renderers[0].bounds;
                        foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
                        float diameter=Mathf.Max(bounds.size.x,bounds.size.z);
                        if(diameter>.01f)wellVisual.transform.localScale*=2.5f/diameter;
                    }
                }
                else
                {
                    wellVisual=ArenaEffects.Orb(actorRoot,effectMaterial,Vector3.zero,1,new Color(.25f,.85f,1));
                    wellVisual.name="Healing well";wellVisual.transform.localScale=new Vector3(2.5f,.35f,2.5f);
                }
            }
            wellVisual.SetActive(true);
            var groundPoint=WorldPoint(well.position);
            wellVisual.transform.position=groundPoint;
            var visuals=wellVisual.GetComponentsInChildren<Renderer>(true);
            wellBounds=new Bounds(wellVisual.transform.position+Vector3.up*.5f,new Vector3(2.5f,1,2.5f));
            if(visuals.Length>0)
            {
                wellBounds=visuals[0].bounds;foreach(var renderer in visuals)wellBounds.Encapsulate(renderer.bounds);
                var offset=Vector3.up*(groundPoint.y-wellBounds.min.y);
                wellVisual.transform.position+=offset;wellBounds.center+=offset;
            }
        }

        bool FindWorldInteraction(Ray ray,out long item,out bool well,out float distance)
        {
            item=0;well=false;distance=float.PositiveInfinity;
            foreach(var pair in groundVisuals)
                if(pair.Value.root&&pair.Value.root.activeInHierarchy&&pair.Value.bounds.IntersectRay(ray,out float hit)&&hit<distance)
                {item=pair.Key;well=false;distance=hit;}
            if(wellVisual&&wellVisual.activeInHierarchy&&wellBounds.IntersectRay(ray,out float wellHit)&&wellHit<distance)
            {item=0;well=true;distance=wellHit;}
            return item!=0||well;
        }

        bool SendWorldInteraction(long item,bool well)
        {
            if(InputBlocked||SelectedUnit==null||SelectedUnit.health<=0)return false;
            AttackTargetArmed=false;ArmedSkillId=null;ArmedItemInstance=0;
            return network.SendCommand(well?OriginalSessionCommandKind.InteractWell:OriginalSessionCommandKind.InteractItem,
                itemInstanceId:item,actorEntityId:SelectedUnit.entityId);
        }

        public Vector3 WellScreenPoint => viewCamera&&wellVisual&&wellVisual.activeInHierarchy?
            viewCamera.WorldToScreenPoint(wellBounds.center+Vector3.up*(wellBounds.extents.y+.35f)):new Vector3(0,0,-1);

        void ClearWorldItems()
        {
            foreach(var visual in groundVisuals.Values)if(visual.root)Destroy(visual.root);
            groundVisuals.Clear();HoveredGroundItem=0;HoveredWell=false;
            if(wellVisual)Destroy(wellVisual);wellVisual=null;
        }
    }
}

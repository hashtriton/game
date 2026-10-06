using Arena.Original;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arena
{
    public sealed class OriginalMinimapInput : MonoBehaviour,IPointerDownHandler,IDragHandler
    {
        public OriginalArenaRuntime runtime;
        public void OnPointerDown(PointerEventData data)=>Apply(data);
        public void OnDrag(PointerEventData data)
        {if(data.button==PointerEventData.InputButton.Left)Apply(data);}
        void Apply(PointerEventData data)
        {
            if(!runtime||runtime.InputBlocked||!runtime.map)return;
            var rect=(RectTransform)transform;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,data.position,data.pressEventCamera,out var point))return;
            float x=Mathf.InverseLerp(rect.rect.xMin,rect.rect.xMax,point.x),y=Mathf.InverseLerp(rect.rect.yMin,rect.rect.yMax,point.y);
            var bounds=runtime.map.WorldBounds;
            var target=new Vector3(Mathf.Lerp(bounds.min.x,bounds.max.x,x),0,Mathf.Lerp(bounds.min.z,bounds.max.z,y));
            target.y=runtime.map.SampleHeight(target);
            if(data.button==PointerEventData.InputButton.Left&&runtime.AttackTargetArmed)runtime.RequestMinimapOrder(target,true);
            else if(data.button==PointerEventData.InputButton.Left&&runtime.cameraControl)runtime.cameraControl.CenterAt(target);
            else if(data.button==PointerEventData.InputButton.Right)runtime.RequestMinimapOrder(target);
        }
    }
}

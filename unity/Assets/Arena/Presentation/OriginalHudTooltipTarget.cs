using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arena
{
    public sealed class OriginalHudTooltipTarget : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        internal OriginalArenaHud owner;
        internal Func<string> description;
        public void OnPointerEnter(PointerEventData data)=>owner.ShowTooltip(this);
        public void OnPointerExit(PointerEventData data)=>owner.HideTooltip(this);
        void OnDisable(){if(owner)owner.HideTooltip(this);}
    }
}

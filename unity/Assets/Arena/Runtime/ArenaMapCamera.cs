using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena
{
    public sealed class ArenaMapCamera : MonoBehaviour
    {
        public ArenaGame game;
        public Camera view;
        public bool Overview { get; private set; }
        float distance=29;

        public void SetOverview(bool enabled)
        {
            Overview=enabled;
            Apply(1);
        }

        void LateUpdate()
        {
            if(!view || !game || !game.Hero)return;
            if(Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame)SetOverview(!Overview);
            if(!Overview && Mouse.current!=null)
                distance=Mathf.Clamp(distance-Mouse.current.scroll.ReadValue().y*.025f,19,52);
            Apply(Mathf.Min(1,Time.unscaledDeltaTime*10));
        }

        void Apply(float blend)
        {
            if(!view || !game || !game.Hero)return;
            view.orthographic=false;
            if(Overview)
            {
                // Reserve the lower status panel and upper toolbar when fitting the full map.
                // A narrow perspective keeps the existing URP overlay UI projection stable.
                view.fieldOfView=20;
                float altitude=Mathf.Max(96,68/Mathf.Max(.1f,view.aspect))/Mathf.Tan(10*Mathf.Deg2Rad);
                view.farClipPlane=altitude+200;
                view.transform.position=new Vector3(0,altitude,-2);
                view.transform.rotation=Quaternion.Euler(90,0,0);
            }
            else
            {
                view.fieldOfView=50;view.farClipPlane=350;
                var target=game.Hero.position;
                var offset=new Vector3(0,distance*.83f,-distance*.56f);
                view.transform.position=Vector3.Lerp(view.transform.position,target+offset,blend);
                view.transform.rotation=Quaternion.Euler(56,0,0);
            }
        }
    }
}

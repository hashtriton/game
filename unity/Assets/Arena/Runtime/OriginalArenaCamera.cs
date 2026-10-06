using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena
{
    [RequireComponent(typeof(Camera))]
    public sealed class OriginalArenaCamera : MonoBehaviour
    {
        public ArenaMap map;
        public Transform hero;
        public bool inputBlocked;
        public bool Overview { get; private set; }
        Camera view;
        Vector3 focus;
        float distance = 26;
        bool initialized;

        public void CenterHero()
        {
            if (hero) focus = hero.position;
            else if (map) focus = map.HeroSpawn;
            Overview = false; initialized = true;
        }

        public void CenterAt(Vector3 position, float? viewDistance = null)
        {
            focus = position;
            if (viewDistance.HasValue) distance = Mathf.Clamp(viewDistance.Value, 17, 48);
            Overview = false; initialized = true;
        }

        void LateUpdate()
        {
            if (!map) return;
            if (!view) view = GetComponent<Camera>();
            if (!initialized) CenterHero();
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (!inputBlocked)
            {
                if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.f1Key.wasPressedThisFrame)) CenterHero();
                if (keyboard != null && keyboard.tabKey.wasPressedThisFrame) Overview = !Overview;
                if (!Overview)
                {
                    if (mouse != null) distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y * .02f, 17, 48);
                    Vector3 pan = Vector3.zero;
                    if (keyboard != null)
                    {
                        if (keyboard.leftArrowKey.isPressed) pan.x--;
                        if (keyboard.rightArrowKey.isPressed) pan.x++;
                        if (keyboard.downArrowKey.isPressed) pan.z--;
                        if (keyboard.upArrowKey.isPressed) pan.z++;
                    }
                    focus += pan.normalized * (distance * .65f * Time.unscaledDeltaTime);
                    var bounds = map.WorldBounds;
                    focus.x = Mathf.Clamp(focus.x, bounds.min.x, bounds.max.x);
                    focus.z = Mathf.Clamp(focus.z, bounds.min.z, bounds.max.z);
                    focus.y = map.SampleHeight(focus);
                }
            }
            view.orthographic = false;
            view.fieldOfView = Overview ? 25 : 48;
            view.farClipPlane = 600;
            Vector3 position;
            Quaternion rotation;
            if (Overview)
            {
                position = new Vector3(map.ArenaBounds.center.x, 200, map.ArenaBounds.center.z);
                rotation = Quaternion.Euler(90, 0, 0);
            }
            else
            {
                position = focus + new Vector3(0, distance * .83f, -distance * .56f);
                rotation = Quaternion.Euler(56, 0, 0);
            }
            float blend = 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime);
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, position, blend), Quaternion.Slerp(transform.rotation, rotation, blend));
        }
    }
}

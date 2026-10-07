using Arena;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    /// <summary>
    /// Warcraft-style camera: fixed pitch and yaw, zoom with the wheel, pan with arrows / screen edge / middle mouse.
    /// It follows the hero until the player pans away; Space or F1 locks it back on.
    /// </summary>
    public sealed class RtsCamera : MonoBehaviour
    {
        public Transform follow;
        public ArenaMap map;
        public Bounds limits;
        public float pitch = 56f;
        public float distance = 22f;
        public float minDistance = 10f;
        public float maxDistance = 46f;
        public float panSpeed = 24f;
        public float edgeMargin = 4f;
        public bool edgeScroll = true;
        public bool lockToHero = true;

        private Vector3 focus;
        private float targetDistance;

        public void Focus(Vector3 point)
        {
            focus = point;
            lockToHero = false;
        }

        private void Start()
        {
            targetDistance = distance;
            // In the Editor the cursor sits on the Game view edge or outside it, which would scroll the map away.
            if (Application.isEditor) edgeScroll = false;
            if (follow != null) focus = follow.position;
            Apply();
        }

        private void LateUpdate()
        {
            var dt = Time.unscaledDeltaTime;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            var pan = Vector2.zero;

            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.isPressed) pan.x -= 1f;
                if (keyboard.rightArrowKey.isPressed) pan.x += 1f;
                if (keyboard.upArrowKey.isPressed) pan.y += 1f;
                if (keyboard.downArrowKey.isPressed) pan.y -= 1f;
                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.f1Key.wasPressedThisFrame) lockToHero = true;
            }

            if (mouse != null)
            {
                if (edgeScroll && Application.isFocused) pan += EdgePan(mouse.position.ReadValue());
                var wheel = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f)
                    targetDistance = Mathf.Clamp(targetDistance - Mathf.Sign(wheel) * 2.5f, minDistance, maxDistance);
                if (mouse.middleButton.isPressed)
                {
                    var drag = mouse.delta.ReadValue();
                    var scale = distance * 0.0016f;
                    focus -= new Vector3(drag.x * scale, 0f, drag.y * scale * 1.6f);
                    lockToHero = false;
                }
            }

            if (pan.sqrMagnitude > 0f)
            {
                lockToHero = false;
                // Pan faster when zoomed out so the screen crossing time stays constant.
                focus += new Vector3(pan.x, 0f, pan.y) * (panSpeed * (distance / 26f) * dt);
            }
            else if (lockToHero && follow != null)
            {
                focus = Vector3.Lerp(focus, follow.position, 1f - Mathf.Exp(-9f * dt));
            }

            focus.x = Mathf.Clamp(focus.x, limits.min.x, limits.max.x);
            focus.z = Mathf.Clamp(focus.z, limits.min.z, limits.max.z);
            distance = Mathf.Lerp(distance, targetDistance, 1f - Mathf.Exp(-10f * dt));
            Apply();
        }

        private Vector2 EdgePan(Vector2 cursor)
        {
            if (cursor.x < 0f || cursor.y < 0f || cursor.x > Screen.width || cursor.y > Screen.height) return Vector2.zero;
            var pan = Vector2.zero;
            if (cursor.x <= edgeMargin) pan.x -= 1f;
            else if (cursor.x >= Screen.width - 1 - edgeMargin) pan.x += 1f;
            if (cursor.y <= edgeMargin) pan.y -= 1f;
            else if (cursor.y >= Screen.height - 1 - edgeMargin) pan.y += 1f;
            return pan;
        }

        private void Apply()
        {
            focus.y = map != null ? map.SampleHeight(focus) : 0f;
            var rotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.rotation = rotation;
            transform.position = focus - rotation * Vector3.forward * distance;
        }
    }
}

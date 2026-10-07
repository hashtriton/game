using Arena;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public float distance = 19f;
        public float minDistance = 8f;
        public float maxDistance = 46f;
        public float panSpeed = 24f;
        /// <summary>Width of the scroll zone at the screen border in pixels at 1080p.</summary>
        public float edgeMargin = 12f;
        public bool edgeScroll = true;
        public bool lockToHero = true;

        private Vector3 focus;
        private float targetDistance;
        private Vector2? firstCursor;
        private bool edgeArmed;

        public void Focus(Vector3 point)
        {
            focus = point;
            lockToHero = false;
        }

        private void Start()
        {
            targetDistance = distance;
#if !UNITY_EDITOR
            // Keeps the cursor in the window so pushing it against the border scrolls the map.
            Cursor.lockState = CursorLockMode.Confined;
#endif
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
                var cursor = mouse.position.ReadValue();
                // A cursor that was already resting on the border when the game started must not scroll the map away.
                if (!firstCursor.HasValue) firstCursor = cursor;
                else if (!edgeArmed && (cursor - firstCursor.Value).sqrMagnitude > 4f) edgeArmed = true;
                if (edgeScroll && edgeArmed && Application.isFocused)
                    pan += EdgePan(cursor, Screen.width, Screen.height, Mathf.Max(4f, edgeMargin * Screen.height / 1080f));
                // Over a window the wheel scrolls the window and the middle button belongs to it.
                var overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                var wheel = overUi ? 0f : mouse.scroll.ReadValue().y;
                if (Mathf.Abs(wheel) > 0.01f)
                    targetDistance = Mathf.Clamp(targetDistance - Mathf.Sign(wheel) * 2.5f, minDistance, maxDistance);
                if (mouse.middleButton.isPressed && !overUi)
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

        /// <summary>Scroll direction for a cursor position; zero when the cursor is not inside the window.</summary>
        public static Vector2 EdgePan(Vector2 cursor, float width, float height, float margin)
        {
            if (cursor.x < 0f || cursor.y < 0f || cursor.x > width || cursor.y > height) return Vector2.zero;
            var pan = Vector2.zero;
            if (cursor.x <= margin) pan.x -= 1f;
            else if (cursor.x >= width - 1f - margin) pan.x += 1f;
            if (cursor.y <= margin) pan.y -= 1f;
            else if (cursor.y >= height - 1f - margin) pan.y += 1f;
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

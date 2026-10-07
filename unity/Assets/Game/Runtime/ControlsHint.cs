using UnityEngine;

namespace Game
{
    /// <summary>Temporary on-screen controls reminder until the real HUD exists.</summary>
    public sealed class ControlsHint : MonoBehaviour
    {
        public float visibleSeconds = 20f;

        private GUIStyle style;

        private void OnGUI()
        {
            if (Time.unscaledTime > visibleSeconds) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(15f * Screen.height / 1080f), alignment = TextAnchor.LowerLeft };
                style.normal.textColor = new Color(0.85f, 0.82f, 0.72f, 0.9f);
            }
            GUI.Label(new Rect(16f, Screen.height - 76f * Screen.height / 1080f, Screen.width - 32f, 64f * Screen.height / 1080f),
                "ПКМ - идти    Колесо - масштаб    Стрелки / средняя кнопка - камера    Space - камера к герою", style);
        }
    }
}

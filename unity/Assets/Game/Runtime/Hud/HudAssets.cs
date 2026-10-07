using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>The look of the interface: skin sprites, fonts and the item icons, wired up by the scene builder.</summary>
    [CreateAssetMenu(menuName = "Game/Hud assets")]
    public sealed class HudAssets : ScriptableObject
    {
        public Font bodyFont;
        public Font titleFont;

        public Sprite panel;
        public Sprite tooltip;
        public Sprite slot;
        public Sprite frame;
        public Sprite button;
        public Sprite tab;
        public Sprite barFill;
        public Sprite divider;
        public Sprite coin;
        public Sprite soul;

        public string[] iconIds = new string[0];
        public Sprite[] icons = new Sprite[0];

        private Dictionary<string, Sprite> iconIndex;

        /// <summary>The icon of an item by its inventory id, or null when none was drawn.</summary>
        public Sprite Icon(string itemId)
        {
            if (itemId == null) return null;
            if (iconIndex == null)
            {
                iconIndex = new Dictionary<string, Sprite>(iconIds.Length);
                for (var i = 0; i < iconIds.Length && i < icons.Length; i++) iconIndex[iconIds[i]] = icons[i];
            }
            return iconIndex.TryGetValue(itemId, out var sprite) ? sprite : null;
        }

        private void OnEnable()
        {
            iconIndex = null;
        }

        private void OnValidate()
        {
            iconIndex = null;
        }
    }
}

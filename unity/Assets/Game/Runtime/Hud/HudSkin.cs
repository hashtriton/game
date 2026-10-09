using UnityEngine;

namespace Game
{
    // Shared visual family; item data and original item paintings stay in HudAssets.
    public sealed class HudSkin : ScriptableObject
    {
        public Sprite plate, frame, badge, segment, ring, smoke, backing, portraitMask, portraitFrame, portrait;
        public Sprite q, w, e, r, passive, attack, armor, locked, coin, soul;
        public Sprite shopWindow, shopTooltip, shopTab, shopHover, shopSelected;

        public static readonly Color White = new Color(0.96f, 0.97f, 1f);
        public static readonly Color Cyan = new Color(0.12f, 0.76f, 1f);
        public static readonly Color ManaNumeral = new Color(0.26f, 0.87f, 1f);
        public static readonly Color Muted = new Color(0.62f, 0.70f, 0.78f);
        public static readonly Color LockedGlyph = new Color(0.84f, 0.89f, 0.95f);
        public static readonly Color ShopMuted = new Color(0.76f, 0.82f, 0.89f);
        public static readonly Color ShopGold = new Color(1f, 0.85f, 0.49f);
        public static readonly Color ShopGood = new Color(0.60f, 1f, 0.76f);
        public static readonly Color ShopBad = new Color(1f, 0.68f, 0.63f);
    }
}

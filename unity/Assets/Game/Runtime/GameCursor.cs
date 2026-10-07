using UnityEngine;

namespace Game
{
    public enum CursorKind { Default, Attack }

    /// <summary>
    /// Cursors drawn in code: the system arrow normally, a red sword over something the hero can attack.
    /// Generated once, so there is no image to import and nothing to ship besides the code.
    /// </summary>
    public static class GameCursor
    {
        private const int Size = 32;

        private static Texture2D attack;
        private static CursorKind current = CursorKind.Default;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            attack = null;
            current = CursorKind.Default;
        }

        public static void Set(CursorKind kind)
        {
            if (kind == current) return;
            current = kind;
            if (kind == CursorKind.Default)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                return;
            }
            if (attack == null) attack = DrawSword();
            Cursor.SetCursor(attack, new Vector2(3f, 3f), CursorMode.Auto);
        }

        /// <summary>Diagonal blade, cross guard and pommel with a dark outline, tip at the upper left.</summary>
        private static Texture2D DrawSword()
        {
            var pixels = new Color32[Size * Size];
            var bladeFrom = new Vector2(5f, 5f);
            var bladeTo = new Vector2(23f, 23f);
            var direction = (bladeTo - bladeFrom).normalized;
            var normal = new Vector2(-direction.y, direction.x);
            var guardCentre = bladeFrom + direction * 19f;
            var pommel = bladeFrom + direction * 25.5f;

            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    var along = Vector2.Dot(p - bladeFrom, direction);
                    var across = Vector2.Dot(p - bladeFrom, normal);
                    var guardAcross = Vector2.Dot(p - guardCentre, normal);
                    var guardAlong = Vector2.Dot(p - guardCentre, direction);

                    // Blade narrows to a point at the tip.
                    var halfWidth = Mathf.Lerp(0.2f, 2.0f, Mathf.Clamp01(along / 6f));
                    var inBlade = along >= 0f && along <= 19f && Mathf.Abs(across) <= halfWidth;
                    var inGuard = Mathf.Abs(guardAlong) <= 1.3f && Mathf.Abs(guardAcross) <= 6f;
                    var inGrip = along > 19f && along <= 24f && Mathf.Abs(across) <= 1.3f;
                    var inPommel = Vector2.Distance(p, pommel) <= 2.4f;

                    // Flip y: textures are bottom-up, the hotspot is measured from the top left.
                    var index = (Size - 1 - y) * Size + x;
                    if (inBlade) pixels[index] = along < 16f ? new Color32(214, 220, 228, 255) : new Color32(150, 40, 32, 255);
                    else if (inGuard) pixels[index] = new Color32(176, 40, 30, 255);
                    else if (inGrip) pixels[index] = new Color32(96, 36, 28, 255);
                    else if (inPommel) pixels[index] = new Color32(176, 40, 30, 255);
                }
            }

            // One pixel dark outline around the shape so it reads on any ground.
            var outlined = (Color32[])pixels.Clone();
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (pixels[y * Size + x].a != 0) continue;
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        for (var dx = -1; dx <= 1; dx++)
                        {
                            var nx = x + dx;
                            var ny = y + dy;
                            if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) continue;
                            if (pixels[ny * Size + nx].a == 0) continue;
                            outlined[y * Size + x] = new Color32(12, 8, 8, 255);
                        }
                    }
                }
            }

            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "AttackCursor" };
            texture.SetPixels32(outlined);
            texture.Apply();
            return texture;
        }
    }
}

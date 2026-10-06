using System;

namespace Arena.Original
{
    // Implementations must leave navigation unchanged when returning false.
    // Dynamic destructables own footprints separately from moving unit bodies.
    public interface IOriginalWorldDynamicNavigation
    {
        bool TryAddDynamicDoodad(int editorId, string rawcode, OriginalPoint position, double facingDegrees, double scale);
        bool RemoveDynamicDoodad(int editorId);
    }

    public sealed partial class OriginalWorld
    {
        public const int FirstDynamicDoodadId = 1000000;
        int nextDynamicDoodadId = FirstDynamicDoodadId;

        public int AddDynamicDoodad(string rawcode, OriginalPoint position, double maxHealth, bool invulnerable,
            double facingDegrees = 0, double scale = 1)
        {
            Coordinates(position);
            // Current authored dynamic family only. war3map.w3b B009/LTbx:
            // HP9999 at3555, both alive/dead path masks8x8 at3513/3571.
            // Source Vrv creates it invulnerable, facing0, visual scale1.2.
            if (rawcode != "B009" || maxHealth != 9999 || !invulnerable ||
                !Finite(facingDegrees) || facingDegrees < 0 || facingDegrees >= 360 || facingDegrees % 90 != 0 ||
                !Finite(scale) || scale <= 0 || scale > 8)
                throw new ArgumentException("Unsupported dynamic destructable declaration.");
            if (doodads.Count >= 4096) throw new InvalidOperationException("dynamic-destructable-limit");
            int id = nextDynamicDoodadId, next = checked(id + 1);
            if (!(navigation is IOriginalWorldDynamicNavigation dynamicNavigation) ||
                !dynamicNavigation.TryAddDynamicDoodad(id, rawcode, position, facingDegrees, scale))
                throw new InvalidOperationException("dynamic-destructable-navigation-rejected");
            doodads.Add(id, new OriginalWorldDoodadView { editorId = id, rawcode = rawcode, position = position,
                maxHealth = maxHealth, health = maxHealth, dynamic = true, invulnerable = invulnerable,
                facingDegrees = facingDegrees, scale = scale });
            nextDynamicDoodadId = next; revision++;
            return id;
        }

        public bool RemoveDynamicDoodad(int editorId)
        {
            if (!doodads.TryGetValue(editorId, out var doodad) || !doodad.dynamic) return false;
            if (!(navigation is IOriginalWorldDynamicNavigation dynamicNavigation) || !dynamicNavigation.RemoveDynamicDoodad(editorId)) return false;
            doodads.Remove(editorId); ClearTargets(OriginalWorldTargetKind.Doodad, editorId); revision++;
            return true;
        }
    }
}

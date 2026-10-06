using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalShopView
    {
        public int instanceId;
        public string unitId;
        public OriginalPoint position;
        public bool open;
        public OriginalShopStockView[] stock = Array.Empty<OriginalShopStockView>();
    }

    [Serializable]
    public sealed class OriginalShopStockView
    {
        public string itemId;
        public bool known;
        public int available;
        public double nextAvailableAt;
    }

    // Mechanical placement facts from the selected 3.9c map, not scenery.
    // war3map.normalized.j 15673..15710; layout choice 19215..19231.
    // IDs distinguish two n0AL shops and the retained compact-layout shops.
    public static class OriginalShops
    {
        public const int AcolyteInstanceId = 203;
        public static OriginalShopView[] Placements(bool compact)
        {
            var result = new List<OriginalShopView>();
            string[] standard = { "n05V", "n05W", "n05X", "n05P", "n05Q", "n05T", "n05S", "n05R", "n05U", "n06V", "n0AF", "n05Z", "n05Y" };
            string[] packed = { "n004", "n00C", "n03S", "n005", "n006", "n03G", "n01J", "n02F", "n04X", "n06U", "n0AE", "n014", "n001" };
            int[] x = { 326, 202, 82, 320, 260, -250, 64, 130, -370, -450, -190, -450, -450 };
            int[] y = { 836, 704, 704, 1220, 1340, 1340, 1474, 1340, 1340, 1220, 1474, 704, 836 };
            for (int i = 0; i < standard.Length; i++)
            {
                if (!compact || i >= 11) result.Add(Shop(i + 2, standard[i], x[i], y[i]));
                if (compact) result.Add(Shop(i + 102, packed[i], 200, 1340));
            }
            // Main shops follow j0/J0. These two lie outside Wi/Nn and remain
            // accessible during a wave: source 16381..16382.
            result.Add(Shop(201, "n0AL", -1945, 2605));
            result.Add(Shop(202, "n0AL", 1700, -900));
            // xU11259 creates GB at WV center (-416,608), rect84340;
            // vU11153 removes it at the next Q3. Presence is phase-driven.
            result.Add(Shop(AcolyteInstanceId, "u00E", -416, 608));
            return result.ToArray();
        }

        static OriginalShopView Shop(int id, string unitId, double x, double y) => new OriginalShopView
        { instanceId = id, unitId = unitId, position = new OriginalPoint(x, y) };

        public static bool IsOpen(int instanceId, bool centralShopsOpen, bool acolytePresent = false) =>
            instanceId == AcolyteInstanceId ? acolytePresent : instanceId == 201 || instanceId == 202 || centralShopsOpen;
    }

    [Serializable]
    public sealed class OriginalGroundItemView
    {
        public OriginalItemInstance item;
        public OriginalPoint position;
    }
}

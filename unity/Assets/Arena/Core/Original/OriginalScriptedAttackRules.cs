using System;
using System.Collections.Generic;

namespace Arena.Original
{
    // Declarative Em registrations from LiA3.9c war3map.rawcodes.j:4066-4114.
    // Nv/bm:3862-3871 has two explicit modes. Missing registrations are native
    // hashtable zero, not a guessed object-editor default.
    public static class OriginalScriptedAttackRules
    {
        readonly struct Entry
        {
            internal readonly double a, b, c; internal readonly int mode;
            internal Entry(double a, double b, double c, int mode) { this.a=a; this.b=b; this.c=c; this.mode=mode; }
        }
        static readonly Dictionary<string, Entry> Entries = new Dictionary<string, Entry>(StringComparer.Ordinal)
        {
            { "A0EJ", new Entry(0.0, 80.0, 0.0, 1) }, // 4066
            { "A0AA", new Entry(0.0, 15.0, 0.0, 1) }, // 4067
            { "A10I", new Entry(0.0, 75.0, 0.0, 1) }, // 4068
            { "A0EO", new Entry(0.0, 50.0, 0.0, 1) }, // 4069
            { "A0E3", new Entry(0.0, 20.0, 0.0, 1) }, // 4070
            { "A0GA", new Entry(0.0, 30.0, 0.0, 1) }, // 4071
            { "A0GG", new Entry(0.0, 40.0, 0.0, 1) }, // 4072
            { "B00N", new Entry(75.0, 0.0, 0.0, 1) }, // 4073
            { "B0AT", new Entry(150.0, 0.0, 0.0, 1) }, // 4074
            { "B0AU", new Entry(225.0, 0.0, 0.0, 1) }, // 4075
            { "B07X", new Entry(0.25, 0.0, 0.0, 0) }, // 4076
            { "B07V", new Entry(0.35, 0.0, 0.0, 0) }, // 4077
            { "B07W", new Entry(0.45, 0.0, 0.0, 0) }, // 4078
            { "B041", new Entry(-0.3, 0.0, 0.0, 0) }, // 4079
            { "B04D", new Entry(-0.45, 0.0, 0.0, 0) }, // 4080
            { "B04F", new Entry(-0.6, 0.0, 0.0, 0) }, // 4081
            { "B08M", new Entry(-0.5, 0.0, 0.0, 0) }, // 4082
            { "B0C7", new Entry(-0.3, 0.0, 0.0, 0) }, // 4083
            { "BNso", new Entry(-0.2, -0.35, -0.5, 0) }, // 4084
            { "B05Z", new Entry(-0.3, -0.5, -0.7, 0) }, // 4085
            { "B0CY", new Entry(0.3, 0.55, 0.8, 0) }, // 4086
            { "B02N", new Entry(0.2, 0.0, 0.0, 0) }, // 4087
            { "B04G", new Entry(0.3, 0.0, 0.0, 0) }, // 4088
            { "B04H", new Entry(0.4, 0.0, 0.0, 0) }, // 4089
            { "A06B", new Entry(0.5, 0.75, 1.0, 0) }, // 4090
            { "B00F", new Entry(0.3, 0.0, 0.0, 0) }, // 4091
            { "B04I", new Entry(0.6, 0.0, 0.0, 0) }, // 4092
            { "B04J", new Entry(0.9, 0.0, 0.0, 0) }, // 4093
            { "B04K", new Entry(1.0, 0.0, 0.0, 0) }, // 4094
            { "BNht", new Entry(-0.5, 0.0, 0.0, 0) }, // 4095
            { "B04L", new Entry(-0.6, 0.0, 0.0, 0) }, // 4096
            { "B04M", new Entry(-0.7, 0.0, 0.0, 0) }, // 4097
            { "B04T", new Entry(-0.2, 0.0, 0.0, 0) }, // 4098
            { "B04U", new Entry(-0.3, 0.0, 0.0, 0) }, // 4099
            { "B04V", new Entry(-0.4, 0.0, 0.0, 0) }, // 4100
            { "B058", new Entry(0.04, 0.0, 0.0, 0) }, // 4101
            { "B059", new Entry(0.08, 0.0, 0.0, 0) }, // 4102
            { "B05A", new Entry(0.12, 0.0, 0.0, 0) }, // 4103
            { "B05B", new Entry(0.08, 0.0, 0.0, 0) }, // 4104
            { "B05C", new Entry(0.16, 0.0, 0.0, 0) }, // 4105
            { "B05D", new Entry(0.24, 0.0, 0.0, 0) }, // 4106
            { "B05E", new Entry(0.12, 0.0, 0.0, 0) }, // 4107
            { "B05F", new Entry(0.24, 0.0, 0.0, 0) }, // 4108
            { "B05G", new Entry(0.36, 0.0, 0.0, 0) }, // 4109
            { "B04N", new Entry(-0.5, 0.0, 0.0, 0) }, // 4110
            { "B04O", new Entry(-0.75, 0.0, 0.0, 0) }, // 4111
            { "Bcri", new Entry(-0.3, 0.0, 0.0, 0) }, // 4112
        };
        public static double AbilityBonus(string id, int rank)
        {
            if (id == null || id.Length != 4 || rank < 0) throw new ArgumentOutOfRangeException();
            if (rank == 0 || !Entries.TryGetValue(id, out var e)) return 0;
            return e.mode == 1 ? e.a + e.b * (rank - 1) : rank == 1 ? e.a : rank == 2 ? e.b : rank == 3 ? e.c : 0;
        }
    }
}

using System;

namespace Arena.Original
{
    // LiA3.9c 02a790123096: xC initialization85275..85413; C6:20769.
    // Integer weights10/8/5 preserve authored1/.8/.5 ratios. The host
    // chooses its deterministic candidate draw, not native RNG replay.
    public static class OriginalItemFatePool
    {
        public const int TotalWeight = 1308;
        static readonly string[] Items = {
            "I01Y","I01L","I02H","I03L","I03M","I022","I023","I0AJ","I06O","I06M","I01D","I021",
            "I094","I07K","I07E","I01M","I01A","I00P","I01W","I00Y","I007","I02A","I00K","I01K",
            "I024","I008","I019","I000","I01E","I00E","I00I","I00R","I00C","I002","I00S","I004",
            "I00N","I08L","I08M","I08K","I06B","I07R","I08F","I013","I0AP","I005","I009","I001",
            "I029","I00F","I042","I00O","I07A","I026","I00H","I01I","I03S","I00L","I048","I00W",
            "I072","I03Y","I01F","I02G","I00B","I017","I025","I0AL","I01N","I00Z","I011","I01R",
            "I02C","I015","I01P","I076","I03N","I03T","I07P","I02N","I02E","I03Z","I07T","I087",
            "I07O","I02P","I06R","I01U","I07N","I01B","I07M","I045","I00V","I00T","I02L","I082",
            "I083","I07Y","I04B","I08Q","I07U","I090","I096","I09A","I09G","I09L","I09M","I09N",
            "I0A3","I09P","I09U","I09X","I0AE","I049","I050","I054","I05A","I05D","I05I","I05K",
            "I05P","I05Q","I05Y","I0AD","I060","I064","I06J","I08U","I0AS","I07C","I070","I05E",
            "I0A6","I0A7","I057","I085","I088","I08D","I08I",
        };
        static readonly int[] Weights = {
            10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
            10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
            10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,8,
            10,10,8,8,10,8,10,10,10,10,10,10,10,10,10,10,10,10,8,10,10,10,10,10,
            10,8,8,10,8,8,10,10,10,8,8,8,10,8,10,10,8,8,8,8,8,8,8,10,
            8,8,8,8,5,8,10,10,10,8,8,8,8,5,10,8,5,5,8,
        };
        public static string At(int draw)
        {
            if(draw<1||draw>TotalWeight)throw new ArgumentOutOfRangeException(nameof(draw));
            for(int i=0;i<Items.Length;i++){draw-=Weights[i];if(draw<=0)return Items[i];}
            throw new InvalidOperationException("Fate pool weight mismatch.");
        }
    }
}

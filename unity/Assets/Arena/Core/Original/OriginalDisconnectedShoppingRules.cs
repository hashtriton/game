using System;

namespace Arena.Original
{
    // Own translation of v4v/wC, 3.9c JASS22854/85739..85913.
    // These are authored AI programs, not recommended item builds.
    public static class OriginalDisconnectedShoppingRules
    {
        static readonly string[] Archer={"I00I","I08M","I00J","I007","I008","I00A","I00I","I016","I00L","I081","I007","I03Q","I007","I01E","I00E","I02D","I02D","I011","I00I","I06V","I07O","I064","I05A","I05P"};
        static readonly string[] Knight={"I02A","I000","I004","I006","I00I","I08M","I00J","I007","I03Q","I007","I01E","I00E","I02D","I02D","I000","I002","I003","I00C","I00D","I01Q","I005","I02P","I08Z","I07T","I076","I05A","I05P"};
        static readonly string[] Pyro={"I02A","I08K","I00R","I02F","I019","I019","I01O","I00S","I010","I007","I03P","I00R","I00S","I00E","I00U","I00U","I05Y","I015","I05Z","I045","I05I","I0AE"};
        public static string Item(string hero,int index)
        {
            var rows=hero=="H008"?Knight:hero=="N0A0"?Archer:hero=="H024"?Pyro:null;
            return rows!=null&&index>0&&index<=rows.Length?rows[index-1]:null;
        }
        public static string ReplacedItem(string hero,int index)=>hero=="N0A0"&&index==23||hero=="H008"&&index==26?"I03S":
            hero=="N0A0"&&index==24?"I07O":hero=="H008"&&index==27?"I07T":null;
        public static bool InShopRegion(OriginalPoint p)=>p.x>=-512&&p.x<=384&&p.y>=640&&p.y<=1344;
        // v7v23038: asi is an authored program counter, not current tech rank.
        // An accepted or rejected native order advances it once funds meet the
        // source threshold. The first60 steps buy ten ranks of each basic.
        public static string SoulUpgrade(int index,int uniqueChoice=1)
        {
            if(index<1)throw new ArgumentOutOfRangeException(nameof(index));
            if(index>=61)
            {
                if(uniqueChoice<1||uniqueChoice>6)throw new ArgumentOutOfRangeException(nameof(uniqueChoice));
                return new[]{"R00H","R00J","R00G","R00F","R00I","R00E"}[uniqueChoice-1];
            }
            if(index<=20)return new[]{"R002","R003","R001","R000"}[(index-1)%4];
            if(index<=50)return new[]{"R004","R006","R002","R003","R001","R000"}[(index-21)%6];
            return index%2==1?"R004":"R006";
        }
        public static int SoulThreshold(int index)
        {
            if(index<1)throw new ArgumentOutOfRangeException(nameof(index));
            if(index>=61)return 90;
            if(index<=20)return 2+(index-1)/4;
            if(index<=50)return ((index-21)%6<2?2:7)+(index-21)/6;
            return 7+(index-51)/2;
        }
        public static OriginalPoint DeparturePoint(int slot,double x,double y)
        {
            // v6v uses UC6, the source default; TC maps slot3 to HX and4 to EX.
            double minX,minY,maxX,maxY;
            switch(slot)
            {
                case 1:minX=-512;minY=2464;maxX=-384;maxY=2688;break;
                case 2:case 7:minX=-2048;minY=1024;maxX=-1792;maxY=1152;break;
                case 3:minX=1696;minY=1664;maxX=1824;maxY=1888;break;
                case 4:minX=-2080;minY=-64;maxX=-1984;maxY=128;break;
                case 5:case 8:minX=640;minY=2624;maxX=832;maxY=2720;break;
                case 6:minX=1696;minY=640;maxX=1824;maxY=864;break;
                default:throw new ArgumentOutOfRangeException(nameof(slot));
            }
            return new OriginalPoint(minX+(maxX-minX)*x,minY+(maxY-minY)*y);
        }
    }
}

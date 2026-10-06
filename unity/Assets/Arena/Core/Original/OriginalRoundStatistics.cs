using System;

namespace Arena.Original
{
    // xU11169..11309 has separate PH/QH/sH pools from the duel rating.
    // Strict comparisons retain the first match slot, even when all are zero.
    public sealed class OriginalRoundStatistics
    {
        readonly float[] taken,dealt,casts;
        public OriginalRoundStatistics(int participants)
        {
            if(participants<1||participants>8)throw new ArgumentOutOfRangeException(nameof(participants));
            taken=new float[participants];dealt=new float[participants];casts=new float[participants];
        }
        void Add(float[] pool,int slot,double amount)
        {
            if(slot<1||slot>pool.Length)throw new ArgumentOutOfRangeException(nameof(slot));
            if(!OriginalCombatDefinition.IsFinite(amount)||amount<0||amount>float.MaxValue)throw new ArgumentOutOfRangeException(nameof(amount));
            float next=pool[slot-1]+(float)amount;
            if(float.IsNaN(next)||float.IsInfinity(next))throw new InvalidOperationException("Round statistic overflow.");
            pool[slot-1]=next;
        }
        static double Damage(double damage,double life)
        {
            if(!OriginalCombatDefinition.IsFinite(damage)||damage<0||!OriginalCombatDefinition.IsFinite(life)||life<0)
                throw new ArgumentOutOfRangeException(nameof(damage));
            return Math.Min(damage,life);
        }
        public void RecordTaken(int slot,double damage,double life)=>Add(taken,slot,Damage(damage,life));
        public void RecordDealt(int slot,double damage,double life)=>Add(dealt,slot,Damage(damage,life));
        public void RecordCast(int slot)=>Add(casts,slot,1);
        public void Reset(){Array.Clear(taken,0,taken.Length);Array.Clear(dealt,0,dealt.Length);Array.Clear(casts,0,casts.Length);}
        int Leader(float[] pool){int leader=0;for(int i=1;i<pool.Length;i++)if(pool[i]>pool[leader])leader=i;return leader+1;}
        public int[] Leaders()=>new[]{Leader(taken),Leader(dealt),Leader(casts)};
    }
}

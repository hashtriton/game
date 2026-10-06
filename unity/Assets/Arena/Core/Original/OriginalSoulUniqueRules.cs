using System;

namespace Arena.Original
{
    // Own translation of qP/FP/GP/hP/KP/LP/mP5877..6076 and eQ/Zq/xQ
    // 6798..6865, 3.9c script SHAfe69d5ec5087303ac93a696c602b746565ee5e9e52917f18a9de3ee47d6824e4.
    public static class OriginalSoulUniqueRules
    {
        public static string[] Abilities(string id)
        {
            switch(id)
            {
                case "R00E":return new[]{"A0UH","A1DQ"};
                case "R00F":return new[]{"A1DX"};
                case "R00G":return new[]{"A1DT","A1DU"};
                case "R00H":return new[]{"A1DR","A1DS"};
                case "R00I":return new[]{"A1DW"};
                case "R00J":return new[]{"A1DV"};
                default:throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
        public static double PhaseShiftHealing(double eventDamage,int roll)
        {
            if(!OriginalCombatDefinition.IsFinite(eventDamage)||eventDamage<0||roll<1||roll>100)
                throw new ArgumentOutOfRangeException();
            return eventDamage>10&&roll<=15?eventDamage:0;
        }
        public static bool BladeProc(int attempt,double random)
        {
            if(attempt<1||!OriginalCombatDefinition.IsFinite(random)||random<0||random>1)
                throw new ArgumentOutOfRangeException();
            // qM uses a shared per-unit accumulator and resets only on success.
            return random<=.04301*attempt;
        }
        public static double BladeRadius(int completedSteps)=>150d*(1+completedSteps);
    }
}

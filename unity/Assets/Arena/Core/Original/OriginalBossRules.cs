using System;

namespace Arena.Original
{
    public readonly struct OriginalBossScaling
    {
        public readonly double attack, armor, health;
        public OriginalBossScaling(double attack, double armor, double health)
        { this.attack = attack; this.armor = armor; this.health = health; }
    }
    // LiA3.9c i0 JASS15759..15812 and f3 JASS17784..17837.
    // Bonuses are additive to the native unit, including native hero attributes.
    public static class OriginalBossRules
    {
        public static OriginalBossScaling Scaling(string id, int participants, int round, bool finalAdd, double baseHealth)
        {
            if (participants < 1 || participants > 8 || round < 1 || round > 30 || !OriginalCombatDefinition.IsFinite(baseHealth) || baseHealth <= 0)
                throw new ArgumentOutOfRangeException();
            switch (id)
            {
                case "n00K": return new OriginalBossScaling(30 * participants, 5 * participants, 350 * participants + (round == 25 ? 7500 : 0));
                case "n00Z": return new OriginalBossScaling(60 * participants, 10 * participants, 1000 * participants + (round == 25 ? 5000 : 0));
                case "n017": return new OriginalBossScaling(100 * participants, 10 * participants, 1500 * participants);
                case "u00G": return new OriginalBossScaling(50 * participants, 10 * participants, 2000 * participants);
                case "n0AW": if (!finalAdd) return new OriginalBossScaling(150 * participants, 20 * participants, 2500 * participants); break;
                case "O006": if (!finalAdd) return new OriginalBossScaling(30 * participants, 12 * participants, 3000 * participants); break;
            }
            if (finalAdd) return new OriginalBossScaling(0, 0, baseHealth <= 2000 ? 2000 - (int)baseHealth : (int)(baseHealth * .5));
            throw new InvalidOperationException("Unknown mega boss scaling: " + id);
        }
    }
}

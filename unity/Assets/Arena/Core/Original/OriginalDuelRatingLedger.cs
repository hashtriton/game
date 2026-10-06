using System;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalDuelRatingSnapshot
    {
        public int[] regularKills, bossDamagePoints, survivalPoints, remainingSurvivalPoints;
        public float[] pendingBossDamage;
        public bool combatActive;
        public int combatSeconds;
    }

    // LiA3.9c script fe69d5ec5087303ac93a696c602b746565ee5e9e52917f18a9de3ee47d6824e4:
    // dP5842/fP5873, DU11360/fU11379, l3/L3/M3 at17945/17961/17988.
    // Slots are the compact xK/Ur player indices, not lobby positions or hero IDs.
    // Only OH inputs nH/AH/bH are tracked; gold, native XP, PH and VH are separate.
    public sealed class OriginalDuelRatingLedger
    {
        readonly int participants;
        int[] kills, bossPoints, survivalPoints, survivalRemaining;
        float[] bossDamage;
        bool combatActive;
        int combatSeconds;
        public int Participants => participants;

        public OriginalDuelRatingLedger(int participants)
        {
            if (participants < 1 || participants > 8) throw new ArgumentOutOfRangeException(nameof(participants));
            this.participants = participants;
            kills = new int[participants]; bossPoints = new int[participants]; survivalPoints = new int[participants];
            survivalRemaining = new int[participants]; bossDamage = new float[8];
        }

        // DU is registered on P11 units in gU. Pass the event-stage native
        // damage and life before subtraction. Summons/illusions credit owner.
        // Unmapped contributors have source slot0, excluded from fU's1..8 sum.
        public void RecordEnemyDamage(int contributorSlot, int victimUserData, double eventDamage, double lifeBefore)
        {
            ValidateContributor(contributorSlot); ValidateUserData(victimUserData);
            if (!Finite(eventDamage) || !Finite(lifeBefore) || eventDamage < 0 || lifeBefore < 0 ||
                eventDamage > float.MaxValue || lifeBefore > float.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(eventDamage));
            if (contributorSlot == 0 || victimUserData != 1) return;
            float damage = Math.Min((float)eventDamage, (float)lifeBefore);
            float next = bossDamage[contributorSlot - 1] + damage;
            if (float.IsInfinity(next) || float.IsNaN(next)) throw new InvalidOperationException("Rating damage pool overflow.");
            bossDamage[contributorSlot - 1] = next;
        }

        // Call once for an actual P11 death, after its final DU event. A source
        // null killer maps to slot0. dP is disabled while kr; fU is not.
        // fU pools ALL normal-boss damage globally and settles on any such
        // death. It does not maintain independent ledgers per boss entity.
        public void RecordEnemyDeath(int killerSlot, int victimUserData, bool victimHasA0K4, bool duelActive)
        {
            ValidateContributor(killerSlot); ValidateUserData(victimUserData);
            if (victimUserData == 0)
            {
                if (killerSlot > 0 && !victimHasA0K4 && !duelActive) kills[killerSlot - 1] = checked(kills[killerSlot - 1] + 1);
                return;
            }
            if (victimUserData != 1) return;
            // Preserve the source's float32 addition order, including slot8
            // before slots1..4. All unused compact slots remain zero.
            float total = bossDamage[7] + bossDamage[0];
            total += bossDamage[1]; total += bossDamage[2]; total += bossDamage[3];
            total += bossDamage[4]; total += bossDamage[5]; total += bossDamage[6];
            if (!(total > 0) || float.IsInfinity(total))
                throw new InvalidOperationException("rating-zero-or-invalid-boss-damage-denominator-unresolved");
            var next = (int[])bossPoints.Clone();
            for (int i = 0; i < participants; i++)
            {
                float numerator = 40f * bossDamage[i];
                float share = .5f + numerator / total;
                if (float.IsInfinity(share) || float.IsNaN(share) || share < 0 || share > int.MaxValue)
                    throw new InvalidOperationException("Rating share cannot be represented.");
                next[i] = checked(next[i] + (int)share); // JASS R2I truncation.
            }
            bossPoints = next;
            // The source resets participant slots only. Unused slots here
            // never receive contributions because xK/Ur mapping is explicit.
            Array.Clear(bossDamage, 0, participants);
        }

        // Q3 calls L3 and starts M3 every1s; caller supplies state at each exact
        // timer callback. Do not replace several ticks with the final life state.
        public void BeginCombat()
        {
            // Q3 also replaces an existing Ja timer on an altar retry. Starting
            // again resets NH/DB but does not itself award any bH points.
            for (int i = 0; i < participants; i++) survivalRemaining[i] = 30;
            // Q3 tail calls vU (11141..11152,18062), clearing pending qH.
            // Partial normal-boss damage from a wiped attempt must not survive.
            Array.Clear(bossDamage, 0, participants);
            combatSeconds = 0; combatActive = true;
        }
        public void TickCombatSecond(bool[] sourceDead)
        {
            ValidateDeaths(sourceDead);
            if (!combatActive) throw new InvalidOperationException("Rating combat timer is not active.");
            int nextSecond = checked(combatSeconds + 1);
            if (nextSecond % 2 == 0)
                for (int i = 0; i < participants; i++)
                    if (sourceDead[i] && survivalRemaining[i] > 0) survivalRemaining[i]--;
            combatSeconds = nextSecond;
        }
        // D4 l3 awards30 to a currently living hero, even if previously dead.
        // A currently dead hero gets its remaining NH. Uk means dead, not alive.
        public void CompleteRound(bool[] sourceDead)
        {
            ValidateDeaths(sourceDead);
            if (!combatActive) throw new InvalidOperationException("Rating combat timer is not active.");
            var next = (int[])survivalPoints.Clone();
            for (int i = 0; i < participants; i++)
                next[i] = checked(next[i] + (sourceDead[i] ? survivalRemaining[i] : 30));
            survivalPoints = next; combatActive = false;
        }
        public int Rating(int slot, int heroLevel)
        {
            ValidateSlot(slot);
            if (heroLevel < 1 || heroLevel > 50) throw new ArgumentOutOfRangeException(nameof(heroLevel));
            return OriginalDuel.SourceRating(kills[slot - 1], heroLevel, bossPoints[slot - 1], survivalPoints[slot - 1]);
        }
        public OriginalDuelRatingSnapshot Snapshot() => new OriginalDuelRatingSnapshot
        {
            regularKills = (int[])kills.Clone(), bossDamagePoints = (int[])bossPoints.Clone(),
            survivalPoints = (int[])survivalPoints.Clone(), remainingSurvivalPoints = (int[])survivalRemaining.Clone(),
            pendingBossDamage = (float[])bossDamage.Clone(), combatActive = combatActive, combatSeconds = combatSeconds
        };
        void ValidateDeaths(bool[] sourceDead)
        {
            if (sourceDead == null || sourceDead.Length != participants) throw new ArgumentException("Expected compact participant death states.");
        }
        void ValidateSlot(int slot)
        { if (slot < 1 || slot > participants) throw new ArgumentOutOfRangeException(nameof(slot)); }
        void ValidateContributor(int slot)
        { if (slot < 0 || slot > participants) throw new ArgumentOutOfRangeException(nameof(slot)); }
        static void ValidateUserData(int value)
        { if (value < 0 || value > 2) throw new ArgumentOutOfRangeException(nameof(value)); }
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

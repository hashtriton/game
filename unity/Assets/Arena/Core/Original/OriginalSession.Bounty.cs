using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalNativeBountyRules nativeBounty;
        readonly HashSet<int> bountyDeaths = new HashSet<int>();
        uint bountyRandom;

        public void ConfigureBounty(OriginalObservedBountyCatalog measured)
        {
            if (Started) throw new InvalidOperationException("Configure bounty before starting a match.");
            nativeBounty = new OriginalNativeBountyRules(combatCatalog, measured);
        }

        // Central trusted death bridge calls this once while victim metadata is
        // still available. Owner persists even when a projectile's source actor
        // was removed. This is never a client command or native-XP grant.
        internal int ApplyNativeBounty(int killerOwnerLobbySlot, OriginalMatchEnemy victim)
        {
            if (victim == null || victim.entityId <= 0) throw new ArgumentException("Missing bounty victim.");
            if (match == null) throw new InvalidOperationException("Bounty requires an active match.");
            if (options.equalGold || killerOwnerLobbySlot == 0 || bountyDeaths.Contains(victim.entityId)) return 0;
            var owner = players.Find(p => p.slot == killerOwnerLobbySlot && p.matchSlot > 0);
            if (owner == null || owner.inventory == null) throw new InvalidOperationException("Unmapped bounty owner.");
            if (nativeBounty == null) throw new InvalidOperationException("Native bounty registry is unavailable.");
            uint state = bountyRandom == 0 ? unchecked((uint)seed) ^ 0x424F554Eu : bountyRandom;
            if (state == 0) state = 1;
            int Draw(int sides)
            {
                uint size = (uint)sides, threshold = unchecked(0u - size) % size;
                do { state ^= state << 13; state ^= state >> 17; state ^= state << 5; } while (state < threshold);
                return 1 + (int)(state % size);
            }
            var reward = nativeBounty.Resolve(victim.rawcode, Draw);
            var candidate = owner.inventory.Copy();
            candidate.GrantResources(reward.amount, 0);
            owner.inventory = candidate; bountyRandom = state; bountyDeaths.Add(victim.entityId);
            return reward.amount;
        }
    }
}

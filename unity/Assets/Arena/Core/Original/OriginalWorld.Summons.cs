using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalWorldSummonSpawn
    {
        public int entityId, ownerSlot, sourceHeroEntityId;
        public string rawcode;
        public OriginalWorldUnitProfile profile;
        public OriginalPoint position;
        public double health, mana;
        public bool invulnerable;
    }

    public sealed class OriginalWorldProfileUpdate
    {
        public int entityId;
        public OriginalWorldUnitProfile profile;
        public double health, mana;
    }

    public sealed partial class OriginalWorld
    {
        public const int FirstSummonEntityId = 100000000;
        public const int LastSummonEntityId = 499999999;

        // Host-only item transaction: publish the equipment result and every
        // summon together. Source identity is checked at birth, then retained
        // as provenance; removing a donor does not remove its summons.
        public bool TryPublishSummons(OriginalWorldSummonSpawn[] rows, OriginalWorldProfileUpdate heroUpdate = null)
            => PublishSummons(rows,heroUpdate,false);

        // Source effects already admitted at SPELL_EFFECT retain the owner's
        // canonical identity if a nested damage callback kills that hero.
        // This does not relax the ordinary item transaction's live-source gate.
        internal bool TryPublishSourceSummons(OriginalWorldSummonSpawn[] rows)
            => PublishSummons(rows,null,true);

        bool PublishSummons(OriginalWorldSummonSpawn[] rows, OriginalWorldProfileUpdate heroUpdate, bool allowDeadSource)
        {
            if (rows == null || rows.Length < 1 || rows.Length > 32) throw new ArgumentException("Invalid summon batch.");
            Unit changedHero = null;
            OriginalWorldUnitProfile heroProfile = null;
            if (heroUpdate != null)
            {
                if (!units.TryGetValue(heroUpdate.entityId, out changedHero) || changedHero.kind != OriginalWorldUnitKind.Hero ||
                    changedHero.owner < 1 || changedHero.id != changedHero.owner || changedHero.health <= 0) return false;
                ValidateProfile(heroUpdate.profile);
                heroProfile = heroUpdate.profile.Copy();
                if (!Finite(heroUpdate.health) || heroUpdate.health <= 0 || heroUpdate.health > heroProfile.maxHealth ||
                    !Finite(heroUpdate.mana) || heroUpdate.mana < 0 || heroUpdate.mana > heroProfile.maxMana) return false;
                if (!changedHero.hidden && !changedHero.pathingDisabled &&
                    heroProfile.collisionRadius != changedHero.profile.collisionRadius &&
                    !Free(changedHero.position, heroProfile.collisionRadius, changedHero.id)) return false;
            }
            var candidates = new List<Unit>(rows.Length);
            var ids = new HashSet<int>();
            foreach (var row in rows)
            {
                if (row == null || row.entityId < FirstSummonEntityId || row.entityId > LastSummonEntityId ||
                    !ids.Add(row.entityId) || units.ContainsKey(row.entityId) || row.ownerSlot < 1 || row.ownerSlot > 8 ||
                    row.sourceHeroEntityId != row.ownerSlot || !units.TryGetValue(row.sourceHeroEntityId, out var donor) ||
                    donor.kind != OriginalWorldUnitKind.Hero || donor.owner != row.ownerSlot || (!allowDeadSource && donor.health <= .405)) return false;
                Rawcode(row.rawcode); Coordinates(row.position); ValidateProfile(row.profile);
                var profile = row.profile.Copy();
                if (!Finite(row.health) || row.health <= 0 || row.health > profile.maxHealth ||
                    !Finite(row.mana) || row.mana < 0 || row.mana > profile.maxMana ||
                    !navigation.IsWalkable(row.position.x, row.position.y, profile.collisionRadius)) return false;
                foreach (var other in units.Values)
                {
                    if (other.health <= 0 || other.hidden || other.pathingDisabled) continue;
                    double radius = other == changedHero ? heroProfile.collisionRadius : other.profile.collisionRadius;
                    if (Distance(row.position, other.position) < profile.collisionRadius + radius) return false;
                }
                foreach (var other in candidates)
                    if (Distance(row.position, other.position) < profile.collisionRadius + other.profile.collisionRadius) return false;
                candidates.Add(new Unit { id = row.entityId, owner = row.ownerSlot, sourceHeroEntityId = row.sourceHeroEntityId,
                    kind = OriginalWorldUnitKind.Summon, rawcode = row.rawcode, profile = profile, position = row.position,
                    destination = row.position, health = row.health, mana = row.mana, invulnerable = row.invulnerable });
            }
            // Navigation and the world are stable within this single host call.
            // No callbacks or further fallible placement queries follow here.
            if (changedHero != null)
            {
                if (changedHero.profile.collisionRadius != heroProfile.collisionRadius)
                {
                    if (!changedHero.hidden && !changedHero.pathingDisabled)
                        motion.SetSourceBody(changedHero.id, changedHero.position.x, changedHero.position.y, heroProfile.collisionRadius);
                    changedHero.pathRevision = -1;
                }
                changedHero.profile = heroProfile; changedHero.health = heroUpdate.health; changedHero.mana = heroUpdate.mana; revision++;
            }
            foreach (var candidate in candidates)
            {
                motion.SetSourceBody(candidate.id, candidate.position.x, candidate.position.y, candidate.profile.collisionRadius);
                units.Add(candidate.id, candidate); revision++;
                Emit(OriginalWorldEventKind.UnitAdded, entityId: candidate.id, rawcode: candidate.rawcode, health: candidate.health);
            }
            return true;
        }
    }
}

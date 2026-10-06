using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalIllusionSpawn
    {
        public int entityId;
        public OriginalWorldUnitProfile profile;
        public OriginalPoint position;
        public double health, mana;
        public bool hidden;
    }

    public sealed partial class OriginalWorld
    {
        // All inputs and mutual occupancy are checked before the first event.
        // The host owns selection of positions; this is not Warcraft's hidden
        // mirror-image placement algorithm.
        public bool TryPublishIllusions(int sourceHeroId, OriginalIllusionSpawn[] rows)
            => units.TryGetValue(sourceHeroId, out var source) &&
                TryPublishImages(sourceHeroId, source.owner, OriginalImageFactory.KnightMirror, rows);

        public bool TryPublishImages(int copySourceEntityId, int ownerSlot, OriginalImageFactory factory, OriginalIllusionSpawn[] rows)
        {
            if (rows == null || rows.Length < 1 || rows.Length > 32) throw new ArgumentException("Invalid illusion batch.");
            if (!units.TryGetValue(copySourceEntityId, out var source) || source.health <= .405) return false;
            bool canonical = source.kind == OriginalWorldUnitKind.Hero && source.owner >= 1 && source.id == source.owner;
            if (factory == OriginalImageFactory.KnightMirror)
            { if (!canonical || ownerSlot != source.owner) return false; }
            else if (factory == OriginalImageFactory.EnemyWand)
            { if (!canonical || ownerSlot != 0) return false; }
            else if (factory == OriginalImageFactory.BossMirror)
            { if (source.kind != OriginalWorldUnitKind.Enemy || source.rawcode != "n017" || ownerSlot != 0) return false; }
            else if (factory == OriginalImageFactory.ItemWand)
            { if (ownerSlot < 1 || ownerSlot > 8 || !units.TryGetValue(ownerSlot,out var caster) || caster.kind != OriginalWorldUnitKind.Hero ||
                (!canonical && (source.kind != OriginalWorldUnitKind.Enemy || source.owner != 0))) return false; }
            else return false;
            var ids = new HashSet<int>();
            var candidates = new List<Unit>(rows.Length);
            foreach (var row in rows)
            {
                if (row == null || row.entityId < 1000000000 || !ids.Add(row.entityId) || units.ContainsKey(row.entityId)) return false;
                Coordinates(row.position); ValidateProfile(row.profile);
                if (!Finite(row.health) || row.health <= 0 || row.health > row.profile.maxHealth ||
                    !Finite(row.mana) || row.mana < 0 || row.mana > row.profile.maxMana ||
                    !row.hidden && !Free(row.position, row.profile.collisionRadius)) return false;
                candidates.Add(new Unit { id = row.entityId, owner = ownerSlot, rawcode = source.rawcode, kind = OriginalWorldUnitKind.Illusion,
                    sourceHeroEntityId = canonical ? source.id : 0, copySourceEntityId = source.id, imageFactory = factory,
                    profile = row.profile.Copy(), position = row.position, destination = row.position, health = row.health, mana = row.mana, hidden = row.hidden });
            }
            for (int i = 0; i < rows.Length; i++)
                for (int j = i + 1; j < rows.Length; j++)
                    if (!rows[i].hidden && !rows[j].hidden && Distance(rows[i].position, rows[j].position) < rows[i].profile.collisionRadius + rows[j].profile.collisionRadius) return false;
            foreach (var candidate in candidates)
            {
                if (!candidate.hidden) motion.SetSourceBody(candidate.id, candidate.position.x, candidate.position.y, candidate.profile.collisionRadius);
                units.Add(candidate.id, candidate); revision++;
                Emit(OriginalWorldEventKind.UnitAdded, entityId: candidate.id, rawcode: candidate.rawcode, health: candidate.health);
            }
            return true;
        }

        public bool CanPlace(OriginalPoint point, double radius, int excludingEntity = 0)
        {
            Coordinates(point);
            if (!Finite(radius) || radius <= 0 || radius > 32768 || excludingEntity < 0) throw new ArgumentOutOfRangeException();
            return Free(point, radius, excludingEntity);
        }

        // Host-only source effects call this; no wire command exposes it.
        // Changing pathing never hides, pauses, heals or clears an actor's order.
        public bool SetPathingEnabled(int entityId, bool enabled)
        {
            if (!units.TryGetValue(entityId, out var unit)) return false;
            if (unit.pathingDisabled == !enabled) return true;
            if (enabled && unit.health > 0 && !unit.hidden)
                motion.SetSourceBody(entityId, unit.position.x, unit.position.y, unit.profile.collisionRadius);
            else motion.Remove(entityId);
            unit.pathingDisabled = !enabled; unit.pathRevision = -1; revision++; return true;
        }

        // A102/Ygv uses SetUnitX/Y even after an earlier failed bound check
        // re-enabled pathing. Dead-but-present actors are moved too. Keeping an
        // overlapping body is a source reconstruction, not a native solver claim.
        public bool ForcePosition(int entityId, OriginalPoint position)
        {
            Coordinates(position);
            if (!units.TryGetValue(entityId, out var unit)) return false;
            if (unit.health > 0 && !unit.hidden && !unit.pathingDisabled)
                motion.SetSourceBody(entityId, position.x, position.y, unit.profile.collisionRadius);
            unit.position = position; unit.pathRevision = -1; revision++; return true;
        }
    }
}

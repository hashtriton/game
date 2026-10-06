using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemNativeCombatAbility[] ItemNativeCombatAbilities(int actorId)
        {
            var actor = world?.UnitState(actorId);
            if (actor == null || actor.kind != OriginalWorldUnitKind.Hero || actor.ownerSlot < 1 ||
                actorId != OriginalWorld.HeroEntityId(actor.ownerSlot) || itemEffects == null)
                return Array.Empty<OriginalItemNativeCombatAbility>();
            var player = players.Find(x => x.slot == actor.ownerSlot);
            var rows = player?.inventory == null ? Array.Empty<OriginalItemNativeCombatAbility>() :
                itemEffects.CombatAbilities(player.inventory.Snapshot());
            foreach (var row in rows) row.rank = WaveTraitItemAbilityRank(actorId, row.abilityId, row.rank);
            foreach (var row in rows) row.rank = ItemScriptAbilityRank(actorId, row.abilityId, row.rank);
            foreach (var row in rows) row.rank = ItemAxeAbilityRank(actorId, row.abilityId, row.rank);
            return rows;
        }
    }
}

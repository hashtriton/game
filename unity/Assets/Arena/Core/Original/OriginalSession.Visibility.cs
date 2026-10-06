namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalWorldSnapshot WorldVisibilitySnapshot()
        {
            var snapshot = world?.Snapshot();
            if (snapshot == null) return null;
            foreach (var unit in snapshot.units)
            {
                unit.invisible = CombatInvisibilityActive(unit.entityId);
                unit.visibleToOwners = 0;
                for (int owner = 1; owner <= 8; owner++)
                    if (CanSeeForCombat(owner, unit)) unit.visibleToOwners |= 1 << (owner - 1);
            }
            return snapshot;
        }
    }
}

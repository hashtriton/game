using Arena.Original;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        public bool UnitVisible(OriginalWorldUnitView unit)
        {
            int owner = network ? network.LocalSlot : 0;
            return unit != null && !unit.hidden && owner >= 1 && owner <= 8 &&
                (unit.visibleToOwners & (1 << (owner - 1))) != 0;
        }
    }
}

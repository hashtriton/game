namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalSpellResistanceLedger nativeSpellResistance = new OriginalSpellResistanceLedger();

        void SetNativeSpellResistance(int actorId, string sourceKey, double reduction) =>
            nativeSpellResistance.Set(actorId, sourceKey, reduction);
        void RemoveNativeSpellResistance(int actorId, string sourceKey) =>
            nativeSpellResistance.Remove(actorId, sourceKey);
        double NativeSpellResistanceMultiplier(int actorId) => nativeSpellResistance.Multiplier(actorId);
    }
}

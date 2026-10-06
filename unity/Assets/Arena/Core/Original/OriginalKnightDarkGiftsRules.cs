using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public readonly struct OriginalDarkGiftCandidate
    {
        public readonly int entityId, ownerSlot;
        public readonly string rawcode;
        public readonly OriginalPoint position;
        public readonly bool hasAcidBuff, hasResistance;
        public OriginalDarkGiftCandidate(int id, int owner, string rawcode, OriginalPoint position, bool acid, bool resistance)
        { entityId = id; ownerSlot = owner; this.rawcode = rawcode; this.position = position; hasAcidBuff = acid; hasResistance = resistance; }
    }
    public enum OriginalDarkGiftInstructionKind { CastAcidBomb, AddResistance, RemoveResistance, RemoveAcidBuff }
    public readonly struct OriginalDarkGiftInstruction
    {
        public readonly int entityId;
        public readonly OriginalDarkGiftInstructionKind kind;
        public OriginalDarkGiftInstruction(int id, OriginalDarkGiftInstructionKind kind) { entityId = id; this.kind = kind; }
    }

    // YDv/Ydv72893/72839: the .5 timer examines remaining time before
    // decrementing. The final removal is at15.5/18/20.5 seconds. No life,
    // HERO-type or visibility filter is present; same-owner H008 images and
    // retained corpses are candidates. A native cast instruction is not proof
    // that ANab reached its SPELL_EFFECT or applied B02W.
    public sealed class OriginalKnightDarkGiftsRules
    {
        public const double Period = .5;
        public readonly int OwnerSlot, Rank;
        public bool Completed { get; private set; }
        int remainingTicks;
        public OriginalKnightDarkGiftsRules(int ownerSlot, int rank)
        {
            if (ownerSlot < 1 || ownerSlot > 8 || rank < 1 || rank > 3) throw new ArgumentOutOfRangeException();
            OwnerSlot = ownerSlot; Rank = rank; remainingTicks = 25 + 5 * rank;
        }
        public OriginalDarkGiftInstruction[] Tick(IReadOnlyList<OriginalDarkGiftCandidate> candidates)
        {
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            var ids = new HashSet<int>();
            foreach (var row in candidates)
                if (row.entityId <= 0 || !ids.Add(row.entityId) || !OriginalCombatDefinition.IsFinite(row.position.x) || !OriginalCombatDefinition.IsFinite(row.position.y))
                    throw new ArgumentException("Invalid or duplicate actor.");
            if (Completed) return Array.Empty<OriginalDarkGiftInstruction>();
            bool expire = remainingTicks == 0;
            if (!expire) remainingTicks--; else Completed = true;
            var result = new List<OriginalDarkGiftInstruction>();
            foreach (var actor in candidates)
            {
                if (actor.ownerSlot != OwnerSlot || actor.rawcode != "H008" || actor.position.x < -2560 || actor.position.x > 2432 ||
                    actor.position.y < -4096 || actor.position.y > 3456) continue;
                if (expire)
                {
                    result.Add(new OriginalDarkGiftInstruction(actor.entityId, OriginalDarkGiftInstructionKind.RemoveResistance));
                    result.Add(new OriginalDarkGiftInstruction(actor.entityId, OriginalDarkGiftInstructionKind.RemoveAcidBuff));
                }
                else
                {
                    if (!actor.hasAcidBuff) result.Add(new OriginalDarkGiftInstruction(actor.entityId, OriginalDarkGiftInstructionKind.CastAcidBomb));
                    if (!actor.hasResistance) result.Add(new OriginalDarkGiftInstruction(actor.entityId, OriginalDarkGiftInstructionKind.AddResistance));
                }
            }
            return result.ToArray();
        }
    }
}

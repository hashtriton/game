using System;

namespace Arena.Original
{
    // Map A166/A168/A165, native metadata Nab3/Nsi2/Nsi4, ARCHH2 exact
    // cache e580c4a53e2a87d018d8850c79d8fbd25b905ac556e6cfd2790c64b8fe1e8a4e.
    // Native r1/r3 corroborate duration/movement. Armor/miss and r2 are
    // declarations, not statistics inferred from the short runtime sample.
    public sealed class OriginalArcherDebuffRules
    {
        public readonly string abilityId, buffId;
        public readonly double duration, heroDuration, area, movementSlow, attackSlow, armorReduction, missChance;
        public readonly bool projectile;
        public OriginalArcherDebuffRules(OriginalCombatCatalog combat, OriginalNativeCatalog native, string id, int rank)
        {
            if (combat == null || native == null) throw new ArgumentNullException();
            if (combat.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected source map.");
            if (rank < 1 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            bool itemFrost = id == "A062" || id == "A0BJ";
            if (id != "A166" && id != "A168" && id != "A165" && id != "A09S" && !itemFrost) throw new ArgumentException("Unknown native debuff ability.");
            if ((id == "A09S" || itemFrost) && rank != 1) throw new ArgumentOutOfRangeException(nameof(rank));
            abilityId = id; var a = combat.Ability(id);
            string code = itemFrost ? "AIob" : id == "A166" ? "ANab" : id == "A168" || id == "A09S" ? "AUfn" : "ANdh";
            if (a.Text("code") != code) throw new InvalidOperationException("Archer native family changed.");
            buffId = a.Text("BuffID" + rank);
            duration = a.Number("Dur" + rank); heroDuration = a.Number("HeroDur" + rank);
            if (itemFrost)
            {
                // ITEMORB2 native I01N/I03N: B00X/B00U, 3s/1s;
                // map frost movement override .4, native attack decrease .25.
                // I03N's separate A0ME aura contributes -.1/-.15 itself.
                if (buffId != (id == "A062" ? "B00X" : "B00U") || duration != 3 || heroDuration != 1 ||
                    a.Number("DataA1") != (id == "A062" ? 30 : 60))
                    throw new InvalidOperationException("Item frost declaration changed.");
                movementSlow = native.Constant("FrostMoveSpeedDecrease").Require();
                attackSlow = native.Constant("FrostAttackSpeedDecrease").Require();
                if (movementSlow != .4 || attackSlow != .25) throw new InvalidOperationException("Item frost constants changed.");
                // This is a weapon debuff only. No AUfn area, dummy zero
                // callbacks or second application of intrinsic DataA damage.
            }
            else if (id == "A166")
            {
                if (buffId != "B0AA" || duration != 5 || heroDuration != 5) throw new InvalidOperationException("Acid duration differs from observations.");
                armorReduction = a.Number("DataC" + rank);
                if (armorReduction != rank * 5) throw new InvalidOperationException("Acid armor declaration changed.");
                Neutral(a, "DataA" + rank); Neutral(a, "DataB" + rank);
                Neutral(a, "DataD" + rank); Neutral(a, "DataE" + rank);
                projectile = true;
            }
            else if (id == "A168" || id == "A09S")
            {
                double expectedDuration = id == "A09S" ? 6 : 3;
                double expectedHeroDuration = id == "A09S" ? 6 : 1.5;
                if (buffId != "Bfro" || duration != expectedDuration || heroDuration != expectedHeroDuration) throw new InvalidOperationException("Frost duration differs from source.");
                area = a.Number("Area" + rank); movementSlow = native.Constant("FrostMoveSpeedDecrease").Require();
                attackSlow = native.Constant("FrostAttackSpeedDecrease").Require();
                if (area != 200 || movementSlow != .4 || attackSlow != .25) throw new InvalidOperationException("Frost constants changed.");
                if (id == "A09S")
                {
                    // Ordinary nova resolves its declared damage separately.
                    // Only Bfro shares this cache; ARCHH2 helper zero events
                    // are not promoted to the damaging nova's event pattern.
                    if (a.Number("DataA1") != 400 || a.Number("DataB1") != 400) throw new InvalidOperationException("Nova damage declaration changed.");
                }
                else { Neutral(a, "DataA" + rank); Neutral(a, "DataB" + rank); }
            }
            else
            {
                if (buffId != "B0A9" || duration != 4 || heroDuration != 4) throw new InvalidOperationException("Haze duration differs from observations.");
                attackSlow = a.Number("DataD" + rank); missChance = a.Number("DataB" + rank);
                if (Math.Abs(attackSlow - (.2 + .15 * rank)) > 1e-12 || attackSlow != missChance)
                    throw new InvalidOperationException("Haze declaration changed.");
                Neutral(a, "DataC" + rank);
                // DataA is the disabled-spell-class mask, not damage. This
                // experiment did not make the target cast while hazed, so an
                // absent mask is not promoted to a measured silence policy.
                projectile = true;
            }
        }
        static void Neutral(OriginalCombatDefinition ability, string key)
        {
            // Missing fields resolve only for these exact native families and
            // observed endpoints. Rank2 continuity is a declared derived port
            // rule; no generic empty SLK numeric default is introduced.
            if (ability.TryNumber(key, out double value, out var evidence))
            { if (value != 0) throw new InvalidOperationException("Native neutral field changed: " + key); }
            else if (evidence != null) throw new InvalidOperationException("Native neutral field conflicts: " + key);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // Ekv31491..31628, called by ordinary EMv31773 and ETv31876.
        // This changes only a spawned instance; later source RemoveAbility
        // remains authoritative over the original declaration and this overlay.
        void ApplyEnemyDifficultyAbilities(OriginalWorldUnitView actor)
        {
            if (actor == null || actor.kind != OriginalWorldUnitKind.Enemy ||
                options.difficulty != OriginalDifficulty.Extreme && options.difficulty != OriginalDifficulty.Nightmare) return;
            string[] added = Array.Empty<string>(), removed = Array.Empty<string>();
            switch (actor.rawcode)
            {
                case "n008":
                    added = new[] { "A0TD" };
                    removed = new[] { "A0TC" };
                    break;
                case "n009":
                    added = new[] { "A0TD", "A047" };
                    removed = new[] { "A0TC", "A046" };
                    break;
                case "n00E":
                    added = new[] { "A0GO", "A0RB", "A0GN" };
                    removed = new[] { "A0G5", "A0RA" };
                    break;
                case "n00G":
                    added = new[] { "A068" };
                    removed = new[] { "A04E" };
                    break;
                case "n00I":
                    added = new[] { "A0QX" };
                    removed = new[] { "A0QV" };
                    break;
                case "n00J":
                    added = new[] { "A0G6" };
                    break;
                case "n00L":
                    added = new[] { "A0RY" };
                    removed = new[] { "A0RX" };
                    break;
                case "n00M":
                    added = new[] { "A0RY", "A05Z" };
                    removed = new[] { "A0RX" };
                    break;
                case "n00N":
                    added = new[] { "A0QZ" };
                    removed = new[] { "A0VP" };
                    break;
                case "n00O":
                    added = new[] { "A0QZ", "A0GQ" };
                    removed = new[] { "A0VP", "A06V" };
                    break;
                case "n00P":
                case "n02T":
                    added = new[] { "A0R0", "A077" };
                    break;
                case "n00V":
                    added = new[] { "A0G7" };
                    break;
                case "n015":
                    added = new[] { "A0QY", "A0RR" };
                    removed = new[] { "A05Y" };
                    break;
                case "n016":
                    added = new[] { "A0RT" };
                    removed = new[] { "A0RS" };
                    break;
                case "n019":
                    added = new[] { "A0R2" };
                    removed = new[] { "A0R1" };
                    break;
                case "n01B":
                    added = new[] { "A077" };
                    break;
                case "n01C":
                    added = new[] { "A0H0" };
                    removed = new[] { "A079" };
                    break;
                case "n01D":
                    added = new[] { "A0R5" };
                    removed = new[] { "A07A" };
                    break;
                case "n01E":
                    added = new[] { "A0R4" };
                    removed = new[] { "A03E" };
                    break;
                case "n027":
                    added = new[] { "A069" };
                    removed = new[] { "A0AN", "A0RV" };
                    break;
                case "n028":
                    added = new[] { "A0H1", "A069" };
                    removed = new[] { "ACsw", "A0AN" };
                    break;
                case "n029":
                    added = new[] { "A0H3" };
                    removed = new[] { "ACpu" };
                    break;
                case "n02B":
                    added = new[] { "A0RH", "A0H4" };
                    removed = new[] { "A09A", "A0AY" };
                    break;
                case "n02C":
                    added = new[] { "A0RH", "A0H5" };
                    removed = new[] { "A09A", "A0AZ" };
                    break;
                case "n02D":
                    added = new[] { "A0H7", "A077" };
                    removed = new[] { "A0B0" };
                    break;
                case "n01U":
                    added = new[] { "A0H6" };
                    removed = new[] { "A0B1" };
                    break;
                case "n023":
                    added = new[] { "A077" };
                    break;
            }
            var additions = new List<string>(added);
            // effective1.26 common.j707: ConvertUnitType(20) is TAUREN.
            if (CasterHasType(actor, "tauren") && !additions.Contains("A077")) additions.Add("A077");
            if (additions.Count != 0 || removed.Length != 0) ApplyUnitAbilityOverlay(actor.entityId, additions.ToArray(), removed);
        }

        void CaptureNativeBrawler(List<NativeWeaponProc> result, OriginalCombatDefinition ability)
        {
            if (ability.Text("code") != "ANdb") return;
            // Effective AbilityMetaData Ocr1/Ocr2/Ocr4 explicitly maps ANdb
            // to critical chance, multiplier and evasion. Transfer to measured
            // AOcr release stages; uniform RNG/other-proc composition is host policy.
            if (!ability.TryNumber("DataA1", out double chance, out _) || !ability.TryNumber("DataB1", out double factor, out _) ||
                chance < 0 || chance > 100 || factor < 0) return;
            if (chance == 100 || chance > 0 && RollWeapon(1000000) - 1 < chance * 10000)
                result.Add(new NativeWeaponProc { ability = ability.id, sourceKey = ability.id, multiplier = factor, targetsAir = true });
        }
    }
}

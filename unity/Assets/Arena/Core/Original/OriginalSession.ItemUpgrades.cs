using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // bCv38429..84 and HeroAbil H024->A0SU87093. LiAPyUp2 measures
        // ranks0..3, duplicate retention, reverse transfer and native learning.
        // Canonical SP/SM remain the public learning identities; ANeg owns
        // the runtime aliases, never a second skill-point or rank counter.
        SortedDictionary<string, int> EquipmentAuxiliaries(Player player, OriginalInventory inventory,
            OriginalHeroProgression progression, SortedDictionary<string, int> source = null)
        {
            var result = new SortedDictionary<string, int>(source ?? player.auxiliaryAbilities, StringComparer.Ordinal);
            if (player.hero != "H024") return result;
            result.Remove("A0SU"); result.Remove("A0SR"); result.Remove("A0SS");
            if (inventory == null || !Array.Exists(inventory.HeroSlots, i => i != null && i.itemId == "I00Z")) return result;
            if (combatCatalog.Ability("A0SU").Text("code") != "ANeg") throw new InvalidOperationException("Pyro item upgrade declaration changed.");
            result.Add("A0SU", 1);
            if (progression != null)
                foreach (var skill in progression.Snapshot().skills)
                    if (skill.rank > 0 && (skill.id == "A0SP" || skill.id == "A0SM"))
                        result.Add(skill.id == "A0SP" ? "A0SR" : "A0SS", skill.rank);
            return result;
        }
        static string CanonicalPyroAbility(string id) => id == "A0SR" ? "A0SP" : id == "A0SS" ? "A0SM" : id;
        static string ActualPyroAbility(Player player, string id) =>
            player.hero == "H024" && player.auxiliaryAbilities.ContainsKey("A0SU") ?
                (id == "A0SP" ? "A0SR" : id == "A0SM" ? "A0SS" : id) : id;
    }
}

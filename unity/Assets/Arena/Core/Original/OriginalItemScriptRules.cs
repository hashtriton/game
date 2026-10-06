using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalItemScriptRule
    {
        public string itemId, abilityId, cooldownGroup;
        public double cooldown, manaCost;
        public bool retired;
    }

    // Own implementation of3.9c uq/Yq6715..6790 and hU..lU11427..11546.
    // The exact map identity is shared with the validated ITEMACT2 catalog.
    // Native activation/cost are observed; script effects are static-path.
    public sealed class OriginalItemScriptRules
    {
        readonly Dictionary<string, OriginalItemScriptRule> rules = new Dictionary<string, OriginalItemScriptRule>();
        static readonly string[][] Parts = {
            new[]{"I07P","I02N","I034"}, new[]{"I07C","I017","I03G"}, new[]{"I03Z","I02E","I035"},
            new[]{"I07N","I01U","I03A"}, new[]{"I07M","I01B","I039"}, new[]{"I00V","I00T","I03C"},
            new[]{"I088","I085","I07Z"}, new[]{"I08U","I06J","I071"}, new[]{"I05Q","I072","I073"},
            new[]{"I03Y","I026","I030"}, new[]{"I096","I045","I043"} };

        public OriginalItemScriptRules(OriginalItemCatalog items, OriginalCombatCatalog combat, OriginalObservedItemActives observed)
        {
            if (observed == null) return;
            // Reuse the existing full provenance/schema validation.
            new OriginalItemActiveRules(items, combat, observed);
            foreach (var id in new[]{"I0B7", "I0AE"})
            {
                var row = Array.Find(observed.items, r => r.itemId == id);
                string abilityId = id == "I0B7" ? "A1E0" : "A0WW";
                double cooldown = id == "I0B7" ? 1 : 120, mana = id == "I0B7" ? 0 : 500;
                var ability = combat.Ability(abilityId);
                if (row == null || row.abilityId != abilityId || !row.nativeUseObserved || !row.orderAccepted ||
                    row.effectSeconds != 0 || row.manaCost != mana || row.retired != (id == "I0B7") ||
                    ability == null || !ability.TryNumber("Cool1", out double actual, out _) || actual != cooldown)
                    throw new InvalidOperationException("Script item activation differs: " + id);
                rules.Add(id, new OriginalItemScriptRule { itemId=id, abilityId=abilityId, cooldownGroup=abilityId,
                    cooldown=cooldown, manaCost=mana, retired=row.retired });
            }
        }
        public OriginalItemScriptRule Rule(string id) => id != null && rules.TryGetValue(id,out var rule) ?
            new OriginalItemScriptRule { itemId=rule.itemId, abilityId=rule.abilityId, cooldownGroup=rule.cooldownGroup,
                cooldown=rule.cooldown, manaCost=rule.manaCost, retired=rule.retired } : null;
        public static string[] Disassembly(string id)
        {
            foreach(var row in Parts) if(row[0]==id) return new[]{row[1],row[2]};
            return null;
        }
    }
}

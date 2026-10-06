using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed class OriginalItemChannelRule
    {
        public string itemId, abilityId, cooldownGroup, targets, nativeCode;
        public double manaCost, cooldown, range;
        public OriginalAbilityTargetMode targetMode;
    }

    // ANcl supplies activation and targeting, not the effect. Each rule below
    // dispatches its own source handler. Instant item activation transfers from
    // ITEMACT2; it is not an independent measurement of every item here.
    public sealed class OriginalItemChannelRules
    {
        readonly Dictionary<string, OriginalItemChannelRule> rules = new Dictionary<string, OriginalItemChannelRule>();
        public OriginalItemChannelRules(OriginalItemCatalog items, OriginalCombatCatalog combat)
        {
            if (items.mapSha256 != OriginalNativeCatalog.ExpectedMapSha256 || combat.sourceSha256 != items.mapSha256)
                throw new ArgumentException("Item channel source differs.");
            Add("I00T", "A12Y", 18, 150, 1000, OriginalAbilityTargetMode.Unit);
            Add("I00V", "A12Z", 18, 300, 1000, OriginalAbilityTargetMode.Unit);
            Add("I04D", "A0CC", 24, 175, 0, OriginalAbilityTargetMode.None);
            Add("I08I", "A17V", 20, 200, 700, OriginalAbilityTargetMode.Point);
            Add("I05A", "A0TX", 12, 0, 700, OriginalAbilityTargetMode.Unit);
            Add("I045", "A0C5", 20, 300, 700, OriginalAbilityTargetMode.Point);
            Add("I096", "A0C5", 20, 300, 700, OriginalAbilityTargetMode.Point);
            Add("I08Q", "A0M9", 30, 200, 1000, OriginalAbilityTargetMode.Point);
            Add("I09P", "A104", 20, 100, 425, OriginalAbilityTargetMode.Unit);
            Add("I05P", "A11S", 14, 120, 600, OriginalAbilityTargetMode.Point);
            Add("I060", "A0KP", 13, 225, 900, OriginalAbilityTargetMode.Unit);
            Add("I06J", "A0FI", 20, 70, 700, OriginalAbilityTargetMode.Unit,"Aste");
            Add("I08U", "A0NA", 15, 90, 700, OriginalAbilityTargetMode.Unit,"Aste");
            Add("I07P", "A0JE", 24, 80, 600, OriginalAbilityTargetMode.Unit,"Auhf");
            Add("I08D", "A0JE", 24, 80, 600, OriginalAbilityTargetMode.Unit,"Auhf");
            Add("I06R", "A0OU", 20, 80, 0, OriginalAbilityTargetMode.None,"Aroa");
            Add("I090", "A0OS", 20, 120, 0, OriginalAbilityTargetMode.None,"Aroa");
            Add("I05D", "A0YK", 12, 100, 0, OriginalAbilityTargetMode.None,"AIha");
            // ITEMCHAIN1 physical use: A028100MP/no native hit; A08A400MP
            // and one native zero callback. Both have synchronous5-stage use.
            Add("I015", "A028", 12, 100, 700, OriginalAbilityTargetMode.Unit,"AOcl");
            Add("I07Y", "A028", 12, 100, 700, OriginalAbilityTargetMode.Unit,"AOcl");
            Add("I02C", "A08A", 18, 400, 700, OriginalAbilityTargetMode.Point,"AOsh");
            void Add(string id, string abilityId, double cooldown, double mana, double range, OriginalAbilityTargetMode mode,string code="ANcl")
            {
                var item = items.Item(id); var ability = combat.Ability(abilityId);
                if (item == null || Array.IndexOf(item.abilityIds, abilityId) < 0 || item.cooldownId != abilityId ||
                    ability == null || ability.Text("code") != code || ability.Number("Cool1") != cooldown ||
                    mode != OriginalAbilityTargetMode.None && ability.Number("Rng1") != range)
                    throw new InvalidOperationException("Item channel identity differs: " + id);
                // ANcl native1.26 War3Patch AbilityData.slk:13330 has Cost1=0.
                // Only A0TX inherits it; explicit map overrides must agree.
                bool cost = Array.Exists(ability.fields, f => f.key == "Cost1") ||
                    Array.Exists(ability.overrides, f => f.field == "amcs" && f.level == 1);
                if (cost && ability.Number("Cost1") != mana || !cost && mana != 0)
                    throw new InvalidOperationException("Item channel cost differs: " + id);
                rules.Add(id, new OriginalItemChannelRule { itemId = id, abilityId = abilityId,
                    cooldownGroup = item.cooldownId, manaCost = mana, cooldown = cooldown, range = range,
                    targetMode = mode, targets = ability.Text("targs1"),nativeCode=code });
            }
        }
        public OriginalItemChannelRule Rule(string id) => id != null && rules.TryGetValue(id, out var rule) ? rule : null;
    }
}

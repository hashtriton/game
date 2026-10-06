using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalItemScriptReference
    {
        public string functionName;
        public int line;
        public int endLine;
    }

    [Serializable]
    public sealed class OriginalItemEffectDefinition
    {
        public string id;
        public string[] abilityIds = Array.Empty<string>();
        public bool cmBonusRegistered;
        public double cmBonus;
        public int cmSourceLine;
        public bool scriptEffectsImplemented;
        public OriginalItemScriptReference[] scriptReferences = Array.Empty<OriginalItemScriptReference>();
    }

    public sealed class OriginalEquippedItemEffects
    {
        public long ItemInstanceId;
        public string ItemId;
        public int Slot;
        public string[] DirectAbilities;
        public OriginalItemScriptReference[] ScriptReferences;
    }

    public sealed class OriginalCmItemContribution
    {
        public long ItemInstanceId;
        public string ItemId;
        public double Value;
        public int SourceLine;
        // Cm treats values <= 3 as additions to its multiplier. These are
        // spell-proxy contributions, never native weapon damage modifiers.
        public bool AddsToMultiplier => Value <= 3;
    }

    public sealed class OriginalInventoryEffectPlan
    {
        public OriginalEquippedItemEffects[] Items;
        public OriginalStatModifier[] DeclaredModifiers;
        public OriginalCmItemContribution[] CmContributions;
        public OriginalItemEffectGap[] Gaps;
        public int ServantItemCount;
        public OriginalNativeItemProfile NativeProfile;
        public bool CanApplyProfile => NativeProfile != null && NativeProfile.known;
        public bool EffectsComplete => Gaps.Length == 0;
    }

    public sealed class OriginalNativeItemProfile
    {
        public bool known;
        public double strength, agility, intelligence, maxHealthFlat, maxManaFlat, attackDamage, armor;
        public double attackSpeedFraction, healthRegenPerSecond, manaRegenBaseFraction;
        public double moveSpeedFlat;
        public bool mixedOrMoreThanTwoIsDerived;
        public string[] evidence = Array.Empty<string>();
        public OriginalNativeItemProfile Require()
        {
            if (!known) throw new InvalidOperationException("Native item profile includes an unresolved item or rank.");
            return this;
        }
        internal void Add(OriginalNativeItemProfile source)
        {
            strength += source.strength; agility += source.agility; intelligence += source.intelligence;
            maxHealthFlat += source.maxHealthFlat; maxManaFlat += source.maxManaFlat; attackDamage += source.attackDamage;
            armor += source.armor; attackSpeedFraction += source.attackSpeedFraction;
            healthRegenPerSecond += source.healthRegenPerSecond; manaRegenBaseFraction += source.manaRegenBaseFraction;
            moveSpeedFlat = Math.Max(moveSpeedFlat, source.moveSpeedFlat);
        }
    }

    // A pure plan for an authoritative inventory candidate. Declared modifiers
    // remain separate from retail application/stacking and script handlers.
    // No charge, resource, HP, world or inventory mutation occurs here.
    public sealed partial class OriginalInventoryEffects
    {
        private const string MapSha256 = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34";
        private readonly Dictionary<string, OriginalItemEffectDefinition> items = new Dictionary<string, OriginalItemEffectDefinition>(StringComparer.Ordinal);
        private readonly OriginalItemPassiveEvaluator evaluator;
        private readonly Dictionary<string, OriginalNativeItemProfile> nativeProfiles = new Dictionary<string, OriginalNativeItemProfile>(StringComparer.Ordinal);
        private readonly HashSet<string> measuredConsumables = new HashSet<string>(StringComparer.Ordinal);
        private readonly OriginalEquipResolver equip;
        public int ItemCount => items.Count;

        public OriginalInventoryEffects(OriginalItemPassiveCatalog source, OriginalItemUseRules consumables = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.schemaVersion != 1 || source.mapSha256 != MapSha256 || source.items == null || source.abilities == null)
                throw new ArgumentException("Invalid item effect catalog identity.");
            var frozen = Freeze(source);
            foreach (var ability in frozen.abilities) nativeAbilityDefinitions.Add(ability.id, ability);
            var abilityIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var ability in frozen.abilities)
                if (!Rawcode(ability.id) || !abilityIds.Add(ability.id)) throw new ArgumentException("Duplicate or invalid passive ability.");
            foreach (var item in frozen.items)
            {
                if (!Rawcode(item.id) || items.ContainsKey(item.id) || !Finite(item.cmBonus) ||
                    (item.cmBonusRegistered && item.cmSourceLine <= 0)) throw new ArgumentException("Invalid item effect definition.");
                foreach (var ability in item.abilityIds)
                    if (!abilityIds.Contains(ability)) throw new ArgumentException("Unknown direct item ability.");
                foreach (var reference in item.scriptReferences)
                    if (reference == null || string.IsNullOrEmpty(reference.functionName) || reference.line <= 0 || reference.endLine < reference.line)
                        throw new ArgumentException("Invalid item script reference.");
                items.Add(item.id, item);
            }
            evaluator = new OriginalItemPassiveEvaluator(frozen);
            if (source.observedEquip != null)
            {
                equip = new OriginalEquipResolver(source.observedEquip);
                foreach (var item in items.Values)
                    if (equip.TryGet(item.id, out var measurement)) nativeProfiles.Add(item.id, Profile(item, measurement, frozen));
            }
            RegisterAdditionalEquip(source.observedAdditionalEquip, frozen);
            if (consumables != null)
                foreach (var id in new[] { "I03L", "I03M", "I022", "I023" })
                {
                    var rule = consumables.Rule(id);
                    if (rule != null && rule.known) RegisterConsumableProfile(items[id], rule, frozen);
                }
        }

        void RegisterConsumableProfile(OriginalItemEffectDefinition item, OriginalConsumableRule rule, OriginalItemPassiveCatalog source)
        {
            var states = new List<OriginalItemAbilityState>();
            foreach (var id in item.abilityIds)
            {
                var ability = Array.Find(source.abilities, row => row.id == id);
                if (ability == null || !ability.levelsKnown || ability.declaredLevels != 1)
                    throw new ArgumentException("Consumable has an unresolved native rank.");
                if (id == rule.abilityId)
                {
                    if (ability.baseCode != (rule.resource == "health" ? "AIhe" : "AIma"))
                        throw new ArgumentException("Consumable native family differs from measured use.");
                    continue;
                }
                // LiAItemUse1 records unchanged attributes and HP/MP maxima.
                // The only extra source abilities are the one-level Arll/AIrn
                // ordinary regeneration families. Surviving-copy slopes also
                // confirm +10HP/s and +.5 natural MP regeneration per copy.
                if (id != "Arll" && id != "AIrn" || ability.baseCode != (id == "Arll" ? "Arel" : "AIrm"))
                    throw new ArgumentException("Unmeasured additional consumable ability.");
                states.Add(new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = id, Level = 1 });
            }
            var evaluation = evaluator.Evaluate(states);
            if (evaluation.Gaps.Count != 0) throw new ArgumentException("Consumable passive parameters remain unresolved.");
            var profile = new OriginalNativeItemProfile { known = true };
            foreach (var modifier in evaluation.Modifiers)
            {
                if (modifier.Operation != "add") throw new ArgumentException("Unsupported consumable passive operation.");
                if (modifier.Stat == "healthRegenPerSecond") profile.healthRegenPerSecond += modifier.Value;
                else if (modifier.Stat == "manaRegenBaseFraction") profile.manaRegenBaseFraction += modifier.Value;
                else throw new ArgumentException("Unmeasured consumable passive stat.");
            }
            if (!nativeProfiles.ContainsKey(item.id))
            { nativeProfiles.Add(item.id, profile); SaveVitalitySteps(item, evaluation.Modifiers); }
            measuredConsumables.Add(item.id);
        }

        // The resolver belongs to the host's verified native-state adapter.
        // Unknown levels return null; zero is preserved as an invalid native
        // rank and must not be converted to one by this planner.
        public OriginalInventoryEffectPlan Plan(OriginalInventorySnapshot snapshot,
            Func<long, string, int?> abilityLevel = null)
        {
            if (snapshot == null || snapshot.ownerId < 1 || snapshot.ownerId > 8 || snapshot.gold < 0 || snapshot.lumber < 0)
                throw new ArgumentException("Invalid inventory snapshot.");
            var identities = new HashSet<long>();
            var hero = ValidateSlots(snapshot.heroSlots, identities);
            var servant = ValidateSlots(snapshot.servantSlots, identities);
            var states = new List<OriginalItemAbilityState>();
            var equipped = new List<OriginalEquippedItemEffects>();
            var cm = new List<OriginalCmItemContribution>();
            var gaps = new List<OriginalItemEffectGap>();
            var profile = new OriginalNativeItemProfile { known = true };
            var equippedIds = new HashSet<string>(StringComparer.Ordinal);
            for (var slot = 0; slot < hero.Length; slot++)
            {
                var instance = hero[slot];
                if (instance == null) continue;
                var item = items[instance.itemId];
                equippedIds.Add(item.id);
                bool ranksResolved = true;
                OriginalEquipResolver.Measurement measurement = null;
                equip?.TryGet(item.id, out measurement);
                if (measurement == null) additionalEquip.TryGetValue(item.id, out measurement);
                equipped.Add(new OriginalEquippedItemEffects
                {
                    ItemInstanceId = instance.instanceId, ItemId = item.id, Slot = slot,
                    DirectAbilities = Strings(item.abilityIds), ScriptReferences = References(item.scriptReferences)
                });
                var direct = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var id in item.abilityIds)
                {
                    direct.TryGetValue(id, out var occurrence);
                    direct[id] = occurrence + 1;
                    // I050 declares A00T twice. LiAEquip1 observes +36 STR,
                    // so each declaration is a separate conditional contribution.
                    var rank = abilityLevel != null ? abilityLevel(instance.instanceId, id) :
                        measurement != null ? (int?)measurement.ranks[Array.IndexOf(measurement.abilities, id)] :
                        measuredConsumables.Contains(item.id) ? (int?)1 : null;
                    int expectedRank = measurement == null ? 1 : measurement.ranks[Array.IndexOf(measurement.abilities, id)];
                    if (!rank.HasValue || rank.Value != expectedRank) ranksResolved = false;
                    if (rank.HasValue) states.Add(new OriginalItemAbilityState { ItemInstanceId = instance.instanceId, AbilityId = id, Level = rank.Value, Occurrence = occurrence });
                    else Gap(gaps, instance.instanceId, id, "native-current-level-unresolved");
                }
                // A spellbook's measured intrinsic profile depends on every
                // queried child rank, not only its container's current level.
                if (abilityLevel != null && measurement != null)
                    for (int q = 0; q < measurement.abilities.Length; q++)
                    {
                        var rank = abilityLevel(instance.instanceId, measurement.abilities[q]);
                        if (!rank.HasValue || rank.Value != measurement.ranks[q]) ranksResolved = false;
                    }
                if (item.cmBonusRegistered)
                    cm.Add(new OriginalCmItemContribution { ItemInstanceId = instance.instanceId, ItemId = item.id, Value = item.cmBonus, SourceLine = item.cmSourceLine });
                // No direct rawcode reference is not proof that generic pickup,
                // use, damage or lifecycle handlers leave this item unchanged.
                if (!item.scriptEffectsImplemented) Gap(gaps, instance.instanceId, null, "item-script-lifecycle-unimplemented");
                if (ranksResolved && nativeProfiles.TryGetValue(item.id, out var itemProfile)) profile.Add(itemProfile);
                else profile.known = false;
            }
            var result = evaluator.Evaluate(states, abilityLevel);
            foreach (var gap in result.Gaps) gaps.Add(gap);
            foreach (var modifier in result.Modifiers)
            {
                modifier.Sources = Strings(modifier.Sources);
                // Runtime flags on the declaration catalog are not per-effect
                // retail evidence. A future observed adapter must resolve each
                // concrete combination before the host changes a profile.
                var instance = Array.Find(hero, candidate => candidate != null && candidate.instanceId == modifier.ItemInstanceId);
                if (instance == null || !nativeProfiles.ContainsKey(instance.itemId) || modifier.Level != 1)
                    Gap(gaps, modifier.ItemInstanceId, modifier.AbilityId, "native-application-and-stacking-unresolved", modifier.Level);
            }
            var servantCount = 0;
            foreach (var item in servant) if (item != null) servantCount++;
            profile.mixedOrMoreThanTwoIsDerived = equipped.Count > 2 || equippedIds.Count > 1;
            profile.evidence = new[] {
                "native declaration:AIat/AIde/AIab/AIml/AImm/Arel/AIas/AIrm; actual levels and 0/1/2/1/0 attributes/maxima:LiAEquip1.w3v",
                "https://classic.battle.net/war3/basics/heroitemspermanent.shtml",
                "https://classic.battle.net/war3/basics/heroes.shtml",
                "Mixed sets and >2 copies use additive ordinary-family rules; not claimed as directly measured.",
                "Current HP/MP change policy, active use and scripted item lifecycle are separate from these bonuses.",
                "Four potion profiles: LiAItemUse1 active rank1 and unchanged attributes/maxima; Arll/AIrn source-level1 regeneration corroborated by surviving copies."
            };
            var evidence = new List<string>(profile.evidence);
            foreach (var item in equipped)
                if (additionalProfileEvidence.TryGetValue(item.ItemId, out var observedSource) && !evidence.Contains(observedSource))
                    evidence.Add(observedSource);
            profile.evidence = evidence.ToArray();
            return new OriginalInventoryEffectPlan
            {
                Items = equipped.ToArray(), DeclaredModifiers = result.Modifiers.ToArray(),
                CmContributions = cm.ToArray(), Gaps = gaps.ToArray(), ServantItemCount = servantCount, NativeProfile = profile
            };
        }

        OriginalNativeItemProfile Profile(OriginalItemEffectDefinition item, OriginalEquipResolver.Measurement measurement, OriginalItemPassiveCatalog frozen, bool additional = false)
        {
            if (item.abilityIds.Length != measurement.abilities.Length) throw new ArgumentException("Equip ability list differs from source.");
            var states = new List<OriginalItemAbilityState>();
            var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < item.abilityIds.Length; i++)
            {
                var id = item.abilityIds[i];
                if (id != measurement.abilities[i]) throw new ArgumentException("Equip ability order differs from source.");
                occurrences.TryGetValue(id, out var occurrence); occurrences[id] = occurrence + 1;
                states.Add(new OriginalItemAbilityState { ItemInstanceId = 1, AbilityId = id, Level = measurement.ranks[i], Occurrence = occurrence });
            }
            var evaluation = evaluator.Evaluate(states);
            foreach (var gap in evaluation.Gaps)
            {
                var ability = Array.Find(frozen.abilities, a => a.id == gap.AbilityId);
                // ITEMSTAT2, campaign8673413b357d, compares native40 damage
                // before/equipped/final: I029 adds5 (A04S), I05D adds8 (A051).
                // Exact rank1 A059 contributes zero Uts3 armor in both contexts.
                // Its reflection/incoming modifiers remain separate consumers.
                if(additional && (item.id=="I029" || item.id=="I05D") && ability!=null &&
                    ability.id=="A059" && ability.baseCode=="AUts" &&
                    gap.Reason=="unresolved-inherited-default:DataC1" &&
                    states.Exists(s=>s.AbilityId=="A059" && s.Level==1))continue;
                // Eq2 measures native +30 to all three attributes. YU's
                // canonical primary selection is applied by Session; the
                // immutable native profile remains the measured raw profile.
                if (additional && item.id == "I05P" && ability != null && ability.baseCode == "AIab" &&
                    gap.Reason == "script-references-need-handler-coverage" &&
                    Array.TrueForAll(ability.jassFunctions, name => name == "YU")) continue;
                // Inv:35982 is restricted to E00L/E00M. The selected three
                // heroes retain their authored orb Idam bonus. Its on-hit
                // behavior and those other heroes' replacement stay separate.
                if (additional && ability != null &&
                    (ability.baseCode == "AIob" || ability.baseCode == "AIfb" || ability.baseCode == "AIsb") &&
                    gap.Reason == "script-references-need-handler-coverage" &&
                    Array.TrueForAll(ability.jassFunctions, name => name == "Inv")) continue;
                // Activated spell families have no continuously applied stats.
                // Eq2 must still reconcile all nine intrinsic snapshots; this
                // does not mark the separate activated spell as implemented.
                if (additional && ability != null && ItemActivationFamily(ability.baseCode) &&
                    (gap.Reason == "native-family-unimplemented:" + ability.baseCode ||
                     gap.Reason == "script-references-need-handler-coverage")) continue;
                if (additional && ability != null && ability.baseCode == "AUts" &&
                    gap.Reason == "script-references-need-handler-coverage") continue;
                // khv creates an O00D helper and adds these mana abilities to
                // that helper (51500); it never mutates the item holder's bonus.
                if (additional && ability != null && ability.baseCode == "AImm" &&
                    gap.Reason == "script-references-need-handler-coverage" &&
                    Array.TrueForAll(ability.jassFunctions, name => name == "khv")) continue;
                // Aura contributions belong to the recipient cache, not the
                // emitter's intrinsic equipment profile. Raw equip attributes
                // and maxima must still match the ordinary decomposition.
                if (additional && ability != null && ItemAuraFamily(ability.baseCode) &&
                    (gap.Reason == "native-family-unimplemented:" + ability.baseCode ||
                     gap.Reason == "script-references-need-handler-coverage")) continue;
                // These proc families have no continuous attribute/maxima
                // contribution. Their source fields run in the shared weapon
                // dispatcher; the measured equip decomposition still guards
                // every ordinary modifier below. This is not proc stacking proof.
                if (additional && ability != null &&
                    (ability.baseCode == "AOcr" || ability.baseCode == "AHbh" || ability.baseCode == "Afbk" ||
                     ability.baseCode == "Assk" || ability.baseCode == "ANss" || ability.baseCode == "ANfd" || ability.baseCode == "AEah" ||
                     ability.baseCode == "AIcb") &&
                    (gap.Reason == "native-family-unimplemented:" + ability.baseCode ||
                     gap.Reason == "script-references-need-handler-coverage")) continue;
                // Lifesteal is applied to actual weapon HP loss, never folded
                // into intrinsic attack damage or maximum health. The source
                // rank/lifecycle references remain visible in the full plan.
                if (additional && ability != null && ability.baseCode == "AIva" &&
                    (gap.Reason == "native-family-unimplemented:AIva" || gap.Reason == "script-references-need-handler-coverage")) continue;
                // Activations and their source callbacks do not supply an
                // intrinsic bonus. Reversible measured equip values and all
                // ordinary modifiers below still have to reconcile exactly.
                if (additional && ability != null &&
                    (ability.baseCode == "AIda" || ability.baseCode == "ANcl" || ability.baseCode == "Aste" || item.id == "I0AE" && ability.id == "A0WW") &&
                    (gap.Reason == "native-family-unimplemented:" + ability.baseCode ||
                     gap.Reason == "script-references-need-handler-coverage" ||
                     ability.baseCode == "ANcl" && gap.Reason == "unknown-ability-or-level")) continue;
                // AIda only supplies an activated armor buff. Eq2's reversible
                // zero intrinsic maxima/attributes remain required here; the
                // activation executor is independently gated by ITEMACT2.
                if (additional && item.id == "I02H" && ability != null && ability.id == "A0A0" &&
                    ability.baseCode == "AIda" && gap.Reason == "native-family-unimplemented:AIda") continue;
                // Eq2 observes these exact AIfs items' unchanged intrinsic
                // profile. ITEMACT2 gates activation and summoned profiles
                // separately; script and active coverage remains in Plan().
                if (additional && Array.IndexOf(new[] { "I01A", "I01D", "I01J", "I01M", "I02G", "I07E", "I07K" }, item.id) >= 0 &&
                    ability != null && ability.baseCode == "AIfs" && gap.Reason == "native-family-unimplemented:AIfs") continue;
                // Exact I00Z reversible +30INT/+300MP was measured in Eq2.
                // Its AUau aura and AUin active are not intrinsic attributes;
                // their coverage remains explicit in the full effect plan.
                if (additional && item.id == "I00Z" && ability != null &&
                    (ability.id == "A01M" && ability.baseCode == "AUau" || ability.id == "S000" && ability.baseCode == "AUin") &&
                    (gap.Reason.StartsWith("native-family-unimplemented:", StringComparison.Ordinal) ||
                     gap.Reason == "script-references-need-handler-coverage")) continue;
                // LiAItemFam3 verifies the five exact equipped item profiles.
                // AOwk/Aste activation and ANfd on-hit are separate executors;
                // iEv is the curse's conditional removal of boot abilities.
                // Their coverage gaps remain in Plan(), not hidden as effects.
                if (additional && ability != null &&
                    ((ability.baseCode == "AIms" && gap.Reason == "script-references-need-handler-coverage" &&
                      Array.TrueForAll(ability.jassFunctions, name => name == "iEv")) ||
                     (MeasuredFamilyItem(item.id) && (ability.baseCode == "AOwk" || ability.baseCode == "ANfd" || ability.baseCode == "Aste") &&
                      (gap.Reason.StartsWith("native-family-unimplemented:", StringComparison.Ordinal) ||
                       gap.Reason == "script-references-need-handler-coverage")))) continue;
                // ANcl supplies a command/channel, not an intrinsic stat bonus.
                // The exact additional equip sequence must still establish
                // unchanged/reversible maxima and attributes for this item.
                // Its active and scripted behaviors remain planner gaps.
                if (additional && ability != null && ability.baseCode == "ANcl" &&
                    (gap.Reason == "native-family-unimplemented:ANcl" || gap.Reason == "unknown-ability-or-level" ||
                     gap.Reason == "script-references-need-handler-coverage" && Array.TrueForAll(ability.jassFunctions, name => name == "hK"))) continue;
                if (ability == null || ability.baseCode != "AIab" || !gap.Reason.StartsWith("unresolved-inherited-default:Data", StringComparison.Ordinal))
                    return RejectProfile(additional, "Observed equip does not close native parameter: " + item.id + ":" + gap.Reason);
            }
            var profile = new OriginalNativeItemProfile { known = true, strength = measurement.strength,
                agility = measurement.agility, intelligence = measurement.intelligence };
            double strength = 0, agility = 0, intelligence = 0;
            foreach (var modifier in evaluation.Modifiers)
            {
                // Transfer the measured AIms maximum rule to authored boot values;
                // this is family-derived, not a runtime measurement of every item.
                if (additional && modifier.Stat == "moveSpeedFlat" && modifier.Operation == "maximum")
                { profile.moveSpeedFlat = Math.Max(profile.moveSpeedFlat, modifier.Value); continue; }
                if (additional && modifier.Stat == "spellResistanceFraction" && modifier.Operation == "last-added") continue;
                if (modifier.Operation != "add") return RejectProfile(additional, "Unsupported native profile operation.");
                switch (modifier.Stat)
                {
                    case "strength": strength += modifier.Value; break;
                    case "agility": agility += modifier.Value; break;
                    case "intelligence": intelligence += modifier.Value; break;
                    case "maxHealth": profile.maxHealthFlat += modifier.Value; break;
                    case "maxMana": profile.maxManaFlat += modifier.Value; break;
                    case "attackDamage": profile.attackDamage += modifier.Value; break;
                    case "armor": profile.armor += modifier.Value; break;
                    case "attackSpeedFraction": profile.attackSpeedFraction += modifier.Value; break;
                    case "healthRegenPerSecond": profile.healthRegenPerSecond += modifier.Value; break;
                    case "manaRegenBaseFraction": profile.manaRegenBaseFraction += modifier.Value; break;
                    default: return RejectProfile(additional, "Unsupported native profile stat.");
                }
            }
            // H008 sample uses map Misc StrHitPointBonus=8, IntManaBonus=10.
            // These checks verify decomposition; the caller derives its actual
            // hero profile using the native catalog, not these sample totals.
            if (strength != measurement.strength || agility != measurement.agility || intelligence != measurement.intelligence ||
                profile.maxHealthFlat + 8 * strength != measurement.maxHealth || profile.maxManaFlat + 10 * intelligence != measurement.maxMana)
                return RejectProfile(additional, "Item declarations disagree with observed additive stats: " + item.id);
            SaveVitalitySteps(item, evaluation.Modifiers);
            return profile;
        }
        static OriginalNativeItemProfile RejectProfile(bool additional, string reason)
        { if (!additional) throw new ArgumentException(reason); return null; }
        static bool MeasuredFamilyItem(string id) => id == "I00Y" || id == "I013" || id == "I08U" || id == "I024" || id == "I026";
        static bool ItemActivationFamily(string code) => code == "AOcl" || code == "ACtc" || code == "Alsh" ||
            code == "AOsh" || code == "AIil" || code == "Aroa" || code == "Aami" || code == "Auhf" ||
            code == "AIha" || code == "Aens" || code == "ANdh" || code == "AOwk" || code == "AIsa" ||
            code == "AIra" || code == "AIrg" || code == "AIvi" || code == "AHds";
        static bool ItemAuraFamily(string code) => code == "AHad" || code == "AHab" || code == "AUau" ||
            code == "AOae" || code == "Aoar" || code == "Aakb" || code == "AUav";

        private OriginalItemInstance[] ValidateSlots(OriginalItemInstance[] slots, HashSet<long> identities)
        {
            if (slots == null || slots.Length != OriginalInventory.SlotsPerBag) throw new ArgumentException("Inventory must contain two six-slot bags.");
            var copy = new OriginalItemInstance[slots.Length];
            for (var i = 0; i < slots.Length; i++)
            {
                var item = slots[i];
                if (item == null) continue;
                if (item.instanceId <= 0 || !identities.Add(item.instanceId) || item.ownerId < 0 || item.ownerId > 8 ||
                    item.removed || item.charges < 0 || item.itemId == null || !items.ContainsKey(item.itemId))
                    throw new ArgumentException("Invalid or duplicated inventory item residency.");
                copy[i] = new OriginalItemInstance { instanceId = item.instanceId, itemId = item.itemId };
            }
            return copy;
        }

        private static void Gap(List<OriginalItemEffectGap> target, long instance, string ability, string reason, int level = 0)
            => target.Add(new OriginalItemEffectGap { ItemInstanceId = instance, AbilityId = ability, Level = level, Reason = reason });

        private static bool Rawcode(string value)
        {
            if (value == null || value.Length != 4) return false;
            foreach (var c in value) if (c < 32 || c > 126) return false;
            return true;
        }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static string[] Strings(string[] value) => value == null ? throw new ArgumentException("Missing array.") : (string[])value.Clone();
        private static OriginalItemScriptReference[] References(OriginalItemScriptReference[] values)
        {
            if (values == null) throw new ArgumentException("Missing script references.");
            var result = new OriginalItemScriptReference[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var source = values[i] ?? throw new ArgumentException("Null script reference.");
                result[i] = new OriginalItemScriptReference { functionName = source.functionName, line = source.line, endLine = source.endLine };
            }
            return result;
        }

        private static OriginalItemPassiveCatalog Freeze(OriginalItemPassiveCatalog source)
        {
            var result = new OriginalItemPassiveCatalog { abilities = new OriginalPassiveAbility[source.abilities.Length], items = new OriginalItemEffectDefinition[source.items.Length] };
            for (var i = 0; i < source.items.Length; i++)
            {
                var item = source.items[i] ?? throw new ArgumentException("Null item effect.");
                result.items[i] = new OriginalItemEffectDefinition { id = item.id, abilityIds = Strings(item.abilityIds), cmBonusRegistered = item.cmBonusRegistered,
                    cmBonus = item.cmBonus, cmSourceLine = item.cmSourceLine, scriptEffectsImplemented = item.scriptEffectsImplemented, scriptReferences = References(item.scriptReferences) };
            }
            for (var i = 0; i < source.abilities.Length; i++)
            {
                var sourceAbility = source.abilities[i] ?? throw new ArgumentException("Null passive ability.");
                if (sourceAbility.levels == null) throw new ArgumentException("Missing ability levels.");
                var ability = new OriginalPassiveAbility { id = sourceAbility.id, baseCode = sourceAbility.baseCode, directItemAbility = sourceAbility.directItemAbility,
                    passiveFamilyMapped = sourceAbility.passiveFamilyMapped, isSpellbook = sourceAbility.isSpellbook, declaredLevels = sourceAbility.declaredLevels,
                    levelsKnown = sourceAbility.levelsKnown, jassFunctions = Strings(sourceAbility.jassFunctions), levels = new OriginalPassiveLevel[sourceAbility.levels.Length] };
                result.abilities[i] = ability;
                var levels = new HashSet<int>();
                for (var l = 0; l < ability.levels.Length; l++)
                {
                    var original = sourceAbility.levels[l];
                    if (original == null || original.level <= 0 || !levels.Add(original.level) || original.modifiers == null) throw new ArgumentException("Invalid passive level.");
                    var level = new OriginalPassiveLevel { level = original.level, withinDeclaredLevels = original.withinDeclaredLevels,
                        spellbookFieldKnown = original.spellbookFieldKnown, spellbookAbilityIds = Strings(original.spellbookAbilityIds), modifiers = new OriginalPassiveParameter[original.modifiers.Length] };
                    ability.levels[l] = level;
                    for (var m = 0; m < level.modifiers.Length; m++)
                    {
                        var p = original.modifiers[m] ?? throw new ArgumentException("Null passive modifier.");
                        if (p.known && (!Finite(p.value) || string.IsNullOrEmpty(p.stat) || string.IsNullOrEmpty(p.operation))) throw new ArgumentException("Invalid known passive modifier.");
                        level.modifiers[m] = new OriginalPassiveParameter { stat = p.stat, operation = p.operation, stackingGroup = p.stackingGroup, stackingRule = p.stackingRule,
                            known = p.known, value = p.value, field = p.field, column = p.column, state = p.state, sources = Strings(p.sources) };
                    }
                }
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedBountySource
    {
        public string cacheName, cacheSha256, capturedUtc, probeMapSha256, probeScriptSha256;
        public int records, succeeded, failed;
        public bool controlsPassed;
    }
    [Serializable] public sealed class OriginalObservedBountySample
    {
        public string sourceKey, error;
        public bool created, valid;
        public int gold, observerGold, lumber, actualUnitLevel, userData, killerExperience, observerExperience;
        public double damageTime, observedTime;
    }
    [Serializable] public sealed class OriginalObservedBountyUnit
    {
        public string id;
        public OriginalCombatField[] sourceFields;
        public OriginalObservedBountySample[] samples;
        public bool measured, distributionKnown, observedEqualsDeclaredBase;
        public int observedMin, observedMax;
    }
    [Serializable] public sealed class OriginalObservedBountyCatalog
    {
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public OriginalObservedBountySource source;
        public OriginalObservedBountyUnit[] units;
        public OriginalObservedBountySample[] controls;
        public string[] limits;
    }
    public readonly struct OriginalNativeBounty
    {
        public readonly int amount;
        public readonly bool distributionKnown;
        public readonly string evidence;
        public OriginalNativeBounty(int amount, bool distributionKnown, string evidence)
        { this.amount = amount; this.distributionKnown = distributionKnown; this.evidence = evidence; }
    }

    // Native gold is separate from Ibv XP and from the original shared-gold
    // round reward. Sparse SLK fields are never converted into known zeros.
    // The measured fallback is an explicit four-sample fixed baseline, not
    // proof that the engine's full random distribution is constant.
    public sealed class OriginalNativeBountyRules
    {
        sealed class Rule { internal int amount, dice, sides; internal bool declared; }
        readonly Dictionary<string, Rule> rules = new Dictionary<string, Rule>(StringComparer.Ordinal);
        public OriginalNativeBountyRules(OriginalCombatCatalog combat, OriginalObservedBountyCatalog measured)
        {
            if (combat == null || measured == null) throw new ArgumentNullException();
            combat.BuildIndexes();
            Check(measured.schemaVersion == 1 && measured.mapSha256 == combat.sourceSha256 &&
                  measured.mapSha256 == "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34" &&
                  measured.engineVersion == "1.26.0.6401", "Wrong native bounty dataset");
            var source = measured.source;
            Check(source != null && source.cacheName == "LiABounty1.w3v" &&
                  source.cacheSha256 == "ecffc4a0cbec11b3279b16e7acfba7d054e177fc0e655c7c20810a830e52b28e" &&
                  source.probeMapSha256 == "965c0c48e1cbf9415f56745265c99e093471ceec7ad0201c58ab9ee3c3641aeb" &&
                  source.probeScriptSha256 == "8d79d2e2eb672bd0155db20575635d2494e683bc3dd19e7b822a1496b837e3fb" &&
                  !string.IsNullOrEmpty(source.capturedUtc) && source.controlsPassed &&
                  source.records == 340 && source.succeeded == 336 && source.failed == 4, "Invalid bounty provenance");
            Check(measured.controls != null && measured.controls.Length == 4, "Missing bounty controls");
            string[] keys = { "control_positive_start", "control_disabled_start", "control_disabled_end", "control_positive_end" };
            for (int i = 0; i < keys.Length; i++)
            {
                var sample = measured.controls[i]; ValidateSample(sample);
                Check(sample.sourceKey == keys[i] && sample.created && sample.valid &&
                      ((i == 0 || i == 3) ? sample.gold >= 58 && sample.gold <= 74 : sample.gold == 0), "Failed bounty control");
            }
            Check(measured.units != null && measured.units.Length == 84, "Incomplete bounty units");
            var identities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var unit in measured.units)
            {
                Check(unit != null && unit.id != null && unit.id.Length == 4 && identities.Add(unit.id) &&
                      unit.samples != null && unit.samples.Length == 4 && !unit.distributionKnown, "Invalid bounty unit");
                for (int i = 0; i < 4; i++)
                {
                    var sample = unit.samples[i]; ValidateSample(sample);
                    Check(sample.sourceKey == "bounty_" + unit.id + "_" + i, "Wrong bounty sample identity");
                    if (unit.id == "n068")
                        Check(!unit.measured && !sample.created && !sample.valid && !string.IsNullOrEmpty(sample.error) &&
                              sample.gold == 0 && sample.killerExperience == 0 && sample.observerExperience == 0 &&
                              sample.damageTime == 0 && sample.observedTime == 0 && sample.actualUnitLevel == 0,
                              "Missing unit has bounty");
                    else Check(unit.measured && sample.created && sample.valid && string.IsNullOrEmpty(sample.error) &&
                               sample.gold == unit.observedMin && sample.gold == unit.observedMax &&
                               (unit.id != "O006" || sample.actualUnitLevel == 50), "Inconsistent bounty samples");
                }
                if (unit.id == "n068") continue;
                var definition = combat.Unit(unit.id);
                bool hasBase = Number(definition, "bountyplus", out int baseGold);
                bool hasDice = Number(definition, "bountydice", out int dice);
                bool hasSides = Number(definition, "bountysides", out int sides);
                if (hasBase) Check(baseGold == unit.observedMin, "Measured bounty conflicts with source base");
                else Check(unit.observedMin == 0 && (unit.id == "O006" || unit.id == "n00K" || unit.id == "n00Z" ||
                          unit.id == "n017" || unit.id == "n0AW" || unit.id == "u00G" || unit.id == "u00L"), "Unreviewed missing base");
                // Preserve complete declared dice when present. No current wave
                // definition supplies all three fields; sparse rows use the
                // separately labelled observed baseline below.
                bool complete = hasBase && hasDice && hasSides;
                Check(!complete || dice == 0 || sides > 0, "Invalid declared bounty dice");
                rules.Add(unit.id, new Rule { amount = complete ? baseGold : unit.observedMin,
                    dice = dice, sides = sides, declared = complete });
            }
            Check(rules.Count == 83 && identities.Contains("n068"), "Wrong measured bounty roster");
        }

        public OriginalNativeBounty Resolve(string id, Func<int, int> rollDie = null)
        {
            if (id == null || !rules.TryGetValue(id, out var rule)) throw new InvalidOperationException("Unresolved native bounty: " + id);
            int total = rule.amount;
            if (rule.declared)
                for (int i = 0; i < rule.dice; i++)
                {
                    if (rollDie == null) throw new InvalidOperationException("Bounty die requires host RNG.");
                    int value = rollDie(rule.sides);
                    if (value < 1 || value > rule.sides) throw new InvalidOperationException("Invalid host bounty die.");
                    total = checked(total + value);
                }
            return new OriginalNativeBounty(total, rule.declared, rule.declared ? "source-declared-dice-model" : "runtime-derived-fixed-baseline; four-samples; distribution-unresolved");
        }

        static bool Number(OriginalCombatDefinition unit, string key, out int value)
        {
            value = 0;
            bool present = Array.Exists(unit.fields, f => f != null && f.key == key);
            if (!present) return false;
            Check(unit.TryNumber(key, out double number, out _) && number >= 0 && number <= 1000000 && number == Math.Truncate(number), "Invalid/ambiguous bounty source field");
            value = (int)number; return true;
        }
        static void ValidateSample(OriginalObservedBountySample sample)
        {
            Check(sample != null && sample.gold >= 0 && sample.observerGold == 0 && sample.lumber == 0 &&
                  sample.killerExperience >= 0 && sample.observerExperience >= 0 &&
                  !double.IsNaN(sample.damageTime) && !double.IsInfinity(sample.damageTime) && sample.damageTime >= 0 &&
                  !double.IsNaN(sample.observedTime) && !double.IsInfinity(sample.observedTime) && sample.observedTime >= sample.damageTime, "Invalid bounty observation");
        }
        static void Check(bool condition, string reason) { if (!condition) throw new ArgumentException(reason); }
    }
}

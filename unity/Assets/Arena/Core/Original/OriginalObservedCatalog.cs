using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedSource
    {
        public int id, records, succeeded, failed;
        public string cacheName, cacheSha256, probeMapSha256, probeScriptSha256, capturedUtc;
        public bool complete;
    }

    [Serializable] public sealed class OriginalObservedUnit
    {
        public string id, error, sourceKey;
        public int sourceId;
        public bool created, known;
        public double maxHP, maxMP, moveSpeed, defaultMoveSpeed;
        public bool armorKnown;
        public double armor;
        public int armorSourceId;
        public string armorSourceKey, armorScope;
        public double RequireMaxHP() => Require(maxHP, true);
        public double RequireMaxMP() => Require(maxMP, false);
        public double RequireMoveSpeed() => Require(moveSpeed, false);
        public double RequireDefaultMoveSpeed() => Require(defaultMoveSpeed, false);
        public double RequireArmor()
        {
            if (!created || !known || !armorKnown || !OriginalObservedCatalog.Finite(armor))
                throw new InvalidOperationException("Native armor measurement unavailable: " + id);
            return armor;
        }
        double Require(double value, bool positive)
        {
            if (!created || !known || !OriginalObservedCatalog.Finite(value) || value < 0 || (positive && value == 0))
                throw new InvalidOperationException("Native unit measurement unavailable: " + id + ": " + error);
            return value;
        }
    }

    [Serializable] public sealed class OriginalObservedHeroLevel
    {
        public string id;
        public int level, experience, strength, agility, intelligence;
        public bool known;
        public double maxHP, maxMP;
        public string[] sourceKeys;
    }

    [Serializable] public sealed class OriginalObservedRankEffect
    {
        public int rank, sourceId;
        public string sourceKey;
        public bool known;
        public double baseStrengthBonus, baseAgilityBonus, baseIntelligenceBonus;
        public double strengthBonus, agilityBonus, intelligenceBonus, maxHPBonus, maxMPBonus;
    }

    [Serializable] public sealed class OriginalObservedSkill
    {
        public string heroId, abilityId, scope;
        public int maximumRank, sourceId;
        public bool known;
        public int[] minimumHeroLevels;
        public OriginalObservedRankEffect[] rankEffects;
        public string[] sourceKeys;
        public int RequiredLevel(int rank)
        {
            if (!known || rank < 1 || rank > maximumRank || minimumHeroLevels == null || minimumHeroLevels.Length != maximumRank)
                throw new InvalidOperationException("Native skill rank not observed: " + abilityId);
            var value = minimumHeroLevels[rank - 1];
            if (value < 1 || value > 27) throw new InvalidOperationException("Invalid observed skill threshold");
            return value;
        }
    }

    [Serializable] public sealed class OriginalObservedVitalityChange
    {
        public string heroId, operation, policy, sourceKey, scope;
        public bool policyKnown;
        public int fromLevel, toLevel, fromRank, toRank, sourceId;
        public double beforeHP, beforeMaxHP, beforeMP, beforeMaxMP;
        public double afterHP, afterMaxHP, afterMP, afterMaxMP, delayedHP, delayedMP;
    }

    // Observations are narrow native measurements, not original full-match behavior.
    [Serializable] public sealed class OriginalObservedCatalog
    {
        public int schemaVersion;
        public string version, sourceSha256, engineVersion;
        public bool runtimeObserved;
        public OriginalObservedSource[] sources;
        public OriginalObservedUnit[] units;
        public OriginalObservedHeroLevel[] heroes;
        public OriginalObservedSkill[] skills;
        public OriginalObservedVitalityChange[] vitalityChanges;
        // Validated against combat declarations by ConfigureBounty; kept separate
        // from exact native unit getters and their known/default flags.
        public OriginalObservedBountyCatalog bounty;
        public OriginalObservedSparseCatalog sparse;
        public string[] limits;
        [NonSerialized] Dictionary<string, OriginalObservedUnit> unitIndex;
        [NonSerialized] Dictionary<string, OriginalObservedHeroLevel> heroIndex;
        [NonSerialized] Dictionary<string, OriginalObservedSkill> skillIndex;
        [NonSerialized] Dictionary<string, OriginalObservedVitalityChange> vitalityIndex;

        public void BuildIndexes()
        {
            Check(schemaVersion == 1 && version == "3.9c" && engineVersion == "1.26.0.6401" && runtimeObserved &&
                  sourceSha256 == "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34", "Wrong observed catalog identity");
            Check(sources != null && sources.Length == 3, "Missing observation sources");
            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                Check(source != null && source.id == i && source.complete &&
                      source.records == (i == 0 ? 105 : i == 1 ? 405 : 156) && source.failed == (i == 1 ? 0 : 1) &&
                      source.succeeded + source.failed == source.records &&
                      source.cacheName == (i == 0 ? "LiAProbe2.w3v" : i == 1 ? "LiASkill2.w3v" : "LiACombatP.w3v") &&
                      Hash(source.cacheSha256) && Hash(source.probeMapSha256) && Hash(source.probeScriptSha256) &&
                      !string.IsNullOrEmpty(source.capturedUtc), "Invalid observation source");
            }
            Check(units != null && units.Length == 84 && heroes != null && heroes.Length == 150 && skills != null && skills.Length == 15 &&
                  vitalityChanges != null && vitalityChanges.Length == 6,
                  "Incomplete observation tables");
            var nextUnits = new Dictionary<string, OriginalObservedUnit>(StringComparer.Ordinal);
            var nextHeroes = new Dictionary<string, OriginalObservedHeroLevel>(StringComparer.Ordinal);
            var nextSkills = new Dictionary<string, OriginalObservedSkill>(StringComparer.Ordinal);
            var nextVitality = new Dictionary<string, OriginalObservedVitalityChange>(StringComparer.Ordinal);
            foreach (var row in units)
            {
                Check(row != null && Rawcode(row.id) && row.created == row.known && row.sourceId == 0 && row.sourceKey == "unit_" + row.id,
                      "Invalid native unit observation");
                if (row.created)
                {
                    row.RequireMaxHP(); row.RequireMaxMP(); row.RequireMoveSpeed(); row.RequireDefaultMoveSpeed();
                    Check(string.IsNullOrEmpty(row.error), "Successful unit has error");
                }
                else Check(!string.IsNullOrEmpty(row.error) && row.maxHP == 0 && row.maxMP == 0 && row.moveSpeed == 0 && row.defaultMoveSpeed == 0,
                           "Failed unit contains measurements");
                if (row.armorKnown)
                    Check(row.created && row.known && row.id == "n008" && row.armor == 0 && row.armorSourceId == 2 && row.armorSourceKey == "armor_n008" &&
                          row.armorScope == "unmodified-native-unit; effective-chaos-normal-armor; two-positive-controls", "Invalid observed armor proof");
                else Check(row.armor == 0, "Unknown armor contains a numeric estimate");
                Check(!nextUnits.ContainsKey(row.id), "Duplicate observed unit"); nextUnits.Add(row.id, row);
            }
            foreach (var row in heroes)
            {
                Check(row != null && HeroId(row.id) && row.known && row.level >= 1 && row.level <= 50 &&
                      row.experience >= 0 && row.strength >= 0 && row.agility >= 0 && row.intelligence >= 0 &&
                      Finite(row.maxHP) && row.maxHP > 0 && Finite(row.maxMP) && row.maxMP >= 0 &&
                      row.sourceKeys != null && row.sourceKeys.Length > 0, "Invalid observed hero baseline");
                var key = HeroKey(row.id, row.level);
                Check(!nextHeroes.ContainsKey(key), "Duplicate observed hero level"); nextHeroes.Add(key, row);
            }
            foreach (var hero in new[] { "H008", "N0A0", "H024" })
                for (var level = 1; level <= 50; level++)
                    Check(nextHeroes.ContainsKey(HeroKey(hero, level)), "Missing observed hero level");
            foreach (var row in skills)
            {
                Check(row != null && HeroId(row.heroId) && Rawcode(row.abilityId) && row.known && row.sourceId == 1 &&
                      row.maximumRank == (row.abilityId == "A001" ? 15 : 3) && row.minimumHeroLevels != null &&
                      row.minimumHeroLevels.Length == row.maximumRank && row.rankEffects != null && row.rankEffects.Length == row.maximumRank &&
                      row.sourceKeys != null && row.sourceKeys.Length == 27 && row.scope == "native-learning-only; original-handlers-not-executed",
                      "Invalid observed skill");
                for (var i = 0; i < row.maximumRank; i++)
                {
                    Check(row.minimumHeroLevels[i] >= 1 && row.minimumHeroLevels[i] <= 27 &&
                          (i == 0 || row.minimumHeroLevels[i] > row.minimumHeroLevels[i - 1]), "Invalid skill threshold");
                    var effect = row.rankEffects[i];
                    Check(effect != null && effect.known && effect.rank == i + 1 && effect.sourceId == 1 && !string.IsNullOrEmpty(effect.sourceKey),
                          "Missing skill rank measurement");
                    foreach (var value in new[] { effect.baseStrengthBonus, effect.baseAgilityBonus, effect.baseIntelligenceBonus,
                        effect.strengthBonus, effect.agilityBonus, effect.intelligenceBonus, effect.maxHPBonus, effect.maxMPBonus })
                        Check(Finite(value), "Nonfinite skill effect");
                }
                var key = row.heroId + "/" + row.abilityId;
                Check(!nextSkills.ContainsKey(key), "Duplicate observed skill"); nextSkills.Add(key, row);
            }
            foreach (var key in new[] { "H008/A05N", "H008/A05M", "H008/A102", "H008/A0E6", "H008/A001",
                "N0A0/A15W", "N0A0/A0AS", "N0A0/A15X", "N0A0/A0AC", "N0A0/A001",
                "H024/A0SJ", "H024/A0SP", "H024/A0AE", "H024/A0SM", "H024/A001" })
                Check(nextSkills.ContainsKey(key), "Missing observed selected-hero skill");
            foreach (var row in vitalityChanges)
            {
                Check(row != null && HeroId(row.heroId) && (row.operation == "level-up" || row.operation == "learn-A001") &&
                      row.sourceId == 2 && !string.IsNullOrEmpty(row.sourceKey) && !string.IsNullOrEmpty(row.scope), "Invalid vitality observation");
                foreach (var number in new[] { row.beforeHP, row.beforeMaxHP, row.beforeMP, row.beforeMaxMP,
                    row.afterHP, row.afterMaxHP, row.afterMP, row.afterMaxMP, row.delayedHP, row.delayedMP })
                    Check(Finite(number) && number >= 0, "Invalid vitality number");
                Check(row.beforeMaxHP > 0 && row.afterMaxHP > 0 && row.beforeHP == row.beforeMaxHP * .5 && row.beforeMP == row.beforeMaxMP * .5 &&
                      row.afterHP <= row.afterMaxHP && row.afterMP <= row.afterMaxMP && row.fromRank == 0, "Wrong vitality initial condition");
                if (row.operation == "level-up")
                    Check(row.policyKnown && row.policy == "preserve-deficit" && row.fromLevel == 1 && row.toLevel == 2 && row.toRank == 0 &&
                          row.beforeMaxHP - row.beforeHP == row.afterMaxHP - row.afterHP && row.beforeMaxMP - row.beforeMP == row.afterMaxMP - row.afterMP,
                          "Unproven level-up deficit policy");
                else Check(!row.policyKnown && row.policy == "ratio-rounding-unresolved" && row.fromLevel == 12 && row.toLevel == 12 && row.toRank == 1,
                           "Unproven A001 vitality policy");
                var key = row.heroId + "/" + row.operation;
                Check(!nextVitality.ContainsKey(key), "Duplicate vitality observation"); nextVitality.Add(key, row);
            }
            // Publish only after all cross-table checks succeed.
            sparse?.BuildIndexes();
            unitIndex = nextUnits; heroIndex = nextHeroes; skillIndex = nextSkills; vitalityIndex = nextVitality;
        }

        public OriginalObservedUnit Unit(string id)
        {
            Ensure();
            return id != null && unitIndex.TryGetValue(id, out var value) ? value :
                new OriginalObservedUnit { id = id, error = "unit-not-observed" };
        }
        public bool TryHero(string id, int level, out OriginalObservedHeroLevel value)
        {
            Ensure();
            if (!heroIndex.TryGetValue(HeroKey(id, level), out value) || !value.known) { value = null; return false; }
            Check(value.strength >= 0 && value.agility >= 0 && value.intelligence >= 0 && value.experience >= 0 &&
                  Finite(value.maxHP) && value.maxHP > 0 && Finite(value.maxMP) && value.maxMP >= 0, "Invalid observed hero values");
            return true;
        }
        public OriginalObservedHeroLevel Hero(string id, int level)
        {
            if (!TryHero(id, level, out var value)) throw new InvalidOperationException("Hero level not observed: " + id + "/" + level);
            return value;
        }
        public OriginalObservedSkill Skill(string heroId, string abilityId)
        {
            Ensure();
            if (!skillIndex.TryGetValue(heroId + "/" + abilityId, out var value)) throw new InvalidOperationException("Skill not observed");
            return value;
        }
        public OriginalObservedVitalityChange VitalityChange(string heroId, string operation)
        {
            Ensure();
            if (!vitalityIndex.TryGetValue(heroId + "/" + operation, out var value)) throw new InvalidOperationException("Vitality change not observed");
            return value;
        }
        void Ensure() { if (unitIndex == null) BuildIndexes(); }
        static string HeroKey(string id, int level) => id + "/" + level;
        static bool HeroId(string id) => id == "H008" || id == "N0A0" || id == "H024";
        static bool Rawcode(string value) => value != null && value.Length == 4;
        static bool Hash(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (var character in value) if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'))) return false;
            return true;
        }
        internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}

using System;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalMatchCatalog
    {
        public int schemaVersion;
        public string sourceSha256;
        public OriginalWaveDefinition[] waves;
        public OriginalEnemyDefinition[] enemies;
        public OriginalRuleSource[] sources;

        public OriginalWaveDefinition Wave(int round)
        {
            if (waves != null)
                foreach (var wave in waves)
                    if (wave.round == round) return wave;
            throw new ArgumentOutOfRangeException(nameof(round), "No source wave declaration.");
        }

        public OriginalEnemyDefinition Enemy(string id)
        {
            if (enemies != null)
                foreach (var enemy in enemies)
                    if (enemy.id == id) return enemy;
            throw new ArgumentException("No source enemy declaration: " + id, nameof(id));
        }

        public void Validate()
        {
            if (schemaVersion != 1 || sourceSha256 != "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34")
                throw new ArgumentException("The rules require the verified 3.9c source map.");
            if (waves == null || waves.Length != 30)
                throw new ArgumentException("Expected 30 original rounds.");
            for (var round = 1; round <= 30; round++)
            {
                var wave = Wave(round);
                Enemy(wave.bossId);
                if (round % 5 != 0) { Enemy(wave.regularId); Enemy(wave.casterId); }
            }
        }
    }

    [Serializable]
    public sealed class OriginalWaveDefinition
    {
        public int round;
        public string name;
        public string regularId;
        public string bossId;
        public string casterId;
        public int rewardRate;
        public int regularBounty;
        public int bossBounty;
        public int sourceLine;
    }

    [Serializable]
    public sealed class OriginalEnemyDefinition
    {
        public string id;
        public string name;
        public int level;
        public string[] abilities;
        public bool definitionKnown;

        public bool HasAbility(string id)
        {
            return abilities != null && Array.IndexOf(abilities, id) >= 0;
        }
    }

    [Serializable]
    public sealed class OriginalRuleSource
    {
        public string ruleId;
        public string path;
        public string function;
        public int line;
        public int endLine;
    }

    public enum OriginalDifficulty { Custom = 0, Easy = 1, Standard = 2, Extreme = 3, Nightmare = 4 }
    public enum OriginalHeroSelection { Free = 1, Random = 2, Duplicates = 3, SameRandom = 4 }
    public enum OriginalDefensiveBarrels { Attackable = 1, Protected = 2, Destroyed = 3 }

    [Serializable]
    public sealed class OriginalMatchOptions
    {
        public OriginalDifficulty difficulty = OriginalDifficulty.Standard;
        public OriginalHeroSelection heroSelection = OriginalHeroSelection.Free;
        public bool altars = true;
        public bool equalGold;
        public bool casters = true;
        public bool runes = true;
        public bool explosiveBarrels = true;
        public OriginalDefensiveBarrels defensiveBarrels = OriginalDefensiveBarrels.Protected;
        public bool curse;
        public bool acolyteBonus;
        public bool returnToCenter = true;
        public bool compactShops;

        public static OriginalMatchOptions ForDifficulty(OriginalDifficulty difficulty)
        {
            var options = new OriginalMatchOptions { difficulty = difficulty };
            switch (difficulty)
            {
                case OriginalDifficulty.Easy:
                    options.equalGold = true; options.casters = false; options.acolyteBonus = true;
                    break;
                case OriginalDifficulty.Custom:
                case OriginalDifficulty.Standard: break;
                case OriginalDifficulty.Extreme:
                    options.altars = false; options.runes = false; options.explosiveBarrels = false;
                    options.defensiveBarrels = OriginalDefensiveBarrels.Attackable;
                    break;
                case OriginalDifficulty.Nightmare:
                    options.altars = false; options.runes = false; options.explosiveBarrels = false;
                    options.defensiveBarrels = OriginalDefensiveBarrels.Destroyed; options.curse = true;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(difficulty));
            }
            return options;
        }

        // wZ15155..15173 compares only these eight survival flags. Hero
        // selection, the return position and shop layout do not alter Qc.
        public OriginalDifficulty ClassifyDifficulty()
        {
            for(int value=1;value<=4;value++)
            {
                var preset=ForDifficulty((OriginalDifficulty)value);
                if(altars==preset.altars&&equalGold==preset.equalGold&&casters==preset.casters&&
                    runes==preset.runes&&explosiveBarrels==preset.explosiveBarrels&&
                    defensiveBarrels==preset.defensiveBarrels&&curse==preset.curse&&acolyteBonus==preset.acolyteBonus)
                    return (OriginalDifficulty)value;
            }
            return OriginalDifficulty.Custom;
        }

        internal OriginalMatchOptions Copy()
        {
            return (OriginalMatchOptions)MemberwiseClone();
        }

        internal void Validate()
        {
            if (!Enum.IsDefined(typeof(OriginalDifficulty), difficulty) ||
                !Enum.IsDefined(typeof(OriginalHeroSelection), heroSelection) ||
                !Enum.IsDefined(typeof(OriginalDefensiveBarrels), defensiveBarrels))
                throw new ArgumentException("Unknown original session option.");
        }
    }
}

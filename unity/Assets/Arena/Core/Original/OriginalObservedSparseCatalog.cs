using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arena.Original
{
    [Serializable] public sealed class OriginalObservedSparseDamage
    {
        public bool known, attack;
        public double requested, before, after, eventDamage, restored;
        public string error;
    }
    [Serializable] public sealed class OriginalObservedSparseArmorRow
    {
        public string id, sourceKey;
        public bool known, hero;
        public int level, strength, agility, intelligence, experience;
        public double maxHP, maxMP, moveSpeed, defaultMoveSpeed;
        public string[] removedAbilities;
        public OriginalObservedSparseDamage[] damage;
    }
    [Serializable] public sealed class OriginalObservedBodyPosition
    {
        public double x, y, centerDistance, moveSpeed;
        public int order;
    }
    [Serializable] public sealed class OriginalObservedSparseBodyRow
    {
        public string id, sourceKey;
        public double requestedDistance;
        public OriginalObservedBodyPosition created, immediate, delayed, moveBefore;
        public OriginalObservedBodyPosition[] samples;
    }
    [Serializable] public sealed class OriginalObservedSparseUnit
    {
        public string id, sourceKey, intactSourceKey, armorScope;
        public bool known, hero, armorKnown;
        public int level, strength, agility, intelligence, experience;
        public double maxHP, maxMP, moveSpeed, defaultMoveSpeed, armor, intactChaosNormalRatio;
        public double RequireMaxHP() => Require(maxHP, true);
        public double RequireMaxMP() => Require(maxMP, false);
        public double RequireMoveSpeed() => Require(moveSpeed, false);
        public double RequireArmor()
        {
            if (!known || !armorKnown || !OriginalObservedCatalog.Finite(armor)) throw new InvalidOperationException("Sparse armor unavailable: " + id);
            return armor;
        }
        public int RequireStrength() => Attribute(strength);
        public int RequireAgility() => Attribute(agility);
        public int RequireIntelligence() => Attribute(intelligence);
        int Attribute(int value)
        {
            if (!known || !hero || value < 0) throw new InvalidOperationException("Sparse hero attribute unavailable: " + id);
            return value;
        }
        double Require(double value, bool positive)
        {
            if (!known || !OriginalObservedCatalog.Finite(value) || value < 0 || positive && value == 0)
                throw new InvalidOperationException("Sparse native getter unavailable: " + id);
            return value;
        }
        internal OriginalObservedSparseUnit Copy() => (OriginalObservedSparseUnit)MemberwiseClone();
    }

    // SPARSE1 is a separate exact batch. It never changes the original three
    // observation sources, 150 selected-hero levels, or unresolved collision data.
    [Serializable] public sealed class OriginalObservedSparseCatalog
    {
        public int schemaVersion;
        public string mapSha256, engineVersion;
        public OriginalObservedSource source;
        public OriginalObservedSparseUnit[] units;
        public OriginalObservedSparseArmorRow[] armorRows;
        public OriginalObservedSparseBodyRow[] bodyRows;
        public string[] limits;
        [NonSerialized] Dictionary<string, OriginalObservedSparseUnit> index;
        static readonly string[] armorIds = { "n009", "n00L", "n008", "n00D", "n00F", "n00I", "n05K", "n05L", "u00L" };
        static readonly string[] bodyIds = { "n008", "hfoo", "n06C", "n06I" };
        static readonly double[] distances = { 0, 23.9, 24.1, 47.9, 48.1, 55.1 };

        public void BuildIndexes()
        {
            Check(schemaVersion == 1 && mapSha256 == OriginalNativeCatalog.ExpectedMapSha256 && engineVersion == "1.26.0.6401", "Wrong sparse identity");
            Check(source != null && source.complete && source.records == 44 && source.succeeded == 44 && source.failed == 0 &&
                source.cacheName == "LiASparse1.w3v" && source.cacheSha256 == "3e7594731144774babe819ab307e083142977675bec4c0cd05576d49ae1bcf88" &&
                source.probeMapSha256 == "dc8cf77cca3dfa1e5bef7ec6b00220f457a951f080422f9b60f9715465ecabd7" &&
                source.probeScriptSha256 == "3b4afd3c4e91181232db0753c6c4db8cecca09e516a474d18dfc1541963f5dbc" && !string.IsNullOrEmpty(source.capturedUtc), "Wrong sparse source");
            Check(units != null && units.Length == 11 && armorRows != null && armorRows.Length == 20 && bodyRows != null && bodyRows.Length == 24, "Sparse matrix incomplete");
            var next = new Dictionary<string, OriginalObservedSparseUnit>(StringComparer.Ordinal);
            for (int i = 0; i < 11; i++)
            {
                bool hero = i >= 9; string id = hero ? "O006" : armorIds[i]; int level = hero ? (i == 9 ? 1 : 50) : 0;
                var row = armorRows[hero ? i + 9 : i * 2 + 1]; var intact = armorRows[hero ? i + 9 : i * 2]; var unit = units[i];
                ValidateArmor(row, id, level, hero ? "orn_L" + level : "armor_" + id + "_1", !hero);
                ValidateArmor(intact, id, level, hero ? "orn_L" + level : "armor_" + id + "_0", false);
                double armor = hero ? 80 : id == "n009" ? 3 : id == "n00L" ? 2 : 0;
                double ratio = id == "n00D" ? .2 : 1;
                foreach (var damage in row.damage)
                    if (damage.known && damage.requested > 1)
                        Check(Math.Abs(damage.eventDamage - damage.requested / (1 + .06 * armor)) < .0001, "Sparse armor inversion failed");
                for (int j = 0; j < 5; j++)
                    Check(row.damage[j].known == intact.damage[j].known && (!row.damage[j].known ||
                        Math.Abs(intact.damage[j].eventDamage - row.damage[j].eventDamage * ratio) < .0001), "Intact/removal damage changed");
                Check(unit != null && unit.id == id && unit.level == level && unit.known && unit.hero == hero && unit.armorKnown && unit.armor == armor &&
                    unit.armorScope == (hero ? "native-total-armor-including-agility" : "native-base-after-ability-removal") &&
                    unit.intactChaosNormalRatio == ratio && unit.sourceKey == row.sourceKey && unit.intactSourceKey == intact.sourceKey &&
                    unit.maxHP == row.maxHP && unit.maxMP == row.maxMP && unit.moveSpeed == row.moveSpeed && unit.defaultMoveSpeed == row.defaultMoveSpeed &&
                    unit.strength == row.strength && unit.agility == row.agility && unit.intelligence == row.intelligence && unit.experience == row.experience, "Sparse projection differs from proof");
                next.Add(Key(id, level), unit.Copy());
            }
            for (int i = 0; i < bodyRows.Length; i++)
            {
                var row = bodyRows[i]; string id = bodyIds[i / 6]; double distance = distances[i % 6];
                string suffix = distance == 0 ? "0" : distance.ToString("0.0", CultureInfo.InvariantCulture).Replace('.', '_');
                Check(row != null && row.id == id && row.sourceKey == "body_" + id + "_" + suffix && row.requestedDistance == distance &&
                    row.samples != null && row.samples.Length == 20, "Sparse body matrix changed");
                ValidatePosition(row.created); ValidatePosition(row.immediate); ValidatePosition(row.delayed); ValidatePosition(row.moveBefore);
                foreach (var sample in row.samples) ValidatePosition(sample);
            }
            index = next;
        }
        static void ValidateArmor(OriginalObservedSparseArmorRow row, string id, int level, string key, bool removed)
        {
            Check(row != null && row.id == id && row.level == level && row.hero == (level > 0) && row.known && row.sourceKey == key &&
                row.removedAbilities != null && (removed ? row.removedAbilities.Length > 0 : row.removedAbilities.Length == 0) &&
                row.damage != null && row.damage.Length == 5, "Sparse armor identity changed");
            Check(Finite(row.maxHP) && row.maxHP > 0 && Finite(row.maxMP) && row.maxMP >= 0 && Finite(row.moveSpeed) && row.moveSpeed >= 0 &&
                Finite(row.defaultMoveSpeed) && row.defaultMoveSpeed >= 0, "Invalid sparse getters");
            Check(level == 0 ? row.strength == 0 && row.agility == 0 && row.intelligence == 0 && row.experience == 0 :
                row.strength == 250 && row.agility == 250 && row.intelligence == 250 && row.maxHP == 30000 && row.maxMP == 2500 &&
                row.experience == (level == 1 ? 0 : 127400), "Wrong sparse hero profile");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string ability in row.removedAbilities) Check(ability != null && ability.Length == 4 && seen.Add(ability), "Invalid removed ability list");
            for (int i = 0; i < 5; i++)
            {
                var d = row.damage[i]; double amount = i == 0 ? 1 : i == 1 || i == 3 ? 10 : 40;
                Check(d != null && d.requested == amount && d.attack == (i >= 3) && Finite(d.before) && d.before > .405, "Invalid sparse damage setup");
                if (!d.known)
                {
                    Check(id == "u00L" && amount == 40 && d.before == 16 && d.after == 0 && d.eventDamage == 0 && d.restored == 0 &&
                        d.error == "Insufficient living HP for requested damage", "Unexpected sparse unknown amount");
                    continue;
                }
                Check(string.IsNullOrEmpty(d.error) && Finite(d.after) && d.after > .405 && Finite(d.eventDamage) && d.eventDamage > 0 &&
                    d.restored == d.before && Math.Abs(d.before - d.after - d.eventDamage) <= Math.Max(.0001, d.before / 4194304), "Sparse damage HP/event mismatch");
            }
        }
        static void ValidatePosition(OriginalObservedBodyPosition value)
        {
            Check(value != null && Finite(value.x) && Finite(value.y) && Math.Abs(value.x) < 1048576 && Math.Abs(value.y) < 1048576 &&
                Finite(value.centerDistance) && value.centerDistance >= 0 && Finite(value.moveSpeed) && value.moveSpeed >= 0 && value.order >= 0 &&
                Math.Abs(Math.Sqrt((value.x - 135) * (value.x - 135) + (value.y - 1000) * (value.y - 1000)) - value.centerDistance) < .003, "Invalid native body position");
        }
        public OriginalObservedSparseUnit Unit(string id, int level = 0)
        {
            if (index == null) BuildIndexes();
            return index.TryGetValue(Key(id, level), out var value) ? value.Copy() : new OriginalObservedSparseUnit { id = id, level = level };
        }
        static string Key(string id, int level) => id + "/" + level;
        static bool Finite(double value) => OriginalObservedCatalog.Finite(value);
        static void Check(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }
    }
}

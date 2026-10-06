using System;

namespace Arena.Original
{
    // THORNS_SLOW LiA3.9c/1.26.0.6401, LiAThSl1 schema16, captured campaign
    // 1c524803798037e7a3b15f20b108c39c7c163f2be6be84cf4723d1e0aae308b7.
    // Seven valid rows; armor3 AddAbility failure is excluded. Melee raw13 ->
    // reflection3.25; hfoo armor2 raw12 ->3; H008 armor5.2 raw57 ->11.4.
    // Therefore AEah percentage uses prearmor damage and the spells matrix,
    // ignoring numeric armor. Three ranged hits produced no reflection.
    public sealed class OriginalThornsRules
    {
        public readonly double fraction;
        public OriginalThornsRules(OriginalCombatCatalog catalog, string abilityId, int rank)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected map version.");
            var ability = catalog.Ability(abilityId);
            if (ability == null || ability.Text("code") != "AEah" || rank < 1 || rank > ability.Number("levels"))
                throw new InvalidOperationException("Unresolved thorns family/rank.");
            if (ability.Text("targs" + rank) != "self" || ability.Number("DataB" + rank) != 1)
                throw new InvalidOperationException("Thorns aura target/flat-damage policy is not resolved.");
            fraction = ability.Number("DataA" + rank);
            if (fraction < 0) throw new InvalidOperationException("Negative reflection fraction.");
        }
        public double ReflectedDamage(OriginalNativeCatalog native, double rawMeleeDamage, string attackerDefense)
        {
            if (!OriginalCombatDefinition.IsFinite(rawMeleeDamage) || rawMeleeDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawMeleeDamage));
            return rawMeleeDamage * fraction * OriginalAttackRules.DamageTypeMultiplier(native, "spells", attackerDefense);
        }
    }

    // Aven declarations: AbilityData.slk A0TC:23468, A0TD:23495.
    // The same captured native experiment observes damage0 for sparse A0TC
    // DataA1, damage10*.8 for A0TD versus a hero, first tick hit+.01 then 1s;
    // refreshed hits extend expiry without restarting that periodic phase.
    // Movement reduces250 to225/200. Attack periods are1.85/(1+.06-.1/.2).
    // LiADefP1 schema19 / campaign87cab7a3296b8905fc6249fe58c64208eed857e68e9fd83cc2e572f9cd87be31
    // additionally measures A0TE/A0TF B06L zero periodic damage, .2/.15 speed
    // penalties and1.5/3sec duration. MIXPOIS2 schema54 separately resolves
    // same-BuffID mixtures. CROSSP1 schema77 separately measures cross-BuffID
    // shared state and the weaker-other-buff expiry rule in Session.OnHit.
    public sealed class OriginalPoisonRules
    {
        public const double InitialTickDelay = .01, TickInterval = 1;
        public readonly string abilityId, buffId;
        public readonly double damagePerTick, moveSlow, attackSlow, duration, heroDuration;
        public OriginalPoisonRules(OriginalCombatCatalog catalog, string abilityId, int rank)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected map version.");
            var ability = catalog.Ability(abilityId);
            if (ability == null || ability.Text("code") != "Aven" || rank != 1 ||
                (abilityId != "A0TC" && abilityId != "A0TD" && abilityId != "A0TE" && abilityId != "A0TF"))
                throw new InvalidOperationException("Unobserved poison native timing family/rank.");
            string expectedBuff = abilityId == "A0TE" || abilityId == "A0TF" ? "B06L" : "B06K";
            if (ability.Text("BuffID1") != expectedBuff + "," + expectedBuff || ability.Text("targs1") != "air,ground,organic")
                throw new InvalidOperationException("Unresolved poison targets/buff.");
            this.abilityId = abilityId; buffId = expectedBuff;
            if (ability.TryNumber("DataA1", out var damage, out _)) damagePerTick = damage;
            else if ((abilityId == "A0TC" || abilityId == "A0TE" || abilityId == "A0TF") &&
                Array.Find(ability.fields, f => f.key == "DataA1") == null && ability.overrides.Length == 0)
                damagePerTick = 0; // Explicit native measured zero, never a general missing-field default.
            else throw new InvalidOperationException("Unresolved poison damage.");
            moveSlow = ability.Number("DataB1"); attackSlow = ability.Number("DataC1");
            duration = ability.Number("Dur1"); heroDuration = ability.Number("HeroDur1");
            if (damagePerTick < 0 || moveSlow < 0 || moveSlow >= 1 || attackSlow < 0 || attackSlow >= 1 || duration <= 0 || heroDuration <= 0)
                throw new InvalidOperationException("Invalid poison values.");
        }
    }
}

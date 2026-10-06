using System;

namespace Arena.Original
{
    // Exact A103 rank1 closure. CHREM1 measured instant single/burst helpers;
    // CRIPDMG1 campaign6b1fe1b07ca024606956714c343de4c8a3b976ccaf1050f8588bacf9d493d876
    // measured unchanged movement/attack cadence and reduced weapon damage.
    public sealed class OriginalCrippleRules
    {
        public readonly double damageReduction, duration, heroDuration;
        public OriginalCrippleRules(OriginalCombatCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) throw new ArgumentException("Unexpected map version.");
            var ability = catalog.Ability("A103");
            if (ability == null || ability.Text("code") != "Acri" || ability.Text("BuffID1") != "B08M" ||
                ability.Text("targs1") != "air,enemies,ground,neutral,organic" || ability.Number("levels") != 1 ||
                ability.Number("DataC1") != .5 || ability.Number("Dur1") != 6 || ability.Number("HeroDur1") != 6)
                throw new InvalidOperationException("A103 conflicts with measured native definition.");
            foreach (var field in ability.fields)
                if ((field.key == "DataA1" || field.key == "DataB1") &&
                    (!field.isNumber || field.conflict || field.number != 0))
                    throw new InvalidOperationException("A103 movement/IAS conflicts with native controls.");
            // Binary exceptions must be reconciled explicitly, not ignored in
            // favour of the flat SLK. This exact reviewed definition has none.
            if (ability.overrides.Length != 0) throw new InvalidOperationException("Unreviewed A103 binary override.");
            damageReduction = .5; duration = heroDuration = 6;
        }
        public double WeaponDamage(double rolledBaseAndPrimary, double flatBonus)
        {
            if (!Finite(rolledBaseAndPrimary) || rolledBaseAndPrimary < 0 || !Finite(flatBonus))
                throw new ArgumentOutOfRangeException();
            // Native controls before/after: white56->28,86->43,12->6,13->6;
            // white56+I007flat12 ->28+12. Floor placement is inferred from
            // the odd n008 control; fractional attributes/crit are not proven.
            return Math.Max(0, Math.Floor(rolledBaseAndPrimary * (1 - damageReduction)) + flatBonus);
        }
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}

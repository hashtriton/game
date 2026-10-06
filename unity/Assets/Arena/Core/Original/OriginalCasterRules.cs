using System;

namespace Arena.Original
{
    public enum OriginalCasterFamily
    {
        Pulse, Swap, AntiHeal, OutsideRing, Pull, StrikeSummon, Collapse, EnemyImage,
        ManaBurst, Homing, AllyThrow, DelayedRing, Drag, MovingWave, SleepWave,
        Totem, FrostPulse, SlowAura, SpellCurseAura, ReverseOrderAura
    }

    // Clean declarative transcription of a0/OWv/OZv and their timer callbacks.
    // 3.9c war3map.normalized.j SHA fe69d5ec5087303ac93a696c602b746565ee5e9e52917f18a9de3ee47d6824e4.
    // Missing native helpers are named explicitly; registering an ID is not a
    // claim that its complete Warcraft ability behavior has been implemented.
    public sealed class OriginalCasterRules
    {
        static readonly string[] ids = { "A0Z3", "A0Z4", "A0Z5", "A0Z6", "A0Z7", "A0Z8", "A0Z9", "A0ZA",
            "A0ZB", "A0ZC", "A0ZD", "A0ZE", "A0ZF", "A0ZG", "A0ZH", "A0ZI", "A120", "A121", "A122", "A123",
            "A1DA", "A1DC", "A1DE", "A1DF" };
        public static string[] AbilityIds => (string[])ids.Clone();
        public readonly string abilityId, unresolvedDependency;
        public readonly OriginalCasterFamily family;
        public readonly double warningSeconds, radius, damage, manaCost, cooldown, castRange;
        public readonly int tickCount;
        public readonly bool scriptImplemented, translatedCenter;
        public readonly int sourceLine;

        public OriginalCasterRules(OriginalCombatCatalog catalog, string abilityId)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256)
                throw new ArgumentException("Unexpected caster map version.");
            this.abilityId = abilityId;
            if (Array.IndexOf(ids, abilityId) < 0) throw new InvalidOperationException("Unknown source caster ability " + abilityId);
            var ability = catalog.Ability(abilityId);
            if (ability == null || ability.Text("code") != "ANsi") throw new InvalidOperationException("Unresolved caster base ability " + abilityId);
            manaCost = ability.Number("Cost1"); cooldown = ability.Number("Cool1"); castRange = ability.Number("Rng1");
            if (manaCost < 0 || cooldown < 0 || castRange <= 0) throw new InvalidOperationException("Invalid caster resource declaration.");
            scriptImplemented = true;
            switch (abilityId)
            {
                case "A0Z3": family = OriginalCasterFamily.Pulse; warningSeconds = 2; radius = 400; damage = 50; tickCount = 6; sourceLine = 32950; break;
                case "A121": family = OriginalCasterFamily.Pulse; warningSeconds = 1; radius = 600; damage = 400; tickCount = 6; sourceLine = 34337; break;
                case "A0Z4": family = OriginalCasterFamily.Swap; warningSeconds = 2; radius = 200; damage = 250; sourceLine = 33041; break;
                case "A0Z5":
                    family = OriginalCasterFamily.AntiHeal; warningSeconds = 1.8; radius = 200; tickCount = 333; sourceLine = 33163;
                    // HELPER1 aura_A0ZK: rank1 self aura leaves native movement
                    // and weapon cadence unchanged. Oxv owns the life clamp.
                    var marker = catalog.Ability("A0ZK");
                    if (marker == null || marker.Text("code") != "Aasl" || marker.Text("targs1") != "self")
                        throw new InvalidOperationException("anti-heal-marker-identity-changed");
                    foreach (var key in new[] { "DataA1", "DataB1" })
                    {
                        if (marker.TryNumber(key, out double value, out var state) ? value != 0 : state != null)
                            throw new InvalidOperationException("anti-heal-marker-modifier-conflict:" + key);
                    }
                    break;
                case "A0Z6": family = OriginalCasterFamily.OutsideRing; warningSeconds = 2.5; radius = 400; damage = 50; tickCount = 5; sourceLine = 33235; translatedCenter = true; break;
                case "A0ZI": family = OriginalCasterFamily.OutsideRing; warningSeconds = 2.5; radius = 300; damage = 130; tickCount = 5; sourceLine = 33235; translatedCenter = true; break;
                case "A0Z7": family = OriginalCasterFamily.Pull; warningSeconds = 1.7; radius = 200; damage = 250; sourceLine = 33367; break;
                case "A0Z8": family = OriginalCasterFamily.StrikeSummon; warningSeconds = 1.7; radius = 250; damage = 350; sourceLine = 33459; break;
                case "A0Z9": family = OriginalCasterFamily.Collapse; warningSeconds = 1.6; radius = 300; damage = 500; sourceLine = 33519; break;
                case "A0ZA": family = OriginalCasterFamily.EnemyImage; warningSeconds = 1.6; radius = 300; sourceLine = 33569; break;
                case "A120": family = OriginalCasterFamily.EnemyImage; warningSeconds = 1; radius = 600; sourceLine = 34284; break;
                case "A0ZB": case "A0ZF": family = OriginalCasterFamily.ManaBurst; warningSeconds = 1.5; radius = 400; sourceLine = 33621; break;
                case "A0ZC": family = OriginalCasterFamily.Homing; warningSeconds = 1.5; radius = 350; damage = 900; sourceLine = 33687; break;
                case "A0ZD": family = OriginalCasterFamily.AllyThrow; warningSeconds = 1.5; radius = 350; damage = 200; tickCount = 5; sourceLine = 33793; break;
                case "A0ZE": family = OriginalCasterFamily.DelayedRing; warningSeconds = 2.5; radius = 350; damage = 1000; sourceLine = 33927; translatedCenter = true; break;
                case "A0ZG": family = OriginalCasterFamily.Drag; warningSeconds = 1.4; damage = 8; radius = 400; sourceLine = 34041; break;
                case "A0ZH": family = OriginalCasterFamily.MovingWave; warningSeconds = 1.3; radius = 200; damage = 500; sourceLine = 34155; break;
                case "A122": family = OriginalCasterFamily.SleepWave; warningSeconds = 1; radius = 450; damage = 1500; sourceLine = 34429; break;
                case "A123": family = OriginalCasterFamily.Totem; warningSeconds = 1; radius = 4000; damage = 150; tickCount = 14; sourceLine = 34551; translatedCenter = true; break;
                case "A1DA": family = OriginalCasterFamily.FrostPulse; warningSeconds = 1; radius = 400; damage = 300; tickCount = 6; sourceLine = 34671; break;
                case "A1DC": family = OriginalCasterFamily.SlowAura; warningSeconds = 1; radius = 400; tickCount = 10; sourceLine = 34724; break;
                case "A1DE": family = OriginalCasterFamily.SpellCurseAura; warningSeconds = 1; radius = 500; damage = 1200; tickCount = 10; sourceLine = 34778; break;
                case "A1DF": family = OriginalCasterFamily.ReverseOrderAura; warningSeconds = 1; radius = 500; tickCount = 10; sourceLine = 34831; break;
            }
            if (unresolvedDependency != null) scriptImplemented = false;
        }

        public static OriginalCasterRules ForUnit(OriginalCombatCatalog catalog, string rawcode)
        {
            var definition = catalog.Unit(rawcode);
            string list = definition?.Text("abilList");
            if (list == null) return null;
            OriginalCasterRules result = null;
            foreach (var id in list.Split(','))
                if (Array.IndexOf(ids, id) >= 0)
                {
                    if (result != null) throw new InvalidOperationException("Ambiguous caster dispatcher " + rawcode);
                    result = new OriginalCasterRules(catalog, id);
                }
            return result;
        }
        public static bool TryCastPoint(OriginalCombatCatalog catalog, string rawcode, out double seconds)
        {
            seconds = 0;
            var unit = catalog.Unit(rawcode);
            if (unit.TryNumber("castpt", out seconds, out var state)) return seconds >= 0;
            // CAST1 confirms five missing cells, with .5/.3 native controls.
            // WAVECAST1 separately confirms n009/A046 and n019/A073 at0.
            // A conflicting declaration must never become zero.
            if (state != null || catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256) return false;
            seconds = 0;
            return rawcode == "n05J" || rawcode == "o00C" || rawcode == "n06K" || rawcode == "n02J" || rawcode == "n02O" ||
                rawcode == "n009" || rawcode == "n019";
        }
        public OriginalPoint WarningCenter(OriginalPoint target)
        {
            if (!translatedCenter) return target;
            double angle = Math.Atan2(1000 - target.y, 100 - target.x);
            return new OriginalPoint(target.x + 500 * Math.Cos(angle), target.y + 500 * Math.Sin(angle));
        }
        public static OriginalPoint WarningScatter(OriginalPoint previous, double angle, double distance) =>
            new OriginalPoint(previous.x + distance * Math.Cos(angle) + distance * Math.Sin(angle), previous.y);
        public bool OutsideSafeRadius(OriginalPoint center, OriginalPoint target)
        {
            double x = target.x - center.x, y = target.y - center.y;
            return x * x + y * y >= radius * radius;
        }
        public double ManaBurst(double maximum, double current)
        {
            if (family != OriginalCasterFamily.ManaBurst || !OriginalCombatDefinition.IsFinite(maximum) ||
                !OriginalCombatDefinition.IsFinite(current) || maximum < 0 || current < 0 || current > maximum)
                throw new ArgumentOutOfRangeException(nameof(current));
            return maximum - current * (abilityId == "A0ZB" ? .5 : .7);
        }
    }
}

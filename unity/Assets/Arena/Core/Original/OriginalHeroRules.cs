using System;
using System.Collections.Generic;

namespace Arena.Original
{
    // All distances below are Warcraft world units. Unity presentation converts
    // once at its boundary (the map uses 64 Warcraft units per Unity unit).
    public static class OriginalHeroRules
    {
        public const string Script = "war3map.normalized.j";

        public static void SkillLevel(int level)
        {
            if (level < 1 || level > 3) throw new ArgumentOutOfRangeException(nameof(level));
        }

        // Cm:3874-3912. This scripted proxy is distinct from an attack dice roll.
        public static double AbilityAttack(double primaryAttribute, int attackUpgradeLevel,
            IEnumerable<double> itemAndAbilityBonuses)
        {
            if (!OriginalCombatDefinition.IsFinite(primaryAttribute) || attackUpgradeLevel < 0)
                throw new ArgumentOutOfRangeException();
            double percentage = 1, flat = 0;
            foreach (var value in itemAndAbilityBonuses)
            {
                if (!OriginalCombatDefinition.IsFinite(value)) throw new ArgumentOutOfRangeException();
                if (value <= 3) percentage += value;
                else flat += value;
            }
            return flat + (primaryAttribute * 1.5 + 25 + 5 * attackUpgradeLevel) * percentage;
        }

        // YBv:72785-72820, YHv:72984-73070, YDV:72893-72911.
        public static double KnightMirrorDamage(int level, double abilityAttack)
        {
            SkillLevel(level);
            return 50 * level + abilityAttack * (0.2 + 0.1 * level);
        }

        public static double KnightShieldDamage(int level) { SkillLevel(level); return 20 + 90 * level; }
        public static double KnightShieldRadius(int level) { SkillLevel(level); return 250 + 25 * level; }
        public static double KnightDarkGiftDuration(int level) { SkillLevel(level); return 12.5 + 2.5 * level; }

        public static bool KnightShieldContains(double facingDegrees, double casterToTargetDegrees, double distance, int level)
        {
            SkillLevel(level);
            if (!OriginalCombatDefinition.IsFinite(facingDegrees) ||
                !OriginalCombatDefinition.IsFinite(casterToTargetDegrees) ||
                !OriginalCombatDefinition.IsFinite(distance) || distance < 0) return false;
            var delta = Math.Abs((casterToTargetDegrees + 180 - facingDegrees) % 360);
            if (delta > 180) delta = 360 - delta;
            return distance <= KnightShieldRadius(level) && delta >= 140;
        }

        // ade:83428-83458, aBe:83189-83221, i6e/i7e/i8e/are:82729-82902.
        public static double ArcherPowerShotDamage(int level, double abilityAttack)
        {
            SkillLevel(level);
            return 45 * level + abilityAttack * (0.4 + 0.1 * level);
        }
        public static double ArcherVolleyDamage(double abilityAttack) => 0.6 * abilityAttack;
        public static double ArcherVenomDamage(int level, double abilityAttack)
        {
            SkillLevel(level);
            return abilityAttack * (0.05 + 0.05 * level);
        }
        public static double ArcherIceDamage(int level) { SkillLevel(level); return 50 * level; }
        public static double ArcherFireDamage(int level) { SkillLevel(level); return 15 + 20 * level; }
        public static double ArcherLightningDamage(int level) { SkillLevel(level); return 25 + 25 * level; }

        // Sqv:64142-64196, Swv:64289-64336, S8v:64659-64688, tev:64720-64790.
        public static double PyroVacuumDamage(int level, double distanceCounter)
        {
            SkillLevel(level);
            if (!OriginalCombatDefinition.IsFinite(distanceCounter) || distanceCounter < 0 || distanceCounter > 900)
                throw new ArgumentOutOfRangeException(nameof(distanceCounter));
            var baseDamage = level == 1 ? 50 : level == 2 ? 100 : 200;
            return baseDamage * (1 + distanceCounter / 900);
        }
        public static double PyroSphereDamage(int level, bool upgraded)
        {
            SkillLevel(level);
            return 10 + 20 * (level + (upgraded ? 2 : 0));
        }
        public static double PyroChainDuration(int level) { SkillLevel(level); return 2 + level; }
        public static double PyroMeteorImpact(int level) { SkillLevel(level); return 100 * level; }
        public static double PyroChainAmplifier(int chainLevel)
        {
            if (chainLevel < 0 || chainLevel > 3) throw new ArgumentOutOfRangeException(nameof(chainLevel));
            return 1 + 0.25 * chainLevel;
        }

        // Swv/tev mutate Gk during enumeration. Input order is the caller's
        // observed enumeration order, not sorted by ID or nearest target.
        public static double[] PyroAreaDamage(double baseDamage, IReadOnlyList<bool> chainedTargets, int chainLevel)
        {
            if (chainLevel < 0 || chainLevel > 3) throw new ArgumentOutOfRangeException(nameof(chainLevel));
            var result = new double[chainedTargets.Count];
            var damage = baseDamage;
            for (var i = 0; i < result.Length; i++)
            {
                if (chainedTargets[i]) damage *= PyroChainAmplifier(chainLevel);
                result[i] = damage;
            }
            return result;
        }
    }

    public enum OriginalBowElement { None, Venom, Ice, Fire, Dark, Lightning }

    // aOe/aXe/aie:82903-83090. Native orb stacking remains the combat engine's
    // responsibility; this class tracks the source trigger's independent state.
    public sealed class OriginalArcherBow
    {
        public OriginalBowElement NextElement { get; private set; } = OriginalBowElement.Venom;
        public OriginalBowElement ActiveElement { get; private set; }
        public int Charges { get; private set; }
        public int ComboSum { get; private set; }
        public double CycleRemaining { get; private set; }
        private double volleyReset = -1;

        public bool Activate()
        {
            if (CycleRemaining > 0) return false;
            ActiveElement = NextElement;
            Charges = 5;
            ComboSum += 3;
            CycleRemaining = 18.9;
            return true;
        }

        public void PowerShotStarted() { ComboSum += 1; }
        public void VolleyStarted() { ComboSum += 2; volleyReset = 5; }
        public void PowerShotFinished() { ComboSum = 0; }
        public bool PowerShotSplits(int ultimateLevel) => ComboSum == 3 && ultimateLevel >= 1;
        public bool PowerShotUsesElement(int ultimateLevel) => ComboSum == 4 && ultimateLevel >= 2;
        public bool VolleyUsesElement(int ultimateLevel) => ComboSum == 5 && ultimateLevel >= 3;

        public OriginalBowElement SuccessfulAttack()
        {
            // aie only enters its counter branch while an orb ability exists.
            if (ActiveElement == OriginalBowElement.None) return OriginalBowElement.None;
            if (Charges > 0)
            {
                Charges--;
                return ActiveElement;
            }
            // The original clears the orb on the NEXT qualifying attack.
            ActiveElement = OriginalBowElement.None;
            ComboSum = 0;
            return OriginalBowElement.None;
        }

        public void Advance(double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (CycleRemaining > 0)
            {
                CycleRemaining = Math.Max(0, CycleRemaining - seconds);
                if (CycleRemaining == 0)
                    NextElement = NextElement == OriginalBowElement.Lightning ? OriginalBowElement.Venom : NextElement + 1;
            }
            if (volleyReset >= 0)
            {
                volleyReset -= seconds;
                if (volleyReset <= 0) { ComboSum = 0; volleyReset = -1; }
            }
        }
    }

    // SQv/Sqv:64142-64230. Manual detonation counts one more tick of distance
    // without moving, exactly as the trigger does before checking its flag.
    public sealed class OriginalPyroVacuum
    {
        public int Level { get; }
        public int DistanceCounter { get; private set; }
        public int TravelDistance { get; private set; }
        public bool Detonated { get; private set; }
        public double Damage => Detonated ? OriginalHeroRules.PyroVacuumDamage(Level, DistanceCounter) : 0;
        private bool moving = true;
        private double time;
        private int ticks;

        public OriginalPyroVacuum(int level) { OriginalHeroRules.SkillLevel(level); Level = level; }
        public void Detonate() { moving = false; }

        public void Advance(double seconds)
        {
            if (!OriginalCombatDefinition.IsFinite(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            time += seconds;
            while (!Detonated && (ticks + 1) * 0.04 <= time + 1e-10)
            {
                ticks++;
                DistanceCounter += 25;
                if (moving) TravelDistance += 25;
                Detonated = !moving || DistanceCounter >= 900;
            }
        }
    }
}

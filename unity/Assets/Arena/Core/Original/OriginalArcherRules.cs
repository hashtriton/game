using System;
using System.Collections.Generic;

namespace Arena.Original
{
    // aDe83459..83467 gives A0N6 on A0AC learning. A0N6 is native AIat.
    // Cm has its own Em registry and must not receive this weapon bonus twice.
    public static class OriginalArcherWeaponBonus
    {
        public static OriginalHeroStatsSnapshot Apply(OriginalCombatCatalog catalog, OriginalHeroStatsSnapshot baseline, int rank)
        {
            if (catalog == null || baseline == null) throw new ArgumentNullException();
            if (catalog.sourceSha256 != OriginalNativeCatalog.ExpectedMapSha256 || baseline.heroId != "N0A0")
                throw new ArgumentException("Expected the original Archer profile.");
            if (rank < 0 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            var result = baseline.Copy();
            if (rank == 0) return result;
            var ability = catalog.Ability("A0N6");
            if (ability == null || ability.Text("code") != "AIat") throw new InvalidOperationException("Unknown Archer native weapon bonus.");
            double bonus = ability.Number("DataA" + rank);
            if (bonus < 0) throw new InvalidOperationException("Invalid Archer weapon bonus.");
            result.itemAttackDamageBonus += bonus;
            result.attackMinimum.value = result.attackMinimum.Require() + bonus;
            result.attackMaximum.value = result.attackMaximum.Require() + bonus;
            var source = new List<string>(result.sources ?? Array.Empty<string>());
            source.Add("aDe83459..83467/A0N6 AIat DataA" + rank + "/declaration"); result.sources = source.ToArray();
            return result;
        }
    }
    public readonly struct OriginalArcherCandidate
    {
        public readonly int entityId;
        public readonly OriginalPoint position;
        public readonly double health;
        public readonly bool enemy, invisible, structure, mechanical, dead, excludedAbility;
        public OriginalArcherCandidate(int entityId, OriginalPoint position, double health, bool enemy = true,
            bool invisible = false, bool structure = false, bool mechanical = false, bool dead = false, bool excludedAbility = false)
        { this.entityId = entityId; this.position = position; this.health = health; this.enemy = enemy; this.invisible = invisible;
          this.structure = structure; this.mechanical = mechanical; this.dead = dead; this.excludedAbility = excludedAbility; }
    }
    public enum OriginalArcherEventKind { DestructableSweep, ForcedPosition, ArrowLaunched, Damage, Completed }
    public sealed class OriginalArcherEffectEvent
    {
        public OriginalArcherEventKind kind;
        public int entityId;
        public OriginalPoint position;
        public double damage;
        public bool applyElement;
        public OriginalBowElement element;
    }

    // LiA3.9c aBe83189..83221 / aNe83144..83178 / aRe83091..83143.
    // The host supplies its deterministic enumeration. Warcraft's native group
    // order and private render objects are not reproduced by this pure kernel.
    public sealed class OriginalArcherVolleyRules
    {
        public const double TickSeconds = .03, Range = 900, StepDistance = 24;
        readonly OriginalPoint origin;
        readonly double damage;
        readonly OriginalBowElement element;
        readonly List<int> targets = new List<int>();
        double distance;
        public bool Completed => targets.Count == 0;
        public int[] Targets => targets.ToArray();
        public OriginalArcherVolleyRules(OriginalPoint origin, double abilityAttack, OriginalBowElement element,
            IReadOnlyList<OriginalArcherCandidate> candidates)
        {
            OriginalArcherPowerShotRules.Point(origin);
            if (!OriginalCombatDefinition.IsFinite(abilityAttack) || abilityAttack < 0 || !Enum.IsDefined(typeof(OriginalBowElement), element))
                throw new ArgumentOutOfRangeException();
            OriginalArcherPowerShotRules.Candidates(candidates);
            this.origin = origin; damage = OriginalHeroRules.ArcherVolleyDamage(abilityAttack); this.element = element;
            foreach (var c in candidates)
            {
                double dx = c.position.x - origin.x, dy = c.position.y - origin.y;
                // Uk is life < .405 OR native dead. This predicate deliberately
                // differs from ace's strict life > .405 filter.
                if (!c.enemy || c.invisible || c.structure || c.dead || c.health < .405 || c.excludedAbility || dx * dx + dy * dy > Range * Range) continue;
                targets.Add(c.entityId); if (targets.Count == 6) break;
            }
        }
        public OriginalArcherEffectEvent[] Tick(Func<int, OriginalPoint?> currentPosition, int comboSum, int ultimateRank)
        {
            if (currentPosition == null) throw new ArgumentNullException(nameof(currentPosition));
            if (ultimateRank < 0 || ultimateRank > 3) throw new ArgumentOutOfRangeException(nameof(ultimateRank));
            if (Completed) return Array.Empty<OriginalArcherEffectEvent>();
            var positions = new OriginalPoint?[targets.Count];
            for (int i = 0; i < targets.Count; i++)
            { positions[i] = currentPosition(targets[i]); if (positions[i].HasValue) OriginalArcherPowerShotRules.Point(positions[i].Value); }
            var result = new List<OriginalArcherEffectEvent>(); distance += StepDistance;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                if (!positions[i].HasValue) { targets.RemoveAt(i); continue; }
                var point = positions[i].Value;
                double dx = point.x - origin.x, dy = point.y - origin.y;
                if (distance >= Math.Sqrt(dx * dx + dy * dy))
                {
                    result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.Damage, entityId = targets[i],
                        position = point, damage = damage, element = element, applyElement = comboSum == 5 && ultimateRank >= 3 });
                    targets.RemoveAt(i);
                }
            }
            // Reverse iteration supports removals; publish native caller order.
            result.Reverse();
            if (Completed) result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.Completed });
            return result.ToArray();
        }
    }

    // ade83428..83458 / aCe83353..83427 / ace83222..83352.
    // Native cast resource/timing and elemental helper effects are separate.
    // No target takes damage while arrows travel: ace retains a shared group
    // and applies one hit per retained unit when distance first exceeds700.
    public sealed class OriginalArcherPowerShotRules
    {
        public const double TickSeconds = .03, StepDistance = 36, HitRadius = 125;
        readonly int casterId;
        readonly OriginalPoint targetPoint;
        readonly double facingRadians, damage;
        readonly List<OriginalPoint> arrows = new List<OriginalPoint>();
        readonly List<double> directions = new List<double>();
        readonly List<int> retained = new List<int>();
        readonly HashSet<int> retainedSet = new HashSet<int>();
        int ticks, flightTicks;
        public bool Completed { get; private set; }
        public bool Launched => arrows.Count != 0;
        public int[] Targets => retained.ToArray();
        public OriginalPoint[] ArrowPositions => arrows.ToArray();
        public OriginalArcherPowerShotRules(int casterId, int rank, double abilityAttack, double facingDegrees, OriginalPoint targetPoint)
        {
            if (casterId <= 0 || !OriginalCombatDefinition.IsFinite(abilityAttack) || abilityAttack < 0 ||
                !OriginalCombatDefinition.IsFinite(facingDegrees) || facingDegrees < 0 || facingDegrees >= 360)
                throw new ArgumentOutOfRangeException();
            OriginalHeroRules.SkillLevel(rank); Point(targetPoint);
            this.casterId = casterId; this.targetPoint = targetPoint; facingRadians = facingDegrees * Math.PI / 180;
            damage = OriginalHeroRules.ArcherPowerShotDamage(rank, abilityAttack);
        }
        public OriginalArcherEffectEvent[] Tick(OriginalPoint casterPosition, IReadOnlyList<OriginalArcherCandidate> candidates,
            int comboSum, int ultimateRank, OriginalBowElement currentElement)
        {
            Point(casterPosition); Candidates(candidates);
            if (ultimateRank < 0 || ultimateRank > 3 || !Enum.IsDefined(typeof(OriginalBowElement), currentElement))
                throw new ArgumentOutOfRangeException();
            if (Completed) return Array.Empty<OriginalArcherEffectEvent>();
            var result = new List<OriginalArcherEffectEvent>(); ticks++;
            if (!Launched)
            {
                if (ticks * StepDistance < 300)
                {
                    result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.DestructableSweep, entityId = casterId, position = casterPosition });
                    var next = new OriginalPoint(casterPosition.x - StepDistance * Math.Cos(facingRadians),
                        casterPosition.y - StepDistance * Math.Sin(facingRadians));
                    if (OriginalShieldBashRules.AllowsForcedPoint(next))
                        result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.ForcedPosition, entityId = casterId, position = next });
                    return result.ToArray();
                }
                double direction = Math.Atan2(targetPoint.y - casterPosition.y, targetPoint.x - casterPosition.x);
                var start = new OriginalPoint(casterPosition.x + 30 * Math.Cos(direction), casterPosition.y + 30 * Math.Sin(direction));
                arrows.Add(start); directions.Add(direction);
                if (comboSum == 3 && ultimateRank >= 1)
                {
                    arrows.Add(start); directions.Add(direction - 20 * Math.PI / 180);
                    arrows.Add(start); directions.Add(direction + 20 * Math.PI / 180);
                }
                foreach (var point in arrows) result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.ArrowLaunched, position = point, element = currentElement });
                return result.ToArray();
            }
            flightTicks++;
            for (int a = 0; a < arrows.Count; a++)
            {
                arrows[a] = new OriginalPoint(arrows[a].x + StepDistance * Math.Cos(directions[a]), arrows[a].y + StepDistance * Math.Sin(directions[a]));
                foreach (var c in candidates)
                {
                    double dx = c.position.x - arrows[a].x, dy = c.position.y - arrows[a].y;
                    if (!c.enemy || c.structure || c.mechanical || c.health <= .405 || retainedSet.Contains(c.entityId) || dx * dx + dy * dy > HitRadius * HitRadius) continue;
                    retainedSet.Add(c.entityId); retained.Add(c.entityId);
                }
            }
            if (flightTicks * StepDistance > 700)
            {
                foreach (int id in retained)
                    result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.Damage, entityId = id,
                        damage = damage, element = currentElement, applyElement = comboSum == 4 && ultimateRank >= 2 });
                Completed = true; result.Add(new OriginalArcherEffectEvent { kind = OriginalArcherEventKind.Completed, entityId = casterId });
            }
            return result.ToArray();
        }
        internal static void Point(OriginalPoint p)
        { if (!OriginalCombatDefinition.IsFinite(p.x) || !OriginalCombatDefinition.IsFinite(p.y)) throw new ArgumentOutOfRangeException(); }
        internal static void Candidates(IReadOnlyList<OriginalArcherCandidate> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var ids = new HashSet<int>();
            foreach (var value in values)
            { Point(value.position); if (value.entityId <= 0 || !ids.Add(value.entityId) || !OriginalCombatDefinition.IsFinite(value.health) || value.health < 0) throw new ArgumentException("Invalid Archer candidate."); }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalShieldBashEventKind { DisablePathingAndMovement, Damage, CrippleOrder, ForcedPosition, DestructableSweep, RestoreDefaultMovement, Completed }
    public enum OriginalTriggeredDamageMode { SpellNormal = 1, SpellMagic = 2, ChaosUniversal = 3 }

    public readonly struct OriginalShieldBashCandidate
    {
        public readonly int entityId;
        public readonly OriginalPoint position;
        public readonly double health;
        public readonly bool enemy, structure;
        public OriginalShieldBashCandidate(int entityId, OriginalPoint position, double health, bool enemy, bool structure)
        { this.entityId = entityId; this.position = position; this.health = health; this.enemy = enemy; this.structure = structure; }
    }

    // These are host effect instructions, never client damage commands. A native
    // cast driver must resolve mana/cooldown/lifecycle before constructing this
    // source effect. CrippleOrder is an order request, not proof of a buff hit.
    public sealed class OriginalShieldBashEvent
    {
        public OriginalShieldBashEventKind kind;
        public int entityId;
        public bool hasPosition;
        public OriginalPoint position;
        public double damage;
        public OriginalTriggeredDamageMode damageMode;
    }

    // LiA3.9c map SHA256 02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34.
    // Declarative reconstruction of YHv72984..73070/Ygv72925..72980, eL2699..2744,
    // iL2820..2841 and gK2381..2394 in war3map.rawcodes.j. Native ANcl/Acri,
    // damage flags/armor and engine enumeration/forced-position behavior are
    // separate dependencies. This does not declare a complete usable A102 cast.
    public sealed class OriginalShieldBashRules
    {
        public const double TickSeconds = .03;
        public const int DisplacementTicks = 33;
        public const string CrippleAbilityId = "A103";
        public const int CrippleOrderId = 852189; // JASS $D00DD.
        readonly int casterId;
        readonly int[] targets;
        readonly double stepDistance;
        readonly double damage;
        readonly OriginalTriggeredDamageMode mode;
        int ticks;
        bool initialEventsRead;
        public bool Completed => ticks > DisplacementTicks;
        public int[] Targets => (int[])targets.Clone();

        public OriginalShieldBashRules(int casterId, int rank, bool darkGifts, OriginalPoint casterPosition,
            double facingDegrees, IReadOnlyList<OriginalShieldBashCandidate> candidates)
        {
            if (casterId <= 0 || rank < 1 || rank > 3) throw new ArgumentOutOfRangeException();
            Point(casterPosition); Finite(facingDegrees);
            if (facingDegrees < 0 || facingDegrees >= 360) throw new ArgumentOutOfRangeException(nameof(facingDegrees));
            if (candidates == null) throw new ArgumentNullException(nameof(candidates));
            this.casterId = casterId; damage = 20 + 90 * rank;
            mode = darkGifts ? OriginalTriggeredDamageMode.ChaosUniversal : OriginalTriggeredDamageMode.SpellNormal;
            stepDistance = (darkGifts ? 225 : 150) * TickSeconds;
            var selected = new List<int>(); var ids = new HashSet<int>();
            double range = 250 + 25 * rank;
            foreach (var candidate in candidates)
            {
                if (candidate.entityId <= 0 || !ids.Add(candidate.entityId)) throw new ArgumentException("Invalid or duplicate candidate identity.");
                Point(candidate.position); Finite(candidate.health);
                if (!candidate.enemy || candidate.structure || candidate.health <= .405) continue;
                double dx = candidate.position.x - casterPosition.x, dy = candidate.position.y - casterPosition.y;
                if (dx * dx + dy * dy > range * range) continue;
                // Yhv uses the reverse bearing with the map's explicit degrees
                // constant; a difference >=140 selects its forward 80deg cone.
                double reverseBearing = 57.2958 * Math.Atan2(casterPosition.y - candidate.position.y, casterPosition.x - candidate.position.x);
                double difference = facingDegrees - reverseBearing;
                if (difference < -180) difference += 360;
                else if (difference > 180) difference -= 360;
                if (Math.Abs(difference) >= 140) selected.Add(candidate.entityId);
            }
            // Caller supplies a deterministic enumeration order. Native group
            // order is unobserved and must not be claimed to equal sorted IDs.
            targets = selected.ToArray();
        }

        public OriginalShieldBashEvent[] BeginEvents()
        {
            if (initialEventsRead) return Array.Empty<OriginalShieldBashEvent>();
            initialEventsRead = true;
            var result = new List<OriginalShieldBashEvent>();
            foreach (int id in targets)
            {
                result.Add(Event(OriginalShieldBashEventKind.DisablePathingAndMovement, id));
                result.Add(new OriginalShieldBashEvent { kind = OriginalShieldBashEventKind.Damage, entityId = id, damage = damage, damageMode = mode });
                result.Add(Event(OriginalShieldBashEventKind.CrippleOrder, id));
            }
            return result.ToArray();
        }

        // Called once per original .03 timer firing. Positions are read afresh
        // because the source caster and retained targets can move in between.
        // A removed target is null; a dead-but-present target is still moved.
        // Resolve all inputs before advancing state so a bad host observation
        // cannot publish only half a timer step.
        public OriginalShieldBashEvent[] Tick(OriginalPoint casterPosition, Func<int, OriginalPoint?> currentPosition)
        {
            if (!initialEventsRead) throw new InvalidOperationException("Consume initial effect events before ticking.");
            if (Completed) return Array.Empty<OriginalShieldBashEvent>();
            Point(casterPosition);
            if (currentPosition == null) throw new ArgumentNullException(nameof(currentPosition));
            var positions = new OriginalPoint?[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            { positions[i] = currentPosition(targets[i]); if (positions[i].HasValue) Point(positions[i].Value); }
            var result = new List<OriginalShieldBashEvent>();
            if (ticks < DisplacementTicks)
                for (int i = 0; i < targets.Length; i++)
                {
                    if (!positions[i].HasValue) continue;
                    var position = positions[i].Value;
                    double angle = Math.Atan2(position.y - casterPosition.y, position.x - casterPosition.x);
                    var next = new OriginalPoint(position.x + stepDistance * Math.Cos(angle), position.y + stepDistance * Math.Sin(angle));
                    if (!AllowsForcedPoint(next)) result.Add(Event(OriginalShieldBashEventKind.RestoreDefaultMovement, targets[i]));
                    else
                    {
                        next = ClampPlayable(next);
                        result.Add(PositionEvent(OriginalShieldBashEventKind.ForcedPosition, targets[i], next));
                        result.Add(PositionEvent(OriginalShieldBashEventKind.DestructableSweep, targets[i], next));
                    }
                }
            else
            {
                for (int i = 0; i < targets.Length; i++)
                    if (positions[i].HasValue) result.Add(Event(OriginalShieldBashEventKind.RestoreDefaultMovement, targets[i]));
                result.Add(Event(OriginalShieldBashEventKind.Completed, casterId));
            }
            ticks++; return result.ToArray();
        }

        // Rectangles zV..CE, source assignments84343..84363, inclusive eL.
        static readonly double[,] excluded = {
            {-2944,2688,-224,3712}, {-224,2912,384,3712}, {384,2688,2816,3712},
            {1792,-2112,2816,960}, {-2944,-1824,1088,-1024}, {-2944,832,-2048,2688},
            {-2944,320,-2272,832}, {-2944,-1024,-2048,320}, {-1376,-3968,-864,-1824},
            {864,-3968,1088,-1824}, {-2944,-3968,-2016,-1824}, {-2016,-2304,-1376,-1824},
            {-2016,-3968,-1376,-3200}, {-864,-3968,864,-3584}, {1792,1472,2816,2688},
            {1984,960,2816,1472}, {-1920,1344,-1600,1472}, {1088,-2112,1792,-1024},
            {1088,-3488,2816,-3072}, {2112,-3072,2816,-2112}, {1088,-3968,1472,-3488}
        };
        public static bool AllowsForcedPoint(OriginalPoint point)
        {
            Point(point);
            for (int i = 0; i < excluded.GetLength(0); i++)
                if (point.x >= excluded[i, 0] && point.y >= excluded[i, 1] && point.x <= excluded[i, 2] && point.y <= excluded[i, 3]) return false;
            // IO84447 and center(0,-2700), inclusive radius exclusion in eL.
            if (point.x >= -864 && point.x <= 864 && point.y >= -3584 && point.y <= -1824 &&
                point.x * point.x + (point.y + 2700) * (point.y + 2700) >= 820 * 820) return false;
            return true;
        }
        public static OriginalPoint ClampPlayable(OriginalPoint point)
        {
            Point(point);
            // main84222 camera bounds, native Blizzard.j9889 removes margins;
            // iL/nL add25 to each minimum and subtract25 from each maximum.
            return new OriginalPoint(Math.Max(-2535, Math.Min(2407, point.x)), Math.Max(-4071, Math.Min(3431, point.y)));
        }
        public static bool SweepDestroys(string rawcode, OriginalPoint movedUnit, OriginalPoint destructable)
        {
            Point(movedUnit); Point(destructable);
            double dx = destructable.x - movedUnit.x, dy = destructable.y - movedUnit.y;
            if (Math.Abs(dx) > 64 || Math.Abs(dy) > 64) return false;
            return rawcode == "LTba" || rawcode == "LTbs" || rawcode == "LTbr" && dx * dx + dy * dy <= 64 * 64;
        }
        static OriginalShieldBashEvent Event(OriginalShieldBashEventKind kind, int id) => new OriginalShieldBashEvent { kind = kind, entityId = id };
        static OriginalShieldBashEvent PositionEvent(OriginalShieldBashEventKind kind, int id, OriginalPoint position) =>
            new OriginalShieldBashEvent { kind = kind, entityId = id, hasPosition = true, position = position };
        static void Point(OriginalPoint point) { Finite(point.x); Finite(point.y); }
        static void Finite(double value)
        { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value)); }
    }
}

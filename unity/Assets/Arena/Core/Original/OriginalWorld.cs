using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public interface IOriginalWorldNavigation
    {
        OriginalPoint HeroSpawn { get; }
        long NavigationRevision { get; }
        bool IsWalkable(double x, double y, double radius);
        bool SegmentClear(OriginalPoint from, OriginalPoint to, double radius);
        OriginalPoint[] FindPath(OriginalPoint from, OriginalPoint to, double radius);
        bool SetDoodadAlive(int editorId, bool alive);
    }

    [Serializable]
    public sealed class OriginalWorldUnitProfile
    {
        public double moveSpeed, collisionRadius, maxHealth, maxMana;
        internal OriginalWorldUnitProfile Copy() => (OriginalWorldUnitProfile)MemberwiseClone();
    }
    public enum OriginalWorldOrder { None, Move, AttackTarget }
    public enum OriginalWorldTargetKind { None, Unit, Doodad }
    public enum OriginalWorldUnitKind { Hero, Enemy, Illusion, Summon }
    public enum OriginalImageFactory { None, KnightMirror, EnemyWand, BossMirror, ItemWand }
    public enum OriginalWorldRemovalReason { Explicit, Expired, Replaced }
    [Serializable]
    public sealed class OriginalWorldUnitView
    {
        public int entityId, ownerSlot;
        public OriginalWorldUnitKind kind;
        public int sourceHeroEntityId;
        public int copySourceEntityId;
        public OriginalImageFactory imageFactory;
        public string rawcode;
        public OriginalPoint position, destination;
        public OriginalWorldUnitProfile profile;
        public double health, mana;
        public OriginalWorldOrder order;
        public OriginalWorldTargetKind targetKind;
        public int targetId;
        public long attackSequence;
        public long castSequence;
        public bool hasFacing;
        public double facingDegrees;
        public bool approaching, holding;
        public bool paused, invulnerable, hidden, pathingDisabled;
        // Session publishes observer visibility separately from physical hiding.
        // Bit0 is owner1; bit7 is owner8. Raw world views have no invisibility rule.
        public bool invisible;
        public int visibleToOwners = 255;
    }
    [Serializable]
    public sealed class OriginalWorldDoodadView
    {
        public int editorId;
        public string rawcode;
        public OriginalPoint position;
        public double maxHealth, health;
        public bool dynamic, invulnerable;
        public double facingDegrees, scale = 1;
    }
    [Serializable]
    public sealed class OriginalWorldSnapshot
    {
        public long revision, navigationRevision;
        public double time;
        public OriginalWorldUnitView[] units;
        public OriginalWorldDoodadView[] doodads;
    }
    public enum OriginalWorldEventKind { UnitAdded, UnitRemoved, DoodadDamaged, DoodadDestroyed, UnitDamaged, UnitDied }
    [Serializable]
    public sealed class OriginalWorldEvent
    {
        public long sequence;
        public double time, health;
        public OriginalWorldEventKind kind;
        public OriginalWorldRemovalReason removalReason;
        public int entityId, editorId;
        public string rawcode;
    }

    public sealed partial class OriginalWorld
    {
        sealed class Unit
        {
            internal int id, owner, targetId, pathIndex;
            internal OriginalWorldUnitKind kind;
            internal int sourceHeroEntityId;
            internal int copySourceEntityId;
            internal OriginalImageFactory imageFactory;
            internal string rawcode;
            internal OriginalWorldUnitProfile profile;
            internal double health, mana;
            internal OriginalWorldOrder order;
            internal OriginalWorldTargetKind targetKind;
            internal OriginalPoint position, destination;
            internal OriginalPoint[] path = Array.Empty<OriginalPoint>();
            internal long pathRevision = -1;
            internal long attackSequence;
            internal long castSequence;
            internal bool hasFacing;
            internal double facingDegrees;
            internal bool approaching, holding;
            internal bool paused, invulnerable, hidden, pathingDisabled;
            internal readonly HashSet<string> temporaryInvulnerability = new HashSet<string>(StringComparer.Ordinal);
        }
        readonly IOriginalWorldNavigation navigation;
        readonly OriginalMotion motion;
        readonly SortedDictionary<int, Unit> units = new SortedDictionary<int, Unit>();
        readonly SortedDictionary<int, OriginalWorldDoodadView> doodads = new SortedDictionary<int, OriginalWorldDoodadView>();
        readonly List<OriginalWorldEvent> events = new List<OriginalWorldEvent>();
        long revision, eventSequence;
        double time;
        const double Epsilon = 1e-7;
        public double Clock => time;

        // Single host simulation thread, Warcraft XY units. Profiles are supplied
        // by verified source rules. This registry provides no AI, attack damage,
        // explosion effects, bounty, native pathfinder or Warcraft RNG emulation.
        public OriginalWorld(IOriginalWorldNavigation navigation)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            motion = new OriginalMotion(navigation.IsWalkable, navigation.SegmentClear);
        }
        public static int HeroEntityId(int slot)
        {
            if (slot < 1 || slot > 8) throw new ArgumentOutOfRangeException(nameof(slot));
            return slot;
        }
        public static int EnemyEntityId(int matchEntityId)
        {
            if (matchEntityId < 1) throw new ArgumentOutOfRangeException(nameof(matchEntityId));
            return checked(1000 + matchEntityId);
        }
        public void AddUnit(int entityId, int ownerSlot, string rawcode, OriginalWorldUnitProfile profile, OriginalPoint position)
        {
            if (ownerSlot > 0 && entityId != ownerSlot || ownerSlot == 0 && (entityId <= 1000 || entityId >= 1000000000 ||
                entityId >= FirstSummonEntityId && entityId <= LastSummonEntityId))
                throw new ArgumentException("Canonical heroes, source enemies and summons use distinct world identities.", nameof(entityId));
            ValidateProfile(profile);
            Add(entityId, ownerSlot, rawcode, profile, position, profile.maxHealth, profile.maxMana,
                ownerSlot > 0 ? OriginalWorldUnitKind.Hero : OriginalWorldUnitKind.Enemy, 0);
        }
        // The caller supplies measured summon vitality and a copied source
        // profile. Ancestry identifies ownership, never a live stats alias.
        public void AddIllusion(int entityId, int sourceHeroEntityId, OriginalWorldUnitProfile profile,
            OriginalPoint position, double health, double mana, bool hidden = false)
        {
            if (entityId < 1000000000) throw new ArgumentOutOfRangeException(nameof(entityId));
            if (!units.TryGetValue(sourceHeroEntityId, out var source) || source.kind != OriginalWorldUnitKind.Hero ||
                source.owner < 1 || source.id != HeroEntityId(source.owner))
                throw new ArgumentException("An illusion needs a canonical source hero.", nameof(sourceHeroEntityId));
            Add(entityId, source.owner, source.rawcode, profile, position, health, mana, OriginalWorldUnitKind.Illusion, sourceHeroEntityId, hidden);
            units[entityId].copySourceEntityId = sourceHeroEntityId;
            units[entityId].imageFactory = OriginalImageFactory.KnightMirror;
        }
        void Add(int entityId, int ownerSlot, string rawcode, OriginalWorldUnitProfile profile, OriginalPoint position,
            double health, double mana, OriginalWorldUnitKind kind, int sourceHeroEntityId, bool hidden = false)
        {
            if (entityId <= 0 || ownerSlot < 0 || ownerSlot > 8) throw new ArgumentOutOfRangeException(nameof(entityId));
            if (units.ContainsKey(entityId)) throw new ArgumentException("Duplicate world entity identity.", nameof(entityId));
            Rawcode(rawcode); Coordinates(position); ValidateProfile(profile);
            if (!Finite(health) || health <= 0 || health > profile.maxHealth || !Finite(mana) || mana < 0 || mana > profile.maxMana)
                throw new ArgumentOutOfRangeException(nameof(health));
            var unit = new Unit { id = entityId, owner = ownerSlot, rawcode = rawcode, profile = profile.Copy(),
                health = health, mana = mana, position = position, destination = position,
                kind = kind, sourceHeroEntityId = sourceHeroEntityId, hidden = hidden };
            // Motion validates terrain and all existing bodies before mutation.
            if (!hidden) motion.Add(entityId, position.x, position.y, profile.collisionRadius);
            units.Add(entityId, unit); revision++;
            Emit(OriginalWorldEventKind.UnitAdded, entityId: entityId, rawcode: rawcode, health: unit.health);
        }
        public bool RemoveUnit(int entityId, OriginalWorldRemovalReason reason = OriginalWorldRemovalReason.Explicit)
        {
            if (!Enum.IsDefined(typeof(OriginalWorldRemovalReason), reason)) throw new ArgumentOutOfRangeException(nameof(reason));
            if (!units.TryGetValue(entityId, out var unit)) return false;
            // Summons and images retain provenance without a live donor alias.
            motion.Remove(entityId); units.Remove(entityId);
            ClearTargets(OriginalWorldTargetKind.Unit, entityId); revision++;
            Emit(OriginalWorldEventKind.UnitRemoved, entityId: entityId, rawcode: unit.rawcode, removalReason: reason);
            return true;
        }
        // Caller already resolved armor, immunity, evasion and source effects.
        // Death retains a snapshot entry, but no longer occupies a motion body.
        public bool ApplyUnitDamage(int entityId, double damage)
        {
            if (!Finite(damage) || damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (damage == 0 || !units.TryGetValue(entityId, out var unit) || unit.health <= 0 || EffectiveInvulnerability(unit)) return false;
            unit.health = Math.Max(0, unit.health - damage); revision++;
            Emit(OriginalWorldEventKind.UnitDamaged, entityId: entityId, rawcode: unit.rawcode, health: unit.health);
            if (unit.health == 0) CompleteDeath(unit);
            return true;
        }
        // Trusted source KillUnit semantics, distinct from resolved damage.
        // Invulnerability prevents damage but does not prevent this explicit kill.
        public bool ForceUnitDeath(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0) return false;
            unit.health = 0; revision++;
            CompleteDeath(unit); return true;
        }
        void CompleteDeath(Unit unit)
        {
            unit.temporaryInvulnerability.Clear();
            unit.holding = false;
            motion.Remove(unit.id); ClearOrder(unit); ClearTargets(OriginalWorldTargetKind.Unit, unit.id);
            Emit(OriginalWorldEventKind.UnitDied, entityId: unit.id, rawcode: unit.rawcode);
        }
        // The host supplies both maxima and current values after resolving the
        // source change policy. This method invents no healing or ratio rule.
        public bool UpdateProfile(int entityId, OriginalWorldUnitProfile profile, double health, double mana)
        {
            ValidateProfile(profile);
            if (!Finite(health) || !Finite(mana) || health < 0 || health > profile.maxHealth || mana < 0 || mana > profile.maxMana)
                throw new ArgumentOutOfRangeException(nameof(health));
            if (!units.TryGetValue(entityId, out var unit) || (unit.health > 0) != (health > 0)) return false;
            bool radiusChanged = profile.collisionRadius != unit.profile.collisionRadius;
            if (unit.health > 0 && !unit.hidden && !unit.pathingDisabled && radiusChanged && !Free(unit.position, profile.collisionRadius, entityId)) return false;
            if (unit.health > 0 && !unit.hidden && !unit.pathingDisabled && radiusChanged)
            {
                motion.Remove(entityId);
                motion.Add(entityId, unit.position.x, unit.position.y, profile.collisionRadius);
            }
            unit.profile = profile.Copy(); unit.health = health; unit.mana = mana;
            if (radiusChanged) unit.pathRevision = -1;
            revision++; return true;
        }
        public bool Relocate(int entityId, OriginalPoint position, bool fullHealth = false, bool fullMana = false)
        {
            Coordinates(position);
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 && !fullHealth ||
                !(unit.hidden ? navigation.IsWalkable(position.x, position.y, unit.profile.collisionRadius) : Free(position, unit.profile.collisionRadius, entityId))) return false;
            // Navigation is stable within this single-threaded operation. All
            // rejection checks precede changes to the body, health or position.
            motion.Remove(entityId);
            if (!unit.hidden && !unit.pathingDisabled) motion.Add(entityId, position.x, position.y, unit.profile.collisionRadius);
            unit.position = position;
            if (fullHealth) unit.health = unit.profile.maxHealth;
            if (fullMana) unit.mana = unit.profile.maxMana;
            unit.path = Array.Empty<OriginalPoint>(); unit.pathIndex = 0; unit.pathRevision = -1;
            revision++;
            return true;
        }
        public bool RestoreUnit(int entityId, OriginalPoint position, bool fullMana = true) =>
            Relocate(entityId, position, fullHealth: true, fullMana: fullMana);
        // Source SetUnitPosition during duel return does not revive a corpse.
        public bool RelocateStoredPosition(int entityId, OriginalPoint position)
        {
            Coordinates(position);
            if (!units.TryGetValue(entityId, out var unit)) return false;
            if (unit.health > 0) return Relocate(entityId, position);
            if (!navigation.IsWalkable(position.x, position.y, unit.profile.collisionRadius)) return false;
            unit.position = position; unit.destination = position; revision++; return true;
        }
        // Hidden units retain identity and vitality. Showing an occupied live
        // body fails before mutation; the caller owns any replacement placement.
        public bool SetVisibility(int entityId, bool visible)
        {
            if (!units.TryGetValue(entityId, out var unit)) return false;
            if (unit.hidden == !visible) return true;
            if (visible && unit.health > 0 && !unit.pathingDisabled)
            {
                if (!Free(unit.position, unit.profile.collisionRadius, entityId)) return false;
                motion.Add(entityId, unit.position.x, unit.position.y, unit.profile.collisionRadius);
            }
            else if (!visible)
            {
                motion.Remove(entityId);
                ClearTargets(OriginalWorldTargetKind.Unit, entityId);
            }
            unit.hidden = !visible; unit.pathRevision = -1; revision++; return true;
        }
        public bool MarkAttack(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0) return false;
            unit.attackSequence = checked(unit.attackSequence + 1); revision++;
            return true;
        }
        public bool MarkCast(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0) return false;
            unit.castSequence = checked(unit.castSequence + 1); revision++; return true;
        }
        public bool SetFacing(int entityId, double degrees)
        {
            if (!Finite(degrees)) throw new ArgumentOutOfRangeException(nameof(degrees));
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0) return false;
            unit.facingDegrees = degrees % 360;
            if (unit.facingDegrees < 0) unit.facingDegrees += 360;
            if (unit.facingDegrees >= 360) unit.facingDegrees = 0;
            unit.hasFacing = true; revision++; return true;
        }
        public bool TrySpendMana(int entityId, double amount)
        {
            if (!Finite(amount) || amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.hidden || unit.paused || unit.mana < amount) return false;
            if (amount != 0) { unit.mana -= amount; revision++; }
            return true;
        }
        public bool SetUnitState(int entityId, bool? paused = null, bool? invulnerable = null)
        {
            if (!units.TryGetValue(entityId, out var unit)) return false;
            if (paused.HasValue) unit.paused = paused.Value;
            if (invulnerable.HasValue) unit.invulnerable = invulnerable.Value;
            revision++; return true;
        }
        // Combat chooses the attack-range approach point. The registry must not
        // invent attack range, chasing AI or damage from an attack intent.
        public bool TryApproachTarget(int entityId, OriginalPoint destination)
        {
            Coordinates(destination);
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.hidden || unit.paused || unit.holding || unit.order != OriginalWorldOrder.AttackTarget ||
                !unit.pathingDisabled && !navigation.IsWalkable(destination.x, destination.y, unit.profile.collisionRadius)) return false;
            if (unit.approaching && Distance(unit.destination, destination) <= Epsilon && unit.pathRevision == navigation.NavigationRevision) return true;
            unit.destination = destination; unit.approaching = true; Repath(unit); revision++;
            return true;
        }
        public bool StopApproach(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.order != OriginalWorldOrder.AttackTarget) return false;
            ClearApproach(unit); revision++;
            return true;
        }
        public bool TryMove(int entityId, OriginalPoint destination)
        {
            Coordinates(destination);
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.hidden || unit.paused || !unit.pathingDisabled && !navigation.IsWalkable(destination.x, destination.y, unit.profile.collisionRadius)) return false;
            // An empty route remains an explicit move intent. Destruction can
            // make it possible later, at which point revision invalidates it.
            unit.holding = false; unit.destination = destination; unit.order = OriginalWorldOrder.Move;
            unit.targetKind = OriginalWorldTargetKind.None; unit.targetId = 0; unit.approaching = false;
            Repath(unit); revision++;
            return true;
        }
        public bool Stop(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit)) return false;
            ClearOrder(unit); unit.holding = false; revision++;
            return true;
        }
        // Warcraft Hold Position permits attacks in range without pursuit.
        // Target death clears an attack intent, but does not clear this order.
        // https://classic.battle.net/war3/basics/unitcommands.shtml
        public bool HoldPosition(int entityId)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.hidden || unit.paused) return false;
            ClearOrder(unit); unit.holding = true; revision++;
            return true;
        }
        public bool TryAttackTarget(int entityId, OriginalWorldTargetKind kind, int targetId, bool preserveHolding = false)
        {
            if (!units.TryGetValue(entityId, out var unit) || unit.health <= 0 || unit.hidden || unit.paused) return false;
            bool valid = kind == OriginalWorldTargetKind.Unit ? targetId != entityId && units.TryGetValue(targetId, out var victim) && victim.health > 0 && !victim.hidden :
                kind == OriginalWorldTargetKind.Doodad && doodads.TryGetValue(targetId, out var target) && target.health > 0;
            if (!valid) return false;
            if (!preserveHolding) unit.holding = false;
            ClearOrder(unit); unit.order = OriginalWorldOrder.AttackTarget; unit.targetKind = kind; unit.targetId = targetId;
            revision++;
            return true;
        }
        public void Advance(double seconds, Func<int, bool> movementAllowed = null)
        {
            if (!Finite(seconds) || seconds < 0 || seconds > .05 || !Finite(time + seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds), "World ticks must be bounded to 0.05 seconds.");
            if (seconds == 0) return;
            foreach (var unit in units.Values)
            {
                if (unit.health <= 0 || unit.hidden || unit.paused || unit.holding || unit.order != OriginalWorldOrder.Move && !unit.approaching) continue;
                if (movementAllowed != null && !movementAllowed(unit.id)) continue;
                if (unit.pathRevision != navigation.NavigationRevision) Repath(unit);
                double budget = unit.profile.moveSpeed * seconds;
                if (unit.pathingDisabled)
                {
                    double distance = Distance(unit.position, unit.destination);
                    double step = Math.Min(distance, budget);
                    if (distance > Epsilon)
                        unit.position = new OriginalPoint(unit.position.x + (unit.destination.x - unit.position.x) * step / distance,
                            unit.position.y + (unit.destination.y - unit.position.y) * step / distance);
                    if (distance <= budget + Epsilon)
                    {
                        if (unit.order == OriginalWorldOrder.Move) ClearOrder(unit);
                        else ClearApproach(unit);
                    }
                    continue;
                }
                while (budget > Epsilon && unit.pathIndex < unit.path.Length)
                {
                    var from = motion.Position(unit.id);
                    // A nearest grid vertex can lie behind a moving body and
                    // inside the next actor in a queue. It is not a mandatory
                    // physical destination when the following leg is visible.
                    // Terrain proves each shortcut; Motion still sweeps every
                    // step against bodies and charges the same travel budget.
                    while (unit.pathIndex + 1 < unit.path.Length &&
                        navigation.SegmentClear(from, unit.path[unit.pathIndex + 1], unit.profile.collisionRadius)) unit.pathIndex++;
                    var next = unit.path[unit.pathIndex];
                    double distance = Distance(from, next);
                    if (distance <= Epsilon) { unit.pathIndex++; continue; }
                    double allowance = Math.Min(distance, budget);
                    var after = motion.Move(unit.id, next.x, next.y, allowance);
                    unit.position = after;
                    // Charge the entire requested allowance, even after a slide
                    // or blocked step, so a turn never grants extra distance.
                    budget -= allowance;
                    if (Distance(after, next) <= Epsilon) unit.pathIndex++;
                    else break;
                }
                if (unit.path.Length > 0 && unit.pathIndex >= unit.path.Length)
                {
                    if (unit.order == OriginalWorldOrder.Move) ClearOrder(unit);
                    else ClearApproach(unit);
                }
            }
            time += seconds; revision++;
        }
        void Repath(Unit unit)
        {
            var route = unit.pathingDisabled ? new[] { unit.destination } :
                navigation.FindPath(motion.Position(unit.id), unit.destination, unit.profile.collisionRadius);
            if (route == null) throw new InvalidOperationException("Navigation must return an empty array for an unavailable route.");
            var copy = (OriginalPoint[])route.Clone();
            foreach (var point in copy) Coordinates(point);
            unit.path = copy; unit.pathIndex = 0; unit.pathRevision = navigation.NavigationRevision;
        }

        // Exact authored position first, then deterministic square rings on a
        // 16 WC sampling grid within the caller's radius. This is a bounded
        // replacement placement search, not the undocumented native algorithm.
        // A successful query reserves nothing; AddUnit still verifies occupancy.
        public bool TryFindFreeSpawn(OriginalPoint center, double radius, double maxDistance, out OriginalPoint position)
        {
            Coordinates(center);
            if (!Finite(radius) || radius <= 0 || radius > 32768 || !Finite(maxDistance) || maxDistance < 0 || maxDistance > 4096)
                throw new ArgumentOutOfRangeException(nameof(radius));
            position = center;
            if (Free(center, radius)) return true;
            if (maxDistance == 0) return false;
            double step = Math.Min(16, maxDistance);
            int rings = (int)Math.Ceiling(maxDistance / step);
            bool Candidate(int x, int y, out OriginalPoint candidate)
            {
                candidate = new OriginalPoint(center.x + x * step, center.y + y * step);
                return (x * step) * (x * step) + (y * step) * (y * step) <= maxDistance * maxDistance &&
                    Math.Abs(candidate.x) <= 1048576 && Math.Abs(candidate.y) <= 1048576 && Free(candidate, radius);
            }
            for (int ring = 1; ring <= rings; ring++)
            {
                for (int x = -ring; x <= ring; x++)
                {
                    if (Candidate(x, -ring, out position) || Candidate(x, ring, out position)) return true;
                }
                for (int y = -ring + 1; y < ring; y++)
                {
                    if (Candidate(-ring, y, out position) || Candidate(ring, y, out position)) return true;
                }
            }
            position = center;
            return false;
        }
        bool Free(OriginalPoint point, double radius, int excludingEntity = 0)
        {
            if (!navigation.IsWalkable(point.x, point.y, radius)) return false;
            foreach (var unit in units.Values)
            {
                if (unit.health <= 0 || unit.hidden || unit.pathingDisabled || unit.id == excludingEntity) continue;
                var other = unit.position;
                double sum = unit.profile.collisionRadius + radius;
                double dx = point.x - other.x, dy = point.y - other.y;
                if (dx * dx + dy * dy < sum * sum) return false;
            }
            return true;
        }

        // HP is already resolved from base HP, placed life percent and source
        // modifiers. The scene's visual size does not change this declaration.
        public void AddDoodad(int editorId, string rawcode, OriginalPoint position, double maxHealth, double health)
        {
            if (editorId < 0 || editorId >= FirstDynamicDoodadId) throw new ArgumentOutOfRangeException(nameof(editorId));
            if (!Finite(maxHealth) || maxHealth <= 0 || !Finite(health) || health < 0 || health > maxHealth)
                throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (doodads.ContainsKey(editorId)) throw new ArgumentException("Duplicate destructable editor identity.", nameof(editorId));
            Rawcode(rawcode); Coordinates(position);
            navigation.SetDoodadAlive(editorId, health > 0);
            doodads.Add(editorId, new OriginalWorldDoodadView { editorId = editorId, rawcode = rawcode,
                position = position, maxHealth = maxHealth, health = health }); revision++;
        }
        // Damage is a trusted, already resolved delta. Armor, attacker range,
        // cooldown, loot and LTex explosions belong to source combat rules.
        public bool ApplyDoodadDamage(int editorId, double damage)
        {
            if (!Finite(damage) || damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (damage == 0 || !doodads.TryGetValue(editorId, out var doodad) || doodad.health <= 0 || doodad.invulnerable) return false;
            doodad.health = Math.Max(0, doodad.health - damage); revision++;
            Emit(OriginalWorldEventKind.DoodadDamaged, editorId: editorId, rawcode: doodad.rawcode, health: doodad.health);
            if (doodad.health == 0)
            {
                navigation.SetDoodadAlive(editorId, false);
                ClearTargets(OriginalWorldTargetKind.Doodad, editorId);
                Emit(OriginalWorldEventKind.DoodadDestroyed, editorId: editorId, rawcode: doodad.rawcode);
            }
            return true;
        }
        public OriginalWorldSnapshot Snapshot()
        {
            var views = new List<OriginalWorldUnitView>();
            foreach (var unit in units.Values)
                views.Add(View(unit));
            var objects = new List<OriginalWorldDoodadView>();
            foreach (var doodad in doodads.Values)
                objects.Add(new OriginalWorldDoodadView { editorId = doodad.editorId, rawcode = doodad.rawcode,
                    position = doodad.position, maxHealth = doodad.maxHealth, health = doodad.health,
                    dynamic = doodad.dynamic, invulnerable = doodad.invulnerable, facingDegrees = doodad.facingDegrees, scale = doodad.scale });
            return new OriginalWorldSnapshot { revision = revision, navigationRevision = navigation.NavigationRevision,
                time = time, units = views.ToArray(), doodads = objects.ToArray() };
        }
        public OriginalWorldUnitView UnitState(int entityId) => units.TryGetValue(entityId, out var unit) ? View(unit) : null;
        static OriginalWorldUnitView View(Unit unit) => new OriginalWorldUnitView { entityId = unit.id, ownerSlot = unit.owner, rawcode = unit.rawcode,
            kind = unit.kind, sourceHeroEntityId = unit.sourceHeroEntityId,
            copySourceEntityId = unit.copySourceEntityId, imageFactory = unit.imageFactory,
            position = unit.position, destination = unit.destination, profile = unit.profile.Copy(),
            health = unit.health, mana = unit.mana, order = unit.order, targetKind = unit.targetKind, targetId = unit.targetId,
            attackSequence = unit.attackSequence, castSequence = unit.castSequence, hasFacing = unit.hasFacing, facingDegrees = unit.facingDegrees,
            approaching = unit.approaching, holding = unit.holding, paused = unit.paused, invulnerable = EffectiveInvulnerability(unit), hidden = unit.hidden,
            pathingDisabled = unit.pathingDisabled, visibleToOwners = unit.hidden ? 0 : 255 };
        public OriginalWorldEvent[] DrainEvents() { var result = events.ToArray(); events.Clear(); return result; }
        void Emit(OriginalWorldEventKind kind, int entityId = 0, int editorId = 0, string rawcode = null, double health = 0,
            OriginalWorldRemovalReason removalReason = OriginalWorldRemovalReason.Explicit)
        {
            events.Add(new OriginalWorldEvent { sequence = ++eventSequence, time = time, kind = kind,
                entityId = entityId, editorId = editorId, rawcode = rawcode, health = health, removalReason = removalReason });
        }
        void ClearTargets(OriginalWorldTargetKind kind, int id)
        {
            foreach (var unit in units.Values)
                if (unit.order == OriginalWorldOrder.AttackTarget && unit.targetKind == kind && unit.targetId == id) ClearOrder(unit);
        }
        static void ClearOrder(Unit unit)
        {
            unit.order = OriginalWorldOrder.None; unit.targetKind = OriginalWorldTargetKind.None; unit.targetId = 0;
            ClearApproach(unit);
        }
        static void ClearApproach(Unit unit)
        {
            unit.approaching = false;
            unit.path = Array.Empty<OriginalPoint>(); unit.pathIndex = 0;
        }
        static void ValidateProfile(OriginalWorldUnitProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (!Finite(profile.moveSpeed) || profile.moveSpeed < 0 || profile.moveSpeed > 81920 ||
                !Finite(profile.collisionRadius) || profile.collisionRadius <= 0 || profile.collisionRadius > 32768 ||
                !Finite(profile.maxHealth) || profile.maxHealth <= 0 || !Finite(profile.maxMana) || profile.maxMana < 0)
                throw new ArgumentOutOfRangeException(nameof(profile), "Source profile values must be known, finite and representable by bounded motion.");
        }
        static double Distance(OriginalPoint a, OriginalPoint b) => Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        static void Coordinates(OriginalPoint point)
        {
            if (!Finite(point.x) || !Finite(point.y) || Math.Abs(point.x) > 1048576 || Math.Abs(point.y) > 1048576)
                throw new ArgumentOutOfRangeException(nameof(point));
        }
        static void Rawcode(string value)
        { if (value == null || value.Length != 4) throw new ArgumentException("A four-character source rawcode is required.", nameof(value)); }
    }
}

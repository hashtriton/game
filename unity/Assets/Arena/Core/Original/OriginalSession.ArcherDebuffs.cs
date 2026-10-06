using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ArcherDebuff
        {
            internal int target;
            internal double remaining;
            internal OriginalArcherDebuffRules rules;
        }
        sealed class ArcherNativeOrder
        {
            internal int target, owner;
            internal double due;
            internal OriginalArcherDebuffRules rules;
        }
        readonly Dictionary<int, Dictionary<string, ArcherDebuff>> archerDebuffs = new Dictionary<int, Dictionary<string, ArcherDebuff>>();
        readonly List<ArcherNativeOrder> archerNativeOrders = new List<ArcherNativeOrder>();
        double archerDebuffClock;

        bool ArcherNativeTarget(OriginalWorldUnitView target, int owner) => target != null && target.health > .405 &&
            !target.hidden && !target.invulnerable && AreEnemies(owner, target.ownerSlot) &&
            !CasterHasType(target, "mechanical") && !IsArcherMagicImmune(target);
        void OrderArcherNativeHelper(int owner, int targetId, string ability, int rank)
        {
            var target = world.UnitState(targetId);
            if (!ArcherNativeTarget(target, owner)) return;
            var rules = new OriginalArcherDebuffRules(combatCatalog, native, ability, rank);
            if (rules.projectile)
            {
                // ARCHH2 after-order has no acid/haze buff; its hit is deferred.
                // Source creates the helper at target coordinates. One host tick
                // is an explicit approximation of that private native scheduling.
                archerNativeOrders.Add(new ArcherNativeOrder { owner = owner, target = targetId, rules = rules, due = world.Clock + .01 });
                return;
            }
            // ARCHH2 A168 records three immediate primary-target zero damage
            // events: two before Bfro, one after it. The source is the helper,
            // never the owning hero (aie tests exact source identity). Extra
            // targets' native event multiplicity was not measured here.
            ApplyResolvedUnitHit(0, owner, world.UnitState(targetId), 0);
            ApplyResolvedUnitHit(0, owner, world.UnitState(targetId), 0);
            ApplyArcherNativeBuff(targetId, rules);
            ApplyResolvedUnitHit(0, owner, world.UnitState(targetId), 0);
            if (rules.area <= 0) return;
            var center = target.position;
            foreach (var other in world.Snapshot().units)
                if (other.entityId != targetId && ArcherNativeTarget(other, owner) && SquaredDistance(other.position, center) <= rules.area * rules.area)
                    ApplyArcherNativeBuff(other.entityId, rules);
        }
        void ApplyArcherNativeBuff(int id, OriginalArcherDebuffRules rules)
        {
            var actor = world.UnitState(id); if (actor == null || actor.health <= .405) return;
            CaptureAbilityMovementBase(id);
            if (!archerDebuffs.TryGetValue(id, out var buffs)) archerDebuffs[id] = buffs = new Dictionary<string, ArcherDebuff>();
            // One state per native buff ID. Same-source refresh is exact; the
            // latest rank replaces a prior rank as a labelled reconstruction,
            // not a claim that unequal native strengths were measured together.
            bool nativeHero = IsNativeHeroPredicate(actor);
            buffs[rules.buffId] = new ArcherDebuff { target = id, rules = rules, remaining = nativeHero ? rules.heroDuration : rules.duration };
            RefreshAbilityMovement(id); RescaleWeaponRate(id, world.Clock);
        }
        double ArcherDebuffMovementBonus(int id) => -ArcherDebuffValue(id, 0);
        double ArcherDebuffAttackSlow(int id) => ArcherDebuffValue(id, 1);
        double ArcherDebuffArmorDelta(int id) => -ArcherDebuffValue(id, 2);
        double ArcherDebuffMissChance(int id) => ArcherDebuffValue(id, 3);
        double ArcherDebuffValue(int id, int kind)
        {
            double value = 0;
            if (archerDebuffs.TryGetValue(id, out var buffs))
                foreach (var buff in buffs.Values)
                    value += kind == 0 ? buff.rules.movementSlow : kind == 1 ? buff.rules.attackSlow : kind == 2 ? buff.rules.armorReduction : buff.rules.missChance;
            return value;
        }
        void RemoveArcherNativeBuff(int id, string buffId)
        {
            if (!archerDebuffs.TryGetValue(id, out var buffs) || !buffs.Remove(buffId)) return;
            if (buffs.Count == 0) archerDebuffs.Remove(id);
            RefreshAbilityMovement(id); RescaleWeaponRate(id, world.Clock); ReleaseAbilityMovementBase(id);
        }
        void RemoveArcherDebuffs(int id)
        {
            if (!archerDebuffs.TryGetValue(id, out var buffs)) return;
            foreach (var key in new List<string>(buffs.Keys)) RemoveArcherNativeBuff(id, key);
        }
        void AdvanceArcherDebuffs()
        {
            double now = world.Clock, elapsed = Math.Max(0, now - archerDebuffClock); archerDebuffClock = now;
            // Buff clocks are suspended by native PauseUnit; ordinary move/stop
            // orders never pause them. Pending helper projectiles remain alive.
            foreach (var pair in new List<KeyValuePair<int, Dictionary<string, ArcherDebuff>>>(archerDebuffs))
            {
                var actor = world.UnitState(pair.Key);
                if (actor == null || actor.health <= .405) { RemoveArcherDebuffs(pair.Key); continue; }
                if (actor.paused) continue;
                foreach (var buff in new List<ArcherDebuff>(pair.Value.Values))
                {
                    buff.remaining -= elapsed;
                    if (buff.remaining <= 1e-9) RemoveArcherNativeBuff(pair.Key, buff.rules.buffId);
                }
            }
            foreach (var order in new List<ArcherNativeOrder>(archerNativeOrders))
            {
                if (order.due > now + 1e-9) continue;
                archerNativeOrders.Remove(order);
                if (ArcherNativeTarget(world.UnitState(order.target), order.owner)) ApplyArcherNativeBuff(order.target, order.rules);
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ArcherWatch { internal int actor, target, owner; internal double expires; }
        sealed class ArcherChain
        {
            internal int actor, owner, target, remaining = 7;
            internal double nextTick, damage;
            internal OriginalPoint position;
            internal readonly HashSet<int> visited = new HashSet<int>();
        }
        sealed class ArcherPush { internal int target; internal OriginalPoint origin; internal double nextTick; internal float elapsed; }
        readonly List<ArcherWatch> archerWatches = new List<ArcherWatch>();
        readonly Dictionary<int, double> archerWatchThrottle = new Dictionary<int, double>();
        readonly List<ArcherChain> archerChains = new List<ArcherChain>();
        readonly List<ArcherPush> archerPushes = new List<ArcherPush>();
        bool ArcherElementAvailable(OriginalBowElement element) => element == OriginalBowElement.Ice || element == OriginalBowElement.Lightning
            ? TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic) : element != OriginalBowElement.Fire ||
            TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.ChaosUniversal);

        // aVe/ane/aae82965..82991: attacked-event watch, .3s throttle and
        // one-second expiry. Any damage from that archer can satisfy the watch.
        void OnArcherAttackStarted(OriginalWorldUnitView attacker, OriginalWorldUnitView target)
        {
            if (attacker.kind != OriginalWorldUnitKind.Hero || attacker.rawcode != "N0A0" || target == null || !AreEnemies(attacker.ownerSlot, target.ownerSlot)) return;
            var player = players.Find(p => p.slot == attacker.ownerSlot);
            if (!player.archerAttackHandlerRegistered || SkillRank(player, "A15X") <= 0 || IsArcherStructure(target)) return;
            if (archerWatchThrottle.TryGetValue(attacker.entityId, out double expires) && expires > world.Clock + 1e-9) return;
            archerWatchThrottle[attacker.entityId] = world.Clock + .3;
            archerWatches.Add(new ArcherWatch { actor = attacker.entityId, target = target.entityId, owner = attacker.ownerSlot, expires = world.Clock + 1 });
        }
        void ObserveArcherDamage(int attacker, int target)
        {
            foreach (var watch in new List<ArcherWatch>(archerWatches))
            {
                if (!archerWatches.Contains(watch)) continue; // Nested proc consumed another live watch.
                if (world.Clock + 1e-9 >= watch.expires) { archerWatches.Remove(watch); continue; }
                if (watch.actor != attacker || watch.target != target) continue;
                var state = Archer(watch.owner);
                if (state.active == OriginalBowElement.None) continue;
                archerWatches.Remove(watch); // DisableTrigger before the original's nested UnitDamageTarget.
                RemoveArcherNativeBuff(target, "Bfro"); // aie removes frost before testing its remaining charge count.
                if (state.charges > 0) { state.charges--; ApplyArcherElement(attacker, watch.owner, target, state.active); }
                else { state.active = OriginalBowElement.None; state.combo = 0; }
            }
        }
        bool IsArcherStructure(OriginalWorldUnitView unit) => Array.IndexOf((combatCatalog.Unit(unit.rawcode).Text("type") ?? "").Split(','), "structure") >= 0;
        bool IsArcherMagicImmune(OriginalWorldUnitView unit)
        {
            foreach (var ability in NativeUnitAbilities(unit)) if (ability.Text("code") == "Amim") return true;
            return false;
        }
        void ApplyArcherElement(int actorId, int owner, int targetId, OriginalBowElement element)
        {
            var player = players.Find(p => p.slot == owner); int rank = SkillRank(player, "A15X");
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (rank <= 0 || actor == null || target == null || element == OriginalBowElement.None) return;
            // i6e..are82727..82902. Native helper orders and scripted damage
            // remain distinct: helpers have no canonical hero source identity.
            if (element == OriginalBowElement.Venom)
            {
                OrderArcherNativeHelper(owner, targetId, "A166", rank);
                ApplyTriggeredNormalHit(0, owner, target, ScriptedAbilityAttack(player) * (.05 + .05 * rank));
            }
            else if (element == OriginalBowElement.Ice)
            {
                var center = target.position;
                OrderArcherNativeHelper(owner, targetId, "A168", rank);
                ApplyTriggeredHit(actorId, owner, target, 50 * rank, OriginalTriggeredDamageMode.SpellMagic);
                foreach (var other in world.Snapshot().units)
                    if (other.entityId != targetId && other.health > .405 && AreEnemies(owner, other.ownerSlot) && !IsArcherMagicImmune(other) && SquaredDistance(other.position, center) <= 200 * 200)
                        ApplyTriggeredHit(actorId, owner, other, 25 * rank, OriginalTriggeredDamageMode.SpellMagic);
            }
            else if (element == OriginalBowElement.Fire)
            {
                foreach (var other in world.Snapshot().units)
                    if (other.health > .405 && AreEnemies(owner, other.ownerSlot) && SquaredDistance(other.position, target.position) <= 150 * 150)
                        ApplyTriggeredHit(actorId, owner, other, 15 + 20 * rank, OriginalTriggeredDamageMode.ChaosUniversal);
            }
            else if (element == OriginalBowElement.Dark)
            {
                OrderArcherNativeHelper(owner, targetId, "A165", rank);
                world.SetPathingEnabled(targetId, false);
                archerPushes.Add(new ArcherPush { target = targetId, origin = actor.position, nextTick = world.Clock + .03 });
            }
            else if (element == OriginalBowElement.Lightning)
            {
                var chain = new ArcherChain { actor = actorId, owner = owner, target = targetId, position = actor.position,
                    nextTick = world.Clock + .04, damage = 25 + 25 * rank };
                chain.visited.Add(targetId); archerChains.Add(chain);
            }
        }
        void AdvanceArcherElements()
        {
            AdvanceArcherDebuffs();
            double now = world.Clock;
            archerWatches.RemoveAll(watch => watch.expires <= now + 1e-9 || world.UnitState(watch.target) == null);
            foreach (var push in new List<ArcherPush>(archerPushes))
            {
                while (push.nextTick <= now + 1e-9)
                {
                    push.nextTick += .03; push.elapsed += .03f;
                    var target = world.UnitState(push.target);
                    if (target == null) { archerPushes.Remove(push); break; }
                    if (push.elapsed >= .15f) { world.SetPathingEnabled(push.target, true); archerPushes.Remove(push); break; }
                    double angle = Math.Atan2(target.position.y - push.origin.y, target.position.x - push.origin.x);
                    var next = new OriginalPoint(target.position.x + 15 * Math.Cos(angle), target.position.y + 15 * Math.Sin(angle));
                    if (!OriginalShieldBashRules.AllowsForcedPoint(next)) { world.SetPathingEnabled(push.target, true); archerPushes.Remove(push); break; }
                    // Native SetUnitPosition with pathing disabled. The host
                    // uses its explicit forced-position policy, not a claim of
                    // identical native projection against private pathing state.
                    world.ForcePosition(push.target, next);
                }
            }
            foreach (var chain in new List<ArcherChain>(archerChains))
            {
                while (chain.nextTick <= now + 1e-9)
                {
                    chain.nextTick += .04;
                    var target = world.UnitState(chain.target);
                    if (target == null) { archerChains.Remove(chain); break; }
                    double angle = Math.Atan2(target.position.y - chain.position.y, target.position.x - chain.position.x);
                    chain.position = new OriginalPoint(chain.position.x + 28 * Math.Cos(angle), chain.position.y + 28 * Math.Sin(angle));
                    if (SquaredDistance(chain.position, target.position) > 28 * 28) continue;
                    chain.position = target.position;
                    if (target.health > .405 && !target.hidden)
                    {
                        ApplyTriggeredHit(chain.actor, chain.owner, target, chain.damage, OriginalTriggeredDamageMode.SpellMagic);
                        chain.remaining--;
                    }
                    OriginalWorldUnitView next = null; double best = double.PositiveInfinity;
                    foreach (var candidate in world.Snapshot().units)
                    {
                        if (candidate.health <= .405 || candidate.hidden || IsArcherStructure(candidate) || chain.visited.Contains(candidate.entityId) ||
                            AreEnemies(target.ownerSlot, candidate.ownerSlot)) continue;
                        double distance = SquaredDistance(candidate.position, target.position);
                        if (distance <= 700 * 700 && distance < best) { best = distance; next = candidate; }
                    }
                    if (chain.remaining == 0 || next == null) { archerChains.Remove(chain); break; }
                    chain.target = next.entityId; chain.visited.Add(next.entityId);
                }
            }
        }
    }
}

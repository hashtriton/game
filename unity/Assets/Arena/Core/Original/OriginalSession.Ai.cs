using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int, OriginalPoint> enemyGoals = new Dictionary<int, OriginalPoint>();
        // Warcraft's documented A+ground command resumes its destination after
        // engaging enemies. Acquisition ordering remains the host policy below.
        // https://classic.battle.net/war3/basics/unitcommands.shtml
        readonly Dictionary<int, OriginalPoint> playerAttackGoals = new Dictionary<int, OriginalPoint>();
        double nextAcquisition, nextBarrelOrder = 7;

        // E5v 32052..32077 / FA 86506..86509 chooses one point per gate
        // group. ETv puts all casters and bosses in GD, including other gates.
        void RefreshWorldOrders()
        {
            var units = world.Snapshot().units;
            var origins = new[] { new OriginalPoint(64, 2624), new OriginalPoint(-1984, 574), new OriginalPoint(1734, 1224) };
            for (int group = 0; group < origins.Length; group++)
            {
                OriginalWorldUnitView closest = null;
                double best = double.PositiveInfinity;
                foreach (var hero in units)
                {
                    if (hero.kind != OriginalWorldUnitKind.Hero || hero.health <= .405 || hero.hidden || hero.position.x < -2560 || hero.position.x > 2304 ||
                        hero.position.y < -1536 || hero.position.y > 3200) continue;
                    double distance = SquaredDistance(hero.position, origins[group]);
                    if (distance <= best) { best = distance; closest = hero; }
                }
                if (closest == null) continue;
                foreach (var enemy in match.Enemies)
                {
                    if (enemy.attackGroup != group || enemy.cocoon || enemy.megaBoss) continue;
                    int id = OriginalWorld.EnemyEntityId(enemy.entityId);
                    var unit = world.UnitState(id);
                    if (unit == null || unit.health <= .405 || unit.hidden || unit.paused) continue;
                    enemyGoals[id] = closest.position;
                    if (IssueNpcAttackPoint(unit, closest.position, units))
                    {
                        OnAcceptedWorldOrder(id);
                        if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
                    }
                }
            }
        }

        bool IssueNpcAttackPoint(OriginalWorldUnitView actor,OriginalPoint point,OriginalWorldUnitView[] units)
        {
            // E1v32027 issues attack-point (851983), not move (851986).
            // Resolve an already reachable attack before the next movement
            // tick; waiting for the .2s acquisition scan walked engaged units
            // toward the hero's centre on every3.5s FA refresh.
            var definition=combatCatalog.Unit(actor.rawcode);
            if(!ActorWeaponBlocked(actor.entityId)&&HasNativeWeapon(definition)&&
                definition.TryNumber("acquire",out double acquire,out _)&&acquire>0)
            {
                bool Reachable(OriginalWorldUnitView candidate)
                {
                    if(candidate==null||candidate.health<=.405||!CanSeeForCombat(actor.ownerSlot,candidate)||candidate.invulnerable||!Enemies(actor,candidate))return false;
                    int weapon=SelectedNativeWeaponIndex(definition,candidate);if(weapon==0)return false;
                    double range=UnitAttackCenterRange(definition.Number("rangeN"+weapon),actor,candidate.profile.collisionRadius,weapon);
                    double distance=SquaredDistance(actor.position,candidate.position);
                    return distance<=range*range&&distance<=acquire*acquire;
                }
                // Retain a valid engagement. The source changed a ground goal,
                // not an explicit unit target. Deterministic nearest-body choice
                // on acquisition is the same host policy as AdvanceWorldAi.
                var target=actor.order==OriginalWorldOrder.AttackTarget&&actor.targetKind==OriginalWorldTargetKind.Unit?
                    world.UnitState(actor.targetId):null;
                if(!Reachable(target))
                {
                    target=null;double nearest=double.PositiveInfinity;
                    foreach(var candidate in units)if(Reachable(candidate))
                    {
                        double distance=SquaredDistance(actor.position,candidate.position);
                        if(distance<nearest){nearest=distance;target=candidate;}
                    }
                }
                if(target!=null)return world.TryAttackTarget(actor.entityId,OriginalWorldTargetKind.Unit,target.entityId);
            }
            return world.TryMove(actor.entityId,point);
        }

        // Unity replaces native acquisition/pathing. The authored acquire range
        // is preserved; nearest-body priority and a 0.2s scan are implementation
        // choices, not observations of Warcraft's private targeting algorithm.
        void AdvanceWorldAi()
        {
            var snapshot = world.Snapshot();
            foreach (int id in new List<int>(playerAttackGoals.Keys))
            {
                var actor = world.UnitState(id);
                if (actor == null || actor.health <= 0 ||
                    actor.order == OriginalWorldOrder.None && SquaredDistance(actor.position, playerAttackGoals[id]) <= 1)
                    playerAttackGoals.Remove(id);
            }
            if (snapshot.time >= nextBarrelOrder)
            {
                nextBarrelOrder += 7;
                if (options.defensiveBarrels == OriginalDefensiveBarrels.Attackable)
                    foreach (var unit in snapshot.units)
                    {
                        if (unit.kind != OriginalWorldUnitKind.Enemy || unit.health <= .405 || unit.hidden || unit.paused ||
                            unit.position.x < -3264 || unit.position.x > 3360 || unit.position.y < -3648 || unit.position.y > 2976) continue;
                        if (!HasNativeWeapon(combatCatalog.Unit(unit.rawcode))) continue;
                        foreach (var barrel in snapshot.doodads)
                            if (barrel.rawcode == "LTbr" && barrel.health > 0 && Math.Abs(barrel.position.x - unit.position.x) <= 125 &&
                                Math.Abs(barrel.position.y - unit.position.y) <= 125)
                            {
                                if (world.TryAttackTarget(unit.entityId, OriginalWorldTargetKind.Doodad, barrel.editorId))
                                {
                                    OnAcceptedWorldOrder(unit.entityId);
                                    enemyGoals.Remove(unit.entityId);
                                    if (weaponCycles.TryGetValue(unit.entityId, out var cycle)) cycle.winding = false;
                                }
                            }
                    }
                // n6's native destructable enumeration order is unobserved.
                // Stable editor IDs make the replacement deterministic.
                snapshot = world.Snapshot();
            }
            if (snapshot.time + 1e-9 < nextAcquisition) return;
            nextAcquisition = snapshot.time + .2;
            SelectWaveSpellOrders();
            SelectOrdinaryNativeSpellOrders();
            snapshot=world.Snapshot();
            foreach (var unit in snapshot.units)
            {
                if (unit.health <= .405 || unit.hidden || unit.paused || ActorWeaponBlocked(unit.entityId) || AbilityControlsActor(unit.entityId) || AutomaticAttackRecovery(unit.entityId) || unit.order == OriginalWorldOrder.AttackTarget ||
                    unit.ownerSlot > 0 && unit.order == OriginalWorldOrder.Move && !playerAttackGoals.ContainsKey(unit.entityId)) continue;
                var definition = combatCatalog.Unit(unit.rawcode);
                if (!HasNativeWeapon(definition)) continue;
                if (!definition.TryNumber("acquire", out double range, out _) || range <= 0) continue;
                OriginalWorldUnitView target = null;
                double nearest = range * range;
                foreach (var candidate in snapshot.units)
                {
                    if (candidate.health <= .405 || !CanSeeForCombat(unit.ownerSlot,candidate) || candidate.invulnerable || !Enemies(unit, candidate)) continue;
                    int weapon = SelectedNativeWeaponIndex(definition, candidate);
                    if (weapon == 0) continue;
                    double distance = SquaredDistance(unit.position, candidate.position);
                    if (unit.holding)
                    {
                        double attackRange = UnitAttackCenterRange(definition.Number("rangeN" + weapon), unit, candidate.profile.collisionRadius, weapon);
                        if (distance > attackRange * attackRange) continue;
                    }
                    if (distance < nearest || target == null && distance <= nearest) { nearest = distance; target = candidate; }
                }
                if (target != null) world.TryAttackTarget(unit.entityId, OriginalWorldTargetKind.Unit, target.entityId, preserveHolding: true);
                else if (unit.order == OriginalWorldOrder.None && playerAttackGoals.TryGetValue(unit.entityId, out var playerPoint))
                    world.TryMove(unit.entityId, playerPoint);
                else if (!unit.holding && unit.order == OriginalWorldOrder.None && enemyGoals.TryGetValue(unit.entityId, out var point))
                    world.TryMove(unit.entityId, point);
            }
        }

        static double SquaredDistance(OriginalPoint a, OriginalPoint b) =>
            (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }
}

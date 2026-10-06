using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class WeaponCycle
        {
            internal double nextAttack, releaseAt, nextPath, rate;
            internal bool winding;
            internal OriginalWorldTargetKind kind;
            internal int target, weapon;
        }
        sealed class Projectile
        {
            internal int attacker, owner, target;
            internal string attackType, poisonAbility;
            internal bool melee, missed;
            internal OriginalWorldTargetKind kind;
            internal double landsAt, damage;
            internal NativeWeaponProc[] nativeProcs;
            internal ItemOrbSecondary itemOrb;
            internal bool fixedImpact;
            internal OriginalPoint impactPoint;
            internal string splashTargets;
            internal double splashFull, splashHalf, splashQuarter, splashHalfFactor, splashQuarterFactor;
        }
        readonly Dictionary<int, WeaponCycle> weaponCycles = new Dictionary<int, WeaponCycle>();
        readonly List<Projectile> projectiles = new List<Projectile>();
        uint weaponRandom;

        // Source weapon declarations drive the basic cycle. Native event timing
        // and projectile homing remain separately tracked in the parity ledger.
        void AdvanceWeapons()
        {
            var snapshot = world.Snapshot();
            double time = snapshot.time;
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var shot = projectiles[i];
                if (shot.landsAt > time) continue;
                projectiles.RemoveAt(i); ApplyWeaponHit(shot);
            }
            foreach (var previous in snapshot.units)
            {
                // Earlier strikes in this same tick can kill, remove or cancel
                // the next actor. A frame-start snapshot cannot grant a last hit.
                var unit = world.UnitState(previous.entityId);
                if (unit == null || unit.health <= 0 || unit.hidden || unit.paused) continue;
                var definition = combatCatalog.Unit(unit.rawcode);
                int weapon = NativeWeaponIndex(definition);
                if (weapon == 0) continue;
                if (!weaponCycles.TryGetValue(unit.entityId, out var cycle))
                    weaponCycles.Add(unit.entityId, cycle = new WeaponCycle());
                if (ActorWeaponBlocked(unit.entityId) || AbilityControlsActor(unit.entityId) || AutomaticAttackRecovery(unit.entityId)) { cycle.winding = false; continue; }
                if (unit.order != OriginalWorldOrder.AttackTarget)
                { cycle.winding = false; continue; }
                if (!WeaponTarget(snapshot, unit, out var target, out var targetRadius))
                {
                    cycle.winding = false;
                    if (unit.holding) world.HoldPosition(unit.entityId);
                    else if(unit.targetKind==OriginalWorldTargetKind.Unit && !CanSeeForCombat(unit.ownerSlot,world.UnitState(unit.targetId)))
                        world.Stop(unit.entityId); // Lost sight ends chasing; attack-point goals remain available to AI.
                    continue;
                }
                weapon = unit.targetKind == OriginalWorldTargetKind.Unit
                    ? SelectedNativeWeaponIndex(definition, world.UnitState(unit.targetId))
                    : SelectedNativeWeaponIndexForType(definition, "debris");
                if (weapon == 0) { cycle.winding = false; continue; }
                double range = definition.Number("rangeN" + weapon);
                double centerRange = unit.targetKind == OriginalWorldTargetKind.Unit
                    ? UnitAttackCenterRange(range, unit, targetRadius, weapon) : range;
                double dx = unit.position.x - target.x, dy = unit.position.y - target.y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (cycle.winding && (cycle.kind != unit.targetKind || cycle.target != unit.targetId || cycle.weapon != weapon)) cycle.winding = false;
                if (distance > centerRange)
                {
                    cycle.winding = false;
                    // Hold Position never follows a retreating target. Clearing
                    // it also lets the next acquisition scan find a closer one.
                    if (unit.holding) { world.HoldPosition(unit.entityId); continue; }
                    if (time >= cycle.nextPath)
                    {
                        cycle.nextPath = time + .25;
                        double approach = unit.targetKind == OriginalWorldTargetKind.Unit
                            ? UnitAttackCenterRange(range * .8, unit, targetRadius, weapon)
                            : Math.Max(unit.profile.collisionRadius + targetRadius, range * .8);
                        double angle = Math.Atan2(dy, dx);
                        // Sample only terrain-valid approach positions. Bodies
                        // remain occupied, so a narrow passage admits a queue.
                        for (int candidate = 0; candidate < 16; candidate++)
                        {
                            double offset = candidate == 0 ? 0 : (candidate % 2 == 0 ? 1 : -1) * ((candidate + 1) / 2) * Math.PI / 8;
                            var point = new OriginalPoint(target.x + approach * Math.Cos(angle + offset), target.y + approach * Math.Sin(angle + offset));
                            if (world.TryApproachTarget(unit.entityId, point)) break;
                        }
                    }
                    continue;
                }
                if (unit.approaching) world.StopApproach(unit.entityId);
                RescaleWeaponRate(unit.entityId, time);
                double rate = WeaponRate(unit); cycle.rate = rate;
                ActorCombatStats? hero = null;
                if (UsesHeroCombatStats(unit))
                {
                    hero = CombatStatsFor(unit);
                }
                if (!cycle.winding && time + 1e-9 >= cycle.nextAttack)
                {
                    cycle.winding = true; cycle.kind = unit.targetKind; cycle.target = unit.targetId; cycle.weapon = weapon;
                    cycle.releaseAt = time + definition.Number("dmgpt" + weapon) / rate;
                    cycle.nextAttack = time + definition.Number("cool" + weapon) / rate;
                    ArmItemWindWalkStrike(unit.entityId);
                    RevealItemInvisibility(unit.entityId);
                    world.MarkAttack(unit.entityId);
                    if (unit.targetKind == OriginalWorldTargetKind.Unit)
                        OnArcherAttackStarted(unit, world.UnitState(unit.targetId));
                }
                if (!cycle.winding || time + 1e-9 < cycle.releaseAt) continue;
                cycle.winding = false;
                double whiteDamage = definition.Number("dmgplus" + weapon) + (hero?.primaryDamageBonus ?? 0) + (hero?.upgradeAttackDamageBonus ?? 0) + BossPrimaryDamageBonus(unit);
                double flatDamage = (hero?.itemAttackDamageBonus ?? 0) + BossAttackBonus(unit.entityId) + SummonScriptAttackBonus(unit.entityId) + ConsumeItemWindWalkStrike(unit.entityId);
                int dice = (int)definition.Number("dice" + weapon), sides = (int)definition.Number("sides" + weapon);
                if (dice < 1 || sides < 1) throw new InvalidOperationException("Invalid authored attack dice.");
                for (int die = 0; die < dice; die++) whiteDamage += RollWeapon(sides);
                flatDamage += NativeCommandDamageBonus(unit, weapon, whiteDamage) + ItemAuraDamageBonus(unit.entityId, weapon, whiteDamage);
                double damage = ApplyCrippleWeaponDamage(unit.entityId, whiteDamage, flatDamage);
                damage = ImageOutgoingDamage(unit.entityId, damage);
                double delay = 0;
                if (definition.Text("weapTp" + weapon) == "missile" || definition.Text("weapTp" + weapon) == "msplash" || definition.Text("weapTp" + weapon) == "artillery")
                {
                    double speed = definition.Number("Missilespeed" + weapon);
                    if (speed <= 0) throw new InvalidOperationException("Invalid authored missile speed.");
                    delay = distance / speed;
                }
                var attack = new Projectile { attacker = unit.entityId, kind = unit.targetKind, target = unit.targetId,
                    owner = unit.ownerSlot, attackType = definition.Text("atkType" + weapon), damage = damage, landsAt = time + delay,
                    melee = definition.Text("weapTp" + weapon) == "normal", poisonAbility = WeaponPoison(unit), missed = RollWeaponMiss(unit.entityId),
                    nativeProcs = unit.targetKind==OriginalWorldTargetKind.Unit ? CaptureNativeWeaponProcs(unit) : Array.Empty<NativeWeaponProc>(),
                    itemOrb = unit.targetKind==OriginalWorldTargetKind.Unit ? CaptureItemOrbSecondary(unit.entityId) : null };
                attack.fixedImpact = definition.Text("weapTp" + weapon) == "artillery";
                attack.impactPoint = target;
                CaptureWeaponSplash(attack, definition, weapon);
                if (delay == 0) ApplyWeaponHit(attack); else projectiles.Add(attack);
            }
        }

        // UnitWeapons.slk explicitly declares '_' for an absent weapon (u00L,
        // row 16856), despite its nonzero acquisition radius. Missing metadata
        // is not treated as absent: the ordinary resolver still reports it.
        static bool HasNativeWeapon(OriginalCombatDefinition definition) => NativeWeaponIndex(definition) != 0;
        static int NativeWeaponIndex(OriginalCombatDefinition definition)
        {
            int index = 1;
            if (definition.TryNumber("weapsOn", out double enabled, out _))
            {
                if (enabled != Math.Floor(enabled) || enabled < 0 || enabled > 3)
                    throw new InvalidOperationException("Invalid authored enabled weapon mask.");
                if (enabled == 0) return 0;
                // n01R explicitly enables only weapon2: melee/chaos60+1d6,
                // .3windup/.9cooldown. Its disabled air splash weapon1 cannot
                // supply range or damage. The target-aware selector below
                // chooses among both enabled weapons without changing this mask.
                index = enabled == 2 ? 2 : 1;
            }
            return definition.Text("weapTp" + index) == "_" ? 0 : index;
        }

        int SelectedNativeWeaponIndex(OriginalCombatDefinition definition, OriginalWorldUnitView target) =>
            target == null ? 0 : SelectedNativeWeaponIndexForType(definition, combatCatalog.Unit(target.rawcode).Text("targType"));

        static int SelectedNativeWeaponIndexForType(OriginalCombatDefinition definition, string targetType)
        {
            int first = NativeWeaponIndex(definition);
            if (first == 0) return 0;
            int mask = definition.TryNumber("weapsOn", out double declared, out _) ? (int)declared : 1;
            for (int index = 1; index <= 2; index++)
                if ((mask & index) != 0 && definition.Text("weapTp" + index) != "_" &&
                    WeaponTargetTypeAllowed(definition.Text("targs" + index), targetType)) return index;
            return 0;
        }

        static bool WeaponTargetTypeAllowed(string declared, string targetType)
        {
            // UnitData.targType is the native attack classification. Movement
            // mode is insufficient: flying movement does not imply air targets.
            // Missing old fixture metadata retains its pre-export behavior;
            // explicit '_' is an empty target list, never an implicit ground.
            if (declared == null) return true;
            if (targetType == null || targetType == "_") return false;
            var flags = declared.Split(',');
            foreach (string flag in targetType.Split(','))
                if (Array.IndexOf(flags, flag) >= 0) return true;
            return false;
        }

        static void CaptureWeaponSplash(Projectile shot, OriginalCombatDefinition definition, int weapon)
        {
            if (definition.Text("weapTp" + weapon) != "msplash" && definition.Text("weapTp" + weapon) != "artillery") return;
            string suffix = weapon.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string targets = definition.Text("splashTargs" + suffix);
            if (targets == null || targets == "_" || !definition.TryNumber("Farea" + suffix, out double full, out _) ||
                !definition.TryNumber("Harea" + suffix, out double half, out _) || !definition.TryNumber("Qarea" + suffix, out double quarter, out _) ||
                !definition.TryNumber("Hfact" + suffix, out double halfFactor, out _) || !definition.TryNumber("Qfact" + suffix, out double quarterFactor, out _))
                return; // Sparse unrelated splash declarations remain primary-only, not invented AoE.
            if (full < 0 || half < full || quarter < half || halfFactor < 0 || halfFactor > 1 || quarterFactor < 0 || quarterFactor > halfFactor)
                throw new InvalidOperationException("Invalid authored splash rings.");
            shot.splashTargets=targets;shot.splashFull=full;shot.splashHalf=half;shot.splashQuarter=quarter;
            shot.splashHalfFactor=halfFactor;shot.splashQuarterFactor=quarterFactor;
        }

        // Runtime inference from LiA3.9c/1.26 RANGE_BOUNDARY2, schema11:
        // n008->H008, H008->n008, n008->hfoo, hfoo->n008 hit at
        // 147.8999/167.8999/154.8999/144.8999 WC respectively. Each is
        // rangeN1 + both collision radii - .1. The +.1 rows move about .12
        // before release, so they are not stationary negative observations.
        // Destructable footprint range and moving-target range buffer are not
        // inferred from these unit-to-unit observations.
        double UnitAttackCenterRange(double range, OriginalWorldUnitView actor, double targetRadius, int weapon = 0)
        {
            var definition = combatCatalog.Unit(actor.rawcode);
            if (weapon == 0) weapon = NativeWeaponIndex(definition);
            if(IsNativeRooted(actor.entityId) && definition.Text("weapTp" + weapon)=="normal")
            {
                // A0KV Aens DataC1=128 and BODY2 rooted n008 hits175.8999,
                // misses176.0996 with two24 bodies. Generic Move-block/Hold,
                // paused units and missile weapons do not imply this floor.
                var root=combatCatalog.Ability("A0KV");
                if(root.Text("code")!="Aens" || root.Number("DataC1")!=128)
                    throw new InvalidOperationException("native-root-range-declaration-conflict");
                range=Math.Max(range,root.Number("DataC1"));
            }
            return range+actor.profile.collisionRadius+targetRadius;
        }

        bool WeaponTarget(OriginalWorldSnapshot snapshot, OriginalWorldUnitView actor, out OriginalPoint point, out double radius)
        {
            point = default; radius = 0;
            if (actor.targetKind == OriginalWorldTargetKind.Doodad)
            {
                var target = Array.Find(snapshot.doodads, d => d.editorId == actor.targetId && d.health > 0);
                if (target == null) return false;
                point = target.position; return true;
            }
            var victim = Array.Find(snapshot.units, u => u.entityId == actor.targetId && u.health > 0 && !u.hidden && !u.invulnerable);
            if (victim == null || !Enemies(actor, victim) || !CanSeeForCombat(actor.ownerSlot,victim)) return false;
            point = victim.position; radius = victim.profile.collisionRadius; return true;
        }

        bool Enemies(OriginalWorldUnitView attacker, OriginalWorldUnitView victim) =>
            AreEnemies(attacker.ownerSlot, victim.ownerSlot);

        void ApplyWeaponHit(Projectile shot)
        {
            // ANdh probability is an authored value. Sampling at release and
            // retaining the result on the projectile is the host convention,
            // not a claim about the private native RNG callback boundary.
            if (shot.missed) return;
            if (shot.fixedImpact)
            {
                // Artillery uses the released point and authored flight speed.
                // Point-centered rings and secondary callback order are host
                // reconstruction; later target movement cannot retarget it.
                ApplyWeaponSplash(shot, shot.impactPoint, includePrimary: true);
                if (WeaponTargetTypeAllowed(shot.splashTargets,"debris"))
                    foreach (var doodad in world.Snapshot().doodads)
                    {
                        double factor=WeaponSplashFactor(shot,Math.Sqrt(SquaredDistance(shot.impactPoint,doodad.position)));
                        if(doodad.health>0 && factor>0)world.ApplyDoodadDamage(doodad.editorId,shot.damage*factor);
                    }
                return;
            }
            if (shot.kind == OriginalWorldTargetKind.Doodad)
            {
                world.ApplyDoodadDamage(shot.target, shot.damage);
                return;
            }
            var target = world.UnitState(shot.target);
            if (target == null || target.health <= 0 || target.hidden || target.invulnerable || !AreEnemies(shot.owner, target.ownerSlot)) return;
            if (RollNativeEvasion(target)) return;
            shot.damage=ResolveNativeWeaponProcs(shot,target);
            target=world.UnitState(shot.target);
            if(target==null || target.health<=0 || target.hidden || target.invulnerable)return;
            ApplyItemOrbFrost(shot, target);
            ApplyPoison(shot, target);
            ApplyMeleeReflection(shot, target);
            if(shot.melee)ReflectNativeCarapace(shot.attacker,target,shot.damage);
            target = world.UnitState(shot.target);
            if (target == null || target.health <= 0 || target.hidden || target.invulnerable) return;
            ApplyNativeCorruption(world.UnitState(shot.attacker), target);
            var defense = combatCatalog.Unit(target.rawcode);
            double armor = UsesHeroCombatStats(target) ? CombatStatsFor(target).armor : EnemyArmor(defense, target.entityId);
            double damage = OriginalAttackRules.WeaponDamage(native, IncomingItemWeaponDamage(target, shot.damage, shot.melee), shot.attackType, defense.Text("defType"), armor);
            damage = IncomingInnateWeaponDamage(target, damage);
            damage = ApplyAbilityIncomingWeaponDamage(target, damage, shot.attackType);
            var impact = target.position;
            // ITEMORB2: native secondary callbacks precede the primary hit.
            // A secondary death can run nested handlers that remove its target.
            ApplyItemOrbSplash(shot, impact);
            target = world.UnitState(shot.target);
            if (target == null || target.health <= 0 || target.hidden || target.invulnerable || !AreEnemies(shot.owner, target.ownerSlot)) return;
            ApplyResolvedWeaponOrSpellHit(shot.attacker, shot.owner, target, damage, primaryWeapon: true);
            ApplyWeaponSplash(shot, impact);
            ApplyNativeCleave(shot, impact);
        }

        static double WeaponSplashFactor(Projectile shot,double distance) =>
            distance <= shot.splashFull ? 1 : distance <= shot.splashHalf ? shot.splashHalfFactor :
            distance <= shot.splashQuarter ? shot.splashQuarterFactor : 0;

        void ApplyWeaponSplash(Projectile shot, OriginalPoint impact, bool includePrimary = false)
        {
            if (shot.splashTargets == null) return;
            // Source rings/factors/target classification are declarations.
            // Host reconstruction: hostile unit centers at current impact,
            // stable entity order, one primary proc roll, no secondary procs or
            // lifesteal. Edge geometry and native splash/proc ordering were not
            // measured by SUMSP1, which covers the separate ground weapon2.
            foreach (var previous in world.Snapshot().units)
            {
                var target = world.UnitState(previous.entityId);
                if (target == null || !includePrimary && target.entityId == shot.target || target.health <= .405 || target.hidden || target.invulnerable ||
                    !AreEnemies(shot.owner, target.ownerSlot)) continue;
                var defense = combatCatalog.Unit(target.rawcode);
                if (!WeaponTargetTypeAllowed(shot.splashTargets, defense.Text("targType"))) continue;
                double distance = Math.Sqrt(SquaredDistance(impact, target.position));
                double factor = WeaponSplashFactor(shot,distance);
                if (factor <= 0) continue;
                double armor = UsesHeroCombatStats(target) ? CombatStatsFor(target).armor : EnemyArmor(defense, target.entityId);
                double damage = OriginalAttackRules.WeaponDamage(native, IncomingItemWeaponDamage(target, shot.damage * factor, shot.melee), shot.attackType, defense.Text("defType"), armor);
                damage = IncomingInnateWeaponDamage(target, damage);
                damage = ApplyAbilityIncomingWeaponDamage(target, damage, shot.attackType);
                ApplyResolvedWeaponOrSpellHit(shot.attacker, shot.owner, target, damage,
                    primaryWeapon: includePrimary && shot.kind == OriginalWorldTargetKind.Unit && target.entityId == shot.target);
            }
        }

        double EnemyArmor(OriginalCombatDefinition definition, int entityId = 0)
        {
            if (IsCasterTotem(entityId)) return 0;
            double bonus = BossArmorBonus(entityId) + SummonScriptArmorBonus(entityId) + ArcherDebuffArmorDelta(entityId) + NativeCorruptionArmorDelta(entityId) + ItemArmorBonus(entityId) + ItemAuraArmorFlat(entityId) + OrdinaryArmorDelta(entityId);
            double factor = 1 + ItemAuraArmorFraction(entityId) + UniqueSoulArmorFraction(entityId);
            // WARD1 plain40 records native CHAOS/NORMAL event40 before the
            // 8HP ward dies; armor0 is inferred from the event, not HP clamp.
            if (definition.id == "u00H") return bonus;
            // SUMSP1 campaignb2689ac7: n026 intact/Asum-removed1/10/40
            // event damage equals actual life loss, on both native attack flags.
            if (definition.id == "n026") return bonus;
            // ITEMWARD2: both native wards take exact CHAOS/NORMAL2, event2,
            // life5->3, then restoration5. The direct axis proves armor0.
            if (definition.id == "ohwd" || definition.id == "o00J") return bonus;
            if (definition.id == "O006") return BossIntrinsic("O006").RequireArmor() * factor + bonus;
            if (definition.TryNumber("def", out double value, out _)) return value * factor + bonus;
            if (observed == null) throw new InvalidOperationException("Unobserved unit armor: " + definition.id);
            var sparse = observed.sparse?.Unit(definition.id);
            if (sparse != null && sparse.armorKnown) return sparse.RequireArmor() * factor + bonus;
            return observed.Unit(definition.id).RequireArmor() * factor + bonus;
        }

        bool RollWeaponMiss(int id)
        {
            double chance = Math.Min(1, ArcherDebuffMissChance(id) + ItemScriptDebuffMissChance(id));
            if (!OriginalCombatDefinition.IsFinite(chance) || chance < 0 || chance > 1)
                throw new InvalidOperationException("Invalid weapon miss probability.");
            if (chance == 0) return false;
            return (RollWeapon(1000000) - 1) < chance * 1000000;
        }

        int RollWeapon(int sides)
        {
            if (weaponRandom == 0) weaponRandom = unchecked((uint)seed) ^ 0xC2B2AE35u;
            if (weaponRandom == 0) weaponRandom = 1;
            weaponRandom ^= weaponRandom << 13; weaponRandom ^= weaponRandom >> 17; weaponRandom ^= weaponRandom << 5;
            return 1 + (int)(weaponRandom % (uint)sides);
        }
    }
}

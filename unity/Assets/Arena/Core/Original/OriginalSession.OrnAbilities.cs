using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrnCast { internal int actor, target; internal string ability; internal double due; }
        sealed class OrnBolt { internal int actor, owner, target; internal double due; }
        sealed class OrnKick { internal int target; internal double next; internal OriginalOrnKickRules rules; }
        sealed class OrnDecoys
        {
            internal int actor, owner, chosen;
            internal double due;
            internal bool selected;
            internal OriginalPoint[] positions;
        }
        sealed class OrnSpin
        {
            internal int actor, owner, target;
            internal double warningAt, damageAt, chaseAt;
            internal OriginalOrnSpinRules rules = new OriginalOrnSpinRules();
        }
        readonly Dictionary<int, OrnCast> ornCasts = new Dictionary<int, OrnCast>();
        readonly Dictionary<string, double> ornCooldowns = new Dictionary<string, double>();
        readonly List<OrnKick> ornKicks = new List<OrnKick>();
        readonly List<OrnDecoys> ornDecoys = new List<OrnDecoys>();
        readonly List<OrnSpin> ornSpins = new List<OrnSpin>();
        readonly List<OrnBolt> ornBolts = new List<OrnBolt>();
        uint ornAbilityRandom;

        void OnOrnActiveMatchEvent(OriginalMatchEvent item)
        {
            if (item.sourceRule != "final-phases" || item.kind != OriginalMatchEventKind.BossResume) return;
            int actor = OriginalWorld.EnemyEntityId(item.entityId);
            if (world.UnitState(actor)?.rawcode != "O006") return;
            int stage = match.FinalStage;
            // ENv31248-31258, before VD increases in the original trigger.
            string ability = stage == 1 ? "A0TW" : stage == 2 ? "A0U0" : stage == 3 ? "A10K" : null;
            if (ability != null) ApplyUnitAbilityOverlay(actor, new[] { ability }, Array.Empty<string>());
        }

        void ClearOrnNativeStun(int actor)
        {
            if (!actorControls.TryGetValue(actor, out var controls)) return;
            // EBv31371 removes BPSE only, not every negative control. AHbh
            // uses the same native BPSE; its pending expiry observer is dropped.
            foreach (string token in new List<string>(controls.Keys))
                if (token.StartsWith("native-stun:BPSE:", StringComparison.Ordinal) ||
                    token.StartsWith("native-bash:", StringComparison.Ordinal) &&
                    !nativeBashes.Exists(b => b.target == actor && b.token == token && b.buff != "BPSE"))
                    ClearActorControl(actor, token);
        }

        void SelectOrnActiveOrders(OriginalWorldUnitView actor)
        {
            ClearOrnNativeStun(actor.entityId);
            TryStartOrnCast(actor.entityId, "A0U0", 0);
            foreach (var target in world.Snapshot().units)
                if (target.health > .405 && AreEnemies(actor.ownerSlot, target.ownerSlot) && !CasterHasAbility(target, "A0K4") &&
                    SquaredDistance(actor.position, target.position) <= 225 * 225)
                    TryStartOrnCast(actor.entityId, "A0TW", target.entityId);
            if (actor.health / actor.profile.maxHealth <= .45) TryStartOrnCast(actor.entityId, "A10K", 0);
        }

        bool TryStartOrnCast(int actorId, string ability, int targetId)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || actor.rawcode != "O006" || actor.health <= .405 || actor.hidden || actor.paused ||
                ActorCastBlocked(actorId) || !HasEffectiveUnitAbility(actor, ability)) return false;
            var declaration = combatCatalog.Ability(ability);
            string code = ability == "A0TW" ? "AHtb" : ability == "A0U0" ? "Aroa" : ability == "A10K" ? "Absk" : null;
            double expectedCost = ability == "A0TW" ? 50 : ability == "A0U0" ? 100 : 150;
            double expectedCooldown = ability == "A0TW" ? 9 : ability == "A0U0" ? 13 : 12;
            if (code == null || declaration.Text("code") != code || declaration.Number("Cost1") != expectedCost ||
                declaration.Number("Cool1") != expectedCooldown || declaration.overrides.Length != 0)
                throw new InvalidOperationException("orn-native-cast-declaration-conflict:" + ability);
            double cost = declaration.Number("Cost1");
            string key = actorId + ":" + ability;
            if (actor.mana < cost || ornCooldowns.TryGetValue(key, out double until) && until > world.Clock + 1e-9) return false;
            if (ability == "A0TW")
            {
                var target = world.UnitState(targetId);
                if (!ValidOrnBoltTarget(actor.ownerSlot, target) || SquaredDistance(actor.position, target.position) > 225 * 225) return false;
            }
            if (ability == "A10K")
            {
                // ORNC1 campaign71979a4d: all five Absk events are synchronous,
                // with150MP debit and no recovery. Keeping the current order
                // is a family transfer from native A0AS, not measured by ORNC1's idle actor.
                if (!world.TrySpendMana(actorId, cost)) return false;
                ornCooldowns[key] = world.Clock + expectedCooldown;
                world.MarkCast(actorId); NotifyNativeSpellEffect(actorId, "A10K"); BeginOrnSpin(actorId);
                return true;
            }
            OnAcceptedWorldOrder(actorId); world.Stop(actorId); world.MarkCast(actorId);
            if (weaponCycles.TryGetValue(actorId, out var cycle)) cycle.winding = false;
            ornCasts[actorId] = new OrnCast { actor = actorId, target = targetId, ability = ability, due = world.Clock + .3 };
            return true;
        }

        bool ValidOrnBoltTarget(int owner, OriginalWorldUnitView target) => target != null && target.health > .405 &&
            !target.hidden && !target.invulnerable && !CasterMagicImmune(target) && AreEnemies(owner, target.ownerSlot);

        void BeginOrnKick(int actorId, int targetId)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null || CasterHasAbility(target, "B06X")) return;
            var rule = new OriginalOrnKickRules(actor.position, target.position);
            if (target.kind == OriginalWorldUnitKind.Illusion && world.ForceUnitDeath(targetId)) OnPyroUnitDied(targetId);
            world.SetPathingEnabled(targetId, false); world.SetUnitState(targetId, paused: true);
            // Native Arav/fly-height visuals are not represented by the ground
            // body. The retained dead target still follows the source timer.
            ornKicks.Add(new OrnKick { target = targetId, rules = rule, next = world.Clock + OriginalOrnKickRules.Period });
        }

        double NextOrnAbilityRandom()
        {
            if (ornAbilityRandom == 0) ornAbilityRandom = unchecked((uint)seed) ^ 0x0A6E0F31u;
            if (ornAbilityRandom == 0) ornAbilityRandom = 1;
            ornAbilityRandom ^= ornAbilityRandom << 13; ornAbilityRandom ^= ornAbilityRandom >> 17; ornAbilityRandom ^= ornAbilityRandom << 5;
            return ornAbilityRandom / 4294967296.0;
        }

        void BeginOrnDecoys(int actorId)
        {
            var actor = world.UnitState(actorId); if (actor == null) return;
            var positions = new OriginalPoint[8];
            for (int slot = 8; slot >= 1; slot--)
            {
                // Source Eav reads retained HK handles, including a hero whose
                // human disconnected and whose orders are now driven by AI.
                var player = players.Find(p => p.matchSlot == slot);
                var hero = player == null ? null : world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                if (hero != null) positions[slot - 1] = hero.position; // No living filter in Eav.
                else
                {
                    double angle = NextOrnAbilityRandom() * 6.2832, distance = NextOrnAbilityRandom() * 900;
                    positions[slot - 1] = new OriginalPoint(actor.position.x + distance * Math.Cos(angle), actor.position.y + distance * Math.Sin(angle));
                }
            }
            world.SetUnitState(actorId, paused: true, invulnerable: true);
            // Logical h016 helpers preserve the authored centers. Native
            // CreateUnit position adjustment and group/RNG order are not replayed.
            ornDecoys.Add(new OrnDecoys { actor = actorId, owner = actor.ownerSlot, positions = positions,
                due = world.Clock + OriginalOrnAbilityRules.DecoyChoiceDelay });
        }

        void BeginOrnSpin(int actorId)
        {
            var actor = world.UnitState(actorId); if (actor == null) return;
            ornSpins.Add(new OrnSpin { actor = actorId, owner = actor.ownerSlot,
                warningAt = world.Clock + OriginalOrnSpinRules.WarningPeriod });
        }

        void AdvanceOrnAbilities()
        {
            foreach (var cast in new List<OrnCast>(ornCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= .405 || actor.paused || actor.hidden || ActorCastBlocked(cast.actor))
                { ornCasts.Remove(cast.actor); continue; }
                if (cast.due > world.Clock + 1e-9) continue;
                ornCasts.Remove(cast.actor);
                if (cast.ability == "A0TW" && !ValidOrnBoltTarget(actor.ownerSlot, world.UnitState(cast.target))) continue;
                var declaration = combatCatalog.Ability(cast.ability);
                if (!world.TrySpendMana(cast.actor, declaration.Number("Cost1"))) continue;
                ornCooldowns[cast.actor + ":" + cast.ability] = cast.due + declaration.Number("Cool1");
                // ORNC1: EFFECT=.3, FINISH=.81 for AHtb/Aroa. Source spell
                // handlers run at EFFECT, independently from the remaining backswing.
                nativeAttackRecovery[cast.actor] = cast.due + .51;
                NotifyNativeSpellEffect(cast.actor, cast.ability);
                if (cast.ability == "A0TW")
                {
                    BeginOrnKick(cast.actor, cast.target);
                    ornBolts.Add(new OrnBolt { actor = cast.actor, owner = actor.ownerSlot, target = cast.target, due = cast.due + .005 });
                }
                else if (cast.ability == "A0U0") BeginOrnDecoys(cast.actor);
            }
            foreach (var bolt in ornBolts.ToArray())
            {
                if (bolt.due > world.Clock + 1e-9) continue;
                ornBolts.Remove(bolt);
                var target = world.UnitState(bolt.target);
                if (!ValidOrnBoltTarget(bolt.owner, target)) continue;
                // ORNC1 at165WC: sparse native damage is0; two callbacks
                // bracket BPSE addition. The .005 latency at other points within
                // authored225 range is an explicit host transfer, not a missile law.
                ApplyResolvedUnitHit(bolt.actor, bolt.owner, target, 0);
                target = world.UnitState(bolt.target);
                if (!ValidOrnBoltTarget(bolt.owner, target)) continue;
                string token = "native-stun:BPSE:" + bolt.actor;
                if (!SetActorControl(bolt.target, token, AllActorControls, 0, true, true)) continue;
                nativeBashes.RemoveAll(s => s.target == bolt.target && s.token == token);
                // Reuse BPSE lifetime/cleanse bookkeeping. CONTROL2 proves pause
                // freezing; ORNC1's natural expiry emits the final0 callback.
                nativeBashes.Add(new NativeBashState { target = bolt.target, source = bolt.actor, owner = bolt.owner, token = token, remaining = 2 });
                ApplyResolvedUnitHit(bolt.actor, bolt.owner, world.UnitState(bolt.target), 0);
            }
            foreach (var kick in ornKicks.ToArray())
                while (kick.next <= world.Clock + 1e-9)
                {
                    kick.next += OriginalOrnKickRules.Period;
                    var target = world.UnitState(kick.target);
                    if (target == null) { ornKicks.Remove(kick); break; }
                    bool active = match.Phase == OriginalMatchPhase.Combat || match.Phase == OriginalMatchPhase.FinalIntermission;
                    if (kick.rules.Tick(target.position, active, out var next)) world.ForcePosition(kick.target, next);
                    else
                    {
                        world.SetUnitState(kick.target, paused: false); world.SetPathingEnabled(kick.target, true);
                        ornKicks.Remove(kick); break;
                    }
                }
            foreach (var copies in ornDecoys.ToArray())
            {
                if (copies.due > world.Clock + 1e-9) continue;
                if (!copies.selected)
                {
                    copies.selected = true; copies.chosen = (int)(NextOrnAbilityRandom() * 8);
                    var point = copies.positions[copies.chosen];
                    world.ForcePosition(copies.actor, OriginalShieldBashRules.AllowsForcedPoint(point) ? point : new OriginalPoint(0, -2680));
                    copies.due += OriginalOrnAbilityRules.DecoyBurstDelay;
                    if (copies.due > world.Clock + 1e-9) continue;
                }
                ornDecoys.Remove(copies);
                for (int index = 7; index >= 0; index--)
                {
                    foreach (var target in world.Snapshot().units)
                        if (target.health > .405 && AreEnemies(copies.owner, target.ownerSlot) && !CasterHasAbility(target, "A0K4") &&
                            SquaredDistance(copies.positions[index], target.position) <= OriginalOrnAbilityRules.DecoyRadius * OriginalOrnAbilityRules.DecoyRadius)
                            ApplyTriggeredHit(0, copies.owner, target, OriginalOrnAbilityRules.DecoyDamage(target.profile.maxHealth, index == copies.chosen),
                                OriginalTriggeredDamageMode.SpellMagic);
                    // Source Eev restores these states independently for every
                    // copy; RemoveUnit(h016) is not a native death notification.
                    world.SetUnitState(copies.actor, paused: false, invulnerable: false);
                }
            }
            foreach (var spin in ornSpins.ToArray()) AdvanceOrnSpin(spin);
        }

        void AdvanceOrnSpin(OrnSpin spin)
        {
            var actor = world.UnitState(spin.actor);
            if (actor == null) { ornSpins.Remove(spin); return; }
            if (!spin.rules.Started)
            {
                while (spin.warningAt <= world.Clock + 1e-9 && !spin.rules.Started)
                {
                    double at = spin.warningAt; spin.warningAt += OriginalOrnSpinRules.WarningPeriod;
                    if (!spin.rules.TickWarning()) continue;
                    ApplyUnitAbilityOverlay(spin.actor, new[] { "A077" }, Array.Empty<string>());
                    world.SetPathingEnabled(spin.actor, false); world.SetUnitState(spin.actor, paused: true);
                    foreach (var target in world.Snapshot().units)
                        if (target.health > .405 && AreEnemies(spin.owner, target.ownerSlot) &&
                            target.kind != OriginalWorldUnitKind.Illusion && (target.kind == OriginalWorldUnitKind.Hero || target.rawcode == "O006") &&
                            SquaredDistance(actor.position, target.position) <= 2000 * 2000) { spin.target = target.entityId; break; }
                    // EOv's original exitwhen b loop never terminates for an
                    // empty/dead party. Bounded traversal keeps its damage timer
                    // and omits chase as explicit recovery from that source bug.
                    spin.damageAt = at + OriginalOrnSpinRules.DamagePeriod;
                    spin.chaseAt = at + OriginalOrnSpinRules.ChasePeriod;
                }
            }
            if (!spin.rules.Started) return;
            for (int callbacks = 0; callbacks < 512; callbacks++)
            {
                bool canChase = !spin.rules.ChaseCompleted && spin.target != 0;
                double next = spin.rules.DamageCompleted ? double.PositiveInfinity : spin.damageAt;
                if (canChase) next = Math.Min(next, spin.chaseAt);
                if (next > world.Clock + 1e-9) break;
                actor = world.UnitState(spin.actor); if (actor == null) { ornSpins.Remove(spin); return; }
                // EEv timer is created first, so equal deadlines damage first.
                if (!spin.rules.DamageCompleted && spin.damageAt <= next + 1e-9)
                {
                    spin.damageAt += OriginalOrnSpinRules.DamagePeriod;
                    if (spin.rules.TickDamage(actor.health > .405))
                    {
                        foreach (var target in world.Snapshot().units)
                            if (target.health > .405 && AreEnemies(spin.owner, target.ownerSlot) &&
                                SquaredDistance(actor.position, target.position) <= OriginalOrnSpinRules.Radius * OriginalOrnSpinRules.Radius)
                                ApplyTriggeredHit(spin.actor, spin.owner, target, OriginalOrnSpinRules.Damage, OriginalTriggeredDamageMode.ChaosUniversal);
                    }
                    else
                    {
                        world.SetUnitState(spin.actor, paused: false); world.SetPathingEnabled(spin.actor, true);
                        ApplyUnitAbilityOverlay(spin.actor, Array.Empty<string>(), new[] { "A077" });
                    }
                }
                else
                {
                    spin.chaseAt += OriginalOrnSpinRules.ChasePeriod;
                    var target = world.UnitState(spin.target);
                    if (target == null) spin.target = 0;
                    else world.ForcePosition(spin.actor, spin.rules.TickChase(actor.position, target.position));
                }
            }
            if (spin.rules.DamageCompleted && (spin.rules.ChaseCompleted || spin.target == 0)) ornSpins.Remove(spin);
        }

        void AppendOrnAbilityVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var copies in ornDecoys)
                for (int i = 0; i < copies.positions.Length; i++)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A0U0",
                        sourceEntityId = copies.actor, position = copies.positions[i], radius = OriginalOrnAbilityRules.DecoyRadius,
                        progress = copies.selected ? 1 : .5, variant = copies.selected && copies.chosen == i ? 1 : 0 });
            foreach (var spin in ornSpins)
            {
                var actor = world.UnitState(spin.actor); if (actor == null) continue;
                output.Add(new OriginalVisualEffectView { kind = spin.rules.Started ? OriginalVisualEffectKind.ActiveCircle : OriginalVisualEffectKind.WarningCircle,
                    abilityId = "A10K", sourceEntityId = spin.actor, position = actor.position, radius = OriginalOrnSpinRules.Radius, progress = 1 });
            }
        }
    }
}

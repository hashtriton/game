using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly HashSet<int> casterAntiHealTargets = new HashSet<int>();

        void ValidateCasterHelper(OriginalCasterRules rule)
        {
            if(rule.family==OriginalCasterFamily.Totem||rule.family==OriginalCasterFamily.SlowAura||
                rule.family==OriginalCasterFamily.SpellCurseAura||rule.family==OriginalCasterFamily.ReverseOrderAura)
                _=new OriginalCasterFieldRules(combatCatalog,rule.abilityId);
            // Validate native defaults before accepting an order/debit, not
            // after the first authored damage pulse has already executed.
            if (rule.family == OriginalCasterFamily.FrostPulse)
                _ = new OriginalArcherDebuffRules(combatCatalog, native, "A168", 1);
            if (rule.family == OriginalCasterFamily.SleepWave) ValidateNativeSleep();
            if (rule.family == OriginalCasterFamily.EnemyImage)
                _ = new OriginalEnemyImageRules(combatCatalog, rule.abilityId == "A120" ? 2 : 1);
            if (rule.family == OriginalCasterFamily.Pull || rule.family == OriginalCasterFamily.Drag)
                ValidateCasterRoot();
            if (rule.family == OriginalCasterFamily.StrikeSummon)
            {
                _ = ScriptedEnemyProfile("n07C");
                _ = combatCatalog.Unit("n07C").Number("mana0");
                ValidateAbilityChanges(new[] { "A0QE", "A0K4" });
            }
        }

        void ActivateCasterStrikeSummon(CasterEffect effect)
        {
            // OVv33459: magic immunity does not exclude the HERO gate, but
            // it excludes the subsequent350 magic damage. Both summons are
            // Player11, unscaled, default userData0 and two rank1 additions.
            if (CasterHeroGate(effect))
            {
                foreach (var target in CasterUnits())
                    if (CasterEligible(effect, target, effect.rules.radius, true))
                        CasterScriptDamage(effect, target, effect.rules.damage, 2);
                for (int i = 0; i < 2; i++)
                    SpawnScriptedEnemy("n07C", effect.center, 0, new[] { "A0QE", "A0K4" }, Array.Empty<string>());
            }
            CompleteCasterEffect(effect);
        }

        void TickCasterFrostPulse(CasterEffect effect)
        {
            if (effect.ticks == 0) { CompleteCasterEffect(effect); return; }
            // OQv34645 performs hL first, then the same helper issues frostnova
            // to each survivor. HELPER1 confirmed multiple orders from one
            // helper; ARCHH2 supplies the shared frost buff implementation.
            foreach (var target in CasterUnits())
                if (CasterEligible(effect, target, effect.rules.radius))
                {
                    CasterScriptDamage(effect, target, effect.rules.damage, 2);
                    OrderArcherNativeHelper(effect.owner, target.entityId, "A168", 1);
                }
            effect.ticks--; effect.nextAt += 1;
        }

        void BeginCasterAntiHeal(CasterEffect effect)
        {
            OriginalWorldUnitView chosen = null;
            foreach (var target in CasterUnits())
                if (CasterEligible(effect, target, effect.rules.radius, true) &&
                    !CasterHasAbility(target, "A0ZK") && !casterAntiHealTargets.Contains(target.entityId)) chosen = target;
            // Oov selects the last eligible native HERO, not every actor in
            // the circle. The host uses its stable enumeration order.
            if (chosen == null) { CompleteCasterEffect(effect); return; }
            effect.target = chosen.entityId; effect.healthFloor = chosen.health;
            casterAntiHealTargets.Add(chosen.entityId);
            effect.nextAt += .03;
        }

        void TickCasterAntiHeal(CasterEffect effect)
        {
            // Oxv performs its SetUnitState clamp before the333rd cleanup.
            // This is neither damage nor healing; armor and invulnerability
            // do not intercept it, and it cannot credit an attacker.
            var target = world.UnitState(effect.target);
            if (target == null) effect.healthFloor = 0;
            else
            {
                if (target.health > effect.healthFloor)
                {
                    if (effect.healthFloor == 0)
                    {
                        if (world.ForceUnitDeath(target.entityId))
                        {
                            OnPyroUnitDied(target.entityId);
                            if (target.kind == OriginalWorldUnitKind.Hero) ReportHeroDied(target.ownerSlot, 0);
                        }
                    }
                    else world.UpdateProfile(target.entityId, target.profile, effect.healthFloor, target.mana);
                }
                else effect.healthFloor = target.health;
            }
            if (--effect.ticks == 0) CompleteCasterEffect(effect);
            else effect.nextAt += .03;
        }

        void BeginCasterAllyThrow(CasterEffect effect)
        {
            OriginalWorldUnitView chosen = null;
            foreach (var target in CasterUnits())
                if (CasterEligible(effect, target, effect.rules.radius, true, false)) chosen = target;
            if (chosen == null) { CompleteCasterEffect(effect); return; }
            effect.target = chosen.entityId; effect.captured = new HashSet<int>(); effect.nextAt += 1.01;
        }

        void TickCasterAllyThrow(CasterEffect effect)
        {
            if (effect.throwing)
            {
                var actor = world.UnitState(effect.actor); var target = world.UnitState(effect.target);
                // HELPER1 proves a missing native source rejects the hit. A
                // removed actor cannot be replaced with a synthetic attacker.
                if (actor == null || target == null) { casterEffects.Remove(effect); return; }
                effect.travelled += 28;
                double dx = target.position.x - effect.origin.x, dy = target.position.y - effect.origin.y;
                double angle = Math.Atan2(dy, dx), distance = Math.Sqrt(dx * dx + dy * dy);
                effect.center = new OriginalPoint(effect.origin.x + effect.travelled * Math.Cos(angle),
                    effect.origin.y + effect.travelled * Math.Sin(angle));
                world.ForcePosition(actor.entityId, effect.center); world.SetFacing(actor.entityId, angle * 180 / Math.PI);
                if (effect.travelled >= distance)
                {
                    CasterScriptDamage(effect, target, effect.rules.damage, 3);
                    // Obv does not change MC; only the parent Ocv owns it.
                    casterEffects.Remove(effect);
                }
                else effect.nextAt += .04;
                return;
            }
            if (effect.ticks == 0) { CompleteCasterEffect(effect); return; }
            effect.ticks--;
            var source = world.UnitState(effect.actor);
            var center = source?.position ?? new OriginalPoint(0, 0);
            OriginalWorldUnitView selected = null;
            foreach (var ally in CasterUnits())
            {
                if (ally.entityId == effect.actor || ally.health <= .405 || AreEnemies(effect.owner, ally.ownerSlot) ||
                    SourceUnitUserData(ally.entityId) == 1 || effect.captured.Contains(ally.entityId) ||
                    SquaredDistance(center, ally.position) > 500 * 500) continue;
                effect.captured.Add(ally.entityId); selected = ally;
            }
            // Ocv marks every eligible unit, then launches only the last one.
            // Its later null selections produce no unit/movement/damage event.
            if (selected != null)
                casterEffects.Add(new CasterEffect { actor = selected.entityId, owner = selected.ownerSlot,
                    target = effect.target, rules = effect.rules, throwing = true, warning = false,
                    origin = selected.position, center = selected.position, nextAt = effect.nextAt + .04 });
            effect.nextAt += 1.01;
        }
    }
}

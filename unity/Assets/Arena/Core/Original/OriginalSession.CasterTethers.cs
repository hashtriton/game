using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        long nextCasterTetherToken;

        void ValidateCasterRoot()
        {
            var root = combatCatalog.Ability("A0KV");
            if (root == null || root.Text("code") != "Aens" || root.Text("targs1") != "enemies,ground,neutral" ||
                root.Text("BuffID1") != "Bena,B08D" || root.Number("HeroDur1") != 6 || root.Number("Dur1") != 6 ||
                root.Number("DataC1") != 128)
                throw new InvalidOperationException("caster-root-native-declaration-conflict");
        }

        void BeginCasterTether(CasterEffect effect)
        {
            bool drag = effect.rules.family == OriginalCasterFamily.Drag;
            OriginalWorldUnitView chosen = null;
            foreach (var unit in CasterUnits())
                if (CasterEligible(effect, unit, effect.rules.radius, true, !drag)) chosen = unit;
            var caster = world.UnitState(effect.actor);
            if (chosen == null || caster == null) { CompleteCasterEffect(effect); return; }
            effect.target = chosen.entityId;
            effect.controlToken = "caster-tether:" + checked(++nextCasterTetherToken);
            var origin = drag ? effect.center : chosen.position;
            var destination = drag ? OriginalCasterTetherRules.DragDestination(CasterRandomUnit(), CasterRandomUnit()) : caster.position;
            effect.tether = new OriginalCasterTetherRules(drag, origin, destination);
            if (drag)
            {
                world.SetPathingEnabled(effect.actor, false);
                // Preserve OFv's literal SetUnitY(u,x), before the first tick.
                world.ForcePosition(effect.actor, OriginalCasterTetherRules.DragInitialCasterPosition(effect.center));
                world.SetUnitState(effect.actor, paused: true);
            }
            world.SetPathingEnabled(effect.target, false);
            SetNativeAbun(effect.target, effect.controlToken, true);
            if (!drag) CasterScriptDamage(effect, chosen, effect.rules.damage, 2);
            chosen = world.UnitState(effect.target);
            // Native order targets a ground, nonimmune living enemy. ABUN1
            // closes the control axes, not sub-frame missile application.
            if (chosen != null && chosen.health > .405 && !chosen.invulnerable && !CasterMagicImmune(chosen) &&
                combatCatalog.Unit(chosen.rawcode).Text("movetp") != "fly")
                AddTimedNativeRoot(effect.target, effect.actor, 6);
            effect.nextAt += OriginalCasterTetherRules.Interval;
        }

        void TickCasterTether(CasterEffect effect)
        {
            var caster = world.UnitState(effect.actor);
            var target = world.UnitState(effect.target);
            // A removed native handle has no live body. A dead-but-retained
            // handle still participates in the final movement/damage callback.
            if (caster == null || target == null) { EndCasterTether(effect); return; }
            var position = effect.tether.Advance(caster.position);
            world.ForcePosition(effect.target, position);
            if (effect.tether.drag)
            {
                world.ForcePosition(effect.actor, position);
                world.SetFacing(effect.actor, Math.Atan2(effect.tether.destination.y - effect.tether.origin.y,
                    effect.tether.destination.x - effect.tether.origin.x) * 180 / Math.PI);
                CasterScriptDamage(effect, world.UnitState(effect.target), effect.tether.DamagePerTick, 3);
            }
            effect.center = position;
            caster = world.UnitState(effect.actor); target = world.UnitState(effect.target);
            if (effect.tether.ShouldFinish(caster?.health ?? 0, target?.health ?? 0)) EndCasterTether(effect);
            else effect.nextAt += OriginalCasterTetherRules.Interval;
        }

        void EndCasterTether(CasterEffect effect)
        {
            world.SetPathingEnabled(effect.target, true);
            SetNativeAbun(effect.target, effect.controlToken, false);
            ClearNativeRoot(effect.target);
            if (effect.tether.drag)
            {
                world.SetPathingEnabled(effect.actor, true);
                world.SetUnitState(effect.actor, paused: false);
            }
            // Oav/Ofv KillUnit their retained helper before removing it. This
            // is another CA death notification, even if its caster just died.
            CompleteCasterEffect(effect);
            ObserveScriptedHelperDeath();
        }
    }
}

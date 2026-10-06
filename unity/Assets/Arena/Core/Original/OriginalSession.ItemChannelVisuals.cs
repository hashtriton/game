using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // Detached render state only. It never advances a projectile or applies damage.
        void AppendItemChannelVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var wave in itemFireWaves)
                output.Add(new OriginalVisualEffectView{kind=OriginalVisualEffectKind.Orb,abilityId="A08A",sourceEntityId=wave.actor,
                    position=wave.point,radius=wave.radius,progress=1});
            foreach (var effect in itemChannelEffects)
            {
                var position = effect.point;
                string ability = effect.ability;
                bool chain = ability == "chain_heal" || ability == "chain_damage" || ability == "A12Y" || ability == "A12Z";
                if (chain)
                {
                    var target = world.UnitState(effect.target);
                    if (target == null) continue;
                    if (ability == "chain_heal" || ability == "chain_damage") ability = effect.visualAbility??"A0KP";
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Beam,
                        abilityId = ability, sourceEntityId = effect.actor, position = position,
                        end = target.position, radius = 5, progress = 1, variant = 1 });
                    continue;
                }
                if (ability == "A17V") position = StepItemPoint(effect.point, effect.destination, effect.travel, true);
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb,
                    abilityId = ability, sourceEntityId = effect.actor, position = position,
                    radius = ability == "A0CC" ? 40 : 22, progress = 1 });
            }
            foreach (var field in itemProtectionFields)
            {
                if (field.totem)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.ActiveCircle,
                        abilityId = "A0M9", sourceEntityId = field.actor, position = field.point,
                        radius = 600, progress = Math.Min(1, field.ticks / 30d) });
                else
                    foreach (int id in field.recipients)
                    {
                        var unit = world.UnitState(id);
                        if (unit == null || unit.health <= .405 || !HasEffectiveUnitAbility(unit, "A19J")) continue;
                        output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.ActiveCircle,
                            abilityId = "A0C5", sourceEntityId = field.actor, position = unit.position,
                            radius = field.burning ? 250 : 70, progress = 1, variant = field.burning ? 1 : 0 });
                    }
            }
            foreach (var bolt in itemCrossbowBolts)
            {
                var target = world.UnitState(bolt.target);
                if (target == null) continue;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb,
                    abilityId = "A104", sourceEntityId = bolt.owner,
                    position = StepItemPoint(bolt.start, target.position, bolt.travel, true), radius = 12, progress = 1 });
            }
            foreach (var dash in spaceBootDashes)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb,
                    abilityId = "A11S", sourceEntityId = dash.actor, position = dash.point, radius = 24, progress = 1 });
            foreach(var jump in itemJetJumps)
            {
                var actor=world.UnitState(jump.actor);if(actor==null)continue;
                output.Add(new OriginalVisualEffectView {kind=OriginalVisualEffectKind.Beam,
                    abilityId=jump.exchange?"A0NA":"A0FI",sourceEntityId=jump.actor,position=jump.start,
                    end=actor.position,radius=9,progress=1,variant=1});
            }
            foreach(var moon in itemMoonExpiries)
            {
                var actor=world.UnitState(moon.Key);if(actor==null||actor.health<=.405)continue;
                output.Add(new OriginalVisualEffectView {kind=OriginalVisualEffectKind.ActiveCircle,
                    abilityId="A0JE",sourceEntityId=moon.Key,position=actor.position,radius=65,progress=1});
            }
            foreach(var eros in erosAmulets)
            {
                var actor=world.UnitState(eros.Key);if(actor==null||actor.health<=.405)continue;
                output.Add(new OriginalVisualEffectView {kind=OriginalVisualEffectKind.ActiveCircle,
                    abilityId="A0YK",sourceEntityId=eros.Key,position=actor.position,radius=60,progress=1});
            }
        }
    }
}

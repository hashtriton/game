using System;

namespace Arena.Original
{
    [Serializable]
    public struct OriginalHeroCombatView
    {
        public bool known;
        public string primaryAttribute;
        public double strength,agility,intelligence,armor,attackMinimum,attackMaximum,attackInterval;
    }

    public sealed partial class OriginalSession
    {
        OriginalHeroCombatView HeroCombatView(Player player)
        {
            var actor=world?.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if(actor==null||native==null||player.progression==null||player.stats==null||!string.IsNullOrEmpty(HaltReason))return default;
            var hero=HeroCombatStats(player.slot);var stats=CombatStatsFor(actor);
            var definition=combatCatalog.Unit(actor.rawcode);int weapon=NativeWeaponIndex(definition);
            double white=definition.Number("dmgplus"+weapon)+stats.primaryDamageBonus+stats.upgradeAttackDamageBonus;
            double dice=definition.Number("dice"+weapon),sides=definition.Number("sides"+weapon);
            double Damage(double rolled)
            {
                double baseDamage=white+rolled;
                double flat=stats.itemAttackDamageBonus+NativeCommandDamageBonus(actor,weapon,baseDamage)+
                    ItemAuraDamageBonus(actor.entityId,weapon,baseDamage);
                // Display the ordinary weapon range. Critical/proc/one-shot
                // bonuses are evaluated at their actual release/hit events.
                return Math.Max(0,ApplyCrippleWeaponDamage(actor.entityId,baseDamage,flat));
            }
            return new OriginalHeroCombatView{known=true,primaryAttribute=hero.primaryAttribute,
                strength=hero.strength.Require(),agility=hero.agility.Require(),intelligence=hero.intelligence.Require(),
                armor=stats.armor,attackMinimum=Damage(dice),attackMaximum=Damage(dice*sides),
                attackInterval=definition.Number("cool"+weapon)/WeaponRate(actor)};
        }
    }
}

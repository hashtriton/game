using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        double NativeCommandDamageBonus(OriginalWorldUnitView recipient,int weapon,double rolledWhiteDamage)
        {
            if(recipient==null || recipient.kind==OriginalWorldUnitKind.Illusion)return 0;
            bool melee=combatCatalog.Unit(recipient.rawcode).Text("weapTp"+weapon)=="normal";
            double factor=0;
            foreach(var emitter in world.Snapshot().units)
            {
                if(emitter.kind==OriginalWorldUnitKind.Illusion)continue;
                foreach(var ability in NativeUnitAbilities(emitter))
                {
                    if(ability.Text("code")!="AOac" || !ItemAuraRecipient(emitter,recipient,ability,1))continue;
                    // 1.26 AbilityMetaData Ear2/Ear3 are melee/ranged flags.
                    // A079 explicitly supplies both1, Area1000 and DataA.4.
                    if(!ability.TryNumber(melee?"DataB1":"DataC1",out double enabled,out _) || enabled!=1 ||
                        !ability.TryNumber("DataA1",out double bonus,out _) || bonus<0)continue;
                    factor=Math.Max(factor,bonus);
                }
            }
            // Continuous membership, strongest family source and percentage of
            // this rolled white component are host reconstruction. The aura's
            // green bonus remains outside Cripple's white-damage reduction.
            return rolledWhiteDamage*factor;
        }

        double NativeThornsFraction(OriginalWorldUnitView defender)
        {
            if(defender.kind==OriginalWorldUnitKind.Illusion)return 0;
            double fraction=0;
            foreach(var emitter in world.Snapshot().units)
            {
                if(emitter.kind==OriginalWorldUnitKind.Illusion)continue;
                foreach(var ability in NativeUnitAbilities(emitter))
                {
                    if(ability.Text("code")!="AEah" || !ability.TryNumber("DataB1",out double percent,out _) || percent!=1 ||
                        !ability.TryNumber("DataA1",out double value,out _) || value<0)continue;
                    bool applies=ability.Text("targs1")=="self"
                        ? emitter.entityId==defender.entityId && emitter.health>.405 && !emitter.hidden
                        : ItemAuraRecipient(emitter,defender,ability,1);
                    if(applies)fraction=Math.Max(fraction,value);
                }
            }
            // AEah raw basis/type resolution is native measured. Membership,
            // strongest-source combination and no lingering after emitter
            // removal transfer the declared aura to the host simulation.
            return fraction;
        }

        void ApplyNativeCleave(Projectile shot,OriginalPoint impact)
        {
            if(!shot.melee || shot.damage<=0)return;
            var actor=world.UnitState(shot.attacker);
            if(actor==null || actor.kind==OriginalWorldUnitKind.Illusion)return;
            foreach(var ability in NativeUnitAbilities(actor))
            {
                if(ability.Text("code")!="ANca" || !ability.TryNumber("Area1",out double radius,out _) || radius<=0 ||
                    !ability.TryNumber("DataA1",out double fraction,out _) || fraction<=0)continue;
                string targets=ability.Text("targs1");if(targets==null || targets=="_")continue;
                // A15J .5/200WC and ACce .6/180WC are map declarations. Native
                // Pit Lord documentation establishes ground-enemy splash.
                // Host reconstruction uses target-centered radius, raw damage
                // after primary procs and attack-type matrix without numeric
                // armor. Exact cleave geometry/order and stacking are not a
                // measured 1.26 control. Secondary hits never recurse/proc.
                foreach(var previous in world.Snapshot().units)
                {
                    var target=world.UnitState(previous.entityId);
                    if(target==null || target.entityId==shot.target || target.health<=.405 || target.hidden || target.invulnerable ||
                        !AreEnemies(shot.owner,target.ownerSlot) || SquaredDistance(impact,target.position)>radius*radius)continue;
                    var definition=combatCatalog.Unit(target.rawcode);
                    if(!WeaponTargetTypeAllowed(targets,definition.Text("targType")))continue;
                    double damage=shot.damage*fraction*OriginalAttackRules.DamageTypeMultiplier(native,shot.attackType,definition.Text("defType"));
                    damage=IncomingInnateWeaponDamage(target,damage);
                    damage=ApplyAbilityIncomingWeaponDamage(target,damage,shot.attackType);
                    ApplyResolvedUnitHit(shot.attacker,shot.owner,target,damage);
                }
            }
        }
    }
}

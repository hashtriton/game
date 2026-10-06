using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ItemOrbSecondary
        {
            internal readonly string ability, targets;
            internal readonly long instance;
            internal readonly double area, damage;
            internal readonly OriginalArcherDebuffRules frost;
            internal ItemOrbSecondary(string ability, long instance, string targets, double area,
                double damage, OriginalArcherDebuffRules frost)
            { this.ability=ability; this.instance=instance; this.targets=targets; this.area=area; this.damage=damage; this.frost=frost; }
        }

        ItemOrbSecondary CaptureItemOrbSecondary(int actorId)
        {
            var row=HighestItemOrb(actorId);
            if(row==null)return null;
            bool frost=row.abilityId=="A062"||row.abilityId=="A0BJ";
            bool fire=row.abilityId=="A07K"||row.abilityId=="A08B";
            if(!frost&&!fire)return null;
            var a=combatCatalog.Ability(row.abilityId);
            if(row.rank!=1||a.Text("code")!=(frost?"AIob":"AIfb")||a.Text("targs1")!="ground,air,ward")
                throw new InvalidOperationException("Item orb family changed.");
            if(frost)return new ItemOrbSecondary(row.abilityId,row.instanceId,a.Text("targs1"),0,0,
                new OriginalArcherDebuffRules(combatCatalog,native,row.abilityId,1));
            double amount=a.Number("DataA1"),area=a.Number("Area1");
            if(amount!=(row.abilityId=="A07K"?15:75)||area!=180)
                throw new InvalidOperationException("Item fire orb declaration changed.");
            // ITEMORB2: detached released I02C missile retains splash75 after
            // native item removal. Transfer to the same ability's other item
            // aliases and simultaneous orb slot arbitration are host policy.
            return new ItemOrbSecondary(row.abilityId,row.instanceId,a.Text("targs1"),area,amount,null);
        }

        bool ItemOrbTarget(Projectile shot,OriginalWorldUnitView target) => target!=null&&
            target.health>.405&&!target.hidden&&!target.invulnerable&&AreEnemies(shot.owner,target.ownerSlot)&&
            WeaponTargetTypeAllowed(shot.itemOrb.targets,combatCatalog.Unit(target.rawcode).Text("targType"));

        void ApplyItemOrbFrost(Projectile shot,OriginalWorldUnitView target)
        {
            if(shot.itemOrb?.frost==null||!ItemOrbTarget(shot,target))return;
            // Declared ground/air/ward targeting, without importing the
            // organic-only dummy spell filter. Other buff IDs compose through
            // the existing host sums; no measured arbitrary-stack claim.
            ApplyArcherNativeBuff(target.entityId,shot.itemOrb.frost);
        }

        void ApplyItemOrbSplash(Projectile shot,OriginalPoint impact)
        {
            var orb=shot.itemOrb;if(orb==null||orb.frost!=null)return;
            // ITEMORB2: one separate15/75 per nearby enemy, hero12/60,
            // Amim edry15/75, no allied hit, and secondary events BEFORE the
            // primary. It is neither a fraction of the shot nor another
            // intrinsic bonus. Centers<=180 transfer the declaration; the
            // measured controls bracket near170 and outside~215..220 only.
            foreach(var old in world.Snapshot().units)
            {
                var target=world.UnitState(old.entityId);
                if(target==null||target.entityId==shot.target||!ItemOrbTarget(shot,target)||
                    SquaredDistance(target.position,impact)>orb.area*orb.area)continue;
                double damage=orb.damage*OriginalAttackRules.DamageTypeMultiplier(native,"spells",combatCatalog.Unit(target.rawcode).Text("defType"));
                // Native splash bypasses armor and Amim. General spell
                // resistance/image/Defend/banish composition is a declared
                // family transfer, not an independent95 damage-flags matrix.
                damage=IncomingMagicDamage(target,damage);
                if(HasBossBanish(target.entityId))damage*=1.66;
                ApplyResolvedUnitHit(shot.attacker,shot.owner,target,
                    BossIncomingTriggeredDamage(target,OriginalTriggeredDamageMode.SpellMagic,damage));
            }
        }
    }
}

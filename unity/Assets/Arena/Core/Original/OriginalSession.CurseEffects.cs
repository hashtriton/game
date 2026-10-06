using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        void AdvanceWorldCurses()
        {
            if(world==null)return;
            foreach(var timer in new List<ColdManaReturn>(coldManaReturns))
                while(timer.next<=world.Clock+1e-9)
                {
                    timer.next+=1;timer.remaining--;
                    if(!timer.helperDied){timer.helperDied=true;ObserveScriptedHelperDeath();}
                    if(timer.remaining<=0){coldManaReturns.Remove(timer);break;}
                    var actor=world.UnitState(timer.actor);
                    if(actor!=null)world.UpdateProfile(actor.entityId,actor.profile,actor.health,Math.Min(actor.profile.maxMana,actor.mana+actor.profile.maxMana*.03));
                }
            while(nextCurseSecond<=world.Clock+1e-9)
            {
                double sourceTime=nextCurseSecond;nextCurseSecond+=1;
                var poisoned=new HashSet<int>();
                foreach(var player in players)
                {
                    var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));if(actor==null)continue;
                    if(HasWorldCurse(player.slot,"A19L"))
                    {
                        // iEv removes Bspe before querying GetUnitMoveSpeed.
                        // Recompose first so an expired positive modifier does
                        // not conceal a surviving slow or trigger a false cap.
                        if(itemHaste.Remove(actor.entityId))
                        {
                            RefreshAbilityMovement(actor.entityId);ReleaseAbilityMovementBase(actor.entityId);
                            actor=world.UnitState(actor.entityId);
                        }
                        if(actor.profile.moveSpeed>250)
                        {
                            heavyCurseCapped.Add(actor.entityId);var profile=actor.profile.Copy();profile.moveSpeed=250;
                            world.UpdateProfile(actor.entityId,profile,actor.health,actor.mana);
                        }
                    }
                    if(HasWorldCurse(player.slot,"A19N"))
                        foreach(var target in world.Snapshot().units)
                        {
                            if(target.entityId==actor.entityId||poisoned.Contains(target.entityId)||AreEnemies(actor.ownerSlot,target.ownerSlot)||
                                target.health<=.405||CasterMagicImmune(target)||HasEffectiveUnitAbility(target,"A0K4")||
                                SquaredDistance(actor.position,target.position)>250*250)continue;
                            poisoned.Add(target.entityId);double amount=target.profile.maxHealth*.02;
                            if(target.health>amount)world.UpdateProfile(target.entityId,target.profile,target.health-amount,target.mana);
                        }
                    if(HasWorldCurse(player.slot,"A19S"))
                    {
                        // A19W ANsi DataA8 is native inventory silence,3s.
                        if(actor.health>.405&&!actor.invulnerable&&!CasterMagicImmune(actor))
                            SetActorControl(actor.entityId,"curse-seal:A19W",OriginalActorControlMask.Item,3,true,true);
                        foreach(var target in world.Snapshot().units)
                            if(target.health>.405&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&!CasterMagicImmune(target)&&
                                !HasEffectiveUnitAbility(target,"A0K4")&&SquaredDistance(actor.position,target.position)<=150*150)
                                ApplyTriggeredHit(actor.entityId,actor.ownerSlot,target,15,OriginalTriggeredDamageMode.SpellMagic);
                    }
                }
                ApplyBloodPoisonSecond(sourceTime);
            }
        }

        void ApplyBloodPoisonSecond(double sourceSeconds)
        {
            // Source initialization sets time-of-day6 and scale1; native
            // MiscData DayLength480,Dawn6,Dusk18. Start-relative phase is the
            // explicit modern lobby policy, rather than the90s menu clock.
            double day=(6+sourceSeconds*24/480)%24,fraction=(day-6)/12;
            if(fraction<0||fraction>1)return;
            double weight=(fraction<.5?fraction:1-fraction)/.5*.02;
            foreach(var player in players)
            {
                if(!HasWorldCurse(player.slot,"A19Q"))continue;
                var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));if(actor==null)continue;
                double amount=actor.profile.maxHealth*weight,health=actor.health;
                if(fraction<.5){if(health>amount)health-=amount;}
                else health=Math.Min(actor.profile.maxHealth,health+amount);
                world.UpdateProfile(actor.entityId,actor.profile,health,actor.mana);
            }
        }
    }
}

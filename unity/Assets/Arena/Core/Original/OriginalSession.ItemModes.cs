using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class SealModeState { internal long instance; internal string item; internal int armor,attack; }
        sealed class WarpathMissile { internal int source,owner,target; internal double damage,distance,due; internal OriginalPoint origin; }
        readonly Dictionary<int,SealModeState> sealModes=new Dictionary<int,SealModeState>();
        readonly List<WarpathMissile> warpathMissiles=new List<WarpathMissile>();
        double nextSealModeTick=1;
        bool warpathDamageRunning;
        bool warpathTriggersEnabled;
        bool IsItemMode(string id)
        {if(OriginalItemModeRules.Family(id)==0)return false;OriginalItemModeRules.Validate(itemCatalog,combatCatalog,id);return true;}
        static OriginalItemInstance ModeItem(OriginalInventory inventory,int family)=>inventory==null?null:
            Array.Find(inventory.HeroSlots,item=>item!=null&&OriginalItemModeRules.Family(item.itemId)==family);
        void SyncItemModeInventory(int actor,OriginalInventory inventory)
        {
            // gp5515 enables Gp once after the first physical I09L pickup.
            // The four global triggers remain enabled after it is dropped.
            if(inventory!=null&&(Array.Exists(inventory.HeroSlots,x=>x?.itemId=="I09L")||
                Array.Exists(inventory.ServantSlots,x=>x?.itemId=="I09L")))warpathTriggersEnabled=true;
            var item=ModeItem(inventory,1);
            if(item==null){sealModes.Remove(actor);return;}
            if(!sealModes.TryGetValue(actor,out var old)||old.instance!=item.instanceId||old.item!=item.itemId)
                sealModes[actor]=new SealModeState{instance=item.instanceId,item=item.itemId};
        }
        void ComposeItemModeStats(OriginalHeroStatsSnapshot stats,OriginalInventory inventory)
        {
            if(inventory==null||!sealModes.TryGetValue(inventory.OwnerId,out var state))return;
            var item=ModeItem(inventory,1);if(item==null||item.instanceId!=state.instance||item.itemId!=state.item)return;
            stats.armor=Plus(stats.armor,state.armor);
            stats.attackMinimum=Plus(stats.attackMinimum,state.attack);
            stats.attackMaximum=Plus(stats.attackMaximum,state.attack);
            stats.itemAttackDamageBonus+=state.attack;
        }
        OriginalSessionReplyCode UseItemMode(Player player,OriginalSessionCommand command,string id)
        {
            if(OriginalItemModeRules.Family(id)==2&&!warpathTriggersEnabled)return OriginalSessionReplyCode.NotReady;
            var actor=world.UnitState(player.slot);var candidate=player.inventory.Copy();
            var replaced=candidate.ReplaceForScript(command.bag,command.itemSlot,command.itemInstanceId,OriginalItemModeRules.Next(id));
            if(!replaced.Applied)return RejectItem(player,replaced.Code);
            try
            {
                var prepared=PrepareEquipmentProfile(player,candidate,actor);
                if(!CommitEquipmentProfile(prepared))return RejectItem(player,OriginalItemActionCode.UnresolvedRule);
            }
            catch(InvalidOperationException){return RejectItem(player,OriginalItemActionCode.UnresolvedRule);}
            player.inventory=candidate;SyncItemModeInventory(player.slot,candidate);player.lastItemAction=OriginalItemActionCode.Success;
            // USE_ITEM replaces the old handle in FT/MW, without a scripted
            // cost or persistent cooldown. Allowing the toggle at full life
            // and immediate activation are explicit AIha host policies until
            // native item activation is measured; never claim DataA=0 healing.
            NotifyNativeSpellEffect(player.slot,OriginalItemModeRules.Ability(id));
            return OriginalSessionReplyCode.Accepted;
        }
        void AdvanceItemModes()
        {
            foreach(var player in players)SyncItemModeInventory(player.slot,player.inventory);
            // QW uses one global1s timer, including paused living heroes.
            while(world.Clock+1e-9>=nextSealModeTick)
            {
                nextSealModeTick+=1;
                foreach(var player in players)
                {
                    if(!sealModes.TryGetValue(player.slot,out var state))continue;
                    if(state.item=="I082"){state.armor=OriginalItemModeRules.Armor(ScriptedAbilityAttack(player));state.attack=0;}
                    else {state.attack=OriginalItemModeRules.Attack(ProbeSealArmor(player));state.armor=0;}
                }
            }
            foreach(var missile in new List<WarpathMissile>(warpathMissiles))
                while(warpathMissiles.Contains(missile)&&world.Clock+1e-9>=missile.due)
                {
                    missile.due+=.03;missile.distance+=24;
                    var target=world.UnitState(missile.target);
                    // Retained handle position follows the native target. A
                    // removed target cannot be read, so terminate its helper.
                    if(target==null){warpathMissiles.Remove(missile);ObserveScriptedHelperDeath();break;}
                    if(missile.distance<Math.Sqrt(SquaredDistance(missile.origin,target.position)))continue;
                    ApplyTriggeredHit(missile.source,missile.owner,target,missile.damage,OriginalTriggeredDamageMode.ChaosUniversal);
                    warpathMissiles.Remove(missile);ObserveScriptedHelperDeath();break;
                }
        }
        double ProbeSealArmor(Player player)
        {
            var actor=world.UnitState(player.slot);if(actor==null||actor.health<.405)return 0;
            double original=actor.health,start=Math.Max(30,original);
            if(start!=original)world.UpdateProfile(actor.entityId,actor.profile,Math.Min(start,actor.profile.maxHealth),actor.mana);
            actor=world.UnitState(actor.entityId);start=actor.health;
            // yP self CHAOS/NORMAL16 preserves observable pre-hit handlers,
            // then restores original life. Native direct ordering with all
            // stacked item modifiers remains the shared damage host policy.
            double damage=IncomingItemWeaponDamage(actor,16,true)*OriginalAttackRules.ArmorMultiplier(native,CombatStatsFor(actor).armor);
            damage=IncomingInnateWeaponDamage(actor,damage);
            damage=ApplyAbilityIncomingWeaponDamage(actor,damage,"chaos");
            ApplyResolvedUnitHit(actor.entityId,actor.ownerSlot,actor,damage);
            var after=world.UnitState(actor.entityId);if(after==null)return 0;
            double z=(16-start+after.health)/16;
            if(after.health>.405)world.UpdateProfile(after.entityId,after.profile,Math.Min(original,after.profile.maxHealth),after.mana);
            if(z>=1)return 917451.519;
            if(z<0)
            {
                // AM binary-search logarithm, preserving its20 iterations.
                double lo=-88,hi=88,middle=0;
                for(int i=0;i<=20;i++){middle=(lo+hi)/2;if(i==20)break;if(Math.Pow(2.71828,middle)>=z+1)hi=middle;else lo=middle;}
                return -middle/-.061875;
            }
            return z/(.06*(1-z));
        }
        bool WarpathBuff(OriginalWorldUnitView target,string buff)
        {
            // Native aura membership is reconstructed continuously without
            // linger. Missing Aakb damage amounts are not assigned zero.
            foreach(var player in players)
            {
                var item=ModeItem(player.inventory,2);if(item==null)continue;
                string ability=item.itemId=="I09L"?"A15U":item.itemId=="I09M"?"A0T1":"A15V";
                var a=combatCatalog.Ability(ability);
                if(a?.Text("BuffID1")==buff&&ItemAuraRecipient(world.UnitState(player.slot),target,a,1))return true;
            }
            return false;
        }
        void ObserveWarpathSpell(int actorId,string ability)
        {
            if(!warpathTriggersEnabled)return;
            var actor=world?.UnitState(actorId);if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero)return;
            var player=PlayerAt(actor.ownerSlot);if(ModeItem(player.inventory,2)?.itemId!="I09N"||!OriginalCasterFieldRules.SpellTriggersCurse(ability,null))return;
            double mana=HeroCombatStats(player.slot).intelligence.Require()*.25;
            foreach(var target in world.Snapshot().units)
                if(WarpathAlly(actor,target,700))world.UpdateProfile(target.entityId,target.profile,target.health,Math.Min(target.profile.maxMana,target.mana+mana));
        }
        void ObserveWarpathKill(int actorId,int deadId)
        {
            if(!warpathTriggersEnabled)return;
            var actor=world.UnitState(actorId);if(actor==null||actor.entityId==deadId||actor.kind!=OriginalWorldUnitKind.Hero)return;
            var player=PlayerAt(actor.ownerSlot);
            if(ModeItem(player.inventory,2)?.itemId!="I09L"||!WarpathBuff(actor,"B0A7"))return;
            double heal=HeroCombatStats(player.slot).strength.Require()*.25;
            foreach(var target in world.Snapshot().units)if(WarpathAlly(actor,target,700))ApplySourceHealing(actorId,target.entityId,heal);
        }
        bool WarpathAlly(OriginalWorldUnitView source,OriginalWorldUnitView target,double radius)=>target.health>.405&&
            !AreEnemies(source.ownerSlot,target.ownerSlot)&&!CasterHasType(target,"structure")&&!HasEffectiveUnitAbility(target,"A0K4")&&
            SquaredDistance(source.position,target.position)<=radius*radius;
        void ObserveWarpathDamage(int sourceId,OriginalWorldUnitView target,double damage)
        {
            if(!warpathTriggersEnabled||warpathDamageRunning||target==null)return;
            int roll=RollWeapon(20); // KT consumes one shared draw per admitted event, even with no equipped mask.
            ResolveWarpathDamage(sourceId,target,damage,roll);
        }
        void ResolveWarpathDamage(int sourceId,OriginalWorldUnitView target,double damage,int roll)
        {
            if(warpathDamageRunning||roll>2||target==null)return;
            var source=world.UnitState(sourceId);if(source==null)return;
            var player=source.ownerSlot>0?PlayerAt(source.ownerSlot):null;
            var item=source.kind==OriginalWorldUnitKind.Hero?ModeItem(player?.inventory,2):null;
            warpathDamageRunning=true;
            try
            {
                if(damage>1&&item?.itemId=="I09M"&&WarpathBuff(target,"B06V"))
                {
                    double amount=HeroCombatStats(player.slot).agility.Require()*.25;
                    foreach(var victim in world.Snapshot().units)
                        if(victim.health>.405&&AreEnemies(source.ownerSlot,victim.ownerSlot)&&!CasterHasType(victim,"structure")&&
                            !CasterMagicImmune(victim)&&!HasEffectiveUnitAbility(victim,"A0K4")&&SquaredDistance(target.position,victim.position)<=350*350)
                            ApplyTriggeredHit(sourceId,source.ownerSlot,victim,amount,OriginalTriggeredDamageMode.SpellMagic);
                }
                target=world.UnitState(target.entityId);if(target==null)return;
                if(damage>20&&item?.itemId=="I09N"&&WarpathBuff(target,"B0A8")&&!IsNativeHeroPredicate(target)&&
                    SourceUnitUserData(target.entityId)!=1&&SourceUnitUserData(target.entityId)!=2&&!CasterMagicImmune(target))
                    ApplyTriggeredHit(sourceId,source.ownerSlot,target,target.health+1,OriginalTriggeredDamageMode.ChaosUniversal);
                // KT intentionally indexes wx by the ATTACKER owner here,
                // although the physical I09L and B0A7 checks use the defender.
                var defender=target.kind==OriginalWorldUnitKind.Hero?PlayerAt(target.ownerSlot):null;
                var definition=combatCatalog.Unit(source.rawcode);
                bool ranged=definition!=null&&definition.TryNumber("weapsOn",out double enabled,out _)&&((int)enabled&1)!=0&&
                    definition.Text("weapTp1")!="normal";
                if(ranged&&damage>1&&ModeItem(player?.inventory,2)?.itemId=="I09L"&&ModeItem(defender?.inventory,2)?.itemId=="I09L"&&WarpathBuff(target,"B0A7"))
                    warpathMissiles.Add(new WarpathMissile{source=target.entityId,owner=target.ownerSlot,target=sourceId,
                        damage=damage,origin=target.position,due=world.Clock+.03});
            }
            finally{warpathDamageRunning=false;}
        }
    }
}

using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalSessionReplyCode ValidateItemFingerTarget(Player player,OriginalSessionCommand command,OriginalItemSummonScriptRules rule,bool enforceRange=true)
        {
            if(command.targetKind!=OriginalWorldTargetKind.Unit)return OriginalSessionReplyCode.InvalidCommand;
            var source=world.UnitState(player.slot);var target=world.UnitState(command.targetId);
            if(source==null||target==null||target.health<=.405||target.invulnerable||!AreEnemies(player.slot,target.ownerSlot)||
                !CanSeeForCombat(player.slot,target)||CasterMagicImmune(target)||CasterHasType(target,"mechanical")||CasterHasType(target,"structure")||
                !WeaponTargetTypeAllowed(combatCatalog.Ability(rule.abilityId).Text("targs1"),combatCatalog.Unit(target.rawcode).Text("targType")))
                return OriginalSessionReplyCode.InvalidCommand;
            return !enforceRange||SquaredDistance(source.position,target.position)<=rule.range*rule.range?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.NotReady;
        }
        OriginalSessionReplyCode UseItemFinger(Player player,OriginalSessionCommand command,OriginalItemSummonScriptRules rule)
        {
            var valid=ValidateItemFingerTarget(player,command,rule);if(valid!=OriginalSessionReplyCode.Accepted)return valid;
            var target=world.UnitState(command.targetId);
            try
            {
                if(ItemAssimilationEligible(target))
                {
                    if(!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.ChaosUniversal))return OriginalSessionReplyCode.RuleUnavailable;
                    PrepareAssimilation(player.slot,target);
                }
            }
            catch(InvalidOperationException){return OriginalSessionReplyCode.RuleUnavailable;}
            if(!world.TrySpendMana(player.slot,rule.manaCost))return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot,rule.cooldownGroup)]=ItemClock+rule.cooldown;
            player.lastItemAction=OriginalItemActionCode.Success;NotifyNativeSpellEffect(player.slot,rule.abilityId);
            // H7 runs on the source EFFECT before native ANfd's two zeros.
            // A native HERO target is legal, but H7 explicitly skips cloning it.
            target=world.UnitState(command.targetId);
            if(target==null||target.health<=.405)return OriginalSessionReplyCode.Accepted;
            if(ItemAssimilationEligible(target))
            {
                try{BeginItemAssimilation(player.slot,target.entityId);}
                catch(InvalidOperationException){return OriginalSessionReplyCode.Accepted;}
            }
            for(int i=0;i<2;i++)
            {
                target=world.UnitState(command.targetId);if(target==null||target.health<=.405)break;
                ApplyResolvedUnitHit(player.slot,player.slot,target,0);
            }
            return OriginalSessionReplyCode.Accepted;
        }
    }
}

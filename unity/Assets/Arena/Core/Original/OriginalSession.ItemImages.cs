using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalItemImageRules itemImageRules;
        OriginalItemImageRules ItemImageRule(string id)
        {
            if(id!=OriginalItemImageRules.ItemId)return null;
            return itemImageRules??(itemImageRules=new OriginalItemImageRules(itemCatalog,combatCatalog));
        }
        OriginalSessionReplyCode ValidateItemImageTarget(Player player,OriginalSessionCommand command,OriginalItemImageRules rule,bool enforceRange=true)
        {
            if(command.targetKind!=OriginalWorldTargetKind.Unit||command.targetId<=0)return OriginalSessionReplyCode.InvalidCommand;
            var actor=world.UnitState(player.slot);var target=world.UnitState(command.targetId);
            if(actor==null||target==null||target.health<=.405||!CanSeeForCombat(player.slot,target)||target.invulnerable||
                CasterMagicImmune(target)||enforceRange&&SquaredDistance(actor.position,target.position)>rule.range*rule.range)
                return OriginalSessionReplyCode.NotReady;
            // b6 cancels AIil at SPELL_CAST specifically for native rank1 A04U.
            // Effective instance overlays remain authoritative after D3 removal.
            if(HasEffectiveUnitAbility(target,"A04U"))return OriginalSessionReplyCode.NotReady;
            if(target.kind==OriginalWorldUnitKind.Hero)
                return target.rawcode=="H008"||target.rawcode=="H024"||target.rawcode=="N0A0"
                    ?OriginalSessionReplyCode.Accepted:OriginalSessionReplyCode.RuleUnavailable;
            if(target.kind!=OriginalWorldUnitKind.Enemy||target.ownerSlot!=0||IsNativeHeroPredicate(target)||
                CasterHasType(target,"structure")||CasterHasType(target,"mechanical"))return OriginalSessionReplyCode.RuleUnavailable;
            // Mega bosses have additional native/script capture requirements.
            if(target.rawcode=="n017"||target.rawcode=="n00K"||target.rawcode=="n00Z"||target.rawcode=="u00G"||target.rawcode=="n0AW")
                return OriginalSessionReplyCode.RuleUnavailable;
            return OriginalSessionReplyCode.Accepted;
        }
        void CaptureItemImage(OriginalWorldUnitView donor,out OriginalWorldUnitProfile profile,out ActorCombatStats stats)
        {
            profile=donor.profile.Copy();
            if(donor.kind==OriginalWorldUnitKind.Hero)
            {
                var hero=HeroCombatStats(donor.ownerSlot);stats=new ActorCombatStats(hero,illusion:true);
                profile.moveSpeed=hero.baseMoveSpeed;return;
            }
            var d=combatCatalog.Unit(donor.rawcode);double armor;
            if(!d.TryNumber("def",out armor,out _))
            {
                var sparse=observed?.sparse?.Unit(donor.rawcode);
                armor=sparse!=null&&sparse.armorKnown?sparse.RequireArmor():observed.Unit(donor.rawcode).RequireArmor();
            }
            // Copy only the source f3 additions, once. Do not freeze temporary
            // corruption, aura armor or +9000 phase protection into base stats.
            // This ordinary-unit inheritance is a host policy pending native
            // AIil controls, not a promotion of the hero IMAGE2 measurement.
            if(bossScaling.TryGetValue(donor.entityId,out var scaling))armor+=scaling.armor;
            stats=new ActorCombatStats(armor,BossAttackBonus(donor.entityId));
            profile.moveSpeed=d.Number("spd");
        }
        OriginalSessionReplyCode UseItemImage(Player player,OriginalSessionCommand command,OriginalItemImageRules rule)
        {
            var valid=ValidateItemImageTarget(player,command,rule);if(valid!=OriginalSessionReplyCode.Accepted)return valid;
            if(nextIllusionId==int.MaxValue)return OriginalSessionReplyCode.RuleUnavailable;
            var donor=world.UnitState(command.targetId);OriginalWorldUnitProfile profile;ActorCombatStats stats;OriginalPoint point;
            try
            {
                CaptureItemImage(donor,out profile,out stats);
                if(!TryEnemyImagePosition(donor,out point))return OriginalSessionReplyCode.NotReady;
            }
            catch(InvalidOperationException){return OriginalSessionReplyCode.RuleUnavailable;}
            if(!world.TrySpendMana(player.slot,rule.manaCost))return OriginalSessionReplyCode.NotReady;
            itemCooldowns[ItemCooldownKey(player.slot,OriginalItemImageRules.AbilityId)]=ItemClock+rule.cooldown;
            player.lastItemAction=OriginalItemActionCode.Success;
            NotifyNativeSpellEffect(player.slot,OriginalItemImageRules.AbilityId);
            // Transfer IMAGE2's AIil zero-event ordering to the item caster.
            // If an actual event handler kills/removes the donor, the accepted
            // cast fizzles; it is not an invalid command with partially spent cost.
            donor=world.UnitState(command.targetId);
            if(donor==null||donor.health<=.405)return OriginalSessionReplyCode.Accepted;
            ApplyResolvedUnitHit(player.slot,player.slot,donor,0);
            donor=world.UnitState(command.targetId);
            if(donor==null||donor.health<=.405)return OriginalSessionReplyCode.Accepted;
            try{CaptureItemImage(donor,out profile,out stats);}
            catch(InvalidOperationException){return OriginalSessionReplyCode.Accepted;}
            if(!TryEnemyImagePosition(donor,out point))return OriginalSessionReplyCode.Accepted;
            var row=new OriginalIllusionSpawn{entityId=nextIllusionId,position=point,profile=profile,health=donor.health,mana=donor.mana};
            if(!world.TryPublishImages(donor.entityId,player.slot,OriginalImageFactory.ItemWand,new[]{row}))return OriginalSessionReplyCode.Accepted;
            illusionCombatStats.Add(row.entityId,stats);
            mirrorImages.Add(row.entityId,new MirrorImage{actor=row.entityId,source=donor.entityId,outgoing=rule.outgoing,incoming=rule.incoming,
                expires=world.Clock+rule.lifetime,lastAdvanced=world.Clock});
            nextIllusionId++;return OriginalSessionReplyCode.Accepted;
        }
    }
}

using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalRoundStatistics roundStatistics;
        long roundBonusEvent;
        double roundBonusAt=double.PositiveInfinity;
        bool roundBonusPending;

        void InitializeRoundBonuses()=>roundStatistics=new OriginalRoundStatistics(players.Count);

        void OnRoundBonusMatchEvent(OriginalMatchEvent item)
        {
            if(world==null||roundStatistics==null||item.kind!=OriginalMatchEventKind.ShopAccess)return;
            // Q3/vU resets all three pools after dispatching the new battle.
            if(!item.enabled){roundStatistics.Reset();roundBonusPending=false;return;}
            // D4 returns before OU while pairs/gladiators are pending. OU
            // runs after Vr advances, once the whole duel series has ended.
            int completed=item.round-1;
            if(match.Phase!=OriginalMatchPhase.Preparation||completed<1||completed>=30||item.sequence<=roundBonusEvent)return;
            roundBonusEvent=item.sequence;
            roundBonusPending=options.acolyteBonus;
            roundBonusAt=ItemClock+item.time-match.Clock+1;
        }

        void RecordRoundDamage(int attackerOwner,OriginalWorldUnitView target,double eventDamage)
        {
            if(roundStatistics==null||target==null)return;
            if(target.kind==OriginalWorldUnitKind.Hero&&target.entityId==OriginalWorld.HeroEntityId(target.ownerSlot))
                roundStatistics.RecordTaken(PlayerAt(target.ownerSlot).matchSlot,eventDamage,target.health);
            // DU/gU registers every Player11 unit, including source helpers.
            if(target.ownerSlot==0&&attackerOwner>0)
                roundStatistics.RecordDealt(PlayerAt(attackerOwner).matchSlot,eventDamage,target.health);
        }

        void RecordRoundSpellEffect(int actorId,string abilityId)
        {
            if(roundStatistics==null)return;
            var actor=world?.UnitState(actorId);
            if(actor==null||actor.kind!=OriginalWorldUnitKind.Hero||actor.entityId!=OriginalWorld.HeroEntityId(actor.ownerSlot))return;
            bool active=match.Phase==OriginalMatchPhase.Combat||match.Phase==OriginalMatchPhase.FinalIntermission;
            bool ordinary=active&&match.Round%5!=0;
            // Literal Yu11111: Mr or (cr and hK(id)). During ordinary rounds
            // every SPELL_EFFECT counts; mega encounters apply hK's filter.
            if(ordinary||active&&OriginalCasterFieldRules.SpellTriggersCurse(abilityId,actor.rawcode))
                roundStatistics.RecordCast(PlayerAt(actor.ownerSlot).matchSlot);
        }

        void AdvanceRoundBonuses()
        {
            if(!roundBonusPending||world==null||ItemClock+1e-9<roundBonusAt)return;
            roundBonusPending=false;
            var leaders=roundStatistics.Leaders();
            for(int axis=0;axis<3;axis++)
            {
                var player=players.Find(p=>p.matchSlot==leaders[axis]);
                if(player==null||player.stats==null||player.inventory==null)continue;
                var actor=world.UnitState(OriginalWorld.HeroEntityId(player.slot));if(actor==null)continue;
                var attributes=itemPermanentAttributes.TryGetValue(player.slot,out var prior)?prior.Copy():new PermanentItemAttributes();
                checked{if(axis==0)attributes.strength+=2;else if(axis==1)attributes.agility+=2;else attributes.intelligence+=2;}
                var stats=ComposeEquipment(player.stats,player.inventory,attributes);var profile=actor.profile.Copy();
                profile.maxHealth=stats.maxHealth.Require();profile.maxMana=stats.maxMana.Require();
                // Source SetHeroStr/Agi/Int(...,true) grants permanent base
                // attributes. The existing scoped tome ratio policy supplies
                // vitality rounding; this is a stated engine approximation.
                double hp=UpdatedVitality(actor.health,actor.profile.maxHealth,profile.maxHealth,"ratio-nearest-approximation",true);
                double mp=UpdatedVitality(actor.mana,actor.profile.maxMana,profile.maxMana,"ratio-nearest-approximation",false);
                if(!world.UpdateProfile(actor.entityId,profile,hp,mp))throw new InvalidOperationException("acolyte-attribute-profile-rejected");
                itemPermanentAttributes[player.slot]=attributes;
            }
        }
    }
}

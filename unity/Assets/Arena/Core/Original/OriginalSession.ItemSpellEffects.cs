using System;
namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        struct ItemSpellEffect { internal double damage, vamp; internal bool improvedHealing; }
        ItemSpellEffect PrepareItemSpellEffect(int actorId,double damage)
        {
            var effect=new ItemSpellEffect{damage=damage};var actor=world.UnitState(actorId);
            if(actor==null || actor.kind!=OriginalWorldUnitKind.Hero || actor.entityId!=actor.ownerSlot)return effect;
            var player=PlayerAt(actor.ownerSlot);if(player.inventory==null)return effect;
            double power=0;bool randomPower=false;
            // V7:21064..21103 aggregates each of the six actual hero slots.
            // I05I/I05Y are booleans; Nemesis contributes once per resident copy.
            foreach(var item in player.inventory.HeroSlots)
            {
                if(item==null)continue;
                switch(item.itemId)
                {
                    case "I07R":power+=.05;break;case "I057":power+=.08;break;case "I09X":power+=.12;break;
                    case "I085":case "I088":power+=.2;break;
                    case "I0AE":power+=NemesisItemSpellPower(actorId);effect.vamp+=NemesisItemMagicVamp(actorId);break;
                    case "I05K":effect.vamp+=.1;break;case "I08D":effect.vamp+=.15;break;
                    case "I05I":randomPower=true;break;case "I05Y":effect.improvedHealing=true;break;
                }
            }
            // Source hL draws1..20 once per invocation, including each area
            // victim. Host deterministic RNG replaces Warcraft's private stream.
            if(randomPower)power+=RollWeapon(20)>3?.15:.3;
            // hL2976: B0B1 or A19O shares one +.2 branch. The existing AIrg state owns expiry and damage interruption.
            if(itemRegeneration.ContainsKey(actorId)||player.auxiliaryAbilities.ContainsKey("A19O"))power+=.2;
            power+=.05*AuxiliaryRank(player,"A19C");
            effect.damage=damage*(1+power);return effect;
        }
        void CompleteItemSpellEffect(int actorId,int owner,ItemSpellEffect effect)
        {
            var actor=world.UnitState(actorId);if(actor==null || actor.kind!=OriginalWorldUnitKind.Hero || actor.health<=.405)return;
            var player=PlayerAt(actor.ownerSlot);
            // hL performs magic vamp after UnitDamageTarget even if that native
            // call was rejected. It uses amplified raw Gk, not actual HP loss.
            if(player.auxiliaryAbilities.ContainsKey("A19O"))
            {
                ApplyNativeTriggeredHit(actorId,owner,actor,effect.damage*.1,OriginalTriggeredDamageMode.ChaosUniversal);
                effect.vamp*=.5;actor=world.UnitState(actorId);if(actor==null||actor.health<=.405)return;
            }
            if(effect.vamp<=0)return;
            // hL3009 B0AS: Z8 grants self-aura A17O to this recipient, and y8 removes it after6s.
            if(HasEffectiveUnitAbility(actor,"A17O"))effect.vamp*=2;
            if(player.auxiliaryAbilities.ContainsKey("A19M"))effect.vamp*=.7;
            double amount=effect.damage*effect.vamp*(effect.improvedHealing?1.2:1);
            // FL checks B0A4 before either healing, full-life mana or curse damage.
            if(HasWaveDarkBuff(actorId))amount*=.5;
            if(player.auxiliaryAbilities.ContainsKey("A19P"))
            {ApplyNativeTriggeredHit(actorId,owner,actor,amount,OriginalTriggeredDamageMode.ChaosUniversal);return;}
            // FL:2960: any missing life gets healing; only already-full life
            // redirects half the amount to mana, with no overflow conversion.
            double health=actor.health,mana=actor.mana;
            if(health<actor.profile.maxHealth)health=Math.Min(actor.profile.maxHealth,health+amount);
            else mana=Math.Min(actor.profile.maxMana,mana+amount*.5);
            if(!world.UpdateProfile(actorId,actor.profile,health,mana))throw new InvalidOperationException("item-magic-vamp-profile-rejected");
            // Non-selected hero H02E healing-XP remains its own driver.
        }
    }
}

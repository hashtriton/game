using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int,HashSet<string>> uniqueSoulGrants=new Dictionary<int,HashSet<string>>();
        readonly Dictionary<int,double> titanPulseAt=new Dictionary<int,double>();
        readonly Dictionary<int,int> soulBladeAttempts=new Dictionary<int,int>();
        sealed class SoulBladeWave
        {
            internal int actor,owner,steps;
            internal double angle,due,damage;
            internal OriginalPoint point;
            internal readonly HashSet<int> hit=new HashSet<int>();
        }
        readonly List<SoulBladeWave> soulBladeWaves=new List<SoulBladeWave>();
        bool HasUniqueSoul(int slot,string id)=>uniqueSoulGrants.TryGetValue(slot,out var grants)&&grants.Contains(id);

        void ValidateUniqueSoul(string id)
        {
            foreach(string ability in OriginalSoulUniqueRules.Abilities(id))
                if(combatCatalog.Ability(ability)==null)throw new InvalidOperationException("unique-soul-ability-missing:"+ability);
            void Field(string rawcode,string key,double expected)
            {
                if(combatCatalog.Ability(rawcode).Number(key)!=expected)
                    throw new InvalidOperationException("unique-soul-declaration-conflict:"+rawcode+":"+key);
            }
            if(id=="R00E")
            {
                var aura=combatCatalog.Ability("A1DQ");
                if(aura.Text("code")!="AHad"||aura.Text("BuffID1")!="B0CS"||aura.Text("targs1")!="air,enemies,ground,invulnerable,vulnerable")
                    throw new InvalidOperationException("unique-abyss-aura-conflict");
                Field("A1DQ","Area1",700);Field("A1DQ","DataA1",-.15);Field("A1DQ","DataB1",1);
            }
            if(id=="R00H")
            {
                if(combatCatalog.Ability("A1DS").Text("code")!="AEev")throw new InvalidOperationException("unique-evasion-family");
                Field("A1DS","DataA1",.15);
            }
            if(id=="R00G")
            {
                var demolish=combatCatalog.Ability("A1DU");
                if(demolish.Text("code")!="ANde"||demolish.Text("targs1")!="enemy,structure")throw new InvalidOperationException("unique-demolish-family");
                Field("A1DU","DataA1",100);Field("A1DU","DataB1",1);Field("A1DU","DataC1",1.15);Field("A1DU","DataD1",1.15);
            }
        }
        bool CanApplyUniqueSoulUpgrade(Player player,string id)
        {
            if(player==null||!OriginalSoulUpgradeRules.IsUnique(id)||world?.UnitState(player.slot)==null||player.progression==null||player.inventory==null)return false;
            try
            {
                ValidateUniqueSoul(id);
                if(id=="R00G")AddPrimarySoulAttributes(HeroCombatStats(player.slot)).maxHealth.Require();
                if(id=="R00J")ScriptedAbilityAttack(player);
                return true;
            }
            catch(InvalidOperationException){return false;}
        }
        OriginalHeroStatsSnapshot AddPrimarySoulAttributes(OriginalHeroStatsSnapshot baseline)
        {
            var result=baseline.Copy();
            switch(result.primaryAttribute)
            {
                case "STR":result.strength=Plus(result.strength,50);result.maxHealth=Plus(result.maxHealth,50*ProgressionConstant("StrHitPointBonus"));break;
                case "AGI":result.agility=Plus(result.agility,50);result.armor=Plus(result.armor,50*ProgressionConstant("AgiDefenseBonus"));
                    result.agilityAttackSpeedBonus=Plus(result.agilityAttackSpeedBonus,50*ProgressionConstant("AgiAttackSpeedBonus"));break;
                case "INT":result.intelligence=Plus(result.intelligence,50);result.maxMana=Plus(result.maxMana,50*ProgressionConstant("IntManaBonus"));break;
                default:throw new InvalidOperationException("unique-primary-attribute-unavailable");
            }
            result.primary=Plus(result.primary,50);double damage=50*ProgressionConstant("StrAttackBonus");
            result.primaryDamageBonus=Plus(result.primaryDamageBonus,damage);result.attackMinimum=Plus(result.attackMinimum,damage);result.attackMaximum=Plus(result.attackMaximum,damage);
            return result;
        }
        OriginalHeroStatsSnapshot ComposeUniqueSoulStats(int slot,OriginalHeroStatsSnapshot baseline)=>HasUniqueSoul(slot,"R00G")?AddPrimarySoulAttributes(baseline):baseline;

        void ApplyUniqueSoulUpgrade(Player player,string id)
        {
            if(HasUniqueSoul(player.slot,id))return;
            if(!CanApplyUniqueSoulUpgrade(player,id))throw new InvalidOperationException("unique-soul-preflight-rejected:"+id);
            var actor=world.UnitState(player.slot);
            if(id=="R00G")
            {
                var stats=AddPrimarySoulAttributes(HeroCombatStats(player.slot));var profile=actor.profile.Copy();
                profile.maxHealth=stats.maxHealth.Require();profile.maxMana=stats.maxMana.Require();
                // hP explicitly preserves current HP for AGI/INT. STR fills
                // life and INT fills mana. A corpse is not revived by a stat grant.
                double health=actor.health>0&&stats.primaryAttribute=="STR"?profile.maxHealth:actor.health;
                double mana=stats.primaryAttribute=="INT"?profile.maxMana:actor.mana;
                if(!world.UpdateProfile(actor.entityId,profile,health,mana))throw new InvalidOperationException("unique-soul-profile-rejected");
            }
            // All definitions and the actor were validated before any mutation.
            ApplyUnitAbilityOverlay(actor.entityId,OriginalSoulUniqueRules.Abilities(id),null);
            if(!uniqueSoulGrants.TryGetValue(player.slot,out var grants))uniqueSoulGrants[player.slot]=grants=new HashSet<string>(StringComparer.Ordinal);
            grants.Add(id);
            if(id=="R00F")titanPulseAt[player.slot]=world.Clock+2;
            if(id=="R00H")world.SetPathingEnabled(actor.entityId,false);
        }

        double UniqueSoulArmorFraction(int targetId)
        {
            var target=world.UnitState(targetId);if(target==null)return 0;
            foreach(var player in players)
                if(HasUniqueSoul(player.slot,"R00E")&&ItemAuraRecipient(world.UnitState(player.slot),target,combatCatalog.Ability("A1DQ"),1))return -.15;
            // Authored AHAd percentage and targets; continuous/no-linger,
            // strongest identical aura are explicit host policies, not native probes.
            return 0;
        }
        void RestoreUniqueSoulPathing(int actorId)
        {
            // A3 (JASS17707) reapplies the persistent ability after party
            // restoration, including after a force effect enabled pathing.
            if(HasEffectiveUnitAbility(world.UnitState(actorId),"A1DR"))world.SetPathingEnabled(actorId,false);
        }
        void ApplyUniqueSoulPhaseShift(OriginalWorldUnitView target,double eventDamage,int roll)
        {
            if(target==null||target.kind!=OriginalWorldUnitKind.Hero||!HasUniqueSoul(target.ownerSlot,"R00I"))return;
            double heal=OriginalSoulUniqueRules.PhaseShiftHealing(eventDamage,roll);if(heal==0)return;
            target=world.UnitState(target.entityId);if(target==null||target.health<=0)return;
            world.UpdateProfile(target.entityId,target.profile,Math.Min(target.profile.maxHealth,target.health+heal),target.mana);
        }
        void ObserveUniqueSoulDamage(int attackerId,OriginalWorldUnitView target,double eventDamage,bool primaryWeapon)
        {
            if(target==null)return;
            if(eventDamage>10&&target.kind==OriginalWorldUnitKind.Hero&&HasUniqueSoul(target.ownerSlot,"R00I"))
                ApplyUniqueSoulPhaseShift(target,eventDamage,RollWeapon(100));
            var actor=world.UnitState(attackerId);
            // DW's gW is consumption of native B061/B062 attack markers.
            // The host primary-weapon seam supplies that distinction; arbitrary
            // spell/secondary wave callbacks must not invent another marker.
            if(!primaryWeapon||eventDamage<=0||actor==null||actor.kind!=OriginalWorldUnitKind.Hero||actor.entityId!=actor.ownerSlot||
                actor.entityId==target.entityId||!AreEnemies(actor.ownerSlot,target.ownerSlot)||!HasUniqueSoul(actor.ownerSlot,"R00J"))return;
            soulBladeAttempts.TryGetValue(actor.entityId,out int previous);int attempt=previous+1;
            // Native GetRandomReal stream is replaced with deterministic host RNG.
            // This accumulator covers xQ only. Native qM's handle999 counter is
            // shared with other source callers; their joint PRD is not replayed.
            bool proc=OriginalSoulUniqueRules.BladeProc(attempt,(RollWeapon(1000001)-1)/1000000d);
            soulBladeAttempts[actor.entityId]=proc?0:attempt;if(proc)BeginUniqueSoulBladeWaves(actor,target);
        }
        void BeginUniqueSoulBladeWaves(OriginalWorldUnitView actor,OriginalWorldUnitView target)
        {
            double heading=Math.Atan2(target.position.y-actor.position.y,target.position.x-actor.position.x)*57.2958;
            double damage=ScriptedAbilityAttack(PlayerAt(actor.ownerSlot))*.33;
            foreach(double offset in new[]{0d,20d,-20d})soulBladeWaves.Add(new SoulBladeWave{actor=actor.entityId,owner=actor.ownerSlot,
                angle=(heading+offset)*.0174532,point=actor.position,damage=damage,due=world.Clock+.03});
        }
        void AdvanceUniqueSoulEffects()
        {
            foreach(int slot in new List<int>(titanPulseAt.Keys))
                while(titanPulseAt[slot]<=world.Clock+1e-9)
                {
                    titanPulseAt[slot]+=2;var actor=world.UnitState(slot);
                    if(actor==null||actor.health<.405||actor.hidden)continue;
                    double damage=HeroCombatStats(slot).strength.Require();
                    foreach(var target in world.Snapshot().units)
                        if(target.health>.405&&AreEnemies(actor.ownerSlot,target.ownerSlot)&&!CasterHasType(target,"structure")&&SquaredDistance(actor.position,target.position)<=300*300)
                            ApplyTriggeredHit(actor.entityId,actor.ownerSlot,target,damage,OriginalTriggeredDamageMode.SpellNormal);
                }
            foreach(var wave in new List<SoulBladeWave>(soulBladeWaves))
                while(wave.due<=world.Clock+1e-9)
                {
                    wave.due+=.03;wave.point=new OriginalPoint(wave.point.x+27*Math.Cos(wave.angle),wave.point.y+27*Math.Sin(wave.angle));
                    double radius=OriginalSoulUniqueRules.BladeRadius(wave.steps);
                    foreach(var target in world.Snapshot().units)
                        if(target.health>.405&&AreEnemies(wave.owner,target.ownerSlot)&&!CasterHasType(target,"structure")&&!CasterMagicImmune(target)&&
                            !wave.hit.Contains(target.entityId)&&SquaredDistance(wave.point,target.position)<=radius*radius)
                        {wave.hit.Add(target.entityId);ApplyTriggeredHit(wave.actor,wave.owner,target,wave.damage,OriginalTriggeredDamageMode.SpellMagic);}
                    // Zq subtracts27 then tests GM<=0 after its final sweep.
                    // Radius really increases by150 every tick in the source.
                    if(++wave.steps>=26){soulBladeWaves.Remove(wave);ObserveScriptedHelperDeath();break;}
                }
        }
        void AppendUniqueSoulVisuals(List<OriginalVisualEffectView> output)
        {
            foreach(var wave in soulBladeWaves)output.Add(new OriginalVisualEffectView{kind=OriginalVisualEffectKind.Ghost,abilityId="A1DV",sourceEntityId=wave.actor,
                position=wave.point,end=new OriginalPoint(wave.point.x+60*Math.Cos(wave.angle),wave.point.y+60*Math.Sin(wave.angle)),radius=150,progress=wave.steps/26d});
        }
    }
}

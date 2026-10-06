using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ImmolationState
        {
            internal string ability;
            internal double remaining, damage, interval, radius, firstPulseAt;
        }
        readonly Dictionary<int, ImmolationState> permanentImmolations = new Dictionary<int, ImmolationState>();
        sealed class NativeCorruption { internal double until, armor; }
        readonly Dictionary<int, NativeCorruption> nativeCorruptionUntil = new Dictionary<int, NativeCorruption>();

        void ApplyNativeCorruption(OriginalWorldUnitView attacker, OriginalWorldUnitView target)
        {
            if(attacker==null || target==null) return;
            if(!WeaponTargetTypeAllowed("ground,air,ward",combatCatalog.Unit(target.rawcode).Text("targType")))return;
            var orb=HighestItemOrb(attacker.entityId);
            if(orb?.abilityId=="A14L")
            {
                var definition=combatCatalog.Ability(orb.abilityId);
                if(orb.rank!=1 || definition.Text("code")!="AIcb" || definition.Text("BuffID1")!="B09V" ||
                    definition.Number("DataB1")!=4 || definition.Number("Dur1")!=6 || definition.Number("HeroDur1")!=6 ||
                    definition.Text("targs1")!="ground,air,ward" || definition.overrides.Length!=0)
                    throw new InvalidOperationException("item-corruption-declaration-conflict");
                // AIcb before-hit stage transfers from measured A0QZ. A14L's
                // amount4/duration6 are declarations, not new runtime rows.
                // Highest-slot orb policy is shared with item lifesteal. Drop
                // does not dispel an already applied debuff. Cross-AIcb latest
                // overwrite and wall-clock duration remain host policies.
                nativeCorruptionUntil[target.entityId]=new NativeCorruption{until=world.Clock+6,armor=4};
                return;
            }
            foreach(var ability in NativeUnitAbilities(attacker))
            {
                if((ability.id!="A0QZ" && ability.id!="A0VP") || ability.Text("code")!="AIcb") continue;
                double armor=ability.Number("DataB1");
                if(ability.Text("BuffID1")!="BIcb" || armor!=(ability.id=="A0QZ"?10:5) ||
                    ability.Number("Dur1")!=.01 || ability.Number("HeroDur1")!=.01 ||
                    ability.Text("targs1")!="ground,air,ward" || ability.overrides.Length!=0)
                    throw new InvalidOperationException("native-corruption-declaration-conflict");
                // SUMW1 four intact n07C hits: BIcb is present in the first
                // callback and damage agrees with target armor2-10. Declared
                // .01 duration is compatible with next-.02 buff disappearance.
                // A0VP on n00N/n00O declares the same family and duration,
                // with armor5. That transfer, refresh/nonstacking and host
                // clock expiry are reconstruction, not separate native rows.
                nativeCorruptionUntil[target.entityId]=new NativeCorruption{until=world.Clock+.01,armor=armor};
                return;
            }
        }
        double NativeCorruptionArmorDelta(int id) => nativeCorruptionUntil.TryGetValue(id,out var state) && world.Clock+1e-9<state.until ? -state.armor : 0;
        void RemoveNativeCorruption(int id) => nativeCorruptionUntil.Remove(id);

        bool RollNativeEvasion(OriginalWorldUnitView target)
        {
            // INDEF1 paused defenders never evade; INDEF2 unpaused AEev/A15G
            // omit entire weapon damage callbacks in some complete intervals.
            // Chance comes from the explicit map field, not sample frequency.
            // Impact-time uniform host RNG is reconstruction; native PRD and
            // simultaneous different-source stacking were not measured.
            if(target.paused) return false;
            double chance=0;
            foreach(var ability in NativeUnitAbilities(target))
            {
                if(ability.Text("code")=="ANdb")
                {
                    if(ability.TryNumber("DataD1",out double brawler,out _) && brawler>=0 && brawler<=1)
                        chance=Math.Max(chance,brawler); // strongest evasion is an explicit mixed-family policy.
                    continue;
                }
                if(ability.Text("code")!="AEev" || (ability.id!="AEev" && ability.id!="A15G" && ability.id!="A1DS")) continue;
                double value=ability.Number("DataA1");
                if(value!=(ability.id=="AEev"?.1:ability.id=="A1DS"?.15:.5) || ability.overrides.Length!=0)
                    throw new InvalidOperationException("native-evasion-declaration-conflict");
                chance=Math.Max(chance,value);
            }
            // Sparse A11I/D/E/K/J are deliberately not assigned zero chance
            // from finite64/64 positive observations or missing DataA1.
            return chance>0 && RollWeapon(1000000)-1<chance*1000000;
        }

        double NativeEnduranceAttackSpeed(OriginalWorldUnitView actor)
        {
            // SCae is O006's authored self-only AOae. Its DataB is additional
            // attack speed, separate from the measured level50 AGI250 bonus.
            // The native O006 move390 profile already contains the move part;
            // do not apply DataA .3 to that measured profile a second time.
            // Images require their own factory-specific aura inheritance proof.
            if(actor.kind==OriginalWorldUnitKind.Illusion) return 0;
            foreach(var ability in NativeUnitAbilities(actor))
            {
                if(ability.id!="SCae" || ability.Text("code")!="AOae") continue;
                if(ability.Text("BuffID1")!="BOae" || ability.Text("targs1")!="self" ||
                    ability.Number("Area1")!=100 || ability.Number("DataA1")!=.3 ||
                    ability.Number("DataB1")!=.75 || ability.overrides.Length!=0)
                    throw new InvalidOperationException("endurance-self-aura-declaration-conflict");
                return ability.Number("DataB1");
            }
            return 0;
        }

        OriginalCombatDefinition PermanentImmolation(OriginalWorldUnitView actor)
        {
            // Image passive inheritance needs an explicit native closure.
            if(actor.kind == OriginalWorldUnitKind.Illusion) return null;
            foreach(var ability in NativeUnitAbilities(actor))
                if(ability.Text("code") == "ANpi" && (ability.id == "ANpi" || ability.id == "A0BY")) return ability;
            return null;
        }
        static ImmolationState CreateImmolation(OriginalCombatDefinition ability)
        {
            // 3.9c Units/AbilityData.slk ANpi:3407 and custom A0BY use the
            // native permanent-immolation family. ANpi's effective1.26 base
            // also declares10 damage /1 second /220WC. No flat armor effect.
            double damage=ability.Number("DataA1"), interval=ability.Number("Dur1"), radius=ability.Number("Area1");
            if(ability.overrides.Length != 0 || ability.Text("BuffID1") != "BNpi" ||
                ability.Text("targs1") != "ground,enemy,neutral,organic" || interval != 1 ||
                ability.Number("HeroDur1") != 1 || radius != 220 || damage != (ability.id == "ANpi" ? 10 : 45))
                throw new InvalidOperationException("immolation-declaration-conflict:"+ability.id);
            return new ImmolationState{ability=ability.id,remaining=interval,damage=damage,interval=interval,radius=radius};
        }
        void SeedNativeImmolationLanding(int id)
        {
            var actor=world.UnitState(id);
            if(actor==null || actor.rawcode!="n025" || actor.health<=.405 || actor.hidden)
                throw new InvalidOperationException("invalid-infernal-immolation-landing");
            var ability=PermanentImmolation(actor);
            if(ability==null) throw new InvalidOperationException("infernal-immolation-missing");
            var state=CreateImmolation(ability);
            // INFSTATE2 actual n025 impact at birth+1, first native ANpi at
            // birth+1.01, later pulses +1. Absolute first deadline prevents an
            // earlier portion of this host frame being counted after landing.
            state.firstPulseAt=world.Clock+.01;
            permanentImmolations[id]=state;
        }
        void AdvancePermanentImmolations(double seconds)
        {
            foreach(int id in new List<int>(nativeCorruptionUntil.Keys))
            {
                var actor=world.UnitState(id);
                if(actor==null || actor.health<=.405 || world.Clock+1e-9>=nativeCorruptionUntil[id].until) nativeCorruptionUntil.Remove(id);
            }
            // Host reconstruction: first pulse follows one interval, inactive
            // hidden/paused emitters freeze phase, and distinct emitters pulse
            // independently. Only the n025 landing seed has measured first
            // phase; generic phase/paused/overlap remain host reconstruction.
            // Declared magnitude, period, radius and target flags are preserved.
            var units=world.Snapshot().units;
            var active=new HashSet<int>();
            foreach(var captured in units)
            {
                var actor=world.UnitState(captured.entityId);
                if(actor == null || actor.health <= 0) continue;
                var ability=PermanentImmolation(actor); if(ability == null) continue;
                active.Add(actor.entityId);
                if(!permanentImmolations.TryGetValue(actor.entityId,out var state) || state.ability != ability.id)
                    permanentImmolations[actor.entityId]=state=CreateImmolation(ability);
                if(actor.paused || actor.hidden) continue;
                if(state.firstPulseAt>0)
                {
                    if(world.Clock+1e-9<state.firstPulseAt) continue;
                    state.remaining=state.firstPulseAt-world.Clock;
                    state.firstPulseAt=0;
                }
                else state.remaining-=seconds;
                while(state.remaining <= 1e-9)
                {
                    state.remaining+=state.interval;
                    foreach(var candidate in units)
                    {
                        actor=world.UnitState(captured.entityId);
                        if(actor == null || actor.health <= 0 || actor.hidden || actor.paused) break;
                        var target=world.UnitState(candidate.entityId);
                        if(target == null || target.health <= 0 || target.hidden || target.invulnerable ||
                            !AreEnemies(actor.ownerSlot,target.ownerSlot) || CasterHasType(target,"mechanical") ||
                            CasterHasType(target,"structure") || combatCatalog.Unit(target.rawcode).Text("movetp") == "fly" ||
                            CasterMagicImmune(target) || SquaredDistance(actor.position,target.position) > state.radius*state.radius) continue;
                        ApplyNativeTriggeredHit(actor.entityId,actor.ownerSlot,target,state.damage,OriginalTriggeredDamageMode.SpellMagic);
                    }
                }
            }
            foreach(int id in new List<int>(permanentImmolations.Keys)) if(!active.Contains(id)) permanentImmolations.Remove(id);
        }

        bool HasMeasuredNativeCarapace(OriginalWorldUnitView target)
        {
            foreach (var ability in NativeUnitAbilities(target))
            {
                if (ability.Text("code") != "AUts" || ability.id != "A15F") continue;
                // A15F is shared by thirteen authored enemy rawcodes. SPARSE20
                // CHAOS/NORMAL and HELPER29 MELEE/PIERCE NORMAL40 all produce8;
                // removal restores40. This is a damage factor, not armor.
                // Missing reflection/armor fields are not inherited as zero.
                if (ability.Text("BuffID1") != "BUts" || ability.Number("DataB1") != .2 || ability.overrides.Length != 0)
                    throw new InvalidOperationException("carapace-native-declaration-conflict");
                return true;
            }
            return false;
        }
        double IncomingInnateWeaponDamage(OriginalWorldUnitView target, double damage)
        {
            double factor=HasMeasuredNativeCarapace(target)?.2:1;
            foreach(var item in ItemNativeCombatAbilities(target.entityId))
            {
                var ability=combatCatalog.Ability(item.abilityId);
                if(ability.Text("code")!="AUts" || item.rank<1)continue;
                if(ability.TryNumber("DataB"+item.rank,out double received,out _) && received>=0 && received<=1)
                    factor=Math.Min(factor,received);
            }
            // Item Uts2 is a received-damage multiplier. Strongest factor
            // across independent carapaces is host reconstruction; intrinsic
            // Uts3 armor is already composed by the equipment profile.
            return damage*factor;
        }

        double IncomingItemWeaponDamage(OriginalWorldUnitView target,double damage,bool melee)
        {
            if(damage<=0)return damage;
            double result=damage;
            foreach(var item in ItemNativeCombatAbilities(target.entityId))
            {
                var ability=combatCatalog.Ability(item.abilityId);string rank=item.rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if(ability.Text("code")!="Assk" || item.rank<1)continue;
                // Effective1.26 AbilityMetaData Ssk4/Ssk5 and editor strings:
                // DataD enables ranged weapons, DataE enables melee weapons.
                // These are weapon kinds, not flying/ground target flags.
                if(!ability.TryNumber("Data"+(melee?"E":"D")+rank,out double enabled,out _) || enabled!=1 ||
                    !ability.TryNumber("DataA"+rank,out double chance,out _) ||
                    !ability.TryNumber("DataB"+rank,out double minimum,out _) ||
                    !ability.TryNumber("DataC"+rank,out double blocked,out _))continue;
                if(chance<0 || chance>100 || minimum<0 || blocked<0)throw new InvalidOperationException("item-skin-declaration-conflict:"+ability.id);
                if(chance==100 || chance>0 && RollWeapon(1000000)-1<chance*10000)
                    result=Math.Min(result,Math.Max(minimum,damage-blocked));
            }
            // ITEMROAR1 settled I06R weapon hits resolve (white-45)/3.448
            // on40.8armor, excluding the previous post-armor subtraction.
            // Callers therefore pass raw damage BEFORE numeric armor. Transfer
            // to other attack matrices/minimum damage, independent impact rolls,
            // strongest success, splash and duplicate stacking remain policies.
            return result;
        }

        void ReflectNativeCarapace(int attackerId,OriginalWorldUnitView defender,double rawDamage)
        {
            if(rawDamage<=0)return;
            double reflected=HasMeasuredNativeCarapace(defender)?1:0;
            foreach(var item in ItemNativeCombatAbilities(defender.entityId))
            {
                var ability=combatCatalog.Ability(item.abilityId);
                if(ability.Text("code")!="AUts" || item.rank<1)continue;
                if(ability.TryNumber("DataA"+item.rank,out double fraction,out _) && fraction>0)
                    reflected=Math.Max(reflected,Math.Max(1,rawDamage*fraction));
            }
            if(reflected<=0)return;
            var attacker=world.UnitState(attackerId);
            if(attacker==null || attacker.health<=0 || attacker.hidden || attacker.invulnerable)return;
            // INDEF2 carapace AGI6 and1000 paired direct controls: reverse
            // event1 precedes forward damage, and source HP631 actually becomes
            // 630. This is not the Banish event1/HP0 sentinel. Melee weapon
            // callbacks also precede the hit with1. Reflected damage does not
            // recursively enter either reflection resolver. Other magnitudes/
            // defender compositions use this explicitly bounded host transfer;
            // ranged weapon reflection has no measured closure here.
            // Item Uts1 fractions transfer the same raw-hit/reverse-damage
            // boundary. Strongest reflection and minimum1 are explicit host
            // policies, not additional item-specific native measurements.
            ApplyResolvedUnitHit(defender.entityId,defender.ownerSlot,attacker,reflected);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class NativeWeaponProc
        {
            internal string ability, sourceKey, buff;
            internal bool targetsAir;
            internal double multiplier=1, magicDamage, stunDuration, heroStunDuration, manaBurn, heroManaBurn, burnDamageFactor, heroBurnDamageFactor;
        }
        sealed class NativeBashState
        {
            internal int target, source, owner;
            internal string token, buff = "BPSE";
            internal double remaining;
        }
        readonly List<NativeBashState> nativeBashes=new List<NativeBashState>();

        NativeWeaponProc[] CaptureNativeWeaponProcs(OriginalWorldUnitView actor)
        {
            if(actor.kind==OriginalWorldUnitKind.Illusion)return Array.Empty<NativeWeaponProc>();
            var result=new List<NativeWeaponProc>();
            foreach(var ability in NativeUnitAbilities(actor))
            { CaptureNativeWeaponProc(result,ability,1,ability.id,false); CaptureNativeBrawler(result,ability); }
            foreach(var item in ItemNativeCombatAbilities(actor.entityId))
                CaptureNativeWeaponProc(result,combatCatalog.Ability(item.abilityId),item.rank,
                    item.instanceId+":"+item.occurrence+":"+item.abilityId,true);
            return result.ToArray();
        }

        void CaptureNativeWeaponProc(List<NativeWeaponProc> result,OriginalCombatDefinition ability,int rank,string sourceKey,bool item)
        {
            if(ability==null || rank<1)return;
            string suffix=rank.ToString(System.Globalization.CultureInfo.InvariantCulture),code=ability.Text("code"),targets=ability.Text("targs"+suffix);
            if(code!="AOcr" && code!="AHbh" && code!="Afbk" || targets==null || targets=="none" || targets=="_")return;
            if(!ability.TryNumber("levels",out double levels,out _) || rank>levels)return;
            var proc=new NativeWeaponProc{ability=ability.id,sourceKey=sourceKey,targetsAir=Array.IndexOf(targets.Split(','),"air")>=0};
            if(!ability.TryNumber("DataA"+suffix,out double chance,out _))return; // Sparse unknown is not a zero chance.
            if(code=="AOcr")
            {
                bool multiplier=ability.TryNumber("DataB"+suffix,out double factor,out _);
                bool flat=ability.TryNumber("DataC"+suffix,out double extra,out _);
                if(!multiplier && !flat)return;
                if(multiplier)proc.multiplier=factor;
                if(flat)proc.magicDamage=extra;
            }
            else if(code=="AHbh")
            {
                proc.buff=ability.Text("BuffID"+suffix);
                if(proc.buff==null || proc.buff.Length!=4 || !ability.TryNumber("Dur"+suffix,out proc.stunDuration,out _) ||
                    !ability.TryNumber("HeroDur"+suffix,out proc.heroStunDuration,out _))return;
                if(!ability.TryNumber("DataC"+suffix,out proc.magicDamage,out _))
                {
                    // I07U/A0OQ and I0A7/A16S have explicit chance/stun but sparse
                    // damage. Their stun works; additional damage remains an
                    // explicit coverage gap, not a measured zero/default claim.
                    if(!item)return;
                    proc.magicDamage=0;
                }
            }
            else
            {
                proc.manaBurn=chance;chance=100;
                if(!ability.TryNumber("DataC"+suffix,out proc.heroManaBurn,out _) ||
                    !ability.TryNumber("DataB"+suffix,out proc.burnDamageFactor,out _) ||
                    !ability.TryNumber("DataD"+suffix,out proc.heroBurnDamageFactor,out _))return;
            }
            foreach(double value in new[]{chance,proc.multiplier,proc.magicDamage,proc.stunDuration,proc.heroStunDuration,
                proc.manaBurn,proc.heroManaBurn,proc.burnDamageFactor,proc.heroBurnDamageFactor})
                if(!OriginalCombatDefinition.IsFinite(value) || value<0)throw new InvalidOperationException("native-weapon-proc-declaration-conflict:"+ability.id);
            if(chance>100)throw new InvalidOperationException("native-weapon-proc-chance-conflict:"+ability.id);
            // WPROC1 verifies individual families. Authored equivalent item
            // fields transfer those rules; release RNG and ordered independent
            // duplicate/family composition are explicit host reconstruction.
            // A0EW Cool1=3 is not a proc cooldown: native procs occur1.11s apart.
            if(chance==100 || chance>0 && RollWeapon(1000000)-1<chance*10000)result.Add(proc);
        }

        double ResolveNativeWeaponProcs(Projectile shot,OriginalWorldUnitView target)
        {
            double damage=shot.damage;
            foreach(var proc in shot.nativeProcs ?? Array.Empty<NativeWeaponProc>())
            {
                target=world.UnitState(shot.target);
                if(target==null || target.health<=0 || target.hidden || target.invulnerable)break;
                // WPROC1 seeded white weapon hits double in one callback.
                // Multiplication of item/cripple-composed damage is a declared
                // family transfer, not an independently measured combination.
                damage*=proc.multiplier;
                bool hero=IsNativeHeroPredicate(target);
                double stun=hero?proc.heroStunDuration:proc.stunDuration;
                if(stun>0)
                {
                    if(!proc.targetsAir && combatCatalog.Unit(target.rawcode).Text("movetp")=="fly" || CasterMagicImmune(target))continue;
                    string token="native-bash:"+shot.attacker+":"+proc.sourceKey;
                    // Own untimed token: expiry is advanced here so the native
                    // final zero event follows removal, without clearing other stuns.
                    if(!SetActorControl(target.entityId,token,AllActorControls,0,true,true))continue;
                    nativeBashes.RemoveAll(s=>s.target==target.entityId && s.token==token);
                    nativeBashes.Add(new NativeBashState{target=target.entityId,source=shot.attacker,owner=shot.owner,token=token,buff=proc.buff,remaining=stun});
                }
                if(proc.magicDamage>0)ApplyNativeTriggeredHit(shot.attacker,shot.owner,target,proc.magicDamage,OriginalTriggeredDamageMode.SpellMagic);
                double manaBurn=hero?proc.heroManaBurn:proc.manaBurn;
                if(manaBurn<=0)continue;
                target=world.UnitState(shot.target);
                if(target==null || target.health<=0 || target.hidden || target.invulnerable || CasterMagicImmune(target) || target.mana<=0)continue;
                // Full-mana WPROC1: a zero callback sees old MP, then debit,
                // then one physical hit includes the burned amount. Low-mana
                // min/debuff/immunity composition is the explicit host transfer.
                ApplyResolvedUnitHit(shot.attacker,shot.owner,target,0);
                target=world.UnitState(shot.target);
                if(target==null || target.health<=0 || target.hidden || target.invulnerable)continue;
                double burned=Math.Min(manaBurn,target.mana);
                if(!world.UpdateProfile(target.entityId,target.profile,target.health,target.mana-burned))
                    throw new InvalidOperationException("native-feedback-resource-update-rejected");
                damage+=burned*(hero?proc.heroBurnDamageFactor:proc.burnDamageFactor);
            }
            return damage;
        }

        void AdvanceNativeWeaponProcs(double seconds)
        {
            foreach(var state in nativeBashes.ToArray())
            {
                var target=world.UnitState(state.target);
                if(target==null || target.health<=0 || !actorControls.TryGetValue(state.target,out var controls) || !controls.ContainsKey(state.token))
                {nativeBashes.Remove(state);continue;}
                if(target.paused)continue;
                state.remaining-=seconds;
                if(state.remaining>1e-9)continue;
                nativeBashes.Remove(state);ClearActorControl(state.target,state.token);
                // Native WPROC1 AHbh emits one zero event on natural expiry.
                // Cleanse/death removes the state without manufacturing that event.
                ApplyResolvedUnitHit(state.source,state.owner,target,0);
            }
        }
    }
}

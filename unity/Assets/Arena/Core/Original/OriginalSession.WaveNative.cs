using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class WaveRootMissile { internal int target; internal double due; }
        readonly Dictionary<int,double> waveBloodlustRemaining=new Dictionary<int,double>();
        readonly List<WaveRootMissile> waveRootMissiles=new List<WaveRootMissile>();

        bool HasWaveDarkBuff(int actorId)
        {
            var actor=world.UnitState(actorId);
            if(!waveHelperActive || !waveTraits.Contains("A15R") || actor==null || actor.health<=.405 || actor.hidden ||
                !AreEnemies(0,actor.ownerSlot))return false;
            var ability=combatCatalog.Ability("A15S");
            if(ability.Text("code")!="AHab" || ability.Text("BuffID1")!="B0A4" || ability.Number("Area1")!=9000)
                throw new InvalidOperationException("wave-dark-aura-declaration-conflict");
            // WTRAIT1 proves B0A4 presence with unchanged mana regeneration.
            // Immediate membership and no linger after MD removal are the
            // explicit host aura policy; native aura scan/linger is unmeasured.
            return SquaredDistance(actor.position,new OriginalPoint(-50,1000))<=9000*9000;
        }

        double NativeDefendItemSpellFactor(OriginalWorldUnitView actor)
        {
            double factor=1;bool present=false;
            foreach(var ability in NativeUnitAbilities(actor))
            {
                if(ability.id!="A15I" && ability.id!="A09A" && ability.id!="A0RH" && ability.id!="A0X3" && ability.id!="A0X4")continue;
                double expected=ability.id=="A15I"?.2:ability.id=="A09A"?.5:ability.id=="A0RH"?.25:ability.id=="A0X3"?1.3:.7;
                if(ability.Text("code")!="AIdd" || ability.Number("DataE1")!=expected ||
                    ability.Number("DataA1")!=1 || ability.Number("DataB1")!=1 || ability.overrides.Length!=0)
                    throw new InvalidOperationException("wave-AIdd-declaration-conflict");
                factor=present?Math.Min(factor,expected):expected;present=true;
            }
            // WTRAIT1 paused hfoo40: E multiplies both SPELLS/NORMAL and
            // SPELLS/MAGIC; CHAOS/NORMAL and UNIVERSAL are unchanged. Multiple
            // simultaneous AIdd abilities use strongest-factor host policy.
            // A0X3/A0X4 book children transfer this native family to declared
            //1.3/.7. An amplification must not be clamped by an absent factor1.
            return factor;
        }

        double WaveBloodlustAttackSpeed(int actor)=>waveBloodlustRemaining.ContainsKey(actor)?1:0;
        void RemoveWaveBloodlust(int actor)
        {if(waveBloodlustRemaining.Remove(actor))RescaleWeaponRate(actor,world.Clock);}

        bool WaveNativeTarget(OriginalWorldUnitView actor,bool hostile)
        {
            return actor!=null && actor.health>.405 && !actor.hidden && !actor.invulnerable &&
                AreEnemies(0,actor.ownerSlot)==hostile && !CasterHasType(actor,"mechanical") &&
                !CasterHasType(actor,"structure") && !CasterMagicImmune(actor);
        }

        partial void ApplyWaveBloodlustPulse()
        {
            var definition=combatCatalog.Ability("A15N");
            if(definition.Text("code")!="Ablo" || definition.Text("BuffID1")!="Bblo" || definition.Number("DataA1")!=1 ||
                definition.Number("Dur1")!=2 || definition.Number("HeroDur1")!=2)
                throw new InvalidOperationException("wave-bloodlust-declaration-conflict");
            // niv28700: allied, living A15E recipients within3000 of MD.
            // WTRAIT1 all five spell callbacks occur in the order callback;
            // complete intervals prove IAS+1, speed stays250, recasts refresh2s.
            foreach(var actor in world.Snapshot().units)
            {
                if(!WaveNativeTarget(actor,false) || !HasEffectiveUnitAbility(actor,"A15E") ||
                    SquaredDistance(actor.position,new OriginalPoint(-50,1000))>3000*3000)continue;
                waveBloodlustRemaining[actor.entityId]=2;RescaleWeaponRate(actor.entityId,world.Clock);
                NotifyNativeSpellEffect(0,"A15N");
            }
        }

        partial void ApplyWaveSilencePulse()
        {
            var definition=combatCatalog.Ability("A15O");
            if(definition.Text("code")!="ANsi" || definition.Text("BuffID1")!="BNsi" || definition.Number("DataA1")!=8 ||
                definition.Number("Area1")!=100 || definition.Number("HeroDur1")!=4 || definition.Number("Dur1")!=4)
                throw new InvalidOperationException("wave-silence-declaration-conflict");
            var units=world.Snapshot().units;
            // nnv28713 selects heroes and issues a separate100WC point spell.
            // Nearby eligible units are native recipients of each such cast.
            foreach(var center in units)
            {
                if(!WaveNativeTarget(center,true) || !IsNativeHeroPredicate(center) ||
                    SquaredDistance(center.position,new OriginalPoint(-50,1000))>3000*3000)continue;
                NotifyNativeSpellEffect(0,"A15O");
                foreach(var actor in units)
                    if(WaveNativeTarget(actor,true) && SquaredDistance(actor.position,center.position)<=100*100)
                        // Unlike A0YI/DataA15, this DataA8 allows actual attacks
                        // and movement while rejecting A0Z3. Item orders were
                        // not tested. Paused countdown is a transferred policy.
                        SetActorControl(actor.entityId,"native-silence:A15O:wave",OriginalActorControlMask.Cast,4,true,true);
            }
        }

        partial void ApplyWaveRoot(int hero)
        {
            var actor=world.UnitState(hero);
            if(!waveHelperActive || !WaveNativeTarget(actor,true) || combatCatalog.Unit(actor.rawcode).Text("movetp")=="fly")return;
            var definition=combatCatalog.Ability("A15P");
            if(definition.Text("code")!="Aens" || definition.Text("BuffID1")!="B0A3,B0A3" ||
                definition.Number("DataC1")!=128 || definition.Number("HeroDur1")!=2 || definition.Number("Dur1")!=2)
                throw new InvalidOperationException("wave-root-declaration-conflict");
            double distance=Math.Sqrt(SquaredDistance(actor.position,new OriginalPoint(-50,1000)));
            if(distance>definition.Number("Rng1"))return;
            NotifyNativeSpellEffect(0,"A15P");
            // WTRAIT1 250WC arrives+.16803, consistent with OrcAbilityFunc
            // Aens Missilespeed1500. Fixed initial-distance scheduling and
            // moving-target delivery are an explicit host reconstruction.
            waveRootMissiles.Add(new WaveRootMissile{target=hero,due=world.Clock+Math.Max(.005,distance/1500)});
        }

        void AdvanceWaveNativeEffects(double seconds)
        {
            foreach(int id in new List<int>(waveBloodlustRemaining.Keys))
            {
                var actor=world.UnitState(id);
                if(actor==null || actor.health<=.405){RemoveWaveBloodlust(id);continue;}
                if(actor.paused)continue; // Shared buff-pause policy, not measured by WTRAIT1.
                waveBloodlustRemaining[id]-=seconds;
                if(waveBloodlustRemaining[id]<=1e-9)RemoveWaveBloodlust(id);
            }
            foreach(var missile in waveRootMissiles.ToArray())
            {
                if(missile.due>world.Clock+1e-9)continue;
                waveRootMissiles.Remove(missile);
                var actor=world.UnitState(missile.target);
                if(!WaveNativeTarget(actor,true))continue;
                // Two native zero callbacks surround application; the first
                // may synchronously kill/remove a unit through an observer.
                ApplyResolvedUnitHit(0,0,actor,0);actor=world.UnitState(missile.target);
                if(!WaveNativeTarget(actor,true))continue;
                if(world.Stop(actor.entityId))OnAcceptedWorldOrder(actor.entityId);
                SetActorControl(actor.entityId,"native-root:A15P:wave",OriginalActorControlMask.Move,2,true,true);
                ApplyResolvedUnitHit(0,0,world.UnitState(actor.entityId),0);
            }
            // A15S/B0A4 is a visible native marker with unchanged mana slope
            // in WTRAIT1. Its gameplay is nAv's item-rank mutation, handled by
            // WaveTraits; missing aura fields do not create a mana modifier.
        }
    }
}

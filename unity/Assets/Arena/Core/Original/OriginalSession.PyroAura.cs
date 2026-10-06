using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int,double> pyroChainBuffs = new Dictionary<int,double>();
        readonly Dictionary<int,double> statusBaseMovement = new Dictionary<int,double>();

        void CaptureAbilityMovementBase(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || UsesHeroCombatStats(actor) || statusBaseMovement.ContainsKey(id)) return;
            statusBaseMovement[id] = actor.profile.moveSpeed;
        }
        void ReleaseAbilityMovementBase(int id)
        {
            if (!poisons.ContainsKey(id) && !ordinaryStatuses.ContainsKey(id) && !pyroChainBuffs.ContainsKey(id) && ArcherDebuffMovementBonus(id) == 0 &&
                !HasBossBanish(id) && BossBindingMovementBonus(id) == 0 && CasterAuraMovementBonus(id) == 0 && ItemAuraMovementBonus(id) == 0 && SummonAbilityMovementBonus(id) == 0 && ItemStatusMovementBonus(id) == 0 && OrdinaryMovementBonus(id) == 0 && ItemScriptDebuffMovementBonus(id) == 0 && !IsRingForcedActor(id) && !IsShieldForceActive(id)) statusBaseMovement.Remove(id);
        }

        void ValidatePyroAura()
        {
            var ability = combatCatalog.Ability("A0SK");
            if (ability.Text("code") != "AOae" || ability.Text("BuffID1") != "B06T" ||
                ability.Number("DataA1") != -.25 || ability.Number("Area1") != 175 ||
                ability.Text("targs1") != "air,enemies,ground,vulnerable")
                throw new InvalidOperationException("Pyro aura conflicts with observed native profile.");
            // PYHELP1 campaign6dc410412ad8: speed250->187.5 while the
            // measured weapon interval is unchanged. Missing DataB is not
            // interpreted as a generic zero; this exact A0SK is observed.
            foreach (var field in ability.fields)
                if (field.key == "DataB1" && (!field.isNumber || field.conflict || field.number != 0))
                    throw new InvalidOperationException("Pyro aura attack-speed conflicts with observation.");
        }

        bool PyroHasChainBuff(int id) => pyroChainBuffs.TryGetValue(id,out double expires) && world.Clock < expires;
        double PyroMovementBonus(int id) => PyroHasChainBuff(id) ? -.25 : 0;
        double UnmodifiedNonHeroMovement(OriginalWorldUnitView actor)
        {
            if (statusBaseMovement.TryGetValue(actor.entityId,out double speed)) return speed;
            if (poisons.TryGetValue(actor.entityId,out var poison)) return poison.baseMoveSpeed;
            return actor.profile.moveSpeed;
        }

        void AddPyroChainBuff(OriginalWorldUnitView actor,double until)
        {
            bool first=!pyroChainBuffs.ContainsKey(actor.entityId);
            CaptureAbilityMovementBase(actor.entityId);
            pyroChainBuffs[actor.entityId]=until;
            if (first) RefreshAbilityMovement(actor.entityId);
        }
        void RemovePyroChainBuff(int id)
        {
            if (!pyroChainBuffs.Remove(id)) return;
            RefreshAbilityMovement(id); ReleaseAbilityMovementBase(id);
        }
        void AdvancePyroAuras()
        {
            // Replacement scheduler, not an observed native phase: .5s
            // membership scans plus3s decay. PYHELP1 observed the first buff
            // at issue+.6 and loss at emitter-removal+3.1 in .1s snapshots.
            foreach(var portal in pyroPortals)
                while(portal.auraNext<=world.Clock+1e-9 && portal.auraNext<=portal.expires+1e-9)
                {
                    double at=portal.auraNext; portal.auraNext+=.5;
                    foreach(var actor in world.Snapshot().units)
                        if(actor.health>.405 && !actor.hidden && !actor.invulnerable && AreEnemies(portal.owner,actor.ownerSlot) &&
                            SquaredDistance(actor.position,portal.position)<=175*175)
                            AddPyroChainBuff(actor,at+3);
                }
            foreach(var id in new List<int>(pyroChainBuffs.Keys))
            {
                var actor=world.UnitState(id);
                if(actor==null || actor.health<=.405 || world.Clock+1e-9>=pyroChainBuffs[id]) RemovePyroChainBuff(id);
            }
        }
    }
}

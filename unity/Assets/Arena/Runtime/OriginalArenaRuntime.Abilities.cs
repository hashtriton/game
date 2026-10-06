using System;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        int selectedEntityId;
        public OriginalWorldUnitView SelectedUnit { get; private set; }
        public string ArmedSkillId { get; private set; }
        public OriginalAbilityView[] SelectedUnitAbilities
        {
            get
            {
                if(SelectedUnit==null||network==null||SelectedUnit.ownerSlot!=network.LocalSlot)return Array.Empty<OriginalAbilityView>();
                if(SelectedUnit.kind==OriginalWorldUnitKind.Summon)
                    return View?.unitAbilities==null?Array.Empty<OriginalAbilityView>():
                        Array.Find(View.unitAbilities,g=>g.entityId==SelectedUnit.entityId)?.abilities??Array.Empty<OriginalAbilityView>();
                if(SelectedUnit.kind!=OriginalWorldUnitKind.Hero)return Array.Empty<OriginalAbilityView>();
                return View?.players==null?Array.Empty<OriginalAbilityView>():
                    Array.Find(View.players,p=>p.slot==network.LocalSlot)?.abilities??Array.Empty<OriginalAbilityView>();
            }
        }
        public string SelectedAbilityName(int index)
        {var abilities=SelectedUnitAbilities;return index<0||index>=abilities.Length?null:abilities[index].name??abilities[index].id;}

        public bool SelectOwnedUnit(int entityId)
        {
            if (View?.world == null) return false;
            var unit = Array.Find(View.world.units, u => u.entityId == entityId);
            if (unit == null || unit.ownerSlot != network.LocalSlot ||
                unit.kind == OriginalWorldUnitKind.Enemy || unit.health <= 0 || unit.hidden) return false;
            selectedEntityId = entityId; SelectedUnit = unit;
            AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0; return true;
        }

        void RefreshSelectedUnit(OriginalWorldSnapshot snapshot)
        {
            RefreshArmedItem();
            SelectedUnit = Array.Find(snapshot.units, u => u.entityId == selectedEntityId && u.ownerSlot == network.LocalSlot && u.health > 0 && !u.hidden);
            if (SelectedUnit == null)
            {
                selectedEntityId = LocalUnit?.entityId ?? 0;
                SelectedUnit = LocalUnit;
                ArmedSkillId = null;
            }
            if(ArmedSkillId!=null&&!Array.Exists(SelectedUnitAbilities,a=>a.castAbilityId==ArmedSkillId&&a.code==OriginalAbilityUseCode.Ready))ArmedSkillId=null;
        }

        public bool RequestSkill(int index)
        {
            if (InputBlocked || View == null || !View.started || !string.IsNullOrEmpty(View.haltReason) ||
                View.phase == OriginalMatchPhase.Won || View.phase == OriginalMatchPhase.Lost) return false;
            var abilities=SelectedUnitAbilities;
            if (SelectedUnit == null || SelectedUnit.hidden || index < 0 || index >= abilities.Length) return false;
            var ability = abilities[index];
            if (ability.code != OriginalAbilityUseCode.Ready) return false;
            AttackTargetArmed = false; ArmedItemInstance = 0;
            if (ability.targetMode == OriginalAbilityTargetMode.None)
            { ArmedSkillId = null; return network.SendCommand(OriginalSessionCommandKind.CastSkill, skillId: ability.castAbilityId,actorEntityId:SelectedUnit.entityId); }
            ArmedSkillId = ability.castAbilityId; return true;
        }

        void SelectPointerUnit()
        {
            var ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            int id = 0; float nearest = float.PositiveInfinity;
            foreach (var pair in actors)
            {
                var visual = pair.Value;
                if (visual.dead || !visual.actor || !UnitVisible(visual.state) || visual.state.ownerSlot != network.LocalSlot) continue;
                var bounds = new Bounds(visual.actor.transform.position + Vector3.up * .95f,
                    new Vector3(Mathf.Max(.55f, visual.actor.Radius * 2), 1.9f, Mathf.Max(.55f, visual.actor.Radius * 2)));
                if (bounds.IntersectRay(ray, out float distance) && distance < nearest) { nearest = distance; id = pair.Key; }
            }
            if (id != 0) SelectOwnedUnit(id);
        }

        void IssuePointerSkill()
        {
            var ability = Array.Find(SelectedUnitAbilities, a => a.castAbilityId == ArmedSkillId);
            if (ability == null || ability.code != OriginalAbilityUseCode.Ready) { ArmedSkillId = null; return; }
            var ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool sent = false;
            if (ability.targetMode == OriginalAbilityTargetMode.Unit)
            {
                int id = 0; float nearest = float.PositiveInfinity;
                foreach (var pair in actors)
                {
                    var visual = pair.Value;
                    if (visual.dead || !visual.actor || !UnitVisible(visual.state)) continue;
                    var bounds = new Bounds(visual.actor.transform.position + Vector3.up * .8f,
                        new Vector3(Mathf.Max(.55f, visual.actor.Radius * 2), 1.9f, Mathf.Max(.55f, visual.actor.Radius * 2)));
                    if (bounds.IntersectRay(ray, out float distance) && distance < nearest) { nearest = distance; id = pair.Key; }
                }
                if (id != 0) sent = network.SendCommand(OriginalSessionCommandKind.CastSkill, skillId: ArmedSkillId,
                    targetKind: OriginalWorldTargetKind.Unit, targetId: id,actorEntityId:SelectedUnit.entityId);
            }
            else if (ability.targetMode == OriginalAbilityTargetMode.Point)
                foreach (var hit in Physics.RaycastAll(ray, 1000))
                {
                    if (!(hit.collider is TerrainCollider)) continue;
                    sent = network.SendCommand(OriginalSessionCommandKind.CastSkill, skillId: ArmedSkillId,
                        x: hit.point.x * map.unitsPerMeter, y: hit.point.z * map.unitsPerMeter,actorEntityId:SelectedUnit.entityId); break;
                }
            if (sent) ArmedSkillId = null;
        }
    }
}

using System;
using Arena.Original;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        public long ArmedItemInstance { get; private set; }
        OriginalItemUseView ArmedItemView()
        {
            var player = View?.players == null ? null : Array.Find(View.players, p => p.slot == network.LocalSlot);
            return player?.itemUses == null ? null : Array.Find(player.itemUses,
                item => item.instanceId == ArmedItemInstance && item.code == OriginalItemUseCode.Ready);
        }
        void RefreshArmedItem() { if (ArmedItemInstance != 0 && ArmedItemView() == null) ArmedItemInstance = 0; }
        public bool RequestItemUse(long instance)
        {
            var player = View?.players == null ? null : Array.Find(View.players, p => p.slot == network.LocalSlot);
            var item = player?.itemUses == null ? null : Array.Find(player.itemUses, i => i.instanceId == instance);
            bool repeatWard = ArmedItemInstance == instance && item != null && (item.itemId == "I021" || item.itemId == "I094");
            if (item == null || item.code != OriginalItemUseCode.Ready || !SelectOwnedUnit(network.LocalSlot)) return false;
            AttackTargetArmed = false; ArmedSkillId = null;
            if (repeatWard)
            {
                bool sent = network.SendCommand(OriginalSessionCommandKind.UseItem, bag:item.bag, itemSlot:item.slot,
                    itemInstanceId:instance, targetItemInstanceId:instance);
                ArmedItemInstance = sent ? 0 : instance;
                return sent;
            }
            if (item.targetMode == OriginalAbilityTargetMode.None)
                return network.SendCommand(OriginalSessionCommandKind.UseItem, bag:item.bag, itemSlot:item.slot, itemInstanceId:instance);
            ArmedItemInstance = instance; return true;
        }
        void IssuePointerItem()
        {
            var item = ArmedItemView(); if (item == null) { ArmedItemInstance = 0; return; }
            var ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool sent = false;
            if (item.targetMode == OriginalAbilityTargetMode.Unit || item.targetMode == OriginalAbilityTargetMode.UnitOrPoint)
            {
                int id = 0; float nearest = float.PositiveInfinity;
                foreach (var pair in actors)
                {
                    var visual = pair.Value;
                    if (visual.dead || !visual.actor || !UnitVisible(visual.state)) continue;
                    float width = Mathf.Max(.55f, visual.actor.Radius * 2);
                    var bounds = new Bounds(visual.actor.transform.position + Vector3.up * .8f, new Vector3(width, 1.9f, width));
                    if (bounds.IntersectRay(ray, out float distance) && distance < nearest) { nearest = distance; id = pair.Key; }
                }
                if (id != 0) sent = network.SendCommand(OriginalSessionCommandKind.UseItem, bag:item.bag, itemSlot:item.slot,
                    itemInstanceId:item.instanceId, targetKind:OriginalWorldTargetKind.Unit, targetId:id);
            }
            if (!sent && item.targetMode != OriginalAbilityTargetMode.Unit)
                foreach (var hit in Physics.RaycastAll(ray, 1000))
                    if (hit.collider is TerrainCollider)
                    {
                        sent = network.SendCommand(OriginalSessionCommandKind.UseItem, bag:item.bag, itemSlot:item.slot,
                            itemInstanceId:item.instanceId, targetKind:OriginalWorldTargetKind.None,
                            x:hit.point.x * map.unitsPerMeter, y:hit.point.z * map.unitsPerMeter); break;
                    }
            if (sent) ArmedItemInstance = 0;
        }
    }
}

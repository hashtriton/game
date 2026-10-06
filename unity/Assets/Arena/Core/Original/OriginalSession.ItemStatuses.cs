using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class NativeItemStatus
        { internal double remaining,fade,bonus,strike; }
        readonly Dictionary<int,NativeItemStatus> itemInvisibility=new Dictionary<int,NativeItemStatus>();
        readonly Dictionary<int,NativeItemStatus> itemInvulnerability=new Dictionary<int,NativeItemStatus>();
        readonly Dictionary<int,NativeItemStatus> itemHaste=new Dictionary<int,NativeItemStatus>();
        const string ItemInvulnerabilityToken="item:B035";

        void ApplyNativeItemStatus(int actor,NativeItemActionRule rule)
        {
            var status=new NativeItemStatus{remaining=rule.duration,fade=rule.fade,bonus=rule.movement,strike=rule.strike};
            if(rule.status=="B034"||rule.status=="B031"||rule.status=="B0BB")
            {
                CaptureAbilityMovementBase(actor);pendingItemWindWalkStrikes.Remove(actor);
                itemInvisibility[actor]=status;RefreshAbilityMovement(actor);
            }
            else if(rule.status=="B035")
            { itemInvulnerability[actor]=status;world.SetTemporaryInvulnerability(actor,ItemInvulnerabilityToken,true); }
            else if(rule.status=="B0B1")itemRegeneration[actor]=new ItemRegenState();
        }
        bool ItemInvisibilityActive(int actor)
        {
            var unit=world?.UnitState(actor);
            return unit!=null&&unit.health>.405&&itemInvisibility.TryGetValue(actor,out var status)&&status.remaining>0&&status.fade<=1e-9;
        }
        bool CanSeeForCombat(int observerOwner,OriginalWorldUnitView target) => target!=null&&!target.hidden&&
            (!AreEnemies(observerOwner,target.ownerSlot)||!CombatInvisibilityActive(target.entityId)||NativeDetectionSees(observerOwner,target));
        void RevealItemInvisibility(int actor)
        {itemInvisibility.Remove(actor);RefreshAbilityMovement(actor);ReleaseAbilityMovementBase(actor);RevealNativeInvisibility(actor);}

        double ItemStatusMovementBonus(int actor) => (itemHaste.TryGetValue(actor,out var status)?status.bonus:0)+
            (itemInvisibility.TryGetValue(actor,out var invis)?invis.bonus:0)+FlameBootMovementBonus(actor);
        void ApplyItemHaste(int actor,double duration,double bonus)
        {
            CaptureAbilityMovementBase(actor);
            itemHaste[actor]=new NativeItemStatus{remaining=duration,bonus=bonus};
            RefreshAbilityMovement(actor);
        }
        void AdvanceItemStatuses(double seconds)
        {
            AdvanceItemRegeneration(seconds);
            AdvanceItemShells(seconds);
            AdvanceItemEnsnare(seconds);
            AdvanceFlameBoots(seconds);
            AdvanceWidowEffects();
            foreach(var pair in new List<KeyValuePair<int,NativeItemStatus>>(itemHaste))
            {
                var unit=world.UnitState(pair.Key);
                if(unit!=null&&unit.health>.405&&unit.paused)continue;
                pair.Value.remaining-=seconds;
                if(unit==null||unit.health<=.405||pair.Value.remaining<=1e-9)
                { itemHaste.Remove(pair.Key);RefreshAbilityMovement(pair.Key);ReleaseAbilityMovementBase(pair.Key); }
            }
            // Paused timers and latest-refresh arbitration are explicit host
            // policies. ITEMSTAT2 measures only a single unpaused use of each.
            foreach(var pair in new List<KeyValuePair<int,NativeItemStatus>>(itemInvisibility))
            {
                var unit=world.UnitState(pair.Key);
                if(unit==null||unit.health<=.405){RevealItemInvisibility(pair.Key);pendingItemWindWalkStrikes.Remove(pair.Key);continue;}
                if(unit.paused)continue;
                pair.Value.fade=Math.Max(0,pair.Value.fade-seconds);pair.Value.remaining-=seconds;
                if(pair.Value.remaining<=1e-9)RevealItemInvisibility(pair.Key);
            }
            foreach(var pair in new List<KeyValuePair<int,NativeItemStatus>>(itemInvulnerability))
            {
                var unit=world.UnitState(pair.Key);
                if(unit!=null&&unit.health>.405&&unit.paused)continue;
                pair.Value.remaining-=seconds;
                if(unit==null||unit.health<=.405||pair.Value.remaining<=1e-9)
                { itemInvulnerability.Remove(pair.Key);world.SetTemporaryInvulnerability(pair.Key,ItemInvulnerabilityToken,false); }
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly HashSet<int> hostilePlayerPairs = new HashSet<int>();
        readonly HashSet<int> duelXpSuspended = new HashSet<int>();
        bool duelShopsHidden, duelBoundaryEnabled;
        readonly Dictionary<int, bool> duelBoundaryInside = new Dictionary<int, bool>();

        bool ApplyDuelWorldBatch(OriginalDuelEvent[] batch)
        {
            if (world == null) return true;
            var transitions = new List<OriginalWorldTransition>();
            var projected = new SortedDictionary<int, OriginalWorldUnitView>();
            foreach (var unit in world.Snapshot().units) projected.Add(unit.entityId, unit);
            var removedGround = new HashSet<long>();
            void Stage(OriginalWorldTransition change)
            {
                if (!projected.TryGetValue(change.entityId, out var unit)) throw new InvalidOperationException("Transition reuses a removed actor.");
                transitions.Add(change);
                if (change.relocate) unit.position = change.position;
                if (change.revive) unit.health = unit.profile.maxHealth;
                if (change.fullHealth && unit.health > 0) unit.health = unit.profile.maxHealth;
                if (change.fullMana) unit.mana = unit.profile.maxMana;
                if (change.visible.HasValue) unit.hidden = !change.visible.Value;
                if (change.paused.HasValue) unit.paused = change.paused.Value;
                if (change.invulnerable.HasValue) unit.invulnerable = change.invulnerable.Value;
                if (change.kill) unit.health = 0;
                if (change.remove) projected.Remove(unit.entityId);
            }
            // Validate every currency delta before changing world positions.
            var balances = new Dictionary<int, OriginalInventory>();
            try
            {
                foreach (var item in batch)
                {
                    var player = item.slot == 0 ? null : players.Find(p => p.matchSlot == item.slot);
                    if (item.slot != 0 && player == null) throw new InvalidOperationException("Unknown duel participant.");
                    if (item.kind == OriginalDuelEventKind.Gold || item.kind == OriginalDuelEventKind.Souls)
                    {
                        if (!balances.TryGetValue(item.slot, out var inventory))
                        { inventory = player.inventory.Copy(); balances.Add(item.slot, inventory); }
                        long gold = item.kind == OriginalDuelEventKind.Gold ? item.amount : 0;
                        long souls = item.kind == OriginalDuelEventKind.Souls ? item.amount : 0;
                        if (item.amount < 0)
                        { if (!inventory.TrySpendResources(-gold, -souls)) throw new InvalidOperationException("Unfunded duel debit."); }
                        else inventory.GrantResources(gold, souls);
                    }
                    if (item.kind == OriginalDuelEventKind.Placement || item.kind == OriginalDuelEventKind.HeroState)
                    {
                        Stage(new OriginalWorldTransition { entityId = OriginalWorld.HeroEntityId(player.slot),
                            relocate = item.kind == OriginalDuelEventKind.Placement, position = new OriginalPoint(item.x, item.y),
                            revive = item.revive, fullHealth = item.fullLife, fullMana = item.fullMana, stop = item.stop,
                            setFacing = item.setFacing, facing = item.facing,
                            visible = item.visible == -1 ? (bool?)null : item.visible == 1,
                            paused = item.paused == -1 ? (bool?)null : item.paused == 1,
                            invulnerable = item.invulnerable == -1 ? (bool?)null : item.invulnerable == 1 });
                    }
                    if (item.kind == OriginalDuelEventKind.WorldRule &&
                        (item.code == "pair-prepare:pause-nonheroes-except-n03W" || item.code == "finish:unpause-world"))
                        foreach (var unit in projected.Values)
                            if (item.code == "finish:unpause-world" || unit.kind != OriginalWorldUnitKind.Hero && unit.rawcode != "n03W")
                                Stage(new OriginalWorldTransition { entityId = unit.entityId,
                                    paused = item.code != "finish:unpause-world" });
                    if (item.kind == OriginalDuelEventKind.WorldRule)
                        PlanDuelCleanup(item.code, projected, Stage, removedGround);
                }
                if (!world.TryApplyTransitions(transitions.ToArray()))
                { HaltReason = "world-duel-batch-placement-unavailable"; return false; }
                foreach (var transition in transitions)
                    if (transition.kill) OnPyroUnitDied(transition.entityId);
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is OverflowException || exception is ArgumentException)
            { HaltReason = "world-duel-batch-unavailable:" + exception.Message; return false; }
            foreach (long id in removedGround) groundItems.Remove(id);
            foreach (var item in batch)
            {
                var player = item.slot == 0 ? null : players.Find(p => p.matchSlot == item.slot);
                if (item.kind == OriginalDuelEventKind.Alliance) ApplyDuelAlliance(item);
                if (item.kind == OriginalDuelEventKind.Curse && player != null) ApplyDuelCurse(player,item);
                if (item.kind == OriginalDuelEventKind.HeroState)
                {
                    if (item.suspendXp == 1) duelXpSuspended.Add(player.slot);
                    else if (item.suspendXp == 0) duelXpSuspended.Remove(player.slot);
                    if (item.resetCooldowns) ResetAbilityCooldowns(player.slot);
                    if (item.removeBuffs) RemoveDispellableAbilityBuffs(player.slot);
                    if (item.stop) OnAcceptedWorldOrder(OriginalWorld.HeroEntityId(player.slot));
                    if (item.stop && weaponCycles.TryGetValue(OriginalWorld.HeroEntityId(player.slot), out var cycle)) cycle.winding = false;
                }
                if (item.kind == OriginalDuelEventKind.WorldRule)
                {
                    if (item.code == "pair-prepare:hide-pause-shops") duelShopsHidden = true;
                    if (item.code == "finish:unpause-world") duelShopsHidden = false;
                    if (item.code == "pair-prepare:enable-MA-boundary")
                    {
                        duelBoundaryEnabled = true; duelBoundaryInside.Clear();
                        foreach (var unit in world.Snapshot().units) duelBoundaryInside[unit.entityId] = InDuelArena(unit.position);
                    }
                    if (item.code == "finish:disable-MA-boundary") { duelBoundaryEnabled = false; duelBoundaryInside.Clear(); }
                }
            }
            return true;
        }

        static bool InDuelArena(OriginalPoint p) => p.x >= -1184 && p.x <= 1088 && p.y >= -3968 && p.y <= -1536;
        static bool InDuelSideArea(OriginalPoint p) => p.x >= 1088 && p.x <= 2304 && p.y >= -3392 && p.y <= -1536;
        // Kz13421. Helpers excluded by the source predicate are retained.
        static bool DuelCleanupEligible(string rawcode)
        {
            switch (rawcode)
            {
                case "h01O": case "h015": case "h04J": case "h046": case "e00M": case "h00G": case "u00M":
                case "h01H": case "n03W": case "e00P": case "n02Z": case "n02Y": case "n02X": case "e00O": return false;
                default: return true;
            }
        }

        void PlanDuelCleanup(string code, SortedDictionary<int, OriginalWorldUnitView> projected,
            Action<OriginalWorldTransition> stage, HashSet<long> removedGround)
        {
            if (code == "pair-prepare:delete-unowned-arena-items")
            {
                foreach (var pair in groundItems)
                    if (pair.Value.item.ownerId == 0 && InDuelArena(pair.Value.position)) removedGround.Add(pair.Key);
                return;
            }
            bool z2 = code == "pair-prepare:Z2-cleanup" || code == "pair-prepare:Z2-cleanup-repeat";
            bool e3 = code == "pair-prepare:combat-e3-cleanup";
            bool secondary = code == "finish:cleanup-secondary-hero-forms";
            if (!z2 && !e3 && !secondary) return;
            foreach (var unit in new List<OriginalWorldUnitView>(projected.Values))
            {
                if ((z2 || secondary) && (unit.rawcode == "E00J" || unit.rawcode == "E00E" || unit.rawcode == "O00D" || unit.rawcode == "U00T"))
                { stage(new OriginalWorldTransition { entityId = unit.entityId, kill = true }); continue; }
                if (!z2 && !e3) continue;
                bool special = HasEffectiveUnitAbility(unit, "A0K4");
                bool remove = false;
                if (InDuelArena(unit.position))
                {
                    if (e3) remove = unit.kind != OriginalWorldUnitKind.Hero && DuelCleanupEligible(unit.rawcode);
                    else if (!DuelCleanupEligible(unit.rawcode) || unit.kind == OriginalWorldUnitKind.Hero)
                    { if (unit.rawcode != "U00T") stage(new OriginalWorldTransition { entityId = unit.entityId, paused = true }); }
                    else remove = SourceUnitUserData(unit.entityId) != 2 && !special;
                }
                else if (z2 && InDuelSideArea(unit.position))
                    remove = unit.kind != OriginalWorldUnitKind.Hero && DuelCleanupEligible(unit.rawcode) && !special;
                if (remove) stage(new OriginalWorldTransition { entityId = unit.entityId, kill = true, remove = true });
            }
        }

        void AdvanceDuelBoundary()
        {
            if (!duelBoundaryEnabled) return;
            foreach (var unit in world.Snapshot().units)
            {
                bool inside = InDuelArena(unit.position);
                if (duelBoundaryInside.TryGetValue(unit.entityId, out bool wasInside) && wasInside && !inside &&
                    unit.kind != OriginalWorldUnitKind.Hero && DuelCleanupEligible(unit.rawcode))
                {
                    if (world.ForceUnitDeath(unit.entityId)) OnPyroUnitDied(unit.entityId); // XSv/Xtv32753, explicit KillUnit, no killer.
                }
                duelBoundaryInside[unit.entityId] = inside;
            }
        }

        bool AreEnemies(int firstOwner, int secondOwner)
        {
            if (firstOwner == secondOwner) return false;
            if (firstOwner == 0 || secondOwner == 0) return true;
            return hostilePlayerPairs.Contains(firstOwner * 16 + secondOwner);
        }

        void ApplyDuelAlliance(OriginalDuelEvent item)
        {
            foreach (var first in players)
                foreach (var second in players)
                {
                    if (first == second) continue;
                    if (item.slot != 0 && !(first.matchSlot == item.slot && second.matchSlot == item.otherSlot ||
                        first.matchSlot == item.otherSlot && second.matchSlot == item.slot)) continue;
                    int key = first.slot * 16 + second.slot;
                    if (item.amount == 0) hostilePlayerPairs.Add(key);
                    else hostilePlayerPairs.Remove(key);
                }
        }
    }
}

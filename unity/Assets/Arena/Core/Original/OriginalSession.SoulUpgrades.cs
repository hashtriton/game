using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class SoulResearch
        {
            internal readonly Dictionary<string, int> ranks = new Dictionary<string, int>(StringComparer.Ordinal);
            internal string researching;
            internal double completesAt;
        }
        readonly Dictionary<int, SoulResearch> soulResearch = new Dictionary<int, SoulResearch>();

        int SoulUpgradeRank(int slot, string id) => soulResearch.TryGetValue(slot, out var state) && state.ranks.TryGetValue(id, out int rank) ? rank : 0;
        int CompletedBasicSoulUpgrades(int slot)
        {
            int total = 0;
            foreach (string id in OriginalSoulUpgradeRules.BasicIds) total += SoulUpgradeRank(slot, id);
            return total;
        }
        OriginalSessionReplyCode BuySoulUpgrade(Player player, string id)
        {
            if (!Started || world == null || player.inventory == null || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            int maximum = OriginalSoulUpgradeRules.Maximum(id);
            if (maximum == 0) return OriginalSessionReplyCode.InvalidCommand;
            int rank = SoulUpgradeRank(player.slot, id);
            if (rank >= maximum || OriginalSoulUpgradeRules.IsUnique(id) && CompletedBasicSoulUpgrades(player.slot) != 60)
                return OriginalSessionReplyCode.NotReady;
            if (OriginalSoulUpgradeRules.IsUnique(id) && !CanApplyUniqueSoulUpgrade(player, id))
                return OriginalSessionReplyCode.RuleUnavailable;
            if (soulResearch.TryGetValue(player.slot, out var current) && current.researching != null)
                return OriginalSessionReplyCode.NotReady;
            int cost = OriginalSoulUpgradeRules.Cost(id, rank);
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null) return OriginalSessionReplyCode.NotReady;
            // The servant is an inventory/research actor in the host model.
            // One pending research is an explicit host queue policy. Native
            // timebase=1 is retained; native multi-order queuing is not measured.
            if (!player.inventory.TrySpendResources(0, cost)) return OriginalSessionReplyCode.ItemRejected;
            if (current == null) soulResearch[player.slot] = current = new SoulResearch();
            current.researching = id; current.completesAt = world.Clock + 1;
            return OriginalSessionReplyCode.Accepted;
        }
        void AdvanceSoulUpgrades()
        {
            foreach (var player in players)
            {
                if (!soulResearch.TryGetValue(player.slot, out var state) || state.researching == null || world.Clock + 1e-9 < state.completesAt) continue;
                string id = state.researching; int previous = SoulUpgradeRank(player.slot, id);
                var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                if (actor == null) continue;
                state.ranks[id] = previous + 1;
                try
                {
                    if (OriginalSoulUpgradeRules.IsUnique(id))
                    {
                        // Unique grants have their own source vitality behavior:
                        // STR restores life, INT restores mana, AGI preserves both.
                        ApplyUniqueSoulUpgrade(player, id);
                        state.researching = null;
                        continue;
                    }
                    var stats = HeroCombatStats(player.slot);
                    var profile = actor.profile.Copy();
                    profile.maxHealth = stats.maxHealth.Require(); profile.maxMana = stats.maxMana.Require();
                    profile.moveSpeed = ResolveAbilityMoveSpeed(player.slot, stats.baseMoveSpeed);
                    if (IsRingForcedActor(actor.entityId) || IsShieldForceActive(actor.entityId)) profile.moveSpeed = 0;
                    // SOUL1 H008 rank0->1:233.47/631 becomes263/711 and
                    //62.35/145 becomes84/195. One rank is applied per research.
                    double health = UpdatedVitality(actor.health, actor.profile.maxHealth, profile.maxHealth, "ratio-nearest-approximation", true);
                    double mana = UpdatedVitality(actor.mana, actor.profile.maxMana, profile.maxMana, "ratio-nearest-approximation", false);
                    if (!world.UpdateProfile(actor.entityId, profile, health, mana)) throw new InvalidOperationException("soul-profile-placement");
                    state.researching = null;
                }
                catch (InvalidOperationException)
                {
                    // A host profile failure rolls back and refunds this single
                    // research. Never publish a rank without its world effect.
                    if (previous == 0) state.ranks.Remove(id); else state.ranks[id] = previous;
                    player.inventory.GrantResources(0, OriginalSoulUpgradeRules.Cost(id, previous));
                    state.researching = null;
                    player.lastItemAction = OriginalItemActionCode.UnresolvedRule;
                }
            }
        }
        OriginalHeroStatsSnapshot ComposeSoulUpgrades(OriginalHeroStatsSnapshot baseline, int slot)
        {
            var result = baseline.Copy();
            int health = SoulUpgradeRank(slot, "R002"), mana = SoulUpgradeRank(slot, "R003"), armor = SoulUpgradeRank(slot, "R000");
            int attack = SoulUpgradeRank(slot, "R001"), speed = SoulUpgradeRank(slot, "R006"), regeneration = SoulUpgradeRank(slot, "R004");
            result.maxHealth = Plus(result.maxHealth, 80 * health); result.maxMana = Plus(result.maxMana, 50 * mana);
            result.armor = Plus(result.armor, armor); result.baseMoveSpeed += 4 * speed;
            result.upgradeAttackDamageBonus = 5 * attack; result.upgradeAttackSpeedBonus = .05 * speed;
            // Native rhpr/rmnr are FLAT additions, despite the old tooltip's%.
            result.upgradeRegenPerSecond = .15 * regeneration;
            result.attackMinimum = Plus(result.attackMinimum, result.upgradeAttackDamageBonus);
            result.attackMaximum = Plus(result.attackMaximum, result.upgradeAttackDamageBonus);
            return ComposeUniqueSoulStats(slot,result);
        }
        OriginalSoulUpgradeView[] SoulUpgradeViews(Player player)
        {
            if (!Started || player.inventory == null) return Array.Empty<OriginalSoulUpgradeView>();
            var result = new List<OriginalSoulUpgradeView>();
            soulResearch.TryGetValue(player.slot, out var state);
            void Add(string id)
            {
                int rank = SoulUpgradeRank(player.slot, id), maximum = OriginalSoulUpgradeRules.Maximum(id);
                bool researching = state != null && state.researching == id;
                result.Add(new OriginalSoulUpgradeView { id = id, name = OriginalSoulUpgradeRules.Name(id), rank = rank,
                    maximumRank = maximum, soulCost = rank < maximum ? OriginalSoulUpgradeRules.Cost(id, rank) : 0,
                    unlocked = OriginalSoulUpgradeRules.IsBasic(id) || CompletedBasicSoulUpgrades(player.slot) == 60,
                    researching = researching, remainingSeconds = researching ? Math.Max(0, state.completesAt - world.Clock) : 0 });
            }
            foreach (string id in OriginalSoulUpgradeRules.BasicIds) Add(id);
            foreach (string id in OriginalSoulUpgradeRules.UniqueIds) Add(id);
            return result.ToArray();
        }
    }
}

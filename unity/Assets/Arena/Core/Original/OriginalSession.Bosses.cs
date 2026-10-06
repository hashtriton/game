using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        readonly Dictionary<int, OriginalBossScaling> bossScaling = new Dictionary<int, OriginalBossScaling>();
        // UnitRemoveBuffs(false,true) leaves positive skills and their source
        // timers intact. Active auras may apply their negative buff again.
        void RemoveNegativeAbilityBuffs(int id)
        {
            RemovePoison(id); RemoveArcherDebuffs(id); RemoveShieldCripple(id);
            RemoveNativeCorruption(id);
            RemoveKnightAcid(id); RemovePyroChainBuff(id);
            bossRainBurns.Remove(id);
            ClearNegativeActorControls(id);
        }
        void SpawnBossPhaseAddIfRequired()
        {
            if (match.Round != 25) return;
            var add = match.SpawnBossPhaseAdd();
            CollectEvents();
            if (HaltReason != null) return;
            ApplyUnitAbilityOverlay(OriginalWorld.EnemyEntityId(add.entityId), null, new[] { "A11G" });
        }
        OriginalObservedSparseUnit BossIntrinsic(string rawcode)
        {
            if (rawcode != "O006") return null;
            if (observed?.sparse == null) throw new InvalidOperationException("final-boss-level50-observation-missing");
            return observed.sparse.Unit("O006", 50);
        }
        double BossPrimaryDamageBonus(OriginalWorldUnitView unit) =>
            unit.rawcode == "O006" ? BossIntrinsic(unit.rawcode).RequireAgility() * native.Constant("StrAttackBonus").Require() : 0;
        double BossAgilityAttackSpeedBonus(OriginalWorldUnitView unit) =>
            unit.rawcode == "O006" ? BossIntrinsic(unit.rawcode).RequireAgility() * native.Constant("AgiAttackSpeedBonus").Require() : 0;
        double BossArmorBonus(int id) => (bossScaling.TryGetValue(id, out var value) ? value.armor : 0) +
            (bossSpecialPhases.Contains(id) ? 9000 : 0); // A0A7 AIde DataA1, nFv/nfv.
        double BossAttackBonus(int id) => bossScaling.TryGetValue(id, out var value) ? value.attack : 0;
        void SyncFinalBossHealth()
        {
            if (world == null || match == null || match.Round != 30 || match.FinalBossEntityId == 0) return;
            var boss = world.UnitState(OriginalWorld.EnemyEntityId(match.FinalBossEntityId));
            if (boss != null) match.SetBossHealthFraction(boss.health / boss.profile.maxHealth);
        }
        bool ApplyBossWorldEvent(OriginalMatchEvent item)
        {
            bool scaling = item.kind == OriginalMatchEventKind.BossScaling || item.kind == OriginalMatchEventKind.FinalAddScaling;
            if (!scaling && item.kind != OriginalMatchEventKind.BossPause && item.kind != OriginalMatchEventKind.BossResume &&
                item.kind != OriginalMatchEventKind.BossRegeneration) return false;
            try
            {
                int id = OriginalWorld.EnemyEntityId(item.entityId);
                var actor = world.UnitState(id);
                if (actor == null) throw new InvalidOperationException("boss-actor-missing:" + id);
                if (scaling)
                {
                    if (bossScaling.ContainsKey(id)) throw new InvalidOperationException("boss-scaled-twice:" + id);
                    var bonus = OriginalBossRules.Scaling(actor.rawcode, item.amount, item.round,
                        item.kind == OriginalMatchEventKind.FinalAddScaling, actor.profile.maxHealth);
                    var profile = actor.profile.Copy(); profile.maxHealth += bonus.health;
                    // Both i0/f3 execute immediately after the native creation.
                    // The newly created unit is at full life before the bonus.
                    if (!world.UpdateProfile(id, profile, profile.maxHealth, actor.mana)) throw new InvalidOperationException("boss-profile-rejected");
                    bossScaling.Add(id, bonus);
                    if (actor.rawcode == "u00G") bossWindRemaining[id] = 4;
                    if (actor.rawcode == "n017") bossQuadrantsRemaining[id] = 3;
                    if (actor.rawcode == "n00Z") bossMeteorRemaining[id] = 3;
                    if (actor.rawcode == "n00K") bossGhostRemaining[id] = 2;
                    if (actor.rawcode == "n0AW") bossBarrageRemaining[id] = 4;
                }
                else if (item.kind == OriginalMatchEventKind.BossRegeneration)
                {
                    if (actor.health > 0 && !world.UpdateProfile(id, actor.profile,
                        Math.Min(actor.profile.maxHealth, actor.health + item.amount),
                        Math.Min(actor.profile.maxMana, actor.mana + item.secondaryAmount)))
                        throw new InvalidOperationException("boss-regeneration-rejected");
                }
                else if (item.sourceRule == "final-phases")
                {
                    bool resume = item.kind == OriginalMatchEventKind.BossResume;
                    var position = resume ? new OriginalPoint(item.x, item.y) : new OriginalPoint(0, -2700);
                    if (resume && !world.TryFindFreeSpawn(position, actor.profile.collisionRadius, 512, out position))
                        throw new InvalidOperationException("boss-resume-placement-unavailable");
                    var change = new OriginalWorldTransition { entityId = id, relocate = true, position = position,
                        paused = !resume, invulnerable = !resume, visible = resume, stop = true };
                    if (!world.TryApplyTransitions(new[] { change })) throw new InvalidOperationException("boss-phase-transition-rejected");
                }
                else world.SetUnitState(id, paused: item.kind == OriginalMatchEventKind.BossPause);
                SyncFinalBossHealth();
            }
            catch (InvalidOperationException ex) { HaltReason = "world-boss-rule-unavailable:" + ex.Message; }
            return true;
        }
    }
}

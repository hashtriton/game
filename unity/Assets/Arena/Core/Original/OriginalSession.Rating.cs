using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        OriginalDuelRatingLedger ratingLedger;
        double nextRatingSecond;

        bool[] RatingDeadStates()
        {
            var result = new bool[players.Count];
            foreach (var player in players)
            {
                var unit = world?.UnitState(OriginalWorld.HeroEntityId(player.slot));
                result[player.matchSlot - 1] = unit == null ? !match.IsAlive(player.matchSlot) : unit.health <= .405;
            }
            return result;
        }

        void ApplyRatingMatchEvent(OriginalMatchEvent item)
        {
            if (ratingLedger == null) return;
            if (item.kind == OriginalMatchEventKind.ShopAccess && !item.enabled)
            { ratingLedger.BeginCombat(); nextRatingSecond = item.time + 1; }
            if (item.kind == OriginalMatchEventKind.PhaseChanged &&
                (item.amount == (int)OriginalMatchPhase.Preparation || item.amount == (int)OriginalMatchPhase.DuelPreparation) &&
                ratingLedger.Snapshot().combatActive)
                ratingLedger.CompleteRound(RatingDeadStates());
        }

        void AdvanceRatingClock()
        {
            if (ratingLedger == null || DuelActive || !ratingLedger.Snapshot().combatActive) return;
            while (match.Clock + 1e-9 >= nextRatingSecond)
            { ratingLedger.TickCombatSecond(RatingDeadStates()); nextRatingSecond += 1; }
        }

        OriginalMatchEnemy EnemyState(int worldEntityId)
        {
            foreach (var enemy in match.Enemies)
                if (OriginalWorld.EnemyEntityId(enemy.entityId) == worldEntityId) return enemy;
            return null;
        }

        // All weapon and triggered-damage callers have already resolved armor,
        // attack type and incoming modifiers. Ownership survives projectile life.
        bool ApplyResolvedUnitHit(int attackerEntityId, int ownerSlot, OriginalWorldUnitView target, double damage, double? sourceEventDamage = null) =>
            ApplyResolvedWeaponOrSpellHit(attackerEntityId, ownerSlot, target, damage, sourceEventDamage, false);

        bool ApplyResolvedWeaponOrSpellHit(int attackerEntityId, int ownerSlot, OriginalWorldUnitView target, double damage, double? sourceEventDamage = null, bool primaryWeapon = false)
        {
            target = target == null ? null : world.UnitState(target.entityId);
            if (!OriginalCombatDefinition.IsFinite(damage) || damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            if (target == null || target.health <= 0 || target.invulnerable) return false;
            var victim = target.kind == OriginalWorldUnitKind.Enemy ? EnemyState(target.entityId) : null;
            int contributor = ownerSlot == 0 ? 0 : PlayerAt(ownerSlot).matchSlot;
            // THORNS_REAL/THORNS_SLOW and ADEF native callbacks report damage
            // after numeric armor/type/Defend, unlike AEah's reflection basis.
            // Keep an explicit override for effects with a separately measured
            // event stage; never substitute raw weapon dice into DU's ledger.
            if (sourceEventDamage.HasValue && (!OriginalCombatDefinition.IsFinite(sourceEventDamage.Value) || sourceEventDamage.Value < 0))
                throw new ArgumentOutOfRangeException(nameof(sourceEventDamage));
            damage = ResolveNativeSleepDamage(target.entityId, damage);
            if (sourceEventDamage.HasValue) sourceEventDamage = ResolveNativeSleepDamage(target.entityId, sourceEventDamage.Value);
            RecordRoundDamage(ownerSlot,target,sourceEventDamage??damage);
            ObserveBossNativeDamage(target, sourceEventDamage ?? damage);
            ObserveBossShieldDamage(target, sourceEventDamage ?? damage);
            ObserveBossBindingDamage(target, sourceEventDamage ?? damage);
            ObserveItemScriptActDamage(target, sourceEventDamage ?? damage);
            ObserveSummonShieldDamage(target, sourceEventDamage ?? damage);
            ObserveItemChannelDamage(world.UnitState(target.entityId), sourceEventDamage ?? damage);
            ObserveErosAmuletDamage(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage);
            ObserveWarpathDamage(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage);
            ObserveArcherDamage(attackerEntityId, target.entityId);
            if (ObserveBossWardDamage(attackerEntityId, world.UnitState(target.entityId), sourceEventDamage ?? damage)) return true;
            if (ObserveCasterTotemDamage(attackerEntityId, world.UnitState(target.entityId), sourceEventDamage ?? damage)) return true;
            ObserveUniqueSoulDamage(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage,primaryWeapon);
            ObserveItemScriptActWeapon(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage,primaryWeapon);
            ObserveWidowWeapon(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage,primaryWeapon);
            ObserveChargeBladeWeapon(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage,primaryWeapon);
            ObserveItemAxeWeapon(attackerEntityId,world.UnitState(target.entityId),sourceEventDamage ?? damage,primaryWeapon);
            target = world.UnitState(target.entityId);
            if (target == null || target.health <= 0) return false;
            // ARCHH2 A0LH emits EVENT_UNIT_DAMAGED even when resistance makes
            // damage zero. aie82917 has no positive-damage guard: its watcher
            // still consumes the orb, but zero must not mutate HP or rewards.
            if (damage == 0) return true;
            if (!world.ApplyUnitDamage(target.entityId, damage)) return false;
            InterruptItemRegeneration(target.entityId);
            if (primaryWeapon) ApplyItemWeaponLifeSteal(attackerEntityId, target.entityId, Math.Min(damage, target.health));
            NotifyNativeSleepDamage(target.entityId, damage);
            if (victim != null) ratingLedger?.RecordEnemyDamage(contributor, victim.sourceUserData, sourceEventDamage ?? damage, target.health);
            if (damage < target.health) return true;
            ObserveWarpathKill(attackerEntityId,target.entityId);
            ObserveWaveTraitDeath(world.UnitState(target.entityId), ownerSlot);
            ForgetActorControls(target.entityId);
            OnPyroUnitDied(target.entityId);
            if (target.kind == OriginalWorldUnitKind.Hero) ReportHeroDied(target.ownerSlot, ownerSlot);
            else if (victim != null)
            {
                ratingLedger?.RecordEnemyDeath(contributor, victim.sourceUserData,
                    HasEffectiveUnitAbility(target, "A0K4"), DuelActive);
                if (nativeBounty != null) ApplyNativeBounty(ownerSlot, victim);
                ReportEnemyKilled(victim.entityId, ownerSlot > 0);
            }
            else if (target.kind == OriginalWorldUnitKind.Enemy) OnScriptedEnemyDied(target.entityId, ownerSlot);
            else if (target.kind == OriginalWorldUnitKind.Summon) ObserveItemSummonDeath(target.entityId);
            else if (target.kind == OriginalWorldUnitKind.Illusion) ObserveImageDeath(target.entityId);
            return true;
        }

        void BeginWorldDuelIfReady()
        {
            if (world == null || !pendingDuel || ratingLedger == null || HaltReason != null) return;
            var roster = new OriginalDuelParticipant[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                roster[i] = new OriginalDuelParticipant { slot = player.slot, heroRawcode = player.hero,
                    rating = ratingLedger.Rating(player.matchSlot, player.progression?.Level ?? 1),
                    mirrorCurse = player.auxiliaryAbilities.ContainsKey("A19P"), bloodPoisonCurse = player.auxiliaryAbilities.ContainsKey("A19Q") };
            }
            if (!BeginDuel(roster)) HaltReason = "world-duel-start-rejected";
        }
    }
}

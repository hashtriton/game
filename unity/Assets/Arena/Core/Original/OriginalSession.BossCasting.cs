using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossNativeCast
        {
            internal int actor;
            internal int targetEntity;
            internal string ability = "A101";
            internal OriginalPoint target;
            internal double effectAt;
        }
        readonly Dictionary<int, BossNativeCast> bossCasts = new Dictionary<int, BossNativeCast>();
        readonly Dictionary<int, double> bossChargeCooldowns = new Dictionary<int, double>();
        double nextBossOrder = 2.5;
        uint bossOrderRandom;

        bool BossControlsActor(int id) => bossCasts.ContainsKey(id) || ornCasts.ContainsKey(id) || BossImageControlsActor(id);
        void CancelBossCastOnOrder(int id) { bossCasts.Remove(id); ornCasts.Remove(id); CancelBossImageCastOnOrder(id); }

        void AdvanceBossNativeCasts()
        {
            foreach (var cast in new List<BossNativeCast>(bossCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(cast.actor))
                { bossCasts.Remove(cast.actor); continue; }
                if (cast.effectAt > world.Clock + 1e-9) continue;
                bossCasts.Remove(cast.actor);
                if (cast.ability == "A1D7")
                {
                    if (!world.TrySpendMana(cast.actor, 150)) continue;
                    bossStormCooldowns[cast.actor] = cast.effectAt + 20;
                    BeginBossStorm(cast.actor); continue;
                }
                if (cast.ability == "A0TU" || cast.ability == "A1D6")
                {
                    var victim = world.UnitState(cast.targetEntity);
                    double cost = cast.ability == "A0TU" ? 300 : 175;
                    if (victim == null || victim.health <= .405 || victim.hidden || victim.invulnerable || CasterMagicImmune(victim) ||
                        !world.TrySpendMana(cast.actor, cost)) continue;
                    if (cast.ability == "A0TU")
                    {
                        bossBindingCooldowns[cast.actor] = cast.effectAt + 34;
                        // BSPAR2's native application event precedes B09F.
                        ApplyResolvedUnitHit(cast.actor, actor.ownerSlot, victim, 0);
                        var live = world.UnitState(cast.targetEntity);
                        if (live != null && live.health > .405) ApplyBossBindingDebuff(cast.targetEntity);
                        BeginBossBinding(cast.actor, cast.targetEntity);
                    }
                    else
                    {
                        bossDeathFingerCooldowns[cast.actor] = cast.effectAt + 15;
                        ResolveBossDeathFinger(cast.actor, cast.targetEntity);
                    }
                    continue;
                }
                if (cast.ability == "A055")
                {
                    var victim = world.UnitState(cast.targetEntity);
                    if (victim == null || victim.health <= .405 || victim.hidden || victim.invulnerable || CasterMagicImmune(victim) ||
                        !world.TrySpendMana(cast.actor, 200)) continue;
                    bossBanishCooldowns[cast.actor] = cast.effectAt + 15;
                    BeginBossBanish(cast.actor, cast.targetEntity); continue;
                }
                if (cast.ability == "A07B")
                {
                    if (!world.TrySpendMana(cast.actor, 200)) continue;
                    bossSilenceCooldowns[cast.actor] = cast.effectAt + 16;
                    BeginBossSilence(cast.actor, actor.ownerSlot, cast.target); continue;
                }
                if (cast.ability == "A0TS")
                {
                    if (!world.TrySpendMana(cast.actor, 200)) continue;
                    bossWardCooldowns[cast.actor] = cast.effectAt + 24;
                    BeginBossWards(cast.actor); continue;
                }
                if (cast.ability == "A0QD")
                {
                    if (!world.TrySpendMana(cast.actor, 150)) continue;
                    bossInfernoCooldowns[cast.actor] = cast.effectAt + 15;
                    BeginBossInferno(cast.actor, actor.ownerSlot, cast.target); continue;
                }
                if (cast.ability == "A04V")
                {
                    if (!world.TrySpendMana(cast.actor, 200)) continue;
                    bossRainCooldowns[cast.actor] = cast.effectAt + 16;
                    BeginBossRain(cast.actor, actor.ownerSlot, cast.target); continue;
                }
                if (cast.ability == "A0X9")
                {
                    if (!world.TrySpendMana(cast.actor, 150)) continue;
                    bossShieldCooldowns[cast.actor] = cast.effectAt + 20;
                    BeginBossShield(cast.actor, cast.effectAt); continue;
                }
                if (!world.TrySpendMana(cast.actor, 50)) continue;
                bossChargeCooldowns[cast.actor] = cast.effectAt + 8;
                BeginBossCharge(cast.actor);
            }
            // dA/Ehv (31430, 86464) issues orders every 2.5 seconds while
            // enabled. Its map clock phase and native group ordering are not
            // replayed; stable host time and entity ordering replace them.
            while (nextBossOrder <= world.Clock + 1e-9)
            {
                nextBossOrder += 2.5;
                if (match.Round % 5 == 0 && (match.Phase == OriginalMatchPhase.Combat ||
                    match.Phase == OriginalMatchPhase.FinalIntermission)) SelectBossOrders();
            }
        }

        void SelectBossOrders()
        {
            var heroes = new List<OriginalWorldUnitView>();
            foreach (var player in players)
            {
                var hero = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                if (hero != null && hero.health > .405) heroes.Add(hero);
            }
            if (heroes.Count == 0) return;
            if (bossOrderRandom == 0) bossOrderRandom = unchecked((uint)seed) ^ 0xB055CA57u;
            if (bossOrderRandom == 0) bossOrderRandom = 1;
            bossOrderRandom ^= bossOrderRandom << 13; bossOrderRandom ^= bossOrderRandom >> 17; bossOrderRandom ^= bossOrderRandom << 5;
            var target = heroes[(int)(bossOrderRandom / 4294967296.0 * heroes.Count)].position;
            foreach (var actor in CasterUnits())
            {
                if (actor.ownerSlot != 0 || actor.health <= .405 || actor.paused || actor.hidden ||
                    CasterHasAbility(actor, "A0K4") || SquaredDistance(actor.position, new OriginalPoint(0, -2688)) > 2400 * 2400) continue;
                enemyGoals[actor.entityId] = target;
                CasterMoveOrder(actor.entityId, target);
                // EBv first attempts A04V, absent until the first ghost phase.
                // A101 is the measured native roar order: BOSS1 effect +0.5s,
                // authored Cost1=50 and Cool1=8. Other boss orders are separate.
                if (actor.rawcode == "n00K") { TryStartBossRainCast(actor.entityId, target); TryStartBossChargeCast(actor.entityId); }
                if (actor.rawcode == "n00Z")
                {
                    if (actor.health / actor.profile.maxHealth <= .75) TryStartBossShieldCast(actor.entityId);
                    if (LivingInfernalCount() < (match.Participants > 4 ? 2 : 1)) TryStartBossInfernoCast(actor.entityId, target);
                }
                if (actor.rawcode == "n0AW")
                {
                    TryStartBossShockwave(actor.entityId); SelectBossDeathFinger(actor);
                    if (actor.health / actor.profile.maxHealth <= .5) TryStartBossStormCast(actor.entityId);
                }
                if (actor.rawcode == "n017" && BossHasNearbyEnemy(actor, 500)) TryStartBossSilenceCast(actor.entityId, target);
                if (actor.rawcode == "n017") { SelectBossBanish(actor); TryStartBossMirrorCast(actor.entityId); }
                if (actor.rawcode == "u00G" && !bossWindPhases.ContainsKey(actor.entityId))
                {
                    if (actor.health / actor.profile.maxHealth <= .9) TryStartBossBurst(actor.entityId);
                    if (actor.health / actor.profile.maxHealth <= .7) TryStartBossWardCast(actor.entityId);
                    SelectBossBinding(actor);
                }
                if (actor.rawcode == "O006") SelectOrnActiveOrders(actor);
            }
        }

        bool TryStartBossChargeCast(int actorId)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(actorId) || actor.mana < 50 ||
                !HasEffectiveUnitAbility(actor, "A101") ||
                bossCharges.ContainsKey(actorId) || bossSpecialPhases.Contains(actorId) ||
                !TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic) ||
                bossChargeCooldowns.TryGetValue(actorId, out double until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(actorId);
            world.Stop(actorId); world.MarkCast(actorId);
            if (weaponCycles.TryGetValue(actorId, out var cycle)) cycle.winding = false;
            bossCasts[actorId] = new BossNativeCast { actor = actorId, effectAt = world.Clock + .5 };
            return true;
        }
    }
}

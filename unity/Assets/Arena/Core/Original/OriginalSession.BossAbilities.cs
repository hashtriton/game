using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossCharge
        {
            internal int actor, owner;
            internal double next;
            internal OriginalBossChargeRules rule;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly Dictionary<int, BossCharge> bossCharges = new Dictionary<int, BossCharge>();
        readonly HashSet<int> bossSpecialPhases = new HashSet<int>();

        // ARCHH2, cache e580c4a53e2a87d018d8850c79d8fbd25b905ac556e6cfd2790c64b8fe1e8a4e:
        // A0LH zeros NORMAL/NORMAL and NORMAL/MAGIC. CHAOS/NORMAL and
        // CHAOS/UNIVERSAL are unchanged. This is not general invulnerability.
        double BossIncomingTriggeredDamage(OriginalWorldUnitView target, OriginalTriggeredDamageMode mode, double damage) =>
            bossSpecialPhases.Contains(target.entityId) &&
            (mode == OriginalTriggeredDamageMode.SpellNormal || mode == OriginalTriggeredDamageMode.SpellMagic) ? 0 : damage;

        // IDv source effect entry. Native A101 order/cost/latency is a separate
        // BOSSCAST1 observation; this method cannot be invoked by client input.
        void BeginBossCharge(int actorId)
        {
            var actor = world.UnitState(actorId);
            if (actor == null || bossCharges.ContainsKey(actorId) || !TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic))
                throw new InvalidOperationException("Boss charge actor unavailable.");
            CancelBossCastOnOrder(actorId);
            var target = new OriginalPoint(0, -2700);
            foreach (var unit in world.Snapshot().units)
                if (unit.kind == OriginalWorldUnitKind.Hero && unit.health > .405 && AreEnemies(actor.ownerSlot, unit.ownerSlot) &&
                    !CasterHasAbility(unit, "A0K4") && SquaredDistance(actor.position, unit.position) <= 2200 * 2200)
                    target = unit.position;
            world.SetUnitState(actorId, paused: true);
            world.SetPathingEnabled(actorId, false);
            bossCharges.Add(actorId, new BossCharge { actor = actorId, owner = actor.ownerSlot,
                rule = new OriginalBossChargeRules(target, actor.facingDegrees), next = world.Clock + OriginalBossChargeRules.TelegraphPeriod });
        }

        void AdvanceBossAbilities()
        {
            AdvanceBossNativeCasts();
            AdvanceBossBanishes();
            AdvanceBossShields();
            AdvanceBossBindings();
            AdvanceBossDeathFinger();
            AdvanceBossStorms();
            AdvanceBossBursts();
            AdvanceBossDooms();
            AdvanceBossWards();
            AdvanceBossSilences();
            AdvanceBossShockwaves();
            AdvanceBossBarrage();
            AdvanceBossRifts();
            AdvanceBossMeteors();
            AdvanceBossInfernos();
            AdvanceBossGhosts();
            AdvanceBossRain();
            AdvanceBossWind();
            AdvanceBossQuadrants();
            foreach (var charge in new List<BossCharge>(bossCharges.Values))
            {
                while (charge.next <= world.Clock + 1e-9 && !charge.rule.Completed)
                {
                    var actor = world.UnitState(charge.actor);
                    if (actor == null) { bossCharges.Remove(charge.actor); break; }
                    if (!charge.rule.Charging)
                    {
                        charge.rule.TickTelegraph(actor.position, actor.health >= .405);
                        world.SetFacing(actor.entityId, charge.rule.Facing);
                        charge.next += charge.rule.Charging ? OriginalBossChargeRules.ChargePeriod : OriginalBossChargeRules.TelegraphPeriod;
                        continue;
                    }
                    charge.next += OriginalBossChargeRules.ChargePeriod;
                    if (charge.rule.TickCharge(actor.position, actor.facingDegrees, actor.health >= .405,
                        bossSpecialPhases.Contains(actor.entityId), out var next))
                    {
                        world.ForcePosition(actor.entityId, next);
                        foreach (var target in world.Snapshot().units)
                        {
                            if (target.health <= .405 || !AreEnemies(charge.owner, target.ownerSlot) || charge.victims.Contains(target.entityId) ||
                                CasterHasType(target, "structure") || CasterMagicImmune(target) || SquaredDistance(next, target.position) > 200 * 200) continue;
                            charge.victims.Add(target.entityId);
                            ApplyTriggeredHit(actor.entityId, charge.owner, target, OriginalBossChargeRules.Damage, OriginalTriggeredDamageMode.SpellMagic);
                        }
                    }
                    if (charge.rule.Completed)
                    {
                        bossCharges.Remove(charge.actor);
                        world.SetPathingEnabled(charge.actor, true); world.SetUnitState(charge.actor, paused: false);
                    }
                }
            }
        }
    }
}

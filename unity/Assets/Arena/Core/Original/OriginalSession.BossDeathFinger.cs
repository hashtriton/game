using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossDeathFingerBeam { internal int actor; internal OriginalPoint start, end; internal double expires; }
        readonly List<BossDeathFingerBeam> bossDeathFingerBeams = new List<BossDeathFingerBeam>();
        readonly Dictionary<int, double> bossDeathFingerCooldowns = new Dictionary<int, double>();

        bool TryStartBossDeathFingerCast(int actorId, int targetId)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(actorId) ||
                target.health <= .405 || target.hidden || target.invulnerable || CasterMagicImmune(target) || CasterHasType(target, "mechanical") ||
                CasterHasType(target, "structure") || !AreEnemies(actor.ownerSlot, target.ownerSlot) || actor.mana < 175 ||
                !HasEffectiveUnitAbility(actor, "A1D6") || SquaredDistance(actor.position, target.position) > 800 * 800 ||
                bossDeathFingerCooldowns.TryGetValue(actorId, out var until) && until > world.Clock + 1e-9) return false;
            // BOSS25UNLOCK1 native EFFECT+.3s, cost175. Its .01s Acri buff
            // has no measured mechanical effect; no stock slow is invented.
            OnAcceptedWorldOrder(actorId); world.Stop(actorId); world.MarkCast(actorId);
            if (weaponCycles.TryGetValue(actorId, out var cycle)) cycle.winding = false;
            bossCasts[actorId] = new BossNativeCast { actor = actorId, ability = "A1D6", targetEntity = targetId, effectAt = world.Clock + .3 };
            return true;
        }
        void ResolveBossDeathFinger(int actorId, int targetId)
        {
            var actor = world.UnitState(actorId); var target = world.UnitState(targetId);
            if (actor == null || target == null) return;
            // aIv27911:20% MAX_LIFE, hL(mode3), fixed lightning endpoints .6s.
            bossDeathFingerBeams.Add(new BossDeathFingerBeam { actor = actorId, start = actor.position,
                end = target.position, expires = world.Clock + .6 });
            ApplyTriggeredHit(actorId, actor.ownerSlot, target, target.profile.maxHealth * .2, OriginalTriggeredDamageMode.ChaosUniversal);
        }
        void SelectBossDeathFinger(OriginalWorldUnitView actor)
        {
            // EBv issues to every eligible unit. An accepted later order
            // replaces the pending native cast; stable host order is derived.
            foreach (var target in world.Snapshot().units)
                if (target.health > .405 && AreEnemies(actor.ownerSlot, target.ownerSlot) &&
                    !CasterHasAbility(target, "A0K4") && SquaredDistance(actor.position, target.position) <= 800 * 800)
                    TryStartBossDeathFingerCast(actor.entityId, target.entityId);
        }
        void AdvanceBossDeathFinger() => bossDeathFingerBeams.RemoveAll(beam => beam.expires <= world.Clock + 1e-9);
        void AppendBossDeathFingerVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var beam in bossDeathFingerBeams)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Beam, abilityId = "A1D6", sourceEntityId = beam.actor,
                    position = beam.start, end = beam.end, radius = 12, progress = 1 });
        }
    }
}

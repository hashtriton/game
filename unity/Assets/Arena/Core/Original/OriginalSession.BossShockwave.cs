using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossShockwave
        {
            internal int actor, owner, steps;
            internal double due, angle;
            internal OriginalPoint point;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly List<BossShockwave> bossShockwaves = new List<BossShockwave>();
        readonly Dictionary<int, double> bossShockwaveCooldowns = new Dictionary<int, double>();

        void BeginBossShockwave(int id)
        {
            var actor = world.UnitState(id);
            double angle = actor.facingDegrees * .0174532;
            bossShockwaves.Add(new BossShockwave { actor = id, owner = actor.ownerSlot, due = world.Clock + .04,
                angle = actor.facingDegrees * Math.PI / 180,
                point = new OriginalPoint(actor.position.x + 50 * Math.Cos(angle), actor.position.y + 50 * Math.Sin(angle)) });
        }
        bool TryStartBossShockwave(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) ||
                !HasEffectiveUnitAbility(actor, "A1D5") || actor.mana < 150 ||
                bossShockwaveCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            // BOSSCAST1: Absk emits all native spell stages synchronously.
            // It has no interruptible windup; the committed projectile is separate.
            if (!world.TrySpendMana(id, 150)) return false;
            bossShockwaveCooldowns[id] = world.Clock + 22;
            world.MarkCast(id); BeginBossShockwave(id); return true;
        }
        void AdvanceBossShockwaves()
        {
            foreach (var wave in new List<BossShockwave>(bossShockwaves))
                while (wave.due <= world.Clock + 1e-9)
                {
                    wave.due += .04;
                    wave.point = new OriginalPoint(wave.point.x + 30 * Math.Cos(wave.angle), wave.point.y + 30 * Math.Sin(wave.angle));
                    foreach (var target in world.Snapshot().units)
                    {
                        // aAv278... moves before sweeping; an invulnerable target
                        // is remembered too, although its eventual hit is rejected.
                        if (target.health <= .405 || !AreEnemies(wave.owner, target.ownerSlot) || CasterHasType(target, "structure") ||
                            wave.victims.Contains(target.entityId) || SquaredDistance(wave.point, target.position) > 140 * 140) continue;
                        wave.victims.Add(target.entityId);
                        ApplyTriggeredHit(wave.actor, wave.owner, target, 1000, OriginalTriggeredDamageMode.SpellMagic);
                    }
                    if (++wave.steps >= 30)
                    {
                        // aAv uses RemoveUnit, so this helper does not run CA.
                        bossShockwaves.Remove(wave); break;
                    }
                }
        }
        void AppendBossShockwaveVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var wave in bossShockwaves)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "A1D5", sourceEntityId = wave.actor,
                    position = wave.point, end = new OriginalPoint(wave.point.x + 90 * Math.Cos(wave.angle), wave.point.y + 90 * Math.Sin(wave.angle)),
                    radius = 140, progress = wave.steps / 30d });
        }
    }
}

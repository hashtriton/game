using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossBurstWarning { internal int actor, owner; internal double due; internal float remaining = 1.5f; }
        sealed class BossBurst
        {
            internal int actor, owner, steps;
            internal double due;
            internal readonly OriginalPoint[] points = new OriginalPoint[6];
            internal readonly double[] angles = new double[6];
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly List<BossBurstWarning> bossBurstWarnings = new List<BossBurstWarning>();
        readonly List<BossBurst> bossBursts = new List<BossBurst>();
        readonly Dictionary<int, double> bossBurstCooldowns = new Dictionary<int, double>();

        bool TryStartBossBurst(int id)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) ||
                !HasEffectiveUnitAbility(actor, "A11F") || actor.mana < 100 ||
                bossBurstCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            // BOSSCAST1: Absk emits its native stages synchronously. A later
            // accepted order does not cancel the committed aZv warning timer.
            if (!world.TrySpendMana(id, 100)) return false;
            bossBurstCooldowns[id] = world.Clock + 14;
            world.MarkCast(id);
            bossBurstWarnings.Add(new BossBurstWarning { actor = id, owner = actor.ownerSlot, due = world.Clock + .04 });
            return true;
        }
        void BeginBossBurst(int id, int owner, double due)
        {
            var actor = world.UnitState(id); if (actor == null) return;
            var burst = new BossBurst { actor = id, owner = owner, due = due + .04 };
            for (int i = 6; i >= 1; i--)
            {
                // azv uses absolute directions for the initial30WC offsets,
                // then adds the boss facing only to projectile headings.
                double positionAngle = i * 60 * .0174532;
                burst.points[i - 1] = new OriginalPoint(actor.position.x + 30 * Math.Cos(positionAngle), actor.position.y + 30 * Math.Sin(positionAngle));
                burst.angles[i - 1] = (i * 60 + actor.facingDegrees) * Math.PI / 180;
            }
            bossBursts.Add(burst);
        }
        void AdvanceBossBursts()
        {
            foreach (var warning in new List<BossBurstWarning>(bossBurstWarnings))
                while (warning.due <= world.Clock + 1e-9)
                {
                    double due = warning.due; warning.due += .04;
                    if (warning.remaining > 0) { warning.remaining -= .04f; continue; }
                    bossBurstWarnings.Remove(warning); BeginBossBurst(warning.actor, warning.owner, due); break;
                }
            foreach (var burst in new List<BossBurst>(bossBursts))
                while (burst.due <= world.Clock + 1e-9)
                {
                    burst.due += .04;
                    for (int i = 5; i >= 0; i--)
                    {
                        var point = burst.points[i]; double angle = burst.angles[i];
                        point = burst.points[i] = new OriginalPoint(point.x + 15 * Math.Cos(angle), point.y + 15 * Math.Sin(angle));
                        foreach (var target in world.Snapshot().units)
                        {
                            if (target.health <= .405 || !AreEnemies(burst.owner, target.ownerSlot) || CasterHasType(target, "structure") ||
                                CasterMagicImmune(target) || burst.victims.Contains(target.entityId) || SquaredDistance(point, target.position) > 90 * 90) continue;
                            burst.victims.Add(target.entityId);
                            ApplyTriggeredHit(burst.actor, burst.owner, target, 600, OriginalTriggeredDamageMode.SpellMagic);
                            var actor = world.UnitState(burst.actor);
                            // aYv heals a fixed300 even when an invulnerable
                            // eligible victim rejects the preceding damage.
                            if (actor != null && actor.health > .405)
                                world.UpdateProfile(actor.entityId, actor.profile, Math.Min(actor.profile.maxHealth, actor.health + 300), actor.mana);
                            QueueBossDoom(burst.owner, target.entityId);
                        }
                    }
                    if (++burst.steps < 20) continue;
                    // Six KillUnit calls notify CA; RemoveUnit(h011) does not.
                    for (int i = 0; i < 6; i++) ObserveScriptedHelperDeath();
                    bossBursts.Remove(burst); break;
                }
        }
        void AppendBossBurstVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var warning in bossBurstWarnings)
            {
                var actor = world.UnitState(warning.actor); if (actor == null) continue;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A11F",
                    sourceEntityId = warning.actor, position = actor.position, radius = 420, progress = 1 - Math.Max(0, warning.remaining) / 1.5 });
            }
            foreach (var burst in bossBursts)
                for (int i = 0; i < 6; i++)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "A11F", sourceEntityId = burst.actor,
                        position = burst.points[i], end = new OriginalPoint(burst.points[i].x + 60 * Math.Cos(burst.angles[i]),
                            burst.points[i].y + 60 * Math.Sin(burst.angles[i])), radius = 90, progress = burst.steps / 20d });
        }
    }
}

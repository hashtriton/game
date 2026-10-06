using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class OrnWarning
        {
            internal OriginalPoint position;
            internal double heading, due;
        }
        sealed class OrnGhost
        {
            internal OriginalOrnGhostRules rule;
            internal double due;
            internal readonly HashSet<int> victims = new HashSet<int>();
        }
        readonly List<OrnWarning> ornWarnings = new List<OrnWarning>();
        readonly List<OrnGhost> ornGhosts = new List<OrnGhost>();
        bool ornPortalsEnabled;
        double ornEnabledAt, nextOrnPortal = OriginalOrnGhostRules.PortalPeriod;
        uint ornRandom;

        void ObserveScriptedHelperDeath()
        {
            // CA listens to native unit deaths for every owner. A logical
            // warning KillUnit must notify it before the callback continues,
            // without inventing a hero killer, bounty or counted wave identity.
            match.ObserveUncountedEnemyDeath();
            CollectEvents();
        }

        void OnOrnMatchEvent(OriginalMatchEvent item)
        {
            if (item.sourceRule != "final-phases") return;
            if (item.kind == OriginalMatchEventKind.BossPause)
            {
                ornPortalsEnabled = true;
                ornEnabledAt = Math.Max(world.Clock, worldStepEndsAt);
            }
            else if (item.kind == OriginalMatchEventKind.BossResume) ornPortalsEnabled = false;
            // BZ removes the portals after CA is disabled. It does not cancel
            // already scheduled Vfv timers or existing moving ghosts.
        }

        int NextOrnPortal()
        {
            if (ornRandom == 0) ornRandom = unchecked((uint)seed) ^ 0x51F15EEDu;
            if (ornRandom == 0) ornRandom = 1;
            ornRandom ^= ornRandom << 13; ornRandom ^= ornRandom >> 17; ornRandom ^= ornRandom << 5;
            return 1 + (int)(ornRandom / 4294967296.0 * 12);
        }

        void AdvanceOrnGhosts()
        {
            // VA keeps its global periodic phase while disabled. The host
            // simulation clock starts with the match; enabling a phase never resets it.
            // Merge due callbacks so catch-up preserves warning-death-before-birth.
            for (int callbacks = 0; ; callbacks++)
            {
                OrnWarning warning = null; OrnGhost ghost = null;
                double next = nextOrnPortal;
                foreach (var value in ornWarnings)
                    if (value.due < next) { next = value.due; warning = value; }
                foreach (var value in ornGhosts)
                    if (value.due < next) { next = value.due; warning = null; ghost = value; }
                if (next > world.Clock + 1e-9) return;
                if (callbacks >= 4096) throw new InvalidOperationException("orn-ghost-callback-budget-exceeded");
                if (warning != null)
                {
                    ornWarnings.Remove(warning);
                    // A native KillUnit(h04R) raises the all-player CA listener.
                    // It has no hero killer, bounty or counted wave identity.
                    ObserveScriptedHelperDeath();
                    ornGhosts.Add(new OrnGhost { rule = new OriginalOrnGhostRules(warning.position, warning.heading),
                        due = next + OriginalOrnGhostRules.Period });
                }
                else if (ghost != null)
                {
                    ghost.due += OriginalOrnGhostRules.Period;
                    if (ghost.rule.Tick(out var center))
                        foreach (var target in world.Snapshot().units)
                        {
                            if (target.health <= .405 || !AreEnemies(0, target.ownerSlot) || ghost.victims.Contains(target.entityId) ||
                                CasterMagicImmune(target) || CasterHasType(target, "structure") ||
                                SquaredDistance(center, target.position) > OriginalOrnGhostRules.Radius * OriginalOrnGhostRules.Radius) continue;
                            ghost.victims.Add(target.entityId);
                            // Attribution0 denotes the P11 helper, not a null
                            // native UnitDamageTarget source and not Orn's hero.
                            ApplyTriggeredHit(0, 0, target, target.profile.maxHealth * ghost.rule.HealthFraction,
                                OriginalTriggeredDamageMode.SpellMagic);
                        }
                    if (ghost.rule.Completed) ornGhosts.Remove(ghost);
                }
                else
                {
                    nextOrnPortal += OriginalOrnGhostRules.PortalPeriod;
                    if (!ornPortalsEnabled || next + 1e-9 < ornEnabledAt) continue;
                    int portal = NextOrnPortal();
                    ornWarnings.Add(new OrnWarning { position = OriginalOrnGhostRules.Portal(portal),
                        heading = OriginalOrnGhostRules.PortalHeading(portal), due = next + OriginalOrnGhostRules.WarningSeconds });
                }
            }
        }

        void AppendOrnGhostVisuals(List<OriginalVisualEffectView> output)
        {
            // These source unit rawcodes label logical helper visuals only.
            if (ornPortalsEnabled)
                for (int i = 1; i <= 12; i++) output.Add(new OriginalVisualEffectView {
                    kind = OriginalVisualEffectKind.Orb, abilityId = "n062", position = OriginalOrnGhostRules.Portal(i), radius = 60, progress = 1 });
            foreach (var warning in ornWarnings)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "h04R",
                    position = warning.position, radius = OriginalOrnGhostRules.Radius,
                    progress = Math.Max(0, Math.Min(1, 1 - (warning.due - world.Clock) / OriginalOrnGhostRules.WarningSeconds)) });
            foreach (var ghost in ornGhosts)
            {
                double radians = ghost.rule.Heading * Math.PI / 180;
                var position = ghost.rule.Position;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Ghost, abilityId = "h016",
                    position = position, end = new OriginalPoint(position.x + 90 * Math.Cos(radians), position.y + 90 * Math.Sin(radians)),
                    radius = OriginalOrnGhostRules.Radius, progress = ghost.rule.Steps / (double)OriginalOrnGhostRules.RemovalStep });
            }
        }
    }
}

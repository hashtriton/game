using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossRiftWarning { internal int actor, owner; internal OriginalPoint point; internal double due; }
        readonly List<BossRiftWarning> bossRiftWarnings = new List<BossRiftWarning>();
        readonly Dictionary<int, List<int>> bossRiftObjects = new Dictionary<int, List<int>>();
        readonly Dictionary<int, HashSet<int>> bossRiftSlots = new Dictionary<int, HashSet<int>>();
        static readonly OriginalPoint[] BossRiftPositions = {
            new OriginalPoint(-577,-0x982), new OriginalPoint(-446,-0xA7E), new OriginalPoint(-737,-0xB7E),
            new OriginalPoint(-257,-0x840), new OriginalPoint(0,-0x8C4), new OriginalPoint(0xFE,-0x840),
            new OriginalPoint(-0xFE,-0xCBA), new OriginalPoint(0,-0xC4C), new OriginalPoint(256,-0xCBA),
            new OriginalPoint(576,-0x986), new OriginalPoint(454,-0xA7E), new OriginalPoint(576,-0xB82) };

        void BeginBossRifts(int actor, int owner, double time)
        {
            if (!bossRiftSlots.TryGetValue(actor, out var used)) { used = new HashSet<int>(); bossRiftSlots.Add(actor, used); }
            // Vrv29954: two previously unused positions, at most51 random draws.
            int count = 0;
            for (int attempt = 0; attempt <= 50 && count < 2; attempt++)
            {
                int index = (int)(NextBossBarrageRandom() * 12);
                if (!used.Add(index)) continue;
                count++;
                bossRiftWarnings.Add(new BossRiftWarning { actor = actor, owner = owner, point = BossRiftPositions[index], due = time + 2 });
            }
        }
        void ClearBossRifts(int actor)
        {
            if (!bossRiftObjects.TryGetValue(actor, out var objects)) return;
            foreach (int id in objects)
                if (!world.RemoveDynamicDoodad(id)) throw new InvalidOperationException("boss-rift-removal-rejected:" + id);
            bossRiftObjects.Remove(actor);
            // hz removes existing destructables. It does not reset rD slots or
            // cancel Vov timers whose warnings have already been created.
        }
        void AdvanceBossRifts()
        {
            foreach (var warning in new List<BossRiftWarning>(bossRiftWarnings))
            {
                if (warning.due > world.Clock + 1e-9) continue;
                var actor = world.UnitState(warning.actor);
                // Vov uses global AD. Boss death clears AD, so the later
                // callback may still create its destructable but cannot hurt.
                if (actor != null && actor.health > .405)
                    foreach (var target in world.Snapshot().units)
                        if (target.health > .405 && AreEnemies(warning.owner, target.ownerSlot) &&
                            SquaredDistance(warning.point, target.position) <= 250 * 250)
                            ApplyTriggeredHit(warning.actor, warning.owner, target, 1500, OriginalTriggeredDamageMode.ChaosUniversal);
                int id = world.AddDynamicDoodad("B009", warning.point, 9999, true, 0, 1.2);
                if (!bossRiftObjects.TryGetValue(warning.actor, out var objects)) { objects = new List<int>(); bossRiftObjects.Add(warning.actor, objects); }
                objects.Add(id); bossRiftWarnings.Remove(warning);
            }
        }
        void AppendBossRiftVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var warning in bossRiftWarnings)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "B009",
                    sourceEntityId = warning.actor, position = warning.point, radius = 250,
                    progress = Math.Max(0, Math.Min(1, 1 - (warning.due - world.Clock) / 2)) });
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossRain
        {
            internal int actor, owner, impacts;
            internal double due;
            internal bool warning = true;
            internal OriginalPoint point;
        }
        sealed class BossRainBurn { internal int target, owner; internal double due, expires; }
        readonly List<BossRain> bossRains = new List<BossRain>();
        readonly Dictionary<int, BossRainBurn> bossRainBurns = new Dictionary<int, BossRainBurn>();
        readonly Dictionary<int, double> bossRainCooldowns = new Dictionary<int, double>();

        void BeginBossRain(int actor, int owner, OriginalPoint point) => bossRains.Add(new BossRain {
            actor = actor, owner = owner, point = point, due = world.Clock + 1.5 });

        void ResolveBossRainImpact(BossRain rain, double due)
        {
            var targets = new List<OriginalWorldUnitView>();
            foreach (var target in world.Snapshot().units)
            {
                // Native BOSSHELP3 hits hfoo at310, RCOUNT2 misses at350/400.
                // Radius plus body is a host frontier reconstruction consistent
                // with those controls, not a measured exact324WC boundary.
                double range = 300 + target.profile.collisionRadius;
                if (target.health > .405 && !target.hidden && !target.invulnerable && AreEnemies(rain.owner, target.ownerSlot) &&
                    !CasterMagicImmune(target) && SquaredDistance(rain.point, target.position) <= range * range) targets.Add(target);
            }
            // RCOUNT2 cache f8fcaa62c262b9cfcf98bebb8425ffd1a4372de1c12d04264adf54c852a912df:
            // one/two/four targets receive1/.5/.25 respectively, six impacts.
            // BOSSHELP3 independently measured1/3 for three targets.
            foreach (var target in targets)
            {
                ApplyTriggeredHit(0, rain.owner, target, 1d / targets.Count, OriginalTriggeredDamageMode.SpellMagic);
                if (!bossRainBurns.TryGetValue(target.entityId, out var burn))
                    bossRainBurns[target.entityId] = burn = new BossRainBurn { target = target.entityId, due = due + .01 };
                burn.owner = rain.owner; burn.expires = due + 3;
            }
        }
        void AdvanceBossRain()
        {
            for (int callbacks = 0; ; callbacks++)
            {
                BossRain rain = null; BossRainBurn burn = null; double due = double.PositiveInfinity;
                foreach (var value in bossRains) if (value.due < due) { rain = value; due = value.due; }
                foreach (var value in bossRainBurns.Values) if (value.due < due) { rain = null; burn = value; due = value.due; }
                if (due > world.Clock + 1e-9) return;
                if (callbacks >= 4096) throw new InvalidOperationException("boss-rain-callback-budget-exceeded");
                if (burn != null)
                {
                    var target = world.UnitState(burn.target);
                    if (due >= burn.expires || target == null || target.health <= .405)
                    { bossRainBurns.Remove(burn.target); continue; }
                    burn.due += 1;
                    ApplyTriggeredHit(0, burn.owner, target, 50, OriginalTriggeredDamageMode.SpellMagic);
                }
                else if (rain.warning)
                {
                    // RZ removes its warning with KillUnit before ordering the
                    // separate helper; the helper itself is only RemoveUnit at15s.
                    ObserveScriptedHelperDeath(); rain.warning = false; rain.due += .9;
                }
                else
                {
                    ResolveBossRainImpact(rain, due);
                    if (++rain.impacts == 6) bossRains.Remove(rain);
                    else rain.due += 1;
                }
            }
        }
        bool TryStartBossRainCast(int id, OriginalPoint target)
        {
            var actor = world.UnitState(id);
            if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(id) || actor.mana < 200 ||
                !HasEffectiveUnitAbility(actor, "A04V") || SquaredDistance(actor.position, target) > 800 * 800 ||
                bossRainCooldowns.TryGetValue(id, out var until) && until > world.Clock + 1e-9) return false;
            OnAcceptedWorldOrder(id); world.Stop(id); world.MarkCast(id);
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            bossCasts[id] = new BossNativeCast { actor = id, ability = "A04V", target = target, effectAt = world.Clock + .5 };
            return true;
        }
        void AppendBossRainVisuals(List<OriginalVisualEffectView> output)
        {
            foreach (var rain in bossRains)
                output.Add(new OriginalVisualEffectView { kind = rain.warning ? OriginalVisualEffectKind.WarningCircle : OriginalVisualEffectKind.ActiveCircle,
                    abilityId = "A0QR", sourceEntityId = rain.actor, position = rain.point, radius = 300,
                    progress = rain.warning ? Math.Max(0, Math.Min(1, 1 - (rain.due - world.Clock) / 1.5)) : 1 });
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class BossManaAura { internal double acquireAt, expires; internal bool active; }
        readonly Dictionary<int, BossManaAura> bossManaAuras = new Dictionary<int, BossManaAura>();

        void AdvanceBossManaAura(double seconds)
        {
            var units = world.Snapshot().units;
            var eligible = new HashSet<int>();
            foreach (var source in units)
            {
                if (source.health <= .405 || source.hidden || !HasEffectiveUnitAbility(source, "A1D8")) continue;
                foreach (var target in units)
                    if (target.health > .405 && !target.hidden && target.profile.maxMana > 0 &&
                        AreEnemies(source.ownerSlot, target.ownerSlot) && SquaredDistance(source.position, target.position) <= 1000 * 1000)
                        eligible.Add(target.entityId);
            }
            foreach (int target in eligible)
            {
                if (!bossManaAuras.TryGetValue(target, out var state))
                    bossManaAuras.Add(target, state = new BossManaAura { acquireAt = world.Clock - seconds + .6 });
                if (state.acquireAt <= world.Clock + 1e-9) state.active = true;
                if (state.active) state.expires = world.Clock + 3.1;
            }
            foreach (int target in new List<int>(bossManaAuras.Keys))
            {
                var actor = world.UnitState(target); var state = bossManaAuras[target];
                if (actor == null || actor.health <= .405 || !eligible.Contains(target) &&
                    (!state.active || state.expires <= world.Clock + 1e-9)) bossManaAuras.Remove(target);
            }
            // MANAAURA2: all three near cases first show B0CM by+.6s and
            // lose it by+3.1s after removal (samples spaced.1s). These host
            // deadlines reconstruct that bracket, not native global aura phase.
            // Dead/hidden emitter handling and coexisting emitters were not
            // measured. Host uses one B0CM contribution, never additive copies.
        }
        double BossManaAuraRate(OriginalWorldUnitView target) => bossManaAuras.TryGetValue(target.entityId, out var state) && state.active
            ? -.02 * target.profile.maxMana : 0;
    }
}

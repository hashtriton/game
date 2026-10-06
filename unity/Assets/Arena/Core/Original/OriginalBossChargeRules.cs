using System;

namespace Arena.Original
{
    // LiA3.9c IDv/Idv/ICv, JASS36162-36277. Cast lifecycle and damage
    // eligibility are session responsibilities; this reconstructs timer motion.
    public sealed class OriginalBossChargeRules
    {
        public const double TelegraphPeriod = .2, ChargePeriod = .03;
        public const double Damage = 300, Radius = 200;
        public const int TelegraphTicks = 9, MovementTicks = 28;
        int telegraphTicks, chargeTicks;
        public bool Charging => telegraphTicks >= TelegraphTicks;
        public bool Completed { get; private set; }
        public double Facing { get; private set; }
        public readonly OriginalPoint target;

        public OriginalBossChargeRules(OriginalPoint target, double facing)
        {
            if (!OriginalCombatDefinition.IsFinite(target.x) || !OriginalCombatDefinition.IsFinite(target.y) ||
                !OriginalCombatDefinition.IsFinite(facing)) throw new ArgumentOutOfRangeException();
            this.target = target; Facing = facing;
        }

        public void TickTelegraph(OriginalPoint caster, bool alive)
        {
            if (Completed || Charging) return;
            // Idv aims eight times; the ninth callback starts the .03 timer.
            if (!alive) { telegraphTicks = TelegraphTicks; return; }
            if (++telegraphTicks < TelegraphTicks)
                Facing = 57.2958 * Math.Atan2(target.y - caster.y, target.x - caster.x);
        }

        public bool TickCharge(OriginalPoint caster, double currentFacing, bool alive, bool specialPhase, out OriginalPoint next)
        {
            next = caster;
            if (!Charging) throw new InvalidOperationException("Charge begins after the telegraph timer.");
            if (Completed) return false;
            // GM increases before the >=600 check, so 28 steps travel 588;
            // callback29 completes at virtual distance609 without movement.
            if (++chargeTicks > MovementTicks || !alive || specialPhase) { Completed = true; return false; }
            double radians = currentFacing * Math.PI / 180;
            var requested = new OriginalPoint(caster.x + 21 * Math.Cos(radians), caster.y + 21 * Math.Sin(radians));
            if (OriginalShieldBashRules.AllowsForcedPoint(requested)) next = requested;
            // Even blocked movement still performs the source damage sweep.
            return true;
        }
    }
}

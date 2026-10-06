using System;

namespace Arena.Original
{
    // S5v/S4v64551/64404: five source helpers orbit every .02, expire on
    // callback751 and launch in creation order. Literal .0174532 is retained.
    public sealed class OriginalPyroSphereOrbit
    {
        const double SourceRadians = .0174532;
        readonly OriginalPoint[] positions = new OriginalPoint[5];
        readonly bool[] available = { true, true, true, true, true };
        int ticks;
        public bool Completed { get; private set; }
        public int Remaining { get; private set; } = 5;
        public OriginalPoint[] Positions => (OriginalPoint[])positions.Clone();
        public OriginalPoint[] UnlaunchedPositions
        {
            get
            {
                var result = new OriginalPoint[Remaining]; int next = 0;
                for (int i = 0; i < available.Length; i++) if (available[i]) result[next++] = positions[i];
                return result;
            }
        }
        public OriginalPyroSphereOrbit(OriginalPoint caster)
        {
            OriginalPyroEffectRules.Point(caster);
            for (int i = 0; i < 5; i++) SetPosition(i, caster, -360.0 * (5 - i) / 5);
        }
        public void Tick(OriginalPoint caster, bool dead)
        {
            OriginalPyroEffectRules.Point(caster);
            if (Completed) return;
            ticks++;
            for (int i = 0; i < 5; i++) if (available[i]) SetPosition(i, caster, 360.0 * (5 - i) / 5 - 1.44 * ticks);
            if (ticks > 750 || dead) { Completed = true; Remaining = 0; Array.Clear(available, 0, available.Length); }
        }
        public bool TryLaunch(out OriginalPoint position)
        {
            position = default;
            if (Completed) return false;
            for (int i = 0; i < 5; i++)
            {
                if (!available[i]) continue;
                available[i] = false; Remaining--; position = positions[i];
                if (Remaining == 0) Completed = true;
                return true;
            }
            return false;
        }
        void SetPosition(int index, OriginalPoint caster, double degrees) => positions[index] =
            new OriginalPoint(caster.x + 250 * Math.Cos(degrees * SourceRadians), caster.y + 250 * Math.Sin(degrees * SourceRadians));
    }

    // S2v/Szv64337..64403 uses the same eighteen-unit advance for periodic
    // callbacks, caster SPELL_EFFECT and target DEATH. A caller must invoke an
    // extra step for those events; this is not elapsed-time-only homing.
    public sealed class OriginalPyroFlyingSphere
    {
        OriginalPoint lastTarget;
        public OriginalPoint Position { get; private set; }
        public bool HasLiveTarget { get; private set; }
        public bool Completed { get; private set; }
        public OriginalPyroFlyingSphere(OriginalPoint start, OriginalPoint target, bool hasLiveTarget)
        {
            OriginalPyroEffectRules.Point(start); OriginalPyroEffectRules.Point(target);
            Position = start; lastTarget = target; HasLiveTarget = hasLiveTarget;
        }
        public bool Step(OriginalPoint? currentTarget = null, bool targetDied = false)
        {
            if (currentTarget.HasValue) OriginalPyroEffectRules.Point(currentTarget.Value);
            if (Completed) return false;
            if (HasLiveTarget && currentTarget.HasValue) lastTarget = currentTarget.Value;
            if (targetDied) HasLiveTarget = false;
            double degrees = Math.Atan2(lastTarget.y - Position.y, lastTarget.x - Position.x) * 180 / Math.PI;
            double angle = degrees * .0174532;
            Position = new OriginalPoint(Position.x + 18 * Math.Cos(angle), Position.y + 18 * Math.Sin(angle));
            double dx = Position.x - lastTarget.x, dy = Position.y - lastTarget.y;
            Completed = dx * dx + dy * dy < 36 * 36;
            return Completed;
        }
    }
}

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        void SyncCocoonAccelerators()
        {
            if (world == null || match == null || match.Round != 23) return;
            OriginalWorldSnapshot snapshot = null;
            foreach (var enemy in match.Enemies)
            {
                if (!enemy.cocoon) continue;
                var cocoon = world.UnitState(OriginalWorld.EnemyEntityId(enemy.entityId));
                if (cocoon == null) continue;
                if (snapshot == null) snapshot = world.Snapshot();
                int count = 0;
                // EKv 31640-31655: includes hidden allies, excludes life <= .405.
                // Evaluate current actor positions before the one-second timer.
                foreach (var unit in snapshot.units)
                {
                    if (unit.health <= .405 || AreEnemies(cocoon.ownerSlot, unit.ownerSlot) ||
                        unit.rawcode != "n067" && unit.rawcode != "n068") continue;
                    double dx = unit.position.x - cocoon.position.x, dy = unit.position.y - cocoon.position.y;
                    if (dx * dx + dy * dy <= 325 * 325) count++;
                }
                match.SetCocoonAccelerators(enemy.entityId, count);
            }
        }
    }
}

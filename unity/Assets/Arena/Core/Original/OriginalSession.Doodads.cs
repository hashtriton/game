using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        int destroyedExplosiveBarrels;
        // Q6/q6, normalized source 20941..20965. LTex calls KillUnit on
        // nearby Player11 units; it is not an amount of physical explosion
        // damage. Mega bosses have userData2 (30549/30564) and are excluded.
        void ProcessWorldEvents()
        {
            OriginalWorldEvent[] batch;
            while ((batch = world.DrainEvents()).Length != 0)
                foreach (var item in batch)
                {
                    if (item.kind == OriginalWorldEventKind.UnitDied) ObserveWaveTraitDeath(world.UnitState(item.entityId), 0);
                    ObserveScriptedEnemyWorldEvent(item);
                    ObserveItemSummonWorldEvent(item);
                    if (item.kind == OriginalWorldEventKind.UnitDied) ObserveImageDeath(item.entityId);
                    if (item.kind == OriginalWorldEventKind.UnitDied) ObserveItemScriptActDeath(item.entityId);
                    if (item.kind == OriginalWorldEventKind.UnitDied && item.rawcode == "n0AW") ClearBossRifts(item.entityId);
                    if (item.kind == OriginalWorldEventKind.UnitDied || item.kind == OriginalWorldEventKind.UnitRemoved) ForgetActorControls(item.entityId);
                    if (item.kind != OriginalWorldEventKind.DoodadDestroyed || item.rawcode != "LTex" || !options.explosiveBarrels) continue;
                    var snapshot = world.Snapshot();
                    var barrel = Array.Find(snapshot.doodads, d => d.editorId == item.editorId);
                    if (barrel == null) continue;
                    destroyedExplosiveBarrels = checked(destroyedExplosiveBarrels + 1); // ma, Q6:20951; rating penalty 18604..18605.
                    foreach (var unit in snapshot.units)
                    {
                        if (unit.ownerSlot != 0 || unit.health <= 0 || SquaredDistance(unit.position, barrel.position) > 500 * 500) continue;
                        OriginalMatchEnemy enemy = null;
                        foreach (var candidate in match.Enemies)
                            if (OriginalWorld.EnemyEntityId(candidate.entityId) == unit.entityId) { enemy = candidate; break; }
                        if (SourceUnitUserData(unit.entityId) == 2 || HasEffectiveUnitAbility(unit, "A0K4")) continue;
                        if (!CanMutateHost()) return;
                        if (enemy == null)
                        {
                            if (world.ForceUnitDeath(unit.entityId))
                            { OnPyroUnitDied(unit.entityId); OnScriptedEnemyDied(unit.entityId, 0); }
                            continue;
                        }
                        match.SetEnemyPosition(enemy.entityId, (float)unit.position.x, (float)unit.position.y);
                        // A0VK suppresses the source soul-drop handler. LiAKill1
                        // native probe: KillUnit has no killing unit, even after
                        // a hero's earlier hit; the custom allied XP gate is false.
                        if (world.ForceUnitDeath(unit.entityId) && !ReportEnemyKilled(enemy.entityId, eligibleAlliedKill: false))
                        { HaltReason = "world-explosion-death-ledger-rejected:" + enemy.entityId; return; }
                    }
                }
        }
    }
}

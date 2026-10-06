using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class UnitAbilityOverlay
        {
            internal readonly HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            internal readonly HashSet<string> removed = new HashSet<string>(StringComparer.Ordinal);
        }
        sealed class ScriptedEnemy
        {
            internal int sourceUserData;
            internal bool deathObserved;
        }
        readonly Dictionary<int, UnitAbilityOverlay> unitAbilityOverlays = new Dictionary<int, UnitAbilityOverlay>();
        readonly Dictionary<int, ScriptedEnemy> scriptedEnemies = new Dictionary<int, ScriptedEnemy>();
        int nextScriptedEnemyId = 500000000;

        void ValidateAbilityChanges(string[] ids)
        {
            if (ids == null) return;
            foreach (string id in ids)
            {
                if (id == null || id.Length != 4) throw new ArgumentException("Invalid dynamic ability rawcode.");
                if (combatCatalog.Ability(id) == null) throw new InvalidOperationException("Unresolved dynamic ability: " + id);
            }
        }

        // Trusted source Add/Remove operations apply to a world instance, not
        // its shared catalog definition. Normal Match enemies also use this.
        void ApplyUnitAbilityOverlay(int worldId, string[] added, string[] removed)
        {
            if (world?.UnitState(worldId) == null) throw new ArgumentException("Missing ability overlay actor.");
            ValidateAbilityChanges(added); ValidateAbilityChanges(removed);
            var candidate = new UnitAbilityOverlay();
            if (unitAbilityOverlays.TryGetValue(worldId, out var previous))
            { candidate.added.UnionWith(previous.added); candidate.removed.UnionWith(previous.removed); }
            foreach (var id in added ?? Array.Empty<string>())
            { candidate.removed.Remove(id); candidate.added.Add(id); }
            foreach (var id in removed ?? Array.Empty<string>())
            { candidate.added.Remove(id); candidate.removed.Add(id); }
            unitAbilityOverlays[worldId] = candidate;
        }

        IEnumerable<string> EffectiveUnitAbilityIds(OriginalWorldUnitView actor)
        {
            if (actor == null) yield break;
            // I048 copies native weapon/profile; ordinary AIil passive
            // inheritance is unmeasured. Rawcode is not an ability grant.
            if (actor.kind == OriginalWorldUnitKind.Illusion && actor.imageFactory == OriginalImageFactory.ItemWand) yield break;
            unitAbilityOverlays.TryGetValue(actor.entityId, out var changes);
            var emitted = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in (combatCatalog.Unit(actor.rawcode).Text("abilList") ?? "").Split(','))
                if (id.Length == 4 && (changes == null || !changes.removed.Contains(id)) && emitted.Add(id)) yield return id;
            if (changes == null) yield break;
            var additions = new List<string>(changes.added); additions.Sort(StringComparer.Ordinal);
            foreach (var id in additions)
                if (!changes.removed.Contains(id) && emitted.Add(id)) yield return id;
        }

        bool HasEffectiveUnitAbility(OriginalWorldUnitView actor, string id)
        {
            foreach (var ability in EffectiveUnitAbilityIds(actor)) if (ability == id) return true;
            return false;
        }

        int SourceUnitUserData(int worldId) => scriptedEnemies.TryGetValue(worldId, out var summon)
            ? summon.sourceUserData : EnemyState(worldId)?.sourceUserData ?? 0;

        OriginalWorldUnitProfile ScriptedEnemyProfile(string rawcode)
        {
            var definition = combatCatalog.Unit(rawcode);
            if (definition == null) throw new InvalidOperationException("Missing scripted unit: " + rawcode);
            // Vitality/speed require explicit declarations; collision uses the
            // separately labelled resolver. A caller may supply another verified
            // profile when sparse native vitality has been measured.
            return new OriginalWorldUnitProfile { maxHealth = definition.Number("HP"), maxMana = definition.Number("manaN"),
                moveSpeed = definition.Number("spd"), collisionRadius = OriginalUnitCollisionRules.Resolve(combatCatalog, rawcode).radius };
        }

        int SpawnScriptedEnemy(string rawcode, OriginalPoint center, int sourceUserData, string[] addedAbilities,
            string[] removedAbilities, OriginalWorldUnitProfile profile = null, double? initialMana = null)
        {
            if (world == null) throw new InvalidOperationException("Scripted unit requires a world.");
            if (sourceUserData < 0) throw new ArgumentOutOfRangeException(nameof(sourceUserData));
            var definition = combatCatalog.Unit(rawcode);
            if (definition == null) throw new InvalidOperationException("Missing scripted unit: " + rawcode);
            ValidateAbilityChanges(addedAbilities); ValidateAbilityChanges(removedAbilities);
            var candidate = (profile ?? ScriptedEnemyProfile(rawcode)).Copy();
            if (!OriginalCombatDefinition.IsFinite(candidate.maxHealth) || candidate.maxHealth <= 0 ||
                !OriginalCombatDefinition.IsFinite(candidate.maxMana) || candidate.maxMana < 0 ||
                !OriginalCombatDefinition.IsFinite(candidate.moveSpeed) || candidate.moveSpeed < 0 ||
                !OriginalCombatDefinition.IsFinite(candidate.collisionRadius) || candidate.collisionRadius <= 0)
                throw new ArgumentException("Invalid scripted unit profile.");
            double mana = initialMana ?? definition.Number("mana0");
            if (!OriginalCombatDefinition.IsFinite(mana) || mana < 0) throw new ArgumentException("Invalid scripted unit initial mana.");
            mana = Math.Min(mana, candidate.maxMana);
            if (!world.TryFindFreeSpawn(center, candidate.collisionRadius, 512, out var position))
                throw new InvalidOperationException("Scripted unit placement unavailable: " + rawcode);
            int id = nextScriptedEnemyId;
            while (id < 1000000000 && world.UnitState(id) != null) id++;
            if (id >= 1000000000) throw new InvalidOperationException("Scripted unit identity budget exceeded.");
            world.AddUnit(id, 0, rawcode, candidate, position);
            world.UpdateProfile(id, candidate, candidate.maxHealth, mana);
            ApplyUnitAbilityOverlay(id, addedAbilities, removedAbilities);
            scriptedEnemies.Add(id, new ScriptedEnemy { sourceUserData = sourceUserData });
            nextScriptedEnemyId = id + 1;
            return id;
        }

        // Logical uncounted helpers never masquerade as Match wave entries.
        // Their native bounty/XP/passive closure is explicitly separate. CA's
        // all-unit death notification still runs once for an actual death.
        bool OnScriptedEnemyDied(int worldId, int killerOwner)
        {
            if (!scriptedEnemies.TryGetValue(worldId, out var summon) || summon.deathObserved) return false;
            var unit = world?.UnitState(worldId);
            if (unit != null && unit.health > 0) return false;
            summon.deathObserved = true;
            ObserveScriptedHelperDeath();
            return true;
        }

        void ObserveScriptedEnemyWorldEvent(OriginalWorldEvent item)
        {
            if (item.kind == OriginalWorldEventKind.UnitDied) OnScriptedEnemyDied(item.entityId, 0);
            else if (item.kind == OriginalWorldEventKind.UnitRemoved)
            {
                scriptedEnemies.Remove(item.entityId); unitAbilityOverlays.Remove(item.entityId);
            }
        }
    }
}

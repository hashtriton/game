using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        IOriginalWorldNavigation navigation;
        OriginalNativeCatalog native;
        OriginalObservedCatalog observed;
        OriginalWorldDoodadView[] authoredDoodads;
        OriginalWorld world;

        // The scene adapter is supplied before hosting. Only the host owns the
        // world; remote peers receive immutable snapshots and submit intents.
        public void ConfigureWorld(IOriginalWorldNavigation value, OriginalNativeCatalog nativeCatalog,
            OriginalWorldDoodadView[] destructables, OriginalObservedCatalog observedCatalog = null)
        {
            if (Started) throw new InvalidOperationException("Configure the world before starting a match.");
            if (value == null || nativeCatalog == null || destructables == null) throw new ArgumentNullException();
            nativeCatalog.BuildIndexes();
            observedCatalog?.BuildIndexes();
            var copy = new OriginalWorldDoodadView[destructables.Length];
            var identities = new HashSet<int>();
            for (int i = 0; i < copy.Length; i++)
            {
                var d = destructables[i];
                if (d == null || d.editorId < 0 || !identities.Add(d.editorId) ||
                    d.rawcode == null || d.rawcode.Length != 4 || !ValidPoint(d.position.x, d.position.y) ||
                    !OriginalCombatDefinition.IsFinite(d.maxHealth) || d.maxHealth <= 0 ||
                    !OriginalCombatDefinition.IsFinite(d.health) || d.health < 0 || d.health > d.maxHealth)
                    throw new ArgumentException("Invalid authored destructable.");
                copy[i] = new OriginalWorldDoodadView { editorId = d.editorId, rawcode = d.rawcode,
                    position = d.position, maxHealth = d.maxHealth, health = d.health };
            }
            navigation = value; native = nativeCatalog; authoredDoodads = copy; observed = observedCatalog;
        }

        OriginalWorld CreateWorld()
        {
            if (navigation == null) return null;
            var candidate = new OriginalWorld(navigation);
            foreach (var d in authoredDoodads) candidate.AddDoodad(d.editorId, d.rawcode, d.position, d.maxHealth, d.health);
            if (options.defensiveBarrels == OriginalDefensiveBarrels.Destroyed)
                foreach (var d in authoredDoodads)
                    if (d.rawcode == "LTbr" && DefensiveBarrelRegion(d.position)) candidate.ApplyDoodadDamage(d.editorId, d.maxHealth);
            foreach (var player in players)
            {
                var stats = OriginalHeroStats.Calculate(combatCatalog, player.hero, 1);
                var profile = new OriginalWorldUnitProfile { moveSpeed = stats.baseMoveSpeed,
                    collisionRadius = combatCatalog.Unit(player.hero).Number("collision"),
                    maxHealth = stats.maxHealth.Require(), maxMana = stats.maxMana.Require() };
                if (!candidate.TryFindFreeSpawn(navigation.HeroSpawn, profile.collisionRadius, 512, out var position))
                    throw new InvalidOperationException("No valid hero placement in the source start area.");
                candidate.AddUnit(OriginalWorld.HeroEntityId(player.slot), player.slot, player.hero, profile, position);
            }
            InitializeRuneAndBarrelWorld(candidate);
            candidate.DrainEvents(); return candidate;
        }

        OriginalSessionReplyCode ApplyWorldCommand(Player player, OriginalSessionCommand command)
        {
            if (world == null || !Started || pendingDuel || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            int id = command.actorEntityId == 0 ? OriginalWorld.HeroEntityId(player.slot) : command.actorEntityId;
            var actor = world.UnitState(id);
            if (actor == null || actor.ownerSlot != player.slot ||
                actor.kind != OriginalWorldUnitKind.Hero && actor.kind != OriginalWorldUnitKind.Illusion && actor.kind != OriginalWorldUnitKind.Summon)
                return OriginalSessionReplyCode.InvalidCommand;
            if (actor.paused) return OriginalSessionReplyCode.InvalidCommand;
            if (actor.health <= 0 || actor.kind == OriginalWorldUnitKind.Hero &&
                !(DuelActive ? duel.Snapshot().alive[player.matchSlot] : match.IsAlive(player.matchSlot)))
                return OriginalSessionReplyCode.NotReady;
            bool accepted;
            OriginalPoint attackPoint = default;
            switch (command.kind)
            {
                case OriginalSessionCommandKind.Move:
                    if (!ValidPoint(command.x, command.y)) return OriginalSessionReplyCode.InvalidCommand;
                    accepted = world.TryMove(id, CasterAuraPointOrder(id, 851986, new OriginalPoint(command.x, command.y))); break;
                case OriginalSessionCommandKind.AttackMove:
                    if (!ValidPoint(command.x, command.y)) return OriginalSessionReplyCode.InvalidCommand;
                    attackPoint = CasterAuraPointOrder(id, 851983, new OriginalPoint(command.x, command.y));
                    accepted = world.TryMove(id, attackPoint); break;
                case OriginalSessionCommandKind.Stop: accepted = world.Stop(id); break;
                case OriginalSessionCommandKind.HoldPosition: accepted = world.HoldPosition(id); break;
                case OriginalSessionCommandKind.AttackTarget:
                    if (command.targetKind == OriginalWorldTargetKind.Unit)
                    {
                        var target = Array.Find(world.Snapshot().units, u => u.entityId == command.targetId);
                        if (target == null || target.invulnerable || !Enemies(actor, target) ||
                            !CanSeeForCombat(actor.ownerSlot,target)) return OriginalSessionReplyCode.InvalidCommand;
                    }
                    accepted = world.TryAttackTarget(id, command.targetKind, command.targetId); break;
                default: return OriginalSessionReplyCode.InvalidCommand;
            }
            if (accepted && weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            if (accepted)
            {
                CancelQueuedInteraction(id);
                if (command.kind == OriginalSessionCommandKind.AttackTarget) { ArmItemWindWalkStrike(id); RevealItemInvisibility(id); }
                else pendingItemWindWalkStrikes.Remove(id);
                OnAcceptedWorldOrder(id);
                if (command.kind == OriginalSessionCommandKind.AttackMove) playerAttackGoals[id] = attackPoint;
            }
            return accepted ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.InvalidCommand;
        }

        void AdvanceWorld(double seconds)
        {
            if (match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
            { queuedInteractions.Clear(); queuedCastApproaches.Clear(); return; }
            try
            {
                world.Advance(seconds, id => !ActorMoveBlocked(id));
                AdvanceQueuedInteractions();
                AdvanceRuneAndBarrelWorld();
                AdvanceRoundBonuses();
                AdvanceHealingWell();
                AdvanceWorldCurses();
                AdvanceActorControls(seconds);
                AdvanceQueuedItems();
                AdvanceItemScripts();
                AdvanceItemModes();
            AdvanceItemFortitude();
                AdvanceSoulUpgrades();
                AdvanceUniqueSoulEffects();
                AdvanceItemAuras();
                if (observed != null) AdvanceRegeneration(seconds);
                AdvanceAbilities(seconds);
                AdvanceItemArmor(seconds);
                AdvanceItemStatuses(seconds);
                AdvanceRunePowerups(seconds);
                AdvanceItemScriptActs();
                AdvanceItemChannels();
                AdvanceItemSummons(seconds);
                AdvanceItemSummonScripts();
                AdvanceSummonAbilities();
                AdvanceBossAbilities();
                AdvanceOrnAbilities();
                AdvanceOrnGhosts();
                AdvanceCasters();
                AdvanceWaveTraits(seconds);
                AdvanceWaveSpells();
                AdvanceDuelRing();
                AdvanceDuelBoundary();
                AdvanceQueuedCastApproaches();
                foreach (var unit in world.Snapshot().units)
                    if (unit.kind == OriginalWorldUnitKind.Enemy) match.SetEnemyPosition(unit.entityId - 1000, (float)unit.position.x, (float)unit.position.y);
                if (observed != null) AdvanceWorldAi();
                AdvanceDisconnectedAi();
                AdvanceItemTauntOrders();
                AdvanceWeapons();
                AdvanceRatingClock();
            }
            catch (InvalidOperationException exception) { HaltReason = "world-rule-unavailable:" + exception.Message; }
            ProcessWorldEvents();
        }

        void ApplyWorldEvent(OriginalMatchEvent item)
        {
            if (world == null) return;
            OnRuneMatchEvent(item);
            OnRoundBonusMatchEvent(item);
            OnHealingWellMatchEvent(item);
            OnWaveTraitMatchEvent(item);
            OnCasterMatchEvent(item);
            OnOrnMatchEvent(item);
            OnOrnActiveMatchEvent(item);
            if (ApplyBossWorldEvent(item)) return;
            if (item.kind == OriginalMatchEventKind.Spawn && observed != null) SpawnWorldEnemy(item);
            else if (item.kind == OriginalMatchEventKind.OrderRefresh && observed != null) RefreshWorldOrders();
            else if (item.kind == OriginalMatchEventKind.Remove)
            {
                int id = OriginalWorld.EnemyEntityId(item.entityId);
                world.RemoveUnit(id); enemyGoals.Remove(id); weaponCycles.Remove(id);
            }
            else if (item.kind == OriginalMatchEventKind.PartyPause)
            {
                foreach (var unit in world.Snapshot().units)
                    if (unit.kind == OriginalWorldUnitKind.Hero) world.SetUnitState(unit.entityId, paused: item.enabled);
            }
            else if (item.kind == OriginalMatchEventKind.TeleportParty)
            {
                var player = players.Find(p => p.matchSlot == item.slot);
                RestoreAt(OriginalWorld.HeroEntityId(player.slot), new OriginalPoint(item.x, item.y));
            }
            else if (item.kind == OriginalMatchEventKind.RestoreParty)
            {
                foreach (var unit in world.Snapshot().units)
                    if (unit.kind == OriginalWorldUnitKind.Hero)
                    { RestoreAt(unit.entityId, unit.position); RestoreUniqueSoulPathing(unit.entityId); }
            }
        }

        void SpawnWorldEnemy(OriginalMatchEvent item)
        {
            try
            {
                var measured = observed.Unit(item.rawcode);
                var definition = combatCatalog.Unit(item.rawcode);
                var profile = new OriginalWorldUnitProfile { moveSpeed = measured.RequireMoveSpeed(),
                    maxHealth = measured.RequireMaxHP(), maxMana = measured.RequireMaxMP(),
                    collisionRadius = OriginalUnitCollisionRules.Resolve(combatCatalog, definition.id).radius };
                var intrinsic = BossIntrinsic(item.rawcode);
                if (intrinsic != null)
                {
                    profile.maxHealth = intrinsic.RequireMaxHP(); profile.maxMana = intrinsic.RequireMaxMP();
                    profile.moveSpeed = intrinsic.RequireMoveSpeed();
                }
                var center = new OriginalPoint(item.x, item.y);
                if (!world.TryFindFreeSpawn(center, profile.collisionRadius, 512, out var position))
                { HaltReason = "world-spawn-placement-unavailable:" + item.rawcode; return; }
                int worldId = OriginalWorld.EnemyEntityId(item.entityId);
                world.AddUnit(worldId, 0, item.rawcode, profile, position);
                var source = EnemyState(worldId);
                if (source != null && source.cocoon) world.SetUnitState(worldId, paused: true);
                if (source != null && !source.megaBoss && !source.finalAdd && !source.cocoon &&
                    !(item.round == 23 && (item.rawcode == "n065" || item.rawcode == "n066")))
                {
                    ApplyEnemyDifficultyAbilities(world.UnitState(worldId));
                    ApplyWaveSpawnAbilities(world.UnitState(worldId));
                }
            }
            catch (InvalidOperationException) { HaltReason = "world-spawn-rule-unavailable:" + item.rawcode; }
        }

        static bool DefensiveBarrelRegion(OriginalPoint p) =>
            p.x >= 640 && p.x <= 1408 && p.y >= 2048 && p.y <= 2688 ||
            p.x >= 1600 && p.x <= 1792 && p.y >= 1664 && p.y <= 1984 ||
            p.x >= -1152 && p.x <= -896 && p.y >= -1024 && p.y <= -640;

        void RestoreAt(int id, OriginalPoint desired)
        {
            if (world.RestoreUnit(id, desired)) return;
            var unit = Array.Find(world.Snapshot().units, candidate => candidate.entityId == id);
            if (unit != null && world.TryFindFreeSpawn(desired, unit.profile.collisionRadius, 512, out var free) && world.RestoreUnit(id, free)) return;
            HaltReason = "world-restore-placement-unavailable:" + id;
        }

        static bool ValidPoint(double x, double y) => OriginalCombatDefinition.IsFinite(x) && OriginalCombatDefinition.IsFinite(y) &&
            Math.Abs(x) <= 1048576 && Math.Abs(y) <= 1048576;
    }
}


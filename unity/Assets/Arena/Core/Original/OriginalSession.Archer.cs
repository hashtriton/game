using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ArcherState
        {
            internal OriginalBowElement active, next = OriginalBowElement.Venom;
            internal int charges, combo;
            internal double cycleAt = -1, berserkUntil = -1;
            internal int berserkRank;
            internal readonly Dictionary<string, double> cooldowns = new Dictionary<string, double>(StringComparer.Ordinal);
            internal readonly List<double> volleyResets = new List<double>();
        }
        sealed class ArcherCast
        {
            internal int actor, owner, rank;
            internal string ability, nativeAbility;
            internal OriginalPoint target;
            internal double effectAt;
            internal OriginalArcherCastRules rules;
        }
        sealed class ArcherVolley
        { internal int actor, owner; internal double nextTick; internal OriginalArcherVolleyRules rules; }
        sealed class ArcherShot
        { internal int actor, owner; internal double nextTick; internal OriginalArcherPowerShotRules rules; }
        readonly Dictionary<int, ArcherState> archers = new Dictionary<int, ArcherState>();
        readonly Dictionary<int, ArcherCast> archerCasts = new Dictionary<int, ArcherCast>();
        readonly List<ArcherVolley> archerVolleys = new List<ArcherVolley>();
        readonly List<ArcherShot> archerShots = new List<ArcherShot>();
        static readonly string[] BowHelpers = { "A15Z", "A160", "A161", "A162", "A17M" };

        ArcherState Archer(int slot)
        {
            if (!archers.TryGetValue(slot, out var state)) archers.Add(slot, state = new ArcherState());
            return state;
        }
        static string BowHelper(OriginalBowElement element) => BowHelpers[(int)element - 1];
        double ArcherCooldown(int slot, string ability) => archers.TryGetValue(slot, out var state) && state.cooldowns.TryGetValue(ability, out double end)
            ? Math.Max(0, end - world.Clock) : 0;
        bool PopulateArcherAbilityView(Player player, OriginalWorldUnitView actor, OriginalAbilityView view)
        {
            if (player.hero != "N0A0" || view.id != "A0AS" && view.id != "A15W" && view.id != "A15X") return false;
            try
            {
                var state = Archer(player.slot);
                string nativeId = view.id == "A15X" ? BowHelper(state.next) : view.id;
                var rules = new OriginalArcherCastRules(combatCatalog, nativeId, Math.Max(1, view.rank));
                view.implemented = true; view.manaCostKnown = true; view.manaCost = rules.manaCost;
                view.targetMode = view.id == "A15W" ? OriginalAbilityTargetMode.Point : OriginalAbilityTargetMode.None;
                view.cooldownRemaining = world == null ? 0 : ArcherCooldown(player.slot, nativeId);
                view.toggledOn = view.id == "A0AS" ? state.berserkUntil > (world?.Clock ?? 0) : view.id == "A15X" && state.active != OriginalBowElement.None;
                view.code = view.cooldownRemaining > 1e-9 ? OriginalAbilityUseCode.Cooldown :
                    actor != null && actor.mana < rules.manaCost ? OriginalAbilityUseCode.NoMana : OriginalAbilityUseCode.Ready;
                if (view.id == "A15X" && !ArcherElementAvailable(state.next))
                { view.implemented = false; view.code = OriginalAbilityUseCode.RuleUnavailable; }
            }
            catch (InvalidOperationException) { view.code = OriginalAbilityUseCode.RuleUnavailable; }
            return true;
        }
        OriginalSessionReplyCode CastArcherAbility(Player player, OriginalSessionCommand command, OriginalAbilityView view)
        {
            try
            {
            if (view.id != "A0AS" && view.id != "A15W" && view.id != "A15X") return OriginalSessionReplyCode.RuleUnavailable;
            if (view.id == "A15W")
            { if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0 || !ValidPoint(command.x, command.y)) return OriginalSessionReplyCode.InvalidCommand; }
            else if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0) return OriginalSessionReplyCode.InvalidCommand;
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            var state = Archer(player.slot); string nativeId = view.id == "A15X" ? BowHelper(state.next) : view.id;
            if (view.id == "A15X" && !ArcherElementAvailable(state.next)) return OriginalSessionReplyCode.RuleUnavailable;
            var rules = new OriginalArcherCastRules(combatCatalog, nativeId, view.rank);
            var target = new OriginalPoint(command.x, command.y);
            if (view.id == "A15W" && SquaredDistance(actor.position, target) > rules.range * rules.range) return OriginalSessionReplyCode.NotReady;
            if (actor.mana < rules.manaCost || ArcherCooldown(player.slot, nativeId) > 1e-9 || archerCasts.ContainsKey(actor.entityId)) return OriginalSessionReplyCode.NotReady;
            // Resource and cooldown commit occurs at SPELL_EFFECT, not at an
            // interruptible order. The immediate Absk branch is set by its probe.
            var cast = new ArcherCast { actor = actor.entityId, owner = player.slot, rank = view.rank, ability = view.id,
                nativeAbility = nativeId, target = target, effectAt = world.Clock + rules.castPoint, rules = rules };
            if (!rules.preservesAttackOrder)
            {
                world.Stop(actor.entityId);
                if (weaponCycles.TryGetValue(actor.entityId, out var cycle)) cycle.winding = false;
            }
            if (view.id == "A15W") world.SetFacing(actor.entityId, Math.Atan2(target.y - actor.position.y, target.x - actor.position.x) * 180 / Math.PI);
            world.MarkCast(actor.entityId); archerCasts.Add(actor.entityId, cast);
            if (rules.castPoint == 0) AdvanceArcherAbilities();
            return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { return OriginalSessionReplyCode.RuleUnavailable; }
        }
        bool ArcherControlsActor(int id) => archerCasts.TryGetValue(id, out var cast) && !cast.rules.preservesAttackOrder;
        void CancelArcherCastOnOrder(int id) { archerCasts.Remove(id); }
        void ResetArcherCooldowns(int slot) { if (archers.TryGetValue(slot, out var state)) state.cooldowns.Clear(); }
        void RemoveArcherBuffs(int slot)
        {
            if (!archers.TryGetValue(slot, out var state)) return;
            state.berserkUntil = -1; RefreshAbilityMovement(OriginalWorld.HeroEntityId(slot));
            RescaleWeaponRate(OriginalWorld.HeroEntityId(slot), world.Clock);
        }
        double ArcherAttackSpeedBonus(int id) => ActiveArcherBerserk(id, out var rules) ? rules.attackSpeedBonus : 0;
        double ArcherMovementMultiplier(int id) => ActiveArcherBerserk(id, out var rules) ? rules.movementMultiplier : 1;
        double ArcherIncomingDamageMultiplier(int id) => ActiveArcherBerserk(id, out var rules) ? rules.incomingMultiplier : 1;
        bool ActiveArcherBerserk(int id, out OriginalArcherCastRules rules)
        {
            rules = null;
            if (!archers.TryGetValue(id, out var state) || state.berserkUntil <= world.Clock) return false;
            rules = new OriginalArcherCastRules(combatCatalog, "A0AS", state.berserkRank); return true;
        }
        OriginalHeroStatsSnapshot ApplyArcherStats(int slot, OriginalHeroStatsSnapshot baseline)
        {
            var player = players.Find(p => p.slot == slot);
            return player?.hero == "N0A0" ? OriginalArcherWeaponBonus.Apply(combatCatalog, baseline, player.progression == null ? 0 : SkillRank(player, "A0AC")) : baseline;
        }
        OriginalArcherCandidate[] ArcherCandidates(int owner)
        {
            var result = new List<OriginalArcherCandidate>();
            foreach (var unit in world.Snapshot().units)
            {
                string[] types = (combatCatalog.Unit(unit.rawcode).Text("type") ?? "").Split(',');
                result.Add(new OriginalArcherCandidate(unit.entityId, unit.position, unit.health, AreEnemies(owner, unit.ownerSlot),
                    unit.hidden || !CanSeeForCombat(owner, unit), Array.IndexOf(types, "structure") >= 0, Array.IndexOf(types, "mechanical") >= 0,
                    unit.health <= 0, HasEffectiveUnitAbility(unit, "A0K4")));
            }
            return result.ToArray();
        }
        void AdvanceArcherAbilities()
        {
            if (world == null) return;
            double now = world.Clock;
            foreach (var entry in archers)
            {
                var state = entry.Value; var actor = world.UnitState(OriginalWorld.HeroEntityId(entry.Key));
                if (state.berserkUntil >= 0 && (now + 1e-9 >= state.berserkUntil || actor == null || actor.health <= 0))
                { state.berserkUntil = -1; RefreshAbilityMovement(entry.Key); RescaleWeaponRate(entry.Key, now); }
                if (state.cycleAt >= 0 && now + 1e-9 >= state.cycleAt)
                {
                    state.cycleAt = -1;
                    var player = players.Find(p => p.slot == entry.Key);
                    player.auxiliaryAbilities.Remove(BowHelper(state.next));
                    state.next = state.next == OriginalBowElement.Lightning ? OriginalBowElement.Venom : state.next + 1;
                    player.bowElement = (int)state.next;
                    player.auxiliaryAbilities[BowHelper(state.next)] = SkillRank(player, "A15X");
                }
                for (int i = state.volleyResets.Count - 1; i >= 0; i--)
                    if (now + 1e-9 >= state.volleyResets[i]) { state.volleyResets.RemoveAt(i); state.combo = 0; }
            }
            foreach (var cast in new List<ArcherCast>(archerCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= 0 || actor.hidden || actor.paused) { archerCasts.Remove(cast.actor); continue; }
                if (now + 1e-9 < cast.effectAt) continue;
                var player = players.Find(p => p.slot == cast.owner); var state = Archer(cast.owner);
                // Preflight the source Cm profile before committing the native cost.
                double attack = ScriptedAbilityAttack(player);
                if (!world.TrySpendMana(cast.actor, cast.rules.manaCost)) { archerCasts.Remove(cast.actor); continue; }
                state.cooldowns[cast.nativeAbility] = cast.effectAt + cast.rules.cooldown;
                archerCasts.Remove(cast.actor);
                NotifyNativeSpellEffect(cast.actor, cast.nativeAbility);
                if (cast.ability == "A0AS")
                {
                    state.berserkUntil = cast.effectAt + cast.rules.duration; state.berserkRank = cast.rank;
                    state.combo += 2; state.volleyResets.Add(cast.effectAt + 5);
                    archerVolleys.Add(new ArcherVolley { actor = cast.actor, owner = cast.owner, nextTick = cast.effectAt + .03,
                        rules = new OriginalArcherVolleyRules(actor.position, attack, state.active, ArcherCandidates(cast.owner)) });
                    RefreshAbilityMovement(cast.actor); RescaleWeaponRate(cast.actor, now);
                }
                else if (cast.ability == "A15W")
                {
                    state.combo++;
                    archerShots.Add(new ArcherShot { actor = cast.actor, owner = cast.owner, nextTick = cast.effectAt + .03,
                        rules = new OriginalArcherPowerShotRules(cast.actor, cast.rank, attack, actor.facingDegrees, cast.target) });
                }
                else
                { state.active = state.next; state.charges = 5; state.combo += 3; state.cycleAt = cast.effectAt + 18.9; }
            }
            AdvanceArcherElements();
            foreach (var volley in new List<ArcherVolley>(archerVolleys))
            {
                var state = Archer(volley.owner); var player = players.Find(p => p.slot == volley.owner);
                while (!volley.rules.Completed && volley.nextTick <= now + 1e-9)
                {
                    volley.nextTick += .03;
                    foreach (var effect in volley.rules.Tick(id => world.UnitState(id)?.position, state.combo, SkillRank(player, "A0AC")))
                        ApplyArcherEffect(volley.actor, volley.owner, effect);
                }
                if (volley.rules.Completed) archerVolleys.Remove(volley);
            }
            foreach (var shot in new List<ArcherShot>(archerShots))
            {
                var state = Archer(shot.owner); var player = players.Find(p => p.slot == shot.owner);
                var actor = world.UnitState(shot.actor); if (actor == null) { archerShots.Remove(shot); continue; }
                while (!shot.rules.Completed && shot.nextTick <= now + 1e-9)
                {
                    shot.nextTick += .03;
                    foreach (var effect in shot.rules.Tick(world.UnitState(shot.actor).position, ArcherCandidates(shot.owner), state.combo, SkillRank(player, "A0AC"), state.active))
                        ApplyArcherEffect(shot.actor, shot.owner, effect);
                }
                if (shot.rules.Completed) { state.combo = 0; archerShots.Remove(shot); }
            }
        }
        void ApplyArcherEffect(int actor, int owner, OriginalArcherEffectEvent effect)
        {
            if (effect.kind == OriginalArcherEventKind.ForcedPosition) world.ForcePosition(actor, effect.position);
            else if (effect.kind == OriginalArcherEventKind.DestructableSweep)
            {
                foreach (var d in world.Snapshot().doodads)
                    if (d.health > 0 && OriginalShieldBashRules.SweepDestroys(d.rawcode, effect.position, d.position)) world.ApplyDoodadDamage(d.editorId, d.health);
            }
            else if (effect.kind == OriginalArcherEventKind.Damage)
            {
                var target = world.UnitState(effect.entityId);
                // aRe/ace use a temporary h03*/h011 damage source, not the
                // canonical hero. Zero denotes an unmaterialized host helper;
                // owner attribution still reaches the player's damage ledger.
                if (target != null) ApplyTriggeredNormalHit(0, owner, target, effect.damage);
                if (effect.applyElement) ApplyArcherElement(actor, owner, effect.entityId, effect.element);
            }
        }
    }
}

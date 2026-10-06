using System;
using System.Collections.Generic;

namespace Arena.Original
{
    [Serializable]
    public sealed class OriginalCasterTelegraph
    {
        public string abilityId;
        public int sourceEntityId;
        public OriginalPoint center;
        public double radius, resolvesAt;
    }

    public sealed partial class OriginalSession
    {
        sealed class CasterCast
        {
            internal int actor, owner;
            internal double effectAt;
            internal OriginalPoint target;
            internal OriginalCasterRules rules;
        }
        sealed class CasterEffect
        {
            internal int actor, owner, target, ticks;
            internal double nextAt, travelled, distance, angle, healthFloor;
            internal bool warning = true, throwing;
            internal OriginalPoint center, origin, targetOrigin;
            internal OriginalCasterRules rules;
            internal HashSet<int> captured;
            internal OriginalCasterTetherRules tether;
            internal OriginalCasterSleepWaveRules sleepWave;
            internal string controlToken;
        }
        readonly Dictionary<int, CasterCast> casterCasts = new Dictionary<int, CasterCast>();
        readonly Dictionary<int, double> casterCooldowns = new Dictionary<int, double>();
        readonly List<CasterEffect> casterEffects = new List<CasterEffect>();
        double nextCasterScan = 2;
        bool casterScanEnabled, casterGate = true;
        uint casterRandom;

        // CAST1 measured all three native hL axes. Helper-specific native
        // behavior remains separately gated by each rule's implementation.
        public bool CasterNativeClosureReady => TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic) &&
            TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.ChaosUniversal);
        public string[] CasterGaps
        {
            get
            {
                var gaps = new List<string>();
                if (!CasterNativeClosureReady) gaps.Add("native-caster-damage-axes-awaiting-CAST1");
                gaps.Add("A123:n06L-MP0-armor0-radius1-derived-ward-proxy;S002/3/4:instant-membership-no-linger-sparse-neutral-host-policy");
                gaps.Add("AIil:placement-and-arbitrary-item-passive-inheritance-derived;native-HERO-and-rank1/2-damage-measured");
                gaps.Add("A0Z8:n07C-rank1-weapon-and-AIcb-measured-SUMW1;native-bounty0-in6-GOLDHARPY1-samples;full-distribution-not-inferred");
                foreach (var id in OriginalCasterRules.AbilityIds)
                {
                    var rule = new OriginalCasterRules(combatCatalog, id);
                    if (!rule.scriptImplemented) gaps.Add(id + ":" + rule.unresolvedDependency);
                }
                foreach (var id in new[] { "n05J", "o00C", "n06K", "n02J", "n02O" })
                    if (!OriginalCasterRules.TryCastPoint(combatCatalog, id, out _)) gaps.Add(id + ":native-castpt-awaiting-CAST1");
                return gaps.ToArray();
            }
        }
        public OriginalCasterTelegraph[] CasterTelegraphs
        {
            get
            {
                var result = new List<OriginalCasterTelegraph>();
                foreach (var effect in casterEffects)
                    if (effect.warning) result.Add(new OriginalCasterTelegraph { abilityId = effect.rules.abilityId,
                        sourceEntityId = effect.actor, center = effect.center, radius = effect.rules.radius, resolvesAt = effect.nextAt });
                return result.ToArray();
            }
        }
        bool CasterControlsActor(int id) => casterCasts.ContainsKey(id);
        void OnCasterAcceptedWorldOrder(int id) => casterCasts.Remove(id);
        void OnCasterMatchEvent(OriginalMatchEvent item)
        {
            if (item.kind != OriginalMatchEventKind.PhaseChanged) return;
            // kI is disabled at round end. Existing effect timers are not killed.
            // Source c1 independently enables it during the duel sequence.
            var phase = (OriginalMatchPhase)item.amount;
            casterScanEnabled = phase == OriginalMatchPhase.Duel || options.casters && phase == OriginalMatchPhase.Combat;
            if (phase == OriginalMatchPhase.Combat) casterGate = true;
        }
        void AdvanceCasters()
        {
            if (world == null) return;
            double now = world.Clock;
            AdvanceCasterFields();
            AdvanceEnemyImageCasts();
            foreach (var cast in new List<CasterCast>(casterCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= .405 || actor.hidden || actor.paused || ActorCastBlocked(cast.actor))
                { casterCasts.Remove(cast.actor); continue; }
                if (cast.effectAt > now + 1e-9) continue;
                casterCasts.Remove(cast.actor);
                if (!world.TrySpendMana(cast.actor, cast.rules.manaCost)) continue;
                casterCooldowns[cast.actor] = cast.effectAt + cast.rules.cooldown;
                BeginCasterEffect(cast.actor, cast.owner, cast.rules, cast.target, cast.effectAt);
            }
            // Preserve timer due-time order across a catch-up host tick. Source
            // coordinates are sampled at host updates, not a Warcraft replay.
            int callbacks = 0;
            while (true)
            {
                CasterEffect next = null;
                foreach (var effect in casterEffects)
                    if (effect.nextAt <= now + 1e-9 && (next == null || effect.nextAt < next.nextAt)) next = effect;
                if (next == null) break;
                if (++callbacks > 4096) throw new InvalidOperationException("caster-timer-budget-exceeded");
                if (next.warning) ActivateCasterEffect(next); else TickCasterEffect(next);
            }
            AdvanceCasterFields();
            while (nextCasterScan <= now + 1e-9)
            {
                nextCasterScan += 2;
                if (casterScanEnabled && CasterNativeClosureReady) SelectCasterOrders();
            }
        }
        double CasterRandomUnit()
        {
            if (casterRandom == 0) casterRandom = unchecked((uint)seed) ^ 0x9E3779B9u;
            casterRandom ^= casterRandom << 13; casterRandom ^= casterRandom >> 17; casterRandom ^= casterRandom << 5;
            return casterRandom / 4294967296.0;
        }
        OriginalWorldUnitView[] CasterUnits()
        {
            var units = world.Snapshot().units;
            Array.Sort(units, (a, b) => a.entityId.CompareTo(b.entityId)); return units;
        }
        void SelectCasterOrders()
        {
            var roster = new List<Player>(players);
            for (int i = 0; i < roster.Count; i++)
            { int j = i + (int)(CasterRandomUnit() * (roster.Count - i)); var tmp = roster[i]; roster[i] = roster[j]; roster[j] = tmp; }
            if (!casterGate) return;
            int attempts = 0; double radius = match.Round == 22 ? 3500 : 900;
            foreach (var player in roster)
            {
                var hero = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                if (hero == null || hero.health <= .405 || hero.hidden) continue;
                foreach (var actor in CasterUnits())
                {
                    if (actor.hidden || SquaredDistance(actor.position, hero.position) > radius * radius) continue;
                    if (actor.health > .405 && CasterHasType(actor, "giant"))
                    { attempts++; TryStartCaster(actor, hero.position); }
                    // a0 uses one shared n0, including rejected native orders.
                    // After the first attempt later heroes inspect only the
                    // first enumerated unit, even if that unit is not a caster.
                    if (attempts >= 1) break;
                }
            }
        }
        bool TryStartCaster(OriginalWorldUnitView actor, OriginalPoint point)
        {
            var rule = OriginalCasterRules.ForUnit(combatCatalog, actor.rawcode);
            if (rule == null || !rule.scriptImplemented || actor.health <= .405 || actor.paused || actor.hidden || ActorCastBlocked(actor.entityId) ||
                casterCasts.ContainsKey(actor.entityId) || actor.mana < rule.manaCost ||
                casterCooldowns.TryGetValue(actor.entityId, out double until) && until > world.Clock + 1e-9 ||
                SquaredDistance(actor.position, point) > rule.castRange * rule.castRange ||
                !OriginalCasterRules.TryCastPoint(combatCatalog, actor.rawcode, out double castPoint)) return false;
            ValidateCasterHelper(rule);
            world.Stop(actor.entityId); world.SetFacing(actor.entityId, Math.Atan2(point.y - actor.position.y, point.x - actor.position.x) * 180 / Math.PI);
            world.MarkCast(actor.entityId);
            if (weaponCycles.TryGetValue(actor.entityId, out var cycle)) cycle.winding = false;
            if (castPoint == 0)
            {
                if (!world.TrySpendMana(actor.entityId, rule.manaCost)) return false;
                casterCooldowns[actor.entityId] = world.Clock + rule.cooldown;
                BeginCasterEffect(actor.entityId, actor.ownerSlot, rule, point, world.Clock);
                return true;
            }
            casterCasts[actor.entityId] = new CasterCast { actor = actor.entityId, owner = actor.ownerSlot, target = point,
                rules = rule, effectAt = world.Clock + castPoint };
            return true;
        }
        void BeginCasterEffect(int actor, int owner, OriginalCasterRules rule, OriginalPoint point, double time)
        {
            if (!rule.scriptImplemented) throw new InvalidOperationException("caster-helper-unresolved:" + rule.abilityId);
            ValidateCasterHelper(rule);
            var center = rule.WarningCenter(point);
            casterEffects.Add(new CasterEffect { actor = actor, owner = owner, rules = rule, center = center,
                ticks = rule.tickCount, nextAt = time + rule.warningSeconds });
            casterGate = false;
            var scatter = center;
            foreach (var hero in CasterUnits())
            {
                if (!CasterComputerOrLeftHero(hero) || hero.health <= .405 || hero.hidden ||
                    SquaredDistance(hero.position, center) > 1000000) continue;
                if (!rule.translatedCenter) scatter = OriginalCasterRules.WarningScatter(scatter, CasterRandomUnit() * 6.2832, 300 + 50 * CasterRandomUnit());
                CasterMoveOrder(hero.entityId, rule.translatedCenter ? center : scatter);
            }
        }
        void CasterMoveOrder(int id, OriginalPoint point)
        {
            if (!world.TryMove(id, CasterAuraPointOrder(id, 851986, point))) return;
            if (weaponCycles.TryGetValue(id, out var cycle)) cycle.winding = false;
            OnAcceptedWorldOrder(id); OnCasterAcceptedWorldOrder(id);
        }
        bool CasterComputerOrLeftHero(OriginalWorldUnitView actor)
        {
            // xZ14396 and OWv34915/Orv33219 control computer/left owners,
            // never a connected human. O006 is the sole native HERO in the
            // 84-type enemy roster; native AOmi images are not HERO units.
            if (!IsNativeHeroPredicate(actor)) return false;
            if (actor.ownerSlot == 0) return true;
            var player = players.Find(p => p.slot == actor.ownerSlot);
            return player != null && !player.connected;
        }
        bool CasterHasType(OriginalWorldUnitView actor, string type)
        {
            var definition=combatCatalog.Unit(actor.rawcode);
            bool Has(string field)=>Array.Exists((definition.Text(field)??"").Split(','),
                value=>string.Equals(value.Trim(),type,StringComparison.OrdinalIgnoreCase));
            // Native structure classification lives in targType for e.g.
            // hhou; its independent type token is Mechanical. Do not infer
            // other IsUnitType predicates from unrelated targeting flags.
            return Has("type")||string.Equals(type,"structure",StringComparison.OrdinalIgnoreCase)&&Has("targType");
        }
        bool CasterHasAbility(OriginalWorldUnitView actor, string id) =>
            HasEffectiveUnitAbility(actor, id);
        bool CasterMagicImmune(OriginalWorldUnitView actor)
        {
            foreach (var ability in NativeUnitAbilities(actor)) if (ability.Text("code") == "Amim") return true;
            return false; // Temporary item/active immunity needs its own published ability-state driver.
        }
        bool CasterEligible(CasterEffect effect, OriginalWorldUnitView target, double radius, bool heroesOnly = false,
            bool excludeMagicImmune = true, bool excludeStructures = false, bool excludeSpecial = false) =>
            target.health > .405 && !target.hidden && AreEnemies(effect.owner, target.ownerSlot) &&
            (!heroesOnly || IsNativeHeroPredicate(target)) &&
            (!excludeMagicImmune || !CasterMagicImmune(target)) &&
            (!excludeStructures || !CasterHasType(target, "structure")) &&
            (!excludeSpecial || !CasterHasAbility(target, "A0K4")) &&
            SquaredDistance(effect.center, target.position) <= radius * radius;
        bool CasterHeroGate(CasterEffect effect)
        {
            foreach (var unit in CasterUnits()) if (CasterEligible(effect, unit, effect.rules.radius, true, false)) return true;
            return false;
        }
        void CompleteCasterEffect(CasterEffect effect)
        {
            if (effect.rules.family == OriginalCasterFamily.AntiHeal) casterAntiHealTargets.Remove(effect.target);
            casterEffects.Remove(effect);
            // Source MC is a boolean. A completed projectile may reopen it even
            // while another effect is still active; a reference count is wrong.
            casterGate = true;
        }
        void ActivateCasterEffect(CasterEffect effect)
        {
            // Opv samples the caster location before warning KillUnit can
            // synchronously dispatch CA and change another match phase.
            if (effect.rules.family == OriginalCasterFamily.SleepWave)
            {
                var actor = world.UnitState(effect.actor);
                if (actor == null) { CompleteCasterEffect(effect); return; }
                effect.origin = actor.position;
            }
            // Source warning callbacks KillUnit before activating their helper
            // or damage. Its synchronous CA listener can resume Orn here.
            ObserveScriptedHelperDeath();
            var rule = effect.rules;
            effect.warning = false;
            switch (rule.family)
            {
                case OriginalCasterFamily.Totem:
                case OriginalCasterFamily.SlowAura:
                case OriginalCasterFamily.SpellCurseAura:
                case OriginalCasterFamily.ReverseOrderAura: BeginCasterField(effect); return;
                case OriginalCasterFamily.AntiHeal: BeginCasterAntiHeal(effect); return;
                case OriginalCasterFamily.AllyThrow: BeginCasterAllyThrow(effect); return;
                case OriginalCasterFamily.StrikeSummon: ActivateCasterStrikeSummon(effect); return;
                case OriginalCasterFamily.EnemyImage: BeginCasterEnemyImages(effect); return;
                case OriginalCasterFamily.SleepWave: BeginCasterSleepWave(effect); return;
                case OriginalCasterFamily.Pull:
                case OriginalCasterFamily.Drag: BeginCasterTether(effect); return;
                case OriginalCasterFamily.Pulse:
                case OriginalCasterFamily.FrostPulse:
                case OriginalCasterFamily.OutsideRing: effect.nextAt += 1; return;
                case OriginalCasterFamily.DelayedRing: effect.nextAt += 3; return;
                case OriginalCasterFamily.Swap:
                    OriginalWorldUnitView chosen = null;
                    foreach (var target in CasterUnits()) if (CasterEligible(effect, target, rule.radius, true, false)) chosen = target;
                    var source = world.UnitState(effect.actor);
                    if (chosen == null || source == null) { CompleteCasterEffect(effect); return; }
                    effect.target = chosen.entityId; effect.origin = source.position; effect.targetOrigin = chosen.position;
                    effect.angle = Math.Atan2(chosen.position.y - source.position.y, chosen.position.x - source.position.x);
                    effect.distance = Math.Sqrt(SquaredDistance(source.position, chosen.position)); effect.nextAt += .03;
                    world.SetPathingEnabled(effect.actor, false); world.SetPathingEnabled(effect.target, false); return;
                case OriginalCasterFamily.Collapse:
                case OriginalCasterFamily.ManaBurst:
                    if (CasterHeroGate(effect)) foreach (var target in CasterUnits())
                        if (CasterEligible(effect, target, rule.radius, rule.family == OriginalCasterFamily.ManaBurst))
                        {
                            CasterScriptDamage(effect, target, rule.family == OriginalCasterFamily.ManaBurst ? rule.ManaBurst(target.profile.maxMana, target.mana) : rule.damage, 2);
                            if (rule.family == OriginalCasterFamily.Collapse) world.ForcePosition(target.entityId, effect.center);
                        }
                    CompleteCasterEffect(effect); return;
                case OriginalCasterFamily.Homing:
                    bool gate = CasterHeroGate(effect);
                    var caster = world.UnitState(effect.actor);
                    if (gate && caster != null)
                        foreach (var target in CasterUnits()) if (CasterEligible(effect, target, rule.radius, true))
                            casterEffects.Add(new CasterEffect { actor = effect.actor, owner = effect.owner, target = target.entityId,
                                origin = caster.position, center = effect.center, rules = rule, warning = false, nextAt = effect.nextAt + .04 });
                    casterEffects.Remove(effect);
                    // ONv sets MC=false even if its only hero is magic immune
                    // and no projectile was created. Keep that authored edge.
                    casterGate = !gate; return;
                case OriginalCasterFamily.MovingWave:
                    var waveCaster = world.UnitState(effect.actor);
                    if (waveCaster == null) { CompleteCasterEffect(effect); return; }
                    // Ojv starts at the warning point and aims back at the
                    // current caster. The visual dummy has pathing disabled.
                    effect.angle = Math.Atan2(waveCaster.position.y - effect.center.y, waveCaster.position.x - effect.center.x);
                    effect.distance = 950; effect.captured = new HashSet<int>(); effect.nextAt += .03; return;
                default: throw new InvalidOperationException("caster-family-not-dispatched:" + rule.abilityId);
            }
        }
        void TickCasterEffect(CasterEffect effect)
        {
            var rule = effect.rules;
            switch (rule.family)
            {
                case OriginalCasterFamily.Totem:
                case OriginalCasterFamily.SlowAura:
                case OriginalCasterFamily.SpellCurseAura:
                case OriginalCasterFamily.ReverseOrderAura: TickCasterField(effect); return;
                case OriginalCasterFamily.AntiHeal: TickCasterAntiHeal(effect); return;
                case OriginalCasterFamily.AllyThrow: TickCasterAllyThrow(effect); return;
                case OriginalCasterFamily.FrostPulse: TickCasterFrostPulse(effect); return;
                case OriginalCasterFamily.SleepWave: TickCasterSleepWave(effect); return;
                case OriginalCasterFamily.Pull:
                case OriginalCasterFamily.Drag: TickCasterTether(effect); return;
                case OriginalCasterFamily.Pulse:
                    if (effect.ticks == 0) { CompleteCasterEffect(effect); return; }
                    foreach (var target in CasterUnits()) if (CasterEligible(effect, target, rule.radius)) CasterScriptDamage(effect, target, rule.damage, 2);
                    effect.ticks--; effect.nextAt += 1; return;
                case OriginalCasterFamily.OutsideRing:
                    var caster = world.UnitState(effect.actor);
                    if (effect.ticks == 0 || caster == null || caster.health < .405) { CompleteCasterEffect(effect); return; }
                    foreach (var target in CasterUnits())
                        if (CasterEligible(effect, target, 3500, false, false, true, true) && rule.OutsideSafeRadius(effect.center, target.position))
                            CasterScriptDamage(effect, target, rule.damage, 3);
                    foreach (var hero in CasterUnits())
                        if (CasterComputerOrLeftHero(hero) && hero.health > .405 && !hero.hidden && SquaredDistance(hero.position, effect.center) <= 4000000)
                            CasterMoveOrder(hero.entityId, effect.center);
                    effect.ticks--; effect.nextAt += 1; return;
                case OriginalCasterFamily.DelayedRing:
                    foreach (var target in CasterUnits())
                        if (CasterEligible(effect, target, 3500, false, true, true, true) && rule.OutsideSafeRadius(effect.center, target.position))
                            CasterScriptDamage(effect, target, rule.damage, 2);
                    CompleteCasterEffect(effect); return;
                case OriginalCasterFamily.Swap:
                    effect.travelled += 24;
                    double x = effect.travelled * Math.Cos(effect.angle), y = effect.travelled * Math.Sin(effect.angle);
                    world.ForcePosition(effect.actor, new OriginalPoint(effect.origin.x + x, effect.origin.y + y));
                    world.ForcePosition(effect.target, new OriginalPoint(effect.targetOrigin.x - x, effect.targetOrigin.y - y));
                    if (effect.travelled >= effect.distance)
                    {
                        world.SetPathingEnabled(effect.actor, true); world.SetPathingEnabled(effect.target, true);
                        effect.center = effect.targetOrigin;
                        foreach (var target in CasterUnits()) if (CasterEligible(effect, target, 200)) CasterScriptDamage(effect, target, 250, 2);
                        CompleteCasterEffect(effect);
                    }
                    else effect.nextAt += .03;
                    return;
                case OriginalCasterFamily.Homing:
                    effect.travelled += 10;
                    var victim = world.UnitState(effect.target);
                    // A removed handle cannot be damaged in this host. Death is
                    // different: native timer persists and reaches the corpse.
                    if (victim == null) { CompleteCasterEffect(effect); return; }
                    bool reached = effect.travelled >= Math.Sqrt(SquaredDistance(effect.origin, victim.position));
                    if (reached) CasterScriptDamage(effect, victim, rule.damage, 2);
                    if (reached || effect.travelled > 1750) CompleteCasterEffect(effect); else effect.nextAt += .04;
                    return;
                case OriginalCasterFamily.MovingWave:
                    // Ohv tests the previous remaining distance after moving
                    // and sweeping. Thus 950 at 27 per tick yields 37 sweeps.
                    bool lastWaveTick = effect.distance <= 0;
                    effect.distance -= 27;
                    effect.center = new OriginalPoint(effect.center.x + 27 * Math.Cos(effect.angle), effect.center.y + 27 * Math.Sin(effect.angle));
                    foreach (var target in CasterUnits())
                        if (CasterEligible(effect, target, rule.radius, false, true, true) && effect.captured.Add(target.entityId))
                            CasterScriptDamage(effect, target, rule.damage, 2);
                    if (OriginalShieldBashRules.AllowsForcedPoint(effect.center))
                        foreach (int id in effect.captured)
                        {
                            var target = world.UnitState(id);
                            if (target != null && target.health > .405) world.ForcePosition(id, effect.center);
                        }
                    if (lastWaveTick) CompleteCasterEffect(effect); else effect.nextAt += .03;
                    return;
                default: throw new InvalidOperationException("caster-timer-not-dispatched:" + rule.abilityId);
            }
        }
        void CasterScriptDamage(CasterEffect effect, OriginalWorldUnitView target, double amount, int mode)
        {
            if (target.health <= .405 || target.hidden || target.invulnerable || !AreEnemies(effect.owner, target.ownerSlot)) return;
            ApplyTriggeredHit(effect.actor, effect.owner, target, amount, (OriginalTriggeredDamageMode)mode);
        }
    }
}

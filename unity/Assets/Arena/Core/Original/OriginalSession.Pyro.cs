using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class VacuumEffect
        {
            internal int caster, owner;
            internal OriginalPoint origin, direction;
            internal OriginalPyroVacuum rule;
            internal double next;
        }
        sealed class VacuumPullEffect
        {
            internal int caster, owner, target;
            internal OriginalPyroVacuumPull rule;
            internal double next;
        }
        sealed class PortalEffect
        {
            internal int caster, owner;
            internal OriginalPoint position;
            internal double next, expires, duration, auraNext;
        }
        sealed class SphereOrbitEffect
        { internal int caster; internal double next; internal OriginalPyroSphereOrbit rule; }
        sealed class FlyingSphereEffect
        { internal int caster, owner, target; internal double next; internal OriginalPyroFlyingSphere rule; }
        sealed class SpherePushEffect
        { internal int target; internal double next; internal OriginalPyroSpherePush rule; }
        readonly Dictionary<int, VacuumEffect> pyroVacuums = new Dictionary<int, VacuumEffect>();
        readonly List<VacuumPullEffect> pyroPulls = new List<VacuumPullEffect>();
        readonly List<PortalEffect> pyroPortals = new List<PortalEffect>();
        readonly Dictionary<int, SphereOrbitEffect> pyroOrbits = new Dictionary<int, SphereOrbitEffect>();
        readonly List<FlyingSphereEffect> pyroSpheres = new List<FlyingSphereEffect>();
        readonly List<SpherePushEffect> pyroSpherePushes = new List<SpherePushEffect>();
        // Mf is deliberately a single script global, not a per-projectile
        // capture. Concurrent detonations and chained victims mutate it.
        double pyroVacuumDamageGlobal;

        void PopulatePyroAbilityView(Player player, OriginalAbilityView view)
        {
            if (player.hero != "H024" || view.id == "A001") return;
            int actorId = OriginalWorld.HeroEntityId(player.slot);
            if (view.id == "A0SJ")
            {
                view.castAbilityId = pyroVacuums.ContainsKey(actorId) ? "A0SN" : "A0SJ";
                view.targetMode = view.castAbilityId == "A0SN" ? OriginalAbilityTargetMode.None : OriginalAbilityTargetMode.Point;
            }
            else if (view.id == "A0SP")
            {
                view.castAbilityId = pyroOrbits.ContainsKey(actorId) ? "A0SO" : view.rank > 0 ? ActualPyroAbility(player, "A0SP") : "A0SP";
                view.targetMode = view.castAbilityId == "A0SO" ? OriginalAbilityTargetMode.Point : OriginalAbilityTargetMode.None;
            }
            else { view.castAbilityId = view.rank > 0 ? ActualPyroAbility(player, view.id) : view.id; view.targetMode = OriginalAbilityTargetMode.Point; }
            if (view.castAbilityId != view.id)
            {
                var declaration = combatCatalog.Ability(view.castAbilityId);
                // Secondary rank is always1: S5v/SQv add it without setting a
                // rank. Missing native cost must not inherit the learned rank.
                // PYCAST2 observes zero mana debit for these exact two helpers.
                view.manaCostKnown = true;
                int nativeRank = view.castAbilityId == "A0SN" || view.castAbilityId == "A0SO" ? 1 : Math.Max(1, view.rank);
                view.manaCost = PyroManaCost(view.castAbilityId, nativeRank);
            }
            view.implemented = PyroCastAvailable(view.castAbilityId);
            view.code = OriginalAbilityUseCode.RuleUnavailable;
            if (view.implemented)
            {
                var actor=world?.UnitState(actorId);
                view.cooldownRemaining=world==null?0:PyroCooldown(player.slot,view.castAbilityId);
                view.code=view.cooldownRemaining>1e-9?OriginalAbilityUseCode.Cooldown:
                    actor!=null&&actor.mana<view.manaCost?OriginalAbilityUseCode.NoMana:OriginalAbilityUseCode.Ready;
            }
        }

        void BeginPyroVacuum(int casterId, int rank, OriginalPoint target)
        {
            OriginalPyroEffectRules.Point(target);
            if (!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic))
                throw new InvalidOperationException("native-triggered-damage-mode-unresolved:SpellMagic");
            var actor = world.UnitState(casterId);
            if (actor == null || pyroVacuums.ContainsKey(casterId)) throw new InvalidOperationException("Pyro vacuum source unavailable.");
            var rule = new OriginalPyroVacuum(rank);
            double angle = Math.Atan2(target.y - actor.position.y, target.x - actor.position.x);
            var direction = new OriginalPoint(Math.Cos(angle), Math.Sin(angle));
            pyroVacuums.Add(casterId, new VacuumEffect { caster = casterId, owner = actor.ownerSlot, rule = rule, direction = direction,
                origin = new OriginalPoint(actor.position.x + 30 * direction.x, actor.position.y + 30 * direction.y), next = world.Clock + .04 });
        }

        void DetonatePyroVacuum(int casterId)
        { if (pyroVacuums.TryGetValue(casterId, out var effect)) effect.rule.Detonate(); }

        void BeginPyroPortal(int casterId, int rank, OriginalPoint position)
        {
            OriginalPyroEffectRules.Point(position);
            ValidatePyroAura();
            var actor = world.UnitState(casterId);
            if (actor == null) throw new InvalidOperationException("Pyro portal source unavailable.");
            pyroPortals.Add(new PortalEffect { caster = casterId, owner = actor.ownerSlot, position = position,
                next = world.Clock + .03, auraNext = world.Clock + .5, duration = OriginalHeroRules.PyroChainDuration(rank), expires = world.Clock + OriginalHeroRules.PyroChainDuration(rank) });
        }

        void BeginPyroSpheres(int casterId)
        {
            if (!TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic))
                throw new InvalidOperationException("native-triggered-damage-mode-unresolved:SpellMagic");
            var actor = world.UnitState(casterId);
            if (actor == null || pyroOrbits.ContainsKey(casterId)) throw new InvalidOperationException("Pyro orbit source unavailable.");
            pyroOrbits.Add(casterId, new SphereOrbitEffect { caster = casterId, next = world.Clock + .02,
                rule = new OriginalPyroSphereOrbit(actor.position) });
        }

        bool LaunchPyroSphere(int casterId, OriginalPoint position, int targetId = 0)
        {
            OriginalPyroEffectRules.Point(position);
            if (!pyroOrbits.TryGetValue(casterId, out var orbit)) return false;
            var caster = world.UnitState(casterId);
            var target = targetId == 0 ? null : world.UnitState(targetId);
            if (caster == null || targetId != 0 && target == null) return false;
            if (!orbit.rule.TryLaunch(out var start)) return false;
            if (targetId != casterId)
                pyroSpheres.Add(new FlyingSphereEffect { caster = casterId, owner = caster.ownerSlot, target = targetId,
                    next = world.Clock + .02, rule = new OriginalPyroFlyingSphere(start, target?.position ?? position, target != null) });
            if (orbit.rule.Completed) pyroOrbits.Remove(casterId);
            return true;
        }

        // Source Szv also advances existing projectiles for each caster
        // SPELL_EFFECT. Called by the native lifecycle adapter, not orders.
        void OnPyroSpellEffect(int casterId)
        {
            foreach (var sphere in new List<FlyingSphereEffect>(pyroSpheres))
                if (sphere.caster == casterId) StepPyroSphere(sphere, false);
        }

        void OnPyroUnitDied(int entityId)
        {
            pyroSphereChannels.Remove(entityId);
            RemovePyroChainBuff(entityId);
            // Native EVENT_UNIT_DEATH consumes these registered callbacks
            // synchronously. HasLiveTarget makes the flying death step exactly
            // once, including a later fallback Advance observing the same body.
            if (pyroOrbits.TryGetValue(entityId, out var orbit))
            {
                orbit.rule.Tick(world.UnitState(entityId)?.position ?? default, true);
                pyroOrbits.Remove(entityId);
            }
            pyroPulls.RemoveAll(pull => pull.target == entityId);
            pyroSpherePushes.RemoveAll(push => push.target == entityId);
            foreach (var sphere in new List<FlyingSphereEffect>(pyroSpheres))
                if (sphere.target == entityId && sphere.rule.HasLiveTarget) StepPyroSphere(sphere, true);
        }

        void StepPyroSphere(FlyingSphereEffect sphere, bool deathEvent)
        {
            if (!pyroSpheres.Contains(sphere)) return;
            var target = sphere.target == 0 ? null : world.UnitState(sphere.target);
            bool died = sphere.rule.HasLiveTarget && (target == null || target.health <= 0);
            if (!sphere.rule.Step(target?.position, died || deathEvent)) return;
            var player = players.Find(p => p.slot == sphere.owner);
            // Swv reads current learned ranks at impact. Spheres have no
            // captured charge-damage multiplier: its jl argument is unused.
            int ordinary = SkillRank(player, "A0SP");
            int upgraded = AuxiliaryRank(player, "A0SR");
            int rank = ordinary > upgraded ? ordinary : upgraded + 2;
            double damage = 10 + 20 * rank;
            var center = sphere.rule.Position;
            foreach (var unit in world.Snapshot().units)
                if (unit.health > .405 && AreEnemies(sphere.owner, unit.ownerSlot) && !PyroMagicImmune(unit) && SquaredDistance(center, unit.position) <= 200 * 200)
                {
                    if (PyroHasChainBuff(unit.entityId)) damage *= OriginalHeroRules.PyroChainAmplifier(SkillRank(player, "A0AE"));
                    ApplyTriggeredHit(sphere.caster, sphere.owner, unit, damage, OriginalTriggeredDamageMode.SpellMagic);
                }
            foreach (var unit in world.Snapshot().units)
                if (unit.health > .405 && AreEnemies(sphere.owner, unit.ownerSlot) && !PyroMagicImmune(unit) && SquaredDistance(center, unit.position) <= 200 * 200)
                    pyroSpherePushes.Add(new SpherePushEffect { target = unit.entityId, next = world.Clock + .03,
                        rule = new OriginalPyroSpherePush(center, unit.position) });
            pyroSpheres.Remove(sphere);
        }
        static int AuxiliaryRank(Player player, string ability) => player != null && player.auxiliaryAbilities.TryGetValue(ability, out int rank) ? rank : 0;

        bool PyroMagicImmune(OriginalWorldUnitView actor)
        {
            // Amim is an explicit native ability declaration. Dynamic
            // immunity/status providers must extend this predicate themselves.
            foreach (var ability in NativeUnitAbilities(actor))
            {
                if (ability != null && ability.Text("code") == "Amim") return true;
            }
            return false;
        }

        bool PyroHasType(OriginalWorldUnitView actor, string expected) =>
            Array.IndexOf((combatCatalog.Unit(actor.rawcode).Text("type") ?? "").Split(','), expected) >= 0;

        partial void AppendPyroVisualEffects(List<OriginalVisualEffectView> output)
        {
            foreach (var meteor in pyroMeteors)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.WarningCircle, abilityId = "A0SM", sourceEntityId = meteor.actor,
                    position = meteor.target, radius = 200, progress = Math.Max(0, Math.Min(1, 1 - (meteor.impactAt - world.Clock) / OriginalPyroEffectRules.MeteorDelay)), variant = 4 });
            foreach (var trail in pyroMeteorTrails)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb, abilityId = "A0SM", sourceEntityId = trail.actor,
                    position = trail.rule.Position, radius = 45, variant = 4 });
            foreach (var field in pyroFlameFields)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.ActiveCircle, abilityId = field.ability, sourceEntityId = field.actor,
                    position = field.position, radius = field.radius,
                    progress = Math.Max(0,Math.Min(1,(world.Clock-field.created)/(field.expires-field.created))), variant = 4 });
            foreach (var vacuum in pyroVacuums.Values)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb, abilityId = "A0SJ", sourceEntityId = vacuum.caster,
                    position = new OriginalPoint(vacuum.origin.x + vacuum.rule.TravelDistance * vacuum.direction.x,
                        vacuum.origin.y + vacuum.rule.TravelDistance * vacuum.direction.y),
                    radius = 18, progress = Math.Min(1, vacuum.rule.DistanceCounter / 900.0), variant = 1 });
            foreach (var orbit in pyroOrbits.Values)
                foreach (var point in orbit.rule.UnlaunchedPositions)
                    output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb, abilityId = "A0SP", sourceEntityId = orbit.caster,
                        position = point, radius = 18, variant = 2 });
            foreach (var sphere in pyroSpheres)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Orb, abilityId = "A0SO", sourceEntityId = sphere.caster,
                    position = sphere.rule.Position, radius = 18, variant = 2 });
            foreach (var portal in pyroPortals)
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.ActiveCircle, abilityId = "A0AE", sourceEntityId = portal.caster,
                    position = portal.position, radius = 175, progress = Math.Max(0, Math.Min(1, 1 - (portal.expires - world.Clock) / portal.duration)), variant = 3 });
        }

        void AdvancePyroEffects()
        {
            AdvancePyroAuras();
            AdvancePyroMeteors();
            foreach (var orbit in new List<SphereOrbitEffect>(pyroOrbits.Values))
            {
                var actor = world.UnitState(orbit.caster);
                while (!orbit.rule.Completed && orbit.next <= world.Clock + 1e-9)
                { orbit.next += .02; orbit.rule.Tick(actor?.position ?? default, actor == null || actor.health <= 0); }
                if (orbit.rule.Completed) pyroOrbits.Remove(orbit.caster);
            }
            foreach (var sphere in new List<FlyingSphereEffect>(pyroSpheres))
            {
                var target = sphere.target == 0 ? null : world.UnitState(sphere.target);
                if (sphere.rule.HasLiveTarget && (target == null || target.health <= 0)) StepPyroSphere(sphere, true);
                while (pyroSpheres.Contains(sphere) && sphere.next <= world.Clock + 1e-9)
                { sphere.next += .02; StepPyroSphere(sphere, false); }
            }
            foreach (var push in new List<SpherePushEffect>(pyroSpherePushes))
            {
                while (!push.rule.Completed && push.next <= world.Clock + 1e-9)
                {
                    push.next += .03; var actor = world.UnitState(push.target);
                    if (!push.rule.Tick(actor == null || actor.health <= 0, out var point)) break;
                    if (SourceUnitUserData(push.target) != 2) world.ForcePosition(push.target, point);
                    actor = world.UnitState(push.target);
                    foreach (var doodad in world.Snapshot().doodads)
                        if (doodad.health > 0 && OriginalShieldBashRules.SweepDestroys(doodad.rawcode, actor.position, doodad.position))
                            world.ApplyDoodadDamage(doodad.editorId, doodad.health);
                }
                if (push.rule.Completed) pyroSpherePushes.Remove(push);
            }
            foreach (var effect in new List<VacuumEffect>(pyroVacuums.Values))
            {
                while (!effect.rule.Detonated && effect.next <= world.Clock + 1e-9)
                { effect.next += .04; effect.rule.Advance(.04); }
                if (!effect.rule.Detonated) continue;
                var center = new OriginalPoint(effect.origin.x + effect.rule.TravelDistance * effect.direction.x,
                    effect.origin.y + effect.rule.TravelDistance * effect.direction.y);
                pyroVacuumDamageGlobal = effect.rule.Damage;
                foreach (var target in world.Snapshot().units)
                    if (target.health > .405 && AreEnemies(effect.owner, target.ownerSlot) && !PyroMagicImmune(target) && SquaredDistance(center, target.position) <= 200 * 200)
                        pyroPulls.Add(new VacuumPullEffect { caster = effect.caster, owner = effect.owner, target = target.entityId,
                            rule = new OriginalPyroVacuumPull(center, target.position), next = effect.next - .04 + .025 });
                pyroVacuums.Remove(effect.caster);
            }
            foreach (var pull in new List<VacuumPullEffect>(pyroPulls))
            {
                while (!pull.rule.Completed && pull.next <= world.Clock + 1e-9)
                {
                    pull.next += .025;
                    var actor = world.UnitState(pull.target);
                    bool moved = pull.rule.Tick(actor?.position ?? default, actor == null || actor.health <= 0, out var point, out bool damage);
                    if (moved) world.ForcePosition(pull.target, point);
                    if (damage)
                    {
                        var player = players.Find(p => p.slot == pull.owner);
                        if (PyroHasChainBuff(pull.target)) pyroVacuumDamageGlobal *= OriginalHeroRules.PyroChainAmplifier(SkillRank(player, "A0AE"));
                        ApplyTriggeredHit(pull.caster, pull.owner, world.UnitState(pull.target), pyroVacuumDamageGlobal, OriginalTriggeredDamageMode.SpellMagic);
                    }
                }
                if (pull.rule.Completed) pyroPulls.Remove(pull);
            }
            foreach (var portal in new List<PortalEffect>(pyroPortals))
            {
                while (portal.next <= world.Clock + 1e-9 && portal.next <= portal.expires + 1e-9)
                {
                    portal.next += .03;
                    foreach (var actor in world.Snapshot().units)
                        if (actor.health > .405 && AreEnemies(portal.owner, actor.ownerSlot) && !PyroHasType(actor, "structure") && !PyroMagicImmune(actor) &&
                            OriginalPyroEffectRules.PortalPull(portal.position, actor.position, out var position))
                            world.ForcePosition(actor.entityId, position);
                }
                if (portal.next > portal.expires + 1e-9 && world.Clock + 1e-9 >= portal.next) pyroPortals.Remove(portal);
            }
        }
    }
}

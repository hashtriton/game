using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class ShieldEffect
        {
            internal int caster, owner;
            internal OriginalShieldBashRules rules;
            internal double nextTick;
        }
        readonly List<ShieldEffect> shieldEffects = new List<ShieldEffect>();
        readonly HashSet<int> shieldZeroSpeed = new HashSet<int>();

        // YHv does not add A0VY and does not stun. Forced displacement must not
        // suppress weapons or casts, and must not exclude the actor from hs.
        bool IsShieldForceActive(int id) => shieldZeroSpeed.Contains(id);

        // Trusted SPELL_EFFECT seam. The command/lifecycle layer calls this
        // only after native channel cost, timing and cooldown are resolved.
        void BeginShieldEffect(int casterId, int rank, bool darkGifts)
        {
            ValidateShieldNative(rank);
            var actor = world.UnitState(casterId);
            if (actor == null) throw new InvalidOperationException("Shield caster is missing.");
            var candidates = new List<OriginalShieldBashCandidate>();
            foreach (var unit in world.Snapshot().units)
            {
                string types = combatCatalog.Unit(unit.rawcode).Text("type") ?? "";
                candidates.Add(new OriginalShieldBashCandidate(unit.entityId, unit.position, unit.health,
                    AreEnemies(actor.ownerSlot, unit.ownerSlot), Array.IndexOf(types.Split(','), "structure") >= 0));
            }
            var rules = new OriginalShieldBashRules(casterId, rank, darkGifts, actor.position, actor.facingDegrees, candidates);
            var mode = darkGifts ? OriginalTriggeredDamageMode.ChaosUniversal : OriginalTriggeredDamageMode.SpellNormal;
            if (!TriggeredDamageModeAvailable(mode)) throw new InvalidOperationException("Shield native damage flags unresolved.");
            var effect = new ShieldEffect { caster = casterId, owner = actor.ownerSlot, rules = rules, nextTick = world.Clock + OriginalShieldBashRules.TickSeconds };
            ApplyShieldEvents(effect, rules.BeginEvents());
            shieldEffects.Add(effect);
        }

        void AdvanceShieldEffects()
        {
            foreach (var effect in new List<ShieldEffect>(shieldEffects))
            {
                var caster = world.UnitState(effect.caster);
                if (caster == null) throw new InvalidOperationException("Retained shield caster is missing.");
                while (!effect.rules.Completed && effect.nextTick <= world.Clock + 1e-9)
                {
                    effect.nextTick += OriginalShieldBashRules.TickSeconds;
                    ApplyShieldEvents(effect, effect.rules.Tick(caster.position, id => world.UnitState(id)?.position));
                }
                if (effect.rules.Completed) shieldEffects.Remove(effect);
            }
        }

        void ApplyShieldEvents(ShieldEffect effect, OriginalShieldBashEvent[] events)
        {
            foreach (var item in events)
            {
                var actor = world.UnitState(item.entityId);
                if (actor == null) continue;
                switch (item.kind)
                {
                    case OriginalShieldBashEventKind.DisablePathingAndMovement:
                        CaptureAbilityMovementBase(item.entityId);
                        world.SetPathingEnabled(item.entityId, false);
                        var profile = actor.profile.Copy(); profile.moveSpeed = 0;
                        world.UpdateProfile(item.entityId, profile, actor.health, actor.mana);
                        shieldZeroSpeed.Add(item.entityId);
                        break;
                    case OriginalShieldBashEventKind.Damage:
                        // YHv uses its h011 helper as damage source. Preserve
                        // owner credit without claiming the hero made this hit.
                        ApplyTriggeredHit(0, effect.owner, actor, item.damage, item.damageMode); break;
                    case OriginalShieldBashEventKind.CrippleOrder:
                        OrderShieldCripple(effect.owner, item.entityId);
                        break;
                    case OriginalShieldBashEventKind.ForcedPosition:
                        world.ForcePosition(item.entityId, item.position); break;
                    case OriginalShieldBashEventKind.DestructableSweep:
                        foreach (var doodad in world.Snapshot().doodads)
                            if (doodad.health > 0 && OriginalShieldBashRules.SweepDestroys(doodad.rawcode, item.position, doodad.position))
                                world.ApplyDoodadDamage(doodad.editorId, doodad.health);
                        break;
                    case OriginalShieldBashEventKind.RestoreDefaultMovement:
                        shieldZeroSpeed.Remove(item.entityId); world.SetPathingEnabled(item.entityId, true);
                        RefreshAbilityMovement(item.entityId);
                        ReleaseAbilityMovementBase(item.entityId);
                        break;
                }
            }
        }
    }
}

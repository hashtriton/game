using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class EnemyImageCast
        {
            internal int donor, owner;
            internal double effectAt;
            internal OriginalEnemyImageRules rules;
        }
        readonly List<EnemyImageCast> enemyImageCasts = new List<EnemyImageCast>();

        // Native HERO is a factory-specific predicate, independent of owner,
        // combat attributes, and the server's controllable actor kind.
        bool IsNativeHeroPredicate(OriginalWorldUnitView actor)
        {
            if (actor == null) return false;
            if (actor.kind == OriginalWorldUnitKind.Illusion)
            {
                if (actor.imageFactory == OriginalImageFactory.KnightMirror || actor.imageFactory == OriginalImageFactory.EnemyWand ||
                    actor.imageFactory == OriginalImageFactory.ItemWand)
                    return false; // OMCOM2 and IMAGE2, three selected hero rawcodes.
                if (actor.imageFactory == OriginalImageFactory.BossMirror && actor.rawcode == "n017") return false;
                throw new InvalidOperationException("native-image-hero-predicate-unavailable");
            }
            string primary = combatCatalog.Unit(actor.rawcode)?.Text("Primary");
            return primary == "STR" || primary == "AGI" || primary == "INT";
        }

        void BeginCasterEnemyImages(CasterEffect effect)
        {
            var rules = new OriginalEnemyImageRules(combatCatalog, effect.rules.abilityId == "A120" ? 2 : 1);
            if (CasterHeroGate(effect))
                foreach (var target in CasterUnits())
                    if (CasterEligible(effect, target, effect.rules.radius, true))
                    {
                        // Current source caster owners are computer enemies.
                        // A playable factory for arbitrary native HERO rawcodes
                        // needs its own copy profile, not an owner's hero stats.
                        if (target.kind != OriginalWorldUnitKind.Hero || effect.owner != 0)
                            throw new InvalidOperationException("enemy-image-donor-profile-unavailable");
                        enemyImageCasts.Add(new EnemyImageCast { donor = target.entityId, owner = effect.owner,
                            effectAt = effect.nextAt + OriginalEnemyImageRules.HelperCastDelay, rules = rules });
                    }
            // Source restores MC after issuing all helper orders. It does not
            // wait for image expiry or retain the original warning timer.
            CompleteCasterEffect(effect);
        }

        void AdvanceEnemyImageCasts()
        {
            AdvanceBossImages();
            foreach (var cast in new List<EnemyImageCast>(enemyImageCasts))
            {
                if (cast.effectAt > world.Clock + 1e-9) continue;
                enemyImageCasts.Remove(cast);
                var donor = world.UnitState(cast.donor);
                if (donor == null || donor.health <= .405 || donor.hidden || CasterMagicImmune(donor)) continue;
                // IMAGE2: h00V produces exactly one native zero damage event
                // at AIil EFFECT. A logical helper never impersonates the boss
                // or copied hero for source-specific watchers.
                ApplyResolvedUnitHit(0, cast.owner, donor, 0);
                donor = world.UnitState(cast.donor);
                if (donor == null || donor.health <= .405) continue;
                if (nextIllusionId == int.MaxValue) throw new InvalidOperationException("image-identity-budget-exceeded");
                var hero = HeroCombatStats(donor.ownerSlot);
                var stats = new ActorCombatStats(hero, illusion: true);
                var profile = donor.profile.Copy(); profile.moveSpeed = hero.baseMoveSpeed;
                // Native hidden placement is not known. Use one deterministic
                // collision-checked ring, then the existing bounded search.
                if (!TryEnemyImagePosition(donor, out var point)) throw new InvalidOperationException("enemy-image-placement-unavailable");
                var row = new OriginalIllusionSpawn { entityId = nextIllusionId, position = point, profile = profile,
                    health = donor.health, mana = donor.mana };
                if (!world.TryPublishImages(donor.entityId, cast.owner, OriginalImageFactory.EnemyWand, new[] { row }))
                    throw new InvalidOperationException("enemy-image-publication-unavailable");
                illusionCombatStats.Add(row.entityId, stats);
                mirrorImages.Add(row.entityId, new MirrorImage { actor = row.entityId, source = donor.entityId,
                    outgoing = cast.rules.outgoing, incoming = cast.rules.incoming, expires = world.Clock + cast.rules.lifetime,
                    lastAdvanced = world.Clock });
                nextIllusionId++;
            }
        }

        bool TryEnemyImagePosition(OriginalWorldUnitView donor, out OriginalPoint position)
        {
            double radius = Math.Max(64, donor.profile.collisionRadius * 2);
            for (int i = 0; i < 32; i++)
            {
                double angle = i * Math.PI / 16;
                position = new OriginalPoint(donor.position.x + radius * Math.Cos(angle), donor.position.y + radius * Math.Sin(angle));
                if (world.CanPlace(position, donor.profile.collisionRadius)) return true;
            }
            return world.TryFindFreeSpawn(donor.position, donor.profile.collisionRadius, 512, out position);
        }

        // Called synchronously from damage and from the world event bridge.
        // Expiry/explicit kills notify the global CA once, without wave XP,
        // bounty, kill credit or copied hero death reporting.
        bool ObserveImageDeath(int entityId)
        {
            if (!mirrorImages.TryGetValue(entityId, out var image) || image.deathObserved || world.UnitState(entityId)?.health > 0)
                return false;
            image.deathObserved = true; ObserveScriptedHelperDeath(); return true;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class MirrorCast
        {
            internal int actor, slot, rank;
            internal OriginalPoint origin;
            internal double effectAt;
            internal bool effect, born;
            internal OriginalMirrorImageRules rules;
        }
        sealed class MirrorImage
        {
            internal int actor, source;
            internal double outgoing, incoming, expires, lastAdvanced, removeAt = -1;
            internal bool deathObserved;
            internal OriginalWorldRemovalReason reason;
        }
        readonly Dictionary<int, MirrorCast> mirrorCasts = new Dictionary<int, MirrorCast>();
        readonly Dictionary<int, double> mirrorCooldowns = new Dictionary<int, double>();
        readonly Dictionary<int, MirrorImage> mirrorImages = new Dictionary<int, MirrorImage>();
        int nextIllusionId = 1000000000;
        bool AbilityControlsActor(int id) => mirrorCasts.ContainsKey(id) || waveNativeCasts.ContainsKey(id) || ordinaryCasts.ContainsKey(id) || IsRingForcedActor(id) || ArcherControlsActor(id) || CasterControlsActor(id) || PyroControlsActor(id) || KnightControlsActor(id) || BossControlsActor(id) || SummonControlsActor(id);
        bool MirrorHiddenRegeneration(int id) => mirrorCasts.TryGetValue(id, out var cast) && cast.effect && !cast.born;
        void OnAcceptedWorldOrder(int id)
        {
            CancelQueuedCastApproach(id);
            CancelQueuedInteraction(id);
            EnforceItemTaunt(id);
            pendingItemOrders.Remove(id);
            playerAttackGoals.Remove(id);
            nativeAttackRecovery.Remove(id);
            CancelArcherCastOnOrder(id);
            CancelPyroCastOnOrder(id);
            knightCasts.Remove(id);
            OnCasterAcceptedWorldOrder(id);
            CancelBossCastOnOrder(id);
            CancelSummonCastOnOrder(id);
            waveNativeCasts.Remove(id);
            ordinaryCasts.Remove(id);
            // Before SPELL_EFFECT the accepted order interrupts the channel;
            // the effect, resource debit and source .7s timer do not happen.
            if (mirrorCasts.TryGetValue(id, out var cast) && !cast.effect) mirrorCasts.Remove(id);
        }
        void ResetAbilityCooldowns(int slot, bool duelReset = true) { mirrorCooldowns.Remove(slot); ResetArcherCooldowns(slot); ResetPyroCooldowns(slot); knightCooldowns.Remove(slot); shieldCooldowns.Remove(slot); ResetItemCooldowns(slot, duelReset); }
        void RemoveDispellableAbilityBuffs(int slot) { RemoveItemScriptDebuffs(OriginalWorld.HeroEntityId(slot)); RemoveOrdinaryNegativeBuffs(OriginalWorld.HeroEntityId(slot)); RemoveBossDoom(OriginalWorld.HeroEntityId(slot)); RemoveBossBindingDebuff(OriginalWorld.HeroEntityId(slot)); RemoveBossBanish(OriginalWorld.HeroEntityId(slot)); RemovePoison(OriginalWorld.HeroEntityId(slot)); RemoveArcherBuffs(slot); RemoveArcherDebuffs(OriginalWorld.HeroEntityId(slot)); RemoveKnightAcid(OriginalWorld.HeroEntityId(slot)); RemovePyroChainBuff(OriginalWorld.HeroEntityId(slot)); RemoveShieldCripple(OriginalWorld.HeroEntityId(slot)); RemoveNativeCorruption(OriginalWorld.HeroEntityId(slot)); }
        double MirrorCooldown(int slot) => mirrorCooldowns.TryGetValue(slot, out double end) ? Math.Max(0, end - world.Clock) : 0;
        OriginalSessionReplyCode StartMirror(Player player, int rank)
        {
            try
            {
                var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                var rules = new OriginalMirrorImageRules(combatCatalog, rank);
                if (actor.mana < rules.manaCost || MirrorCooldown(player.slot) > 1e-9) return OriginalSessionReplyCode.NotReady;
                if (!MirrorPlacement(actor, rules, out _)) return OriginalSessionReplyCode.RuleUnavailable;
                _ = ScriptedAbilityAttack(player);
                if (nextIllusionId > int.MaxValue - rules.count) return OriginalSessionReplyCode.RuleUnavailable;
                var cast = new MirrorCast { actor = actor.entityId, slot = player.slot, rank = rank, origin = actor.position,
                    rules = rules, effectAt = world.Clock + rules.castPoint };
                world.Stop(actor.entityId); world.MarkCast(actor.entityId);
                if (weaponCycles.TryGetValue(actor.entityId, out var cycle)) cycle.winding = false;
                mirrorCasts.Add(actor.entityId, cast); return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { return OriginalSessionReplyCode.RuleUnavailable; }
        }
        void AdvanceMirrors()
        {
            double now = world.Clock;
            foreach (var image in new List<MirrorImage>(mirrorImages.Values))
            {
                var actor = world.UnitState(image.actor);
                if (actor == null) { ForgetImage(image.actor); continue; }
                // IMAGE2's controlled pause extends native illusion lifetime.
                // Pause does not erase the copy or its combat lineage.
                if (actor.paused && actor.health > 0 && image.removeAt < 0) image.expires += now - image.lastAdvanced;
                image.lastAdvanced = now;
                if (image.removeAt < 0 && (actor.health <= 0 || now + 1e-9 >= image.expires))
                    EndImage(image, actor.health <= 0 ? OriginalWorldRemovalReason.Explicit : OriginalWorldRemovalReason.Expired);
                if (image.removeAt >= 0 && now + 1e-9 >= image.removeAt)
                { world.RemoveUnit(image.actor, image.reason); ForgetImage(image.actor); }
            }
            foreach (var cast in new List<MirrorCast>(mirrorCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || !cast.effect && (actor.health <= 0 || actor.hidden || actor.paused))
                { if (actor != null && cast.effect) world.SetVisibility(cast.actor, true); mirrorCasts.Remove(cast.actor); continue; }
                if (!cast.effect && now + 1e-9 >= cast.effectAt)
                {
                    if (!world.TrySpendMana(cast.actor, cast.rules.manaCost)) { mirrorCasts.Remove(cast.actor); continue; }
                    cast.effect = true; cast.origin = actor.position;
                    NotifyNativeSpellEffect(cast.actor, "A05N");
                    // LiAOmiC2: B06K is removed on the .3s effect/hide boundary,
                    // before image creation. Defend remains active on the hero.
                    RemovePoison(cast.actor);
                    RemoveOrdinaryNegativeBuffs(cast.actor);
                    RemoveItemScriptDebuffs(cast.actor);
                    RemoveBossDoom(cast.actor); RemoveBossBanish(cast.actor); RemoveBossBindingDebuff(cast.actor);
                    RemoveShieldCripple(cast.actor);
                    RemoveArcherDebuffs(cast.actor);
                    RemoveNativeCorruption(cast.actor);
                    RemoveKnightAcid(cast.actor);
                    RemovePyroChainBuff(cast.actor);
                    mirrorCooldowns[cast.slot] = cast.effectAt + cast.rules.cooldown;
                    foreach (var old in new List<MirrorImage>(mirrorImages.Values))
                        if (old.source == cast.actor && old.removeAt < 0 &&
                            world.UnitState(old.actor)?.imageFactory == OriginalImageFactory.KnightMirror)
                            EndImage(old, OriginalWorldRemovalReason.Replaced);
                    world.SetVisibility(cast.actor, false);
                }
                if (cast.effect && !cast.born && now + 1e-9 >= cast.effectAt + cast.rules.creationDelay)
                {
                    actor = world.UnitState(cast.actor);
                    if (actor.health <= 0)
                    {
                        // The accepted source timer still runs with a corpse.
                        // No living images are invented from a dead source.
                        world.SetVisibility(cast.actor, true); cast.born = true;
                    }
                    else
                    {
                    if (!MirrorPlacement(actor, cast.rules, out var positions)) throw new InvalidOperationException("mirror-placement-unavailable");
                    var heroStats = HeroCombatStats(cast.slot);
                    var captured = new ActorCombatStats(heroStats, illusion: true);
                    var imageProfile = actor.profile.Copy();
                    // Native image has the learned Defend ability but its
                    // toggle is off: measured speed250, not caster speed175.
                    imageProfile.moveSpeed = heroStats.baseMoveSpeed;
                    var rows = new OriginalIllusionSpawn[cast.rules.count];
                    for (int i = 0; i < rows.Length; i++) rows[i] = new OriginalIllusionSpawn {
                        entityId = checked(nextIllusionId + i), position = positions[i + 1], profile = imageProfile.Copy(), health = actor.health, mana = actor.mana };
                    if (!world.TryPublishIllusions(cast.actor, rows)) throw new InvalidOperationException("mirror-publication-unavailable");
                    foreach (var row in rows)
                    {
                        illusionCombatStats.Add(row.entityId, captured);
                        mirrorImages.Add(row.entityId, new MirrorImage { actor = row.entityId, source = cast.actor,
                            outgoing = cast.rules.outgoing, incoming = cast.rules.incoming, expires = now + cast.rules.lifetime, lastAdvanced = now });
                    }
                    nextIllusionId = checked(nextIllusionId + rows.Length);
                    world.ForcePosition(cast.actor, positions[0]);
                    if (!world.SetVisibility(cast.actor, true)) throw new InvalidOperationException("mirror-reveal-placement-unavailable");
                    cast.born = true;
                    }
                }
                if (cast.effect && now + 1e-9 >= cast.effectAt + OriginalMirrorImageRules.HelperDelay)
                {
                    var player = players.Find(p => p.slot == cast.slot);
                    int currentRank = SkillRank(player, "A05N");
                    double damage = OriginalHeroRules.KnightMirrorDamage(currentRank, ScriptedAbilityAttack(player));
                    foreach (var target in world.Snapshot().units)
                        if (target.health > .405 && AreEnemies(cast.slot, target.ownerSlot) &&
                            SquaredDistance(target.position, cast.origin) <= 350 * 350)
                            ApplyTriggeredNormalHit(cast.actor, cast.slot, target, damage);
                    if (OriginalMirrorImageRules.ReturnsToOrigin(cast.origin))
                    {
                        actor = world.UnitState(cast.actor);
                        // SetUnitPosition projects when occupied. The bounded
                        // free-position search is the host's replacement policy.
                        if (actor.health <= 0) world.RelocateStoredPosition(cast.actor, cast.origin);
                        else if (!world.Relocate(cast.actor, cast.origin))
                        {
                            if (!world.TryFindFreeSpawn(cast.origin, actor.profile.collisionRadius, 512, out var point) || !world.Relocate(cast.actor, point))
                                throw new InvalidOperationException("mirror-return-placement-unavailable");
                        }
                    }
                    mirrorCasts.Remove(cast.actor);
                }
            }
        }
        // AOmi's private placeholder/pathing algorithm is not reproduced.
        // Authored radius128 drives deterministic, fully collision-checked
        // positions. All N+1 locations are reserved before any image is added.
        bool MirrorPlacement(OriginalWorldUnitView source, OriginalMirrorImageRules rules, out OriginalPoint[] result)
        {
            result = new OriginalPoint[rules.count + 1]; int count = 0;
            for (int ring = 1; ring <= 4 && count < result.Length; ring++)
                for (int i = 0; i < 32 && count < result.Length; i++)
                {
                    double angle = 2 * Math.PI * i / 32;
                    var p = new OriginalPoint(source.position.x + rules.placementRadius * ring * Math.Cos(angle),
                        source.position.y + rules.placementRadius * ring * Math.Sin(angle));
                    if (!world.CanPlace(p, source.profile.collisionRadius, source.entityId)) continue;
                    bool overlaps = false;
                    for (int j = 0; j < count; j++) if (SquaredDistance(result[j], p) < 4 * source.profile.collisionRadius * source.profile.collisionRadius) overlaps = true;
                    if (!overlaps) result[count++] = p;
                }
            return count == result.Length;
        }
        void EndImage(MirrorImage image, OriginalWorldRemovalReason reason)
        {
            if (world.ForceUnitDeath(image.actor)) OnPyroUnitDied(image.actor);
            ObserveImageDeath(image.actor);
            world.SetVisibility(image.actor, false);
            image.removeAt = world.Clock + OriginalMirrorImageRules.CorpseDelay; image.reason = reason;
            weaponCycles.Remove(image.actor);
        }
        void ForgetImage(int id) { mirrorImages.Remove(id); illusionCombatStats.Remove(id); weaponCycles.Remove(id); bossImageInsideArena.Remove(id); bossImageDamageWatches.Remove(id); }
        double ImageOutgoingDamage(int id, double value) => mirrorImages.TryGetValue(id, out var image) ? value * image.outgoing : value;
        double ImageIncomingDamage(int id, double value) => mirrorImages.TryGetValue(id, out var image) ? value * image.incoming : value;
        static int SkillRank(Player player, string id)
        {
            foreach (var skill in player.progression.Snapshot().skills) if (skill.id == id) return skill.rank;
            return 0;
        }
        double ScriptedAbilityAttack(Player player)
        {
            var bonuses = new List<double>();
            var ranks = new Dictionary<string, int>(player.auxiliaryAbilities, StringComparer.Ordinal);
            foreach (var skill in player.progression.Snapshot().skills)
                if (skill.rank > 0) ranks[skill.id] = skill.rank;
            if (itemEffects != null)
            {
                var plan = itemEffects.Plan(player.inventory.Snapshot());
                foreach (var entry in plan.CmContributions) bonuses.Add(entry.Value);
                foreach (var item in plan.Items)
                    foreach (var ability in item.DirectAbilities)
                        if (!ranks.ContainsKey(ability)) ranks.Add(ability, 1);
            }
            else foreach (var item in player.inventory.HeroSlots)
                if (item != null) throw new InvalidOperationException("scripted-item-damage-registry-unavailable");
            foreach (var ability in ranks) bonuses.Add(OriginalScriptedAttackRules.AbilityBonus(ability.Key, ability.Value));
            // Temporary Em buffs are added by their own effect drivers.
            return OriginalHeroRules.AbilityAttack(HeroCombatStats(player.slot).primary.Require(), SoulUpgradeRank(player.slot, "R001"), bonuses);
        }
    }
}

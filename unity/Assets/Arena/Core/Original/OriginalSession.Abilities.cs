using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalAbilityTargetMode { None, Unit, Point, UnitOrPoint }
    public enum OriginalAbilityUseCode { Ready, NotLearned, Passive, Cooldown, NoMana, Busy, Dead, Paused, RuleUnavailable }
    [Serializable]
    public sealed class OriginalAbilityView
    {
        public string id, castAbilityId, name;
        public int rank;
        public double manaCost, cooldownRemaining;
        public bool manaCostKnown, implemented, toggledOn;
        public OriginalAbilityTargetMode targetMode;
        public OriginalAbilityUseCode code;
    }
    public sealed partial class OriginalSession
    {
        readonly struct ActorCombatStats
        {
            internal readonly double armor, primaryDamageBonus, agilityAttackSpeedBonus, strength, intelligence, primary;
            internal readonly double itemAttackDamageBonus, itemAttackSpeedBonus, itemHealthRegen, itemManaRegenFraction;
            internal readonly double upgradeAttackDamageBonus, upgradeAttackSpeedBonus, upgradeRegenPerSecond;
            internal ActorCombatStats(OriginalHeroStatsSnapshot source, bool illusion = false, double armorBonus = 0)
            {
                armor = source.armor.Require() + armorBonus; primaryDamageBonus = source.primaryDamageBonus.Require();
                agilityAttackSpeedBonus = source.agilityAttackSpeedBonus.Require(); strength = source.strength.Require();
                intelligence = source.intelligence.Require(); primary = source.primary.Require();
                // LiAOmiC2 native paired I007 rows: flat +12 damage affects the
                // hero but not either rank1/rank3 image's matching hit sequence.
                // Attribute-derived damage remains part of the captured base.
                itemAttackDamageBonus = illusion ? 0 : source.itemAttackDamageBonus; itemAttackSpeedBonus = source.itemAttackSpeedBonus;
                itemHealthRegen = source.itemHealthRegen; itemManaRegenFraction = source.itemManaRegenFraction;
                upgradeAttackDamageBonus = source.upgradeAttackDamageBonus; upgradeAttackSpeedBonus = source.upgradeAttackSpeedBonus;
                upgradeRegenPerSecond = source.upgradeRegenPerSecond;
                if (!OriginalCombatDefinition.IsFinite(itemAttackDamageBonus) || !OriginalCombatDefinition.IsFinite(itemAttackSpeedBonus) ||
                    !OriginalCombatDefinition.IsFinite(itemHealthRegen) || !OriginalCombatDefinition.IsFinite(itemManaRegenFraction))
                    throw new InvalidOperationException("Non-finite composed item combat modifier.");
            }
            internal ActorCombatStats(ActorCombatStats source, double armorBonus)
            {
                armor = source.armor + armorBonus; primaryDamageBonus = source.primaryDamageBonus;
                agilityAttackSpeedBonus = source.agilityAttackSpeedBonus; strength = source.strength;
                intelligence = source.intelligence; primary = source.primary;
                itemAttackDamageBonus = source.itemAttackDamageBonus; itemAttackSpeedBonus = source.itemAttackSpeedBonus;
                itemHealthRegen = source.itemHealthRegen; itemManaRegenFraction = source.itemManaRegenFraction;
                upgradeAttackDamageBonus = source.upgradeAttackDamageBonus; upgradeAttackSpeedBonus = source.upgradeAttackSpeedBonus;
                upgradeRegenPerSecond = source.upgradeRegenPerSecond;
            }
            internal ActorCombatStats(double nativeArmor, double scriptedAttackBonus)
            {
                if (!OriginalCombatDefinition.IsFinite(nativeArmor) || !OriginalCombatDefinition.IsFinite(scriptedAttackBonus))
                    throw new InvalidOperationException("Non-finite nonhero image combat capture.");
                armor = nativeArmor; itemAttackDamageBonus = scriptedAttackBonus;
                primaryDamageBonus = agilityAttackSpeedBonus = strength = intelligence = primary = 0;
                itemAttackSpeedBonus = itemHealthRegen = itemManaRegenFraction = 0;
                upgradeAttackDamageBonus = upgradeAttackSpeedBonus = upgradeRegenPerSecond = 0;
            }
        }
        // Values are captured at native summon publication. The ancestry link
        // never grants an illusion the owner's later levels, items or buffs.
        readonly Dictionary<int, ActorCombatStats> illusionCombatStats = new Dictionary<int, ActorCombatStats>();
        readonly HashSet<int> defendingHeroes = new HashSet<int>();
        readonly Dictionary<int, double> nativeAttackRecovery = new Dictionary<int, double>();
        partial void OnActorControlsAdded(int actor, OriginalActorControlMask newlyBlocked)
        {
            if ((newlyBlocked & OriginalActorControlMask.Weapon) != 0 && weaponCycles.TryGetValue(actor, out var cycle))
                cycle.winding = false;
            if ((newlyBlocked & OriginalActorControlMask.Cast) == 0) return;
            if (queuedCastApproaches.TryGetValue(actor, out var approach) && approach.command.kind == OriginalSessionCommandKind.CastSkill)
                CancelQueuedCastApproach(actor);
            // Native control is not an accepted replacement order. Preserve
            // destinations, already released effects and attack recovery.
            CancelArcherCastOnOrder(actor); CancelPyroCastOnOrder(actor);
            knightCasts.Remove(actor); OnCasterAcceptedWorldOrder(actor); CancelBossCastOnOrder(actor);
            waveNativeCasts.Remove(actor);
            ordinaryCasts.Remove(actor);
            CancelSummonCastOnOrder(actor);
            if (mirrorCasts.TryGetValue(actor, out var mirror) && !mirror.effect) mirrorCasts.Remove(actor);
        }
        bool AutomaticAttackRecovery(int actor) => PyroPostEffectChannel(actor) ||
            nativeAttackRecovery.TryGetValue(actor, out double until) && world.Clock + 1e-9 < until;
        OriginalSessionReplyCode FinishAbilityOrder(Player player, OriginalSessionReplyCode result)
        {
            if (result == OriginalSessionReplyCode.Accepted)
            {
                int actor = OriginalWorld.HeroEntityId(player.slot);
                RevealItemInvisibility(actor);
                pendingItemOrders.Remove(actor);
                // An accepted explicit ability replaces the prior attack-move
                // destination, just as an accepted Move/Stop order does.
                playerAttackGoals.Remove(actor);
                nativeAttackRecovery.Remove(actor); pyroSphereChannels.Remove(actor);
            }
            return result;
        }
        static bool UsesHeroCombatStats(OriginalWorldUnitView actor) => actor.kind==OriginalWorldUnitKind.Hero || actor.kind==OriginalWorldUnitKind.Illusion;
        ActorCombatStats CombatStatsFor(OriginalWorldUnitView actor)
        {
            double armorBonus = KnightArmorBonus(actor.entityId) + ArcherDebuffArmorDelta(actor.entityId) + NativeCorruptionArmorDelta(actor.entityId) + ItemArmorBonus(actor.entityId);
            armorBonus += ItemAuraArmorFlat(actor.entityId) + OrdinaryArmorDelta(actor.entityId);
            if (actor.kind == OriginalWorldUnitKind.Hero)
            {
                var baseline = HeroCombatStats(actor.ownerSlot);
                return new ActorCombatStats(baseline, armorBonus: armorBonus + baseline.armor.Require() * (ItemAuraArmorFraction(actor.entityId) + UniqueSoulArmorFraction(actor.entityId) + CurseArmorFraction(actor.entityId)));
            }
            if (actor.kind == OriginalWorldUnitKind.Illusion && illusionCombatStats.TryGetValue(actor.entityId, out var stats)) return new ActorCombatStats(stats, armorBonus + stats.armor * (ItemAuraArmorFraction(actor.entityId) + UniqueSoulArmorFraction(actor.entityId)));
            throw new InvalidOperationException("illusion-combat-profile-unavailable:" + actor.entityId);
        }
        OriginalAbilityView[] AbilityViews(Player player)
        {
            if (player.progression == null) return Array.Empty<OriginalAbilityView>();
            var hero = combatCatalog.Hero(player.hero);
            if (hero == null) return Array.Empty<OriginalAbilityView>();
            var result = new OriginalAbilityView[hero.skills.Length];
            var learned = player.progression?.Snapshot();
            var actor = world?.UnitState(OriginalWorld.HeroEntityId(player.slot));
            for (int i = 0; i < result.Length; i++)
            {
                string id = hero.skills[i]; int rank = 0;
                if (learned != null)
                    foreach (var skill in learned.skills) if (skill.id == id) rank = skill.rank;
                var declaration = combatCatalog.Ability(id);
                var view = new OriginalAbilityView { id = id, castAbilityId = id, rank = rank, code = OriginalAbilityUseCode.RuleUnavailable };
                view.manaCostKnown = declaration.TryNumber("Cost" + Math.Max(1, rank), out double cost, out _) && cost >= 0;
                view.manaCost = view.manaCostKnown ? cost : 0;
                if (id == "A05M")
                {
                    try
                    {
                        _ = new OriginalDefendRules(combatCatalog, Math.Max(1, rank));
                        view.manaCostKnown = true; view.manaCost = 0;
                        view.implemented = true; view.toggledOn = defendingHeroes.Contains(player.slot);
                        view.code = OriginalAbilityUseCode.Ready;
                    }
                    catch (InvalidOperationException) { view.code = OriginalAbilityUseCode.RuleUnavailable; }
                }
                if (id == "A05N")
                {
                    try
                    {
                        var rules = new OriginalMirrorImageRules(combatCatalog, Math.Max(1, rank));
                        view.implemented = true; view.manaCostKnown = true; view.manaCost = rules.manaCost;
                        view.cooldownRemaining = world == null ? 0 : MirrorCooldown(player.slot);
                        view.code = view.cooldownRemaining > 1e-9 ? OriginalAbilityUseCode.Cooldown :
                            actor != null && actor.mana < rules.manaCost ? OriginalAbilityUseCode.NoMana : OriginalAbilityUseCode.Ready;
                    }
                    catch (InvalidOperationException) { view.code = OriginalAbilityUseCode.RuleUnavailable; }
                }
                PopulateArcherAbilityView(player, actor, view);
                PopulateKnightAbilityView(player, actor, view);
                PopulatePyroAbilityView(player, view);
                // Aamk and AHbh are native passive skills. Channel-derived
                // skills with a missing order are not assumed passive here.
                string code = declaration.Text("code");
                if (code == "Aamk" || code == "AHbh") view.code = OriginalAbilityUseCode.Passive;
                else if (rank == 0) view.code = OriginalAbilityUseCode.NotLearned;
                else if (actor != null && actor.health <= 0) view.code = OriginalAbilityUseCode.Dead;
                else if (actor != null && actor.paused) view.code = OriginalAbilityUseCode.Paused;
                else if (view.implemented && (actor == null || actor.hidden || AbilityControlsActor(actor.entityId) || ActorCastBlocked(actor.entityId))) view.code = OriginalAbilityUseCode.Busy;
                result[i] = view;
            }
            return result;
        }
        OriginalSessionReplyCode ApplyAbilityCommand(Player player, OriginalSessionCommand command)
        {
            if(command.actorEntityId!=0&&command.actorEntityId!=OriginalWorld.HeroEntityId(player.slot))
                return ApplySummonAbilityCommand(player,command);
            if (command.skillId == null || command.skillId.Length != 4 ||
                command.actorEntityId != 0 && command.actorEntityId != OriginalWorld.HeroEntityId(player.slot))
                return OriginalSessionReplyCode.InvalidCommand;
            var hero = combatCatalog.Hero(player.hero);
            if (hero == null) return OriginalSessionReplyCode.InvalidCommand;
            var view = Array.Find(AbilityViews(player), ability => ability.castAbilityId == command.skillId);
            if (view == null) return OriginalSessionReplyCode.InvalidCommand;
            if (!Started || world == null || player.progression == null || pendingDuel ||
                match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            if (view.code == OriginalAbilityUseCode.NotLearned || view.code == OriginalAbilityUseCode.Dead ||
                view.code == OriginalAbilityUseCode.Paused || view.code == OriginalAbilityUseCode.Passive || view.code == OriginalAbilityUseCode.Busy ||
                view.code == OriginalAbilityUseCode.Cooldown || view.code == OriginalAbilityUseCode.NoMana)
                return OriginalSessionReplyCode.NotReady;
            if (command.skillId == "A05N" && view.code == OriginalAbilityUseCode.Ready)
            {
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0) return OriginalSessionReplyCode.InvalidCommand;
                return FinishAbilityOrder(player, StartMirror(player, view.rank));
            }
            if (command.skillId == "A05M" && view.code == OriginalAbilityUseCode.Ready)
            {
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0) return OriginalSessionReplyCode.InvalidCommand;
                return FinishAbilityOrder(player, ToggleDefend(player, view.rank));
            }
            if ((command.skillId == "A0E6" || command.skillId == "A102") && view.code == OriginalAbilityUseCode.Ready) return FinishAbilityOrder(player, CastKnightAbility(player, command, view));
            if (player.hero == "N0A0" && view.code == OriginalAbilityUseCode.Ready)
                return FinishAbilityOrder(player, CastArcherAbility(player, command, view));
            if (player.hero == "H024" && view.code == OriginalAbilityUseCode.Ready)
                return FinishAbilityOrder(player, CastPyroAbility(player, command, view));
            return OriginalSessionReplyCode.RuleUnavailable;
        }
        OriginalSessionReplyCode ToggleDefend(Player player, int rank)
        {
            try
            {
                var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
                bool enabled = !defendingHeroes.Contains(player.slot);
                var rules = new OriginalDefendRules(combatCatalog, rank);
                var profile = actor.profile.Copy();
                // Rebuild from the full unshielded baseline, never invert the
                // previously modified speed or accumulate toggle multipliers.
                profile.moveSpeed = ClampAbilityMoveSpeed(HeroCombatStats(player.slot).baseMoveSpeed *
                    ((enabled ? rules.movementMultiplier : 1) + PoisonMovementMultiplier(actor.entityId) - 1 + KnightMovementBonus(actor.entityId) + PyroMovementBonus(actor.entityId) + ArcherDebuffMovementBonus(actor.entityId) + BossBanishMovementBonus(actor.entityId) + BossBindingMovementBonus(actor.entityId) + CasterAuraMovementBonus(actor.entityId) + ItemAuraMovementBonus(actor.entityId) + SummonAbilityMovementBonus(actor.entityId) + ItemStatusMovementBonus(actor.entityId) + OrdinaryMovementBonus(actor.entityId) + ItemScriptDebuffMovementBonus(actor.entityId)));
                if(OrdinaryEntangled(actor.entityId))profile.moveSpeed=0;
                if (!world.UpdateProfile(actor.entityId, profile, actor.health, actor.mana)) return OriginalSessionReplyCode.RuleUnavailable;
                if (enabled) defendingHeroes.Add(player.slot); else defendingHeroes.Remove(player.slot);
                if (enabled)
                {
                    // ARCHH2 rank1/rank3: accepting defend clears the current
                    // attack order. Existing released projectiles survive.
                    world.Stop(actor.entityId);
                    if (weaponCycles.TryGetValue(actor.entityId,out var cycle)) cycle.winding=false;
                }
                // Native defend emits no SPELL_* events and spends no mana.
                return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { return OriginalSessionReplyCode.RuleUnavailable; }
        }
        double AbilityMovementMultiplier(int lobbySlot, OriginalHeroProgression candidate = null)
        {
            if (IsRingForcedActor(OriginalWorld.HeroEntityId(lobbySlot)) || IsShieldForceActive(OriginalWorld.HeroEntityId(lobbySlot))) return 0;
            double poison = PoisonMovementMultiplier(OriginalWorld.HeroEntityId(lobbySlot));
            double archer = ArcherMovementMultiplier(OriginalWorld.HeroEntityId(lobbySlot));
            double knight = KnightMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + PyroMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + ArcherDebuffMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + BossBanishMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + BossBindingMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + CasterAuraMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + ItemAuraMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + SummonAbilityMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + ItemStatusMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + OrdinaryMovementBonus(OriginalWorld.HeroEntityId(lobbySlot)) + ItemScriptDebuffMovementBonus(OriginalWorld.HeroEntityId(lobbySlot));
            if (!defendingHeroes.Contains(lobbySlot)) return (poison + knight) * archer;
            var player = players.Find(p => p.slot == lobbySlot);
            int rank = 0;
            foreach (var skill in (candidate ?? player.progression).Snapshot().skills) if (skill.id == "A05M") rank = skill.rank;
            // LiADefP1 late-order controls:250 with rank1Defend +TC/TD is
            // 150/125, rank3 gives175/150. Reductions share the base speed.
            return (new OriginalDefendRules(combatCatalog, rank).movementMultiplier + poison - 1 + knight) * archer;
        }
        double ResolveAbilityMoveSpeed(int slot, double baseSpeed, OriginalHeroProgression candidate = null)
        {
            int actor=OriginalWorld.HeroEntityId(slot);
            if(IsRingForcedActor(actor) || IsShieldForceActive(actor) || OrdinaryEntangled(actor))return 0;
            return CurseMoveSpeed(actor,ClampAbilityMoveSpeed(baseSpeed * AbilityMovementMultiplier(slot, candidate)));
        }
        static double ClampAbilityMoveSpeed(double speed)
        {
            if (!OriginalCombatDefinition.IsFinite(speed)) throw new InvalidOperationException("Non-finite ability movement speed.");
            // CAPS3/1.26: requested0 and combined slow both getter1 with
            // positive movement; requested600 clamps522. Explicit scripted
            // force execution remains zero via its separate caller guard.
            return Math.Max(1, Math.Min(522, speed));
        }
        double ApplyAbilityIncomingWeaponDamage(OriginalWorldUnitView target, double damage, string attackType)
        {
            damage = ImageIncomingDamage(target.entityId, damage) * ArcherIncomingDamageMultiplier(target.entityId);
            if (target.kind != OriginalWorldUnitKind.Hero || !defendingHeroes.Contains(target.ownerSlot)) return damage;
            var player = players.Find(p => p.slot == target.ownerSlot);
            int rank = 0;
            foreach (var skill in player.progression.Snapshot().skills) if (skill.id == "A05M") rank = skill.rank;
            return new OriginalDefendRules(combatCatalog, rank).IncomingWeaponDamage(damage, attackType);
        }
        void AdvanceAbilities(double seconds) { AdvancePoisons(); AdvanceShieldCripples(); AdvanceMirrors(); AdvanceKnightAbilities(); AdvanceShieldEffects(); AdvancePyroCasts(); AdvancePyroEffects(); AdvanceArcherAbilities(); AdvancePermanentImmolations(seconds); AdvanceNativeWeaponProcs(seconds); AdvanceItemAxes(); }
    }
}

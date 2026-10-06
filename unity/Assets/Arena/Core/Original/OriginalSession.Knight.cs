using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class KnightCast { internal int actor, owner, rank; internal string ability; internal double effectAt, cost, cooldown; }
        sealed class DarkGiftEffect { internal OriginalKnightDarkGiftsRules rules; internal int rank; internal double next; }
        sealed class DarkGiftAcid { internal int rank; internal double expires; }
        readonly Dictionary<int, KnightCast> knightCasts = new Dictionary<int, KnightCast>();
        readonly Dictionary<int, double> knightCooldowns = new Dictionary<int, double>();
        readonly Dictionary<int, double> shieldCooldowns = new Dictionary<int, double>();
        readonly List<DarkGiftEffect> darkGiftEffects = new List<DarkGiftEffect>();
        readonly Dictionary<int, DarkGiftAcid> darkGiftAcids = new Dictionary<int, DarkGiftAcid>();
        readonly HashSet<int> darkGiftResistance = new HashSet<int>();

        // LiAKult2, campaign d4397ebf710e3f23b0ddb2ca51c5bc6ab2330d674fe9c09e01cb1dc5daa019e0.
        // Allied ANab: +40/80/120 armor, +.05/.10/.15 movement and +.3/.4/.5 IAS.
        // AIsr .4 affects ATTACK_TYPE_NORMAL with NORMAL/MAGIC, not UNIVERSAL.
        // The native helper's same-callback multi-target dispatch is reconstructed
        // from Ydv72841; individual helper effects are measured in KULT2.
        void ValidateDarkGiftNative(int rank)
        {
            if (rank < 1 || rank > 3) throw new ArgumentOutOfRangeException(nameof(rank));
            var acid = combatCatalog.Ability("A0UU"); var cast = combatCatalog.Ability("A0E6");
            if (acid.Text("code") != "ANab" || cast.Text("code") != "AOws" || cast.Text("targs" + rank) != "none" ||
                Math.Abs(acid.Number("DataA" + rank) - .05 * rank) > 1e-9 || Math.Abs(acid.Number("DataB" + rank) - (.2 + .1 * rank)) > 1e-9 ||
                acid.Number("DataC" + rank) != -40 * rank || acid.Number("Dur" + rank) != 20 ||
                combatCatalog.Ability("A0E5").Number("DataB1") != .4)
                throw new InvalidOperationException("dark-gifts-native-declaration-conflict");
            _ = cast.Number("Cost" + rank); _ = cast.Number("Cool" + rank);
        }
        double KnightCooldown(int owner, bool shield = false) => (shield ? shieldCooldowns : knightCooldowns).TryGetValue(owner, out double until) ? Math.Max(0, until - world.Clock) : 0;
        bool KnightControlsActor(int actor) => knightCasts.ContainsKey(actor);
        void PopulateKnightAbilityView(Player player, OriginalWorldUnitView actor, OriginalAbilityView view)
        {
            if (view.id != "A0E6" && view.id != "A102") return;
            try
            {
                int rank = Math.Max(1, view.rank);
                if (view.id == "A102") ValidateShieldNative(rank); else ValidateDarkGiftNative(rank);
                view.implemented = true; view.manaCostKnown = true; view.manaCost = combatCatalog.Ability(view.id).Number("Cost" + rank);
                view.cooldownRemaining = world == null ? 0 : KnightCooldown(player.slot, view.id == "A102");
                view.code = view.cooldownRemaining > 1e-9 ? OriginalAbilityUseCode.Cooldown :
                    actor != null && actor.mana < view.manaCost ? OriginalAbilityUseCode.NoMana : OriginalAbilityUseCode.Ready;
            }
            catch (InvalidOperationException) { view.implemented = false; view.code = OriginalAbilityUseCode.RuleUnavailable; }
        }
        OriginalSessionReplyCode CastKnightAbility(Player player, OriginalSessionCommand command, OriginalAbilityView view)
        {
            if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0) return OriginalSessionReplyCode.InvalidCommand;
            if (view.id == "A102") ValidateShieldNative(view.rank); else ValidateDarkGiftNative(view.rank);
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null || actor.health <= 0 || actor.hidden || actor.paused || KnightControlsActor(actor.entityId)) return OriginalSessionReplyCode.NotReady;
            var declaration = combatCatalog.Ability(view.id);
            double cost = declaration.Number("Cost" + view.rank);
            if (actor.mana < cost || KnightCooldown(player.slot, view.id == "A102") > 1e-9) return OriginalSessionReplyCode.NotReady;
            world.Stop(actor.entityId); world.MarkCast(actor.entityId);
            if (weaponCycles.TryGetValue(actor.entityId, out var cycle)) cycle.winding = false;
            knightCasts.Add(actor.entityId, new KnightCast { actor = actor.entityId, owner = player.slot, rank = view.rank, ability = view.id,
                effectAt = world.Clock + combatCatalog.Unit("H008").Number("castpt"), cost = cost, cooldown = declaration.Number("Cool" + view.rank) });
            return OriginalSessionReplyCode.Accepted;
        }
        void BeginDarkGifts(int owner, int rank)
        {
            ValidateDarkGiftNative(rank);
            darkGiftEffects.Add(new DarkGiftEffect { rank = rank, rules = new OriginalKnightDarkGiftsRules(owner, rank), next = world.Clock + .5 });
        }
        void AdvanceKnightAbilities()
        {
            double now = world.Clock;
            foreach (var cast in new List<KnightCast>(knightCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= 0 || actor.hidden || actor.paused) { knightCasts.Remove(cast.actor); continue; }
                if (now + 1e-9 < cast.effectAt) continue;
                knightCasts.Remove(cast.actor);
                if (!world.TrySpendMana(cast.actor, cast.cost)) continue;
                NotifyNativeSpellEffect(cast.actor, cast.ability);
                if (cast.ability == "A102")
                {
                    shieldCooldowns[cast.owner] = cast.effectAt + cast.cooldown;
                    BeginShieldEffect(cast.actor, cast.rank, darkGiftAcids.ContainsKey(cast.actor));
                }
                else
                {
                    knightCooldowns[cast.owner] = cast.effectAt + cast.cooldown;
                    nativeAttackRecovery[cast.actor] = cast.effectAt + .51; // KULT2 FINISH=.81, EFFECT=.3.
                    BeginDarkGifts(cast.owner, cast.rank);
                }
            }
            foreach (var pair in new List<KeyValuePair<int, DarkGiftAcid>>(darkGiftAcids))
                if (now + 1e-9 >= pair.Value.expires) RemoveKnightAcid(pair.Key);
            foreach (var effect in new List<DarkGiftEffect>(darkGiftEffects))
            {
                while (!effect.rules.Completed && effect.next <= now + 1e-9)
                {
                    effect.next += .5;
                    var candidates = new List<OriginalDarkGiftCandidate>();
                    foreach (var actor in world.Snapshot().units) candidates.Add(new OriginalDarkGiftCandidate(actor.entityId, actor.ownerSlot, actor.rawcode,
                        actor.position, darkGiftAcids.ContainsKey(actor.entityId), darkGiftResistance.Contains(actor.entityId)));
                    foreach (var instruction in effect.rules.Tick(candidates))
                    {
                        int id = instruction.entityId; var actor = world.UnitState(id);
                        if (actor == null) continue;
                        switch (instruction.kind)
                        {
                            case OriginalDarkGiftInstructionKind.AddResistance:
                                SetNativeSpellResistance(id, "A0E5", combatCatalog.Ability("A0E5").Number("DataB1"));
                                darkGiftResistance.Add(id); break;
                            case OriginalDarkGiftInstructionKind.RemoveResistance:
                                RemoveNativeSpellResistance(id, "A0E5");
                                darkGiftResistance.Remove(id); break;
                            case OriginalDarkGiftInstructionKind.RemoveAcidBuff: RemoveKnightAcid(id); break;
                            case OriginalDarkGiftInstructionKind.CastAcidBomb:
                                // Ydv requests on corpses/hidden units too. Native
                                // organic friendly target eligibility determines effect.
                                if (actor.health <= .405 || actor.hidden || actor.invulnerable) break;
                                darkGiftAcids[id] = new DarkGiftAcid { rank = effect.rank, expires = now + 20 };
                                RefreshAbilityMovement(id); RescaleWeaponRate(id, now); break;
                        }
                    }
                }
                if (effect.rules.Completed) darkGiftEffects.Remove(effect);
            }
        }
        void RemoveKnightAcid(int id)
        {
            if (!darkGiftAcids.Remove(id)) return;
            RefreshAbilityMovement(id); RescaleWeaponRate(id, world.Clock);
        }
        double KnightArmorBonus(int id) => darkGiftAcids.TryGetValue(id, out var acid) ? -combatCatalog.Ability("A0UU").Number("DataC" + acid.rank) : 0;
        double KnightAttackSpeedBonus(int id) => darkGiftAcids.TryGetValue(id, out var acid) ? combatCatalog.Ability("A0UU").Number("DataB" + acid.rank) : 0;
        double KnightMovementBonus(int id) => darkGiftAcids.TryGetValue(id, out var acid) ? combatCatalog.Ability("A0UU").Number("DataA" + acid.rank) : 0;
        double KnightSpellResistance(int id) => darkGiftResistance.Contains(id) ? 1 - combatCatalog.Ability("A0E5").Number("DataB1") : 1;
    }
}

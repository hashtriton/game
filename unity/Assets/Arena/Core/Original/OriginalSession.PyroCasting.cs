using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        sealed class PyroCast
        {
            internal int actor, owner, rank, targetId;
            internal string ability;
            internal OriginalPoint target;
            internal double effectAt, manaCost, cooldown;
        }
        readonly Dictionary<int, PyroCast> pyroCasts = new Dictionary<int, PyroCast>();
        readonly Dictionary<int, Dictionary<string, double>> pyroCooldowns = new Dictionary<int, Dictionary<string, double>>();
        readonly HashSet<int> pyroSphereChannels = new HashSet<int>();

        bool PyroControlsActor(int id) => pyroCasts.ContainsKey(id);
        void CancelPyroCastOnOrder(int id) { pyroCasts.Remove(id); pyroSphereChannels.Remove(id); }
        bool PyroPostEffectChannel(int id) => pyroSphereChannels.Contains(id);
        void ResetPyroCooldowns(int slot) => pyroCooldowns.Remove(slot);
        double PyroCooldown(int slot, string ability) => pyroCooldowns.TryGetValue(slot, out var rows) && rows.TryGetValue(CanonicalPyroAbility(ability), out double until)
            ? Math.Max(0, until - world.Clock) : 0;

        // PYCAST2 campaign2ab133a8d91e077b25dd757ea940842eeefb42ce9490aae4cdbea499027925ec:
        // H024 ANcl effects at.1; A0SN Absk effect/debit0 is immediate.
        // Source removes A0SN at the next detonation callback, so it has no
        // persistent cooldown carried into the next added helper instance.
        bool PyroCastAvailable(string ability) => (ability == "A0SJ" || ability == "A0SN" || ability == "A0SP" || ability == "A0SO" || ability == "A0AE" || ability == "A0SM" || ability == "A0SR" || ability == "A0SS") &&
            TriggeredDamageModeAvailable(OriginalTriggeredDamageMode.SpellMagic);
        double PyroManaCost(string ability, int rank)
        {
            var declaration = combatCatalog.Ability(ability);
            if (ability != "A0SN" && ability != "A0SO") return declaration.Number("Cost" + rank);
            if (rank != 1) throw new InvalidOperationException("Unobserved secondary Pyro rank.");
            // Zero is measured for these exact helper instances, never a sparse
            // field fallback. An explicit conflicting declaration is rejected.
            foreach (var field in declaration.fields)
                if (field.key == "Cost1" && (!field.isNumber || field.conflict || field.number != 0))
                    throw new InvalidOperationException("Observed secondary mana conflicts with map.");
            foreach (var field in declaration.overrides)
                if (field.field == "amcs" && field.level == 1 && (!field.isNumber || field.number != 0))
                    throw new InvalidOperationException("Observed secondary mana conflicts with binary map.");
            return 0;
        }
        double PyroDeclaredCooldown(string ability, int rank)
        {
            if (ability == "A0SN") return 0;
            if (ability != "A0SO") return combatCatalog.Ability(ability).Number("Cool" + rank);
            // Exact rank1 ANcl inherited Cool1=0: priority War3Patch.mpq
            // Units/AbilityData.slk:13348 (also War3x:8188). Rapid recasts are
            // compatible evidence, not a standalone measurement of zero.
            var declaration = combatCatalog.Ability(ability);
            if (rank != 1 || declaration.Text("code") != "ANcl") throw new InvalidOperationException("Unknown secondary sphere cooldown.");
            foreach (var field in declaration.fields)
                if (field.key == "Cool1" && (!field.isNumber || field.conflict || field.number != 0))
                    throw new InvalidOperationException("Secondary cooldown conflicts with inherited native value.");
            foreach (var field in declaration.overrides)
                if (field.field == "acdn" && field.level == 1 && (!field.isNumber || field.number != 0))
                    throw new InvalidOperationException("Secondary cooldown conflicts with binary map.");
            return 0;
        }

        OriginalSessionReplyCode CastPyroAbility(Player player, OriginalSessionCommand command, OriginalAbilityView view)
        {
            if (!PyroCastAvailable(view.castAbilityId)) return OriginalSessionReplyCode.RuleUnavailable;
            var actor = world.UnitState(OriginalWorld.HeroEntityId(player.slot));
            if (actor == null || actor.health <= 0 || actor.hidden || actor.paused || pyroCasts.ContainsKey(actor.entityId))
                return OriginalSessionReplyCode.NotReady;
            bool point = view.targetMode == OriginalAbilityTargetMode.Point;
            if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0 || point && !ValidPoint(command.x, command.y))
                return OriginalSessionReplyCode.InvalidCommand;
            var declaration = combatCatalog.Ability(view.castAbilityId);
            int nativeRank = view.castAbilityId == "A0SN" || view.castAbilityId == "A0SO" ? 1 : view.rank;
            double cost = PyroManaCost(view.castAbilityId,nativeRank), cooldown = PyroDeclaredCooldown(view.castAbilityId,nativeRank);
            if (point && SquaredDistance(actor.position, new OriginalPoint(command.x, command.y)) > Math.Pow(declaration.Number("Rng" + nativeRank), 2))
                return OriginalSessionReplyCode.NotReady;
            if (actor.mana < cost || PyroCooldown(player.slot, view.castAbilityId) > 1e-9) return OriginalSessionReplyCode.NotReady;
            if(view.castAbilityId=="A0AE") ValidatePyroAura();
            if(CanonicalPyroAbility(view.castAbilityId)=="A0SM")
            {
                PyroMeteorRank(player,out string helper,out int helperRank);
                if(!PyroFlameStrikeAvailable(helper,helperRank))return OriginalSessionReplyCode.RuleUnavailable;
            }
            var cast = new PyroCast { actor = actor.entityId, owner = player.slot, rank = view.rank, ability = view.castAbilityId,
                target = new OriginalPoint(command.x, command.y), targetId = 0, manaCost = cost, cooldown = cooldown,
                effectAt = world.Clock + (view.castAbilityId == "A0SN" ? 0 : combatCatalog.Unit(player.hero).Number("castpt")) };
            // PYCASTC campaign0c11029f308e: even instant A0SN changes a native
            // attack order851983 to0. An already released missile survives.
            world.Stop(actor.entityId);
            if (weaponCycles.TryGetValue(actor.entityId, out var cycle)) cycle.winding = false;
            if (point) world.SetFacing(actor.entityId, Math.Atan2(cast.target.y - actor.position.y, cast.target.x - actor.position.x) * 180 / Math.PI);
            world.MarkCast(actor.entityId); pyroCasts.Add(actor.entityId, cast);
            if (cast.effectAt <= world.Clock) AdvancePyroCasts();
            return OriginalSessionReplyCode.Accepted;
        }

        void AdvancePyroCasts()
        {
            foreach (var cast in new List<PyroCast>(pyroCasts.Values))
            {
                var actor = world.UnitState(cast.actor);
                if (actor == null || actor.health <= 0 || actor.hidden || actor.paused)
                { pyroCasts.Remove(cast.actor); continue; }
                if (world.Clock + 1e-9 < cast.effectAt) continue;
                if (!PyroCastAvailable(cast.ability)) throw new InvalidOperationException("pyro-native-closure-unavailable:" + cast.ability);
                if (!world.TrySpendMana(cast.actor, cast.manaCost)) { pyroCasts.Remove(cast.actor); continue; }
                pyroCasts.Remove(cast.actor);
                if (!pyroCooldowns.TryGetValue(cast.owner, out var cooldowns)) pyroCooldowns.Add(cast.owner, cooldowns = new Dictionary<string, double>(StringComparer.Ordinal));
                cooldowns[CanonicalPyroAbility(cast.ability)] = cast.effectAt + cast.cooldown;
                // PYCASTC: SO has no FINISH before a recast/Stop/attack. Keep
                // its post-effect order separate from the pre-effect lock so
                // a new explicit command can interrupt it. Natural timeout
                // remains unmeasured and is not claimed as native parity.
                if (cast.ability == "A0SO") pyroSphereChannels.Add(cast.actor);
                if (cast.ability == "A0AE") nativeAttackRecovery[cast.actor] = cast.effectAt + .77; // PYCAST2 FINISH=.87.
                // Existing projectile registrations receive this spell event;
                // a sphere launched by this event registers only afterward.
                NotifyNativeSpellEffect(cast.actor, cast.ability);
                switch (CanonicalPyroAbility(cast.ability))
                {
                    case "A0SJ": BeginPyroVacuum(cast.actor, cast.rank, cast.target); break;
                    case "A0SN": DetonatePyroVacuum(cast.actor); break;
                    case "A0SP": BeginPyroSpheres(cast.actor); break;
                    case "A0SO": LaunchPyroSphere(cast.actor, cast.target, cast.targetId); break;
                    case "A0AE": BeginPyroPortal(cast.actor, cast.rank, cast.target); break;
                    case "A0SM": BeginPyroMeteor(cast.actor, cast.target); break;
                    default: throw new InvalidOperationException("pyro-effect-driver-unavailable:" + cast.ability);
                }
            }
        }
    }
}

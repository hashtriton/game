using System;

namespace Arena.Original
{
    public sealed partial class OriginalSession
    {
        // CASTAPPROACH1 measures item orders walking before EFFECT/debit.
        // Applying the same host intent to supported hero/summon spell orders
        // is a family policy. Their existing target/range/effect rules remain
        // authoritative; this preflight never moves or commits a spell.
        OriginalSessionReplyCode PlanSkillCastApproach(Player player, OriginalSessionCommand command,
            out CastApproachTarget target)
        {
            target = null;
            if (player == null || command == null || command.kind != OriginalSessionCommandKind.CastSkill ||
                command.skillId == null || command.skillId.Length != 4)
                return OriginalSessionReplyCode.InvalidCommand;
            if (!Started || world == null || pendingDuel || match.Phase == OriginalMatchPhase.Won || match.Phase == OriginalMatchPhase.Lost)
                return OriginalSessionReplyCode.NotReady;
            int actorId = command.actorEntityId == 0 ? OriginalWorld.HeroEntityId(player.slot) : command.actorEntityId;
            var actor = world.UnitState(actorId);
            if (actor == null || actor.ownerSlot != player.slot)
                return OriginalSessionReplyCode.InvalidCommand;
            try
            {
                if (actor.kind == OriginalWorldUnitKind.Summon)
                    return PlanSummonCastApproach(actor, command, out target);
                if (actor.kind != OriginalWorldUnitKind.Hero || actorId != OriginalWorld.HeroEntityId(player.slot))
                    return OriginalSessionReplyCode.InvalidCommand;
                if (player.progression == null) return OriginalSessionReplyCode.NotReady;
                var view = Array.Find(AbilityViews(player), ability => ability.castAbilityId == command.skillId);
                if (view == null) return OriginalSessionReplyCode.InvalidCommand;
                if (view.code == OriginalAbilityUseCode.RuleUnavailable) return OriginalSessionReplyCode.RuleUnavailable;
                if (view.code != OriginalAbilityUseCode.Ready) return OriginalSessionReplyCode.NotReady;
                if (!view.implemented) return OriginalSessionReplyCode.RuleUnavailable;
                if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0)
                    return OriginalSessionReplyCode.InvalidCommand;
                if (view.targetMode == OriginalAbilityTargetMode.None) return OriginalSessionReplyCode.Accepted;
                if (view.targetMode != OriginalAbilityTargetMode.Point) return OriginalSessionReplyCode.RuleUnavailable;
                if (!ValidPoint(command.x, command.y)) return OriginalSessionReplyCode.InvalidCommand;
                double range;
                if (player.hero == "N0A0" && view.id == "A15W")
                    range = new OriginalArcherCastRules(combatCatalog, view.castAbilityId, view.rank).range;
                else if (player.hero == "H024")
                {
                    if (!PyroCastAvailable(view.castAbilityId)) return OriginalSessionReplyCode.RuleUnavailable;
                    int nativeRank = view.castAbilityId == "A0SO" ? 1 : view.rank;
                    range = combatCatalog.Ability(view.castAbilityId).Number("Rng" + nativeRank);
                    if (view.castAbilityId == "A0AE") ValidatePyroAura();
                    if (CanonicalPyroAbility(view.castAbilityId) == "A0SM")
                    {
                        PyroMeteorRank(player, out string helper, out int helperRank);
                        if (!PyroFlameStrikeAvailable(helper, helperRank)) return OriginalSessionReplyCode.RuleUnavailable;
                    }
                }
                else return OriginalSessionReplyCode.RuleUnavailable;
                if (!OriginalCombatDefinition.IsFinite(range) || range <= 0) return OriginalSessionReplyCode.RuleUnavailable;
                target = new CastApproachTarget { actor = actorId, point = new OriginalPoint(command.x, command.y), range = range };
                return OriginalSessionReplyCode.Accepted;
            }
            catch (InvalidOperationException) { return OriginalSessionReplyCode.RuleUnavailable; }
        }

        OriginalSessionReplyCode PlanSummonCastApproach(OriginalWorldUnitView actor, OriginalSessionCommand command,
            out CastApproachTarget target)
        {
            target = null;
            var view = Array.Find(SummonAbilityViews(actor), ability => ability.id == command.skillId);
            if (view == null) return OriginalSessionReplyCode.InvalidCommand;
            if (view.code == OriginalAbilityUseCode.RuleUnavailable) return OriginalSessionReplyCode.RuleUnavailable;
            if (view.code != OriginalAbilityUseCode.Ready) return OriginalSessionReplyCode.NotReady;
            if (!view.implemented) return OriginalSessionReplyCode.RuleUnavailable;
            var declaration = combatCatalog.Ability(view.id);
            var caster = OriginalCasterRules.ForUnit(combatCatalog, actor.rawcode);
            bool scripted = caster != null && caster.abilityId == view.id;
            double range;
            if (scripted)
            {
                ValidateCasterHelper(caster);
                range = caster.castRange;
            }
            else if (!SummonAbilityRules(actor, declaration, out _, out _, out range, out _))
                return OriginalSessionReplyCode.RuleUnavailable;
            ValidateSummonSourceAbility(view.id);
            if (view.targetMode == OriginalAbilityTargetMode.None)
                return command.targetKind == OriginalWorldTargetKind.None && command.targetId == 0
                    ? OriginalSessionReplyCode.Accepted : OriginalSessionReplyCode.InvalidCommand;
            if (!OriginalCombatDefinition.IsFinite(range) || range <= 0) return OriginalSessionReplyCode.RuleUnavailable;
            if (view.targetMode == OriginalAbilityTargetMode.Unit)
            {
                var victim = world.UnitState(command.targetId);
                if (command.targetKind != OriginalWorldTargetKind.Unit || !SummonAbilityTarget(actor, victim, view.id))
                    return OriginalSessionReplyCode.InvalidCommand;
                target = new CastApproachTarget { actor = actor.entityId, unitTarget = victim.entityId,
                    point = victim.position, range = range };
                return OriginalSessionReplyCode.Accepted;
            }
            if (view.targetMode != OriginalAbilityTargetMode.Point) return OriginalSessionReplyCode.RuleUnavailable;
            if (command.targetKind != OriginalWorldTargetKind.None || command.targetId != 0 || !ValidPoint(command.x, command.y))
                return OriginalSessionReplyCode.InvalidCommand;
            target = new CastApproachTarget { actor = actor.entityId, point = new OriginalPoint(command.x, command.y), range = range };
            return OriginalSessionReplyCode.Accepted;
        }
    }
}

using System;
using System.Collections.Generic;

namespace Arena.Original
{
    public enum OriginalVisualEffectKind { WarningCircle, ActiveCircle, Orb, Beam, Ghost, Rectangle }
    [Serializable]
    public sealed class OriginalVisualEffectView
    {
        public OriginalVisualEffectKind kind;
        public string abilityId;
        public int sourceEntityId, variant;
        public OriginalPoint position, end;
        public double radius, progress;
    }
    public sealed partial class OriginalSession
    {
        // A complete detached drawing state, never a gameplay/event channel.
        // Array indices are not effect identities: clients must not interpolate
        // a reused entry from its previous position after removal/reordering.
        OriginalVisualEffectView[] VisualEffects()
        {
            if (world == null) return Array.Empty<OriginalVisualEffectView>();
            var output = new List<OriginalVisualEffectView>();
            foreach (var effect in casterEffects)
                output.Add(new OriginalVisualEffectView { kind = effect.warning ? OriginalVisualEffectKind.WarningCircle : OriginalVisualEffectKind.ActiveCircle,
                    abilityId = effect.rules.abilityId, sourceEntityId = effect.actor,
                    position = effect.center, radius = effect.rules.radius,
                    progress = effect.warning && effect.rules.warningSeconds > 0 ?
                        Math.Max(0, Math.Min(1, 1 - (effect.nextAt - world.Clock) / effect.rules.warningSeconds)) : 1 });
            foreach (var charge in bossCharges.Values)
            {
                var actor = world.UnitState(charge.actor); if (actor == null) continue;
                double angle = charge.rule.Facing * Math.PI / 180;
                output.Add(new OriginalVisualEffectView { kind = OriginalVisualEffectKind.Beam, abilityId = "A101",
                    sourceEntityId = charge.actor, position = actor.position,
                    end = new OriginalPoint(actor.position.x + 588 * Math.Cos(angle), actor.position.y + 588 * Math.Sin(angle)),
                    radius = 200, variant = charge.rule.Charging ? 1 : 0 });
            }
            AppendBossGhostVisuals(output);
            AppendItemScriptActVisuals(output);
            AppendItemFortitudeVisuals(output);
            AppendFlameBootVisuals(output);
            AppendItemChannelVisuals(output);
            AppendBossRainVisuals(output);
            AppendBossWindVisuals(output);
            AppendBossQuadrantVisuals(output);
            AppendBossMeteorVisuals(output);
            AppendBossInfernoVisuals(output);
            AppendBossShockwaveVisuals(output);
            AppendBossBarrageVisuals(output);
            AppendBossRiftVisuals(output);
            AppendBossSilenceVisuals(output);
            AppendBossDeathFingerVisuals(output);
            AppendBossStormVisuals(output);
            AppendBossBurstVisuals(output);
            AppendOrnGhostVisuals(output);
            AppendOrnAbilityVisuals(output);
            AppendUniqueSoulVisuals(output);
            AppendWaveSpellVisuals(output);
            AppendPyroVisualEffects(output);
            return output.ToArray();
        }
        partial void AppendPyroVisualEffects(List<OriginalVisualEffectView> output);
    }
}


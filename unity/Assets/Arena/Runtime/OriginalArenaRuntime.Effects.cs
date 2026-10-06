using System.Collections.Generic;
using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        sealed class EffectVisual
        {
            internal LineRenderer line;
            internal GameObject orb;
            internal MaterialPropertyBlock properties = new MaterialPropertyBlock();
        }
        readonly List<EffectVisual> effectVisuals = new List<EffectVisual>();

        void UpdateAbilityVisuals()
        {
            var effects = View?.effects;
            int count = effects?.Length ?? 0;
            while (effectVisuals.Count < count) effectVisuals.Add(new EffectVisual());
            for (int i = 0; i < effectVisuals.Count; i++)
            {
                var visual = effectVisuals[i];
                if (i >= count) { if (visual.line) visual.line.enabled = false; if (visual.orb) visual.orb.SetActive(false); continue; }
                var effect = effects[i];
                bool orb = effect.kind == OriginalVisualEffectKind.Orb;
                if (visual.line) visual.line.enabled = !orb;
                if (visual.orb) visual.orb.SetActive(orb);
                Color color = EffectColor(effect);
                if (orb)
                {
                    if (!visual.orb) visual.orb = ArenaEffects.Orb(transform, effectMaterial, Vector3.zero, 1, color);
                    visual.orb.transform.position = WorldPoint(effect.position) + Vector3.up * .8f;
                    visual.orb.transform.localScale = Vector3.one * Mathf.Clamp((float)(effect.radius * 2 / map.unitsPerMeter), .18f, 3);
                    ApplyEffectColor(visual.orb.GetComponent<Renderer>(), visual.properties, color);
                    continue;
                }
                if (!visual.line)
                {
                    var go = new GameObject("Source ability effect"); go.transform.SetParent(transform, false);
                    visual.line = go.AddComponent<LineRenderer>(); visual.line.sharedMaterial = effectMaterial;
                    visual.line.useWorldSpace = true; visual.line.numCornerVertices = 3; visual.line.numCapVertices = 3;
                    visual.line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    visual.line.receiveShadows = false;
                }
                var line = visual.line; line.enabled = true;
                line.widthMultiplier = effect.kind == OriginalVisualEffectKind.ActiveCircle ? .13f : .08f;
                line.startColor = line.endColor = color; ApplyEffectColor(line, visual.properties, color);
                if (effect.kind == OriginalVisualEffectKind.Rectangle)
                {
                    line.loop = true; line.positionCount = 4; line.widthMultiplier = .16f;
                    line.SetPosition(0, WorldPoint(effect.position) + Vector3.up * .13f);
                    line.SetPosition(1, WorldPoint(new OriginalPoint(effect.end.x, effect.position.y)) + Vector3.up * .13f);
                    line.SetPosition(2, WorldPoint(effect.end) + Vector3.up * .13f);
                    line.SetPosition(3, WorldPoint(new OriginalPoint(effect.position.x, effect.end.y)) + Vector3.up * .13f);
                }
                else if (effect.kind == OriginalVisualEffectKind.Beam || effect.kind == OriginalVisualEffectKind.Ghost)
                {
                    var a = WorldPoint(effect.position) + Vector3.up * .12f;
                    var b = WorldPoint(effect.end) + Vector3.up * .12f;
                    var direction = b - a; direction.y = 0;
                    if (direction.sqrMagnitude < .0001f) direction = Vector3.forward;
                    var side = Vector3.Cross(direction.normalized, Vector3.up) * (float)(effect.radius / map.unitsPerMeter);
                    line.loop = true; line.positionCount = 5;
                    line.SetPosition(0, a - side); line.SetPosition(1, b - side);
                    line.SetPosition(2, b + direction.normalized * .35f);
                    line.SetPosition(3, b + side); line.SetPosition(4, a + side);
                }
                else
                {
                    line.loop = true; line.positionCount = 64;
                    for (int j = 0; j < 64; j++)
                    {
                        double angle = j * 2 * System.Math.PI / 64;
                        line.SetPosition(j, WorldPoint(new OriginalPoint(effect.position.x + effect.radius * System.Math.Cos(angle),
                            effect.position.y + effect.radius * System.Math.Sin(angle))) + Vector3.up * .1f);
                    }
                }
            }
        }
        static Color EffectColor(OriginalVisualEffectView effect)
        {
            if (effect.kind == OriginalVisualEffectKind.Rectangle)
                return effect.variant == 1 ? new Color(.2f, 1f, .45f) : new Color(1f, .15f, .3f);
            if (effect.kind == OriginalVisualEffectKind.WarningCircle || effect.kind == OriginalVisualEffectKind.Beam && effect.variant == 0)
                return Color.Lerp(new Color(1f, .72f, .18f), new Color(1f, .16f, .08f), (float)effect.progress);
            if (effect.abilityId == "A0SJ" || effect.abilityId == "A0SN") return new Color(.48f, .3f, 1f);
            if (effect.abilityId == "A0AE") return new Color(.18f, .77f, 1f);
            if (effect.abilityId == "A12Y" || effect.abilityId == "A12Z" || effect.abilityId == "A0CC") return new Color(.2f, 1f, .65f);
            if (effect.abilityId == "A17V") return new Color(.56f, .95f, .16f);
            if (effect.abilityId == "A0TX") return new Color(.82f, .18f, .58f);
            if (effect.abilityId == "A0C5") return effect.variant == 1 ? new Color(1f, .6f, .2f) : new Color(.3f, .72f, 1f);
            if (effect.abilityId == "A0M9" || effect.abilityId == "A11S" || effect.abilityId == "A0KP") return new Color(.45f, .55f, 1f);
            if (effect.abilityId == "A104") return new Color(1f, .87f, .52f);
            return new Color(1f, .31f, .08f);
        }
        static void ApplyEffectColor(Renderer renderer, MaterialPropertyBlock block, Color color)
        {
            block.SetColor("_BaseColor", color); block.SetColor("_Color", color); renderer.SetPropertyBlock(block);
        }
        void DestroyAbilityVisuals()
        {
            foreach (var visual in effectVisuals)
            { if (visual.line) Destroy(visual.line.gameObject); if (visual.orb) Destroy(visual.orb); }
            effectVisuals.Clear();
        }
    }
}

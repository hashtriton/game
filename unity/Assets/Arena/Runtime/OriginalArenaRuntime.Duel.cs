using Arena.Original;
using UnityEngine;

namespace Arena
{
    public sealed partial class OriginalArenaRuntime
    {
        LineRenderer duelRing;
        Material duelRingMaterial;
        long cameraDuelId = -1;
        bool cameraWasInDuel;
        OriginalPoint? previousLocalPosition;
        void UpdatePhaseCamera()
        {
            if (!cameraControl || View == null || !View.started)
            {
                cameraDuelId = -1; cameraWasInDuel = false; previousLocalPosition = null;
                return;
            }
            bool inDuel = View.hasDuel && View.duel != null && View.duel.phase != OriginalDuelPhase.Completed;
            if (inDuel && (View.duelId != cameraDuelId || !cameraWasInDuel))
            {
                // The source pair-prepare camera applies to spectators as well.
                cameraControl.CenterAt(WorldPoint(new OriginalPoint(0, -2688)), 38);
                cameraDuelId = View.duelId;
            }
            else if (!inDuel && LocalUnit != null && !LocalUnit.hidden)
            {
                bool relocated = false;
                if (previousLocalPosition.HasValue)
                {
                    double dx = LocalUnit.position.x - previousLocalPosition.Value.x;
                    double dy = LocalUnit.position.y - previousLocalPosition.Value.y;
                    relocated = dx * dx + dy * dy > 1024 * 1024;
                }
                if (cameraWasInDuel || relocated)
                    cameraControl.CenterAt(WorldPoint(LocalUnit.position));
            }
            cameraWasInDuel = inDuel;
            if (LocalUnit != null) previousLocalPosition = LocalUnit.position;
        }
        void UpdateDuelRingVisual()
        {
            var duel = View?.duel;
            bool visible = duel != null && duel.ringStage > 0 && duel.ringRadius > 0;
            if (!visible) { if (duelRing) duelRing.enabled = false; return; }
            if (!duelRing)
            {
                var go = new GameObject("Duel boundary"); go.transform.SetParent(transform, false);
                duelRing = go.AddComponent<LineRenderer>(); duelRing.loop = true; duelRing.useWorldSpace = true;
                duelRing.positionCount = 96; duelRing.widthMultiplier = .09f;
                duelRing.numCornerVertices = 3; duelRing.numCapVertices = 3;
                duelRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                duelRing.receiveShadows = false;
                if (effectMaterial) { duelRingMaterial = new Material(effectMaterial); duelRing.sharedMaterial = duelRingMaterial; }
            }
            duelRing.enabled = true;
            var color = duel.ringStage == 1 ? new Color(.12f, .75f, 1f) : new Color(1f, .31f, .1f);
            duelRing.startColor = duelRing.endColor = color;
            if (duelRingMaterial)
            {
                if (duelRingMaterial.HasProperty("_BaseColor")) duelRingMaterial.SetColor("_BaseColor", color);
                if (duelRingMaterial.HasProperty("_Color")) duelRingMaterial.SetColor("_Color", color);
            }
            for (int i = 0; i < duelRing.positionCount; i++)
            {
                double angle = i * 2 * System.Math.PI / duelRing.positionCount;
                // LiA3.9c hs uses the authored arena centre (0,-2700).
                var p = new OriginalPoint(duel.ringRadius * System.Math.Cos(angle), -2700 + duel.ringRadius * System.Math.Sin(angle));
                duelRing.SetPosition(i, WorldPoint(p) + Vector3.up * .08f);
            }
        }
        void DestroyDuelRingVisual()
        {
            if (duelRing) Destroy(duelRing.gameObject);
            if (duelRingMaterial) Destroy(duelRingMaterial);
        }
    }
}

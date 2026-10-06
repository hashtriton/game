using System;
using System.Collections.Generic;
using Arena.Original;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Arena
{
    // This is a presenter and input adapter. Positions, life and attack decisions
    // come exclusively from the session; interpolation changes only the picture.
    [DisallowMultipleComponent]
    public sealed partial class OriginalArenaRuntime : MonoBehaviour
    {
        public OriginalNetworkGame network;
        public ArenaMap map;
        public Camera viewCamera;
        public OriginalArenaCamera cameraControl;
        public GameObject heroPrefab, meleePrefab, wellPrefab;
        public Material effectMaterial;
        public bool InputBlocked { get; set; }
        public bool AttackTargetArmed { get; private set; }
        public string Notice { get; private set; }
        public Transform Hero { get; private set; }
        public OriginalSessionView View => network ? network.View : null;
        public OriginalWorldUnitView LocalUnit { get; private set; }

        sealed class Visual
        {
            public ArenaActor actor;
            public OriginalWorldUnitView state;
            public long attackSequence;
            public long castSequence;
            public bool dead;
        }
        sealed class Scenery
        {
            public GameObject root;
            public Bounds bounds;
            public bool initiallyActive;
        }
        readonly Dictionary<int, Visual> actors = new Dictionary<int, Visual>();
        readonly Dictionary<int, Scenery> scenery = new Dictionary<int, Scenery>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly HashSet<int> presentScenery = new HashSet<int>();
        readonly List<int> removed = new List<int>();
        readonly List<RaycastResult> uiHits = new List<RaycastResult>();
        Transform actorRoot;
        OriginalWorldSnapshot observedWorld;
        PointerEventData pointer;
        EventSystem pointerSystem;

        void Awake()
        {
            if (!network) network = GetComponent<OriginalNetworkGame>();
            if (!viewCamera) viewCamera = Camera.main;
            if (!cameraControl && viewCamera) cameraControl = viewCamera.GetComponent<OriginalArenaCamera>();
            actorRoot = new GameObject("Authoritative arena actors").transform;
            actorRoot.SetParent(transform, false);
            IndexScenery();
        }

        void OnEnable()
        {
            if (network) network.CommandReplied += OnReply;
        }

        void OnDisable()
        {
            if (network) network.CommandReplied -= OnReply;
            AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0;
        }

        void OnDestroy()
        {
            ClearWorldItems();
            DestroyDuelRingVisual();
            DestroyAbilityVisuals();
            if (actorRoot) Destroy(actorRoot.gameObject);
        }

        public bool Host(OriginalMatchOptions options, int port, string bindAddress)
        {
            if (!network || !map || network.State != OriginalConnectionState.Disconnected) return false;
            try
            {
                // A fresh adapter resets the original destructable pathing masks.
                network.ConfigureWorld(new OriginalMapNavigation(map));
                Notice = null;
                return network.Host(options, Environment.TickCount, port, bindAddress);
            }
            catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException)
            {
                Notice = exception.Message;
                return false;
            }
        }

        public void Disconnect()
        {
            if (network) network.Disconnect();
            ClearActors(); RestoreScenery(); Notice = null; AttackTargetArmed = false;
        }

        void OnReply(OriginalNetworkResponse reply)
        {
            Notice = reply.code == OriginalSessionReplyCode.Accepted ? null : "Команда отклонена: " + reply.code;
        }

        void Update()
        {
            if (!network || !map || !viewCamera) return;
            if (network.IsHost)
            {
                // The network component only pumps transport. This is the sole
                // frame clock. Long stalls slow the simulation instead of a burst
                // of catch-up attacks. Session subdivides its own bounded steps.
                network.AdvanceHost(Math.Min(.05, Math.Max(0, Time.unscaledDeltaTime)));
                network.DrainHostEvents();
            }
            var world = View != null && View.hasWorld ? View.world : null;
            if (!ReferenceEquals(world, observedWorld))
            {
                if (world == null) { ClearActors(); RestoreScenery(); }
                else ApplySnapshot(world);
                observedWorld = world;
            }
            AnimateActors();
            UpdateWorldItems();
            UpdatePhaseCamera();
            UpdateDuelRingVisual();
            UpdateAbilityVisuals();
            bool pointerOverUi = PointerOverUi();
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            var field = selected ? selected.GetComponent<UnityEngine.UI.InputField>() : null;
            bool typing = field && field.isActiveAndEnabled && field.isFocused;
            if (cameraControl) cameraControl.inputBlocked = InputBlocked || typing || pointerOverUi;
            if (InputBlocked || typing || View == null || !View.started || !string.IsNullOrEmpty(View.haltReason))
            { AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0; return; }
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) SelectOwnedUnit(network.LocalSlot);
            if (SelectedUnit == null || SelectedUnit.health <= 0 || SelectedUnit.hidden)
            {
                AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0;
                if (!pointerOverUi && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) SelectPointerUnit();
                if (SelectedUnit == null || SelectedUnit.health <= 0 || SelectedUnit.hidden) return;
            }
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) { AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0; }
                if (keyboard.sKey.wasPressedThisFrame)
                {
                    AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0; network.SendCommand(OriginalSessionCommandKind.Stop, actorEntityId: SelectedUnit.entityId);
                }
                if (keyboard.hKey.wasPressedThisFrame)
                {
                    AttackTargetArmed = false; ArmedSkillId = null; ArmedItemInstance = 0; network.SendCommand(OriginalSessionCommandKind.HoldPosition, actorEntityId: SelectedUnit.entityId);
                }
                if (keyboard.aKey.wasPressedThisFrame) { AttackTargetArmed = true; ArmedSkillId = null; ArmedItemInstance = 0; }
                if (keyboard.qKey.wasPressedThisFrame) RequestSkill(0);
                if (keyboard.wKey.wasPressedThisFrame) RequestSkill(1);
                if (keyboard.eKey.wasPressedThisFrame) RequestSkill(2);
                if (keyboard.rKey.wasPressedThisFrame) RequestSkill(3);
            }
            var mouse = Mouse.current;
            if (mouse == null || pointerOverUi) return;
            if (mouse.rightButton.wasPressedThisFrame)
            {
                if (ArmedSkillId != null || ArmedItemInstance != 0) { ArmedSkillId = null; ArmedItemInstance = 0; return; }
                AttackTargetArmed = false; IssuePointerOrder(false);
            }
            else if (mouse.leftButton.wasPressedThisFrame)
            {
                if (ArmedItemInstance != 0) IssuePointerItem();
                else if (ArmedSkillId != null) IssuePointerSkill();
                else if (AttackTargetArmed) { if (IssuePointerOrder(true)) AttackTargetArmed = false; }
                else SelectPointerUnit();
            }
        }

        void ApplySnapshot(OriginalWorldSnapshot world)
        {
            seen.Clear(); LocalUnit = null;
            foreach (var unit in world.units)
            {
                seen.Add(unit.entityId);
                if (!actors.TryGetValue(unit.entityId, out var visual))
                {
                    visual = new Visual(); actors.Add(unit.entityId, visual);
                }
                bool wasVisible=visual.actor&&visual.actor.gameObject.activeSelf;
                visual.state = unit;
                if (unit.health > 0 && (!visual.actor || visual.dead))
                {
                    if (visual.actor) Destroy(visual.actor.gameObject);
                    visual.actor = CreateActor(unit); visual.dead = false;
                    visual.attackSequence = unit.attackSequence;
                    visual.castSequence = unit.castSequence;
                }
                if (unit.health <= 0 && !visual.dead)
                {
                    visual.dead = true;
                    if (visual.actor) visual.actor.Die();
                }
                if (visual.actor)
                {
                    bool visible=UnitVisible(unit);
                    if(visible&&!wasVisible)
                    {
                        visual.actor.transform.position=WorldPoint(unit.position);
                        visual.attackSequence=unit.attackSequence;visual.castSequence=unit.castSequence;
                    }
                    visual.actor.gameObject.SetActive(visible);
                }
                if (visual.actor && !visual.dead && UnitVisible(unit) && unit.attackSequence > visual.attackSequence)
                {
                    // Cosmetic clip duration only; the server controls hit timing.
                    visual.actor.Attack(.45f); visual.attackSequence = unit.attackSequence;
                }
                if (visual.actor && !visual.dead && UnitVisible(unit) && unit.castSequence > visual.castSequence)
                { visual.actor.Attack(.35f); visual.castSequence = unit.castSequence; }
                if (unit.kind == OriginalWorldUnitKind.Hero && unit.ownerSlot == network.LocalSlot) LocalUnit = unit;
            }
            RefreshSelectedUnit(world);
            removed.Clear();
            foreach (var pair in actors) if (!seen.Contains(pair.Key)) removed.Add(pair.Key);
            foreach (int id in removed)
            {
                if (actors[id].actor) Destroy(actors[id].actor.gameObject);
                actors.Remove(id);
            }
            ApplyDynamicDoodadSnapshot(world);
            presentScenery.Clear();
            foreach (var doodad in world.doodads)
            {
                presentScenery.Add(doodad.editorId);
                if (scenery.TryGetValue(doodad.editorId, out var visual) && visual.root)
                    visual.root.SetActive(doodad.health > 0 && visual.initiallyActive);
            }
            foreach(var prop in scenery)
                if(!presentScenery.Contains(prop.Key)&&prop.Value.root)prop.Value.root.SetActive(false);
            Transform nextHero = LocalUnit != null && actors.TryGetValue(LocalUnit.entityId, out var local) && local.actor ? local.actor.transform : null;
            if (nextHero != Hero)
            {
                Hero = nextHero;
                if (cameraControl) { cameraControl.hero = Hero; cameraControl.CenterHero(); }
            }
        }

        ArenaActor CreateActor(OriginalWorldUnitView unit)
        {
            bool hero = unit.ownerSlot != 0;
            var go = new GameObject((hero ? "Hero " : "Creep ") + unit.entityId + " / " + unit.rawcode);
            go.transform.SetParent(actorRoot, false);
            go.transform.position = WorldPoint(unit.position);
            var actor = go.AddComponent<ArenaActor>();
            actor.Initialize(hero ? heroPrefab : meleePrefab, hero ? 1.9f : 1.6f,
                (float)(unit.profile.collisionRadius / map.unitsPerMeter), hero, effectMaterial);
            return actor;
        }

        void AnimateActors()
        {
            float blend = 1 - Mathf.Exp(-18 * Time.unscaledDeltaTime);
            foreach (var visual in actors.Values)
            {
                if (!visual.actor || visual.dead || !UnitVisible(visual.state)) continue;
                var actor = visual.actor;
                Vector3 previous = actor.transform.position, target = WorldPoint(visual.state.position);
                actor.transform.position = (target - previous).sqrMagnitude > 64 ? target : Vector3.Lerp(previous, target, blend);
                Vector3 movement = actor.transform.position - previous;
                actor.SetMoving(movement.sqrMagnitude > .000001f);
                if (visual.state.order == OriginalWorldOrder.AttackTarget && TryTargetPoint(visual.state, out var aim))
                    actor.Face(aim - actor.transform.position, Time.unscaledDeltaTime);
                else if (visual.state.hasFacing)
                {
                    float angle = (float)visual.state.facingDegrees * Mathf.Deg2Rad;
                    actor.Face(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)), Time.unscaledDeltaTime);
                }
                else actor.Face(movement, Time.unscaledDeltaTime);
            }
        }

        bool TryTargetPoint(OriginalWorldUnitView unit, out Vector3 point)
        {
            if (unit.targetKind == OriginalWorldTargetKind.Unit && actors.TryGetValue(unit.targetId, out var target))
            { point = WorldPoint(target.state.position); return true; }
            if (unit.targetKind == OriginalWorldTargetKind.Doodad && scenery.TryGetValue(unit.targetId, out var prop))
            { point = prop.bounds.center; return true; }
            point = default; return false;
        }

        public Vector3 UnitScreenPoint(int entityId)
        {
            if (!viewCamera || !actors.TryGetValue(entityId, out var visual) || !visual.actor || !UnitVisible(visual.state)) return new Vector3(0, 0, -1);
            return viewCamera.WorldToScreenPoint(visual.actor.transform.position + Vector3.up * (visual.state.ownerSlot != 0 ? 2.2f : 1.9f));
        }

        public Vector3 WorldPoint(OriginalPoint point) => map.WcToWorld((float)point.x, (float)point.y);

        void FindPointerCombatTarget(Ray ray,out OriginalWorldTargetKind kind,out int targetId,out float nearest)
        {
            nearest = float.PositiveInfinity;
            kind = OriginalWorldTargetKind.None;
            targetId = 0;
            foreach (var pair in actors)
            {
                var visual = pair.Value;
                if (visual.dead || !visual.actor || !UnitVisible(visual.state) || visual.state.ownerSlot == network.LocalSlot) continue;
                float height = visual.state.ownerSlot != 0 ? 1.9f : 1.6f;
                float width = Mathf.Max(.55f, visual.actor.Radius * 2);
                var bounds = new Bounds(visual.actor.transform.position + Vector3.up * (height * .5f), new Vector3(width, height, width));
                if (bounds.IntersectRay(ray, out float distance) && distance < nearest)
                { nearest = distance; kind = OriginalWorldTargetKind.Unit; targetId = pair.Key; }
            }
            var world = View != null && View.hasWorld ? View.world : null;
            if (world != null) foreach (var doodad in world.doodads)
            {
                if (doodad.health <= 0 || doodad.invulnerable || !scenery.TryGetValue(doodad.editorId, out var prop) || !prop.root || !prop.root.activeInHierarchy) continue;
                if (prop.bounds.IntersectRay(ray, out float distance) && distance < nearest)
                { nearest = distance; kind = OriginalWorldTargetKind.Doodad; targetId = doodad.editorId; }
            }
        }

        bool IssuePointerOrder(bool requireTarget)
        {
            var ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            FindPointerCombatTarget(ray,out var kind,out int targetId,out float nearest);
            if(!requireTarget&&FindWorldInteraction(ray,out long item,out bool well,out float interactionDistance)&&interactionDistance<nearest)
                return SendWorldInteraction(item,well);
            if (kind != OriginalWorldTargetKind.None)
                return network.SendCommand(OriginalSessionCommandKind.AttackTarget, targetKind: kind, targetId: targetId, actorEntityId: SelectedUnit.entityId);
            // Measured terrain has its own TerrainCollider. Render models do not
            // participate in movement validation; that belongs to the authority.
            foreach (var hit in Physics.RaycastAll(ray, 1000))
            {
                if (!(hit.collider is TerrainCollider)) continue;
                var point = hit.point;
                return network.SendCommand(requireTarget ? OriginalSessionCommandKind.AttackMove : OriginalSessionCommandKind.Move,
                    x: point.x * map.unitsPerMeter, y: point.z * map.unitsPerMeter, actorEntityId: SelectedUnit.entityId);
            }
            return false;
        }

        bool PointerOverUi()
        {
            if (!EventSystem.current || Mouse.current == null) return false;
            if (pointer == null || pointerSystem != EventSystem.current)
            { pointerSystem = EventSystem.current; pointer = new PointerEventData(pointerSystem); }
            pointer.position = Mouse.current.position.ReadValue(); uiHits.Clear();
            pointerSystem.RaycastAll(pointer, uiHits); return uiHits.Count != 0;
        }

        void IndexScenery()
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
            {
                string name = candidate.name;
                if (!(name.StartsWith("LTbr#", StringComparison.Ordinal) || name.StartsWith("LTbs#", StringComparison.Ordinal) || name.StartsWith("LTex#", StringComparison.Ordinal))) continue;
                int end = name.IndexOf(' ', 5);
                if (!int.TryParse(end < 0 ? name.Substring(5) : name.Substring(5, end - 5), out int id)) continue;
                var renderers = candidate.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                scenery[id] = new Scenery { root = candidate.gameObject, bounds = bounds, initiallyActive = candidate.gameObject.activeSelf };
            }
        }

        void RestoreScenery()
        {
            ClearDynamicScenery();
            foreach (var prop in scenery.Values) if (prop.root) prop.root.SetActive(prop.initiallyActive);
        }

        void ClearActors()
        {
            ClearWorldItems();
            ClearDynamicScenery();
            foreach (var visual in actors.Values) if (visual.actor) Destroy(visual.actor.gameObject);
            actors.Clear(); observedWorld = null; LocalUnit = null; SelectedUnit = null; selectedEntityId = 0; ArmedSkillId = null; ArmedItemInstance = 0; Hero = null;
            if (cameraControl) cameraControl.hero = null;
        }
    }
}

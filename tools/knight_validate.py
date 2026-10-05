"""Read-only geometry QA for a saved knight-v3 scene, in background Blender.

Writes only verification/knight-v3-<source stem>.json. Render settings are
mirrored into the temporary process's viewport depsgraph; no scene or user
preferences are saved. Visual quality and game readiness are separate checks.
"""
from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
from validate_anvil_trial import bounds, external_files, inspect_mesh

ROOT = Path(__file__).resolve().parents[1]
MODEL_ROOT = ROOT / "art/heroes/knight-v3"
GEOMETRY_TYPES = {"MESH", "CURVE", "SURFACE", "FONT", "META"}
DEFECT_KEYS = (
    "nonfinite_vertices", "nonfinite_triangles", "near_zero_area_triangles",
    "invalid_polygon_normals", "missing_material_faces", "boundary_edges",
    "nonmanifold_edges_more_than_two_faces", "inconsistent_winding_edges",
    "negative_closed_components",
)


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def json_safe(value):
    if isinstance(value, float) and not math.isfinite(value):
        return None
    if isinstance(value, dict):
        return {k: json_safe(v) for k, v in value.items()}
    if isinstance(value, (list, tuple)):
        return [json_safe(v) for v in value]
    return value


def main():
    if not bpy.app.background:
        raise RuntimeError("Use a separate background Blender process")
    source = Path(bpy.data.filepath).resolve()
    if source.parent != MODEL_ROOT.resolve() or source.suffix.lower() != ".blend":
        raise RuntimeError("Load a saved .blend directly from art/heroes/knight-v3")
    before = sha256(source)
    scene = bpy.context.scene
    errors, warnings = [], []
    scale = scene.unit_settings.scale_length
    if not math.isfinite(scale) or scale <= 0:
        raise RuntimeError("Invalid scene unit scale")
    if abs(scale - 1) > 1e-6:
        errors.append("Expected unit scale 1 meter per Blender unit")
    if len(bpy.data.scenes) != 1:
        errors.append("Expected exactly one scene")
    if len(scene.view_layers) != 1:
        warnings.append("Only the active view layer is evaluated")
    knight = bpy.data.collections.get("KNIGHT")
    if knight is None:
        errors.append("KNIGHT collection is missing")
    members = set(knight.all_objects) if knight else set()
    visible = set()
    collection_visibility = []

    def visit(layer, enabled=True):
        enabled = enabled and not layer.exclude and not layer.collection.hide_render
        collection_visibility.append({"name": layer.collection.name,
                                      "render_enabled_path": enabled,
                                      "excluded": layer.exclude,
                                      "hide_render": layer.collection.hide_render})
        if enabled:
            visible.update(o for o in layer.collection.objects if not o.hide_render)
            layer.collection.hide_viewport = False
            layer.hide_viewport = False
        for child in layer.children:
            visit(child, enabled)

    visit(bpy.context.view_layer.layer_collection)
    selected = sorted(members & visible, key=lambda o: o.name)
    geometry = [o for o in selected if o.type in GEOMETRY_TYPES]
    adjustments, modifier_records, boolean_operands = [], [], []
    for obj in scene.objects:
        # Hidden operands can still contribute to a Boolean, so match modifier
        # flags for all scene dependencies, not just visible character pieces.
        if obj in visible:
            obj.hide_viewport = False
            obj.hide_set(False)
        for mod in obj.modifiers:
            if obj in members:
                modifier_records.append({"object": obj.name, "modifier": mod.name,
                                         "type": mod.type, "show_render": mod.show_render,
                                         "show_viewport_before": mod.show_viewport,
                                         "levels_before": getattr(mod, "levels", None),
                                         "render_levels": getattr(mod, "render_levels", None)})
            if mod.show_viewport != mod.show_render:
                adjustments.append({"object": obj.name, "modifier": mod.name,
                                    "property": "show_viewport", "value": mod.show_render})
                mod.show_viewport = mod.show_render
            if mod.type in {"SUBSURF", "MULTIRES"} and mod.levels != mod.render_levels:
                adjustments.append({"object": obj.name, "modifier": mod.name,
                                    "property": "levels", "value": mod.render_levels})
                mod.levels = mod.render_levels
            if obj in members and mod.show_render and mod.type in {"NODES", "PARTICLE_SYSTEM"}:
                errors.append(obj.name + ": exact render geometry unsupported for " + mod.type)
            if obj in members and mod.show_render and mod.type == "BOOLEAN":
                operands = ([mod.object] if mod.object else []) if mod.operand_type == "OBJECT" else (
                    list(mod.collection.all_objects) if mod.collection else [])
                if not operands:
                    errors.append(obj.name + ": enabled Boolean has no operand: " + mod.name)
                for operand in operands:
                    boolean_operands.append({"object": obj.name, "modifier": mod.name,
                                             "operand": operand.name, "render_visible": operand in visible})
                    if operand in visible:
                        warnings.append(obj.name + ": Boolean operand also renders: " + operand.name)
    for obj in selected:
        if obj.is_instancer:
            errors.append(obj.name + ": instancing is outside exact-count scope")
        if obj.type not in GEOMETRY_TYPES | {"EMPTY", "ARMATURE"}:
            errors.append(obj.name + ": unsupported visible object type " + obj.type)
    bpy.context.view_layer.update()
    graph = bpy.context.evaluated_depsgraph_get()
    meshes, all_points, body_points = [], [], []
    for obj in geometry:
        try:
            item, points = inspect_mesh(obj, graph, scale)
        except Exception as exc:
            errors.append(obj.name + ": inspection failed: " + repr(exc))
            continue
        item["collections"] = [c.name for c in obj.users_collection]
        meshes.append(item)
        all_points.extend(points)
        if "WEAPON" not in item["collections"]:
            body_points.extend(points)
        for key in DEFECT_KEYS:
            if item[key]:
                errors.append(f"{obj.name}: {key}={item[key]}")
        if not item["evaluated_triangles"]:
            errors.append(obj.name + ": empty render-visible geometry")
        if any(uv["nonfinite"] for uv in item["uv_layers"]):
            errors.append(obj.name + ": nonfinite UV coordinates")
        if item["loose_edges"] or item["loose_vertices"]:
            warnings.append(obj.name + ": loose non-surface geometry")
        if item["unapplied_object_scale"] or item["nonunit_world_scale"]:
            warnings.append(obj.name + ": nonunit scale; measurements include world transformation")
    triangles = sum(m["evaluated_triangles"] for m in meshes)
    if not meshes:
        errors.append("No evaluated render-visible character geometry")
    if len(geometry) > 350 or triangles > 350000:
        errors.append(f"Character budget exceeded: {len(geometry)} objects, {triangles} triangles")
    overall = bounds(all_points)
    body = bounds(body_points)
    if body and not 1.9 <= body["size"][2] <= 2.5:
        errors.append("Character height excluding WEAPON is outside 1.9..2.5 meters")
    boots = [m for m in meshes if m["name"].lower().startswith("boot sole")]
    ground = [{"name": m["name"], "minimum_z_m": m["bounds_m"]["min"][2],
               "target_z_m": .025, "tolerance_m": .01,
               "pass": abs(m["bounds_m"]["min"][2] - .025) <= .010001}
              for m in boots if m["bounds_m"]]
    if len(ground) != 2:
        errors.append("Expected two render-visible Boot sole meshes for independent foot contact checks")
    for contact in ground:
        if not contact["pass"]:
            errors.append(contact["name"] + ": sole is not at Z=.025 within 1 cm")
    if overall and overall["min"][2] < -.001:
        errors.append("Character or weapon extends below Z=0")
    dependencies = external_files()
    for dependency in dependencies:
        if not dependency["available"]:
            errors.append("Missing external dependency: " + dependency["path"])
    uv_missing = [m["name"] for m in meshes if not m["uv_layers"]]
    if uv_missing:
        warnings.append(f"{len(uv_missing)} meshes have no UV layer; permitted for a procedural visual study")
    outside = sorted(o.name for o in visible - members if o.type in GEOMETRY_TYPES)
    if outside:
        warnings.append("Render-visible geometry outside KNIGHT is excluded; review as studio content: " + ", ".join(outside))
    after = sha256(source)
    if after != before:
        errors.append("Source file changed during validation")
    report = {
        "checked_at_utc": datetime.now(timezone.utc).isoformat(),
        "blender_version": bpy.app.version_string, "source_file": str(source),
        "source_sha256_before": before, "source_sha256_after": after, "source_unchanged": before == after,
        "scene_count": len(bpy.data.scenes), "unit_scale": scale, "collection": "KNIGHT",
        "render_visible_geometry_objects": len(geometry), "evaluated_triangles": triangles,
        "budgets": {"objects": 350, "evaluated_triangles": 350000},
        "near_zero_area_threshold_m2": 1e-12,
        "totals": {key: sum(m[key] for m in meshes) for key in DEFECT_KEYS},
        "bounds_m": overall, "body_bounds_excluding_weapon_m": body, "ground_contacts": ground,
        "meshes": meshes, "objects_without_uv": uv_missing, "external_files": dependencies,
        "render_visible_geometry_outside_knight": outside,
        "hidden_knight_objects": sorted(o.name for o in members - visible),
        "collection_visibility": collection_visibility, "modifiers": modifier_records,
        "boolean_operands": boolean_operands,
        "evaluation": {"depsgraph_mode": graph.mode, "temporary_render_adjustments": adjustments},
        "errors": errors, "warnings": warnings, "technical_pass": not errors,
        "limits": ["No self-intersection or inter-object collision test",
                   "No visual, pose, silhouette, rig, UV completeness or game-readiness acceptance",
                   "Solidify cloth is expected to be closed after evaluation",
                   "Negative volume is per closed consistently wound connected surface; deliberate nested cavities need review",
                   "Viewport depsgraph with render flags and subdivision levels mirrored in memory; render engine displacement is not included",
                   "File existence does not validate every sequence frame, UDIM tile or dependency contents"],
    }
    destination = ROOT / "verification" / ("knight-v3-" + source.stem.removeprefix("knight-") + ".json")
    destination.write_text(json.dumps(json_safe(report), ensure_ascii=False, indent=2, allow_nan=False) + "\n", encoding="utf-8")
    print("KNIGHT_QA " + json.dumps({"report": str(destination), "technical_pass": not errors,
          "objects": len(geometry), "triangles": triangles, "source_unchanged": before == after,
          "errors": errors, "warnings": warnings}))
    if errors:
        raise RuntimeError("Knight geometry QA failed; see " + str(destination))


if __name__ == "__main__":
    main()

"""Inspect a saved anvil trial in a separate background Blender process.

Only verification/anvil-<variant>-<stem>.json is written. The loaded scene is
never saved and preferences are not changed. This is geometry QA, not art or
game-readiness acceptance. Render modifier flags are mirrored in memory for
evaluated meshes because the Python context provides a viewport depsgraph.
"""
from collections import defaultdict
from datetime import datetime, timezone
import glob
import hashlib
import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[1]
TRIAL_ROOT = ROOT / "art/tests/anvil-low"
AREA_EPSILON_M2 = 1e-12
GEOMETRY_TYPES = {"MESH", "CURVE", "SURFACE", "FONT", "META"}


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def bounds(points):
    finite = [p for p in points if all(math.isfinite(v) for v in p)]
    if not finite:
        return None
    lo = [min(p[i] for p in finite) for i in range(3)]
    hi = [max(p[i] for p in finite) for i in range(3)]
    return {"min": lo, "max": hi, "size": [hi[i] - lo[i] for i in range(3)]}


def inspect_mesh(obj, graph, unit_scale):
    evaluated = obj.evaluated_get(graph)
    mesh = evaluated.to_mesh(preserve_all_data_layers=True, depsgraph=graph)
    if mesh is None:
        raise RuntimeError("Object did not produce an evaluated mesh")
    try:
        mesh.calc_loop_triangles()
        points = [(evaluated.matrix_world @ v.co) * unit_scale for v in mesh.vertices]
        finite = [all(math.isfinite(c) for c in p) for p in points]
        parent = list(range(len(mesh.polygons)))

        def root(index):
            while parent[index] != index:
                parent[index] = parent[parent[index]]
                index = parent[index]
            return index

        def join(a, b):
            a, b = root(a), root(b)
            if a != b:
                parent[b] = a

        edge_faces = defaultdict(list)
        for polygon in mesh.polygons:
            ids = list(polygon.vertices)
            for a, b in zip(ids, ids[1:] + ids[:1]):
                key = tuple(sorted((a, b)))
                edge_faces[key].append((polygon.index, 1 if a < b else -1))
        for users in edge_faces.values():
            for face, _ in users[1:]:
                join(users[0][0], face)
        components = {}
        for polygon in mesh.polygons:
            key = root(polygon.index)
            components.setdefault(key, {"faces": 0, "closed": True,
                                       "winding_consistent": True,
                                       "signed_volume_m3": 0.0})["faces"] += 1
        boundary = sum(len(users) == 1 for users in edge_faces.values())
        nonmanifold = sum(len(users) > 2 for users in edge_faces.values())
        winding = sum(len(users) == 2 and users[0][1] == users[1][1]
                      for users in edge_faces.values())
        for users in edge_faces.values():
            key = root(users[0][0])
            if len(users) != 2:
                components[key]["closed"] = False
            if len(users) == 2 and users[0][1] == users[1][1]:
                components[key]["winding_consistent"] = False
        actual_edges = {tuple(sorted(e.vertices)) for e in mesh.edges}
        loose_edges = len(actual_edges - edge_faces.keys())
        used_vertices = {v for p in mesh.polygons for v in p.vertices}
        loose_vertices = len(mesh.vertices) - len(used_vertices)

        near_zero = 0
        invalid_triangles = 0
        minimum_area = None
        tiny_samples = []
        origin = next((p for p, ok in zip(points, finite) if ok), Vector((0, 0, 0)))
        for tri in mesh.loop_triangles:
            if not all(finite[i] for i in tri.vertices):
                invalid_triangles += 1
                continue
            a, b, c = (points[i] for i in tri.vertices)
            area = (b - a).cross(c - a).length * 0.5
            minimum_area = area if minimum_area is None else min(minimum_area, area)
            if area <= AREA_EPSILON_M2:
                near_zero += 1
                if len(tiny_samples) < 12:
                    tiny_samples.append({"triangle": tri.index, "polygon": tri.polygon_index,
                                         "area_m2": area})
            component = components[root(tri.polygon_index)]
            component["signed_volume_m3"] += (a - origin).dot((b - origin).cross(c - origin)) / 6
        for component in components.values():
            # Open surfaces have origin-dependent volume; do not interpret it.
            if not component["closed"] or not component["winding_consistent"] or invalid_triangles:
                component["signed_volume_m3"] = None

        missing_material_faces = sum(
            p.material_index >= len(evaluated.material_slots)
            or evaluated.material_slots[p.material_index].material is None
            for p in mesh.polygons
        )
        normals_invalid = sum(not all(math.isfinite(v) for v in p.normal)
                              or p.normal.length_squared < 1e-20 for p in mesh.polygons)
        uv = []
        for layer in mesh.uv_layers:
            nonfinite_uv = sum(not all(math.isfinite(v) for v in loop.uv) for loop in layer.data)
            uv.append({"name": layer.name, "loops": len(layer.data), "nonfinite": nonfinite_uv})
        component_list = list(components.values())
        negative = [c for c in component_list if c["signed_volume_m3"] is not None
                    and c["signed_volume_m3"] < -1e-15]
        matrix_scale = list(obj.matrix_world.to_scale())
        result = {
            "name": obj.name, "source_type": obj.type,
            "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
            "evaluated_triangles": len(mesh.loop_triangles), "bounds_m": bounds(points),
            "nonfinite_vertices": finite.count(False), "nonfinite_triangles": invalid_triangles,
            "near_zero_area_triangles": near_zero, "minimum_triangle_area_m2": minimum_area,
            "near_zero_samples": tiny_samples, "invalid_polygon_normals": normals_invalid,
            "missing_material_faces": missing_material_faces,
            "materials": [slot.material.name if slot.material else None for slot in evaluated.material_slots],
            "uv_layers": uv, "boundary_edges": boundary,
            "nonmanifold_edges_more_than_two_faces": nonmanifold,
            "inconsistent_winding_edges": winding, "loose_edges": loose_edges,
            "loose_vertices": loose_vertices, "connected_surface_components": component_list,
            "negative_closed_components": len(negative),
            "object_scale": list(obj.scale), "world_scale": matrix_scale,
            "world_determinant": obj.matrix_world.to_3x3().determinant(),
            "unapplied_object_scale": any(abs(v - 1) > 1e-6 for v in obj.scale),
            "nonunit_world_scale": any(abs(v - 1) > 1e-6 for v in matrix_scale),
        }
        return result, points
    finally:
        evaluated.to_mesh_clear()


def external_files():
    result = []
    for value in sorted(set(bpy.utils.blend_paths(absolute=True, packed=False, local=False))):
        # UDIM and image-sequence templates are dependencies, not literal names.
        pattern = value.replace("<UDIM>", "[0-9][0-9][0-9][0-9]").replace("<UVTILE>", "u*_v*")
        available = bool(glob.glob(pattern)) if pattern != value else Path(value).exists()
        result.append({"path": value, "available": available,
                       "template": pattern != value})
    return result


def main():
    if not bpy.app.background:
        raise RuntimeError("Use a separate background Blender process")
    source = Path(bpy.data.filepath).resolve()
    if (source.suffix.lower() != ".blend" or source.parent.parent != TRIAL_ROOT.resolve()
            or source.parent.name not in {"astra-low", "sol-low"}):
        raise RuntimeError("Load a saved .blend from an anvil-low variant directory")
    before = sha256(source)
    variant = source.parent.name
    stage = "blockout" if "blockout" in source.stem.lower() else "final"
    object_budget = 12 if stage == "blockout" else 120
    triangle_budget = 20000 if stage == "blockout" else 100000
    errors, warnings = [], []
    scene = bpy.context.scene
    unit_scale = scene.unit_settings.scale_length
    if not math.isfinite(unit_scale) or unit_scale <= 0:
        raise RuntimeError("Invalid unit scale")
    if abs(unit_scale - 1) > 1e-6:
        errors.append("Brief requires one Blender unit per meter; scale_length is not 1")
    if len(bpy.data.scenes) != 1:
        errors.append("Brief requires exactly one scene")
    if len(scene.view_layers) != 1:
        warnings.append("Only the active view layer is evaluated; other render layers need separate review")
    asset = bpy.data.collections.get("ASSET")
    if asset is None:
        errors.append("ASSET collection is missing")
    members = set(asset.all_objects) if asset else set()
    visible = set()

    def visit(layer, enabled=True):
        enabled = enabled and not layer.exclude and not layer.collection.hide_render
        if enabled:
            visible.update(o for o in layer.collection.objects if not o.hide_render)
            # These temporary visibility changes never get saved.
            layer.collection.hide_viewport = False
            layer.hide_viewport = False
        for child in layer.children:
            visit(child, enabled)

    visit(bpy.context.view_layer.layer_collection)
    selected = sorted(members & visible, key=lambda o: o.name)
    geometry = [o for o in selected if o.type in GEOMETRY_TYPES]
    unsupported = [o.name for o in selected if o.type not in GEOMETRY_TYPES
                   and o.type not in {"EMPTY", "ARMATURE"}]
    if unsupported:
        errors.append("Unsupported render-visible ASSET objects: " + ", ".join(unsupported))
    instancers = [o.name for o in selected if o.is_instancer]
    if instancers:
        errors.append("Instanced geometry is outside this validator's exact-count scope: " + ", ".join(instancers))
    adjustments = []
    for obj in selected:
        obj.hide_viewport = False
        obj.hide_set(False)
        for modifier in obj.modifiers:
            if modifier.show_viewport != modifier.show_render:
                adjustments.append({"object": obj.name, "modifier": modifier.name,
                                    "property": "show_viewport", "value": modifier.show_render})
                modifier.show_viewport = modifier.show_render
            if modifier.type == "SUBSURF" and modifier.levels != modifier.render_levels:
                adjustments.append({"object": obj.name, "modifier": modifier.name,
                                    "property": "levels", "value": modifier.render_levels})
                modifier.levels = modifier.render_levels
            if modifier.type in {"NODES", "MULTIRES", "PARTICLE_SYSTEM"}:
                warnings.append(obj.name + ": render-dependent modifier needs separate render evaluation: " + modifier.type)
    bpy.context.view_layer.update()
    graph = bpy.context.evaluated_depsgraph_get()
    meshes, all_points = [], []
    for obj in geometry:
        try:
            item, points = inspect_mesh(obj, graph, unit_scale)
        except Exception as exc:
            errors.append(obj.name + ": inspection failed: " + repr(exc))
            continue
        meshes.append(item)
        all_points.extend(points)
        for key in ("nonfinite_vertices", "nonfinite_triangles", "near_zero_area_triangles",
                    "invalid_polygon_normals", "missing_material_faces", "boundary_edges",
                    "nonmanifold_edges_more_than_two_faces", "inconsistent_winding_edges",
                    "negative_closed_components"):
            if item[key]:
                errors.append(f"{obj.name}: {key}={item[key]}")
        if not item["evaluated_triangles"]:
            errors.append(obj.name + ": empty render-visible geometry")
        if any(layer["nonfinite"] for layer in item["uv_layers"]):
            errors.append(obj.name + ": nonfinite UV coordinates")
        if item["loose_edges"] or item["loose_vertices"]:
            warnings.append(obj.name + ": loose geometry does not form rendered faces; remove or justify it")
        if item["unapplied_object_scale"] or item["nonunit_world_scale"]:
            warnings.append(obj.name + ": scale is not applied; evaluated dimensions remain measured in meters")
    if not meshes:
        errors.append("No evaluated render-visible ASSET geometry")
    triangles = sum(m["evaluated_triangles"] for m in meshes)
    if len(geometry) > object_budget:
        errors.append(f"Visible geometry object budget exceeded: {len(geometry)} > {object_budget}")
    if triangles > triangle_budget:
        errors.append(f"Evaluated triangle budget exceeded: {triangles} > {triangle_budget}")
    overall = bounds(all_points)
    dimensions = []
    if overall:
        for name, measured, expected in (("overall_X_length", overall["size"][0], 1.10),
                                         ("overall_Z_height", overall["size"][2], 0.90)):
            passed = abs(measured - expected) <= expected * .10 + 1e-6
            dimensions.append({"name": name, "measured_m": measured, "expected_m": expected,
                               "tolerance_percent": 10, "pass": passed})
            if not passed:
                errors.append(name + ": outside brief's 10 percent tolerance")
        if abs(overall["min"][2]) > 0.001:
            errors.append("Bottom is not on Z=0 within a 1 mm numerical placement allowance")
    dependencies = external_files()
    for dependency in dependencies:
        if not dependency["available"]:
            errors.append("Missing external dependency: " + dependency["path"])
    uv_missing = [m["name"] for m in meshes if not m["uv_layers"]]
    if uv_missing:
        warnings.append(f"{len(uv_missing)} evaluated meshes have no UV layer; allowed for this procedural-material study")
    outside = sorted(o.name for o in visible - members if o.type in GEOMETRY_TYPES)
    if outside:
        warnings.append("Render-visible geometry outside ASSET is excluded; confirm it is studio-only: " + ", ".join(outside))
    after = sha256(source)
    if before != after:
        errors.append("Source .blend changed during validation")
    report = {
        "checked_at_utc": datetime.now(timezone.utc).isoformat(),
        "blender_version": bpy.app.version_string, "source_file": str(source),
        "source_sha256_before": before, "source_sha256_after": after, "source_unchanged": before == after,
        "variant": variant, "stage": stage, "scene": scene.name,
        "scene_count": len(bpy.data.scenes), "active_view_layer": bpy.context.view_layer.name,
        "collection": "ASSET", "unit_scale": unit_scale,
        "render_visible_geometry_objects": len(geometry), "evaluated_triangles": triangles,
        "budgets": {"visible_geometry_objects": object_budget, "evaluated_triangles": triangle_budget},
        "near_zero_area_threshold_m2": AREA_EPSILON_M2,
        "totals": {key: sum(m[key] for m in meshes) for key in (
            "nonfinite_vertices", "near_zero_area_triangles", "invalid_polygon_normals",
            "missing_material_faces", "boundary_edges", "nonmanifold_edges_more_than_two_faces",
            "inconsistent_winding_edges", "negative_closed_components")},
        "bounds_m": overall, "dimension_checks": dimensions,
        "objects_without_uv": uv_missing, "external_files": dependencies,
        "render_visible_geometry_outside_asset": outside,
        "hidden_asset_objects": sorted(o.name for o in members - visible),
        "evaluation": {"depsgraph_mode": graph.mode, "temporary_render_flag_adjustments": adjustments},
        "meshes": meshes, "errors": errors, "warnings": warnings,
        "technical_pass": not errors,
        "interpretation": {
            "boundary_and_nonmanifold": "Errors for the solid anvil and stump brief; legitimate intentionally open detail would need explicit review",
            "signed_volume": "Per closed consistently wound connected surface, in world meters; negative components flagged for inward orientation or deliberate cavities requiring review",
            "uv": "Absence is allowed; no UV completeness or game-readiness claim",
            "limits": ["No self-intersection or inter-object collision test", "No art or silhouette acceptance",
                       "No automatic semantic measurement of stump top, anvil face width, holes or horn direction",
                       "No export or game-readiness validation", "Render-specific Geometry Nodes, particles, displacement and instancing are not fully evaluated",
                       "External path existence does not validate every sequence frame, UDIM tile or linked asset contents"],
        },
    }
    destination = ROOT / "verification" / f"anvil-{variant}-{source.stem}.json"
    destination.write_text(json.dumps(report, indent=2, ensure_ascii=False, allow_nan=False) + "\n", encoding="utf-8")
    print("ANVIL_TRIAL_CHECK " + json.dumps({"report": str(destination), "technical_pass": not errors,
          "objects": len(geometry), "triangles": triangles, "source_unchanged": before == after,
          "errors": errors, "warnings": warnings}))
    if errors:
        raise RuntimeError("Technical inspection failed; see " + str(destination))


if __name__ == "__main__":
    main()

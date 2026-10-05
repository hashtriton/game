"""Inspect a saved Veteran model study using evaluated geometry, without exporting.

Run in a separate Blender process:
  blender --background --factory-startup --disable-autoexec --offline-mode \
    art/heroes/veteran/veteran.blend --python-exit-code 1 \
    --python tools/validate_veteran.py

Only verification/veteran-review.json is written. No scene, mesh, preferences,
UVs or exports are saved or changed. Missing UVs are reported as a study
limitation, not treated as a failed geometry check.
"""
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path
import traceback

import bmesh
import bpy


ROOT = Path(__file__).resolve().parents[1]
REPORT_PATH = ROOT / 'verification' / 'veteran-review.json'
GEOMETRY_TYPES = {'MESH', 'CURVE', 'SURFACE', 'FONT', 'META'}
AREA_EPSILON = 1e-12
REPORT = {
    'asset': 'veteran', 'asset_stage': 'Editable static Blender model study',
    'reviewed_at_utc': datetime.now(timezone.utc).isoformat(),
    'blender_version': bpy.app.version_string,
    'export_performed': False, 'game_ready_claim': False,
    'findings': [],
    'limitations': [
        'Evaluated dependency-graph geometry is inspected, including converted curves; original data is not modified.',
        'No export, texture baking, UV packing, rigging, animation or Unity import was performed.',
        'Independent parts and open trim endpoints may overlap intentionally. Exhaustive self-intersection and inter-object intersection checks are not performed.',
        'UV island overlap is not tested; UV absence is acceptable for this model study and remains an export preparation task.',
        'Observed build/head source hashes record files at validation time, not proof that those exact sources generated the saved blend.',
    ],
}


def finding(severity, code, message, **details):
    REPORT['findings'].append({'severity': severity, 'code': code, 'message': message, **details})


def file_hash(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def finite(vector):
    return all(math.isfinite(float(value)) for value in vector)


def empty_bounds():
    return {'min': [math.inf]*3, 'max': [-math.inf]*3}


def extend_bounds(bounds, point):
    for axis in range(3):
        bounds['min'][axis] = min(bounds['min'][axis], point[axis])
        bounds['max'][axis] = max(bounds['max'][axis], point[axis])


def finalized_bounds(bounds):
    return bounds if finite(bounds['min'] + bounds['max']) else None


def material_report(material):
    if material.node_tree is None:
        return {'nodes': [], 'connected_principled_inputs': [], 'procedural_nodes': [],
                'image_textures': [], 'principled_count': 0}
    nodes = material.node_tree.nodes
    principled = [node for node in nodes if node.type == 'BSDF_PRINCIPLED']
    procedural = [node.type for node in nodes if node.type in {
        'TEX_NOISE', 'TEX_VORONOI', 'TEX_WAVE', 'TEX_MAGIC', 'TEX_CHECKER',
        'TEX_GRADIENT', 'BUMP', 'VALTORGB'}]
    return {
        'nodes': sorted(set(node.type for node in nodes)),
        'principled_count': len(principled),
        'connected_principled_inputs': sorted({socket.name for node in principled
                                               for socket in node.inputs if socket.is_linked}),
        'procedural_nodes': sorted(set(procedural)),
        'image_textures': [{'name': node.image.name if node.image else None,
                            'filepath': node.image.filepath if node.image else None,
                            'packed': bool(node.image and node.image.packed_file)}
                           for node in nodes if node.type == 'TEX_IMAGE'],
    }


def inspect_geometry(obj, depsgraph):
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh(preserve_all_data_layers=True, depsgraph=depsgraph)
    if mesh is None:
        return {'object': obj.name, 'source_type': obj.type, 'evaluation_failed': True}
    try:
        mesh.calc_loop_triangles()
        world = evaluated.matrix_world
        points = [world @ vertex.co for vertex in mesh.vertices]
        bbox = empty_bounds()
        for point in points:
            if finite(point):
                extend_bounds(bbox, point)
        report = {
            'object': obj.name, 'source_type': obj.type,
            'source_vertices': len(obj.data.vertices) if obj.type == 'MESH' else None,
            'evaluated_vertices': len(mesh.vertices), 'evaluated_polygons': len(mesh.polygons),
            'evaluated_triangles': len(mesh.loop_triangles), 'bounds_blender_units': finalized_bounds(bbox),
            'nonfinite_coordinates': sum(not finite(point) for point in points),
            'nonfinite_transform': not finite(value for row in world for value in row),
            'nonfinite_face_normals': sum(not finite(poly.normal) for poly in mesh.polygons),
            'zero_face_normals': sum(poly.normal.length < 1e-6 for poly in mesh.polygons),
            'nonfinite_corner_normals': sum(not finite(normal.vector) for normal in mesh.corner_normals),
            'zero_corner_normals': sum(normal.vector.length < 1e-6 for normal in mesh.corner_normals),
            'degenerate_triangles': 0, 'degenerate_triangle_examples': [],
            'missing_material_faces': 0, 'materials': [mat.name if mat else None for mat in mesh.materials],
            'uv_layers': len(mesh.uv_layers), 'nonfinite_uv_loops': 0,
            'modifiers': [{'name': mod.name, 'type': mod.type, 'viewport': mod.show_viewport,
                           'render': mod.show_render} for mod in obj.modifiers],
        }
        for triangle in mesh.loop_triangles:
            a, b, c = [points[index] for index in triangle.vertices]
            area = (b-a).cross(c-a).length*.5
            if area <= AREA_EPSILON:
                report['degenerate_triangles'] += 1
                if len(report['degenerate_triangle_examples']) < 4:
                    report['degenerate_triangle_examples'].append({
                        'polygon': triangle.polygon_index, 'area_blender_units_squared': area,
                        'vertices': [list(a), list(b), list(c)]})
        report['missing_material_faces'] = sum(
            poly.material_index >= len(mesh.materials) or mesh.materials[poly.material_index] is None
            for poly in mesh.polygons)
        for layer in mesh.uv_layers:
            report['nonfinite_uv_loops'] += sum(not finite(loop.uv) for loop in layer.data)
        # Coincident positions are diagnostic only: UV seams and detached pieces
        # can be legitimate. Do not merge or repair the source to test them.
        finite_points = [point for point in points if finite(point)]
        report['coincident_vertex_candidates_1e_7'] = len(finite_points)-len({
            tuple(round(value, 7) for value in point) for point in finite_points})
        bm = bmesh.new()
        bm.from_mesh(mesh)
        report['boundary_edges'] = sum(edge.is_boundary for edge in bm.edges)
        report['wire_edges'] = sum(edge.is_wire for edge in bm.edges)
        report['edges_with_more_than_two_faces'] = sum(len(edge.link_faces) > 2 for edge in bm.edges)
        report['inconsistent_winding_edges'] = sum(edge.is_manifold and not edge.is_contiguous for edge in bm.edges)
        report['isolated_vertices'] = sum(not vertex.link_faces for vertex in bm.verts)
        report['signed_volume_local'] = bm.calc_volume(signed=True)
        bm.free()
        return report
    finally:
        evaluated.to_mesh_clear()


def main():
    if not bpy.app.background:
        raise RuntimeError('Run in a separate background Blender process')
    if not bpy.data.filepath:
        raise RuntimeError('Load the saved veteran.blend in Blender CLI before this script')
    source = Path(bpy.data.filepath).resolve()
    REPORT['source_file'] = str(source)
    REPORT['source_sha256'] = file_hash(source)
    REPORT['source_bytes'] = source.stat().st_size
    REPORT['observed_source_files'] = {str(path.relative_to(ROOT)): file_hash(path)
        for path in (ROOT/'tools/build_veteran.py', ROOT/'tools/hero_veteran_head.py') if path.is_file()}
    hero = bpy.data.collections.get('HERO')
    if hero is None:
        raise RuntimeError('HERO collection missing')
    objects = list(hero.all_objects)
    bpy.context.view_layer.update()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    REPORT['depsgraph_mode'] = depsgraph.mode
    REPORT['scene_names'] = [scene.name for scene in bpy.data.scenes]
    REPORT['unit_scale_m_per_blender_unit'] = bpy.context.scene.unit_settings.scale_length
    REPORT['object_types'] = dict(Counter(obj.type for obj in objects))
    REPORT['hero_object_count'] = len(objects)
    REPORT['actions'] = len(bpy.data.actions)
    REPORT['armatures'] = len(bpy.data.armatures)
    REPORT['external_libraries'] = [library.filepath for library in bpy.data.libraries]
    REPORT['render_viewport_modifier_differences'] = []
    REPORT['cyclic_curves_with_repeated_endpoint'] = []
    used_materials = {mat.name: mat for obj in objects if getattr(obj.data, 'materials', None) is not None
                      for mat in obj.data.materials if mat}
    REPORT['materials'] = {name: material_report(mat) for name, mat in sorted(used_materials.items())}
    for obj in objects:
        for modifier in obj.modifiers:
            level_diff = modifier.type == 'SUBSURF' and modifier.levels != modifier.render_levels
            if level_diff or modifier.show_viewport != modifier.show_render:
                REPORT['render_viewport_modifier_differences'].append({'object': obj.name,
                    'modifier': modifier.name, 'type': modifier.type})
        if obj.type == 'CURVE':
            for spline in obj.data.splines:
                points = spline.bezier_points if spline.type == 'BEZIER' else spline.points
                if spline.use_cyclic_u and len(points) > 1:
                    if (points[0].co.xyz-points[-1].co.xyz).length < 1e-7:
                        REPORT['cyclic_curves_with_repeated_endpoint'].append(obj.name)
    geometry = [inspect_geometry(obj, depsgraph) for obj in objects if obj.type in GEOMETRY_TYPES]
    REPORT['geometry'] = geometry
    REPORT['geometry_object_count'] = len(geometry)
    keys = ('evaluated_vertices', 'evaluated_polygons', 'evaluated_triangles',
            'nonfinite_coordinates', 'nonfinite_face_normals', 'nonfinite_corner_normals',
            'zero_face_normals', 'zero_corner_normals', 'degenerate_triangles',
            'missing_material_faces', 'nonfinite_uv_loops', 'boundary_edges', 'wire_edges',
            'edges_with_more_than_two_faces', 'inconsistent_winding_edges', 'isolated_vertices')
    REPORT['totals'] = {key: sum(item.get(key, 0) for item in geometry) for key in keys}
    bbox = empty_bounds()
    for item in geometry:
        if item.get('bounds_blender_units'):
            extend_bounds(bbox, item['bounds_blender_units']['min'])
            extend_bounds(bbox, item['bounds_blender_units']['max'])
    REPORT['bounds_blender_units'] = finalized_bounds(bbox)
    scale = REPORT['unit_scale_m_per_blender_unit']
    REPORT['bounds_m'] = {key: [value*scale for value in values] for key, values in bbox.items()} if finalized_bounds(bbox) else None
    error_keys = ('evaluation_failed', 'nonfinite_coordinates', 'nonfinite_transform',
                  'nonfinite_face_normals', 'nonfinite_corner_normals', 'zero_face_normals',
                  'zero_corner_normals', 'degenerate_triangles', 'missing_material_faces',
                  'nonfinite_uv_loops', 'wire_edges', 'edges_with_more_than_two_faces',
                  'inconsistent_winding_edges', 'isolated_vertices')
    broken = [{'object': item['object'], 'issues': {key: item[key] for key in error_keys if item.get(key)}}
              for item in geometry if any(item.get(key) for key in error_keys)]
    if broken:
        finding('error', 'evaluated_geometry_integrity', 'Evaluated geometry contains technical defects', objects=broken)
    REPORT['meshes_without_uv'] = [item['object'] for item in geometry if not item.get('uv_layers')]
    REPORT['closed_negative_signed_volume_objects'] = [item['object'] for item in geometry
        if not item.get('boundary_edges') and item.get('signed_volume_local', 0) < -1e-10]
    if REPORT['closed_negative_signed_volume_objects']:
        finding('warning', 'negative_signed_volume', 'Closed evaluated meshes have negative signed volume; inspect winding',
                objects=REPORT['closed_negative_signed_volume_objects'])
    if REPORT['cyclic_curves_with_repeated_endpoint']:
        finding('warning', 'cyclic_duplicate_endpoint', 'Cyclic curves repeat their first point at the end',
                objects=REPORT['cyclic_curves_with_repeated_endpoint'])
    if REPORT['render_viewport_modifier_differences']:
        finding('warning', 'render_geometry_differs', 'Counts use evaluated viewport geometry; render modifier settings differ')
    procedural = [name for name, material in REPORT['materials'].items() if material['procedural_nodes']]
    REPORT['export_preparation'] = {
        'not_verified_for_export': True, 'game_ready': False,
        'curves_require_explicit_conversion_decision': sum(obj.type in {'CURVE', 'SURFACE', 'FONT', 'META'} for obj in objects),
        'missing_uv_geometry_count': len(REPORT['meshes_without_uv']),
        'procedural_materials_requiring_bake_or_simplification': procedural,
        'note': 'Current Blender appearance uses procedural nodes and editable curves. Plan conversion, material baking and UVs before claiming appearance-preserving GLB/FBX export.',
    }
    if REPORT['meshes_without_uv']:
        finding('info', 'uv_not_authored', 'Some evaluated geometry has no UV layers; acceptable for this model study',
                count=len(REPORT['meshes_without_uv']))
    if procedural:
        finding('info', 'procedural_materials', 'Procedural shader nodes are present; no material baking or export tested',
                count=len(procedural))
    REPORT['source_unchanged'] = file_hash(source) == REPORT['source_sha256']
    if not REPORT['source_unchanged']:
        finding('error', 'source_changed_during_validation', 'Source .blend changed while validation ran')
    REPORT['passed'] = not any(item['severity'] == 'error' for item in REPORT['findings'])


try:
    main()
except Exception:
    REPORT['passed'] = False
    REPORT['exception'] = traceback.format_exc()
    finding('error', 'validation_exception', 'Validation could not finish')
finally:
    REPORT_PATH.write_text(json.dumps(REPORT, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    print('VETERAN_REVIEW ' + json.dumps({'passed': REPORT.get('passed'),
        'totals': REPORT.get('totals'), 'findings': len(REPORT['findings']),
        'report': str(REPORT_PATH)}), flush=True)

if not REPORT['passed']:
    raise RuntimeError('Veteran geometry review failed; read verification/veteran-review.json')

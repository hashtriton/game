"""Make and round-trip a local static GLB copy of the saved Veteran study.

Run in background Blender after loading art/heroes/veteran/veteran.blend.
Only art/heroes/veteran/veteran.glb and verification/veteran-export.json
are written. Source geometry and procedural materials remain in the .blend.
The export uses evaluated meshes, including curves, baked world transforms
and scene unit scale, with simplified constant Principled materials.
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
from mathutils import Matrix


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / 'art/heroes/veteran/veteran.glb'
REPORT_PATH = ROOT / 'verification/veteran-export.json'
REPORT = {
    'asset': 'veteran', 'created_at_utc': datetime.now(timezone.utc).isoformat(),
    'blender_version': bpy.app.version_string, 'game_ready': False,
    'limitations': [
        'Static high-resolution model study. No rig, animations, LOD, collision or engine validation.',
        'Materials are simplified constant colors using source diffuse_color, Principled metallic and roughness. Procedural color/noise/bump, textures and other advanced shader effects are not transferred or baked.',
        'Evaluated curves and modifiers are converted to meshes only in the export copy. Missing UVs are not authored by this script.',
        'Cleanup removes coincident vertices and degenerate geometry only in the copy; this does not repair the editable .blend.',
    ],
    'errors': [],
}


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def finite(values):
    return all(math.isfinite(float(value)) for value in values)


def bounds(points):
    if not points:
        return None
    return {'min': [min(point[axis] for point in points) for axis in range(3)],
            'max': [max(point[axis] for point in points) for axis in range(3)]}


def shader_values(material):
    principled = next((node for node in material.node_tree.nodes
                       if node.type == 'BSDF_PRINCIPLED'), None) if material.node_tree else None
    if principled is None:
        return {'base_color': list(material.diffuse_color), 'metallic': material.metallic,
                'roughness': material.roughness, 'alpha': material.diffuse_color[3]}
    return {'base_color': list(principled.inputs['Base Color'].default_value),
            'metallic': principled.inputs['Metallic'].default_value,
            'roughness': principled.inputs['Roughness'].default_value,
            'alpha': principled.inputs['Alpha'].default_value}


def simplify_material(source, cache):
    if source is None:
        raise ValueError('Source mesh has an empty material slot')
    if source.name in cache:
        return cache[source.name]
    values = shader_values(source)
    color = tuple(source.diffuse_color)
    material = bpy.data.materials.new('GLB | ' + source.name)
    material.use_nodes = True
    material.diffuse_color = color
    bsdf = material.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Metallic'].default_value = values['metallic']
    bsdf.inputs['Roughness'].default_value = values['roughness']
    bsdf.inputs['Alpha'].default_value = color[3]
    material['source_material'] = source.name
    material['simplification'] = 'Constant base color, metallic and roughness; no procedural detail'
    cache[source.name] = material
    return material


def cleanup_mesh(mesh, area_epsilon):
    mesh.calc_loop_triangles()
    before = {'vertices': len(mesh.vertices), 'triangles': len(mesh.loop_triangles)}
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.remove_doubles(bm, verts=list(bm.verts), dist=1e-7)
    bmesh.ops.dissolve_degenerate(bm, edges=list(bm.edges), dist=1e-7)
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    degenerate = [face for face in bm.faces if face.calc_area() <= area_epsilon]
    removed_faces = len(degenerate)
    if degenerate:
        bmesh.ops.delete(bm, geom=degenerate, context='FACES_ONLY')
    wire = [edge for edge in bm.edges if not edge.link_faces]
    if wire:
        bmesh.ops.delete(bm, geom=wire, context='EDGES')
    isolated = [vertex for vertex in bm.verts if not vertex.link_faces]
    if isolated:
        bmesh.ops.delete(bm, geom=isolated, context='VERTS')
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    mesh.update()
    mesh.calc_loop_triangles()
    return {'before': before, 'after': {'vertices': len(mesh.vertices),
            'triangles': len(mesh.loop_triangles)}, 'explicit_zero_area_faces_removed': removed_faces}


def summarize(objects, area_epsilon):
    objects = list(objects)
    result = {'object_types': dict(Counter(obj.type for obj in objects)),
              'meshes': {}, 'materials': {}, 'vertices': 0, 'triangles': 0,
              'nonfinite_vertices': 0, 'missing_material_faces': 0, 'degenerate_triangles': 0,
              'uv_missing_meshes': 0}
    corners = []
    for obj in objects:
        if obj.type != 'MESH':
            continue
        mesh = obj.data
        mesh.calc_loop_triangles()
        points = [obj.matrix_world @ vertex.co for vertex in mesh.vertices]
        if any(not finite(point) for point in points):
            result['nonfinite_vertices'] += sum(not finite(point) for point in points)
        box = bounds(points)
        if box:
            corners.extend([box['min'], box['max']])
        triangle_count = len(mesh.loop_triangles)
        missing = sum(poly.material_index >= len(mesh.materials) or mesh.materials[poly.material_index] is None
                      for poly in mesh.polygons)
        degenerate = 0
        for tri in mesh.loop_triangles:
            a, b, c = [points[index] for index in tri.vertices]
            if (b-a).cross(c-a).length*.5 <= area_epsilon:
                degenerate += 1
        result['vertices'] += len(mesh.vertices)
        result['triangles'] += triangle_count
        result['missing_material_faces'] += missing
        result['degenerate_triangles'] += degenerate
        result['uv_missing_meshes'] += not bool(mesh.uv_layers)
        result['meshes'][obj.name] = {'triangles': triangle_count, 'vertices': len(mesh.vertices),
            'bounds_m': box, 'materials': sorted(mat.name for mat in mesh.materials if mat)}
        for material in mesh.materials:
            if material:
                result['materials'][material.name] = shader_values(material)
    result['mesh_count'] = len(result['meshes'])
    result['bounds_m'] = bounds(corners)
    return result


def compare(expected, actual):
    result = {'missing_meshes': sorted(set(expected['meshes'])-set(actual['meshes'])),
              'unexpected_meshes': sorted(set(actual['meshes'])-set(expected['meshes'])),
              'mesh_differences': [], 'material_differences': [], 'maximum_bound_difference_m': 0.0}
    for name in set(expected['meshes']) & set(actual['meshes']):
        a, b = expected['meshes'][name], actual['meshes'][name]
        delta = max(abs(a['bounds_m'][side][axis]-b['bounds_m'][side][axis])
                    for side in ('min', 'max') for axis in range(3))
        result['maximum_bound_difference_m'] = max(result['maximum_bound_difference_m'], delta)
        if a['triangles'] != b['triangles'] or a['materials'] != b['materials'] or delta > 1e-5:
            result['mesh_differences'].append({'object': name, 'expected': a, 'imported': b})
    if set(expected['materials']) != set(actual['materials']):
        result['material_differences'].append({'expected_names': sorted(expected['materials']),
                                               'imported_names': sorted(actual['materials'])})
    for name in set(expected['materials']) & set(actual['materials']):
        a, b = expected['materials'][name], actual['materials'][name]
        for key in a:
            differences = [abs(x-y) for x, y in zip(a[key], b[key])] if isinstance(a[key], list) else [abs(a[key]-b[key])]
            if max(differences) > 1e-5:
                result['material_differences'].append({'material': name, 'property': key, 'expected': a[key], 'imported': b[key]})
    return result


def main():
    if not bpy.app.background or not bpy.data.filepath:
        raise RuntimeError('Load veteran.blend in a separate background Blender process')
    source_path = Path(bpy.data.filepath)
    source_hash = sha256(source_path)
    REPORT['source_file'] = str(source_path)
    REPORT['source_sha256'] = source_hash
    scale = bpy.context.scene.unit_settings.scale_length
    REPORT['source_unit_scale_m'] = scale
    REPORT['scale_baked_into_vertices'] = True
    area_epsilon = 1e-12*scale*scale
    REPORT['degenerate_area_epsilon_m2'] = area_epsilon
    hero = bpy.data.collections.get('HERO')
    if hero is None:
        raise RuntimeError('HERO collection missing')
    source_objects = list(hero.all_objects)
    REPORT['source_object_types'] = dict(Counter(obj.type for obj in source_objects))
    bpy.context.view_layer.update()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    modifier_differences = [obj.name for obj in source_objects for mod in obj.modifiers
        if mod.show_viewport != mod.show_render or (mod.type == 'SUBSURF' and mod.levels != mod.render_levels)]
    if modifier_differences:
        raise RuntimeError('Render/viewport modifier settings differ: ' + repr(modifier_differences))
    scene = bpy.data.scenes.new('Veteran | Static export only')
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1.0
    collection = bpy.data.collections.new('Veteran | Evaluated export meshes')
    scene.collection.children.link(collection)
    root = bpy.data.objects.new('Veteran', None)
    collection.objects.link(root)
    root['asset_stage'] = 'Static model study, simplified materials, unrigged'
    root['source_unit_scale_baked'] = scale
    material_cache, cleanup = {}, {}
    for original in source_objects:
        if original.type not in {'MESH', 'CURVE', 'SURFACE', 'FONT', 'META'}:
            continue
        evaluated = original.evaluated_get(depsgraph)
        mesh = bpy.data.meshes.new_from_object(evaluated, preserve_all_data_layers=True, depsgraph=depsgraph)
        mesh.transform(Matrix.Diagonal((scale, scale, scale, 1.0)) @ evaluated.matrix_world)
        source_materials = list(mesh.materials)
        source_material_indices = [polygon.material_index for polygon in mesh.polygons]
        mesh.materials.clear()
        for material in source_materials:
            mesh.materials.append(simplify_material(material, material_cache))
        for polygon, material_index in zip(mesh.polygons, source_material_indices):
            polygon.material_index = material_index
        cleaned = cleanup_mesh(mesh, area_epsilon)
        if not mesh.polygons:
            raise RuntimeError('Cleanup produced an empty mesh: ' + original.name)
        copied = bpy.data.objects.new('GLB | ' + original.name, mesh)
        collection.objects.link(copied)
        copied.parent = root
        copied['source_object'] = original.name
        cleanup[original.name] = cleaned
    REPORT['cleanup'] = cleanup
    REPORT['material_mapping'] = {name: material.name for name, material in material_cache.items()}
    bpy.context.window.scene = scene
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action='DESELECT')
    for obj in collection.all_objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    expected = summarize(collection.all_objects, area_epsilon)
    REPORT['export_copy'] = expected
    if any(expected[key] for key in ('nonfinite_vertices', 'missing_material_faces', 'degenerate_triangles')):
        raise RuntimeError('Export copy contains invalid geometry or missing materials')
    bpy.ops.export_scene.gltf(filepath=str(OUTPUT), export_format='GLB', use_selection=True,
        export_apply=False, export_animations=False, export_cameras=False, export_lights=False,
        export_materials='EXPORT', export_yup=True, export_extras=True)
    REPORT['glb_file'] = str(OUTPUT)
    REPORT['glb_bytes'] = OUTPUT.stat().st_size
    REPORT['glb_sha256'] = sha256(OUTPUT)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(OUTPUT))
    actual = summarize(bpy.context.scene.objects, area_epsilon)
    REPORT['reimport'] = actual
    REPORT['comparison'] = compare(expected, actual)
    for key in ('missing_meshes', 'unexpected_meshes', 'mesh_differences', 'material_differences'):
        if REPORT['comparison'][key]:
            REPORT['errors'].append('Round-trip comparison failed: ' + key)
    for key in ('nonfinite_vertices', 'missing_material_faces', 'degenerate_triangles'):
        if actual[key]:
            REPORT['errors'].append('Reimport failed: ' + key)
    if any(obj.type not in {'MESH', 'EMPTY'} for obj in bpy.context.scene.objects):
        REPORT['errors'].append('Unexpected lights/cameras or other non-geometry object types')
    REPORT['source_unchanged'] = sha256(source_path) == source_hash
    if not REPORT['source_unchanged']:
        REPORT['errors'].append('Source blend changed during export')
    REPORT['passed'] = not REPORT['errors']


try:
    main()
except Exception:
    REPORT['passed'] = False
    REPORT['exception'] = traceback.format_exc()
    REPORT['errors'].append('Export or validation could not finish')
finally:
    if REPORT.get('source_file') and Path(REPORT['source_file']).is_file():
        REPORT['source_unchanged'] = sha256(Path(REPORT['source_file'])) == REPORT['source_sha256']
    REPORT_PATH.write_text(json.dumps(REPORT, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    print('VETERAN_EXPORT ' + json.dumps({'passed': REPORT.get('passed'),
        'errors': REPORT['errors'], 'glb_bytes': REPORT.get('glb_bytes'),
        'report': str(REPORT_PATH)}), flush=True)

if not REPORT['passed']:
    raise RuntimeError('Veteran export validation failed; read verification/veteran-export.json')

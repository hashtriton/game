"""Read-only inspection of a saved local Blender bust, separate from visual review.

Load the trial .blend in a background process and pass its model collection names
after --. Only verification/astra-bust-review.json is written.
"""
from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy

ROOT = Path(__file__).resolve().parents[1]
if not bpy.app.background:
    raise RuntimeError('Use a separate background Blender process.')
source = Path(bpy.data.filepath).resolve()
expected_directory = (ROOT / 'art/heroes/astra-bust-trial').resolve()
if source.parent != expected_directory or source.suffix != '.blend':
    raise RuntimeError('Load a saved .blend from art/heroes/astra-bust-trial.')
before = hashlib.sha256(source.read_bytes()).hexdigest()
args = sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if not args:
    raise RuntimeError('Pass the model collection names after --.')
models = [bpy.data.collections.get(name) for name in args]
if any(model is None for model in models):
    raise RuntimeError('Model collection not found in: ' + repr(args))
objects = {obj.name: obj for model in models for obj in model.all_objects}

errors = []
meshes = []
mins = [float('inf')] * 3
maxs = [-float('inf')] * 3
graph = bpy.context.evaluated_depsgraph_get()
for obj in objects.values():
    if obj.type not in {'MESH', 'CURVE', 'SURFACE', 'FONT', 'META'}:
        continue
    evaluated = obj.evaluated_get(graph)
    mesh = evaluated.to_mesh()
    if mesh is None:
        errors.append('No evaluated mesh: ' + obj.name)
        continue
    mesh.calc_loop_triangles()
    nonfinite = 0
    for vertex in mesh.vertices:
        point = evaluated.matrix_world @ vertex.co
        if not all(math.isfinite(v) for v in point):
            nonfinite += 1
            continue
        for axis in range(3):
            mins[axis] = min(mins[axis], point[axis])
            maxs[axis] = max(maxs[axis], point[axis])
    missing_materials = sum(
        p.material_index >= len(mesh.materials) or mesh.materials[p.material_index] is None
        for p in mesh.polygons
    )
    zero_area = sum(t.area <= 1e-15 for t in mesh.loop_triangles)
    item = {'name': obj.name, 'vertices': len(mesh.vertices),
            'triangles': len(mesh.loop_triangles), 'near_zero_area_triangles': zero_area,
            'nonfinite_vertices': nonfinite, 'missing_material_faces': missing_materials,
            'uv_layers': len(mesh.uv_layers)}
    meshes.append(item)
    if nonfinite or missing_materials:
        errors.append('Invalid coordinates or missing materials: ' + obj.name)
    evaluated.to_mesh_clear()
if not meshes:
    errors.append('No evaluated model geometry.')

external_images = []
for im in bpy.data.images:
    if im.source != 'FILE' or not im.users:
        continue
    packed = bool(im.packed_file or im.packed_files)
    path = Path(bpy.path.abspath(im.filepath)) if im.filepath else None
    exists = packed or bool(path and path.is_file())
    external_images.append({'name': im.name, 'packed': packed, 'available': exists})
    if not exists:
        errors.append('Missing image resource: ' + im.name)
for library in bpy.data.libraries:
    if not Path(bpy.path.abspath(library.filepath)).is_file():
        errors.append('Missing linked library: ' + library.name)

after = hashlib.sha256(source.read_bytes()).hexdigest()
if before != after:
    errors.append('Source file changed during read-only validation.')
report = {
    'checked_at_utc': datetime.now(timezone.utc).isoformat(),
    'blender_version': bpy.app.version_string,
    'source_file': str(source), 'source_sha256': before,
    'source_unchanged': before == after,
    'model_collections': [model.name for model in models], 'mesh_objects': len(meshes),
    'evaluated_triangles': sum(m['triangles'] for m in meshes),
    'near_zero_area_triangles': sum(m['near_zero_area_triangles'] for m in meshes),
    'nonfinite_vertices': sum(m['nonfinite_vertices'] for m in meshes),
    'missing_material_faces': sum(m['missing_material_faces'] for m in meshes),
    'objects_without_uv': sum(not m['uv_layers'] for m in meshes),
    'bounds_blender_units': {'min': mins, 'max': maxs} if meshes else None,
    'unit_scale': bpy.context.scene.unit_settings.scale_length,
    'external_images': external_images,
    'camera_present': bpy.context.scene.camera is not None,
    'light_count': sum(o.type == 'LIGHT' for o in bpy.context.scene.objects),
    'errors': errors, 'technical_pass': not errors,
    'visual_acceptance': 'Requires separate inspection against the user reference.',
    'scope': 'Local static bust trial. No claim of game readiness, rig, UVs or export.',
    'meshes_with_geometry_warnings': [m for m in meshes if m['near_zero_area_triangles']],
}
destination = ROOT / 'verification/astra-bust-review.json'
destination.write_text(json.dumps(report, indent=2, ensure_ascii=False)+'\n', encoding='utf-8')
print('ASTRA_TRIAL_CHECK ' + json.dumps({k: report[k] for k in (
    'technical_pass', 'mesh_objects', 'evaluated_triangles',
    'near_zero_area_triangles', 'source_unchanged', 'errors')}))
if errors:
    raise RuntimeError('Technical inspection failed; see ' + str(destination))

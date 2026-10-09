"""Evaluate both reversible poses without modifying the loaded source."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import bpy
import bmesh

ROOT = Path(__file__).resolve().parents[2]


def look_contract():
    def value(v):
        if isinstance(v,(str,bool,int)):
            return v
        if isinstance(v,float):
            return round(v,7)
        try:
            return [value(x) for x in v]
        except TypeError:
            return str(v)
    materials=[]
    for m in sorted(bpy.data.materials,key=lambda x:x.name):
        nodes=[]
        for n in m.node_tree.nodes if m.use_nodes else []:
            nodes.append({'name':n.name,'type':n.bl_idname,
                          'inputs':[(i.identifier,value(i.default_value)) for i in n.inputs if hasattr(i,'default_value')],
                          'enums':{k:value(getattr(n,k)) for k in ['operation','blend_type','wave_type','bands_direction','samples'] if hasattr(n,k)}})
        links=[(l.from_node.name,l.from_socket.identifier,l.to_node.name,l.to_socket.identifier) for l in m.node_tree.links] if m.use_nodes else []
        materials.append({'name':m.name,'diffuse':list(m.diffuse_color),'nodes':nodes,'links':sorted(links)})
    return {'materials':materials,'rest_properties':{o.name:{k:value(o[k]) for k in ['rest_location','rest_rotation','footprint','weapon'] if k in o}
                                                     for o in sorted(bpy.context.scene.objects,key=lambda o:o.name)},
            'units':{'system':bpy.context.scene.unit_settings.system,'scale':bpy.context.scene.unit_settings.scale_length}}


def inspect(prefix, state):
    from A_common import pose
    pose(prefix, state)
    deps = bpy.context.evaluated_depsgraph_get()
    records, coords, footprint = [], [], []
    bad_finite = bad_area = triangles = 0
    bad_topology=[]
    for obj in sorted(bpy.context.scene.objects, key=lambda o: o.name):
        row = {'name': obj.name, 'parent': obj.parent.name if obj.parent else None,
               'scale': list(obj.scale), 'pivot': list(obj.matrix_world.translation),
               'local_matrix': [round(float(v),7) for line in obj.matrix_local for v in line],
               'world_matrix': [round(float(v),7) for line in obj.matrix_world for v in line]}
        if obj.type == 'MESH':
            ev = obj.evaluated_get(deps)
            mesh = ev.to_mesh()
            mesh.calc_loop_triangles()
            vs = [ev.matrix_world @ v.co for v in mesh.vertices]
            coords.extend(vs)
            if obj.get('footprint', False):
                footprint.extend(vs)
            bad_finite += sum(not all(math.isfinite(v) for v in co) for co in vs)
            bad_area += sum(t.area <= 1e-12 for t in mesh.loop_triangles)
            triangles += len(mesh.loop_triangles)
            raw = {'v': [[round(float(x), 7) for x in v.co] for v in mesh.vertices],
                   't': [list(t.vertices) for t in mesh.loop_triangles],
                   'polygon_contract': [{'vertices':list(p.vertices),'material_index':p.material_index,
                                         'smooth':p.use_smooth,'normal':[round(float(v),7) for v in p.normal]} for p in mesh.polygons],
                   'sharp_edges': [(list(e.vertices),bool(e.use_edge_sharp)) for e in mesh.edges]}
            row.update(triangles=len(mesh.loop_triangles), vertices=len(vs),
                       geometry_sha256=hashlib.sha256(json.dumps(raw, sort_keys=True).encode()).hexdigest(),
                       materials=[m.name for m in obj.data.materials],
                       bounds=[[min(v[i] for v in vs) for i in range(3)],
                               [max(v[i] for v in vs) for i in range(3)]])
            bm=bmesh.new()
            bm.from_mesh(mesh)
            topology={'nonmanifold_edges':sum(not e.is_manifold for e in bm.edges),
                      'inconsistent_edges':sum(e.is_manifold and not e.is_contiguous for e in bm.edges),
                      'signed_volume':bm.calc_volume(signed=True)}
            bm.free()
            row['topology']=topology
            if topology['nonmanifold_edges'] or topology['inconsistent_edges'] or topology['signed_volume']<=0:
                bad_topology.append(obj.name)
            ev.to_mesh_clear()
        records.append(row)
    if not coords or not footprint:
        raise AssertionError('Expected creep meshes and marked torso/leg footprint')
    body = [v for o in bpy.context.scene.objects if o.type == 'MESH' and not o.get('weapon', False)
            for v in [o.evaluated_get(deps).matrix_world @ p.co for p in o.evaluated_get(deps).data.vertices]]
    bounds = [[min(v[i] for v in coords) for i in range(3)], [max(v[i] for v in coords) for i in range(3)]]
    height = max(v.z for v in body) - min(v.z for v in body)
    radius = max(math.hypot(v.x, v.y) for v in footprint)
    soles = {s: min((bpy.data.objects[prefix+'Foot_'+s].evaluated_get(deps).matrix_world @ v.co).z
                    for v in bpy.data.objects[prefix+'Foot_'+s].evaluated_get(deps).data.vertices) for s in ['L','R']}
    names = {r['name']: r for r in records}
    parents = {prefix+'Root': None, prefix+'Pelvis': prefix+'Root', prefix+'Torso': prefix+'Pelvis',
               prefix+'Head': prefix+'Torso'}
    for s in ['L','R']:
        parents.update({prefix+'UpperArm_'+s: prefix+'Torso', prefix+'Forearm_'+s: prefix+'UpperArm_'+s,
                        prefix+'Thigh_'+s: prefix+'Pelvis', prefix+'Shin_'+s: prefix+'Thigh_'+s,
                        prefix+'Foot_'+s: prefix+'Shin_'+s})
    if prefix == 'B1_':
        parents[prefix+'Yoke'] = prefix+'Torso'
    else:
        parents[prefix+'Sail'] = prefix+'Torso'
        parents['Hand_R_socket'] = prefix+'Forearm_R'
        parents[prefix+'Cleaver'] = 'Hand_R_socket'
    checks = {'finite': bad_finite == 0, 'positive_triangle_area': bad_area == 0,
              'closed_outward_consistent_meshes': not bad_topology,
              'unit_scales': all(max(abs(s-1) for s in r['scale']) < 1e-6 for r in records),
              'names': all(n.startswith(prefix) or n == 'Hand_R_socket' for n in names),
              'parents': all(n in names and names[n]['parent'] == par for n,par in parents.items())
                         and all(r['name']==prefix+'Root' or r['parent'] in names for r in records),
              'root_origin': prefix+'Root' in names and max(abs(v) for v in names[prefix+'Root']['pivot'])<1e-7,
              'radius': radius <= (.7 if prefix == 'B1_' else .6) + 1e-6,
              'triangles': triangles <= (10000 if prefix == 'B1_' else 12000),
              'five_materials': len({m for r in records for m in r.get('materials',[])}) <= 5,
              'soles': all(abs(z) < 1e-5 for z in soles.values()),
              'height': state != 'rest' or abs(height-(2.9 if prefix == 'B1_' else 3.5)) < 1e-5,
              'no_armature_actions': not bpy.data.armatures and not bpy.data.actions,
              'no_uv_images': all(not o.data.uv_layers for o in bpy.context.scene.objects if o.type == 'MESH')
                               and not any(n.type == 'TEX_IMAGE' for m in bpy.data.materials if m.use_nodes for n in m.node_tree.nodes)}
    return {'height_m': height, 'footprint_radius_m': radius, 'triangles': triangles, 'bounds': bounds,
            'soles_z': soles, 'nonfinite': bad_finite, 'zero_area_triangles': bad_area,
            'bad_topology':bad_topology,
            'objects': records, 'checks': checks, 'pass': all(checks.values()),
            'fingerprint': hashlib.sha256(json.dumps(records, sort_keys=True).encode()).hexdigest()}


def main(creep, prefix):
    p = argparse.ArgumentParser()
    p.add_argument('--output', type=Path, default=ROOT/'art/creatures'/creep/'validation.json')
    p.add_argument('--compare', type=Path)
    a = p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    source = Path(bpy.data.filepath)
    before = hashlib.sha256(source.read_bytes()).hexdigest() if source.is_file() else None
    result = {'source': str(source), 'source_sha256': before, 'blender': bpy.app.version_string,
              'rest': inspect(prefix, 'rest'), 'display': inspect(prefix, 'display')}
    result['rig_look_contract']=look_contract()
    result['rig_look_fingerprint']=hashlib.sha256(json.dumps(result['rig_look_contract'],sort_keys=True).encode()).hexdigest()
    if a.compare:
        reference = json.loads(a.compare.read_text(encoding='utf-8'))
        result['comparison'] = {s: reference[s]['fingerprint'] == result[s]['fingerprint'] for s in ['rest','display']}
        result['comparison']['rig_look']=reference.get('rig_look_fingerprint')==result['rig_look_fingerprint']
    result['pass'] = all(result[s]['pass'] for s in ['rest','display']) and all(result.get('comparison',{}).values())
    result['source_unchanged'] = before == (hashlib.sha256(source.read_bytes()).hexdigest() if source.is_file() else None)
    a.output.parent.mkdir(parents=True, exist_ok=True)
    a.output.write_text(json.dumps(result, indent=2)+'\n', encoding='utf-8')
    print(json.dumps({s: {k: result[s][k] for k in ['height_m','footprint_radius_m','triangles','checks']} for s in ['rest','display']}, indent=2))
    if not result['pass'] or not result['source_unchanged']:
        raise AssertionError('Validation failed; read output JSON')

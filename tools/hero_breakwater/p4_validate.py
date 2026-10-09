"""Evaluated geometry, retained hierarchy and deterministic two-pose gates."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
from validate_p3 import measure as old_measure

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/heroes/breakwater'
OUT = ART / 'p4'
LOCAL = ROOT / '.local/codex-tasks/hero-b'


def rounded(value):
    if isinstance(value,(float,int)):
        return round(float(value),7)
    if isinstance(value,(str,bool)) or value is None:
        return value
    try:
        return [rounded(v) for v in value]
    except TypeError:
        return str(value)


def contract_fingerprint():
    objects=[]
    for o in sorted(bpy.context.scene.objects,key=lambda o:o.name):
        objects.append({'name':o.name,'parent':o.parent.name if o.parent else None,
            'matrix':rounded(o.matrix_world),'local_matrix':rounded(o.matrix_local),
            'parent_inverse':rounded(o.matrix_parent_inverse),
            'rest_location':rounded(o.get('rest_location')),'rest_rotation':rounded(o.get('rest_rotation')),
            'rest_parent_inverse':rounded(o.get('rest_parent_inverse')),
            'grip_offset':rounded(o.get('grip_offset')),'grip_local':rounded(o.get('grip_local'))})
    materials=[]
    for m in sorted(bpy.data.materials,key=lambda m:m.name):
        if not m.use_nodes or m.users==0:
            continue
        nodes=[]
        for n in sorted(m.node_tree.nodes,key=lambda n:n.name):
            properties={p:rounded(getattr(n,p)) for p in ['operation','blend_type','samples','wave_type','bands_direction','clamp','mute'] if hasattr(n,p)}
            values={i.identifier:rounded(i.default_value) for i in n.inputs if hasattr(i,'default_value')}
            ramps=[(rounded(e.position),rounded(e.color)) for e in n.color_ramp.elements] if hasattr(n,'color_ramp') else []
            if hasattr(n,'color_ramp'):
                properties['ramp_interpolation']=n.color_ramp.interpolation
            nodes.append({'name':n.name,'type':n.bl_idname,'inputs':values,'properties':properties,'ramp':ramps})
        links=sorted((l.from_node.name,l.from_socket.identifier,l.to_node.name,l.to_socket.identifier) for l in m.node_tree.links)
        materials.append({'name':m.name,'nodes':nodes,'links':links})
    scene=bpy.context.scene
    metadata={k:rounded(scene.get(k)) for k in ['hero_variant','revision','spec','p4_spec','display_rotations']}
    return hashlib.sha256(json.dumps({'objects':objects,'materials':materials,'scene':metadata},sort_keys=True).encode()).hexdigest()


def sword_topology():
    obj=bpy.data.objects['HB_Sword']
    mesh=obj.data
    uses={}
    for f in mesh.polygons:
        indices=list(f.vertices)
        for a,b in zip(indices,indices[1:]+indices[:1]):
            key=tuple(sorted((a,b)))
            uses.setdefault(key,[]).append((a,b))
    bad_edges=sum(len(u)!=2 for u in uses.values())
    bad_winding=sum(len(u)==2 and u[0]==u[1] for u in uses.values())
    mesh.calc_loop_triangles()
    signed=sum(mesh.vertices[t.vertices[0]].co.dot(mesh.vertices[t.vertices[1]].co.cross(mesh.vertices[t.vertices[2]].co))/6 for t in mesh.loop_triangles)
    return {'nonmanifold_edges':bad_edges,'inconsistent_edges':bad_winding,'signed_volume':signed}


def measure():
    result = old_measure()
    old = json.loads((ART / 'validation-p3.json').read_text())['rest']['objects']
    names = {o.name for o in bpy.context.scene.objects}
    parents = {o.name: o.parent.name if o.parent else None for o in bpy.context.scene.objects}
    local = result['part_dimensions_local']
    bounds = result['part_bounds']
    span = max(bounds[n]['max'][0] for n in bounds if n.startswith('HB_Pauldron')) - min(bounds[n]['min'][0] for n in bounds if n.startswith('HB_Pauldron'))
    checks = {
        'all_p3_names_kept': {o['name'] for o in old} <= names,
        'all_p3_parents_kept': all(parents.get(o['name']) == o['parent'] for o in old),
        'triangles_le_10000': result['total_triangles'] <= 10000,
        'footprint_le_0_42': result['footprint_radius'] <= .420001,
        'max_six_hero_materials': len(result['unique_materials']) <= 6,
        'finite_vertices': result['nonfinite_vertices'] == 0,
        'no_zero_area_triangles': result['zero_area_triangles'] == 0,
        'unit_scales': all(all(abs(s - 1) < 1e-6 for s in o['scale']) for o in result['objects']),
        'family_only': all(n.startswith('HB_') or n in ['Hand_R_socket', 'Forearm_L_socket'] for n in names),
        'root_origin': bpy.data.objects['HB_Root'].location.length < 1e-7,
        'socket_parents': parents['Hand_R_socket'] == 'HB_Forearm_R' and parents['Forearm_L_socket'] == 'HB_Forearm_L',
        'equipment_parents': parents['HB_Sword'] == 'Hand_R_socket' and parents['HB_Shield'] == 'Forearm_L_socket',
        'no_armature_actions': not bpy.data.armatures and not bpy.data.actions,
        'no_uv_image_textures': all(not o.data.uv_layers for o in bpy.context.scene.objects if o.type == 'MESH') and not any(n.type == 'TEX_IMAGE' for m in bpy.data.materials if m.use_nodes for n in m.node_tree.nodes),
        'metric_metres': bpy.context.scene.unit_settings.system == 'METRIC' and bpy.context.scene.unit_settings.scale_length == 1,
        'soles_on_ground': abs(result['body_z_min']) < 1e-5,
        'equipped_width_le_1_8': result['total_width'] <= 1.800001,
        'blade_length_0_85_to_0_95': .8499 <= local['HB_Sword'][2] <= .9501,
        'blade_width_0_19_to_0_22': .1899 <= local['HB_Sword'][0] <= .2201,
        'shield_has_handle': 'HB_ShieldHandle' in names and 'HB_ShieldStrap' in names,
        'closed_grips': all('HB_GripFingers_' + side in names for side in ['L', 'R']),
    }
    result['shoulder_span'] = span
    topology=sword_topology()
    result['sword_topology']=topology
    checks['sword_closed_outward_winding']=topology['nonmanifold_edges']==0 and topology['inconsistent_edges']==0 and topology['signed_volume']>0
    result['grip_center_errors']={}
    for side in ['L','R']:
        hand=bpy.data.objects['HB_Gauntlet_'+side]
        center=hand.matrix_world@(sum((v.co for v in hand.data.vertices),Vector())/len(hand.data.vertices))
        if side=='R':
            grip=bpy.data.objects['HB_SwordGrip']
            target=grip.matrix_world@(sum((v.co for v in grip.data.vertices),Vector())/len(grip.data.vertices))
        else:
            handle=bpy.data.objects.get('HB_ShieldHandle')
            target=handle.matrix_world@Vector(handle['grip_local']) if handle else center+Vector((100,0,0))
        result['grip_center_errors'][side]=(center-target).length
    checks['fists_centered_on_grips']=all(d<=.03 for d in result['grip_center_errors'].values())
    result['rig_look_fingerprint_sha256']=contract_fingerprint()
    if result['pose'] == 'rest':
        checks['rest_height_2_4'] = abs(result['body_height'] - 2.4) <= .0024
        checks['shoulder_span_1_15_to_1_30'] = 1.15 <= span <= 1.300001
        pivots=json.loads((ART/'validation-p3.json').read_text())['rest']['pivots']
        errors={n:(Vector(result['pivots'][n])-Vector(point)).length for n,point in pivots.items() if n in result['pivots']}
        result['p3_rest_pivot_errors']=errors
        checks['all_p3_rest_pivots_kept']=all(d<=1e-6 for d in errors.values())
    else:
        feet = [bpy.data.objects['HB_Boot_' + s].matrix_world.translation for s in ['L', 'R']]
        separation = math.hypot(feet[0].x - feet[1].x, feet[0].y - feet[1].y)
        result['feet_separation'] = separation
        checks['lower_display'] = result['body_height'] < 2.4
        checks['feet_0_55_to_0_60'] = .5499 <= separation <= .6001
    result['checks'] = checks
    result['passed'] = all(checks.values())
    return result


def main():
    p = argparse.ArgumentParser()
    p.add_argument('--output', type=Path, required=True)
    p.add_argument('--compare', type=Path)
    p.add_argument('--baseline-p3', action='store_true')
    args = p.parse_args(sys.argv[sys.argv.index('--') + 1:])
    output=args.output.resolve()
    if OUT.resolve() not in output.parents and LOCAL.resolve() not in output.parents:
        raise ValueError('P4 or task scratch outputs only')
    if args.baseline_p3:
        from build_p3 import pose
    else:
        from p4_build import pose
    source = Path(bpy.data.filepath)
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    result = {'blender_version': bpy.app.version_string, 'source': str(source.relative_to(ROOT)),
              'source_sha256': before, 'saved_pose': bpy.context.scene['pose_state']}
    result['saved_fingerprint'] = measure()['geometry_fingerprint_sha256']
    for state in ['rest', 'display']:
        pose(state)
        result[state] = measure()
    checks = {'both_poses_pass': all(result[s]['passed'] for s in ['rest', 'display']),
              'source_unchanged': before == hashlib.sha256(source.read_bytes()).hexdigest(),
              'saved_pose_matches': result['saved_fingerprint'] == result[result['saved_pose']]['geometry_fingerprint_sha256'],
              'equal_triangle_budget': result['rest']['total_triangles'] == result['display']['total_triangles']}
    if args.compare:
        previous = json.loads(args.compare.read_text())
        for s in ['rest', 'display']:
            checks[s + '_deterministic'] = result[s]['geometry_fingerprint_sha256'] == previous[s]['geometry_fingerprint_sha256']
            checks[s + '_rig_look_deterministic'] = result[s]['rig_look_fingerprint_sha256'] == previous[s]['rig_look_fingerprint_sha256']
    result['checks'] = checks
    result['passed'] = all(checks.values())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    for s in ['rest', 'display']:
        print(s, {k: result[s].get(k) for k in ['body_height', 'total_triangles', 'total_width', 'footprint_radius', 'shoulder_span', 'feet_separation']})
        print('FAILED', [n for n, v in result[s]['checks'].items() if not v])
    print('P4 VALIDATION', result['passed'], checks)
    if not result['passed']:
        raise SystemExit(1)


if __name__ == '__main__':
    main()

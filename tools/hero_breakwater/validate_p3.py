"""Evaluated P3 geometry gates, pose dimensions and reproducibility evidence."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from build_p3 import SPEC, pose

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'
SCRATCH=ROOT/'.local/codex-tasks/hero-b'


def measure():
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get()
    report={'pose':bpy.context.scene['pose_state'],'objects':[], 'total_triangles':0,
            'nonfinite_vertices':0,'zero_area_triangles':0,'degenerate_details':[],
            'footprint_radius':0,'part_bounds':{},'pivots':{},'part_dimensions_local':{}}
    allpoints=[]
    bodypoints=[]
    fp=[]
    footprint=('HB_Torso','HB_Breastplate','HB_Back','HB_ChestLip','HB_Pelvis','HB_Thigh',
               'HB_Cuisse','HB_Knee','HB_Poleyn','HB_Shin','HB_Greave','HB_Boot','HB_Sabaton',
               'HB_Belt','HB_Fauld','HB_Tabard','HB_Pouches')
    for o in sorted(bpy.context.scene.objects,key=lambda o:o.name):
        row={'name':o.name,'type':o.type,'parent':o.parent.name if o.parent else None,
             'scale':list(o.scale),'materials':[s.material.name if s.material else None for s in o.material_slots],
             'modifiers':[{'type':m.type,'name':m.name,
                           'thickness':getattr(m,'thickness',None),'width':getattr(m,'width',None),
                           'segments':getattr(m,'segments',None)} for m in o.modifiers], 'triangles':0}
        report['pivots'][o.name]=list(o.matrix_world.translation)
        if o.type=='MESH':
            e=o.evaluated_get(deps)
            mesh=e.to_mesh()
            mesh.calc_loop_triangles()
            points=[e.matrix_world@v.co for v in mesh.vertices]
            report['nonfinite_vertices']+=sum(not all(math.isfinite(x) for x in p) for p in points)
            row['triangles']=len(mesh.loop_triangles)
            report['total_triangles']+=row['triangles']
            for tri in mesh.loop_triangles:
                a,b,c=(points[i] for i in tri.vertices)
                area=(b-a).cross(c-a).length*.5
                if area<=1e-12:
                    report['zero_area_triangles']+=1
                    report['degenerate_details'].append({'object':o.name,'area':area})
            bounds={'min':[min(p[i] for p in points) for i in range(3)],
                    'max':[max(p[i] for p in points) for i in range(3)]}
            bounds['size']=[bounds['max'][i]-bounds['min'][i] for i in range(3)]
            report['part_bounds'][o.name]=bounds
            row['ground_radius']=max(math.hypot(p.x,p.y) for p in points)
            report['part_dimensions_local'][o.name]=[max(v.co[i] for v in mesh.vertices)-min(v.co[i] for v in mesh.vertices) for i in range(3)]
            allpoints.extend(points)
            if not o.name.startswith(('HB_Shield','HB_Rim','HB_Sword')):
                bodypoints.extend(points)
            if o.name.startswith(footprint):
                report['footprint_radius']=max(report['footprint_radius'],max(math.hypot(p.x,p.y) for p in points))
            fp.append({'name':o.name,'parent':row['parent'],
                       'points':[[round(x,7) for x in p] for p in points],
                       'triangles':[list(t.vertices) for t in mesh.loop_triangles],
                       'modifiers':row['modifiers'],'materials':row['materials']})
            e.to_mesh_clear()
        else:
            fp.append({'name':o.name,'parent':row['parent'],'matrix':[[round(x,7) for x in r] for r in o.matrix_world]})
        report['objects'].append(row)
    report['body_z_min']=min(p.z for p in bodypoints)
    report['body_z_max']=max(p.z for p in bodypoints)
    report['body_height']=report['body_z_max']-report['body_z_min']
    report['total_width']=max(p.x for p in allpoints)-min(p.x for p in allpoints)
    report['unique_materials']=sorted({m for o in report['objects'] for m in o['materials'] if m})
    report['geometry_fingerprint_sha256']=hashlib.sha256(json.dumps(fp,sort_keys=True).encode()).hexdigest()
    old=json.loads((ART/'validation-p2.json').read_text())
    names={o.name for o in bpy.context.scene.objects}
    checks={'all_first_pass_names_preserved':{o['name'] for o in old['objects']}<=names,
            'triangles_le_9000':report['total_triangles']<=9000,
            'footprint_le_0_42':report['footprint_radius']<=.420001,
            'max_two_materials':len(report['unique_materials'])<=2,
            'finite_vertices':report['nonfinite_vertices']==0,
            'no_zero_area_triangles':report['zero_area_triangles']==0,
            'unit_scales':all(all(abs(s-1)<1e-7 for s in o['scale']) for o in report['objects']),
            'only_hero_family_sockets':all(n.startswith('HB_') or n in ['Hand_R_socket','Forearm_L_socket'] for n in names),
            'only_mesh_empty':all(o['type'] in ['MESH','EMPTY'] for o in report['objects']),
            'root_origin':bpy.data.objects['HB_Root'].location.length<1e-7,
            'metric_metres':bpy.context.scene.unit_settings.system=='METRIC' and bpy.context.scene.unit_settings.scale_length==1,
            'socket_parents':bpy.data.objects['Hand_R_socket'].parent.name=='HB_Forearm_R' and bpy.data.objects['Forearm_L_socket'].parent.name=='HB_Forearm_L',
            'equipment_parents':bpy.data.objects['HB_Sword'].parent.name=='Hand_R_socket' and bpy.data.objects['HB_Shield'].parent.name=='Forearm_L_socket',
            'no_armature_actions':not bpy.data.armatures and not bpy.data.actions,
            'no_uv_textures':all(not o.data.uv_layers for o in bpy.context.scene.objects if o.type=='MESH') and not any(n.type=='TEX_IMAGE' for m in bpy.data.materials if m.use_nodes for n in m.node_tree.nodes),
            'soles_on_ground':abs(report['body_z_min'])<1e-5}
    if report['pose']=='rest':
        checks['rest_height_2_4']=abs(report['body_height']-2.4)<=.024
        checks['rest_width_le_1_7']=report['total_width']<=1.700001
        b=report['part_bounds']
        local=report['part_dimensions_local']
        shoulder_span=max(b[n]['max'][0] for n in b if n.startswith('HB_Pauldron'))-min(b[n]['min'][0] for n in b if n.startswith('HB_Pauldron'))
        chest_depth=max(b[n]['max'][1] for n in ['HB_Breastplate','HB_Back'])-min(b[n]['min'][1] for n in ['HB_Breastplate','HB_Back'])
        report['proportions']={'helmet_height':local['HB_Helmet'][2],
                              'helmet_heights':report['body_height']/local['HB_Helmet'][2],
                              'shoulder_span':shoulder_span,'chest_depth':chest_depth,
                              'thigh_width':local['HB_Thigh_L'][0],
                              'shin_width':local['HB_Shin_L'][0],
                              'boot_length':local['HB_Boot_L'][1],
                              'forearm_width':local['HB_Forearm_L'][0],
                              'hip_height_ratio':report['pivots']['HB_Pelvis'][2]/report['body_height']}
        checks.update({'helmet_height_target':.40<=local['HB_Helmet'][2]<=.44,
                       'shoulder_span_target':1.15<=shoulder_span<=1.300001,
                       'chest_depth_at_least_0_5':chest_depth>=.50,
                       'thigh_width_at_least_0_32':local['HB_Thigh_L'][0]>=.32,
                       'shin_width_at_least_0_30':local['HB_Shin_L'][0]>=.30,
                       'boot_length_at_least_0_46':local['HB_Boot_L'][1]>=.46,
                       'forearm_width_at_least_0_27':local['HB_Forearm_L'][0]>=.27,
                       'legs_45_to_47_percent':.45<=report['proportions']['hip_height_ratio']<=.47})
    else:
        checks['display_lower_than_rest']=report['body_height']<2.4
    report['checks']=checks
    report['passed']=all(checks.values())
    return report


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--variant',choices=['p3'],default='p3')
    parser.add_argument('--output',type=Path,default=ART/'validation-p3.json')
    parser.add_argument('--compare',type=Path)
    opt=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    source=Path(bpy.data.filepath)
    before=hashlib.sha256(source.read_bytes()).hexdigest()
    report={'blender_version':bpy.app.version_string,'source':str(source.relative_to(ROOT)),
            'source_sha256':before,'saved_pose':bpy.context.scene['pose_state'],'spec':SPEC}
    report['saved_pose_fingerprint']=measure()['geometry_fingerprint_sha256']
    for state in ['rest','display']:
        pose(state)
        report[state]=measure()
    checks={'both_poses_pass':report['rest']['passed'] and report['display']['passed'],
            'same_triangle_budget':report['rest']['total_triangles']==report['display']['total_triangles'],
            'source_unchanged':before==hashlib.sha256(source.read_bytes()).hexdigest()}
    checks['saved_pose_matches_reproduced_pose']=report['saved_pose_fingerprint']==report[report['saved_pose']]['geometry_fingerprint_sha256']
    if opt.compare:
        previous=json.loads(opt.compare.read_text())
        checks['rest_geometry_matches']=report['rest']['geometry_fingerprint_sha256']==previous['rest']['geometry_fingerprint_sha256']
        checks['display_geometry_matches']=report['display']['geometry_fingerprint_sha256']==previous['display']['geometry_fingerprint_sha256']
        checks['same_numbers']=all(report[s][k]==previous[s][k] for s in ['rest','display'] for k in
                                  ['body_height','footprint_radius','total_width','total_triangles','objects','part_bounds'])
    report['checks']=checks
    report['passed']=all(checks.values())
    opt.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({s:{k:report[s][k] for k in ['body_height','footprint_radius','total_width','total_triangles','zero_area_triangles','checks']} for s in ['rest','display']},indent=2))
    if not report['passed']:
        raise SystemExit(1)


if __name__=='__main__':
    main()

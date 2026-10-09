"""Evaluate saved meshes and enforce the hero blockout technical contract."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

import bpy

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'
REQUIRED=['HB_Helmet','HB_Torso','HB_Back','HB_Pauldron_L','HB_Pauldron_R',
          'HB_UpperArm_L','HB_UpperArm_R','HB_Forearm_L','HB_Forearm_R','HB_Pelvis','HB_Tabard',
          'HB_Thigh_L','HB_Thigh_R','HB_Shin_L','HB_Shin_R','HB_Boot_L','HB_Boot_R','HB_Belt',
          'HB_Pouches','HB_Shield','HB_Sword','Hand_R_socket','Forearm_L_socket']
FOOTPRINT=('HB_Torso','HB_Back','HB_ChestLip','HB_Pelvis','HB_Thigh','HB_Knee','HB_Shin','HB_Boot')
EQUIPMENT=('HB_Shield','HB_Sword')

def measure():
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get()
    report={'objects':[], 'total_triangles':0,'nonfinite_vertices':0,'zero_area_triangles':0,
            'missing_material_faces':0,'degenerate_details':[],'footprint_radius':0,'all_x_min':1e30,'all_x_max':-1e30,
            'body_z_min':1e30,'body_z_max':-1e30,'equipment':{},'part_bounds':{}}
    fingerprint=[]
    names={o.name for o in bpy.context.scene.objects}
    for obj in sorted(bpy.context.scene.objects,key=lambda x:x.name):
        row={'name':obj.name,'type':obj.type,'parent':obj.parent.name if obj.parent else None,
             'scale':[round(float(s),9) for s in obj.scale],
             'modifiers':[{'name':m.name,'type':m.type,'width':m.width,'segments':m.segments} for m in obj.modifiers],
             'materials':[slot.material.name if slot.material else None for slot in obj.material_slots],
             'triangles':0}
        if obj.type=='MESH':
            evaluated=obj.evaluated_get(deps)
            mesh=evaluated.to_mesh()
            mesh.calc_loop_triangles()
            points=[evaluated.matrix_world@v.co for v in mesh.vertices]
            row['triangles']=len(mesh.loop_triangles)
            report['total_triangles']+=row['triangles']
            for p in points:
                if not all(math.isfinite(x) for x in p):
                    report['nonfinite_vertices']+=1
            triangles=[list(t.vertices) for t in mesh.loop_triangles]
            for tri in mesh.loop_triangles:
                a,b,c=(points[i] for i in tri.vertices)
                if (b-a).cross(c-a).length*.5<=1e-12:
                    report['zero_area_triangles']+=1
                    report['degenerate_details'].append({'object':obj.name,'indices':list(tri.vertices),
                                                         'area':(b-a).cross(c-a).length*.5,
                                                         'points':[list(a),list(b),list(c)]})
            report['missing_material_faces']+=sum(p.material_index>=len(obj.material_slots) for p in mesh.polygons)
            bounds={'min':[min(p[i] for p in points) for i in range(3)],
                    'max':[max(p[i] for p in points) for i in range(3)]}
            bounds['size']=[bounds['max'][i]-bounds['min'][i] for i in range(3)]
            report['part_bounds'][obj.name]=bounds
            report['all_x_min']=min(report['all_x_min'],bounds['min'][0])
            report['all_x_max']=max(report['all_x_max'],bounds['max'][0])
            radius=max(math.hypot(p.x,p.y) for p in points)
            if obj.name.startswith(FOOTPRINT):
                report['footprint_radius']=max(report['footprint_radius'],radius)
            if not obj.name.startswith(EQUIPMENT):
                report['body_z_min']=min(report['body_z_min'],bounds['min'][2])
                report['body_z_max']=max(report['body_z_max'],bounds['max'][2])
            if obj.name in ['HB_Shield','HB_Sword']:
                report['equipment'][obj.name]={'radius':radius,'overreach_beyond_collision':max(0,radius-.4),
                                              'bounds':bounds}
            fingerprint.append({'name':obj.name,'parent':row['parent'],'points':[[round(x,7) for x in p] for p in points],
                                'triangles':triangles,'modifiers':row['modifiers'],'materials':row['materials']})
            evaluated.to_mesh_clear()
        else:
            fingerprint.append({'name':obj.name,'parent':row['parent'],
                                'matrix_world':[[round(x,7) for x in r] for r in obj.matrix_world]})
        report['objects'].append(row)
    report['body_height']=report['body_z_max']-report['body_z_min']
    report['total_width']=report['all_x_max']-report['all_x_min']
    report['unique_materials']=sorted({m for o in report['objects'] for m in o['materials'] if m})
    report['geometry_fingerprint_sha256']=hashlib.sha256(json.dumps(fingerprint,sort_keys=True).encode()).hexdigest()
    checks={
        'required_names':set(REQUIRED)<=names,
        'required_parts_are_meshes':all(bpy.data.objects.get(n) is not None and bpy.data.objects[n].type=='MESH' for n in REQUIRED if not n.endswith('_socket')),
        'height_2_4_m_plus_minus_one_percent':abs(report['body_height']-2.4)<=.024,
        'soles_on_zero':abs(report['body_z_min'])<=1e-6,
        'footprint_radius_le_0_4':report['footprint_radius']<=.400001,
        'width_le_1_3':report['total_width']<=1.300001,
        'triangles_le_6000':report['total_triangles']<=6000,
        'only_two_materials':len(report['unique_materials'])<=2,
        'max_two_slots_per_part':all(len(o['materials'])<=2 for o in report['objects']),
        'no_nonfinite':report['nonfinite_vertices']==0,
        'no_zero_area':report['zero_area_triangles']==0,
        'no_missing_material_faces':report['missing_material_faces']==0,
        'only_hero_family_and_sockets':all(n.startswith('HB_') or n in ['Hand_R_socket','Forearm_L_socket'] for n in names),
        'only_meshes_and_empties':all(o['type'] in ['MESH','EMPTY'] for o in report['objects']),
        'unit_scale_applied':all(all(abs(s-1)<=1e-7 for s in o['scale']) for o in report['objects']),
        'metric_units':bpy.context.scene.unit_settings.system=='METRIC' and bpy.context.scene.unit_settings.scale_length==1,
        'sword_socket_parent':bpy.data.objects.get('HB_Sword') is not None and bpy.data.objects['HB_Sword'].parent.name=='Hand_R_socket',
        'shield_socket_parent':bpy.data.objects.get('HB_Shield') is not None and bpy.data.objects['HB_Shield'].parent.name=='Forearm_L_socket',
        'right_socket_forearm_parent':bpy.data.objects.get('Hand_R_socket') is not None and bpy.data.objects['Hand_R_socket'].type=='EMPTY' and bpy.data.objects['Hand_R_socket'].parent.name=='HB_Forearm_R',
        'left_socket_forearm_parent':bpy.data.objects.get('Forearm_L_socket') is not None and bpy.data.objects['Forearm_L_socket'].type=='EMPTY' and bpy.data.objects['Forearm_L_socket'].parent.name=='HB_Forearm_L',
        'root_on_origin':bpy.data.objects.get('HB_Root') is not None and bpy.data.objects['HB_Root'].location.length<=1e-7,
        'no_rig_or_actions':not bpy.data.armatures and not bpy.data.actions,
        'no_uv_or_image_textures':all(not o.data.uv_layers for o in bpy.context.scene.objects if o.type=='MESH') and not any(m.use_nodes and any(n.type=='TEX_IMAGE' for n in m.node_tree.nodes) for m in bpy.data.materials),
    }
    report['checks']=checks
    report['passed']=all(checks.values())
    return report

def main():
    if '--variant' in sys.argv and sys.argv[sys.argv.index('--variant')+1]=='p3':
        sys.dont_write_bytecode=True
        sys.path.insert(0,str(Path(__file__).resolve().parent))
        from validate_p3 import main as p3_main
        return p3_main()
    parser=argparse.ArgumentParser()
    parser.add_argument('--variant',choices=['p1','p2'],required=True)
    parser.add_argument('--output',type=Path)
    parser.add_argument('--compare',type=Path)
    args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    source=Path(bpy.data.filepath).resolve()
    if ART.resolve() not in source.parents and (ROOT/'.local/codex-tasks/hero-b').resolve() not in source.parents:
        raise ValueError('Load a blockout from approved folders')
    before=hashlib.sha256(source.read_bytes()).hexdigest()
    report=measure()
    report['source']=source.relative_to(ROOT).as_posix()
    report['source_sha256']=before
    report['variant']=args.variant
    report['blender_version']=bpy.app.version_string
    if args.compare:
        previous=json.loads(args.compare.read_text(encoding='utf-8'))
        report['checks']['matches_separate_process']=report['geometry_fingerprint_sha256']==previous['geometry_fingerprint_sha256']
        report['checks']['same_numbers_separate_process']=all(report[k]==previous[k] for k in
            ['body_height','body_z_min','body_z_max','footprint_radius','total_width','total_triangles',
             'nonfinite_vertices','zero_area_triangles','part_bounds','objects'])
        report['comparison_source']=args.compare.as_posix()
    report['checks']['source_unchanged']=before==hashlib.sha256(source.read_bytes()).hexdigest()
    report['passed']=all(report['checks'].values())
    output=args.output or ART/f'validation-{args.variant}.json'
    output=output.resolve()
    if ART.resolve() not in output.parents and (ROOT/'.local/codex-tasks/hero-b').resolve() not in output.parents:
        raise ValueError('Approved output folders only')
    output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'passed':report['passed'],'height':report['body_height'],
                      'radius':report['footprint_radius'],'width':report['total_width'],
                      'triangles':report['total_triangles'],'checks':report['checks']},indent=2))
    if not report['passed']:
        raise SystemExit(1)

if __name__=='__main__':
    main()

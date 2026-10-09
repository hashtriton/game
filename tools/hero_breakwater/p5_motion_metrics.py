"""Sample actual evaluated rig geometry and record contact and carry constraints."""
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
from p5_triangle_distance import separation
from mathutils import Vector
from mathutils.bvhtree import BVHTree

def evaluated(obj,deps):
    e=obj.evaluated_get(deps); m=e.to_mesh(); m.calc_loop_triangles(); pts=[e.matrix_world@v.co for v in m.vertices]; faces=[tuple(p.vertices) for p in m.loop_triangles]; e.to_mesh_clear(); return pts,faces

if __name__=='__main__':
    p=argparse.ArgumentParser(); p.add_argument('--output',type=Path,required=True); a=p.parse_args(sys.argv[sys.argv.index('--')+1:]); rig=bpy.data.objects['CharacterArmature']; parts=[o for o in bpy.data.objects if o.type=='MESH']; rows={}
    for name in ['Idle','Run','Attack','Death']:
        action=bpy.data.actions[name]; rig.animation_data.action=action; rig.animation_data.action_slot=action.slots[0]; samples=[]
        for i in range(97):
            frame=1+(action.frame_range[1]-1)*i/96; bpy.context.scene.frame_set(int(frame),subframe=frame%1); deps=bpy.context.evaluated_depsgraph_get(); points={o.name:evaluated(o,deps) for o in parts}
            allmin=min(v.z for pts,_ in points.values() for v in pts); feet=[v.z for n,(pts,_) in points.items() if n.startswith(('HB_Boot_','HB_Sabaton_')) for v in pts]
            shield=[v for n,(pts,_) in points.items() if n.startswith(('HB_Shield','HB_Rim')) for v in pts]; shoulder=(rig.pose.bones['Shoulder.L'].head.z+rig.pose.bones['Shoulder.R'].head.z)/2; helmet=max(v.z for v in points['HB_Helmet'][0])
            sp,sf=points['HB_Shield']; wp,wf=points['HB_Sword']; sb=BVHTree.FromPolygons(sp,sf); wb=BVHTree.FromPolygons(wp,wf)
            distance=min([sb.find_nearest(v)[3] for v in wp]+[wb.find_nearest(v)[3] for v in sp])
            exact_distance=separation(wp,wf,sp,sf)
            hp,hf=points['HB_Helmet']; head=BVHTree.FromPolygons(hp,hf)
            bodyparts=['HB_Torso','HB_Breastplate','HB_Back','HB_Pelvis']; bp=[]; bf=[]
            for part in bodyparts:
                pts,faces=points[part]; offset=len(bp); bp.extend(pts); bf.extend(tuple(v+offset for v in face) for face in faces)
            body=BVHTree.FromPolygons(bp,bf)
            distances=dict(sword_head=min(head.find_nearest(v)[3] for v in wp),shield_head=min(head.find_nearest(v)[3] for v in sp),sword_body=min(body.find_nearest(v)[3] for v in wp),shield_body=min(body.find_nearest(v)[3] for v in sp))
            samples.append(dict(sample=i,phase=i/96,all_min=allmin,feet_min=min(feet),shield_top=max(v.z for v in shield),shoulder_line=shoulder,helmet_top=helmet,shield_over_shoulder=max(v.z for v in shield)-shoulder,shield_over_helmet=max(v.z for v in shield)-helmet,sword_shield_vertex_surface_distance=distance,sword_shield_surface_distance=exact_distance,left_fist=list(rig.pose.bones['Fist.L'].head),surface_distances=distances))
        rows[name]=dict(samples=samples,all_min=min(s['all_min'] for s in samples),feet_min=min(s['feet_min'] for s in samples),feet_max=min(s['feet_min'] for s in samples) if False else max(s['feet_min'] for s in samples),shield_over_shoulder=max(s['shield_over_shoulder'] for s in samples),shield_over_helmet=max(s['shield_over_helmet'] for s in samples),sword_shield_vertex_surface_distance=min(s['sword_shield_vertex_surface_distance'] for s in samples))
        rows[name]['surface_distances']={key:min(s['surface_distances'][key] for s in samples) for key in distances}
        rows[name]['sword_shield_surface_distance']=min(s['sword_shield_surface_distance'] for s in samples)
        rows[name]['sword_shield_intersection_samples']=sum(s['sword_shield_surface_distance']<1e-7 for s in samples)
    a.output.write_text(json.dumps(dict(clips=rows,distance_method='Evaluated triangle surfaces: segment-triangle crossings, bidirectional vertex-face and all edge-edge pairs. Exact spatial distance at 97 clip samples, not continuous swept time. Body/head fields remain vertex-surface samples.'),indent=2),encoding='utf-8')
    print({n:{k:v for k,v in row.items() if k!='samples'} for n,row in rows.items()})

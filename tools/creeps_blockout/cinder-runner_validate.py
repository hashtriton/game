"""Geometry and hierarchy gates, separate reopen and rebuild comparisons."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path
sys.dont_write_bytecode=True
import bpy
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).resolve().parent))
import importlib.util
spec=importlib.util.spec_from_file_location('r1_build',Path(__file__).with_name('cinder-runner_build.py'))
builder=importlib.util.module_from_spec(spec)
spec.loader.exec_module(builder)

def measure():
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get()
    tris=0
    nonfinite=zero=0
    allpts=[]
    bodypts=[]
    footpts=[]
    rows=[]
    fingerprint=[]
    pivots={}
    for o in sorted(bpy.context.scene.objects,key=lambda o:o.name):
        pivots[o.name]=list(o.matrix_world.translation)
        row={'name':o.name,'parent':o.parent.name if o.parent else None,'scale':list(o.scale),'type':o.type}
        if o.type=='MESH':
            evaluated=o.evaluated_get(deps)
            mesh=evaluated.to_mesh()
            mesh.calc_loop_triangles()
            points=[evaluated.matrix_world@v.co for v in mesh.vertices]
            tris+=len(mesh.loop_triangles)
            nonfinite+=sum(not all(math.isfinite(c) for c in v) for v in points)
            zero+=sum(t.area<1e-10 for t in mesh.loop_triangles)
            allpts.extend(points)
            if o.name.startswith(('R1_Torso','R1_Pelvis','R1_Thigh','R1_Shin','R1_Foot','R1_Chest','R1_Belt')):
                bodypts.extend(points)
            if o.name.startswith('R1_Foot'):
                footpts.extend(points)
            row['triangles']=len(mesh.loop_triangles)
            row['materials']=[m.name for m in o.data.materials]
            row['bounds']={'min':[min(v[k] for v in points) for k in range(3)],'max':[max(v[k] for v in points) for k in range(3)]}
            fingerprint.append([o.name,[[round(c,7) for c in v] for v in points],[[int(i) for i in t.vertices] for t in mesh.loop_triangles]])
            evaluated.to_mesh_clear()
        rows.append(row)
    height=max(v.z for v in allpts)-min(v.z for v in footpts)
    radius=max(math.hypot(v.x,v.y) for v in bodypts)
    names={r['name'] for r in rows}
    required={'R1_Root','Hand_R_socket','R1_Cleaver','R1_Head','R1_Torso','R1_Pelvis'}|{'R1_'+stem+'_'+side for stem in ['UpperArm','Forearm','Thigh','Shin','Foot'] for side in ['L','R']}
    materials=sorted({m for row in rows for m in row.get('materials',[])})
    expected={'R1_Pelvis':'R1_Root','R1_Torso':'R1_Pelvis','R1_Head':'R1_Torso'}
    for side in ['L','R']:
        expected.update({'R1_UpperArm_'+side:'R1_Torso','R1_Forearm_'+side:'R1_UpperArm_'+side,'R1_Thigh_'+side:'R1_Pelvis','R1_Shin_'+side:'R1_Thigh_'+side,'R1_Foot_'+side:'R1_Shin_'+side})
    shader=[]
    for name in materials:
        m=bpy.data.materials[name]
        nodes=[]
        for n in sorted(m.node_tree.nodes,key=lambda n:n.name):
            vals={}
            for inp in n.inputs:
                if hasattr(inp,'default_value'):
                    val=inp.default_value
                    vals[inp.identifier]=val if isinstance(val,(float,int,bool,str)) else list(val)
            props={key:getattr(n,key) for key in ['operation','blend_type','samples','bands_direction','wave_type'] if hasattr(n,key)}
            nodes.append([n.name,n.bl_idname,vals,props])
        links=sorted([l.from_node.name,l.from_socket.identifier,l.to_node.name,l.to_socket.identifier] for l in m.node_tree.links)
        shader.append([name,nodes,links])
    checks={'required_names':required<=names,'prefixes':all(n.startswith('R1_') or n=='Hand_R_socket' for n in names),
            'root_origin':bpy.data.objects['R1_Root'].location.length<1e-7,
            'all_parts_parented':all(row['parent'] is not None for row in rows if row['name']!='R1_Root'),
            'major_parent_contract':all(bpy.data.objects[n].parent.name==p for n,p in expected.items()),
            'socket_parent':bpy.data.objects['Hand_R_socket'].parent.name=='R1_Forearm_R',
            'weapon_parent':bpy.data.objects['R1_Cleaver'].parent.name=='Hand_R_socket',
            'finite_vertices':nonfinite==0,'no_zero_area':zero==0,'unit_scales':all(all(abs(s-1)<1e-6 for s in r['scale']) for r in rows),
            'triangles_le_8000':tris<=8000,'footprint_le_0_5':radius<=.500001,'max_five_materials':len(materials)<=5,
            'metric':bpy.context.scene.unit_settings.system=='METRIC' and bpy.context.scene.unit_settings.scale_length==1,
            'soles_ground':abs(min(v.z for v in footpts))<1e-5,
            'both_soles_ground':all(abs(next(r for r in rows if r['name']=='R1_Foot_'+s)['bounds']['min'][2])<1e-5 for s in ['L','R']),
            'no_uv_images':all(not o.data.uv_layers for o in bpy.context.scene.objects if o.type=='MESH') and not any(n.type=='TEX_IMAGE' for m in bpy.data.materials if m.use_nodes for n in m.node_tree.nodes),
            'no_rig_claim':not bpy.data.armatures and not bpy.data.actions}
    if bpy.context.scene['pose_state']=='rest':
        checks['height_2_1']=abs(height-2.1)<.0021
    return {'height':height,'footprint_radius':radius,'triangles':tris,'nonfinite':nonfinite,'zero_area':zero,'materials':materials,'objects':rows,'pivots':pivots,
            'geometry_fingerprint':hashlib.sha256(json.dumps(fingerprint,separators=(',',':')).encode()).hexdigest(),
            'contract_fingerprint':hashlib.sha256(json.dumps([rows,pivots,shader],sort_keys=True).encode()).hexdigest(),'checks':checks,'passed':all(checks.values())}

def main():
    p=argparse.ArgumentParser()
    p.add_argument('--output',type=Path,default=ROOT/'art/creatures/cinder-runner/validation.json')
    p.add_argument('--compare',type=Path)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    source=Path(bpy.data.filepath)
    sha=hashlib.sha256(source.read_bytes()).hexdigest()
    saved=bpy.context.scene['pose_state']
    savedmeasure=measure()
    result={'blender':bpy.app.version_string,'source':str(source.relative_to(ROOT)),'source_sha256':sha,'saved_pose':saved,'revision':bpy.context.scene['revision']}
    for state in ['rest','display']:
        builder.pose(state)
        result[state]=measure()
    checks={'poses_pass':all(result[s]['passed'] for s in ['rest','display']),'saved_pose_consistent':savedmeasure['geometry_fingerprint']==result[saved]['geometry_fingerprint'],
            'source_unchanged':sha==hashlib.sha256(source.read_bytes()).hexdigest(),'same_triangles':result['rest']['triangles']==result['display']['triangles']}
    if a.compare:
        old=json.loads(a.compare.read_text())
        for state in ['rest','display']:
            checks[state+'_geometry_deterministic']=old[state]['geometry_fingerprint']==result[state]['geometry_fingerprint']
            checks[state+'_contract_deterministic']=old[state]['contract_fingerprint']==result[state]['contract_fingerprint']
    result['checks']=checks
    result['passed']=all(checks.values())
    a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    for state in ['rest','display']:
        print(state,{k:result[state][k] for k in ['height','footprint_radius','triangles']},'FAIL',[k for k,v in result[state]['checks'].items() if not v])
    print('R1 VALIDATION',result['passed'],checks)
    if not result['passed']:
        raise SystemExit(1)

if __name__=='__main__':
    main()

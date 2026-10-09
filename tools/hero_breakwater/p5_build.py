"""Build an isolated P5 form pass with the P4 hierarchy and six materials."""
import argparse
import json
import math
import sys
from pathlib import Path
sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Matrix, Vector
import p4_build as p4
from build_blockout import mesh_object

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/heroes/breakwater/p5'

def material(obj,key):
    obj.data.materials.clear(); obj.data.materials.append(bpy.data.materials[key])

def rim(side,mat):
    vertices=[]; faces=[]; n=20; cross=6
    for i in range(n):
        a=2*math.pi*i/n
        for j in range(cross):
            b=2*math.pi*j/cross
            vertices.append((side*.409+(.185+.022*math.cos(b))*math.cos(a),(.238+.022*math.cos(b))*math.sin(a),1.934+.022*math.sin(b)))
    for i in range(n):
        for j in range(cross): faces.append((i*cross+j,((i+1)%n)*cross+j,((i+1)%n)*cross+(j+1)%cross,i*cross+(j+1)%cross))
    return mesh_object('TmpRolledRim',vertices,faces,mat,smooth=True)

def dome(side,mat):
    rings=[(1.929,.185,.238),(2.02,.177,.220),(2.10,.140,.180),(2.155,.082,.112),(2.180,.018,.025)]
    rows=[[(side*.409+rx*math.cos(2*math.pi*i/16),ry*math.sin(2*math.pi*i/16),z) for i in range(17)] for z,rx,ry in rings]
    obj=p4.p3.surface('TmpDome',rows,mat,.055,0)
    # A small top disk closes the dome without a pole of degenerate quads.
    verts=[tuple(v.co) for v in obj.data.vertices]; faces=[tuple(p.vertices) for p in obj.data.polygons]; faces.append(tuple(4*17+i for i in range(16)))
    obj.data.clear_geometry(); obj.data.from_pydata(verts,[],faces); obj.data.update()
    return obj

def build(revision):
    p4.build(5)
    mats={k:bpy.data.materials[n] for k,n in dict(paint='HB_WhitePaintedSteel',navy='HB_NavyClothLeather',steel='HB_BrushedSteel',dark='HB_DarkJointSteel',cyan='HB_CyanAccent',bronze='HB_RestrainedBronze').items()}
    for side,s in [('L',1),('R',-1)]:
        arm=bpy.data.objects['HB_UpperArm_'+side]; rotation=arm.rotation_euler.copy(); arm.rotation_euler=(0,0,0); bpy.context.view_layer.update()
        p4.replace('HB_Pauldron_'+side,dome(s,mats['paint']))
        p4.replace('HB_PauldronEdge_'+side,rim(s,mats['steel']))
        for tier,prefix in enumerate(['HB_PauldronLip_','HB_PauldronLame_']):
            z=1.89-tier*.092
            rows=[[(s*.409+rx*math.cos(2*math.pi*i/16),ry*math.sin(2*math.pi*i/16),zz) for i in range(17)] for zz,rx,ry in [(z-.07,.174,.222),(z,.194,.245),(z+.038,.187,.238)]]
            p4.replace(prefix+side,p4.p3.surface('TmpLame',rows,mats['navy' if tier==0 else 'paint'],.035,0))
        if revision>=2:
            for prefix in ['HB_Pauldron_','HB_PauldronLip_','HB_PauldronLame_','HB_PauldronEdge_']:
                obj=bpy.data.objects[prefix+side]; world=obj.matrix_world.copy()
                for v in obj.data.vertices:
                    p=world@v.co; p.x=s*.409+(p.x-s*.409)*.84; v.co=world.inverted()@p
                obj.data.update()
                if prefix=='HB_Pauldron_':
                    for poly in obj.data.polygons: poly.use_smooth=True
        arm.rotation_euler=rotation; bpy.context.view_layer.update()
    shield=bpy.data.objects['HB_Shield']; inv=shield.matrix_world.inverted()
    for o in [shield]+list(shield.children_recursive):
        if o.type!='MESH': continue
        m=o.matrix_world.copy()
        for v in o.data.vertices:
            p=inv@m@v.co
            p.z *= 1.26/1.65
            if p.z<0: p.x *= .95
            if revision>=3 and o.name in ['HB_Shield','HB_Rim','HB_ShieldPanel','HB_Shield_Stripe']:
                p.z -= .10 if revision>=4 else .25
            v.co=m.inverted()@shield.matrix_world@p
    # The carry offset is baked into the rest hierarchy before the rig is generated.
    carry=shield.matrix_world.copy(); carry.translation.x += .095 if revision==1 else .13; carry.translation.z += .04
    shield.matrix_world=carry; bpy.context.view_layer.update()
    handle=bpy.data.objects['HB_ShieldHandle']; handle['grip_local']=list(Vector(handle['grip_local']) * (1.26/1.65))
    sword=bpy.data.objects['HB_Sword']; inv=sword.matrix_world.inverted()
    for v in sword.data.vertices:
        p=v.co; p.x *= .28/.22; p.y *= 1.25
    sword.data.update()
    if revision>=5:
        for v in sword.data.vertices: v.co.y *= .0945/.0675
        sword.data.update()
        p4.replace('HB_SwordGuard',p4.local_new(p4.box('TmpGuard',(.37,.14,.075),(0,0,-.087),mats['steel'],.005),sword.matrix_world))
    material(sword,mats['dark'].name)
    sword.data.materials.append(mats['steel'])
    for poly in sword.data.polygons:
        if abs(poly.center.x)>(.04 if revision>=5 else .065): poly.material_index=1
    for prefix in ['HB_Vambrace_','HB_Greave_','HB_Couter_']: 
        for side in ['L','R']: material(bpy.data.objects[prefix+side],mats['steel'].name)
    if revision>=2:
        for name in ['HB_Breastplate','HB_Back']:
            obj=bpy.data.objects[name]; obj.data.materials.append(mats['navy']); idx=len(obj.data.materials)-1
            for poly in obj.data.polygons:
                p=obj.matrix_world@poly.center
                if abs(p.x)<(.18 if name=='HB_Breastplate' else .21): poly.material_index=idx
        for side in ['L','R']:
            obj=bpy.data.objects['HB_Pauldron_'+side]; obj.data.materials.append(mats['navy'])
            for poly in obj.data.polygons:
                p=obj.matrix_world@poly.center
                if abs(p.x)<.43: poly.material_index=1
    else:
        material(bpy.data.objects['HB_Back'],mats['navy'].name)
    for o in bpy.context.scene.objects: o['variant']='p5'
    scene=bpy.context.scene; scene['hero_variant']='p5'; scene['p5_revision']=revision
    scene['p5_spec']=json.dumps(dict(shield_length=1.26,blade_width=.28,blade_thickness=.0945 if revision>=5 else .0675,blade_length=.90,guard_width=.37 if revision>=5 else .31,cap_thickness=.055,lame_thickness=.035))
    p4.p3.record_rest(); p4.pose('rest')
    return scene

if __name__=='__main__':
    p=argparse.ArgumentParser(); p.add_argument('--revision',type=int,default=5); p.add_argument('--output',type=Path,default=OUT/'blockout-p5.blend'); a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    output=a.output.resolve()
    if not any(base.resolve() in output.parents for base in [OUT,ROOT/'.local/codex-tasks/hero-p5']): raise ValueError('P5 outputs only')
    output.parent.mkdir(parents=True,exist_ok=True); bpy.context.preferences.filepaths.save_version=0
    build(a.revision); bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)
    p4.pose('display'); bpy.ops.wm.save_as_mainfile(filepath=str(output.with_name(output.stem+'-display.blend')),compress=True)
    print('P5 BUILT',a.revision)

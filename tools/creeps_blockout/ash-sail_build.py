"""E2 original asymmetric dorsal plate biped with rigid crescent weapon."""
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
from A_common import *


def blade(name,mat,ember,revision):
    n=14 if revision==1 else 22
    verts=[]
    centre=Vector((-1.43,-.37,1.13))
    for y in [-.425,-.315]:
        for r in [.27,.61]:
            for i in range(n+1):
                a=rad(118+205*i/n)
                tooth=.035 if r>.5 and i in [3,6,9] else 0
                rr=r+tooth
                if revision>=2 and r<.5:
                    rr=.61-(.018+.34*math.sin(math.pi*i/n)**.7)
                verts.append((centre.x+rr*math.cos(a),y,centre.z+rr*math.sin(a)))
    stride=n+1
    fs=[]
    for layer in range(2):
        base=layer*stride*2
        for i in range(n):
            f=(base+i,base+i+1,base+stride+i+1,base+stride+i)
            fs.append(f if layer==0 else tuple(reversed(f)))
    for edge in [0,stride]:
        for i in range(n):
            fs.append((edge+i,edge+2*stride+i,edge+2*stride+i+1,edge+i+1))
    fs += [(0,stride,3*stride,2*stride),(n,2*stride+n,3*stride+n,stride+n)]
    o=mesh_object(name,verts,fs,mat,smooth=True)
    bm=bmesh.new()
    bm.from_mesh(o.data)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(o.data)
    bm.free()
    o.data.update()
    o.data.set_sharp_from_angle(angle=rad(40))
    be=o.modifiers.new('Blade_edge','BEVEL')
    be.width=.006
    be.segments=2
    be.limit_method='ANGLE'
    be.angle_limit=rad(55)
    o.data.materials.append(ember)
    for i in range(n):
        o.data.polygons[2*n+n+i].material_index=1
    o['thickness_m']=.11
    return o


def build(revision=4,output=None):
    p='E2_'
    root=start(p)
    m=materials(p,revision)
    if revision>=2:
        for node in m['plate'].node_tree.nodes:
            if node.type=='MIX_RGB' and node.blend_type=='MIX':
                node.inputs[1].default_value=(.21,.19,.16,1)
                node.inputs[2].default_value=(.40,.36,.30,1)
        m['plate'].diffuse_color=(.21,.19,.16,1)
    if revision>=4:
        for mat,color in [(m['rock'],(.17,.19,.21)),(m['plate'],(.26,.235,.19))]:
            mat.diffuse_color=(*color,1)
            for node in mat.node_tree.nodes:
                if node.type=='MIX_RGB' and node.blend_type=='MIX':
                    node.inputs[1].default_value=(*color,1)
                    node.inputs[2].default_value=(*[c*1.7 for c in color],1)
        m['plate'].node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.34
        m['plate'].node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value=.78
        m['cloth'].node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.37,.045,.023,1)
        m['cloth'].diffuse_color=(.37,.045,.023,1)
    pelvis=attach(mineral(p+'Pelvis',(0,.025,1.51),(.54,.40,.35),m['dark'],.1,.86),root,(0,0,1.43),True)
    torso=attach(mineral(p+'Torso',(0,0,2.12),(.70,.46,1.05),m['dark'],.12,.83),pelvis,(0,0,2.18),True)
    chest_outline=[(-.35,2.49),(.30,2.48),(.34,2.28),(.09,2.01),(-.12,2.07),(-.36,2.32)] if revision<4 else [(-.39,2.49),(.28,2.64),(.37,2.36),(.22,2.25),(-.14,2.06),(-.41,2.28)]
    attach(plate(p+'Breastplate',chest_outline,.14,m['plate'],y=-.252),torso,footprint=True)
    attach(plate(p+'Abdomen',[(-.19,2.05),(.23,2.05),(.21,1.63),(-.23,1.66)],.09,m['rock'],y=-.21),torso,footprint=True)
    for j in range(3 if revision>=2 else 2):
        z=1.78+j*.14
        attach(plate(p+'RibPlate_'+str(j),[(.06,z+.11),(.34,z+.18),(.34,z+.11),(.08,z+.035)],.08,m['plate'],y=-.26),torso,footprint=True)
    attach(mineral(p+'BackSpine',(0,.26,2.12),(.26,.14,.95),m['plate'],.13,.87),torso,footprint=True)
    head_outline=[(-.35,3.04),(.24,3.16),(.31,2.97),(.015,2.74),(-.16,2.83)] if revision==1 else [(-.32,2.93),(.29,3.10),(.16,2.87),(-.015,2.68),(-.13,2.77)]
    head=attach(plate(p+'Head',head_outline,.34,m['rock'],y=-.03,crease=.94),torso,(0,0,2.75))
    seam=[(-.033,3.025),(.003,3.04),(.049,2.83),(.027,2.78),(.003,2.87)]
    if revision>=2:
        seam=[(x,z-.09) for x,z in seam]
    attach(plate(p+'FaceSeam',seam,.012,m['ember'],y=-.208),head)
    sail_outline=[(-.55,2.39),(.11,2.11),(.43,2.24),(1.05,3.5),(.52,3.03)]
    if revision==1:
        sail_outline=[(-.46,2.41),(.12,2.15),(.35,2.27),(.85,3.35),(.49,2.96)]
    elif revision>=3:
        sail_outline=[(-.57,2.40),(.10,2.10),(.60,2.24),(1.24,3.5),(.57,3.01)]
    sail=attach(plate(p+'Sail',sail_outline,.16,m['plate'],y=.42,crease=.96),torso,(.15,.35,2.42))
    for v in sail.data.vertices:
        v.co.y+=.22*v.co.x+.12*(v.co.z-.2)
    centre=Vector((.33,2.79))
    inset=[tuple(centre+(Vector(v)-centre)*(.77 if revision==1 else .88)) for v in sail_outline]
    under=attach(plate(p+'Sail_Underside',inset,.023,m['ember'],y=.313,crease=.97),sail)
    for v in under.data.vertices:
        v.co.y+=.22*v.co.x+.12*(v.co.z-2.62)
    for s,sign in [('L',1),('R',-1)]:
        shoulder=(sign*.40,0,2.50)
        elbow=(sign*.67,-.035,1.97)
        wrist=(sign*.93,-.085,1.41)
        arm=attach(segment(p+'UpperArm_'+s,shoulder,elbow,(.28,.30),m['dark'],.83),torso,shoulder)
        fore=attach(segment(p+'Forearm_'+s,elbow,wrist,(.29,.30),m['dark'],.84),arm,elbow)
        attach(plate(p+'ShoulderPlate_'+s,[(sign*.20,2.50),(sign*.40,2.68),(sign*.65,2.55),(sign*.58,2.37),(sign*.30,2.40)],.27,m['plate'],y=.00),arm)
        attach(plate(p+'BicepPlate_'+s,[(sign*.39,2.43),(sign*.58,2.44),(sign*.74,2.10),(sign*.60,2.01)],.12,m['rock'],y=-.13),arm)
        if s=='L':
            outline=[(.53,2.04),(.80,2.23),(1.04,1.84),(1.01,1.38),(.77,1.43),(.60,1.73)]
        else:
            outline=[(-.55,2.0),(-.78,1.93),(-.98,1.51),(-.74,1.46),(-.62,1.76)]
        attach(plate(p+'Vambrace_'+s,outline,.29 if s=='L' else .17,m['plate'],y=-.15),fore)
        hand=attach(mineral(p+'Hand_'+s,(sign*.96,-.09,1.30),(.29,.31,.28),m['rock'],.1,.89),fore,wrist)
        for j in range(3):
            attach(mineral(p+'GripFinger_'+s+'_'+str(j),(sign*(.875+j*.078),-.226,1.31),(.079,.16,.15),m['plate'],0,.88),hand)
        hip=(sign*.23,.015,1.43)
        knee=(sign*.275,-.015,.76)
        ankle=(sign*.29,.015,.17)
        thigh=attach(segment(p+'Thigh_'+s,hip,knee,(.32,.35),m['dark'],.84),pelvis,hip,True)
        shin=attach(segment(p+'Shin_'+s,knee,ankle,(.29,.29),m['dark'],.87),thigh,knee,True)
        attach(plate(p+'Cuisse_'+s,[(sign*.105,1.42),(sign*.38,1.42),(sign*.46,1.12),(sign*.29,.77),(sign*.17,.87)],.13,m['rock'],y=-.18),thigh,footprint=True)
        attach(plate(p+'Greave_'+s,[(sign*.13,.73),(sign*.37,.83),(sign*.43,.59),(sign*.37,.22),(sign*.19,.20)],.11,m['plate'],y=-.15),shin,footprint=True)
        foot=attach(mineral(p+'Foot_'+s,(sign*.29,-.10,.142),(.33,.49,.285),m['plate'],0,.95),shin,ankle,True)
        if revision>=3:
            attach(plate(p+'Poleyn_'+s,[(sign*.14,.85),(sign*.28,.88),(sign*.38,.81),(sign*.34,.70),(sign*.17,.66)],.11,m['plate'],y=-.115),shin,footprint=True)
            for j in range(2):
                attach(mineral(p+'Toe_'+s+'_'+str(j),(sign*(.22+j*.13),-.286,.105),(.13,.17,.17),m['rock'],0,.91),foot,footprint=True)
    attach(mineral(p+'Belt',(0,0,1.58),(.56,.46,.12),m['cloth'],.12,.86),pelvis,footprint=True)
    attach(cloth(p+'Cloth_Front',-.15,.17,1.57,.94,-.253,m['cloth']),pelvis,footprint=True)
    attach(cloth(p+'Cloth_L',.14,.25,1.53,1.18,-.203,m['cloth']),pelvis,footprint=True)
    attach(cloth(p+'Cloth_R',-.24,-.14,1.53,1.08,-.203,m['cloth']),pelvis,footprint=True)
    socket=bpy.data.objects.new('Hand_R_socket',None)
    bpy.context.collection.objects.link(socket)
    socket.location=(-.96,-.09,1.30)
    parent(socket,bpy.data.objects[p+'Forearm_R'])
    weapon=attach(blade(p+'Cleaver',m['plate'],m['ember'],revision),socket,(-.96,-.09,1.30),weapon=True)
    grip=attach(segment(p+'Cleaver_Handle',(-.82,-.09,1.36),(-1.43,-.37,1.13),(.085,.085),m['dark'],.94),weapon,weapon=True)
    attach(mineral(p+'Cleaver_Ferrule',(-1.30,-.32,1.18),(.17,.16,.16),m['plate'],0,.94),weapon,weapon=True)
    if revision>=2:
        attach(plate(p+'VambraceLip_L',[(.56,1.97),(.78,2.08),(.95,1.78),(.94,1.68),(.68,1.82)],.05,m['rock'],y=-.321),bpy.data.objects[p+'Forearm_L'])
    if revision>=3:
        attach(plate(p+'Collar',[(-.22,2.64),(.18,2.64),(.30,2.46),(.06,2.31),(-.27,2.51)],.10,m['rock'],y=-.263),torso,footprint=True)
    if revision>=2:
        for s in ['L','R']:
            for stem in ['Cuisse_','Greave_']:
                obj=bpy.data.objects[p+stem+s]
                for v in obj.data.vertices:
                    v.co.x*=.89
                    v.co.y*=.82
    if revision>=4:
        attach(plate(p+'ChestDiagonal',[(-.43,2.42),(.27,2.62),(.34,2.51),(-.38,2.28)],.065,m['rock'],y=-.343),torso,footprint=True)
        for s,sign in [('L',1),('R',-1)]:
            shin=bpy.data.objects[p+'Shin_'+s]
            attach(plate(p+'GreaveRidge_'+s,[(sign*.19,.73),(sign*.31,.82),(sign*.32,.28),(sign*.23,.23)],.048,m['rock'],y=-.21),shin,footprint=True)
            foot=bpy.data.objects[p+'Foot_'+s]
            for v in foot.data.vertices:
                v.co.z*=.87
                v.co.x*=1.06
            for j in range(2):
                toe=bpy.data.objects[p+'Toe_'+s+'_'+str(j)]
                for value in toe.data.attributes['crease_edge'].data:
                    value.value=.98
                for v in toe.data.vertices:
                    v.co.z+=.055*(v.co.y+.286)
            for stem in ['UpperArm_','Thigh_','Shin_']:
                obj=bpy.data.objects[p+stem+s]
                for value in obj.data.attributes['crease_edge'].data:
                    value.value=.93 if stem=='Thigh_' else .89
                for i,v in enumerate(obj.data.vertices):
                    if i%8 in [0,1]:
                        v.co.y-=.035
            obj=bpy.data.objects[p+'Vambrace_'+s]
            for v in obj.data.vertices:
                v.co.y+=.10*(v.co.z-1.65)
            for mod in obj.modifiers:
                if mod.type=='BEVEL':
                    mod.width=.007 if s=='L' else .0035
        for obj in [o for o in bpy.context.scene.objects if o.name.startswith(p+'Cloth_')]:
            for v in obj.data.vertices:
                v.co.y-=.085*math.sin((v.co.x+.24)*17)*(1.57-v.co.z)
        bpy.context.view_layer.update()
        deps=bpy.context.evaluated_depsgraph_get()
        for s in ['L','R']:
            foot=bpy.data.objects[p+'Foot_'+s].evaluated_get(deps)
            sole=min((foot.matrix_world @ v.co).z for v in foot.data.vertices)
            for j in range(2):
                obj=bpy.data.objects[p+'Toe_'+s+'_'+str(j)]
                ev=obj.evaluated_get(deps)
                toe_z=min((ev.matrix_world @ v.co).z for v in ev.data.vertices)
                delta=obj.matrix_world.inverted().to_3x3() @ Vector((0,0,sole+.004-toe_z))
                for v in obj.data.vertices:
                    v.co+=delta
    finish('ash-sail',p,revision,output)


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--revision',type=int,default=4)
    parser.add_argument('--output',type=Path)
    a=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    build(a.revision,a.output)

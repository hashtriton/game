"""Extend P3 in memory, keeping its named objects, hierarchy and joint pivots."""
import argparse
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Euler, Matrix, Vector
import build_p3 as p3
from build_blockout import box, mesh_object, parent
from p4_materials import create

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'art/heroes/breakwater/p4'
LOCAL = ROOT / '.local/codex-tasks/hero-b'
REVISION = 1


def replace(name, temp):
    target = bpy.data.objects[name]
    temp.data.transform(target.matrix_world.inverted() @ temp.matrix_world)
    old = target.data
    target.data = temp.data
    target.modifiers.clear()
    for modifier in temp.modifiers:
        new = target.modifiers.new(modifier.name, modifier.type)
        for prop in modifier.bl_rna.properties:
            if prop.identifier in ['name', 'type', 'rna_type'] or prop.is_readonly:
                continue
            try:
                setattr(new, prop.identifier, getattr(modifier, prop.identifier))
            except (AttributeError, TypeError, ValueError):
                pass
    bpy.data.objects.remove(temp, do_unlink=True)
    if old.users == 0:
        bpy.data.meshes.remove(old)
    return target


def attach(obj, target):
    parent(obj, target)
    p3.recenter(obj, target.matrix_world.translation)
    return obj


def cap(side, tier, material):
    if REVISION>=5:
        profile=[(.255,.329,2.155,.205,.245),(.34,.245,2.09,.21,.238),(.40,.178,1.965,.175,.221)][tier]
        inner,width,top,drop,depth=profile
        rows=[]
        for j in range(7):
            u=j/6
            rows.append([(side*(inner+width*u),(i/4-1)*depth,
                          top-drop*u**1.55+.018*math.sin(math.pi*u)-.035*(i/4-1)**2)
                         for i in range(9)])
        return p3.surface('TmpCap',rows,material,.034,0)
    if REVISION == 1:
        rows=[]
        for j in range(7):
            a=math.radians(-58+157*j/6)
            rows.append([(side*(.409+(.198-tier*.017)*math.sin(a)),
                          (i/4-1)*(.241-tier*.007),
                          1.974-tier*.087+(.226-tier*.020)*math.cos(a)*math.sqrt(1-.47*(i/4-1)**2))
                         for i in range(9)])
        return p3.surface('TmpCap',rows,material,.032,0)
    rings=[[(1.91,.183,.240),(2.045,.177,.232),(2.13,.126,.195),(2.19,.042,.103),(2.198,.004,.006)],
           [(1.845,.187,.233),(1.90,.191,.239),(1.945,.183,.235)],
           [(1.76,.164,.211),(1.81,.180,.223),(1.895,.183,.231)]][tier]
    rows=[]
    for z,rx,ry in rings:
        rows.append([(side*.409+rx*math.cos(2*math.pi*i/16),ry*math.sin(2*math.pi*i/16),z) for i in range(17)])
    obj=p3.surface('TmpCap',rows,material,.028,0)
    if tier==0:
        vertices=[tuple(v.co) for v in obj.data.vertices]
        faces=[tuple(p.vertices) for p in obj.data.polygons]
        faces.append(tuple(4*17+i for i in range(16)))
        temp=mesh_object('TmpClosedCap',vertices,faces,material,smooth=True)
        solid=temp.modifiers.new('Forged_thickness','SOLIDIFY')
        solid.thickness=.024
        solid.offset=0
        bpy.data.objects.remove(obj,do_unlink=True)
        obj=temp
    return obj


def shield_surface(material, name, edge=False):
    def curve(x, z):
        return -.035 - (.135 if REVISION>=5 else .105) * (1 - (x / .26) ** 2) + .008 * (z / .825) ** 2
    shape = [(-.20, -.825), (.20, -.825), (.23, -.74), (.26, .70),
             (.235, .825), (-.235, .825), (-.26, .70), (-.23, -.74)]
    if edge:
        rows = [[(x * t, curve(x * t, z * t) - .032, z * t) for x, z in shape + [shape[0]]]
                for t in [1, .89]]
        return p3.surface(name, rows, material, .05 if REVISION>=5 else .038, .003)
    rows = []
    for z, w in [(-.825, .20), (-.74, .23), (-.25, .245), (.35, .258), (.70, .26), (.825, .235)]:
        rows.append([(x, curve(x, z), z) for x in [-w, -w*.66, -w*.33, 0, w*.33, w*.66, w]])
    return p3.surface(name, rows, material, .068, .003)


def sword_mesh(material):
    vertices = []
    fractions = [-1, -.64, -.26, -.12, .12, .26, .64, 1]
    for z, w in [(-1.02, .003), (-.89, .076), (-.74, .105), (-.35, .110), (-.12, .110)]:
        for back in [False, True]:
            for f in fractions:
                y = -.027 if .15 < abs(f) < .8 else (-.013 if abs(f) < .15 else -.002)
                vertices.append((w * f, -y if back else y, z))
    faces = []
    for j in range(4):
        for k in range(7):
            a = j * 16 + k
            faces.append((a, a+1, a+17, a+16))
            a += 8
            faces.append((a+16, a+17, a+1, a))
        for k in [0, 7]:
            a = j*16 + k
            sideface=(a,a+16,a+24,a+8)
            faces.append(sideface if k==0 else tuple(reversed(sideface)))
    for j in [0, 4]:
        for k in range(7):
            a = j*16 + k
            endface=(a,a+8,a+9,a+1)
            faces.append(endface if j==0 else tuple(reversed(endface)))
    obj = mesh_object('TmpSword', vertices, faces, material)
    return obj


def local_new(obj, frame):
    obj.matrix_world = frame.copy() @ obj.matrix_world
    return obj


def grip_parts(side, frame, material, dark):
    hand = bpy.data.objects['HB_Gauntlet_' + side]
    rings = [(z, .091, .085, 0, 0) for z in [-.078, -.038, .054, .085]]
    temp = p3.volume('TmpHand', rings, dark, 12)
    local_new(temp, frame)
    replace(hand.name, temp)
    fingers = []
    for j in range(3):
        z = -.046 + j * .045
        rows = []
        for dz in [-.018, .018]:
            rows.append([(.062*math.sin(a), -.062*math.cos(a), z+dz)
                         for a in [math.radians(-115 + i*230/8) for i in range(9)]])
        fingers.append(p3.surface('TmpFinger', rows, material, .026, 0))
    verts, faces = [], []
    for part in fingers:
        deps = bpy.context.evaluated_depsgraph_get()
        mesh = part.evaluated_get(deps).to_mesh()
        offset = len(verts)
        verts.extend([tuple(v.co) for v in mesh.vertices])
        faces.extend([tuple(i+offset for i in f.vertices) for f in mesh.polygons])
        part.evaluated_get(deps).to_mesh_clear()
        bpy.data.objects.remove(part, do_unlink=True)
    fingers = mesh_object('HB_GripFingers_' + side, verts, faces, material, smooth=True)
    local_new(fingers, frame)
    attach(fingers, hand)
    temp = p3.plate_patch('TmpKnuckle', [(-.084,-.079,-.069),(.084,-.079,-.069),
                            (.082,-.079,.078),(-.082,-.079,.078)], (0,-.091,0), material,.003,.012)
    local_new(temp, frame)
    replace('HB_Knuckle_' + side, temp)
    thumb = box('HB_GripThumb_' + side, (.055,.07,.078), (.078,-.01,.031), material,.005)
    local_new(thumb, frame)
    attach(thumb, hand)


def align_hands():
    for side in ['L', 'R']:
        hand = bpy.data.objects['HB_Gauntlet_' + side]
        if side == 'R':
            sword = bpy.data.objects['HB_Sword']
            frame = sword.matrix_world.copy()
            frame.translation = sword.matrix_world @ Vector((0,0,0))
        else:
            handle = bpy.data.objects['HB_ShieldHandle']
            frame = handle.matrix_world.copy()
            frame.translation = handle.matrix_world @ Vector(handle['grip_local'])
        hand.matrix_world = frame @ Matrix(hand['grip_offset'])
    bpy.context.view_layer.update()


def pose(state):
    for o in bpy.context.scene.objects:
        if 'rest_location' in o:
            o.location = o['rest_location']
            o.rotation_euler = o['rest_rotation']
            o.scale = (1,1,1)
    bpy.context.view_layer.update()
    if state == 'display':
        pelvis = bpy.data.objects['HB_Pelvis']
        pelvis.location.z -= .065
        pelvis.location.y += .014
        torso = bpy.data.objects['HB_Torso']
        torso.rotation_euler.x += p3.rad(8)
        bpy.data.objects['HB_Helmet'].rotation_euler.x += p3.rad(5)
        for side, s, bend in [('L',1,20),('R',-1,35)]:
            thigh = bpy.data.objects['HB_Thigh_' + side]
            thigh.rotation_euler.x -= p3.rad(bend * .53)
            spread=9.1 if bpy.context.scene['revision']==1 else 5.0
            thigh.rotation_euler.y -= p3.rad(s*spread)
            shin = bpy.data.objects['HB_Shin_' + side]
            shin.rotation_euler.x += p3.rad(bend)
            boot = bpy.data.objects['HB_Boot_' + side]
            boot.rotation_euler.x -= p3.rad(bend*.47)
            boot.rotation_euler.y += p3.rad(s*spread)
            boot.rotation_euler.z += p3.rad(s*7)
            arm = bpy.data.objects['HB_UpperArm_' + side]
            arm.rotation_euler.y = p3.rad(-s*(13 if side=='R' else 9))
            arm.rotation_euler.x -= p3.rad(9)
            fore = bpy.data.objects['HB_Forearm_' + side]
            fore.rotation_euler.x -= p3.rad(40 if side=='L' else 12)
        bpy.data.objects['Forearm_L_socket'].rotation_euler.x += p3.rad(48)
        bpy.data.objects['HB_Shield'].rotation_euler.z -= p3.rad(10)
        bpy.context.view_layer.update()
        sword = bpy.data.objects['HB_Sword']
        angles=(-28,29,-10) if bpy.context.scene['revision']<=2 else ((-8,29,-10) if bpy.context.scene['revision']==3 else (-20,28,-7))
        desired = Euler(tuple(p3.rad(a) for a in angles), 'XYZ').to_quaternion()
        sword.rotation_euler = (sword.parent.matrix_world.to_quaternion().inverted() @ desired).to_euler()
        bpy.context.view_layer.update()
        deps = bpy.context.evaluated_depsgraph_get()
        bottoms = {}
        for side in ['L','R']:
            bottoms[side] = min((o.evaluated_get(deps).matrix_world @ v.co).z
                               for o in bpy.context.scene.objects if o.name in ['HB_Boot_'+side,'HB_Sabaton_'+side]
                               for v in o.evaluated_get(deps).data.vertices)
        pelvis.location.z -= min(bottoms.values())
        bpy.context.view_layer.update()
        for side in ['L','R']:
            boot = bpy.data.objects['HB_Boot_'+side]
            world = boot.matrix_world.copy()
            world.translation.z -= bottoms[side] - min(bottoms.values())
            boot.matrix_world = world
        bpy.context.view_layer.update()
    if state=='display':
        align_hands()
    bpy.context.scene['pose_state'] = state
    bpy.context.view_layer.update()


def build(revision):
    global REVISION
    REVISION = revision
    p3.build()
    mats = create()
    for o in bpy.context.scene.objects:
        if o.type != 'MESH':
            continue
        key = 'paint'
        if o.name.startswith(('HB_Torso','HB_Pelvis','HB_Thigh','HB_UpperArm','HB_Forearm','HB_Tabard','HB_Belt','HB_Pouches')):
            key = 'navy'
        elif o.name.startswith(('HB_Elbow','HB_Knee','HB_Gauntlet')):
            key = 'dark'
        elif o.name.startswith(('HB_Rim','HB_Collar','HB_Gorget','HB_ChestLip','HB_Brow','HB_Sword','HB_PauldronLip')):
            key = 'steel'
        if o.name in ['HB_Buckle','HB_SwordPommel']:
            key = 'bronze'
        if o.name == 'HB_Shield_Stripe':
            key = 'cyan'
        o.data.materials.clear()
        o.data.materials.append(mats[key])
    rotations = {}
    for side in ['L','R']:
        arm = bpy.data.objects['HB_UpperArm_'+side]
        rotations[side] = arm.rotation_euler.copy()
        arm.rotation_euler = (0,0,0)
    bpy.context.view_layer.update()
    for side, s in [('L',1),('R',-1)]:
        for tier, prefix in enumerate(['HB_Pauldron_','HB_PauldronLip_','HB_PauldronLame_']):
            replace(prefix+side, cap(s,tier,mats['steel' if tier==1 and revision<5 else 'paint']))
        for prefix in ['HB_Boot_','HB_Sabaton_']:
            o = bpy.data.objects[prefix+side]
            for v in o.data.vertices:
                v.co.x *= .82
        for prefix in ['HB_Forearm_','HB_UpperArm_']:
            o = bpy.data.objects[prefix+side]
            for v in o.data.vertices:
                v.co.x *= .93
                v.co.y *= .93
        if revision>=4:
            if revision>=5:
                rows=[]
                for u in [.96,1]:
                    rows.append([(s*(.255+.329*u),(i/4-1)*.245,
                                  2.155-.205*u**1.55+.018*math.sin(math.pi*u)-.035*(i/4-1)**2+.004) for i in range(9)])
                edge=p3.surface('HB_PauldronEdge_'+side,rows,mats['steel'],.017,0)
            else:
                edge=p3.wrap('HB_PauldronEdge_'+side,[(1.902,.188,.245,s*.409,0),(1.932,.185,.244,s*.409,0)],mats['steel'],n=16,thickness=.014)
            attach(edge,bpy.data.objects['HB_Pauldron_'+side])
            ax=s*.406
            cup=p3.wrap('TmpCouter',[(1.51,.100,.112,ax,-.010),(1.59,.142,.164,ax,-.006),(1.665,.108,.117,ax,0)],mats['paint'],-p3.rad(102),p3.rad(102),10,.024,.008)
            replace('HB_Couter_'+side,cup)
            vam=p3.wrap('TmpVambrace',[(1.213,.114,.126,ax,-.016),(1.335,.145,.146,ax,-.012),(1.51,.129,.144,ax,0)],mats['paint'],-p3.rad(106),p3.rad(106),12,.021,.013)
            replace('HB_Vambrace_'+side,vam)
        if revision>=5:
            x=s*.198
            knee=p3.wrap('TmpPoleyn',[(.500,.115,.132,x,-.012),(.589,.145,.180,x,-.008),(.677,.126,.144,x,0)],mats['paint'],-p3.rad(104),p3.rad(104),12,.025,.012)
            replace('HB_Poleyn_'+side,knee)
    shield = bpy.data.objects['HB_Shield']
    frame = shield.matrix_world.copy()
    replace('HB_Shield', local_new(shield_surface(mats['paint'],'TmpShield'),frame))
    replace('HB_Rim', local_new(shield_surface(mats['steel'],'TmpRim',True),frame))
    convexity=.135 if revision>=5 else .105
    rows = [[(x,-.035-convexity*(1-(x/.26)**2)-.035,z) for x in [.069,.109]] for z in [-.72,-.30,.30,.73]]
    replace('HB_Shield_Stripe', local_new(p3.surface('TmpStripe',rows,mats['cyan'],.004,0),frame))
    panel = p3.surface('TmpPanel', [[(-.17,.015,-.35),(.17,.015,-.35)],[(-.17,.015,.35),(.17,.015,.35)]],mats['dark'],.024,0)
    replace('HB_ShieldPanel',local_new(panel,frame))
    handle = p3.volume('HB_ShieldHandle',[(.045,.028,.026,-.115,.155),(.30,.028,.026,-.115,.155)],mats['navy'],10)
    local_new(handle,frame)
    attach(handle,shield)
    grip_world = frame @ Vector((-.115,.155,.175))
    handle['grip_local'] = list(handle.matrix_world.inverted() @ grip_world)
    strap = box('HB_ShieldStrap',(.23,.045,.092),(.0,.085,.47),mats['navy'],.006)
    local_new(strap,frame)
    attach(strap,shield)
    for i,z in enumerate([.05,.29]):
        mount = box('HB_ShieldHandleMount_'+str(i),(.055,.14,.045),(-.115,.10,z),mats['steel'],.004)
        local_new(mount,frame)
        attach(mount,shield)
    sword = bpy.data.objects['HB_Sword']
    swframe = sword.matrix_world.copy()
    replace('HB_Sword', local_new(sword_mesh(mats['steel']),swframe))
    replace('HB_SwordGuard',local_new(box('TmpGuard',(.31,.077,.060),(0,0,-.087),mats['steel'],.005),swframe))
    replace('HB_SwordGrip',local_new(p3.volume('TmpGrip',[(-.065,.032,.030,0,0),(.09,.032,.030,0,0)],mats['navy'],10),swframe))
    for side in ['L','R']:
        if side=='R':
            handframe=swframe.copy()
        else:
            handframe=handle.matrix_world.copy()
            handframe.translation=grip_world
        grip_parts(side,handframe,mats['steel'],mats['dark'])
        hand=bpy.data.objects['HB_Gauntlet_'+side]
        hand['grip_offset']=[list(r) for r in handframe.inverted() @ hand.matrix_world]
    brow = p3.wrap('TmpBrow',[(2.169,.181,.227,0,-.023),(2.215,.175,.213,0,-.021)],mats['steel'],-p3.rad(82),p3.rad(82),12,.025,.008)
    replace('HB_Brow',brow)
    visor = p3.wrap('HB_VisorShadow',[(2.125,.167,.195,0,-.022),(2.169,.168,.198,0,-.022)],mats['dark'],-p3.rad(71),p3.rad(71),10,.012,0)
    attach(visor,bpy.data.objects['HB_Helmet'])
    ridge = p3.surface('HB_CrownRidge', [[(-.009,y,z+.004),(.009,y,z+.004)] for y,z in [(-.15,2.31),(-.09,2.366),(.018,2.390),(.095,2.36),(.16,2.28)]],mats['steel'],.009,0)
    attach(ridge,bpy.data.objects['HB_Helmet'])
    for s in [-1,1]:
        for j in range(2):
            slot = box('HB_BreathSlot_'+str(s)+'_'+str(j),(.007,.006,.035),(s*(.052+j*.027),-.227,2.068),mats['dark'],.001)
            attach(slot,bpy.data.objects['HB_Helmet'])
    for side,s in [('L',1),('R',-1)]:
        accent=box('HB_VambraceAccent_'+side,(.040,.005,.105),(s*.406,-.168,1.40),mats['cyan'],.001)
        attach(accent,bpy.data.objects['HB_Vambrace_'+side])
    if revision>=4:
        inlay=p3.surface('HB_ChestInlay',[[(-w,y,z),(w,y,z)] for z,w,y in [(1.56,.029,-.307),(1.65,.026,-.323),(1.83,.024,-.322),(1.975,.018,-.252)]],mats['steel'],.005,0)
        attach(inlay,bpy.data.objects['HB_Torso'])
    for side in ['L','R']:
        bpy.data.objects['HB_UpperArm_'+side].rotation_euler=rotations[side]
    bpy.context.view_layer.update()
    for o in bpy.context.scene.objects:
        o['variant']='p4'
        if o.type=='MESH':
            o['role']='original procedural daylight plate study'
    for m in list(bpy.data.materials):
        if m.users==0:
            bpy.data.materials.remove(m)
    scene=bpy.context.scene
    scene['hero_variant']='p4'
    scene['revision']=revision
    scene['p4_spec']=json.dumps({'blade_length':.90,'blade_width':.22,'shield_top':.52,'shield_bottom':.40,'knee_degrees':[20,35],'torso_lean':8})
    p3.record_rest()
    pose('rest')
    for o in scene.objects:
        o.scale=(1,1,1)
    p3.record_rest()
    if revision>=3:
        pose('display')
        for o in scene.objects:
            if o.type!='MESH' or not o.name.startswith(('HB_Thigh','HB_Cuisse','HB_Knee','HB_Poleyn','HB_Shin','HB_Greave','HB_Boot','HB_Sabaton')):
                continue
            world=o.matrix_world.copy()
            inverse=world.inverted()
            # Tailor the outer plate corners to the collision footprint, without moving joint pivots.
            for v in o.data.vertices:
                p=world@v.co
                r=math.hypot(p.x,p.y)
                if r>.396:
                    p.x*=.396/r
                    p.y*=.396/r
                    v.co=inverse@p
        pose('rest')
    return scene


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--revision',type=int,default=5)
    p.add_argument('--output',type=Path,default=OUT/'blockout-p4.blend')
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    if not bpy.app.background:
        raise RuntimeError('Background only')
    output=a.output.resolve()
    if OUT.resolve() not in output.parents and LOCAL.resolve() not in output.parents:
        raise ValueError('P4 or task scratch outputs only')
    output.parent.mkdir(parents=True,exist_ok=True)
    bpy.context.preferences.filepaths.save_version=0
    build(a.revision)
    bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)
    pose('display')
    bpy.ops.wm.save_as_mainfile(filepath=str(output.with_name(output.stem+'-display.blend')),compress=True)
    print('BUILT P4',a.revision,len(bpy.context.scene.objects),'objects')


if __name__=='__main__':
    main()

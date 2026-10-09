"""Deterministic metre-scale clay blockout. Run in background Blender only."""
import argparse
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/heroes/breakwater'
SPEC = {
    'height': 2.4, 'hip_z': 1.10, 'knee_z': .61, 'belt_z': 1.34,
    'helmet_height': .36, 'helmet_width': .30, 'helmet_depth': .36,
    'torso_width': .78, 'torso_depth': .46, 'shoulder_span': 1.12,
    'shield_width': .46, 'shield_height': 1.64, 'shield_bottom': .14,
    'shield_x': .445, 'shield_y': -.345, 'shield_angle': -8,
    'blade_length': .79, 'blade_width': .180, 'sword_angle': 24,
    'p2_shoulder_factor': .92, 'p2_leg_factor': 1.06,
    'p2_head_factor': 1.05, 'p2_torso_width_factor': .96, 'p2_torso_depth_factor': .90,
}

def args():
    parser = argparse.ArgumentParser()
    parser.add_argument('--variant', choices=['p1', 'p2'], default='p1')
    parser.add_argument('--output', type=Path)
    return parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])

def mesh_object(name, vertices, faces, material, bevel=0, segments=1, smooth=False):
    mesh = bpy.data.meshes.new(name + '_mesh')
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    for face in mesh.polygons:
        face.use_smooth = smooth
    if bevel:
        mod = obj.modifiers.new('Plate_edge', 'BEVEL')
        mod.width = bevel
        mod.segments = segments
        mod.limit_method = 'ANGLE'
        mod.angle_limit = .55
        mod.affect = 'EDGES'
        mod.use_clamp_overlap = True
    return obj

def loft(name, rings, material, bevel=0, segments=1, smooth=False, n=8):
    vertices = []
    for z, rx, ry, cx, cy in rings:
        for i in range(n):
            a = 2 * math.pi * i / n
            vertices.append((cx + rx * math.cos(a), cy + ry * math.sin(a), z))
    faces = [tuple(reversed(range(n)))]
    for j in range(len(rings) - 1):
        for i in range(n):
            a = j * n + i
            b = j * n + (i + 1) % n
            faces.append((a, b, b + n, a + n))
    faces.append(tuple((len(rings) - 1) * n + i for i in range(n)))
    return mesh_object(name, vertices, faces, material, bevel, segments, smooth)

def box(name, size, location, material, bevel=0):
    x, y, z = (n / 2 for n in size)
    verts = [(a*x, b*y, c*z) for a, b, c in
             [(-1,-1,-1), (1,-1,-1), (1,1,-1), (-1,1,-1), (-1,-1,1), (1,-1,1), (1,1,1), (-1,1,1)]]
    faces = [(3,2,1,0), (0,1,5,4), (1,2,6,5), (2,3,7,6), (3,0,4,7), (4,5,6,7)]
    obj = mesh_object(name, verts, faces, material, bevel)
    obj.location = location
    return obj

def extruded_outline(name, outline_xz, front_y, back_y, material, bevel=0):
    n = len(outline_xz)
    vertices = [(x, y, z) for y in (front_y, back_y) for x, z in outline_xz]
    faces = [tuple(reversed(range(n))), tuple(range(n, 2*n))]
    for i in range(n):
        j = (i+1) % n
        faces.append((i, j, j+n, i+n))
    obj = mesh_object(name, vertices, faces, material, bevel)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    obj.select_set(False)
    return obj

def merge(name, objects, material):
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    obj = objects[0]
    obj.name = name
    obj.data.materials.clear()
    obj.data.materials.append(material)
    for face in obj.data.polygons:
        face.material_index = 0
    return obj

def origin(obj, point):
    bpy.context.view_layer.update()
    inv = obj.matrix_world.inverted()
    local = inv @ Vector(point)
    obj.data.transform(Matrix.Translation(-local))
    obj.matrix_world.translation = point

def parent(obj, target):
    bpy.context.view_layer.update()
    matrix = obj.matrix_world.copy()
    obj.parent = target
    obj.matrix_world = matrix
    bpy.context.view_layer.update()

def shoulder_plate(name, side, factor, material):
    outline=[(.210,1.884),(.202,2.105),(.245,2.182),(.354,2.171),
             (.490,2.098),(.560,1.967),(.535,1.853),(.399,1.815)]
    if side==1:
        outline=[(x,z+.013*(x-.21)/.35) for x,z in outline]
    vertices=[]
    for y,shrink in [(-.186,.89),(0,1),(.173,.92)]:
        for x,z in outline:
            vertices.append((side*(.38+(x-.38)*shrink)*factor,y*(1.03 if side==1 else 1),
                             2.0+(z-2.0)*shrink))
    n=len(outline)
    faces=[tuple(reversed(range(n))),tuple(range(2*n,3*n))]
    for j in range(2):
        for i in range(n):
            k=(i+1)%n
            faces.append((j*n+i,j*n+k,(j+1)*n+k,(j+1)*n+i))
    obj=mesh_object(name,vertices,faces,material,.012 if side==1 else .010)
    bpy.context.view_layer.objects.active=obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False)
    bpy.ops.object.mode_set(mode='OBJECT')
    obj.select_set(False)
    return obj

def main():
    options = args()
    p2 = options.variant == 'p2'
    leg = SPEC['p2_leg_factor'] if p2 else 1
    sh = (SPEC['p2_shoulder_factor'] if p2 else 1) * SPEC['shoulder_span']/1.12
    hw = SPEC['p2_head_factor'] if p2 else 1
    tw = (SPEC['p2_torso_width_factor'] if p2 else 1) * SPEC['torso_width']/.78
    td = (SPEC['p2_torso_depth_factor'] if p2 else 1) * SPEC['torso_depth']/.46
    dz = SPEC['hip_z'] * (leg - 1)
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.unit_settings.scale_length = 1
    bpy.context.preferences.filepaths.save_version = 0
    clay = bpy.data.materials.new('HB_Clay')
    clay.diffuse_color = (.43, .46, .49, 1)
    clay.use_nodes = True
    bsdf = clay.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = clay.diffuse_color
    bsdf.inputs['Roughness'].default_value = .68
    cyan = bpy.data.materials.new('HB_Cyan')
    cyan.diffuse_color = (.018, .47, .58, 1)
    cyan.use_nodes = True
    bsdf = cyan.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = cyan.diffuse_color
    bsdf.inputs['Roughness'].default_value = .70
    root = bpy.data.objects.new('HB_Root', None)
    bpy.context.collection.objects.link(root)
    root.empty_display_type = 'PLAIN_AXES'
    root.empty_display_size = .15

    torso = loft('HB_Torso', [
        (1.30+dz, .255*tw,.172*td,0,0), (1.40+dz,.288*tw,.195*td,0,0),
        (1.62+dz*.6,.382*tw,.228*td,0,-.018),
        (1.85+dz*.2,.390*tw,.213*td,0,-.010),
        (1.99,.312*tw,.158*td,0,.004)], clay, .012)
    parent(torso, root)
    origin(torso, (0,0,1.48+dz))
    chest_lip = loft('HB_ChestLip', [(1.535+dz*.7,.345*tw,.218*td,0,-.023),
                                   (1.565+dz*.7,.363*tw,.235*td,0,-.025),
                                   (1.610+dz*.7,.374*tw,.230*td,0,-.020)], clay,.004)
    parent(chest_lip, torso)
    back = loft('HB_Back', [(1.43+dz,.23*tw,.032,0,.162*td),
                           (1.68,.29*tw,.0475,0,.180*td),
                           (1.96,.235*tw,.030,0,.147*td)], clay,.009)
    parent(back,torso)
    collar = loft('HB_Collar', [(1.965,.217,.177,0,.010), (2.028,.228,.182,0,.010),
                              (2.103,.201,.161,0,.012), (2.129,.182,.146,0,.018)],clay,.004)
    parent(collar,torso)

    bottom = SPEC['height'] - SPEC['helmet_height'] * hw
    upper = loft('HB_Helmet', [(bottom+.158*hw,.15*hw,.177*hw,0,0),
                              (bottom+.24*hw,.145*hw,.164*hw,0,.008),
                              (bottom+.31*hw,.119*hw,.134*hw,0,.012),
                              (SPEC['height'],.049*hw,.071*hw,0,.013)],clay,.006,2,n=12)
    face = loft('HB_HelmetFace', [(bottom,.082*hw,.099*hw,0,-.027),
                                 (bottom+.056*hw,.120*hw,.149*hw,0,-.020),
                                 (bottom+.144*hw,.143*hw,.171*hw,0,-.004)],clay,0,n=12)
    brow = loft('HB_VisorRecess', [(bottom+.143*hw,.130*hw,.144*hw,0,.013),
                                 (bottom+.161*hw,.130*hw,.145*hw,0,.013)],clay,0,n=12)
    # The visor opening is front-only; a rear bridge avoids a robotic ring face.
    rear_outline=[(.140*hw*math.cos(i*math.pi/6), (.166*math.sin(i*math.pi/6)+.006)*hw) for i in range(7)]
    rear_vertices=[(x,y,bottom+z*hw) for z in [.131,.176] for x,y in rear_outline]
    rear_faces=[tuple(reversed(range(7))),tuple(range(7,14))]
    rear_faces.extend((i,(i+1)%7,(i+1)%7+7,i+7) for i in range(7))
    rear=mesh_object('HB_HelmetRear',rear_vertices,rear_faces,clay,0)
    for vertex in face.data.vertices:
        if vertex.co.y<-.05*hw:
            vertex.co.y-=.020*hw*(1-abs(vertex.co.x)/(.15*hw))
    helmet = merge('HB_Helmet',[upper,face,brow,rear],clay)
    for vertex in helmet.data.vertices:
        vertex.co.x*=SPEC['helmet_width']/.30
        vertex.co.y*=SPEC['helmet_depth']/.36
    origin(helmet,(0,0,2.14))
    parent(helmet,torso)

    pelvis = loft('HB_Pelvis',[(1.07+dz,.25,.150,0,0),(1.18+dz,.29,.1825,0,0),
                             (1.33+dz,.252,.170,0,0)],clay,.008)
    origin(pelvis,(0,0,1.10+dz))
    parent(pelvis,root)
    belt = loft('HB_Belt',[(1.2925+dz,.284,.195,0,0),(1.3875+dz,.2975,.2025,0,0)],clay,.003)
    parent(belt,pelvis)
    buckle = box('HB_Buckle',(.085,.037,.068),(0,-.211,1.34+dz),clay,.003)
    parent(buckle,belt)
    panels=[]
    for s in (-1,1):
        x=s*.119
        panel=extruded_outline('HB_TabardPanel',[(x-.103,1.30+dz),(x+.103,1.30+dz),
                                                (x+.100,1.010+dz),(x-.088,.990+dz)],-.206,-.157,clay,.003)
        panels.append(panel)
    tabard=merge('HB_Tabard',panels,clay)
    parent(tabard,pelvis)
    pouches=[]
    for s in (-1,1):
        pouch=box('HB_Pouch',(.125,.095,.165),(s*.238,.131,1.243+dz),clay,.006)
        pouch.rotation_euler[2]=s*math.radians(10)
        pouches.append(pouch)
    pouch=merge('HB_Pouches',pouches,clay)
    parent(pouch,belt)

    for side,s in [('L',1),('R',-1)]:
        sx=s*.375*sh
        shoulder=shoulder_plate('HB_Pauldron_'+side,s,sh,clay)
        origin(shoulder,(sx,0,1.975))
        shoulder.rotation_euler[1]=math.radians(s*(3 if s==1 else 1))
        parent(shoulder,torso)
        lip=loft('HB_PauldronLip_'+side,[(1.738,.111*sh,.138,s*.410*sh,-.004),
                                       (1.795,.145*sh,.168,s*.411*sh,-.004),
                                       (1.855,.151*sh,.173,s*.408*sh,-.004)],clay,.005)
        parent(lip,shoulder)
        arm=loft('HB_UpperArm_'+side,[(1.530,.095,.109,s*.440*sh,-.007),
                                     (1.634,.109,.1275,s*.428*sh,-.008),
                                     (1.895,.1125,.125,s*.389*sh,0)],clay,.010)
        origin(arm,(sx,0,1.91))
        parent(arm,torso)
        elbow=loft('HB_Elbow_'+side,[(1.489,.090,.088,s*.452*sh,-.023),
                                   (1.565,.102,.111,s*.450*sh,-.019)],clay,.004)
        parent(elbow,arm)
        forearm=loft('HB_Forearm_'+side,[
            (1.13,.092,.105,s*.483*sh,-.092),
            (1.265,.106,.128,s*.476*sh,-.070),
            (1.315,.125,.1425,s*.467*sh,-.059),
            (1.493,.111,.123,s*.452*sh,-.026),
            (1.540,.099,.114,s*.449*sh,-.020)],clay,.008)
        origin(forearm,(s*.452*sh,-.026,1.53))
        parent(forearm,arm)
        hand=loft('HB_Gauntlet_'+side,[(1.06,.080,.087,s*.483*sh,-.095),
                                      (1.115,.098,.104,s*.483*sh,-.092),
                                      (1.23,.097,.099,s*.483*sh,-.082)],clay,.006)
        parent(hand,forearm)
        x=s*.201
        thigh=loft('HB_Thigh_'+side,[(.640*leg,.107,.126,x,-.023),
                                    (.820*leg,.153,.159,x,-.008),
                                    (1.120*leg,.146,.154,x,.010)],clay,.010)
        origin(thigh,(x,0,1.10*leg))
        parent(thigh,pelvis)
        knee=loft('HB_Knee_'+side,[(.527*leg,.103,.127,x,-.036),
                                  (.595*leg,.150,.169,x,-.031),
                                  (.681*leg,.124,.143,x,-.025)],clay,.009)
        parent(knee,thigh)
        shin=loft('HB_Shin_'+side,[(.185*leg,.111,.130,x,.012),
                                  (.265*leg,.103,.115,x,.020),
                                  (.436*leg,.119,.142,x,-.010),
                                  (.600*leg,.1425,.160,x,-.012)],clay,.008)
        origin(shin,(x,-.018,.61*leg))
        parent(shin,thigh)
        boot=loft('HB_Boot_'+side,[(0,.136,.202,x,-.036),
                                  (.045,.140,.210,x,-.036),
                                  (.124,.133,.200,x,-.032),
                                  (.206*leg,.118,.164,x,.004),
                                  (.260*leg,.106,.120,x,.014)],clay,.008)
        origin(boot,(x,.014,.18*leg))
        parent(boot,shin)

    socket_r=bpy.data.objects.new('Hand_R_socket',None)
    bpy.context.collection.objects.link(socket_r)
    socket_r.location=(-.483*sh,-.092,1.20)
    socket_r.empty_display_size=.08
    parent(socket_r,bpy.data.objects['HB_Forearm_R'])
    socket_l=bpy.data.objects.new('Forearm_L_socket',None)
    bpy.context.collection.objects.link(socket_l)
    socket_l.location=(.467*sh,-.059,1.33)
    socket_l.empty_display_size=.08
    parent(socket_l,bpy.data.objects['HB_Forearm_L'])
    w=SPEC['shield_width']/2
    z0=SPEC['shield_bottom']
    h=SPEC['shield_height']
    outline=[(-w*.53,z0), (w*.53,z0), (w*.66,z0+.06), (w,z0+h-.07),
             (w*.83,z0+h),(-w*.83,z0+h),(-w,z0+h-.07),(-w*.66,z0+.06)]
    shield=extruded_outline('HB_Shield',outline,-.062,.053,clay,.016)
    shield.location=(SPEC['shield_x']*sh,SPEC['shield_y'],0)
    shield.rotation_euler[2]=math.radians(SPEC['shield_angle'])
    parent(shield,socket_l)
    stripe=extruded_outline('HB_Shield_Stripe',[(.047,z0+.047),(.080,z0+.047),
                                               (.080,z0+h-.027),(.047,z0+h-.027)],-.067,-.064,cyan,0)
    stripe.matrix_world=shield.matrix_world.copy()
    parent(stripe,shield)
    # The rim is a structural step and remains in the clay material.
    inset=[(x*.90, (z-(z0+h/2))*.958+(z0+h/2)) for x,z in outline]
    rim=extruded_outline('HB_ShieldPanel',inset,-.066,-.059,clay,.0015)
    rim.matrix_world=shield.matrix_world.copy()
    parent(rim,shield)
    stripe.matrix_world.translation=shield.matrix_world.translation
    # Move the accent in front of the inset without a textured mask.
    for v in stripe.data.vertices:
        v.co.y-=.007

    blade_half=SPEC['blade_width']/2
    blade_tip=-.158-SPEC['blade_length']
    blade_outline=[(-.012,blade_tip),(.012,blade_tip),(blade_half,blade_tip+.123),
                   (blade_half,-.158),(-blade_half,-.158),(-blade_half,blade_tip+.123)]
    blade=extruded_outline('HB_Sword',blade_outline,-.018,.018,clay,.0025)
    guard=box('HB_SwordGuard',(.260,.065,.045),(0,0,-.146),clay,0)
    grip=loft('HB_SwordGrip',[(-.125,.041,.035,0,0),(.043,.033,.032,0,0)],clay,0)
    pommel=box('HB_SwordPommel',(.074,.060,.055),(0,0,.061),clay,0)
    sword=merge('HB_Sword',[blade,guard,grip,pommel],clay)
    sword.location=(-.483*sh,-.092,1.20)
    sword.rotation_euler[0]=math.radians(-SPEC['sword_angle'])
    parent(sword,socket_r)

    for obj in scene.objects:
        if obj.type=='MESH':
            obj['role']='original clay blockout'
            obj['variant']=options.variant
    scene['hero_variant']=options.variant
    scene['spec']=json.dumps(SPEC,sort_keys=True)
    scene['rest_pose']='compact A, not yet bound to donor T'
    scene['revision']='review-fix-1'
    bpy.ops.object.select_all(action='DESELECT')
    bpy.context.view_layer.objects.active=torso
    torso.select_set(True)
    ART.mkdir(parents=True,exist_ok=True)
    output=options.output or ART / f'blockout-{options.variant}.blend'
    output=output.resolve()
    if ART.resolve() not in output.parents and (ROOT/'.local/codex-tasks/hero-b').resolve() not in output.parents:
        raise ValueError('Output must remain inside approved hero folders')
    bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)
    print(f'BUILT {options.variant}: {output}, objects={len(scene.objects)}')

if __name__=='__main__':
    main()

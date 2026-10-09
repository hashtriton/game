"""Original P3 plate forms and two reversible hierarchical poses."""
import argparse
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Euler, Matrix, Vector
from build_blockout import mesh_object, loft, box, origin, parent, merge

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/heroes/breakwater'
SCRATCH = ROOT / '.local/codex-tasks/hero-b'
SPEC = {'height': 2.4, 'helmet_height': .42, 'hip_z': 1.104,
        'knee_z': .59, 'chest_width': .80, 'chest_depth': .53,
        'shoulder_span': 1.23, 'thigh_width': .335, 'shin_width': .312,
        'boot_length': .47, 'forearm_width': .29,
        'shield_height': 1.65, 'shield_width': .49,
        'blade_length': .66, 'blade_width': .22, 'rest_arm_degrees': 25}
DISPLAY = {'knee_bend':14, 'leg_out':3, 'foot_yaw':4, 'pelvis_y':.06,
           'right_arm_out':22, 'left_arm_out':17}


def rad(value):
    return math.radians(value)


def surface(name, rows, material, thickness=.022, bevel=.003, angle=45):
    n = len(rows[0])
    vertices = [v for row in rows for v in row]
    faces = [(j*n+i, j*n+i+1, (j+1)*n+i+1, (j+1)*n+i)
             for j in range(len(rows)-1) for i in range(n-1)]
    obj = mesh_object(name, vertices, faces, material, smooth=True)
    obj.data.set_sharp_from_angle(angle=rad(angle))
    edge_use={tuple(sorted(e.vertices)):0 for e in obj.data.edges}
    for f in obj.data.polygons:
        for key in f.edge_keys:
            edge_use[tuple(sorted(key))]+=1
    for edge in obj.data.edges:
        if edge_use[tuple(sorted(edge.vertices))]==1:
            edge.use_edge_sharp=True
    solid = obj.modifiers.new('Forged_thickness', 'SOLIDIFY')
    solid.thickness = thickness
    solid.offset = 0
    solid.use_even_offset = True
    if bevel and name.startswith(('HB_Pauldron','HB_Shield','HB_Rim','HB_Brow')):
        edge = obj.modifiers.new('Forged_edge', 'BEVEL')
        edge.width = bevel
        edge.segments = 1
        edge.limit_method = 'ANGLE'
        edge.angle_limit = rad(75)
        edge.use_clamp_overlap = True
        edge.harden_normals = True
    obj['thickness_m'] = thickness
    obj['smooth_angle_degrees'] = angle
    return obj


def volume(name, rings, material, n=16, bevel=0):
    obj = loft(name, rings, material, bevel, 1, True, n)
    obj.data.set_sharp_from_angle(angle=rad(45))
    obj.data.polygons[0].use_smooth = False
    obj.data.polygons[-1].use_smooth = False
    return obj


def wrap(name, rings, material, start=-math.pi, stop=math.pi, n=12, thickness=.022, ridge=0):
    rows = []
    for z, rx, ry, cx, cy in rings:
        row = []
        for i in range(n+1):
            a = start+(stop-start)*i/n
            x = cx+rx*math.sin(a)
            y = cy-ry*math.cos(a)-ridge*max(0, 1-abs(math.sin(a))*4)*max(0, math.cos(a))
            row.append((x, y, z))
        rows.append(row)
    return surface(name, rows, material, thickness, .0025)


def plate_patch(name, outline, center, material, bulge=.04, thickness=.018, bevel=.002):
    cx, cy, cz = center
    vertices = [(cx, cy-bulge, cz)]+[(x, y, z) for x, y, z in outline]
    faces = [(0, i+1, (i+1) % len(outline)+1) for i in range(len(outline))]
    obj = mesh_object(name, vertices, faces, material, smooth=True)
    solid = obj.modifiers.new('Forged_thickness', 'SOLIDIFY')
    solid.thickness = thickness
    solid.offset = 0
    solid.use_even_offset = True
    if name.startswith(('HB_Cheek','HB_Couter','HB_Poleyn')):
        edge = obj.modifiers.new('Forged_edge', 'BEVEL')
        edge.width, edge.segments = bevel, 1
        edge.limit_method = 'ANGLE'
        edge.angle_limit = rad(75)
    obj.data.set_sharp_from_angle(angle=rad(40))
    obj['thickness_m'] = thickness
    return obj


def shoulder(name, side, tier, clay):
    rows = []
    steps=6 if tier==0 else 4
    across=12 if tier==0 else 8
    for j in range(steps+1):
        v = j/steps
        a = rad((-62+162*v) if tier==0 else (10+tier*10+(95-tier*10)*v))
        row = []
        for i in range(across+1):
            u = (i/across)*2-1
            taper = math.sqrt(1-.35*u*u)
            rx = (.225 if side == 1 else .215)*.94*(1-tier*.10)
            rz = (.260 if side==1 else .247)-tier*.025
            cx, cz = side*.414, 1.96-tier*.077
            x = cx+side*rx*math.sin(a)
            y = u*(.247-tier*.020)
            z = cz+rz*math.cos(a)*taper
            row.append((x, y, z))
        rows.append(row)
    return surface(name, rows, clay, .029-tier*.003, .004-tier*.001)


def setup_material(name, color):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = color
    bsdf.inputs['Roughness'].default_value = .54
    return mat


def record_rest():
    for o in bpy.context.scene.objects:
        o['rest_location'] = list(o.location)
        o['rest_rotation'] = list(o.rotation_euler)
        o['rest_parent_inverse'] = [v for row in o.matrix_parent_inverse for v in row]


def recenter(obj, point):
    children={c:c.matrix_world.copy() for c in obj.children}
    origin(obj,point)
    for child,world in children.items():
        child.matrix_world=world
    bpy.context.view_layer.update()


def pose(state):
    for o in bpy.context.scene.objects:
        if 'rest_location' in o:
            o.location = o['rest_location']
            o.rotation_euler = o['rest_rotation']
    if state == 'display':
        pelvis = bpy.data.objects['HB_Pelvis']
        pelvis.location.z -= .033
        pelvis.location.y += DISPLAY['pelvis_y']
        bpy.data.objects['HB_Torso'].rotation_euler.x += rad(6)
        for side, s in [('L', 1), ('R', -1)]:
            thigh = bpy.data.objects['HB_Thigh_'+side]
            thigh.rotation_euler.x -= rad(DISPLAY['knee_bend'])
            thigh.rotation_euler.y -= rad(s*DISPLAY['leg_out'])
            shin = bpy.data.objects['HB_Shin_'+side]
            shin.rotation_euler.x += rad(DISPLAY['knee_bend']*2)
            boot = bpy.data.objects['HB_Boot_'+side]
            boot.rotation_euler.x -= rad(DISPLAY['knee_bend'])
            boot.rotation_euler.y += rad(s*DISPLAY['leg_out'])
            boot.rotation_euler.z += rad(s*DISPLAY['foot_yaw'])
            arm = bpy.data.objects['HB_UpperArm_'+side]
            arm.rotation_euler.y += rad(s*(25-DISPLAY['left_arm_out' if side=='L' else 'right_arm_out']))
            arm.rotation_euler.x -= rad(12)
            fore = bpy.data.objects['HB_Forearm_'+side]
            fore.rotation_euler.x -= rad(50 if side == 'L' else 17)
        # Wrist attachment compensates forearm pitch to keep the shield upright.
        bpy.data.objects['Forearm_L_socket'].rotation_euler.x += rad(56)
        bpy.data.objects['HB_Shield'].rotation_euler.z -= rad(12)
        sword = bpy.data.objects['HB_Sword']
        bpy.context.view_layer.update()
        desired=Euler((rad(-25),rad(38),0),'XYZ').to_quaternion()
        sword.rotation_euler=(sword.parent.matrix_world.to_quaternion().inverted() @ desired).to_euler()
        bpy.context.view_layer.update()
        deps = bpy.context.evaluated_depsgraph_get()
        sole = min((o.evaluated_get(deps).matrix_world @ v.co).z
                   for o in bpy.context.scene.objects if o.name.startswith('HB_Boot_')
                   for v in o.evaluated_get(deps).data.vertices)
        pelvis.location.z -= sole
    bpy.context.scene['pose_state'] = state
    bpy.context.view_layer.update()


def build(revision=1):
    if not bpy.app.background:
        raise RuntimeError('Background CLI only')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for mat in list(bpy.data.materials):
        bpy.data.materials.remove(mat)
    bpy.context.preferences.filepaths.save_version = 0
    scene = bpy.context.scene
    scene.unit_settings.system, scene.unit_settings.scale_length = 'METRIC', 1
    clay = setup_material('HB_Clay', (.40, .43, .46, 1))
    cyan = setup_material('HB_Cyan', (.015, .48, .58, 1))
    root = bpy.data.objects.new('HB_Root', None)
    bpy.context.collection.objects.link(root)
    root.empty_display_size = .12
    pelvis = volume('HB_Pelvis', [(1.05,.28,.185,0,0),(1.17,.322,.203,0,0),
                                 (1.35,.268,.18,0,0)], clay)
    origin(pelvis, (0,0,SPEC['hip_z']))
    parent(pelvis, root)
    torso = volume('HB_Torso', [(1.30,.265,.19,0,0),(1.45,.32,.234,0,0),
                               (1.72,.398,.251,0,-.014),(1.94,.370,.218,0,0),
                               (2.07,.245,.145,0,0)], clay)
    for v in torso.data.vertices:
        v.co.x*=.92
        v.co.y*=.92
    origin(torso, (0,0,1.74))
    parent(torso, pelvis)
    chest = wrap('HB_Breastplate', [(1.50,.30,.228,0,-.018),(1.65,.375,.255,0,-.016),
                                  (1.83,.397,.253,0,-.015),(1.99,.318,.20,0,0)],
                 clay, -math.pi/2, math.pi/2, 12, .030, .046)
    for v in chest.data.vertices[:13]:
        v.co.z+=.075*abs(v.co.x)/.30
    parent(chest, torso)
    back = wrap('HB_Back', [(1.40,.29,.218,0,0),(1.66,.376,.263,0,0),
                           (1.92,.348,.217,0,0),(2.02,.246,.148,0,0)],
                clay, math.pi/2, 3*math.pi/2, 12, .022)
    parent(back, torso)
    chestlip = wrap('HB_ChestLip', [(1.47,.315,.239,0,-.012),(1.56,.357,.259,0,-.018)],
                    clay, -math.pi/2, math.pi/2, 12, .021, .014)
    for v in chestlip.data.vertices:
        v.co.z+=.075*abs(v.co.x)/.34
    parent(chestlip, torso)
    collar = wrap('HB_Collar', [(1.99,.253,.193,0,.006),(2.06,.228,.172,0,.009),
                               (2.115,.220,.167,0,.011)], clay, n=16, thickness=.026)
    parent(collar, torso)
    gorget = wrap('HB_Gorget', [(1.96,.27,.208,0,0),(2.009,.264,.204,0,0)],
                  clay, n=16, thickness=.025)
    parent(gorget, torso)
    helmet = volume('HB_Helmet', [(1.98,.116,.142,0,.025),(2.11,.174,.193,0,.012),
                                 (2.22,.178,.189,0,.015),(2.30,.157,.165,0,.018),
                                 (2.36,.109,.119,0,.020),(2.40,.032,.040,0,.020)], clay, 24)
    # Front slit is a true shadowed recess between two shell surfaces.
    for v in helmet.data.vertices:
        if v.co.y < -.07 and 2.09 < v.co.z < 2.23:
            v.co.y += .043
    origin(helmet, (0,0,2.08))
    parent(helmet, torso)
    face = wrap('HB_Faceplate', [(1.992,.081,.122,0,-.048),(2.052,.14,.185,0,-.042),
                               (2.125,.168,.204,0,-.024)], clay,
                -rad(72), rad(72), 10, .018, .019)
    parent(face, helmet)
    brow = wrap('HB_Brow', [(2.169,.175,.223,0,-.016),(2.200,.173,.214,0,-.016)],
                clay, -rad(82), rad(82), 12, .018, .006)
    parent(brow, helmet)
    for side,s in [('L',1),('R',-1)]:
        cheek = plate_patch('HB_Cheek_'+side,
            [(s*.135,-.16,2.15),(s*.181,-.095,2.15),(s*.168,-.035,2.039),
             (s*.113,-.116,2.018)], (s*.156,-.12,2.085), clay,.009,.019)
        parent(cheek, helmet)
    neckguard = wrap('HB_NeckGuard', [(1.985,.145,.158,0,.025),(2.02,.182,.191,0,.025),
                                    (2.16,.175,.185,0,.021)], clay,
                     math.pi/2, 3*math.pi/2, 10, .018)
    parent(neckguard, helmet)
    belt = wrap('HB_Belt', [(1.30,.281,.203,0,0),(1.375,.282,.202,0,0)], clay,
                n=16, thickness=.025)
    parent(belt, pelvis)
    buckle = box('HB_Buckle',(.095,.035,.072),(0,-.219,1.337),clay,.003)
    parent(buckle, belt)
    for tier in range(3):
        z=1.305-tier*.084
        fauld=wrap('HB_Fauld' if tier==0 else 'HB_Fauld_'+str(tier+1),
            [(z-.105,.316+tier*.014,.225+tier*.010,0,0),
             (z,.287+tier*.014,.211+tier*.010,0,0)],clay,n=16,thickness=.018)
        parent(fauld,pelvis)
    panels=[]
    for s in [-1,1]:
        cx=s*.120
        tab=plate_patch('HB_TabardPanel',[(cx-.102,-.24,1.285),(cx+.102,-.24,1.285),
                                        (cx+.098,-.252,.98),(cx-.09,-.255,.95)],
                         (cx,-.248,1.12),clay,.009,.016,.001)
        panels.append(tab)
    # Join evaluated-independent surfaces only; Solidify remains on the joined skirt.
    tabard=merge('HB_Tabard',panels,clay)
    parent(tabard,pelvis)
    pouches=[]
    for s in [-1,1]:
        pouch=box('HB_Pouch',(.13,.10,.18),(s*.238,.16,1.265),clay,.010)
        pouches.append(pouch)
    pouches=merge('HB_Pouches',pouches,clay)
    parent(pouches,belt)
    for side,s in [('L',1),('R',-1)]:
        ax=s*.406
        arm=volume('HB_UpperArm_'+side,[(1.57,.112,.125,ax,0),(1.70,.141,.142,ax,0),
                                      (1.91,.126,.135,ax,0)],clay,16)
        origin(arm,(ax,0,1.94))
        parent(arm,torso)
        for tier,name in enumerate(['HB_Pauldron_'+side,'HB_PauldronLip_'+side,'HB_PauldronLame_'+side]):
            sh=shoulder(name,s,tier,clay)
            parent(sh,arm)
        elbow=volume('HB_Elbow_'+side,[(1.535,.106,.111,ax,0),(1.61,.111,.116,ax,0)],clay,12)
        parent(elbow,arm)
        couter=plate_patch('HB_Couter_'+side,[(ax-.11,-.055,1.66),(ax+.11,-.055,1.66),
                   (ax+.14,-.065,1.585),(ax+.09,-.071,1.51),(ax-.09,-.071,1.51),(ax-.14,-.065,1.585)],
                   (ax,-.13,1.585),clay,.047,.022,.003)
        parent(couter,arm)
        fore=volume('HB_Forearm_'+side,[(1.19,.105,.116,ax,-.018),
                    (1.29,.134,.14,ax,-.014),(1.43,.142,.146,ax,-.006),
                    (1.57,.121,.128,ax,0)],clay,16)
        origin(fore,(ax,0,1.58))
        parent(fore,arm)
        vambrace=wrap('HB_Vambrace_'+side,[(1.22,.118,.13,ax,-.016),
                      (1.34,.146,.153,ax,-.01),(1.53,.132,.146,ax,0)],clay,
                      -rad(106),rad(106),10,.019,.010)
        parent(vambrace,fore)
        hand=volume('HB_Gauntlet_'+side,[(1.105,.092,.111,ax,-.018),
                     (1.165,.119,.12,ax,-.018),(1.258,.111,.116,ax,-.018)],clay,12)
        parent(hand,fore)
        knuckle=plate_patch('HB_Knuckle_'+side,[(ax-.094,-.132,1.13),(ax+.094,-.132,1.13),
                    (ax+.102,-.13,1.247),(ax-.102,-.13,1.247)],(ax,-.14,1.19),clay,.012,.017)
        parent(knuckle,hand)
        x=s*.198
        thigh=volume('HB_Thigh_'+side,[(.60,.124,.148,x,0),(.79,.170,.18,x,0),
                     (.985,.165,.171,x,0),(1.12,.150,.153,x,0)],clay,16)
        origin(thigh,(x,0,SPEC['hip_z']))
        parent(thigh,pelvis)
        cuisse=wrap('HB_Cuisse_'+side,[(.63,.138,.153,x,-.01),(.86,.174,.183,x,-.011),
                     (1.064,.169,.179,x,0)],clay,-rad(110),rad(110),10,.023,.006)
        parent(cuisse,thigh)
        knee=volume('HB_Knee_'+side,[(.505,.128,.139,x,0),(.595,.134,.145,x,0),
                                  (.665,.126,.136,x,0)],clay,12)
        parent(knee,thigh)
        poleyn=plate_patch('HB_Poleyn_'+side,[(x-.127,-.11,.662),(x+.127,-.11,.662),
                    (x+.143,-.115,.587),(x+.100,-.128,.493),(x-.096,-.128,.493),(x-.143,-.115,.587)],
                    (x,-.168,.588),clay,.017,.024,.0025)
        parent(poleyn,knee)
        shin=volume('HB_Shin_'+side,[(.18,.119,.14,x,.008),(.32,.129,.147,x,0),
                    (.46,.155,.174,x,0),(.58,.149,.157,x,0)],clay,16)
        origin(shin,(x,0,.59))
        parent(shin,thigh)
        greave=wrap('HB_Greave_'+side,[(.205,.131,.148,x,0),(.315,.140,.159,x,0),
                     (.46,.159,.182,x,0),(.552,.157,.17,x,0)],clay,
                     -rad(110),rad(110),10,.023,.018)
        parent(greave,shin)
        boot=volume('HB_Boot_'+side,[(0,.138,.235,x,-.035),(.035,.145,.235,x,-.035),
                     (.105,.147,.219,x,-.035),(.17,.132,.17,x,-.016),
                     (.29,.116,.128,x,.010)],clay,16)
        origin(boot,(x,.01,.15))
        parent(boot,shin)
        toe_rows=[]
        for y,z,w in [(-.264,.068,.025),(-.238,.125,.069),(-.150,.183,.096),(-.055,.215,.107)]:
            toe_rows.append([(x+u*w,y,z+.022*(1-u*u)) for u in [-1,-.66,-.33,0,.33,.66,1]])
        toe=surface('HB_Sabaton_'+side,toe_rows,clay,.019,.003)
        parent(toe,boot)
        arm.rotation_euler.y = rad(-s*SPEC['rest_arm_degrees'])
    bpy.context.view_layer.update()
    for name,side,z in [('Hand_R_socket','R',1.185),('Forearm_L_socket','L',1.39)]:
        fore=bpy.data.objects['HB_Forearm_'+side]
        s=1 if side=='L' else -1
        obj=bpy.data.objects.new(name,None)
        bpy.context.collection.objects.link(obj)
        rest_arm=bpy.data.objects['HB_UpperArm_'+side]
        pre=rest_arm.rotation_euler.copy()
        rest_arm.rotation_euler=(0,0,0)
        bpy.context.view_layer.update()
        obj.location=(s*.406,-.018,z)
        parent(obj,fore)
        rest_arm.rotation_euler=pre
        bpy.context.view_layer.update()
    socket=bpy.data.objects['Forearm_L_socket']
    sx=.49/2
    outline=[(-sx*.63,-.825),(sx*.63,-.825),(sx*.88,-.75),(sx,.70),
             (sx*.80,.825),(-sx*.80,.825),(-sx,.70),(-sx*.88,-.75)]
    def convex(x,z):
        return -.055-.090*(1-(x/sx)**2)+.005*(z/.825)**2
    rows=[]
    for z,w in [(-.825,sx*.63),(-.75,sx*.88),(-.20,sx*.94),(.35,sx),(.70,sx),(.825,sx*.80)]:
        rows.append([(x,convex(x,z),z) for x in [-w,-w*.66,-w*.33,0,w*.33,w*.66,w]])
    shield=surface('HB_Shield',rows,clay,.055,.004)
    shield.location=(.585,-.30,1.03)
    shield.rotation_euler.z=rad(-8)
    parent(shield,socket)
    rimrows=[]
    for shrink in [1,.88]:
        rimrows.append([(x*shrink,convex(x*shrink,z*shrink)-.035,z*shrink) for x,z in outline]+
                       [(outline[0][0]*shrink,convex(outline[0][0]*shrink,outline[0][1]*shrink)-.035,outline[0][1]*shrink)])
    rim=surface('HB_Rim',rimrows,clay,.026,.003)
    rim.matrix_world=shield.matrix_world.copy()
    parent(rim,shield)
    panel=surface('HB_ShieldPanel',[[(-.14,-.008,-.27),(.14,-.008,-.27)],
                                  [(-.14,-.008,.27),(.14,-.008,.27)]],clay,.025,.002)
    panel.matrix_world=shield.matrix_world.copy()
    parent(panel,shield)
    stripe=surface('HB_Shield_Stripe',[
        [(x,convex(x,z)-.030,z) for x in [.077,.113]] for z in [-.72,-.30,.30,.73]],cyan,.003,0)
    stripe.matrix_world=shield.matrix_world.copy()
    parent(stripe,shield)
    sw=bpy.data.objects['Hand_R_socket']
    rows=[]
    for z,w in [(-.79,.010),(-.69,.090),(-.42,.110),(-.13,.110)]:
        rows.append([(x, -.025 if abs(x)<.001 else .010*(1-abs(x)/w),z)
                     for x in [-w,-w*.3,0,w*.3,w]])
    sword=surface('HB_Sword',rows,clay,.028,.001)
    sword.location=sw.matrix_world.translation
    sword.location.x+=.025
    parent(sword,sw)
    guard=box('HB_SwordGuard',(.30,.07,.048),(0,0,-.109),clay,.004)
    grip=volume('HB_SwordGrip',[(-.075,.043,.037,0,0),(.07,.038,.034,0,0)],clay,10)
    pommel=volume('HB_SwordPommel',[(.07,.038,.034,0,0),(.095,.059,.048,0,0),
                                 (.13,.043,.038,0,0)],clay,10)
    for part in [guard,grip,pommel]:
        part.matrix_world.translation+=sword.matrix_world.translation
        parent(part,sword)
    bpy.context.view_layer.update()
    for obj in list(scene.objects):
        if obj.type!='MESH':
            continue
        target=None
        for side in ['L','R']:
            if obj.name.endswith('_'+side):
                if obj.name.startswith('HB_Pauldron'):
                    target='HB_UpperArm_'+side
                elif obj.name.startswith(('HB_Elbow','HB_Couter','HB_Vambrace')):
                    target='HB_Forearm_'+side
                elif obj.name.startswith('HB_Cuisse'):
                    target='HB_Thigh_'+side
                elif obj.name.startswith(('HB_Knee','HB_Poleyn','HB_Greave')):
                    target='HB_Shin_'+side
                elif obj.name.startswith('HB_Sabaton'):
                    target='HB_Boot_'+side
                elif obj.name.startswith('HB_Cheek'):
                    target='HB_Helmet'
                elif obj.name.startswith(('HB_Gauntlet','HB_Knuckle')):
                    fore=bpy.data.objects['HB_Forearm_'+side]
                    target_point=fore.matrix_world @ Vector((0,-.018,-.37))
                    recenter(obj,target_point)
        if obj.name in ['HB_Back','HB_Breastplate','HB_ChestLip']:
            target='HB_Torso'
        elif obj.name in ['HB_Collar','HB_Gorget','HB_Faceplate','HB_Brow','HB_NeckGuard']:
            target='HB_Helmet'
        elif obj.name.startswith(('HB_Belt','HB_Buckle','HB_Fauld','HB_Tabard','HB_Pouches')):
            target='HB_Pelvis'
        elif obj.name in ['HB_ShieldPanel','HB_Shield_Stripe','HB_Rim']:
            target='HB_Shield'
        elif obj.name.startswith('HB_Sword') and obj.name!='HB_Sword':
            target='HB_Sword'
        if target:
            recenter(obj,bpy.data.objects[target].matrix_world.translation)
    for o in scene.objects:
        o.scale=(1,1,1)
        o['variant']='p3'
        if o.type=='MESH':
            o['role']='original gray clay plate study'
    scene['hero_variant']='p3'
    scene['revision']=revision
    scene['spec']=json.dumps(SPEC,sort_keys=True)
    scene['display_rotations']=json.dumps(DISPLAY,sort_keys=True)
    record_rest()
    pose('rest')
    return scene


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--variant',choices=['p3'],default='p3')
    parser.add_argument('--pose',choices=['rest','display','both'],default='both')
    parser.add_argument('--revision',type=int,default=1)
    parser.add_argument('--output',type=Path)
    opt=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    build(opt.revision)
    output=(opt.output or ART/'blockout-p3.blend').resolve()
    if ART.resolve() not in output.parents and SCRATCH.resolve() not in output.parents:
        raise ValueError('Approved folders only')
    output.parent.mkdir(parents=True,exist_ok=True)
    if opt.pose in ['rest','both']:
        bpy.ops.wm.save_as_mainfile(filepath=str(output),compress=True)
    if opt.pose in ['display','both']:
        pose('display')
        display=output.with_name(output.stem+'-display.blend') if opt.pose=='both' else output
        bpy.ops.wm.save_as_mainfile(filepath=str(display),compress=True)
    print('BUILT P3 revision',opt.revision,'objects',len(bpy.context.scene.objects))


if __name__=='__main__':
    main()

"""Original R1 rigid blockout, background CLI only."""
import argparse
import math
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import bpy
from mathutils import Vector, Matrix
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'art/creatures/cinder-runner'
sys.path.insert(0, str(ROOT / 'tools/hero_breakwater'))
from build_blockout import mesh_object, loft, origin, parent, extruded_outline

def rad(v):
    return math.radians(v)

def material(name, color, metal, rough, kind):
    m = bpy.data.materials.new('R1_' + name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    n, l = m.node_tree.nodes, m.node_tree.links
    p = n.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = rough
    coord = n.new('ShaderNodeTexCoord')
    if kind == 'ember':
        p.inputs['Emission Color'].default_value = (*color, 1)
        p.inputs['Emission Strength'].default_value = .42
    elif kind == 'cloth':
        wave = n.new('ShaderNodeTexWave')
        wave.inputs['Scale'].default_value = 110
        wave.bands_direction = 'DIAGONAL'
        l.new(coord.outputs['Object'], wave.inputs['Vector'])
        bump = n.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .055
        bump.inputs['Distance'].default_value = .00065
        l.new(wave.outputs['Fac'], bump.inputs['Height'])
        l.new(bump.outputs['Normal'], p.inputs['Normal'])
        p.inputs['Sheen Weight'].default_value = .16
    else:
        bevel = n.new('ShaderNodeBevel')
        bevel.inputs['Radius'].default_value = .003
        bevel.samples = 3
        l.new(bevel.outputs['Normal'], p.inputs['Normal'])
        geom = n.new('ShaderNodeNewGeometry')
        dot = n.new('ShaderNodeVectorMath')
        dot.operation = 'DOT_PRODUCT'
        l.new(bevel.outputs['Normal'], dot.inputs[0])
        l.new(geom.outputs['Normal'], dot.inputs[1])
        wear = n.new('ShaderNodeMapRange')
        wear.inputs['From Min'].default_value = .94
        wear.inputs['From Max'].default_value = .999
        wear.inputs['To Min'].default_value = .55
        wear.inputs['To Max'].default_value = 0
        l.new(dot.outputs['Value'], wear.inputs['Value'])
        mix = n.new('ShaderNodeMixRGB')
        mix.inputs[1].default_value = (*color, 1)
        mix.inputs[2].default_value = (.26, .24, .21, 1) if kind == 'steel' else (.22, .16, .10, 1)
        l.new(wear.outputs['Result'], mix.inputs[0])
        ao = n.new('ShaderNodeAmbientOcclusion')
        ao.inputs['Distance'].default_value = .09
        dirt = n.new('ShaderNodeMixRGB')
        dirt.blend_type = 'MULTIPLY'
        dirt.inputs[0].default_value = .35
        l.new(mix.outputs['Color'], dirt.inputs[1])
        l.new(ao.outputs['AO'], dirt.inputs[2])
        l.new(dirt.outputs['Color'], p.inputs['Base Color'])
    return m

def empty(name, point, target=None):
    o = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(o)
    o.location = point
    if target:
        parent(o, target)
    return o

def attach(o, pivot, target):
    origin(o, pivot)
    parent(o, target)
    return o

def subvolume(name, rings, mat, pivot, target, sides=8, crease=.45):
    o = loft('R1_' + name, rings, mat, smooth=True, n=sides)
    c = o.data.attributes.new('crease_edge', 'FLOAT', 'EDGE')
    for e in o.data.edges:
        c.data[e.index].value = crease if e.vertices[0] // sides == e.vertices[1] // sides else (.34 if name=='Head' else .20)
    s = o.modifiers.new('Controlled_Subdivision', 'SUBSURF')
    s.levels = s.render_levels = 1
    o.data.set_sharp_from_angle(angle=rad(50))
    return attach(o, pivot, target)

def shell(name, rows, mat, pivot, target, thickness=.023, crease=.72):
    count = len(rows[0])
    verts = [v for row in rows for v in row]
    faces = [(j*count+i,j*count+i+1,(j+1)*count+i+1,(j+1)*count+i)
             for j in range(len(rows)-1) for i in range(count-1)]
    o = mesh_object('R1_' + name, verts, faces, mat, smooth=True)
    uses = {tuple(e.vertices): 0 for e in o.data.edges}
    for f in o.data.polygons:
        for key in f.edge_keys:
            key = tuple(sorted(key))
            uses[key] = uses.get(key, 0) + 1
    c = o.data.attributes.new('crease_edge', 'FLOAT', 'EDGE')
    for e in o.data.edges:
        c.data[e.index].value = crease if uses.get(tuple(sorted(e.vertices)), 0) == 1 else .15
    sub = o.modifiers.new('Plate_Curvature', 'SUBSURF')
    sub.levels = sub.render_levels = 1
    solid = o.modifiers.new('Real_Thickness', 'SOLIDIFY')
    solid.thickness = thickness
    solid.offset = 0
    if thickness>=.026:
        bevel = o.modifiers.new('Variable_Edge', 'BEVEL')
        bevel.width = .004
        bevel.segments = 1
        bevel.limit_method = 'ANGLE'
        bevel.angle_limit = rad(65)
    o.data.set_sharp_from_angle(angle=rad(50))
    o['thickness_m'] = thickness
    return attach(o, pivot, target)

def limb(name, a, b, radii, mat, target):
    axis = Vector(b)-Vector(a)
    basis = Vector((0, 0, 1)).rotation_difference(axis.normalized()).to_matrix()
    rings = []
    length = axis.length
    for t, r in radii:
        rings.append((length*t, r, r*.82, 0, 0))
    sides=4 if 'Finger' in name else (6 if 'Grip' in name else 8)
    o = subvolume(name, rings, mat, (0,0,0), target, sides=sides, crease=.5)
    # Bake world geometry while keeping the joint pivot and unit object scale.
    world = Matrix.Translation(Vector(a)) @ basis.to_4x4()
    o.data.transform(world)
    o.parent = None
    o.matrix_world = Matrix.Identity(4)
    attach(o, a, target)
    return o

def record_rest():
    bpy.context.view_layer.update()
    for o in bpy.context.scene.objects:
        o['rest_location'] = list(o.location)
        o['rest_rotation'] = list(o.rotation_euler)

def pose(state):
    for o in bpy.context.scene.objects:
        if 'rest_location' in o:
            o.location = o['rest_location']
            o.rotation_euler = o['rest_rotation']
    if state == 'display':
        rev = bpy.context.scene['revision']
        bpy.data.objects['R1_Torso'].rotation_euler.x += rad(34 if rev>=4 else (12 if rev == 1 else 21))
        bpy.data.objects['R1_Head'].rotation_euler.x += rad(9 if rev>=4 else 5)
        for side, sign in [('L',1),('R',-1)]:
            bpy.data.objects['R1_Thigh_'+side].rotation_euler.x -= rad(24 if rev>=4 else (12 if side=='L' else 20))
            bpy.data.objects['R1_Shin_'+side].rotation_euler.x += rad(45 if rev>=4 else (25 if side=='L' else 38))
            bpy.data.objects['R1_Foot_'+side].rotation_euler.x -= rad(21 if rev>=4 else (13 if side=='L' else 18))
            if rev>=4:
                bpy.data.objects['R1_Thigh_'+side].rotation_euler.y-=rad(sign*4)
                bpy.data.objects['R1_Foot_'+side].rotation_euler.y+=rad(sign*4)
                bpy.data.objects['R1_Foot_'+side].rotation_euler.z+=rad(sign*8)
            bpy.data.objects['R1_UpperArm_'+side].rotation_euler.x += rad(10 if side=='L' else (-18 if rev>=4 else -5))
            bpy.data.objects['R1_Forearm_'+side].rotation_euler.x -= rad(40 if rev>=4 and side=='L' else (20 if side=='L' else 12))
        bpy.context.view_layer.update()
        deps = bpy.context.evaluated_depsgraph_get()
        sole = min((o.evaluated_get(deps).matrix_world@v.co).z for o in bpy.context.scene.objects
                   if o.name.startswith('R1_Foot_') for v in o.evaluated_get(deps).data.vertices)
        bpy.data.objects['R1_Pelvis'].location.z -= sole
    bpy.context.scene['pose_state'] = state
    bpy.context.view_layer.update()

def build(revision=4):
    if not bpy.app.background:
        raise RuntimeError('Background CLI only')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for m in list(bpy.data.materials):
        bpy.data.materials.remove(m)
    bpy.context.preferences.filepaths.save_version = 0
    scene = bpy.context.scene
    scene.unit_settings.system, scene.unit_settings.scale_length = 'METRIC', 1
    scene['revision'] = revision
    dark = material('Basalt', (.040,.036,.030), .22, .65, 'basalt')
    steel = material('WornSteel', (.135,.126,.108), .60, .57 if revision>=4 else .40, 'steel')
    orange = material('OxideCollar', (.36,.075,.018), .05 if revision>=4 else .45, .78 if revision>=4 else .48, 'cloth' if revision>=4 else 'steel')
    ember = material('Ember', (.70,.115,.018), .15, .45, 'ember')
    cloth = material('RedCloth', (.20,.023,.014), 0, .78, 'cloth')
    root = empty('R1_Root', (0,0,0))
    pelvis = subvolume('Pelvis', [(1.00,.16,.13,0,0),(1.04,.21,.15,0,0),(1.20,.185,.13,0,-.01),(1.25,.16,.12,0,-.01)],dark,(0,0,1.04),root)
    torso = subvolume('Torso', [(1.21,.15,.11,0,-.025),(1.29,.18,.13,0,-.025),(1.53,.235,.16,0,-.035),(1.66,.23,.14,0,-.05),(1.71,.16,.10,0,-.05)],dark,(0,-.025,1.22),pelvis)
    shell('ChestPlate', [[(x,-.18-abs(x)*.06,z) for x,z in [(-.19,1.39),(0,1.33),(.19,1.39)]],
                         [(x,-.22+abs(x)*.22,z) for x,z in [(-.23,1.57),(0,1.62),(.23,1.57)]],
                         [(x,-.15+abs(x)*.12,z) for x,z in [(-.19,1.68),(0,1.66),(.19,1.68)]]],steel,(0,-.025,1.22),torso,.026)
    if revision>=4:
        for i in range(2):
            z=1.25+i*.085
            shell('AbdominalLame_'+str(i),[[(-.145,-.161,z),(0,-.175,z-.035),(.145,-.161,z)],
                                        [(-.17,-.168,z+.07),(0,-.195,z+.035),(.17,-.168,z+.07)]],steel,(0,-.025,1.22),torso,.018,.88)
    subvolume('Belt',[(1.17,.194,.142,0,-.01),(1.19,.20,.15,0,-.01),(1.25,.20,.15,0,-.01),(1.265,.192,.14,0,-.01)],steel,(0,0,1.04),pelvis,crease=.8)
    for sign, side in [(1,'L'),(-1,'R')]:
        x=sign*.18
        thigh=limb('Thigh_'+side,(x,0,1.04),(x,-.015,.55),[(0,.105),(.12,.125),(.68,.105),(1,.085)],dark,pelvis)
        shin=limb('Shin_'+side,(x,-.015,.55),(x,.015,.13),[(0,.085),(.17,.105),(.68,.074),(1,.06)],dark,thigh)
        foot=subvolume('Foot_'+side,[(0,.075,.18,x,-.09),(.018,.085,.19,x,-.09),(.085,.082,.18,x,-.085),(.17,.07,.08,x,.015)],steel,(x,.015,.13),shin,crease=.8)
        for stem,zs,width,depth,target in [('ThighPlate',(.65,.83,1.00),.115,.10,thigh),('ShinPlate',(.16,.34,.53),.095,.10,shin)]:
            rows=[[(x-width*.85,-depth,zs[0]),(x,-depth-.035,zs[0]-.025),(x+width*.85,-depth,zs[0])],
                  [(x-width,-depth,zs[1]),(x,-depth-.07,zs[1]),(x+width,-depth,zs[1])],
                  [(x-width*.88,-depth,zs[2]),(x,-depth-.04,zs[2]+.025),(x+width*.88,-depth,zs[2])]]
            shell(stem+'_'+side,rows,steel,target.matrix_world.translation,target,.024)
        shoulder=(sign*.28,-.025,1.65)
        elbow=(sign*.455,-.02,1.32)
        wrist=(sign*.59,-.055,1.055)
        arm=limb('UpperArm_'+side,shoulder,elbow,[(0,.09),(.18,.105),(.75,.085),(1,.074)],dark,torso)
        fore=limb('Forearm_'+side,elbow,wrist,[(0,.075),(.24,.105),(.73,.085),(1,.064)],steel,arm)
        hand=limb('Hand_'+side,wrist,(sign*.625,-.065,.92),[(0,.063),(.28,.082),(.85,.075),(1,.054)],dark,fore)
        for tier in range(2):
            rows=[]
            if revision>=4 and tier==0:
                for x,z in [(.18,1.755),(.315,1.72),(.465,1.575)]:
                    rows.append([(sign*x,-.035+u*.16,z-.035*u*u+(.022 if u==0 else 0)) for u in [-1,-.5,0,.5,1]])
            else:
                for j in range(3):
                    a=rad(-35+j*65)
                    rows.append([(sign*(.285+(.15-tier*.025)*math.sin(a)),-.025+u*.14,1.66+(.105-tier*.025)*math.cos(a)-tier*.07-.035*u*u) for u in [-1,-.5,0,.5,1]])
            shell('ShoulderPlate_'+side+'_'+str(tier),rows,orange if revision>=2 and tier==0 else steel,shoulder,arm,.03-tier*.008)
        if side=='L':
            for f in range(3):
                fx=sign*.625+(f-1)*.035
                limb('Finger_L_'+str(f),(fx,-.075,.94),(fx,-.135,.855+(f==2)*.024),[(0,.016),(.4,.018),(1,.011)],steel,hand)
        else:
            socket=empty('Hand_R_socket',(sign*.625,-.065,.955),fore)
            grip=limb('CleaverGrip',(sign*.625,-.15,.955),(sign*.625,.02,.955),[(0,.025),(.10,.03),(.90,.03),(1,.025)],dark,socket)
            width=.14 if revision==1 else .205
            outline=[(-.605,.985),(-.65,.79),(-.82,.30),(-.82-width,.27),(-.88-width,.36),(-.665,.985)]
            blade=extruded_outline('R1_Cleaver',outline,-.087,-.035,steel,.004)
            attach(blade,(-.625,-.065,.955),socket)
            shell('CleaverEdge',[[(-.82-width,-.092,.275),(-.88-width,-.092,.36),(-.745,-.092,.86)],
                                 [(-.82-width+.033,-.092,.30),(-.88-width+.037,-.092,.36),(-.705,-.092,.86)]],steel,(-.625,-.065,.955),blade,.009)
            for f in range(3):
                limb('GripFinger_R_'+str(f),(-.665,-.12+f*.045,.971),(-.585,-.12+f*.045,.935),[(0,.017),(.3,.021),(.7,.021),(1,.017)],dark,hand)
        clothrows=[]
        for z,w,y in [(1.18,.092,-.176),(1.05,.104,-.19),(.89,.118,-.215)]:
            clothrows.append([(sign*.105+u*w,y-.015*(1-u*u),z+.025*abs(u)) for u in [-1,0,1]])
        shell('Cloth_'+side,clothrows,cloth,(sign*.105,-.176,1.18),pelvis,.012,.8)
    collarwidth=.31 if revision<3 else .37
    for side,sign in [('L',1),('R',-1)]:
        rows=[]
        for z,x,y in [(1.53,.19,-.18),(1.68,collarwidth,-.04),(1.75 if revision>=4 else 1.80,.20,.09)]:
            rows.append([(sign*x*(1-.10*u*u),y+u*.17,z-.045*u*u) for u in [-1,-.5,0,.5,1]])
        collar=shell('Collar_'+side,rows,orange,(0,-.025,1.22),torso,.034,.84)
        shell('CollarHotUnderside_'+side,[[(sign*x,y+.006,z-.023) for x,y,z in [(v[0]*sign,v[1],v[2]) for v in row]] for row in rows[:2]],ember,(0,-.025,1.22),torso,.01,.85)
    hood=subvolume('Head',[(1.75,.09,.115,0,-.13),(1.79,.135,.15,0,-.12),(1.95,.14,.155,0,-.075),(2.045,.065,.085,0,-.075),(2.10,.012,.026,0,-.07)],steel,(0,-.07,1.78),torso,sides=8,crease=.86)
    shell('HoodFace',[[(-.095,-.215,1.84),(0,-.255,1.77),(.095,-.215,1.84)],
                      [(-.107,-.181,1.92),(0,-.225,1.93),(.107,-.181,1.92)],
                      [(-.06,-.10,2.03),(0,-.125,2.06),(.06,-.10,2.03)]],steel,(0,-.07,1.78),hood,.021,.92)
    for sign in [-1,1]:
        shell('EyeRecess_'+str(sign),[[(sign*.030,-.224,1.905),(sign*.077,-.207,1.915)],[(sign*.032,-.222,1.918),(sign*.074,-.205,1.927)]],dark,(0,-.07,1.78),hood,.003,.95)
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get()
    zmax=max((o.evaluated_get(deps).matrix_world@v.co).z for o in scene.objects if o.type=='MESH' for v in o.evaluated_get(deps).data.vertices)
    factor=2.1/zmax
    for o in scene.objects:
        if o.type=='MESH':
            world=o.matrix_world.copy()
            coords=[world@v.co for v in o.data.vertices]
            for v,p in zip(o.data.vertices,coords):
                p.z*=factor
                v.co=world.inverted()@p
    record_rest()
    pose('rest')
    return scene

def main():
    p=argparse.ArgumentParser()
    p.add_argument('--revision',type=int,default=4)
    p.add_argument('--output',type=Path,default=OUT)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    dest=a.output.resolve()
    if dest!=OUT.resolve() and (ROOT/'.local/codex-tasks/creeps').resolve() not in dest.parents:
        raise ValueError('R1 or task outputs only')
    dest.mkdir(parents=True,exist_ok=True)
    build(a.revision)
    for state in ['rest','display']:
        pose(state)
        bpy.ops.wm.save_as_mainfile(filepath=str(dest/('blockout-'+state+'.blend')))
    print('R1 BUILD',a.revision)

if __name__=='__main__':
    main()

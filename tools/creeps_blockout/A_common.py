"""Original rigid mineral and plate volumes with joint-centred transforms."""
import hashlib
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import bpy
import bmesh
from mathutils import Euler, Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'tools/hero_breakwater'))
from build_blockout import mesh_object, origin, parent


def rad(v):
    return math.radians(v)


def start(prefix):
    if not bpy.app.background:
        raise RuntimeError('Background CLI only')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for m in list(bpy.data.materials):
        bpy.data.materials.remove(m)
    bpy.context.preferences.filepaths.save_version = 0
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1
    o = bpy.data.objects.new(prefix+'Root', None)
    bpy.context.collection.objects.link(o)
    o.empty_display_size = .14
    return o


def material(name, color, roughness, metallic=0, cloth=False, emission=0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    m.diffuse_color = (*color,1)
    ns, ls = m.node_tree.nodes, m.node_tree.links
    b = ns.get('Principled BSDF')
    b.inputs['Base Color'].default_value = (*color,1)
    b.inputs['Roughness'].default_value = roughness
    b.inputs['Metallic'].default_value = metallic
    if emission:
        b.inputs['Emission Color'].default_value = (*color,1)
        b.inputs['Emission Strength'].default_value = emission
        return m
    coord = ns.new('ShaderNodeTexCoord')
    if cloth:
        wave = ns.new('ShaderNodeTexWave')
        wave.inputs['Scale'].default_value = 110
        wave.bands_direction = 'DIAGONAL'
        ls.new(coord.outputs['Object'], wave.inputs['Vector'])
        bump = ns.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .055
        bump.inputs['Distance'].default_value = .0006
        ls.new(wave.outputs['Fac'], bump.inputs['Height'])
        ls.new(bump.outputs['Normal'], b.inputs['Normal'])
        b.inputs['Sheen Weight'].default_value = .12
    else:
        bevel = ns.new('ShaderNodeBevel')
        bevel.samples = 3
        bevel.inputs['Radius'].default_value = .007 if metallic < .1 else .0035
        ls.new(bevel.outputs['Normal'], b.inputs['Normal'])
        geo = ns.new('ShaderNodeNewGeometry')
        dot = ns.new('ShaderNodeVectorMath')
        dot.operation = 'DOT_PRODUCT'
        ls.new(bevel.outputs['Normal'], dot.inputs[0])
        ls.new(geo.outputs['Normal'], dot.inputs[1])
        remap = ns.new('ShaderNodeMapRange')
        remap.inputs['From Min'].default_value = .90
        remap.inputs['From Max'].default_value = .9998
        remap.inputs['To Min'].default_value = .8
        remap.inputs['To Max'].default_value = 0
        mix = ns.new('ShaderNodeMixRGB')
        mix.inputs[1].default_value = (*color,1)
        mix.inputs[2].default_value = (*[min(1,c*1.65+.022) for c in color],1)
        ls.new(dot.outputs['Value'], remap.inputs['Value'])
        ls.new(remap.outputs['Result'], mix.inputs[0])
        ao = ns.new('ShaderNodeAmbientOcclusion')
        ao.inputs['Distance'].default_value = .11
        ao.samples = 8
        dirt = ns.new('ShaderNodeMapRange')
        dirt.inputs['To Min'].default_value = .72
        dirt.inputs['To Max'].default_value = 1
        ls.new(ao.outputs['AO'], dirt.inputs['Value'])
        shade = ns.new('ShaderNodeMixRGB')
        shade.blend_type = 'MULTIPLY'
        shade.inputs[0].default_value = 1
        ls.new(mix.outputs['Color'], shade.inputs[1])
        ls.new(dirt.outputs['Result'], shade.inputs[2])
        ls.new(shade.outputs['Color'], b.inputs['Base Color'])
    return m


def materials(prefix, revision=1):
    return {'rock': material(prefix+'Basalt',(.065,.074,.082) if revision==1 else (.13,.145,.155),.67),
            'plate': material(prefix+'BlackenedPlate',(.105,.094,.083),.44,.62),
            'dark': material(prefix+'Recess',(.022,.018,.014),.77,.10),
            'ember': material(prefix+'Ember',(.95,.145,.009),.38,.08,emission=.65),
            'cloth': material(prefix+'RedCloth',(.24,.036,.018),.79,cloth=True)}


def form(name, rings, mat, crease=.7, subdivision=1, bevel=.008):
    n = len(rings[0])
    verts = [v for ring in rings for v in ring]
    faces = [tuple(reversed(range(n)))]
    for j in range(len(rings)-1):
        for i in range(n):
            faces.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
    faces.append(tuple((len(rings)-1)*n+i for i in range(n)))
    o = mesh_object(name,verts,faces,mat,smooth=True)
    bm=bmesh.new()
    bm.from_mesh(o.data)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(o.data)
    bm.free()
    o.data.update()
    attr = o.data.attributes.new('crease_edge','FLOAT','EDGE')
    for d,e in zip(attr.data,o.data.edges):
        j,k = [v//n for v in e.vertices]
        d.value = min(1,crease+.15) if j==k else crease
    if subdivision:
        sub = o.modifiers.new('Controlled_surface','SUBSURF')
        sub.levels = sub.render_levels = subdivision
    if bevel:
        be = o.modifiers.new('Unequal_edge_radius','BEVEL')
        be.width, be.segments = bevel, 1
        be.limit_method = 'WEIGHT'
        weights = o.data.attributes.new('bevel_weight_edge','FLOAT','EDGE')
        for d,e in zip(weights.data,o.data.edges):
            d.value = .65 if set(e.vertices) in [{0,1},{(len(rings)-1)*n+4,(len(rings)-1)*n+5}] else 0
    o.data.set_sharp_from_angle(angle=rad(48))
    return o


OCT = [(-.72,-1),(.62,-1),(1,-.54),(1,.56),(.62,1),(-.72,1),(-1,.48),(-1,-.58)]


def mineral(name, center, size, mat, irregular=0, crease=.72):
    cx,cy,cz = center
    rx,ry,rz = [s/2 for s in size]
    rings=[]
    for j,(f,t) in enumerate([(.73,-1),(1,-.66),(1,.60),(.70,1)]):
        ring=[]
        for i,(x,y) in enumerate(OCT):
            off = irregular*math.sin(i*2.31+j*1.17)
            ring.append((cx+rx*(x*f+off*.15),cy+ry*(y*f+off*.13),cz+rz*(t+off*.11)))
        rings.append(ring)
    return form(name,rings,mat,crease,1,.007+rx*.012)


def segment(name, a, b, widths, mat, crease=.72):
    delta=Vector(b)-Vector(a)
    mid=(Vector(a)+Vector(b))*.5
    o=mineral(name,(0,0,0),(widths[0],widths[1],delta.length),mat,.16,crease)
    rot=delta.to_track_quat('Z','Y').to_matrix().to_4x4()
    o.data.transform(Matrix.Translation(mid) @ rot)
    return o


def attach(o, par, pivot=None, footprint=False, weapon=False):
    if pivot is not None:
        origin(o,pivot)
    parent(o,par)
    o['footprint']=footprint
    o['weapon']=weapon
    return o


def plate(name, outline, depth, mat, y=0, crease=.85):
    rings=[[(x,y-depth*.5,z) for x,z in outline],[(x,y+depth*.5,z) for x,z in outline]]
    o=form(name,rings,mat,crease,1,.006)
    o['thickness_m']=depth
    return o


def cloth(name, x0,x1,top,bottom,y,mat):
    rows=[]
    for j in range(4):
        v=j/3
        row=[]
        for i in range(5):
            u=i/4
            x=x0+(x1-x0)*(u-.08*v*(u-.5))
            z=top+(bottom-top)*v+(v**5)*(.06 if i%2 else -.04)
            row.append((x,y-.05*math.sin(u*math.pi*3)*v-.10*v,z))
        rows.append(row)
    vs=[v for row in rows for v in row]
    fs=[(j*5+i,j*5+i+1,(j+1)*5+i+1,(j+1)*5+i) for j in range(3) for i in range(4)]
    o=mesh_object(name,vs,fs,mat,smooth=True)
    sub=o.modifiers.new('Cloth_curvature','SUBSURF')
    sub.levels=sub.render_levels=1
    solid=o.modifiers.new('Cloth_thickness','SOLIDIFY')
    solid.thickness=.016
    solid.offset=0
    return o


def remember():
    for o in bpy.context.scene.objects:
        o['rest_location']=list(o.location)
        o['rest_rotation']=list(o.rotation_euler)
    bpy.context.view_layer.update()


def pose(prefix,state):
    for o in bpy.context.scene.objects:
        if 'rest_location' in o:
            o.location=o['rest_location']
            o.rotation_euler=o['rest_rotation']
    if state=='display':
        b1=prefix=='B1_'
        pelvis=bpy.data.objects[prefix+'Pelvis']
        pelvis.location.z-=.065 if b1 else .08
        pelvis.location.y+=.045
        bpy.data.objects[prefix+'Torso'].rotation_euler.x+=rad(12 if b1 else 8)
        bpy.data.objects[prefix+'Head'].rotation_euler.x+=rad(6)
        for s,sign in [('L',1),('R',-1)]:
            knee=17 if b1 else (20 if s=='L' else 27)
            thigh=bpy.data.objects[prefix+'Thigh_'+s]
            thigh.rotation_euler.x-=rad(knee)
            shin=bpy.data.objects[prefix+'Shin_'+s]
            shin.rotation_euler.x+=rad(knee*2)
            foot=bpy.data.objects[prefix+'Foot_'+s]
            foot.rotation_euler.x-=rad(knee)
            foot.rotation_euler.z+=rad(sign*5)
            arm=bpy.data.objects[prefix+'UpperArm_'+s]
            arm.rotation_euler.y+=rad(sign*(12 if b1 else -4))
            arm.rotation_euler.x-=rad(17 if b1 else 9)
            fore=bpy.data.objects[prefix+'Forearm_'+s]
            fore.rotation_euler.x-=rad(25 if b1 else (16 if s=='L' else 23))
        bpy.context.view_layer.update()
        if not b1:
            socket=bpy.data.objects['Hand_R_socket']
            desired=Euler((rad(-12),rad(-18),rad(-9)),'XYZ').to_quaternion()
            socket.rotation_euler=(socket.parent.matrix_world.to_quaternion().inverted() @ desired).to_euler()
        bpy.context.view_layer.update()
        deps=bpy.context.evaluated_depsgraph_get()
        for s in ['L','R']:
            foot=bpy.data.objects[prefix+'Foot_'+s]
            ev=foot.evaluated_get(deps)
            z=min((ev.matrix_world @ v.co).z for v in ev.data.vertices)
            correction=foot.parent.matrix_world.inverted().to_3x3() @ Vector((0,0,-z))
            foot.location+=correction
        bpy.context.view_layer.update()
    bpy.context.scene['pose_state']=state


def normalize_height(prefix,target):
    bpy.context.view_layer.update()
    deps=bpy.context.evaluated_depsgraph_get()
    pts=[o.evaluated_get(deps).matrix_world @ v.co for o in bpy.context.scene.objects
         if o.type=='MESH' and not o.get('weapon',False) for v in o.evaluated_get(deps).data.vertices]
    bottom=min(v.z for v in pts)
    factor=target/(max(v.z for v in pts)-bottom)
    world={o:o.matrix_world.copy() for o in bpy.context.scene.objects}
    transform=Matrix.Diagonal((factor,factor,factor,1)) @ Matrix.Translation((0,0,-bottom))
    updated={}
    for o,matrix in world.items():
        matrix=matrix.copy()
        if o.name==prefix+'Root':
            updated[o]=matrix
            continue
        matrix.translation=transform @ matrix.translation
        updated[o]=matrix
    for o in bpy.context.scene.objects:
        if o.type=='MESH':
            o.data.transform(updated[o].inverted() @ transform @ world[o])
    for o,matrix in updated.items():
        o.matrix_world=matrix
        bpy.context.view_layer.update()
    bpy.context.scene['normalization_factor']=factor


def finish(creep,prefix,revision,output=None):
    normalize_height(prefix,2.9 if prefix=='B1_' else 3.5)
    remember()
    scene=bpy.context.scene
    scene['creep_id']=creep
    scene['revision']=revision
    scene['prefix']=prefix
    out=output or ROOT/'art/creatures'/creep
    out=Path(out).resolve()
    allowed=(ROOT/'art/creatures'/creep).resolve()
    local=(ROOT/'.local/codex-tasks/creeps').resolve()
    if out!=allowed and local not in out.parents:
        raise ValueError('Output outside owned creep/local paths')
    out.mkdir(parents=True,exist_ok=True)
    pose(prefix,'rest')
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'blockout-rest.blend'))
    pose(prefix,'display')
    bpy.ops.wm.save_as_mainfile(filepath=str(out/'blockout-display.blend'))
    print('BUILT',creep,'revision',revision,'normalization',scene['normalization_factor'])

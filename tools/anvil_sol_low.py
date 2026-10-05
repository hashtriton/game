"""Local, deterministic anvil probe. Run with Blender background Python."""
import argparse
import math
import sys
from pathlib import Path

import bpy
import bmesh

if not bpy.app.background:
    raise RuntimeError('This builder requires a separate background Blender process')

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--stage', choices=['blockout', 'refined', 'final'], default='blockout')
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for collection in list(bpy.data.collections):
    bpy.data.collections.remove(collection)
asset = bpy.data.collections.new('ASSET')
bpy.context.scene.collection.children.link(asset)
bpy.context.scene.unit_settings.system = 'METRIC'

def material(name, color, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Metallic'].default_value = metallic
    bsdf.inputs['Roughness'].default_value = .48
    return mat

iron = material('Forged iron blockout', (.14, .16, .18), .65)
wood = material('Warm wood blockout', (.28, .13, .055))

def mesh_object(name, vertices, faces, mat):
    mesh = bpy.data.meshes.new(name + ' mesh')
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    asset.objects.link(obj)
    obj.data.materials.append(mat)
    return obj

def loft(name, rings, mat):
    n = len(rings[0])
    verts = [v for ring in rings for v in ring]
    faces = [tuple(reversed(range(n)))]
    for i in range(len(rings)-1):
        for j in range(n):
            a = i*n+j
            b = i*n+(j+1)%n
            faces.append((a,b,b+n,a+n))
    faces.append(tuple((len(rings)-1)*n+j for j in range(n)))
    return mesh_object(name, verts, faces, mat)

def rectangular_ring_z(z, xhalf, yhalf, centerx=0):
    return [(centerx-xhalf,-yhalf,z),(centerx+xhalf,-yhalf,z),
            (centerx+xhalf,yhalf,z),(centerx-xhalf,yhalf,z)]

# A continuous top/horn loft eliminates intersections at the horn shoulder.
def upper_ring(x, wy, hz, cz, rounded):
    points=[]
    for i in range(32):
        theta=2*math.pi*i/32
        cy,sz=math.cos(theta),math.sin(theta)
        divisor=max(abs(cy),abs(sz))
        y=(cy*rounded+cy/divisor*(1-rounded))*wy
        z=(sz*rounded+sz/divisor*(1-rounded))*hz+cz
        points.append((x,y,z))
    return points

upper=[]
for x,wy,hz,cz,roundness in [
    (-.60,.003,.003,.854,1),(-.597,.006,.006,.854,1),
    (-.58,.015,.013,.854,1),(-.54,.030,.025,.850,1),
    (-.48,.048,.039,.845,1),(-.41,.068,.050,.840,1),
    (-.34,.086,.057,.839,.8),(-.29,.101,.060,.840,.50),
    (-.25,.112,.060,.840,.25),(-.225,.122,.060,.840,.08),
    (-.20,.125,.060,.840,0),(.12,.125,.060,.840,0),
    (.40,.125,.060,.840,0),(.50,.125,.060,.840,0)]:
    upper.append(upper_ring(x,wy,hz,cz,roundness))
body=loft('Anvil continuous forged body',upper,iron)

def base_ring(z,xh,yh):
    c=.016
    return [(-xh+c+.04,-yh,z),(xh-c+.04,-yh,z),(xh+.04,-yh+c,z),
            (xh+.04,yh-c,z),(xh-c+.04,yh,z),(-xh+c+.04,yh,z),
            (-xh+.04,yh-c,z),(-xh+.04,-yh+c,z)]

# Cubic interpolation describes a smooth concave waist with deliberate corner rails.
controls=[(.50,.225,.165),(.535,.225,.165),(.56,.21,.145),
          (.62,.166,.101),(.675,.145,.080),(.715,.158,.086),
          (.755,.194,.108),(.79,.228,.122),(.805,.23,.123)]
neckrings=[]
for idx in range(len(controls)-1):
    p0=controls[max(idx-1,0)];p1=controls[idx]
    p2=controls[idx+1];p3=controls[min(idx+2,len(controls)-1)]
    for j in range(6):
        t=j/6
        z=p1[0]+t*(p2[0]-p1[0])
        vals=[]
        for axis in (1,2):
            q0,q1,q2,q3=[p[axis] for p in (p0,p1,p2,p3)]
            vals.append(.5*((2*q1)+(-q0+q2)*t+(2*q0-5*q1+4*q2-q3)*t*t+(-q0+3*q1-3*q2+q3)*t*t*t))
        neckrings.append(base_ring(z,*vals))
neckrings.append(base_ring(*controls[-1]))
neck=loft('Waist construction',neckrings,iron)
bpy.context.view_layer.objects.active=body
body.select_set(True)
mod=body.modifiers.new('Integrated waist','BOOLEAN')
mod.operation='UNION';mod.solver='EXACT';mod.object=neck
bpy.ops.object.modifier_apply(modifier=mod.name)
bpy.data.objects.remove(neck,do_unlink=True)
for poly in body.data.polygons:
    poly.use_smooth=not (abs(poly.normal.z)>.999 or abs(poly.normal.x)>.999)
body.data.set_sharp_from_angle(angle=math.radians(35))

# Slightly irregular radial sections retain the weight of a thick log.
rings=[]
for z,radius in [(0,.325),(.025,.331),(.16,.326),(.34,.32),(.485,.325),(.50,.32)]:
    ring=[]
    for i in range(32):
        theta=2*math.pi*i/32
        r=radius*(1+.012*math.sin(i*2.3)+.008*math.cos(i*.8))
        ring.append((r*math.cos(theta),r*math.sin(theta),z))
    rings.append(ring)
stump=loft('Solid wooden stump',rings,wood)

def activate(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj

def relocate(obj,name,mat):
    obj.name=name
    for col in list(obj.users_collection):
        col.objects.unlink(obj)
    asset.objects.link(obj)
    obj.data.materials.append(mat)
    activate(obj)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return obj

def cube(name,loc,size,mat):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    obj=bpy.context.object;obj.scale=size
    return relocate(obj,name,mat)

def boolean_cut(target,cut):
    activate(target)
    modifier=target.modifiers.new('Exact through cut','BOOLEAN')
    modifier.operation='DIFFERENCE';modifier.solver='EXACT';modifier.object=cut
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    bpy.data.objects.remove(cut,do_unlink=True)

def bevel(obj,width,segments=3):
    activate(obj)
    mod=obj.modifiers.new('Small manufactured edge radius','BEVEL')
    mod.width=width;mod.segments=segments;mod.limit_method='ANGLE'
    mod.angle_limit=math.radians(28)
    bpy.ops.object.modifier_apply(modifier=mod.name)

if args.stage=='final':
    square=cube('Square hardy cutter',(.337,-.040,.865),(.052,.052,.35),iron)
    boolean_cut(body,square)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=.015,depth=.35,location=(.432,.045,.865))
    hole=relocate(bpy.context.object,'Round pritchel cutter',iron)
    boolean_cut(body,hole)
    # Three localized nicks where hammer work plausibly catches the face edge.
    for x in (-.055,.065,.19):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=.0038,location=(x,-.125,.898))
        cut=relocate(bpy.context.object,'Edge wear cutter',iron)
        boolean_cut(body,cut)
    bevel(body,.0022)
    face=material('Worn bright working steel',(.34,.37,.39),.90)
    face.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.30
    body.data.materials.append(face)
    body.data.set_sharp_from_angle(angle=math.radians(35))
    # set_sharp_from_angle enables smooth shading: flat flags must be assigned after it.
    for polygon in body.data.polygons:
        coords=[body.data.vertices[i].co for i in polygon.vertices]
        working_face=all(abs(v.z-.90)<1e-6 and v.x>=-.20001 for v in coords)
        polygon.material_index=1 if working_face else 0
        polygon.use_smooth=not (abs(polygon.normal.z)>.999999 or abs(polygon.normal.x)>.999999)
    # Forged material variation stays at millimeter scale and low strength.
    nodes=iron.node_tree.nodes;links=iron.node_tree.links
    noise=nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=110
    noise.inputs['Detail'].default_value=2
    ramp=nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].color=(.055,.065,.073,1)
    ramp.color_ramp.elements[1].color=(.12,.14,.15,1)
    links.new(noise.outputs['Fac'],ramp.inputs[0])
    links.new(ramp.outputs['Color'],nodes.get('Principled BSDF').inputs['Base Color'])
    bump=nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.04
    bump.inputs['Distance'].default_value=.00015
    links.new(noise.outputs['Fac'],bump.inputs['Height'])
    links.new(bump.outputs['Normal'],nodes.get('Principled BSDF').inputs['Normal'])
    dark=material('Black iron fittings',(.055,.064,.070),.78)
    dark.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.40
    # Closed rectangular-section hoops follow the same stump radial profile.
    for index,z in enumerate((.11,.385)):
        vertices=[]
        for dz,radius in [(-.024,.326),(-.024,.336),(.024,.336),(.024,.326)]:
            for i in range(64):
                t=math.tau*i/64
                r=radius*(1+.006*math.sin(2.3*i/2)+.004*math.cos(.8*i/2))
                vertices.append((r*math.cos(t),r*math.sin(t),z+dz))
        faces=[]
        for j in range(4):
            for i in range(64):
                faces.append((j*64+i,j*64+(i+1)%64,((j+1)%4)*64+(i+1)%64,((j+1)%4)*64+i))
        hoop=mesh_object('Iron hoop '+str(index+1),vertices,faces,dark)
        bevel(hoop,.0013,2)
        for poly in hoop.data.polygons: poly.use_smooth=True
        hoop.data.set_sharp_from_angle(angle=math.radians(35))
    for index,(x,y) in enumerate([(-.145,-.166),(.18,-.166),(-.145,.166),(.18,.166)]):
        # A bearing shoe overlaps the foot, with an anchor descending into timber.
        shoe=cube('Hold-down shoe '+str(index+1),(x,y,.545),(.055,.078,.022),dark)
        bevel(shoe,.003,3)
        bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=.011,depth=.08,location=(x,y*1.18,.516))
        pin=relocate(bpy.context.object,'Anchor pin '+str(index+1),dark)
        bevel(pin,.0014,2)
        bpy.ops.mesh.primitive_cylinder_add(vertices=6,radius=.018,depth=.011,location=(x,y*1.18,.562))
        head=relocate(bpy.context.object,'Hex anchor head '+str(index+1),dark)
        bevel(head,.001,2)

    # Procedural log: axial grain on bark-free sides; radial growth rings on end grain.
    nodes=wood.node_tree.nodes;links=wood.node_tree.links
    nodes.clear()
    output=nodes.new('ShaderNodeOutputMaterial');bsdf=nodes.new('ShaderNodeBsdfPrincipled')
    links.new(bsdf.outputs[0],output.inputs[0]);bsdf.inputs['Roughness'].default_value=.72
    geometry=nodes.new('ShaderNodeNewGeometry')
    coord=nodes.new('ShaderNodeTexCoord')
    stretch=nodes.new('ShaderNodeVectorMath');stretch.operation='MULTIPLY'
    stretch.inputs[1].default_value=(34,34,1.2)
    links.new(coord.outputs['Generated'],stretch.inputs[0])
    grain=nodes.new('ShaderNodeTexNoise');grain.inputs['Scale'].default_value=2
    grain.inputs['Detail'].default_value=3;grain.inputs['Roughness'].default_value=.65
    links.new(stretch.outputs[0],grain.inputs['Vector'])
    sidecolor=nodes.new('ShaderNodeValToRGB')
    sidecolor.color_ramp.elements[0].position=.2
    sidecolor.color_ramp.elements[0].color=(.105,.038,.013,1)
    sidecolor.color_ramp.elements[1].position=.8
    sidecolor.color_ramp.elements[1].color=(.37,.18,.068,1)
    links.new(grain.outputs['Fac'],sidecolor.inputs[0])
    xy=nodes.new('ShaderNodeVectorMath');xy.operation='MULTIPLY'
    xy.inputs[1].default_value=(1,1,0);links.new(geometry.outputs['Position'],xy.inputs[0])
    radius=nodes.new('ShaderNodeVectorMath');radius.operation='LENGTH';links.new(xy.outputs[0],radius.inputs[0])
    frequency=nodes.new('ShaderNodeMath');frequency.operation='MULTIPLY';frequency.inputs[1].default_value=195
    links.new(radius.outputs['Value'],frequency.inputs[0])
    warp=nodes.new('ShaderNodeTexNoise');warp.inputs['Scale'].default_value=6
    warp.inputs['Detail'].default_value=2
    links.new(geometry.outputs['Position'],warp.inputs['Vector'])
    amplitude=nodes.new('ShaderNodeMath');amplitude.operation='MULTIPLY';amplitude.inputs[1].default_value=3.5
    links.new(warp.outputs['Fac'],amplitude.inputs[0])
    distorted=nodes.new('ShaderNodeMath');distorted.operation='ADD'
    links.new(frequency.outputs[0],distorted.inputs[0]);links.new(amplitude.outputs[0],distorted.inputs[1])
    sine=nodes.new('ShaderNodeMath');sine.operation='SINE';links.new(distorted.outputs[0],sine.inputs[0])
    endcolor=nodes.new('ShaderNodeValToRGB')
    endcolor.color_ramp.elements[0].color=(.24,.130,.049,1)
    endcolor.color_ramp.elements[1].color=(.30,.175,.078,1)
    links.new(sine.outputs[0],endcolor.inputs[0])
    normal=nodes.new('ShaderNodeSeparateXYZ');links.new(geometry.outputs['Normal'],normal.inputs[0])
    mask=nodes.new('ShaderNodeMath');mask.operation='GREATER_THAN';mask.inputs[1].default_value=.7
    links.new(normal.outputs['Z'],mask.inputs[0])
    mix=nodes.new('ShaderNodeMixRGB');links.new(mask.outputs[0],mix.inputs[0])
    links.new(sidecolor.outputs[0],mix.inputs[1]);links.new(endcolor.outputs[0],mix.inputs[2])
    links.new(mix.outputs[0],bsdf.inputs['Base Color'])
    bump=nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.20;bump.inputs['Distance'].default_value=.001
    links.new(grain.outputs['Fac'],bump.inputs['Height']);links.new(bump.outputs[0],bsdf.inputs['Normal'])
    # Radial splits are geometric incisions, concentrated on exposed outer end grain.
    for index,(theta,length) in enumerate([(0.8,.055),(2.7,.075),(4.1,.05)]):
        r=.295;x=r*math.cos(theta);y=r*math.sin(theta)
        cut=cube('End split cutter',(x,y,.493),(length,.0025,.022),wood)
        cut.rotation_euler.z=theta
        boolean_cut(stump,cut)
    bevel(stump,.0015,2)
    for poly in stump.data.polygons: poly.use_smooth=abs(poly.normal.z)<.8
    stump.data.set_sharp_from_angle(angle=math.radians(35))

for obj in asset.objects:
    assert tuple(obj.scale)==(1,1,1)
    assert all(math.isfinite(c) for v in obj.data.vertices for c in v.co)
    bm=bmesh.new(); bm.from_mesh(obj.data)
    assert all(edge.is_manifold for edge in bm.edges), obj.name
    bm.free()
assert len(asset.objects)<=(120 if args.stage=='final' else 12)
triangles=sum(sum(len(p.vertices)-2 for p in obj.data.polygons) for obj in asset.objects)
assert triangles<(100000 if args.stage=='final' else 20000)
assert max(math.hypot(v.co.x,v.co.y) for v in body.data.vertices if v.co.z<.536)<.312
out=ROOT/'art/tests/anvil-low/sol-low'/('final.blend' if args.stage=='final' else 'blockout-refined.blend')
out.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out))
print('BLOCKOUT_OK',out,'objects',len(asset.objects),'triangles',triangles)
for obj in asset.objects:
    print(obj.name,'dimensions',tuple(round(v,5) for v in obj.dimensions))

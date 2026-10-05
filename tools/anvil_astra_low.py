"""Independent local anvil study. Run only in Blender background mode."""
import argparse
import math
from pathlib import Path
import sys
import json

import bpy
import bmesh

if not bpy.app.background:
    raise RuntimeError('This builder requires Blender background mode')

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'art/tests/anvil-low/astra-low'


def material(name, color, metallic=0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value = (*color, 1)
    shader.inputs['Metallic'].default_value = metallic
    shader.inputs['Roughness'].default_value = .48
    return mat


def mesh_object(name, rings, mat):
    n = len(rings[0])
    vertices = [v for ring in rings for v in ring]
    faces = [tuple(reversed(range(n)))]
    for j in range(len(rings) - 1):
        for i in range(n):
            k = (i + 1) % n
            faces.append((j*n+i, j*n+k, (j+1)*n+k, (j+1)*n+i))
    faces.append(tuple(range((len(rings)-1)*n, len(rings)*n)))
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    bm = bmesh.new()
    bm.from_mesh(mesh)
    bmesh.ops.remove_doubles(bm, verts=list(bm.verts), dist=1e-7)
    bmesh.ops.dissolve_degenerate(bm, edges=list(bm.edges), dist=1e-8)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.data.collections['ASSET'].objects.link(obj)
    obj.data.materials.append(mat)
    return obj


def rectangle(xmin, xmax, halfwidth, z):
    c = min(.025, halfwidth*.24)
    return [(xmin+c,-halfwidth,z), (xmax-c,-halfwidth,z),
            (xmax,-halfwidth+c,z), (xmax,halfwidth-c,z),
            (xmax-c,halfwidth,z), (xmin+c,halfwidth,z),
            (xmin,halfwidth-c,z), (xmin,-halfwidth+c,z)]


def build_blockout():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    bpy.context.scene.collection.children.link(bpy.data.collections.new('ASSET'))
    metal = material('Forged iron', (.12,.14,.16), .75)
    wood = material('Warm oak', (.29,.14,.055))
    body = mesh_object('Anvil - continuous forged body', [rectangle(*r) for r in [
        (-.27,.28,.19,.50), (-.27,.28,.19,.535),
        (-.225,.235,.155,.568), (-.17,.175,.105,.625),
        (-.145,.155,.082,.685), (-.165,.19,.088,.735),
        (-.205,.29,.11,.785), (-.235,.53,.125,.82),
        (-.235,.53,.125,.90),
    ]], metal)
    # Eight-sided cross sections transition from rectangular shoulder to oval horn.
    horn_rings = []
    for x, width, bottom, top in [(-.20,.122,.816,.896),
                                 (-.27,.115,.82,.895),
                                 (-.35,.089,.833,.89),
                                 (-.44,.06,.847,.883),
                                 (-.515,.028,.86,.878),
                                 (-.57,.002,.869,.873)]:
        mid = (bottom+top)/2
        height = (top-bottom)/2
        horn_rings.append([(x,width*math.cos(a),mid+height*math.sin(a))
                           for a in [2*math.pi*i/8 for i in range(8)]])
    horn = mesh_object('Horn construction', horn_rings, metal)
    bpy.context.view_layer.objects.active = body
    union = body.modifiers.new('Continuous horn union', 'BOOLEAN')
    union.operation = 'UNION'
    union.solver = 'EXACT'
    union.object = horn
    bpy.ops.object.modifier_apply(modifier=union.name)
    bpy.data.objects.remove(horn, do_unlink=True)
    # Slightly irregular trunk, with absolutely level contact surfaces.
    stump_rings = []
    for z, scale in [(0,1.02),(.045,1.01),(.25,.985),(.465,.98),(.50,.98)]:
        ring = []
        for i in range(32):
            a = 2*math.pi*i/32
            r = .325*scale*(1+.025*math.sin(5*a)+.012*math.cos(9*a))
            ring.append((r*math.cos(a),r*math.sin(a),z))
        stump_rings.append(ring)
    mesh_object('Oak stump', stump_rings, wood)
    bpy.context.scene.unit_settings.system = 'METRIC'
    bpy.context.scene.unit_settings.scale_length = 1
    return body


def build_refined(final_quality=False):
    build_blockout()
    old = bpy.data.objects['Anvil - continuous forged body']
    metal = old.data.materials[0]
    bpy.data.objects.remove(old, do_unlink=True)
    # Rounded rectangular cross sections share vertex correspondence with the horn.
    rings = []
    for x, w, bottom, top, radius in [
        (-.57,.003,.856,.862,.003), (-.561,.009,.851,.869,.009),
        (-.54,.018,.842,.877,.0175), (-.50,.031,.829,.885,.028),
        (-.45,.047,.810,.893,.0415), (-.39,.066,.793,.898,.0525),
        (-.33,.085,.778,.900,.061), (-.27,.104,.769,.900,.060),
        (-.22,.116,.768,.900,.046), (-.17,.125,.775,.900,.025),
        (-.11,.125,.786,.900,.009), (.01,.125,.79,.900,.008),
        (.30,.125,.79,.900,.008), (.53,.125,.79,.900,.008),
    ]:
        ring=[]
        for cy, cz, start in [(w-radius,top-radius,0),
                               (-w+radius,top-radius,90),
                               (-w+radius,bottom+radius,180),
                               (w-radius,bottom+radius,270)]:
            for i in range(8):
                a=math.radians(start+i*90/7)
                ring.append((x,cy+radius*math.cos(a),cz+radius*math.sin(a)))
        if final_quality and x <= -.17:
            # Equal-angle oval sections remove the pinched, collapsing top strip.
            blend=max(0,min(1,(-x-.17)/.10))
            for i,(rx,ry,rz) in enumerate(ring):
                a=2*math.pi*i/32+math.pi/4
                ey=w*math.cos(a)
                ez=(top+bottom)/2+(top-bottom)/2*math.sin(a)
                ring[i]=(rx,ry*(1-blend)+ey*blend,rz*(1-blend)+ez*blend)
        rings.append(ring)
    head = mesh_object('Anvil - continuous head and round horn', rings, metal)
    # Hermite smooth interpolation through the waist, retaining a firm base rim.
    controls=[(.535,-.245,.25,.175),(.59,-.192,.20,.126),
              (.66,-.147,.158,.085),(.70,-.146,.161,.083),
              (.75,-.17,.20,.095),(.805,-.213,.295,.12)]
    bodyrings=[rectangle(-.245,.25,.175,.50),rectangle(-.245,.25,.175,.535)]
    for j in range(len(controls)-1):
        a,b=controls[j],controls[j+1]
        for k in range(1,9):
            t=k/8
            vals=[]
            for c in range(1,4):
                prev=controls[max(j-1,0)]
                nxt=controls[min(j+2,len(controls)-1)]
                m0=(b[c]-prev[c])/(b[0]-prev[0])*(b[0]-a[0])
                m1=(nxt[c]-a[c])/(nxt[0]-a[0])*(b[0]-a[0])
                vals.append((2*t**3-3*t*t+1)*a[c]+(t**3-2*t*t+t)*m0+
                            (-2*t**3+3*t*t)*b[c]+(t**3-t*t)*m1)
            bodyrings.append(rectangle(*vals,a[0]+t*(b[0]-a[0])))
    body=mesh_object('Waist construction',bodyrings,metal)
    bpy.context.view_layer.objects.active=head
    union=head.modifiers.new('Forged shoulder union','BOOLEAN')
    union.operation='UNION'
    union.solver='EXACT'
    union.object=body
    bpy.ops.object.modifier_apply(modifier=union.name)
    bpy.data.objects.remove(body,do_unlink=True)
    # Smooth only adjacent faces below 35 degrees; top and base stay flat.
    bm=bmesh.new()
    bm.from_mesh(head.data)
    for face in bm.faces:
        face.smooth=True
    for edge in bm.edges:
        edge.smooth=edge.is_manifold and edge.calc_face_angle()<math.radians(35)
    bm.to_mesh(head.data)
    bm.free()
    return head


def activate(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active=obj


def into_asset(obj, name, mat):
    obj.name=name
    for collection in list(obj.users_collection):
        collection.objects.unlink(obj)
    bpy.data.collections['ASSET'].objects.link(obj)
    obj.data.materials.append(mat)
    activate(obj)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return obj


def bevel(obj, width, segments=3):
    activate(obj)
    mod=obj.modifiers.new('Small forged edge radii','BEVEL')
    mod.width=width
    mod.segments=segments
    mod.limit_method='ANGLE'
    mod.angle_limit=math.radians(28)
    bpy.ops.object.modifier_apply(modifier=mod.name)


def cut(obj, cutter):
    activate(obj)
    mod=obj.modifiers.new('Exact through cut','BOOLEAN')
    mod.operation='DIFFERENCE'
    mod.solver='EXACT'
    mod.object=cutter
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(cutter,do_unlink=True)


def wood_material(name, endgrain=False):
    mat=material(name,(.30,.135,.047))
    nt=mat.node_tree
    shader=nt.nodes.get('Principled BSDF')
    shader.inputs['Roughness'].default_value=.66
    coord=nt.nodes.new('ShaderNodeTexCoord')
    scale=nt.nodes.new('ShaderNodeVectorMath')
    scale.operation='MULTIPLY'
    scale.inputs[1].default_value=(1,1,1) if endgrain else (5,5,.20)
    nt.links.new(coord.outputs['Object'],scale.inputs[0])
    noise=nt.nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value=5
    noise.inputs['Detail'].default_value=3
    nt.links.new(scale.outputs['Vector'],noise.inputs['Vector'])
    if endgrain:
        wave=nt.nodes.new('ShaderNodeTexWave')
        wave.wave_type='RINGS'
        wave.rings_direction='Z'
        wave.inputs['Scale'].default_value=12
        wave.inputs['Distortion'].default_value=6
        wave.inputs['Detail Scale'].default_value=2
        nt.links.new(scale.outputs['Vector'],wave.inputs['Vector'])
        output=wave.outputs['Color']
    else:
        output=noise.outputs['Fac']
    ramp=nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].position=.18
    ramp.color_ramp.elements[0].color=(.11,.045,.014,1)
    ramp.color_ramp.elements[1].position=.83
    ramp.color_ramp.elements[1].color=(.40,.215,.085,1)
    if endgrain:
        ramp.color_ramp.elements[0].color=(.24,.115,.042,1)
        ramp.color_ramp.elements[1].color=(.34,.185,.078,1)
    nt.links.new(output,ramp.inputs[0])
    nt.links.new(ramp.outputs['Color'],shader.inputs['Base Color'])
    bump=nt.nodes.new('ShaderNodeBump')
    bump.inputs['Strength'].default_value=.17
    bump.inputs['Distance'].default_value=.0008
    nt.links.new(output,bump.inputs['Height'])
    nt.links.new(bump.outputs['Normal'],shader.inputs['Normal'])
    return mat


def build_final():
    head=build_refined(final_quality=True)
    steel=head.data.materials[0]
    steel.diffuse_color=(.085,.105,.125,1)
    shader=steel.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value=steel.diffuse_color
    shader.inputs['Roughness'].default_value=.43
    face=material('Polished working face',(.29,.32,.35),.88)
    face.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.31
    dark=material('Oxidized iron fittings',(.055,.063,.067),.72)
    # Closed cutters extend well above and below the heel. Apply before bevel.
    bpy.ops.mesh.primitive_cube_add(size=1,location=(.375,-.03,.85))
    cutter=bpy.context.object
    cutter.dimensions=(.060,.060,.40)
    activate(cutter)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    cut(head,cutter)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=.014,depth=.4,
                                       location=(.455,.053,.85))
    cut(head,bpy.context.object)
    # Three small, localized work marks on the front edge, not surface noise.
    for x,y,z,r in [(.13,-.126,.899,.006),(.19,-.126,.899,.004),(-.015,-.126,.899,.0045)]:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=r,location=(x,y,z))
        cut(head,bpy.context.object)
    bevel(head,.0025,3)
    # Boolean material slots and tessellation are explicit final-mesh contracts.
    head.data.materials.clear()
    head.data.materials.append(steel)
    head.data.materials.append(face)
    bm=bmesh.new(); bm.from_mesh(head.data)
    bmesh.ops.triangulate(bm,faces=list(bm.faces))
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=2e-6)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=2e-6)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(head.data); bm.free()
    head.data.update()
    for poly in head.data.polygons:
        poly.material_index=0
        poly.use_smooth=True
        coords=[head.data.vertices[i].co for i in poly.vertices]
        planar_top=all(abs(v.z-.90)<1e-6 for v in coords)
        working_plate=planar_top and all(v.x>=-.110001 for v in coords)
        if working_plate:
            poly.material_index=1
        if planar_top:
            poly.use_smooth=False
        elif abs(poly.normal.z)>.999:
            poly.use_smooth=False
    stump=bpy.data.objects['Oak stump']
    stump.data.materials.clear()
    stump.data.materials.append(wood_material('Oak - long side grain'))
    stump.data.materials.append(wood_material('Oak - cut end rings',True))
    for poly in stump.data.polygons:
        poly.material_index=1 if abs(poly.normal.z)>.8 else 0
    bevel(stump,.005,3)
    # A few narrow drying checks on exposed timber, cut into the top surface.
    for angle,length in [(1.15,.060),(2.70,.042),(4.45,.048)]:
        x=.30*math.cos(angle); y=.30*math.sin(angle)
        bpy.ops.mesh.primitive_cube_add(size=1,location=(x,y,.501))
        cutter=bpy.context.object
        cutter.dimensions=(length,.0018,.008)
        cutter.rotation_euler[2]=angle
        activate(cutter)
        bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
        cut(stump,cutter)
    # Cutters can introduce an empty slot; keep wood assignments meaningful.
    while len(stump.data.materials)>2:
        stump.data.materials.pop(index=len(stump.data.materials)-1)
    for p in stump.data.polygons:
        if p.material_index>=2: p.material_index=1
    # Hoops follow the same restrained irregularity as the trunk.
    for index,zcenter in enumerate([.095,.395],1):
        rings=[]
        for z,extra in [(zcenter-.021,.003),(zcenter-.021,.013),
                        (zcenter+.021,.013),(zcenter+.021,.003)]:
            radius_scale=1.005 if zcenter<.2 else .982
            ring=[]
            for i in range(64):
                a=2*math.pi*i/64
                r=.325*radius_scale*(1+.025*math.sin(5*a)+.012*math.cos(9*a))+extra
                ring.append((r*math.cos(a),r*math.sin(a),z))
            rings.append(ring)
        # Close toroidal cross-section without filling the center with cap faces.
        n=64
        verts=[v for ring in rings for v in ring]
        faces=[(j*n+i,j*n+(i+1)%n,((j+1)%4)*n+(i+1)%n,((j+1)%4)*n+i)
               for j in range(4) for i in range(n)]
        mesh=bpy.data.meshes.new('Hoop mesh')
        mesh.from_pydata(verts,[],faces)
        bm=bmesh.new(); bm.from_mesh(mesh)
        bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
        bm.to_mesh(mesh); bm.free()
        obj=bpy.data.objects.new('Iron hoop '+str(index),mesh)
        bpy.data.collections['ASSET'].objects.link(obj)
        obj.data.materials.append(dark)
        bevel(obj,.0017,2)
        for p in obj.data.polygons: p.use_smooth=True
    # Four bent hold-down straps bear on the foot and on the timber.
    for x in [-.17,.17]:
        for sign in [-1,1]:
            profile=[(.145,.538),(.183,.538),(.205,.501),(.257,.501),
                     (.257,.514),(.214,.514),(.191,.551),(.145,.551)]
            rings=[[(xx,sign*y,z) for y,z in profile] for xx in [x-.018,x+.018]]
            strap=mesh_object('Bent hold-down '+str(x)+' '+str(sign),rings,dark)
            bevel(strap,.002,2)
            bpy.ops.mesh.primitive_cylinder_add(vertices=6,radius=.012,depth=.009,
                                               location=(x,sign*.237,.518))
            bolt=into_asset(bpy.context.object,'Hex timber fixing',dark)
            bevel(bolt,.001,2)
    return head


def validate_scene():
    report={'objects':[], 'total_triangles':0}
    for obj in bpy.data.collections['ASSET'].objects:
        mesh=obj.data
        mesh.calc_loop_triangles()
        bm=bmesh.new(); bm.from_mesh(mesh)
        row={'name':obj.name,'triangles':len(mesh.loop_triangles),
             'non_manifold':sum(not e.is_manifold for e in bm.edges),
             'zero_area_faces':sum(f.calc_area()<1e-12 for f in bm.faces),
             'signed_volume':bm.calc_volume(signed=True),
             'finite':all(math.isfinite(c) for v in mesh.vertices for c in v.co),
             'dimensions':list(obj.dimensions),'materials':len(mesh.materials)}
        bm.free()
        assert row['non_manifold']==0 and row['zero_area_faces']==0 and row['finite'],row
        assert row['signed_volume']>0 and row['materials']>0,row
        report['objects'].append(row)
        report['total_triangles']+=row['triangles']
    assert len(report['objects'])<=120 and report['total_triangles']<100000
    (OUT/'final-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print('FINAL_VALIDATION',len(report['objects']),report['total_triangles'])


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--stage', choices=['blockout', 'refined', 'final'], default='blockout')
    args = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    {'blockout':build_blockout,'refined':build_refined,'final':build_final}[args.stage]()
    OUT.mkdir(parents=True, exist_ok=True)
    filename = {'blockout':'blockout.blend','refined':'blockout-refined.blend','final':'final.blend'}[args.stage]
    if args.stage == 'final':
        validate_scene()
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT / filename))
    for obj in bpy.data.collections['ASSET'].objects:
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        print('BLOCKOUT_CHECK', obj.name, 'dimensions', tuple(round(x,4) for x in obj.dimensions),
              'non_manifold', sum(not e.is_manifold for e in bm.edges),
              'vertices', len(bm.verts))
        bm.free()


if __name__ == '__main__':
    main()

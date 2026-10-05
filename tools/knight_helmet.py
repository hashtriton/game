"""Original knight helmet. Import is inert; CLI creates a local clay study only."""
import math
from pathlib import Path
import bpy
from mathutils import Vector


def _mesh(name, verts, faces, collection, material, thickness=0.004, bevel=0.002):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.data.materials.append(material)
    # Ring/strip generators use consistent outward winding.
    for polygon in mesh.polygons:
        polygon.use_smooth = True
    if thickness:
        mod = obj.modifiers.new('Forged shell thickness', 'SOLIDIFY')
        mod.thickness = thickness
        mod.offset = -1
    if bevel:
        mod = obj.modifiers.new('Soft forged edges', 'BEVEL')
        mod.width = bevel
        mod.segments = 3
    return obj


def _rings(name, rings, angles, collection, material, closed=False, thickness=.004, bevel=.002):
    verts = []
    for rx, ry, cy, z, front_drop, keel in rings:
        for a in angles:
            front = max(0, math.cos(a))
            x = rx * math.sin(a)
            y = cy - ry * math.cos(a) - keel * front ** 12
            v_profile=max(0,1-abs(math.sin(a))) if math.cos(a)>0 else 0
            verts.append((x, y, z - front_drop * v_profile))
    n = len(angles)
    faces = []
    for j in range(len(rings)-1):
        for k in range(n if closed else n-1):
            kk = (k+1) % n
            faces.append((j*n+k, j*n+kk, (j+1)*n+kk, (j+1)*n+k))
    return _mesh(name, verts, faces, collection, material, thickness, bevel)


def build_helmet(collection, materials, detailed=False):
    """Build at origin, facing -Y; return all newly created objects.

    ``collection`` is an existing bpy Collection. ``materials`` supplies
    steel/bronze/dark/edge. No deletion, save, rendering or context switch.
    Detailed mode adds restrained fitted edging and inset ventilation.
    """
    objects = []
    full = [2*math.pi*i/64 for i in range(64)]
    # Brow is carried by the dome itself. Crown sections flatten near the
    # temples and converge to a small cap, rather than a spherical primitive.
    dome = [(.128,.139,.000,.048,.052,.008),
            (.135,.138,.003,.092,.014,.008),
            (.131,.132,.006,.131,.005,.006),
            (.108,.113,.010,.163,0,.003),
            (.074,.078,.015,.183,0,0),
            (.035,.038,.018,.190,0,0),
            (.001,.001,.018,.192,0,0)]
    # Sample a cubic section curve. Smooth silhouette is real geometry, not a
    # subdivision modifier that could pull the crown away from its fitted rib.
    controls = dome
    dome=[]
    for i in range(len(controls)-1):
        p0=controls[max(0,i-1)]; p1=controls[i]
        p2=controls[i+1]; p3=controls[min(len(controls)-1,i+2)]
        for step in range(4):
            t=step/4
            ring=[.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t)
                  for a,b,c,d in zip(p0,p1,p2,p3)]
            ring[0]=max(.001,ring[0]); ring[1]=max(.001,ring[1])
            dome.append(tuple(ring))
    dome.append(controls[-1])
    objects.append(_rings('Helmet sculpted crown', dome, full, collection, materials['steel'], True))
    # Rear and temples join below the eye slit; the front section is omitted.
    rear_angles = [1.24 + (2*math.pi-2*1.24)*i/48 for i in range(49)]
    rear = [(.079,.090,.019,-.140,0,0),
            (.101,.111,.014,-.112,0,0),
            (.121,.124,.014,-.082,0,0),
            (.131,.137,.009,-.025,0,0),
            (.128,.139,.000,.050,0,0)]
    objects.append(_rings('Helmet rear and temple shell', rear, rear_angles, collection, materials['steel']))
    # One continuous face plate with a central keel and shaped cheek volumes.
    # The narrowing chin sweeps backwards, leaving the silhouette anatomical.
    face_angles = [-1.39 + 2*1.39*i/48 for i in range(49)]
    # Explicit visor sections: nose -> folded cheek -> temple. These are
    # substantial forged planes, not an elliptical cylindrical face plate.
    rows=[(-.154,[0,.018,.046,.068,.076],[-.098,-.096,-.086,-.060,-.025],.006),
          (-.129,[0,.023,.060,.090,.102],[-.128,-.119,-.097,-.064,-.018],.004),
          (-.081,[0,.026,.076,.113,.124],[-.164,-.144,-.116,-.071,-.020],.006),
          (-.041,[0,.027,.079,.119,.130],[-.176,-.149,-.117,-.069,-.016],.007),
          (-.009,[0,.021,.073,.113,.128],[-.157,-.144,-.116,-.069,-.014],.045)]
    verts=[]
    for z,xs,ys,rise in rows:
        for side,k in [(-1,4),(-1,3),(-1,2),(-1,1),(1,0),(1,1),(1,2),(1,3),(1,4)]:
            x=side*xs[k]
            verts.append((x,ys[k],z+rise*abs(x)/xs[-1]))
    faces=[(j*9+k,j*9+k+1,(j+1)*9+k+1,(j+1)*9+k) for j in range(len(rows)-1) for k in range(8)]
    visor=_mesh('Helmet folded cheek and jaw visor',verts,faces,collection,materials['steel'],.005,.0015)
    # Smooth within forged cheek volumes, preserve the long central nose fold.
    sharp=visor.data.attributes.new('sharp_edge','BOOLEAN','EDGE')
    for edge in visor.data.edges:
        a,b=(visor.data.vertices[i].co for i in edge.vertices)
        sharp.data[edge.index].value=abs(a.x)<1e-7 and abs(b.x)<1e-7
    objects.append(visor)
    # Real opening: dark recess behind the empty 15-23 mm eye slit.
    eye = [(.120,.133,.009,-.021,.026,0),(.120,.133,.009,.058,.055,0)]
    objects.append(_rings('Helmet shadow inside eye opening', eye, face_angles, collection, materials['dark'], thickness=.002,bevel=0))
    # Narrow brow strip hugs the crown, accentuating its overhang.
    brow = [(.130,.141,.000,.045,.052,.008),(.131,.142,.000,.058,.052,.008)]
    objects.append(_rings('Helmet heavy V brow', brow, face_angles, collection, materials['bronze'] if detailed else materials['steel'], thickness=.004))
    # A low forged blade ridge, continuous from brow over the crown.
    profile=[(cy-ry-keel,z-drop) for rx,ry,cy,z,drop,keel in dome]
    profile += [(cy+ry,z) for rx,ry,cy,z,drop,keel in reversed(dome[8:-1])]
    verts = []
    for i,(y,z) in enumerate(profile):
        before=profile[max(0,i-1)]; after=profile[min(len(profile)-1,i+1)]
        dy=after[0]-before[0]; dz=after[1]-before[1]
        length=math.hypot(dy,dz)
        ny,nz=-dz/length,dy/length
        w = .007
        verts.extend([(-w,y-ny*.002,z-nz*.002),(0,y+ny*.006,z+nz*.006),(w,y-ny*.002,z-nz*.002)])
    faces=[]
    for i in range(len(profile)-1):
        for k in range(2):
            faces.append((i*3+k,(i+1)*3+k,(i+1)*3+k+1,i*3+k+1))
    objects.append(_mesh('Helmet low central crown rib', verts, faces, collection, materials['steel'], .003, 0))
    if detailed:
        # A fitted thin bronze inner brow follows the same actual V sections.
        lip=[(.131,.143,-.001,.045,.052,.008),(.131,.143,-.001,.049,.052,.008)]
        objects.append(_rings('Helmet restrained brow piping',lip,face_angles,collection,materials['edge'],thickness=.0015,bevel=0))
        # Preserve the forged surface's shading across triangulated Boolean
        # results. Newly cut interior walls keep their own planar normals.
        from mathutils.bvhtree import BVHTree
        from mathutils.geometry import barycentric_transform
        deps=bpy.context.evaluated_depsgraph_get()
        source_eval=visor.evaluated_get(deps); source_mesh=source_eval.to_mesh()
        source_mesh.calc_loop_triangles()
        source_positions=[v.co.copy() for v in source_mesh.vertices]
        source_triangles=[tuple(t.vertices) for t in source_mesh.loop_triangles]
        source_normals=[tuple(source_mesh.corner_normals[i].vector.copy() for i in t.loops) for t in source_mesh.loop_triangles]
        tree=BVHTree.FromPolygons(source_positions,source_triangles,all_triangles=True)
        source_eval.to_mesh_clear()
        # Three slim inset breathing slots on each cheek; cuts are physical.
        # Boolean cutter is local temporary geometry and never touches other objects.
        for side in (-1,1):
            for idx in range(3):
                z=-.079-idx*.016
                x=side*(.080-idx*.003)
                y=-.117+idx*.004
                cutter_verts=[(x+dx,y+dy,z+dz) for dx,dy,dz in [(-.016,-.018,-.002),(.016,-.018,-.002),(.016,.018,-.002),(-.016,.018,-.002),(-.016,-.018,.002),(.016,-.018,.002),(.016,.018,.002),(-.016,.018,.002)]]
                cutter_faces=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
                cutter=_mesh('Local vent cutter',cutter_verts,cutter_faces,collection,materials['dark'],0,0)
                mod=visor.modifiers.new('Cheek breathing opening','BOOLEAN'); mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
                # Bake through evaluated meshes without selection or operator context.
                deps=bpy.context.evaluated_depsgraph_get(); evaluated=visor.evaluated_get(deps)
                new_mesh=bpy.data.meshes.new_from_object(evaluated,depsgraph=deps)
                old_mesh=visor.data; visor.modifiers.clear(); visor.data=new_mesh
                bpy.data.meshes.remove(old_mesh)
                cutter_mesh=cutter.data
                bpy.data.objects.remove(cutter,do_unlink=True)
                bpy.data.meshes.remove(cutter_mesh)
                inside=[(x-.018,-.059,z-.003),(x+.018,-.059,z-.003),(x+.018,-.059,z+.003),(x-.018,-.059,z+.003)]
                objects.append(_mesh('Helmet inset cheek vent',inside,[(0,1,2,3)],collection,materials['dark'],.001,0))
        normals=[None]*len(visor.data.loops)
        for polygon in visor.data.polygons:
            for loop_index in polygon.loop_indices:
                point=visor.data.vertices[visor.data.loops[loop_index].vertex_index].co
                position,normal,triangle_index,distance=tree.find_nearest(point)
                indices=source_triangles[triangle_index]
                smooth=barycentric_transform(position,*[source_positions[i] for i in indices],*source_normals[triangle_index]).normalized()
                normals[loop_index]=smooth if distance<.00005 and polygon.normal.dot(normal)>.6 else polygon.normal
        visor.data.normals_split_custom_set(normals)
        # The central keel remains steel; restrained edging fits lower chin.
        chin=rows[0]
        cv=[]
        for offset in (0,.003):
            z,xs,ys,rise=chin
            for side,k in [(-1,4),(-1,3),(-1,2),(-1,1),(1,0),(1,1),(1,2),(1,3),(1,4)]:
                x=side*xs[k];cv.append((x,ys[k]-.001,z+rise*abs(x)/xs[-1]+offset))
        objects.append(_mesh('Helmet fitted chin edging',cv,[(k,k+1,k+10,k+9) for k in range(8)],collection,materials['bronze'],.001,0))
    return objects


def _material(name, color):
    mat=bpy.data.materials.new(name)
    mat.diffuse_color=(*color,1)
    mat.use_nodes=True
    shader=mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value=(*color,1)
    shader.inputs['Roughness'].default_value=.7
    return mat


def _study():
    if not bpy.app.background:
        raise RuntimeError('Study requires background Blender; existing GUI files are protected.')
    out=Path(__file__).resolve().parents[1]/'art/heroes/knight-v3/helmet-study'
    out.mkdir(parents=True,exist_ok=True)
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    coll=bpy.data.collections.new('HELMET')
    bpy.context.scene.collection.children.link(coll)
    clay=_material('Neutral clay',(.37,.39,.41))
    dark=_material('Opening darkness',(.018,.019,.021))
    mats={'steel':clay,'bronze':clay,'dark':dark,'edge':clay}
    import sys
    detailed='--detailed' in sys.argv
    if detailed and '--clay' not in sys.argv:
        mats={'steel':_material('Blackened steel',(.075,.095,.115)),'bronze':_material('Muted bronze',(.26,.15,.055)),'dark':dark,'edge':_material('Polished bronze edge',(.38,.25,.09))}
        for key in ('steel','bronze','edge'):
            mats[key].node_tree.nodes.get('Principled BSDF').inputs['Metallic'].default_value=.75
    objects=build_helmet(coll,mats,detailed)
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=40
    try:
        from hero_veteran_render import configure_cycles_device
        configure_cycles_device(scene)
    except ImportError:
        pass
    scene.render.resolution_x=900
    scene.render.resolution_y=1000
    scene.render.resolution_percentage=100
    scene.world.color=(.22,.22,.22)
    scene.view_settings.view_transform='AgX'
    for name,loc,power,size in [('Key',(-.6,-.7,.9),65,.55),('Fill',(.6,-.3,.3),18,.5),('Rim',(.2,.55,.7),80,.45)]:
        data=bpy.data.lights.new(name,'AREA'); data.energy=power; data.shape='DISK'; data.size=size
        light=bpy.data.objects.new(name,data); scene.collection.objects.link(light); light.location=loc
        light.rotation_euler=(Vector((0,0,.03))-light.location).to_track_quat('-Z','Y').to_euler()
    data=bpy.data.cameras.new('Study camera'); camera=bpy.data.objects.new('Study camera',data)
    scene.collection.objects.link(camera); scene.camera=camera; data.type='ORTHO'; data.ortho_scale=.46
    views={'front':(0,-1,.09),'profile':(.95,0,.09),'three-quarter':(.67,-.85,.22),'back':(0,1,.1)}
    for name,loc in views.items():
        camera.location=loc
        camera.rotation_euler=(Vector((0,0,.025))-camera.location).to_track_quat('-Z','Y').to_euler()
        prefix='detailed-' if detailed else ''
        scene.render.filepath=str(out/(prefix+name+'.png'))
        bpy.ops.render.render(write_still=True)
    camera.location=views['three-quarter']
    camera.rotation_euler=(Vector((0,0,.025))-camera.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.wm.save_as_mainfile(filepath=str(out/('detailed.blend' if detailed else 'blockout.blend')))
    deps=bpy.context.evaluated_depsgraph_get()
    triangles=0
    for obj in objects:
        evaluated=obj.evaluated_get(deps); mesh=evaluated.to_mesh(); mesh.calc_loop_triangles()
        triangles+=len(mesh.loop_triangles); evaluated.to_mesh_clear()
    print('HELMET_STUDY objects='+str(len(objects))+' evaluated_triangles='+str(triangles))


if __name__=='__main__':
    _study()

"""Small mesh-building vocabulary for the original knight study, in meters."""
import math
import bpy
import bmesh
import numpy as np
from mathutils import Vector, Quaternion


def mesh(name, vertices, faces, mat, col, smooth=True, bevel=0):
    data = bpy.data.meshes.new(name)
    data.from_pydata(vertices, [], faces)
    data.update()
    bm = bmesh.new()
    bm.from_mesh(data)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(data)
    bm.free()
    ob = bpy.data.objects.new(name, data)
    col.objects.link(ob)
    if mat:
        data.materials.append(mat)
    if smooth:
        for p in data.polygons:
            p.use_smooth = True
        data.set_sharp_from_angle(angle=math.radians(42))
    if bevel:
        mod = ob.modifiers.new('Forged edge radius', 'BEVEL')
        mod.width = bevel
        mod.segments = 3
        mod.limit_method = 'ANGLE'
        mod.angle_limit = math.radians(35)
        mod.harden_normals = True
    return ob


def loft(name, rings, mat, col, smooth=True, bevel=0):
    n = len(rings[0])
    vertices = [p for ring in rings for p in ring]
    faces = [tuple(reversed(range(n)))]
    for j in range(len(rings)-1):
        for i in range(n):
            ni = (i+1) % n
            faces.append((j*n+i, j*n+ni, (j+1)*n+ni, (j+1)*n+i))
    faces.append(tuple((len(rings)-1)*n+i for i in range(n)))
    return mesh(name, vertices, faces, mat, col, smooth, bevel)


def zloft(name, sections, mat, col, sides=40, bevel=0):
    # z, x-radius, y-radius, x-center, y-center
    rings = [[(cx+rx*math.cos(a), cy+ry*math.sin(a), z)
              for a in [2*math.pi*i/sides for i in range(sides)]]
             for z, rx, ry, cx, cy in sections]
    return loft(name, rings, mat, col, True, bevel)


def limb(name, start, end, profiles, mat, col, sides=32, bevel=0):
    start, end = Vector(start), Vector(end)
    axis = end-start
    q = Vector((0, 0, 1)).rotation_difference(axis.normalized())
    rings = []
    for t, rx, ry in profiles:
        rings.append([tuple(start+axis*t+q@Vector((rx*math.cos(a), ry*math.sin(a), 0)))
                      for a in [2*math.pi*i/sides for i in range(sides)]])
    return loft(name, rings, mat, col, True, bevel)


def tube(name, points, radius, mat, col, closed=False, sides=8):
    points = [Vector(p) for p in points]
    tangents=[]
    for i,p in enumerate(points):
        prev = points[(i-1)%len(points)] if closed or i else p
        nxt = points[(i+1)%len(points)] if closed or i<len(points)-1 else p
        tangents.append((nxt-prev).normalized())
    guide=min([Vector((1,0,0)),Vector((0,1,0)),Vector((0,0,1))],key=lambda a:abs(a.dot(tangents[0])))
    frames=[tangents[0].cross(guide).normalized()]
    for i in range(1,len(points)):
        u=tangents[i-1].rotation_difference(tangents[i])@frames[-1]
        frames.append((u-tangents[i]*u.dot(tangents[i])).normalized())
    if closed:
        transported=tangents[-1].rotation_difference(tangents[0])@frames[-1]
        angle=math.atan2(tangents[0].dot(transported.cross(frames[0])),transported.dot(frames[0]))
        frames=[Quaternion(tangents[i],angle*i/len(points))@u for i,u in enumerate(frames)]
    rings = []
    for p,tangent,u in zip(points,tangents,frames):
        v = tangent.cross(u).normalized()
        rings.append([tuple(p+radius*(math.cos(a)*u+math.sin(a)*v))
                      for a in [2*math.pi*k/sides for k in range(sides)]])
    if not closed:
        return loft(name, rings, mat, col)
    vertices = [tuple(p) for r in rings for p in r]
    faces = [(j*sides+i, j*sides+(i+1)%sides,
              ((j+1)%len(rings))*sides+(i+1)%sides, ((j+1)%len(rings))*sides+i)
             for j in range(len(rings)) for i in range(sides)]
    return mesh(name, vertices, faces, mat, col)


def solid_patch(name, vertices, faces, mat, col, thickness=.012, bevel=.002):
    ob = mesh(name, vertices, faces, mat, col)
    mod=ob.modifiers.new('Plate thickness', 'SOLIDIFY')
    mod.thickness=thickness
    mod.offset=0
    if bevel:
        mod=ob.modifiers.new('Plate edge radius','BEVEL')
        mod.width=bevel
        mod.segments=3
        mod.limit_method='ANGLE'
        mod.angle_limit=math.radians(40)
    return ob


def conform_path(ob, points, offset=.002):
    bpy.context.view_layer.update()
    evaluated=ob.evaluated_get(bpy.context.evaluated_depsgraph_get())
    result=[]
    for x,y,z in points:
        hit,loc,_,_=evaluated.ray_cast(Vector((x,-3,z)),Vector((0,1,0)))
        result.append((x,loc.y-offset if hit else y,z))
    return result


def plate(name, outline, mat, col, bulge=.012, thickness=.015, bevel=.002,support=None):
    # Dense radial patch preserves the designed perimeter exactly. The surface
    # is projected after tessellation so it cannot shrink into the backing mesh.
    original=list(outline)
    outline=[]
    for i,p in enumerate(original):
        q=original[(i+1)%len(original)]
        for j in range(4):
            t=j/4
            outline.append(tuple(p[k]*(1-t)+q[k]*t for k in range(3)))
    c=sum((Vector(p) for p in outline), Vector())/len(outline)
    vertices=[tuple(c-Vector((0,bulge,0)))]
    n=len(outline)
    for r in [.25,.50,.75,1.0]:
        for p in outline:
            pos=c+(Vector(p)-c)*r
            pos.y-=bulge*(1-r*r)
            vertices.append(tuple(pos))
    faces=[(0,i+1,(i+1)%n+1) for i in range(n)]
    for j in range(3):
        a=1+j*n
        b=a+n
        faces.extend((a+i,a+(i+1)%n,b+(i+1)%n,b+i) for i in range(n))
    if support:
        # Fit a smooth frontal height field. A raw projection onto end caps or
        # alternating hit/miss samples creates folds at the edge of a limb.
        bpy.context.view_layer.update()
        backing=support.evaluated_get(bpy.context.evaluated_depsgraph_get())
        cx=(min(p[0] for p in vertices)+max(p[0] for p in vertices))*.5
        cz=(min(p[2] for p in vertices)+max(p[2] for p in vertices))*.5
        sx=max(.01,max(p[0] for p in vertices)-min(p[0] for p in vertices))
        sz=max(.01,max(p[2] for p in vertices)-min(p[2] for p in vertices))
        def basis(p):
            x=(p[0]-cx)/sx
            z=(p[2]-cz)/sz
            return [1,x,z,x*x,x*z,z*z]
        samples=[]
        heights=[]
        for p in vertices:
            hit,loc,normal,_=backing.ray_cast(Vector((p[0],-3,p[2])),Vector((0,1,0)))
            if hit and normal.y<-.25:
                samples.append(basis(p))
                heights.append(loc.y)
        if len(samples)<6:
            raise RuntimeError('Not enough frontal surface samples for '+name)
        matrix=np.array(samples)
        coefficients=np.linalg.lstsq(matrix,np.array(heights),rcond=None)[0]
        clearance=max(0,float(np.max(matrix@coefficients-np.array(heights))))+thickness*.7
        vertices=[(p[0],float(np.dot(basis(p),coefficients))-clearance,p[2]) for p in vertices]
        outline=vertices[-n:]
    ob=mesh(name,vertices,faces,mat,col)
    for edge in ob.data.edges:
        edge.use_edge_sharp=False
    ob['plate_perimeter']=[v for point in outline for v in point]
    sol=ob.modifiers.new('Forged plate thickness','SOLIDIFY')
    sol.thickness=thickness
    sol.offset=0
    if bevel:
        edge=ob.modifiers.new('Rounded plate rim','BEVEL')
        edge.width=bevel
        edge.segments=2
        edge.limit_method='ANGLE'
        edge.angle_limit=math.radians(45)
    return ob


def plate_perimeter(ob):
    values=ob['plate_perimeter']
    return [tuple(values[i:i+3]) for i in range(0,len(values),3)]


def oval_path(center, radii, steps=48):
    x,y,z=center
    return [(x+radii[0]*math.cos(2*math.pi*i/steps),
             y+radii[1]*math.sin(2*math.pi*i/steps),z) for i in range(steps)]


def rivet(name, position, mat, col, radius=.008):
    # Rivet axis faces -Y; a short domed loft creates one watertight object.
    return limb(name, position, (position[0],position[1]-.006,position[2]),
                [(0,radius,radius),(.4,radius,radius),(1,radius*.65,radius*.65)],
                mat,col,12)

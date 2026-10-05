"""Create an original, editable armored veteran entirely inside local Blender."""
import bpy
import math
import sys
import json
import random
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'art/heroes/veteran'
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
BLEND_NAME=args[0] if args else 'veteran.blend'
if Path(BLEND_NAME).name!=BLEND_NAME or not BLEND_NAME.endswith('.blend'):
    raise ValueError('Output must be a .blend basename inside the hero folder.')
random.seed(41)
if not bpy.app.background:
    raise RuntimeError('Run in a separate background Blender; do not replace an interactive scene.')
bpy.ops.wm.read_factory_settings(use_empty=True)
OUT.mkdir(parents=True, exist_ok=True)
HERO = bpy.data.collections.new('HERO')
bpy.context.scene.collection.children.link(HERO)


def material(name, color, metal=0, rough=.45, texture=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    n = m.node_tree.nodes
    p = n.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = rough
    if texture:
        noise = n.new('ShaderNodeTexNoise')
        noise.inputs['Scale'].default_value = 24 if metal else 80
        noise.inputs['Detail'].default_value = 3
        bump = n.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = texture
        bump.inputs['Distance'].default_value = .008 if metal else .012
        m.node_tree.links.new(noise.outputs['Fac'], bump.inputs['Height'])
        m.node_tree.links.new(bump.outputs['Normal'], p.inputs['Normal'])
        ramp = n.new('ShaderNodeValToRGB')
        ramp.color_ramp.elements[0].position = .2
        ramp.color_ramp.elements[0].color = (*(c * .72 for c in color), 1)
        ramp.color_ramp.elements[1].position = .8
        ramp.color_ramp.elements[1].color = (*(c * 1.16 for c in color), 1)
        m.node_tree.links.new(noise.outputs['Fac'], ramp.inputs[0])
        m.node_tree.links.new(ramp.outputs[0], p.inputs['Base Color'])
    return m


M = {
    'steel': material('01 | Blackened tempered steel', (.032, .043, .052), .70, .47, .055),
    'steel_light': material('02 | Polished steel edges', (.15, .19, .21), .80, .36, .04),
    'steel_dark': material('03 | Darkened steel recesses', (.029, .042, .047), .72, .44, .14),
    'gold': material('04 | Aged bronze', (.28, .156, .045), .70, .43, .08),
    'gold_light': material('05 | Worn bronze highlights', (.43, .27, .09), .74, .34),
    'leather': material('06 | Oiled oxblood leather', (.075, .038, .026), 0, .57, .24),
    'leather_edge': material('07 | Leather piping', (.14, .071, .035), 0, .56),
    'cloth': material('08 | Forest green wool', (.063, .10, .055), 0, .84, .20),
    'cloth_light': material('09 | Worn green cloth folds', (.095, .135, .068), 0, .87),
    'red': material('10 | Desaturated wine sash', (.14, .043, .033), 0, .82, .22),
    'linen': material('11 | Old warm linen', (.40, .33, .20), 0, .91, .17),
    'fur': material('12 | Collar fur umber', (.23, .195, .139), 0, .87),
    'fur_light': material('13 | Collar fur tips', (.39, .34, .25), 0, .88),
    'skin': material('14 | Warm skin', (.42, .235, .14), 0, .65),
    'skin_shadow': material('15 | Skin creases', (.20, .083, .048), 0, .66),
    'lip': material('16 | Muted lips', (.28, .103, .070), 0, .57),
    'hair': material('17 | Dark ash hair', (.029, .024, .02), 0, .80),
    'hair_light': material('18 | Ash grey strands', (.095, .087, .071), 0, .80),
    'eye_white': material('19 | Eye warm ivory', (.22, .20, .16), 0, .52),
    'iris': material('20 | Hazel iris', (.018, .020, .012), 0, .50),
    'pupil': material('21 | Pupil', (.003, .004, .003), 0, .50),
}


def objmesh(name, vertices, faces, mat, smooth=True):
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    ob = bpy.data.objects.new(name, mesh)
    HERO.objects.link(ob)
    if mat:
        mesh.materials.append(M.get(mat, mat))
    for p in mesh.polygons:
        p.use_smooth = smooth
    return ob


def finish(ob, solid=0, bevel=0, sub=0):
    if sub:
        md = ob.modifiers.new('Form smoothing', 'SUBSURF')
        md.levels = sub
        md.render_levels = sub
    if solid:
        md = ob.modifiers.new('Forged thickness', 'SOLIDIFY')
        md.thickness = solid
        md.offset = 0
    if bevel:
        md = ob.modifiers.new('Hand finished edges', 'BEVEL')
        md.width = bevel
        md.segments = 3
    return ob


def ellipsoid(name, loc, scale, mat, rotation=None):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=20, location=loc)
    ob = bpy.context.object
    ob.name = name
    for c in list(ob.users_collection):
        c.objects.unlink(ob)
    HERO.objects.link(ob)
    ob.scale = scale
    if rotation:
        ob.rotation_euler = rotation
    ob.data.materials.append(M[mat])
    for p in ob.data.polygons:
        p.use_smooth = True
    return ob


def curve(name, points, radius, mat, cyclic=False):
    if cyclic and (Vector(points[0])-Vector(points[-1])).length<.000001:
        points=points[:-1]
    cu = bpy.data.curves.new(name, 'CURVE')
    cu.dimensions = '3D'
    cu.resolution_u = 16
    cu.bevel_depth = radius
    cu.bevel_resolution = 3
    sp = cu.splines.new('BEZIER')
    sp.bezier_points.add(len(points) - 1)
    for bp, co in zip(sp.bezier_points, points):
        bp.co = co
        bp.handle_left_type = 'AUTO'
        bp.handle_right_type = 'AUTO'
    sp.use_cyclic_u = cyclic
    ob = bpy.data.objects.new(name, cu)
    HERO.objects.link(ob)
    cu.materials.append(M[mat])
    return ob


def surface(name, fn, nu, nv, mat, solid=.04, bevel=.015):
    verts = [fn(i / nu, j / nv) for i in range(nu + 1) for j in range(nv + 1)]
    faces = []
    for i in range(nu):
        for j in range(nv):
            a = i * (nv + 1) + j
            faces.append((a, a+1, a+nv+2, a+nv+1))
    return finish(objmesh(name, verts, faces, mat), solid, bevel)


def trim_surface(name, fn, mat='gold', radius=.025, steps=20, inner=True):
    for side in (0, 1):
        curve(name + ' transverse ' + str(side), [fn(side, j/steps) for j in range(steps+1)], radius, mat)
    if inner:
        for side in (0, 1):
            curve(name + ' longitudinal ' + str(side), [fn(j/steps, side) for j in range(steps+1)], radius, mat)


def border_surface(name,fn,width=.04,mat='gold'):
    """Flat metal edging follows the plate instead of floating tubular ornament."""
    def lift(u,v):
        p=Vector(fn(u,v))
        a=Vector(fn(min(1,u+.002),v))-Vector(fn(max(0,u-.002),v))
        b=Vector(fn(u,min(1,v+.002)))-Vector(fn(u,max(0,v-.002)))
        n=a.cross(b).normalized()
        outward=Vector((0,p.y-.035,p.z-5.2)) if name.startswith('Pauldron') else Vector((0,-1,0))
        if n.dot(outward)<0:n=-n
        return p+n*.065
    for side in (0,1):
        surface(name+' end',lambda u,v,side=side:lift(side+(1 if side==0 else -1)*u*width,v),3,32,mat,.014,.006)
        surface(name+' side',lambda u,v,side=side:lift(u,side+(1 if side==0 else -1)*v*width),32,3,mat,.014,.006)


def front_patch(name, outline, depth_fn, mat, thickness=.03, bulge=.02):
    """A curved embossed panel triangulated in concentric rings."""
    center=Vector((sum(p[0] for p in outline)/len(outline),sum(p[1] for p in outline)/len(outline)))
    verts=[]; faces=[]; nn=len(outline)
    for i in range(7):
        t=.03+(1-.03)*i/6
        for p in outline:
            x,z=center.lerp(Vector(p),t)
            verts.append((x,depth_fn(x,z)-bulge*(1-t*t),z))
    for i in range(6):
        for j in range(nn):
            a=i*nn+j; b=i*nn+(j+1)%nn
            faces.append((a,b,b+nn,a+nn))
    faces.append(tuple(reversed(range(nn))))
    return finish(objmesh(name,verts,faces,mat),thickness,.007)


def surface_motif(name,fn,outline,mat='gold',depth=.071):
    """Relief inset expressed in the curved plate's own UV coordinates."""
    verts=[]
    center=(sum(p[0] for p in outline)/len(outline),sum(p[1] for p in outline)/len(outline))
    for u,v in [center,*outline]:
        p=Vector(fn(u,v))
        a=Vector(fn(min(1,u+.001),v))-Vector(fn(max(0,u-.001),v))
        b=Vector(fn(u,min(1,v+.001)))-Vector(fn(u,max(0,v-.001)))
        n=a.cross(b).normalized()
        outward=Vector((0,p.y-.035,p.z-5.2)) if name.startswith('Pauldron') else Vector((0,-1,0))
        if n.dot(outward)<0:n=-n
        verts.append(p+n*(depth+.015 if (u,v)==center else depth))
    nn=len(outline)
    return finish(objmesh(name,verts,[(0,i+1,(i+1)%nn+1) for i in range(nn)],mat,False),.012,.006)


def catmull(points, steps=12):
    result = []
    pts = [Vector(points[0]), *[Vector(x) for x in points], Vector(points[-1])]
    for i in range(1, len(pts)-2):
        a,b,c,d = pts[i-1:i+3]
        for j in range(steps):
            t=j/steps
            result.append((2*b + (-a+c)*t + (2*a-5*b+4*c-d)*t*t + (-a+3*b-3*c+d)*t*t*t)*.5)
    result.append(Vector(points[-1]))
    return result


def sweep(name, points, widths, mat, depth=1, sides=10):
    path=catmull(points, 6)
    verts=[]
    for i,p in enumerate(path):
        t=i/(len(path)-1)*(len(widths)-1)
        k=min(int(t),len(widths)-2)
        r=widths[k]*(1-t+k)+widths[k+1]*(t-k)
        tangent=(path[min(i+1,len(path)-1)]-path[max(i-1,0)]).normalized()
        n=tangent.cross(Vector((0,1,0))).normalized()
        if n.length < .1:
            n=tangent.cross(Vector((1,0,0))).normalized()
        b=tangent.cross(n).normalized()
        for j in range(sides):
            a=2*pi*j/sides
            verts.append(p+r*(cos(a)*n+sin(a)*b*depth))
    faces=[]
    for i in range(len(path)-1):
        for j in range(sides):
            a=i*sides+j; bb=i*sides+(j+1)%sides
            faces.append((a,bb,bb+sides,a+sides))
    faces += [tuple(reversed(range(sides))), tuple((len(path)-1)*sides+j for j in range(sides))]
    return objmesh(name,verts,faces,mat)


def limb_basis(a,b):
    a,b=Vector(a),Vector(b)
    axis=(b-a).normalized()
    x=Vector((1,0,0))
    x=(x-axis*axis.dot(x)).normalized()
    y=axis.cross(x).normalized()
    return a,b,x,y


def tube(name, a, b, radii, mat, angles=(0,2*pi), solid=0, ribs=0):
    a,b,x,y=limb_basis(a,b)
    def fn(u,v):
        t=u*(len(radii)-1); k=min(int(t),len(radii)-2)
        rx=radii[k][0]*(1-t+k)+radii[k+1][0]*(t-k)
        ry=radii[k][1]*(1-t+k)+radii[k+1][1]*(t-k)
        th=angles[0]+(angles[1]-angles[0])*v
        f=1+ribs*.5*(sin(u*65+th*2)+sin(u*33-th*3))
        p=a.lerp(b,u)+x*cos(th)*rx*f+y*sin(th)*ry*f
        return p
    ob=surface(name,fn,30,48,mat,solid, .012 if solid and angles!=(0,2*pi) else 0)
    return ob,fn


def rivet(name, loc, scale=.04, mat='gold'):
    return ellipsoid(name,loc,(scale,scale*.5,scale),mat)


def badge(name, center, r, mat='gold'):
    x,y,z=center
    ellipsoid(name+' bronze boss',center,(r,.095,r),'gold')
    ellipsoid(name+' inset',(x,y-.085,z),(r*.8,.026,r*.8),'steel_dark')
    curve(name+' rolled border',[(x+r*.91*cos(a),y-.08,z+r*.91*sin(a)) for a in [2*pi*i/32 for i in range(32)]],r*.09,mat,True)
    # Original eight-point hammered compass motif, no borrowed insignia.
    for i in range(8):
        a=pi*i/4
        radius=r*(.67 if i%2==0 else .53)
        poly=[(x,y-.132,z),(x+radius*cos(a-.18),y-.112,z+radius*sin(a-.18)),(x+r*.79*cos(a),y-.12,z+r*.79*sin(a)),(x+radius*cos(a+.18),y-.112,z+radius*sin(a+.18))]
        finish(objmesh(name+' radial casting '+str(i),poly,[(0,1,2,3)],mat,False),.025,0)
    ellipsoid(name+' center',(x,y-.16,z),(r*.20,.04,r*.20),'gold_light')


def chainmail(name,a,b,rx,ry,rows=13,cols=24):
    a,b,x,y=limb_basis(a,b)
    verts=[]; faces=[]
    # Interlocking wire loops joined into one mesh, not one object per loop.
    for row in range(rows):
        u=(row+.5)/rows
        for col in range(cols):
            th=2*pi*(col+(row%2)*.5)/cols
            outward=(x*cos(th)+y*sin(th)).normalized()
            tangent=(-x*sin(th)+y*cos(th)).normalized()
            up=(b-a).normalized()
            center=a.lerp(b,u)+x*cos(th)*rx+y*sin(th)*ry
            radius=.045
            start=len(verts)
            for i in range(12):
                t=2*pi*i/12
                radial=tangent*cos(t)+up*sin(t)*1.16
                for j in range(4):
                    q=2*pi*j/4
                    verts.append(center+radial*(radius+.009*cos(q))+outward*.009*sin(q))
            for i in range(12):
                for j in range(4):
                    faces.append(tuple(start+k for k in (i*4+j,((i+1)%12)*4+j,((i+1)%12)*4+(j+1)%4,i*4+(j+1)%4)))
    return objmesh(name,verts,faces,'steel_dark')


# Torso: an anatomically tapered closed cuirass with a sternum ridge.
profile=[(3.82,.64,.37),(4.05,.65,.40),(4.35,.71,.45),(4.68,.88,.56),(5.05,1.04,.65),(5.37,1.08,.64),(5.60,.99,.53),(5.80,.67,.40),(5.89,.43,.33)]
def torso(u,v,offset=0):
    t=u*(len(profile)-1); i=min(int(t),len(profile)-2); f=t-i
    z,rx,ry=[profile[i][k]*(1-f)+profile[i+1][k]*f for k in range(3)]
    th=2*pi*v
    x=rx*cos(th)
    front=max(0,-sin(th))
    y=ry*sin(th)-.075*front**12*sin(pi*u)
    # Rib-cage shape and bilateral hammered pectoral facets.
    y-=.05*front**3*math.exp(-((z-5.2)/.4)**2)*sin(abs(x)*pi)
    return (x,y+offset,z)
finish(surface('Cuirass | continuous forged torso',torso,56,96,'steel',.095,.018),sub=1)
for u in (0,.97):
    curve('Cuirass | collar and waist rolled edge',[torso(u,j/80,-.014) for j in range(81)],.027,'gold',True)

# Front armor ornament, a sparse branching construction that follows the chest.
def cy(x,z):
    for i in range(len(profile)-1):
        if profile[i][0]<=z<=profile[i+1][0]:
            f=(z-profile[i][0])/(profile[i+1][0]-profile[i][0])
            rx=profile[i][1]*(1-f)+profile[i+1][1]*f
            ry=profile[i][2]*(1-f)+profile[i+1][2]*f
            front=math.sqrt(max(.005,1-(x/rx)**2))
            u=(i+f)/(len(profile)-1)
            return -ry*front-.075*front**12*sin(pi*u)-.05*front**3*math.exp(-((z-5.2)/.4)**2)*sin(abs(x)*pi)-.031
    return -.45
for s in (-1,1):
    paths=[[(.02,4.33),(.11,4.72),(.21,5.17),(.04,5.70)],[(.10,4.69),(.38,4.90),(.65,5.00),(.83,5.18)],[(.15,5.02),(.39,5.39),(.66,5.49),(.77,5.66)],[(.02,4.38),(.41,4.52),(.64,4.69)],[(.27,5.34),(.44,5.64),(.34,5.80)]]
    for k,pa in enumerate(paths):
        curve('Cuirass | bronze rib '+str(s)+' '+str(k),[(s*x,cy(s*x,z)-.012,z) for x,z in pa],.024 if k==0 else .011,'gold')
    for z,x in ((4.62,.63),(5.0,.78),(5.5,.66)):
        curve('Cuirass | engraved curl',[(s*(x-.16),cy(s*(x-.16),z+.06)-.015,z+.06),(s*x,cy(s*x,z)-.025,z),(s*(x-.07),cy(s*(x-.07),z-.1)-.025,z-.1)],.008,'gold_light')

# A soft collar under the plate collar, with a raised rear neck support.
tube('Neck | leather gorget',(0,0,5.60),(0,0,6.12),[(.43,.34),(.39,.33),(.32,.29)],'leather',solid=.04)
for i in range(3):
    z=5.70+i*.11
    curve('Gorget | collar roll',[(.42*cos(a),.35*sin(a),z+.045*cos(a)) for a in [2*pi*k/40 for k in range(40)]],.025,'gold',True)

# Legs: wide grounded stance, cloth volume beneath articulated armor.
for s in (-1,1):
    hip=(s*.50,.04,3.97); knee=(s*.73,-.06,2.34); ankle=(s*.89,.03,.66)
    tube('Leg | padded thigh '+str(s),knee,hip,[(.32,.34),(.43,.40),(.48,.41)],'leather',ribs=.022)
    tube('Leg | calf boot '+str(s),ankle,knee,[(.27,.30),(.38,.37),(.31,.30)],'leather',ribs=.012)
    chainmail('Mail | thigh skirt '+str(s),(s*.60,.025,2.75),(s*.52,.03,3.45),.46,.42,12,29)
    # Front cuisse: curved, asymmetrically pointed plates over the outer thigh.
    for layer in range(3):
        top=3.91-layer*.30
        def thigh(u,v,s=s,top=top,layer=layer):
            ang=(-.98+1.96*v)
            z=top-u*.66-.26*(sin(pi*v)**4)*u**2
            x=s*(.57+(3.9-z)*.14)+sin(ang)*(.44+.04*u)
            y=-cos(ang)*(.46+.07*u)-.065-layer*.005
            return (x,y,z)
        surface('Cuisses | lapped plate '+str(s)+' '+str(layer),thigh,14,24,'steel',.065,.021)
        trim_surface('Cuisses | bronze seam',thigh,radius=.021)
        border_surface('Cuisses | wide forged border',thigh,.065)
        for v in (.12,.88):
            rivet('Cuisses | fastener',thigh(.18,v),.027)
    # Forged kneecap with a central ridge and lateral wing.
    def kneecap(u,v,s=s):
        z=2.13+u*.63
        w=.10+.28*sin(pi*u)**.6
        x=s*.74+(v*2-1)*w
        y=-.39-.16*sin(pi*u)*sin(pi*v)
        z+=.05*sin(pi*v)
        return (x,y,z)
    surface('Poleyn | shaped knee '+str(s),kneecap,20,24,'steel',.07,.014)
    trim_surface('Poleyn | edge',kneecap,radius=.026)
    border_surface('Poleyn | forged bronze bezel',kneecap,.065)
    surface_motif('Poleyn | tapered cast crest',kneecap,[(.13,.50),(.43,.40),(.52,.16),(.61,.40),(.91,.50),(.61,.60),(.52,.84),(.43,.60)],depth=.078)
    curve('Poleyn | central arris',[(s*.74,-.41,2.17),(s*.74,-.58,2.44),(s*.74,-.43,2.76)],.027,'gold')
    # Greave with a long front keel. The open back exposes leather and straps.
    def greave(u,v,s=s):
        z=.62+u*1.53
        ang=-1.22+2.44*v
        radius=.27+.115*sin(pi*u*.86)
        x=s*(.9-.14*u)+sin(ang)*radius
        y=.035-cos(ang)*(radius+.035)-.15*(sin(pi*v)**5)*sin(pi*u)
        return (x,y,z+.13*sin(pi*v)*u**6)
    surface('Greave | sculpted shin '+str(s),greave,36,32,'steel',.065,.023)
    trim_surface('Greave | rim',greave,radius=.029)
    border_surface('Greave | broad bronze rim',greave,.045)
    surface_motif('Greave | pointed upper crest',greave,[(.58,.50),(.77,.39),(.82,.13),(.86,.40),(.97,.50),(.86,.60),(.82,.87),(.77,.61)],depth=.074)
    curve('Greave | forged ridge',[greave(i/20,.5) for i in range(21)],.022,'steel_light')
    for z in (.80,1.31,1.92):
        center=(s*(.9-.14*(z-.62)/1.53),.04,z)
        tube('Greave | wrap strap '+str(s), (center[0],center[1],z-.065),(center[0],center[1],z+.065),[(.37,.385),(.37,.385)],'leather',solid=.025)
        for d in (-.12,.12):
            rivet('Greave | strap stud',(center[0]+d,-.382,z),.025)
    # Sabatons: broad toe with overlapping curved transverse plates.
    footx=s*.94
    ellipsoid('Boot | leather foot '+str(s),(footx,-.27,.35),(.36,.64,.27),'leather')
    ellipsoid('Sabatons | enclosed steel toecap '+str(s),(footx,-.75,.34),(.35,.23,.21),'steel')
    curve('Sabatons | toe bumper',[(footx+.355*cos(a),-.66+.31*sin(a),.21) for a in [pi+pi*i/30 for i in range(31)]],.027,'gold')
    def sole(u,v,s=s,footx=footx):
        a=2*pi*v
        x=footx+.38*math.copysign(abs(cos(a))**.72,cos(a))
        y=-.28+.69*math.copysign(abs(sin(a))**.86,sin(a))
        return (x,y,.12+u*.11)
    surface('Boot | heavy sole '+str(s),sole,2,64,'steel_dark',.045,.018)
    for i in range(5):
        ya=-.88+i*.21
        def footplate(u,v,ya=ya,footx=footx,i=i):
            ang=-1.33+v*2.66
            w=.345 if i<3 else .31
            return (footx+w*sin(ang),ya+u*.245,.23+(.28+i*.055)*cos(ang)+.065*u)
        surface('Sabatons | toe articulation '+str(s)+' '+str(i),footplate,10,24,'steel',.04,.013)
        curve('Sabatons | rolled lip',[footplate(0,i/24) for i in range(25)],.017,'gold')

# Body belt, a woven sash under substantial leather work.
tube('Waist | gathered wine sash',(0,0,3.79),(0,0,4.17),[(.77,.47),(.81,.50),(.72,.44)],'red',ribs=.025,solid=.04)
tube('Waist | broad leather belt',(0,0,3.93),(0,0,4.22),[(.79,.52),(.78,.51)],'leather',solid=.05)
for z in (3.95,4.20):
    curve('Belt | stitched edge',[(.79*cos(a),.535*sin(a),z) for a in [2*pi*i/80 for i in range(80)]],.016,'leather_edge',True)
badge('Belt | compass buckle',(0,-.60,4.08),.25)
for x in (-.66,-.43,.43,.66):
    rivet('Belt | pin',(x,-.52*math.sqrt(1-(x/.8)**2)-.055,4.085),.035)

# Flowing tabards have actual folds, tapered free ends and embroidered hems.
def hanging_cloth(name,xcenter,topz,bottomz,wt,wb,ybase,mat,phase):
    def fn(u,v):
        x=xcenter+(v-.5)*(wt*(1-u)+wb*.64*u)+.23*sin(pi*u)*sin(phase)+.16*u*u*sin(phase+1)
        z=topz+(bottomz-topz)*u+u**6*(.65*abs(v-.5)*2+.04*sin(v*23+phase))
        y=ybase-.1*sin(pi*u)+(.035+.055*u)*sin(v*4*pi+phase+u*1.6)+.15*u*sin(u*4+phase)
        return (x,y,z)
    surface(name,fn,50,30,mat,.026,.008)
    for vv in (.035,.965):
        curve(name+' sewn long border',[fn(i/40,vv) for i in range(41)],.012,'linen')
    curve(name+' bottom hem',[fn(.93,i/30) for i in range(31)],.025,'linen')
    # Angular needlework follows the cloth, restrained to the lower border.
    for i in range(6):
        v=.10+i*.14
        curve(name+' embroidery chevron',[fn(.86,v-.045),fn(.89,v),fn(.86,v+.045)],.009,'gold')
    return fn
hanging_cloth('Tabard | old linen underlayer',-.14,3.85,1.53,.67,.74,-.48,'linen',1.3)
hanging_cloth('Tabard | green central pennant',.12,3.93,.98,.62,.81,-.62,'cloth',.4)
hanging_cloth('Tabard | wine scarf end',.39,4.00,1.89,.32,.41,-.76,'red',2.0)

# Arms: cloth joints, linked mail, articulated plate vambraces and fingers.
for s in (-1,1):
    shoulder=(s*1.19,.02,5.46); elbow=(s*1.57,-.06,4.56); wrist=(s*1.85,-.31,3.72)
    tube('Arm | quilted upper sleeve '+str(s),elbow,shoulder,[(.29,.29),(.39,.37),(.40,.36)],'leather',ribs=.029)
    chainmail('Mail | upper arm '+str(s),(s*1.47,-.02,4.78),(s*1.28,.02,5.41),.415,.397,12,29)
    # Soft elbow volume retains a fleshy rather than mechanical joint.
    ellipsoid('Arm | inner linen elbow '+str(s),elbow,(.29,.29,.32),'linen')
    tube('Arm | forearm glove '+str(s),wrist,elbow,[(.23,.24),(.31,.31),(.32,.31)],'leather',ribs=.017)
    arm,afn=tube('Vambrace | fluted steel '+str(s),wrist,(s*1.60,-.08,4.44),[(.26,.27),(.32,.34),(.37,.355)],'steel',angles=(pi,2*pi),solid=.065)
    trim_surface('Vambrace | bronze edges',afn,radius=.027)
    curve('Vambrace | central spine',[afn(i/25,.5) for i in range(26)],.027,'gold')
    for u in (.14,.87):
        curve('Vambrace | cross strap',[afn(u,i/24) for i in range(25)],.054,'leather')
        rivet('Vambrace | strap pin',afn(u,.3),.027)
    for u in (.4,.61):
        curve('Vambrace | etched sweep',[afn(u-.12,.16),afn(u,.35),afn(u+.15,.5),afn(u,.68),afn(u-.12,.85)],.012,'gold')
    # Side elbow wing, not an exposed ball joint.
    ellipsoid('Couter | elbow cap '+str(s),(s*1.74,-.04,4.55),(.22,.34,.32),'steel')
    curve('Couter | cast ridge',[(s*1.83,-.27,4.35),(s*1.94,-.08,4.60),(s*1.71,.03,4.84)],.028,'gold')
    # Gauntlet palm, oriented fingers following a believable relaxed curl.
    hc=Vector((s*1.92,-.39,3.47))
    ellipsoid('Hand | leather palm '+str(s),hc,(.245,.17,.30),'leather')
    def handplate(u,v,s=s,hc=hc):
        return (hc.x+(v-.5)*(.42-.10*u),hc.y-.14-.045*sin(pi*v),3.72-u*.43+.055*sin(pi*v))
    surface('Gauntlet | metacarpal plate '+str(s),handplate,12,18,'steel',.035,.012)
    trim_surface('Gauntlet | trim',handplate,radius=.015)
    for finger in range(4):
        fx=hc.x+(finger-1.5)*.11
        length=.34+(1-abs(finger-1.5)/1.5)*.08
        start=Vector((fx,-.43,3.30))
        if s<0:
            points=[start,(fx,-.54,3.14),(fx,-.43,3.04),(fx,-.32,3.17)]
        else:
            points=[start,(fx,-.46,3.13),(fx,-.40,3.30-length),(fx,-.28,3.28-length)]
        sweep('Finger | articulated glove '+str(s)+' '+str(finger),points,[.064,.060,.052,.042],'leather',.9,12)
        for joint,p in enumerate(points[:3]):
            ellipsoid('Finger | steel knuckle '+str(s),Vector(p)+Vector((0,-.036,0)),(.059,.033,.060),'steel')
        curve('Hand | tendon plate ridge',[(fx,-.585,3.66),(fx,-.594,3.45),(fx,-.51,3.30)],.017,'steel_light')
    thumb=[(hc.x-s*.19,-.39,3.52),(hc.x-s*.30,-.45,3.35),(hc.x-s*.27,-.35,3.18)]
    sweep('Thumb | leather articulated '+str(s),thumb,[.085,.074,.052],'leather',1,12)
    ellipsoid('Thumb | steel knuckle '+str(s),thumb[1],(.09,.065,.11),'steel')

# Cascading shoulder armor. Each plate is a varying curved shell, not a sphere.
for s in (-1,1):
    bigger=1.04 if s==1 else .93
    for layer in range(3):
        start=.71+layer*.26
        end=1.63+layer*.155
        def shoulder_fn(u,v,s=s,layer=layer,start=start,end=end,bigger=bigger):
            xx=start+(end-start)*u
            angle=-1.50+3.03*v
            width=(.42+.27*sin(pi*u*.8))*bigger
            ztop=6.11-.48*u**1.5-layer*.195
            yy=.035+width*sin(angle)
            zz=ztop-(.41+.08*u)*(1-cos(angle))+.085*sin(pi*u)-.07*u**8*sin(pi*v*3)**2
            return (s*xx*bigger,yy,zz)
        surface('Pauldron | cascading shell '+str(s)+' '+str(layer),shoulder_fn,28,40,'steel',.09,.027)
        trim_surface('Pauldron | broad bronze casting',shoulder_fn,radius=.038 if layer==0 else .027)
        border_surface('Pauldron | wide flat gilt frame',shoulder_fn,.075 if layer==0 else .065)
        # A thin inner parallel engraving makes the raised frame read as forged metal.
        for vv in (.055,.945):
            curve('Pauldron | inset engraved rim',[shoulder_fn(i/30,vv) for i in range(31)],.012,'gold_light')
        for u in (.17,.50,.83):
            for v in (.055,.945):
                rivet('Pauldron | edge rivet',shoulder_fn(u,v),.028)
        if layer==0:
            surface_motif('Pauldron | angular raised relief '+str(s),shoulder_fn,[(.15,.18),(.32,.16),(.44,.05),(.54,.19),(.82,.21),(.57,.29),(.53,.47),(.40,.30),(.23,.30)],depth=.078)
            for uu in (.28,.60):
                curve('Pauldron | raised reinforcing flute',[shoulder_fn(uu,j/20) for j in range(21)],.025,'steel_light')
            # Raised inner flange describes the high iconic shoulder contour.
            def flange(u,v,s=s,bigger=bigger):
                a=-1.5+3.02*v
                return (s*(.71+.11*u)*bigger,.04+.43*sin(a),6.11-.41*(1-cos(a))+.13*u)
            surface('Pauldron | standing inner flange '+str(s),flange,4,40,'gold',.055,.02)
            curve('Pauldron | flange rolled lip',[flange(1,j/30) for j in range(31)],.027,'gold_light')
            badge('Pauldron | circular clasp '+str(s),(s*.87*bigger,-.47,5.66),.20 if s<0 else .26)

# Green half cloak: continuous draped cloth with a windless S-shaped hanging mass.
def cape(u,v):
    width=.90+1.50*sin(pi*u*.52)
    center=.59+.52*u+.21*sin(u*5)
    x=center+(v-.5)*width
    y=.46+.41*sin(min(1,u/.24)*pi/2)+.18*sin(pi*u)+.17*sin(v*pi*5+.75*u)*(sin(pi*u*.85)+.20)+.05*u
    z=5.94-4.93*u+(.27*sin(v*9+1)+.45*v)*u**8-.37*abs(v-.5)*sin(pi*u)
    return (x,y,z)
surface('Cloak | asymmetrical forest wool',cape,90,64,'cloth',.042,.009)
for v in (.026,.974):
    curve('Cloak | embroidered longitudinal hem',[cape(i/80,v) for i in range(81)],.032,'linen')
for u in (.92,.96):
    curve('Cloak | embroidered lower border',[cape(u,j/64) for j in range(65)],.025,'linen')
for j in range(16):
    v=.07+j*.056
    curve('Cloak | hem needlework',[cape(.87,v-.018),cape(.90,v),cape(.87,v+.018)],.01,'gold')
# Folds of the scarf cross the collar and cover the cloak attachment.
for i in range(4):
    sweep('Scarf | gathered neck fold '+str(i),[(-.48,-.17,5.91-i*.035),(-.15,-.43,5.80-i*.045),(.32,-.39,5.86-i*.04),(.84,.04,5.97-i*.05)],[.09,.11,.09,.035],'cloth' if i%2 else 'cloth_light',.75,12)

# Fur collar modeled as tapered, directional layered locks.
for i in range(76):
    a=2*pi*i/76
    if sin(a)<-.5:
        continue
    center=Vector((.52*cos(a),.34*sin(a)+.04,5.91+.05*cos(a)))
    off=Vector((cos(a)*.22,sin(a)*.19,.12))
    for row in range(2):
        base=center+Vector((cos(a)*row*.075,sin(a)*row*.075,-row*.055))
        dest=base+off*random.uniform(.8,1.6)+Vector((0,0,random.uniform(-.035,.14)))
        sweep('Fur | tapered collar lock', [base,base+off*.55,dest], [.085,.063,.004], 'fur_light' if i%3==0 else 'fur',.7,7)

# Belt hanger, original forged sword and a grip actually inside the curled hand.
sword_top=Vector((-1.94,-.34,3.56))
sword_axis=Vector((-.255,-.025,-.967)).normalized()
sword_side=Vector((sword_axis.z,0,-sword_axis.x)).normalized()
normal=Vector((0,-1,0))
def sw(x,y,z):
    return sword_top+sword_side*x+normal*y+sword_axis*z
# Pommel is above the hand. Guard starts directly below the curled fingers.
sweep('Sword | leather bound grip',[sw(0,0,-.33),sw(0,0,.17),sw(0,0,.39)],[.096,.098,.097],'leather',1,16)
for i in range(12):
    curve('Sword | grip binding',[sw(.103*cos(a),.103*sin(a),-.29+i*.051+.013*a/pi) for a in [2*pi*k/28 for k in range(29)]],.016,'leather_edge')
ellipsoid('Sword | bronze pommel',sw(0,0,-.37),(.15,.12,.15),'gold')
guardz=.41
guardpts=[(-.48,guardz+.09),(-.46,guardz-.07),(-.36,guardz-.16),(-.34,guardz-.04),(-.14,guardz-.015),(0,guardz-.12),(.14,guardz-.02),(.34,guardz-.04),(.41,guardz-.14),(.50,guardz-.13),(.47,guardz+.09),(.32,guardz+.10),(0,guardz+.07),(-.3,guardz+.13)]
gv=[sw(x,y,z) for y in (-.085,.085) for x,z in guardpts]
ng=len(guardpts)
gf=[tuple(reversed(range(ng))),tuple(range(ng,ng*2))]+[(i,(i+1)%ng,(i+1)%ng+ng,i+ng) for i in range(ng)]
finish(objmesh('Sword | sculpted hooked bronze guard',gv,gf,'gold',False),bevel=.027)
badge('Sword | guard seal',sw(0,.125,guardz),.135)
# Asymmetric cleaver blade; edge, fuller, spine are distinct physical strips.
blade_sections=[(.51,-.25,.23),(.78,-.27,.24),(1.0,-.285,.24),(2.40,-.34,.24),(2.80,-.45,.24),(2.88,-.47,.24),(3.22,-.20,.12),(3.46,.025,.06)]
vv=[]
for z,left,right in blade_sections:
    for y in (.06,-.06):
        for f in (0,.15,.46,.64,.94,1):
            x=left+(right-left)*f
            edge=min(abs(x-left),abs(x-right))
            yy=math.copysign(.003+abs(y)*min(1,edge/.065),y)
            vv.append(sw(x,yy,z))
ff=[]; matids=[]
for i in range(len(blade_sections)-1):
    for side in range(2):
        for j in range(5):
            a=i*12+side*6+j
            ff.append((a,a+1,a+13,a+12)); matids.append(1 if j in (0,4) else (2 if j==2 else 0))
    for j in (0,5):
        a=i*12+j; ff.append((a,a+6,a+18,a+12));matids.append(1)
ring=[0,1,2,3,4,5,11,10,9,8,7,6]
ff += [tuple(reversed(ring)),tuple((len(blade_sections)-1)*12+j for j in ring)];matids +=[0,0]
blade=objmesh('Sword | broad asymmetric beveled blade',vv,ff,'steel',False)
blade.data.materials.append(M['steel_light']);blade.data.materials.append(M['steel_dark'])
for p,mi in zip(blade.data.polygons,matids):p.material_index=mi
finish(blade,bevel=0)
for x in (-.05,.07):
    curve('Sword | bronze fuller inlay',[sw(x,.067,z) for z in (.62,.85,1.3,2.1,2.7,2.97)],.012,'gold')
for i in range(5):
    z=.73+i*.24
    curve('Sword | inset herringbone', [sw(-.035,.074,z),sw(.015,.08,z+.06),sw(.056,.074,z)],.008,'gold_light')

# Crossing chest leather baldric, with folded seams and real buckle hardware.
strap_points=[(-.69,-.36,5.80),(-.59,-.58,5.46),(-.39,-.66,5.12),(-.23,-.64,4.74),(-.05,-.54,4.30)]
path=catmull(strap_points,14)
verts=[]
for p in path:
    verts.extend([p+Vector((-.080,-.026,.035)),p+Vector((.080,-.026,-.035))])
finish(objmesh('Baldric | continuous diagonal leather',verts,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(path)-1)],'leather'),.036,.014)
for side in (-1,1):
    curve('Baldric | raised seam',[p+Vector((side*.068,-.046,-side*.028)) for p in path],.009,'leather_edge')
for z,x,y in ((5.43,-.57,-.64),(4.81,-.25,-.70)):
    curve('Baldric | buckle frame',[(x-.10,y,z-.09),(x+.10,y,z-.09),(x+.10,y,z+.09),(x-.10,y,z+.09)],.022,'gold',True)
    curve('Baldric | buckle tongue',[(x,y-.008,z-.085),(x,y-.025,z+.075)],.013,'gold_light')

# Fine directional wear is modeled sparingly on large surfaces.
for i in range(32):
    x=random.uniform(-.73,.73);z=random.uniform(4.42,5.6)
    length=random.uniform(.035,.10)
    if abs(x)>.12:
        curve('Cuirass | incidental worn nick',[(x,cy(x,z)-.023,z),(x+length*.32,cy(x+length*.32,z+length)-.023,z+length)],.0035,'steel_light')

# Shaped pectoral plates and pointed waist lames provide broad, designed planes.
for ob in list(HERO.objects):
    if ob.name.startswith(('Cuirass | bronze rib','Cuirass | engraved curl','Cuirass | incidental worn nick')):
        bpy.data.objects.remove(ob,do_unlink=True)
for s in (-1,1):
    petals=[[(.095,5.69),(.35,5.73),(.65,5.57),(.94,5.30),(.85,5.07),(.61,5.04),(.36,5.20),(.15,5.29)],
            [(.12,5.13),(.37,5.00),(.60,4.92),(.86,4.96),(.76,4.70),(.43,4.56),(.19,4.65)],
            [(.11,4.76),(.36,4.53),(.67,4.52),(.63,4.31),(.25,4.24),(.10,4.38)]]
    for k,out in enumerate(petals):
        out=[(s*x,z) for x,z in out]
        front_patch('Cuirass | shaped bronze panel '+str(s)+' '+str(k),out,lambda x,z:cy(x,z)-.026,'gold',.025,.055)
        cx=sum(x for x,z in out)/len(out);cz=sum(z for x,z in out)/len(out)
        inset=[(cx+(x-cx)*.89,cz+(z-cz)*.88) for x,z in out]
        front_patch('Cuirass | shaped inset steel '+str(s)+' '+str(k),inset,lambda x,z:cy(x,z)-.058,'steel',.024,.061)
    center=[(s*.012,4.26),(s*.068,4.60),(s*.10,5.00),(s*.14,5.36),(s*.075,5.71),(s*.025,5.82)]
    front_patch('Cuirass | tapered sternum casting '+str(s),center,lambda x,z:cy(x,z)-.060,'gold',.025,.016)

# A diagonally tied sash breaks the repeated verticals above the tabards.
def sash(u,v):
    x=-.79+1.37*u
    z=3.57+.34*u+(v-.5)*.22
    y=-.65-.08*sin(pi*u)-.035*sin(v*3*pi+u*2)
    return (x,y,z)
surface('Sash | diagonal wine wrap',sash,40,16,'red',.027,.008)
for v in (0,1):curve('Sash | hem',[sash(i/30,v) for i in range(31)],.013,'linen')
ellipsoid('Sash | folded knot',(.53,-.55,3.94),(.16,.09,.12),'red')
# Belt suspension tongues anchor the faulds to the waist.
for s in (-1,1):
    def hanger(u,v,s=s):
        x=s*(.62+.14*u)+(v-.5)*.19
        z=4.12-.67*u+.07*abs(v-.5)*u
        return (x,-.42-.07*sin(pi*u),z)
    surface('Belt | fauld suspension strap '+str(s),hanger,20,6,'leather',.036,.016)
    for u in (.18,.79):rivet('Belt | suspension pin',hanger(u,.5),.032)
    curve('Belt | suspension buckle',[hanger(.26,.05),hanger(.26,.95),hanger(.52,.95),hanger(.52,.05)],.020,'gold',True)

# Turn the sabatons out and widen the lower stance without moving the upper body.
from mathutils import Matrix
leg_prefixes=('Leg |','Mail | thigh','Cuisses |','Poleyn |','Greave |','Boot |','Sabatons |')
for ob in list(HERO.objects):
    if not ob.name.startswith(leg_prefixes):continue
    coords=[ob.matrix_world@v.co for v in ob.data.vertices] if ob.type=='MESH' else [ob.matrix_world@p.co for sp in ob.data.splines for p in sp.bezier_points]
    if not coords:continue
    s=1 if sum(p.x for p in coords)>0 else -1
    rot=Matrix.Rotation(s*.17,3,'Z')
    pivot=Vector((s*.94,0,.1))
    def pose(p):
        p=p.copy()
        if ob.name.startswith(('Boot |','Sabatons |')):p=pivot+rot@(p-pivot)
        p.x+=s*.16*max(0,min(1,(3.8-p.z)/3.0))
        return p
    inv=ob.matrix_world.inverted()
    if ob.type=='MESH':
        for v in ob.data.vertices:v.co=inv@pose(ob.matrix_world@v.co)
    else:
        for sp in ob.data.splines:
            for p in sp.bezier_points:
                p.co=inv@pose(ob.matrix_world@p.co)
                p.handle_left_type='AUTO';p.handle_right_type='AUTO'

# Integrate the independently sculpted local human head.
sys.path.insert(0,str(ROOT/'tools'))
from hero_veteran_head import build_head
head_objects=build_head(M,HERO)
for key in ('hair','hair_light'):
    M[key].node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.13
for key in ('eye_white','iris','pupil'):
    M[key].node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=0 if key=='pupil' else .10

# Weld periodic surface seams and consistently orient the authored mesh shells.
import bmesh
for ob in HERO.objects:
    if ob.type!='MESH':continue
    bm=bmesh.new();bm.from_mesh(ob.data)
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
    bmesh.ops.dissolve_degenerate(bm,edges=list(bm.edges),dist=.0000001)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(ob.data);bm.free();ob.data.update()

# Feet contact the preview floor; the source remains at the same modeling scale.
for ob in HERO.objects:
    ob.location.z-=.055

groups={}
for name in ('01 Face and hair','02 Forged armor and leather','03 Cloth and fur','04 Linked mail','05 Sword'):
    group=bpy.data.collections.new(name)
    HERO.children.link(group)
    groups[name]=group
head_names={ob.name for ob in head_objects}
for ob in list(HERO.objects):
    if ob.name in head_names:group=groups['01 Face and hair']
    elif ob.name.startswith('Sword |'):group=groups['05 Sword']
    elif ob.name.startswith('Mail |'):group=groups['04 Linked mail']
    elif ob.name.startswith(('Cloak |','Tabard |','Scarf |','Sash |','Fur |','Waist | gathered')):group=groups['03 Cloth and fur']
    else:group=groups['02 Forged armor and leather']
    group.objects.link(ob);HERO.objects.unlink(ob)

scene=bpy.context.scene
scene.unit_settings.system='METRIC'
scene.unit_settings.scale_length=.3
scene['asset_status']='Original static character study; no rig or Unity integration.'
scene['direction']='Modern stylized fantasy; local Blender geometry; open human face and layered forged armor.'
scene['lore_status']='Unnamed arena veteran. No faction or biography canon has been established.'
scene['source_concept']='art/concepts/2026-10-05-heavy-warrior-v2.png'
scene['external_generation']='None for the 3D model. Created locally in Blender.'
for ob in HERO.all_objects:
    ob['asset']='veteran'

# Pack all internal data and provide a useful material viewport when opened.
for area in bpy.context.screen.areas if bpy.context.screen else []:
    if area.type=='VIEW_3D':
        area.spaces.active.clip_end=1000
        area.spaces.active.shading.type='MATERIAL'
        area.spaces.active.region_3d.view_distance=11
        area.spaces.active.region_3d.view_location=(0,0,3.6)
bpy.ops.object.select_all(action='DESELECT')
scene.render.engine='CYCLES'
scene.render.resolution_x=1400
scene.render.resolution_y=1600
scene.render.resolution_percentage=100
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/BLEND_NAME))
report={'blender':bpy.app.version_string,'objects':len(HERO.all_objects),'head_objects':len(head_objects),'materials':len(bpy.data.materials),'blend':str(OUT/BLEND_NAME),'local_only':True}
(ROOT/'verification/veteran-build.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('VETERAN_BUILD_OK '+json.dumps(report))

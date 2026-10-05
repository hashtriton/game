"""Original local bust study. Run only in an isolated Blender background process.

All surfaces and materials are authored here. No external assets or network use.
This is a visual sculpt study, not a rigged or game-ready character.
"""
import bpy, math, random, json, sys
from pathlib import Path
from mathutils import Vector
from math import sin, cos, pi, exp, sqrt

if not bpy.app.background:
    raise RuntimeError('This study must run in a separate background process')
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'art/heroes/astra-bust-trial'
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'previews').mkdir(exist_ok=True)
random.seed(81)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.use_preferences_save=False
COL={}
for n in ['01 Sculpt and eyes','02 Groom','03 Forged cuirass','04 Mantle and collar','05 Studio']:
    COL[n]=bpy.data.collections.new(n);bpy.context.scene.collection.children.link(COL[n])
ACTIVE=COL['01 Sculpt and eyes']

def mesh(name,verts,faces,mat,sub=0):
    m=bpy.data.meshes.new(name);m.from_pydata(verts,[],faces);m.update()
    o=bpy.data.objects.new(name,m);ACTIVE.objects.link(o)
    if mat:m.materials.append(mat)
    for p in m.polygons:p.use_smooth=True
    if sub:
        md=o.modifiers.new('Surface refinement','SUBSURF');md.levels=sub
    return o

def material(name,color,metal=0,rough=.45,noise=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    n=m.node_tree.nodes;l=m.node_tree.links;p=n.get('Principled BSDF')
    p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
    if noise:
        tex=n.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=120 if metal else 80;tex.inputs['Detail'].default_value=3
        b=n.new('ShaderNodeBump');b.inputs['Strength'].default_value=noise;b.inputs['Distance'].default_value=.018 if metal else .007
        l.new(tex.outputs['Fac'],b.inputs['Height']);l.new(b.outputs['Normal'],p.inputs['Normal'])
        ramp=n.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.15;ramp.color_ramp.elements[0].color=(*(c*.58 for c in color),1)
        ramp.color_ramp.elements[1].position=.82;ramp.color_ramp.elements[1].color=(*(min(1,c*1.2) for c in color),1)
        l.new(tex.outputs['Fac'],ramp.inputs[0]);l.new(ramp.outputs[0],p.inputs['Base Color'])
    return m

skin=material('Warm weathered skin',(.32,.175,.103),rough=.58,noise=.10)
for e,c in zip(skin.node_tree.nodes.get('Color Ramp').color_ramp.elements,[(.302,.164,.096,1),(.334,.184,.110,1)]):e.color=c
skin.node_tree.nodes.get('Principled BSDF').inputs['Subsurface Weight'].default_value=.055
skin.node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.27
skin.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.68
lip=material('Natural lip',(.29,.125,.082),rough=.49)
crease=material('Deep warm creases',(.075,.025,.016),rough=.7)
hair=material('Charcoal umber hair',(.025,.018,.013),rough=.73,noise=.10)
hair2=material('Warm hair glints',(.038,.027,.019),rough=.77)
gray=material('Iron grey strands',(.12,.115,.10),rough=.75)
for m in [hair,hair2,gray]:m.node_tree.nodes.get('Principled BSDF').inputs['Specular IOR Level'].default_value=.18
white=material('Ivory sclera',(.31,.285,.235),rough=.34)
iris=material('Muted hazel iris',(.072,.079,.035),rough=.32)
pupil=material('Pupil',(.005,.006,.004),rough=.14)
steel=material('Hammered blackened steel',(.071,.080,.082),.85,.36,.20)
edge=material('Worn bronze edge',(.35,.208,.078),.8,.32,.22)
goldDark=material('Recessed aged bronze',(.14,.079,.026),.75,.5,.21)
cloth=material('Deep olive woven cloth',(.055,.068,.034),rough=.92,noise=.35)
leather=material('Oxblood leather',(.070,.032,.020),rough=.6,noise=.23)
black=material('Shadow in construction',(.014,.017,.016),metal=.1,rough=.7)

def ell(name,loc,scale,mat,seg=48,rings=32):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=loc)
    o=bpy.context.object;o.name=name
    for c in list(o.users_collection):c.objects.unlink(o)
    ACTIVE.objects.link(o);o.scale=scale;o.data.materials.append(mat)
    for p in o.data.polygons:p.use_smooth=True
    return o

def curve(name,pts,r,mat,radii=None):
    c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=12;c.bevel_depth=r;c.bevel_resolution=3
    sp=c.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
    for i,(b,p) in enumerate(zip(sp.bezier_points,pts)):
        b.co=p;b.handle_left_type=b.handle_right_type='AUTO';b.radius=radii[i] if radii else 1
    o=bpy.data.objects.new(name,c);ACTIVE.objects.link(o);c.materials.append(mat);return o

def interp(points,z):
    if z<=points[0][0]:return points[0][1]
    if z>=points[-1][0]:return points[-1][1]
    for (a,v),(b,w) in zip(points,points[1:]):
        if a<=z<=b:
            t=(z-a)/(b-a);t=t*t*(3-2*t);return v+(w-v)*t

WIDTH=[(5.19,.08),(5.29,.43),(5.43,.60),(5.64,.73),(5.91,.83),(6.23,.90),(6.53,.97),(6.85,.94),(7.10,.96),(7.43,.99),(7.72,.91),(7.97,.65),(8.17,.06)]
FRONT=[(5.19,-.40),(5.35,-.71),(5.6,-.79),(5.9,-.80),(6.15,-.78),(6.45,-.78),(6.8,-.76),(7.13,-.83),(7.43,-.79),(7.7,-.62),(7.97,-.33),(8.17,.13)]

def g(x,z,cx,cz,sx,sz):return exp(-((x-cx)/sx)**2-((z-cz)/sz)**2)
def front(x,z):
    w=interp(WIDTH,z);q=min(.9999,abs(x)/w)
    y=interp(FRONT,z)+.76*(w/.99)*(1-sqrt(1-q*q))
    # Distinct orbital hollows, zygomatic planes and stern brow.
    for s in [-1,1]:
        y+=.11*g(x,z,s*.46,6.84,.30,.19)
        y-=.245*g(x,z,s*.40,7.025,.35,.12)
        y-=.18*g(x,z,s*.66,6.56,.25,.17)
        y+=.12*g(x,z,s*.68,6.13,.23,.24)
        y-=.045*g(x,z,s*.73,5.72,.20,.21)
        y-=.038*g(x,z,s*.22,6.11,.15,.22)
        # Nasolabial valley.
        y+=.027*g(x,z,s*(.26+(6.26-z)*.3),6.10,.034,.20)
    # Integrated bridge, tip and alar wings.
    y-=.285*g(x,z,0,6.74,.135,.39)
    y-=.42*g(x,z,0,6.36,.17,.15)
    y-=.135*g(x,z,.18,6.305,.11,.10)+.135*g(x,z,-.18,6.305,.11,.10)
    y+=.040*g(x,z,0,6.22,.048,.09)
    y-=.065*g(x,z,0,6.05,.31,.10)
    y-=.075*g(x,z,0,5.88,.30,.06)
    y+=.05*g(x,z,0,5.765,.32,.06)
    y-=.15*g(x,z,0,5.58,.39,.16)
    # Quiet asymmetry: old brow furrow and a small cheek scar.
    y+=.023*g(x,z,.14,7.16,.024,.16)+.014*g(x,z,-.11,7.2,.025,.11)
    y+=.018*g(x,z,-.65+(z-6.5)*.18,6.50,.018,.19)
    return y

def headpoint(theta,z):
    w=interp(WIDTH,z);x=w*sin(theta)
    if cos(theta)>=0:y=front(x,z)
    else:y=front(w*.9999,z)+(1.0 if z<7.7 else .8)*(w/.99)*(-cos(theta))
    return (x,y,z)

# Continuous sculpted head surface. Apertures are masked behind modeled eyelids.
V=[];F=[];N=256;NZ=256
for j in range(NZ+1):
    z=5.19+(8.17-5.19)*j/NZ
    for i in range(N):V.append(headpoint(2*pi*i/N,z))
for j in range(NZ):
    for i in range(N):
        theta=2*pi*(i+.5)/N;z=5.19+(8.17-5.19)*(j+.5)/NZ;x=interp(WIDTH,z)*sin(theta)
        dx=(abs(x)-.46)/.267;dz=(z-6.84)/.048
        if cos(theta)>.7 and abs(dx)<1 and abs(dz)<max(0,1-dx*dx)**.55:continue
        a=j*N+i;b=j*N+(i+1)%N;F.append((a,b,b+N,a+N))
F.append(tuple(reversed(range(N))));F.append(tuple(NZ*N+i for i in range(N)))
head=mesh('Sculpt | continuous cranium face jaw',V,F,skin)
headmat=skin.copy();headmat.name='Skin and feathered beard roots';head.data.materials[0]=headmat
attr=head.data.color_attributes.new(name='Complexion',type='FLOAT_COLOR',domain='POINT')
for i,v in enumerate(V):
    x,y,z=v;ax=abs(x);top=5.89+.64*(min(1,ax/.87)**1.7)
    beard=max(0,min(1,(top-z)/.11)) if y<.25 else 0
    mouth=max(0,min(1,(ax-.32)/.14))+max(0,min(1,(5.79-z)/.14))
    beard*=min(1,mouth)
    c=tuple((1-beard)*a+beard*b for a,b in zip((.32,.175,.103),(.065,.048,.034)))
    attr.data[i].color=(*c,1)
n=headmat.node_tree.nodes.new('ShaderNodeVertexColor');n.layer_name='Complexion'
headmat.node_tree.links.new(n.outputs['Color'],headmat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])

# Neck with sternomastoid relief, joined shoulder base hidden inside armor.
V=[];F=[]
for j in range(65):
    t=j/64;z=3.93+1.62*t;w=.90-.32*t
    for i in range(96):
        a=2*pi*i/96;x=w*sin(a);y=.24-.68*cos(a)
        if cos(a)>0:
            y-=.10*exp(-((abs(x)-(.29+.30*(1-t)))/.11)**2)*sin(pi*t)
            y-=.045*exp(-(x/.14)**2)*exp(-((z-4.97)/.17)**2)
        V.append((x,y,z))
for j in range(64):
    for i in range(96):a=j*96+i;b=j*96+(i+1)%96;F.append((a,b,b+96,a+96))
mesh('Sculpt | neck and sternomastoid',V,F,skin)

# Eye surfaces sit physically behind almond lids; small hazel irises.
for s in [-1,1]:
    cx=s*.46;cz=6.84;cy=-.445;r=.287
    ev=[];ef=[]
    for jj in range(17):
        v=-1+2*jj/16
        for ii in range(65):
            u=-1+2*ii/64;xx=cx+.260*u;zz=cz+.039*v*max(0,1-u*u)**.60+s*u*.021
            yy=cy-sqrt(max(.001,r*r-(xx-cx)**2-(zz-cz)**2))
            ev.append((xx,yy,zz))
    for jj in range(16):
        for ii in range(64):
            aa=jj*65+ii;ef.append((aa,aa+1,aa+66,aa+65))
    mesh('Visible spherical sclera',ev,ef,white)
    # Iris surface also follows the aperture; lids occlude its superior/inferior arcs.
    iv=[];ip=[];pv=[];pf=[]
    for jj in range(17):
        v=-1+2*jj/16
        for ii in range(33):
            u=-1+2*ii/32;xx=cx+.067*u;zz=cz+.041*v*sqrt(max(0,1-u*u))
            yy=cy-sqrt(max(.001,r*r-(xx-cx)**2-(zz-cz)**2))-.003
            iv.append((xx,yy,zz))
    for jj in range(16):
        for ii in range(32):aa=jj*33+ii;ip.append((aa,aa+1,aa+34,aa+33))
    mesh('Iris under heavy eyelids',iv,ip,iris)
    ell('Pupil', (cx,-.736,cz),(.020,.003,.022),pupil)
    # Broad eyelid patches transition into sockets. Upper lid overhangs iris.
    for upper in [True,False]:
        vv=[];ff=[]
        for j in range(7):
            t=j/6
            for i in range(65):
                u=-1+2*i/64;x=cx+u*(.262+.065*t)
                bow=max(0,1-u*u)**.65
                z=cz+((.043 if upper else -.038)*bow)+(.092 if upper else -.107)*t*bow+s*u*.021
                innery=cy-sqrt(max(.005,r*r-(u*.262)**2-(z-cz)**2))-.008
                y=innery*(1-t)+front(x,z)*t-.01*sin(pi*t)
                vv.append((x,y,z))
        for j in range(6):
            for i in range(64):a=j*65+i;ff.append((a,a+1,a+66,a+65))
        mesh('Upper lid' if upper else 'Lower lid',vv,ff,skin,1)
    # Tear duct and a restrained lid crease.
    ell('Tear corner',(cx-s*.248,-.584,cz-s*.014),(.024,.021,.017),lip,24,16)
    pts=[]
    for k in range(11):
        u=-.91+1.82*k/10;x=cx+u*.29;z=cz+.117*sqrt(max(0,1-u*u))+.030
        pts.append((x,front(x,z)-.009,z))
    curve('Upper eyelid crease',pts,.007,crease,[.1,.4,.6,.7,.8,.8,.7,.6,.5,.3,.1])
    # Fine lower orbital wrinkles, skin edge plus shadow rather than painted eyes.
    for q in range(1):
        pts=[]
        for k in range(9):
            u=-.7+1.55*k/8;x=cx+u*.30;z=cz-.12-.030*q-.025*(1-u*u)
            pts.append((x,front(x,z)-.006,z))
        curve('Subtle orbital fold',pts,.003,lip,[.1,.4,.6,.8,.8,.7,.6,.3,.1])
    # Nostril crevice under each alar wing.
    x=s*.151;z=6.262
    ell('Nostril',(x,front(x,z)-.006,z),(.071,.008,.024),crease,32,16)

# Lips made as a single shaped pair, tucked into the surrounding muzzle.
for upper in [True,False]:
    V=[];F=[]
    for j in range(9):
        t=j/8
        for i in range(81):
            u=-1+2*i/80;x=.35*u
            seam=5.972+.013*cos(u*pi)-.015*exp(-(u/.22)**2)
            z=seam+(1 if upper else -1)*(.064 if upper else .072)*sin(pi*(u+1)/2)**.7*sin(t*pi/2)
            y=front(x,z)-.012-.026*sin(pi*t)*(1-u*u)
            V.append((x,y,z))
    for j in range(8):
        for i in range(80):a=j*81+i;F.append((a,a+1,a+82,a+81))
    mesh('Upper sculpted lip' if upper else 'Lower sculpted lip',V,F,lip)
pts=[]
for i in range(17):
    x=-.36+.72*i/16;u=x/.36;z=5.972+.013*cos(u*pi)-.015*exp(-(u/.22)**2);pts.append((x,front(x,z)-.022,z))
curve('Closed mouth',pts,.008,crease,[.1]+[.6]*15+[.1])

# Ear bowls with helix and antihelix, modeled rather than a sphere on each side.
for s in [-1,1]:
    vv=[];ff=[]
    for j in range(13):
        r=j/12
        for i in range(64):
            a=2*pi*i/64;z=6.62+.36*r*cos(a);x=s*(.94+.235*r*sin(a)+.085*r)
            y=-.055-.09*sin(pi*r)+.018*cos(a)
            vv.append((x,y,z))
    for j in range(12):
        for i in range(64):a=j*64+i;b=j*64+(i+1)%64;ff.append((a,b,b+64,a+64))
    mesh('Ear concha',vv,ff,skin,1)
    pts=[(s*(.94+.235*sin(a)+.085),-.074,6.62+.36*cos(a)) for a in [2*pi*i/24 for i in range(25)]]
    curve('Rolled ear helix',pts,.044,skin)
    curve('Ear antihelix',[(s*.99,-.155,6.42),(s*1.065,-.16,6.64),(s*1.08,-.125,6.83)],.027,skin,[.7,1,.5])
    ell('Tragus',(s*.97,-.20,6.56),(.055,.055,.105),skin)
for o in list(ACTIVE.objects):
    if any(word in o.name for word in ['Ear concha','Rolled ear helix','Ear antihelix','Tragus']):
        # Set transform about the auricular attachment instead of enlarging the skull.
        s=1 if ('Tragus' in o.name and o.location.x>0) else -1 if 'Tragus' in o.name else 1
        if o.type=='MESH' and 'Tragus' not in o.name:s=1 if sum(v.co.x for v in o.data.vertices)>0 else -1
        if o.type=='CURVE':s=1 if sum(p.co.x for sp in o.data.splines for p in sp.bezier_points)>0 else -1
        pivot=Vector((s*.94,0,6.62))
        o.location=pivot+(o.location-pivot)*.79;o.scale*=.79

ACTIVE=COL['02 Groom']
# Swept volumetric hair locks. An elliptical cross-section has subtle longitudinal furrows.
def catmull(pts,t):
    n=len(pts)-1;q=t*n;i=min(n-1,int(q));u=q-i
    p0=Vector(pts[max(0,i-1)]);p1=Vector(pts[i]);p2=Vector(pts[i+1]);p3=Vector(pts[min(n,i+2)])
    return .5*((2*p1)+(-p0+p2)*u+(2*p0-5*p1+4*p2-p3)*u*u+(-p0+3*p1-3*p2+p3)*u*u*u)

def lock(name,pts,width,depth,mat,detail=True):
    v=[];f=[];ns=38;nr=14
    for j in range(ns+1):
        t=j/ns;p=catmull(pts,t);tan=(catmull(pts,min(1,t+.01))-catmull(pts,max(0,t-.01))).normalized()
        side=tan.cross(Vector((0,-1,.3))).normalized()
        if side.length<.1:side=Vector((1,0,0))
        up=side.cross(tan).normalized()
        taper=(.05+.95*sin(pi*min(.98,t+.01))**.40)*(1-.97*t**8)
        for k in range(nr):
            a=2*pi*k/nr;groove=1+.045*cos(7*a+1.2*sin(t*4))
            v.append(tuple(p+side*(width*cos(a)*taper)+up*(depth*sin(a)*taper*groove)))
    for j in range(ns):
        for k in range(nr):a=j*nr+k;b=j*nr+(k+1)%nr;f.append((a,b,b+nr,a+nr))
    f.append(tuple(reversed(range(nr))));f.append(tuple(ns*nr+k for k in range(nr)))
    mesh(name,v,f,mat,1)
    if detail:
        for q in [-.5,.05,.55]:
            pts2=[]
            for j in range(15):
                t=.04+.91*j/14;p=catmull(pts,t);tan=(catmull(pts,min(1,t+.01))-catmull(pts,max(0,t-.01))).normalized();side=tan.cross(Vector((0,-1,.3))).normalized();up=side.cross(tan).normalized()
                taper=(.05+.95*sin(pi*min(.98,t+.01))**.40)*(1-.97*t**8)
                pts2.append(tuple(p+side*(width*q*taper)+up*(depth*sqrt(1-q*q)*taper*1.02)))
            curve('Longitudinal hair strand',pts2,.0020,hair,[.2]+[.8]*13+[.05])

# Matte irregular scalp underlayer under the swept clumps.
hv=[];hf=[];nr=100;nt=128
for j in range(nr+1):
    t=j/nr
    for i in range(nt):
        a=2*pi*i/nt;c=cos(a);bottom=7.42 if c>.65 else 6.90 if c>-.2 else 6.35
        bottom+=.045*sin(11*a);z=bottom+(8.16-bottom)*t;p=Vector(headpoint(a,z));p.x*=1.028;p.y=.13+(p.y-.13)*1.028
        hv.append(tuple(p))
for j in range(nr):
    for i in range(nt):a=j*nt+i;b=j*nt+(i+1)%nt;hf.append((a,b,b+nt,a+nt))
mesh('Dark scalp beneath parted waves',hv,hf,hair)
# Asymmetric swept waves with varying starts and heights.
for i in range(15):
    x=-.90+1.80*i/14+random.uniform(-.035,.035);a=abs(x)/.95;dz=random.uniform(-.055,.08);s=1 if x>.05 else -1
    pts=[(x,-.65+.24*a,7.41-.20*a+dz),(x+s*.04,-.62,7.69-.05*a+dz),(x+s*.16,-.29,7.96-.12*a+dz),(x+s*.23,.23,8.04-.14*a+dz),(x+s*.18,.75,7.85-.20*a),(x+s*.04,1.04,7.51-.24*a)]
    lock('Swept crown wave %02d'%i,pts,.155,.078,hair if i%5 else hair2)
for s in [-1,1]:
    for i in range(7):
        t=i/6;z=7.50-.67*t
        pts=[(s*(.92+.05*t),-.25,z),(s*(1.025+.05*t),.07,z+.11),(s*(1.04+.04*t),.48,z+.07),(s*.98,.90,z-.15),(s*(.81-.10*t),1.02,z-.45)]
        lock('Temple swept curl',pts,.155,.032,hair,True)
    for i in range(6):
        z=7.15-i*.13;pts=[(s*.90,.75,z),(s*1.0,1.03,z-.12),(s*.84,1.13,z-.37),(s*.62,1.00,z-.48)]
        lock('Nape wave',pts,.13,.07,hair)
    # Sideburns.
    for i in range(180):
        x=s*random.uniform(.84,.94);z=random.uniform(6.37,6.96)
        if abs(x)>=interp(WIDTH,z):continue
        curve('Fine sideburn hair',[(x,front(x,z)-.022,z),(x*.997,front(x*.997,z-.035)-.025,z-.035)],.003,hair,[1,.05])
# A few unruly forelocks; asymmetry breaks the slick helmet silhouette.
lock('Loose forelock',[(.02,.34,8.07),(.18,-.18,8.06),(.45,-.63,7.87),(.57,-.85,7.57),(.49,-.95,7.29)],.088,.044,hair)
lock('Left loose wave',[(-.42,.16,8.01),(-.70,-.20,7.90),(-.91,-.56,7.62),(-.91,-.56,7.25)],.088,.044,hair)

# Brows made of small tapered curved hairs in the brow ridge, never a flat black sticker.
for s in [-1,1]:
    for i in range(115):
        t=random.random();x=s*(.18+.59*t);z=7.003+.085*sin(pi*t/2)-.022*t+random.uniform(-.025,.025)
        pts=[]
        for j in range(4):
            u=j/3;xx=x+s*.072*u;zz=z+.023*sin(u*pi)-.007*u;pts.append((xx,front(xx,zz)-.016,zz))
        curve('Brow strand',pts,random.uniform(.0035,.006),hair,[.8,1,.7,.05])

# Short salt-and-pepper beard built from surface-following tapered hairs.
# The jaw stays readable, and irregular feathered cheeks avoid a hard mask line.
for i in range(10500):
    z=random.uniform(5.28,6.61);w=interp(WIDTH,z);x=random.uniform(-w*.99,w*.99);ax=abs(x)
    top=5.89+.64*(min(1,ax/.87)**1.7)
    if z>top+random.uniform(-.055,.045):continue
    if ax<.40 and z>5.73:continue
    if ax<.14 and z>5.66:continue
    length=random.uniform(.022,.046)*(1+.25*(5.9-z));pts=[]
    for j in range(4):
        t=j/3;zz=z-length*t;xx=x*(1-.012*t);yy=front(xx,zz)-.012-.012*sin(pi*t)-.008*t
        pts.append((xx,yy,zz))
    m=gray if random.random()<(.13+.18*(z<5.6)) else hair if random.random()<.8 else hair2
    curve('Short beard fibre',pts,random.uniform(.002,.004),m,[.85,1,.55,.01])
# Moustache follows the muzzle with split philtrum and irregular fine ends.
for s in [-1,1]:
    for i in range(260):
        x=s*random.uniform(.035,.34);z=random.uniform(6.025,6.135)-.11*abs(x);pts=[]
        for j in range(4):
            t=j/3;xx=x+s*.050*t;zz=z-.035*t;pts.append((xx,front(xx,zz)-.015-.020*sin(pi*t),zz))
        curve('Moustache fibre',pts,random.uniform(.002,.0037),hair2 if i%7==0 else hair,[.5,1,.7,.05])

ACTIVE=COL['03 Forged cuirass']
def chest(x,z):
    return -.89-.28*(1-(x/1.9)**2)-.19*exp(-((z-3.32)/.92)**2)-.12*exp(-(x/.22)**2)-.16*g(abs(x),z,.82,3.63,.63,.52)
def armor_x(x,z):return x*(.78+.22*max(0,min(1,(z-1.33)/3.3))**.75)
def topedge(x):return 4.02+.62*(abs(x)/1.86)**.65
def botedge(x):return 1.33+.24*(abs(x)/1.86)
# Two forged halves with a central keel and generous compound curvature.
for s in [-1,1]:
    v=[];f=[]
    for j in range(65):
        t=j/64
        for i in range(65):
            u=i/64;x=s*(.025+1.835*u);z=botedge(x)+(topedge(x)-botedge(x))*t
            y=chest(x,z)+.035*sin(pi*u)*sin(pi*t)
            v.append((armor_x(x,z),y,z))
    for j in range(64):
        for i in range(64):
            a=j*65+i;face=(a,a+1,a+66,a+65);f.append(face if s>0 else tuple(reversed(face)))
    o=mesh('Forged breastplate half',v,f,steel)
    md=o.modifiers.new('Forged wall thickness','SOLIDIFY');md.thickness=.12
    md=o.modifiers.new('Soft hammered edges','BEVEL');md.width=.022;md.segments=3

def relief(name,pts,width,mat,raised=.026):
    # Tapered sculpted metal ribbon with actual raised cross section.
    v=[];f=[];n=80
    for j in range(n+1):
        t=j/n;p=catmull([(x,0,z) for x,z in pts],t);a=catmull([(x,0,z) for x,z in pts],max(0,t-.005));b=catmull([(x,0,z) for x,z in pts],min(1,t+.005));d=(b-a).normalized();perp=Vector((-d.z,0,d.x));w=width*(.30+.70*sin(pi*t)**.55)*(1-.9*t**14)
        for k in range(5):
            q=-1+2*k/4;pp=p+perp*(w*q);v.append((armor_x(pp.x,pp.z),chest(pp.x,pp.z)-raised*(1-q*q)-.025,pp.z))
    for j in range(n):
        for k in range(4):a=j*5+k;f.append((a,a+1,a+6,a+5))
    o=mesh(name,v,f,mat);md=o.modifiers.new('Relief thickness','SOLIDIFY');md.thickness=.008
    return o

for s in [-1,1]:
    # Substantial borders and swept botanical motifs, independent original ornament.
    relief('Upper rolled bronze border',[(s*.02,4.02),(s*.68,4.34),(s*1.25,4.5),(s*1.86,4.64)],.071,edge,.032)
    relief('Lower bronze border',[(s*.03,1.33),(s*.68,1.41),(s*1.35,1.49),(s*1.86,1.57)],.066,edge)
    relief('Outer folded border',[(s*1.82,1.6),(s*1.87,2.5),(s*1.85,3.5),(s*1.86,4.63)],.062,edge)
    relief('Central branching stem',[(s*.03,1.65),(s*.12,2.45),(s*.22,3.28),(s*.32,4.13)],.062,edge,.052)
    for pts,w in [([(.10,2.0),(.40,2.65),(.98,2.95),(1.49,2.88)],.056),([(.13,2.52),(.46,3.23),(1.06,3.60),(1.50,3.62)],.079),([(.22,3.09),(.55,3.73),(.87,3.93),(1.12,4.12)],.081)]:
        relief('Swept bronze leaf rib',[(s*x,z) for x,z in pts],w,edge,.032)
    relief('Lower engraved rib',[(s*.29,1.48),(s*.59,1.87),(s*1.11,2.18),(s*1.66,2.21)],.031,goldDark,.009)
    for z in [1.83,2.24,2.72,3.21,3.73,4.19]:
        x=s*1.76;ell('Peened bronze rivet',(armor_x(x,z),chest(x,z)-.025,z),(.040,.018,.040),edge,20,12)

# Curved back and sides of the armor make the breastplate an enclosing cuirass.
vv=[];ff=[]
for j in range(49):
    t=j/48;z=1.43+2.90*t;w=1.46+.37*t
    for i in range(97):
        a=-pi/2+pi*i/96;x=w*sin(a);y=.18+1.01*cos(a)
        vv.append((x,y,z+.14*abs(sin(a))))
for j in range(48):
    for i in range(96):a=j*97+i;ff.append((a,a+1,a+98,a+97))
o=mesh('Cuirass back and curved flanks',vv,ff,steel);md=o.modifiers.new('Backplate metal','SOLIDIFY');md.thickness=.10

# Broad overlapping pauldrons, each a curved plate with a thicker gilded perimeter.
def shoulder(s,layer):
    vv=[];ff=[];nu=64;nv=40
    for j in range(nv+1):
        v=j/nv
        for i in range(nu+1):
            u=i/nu;theta=-1.02+2.04*u
            x=s*(1.48+layer*.19+v*(1.33-.09*layer))
            y=.14+(1.17-.20*v)*sin(theta)
            z=4.45+.69*sin(pi*(.18+.74*v))-.94*v-layer*.30+.20*cos(theta)
            # Raised forged center flute.
            z+=.105*exp(-(theta/.25)**2)*sin(pi*v)
            vv.append((x,y,z))
    for j in range(nv):
        for i in range(nu):a=j*(nu+1)+i;ff.append((a,a+1,a+nu+2,a+nu+1))
    o=mesh('Pauldron | overlapping forged plate',vv,ff,steel)
    md=o.modifiers.new('Thick folded plate','SOLIDIFY');md.thickness=.10
    md=o.modifiers.new('Forged softened edge','BEVEL');md.width=.025;md.segments=3
    # Flat broad bronze bands share surface and silhouette, not a pipe outline.
    for which in ['front','outer','rear']:
        bv=[];bf=[]
        n=80
        for k in range(n+1):
            t=k/n
            for q in [0,1]:
                if which=='outer':v=1-.10*q;theta=-1.02+2.04*t
                else:v=t;theta=(-1 if which=='front' else 1)*(1.025-.17*q)
                x=s*(1.48+layer*.19+v*(1.33-.09*layer));y=.14+(1.17-.20*v)*sin(theta)
                if which=='front':y-=.025
                if which=='rear':y+=.025
                z=4.45+.69*sin(pi*(.18+.74*v))-.94*v-layer*.30+.20*cos(theta)+.105*exp(-(theta/.25)**2)*sin(pi*v)+.015
                bv.append((x,y,z))
        for k in range(n):a=k*2;bf.append((a,a+1,a+3,a+2))
        b=mesh('Pauldron | broad bronze edge',bv,bf,edge);md=b.modifiers.new('Bronze folded rim','SOLIDIFY');md.thickness=.024
    # Small inset fluted lames under the front band.
    for k in range(5):
        v=.20+.15*k;theta=-1.05;x=s*(1.48+layer*.19+v*(1.33-.09*layer));y=.14+(1.17-.20*v)*sin(theta)-.018;z=4.45+.69*sin(pi*(.18+.74*v))-.94*v-layer*.30+.20*cos(theta)
        ell('Pauldron perimeter rivet',(x,y,z),(.035,.025,.035),edge,20,12)
for s in [-1,1]:
    for layer in [2,1,0]:shoulder(s,layer)

# Upright sword-breaker collars behind the shoulders.
for s in [-1,1]:
    vv=[];ff=[]
    for j in range(31):
        t=j/30
        for i in range(31):
            u=i/30;x=s*(1.14+.34*u+.25*sin(pi*t));y=-.84+1.7*t;z=4.38+.77*sin(pi*t)**.5+.25*u
            vv.append((x,y,z))
    for j in range(30):
        for i in range(30):a=j*31+i;ff.append((a,a+1,a+32,a+31))
    o=mesh('Upright gorget wing',vv,ff,steel);md=o.modifiers.new('Gorget wall','SOLIDIFY');md.thickness=.08
    curve('Gorget forged lip',[(s*(1.48+.25*sin(pi*t)),-.84+1.7*t,4.63+.77*sin(pi*t)**.5) for t in [i/30 for i in range(31)]],.040,edge)

# Round fasteners: convex chased bronze medallion with restrained original radial motif.
for s in [-1,1]:
    cx=s*1.47;cy=-1.13;cz=4.18
    ell('Shoulder brooch base',(cx,cy,cz),(.28,.095,.28),goldDark)
    pts=[(cx+.267*cos(t),cy-.074,cz+.267*sin(t)) for t in [2*pi*i/48 for i in range(49)]];curve('Chased brooch rim',pts,.022,edge)
    for k in range(8):
        a=2*pi*k/8;pts=[(cx+.055*cos(a),cy-.110,cz+.055*sin(a)),(cx+.16*cos(a+.12),cy-.098,cz+.16*sin(a+.12)),(cx+.227*cos(a),cy-.078,cz+.227*sin(a))]
        curve('Chased brooch petal',pts,.023,edge,[.6,1,.05])
    ell('Brooch central boss',(cx,cy-.113,cz),(.062,.018,.062),edge,32,16)

ACTIVE=COL['04 Mantle and collar']
# The dark leather arming collar supports the neck and armor.
V=[];F=[]
for j in range(26):
    t=j/25
    for i in range(96):
        a=2*pi*i/96;r=.83+.15*(1-t);V.append((r*sin(a),.20-.80*cos(a),4.09+.61*t+.12*cos(a)))
for j in range(25):
    for i in range(96):a=j*96+i;b=j*96+(i+1)%96;F.append((a,b,b+96,a+96))
o=mesh('Leather arming collar',V,F,leather);md=o.modifiers.new('Collar thickness','SOLIDIFY');md.thickness=.055
# Cloth cowl as an irregular draped annulus with broad gathered folds.
V=[];F=[]
for j in range(37):
    t=j/36
    for i in range(160):
        a=2*pi*i/160;r=.85+.65*t
        x=r*sin(a);y=.21-(.74+.35*t)*cos(a)
        z=4.59-.41*t+.075*sin(3*a+6*t)+.040*sin(9*a-4*t)+.10*sin(a)
        V.append((x,y,z))
for j in range(36):
    for i in range(160):a=j*160+i;b=j*160+(i+1)%160;F.append((a,b,b+160,a+160))
o=mesh('Olive cowl | woven broad folds',V,F,cloth,1);md=o.modifiers.new('Fabric body','SOLIDIFY');md.thickness=.04
# Rear cape gives the bust a finished silhouette and reveals shoulder separation.
V=[];F=[]
for j in range(60):
    t=j/59
    for i in range(80):
        u=-1+2*i/79;x=(1.65+.78*t)*u;y=.56+.52*sin(pi*(u+1)/2)+.15*sin(u*19+1.5*t)*t;z=4.48-3.10*t+.11*cos(u*12)*t
        V.append((x,y,z))
for j in range(59):
    for i in range(79):a=j*80+i;F.append((a,a+1,a+81,a+80))
o=mesh('Heavy mantle drape',V,F,cloth,1);md=o.modifiers.new('Wool cloth body','SOLIDIFY');md.thickness=.04

ACTIVE=COL['05 Studio']
stone=material('Display plinth charcoal stone',(.025,.029,.032),rough=.64,noise=.3)
def cylinder(name,r,depth,z,mat):
    bpy.ops.mesh.primitive_cylinder_add(vertices=96,radius=r,depth=depth,location=(0,0,z));o=bpy.context.object;o.name=name
    for c in list(o.users_collection):c.objects.unlink(o)
    ACTIVE.objects.link(o);o.data.materials.append(mat);m=o.modifiers.new('Chamfered plinth','BEVEL');m.width=.07;m.segments=3
    for p in o.data.polygons:p.use_smooth=True
    return o
cylinder('Stone bust plinth',1.58,.32,1.04,stone);cylinder('Bronze plinth foot',1.64,.085,.86,goldDark)
floor=mesh('Studio ground',[(-200,-200,.80),(200,-200,.80),(200,200,.80),(-200,200,.80)],[(0,1,2,3)],material('Studio ground',(.047,.053,.058),rough=.8))

def point(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
def area(n,loc,energy,color,size):
    d=bpy.data.lights.new(n,'AREA');d.energy=energy;d.color=color;d.shape='DISK';d.size=size;o=bpy.data.objects.new(n,d);ACTIVE.objects.link(o);o.location=loc;point(o,(0,0,5));return o
area('Large warm key',(-6,-9,12),2100,(1,.94,.86),5)
area('Cool portrait fill',(5,-6,8),650,(.80,.86,1),4)
area('Shoulder rim',(3,4,10),2600,(.91,.94,1),4)
area('Face bounce',(-1,-5,5.5),120,(1,.77,.58),2)
scene=bpy.context.scene;scene.world=bpy.data.worlds.new('Dark studio');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.075,.09,.12,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.24
d=bpy.data.cameras.new('Portrait camera');cam=bpy.data.objects.new('Portrait camera',d);ACTIVE.objects.link(cam);scene.camera=cam;d.type='ORTHO';d.clip_end=500
scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
prefs=bpy.context.preferences.addons['cycles'].preferences
try:
    prefs.compute_device_type='OPTIX';prefs.get_devices_for_type('OPTIX')
    for d in prefs.devices:d.use=d.type=='OPTIX'
    scene.cycles.device='GPU'
except Exception:scene.cycles.device='CPU'
scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG';scene.render.resolution_percentage=100
scene.cycles.max_bounces=8
def view(name,loc,target,scale,res,samples):
    cam.location=loc;point(cam,target);cam.data.ortho_scale=scale;scene.render.resolution_x=res[0];scene.render.resolution_y=res[1];scene.cycles.samples=samples;scene.render.filepath=str(OUT/'previews'/f'{name}.png')
for cname in ['01 Sculpt and eyes','02 Groom']:
    for o in COL[cname].objects:o.location.z-=.25
view('head-draft',(3.4,-11,6.95),(0,-.04,6.45),3.9,(900,1100),32)
# Collapse repeated endpoints at ear, eye and lip poles. Keep open sculpt patches.
import bmesh
for o in bpy.data.objects:
    if o.type!='MESH':continue
    bm=bmesh.new();bm.from_mesh(o.data)
    bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
    dead=[f for f in bm.faces if f.calc_area()<1e-12]
    if dead:bmesh.ops.delete(bm,geom=dead,context='FACES_ONLY')
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free();o.data.update()
view('beauty',(5.5,-15,8.0),(0,0,4.5),8.35,(1400,1600),80)
for screen in bpy.data.screens:
    for a in screen.areas:
        if a.type=='VIEW_3D':
            sp=a.spaces.active;sp.region_3d.view_distance=11;sp.region_3d.view_location=Vector((0,0,4.5));sp.region_3d.view_rotation=cam.rotation_euler.to_quaternion();sp.region_3d.view_perspective='ORTHO';sp.shading.type='MATERIAL';sp.overlay.show_overlays=False
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'astra-bust.blend'))
view('head-draft',(3.4,-11,6.95),(0,-.04,6.45),3.9,(900,1100),32)
bpy.ops.render.render(write_still=True)
print('ASTRA_BUST_SAVED',str(OUT/'astra-bust.blend'),flush=True)

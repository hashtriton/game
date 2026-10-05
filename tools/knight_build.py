"""Create the new original knight through blockout and detailed stages.

Run only in a separate background Blender. Existing hero scenes stay untouched.
"""
import argparse
import json
import math
from pathlib import Path
import sys

import bpy
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'art/heroes/knight-v3'
sys.path.insert(0,str(ROOT/'tools'))
import knight_shapes as S


def material(name, color, metal=0, rough=.5, noise=0):
    mat=bpy.data.materials.new(name)
    mat.diffuse_color=(*color,1)
    mat.use_nodes=True
    nt=mat.node_tree
    bs=nt.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Metallic'].default_value=metal
    bs.inputs['Roughness'].default_value=rough
    coarse=nt.nodes.new('ShaderNodeTexNoise')
    coarse.inputs['Scale'].default_value=5.5
    coarse.inputs['Detail'].default_value=3
    color_ramp=nt.nodes.new('ShaderNodeValToRGB')
    color_ramp.color_ramp.elements[0].position=.15
    color_ramp.color_ramp.elements[0].color=(*(c*.90 for c in color),1)
    color_ramp.color_ramp.elements[1].position=.85
    color_ramp.color_ramp.elements[1].color=(*(c*1.04 for c in color),1)
    nt.links.new(coarse.outputs['Fac'],color_ramp.inputs[0])
    nt.links.new(color_ramp.outputs['Color'],bs.inputs['Base Color'])
    rough_ramp=nt.nodes.new('ShaderNodeValToRGB')
    for element,value in zip(rough_ramp.color_ramp.elements,[max(.05,rough-.025),min(.98,rough+.10)]):
        element.color=(value,value,value,1)
    nt.links.new(coarse.outputs['Fac'],rough_ramp.inputs[0])
    nt.links.new(rough_ramp.outputs['Color'],bs.inputs['Roughness'])
    if noise:
        tex=nt.nodes.new('ShaderNodeTexNoise')
        tex.inputs['Scale'].default_value=165
        tex.inputs['Detail'].default_value=2
        bump=nt.nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value=.20
        bump.inputs['Distance'].default_value=noise
        nt.links.new(tex.outputs['Fac'],bump.inputs['Height'])
        nt.links.new(bump.outputs['Normal'],bs.inputs['Normal'])
    return mat


def collection(name):
    col=bpy.data.collections.new(name)
    KNIGHT.children.link(col)
    return col


def shoulder(name, sign, layer, col, detailed):
    # One continuous crown with a swept inner ridge, followed by short sliding lames.
    big=sign>0
    cx=sign*(.370 if big else .356)+sign*layer*.027
    cy=.014+layer*.018
    zc=(1.628 if big else 1.604)-layer*.061
    rx=(.228 if big else .188)-layer*.016
    ry=(.236 if big else .195)-layer*.006
    nu,nv=48,12
    def pos(r,phi):
        outward=max(0,sign*math.cos(phi))
        inward=max(0,-sign*math.cos(phi))
        front=max(0,-math.sin(phi))
        if layer==0:
            rise=(.188 if big else .158)*(1-r*r)**.72
            crest=(.102 if big else .024)*math.exp(-((r-.73)/.23)**2)*inward**7
            z=zc+rise+crest-.075*r*outward-.025*r*front
        else:
            z=zc+.25*(1-r)-.055*r*outward-.033*r*front
        x=cx+rx*r*math.cos(phi)*(1-.055*front)
        y=cy+ry*r*math.sin(phi)*(1+.08*outward)
        return x,y,z
    if layer==0:
        vertices=[pos(0,0)]
        for j in range(1,nv+1):
            vertices.extend(pos(j/nv,2*math.pi*i/nu) for i in range(nu))
        faces=[(0,1+i,1+(i+1)%nu) for i in range(nu)]
        for j in range(nv-1):
            a=1+j*nu
            faces.extend((a+i,a+(i+1)%nu,a+nu+(i+1)%nu,a+nu+i) for i in range(nu))
    else:
        vertices=[pos(.62+.38*j/nv,2*math.pi*i/nu) for j in range(nv+1) for i in range(nu)]
        faces=[(j*nu+i,j*nu+(i+1)%nu,(j+1)*nu+(i+1)%nu,(j+1)*nu+i)
               for j in range(nv) for i in range(nu)]
    ob=S.solid_patch(name,vertices,faces,M['steel'],col,.017,.002)
    if detailed:
        S.tube(name+' rolled bronze hem',vertices[-nu:],.008,M['bronze'],col,True,8)
        if layer==0:
            # Radial embossing follows the shoulder surface, without crowded glyphs.
            for phi in [-1.75,-.82]:
                path=[]
                for j in range(12):
                    x,y,z=pos(.27+j*.061,phi if sign>0 else math.pi-phi)
                    path.append((x,y,z+.012))
                S.tube('Pauldron fluted ridge',path,.005,M['bronze'],col)
    return ob


CHEST=[(1.255,.220,.123,.025),(1.30,.240,.144,.025),
       (1.365,.277,.186,.025),(1.455,.322,.222,.018),
       (1.55,.335,.222,.020),(1.625,.300,.186,.015),
       (1.69,.239,.149,.020),(1.745,.154,.117,.020)]


def chest_front(x,z):
    for a,b in zip(CHEST,CHEST[1:]):
        if z<=b[0]:
            t=max(0,min(1,(z-a[0])/(b[0]-a[0])))
            w=a[1]*(1-t)+b[1]*t
            d=a[2]*(1-t)+b[2]*t
            cy=a[3]*(1-t)+b[3]*t
            return cy-d*max(.04,1-(x/w)**2)**.38-.024*(max(0,1-abs(x/w)))**7-.004
    return -.115


def chest_path(xz,offset=-.002):
    # Reproject every subdivision; straight chords between endpoints float over
    # the convex breastplate even when both endpoints touch its surface.
    result=[]
    for a,b in zip(xz,xz[1:]):
        for j in range(10):
            t=j/10
            x=a[0]*(1-t)+b[0]*t
            z=a[1]*(1-t)+b[1]*t
            result.append((x,chest_front(x,z)-offset,z))
    x,z=xz[-1]
    result.append((x,chest_front(x,z)-offset,z))
    return result


def body(detailed):
    col=collection('BODY')
    S.zloft('Quilted arming jacket',[(1.03,.205,.135,0,.035),(1.25,.198,.113,0,.035),
        (1.5,.3,.18,0,.03),(1.72,.275,.145,0,.04)],M['dark'],col)
    S.zloft('Padded continuous pelvis',[(.965,.207,.132,0,.04),(1.02,.250,.158,0,.04),
        (1.10,.244,.160,0,.035),(1.16,.216,.138,0,.035)],M['dark'],col)
    rings=[]
    for z,rx,ry,cy in CHEST:
        ring=[]
        for i in range(64):
            a=2*math.pi*i/64
            x=rx*math.cos(a)
            y=cy+ry*math.sin(a)
            if math.sin(a)<0:
                y=chest_front(x,z)+.004
            ring.append((x,y,z))
        rings.append(ring)
    S.loft('Sculpted cuirass',rings,M['steel'],col,True,.003)
    S.zloft('Protective neck collar',[(1.715,.144,.122,0,.019),(1.75,.135,.113,0,.02),
        (1.795,.121,.104,0,.024),(1.815,.120,.102,0,.024)],M['steel'],col,40,.002)
    S.tube('Gorget rim',S.oval_path((0,.024,1.812),(.120,.102)),.005,M['bronze'] if detailed else M['steel'],col,True)
    # Three articulated abdomen lames lead into the narrow waist.
    for j in range(3):
        z=1.14+j*.047
        S.zloft('Abdomen articulated lame '+str(j),[(z,.229+j*.003,.142,0,.025),
            (z+.042,.238+j*.003,.15,0,.025)],M['steel'],col,48,.002)
    S.zloft('Oxblood wrapped waist sash',[(1.045,.249,.158,0,.025),(1.09,.248,.16,0,.025),
        (1.14,.24,.16,0,.025)],M['wine'],col,48)
    S.zloft('Leather weapon belt',[(1.08,.259,.17,0,.024),(1.15,.252,.168,0,.024)],M['leather'],col,48,.002)
    if detailed:
        # Raised center keel and asymmetric structural motifs follow the volume.
        # The center keel is part of the cuirass surface; restrained channels
        # flank it instead of an unrelated bright shield laid over the chest.
        for sign in [-1,1]:
            S.tube('Cuirass central engraving',chest_path([(sign*.016,1.295),
                (sign*.033,1.42),(sign*.045,1.53),(sign*.065,1.625)]),.004,M['bronze'],col)
        for sign in [-1,1]:
            paths=[[(sign*.015,1.29),(sign*.08,1.37),(sign*.13,1.40),(sign*.215,1.41)],
                   [(sign*.04,1.45),(sign*.09,1.52),(sign*.16,1.55),(sign*.235,1.55)],
                   [(sign*.07,1.61),(sign*.12,1.665),(sign*.20,1.67)]]
            for idx,path in enumerate(paths):
                S.tube('Cuirass sweeping inlay',chest_path(path),.0055 if idx==1 else .004,M['bronze'],col)
            S.tube('Chest shoulder border',chest_path([(sign*.26,1.67),
                (sign*.28,1.54),(sign*.255,1.39),(sign*.204,1.27)]),.007,M['bronze'],col)
        for x in [-.20,-.10,.10,.20]:
            S.rivet('Belt anchor', (x,-.16*math.sqrt(1-(x/.27)**2)-.004,1.12),M['bronze'],col,.009)
        # Buckle, layered diamond instead of a borrowed emblem.
        buckle=[(0,-.174,1.177),(-.064,-.174,1.127),(0,-.174,1.07),(.064,-.174,1.127)]
        S.plate('Belt escutcheon',buckle,M['bronze'],col,.018,.025,.003)
        S.plate('Belt inset steel',[(0,-.205,1.163),(-.041,-.205,1.127),(0,-.205,1.09),(.041,-.205,1.127)],M['steel'],col,.012,.008,.002)
    return col


def arms(detailed):
    col=collection('ARMS')
    for sign in [-1,1]:
        upper=(sign*.352,.015,1.61)
        elbow=(sign*.455,-.005,1.305)
        wrist=(sign*.523,-.090,1.02)
        palm=(sign*.540,-.105,.935)
        S.limb('Upper sleeve',upper,elbow,[(0,.116,.112),(.40,.122,.12),(1,.095,.095)],M['mail'],col,32)
        elbow_support=S.limb('Elbow flexible leather',(sign*.431,.0,1.38),(sign*.49,-.04,1.20),
               [(0,.092,.091),(.4,.098,.092),(.75,.085,.080),(1,.08,.078)],M['leather'],col,32)
        bracer=S.limb('Tapered vambrace',(sign*.473,-.03,1.285),wrist,
               [(0,.118,.122),(.15,.119,.125),(.7,.087,.100),(1,.075,.087)],M['steel'],col,32,.003)
        # Shield-shaped elbow wings and reinforced front-facing bracer blades.
        for z,rx,rz,cy in [(1.29,.117,.10,-.13),(1.13,.095,.155,-.169)]:
            cx=sign*(.46 if z>1.2 else .501)
            outline=[(cx,cy,z+rz),(cx-rx,cy+.025,z+.025),
                     (cx-rx*.70,cy+.01,z-rz*.62),(cx,cy-.01,z-rz),
                     (cx+rx*.70,cy+.01,z-rz*.62),(cx+rx,cy+.025,z+.025)]
            panel=S.plate('Elbow fan' if z>1.2 else 'Bracer ridge plate',outline,M['steel'],col,.024,.018,.003,
                          support=elbow_support if z>1.2 else bracer)
            outline=S.plate_perimeter(panel)
            if detailed:
                S.tube('Arm plate bronze border',outline,.0055,M['bronze'],col,True)
        S.limb('Gauntlet cuff',(sign*.522,-.085,1.047),(sign*.535,-.1,.984),
               [(0,.090,.095),(1,.074,.077)],M['steel'],col,32,.003)
        S.limb('Leather palm',(sign*.535,-.126 if sign<0 else -.11,.997),
               (sign*.540,-.152 if sign<0 else -.105,.917),
               [(0,.063,.055),(.5,.073,.061),(1,.064,.059)],M['leather'],col,24)
        if detailed:
            # Fingers curl around the grip on the right; the left rests half closed.
            for k in range(4):
                if sign<0:
                    z=.997-k*.027
                    cx=-.543+.247*(z-.87)
                    joints=[(cx-.055,-.143,z),(cx-.056,-.220,z),
                            (cx-.020,-.245,z-.004),(cx+.025,-.224,z-.005)]
                else:
                    x=sign*.54+(k-1.5)*.027
                    z=.944-abs(k-1.5)*.004
                    joints=[(x,-.151,z),(x,-.174,z-.045),(x,-.147,z-.074)]
                S.tube('Gloved finger',joints,.014,M['leather'],col,False,10)
                S.limb('Finger first articulated plate',joints[0],joints[1],
                       [(0,.0148,.0148),(.80,.015,.015),(1,.013,.013)],M['steel'],col,10,.001)
                S.limb('Finger return plate',joints[1],joints[2],
                       [(0,.013,.014),(1,.010,.012)],M['steel'],col,10,.001)
            thumb=([(sign*.602,-.103,.980),(sign*.615,-.153,.94),(sign*.584,-.184,.912)]
                   if sign>0 else [(-.487,-.133,1.015),(-.476,-.223,.988),(-.520,-.252,.965)])
            S.tube('Opposed glove thumb',thumb,.020,M['leather'],col,False,12)
            if sign>0:
                S.plate('Hand metacarpal shell',[(.60,-.173,.99),(.48,-.173,.99),
                    (.482,-.183,.94),(.585,-.183,.92)],M['steel'],col,.010,.011,.002)
        else:
            S.limb('Fist main volume',(sign*.54,-.125,.955),(sign*.54,-.147,.874),
                   [(0,.067,.06),(.5,.07,.06),(1,.055,.043)],M['leather'],col,20,.003)
        for j in range(3):
            if sign<0 and j==2:
                continue
            shoulder(('Right' if sign<0 else 'Left')+' swept pauldron '+str(j),sign,j,col,detailed)


def legs(detailed):
    col=collection('LEGS')
    for sign in [-1,1]:
        x=sign*.235
        S.limb('Upper leg padded leather',(sign*.195,.035,1.08),(sign*.247,.02,.65),
               [(0,.14,.13),(.5,.132,.123),(1,.108,.105)],M['dark'],col,32)
        S.zloft('Continuous knee joint',[(.485,.084,.081,sign*.255,.035),
            (.55,.093,.085,sign*.25,.03),(.63,.097,.087,sign*.249,.025),
            (.70,.106,.096,sign*.245,.025),(.755,.113,.102,sign*.24,.028)],M['mail'],col,32)
        thigh=S.limb('Upper thigh curved cuirass',(sign*.218,-.036,.99),(sign*.25,-.045,.68),
               [(0,.132,.131),(.25,.139,.132),(.8,.114,.116),(1,.095,.10)],M['steel'],col,32,.002)
        # Greaves have an anterior ridge and a swelling calf, not a cylinder.
        profiles=[(.16,.084,.09,sign*.275,.0),(.24,.091,.108,sign*.269,.016),
                  (.37,.112,.132,sign*.263,.032),(.49,.107,.108,sign*.252,.018),
                  (.56,.102,.103,sign*.249,.012)]
        greave=S.zloft('Anatomical greave',profiles,M['steel'],col,40,.003)
        knee=[(sign*.249,-.125,.727),(sign*.249-.101,-.128,.63),
              (sign*.249-.083,-.133,.56),(sign*.249,-.156,.52),
              (sign*.249+.083,-.133,.56),(sign*.249+.101,-.128,.63)]
        knee=[(x,y+.023,z) for x,y,z in knee]
        knee_plate=S.plate('Ridged knee cop',knee,M['steel'],col,.035,.020,.003)
        # Boot has a broad sole, projecting toe and raised instep.
        S.zloft('Boot sole',[(.025,.109,.181,sign*.274,-.073),(.065,.114,.183,sign*.274,-.073)],M['dark'],col,40,.006)
        boot=S.zloft('Armored sabaton',[(.057,.107,.173,sign*.274,-.069),(.064,.11,.176,sign*.274,-.069),
            (.112,.109,.175,sign*.274,-.07),(.125,.106,.166,sign*.274,-.064),
            (.165,.094,.134,sign*.271,-.031),(.22,.080,.09,sign*.269,.015)],M['steel'],col,40)
        if detailed:
            S.tube('Knee bronze rim',knee,.006,M['bronze'],col,True)
            S.tube('Knee central ridge',S.conform_path(knee_plate,[(sign*.249,-.17,.56+j*.14/20)
                    for j in range(21)]),.005,M['bronze'],col)
            # Raised sinuous shin spine and side trims.
            for sign2 in [-1,1]:
                path=[(sign*(.275-.023*t)+sign2*(.055+.019*t),-.15,.18+.33*t) for t in [j/30 for j in range(31)]]
                S.tube('Greave border',S.conform_path(greave,path),.005,M['bronze'],col)
            path=[(sign*(.273-.021*t),-.15,.19+.34*t) for t in [j/30 for j in range(31)]]
            S.tube('Greave embossed keel',S.conform_path(greave,path),.006,M['edge'],col)
            bpy.context.view_layer.update()
            evaluated=boot.evaluated_get(bpy.context.evaluated_depsgraph_get())
            for j,yy in enumerate([-.218,-.170,-.125]):
                verts=[]
                nu,nv=24,4
                for v in range(nv+1):
                    y=yy+(v/nv-.5)*.028
                    width=.105*math.sqrt(max(.04,1-((y+.07)/.175)**2))*.91
                    for u in range(nu+1):
                        x=sign*.274+width*(2*u/nu-1)
                        hit,loc,_,_=evaluated.ray_cast(Vector((x,y,2)),Vector((0,0,-1)))
                        if not hit:
                            raise RuntimeError('Sabaton lame left its supporting surface')
                        verts.append((x,y,loc.z+.007))
                faces=[(v*(nu+1)+u,v*(nu+1)+u+1,(v+1)*(nu+1)+u+1,(v+1)*(nu+1)+u)
                       for v in range(nv) for u in range(nu)]
                S.solid_patch('Fitted sabaton transverse plate',verts,faces,M['steel'],col,.009,.001)
            for z in [.29,.46]:
                a,b=next((a,b) for a,b in zip(profiles,profiles[1:]) if a[0]<=z<=b[0])
                t=(z-a[0])/(b[0]-a[0])
                rx,ry,cx,cy=[a[i]*(1-t)+b[i]*t for i in range(1,5)]
                S.zloft('Greave fastening strap',[(z-.014,rx+.008,ry+.008,cx,cy),
                    (z+.014,rx+.008,ry+.008,cx,cy)],M['leather'],col,40,.001)
        # Two outward-spreading articulated tassets on each thigh.
        for j in range(2):
            cx=sign*(.185+j*.081)
            z=1.12-j*.053
            outline=[(cx-.080,-.153+j*.017,z),(cx+.080,-.153+j*.017,z),
                     (cx+.10+sign*.024,-.131+j*.017,z-.235),
                     (cx+sign*.030,-.16+j*.017,z-.29),
                     (cx-.10+sign*.024,-.131+j*.017,z-.235)]
            tasset=S.plate('Curved flared thigh tasset',outline,M['steel'],col,.035,.017,.003,support=thigh)
            outline=S.plate_perimeter(tasset)
            if detailed:
                S.tube('Tasset bronze edge',outline,.006,M['bronze'],col,True)


def cloth_surface(name, fn, mat, col, nu=24,nv=36):
    vertices=[fn(i/nu,j/nv) for j in range(nv+1) for i in range(nu+1)]
    faces=[(j*(nu+1)+i,j*(nu+1)+i+1,(j+1)*(nu+1)+i+1,(j+1)*(nu+1)+i)
           for j in range(nv) for i in range(nu)]
    return S.solid_patch(name,vertices,faces,mat,col,.007,0),vertices


def cloth(detailed):
    col=collection('CLOTH')
    # Cape hangs from a shoulder line; folds expand away from the attachment.
    def cape(u,v):
        # The upper edge pulls diagonally toward the left shoulder. Four unequal
        # hanging folds widen under gravity and release beyond the pelvis.
        left=-.255-.045*v
        right=.33+.16*v+.05*math.sin(v*math.pi)
        x=left+(right-left)*u+.042*math.sin(v*math.pi)*(1-u)
        z=1.64+.105*u-1.26*v+.19*(1-u)**2*v**3-.055*u*v**5
        z+=v**9*(.025*math.sin(u*5*math.pi+.7)-.038*math.exp(-((u-.77)/.10)**2))
        fold=(.040*math.sin(2*math.pi*(u+.07*v))+
              .031*math.sin(6*math.pi*u+.8*v)+.020*math.sin(9*math.pi*u-.4*v))
        y=.275+.085*math.sin(v*math.pi*.88)+.07*v*v+fold*(.3+.7*v)
        return x,y,z
    ob,verts=cloth_surface('Heavy forest half cloak',cape,M['cloth'],col)
    if detailed:
        # A sewn border follows the actual hanging surface.
        for edge in [0,1]:
            S.tube('Cape woven side seam',[cape(edge,j/40) for j in range(41)],.006,M['cloth_edge'],col)
        S.tube('Cape lower woven hem',[cape(i/60,1) for i in range(61)],.007,M['cloth_edge'],col)
    def tabard(u,v):
        width=.104*(1-.20*v)
        x=(u-.5)*2*width+.035*math.sin(v*2)
        z=1.075-.68*v-.09*(1-abs(2*u-1))*v**6
        y=-.174-.014*v-.022*math.sin(u*3*math.pi)*math.sin(v*math.pi)
        return x,y,z
    ob,verts=cloth_surface('Split green front tabard',tabard,M['cloth'],col,18,30)
    if detailed:
        for edge in [0,1]:
            S.tube('Tabard woven border',[tabard(edge,j/35) for j in range(36)],.005,M['cloth_edge'],col)
    def sash(u,v):
        x=.074+(u-.5)*.071+.077*v
        y=-.204-.015*math.sin(v*4)-.008*math.sin(u*math.pi)
        z=1.105-.62*v-.025*u*v**6
        return x,y,z
    cloth_surface('Oxblood sash tail',sash,M['wine'],col,8,28)
    def mantle(u,v):
        x=-.265+.575*u
        y=-.05-.118*math.sin(math.pi*u)-.010*math.sin(v*math.pi*3)
        z=1.72+.055*u-.059*math.sin(math.pi*u)+.048*v
        return x,y,z
    cloth_surface('Gathered cloth at collar',mantle,M['cloth'],col,32,8)


def weapon(detailed):
    col=collection('WEAPON')
    # All coordinates are designed in the sword frame: negative local Z is blade.
    origin=Vector((-.543,-.207,.87))
    direction=Vector((-.24,0,-.971)).normalized()
    q=Vector((0,0,-1)).rotation_difference(direction)
    def tr(p):
        return tuple(origin+q@Vector(p))
    def local_loft(name,rings,mat,bevel=0):
        return S.loft(name,[[tr(p) for p in r] for r in rings],mat,col,False,bevel)
    # Thick spine, recessed fuller and a broad real cutting bevel, on both faces.
    rings=[]
    profile=[(-1,-.62),(-.90,-1),(-.70,-1),(-.60,-.65),(-.40,-.65),(-.30,-1),
             (-.14,-1),(1,0),(-.14,1),(-.30,1),(-.40,.65),(-.60,.65),(-.70,1),(-.90,1),(-1,.62)]
    for z,w,t,cx,slant in [(0,.068,.018,0,0),(-.065,.081,.019,0,0),(-.43,.071,.016,0,0),
                          (-.69,.061,.014,0,0),(-.79,.050,.012,0,.018),
                          (-.832,.027,.008,-.017,.022),(-.862,.0025,.0015,-.029,0)]:
        rings.append([(cx+x*w,y*t,z+x*slant) for x,y in profile])
    blade=local_loft('Broad clipped sword blade',rings,M['edge'],.001)
    blade.data.materials.append(M['steel'])
    for poly in blade.data.polygons:
        poly.material_index=0 if poly.index>0 and (poly.index-1)%len(profile) in {6,7} else 1
    grip=[[(.026*math.cos(a),.026*math.sin(a),z) for a in [2*math.pi*i/20 for i in range(20)]]
          for z in [.018,.10,.143]]
    local_loft('Wrapped sword grip',grip,M['leather'],.002)
    # A winged guard is a thick shaped solid, with its own weight and silhouette.
    outline=[(-.175,-.016,.025),(-.156,-.016,.057),(-.072,-.022,.025),
             (0,-.024,.012),(.075,-.022,.025),(.156,-.016,.057),(.175,-.016,.025),
             (.094,-.020,-.006),(0,-.025,-.025),(-.094,-.020,-.006)]
    guard_vertices=[tr((x,y,z)) for y in [-.025,.025] for x,_,z in outline]
    n=len(outline)
    guard_faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]
    guard_faces.extend((i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n))
    S.mesh('Swept bronze sword guard',guard_vertices,guard_faces,M['bronze'],col,False,.004)
    local_loft('Sword pommel',[[ (r*math.cos(a),r*math.sin(a),z)
                 for a in [2*math.pi*i/12 for i in range(12)]]
                 for z,r in [(.139,.026),(.150,.029),(.167,.018)]],M['bronze'],.002)
    if detailed:
        for j in range(5):
            pts=[tr((.027*math.cos(a),.027*math.sin(a),.039+j*.02))
                 for a in [2*math.pi*i/20 for i in range(20)]]
            S.tube('Sword grip wrap',pts,.004,M['dark'],col,True)
        emblem=[tr((0,-.037,.046)),tr((-.028,-.037,.009)),
                tr((0,-.037,-.028)),tr((.028,-.037,.009))]
        S.plate('Sword guard inset',emblem,M['steel'],col,.004,.010,.001)


def main():
    global KNIGHT,M
    if not bpy.app.background:
        raise RuntimeError('Run the knight builder in an isolated background Blender.')
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
    parser=argparse.ArgumentParser()
    parser.add_argument('--stage',choices=['blockout','final'],default='blockout')
    opts=parser.parse_args(args)
    bpy.context.preferences.use_preferences_save=False
    bpy.ops.wm.read_factory_settings(use_empty=True)
    KNIGHT=bpy.data.collections.new('KNIGHT')
    bpy.context.scene.collection.children.link(KNIGHT)
    M={'steel':material('Blued forged steel',(.065,.091,.115),.72,.42,.00017),
       'bronze':material('Aged warm bronze',(.26,.145,.049),.80,.43,.00013),
       'edge':material('Worn steel bevel',(.18,.225,.26),.86,.37,.00005),
       'dark':material('Near black padded cloth',(.015,.021,.025),0,.80),
       'mail':material('Dark quilted underlayers',(.027,.032,.035),.05,.88,.0010),
       'leather':material('Worn brown leather',(.042,.024,.015),0,.69,.00075),
       'cloth':material('Forest green heavy cloth',(.038,.068,.039),0,.94,.00060),
       'cloth_edge':material('Muted gold woven trim',(.24,.205,.107),.1,.72),
       'wine':material('Oxblood sash',(.15,.037,.034),0,.82,.00012)}
    detailed=opts.stage=='final'
    body(detailed)
    arms(detailed)
    legs(detailed)
    cloth(detailed)
    weapon(detailed)
    headcol=collection('HELMET')
    from knight_helmet import build_helmet
    headobjects=build_helmet(headcol,M,detailed=detailed)
    for ob in headobjects:
        ob.location+=Vector((0,-.015,1.96))
    scene=bpy.context.scene
    scene.unit_settings.system='METRIC'
    scene.unit_settings.scale_length=1
    scene['asset_status']='Original knight visual study; no game-ready or rig claim'
    scene['design_reference']='art/concepts/2026-10-05-heavy-warrior-v2.png'
    scene['design_change']='Closed battle helmet, new topology and original armor construction'
    scene['build_stage']=opts.stage
    OUT.mkdir(parents=True,exist_ok=True)
    file=OUT/('knight-blockout.blend' if not detailed else 'knight.blend')
    bpy.ops.wm.save_as_mainfile(filepath=str(file))
    print('KNIGHT_SAVED '+json.dumps({'file':str(file),'objects':len(KNIGHT.all_objects),'stage':opts.stage}))


if __name__=='__main__':
    main()

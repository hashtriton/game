"""Editable, original veteran head for the local heavy-warrior sculpt.

The public build_head(materials, collection) function only adds mesh objects and
missing materials. It does not select objects, alter the scene, save or render.
Coordinates use Z up and the face points toward -Y; dimensions match the body
brief with the neck at 5.85 and the swept hair at approximately 7.15.
"""

from math import sin, cos, pi, sqrt, exp
import random

import bpy
import bmesh
from mathutils import Vector


_PROFILE = [
    # z, half width, face depth, back depth, center y
    (6.145, .085, .230, .145, .040),
    (6.180, .218, .367, .210, .040),
    (6.245, .292, .392, .260, .038),
    (6.335, .316, .378, .285, .036),
    (6.430, .329, .382, .309, .032),
    (6.535, .368, .384, .332, .030),
    (6.630, .386, .389, .347, .030),
    (6.725, .390, .398, .354, .028),
    (6.825, .380, .391, .355, .027),
    (6.920, .343, .341, .337, .025),
    (7.000, .253, .254, .270, .025),
    (7.065, .020, .030, .045, .025),
]

_NASAL_ROWS = [
    (6.790,.052,-.369), (6.768,.046,-.400), (6.730,.050,-.441),
    (6.688,.056,-.488), (6.645,.060,-.531), (6.598,.066,-.573),
    (6.557,.074,-.613), (6.527,.098,-.625), (6.509,.109,-.605),
    (6.487,.086,-.530), (6.468,.047,-.454), (6.454,.035,-.395),
    (6.439,.028,-.366),
]


def _nasal_y(x, z, base):
    """Piecewise designed dorsal planes, blended into the continuous skin."""
    if not _NASAL_ROWS[-1][0] <= z <= _NASAL_ROWS[0][0]:
        return base
    for a,b in zip(_NASAL_ROWS[:-1], _NASAL_ROWS[1:]):
        if b[0] <= z <= a[0]:
            t=(a[0]-z)/(a[0]-b[0])
            half=a[1]*(1-t)+b[1]*t
            front=a[2]*(1-t)+b[2]*t
            au=abs(x)/half
            if au > 1:
                return base
            if au <= .19:
                y=front+.007*au/.19
            elif au <= .48:
                y=front+.007+.048*(au-.19)/.29
            else:
                y=front+.055+(base-front-.055)*(au-.48)/.52
            if 6.48 < z < 6.545:
                wing=.036*sin(pi*(z-6.48)/.065)
                y-=wing*exp(-((au-.76)/.19)**2)
            return min(base,y)
    return base


def _gauss(x, z, cx, cz, sx, sz):
    return exp(-((x - cx) / sx) ** 2 - ((z - cz) / sz) ** 2)


def _profile(z):
    """Cubic interpolation avoids visible horizontal profile bands."""
    if z <= _PROFILE[0][0]:
        return _PROFILE[0][1:]
    if z >= _PROFILE[-1][0]:
        return _PROFILE[-1][1:]
    for i in range(len(_PROFILE) - 1):
        a, b = _PROFILE[i], _PROFILE[i + 1]
        if a[0] <= z <= b[0]:
            t = (z - a[0]) / (b[0] - a[0])
            before = _PROFILE[max(i - 1, 0)]
            after = _PROFILE[min(i + 2, len(_PROFILE) - 1)]
            out = []
            for k in range(1, 5):
                ma = (b[k] - before[k]) / (b[0] - before[0])
                mb = (after[k] - a[k]) / (after[0] - a[0])
                span = b[0] - a[0]
                out.append((2*t**3 - 3*t*t + 1)*a[k]
                           + (t**3 - 2*t*t + t)*span*ma
                           + (-2*t**3 + 3*t*t)*b[k]
                           + (t**3 - t*t)*span*mb)
            return out
    raise ValueError(z)


def _relief(x, z):
    """Anterior relief: the nose and facial bones belong to the skin mesh."""
    d = 0.0
    for s in (-1, 1):
        d += .031 * _gauss(x, z, s*.173, 6.658, .104, .041)  # socket
        d += .012 * _gauss(x, z, s*.188, 6.602, .103, .026)  # tear trough
        d -= .072 * _gauss(x, z, s*.267, 6.574, .104, .036)  # cheekbone
        d += .051 * _gauss(x, z, s*.265, 6.475, .075, .055)  # hollow cheek
        d -= .034 * _gauss(x, z, s*.175, 6.711, .123, .044)  # brow foundation
        d += .010 * _gauss(x, z, s*.117, 6.434, .020, .077)  # nasolabial
    ridge_z = 6.700 + .12*abs(x)
    ridge_x = max(0, 1-((abs(x)-.177)/.151)**4)
    d -= .060*ridge_x*max(0, 1-abs(z-ridge_z)/.044)
    d -= .022 * _gauss(x, z, 0, 6.390, .112, .060)  # muzzle
    d -= .017 * _gauss(x, z, 0, 6.265, .167, .046)  # mental eminence
    d += .012 * _gauss(x, z, 0, 6.318, .108, .021)  # lower lip groove
    d -= .011 * _gauss(x, z, 0, 6.735, .045, .055)  # glabella
    for crease in (6.806, 6.837, 6.863):
        d += .0024*exp(-((z-crease-.003*cos(x*15))/.0028)**2-(x/.255)**6)
    for s in (-1, 1):
        d += .0035*_gauss(x, z, s*.036, 6.766, .005, .025)
    return d


def _headpoint(theta, z, offset=0.0):
    rx, fd, bd, cy = _profile(z)
    c = cos(theta)
    x = rx * sin(theta)
    if c >= 0:
        y = cy - fd * c**.55 + _relief(x, z) * c**.4
        if c > .65:
            y = _nasal_y(x,z,y)
    else:
        y = cy + bd * (-c)**.85
    return Vector((x + offset*sin(theta), y - offset*cos(theta), z))


def _face_y(x, z):
    rx, fd, _, cy = _profile(z)
    c = sqrt(max(.001, 1.0 - (x / max(.01, rx))**2))
    return _nasal_y(x,z,cy - fd * c**.55 + _relief(x, z) * c**.4)


def _material(name, color, roughness=.5, metallic=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get('Principled BSDF')
    shader.inputs['Base Color'].default_value = (*color, 1)
    shader.inputs['Roughness'].default_value = roughness
    shader.inputs['Metallic'].default_value = metallic
    shader.inputs['Specular IOR Level'].default_value = .22
    if 'Subsurface Weight' in shader.inputs and 'Skin' in name:
        shader.inputs['Subsurface Weight'].default_value = .045
        shader.inputs['Subsurface Radius'].default_value = (.9, .4, .22)
    if 'Skin' in name or 'Hair' in name:
        nodes, links = mat.node_tree.nodes, mat.node_tree.links
        tex = nodes.new('ShaderNodeTexNoise')
        tex.inputs['Scale'].default_value = 115 if 'Skin' in name else 175
        tex.inputs['Detail'].default_value = 2.8
        tex.inputs['Roughness'].default_value = .73
        bump = nodes.new('ShaderNodeBump')
        bump.inputs['Strength'].default_value = .17 if 'Skin' in name else .30
        bump.inputs['Distance'].default_value = .0025 if 'Skin' in name else .005
        links.new(tex.outputs['Fac'], bump.inputs['Height'])
        links.new(bump.outputs['Normal'], shader.inputs['Normal'])
        if 'Hair' in name:
            shader.inputs['Specular IOR Level'].default_value = .13
    return mat


def _mesh(name, verts, faces, material, collection, objects):
    data = bpy.data.meshes.new(name + ' Mesh')
    data.from_pydata(verts, [], faces)
    data.update()
    bm = bmesh.new()
    bm.from_mesh(data)
    bmesh.ops.remove_doubles(bm, verts=list(bm.verts), dist=1e-7)
    bmesh.ops.dissolve_degenerate(bm, edges=list(bm.edges), dist=1e-8)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(data)
    bm.free()
    obj = bpy.data.objects.new(name, data)
    collection.objects.link(obj)
    if material is not None:
        data.materials.append(material)
    for polygon in data.polygons:
        polygon.use_smooth = True
    obj['asset_part'] = 'veteran_head'
    obj['original_procedural_geometry'] = True
    objects.append(obj)
    return obj


def _grid(name, points, nu, nv, material, collection, objects, wrap=False):
    faces = []
    for j in range(nv - 1):
        for i in range(nu if wrap else nu - 1):
            i2 = (i + 1) % nu
            faces.append((j*nu+i, j*nu+i2, (j+1)*nu+i2, (j+1)*nu+i))
    return _mesh(name, points, faces, material, collection, objects)


def _ellipsoid(name, center, radii, material, collection, objects, n=40, rings=24):
    verts, faces = [], []
    for j in range(rings + 1):
        a = pi * j / rings
        for i in range(n):
            t = 2*pi*i/n
            verts.append((center[0]+radii[0]*sin(a)*cos(t),
                          center[1]+radii[1]*sin(a)*sin(t),
                          center[2]+radii[2]*cos(a)))
    for j in range(rings):
        for i in range(n):
            k = j*n+i
            faces.append((k, (j+1)*n+i, (j+1)*n+(i+1)%n, j*n+(i+1)%n))
    return _mesh(name, verts, faces, material, collection, objects)


def _catmull(points, count):
    pts = [Vector(p) for p in points]
    result = []
    for k in range(count):
        u = k/(count-1)*(len(pts)-1)
        i = min(int(u), len(pts)-2)
        t = u-i
        p0, p1 = pts[max(i-1, 0)], pts[i]
        p2, p3 = pts[i+1], pts[min(i+2, len(pts)-1)]
        result.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t
                          +(-p0+3*p1-3*p2+p3)*t*t*t))
    return result


def _lock(name, path, width, depth, material, collection, objects,
          sections=20, sides=10, groove=0.0, tip=.10, normal=(0, -1, 0)):
    """Tapered elliptical sweep; hair has sculpted longitudinal ridges."""
    path = _catmull(path, sections)
    verts, faces = [], []
    previous_across = None
    for j, point in enumerate(path):
        t = j/(sections-1)
        tangent = path[min(j+1, sections-1)]-path[max(j-1, 0)]
        tangent.normalize()
        if previous_across is None:
            n = Vector(normal)
            if abs(tangent.dot(n)) > .85:
                n = Vector((0,-1,0))
                if abs(tangent.dot(n)) > .85:
                    n = Vector((1,0,0))
            across = tangent.cross(n).normalized()
        else:
            across = previous_across-tangent*previous_across.dot(tangent)
            across.normalize()
        previous_across = across.copy()
        outward = across.cross(tangent).normalized()
        taper = .08 + .92*sin(pi*t)**.55
        taper *= 1-(1-tip)*t**5
        for i in range(sides):
            a = 2*pi*i/sides
            rib = 1 + groove*cos(5*a + .25*sin(t*pi))
            q = point + across*(width*.5*taper*cos(a)) + outward*(depth*taper*sin(a)*rib)
            verts.append(tuple(q))
    for j in range(sections-1):
        for i in range(sides):
            faces.append((j*sides+i, j*sides+(i+1)%sides,
                          (j+1)*sides+(i+1)%sides, (j+1)*sides+i))
    faces.extend([tuple(reversed(range(sides))),
                  tuple((sections-1)*sides+i for i in range(sides))])
    return _mesh(name, verts, faces, material, collection, objects)


def build_head(materials, collection):
    """Build the original editable head in collection; return created objects.

    Existing supplied material datablocks are respected. Missing keys are added
    to the supplied dictionary. No bpy operator, render, save or scene reset is
    called, so the builder can safely be composed with the body generator.
    """
    defaults = {
        'skin': ('Veteran Skin', (.37, .194, .110), .65),
        'skin_shadow': ('Veteran Skin Crease', (.17, .073, .041), .67),
        'lip': ('Veteran Lip', (.285, .111, .076), .53),
        'hair': ('Veteran Charcoal Hair', (.021, .017, .012), .80),
        'hair_light': ('Veteran Salt Hair', (.095, .087, .071), .80),
        'eye_white': ('Veteran Warm Sclera', (.22, .20, .16), .52),
        'iris': ('Veteran Amber Iris', (.018, .020, .012), .50),
        'pupil': ('Veteran Pupil', (.004, .003, .002), .23),
    }
    for key, (name, color, roughness) in defaults.items():
        if key not in materials or materials[key] is None:
            materials[key] = _material(name, color, roughness)
    m, made = materials, []

    # One closed anatomical skin surface with sculpted facial planes.
    n, nz = 192, 128
    verts = [tuple(_headpoint(2*pi*i/n, 6.145+(7.065-6.145)*j/(nz-1)))
             for j in range(nz) for i in range(n)]
    head = _grid('Head | continuous anatomical skin', verts, n, nz, m['skin'], collection, made, True)
    # The tiny crown and chin openings are covered by separate connected caps.
    bottom = list(range(n-1, -1, -1))
    top = [(nz-1)*n+i for i in range(n)]
    data = head.data
    oldverts = [tuple(v.co) for v in data.vertices]
    oldfaces = [tuple(p.vertices) for p in data.polygons]
    data.clear_geometry()
    data.from_pydata(oldverts, [], oldfaces+[tuple(bottom), tuple(top)])
    for polygon in data.polygons:
        polygon.use_smooth = True
    data.update()

    # Neck with integrated paired sternocleidomastoid relief.
    verts = []
    for j in range(33):
        t = j/32
        z = 5.85 + .49*t
        rx = .272 - .042*sin(pi*t) + .018*t
        ry = .231 - .016*sin(pi*t)
        for i in range(80):
            theta = 2*pi*i/80
            x = rx*sin(theta)
            y = .050-ry*cos(theta)
            if cos(theta) > 0:
                ridge_x = .065 + .145*t
                y -= .020*exp(-((abs(x)-ridge_x)/.030)**2)*sin(pi*t)**.5
                y -= .010*exp(-(x/.046)**2-((z-6.05)/.10)**2)
            verts.append((x, y, z))
    _grid('Neck | sternomastoid planes', verts, 80, 33, m['skin'], collection, made, True)

    # Small almond-shaped eyeball surfaces instead of protruding full spheres.
    # Surrounding lid ribbons extend back onto the continuous socket surface.
    for side, label in ((-1, 'R'), (1, 'L')):
        cx, cy, ez, radius = side*.177, -.279, 6.651, .074

        def eye_edge(t, upper):
            local = -.071 + .142*t
            # Inner corner is slightly lower, accentuating a mature stern gaze.
            base = ez + .012*(2*t-1)
            z = base + (.010 if upper else -.011)*sin(pi*t)**.75
            x = cx+side*local
            dy = sqrt(max(.0001, radius*radius-local*local-(z-ez)**2))
            return Vector((x, cy-dy, z))

        verts = []
        for j in range(13):
            v = j/12
            for i in range(41):
                t = i/40
                low, high = eye_edge(t, False), eye_edge(t, True)
                p = low.lerp(high, v)
                dx, dz = p.x-cx, p.z-ez
                p.y = cy-sqrt(max(.0001, radius*radius-dx*dx-dz*dz))
                verts.append(tuple(p))
        _grid('Eye '+label+' | almond sclera', verts, 41, 13, m['eye_white'], collection, made)
        # Pigmentation lies on the eyeball and is clipped by the lid opening.
        # Separate full spheres would protrude in front of the eyelids.
        for disc, dr, material_key, push in (
                ('amber iris', .026, 'iris', .0008),
                ('pupil', .0115, 'pupil', .0015)):
            verts, faces = [], []
            for j in range(13):
                r = dr*j/12
                for i in range(48):
                    a = 2*pi*i/48
                    x = cx+r*cos(a)
                    z = ez+.001+r*sin(a)
                    et = max(.001, min(.999, (side*(x-cx)+.071)/.142))
                    lo, hi = eye_edge(et, False).z, eye_edge(et, True).z
                    z = max(lo+.0006, min(hi-.0006, z))
                    dy = sqrt(max(.0001, radius*radius-(x-cx)**2-(z-ez)**2))
                    verts.append((x, cy-dy-push, z))
            for j in range(12):
                for i in range(48):
                    faces.append((j*48+i,j*48+(i+1)%48,
                                  (j+1)*48+(i+1)%48,(j+1)*48+i))
            _mesh('Eye '+label+' | '+disc, verts, faces, m[material_key], collection, made)

        for upper in (False, True):
            verts = []
            for j in range(7):
                v = j/6
                for i in range(41):
                    t = i/40
                    p = eye_edge(t, upper)
                    spread = sin(pi*t)**.5
                    x = p.x + side*(2*t-1)*.017*v
                    z = p.z + (1 if upper else -1)*(.033 if upper else .030)*spread*v
                    y = (1-v)*(p.y-.004*sin(pi*v)) + v*(_face_y(x, z)-.002)
                    verts.append((x, y, z))
            _grid('Lid '+label+(' upper' if upper else ' lower'), verts, 41, 7,
                  m['skin'], collection, made)
            line = [tuple(eye_edge(i/18, upper)+Vector((0, -.0035, 0))) for i in range(19)]
            _lock('Lid '+label+' fleshy rim '+str(upper), line,
                  .0048 if upper else .004, .0018, m['skin'], collection, made,
                  sections=40, sides=8, tip=.5)

        # Dark upper-lid lash line and crease remain very fine at hero scale.
        line = [tuple(eye_edge(i/15, True)+Vector((0, -.004, -.001))) for i in range(16)]
        _lock('Eye '+label+' upper lash', line, .0037, .0021, m['hair'], collection, made,
              sections=32, sides=6, tip=.15)
        _ellipsoid('Eye '+label+' tear corner', (cx-side*.067, -.307, ez-.011),
                   (.008, .004, .005), m['lip'], collection, made, 24, 12)

        # Heavy brows are sculpted flowing ribbons, not detached blocks.
        brow = [(side*x, _face_y(side*x,z)-.005, z)
                for x,z in ((.064,6.702),(.122,6.713),(.203,6.727),(.288,6.720))]
        _lock('Brow '+label+' | heavy swept form', brow, .030, .004, m['hair'],
              collection, made, sections=28, sides=12, groove=.16, tip=.12)
        for k in range(10):
            t = (k+.5)/11
            x = side*(.082+.190*t)
            z = 6.704+.026*sin(pi*t)
            y = _face_y(x, z)-.014
            _lock('Brow '+label+' strand '+str(k),
                  [(x, y, z-.008), (x+side*.017, y-.006, z+.012),
                   (x+side*.027, y+.001, z+.018)],
                  .003, .0016, m['hair_light'] if k == 8 else m['hair'],
                  collection, made, sections=9, sides=6, tip=.1)

    # Anatomical lips: a cupid bow, full lower lip and a thin closed mouth line.
    def mouth_line(x):
        q = abs(x)/.106
        return 6.385 - .008*q**1.8 + .002*cos(x/.106*pi)

    for upper in (True, False):
        verts = []
        for j in range(9):
            t = j/8
            for i in range(65):
                x = -.106+.212*i/64
                q = abs(x)/.106
                envelope = max(0, 1-q*q)**.65
                height = (.016 if upper else .021)*envelope
                if upper:
                    height *= .72+.34*exp(-((abs(x)-.030)/.018)**2)
                z = mouth_line(x)+(1 if upper else -1)*height*t
                bulge = (.013 if upper else .019)*sin(pi*t)*envelope
                y = _face_y(x, z)-.004-bulge
                verts.append((x, y, z))
        _grid('Mouth | '+('cupid upper lip' if upper else 'lower lip'), verts, 65, 9,
              m['lip'], collection, made)
    line = [(x, _face_y(x, mouth_line(x))-.006, mouth_line(x))
            for x in [-.104+.208*i/28 for i in range(29)]]
    _lock('Mouth | closed seam', line, .0043, .0028, m['skin_shadow'], collection, made,
          sections=40, sides=6, tip=.1)

    # Narrow dark nostril interiors sit in the underside of the nasal wings.
    for side in (-1, 1):
        _ellipsoid('Nostril '+str(side), (side*.062, -.510, 6.491),
                   (.017, .012, .0043), m['skin_shadow'], collection, made, 28, 14)

    # Two shaped pinnae, each with a rolled helix, concha and antihelix.
    for side, label in ((-1, 'R'), (1, 'L')):
        outline = [(.365, -.005, 6.523), (.421, -.023, 6.515),
                   (.462, -.031, 6.560), (.485, -.033, 6.665),
                   (.467, -.023, 6.751), (.420, -.010, 6.779),
                   (.372, .004, 6.748), (.355, -.002, 6.674),
                   (.365, -.005, 6.523)]
        boundary = _catmull(outline, 65)
        center = Vector((.414, .010, 6.635))
        verts = []
        for j in range(13):
            t = j/12
            for p in boundary[:-1]:
                q = center.lerp(p, t)
                q.y += .018*sin(pi*t)
                verts.append((side*q.x, q.y, q.z))
        _grid('Ear '+label+' | pinna shell', verts, 64, 13, m['skin'], collection, made, True)
        rim = [(side*p.x, p.y-.008, p.z) for p in boundary]
        _lock('Ear '+label+' | rolled helix', rim, .023, .017,
              m['skin'], collection, made, sections=80, sides=10, tip=.85, normal=(0,-1,0))
        _ellipsoid('Ear '+label+' | recessed concha', (side*.408, -.011, 6.625),
                   (.024, .004, .039), m['skin_shadow'], collection, made, 32, 18)
        _lock('Ear '+label+' | antihelix',
              [(side*.416, -.026, 6.564), (side*.440, -.039, 6.623),
               (side*.440, -.036, 6.690), (side*.411, -.022, 6.729)],
              .019, .011, m['skin'], collection, made, sections=28, sides=10, tip=.35)
        _ellipsoid('Ear '+label+' | tragus', (side*.379, -.031, 6.621),
                   (.018, .018, .026), m['skin'], collection, made, 28, 18)

    # A short beard conforms to the actual jaw and follows its anatomical turn.
    def beard_top(theta):
        u = abs(sin(theta))
        return 6.333+.124*u**1.2+.165*u**5

    verts = []
    bn, bz = 97, 26
    for j in range(bz):
        t = j/(bz-1)
        for i in range(bn):
            theta = -1.59+3.18*i/(bn-1)
            low = 6.141+.190*abs(sin(theta))**1.3+.006*sin(theta*23)
            top = beard_top(theta)+.009*sin(theta*79)+.005*sin(theta*133)
            z = low*(1-t)+top*t
            relief = .0025*sin(theta*117+z*58)+.0016*sin(theta*191-z*141)
            p = _headpoint(theta, z, .008+.025*(1-t)*max(0,cos(theta))**.6+relief)
            p.z -= .012*(1-t)
            verts.append(tuple(p))
    _grid('Beard | short sculpted jaw mass', verts, bn, bz, m['hair'], collection, made)

    rng = random.Random(4142)
    for k in range(350):
        theta = rng.uniform(-1.53, 1.53)
        low = 6.163+.177*abs(sin(theta))**1.3
        top = beard_top(theta)-.014
        z = rng.uniform(low, top)
        length = rng.uniform(.034, .054)
        p = _headpoint(theta, z, .010)
        q = _headpoint(theta+(.008 if theta>0 else -.008), max(6.155, z-length), .014)
        mid = p.lerp(q, .5)+Vector((0, -.001*cos(theta), .001))
        gray = (k%11 == 0) or (abs(theta) > 1.1 and k%5 == 0)
        _lock('Beard | grain %03d'%k, [p, mid, q], rng.uniform(.0028, .004), .0012,
              m['hair_light'] if gray else m['hair'], collection, made,
              sections=7, sides=6, groove=.05, tip=.1)

    for side, label in ((-1, 'R'), (1, 'L')):
        verts=[]
        for j in range(14):
            v=j/13
            for i in range(35):
                u=i/34
                x=side*(.0008+.157*u)
                high=6.486-.067*u**.75+.002*sin(u*91)
                low=6.414-.063*u**2
                end=max(0,(u-.80)/.20)
                high=high*(1-end)+low*end
                z=low*(1-v)+high*v
                y=_face_y(x,z)-.012-.026*exp(-(x/.075)**2)*v
                y-=.0017*sin(u*113+v*7)
                verts.append((x,y,z))
        _grid('Moustache '+label+' | connected upper lip mantle',verts,35,14,
              m['hair'],collection,made)

    # Hair is built from a continuous ridged scalp and a small set of large
    # overlapping sculpted masses. There are no narrow hanging strip fringes.
    def hair_bottom(theta):
        c = cos(theta)
        return 6.695+.187*max(c, 0)-.110*max(-c, 0)+.009*sin(theta*9)

    hn, hz, verts = 160, 64, []
    for j in range(hz):
        t = j/(hz-1)
        for i in range(hn):
            theta = 2*pi*i/hn
            low = hair_bottom(theta)
            z = low*(1-t)+7.095*t
            source_z = min(7.059, z-.045*t)
            ridges = .006*sin(theta*23+t*15)+.004*cos(theta*7-t*13)
            p = _headpoint(theta, source_z, .025+ridges*sin(pi*t))
            # Close the crown to a single natural-shaped tuft region.
            if t > .92:
                f = max(.03, (1-t)/.08)
                p.x *= f
                p.y = .065+(p.y-.065)*f
            p.z = z
            verts.append(tuple(p))
    _grid('Hair | sculpted scalp foundation', verts, hn, hz, m['hair'], collection, made, True)

    for k in range(9):
        u = (k-4)/4
        x = .300*u
        path = [(x, -.367+.085*abs(u), 6.851-.048*abs(u)+.014*sin(k*2.1)),
                (x*.85+.046, -.272, 7.007-.042*abs(u)),
                (x*.80+.049, -.022, 7.114-.047*abs(u)),
                (x*.85+.018, .205, 7.035-.035*abs(u)),
                (x*.77-.025, .373, 6.817-.025*abs(u))]
        _lock('Hair | broad swept crown mass %02d'%k, path, .182, .028,
              m['hair'], collection, made,
              sections=34, sides=18, groove=.11, tip=.12, normal=(0,0,1))

    for side, label in ((-1, 'R'), (1, 'L')):
        for k in range(5):
            t = k/4
            x0 = side*(.343+.036*sin(pi*t))
            y0 = -.266+.110*t
            z0 = 6.858-.159*t
            path = [(x0,y0,z0), (side*.402,y0+.121,z0+.041),
                    (side*.410,y0+.303,z0+.012),
                    (side*.346,.389,z0-.097)]
            _lock('Hair '+label+' | overlapping temple mass %02d'%k, path,
                  .145, .023, m['hair_light'] if k==4 else m['hair'],
                  collection, made, sections=26, sides=16, groove=.13, tip=.11,
                  normal=(side,0,0))
        # Sideburns meet the beard ahead of the ear rather than covering it.
        for k in range(2):
            x = side*(.354+.009*k)
            _lock('Hair '+label+' | sideburn '+str(k),
                  [(x, -.178+.018*k, 6.793), (x, -.148+.017*k, 6.715),
                   (x-side*.011, -.132+.018*k, 6.637)],
                  .046, .008, m['hair_light'] if k==1 else m['hair'], collection, made,
                  sections=15, sides=8, groove=.08, tip=.20)

    # A small turn gives the face a living direction; the lower neck is fixed.
    # Transform actual vertices, preserving the caller's object hierarchy.
    turn = 12*pi/180
    for obj in made:
        for vertex in obj.data.vertices:
            p = vertex.co
            blend = min(1.0, max(0.0, (p.z-5.85)/.33))
            angle = turn*blend
            x, y = p.x*(1+.08*blend), p.y
            p.x = x*cos(angle)-y*sin(angle)
            p.y = x*sin(angle)+y*cos(angle)
        obj.data.update()
    return made

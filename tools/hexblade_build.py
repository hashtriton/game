"""Собирает героя Hexblade (рабочее название) в art/heroes/hexblade/hexblade.blend.

Запуск только отдельным background-процессом из корня game:
blender --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/hexblade_build.py
"""
import json
import math
import sys
from pathlib import Path

import bmesh
import bpy
from mathutils import Matrix, Quaternion, Vector
from mathutils import noise as mnoise
from mathutils.bvhtree import BVHTree

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import add_area, point_at  # noqa: E402

OUT = ROOT / 'art/heroes/hexblade'
V = Vector
# Metaball: при threshold 0.6 и stiffness 2 видимый радиус равен 0.575 * radius (замерено в 5.2.2).
K = 1 / 0.5748
UP = V((0, 0, 1))


# ---------------------------------------------------------------- материалы

def _gradient_ao(nt, color_socket, base, ao_dist=.06, ao_mix=.55, grad=(.62, 1.0)):
    """Стилизованный верхний свет и затемнение щелей поверх базового цвета."""
    coord = nt.nodes.new('ShaderNodeTexCoord')
    sep = nt.nodes.new('ShaderNodeSeparateXYZ')
    nt.links.new(coord.outputs['Object'], sep.inputs[0])
    rng = nt.nodes.new('ShaderNodeMapRange')
    rng.inputs['From Min'].default_value = 0
    rng.inputs['From Max'].default_value = 2.1
    rng.inputs['To Min'].default_value = grad[0]
    rng.inputs['To Max'].default_value = grad[1]
    nt.links.new(sep.outputs['Z'], rng.inputs['Value'])
    ao = nt.nodes.new('ShaderNodeAmbientOcclusion')
    ao.inputs['Distance'].default_value = ao_dist
    ao_ramp = nt.nodes.new('ShaderNodeMapRange')
    ao_ramp.inputs['To Min'].default_value = 1 - ao_mix
    nt.links.new(ao.outputs['AO'], ao_ramp.inputs['Value'])
    # У ShaderNodeMix сокеты A/B/Result повторяются для float/vector/color; цветовые: inputs 6, 7, outputs 2.
    m1 = nt.nodes.new('ShaderNodeMix')
    m1.data_type = 'RGBA'
    m1.blend_type = 'MULTIPLY'
    m1.inputs[0].default_value = 1
    nt.links.new(base, m1.inputs[6])
    nt.links.new(rng.outputs['Result'], m1.inputs[7])
    m2 = nt.nodes.new('ShaderNodeMix')
    m2.data_type = 'RGBA'
    m2.blend_type = 'MULTIPLY'
    m2.inputs[0].default_value = 1
    nt.links.new(m1.outputs[2], m2.inputs[6])
    nt.links.new(ao_ramp.outputs['Result'], m2.inputs[7])
    nt.links.new(m2.outputs[2], color_socket)


def material(name, color, **kw):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    m.diffuse_color = (*color, 1)
    _setup_bsdf(m.node_tree, m.node_tree.nodes['Principled BSDF'], color, **kw)
    return m


def layered(name, attr, base, top):
    """Два шейдера на одном меше; граница - изолиния атрибута attr = 0 (ровная кривая, а не ступеньки)."""
    m = material(name, base[0], **base[1])
    nt = m.node_tree
    out = nt.nodes['Material Output']
    bs_a = nt.nodes['Principled BSDF']
    bs_b = nt.nodes.new('ShaderNodeBsdfPrincipled')
    _setup_bsdf(nt, bs_b, top[0], **top[1])
    at = nt.nodes.new('ShaderNodeAttribute')
    at.attribute_name = attr
    edge = nt.nodes.new('ShaderNodeMapRange')
    edge.inputs['From Min'].default_value = -.0015
    edge.inputs['From Max'].default_value = .0015
    nt.links.new(at.outputs['Fac'], edge.inputs['Value'])
    mix = nt.nodes.new('ShaderNodeMixShader')
    nt.links.new(edge.outputs['Result'], mix.inputs['Fac'])
    nt.links.new(bs_a.outputs['BSDF'], mix.inputs[1])
    nt.links.new(bs_b.outputs['BSDF'], mix.inputs[2])
    nt.links.new(mix.outputs['Shader'], out.inputs['Surface'])
    return m


def _setup_bsdf(nt, bs, color, rough=.5, metal=0., var=.12, scale=6., bump=0., bump_scale=120.,
                sss=0., sheen=0., coat=0., emit=None, strength=0., ao=.55, grad=(.62, 1.0), alpha_tr=0., spec=.5, rib=0.):
    bs.inputs['Metallic'].default_value = metal
    bs.inputs['Specular IOR Level'].default_value = spec
    bs.inputs['Roughness'].default_value = rough
    noise = nt.nodes.new('ShaderNodeTexNoise')
    noise.inputs['Scale'].default_value = scale
    noise.inputs['Detail'].default_value = 4
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    dark = tuple(max(0, c * (1 - var)) for c in color)
    light = tuple(min(1, c * (1 + var)) for c in color)
    ramp.color_ramp.elements[0].color = (*dark, 1)
    ramp.color_ramp.elements[1].color = (*light, 1)
    ramp.color_ramp.elements[0].position = .3
    ramp.color_ramp.elements[1].position = .7
    nt.links.new(noise.outputs['Fac'], ramp.inputs[0])
    _gradient_ao(nt, bs.inputs['Base Color'], ramp.outputs['Color'], ao_mix=ao, grad=grad)
    if bump:
        tex = nt.nodes.new('ShaderNodeTexNoise')
        tex.inputs['Scale'].default_value = bump_scale
        tex.inputs['Detail'].default_value = 3
        b = nt.nodes.new('ShaderNodeBump')
        b.inputs['Strength'].default_value = bump
        b.inputs['Distance'].default_value = .002
        nt.links.new(tex.outputs['Fac'], b.inputs['Height'])
        nt.links.new(b.outputs['Normal'], bs.inputs['Normal'])
    if rib:
        wave = nt.nodes.new('ShaderNodeTexWave')
        wave.bands_direction = 'Z'
        wave.inputs['Scale'].default_value = rib
        wave.inputs['Distortion'].default_value = 2
        rb = nt.nodes.new('ShaderNodeBump')
        rb.inputs['Strength'].default_value = .6
        rb.inputs['Distance'].default_value = .004
        nt.links.new(wave.outputs['Fac'], rb.inputs['Height'])
        nt.links.new(rb.outputs['Normal'], bs.inputs['Normal'])
    if sss:
        bs.inputs['Subsurface Weight'].default_value = sss
        bs.inputs['Subsurface Scale'].default_value = .03
    if sheen:
        bs.inputs['Sheen Weight'].default_value = sheen
        bs.inputs['Sheen Tint'].default_value = (*light, 1)
    if coat:
        bs.inputs['Coat Weight'].default_value = coat
        bs.inputs['Coat Roughness'].default_value = .25
    if alpha_tr:
        bs.inputs['Transmission Weight'].default_value = alpha_tr
    if emit:
        bs.inputs['Emission Color'].default_value = (*emit, 1)
        bs.inputs['Emission Strength'].default_value = strength


SKIN = ((.15, .13, .21), dict(rough=.5, var=.1, scale=9, sss=.12, bump=.08, bump_scale=260, ao=.6))
TUNIC = ((.035, .038, .055), dict(rough=.7, var=.12, scale=12, sheen=.12, bump=.08, bump_scale=500, ao=.6))


def make_materials():
    return {
        'skin': material('HX Skin ash-violet', SKIN[0], **SKIN[1]),
        'body': layered('HX Skin + tunic', 'tunic', SKIN, TUNIC),
        'pants': material('HX Cloth charcoal', (.06, .062, .08), rough=.86, var=.18, scale=14, sheen=.18,
                          bump=.12, bump_scale=420, ao=.7, grad=(.75, 1.15)),
        'hood': material('HX Cloth indigo', (.08, .09, .23), rough=.82, var=.22, scale=10, sheen=.2,
                         bump=.10, bump_scale=420, ao=.6),
        'lining': material('HX Lining', (.02, .02, .03), rough=.9, var=.1, ao=.2),
        'orange': material('HX Cloth burnt orange', (.70, .23, .05), rough=.78, var=.18, scale=11, sheen=.18,
                           bump=.10, bump_scale=420, ao=.6),
        'knit': material('HX Knit orange', (.66, .22, .05), rough=.9, var=.2, scale=30, sheen=.2, rib=45, bump=.5,
                         bump_scale=900, ao=.7),
        'linen': material('HX Wraps linen', (.70, .64, .53), rough=.8, var=.2, scale=18, bump=.18,
                          bump_scale=300, ao=.7),
        'bone': material('HX Bone', (.78, .72, .6), rough=.5, var=.3, scale=14, coat=.15, bump=.35,
                         bump_scale=90, ao=.7),
        'lacquer': material('HX Lacquer black', (.025, .025, .03), rough=.3, var=.1, coat=.6, ao=.3),
        'brass': material('HX Brass', (.80, .53, .23), rough=.32, metal=1., var=.15, scale=20, ao=.5),
        'leather': material('HX Leather', (.19, .105, .055), rough=.62, var=.22, scale=12, bump=.12,
                            bump_scale=200, ao=.6),
        'sole': material('HX Sole', (.05, .05, .055), rough=.7, var=.1, ao=.4),
        'wood': material('HX Wood dark', (.12, .07, .045), rough=.6, var=.3, scale=40, ao=.5),
        'steel': material('HX Obsidian steel', (.03, .035, .045), rough=.45, metal=.0, var=.15, ao=.4, spec=.12),
        'crystal': material('HX Crystal', (.1, .55, .5), rough=.15, alpha_tr=.4, emit=(.15, 1., .82),
                            strength=2.5, ao=0),
        'glow': material('HX Glow teal', (.1, .9, .75), rough=.3, emit=(.15, 1., .82), strength=9., ao=0),
        'glow_soft': material('HX Glow teal soft', (.1, .7, .6), rough=.2, emit=(.15, 1., .82), strength=3.5,
                              ao=0),
        'eye': material('HX Eyes', (.6, 1, .9), rough=.3, emit=(.3, 1., .9), strength=25., ao=0),
    }


# ---------------------------------------------------------------- базовая геометрия

def mesh_obj(name, verts, faces, coll, mat=None, smooth=True):
    me = bpy.data.meshes.new(name)
    me.from_pydata([tuple(v) for v in verts], [], faces)
    me.validate()
    me.update()
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    if mat:
        me.materials.append(mat)
    if smooth:
        me.polygons.foreach_set('use_smooth', [True] * len(me.polygons))
    return ob


def recalc_normals(ob):
    bm = bmesh.new()
    bm.from_mesh(ob.data)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(ob.data)
    bm.free()


def drop_loose(ob):
    bm = bmesh.new()
    bm.from_mesh(ob.data)
    bmesh.ops.delete(bm, geom=[v for v in bm.verts if not v.link_faces], context='VERTS')
    loops = boundary_loops(bm)
    bm.to_mesh(ob.data)
    bm.free()
    return loops


def subsurf(ob, lv=2):
    m = ob.modifiers.new('Subdivision', 'SUBSURF')
    m.levels = lv
    m.render_levels = lv
    return m


def solidify(ob, t, offset=-1., mat_offset=0):
    m = ob.modifiers.new('Solidify', 'SOLIDIFY')
    m.thickness = t
    m.offset = offset
    m.use_even_offset = True
    m.material_offset = mat_offset
    return m


def bevel(ob, w, seg=2, angle=40):
    m = ob.modifiers.new('Bevel', 'BEVEL')
    m.width = w
    m.segments = seg
    m.limit_method = 'ANGLE'
    m.angle_limit = math.radians(angle)
    return m


def bake(ob):
    """Применяет модификаторы через evaluated mesh, без операторов контекста."""
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
    old = ob.data
    ob.modifiers.clear()
    ob.data = me
    if old.users == 0:
        bpy.data.meshes.remove(old)
    return ob


def spline(ctrl, n=8, closed=False):
    P = [V(p) for p in ctrl]
    if closed:
        ext = [P[-1]] + P + [P[0], P[1]]
        segs = len(P)
    else:
        ext = [P[0] * 2 - P[1]] + P + [P[-1] * 2 - P[-2]]
        segs = len(P) - 1
    out, ts = [], []
    for i in range(segs):
        p0, p1, p2, p3 = ext[i], ext[i + 1], ext[i + 2], ext[i + 3]
        for j in range(n):
            t = j / n
            out.append(.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t
                             + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
            ts.append((i + t) / segs)
    if not closed:
        out.append(P[-1])
        ts.append(1.)
    return out, ts


def lerp_list(vals, u):
    if len(vals) == 1:
        return vals[0]
    x = u * (len(vals) - 1)
    i = min(int(x), len(vals) - 2)
    f = x - i
    return vals[i] * (1 - f) + vals[i + 1] * f


def frames(pts, up=UP, closed=False):
    n = len(pts)
    T = []
    for i in range(n):
        if closed:
            a, b = pts[i - 1], pts[(i + 1) % n]
        else:
            a, b = pts[max(i - 1, 0)], pts[min(i + 1, n - 1)]
        T.append((b - a).normalized())
    N0 = up - T[0] * up.dot(T[0])
    if N0.length < 1e-4:
        N0 = V((1, 0, 0)) - T[0] * T[0].x
    N = [N0.normalized()]
    for i in range(1, n):
        q = T[i - 1].rotation_difference(T[i])
        v = q @ N[-1]
        v = (v - T[i] * v.dot(T[i])).normalized()
        N.append(v)
    B = [T[i].cross(N[i]).normalized() for i in range(n)]
    return T, N, B


def tube(name, ctrl, radii, coll, mat, segs=14, n=8, sx=1., sy=1., up=UP, rfn=None, caps=(True, True),
         closed=False, sub=1, roll=0.):
    pts, ts = spline(ctrl, n, closed)
    T, N, B = frames(pts, up, closed)
    verts, faces = [], []
    for i, (p, u) in enumerate(zip(pts, ts)):
        r = max(lerp_list(radii, u), .0015)
        for k in range(segs):
            a = 2 * math.pi * k / segs + roll
            rr = rfn(u, a, r) if rfn else r
            verts.append(p + (N[i] * math.cos(a) * sx + B[i] * math.sin(a) * sy) * rr)
    rings = len(pts)
    last = rings if closed else rings - 1
    for i in range(last):
        i2 = (i + 1) % rings
        for k in range(segs):
            k2 = (k + 1) % segs
            faces.append((i * segs + k, i * segs + k2, i2 * segs + k2, i2 * segs + k))
    if not closed:
        if caps[0]:
            verts.append(pts[0] - T[0] * min(lerp_list(radii, 0) * .4, .01))
            c = len(verts) - 1
            faces += [(c, (k + 1) % segs, k) for k in range(segs)]
        if caps[1]:
            verts.append(pts[-1] + T[-1] * min(lerp_list(radii, 1) * .4, .01))
            c = len(verts) - 1
            base = (rings - 1) * segs
            faces += [(c, base + k, base + (k + 1) % segs) for k in range(segs)]
    ob = mesh_obj(name, verts, faces, coll, mat)
    recalc_normals(ob)
    if sub:
        subsurf(ob, sub)
    return ob


def wrap(name, ctrl, radii, coll, mat, turns, width, thick=.006, n=40, up=UP, tilt=.35, phase=0.):
    """Спиральная обмотка поверх трубки; отрицательные turns дают встречное направление."""
    pts, ts = spline(ctrl, n)
    T, N, B = frames(pts, up)
    total = len(pts) - 1
    steps = int(abs(turns) * 28)
    verts, faces = [], []
    for s in range(steps + 1):
        u = s / steps
        x = u * total
        i = min(int(x), total - 1)
        f = x - i
        p = pts[i].lerp(pts[i + 1], f)
        t = T[i].lerp(T[i + 1], f).normalized()
        nn = N[i].lerp(N[i + 1], f).normalized()
        bb = B[i].lerp(B[i + 1], f).normalized()
        a = 2 * math.pi * turns * u + phase
        d = nn * math.cos(a) + bb * math.sin(a)
        r = lerp_list(radii, u) * (1 + .03 * math.sin(a * 3 + u * 7))
        side = (t + d.cross(t) * tilt * (1 if turns > 0 else -1)).normalized() * width / 2
        verts += [p + d * r - side, p + d * r + side, p + d * (r + thick) + side, p + d * (r + thick) - side]
    for s in range(steps):
        a, b = s * 4, (s + 1) * 4
        for k in range(4):
            k2 = (k + 1) % 4
            faces.append((a + k, a + k2, b + k2, b + k))
    faces += [(0, 3, 2, 1), (steps * 4, steps * 4 + 1, steps * 4 + 2, steps * 4 + 3)]
    ob = mesh_obj(name, verts, faces, coll, mat)
    recalc_normals(ob)
    subsurf(ob, 1)
    return ob


def ring(name, center, normal, R, r, coll, mat, segs=32, rsegs=8, flat=1.):
    c = V(center)
    nz = V(normal).normalized()
    nx = nz.orthogonal().normalized()
    ny = nz.cross(nx)
    verts, faces = [], []
    for i in range(segs):
        a = 2 * math.pi * i / segs
        d = nx * math.cos(a) + ny * math.sin(a)
        for j in range(rsegs):
            b = 2 * math.pi * j / rsegs
            verts.append(c + d * (R + r * math.cos(b)) + nz * r * math.sin(b) * flat)
    for i in range(segs):
        for j in range(rsegs):
            i2, j2 = (i + 1) % segs, (j + 1) % rsegs
            faces.append((i * rsegs + j, i2 * rsegs + j, i2 * rsegs + j2, i * rsegs + j2))
    ob = mesh_obj(name, verts, faces, coll, mat)
    recalc_normals(ob)
    return ob


def rounded_box(name, center, size, coll, mat, rot=None, bev=.3, sub=2):
    sx, sy, sz = size
    verts = [V((x * sx / 2, y * sy / 2, z * sz / 2)) for x in (-1, 1) for y in (-1, 1) for z in (-1, 1)]
    faces = [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]
    m = (rot.to_matrix() if rot else Matrix.Identity(3))
    verts = [V(center) + m @ v for v in verts]
    ob = mesh_obj(name, verts, faces, coll, mat)
    recalc_normals(ob)
    bevel(ob, min(size) * bev, seg=3, angle=30)
    if sub:
        subsurf(ob, sub)
    return ob


def crystal(name, center, axis, length, radius, coll, mat, roll=0.):
    ax = V(axis).normalized()
    x = ax.orthogonal().normalized()
    y = ax.cross(x)
    c = V(center)
    verts = [c - ax * length, c + ax * length]
    for k in range(6):
        a = math.pi / 3 * k + roll
        verts.append(c + ax * length * .12 + (x * math.cos(a) + y * math.sin(a)) * radius)
    faces = []
    for k in range(6):
        k2 = (k + 1) % 6
        faces += [(0, 2 + k2, 2 + k), (1, 2 + k, 2 + k2)]
    ob = mesh_obj(name, verts, faces, coll, mat, smooth=False)
    recalc_normals(ob)
    return ob


def bvh_of(objs):
    dg = bpy.context.evaluated_depsgraph_get()
    bm = bmesh.new()
    for ob in objs:
        me = ob.evaluated_get(dg).to_mesh()
        tmp = bmesh.new()
        tmp.from_mesh(me)
        tmp.transform(ob.matrix_world)
        m2 = bpy.data.meshes.new('tmp')
        tmp.to_mesh(m2)
        bm.from_mesh(m2)
        bpy.data.meshes.remove(m2)
        tmp.free()
        ob.evaluated_get(dg).to_mesh_clear()
    tree = BVHTree.FromBMesh(bm)
    bm.free()
    return tree


def strip(name, ctrl, width, thick, coll, mat, tree, off=.004, n=10, closed=False, sub=1, smooth_iter=3):
    """Лента, спроецированная на поверхность (ремни, татуировки, пояс)."""
    pts, _ = spline(ctrl, n, closed)
    for _ in range(2):
        pts = [tree.find_nearest(p)[0] for p in pts]
        cnt = len(pts)
        for _s in range(smooth_iter):
            new = []
            for i in range(cnt):
                if not closed and i in (0, cnt - 1):
                    new.append(pts[i])
                    continue
                new.append((pts[i - 1] + pts[i] * 2 + pts[(i + 1) % cnt]) / 4)
            pts = new
    proj = [tree.find_nearest(p) for p in pts]
    cnt = len(pts)
    verts, faces = [], []
    for i in range(cnt):
        loc, nor = proj[i][0], proj[i][1]
        a = pts[i - 1] if (closed or i > 0) else pts[i]
        b = pts[(i + 1) % cnt] if (closed or i < cnt - 1) else pts[i]
        t = (b - a).normalized()
        s = t.cross(nor).normalized() * width / 2
        base = loc + nor * off
        verts += [base - s, base + s, base + s + nor * thick, base - s + nor * thick]
    last = cnt if closed else cnt - 1
    for i in range(last):
        a, b = i * 4, ((i + 1) % cnt) * 4
        for k in range(4):
            k2 = (k + 1) % 4
            faces.append((a + k, a + k2, b + k2, b + k))
    if not closed:
        e = (cnt - 1) * 4
        faces += [(0, 3, 2, 1), (e, e + 1, e + 2, e + 3)]
    ob = mesh_obj(name, verts, faces, coll, mat)
    recalc_normals(ob)
    if sub:
        subsurf(ob, sub)
    return ob


def boundary_loops(bm):
    edges = [e for e in bm.edges if e.is_boundary]
    adj = {}
    for e in edges:
        a, b = e.verts
        adj.setdefault(a, []).append(b)
        adj.setdefault(b, []).append(a)
    seen, loops = set(), []
    for start in adj:
        if start in seen:
            continue
        loop, prev, cur = [], None, start
        while cur not in seen:
            seen.add(cur)
            loop.append(cur.co.copy())
            nxt = [v for v in adj[cur] if v is not prev and v not in seen]
            if not nxt:
                break
            prev, cur = cur, nxt[0]
        if len(loop) > 8:
            loops.append(loop[::2])
    return loops


def inside(pt, poly):
    x, y = pt
    res = False
    for i in range(len(poly)):
        (x1, y1), (x2, y2) = poly[i - 1], poly[i]
        if (y1 > y) != (y2 > y) and x < (x2 - x1) * (y - y1) / (y2 - y1) + x1:
            res = not res
    return res


# ---------------------------------------------------------------- скелет позы
# Персонаж смотрит в -Y, его левая сторона +X. Метры, пол Z=0. Сутулая хищная стойка.
J = {
    'pelvis': V((0, .02, 1.0)), 'neck': V((0, -.05, 1.69)), 'head': V((0, -.135, 1.855)),
    'r_sh': V((-.27, -.01, 1.625)), 'r_el': V((-.47, -.04, 1.36)), 'r_wr': V((-.54, -.24, 1.15)),
    'l_sh': V((.27, -.01, 1.625)), 'l_el': V((.43, -.03, 1.36)), 'l_wr': V((.37, -.30, 1.42)),
    'l_hip': V((.12, .02, .97)), 'l_kn': V((.22, -.10, .55)), 'l_an': V((.27, -.04, .13)),
    'r_hip': V((-.12, .02, .97)), 'r_kn': V((-.20, .06, .54)), 'r_an': V((-.25, .16, .13)),
    'l_toe': V((.30, -.21, .06)), 'r_toe': V((-.30, -.01, .06)),
}
GLAIVE_A = V((-.64, -.48, .11))
GLAIVE_B = V((-.50, -.10, 2.12))
GLAIVE_D = (GLAIVE_B - GLAIVE_A).normalized()
GRIP = GLAIVE_A + GLAIVE_D * ((1.10 - GLAIVE_A.z) / GLAIVE_D.z)
ORB = V((.31, -.44, 1.535))
MASK_C = J['head'] + V((0, -.118, .0))


# ---------------------------------------------------------------- тело

class Meta:
    def __init__(self, name, coll, res=.009):
        self.mb = bpy.data.metaballs.new(name)
        self.mb.resolution = res
        self.mb.render_resolution = res
        self.ob = bpy.data.objects.new(name, self.mb)
        coll.objects.link(self.ob)

    def ball(self, co, r):
        e = self.mb.elements.new(type='BALL')
        e.co = co
        e.radius = r * K

    def ellip(self, co, dims, rot=None):
        e = self.mb.elements.new(type='ELLIPSOID')
        e.co = co
        e.radius = K
        e.size_x, e.size_y, e.size_z = dims
        if rot:
            e.rotation = rot

    def cap(self, a, b, r):
        a, b = V(a), V(b)
        d = b - a
        e = self.mb.elements.new(type='CAPSULE')
        e.co = (a + b) / 2
        e.size_x = d.length / 2
        e.radius = r * K
        e.rotation = V((1, 0, 0)).rotation_difference(d.normalized())

    def taper(self, a, b, r0, r1, n=4):
        a, b = V(a), V(b)
        for i in range(n):
            u0, u1 = i / n, (i + 1) / n
            self.cap(a.lerp(b, u0), a.lerp(b, u1), r0 + (r1 - r0) * (u0 + u1) / 2)

    def to_mesh(self, name, coll, mat, smooth_iter=4):
        dg = bpy.context.evaluated_depsgraph_get()
        me = bpy.data.meshes.new_from_object(self.ob.evaluated_get(dg))
        bpy.data.objects.remove(self.ob)
        bpy.data.metaballs.remove(self.mb)
        me.name = name
        ob = bpy.data.objects.new(name, me)
        coll.objects.link(ob)
        me.materials.clear()
        me.materials.append(mat)
        me.polygons.foreach_set('use_smooth', [True] * len(me.polygons))
        sm = ob.modifiers.new('Smooth', 'SMOOTH')
        sm.factor = .6
        sm.iterations = smooth_iter
        bake(ob)
        return ob


def rot_to(vec):
    return V((1, 0, 0)).rotation_difference(V(vec).normalized())


def build_body(coll, M):
    m = Meta('HX_BodyMeta', coll)
    # Торс: широкий верх с сутулой спиной, сужение к талии.
    m.ellip((0, -.005, 1.46), (.165, .135, .17))
    m.cap((-.19, .02, 1.585), (.19, .02, 1.585), .085)
    m.ellip((0, .075, 1.56), (.15, .075, .11))
    m.ellip((0, -.08, 1.515), (.145, .055, .08))
    m.ellip((0, -.03, 1.30), (.122, .1, .12))
    m.ellip((0, .0, 1.14), (.13, .1, .1))
    m.ellip((0, .02, 1.0), (.155, .115, .1))
    for s in (-1, 1):
        m.cap((s * .05, -.015, 1.70), (s * .2, .01, 1.655), .05)
        m.ball((s * .27, -.015, 1.63), .08)
        m.ellip((s * .085, .095, .99), (.09, .06, .085))
    m.cap((0, -.035, 1.66), (0, -.1, 1.79), .056)
    m.ellip(J['head'], (.09, .1, .1))
    m.ellip(J['head'] + V((0, -.04, -.06)), (.055, .05, .04))
    for sh, el, wr, s in (('r_sh', 'r_el', 'r_wr', -1), ('l_sh', 'l_el', 'l_wr', 1)):
        a, b, c = J[sh], J[el], J[wr]
        m.taper(a, b, .064, .049)
        m.ellip(a.lerp(b, .5) + V((0, -.028, 0)), (.08, .048, .048), rot_to(b - a))
        m.ellip(a.lerp(b, .55) + V((s * .012, .028, 0)), (.075, .044, .044), rot_to(b - a))
        m.taper(b, c, .055, .036)
        m.ellip(b.lerp(c, .3), (.085, .05, .05), rot_to(c - b))
    for hip, kn, an in (('l_hip', 'l_kn', 'l_an'), ('r_hip', 'r_kn', 'r_an')):
        m.taper(J[hip], J[kn], .08, .055)
        m.taper(J[kn], J[an], .052, .038)
    build_hands(m)
    body = m.to_mesh('HX_Body', coll, M['body'])
    # Облегающий топ без рукавов с высоким воротом: знаковое расстояние до выреза, > 0 внутри топа.
    vals = []
    for v in body.data.vertices:
        x, y, z = v.co
        lim = .2 - max(0., z - 1.56) * .9
        region = max(lim - abs(x), min(.075 - abs(x), y + .13))
        vals.append(min(z - 1.02, 1.75 - z, region))
    body.data.attributes.new('tunic', 'FLOAT', 'POINT').data.foreach_set('value', vals)
    return body


def build_hands(m):
    d = GLAIVE_D
    side = (J['r_wr'] - GRIP)
    side = (side - d * side.dot(d)).normalized()
    palm = GRIP + side * .045
    m.ellip(palm, (.05, .03, .045), rot_to(d))
    m.cap(J['r_wr'], palm, .034)
    ax2 = d.cross(side).normalized()
    for j in range(4):
        base = GRIP + d * (.022 * (1.5 - j))
        r = .028 - abs(j - 1.2) * .002
        prev = None
        for k in range(6):
            a = -.3 + k * .62
            p = base + (side * math.cos(a) + ax2 * math.sin(a)) * (.026 + r * .55)
            if prev is not None:
                m.cap(prev, p, .0135 - k * .0007)
            prev = p
    m.cap(palm + d * .04 + ax2 * .02, GRIP + d * .05 - ax2 * .028 - side * .005, .015)
    wr = J['l_wr']
    fwd = (ORB - wr)
    fwd.z = 0
    fwd.normalize()
    lat = fwd.cross(UP).normalized()
    palm = wr + fwd * .055 + V((0, 0, -.005))
    m.ellip(palm, (.055, .045, .022), rot_to(fwd))
    m.cap(wr, palm, .03)
    for j in range(4):
        o = (j - 1.5) * .022
        b0 = palm + fwd * .045 + lat * o
        pts = [b0, b0 + fwd * .04 + UP * .01 + lat * o * .3, b0 + fwd * .065 + UP * .045 + lat * o * .5,
               b0 + fwd * .07 + UP * .075 + lat * o * .6]
        for p, q in zip(pts, pts[1:]):
            m.cap(p, q, .012)
    t0 = palm - lat * .045
    m.cap(t0, t0 + fwd * .035 - lat * .03 + UP * .03, .013)
    m.cap(t0 + fwd * .035 - lat * .03 + UP * .03, t0 + fwd * .06 - lat * .03 + UP * .06, .011)


def build_feet(coll, M):
    m = Meta('HX_FeetMeta', coll, res=.008)
    for an, toe in (('l_an', 'l_toe'), ('r_an', 'r_toe')):
        a, t = J[an], J[toe]
        m.ball(a + V((0, 0, -.01)), .06)
        mid = a.lerp(t, .55)
        mid.z = .09
        m.ellip(mid, (.13, .07, .06), rot_to(t - a + V((0, 0, -.02))))
        m.ball(V((t.x, t.y, .075)), .055)
    return m.to_mesh('HX_Boots', coll, M['leather'])


# ---------------------------------------------------------------- одежда

def build_pants(coll, M):
    m = Meta('HX_PantsMeta', coll, res=.01)
    m.ellip((0, .02, 1.0), (.178, .135, .11))
    m.ellip((0, .02, 1.085), (.168, .128, .06))
    for hip, kn, an in (('l_hip', 'l_kn', 'l_an'), ('r_hip', 'r_kn', 'r_an')):
        a, b, c = J[hip], J[kn], J[an] + V((0, 0, .06))
        m.taper(a, b, .112, .09, n=5)
        m.ball(b, .1)
        m.taper(b, c, .086, .072, n=4)
        for k in range(3):
            m.ball(c.lerp(b, .08 + k * .1) + V((.006 * (k - 1), .005 * k, 0)), .08)
    ob = m.to_mesh('HX_Pants', coll, M['pants'], smooth_iter=6)
    # Поперечные складки джоггеров: сильнее у щиколотки и под коленом, плюс крупные мягкие объемы.
    for v in ob.data.vertices:
        p, n = v.co, v.normal
        side = J['l_kn'] if p.x > 0 else J['r_kn']
        ang = math.atan2(p.y - side.y, p.x - side.x)
        amp = .005 + .011 * min(1., max(0., (.5 - p.z) / .3)) + .006 * max(0., 1 - abs(p.z - .62) / .12)
        fold = math.sin(p.z * 48 + 2.2 * math.sin(ang * 2 + p.z * 9)) * amp
        bulk = mnoise.noise(p * 3) * .007
        v.co = p + n * (fold + bulk)
    ob.data.update()
    return ob


def build_cloth_sim(coll, colliders, panels, frames=75):
    """Симулирует ткань панелей над коллайдерами, затем запекает форму."""
    scene = bpy.context.scene
    col_objs = []
    for src in colliders:
        c = src.copy()
        c.data = src.data.copy()
        coll.objects.link(c)
        bake(c)
        dec = c.modifiers.new('Decimate', 'DECIMATE')
        dec.ratio = .3
        bake(c)
        c.modifiers.new('Collision', 'COLLISION')
        c.collision.thickness_outer = .012
        c.collision.cloth_friction = 10
        col_objs.append(c)
    for ob, pins in panels:
        vg = ob.vertex_groups.new(name='pin')
        for idx, w in pins:
            vg.add([idx], w, 'REPLACE')
        mod = ob.modifiers.new('Cloth', 'CLOTH')
        s = mod.settings
        s.vertex_group_mass = 'pin'
        s.quality = 10
        s.mass = .3
        s.tension_stiffness = 18
        s.compression_stiffness = 18
        s.shear_stiffness = 6
        s.bending_stiffness = .9
        s.air_damping = 1.5
        mod.collision_settings.distance_min = .01
        mod.collision_settings.collision_quality = 5
        mod.point_cache.frame_start = 1
        mod.point_cache.frame_end = frames
    scene.frame_start = 1
    scene.frame_end = frames
    for f in range(1, frames + 1):
        scene.frame_set(f)
    for ob, _p in panels:
        dg = bpy.context.evaluated_depsgraph_get()
        me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
        old = ob.data
        ob.modifiers.clear()
        ob.data = me
        bpy.data.meshes.remove(old)
        ob.vertex_groups.clear()
    scene.frame_set(1)
    for c in col_objs:
        bpy.data.meshes.remove(c.data)


def skirt_panel(name, coll, mat, center, rx, ry, a0, a1, z_top, length, nx, ny, flare=.3, drop=.0, hem=None,
                slits=()):
    """Ткань как сектор юбки: верх закреплен по дуге (пояс, плечи), книзу шире, чтобы при падении легли складки.

    hem(u) удлиняет низ, slits режут подол на хвосты, drop(u) опускает верхнюю дугу.
    """
    c = V(center)
    verts, faces = [], []
    for j in range(ny + 1):
        v = j / ny
        for i in range(nx + 1):
            u = i / nx
            a = math.radians(a0 + (a1 - a0) * u)
            s = 1 + flare * v
            L = length * (hem(u) if hem else 1)
            z = z_top - (drop(u) if callable(drop) else drop) - L * v
            verts.append(V((c.x + math.cos(a) * rx * s, c.y + math.sin(a) * ry * s, z)))
    slit_cols = {round(s * nx) for s in slits}
    for j in range(ny):
        for i in range(nx):
            # Ровно одна колонка граней на разрез: сравнение по float на границе захватывало две и
            # оставляло среднюю колонку вершин без граней.
            if j > ny * .55 and i in slit_cols:
                continue
            a = j * (nx + 1) + i
            faces.append((a, a + 1, a + nx + 2, a + nx + 1))
    ob = mesh_obj(name, verts, faces, coll, mat)
    pins = [(i, 1.) for i in range(nx + 1)] + [(nx + 1 + i, .45) for i in range(nx + 1)]
    return ob, pins


def add_folds(ob, nx, ny, count, amp):
    """Вертикальные складки, расходящиеся от линии крепления; порядок вершин сетки сохранен симуляцией."""
    me = ob.data
    for v in me.vertices:
        u = (v.index % (nx + 1)) / nx
        w = (v.index // (nx + 1)) / ny
        f = math.sin(u * 2 * math.pi * count + 1.7 * math.sin(u * 9)) * amp * w ** 1.1
        v.co = v.co + v.normal * f
    me.update()


def build_hood(coll, M):
    hc = J['head'] + V((0, .02, .03))
    nu, nv = 56, 32
    verts = []
    for j in range(nv + 1):
        phi = math.pi * j / nv
        for i in range(nu):
            th = 2 * math.pi * i / nu
            d = V((math.sin(phi) * math.cos(th), math.sin(phi) * math.sin(th), math.cos(phi)))
            p = V((d.x * .172, d.y * .19, d.z * .18))
            tip = max(0., d.dot(V((0, .85, .5)).normalized())) ** 4
            p += V((0, .1, -.03)) * tip
            if d.y < 0 and d.z > 0:
                p += V((0, -.04, .0)) * min(1, (-d.y) * (d.z + .3))
            if d.z < -.25:
                p += V((d.x * .1, d.y * .06, 0)) * (-d.z - .25)
            verts.append(hc + p)
    faces = []
    for j in range(nv):
        for i in range(nu):
            i2 = (i + 1) % nu
            ids = (j * nu + i, j * nu + i2, (j + 1) * nu + i2, (j + 1) * nu + i)
            c = sum((verts[k] for k in ids), V()) / 4
            front = c.y < hc.y - .06
            ex = (c.x / .118) ** 2 + ((c.z - MASK_C.z + .012) / .155) ** 2
            if (front and ex < 1) or c.z < hc.z - .17:
                continue
            faces.append(ids)
    ob = mesh_obj('HX_Hood', verts, faces, coll, M['hood'])
    loops = drop_loose(ob)
    recalc_normals(ob)
    ob.data.materials.append(M['lining'])
    solidify(ob, .016, mat_offset=1)
    subsurf(ob, 2)
    front = min(loops, key=lambda lp: sum(p.y for p in lp) / len(lp))
    for _ in range(4):
        front = [(front[i - 1] + front[i] * 2 + front[(i + 1) % len(front)]) / 4 for i in range(len(front))]
    trim = tube('HX_Hood_Trim', front, [.01], coll, M['brass'], segs=8, n=2, closed=True, sub=1)
    return [ob, trim]


def build_snood(coll, M):
    pts = []
    for i in range(18):
        a = 2 * math.pi * i / 18
        pts.append(V((math.cos(a) * .18, -.035 + math.sin(a) * .155, 1.70 + .035 * math.sin(a))))

    def knit(u, a, r):
        return r * (1 + .16 * math.sin(u * 2 * math.pi * 11) ** 2 + .05 * math.sin(a * 2 + u * 20))
    return tube('HX_Snood', pts, [.052], coll, M['knit'], segs=18, n=4, closed=True, rfn=knit, sub=1, sx=.8)


# ---------------------------------------------------------------- голова: маска, клыки, уши

def mask_surface(x, z):
    rc = .1
    return MASK_C + V((x, rc - math.sqrt(max(rc * rc - x * x, 1e-6)) + .9 * z * z, z))


def build_mask(coll, M):
    objs = []
    half = [(0, .105), (.032, .098), (.062, .122), (.083, .08), (.096, .03), (.09, -.02), (.072, -.06),
            (.046, -.1), (.02, -.135), (0, -.152)]
    outline = half + [(-x, z) for x, z in reversed(half[1:-1])]
    eye_r = [(.016, .036), (.07, .056), (.074, .04), (.024, .018)]
    eyes = [eye_r, [(-x, z) for x, z in eye_r]]
    loop, _ = spline([V((x, 0, z)) for x, z in outline], 5, closed=True)
    bm = bmesh.new()
    edges = []
    for poly in [[(p.x, p.z) for p in loop]] + [[(p.x, p.z) for p in spline([V((x, 0, z)) for x, z in e], 3,
                                                                                closed=True)[0]] for e in eyes]:
        vs = [bm.verts.new((x, 0, z)) for x, z in poly]
        for i in range(len(vs)):
            edges.append(bm.edges.new((vs[i], vs[(i + 1) % len(vs)])))
    bmesh.ops.triangle_fill(bm, use_beauty=True, use_dissolve=False, edges=edges, normal=(0, -1, 0))
    eye_polys = [[(p.x, p.z) for p in spline([V((x, 0, z)) for x, z in e], 3, closed=True)[0]] for e in eyes]
    bmesh.ops.delete(bm, geom=[f for f in bm.faces
                               if any(inside((f.calc_center_median().x, f.calc_center_median().z), ep)
                                      for ep in eye_polys)], context='FACES')
    bmesh.ops.subdivide_edges(bm, edges=bm.edges[:], cuts=3, use_grid_fill=True)
    for v in bm.verts:
        v.co = mask_surface(v.co.x, v.co.z)
    me = bpy.data.meshes.new('HX_Mask')
    bm.to_mesh(me)
    bm.free()
    mask = bpy.data.objects.new('HX_Mask', me)
    coll.objects.link(mask)
    me.materials.append(M['bone'])
    me.polygons.foreach_set('use_smooth', [True] * len(me.polygons))
    if sum(p.normal.y for p in me.polygons) > 0:
        me.flip_normals()
    solidify(mask, .016, offset=-1)
    bevel(mask, .004, seg=2, angle=25)
    objs.append(mask)
    # Рельеф тотема, а не лица: единая гребенчатая дуга над прорезями и резные черные бороздки.
    crown = [mask_surface(x, z) + V((0, -.011, 0)) for x, z in
             ((-.088, .088), (-.05, .074), (-.018, .08), (0, .102), (.018, .08), (.05, .074), (.088, .088))]
    objs.append(tube('HX_Mask_Crown', crown, [.004, .011, .012, .014, .012, .011, .004], coll, M['bone'], segs=10,
                     n=5))
    for s in (-1, 1):
        for k, (x0, x1) in enumerate(((.03, .05), (.055, .078))):
            groove = [mask_surface(s * x0, -.02 - k * .01) + V((0, -.004, 0)),
                      mask_surface(s * x1, -.06 - k * .015) + V((0, -.004, 0))]
            objs.append(tube(f'HX_Mask_Groove_{s}_{k}', groove, [.0035, .002], coll, M['lacquer'], segs=6,
                             sub=0))
        for k in range(2):
            a = mask_surface(s * (.03 + k * .018), -.045 - k * .006) + V((0, -.012, 0))
            b = mask_surface(s * (.032 + k * .02), -.09 - k * .01) + V((0, -.009, 0))
            objs.append(tube(f'HX_Mask_Mark_{s}_{k}', [a, b], [.004, .0025], coll, M['glow_soft'], segs=6,
                             sub=0))
        ep = [mask_surface(s * x, z) + V((0, .012, 0)) for x, z in eye_r]
        c = sum(ep, V()) / 4
        eye = mesh_obj(f'HX_Eye_{s}', [c + (p - c) * 2.2 for p in ep], [(0, 1, 2, 3)], coll, M['eye'],
                       smooth=False)
        objs.append(eye)
    split = [mask_surface(0, z) + V((0, -.004, 0)) for z in (.07, .0, -.075)]
    objs.append(tube('HX_Mask_Split', split, [.003, .004, .003], coll, M['lacquer'], segs=6, sub=0))
    stripe = [mask_surface(0, z) + V((0, -.004, 0)) for z in (.1, .085)]
    objs.append(tube('HX_Mask_Rune', stripe, [.006, .003], coll, M['glow_soft'], segs=6, sub=0))
    chin = [mask_surface(0, z) + V((0, -.006, 0)) for z in (-.08, -.14)]
    objs.append(tube('HX_Mask_ChinStripe', chin, [.012, .004], coll, M['lacquer'], segs=8, sy=.4,
                     up=V((0, -1, 0))))
    return objs


def build_head_extras(coll, M):
    objs = []
    hc = J['head']
    for s in (-1, 1):
        # Клыки снизу из-под маски, вверх перед скулами.
        pts = [MASK_C + V(p) for p in ((s * .045, .03, -.14), (s * .095, .005, -.115), (s * .13, -.005, -.05),
                                       (s * .14, .015, .015))]
        objs.append(tube(f'HX_Tusk_{s}', pts, [.027, .023, .014, .003], coll, M['bone'], segs=12, n=8))
        objs.append(ring(f'HX_TuskBand_{s}', pts[0].lerp(pts[1], .5), pts[1] - pts[0], .027, .006, coll,
                         M['brass'], segs=16, rsegs=6))
        e0 = hc + V((s * .085, .01, .0))
        ear = [e0, e0 + V((s * .09, .03, .03)), e0 + V((s * .18, .08, .04)), e0 + V((s * .27, .13, .02))]
        objs.append(tube(f'HX_Ear_{s}', ear, [.034, .044, .03, .004], coll, M['skin'], segs=14, n=8,
                         sx=1., sy=.32, up=V((0, -.2, 1))))
        for k, u in enumerate((.45, .6)):
            p = e0.lerp(ear[2], u) + V((0, 0, .03 - k * .005))
            objs.append(ring(f'HX_EarRing_{s}_{k}', p, ear[2] - e0, .012 - k * .001, .0035, coll, M['brass'],
                             segs=20, rsegs=6))
    return objs


# ---------------------------------------------------------------- снаряжение

def build_gear(coll, M, body, pants):
    objs = []
    tree = bvh_of([body])
    cross = V((0, -.17, 1.38))
    for s in (-1, 1):
        objs.append(strip(f'HX_Harness_F_{s}', [(s * .15, -.05, 1.72), (s * .1, -.12, 1.58), cross,
                                               (-s * .1, -.14, 1.2), (-s * .15, -.12, 1.08)],
                          .034, .007, coll, M['leather'], tree, n=10))
        objs.append(strip(f'HX_Harness_B_{s}', [(s * .15, .03, 1.72), (s * .1, .14, 1.56), (0, .16, 1.38),
                                               (-s * .1, .13, 1.2), (-s * .15, .1, 1.08)],
                          .034, .007, coll, M['leather'], tree, n=10))
        for z in (1.55, 1.24):
            loc, nor, _i, _d = tree.find_nearest(V((s * .1 if z > 1.4 else -s * .1, -.14, z)))
            objs.append(rounded_box(f'HX_Harness_Buckle_{s}_{z}', loc + nor * .012, (.034, .012, .02), coll,
                                    M['brass'], rot=V((0, -1, 0)).rotation_difference(nor), bev=.2, sub=1))
    loc, nor, _i, _d = tree.find_nearest(cross)
    objs.append(ring('HX_Harness_Ring', loc + nor * .016, nor, .03, .008, coll, M['brass'], flat=.8))
    objs.append(crystal('HX_Harness_Gem', loc + nor * .018, nor, .022, .02, coll, M['glow']))
    ptree = bvh_of([pants])
    belt_ctrl = [(math.cos(a) * .2, .02 + math.sin(a) * .16, 1.07 - .015 * math.sin(a))
                 for a in [2 * math.pi * i / 16 for i in range(16)]]
    objs.append(strip('HX_Belt', belt_ctrl, .06, .012, coll, M['leather'], ptree, n=6, closed=True))
    bl, bn, _i, _d = ptree.find_nearest(V((0, -.2, 1.07)))
    objs.append(ring('HX_Belt_Buckle', bl + bn * .022, bn, .036, .01, coll, M['brass'], segs=6, rsegs=6))
    objs.append(crystal('HX_Belt_Gem', bl + bn * .024, bn, .026, .026, coll, M['glow']))
    for s, y in ((-1, -.06), (1, .06)):
        pl, pn, _i, _d = ptree.find_nearest(V((s * .22, y, 1.0)))
        rot = V((0, -1, 0)).rotation_difference(pn)
        objs.append(rounded_box(f'HX_Pouch_{s}', pl + pn * .03, (.1, .055, .11), coll, M['leather'], rot=rot))
        objs.append(rounded_box(f'HX_PouchFlap_{s}', pl + pn * .035 + V((0, 0, .045)), (.108, .064, .03), coll,
                                M['leather'], rot=rot))
    for k in range(3):
        top, _n, _i, _d = ptree.find_nearest(V((.13 + k * .03, -.15 + k * .03, 1.04)))
        top = top + _n * .015
        bot = top + V((.01, -.015, -.12 - k * .035))
        objs.append(tube(f'HX_Charm_Cord_{k}', [top, bot], [.003], coll, M['leather'], segs=6, sub=0))
        objs.append(crystal(f'HX_Charm_{k}', bot - V((0, 0, .022)), (0, 0, 1), .024, .011, coll,
                            M['crystal'] if k == 1 else M['bone'], roll=k))
    objs += build_pauldron(coll, M)
    objs += build_wraps(coll, M)
    objs += build_soles(coll, M)
    objs += build_tattoos(coll, M, tree)
    return objs


def cap_shell(name, center, axis, R, ang, coll, mat, n_r=12, n_a=40):
    c, ax = V(center), V(axis).normalized()
    x = ax.orthogonal().normalized()
    y = ax.cross(x)
    verts, faces = [V(c + ax * R)], []
    for j in range(1, n_r + 1):
        t = ang * j / n_r
        for i in range(n_a):
            ph = 2 * math.pi * i / n_a
            verts.append(c + (ax * math.cos(t) + (x * math.cos(ph) + y * math.sin(ph)) * math.sin(t)) * R)
    for i in range(n_a):
        faces.append((0, 1 + i, 1 + (i + 1) % n_a))
    for j in range(n_r - 1):
        for i in range(n_a):
            i2 = (i + 1) % n_a
            a, b = 1 + j * n_a, 1 + (j + 1) * n_a
            faces.append((a + i, b + i, b + i2, a + i2))
    ob = mesh_obj(name, verts, faces, coll, mat)
    loops = drop_loose(ob)
    recalc_normals(ob)
    return ob, loops


def build_pauldron(coll, M):
    objs = []
    sh = J['l_sh'] + V((.02, .0, .02))
    plates = [((.0, 0, .0), (.45, 0, 1), .135, 62),
              ((.03, 0, -.045), (.85, 0, .55), .14, 48),
              ((.05, 0, -.095), (1, 0, .2), .135, 40)]
    for k, (off, axis, R, ang) in enumerate(plates):
        ob, loops = cap_shell(f'HX_Pauldron_{k}', sh + V(off), axis, R, math.radians(ang), coll, M['bone'])
        solidify(ob, .014, offset=1)
        bevel(ob, .004, seg=2, angle=30)
        subsurf(ob, 2)
        objs.append(ob)
        if loops:
            objs.append(tube(f'HX_Pauldron_Rim_{k}', max(loops, key=len), [.008], coll, M['brass'], segs=8, n=1,
                             closed=True, sub=1))
    # Один мощный изогнутый рог задает силуэт плеча.
    p0 = sh + V((.04, .02, .11))
    horn = [p0, p0 + V((.05, .05, .1)), p0 + V((.05, .13, .2)), p0 + V((.0, .22, .26))]
    objs.append(tube('HX_Pauldron_Horn', horn, [.042, .032, .017, .003], coll, M['bone'], segs=14, n=10))
    objs.append(ring('HX_Horn_Band', p0.lerp(horn[1], .3), horn[1] - p0, .04, .008, coll, M['brass'], segs=20,
                     rsegs=6))
    top = sh + V((.45, 0, 1)).normalized() * .152
    rune = [top + V((-.03, -.06, .015)), top + V((.0, -.02, .005)), top + V((-.02, .03, .01)),
            top + V((.01, .07, -.005))]
    objs.append(tube('HX_Pauldron_Rune', rune, [.005], coll, M['glow'], segs=6, sub=0))
    # Ремень наплечника под мышку.
    return objs


def build_wraps(coll, M):
    objs = []
    for side, kn, an in (('L', 'l_kn', 'l_an'), ('R', 'r_kn', 'r_an')):
        a, b = J[kn].lerp(J[an], .38), J[an] + V((0, 0, .03))
        objs.append(wrap(f'HX_ShinWrap_{side}', [a, b], [.106, .096], coll, M['linen'], turns=6, width=.062))
    for side, el, wr in (('R', 'r_el', 'r_wr'), ('L', 'l_el', 'l_wr')):
        a, b = J[el].lerp(J[wr], .3), J[wr] + (J[wr] - J[el]).normalized() * .05
        objs.append(wrap(f'HX_ArmWrap_{side}', [a, b], [.046, .036], coll, M['linen'], turns=6, width=.04,
                         thick=.005))
        objs.append(wrap(f'HX_ArmWrapX_{side}', [a.lerp(b, .15), b], [.051, .041], coll, M['linen'], turns=-3,
                         width=.03, thick=.004, phase=.8))
        objs.append(ring(f'HX_Cuff_{side}', a.lerp(b, .08), b - a, .05, .01, coll, M['brass'], segs=24,
                         rsegs=8, flat=1.8))
    return objs


def build_soles(coll, M):
    objs = []
    for side, an, toe in (('L', 'l_an', 'l_toe'), ('R', 'r_an', 'r_toe')):
        a, t = J[an], J[toe]
        d = V((t.x - a.x, t.y - a.y, 0))
        dn = d.normalized()
        heel = V((a.x, a.y, 0)) - dn * .065
        tip = V((t.x, t.y, 0)) + dn * .055
        c = (heel + tip) / 2
        c.z = .0375
        rot = V((0, 1, 0)).rotation_difference(dn)
        L = (tip - heel).length
        objs.append(rounded_box(f'HX_Sole_{side}', c, (.14, L, .075), coll, M['sole'], rot=rot, bev=.35))
        objs.append(rounded_box(f'HX_SoleStripe_{side}', c + V((0, 0, .008)), (.14, L * .92, .013), coll,
                                M['orange'], rot=rot, bev=.45))
        objs.append(wrap(f'HX_FootWrap_{side}', [a.lerp(t, .2) + V((0, 0, -.02)), a.lerp(t, .6)
                                                 + V((0, 0, -.01))],
                         [.066, .068], coll, M['linen'], turns=2.5, width=.04, thick=.005))
    return objs


def build_tattoos(coll, M, tree):
    objs = []
    a, b = J['r_sh'], J['r_el']
    for k in range(3):
        p = a.lerp(b, .25 + k * .2)
        objs.append(strip(f'HX_Tattoo_Arm_{k}', [p + V((.03, -.07, .03)), p + V((-.035, -.06, -.02)),
                                                p + V((-.07, .0, .025))],
                          .009, .0012, coll, M['glow_soft'], tree, off=.0015, n=8, sub=0))
    a, b = J['l_el'], J['l_sh']
    for k in range(2):
        p = a.lerp(b, .35 + k * .22)
        objs.append(strip(f'HX_Tattoo_ArmL_{k}', [p + V((-.03, -.07, .02)), p + V((.03, -.065, -.02))],
                          .008, .0012, coll, M['glow_soft'], tree, off=.0015, n=8, sub=0))
    return objs


# ---------------------------------------------------------------- оружие и магия

def outline_mesh(name, pts2d, origin, u_axis, v_axis, coll, mat, thick, edge=.025):
    """Клинок по контуру с заточкой: фаски шириной edge сходятся в острую кромку с обеих сторон."""
    o, u, v = V(origin), V(u_axis), V(v_axis)
    n = u.cross(v).normalized()
    loop, _ = spline([o + u * x + v * y for x, y in pts2d], 6, closed=True)
    bm = bmesh.new()
    vs = [bm.verts.new(p) for p in loop]
    edges = [bm.edges.new((vs[i], vs[(i + 1) % len(vs)])) for i in range(len(vs))]
    bmesh.ops.triangle_fill(bm, use_beauty=True, use_dissolve=False, edges=edges, normal=n)
    bmesh.ops.inset_region(bm, faces=bm.faces[:], thickness=edge, depth=thick / 2, use_even_offset=True)
    dup = bmesh.ops.duplicate(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:])
    for e in dup['geom']:
        if isinstance(e, bmesh.types.BMVert):
            e.co -= n * (2 * (e.co - o).dot(n))
    bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=1e-6)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    me.materials.append(mat)
    return ob


def build_glaive(coll, M):
    objs = []
    A, B, D = GLAIVE_A, GLAIVE_B, GLAIVE_D
    S = D.cross(V((0, 1, 0))).normalized()
    objs.append(tube('HX_Glaive_Shaft', [A + D * .05, B - D * .05], [.02, .021, .019], coll, M['wood'], segs=12,
                     n=2))
    objs.append(wrap('HX_Glaive_Grip', [GRIP - D * .16, GRIP + D * .14], [.022, .022], coll, M['leather'],
                     turns=8, width=.03, thick=.004, up=S))
    for k, u in enumerate((.04, .3, .62, .9)):
        p = A.lerp(B, u)
        objs.append(tube(f'HX_Glaive_Collar_{k}', [p - D * .025, p + D * .025], [.027, .03, .027], coll,
                         M['brass'], segs=14, n=2))
    top = B - D * .06
    big = [(-.01, -.01), (-.09, .0), (-.2, .05), (-.3, .14), (-.36, .27), (-.36, .4), (-.31, .53), (-.27, .45),
           (-.25, .34), (-.21, .25), (-.14, .18), (-.06, .15), (.0, .13)]
    objs.append(outline_mesh('HX_Glaive_Blade', big, top, S, D, coll, M['steel'], .022))
    edge = [top + S * x + D * y + V((0, -.002, 0)) for x, y in
            [(-.08, .03), (-.19, .075), (-.28, .16), (-.33, .27), (-.33, .39), (-.3, .48)]]
    objs.append(tube('HX_Glaive_Edge', edge, [.005, .012, .014, .012, .005], coll, M['glow'], segs=8, n=6))
    objs.append(outline_mesh('HX_Glaive_Spike', [(-.024, .1), (-.03, .2), (0, .3), (.03, .2), (.024, .1)], top,
                             S, D, coll, M['steel'], .022, edge=.011))
    objs.append(outline_mesh('HX_Glaive_Hook', [(.01, .02), (.09, .045), (.15, .11), (.14, .18), (.105, .11),
                                                (.035, .085)], top, S, D, coll, M['steel'], .016, edge=.014))
    objs.append(tube('HX_Glaive_Socket', [top - D * .04, top + D * .12], [.032, .037, .024], coll, M['brass'],
                     segs=6, n=3, sub=0))
    objs.append(crystal('HX_Glaive_SocketGem', top + D * .04 - V((0, .036, 0)), (0, -1, 0), .02, .017, coll,
                        M['glow']))
    bot = A + D * .04
    objs.append(outline_mesh('HX_Glaive_Tail', [(-.02, 0), (-.026, -.08), (0, -.15), (.026, -.08), (.02, 0)],
                             bot, S, D, coll, M['steel'], .02, edge=.01))
    r0 = top - D * .08 + S * .02
    objs.append(tube('HX_Glaive_Ribbon', [r0, r0 + V((.02, -.03, -.12)), r0 + V((.06, -.02, -.26)),
                                          r0 + V((.05, .02, -.4))],
                     [.02, .022, .024, .018], coll, M['orange'], segs=8, sx=1, sy=.15, up=V((0, -1, 0))))
    return objs


def build_magic(coll, M):
    objs = []
    objs.append(tube('HX_Orb', [ORB - V((0, 0, .055)), ORB + V((0, 0, .055))], [.002, .05, .055, .05, .002],
                     coll, M['glow'], segs=16, n=4, sub=2))
    objs.append(ring('HX_Orb_Ring_A', ORB, (.3, .2, 1), .095, .004, coll, M['glow_soft'], segs=40, rsegs=6))
    objs.append(ring('HX_Orb_Ring_B', ORB, (-.6, .5, .4), .085, .003, coll, M['glow_soft'], segs=40, rsegs=6))
    for k in range(3):
        a = 2 * math.pi * k / 3 + .4
        p = ORB + V((math.cos(a) * .11, math.sin(a) * .11, .03 * math.sin(a * 2)))
        objs.append(crystal(f'HX_Orb_Shard_{k}', p, (math.cos(a), math.sin(a), .6), .022, .009, coll,
                            M['crystal'], roll=k))
    # Нимб: цельное кольцо и три кристалла на нем.
    hc = V((0, .31, 1.93))
    nrm = V((0, 1, .22)).normalized()
    objs.append(ring('HX_Halo', hc, nrm, .34, .0065, coll, M['glow'], segs=72, rsegs=8))
    objs.append(ring('HX_Halo_Inner', hc + nrm * .01, nrm, .3, .0025, coll, M['glow_soft'], segs=72, rsegs=6))
    x = V((1, 0, 0))
    y = nrm.cross(x).normalized()
    for k, a in enumerate((math.pi / 2, math.pi / 2 + 2.2, math.pi / 2 - 2.2)):
        d = x * math.cos(a) + y * math.sin(a)
        objs.append(crystal(f'HX_Halo_Crystal_{k}', hc + d * .34, d, .09 if k == 0 else .065, .032,
                            coll, M['crystal'], roll=.3))
    return objs


# ---------------------------------------------------------------- студия

def build_studio(scene):
    studio = bpy.data.collections.new('STUDIO')
    scene.collection.children.link(studio)
    # Циклорама: пол плавно переходит в стену за героем, без линии горизонта.
    prof = [(-25., 0.)] + [(3 + 3 * math.cos(a), 3 + 3 * math.sin(a))
                           for a in [math.radians(-90 + 90 * k / 16) for k in range(17)]] + [(6., 14.)]
    verts, faces = [], []
    for x in (-20, 20):
        verts += [(x, y, z) for y, z in prof]
    n = len(prof)
    faces = [(i, i + 1, n + i + 1, n + i) for i in range(n - 1)]
    floor = mesh_obj('Studio_Cyclorama', verts, faces, studio,
                     material('Studio floor', (.022, .025, .036), rough=.8, var=.05, ao=.0, grad=(1, 1)))
    recalc_normals(floor)
    floor.hide_select = True
    world = bpy.data.worlds.new('Studio world')
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (.03, .038, .06, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = .7
    scene.world = world
    add_area(studio, 'Key warm', (-2.6, -3.6, 4.2), (0, 0, 1.2), 520, (1, .86, .72), 3)
    add_area(studio, 'Rim teal', (2.4, 2.8, 3.0), (0, 0, 1.4), 1500, (.35, .95, .9), 2)
    add_area(studio, 'Rim violet', (-2.6, 2.4, 2.6), (0, 0, 1.3), 1100, (.6, .45, 1), 2)
    add_area(studio, 'Fill', (3, -3, 1.6), (0, 0, 1.0), 150, (.7, .8, 1), 4)
    cam_data = bpy.data.cameras.new('Camera')
    cam_data.lens = 60
    cam = bpy.data.objects.new('Camera', cam_data)
    studio.objects.link(cam)
    cam.location = (2.3, -5.6, 2.05)
    point_at(cam, (0, -.05, 1.18))
    scene.camera = cam


def set_viewports():
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type != 'VIEW_3D':
                continue
            sp = area.spaces.active
            sp.shading.type = 'MATERIAL'
            sp.shading.use_scene_lights = True
            sp.shading.use_scene_world = True
            sp.region_3d.view_perspective = 'CAMERA'


# ---------------------------------------------------------------- сборка

HIP_Z = .97
LEG_STRETCH = 1.1


def warp_proportions(ob, rigid=False):
    """Долговязость и сутулость поверх собранной фигуры: ноги длиннее, верх корпуса и голова уходят вперед.

    Прямое оружие только масштабируется по Z, иначе излом на линии бедер согнул бы древко.
    """
    for v in ob.data.vertices:
        x, y, z = v.co
        if rigid:
            v.co.z = z * LEG_STRETCH
            continue
        nz = z * LEG_STRETCH if z < HIP_Z else z + HIP_Z * (LEG_STRETCH - 1)
        t = min(1., max(0., (z - 1.35) / .5))
        v.co = V((x, y - .075 * t * t * (3 - 2 * t), nz))
    ob.data.update()


def main():
    if not bpy.app.background:
        raise RuntimeError('Run in background Blender.')
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.use_preferences_save = False
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    hero = bpy.data.collections.new('HEXBLADE')
    scene.collection.children.link(hero)
    groups = {n: bpy.data.collections.new(f'HX {n}') for n in ('Body', 'Cloth', 'Gear', 'Weapon', 'Magic')}
    for c in groups.values():
        hero.children.link(c)
    M = make_materials()

    body = build_body(groups['Body'], M)
    boots = build_feet(groups['Body'], M)
    pants = build_pants(groups['Cloth'], M)
    build_mask(groups['Gear'], M)
    build_head_extras(groups['Gear'], M)
    build_hood(groups['Cloth'], M)
    build_snood(groups['Cloth'], M)
    gear = build_gear(groups['Gear'], M, body, pants)
    build_glaive(groups['Weapon'], M)
    build_magic(groups['Magic'], M)

    cape, cape_pins = skirt_panel('HX_Cape', groups['Cloth'], M['hood'], (0, .0, 0), .235, .2, 30, 165, 1.72,
                                  1.0, 30, 40, flare=.55, drop=lambda u: .06 * math.sin(math.pi * u),
                                  hem=lambda u: 1 + .12 * (1 - abs(u - .5) * 2) ** 2)
    front, front_pins = skirt_panel('HX_Loincloth_Front', groups['Cloth'], M['orange'], (0, .02, 0), .2, .2,
                                    238, 302, 1.06, .62, 14, 30, flare=.6,
                                    hem=lambda u: 1 + .18 * (1 - abs(u - .5) * 2))
    back, back_pins = skirt_panel('HX_Loincloth_Back', groups['Cloth'], M['hood'], (0, .02, 0), .2, .17, 58, 122,
                                  1.06, .62, 14, 28, flare=.5, hem=lambda u: 1 + .14 * (1 - abs(u - .5) * 2))
    for ob in hero.all_objects:
        if ob.type == 'MESH':
            warp_proportions(ob, rigid=ob.users_collection[0] == groups['Weapon'])
    colliders = [body, pants, boots] + [g for g in gear if g.name.startswith(('HX_Pauldron_', 'HX_Pouch_'))]
    build_cloth_sim(groups['Cloth'], colliders, [(cape, cape_pins), (front, front_pins), (back, back_pins)])
    for ob, t, grid, count, amp in ((cape, .01, (30, 40), 7, .026), (front, .008, (14, 26), 4, .022),
                                    (back, .008, (14, 28), 4, .022)):
        add_folds(ob, *grid, count, amp)
        ob.data.polygons.foreach_set('use_smooth', [True] * len(ob.data.polygons))
        solidify(ob, t, offset=0)
        subsurf(ob, 1)

    build_studio(scene)
    set_viewports()
    OUT.mkdir(parents=True, exist_ok=True)
    path = OUT / 'hexblade.blend'
    bpy.ops.wm.save_as_mainfile(filepath=str(path))
    dg = bpy.context.evaluated_depsgraph_get()
    tris = 0
    for ob in hero.all_objects:
        if ob.type == 'MESH':
            me = ob.evaluated_get(dg).to_mesh()
            me.calc_loop_triangles()
            tris += len(me.loop_triangles)
            ob.evaluated_get(dg).to_mesh_clear()
    print('HEXBLADE_BUILD ' + json.dumps({'path': str(path), 'objects': len(hero.all_objects),
                                          'evaluated_triangles': tris}))


if __name__ == '__main__':
    main()

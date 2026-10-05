"""Собирает нефритового скорпиона (рабочее название) в art/creatures/jade-scorpion/jade-scorpion.blend.

Запуск только отдельным background-процессом из корня game:
blender --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 --python tools/scorpion_build.py
"""
import json
import math
import random
import sys
import zlib
from pathlib import Path

import bmesh
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import add_area, point_at  # noqa: E402

OUT = ROOT / 'art/creatures/jade-scorpion'
V = Vector
UP = V((0, 0, 1))
SIDES = (-1, 1)


# ---------------------------------------------------------------- материалы

def _node(nt, kind, **inputs):
    n = nt.nodes.new(kind)
    for k, v in inputs.items():
        n.inputs[k].default_value = v
    return n


def _mix(nt, a, b, fac, blend='MIX'):
    # У ShaderNodeMix цветовые сокеты: inputs 6/7, output 2.
    m = nt.nodes.new('ShaderNodeMix')
    m.data_type = 'RGBA'
    m.blend_type = blend
    if isinstance(fac, float):
        m.inputs[0].default_value = fac
    else:
        nt.links.new(fac, m.inputs[0])
    for sock, val in ((m.inputs[6], a), (m.inputs[7], b)):
        if isinstance(val, tuple):
            sock.default_value = (*val, 1)
        else:
            nt.links.new(val, sock)
    return m.outputs[2]


def jade_material():
    """Камуфляжные пятна нефрита, светлые сколы по ребрам граней, темные кончики по атрибуту tip."""
    m = bpy.data.materials.new('JS Jade camo')
    m.use_nodes = True
    m.diffuse_color = (.35, .45, .25, 1)
    nt = m.node_tree
    L = nt.links
    bs = nt.nodes['Principled BSDF']
    bs.inputs['Roughness'].default_value = .45
    bs.inputs['Coat Weight'].default_value = .3
    bs.inputs['Coat Roughness'].default_value = .35

    # Геометрия собрана в мировых координатах при единичных трансформах, поэтому узор непрерывен между деталями.
    coord = nt.nodes.new('ShaderNodeTexCoord')
    warp = _node(nt, 'ShaderNodeTexNoise', Scale=2.2, Detail=3.)
    wsc = nt.nodes.new('ShaderNodeVectorMath')
    wsc.operation = 'MULTIPLY_ADD'
    wsc.inputs[1].default_value = (.45, .45, .45)
    wsc.inputs[2].default_value = (-.22, -.22, -.22)
    L.new(warp.outputs['Color'], wsc.inputs[0])
    add = nt.nodes.new('ShaderNodeVectorMath')
    add.operation = 'ADD'
    L.new(coord.outputs['Object'], add.inputs[0])
    L.new(wsc.outputs[0], add.inputs[1])
    vor = _node(nt, 'ShaderNodeTexVoronoi', Scale=4.5)
    L.new(add.outputs[0], vor.inputs['Vector'])
    sep = nt.nodes.new('ShaderNodeSeparateColor')
    L.new(vor.outputs['Color'], sep.inputs[0])
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    ramp.color_ramp.interpolation = 'CONSTANT'
    el = ramp.color_ramp.elements
    el[0].position, el[0].color = 0., (.36, .36, .19, 1)
    el[1].position, el[1].color = .3, (.13, .25, .07, 1)
    e3 = el.new(.62)
    e3.color = (.045, .12, .035, 1)
    L.new(sep.outputs[0], ramp.inputs[0])
    # Мелкая вариация внутри пятен, как мазки кисти.
    fine = _node(nt, 'ShaderNodeTexNoise', Scale=14., Detail=4.)
    fmap = _node(nt, 'ShaderNodeMapRange', **{'To Min': .8, 'To Max': 1.15})
    L.new(fine.outputs['Fac'], fmap.inputs['Value'])
    col = _mix(nt, ramp.outputs['Color'], fmap.outputs['Result'], 1., 'MULTIPLY')

    # Ребра: нормаль Bevel отличается от плоской нормали грани только у кромки.
    bev = _node(nt, 'ShaderNodeBevel', Radius=.012)
    bev.samples = 6
    geo = nt.nodes.new('ShaderNodeNewGeometry')
    dot = nt.nodes.new('ShaderNodeVectorMath')
    dot.operation = 'DOT_PRODUCT'
    L.new(bev.outputs['Normal'], dot.inputs[0])
    L.new(geo.outputs['Normal'], dot.inputs[1])
    edge = _node(nt, 'ShaderNodeMapRange', **{'From Min': .97, 'From Max': .995, 'To Min': .6, 'To Max': 0.})
    L.new(dot.outputs['Value'], edge.inputs['Value'])
    col = _mix(nt, col, (.62, .6, .36), edge.outputs['Result'])

    ao = _node(nt, 'ShaderNodeAmbientOcclusion', Distance=.1)
    aor = _node(nt, 'ShaderNodeMapRange', **{'To Min': .35})
    L.new(ao.outputs['AO'], aor.inputs['Value'])
    col = _mix(nt, col, aor.outputs['Result'], 1., 'MULTIPLY')

    tip = nt.nodes.new('ShaderNodeAttribute')
    tip.attribute_name = 'tip'
    col = _mix(nt, col, (.035, .028, .022), tip.outputs['Fac'])
    L.new(col, bs.inputs['Base Color'])
    rr = _node(nt, 'ShaderNodeMapRange', **{'To Min': .45, 'To Max': .18})
    L.new(tip.outputs['Fac'], rr.inputs['Value'])
    L.new(rr.outputs['Result'], bs.inputs['Roughness'])

    bump_tex = _node(nt, 'ShaderNodeTexNoise', Scale=38., Detail=5.)
    bump = _node(nt, 'ShaderNodeBump', Strength=.18, Distance=.003)
    L.new(bump_tex.outputs['Fac'], bump.inputs['Height'])
    L.new(bump.outputs['Normal'], bs.inputs['Normal'])
    return m


def simple_material(name, color, rough=.5, emit=None, strength=0., coat=0., rib=0.):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    m.diffuse_color = (*color, 1)
    nt = m.node_tree
    bs = nt.nodes['Principled BSDF']
    bs.inputs['Base Color'].default_value = (*color, 1)
    bs.inputs['Roughness'].default_value = rough
    if coat:
        bs.inputs['Coat Weight'].default_value = coat
        bs.inputs['Coat Roughness'].default_value = .05
    if emit:
        bs.inputs['Emission Color'].default_value = (*emit, 1)
        bs.inputs['Emission Strength'].default_value = strength
    if rib:
        wave = _node(nt, 'ShaderNodeTexWave', Scale=rib, Distortion=1.5)
        wave.bands_direction = 'Z'
        b = _node(nt, 'ShaderNodeBump', Strength=.7, Distance=.004)
        nt.links.new(wave.outputs['Fac'], b.inputs['Height'])
        nt.links.new(b.outputs['Normal'], bs.inputs['Normal'])
    return m


def make_materials():
    return {
        'jade': jade_material(),
        'sinew': simple_material('JS Sinew', (.03, .018, .01), rough=.75, rib=60.),
        'flesh': simple_material('JS Flesh dark', (.035, .03, .022), rough=.6),
        'eye': simple_material('JS Eyes', (.25, .03, .01), rough=.15, emit=(1., .15, .03), strength=.35, coat=1.),
    }


# ---------------------------------------------------------------- геометрия

def mesh_obj(name, verts, faces, coll, mat, smooth=False, tips=None):
    me = bpy.data.meshes.new(name)
    me.from_pydata([tuple(v) for v in verts], [], faces)
    me.validate()
    me.materials.append(mat)
    me.polygons.foreach_set('use_smooth', [smooth] * len(me.polygons))
    at = me.attributes.new('tip', 'FLOAT', 'POINT')
    at.data.foreach_set('value', tips or [0.] * len(verts))
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me)
    bm.free()
    me.update()
    return ob


def frames(pts, up):
    """Параллельный перенос рамки вдоль ломаной: без перекручивания колец."""
    n = len(pts)
    T = [(pts[min(i + 1, n - 1)] - pts[max(i - 1, 0)]).normalized() for i in range(n)]
    u = up - up.dot(T[0]) * T[0]
    if u.length < 1e-6:
        u = V((1, 0, 0)) - T[0].x * T[0]
    N = [u.normalized()]
    for i in range(1, n):
        u = N[-1] - N[-1].dot(T[i]) * T[i]
        N.append(u.normalized())
    return T, N, [T[i].cross(N[i]) for i in range(n)]


def seed_of(name):
    return random.Random(zlib.crc32(name.encode()))


def chunk(name, pts, radii, coll, mat, sides=6, su=1., ss=1., up=UP, jit=.07, roll=0., tip=None, bev=.006,
          cap=.35):
    """Граненый сегмент вдоль ломаной: кольца с шумом, плоские грани, пирамидальные торцы.

    su - масштаб сечения вдоль up, ss - поперек; радиус 0 дает острие.
    tip - доля длины, после которой атрибут tip плавно растет до 1 (темный кончик).
    """
    pts = [V(p) for p in pts]
    T, N, B = frames(pts, V(up))
    rng = seed_of(name)
    lens = [0.]
    for a, b in zip(pts, pts[1:]):
        lens.append(lens[-1] + (b - a).length)
    total = lens[-1] or 1.
    verts, tips, rings = [], [], []

    def tipv(u):
        if tip is None or u <= tip:
            return 0.
        t = min(1., (u - tip) / (1 - tip) * 1.6)
        return t * t * (3 - 2 * t)

    for i, p in enumerate(pts):
        u = lens[i] / total
        if radii[i] < 1e-5:
            rings.append([len(verts)])
            verts.append(p)
            tips.append(tipv(u))
            continue
        ring = []
        for k in range(sides):
            th = 2 * math.pi * k / sides + roll
            j = 1 + rng.uniform(-jit, jit)
            off = (math.cos(th) * su * N[i] + math.sin(th) * ss * B[i]) * radii[i] * j
            off += T[i] * radii[i] * rng.uniform(-jit, jit)
            ring.append(len(verts))
            verts.append(p + off)
            tips.append(tipv(u))
        rings.append(ring)
    faces = []
    for a, b in zip(rings, rings[1:]):
        if len(a) == 1:
            faces += [(a[0], b[k], b[(k + 1) % sides]) for k in range(sides)]
        elif len(b) == 1:
            faces += [(a[k], a[(k + 1) % sides], b[0]) for k in range(sides)]
        else:
            faces += [(a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]) for k in range(sides)]
    for idx, ring, sign in ((0, rings[0], -1), (len(pts) - 1, rings[-1], 1)):
        if len(ring) == 1:
            continue
        verts.append(pts[idx] + T[idx] * sign * radii[idx] * cap)
        tips.append(tips[ring[0]])
        c = len(verts) - 1
        faces += [(ring[k], ring[(k + 1) % sides], c) for k in range(sides)]
    ob = mesh_obj(name, verts, faces, coll, mat, tips=tips)
    if bev:
        m = ob.modifiers.new('Bevel', 'BEVEL')
        m.width = bev
        m.segments = 1
        m.limit_method = 'ANGLE'
        m.angle_limit = math.radians(25)
    return ob


def rope(name, center, axis, R, coll, mat, r=None, turns=2):
    """Связка из нескольких колец-жгутов вокруг сустава."""
    axis = V(axis).normalized()
    r = r or R * .22
    a = axis.orthogonal().normalized()
    b = axis.cross(a)
    verts, faces = [], []
    segs, rs = 20, 7
    for t in range(turns):
        c0 = V(center) + axis * (t - (turns - 1) / 2) * r * 1.7
        base = len(verts)
        for i in range(segs):
            phi = 2 * math.pi * i / segs
            d = a * math.cos(phi) + b * math.sin(phi)
            rr = R * (1 + .04 * math.sin(phi * 3 + t))
            for k in range(rs):
                th = 2 * math.pi * k / rs
                verts.append(c0 + d * (rr + r * math.cos(th)) + axis * r * math.sin(th))
        for i in range(segs):
            for k in range(rs):
                i2, k2 = (i + 1) % segs, (k + 1) % rs
                faces.append((base + i * rs + k, base + i2 * rs + k, base + i2 * rs + k2, base + i * rs + k2))
    ob = mesh_obj(name, verts, faces, coll, mat, smooth=True)
    return ob


def sphere(name, center, r, coll, mat, scale=(1, 1, 1)):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=20, v_segments=12, radius=r)
    for v in bm.verts:
        v.co = V(center) + V((v.co.x * scale[0], v.co.y * scale[1], v.co.z * scale[2]))
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(mat)
    me.polygons.foreach_set('use_smooth', [True] * len(me.polygons))
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    return ob


def spike(name, base, direction, length, r, coll, mat, bend=V((0, 0, 0)), sides=5):
    d = V(direction).normalized()
    base = V(base)
    pts = [base - d * r * .6, base + d * length * .45 + bend * .4, base + d * length + bend]
    return chunk(name, pts, [r, r * .55, 0], coll, mat, sides=sides, jit=.1, tip=.75, bev=.004)


def lerp(a, b, t):
    return V(a).lerp(V(b), t)


def polyline(*pts, n=4):
    """Точки ломаной с промежуточными кольцами на каждом отрезке."""
    out = []
    for a, b in zip(pts, pts[1:]):
        out += [lerp(a, b, k / n) for k in range(n)]
    return out + [V(pts[-1])]


def catmull(ctrl, n):
    ctrl = [V(c) for c in ctrl]
    pts = []
    m = len(ctrl) - 1
    for s in range(n + 1):
        u = s / n * m
        i = min(int(u), m - 1)
        t = u - i
        p0, p1, p2, p3 = ctrl[max(i - 1, 0)], ctrl[i], ctrl[i + 1], ctrl[min(i + 2, m)]
        pts.append(.5 * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t
                         + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
    return pts


def at_length(pts, u):
    lens = [0.]
    for a, b in zip(pts, pts[1:]):
        lens.append(lens[-1] + (b - a).length)
    target = u * lens[-1]
    for i in range(len(pts) - 1):
        if lens[i + 1] >= target:
            t = (target - lens[i]) / max(lens[i + 1] - lens[i], 1e-9)
            return pts[i].lerp(pts[i + 1], t), (pts[i + 1] - pts[i]).normalized()
    return pts[-1], (pts[-1] - pts[-2]).normalized()


# ---------------------------------------------------------------- части тела

def build_carapace(coll, M):
    j = M['jade']
    chunk('JS_Cephalothorax', polyline((0, -.62, .44), (0, -.42, .5), (0, -.12, .54), (0, .16, .53),
                                       (0, .38, .48), n=1),
          [.2, .37, .46, .46, .38], coll, j, sides=8, su=.55, roll=math.pi / 8)
    # Лобный щит с гребнем: вершина кольца сверху дает ребро.
    chunk('JS_Head_Shield', [(0, -.12, .8), (0, -.4, .76), (0, -.64, .6), (0, -.78, .5)], [.27, .31, .23, .07],
          coll, j, sides=6, su=.34, ss=1.05, up=V((0, -.4, 1)))
    for s in SIDES:
        for i, y in enumerate((-.42, -.13, .17)):
            chunk(f'JS_Flank_Plate_{i}_{"LR"[s > 0]}',
                  [(s * .1, y, .8 - .02 * i), (s * .33, y + .03, .72), (s * .52, y + .06, .52),
                   (s * .56, y + .07, .38)],
                  [.13, .17, .14, .06], coll, j, sides=6, su=.32, up=V((s * .7, 0, 1)), roll=math.pi / 6)
            spike(f'JS_Flank_Spike_{i}_{"LR"[s > 0]}', (s * (.3 + .03 * i), y + .02, .76),
                  (s * .5, .25, 1), .28 + .06 * (i == 1), .075, coll, j, bend=V((0, .04, 0)))
        spike(f'JS_Rear_Spike_{"LR"[s > 0]}', (s * .2, .33, .73), (s * .35, .5, 1), .26, .075, coll, j)
    chunk('JS_Dorsal_Ridge', [(0, -.05, .82), (0, .15, .82), (0, .36, .72)], [.12, .14, .07], coll, j,
          sides=5, su=.6, ss=1.)
    spike('JS_Dorsal_Spike', (0, .12, .86), (0, .35, 1), .3, .09, coll, j)


def build_abdomen(coll, M):
    j = M['jade']
    chunk('JS_Abdomen_Core', polyline((0, .2, .4), (0, .62, .39), (0, 1.0, .37), (0, 1.22, .38), n=1),
          [.38, .4, .32, .2], coll, j, sides=8, su=.62, roll=math.pi / 8)
    for i in range(6):
        y = .3 + i * .16
        w = .5 - .035 * i
        h = .34 - .03 * i
        zc = .38
        pts = [V((w * math.sin(f), y + .02 * math.cos(f), zc + h * math.cos(f)))
               for f in [-1.35 + 2.7 * k / 6 for k in range(7)]]
        # Сечение пластины: длинное вдоль Y (N), тонкое по радиусу (B).
        chunk(f'JS_Tergite_{i}', pts, [.06, .11, .125, .13, .125, .11, .06], coll, j, sides=6, su=1., ss=.42,
              up=V((0, 1, 0)), roll=math.pi / 6, jit=.06)
        for s in SIDES:
            spike(f'JS_Tergite_Spike_{i}_{"LR"[s > 0]}', (s * w * .93, y, zc + h * .3),
                  (s, .5, .45), .12 - .01 * i, .045, coll, j)


TAIL_CTRL = [(0, 1.12, .42), (0, 1.4, .64), (0, 1.53, 1.0), (0, 1.46, 1.4), (0, 1.22, 1.7), (0, .9, 1.84),
             (0, .64, 1.78)]


def build_tail(coll, M):
    j = M['jade']
    curve = catmull(TAIL_CTRL, 240)
    bounds = [0, .19, .37, .54, .71, .87, 1.]
    for k in range(6):
        u0, u1 = bounds[k], bounds[k + 1]
        R = .21 - .012 * k
        gap = .012
        pts = [at_length(curve, u0 + gap + (u1 - u0 - 2 * gap) * t / 4)[0] for t in range(5)]
        chunk(f'JS_Tail_Segment_{k}', pts, [R * .72, R * .96, R, R * .95, R * .72], coll, j, sides=6,
              ss=.92, up=V((1, 0, 0)), roll=0., jit=.08)
        # Киль сверху сегмента.
        mid, d = at_length(curve, (u0 + u1) / 2)
        out = d.cross(V((1, 0, 0))).normalized()
        if out.dot(V((0, .6, 1))) < 0:
            out = -out
        spike(f'JS_Tail_Keel_{k}', mid + out * R * .8, out + d * .4, .14, .06, coll, j)
        if k:
            p, d = at_length(curve, u0)
            rope(f'JS_Tail_Joint_{k}', p, d, R * .78, coll, M['sinew'])
    end, d = at_length(curve, 1.)
    rope('JS_Tail_Joint_6', end, d, .1, coll, M['sinew'])
    b = [end + d * t for t in (.02, .1, .2, .3)]
    chunk('JS_Telson_Bulb', b, [.12, .19, .17, .08], coll, j, sides=7, jit=.08, up=V((1, 0, 0)))
    b0 = b[-1] - d * .02
    barb = catmull([b0, b0 + V((0, -.14, -.02)), b0 + V((0, -.25, -.12)), b0 + V((0, -.28, -.27)),
                    b0 + V((0, -.24, -.4))], 8)
    rad = [.095 * (1 - i / 8) ** .8 for i in range(9)]
    chunk('JS_Stinger', barb, rad, coll, j, sides=6, up=V((1, 0, 0)), tip=.35, jit=.04)


def side_axis(a, s):
    sv = a.cross(UP).normalized()
    return sv if sv.x * s > 0 else -sv


def build_claws(coll, M):
    j, sinew = M['jade'], M['sinew']
    for s in SIDES:
        tag = 'LR'[s > 0]
        A = V((s * .3, -.5, .42))
        E = V((s * .74, -.72, .56))
        W = V((s * .8, -1.12, .42))
        H = V((s * .68, -1.58, .28))
        chunk(f'JS_Palp_Arm_{tag}', polyline(A, E, n=3), [.11, .15, .15, .11], coll, j, sides=6,
              up=V((0, 0, 1)), roll=math.pi / 6)
        rope(f'JS_Palp_Joint_Elbow_{tag}', E, W - A, .11, coll, sinew)
        chunk(f'JS_Palp_Forearm_{tag}', polyline(E, W, n=3), [.12, .17, .16, .12], coll, j, sides=6, roll=.3)
        rope(f'JS_Palp_Joint_Wrist_{tag}', W, H - E, .125, coll, sinew)
        a = (H - W).normalized()
        sv = side_axis(a, s)
        down = sv.cross(a) if sv.cross(a).z < 0 else -sv.cross(a)
        palm = [W - a * .02, lerp(W, H, .25), lerp(W, H, .55), lerp(W, H, .85), H + a * .04]
        chunk(f'JS_Claw_Palm_{tag}', palm, [.15, .27, .33, .3, .2], coll, j, sides=7, su=.6,
              up=UP, jit=.06, roll=math.pi / 7)
        # Ребро-гребень на ладони клешни.
        chunk(f'JS_Claw_Crest_{tag}', [lerp(W, H, .2) + UP * .12 + sv * .05, lerp(W, H, .6) + UP * .14 + sv * .07,
                                       H + UP * .08 + sv * .08], [.06, .07, .03], coll, j, sides=5, su=.7)
        F0 = H + sv * .09 + a * .02
        fixed = catmull([F0, F0 + a * .2 + down * .02, F0 + a * .37 - sv * .06 + down * .05,
                         F0 + a * .48 - sv * .18 + down * .08], 8)
        rf = [.16 * (1 - i / 8) ** .75 for i in range(9)]
        chunk(f'JS_Claw_Finger_Fixed_{tag}', fixed, rf, coll, j, sides=6, su=.62, tip=.55, jit=.05)
        M0 = H - sv * .1 + a * .02
        mov = catmull([M0, M0 + a * .17 - sv * .03, M0 + a * .31 + sv * .02 + down * .02,
                       M0 + a * .39 + sv * .1 + down * .05], 8)
        rm = [.13 * (1 - i / 8) ** .75 for i in range(9)]
        chunk(f'JS_Claw_Finger_Moving_{tag}', mov, rm, coll, j, sides=6, su=.62, tip=.55, jit=.05)
        # Зубцы по внутренним кромкам, направлены к противоположному пальцу.
        for name, curve, rr, inward in (('F', fixed, rf, -sv), ('M', mov, rm, sv)):
            for n, i in enumerate((2, 3, 4, 5)):
                p = curve[i]
                tdir = (curve[i + 1] - curve[i - 1]).normalized()
                inn = (inward - inward.dot(tdir) * tdir).normalized()
                spike(f'JS_Claw_Tooth_{name}{n}_{tag}', p + inn * rr[i] * .7, inn + tdir * .5, .065 + .015 * (n % 2),
                      .034, coll, j, sides=4)


LEGS = [  # y крепления, сдвиг колена и стопы по Y, длина
    (-.28, -.24, -.5, 1.),
    (-.06, -.08, -.18, 1.),
    (.16, .1, .2, 1.03),
    (.36, .28, .55, 1.08),
]


def build_legs(coll, M):
    j, sinew = M['jade'], M['sinew']
    for s in SIDES:
        tag = 'LR'[s > 0]
        for i, (yb, dk, df, ln) in enumerate(LEGS):
            B = V((s * .38, yb, .4))
            K = V((s * .82 * ln, yb + dk, .68))
            A = V((s * 1.14 * ln, yb + (dk + df) * .55, .36))
            F = V((s * 1.3 * ln, yb + df, 0.))
            chunk(f'JS_Leg_{i}_Femur_{tag}', polyline(B, K, n=3), [.1, .125, .12, .095], coll, j, sides=6,
                  roll=math.pi / 6)
            rope(f'JS_Leg_{i}_Knee_{tag}', K, A - B, .09, coll, sinew)
            spike(f'JS_Leg_{i}_Spur_{tag}', lerp(B, K, .75) + UP * .06, V((s * .3, 0, 1)), .13, .05, coll, j)
            chunk(f'JS_Leg_{i}_Tibia_{tag}', polyline(K, A, n=3), [.095, .11, .1, .08], coll, j, sides=6,
                  roll=.4)
            rope(f'JS_Leg_{i}_Ankle_{tag}', A, F - K, .075, coll, sinew)
            claw = catmull([A, lerp(A, F, .5) + V((s * .04, 0, .02)), F], 6)
            chunk(f'JS_Leg_{i}_Tarsus_{tag}', claw, [.08 * (1 - k / 6) ** .7 for k in range(7)], coll, j,
                  sides=6, tip=.5, jit=.05)
            rope(f'JS_Leg_{i}_Hip_{tag}', B, K - B, .1, coll, sinew)


def build_face(coll, M):
    j = M['jade']
    sphere('JS_Face_Cavity', (0, -.64, .4), .15, coll, M['flesh'], scale=(1.15, .8, .7))
    for s in SIDES:
        tag = 'LR'[s > 0]
        sphere(f'JS_Eye_Inner_{tag}', (s * .055, -.735, .45), .029, coll, M['eye'])
        sphere(f'JS_Eye_Outer_{tag}', (s * .14, -.7, .44), .025, coll, M['eye'])
        chunk(f'JS_Chelicera_{tag}', [(s * .06, -.64, .37), (s * .07, -.74, .35), (s * .07, -.8, .33)],
              [.05, .055, .04], coll, j, sides=5)
        fang = catmull([(s * .07, -.79, .32), (s * .085, -.86, .25), (s * .075, -.88, .14)], 6)
        chunk(f'JS_Fang_{tag}', fang, [.04 * (1 - k / 6) ** .7 for k in range(7)], coll, j, sides=5, tip=.45)
        spike(f'JS_Brow_Spike_{tag}', (s * .2, -.66, .55), (s * .4, -1, .25), .12, .04, coll, j)


# ---------------------------------------------------------------- студия

def build_studio(scene):
    studio = bpy.data.collections.new('STUDIO')
    scene.collection.children.link(studio)
    prof = [(-25., 0.)] + [(3 + 3 * math.cos(a), 3 + 3 * math.sin(a))
                           for a in [math.radians(-90 + 90 * k / 16) for k in range(17)]] + [(6., 14.)]
    verts = []
    for x in (-20, 20):
        verts += [(x, y, z) for y, z in prof]
    n = len(prof)
    faces = [(i, i + 1, n + i + 1, n + i) for i in range(n - 1)]
    floor_mat = simple_material('Studio floor', (.02, .018, .016), rough=.85)
    floor = mesh_obj('Studio_Cyclorama', verts, faces, studio, floor_mat, smooth=True)
    floor.hide_select = True
    world = bpy.data.worlds.new('Studio world')
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (.04, .04, .05, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = .8
    scene.world = world
    add_area(studio, 'Key warm', (-3.2, -4.2, 4.8), (0, -.3, .6), 520, (1, .9, .78), 3)
    add_area(studio, 'Fill cool', (3.8, -3.2, 2.2), (0, -.3, .6), 110, (.75, .85, 1), 4)
    add_area(studio, 'Rim torch', (1.5, 4.2, 3.2), (0, .3, 1.), 1400, (1, .62, .3), 2)
    add_area(studio, 'Rim back', (-3.0, 3.2, 2.6), (0, .3, .9), 700, (1, .7, .45), 2)
    cam_data = bpy.data.cameras.new('Camera')
    cam_data.lens = 50
    cam = bpy.data.objects.new('Camera', cam_data)
    studio.objects.link(cam)
    cam.location = (-2.6, -5.8, 3.0)
    point_at(cam, (0, -.2, .7))
    scene.camera = cam


def set_viewports():
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type != 'VIEW_3D':
                continue
            sp = area.spaces.active
            sp.shading.type = 'MATERIAL'
            sp.region_3d.view_perspective = 'CAMERA'


def main():
    if not bpy.app.background:
        raise RuntimeError('Run in background Blender.')
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.use_preferences_save = False
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    root = bpy.data.collections.new('JADE_SCORPION')
    scene.collection.children.link(root)
    groups = {n: bpy.data.collections.new(f'JS {n}') for n in ('Body', 'Tail', 'Claws', 'Legs', 'Face')}
    for c in groups.values():
        root.children.link(c)
    M = make_materials()
    build_carapace(groups['Body'], M)
    build_abdomen(groups['Body'], M)
    build_tail(groups['Tail'], M)
    build_claws(groups['Claws'], M)
    build_legs(groups['Legs'], M)
    build_face(groups['Face'], M)
    build_studio(scene)
    set_viewports()
    OUT.mkdir(parents=True, exist_ok=True)
    path = OUT / 'jade-scorpion.blend'
    bpy.ops.wm.save_as_mainfile(filepath=str(path))

    dg = bpy.context.evaluated_depsgraph_get()
    tris, lo, hi, bad = 0, V((1e9,) * 3), V((-1e9,) * 3), []
    for ob in root.all_objects:
        me = ob.evaluated_get(dg).to_mesh()
        me.calc_loop_triangles()
        tris += len(me.loop_triangles)
        for v in me.vertices:
            co = ob.matrix_world @ v.co
            if not all(math.isfinite(c) for c in co):
                bad.append(ob.name)
                break
            lo = V(map(min, lo, co))
            hi = V(map(max, hi, co))
        ob.evaluated_get(dg).to_mesh_clear()
    print('SCORPION_BUILD ' + json.dumps({'path': str(path), 'objects': len(root.all_objects),
                                          'evaluated_triangles': tris, 'non_finite': bad,
                                          'bbox_min': [round(c, 3) for c in lo],
                                          'bbox_max': [round(c, 3) for c in hi]}))


if __name__ == '__main__':
    main()

"""Иглогрив: коренастый вепрь-воин, рыжая шерсть, гребень жестких игл, бронза и кожа, тяжелый тесак."""
import math

import bmesh
import bpy
from mathutils import Vector
from mathutils.bvhtree import BVHTree

from kit import (SIDES, UP, V, _mix, add, auto_weights, blob, box, catmull, chunk, ease, lathe, lerp, mesh_obj,
                 painted, part, pulse, rng, sphere, tube, wave, window, flat)

SLUG = 'boar-warrior'
TITLE = 'Иглогрив'
UNITY_NAME = 'BoarWarrior'
HEIGHT = 2.3
GAME_TRIS = 14000

K = 1 / .575  # metaball: при stiff=2 и пороге .6 видимый радиус одиночного шара = .575 r


def tag(s):
    return 'L' if s > 0 else 'R'


# ---------------------------------------------------------------- опорные точки скелета

def arm_pts(s):
    SH = V((s * .49, .0, 1.58))
    EL = V((s * .6, .07, 1.22))
    WR = V((s * .64, -.05, .96))
    a = V((0, .45, -.89) if s < 0 else (0, -.12, -.99)).normalized()
    return SH, EL, WR, a


def leg_pts(s):
    return (V((s * .21, .06, .95)), V((s * .26, -.15, .58)), V((s * .28, .14, .25)), V((s * .29, -.07, .055)))


def _bones():
    b = [('Root', (0, 0, 0), (0, 0, .3), None),
         ('Pelvis', (0, .06, .92), (0, .05, 1.12), 'Root'),
         ('Spine', (0, .05, 1.12), (0, .03, 1.36), 'Pelvis'),
         ('Chest', (0, .03, 1.36), (0, -.02, 1.62), 'Spine'),
         ('Neck', (0, -.02, 1.62), (0, -.16, 1.79), 'Chest'),
         ('Head', (0, -.16, 1.79), (0, -.46, 1.82), 'Neck'),
         ('Jaw', (0, -.22, 1.72), (0, -.5, 1.66), 'Head')]
    for s in SIDES:
        t = tag(s)
        SH, EL, WR, a = arm_pts(s)
        b += [(f'UpperArm_{t}', tuple(SH), tuple(EL), 'Chest'),
              (f'Forearm_{t}', tuple(EL), tuple(WR), f'UpperArm_{t}'),
              (f'Hand_{t}', tuple(WR), tuple(WR + a * .17), f'Forearm_{t}')]
        H, Kn, Ho, F = leg_pts(s)
        b += [(f'Thigh_{t}', tuple(H), tuple(Kn), 'Pelvis'),
              (f'Shin_{t}', tuple(Kn), tuple(Ho), f'Thigh_{t}'),
              (f'Foot_{t}', tuple(Ho), tuple(F), f'Shin_{t}')]
    return b


BONES = _bones()


# ---------------------------------------------------------------- материалы

def tipped(m, color, gain=1.):
    """Подмешивает цвет к кончикам по атрибуту tip, который пишет chunk(tip=...)."""
    nt = m.node_tree
    bs = nt.nodes['Principled BSDF']
    src = bs.inputs['Base Color'].links[0].from_socket
    at = nt.nodes.new('ShaderNodeAttribute')
    at.attribute_name = 'tip'
    fac = at.outputs['Fac']
    if gain != 1.:
        mul = nt.nodes.new('ShaderNodeMath')
        mul.operation = 'MULTIPLY'
        mul.use_clamp = True
        mul.inputs[1].default_value = gain
        nt.links.new(fac, mul.inputs[0])
        fac = mul.outputs[0]
    nt.links.new(_mix(nt, src, color, fac), bs.inputs['Base Color'])
    return m


def materials():
    return {
        'fur': painted('BW Russet fur', [(0., (.07, .018, .008)), (.35, (.16, .045, .018)), (.7, (.27, .085, .035)),
                                         (1., (.4, .15, .065))],
                       scale=9., stretch=(1.6, 1.6, .45), rough=.82, bump=.55, bump_scale=70., ao=.55,
                       edge=(.5, .22, .1), edge_amount=.25, detail=6.),
        'skin': painted('BW Snout skin', [(0., (.14, .035, .03)), (.6, (.26, .08, .065)), (1., (.36, .13, .1))],
                        scale=14., rough=.55, bump=.35, bump_scale=90., ao=.5),
        'quill': tipped(painted('BW Quills', [(0., (.06, .01, .006)), (.6, (.16, .03, .012)),
                                              (1., (.24, .06, .02))],
                                scale=30., stretch=(1, 1, 4), rough=.45, bump=.15, ao=.3),
                        (.72, .5, .3), gain=1.2),
        'ivory': tipped(painted('BW Ivory', [(0., (.42, .33, .2)), (.6, (.68, .58, .4)), (1., (.82, .74, .56))],
                                scale=18., kind='wave', rough=.35, bump=.2, ao=.4),
                        (.92, .88, .76)),
        'hoof': flat('BW Hoof', (.022, .016, .013), rough=.35),
        'bronze': painted('BW Bronze', [(0., (.12, .055, .015)), (.5, (.36, .2, .06)), (1., (.62, .42, .14))],
                          scale=7., rough=.32, metal=.9, edge=(.95, .78, .42), edge_amount=.7, bump=.18,
                          ao=.6, warp=.5),
        'iron': painted('BW Dark iron', [(0., (.012, .012, .013)), (.6, (.035, .033, .032)),
                                         (1., (.07, .06, .055))],
                        scale=10., rough=.42, metal=.85, edge=(.32, .3, .28), edge_amount=.55, bump=.25, ao=.6),
        'leather': painted('BW Dark leather', [(0., (.025, .012, .006)), (.5, (.06, .03, .014)),
                                               (1., (.11, .06, .03))],
                           scale=12., rough=.7, edge=(.2, .12, .06), edge_amount=.45, bump=.4, bump_scale=60.,
                           ao=.55),
        'strap': painted('BW Strap leather', [(0., (.06, .025, .01)), (.6, (.13, .065, .03)), (1., (.2, .11, .05))],
                         scale=10., rough=.62, edge=(.32, .2, .1), edge_amount=.4, bump=.3, ao=.5),
        'steel': painted('BW Worn steel', [(0., (.09, .04, .02)), (.3, (.2, .19, .18)), (.7, (.42, .42, .42)),
                                           (1., (.62, .62, .6))],
                         scale=6., kind='voronoi', rough=.38, metal=1., edge=(.9, .9, .88), edge_amount=.8,
                         bump=.2, ao=.4, warp=.7),
        'wrap': painted('BW Grip wrap', [(0., (.05, .03, .015)), (1., (.16, .1, .05))], scale=60., kind='wave',
                        rough=.8, bump=.5, ao=.4),
        'mouth': flat('BW Mouth', (.03, .006, .005), rough=.5),
        'eye': flat('BW Eyes', (.9, .45, .08), rough=.15, emit=(1., .45, .05), emit_strength=3.),
    }


# ---------------------------------------------------------------- вспомогательная геометрия

def E(co, semi, rot=(0, 0, 0), stiff=2., neg=False):
    return dict(type='ELLIPSOID', co=co, r=K, size=semi, rot=rot, stiff=stiff, neg=neg)


def Bl(co, R, stiff=2., neg=False):
    return dict(type='BALL', co=co, r=R * K, stiff=stiff, neg=neg)


def limb(pts, radii, n=None, f=.86):
    """Цепочка шаров по сплайну: сужающаяся конечность. f компенсирует разбухание при слиянии."""
    pts = [V(p) for p in pts]
    length = sum((b - a).length for a, b in zip(pts, pts[1:]))
    n = n or max(4, int(length / (min(radii) * .45)))
    curve = catmull(pts, n) if len(pts) > 2 else [lerp(pts[0], pts[1], k / n) for k in range(n + 1)]
    out = []
    for k, p in enumerate(curve):
        u = k / n * (len(radii) - 1)
        i = min(int(u), len(radii) - 2)
        r = radii[i] + (radii[i + 1] - radii[i]) * (u - i)
        out.append(Bl(tuple(p), r * f))
    return out


class Fit:
    """Лучи по поверхности органики в позе покоя: посадка ремней, пояса, юбки и игл."""

    def __init__(self, objs):
        verts, polys = [], []
        for ob in objs:
            base = len(verts)
            verts += [ob.matrix_world @ v.co for v in ob.data.vertices]
            polys += [[base + i for i in p.vertices] for p in ob.data.polygons]
        self.bvh = BVHTree.FromPolygons(verts, polys)

    def hit(self, origin, direction):
        loc, nrm, _, _ = self.bvh.ray_cast(V(origin), V(direction).normalized())
        return (loc, nrm) if loc is not None else (None, None)

    def radial(self, center, ang, far=2.):
        d = V((math.cos(ang), math.sin(ang), 0))
        loc, nrm = self.hit(V(center) + d * far, -d)
        return ((loc - V(center)).length if loc is not None else 0.), nrm


def ribbon(name, pts, nrms, width, th, coll, mat, closed=False):
    """Плоский ремень по поверхности: сечение - прямоугольник, ширина поперек нормали."""
    n = len(pts)
    verts, faces = [], []
    for i in range(n):
        t = (pts[(i + 1) % n if closed else min(i + 1, n - 1)] - pts[(i - 1) % n if closed else max(i - 1, 0)])
        t.normalize()
        nr = V(nrms[i]).normalized()
        b = t.cross(nr).normalized()
        w = width[i] if isinstance(width, (list, tuple)) else width
        p = V(pts[i])
        verts += [p - b * w / 2, p + b * w / 2, p + b * w / 2 + nr * th, p - b * w / 2 + nr * th]
    m = n if closed else n - 1
    for i in range(m):
        a, c = i * 4, ((i + 1) % n) * 4
        for k in range(4):
            faces.append((a + k, a + (k + 1) % 4, c + (k + 1) % 4, c + k))
    if not closed:
        faces += [(3, 2, 1, 0), ((n - 1) * 4, (n - 1) * 4 + 1, (n - 1) * 4 + 2, (n - 1) * 4 + 3)]
    ob = mesh_obj(name, verts, faces, coll, mat)
    bev = ob.modifiers.new('Bevel', 'BEVEL')
    bev.width = min(th, w if not isinstance(width, (list, tuple)) else min(width)) * .3
    bev.segments = 2
    bev.limit_method = 'ANGLE'
    return ob


def torus(name, center, axis, R, r, coll, mat, segs=20, rs=8):
    axis = V(axis).normalized()
    a = axis.orthogonal().normalized()
    b = axis.cross(a)
    verts, faces = [], []
    for i in range(segs):
        ph = 2 * math.pi * i / segs
        d = a * math.cos(ph) + b * math.sin(ph)
        for k in range(rs):
            th = 2 * math.pi * k / rs
            verts.append(V(center) + d * (R + r * math.cos(th)) + axis * r * math.sin(th))
    for i in range(segs):
        for k in range(rs):
            i2, k2 = (i + 1) % segs, (k + 1) % rs
            faces.append((i * rs + k, i2 * rs + k, i2 * rs + k2, i * rs + k2))
    return mesh_obj(name, verts, faces, coll, mat, smooth=True)


def shell(name, center, axis, R, h, th, coll, mat, segs=24, squash=(1., 1.), up=None):
    """Купол пластины брони с толщиной: наружная и внутренняя поверхность, скругленный край."""
    prof = []
    for k in range(9):
        f = k / 8
        prof.append((R * math.sin(f * math.pi / 2), h * math.cos(f * math.pi / 2)))
    inner = [(r * .96, z - th) for r, z in reversed(prof)]
    full = prof + [(R + th * .3, -th * .5)] + inner
    ob = lathe(name, full, coll, mat, center=(0, 0, 0), axis=UP, segs=segs)
    axis = V(axis).normalized()
    upv = V(up) if up is not None else (V((0, 0, 1)) if abs(axis.z) < .9 else V((0, 1, 0)))
    xv = upv.cross(axis).normalized()
    yv = axis.cross(xv)
    for v in ob.data.vertices:
        p = v.co
        v.co = V(center) + xv * p.x * squash[0] + yv * p.y * squash[1] + axis * p.z
    return ob


def spikes_on(name, base, direction, length, r, coll, mat, sides=6):
    d = V(direction).normalized()
    pts = [V(base) - d * r * .5, V(base) + d * length * .35, V(base) + d * length * .75, V(base) + d * length]
    return chunk(name, pts, [r, r * .78, r * .35, 0.], coll, mat, sides=sides, jit=.02, bev=.003, tip=.6)


def quill(name, base, direction, length, r, coll, mat, bend=(0, 0, 0), sides=5):
    d = V(direction).normalized()
    bend = V(bend)
    pts = [V(base) - d * r, V(base) + d * length * .3 + bend * .1, V(base) + d * length * .65 + bend * .45,
           V(base) + d * length + bend]
    return chunk(name, pts, [r, r * .8, r * .45, 0.], coll, mat, sides=sides, jit=.04, bev=0, tip=.35)


def tuft(name, base, direction, n, length, r, coll, mat, spread=.5, droop=.3):
    g = rng(name)
    d0 = V(direction).normalized()
    for i in range(n):
        d = (d0 + V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-1, 1))) * spread).normalized()
        ln = length * g.uniform(.6, 1.)
        off = V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-1, 1))) * r * 1.5
        quill(f'{name}_{i}', V(base) + off, d, ln, r * g.uniform(.7, 1.1), coll, mat,
              bend=V((0, 0, -ln * droop)), sides=4)


def blade_plate(name, spine0, g, w, length, w0, w1, th, coll, mat, side):
    """Тесак: обух по линии g, полотно в сторону w, острая кромка, скошенный носок."""
    side = V(side).normalized()
    g, w = V(g).normalized(), V(w).normalized()
    rows = []
    for k in range(7):
        u = k / 6
        top = V(spine0) + g * length * u
        width = w0 + (w1 - w0) * u
        # Носок срезан наискосок, как у мясницкого тесака.
        if u == 1:
            top = top - g * .06
        rows.append([top + side * th / 2, top + w * width * .55 + side * th * .3, top + w * width,
                     top + w * width * .55 - side * th * .3, top - side * th / 2])
    verts = [p for r in rows for p in r]
    faces = []
    for k in range(6):
        a, b = k * 5, (k + 1) * 5
        for i in range(5):
            faces.append((a + i, a + (i + 1) % 5, b + (i + 1) % 5, b + i))
    faces += [tuple(range(4, -1, -1)), tuple(range(30, 35))]
    ob = mesh_obj(name, verts, faces, coll, mat)
    bev = ob.modifiers.new('Bevel', 'BEVEL')
    bev.width = .004
    bev.segments = 2
    bev.limit_method = 'ANGLE'
    return ob


# ---------------------------------------------------------------- тело

def build_torso(M, coll):
    el = [
        E((0, -.08, 1.17), (.28, .27, .25)),  # тяжелое брюхо
        E((0, .04, 1.05), (.26, .22, .18)),
        E((0, -.02, 1.44), (.33, .25, .23)),  # грудная клетка
        E((0, .13, 1.64), (.28, .2, .15), rot=(-15, 0, 0)),  # горб трапеций
        E((0, -.05, 1.31), (.24, .25, .14)),
        Bl((0, -.07, 1.66), .14),
        dict(type='CAPSULE', a=(0, -.02, 1.6), b=(0, -.15, 1.76), r=.14 * K, stiff=2.),
        E((0, -.25, 1.12), (.12, .06, .1), stiff=1.2),  # низ живота
    ]
    for s in SIDES:
        el += [
            E((s * .16, -.17, 1.5), (.16, .1, .11), rot=(0, 0, -s * 12)),  # грудные
            E((s * .17, .05, 1.71), (.15, .14, .09)),  # трапеция к плечу
            Bl((s * .43, .0, 1.6), .135),  # дельта
            E((s * .27, .1, 1.38), (.11, .15, .2)),  # широчайшие
            E((s * .2, -.07, 1.2), (.12, .2, .14)),  # бока
            Bl((s * .16, .12, 1.0), .16),  # ягодицы
            E((s * .12, -.1, .98), (.12, .1, .1)),
            E((s * .065, -.3, 1.3), (.06, .04, .05), stiff=1.4),  # пресс
            E((s * .065, -.31, 1.2), (.06, .04, .045), stiff=1.4),
        ]
    ob = blob('BW_Torso', el, coll, M['fur'], resolution=.028)
    return auto_weights(ob, ['Pelvis', 'Spine', 'Chest', 'Neck'])


def build_head(M, coll):
    with part('Head'):
        el = [
            E((0, -.2, 1.86), (.15, .17, .14)),  # свод
            E((0, -.33, 1.9), (.15, .07, .055), rot=(-20, 0, 0)),  # тяжелые надбровья
            E((0, -.21, 1.72), (.13, .14, .08)),
            Bl((0, -.42, 1.8), .11),
            Bl((0, -.52, 1.775), .095),
            Bl((0, -.6, 1.755), .088),
            E((0, -.48, 1.855), (.055, .12, .04), rot=(-8, 0, 0)),  # спинка рыла
        ]
        for s in SIDES:
            el += [Bl((s * .12, -.31, 1.77), .1),  # щеки
                   E((s * .095, -.43, 1.75), (.06, .1, .05)),
                   Bl((s * .1, -.375, 1.855), .03, neg=True)]  # глазницы
        blob('BW_Head', el, coll, M['fur'], resolution=.02)
        # Пятак с ноздрями.
        disc = lathe('BW_Snout_Disc', [(0., .0), (.07, .0), (.088, -.012), (.092, -.04), (.09, -.07)],
                     coll, M['skin'], center=(0, -.68, 1.755), axis=V((0, 1, 0)), segs=20)
        for v in disc.data.vertices:
            v.co.z = 1.755 + (v.co.z - 1.755) * .82
        for s in SIDES:
            sphere(f'BW_Nostril_{tag(s)}', (s * .033, -.683, 1.758), .022, coll, M['mouth'], scale=(.8, .5, 1.2))
            sphere(f'BW_Eye_{tag(s)}', (s * .098, -.37, 1.853), .021, coll, M['eye'])
            # Уши: короткие, торчат вбок-назад.
            ear = catmull([(s * .11, -.12, 1.94), (s * .17, -.1, 1.99), (s * .23, -.05, 2.04), (s * .27, .0, 2.07)], 6)
            chunk(f'BW_Ear_{tag(s)}', ear, [.045, .055, .05, .035, .022, .01, 0.], coll, M['fur'], sides=7, su=.38,
                  up=V((s * .3, .6, .7)), jit=.03, bev=0)
            # Бакенбарды.
            for i, z in enumerate((1.78, 1.72)):
                tuft(f'BW_Cheek_Tuft_{i}_{tag(s)}', (s * .2, -.27, z), (s * .8, .5, -.2), 4, .12, .016, coll,
                     M['quill'], spread=.3, droop=.2)
        # Губа верхней челюсти.
        chunk('BW_Upper_Lip', catmull([(-.08, -.5, 1.715), (0, -.62, 1.71), (.08, -.5, 1.715)], 6),
              [.022] * 7, coll, M['skin'], sides=6, jit=0, bev=0)
    with part('Jaw'):
        el = [E((0, -.4, 1.685), (.09, .15, .045), rot=(10, 0, 0)), Bl((0, -.52, 1.68), .05),
              E((0, -.28, 1.69), (.1, .07, .05))]
        blob('BW_Jaw', el, coll, M['fur'], resolution=.02)
        sphere('BW_Mouth_Inner', (0, -.45, 1.705), .07, coll, M['mouth'], scale=(1.1, 1.6, .35))
        for s in SIDES:
            t = tag(s)
            tusk = catmull([(s * .07, -.5, 1.68), (s * .1, -.56, 1.71), (s * .14, -.58, 1.78),
                            (s * .165, -.55, 1.85), (s * .17, -.49, 1.9)], 10)
            chunk(f'BW_Tusk_{t}', tusk, [.034 * (1 - k / 10) ** .75 for k in range(11)], coll, M['ivory'],
                  sides=8, jit=.01, bev=0, tip=.55)
            chunk(f'BW_Lower_Tooth_{t}', [(s * .035, -.56, 1.69), (s * .035, -.565, 1.72)], [.012, 0.], coll,
                  M['ivory'], sides=5, jit=0, bev=0)


def build_arms(M, coll):
    for s in SIDES:
        t = tag(s)
        SH, EL, WR, a = arm_pts(s)
        el = limb([SH + V((-s * .03, 0, .02)), lerp(SH, EL, .45) + V((0, -.03, 0)), EL],
                  [.135, .13, .095])
        el += [E(tuple(lerp(SH, EL, .4) + V((0, -.06, 0))), (.085, .08, .11)),  # бицепс
               E(tuple(lerp(SH, EL, .45) + V((0, .06, 0))), (.08, .07, .12))]  # трицепс
        el += limb([EL, lerp(EL, WR, .35) + V((s * .02, 0, 0)), WR], [.1, .12, .08])
        ob = blob(f'BW_Arm_{t}', el, coll, M['fur'], resolution=.024)
        auto_weights(ob, [f'UpperArm_{t}', f'Forearm_{t}'])
        with part(f'Hand_{t}'):
            build_hand(M, coll, s, WR, a, fist=s < 0)
        with part(f'Forearm_{t}'):
            tuft(f'BW_Elbow_Tuft_{t}', EL + V((s * .03, .08, 0)), (s * .3, 1, -.4), 5, .13, .018, coll,
                 M['quill'], spread=.35)


def hand_frame(s, a):
    out = V((s, 0, 0))
    out = (out - a * out.dot(a)).normalized()
    k = a.cross(out).normalized()
    return out, k


def build_hand(M, coll, s, W, a, fist):
    t = tag(s)
    out, k = hand_frame(s, a)
    el = [E(tuple(W + a * .07), (.055, .085, .075)), Bl(tuple(W + a * .02), .07)]
    # Мизинец со стороны -k? Пальцы рядом вдоль k, большой палец - по стороне к телу и вперед.
    fingers = []
    for i, off in enumerate((-.055, -.018, .018, .054)):
        Kn = W + a * .13 + k * off + out * .015
        if fist:
            pts = [Kn, Kn + a * .04 - out * .03, Kn + a * .025 - out * .075, Kn - a * .015 - out * .085]
        else:
            pts = [Kn, Kn + a * .055 - out * .012, Kn + a * .1 - out * .04, Kn + a * .12 - out * .075]
        r = .028 if i in (1, 2) else .025
        el += limb(pts, [r, r * .95, r * .85, r * .75], n=6, f=.9)
        fingers.append(pts)
    # Большой палец: от основания ладони к центру кулака.
    side = -k if s < 0 else k
    T0 = W + a * .05 - out * .04 + side * .05
    tp = [T0, T0 + a * .05 - out * .03 + side * .02, T0 + a * .09 - out * .02 - side * .01]
    el += limb(tp, [.03, .027, .022], n=6, f=.9)
    blob(f'BW_Hand_{t}', el, coll, M['fur'], resolution=.014)
    for i, pts in enumerate(fingers):
        tip_dir = (pts[-1] - pts[-2]).normalized()
        spikes_on(f'BW_Nail_{i}_{t}', pts[-1] + tip_dir * .006 + out * .006, tip_dir + out * .3, .04, .015,
                  coll, M['hoof'], sides=5)
    spikes_on(f'BW_Nail_Thumb_{t}', tp[-1], (tp[-1] - tp[-2]), .035, .015, coll, M['hoof'], sides=5)
    if fist:
        build_cleaver(M, coll, s, W, a, out, k)


def build_cleaver(M, coll, s, W, a, out, k):
    t = tag(s)
    g = -k if (-k).z < 0 else k  # рукоять вперед-вниз сквозь кулак
    if g.y > 0:
        g = -g
    c = W + a * .12 - out * .045
    H0, H1 = c - g * .14, c + g * .17
    tube(f'BW_Cleaver_Grip_{t}', [H0, lerp(H0, H1, .5), H1], [.026, .027, .026], coll, M['wrap'], sides=10)
    for i, u in enumerate((.0, .93)):
        torus(f'BW_Cleaver_Ferrule_{i}_{t}', lerp(H0, H1, u), g, .029, .009, coll, M['bronze'], segs=14, rs=6)
    sphere(f'BW_Cleaver_Pommel_{t}', H0 - g * .025, .034, coll, M['bronze'], scale=(1, 1, 1))
    side = V((1, 0, 0))
    w = g.cross(side).normalized()
    if w.z > 0:
        w = -w
    # Небольшой разворот плоскости полотна наружу, чтобы тесак не шел в бедро.
    w = (w + side * s * .18).normalized()
    sideb = g.cross(w).normalized()
    blade_plate(f'BW_Cleaver_Blade_{t}', H1 - g * .01, g, w, .58, .27, .33, .028, coll, M['steel'], sideb)
    box(f'BW_Cleaver_Spine_{t}', tuple(H1 + g * .2 + w * .012), (.04, .4, .02), coll, M['iron'],
        rot=tuple(math.degrees(x) for x in _euler_from(g, w)), bevel=.006)
    torus(f'BW_Cleaver_Hole_{t}', H1 + g * .1 + w * .07, sideb, .022, .008, coll, M['iron'], segs=14, rs=6)


def _euler_from(yaxis, zaxis):
    """Эйлер XYZ для бокса, у которого локальная Y идет по yaxis, Z - по zaxis."""
    from mathutils import Matrix
    y = V(yaxis).normalized()
    z = (V(zaxis) - y * V(zaxis).dot(y)).normalized()
    x = y.cross(z)
    return Matrix((x, y, z)).transposed().to_euler('XYZ')


def build_legs(M, coll):
    for s in SIDES:
        t = tag(s)
        H, Kn, Ho, F = leg_pts(s)
        el = limb([H + V((0, .02, .05)), lerp(H, Kn, .5) + V((s * .02, -.02, 0)), Kn], [.19, .17, .11])
        el += [E(tuple(lerp(H, Kn, .45) + V((0, -.06, 0))), (.12, .1, .16))]  # квадрицепс
        el += limb([Kn, lerp(Kn, Ho, .3) + V((0, .03, 0)), Ho], [.1, .1, .06])
        ob = blob(f'BW_Leg_{t}', el, coll, M['fur'], resolution=.026)
        auto_weights(ob, [f'Thigh_{t}', f'Shin_{t}'])
        with part(f'Foot_{t}'):
            el = limb([Ho, lerp(Ho, F, .6), F + V((0, .02, .02))], [.065, .058, .07], f=.9)
            blob(f'BW_Foot_{t}', el, coll, M['fur'], resolution=.02)
            d = (F - Ho).normalized()
            fw = V((0, -1, 0))
            for i, x in enumerate((-.042, .042)):
                hp = F + V((s * x, -.02, -.012))
                chunk(f'BW_Hoof_{i}_{t}', [hp + fw * -.05 + UP * .02, hp + fw * .01 + UP * .015,
                                           hp + fw * .06 + UP * -.005, hp + fw * .09 + UP * -.02],
                      [.04, .047, .04, .012], coll, M['hoof'], sides=8, su=.85, jit=.02, bev=.004)
            chunk(f'BW_Dewclaw_{t}', [F + V((0, .07, .07)), F + V((0, .1, .03))], [.022, 0.], coll, M['hoof'],
                  sides=5, jit=0)
            tuft(f'BW_Ankle_Tuft_{t}', F + d * -.06 + UP * .05, (0, .1, -1), 9, .1, .02, coll, M['quill'],
                 spread=.6, droop=.05)
        with part(f'Shin_{t}'):
            tuft(f'BW_Hock_Tuft_{t}', Ho + V((0, .04, .05)), (0, 1, -.6), 5, .12, .02, coll, M['quill'],
                 spread=.4)


# ---------------------------------------------------------------- гребень игл

def build_quills(M, coll, fit):
    ridge = catmull([(0, -.3, 1.97), (0, -.12, 2.02), (0, .1, 1.92), (0, .25, 1.72), (0, .3, 1.48),
                     (0, .28, 1.25), (0, .2, 1.1)], 40)
    g = rng('BW_Quills')
    count = 0
    for i, p in enumerate(ridge):
        u = i / 40
        if u < .14:
            bone = 'Head'
        elif u < .3:
            bone = 'Neck'
        elif u < .7:
            bone = 'Chest'
        else:
            bone = 'Spine'
        # Корень иглы садится на поверхность лучом изнутри-наружу: сверху для головы, сзади для спины.
        nrm_guess = V((0, .25 + u, 1.1 - u)).normalized()
        loc, nrm = fit.hit(p + nrm_guess * .6, -nrm_guess)
        if loc is None:
            continue
        base_len = .2 + .32 * math.sin(math.pi * min(1., u * 1.25)) ** .7
        rows = 3 if u < .75 else 2
        for j in range(rows):
            lat = (j - (rows - 1) / 2) * (.08 if u < .75 else .1)
            sx = lat + g.uniform(-.02, .02)
            root, rn = fit.hit(V((sx, p.y, p.z)) + nrm_guess * .6, -nrm_guess)
            if root is None:
                continue
            d = (rn * .35 + V((sx * 3, .55 + .3 * u, .7 - .5 * u))).normalized()
            ln = base_len * g.uniform(.8, 1.1) * (1 - abs(lat) * 1.6)
            with part(bone):
                quill(f'BW_Quill_{count}', root - rn * .02, d, ln, .022 + .012 * ln, coll, M['quill'],
                      bend=V((0, ln * .12, -ln * .05)))
            count += 1
    # Дополнительный веер у затылка, как у гривы на концепте.
    for j in range(7):
        ang = math.radians(-60 + 20 * j)
        d = V((math.sin(ang) * .9, .45, math.cos(ang))).normalized()
        root = V((math.sin(ang) * .1, -.05, 1.9))
        with part('Head'):
            quill(f'BW_Crown_Quill_{j}', root, d, .3 + .1 * math.cos(ang), .028, coll, M['quill'],
                  bend=V((0, .06, -.02)))


# ---------------------------------------------------------------- экипировка

def build_belt_and_skirt(M, coll, fit):
    zc = 1.02
    ring_pts, ring_n = [], []
    N = 40
    for i in range(N):
        ang = 2 * math.pi * i / N
        rs = [fit.radial((0, 0, z), ang)[0] for z in (zc - .06, zc, zc + .06)]
        r = max(rs) + .02
        d = V((math.cos(ang), math.sin(ang), 0))
        ring_pts.append(V((0, 0, zc - .065)) + d * r)
        ring_n.append(d)
    verts, faces = [], []
    for i in range(N):
        d = ring_n[i]
        p = ring_pts[i]
        verts += [p, p + d * .035, p + d * .035 + UP * .13, p + UP * .13]
    for i in range(N):
        a, c = i * 4, ((i + 1) % N) * 4
        for k in range(4):
            faces.append((a + k, c + k, c + (k + 1) % 4, a + (k + 1) % 4))
    belt = mesh_obj('BW_Belt', verts, faces, coll, M['leather'])
    bv = belt.modifiers.new('Bevel', 'BEVEL')
    bv.width, bv.segments, bv.limit_method = .008, 2, 'ANGLE'
    auto_weights(belt, ['Pelvis', 'Spine'])
    front = ring_pts[N * 3 // 4] + ring_n[N * 3 // 4] * .04 + UP * .065
    with part('Pelvis'):
        box('BW_Buckle_Plate', tuple(front + V((0, -.005, 0))), (.13, .02, .11), coll, M['bronze'], bevel=.01)
        torus('BW_Belt_Ring', front + V((0, -.035, -.055)), (0, 1, 0), .055, .013, coll, M['bronze'])
        for i in range(0, N, 4):
            if abs(i - N * 3 // 4) < 2:
                continue
            sphere(f'BW_Belt_Stud_{i}', ring_pts[i] + ring_n[i] * .04 + UP * .065, .014, coll, M['bronze'])
    # Юбка: кожаные полосы, свисающие с ремня, огибают бедра лучами по поверхности.
    flaps = [(-108, -72, .42, 'Pelvis'), (-140, -110, .38, 'Thigh_R'), (-70, -40, .38, 'Thigh_L'),
             (-175, -143, .34, 'Thigh_R'), (-37, -5, .34, 'Thigh_L'), (60, 120, .4, 'Pelvis'),
             (125, 170, .34, 'Pelvis'), (10, 55, .34, 'Pelvis')]
    g = rng('BW_Skirt')
    for fi, (a0, a1, L, bone) in enumerate(flaps):
        cols, rows = 6, 6
        verts, faces = [], []
        prev_r = {}
        for j in range(rows + 1):
            v = j / rows
            z = zc - .06 - v * L
            for i in range(cols + 1):
                ang = math.radians(a0 + (a1 - a0) * i / cols)
                d = V((math.cos(ang), math.sin(ang), 0))
                r = fit.radial((0, 0, z), ang)[0] + .03
                r = max(r, prev_r.get(i, 0.) + .012 * (1 if j else 0))
                prev_r[i] = r
                jag = (g.uniform(-.03, .02) if j == rows else 0.)
                verts.append(d * r + V((0, 0, z + jag)))
        for j in range(rows):
            for i in range(cols):
                a = j * (cols + 1) + i
                faces.append((a, a + 1, a + cols + 2, a + cols + 1))
        with part(bone):
            ob = mesh_obj(f'BW_Skirt_{fi}', verts, faces, coll, M['leather'], smooth=True)
            so = ob.modifiers.new('Solidify', 'SOLIDIFY')
            so.thickness, so.offset = .014, 1.
            # Заклепки по низу и по краям полосы.
            for i in (1, cols - 1):
                for j in (2, rows - 1):
                    p = verts[j * (cols + 1) + i]
                    d = V((p.x, p.y, 0)).normalized()
                    sphere(f'BW_Skirt_Stud_{fi}_{i}_{j}', p + d * .012, .011, coll, M['bronze'])
            p = verts[(rows - 1) * (cols + 1) + cols // 2]
            d = V((p.x, p.y, 0)).normalized()
            box(f'BW_Skirt_Plate_{fi}', tuple(p + d * .014 + UP * .04), (.07, .012, .08), coll, M['bronze'],
                rot=(0, 0, math.degrees(math.atan2(d.y, d.x)) - 90), bevel=.004)


def build_straps(M, coll, fit):
    """Перевязь от правого плеча к левому бедру спереди и сзади, кольцо на груди."""
    front, back = [], []
    for k in range(15):
        u = k / 14
        x, z = -.36 + .66 * u, 1.74 - .64 * u
        lf, nf = fit.hit((x, -2, z), (0, 1, 0))
        lb, nb = fit.hit((x, 2, z), (0, -1, 0))
        if lf is not None:
            front.append((lf + nf * .012, nf))
        if lb is not None:
            back.append((lb + nb * .012, nb))
    top = []
    for k in range(1, 6):
        y = lerp(front[0][0], back[0][0], k / 6).y
        lt, nt = fit.hit((-.36, y, 3), (0, 0, -1))
        if lt is not None:
            top.append((lt + nt * .012, nt))
    path = list(reversed(front)) + top + back
    ob = ribbon('BW_Baldric', [p for p, _ in path], [n for _, n in path], .07, .016, coll, M['strap'])
    auto_weights(ob, ['Spine', 'Chest'])
    ring_at = front[3][0]
    with part('Chest'):
        torus('BW_Chest_Ring', ring_at + front[3][1] * .02, front[3][1], .05, .012, coll, M['bronze'])
        box('BW_Chest_Clasp', tuple(ring_at + front[3][1] * .01 + UP * .05), (.06, .02, .05), coll, M['bronze'],
            bevel=.006)


def build_pauldron(M, coll):
    s = -1
    t = tag(s)
    SH, EL, WR, a = arm_pts(s)
    axis = V((s * .78, .05, .62)).normalized()
    with part(f'UpperArm_{t}'):
        c = SH + V((s * .03, 0, .06))
        shell('BW_Pauldron_Main', c, axis, .25, .16, .025, coll, M['iron'], squash=(1., 1.15), up=V((0, -1, 0)))
        torus('BW_Pauldron_Rim', c + axis * -.012, axis, .255, .016, coll, M['bronze'], segs=28)
        shell('BW_Pauldron_Cap', c + axis * .09, axis, .15, .08, .02, coll, M['bronze'], squash=(1., 1.1),
              up=V((0, -1, 0)))
        for i in range(2):
            ax2 = (axis + V((0, 0, -.55 - .35 * i))).normalized()
            c2 = c + V((s * (.06 + .04 * i), 0, -.12 - .09 * i))
            shell(f'BW_Pauldron_Lame_{i}', c2, ax2, .2 - .025 * i, .08, .018, coll, M['iron'], squash=(1., 1.2),
                  up=V((0, -1, 0)))
            torus(f'BW_Pauldron_Lame_Rim_{i}', c2 - ax2 * .008, ax2, .2 - .025 * i, .011, coll, M['bronze'],
                  segs=24)
        for i, (dy, ln) in enumerate(((-.12, .16), (.0, .22), (.12, .17))):
            base = c + axis * .14 + V((0, dy, 0))
            spikes_on(f'BW_Pauldron_Spike_{i}', base, axis + V((0, dy * 1.5, .3)), ln, .04, coll, M['bronze'])
        for i, (dy, dz) in enumerate(((-.17, -.04), (.17, -.04), (0, -.14))):
            spikes_on(f'BW_Pauldron_Spike_Low_{i}', c + V((s * .17, dy, dz)), V((s, dy * 2, .1)), .09, .025, coll,
                      M['bronze'])
        for i in range(5):
            ang = math.radians(-60 + 30 * i)
            p = c + axis * .03 + V((s * .02, .22 * math.sin(ang), .18 * math.cos(ang) * .3))
            sphere(f'BW_Pauldron_Rivet_{i}', p + axis * .1, .016, coll, M['bronze'])
    # Левое плечо: кожаная накладка на ремне.
    s = 1
    t = tag(s)
    SH, EL, WR, a = arm_pts(s)
    axis = V((s * .7, .0, .72)).normalized()
    with part(f'UpperArm_{t}'):
        c = SH + V((s * .02, 0, .04))
        shell('BW_Shoulder_Pad_L', c, axis, .17, .09, .02, coll, M['leather'], squash=(1., 1.2), up=V((0, -1, 0)))
        for i in range(3):
            sphere(f'BW_Shoulder_Pad_Stud_{i}', c + axis * .09 + V((0, -.07 + .07 * i, 0)), .014, coll,
                   M['bronze'])


def build_bracers(M, coll):
    for s in SIDES:
        t = tag(s)
        SH, EL, WR, a = arm_pts(s)
        ax = (WR - EL).normalized()
        L = (WR - EL).length
        with part(f'Forearm_{t}'):
            if s < 0:
                prof = [(.115, L * .3), (.13, L * .42), (.12, L * .7), (.098, L * .95), (.1, L * 1.02)]
                lathe(f'BW_Bracer_{t}', prof, coll, M['iron'], center=EL, axis=ax, segs=18)
                for u in (.3, 1.):
                    torus(f'BW_Bracer_Band_{u}_{t}', EL + ax * L * u, ax, .118 - .02 * u, .014, coll,
                          M['bronze'], segs=20)
                for i, u in enumerate((.45, .65, .85)):
                    out = V((s, .2, .1)).normalized()
                    spikes_on(f'BW_Bracer_Spike_{i}_{t}', EL + ax * L * u + out * .11, out, .1, .028, coll,
                              M['bronze'])
                shell('BW_Bracer_Plate_R', EL + ax * L * .62 + V((s * .1, 0, 0)), V((s, 0, 0)), .09, .03, .012,
                      coll, M['bronze'], squash=(1., 1.6), up=ax)
            else:
                prof = [(.11, L * .5), (.118, L * .58), (.1, L * .95), (.095, L * 1.02)]
                lathe(f'BW_Bracer_{t}', prof, coll, M['leather'], center=EL, axis=ax, segs=16)
                for u in (.62, .8, .95):
                    torus(f'BW_Bracer_Strap_{u}_{t}', EL + ax * L * u, ax, .116 - .022 * u, .01, coll,
                          M['strap'], segs=18)


def build(M, coll):
    torso = build_torso(M, coll)
    build_head(M, coll)
    build_arms(M, coll)
    build_legs(M, coll)
    legs = [o for o in coll.objects if o.name.startswith('BW_Leg_')]
    fit = Fit([torso])
    fit_all = Fit([torso] + legs)
    build_quills(M, coll, Fit([torso, bpy.data.objects['BW_Head']]))
    build_belt_and_skirt(M, coll, fit_all)
    build_straps(M, coll, fit)
    build_pauldron(M, coll)
    build_bracers(M, coll)


# ---------------------------------------------------------------- клипы

def _arm(p, s, x=0, y=0, z=0, fx=0, hx=0):
    t = tag(s)
    add(p, f'UpperArm_{t}', rot=(x, y, z))
    add(p, f'Forearm_{t}', rot=(fx, 0, 0))
    add(p, f'Hand_{t}', rot=(hx, 0, 0))


def idle(t):
    p = {}
    b = wave(t, 1)
    add(p, 'Spine', rot=(1. * b, 0, 0))
    add(p, 'Chest', rot=(1.5 * b, 0, 1.2 * wave(t, 1, .3)))
    add(p, 'Neck', rot=(-1.5 * b, 0, 0))
    add(p, 'Head', rot=(-2 * wave(t, 1, .15), 0, 4 * wave(t, 1, .25)))
    add(p, 'Jaw', rot=(3 + 2 * wave(t, 2), 0, 0))
    for s in SIDES:
        _arm(p, s, x=2 * wave(t, 1, .1), y=-s * (1.5 + 1.5 * b), fx=-3 - 2 * wave(t, 1, .2))
    return p


def run(t):
    p = {}
    for s in SIDES:
        tg = tag(s)
        ph = 0. if s > 0 else .5
        c = wave(t, 1, ph)
        lift = max(0., -wave(t, 1, ph + .25))
        # c > 0 - нога сзади; подъем в фазе переноса (нога идет вперед).
        add(p, f'Thigh_{tg}', rot=(-6 + 30 * c - 18 * lift, 0, 0))
        add(p, f'Shin_{tg}', rot=(10 * lift + 8 * max(0., c), 0, 0))
        add(p, f'Foot_{tg}', rot=(-10 * lift + 14 * max(0., c), 0, 0))
        _arm(p, s, x=-24 * c - 8, y=-s * 8, fx=-20 - 10 * max(0., -c))
    add(p, 'Root', loc=(0, 0, .045 * abs(wave(t, 2, .1))))
    add(p, 'Pelvis', rot=(0, 0, 6 * wave(t, 1)))
    add(p, 'Spine', rot=(8 + 2 * wave(t, 2), 0, -4 * wave(t, 1)))
    add(p, 'Chest', rot=(4, 0, -4 * wave(t, 1)))
    add(p, 'Neck', rot=(-8, 0, 0))
    add(p, 'Head', rot=(-4 - 2 * wave(t, 2), 0, 2 * wave(t, 1)))
    add(p, 'Jaw', rot=(6, 0, 0))
    return p


def attack(t):
    p = {}
    wind = pulse(t, 0., .36, .52)
    strike = pulse(t, .4, .55, .95)
    add(p, 'Spine', rot=(-6 * wind + 12 * strike, 0, 10 * wind - 8 * strike))
    add(p, 'Chest', rot=(-6 * wind + 10 * strike, 0, 8 * wind - 6 * strike))
    add(p, 'Neck', rot=(4 * wind - 8 * strike, 0, 0))
    add(p, 'Head', rot=(-8 * wind, 0, -6 * wind))
    add(p, 'Jaw', rot=(4 + 20 * wind + 10 * strike, 0, 0))
    add(p, 'Root', loc=(0, .05 * wind - .1 * strike, 0))
    _arm(p, -1, x=-150 * wind - 35 * strike, y=12 * wind, fx=-70 * wind - 5 * strike, hx=-30 * wind + 20 * strike)
    _arm(p, 1, x=20 * wind - 25 * strike, y=-20 * wind - 10 * strike, fx=-30 * wind - 20 * strike)
    for s in SIDES:
        tg = tag(s)
        add(p, f'Thigh_{tg}', rot=(-10 * strike if s < 0 else 8 * strike, 0, 0))
        add(p, f'Shin_{tg}', rot=(14 * strike, 0, 0))
        add(p, f'Foot_{tg}', rot=(-4 * strike, 0, 0))
    return p


def death(t):
    p = {}
    hit = pulse(t, 0., .14, .35)
    fall = window(t, .18, .62)
    settle = window(t, .55, .85)
    add(p, 'Root', rot=(0, 82 * ease(fall), -10 * fall), loc=(.06 * fall, 0, .3 * ease(fall) - .02 * settle))
    add(p, 'Spine', rot=(-10 * hit + 6 * fall, 0, 0))
    add(p, 'Chest', rot=(-8 * hit, 0, 0))
    add(p, 'Neck', rot=(-10 * hit, 18 * fall, 0))
    add(p, 'Head', rot=(-12 * hit + 8 * settle, 12 * fall, 0))
    add(p, 'Jaw', rot=(10 * hit + 18 * settle, 0, 0))
    _arm(p, 1, x=-40 * fall, y=-110 * fall, fx=-20 * fall)
    _arm(p, -1, x=-30 * fall + 10 * settle, y=20 * fall, fx=-40 * fall)
    for s in SIDES:
        tg = tag(s)
        add(p, f'Thigh_{tg}', rot=(-25 * fall - 10 * hit, 0, 0))
        add(p, f'Shin_{tg}', rot=(30 * fall, 0, 0))
        add(p, f'Foot_{tg}', rot=(-10 * fall, 0, 0))
    return p


def clips():
    return {'Idle': (60, idle), 'Run': (22, run), 'Attack': (34, attack), 'Death': (48, death)}

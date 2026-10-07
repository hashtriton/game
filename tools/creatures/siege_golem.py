"""Осадный голем: приземистый бронзово-железный автомат с огромными таранными кулаками и горном внутри."""
import math

from mathutils import Euler

from kit import (SIDES, UP, V, add, ease, flat, lathe, lerp, mesh_obj, painted, part, rng, wave)

SLUG = 'siege-golem'
TITLE = 'Осадный голем'
UNITY_NAME = 'SiegeGolem'
HEIGHT = 3.2
GAME_TRIS = 15000

X = V((1, 0, 0))
FWD = V((0, -1, 0))


def _arm_points(s):
    P = V((s * 1.06, 0, 2.42))
    E = V((s * 1.2, -.04, 1.78))
    W = V((s * 1.26, -.2, 1.02))
    F = V((s * 1.26, -.28, .5))
    return P, E, W, F


def _leg_points(s):
    H = V((s * .46, 0, 1.28))
    K = V((s * .5, -.06, .78))
    A = V((s * .52, 0, .28))
    T = V((s * .52, -.48, .1))
    return H, K, A, T


def _bones():
    bones = [('Root', (0, 0, 0), (0, 0, .4), None),
             ('Pelvis', (0, 0, 1.25), (0, 0, 1.6), 'Root'),
             ('Torso', (0, 0, 1.6), (0, 0, 2.6), 'Pelvis'),
             ('Head', (0, -.24, 2.62), (0, -.24, 3.0), 'Torso')]
    for s in SIDES:
        t = 'LR'[s > 0]
        P, E, W, F = _arm_points(s)
        bones += [(f'UpperArm_{t}', tuple(P), tuple(E), 'Torso'), (f'Forearm_{t}', tuple(E), tuple(W), f'UpperArm_{t}'),
                  (f'Fist_{t}', tuple(W), tuple(F), f'Forearm_{t}')]
        H, K, A, T = _leg_points(s)
        bones += [(f'Thigh_{t}', tuple(H), tuple(K), 'Pelvis'), (f'Shin_{t}', tuple(K), tuple(A), f'Thigh_{t}'),
                  (f'Foot_{t}', tuple(A), tuple(T), f'Shin_{t}')]
    return bones


BONES = _bones()


def materials():
    return {
        'iron': painted('SG Dark iron', [(0., (.035, .033, .032)), (.4, (.075, .07, .066)), (.72, (.12, .11, .1)),
                                        (1., (.2, .18, .16))],
                        scale=6., rough=.48, metal=1., edge=(.55, .5, .44), edge_amount=.6, bump=.4,
                        bump_scale=24., ao=.55, warp=.5),
        'bronze': painted('SG Weathered bronze', [(0., (.12, .06, .025)), (.35, (.32, .17, .055)),
                                                  (.7, (.55, .33, .1)), (1., (.74, .5, .2))],
                          scale=8., rough=.34, metal=1., edge=(1., .8, .45), edge_amount=.65, bump=.3,
                          bump_scale=30., ao=.5, warp=.6),
        'glow': flat('SG Furnace glow', (1., .55, .15), rough=.4, emit=(1., .42, .06), emit_strength=10.),
        'ember': painted('SG Ember core', [(0., (.012, .01, .009)), (.55, (.035, .025, .018)), (1., (.5, .2, .04))],
                         scale=7., rough=.6, metal=.0, emit=(1., .38, .05), emit_strength=7., emit_mask=.6,
                         bump=.3, ao=.6, warp=.8),
    }


# ---------------------------------------------------------------- сборка из пластин

def frame(n, up):
    """Локальный базис пластины: z - наружная нормаль, y - направление 'верха', x = y x z."""
    z = V(n).normalized()
    y = V(up) - V(up).dot(z) * z
    if y.length < 1e-6:
        y = z.orthogonal()
    y.normalize()
    return y.cross(z), y, z


CUBE_FACES = [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]

GLYPHS = [
    [((-.6, -.8), (-.6, .8)), ((-.6, .8), (.5, .2)), ((-.6, 0), (.5, -.6))],
    [((0, -.8), (0, .8)), ((-.6, .35), (.6, .35)), ((-.55, -.45), (0, -.05)), ((.55, -.45), (0, -.05))],
    [((-.6, -.7), (0, .8)), ((0, .8), (.6, -.7)), ((-.35, -.1), (.35, -.1))],
    [((-.6, .7), (.6, .7)), ((.6, .7), (-.6, -.7)), ((-.6, -.7), (.6, -.7))],
    [((0, .8), (.6, 0)), ((.6, 0), (0, -.8)), ((0, -.8), (-.6, 0)), ((-.6, 0), (0, .8)), ((0, .45), (0, -.45))],
    [((-.6, .7), (.6, .7)), ((.6, .7), (.6, -.1)), ((.6, -.1), (-.2, -.1)), ((-.2, -.1), (-.2, -.7)),
     ((-.2, -.7), (.6, -.7))],
    [((-.6, -.8), (-.6, .8)), ((.6, -.8), (.6, .8)), ((-.6, .2), (.6, -.2))],
]


class Smith:
    """Копит коробки и заклепки по материалам и выпускает по одному мешу на материал внутри part()."""

    def __init__(self, M, coll, name):
        self.M, self.coll, self.name = M, coll, name
        self.acc = {}
        self.g = rng(name)

    def _a(self, mat, kind):
        return self.acc.setdefault((mat, kind), ([], []))

    def box(self, mat, c, n, up, half, taper=None, jit=.035, kind='bevel'):
        """half = (по x, по y-верху, по z-нормали); taper=(sx, sz) сужает/расширяет край +y."""
        x, y, z = frame(n, up)
        verts, faces = self._a(mat, kind)
        base = len(verts)
        m = min(half)
        for i in range(8):
            p = [(-1, 1)[i >> 2 & 1] * half[0], (-1, 1)[i >> 1 & 1] * half[1], (-1, 1)[i & 1] * half[2]]
            if taper and p[1] > 0:
                p[0] *= taper[0]
                p[2] *= taper[1]
            # Легкая неровность углов: литье и вмятины вместо идеальных кубов.
            p = [a + self.g.uniform(-1, 1) * m * jit for a in p]
            verts.append(V(c) + x * p[0] + y * p[1] + z * p[2])
        faces += [tuple(base + k for k in f) for f in CUBE_FACES]

    def bar(self, mat, a, b, n, w, h, kind='bevel'):
        """Брусок от a до b шириной w, высотой h над плоскостью с нормалью n."""
        a, b = V(a), V(b)
        d = b - a
        self.box(mat, (a + b) / 2, n, d, (w / 2, d.length / 2 + w / 2, h / 2), jit=0., kind=kind)

    def rivet(self, p, n, r, mat='bronze'):
        x, y, z = frame(n, V(n).orthogonal())
        verts, faces = self._a(mat, 'smooth')
        base = len(verts)
        for rr, hh in ((1., -.3), (1., 0.), (.72, .5)):
            for k in range(6):
                t = 2 * math.pi * k / 6
                verts.append(V(p) + (x * math.cos(t) + y * math.sin(t)) * r * rr + z * r * hh)
        verts.append(V(p) + z * r * .75)
        for ring in range(2):
            for k in range(6):
                k2 = (k + 1) % 6
                faces.append((base + ring * 6 + k, base + ring * 6 + k2, base + ring * 6 + 6 + k2,
                              base + ring * 6 + 6 + k))
        faces += [(base + 12 + k, base + 12 + (k + 1) % 6, base + 18) for k in range(6)]

    def rune(self, c, n, up, size, glyph=None, mat='bronze', depth=.014):
        x, y, z = frame(n, up)
        gl = GLYPHS[glyph if glyph is not None else self.g.randrange(len(GLYPHS))]
        w = size * .085
        for (ax, ay), (bx, by) in gl:
            a = V(c) + (x * ax + y * ay) * size * .5 + z * depth * .5
            b = V(c) + (x * bx + y * by) * size * .5 + z * depth * .5
            self.bar(mat, a, b, n, w, depth)

    def plate(self, c, n, up, w, h, d=.06, rim=.045, rune=True, rivets=2, taper=None, inset='iron',
              frame_mat='bronze', glyph=None):
        """Литая пластина: бронзовая рамка, утопленная железная вставка, заклепки по рамке, руна."""
        c = V(c)
        x, y, z = frame(n, up)
        self.box(frame_mat, c, n, up, (w, h, d), taper)
        self.box(inset, c + z * d * .5, n, up, (w - rim, h - rim, d * .62), taper, jit=.02)
        face = d * 1.12
        tx = taper[0] if taper else 1.
        for sy in (-1, 1):
            sc = tx if sy > 0 else 1.
            for k in range(rivets):
                u = -1 + 2 * (k + .5) / rivets if rivets > 1 else 0.
                for px in ((u * (w - rim * 1.6) * sc,) if rivets > 1 else (-(w - rim * .5) * sc,
                                                                          (w - rim * .5) * sc)):
                    self.rivet(c + x * px + y * sy * (h - rim * .5) + z * d, z, rim * .32)
        if rune:
            self.rune(c + z * face, n, up, min(w, h) * 1.15, glyph)

    def done(self):
        for (mat, kind), (verts, faces) in self.acc.items():
            ob = mesh_obj(f'{self.name}_{mat}_{kind}', verts, faces, self.coll, self.M[mat], smooth=kind == 'smooth')
            if kind == 'bevel':
                b = ob.modifiers.new('Bevel', 'BEVEL')
                b.width = .018
                b.segments = 2
                b.limit_method = 'ANGLE'
        self.acc = {}


def joint(name, c, axis, r, length, coll, M):
    """Сустав: ребристый темный цилиндр, в зазорах которого просвечивает жар горна."""
    prof = [(0., -length / 2), (r * .7, -length / 2), (r, -length * .35), (r * .86, -length * .18), (r, 0.),
            (r * .86, length * .18), (r, length * .35), (r * .7, length / 2), (0., length / 2)]
    lathe(name, prof, coll, M['ember'], center=c, axis=axis, segs=14)


# ---------------------------------------------------------------- части тела

def build_torso(M, coll):
    with part('Pelvis'):
        S = Smith(M, coll, 'SG_Pelvis')
        joint('SG_Waist', (0, 0, 1.48), UP, .42, .36, coll, M)
        S.box('iron', (0, 0, 1.33), FWD, UP, (.56, .14, .38))
        S.box('bronze', (0, 0, 1.5), FWD, UP, (.6, .06, .42))
        S.plate((0, -.42, 1.27), (0, -1, -.1), UP, .26, .2, glyph=4)
        for s in SIDES:
            S.plate((s * .5, -.26, 1.3), (s * .7, -1, -.15), UP, .18, .18, rune=False, rivets=1)
            S.plate((s * .54, .2, 1.3), (s, .4, -.1), UP, .18, .18, rune=False, rivets=1)
        S.plate((0, .42, 1.3), (0, 1, -.1), UP, .3, .18, rune=False)
        S.done()
    with part('Torso'):
        S = Smith(M, coll, 'SG_Torso')
        S.box('iron', (0, .02, 2.18), FWD, UP, (.64, .5, .46), taper=(1.12, 1.05))
        # Жар за щелями грудных пластин.
        S.box('glow', (0, -.44, 2.12), FWD, UP, (.5, .26, .02), jit=0.)
        S.plate((0, -.5, 2.42), (0, -1, .25), UP, .2, .24, d=.07, glyph=1)
        for s in SIDES:
            S.plate((s * .45, -.48, 2.4), (s * .3, -1, .3), UP, .22, .26, d=.08, glyph=(0, 5)[s > 0])
            S.plate((s * .72, -.04, 2.22), (s, 0, .05), UP, .4, .36, d=.07, glyph=(3, 6)[s > 0])
            S.plate((s * .62, -.4, 2.05), (s * .6, -1, 0), UP, .14, .22, rune=False, rivets=1)
        S.plate((0, -.47, 1.97), (0, -1, -.05), UP, .36, .12, d=.07, rune=False, rivets=3)
        S.plate((0, -.43, 1.76), (0, -1, -.15), UP, .3, .1, d=.06, rune=False, rivets=3)
        for k in range(3):
            S.box('glow', (0, -.5, 2.22 - k * .045), FWD, UP, (.12 - .02 * k, .008, .02), jit=0.)
        # Горжет утапливает голову между плечами.
        S.plate((0, -.42, 2.66), (0, -1, .7), UP, .3, .1, d=.06, rune=False, rivets=3)
        for s in SIDES:
            S.plate((s * .34, -.2, 2.72), (s * .7, -.2, .8), UP, .2, .2, d=.06, rune=False, rivets=1)
        S.box('iron', (0, .06, 2.66), UP, FWD, (.56, .42, .06))
        # Спина: горн с решеткой и тяжелый горб.
        S.plate((0, .5, 2.28), (0, 1, .1), UP, .5, .3, d=.08, glyph=2)
        S.plate((0, .42, 2.6), (0, .7, .75), UP, .48, .18, d=.07, rune=False, rivets=3)
        S.box('ember', (0, .5, 1.9), V((0, 1, 0)), UP, (.36, .1, .06), jit=0.)
        for k in range(4):
            S.box('glow', (0, .56, 1.84 + k * .045), V((0, 1, 0)), UP, (.3, .01, .02), jit=0.)
            S.box('iron', (0, .58, 1.862 + k * .045), V((0, 1, 0)), UP, (.34, .012, .03), jit=0.)
        S.done()


def build_head(M, coll):
    with part('Head'):
        S = Smith(M, coll, 'SG_Head')
        c = V((0, -.26, 2.84))
        S.box('iron', c, FWD, UP, (.24, .22, .25), taper=(.86, .9))
        S.plate(c + V((0, -.27, -.02)), FWD, UP, .22, .17, d=.04, rune=False, rivets=2)
        S.box('glow', c + V((0, -.32, .05)), FWD, UP, (.17, .022, .02), jit=0.)
        S.box('iron', c + V((0, -.3, .05)), FWD, UP, (.19, .04, .015), jit=0.)
        for s in SIDES:
            # Тяжелые надбровные пластины V-образно, взгляд исподлобья.
            S.box('bronze', c + V((s * .1, -.34, .1)), FWD, V((s * .45, 0, 1)), (.11, .026, .035), jit=.02)
            S.plate(c + V((s * .26, 0, 0)), V((s, 0, 0)), UP, .18, .16, d=.04, rune=False, rivets=1)
        for k in (-1, 0, 1):
            S.box('glow', c + V((k * .075, -.31, -.1)), FWD, UP, (.016, .06, .015), jit=0.)
        for k in (-1.5, -.5, .5, 1.5):
            S.box('iron', c + V((k * .075, -.33, -.1)), FWD, UP, (.02, .07, .02), jit=0.)
        S.box('bronze', c + V((0, -.02, .25)), X, UP, (.24, .04, .03))
        S.box('iron', c + V((0, .1, .27)), UP, FWD, (.08, .08, .06))
        S.box('glow', c + V((0, .1, .335)), UP, FWD, (.055, .055, .012), jit=0.)
        S.done()


def build_arms(M, coll):
    for s in SIDES:
        t = 'LR'[s > 0]
        P, E, W, F = _arm_points(s)
        with part(f'UpperArm_{t}'):
            S = Smith(M, coll, f'SG_UpperArm_{t}')
            joint(f'SG_Shoulder_{t}', V((s * .86, 0, 2.42)), X, .32, .44, coll, M)
            # Наплечник: ступенчатые пластины, как черепица.
            S.plate((s * 1.08, 0, 2.86), (s * .45, 0, 1), FWD, .46, .52, d=.09, glyph=(2, 4)[s > 0])
            S.plate((s * 1.36, 0, 2.62), (s * .9, 0, .5), FWD, .3, .54, d=.08, rune=False, rivets=3)
            S.plate((s * 1.5, 0, 2.36), (s, 0, .12), FWD, .2, .5, d=.07, rune=False, rivets=3)
            for sy in (-1, 1):
                S.plate((s * 1.2, sy * .5, 2.6), (0, sy, 0), UP, .3, .24, d=.06, rune=False, rivets=1)
            S.box('ember', (s * 1.08, 0, 2.6), FWD, UP, (.3, .2, .46), jit=0.)
            for k in (-1, 1):
                S.rivet((s * (1.08 - .12 * k * s * 0 - .04), k * .25, 3.0), (s * .45, 0, 1), .05)
            mid = lerp(P, E, .55)
            d = E - P
            S.box('iron', mid, FWD, -d, (.26, .3, .26), taper=(1.1, 1.1))
            S.plate(mid + V((s * .27, 0, 0)), V((s, 0, 0)), -d, .22, .24, d=.06, rune=False, rivets=1)
            S.plate(mid + V((0, -.27, 0)), FWD, -d, .2, .24, d=.05, rune=False, rivets=1)
            S.box('bronze', lerp(P, E, .82), FWD, -d, (.3, .04, .3))
            S.done()
        with part(f'Forearm_{t}'):
            S = Smith(M, coll, f'SG_Forearm_{t}')
            joint(f'SG_Elbow_{t}', E, X, .26, .4, coll, M)
            d = (W - E)
            n = (FWD - FWD.dot(d.normalized()) * d.normalized()).normalized()
            c = lerp(E, W, .55)
            # Таранный наруч шире головы, расширяется к запястью.
            S.box('ember', c, n, d, (.36, .42, .36), taper=(1.12, 1.12), jit=0.)
            S.plate(c + V((s * .4, 0, 0)), V((s, 0, 0)), d, .34, .38, d=.07, taper=(1.12, 1.), glyph=(5, 1)[s > 0])
            S.plate(c + n * .4, n, d, .32, .38, d=.07, taper=(1.12, 1.), glyph=(6, 3)[s > 0])
            S.plate(c - n * .4, -n, d, .3, .36, d=.06, rune=False, rivets=2)
            S.plate(c + V((-s * .38, 0, 0)), V((-s, 0, 0)), d, .28, .34, d=.05, rune=False, rivets=1)
            S.box('bronze', lerp(E, W, .12), n, d, (.4, .05, .4))
            S.box('bronze', lerp(E, W, 1.), n, d, (.46, .06, .46))
            for k in range(3):
                S.rivet(lerp(E, W, 1.) + V((s * .47, 0, 0)) + n * (k - 1) * .25, V((s, 0, 0)), .04)
            S.done()
        with part(f'Fist_{t}'):
            S = Smith(M, coll, f'SG_Fist_{t}')
            joint(f'SG_Wrist_{t}', W + V((0, 0, -.04)), (W - F).normalized(), .28, .2, coll, M)
            pc = V((s * 1.26, -.24, .74))
            S.box('iron', pc, V((s, 0, 0)), UP, (.3, .2, .27))
            S.plate(pc + V((s * .28, 0, .02)), V((s, 0, 0)), UP, .28, .18, d=.06, glyph=(4, 2)[s > 0])
            S.plate(pc + V((0, -.32, .02)), FWD, UP, .22, .18, d=.05, rune=False, rivets=1)
            # Сжатые пальцы: три фаланги загибаются внутрь, к телу.
            for i, y in enumerate((-.48, -.33, -.18, -.03)):
                g = rng(f'{t}{i}')
                for j, (dx, dz, ang) in enumerate(((.13, -.03, 70), (-.03, -.12, 0), (-.18, -.04, -60))):
                    up = V((s * math.sin(math.radians(ang)), 0, math.cos(math.radians(ang))))
                    cc = pc + V((s * dx, y + .24 + .0 * j, .48 - .74 + dz + g.uniform(-.01, .01)))
                    S.box('iron', cc, V((0, -1, 0)), up, (.065, .09, .068))
                    if j == 0:
                        S.box('bronze', cc + V((s * .07, 0, -.01)), V((s, 0, -.5)), FWD, (.075, .06, .025))
                        S.rivet(cc + V((s * .1, 0, -.02)), V((s, 0, -.5)), .022)
            # Большой палец обхватывает спереди.
            S.box('iron', pc + V((-s * .06, -.38, -.12)), FWD, V((s * .5, 0, 1)), (.07, .14, .07))
            S.box('bronze', pc + V((-s * .1, -.4, -.22)), FWD, V((s, 0, .3)), (.075, .07, .075))
            # Таранная плита поверх костяшек.
            S.plate(pc + V((s * .28, 0, -.3)), V((s, 0, -.7)), FWD, .14, .3, d=.05, rune=False, rivets=3)
            S.done()


def build_legs(M, coll):
    for s in SIDES:
        t = 'LR'[s > 0]
        H, K, A, T = _leg_points(s)
        with part(f'Thigh_{t}'):
            S = Smith(M, coll, f'SG_Thigh_{t}')
            joint(f'SG_Hip_{t}', V((s * .4, 0, 1.28)), X, .27, .3, coll, M)
            c = lerp(H, K, .5)
            S.box('iron', c, FWD, H - K, (.28, .3, .28), taper=(1.12, 1.05))
            S.plate(c + V((0, -.3, .02)), FWD, H - K, .24, .24, d=.06, glyph=(3, 0)[s > 0])
            S.plate(c + V((s * .3, 0, 0)), V((s, 0, 0)), H - K, .24, .22, d=.06, rune=False, rivets=1)
            S.done()
        with part(f'Shin_{t}'):
            S = Smith(M, coll, f'SG_Shin_{t}')
            joint(f'SG_Knee_{t}', K, X, .22, .42, coll, M)
            S.plate(K + V((0, -.27, .04)), V((0, -1, .25)), UP, .2, .18, d=.08, rune=False, rivets=2)
            S.rivet(K + V((0, -.37, .06)), V((0, -1, .25)), .06)
            c = lerp(K, A, .55)
            S.box('iron', c, FWD, A - K, (.28, .26, .29), taper=(1.18, 1.14))
            S.plate(c + V((0, -.32, -.02)), FWD, A - K, .26, .22, d=.07, taper=(1.15, 1.), glyph=(6, 5)[s > 0])
            S.plate(c + V((s * .31, 0, 0)), V((s, 0, 0)), A - K, .24, .22, d=.06, rune=False, rivets=1)
            S.box('bronze', lerp(K, A, .95), FWD, A - K, (.34, .04, .34))
            S.done()
        with part(f'Foot_{t}'):
            S = Smith(M, coll, f'SG_Foot_{t}')
            joint(f'SG_Ankle_{t}', A + V((0, 0, -.02)), X, .2, .36, coll, M)
            fc = V((s * .52, -.12, .13))
            S.box('iron', fc, FWD, UP, (.34, .13, .44), taper=(.85, .82))
            S.plate(fc + V((0, -.04, .14)), UP, FWD, .28, .3, d=.04, rune=False, rivets=2)
            for k in (-1, 0, 1):
                S.box('iron', fc + V((k * .2, -.5, -.03)), FWD, UP, (.09, .1, .1), taper=(.8, .7))
                S.box('bronze', fc + V((k * .2, -.6, -.0)), V((0, -1, .6)), UP, (.095, .05, .02))
            S.box('iron', fc + V((0, .42, -.04)), V((0, 1, 0)), UP, (.22, .09, .08))
            S.done()


def build(M, coll):
    build_torso(M, coll)
    build_head(M, coll)
    build_arms(M, coll)
    build_legs(M, coll)


# ---------------------------------------------------------------- клипы

def curve(t, keys):
    """Кусочная кривая с плавными переходами по ключам [(t, value)]."""
    if t <= keys[0][0]:
        return keys[0][1]
    for (t0, v0), (t1, v1) in zip(keys, keys[1:]):
        if t <= t1:
            return v0 + (v1 - v0) * ease((t - t0) / max(t1 - t0, 1e-6))
    return keys[-1][1]


LEG_LEN = 1.02


def crouch(pose, s, a):
    """Присед без скольжения стопы: бедро вперед, голень назад, стопа ровно."""
    t = 'LR'[s > 0]
    add(pose, f'Thigh_{t}', rot=(-a, 0, 0))
    add(pose, f'Shin_{t}', rot=(2 * a, 0, 0))
    add(pose, f'Foot_{t}', rot=(-a, 0, 0))
    return LEG_LEN * (1 - math.cos(math.radians(a)))


def idle(t):
    p = {}
    b = wave(t, 1)
    add(p, 'Torso', rot=(1.5 * b, 0, 1.2 * wave(t, 1, .3)), loc=(0, 0, .012 * b))
    add(p, 'Head', rot=(-2 * b, 0, 5 * wave(t, 1, .15)))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'UpperArm_{tg}', rot=(-2.5 * wave(t, 1, .1), s * 1.5 * b, 0))
        add(p, f'Forearm_{tg}', rot=(-3 * wave(t, 1, .2), 0, 0))
        add(p, f'Fist_{tg}', rot=(-2 * wave(t, 1, .3), 0, 0))
    return p


def run(t):
    p = {}
    c = wave(t, 1)
    swing = 20.
    for s in SIDES:
        tg = 'LR'[s > 0]
        ph = 0. if s < 0 else .5
        cs = wave(t, 1, ph)
        lift = max(0., wave(t, 1, ph + .25))
        add(p, f'Thigh_{tg}', rot=(-swing * cs - 16 * lift, 0, 0))
        add(p, f'Shin_{tg}', rot=(34 * lift, 0, 0))
        add(p, f'Foot_{tg}', rot=(-14 * lift + 6 * cs, 0, 0))
        add(p, f'UpperArm_{tg}', rot=(14 * cs - 6, -s * 4, 0))
        add(p, f'Forearm_{tg}', rot=(-18 + 6 * cs, 0, 0))
    drop = LEG_LEN * (1 - math.cos(math.radians(swing * c)))
    add(p, 'Root', loc=(0, 0, -drop + .04 * max(0., wave(t, 2, .25))))
    add(p, 'Pelvis', rot=(0, 2.5 * c, -4 * c))
    add(p, 'Torso', rot=(9, -2 * c, 6 * c))
    add(p, 'Head', rot=(-6, 0, -4 * c))
    return p


def attack(t):
    """Сдвоенный удар кулаками сверху: замах над головой, обрушение в землю перед собой, возврат."""
    p = {}
    arm = curve(t, [(0, 0), (.34, -150), (.46, -158), (.56, -62), (.74, -62), (1, 0)])
    fore = curve(t, [(0, 0), (.34, -55), (.46, -62), (.56, -12), (.74, -12), (1, 0)])
    inward = curve(t, [(0, 0), (.34, 14), (.56, 10), (.74, 10), (1, 0)])
    torso = curve(t, [(0, 0), (.34, -12), (.46, -14), (.56, 34), (.74, 34), (1, 0)])
    knee = curve(t, [(0, 0), (.34, 6), (.56, 22), (.74, 22), (1, 0)])
    drop = 0.
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'UpperArm_{tg}', rot=(arm, -s * inward, 0))
        add(p, f'Forearm_{tg}', rot=(fore, 0, 0))
        add(p, f'Fist_{tg}', rot=(fore * .3, 0, 0))
        drop = crouch(p, s, knee)
    add(p, 'Root', loc=(0, 0, -drop))
    add(p, 'Torso', rot=(torso, 0, 0))
    add(p, 'Head', rot=(-torso * .5, 0, 0))
    return p


def death(t):
    """Колени подламываются, голем тяжело валится вперед лицом в землю."""
    p = {}
    knee = curve(t, [(0, 0), (.3, 32), (.55, 40), (1, 40)])
    fall = curve(t, [(0, 0), (.25, 0), (.72, 78), (.8, 74), (.88, 77), (1, 76)])
    sag = curve(t, [(0, 0), (.3, 12), (1, 18)])
    drop = 0.
    for s in SIDES:
        tg = 'LR'[s > 0]
        drop = crouch(p, s, knee)
        add(p, f'UpperArm_{tg}', rot=(curve(t, [(0, 0), (.3, 10), (.7, -40), (1, -36)]), s * 14 * ease(t * 1.4), 0))
        add(p, f'Forearm_{tg}', rot=(curve(t, [(0, 0), (.6, -10), (1, -30)]), 0, 0))
    add(p, 'Torso', rot=(sag, 0, 0))
    add(p, 'Head', rot=(curve(t, [(0, 0), (.4, 12), (1, -20)]), 0, 6 * ease(t)))
    add(p, 'Root', rot=(fall, 0, 0), loc=(0, curve(t, [(0, 0), (.72, -.3), (1, -.3)]),
                                          -drop + curve(t, [(0, 0), (.3, 0), (.72, .25), (1, .25)])))
    return p


def clips():
    return {'Idle': (72, idle), 'Run': (28, run), 'Attack': (40, attack), 'Death': (60, death)}

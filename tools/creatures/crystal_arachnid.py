"""Хрустальный арахнид: низкий скорпионоподобный хищник, темный изумрудный хитин, бирюзовые кристаллы."""
import math

from kit import (SIDES, UP, V, add, at_length, catmull, chunk, crystal, ease, lerp, painted, part, polyline,
                 pulse, rng, rope, side_axis, sphere, wave, window, flat)

SLUG = 'crystal-arachnid'
TITLE = 'Хрустальный арахнид'
UNITY_NAME = 'CrystalArachnid'
HEIGHT = 1.9
GAME_TRIS = 14000

TAIL_CTRL = [(0, 1.12, .44), (0, 1.42, .68), (0, 1.56, 1.05), (0, 1.48, 1.45), (0, 1.22, 1.74), (0, .88, 1.86),
             (0, .6, 1.76)]
TAIL_BOUNDS = [0, .19, .37, .54, .71, .87, 1.]
LEGS = [  # y крепления, сдвиг колена и стопы по Y, длина
    (-.28, -.3, -.62, 1.),
    (-.06, -.1, -.22, 1.04),
    (.16, .12, .26, 1.06),
    (.36, .32, .66, 1.1),
]


def _tail_curve():
    return catmull(TAIL_CTRL, 240)


def _claw_points(s):
    A = V((s * .3, -.5, .44))
    E = V((s * .76, -.74, .6))
    W = V((s * .82, -1.16, .46))
    H = V((s * .7, -1.6, .32))
    return A, E, W, H


def _leg_points(s, i):
    yb, dk, df, ln = LEGS[i]
    B = V((s * .38, yb, .42))
    K = V((s * .9 * ln, yb + dk, .86))
    A = V((s * 1.28 * ln, yb + (dk + df) * .6, .42))
    F = V((s * 1.46 * ln, yb + df, 0.))
    return B, K, A, F


def _bones():
    bones = [('Root', (0, 0, 0), (0, 0, .35), None), ('Body', (0, .3, .45), (0, -.55, .5), 'Root')]
    curve = _tail_curve()
    parent = 'Body'
    for k in range(6):
        a = at_length(curve, TAIL_BOUNDS[k])[0]
        b = at_length(curve, TAIL_BOUNDS[k + 1])[0]
        bones.append((f'Tail_{k}', tuple(a), tuple(b), parent))
        parent = f'Tail_{k}'
    end, d = at_length(curve, 1.)
    bones.append(('Stinger', tuple(end), tuple(end + V((0, -.2, -.25))), 'Tail_5'))
    for s in SIDES:
        t = 'LR'[s > 0]
        A, E, W, H = _claw_points(s)
        bones += [(f'Arm_{t}', tuple(A), tuple(E), 'Body'), (f'Forearm_{t}', tuple(E), tuple(W), f'Arm_{t}'),
                  (f'Claw_{t}', tuple(W), tuple(H), f'Forearm_{t}')]
        a = (H - W).normalized()
        sv = side_axis(a, s)
        M0 = H - sv * .1 + a * .02
        bones.append((f'Pincer_{t}', tuple(M0), tuple(M0 + a * .35 + sv * .05), f'Claw_{t}'))
        for i in range(4):
            B, K, Ak, F = _leg_points(s, i)
            bones += [(f'Leg{i}_Femur_{t}', tuple(B), tuple(K), 'Body'),
                      (f'Leg{i}_Tibia_{t}', tuple(K), tuple(F), f'Leg{i}_Femur_{t}')]
    return bones


BONES = _bones()


def materials():
    return {
        'shell': painted('CA Emerald chitin', [(0., (.004, .016, .014)), (.45, (.008, .04, .033)),
                                              (.8, (.02, .08, .062)), (1., (.035, .13, .1))],
                         scale=5., kind='voronoi', rough=.32, edge=(.05, .26, .21), edge_amount=.35, bump=.12,
                         ao=.5),
        'crystal': painted('CA Turquoise crystal', [(0., (.01, .14, .13)), (.45, (.03, .38, .34)),
                                                    (.8, (.1, .72, .62)), (1., (.4, .95, .85))],
                           scale=3., rough=.08, emit=(.05, .9, .7), emit_strength=2.2, emit_mask=.6,
                           edge=(.55, 1., .9), edge_amount=.55, bump=.03, ao=.2, warp=.6),
        'sinew': painted('CA Sinew', [(0., (.006, .012, .012)), (1., (.02, .04, .035))], scale=40., rough=.65,
                         kind='wave', bump=.4, ao=.3),
        'flesh': flat('CA Flesh', (.02, .03, .028), rough=.55),
        'eye': flat('CA Eyes', (.3, 1., .3), rough=.1, emit=(.45, 1., .25), emit_strength=6.),
    }


def cluster(name, base, normal, n, size, coll, mat, spread=.55):
    g = rng(name)
    nrm = V(normal).normalized()
    for i in range(n):
        d = (nrm + V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-1, 1))) * spread).normalized()
        ln = size * (1. if i == 0 else g.uniform(.45, .85))
        off = V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-.3, .3))) * size * .18
        crystal(f'{name}_{i}', V(base) + off, d, ln, ln * g.uniform(.15, .21), coll, mat, seed=i)


def build_body(M, coll):
    sh, cr = M['shell'], M['crystal']
    with part('Body'):
        chunk('CA_Cephalothorax', polyline((0, -.66, .46), (0, -.44, .52), (0, -.12, .56), (0, .16, .55),
                                           (0, .38, .5), n=1),
              [.2, .36, .46, .46, .38], coll, sh, sides=8, su=.52, roll=math.pi / 8)
        chunk('CA_Head_Shield', [(0, -.1, .82), (0, -.42, .78), (0, -.68, .62), (0, -.84, .5)],
              [.27, .32, .24, .06], coll, sh, sides=6, su=.32, ss=1.08, up=V((0, -.4, 1)))
        for s in SIDES:
            t = 'LR'[s > 0]
            for i, y in enumerate((-.42, -.12, .18)):
                chunk(f'CA_Flank_{i}_{t}', [(s * .1, y, .82 - .02 * i), (s * .34, y + .03, .74),
                                            (s * .53, y + .06, .54), (s * .57, y + .07, .4)],
                      [.13, .17, .14, .05], coll, sh, sides=6, su=.3, up=V((s * .7, 0, 1)), roll=math.pi / 6)
            cluster(f'CA_Shoulder_Crystal_{t}', (s * .34, -.3, .76), (s * .55, -.2, 1), 3, .58, coll, cr)
        cluster('CA_Back_Crystal', (0, .05, .84), (0, .25, 1), 4, .82, coll, cr, spread=.45)
        # Брюшко: ядро и шесть тергитов с кристаллами.
        chunk('CA_Abdomen_Core', polyline((0, .2, .42), (0, .62, .41), (0, 1.0, .39), (0, 1.22, .4), n=1),
              [.38, .4, .32, .2], coll, sh, sides=8, su=.6, roll=math.pi / 8)
        for i in range(6):
            y = .3 + i * .16
            w, h, zc = .5 - .035 * i, .34 - .03 * i, .4
            pts = [V((w * math.sin(f), y + .02 * math.cos(f), zc + h * math.cos(f)))
                   for f in [-1.35 + 2.7 * k / 6 for k in range(7)]]
            chunk(f'CA_Tergite_{i}', pts, [.06, .11, .125, .13, .125, .11, .06], coll, sh, sides=6, ss=.42,
                  up=V((0, 1, 0)), roll=math.pi / 6, jit=.06)
            if i in (1, 3, 4):
                for s in SIDES:
                    cluster(f'CA_Tergite_Crystal_{i}_{"LR"[s > 0]}', (s * w * .55, y, zc + h * .85),
                            (s * .6, .3, 1), 2, .42 - .03 * i, coll, cr)
        # Морда: полость, россыпь глаз, хелицеры.
        sphere('CA_Face', (0, -.68, .42), .15, coll, M['flesh'], scale=(1.15, .8, .7))
        for s in SIDES:
            t = 'LR'[s > 0]
            for k, (x, y, z, r) in enumerate(((.05, -.79, .5, .03), (.13, -.75, .49, .025), (.09, -.8, .44, .02),
                                              (.18, -.7, .52, .018))):
                sphere(f'CA_Eye_{k}_{t}', (s * x, y, z), r, coll, M['eye'])
            chunk(f'CA_Chelicera_{t}', [(s * .06, -.68, .38), (s * .07, -.78, .36), (s * .07, -.84, .34)],
                  [.05, .055, .04], coll, sh, sides=5)
            fang = catmull([(s * .07, -.83, .33), (s * .09, -.9, .26), (s * .07, -.92, .15)], 6)
            chunk(f'CA_Fang_{t}', fang, [.04 * (1 - k / 6) ** .7 for k in range(7)], coll, cr, sides=5)


def build_tail(M, coll):
    sh, cr = M['shell'], M['crystal']
    curve = _tail_curve()
    for k in range(6):
        u0, u1 = TAIL_BOUNDS[k], TAIL_BOUNDS[k + 1]
        R = .25 - .015 * k
        gap = .012
        with part(f'Tail_{k}'):
            pts = [at_length(curve, u0 + gap + (u1 - u0 - 2 * gap) * t / 4)[0] for t in range(5)]
            chunk(f'CA_Tail_{k}', pts, [R * .72, R * .96, R, R * .95, R * .72], coll, sh, sides=6, ss=.92,
                  up=V((1, 0, 0)), jit=.08)
            mid, d = at_length(curve, (u0 + u1) / 2)
            out = d.cross(V((1, 0, 0))).normalized()
            if out.dot(V((0, .6, 1))) < 0:
                out = -out
            crystal(f'CA_Tail_Crystal_{k}', mid + out * R * .7, out + d * .35, .36 - .02 * k, .08, coll, cr)
            for s in SIDES:
                crystal(f'CA_Tail_Side_{k}_{"LR"[s > 0]}', mid + out * R * .3 + V((s * R * .8, 0, 0)),
                        out * .5 + V((s, 0, 0)), .12, .03, coll, cr)
            if k:
                p, d = at_length(curve, u0)
                rope(f'CA_Tail_Joint_{k}', p, d, R * .78, coll, M['sinew'])
    end, d = at_length(curve, 1.)
    with part('Stinger'):
        rope('CA_Tail_Joint_6', end, d, .1, coll, M['sinew'])
        b = [end + d * t for t in (.02, .1, .2, .3)]
        chunk('CA_Telson', b, [.15, .25, .22, .1], coll, sh, sides=7, jit=.08, up=V((1, 0, 0)))
        cluster('CA_Telson_Crystal', b[2], V((0, .3, 1)), 3, .2, coll, cr)
        b0 = b[-1] - d * .02
        barb = catmull([b0, b0 + V((0, -.19, -.02)), b0 + V((0, -.34, -.16)), b0 + V((0, -.38, -.38)),
                        b0 + V((0, -.31, -.58))], 8)
        chunk('CA_Stinger', barb, [.14 * (1 - i / 8) ** .8 for i in range(9)], coll, cr, sides=6, up=V((1, 0, 0)),
              jit=.04)


def build_claws(M, coll):
    sh, cr, sinew = M['shell'], M['crystal'], M['sinew']
    for s in SIDES:
        t = 'LR'[s > 0]
        A, E, W, H = _claw_points(s)
        with part(f'Arm_{t}'):
            chunk(f'CA_Arm_{t}', polyline(A, E, n=3), [.14, .19, .19, .14], coll, sh, sides=6, roll=math.pi / 6)
            crystal(f'CA_Arm_Crystal_{t}', lerp(A, E, .6) + UP * .1, V((s * .3, .2, 1)), .22, .05, coll, cr)
        with part(f'Forearm_{t}'):
            rope(f'CA_Elbow_{t}', E, W - A, .14, coll, sinew)
            chunk(f'CA_Forearm_{t}', polyline(E, W, n=3), [.16, .22, .21, .16], coll, sh, sides=6, roll=.3)
            cluster(f'CA_Forearm_Crystal_{t}', lerp(E, W, .5) + UP * .12, V((s * .4, 0, 1)), 3, .26, coll, cr)
        a = (H - W).normalized()
        sv = side_axis(a, s)
        down = sv.cross(a) if sv.cross(a).z < 0 else -sv.cross(a)
        with part(f'Claw_{t}'):
            rope(f'CA_Wrist_{t}', W, H - E, .16, coll, sinew)
            palm = [W - a * .02, lerp(W, H, .25), lerp(W, H, .55), lerp(W, H, .85), H + a * .04]
            chunk(f'CA_Palm_{t}', palm, [.2, .38, .46, .42, .27], coll, sh, sides=7, su=.62, jit=.06,
                  roll=math.pi / 7)
            cluster(f'CA_Palm_Crystal_{t}', lerp(W, H, .5) + UP * .26 + sv * .08, UP + sv * .4, 4, .48, coll, cr)
            cluster(f'CA_Palm_Side_Crystal_{t}', lerp(W, H, .6) + sv * .36, sv + UP * .3, 3, .36, coll, cr)
            # Неподвижный палец: длинный кристаллический клинок.
            F0 = H + sv * .12 + a * .02
            fixed = catmull([F0, F0 + a * .36 + down * .02, F0 + a * .7 - sv * .1 + down * .06,
                             F0 + a * .92 - sv * .3 + down * .1], 8)
            chunk(f'CA_Blade_Fixed_{t}', fixed, [.24 * (1 - i / 8) ** .7 for i in range(9)], coll, cr, sides=5,
                  su=.5, jit=.05)
        with part(f'Pincer_{t}'):
            M0 = H - sv * .13 + a * .02
            mov = catmull([M0, M0 + a * .3 - sv * .04, M0 + a * .58 + sv * .04 + down * .03,
                           M0 + a * .76 + sv * .2 + down * .06], 8)
            chunk(f'CA_Blade_Moving_{t}', mov, [.2 * (1 - i / 8) ** .7 for i in range(9)], coll, cr, sides=5,
                  su=.5, jit=.05)


def build_legs(M, coll):
    sh, cr, sinew = M['shell'], M['crystal'], M['sinew']
    for s in SIDES:
        t = 'LR'[s > 0]
        for i in range(4):
            B, K, A, F = _leg_points(s, i)
            with part(f'Leg{i}_Femur_{t}'):
                rope(f'CA_Leg{i}_Hip_{t}', B, K - B, .13, coll, sinew)
                chunk(f'CA_Leg{i}_Femur_{t}', polyline(B, K, n=3), [.13, .16, .15, .12], coll, sh, sides=6,
                      roll=math.pi / 6)
                crystal(f'CA_Leg{i}_Spur_{t}', lerp(B, K, .7) + UP * .07, V((s * .2, .1, 1)), .3 + .05 * (i % 2),
                        .06, coll, cr)
            with part(f'Leg{i}_Tibia_{t}'):
                rope(f'CA_Leg{i}_Knee_{t}', K, A - B, .11, coll, sinew)
                chunk(f'CA_Leg{i}_Tibia_{t}', polyline(K, A, n=3), [.12, .135, .12, .1], coll, sh, sides=6,
                      roll=.4)
                rope(f'CA_Leg{i}_Ankle_{t}', A, F - K, .095, coll, sinew)
                claw = catmull([A, lerp(A, F, .5) + V((s * .05, 0, .03)), F], 6)
                chunk(f'CA_Leg{i}_Tarsus_{t}', claw, [.1 * (1 - k / 6) ** .7 for k in range(7)], coll, sh,
                      sides=6, jit=.05)
                crystal(f'CA_Leg{i}_Knee_Crystal_{t}', K + UP * .06, V((s * .5, 0, 1)), .2, .045, coll, cr)


def build(M, coll):
    build_body(M, coll)
    build_tail(M, coll)
    build_claws(M, coll)
    build_legs(M, coll)


# ---------------------------------------------------------------- клипы

def _legs(pose, t, stride, lift, speed=1.):
    """Походка тетрапод: L0, R1, L2, R3 в одной фазе. Вперед = поворот вокруг Z на -s*a, подъем = Y на -s*a."""
    for s in SIDES:
        tg = 'LR'[s > 0]
        for i in range(4):
            ph = (i + (s > 0)) % 2 * .5
            c = wave(t, speed, ph)
            up = max(0., wave(t, speed, ph + .25))
            add(pose, f'Leg{i}_Femur_{tg}', rot=(0, -s * lift * up, -s * stride * c))
            add(pose, f'Leg{i}_Tibia_{tg}', rot=(0, s * lift * .5 * up, 0))
    return pose


def idle(t):
    p = {}
    add(p, 'Root', loc=(0, 0, .015 * wave(t, 2)))
    add(p, 'Body', rot=(1.5 * wave(t, 1), 0, 0))
    for k in range(6):
        add(p, f'Tail_{k}', rot=(2.5 * wave(t, 1, -.1 * k), 0, 2. * wave(t, 1, .25 - .08 * k)))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'Arm_{tg}', rot=(-2 * wave(t, 1, .1), 0, s * 2 * wave(t, 1)))
        add(p, f'Pincer_{tg}', rot=(0, 0, -s * (6 + 6 * wave(t, 2))))
        for i in range(4):
            add(p, f'Leg{i}_Femur_{tg}', rot=(0, s * 1.5 * wave(t, 2, i * .1), 0))
    return p


def run(t):
    p = _legs({}, t, 20, 22, speed=1)
    add(p, 'Root', loc=(0, 0, .03 * abs(wave(t, 2))))
    add(p, 'Body', rot=(2 * wave(t, 2), 0, 3 * wave(t, 1)))
    for k in range(6):
        add(p, f'Tail_{k}', rot=(3 * wave(t, 2, -.08 * k), 0, 4 * wave(t, 1, -.1 * k)))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'Arm_{tg}', rot=(-8 + 4 * wave(t, 1, .5 * (s > 0)), 0, 0))
        add(p, f'Pincer_{tg}', rot=(0, 0, -s * 5))
    return p


def attack(t):
    p = {}
    wind = pulse(t, 0., .32, .5)
    strike = pulse(t, .32, .48, .9)
    add(p, 'Body', rot=(-6 * wind + 8 * strike, 0, 0))
    add(p, 'Root', loc=(0, .06 * wind - .1 * strike, 0))
    for k in range(6):
        add(p, f'Tail_{k}', rot=(-9 * wind + (14 + 4 * k) * strike, 0, 0))
    add(p, 'Stinger', rot=(-10 * wind + 35 * strike, 0, 0))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'Arm_{tg}', rot=(-18 * wind + 6 * strike, 0, -s * 10 * wind))
        add(p, f'Forearm_{tg}', rot=(0, 0, s * 12 * wind - s * 6 * strike))
        add(p, f'Pincer_{tg}', rot=(0, 0, -s * (28 * wind - 4 * strike)))
    return p


def death(t):
    p = {}
    fall = window(t, .1, .55)
    curl = window(t, .05, .8)
    add(p, 'Root', rot=(0, 22 * fall, 0), loc=(0, 0, -.26 * ease(fall)))
    add(p, 'Body', rot=(6 * fall, 0, 0))
    for k in range(6):
        add(p, f'Tail_{k}', rot=(-10 * curl, 0, 4 * curl))
    add(p, 'Stinger', rot=(-25 * curl, 0, 0))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'Arm_{tg}', rot=(14 * curl, 0, s * 18 * curl))
        add(p, f'Pincer_{tg}', rot=(0, 0, -s * 25 * curl))
        for i in range(4):
            add(p, f'Leg{i}_Femur_{tg}', rot=(0, -s * 38 * curl, 0))
            add(p, f'Leg{i}_Tibia_{tg}', rot=(0, s * 70 * curl, 0))
    return p


def clips():
    return {'Idle': (60, idle), 'Run': (24, run), 'Attack': (36, attack), 'Death': (45, death)}

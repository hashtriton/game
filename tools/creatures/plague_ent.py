"""Чумной энт: гнилое ходячее дерево, витой черный ствол, полое лицо с пастью, мертвая крона, светящиеся грибы."""
import math

from kit import (SIDES, UP, V, add, at_length, auto_weights, blob, ball, cap, catmull, ease, ell, flat, frames,
                 lathe, lerp, painted, part, pulse, rng, sphere, spike, tube, wave, window)

SLUG = 'plague-ent'
TITLE = 'Чумной энт'
UNITY_NAME = 'PlagueEnt'
HEIGHT = 3.4
GAME_TRIS = 15000

FWD = V((0, -1, 0))
HEAD = V((.22, -.6, 2.6))  # центр головы, голова смещена вправо от оси ствола


def _arm_points(s):
    S = V((s * .82, .05, 2.6))
    E = V((s * 1.3, -.02, 1.85))
    W = V((s * 1.55, -.28, 1.08))
    H = V((s * 1.64, -.48, .6))
    return S, E, W, H


def _leg_points(s):
    P = V((s * .34, .05, 1.42))
    K = V((s * .62, -.12, .72))
    A = V((s * .72, .04, .24))
    T = V((s * .78, -.36, .06))
    return P, K, A, T


def _bones():
    b = [('Root', (0, 0, 0), (0, 0, .4), None),
         ('Pelvis', (0, .05, 1.15), (0, .05, 1.6), 'Root'),
         ('Spine', (0, .05, 1.6), (0, .07, 2.15), 'Pelvis'),
         ('Chest', (0, .07, 2.15), (0, .1, 2.85), 'Spine'),
         ('Crown', (0, .1, 2.85), (0, .1, 3.3), 'Chest'),
         ('Head', tuple(HEAD + V((-.05, .42, -.1))), tuple(HEAD + V((.02, -.1, .15))), 'Chest'),
         ('Jaw', tuple(HEAD + V((0, .05, -.18))), tuple(HEAD + V((0, -.3, -.34))), 'Head')]
    for s in SIDES:
        t = 'LR'[s > 0]
        S, E, W, H = _arm_points(s)
        b += [(f'UpperArm_{t}', tuple(S), tuple(E), 'Chest'), (f'Forearm_{t}', tuple(E), tuple(W), f'UpperArm_{t}'),
              (f'Hand_{t}', tuple(W), tuple(H), f'Forearm_{t}')]
        P, K, A, T = _leg_points(s)
        b += [(f'Thigh_{t}', tuple(P), tuple(K), 'Pelvis'), (f'Shin_{t}', tuple(K), tuple(A), f'Thigh_{t}'),
              (f'Foot_{t}', tuple(A), tuple(T), f'Shin_{t}')]
    return b


BONES = _bones()


def materials():
    return {
        'bark': painted('PE Rotten bark', [(0., (.008, .006, .004)), (.35, (.026, .017, .01)),
                                          (.65, (.06, .04, .022)), (1., (.13, .09, .05))],
                        scale=7., stretch=(3., 3., .7), rough=.82, edge=(.2, .15, .1), edge_amount=.4, bump=.6,
                        bump_scale=22., ao=.55, detail=6., warp=.5),
        'heart': painted('PE Heartwood fissures', [(0., (.006, .005, .003)), (.6, (.02, .016, .008)),
                                                   (1., (.15, .3, .03))],
                         scale=5., rough=.7, emit=(.45, 1., .08), emit_strength=5., emit_mask=.62, bump=.3, ao=.6),
        'moss': painted('PE Rotten moss', [(0., (.02, .03, .006)), (.45, (.06, .09, .015)),
                                          (.8, (.13, .18, .03)), (1., (.22, .27, .05))],
                        scale=12., rough=.95, bump=.5, bump_scale=60., ao=.5, warp=.6),
        'cap': painted('PE Fungus cap', [(0., (.06, .025, .012)), (.5, (.16, .075, .035)), (1., (.32, .17, .08))],
                       scale=9., kind='voronoi', rough=.45, edge=(.45, .3, .16), edge_amount=.4, bump=.15, ao=.3),
        'glow': painted('PE Plague glow', [(0., (.22, .55, .03)), (1., (.6, 1., .15))], scale=6., rough=.3,
                        emit=(.5, 1., .1), emit_strength=6., bump=.05, ao=.15),
        'eye': flat('PE Eye light', (.7, 1., .25), rough=.2, emit=(.6, 1., .15), emit_strength=14.),
        'fang': painted('PE Splinter fang', [(0., (.05, .04, .025)), (.6, (.16, .13, .08)), (1., (.3, .26, .17))],
                        scale=14., stretch=(4., 4., 1.), rough=.6, bump=.3, ao=.3),
    }


# ---------------------------------------------------------------- помощники формы

def lump(r, u, g, k=.18, f=9.):
    """Радиус волокна с узлами вдоль длины."""
    return r * (1 + k * math.sin(u * f + g) + .5 * k * math.sin(u * f * 2.3 + 2 * g))


def strand(name, pts, r0, r1, coll, mat, g, sides=7, knots=.18, jit=.12):
    n = len(pts)
    radii = [lump(r0 + (r1 - r0) * i / (n - 1), i / (n - 1), g, knots) for i in range(n)]
    return tube(name, pts, radii, coll, mat, sides=sides, jit=jit)


def helix(axis, rad, phi0, turns, wn=1., wb=1., up=FWD, wob=0., seed=0.):
    """Точки волокна, обвивающего ось: rad(u) - радиус, N - вперед/вбок по up, B - поперек."""
    T, N, B = frames(axis, V(up))
    n = len(axis)
    out = []
    for i, p in enumerate(axis):
        u = i / (n - 1)
        ph = phi0 + 2 * math.pi * turns * u + wob * math.sin(u * 7 + seed)
        r = rad(u)
        out.append(p + N[i] * math.cos(ph) * r * wn + B[i] * math.sin(ph) * r * wb)
    return out


def interp(table, x):
    if x <= table[0][0]:
        return table[0][1]
    for (x0, y0), (x1, y1) in zip(table, table[1:]):
        if x <= x1:
            return y0 + (y1 - y0) * (x - x0) / (x1 - x0)
    return table[-1][1]


def moss_strip(name, anchor, length, width, normal, coll, mat, g):
    """Свисающая прядь гнилого мха: плоская сужающаяся лента."""
    a = V(anchor)
    nrm = V(normal).normalized()
    sway = V((g.uniform(-1, 1), g.uniform(-1, 1), 0)) * length * .15
    pts = catmull([a, a + nrm * .03 + V((0, 0, -length * .3)) + sway * .3, a + V((0, 0, -length * .65)) + sway,
                   a + V((0, 0, -length)) + sway * 1.4], 8)
    radii = [width * (1 - (i / 8) ** 1.4) * (1 + .25 * math.sin(i * 2.1 + g.random())) + .004 for i in range(9)]
    return tube(name, pts, radii, coll, mat, sides=5, up=nrm, flat_scale=(.28, 1.), jit=.18)


def moss_clump(name, center, size, coll, mat, g, n=6):
    c = V(center)
    els = [ball(c + V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-.4, .4))) * size, size * g.uniform(.5, .8))
           for _ in range(n)]
    return blob(name, els, coll, mat, resolution=max(.025, size * .22), displace=size * .35, disp_scale=size * .5)


def mushroom(name, base, normal, size, M, coll, g, glow_cap=False):
    """Гриб: ножка, шляпка и светящиеся пластинки снизу (или целиком светящийся шар-трутовик)."""
    nrm = (V(normal).normalized() + V((g.uniform(-.2, .2), g.uniform(-.2, .2), .25))).normalized()
    b = V(base)
    top = b + nrm * size * .9
    tube(name + '_Stem', [b - nrm * size * .2, lerp(b, top, .5), top], [size * .16, size * .13, size * .12], coll,
         M['moss'] if glow_cap else M['fang'], sides=6)
    if glow_cap:
        sphere(name + '_Cap', top, size * .55, coll, M['glow'], scale=(1., 1., .62))
        return
    r = size * .62
    lathe(name + '_Cap', [(r * 1.02, -.02 * size), (r * .95, .12 * size), (r * .75, .28 * size),
                          (r * .4, .4 * size), (.002, .44 * size)], coll, M['cap'], center=top, axis=nrm, segs=14)
    lathe(name + '_Gills', [(.002, .02 * size), (r * .5, -.02 * size), (r * .98, -.03 * size)], coll, M['glow'],
          center=top, axis=nrm, segs=14)


def twig(name, base, d, length, r, depth, coll, mat, g, kids=2, up_bias=.06):
    """Рекурсивная мертвая ветка: изломанная ось, сужение, побеги."""
    p = V(base)
    dr = V(d).normalized()
    ctrl = [p]
    for i in range(5):
        dr = (dr + V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-.6, 1))) * .32 + UP * up_bias).normalized()
        p = p + dr * length / 5
        ctrl.append(p)
    pts = catmull(ctrl, 15)
    radii = [max(r * (1 - .88 * (i / 15) ** .9), .006) * (1 + .12 * math.sin(i * 1.7 + g.random())) for i in
             range(16)]
    radii[-1] = 0.
    tube(name, pts, radii, coll, mat, sides=6 if r > .03 else 5, jit=.14)
    if depth > 0:
        for k in range(kids):
            u = g.uniform(.35, .85)
            q, dd = at_length(pts, u)
            cd = (dd + V((g.uniform(-1, 1), g.uniform(-1, 1), g.uniform(-.2, .8))) * .9).normalized()
            twig(f'{name}_{k}', q, cd, length * g.uniform(.38, .55), r * (1 - .8 * u) * .85, depth - 1, coll, mat, g,
                 kids=kids, up_bias=up_bias)


# ---------------------------------------------------------------- тело

TRUNK_R = [(0., .5), (.12, .54), (.3, .45), (.5, .5), (.7, .64), (.85, .62), (1., .42)]  # u по высоте 1.0..3.0


def build_trunk(M, coll):
    g = rng('PE_trunk')
    core = [ell((0, .07, 1.32), .58, (1.05, .85, .75)), ell((0, .06, 1.8), .5, (1., .85, 1.)),
            ell((0, .08, 2.35), .66, (1.25, .85, .9)), ell((0, .1, 2.78), .5, (1.1, .9, .7))]
    for s in SIDES:
        core.append(ball((s * .62, .06, 2.55), .38))
    core.append(cap((.04, -.18, 2.42), tuple(HEAD + V((-.04, .22, -.02))), .26))
    auto_weights(blob('PE_Trunk_Core', core, coll, M['heart'], resolution=.05, displace=.06, disp_scale=.35),
                 ['Pelvis', 'Spine', 'Chest'])
    axis = [V((0, .06, 1. + 2. * k / 30)) for k in range(31)]
    n = 26
    for k in range(n):
        phi0 = 2 * math.pi * k / n + g.uniform(-.1, .1)
        u0, u1 = g.uniform(0, .12), g.uniform(.86, 1.)
        ax = axis[int(u0 * 30):int(u1 * 30) + 1]
        rad = (lambda a, b: lambda u: interp(TRUNK_R, a + (b - a) * u) * 1.04)(u0, u1)
        pts = helix(ax, rad, phi0, g.uniform(.18, .3) * (1 if k % 3 else -.6), wn=.86, wb=1.2, wob=.18,
                    seed=g.random() * 6)
        auto_weights(strand(f'PE_Trunk_Strand_{k}', pts, g.uniform(.075, .11), g.uniform(.03, .06), coll,
                            M['bark'], g.random() * 6), ['Pelvis', 'Spine', 'Chest'])
    # Наплывы коры на плечах и груди.
    for s in SIDES:
        for k in range(5):
            a = V((s * (.35 + .1 * k), g.uniform(-.25, .25), 2.95 - .04 * k))
            pts = catmull([a, a + V((s * .25, g.uniform(-.1, .1), -.12)), a + V((s * .45, g.uniform(-.15, .15), -.45)),
                           a + V((s * .55, g.uniform(-.1, .1), -.8))], 12)
            with part('Chest'):
                strand(f'PE_Shoulder_Strand_{k}_{"LR"[s > 0]}', pts, .085, .05, coll, M['bark'], g.random() * 6)


def build_head(M, coll):
    h = HEAD
    with part('Head'):
        els = [ell(h + V((0, .06, .1)), .32, (1., .95, 1.15)), ell(h + V((0, -.18, .2)), .2, (1.55, .7, .55)),
               ell(h + V((0, -.24, -.06)), .18, (1.15, .95, .75)), cap(h + V((0, -.33, .2)), h + V((0, -.42, -.04)), .06)]
        for s in SIDES:
            els.append(ball(h + V((s * .17, -.2, 0)), .13))
            els.append(ball(h + V((s * .12, -.34, .08)), .085, stiff=3., neg=True))
        els.append(ell(h + V((0, -.38, -.17)), .15, (1.2, .9, .6), stiff=3., neg=True))
        blob('PE_Head', els, coll, M['bark'], resolution=.022, displace=.025, disp_scale=.12)
        for s in SIDES:
            sphere(f'PE_Eye_{"LR"[s > 0]}', h + V((s * .115, -.29, .075)), .05, coll, M['eye'], scale=(1.1, .7, .8))
        sphere('PE_Maw_Glow', h + V((0, -.27, -.18)), .11, coll, M['glow'], scale=(1.2, .6, .5))
        g = rng('PE_fangs')
        for i in range(7):
            x = -.15 + .3 * i / 6
            spike(f'PE_Fang_Up_{i}', h + V((x, -.4 + .04 * abs(x) * 3, -.11)), V((x * .4, -.2, -1)),
                  g.uniform(.09, .17), .025, coll, M['fang'])
        # Брови и волокна по черепу.
        for k in range(9):
            x = -.3 + .6 * k / 8 + g.uniform(-.03, .03)
            a = h + V((x * .8, -.3, .24))
            pts = catmull([a, a + V((x * .2, .12, .14)), a + V((x * .5, .34, .2)), a + V((x * .8, .58, .05))], 10)
            strand(f'PE_Brow_Strand_{k}', pts, .05, .028, coll, M['bark'], g.random() * 6)
        # Рог-сук с правой стороны головы и пара мелких.
        twig('PE_Head_Horn', h + V((.2, .05, .26)), V((.6, .2, 1)), .55, .07, 1, coll, M['bark'], g, kids=2)
        twig('PE_Head_Horn_B', h + V((-.18, .1, .3)), V((-.4, .3, 1)), .35, .05, 1, coll, M['bark'], g, kids=1)
    with part('Jaw'):
        g = rng('PE_jaw')
        blob('PE_Jaw', [ell(h + V((0, -.2, -.3)), .19, (1.15, 1.1, .55)), cap(h + V((0, -.32, -.33)),
                                                                            h + V((0, -.36, -.44)), .08),
                        ell(h + V((0, -.36, -.24)), .1, (1.4, .8, .5), stiff=3., neg=True)],
             coll, M['bark'], resolution=.022, displace=.02, disp_scale=.12)
        for i in range(6):
            x = -.13 + .26 * i / 5
            spike(f'PE_Fang_Low_{i}', h + V((x, -.34, -.28)), V((x * .5, -.15, 1)), g.uniform(.08, .14), .022, coll,
                  M['fang'])
        # Борода из волокон и мха под челюстью.
        for k in range(7):
            x = -.14 + .28 * k / 6
            a = h + V((x, -.3, -.4))
            L = g.uniform(.35, .7)
            pts = catmull([a, a + V((x * .3, -.04, -L * .4)), a + V((x * .5, .02, -L * .8)), a + V((x * .6, .08, -L))],
                          10)
            strand(f'PE_Beard_{k}', pts, .035, .012, coll, M['bark'] if k % 2 else M['moss'], g.random() * 6,
                   sides=5)


def build_crown(M, coll):
    g = rng('PE_crown')
    limbs = [((-.45, .05, 2.85), (-.9, .1, .8), .62, .1), ((.45, .1, 2.85), (.9, .2, .7), .6, .1),
             ((-.15, .25, 2.95), (-.25, .5, 1), .5, .085), ((.12, .22, 2.97), (.3, .35, 1), .52, .09),
             ((-.62, .15, 2.72), (-1, -.2, .25), .65, .085), ((.66, .1, 2.7), (1, -.1, .35), .6, .085),
             ((0, .35, 2.85), (0, .9, .6), .45, .08)]
    with part('Crown'):
        for i, (b, d, ln, r) in enumerate(limbs):
            twig(f'PE_Crown_{i}', b, d, ln, r, 2, coll, M['bark'], g, kids=2, up_bias=.1)
        for i in range(8):
            a = V((g.uniform(-.8, .8), g.uniform(-.2, .3), g.uniform(2.95, 3.15)))
            moss_strip(f'PE_Crown_Moss_{i}', a, g.uniform(.3, .6), .045, a - V((0, 0, 2.6)), coll, M['moss'], g)


def build_arms(M, coll):
    for s in SIDES:
        t = 'LR'[s > 0]
        g = rng('PE_arm' + t)
        S, E, W, H = _arm_points(s)
        bones = ['Chest', f'UpperArm_{t}', f'Forearm_{t}', f'Hand_{t}']
        core = [cap(S, E, .26), cap(E, W, .21), cap(W, lerp(W, H, .4), .17), ball(E, .27), ball(S, .34),
                ball(lerp(E, W, .45) + V((s * .06, 0, 0)), .22)]
        auto_weights(blob(f'PE_Arm_Core_{t}', core, coll, M['heart'], resolution=.045, displace=.05,
                          disp_scale=.3), bones)
        axis = catmull([S + V((-s * .1, 0, .15)), S, E, W, W + (W - E).normalized() * .12], 30)
        rad = lambda u: interp([(0, .34), (.2, .3), (.45, .28), (.7, .22), (1., .16)], u)
        d = (H - W).normalized()
        side = d.cross(FWD).normalized()
        fwd = side.cross(d).normalized()
        for k in range(8):
            pts = helix(axis, rad, 2 * math.pi * k / 8 + g.uniform(-.15, .15), .35 * s, wob=.2, seed=k)
            if k < 5:
                # Волокно продолжается пальцем-когтем.
                a = -.9 + 1.8 * k / 4
                spread = side * math.sin(a) * .9 + fwd * math.cos(a) * .55
                w0 = pts[-1]
                pts += catmull([w0, w0 + d * .2 + spread * .12, w0 + d * .45 + spread * .24 + fwd * .06,
                                w0 + d * .62 + spread * .26 + fwd * .2, w0 + d * .66 + spread * .22 + fwd * .34],
                               10)[1:]
                ob = strand(f'PE_Arm_Strand_{k}_{t}', pts, .085, .0, coll, M['bark'], g.random() * 6, knots=.12)
            else:
                ob = strand(f'PE_Arm_Strand_{k}_{t}', pts, .08, .03, coll, M['bark'], g.random() * 6)
            auto_weights(ob, bones)
        with part(f'Forearm_{t}'):
            for k in range(3):
                q = lerp(E, W, .25 + .25 * k)
                out = (V((s, 0, 0)) * .8 + V((0, g.uniform(-.5, .5), g.uniform(-.2, .6)))).normalized()
                twig(f'PE_Arm_Twig_{k}_{t}', q + out * .2, out, g.uniform(.25, .4), .045, 1, coll, M['bark'], g,
                     kids=1)
        with part(f'UpperArm_{t}'):
            twig(f'PE_Elbow_Twig_{t}', E + V((s * .15, .12, .05)), V((s * .5, .5, .3)), .45, .06, 1, coll, M['bark'],
                 g, kids=2)


def build_legs(M, coll):
    for s in SIDES:
        t = 'LR'[s > 0]
        g = rng('PE_leg' + t)
        P, K, A, T = _leg_points(s)
        bones = ['Pelvis', f'Thigh_{t}', f'Shin_{t}', f'Foot_{t}']
        core = [cap(P, K, .32), cap(K, A, .27), ball(K, .33), ball(A + V((0, 0, -.04)), .3),
                ball(P + V((0, 0, .1)), .38)]
        auto_weights(blob(f'PE_Leg_Core_{t}', core, coll, M['heart'], resolution=.05, displace=.05, disp_scale=.3),
                     bones)
        axis = catmull([V((s * .2, .05, 1.65)), P, K, A + V((0, 0, .1)), A + V((0, 0, -.05))], 30)
        rad = lambda u: interp([(0, .32), (.25, .36), (.55, .32), (.8, .3), (1., .32)], u)
        n = 10
        for k in range(n):
            phi = 2 * math.pi * k / n + g.uniform(-.15, .15)
            pts = helix(axis, rad, phi, -.3 * s, wob=.2, seed=k)
            # Внизу волокно расходится корнем по земле.
            end = pts[-1]
            out = V((end.x - A.x, end.y - A.y, 0))
            out = out.normalized() if out.length > 1e-3 else V((s, 0, 0))
            out = (out + V((s * .35, -.25, 0))).normalized()
            L = g.uniform(.45, .8)
            pts += catmull([end, end + out * L * .3 + V((0, 0, -.12)), end + out * L * .65 + V((0, 0, -.2)),
                            V((*(end + out * L).xy, .03)), V((*(end + out * L * 1.15).xy, .0))], 10)[1:]
            auto_weights(strand(f'PE_Leg_Strand_{k}_{t}', pts, .1, .0, coll, M['bark'], g.random() * 6, knots=.14),
                         bones)
        with part(f'Thigh_{t}'):
            twig(f'PE_Knee_Twig_{t}', K + V((s * .22, -.15, .05)), V((s * .5, -.6, .4)), .35, .055, 1, coll,
                 M['bark'], g, kids=1)


def build_decor(M, coll):
    """Мох, грибы и светящиеся трутовики."""
    g = rng('PE_decor')
    with part('Chest'):
        for i, (c, sz) in enumerate((((-.6, -.15, 2.85), .2), ((.55, -.05, 2.9), .17), ((-.3, -.45, 2.45), .14),
                                     ((-.85, -.05, 2.55), .16), ((.1, .45, 2.7), .2))):
            moss_clump(f'PE_Moss_Chest_{i}', c, sz, coll, M['moss'], g)
        for i in range(18):
            ang = g.uniform(-2.6, 2.6)
            a = V((math.sin(ang) * .7, -math.cos(ang) * .55 + .08, g.uniform(2.35, 2.85)))
            moss_strip(f'PE_Moss_Chest_Strip_{i}', a, g.uniform(.3, .75), .05, a - V((0, .08, a.z)), coll, M['moss'],
                       g)
        # Скопление светящихся трутовиков на левом плече (со стороны -X) и на груди.
        for i in range(7):
            p = V((-.42 + g.uniform(-.17, .17), -.42 + g.uniform(-.1, .1), 2.62 + g.uniform(-.15, .2)))
            mushroom(f'PE_Glowcap_Chest_{i}', p, V((-.3, -1, .4)), g.uniform(.12, .2), M, coll, g, glow_cap=True)
        for i in range(4):
            p = V((-.68 + g.uniform(-.1, .1), -.1 + g.uniform(-.15, .15), 2.95 + g.uniform(-.05, .05)))
            mushroom(f'PE_Shroom_Shoulder_{i}', p, V((-.3, -.2, 1)), g.uniform(.16, .26), M, coll, g)
    with part('Spine'):
        for i in range(5):
            p = V((-.2 + g.uniform(-.12, .12), -.48 + g.uniform(-.05, .05), 1.85 + g.uniform(-.15, .15)))
            mushroom(f'PE_Glowcap_Belly_{i}', p, V((0, -1, .3)), g.uniform(.1, .16), M, coll, g, glow_cap=True)
        moss_clump('PE_Moss_Belly', (.15, -.45, 1.55), .2, coll, M['moss'], g)
        for i in range(8):
            a = V((g.uniform(-.4, .4), -.5, g.uniform(1.4, 1.9)))
            moss_strip(f'PE_Moss_Belly_Strip_{i}', a, g.uniform(.3, .6), .045, FWD, coll, M['moss'], g)
    for s in SIDES:
        t = 'LR'[s > 0]
        S, E, W, H = _arm_points(s)
        with part(f'Forearm_{t}'):
            moss_clump(f'PE_Moss_Forearm_{t}', lerp(E, W, .3) + V((s * .12, .05, .12)), .16, coll, M['moss'], g)
            for i in range(5):
                a = lerp(E, W, .1 + .18 * i) + V((s * .15, g.uniform(-.1, .1), 0))
                moss_strip(f'PE_Moss_Forearm_Strip_{i}_{t}', a, g.uniform(.3, .55), .04, V((s, 0, 0)), coll,
                           M['moss'], g)
            if s > 0:
                for i in range(4):
                    p = lerp(E, W, .25 + .12 * i) + V((s * .22, g.uniform(-.1, .1), .05))
                    mushroom(f'PE_Shroom_Forearm_{i}', p, V((s, -.3, .5)), g.uniform(.15, .24), M, coll, g)
        with part(f'UpperArm_{t}'):
            for i in range(4):
                a = lerp(S, E, .2 + .2 * i) + V((s * .2, g.uniform(-.1, .1), 0))
                moss_strip(f'PE_Moss_Upper_Strip_{i}_{t}', a, g.uniform(.35, .6), .045, V((s, 0, 0)), coll,
                           M['moss'], g)
        P, K, A, T = _leg_points(s)
        with part(f'Thigh_{t}'):
            moss_clump(f'PE_Moss_Thigh_{t}', lerp(P, K, .45) + V((s * .1, -.25, 0)), .17, coll, M['moss'], g)
            if s < 0:
                for i in range(3):
                    p = lerp(P, K, .35 + .15 * i) + V((s * .3, -.12, 0))
                    mushroom(f'PE_Shroom_Thigh_{i}', p, V((s, -.6, .4)), g.uniform(.13, .2), M, coll, g)
            else:
                for i in range(3):
                    p = lerp(P, K, .3 + .15 * i) + V((s * .05, -.32, 0))
                    mushroom(f'PE_Glowcap_Thigh_{i}', p, V((.3, -1, .2)), g.uniform(.1, .15), M, coll, g,
                             glow_cap=True)
        with part(f'Foot_{t}'):
            moss_clump(f'PE_Moss_Foot_{t}', A + V((0, -.15, .02)), .2, coll, M['moss'], g)


def build(M, coll):
    build_trunk(M, coll)
    build_head(M, coll)
    build_crown(M, coll)
    build_arms(M, coll)
    build_legs(M, coll)
    build_decor(M, coll)


# ---------------------------------------------------------------- клипы

def _arms(p, rot_l, rot_r, fore=(0, 0, 0)):
    for s, r in ((-1, rot_l), (1, rot_r)):
        add(p, f'UpperArm_{"LR"[s > 0]}', rot=r)
        add(p, f'Forearm_{"LR"[s > 0]}', rot=fore)
    return p


def idle(t):
    p = {}
    b = wave(t, 1)
    add(p, 'Root', loc=(0, 0, .012 * wave(t, 2)))
    add(p, 'Spine', rot=(1.5 * b, 0, 1.2 * wave(t, 1, .2)))
    add(p, 'Chest', rot=(2. * wave(t, 1, .1), 0, 0))
    add(p, 'Head', rot=(-3 * wave(t, 1, .15), 0, 6 * wave(t, 1, .35)))
    add(p, 'Jaw', rot=(6 + 5 * wave(t, 2, .2), 0, 0))
    add(p, 'Crown', rot=(1.5 * wave(t, 1, .3), 2.5 * wave(t, 1, .55), 0))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'UpperArm_{tg}', rot=(3 * wave(t, 1, .1 + .3 * (s > 0)), s * 2 * wave(t, 1, .2), 0))
        add(p, f'Forearm_{tg}', rot=(-3 * wave(t, 1, .25), 0, 0))
        add(p, f'Hand_{tg}', rot=(-4 * wave(t, 2, .1 * s), 0, 0))
    return p


def run(t):
    """Тяжелая переваливающаяся походка на месте: ноги-корни, противоход рук, крен корпуса."""
    p = {}
    for s in SIDES:
        tg = 'LR'[s > 0]
        ph = 0. if s < 0 else .5
        c = wave(t, 1, ph)
        lift = max(0., wave(t, 1, ph + .25))
        add(p, f'Thigh_{tg}', rot=(-24 * c - 22 * lift, 0, 0))
        add(p, f'Shin_{tg}', rot=(38 * lift, 0, 0))
        add(p, f'Foot_{tg}', rot=(-12 * lift + 6 * c, 0, 0))
        add(p, f'UpperArm_{tg}', rot=(22 * c - 8, 0, -s * 4))
        add(p, f'Forearm_{tg}', rot=(-12 - 8 * max(0., -c), 0, 0))
        add(p, f'Hand_{tg}', rot=(-8 * c, 0, 0))
    add(p, 'Root', loc=(0, 0, -.07 * wave(t, 1) ** 2 + .02))
    add(p, 'Pelvis', rot=(0, 5 * wave(t, 1), 6 * wave(t, 1)))
    add(p, 'Spine', rot=(8 + 2 * wave(t, 2), -3 * wave(t, 1), -7 * wave(t, 1)))
    add(p, 'Chest', rot=(4, 0, -3 * wave(t, 1)))
    add(p, 'Head', rot=(-8 + 3 * wave(t, 2, .2), 0, 4 * wave(t, 1)))
    add(p, 'Jaw', rot=(10 + 4 * wave(t, 2), 0, 0))
    add(p, 'Crown', rot=(-3 * wave(t, 2, .25), 3 * wave(t, 1, .3), 0))
    return p


def attack(t):
    """Двуручный удар сверху: замах над кроной, рев, обрушение лапами в землю, возврат."""
    p = {}
    wind = pulse(t, 0., .38, .52)
    strike = pulse(t, .4, .52, .95)
    add(p, 'Spine', rot=(-12 * wind + 22 * strike, 0, 0))
    add(p, 'Chest', rot=(-8 * wind + 12 * strike, 0, 0))
    add(p, 'Pelvis', rot=(-3 * wind + 6 * strike, 0, 0))
    add(p, 'Root', loc=(0, .05 * wind - .1 * strike, -.06 * strike))
    add(p, 'Head', rot=(-14 * wind + 6 * strike, 0, 0))
    add(p, 'Jaw', rot=(10 + 26 * wind + 8 * strike, 0, 0))
    add(p, 'Crown', rot=(6 * wind - 8 * strike, 0, 0))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'UpperArm_{tg}', rot=(-125 * wind - 18 * strike, 0, s * 25 * wind))
        add(p, f'Forearm_{tg}', rot=(-30 * wind + 5 * strike, 0, 0))
        add(p, f'Hand_{tg}', rot=(-20 * wind + 25 * strike, 0, 0))
        add(p, f'Thigh_{tg}', rot=(-10 * strike, 0, 0))
        add(p, f'Shin_{tg}', rot=(14 * strike, 0, 0))
    return p


def death(t):
    """Колени подламываются, ствол заваливается назад и с треском ложится на землю."""
    p = {}
    buckle = pulse(t, 0., .25, .6)
    fall = window(t, .15, .75)
    f2 = fall * fall
    add(p, 'Root', rot=(-84 * f2, 0, 6 * fall), loc=(0, .1 * fall, -.12 * buckle + .52 * ease(fall)))
    add(p, 'Spine', rot=(-6 * fall, 0, 0))
    add(p, 'Head', rot=(-10 * fall + 6 * buckle, 0, 18 * fall))
    add(p, 'Jaw', rot=(30 * window(t, .1, .5), 0, 0))
    add(p, 'Crown', rot=(-10 * window(t, .6, .9), 0, 0))
    for s in SIDES:
        tg = 'LR'[s > 0]
        add(p, f'Thigh_{tg}', rot=(-30 * buckle - 40 * fall, 0, 0))
        add(p, f'Shin_{tg}', rot=(40 * buckle + 25 * fall, 0, 0))
        add(p, f'UpperArm_{tg}', rot=(-40 * fall + 20 * buckle, 0, s * 30 * fall))
        add(p, f'Forearm_{tg}', rot=(-25 * fall, 0, 0))
    return p


def clips():
    return {'Idle': (72, idle), 'Run': (32, run), 'Attack': (44, attack), 'Death': (54, death)}

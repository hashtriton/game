"""Общие помощники для существ-кентавров (молодой драконид, ледяной гигант).

Радиусы blob-элементов здесь задаются видимыми: при threshold .6 и stiffness 2 поверхность одиночного
metaball лежит на .574 r (замерено в Blender 5.2), поэтому B/C/E пересчитывают видимый размер в r.
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector
from mathutils.bvhtree import BVHTree

from kit import UP, V, ball, cap, catmull, chunk, ell, mesh_obj, rng, tube

K = 1 / .574


def B(co, vr, neg=False):
    return ball(co, vr * K, neg=neg)


def C(a, b, vr):
    return cap(a, b, vr * K)


def E(co, semi, rot=(0, 0, 0), neg=False):
    return ell(co, K, semi, rot, neg=neg)


def EA(co, semi, x_axis, z_hint=UP):
    """Эллипсоид с полуосями (вдоль x_axis, поперек, вдоль z_hint) - удобно для мышц вдоль кости."""
    x = V(x_axis).normalized()
    y = V(z_hint).cross(x)
    if y.length < 1e-6:
        y = x.orthogonal()
    y.normalize()
    z = x.cross(y)
    q = Matrix((x, y, z)).transposed().to_quaternion()
    d = dict(type='ELLIPSOID', co=co, r=K, size=semi, stiff=2., neg=False)
    d['quat'] = q
    return d


def blob_q(name, elements, coll, mat, resolution=.03):
    """kit.blob с поддержкой поворота эллипсоида кватернионом (ключ quat)."""
    from kit import blob
    plain = []
    quats = []
    for e in elements:
        e = dict(e)
        q = e.pop('quat', None)
        quats.append(q)
        plain.append(e)
    if not any(quats):
        return blob(name, plain, coll, mat, resolution=resolution)
    # Повороты задаются через Euler, поэтому переводим кватернион в градусы.
    for e, q in zip(plain, quats):
        if q is not None:
            e['rot'] = tuple(math.degrees(a) for a in q.to_euler('XYZ'))
    return blob(name, plain, coll, mat, resolution=resolution)


# ---------------------------------------------------------------- поверхность

def bvh_of(ob):
    me = ob.data
    return BVHTree.FromPolygons([ob.matrix_world @ v.co for v in me.vertices], [p.vertices for p in me.polygons])


def on_surface(bvh, off=0.):
    def proj(p):
        loc, nrm, _, _ = bvh.find_nearest(V(p))
        return loc + nrm * off, nrm
    return proj


def on_sphere(center, R):
    center = V(center)

    def proj(p):
        n = (V(p) - center).normalized()
        return center + n * R, n
    return proj


def ray(bvh, origin, direction):
    loc, nrm, _, _ = bvh.ray_cast(V(origin), V(direction).normalized())
    return loc, nrm


def ribbon(name, pts, nrms, width, thick, coll, mat, closed=False, taper=None, bevel=.004):
    """Ремень/пластина вдоль точек на поверхности: прямоугольное сечение, плоскость по нормали."""
    n = len(pts)
    verts, faces = [], []
    for i, p in enumerate(pts):
        a = pts[(i + 1) % n] if closed else pts[min(i + 1, n - 1)]
        b = pts[(i - 1) % n] if closed else pts[max(i - 1, 0)]
        t = (a - b).normalized()
        nr = V(nrms[i]).normalized()
        s = t.cross(nr).normalized()
        w = width * (taper[i] if taper else 1.) / 2
        verts += [p - s * w, p + s * w, p + s * w + nr * thick, p - s * w + nr * thick]
    for i in range(n if closed else n - 1):
        j = (i + 1) % n
        a, c = 4 * i, 4 * j
        faces += [(a + k, a + (k + 1) % 4, c + (k + 1) % 4, c + k) for k in range(4)]
    if not closed:
        faces += [(3, 2, 1, 0), tuple(4 * (n - 1) + k for k in range(4))]
    ob = mesh_obj(name, verts, faces, coll, mat)
    if bevel:
        m = ob.modifiers.new('Bevel', 'BEVEL')
        m.width = bevel
        m.segments = 1
        m.limit_method = 'ANGLE'
    return ob


def hugged(proj, guide, off=0.):
    pts, nrms = [], []
    for g in guide:
        p, n = proj(g)
        pts.append(p + n * off)
        nrms.append(n)
    return pts, nrms


def patch(name, proj, center, U, Vv, coll, mat, nu=9, nv=7, off=.01, thick=.02, shape=None, bevel=.006,
          smooth=True, bulge=0.):
    """Пластина брони, облегающая поверхность proj: сетка center + U*a*shape(b) + V*b, толщина thick."""
    center, U, Vv = V(center), V(U), V(Vv)
    shape = shape or (lambda b: 1.)
    inner, outer = [], []
    for j in range(nv):
        b = -1 + 2 * j / (nv - 1)
        for i in range(nu):
            a = -1 + 2 * i / (nu - 1)
            p, n = proj(center + U * a * shape(b) + Vv * b)
            lift = off + bulge * (1 - a * a) * (1 - b * b)
            inner.append(p + n * lift)
            outer.append(p + n * (lift + thick))
    N = nu * nv
    verts = inner + outer
    faces = []
    idx = lambda i, j: j * nu + i  # noqa: E731
    for j in range(nv - 1):
        for i in range(nu - 1):
            q = (idx(i, j), idx(i + 1, j), idx(i + 1, j + 1), idx(i, j + 1))
            faces.append(tuple(reversed(q)))
            faces.append(tuple(N + k for k in q))
    border = ([idx(i, 0) for i in range(nu)] + [idx(nu - 1, j) for j in range(1, nv)]
              + [idx(i, nv - 1) for i in range(nu - 2, -1, -1)] + [idx(0, j) for j in range(nv - 2, 0, -1)])
    for k in range(len(border)):
        a, b = border[k], border[(k + 1) % len(border)]
        faces.append((a, b, N + b, N + a))
    ob = mesh_obj(name, verts, faces, coll, mat, smooth=smooth)
    if bevel:
        m = ob.modifiers.new('Bevel', 'BEVEL')
        m.width = bevel
        m.segments = 2
        m.limit_method = 'ANGLE'
        m.angle_limit = math.radians(40)
        m.harden_normals = True
    return ob


def blade(name, origin, d, e, outline, thick, coll, mat, inner=.62):
    """Плоский клинок с ромбическим сечением: острая кромка по контуру outline [(вдоль d, вдоль e)]."""
    origin, d, e = V(origin), V(d).normalized(), V(e).normalized()
    nrm = d.cross(e).normalized()
    pts = [origin + d * a + e * b for a, b in outline]
    c = sum(pts, V()) / len(pts)
    n = len(pts)
    verts = list(pts)
    verts += [c + (p - c) * inner + nrm * thick for p in pts]
    verts += [c + (p - c) * inner - nrm * thick for p in pts]
    verts += [c + nrm * thick * 1.1, c - nrm * thick * 1.1]
    cf, cb = 3 * n, 3 * n + 1
    faces = []
    for i in range(n):
        j = (i + 1) % n
        faces += [(i, j, n + j, n + i), (j, i, 2 * n + i, 2 * n + j), (n + i, n + j, cf), (2 * n + j, 2 * n + i, cb)]
    return mesh_obj(name, verts, faces, coll, mat)


def band(name, a, b, r, coll, mat, rim=.012, segs=18, flare=0.):
    """Кольцевой наруч/обойма между точками a и b вокруг оси: с бортиками по краям."""
    a, b = V(a), V(b)
    ax = b - a
    L = ax.length
    prof = [(r - .004, 0.), (r + rim, .0), (r + rim * 1.2, rim * .8), (r + rim * .55, rim * 1.6),
            (r + rim * .5 + flare * .5, L * .5), (r + rim * .55 + flare, L - rim * 1.6),
            (r + rim * 1.2 + flare, L - rim * .8), (r + rim + flare, L), (r - .004 + flare, L)]
    from kit import lathe
    return lathe(name, prof, coll, mat, center=a, axis=ax, segs=segs, smooth=False)


def grip(name, G, d, back, rh, coll, skin, nail, fr=.021, spread=.036, thumb_side=1, palm_mat=None):
    """Кисть, обхватившая древко: ладонь со стороны запястья, четыре пальца и большой палец вокруг оси d."""
    G, d = V(G), V(d).normalized()
    u = V(back) - G
    u = (u - u.dot(d) * d).normalized()
    w = d.cross(u)
    R = rh + fr * .9
    objs = []
    for k in range(4):
        off = (k - 1.5) * spread
        ang = [math.radians(a) for a in (-5, 40, 90, 140, 190, 235)]
        pts = [G + d * off + (u * math.cos(t) + w * math.sin(t)) * R * (1.25 if i == 0 else 1.)
               for i, t in enumerate(ang)]
        rad = [fr * 1.25, fr * 1.15, fr * 1.08, fr, fr * .95, fr * .8]
        objs.append(tube(f'{name}_Finger{k}', catmull(pts, 12), [rad[min(5, int(i / 2.2))] for i in range(13)],
                         coll, skin, sides=8))
        tip = G + d * off + (u * math.cos(ang[-1]) + w * math.sin(ang[-1])) * R
        tdir = -u * math.sin(ang[-1]) + w * math.cos(ang[-1])
        objs.append(chunk(f'{name}_Nail{k}', [tip - tdir * .005, tip + tdir * .025 - (tip - G).normalized() * .01],
                          [fr * .6, 0.], coll, nail, sides=5, jit=0., bev=0))
    # Большой палец: идет от ладони в обратную сторону и ложится поверх пальцев.
    th0 = -d * thumb_side * 2.4 * spread
    ang = [math.radians(a) for a in (10, -30, -75, -120)]
    pts = [G + th0 * (1 - i * .18) + (u * math.cos(t) - w * math.sin(t)) * R * (1.3 if i == 0 else 1.05)
           for i, t in enumerate(ang)]
    objs.append(tube(f'{name}_Thumb', catmull(pts, 8), [fr * 1.4 - fr * .06 * i for i in range(9)], coll, skin,
                     sides=8))
    return objs, u, w


def tuft(name, base, direction, length, r, coll, mat, bend=(0, 0, 0), sides=5, seed=0, su=1.):
    """Прядь шерсти: изогнутый сужающийся клин."""
    g = rng(name + str(seed))
    d = V(direction).normalized()
    bend = V(bend)
    base = V(base)
    pts = [base - d * r * .5, base + d * length * .35 + bend * .25, base + d * length * .7 + bend * .6,
           base + d * length + bend]
    return chunk(name, pts, [r, r * .8, r * .45, 0.], coll, mat, sides=sides, jit=.12, su=su,
                 roll=g.random() * 6.28, bev=0, cap=.2)


def smooth_obj(ob, on=True):
    ob.data.polygons.foreach_set('use_smooth', [on] * len(ob.data.polygons))
    return ob


def merge_objs(objs, name):
    """Склеивает несколько мешей одной кости в один объект (меньше объектов - быстрее сборка и экспорт)."""
    objs = [o for o in objs if o is not None]
    if len(objs) < 2:
        return objs[0] if objs else None
    bone = objs[0].get('bone')
    with bpy.context.temp_override(active_object=objs[0], selected_editable_objects=objs):
        bpy.ops.object.join()
    ob = objs[0]
    ob.name = name
    if bone is not None:
        ob['bone'] = bone
    return ob

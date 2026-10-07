"""Общий набор для процедурных существ: материалы, органика из metaball, риг, процедурные клипы, студия.

Модуль существа (tools/creatures/<id>.py) задает:
  SLUG, TITLE, UNITY_NAME, HEIGHT (ориентир высоты, м),
  BONES: [(имя, голова, хвост, родитель | None)], первая кость - корень,
  materials() -> dict, build(M, coll), clips() -> {'Idle': (кадры, fn), 'Run': ..., 'Attack': ..., 'Death': ...}
  fn(t) для t в [0, 1] возвращает {кость: (rot_deg_xyz, loc_xyz)}; оси мировые в позе покоя.
Существо смотрит в -Y (в Unity после экспорта это +Z), стоит на z=0.
"""
import math
import random
import sys
import zlib
from contextlib import contextmanager
from pathlib import Path

import bmesh
import bpy
from mathutils import Euler, Matrix, Quaternion, Vector

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import add_area, point_at  # noqa: E402,F401
from scorpion_build import (_mix, _node, at_length, catmull, chunk, frames, lerp, mesh_obj,  # noqa: E402,F401
                            polyline, rope, side_axis, sphere, spike)

V = Vector
UP = V((0, 0, 1))
SIDES = (-1, 1)
FPS = 30


def rng(name):
    return random.Random(zlib.crc32(name.encode()))


# ---------------------------------------------------------------- привязка деталей к костям

_BONE = []


@contextmanager
def part(bone):
    """Все объекты, созданные внутри блока, жестко привязываются к кости bone."""
    before = set(bpy.data.objects)
    _BONE.append(bone)
    try:
        yield
    finally:
        _BONE.pop()
        for ob in set(bpy.data.objects) - before:
            if 'bone' not in ob and ob.type == 'MESH':
                ob['bone'] = bone


def auto_weights(ob, bones=None):
    """Сплошная органика (metaball): веса считаются по близости к костям, список bones ограничивает выбор."""
    ob['bone'] = '*auto'
    if bones:
        ob['auto_bones'] = ','.join(bones)
    return ob


# ---------------------------------------------------------------- материалы

def painted(name, colors, scale=4., kind='noise', rough=.6, metal=0., emit=None, emit_strength=0.,
            emit_mask=None, bump=.25, bump_scale=30., edge=None, edge_amount=.45, ao=.4, stretch=(1, 1, 1),
            detail=4., warp=.3, alpha=1.):
    """Расписанный стилизованный материал: цветовая рампа по шуму/вороному, светлые ребра, AO, рельеф.

    colors: [(позиция, (r, g, b))]; kind: noise | voronoi | wave; emit_mask - позиция рампы, после которой
    рампа светится (маска свечения по тому же узору), иначе свечение равномерное.
    Свойства metal/rough сохраняются в материале для запекания маски Unity.
    """
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    m['metal'] = metal
    m['rough'] = rough
    m['emit_strength'] = emit_strength if emit else 0.
    m['alpha'] = alpha
    m.diffuse_color = (*colors[len(colors) // 2][1], 1)
    nt = m.node_tree
    L = nt.links
    bs = nt.nodes['Principled BSDF']
    bs.inputs['Roughness'].default_value = rough
    bs.inputs['Metallic'].default_value = metal
    coord = nt.nodes.new('ShaderNodeTexCoord')
    mp = nt.nodes.new('ShaderNodeMapping')
    mp.inputs['Scale'].default_value = stretch
    L.new(coord.outputs['Object'], mp.inputs['Vector'])
    vec = mp.outputs['Vector']
    if warp:
        w = _node(nt, 'ShaderNodeTexNoise', Scale=scale * .5, Detail=2.)
        ws = nt.nodes.new('ShaderNodeVectorMath')
        ws.operation = 'MULTIPLY_ADD'
        ws.inputs[1].default_value = (warp,) * 3
        ws.inputs[2].default_value = (-warp / 2,) * 3
        L.new(w.outputs['Color'], ws.inputs[0])
        add = nt.nodes.new('ShaderNodeVectorMath')
        add.operation = 'ADD'
        L.new(vec, add.inputs[0])
        L.new(ws.outputs[0], add.inputs[1])
        vec = add.outputs[0]
    if kind == 'voronoi':
        tex = _node(nt, 'ShaderNodeTexVoronoi', Scale=scale)
        L.new(vec, tex.inputs['Vector'])
        fac = tex.outputs['Distance']
    elif kind == 'wave':
        tex = _node(nt, 'ShaderNodeTexWave', Scale=scale, Distortion=4., Detail=detail)
        tex.bands_direction = 'Z'
        L.new(vec, tex.inputs['Vector'])
        fac = tex.outputs['Fac']
    else:
        tex = _node(nt, 'ShaderNodeTexNoise', Scale=scale, Detail=detail)
        L.new(vec, tex.inputs['Vector'])
        fac = tex.outputs['Fac']
    ramp = nt.nodes.new('ShaderNodeValToRGB')
    el = ramp.color_ramp.elements
    el[0].position, el[0].color = colors[0][0], (*colors[0][1], 1)
    el[1].position, el[1].color = colors[-1][0], (*colors[-1][1], 1)
    for pos, c in colors[1:-1]:
        e = el.new(pos)
        e.color = (*c, 1)
    L.new(fac, ramp.inputs[0])
    col = ramp.outputs['Color']
    if edge is not None:
        bev = _node(nt, 'ShaderNodeBevel', Radius=.015)
        bev.samples = 6
        geo = nt.nodes.new('ShaderNodeNewGeometry')
        dot = nt.nodes.new('ShaderNodeVectorMath')
        dot.operation = 'DOT_PRODUCT'
        L.new(bev.outputs['Normal'], dot.inputs[0])
        L.new(geo.outputs['Normal'], dot.inputs[1])
        er = _node(nt, 'ShaderNodeMapRange', **{'From Min': .96, 'From Max': .995, 'To Min': edge_amount,
                                                'To Max': 0.})
        L.new(dot.outputs['Value'], er.inputs['Value'])
        col = _mix(nt, col, edge, er.outputs['Result'])
    if ao:
        aon = _node(nt, 'ShaderNodeAmbientOcclusion', Distance=.15)
        aor = _node(nt, 'ShaderNodeMapRange', **{'To Min': 1 - ao})
        L.new(aon.outputs['AO'], aor.inputs['Value'])
        col = _mix(nt, col, aor.outputs['Result'], 1., 'MULTIPLY')
    L.new(col, bs.inputs['Base Color'])
    if emit:
        if emit_mask is None:
            bs.inputs['Emission Color'].default_value = (*emit, 1)
        else:
            mr = _node(nt, 'ShaderNodeMapRange', **{'From Min': emit_mask, 'From Max': emit_mask + .06})
            L.new(fac, mr.inputs['Value'])
            ec = _mix(nt, (0., 0., 0.), emit, mr.outputs['Result'])
            L.new(ec, bs.inputs['Emission Color'])
        bs.inputs['Emission Strength'].default_value = emit_strength
    if alpha < 1:
        bs.inputs['Alpha'].default_value = alpha
        bs.inputs['Transmission Weight'].default_value = .6
    if bump:
        bt = _node(nt, 'ShaderNodeTexNoise', Scale=bump_scale, Detail=5.)
        L.new(vec, bt.inputs['Vector'])
        b = _node(nt, 'ShaderNodeBump', Strength=bump, Distance=.004)
        L.new(bt.outputs['Fac'], b.inputs['Height'])
        L.new(b.outputs['Normal'], bs.inputs['Normal'])
    return m


def flat(name, color, rough=.5, metal=0., emit=None, emit_strength=0., alpha=1.):
    return painted(name, [(0., tuple(c * .8 for c in color)), (1., color)], scale=6., rough=rough, metal=metal,
                   emit=emit, emit_strength=emit_strength, bump=.05, ao=.25, warp=0., alpha=alpha)


# ---------------------------------------------------------------- органика

def blob(name, elements, coll, mat, resolution=.035, threshold=.6, smooth=True, displace=0., disp_scale=.6):
    """Слитная органическая форма из metaball-элементов, сразу превращенная в меш.

    elements: dict(type='BALL'|'ELLIPSOID'|'CAPSULE', co=, r=, size=(x,y,z), rot=(deg xyz) | quat, stiff=, neg=)
    CAPSULE удобнее задавать как a/b концы: dict(type='CAPSULE', a=, b=, r=).
    """
    mb = bpy.data.metaballs.new(name + '_mb')
    mb.resolution = resolution
    mb.render_resolution = resolution
    mb.threshold = threshold
    for e in elements:
        el = mb.elements.new(type=e.get('type', 'BALL'))
        el.stiffness = e.get('stiff', 2.)
        el.use_negative = e.get('neg', False)
        if el.type == 'CAPSULE' and 'a' in e:
            a, b = V(e['a']), V(e['b'])
            el.co = (a + b) / 2
            el.size_x = (b - a).length / 2
            el.rotation = V((1, 0, 0)).rotation_difference((b - a).normalized())
        else:
            el.co = e['co']
            if 'rot' in e:
                el.rotation = Euler([math.radians(x) for x in e['rot']]).to_quaternion()
            if 'size' in e:
                el.size_x, el.size_y, el.size_z = e['size']
        el.radius = e['r']
    tmp = bpy.data.objects.new(name + 'MB', mb)
    coll.objects.link(tmp)
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(tmp.evaluated_get(dg))
    bpy.data.objects.remove(tmp)
    bpy.data.metaballs.remove(mb)
    me.name = name
    me.materials.clear()
    me.materials.append(mat)
    bm = bmesh.new()
    bm.from_mesh(me)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=resolution * .05)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me)
    bm.free()
    me.polygons.foreach_set('use_smooth', [smooth] * len(me.polygons))
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    if displace:
        tex = bpy.data.textures.new(name + '_disp', 'CLOUDS')
        tex.noise_scale = disp_scale
        d = ob.modifiers.new('Displace', 'DISPLACE')
        d.texture = tex
        d.texture_coords = 'GLOBAL'
        d.strength = displace
        d.mid_level = .5
    return ob


def cap(a, b, r, stiff=2.):
    return dict(type='CAPSULE', a=a, b=b, r=r, stiff=stiff)


def ell(co, r, size, rot=(0, 0, 0), stiff=2., neg=False):
    return dict(type='ELLIPSOID', co=co, r=r, size=size, rot=rot, stiff=stiff, neg=neg)


def ball(co, r, stiff=2., neg=False):
    return dict(type='BALL', co=co, r=r, stiff=stiff, neg=neg)


def tube(name, pts, radii, coll, mat, sides=10, smooth=True, up=UP, flat_scale=(1., 1.), jit=0., caps=True,
         subsurf=0):
    """Гладкая труба вдоль ломаной (шеи, хвосты, рукояти): то же сечение, что у chunk, но сглаженное."""
    ob = chunk(name, pts, radii, coll, mat, sides=sides, su=flat_scale[0], ss=flat_scale[1], up=up, jit=jit,
               bev=0, cap=.5 if caps else .01)
    ob.data.polygons.foreach_set('use_smooth', [smooth] * len(ob.data.polygons))
    if subsurf:
        s = ob.modifiers.new('Subsurf', 'SUBSURF')
        s.levels = s.render_levels = subsurf
    return ob


def box(name, center, size, coll, mat, rot=(0, 0, 0), bevel=.02, segments=2, taper=None):
    """Пластина/блок с фаской; taper=(sx, sy) сужает верх для литых плит брони."""
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.)
    for v in bm.verts:
        v.co = V((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
        if taper and v.co.z > 0:
            v.co.x *= taper[0]
            v.co.y *= taper[1]
    R = Euler([math.radians(x) for x in rot]).to_matrix()
    for v in bm.verts:
        v.co = R @ v.co + V(center)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(mat)
    ob = bpy.data.objects.new(name, me)
    coll.objects.link(ob)
    if bevel:
        b = ob.modifiers.new('Bevel', 'BEVEL')
        b.width = bevel
        b.segments = segments
        b.limit_method = 'ANGLE'
    return ob


def crystal(name, base, direction, length, r, coll, mat, sides=6, seed=0):
    """Граненый кристалл: шестигранная призма с пирамидальной вершиной."""
    d = V(direction).normalized()
    base = V(base)
    g = rng(name + str(seed))
    pts = [base - d * r * .3, base + d * length * .15, base + d * length * (.62 + g.uniform(-.08, .08)),
           base + d * length]
    ob = chunk(name, pts, [r * .8, r, r * .92, 0.], coll, mat, sides=sides, jit=.05, bev=.004, roll=g.random())
    return ob


def lathe(name, profile, coll, mat, center=(0, 0, 0), axis=UP, segs=16, smooth=True):
    """Тело вращения: profile [(радиус, высота)] вдоль axis."""
    axis = V(axis).normalized()
    a = axis.orthogonal().normalized()
    b = axis.cross(a)
    verts, faces = [], []
    for r, h in profile:
        for i in range(segs):
            t = 2 * math.pi * i / segs
            verts.append(V(center) + axis * h + (a * math.cos(t) + b * math.sin(t)) * r)
    n = len(profile)
    for k in range(n - 1):
        for i in range(segs):
            i2 = (i + 1) % segs
            faces.append((k * segs + i, k * segs + i2, (k + 1) * segs + i2, (k + 1) * segs + i))
    ob = mesh_obj(name, verts, faces, coll, mat, smooth=smooth)
    return ob


# ---------------------------------------------------------------- риг

def build_rig(bones, coll):
    arm_data = bpy.data.armatures.new('Rig')
    arm = bpy.data.objects.new('Rig', arm_data)
    coll.objects.link(arm)
    arm.show_in_front = True
    bpy.context.view_layer.objects.active = arm
    bpy.ops.object.mode_set(mode='EDIT')
    for name, head, tail, parent in bones:
        eb = arm_data.edit_bones.new(name)
        eb.head = head
        eb.tail = tail
        eb.roll = 0.
        if parent:
            eb.parent = arm_data.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    for pb in arm.pose.bones:
        pb.rotation_mode = 'QUATERNION'
    return arm


def _segment_distance(p, a, b):
    ab = b - a
    t = max(0., min(1., (p - a).dot(ab) / max(ab.length_squared, 1e-12)))
    return (p - (a + ab * t)).length


def bind(arm, objs):
    """Жесткие детали получают одну кость с весом 1; '*auto' - смешивание двух ближайших костей."""
    names = {b.name for b in arm.data.bones}
    segs = {b.name: (b.head_local.copy(), b.tail_local.copy()) for b in arm.data.bones}
    for ob in objs:
        bone = ob.get('bone')
        if bone is None:
            raise RuntimeError(f'{ob.name}: деталь без кости')
        ob.vertex_groups.clear()
        if bone == '*auto':
            allowed = ob['auto_bones'].split(',') if 'auto_bones' in ob else sorted(names)
            groups = {n: ob.vertex_groups.new(name=n) for n in allowed}
            for v in ob.data.vertices:
                p = ob.matrix_world @ v.co
                d = sorted((_segment_distance(p, *segs[n]), n) for n in allowed)[:2]
                if len(d) == 1 or d[1][0] - d[0][0] > .12:
                    groups[d[0][1]].add([v.index], 1., 'REPLACE')
                    continue
                # Плавный переход шириной 0.12 м между двумя ближайшими костями.
                t = (d[1][0] - d[0][0]) / .12
                w0 = .5 + .5 * t
                groups[d[0][1]].add([v.index], w0, 'REPLACE')
                groups[d[1][1]].add([v.index], 1 - w0, 'REPLACE')
        else:
            if bone not in names:
                raise RuntimeError(f'{ob.name}: нет кости {bone}')
            ob.vertex_groups.new(name=bone).add(list(range(len(ob.data.vertices))), 1., 'REPLACE')
        mod = ob.modifiers.new('Armature', 'ARMATURE')
        mod.object = arm


def _rest3(arm):
    return {b.name: b.matrix_local.to_3x3() for b in arm.data.bones}


def make_clips(arm, clips, step=2):
    """Ключи поз по функциям клипов; повороты в мировых осях позы покоя переводятся в локальные оси костей."""
    rest = _rest3(arm)
    arm.animation_data_create()
    made = []
    for clip, (length, fn) in clips.items():
        act = bpy.data.actions.new(clip)
        act.use_fake_user = True
        arm.animation_data.action = act
        frames_ = sorted(set(list(range(0, length + 1, step)) + [length]))
        for f in frames_:
            pose = fn(f / length)
            for pb in arm.pose.bones:
                rot, loc = pose.get(pb.name, ((0, 0, 0), (0, 0, 0)))
                B = rest[pb.name]
                Rw = Euler([math.radians(x) for x in rot]).to_matrix()
                pb.rotation_quaternion = (B.inverted() @ Rw @ B).to_quaternion()
                pb.location = B.inverted() @ V(loc)
                pb.keyframe_insert('rotation_quaternion', frame=f)
                pb.keyframe_insert('location', frame=f)
        act.frame_range = (0, length)
        act['loop'] = clip in ('Idle', 'Run')
        made.append(act.name)
    arm.animation_data.action = bpy.data.actions['Idle']
    for pb in arm.pose.bones:
        pb.rotation_quaternion = Quaternion()
        pb.location = (0, 0, 0)
    return made


# Помощники для функций клипов.

def wave(t, cycles=1., phase=0.):
    return math.sin(2 * math.pi * (t * cycles + phase))


def ease(x):
    x = max(0., min(1., x))
    return x * x * (3 - 2 * x)


def window(t, a, b):
    """0 до a, плавно 0..1 на [a, b], затем 1."""
    return ease((t - a) / max(b - a, 1e-6))


def pulse(t, a, peak, b):
    """Плавный подъем a..peak и спад peak..b."""
    if t <= peak:
        return window(t, a, peak)
    return 1 - window(t, peak, b)


def add(pose, bone, rot=(0, 0, 0), loc=(0, 0, 0)):
    r0, l0 = pose.get(bone, ((0, 0, 0), (0, 0, 0)))
    pose[bone] = (tuple(a + b for a, b in zip(r0, rot)), tuple(a + b for a, b in zip(l0, loc)))
    return pose


# ---------------------------------------------------------------- студия

def build_studio(scene, height):
    studio = bpy.data.collections.new('STUDIO')
    scene.collection.children.link(studio)
    s = max(1., height / 1.8)
    prof = [(-25., 0.)] + [(3 * s + 3 * s * math.cos(a), 3 * s + 3 * s * math.sin(a))
                           for a in [math.radians(-90 + 90 * k / 16) for k in range(17)]] + [(6. * s, 14. * s)]
    verts = []
    for x in (-30, 30):
        verts += [(x, y, z) for y, z in prof]
    n = len(prof)
    faces = [(i, i + 1, n + i + 1, n + i) for i in range(n - 1)]
    floor_mat = flat('Studio floor', (.012, .012, .013), rough=.9)
    floor = mesh_obj('Studio_Cyclorama', verts, faces, studio, floor_mat, smooth=True)
    floor.hide_select = True
    world = bpy.data.worlds.new('Studio world')
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (.02, .022, .028, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = .8
    scene.world = world
    h = height
    # Узкий ключ без пятна на полу и сильный контровой свет, как на листе концептов.
    add_area(studio, 'Key warm', (-3.2 * s, -4.2 * s, 4.8 * s), (0, 0, h * .45), 320 * s * s, (1, .92, .82), 2 * s)
    add_area(studio, 'Fill cool', (3.8 * s, -3.2 * s, 2.2 * s), (0, 0, h * .45), 70 * s * s, (.75, .85, 1), 4 * s)
    add_area(studio, 'Rim right', (2.5 * s, 4.2 * s, 3.2 * s), (0, 0, h * .6), 1300 * s * s, (.85, .9, 1), 2 * s)
    add_area(studio, 'Rim left', (-3.0 * s, 3.2 * s, 2.6 * s), (0, 0, h * .55), 800 * s * s, (1, .85, .7), 2 * s)
    cam_data = bpy.data.cameras.new('Camera')
    cam_data.lens = 50
    cam = bpy.data.objects.new('Camera', cam_data)
    studio.objects.link(cam)
    cam.location = (-2.6 * s, -5.8 * s, 3.0 * s)
    point_at(cam, (0, 0, h * .45))
    scene.camera = cam
    return studio

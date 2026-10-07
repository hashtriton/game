"""Игровая версия существа для Unity: децимация, UV, запекание текстур, риг и клипы в один FBX.

blender --background --factory-startup --disable-autoexec --offline-mode art/creatures/<slug>/<slug>.blend \
    --python-exit-code 1 --python tools/creatures/export.py -- <id>
Существо смотрит в -Y Blender; риг разворачивается на 180 градусов, чтобы в Unity оно смотрело в +Z.
Пишет unity/Assets/Creatures/<UnityName>/: <UnityName>.fbx, _Albedo.png, _Normal.png, _Mask.png, _Emission.png;
art/creatures/<slug>/game/<slug>-game.blend; verification/creatures/<slug>-export.json.
"""
import importlib
import json
import math
import sys
from pathlib import Path

import bpy
import numpy as np

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import kit  # noqa: E402
from hero_veteran_render import configure_cycles_device  # noqa: E402

TEX = 2048
MIN_TRIS = 60


def tri_count(me):
    me.calc_loop_triangles()
    return len(me.loop_triangles)


def evaluated_copy(ob, coll, suffix):
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
    me.transform(ob.matrix_world)
    cp = bpy.data.objects.new(ob.name + suffix, me)
    for k in ('bone', 'auto_bones'):
        if k in ob:
            cp[k] = ob[k]
    coll.objects.link(cp)
    return cp


def decimate(cp, ratio):
    if ratio >= 1:
        return
    dec = cp.modifiers.new('Decimate', 'DECIMATE')
    dec.ratio = ratio
    dg = bpy.context.evaluated_depsgraph_get()
    low = bpy.data.meshes.new_from_object(cp.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
    cp.modifiers.clear()
    old = cp.data
    cp.data = low
    bpy.data.meshes.remove(old)


def join(objs, name):
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    ob = bpy.context.view_layer.objects.active
    ob.name = name
    ob.data.name = name
    return ob


def unwrap(ob):
    for o in bpy.context.view_layer.objects:
        o.select_set(o == ob)
    bpy.context.view_layer.objects.active = ob
    while ob.data.uv_layers:
        ob.data.uv_layers.remove(ob.data.uv_layers[0])
    ob.data.uv_layers.new(name='UVMap')
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(62), island_margin=.003, scale_to_bounds=True)
    bpy.ops.object.mode_set(mode='OBJECT')


def bake_target(ob, image):
    for slot in ob.material_slots:
        nt = slot.material.node_tree
        node = nt.nodes.get('CR bake target') or nt.nodes.new('ShaderNodeTexImage')
        node.name = 'CR bake target'
        node.image = image
        nt.nodes.active = node


def bake(ob, image, kind, selected=False, extrusion=0., ray=0.):
    bake_target(ob, image)
    scene = bpy.context.scene
    scene.render.bake.use_selected_to_active = selected
    scene.render.bake.cage_extrusion = extrusion
    scene.render.bake.max_ray_distance = ray
    if kind == 'DIFFUSE':
        bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, margin=8, use_clear=True)
    elif kind == 'NORMAL':
        bpy.ops.object.bake(type='NORMAL', normal_space='TANGENT', margin=8, use_clear=True)
    else:
        bpy.ops.object.bake(type=kind, margin=8, use_clear=True)


def pixels(image):
    return np.array(image.pixels[:], dtype=np.float32).reshape(image.size[1], image.size[0], 4)


def save(image, path):
    image.filepath_raw = str(path)
    image.file_format = 'PNG'
    image.save()


class Rewire:
    """Временно выводит константу материала (metal, 1-rough) или свечение без силы через Emission."""

    def __init__(self, materials, key):
        self.saved = []
        for m in materials:
            nt = m.node_tree
            out = next(n for n in nt.nodes if n.type == 'OUTPUT_MATERIAL' and n.is_active_output)
            old = out.inputs['Surface'].links[0].from_socket
            em = nt.nodes.new('ShaderNodeEmission')
            em.name = 'CR rewire'
            bs = nt.nodes['Principled BSDF']
            if key == 'emit':
                src = bs.inputs['Emission Color']
                if src.links:
                    nt.links.new(src.links[0].from_socket, em.inputs['Color'])
                else:
                    em.inputs['Color'].default_value = src.default_value if m['emit_strength'] else (0, 0, 0, 1)
            else:
                v = m[key] if key == 'metal' else 1 - m['rough']
                em.inputs['Color'].default_value = (v, v, v, 1)
            nt.links.new(em.outputs[0], out.inputs['Surface'])
            self.saved.append((nt, out, old, em))

    def restore(self):
        for nt, out, old, em in self.saved:
            nt.links.new(old, out.inputs['Surface'])
            nt.nodes.remove(em)


def single_material(ob, name, images):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nt = m.node_tree
    bs = nt.nodes['Principled BSDF']
    for img, socket in ((images['Albedo'], 'Base Color'), (images['Emission'], 'Emission Color')):
        tex = nt.nodes.new('ShaderNodeTexImage')
        tex.image = img
        nt.links.new(tex.outputs['Color'], bs.inputs[socket])
    nmap = nt.nodes.new('ShaderNodeNormalMap')
    ntex = nt.nodes.new('ShaderNodeTexImage')
    ntex.image = images['Normal']
    ntex.image.colorspace_settings.name = 'Non-Color'
    nt.links.new(ntex.outputs['Color'], nmap.inputs['Color'])
    nt.links.new(nmap.outputs['Normal'], bs.inputs['Normal'])
    bs.inputs['Emission Strength'].default_value = 1.
    ob.data.polygons.foreach_set('material_index', [0] * len(ob.data.polygons))
    ob.data.materials.clear()
    ob.data.materials.append(m)


def main():
    bpy.context.preferences.use_preferences_save = False
    mod = importlib.import_module(sys.argv[sys.argv.index('--') + 1])
    source = Path(bpy.data.filepath).resolve()
    if source.name != f'{mod.SLUG}.blend':
        raise RuntimeError(f'Open art/creatures/{mod.SLUG}/{mod.SLUG}.blend')
    name = mod.UNITY_NAME
    out = kit.ROOT / 'unity/Assets/Creatures' / name
    out.mkdir(parents=True, exist_ok=True)
    game_dir = source.parent / 'game'
    game_dir.mkdir(exist_ok=True)
    scene = bpy.context.scene
    arm = bpy.data.objects['Rig']
    arm.data.pose_position = 'REST'
    src = [o for o in bpy.data.objects if o.type == 'MESH' and 'bone' in o]
    low_c = bpy.data.collections.new('GAME_LOW')
    high_c = bpy.data.collections.new('GAME_HIGH')
    scene.collection.children.link(low_c)
    scene.collection.children.link(high_c)

    highs = [evaluated_copy(o, high_c, '_HI') for o in src]
    high_tris = sum(tri_count(h.data) for h in highs)
    target = getattr(mod, 'GAME_TRIS', 14000)
    ratio = min(1., target / high_tris)
    lows = []
    for o in src:
        cp = evaluated_copy(o, low_c, '_LP')
        n = tri_count(cp.data)
        decimate(cp, max(ratio, min(1., MIN_TRIS / max(n, 1))))
        lows.append(cp)
    kit.bind(arm, lows)
    for lp in lows:
        lp.modifiers.clear()
    ob = join(lows, name)
    high = join(highs, name + '_High')
    unwrap(ob)
    for c in list(scene.collection.children):
        if c not in (low_c, high_c):
            scene.collection.children.unlink(c)
    # Высокий меш скрыт из рендера, иначе AO в материалах затемнит низкий меш перекрытием копий.
    high.hide_render = True
    scene.collection.objects.link(arm)

    device = configure_cycles_device(scene)
    scene.cycles.samples = 24
    for o in bpy.context.view_layer.objects:
        o.select_set(o == ob)
    bpy.context.view_layer.objects.active = ob
    mats = [s.material for s in ob.material_slots]
    images = {k: bpy.data.images.new(f'{name}_{k}', TEX, TEX, alpha=k == 'Mask')
              for k in ('Albedo', 'Normal', 'Mask', 'Emission', 'Metal', 'Smooth')}
    # Линейные данные: без этого запекание в байтовый буфер закодирует значения в sRGB.
    for k in ('Normal', 'Mask', 'Metal', 'Smooth'):
        images[k].colorspace_settings.name = 'Non-Color'
    bake(ob, images['Albedo'], 'DIFFUSE')
    for key, img in (('emit', images['Emission']), ('metal', images['Metal']), ('rough', images['Smooth'])):
        rw = Rewire(mats, key)
        bake(ob, img, 'EMIT')
        rw.restore()
    mask = np.zeros((TEX, TEX, 4), dtype=np.float32)
    mask[..., 0] = pixels(images['Metal'])[..., 0]
    mask[..., 1] = mask[..., 0]
    mask[..., 2] = mask[..., 0]
    mask[..., 3] = pixels(images['Smooth'])[..., 0]
    images['Mask'].pixels = mask.ravel()

    high.hide_render = False
    for o in bpy.context.view_layer.objects:
        o.select_set(o in (ob, high))
    bpy.context.view_layer.objects.active = ob
    h = getattr(mod, 'HEIGHT', 2.)
    bake(ob, images['Normal'], 'NORMAL', selected=True, extrusion=h * .006, ray=h * .02)
    for k in ('Albedo', 'Normal', 'Mask', 'Emission'):
        save(images[k], out / f'{name}_{k}.png')
    for k in ('Metal', 'Smooth'):
        bpy.data.images.remove(images.pop(k))
    bpy.data.objects.remove(high)
    scene.collection.children.unlink(high_c)
    bpy.context.view_layer.update()

    single_material(ob, name, images)
    mod_arm = ob.modifiers.new('Armature', 'ARMATURE')
    mod_arm.object = arm
    ob.parent = arm
    arm.data.pose_position = 'POSE'
    arm.animation_data.action = None
    for pb in arm.pose.bones:
        pb.rotation_quaternion = (1, 0, 0, 0)
        pb.location = (0, 0, 0)
    # Unity восстанавливает оси из заголовка FBX, поэтому смена axis_forward не поворачивает модель физически.
    arm.rotation_euler.z = math.pi
    for o in bpy.context.view_layer.objects:
        o.select_set(o in (ob, arm))
    bpy.context.view_layer.objects.active = arm
    fbx = out / f'{name}.fbx'
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True, object_types={'ARMATURE', 'MESH'},
                             apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                             mesh_smooth_type='OFF', use_mesh_modifiers=False, add_leaf_bones=False,
                             use_armature_deform_only=False, bake_anim=True, bake_anim_use_all_actions=True,
                             bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True,
                             bake_anim_simplify_factor=0., path_mode='STRIP')
    settings = {'title': mod.TITLE, 'alpha': getattr(mod, 'ALPHA', 1.), 'emission': getattr(mod, 'EMISSION', 2.)}
    (out / f'{name}.creature.json').write_text(json.dumps(settings, ensure_ascii=False, indent=2) + '\n',
                                               encoding='utf-8')
    bpy.ops.wm.save_as_mainfile(filepath=str(game_dir / f'{mod.SLUG}-game.blend'))
    lo = [min(v.co[i] for v in ob.data.vertices) for i in range(3)]
    hi = [max(v.co[i] for v in ob.data.vertices) for i in range(3)]
    groups = sorted({ob.vertex_groups[g.group].name for v in ob.data.vertices for g in v.groups})
    unweighted = sum(1 for v in ob.data.vertices if not any(g.weight > 0 for g in v.groups))
    report = {'id': mod.__name__, 'unity_name': name, 'device': device['backend'], 'high_triangles': high_tris,
              'game_triangles': tri_count(ob.data), 'texture_size': TEX, 'bones': len(arm.data.bones),
              'weighted_bones': len(groups), 'unweighted_vertices': unweighted,
              'actions': sorted(a.name for a in bpy.data.actions),
              'bounds_min_m': [round(x, 3) for x in lo], 'bounds_max_m': [round(x, 3) for x in hi],
              'fbx': str(fbx.relative_to(kit.ROOT)).replace('\\', '/'),
              'fbx_bytes': fbx.stat().st_size}
    vdir = kit.ROOT / 'verification/creatures'
    vdir.mkdir(parents=True, exist_ok=True)
    (vdir / f'{mod.SLUG}-export.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n',
                                                 encoding='utf-8')
    print('CREATURE_EXPORT ' + json.dumps(report, ensure_ascii=False))


if __name__ == '__main__':
    main()

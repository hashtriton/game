"""Игровая версия Hexblade для Unity: децимация, UV, запекание процедурных материалов, экспорт FBX.

blender --background --factory-startup --disable-autoexec --offline-mode art/heroes/hexblade/hexblade.blend \
    --python-exit-code 1 --python tools/hexblade_export.py
Пишет art/heroes/hexblade/game/: hexblade-game.blend, Hexblade.fbx, Hexblade_Albedo.png, Hexblade_Emission.png.
"""
import json
import math
import sys
from pathlib import Path

import bpy

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import configure_cycles_device  # noqa: E402

OUT = ROOT / 'art/heroes/hexblade/game'
TEX = 2048
# Мелкие детали (кольца, руны, ремни) теряют форму при общей децимации: каждой оставляем минимум треугольников.
MIN_TRIS = 400
MIN_RATIO = .07


def tri_count(me):
    me.calc_loop_triangles()
    return len(me.loop_triangles)


def lowpoly_copies(hero, coll):
    dg = bpy.context.evaluated_depsgraph_get()
    out = []
    for ob in list(hero.all_objects):
        if ob.type != 'MESH':
            continue
        me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg), preserve_all_data_layers=True, depsgraph=dg)
        me.transform(ob.matrix_world)
        cp = bpy.data.objects.new(ob.name + '_LP', me)
        coll.objects.link(cp)
        tris = tri_count(me)
        ratio = max(MIN_RATIO, min(1., MIN_TRIS / max(tris, 1)))
        if ratio < 1:
            dec = cp.modifiers.new('Decimate', 'DECIMATE')
            dec.ratio = ratio
            dg2 = bpy.context.evaluated_depsgraph_get()
            low = bpy.data.meshes.new_from_object(cp.evaluated_get(dg2), preserve_all_data_layers=True,
                                                  depsgraph=dg2)
            cp.modifiers.clear()
            cp.data = low
            bpy.data.meshes.remove(me)
        out.append(cp)
    return out


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
    ob.data.uv_layers.new(name='UVMap')
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(60), island_margin=.004, scale_to_bounds=True)
    bpy.ops.object.mode_set(mode='OBJECT')


def bake(ob, image, kind):
    for slot in ob.material_slots:
        nt = slot.material.node_tree
        node = nt.nodes.get('HX bake target') or nt.nodes.new('ShaderNodeTexImage')
        node.name = 'HX bake target'
        node.image = image
        nt.nodes.active = node
    if kind == 'DIFFUSE':
        bpy.ops.object.bake(type='DIFFUSE', pass_filter={'COLOR'}, margin=8, use_clear=True)
    else:
        bpy.ops.object.bake(type=kind, margin=8, use_clear=True)
    image.filepath_raw = str(OUT / f'{image.name}.png')
    image.file_format = 'PNG'
    image.save()


def single_material(ob, albedo, emission):
    """После запекания все слоты сводятся к одному материалу: один сабмеш и один draw call в Unity."""
    m = bpy.data.materials.new('Hexblade')
    m.use_nodes = True
    nt = m.node_tree
    bs = nt.nodes['Principled BSDF']
    bs.inputs['Roughness'].default_value = .6
    for img, socket in ((albedo, 'Base Color'), (emission, 'Emission Color')):
        tex = nt.nodes.new('ShaderNodeTexImage')
        tex.image = img
        nt.links.new(tex.outputs['Color'], bs.inputs[socket])
    bs.inputs['Emission Strength'].default_value = 1.
    ob.data.polygons.foreach_set('material_index', [0] * len(ob.data.polygons))
    ob.data.materials.clear()
    ob.data.materials.append(m)


def main():
    bpy.context.preferences.use_preferences_save = False
    source = Path(bpy.data.filepath).resolve()
    if source.name != 'hexblade.blend':
        raise RuntimeError('Open art/heroes/hexblade/hexblade.blend')
    OUT.mkdir(parents=True, exist_ok=True)
    scene = bpy.context.scene
    hero = bpy.data.collections['HEXBLADE']
    game = bpy.data.collections.new('HEXBLADE_GAME')
    scene.collection.children.link(game)
    dg = bpy.context.evaluated_depsgraph_get()
    high_tris = 0
    for o in list(hero.all_objects):
        if o.type == 'MESH':
            high_tris += tri_count(o.evaluated_get(dg).to_mesh())
            o.evaluated_get(dg).to_mesh_clear()
    parts = lowpoly_copies(hero, game)
    ob = join(parts, 'Hexblade')
    ob.data.set_sharp_from_angle(angle=math.radians(40))
    unwrap(ob)
    # Исходник и студия не нужны в игровом файле и не должны попадать в запекание.
    for c in list(scene.collection.children):
        if c != game:
            scene.collection.children.unlink(c)

    configure_cycles_device(scene)
    scene.cycles.samples = 32
    scene.render.bake.use_selected_to_active = False
    for o in bpy.context.view_layer.objects:
        o.select_set(o == ob)
    bpy.context.view_layer.objects.active = ob
    albedo = bpy.data.images.new('Hexblade_Albedo', TEX, TEX)
    emission = bpy.data.images.new('Hexblade_Emission', TEX, TEX)
    bake(ob, albedo, 'DIFFUSE')
    bake(ob, emission, 'EMIT')
    single_material(ob, albedo, emission)

    fbx = OUT / 'Hexblade.fbx'
    bpy.ops.export_scene.fbx(filepath=str(fbx), use_selection=True, object_types={'MESH'},
                             apply_scale_options='FBX_SCALE_UNITS', axis_forward='-Z', axis_up='Y',
                             mesh_smooth_type='FACE', use_mesh_modifiers=False, path_mode='STRIP')
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT / 'hexblade-game.blend'))
    lo, hi = [min(v.co[i] for v in ob.data.vertices) for i in range(3)], \
             [max(v.co[i] for v in ob.data.vertices) for i in range(3)]
    report = {'source': str(source), 'high_triangles': high_tris, 'game_triangles': tri_count(ob.data),
              'materials': len(ob.material_slots), 'uv_layers': [u.name for u in ob.data.uv_layers],
              'texture_size': TEX, 'bounds_min_m': [round(x, 4) for x in lo],
              'bounds_max_m': [round(x, 4) for x in hi], 'fbx': str(fbx)}
    (ROOT / 'verification/hexblade-game-export.json').write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('HEXBLADE_EXPORT ' + json.dumps(report, ensure_ascii=False))


if __name__ == '__main__':
    main()

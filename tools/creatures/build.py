"""Собирает существо из tools/creatures/<id>.py в art/creatures/<slug>/<slug>.blend: модель, риг, клипы, студия.

Из корня game, отдельным background-процессом:
blender --background --factory-startup --disable-autoexec --offline-mode --python-exit-code 1 \
    --python tools/creatures/build.py -- <id>
"""
import importlib
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import kit  # noqa: E402


def load(cid):
    return importlib.import_module(cid)


def stats(objs):
    dg = bpy.context.evaluated_depsgraph_get()
    tris, lo, hi, bad = 0, Vector((1e9,) * 3), Vector((-1e9,) * 3), []
    for ob in objs:
        me = ob.evaluated_get(dg).to_mesh()
        me.calc_loop_triangles()
        tris += len(me.loop_triangles)
        for v in me.vertices:
            co = ob.matrix_world @ v.co
            if not all(math.isfinite(c) for c in co):
                bad.append(ob.name)
                break
            lo = Vector(map(min, lo, co))
            hi = Vector(map(max, hi, co))
        ob.evaluated_get(dg).to_mesh_clear()
    return tris, lo, hi, bad


def main():
    if not bpy.app.background:
        raise RuntimeError('Run in background Blender.')
    cid = sys.argv[sys.argv.index('--') + 1]
    mod = load(cid)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.context.preferences.use_preferences_save = False
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    scene.render.fps = kit.FPS
    root = bpy.data.collections.new(mod.SLUG.upper().replace('-', '_'))
    scene.collection.children.link(root)
    model = bpy.data.collections.new(mod.SLUG + ' model')
    root.children.link(model)
    M = mod.materials()
    mod.build(M, model)
    meshes = [o for o in model.all_objects if o.type == 'MESH']
    arm = kit.build_rig(mod.BONES, root)
    kit.bind(arm, meshes)
    clips = kit.make_clips(arm, mod.clips())
    kit.build_studio(scene, mod.HEIGHT)
    scene.frame_start, scene.frame_end = 0, mod.clips()['Idle'][0]
    for screen in bpy.data.screens:
        for area in screen.areas:
            if area.type == 'VIEW_3D':
                area.spaces.active.shading.type = 'MATERIAL'
    out = kit.ROOT / 'art/creatures' / mod.SLUG
    out.mkdir(parents=True, exist_ok=True)
    path = out / f'{mod.SLUG}.blend'
    bpy.ops.wm.save_as_mainfile(filepath=str(path))
    tris, lo, hi, bad = stats(meshes)
    unbound = [o.name for o in meshes if not o.vertex_groups]
    print('CREATURE_BUILD ' + json.dumps({
        'id': cid, 'path': str(path), 'objects': len(meshes), 'bones': len(arm.data.bones), 'clips': clips,
        'evaluated_triangles': tris, 'non_finite': bad, 'unbound': unbound,
        'bbox_min': [round(c, 3) for c in lo], 'bbox_max': [round(c, 3) for c in hi]}, ensure_ascii=False))


if __name__ == '__main__':
    main()

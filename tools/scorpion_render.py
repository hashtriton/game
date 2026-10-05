"""Рендерит ракурсы jade-scorpion.blend в Cycles, не изменяя исходник.

blender --background --factory-startup --disable-autoexec --offline-mode art/creatures/jade-scorpion/jade-scorpion.blend \
    --python-exit-code 1 --python tools/scorpion_render.py -- [clay|final] [views...]
"""
import hashlib
import json
import math
import sys
import time
from pathlib import Path

import bpy

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import configure_cycles_device, point_at  # noqa: E402

VIEWS = {
    # имя: (позиция камеры, цель, фокусное)
    'beauty': ((-2.6, -5.8, 3.0), (0, -.2, .7), 46),
    'front': ((0, -8.5, 1.4), (0, -.2, .75), 50),
    'side': ((8.5, -.2, 1.3), (0, .0, .85), 48),
    'back': ((-3.2, 6.6, 3.0), (0, .2, .8), 46),
    'game': ((4.6, -6.6, 7.8), (0, -.1, .5), 50),
    'head': ((-1.1, -4.4, 1.5), (0, -.9, .5), 55),
}


def main():
    bpy.context.preferences.use_preferences_save = False
    source = Path(bpy.data.filepath).resolve()
    if source.name != 'jade-scorpion.blend':
        raise RuntimeError('Open art/creatures/jade-scorpion/jade-scorpion.blend')
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else ['final']
    mode, selected = args[0], args[1:] or list(VIEWS)
    if mode not in {'clay', 'final'}:
        raise ValueError(mode)
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    scene = bpy.context.scene
    device = configure_cycles_device(scene)
    if mode == 'clay':
        clay = bpy.data.materials.new('Review clay')
        clay.use_nodes = True
        clay.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (.36, .37, .4, 1)
        scene.view_layers[0].material_override = clay
    scene.cycles.samples = 40 if mode == 'clay' else 160
    scene.cycles.use_denoising = True
    scene.render.resolution_x = 1100 if mode == 'clay' else 1600
    scene.render.resolution_y = 900 if mode == 'clay' else 1300
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGB'
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.look = 'AgX - Medium High Contrast'
    cam = scene.camera
    out = source.parent / 'previews'
    out.mkdir(exist_ok=True)
    records = []
    cyclorama = bpy.data.objects['Studio_Cyclorama']
    for name in selected:
        pos, target, lens = VIEWS[name]
        # Стена циклорамы разворачивается за существом относительно каждой камеры.
        cyclorama.rotation_euler.z = math.atan2(-pos[1], -pos[0]) - math.pi / 2
        cam.location = pos
        point_at(cam, target)
        cam.data.lens = lens
        scene.render.filepath = str(out / f'jade-scorpion-{mode}-{name}.png')
        start = time.monotonic()
        bpy.ops.render.render(write_still=True)
        records.append({'view': name, 'path': scene.render.filepath, 'seconds': round(time.monotonic() - start, 1)})
    if hashlib.sha256(source.read_bytes()).hexdigest() != before:
        raise RuntimeError('Source changed during rendering.')
    print('SCORPION_RENDER ' + json.dumps({'device': device, 'mode': mode, 'views': records}))


if __name__ == '__main__':
    main()

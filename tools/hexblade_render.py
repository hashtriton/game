"""Рендерит ракурсы hexblade.blend в Cycles, не изменяя исходник.

blender --background --factory-startup --disable-autoexec --offline-mode art/heroes/hexblade/hexblade.blend \
    --python-exit-code 1 --python tools/hexblade_render.py -- [clay|final] [views...]
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
    'beauty': ((2.3, -5.6, 2.15), (0, -.08, 1.26), 64),
    'front': ((0, -7.5, 1.35), (0, -.08, 1.24), 68),
    'side': ((7.5, -.2, 1.35), (0, -.08, 1.24), 68),
    'back': ((-2.4, 6.2, 2.3), (0, .05, 1.28), 62),
    'game': ((4.2, -6.2, 7.2), (0, -.05, 1.05), 62),
    'head': ((.9, -2.6, 2.15), (0, -.2, 1.94), 85),
}


def main():
    bpy.context.preferences.use_preferences_save = False
    source = Path(bpy.data.filepath).resolve()
    if source.name != 'hexblade.blend':
        raise RuntimeError('Open art/heroes/hexblade/hexblade.blend')
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
    scene.render.resolution_x = 1100 if mode == 'clay' else 1440
    scene.render.resolution_y = 1350 if mode == 'clay' else 1760
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
        # Стена циклорамы (+Y в ее локальных осях) разворачивается за героя относительно каждой камеры.
        cyclorama.rotation_euler.z = math.atan2(-pos[1], -pos[0]) - math.pi / 2
        cam.location = pos
        point_at(cam, target)
        cam.data.lens = lens
        scene.render.filepath = str(out / f'hexblade-{mode}-{name}.png')
        start = time.monotonic()
        bpy.ops.render.render(write_still=True)
        records.append({'view': name, 'path': scene.render.filepath, 'seconds': round(time.monotonic() - start, 1)})
    if hashlib.sha256(source.read_bytes()).hexdigest() != before:
        raise RuntimeError('Source changed during rendering.')
    print('HEXBLADE_RENDER ' + json.dumps({'device': device, 'mode': mode, 'views': records}))


if __name__ == '__main__':
    main()

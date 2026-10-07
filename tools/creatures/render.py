"""Рендер ракурсов и листа поз существа в Cycles, не изменяя исходный .blend.

blender --background --factory-startup --disable-autoexec --offline-mode art/creatures/<slug>/<slug>.blend \
    --python-exit-code 1 --python tools/creatures/render.py -- <id> [final|draft|poses] [views...]
final/draft: ракурсы beauty, front, side, back, game. poses: лист 4 клипа x 4 кадра, один PNG.
"""
import hashlib
import importlib
import json
import math
import sys
import time
from pathlib import Path

import bpy
import numpy as np
from mathutils import Vector

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import kit  # noqa: E402
from hero_veteran_render import configure_cycles_device, point_at  # noqa: E402

# азимут камеры (0 = спереди, со стороны -Y), возвышение, градусы
VIEWS = {'beauty': (-35, 16), 'front': (0, 6), 'side': (90, 6), 'back': (155, 18), 'game': (-30, 52)}


def bounds(objs):
    dg = bpy.context.evaluated_depsgraph_get()
    lo, hi = Vector((1e9,) * 3), Vector((-1e9,) * 3)
    for ob in objs:
        ev = ob.evaluated_get(dg)
        for c in ev.bound_box:
            p = ev.matrix_world @ Vector(c)
            lo = Vector(map(min, lo, p))
            hi = Vector(map(max, hi, p))
    return lo, hi


def frame_camera(scene, cam, objs, az, el, lens=50, margin=1.18):
    lo, hi = bounds(objs)
    center = (lo + hi) / 2
    radius = (hi - lo).length / 2
    fov = 2 * math.atan(18 / lens)  # сенсор 36 мм по ширине
    aspect = scene.render.resolution_y / scene.render.resolution_x
    dist = radius * margin / math.sin(min(fov, 2 * math.atan(math.tan(fov / 2) * aspect)) / 2)
    a, e = math.radians(az), math.radians(el)
    d = Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e)))
    cam.location = center + d * dist
    cam.data.lens = lens
    cam.data.clip_end = dist * 4
    point_at(cam, center)
    bpy.data.objects['Studio_Cyclorama'].rotation_euler.z = math.atan2(-d.y, -d.x) - math.pi / 2


def setup(scene, samples, res):
    configure_cycles_device(scene)
    scene.cycles.samples = samples
    scene.cycles.use_denoising = True
    scene.render.resolution_x, scene.render.resolution_y = res
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGB'
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.look = 'AgX - Medium High Contrast'


def model_objects():
    return [o for o in bpy.data.objects if o.type == 'MESH' and 'bone' in o]


def main():
    bpy.context.preferences.use_preferences_save = False
    args = sys.argv[sys.argv.index('--') + 1:]
    mod = importlib.import_module(args[0])
    mode = args[1] if len(args) > 1 else 'final'
    source = Path(bpy.data.filepath).resolve()
    if source.name != f'{mod.SLUG}.blend':
        raise RuntimeError(f'Open art/creatures/{mod.SLUG}/{mod.SLUG}.blend')
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    scene = bpy.context.scene
    cam = scene.camera
    arm = bpy.data.objects['Rig']
    out = source.parent / 'previews'
    out.mkdir(exist_ok=True)
    objs = model_objects()
    records = []
    if mode in ('final', 'draft'):
        setup(scene, 128 if mode == 'final' else 32, (1400, 1200) if mode == 'final' else (900, 760))
        arm.data.pose_position = 'REST'
        for name in args[2:] or list(VIEWS):
            frame_camera(scene, cam, objs, *VIEWS[name])
            scene.render.filepath = str(out / f'{mod.SLUG}-{mode}-{name}.png')
            t = time.monotonic()
            bpy.ops.render.render(write_still=True)
            records.append({'view': name, 'path': scene.render.filepath, 'seconds': round(time.monotonic() - t, 1)})
    elif mode == 'poses':
        setup(scene, 24, (480, 400))
        arm.data.pose_position = 'POSE'
        clips = mod.clips()
        rows = []
        tmp = out / '_pose.png'
        # Камера одна на весь лист по позе покоя, чтобы движение не маскировалось перекадрированием.
        arm.data.pose_position = 'REST'
        frame_camera(scene, cam, objs, -40, 14, margin=1.45)
        arm.data.pose_position = 'POSE'
        for clip in ('Idle', 'Run', 'Attack', 'Death'):
            length = clips[clip][0]
            arm.animation_data.action = bpy.data.actions[clip]
            row = []
            for k in range(4):
                f = round(length * (k / 3 if clip in ('Attack', 'Death') else k / 4))
                scene.frame_set(f)
                scene.render.filepath = str(tmp)
                bpy.ops.render.render(write_still=True)
                img = bpy.data.images.load(str(tmp))
                px = np.array(img.pixels[:], dtype=np.float32).reshape(img.size[1], img.size[0], 4)
                bpy.data.images.remove(img)
                row.append(px)
            rows.append(np.concatenate(row, axis=1))
        sheet = np.concatenate(rows[::-1], axis=0)  # пиксели Blender снизу вверх: Idle сверху
        h, w = sheet.shape[:2]
        img = bpy.data.images.new('pose_sheet', w, h)
        img.pixels = sheet.ravel()
        img.filepath_raw = str(out / f'{mod.SLUG}-poses.png')
        img.file_format = 'PNG'
        img.save()
        tmp.unlink()
        records.append({'view': 'poses', 'path': img.filepath_raw})
    else:
        raise ValueError(mode)
    if hashlib.sha256(source.read_bytes()).hexdigest() != before:
        raise RuntimeError('Source changed during rendering.')
    print('CREATURE_RENDER ' + json.dumps({'id': args[0], 'mode': mode, 'views': records}, ensure_ascii=False))


if __name__ == '__main__':
    main()

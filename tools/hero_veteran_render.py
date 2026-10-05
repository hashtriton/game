"""Render the saved Veteran hero with a temporary Cycles studio.

Example, using an isolated background Blender process:
  blender --background --factory-startup --disable-autoexec --offline-mode \
    art/heroes/veteran/veteran.blend --python-exit-code 1 \
    --python tools/hero_veteran_render.py -- draft

Modes: draft, front, back, game, head, beauty, all.
The all mode renders front, back, game, head and beauty; draft is a quick
lower-quality beauty preview. Only PNGs under art/heroes/veteran/previews
are written. The loaded .blend and user preferences are never saved.
"""
import argparse
import hashlib
import json
from pathlib import Path
import sys
import time

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / 'art' / 'heroes' / 'veteran' / 'previews'
VIEWS = {
    'draft': {'camera': (10, -18, 10), 'target': (0, 0, 3.65),
              'scale': 9.7, 'resolution': (800, 960), 'samples': 24},
    'beauty': {'camera': (10, -18, 10), 'target': (0, 0, 3.65),
               'scale': 9.7, 'resolution': (1400, 1600), 'samples': 80},
    'front': {'camera': (0, -22, 4.3), 'target': (0, 0, 3.65),
              'scale': 9.1, 'resolution': (1400, 1600), 'samples': 80},
    'back': {'camera': (-9, 18, 9), 'target': (0, 0, 3.65),
             'scale': 9.7, 'resolution': (1400, 1600), 'samples': 80},
    'game': {'camera': (11, -15, 16), 'target': (0, 0, 3.1),
             'scale': 9.5, 'resolution': (1200, 1200), 'samples': 64},
    'head': {'camera': (3.5, -8, 7.0), 'target': (0, -.15, 6.65),
             'scale': 2.45, 'resolution': (1200, 1400), 'samples': 96},
}


def configure_cycles_device(scene):
    """Choose OptiX, then CUDA, then CPU, only in this Blender process."""
    bpy.context.preferences.use_preferences_save = False
    scene.render.engine = 'CYCLES'
    preferences = bpy.context.preferences.addons['cycles'].preferences
    available_backends = {item[0] for item in preferences.get_device_types(bpy.context)}
    attempts = []
    for backend in ('OPTIX', 'CUDA'):
        if backend not in available_backends:
            continue
        try:
            preferences.compute_device_type = backend
            # Query only this backend; do not initialize unrelated HIP/oneAPI devices.
            preferences.get_devices_for_type(backend)
            gpu_devices = [device for device in preferences.devices if device.type == backend]
            if not gpu_devices:
                attempts.append({'backend': backend, 'result': 'No matching GPU devices'})
                continue
            for device in preferences.devices:
                device.use = device.type == backend
            scene.cycles.device = 'GPU'
            return {'backend': backend, 'device': 'GPU',
                    'devices': [device.name for device in gpu_devices], 'fallbacks': attempts}
        except (RuntimeError, TypeError, ValueError) as exc:
            attempts.append({'backend': backend, 'result': str(exc)})
    preferences.compute_device_type = 'NONE'
    scene.cycles.device = 'CPU'
    return {'backend': 'CPU', 'device': 'CPU', 'devices': [], 'fallbacks': attempts}


def point_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat('-Z', 'Y').to_euler()


def add_area(collection, name, location, target, power, color, size):
    data = bpy.data.lights.new(name, 'AREA')
    data.energy = power
    data.color = color
    data.shape = 'DISK'
    data.size = size
    obj = bpy.data.objects.new(name, data)
    collection.objects.link(obj)
    obj.location = location
    point_at(obj, target)
    return obj


def create_studio(hero_collection):
    """Link the existing hero to a new in-memory scene; leave source scenes alone."""
    scene = bpy.data.scenes.new('Veteran | Temporary preview')
    scene.collection.children.link(hero_collection)
    studio = bpy.data.collections.new('Veteran | Temporary studio')
    scene.collection.children.link(studio)
    bpy.context.window.scene = scene

    floor_mesh = bpy.data.meshes.new('Preview | Floor mesh')
    floor_mesh.from_pydata([(-2000, -2000, .04), (2000, -2000, .04),
                           (2000, 2000, .04), (-2000, 2000, .04)], [], [(0, 1, 2, 3)])
    floor_mesh.update()
    floor = bpy.data.objects.new('Preview | Plain floor', floor_mesh)
    studio.objects.link(floor)
    floor_material = bpy.data.materials.new('Preview | Charcoal floor')
    floor_material.diffuse_color = (.037, .043, .050, 1)
    floor_material.use_nodes = True
    bsdf = floor_material.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (.037, .043, .050, 1)
    bsdf.inputs['Roughness'].default_value = .76
    floor_mesh.materials.append(floor_material)

    world = bpy.data.worlds.new('Preview | Charcoal world')
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (.055, .065, .078, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = .30
    scene.world = world

    add_area(studio, 'Preview | Neutral key', (-7, -10, 12), (0, 0, 4.3),
             3800, (1.0, .97, .93), 6.0)
    add_area(studio, 'Preview | Cool fill', (8, -5, 7), (0, 0, 4.2),
             1400, (.76, .83, 1.0), 6.0)
    add_area(studio, 'Preview | Warm rim', (2, 6, 10), (0, 0, 4.5),
             3200, (1.0, .79, .56), 5.0)
    add_area(studio, 'Preview | Face bounce', (.5, -7, 7), (0, -.2, 6.5),
             350, (.91, .95, 1.0), 4.0)

    camera_data = bpy.data.cameras.new('Preview | Camera')
    camera_data.type = 'ORTHO'
    camera_data.clip_start = .05
    camera_data.clip_end = 500
    camera_data.dof.use_dof = False
    camera = bpy.data.objects.new('Preview | Camera', camera_data)
    studio.objects.link(camera)
    scene.camera = camera

    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGB'
    scene.render.image_settings.color_depth = '8'
    scene.render.resolution_percentage = 100
    scene.render.film_transparent = False
    scene.render.use_file_extension = True
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.exposure = 0
    scene.view_settings.gamma = 1
    scene.render.engine = 'CYCLES'
    scene.cycles.use_denoising = True
    scene.cycles.use_adaptive_sampling = True
    scene.cycles.max_bounces = 8
    scene.cycles.diffuse_bounces = 4
    scene.cycles.glossy_bounces = 4
    scene.cycles.transparent_max_bounces = 8
    return scene


def apply_view(scene, mode):
    view = VIEWS[mode]
    scene.camera.location = view['camera']
    point_at(scene.camera, view['target'])
    scene.camera.data.ortho_scale = view['scale']
    scene.render.resolution_x, scene.render.resolution_y = view['resolution']
    scene.cycles.samples = view['samples']
    scene.cycles.adaptive_threshold = .045 if mode == 'draft' else .015
    scene.render.filepath = str(OUTPUT / f'veteran-{mode}.png')
    bpy.context.view_layer.update()


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('mode', choices=[*VIEWS, 'all'], nargs='?', default='draft')
    options = parser.parse_args(args)
    if not bpy.app.background:
        raise RuntimeError('Use a separate background Blender process; this script does not run in the GUI')
    source_path = Path(bpy.data.filepath)
    if not bpy.data.filepath or not source_path.is_file():
        raise RuntimeError('Load the saved veteran.blend through Blender CLI before this script')
    hero = bpy.data.collections.get('HERO')
    if hero is None or not any(obj.type == 'MESH' for obj in hero.all_objects):
        raise RuntimeError('The loaded .blend must contain a HERO collection with meshes')
    source_hash = hashlib.sha256(source_path.read_bytes()).hexdigest()
    scene = create_studio(hero)
    device_info = configure_cycles_device(scene)
    print('VETERAN_RENDER_DEVICE ' + json.dumps(device_info), flush=True)
    OUTPUT.mkdir(parents=True, exist_ok=True)
    modes = ['front', 'back', 'game', 'head', 'beauty'] if options.mode == 'all' else [options.mode]
    for mode in modes:
        apply_view(scene, mode)
        started = time.perf_counter()
        bpy.ops.render.render(write_still=True, scene=scene.name)
        output = Path(scene.render.filepath)
        if not output.is_file() or output.stat().st_size == 0:
            raise RuntimeError(f'Render output missing or empty: {output}')
        if hashlib.sha256(source_path.read_bytes()).hexdigest() != source_hash:
            raise RuntimeError('The source .blend changed during rendering')
        print('VETERAN_RENDER_OK ' + json.dumps({
            'mode': mode, 'output': str(output), 'bytes': output.stat().st_size,
            'seconds': round(time.perf_counter() - started, 3),
            'resolution': list(VIEWS[mode]['resolution']), 'samples': VIEWS[mode]['samples'],
            'device': device_info, 'source_sha256': source_hash, 'source_unchanged': True,
        }), flush=True)


if __name__ == '__main__':
    main()

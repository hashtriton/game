"""Common neutral review studio for the two independent anvil trials.

Load a saved blockout/final .blend in an isolated background Blender process.
After -- pass clay, final, present, or prepare. Rendering never overwrites the source.
Present additionally saves a separate presentation.blend for interactive viewing.
Prepare saves that studio without repeating an already completed render.
"""
import hashlib
import json
from pathlib import Path
import sys
import time

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
from hero_veteran_render import configure_cycles_device, point_at, add_area


def material(name, color, roughness, metallic=0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1)
    mat.use_nodes = True
    node = mat.node_tree.nodes.get('Principled BSDF')
    node.inputs['Base Color'].default_value = (*color, 1)
    node.inputs['Roughness'].default_value = roughness
    node.inputs['Metallic'].default_value = metallic
    return mat


def main():
    if not bpy.app.background:
        raise RuntimeError('Use an isolated background Blender process.')
    bpy.context.preferences.use_preferences_save = False
    source = Path(bpy.data.filepath).resolve()
    trial = (ROOT / 'art/tests/anvil-low').resolve()
    if source.parent.parent != trial or source.parent.name not in {'astra-low', 'sol-low'}:
        raise RuntimeError('Load one of the saved anvil trial files.')
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else ['clay']
    mode = args[0]
    if mode not in {'clay', 'final', 'present', 'prepare'}:
        raise ValueError(mode)
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    asset = bpy.data.collections.get('ASSET')
    if not asset:
        raise RuntimeError('Missing ASSET collection.')
    output = source.parent / 'previews'
    output.mkdir(parents=True, exist_ok=True)
    scene = bpy.context.scene
    scene.name = 'Anvil | Review studio'
    # Keep source collections because live booleans can depend on hidden cutters.
    # Asset makers are instructed to save geometry only, without their own studio.
    studio = bpy.data.collections.new('REVIEW_STUDIO')
    scene.collection.children.link(studio)
    for obj in list(scene.objects):
        if obj.type in {'CAMERA', 'LIGHT'}:
            obj.hide_render = True
    floor_mesh = bpy.data.meshes.new('Review floor')
    floor_mesh.from_pydata([(-200, -200, -.012), (200, -200, -.012),
                            (200, 200, -.012), (-200, 200, -.012)], [], [(0, 1, 2, 3)])
    floor_mesh.update()
    floor = bpy.data.objects.new('Review floor', floor_mesh)
    studio.objects.link(floor)
    floor_mesh.materials.append(material('Review charcoal', (.033, .042, .054), .8))
    world = bpy.data.worlds.new('Review neutral world')
    world.use_nodes = True
    world.node_tree.nodes['Background'].inputs['Color'].default_value = (.18, .20, .24, 1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value = .35
    scene.world = world
    add_area(studio, 'Review key', (-3, -4, 5), (0, 0, .5), 520, (1, .96, .91), 3)
    add_area(studio, 'Review fill', (3, -2, 2.8), (0, 0, .5), 240, (.83, .9, 1), 2.5)
    add_area(studio, 'Review rim', (1, 3, 4), (0, 0, .6), 650, (1, .90, .76), 2.8)
    data = bpy.data.cameras.new('Review camera')
    data.type = 'ORTHO'
    data.clip_start = .01
    data.clip_end = 500
    camera = bpy.data.objects.new('Review camera', data)
    studio.objects.link(camera)
    scene.camera = camera
    clay = mode == 'clay'
    if clay:
        scene.view_layers[0].material_override = material('Review clay', (.30, .33, .36), .4, .12)
    device = configure_cycles_device(scene)
    scene.cycles.samples = 24 if clay else 64
    scene.cycles.use_denoising = True
    scene.render.resolution_percentage = 100
    scene.render.resolution_x = scene.render.resolution_y = 720 if clay else 1200
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGB'
    scene.render.film_transparent = False
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.look = 'AgX - Medium High Contrast'
    scene.view_settings.exposure = 0
    scene.view_settings.gamma = 1
    views = {
        'beauty': ((-2.7, -4, 2.7), (0, 0, .45), 1.58),
        'front': ((0, -5, .5), (0, 0, .5), 1.38),
        'side': ((-5, 0, .5), (0, 0, .5), 1.38),
        'top': ((0, 0, 5), (0, 0, .45), 1.38),
    }
    if not clay:
        views['detail'] = ((1.7, -2.5, 2.6), (.10, 0, .77), 1.10)
    records = []
    render_views = {} if mode == 'prepare' else views
    for name, (position, target, scale) in render_views.items():
        camera.location = position
        point_at(camera, target)
        data.ortho_scale = scale
        destination = output / f'{source.stem}-{name}.png'
        scene.render.filepath = str(destination)
        started = time.monotonic()
        bpy.ops.render.render(write_still=True)
        records.append({'view': name, 'path': str(destination),
                        'seconds': round(time.monotonic() - started, 3)})
    if mode in {'present', 'prepare'}:
        camera.location, target, data.ortho_scale = views['beauty']
        point_at(camera, target)
        for obj in bpy.context.selected_objects:
            obj.select_set(False)
        for screen in bpy.data.screens:
            for area in screen.areas:
                if area.type == 'VIEW_3D':
                    area.spaces.active.shading.type = 'MATERIAL'
                    area.spaces.active.region_3d.view_perspective = 'CAMERA'
        scene['trial_source'] = str(source.relative_to(ROOT))
        scene['trial_source_sha256'] = before
        bpy.ops.wm.save_as_mainfile(filepath=str(source.parent / 'presentation.blend'))
    after = hashlib.sha256(source.read_bytes()).hexdigest()
    if before != after:
        raise RuntimeError('The rendering operation changed its source file.')
    report = {'source': str(source), 'sha256': before, 'source_unchanged': True,
              'mode': mode, 'device': device, 'views': records}
    report_name = f'{source.stem}-prepare.json' if mode == 'prepare' else f'{source.stem}-render.json'
    (output / report_name).write_text(
        json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print('ANVIL_RENDER ' + json.dumps(report))


if __name__ == '__main__':
    main()

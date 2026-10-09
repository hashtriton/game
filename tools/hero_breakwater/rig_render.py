"""Render five samples and a close view per baked clip without saving the scene."""
import argparse
import math
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'art/heroes/breakwater/rig'
parser = argparse.ArgumentParser()
parser.add_argument('--input', type=Path, default=OUT / 'Breakwater_P3_rig.blend')
parser.add_argument('--frames', type=Path, default=OUT / 'frames')
opt = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])
opt.frames = opt.frames.resolve()
bpy.ops.wm.open_mainfile(filepath=str(opt.input.resolve()))
scene = bpy.context.scene
scene.render.engine = 'BLENDER_WORKBENCH'
scene.display.shading.light = 'STUDIO'
scene.display.shading.color_type = 'MATERIAL'
scene.display.shading.show_shadows = True
scene.display.shading.show_cavity = True
scene.display.shading.cavity_type = 'BOTH'
scene.display.shading.show_specular_highlight = True
scene.display.shading.background_type = 'WORLD'
scene.world.color = (.22, .24, .27)
scene.render.film_transparent = True
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.render.resolution_percentage = 100
data = bpy.data.cameras.new('ProbeCamera')
camera = bpy.data.objects.new('ProbeCamera', data)
scene.collection.objects.link(camera)
scene.camera = camera
data.sensor_fit = 'VERTICAL'
data.angle_y = math.radians(45)
data.clip_start = .01
rig = bpy.data.objects['CharacterArmature']
for name, last in [('Idle', 76), ('Run', 22), ('Attack', 21), ('Death', 29)]:
    action = bpy.data.actions[name]
    rig.animation_data.action = action
    rig.animation_data.action_slot = action.slots[0]
    for index, t in enumerate([0, .25, .5, .75, 1]):
        frame = 1 + (last - 1) * t
        scene.frame_set(int(frame), subframe=frame % 1)
        camera.location = (0, -19 * math.cos(math.radians(56)), 19 * math.sin(math.radians(56)))
        camera.rotation_euler = (Vector((0, 0, 0)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
        data.type = 'PERSP'
        scene.render.resolution_x, scene.render.resolution_y = 1920, 1080
        scene.render.filepath = str(opt.frames / f'{name}-{index}.png')
        bpy.ops.render.render(write_still=True)
    scene.frame_set(1 + round((last - 1) * (.75 if name == 'Death' else .5)))
    camera.location = (-4.2, -6.4, 3.15)
    camera.rotation_euler = (Vector((0, 0, 1.2)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
    data.type = 'ORTHO'
    data.ortho_scale = 3.9
    scene.render.resolution_x, scene.render.resolution_y = 700, 700
    scene.render.filepath = str(opt.frames / f'{name}-close.png')
    bpy.ops.render.render(write_still=True)
print('Rendered 24 frames')

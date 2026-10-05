"""Set up a useful camera, studio, and opening view in our saved character file."""
from pathlib import Path
import sys
import bpy
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tools'))
from hero_veteran_render import create_studio,apply_view,configure_cycles_device
if not bpy.app.background:
    raise RuntimeError('Prepare only the separately loaded background file.')
output_path=Path(bpy.data.filepath).resolve()
if output_path.parent!= (ROOT/'art/heroes/veteran').resolve() or output_path.suffix!='.blend':
    raise RuntimeError('Unexpected source file.')
source_scenes=list(bpy.data.scenes)
scene=create_studio(bpy.data.collections['HERO'])
scene.name='Veteran | Character studio'
apply_view(scene,'beauty')
configure_cycles_device(scene)
scene.unit_settings.system='METRIC'
scene.unit_settings.scale_length=.3
for key in source_scenes[0].keys():
    scene[key]=source_scenes[0][key]
for old in source_scenes:
    bpy.data.scenes.remove(old)
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            space=area.spaces.active
            space.overlay.show_overlays=False
            space.shading.type='MATERIAL'
            space.shading.use_scene_lights=False
            space.shading.use_scene_world=False
            space.region_3d.view_location=Vector((0,0,3.55))
            space.region_3d.view_rotation=scene.camera.rotation_euler.to_quaternion()
            space.region_3d.view_distance=11
            space.region_3d.view_perspective='ORTHO'
            space.clip_end=1000
bpy.ops.object.select_all(action='DESELECT')
bpy.ops.wm.save_as_mainfile(filepath=str(output_path))
print('VETERAN_VIEW_SAVED')

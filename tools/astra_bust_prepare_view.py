"""Save the bust with a useful initial material viewport and beauty camera."""
import bpy
from mathutils import Vector
from pathlib import Path
if not bpy.app.background:raise RuntimeError('Background only')
ROOT=Path(__file__).resolve().parents[1]
expected=ROOT/'art/heroes/astra-bust-trial/astra-bust.blend'
if Path(bpy.data.filepath).resolve()!=expected.resolve():raise RuntimeError('Wrong source blend')
bpy.context.preferences.use_preferences_save=False
bpy.context.preferences.filepaths.save_version=0
sc=bpy.context.scene;cam=sc.camera;cam.location=(5.5,-15,8)
cam.rotation_euler=(Vector((0,0,4.5))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=8.35
sc.render.resolution_x=1400;sc.render.resolution_y=1600;sc.cycles.samples=80
sc.render.filepath=str(expected.parent/'previews/beauty.png')
for screen in bpy.data.screens:
    for a in screen.areas:
        if a.type=='VIEW_3D':
            sp=a.spaces.active;sp.region_3d.view_distance=11;sp.region_3d.view_location=Vector((0,0,4.5));sp.region_3d.view_rotation=cam.rotation_euler.to_quaternion();sp.region_3d.view_perspective='ORTHO';sp.shading.type='MATERIAL';sp.overlay.show_overlays=False
bpy.ops.wm.save_as_mainfile(filepath=str(expected))
print('BEAUTY_VIEW_SAVED')

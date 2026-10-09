"""Choose sword carry using measured occlusion across eight camera facings."""
import json
import math
import sys
from pathlib import Path
sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
import numpy as np
from mathutils import Euler,Vector
from p4_build import pose,align_hands
from p4_render import setup
from render_views import point_at
from validate_p3 import measure

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'.local/codex-tasks/hero-b/sword-probe-3'
OUT.mkdir(parents=True,exist_ok=True)
pose('display')
scene=bpy.context.scene
floor,camera,device,ray=setup(scene)
floor.hide_render=True
scene.render.engine='BLENDER_WORKBENCH'
scene.render.film_transparent=True
scene.display.shading.light='FLAT'
scene.display.shading.color_type='OBJECT'
scene.display.shading.show_shadows=False
scene.display.shading.show_cavity=False
scene.display.shading.show_specular_highlight=False
scene.view_settings.view_transform='Standard'
scene.view_settings.exposure=0
scene.render.resolution_x,scene.render.resolution_y=960,540
camera.data.type='PERSP'
camera.data.sensor_fit='VERTICAL'
camera.data.angle_y=math.radians(45)
camera.location=(0,-19*math.cos(math.radians(56)),19*math.sin(math.radians(56)))
point_at(camera,(0,0,0))
for o in scene.objects:
    o.color=(1,0,0,1) if o.name.startswith('HB_Sword') else (0,0,0,1)
root=bpy.data.objects['HB_Root']
sword=bpy.data.objects['HB_Sword']
results=[]
for armout in [13,17]:
    for forebend in [12,20,28]:
        x,y=-20,28
        root.rotation_euler.z=0
        pose('display')
        arm=bpy.data.objects['HB_UpperArm_R']
        arm.rotation_euler.y=math.radians(armout)
        fore=bpy.data.objects['HB_Forearm_R']
        fore.rotation_euler.x=math.radians(-forebend)
        bpy.context.view_layer.update()
        desired=Euler(tuple(math.radians(a) for a in [x,y,-7]),'XYZ').to_quaternion()
        sword.rotation_euler=(sword.parent.matrix_world.to_quaternion().inverted()@desired).to_euler()
        align_hands()
        deps=bpy.context.evaluated_depsgraph_get()
        points=[o.evaluated_get(deps).matrix_world@v.co for o in scene.objects if o.type=='MESH' and o.name.startswith('HB_') for v in o.evaluated_get(deps).data.vertices]
        width=max(p.x for p in points)-min(p.x for p in points)
        row={'euler':[x,y,-7],'arm_out':armout,'fore_bend':forebend,'equipped_width':width,'visible_sword_pixels_half_res':{}}
        for yaw in range(0,360,45):
            root.rotation_euler.z=math.radians(yaw)
            bpy.context.view_layer.update()
            path=OUT/'probe.png'
            scene.render.filepath=str(path)
            bpy.ops.render.render(write_still=True)
            image=bpy.data.images.load(str(path),check_existing=False)
            pixels=np.array(image.pixels[:]).reshape(-1,4)
            count=int(np.sum((pixels[:,0]>.3)&(pixels[:,1]<.05)&(pixels[:,3]>.5)))
            bpy.data.images.remove(image)
            row['visible_sword_pixels_half_res'][str(yaw)]=count
        row['minimum']=min(row['visible_sword_pixels_half_res'].values())
        results.append(row)
        print('CANDIDATE',row,flush=True)
(OUT/'scores-arms.json').write_text(json.dumps(results,indent=2)+'\n',encoding='utf-8')

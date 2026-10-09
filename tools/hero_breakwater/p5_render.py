"""Render sequential daylight views and flat material/occlusion masks."""
import argparse
import json
import math
import sys
from pathlib import Path
sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
from p4_render import setup
from render_views import point_at

PALETTE={'HB_WhitePaintedSteel':(1,0,0,1),'HB_NavyClothLeather':(0,1,0,1),'HB_BrushedSteel':(0,0,1,1),'HB_DarkJointSteel':(1,1,0,1),'HB_CyanAccent':(0,1,1,1),'HB_RestrainedBronze':(1,0,1,1)}
if __name__=='__main__':
    p=argparse.ArgumentParser(); p.add_argument('--output',type=Path,required=True); p.add_argument('--views',nargs='+',default=['three-quarter','back-three-quarter','side']+[f'game-{d:03}' for d in range(0,360,45)]); p.add_argument('--ids-only',action='store_true'); p.add_argument('--rig',action='store_true'); p.add_argument('--phase',type=float,default=0); a=p.parse_args(sys.argv[sys.argv.index('--')+1:]); out=a.output.resolve(); out.mkdir(parents=True,exist_ok=True)
    scene=bpy.context.scene
    if not a.rig:
        from p4_build import pose
        pose('display')
    else:
        rig=bpy.data.objects['CharacterArmature']; action=bpy.data.actions['Idle']; rig.animation_data.action=action; rig.animation_data.action_slot=action.slots[0]; frame=1+(action.frame_range[1]-1)*a.phase; scene.frame_set(int(frame),subframe=frame%1)
    floor,camera,device,ray=setup(scene); scene.cycles.samples=32
    data=camera.data; transform=scene.view_settings.view_transform; look=scene.view_settings.look
    root=bpy.data.objects.get('HB_Root') or bpy.data.objects['CharacterArmature']
    hero=[o for o in scene.objects if o.type=='MESH' and o.name.startswith('HB_')]
    colors={m.name:tuple(m.diffuse_color) for m in bpy.data.materials if m.name in PALETTE}
    for name in a.views:
        root.rotation_euler.z=0
        if name.startswith('game-'):
            root.rotation_euler.z=math.radians(float(name.split('-')[1])); data.type='PERSP'; data.sensor_fit='VERTICAL'; data.angle_y=math.radians(45); camera.location=(0,-19*math.cos(math.radians(56)),19*math.sin(math.radians(56))); point_at(camera,(0,0,0)); scene.render.resolution_x,scene.render.resolution_y=1920,1080
        else:
            data.type='ORTHO'; data.ortho_scale=3.10; scene.render.resolution_x=scene.render.resolution_y=960
            camera.location={'three-quarter':(-4.2,-6.4,3.15),'back-three-quarter':(4.2,6.4,3.15),'side':(-8,0,1.20)}[name]; point_at(camera,(0,-.08,1.20))
        bpy.context.view_layer.update()
        if not a.ids_only:
            for n,c in colors.items(): bpy.data.materials[n].diffuse_color=c
            floor.hide_render=False; scene.render.engine='CYCLES'; scene.render.film_transparent=False; scene.view_settings.view_transform=transform; scene.view_settings.look=look
            scene.render.filepath=str(out/(name+'.png')); bpy.ops.render.render(write_still=True)
        floor.hide_render=True; scene.render.engine='BLENDER_WORKBENCH'; scene.render.film_transparent=True; scene.display.shading.light='FLAT'; scene.display.shading.color_type='MATERIAL'; scene.display.shading.show_shadows=False; scene.display.shading.show_cavity=False; scene.display.shading.show_specular_highlight=False; scene.view_settings.view_transform='Standard'; scene.view_settings.look='None'; scene.view_settings.exposure=0
        for n,c in PALETTE.items(): bpy.data.materials[n].diffuse_color=c
        scene.render.filepath=str(out/(name+'-id.png')); bpy.ops.render.render(write_still=True)
        scene.display.shading.color_type='SINGLE'; scene.display.shading.single_color=(0,0,0)
        scene.render.filepath=str(out/(name+'-mask.png')); bpy.ops.render.render(write_still=True)
        if name.startswith('game-'):
            scene.display.shading.color_type='OBJECT'
            for o in hero: o.color=(0,1,0,1) if o.name.endswith('_L') and o.name.startswith(('HB_Thigh','HB_Cuisse','HB_Knee','HB_Poleyn','HB_Shin','HB_Greave','HB_Boot','HB_Sabaton')) else (1,0,0,1) if o.name.startswith(('HB_Shield','HB_Rim')) else (0,0,0,1)
            for hidden,suffix in [(False,'leg-visible'),(True,'leg-no-shield')]:
                for o in hero:
                    if o.name.startswith(('HB_Shield','HB_Rim')): o.hide_render=hidden
                scene.render.filepath=str(out/(name+'-'+suffix+'.png')); bpy.ops.render.render(write_still=True)
            for o in hero: o.hide_render=False
        scene.view_settings.exposure=.7
        print('P5 VIEW',name,flush=True)
    (out/'render-facts.json').write_text(json.dumps(dict(source=bpy.data.filepath,device=device,samples=32,pose='Idle' if a.rig else 'display',phase=a.phase,camera=dict(pitch=56,fov=45,distance=19)),indent=2),encoding='utf-8')

"""Sequential daylight render using the untouched hero P4 lighting helper."""
import argparse
import json
import math
import sys
import time
from pathlib import Path
sys.dont_write_bytecode=True
import bpy
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'tools/hero_breakwater'))
from p4_render import setup
from render_views import point_at

def main():
    p=argparse.ArgumentParser()
    p.add_argument('--output',type=Path,required=True)
    p.add_argument('--views',nargs='+',default=['three-quarter','back','side','clay']+[f'game-{v:03}' for v in range(0,360,45)])
    p.add_argument('--size',type=int,default=1280)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    a.output=a.output.resolve()
    if (ROOT/'art/creatures/cinder-runner').resolve() not in a.output.parents and (ROOT/'.local/codex-tasks/creeps').resolve() not in a.output.parents:
        raise ValueError('R1 or task output only')
    a.output.mkdir(parents=True,exist_ok=True)
    scene=bpy.context.scene
    floor,camera,device,ray=setup(scene)
    root=bpy.data.objects['R1_Root']
    originals={o.name:list(o.data.materials) for o in scene.objects if o.type=='MESH' and o.name.startswith('R1_')}
    clay=bpy.data.materials.new('Control_Clay')
    clay.use_nodes=True
    clay.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.4,.43,.46,1)
    clay.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.54
    records=[]
    for name in a.views:
        root.rotation_euler.z=0
        for key,mats in originals.items():
            o=bpy.data.objects[key]
            o.data.materials.clear()
            for mat in ([clay] if name=='clay' else mats):
                o.data.materials.append(mat)
        if name.startswith('game-'):
            root.rotation_euler.z=math.radians(int(name.split('-')[1]))
            scene.render.resolution_x,scene.render.resolution_y=1920,1080
            camera.data.type='PERSP'
            camera.data.sensor_fit='VERTICAL'
            camera.data.angle_y=math.radians(45)
            camera.location=(0,-19*math.cos(math.radians(56)),19*math.sin(math.radians(56)))
            point_at(camera,(0,0,0))
        else:
            scene.render.resolution_x=scene.render.resolution_y=a.size
            camera.data.type='ORTHO'
            camera.data.ortho_scale=2.75
            coords={'three-quarter':(-4.2,-6.4,3.15),'clay':(-4.2,-6.4,3.15),'back':(4.2,6.4,3.15),'side':(-8,0,1.2)}
            camera.location=coords[name]
            point_at(camera,(0,-.12,1.06))
        floor.hide_render=False
        scene.render.engine='CYCLES'
        scene.view_settings.view_transform='ACES 2.0'
        scene.view_settings.exposure=.7
        scene.render.film_transparent=False
        scene.render.filepath=str(a.output/(name+'.png'))
        started=time.monotonic()
        print('START',name,flush=True)
        bpy.ops.render.render(write_still=True)
        floor.hide_render=True
        scene.render.engine='BLENDER_WORKBENCH'
        scene.render.film_transparent=True
        scene.display.shading.light='FLAT'
        scene.display.shading.color_type='SINGLE'
        scene.display.shading.single_color=(0,0,0)
        scene.display.shading.show_shadows=False
        scene.display.shading.show_cavity=False
        scene.display.shading.show_specular_highlight=False
        scene.view_settings.view_transform='Standard'
        scene.view_settings.exposure=0
        scene.render.filepath=str(a.output/(name+'-mask.png'))
        bpy.ops.render.render(write_still=True)
        if name.startswith('game-'):
            scene.display.shading.color_type='OBJECT'
            for key in originals:
                bpy.data.objects[key].color=(1,0,0,1) if key.startswith('R1_Cleaver') else (0,0,0,1)
            scene.render.filepath=str(a.output/(name+'-weapon-id.png'))
            bpy.ops.render.render(write_still=True)
        records.append({'view':name,'seconds':round(time.monotonic()-started,3)})
        (a.output/'render-log.json').write_text(json.dumps({'source':bpy.data.filepath,'revision':scene['revision'],'device':device,'samples':48,'denoising':True,'view_transform':'ACES 2.0','sun_energy':2.9,'sky_strength':.48,'sun_ray':ray,'camera':{'pitch':56,'vertical_fov':45,'distance':19,'resolution':[1920,1080]},'views':records},indent=2)+'\n',encoding='utf-8')
    print('R1 RENDERED',len(records),flush=True)

if __name__=='__main__':
    main()

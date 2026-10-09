"""Sequential daylight render study using the read-only hero P4 lighting."""
import argparse
import hashlib
import json
import math
import sys
import time
from pathlib import Path

sys.dont_write_bytecode=True
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).resolve().parent))
sys.path.insert(0,str(ROOT/'tools/hero_breakwater'))
import bpy
from A_common import pose
from p4_render import setup
from render_views import point_at


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--output',type=Path,required=True)
    p.add_argument('--views',nargs='+',default=['three-quarter','back-three-quarter','side','clay']+[f'game-{d:03}' for d in range(0,360,45)])
    p.add_argument('--size',type=int,default=1280)
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    scene=bpy.context.scene
    prefix=scene['prefix']
    creep=scene['creep_id']
    out=a.output.resolve()
    if (ROOT/'art/creatures'/creep).resolve() not in out.parents and (ROOT/'.local/codex-tasks/creeps').resolve() not in out.parents:
        raise ValueError('Output outside owned paths')
    out.mkdir(parents=True,exist_ok=True)
    source=Path(bpy.data.filepath)
    digest=hashlib.sha256(source.read_bytes()).hexdigest()
    pose(prefix,'display')
    floor,camera,device,ray=setup(scene)
    data=camera.data
    root=bpy.data.objects[prefix+'Root']
    original={o.name:list(o.data.materials) for o in scene.objects if o.type=='MESH' and o.name.startswith(prefix)}
    clay=bpy.data.materials.new('Control_Clay')
    clay.use_nodes=True
    b=clay.node_tree.nodes.get('Principled BSDF')
    b.inputs['Base Color'].default_value=(.40,.43,.46,1)
    b.inputs['Roughness'].default_value=.54
    transform=scene.view_settings.view_transform
    look=scene.view_settings.look
    records=[]
    scale=4.2 if prefix=='B1_' else 4.6
    target_z=1.4 if prefix=='B1_' else 1.72
    for name in a.views:
        root.rotation_euler.z=0
        for key,mats in original.items():
            obj=bpy.data.objects[key]
            obj.data.materials.clear()
            for mat in ([clay] if name=='clay' else mats):
                obj.data.materials.append(mat)
        if name.startswith('game-'):
            yaw=int(name.split('-')[1])
            root.rotation_euler.z=math.radians(yaw)
            scene.render.resolution_x,scene.render.resolution_y=1920,1080
            data.type='PERSP'
            data.sensor_fit='VERTICAL'
            data.angle_y=math.radians(45)
            camera.location=(0,-19*math.cos(math.radians(56)),19*math.sin(math.radians(56)))
            point_at(camera,(0,0,0))
        else:
            scene.render.resolution_x=scene.render.resolution_y=a.size
            data.type='ORTHO'
            data.ortho_scale=scale
            views={'three-quarter':(-4.2,-6.4,target_z+2.0),'clay':(-4.2,-6.4,target_z+2.0),
                   'side':(-8,0,target_z),'back-three-quarter':(4.2,6.4,target_z+2.0)}
            camera.location=views[name]
            point_at(camera,(0,0,target_z))
        bpy.context.view_layer.update()
        floor.hide_render=False
        scene.render.engine='CYCLES'
        scene.view_settings.view_transform=transform
        scene.view_settings.look=look
        scene.render.film_transparent=False
        scene.render.filepath=str(out/(name+'.png'))
        started=time.monotonic()
        print('START VIEW',name,flush=True)
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
        scene.render.filepath=str(out/(name+'-mask.png'))
        bpy.ops.render.render(write_still=True)
        if name.startswith('game-'):
            scene.display.shading.color_type='OBJECT'
            for key in original:
                special=key.startswith(prefix+('Yoke' if prefix=='B1_' else 'Sail')) or key==prefix+'Cleaver'
                bpy.data.objects[key].color=(0,1,0,1) if prefix=='E2_' and key==prefix+'Cleaver' else ((1,0,0,1) if special else (0,0,0,1))
            scene.render.filepath=str(out/(name+'-special-id.png'))
            bpy.ops.render.render(write_still=True)
        records.append({'view':name,'seconds':round(time.monotonic()-started,3)})
        (out/'render-log.json').write_text(json.dumps({'source':str(source.relative_to(ROOT)),
            'source_sha256':digest,'revision':scene['revision'],'device':device,'samples':48,
            'denoising':True,'view_transform':transform,'look':look,'sun_ray':list(ray),'exposure':.7,
            'game_camera':{'pitch':56,'vertical_fov':45,'distance':19,'resolution':[1920,1080]},
            'orthographic_scale':scale,'mask_ids':'B1 yoke red; E2 sail red and cleaver green; other body black',
            'views':records},indent=2)+'\n',encoding='utf-8')
    if hashlib.sha256(source.read_bytes()).hexdigest()!=digest:
        raise RuntimeError('Loaded source changed')
    print('RENDERED',creep,len(records),device,transform)


if __name__=='__main__':
    main()

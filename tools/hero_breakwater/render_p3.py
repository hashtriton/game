"""Sequential clay renders and exact geometry masks for pose/facing review."""
import argparse
import hashlib
import json
import math
import sys
import time
from pathlib import Path

sys.dont_write_bytecode=True
sys.path.insert(0,str(Path(__file__).resolve().parent))
import bpy
from render_views import point_at, area
from build_p3 import pose

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--variant',choices=['p2','p3'],default='p3')
    parser.add_argument('--views',nargs='*',default=['three-quarter','side','back','game-045'])
    parser.add_argument('--output',type=Path,default=ART/'renders/p3')
    parser.add_argument('--size',type=int,default=1000)
    opt=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    if not bpy.app.background:
        raise RuntimeError('Background Blender only')
    out=opt.output.resolve()
    if ART.resolve() not in out.parents and (ROOT/'.local/codex-tasks/hero-b').resolve() not in out.parents:
        raise ValueError('Approved output folders only')
    out.mkdir(parents=True,exist_ok=True)
    source=Path(bpy.data.filepath)
    digest=hashlib.sha256(source.read_bytes()).hexdigest()
    if opt.variant=='p3':
        pose('display')
    scene=bpy.context.scene
    scene.render.engine='CYCLES'
    scene.cycles.samples=32
    scene.cycles.use_denoising=True
    pref=bpy.context.preferences.addons['cycles'].preferences
    device='CPU'
    for backend in ['OPTIX','CUDA']:
        try:
            pref.compute_device_type=backend
            pref.get_devices()
            gpu=[d for d in pref.devices if d.type==backend]
            if gpu:
                for d in pref.devices:
                    d.use=d.type==backend
                scene.cycles.device='GPU'
                device=backend+': '+', '.join(d.name for d in gpu)
                break
        except Exception:
            continue
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.image_settings.color_mode='RGBA'
    scene.render.image_settings.color_depth='8'
    scene.render.image_settings.compression=90
    scene.view_settings.view_transform='AgX'
    scene.view_settings.look='AgX - Medium High Contrast'
    scene.world.use_nodes=True
    scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.40,.46,.53,1)
    scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.22
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008))
    floor=bpy.context.object
    floor.name='Studio_Floor'
    mat=bpy.data.materials.new('Studio_Ground')
    mat.use_nodes=True
    mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.27,.30,.34,1)
    mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.9
    floor.data.materials.append(mat)
    area('Studio_Key',(-3.5,-4.5,6),720,3,(1,.95,.89))
    area('Studio_Fill',(4,-1,3.3),220,3.5,(.80,.89,1))
    area('Studio_Rim',(1,4,5),520,3,(.89,.95,1))
    data=bpy.data.cameras.new('Studio_Camera')
    data.clip_start,data.clip_end=.01,300
    camera=bpy.data.objects.new('Studio_Camera',data)
    bpy.context.collection.objects.link(camera)
    scene.camera=camera
    views={'front':((0,-8,1.20),(0,-.08,1.20),3.10),
           'side':((-8,0,1.2),(0,-.08,1.20),3.10),
           'back':((0,8,1.2),(0,-.08,1.20),3.10),
           'three-quarter':((-4.2,-6.4,3.15),(0,-.08,1.20),3.10)}
    root=bpy.data.objects['HB_Root']
    records=[]
    for name in opt.views:
        root.rotation_euler.z=0
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
            location,target,scale=views[name]
            scene.render.resolution_x=scene.render.resolution_y=opt.size
            data.type='ORTHO'
            data.ortho_scale=scale
            camera.location=location
            point_at(camera,target)
        bpy.context.view_layer.update()
        scene.render.engine='CYCLES'
        scene.render.film_transparent=False
        scene.view_settings.view_transform='AgX'
        scene.render.filepath=str(out/(name+'.png'))
        started=time.monotonic()
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
        floor.hide_render=False
        records.append({'view':name,'seconds':round(time.monotonic()-started,3),
                        'root_yaw_degrees':math.degrees(root.rotation_euler.z)})
    if hashlib.sha256(source.read_bytes()).hexdigest()!=digest:
        raise RuntimeError('Source changed during render')
    log={'source':str(source.relative_to(ROOT)),'source_sha256':digest,'source_unchanged':True,
         'device':device,'samples':32,'pose':'display' if opt.variant=='p3' else 'original P2 rest',
         'game_camera':{'pitch':56,'yaw':0,'distance':19,'vertical_fov':45,'resolution':[1920,1080],
                        'coordinate_convention':'Front -Y; camera front side, hero facing degrees relative to camera'},
         'views':records}
    (out/'render-log.json').write_text(json.dumps(log,indent=2)+'\n',encoding='utf-8')
    print('RENDERED',opt.variant,device,'source unchanged')


if __name__=='__main__':
    main()

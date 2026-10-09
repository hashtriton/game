"""Load a blockout, render neutral inspection views, never save the source."""
import argparse
import hashlib
import json
import math
import sys
import time
from pathlib import Path

import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/heroes/breakwater'

def point_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat('-Z', 'Y').to_euler()

def area(name, location, energy, size, color):
    data = bpy.data.lights.new(name, 'AREA')
    data.energy, data.size, data.color = energy, size, color
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    point_at(obj, (0, 0, 1.1))

def main():
    if '--variant' in sys.argv and sys.argv[sys.argv.index('--variant')+1]=='p3':
        sys.dont_write_bytecode=True
        sys.path.insert(0,str(Path(__file__).resolve().parent))
        from render_p3 import main as p3_main
        return p3_main()
    parser = argparse.ArgumentParser()
    parser.add_argument('--variant', choices=['p1','p2'], required=True)
    parser.add_argument('--views', nargs='*', default=['front','side','back','top','three-quarter','game','game-front'])
    opt = parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    if not bpy.app.background:
        raise RuntimeError('Background Blender only')
    source = Path(bpy.data.filepath).resolve()
    if source != (ART / f'blockout-{opt.variant}.blend').resolve():
        raise ValueError('Load the matching blockout file')
    before = hashlib.sha256(source.read_bytes()).hexdigest()
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 32
    scene.cycles.use_denoising = True
    device = 'CPU'
    try:
        preferences = bpy.context.preferences.addons['cycles'].preferences
        for backend in ['OPTIX','CUDA']:
            try:
                preferences.compute_device_type = backend
                preferences.get_devices()
                active = [d for d in preferences.devices if d.type == backend]
                if active:
                    for d in preferences.devices:
                        d.use = d.type == backend
                    scene.cycles.device = 'GPU'
                    device = backend + ': ' + ', '.join(d.name for d in active)
                    break
            except Exception:
                continue
    except Exception:
        scene.cycles.samples = 16
    if device == 'CPU':
        scene.cycles.samples = 16
        scene.cycles.device = 'CPU'
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGBA'
    scene.render.image_settings.color_depth = '8'
    scene.render.image_settings.compression = 80
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.look = 'AgX - Medium High Contrast'
    scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs['Color'].default_value = (.64,.70,.78,1)
    scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value = .35
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0,0,-.008))
    floor = bpy.context.object
    floor.name = 'Studio_Floor'
    mat = bpy.data.materials.new('Studio_Floor_Clay')
    mat.diffuse_color = (.67,.68,.70,1)
    mat.use_nodes = True
    mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value = mat.diffuse_color
    mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value = .9
    floor.data.materials.append(mat)
    area('Studio_Key',(-3.5,-4.5,6),580,4,(1,.95,.88))
    area('Studio_Fill',(4,-1,3.3),300,3.5,(.78,.88,1))
    area('Studio_Rim',(1,4,5),490,3.5,(.89,.95,1))
    data = bpy.data.cameras.new('Studio_Camera')
    data.clip_start, data.clip_end = .01, 300
    camera = bpy.data.objects.new('Studio_Camera',data)
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    views = {
        'front': ((0,-8,1.2),(0,0,1.2),2.90),
        'side': ((-8,0,1.2),(0,-.08,1.2),2.90),
        'back': ((0,8,1.2),(0,0,1.2),2.90),
        'top': ((0,0,8),(0,0,0),2.90),
        'three-quarter': ((-4.2,-6.4,3.5),(0,-.06,1.20),2.95),
    }
    output = ART/'renders'/opt.variant
    output.mkdir(parents=True,exist_ok=True)
    records=[]
    for name in opt.views:
        scene.render.engine = 'CYCLES'
        scene.render.film_transparent = False
        scene.view_settings.view_transform='AgX'
        if name in ('game','game-front'):
            data.type = 'PERSP'
            data.sensor_fit = 'VERTICAL'
            data.angle_y = math.radians(45)
            scene.render.resolution_x,scene.render.resolution_y=1920,1080
            # Unity yaw=0 looks toward +Z, which maps to Blender -Y.
            sign = 1 if name=='game' else -1
            camera.location=(0,sign*19*math.cos(math.radians(56)),19*math.sin(math.radians(56)))
            point_at(camera,(0,0,0))
        else:
            position,target,scale=views[name]
            data.type='ORTHO'
            data.ortho_scale=scale
            scene.render.resolution_x=scene.render.resolution_y=1280
            camera.location=position
            point_at(camera,target)
        scene.render.filepath=str(output/f'{name}.png')
        started=time.monotonic()
        bpy.ops.render.render(write_still=True)
        records.append({'view':name,'seconds':round(time.monotonic()-started,3)})
        if name in ('game','game-front'):
            record={'pitch':56,'distance':19,'vertical_fov_degrees':math.degrees(data.angle_y),
                    'resolution':[1920,1080], 'position':list(camera.location),
                    'focus':[0,0,0],'yaw':0 if name=='game' else 180,
                    'description':'Runtime relative rear direction' if name=='game' else 'Same pitch/FOV/distance, front direction; equivalent to hero turned 180 degrees in runtime'}
            (output/f'{name}-camera.json').write_text(json.dumps(record,indent=2)+'\n',encoding='utf-8')
            # The mask is generated by geometry, not a luminance threshold on clay.
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
            scene.render.filepath=str(output/f'{name}-mask.png')
            bpy.ops.render.render(write_still=True)
            floor.hide_render=False
    after=hashlib.sha256(source.read_bytes()).hexdigest()
    if before!=after:
        raise RuntimeError('Renderer changed source')
    (output/'render-log.json').write_text(json.dumps({'source_sha256':before,'source_unchanged':True,
                                                   'device':device,'samples':scene.cycles.samples,
                                                   'views':records},indent=2)+'\n',encoding='utf-8')
    print('RENDERED '+opt.variant+' '+device+' source unchanged')

if __name__=='__main__':
    main()

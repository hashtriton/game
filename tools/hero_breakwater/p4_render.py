"""Sequential OptiX daylight views, masks and same-camera clay controls."""
import argparse
import hashlib
import json
import math
import sys
import time
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from mathutils import Vector
from render_views import point_at

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'art/heroes/breakwater/p4'
LOCAL = ROOT / '.local/codex-tasks/hero-b'


def setup(scene):
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 48
    scene.cycles.use_denoising = True
    scene.cycles.seed = 42
    scene.cycles.use_animated_seed = False
    scene.cycles.max_bounces = 6
    scene.cycles.diffuse_bounces = 3
    scene.cycles.glossy_bounces = 3
    pref = bpy.context.preferences.addons['cycles'].preferences
    pref.compute_device_type = 'OPTIX'
    pref.get_devices()
    gpu = [d for d in pref.devices if d.type == 'OPTIX']
    if not gpu:
        raise RuntimeError('OptiX required; no silent CPU fallback')
    for d in pref.devices:
        d.use = d.type == 'OPTIX'
    scene.cycles.device = 'GPU'
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.image_settings.color_mode = 'RGBA'
    scene.render.image_settings.color_depth = '8'
    scene.render.image_settings.compression = 90
    try:
        scene.view_settings.view_transform = 'ACES 2.0'
    except TypeError:
        scene.view_settings.view_transform = 'AgX'
        scene.view_settings.look = 'AgX - Medium High Contrast'
    scene.view_settings.exposure = .7
    scene.world.use_nodes = True
    n, l = scene.world.node_tree.nodes, scene.world.node_tree.links
    n.clear()
    coord = n.new('ShaderNodeTexCoord')
    split = n.new('ShaderNodeSeparateXYZ')
    l.new(coord.outputs['Normal'], split.inputs['Vector'])
    ramp = n.new('ShaderNodeValToRGB')
    ramp.color_ramp.elements[0].position = 0
    ramp.color_ramp.elements[0].color = (.46,.39,.28,1)
    ramp.color_ramp.elements[1].position = 1
    ramp.color_ramp.elements[1].color = (.4,.5,.64,1)
    ramp.color_ramp.elements.new(.5).color = (.3,.37,.47,1)
    remap = n.new('ShaderNodeMapRange')
    remap.inputs['From Min'].default_value = -1
    remap.inputs['From Max'].default_value = 1
    l.new(split.outputs['Z'], remap.inputs['Value'])
    l.new(remap.outputs['Result'], ramp.inputs['Fac'])
    bg = n.new('ShaderNodeBackground')
    bg.inputs['Strength'].default_value = .48
    l.new(ramp.outputs['Color'], bg.inputs['Color'])
    output = n.new('ShaderNodeOutputWorld')
    l.new(bg.outputs['Background'],output.inputs['Surface'])
    data = bpy.data.lights.new('Daylight_Sun','SUN')
    data.energy = 2.9
    data.color = (1,.9,.76)
    data.angle = math.radians(4)
    sun = bpy.data.objects.new('Daylight_Sun',data)
    bpy.context.collection.objects.link(sun)
    ray = Vector((math.sin(math.radians(135))*math.cos(math.radians(42)),
                  math.cos(math.radians(135))*math.cos(math.radians(42)),
                  -math.sin(math.radians(42))))
    sun.rotation_euler = ray.to_track_quat('-Z','Y').to_euler()
    bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.012))
    floor = bpy.context.object
    floor.name='Daylight_Floor'
    mat = bpy.data.materials.new('Daylight_PaleBlueStone')
    mat.use_nodes=True
    bsdf=mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value=(.27,.31,.36,1)
    bsdf.inputs['Roughness'].default_value=.78
    tex=mat.node_tree.nodes.new('ShaderNodeTexNoise')
    tex.inputs['Scale'].default_value=7
    tex.inputs['Detail'].default_value=1
    bump=mat.node_tree.nodes.new('ShaderNodeBump')
    bump.inputs['Strength'].default_value=.12
    bump.inputs['Distance'].default_value=.008
    mat.node_tree.links.new(tex.outputs['Fac'],bump.inputs['Height'])
    mat.node_tree.links.new(bump.outputs['Normal'],bsdf.inputs['Normal'])
    floor.data.materials.append(mat)
    camdata=bpy.data.cameras.new('Daylight_Camera')
    camera=bpy.data.objects.new('Daylight_Camera',camdata)
    bpy.context.collection.objects.link(camera)
    camdata.clip_start,camdata.clip_end=.01,300
    scene.camera=camera
    return floor,camera,', '.join(d.name for d in gpu),tuple(ray)


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--output',type=Path,default=OUT/'renders')
    p.add_argument('--views',nargs='+',default=['three-quarter','back-three-quarter','side']+[f'game-{d:03}' for d in range(0,360,45)]+['clay'])
    p.add_argument('--size',type=int,default=1280)
    p.add_argument('--p3',action='store_true')
    a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    out=a.output.resolve()
    if OUT.resolve() not in out.parents and LOCAL.resolve() not in out.parents:
        raise ValueError('P4 or local task outputs only')
    out.mkdir(parents=True,exist_ok=True)
    source=Path(bpy.data.filepath)
    before=hashlib.sha256(source.read_bytes()).hexdigest()
    if a.p3:
        from build_p3 import pose
    else:
        from p4_build import pose
    pose('display')
    scene=bpy.context.scene
    floor,camera,device,ray=setup(scene)
    data=camera.data
    transform=scene.view_settings.view_transform
    look=scene.view_settings.look
    root=bpy.data.objects['HB_Root']
    original={o.name:list(o.data.materials) for o in scene.objects if o.name.startswith('HB_') and o.type=='MESH'}
    clay=bpy.data.materials.new('Control_Clay')
    clay.use_nodes=True
    clay.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.40,.43,.46,1)
    clay.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.54
    records=[]
    for name in a.views:
        root.rotation_euler.z=0
        for key,mats in original.items():
            obj=bpy.data.objects[key]
            obj.data.materials.clear()
            for m in mats:
                obj.data.materials.append(m)
        if name=='clay' or a.p3:
            for key in original:
                obj=bpy.data.objects[key]
                obj.data.materials.clear()
                obj.data.materials.append(clay)
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
            data.ortho_scale=3.10
            views={'three-quarter':((-4.2,-6.4,3.15),(0,-.08,1.20)),
                   'clay':((-4.2,-6.4,3.15),(0,-.08,1.20)),
                   'side':((-8,0,1.20),(0,-.08,1.20)),
                   'back-three-quarter':((4.2,6.4,3.15),(0,-.08,1.20)),
                   'front':((0,-8,1.20),(0,-.08,1.20))}
            camera.location,target=views[name]
            point_at(camera,target)
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
                bpy.data.objects[key].color=(1,0,0,1) if key.startswith('HB_Sword') else (0,0,0,1)
            scene.render.filepath=str(out/(name+'-sword-id.png'))
            bpy.ops.render.render(write_still=True)
            scene.display.shading.color_type='SINGLE'
            if name=='game-045':
                equipment=[bpy.data.objects[k] for k in original if k.startswith(('HB_Sword','HB_Shield','HB_Rim'))]
                for o in equipment:
                    o.hide_render=True
                scene.render.filepath=str(out/(name+'-body-mask.png'))
                bpy.ops.render.render(write_still=True)
                for o in equipment:
                    o.hide_render=False
        records.append({'view':name,'seconds':round(time.monotonic()-started,3),'yaw':math.degrees(root.rotation_euler.z)})
        (out/'render-log.json').write_text(json.dumps({'source':str(source.relative_to(ROOT)),
            'source_sha256':before,'pose':'display','device':'OPTIX: '+device,'samples':48,
            'denoising':True,'view_transform':transform,'look':look,'sun_energy':2.9,
            'sun_color':[1,.9,.76],'sun_ray_blender':ray,'ambient_strength':.48,
            'game_camera':{'pitch':56,'vertical_fov':45,'distance':19,'resolution':[1920,1080]},
            'views':records},indent=2)+'\n',encoding='utf-8')
    if before!=hashlib.sha256(source.read_bytes()).hexdigest():
        raise RuntimeError('Source changed')
    print('RENDERED',len(records),'OPTIX',transform,device)


if __name__=='__main__':
    main()

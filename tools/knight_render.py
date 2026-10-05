"""Read-only Cycles review of saved knight-v3 geometry, with a separate GUI copy."""
import hashlib
import json
from pathlib import Path
import sys
import time
import bpy
from mathutils import Vector

ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'tools'))
from hero_veteran_render import configure_cycles_device, point_at, add_area


def main():
    if not bpy.app.background:
        raise RuntimeError('Use background Blender.')
    bpy.context.preferences.use_preferences_save=False
    source=Path(bpy.data.filepath).resolve()
    if source.parent!=(ROOT/'art/heroes/knight-v3').resolve():
        raise RuntimeError('Load a knight-v3 source blend.')
    args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else ['clay']
    mode=args[0]
    if mode not in {'clay','final','prepare'}:
        raise ValueError(mode)
    presentation=source.parent/'knight-presentation.blend'
    if mode=='prepare' and source==presentation.resolve():
        raise ValueError('Prepare requires an asset source, not the presentation destination.')
    selected=args[1:] or (['beauty','front','side','back','game'] if mode=='clay'
                          else ['beauty','front','side','back','game','head'])
    before=hashlib.sha256(source.read_bytes()).hexdigest()
    scene=bpy.context.scene
    studio=bpy.data.collections.new('REVIEW_STUDIO')
    scene.collection.children.link(studio)
    def mat(name,color,rough):
        m=bpy.data.materials.new(name)
        m.use_nodes=True
        m.diffuse_color=(*color,1)
        bs=m.node_tree.nodes.get('Principled BSDF')
        bs.inputs['Base Color'].default_value=(*color,1)
        bs.inputs['Roughness'].default_value=rough
        return m
    floor=bpy.data.meshes.new('Ground')
    floor.from_pydata([(-200,-200,.024),(200,-200,.024),(200,200,.024),(-200,200,.024)],[],[(0,1,2,3)])
    floor.update()
    ob=bpy.data.objects.new('Ground',floor)
    studio.objects.link(ob)
    floor.materials.append(mat('Neutral ground',(.039,.049,.062),.83))
    world=bpy.data.worlds.new('Neutral studio')
    world.use_nodes=True
    world.node_tree.nodes['Background'].inputs['Color'].default_value=(.15,.19,.25,1)
    world.node_tree.nodes['Background'].inputs['Strength'].default_value=.36
    scene.world=world
    add_area(studio,'Large key',(-3,-4,5),(0,0,1.1),700,(1,.92,.81),3.5)
    add_area(studio,'Fill',(3,-2,3),(0,0,1.1),460,(.72,.83,1),3)
    add_area(studio,'Rim',(1,3,4),(0,0,1.3),950,(1,.84,.64),2.7)
    add_area(studio,'Front softbox',(0,-4,2.1),(0,0,1.25),95,(.92,.96,1),2.5)
    data=bpy.data.cameras.new('Camera')
    data.type='ORTHO'
    camera=bpy.data.objects.new('Camera',data)
    studio.objects.link(camera)
    scene.camera=camera
    device=configure_cycles_device(scene)
    clay=mode=='clay'
    if clay:
        scene.view_layers[0].material_override=mat('Neutral clay',(.29,.33,.38),.55)
    scene.cycles.samples=28 if clay else 96
    scene.cycles.use_denoising=True
    scene.render.resolution_x=900 if clay else 1440
    scene.render.resolution_y=1100 if clay else 1760
    scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'
    scene.render.image_settings.color_mode='RGB'
    scene.view_settings.view_transform='AgX'
    scene.view_settings.look='AgX - Medium High Contrast'
    scene.view_settings.exposure=0
    scene.view_settings.gamma=1
    views={
        'beauty':((3.2,-8,3.4),(0,0,1.1),2.65),
        'front':((0,-9,1.20),(0,0,1.10),2.56),
        'side':((9,0,1.2),(0,0,1.10),2.56),
        'back':((-3,8,3.0),(0,.05,1.10),2.65),
        'game':((4,-6,7),(0,0,1.03),2.8),
        'head':((1.6,-5,2.9),(0,-.02,1.81),.88),
    }
    out=source.parent/'previews'
    out.mkdir(exist_ok=True)
    records=[]
    if mode!='prepare':
        for name in selected:
            position,target,scale=views[name]
            camera.location=position
            point_at(camera,target)
            data.ortho_scale=scale
            scene.render.filepath=str(out/f'{source.stem}-{name}.png')
            start=time.monotonic()
            bpy.ops.render.render(write_still=True)
            records.append({'view':name,'path':scene.render.filepath,'seconds':round(time.monotonic()-start,2)})
    else:
        position,target,scale=views['beauty']
        camera.location=position
        point_at(camera,target)
        data.ortho_scale=scale
        for o in bpy.context.selected_objects:
            o.select_set(False)
        for screen in bpy.data.screens:
            for area in screen.areas:
                if area.type=='VIEW_3D':
                    area.spaces.active.shading.type='MATERIAL'
                    area.spaces.active.region_3d.view_perspective='CAMERA'
        scene['source_sha256']=before
        bpy.ops.wm.save_as_mainfile(filepath=str(presentation))
    after=hashlib.sha256(source.read_bytes()).hexdigest()
    if before!=after:
        raise RuntimeError('Source changed during rendering.')
    report={'source':str(source),'sha256':before,'source_unchanged':True,'device':device,
            'mode':mode,'views':records}
    name=f'{source.stem}-{mode}-'+('-'.join(selected))+'.json'
    (out/name).write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('KNIGHT_RENDER '+json.dumps(report))


if __name__=='__main__':
    main()

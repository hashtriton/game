"""Measured direction sheets and native-scale mock compositing, no stretch."""
import argparse
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[2]
LOCAL=ROOT/'.local/codex-tasks/creeps'
BG=(224,231,236)
INK=(26,45,61)
CONFIG={'basalt-yoke':{'label':'B1 / БАЗАЛЬТОВОЕ ЯРМО','concept':'bruisers','crop':(30,132,580,610),
                       'mask_roi':(765,833,957,995)},
        'ash-sail':{'label':'E2 / ПЕПЕЛЬНЫЙ ПАРУС','concept':'elites','crop':(990,132,1422,669),
                    'mask_roi':(1723,832,1866,995)}}


def text(d,xy,s,size=20,bold=False):
    f=ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),size)
    d.text(xy,s,font=f,fill=INK)


def save(im,path):
    path.parent.mkdir(parents=True,exist_ok=True)
    im.save(path,optimize=True)
    if path.stat().st_size>=8000000:
        raise AssertionError('PNG over 8 MB: '+str(path))


def mask(folder,name):
    return Image.open(folder/(name+'-mask.png')).getchannel('A').point(lambda v:255 if v>=128 else 0)


def numbers(m):
    b=m.getbbox()
    crop=np.asarray(m.crop(b))>=128
    return {'bbox':list(b),'width':b[2]-b[0],'height':b[3]-b[1],
            'aspect_ratio':crop.shape[1]/crop.shape[0],'fill_ratio':int(crop.sum())/crop.size,
            'area_pixels':int(crop.sum())}


def tile(folder,name,h,black=False):
    m=mask(folder,name)
    b=m.getbbox()
    w=round((b[2]-b[0])*h/(b[3]-b[1]))
    alpha=m.crop(b).resize((w,h),Image.Resampling.LANCZOS)
    im=Image.new('RGB',(w,h),BG)
    color=(0,0,0) if black else Image.open(folder/(name+'.png')).convert('RGB').crop(b).resize((w,h),Image.Resampling.LANCZOS)
    im.paste(color,(0,0,w,h) if black else (0,0),alpha)
    return im


def fitted(folder,name,size):
    b=mask(folder,name).getbbox()
    im=Image.open(folder/(name+'.png')).convert('RGB').crop((b[0]-8,b[1]-8,b[2]+8,b[3]+8))
    im.thumbnail(size,Image.Resampling.LANCZOS)
    return im


def contact(creep,before,after,index):
    canvas=Image.new('RGB',(1920,950),BG)
    d=ImageDraw.Draw(canvas)
    text(d,(24,18),CONFIG[creep]['label']+f' / ЦИКЛ {index} / ДО И ПОСЛЕ',30,True)
    for j,folder in enumerate([before,after]):
        text(d,(24+j*960,70),('ДО' if j==0 else 'ПОСЛЕ')+' / '+folder.parent.name,24,True)
        for k,name in enumerate(['three-quarter','side']):
            im=Image.open(folder/(name+'.png')).convert('RGB').resize((450,450),Image.Resampling.LANCZOS)
            canvas.paste(im,(20+j*960+k*465,113))
        for k,h in enumerate([110,135,160]):
            im=tile(folder,'game-045',h)
            canvas.paste(im,(70+j*960+k*280,800-h))
            text(d,(70+j*960+k*280,815),str(h)+' px',20)
    text(d,(24,905),'Одинаковые камера/свет/масштаб. Display pose, Cycles OptiX 48, ACES 2.0.',20)
    path=LOCAL/f'cycles-{creep}'/f'cycle{index}-before-after.png'
    save(canvas,path)
    print(path)


def compose(creep):
    cfg=CONFIG[creep]
    out=ROOT/'art/creatures'/creep
    renders=out/'renders'
    conceptpath=ROOT/'art/concepts'/('2026-10-09-creeps-'+cfg['concept']+'-v1.png')
    concept=Image.open(conceptpath).convert('RGB')
    roi=concept.crop(cfg['mask_roi'])
    sensitivity={}
    for threshold in [50,65,80]:
        a=np.asarray(roi)
        binary=(a.max(axis=2)<threshold)
        m=Image.fromarray((binary*255).astype('uint8'))
        sensitivity[str(threshold)]=numbers(m)
        save(m,out/f'concept-mask-{threshold}.png')
    cmask=Image.open(out/'concept-mask-65.png')
    metric={'concept':numbers(cmask),'concept_extraction':{'source':conceptpath.relative_to(ROOT).as_posix(),
            'sha256':hashlib.sha256(conceptpath.read_bytes()).hexdigest(),'roi':cfg['mask_roi'],
            'method':'Existing black 160 px concept silhouette, max RGB <65; no filling, no stretching.',
            'sensitivity':sensitivity,'limitations':'AI concept top camera is labelled 56 degrees but no verified extrinsics. Compare projection guide, not reconstructed anatomy.'},
            'render_game_000':numbers(mask(renders,'game-000')),'comparison_facing':0,'directions':{}}
    metric['relative_error']={k:abs(metric['render_game_000'][k]/metric['concept'][k]-1) for k in ['aspect_ratio','fill_ratio']}
    metric['within_12_percent']=all(v<=.12 for v in metric['relative_error'].values())
    separate_weapon='mask_ids' in json.loads((renders/'render-log.json').read_text()) and creep=='ash-sail'
    for yaw in range(0,360,45):
        name=f'game-{yaw:03}'
        row=numbers(mask(renders,name))
        ids=np.asarray(Image.open(renders/(name+'-special-id.png')).convert('RGBA'))
        special=(ids[:,:,0]>90)&(ids[:,:,1]<40)&(ids[:,:,3]>=128)
        cleaver=(ids[:,:,1]>90)&(ids[:,:,0]<40)&(ids[:,:,3]>=128)
        row['special_visible_pixels']=int(special.sum())
        row['special_fraction']=float(special.sum()/row['area_pixels'])
        if separate_weapon:
            row['cleaver_visible_pixels']=int(cleaver.sum())
            row['cleaver_fraction']=float(cleaver.sum()/row['area_pixels'])
        metric['directions'][str(yaw)]=row
        for h in [110,135,160]:
            for black in [False,True]:
                suffix='silhouette' if black else 'color'
                save(tile(renders,name,h,black),out/'crops'/f'{name}-{h}-{suffix}.png')
    automatic=sorted(range(0,360,45),key=lambda y:metric['directions'][str(y)]['special_fraction'])[:2]
    worst=[90,270] if creep=='basalt-yoke' else (sorted(range(0,360,45),key=lambda y:metric['directions'][str(y)]['cleaver_fraction'])[:2] if separate_weapon else automatic)
    metric['worst_directions']=worst
    metric['lowest_special_fraction_facings']=automatic
    metric['worst_selection']='B1: visually selected side views 90/270, yoke and limbs merge into one narrow column. E2 final: two lowest visible cleaver fractions from occluded green ID renders; before separate IDs, lowest combined special fraction. All eight directions retained.'
    metric['normalization']='Game-size crops normalize full bbox height to 110/135/160; mock retains native projected pixels.'
    metric['clay_material_masks_equal']=mask(renders,'clay').tobytes()==mask(renders,'three-quarter').tobytes()
    (out/'silhouette-metrics.json').write_text(json.dumps(metric,indent=2)+'\n',encoding='utf-8')
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    text(d,(24,15),cfg['label']+' / BLOCKOUT + MATERIAL LOOK TEST',29,True)
    text(d,(24,54),'Blender CLI / display / original geometry / 5 procedural slots / без рига',20)
    im=concept.crop(cfg['crop'])
    im.thumbnail((420,430),Image.Resampling.LANCZOS)
    canvas.paste(im,(25+(420-im.width)//2,120))
    text(d,(30,92),'Концепт (AI)',22,True)
    for name,x,title in [('three-quarter',465,'3/4 материал'),('clay',950,'Clay, та же геометрия')]:
        text(d,(x+10,92),title,22,True)
        im=fitted(renders,name,(435,435))
        canvas.paste(im,(x+(435-im.width)//2,120))
    text(d,(1435,100),'Силуэт vs concept / game 0°',20,True)
    text(d,(1435,143),f'aspect error {metric["relative_error"]["aspect_ratio"]*100:.1f}%',23)
    text(d,(1435,181),f'fill error {metric["relative_error"]["fill_ratio"]*100:.1f}%',23)
    text(d,(1435,220),'12%: '+('PASS' if metric['within_12_percent'] else 'НЕ ДОСТИГНУТО'),23,True)
    for j,h in enumerate([110,160]):
        im=tile(renders,'game-000',h)
        canvas.paste(im,(1435+j*240,470-h))
        text(d,(1440+j*240,490),str(h)+' px',18)
    text(d,(24,564),'8 НАПРАВЛЕНИЙ / ЦВЕТ И ЧЕРНЫЙ СИЛУЭТ / 135 px',25,True)
    for i,yaw in enumerate(range(0,360,45)):
        x=24+i*235
        for black,y in [(False,620),(True,825)]:
            im=tile(renders,f'game-{yaw:03}',135,black)
            canvas.paste(im,(x+(225-im.width)//2,y))
        text(d,(x+80,773),str(yaw)+'°',19,True)
    text(d,(24,1005),'Камера 56° / vertical FOV 45 / distance 19. Нижние crops нормированы, MOCK-UP показывает native size.',20)
    save(canvas,out/'sheet.png')
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    text(d,(24,20),cfg['label']+' / BEAUTY / BACK / SIDE',30,True)
    for i,name in enumerate(['three-quarter','back-three-quarter','side']):
        im=fitted(renders,name,(620,865))
        canvas.paste(im,(10+i*635+(620-im.width)//2,130))
        text(d,(30+i*635,85),name,22,True)
    save(canvas,out/'sheet-materials.png')
    canvas=Image.new('RGB',(1400,780),BG)
    d=ImageDraw.Draw(canvas)
    text(d,(24,20),'CLAY / MATERIAL / одна геометрия, камера, свет',27,True)
    for i,name in enumerate(['clay','three-quarter']):
        im=Image.open(renders/(name+'.png')).convert('RGB').resize((640,640),Image.Resampling.LANCZOS)
        canvas.paste(im,(25+i*700,90))
    save(canvas,out/'sheet-clay-material.png')
    canvas=Image.new('RGB',(1450,760),BG)
    d=ImageDraw.Draw(canvas)
    text(d,(24,20),'Два слабых направления / 110 и 160 px / цвет + силуэт',27,True)
    for j,yaw in enumerate(worst):
        text(d,(30+j*720,82),f'{yaw}° / special pixels {metric["directions"][str(yaw)]["special_visible_pixels"]}',23,True)
        for i,(h,black) in enumerate([(110,False),(160,False),(110,True),(160,True)]):
            im=tile(renders,f'game-{yaw:03}',h,black)
            x=35+j*720+(i%2)*330
            y=310 if i<2 else 610
            canvas.paste(im,(x,y-h))
            text(d,(x,y+12),str(h)+' px',20)
    save(canvas,out/'sheet-worst-directions.png')
    mock(creep,metric)
    print('COMPOSED',creep,'errors',metric['relative_error'],'worst',worst)


def mock(creep,metric):
    out=ROOT/'art/creatures'/creep
    source=ROOT/'.local/codex-tasks/ow2-arena/shots/final-arena-1920x1080.png'
    im=Image.open(source).convert('RGB')
    layers=[]
    hero=ROOT/'art/heroes/breakwater/p4/renders'
    for name,folder,ground,color in [('hero P4',hero,(810,630),(80,221,255)),
                                    (CONFIG[creep]['label'],out/'renders',(570,610),(227,62,44))]:
        m=mask(folder,'game-045')
        b=m.getbbox()
        cut=Image.open(folder/'game-045.png').convert('RGB').crop(b)
        xy=(ground[0]-(960-b[0]),ground[1]-(540-b[1]))
        d=ImageDraw.Draw(im)
        d.ellipse((ground[0]-28,ground[1]-15,ground[0]+28,ground[1]+15),outline=color,width=2)
        im.paste(cut,xy,m.crop(b))
        d=ImageDraw.Draw(im)
        barx=xy[0]+cut.width//2-31
        bary=xy[1]-13
        d.rectangle((barx,bary,barx+62,bary+6),fill=(30,35,35))
        d.rectangle((barx+1,bary+1,barx+61,bary+5),fill=color)
        layers.append({'name':name,'source':folder.relative_to(ROOT).as_posix(),'bbox':list(b),'placement':list(xy),'native_size':list(cut.size),'resized':False})
    d=ImageDraw.Draw(im)
    d.rectangle((16,16,1904,108),fill=BG)
    text(d,(28,26),'MOCK-UP / Blender creep + hero P4 поверх реального сохраненного кадра арены',27,True)
    text(d,(28,66),'НЕ Unity screenshot новых моделей / 56° / FOV 45 / 19 m / без resize / красная полоса врага',20)
    d.rectangle((24,953,1896,1059),fill=BG)
    text(d,(40,965),f'Native creep bbox {layers[1]["native_size"]} px; hero P4 {layers[0]["native_size"]} px. Старый герой кадра сохранен.',20,True)
    text(d,(40,1005),'Тени, кольца и HP условные. Перенос экрана не воспроизводит perspective shift, occlusion или интеграцию.',19)
    save(im,out/'sheet-gameview.png')
    (out/'mock-gameview.json').write_text(json.dumps({'source':source.relative_to(ROOT).as_posix(),
        'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'source_contains_old_hero':True,
        'label':'MOCK-UP','layers':layers,'limitations':'No engine integration. Native centre-camera projection translated to free paving; perspective shift, scene occlusion and integrated shadows untested.'},indent=2)+'\n',encoding='utf-8')


if __name__=='__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--creep',choices=CONFIG,required=True)
    p.add_argument('--before',type=Path)
    p.add_argument('--after',type=Path)
    p.add_argument('--cycle',default='1')
    a=p.parse_args()
    if a.before:
        contact(a.creep,a.before,a.after,a.cycle)
    else:
        compose(a.creep)

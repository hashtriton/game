"""Honest scale comparisons, retained P3 metrics and clearly marked compositing."""
import argparse
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'
OUT=ART/'p4'
LOCAL=ROOT/'.local/codex-tasks/hero-b'
BG=(224,231,236)
INK=(26,45,61)


def font(size,bold=False):
    return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),size)


def label(d,xy,text,size=20,bold=False):
    d.text(xy,text,font=font(size,bold),fill=INK)


def save(im,p):
    p.parent.mkdir(parents=True,exist_ok=True)
    im.save(p,optimize=True)
    if p.stat().st_size>=8*1024*1024:
        raise ValueError(str(p)+' over 8 MiB')


def alpha(folder,name):
    return Image.open(folder/(name+'-mask.png')).getchannel('A').point(lambda v:255 if v>=128 else 0)


def numbers(mask):
    box=mask.getbbox()
    m=mask.crop(box)
    a=np.asarray(m)>=128
    return {'bbox':list(box),'width':m.width,'height':m.height,'aspect_ratio':m.width/m.height,
            'fill_ratio':int(a.sum())/a.size,'area_pixels':int(a.sum())}


def tile(folder,name,height,black=False):
    mask=alpha(folder,name)
    box=mask.getbbox()
    m=mask.crop(box)
    width=round(m.width*height/m.height)
    m=m.resize((width,height),Image.Resampling.LANCZOS)
    c=Image.new('RGB',(width,height),BG)
    if black:
        c.paste((0,0,0),(0,0,width,height),m)
    else:
        color=Image.open(folder/(name+'.png')).convert('RGB').crop(box).resize((width,height),Image.Resampling.LANCZOS)
        c.paste(color,(0,0),m)
    return c


def fitted(folder,name,size):
    m=alpha(folder,name)
    b=m.getbbox()
    p=Image.open(folder/(name+'.png')).convert('RGB').crop((b[0]-10,b[1]-10,b[2]+10,b[3]+10))
    p.thumbnail(size,Image.Resampling.LANCZOS)
    return p


def contact(before,after,index):
    canvas=Image.new('RGB',(1920,1000),BG)
    d=ImageDraw.Draw(canvas)
    label(d,(28,18),f'P4 / ЦИКЛ {index} / ДО И ПОСЛЕ / один ракурс и масштаб',30,True)
    for j,(folder,title) in enumerate([(before,'ДО'),(after,'ПОСЛЕ')]):
        label(d,(30+j*960,72),title+' / '+folder.name,23,True)
        for i,name in enumerate(['three-quarter','side']):
            p=Image.open(folder/(name+'.png')).convert('RGB').resize((450,450),Image.Resampling.LANCZOS)
            canvas.paste(p,(20+j*960+i*465,115))
        for i,(name,height) in enumerate([('game-045',110),('game-045',135),('game-045',160)]):
            p=tile(folder,name,height)
            canvas.paste(p,(80+j*960+i*265,810-height))
            label(d,(80+j*960+i*265,834),str(height)+' px',20)
    label(d,(28,945),'Cycles OptiX / 48 samples / display. Цвет и форма менялись вместе; isolated material control отдельно.',19)
    save(canvas,LOCAL/f'cycles-3/cycle{index}-before-after.png')


def screenshot_inventory():
    files=[]
    for folder in [ROOT/'.local/codex-tasks/ow2-arena/shots',ROOT/'.local/codex-tasks/hud/shots']:
        for p in folder.glob('*.png'):
            if Image.open(p).size==(1920,1080):
                files.append(p)
    canvas=Image.new('RGB',(1500,220*((len(files)+4)//5)),BG)
    d=ImageDraw.Draw(canvas)
    for i,p in enumerate(files):
        x,y=(i%5)*300,(i//5)*220
        im=Image.open(p).convert('RGB')
        im.thumbnail((290,170))
        canvas.paste(im,(x,y))
        label(d,(x+2,y+173),p.name,12)
    save(canvas,LOCAL/'cycles-3/arena-inventory.png')


def compose():
    renders=OUT/'renders'
    p3=OUT/'p3-control'
    old=json.loads((ART/'silhouette-metrics-p3.json').read_text())
    metrics={k:old[k] for k in ['concept_3quarter','concept_extraction','p2_3quarter','p3_3quarter']}
    metrics['method']='Same saved threshold concept mask and P3 silhouette_numbers: alpha >=128, tight bbox, width/height and binary area/bbox area. Same orthographic camera and 3.10 m scale; raster resolution differs only.'
    metrics['p4_3quarter']=numbers(alpha(renders,'three-quarter'))
    metrics['p4_3quarter']['relative_error_to_concept']={k:abs(metrics['p4_3quarter'][k]/metrics['concept_3quarter'][k]-1) for k in ['aspect_ratio','fill_ratio']}
    metrics['p4_3quarter']['within_12_percent']=all(v<=.12 for v in metrics['p4_3quarter']['relative_error_to_concept'].values())
    metrics['directions']={}
    for yaw in range(0,360,45):
        name=f'game-{yaw:03}'
        row=numbers(alpha(renders,name))
        segmented=np.asarray(Image.open(renders/(name+'-sword-id.png')).convert('RGBA'))
        sword=(segmented[:,:,0]>90)&(segmented[:,:,1]<40)&(segmented[:,:,3]>128)
        row['visible_sword_pixels']=int(sword.sum())
        row['sword_visible_fraction_of_hero']=float(sword.sum()/row['area_pixels'])
        metrics['directions'][str(yaw)]=row
    worst=sorted(range(0,360,45),key=lambda y:metrics['directions'][str(y)]['sword_visible_fraction_of_hero'])[:2]
    metrics['worst_directions_degrees']=worst
    metrics['worst_selection']='Two lowest visible sword pixel fractions in actual occluded object-ID renders, followed by visual review. Shield edge-on weakness is also shown by all eight silhouettes.'
    metrics['normalization']='110/135/160 px tiles rescale the full equipment bbox to requested height. Mock uses original projected pixels without resizing.'
    metrics['body_game_045']=numbers(Image.open(renders/'game-045-body-mask.png').getchannel('A'))
    (OUT/'silhouette-metrics-p4.json').write_text(json.dumps(metrics,indent=2)+'\n',encoding='utf-8')
    for yaw in range(0,360,45):
        for h in [110,135,160]:
            for black in [False,True]:
                suffix='silhouette' if black else 'color'
                save(tile(renders,f'game-{yaw:03}',h,black),OUT/f'crops/game-{yaw:03}-{h}-{suffix}.png')
    concept=Image.open(ROOT/'art/concepts/2026-10-09-hero-b-breakwater-v1.png').convert('RGB').crop((84,136,841,735))
    concept.thumbnail((380,395),Image.Resampling.LANCZOS)
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    label(d,(28,18),'ВОЛНОЛОМ / P4 / ФОРМА + МАТЕРИАЛЫ / ДНЕВНОЙ СВЕТ',29,True)
    label(d,(28,57),'Blender CLI / 6 procedural materials / Cycles OptiX 48 / ACES 2.0 / display pose / без рига',19)
    label(d,(28,96),'Концепт B (AI)',20,True)
    canvas.paste(concept,(20+(390-concept.width)//2,130))
    label(d,(423,96),'P4 / 3/4 / material',20,True)
    p=fitted(renders,'three-quarter',(425,395))
    canvas.paste(p,(417+(425-p.width)//2,130))
    for name,x,title in [('clay',867,'P4 / clay'),('three-quarter',1162,'P4 / material')]:
        label(d,(x,96),title,20,True)
        p=fitted(renders,name,(280,335))
        canvas.paste(p,(x+(280-p.width)//2,145))
    label(d,(1480,96),'Одинаковая камера',19,True)
    label(d,(1480,123),'Только замена materials',18)
    for i,yaw in enumerate([45]+worst):
        label(d,(1470,164+i*105),f'{yaw}° / обзор, уменьшен до 82 px',16,True)
        for j,h in enumerate([110,135,160]):
            p=tile(renders,f'game-{yaw:03}',h)
            # Larger tiles are below; compact thumbnails here only identify directions.
            p.thumbnail((82,82))
            canvas.paste(p,(1480+j*135,189+i*105))
    d.line((28,533,1892,533),fill=(148,166,181))
    label(d,(28,545),'8 НАПРАВЛЕНИЙ / ЦВЕТ / 135 px высота / силуэты на sheet-p4-directions.png',22,True)
    for i,yaw in enumerate(range(0,360,45)):
        x=25+i*236
        color=tile(renders,f'game-{yaw:03}',135)
        canvas.paste(color,(x+(220-color.width)//2,589))
        label(d,(x+82,730),f'{yaw}°',18,True)
    label(d,(28,776),'Диагональ и два слабых направления / реальные контрольные размеры / цвет',22,True)
    for i,yaw in enumerate([45]+worst):
        x=20+i*633
        for j,h in enumerate([110,135,160]):
            p=tile(renders,f'game-{yaw:03}',h)
            canvas.paste(p,(x+j*206+(196-p.width)//2,1008-h))
            label(d,(x+j*206+45,1018),f'{yaw}° / {h} px',17)
    save(canvas,OUT/'sheet-p4.png')
    directions=Image.new('RGB',(1920,650),BG)
    dd=ImageDraw.Draw(directions)
    label(dd,(28,18),'P4 / 8 НАПРАВЛЕНИЙ / ЦВЕТ + ЧЕРНЫЙ СИЛУЭТ / 135 px',30,True)
    for i,yaw in enumerate(range(0,360,45)):
        x=25+i*236
        for black,y in [(False,112),(True,340)]:
            t=tile(renders,f'game-{yaw:03}',135,black)
            directions.paste(t,(x+(220-t.width)//2,y))
        label(dd,(x+83,264),f'{yaw}°',20,True)
        label(dd,(x+60,492),f'sword {metrics["directions"][str(yaw)]["visible_sword_pixels"]} px',15)
    label(dd,(28,570),'Размеры нормированы по bbox снаряжения. Камера 56° / FOV 45 / distance 19, вращается HB_Root.',20)
    save(directions,OUT/'sheet-p4-directions.png')
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    label(d,(28,18),'P3 CLAY / P4 MATERIAL / одна камера, свет и масштаб',30,True)
    label(d,(28,61),'Orthographic 3.10 m / display / warm daylight + cool ambient / ACES 2.0. P4 отличается также формой.',20)
    for folder,x,title in [(p3,25,'P3 / clay'),(renders,985,'P4 / material')]:
        label(d,(x+25,99),title,24,True)
        im=Image.open(folder/'three-quarter.png').convert('RGB').resize((650,650),Image.Resampling.LANCZOS)
        canvas.paste(im,(x+120,145))
        for j,h in enumerate([110,135,160]):
            im=tile(folder,'game-045',h)
            canvas.paste(im,(x+135+j*240,1010-h))
            label(d,(x+135+j*240,1020),str(h)+' px',16)
    save(canvas,OUT/'sheet-p4-vs-p3.png')
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    label(d,(28,18),'P4 / BEAUTY / BACK 3/4 / SIDE / GRIP И СЛОИ',30,True)
    for i,name in enumerate(['three-quarter','back-three-quarter','side']):
        im=fitted(renders,name,(620,770))
        canvas.paste(im,(10+i*635+(620-im.width)//2,125))
        label(d,(30+i*635,92),name,21,True)
    save(canvas,OUT/'sheet-p4-details.png')
    mock_game(renders,metrics)
    print('COMPOSED',metrics['p4_3quarter'],'worst',worst,'body native',metrics['body_game_045']['height'])


def mock_game(renders,metrics):
    path=ROOT/'.local/codex-tasks/ow2-arena/shots/final-arena-1920x1080.png'
    im=Image.open(path).convert('RGB')
    mask=alpha(renders,'game-045')
    b=mask.getbbox()
    hero=Image.open(renders/'game-045.png').convert('RGB').crop(b)
    cut=mask.crop(b)
    pos=(570,610-hero.height)
    d=ImageDraw.Draw(im)
    cx,cy=pos[0]+960-b[0],pos[1]+540-b[1]
    d.ellipse((cx-29,cy-16,cx+29,cy+16),outline=(86,229,255),width=2)
    im.paste(hero,pos,cut)
    d=ImageDraw.Draw(im)
    d.rectangle((16,16,1904,95),fill=BG)
    label(d,(28,25),'MOCK-UP / Blender P4 поверх сохраненного кадра арены / НЕ Unity screenshot P4',26,True)
    label(d,(28,61),f'Камера 56° / FOV 45 / 19 m / без resize: тело {metrics["body_game_045"]["height"]} px, со снаряжением {hero.height} px. Исходный герой справа сохранен.',18)
    d.rectangle((24,946,1896,1058),fill=BG)
    label(d,(40,960),'127 px class: показана реальная проекция этой модели при заданной камере, без принудительного уменьшения.',21,True)
    label(d,(40,996),'Источник: final-arena-1920x1080.png. Кадр содержит прежнего героя. Тень и кольцо в mock условные; экспорт не выполнялся.',19)
    save(im,OUT/'sheet-p4-gameview.png')
    (OUT/'mock-gameview.json').write_text(json.dumps({'source':str(path.relative_to(ROOT)),
        'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'source_contains_old_hero':True,
        'placement':pos,'equipment_bbox':b,'native_equipment_size':hero.size,'native_body_height':metrics['body_game_045']['height'],
        'resized':False,'label':'MOCK-UP','not_unity_p4_screenshot':True,
        'limitations':'No engine integration, shadow and selection ring approximations, saved frame has original hero.'},indent=2)+'\n',encoding='utf-8')


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--cycles',action='store_true')
    p.add_argument('--inventory',action='store_true')
    a=p.parse_args()
    if a.inventory:
        screenshot_inventory()
    elif a.cycles:
        contact(LOCAL/'cycles-3/r1',LOCAL/'cycles-3/r2',1)
        contact(LOCAL/'cycles-3/r2',LOCAL/'cycles-3/r3',2)
    else:
        compose()


if __name__=='__main__':
    main()

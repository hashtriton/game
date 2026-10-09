"""P3 decision sheets, eight facing masks, and auditable silhouette metrics."""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy import ndimage

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'
SCRATCH=ROOT/'.local/codex-tasks/hero-b'
BG=(223,229,233)
INK=(31,47,60)


def font(size,bold=False):
    return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),size)


def label(draw,xy,text,size=20,bold=False):
    draw.text(xy,text,font=font(size,bold),fill=INK)


def save(im,path):
    path.parent.mkdir(parents=True,exist_ok=True)
    im.save(path,optimize=True)
    if path.stat().st_size>=8*1024*1024:
        raise ValueError('PNG over 8 MiB: '+str(path))


def alpha(directory,name):
    return Image.open(directory/(name+'-mask.png')).getchannel('A').point(lambda v:255 if v>=128 else 0)


def silhouette_numbers(mask):
    box=mask.getbbox()
    cropped=mask.crop(box)
    a=np.array(cropped)>=128
    return {'bbox':list(box),'width':cropped.width,'height':cropped.height,
            'aspect_ratio':cropped.width/cropped.height,'fill_ratio':int(a.sum())/a.size,
            'area_pixels':int(a.sum())}


def tiles(directory,name,height):
    mask=alpha(directory,name)
    box=mask.getbbox()
    color=Image.open(directory/(name+'.png')).convert('RGB').crop(box)
    m=mask.crop(box)
    width=round(m.width*height/m.height)
    m=m.resize((width,height),Image.Resampling.LANCZOS)
    color=color.resize((width,height),Image.Resampling.LANCZOS)
    c=Image.new('RGB',(width,height),BG)
    c.paste(color,(0,0),m)
    black=Image.new('RGB',(width,height),BG)
    black.paste((0,0,0),(0,0,width,height),m)
    return c,black


def crop_fitted(directory,name,size):
    mask=alpha(directory,name)
    b=mask.getbbox()
    color=Image.open(directory/(name+'.png')).convert('RGB')
    im=color.crop((b[0]-10,b[1]-10,b[2]+10,b[3]+10))
    im.thumbnail(size,Image.Resampling.LANCZOS)
    return im


def concept_mask():
    path=ROOT/'art/concepts/2026-10-09-hero-b-breakwater-source-v1.png'
    color=Image.open(path).convert('RGB')
    data=np.asarray(color).astype(float)
    h,w=data.shape[:2]
    y,x=np.mgrid[:h,:w]
    sample=np.zeros((h,w),dtype=bool)
    for x0,y0,x1,y1 in [(45,180,175,320),(45,70,250,96),(696,120,745,590),(100,535,190,565),(55,340,155,365)]:
        sample[y0:y1,x0:x1]=True
    design=np.stack([np.ones_like(x),x/w,y/h],axis=2)
    coeff=np.linalg.lstsq(design[sample],data[sample],rcond=None)[0]
    distance=np.linalg.norm(data-design@coeff,axis=2)
    roi=(x>=45)&(x<=680)&(y>=65)&(y<=629)&~((x<236)&(y<170))
    roi &= ~((y<108)&((x<360)|(x>455)))
    roi &= ~((y>555)&(x>358)&(x<492))
    roi &= ~((y>585)&(x<220))
    roi &= ~((y>615)&((x<490)|(x>589)))
    masks={}
    for threshold in [28,32,36]:
        binary=(distance>threshold)&roi
        binary=ndimage.binary_closing(binary,iterations=2)
        labels,count=ndimage.label(binary)
        sizes=np.bincount(labels.ravel())
        keep=np.flatnonzero(sizes>=1200)
        keep=keep[keep!=0]
        binary=np.isin(labels,keep)&roi
        binary=ndimage.binary_fill_holes(binary)
        masks[threshold]=Image.fromarray((binary*255).astype('uint8'))
    final=masks[32]
    save(final,ART/'concept-front-threshold-mask.png')
    overlay=color.copy()
    red=Image.new('RGB',color.size,(235,65,70))
    overlay.paste(red,(0,0),final.point(lambda v:int(v*.35)))
    save(overlay,ART/'concept-front-threshold-overlay.png')
    meta={'source':str(path.relative_to(ROOT)),'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
          'method':'RGB Euclidean distance from least-squares spatial plane fitted to five fixed plain-background patches; threshold 32/255; landmark ROI excludes title/rule above helmet and floor shadow outside boots below 555/585/615 px; closing radius 2 pixels; components at least 1200 pixels; enclosed holes filled',
          'limitations':'AI perspective; manual background/ROI selection; shadow contamination and armor similar to background; sensitivity 28/32/36 shown, not a 3D measurement',
          'background_patches':[[45,180,175,320],[45,70,250,96],[696,120,745,590],[100,535,190,565],[55,340,155,365]],
          'sensitivity':{str(t):silhouette_numbers(m) for t,m in masks.items()}}
    return color,final,meta


def compose(p3,p2):
    concept,cmask,cmeta=concept_mask()
    metrics={'concept_3quarter':silhouette_numbers(cmask),'concept_extraction':cmeta,
             'p2_3quarter':silhouette_numbers(alpha(p2,'three-quarter')),
             'p3_3quarter':silhouette_numbers(alpha(p3,'three-quarter')),
             'directions':{}}
    for variant in ['p2','p3']:
        key=variant+'_3quarter'
        metrics[key]['relative_error_to_concept']={m:abs(metrics[key][m]/metrics['concept_3quarter'][m]-1)
                                                   for m in ['aspect_ratio','fill_ratio']}
        metrics[key]['within_12_percent']=all(v<=.12 for v in metrics[key]['relative_error_to_concept'].values())
    for yaw in range(0,360,45):
        name=f'game-{yaw:03}'
        metrics['directions'][str(yaw)]=silhouette_numbers(alpha(p3,name))
        for height in [110,135,160]:
            color,black=tiles(p3,name,height)
            save(color,p3/f'{name}-{height}-color.png')
            save(black,p3/f'{name}-{height}-silhouette.png')
    worst=[90,270]
    metrics['worst_directions_degrees']=worst
    metrics['worst_selection']='Side facings: shield is edge-on and sword overlaps the arm/body more than front diagonals; chosen after visual inspection, not the lowest fill-ratio heuristic'
    directions=Image.new('RGB',(1920,850),BG)
    d=ImageDraw.Draw(directions)
    label(d,(28,20),'P3 / 8 НАПРАВЛЕНИЙ / DISPLAY / 135 px',30,True)
    label(d,(28,58),'Фиксированная камера 56 / FOV 45 / distance 19. Поворачивается HB_Root.',20)
    for i,yaw in enumerate(range(0,360,45)):
        color,black=tiles(p3,f'game-{yaw:03}',135)
        x=20+i*235
        directions.paste(color,(x+(210-color.width)//2,112))
        directions.paste(black,(x+(210-black.width)//2,307))
        label(d,(x+78,258),f'{yaw}°',20,True)
    label(d,(28,470),'Два слабых боковых направления / 110 и 160 px / цвет и силуэт',23,True)
    for i,yaw in enumerate(worst):
        for j,height in enumerate([110,160]):
            color,black=tiles(p3,f'game-{yaw:03}',height)
            x=30+i*945+j*440
            directions.paste(color,(x+40,714-height))
            directions.paste(black,(x+245,714-height))
            label(d,(x+45,735),f'{yaw}° / {height} px',20)
    save(directions,ART/'sheet-p3-directions.png')
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    label(d,(28,17),'ВОЛНОЛОМ / P3 / ВТОРОЙ ПРОХОД КРУПНЫХ ФОРМ',30,True)
    label(d,(28,59),'Серая броня + cyan / display pose / Blender CLI / без текстур и рига',20)
    b=cmask.getbbox()
    ci=concept.crop((b[0]-8,b[1]-8,b[2]+8,b[3]+8))
    ci.thumbnail((450,435),Image.Resampling.LANCZOS)
    canvas.paste(ci,(22+(450-ci.width)//2,127))
    label(d,(28,96),'Концепт B (AI)',20,True)
    for name,x,width,title in [('three-quarter',488,510,'P3 / 3/4'),('front',1020,275,'Спереди'),
                               ('side',1320,275,'Справа'),('back',1620,275,'Сзади')]:
        im=crop_fitted(p3,name,(width,435))
        canvas.paste(im,(x+(width-im.width)//2,127))
        label(d,(x,96),title,20,True)
    d.line((28,573,1892,573),fill=(150,170,184),width=1)
    label(d,(28,584),'8 НАПРАВЛЕНИЙ / ЧЕРНЫЙ СИЛУЭТ / 135 px',22,True)
    for i,yaw in enumerate(range(0,360,45)):
        _,black=tiles(p3,f'game-{yaw:03}',135)
        x=25+i*235
        canvas.paste(black,(x+(210-black.width)//2,620))
        label(d,(x+76,761),f'{yaw}°',18)
    label(d,(28,805),'Передняя диагональ / цвет / 110, 135, 160 px',22,True)
    label(d,(982,805),'Та же диагональ / черный силуэт / 110, 135, 160 px',22,True)
    for i,height in enumerate([110,135,160]):
        color,black=tiles(p3,'game-045',height)
        for im,x in [(color,80+i*275),(black,1040+i*275)]:
            canvas.paste(im,(x+(205-im.width)//2,1012-height))
            label(d,(x+65,1025),str(height)+' px',18)
    save(canvas,ART/'sheet-p3.png')
    comparison=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(comparison)
    label(d,(28,20),'ВОЛНОЛОМ / P2 И P3 / ОДИН РАКУРС И МАСШТАБ',30,True)
    label(d,(28,65),'P2: исходная поза. P3: защитная стойка. Камера, свет и orthographic scale 3.10 м одинаковы.',20)
    for directory,x,variant in [(p2,30,'P2'),(p3,990,'P3')]:
        label(d,(x,105),variant,27,True)
        im=Image.open(directory/'three-quarter.png').convert('RGB')
        im=im.resize((430,430),Image.Resampling.LANCZOS)
        comparison.paste(im,(x,146))
        mask=alpha(directory,'three-quarter').resize((430,430),Image.Resampling.LANCZOS)
        black=Image.new('RGB',mask.size,BG)
        black.paste((0,0,0),(0,0,430,430),mask)
        comparison.paste(black,(x+475,146))
        label(d,(x,605),'Цвет / 110, 135, 160 px',21,True)
        label(d,(x,829),'Силуэт / 110, 135, 160 px',21,True)
        for i,height in enumerate([110,135,160]):
            c,b=tiles(directory,'game-045',height)
            xx=x+60+i*275
            comparison.paste(c,(xx,803-height))
            comparison.paste(b,(xx,1025-height))
            label(d,(xx,808),f'{height} px',18)
    save(comparison,ART/'sheet-p2-vs-p3.png')
    (ART/'silhouette-metrics-p3.json').write_text(json.dumps(metrics,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in metrics.items() if k.endswith('3quarter')},indent=2))


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--p3',type=Path,default=ART/'renders/p3')
    p.add_argument('--p2',type=Path,default=SCRATCH/'evaluation/p2')
    opt=p.parse_args()
    compose(opt.p3,opt.p2)


if __name__=='__main__':
    main()

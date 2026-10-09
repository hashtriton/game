"""Measured contact sheets and honest native-scale arena mock."""
import hashlib
import json
from pathlib import Path
import numpy as np
from scipy import ndimage
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/creatures/cinder-runner'
TASK=ROOT/'.local/codex-tasks/creeps'
R=OUT/'renders'
BG=(218,228,235)
INK=(25,42,57)

def text(im,xy,value,size=20):
    ImageDraw.Draw(im).text(xy,value,font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',size),fill=INK)

def save(im,path):
    path.parent.mkdir(parents=True,exist_ok=True)
    im.save(path,optimize=True)
    if path.stat().st_size>=8*1024**2:
        raise ValueError('PNG >8 MiB: '+str(path))

def mask(folder,name):
    return Image.open(folder/(name+'-mask.png')).getchannel('A').point(lambda p:255 if p>=128 else 0)

def numbers(m):
    b=m.getbbox()
    a=np.asarray(m.crop(b))>=128
    return {'bbox':list(b),'width':b[2]-b[0],'height':b[3]-b[1],'aspect':(b[2]-b[0])/(b[3]-b[1]),'fill':float(a.sum()/a.size),'area':int(a.sum())}

def tile(folder,name,height,black=False):
    m=mask(folder,name)
    b=m.getbbox()
    w=round((b[2]-b[0])*height/(b[3]-b[1]))
    c=Image.new('RGB',(w,height),BG)
    mm=m.crop(b).resize((w,height),Image.Resampling.LANCZOS)
    pixels=(0,0,0) if black else Image.open(folder/(name+'.png')).convert('RGB').crop(b).resize((w,height),Image.Resampling.LANCZOS)
    c.paste(pixels,(0,0),mm)
    return c

def fitted(folder,name,size):
    m=mask(folder,name)
    b=m.getbbox()
    c=Image.new('RGB',m.size,BG)
    c.paste(Image.open(folder/(name+'.png')).convert('RGB'),(0,0),m)
    c=c.crop((max(0,b[0]-8),max(0,b[1]-8),b[2]+8,b[3]+8))
    c.thumbnail(size,Image.Resampling.LANCZOS)
    return c

def concept():
    path=ROOT/'art/concepts/2026-10-09-creeps-raiders-v1.png'
    im=Image.open(path).convert('RGB')
    data=np.asarray(im).astype(float)
    h,w=data.shape[:2]
    y,x=np.mgrid[:h,:w]
    patches=[(35,210,120,330),(465,210,490,520),(40,615,100,640),(340,635,455,661)]
    sample=np.zeros((h,w),bool)
    for x0,y0,x1,y1 in patches:
        sample[y0:y1,x0:x1]=True
    design=np.stack([np.ones_like(x),x/w,y/h],2)
    coeff=np.linalg.lstsq(design[sample],data[sample],rcond=None)[0]
    dist=np.linalg.norm(data-design@coeff,axis=2)
    roi=(x>=31)&(x<=480)&(y>=136)&(y<=662)
    roi &= ~((x<320)&(y<191))
    # Floor shadow is excluded by anatomical landmarks, not by selecting better metrics.
    roi &= ~((y>615)&(x<169))
    roi &= ~((y>548)&(x<222-.8*(y-550)))
    roi &= ~((y>612)&(x>280))
    sensitivity={}
    masks={}
    for threshold in [28,32,36]:
        binary=ndimage.binary_closing((dist>threshold)&roi,iterations=1)
        labels,n=ndimage.label(binary)
        counts=np.bincount(labels.ravel())
        keep=np.flatnonzero(counts>=1200)
        keep=keep[keep!=0]
        binary=ndimage.binary_fill_holes(np.isin(labels,keep))&roi
        m=Image.fromarray((binary*255).astype('uint8'))
        masks[threshold]=m
        sensitivity[str(threshold)]=numbers(m)
    m=masks[32]
    save(m,OUT/'concept-mask.png')
    overlay=im.copy()
    overlay.paste((245,60,60),(0,0,w,h),m.point(lambda p:round(p*.35)))
    save(overlay,OUT/'concept-mask-overlay.png')
    return im,m,{'source':str(path.relative_to(ROOT)),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'method':'RGB Euclidean distance from least-squares spatial background plane, threshold32, 1px closing, components>=1200, fill holes, explicit landmark ROI','patches':patches,'sensitivity':sensitivity,'limitations':'AI perspective, manually specified ROI/background, possible floor-shadow contamination; projection proxy, not runtime or 3D reconstruction'}

def contact(before,after,index):
    im=Image.new('RGB',(1600,950),BG)
    text(im,(25,18),f'R1 / цикл {index} / ДО -> ПОСЛЕ / одинаковые камеры',28)
    for col,(folder,title) in enumerate([(before,'ДО'),(after,'ПОСЛЕ')]):
        text(im,(30+col*800,65),title,24)
        for name,x,y,size in [('three-quarter',20,110,(410,610)),('side',435,110,(330,610))]:
            p=fitted(folder,name,size)
            im.paste(p,(col*800+x+(size[0]-p.width)//2,y))
        p=tile(folder,'game-045',135)
        im.paste(p,(col*800+280,770))
        text(im,(col*800+440,820),'45° / 135 px',20)
    save(im,TASK/f'cycles-cinder-runner/cycle{index}-before-after.png')

def compose():
    cim,cm,meta=concept()
    cr=numbers(cm)
    mr=numbers(mask(R,'three-quarter'))
    err={k:abs(mr[k]/cr[k]-1) for k in ['aspect','fill']}
    metrics={'concept':cr,'render_3quarter':mr,'relative_error':err,'within_12_percent':all(v<=.12 for v in err.values()),'concept_extraction':meta,'directions':{},'normalization':'Tiles scale full equipped tight bbox to 110/135/160 px; mock uses unresized native pixels.'}
    for yaw in range(0,360,45):
        name=f'game-{yaw:03}'
        m=mask(R,name)
        row=numbers(m)
        ids=np.asarray(Image.open(R/(name+'-weapon-id.png')).convert('RGB'))
        row['visible_weapon_pixels']=int(((ids[:,:,0]>150)&(ids[:,:,1]<70)).sum())
        row['weapon_fraction']=row['visible_weapon_pixels']/row['area']
        metrics['directions'][str(yaw)]=row
        for h in [110,135,160]:
            for black in [False,True]:
                save(tile(R,name,h,black),OUT/('crops/'+name+'-'+str(h)+'-'+('silhouette.png' if black else 'color.png')))
    worst=sorted(range(0,360,45),key=lambda y:metrics['directions'][str(y)]['weapon_fraction'])[:2]
    metrics['worst_directions']=worst
    metrics['worst_selection']='Two lowest visible weapon pixel fractions in occluded object-ID renders; all eight also shown for visual review.'
    im=Image.new('RGB',(1920,1340),BG)
    text(im,(28,18),'R1 / УГОЛЬНЫЙ ГОНЕЦ / BLENDER BLOCKOUT + MATERIAL LOOK TEST',29)
    text(im,(28,59),'Display pose / Cycles OptiX48 / ACES2.0 / оригинальная геометрия / без рига и UV',19)
    p=cim.crop((30,135,480,668));p.thumbnail((415,445))
    im.paste(p,(30,110));text(im,(28,85),'AI концепт R1',20)
    for name,x,title in [('three-quarter',490,'3/4 / материалы'),('clay',965,'Та же геометрия / clay'),('side',1450,'Профиль / материалы')]:
        p=fitted(R,name,(430,450));im.paste(p,(x+(430-p.width)//2,110));text(im,(x,85),title,20)
    text(im,(28,570),'8 НАПРАВЛЕНИЙ / 135 px / цвет и черный силуэт',23)
    for i,yaw in enumerate(range(0,360,45)):
        x=20+i*237
        for black,y in [(False,615),(True,785)]:
            p=tile(R,f'game-{yaw:03}',135,black);im.paste(p,(x+(220-p.width)//2,y))
        text(im,(x+80,752),str(yaw)+'°',17)
    text(im,(28,930),f'Худшие по видимости оружия: {worst} / контроль 110, 135, 160 px на sheet-worst.png',20)
    text(im,(28,967),f'Concept aspect/fill {cr["aspect"]:.3f}/{cr["fill"]:.3f}; модель {mr["aspect"]:.3f}/{mr["fill"]:.3f}; ошибки {err["aspect"]:.1%}/{err["fill"]:.1%}',20)
    text(im,(28,1003),'Числа - сравнение проекций с AI-изображением. Native scale отдельно в MOCK-UP.',20)
    for i,yaw in enumerate(worst):
        for j,h in enumerate([110,135,160]):
            x=40+i*950+j*295
            p=tile(R,f'game-{yaw:03}',h)
            im.paste(p,(x+(260-p.width)//2,1280-h))
            text(im,(x+70,1290),f'{yaw}° / {h} px',18)
    save(im,OUT/'sheet.png')
    im=Image.new('RGB',(1600,520),BG)
    text(im,(25,20),'R1 / ДВА СЛАБЫХ НАПРАВЛЕНИЯ / РЕАЛЬНЫЕ CROP РАЗМЕРЫ',25)
    for i,yaw in enumerate(worst):
        for j,h in enumerate([110,135,160]):
            x=30+i*790+j*250
            for black,y in [(False,240-h),(True,450-h)]:
                p=tile(R,f'game-{yaw:03}',h,black);im.paste(p,(x+(220-p.width)//2,y))
            text(im,(x+50,465),f'{yaw}° / {h}px',18)
    save(im,OUT/'sheet-worst.png')
    im=Image.new('RGB',(1920,780),BG)
    text(im,(25,20),'R1 / BEAUTY + BACK + SIDE / МАТЕРИАЛЫ И ТОЛЩИНА',27)
    for i,name in enumerate(['three-quarter','back','side']):
        p=fitted(R,name,(615,675));im.paste(p,(i*640+(640-p.width)//2,75));text(im,(i*640+30,52),name,20)
    save(im,OUT/'sheet-materials.png')
    im=Image.new('RGB',(1280,770),BG)
    text(im,(25,20),'R1 / CLAY - MATERIAL / одна геометрия, камера и свет',25)
    for i,name in enumerate(['clay','three-quarter']):
        p=fitted(R,name,(615,660));im.paste(p,(i*640+(640-p.width)//2,80))
    save(im,OUT/'sheet-clay-material.png')
    frame=ROOT/'.local/codex-tasks/ow2-arena/shots/final-arena-1920x1080.png'
    im=Image.open(frame).convert('RGB')
    placements=[]
    for folder,name,anchor,title in [(ROOT/'art/heroes/breakwater/p4/renders','game-045',(590,640),'Герой P4'),(R,'game-045',(820,635),'R1'),(R,'game-225',(1180,715),'R1 225°')]:
        mm=mask(folder,name);b=mm.getbbox();c=Image.open(folder/(name+'.png')).convert('RGB').crop(b)
        pos=(anchor[0]-c.width//2,anchor[1]-c.height)
        im.paste(c,pos,mm.crop(b))
        d=ImageDraw.Draw(im)
        d.rectangle((anchor[0]-27,pos[1]-13,anchor[0]+27,pos[1]-8),fill=(190,36,30) if title.startswith('R1') else (45,180,211))
        text(im,(pos[0],anchor[1]+9),title,17)
        placements.append({'label':title,'source':str(folder.relative_to(ROOT)/ (name+'.png')),'bbox':b,'placement':pos,'native_size':c.size,'resized':False})
    d=ImageDraw.Draw(im);d.rectangle((14,14,1906,96),fill=BG);d.rectangle((14,966,1906,1066),fill=BG)
    text(im,(28,23),'MOCK-UP / R1 И ГЕРОЙ P4 НА РЕАЛЬНОМ КАДРЕ АРЕНЫ / НЕ UNITY SCREENSHOT',27)
    text(im,(28,63),'Без resize / pitch56 / FOV45 / distance19 / красные health bars R1 условные',20)
    text(im,(28,982),'В кадре сохранен прежний герой справа. Освещение и тени композита не являются интеграцией.',20)
    text(im,(28,1017),'Блок-ауты без rig/animation; ни runtime, ни коллизии этим mock не проверены.',20)
    save(im,OUT/'sheet-gameview.png')
    (OUT/'mock-gameview.json').write_text(json.dumps({'source':str(frame.relative_to(ROOT)),'source_sha256':hashlib.sha256(frame.read_bytes()).hexdigest(),'source_contains_old_hero':True,'label':'MOCK-UP','not_unity_screenshot':True,'placements':placements},indent=2)+'\n',encoding='utf-8')
    (OUT/'silhouette-metrics.json').write_text(json.dumps(metrics,indent=2)+'\n',encoding='utf-8')
    print('R1 COMPOSE',err,'worst',worst)

if __name__=='__main__':
    import argparse
    p=argparse.ArgumentParser();p.add_argument('--contact',nargs=3)
    a=p.parse_args()
    if a.contact:
        contact(Path(a.contact[0]),Path(a.contact[1]),a.contact[2])
    else:
        compose()

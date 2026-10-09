"""Compose honest native crops, form cycles and mask-derived area measurements."""
import json
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2]; OUT=ROOT/'art/heroes/breakwater/p5'; TASK=ROOT/'.local/codex-tasks/hero-p5'; BG='#d6e0e6'
FONT='C:/Windows/Fonts/arial.ttf'
def label(im,xy,text,size=18): ImageDraw.Draw(im).text(xy,text,font=ImageFont.truetype(FONT,size),fill='#172d3c')
def save(im,path):
    path.parent.mkdir(parents=True,exist_ok=True); im.save(path,optimize=True)
    assert path.stat().st_size<8*1024*1024,path
def tile(folder,name,height=None,black=False):
    mask=Image.open(folder/(name+'-mask.png')).getchannel('A'); box=mask.getbbox(); mask=mask.crop(box)
    image=Image.new('RGB',mask.size,BG); color=Image.open(folder/(name+'.png')).convert('RGB').crop(box)
    image.paste((0,0,0) if black else color,(0,0,mask.width,mask.height) if black else (0,0),mask)
    if height: image=image.resize((round(image.width*height/image.height),height),Image.Resampling.LANCZOS)
    return image
def metrics(folder,yaws=range(0,360,45)):
    result={}
    colors=np.array([[255,0,0],[0,255,0],[0,0,255],[255,255,0],[0,255,255],[255,0,255]])
    keys=['white','navy','steel','dark','cyan','bronze']
    for yaw in yaws:
        name=f'game-{yaw:03}'; a=np.array(Image.open(folder/(name+'-id.png')).convert('RGBA')); active=a[:,:,3]>=128; pixels=a[:,:,:3][active].astype(float); classes=((pixels[:,None,:]-colors[None,:,:])**2).sum(axis=2).argmin(axis=1)
        counts={key:int((classes==i).sum()) for i,key in enumerate(keys)}
        counts['visible_total']=int(active.sum()); fractions={key:round(value/active.sum()*100,3) for key,value in counts.items() if key!='visible_total'}
        greens=[]
        for suffix in ['leg-visible','leg-no-shield']:
            arr=np.array(Image.open(folder/(name+'-'+suffix+'.png')).convert('RGBA')); greens.append(int(((arr[:,:,1]>128)&(arr[:,:,0]<64)&(arr[:,:,3]>=128)).sum()))
        result[str(yaw)]={'pixels':counts,'percent':fractions,'left_leg_visible':greens[0],'left_leg_without_shield':greens[1],'shield_left_leg_cover_percent':round(max(0,greens[1]-greens[0])/max(1,greens[1])*100,3)}
    return result
def cycles():
    v1=TASK/'cycles/v1/renders'; v2=TASK/'cycles/v2/renders'; p4=ROOT/'art/heroes/breakwater/p4/renders'
    for i,(before,after) in enumerate([(p4,v1),(v1,v2)],1):
        canvas=Image.new('RGB',(1800,900),BG)
        label(canvas,(20,12),f'P5 / цикл {i} / до и после / одинаковая камера',28)
        for j,(folder,title) in enumerate([(before,'До'),(after,'После')]):
            label(canvas,(20+j*900,54),title+' / '+str(folder.parent.name),22)
            for k,name in enumerate(['three-quarter','back-three-quarter']): canvas.paste(Image.open(folder/(name+'.png')).convert('RGB').resize((430,430)),(15+j*900+k*445,100))
            for k,h in enumerate([110,135,160]):
                t=tile(folder,'game-045',h); canvas.paste(t,(80+j*900+k*275,770-h)); label(canvas,(80+j*900+k*275,785),f'{h} px')
        label(canvas,(20,852),'Look renders Blender, display. Это сравнение формы, игровые кадры отдельно.',20)
        save(canvas,TASK/f'cycles/cycle{i}-before-after.png')
def compose():
    renders=OUT/'renders'; old=ROOT/'art/heroes/breakwater/p4/renders'; canvas=Image.new('RGB',(1920,1080),BG)
    label(canvas,(25,18),'Волнолом P5 / отдельная проба / Blender',30)
    concept=Image.open(ROOT/'art/concepts/2026-10-09-hero-b-breakwater-v1.png').convert('RGB').crop((84,136,841,735)); concept.thumbnail((430,470)); canvas.paste(concept,(20,90))
    for x,name in [(480,'three-quarter'),(965,'back-three-quarter'),(1450,'side')]:
        t=tile(renders,name,440); t.thumbnail((450,440)); canvas.paste(t,(x,90)); label(canvas,(x,550),name)
    label(canvas,(25,603),'8 направлений / native projected pixels / камера 56, FOV 45, distance 19',22)
    for i,yaw in enumerate(range(0,360,45)):
        name=f'game-{yaw:03}'; t=tile(renders,name); canvas.paste(t,(25+i*236+(205-t.width)//2,690)); label(canvas,(85+i*236,920),str(yaw)+' deg')
    save(canvas,OUT/'sheet-p5.png')
    strip=Image.new('RGB',(1920,620),BG); label(strip,(25,18),'P5 / 8 направлений / цвет + черный силуэт / native',28)
    for i,yaw in enumerate(range(0,360,45)):
        name=f'game-{yaw:03}'
        for black,y in [(False,100),(True,360)]:
            t=tile(renders,name,black=black); strip.paste(t,(25+i*236+(205-t.width)//2,y))
        label(strip,(85+i*236,280),str(yaw)+' deg')
    save(strip,OUT/'sheet-p5-directions.png')
    pair=Image.new('RGB',(1920,1120),BG); label(pair,(25,18),'P4 / P5 / та же камера и display pose',30)
    for j,folder in enumerate([old,renders]):
        label(pair,(25+j*960,62),'P4' if j==0 else 'P5',24)
        for k,name in enumerate(['three-quarter','back-three-quarter']): pair.paste(Image.open(folder/(name+'.png')).convert('RGB').resize((465,465)),(10+j*960+k*475,100))
        for i,yaw in enumerate([0,45,135,225]):
            for black,y in [(False,640),(True,880)]:
                t=tile(folder,f'game-{yaw:03}',135,black); pair.paste(t,(20+j*960+i*235+(210-t.width)//2,y))
            label(pair,(90+j*960+i*235,1030),str(yaw)+' deg')
    save(pair,OUT/'sheet-p5-vs-p4.png')
    old_metrics=json.loads((ROOT/'art/heroes/breakwater/silhouette-metrics-p3.json').read_text()); mask=Image.open(renders/'three-quarter-mask.png').getchannel('A'); b=mask.getbbox(); ar=np.array(mask.crop(b))>=128; numbers=dict(aspect_ratio=ar.shape[1]/ar.shape[0],fill_ratio=float(ar.mean()),native_bbox=b)
    numbers['relative_error_to_concept']={k:abs(numbers[k]/old_metrics['concept_3quarter'][k]-1) for k in ['aspect_ratio','fill_ratio']}; numbers['within_12_percent']=all(v<=.12 for v in numbers['relative_error_to_concept'].values()); (OUT/'silhouette-metrics-p5.json').write_text(json.dumps(numbers,indent=2),encoding='utf-8')
    unity=json.loads((TASK/'unity-area-metrics.json').read_text())
    results={'method':'Blender flat material ID, alpha>=128, nearest RGB category, all visible geometry including equipment. Display and source Idle frame1 per8directions. Actual arena data is authoritative Unity-ID: same ArenaActor, camera and frozen Idle phase0.01. FBX axis transform measured as Blender(x,y,z)->Unity(-x,z,-y), so hero yaw145.196 corresponds to Blender camera-relative34.804. The old325.201 conversion was mirrored and rejected.','P4_display':metrics(TASK/'p4-masks'),'P5_display':metrics(renders),'P4_Idle':metrics(TASK/'p4-idle-masks'),'P5_Idle':metrics(OUT/'idle-masks'),'P4_UnityIdle':unity['P4'],'P5_UnityIdle':unity['P5'],'native_ID_method':unity['method']}
    (OUT/'area-metrics-p5.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
if __name__=='__main__':
    import sys
    cycles() if '--cycles' in sys.argv else compose()

"""Compose inspection renders at their labelled pixel sizes with Pillow."""
import argparse
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'art/heroes/breakwater'
BG=(223,229,233)
INK=(31,47,60)
def font(size,bold=False):
    return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if bold else 'arial.ttf'),size)
def fit(image,size):
    result=image.copy()
    result.thumbnail(size,Image.Resampling.LANCZOS)
    return result
def caption(draw,xy,label,size=20,bold=False):
    draw.text(xy,label,font=font(size,bold),fill=INK)
def png(image,path):
    image.save(path,optimize=True)
    if path.stat().st_size>=8*1024*1024:
        raise ValueError('PNG exceeds 8 MB: '+str(path))
def crops(variant):
    directory=ART/'renders'/variant
    metrics={}
    for view in ['game','game-front']:
        color=Image.open(directory/f'{view}.png').convert('RGB')
        mask=Image.open(directory/f'{view}-mask.png').getchannel('A')
        threshold=mask.point(lambda v:255 if v>=128 else 0)
        bbox=threshold.getbbox()
        if not bbox:
            raise ValueError('Empty game silhouette')
        x0,y0,x1,y1=bbox
        native=color.crop((x0-6,y0-6,x1+6,y1+6))
        png(native,directory/f'{view}-true-crop.png')
        fullcolor=color.crop(bbox)
        fullmask=mask.crop(bbox)
        fullblack=Image.new('RGB',fullmask.size,(0,0,0))
        metrics[view]={'bbox_xyxy':bbox,'actual_silhouette_height_px':y1-y0,
                       'actual_silhouette_width_px':x1-x0,'true_crop_size':native.size,
                       'normalization':'Target height is full geometric silhouette, not helmet-to-sole vertical segment'}
        for height in [110,135,160]:
            width=round(fullmask.width*height/fullmask.height)
            c=fullcolor.resize((width,height),Image.Resampling.LANCZOS)
            m=fullmask.resize((width,height),Image.Resampling.LANCZOS)
            black=Image.new('RGB',(width,height),BG)
            black.paste((0,0,0),(0,0,width,height),m)
            png(c,directory/f'{view}-{height}-color.png')
            png(black,directory/f'{view}-{height}-silhouette.png')
    (directory/'render-metrics.json').write_text(json.dumps(metrics,indent=2)+'\n',encoding='utf-8')
    return metrics

def sheet(variant):
    metrics=crops(variant)
    directory=ART/'renders'/variant
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    caption(d,(30,20),f'B / ВОЛНОЛОМ  |  {variant.upper()}  |  CLAY BLOCKOUT',30,True)
    caption(d,(30,62),'2.4 м  |  A-pose для оценки формы  |  без рига и текстур  |  смотреть при 100%',20)
    concept=Image.open(ROOT/'art/concepts/2026-10-09-hero-b-breakwater-source-v1.png').crop((30,64,704,640)).convert('RGB')
    tiles=[('Концепт B (AI)',concept,(25,105),(480,430)),
           ('Реальный Blender 3/4',Image.open(directory/'three-quarter.png'),(520,105),(480,430)),
           ('Спереди',Image.open(directory/'front.png'),(1025,105),(285,430)),
           ('Справа',Image.open(directory/'side.png'),(1320,105),(285,430)),
           ('Сзади',Image.open(directory/'back.png'),(1620,105),(280,430))]
    for title,im,xy,space in tiles:
        caption(d,(xy[0],xy[1]),title,20,True)
        thumb=fit(im.convert('RGB'),space)
        canvas.paste(thumb,(xy[0]+(space[0]-thumb.width)//2,xy[1]+34))
    d.line((30,584,1890,584),fill=(154,170,183),width=1)
    caption(d,(30,603),'Игровая камера 56° / FOV 45° / distance 19 / 1920x1080',23,True)
    caption(d,(30,635),'Передний ракурс = герой повернут к камере. Черный силуэт получен из геометрии.',18)
    native=Image.open(directory/'game-front-true-crop.png')
    canvas.paste(native,(52,730))
    real=metrics['game-front']['actual_silhouette_height_px']
    caption(d,(35,970),f'Без resize: {real} px',18)
    for h,x in [(110,340),(135,570),(160,820)]:
        im=Image.open(directory/f'game-front-{h}-color.png')
        canvas.paste(im,(x+(180-im.width)//2,926-im.height))
        caption(d,(x+42,948),f'{h} px',20)
    for h,x in [(110,1110),(135,1360),(160,1620)]:
        im=Image.open(directory/f'game-front-{h}-silhouette.png')
        canvas.paste(im,(x+(180-im.width)//2,926-im.height))
        caption(d,(x+42,948),f'{h} px',20)
    caption(d,(330,688),'CLAY + CYAN, нормализованная высота',18,True)
    caption(d,(1110,688),'ЧЕРНЫЙ СИЛУЭТ, та же высота',18,True)
    caption(d,(30,1022),'110 / 135 / 160 px - сравнительные размеры. Фактический кадр и все 1280px виды сохранены отдельно.',18)
    png(canvas,ART/f'sheet-{variant}.png')

def comparison():
    canvas=Image.new('RGB',(1920,1080),BG)
    d=ImageDraw.Draw(canvas)
    caption(d,(30,20),'ВОЛНОЛОМ / P1 И P2 / СРАВНЕНИЕ ФОРМЫ',30,True)
    caption(d,(30,65),'P2: плечи -8%, ноги +6%, шлем +5%, корпус менее объемный. Высота обоих 2.4 м.',21)
    for variant,x in [('p1',30),('p2',990)]:
        directory=ART/'renders'/variant
        caption(d,(x,105),variant.upper(),27,True)
        beauty=fit(Image.open(directory/'three-quarter.png').convert('RGB'),(530,405))
        canvas.paste(beauty,(x,150))
        front=fit(Image.open(directory/'front.png').convert('RGB'),(360,405))
        canvas.paste(front,(x+550,150))
        caption(d,(x,600),'Цвет / 110, 135, 160 px',21,True)
        caption(d,(x,824),'Силуэт / 110, 135, 160 px',21,True)
        for h,dx in [(110,90),(135,340),(160,615)]:
            for kind,y in [('color',796),('silhouette',1020)]:
                im=Image.open(directory/f'game-front-{h}-{kind}.png')
                canvas.paste(im,(x+dx,y-im.height))
            caption(d,(x+dx,800),f'{h}',17)
    png(canvas,ART/'sheet-compare.png')

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--variant',choices=['p1','p2','all'],default='all')
    args=parser.parse_args()
    for variant in (['p1','p2'] if args.variant=='all' else [args.variant]):
        sheet(variant)
    if args.variant=='all':
        comparison()
    print('Sheets composed with exact normalized crop heights')

if __name__=='__main__':
    main()

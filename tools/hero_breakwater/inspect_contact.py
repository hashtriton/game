"""Small visual inspection sheets preserve the same crops before and after."""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont


def main():
    p=argparse.ArgumentParser()
    p.add_argument('--before',type=Path,required=True)
    p.add_argument('--after',type=Path)
    p.add_argument('--output',type=Path,required=True)
    args=p.parse_args()
    canvas=Image.new('RGB',(1600,520 if not args.after else 1040),(223,229,233))
    draw=ImageDraw.Draw(canvas)
    font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',20)
    for row,directory in enumerate([args.before]+([args.after] if args.after else [])):
        for col,name in enumerate(['three-quarter','side','back','game-045']):
            mask=Image.open(directory/(name+'-mask.png')).getchannel('A').point(lambda p:255 if p>=128 else 0)
            bbox=mask.getbbox()
            color=Image.open(directory/(name+'.png')).convert('RGB')
            box=(bbox[0]-12,bbox[1]-12,bbox[2]+12,bbox[3]+12)
            im=color.crop(box)
            im.thumbnail((380,470),Image.Resampling.LANCZOS)
            canvas.paste(im,(col*400+(400-im.width)//2,row*520+40))
            draw.text((col*400+12,row*520+10),('BEFORE' if row==0 else 'AFTER')+' / '+name,font=font,fill=(31,47,60))
    args.output.parent.mkdir(parents=True,exist_ok=True)
    canvas.save(args.output,optimize=True)


if __name__=='__main__':
    main()

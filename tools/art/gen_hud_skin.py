"""Original floating HUD geometry. Glyph alpha is derived from image_gen luminance."""
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'unity/Assets/Game/Art/Ui/Hud/Resources/Sprites'
ORIGINAL = ROOT / 'art/ui/hud'
OUT.mkdir(parents=True, exist_ok=True)
ORIGINAL.mkdir(parents=True, exist_ok=True)
S = 4

def canvas(w, h):
    im = Image.new('RGBA', (w*S, h*S))
    return im, ImageDraw.Draw(im)

def save(im, name, w, h):
    im = im.resize((w,h), Image.Resampling.LANCZOS)
    im.save(OUT/(name+'.png'), optimize=True)
    im.save(ORIGINAL/(name+'.png'), optimize=True)

def polygon(name,w,h,fill,outline=None):
    im,d=canvas(w,h)
    pts=[(w*.17,2),(w-8,2),(w-2,8),(w*.83,h-2),(8,h-2),(2,h-8)]
    pts=[(int(x*S),int(y*S)) for x,y in pts]
    d.polygon(pts,fill=fill)
    if outline: d.line(pts+[pts[0]],fill=outline,width=2*S,joint='curve')
    save(im,name,w,h)

polygon('plate',192,128,(18,28,39,194))
polygon('frame',192,128,(0,0,0,0),(227,238,248,230))
polygon('badge',96,48,(18,28,39,240),(176,191,204,255))
polygon('segment',64,32,(255,255,255,255))
polygon('portrait_mask',224,280,(255,255,255,255))
polygon('portrait_frame',224,280,(0,0,0,0),(224,239,255,255))

im,d=canvas(256,256)
d.ellipse((12*S,12*S,244*S,244*S),fill=(15,25,38,188),outline=(151,166,182,255),width=S)
d.ellipse((33*S,33*S,223*S,223*S),outline=(105,119,133,255),width=16*S)
d.ellipse((52*S,52*S,204*S,204*S),outline=(220,227,238,240),width=2*S)
for angle in range(0,360,15):
 import math
 a=math.radians(angle)
 d.line([(int((128+math.cos(a)*r)*S),int((128+math.sin(a)*r)*S)) for r in (98,105)],fill=(39,49,62,255),width=S)
# A small orange locator carries the accent without suggesting charge.
d.arc((28*S,28*S,228*S,228*S),270,274,fill=(255,177,43,255),width=5*S)
save(im,'ring',256,256)

y,x=np.mgrid[0:128,0:512]
a=np.clip(1-((x-256)/290)**2,0,1)*np.clip(1-((y-64)/84)**2,0,1)*235
arr=np.zeros((128,512,4),dtype=np.uint8);arr[:,:,:3]=[12,22,34];arr[:,:,3]=a.astype(np.uint8)
save(Image.fromarray(arr).resize((2048,512)), 'smoke',512,128)
# Local backing with a stable centre; only its short edges feather into the world.
a = np.minimum(np.minimum(x / 12, (511 - x) / 12), np.minimum(y / 10, (127 - y) / 10))
arr[:, :, 3] = (np.clip(a, 0, 1) * 204).astype(np.uint8)
save(Image.fromarray(arr).resize((2048,512)), 'backing',512,128)
im,d=canvas(64,64)
d.rounded_rectangle((18*S,27*S,46*S,53*S),radius=3*S,fill='white')
d.arc((22*S,9*S,42*S,39*S),180,360,fill='white',width=5*S)
save(im,'locked',64,64)
im,d=canvas(64,64)
d.ellipse((8*S,8*S,56*S,56*S),fill=(255,188,49,255),outline=(255,229,143,255),width=3*S)
d.ellipse((17*S,14*S,46*S,50*S),outline=(156,95,21,255),width=3*S)
save(im,'coin',64,64)
im,d=canvas(64,64)
d.polygon([(33*S,4*S),(19*S,27*S),(11*S,44*S),(22*S,58*S),(44*S,55*S),(55*S,38*S),(46*S,23*S),(38*S,37*S)],fill=(32,182,255,255))
d.polygon([(33*S,9*S),(24*S,30*S),(25*S,47*S),(33*S,37*S)],fill=(181,246,255,255))
save(im,'soul',64,64)

if __name__=='__main__':
 for name in ('q','w','e','r','passive','attack','armor'):
  source=ORIGINAL/'generated'/(name+'.png')
  if not source.exists(): continue
  raw=Image.open(source).convert('L')
  bbox=raw.point(lambda p:255 if p>32 else 0).getbbox()
  if not bbox: raise ValueError('Empty glyph '+name)
  crop=raw.crop(bbox);crop.thumbnail((208,208),Image.Resampling.LANCZOS)
  alpha=Image.new('L',(256,256));alpha.paste(crop,((256-crop.width)//2,(256-crop.height)//2))
  result=Image.new('RGBA',(256,256),'white');result.putalpha(alpha)
  result.save(OUT/(name+'.png'),optimize=True)
  result.save(ORIGINAL/(name+'.png'),optimize=True)
 print('HUD geometry and available glyphs:',OUT)

"""Original shop plates in the existing HUD family; no external assets."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'unity/Assets/Game/Art/Ui/Hud/Resources/Sprites'
ORIGINAL = ROOT / 'art/ui/hud'
S = 4

def plate(name, w, h, cut, fill, outline, accent=False):
    image = Image.new('RGBA', (w*S, h*S))
    draw = ImageDraw.Draw(image)
    points = [(cut, 2), (w-8, 2), (w-2, 8), (w-cut, h-2), (8, h-2), (2, h-8)]
    points = [(int(x*S), int(y*S)) for x,y in points]
    draw.polygon(points, fill=fill)
    draw.line(points+[points[0]], fill=outline, width=2*S, joint='curve')
    if accent:
        draw.line([(int(cut*S), 9*S), ((w-16)*S, 9*S)], fill=(85,210,255,255), width=3*S)
    image = image.resize((w,h), Image.Resampling.LANCZOS)
    for folder in (OUT, ORIGINAL):
        folder.mkdir(parents=True, exist_ok=True)
        image.save(folder/(name+'.png'), optimize=True)

plate('shopWindow',512,320,24,(13,24,38,210),(212,228,243,230))
plate('shopTooltip',512,320,16,(13,24,38,242),(212,228,243,240))
plate('shopTab',256,64,12,(18,30,45,205),(129,150,171,220))
plate('shopHover',256,64,12,(30,55,75,235),(245,249,255,255))
plate('shopSelected',256,64,12,(22,47,65,240),(245,249,255,255),True)
print('Shop skin: 5 procedural sprites')

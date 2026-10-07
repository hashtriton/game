"""Skin of the in-game interface: iron plates with worn bronze edges, inset item slots, tooltip, bars and small icons.

Run from the repository root:  python -X utf8 tools/art/gen_ui_skin.py
Writes PNGs into unity/Assets/Game/Art/Ui. Everything is drawn here, nothing is taken from another game.
Sliced images (panel, tooltip, slot, button) keep their corners at the sizes listed in SLICES; the Unity builder
reads that table to set the sprite borders.
"""
import os
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from scipy import ndimage as ndi

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'unity', 'Assets', 'Game', 'Art', 'Ui')
OUT = os.path.normpath(OUT)
os.makedirs(OUT, exist_ok=True)

# name -> border in pixels (left, bottom, right, top), read by the builder
SLICES = {
    'panel': (22, 22, 22, 22),
    'tooltip': (14, 14, 14, 14),
    'slot': (6, 6, 6, 6),
    'button': (10, 10, 10, 10),
    'frame': (6, 6, 6, 6),
    'tab': (8, 8, 8, 8),
}

BRONZE = np.array([0.50, 0.38, 0.21])
STEEL = np.array([0.30, 0.31, 0.33])
IRON = np.array([0.062, 0.060, 0.062])


def noise(h, w, seed, sigma, amp):
    rng = np.random.default_rng(seed)
    field = ndi.gaussian_filter(rng.standard_normal((h, w)), sigma)
    field = (field - field.mean()) / (field.std() + 1e-6)
    return field * amp


def edge_distance(h, w):
    y, x = np.mgrid[0:h, 0:w]
    return np.minimum.reduce([x, y, w - 1 - x, h - 1 - y]).astype(np.float32)


def lit(height, light=(-0.6, -0.8), strength=1.0):
    """Shade a height map: faces turned to the upper left are brighter."""
    gy, gx = np.gradient(height)
    shade = -(gx * light[0] + gy * light[1]) * strength
    return shade


def to_image(rgb, alpha=None):
    rgb = np.clip(rgb, 0, 1)
    if alpha is None:
        alpha = np.ones(rgb.shape[:2])
    arr = np.dstack([rgb, np.clip(alpha, 0, 1)])
    return Image.fromarray((arr * 255 + 0.5).astype(np.uint8), 'RGBA')


def save(img, name):
    img.save(os.path.join(OUT, name + '.png'), optimize=True)


def metal_plate(h, w, seed, base, grain=0.012):
    """Dark iron with a faint brushed grain and some wear."""
    rng = np.random.default_rng(seed)
    body = np.ones((h, w, 3)) * base
    streak = ndi.gaussian_filter(rng.standard_normal((h, w)), (0.6, 7.0))
    streak = (streak - streak.mean()) / (streak.std() + 1e-6)
    blotch = noise(h, w, seed + 1, 9.0, 1.0)
    fine = rng.standard_normal((h, w)) * 0.35
    wear = (streak * 0.55 + blotch * 0.8 + fine) * grain
    body += wear[..., None]
    return body


def panel(w=128, h=128, seed=3, border=22):
    body = metal_plate(h, w, seed, IRON)
    d = edge_distance(h, w)

    # Raised rim: bevel from the distance to the edge, bronze toned, worn at random.
    rim_h = np.clip(d / 4.0, 0, 1)
    rim_h = np.where(d < 5, rim_h, 1.0)
    shade = lit(ndi.gaussian_filter(rim_h, 0.6), strength=2.4)
    wear = np.clip(noise(h, w, seed + 5, 2.5, 0.5) + 0.5, 0, 1)
    rim_color = BRONZE[None, None, :] * (0.55 + 0.45 * wear[..., None])
    rim_mask = (d < 4.2)[..., None].astype(np.float32)
    rgb = body * (1 - rim_mask) + rim_color * rim_mask
    rgb += shade[..., None] * 0.55 * (d < 6)[..., None]

    # Dark groove inside the rim, then a thin steel line.
    groove = ((d >= 4.2) & (d < 6.2))[..., None].astype(np.float32)
    rgb = rgb * (1 - groove) + np.array([0.015, 0.014, 0.014]) * groove
    line = ((d >= 6.2) & (d < 7.4))[..., None].astype(np.float32)
    rgb = rgb * (1 - line) + STEEL * 0.75 * line

    # Inner shadow so the plate looks recessed under the rim.
    inner = np.clip(1.0 - (d - 7.4) / 12.0, 0, 1) * (d >= 7.4)
    rgb *= (1 - inner * 0.55)[..., None]

    # Four rivets.
    yy, xx = np.mgrid[0:h, 0:w]
    for cx, cy in ((13, 13), (w - 14, 13), (13, h - 14), (w - 14, h - 14)):
        r = np.hypot(xx - cx, yy - cy)
        dome = np.clip(1 - r / 3.4, 0, 1)
        mask = (r < 3.4)[..., None]
        shade_r = lit(ndi.gaussian_filter(dome, 0.4), strength=3.0)
        col = BRONZE * 0.7 + shade_r[..., None] * 0.9
        rgb = np.where(mask, col, rgb)
    return to_image(rgb)


def tooltip(w=96, h=96, seed=11):
    body = metal_plate(h, w, seed, np.array([0.045, 0.043, 0.046]), grain=0.01)
    d = edge_distance(h, w)
    edge = (d < 1.6)[..., None].astype(np.float32)
    edge2 = ((d >= 1.6) & (d < 2.8))[..., None].astype(np.float32)
    rgb = body * (1 - edge) + BRONZE * 0.8 * edge
    rgb = rgb * (1 - edge2) + np.array([0.01, 0.01, 0.01]) * edge2
    inner = np.clip(1.0 - (d - 2.8) / 7.0, 0, 1) * (d >= 2.8)
    rgb *= (1 - inner * 0.5)[..., None]
    return to_image(rgb, np.full((h, w), 0.97))


def slot(w=64, h=64, seed=21):
    d = edge_distance(h, w)
    base = np.ones((h, w, 3)) * np.array([0.034, 0.033, 0.036])
    base += noise(h, w, seed, 4.0, 0.006)[..., None]
    height = np.clip((d - 1) / 5.0, 0, 1)
    # Inset: the upper left falls in shadow, the lower right catches light.
    shade = -lit(ndi.gaussian_filter(height, 0.7), strength=1.6)
    rgb = base + shade[..., None] * 0.22
    edge = (d < 1.2)[..., None].astype(np.float32)
    rgb = rgb * (1 - edge) + BRONZE * 0.38 * edge
    return to_image(rgb)


def frame(w=64, h=64):
    """Thin bevelled frame, white so the interface can tint it by item grade."""
    d = edge_distance(h, w)
    ring = ((d >= 0) & (d < 2.4)).astype(np.float32)
    height = np.clip(1 - np.abs(d - 1.2) / 1.2, 0, 1) * ring
    shade = lit(ndi.gaussian_filter(height, 0.5), strength=1.4)
    value = np.clip(0.85 + shade * 0.5, 0.3, 1.0)
    rgb = np.dstack([value, value, value])
    return to_image(rgb, ring)


def button(w=64, h=32, seed=31):
    body = metal_plate(h, w, seed, np.array([0.085, 0.078, 0.07]), grain=0.014)
    d = edge_distance(h, w)
    height = np.clip(d / 5.0, 0, 1)
    shade = lit(ndi.gaussian_filter(height, 0.8), strength=1.8)
    rgb = body + shade[..., None] * 0.20
    edge = (d < 1.4)[..., None].astype(np.float32)
    rgb = rgb * (1 - edge) + BRONZE * 0.7 * edge
    dark = ((d >= 1.4) & (d < 2.4))[..., None].astype(np.float32)
    rgb = rgb * (1 - dark) + np.array([0.01, 0.01, 0.01]) * dark
    return to_image(rgb)


def tab(w=48, h=32, seed=41):
    body = metal_plate(h, w, seed, np.array([0.055, 0.052, 0.05]), grain=0.012)
    d = edge_distance(h, w)
    edge = (d < 1.2)[..., None].astype(np.float32)
    rgb = body * (1 - edge) + BRONZE * 0.45 * edge
    return to_image(rgb)


def bar_fill(w=64, h=16):
    y = np.linspace(0, 1, h)[:, None]
    gloss = 0.78 + 0.22 * np.cos((y - 0.35) * 3.0)
    value = np.repeat(gloss, w, axis=1)
    grain = noise(h, w, 51, 1.2, 0.03)
    value = value + grain
    return to_image(np.dstack([value, value, value]))


def divider(w=128, h=4):
    x = np.linspace(-1, 1, w)[None, :]
    a = np.clip(1 - np.abs(x) ** 2.2, 0, 1)
    a = np.repeat(a, h, axis=0)
    core = np.zeros((h, w))
    core[1:3, :] = 1
    rgb = np.dstack([np.full((h, w), BRONZE[0]), np.full((h, w), BRONZE[1]), np.full((h, w), BRONZE[2])])
    return to_image(rgb, a * core * 0.85)


def coin(size=32):
    s = size * 8
    img = Image.new('RGBA', (s, s), (0, 0, 0, 0))
    g = ImageDraw.Draw(img)
    m = s * 0.09
    g.ellipse([m, m, s - m, s - m], fill=(86, 62, 20, 255))
    g.ellipse([m * 1.5, m * 1.5, s - m * 1.5, s - m * 1.5], fill=(176, 136, 52, 255))
    g.ellipse([m * 2.4, m * 2.4, s - m * 2.4, s - m * 2.4], outline=(110, 80, 26, 255), width=int(s * 0.04))
    g.rectangle([s * 0.44, s * 0.30, s * 0.56, s * 0.70], fill=(110, 80, 26, 255))
    arr = np.array(img).astype(np.float32) / 255
    alpha = arr[..., 3]
    h = ndi.gaussian_filter(alpha, s * 0.03)
    shade = lit(h, strength=s * 0.25)
    arr[..., :3] = np.clip(arr[..., :3] + shade[..., None] * 0.35, 0, 1)
    out = Image.fromarray((arr * 255 + 0.5).astype(np.uint8), 'RGBA')
    return out.resize((size, size), Image.LANCZOS)


def soul(size=32):
    s = size * 8
    yy, xx = np.mgrid[0:s, 0:s].astype(np.float32)
    x = (xx / s - 0.5) * 2
    y = (yy / s - 0.5) * 2
    # A teardrop wisp pointing up.
    width = np.clip(0.55 * (1 - ((y + 0.15) / 0.95) ** 2), 0, 1) * np.clip(1 - np.abs(y + 0.15 - 0.45) * 0.0, 0, 1)
    body = (np.abs(x) < width * (0.5 + 0.5 * np.clip((y + 0.9) / 1.6, 0, 1))) & (y > -0.85) & (y < 0.75)
    glow = ndi.gaussian_filter(body.astype(np.float32), s * 0.05)
    core = ndi.gaussian_filter(body.astype(np.float32), s * 0.02)
    rgb = np.dstack([0.25 * glow + 0.4 * core, 0.7 * glow + 0.5 * core, 0.9 * glow + 0.4 * core])
    alpha = np.clip(glow * 1.6, 0, 1)
    out = to_image(rgb, alpha)
    return out.resize((size, size), Image.LANCZOS)


def preview():
    sheet = Image.new('RGBA', (640, 360), (24, 24, 28, 255))
    for name, pos, size in (('panel', (20, 20), (260, 160)), ('tooltip', (300, 20), (200, 120)), ('slot', (520, 20), (72, 72)),
                            ('button', (300, 160), (160, 44)), ('frame', (520, 120), (72, 72)), ('tab', (300, 220), (120, 36))):
        src = Image.open(os.path.join(OUT, name + '.png'))
        b = SLICES[name]
        w, h = src.size
        dst = Image.new('RGBA', size, (0, 0, 0, 0))
        l, bt, r, t = b
        # nine-slice scale
        regions_src = [(0, 0, l, t), (l, 0, w - r, t), (w - r, 0, w, t),
                       (0, t, l, h - bt), (l, t, w - r, h - bt), (w - r, t, w, h - bt),
                       (0, h - bt, l, h), (l, h - bt, w - r, h), (w - r, h - bt, w, h)]
        W, H = size
        xs = [0, l, W - r, W]
        ys = [0, t, H - bt, H]
        for i, rs in enumerate(regions_src):
            cx, cy = i % 3, i // 3
            box = (xs[cx], ys[cy], xs[cx + 1], ys[cy + 1])
            piece = src.crop(rs).resize((max(1, box[2] - box[0]), max(1, box[3] - box[1])), Image.NEAREST)
            dst.paste(piece, box[:2])
        sheet.paste(dst, pos, dst)
    for name, pos in (('coin', (20, 220)), ('soul', (70, 220))):
        img = Image.open(os.path.join(OUT, name + '.png')).resize((48, 48), Image.NEAREST)
        sheet.paste(img, pos, img)
    preview_dir = os.path.normpath(os.path.join(OUT, '..', '..', '..', '..', '..', '.local', 'work'))
    os.makedirs(preview_dir, exist_ok=True)
    sheet.save(os.path.join(preview_dir, 'ui-skin-preview.png'))


if __name__ == '__main__':
    save(panel(), 'panel')
    save(tooltip(), 'tooltip')
    save(slot(), 'slot')
    save(frame(), 'frame')
    save(button(), 'button')
    save(tab(), 'tab')
    save(bar_fill(), 'bar_fill')
    save(divider(), 'divider')
    save(coin(), 'coin')
    save(soul(), 'soul')
    preview()
    print('ui skin written to', OUT)

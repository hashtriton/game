"""Second batch: wood, crate, barrel, bark, cloth, pine card, flame sheet, mist, decals, rust."""
import numpy as np, os, sys
from PIL import Image, ImageDraw
from tg import *


def grid(n):
    yy, xx = np.mgrid[0:n, 0:n].astype(np.float64)
    return yy, xx


def wood(n=512, seed=31, planks=6):
    rng = np.random.default_rng(seed)
    yy, xx = grid(n)
    pw = n / planks
    pid = (xx // pw).astype(int)
    px = (xx % pw) / pw
    gr = fnoise(n, 1.3, seed + 1, ay=18, hi=.45)
    gm = fnoise(n, 2.4, seed + 2, ay=5, hi=.06)
    warp = fnoise(n, 2.2, seed + 3, hi=.02)
    tones = np.array([[.30, .235, .165], [.26, .21, .16], [.33, .27, .2], [.23, .2, .17], [.28, .23, .18], [.2, .17, .14]])
    tone = tones[rng.integers(0, 6, planks)][pid % planks] * rng.uniform(.85, 1.15, planks)[pid % planks][..., None]
    cr = sstep(.78, .9, fnoise(n, 1.2, seed + 4, ay=30, hi=.45))
    e = np.minimum(px, 1 - px)
    gap = 1 - sstep(0, .03, e)
    bv = sstep(0, .09, e)
    h = bv * .55 + gr * .2 + gm * .22 - cr * .5 - gap * .7
    col = tone * (.55 + .75 * gm[..., None]) * (.78 + .44 * gr[..., None])
    col *= (1 - .45 * cr)[..., None]
    col *= (1 - .55 * gap)[..., None] * (.8 + .2 * bv[..., None])
    col *= (1 + .18 * (1 - bv)[..., None])
    # nails with rust streaks
    for pi in range(planks):
        cx = (pi + .5) * pw
        for cy in (n * .06, n * .94):
            dx = xx - cx; dy = yy - cy
            nail = np.sqrt(dx * dx + dy * dy) < 4.2
            streak = (np.abs(dx) < 3.5) & (dy > 0) & (dy < 60 * rng.uniform(.5, 1.2))
            col = np.where(streak[..., None], col * .62, col)
            col = np.where(nail[..., None], np.array([.09, .08, .075]), col)
            h = np.where(nail, h + .5, h)
    rough = np.clip(.86 + .08 * (gr - .5), 0, 1)
    return col, normal_map(h, 3.5), rough, h


def crate(n=512, seed=41):
    rng = np.random.default_rng(seed)
    yy, xx = grid(n)
    b = 46
    gx = fnoise(n, 1.3, seed + 1, ax=18, hi=.45)   # grain along x
    gy = fnoise(n, 1.3, seed + 2, ay=18, hi=.45)   # grain along y
    gm = fnoise(n, 2.4, seed + 3, hi=.06)
    border = (xx < b) | (xx >= n - b) | (yy < b) | (yy >= n - b)
    vert = ((xx < b) | (xx >= n - b))
    inner_y = (yy - b) / (n - 2 * b)
    plank = np.clip((inner_y * 4).astype(int), 0, 3)
    brace = (np.abs(xx - yy) * .7071 < 24) & ~border
    grain = np.where(border & vert, gy, np.where(border, gx, np.where(brace, gx, gx)))
    tones = np.array([[.28, .22, .16], [.24, .2, .15], [.31, .25, .18], [.22, .19, .15]])
    tone = tones[plank]
    tone = np.where(border[..., None], np.array([.21, .17, .13]), tone)
    tone = np.where(brace[..., None], np.array([.27, .22, .165]), tone)
    # seams
    seam = np.zeros((n, n))
    for k in range(1, 4):
        y0 = b + k * (n - 2 * b) / 4
        seam = np.maximum(seam, 1 - sstep(0, 3, np.abs(yy - y0)))
    ed = np.minimum(np.minimum(xx - 0, n - 1 - xx), np.minimum(yy - 0, n - 1 - yy))
    inner_edge = np.minimum(np.minimum(np.abs(xx - b), np.abs(xx - (n - b))), np.minimum(np.abs(yy - b), np.abs(yy - (n - b))))
    seam = np.maximum(seam * (~border), (1 - sstep(0, 3, inner_edge)) * 1.0)
    bd = np.abs((np.abs(xx - yy) * .7071) - 24)
    seam = np.maximum(seam, (1 - sstep(0, 2.5, bd)) * (~border) * (np.abs(xx - yy) * .7071 < 30))
    h = gx * .15 + gm * .2 + (~border) * 0 - seam * .7 + border * .35 + brace * .2
    col = tone * (.6 + .7 * gm[..., None]) * (.78 + .44 * grain[..., None])
    col *= (1 - .6 * seam)[..., None]
    # iron corner brackets
    iron = np.zeros((n, n), bool)
    for (cx, cy) in ((0, 0), (n, 0), (0, n), (n, n)):
        dx = np.abs(xx - cx); dy = np.abs(yy - cy)
        iron |= ((dx < 112) & (dy < 30)) | ((dx < 30) & (dy < 112))
    rustn = fnoise(n, 1.8, seed + 4, hi=.2)
    ic = mix(np.array([.10, .095, .09]), np.array([.30, .15, .07]), sstep(.55, .85, rustn)) * (.8 + .4 * fnoise(n, 1.0, seed + 5, hi=.5)[..., None])
    col = np.where(iron[..., None], ic, col)
    h = np.where(iron, h + .5 + .1 * rustn, h)
    # nails
    for (cx, cy) in ((20, 20), (n - 20, 20), (20, n - 20), (n - 20, n - 20), (80, 15), (n - 80, 15), (80, n - 15), (n - 80, n - 15), (15, 80), (15, n - 80), (n - 15, 80), (n - 15, n - 80)):
        d = np.sqrt((xx - cx) ** 2 + (yy - cy) ** 2) < 4.5
        col = np.where(d[..., None], np.array([.07, .065, .06]), col)
        h = np.where(d, h + .3, h)
    col *= (1 - .35 * (1 - sstep(0, 22, ed)))[..., None] * 1.0
    rough = np.where(iron, .55, np.clip(.88 + .08 * (grain - .5), 0, 1))
    return col, normal_map(h, 3.5), rough, h


def barrel(n=512, seed=51, staves=14):
    rng = np.random.default_rng(seed)
    yy, xx = grid(n)
    sw = n / staves
    sid = (xx // sw).astype(int) % staves
    sx = (xx % sw) / sw
    gr = fnoise(n, 1.3, seed + 1, ay=16, hi=.45)
    gm = fnoise(n, 2.4, seed + 2, ay=3, hi=.05)
    tones = np.array([[.27, .20, .135], [.24, .18, .125], [.30, .22, .15], [.21, .165, .12], [.26, .2, .14]])
    tone = tones[rng.integers(0, 5, staves)][sid] * rng.uniform(.85, 1.15, staves)[sid][..., None]
    e = np.minimum(sx, 1 - sx)
    gap = 1 - sstep(0, .06, e)
    h = sstep(0, .22, e) * .5 + gr * .15 + gm * .15 - gap * .6
    col = tone * (.6 + .7 * gm[..., None]) * (.8 + .4 * gr[..., None])
    col *= (1 - .6 * gap)[..., None]
    # hoops
    hoop = np.zeros((n, n))
    for (a, bb) in ((.04, .105), (.27, .33), (.67, .73), (.895, .96)):
        y0, y1 = a * n, bb * n
        hoop = np.maximum(hoop, sstep(y0, y0 + 3, yy) * (1 - sstep(y1 - 3, y1, yy)))
    rustn = fnoise(n, 1.8, seed + 3, hi=.25)
    hc = mix(np.array([.095, .092, .09]), np.array([.34, .17, .075]), sstep(.5, .85, rustn)) * (.8 + .4 * fnoise(n, 1.0, seed + 4, hi=.5)[..., None])
    col = mix(col, hc, hoop)
    h = h + hoop * .7
    # dirt and grime
    col *= (1 - .45 * sstep(.75, 1.0, yy / n))[..., None]
    col *= (1 - .25 * sstep(.45, .85, fnoise(n, 2.0, seed + 6, ay=5, hi=.1)))[..., None]
    rough = np.where(hoop > .5, .5, np.clip(.88 + .08 * (gr - .5), 0, 1))
    return col, normal_map(h, 3.8), rough, h


def bark(n=512, seed=71):
    yy, xx = grid(n)
    r = fnoise(n, 2.0, seed, ay=10, hi=.25)
    fis = 1 - np.abs(2 * fnoise(n, 2.0, seed + 1, ay=7, hi=.07) - 1)
    ridge = sstep(.35, 1.0, fis) ** 1.2
    cross = sstep(.7, .9, fnoise(n, 1.6, seed + 2, ax=1, ay=.4, hi=.2))
    h = ridge * .9 + r * .25 - cross * .25
    col = mix(np.array([.06, .05, .043]), np.array([.2, .17, .14]), np.clip(h * .9 + .1, 0, 1))
    lich = sstep(.62, .78, fnoise(n, 2.4, seed + 3, hi=.05)) * (.3 + .7 * ridge)
    col = mix(col, np.array([.2, .22, .17]) * (.7 + .5 * r[..., None]), lich * .55)
    col *= (.8 + .4 * fnoise(n, 1.0, seed + 4, hi=.5))[..., None]
    rough = np.clip(.95 - .1 * ridge, 0, 1)
    return col, normal_map(h, 5.5), rough, h


def cloth(n=256, seed=81):
    yy, xx = grid(n)
    wv = (.93 + .07 * np.sin(xx * 2 * np.pi / 3)) * (.94 + .06 * np.sin(yy * 2 * np.pi / 3.2))
    st = fnoise(n, 2.2, seed, hi=.08)
    fine = fnoise(n, 1.2, seed + 1, hi=.5)
    crease = sstep(.7, .9, fnoise(n, 1.6, seed + 2, ay=3, hi=.1))
    col = np.array([.62, .56, .46]) * (.55 + .7 * st[..., None]) * (.85 + .3 * fine[..., None]) * wv[..., None]
    col *= (1 - .3 * crease)[..., None]
    col *= (1 - .3 * sstep(.6, .9, fnoise(n, 2.0, seed + 3, ay=6, hi=.08)))[..., None]
    h = wv * .3 + fine * .1 + st * .1 - crease * .3
    return col, normal_map(h, 2.0), np.full((n, n), .95), h


def cloth_alpha(n=256, seed=82):
    yy, xx = grid(n)
    # ragged bottom edge in image space (v=0 at the bottom of the image)
    rng = np.random.default_rng(seed)
    prof = np.cumsum(rng.standard_normal(n))
    prof = prof - np.linspace(prof[0], prof[-1], n)
    prof = ndi.gaussian_filter1d(prof, 1.5, mode='wrap')
    prof = prof / (np.abs(prof).max() + 1e-6)
    notch = np.zeros(n)
    for _ in range(9):
        c = rng.integers(0, n); wd = rng.integers(4, 14); dp = rng.uniform(.04, .16)
        notch += np.maximum(0, 1 - np.abs((np.arange(n) - c) / wd)) * dp
    edge = n * (.9 - .05 * prof - notch)
    a = (yy < edge[None, :].repeat(n, 0)).astype(float)
    # holes
    f1, f2, c, k = voronoi(n, 14, seed + 1)
    holes = (f1 < 6 + 8 * np.random.default_rng(seed + 2).random(k)[c]) * (np.random.default_rng(seed + 3).random(k)[c] > .62) * sstep(.45, .7, fnoise(n, 2.0, seed + 4, hi=.05))
    a = a * (1 - holes)
    a = np.clip(ndi.gaussian_filter(a, .8) * 1.4 - .2, 0, 1)
    return a


def pine_card(W=768, H=384, seed=61):
    S = 2
    w, h = W * S, H * S
    im = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    rng = np.random.default_rng(seed)
    cy = h // 2
    d.line([(0, cy), (w * .985, cy)], fill=(40, 28, 20, 255), width=int(5 * S))
    x = 26 * S
    while x < w * .97:
        for side in (-1, 1):
            t = x / w
            L = ((1 - t) * h * .44 + 22 * S) * rng.uniform(.75, 1.1)
            a = np.radians(rng.uniform(46, 66))
            ex = x + np.cos(a) * L * .72
            ey = cy + side * np.sin(a) * L
            d.line([(x, cy), (ex, ey)], fill=(42, 30, 22, 255), width=int(2.4 * S))
            dx, dy = ex - x, ey - cy
            ln = np.hypot(dx, dy)
            ux, uy = dx / ln, dy / ln
            for tt in np.linspace(.06, 1.0, 15):
                px, py = x + dx * tt, cy + dy * tt
                nl = (13 - 5 * tt) * S
                for _ in range(5):
                    sgn = rng.choice([-1, 1])
                    ang = np.arctan2(uy, ux) + sgn * np.radians(rng.uniform(32, 82))
                    L2 = nl * rng.uniform(.7, 1.25)
                    k = rng.random()
                    if k < .08:
                        c = (70 + int(rng.integers(0, 30)), 52 + int(rng.integers(0, 20)), 30, 235)
                    else:
                        g = float(rng.uniform(0, 1))
                        c = (int(14 + 18 * g), int(34 + 40 * g + 18 * tt), int(22 + 18 * g), 245)
                    d.line([(px, py), (px + np.cos(ang) * L2, py + np.sin(ang) * L2)], fill=c, width=int(1.7 * S))
        x += rng.uniform(11, 17) * S
    im = im.resize((W, H), Image.LANCZOS)
    p = os.path.join(OUT, 'pine.png')
    im.save(p, optimize=True)
    return p


def flame_sheet(fw=128, fh=256, seed=91):
    sheet = np.zeros((fh * 2, fw * 4, 4))
    big = fnoise(512, 2.0, seed, ay=1.0, hi=.2)
    for k in range(8):
        rng = np.random.default_rng(seed + k)
        v = np.linspace(1, 0, fh)[:, None] * np.ones((1, fw))      # 0 at top, 1 at bottom -> height from bottom
        v = 1 - v                                                   # v=0 bottom, 1 top
        u = (np.linspace(-1, 1, fw)[None, :] * np.ones((fh, 1)))
        ph = rng.random() * 100
        yy = np.arange(fh)[:, None]; xx = np.arange(fw)[None, :]
        nz = ndi.map_coordinates(big, [(yy * 1.6 + ph * 3.3) % 512 + 0 * xx, (xx * 1.6 + ph * 5.7) % 512 + 0 * yy], order=1, mode='grid-wrap')
        nz2 = ndi.map_coordinates(big, [(yy * 3.1 + ph * 7.1) % 512 + 0 * xx, (xx * 3.1 + ph * 2.3) % 512 + 0 * yy], order=1, mode='grid-wrap')
        wob = (nz - .5) * .9 * v + (nz2 - .5) * .35 * v
        sway = np.sin(v * 5 + ph) * .12 * v
        width = (.62 * (1 - v) ** .7 + .07) * (1 - .3 * v)
        dens = np.exp(-(((u - wob - sway) / (width + 1e-3)) ** 2) * 1.6) * np.clip(1 - v ** 1.25, 0, 1)
        dens *= (.7 + .6 * nz2) ** 1.0
        I = np.clip(dens * 1.25, 0, 1.2)
        r = np.clip(I * 1.6, 0, 1)
        g = np.clip((I - .25) * 1.5, 0, 1) ** 1.2
        b = np.clip((I - .75) * 2.5, 0, 1)
        a = np.clip(I * 1.1, 0, 1)
        fr = np.stack([r, g, b, a], -1)[::-1]
        ox, oy = (k % 4) * fw, (k // 4) * fh
        sheet[oy:oy + fh, ox:ox + fw] = fr
    save_png(sheet, 'flame')
    return sheet


def mist(n=256, seed=95):
    a = fnoise(n, 2.6, seed, hi=.05)
    a2 = fnoise(n, 1.8, seed + 1, hi=.1)
    m = np.clip((a * .7 + a2 * .3 - .38) * 2.0, 0, 1) ** 1.3
    rgba = np.zeros((n, n, 4))
    rgba[..., 0] = .62; rgba[..., 1] = .72; rgba[..., 2] = .8; rgba[..., 3] = m
    save_png(rgba, 'mist')


def macro(n=256, seed=96):
    a = fnoise(n, 2.4, seed, hi=.07)
    b = fnoise(n, 1.6, seed + 1, hi=.2)
    save_jpg(np.clip(a * .75 + b * .25, 0, 1), 'macro', 85)


def blood(n=256, seed=97):
    yy, xx = grid(n)
    rng = np.random.default_rng(seed)
    c = n / 2
    r = np.sqrt((xx - c) ** 2 + (yy - c) ** 2) / (n * .5)
    nz = fnoise(n, 1.8, seed, hi=.12)
    m = sstep(.55, .35, r + (nz - .5) * .6)
    for _ in range(46):
        a = rng.uniform(0, 6.283); rr = rng.uniform(.25, .95) * n * .5; rad = rng.uniform(1.5, 6) * (1.3 - rr / n)
        px, py = c + np.cos(a) * rr, c + np.sin(a) * rr
        m = np.maximum(m, (np.sqrt((xx - px) ** 2 + (yy - py) ** 2) < rad) * 1.0)
    m = np.clip(ndi.gaussian_filter(m, .7), 0, 1)
    wet = fnoise(n, 1.4, seed + 1, hi=.3)
    col = mix(np.array([.20, .03, .025]), np.array([.07, .012, .012]), wet)
    rgba = np.concatenate([col, (m * (.55 + .4 * wet))[..., None]], -1)
    save_png(rgba, 'blood')


def scorch(n=256, seed=98):
    yy, xx = grid(n)
    c = n / 2
    r = np.sqrt((xx - c) ** 2 + (yy - c) ** 2) / (n * .5)
    nz = fnoise(n, 2.0, seed, hi=.12)
    m = sstep(1.0, .2, r + (nz - .5) * .8)
    rgba = np.zeros((n, n, 4)); rgba[..., :3] = .02; rgba[..., 3] = m * .85
    save_png(rgba, 'scorch')


def rust(n=256, seed=99):
    yy, xx = grid(n)
    rn = fnoise(n, 1.8, seed, hi=.25)
    fine = fnoise(n, 1.0, seed + 1, hi=.5)
    m = sstep(.45, .8, rn)
    col = mix(np.array([.12, .115, .11]), np.array([.34, .16, .07]), m) * (.7 + .5 * fine[..., None])
    h = rn * .3 + fine * .2
    return col, normal_map(h, 3.0), np.clip(.55 + .3 * m + .1 * fine, 0, 1), h


def card(c, nm, r, name, sz=512):
    def rs(a):
        im = Image.fromarray((np.clip(a, 0, 1) * 255 + .5).astype(np.uint8))
        return np.asarray(im.resize((sz, sz), Image.LANCZOS)).astype(np.float64) / 255
    return rs(c), rs(nm), rs(r)


if __name__ == '__main__':
    which = sys.argv[1:] or ['wood', 'crate', 'barrel', 'bark', 'cloth', 'rust']
    for w in which:
        if w in ('wood', 'crate', 'barrel', 'bark', 'cloth', 'rust'):
            c, nm, r, h = globals()[w]()
            save_jpg(c, w + '_a', 82); save_jpg(nm, w + '_n', 86); save_jpg(r, w + '_r', 80)
            preview([c, nm, r], w)
            print(w, 'ok', [os.path.getsize(os.path.join(OUT, w + s + '.jpg')) // 1024 for s in ('_a', '_n', '_r')], 'KB')
        elif w == 'cloth_alpha':
            a = cloth_alpha(); save_jpg(a, 'cloth_alpha', 85); preview([a, a], 'cloth_alpha')
        elif w == 'pine':
            print(pine_card())
        elif w == 'flame':
            s = flame_sheet(); a = s[..., :3] * s[..., 3:4]; preview([a], 'flame')
        elif w == 'mist':
            mist()
        elif w == 'macro':
            macro()
        elif w == 'blood':
            blood()
        elif w == 'scorch':
            scorch()




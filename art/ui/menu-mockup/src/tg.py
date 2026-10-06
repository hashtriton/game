"""Texture generation toolkit for the grim arena (numpy, tileable)."""
import numpy as np
from scipy import ndimage as ndi
from scipy.spatial import cKDTree
from PIL import Image, ImageDraw, ImageFilter
import os, io

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'tex')
os.makedirs(OUT, exist_ok=True)


def fnoise(n, beta=2.0, seed=0, ax=1.0, ay=1.0, lo=1.0, hi=0.5):
    """Tileable spectral noise normalised to ~[0,1]. ay>1 stretches features along y."""
    rng = np.random.default_rng(seed)
    F = np.fft.fft2(rng.standard_normal((n, n)))
    fy = np.fft.fftfreq(n)[:, None]
    fx = np.fft.fftfreq(n)[None, :]
    f = np.sqrt((fx * ax) ** 2 + (fy * ay) ** 2)
    f[0, 0] = 1.0
    amp = f ** (-beta / 2.0)
    fr = np.sqrt(fx ** 2 + fy ** 2)
    amp = amp * (fr <= hi) * (fr * n >= lo)
    out = np.real(np.fft.ifft2(F * amp))
    p1, p99 = np.percentile(out, [1, 99])
    return np.clip((out - p1) / (p99 - p1), 0, 1)


def sstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def mix(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2:
        t = t[..., None]
    return a * (1 - t) + b * t


def voronoi(n, k, seed, grid=False, jit=0.8):
    rng = np.random.default_rng(seed)
    if grid:
        g = int(round(np.sqrt(k)))
        gy, gx = np.mgrid[0:g, 0:g]
        pts = np.stack([(gx.ravel() + .5 + (rng.random(g * g) - .5) * jit) * n / g,
                        (gy.ravel() + .5 + (rng.random(g * g) - .5) * jit) * n / g], 1)
        k = g * g
    else:
        pts = rng.random((k, 2)) * n
    P = np.concatenate([pts + np.array([dx * n, dy * n]) for dx in (-1, 0, 1) for dy in (-1, 0, 1)])
    tree = cKDTree(P)
    yy, xx = np.mgrid[0:n, 0:n]
    q = np.stack([xx.ravel() + .5, yy.ravel() + .5], 1)
    d, i = tree.query(q, k=2)
    return d[:, 0].reshape(n, n), d[:, 1].reshape(n, n), (i[:, 0] % k).reshape(n, n), k


def normal_map(h, s):
    gx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * .5
    gy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * .5
    nx, ny, nz = -gx * s, gy * s, np.ones_like(h)
    l = np.sqrt(nx * nx + ny * ny + nz * nz)
    return np.stack([nx / l, ny / l, nz / l], -1) * .5 + .5


def save_jpg(arr, name, q=82):
    a = (np.clip(arr, 0, 1) * 255 + .5).astype(np.uint8)
    if a.ndim == 2:
        im = Image.fromarray(a, 'L')
    else:
        im = Image.fromarray(a, 'RGB')
    p = os.path.join(OUT, name + '.jpg')
    im.save(p, quality=q, optimize=True, subsampling=0 if arr.ndim == 3 else 0)
    return p


def save_png(arr, name):
    a = (np.clip(arr, 0, 1) * 255 + .5).astype(np.uint8)
    im = Image.fromarray(a, 'RGBA')
    p = os.path.join(OUT, name + '.png')
    im.save(p, optimize=True)
    return p


def blur(a, s):
    return ndi.gaussian_filter(a, s, mode='wrap')


# ---------------------------------------------------------------- stone wall
def stone(n=1024, rows=7, seed=1):
    rng = np.random.default_rng(seed)
    hts = rng.uniform(.7, 1.45, rows)
    hts = hts / hts.sum() * n
    yb = np.concatenate([[0], np.cumsum(hts)]).round().astype(int)
    yb[-1] = n
    ids = np.zeros((n, n), np.int32)
    xs = np.arange(n)
    bid = 0
    for r in range(rows):
        per = rng.integers(3, 6)
        w = rng.uniform(.6, 1.6, per)
        e = np.cumsum(w / w.sum() * n)
        rid = np.searchsorted(e[:-1], xs, side='right') + bid
        ids[yb[r]:yb[r + 1], :] = rid[None, :]
        bid += per
    yy, xx = np.mgrid[0:n, 0:n].astype(np.float64)
    wx = (fnoise(n, 2.2, seed + 1, hi=.025) - .5) * 2 * 20 + (fnoise(n, 1.8, seed + 2, hi=.1) - .5) * 2 * 5
    wy = (fnoise(n, 2.2, seed + 3, hi=.025) - .5) * 2 * 20 + (fnoise(n, 1.8, seed + 4, hi=.1) - .5) * 2 * 5
    idw = ndi.map_coordinates(ids, [(yy + wy) % n, (xx + wx) % n], order=0, mode='grid-wrap')
    edge = (idw != np.roll(idw, 1, 0)) | (idw != np.roll(idw, 1, 1))
    big = np.tile(~edge, (3, 3))
    dist = ndi.distance_transform_edt(big)[n:2 * n, n:2 * n]
    nb = bid
    # block-local coordinates for per-stone tilt and doming
    cx = ndi.mean(xx, idw, np.arange(nb)); cy = ndi.mean(yy, idw, np.arange(nb))
    cx = np.nan_to_num(cx); cy = np.nan_to_num(cy)
    lx = (xx - cx[idw]) / 90.0; ly = (yy - cy[idw]) / 60.0
    tiltx = rng.uniform(-.18, .18, nb)[idw]; tilty = rng.uniform(-.18, .18, nb)[idw]
    dome = (1 - np.clip(lx * lx * .5 + ly * ly * .9, 0, 1)) * rng.uniform(.1, .35, nb)[idw]
    bev = rng.uniform(8, 18, nb)[idw]
    off = rng.uniform(-.14, .14, nb)[idw]
    t = np.clip(dist / bev, 0, 1)
    prof = np.sqrt(sstep(0, 1, t))
    g1 = fnoise(n, 1.5, seed + 5, hi=.5)
    g2 = fnoise(n, 2.6, seed + 6, hi=.08)
    g3 = fnoise(n, 2.4, seed + 7, hi=.02)
    h = prof * .8 + dome + tiltx * lx + tilty * ly + off + (g1 - .5) * .09 + (g2 - .5) * .42 + (g3 - .5) * .3
    chip = sstep(.60, .76, fnoise(n, 1.5, seed + 8, hi=.2)) * (1 - t) ** .5
    h -= chip * .5
    pit_f1, _, pc, pk = voronoi(n, 5200, seed + 15)
    pr = np.random.default_rng(seed + 16).random(pk)
    pit = (pit_f1 < 1.6 + 1.6 * pr[pc]) * (pr[pc] > .8) * sstep(.55, .75, fnoise(n, 2.0, seed + 17, hi=.06))
    h -= pit * .22
    cr_f1, cr_f2, _, _ = voronoi(n, 46, seed + 9)
    crack = (1 - sstep(0, 1.8, cr_f2 - cr_f1)) * sstep(.52, .68, fnoise(n, 2.2, seed + 10, hi=.012))
    h -= crack * .6
    mort = 1 - sstep(.0, .45, dist / 3.2)
    h -= mort * .3
    # albedo: weathered greys with brown, blue and warm variation (darker, grimmer)
    palette = np.array([[.30, .285, .255], [.27, .245, .21], [.25, .265, .275], [.33, .305, .27], [.21, .205, .20],
                        [.285, .26, .225], [.18, .18, .185]])
    tone = palette[rng.integers(0, len(palette), nb)] * rng.uniform(.8, 1.2, (nb, 1)) * 1.4
    col = tone[idw]
    shade = .50 + .55 * np.clip(h, -.3, 1.2) + (g1 - .5) * .30
    col = col * shade[..., None]
    grain = fnoise(n, 1.0, seed + 18, hi=.5)
    col *= (.88 + .24 * grain)[..., None]
    streak = fnoise(n, 2.2, seed + 11, ay=7, hi=.1)
    col *= (1 - .38 * sstep(.52, .9, streak))[..., None]
    soot = fnoise(n, 2.6, seed + 19, ay=3, hi=.04)
    col *= (1 - .3 * sstep(.4, .8, soot))[..., None]
    lich = sstep(.60, .78, fnoise(n, 2.4, seed + 12, hi=.05)) * (.4 + .6 * (1 - t))
    col = mix(col, np.array([.17, .20, .105]) * shade[..., None], lich * .5)
    edgeworn = (1 - t) ** 2 * sstep(.3, .8, g2)
    col *= (1 + .35 * edgeworn)[..., None]
    mortc = np.array([.20, .185, .16]) * (.7 + .5 * grain[..., None]) * (.8 + .3 * g2[..., None])
    col = mix(col, mortc, np.clip(mort * 1.05, 0, 1))
    col *= (1 - .3 * (1 - t) ** 3)[..., None]
    col *= (1 - .75 * crack)[..., None]
    col *= (1 - .3 * pit)[..., None]
    rough = np.clip(.9 + .1 * (g1 - .5) - .1 * sstep(.6, .9, streak), .5, 1)
    rough = np.where(mort > .5, 1.0, rough)
    return col, normal_map(h, 5.0), rough, h


# ---------------------------------------------------------------- ground
def gravel_layer(n, k, seed, rmin, rmax, cover, warp=.35):
    """Irregular embedded stones from a voronoi field. Returns dome height, colour tone and coverage mask."""
    f1, f2, cell, kk = voronoi(n, k, seed, grid=True, jit=.95)
    rng = np.random.default_rng(seed + 100)
    sp = n / np.sqrt(kk)
    present = rng.random(kk) < cover
    rad = rng.uniform(rmin, rmax, kk) * sp * .5
    dn = fnoise(n, 1.8, seed + 101, hi=.3)
    d = f1 * (1 + warp * (dn - .5) * 2)
    dome = np.sqrt(np.clip(1 - (d / rad[cell]) ** 2, 0, 1)) * present[cell]
    tone = rng.uniform(.14, .26, kk)[cell]
    warm = rng.uniform(0, 1, kk)[cell]
    return dome, tone, warm


def ground(n=1024, seed=7):
    fine = fnoise(n, 1.0, seed + 2, hi=.5)
    mid = fnoise(n, 1.8, seed, hi=.2)
    low = fnoise(n, 2.6, seed + 1, hi=.03)
    dA = np.array([.145, .12, .092]); dB = np.array([.235, .2, .158])
    col = mix(dA, dB, mid * .6 + low * .4) * (.78 + .44 * fine[..., None])
    h = mid * .22 + fine * .16 + low * .1
    # trampled damp patches: soft edged, fairly small
    mud = sstep(.58, .8, fnoise(n, 2.2, seed + 3, hi=.05)) * .75
    mcol = np.array([.07, .058, .047]) * (.85 + .3 * fnoise(n, 1.6, seed + 13, hi=.15)[..., None])
    col = mix(col, mcol, mud)
    h = h * (1 - .6 * mud)
    # gravel in 3 scales (power-law sizes), colours earthy not white
    cover_mask = np.zeros((n, n))
    for (k, rmin, rmax, cov, sd) in [(5500, .25, .6, .4, 41), (1400, .3, .7, .3, 42), (160, .45, .8, .35, 43)]:
        dome, tone, warm = gravel_layer(n, k, seed + sd, rmin, rmax, cov)
        sc = np.stack([tone * 1.0, tone * (.94 - .05 * warm), tone * (.86 - .1 * warm)], -1) * (.8 + .4 * fine[..., None])
        a = sstep(0, .18, dome)
        shade = (.6 + .55 * dome)[..., None]
        col = mix(col, sc * shade, a[..., None] * (1 - .55 * mud[..., None]))
        # contact darkening just around each stone
        halo = np.clip(blur(a, 2.2) - a, 0, 1)
        col *= (1 - .35 * halo)[..., None]
        h += dome * (.5 if k > 3000 else .8)
        cover_mask = np.maximum(cover_mask, a)
    # dry cracks
    c1, c2, _, _ = voronoi(n, 60, seed + 8)
    crack = (1 - sstep(0, 2.4, c2 - c1)) * sstep(.45, .62, fnoise(n, 2.4, seed + 9, hi=.015)) * (1 - mud)
    col *= (1 - .6 * crack)[..., None]
    h -= crack * .5
    # dead grass blades scattered (flat, pale-brown flecks)
    fl = sstep(.78, .9, fnoise(n, .8, seed + 21, ay=2.5, hi=.5)) * (1 - mud) * (1 - cover_mask)
    col = mix(col, np.array([.27, .23, .15]) * (.8 + .4 * fine[..., None]), fl * .5)
    rough = np.clip(.94 - .06 * fine, 0, 1)
    rough = mix(rough[..., None], (.2 + .22 * fnoise(n, 2.0, seed + 14, hi=.2))[..., None], mud[..., None])[..., 0]
    rough = np.where(cover_mask > .3, .66, rough)
    return col, normal_map(h, 4.5), rough, h


# ---------------------------------------------------------------- cobble
def cobble(n=1024, seed=21):
    f1, f2, cell, k = voronoi(n, 81, seed, grid=True, jit=.85)
    rng = np.random.default_rng(seed + 1)
    d = (f2 - f1) * .5
    d = d + (fnoise(n, 1.8, seed + 2, hi=.15) - .5) * 5
    bev = rng.uniform(10, 18, k)[cell]
    t = np.clip(d / bev, 0, 1)
    prof = np.sqrt(sstep(0, 1, t))
    off = rng.uniform(-.2, .2, k)[cell]
    g1 = fnoise(n, 1.6, seed + 3, hi=.5)
    g2 = fnoise(n, 2.5, seed + 4, hi=.06)
    h = prof * 1.0 + off + (g1 - .5) * .1 + (g2 - .5) * .25
    gap = 1 - sstep(0, .62, t)
    h -= gap * .45
    pal = np.array([[.34, .33, .31], [.29, .30, .30], [.37, .34, .30], [.27, .26, .25], [.33, .30, .26]])
    tone = pal[rng.integers(0, len(pal), k)] * rng.uniform(.7, 1.1, (k, 1)) * .62
    col = tone[cell] * (.55 + .6 * np.clip(h, -.2, 1.1) + (g1 - .5) * .3)[..., None]
    dirt = np.array([.07, .058, .045]) * (.8 + .4 * g1[..., None])
    moss = sstep(.5, .7, fnoise(n, 2.4, seed + 5, hi=.05))
    dirt = mix(dirt, np.array([.08, .1, .045]), moss * .7)
    col = mix(col, dirt, np.clip(gap * 1.15, 0, 1))
    streak = fnoise(n, 2.0, seed + 6, hi=.15)
    col *= (1 - .35 * sstep(.6, .9, streak))[..., None]
    rough = np.where(gap > .5, 1.0, np.clip(.8 + .1 * (g1 - .5), 0, 1))
    return col, normal_map(h, 5.5), rough, h


def preview(items, name):
    ims = []
    for a in items:
        a = (np.clip(a, 0, 1) * 255).astype(np.uint8)
        if a.ndim == 2:
            a = np.stack([a] * 3, -1)
        ims.append(Image.fromarray(a).resize((512, 512), Image.LANCZOS))
    W = sum(i.width for i in ims)
    sheet = Image.new('RGB', (W, 512))
    x = 0
    for i in ims:
        sheet.paste(i, (x, 0)); x += i.width
    sheet.save(os.path.join(OUT, '_prev_' + name + '.png'))


if __name__ == '__main__':
    import sys
    which = sys.argv[1:] or ['stone', 'ground', 'cobble']
    for w in which:
        c, nm, r, h = globals()[w]()
        save_jpg(c, w + '_a', 80); save_jpg(nm, w + '_n', 85); save_jpg(r, w + '_r', 80)
        t2 = np.tile(c, (2, 2, 1))[:1024, :1024]
        preview([c, nm, r], w)
        print(w, 'ok', [os.path.getsize(os.path.join(OUT, w + s + '.jpg')) // 1024 for s in ('_a', '_n', '_r')], 'KB')



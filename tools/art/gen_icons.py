"""Item icons for the game: dark-fantasy, hand-painted look drawn entirely by code (PIL + numpy).

Every icon is 128x128 RGBA (opaque), rendered at 512x512 with 2x mask supersampling and reduced with LANCZOS.
Volume comes from a height map (distance transform of each part's mask) lit from the top-left, not from flat fills.

Run from the repository root:
    python -X utf8 tools/art/gen_icons.py                      # all icons -> unity/Assets/Game/Art/Icons/<ID>.png
    python -X utf8 tools/art/gen_icons.py --only I009,I00A     # a few ids
    python -X utf8 tools/art/gen_icons.py --sheets .local/work # also write contact sheets (12 columns) and a 64 px sheet

Input: tools/art/items-for-icons.json (id, name, role, price, scrollFor). Output is deterministic (fixed seeds derived
from the item id; the PNG bytes do not depend on the worker count). scipy is optional: without it (or with the
environment variable GEN_ICONS_NO_SCIPY=1) pure numpy fallbacks are used.

How it works:
  * DESIGNS maps an item name to an archetype function and its options. Levels ("I", "II") reuse the design of the base
    name and get vertical tick marks in the corner. Richness (detail, glow) grows with the price unless overridden.
  * An archetype draws parts on a Ctx in a 0..100 design board (Ctx.layer: polygon -> lit material, Ctx.decal: engraving,
    emissive inlays, gems, orbs). Ctx.fit centres the result, Ctx.finish adds the dark background, shadow and rim light.
  * Recipe scrolls (build_scroll) paste a miniature of the target item onto a parchment with a red wax seal.
"""
import argparse
import contextlib
import json
import math
import os
import re
import sys
import zlib

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

try:
    if os.environ.get('GEN_ICONS_NO_SCIPY'):      # lets you test the pure numpy fallbacks
        raise ImportError
    from scipy import ndimage as ndi
except ImportError:  # numpy fallbacks below keep the script runnable without scipy
    ndi = None

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
ITEMS_JSON = os.path.join(HERE, 'items-for-icons.json')
OUT_DIR = os.path.join(ROOT, 'unity', 'Assets', 'Game', 'Art', 'Icons')

W = 512          # working resolution
OUT = 128        # final resolution
MS = 2           # mask supersampling inside the working resolution
U = W / 100.0    # pixels per design unit (items are designed on a 0..100 board)
MARGIN = 30      # extra pixels around a part for shadows/blur


# ------------------------------------------------------------------ numeric helpers

def smoothstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)


def mixc(a, b, t):
    return a + (b - a) * t


def gblur(a, sigma):
    if sigma <= 0.05:
        return a
    if ndi is not None:
        return ndi.gaussian_filter(a, sigma, mode='constant')
    rad = max(1, int(sigma * 3))
    k = np.exp(-0.5 * (np.arange(-rad, rad + 1) / sigma) ** 2)
    k /= k.sum()
    for ax in (0, 1):
        pad = [(0, 0)] * a.ndim
        pad[ax] = (rad, rad)
        p = np.pad(a, pad)
        out = np.zeros_like(a)
        for i, kv in enumerate(k):
            sl = [slice(None)] * a.ndim
            sl[ax] = slice(i, i + a.shape[ax])
            out += kv * p[tuple(sl)]
        a = out
    return a


def edt(mask):
    if ndi is not None:
        return ndi.distance_transform_edt(mask)
    d = np.zeros(mask.shape, np.float32)
    cur = mask.copy()
    i = 0
    while cur.any() and i < 400:
        i += 1
        d += cur
        p = np.pad(cur, 1)
        er = p[1:-1, 1:-1] & p[:-2, 1:-1] & p[2:, 1:-1] & p[1:-1, :-2] & p[1:-1, 2:]
        if i % 2:
            er &= p[:-2, :-2] & p[:-2, 2:] & p[2:, :-2] & p[2:, 2:]
        cur = er
    return d.astype(np.float64) * 1.0


def _slide_max(a, r, ax):
    """Maximum over [-r, r] along one axis by repeated doubling."""
    n = a.shape[ax]
    pad = [(0, 0), (0, 0)]
    pad[ax] = (r, r)
    cur = np.pad(a, pad, mode='constant', constant_values=float(a.min()))
    L = 2 * r + 1
    w = 1
    while w * 2 <= L:
        la = [slice(None), slice(None)]
        lb = [slice(None), slice(None)]
        la[ax] = slice(0, cur.shape[ax] - w)
        lb[ax] = slice(w, cur.shape[ax])
        cur = np.maximum(cur[tuple(la)], cur[tuple(lb)])
        w *= 2
    la = [slice(None), slice(None)]
    lb = [slice(None), slice(None)]
    la[ax] = slice(0, n)
    lb[ax] = slice(L - w, L - w + n)
    return np.maximum(cur[tuple(la)], cur[tuple(lb)])


def maxfilt(a, size):
    if ndi is not None:
        return ndi.maximum_filter(a, size=size)
    r = size // 2
    return _slide_max(_slide_max(a, r, 0), r, 1)


def shift2d(a, dx, dy):
    out = np.zeros_like(a)
    h, w = a.shape[:2]
    xs, xd = (slice(0, w - dx), slice(dx, w)) if dx >= 0 else (slice(-dx, w), slice(0, w + dx))
    ys, yd = (slice(0, h - dy), slice(dy, h)) if dy >= 0 else (slice(-dy, h), slice(0, h + dy))
    out[yd, xd] = a[ys, xs]
    return out


def bilinear(a, x, y):
    h, w = a.shape
    x = np.clip(x, 0, w - 1.001)
    y = np.clip(y, 0, h - 1.001)
    x0 = x.astype(np.int32)
    y0 = y.astype(np.int32)
    fx = (x - x0).astype(np.float32)
    fy = (y - y0).astype(np.float32)
    return (a[y0, x0] * (1 - fx) * (1 - fy) + a[y0, x0 + 1] * fx * (1 - fy) + a[y0 + 1, x0] * (1 - fx) * fy + a[y0 + 1, x0 + 1] * fx * fy)


def lut_from_stops(stops, n=256):
    ts = np.array([s[0] for s in stops], np.float64)
    cs = np.array([s[1] for s in stops], np.float64)
    x = np.linspace(0, 1, n)
    return np.stack([np.interp(x, ts, cs[:, i]) for i in range(3)], 1).astype(np.float32)


# ------------------------------------------------------------------ noise (cached per process)

_NC = {}


def vnoise(scale, seed, sx=None, sy=None, angle=0.0):
    """Smooth value noise covering the whole 512 canvas, ~N(0.5, small), cached."""
    sx = sx or scale
    sy = sy or scale
    key = (round(sx, 2), round(sy, 2), seed, round(angle, 1))
    if key in _NC:
        return _NC[key]
    big = W if angle == 0 else int(W * 1.5)
    rng = np.random.default_rng(seed)
    gw, gh = int(big / sx) + 3, int(big / sy) + 3
    g = rng.random((gh, gw)).astype(np.float32)
    im = Image.fromarray(g, 'F').resize((big, big), Image.BICUBIC)
    if angle:
        im = im.rotate(angle, resample=Image.BICUBIC)
        o = (big - W) // 2
        im = im.crop((o, o, o + W, o + W))
    a = np.asarray(im, dtype=np.float32)
    a = (a - a.mean()) / (a.std() + 1e-6) * 0.22 + 0.5
    _NC[key] = a
    return a


# ------------------------------------------------------------------ geometry (design units 0..100)

def _arr(p):
    return np.asarray(p, dtype=np.float64)


def bez(pts, n=28):
    P = _arr(pts)
    t = np.linspace(0, 1, n)[:, None]
    k = len(P) - 1
    out = 0
    for i in range(k + 1):
        out = out + math.comb(k, i) * (1 - t) ** (k - i) * t ** i * P[i]
    return out


def stroke(pts, w=1.0, wr=None, caps='flat'):
    """Polygon around a polyline; w is the left half-width (scalar or per point), wr the right one."""
    P = _arr(pts)
    N = len(P)
    wl = np.broadcast_to(np.asarray(w, float), (N,)).copy()
    wr_ = wl.copy() if wr is None else np.broadcast_to(np.asarray(wr, float), (N,)).copy()
    T = np.gradient(P, axis=0)
    T /= np.linalg.norm(T, axis=1, keepdims=True) + 1e-9
    Nn = np.stack([-T[:, 1], T[:, 0]], 1)
    Lp = P + Nn * wl[:, None]
    Rp = P - Nn * wr_[:, None]
    pts_out = list(Lp)
    if caps == 'round':
        th = math.atan2(T[-1, 1], T[-1, 0])
        r = (wl[-1] + wr_[-1]) / 2
        c = P[-1] + Nn[-1] * (wl[-1] - wr_[-1]) / 2
        pts_out += [c + r * np.array([math.cos(th + a), math.sin(th + a)]) for a in np.linspace(math.pi / 2, -math.pi / 2, 9)[1:-1]]
    pts_out += list(Rp[::-1])
    if caps == 'round':
        th = math.atan2(T[0, 1], T[0, 0])
        r = (wl[0] + wr_[0]) / 2
        c = P[0] + Nn[0] * (wl[0] - wr_[0]) / 2
        pts_out += [c + r * np.array([math.cos(th - math.pi / 2 - a), math.sin(th - math.pi / 2 - a)]) for a in np.linspace(0, math.pi, 9)[1:-1]]
    return _arr(pts_out)


def line_poly(p0, p1, w, caps='round', n=2):
    p0, p1 = _arr(p0), _arr(p1)
    pts = np.linspace(p0, p1, n)
    return stroke(pts, w, caps=caps)


def ellipse_poly(cx, cy, rx, ry=None, rot=0.0, n=56, a0=0.0, a1=360.0):
    ry = rx if ry is None else ry
    a = np.radians(np.linspace(a0, a1, n, endpoint=(a1 - a0) < 360))
    x, y = rx * np.cos(a), ry * np.sin(a)
    c, s = math.cos(math.radians(rot)), math.sin(math.radians(rot))
    return np.stack([cx + x * c - y * s, cy + x * s + y * c], 1)


def circle_poly(cx, cy, r, n=48):
    return ellipse_poly(cx, cy, r, r, 0, n)


def rect_poly(x0, y0, x1, y1):
    return _arr([(x0, y0), (x1, y0), (x1, y1), (x0, y1)])


def rrect_poly(x0, y0, x1, y1, r, n=7):
    r = min(r, (x1 - x0) / 2, (y1 - y0) / 2)
    out = []
    for cx, cy, a in ((x1 - r, y0 + r, -90), (x1 - r, y1 - r, 0), (x0 + r, y1 - r, 90), (x0 + r, y0 + r, 180)):
        for t in np.linspace(a, a + 90, n):
            out.append((cx + r * math.cos(math.radians(t)), cy + r * math.sin(math.radians(t))))
    return _arr(out)


def sym_poly(half, cx=50.0):
    """Right-half points (top to bottom) -> full symmetric polygon about x=cx."""
    h = _arr(half)
    m = h[::-1].copy()
    m[:, 0] = 2 * cx - m[:, 0]
    return np.vstack([h, m])


def star_poly(cx, cy, ro, ri, n=5, rot=-90.0):
    out = []
    for i in range(n * 2):
        r = ro if i % 2 == 0 else ri
        a = math.radians(rot + i * 180.0 / n)
        out.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return _arr(out)


def blob_poly(cx, cy, r, rng, n=36, jitter=0.12, squash=1.0):
    a = np.linspace(0, 2 * math.pi, n, endpoint=False)
    rr = r * (1 + rng.uniform(-jitter, jitter, n))
    rr = (rr + np.roll(rr, 1) + np.roll(rr, -1)) / 3
    return np.stack([cx + rr * np.cos(a), cy + squash * rr * np.sin(a)], 1)


class Shape:
    """Ordered polygon ops in local design units: add / sub / and (and = intersect with the union of polys)."""
    __slots__ = ('ops',)

    def __init__(self, ops=None):
        self.ops = ops or []

    @staticmethod
    def of(*polys):
        return Shape([('add', _arr(p)) for p in polys])

    def __add__(self, o):
        return Shape(self.ops + o.ops)

    def minus(self, *others):
        ops = list(self.ops)
        for o in others:
            ops += [('sub', p) for k, p in o.ops if k == 'add']
        return Shape(ops)

    def clip(self, o):
        return Shape(self.ops + [('and', [p for k, p in o.ops if k == 'add'])])


def S(*polys):
    return Shape.of(*polys)


# ------------------------------------------------------------------ materials

def _ramp(c, lo=0.07, hi=1.55, lift=0.16):
    c = np.asarray(c, float)
    return [(0.0, np.clip(c * lo, 0, 1)), (0.22, np.clip(c * 0.32, 0, 1)), (0.5, np.clip(c * 0.78, 0, 1)),
            (0.76, np.clip(c * 1.18 + 0.04, 0, 1)), (1.0, np.clip(c * hi + lift, 0, 1))]


_RAW = {
    # kind metal: reflective; matte: diffuse; wax/gem handled specially
    'steel': dict(kind='metal', stops=[(0, (.018, .02, .026)), (.16, (.07, .078, .095)), (.38, (.23, .25, .29)), (.6, (.50, .53, .58)), (.82, (.80, .83, .87)), (1, (.98, .985, 1))],
                  spec=.14, shin=26, scratch=.55, rust=.07, wear=.5, brushed=.10),
    'darksteel': dict(kind='metal', stops=[(0, (.012, .013, .018)), (.2, (.06, .065, .085)), (.45, (.17, .18, .22)), (.7, (.35, .37, .45)), (1, (.78, .82, .9))],
                      spec=.12, shin=26, scratch=.45, rust=.03, wear=.4, brushed=.08),
    'iron': dict(kind='metal', stops=[(0, (.02, .018, .016)), (.2, (.075, .07, .065)), (.45, (.21, .195, .18)), (.7, (.42, .39, .35)), (1, (.78, .73, .66))],
                 spec=.10, shin=20, scratch=.6, rust=.20, wear=.55, brushed=.08),
    'bronze': dict(kind='metal', stops=[(0, (.03, .02, .01)), (.18, (.12, .07, .03)), (.42, (.34, .2, .075)), (.66, (.62, .42, .17)), (.86, (.88, .70, .4)), (1, (1, .93, .72))],
                   spec=.22, shin=26, scratch=.4, rust=.05, wear=.4, dentmat=.35),
    'darkbronze': dict(kind='metal', stops=[(0, (.02, .015, .01)), (.18, (.07, .05, .03)), (.42, (.19, .13, .07)), (.66, (.38, .28, .14)), (.86, (.62, .5, .3)), (1, (.85, .75, .52))],
                       spec=.16, shin=22, scratch=.4, rust=.06, wear=.5),
    'gold': dict(kind='metal', stops=[(0, (.04, .03, .01)), (.18, (.16, .1, .03)), (.42, (.44, .3, .07)), (.66, (.76, .56, .17)), (.86, (.96, .83, .42)), (1, (1, .97, .78))],
                 spec=.34, shin=36, scratch=.3, rust=0, wear=.3, dentmat=.45),
    'silver': dict(kind='metal', stops=[(0, (.03, .035, .04)), (.2, (.11, .12, .135)), (.45, (.36, .38, .42)), (.7, (.68, .71, .75)), (1, (1, 1, 1))],
                   spec=.22, shin=36, scratch=.3, rust=0, wear=.3, brushed=.06),
    'mithril': dict(kind='metal', stops=[(0, (.02, .03, .04)), (.2, (.07, .1, .13)), (.45, (.26, .36, .44)), (.7, (.58, .72, .82)), (1, (.95, 1, 1))],
                    spec=.22, shin=38, scratch=.3, rust=0, wear=.3, brushed=.06),
    'mithrilmail': dict(kind='metal', stops=[(0, (.02, .03, .04)), (.2, (.07, .1, .13)), (.45, (.26, .36, .44)), (.7, (.58, .72, .82)), (1, (.95, 1, 1))],
                        spec=.18, shin=30, scratch=.2, rust=0, wear=.2, mail=True),
    'blackiron': dict(kind='metal', stops=[(0, (.008, .008, .01)), (.2, (.035, .035, .042)), (.45, (.1, .1, .12)), (.7, (.25, .25, .3)), (1, (.62, .62, .72))],
                      spec=.12, shin=26, scratch=.35, rust=.04, wear=.35, brushed=.06),
    'hotiron': dict(kind='metal', stops=[(0, (.02, .012, .01)), (.2, (.075, .05, .042)), (.45, (.19, .135, .11)), (.7, (.4, .3, .25)), (1, (.8, .68, .6))],
                    spec=.12, shin=22, scratch=.5, rust=.14, wear=.6, brushed=.08),
    'rustiron': dict(kind='metal', stops=[(0, (.02, .012, .01)), (.2, (.09, .05, .035)), (.45, (.26, .15, .09)), (.7, (.48, .28, .16)), (1, (.8, .55, .36))],
                     spec=.15, shin=14, scratch=.5, rust=.4, wear=.5),
    'verdigris': dict(kind='metal', stops=[(0, (.012, .025, .02)), (.2, (.04, .09, .075)), (.45, (.12, .26, .21)), (.7, (.27, .5, .41)), (1, (.62, .85, .72))],
                      spec=.25, shin=18, scratch=.3, rust=0, wear=.3),
    'wood': dict(kind='matte', stops=[(0, (.018, .012, .009)), (.2, (.07, .047, .03)), (.45, (.17, .115, .072)), (.7, (.32, .23, .15)), (1, (.52, .4, .28))],
                 spec=.05, shin=10, grain=True),
    'darkwood': dict(kind='matte', stops=[(0, (.012, .008, .006)), (.2, (.05, .034, .026)), (.45, (.12, .082, .06)), (.7, (.23, .165, .12)), (1, (.4, .3, .22))],
                     spec=.06, shin=12, grain=True),
    'leather': dict(kind='matte', stops=[(0, (.018, .011, .008)), (.2, (.075, .046, .03)), (.45, (.175, .11, .07)), (.7, (.32, .21, .135)), (1, (.5, .37, .26))],
                    spec=.12, shin=12, leather=True),
    'blackleather': dict(kind='matte', stops=[(0, (.008, .008, .01)), (.2, (.035, .033, .036)), (.45, (.085, .08, .085)), (.7, (.17, .16, .16)), (1, (.32, .3, .3))],
                         spec=.18, shin=14, leather=True),
    'redleather': dict(kind='matte', stops=[(0, (.02, .006, .006)), (.2, (.1, .02, .02)), (.45, (.26, .05, .045)), (.7, (.44, .1, .08)), (1, (.66, .24, .17))],
                       spec=.14, shin=12, leather=True),
    'bone': dict(kind='matte', stops=[(0, (.06, .05, .04)), (.2, (.2, .17, .13)), (.45, (.47, .42, .33)), (.7, (.74, .69, .57)), (1, (.95, .91, .8))],
                 spec=.12, shin=18),
    'stone': dict(kind='matte', stops=[(0, (.015, .015, .018)), (.2, (.06, .06, .07)), (.45, (.16, .16, .18)), (.7, (.32, .32, .35)), (1, (.55, .55, .6))],
                  spec=.05, shin=10, stone=True),
    'parchment': dict(kind='matte', stops=[(0, (.10, .07, .04)), (.25, (.30, .22, .13)), (.5, (.52, .42, .27)), (.75, (.70, .60, .42)), (1, (.84, .76, .57))],
                      spec=0, shin=8, paper=True),
    'wax': dict(kind='matte', stops=[(0, (.05, .008, .008)), (.2, (.22, .025, .025)), (.45, (.45, .05, .045)), (.7, (.66, .11, .09)), (1, (.95, .42, .34))],
                spec=.55, shin=28),
    'blood': dict(kind='matte', stops=[(0, (.03, .0, .0)), (.3, (.2, .01, .01)), (.6, (.45, .03, .03)), (1, (.85, .15, .12))], spec=.5, shin=30),
    'cloth': dict(kind='matte', stops=_ramp((.3, .25, .3)), spec=0, shin=8, cloth=True),
}
_MATS = {}


def get_mat(m):
    if isinstance(m, dict):
        return m
    if m in _MATS:
        return _MATS[m]
    d = dict(_RAW[m])
    d['lut'] = lut_from_stops(d['stops'])
    d['name'] = m
    _MATS[m] = d
    return d


def tinted(rgb, kind='matte', **kw):
    """Custom material from one base colour (cloth, glass, painted bits)."""
    key = ('t', tuple(round(float(v), 3) for v in rgb), kind, tuple(sorted(kw.items())))
    if key in _MATS:
        return _MATS[key]
    d = dict(kind=kind, stops=_ramp(rgb), spec=kw.get('spec', 0.05), shin=kw.get('shin', 12))
    if kind == 'metal':
        d.update(scratch=0.3, rust=0.0, wear=0.3)
    d.update(kw)
    d['lut'] = lut_from_stops(d['stops'])
    d['name'] = str(key)
    _MATS[key] = d
    return d


def gem_mat(rgb):
    c = np.asarray(rgb, float)
    stops = [(0.0, c * 0.05), (0.3, c * 0.38), (0.6, c * 0.85), (0.85, np.clip(c * 1.2 + 0.1, 0, 1)), (1.0, np.clip(c * 0.6 + 0.65, 0, 1))]
    return dict(kind='gem', lut=lut_from_stops(stops), name='gem' + str(tuple(np.round(c, 2))), rgb=c)


def orb_mat(c1, c2, mode='glass', swirl=0.8, twist=0.6, nscale=13.0, shine=1.0, seed=0):
    return dict(kind='orb', name='orb' + str((tuple(np.round(c1, 2)), tuple(np.round(c2, 2)), mode, seed)), c1=np.asarray(c1, np.float32),
                c2=np.asarray(c2, np.float32), mode=mode, swirl=swirl, twist=twist, nscale=nscale, shine=shine, seed=seed)


def emit_mat(stops):
    return dict(kind='emit', lut=lut_from_stops(stops), name='emit')


FIRE = emit_mat([(0, (.35, .03, .01)), (.3, (.8, .17, .03)), (.6, (1, .5, .08)), (.85, (1, .8, .35)), (1, (1, .96, .78))])
ICEGLOW = emit_mat([(0, (.05, .2, .35)), (.4, (.25, .6, .85)), (.75, (.65, .9, 1)), (1, (.95, 1, 1))])
ARCANE = emit_mat([(0, (.08, .1, .35)), (.4, (.25, .35, .85)), (.75, (.6, .72, 1)), (1, (.96, .98, 1))])
POISON = emit_mat([(0, (.05, .2, .02)), (.4, (.25, .55, .06)), (.75, (.6, .88, .25)), (1, (.92, 1, .7))])
HOLY = emit_mat([(0, (.55, .35, .08)), (.4, (.9, .7, .28)), (.75, (1, .92, .6)), (1, (1, 1, .92))])
VOID = emit_mat([(0, (.15, .04, .28)), (.4, (.4, .15, .65)), (.75, (.7, .45, .92)), (1, (.95, .85, 1))])
BLOODG = emit_mat([(0, (.3, .01, .02)), (.4, (.65, .05, .06)), (.75, (.95, .25, .2)), (1, (1, .7, .55))])
BOLT = emit_mat([(0, (.15, .3, .7)), (.4, (.4, .65, 1)), (.75, (.8, .92, 1)), (1, (1, 1, 1))])
SOUL = emit_mat([(0, (.05, .3, .25)), (.4, (.2, .65, .55)), (.75, (.6, .95, .85)), (1, (.95, 1, .98))])

# element -> (glow colour, emit material, accent colour, background tint)
ELEM = {
    'fire': ((1.0, .42, .08), FIRE, (.92, .33, .06), 'warm'),
    'ice': ((.35, .75, 1.0), ICEGLOW, (.45, .8, .95), 'cold'),
    'poison': ((.5, .9, .2), POISON, (.45, .75, .15), 'green'),
    'holy': ((1.0, .85, .45), HOLY, (.95, .8, .4), 'gold'),
    'arcane': ((.35, .5, 1.0), ARCANE, (.35, .5, .95), 'cold'),
    'shadow': ((.6, .3, .9), VOID, (.5, .28, .75), 'violet'),
    'blood': ((.9, .1, .1), BLOODG, (.7, .07, .08), 'crimson'),
    'bolt': ((.55, .75, 1.0), BOLT, (.55, .75, 1.0), 'cold'),
    'soul': ((.3, .9, .75), SOUL, (.35, .8, .65), 'green'),
    'none': ((.8, .75, .6), HOLY, (.7, .6, .4), None),
}

BG_TINTS = {
    None: ((.072, .074, .082), (.03, .032, .04)),
    'warm': ((.095, .072, .055), (.04, .03, .024)),
    'cold': ((.05, .068, .095), (.02, .028, .044)),
    'green': ((.055, .082, .06), (.022, .034, .026)),
    'violet': ((.078, .056, .1), (.034, .024, .046)),
    'crimson': ((.1, .052, .052), (.045, .02, .02)),
    'gold': ((.095, .084, .06), (.04, .035, .026)),
}

LIGHT = np.array([-0.50, -0.62, 0.62])
LIGHT = LIGHT / np.linalg.norm(LIGHT)
FILL = np.array([0.62, 0.52, 0.45])
FILL = FILL / np.linalg.norm(FILL)


class Layer:
    """Handle to a rendered part: its soft mask in canvas crop coordinates."""

    def __init__(self, m, box):
        self.m, self.box = m, box


# ------------------------------------------------------------------ drawing context

class Ctx:
    def __init__(self, seed, bare=False):
        self.seed = seed
        self.rng = np.random.default_rng(seed)
        self.PC = np.zeros((W, W, 3), np.float32)   # premultiplied colour
        self.A = np.zeros((W, W), np.float32)
        self.GL = np.zeros((W, W, 3), np.float32)   # additive glow
        self.M = np.eye(3)
        self.tint = None
        self.aura = None
        self.scr = None
        self.bare = bare
        self._grain_cache = {}

    # --- transforms
    @contextlib.contextmanager
    def local(self, rot=0.0, pivot=(50.0, 50.0), scale=1.0, move=(0.0, 0.0), flip=False):
        old = self.M
        a = math.radians(rot)
        c, s = math.cos(a) * scale, math.sin(a) * scale
        fx = -1 if flip else 1
        R = np.array([[c * fx, -s, 0], [s * fx, c, 0], [0, 0, 1.0]])
        T1 = np.array([[1, 0, pivot[0] + move[0]], [0, 1, pivot[1] + move[1]], [0, 0, 1.0]])
        T0 = np.array([[1, 0, -pivot[0]], [0, 1, -pivot[1]], [0, 0, 1.0]])
        self.M = old @ T1 @ R @ T0
        try:
            yield
        finally:
            self.M = old

    @contextlib.contextmanager
    def reset(self):
        old = self.M
        self.M = np.eye(3)
        try:
            yield
        finally:
            self.M = old

    def tx(self, P):
        P = _arr(P).reshape(-1, 2)
        out = P @ self.M[:2, :2].T + self.M[:2, 2]
        return out * U

    def grain_angle(self, axis='y'):
        v = np.array([0.0, 1.0]) if axis == 'y' else np.array([1.0, 0.0])
        w = self.M[:2, :2] @ v
        return math.degrees(math.atan2(w[1], w[0]))

    # --- rasterising
    def raster(self, shape, margin=MARGIN):
        txd = []
        xs, ys = [], []
        for kind, p in shape.ops:
            if kind == 'and':
                txd.append((kind, [self.tx(q) for q in p]))
            else:
                q = self.tx(p)
                txd.append((kind, q))
                if kind == 'add':
                    xs += [q[:, 0].min(), q[:, 0].max()]
                    ys += [q[:, 1].min(), q[:, 1].max()]
        if not xs:
            return None
        x0 = max(0, int(math.floor(min(xs) - margin)))
        y0 = max(0, int(math.floor(min(ys) - margin)))
        x1 = min(W, int(math.ceil(max(xs) + margin)))
        y1 = min(W, int(math.ceil(max(ys) + margin)))
        if x1 - x0 < 2 or y1 - y0 < 2:
            return None
        cw, ch = x1 - x0, y1 - y0
        im = Image.new('L', (cw * MS, ch * MS), 0)
        d = ImageDraw.Draw(im)
        andmasks = []
        for kind, p in txd:
            if kind == 'and':
                t = Image.new('L', im.size, 0)
                td = ImageDraw.Draw(t)
                for q in p:
                    td.polygon([((x - x0) * MS, (y - y0) * MS) for x, y in q], fill=255)
                andmasks.append(t)
                continue
            d.polygon([((x - x0) * MS, (y - y0) * MS) for x, y in p], fill=255 if kind == 'add' else 0)
        m = np.asarray(im.resize((cw, ch), Image.BOX), dtype=np.float32) / 255.0
        for t in andmasks:
            m *= np.asarray(t.resize((cw, ch), Image.BOX), dtype=np.float32) / 255.0
        return m, (x0, y0, x1, y1)

    # --- height from mask
    @staticmethod
    def _height(m, relief, r, k):
        ins = m > 0.35
        if not ins.any():
            return np.zeros_like(m), np.zeros_like(m), 1.0
        dt = edt(ins).astype(np.float32)
        mx = float(dt.max())
        if relief == 'roof':
            h = (dt + 0.8 * np.minimum(dt, 3.0)) * k
        elif relief == 'bevel':
            rr = (r * U) if r else 0.3 * mx
            h = np.minimum(dt, rr) * k
        elif relief == 'flat':
            h = np.minimum(dt, 2.5) * k
        elif relief == 'round':
            if r:
                rr = r * U
                t = np.clip(dt / rr, 0, 1)
                h = np.sqrt(1 - (1 - t) ** 2) * rr * k
            else:
                ridge = maxfilt(dt, max(3, int(mx * 1.25) | 1))
                t = np.clip(dt / np.maximum(ridge, 1e-3), 0, 1)
                h = np.sqrt(1 - (1 - t) ** 2) * ridge * k
        else:
            raise ValueError(relief)
        return gblur(h, 0.8) * (m > 0.02), dt, mx

    def _grain(self, mat, box, axis, scale=1.0):
        ang = self.grain_angle(axis)
        n = vnoise(0, 77, sx=70.0 * scale, sy=2.4 * scale, angle=ang)
        n2 = vnoise(0, 78, sx=22.0 * scale, sy=1.3 * scale, angle=ang)
        x0, y0, x1, y1 = box
        return (n[y0:y1, x0:x1] - 0.5) * 1.0 + (n2[y0:y1, x0:x1] - 0.5) * 0.6

    # --- shading
    def _shade(self, m, box, mat, relief, r, k, ao, wear, edge, grain_axis, dent, ink=0.5):
        x0, y0, x1, y1 = box
        h, dt, mx = self._height(m, relief, r, k)
        lo = vnoise(60, 11)[y0:y1, x0:x1]
        mid = vnoise(14, 12)[y0:y1, x0:x1]
        fine = vnoise(3.2, 13)[y0:y1, x0:x1]
        kind = mat['kind']
        dent = dent or mat.get('dentmat', 0.0)
        if dent:
            h = h + (mid - 0.5) * dent * 3.0 + (fine - 0.5) * dent * 1.2
        if mat.get('stone'):
            h = h + (mid - 0.5) * 2.2 + (fine - 0.5) * 1.6
        if mat.get('leather'):
            h = h + (fine - 0.5) * 0.7 + (vnoise(6.5, 14)[y0:y1, x0:x1] - 0.5) * 1.0
        if mat.get('cloth'):
            h = h + (vnoise(2.4, 15)[y0:y1, x0:x1] - 0.5) * 1.1
        mailp = None
        if mat.get('mail'):
            yy_, xx_ = np.mgrid[y0:y1, x0:x1].astype(np.float32)
            row = np.floor(yy_ / 6.0)
            u = xx_ / 11.0 + (row % 2) * 0.5
            mailp = (0.5 + 0.5 * np.cos(2 * np.pi * u)) * (0.5 + 0.5 * np.cos(2 * np.pi * yy_ / 12.0))
            h = h + mailp * 2.2
        gy, gx = np.gradient(h)
        nz = 1.0 / np.sqrt(gx * gx + gy * gy + 1.0)
        nx, ny = -gx * nz, -gy * nz
        diff = np.clip(nx * LIGHT[0] + ny * LIGHT[1] + nz * LIGHT[2], 0, 1)
        fill = np.clip(nx * FILL[0] + ny * FILL[1] + nz * FILL[2], 0, 1)
        if kind == 'metal':
            rx, ry, rz = 2 * nx * nz, 2 * ny * nz, 2 * nz * nz - 1
            # chrome-like environment: bright sky up-left, dark horizon band, dim bounce light below
            s_ = -ry * 0.9 - rx * 0.45 + 0.35 + (lo - 0.5) * 0.9
            env = np.where(s_ >= 0, 0.07 + 0.88 * smoothstep(0.0, 0.7, s_), 0.07 + 0.30 * smoothstep(0.0, -0.75, s_))
            t = 0.02 + 0.56 * env + 0.22 * diff ** 1.3 + 0.05 * fill ** 2
            spec = np.clip(rx * LIGHT[0] + ry * LIGHT[1] + rz * LIGHT[2], 0, 1) ** mat['shin'] * mat['spec']
        else:
            t = 0.12 + 0.88 * np.clip((nx * LIGHT[0] + ny * LIGHT[1] + nz * LIGHT[2] + 0.18) / 1.18, 0, 1) + 0.10 * fill ** 1.5
            hv = np.array([LIGHT[0], LIGHT[1], LIGHT[2] + 1.0])
            hv /= np.linalg.norm(hv)
            spec = np.clip(nx * hv[0] + ny * hv[1] + nz * hv[2], 0, 1) ** mat['shin'] * mat['spec']
        # material texture on the tone
        if mat.get('grain'):
            t = t + self._grain(mat, box, grain_axis) * 0.22
        if mat.get('paper'):
            t = t + (mid - 0.5) * 0.24 + (fine - 0.5) * 0.09
        if mat.get('cloth'):
            t = t + (vnoise(2.4, 15)[y0:y1, x0:x1] - 0.5) * 0.22
        if mailp is not None:
            t = t + (mailp - 0.5) * 0.30
        if kind == 'metal':
            t = t + (lo - 0.5) * 0.10 + (mid - 0.5) * 0.08 + (fine - 0.5) * 0.06
            if mat.get('brushed'):
                t = t + self._grain(mat, box, grain_axis) * mat['brushed']
            sc = self.scr[y0:y1, x0:x1] if self.scr is not None else 0
            t = t + sc * mat['scratch'] * 0.26
            ew = np.exp(-dt / 1.6) * smoothstep(0.42, 0.7, mid) * mat['wear'] * wear
            t = t + ew * 0.22
        else:
            t = t + (lo - 0.5) * 0.22 + (fine - 0.5) * 0.07
        t = np.clip(t, 0, 1)
        col = mat['lut'][(t * 255).astype(np.int32)]
        if kind == 'metal':
            rust = mat['rust'] * wear
            if rust > 0:
                nzr = lo * 0.25 + mid * 0.45 + fine * 0.30
                rm = smoothstep(0.80 - rust * 0.55, 0.97 - rust * 0.35, nzr) * min(rust * 3.0, 0.62)
                rm = rm * (0.30 + 0.70 * np.exp(-dt / 14.0))
                rc = np.array([0.36, 0.165, 0.07], np.float32) * (0.30 + 0.70 * t[..., None])
                col = mixc(col, rc, rm[..., None])
        col = col + spec[..., None] * np.array([1.0, 0.97, 0.9], np.float32) * (1.0 if kind == 'metal' else 0.9)
        if ao > 0:
            cav = np.clip((gblur(h, 5.0) - h) / 7.0, 0, 1)
            col = col * (1 - ao * cav)[..., None]
        if edge:
            ec, ew_, es = edge
            f = np.exp(-dt / (ew_ * U)) * es
            col = mixc(col, np.asarray(ec, np.float32), f[..., None])
        if ink:
            col = col * (1 - ink * np.exp(-np.clip(dt - 0.5, 0, None) / 2.4))[..., None]
        return np.clip(col, 0, 1.2)

    def _emit(self, m, box, mat, field):
        ins = m > 0.35
        dt = edt(ins).astype(np.float32) if ins.any() else np.zeros_like(m)
        mx = max(float(dt.max()), 1.0)
        t = np.clip(dt / (mx * 0.85), 0, 1) ** 0.75
        if field is not None:
            t = np.clip(field(box, dt, mx, t), 0, 1)
        return mat['lut'][(t * 255).astype(np.int32)]

    # --- main layer call
    def layer(self, shape, mat, relief='round', r=None, k=0.7, sh=1.0, glow=None, op=1.0, ao=0.35, wear=1.0,
              edge=None, field=None, grain='y', dent=0.0, ink=0.5):
        R = self.raster(shape)
        if R is None:
            return None
        m, box = R
        x0, y0, x1, y1 = box
        mat = get_mat(mat)
        if mat['kind'] == 'emit':
            col = self._emit(m, box, mat, field)
        elif mat['kind'] == 'gem':
            col = self._gem_flat(m, box, mat)
        elif mat['kind'] == 'orb':
            col = self._orb_flat(m, box, mat)
        else:
            col = self._shade(m, box, mat, relief, r, k, ao, wear, edge, grain, dent, ink)
        a = (m * op)
        subPC, subA = self.PC[y0:y1, x0:x1], self.A[y0:y1, x0:x1]
        if sh > 0 and not self.bare:
            s = gblur(shift2d(m, int(round(4 * sh)), int(round(6 * sh))), 4.2 * sh + 0.4)
            subPC *= (1 - 0.62 * s * op)[..., None]
        subPC[:] = col * a[..., None] + subPC * (1 - a[..., None])
        subA[:] = a + subA * (1 - a)
        if glow:
            self.add_glow(m, box, *glow)
        return Layer(m, box)

    def add_glow(self, m, box, color, sigma, inten):
        x0, y0, x1, y1 = box
        # blur on the whole box; sigma in design units
        g = gblur(m, sigma * U)
        self.GL[y0:y1, x0:x1] += g[..., None] * np.asarray(color, np.float32) * inten

    def glow_shape(self, shape, color, sigma, inten):
        big = max(MARGIN, int(sigma * U * 3))
        R = self.raster(shape, margin=big)
        if R is None:
            return
        m, box = R
        self.add_glow(m, box, color, sigma, inten)

    def mask_of(self, shape):
        R = self.raster(shape)
        return R

    # --- decals painted on what is already there
    def decal(self, shape, color=(0, 0, 0), alpha=0.6, mode='mul', clip=None, blur=0.0):
        R = self.raster(shape, margin=6 + int(blur * 3))
        if R is None:
            return
        m, box = R
        x0, y0, x1, y1 = box
        if blur:
            m = gblur(m, blur)
        if clip is not None:
            c = self.raster(clip, margin=6 + int(blur * 3))
            if c is None:
                return
            cm, cb = c
            full = np.zeros((W, W), np.float32)
            full[cb[1]:cb[3], cb[0]:cb[2]] = cm
            m = m * full[y0:y1, x0:x1]
        A = self.A[y0:y1, x0:x1]
        PC = self.PC[y0:y1, x0:x1]
        col = np.asarray(color, np.float32)
        if mode == 'mul':        # darken / tint existing pixels
            f = 1 - (m * alpha)[..., None] * (1 - col[None, None, :])
            PC *= f
        elif mode == 'add':      # light up (only where something is drawn)
            PC += col[None, None, :] * (m * alpha)[..., None] * (A > 0.05)[..., None]
        elif mode == 'over':
            a = m * alpha
            PC[:] = col * a[..., None] + PC * (1 - a[..., None])
            A[:] = a + A * (1 - a)
        elif mode == 'engrave':  # groove: dark line plus a lit lower-right lip
            lip = shift2d(m, 2, 2) * (1 - m)
            PC *= (1 - m * alpha * 0.8)[..., None]
            PC += (lip * 0.22 * alpha * (A > 0.05))[..., None] * np.array([1, .95, .88], np.float32)
        elif mode == 'emboss':   # raised line: lit upper-left lip, dark lower-right
            lit = shift2d(m, -2, -2) * (1 - m)
            drk = shift2d(m, 2, 2) * (1 - m)
            PC += (lit * 0.30 * alpha * (A > 0.05))[..., None] * np.array([1, .95, .88], np.float32)
            PC *= (1 - drk * 0.4 * alpha)[..., None]

    def emit_decal(self, shape, color, alpha=1.0, glow=None, blur=0.5, clip=None):
        """Glowing inlay (runes, fuller light): painted bright, plus a soft glow."""
        self.decal(shape, color, alpha, 'over' if clip is None else 'over', clip=clip, blur=blur)
        if glow:
            self.glow_shape(shape if clip is None else shape.clip(clip), color, glow[0], glow[1])

    # --- a faceted gem; cx, cy in local units, r in units
    def gem(self, cx, cy, r, color, facets=8, sq=1.0, shine=1.0, glow=0.0, shape=None, rot=0.0, op=1.0, sh=0.8):
        if shape == 'diamond':
            poly = _arr([(cx, cy - r * sq), (cx + r * 0.8, cy), (cx, cy + r * sq), (cx - r * 0.8, cy)])
        else:
            poly = ellipse_poly(cx, cy, r, r * sq, rot, 40)
        mat = gem_mat(color)
        mat = dict(mat, facets=facets, ctr=self.tx([[cx, cy]])[0], rad=r * U, sq=sq, shine=shine)
        L = self.layer(Shape.of(poly), mat, sh=sh, op=op)
        if glow:
            self.glow_shape(Shape.of(circle_poly(cx, cy, r * 0.9)), color, r * 0.9 + 1.5, glow)
        return L

    def orb(self, cx, cy, r, c1, c2, mode='glass', glow=0.0, sh=0.8, op=1.0, swirl=0.8, twist=0.6, shine=1.0, seed=0, nscale=13.0, gl_sigma=None):
        mat = orb_mat(c1, c2, mode, swirl, twist, nscale, shine, seed)
        mat['ctr'] = self.tx([[cx, cy]])[0]
        mat['rad'] = r * U * math.sqrt(abs(self.M[0, 0] * self.M[1, 1] - self.M[0, 1] * self.M[1, 0]))
        L = self.layer(Shape.of(circle_poly(cx, cy, r, 56)), mat, sh=sh, op=op)
        if glow:
            self.glow_shape(Shape.of(circle_poly(cx, cy, r * 0.95)), c2, gl_sigma if gl_sigma is not None else r * 0.55 + 1.2, glow)
        return L

    def _gem_flat(self, m, box, mat):
        x0, y0, x1, y1 = box
        yy, xx = np.mgrid[y0:y1, x0:x1].astype(np.float32)
        cxp, cyp = mat['ctr']
        rad_px = max(mat['rad'], 2.0)
        dx = (xx + 0.5 - cxp) / rad_px
        dy = (yy + 0.5 - cyp) / (rad_px * max(mat['sq'], 0.3))
        rr = np.sqrt(dx * dx + dy * dy)
        ang = np.arctan2(dy, dx)
        nf = mat['facets']
        sector = np.floor((ang / (2 * math.pi) + 0.5) * nf)
        aq = (sector + 0.5) / nf * 2 * math.pi - math.pi
        tilt = 0.12 + 0.75 * smoothstep(0.38, 0.62, rr)
        gx, gy = np.cos(aq) * tilt, np.sin(aq) * tilt
        nz = 1.0 / np.sqrt(gx * gx + gy * gy + 1.0)
        nxx, nyy = gx * nz, gy * nz
        diff = np.clip(nxx * LIGHT[0] + nyy * LIGHT[1] + nz * LIGHT[2], 0, 1)
        core = (1 - np.clip(rr, 0, 1)) ** 1.6
        t = 0.10 + 0.62 * diff + 0.36 * core
        t = np.clip(t, 0, 1)
        col = mat['lut'][(t * 255).astype(np.int32)]
        spot = np.exp(-(((dx + 0.34) ** 2 + (dy + 0.38) ** 2) / 0.018)) * mat['shine']
        col = col + spot[..., None] * 0.85
        col = col * (1 - 0.45 * smoothstep(0.82, 1.0, rr))[..., None]
        return np.clip(col, 0, 1.2)

    # --- sphere shading (glass orbs, moons, soul stones); `mat` comes from orb_mat()
    def _orb_flat(self, m, box, mat):
        x0, y0, x1, y1 = box
        yy, xx = np.mgrid[y0:y1, x0:x1].astype(np.float32)
        cxp, cyp = mat['ctr']
        R = max(mat['rad'], 2.0)
        dx = (xx + 0.5 - cxp) / R
        dy = (yy + 0.5 - cyp) / R
        rho = np.sqrt(dx * dx + dy * dy)
        rc = np.clip(rho, 0, 1)
        nz = np.sqrt(np.clip(1 - rc * rc, 0, 1))
        c1 = np.asarray(mat['c1'], np.float32)
        c2 = np.asarray(mat['c2'], np.float32)
        glass = mat['mode'] == 'glass'
        ang = np.arctan2(dy, dx) + mat['twist'] * (1 - rc) ** 1.2 * 3.2
        sx = cxp + np.cos(ang) * rho * R
        sy = cyp + np.sin(ang) * rho * R
        n1 = bilinear(vnoise(mat['nscale'], 41 + mat['seed']), sx, sy)
        n2 = bilinear(vnoise(mat['nscale'] * 0.5, 43 + mat['seed']), sx, sy)
        sw = np.clip((n1 - 0.5) * 2.0 + (n2 - 0.5) * 1.1 + 0.5, 0, 1)
        diff = np.clip(-0.50 * dx - 0.62 * dy + 0.60 * nz, 0, 1) / 0.95
        if glass:
            wisps = smoothstep(0.25, 1.0, sw) ** 1.6
            core = np.exp(-(rc / 0.60) ** 2)
            body = c1[None, None, :] * (0.55 + 0.9 * (1 - rc) ** 1.2)[..., None]
            body = body + (c2 * 0.55)[None, None, :] * (core * (0.55 + 0.6 * sw) * mat['swirl'] + core * 0.25)[..., None]
            body = body + (c2 * 0.30)[None, None, :] * (wisps * (0.4 + 0.6 * (1 - rc)))[..., None] * mat['swirl']
            body = body * (0.70 + 0.45 * diff)[..., None]
            lit = np.clip(0.5 - 0.5 * (dx * 0.55 + dy * 0.7), 0, 1)
            rim = (smoothstep(0.78, 1.0, rc) ** 1.4) * (0.25 + 0.75 * lit)
            col = body + (c2 * 0.55 + 0.25)[None, None, :] * rim[..., None] * 0.55
            low = smoothstep(0.35, 0.95, dx * 0.5 + dy * 0.75) * smoothstep(0.55, 0.95, rc)
            col = col + (low * 0.22)[..., None] * c2
            # window-like reflection arc, upper left
            th = np.arctan2(dy, dx)
            arc = smoothstep(0.62, 0.74, rho) * (1 - smoothstep(0.80, 0.90, rho)) * smoothstep(-2.95, -2.45, th) * (1 - smoothstep(-1.85, -1.40, th))
            col = col + arc[..., None] * 0.55 * mat['shine']
        else:
            t = np.clip(0.08 + 0.90 * diff ** 1.15, 0, 1)
            body = c1 + (c2 - c1) * t[..., None]
            body = body * (1.0 + 0.9 * mat['swirl'] * (sw - 0.5))[..., None]
            col = body
        spot = np.exp(-(((dx + 0.36) ** 2 + (dy + 0.40) ** 2) / 0.014)) * mat['shine']
        col = col + spot[..., None] * np.array([1.0, 0.98, 0.94], np.float32) * (0.85 if glass else 0.5)
        col = col * (1 - 0.45 * smoothstep(0.86, 1.0, rho))[..., None]
        return np.clip(col, 0, 1.2)

    # --- compose an already finished sub-context (used for the miniature on recipe scrolls)
    def paste(self, other, scale, cx, cy, mix_sepia=0.3, outline=0.75, bright=0.95, glow_k=0.35):
        """Paste other.PC/A scaled by `scale` so that the centre of its board lands at (cx, cy) design units."""
        n = int(round(W * scale))
        def rs(a, mode):
            ims = [Image.fromarray(np.ascontiguousarray(a[..., i]), 'F').resize((n, n), Image.LANCZOS) for i in range(a.shape[2])]
            return np.stack([np.asarray(i, np.float32) for i in ims], 2)
        pc = rs(other.PC, 'F')
        a = np.asarray(Image.fromarray(other.A, 'F').resize((n, n), Image.LANCZOS), np.float32)
        gl = rs(other.GL, 'F')
        ox = int(round(cx * U - n / 2))
        oy = int(round(cy * U - n / 2))
        sx0, sy0 = max(0, -ox), max(0, -oy)
        dx0, dy0 = max(0, ox), max(0, oy)
        wv = min(n - sx0, W - dx0)
        hv = min(n - sy0, W - dy0)
        pc = pc[sy0:sy0 + hv, sx0:sx0 + wv]
        a = np.clip(a[sy0:sy0 + hv, sx0:sx0 + wv], 0, 1)
        gl = gl[sy0:sy0 + hv, sx0:sx0 + wv]
        # drawn-on-parchment look: ink outline, slight sepia, a bit darker
        lum = (pc * np.array([0.3, 0.55, 0.15], np.float32)).sum(2, keepdims=True)
        pc = mixc(pc, lum * np.array([1.1, 0.86, 0.6], np.float32), mix_sepia) * bright
        if outline:
            ring = np.clip(gblur(a, 1.6) * 1.8 - a, 0, 1)
            ink = ring * outline
            tgtPC = self.PC[dy0:dy0 + hv, dx0:dx0 + wv]
            tgtA = self.A[dy0:dy0 + hv, dx0:dx0 + wv]
            tgtPC[:] = np.array([0.05, 0.03, 0.02], np.float32) * ink[..., None] + tgtPC * (1 - ink[..., None])
            tgtA[:] = ink + tgtA * (1 - ink)
        tgtPC = self.PC[dy0:dy0 + hv, dx0:dx0 + wv]
        tgtA = self.A[dy0:dy0 + hv, dx0:dx0 + wv]
        tgtPC[:] = pc + tgtPC * (1 - a[..., None])
        tgtA[:] = a + tgtA * (1 - a)
        self.GL[dy0:dy0 + hv, dx0:dx0 + wv] += gl * glow_k

    # --- keep the finished drawing inside the frame and centred (integer-exact shift, rare downscale)
    def fit(self, target=0.82, bias=(0.0, 0.0), maxscale=1.0):
        ys, xs = np.where(self.A > 0.08)
        if len(xs) == 0:
            return
        x0, x1, y0, y1 = xs.min(), xs.max() + 1, ys.min(), ys.max() + 1
        m = max(x1 - x0, y1 - y0)
        s = min(maxscale, target * W / m)
        cxp, cyp = (x0 + x1) / 2.0, (y0 + y1) / 2.0
        tcx, tcy = W / 2.0 + bias[0] * W, W / 2.0 + bias[1] * W
        if abs(s - 1.0) < 1e-3:
            s = 1.0
            tcx = cxp + round(tcx - cxp)
            tcy = cyp + round(tcy - cyp)
        coef = (1.0 / s, 0, cxp - tcx / s, 0, 1.0 / s, cyp - tcy / s)

        def tf(a):
            return np.asarray(Image.fromarray(np.ascontiguousarray(a), 'F').transform((W, W), Image.AFFINE, coef, Image.BILINEAR), np.float32).copy()
        self.PC = np.stack([tf(self.PC[..., i]) for i in range(3)], 2)
        self.GL = np.stack([tf(self.GL[..., i]) for i in range(3)], 2)
        self.A = tf(self.A)

    # --- final image
    def finish(self, tint=None, aura=None, aura_k=0.0, seed=0):
        top, bot = BG_TINTS.get(tint, BG_TINTS[None])
        yy, xx = np.mgrid[0:W, 0:W].astype(np.float32)
        gy = (yy / W)[..., None]
        bg = mixc(np.array(top, np.float32), np.array(bot, np.float32), gy)
        cxp, cyp = 0.47 * W, 0.45 * W
        r = np.sqrt(((xx - cxp) / W) ** 2 + ((yy - cyp) / W) ** 2)
        spot = np.exp(-(r / 0.42) ** 2)
        vig = 1 - 0.62 * smoothstep(0.32, 0.78, r)
        bg = bg * (0.78 + 0.62 * spot)[..., None] * vig[..., None]
        bg = bg * (0.9 + 0.2 * vnoise(70, 21))[..., None] + (vnoise(2.0, 22) - 0.5)[..., None] * 0.012
        if aura is not None and aura_k > 0:
            ar = np.exp(-(r / 0.30) ** 2)
            bg = bg + ar[..., None] * np.asarray(aura, np.float32) * aura_k * 0.16
        # soft ground shadow of the whole item
        sh = gblur(shift2d(self.A, 10, 15), 9.0)
        bg = bg * (1 - 0.7 * sh)[..., None]
        rim = gblur(np.clip(self.A - shift2d(self.A, 3, 3), 0, 1), 0.9)
        pc = self.PC + (rim * 0.30)[..., None] * np.array([0.62, 0.66, 0.78], np.float32) * (self.A > 0.3)[..., None]
        img = bg * (1 - self.A)[..., None] + pc
        gl = np.clip(self.GL, 0, 1.0)
        img = 1 - (1 - np.clip(img, 0, 1)) * (1 - gl * 0.85)
        # mild film grain keeps it from looking airbrushed
        img = img + (vnoise(1.5, 23) - 0.5)[..., None] * 0.02
        img = np.clip(img, 0, 1)
        img = img ** 1.03
        self.full = Image.fromarray((img * 255 + 0.5).astype(np.uint8), 'RGB')
        im = Image.fromarray((img * 255 + 0.5).astype(np.uint8), 'RGB').resize((OUT, OUT), Image.LANCZOS)
        im = im.filter(ImageFilter.UnsharpMask(radius=0.9, percent=55, threshold=2))
        return im.convert('RGBA')


# ------------------------------------------------------------------ shared building blocks

def make_scratches(rng, n=110):
    im = Image.new('L', (W, W), 0)
    d = ImageDraw.Draw(im)
    for _ in range(n):
        x, y = rng.uniform(0, W, 2)
        a = math.radians(rng.normal(-35, 30))
        ln = rng.uniform(6, 46)
        d.line([(x, y), (x + ln * math.cos(a), y + ln * math.sin(a))], fill=int(rng.uniform(100, 255)), width=1)
    return gblur(np.asarray(im, np.float32) / 255.0, 0.55) * 1.6


def pal(c):
    return np.asarray(c, float)


def elem_of(o):
    return ELEM[o.get('el') or 'none']


def wraps(c, p0, p1, w, n, color=(0.02, 0.012, 0.008), alpha=0.7, slant=0.9, mode='engrave'):
    """Diagonal binding lines across a straight grip from p0 to p1 (half-width w)."""
    p0, p1 = _arr(p0), _arr(p1)
    d = p1 - p0
    ln = np.linalg.norm(d)
    u = d / ln
    nrm = np.array([-u[1], u[0]])
    for i in range(n):
        t = (i + 0.5) / n
        ctr = p0 + d * t
        a = ctr - nrm * w * 1.05 - u * slant
        b = ctr + nrm * w * 1.05 + u * slant
        c.decal(S(line_poly(a, b, 0.45, caps='flat')), color, alpha, mode)


def rivet(c, x, y, r=1.1, mat='iron', sh=0.5):
    c.layer(S(circle_poly(x, y, r, 14)), mat, relief='round', k=1.0, sh=sh, ao=0.1, wear=0.3)


def band(c, p0, p1, w, mat):
    c.layer(S(line_poly(p0, p1, w, caps='flat')), mat, relief='round', k=0.9, sh=0.6, ao=0.2)


def spark(c, x, y, r, color=(1, .95, .8)):
    c.layer(S(star_poly(x, y, r, r * 0.22, 4, 0)), emit_mat([(0, color), (1, (1, 1, 1))]), glow=(color, r * 0.9, 0.6), sh=0)


def blade_poly(x, y0, y1, w, tip=0.16, belly=0.0, taper=0.12, curve=0.0, back=0.0, serr=0, serr_amp=1.6,
               wave=0.0, waves=2.0, n=56, wtip=0.0):
    """Blade outline from the guard (y0) up to the tip (y1). Returns polygon, centreline and half-width array."""
    t = np.linspace(0, 1, n)
    y = y0 + (y1 - y0) * t
    cx = x + curve * t ** 2 + wave * np.sin(t * 2 * math.pi * waves) * np.minimum(1, t * 3)
    body = w * (1 - taper * t) * (1 + belly * np.sin(np.pi * t) ** 1.2)
    u = np.clip((t - (1 - tip)) / tip, 0, 1)
    wd = body * (1 - u ** 1.8) ** 0.85 + wtip * (u > 0)
    wl = wd * (1 - back)
    wr = wd.copy()
    if serr:
        saw = (t * serr) % 1.0
        wl = wl + serr_amp * saw * ((t > 0.14) & (t < 1 - tip * 0.6))
    pts = np.stack([cx, y], 1)
    return stroke(pts, wl, wr), pts, wd


def arch_sword(c, o):
    rich = o['rich']
    if o.get('glow_variants'):
        o = dict(o, glowline=o['glow_variants'][o['variant'] % len(o['glow_variants'])])
    if o.get('gem_variants'):
        o = dict(o, gem=o['gem_variants'][o['variant'] % len(o['gem_variants'])], pommel=o['pommel_variants'][o['variant'] % len(o['pommel_variants'])],
                 pgem=o['gem_variants'][o['variant'] % len(o['gem_variants'])])
    L = o.get('len', 1.0)
    T = 112 * L
    y_tip = 50 - T / 2
    y_g = y_tip + T * o.get('bl', 0.64)
    w0 = o.get('w', 5.2)
    pr = o.get('pommel_r', 4.4)
    y_grip_end = y_g + T * o.get('gl', 0.25)
    y_pom = y_grip_end + pr * 0.6
    bmat = o.get('bmat', 'steel')
    gmat = o.get('gmat', 'iron')
    ecol, emat, acc, _ = elem_of(o)
    with c.local(rot=o.get('rot', 45)):
        poly, mid, wd = blade_poly(50, y_g + 1.5, y_tip, w0, tip=o.get('tip', 0.16), belly=o.get('belly', 0.0), taper=o.get('taper', 0.12),
                                   curve=o.get('curve', 0.0), back=o.get('back', 0.0), serr=o.get('serr', 0), wave=o.get('wave', 0.0),
                                   waves=o.get('waves', 2.0), serr_amp=o.get('serr_amp', 1.6))
        bshape = S(poly)
        c.layer(bshape, bmat, relief='roof', k=o.get('bk', 0.6), sh=1.0, op=o.get('bop', 1.0), wear=o.get('bwear', 1.0), glow=o.get('bglow'))
        if o.get('fuller', True):
            f0, f1 = int(len(mid) * 0.05), int(len(mid) * o.get('fuller_end', 0.70))
            fw = w0 * 0.17
            fpoly = stroke(mid[f0:f1], np.linspace(fw, fw * 0.5, f1 - f0), caps='round')
            if o.get('glowline') is not None:
                c.emit_decal(S(fpoly).clip(bshape), o['glowline'], 0.9, glow=(2.2, 0.55), blur=0.7)
            else:
                c.decal(S(fpoly).clip(bshape), (0.02, 0.02, 0.03), 0.8, 'engrave')
        if o.get('blood'):
            rngb = np.random.default_rng(5)
            for k in range(o['blood']):
                i = int(rngb.uniform(0.15, 0.7) * (len(mid) - 1))
                px, py = mid[i]
                dr = stroke([(px + rngb.uniform(-2, 2), py), (px + rngb.uniform(-2, 2), py + rngb.uniform(5, 12))], [1.0, 0.5], caps='round')
                c.decal(S(dr).clip(bshape), (0.35, 0.02, 0.02), 0.85, 'over')
        if o.get('runes'):
            col = o.get('runecol', acc)
            for i in range(5):
                px, py = mid[int((0.14 + i * 0.1) * (len(mid) - 1))]
                rp = S(line_poly((px - 1.0, py), (px + 1.0, py), 0.3)) if i % 2 else S(line_poly((px, py - 1.4), (px, py + 1.4), 0.3), line_poly((px - 1, py), (px + 1, py), 0.3))
                c.emit_decal(rp, pal(col), 0.7, glow=(1.2, 0.35), blur=0.4)
        if o.get('diamonds'):
            for i in range(9):
                px, py = mid[int((0.12 + i * 0.085) * (len(mid) - 1))]
                dw = wd[int((0.12 + i * 0.085) * (len(mid) - 1))] * 0.62
                c.decal(S([(px, py - dw * 1.1), (px + dw, py), (px, py + dw * 1.1), (px - dw, py)]).clip(bshape), (0.03, 0.05, 0.03), 0.55, 'engrave')
        if o.get('flames'):
            _blade_flames(c, mid, wd, o)
        if o.get('embers'):
            embers(c, np.random.default_rng(17), 50, 50 - T * 0.34, 10, T * 0.26, 10, color=ecol, glow=0.5)
        if o.get('crystals'):
            _blade_crystals(c, mid, wd, o)
        if o.get('wisps'):
            _blade_wisps(c, mid, wd, o)
        _guard(c, 50, y_g, o, gmat, rich)
        gw = o.get('grip_w', 3.0)
        c.layer(S(stroke([(50, y_g + 1), (50, y_grip_end)], gw, caps='flat')), o.get('grip', 'leather'), relief='round', k=0.9, sh=0.8)
        wraps(c, (50, y_g + 3), (50, y_grip_end - 1), gw, int((y_grip_end - y_g) / 2.4))
        if rich > 0.25 or o.get('ferrule'):
            band(c, (50 - gw - 0.3, y_g + 2.2), (50 + gw + 0.3, y_g + 2.2), 1.3, o.get('trim', gmat))
        _pommel(c, 50, y_pom, pr, o, gmat, rich)
        if o.get('gem') is not None:
            c.gem(50, y_g, o.get('gem_r', 2.4), pal(o['gem']), facets=8, glow=0.5 if rich > 0.4 else 0.0)


def _guard(c, x, y, o, gmat, rich):
    g = o.get('guard', 'cross')
    span = o.get('gspan', 15.0)
    th = o.get('gth', 2.6)
    if g == 'cross':
        pts = bez([(x - span, y + 2.8), (x, y - 1.6), (x + span, y + 2.8)], 18)
        poly = stroke(pts, np.concatenate([np.linspace(th * 0.75, th, 9), np.linspace(th, th * 0.75, 9)]), caps='flat')
        c.layer(S(poly, circle_poly(x - span, y + 2.8, th * 0.95, 14), circle_poly(x + span, y + 2.8, th * 0.95, 14)), gmat, relief='bevel', r=th * 0.8, k=1.0, sh=1.0, ao=0.1)
    elif g == 'flat':
        c.layer(S(rrect_poly(x - span, y - th, x + span, y + th, 1.4)), gmat, relief='bevel', r=1.6, k=0.9, sh=1.0)
    elif g == 'curve':
        for s in (-1, 1):
            pts = bez([(x + s * 2, y + 1), (x + s * span * 0.75, y + 5), (x + s * span, y - 5.5)], 22)
            c.layer(S(stroke(pts, np.linspace(th, th * 0.35, 22), caps='round')), gmat, relief='round', k=0.9, sh=1.0)
        c.layer(S(ellipse_poly(x, y, 4.2, 3.4)), gmat, relief='round', k=0.95, sh=0.8)
    elif g == 'wing':
        for s in (-1, 1):
            for i in range(4):
                a = bez([(x + s * 3, y - 1 + i * 1.7), (x + s * (span * 0.55), y + 1 + i * 0.4), (x + s * (span - i * 2.4), y - 7 + i * 2.6)], 14)
                c.layer(S(stroke(a, np.linspace(th * 0.9, th * 0.2, 14), caps='round')), gmat, relief='round', k=0.9, sh=0.6)
        c.layer(S(ellipse_poly(x, y, 4.3, 3.8)), gmat, relief='round', k=0.95, sh=0.8)
    elif g == 'ring':
        rr = o.get('ring_r', 9.0)
        c.layer(Shape([('add', circle_poly(x, y - 1, rr)), ('sub', circle_poly(x, y - 1, rr - 2.6))]), gmat, relief='round', k=0.9, sh=1.0)
        c.layer(S(rrect_poly(x - span, y + 2.6 - th, x + span, y + 2.6 + th, 1.2)), gmat, relief='bevel', r=1.4, k=0.9, sh=0.8)
    elif g == 'demon':
        for s in (-1, 1):
            pts = bez([(x + s * 2, y + 2), (x + s * span * 0.9, y + 3), (x + s * (span + 1), y - 9)], 22)
            c.layer(S(stroke(pts, np.linspace(th * 1.1, th * 0.2, 22), caps='round')), gmat, relief='round', k=0.9, sh=1.0)
            pts = bez([(x + s * 3, y + 2.5), (x + s * span * 0.5, y + 8), (x + s * span * 0.7, y + 10)], 14)
            c.layer(S(stroke(pts, np.linspace(th * 0.8, th * 0.2, 14), caps='round')), gmat, relief='round', k=0.9, sh=0.8)
        c.layer(S(ellipse_poly(x, y, 4.0, 4.6)), gmat, relief='round', k=0.95, sh=0.8)
    elif g == 'spiked':
        c.layer(S(rrect_poly(x - span, y - th, x + span, y + th, 1.2)), gmat, relief='bevel', r=1.6, k=0.9, sh=1.0)
        for sd in (-1, 1):
            c.layer(S(spike_poly(x + sd * (span - 1), y, 90 - sd * 55, 9, 2.0)), gmat, relief='roof', k=0.7, sh=0.6, ink=0.5)
            c.layer(S(spike_poly(x + sd * (span - 1), y, -90 + sd * 40, 6, 1.6)), gmat, relief='roof', k=0.7, sh=0.4, ink=0.5)
    elif g == 'disk':
        c.layer(S(ellipse_poly(x, y, span * 0.55, 2.6 + th * 0.4)), gmat, relief='round', k=0.95, sh=1.0)


def _pommel(c, x, y, r, o, gmat, rich):
    p = o.get('pommel', 'ball')
    pm = o.get('pmat', gmat)
    if p == 'ball':
        c.layer(S(circle_poly(x, y, r)), pm, relief='round', k=0.95, sh=1.0)
    elif p == 'spike':
        c.layer(S(circle_poly(x, y - 1, r * 0.9), [(x - r * 0.5, y + r * 0.2), (x, y + r * 2.2), (x + r * 0.5, y + r * 0.2)]), pm, relief='round', k=0.9, sh=1.0)
    elif p == 'disc':
        c.layer(S(ellipse_poly(x, y, r * 1.25, r * 0.9)), pm, relief='round', k=0.95, sh=1.0)
    elif p == 'ring':
        c.layer(Shape([('add', circle_poly(x, y + 1, r * 1.1)), ('sub', circle_poly(x, y + 1, r * 0.55))]), pm, relief='round', k=0.9, sh=1.0)
    elif p == 'gem':
        c.layer(S(circle_poly(x, y, r)), pm, relief='round', k=0.95, sh=1.0)
        c.gem(x, y, r * 0.62, pal(o.get('pgem', (.7, .1, .1))), facets=6, glow=0.4)


def _blade_flames(c, mid, wd, o):
    rng = np.random.default_rng(31)
    n = o.get('flames') if isinstance(o.get('flames'), int) and not isinstance(o.get('flames'), bool) else 5
    for i in range(n):
        t = 0.16 + i * (0.62 / n)
        j = int(t * (len(mid) - 1))
        px, py = mid[j]
        side = 1 if i % 2 == 0 else -1
        x = px + side * wd[j] * 0.78
        h = rng.uniform(8, 13)
        fp = flame_poly(x, py + 1.0, h, 1.25, lean=side * rng.uniform(0.5, 2.0))
        c.layer(S(fp), FIRE, glow=((1, .45, .1), 1.5, 0.22), sh=0, ink=0, op=0.7)


def _blade_crystals(c, mid, wd, o):
    rng = np.random.default_rng(37)
    col = o.get('crystals')
    for i in range(7):
        t = 0.12 + i * 0.1
        j = int(t * (len(mid) - 1))
        px, py = mid[j]
        side = 1 if i % 2 == 0 else -1
        crystal(c, px + side * (wd[j] * 0.95), py + 2.0, rng.uniform(5, 8.5), 1.5, side * rng.uniform(25, 55), col, glow=0.18, sh=0.3)


def _blade_wisps(c, mid, wd, o):
    rng = np.random.default_rng(41)
    col = o.get('wisps')
    for i in range(9):
        t = 0.1 + rng.uniform(0, 0.85)
        j = int(t * (len(mid) - 1))
        px, py = mid[j]
        side = rng.choice([-1, 1])
        c.decal(S(ellipse_poly(px + side * (wd[j] + 2.5), py, rng.uniform(1.4, 2.4), rng.uniform(3.5, 7.0), rng.uniform(-20, 20), 16)), pal(col), 0.30, 'add', blur=1.6)


# ------------------------------------------------------------------ parts library

def spike_poly(x, y, ang, length, width):
    """Triangle with base centre (x, y), pointing at angle `ang` degrees (0 = +x, 90 = down)."""
    a = math.radians(ang)
    d = np.array([math.cos(a), math.sin(a)])
    n = np.array([-d[1], d[0]])
    p = np.array([x, y], float)
    return _arr([p + n * width, p + d * length, p - n * width])


def chain_links(c, pts, mat='iron', rx=1.5, ry=0.9, step=2.3, sh=0.4, thick=0.55):
    """Chain of alternating links along a polyline (design units)."""
    P = _arr(pts)
    seg = np.linalg.norm(np.diff(P, axis=0), axis=1)
    cum = np.concatenate([[0], np.cumsum(seg)])
    n = int(cum[-1] / step)
    for i in range(n + 1):
        d = i * step
        j = min(np.searchsorted(cum, d, side='right') - 1, len(P) - 2)
        f = (d - cum[j]) / max(seg[j], 1e-6)
        p = P[j] + (P[j + 1] - P[j]) * f
        ang = math.degrees(math.atan2(*(P[j + 1] - P[j])[::-1]))
        flat = i % 2 == 0
        a, b = (rx, ry) if flat else (rx * 0.62, ry * 0.8)
        ring = Shape([('add', ellipse_poly(p[0], p[1], a, b, ang, 24)), ('sub', ellipse_poly(p[0], p[1], a - thick, max(b - thick, 0.15), ang, 24))])
        c.layer(ring, mat, relief='round', k=0.9, sh=sh, ao=0.0, ink=0.35, wear=0.4)


def crystal(c, cx, cy, h, w, ang, color, glow=0.0, sh=0.6, alpha=1.0):
    """Hexagonal-ish shard standing on (cx, cy), pointing up before `ang` rotation."""
    col = np.asarray(color, float)
    with c.local(rot=ang, pivot=(cx, cy)):
        rx = cx + 0.12 * w
        left = [(cx - w, cy), (cx - w, cy - 0.62 * h), (rx, cy - h), (rx, cy)]
        right = [(rx, cy), (rx, cy - h), (cx + w, cy - 0.62 * h), (cx + w, cy)]
        top = [(cx - w, cy - 0.62 * h), (rx, cy - h), (cx + w, cy - 0.62 * h), (rx, cy - 0.46 * h)]
        for poly, k in ((left, 1.0), (right, 0.45), (top, 1.35)):
            m = tinted(np.clip(col * k * 0.9, 0, 1), spec=0.4, shin=24)
            c.layer(S(poly), m, relief='flat', k=0.5, sh=sh if k == 1.0 else 0.0, ao=0.0, ink=0.45, op=alpha)
        c.decal(S(line_poly((rx, cy - 0.95 * h), (rx, cy - 0.1 * h), 0.22, caps='flat')), (1, 1, 1), 0.28 * alpha, 'add', blur=0.6)
        if glow:
            c.glow_shape(S(left, right), col, w * 1.6 + 1.0, glow)


def bolt_path(p0, p1, rng, segs=6, amp=2.6):
    p0, p1 = _arr(p0), _arr(p1)
    d = p1 - p0
    ln = np.linalg.norm(d)
    u = d / ln
    nrm = np.array([-u[1], u[0]])
    pts = [p0]
    for i in range(1, segs):
        t = i / segs
        pts.append(p0 + d * t + nrm * rng.uniform(-amp, amp))
    pts.append(p1)
    return _arr(pts)


def bolt(c, p0, p1, rng, width=0.55, color=(0.65, 0.82, 1.0), segs=6, amp=2.6, glow=0.55, branch=True):
    pts = bolt_path(p0, p1, rng, segs, amp)
    m = emit_mat([(0, color), (1, (1, 1, 1))])
    c.layer(S(stroke(pts, width, caps='round')), m, glow=(color, width * 2.6, glow), sh=0, ink=0)
    if branch and len(pts) > 3:
        i = int(rng.integers(1, len(pts) - 2))
        q = pts[i] + (pts[i + 1] - pts[i]) * 0.5 + rng.uniform(-3, 3, 2) * 1.5
        c.layer(S(stroke(bolt_path(pts[i], q, rng, 3, amp * 0.5), width * 0.6, caps='round')), m, glow=(color, width * 2, glow * 0.6), sh=0, ink=0)


def embers(c, rng, cx, cy, rx, ry, n, color=(1.0, 0.55, 0.15), rmin=0.35, rmax=0.9, glow=0.5):
    m = emit_mat([(0, color), (1, (1, 0.95, 0.7))])
    for _ in range(n):
        x = cx + rng.uniform(-rx, rx)
        y = cy + rng.uniform(-ry, ry)
        r = rng.uniform(rmin, rmax)
        c.layer(S(circle_poly(x, y, r, 10)), m, glow=(color, r * 2.6, glow), sh=0, ink=0)


GLYPHS = [
    [((0, -1), (0.8, 0)), ((0.8, 0), (0, 1)), ((0, 1), (-0.8, 0)), ((-0.8, 0), (0, -1)), ((0, -1.3), (0, 1.3))],   # lozenge with bar
    [((-0.8, -0.9), (0, -0.1)), ((0, -0.1), (0.8, -0.9)), ((-0.8, 0.1), (0, 0.9)), ((0, 0.9), (0.8, 0.1))],       # double chevron
    [((-0.8, 0.9), (0, -0.9)), ((0, -0.9), (0.8, 0.9)), ((0.8, 0.9), (-0.8, 0.9)), ((0, 0.1), (0, 0.9))],          # triangle with stem
    [((-0.6, -1), (0.4, -0.3)), ((0.4, -0.3), (-0.4, 0.3)), ((-0.4, 0.3), (0.6, 1))],                                # zigzag
    [((-0.8, -0.9), (0.8, 0.9)), ((0.8, -0.9), (-0.8, 0.9)), ((-0.8, -0.9), (-0.8, -0.2)), ((0.8, -0.9), (0.8, -0.2))],  # crossed with ticks
    [((-0.8, -0.8), (0.8, -0.8)), ((0.8, -0.8), (0.8, 0.8)), ((0.8, 0.8), (-0.3, 0.8)), ((-0.3, 0.8), (-0.3, -0.1)), ((-0.3, -0.1), (0.3, -0.1))],  # square spiral
    [((-0.9, 0), (0, -0.9)), ((0, -0.9), (0.9, 0)), ((-0.9, 0.9), (0.9, 0.9)), ((0, -0.2), (0, 0.9))],              # arrowhead on bar
    [((-0.7, -0.9), (0.7, 0.9)), ((-0.7, 0.9), (0.7, -0.9)), ((0, -1.1), (0, 1.1))],                                # star-cross
]


def glyph_shape(kind, x, y, s, lw=0.28):
    segs = GLYPHS[kind % len(GLYPHS)]
    return S(*[line_poly((x + a[0] * s, y + a[1] * s), (x + b[0] * s, y + b[1] * s), lw, caps='round') for a, b in segs])


def rune_row(c, p0, p1, n, s, color, rng=None, glow=(1.1, 0.35), alpha=0.85, start=0):
    p0, p1 = _arr(p0), _arr(p1)
    for i in range(n):
        t = (i + 0.5) / n
        p = p0 + (p1 - p0) * t
        c.emit_decal(glyph_shape(start + i * 3 + (i % 2), p[0], p[1], s), pal(color), alpha, glow=glow, blur=0.35)


def flame_poly(cx, cy, h, w, lean=0.0, tongues=1):
    """A single flame silhouette standing on (cx, cy)."""
    pts = [(cx - w, cy), (cx - w * 0.85, cy - h * 0.35), (cx - w * 0.35 + lean * 0.4, cy - h * 0.65),
           (cx + lean, cy - h), (cx + w * 0.25 + lean * 0.6, cy - h * 0.62), (cx + w * 0.8, cy - h * 0.3), (cx + w, cy)]
    arc = ellipse_poly(cx, cy, w, h * 0.18, 0, 12, 0, 180)
    return _arr(pts + [tuple(p) for p in arc[::-1][1:-1]])


def skull(c, cx, cy, r, glow=None, mat='bone', sh=0.8, rot=0.0):
    with c.local(rot=rot, pivot=(cx, cy)):
        cran = ellipse_poly(cx, cy - r * 0.12, r * 0.92, r * 0.86, 0, 40)
        jaw = rrect_poly(cx - r * 0.52, cy + r * 0.3, cx + r * 0.52, cy + r * 1.0, r * 0.22)
        c.layer(S(cran, jaw), mat, relief='round', k=0.85, sh=sh, ao=0.3, ink=0.55)
        for s in (-1, 1):
            c.decal(S(ellipse_poly(cx + s * r * 0.38, cy - r * 0.02, r * 0.26, r * 0.31, 0, 20)), (0.02, 0.012, 0.01), 0.96, 'over')
            if glow is not None:
                c.emit_decal(S(circle_poly(cx + s * r * 0.38, cy, r * 0.12, 10)), pal(glow), 0.95, glow=(r * 0.5, 0.5), blur=0.3)
        c.decal(S([(cx, cy + r * 0.12), (cx - r * 0.12, cy + r * 0.42), (cx + r * 0.12, cy + r * 0.42)]), (0.03, 0.02, 0.02), 0.9, 'over')
        for i in range(-2, 3):
            c.decal(S(line_poly((cx + i * r * 0.2, cy + r * 0.64), (cx + i * r * 0.2, cy + r * 0.98), r * 0.035, caps='flat')), (0.04, 0.03, 0.03), 0.7, 'over')


def wood_shaft(c, x, y0, y1, w, mat='wood', w1=None, sh=1.0):
    w1 = w if w1 is None else w1
    return c.layer(S(stroke([(x, y0), (x, y1)], [w, w1], caps='flat')), mat, relief='round', k=0.9, sh=sh, ao=0.2, grain='y')


def ferrule(c, x, y, w, mat='iron', h=2.2, sh=0.7):
    return c.layer(S(rrect_poly(x - w, y, x + w, y + h, 0.7)), mat, relief='bevel', r=0.9, k=1.0, sh=sh, ao=0.0, wear=0.4)


def feather_wing(c, x, y, side=1, scale=1.0, mat=None, n=4, a0=-100.0, a1=-30.0, length=22.0, width=3.2):
    """Fan of feathers radiating from (x, y) up and outwards; side=-1 mirrors horizontally."""
    mat = mat or tinted((0.62, 0.64, 0.68), 'matte', cloth=True, spec=0.05)
    for i in range(n):
        t = i / max(n - 1, 1)
        a = math.radians(a0 + (a1 - a0) * t)
        d = np.array([math.cos(a) * side, math.sin(a)])
        ln = (length - 5.0 * t) * scale
        nrm = np.array([-d[1], d[0]]) * side
        p0 = np.array([x, y], float)
        pts = bez([p0, p0 + d * ln * 0.55 + nrm * ln * 0.10, p0 + d * ln], 14)
        wd = width * scale * np.concatenate([np.linspace(0.35, 1.0, 5), np.linspace(1.0, 0.25, 9)])
        c.layer(S(stroke(pts, wd, caps='round')), mat, relief='round', k=0.8, sh=0.4, ao=0.1, ink=0.4)


def tassel(c, x, y, color=(0.55, 0.06, 0.06), length=9.0, w=1.4):
    m = tinted(color, 'matte', cloth=True)
    for i, dx in enumerate((-1.2, 0.0, 1.2)):
        pts = bez([(x + dx * 0.3, y), (x + dx * 1.8, y + length * 0.5), (x + dx * 2.8 + (i - 1) * 0.5, y + length)], 12)
        c.layer(S(stroke(pts, np.linspace(w, w * 0.35, 12), caps='round')), m, relief='round', k=0.8, sh=0.5, ink=0.4)


# ------------------------------------------------------------------ polearms (spear, glaive, scythe, crescent)

def arch_polearm(c, o):
    head = o.get('head', 'spear')
    rich = o['rich']
    smat = o.get('shaft', 'wood')
    hmat = o.get('hmat', 'steel')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 3))
    L = o.get('len', 1.0)
    y_top = 50 - 50 * L
    y_bot = 50 + 50 * L
    sw = o.get('sw', 1.9)
    with c.local(rot=o.get('rot', 40)):
        if head == 'spear':
            hl = o.get('hl', 38)
            hw = o.get('hw', 6.0)
            wood_shaft(c, 50, y_top + hl - 2, y_bot, sw, smat, sw * 0.9)
            wraps(c, (50, 50 + 22 * L), (50, 50 + 34 * L), sw, 6, alpha=0.8)
            band(c, (50 - sw - 0.4, 50 + 22 * L), (50 + sw + 0.4, 50 + 22 * L), 1.1, o.get('trim', 'iron'))
            ferrule(c, 50, y_bot - 3, sw * 1.15, o.get('trim', 'iron'), 4.5)
            poly, mid, wd = blade_poly(50, y_top + hl, y_top, hw, tip=o.get('tip', 0.62), belly=o.get('belly', 0.25), taper=0.0, serr=o.get('serr', 0))
            bs = S(poly)
            c.layer(bs, hmat, relief='roof', k=0.55, sh=1.0, wear=o.get('bwear', 1.0), glow=o.get('bglow'))
            f0, f1 = int(len(mid) * 0.08), int(len(mid) * 0.8)
            if o.get('glowline') is not None:
                c.emit_decal(S(stroke(mid[f0:f1], np.linspace(0.5, 0.2, f1 - f0), caps='round')).clip(bs), o['glowline'], 0.9, glow=(1.6, 0.5), blur=0.5)
            else:
                c.decal(S(stroke(mid[f0:f1], np.linspace(0.45, 0.15, f1 - f0), caps='round')).clip(bs), (0.02, 0.02, 0.03), 0.6, 'engrave')
            # socket and collar
            c.layer(S([(50 - 2.4, y_top + hl - 1), (50 + 2.4, y_top + hl - 1), (50 + 3.0, y_top + hl + 6.5), (50 - 3.0, y_top + hl + 6.5)]),
                    o.get('trim', 'iron'), relief='bevel', r=1.2, k=1.0, sh=0.8, wear=0.5)
            band(c, (50 - 3.4, y_top + hl + 6.8), (50 + 3.4, y_top + hl + 6.8), 1.0, o.get('trim', 'iron'))
            if o.get('tassel'):
                tassel(c, 50, y_top + hl + 8, o['tassel'], 10 + 6 * rich)
            if o.get('wing'):
                for s in (-1, 1):
                    wp = bez([(50 + s * 2.5, y_top + hl + 2), (50 + s * 11, y_top + hl - 3), (50 + s * 13, y_top + hl - 14)], 14)
                    c.layer(S(stroke(wp, np.linspace(2.4, 0.2, 14), caps='round')), hmat, relief='round', k=0.9, sh=0.8)
            if o.get('bolts'):
                for k in range(o['bolts']):
                    y = y_top + 2 + k * 7
                    s = -1 if k % 2 else 1
                    bolt(c, (50 + s * 4, y + 4), (50 + s * rng.uniform(10, 15), y - rng.uniform(1, 5)), rng, color=ecol, amp=2.2)
        elif head == 'glaive':
            hl = o.get('hl', 46)
            wood_shaft(c, 50, y_top + hl - 14, y_bot, sw, smat, sw * 0.9)
            wraps(c, (50, 50 + 20 * L), (50, 50 + 32 * L), sw, 6, alpha=0.8)
            ferrule(c, 50, y_bot - 3, sw * 1.15, o.get('trim', 'iron'), 4.5)
            poly, mid, wd = blade_poly(50, y_top + hl, y_top, o.get('hw', 6.3), tip=0.42, belly=0.15, taper=-0.15, curve=o.get('curve', 11), back=0.82, n=64)
            bs = S(poly)
            c.layer(bs, hmat, relief='roof', k=0.5, sh=1.0, glow=o.get('bglow'))
            f0, f1 = int(len(mid) * 0.06), int(len(mid) * 0.72)
            gl = o.get('glowline')
            if gl is not None:
                c.emit_decal(S(stroke(mid[f0:f1], np.linspace(0.5, 0.2, f1 - f0), caps='round')).clip(bs), gl, 0.9, glow=(1.6, 0.5), blur=0.5)
            else:
                c.decal(S(stroke(mid[f0:f1], np.linspace(0.45, 0.15, f1 - f0), caps='round')).clip(bs), (0.02, 0.02, 0.03), 0.6, 'engrave')
            c.layer(S([(50 - 2.6, y_top + hl - 3), (50 + 2.6, y_top + hl - 3), (50 + 3.1, y_top + hl + 9), (50 - 3.1, y_top + hl + 9)]),
                    o.get('trim', 'iron'), relief='bevel', r=1.2, k=1.0, sh=0.8, wear=0.5)
            band(c, (50 - 3.4, y_top + hl + 9.2), (50 + 3.4, y_top + hl + 9.2), 1.0, o.get('trim', 'iron'))
            c.gem(50, y_top + hl + 3, 1.6, pal(o.get('gem', acc)), facets=6, glow=0.35 if rich > 0.3 else 0.0)
            if o.get('tassel'):
                tassel(c, 50, y_top + hl + 10, o['tassel'], 9 + 5 * rich)
        elif head == 'scythe':
            wood_shaft(c, 50, y_top + 6, y_bot, sw, smat, sw * 0.9)
            c.layer(S(stroke([(50 - 8, 50 - 8 * L), (50 - 3, 50 - 8 * L)], 1.1, caps='round'), stroke([(50 - 8, 50 - 8 * L), (50 - 8, 50 - 2 * L)], 1.1, caps='round')),
                    smat, relief='round', k=0.9, sh=0.5, grain='y')
            pts = bez([(50, y_top + 7), (20, y_top - 9), (15, y_top + 20)], 40)
            wl = np.concatenate([np.linspace(2.4, 3.3, 14), np.linspace(3.3, 0.3, 26)])
            wr = np.concatenate([np.linspace(1.6, 4.4, 14), np.linspace(4.4, 0.2, 26)])
            poly = stroke(pts, wl, wr, caps='flat')
            bs = S(poly)
            c.layer(bs, hmat, relief='roof', k=0.7, sh=1.0, glow=o.get('bglow'), op=o.get('bop', 1.0))
            if o.get('glowline') is not None:
                c.emit_decal(S(stroke(pts[3:-6], np.linspace(0.45, 0.2, len(pts) - 9), caps='round')).clip(bs), o['glowline'], 0.9, glow=(1.6, 0.5), blur=0.5)
            c.layer(S(rrect_poly(50 - 3.2, y_top + 1, 50 + 3.2, y_top + 11, 1.0)), o.get('trim', 'iron'), relief='bevel', r=1.0, k=1.0, sh=0.8, wear=0.5)
            band(c, (50 - sw - 0.4, y_top + 14), (50 + sw + 0.4, y_top + 14), 1.0, o.get('trim', 'iron'))
        elif head == 'crescent':
            wood_shaft(c, 50, y_top + 22, y_bot, sw, smat, sw * 0.9)
            wraps(c, (50, 50 + 22 * L), (50, 50 + 34 * L), sw, 6, alpha=0.8)
            ferrule(c, 50, y_bot - 3, sw * 1.15, o.get('trim', 'iron'), 4.5)
            cy = y_top + 17
            outer = ellipse_poly(50, cy, 17.5, 17.5, 0, 56)
            inner = ellipse_poly(50, cy - 5.5, 14.5, 14.0, 0, 56)
            bs = Shape([('add', outer), ('sub', inner)])
            c.layer(bs, hmat, relief='roof', k=0.55, sh=1.0, glow=o.get('bglow'))
            if o.get('moon') is not None:
                c.orb(50, cy - 4.0, 7.8, (.14, .0, .01), (.82, .1, .08), mode='solid', glow=0.9, sh=0.3, shine=0.4, seed=9, gl_sigma=6.5, swirl=0.45, twist=0.0, nscale=7)
            c.layer(S(rrect_poly(50 - 3.2, cy + 11, 50 + 3.2, cy + 20, 1.0)), o.get('trim', 'iron'), relief='bevel', r=1.0, k=1.0, sh=0.8, wear=0.5)
            c.layer(S([(50, cy + 8.5), (47.0, cy + 12), (50, cy + 16), (53, cy + 12)]), o.get('trim', 'iron'), relief='bevel', r=1.0, k=1.0, sh=0.4)
            if o.get('gem') is not None:
                c.gem(50, cy + 12, 1.7, pal(o['gem']), facets=6, glow=0.5)


# ------------------------------------------------------------------ axes

def _axe_head(typ, sz):
    """(polygon list, edge polylines) of the head in coordinates relative to the haft attachment point."""
    if typ in ('single', 'beard'):
        low = 17 if typ == 'beard' else 14
        reach = 27 if typ == 'beard' else 24
        top_w = bez([(2, -6.5), (-12, -5.0), (-23, -15)], 16)
        edge = bez([(-23, -15), (-33, 0), (-reach - 1, low + 6 if typ == 'beard' else low)], 26)
        bot_w = bez([(edge[-1][0], edge[-1][1]), (-13, 5.5), (2, 6.5)], 16)
        poly = np.vstack([top_w, edge[1:], bot_w[1:]])
        poll = [(1.6, -5.5), (8.5, -5.0), (9.6, 0), (8.5, 5.0), (1.6, 5.5)]
        return [poly * sz, _arr(poll) * sz], [edge * sz]
    if typ in ('double', 'great'):
        top_w = bez([(0, -6.5), (-12, -5.0), (-23, -16)], 16)
        edge = bez([(-23, -16), (-34, 0), (-23, 16)], 26)
        bot_w = bez([(-23, 16), (-12, 5.0), (0, 6.5)], 16)
        left = np.vstack([top_w, edge[1:], bot_w[1:]]) * sz
        right = left.copy()
        right[:, 0] *= -1
        return [left, right], [edge * sz, edge * sz * np.array([-1, 1])]
    raise ValueError(typ)


def arch_axe(c, o):
    rich = o['rich']
    typ = o.get('type', 'single')
    hmat = o.get('hmat', 'steel')
    haft = o.get('haft', 'wood')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 4))
    L = o.get('hl', 1.0)
    sz = o.get('sz', 1.0)
    top, bot = 50 - 43 * L, 50 + 45 * L
    hx, hy = 50.0, top + 17 * sz
    with c.local(rot=o.get('rot', 38)):
        wood_shaft(c, 50, top + 3, bot, 2.1, haft, 1.9)
        wraps(c, (50, bot - 24), (50, bot - 3), 2.1, 8, alpha=0.8)
        ferrule(c, 50, bot - 3.5, 2.5, o.get('trim', 'iron'), 4.0)
        if rich > 0.3:
            band(c, (50 - 2.7, bot - 25), (50 + 2.7, bot - 25), 1.0, o.get('trim', 'iron'))
        polys, edges = _axe_head(typ, sz)
        for dx in (-1.7, 1.7):
            c.layer(S(rrect_poly(50 + dx - 0.7, hy + 6 * sz, 50 + dx + 0.7, hy + 25 * sz, 0.5)), o.get('trim', 'iron'), relief='bevel', r=0.5, k=1.0, sh=0.4, wear=0.4)
        shifted = [p + np.array([hx, hy]) for p in polys]
        head = S(*shifted)
        notch = o.get('notches', 0)
        if notch:
            for i in range(notch):
                e = edges[0] + np.array([hx, hy])
                j = int((0.2 + 0.6 * (i + 0.5) / notch) * (len(e) - 1))
                head = head.minus(S(circle_poly(e[j][0] + 0.8, e[j][1], 1.5, 8)))
        c.layer(head, hmat, relief='bevel', r=3.2 * sz, k=0.85, sh=1.0, glow=o.get('bglow'), wear=o.get('bwear', 1.0))
        for e in edges:
            ee = e + np.array([hx, hy])
            c.decal(S(stroke(ee, 0.5, caps='round')), (0.95, 0.96, 1.0), 0.4, 'add')
            c.decal(S(stroke(e * 0.9 + np.array([hx, hy]), 1.7, caps='round')).clip(head), (1, 1, 1), 0.13, 'add', blur=0.7)
        if o.get('glowline') is not None:
            for e in edges:
                ee = e[3:-3] * 0.84 + np.array([hx, hy])
                c.emit_decal(S(stroke(ee, 0.5, caps='round')).clip(head), o['glowline'], 0.9, glow=(1.4, 0.5), blur=0.4)
        elif rich > 0.25 or o.get('engrave'):
            for e in edges[:1]:
                ee = e[3:-3] * 0.78 + np.array([hx, hy])
                c.decal(S(stroke(ee, 0.3, caps='round')).clip(head), (0.02, 0.02, 0.03), 0.7, 'engrave')
        if o.get('runes'):
            for k, e in enumerate(edges):
                rune_row(c, np.array([hx - 5 * (1 - 2 * k), hy - 4]), np.array([hx - 5 * (1 - 2 * k), hy + 5]), 3, 1.2, o.get('runecol', acc), start=k)
        if o.get('spike'):
            c.layer(S(spike_poly(hx + 7 * sz, hy, 0, 9 * sz, 2.4)), hmat, relief='roof', k=0.6, sh=0.8)
        if o.get('top_spike'):
            c.layer(S(spike_poly(50, hy - 8 * sz, -90, 10, 2.0)), hmat, relief='roof', k=0.6, sh=0.8)
        if o.get('gem') is not None:
            c.gem(50, hy, 2.3, pal(o['gem']), facets=8, glow=0.5)
        if o.get('skulls'):
            for k in range(o['skulls']):
                skull(c, 50 + (k - (o['skulls'] - 1) / 2) * 7, bot - 30, 3.0, glow=ecol if o.get('skull_glow') else None, rot=(-1) ** k * 8)
        if o.get('flames'):
            for k in range(5):
                e = edges[0] + np.array([hx, hy])
                j = int((0.12 + 0.18 * k) * (len(e) - 1))
                x, y = e[j]
                fp = flame_poly(x - 1.0, y + 1.0, 7.5 + rng.uniform(0, 3), 1.8, lean=-1.5)
                c.layer(S(fp), FIRE, glow=((1, .45, .1), 1.5, 0.28), sh=0, ink=0, op=0.85)
        if o.get('five'):
            cols = [(.85, .15, .1), (.2, .55, .9), (.3, .75, .25), (.9, .75, .2), (.7, .7, .75)]
            for k in range(5):
                c.gem(50, hy + 2 + 6.4 * k * 0.9 + 4, 1.45, pal(cols[k]), facets=6, glow=0.35)


# ------------------------------------------------------------------ hammers

def arch_hammer(c, o):
    rich = o['rich']
    hmat = o.get('hmat', 'steel')
    haft = o.get('haft', 'wood')
    ecol, emat, acc, _ = elem_of(o)
    L = o.get('hl', 1.0)
    top, bot = 50 - 44 * L, 50 + 46 * L
    hw, hh = o.get('hw', 21.0), o.get('hh', 11.0)
    hy = top + hh + 3
    with c.local(rot=o.get('rot', 38)):
        wood_shaft(c, 50, hy, bot, 2.2, haft, 2.0)
        wraps(c, (50, bot - 26), (50, bot - 3), 2.2, 9, alpha=0.8)
        ferrule(c, 50, bot - 3.5, 2.6, o.get('trim', 'iron'), 4.0)
        for dx in (-1.8, 1.8):
            c.layer(S(rrect_poly(50 + dx - 0.7, hy, 50 + dx + 0.7, hy + 24, 0.5)), o.get('trim', 'iron'), relief='bevel', r=0.5, k=1.0, sh=0.4, wear=0.4)
        body = S(rrect_poly(50 - hw, hy - hh, 50 + hw, hy + hh * 0.9, 1.6))
        if o.get('claw'):
            body = body + S([(50 + hw - 1, hy - 5), (50 + hw + 10, hy - 9), (50 + hw + 4, hy), (50 + hw + 12, hy + 6), (50 + hw - 1, hy + 5)])
        if o.get('spike'):
            body = body + S(spike_poly(50 + hw - 0.5, hy - 0.5, 0, 12, 3.4))
        c.layer(body, hmat, relief='bevel', r=3.6, k=0.9, sh=1.0, glow=o.get('bglow'), wear=1.0, dent=o.get('dent', 0.0), ao=0.4)
        for sx in (-1, 1):
            xa = 50 + sx * (hw - 4.5)
            c.layer(S(rrect_poly(min(xa, 50 + sx * hw), hy - hh + 0.8, max(xa, 50 + sx * hw), hy + hh * 0.9 - 0.8, 1.0)), o.get('cap', 'darksteel'), relief='bevel', r=1.2, k=1.0, sh=0.6, ao=0.0)
        c.layer(S(rrect_poly(50 - 4.0, hy - hh - 0.6, 50 + 4.0, hy + hh * 0.9 + 0.6, 1.0)), o.get('trim', 'iron'), relief='bevel', r=1.4, k=1.0, sh=0.8, ao=0.0)
        if o.get('glowline') is not None or o.get('runes'):
            rune_row(c, (50 - hw + 7.5, hy - 0.5), (50 - 6.0, hy - 0.5), 3, 2.1, o.get('runecol', acc), start=1)
            rune_row(c, (50 + 6.0, hy - 0.5), (50 + hw - 7.5, hy - 0.5), 3, 2.1, o.get('runecol', acc), start=5)
        if o.get('gem') is not None:
            c.gem(50, hy + 0.4, 2.7, pal(o['gem']), facets=8, glow=0.6)
        if o.get('rivets'):
            for sx in (-1, 1):
                for sy in (-1, 1):
                    rivet(c, 50 + sx * (hw - 9), hy + sy * (hh * 0.55), 1.0)
        if o.get('sparks'):
            embers(c, np.random.default_rng(11), 50 - hw * 0.4, hy - hh - 6, 14, 8, 9, color=(1.0, .6, .15), glow=0.5)
        if o.get('lightning'):
            rng = np.random.default_rng(9)
            for k in range(3):
                bolt(c, (50 - hw - 1, hy - 4 + k * 4), (50 - hw - 7 - k * 2, hy - 6 + k * 5), rng, color=ecol, amp=1.6, segs=4)


# ------------------------------------------------------------------ gloves, gauntlets, fist, bracers, claws

FINGERS = [  # (x of the finger axis, half width, length, tilt in degrees)
    (35.0, 4.9, 25.0, -6.0),
    (45.2, 5.0, 29.0, -2.0),
    (55.0, 4.9, 26.0, 2.5),
    (64.6, 4.4, 20.0, 7.5),
]


def _finger(c, fx, hw, ln, tilt, base_y, o, mat, under):
    segs = (0.42, 0.33, 0.25)
    style = o.get('style', 'plate')
    with c.local(rot=tilt, pivot=(fx, base_y)):
        y = base_y + 1.5
        if style == 'plate':
            for i, f in enumerate(segs):
                h = ln * f
                y0, y1 = y - h, y
                tip = i == 2
                poly = rrect_poly(fx - hw + 0.2, y0, fx + hw - 0.2, y1 - 0.5, hw * (0.95 if tip else 0.45))
                c.layer(S(poly), mat, relief='bevel', r=hw * 0.55, k=0.95, sh=0.5, ao=0.15, wear=o.get('wear', 1.0))
                y = y0
            if o.get('claws'):
                c.layer(S(spike_poly(fx, y - 1.0, -90, 6.0 + 4 * o['rich'], hw * 0.55)), mat, relief='roof', k=0.7, sh=0.5, ink=0.5)
        else:
            body = S(rrect_poly(fx - hw + 0.1, base_y - ln, fx + hw - 0.1, base_y + 1.5, hw * 0.95))
            c.layer(body, mat, relief='round', r=hw * 0.7, k=0.5, sh=0.6, ao=0.2)
            c.decal(S(stroke([(fx - hw * 0.62, base_y - ln + hw * 0.7), (fx - hw * 0.62, base_y)], 0.2)), (0.03, 0.02, 0.01), 0.55, 'engrave')
            yy = base_y + 1.5
            for f in segs[:-1]:
                yy -= ln * f
                c.decal(S(line_poly((fx - hw * 0.8, yy), (fx + hw * 0.8, yy), 0.28, caps='flat')), (0.02, 0.012, 0.01), 0.7, 'engrave')
            if o.get('studs'):
                for i in range(2):
                    rivet(c, fx, base_y - ln * (0.2 + 0.32 * i), 0.95, o.get('stud', 'iron'), sh=0.3)


def arch_glove(c, o):
    rich = o['rich']
    style = o.get('style', 'plate')
    mat = o.get('mat', 'steel')
    under = o.get('under', 'blackleather')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 6))
    glow = o.get('glowcol')
    base_y = 49.0
    with c.local(rot=o.get('rot', 9), pivot=(50, 55)):
        # cuff behind everything
        cuffmat = o.get('cuff', mat if style == 'plate' else 'leather')
        cuff = [(35.5, 73), (64.5, 73), (70.5, 95), (29.5, 95)]
        c.layer(S(cuff), cuffmat, relief='bevel', r=3.0, k=0.9, sh=1.0, ao=0.3, wear=o.get('wear', 1.0))
        # palm / back of hand underlay
        palm = [(36, 76), (64, 76), (68.5, 62), (71.5, 49), (28.5, 49), (31.5, 62)]
        c.layer(S(palm), mat if style == 'leather' else under, relief='bevel', r=5.0, k=0.9, sh=0.9, ao=0.2)
        # thumb
        with c.local(rot=-52, pivot=(32, 66)):
            if style == 'plate':
                y = 70.0
                for i, (f, w) in enumerate(((13.0, 5.4), (10.0, 4.8), (7.5, 4.4))):
                    poly = rrect_poly(32 - w, y - f, 32 + w, y - 0.4, w * (0.95 if i == 2 else 0.45))
                    c.layer(S(poly), mat, relief='bevel', r=w * 0.55, k=0.95, sh=0.6, ao=0.15)
                    y -= f
            else:
                c.layer(S(stroke([(32, 70), (32, 52)], 5.4, caps='round')), mat, relief='round', k=0.85, sh=0.8, ao=0.2)
        for fx, hw, ln, tilt in FINGERS:
            _finger(c, fx, hw, ln, tilt, base_y, dict(o, rich=rich), mat, under)
        # back-of-hand plate
        if style == 'plate':
            bp = [(38, 74), (62, 74), (66, 62), (68.5, 51.5), (31.5, 51.5), (34, 62)]
            c.layer(S(bp), mat, relief='bevel', r=3.4, k=0.85, sh=0.8, ao=0.25, wear=o.get('wear', 1.0))
            for fx, hw, ln, tilt in FINGERS:
                c.layer(S(circle_poly(fx, 51.5, hw * 0.92, 20)), mat, relief='round', k=1.0, sh=0.7, ao=0.0)
                if o.get('spikes'):
                    c.layer(S(spike_poly(fx, 48.5, -90 + tilt * 0.6, 5.5 + 4.5 * rich + o.get('spike_len', 0), 1.9)), o.get('spmat', 'blackiron'), relief='roof', k=0.7, sh=0.6, ink=0.5)
        else:
            c.decal(S(line_poly((38, 60), (62, 60), 0.3)), (0.02, 0.012, 0.01), 0.6, 'engrave')
            for dx in (-7, 0, 7):
                c.decal(S(line_poly((50 + dx, 52), (50 + dx * 0.9, 62), 0.28)), (0.02, 0.012, 0.01), 0.55, 'engrave')
            if o.get('plates'):
                for fx, hw, ln, tilt in FINGERS:
                    c.layer(S(rrect_poly(fx - hw * 0.8, 50.0, fx + hw * 0.8, 55.5, 1.6)), o.get('plmat', 'iron'), relief='bevel', r=1.3, k=1.0, sh=0.5, ao=0.0, wear=0.6)
            if o.get('spikes'):
                for fx, hw, ln, tilt in FINGERS:
                    c.layer(S(spike_poly(fx, 51.5, -90 + tilt * 0.6, 4.0 + 4 * rich, 1.5)), o.get('spmat', 'iron'), relief='roof', k=0.7, sh=0.5, ink=0.5)
        # cuff rims, straps and ornaments
        rim = o.get('rim', 'bronze' if rich > 0.3 else 'iron')
        c.layer(S(rrect_poly(35.0, 72.5, 65.0, 77.5, 1.2)), rim, relief='bevel', r=1.3, k=1.0, sh=0.7, ao=0.0)
        c.layer(S(rrect_poly(29.0, 91.0, 71.0, 96.0, 1.2)), rim, relief='bevel', r=1.3, k=1.0, sh=0.7, ao=0.0)
        if o.get('strap', style == 'leather'):
            c.layer(S(rrect_poly(33.0, 82.0, 67.0, 86.5, 0.8)), o.get('strapmat', 'blackleather'), relief='round', k=0.8, sh=0.5, ao=0.0)
            c.layer(S(rrect_poly(54.5, 80.6, 61.0, 88.0, 0.9)), 'iron', relief='bevel', r=0.9, k=1.0, sh=0.5, ao=0.0)
            c.decal(S(rrect_poly(56.0, 82.4, 59.6, 86.2, 0.4)), (0.02, 0.02, 0.02), 0.85, 'over')
        if o.get('wing'):
            for s_ in (-1, 1):
                feather_wing(c, 50 + s_ * 19, 86, s_, 0.95, o['wing'], n=4, a0=-95, a1=-20, length=24, width=3.0)
        if o.get('gem') is not None:
            c.gem(50, 84, 3.2 + 1.0 * rich, pal(o['gem']), facets=8, glow=0.5)
        if o.get('runes'):
            rune_row(c, (40, 62), (60, 62), 3, 2.3, o.get('runecol', acc), start=o.get('rstart', 0))
            if rich > 0.3:
                rune_row(c, (40, 85), (60, 85), 3, 1.8, o.get('runecol', acc), start=4)
        if o.get('cracks'):
            for k in range(4):
                x = 38 + k * 8 + rng.uniform(-1.5, 1.5)
                pts = bolt_path((x, 73), (x + rng.uniform(-5, 5), 57 + rng.uniform(0, 5)), rng, 6, 1.7)
                c.emit_decal(S(stroke(pts, 0.30, caps='round')), pal(o['cracks']), 0.95, glow=(0.9, 0.4), blur=0.25)
                if k % 2 == 0:
                    q = pts[3] + np.array([rng.uniform(-4, 4), rng.uniform(-4, -1)])
                    c.emit_decal(S(stroke(bolt_path(pts[3], q, rng, 3, 1.0), 0.22, caps='round')), pal(o['cracks']), 0.9, glow=(0.7, 0.3), blur=0.2)
        if o.get('embers'):
            embers(c, rng, 50, 30, 20, 18, o['embers'], color=ecol, glow=0.5)
        if o.get('flames'):
            for fx, hw, ln, tilt in FINGERS:
                a_ = math.radians(tilt)
                tx_, ty_ = fx + ln * math.sin(a_), base_y - ln * math.cos(a_)
                c.layer(S(flame_poly(tx_, ty_ + 1.5, 9 + 5 * rng.random(), 2.4, lean=rng.uniform(-1.5, 1.5))), FIRE, glow=((1, .45, .1), 1.6, 0.25), sh=0, ink=0, op=0.8)
        if o.get('skull'):
            skull(c, 50, 63, 4.2, glow=ecol, mat=o.get('skmat', 'bone'))


def arch_fist(c, o):
    """Closed gauntlet seen from the back with a long horn (Narwhal fist)."""
    rich = o['rich']
    mat = o.get('mat', 'steel')
    ecol, emat, acc, _ = elem_of(o)
    with c.local(rot=o.get('rot', 4), pivot=(50, 60)):
        # horn first (behind the fist)
        pts = bez([(50, 44), (49, 28), (56, 8)], 40)
        wd = np.linspace(7.0, 0.5, 40)
        horn = S(stroke(pts, wd, caps='round'))
        c.layer(horn, 'bone', relief='round', k=0.9, sh=1.0, ao=0.1, grain='y', glow=((.6, .85, 1.0), 4.0, 0.25))
        for i in range(9):
            t = 0.1 + i * 0.1
            j = int(t * 39)
            px, py = pts[j]
            w = wd[j]
            c.decal(S(line_poly((px - w * 0.95, py + w * 0.5), (px + w * 0.95, py - w * 0.5), 0.5, caps='flat')).clip(horn), (0.12, 0.08, 0.05), 0.55, 'engrave')
        cuff = [(34, 78), (66, 78), (72, 96), (28, 96)]
        c.layer(S(cuff), mat, relief='bevel', r=3.0, k=0.9, sh=1.0, ao=0.3)
        fist = rrect_poly(26, 42, 74, 82, 9.0)
        c.layer(S(fist), mat, relief='bevel', r=7.0, k=0.85, sh=1.0, ao=0.3)
        # curled finger plates and knuckles
        for i in range(4):
            fx = 33.5 + i * 11.0
            c.layer(S(rrect_poly(fx - 5.0, 43.5, fx + 5.0, 59.0, 4.5)), mat, relief='bevel', r=3.0, k=0.95, sh=0.6, ao=0.2)
            c.layer(S(circle_poly(fx, 46.5, 4.6, 20)), mat, relief='round', k=1.0, sh=0.6, ao=0.0)
            c.layer(S(rrect_poly(fx - 4.6, 60.0, fx + 4.6, 68.0, 3.0)), mat, relief='bevel', r=2.6, k=0.9, sh=0.5, ao=0.1)
        # thumb wrapped across
        c.layer(S(rrect_poly(24, 62, 56, 76, 6.5)), mat, relief='bevel', r=5.0, k=0.9, sh=0.8, ao=0.2)
        c.layer(S(rrect_poly(33, 78.0, 67, 83.0, 1.2)), o.get('rim', 'bronze'), relief='bevel', r=1.3, k=1.0, sh=0.7, ao=0.0)
        c.layer(S(rrect_poly(27, 92.0, 73, 97.0, 1.2)), o.get('rim', 'bronze'), relief='bevel', r=1.3, k=1.0, sh=0.7, ao=0.0)
        if o.get('gem') is not None:
            c.gem(50, 70, 3.4, pal(o['gem']), facets=8, glow=0.6)
        rune_row(c, (38, 88), (62, 88), 3, 1.9, o.get('runecol', acc), start=2)


def arch_bracers(c, o):
    """A pair of runic bracers (vambraces)."""
    rich = o['rich']
    mat = o.get('mat', 'steel')
    ecol, emat, acc, _ = elem_of(o)
    for k, (cx, cy, rot) in enumerate(((36, 52, -16), (62, 50, 14))):
        with c.local(rot=rot, pivot=(cx, cy)):
            body = [(cx - 12, cy - 26), (cx + 12, cy - 26), (cx + 8, cy + 26), (cx - 8, cy + 26)]
            c.layer(S(body), mat, relief='round', k=0.9, sh=1.0, ao=0.3, wear=1.0)
            c.layer(S(rrect_poly(cx - 13.5, cy - 29, cx + 13.5, cy - 22, 1.5)), o.get('rim', 'bronze'), relief='bevel', r=1.8, k=1.0, sh=0.6, ao=0.0)
            c.layer(S(rrect_poly(cx - 9.5, cy + 22, cx + 9.5, cy + 29, 1.5)), o.get('rim', 'bronze'), relief='bevel', r=1.8, k=1.0, sh=0.6, ao=0.0)
            c.decal(S(line_poly((cx - 1.0, cy - 21), (cx - 1.0, cy + 21), 0.35)), (0.02, 0.02, 0.03), 0.55, 'engrave')
            for yy in (-8, 6):
                c.layer(S(rrect_poly(cx - 11 + yy * 0.1, cy + yy - 1.6, cx + 11 - yy * 0.1, cy + yy + 1.6, 0.8)), 'blackleather', relief='round', k=0.8, sh=0.4, ao=0.0)
                c.layer(S(rrect_poly(cx + 4.5, cy + yy - 2.4, cx + 8.5, cy + yy + 2.4, 0.6)), 'iron', relief='bevel', r=0.6, k=1.0, sh=0.3, ao=0.0)
            rune_row(c, (cx - 5, cy - 14), (cx - 5, cy + 16), 4, 1.7, o.get('runecol', acc), start=k * 3)


def arch_claws(c, o):
    """Three curved claw blades on a knuckle bar (Когти)."""
    rich = o['rich']
    bmat = o.get('bmat', 'steel')
    ecol, emat, acc, _ = elem_of(o)
    with c.local(rot=o.get('rot', 18), pivot=(50, 80)):
        for k, (dx, ang, ln) in enumerate(((-17, -22, 50), (0, 0, 58), (17, 22, 50))):
            with c.local(rot=ang, pivot=(50 + dx, 80)):
                pts = bez([(50 + dx, 80), (50 + dx - 5 * np.sign(ang or 1) * 0.0 - 8, 52), (50 + dx + 4 * (1 if ang >= 0 else -1) - 1, 80 - ln)], 36)
                wd = 1.45 * np.concatenate([np.linspace(5.4, 3.2, 14), np.linspace(3.2, 0.2, 22)])
                bl = S(stroke(pts, wd * 0.95, wd * 0.4, caps='flat'))
                c.layer(bl, bmat, relief='roof', k=0.6, sh=1.0, glow=o.get('bglow'))
                c.decal(S(stroke(pts[2:-6], np.linspace(0.45, 0.15, len(pts) - 8))).clip(bl), (0.02, 0.02, 0.03), 0.55, 'engrave')
        bar = [(26, 80), (74, 80), (72, 90), (28, 90)]
        c.layer(S(bar), o.get('grip', 'leather'), relief='round', k=0.85, sh=1.0, ao=0.2)
        for dx in (-17, 0, 17):
            c.layer(S(rrect_poly(50 + dx - 5.8, 76, 50 + dx + 5.8, 82.5, 1.2)), o.get('trim', 'iron'), relief='bevel', r=1.3, k=1.0, sh=0.7, ao=0.0)
            rivet(c, 50 + dx, 85, 1.1, o.get('trim', 'iron'), sh=0.3)
        c.layer(S(rrect_poly(22, 88.5, 78, 92.5, 1.0)), o.get('trim', 'iron'), relief='bevel', r=1.0, k=1.0, sh=0.7, ao=0.0)
        c.layer(S(stroke([(24, 90), (14, 98)], 1.6, caps='round')), 'blackleather', relief='round', k=0.8, sh=0.4)
        c.layer(S(stroke([(76, 90), (86, 98)], 1.6, caps='round')), 'blackleather', relief='round', k=0.8, sh=0.4)


# ------------------------------------------------------------------ boots

def _boot_outline():
    pts = [(38.5, 12), (40.5, 30), (41.5, 48)]
    pts += list(bez([(41.5, 48), (40.5, 56.5), (30, 59)], 10))
    pts += list(bez([(30, 59), (17, 61.5), (10.5, 69.5)], 10))[1:]
    pts += list(bez([(10.5, 69.5), (6.5, 76), (10.5, 82.5)], 8))[1:]
    pts += [(73.5, 82.5), (75.5, 79)]
    pts += list(bez([(75.5, 79), (76, 66), (68.5, 54)], 10))[1:]
    pts += [(67.5, 30), (69, 12)]
    return _arr(pts)


def arch_boot(c, o):
    rich = o['rich']
    mat = o.get('mat', 'leather')
    sole = o.get('sole', 'blackleather')
    cuffm = o.get('cuff', mat)
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 8))
    op = o.get('op', 1.0)
    plated = o.get('plated', False)
    with c.local(rot=o.get('rot', 0)):
        # jets / back details drawn first so the boot overlaps them
        if o.get('jet'):
            c.layer(S(rrect_poly(70, 55, 86, 72, 3.0)), 'darksteel', relief='round', k=0.9, sh=0.9, ao=0.2)
            c.layer(S(rrect_poly(84, 53, 90, 74, 1.5)), 'iron', relief='bevel', r=1.5, k=1.0, sh=0.5, ao=0.0)
            for k, (dy, ln) in enumerate(((60.0, 18.0), (68.0, 14.0))):
                fl = bez([(90, dy - 2.4), (90 + ln * 0.5, dy - 1.2), (90 + ln, dy)], 12)
                fl2 = bez([(90, dy + 2.4), (90 + ln * 0.5, dy + 1.2), (90 + ln, dy)], 12)
                c.layer(S(np.vstack([fl, fl2[::-1]])), ICEGLOW, glow=((.5, .8, 1.0), 2.4, 0.5), sh=0, ink=0)
        if o.get('wing'):
            feather_wing(c, 66, 36, 1, 1.15, o['wing'], n=5, a0=-122, a1=-28, length=26, width=3.4)
        if o.get('horns'):
            for sx in (-1, 1):
                pts = bez([(53 + sx * 11, 30), (53 + sx * 24, 28), (53 + sx * 22, 9)], 20)
                c.layer(S(stroke(pts, np.linspace(3.6, 0.5, 20), caps='round')), 'bone', relief='round', k=0.9, sh=0.8, ao=0.1)
        if o.get('legs'):
            for k in range(4):
                a0 = 72 + k * 4
                pts = bez([(70, 72), (84 + k * 2, 60 + k * 3), (88, 78 + k * 4)], 14)
                c.layer(S(stroke(pts, np.linspace(0.9, 0.3, 14), caps='round')), 'blackiron', relief='round', k=0.9, sh=0.5, ink=0.4)
        # heel & sole
        c.layer(S([(6.5, 85), (7.5, 80.5), (76.5, 80.5), (77, 92), (54, 92), (54, 88), (13, 88)]), sole, relief='bevel', r=2.2, k=0.9, sh=1.0, ao=0.2, op=op)
        c.decal(S(line_poly((13, 88.2), (54, 88.2), 0.3, caps='flat')), (0.0, 0.0, 0.0), 0.5, 'engrave')
        body = S(_boot_outline())
        c.layer(body, mat, relief='round', r=9.0, k=0.62, sh=1.0, ao=0.3, op=op, wear=o.get('wear', 1.0))
        bs = body
        # toe cap, welt and ankle creases
        c.layer(S([(6.5, 74), (13, 66), (22, 61), (28.5, 66), (27, 83), (9, 83)]).clip(bs), o.get('toe', mat), relief='round', r=5.0, k=0.6, sh=0.4, ao=0.1, op=op)
        c.decal(S(stroke(bez([(41, 50), (31, 58.5), (22, 61.5)], 10), 0.3)), (0.02, 0.012, 0.01), 0.7, 'engrave')
        c.decal(S(line_poly((9, 80.2), (74, 80.2), 0.35, caps='flat')), (0.02, 0.012, 0.01), 0.75, 'engrave')
        if not plated:
            for k in range(3):
                pts = bez([(41.5, 50 - k * 2.6), (53, 53 - k * 2.6 + 2), (66.5, 49 - k * 2.6)], 10)
                c.decal(S(stroke(pts, 0.5)).clip(bs), (0.01, 0.006, 0.004), 0.45, 'mul', blur=0.6)
        # cuff (flared top)
        cuff = rrect_poly(35.5, 9.5, 71.5, 22, 2.6)
        c.layer(S(cuff), cuffm, relief='round', r=3.5, k=0.7, sh=0.8, ao=0.2, op=op)
        # straps / plates
        if plated:
            for k, y in enumerate((26, 34.5, 43)):
                c.layer(S(rrect_poly(39.5, y, 67.5, y + 7.8, 1.5)), mat, relief='bevel', r=2.0, k=0.95, sh=0.5, ao=0.1, wear=0.7)
            c.layer(S(rrect_poly(41, 49, 66, 57, 2.0)).clip(bs), mat, relief='bevel', r=2.0, k=0.95, sh=0.4, ao=0.1)
            for k in range(3):
                rivet(c, 41.5, 29 + k * 8.5, 0.9, 'iron', sh=0.2)
        else:
            for y in (30, 41):
                c.layer(S(rrect_poly(38.5, y, 68.5, y + 4.6, 0.8)), o.get('strap', 'blackleather'), relief='round', k=0.8, sh=0.5, ao=0.0, op=op)
                c.layer(S(rrect_poly(59.5, y - 1.0, 64.5, y + 5.6, 0.8)), o.get('buckle', 'iron'), relief='bevel', r=0.8, k=1.0, sh=0.4, ao=0.0)
                c.decal(S(rrect_poly(60.8, y + 0.9, 63.2, y + 3.7, 0.3)), (0.02, 0.02, 0.02), 0.85, 'over')
        # stitching
        for k in range(9):
            c.decal(S(line_poly((40.0 + k * 3.2, 23.4), (40.0 + k * 3.2, 25.0), 0.2, caps='flat')), (0.03, 0.02, 0.01), 0.55, 'over')
        # accessories
        if o.get('flames'):
            for k in range(6):
                x = 40 + k * 5.4
                fp = flame_poly(x, 10.5, 9 + rng.uniform(0, 5), 2.3, lean=rng.uniform(-1.5, 1.5))
                c.layer(S(fp), FIRE, glow=((1, .45, .1), 1.6, 0.3), sh=0, ink=0, op=0.9)
            c.glow_shape(S(rrect_poly(37, 9, 70, 22, 1)), (1, .4, .1), 3.0, 0.25)
        if o.get('runes'):
            rune_row(c, (46, 28), (60, 28), 3, 2.0, o.get('runecol', acc), start=o.get('rstart', 0))
        if o.get('gem') is not None:
            c.gem(53.5, 16, 2.6 + rich, pal(o['gem']), facets=8, glow=0.5)
        if o.get('hourglass'):
            c.layer(S([(50, 30), (58, 30), (54, 36), (58, 42), (50, 42), (54, 36)]), tinted((.55, .03, .04), 'matte', spec=0.4), relief='flat', k=0.4, sh=0.3, ink=0.5)
        if o.get('core'):
            c.orb(53.5, 38, 8.6, (.1, .02, .18), o['core'], glow=0.8, sh=0.9, nscale=7, twist=1.2, seed=7)
            for k in range(3):
                a = k * 2.1 + 0.9
                c.orb(53.5 + 17 * math.cos(a), 38 + 11 * math.sin(a), 2.4, (.12, .02, .2), o['core'], glow=0.5, sh=0.3, nscale=6)
        if o.get('orbit'):
            for k in range(3):
                a = k * 2.1 + 0.4
                c.orb(53.5 + 24 * math.cos(a), 52 + 7 * math.sin(a) - 4, 2.7, (.12, .02, .2), o['orbit'], glow=0.55, sh=0.4, nscale=8)
            ring = Shape([('add', ellipse_poly(53.5, 50, 25, 7.5, 0, 50)), ('sub', ellipse_poly(53.5, 50, 24.0, 6.5, 0, 50))])
            c.layer(ring, emit_mat([(0, o['orbit']), (1, (1, 1, 1))]), glow=(o['orbit'], 2.0, 0.35), sh=0, ink=0, op=0.7)
        if o.get('stars'):
            for k in range(7):
                spark(c, 42 + rng.uniform(0, 26), 20 + rng.uniform(0, 55), rng.uniform(0.9, 1.7), o['stars'])
        if o.get('crystals'):
            for k, (x, y, h, a) in enumerate(((44, 12, 11, -14), (54, 11, 14, 4), (63, 12, 10, 16))):
                crystal(c, x, y, h, 2.3, a, o['crystals'], glow=0.25)
        if o.get('wisps'):
            for k in range(5):
                x, y = 16 + k * 14 + rng.uniform(-3, 3), 80 + rng.uniform(-2, 8)
                c.decal(S(ellipse_poly(x, y, 9, 2.6, rng.uniform(-10, 10), 20)), (0.8, 0.82, 0.95), 0.18, 'add', blur=2.0)
        if o.get('trim') is not None:
            c.layer(S(rrect_poly(36.5, 20.5, 70.5, 23.5, 0.8)), o['trim'], relief='bevel', r=0.9, k=1.0, sh=0.3, ao=0.0, op=op)


# ------------------------------------------------------------------ helms, masks, crowns, circlet

def horn_pair(c, cx, y, spread, rise, mat='bone', w=4.2, ysep=0.0, sh=0.9):
    for sx in (-1, 1):
        pts = bez([(cx + sx * 20, y + 2), (cx + sx * (20 + spread), y - 2), (cx + sx * (18 + spread * 0.9), y - rise)], 26)
        c.layer(S(stroke(pts, np.linspace(w, 0.5, 26), caps='round')), mat, relief='round', k=0.9, sh=sh, ao=0.1, grain='y')


def arch_helm(c, o):
    rich = o['rich']
    mat = o.get('mat', 'steel')
    trim = o.get('trim', 'bronze' if rich > 0.3 else 'iron')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 12))
    eye = o.get('eye')
    if o.get('horns'):
        horn_pair(c, 50, 36, o.get('hspread', 17), o.get('hrise', 26), o.get('hmat', 'bone'), w=o.get('hw', 4.6))
    if o.get('wings'):
        for sx in (-1, 1):
            feather_wing(c, 50 + sx * 25, 44, sx, 1.1, o.get('wmat'), n=5, a0=-112, a1=-22, length=27, width=3.4)
    dome = ellipse_poly(50, 41, 25.5, 28, 0, 64)
    lower = [(26.5, 48), (73.5, 48), (71, 74), (61, 89), (50, 95), (39, 89), (29, 74)]
    body = S(dome, lower)
    c.layer(body, mat, relief='round', r=13.0, k=0.72, sh=1.0, ao=0.3, wear=o.get('wear', 1.0), dent=o.get('dent', 0.0))
    bs = body
    # face opening
    if o.get('visor', 'open') == 'open':
        face = [(34, 54), (66, 54), (63.5, 75), (56, 86), (50, 90), (44, 86), (36.5, 75)]
    else:
        face = [(33, 55), (67, 55), (67, 63), (33, 63)]
    c.decal(S(face), (0.015, 0.012, 0.014), 0.97, 'over')
    if o.get('visor', 'open') == 'open':
        c.layer(S(rrect_poly(47.2, 50, 52.8, 79, 1.6)), mat, relief='bevel', r=2.0, k=1.0, sh=0.7, ao=0.0, wear=0.6)
    else:
        for i in range(5):
            c.decal(S(circle_poly(38 + i * 6, 71 + (i % 2) * 5, 1.0, 8)), (0.015, 0.012, 0.014), 0.9, 'over')
        c.layer(S(rrect_poly(46.8, 63, 53.2, 90, 1.6)), mat, relief='bevel', r=2.0, k=1.0, sh=0.7, ao=0.0, wear=0.6)
    if eye is not None:
        for sx in (-1, 1):
            ey = 58.5 if o.get('visor', 'open') != 'open' else 61
            c.emit_decal(S(ellipse_poly(50 + sx * 9.5, ey, 3.6, 1.15, sx * 8, 14)), pal(eye), 1.0, glow=(2.0, 0.85), blur=0.35)
    # brow band and crest
    c.layer(S(rrect_poly(24.5, 44, 75.5, 52, 2.0)), trim, relief='bevel', r=2.2, k=1.0, sh=0.8, ao=0.0, wear=0.6)
    if o.get('crest', True):
        c.layer(S(rrect_poly(47.0, 13.5, 53.0, 46, 2.2)), o.get('crestmat', trim), relief='bevel', r=2.4, k=1.0, sh=0.7, ao=0.0, wear=0.6)
    for k in range(7):
        rivet(c, 29 + k * 7, 48, 1.0, 'iron' if trim == 'iron' else trim, sh=0.2)
    c.decal(S(stroke(bez([(27, 52), (28, 66), (36, 82)], 12), 0.3)), (0.02, 0.02, 0.03), 0.6, 'engrave')
    c.decal(S(stroke(bez([(73, 52), (72, 66), (64, 82)], 12), 0.3)), (0.02, 0.02, 0.03), 0.6, 'engrave')
    if o.get('spikes'):
        for k in range(5):
            c.layer(S(spike_poly(50, 14 + k * 8.5, -90, 5.0 - k * 0.4, 1.8)), 'blackiron', relief='roof', k=0.7, sh=0.4, ink=0.5)
    if o.get('plume'):
        for k in range(3):
            pts = bez([(50, 16), (50 + (k - 1) * 12, 2), (50 + (k - 1) * 26, 20 + k * 3)], 20)
            c.layer(S(stroke(pts, np.linspace(1.0, 3.8, 20), caps='round')), o['plume'], relief='round', k=0.8, sh=0.5, ink=0.4)
    if o.get('gem') is not None:
        c.gem(50, 32, 3.8 + 1.4 * rich, pal(o['gem']), facets=8, glow=0.7)
    if o.get('runes'):
        rune_row(c, (36, 36), (64, 36), 4, 1.9, o.get('runecol', acc), start=1)


def arch_mask(c, o):
    rich = o['rich']
    kind = o.get('kind', 'plain')
    mat = o.get('mat', 'bone')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 14))
    if o.get('horns'):
        horn_pair(c, 50, 30, o.get('hspread', 12), o.get('hrise', 22), o.get('hmat', 'blackiron'), w=o.get('hw', 4.4))
    if o.get('feathers'):
        for sx in (-1, 1):
            feather_wing(c, 50 + sx * 14, 24, sx, 1.0, o['feathers'], n=4, a0=-105, a1=-45, length=26, width=3.0)
    half = [(50, 17), (63, 18), (73, 26), (77, 40), (73, 58), (64, 76), (55, 89), (50, 92)]
    poly = sym_poly(half)
    mask = S(poly)
    c.layer(mask, mat, relief='round', r=14.0, k=0.62, sh=1.0, ao=0.3, wear=o.get('wear', 1.0))
    dark = (0.012, 0.008, 0.01)
    if kind == 'skull':
        for sx in (-1, 1):
            c.decal(S(ellipse_poly(50 + sx * 12.5, 43, 8.6, 9.6, sx * 8, 30)), dark, 0.97, 'over')
        c.decal(S([(50, 56), (44.5, 69), (55.5, 69)]), dark, 0.95, 'over')
        c.decal(S(rrect_poly(36, 77, 64, 90, 3.0)), (0.1, 0.08, 0.06), 0.6, 'over')
        for i in range(-5, 6):
            c.decal(S(line_poly((50 + i * 2.5, 77.5), (50 + i * 2.5, 89.5), 0.28, caps='flat')), dark, 0.9, 'over')
        c.decal(S(line_poly((38, 84), (62, 84), 0.3, caps='flat')), dark, 0.8, 'over')
        for k in range(6):
            c.decal(S(blob_poly(rng.uniform(32, 68), rng.uniform(24, 84), rng.uniform(4, 9), rng, n=14, jitter=0.3)), (0.38, 0.27, 0.14), 0.30, 'mul', blur=1.6, clip=mask)
        # cracks
        c.decal(S(stroke(bolt_path((60, 18.5), (56, 35), rng, 5, 1.6), 0.3)), dark, 0.8, 'over')
        c.decal(S(stroke(bolt_path((36, 20), (40, 31), rng, 4, 1.2), 0.25)), dark, 0.7, 'over')
        if o.get('eye') is not None:
            for sx in (-1, 1):
                c.emit_decal(S(circle_poly(50 + sx * 12.5, 45, 1.3, 12)), pal(o['eye']), 1.0, glow=(1.8, 0.8), blur=0.3)
    elif kind == 'demon':
        for sx in (-1, 1):
            c.decal(S(ellipse_poly(50 + sx * 13, 43, 9.5, 4.2, sx * -18, 24)), dark, 0.97, 'over')
            if o.get('eye') is not None:
                c.emit_decal(S(ellipse_poly(50 + sx * 13, 43.6, 6.0, 2.2, sx * -18, 20)), pal(o['eye']), 1.0, glow=(2.6, 1.0), blur=0.4)
            # brow ridges
            c.layer(S(stroke(bez([(50 + sx * 4, 36), (50 + sx * 14, 30), (50 + sx * 26, 36)], 12), np.linspace(2.4, 0.8, 12), caps='round')), mat, relief='round', k=0.9, sh=0.5, ao=0.0)
        c.decal(S([(48.4, 52), (51.6, 52), (53, 64), (47, 64)]), dark, 0.85, 'over')
        # grin with fangs
        grin = bez([(33, 72), (50, 86), (67, 72)], 18)
        c.decal(S(stroke(grin, 3.0, caps='round')), dark, 0.97, 'over')
        for i in range(-3, 4):
            t = (i + 3.5) / 7.0
            gx, gy = grin[int(t * 17)]
            c.layer(S(spike_poly(gx, gy - 2.4, 90, 4.6 if abs(i) != 1 else 7.0, 1.0)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.45)
    elif kind == 'tribal':
        for sx in (-1, 1):
            c.decal(S(ellipse_poly(50 + sx * 13, 42, 7.0, 3.4, sx * -10, 24)), dark, 0.97, 'over')
        c.decal(S(rrect_poly(46.8, 48, 53.2, 72, 2.0)), (0.05, 0.03, 0.02), 0.35, 'over')
        c.decal(S(ellipse_poly(50, 78, 9.5, 3.0, 0, 20)), dark, 0.95, 'over')
        stripe = o.get('stripe', (0.62, 0.08, 0.06))
        for k, y in enumerate((29, 53, 65)):
            c.decal(S(line_poly((27, y), (73, y), 1.3, caps='flat')).clip(mask), stripe, 0.88, 'over')
        for sx in (-1, 1):
            c.decal(S(line_poly((50 + sx * 6, 26), (50 + sx * 20, 40), 1.0, caps='flat')).clip(mask), (0.85, 0.82, 0.74), 0.8, 'over')
            c.decal(S(line_poly((50 + sx * 15, 72), (50 + sx * 26, 56), 1.0, caps='flat')).clip(mask), (0.85, 0.82, 0.74), 0.8, 'over')
    else:
        for sx in (-1, 1):
            c.decal(S(ellipse_poly(50 + sx * 12, 44, 6.8, 4.4, sx * -8, 24)), dark, 0.97, 'over')
        c.decal(S([(48.6, 50), (51.4, 50), (52.6, 64), (47.4, 64)]), dark, 0.55, 'over')
        c.decal(S(rrect_poly(41, 76, 59, 79.4, 1.5)), dark, 0.9, 'over')
        # strap loops
        for sx in (-1, 1):
            c.layer(S(rrect_poly(50 + sx * 27 - 1.6, 34, 50 + sx * 27 + 1.6, 52, 1.0)), 'leather', relief='round', k=0.8, sh=0.5, ao=0.0)
        c.decal(S(stroke(bez([(34, 24), (50, 20), (66, 24)], 14), 0.35)), (0.02, 0.01, 0.01), 0.55, 'engrave')
    if o.get('gem') is not None:
        c.gem(50, 30, 3.0 + rich, pal(o['gem']), facets=8, glow=0.6)


def arch_crown(c, o):
    rich = o['rich']
    mat = o.get('mat', 'gold')
    ecol, emat, acc, _ = elem_of(o)
    n = o.get('tines', 5)
    tall = o.get('tall', 30.0)
    tx = np.linspace(26, 74, n)
    heights = [tall * (0.55 + 0.45 * (1 - abs(i - (n - 1) / 2) / ((n - 1) / 2))) for i in range(n)]
    shape_t = []
    for x, h in zip(tx, heights):
        if o.get('style', 'spike') == 'leaf':
            shape_t.append(_arr(bez([(x - 4.5, 64), (x - 5.5, 64 - h * 0.55), (x, 64 - h)], 10).tolist() + bez([(x, 64 - h), (x + 5.5, 64 - h * 0.55), (x + 4.5, 64)], 10).tolist()[1:]))
        else:
            shape_t.append(_arr([(x - 4.2, 64), (x - 2.0, 64 - h * 0.5), (x, 64 - h), (x + 2.0, 64 - h * 0.5), (x + 4.2, 64)]))
    c.layer(S(*shape_t), mat, relief='roof', k=0.55, sh=1.0, ao=0.2, wear=o.get('wear', 1.0), glow=o.get('glow'))
    band = S(rrect_poly(23, 60, 77, 78, 3.0))
    c.layer(band, mat, relief='bevel', r=4.0, k=0.9, sh=1.0, ao=0.2, wear=o.get('wear', 1.0))
    c.layer(S(rrect_poly(22, 59, 78, 63, 1.2)), o.get('trim', mat), relief='bevel', r=1.5, k=1.0, sh=0.4, ao=0.0)
    c.layer(S(rrect_poly(22, 75.5, 78, 79.5, 1.2)), o.get('trim', mat), relief='bevel', r=1.5, k=1.0, sh=0.4, ao=0.0)
    gcols = o.get('gems', [])
    for i, gcol in enumerate(gcols):
        gx = 50 + (i - (len(gcols) - 1) / 2) * (36.0 / max(len(gcols) - 1, 1)) if len(gcols) > 1 else 50
        r = 4.2 if abs(gx - 50) < 1 else 3.0
        c.gem(gx, 69, r, pal(gcol), facets=8, glow=0.55 if rich > 0.3 else 0.2)
    for x, h in zip(tx, heights):
        if o.get('tipgem') is not None:
            c.gem(x, 64 - h - 1.0, 1.9, pal(o['tipgem']), facets=6, glow=0.45)
    if o.get('thorns'):
        for k in range(6):
            c.layer(S(spike_poly(27 + k * 9.2, 60, -90 + (k - 2.5) * 6, 4.5, 1.3)), mat, relief='roof', k=0.7, sh=0.3, ink=0.5)


def arch_circlet(c, o):
    """Sorcerer's ribbon: a cloth headband lying in perspective with a small gem and two tails."""
    rich = o['rich']
    cloth = o.get('mat', tinted((.18, .13, .3), 'matte', cloth=True))
    ecol, emat, acc, _ = elem_of(o)
    ring = Shape([('add', ellipse_poly(50, 56, 36, 17, 0, 64)), ('sub', ellipse_poly(50, 56, 31, 12.5, 0, 64))])
    for k, sx in enumerate((-1, 1)):
        pts = bez([(50 + sx * 20, 70), (50 + sx * 34, 80), (50 + sx * 30 + sx * k * 2, 94)], 20)
        c.layer(S(stroke(pts, np.linspace(4.0, 3.0, 20), caps='flat')), cloth, relief='round', k=0.7, sh=0.8, ao=0.1)
    c.layer(ring, cloth, relief='round', k=0.8, sh=1.0, ao=0.2)
    c.decal(S(stroke(ellipse_poly(50, 56, 33.5, 14.8, 0, 64, 20, 160), 0.45)), pal(o.get('thread', (0.75, 0.6, 0.3))), 0.7, 'over')
    c.layer(S(rrect_poly(44, 66.5, 56, 76, 2.5)), o.get('trim', 'gold'), relief='bevel', r=2.0, k=1.0, sh=0.8, ao=0.0)
    c.gem(50, 71, 3.6 + 1.2 * rich, pal(o.get('gem', (0.3, 0.5, 1.0))), facets=8, glow=0.7)


# ------------------------------------------------------------------ shields

def _shield_outline(kind):
    if kind == 'round':
        return ellipse_poly(50, 52, 38, 38, 0, 72)
    if kind == 'heater':
        top = bez([(20, 22), (50, 12), (80, 22)], 14)
        right = bez([(80, 22), (82, 52), (76, 68)], 10)[1:]
        rb = bez([(76, 68), (68, 84), (50, 94)], 12)[1:]
        pts = list(top) + list(right) + list(rb)
        left = [(100 - x, y) for x, y in pts[::-1]]
        return _arr(pts + left[1:-1])
    if kind == 'kite':
        top = bez([(26, 24), (50, 6), (74, 24)], 14)
        right = bez([(74, 24), (80, 50), (66, 74)], 12)[1:]
        rb = bez([(66, 74), (60, 84), (50, 96)], 10)[1:]
        pts = list(top) + list(right) + list(rb)
        left = [(100 - x, y) for x, y in pts[::-1]]
        return _arr(pts + left[1:-1])
    raise ValueError(kind)


def _scaled(poly, k, cx=50.0, cy=52.0):
    p = _arr(poly).copy()
    p[:, 0] = cx + (p[:, 0] - cx) * k
    p[:, 1] = cy + (p[:, 1] - cy) * k
    return p


def arch_shield(c, o):
    rich = o['rich']
    kind = o.get('kind', 'heater')
    rim = o.get('rim', 'iron')
    face = o.get('face', 'wood')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 15))
    outline = _shield_outline(kind)
    cx, cy = (50.0, 52.0) if kind == 'round' else (50.0, 50.0 if kind == 'heater' else 52.0)
    if o.get('horns'):
        horn_pair(c, 50, 32, o.get('hspread', 14), 22, o.get('hmat', 'bone'), w=4.2)
    base = S(outline)
    c.layer(base, rim, relief='round', r=7.0, k=0.9, sh=1.0, ao=0.3, glow=o.get('bglow'), wear=1.0)
    inner = _scaled(outline, o.get('inset', 0.86), cx, cy)
    fmat = face
    c.layer(S(inner), fmat, relief='round', r=8.0, k=0.55, sh=0.4, ao=0.25, grain='y', wear=0.8)
    fs = S(inner)
    # emblem
    em = o.get('emblem')
    emc = pal(o.get('emblem_col', (0.1, 0.2, 0.55)))
    if em == 'cross':
        c.decal(S(rrect_poly(44.5, 20, 55.5, 90, 1.0)).clip(fs), emc, 0.9, 'over')
        c.decal(S(rrect_poly(22, 40, 78, 51, 1.0)).clip(fs), emc, 0.9, 'over')
        c.decal(S(line_poly((50, 22), (50, 88), 0.5, caps='flat')), (0.9, 0.9, 0.95), 0.35, 'add')
    elif em == 'bars':
        for k, y in enumerate((34, 52, 70)):
            c.decal(S(rrect_poly(20, y - 3.5, 80, y + 3.5, 1.0)).clip(fs), emc, 0.88, 'over')
    elif em == 'scales':
        for r_ in range(7):
            for k_ in range(9):
                x = 20 + k_ * 7.4 + (r_ % 2) * 3.7
                y = 22 + r_ * 8.6
                a = ellipse_poly(x, y, 4.1, 4.1, 0, 14, 0, 180)
                c.decal(S(stroke(a, 0.35)).clip(fs), (0.03, 0.01, 0.01), 0.7, 'over')
                c.decal(S(ellipse_poly(x, y + 1.0, 3.2, 3.0, 0, 14, 0, 180)).clip(fs), emc, 0.35, 'over')
    elif em == 'plates':
        for k in range(4):
            c.decal(S(line_poly((50 - 24 + k * 16, 20), (50 - 24 + k * 16, 90), 0.4)).clip(fs), (0.02, 0.02, 0.02), 0.75, 'engrave')
    c.decal(S(stroke(np.vstack([inner, inner[:1]]), 0.45)), (0.02, 0.02, 0.03), 0.8, 'engrave')
    for k in range(o.get('rivets', 12)):
        a = 2 * math.pi * k / o.get('rivets', 12) - math.pi / 2
        ro = 36.5 if kind == 'round' else 0
        if kind == 'round':
            rivet(c, 50 + ro * math.cos(a), 52 + ro * math.sin(a), 1.2, rim, sh=0.2)
    if kind != 'round':
        idx = np.linspace(0, len(outline) - 1, o.get('rivets', 14), endpoint=False).astype(int)
        inn = _scaled(outline, 0.93, cx, cy)
        for i in idx:
            rivet(c, inn[i][0], inn[i][1], 1.1, rim, sh=0.2)
    # boss
    br = o.get('boss_r', 8.5)
    if o.get('boss') == 'skull':
        skull(c, cx, cy - 1, br * 1.15, glow=ecol if o.get('skull_eye', True) else None, mat='bone')
    elif o.get('boss') == 'dragon':
        horn_pair(c, cx, cy + 4, 7, 12, 'bronze', w=2.6, sh=0.4)
        c.layer(S(ellipse_poly(cx, cy, br * 0.95, br * 1.1)), 'bronze', relief='round', k=0.95, sh=0.8, ao=0.1)
        c.layer(S(spike_poly(cx, cy + br * 0.35, 90, br * 1.4, br * 0.55)), 'bronze', relief='round', k=0.9, sh=0.5, ao=0.1)
        for sx in (-1, 1):
            c.emit_decal(S(ellipse_poly(cx + sx * br * 0.45, cy - br * 0.2, br * 0.28, br * 0.15, sx * -20, 12)), pal(o.get('eye', (1, .3, .05))), 1.0, glow=(1.2, 0.7), blur=0.25)
    elif o.get('boss') == 'gem':
        c.layer(S(circle_poly(cx, cy, br, 28)), o.get('bossmat', 'steel'), relief='round', k=1.0, sh=0.9, ao=0.1)
        c.gem(cx, cy, br * 0.62, pal(o.get('gem', (.2, .8, .3))), facets=8, glow=0.7)
    elif o.get('boss') != 'none':
        c.layer(S(circle_poly(cx, cy, br, 28)), o.get('bossmat', 'steel'), relief='round', k=1.0, sh=0.9, ao=0.1)
        c.layer(S(circle_poly(cx, cy, br * 0.35, 14)), o.get('bossmat', 'steel'), relief='round', k=1.0, sh=0.3, ao=0.0)
    if o.get('runes'):
        rune_row(c, (30, 30), (70, 30), 5, 2.0, o.get('runecol', acc), start=2)
    if o.get('spikes'):
        for k in range(6):
            a = math.radians(-90 + k * 60 + 30)
            c.layer(S(spike_poly(cx + 38 * math.cos(a), cy + 38 * math.sin(a), math.degrees(a), 8, 2.4)), rim, relief='roof', k=0.7, sh=0.4, ink=0.5)
    if o.get('glowring'):
        ringp = Shape([('add', ellipse_poly(cx, cy, 31, 31, 0, 60)), ('sub', ellipse_poly(cx, cy, 29.8, 29.8, 0, 60))])
        c.layer(ringp, emit_mat([(0, o['glowring']), (1, (1, 1, 1))]), glow=(o['glowring'], 2.4, 0.4), sh=0, ink=0, op=0.85)


# ------------------------------------------------------------------ armour (front view cuirass) and robe

def arch_armor(c, o):
    rich = o['rich']
    mat = o.get('mat', 'steel')
    trim = o.get('trim', 'bronze' if rich > 0.3 else 'iron')
    cloth = o.get('cloth')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 17))
    # tabard / skirt cloth behind
    if cloth is not None:
        c.layer(S([(36, 66), (64, 66), (68, 98), (50, 94), (32, 98)]), cloth, relief='round', k=0.6, sh=0.6, ao=0.1)
    if o.get('rays'):
        for k in range(16):
            a = k * 22.5 + 11
            c.layer(S(spike_poly(50 + 24 * math.cos(math.radians(a)), 52 + 24 * math.sin(math.radians(a)), a, 24, 3.4 if k % 2 else 2.2)),
                    emit_mat([(0, (.75, .55, .15)), (1, (1, .95, .7))]), glow=((1, .85, .4), 2.0, 0.08), sh=0, ink=0, op=0.55)
    half = [(50, 22), (60, 19), (69, 25), (75, 35), (77, 47), (73, 61), (70.5, 72), (72, 83), (70, 93), (50, 96)]
    torso = S(sym_poly(half))
    c.layer(torso, mat, relief='round', r=9.0, k=0.78, sh=1.0, ao=0.3, wear=o.get('wear', 1.0), dent=o.get('dent', 0.0))
    ts = torso
    for yy, amp in ((56.5, 3.0), (62.5, 2.6)):
        c.decal(S(stroke(bez([(26, yy - amp), (50, yy + amp), (74, yy - amp)], 18), 0.45)).clip(ts), (0.01, 0.01, 0.02), 0.6, 'engrave')
    # pectoral split, keel, waist belt, tassets
    c.decal(S(line_poly((50, 26), (50, 70), 0.45, caps='flat')), (0.01, 0.01, 0.02), 0.75, 'engrave')
    for sx in (-1, 1):
        c.decal(S(stroke(bez([(50 + sx * 3, 34), (50 + sx * 14, 36), (50 + sx * 21, 52)], 14), 0.4)).clip(ts), (0.01, 0.01, 0.02), 0.55, 'engrave')
    c.layer(S(rrect_poly(27, 68, 73, 76.5, 1.6)).clip(ts), o.get('beltmat', 'blackleather'), relief='round', k=0.8, sh=0.5, ao=0.0)
    c.layer(S(rrect_poly(44.5, 66.5, 55.5, 78, 1.5)), trim, relief='bevel', r=1.8, k=1.0, sh=0.7, ao=0.0)
    for k in range(5):
        x0 = 28 + k * 8.9
        c.layer(S([(x0, 79), (x0 + 9.6, 79), (x0 + 8.8, 95), (x0 + 0.8, 95)]).clip(ts), mat, relief='bevel', r=2.2, k=0.9, sh=0.5, ao=0.1, wear=0.7)
    # gorget and neck opening
    c.decal(S(ellipse_poly(50, 22, 9.5, 4.8, 0, 28)), (0.012, 0.01, 0.012), 0.97, 'over')
    c.layer(Shape([('add', ellipse_poly(50, 22, 11.5, 6.4, 0, 32)), ('sub', ellipse_poly(50, 22.6, 9.2, 4.6, 0, 32))]), trim, relief='round', k=0.8, sh=0.4, ao=0.0)
    # pauldrons
    for sx in (-1, 1):
        px = 50 + sx * 29
        c.layer(S(ellipse_poly(px + sx * 3, 40, 14.5, 11.5, sx * 18, 36)), mat, relief='bevel', r=4.2, k=0.85, sh=0.8, ao=0.2, wear=0.8)
        c.layer(S(ellipse_poly(px + sx * 2.0, 32.5, 13, 9.0, sx * 24, 36)), mat, relief='bevel', r=3.8, k=0.9, sh=0.8, ao=0.2, wear=0.8)
        c.layer(S(ellipse_poly(px + sx * 1.0, 26.5, 11, 7.0, sx * 28, 36)), o.get('pmat', mat), relief='bevel', r=3.4, k=0.95, sh=0.8, ao=0.2, wear=0.8)
        if o.get('pspike'):
            c.layer(S(spike_poly(px + sx * 4, 20, -90 + sx * 28, 9, 2.4)), 'blackiron', relief='roof', k=0.7, sh=0.5, ink=0.5)
        rivet(c, px + sx * 3, 33, 1.1, trim, sh=0.2)
    # emblem on the chest
    em = o.get('emblem')
    if em == 'gem' and o.get('gem') is not None:
        c.layer(S(circle_poly(50, 50, 7.8, 24)), trim, relief='round', k=1.0, sh=0.8, ao=0.0)
        c.gem(50, 50, 5.6 + 0.8 * rich, pal(o['gem']), facets=8, glow=0.7)
    elif em == 'cross':
        c.decal(S(rrect_poly(47.2, 34, 52.8, 66, 0.8)), pal(o.get('emblem_col', (0.12, 0.25, 0.65))), 0.9, 'over')
        c.decal(S(rrect_poly(38, 42, 62, 47.5, 0.8)), pal(o.get('emblem_col', (0.12, 0.25, 0.65))), 0.9, 'over')
    elif em == 'gear':
        pts = []
        for i in range(16):
            a = i * math.pi / 8
            rr = 8.8 if i % 2 == 0 else 6.6
            pts.append((50 + rr * math.cos(a), 50 + rr * math.sin(a)))
        c.layer(Shape([('add', _arr(pts)), ('sub', circle_poly(50, 50, 3.0, 14))]), trim, relief='round', k=0.9, sh=0.8, ao=0.0)
    elif em == 'skull':
        skull(c, 50, 49, 7.0, glow=ecol, mat='bone')
    elif em == 'leaf':
        for k in range(3):
            a = -90 + (k - 1) * 40
            pts = bez([(50, 58), (50 + 10 * math.cos(math.radians(a)) * 0.6, 50 + 8 * math.sin(math.radians(a))), (50 + 15 * math.cos(math.radians(a)), 44 + 15 * math.sin(math.radians(a)) * 0.9)], 12)
            c.layer(S(stroke(pts, np.linspace(0.6, 3.2, 12), np.linspace(0.6, 3.2, 12), caps='round')), tinted((.2, .5, .2), 'matte'), relief='round', k=0.8, sh=0.3, ink=0.4)
    elif em == 'wings':
        pass
    if o.get('runes'):
        rune_row(c, (38, 56), (62, 56), 4, 1.9, o.get('runecol', acc), start=3)
    if o.get('mist'):
        for k in range(8):
            x, y = 28 + rng.uniform(0, 44), 30 + rng.uniform(0, 60)
            c.decal(S(ellipse_poly(x, y, rng.uniform(7, 13), rng.uniform(2, 4), rng.uniform(-30, 30), 18)), pal(o['mist']), 0.28, 'add', blur=1.6, clip=ts)
    if o.get('crack') is not None:
        pts = bolt_path((58, 30), (46, 70), rng, 7, 3.0)
        c.emit_decal(S(stroke(pts, 0.4, caps='round')).clip(ts), pal(o['crack']), 0.95, glow=(1.1, 0.55), blur=0.3)


def arch_robe(c, o):
    rich = o['rich']
    cloth = o.get('mat', tinted((.20, .14, .30), 'matte', cloth=True))
    trim = o.get('trim', 'gold')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 19))
    # body: narrow shoulders flaring to a long hem, wide sleeves
    half = [(50, 21), (58, 20), (66, 25), (70, 36), (73, 58), (79, 94), (50, 96)]
    body = S(sym_poly(half))
    c.layer(body, cloth, relief='round', r=13.0, k=0.5, sh=1.0, ao=0.2)
    for sx in (-1, 1):
        sl = [(50 + sx * 17, 26), (50 + sx * 31, 36), (50 + sx * 40, 62), (50 + sx * 33, 70), (50 + sx * 24, 58), (50 + sx * 20, 40)]
        c.layer(S(sl), cloth, relief='round', r=9.0, k=0.55, sh=0.8, ao=0.2)
    # hood: dark interior and the folded rim
    hood = ellipse_poly(50, 25, 15.5, 11.5, 0, 36)
    c.layer(S(hood), cloth, relief='round', r=6.0, k=0.7, sh=0.8, ao=0.2)
    c.decal(S(ellipse_poly(50, 25.5, 9.8, 7.4, 0, 30)), (0.012, 0.01, 0.015), 0.96, 'over')
    # folds
    for k in range(7):
        x = 30 + k * 6.6
        pts = bez([(50 + (x - 50) * 0.35, 40), (50 + (x - 50) * 0.8, 70), (50 + (x - 50) * 1.15, 95)], 14)
        c.decal(S(stroke(pts, 0.7)), (0.0, 0.0, 0.0), 0.5, 'mul', blur=1.2, clip=body)
    # trim along the front opening and hem
    c.layer(S(stroke([(50, 34), (50, 95)], 1.1, caps='flat')), trim, relief='round', k=0.8, sh=0.3, ao=0.0)
    c.layer(S(stroke(bez([(21, 94), (50, 98), (79, 94)], 16), 1.3, caps='flat')), trim, relief='round', k=0.8, sh=0.3, ao=0.0)
    c.layer(S(stroke([(26, 62), (74, 62)], 1.6, caps='flat')), 'blackleather', relief='round', k=0.8, sh=0.4, ao=0.0)
    c.layer(S(rrect_poly(46.0, 58.8, 54.0, 65.5, 1.0)), trim, relief='bevel', r=1.2, k=1.0, sh=0.4, ao=0.0)
    if o.get('gem') is not None:
        c.gem(50, 42, 3.4 + rich, pal(o['gem']), facets=8, glow=0.6)
    if o.get('runes'):
        rune_row(c, (30, 84), (70, 84), 5, 2.0, o.get('runecol', acc), start=2)


# ------------------------------------------------------------------ kusarigama (sickle + chain + weight)

def arch_kusari(c, o):
    rich = o['rich']
    bmat = o.get('bmat', 'darksteel')
    ecol, emat, acc, _ = elem_of(o)
    with c.local(rot=o.get('rot', 8), pivot=(50, 55)):
        # chain first, behind the handle
        path = bez([(30, 88), (40, 104), (66, 100), (75, 80)], 40)
        chain_links(c, path, 'iron', rx=1.7, ry=1.0, step=2.6, sh=0.4)
        # weight: spiked iron ball
        bx, by = 77.5, 70.0
        for k in range(8):
            a = k * 45 + 12
            c.layer(S(spike_poly(bx + 4 * math.cos(math.radians(a)), by + 4 * math.sin(math.radians(a)), a, 7.5, 2.0)), 'darksteel', relief='roof', k=0.7, sh=0.3, ink=0.5)
        c.orb(bx, by, 8.6, (.02, .02, .03), (.45, .47, .52), mode='solid', sh=0.9, shine=0.5, seed=3)
        # handle
        c.layer(S(stroke([(30, 90), (30, 38)], [2.6, 2.3], caps='flat')), o.get('haft', 'darkwood'), relief='round', k=0.9, sh=1.0, ao=0.2, grain='y')
        wraps(c, (30, 52), (30, 80), 2.6, 11, alpha=0.8)
        ferrule(c, 30, 88.5, 3.2, 'iron', 4.5)
        ferrule(c, 30, 36.5, 3.4, 'iron', 4.0)
        # sickle blade: edge on the concave side
        pts = bez([(30, 38), (28, 8), (64, 8), (76, 40)], 44)
        wl = np.concatenate([np.linspace(3.4, 4.4, 20), np.linspace(4.4, 0.3, 24)])
        wr = np.concatenate([np.linspace(2.4, 3.6, 20), np.linspace(3.6, 0.2, 24)])
        bl = S(stroke(pts, wl, wr, caps='flat'))
        c.layer(bl, bmat, relief='roof', k=0.7, sh=1.0, glow=o.get('bglow'))
        c.decal(S(stroke(pts[2:-8], np.linspace(0.45, 0.2, len(pts) - 10))).clip(bl), (0.02, 0.02, 0.03), 0.55, 'engrave')
        if o.get('glowline') is not None:
            c.emit_decal(S(stroke(pts[5:-10], 0.35, caps='round')).clip(bl), o['glowline'], 0.9, glow=(1.4, 0.45), blur=0.4)
        c.layer(S(rrect_poly(26.5, 38, 36, 45, 1.2)), 'iron', relief='bevel', r=1.2, k=1.0, sh=0.6, ao=0.0)


# ------------------------------------------------------------------ jewellery: ring, pendants, chain

def arch_ring(c, o):
    rich = o['rich']
    mat = o.get('mat', 'gold')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 21))
    cy = 63
    band = Shape([('add', ellipse_poly(50, cy, 29, 28.5, 0, 64)), ('sub', ellipse_poly(50, cy + 1, 18.5, 18.5, 0, 56))])
    c.layer(band, mat, relief='round', k=0.95, sh=1.0, ao=0.3, wear=o.get('wear', 1.0))
    # setting
    st = o.get('setting', 'claw')
    if st == 'shield':
        sp = [(36, 18), (64, 18), (64, 36), (50, 52), (36, 36)]
        c.layer(S(sp), mat, relief='bevel', r=3.4, k=1.0, sh=1.0, ao=0.1)
        c.gem(50, 31, 7.2, pal(o.get('gem', (.2, .45, 1.0))), facets=8, glow=0.7, shape='diamond', sq=1.1)
    elif st == 'spider':
        sx0, sy0 = 50, 31
        for k in range(4):
            for sd in (-1, 1):
                a0 = -62 + k * 38
                pts = bez([(sx0 + sd * 3, sy0), (sx0 + sd * (11 + k * 0.5), sy0 + (-12 + k * 8)), (sx0 + sd * (17 - k * 1.5), sy0 + (-3 + k * 9 + (k == 3) * 3))], 12)
                c.layer(S(stroke(pts, np.linspace(1.0, 0.45, 12), caps='round')), 'blackiron', relief='round', k=0.9, sh=0.4, ao=0.0, ink=0.45)
        c.layer(S(ellipse_poly(sx0, sy0 + 2.5, 5.6, 7.2)), 'blackiron', relief='round', k=0.95, sh=0.9, ao=0.1)
        c.layer(S(ellipse_poly(sx0, sy0 - 5, 3.5, 3.6)), 'blackiron', relief='round', k=0.95, sh=0.4, ao=0.0)
        c.layer(S([(sx0, sy0 + 3), (sx0 - 2.2, sy0 + 7.5), (sx0 + 2.2, sy0 + 7.5)]), tinted((.55, .03, .04), 'matte', spec=.5), relief='flat', k=0.4, sh=0.0, ink=0.4)
        for sd in (-1, 1):
            c.emit_decal(S(circle_poly(sx0 + sd * 1.4, sy0 - 5.6, 0.7, 8)), (1, .2, .15), 1.0, glow=(0.8, 0.6), blur=0.2)
    else:
        c.layer(S(ellipse_poly(50, 34, 12, 9)), mat, relief='round', k=0.95, sh=1.0, ao=0.1)
        for sd in (-1, 1):
            c.layer(S(spike_poly(50 + sd * 10, 38, 90 + sd * 35, 9, 1.8)), mat, relief='round', k=0.9, sh=0.4, ao=0.0, ink=0.4)
        c.gem(50, 29, 9.6 + 1.2 * rich, pal(o.get('gem', (.2, .8, .3))), facets=8, glow=0.75, shape=o.get('gemshape'), sq=o.get('sq', 1.0))
    if o.get('leaves'):
        for sd in (-1, 1):
            for k in range(3):
                pts = bez([(50 + sd * 26, 52 + k * 8), (50 + sd * 34, 46 + k * 8), (50 + sd * 38, 50 + k * 9)], 10)
                c.layer(S(stroke(pts, np.linspace(0.3, 2.4, 10), np.linspace(0.3, 2.4, 10), caps='round')), tinted((.2, .5, .2), 'matte'), relief='round', k=0.8, sh=0.3, ink=0.4)


def _chain_path(top_w=29, drop=70, sag=1.0):
    return bez([(50 - top_w, 4), (50 - top_w * 0.45, drop * 0.95), (50, drop * 1.12), (50 + top_w * 0.45, drop * 0.95), (50 + top_w, 4)], 46)


def arch_pendant(c, o):
    rich = o['rich']
    mat = o.get('mat', 'gold')
    cm = o.get('chain', 'iron')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 23))
    kind = o.get('kind', 'disc')
    py = o.get('py', 62)
    if kind == 'beads':
        path = bez([(18, 6), (20, 62), (50, 80), (80, 62), (82, 6)], 40)
        for i in range(15):
            t = i / 14.0
            x, y = path[int(t * 39)]
            col = o.get('beadmat', 'bone')
            r = 4.2 - 1.6 * abs(t - 0.5) * 2 * 0.3
            c.layer(S(circle_poly(x, y, r, 18)), col, relief='round', k=1.0, sh=0.6, ao=0.0, ink=0.6)
        pend = bez([(50, 84), (47, 92), (50, 99)], 8)
        c.layer(S(stroke(bez([(50, 82), (44, 92), (50, 101)], 12), np.linspace(3.6, 0.3, 12), caps='round')), 'bone', relief='round', k=0.9, sh=0.8, ao=0.1)
        return
    chain_links(c, _chain_path(o.get('top_w', 30), py - 4), cm, rx=1.6, ry=1.0, step=2.5, sh=0.3)
    cx = 50
    if kind == 'disc':
        c.layer(S(circle_poly(cx, py, 17, 36)), mat, relief='round', r=7.0, k=0.9, sh=1.0, ao=0.2, wear=o.get('wear', 1.0))
        c.layer(Shape([('add', circle_poly(cx, py, 17, 36)), ('sub', circle_poly(cx, py, 14.2, 36))]), o.get('rimmat', mat), relief='round', k=0.9, sh=0.0, ao=0.0)
        if o.get('gem') is not None:
            c.gem(cx, py, 8.0 + rich * 1.5, pal(o['gem']), facets=8, glow=0.7)
    elif kind == 'shield':
        sp = bez([(30, py - 18), (50, py - 22), (70, py - 18)], 8).tolist() + [(70, py + 4)] + bez([(70, py + 4), (66, py + 18), (50, py + 26)], 10).tolist()[1:] + bez([(50, py + 26), (34, py + 18), (30, py + 4)], 10).tolist()[1:]
        c.layer(S(sp), mat, relief='round', r=6.0, k=0.9, sh=1.0, ao=0.2, wear=o.get('wear', 1.0))
        c.layer(S(_scaled(sp, 0.72, 50, py)), o.get('facemat', 'bronze'), relief='bevel', r=3.0, k=0.8, sh=0.0, ao=0.1)
        c.gem(cx, py - 1, 7.0, pal(o.get('gem', (.25, .45, 1.0))), facets=8, glow=0.7)
    elif kind == 'moon':
        outer = ellipse_poly(cx, py, 20, 20, 0, 56)
        inner = ellipse_poly(cx + 8.5, py - 3.5, 17, 17, 0, 56)
        c.layer(Shape([('add', outer), ('sub', inner)]), mat, relief='round', k=0.95, sh=1.0, ao=0.1, wear=o.get('wear', 1.0), glow=o.get('bglow'))
        c.gem(cx - 11, py + 7, 3.4, pal(o.get('gem', (.75, .85, 1.0))), facets=6, glow=0.8)
        c.orb(cx + 6, py - 6, 6.0, (.2, .25, .4), (.8, .9, 1.0), glow=0.8, sh=0.4, nscale=7)
    elif kind == 'eye':
        eye = Shape.of(sym_poly([(50, py - 14), (62, py - 11), (74, py), (62, py + 11), (50, py + 14)])[:0] if False else _arr(bez([(24, py), (50, py - 24), (76, py)], 16).tolist() + bez([(76, py), (50, py + 24), (24, py)], 16).tolist()[1:]))
        c.layer(eye, mat, relief='round', k=0.9, sh=1.0, ao=0.2, wear=o.get('wear', 1.0))
        c.layer(S(_scaled(np.asarray(bez([(24, py), (50, py - 24), (76, py)], 16).tolist() + bez([(76, py), (50, py + 24), (24, py)], 16).tolist()[1:]), 0.72, 50, py)), tinted((.82, .78, .66), 'matte'), relief='round', k=0.7, sh=0.0, ao=0.0)
        c.orb(cx, py, 8.0, (.25, .0, .02), (1.0, .22, .12), mode='glass', glow=0.8, sh=0.0, nscale=6)
        c.decal(S(ellipse_poly(cx, py, 2.0, 5.5)), (0.0, 0.0, 0.0), 0.9, 'over')
    elif kind == 'sun':
        for k in range(12):
            a = k * 30
            c.layer(S(spike_poly(cx + 12 * math.cos(math.radians(a)), py + 12 * math.sin(math.radians(a)), a, 11 if k % 2 == 0 else 7, 2.6)), mat, relief='roof', k=0.7, sh=0.5, ink=0.5)
        c.layer(S(circle_poly(cx, py, 14, 36)), mat, relief='round', k=0.95, sh=1.0, ao=0.1)
        c.gem(cx, py, 7.5, pal(o.get('gem', (.85, .1, .1))), facets=8, glow=0.8)
    elif kind == 'swirl':
        oc = [(cx + 22 * math.cos(math.radians(a)), py + 22 * math.sin(math.radians(a))) for a in range(22, 382, 45)]
        c.layer(S(_arr(oc)), mat, relief='round', r=5.0, k=0.9, sh=1.0, ao=0.15, wear=o.get('wear', 1.0))
        c.orb(cx, py, 15.0, o.get('c1', (.35, .02, .05)), o.get('c2', (.3, .5, 1.0)), mode='glass', glow=0.7, sh=0.0, nscale=7, twist=1.4, swirl=1.0)
    elif kind == 'brooch':
        c.layer(S(spike_poly(cx - 14, py + 2, 90, 28, 1.3)), mat, relief='round', k=0.9, sh=0.8, ao=0.0)
        c.layer(Shape([('add', circle_poly(cx, py, 21, 40)), ('sub', circle_poly(cx, py, 15, 40))]), mat, relief='round', k=0.95, sh=1.0, ao=0.2)
        for sd in (-1, 1):
            feather_wing(c, cx + sd * 20, py - 4, sd, 0.75, tinted((.16, .1, .26), 'matte', cloth=True), n=3, a0=-110, a1=-40, length=18, width=2.4)
        c.gem(cx, py, 9, pal(o.get('gem', (.5, .25, .85))), facets=8, glow=0.8, shape='diamond', sq=1.1)
    elif kind == 'seal':
        for k in range(16):
            a = k * 22.5
            c.layer(S(spike_poly(cx + 17 * math.cos(math.radians(a)), py + 17 * math.sin(math.radians(a)), a, 3.2, 2.0)), mat, relief='roof', k=0.7, sh=0.2, ink=0.45)
        c.layer(S(circle_poly(cx, py, 19, 40)), mat, relief='round', r=7.0, k=0.95, sh=1.0, ao=0.2)
        c.layer(Shape([('add', circle_poly(cx, py, 16, 40)), ('sub', circle_poly(cx, py, 14.2, 40))]), 'blackiron', relief='round', k=0.6, sh=0.0, ao=0.0)
        pts = star_poly(cx, py, 11, 4.6, 5, -90)
        c.layer(S(pts), o.get('glyphmat', 'blackiron'), relief='round', k=0.7, sh=0.2, ao=0.0)
        c.emit_decal(S(circle_poly(cx, py, 2.2, 12)), pal(o.get('gem', (1, .3, .1))), 1.0, glow=(1.6, 0.8), blur=0.3)
        c.layer(S(stroke(bez([(cx - 6, py - 19), (cx - 11, 8), (cx - 3, 4)], 10), 1.1)), tinted((.5, .06, .06), 'matte', cloth=True), relief='round', k=0.8, sh=0.0)
    elif kind == 'fang':
        c.layer(S(stroke(bez([(cx - 6, py - 14), (cx - 10, py + 6), (cx + 2, py + 26)], 16), np.linspace(5.0, 0.4, 16), caps='round')), 'bone', relief='round', k=0.95, sh=1.0, ao=0.1)
        c.layer(S(rrect_poly(cx - 9.5, py - 18, cx + 0.5, py - 11, 1.5)), mat, relief='bevel', r=1.5, k=1.0, sh=0.6, ao=0.0)
    if o.get('runes'):
        rune_row(c, (cx - 8, py + 11), (cx + 8, py + 11), 3, 1.6, o.get('runecol', acc), start=2)


def arch_chain(c, o):
    """Heavy magic chain: a closed loop of large links with runes and a gem clasp."""
    rich = o['rich']
    mat = o.get('mat', 'silver')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 25))
    path = _arr([(50 + 30 * math.cos(math.radians(a)), 52 + 34 * math.sin(math.radians(a))) for a in range(-80, 281, 8)])
    chain_links(c, path, mat, rx=4.2, ry=2.7, step=5.6, sh=0.7, thick=1.3)
    # glowing links
    for i, a in enumerate(range(-80, 281, 8)):
        if i % 4 == 0:
            x, y = 50 + 30 * math.cos(math.radians(a)), 52 + 34 * math.sin(math.radians(a))
            c.emit_decal(S(circle_poly(x, y, 0.9, 8)), pal(acc), 0.9, glow=(1.2, 0.5), blur=0.3)
    c.layer(S(rrect_poly(40, 5, 60, 22, 4.0)), 'gold', relief='bevel', r=3.0, k=1.0, sh=1.0, ao=0.1)
    c.gem(50, 13.5, 5.2, pal(o.get('gem', (.3, .5, 1.0))), facets=8, glow=0.8)
    # hanging charm
    c.layer(S(rrect_poly(47.6, 84, 52.4, 92, 1.0)), 'gold', relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
    c.gem(50, 94, 4.2, pal(o.get('gem2', (.3, .5, 1.0))), facets=8, glow=0.6, shape='diamond', sq=1.3)


# ------------------------------------------------------------------ orbs on stands, cut stones, soul stone

def arch_orbitem(c, o):
    rich = o['rich']
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 27))
    mat = o.get('stand', 'bronze')
    c1, c2 = o['c1'], o['c2']
    cy = 44
    # stand
    c.layer(S([(33, 72), (67, 72), (60, 86), (40, 86)]), mat, relief='round', r=5.0, k=0.9, sh=1.0, ao=0.2)
    c.layer(S(rrect_poly(29, 68, 71, 75.5, 2.0)), mat, relief='bevel', r=2.2, k=1.0, sh=0.8, ao=0.0)
    c.layer(S(rrect_poly(34, 86, 66, 93, 2.0)), mat, relief='bevel', r=2.2, k=1.0, sh=0.8, ao=0.0)
    if o.get('crystals'):
        for k, (x, h, a) in enumerate(((22, 14, -28), (78, 12, 26), (34, 9, -14), (66, 10, 12))):
            crystal(c, x, 80 + (k > 1) * 4, h, 2.4, a, o['crystals'], glow=0.2)
    c.orb(50, cy, 27, c1, c2, mode='glass', glow=o.get('glow', 0.6), sh=0.9, seed=o.get('oseed', 1), nscale=o.get('nscale', 22), twist=o.get('twist', 0.6), swirl=o.get('swirl', 0.9))
    # front claws holding the sphere
    for sx in (-1, 1):
        pts = bez([(50 + sx * 14, 74), (50 + sx * 33, 62), (50 + sx * 29, 38)], 20)
        c.layer(S(stroke(pts, np.linspace(2.6, 1.0, 20), caps='round')), mat, relief='round', k=0.95, sh=0.6, ao=0.0, ink=0.55)
    if o.get('embers'):
        embers(c, rng, 50, 20, 24, 18, 9, color=o['embers'], glow=0.5)
    if o.get('bubbles'):
        for k in range(6):
            c.decal(S(circle_poly(38 + rng.uniform(0, 24), 50 + rng.uniform(0, 22), rng.uniform(1.0, 2.4), 12)), (0.9, 1.0, 0.7), 0.28, 'add', blur=0.4)
    if o.get('sparks'):
        for k in range(6):
            spark(c, 50 + rng.uniform(-26, 26), 44 + rng.uniform(-24, 24), rng.uniform(1.0, 1.9), o['sparks'])


def _cut_gem(c, cx, cy, s, col, glow=0.6):
    """Front view of a brilliant-cut stone; s scales the 100-unit design."""
    col = np.asarray(col, float)

    def P(pts):
        return [(cx + (x - 50) * s, cy + (y - 50) * s) for x, y in pts]
    outline = P([(26, 36), (38, 20), (62, 20), (74, 36), (50, 86)])
    facets = [
        ([(38, 20), (62, 20), (66, 33), (34, 33)], 1.25),
        ([(26, 36), (38, 20), (34, 33)], 0.95),
        ([(74, 36), (62, 20), (66, 33)], 0.60),
        ([(26, 36), (34, 33), (50, 36)], 0.80),
        ([(74, 36), (66, 33), (50, 36)], 0.50),
        ([(34, 33), (66, 33), (50, 36)], 1.0),
        ([(26, 36), (50, 36), (50, 86)], 0.75),
        ([(74, 36), (50, 36), (50, 86)], 0.42),
    ]
    c.glow_shape(S(outline), col, 9 * s, glow)
    c.layer(S(outline), tinted(col * 0.25), relief='flat', k=0.2, sh=1.0, ink=0.0)
    for pts, k in facets:
        m = tinted(np.clip(col * k * 0.95, 0, 1), spec=0.5, shin=30)
        c.layer(S(P(pts)), m, relief='flat', k=0.3, sh=0.0, ao=0.0, ink=0.35)
    c.decal(S(P([(40, 24), (56, 24), (52, 30), (38, 30)])), (1, 1, 1), 0.35, 'add', blur=0.5)
    spark(c, cx - 8 * s, cy - 22 * s, 2.6 * s, (1, 1, 1))


def arch_stone(c, o):
    """Cluster of rough crystals on a dark rock (health / mana stones)."""
    col = np.asarray(o['color'], float)
    rng = np.random.default_rng(o.get('seed', 51))
    c.layer(S(blob_poly(50, 84, 27, rng, n=24, jitter=0.1, squash=0.3)), 'stone', relief='round', k=0.7, sh=1.0, ao=0.3, dent=0.8)
    c.glow_shape(S(ellipse_poly(50, 60, 24, 26)), col, 12, o.get('glow', 0.55))
    for x, h, w, a_ in ((50, 56, 10.5, 0), (32, 40, 7.5, -20), (68, 43, 8.0, 18), (41, 27, 5.5, -36), (60, 29, 5.5, 32)):
        crystal(c, x, 82 + (x != 50) * 1.5, h, w, a_, col * 0.66, glow=0.0, sh=0.7)
    spark(c, 44, 30, 3.0, (1, 1, 1))


def arch_soulstone(c, o):
    rich = o['rich']
    rng = np.random.default_rng(o.get('seed', 29))
    blob = blob_poly(50, 62, 27, rng, n=30, jitter=0.1, squash=0.78)
    c.layer(S(blob), 'stone', relief='round', k=0.7, sh=1.0, ao=0.3, dent=1.0)
    for k in range(4):
        x = 36 + k * 9
        pts = bolt_path((x, 44 + rng.uniform(0, 6)), (x + rng.uniform(-6, 6), 78), rng, 5, 2.2)
        c.emit_decal(S(stroke(pts, 0.4, caps='round')), (0.4, 1.0, 0.8), 0.95, glow=(1.0, 0.5), blur=0.3)
    # soul flame
    for k, (dx, h, w) in enumerate(((0, 28, 6.0), (-8, 19, 4.0), (8, 17, 3.8))):
        c.layer(S(flame_poly(50 + dx, 46, h, w, lean=(k - 1) * 2.0)), SOUL, glow=((.3, .95, .8), 2.4, 0.5), sh=0, ink=0, op=0.88)
    for k in range(5):
        spark(c, 50 + rng.uniform(-20, 20), 12 + rng.uniform(0, 26), rng.uniform(0.8, 1.4), (.6, 1.0, .85))


# ------------------------------------------------------------------ staffs and rods

def _prongs(c, cx, y, n, h, mat, spread=9.0, w=1.8):
    for k in range(n):
        t = (k / (n - 1) - 0.5) * 2 if n > 1 else 0
        pts = bez([(cx + t * 2.0, y + 2), (cx + t * spread * 1.7, y - h * 0.35), (cx + t * spread, y - h)], 16)
        c.layer(S(stroke(pts, np.linspace(w * 1.1, w * 0.45, 16), caps='round')), mat, relief='round', k=0.95, sh=0.7, ao=0.0, ink=0.55)


def arch_staff(c, o):
    rich = o['rich']
    head = o.get('head', 'plain')
    smat = o.get('shaft', 'wood')
    mm = o.get('metal', 'iron')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 31))
    L = o.get('len', 1.0)
    y_top = 14 + (1 - L) * 36
    y_bot = 95
    hy = y_top + o.get('hy', 20)       # where the head sits on the shaft
    with c.local(rot=o.get('rot', 38)):
        wood_shaft(c, 50, hy, y_bot, 2.1, smat, 1.8)
        wraps(c, (50, y_bot - 30), (50, y_bot - 8), 2.1, 8, alpha=0.8)
        ferrule(c, 50, y_bot - 3.0, 2.6, mm, 4.0)
        band(c, (50 - 2.8, y_bot - 31), (50 + 2.8, y_bot - 31), 1.0, mm)
        if head == 'plain':
            kn = blob_poly(50, hy - 1, 6.5, rng, n=18, jitter=0.12)
            c.layer(S(kn), smat, relief='round', k=0.9, sh=0.9, ao=0.2, grain='y')
            for sx in (-1, 1):
                pts = bez([(50 + sx * 2, hy + 1), (50 + sx * 9, hy - 2), (50 + sx * 10, hy - 11)], 12)
                c.layer(S(stroke(pts, np.linspace(1.8, 0.4, 12), caps='round')), smat, relief='round', k=0.9, sh=0.5, grain='y')
            c.layer(S(rrect_poly(46, hy + 4, 54, hy + 8, 1.0)), mm, relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
        elif head == 'crystal':
            _prongs(c, 50, hy + 2, 3, 15, mm, spread=7.5)
            crystal(c, 50, hy - 1, 22, 5.2, 0, o.get('col', (.3, .5, 1.0)), glow=0.6)
            c.layer(S(rrect_poly(46, hy + 2, 54, hy + 6.4, 1.0)), mm, relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
        elif head == 'cross':
            c.layer(Shape([('add', circle_poly(50, hy - 8, 11.5, 36)), ('sub', circle_poly(50, hy - 8, 8.4, 36))]), mm, relief='round', k=0.95, sh=1.0, ao=0.1)
            c.layer(S(rrect_poly(48.6, hy - 17, 51.4, hy + 1, 0.6), rrect_poly(41, hy - 9.4, 59, hy - 6.6, 0.6)), mm, relief='bevel', r=0.8, k=1.0, sh=0.5, ao=0.0)
            c.gem(50, hy - 8, 4.0, pal(o.get('col', (.3, .95, .5))), facets=8, glow=0.8)
            for sx in (-1, 1):
                feather_wing(c, 50 + sx * 3, hy + 2, sx, 0.6, o.get('wmat'), n=3, a0=-45, a1=15, length=17, width=2.4)
        elif head == 'orb':
            _prongs(c, 50, hy + 3, 4, 13, mm, spread=10.5)
            c.orb(50, hy - 12, 10.5, o.get('c1', (.02, .05, .3)), o.get('c2', (.35, .55, 1.0)), glow=0.7, sh=0.8, nscale=12, seed=o.get('oseed', 3))
        elif head == 'mirror':
            _prongs(c, 50, hy + 3, 3, 12, mm, spread=8.0)
            c.orb(46, hy - 11, 9.5, (.1, .02, .22), (.65, .4, 1.0), glow=0.6, sh=0.8, nscale=11, seed=4)
            c.orb(56, hy - 15, 8.0, (.1, .02, .22), (.7, .5, 1.0), glow=0.5, sh=0.0, op=0.55, nscale=9, seed=5)
            for k in range(4):
                spark(c, 36 + k * 9, hy - 28 + (k % 2) * 6, 1.3, (.8, .7, 1.0))
        elif head == 'sun':
            for k in range(16):
                a = k * 22.5
                c.layer(S(spike_poly(50 + 9 * math.cos(math.radians(a)), hy - 12 + 9 * math.sin(math.radians(a)), a, 12 if k % 2 == 0 else 7, 2.4)), 'gold', relief='roof', k=0.7, sh=0.4, ink=0.5)
            c.layer(S(circle_poly(50, hy - 12, 11.5, 36)), 'gold', relief='round', r=5.0, k=0.95, sh=1.0, ao=0.1)
            c.orb(50, hy - 12, 7.2, (.5, .25, .02), (1.0, .9, .5), glow=1.0, sh=0.0, nscale=7, twist=0.4)
            c.glow_shape(S(circle_poly(50, hy - 12, 20)), (1.0, .85, .4), 12, 0.35)
        elif head == 'sphere':
            c.orb(50, hy - 14, 14, (.04, .06, .26), (.4, .6, 1.0), glow=0.75, sh=0.9, nscale=14, seed=6)
            for rot in (-62, 24):
                rg = Shape([('add', ellipse_poly(50, hy - 14, 21, 7.2, rot, 40)), ('sub', ellipse_poly(50, hy - 14, 19.4, 5.7, rot, 40))])
                c.layer(rg, 'gold', relief='round', k=0.9, sh=0.6, ao=0.0, ink=0.5)
            c.layer(S(rrect_poly(46, hy + 1, 54, hy + 5, 1.0)), 'gold', relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
            for sx in (-1, 1):
                c.gem(50 + sx * 17, hy - 20, 2.0, pal((.5, .8, 1.0)), facets=6, glow=0.5)
        elif head == 'grand':
            for sx in (-1, 1):
                feather_wing(c, 50 + sx * 3, hy + 2, sx, 1.0, tinted((.78, .74, .56), 'matte', cloth=True), n=5, a0=-100, a1=-5, length=26, width=3.0)
            _prongs(c, 50, hy + 3, 5, 18, 'gold', spread=11.5, w=1.9)
            crystal(c, 50, hy - 4, 26, 5.6, 0, (.85, .35, .95), glow=0.8)
            for sx in (-1, 1):
                crystal(c, 50 + sx * 12, hy - 4, 14, 3.4, sx * 18, (.35, .6, 1.0), glow=0.5)
            c.layer(S(rrect_poly(45, hy + 2, 55, hy + 7, 1.2)), 'gold', relief='bevel', r=1.2, k=1.0, sh=0.6, ao=0.0)
            c.gem(50, hy + 4.5, 2.3, pal((.9, .2, .2)), facets=6, glow=0.5)
        elif head == 'wand':
            c.layer(S(rrect_poly(46.4, hy - 4, 53.6, hy + 6, 1.2)), mm, relief='bevel', r=1.4, k=1.0, sh=0.6, ao=0.0)
            for k in range(3):
                c.layer(S(flame_poly(50 + (k - 1) * 4.5, hy - 5, 16 - abs(k - 1) * 5, 3.4, lean=(k - 1) * 2.4)), FIRE, glow=((1, .45, .1), 2.4, 0.5), sh=0, ink=0, op=0.92)
            c.orb(50, hy - 9, 5.8, (.3, .02, .0), (1.0, .45, .08), glow=0.9, sh=0.6, nscale=6)
            embers(c, rng, 50, hy - 24, 12, 8, 6, color=(1.0, .5, .1), glow=0.5)
        elif head == 'pure':
            for k in range(8):
                a = -90 + (k - 3.5) * 22
                c.layer(S(line_poly((50, hy - 14), (50 + 24 * math.cos(math.radians(a)), hy - 14 + 24 * math.sin(math.radians(a))), 0.6)), emit_mat([(0, (1, .95, .7)), (1, (1, 1, 1))]), glow=((1, .95, .7), 1.0, 0.3), sh=0, ink=0, op=0.7)
            _prongs(c, 50, hy + 3, 3, 12, 'gold', spread=6.5)
            _cut_gem(c, 50, hy - 12, 0.30, (.95, .95, .85), 0.9)
        elif head == 'censer':
            hook = bez([(50, hy), (50, hy - 14), (68, hy - 12), (68, hy - 3)], 20)
            c.layer(S(stroke(hook, np.linspace(2.2, 1.6, 20), caps='round')), smat, relief='round', k=0.9, sh=0.8, grain='y')
            chain_links(c, [(68, hy - 3), (68, hy + 12)], mm, rx=1.3, ry=0.8, step=2.1, sh=0.3)
            c.layer(S(ellipse_poly(68, hy + 22, 11, 11)), 'bronze', relief='round', k=0.95, sh=1.0, ao=0.1)
            c.layer(S(rrect_poly(60, hy + 11, 76, hy + 14.5, 1.0)), 'bronze', relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
            for k in range(5):
                c.decal(S(circle_poly(62.5 + k * 3.0, hy + 22 + (k % 2) * 3 - 1, 1.1, 8)), (1.0, .7, .25), 0.95, 'over')
            c.glow_shape(S(circle_poly(68, hy + 22, 9)), (1.0, .7, .3), 7, 0.55)
            for k in range(3):
                pts = bez([(68 + (k - 1) * 2, hy + 10), (68 + (k - 1) * 8, hy - 2), (68 + (k - 1) * 3, hy - 14)], 14)
                c.decal(S(stroke(pts, np.linspace(1.0, 3.0, 14))), (0.95, 0.9, 0.75), 0.22, 'add', blur=1.8)
        if o.get('runes'):
            rune_row(c, (50, y_bot - 54), (50, y_bot - 36), 3, 1.5, o.get('runecol', acc), start=1)


# ------------------------------------------------------------------ bows and crossbow

def arch_bow(c, o):
    rich = o['rich']
    typ = o.get('type', 'bow')
    limb = o.get('limb', 'wood')
    mm = o.get('metal', 'iron')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 33))
    strcol = o.get('string', (0.8, 0.78, 0.7))
    glow_str = o.get('glowstring')
    if typ == 'bow':
        with c.local(rot=o.get('rot', 135)):
            curve = bez([(64, 4), (40, 8), (21, 28), (24, 50), (21, 72), (40, 92), (64, 96)], 60)
            wd = np.concatenate([np.linspace(1.5, 3.4, 20), np.linspace(3.4, 4.2, 10), np.linspace(4.2, 3.4, 10), np.linspace(3.4, 1.5, 20)])
            bowshape = S(stroke(curve, wd, caps='round'))
            # string
            sp = [(64, 4.5), (64, 95.5)]
            if glow_str is not None:
                c.layer(S(stroke(sp, 0.35, caps='flat')), emit_mat([(0, glow_str), (1, (1, 1, 1))]), glow=(glow_str, 1.0, 0.35), sh=0, ink=0)
            else:
                c.layer(S(stroke(sp, 0.32, caps='flat')), tinted(strcol, 'matte'), relief='flat', k=0.1, sh=0.2, ink=0.2)
            # arrow
            if o.get('arrow', True):
                c.layer(S(stroke([(64, 50), (12, 50)], 0.75, caps='flat')), 'wood', relief='round', k=0.8, sh=0.6, ink=0.4)
                c.layer(S([(3, 50), (13, 46), (13, 54)]), o.get('headmat', 'steel'), relief='roof', k=0.6, sh=0.6, ink=0.5)
                for k in range(3):
                    x = 60 - k * 2.6
                    c.layer(S([(x, 50), (x + 5, 44.5), (x + 6, 44.5), (x + 2.6, 50), (x + 6, 55.5), (x + 5, 55.5)]), tinted((.78, .74, .66), 'matte', cloth=True), relief='flat', k=0.3, sh=0.0, ink=0.4)
            c.layer(bowshape, limb, relief='round', k=0.9, sh=1.0, ao=0.1, grain='y', glow=o.get('bglow'))
            # metal tips and grip
            for ty in (4.5, 95.5):
                c.layer(S(circle_poly(64, ty, 2.4, 14)), mm, relief='round', k=0.9, sh=0.5, ao=0.0)
            c.layer(S(rrect_poly(19.5, 41, 28.5, 59, 1.2)), o.get('grip', 'leather'), relief='round', k=0.8, sh=0.6, ao=0.0)
            wraps(c, (24, 42.5), (24, 57.5), 4.5, 6, alpha=0.8, slant=0.9)
            if o.get('gem') is not None:
                c.gem(23.5, 50, 2.6, pal(o['gem']), facets=6, glow=0.6)
            if o.get('runes'):
                rune_row(c, (25.5, 22), (22, 36), 3, 1.4, o.get('runecol', acc), start=2)
                rune_row(c, (22, 64), (25.5, 78), 3, 1.4, o.get('runecol', acc), start=5)
            if o.get('bolts'):
                for k in range(o['bolts']):
                    p0 = curve[8 + k * (44 // o['bolts'])]
                    bolt(c, p0 + np.array([-1, 0]), p0 + np.array([-10 - (k % 3) * 2, rng.uniform(-9, 9)]), rng, width=0.8, color=ecol, amp=2.4, segs=5, glow=0.7)
            if o.get('wings'):
                for sd in (-1, 1):
                    feather_wing(c, 22, 50 + sd * 7, -1, 1.0, o['wings'], n=5, a0=-60 if sd < 0 else 5, a1=-5 if sd < 0 else 60, length=24, width=3.0)
            if o.get('ornament'):
                for i in range(7):
                    p0 = curve[8 + i * 7]
                    c.gem(p0[0], p0[1], 1.15, pal(o['ornament']), facets=6, glow=0.3)
    else:
        with c.local(rot=o.get('rot', 30)):
            # stock
            stock = S([(46.5, 94), (53.5, 94), (55, 30), (45, 30)])
            c.layer(stock, limb, relief='round', k=0.8, sh=1.0, ao=0.2, grain='y')
            c.layer(S(rrect_poly(44, 78, 56, 88, 1.6)), mm, relief='bevel', r=1.5, k=1.0, sh=0.6, ao=0.0)
            # prod (the bow part)
            prod = bez([(8, 36), (30, 22), (50, 26), (70, 22), (92, 36)], 40)
            wd = np.concatenate([np.linspace(1.6, 3.6, 20), np.linspace(3.6, 1.6, 20)])
            c.layer(S(stroke(prod, wd, caps='round')), o.get('prod', 'darksteel'), relief='round', k=0.9, sh=1.0, ao=0.1, wear=o.get('wear', 1.0))
            # string + loaded bolt
            pts = [(8.5, 36), (50, 62), (91.5, 36)]
            c.layer(S(stroke(pts, 0.32, caps='flat')), tinted(strcol, 'matte'), relief='flat', k=0.1, sh=0.2, ink=0.2)
            c.layer(S(stroke([(50, 62), (50, 14)], 0.8, caps='flat')), 'wood', relief='round', k=0.8, sh=0.5, ink=0.4)
            c.layer(S([(50, 5), (46.2, 15), (53.8, 15)]), o.get('headmat', 'steel'), relief='roof', k=0.6, sh=0.4, ink=0.5)
            # stirrup and trigger housing
            c.layer(Shape([('add', ellipse_poly(50, 22, 5.5, 4.2)), ('sub', ellipse_poly(50, 22, 3.4, 2.4))]), mm, relief='round', k=0.9, sh=0.5, ao=0.0)
            c.layer(S(rrect_poly(44, 56, 56, 66, 1.4)), mm, relief='bevel', r=1.4, k=1.0, sh=0.7, ao=0.0)
            c.layer(S([(49, 66), (51, 66), (52, 74), (48, 74)]), 'blackiron', relief='bevel', r=0.8, k=1.0, sh=0.3, ao=0.0)
            if o.get('glowline') is not None:
                c.emit_decal(S(stroke(prod[5:-5], 0.4, caps='round')), o['glowline'], 0.9, glow=(1.2, 0.5), blur=0.3)
            if o.get('spikes'):
                for sx in (-1, 1):
                    c.layer(S(spike_poly(50 + sx * 41, 33, -90 + sx * 15, 10, 1.8)), 'blackiron', relief='roof', k=0.7, sh=0.4, ink=0.5)
            if o.get('gem') is not None:
                c.gem(50, 70, 2.4, pal(o['gem']), facets=6, glow=0.6)


# ------------------------------------------------------------------ misc objects: tome, horn, banner, totem, urn, pouch, chalice, chest

def arch_tome(c, o):
    rich = o['rich']
    cover = o.get('cover', 'leather')
    trim = o.get('trim', 'silver')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 35))
    with c.local(rot=o.get('rot', -6), pivot=(50, 52)):
        # page block (behind and below the cover)
        c.layer(S([(30, 18), (80, 18), (80, 93), (30, 93)]), 'parchment', relief='bevel', r=2.0, k=0.5, sh=1.0, ao=0.2)
        for k in range(6):
            c.decal(S(line_poly((78.8 - k * 0.0, 22 + k * 11), (78.8, 29 + k * 11), 0.15)), (0.25, 0.18, 0.1), 0.35, 'over')
        for k in range(4):
            c.decal(S(line_poly((34 + k * 12, 91.8), (40 + k * 12, 91.8), 0.15)), (0.25, 0.18, 0.1), 0.3, 'over')
        # cover
        c.layer(S(rrect_poly(22, 10, 74, 88, 2.4)), cover, relief='bevel', r=3.5, k=0.7, sh=1.0, ao=0.25, wear=1.0)
        # spine with raised bands
        c.layer(S(rrect_poly(22, 10, 31, 88, 2.4)), o.get('spine', cover), relief='round', k=0.8, sh=0.4, ao=0.2)
        for y in (22, 38, 54, 70):
            c.layer(S(rrect_poly(21.5, y - 1.3, 31.5, y + 1.3, 0.6)), trim, relief='bevel', r=0.8, k=1.0, sh=0.3, ao=0.0)
        # corner guards
        for (x0, y0, sx, sy) in ((74, 10, -1, 1), (74, 88, -1, -1), (22, 10, 1, 1), (22, 88, 1, -1)):
            poly = [(x0, y0), (x0 + sx * 11, y0), (x0 + sx * 11, y0 + sy * 2.4), (x0 + sx * 2.4, y0 + sy * 2.4), (x0 + sx * 2.4, y0 + sy * 11), (x0, y0 + sy * 11)]
            c.layer(S(poly), trim, relief='bevel', r=0.9, k=1.0, sh=0.4, ao=0.0, wear=0.6)
        # frame and emblem
        c.decal(S(stroke(np.vstack([rrect_poly(35, 20, 66, 78, 3.0), rrect_poly(35, 20, 66, 78, 3.0)[:1]]), 0.5)), (0.02, 0.012, 0.01), 0.55, 'engrave')
        em = o.get('emblem', 'gem')
        cx, cy = 50.5, 49
        if em == 'gem':
            c.layer(S(circle_poly(cx, cy, 11, 28)), trim, relief='round', k=0.95, sh=0.8, ao=0.1)
            c.gem(cx, cy, 7.2 + rich, pal(o.get('gem', (.3, .5, 1.0))), facets=8, glow=0.8)
            for k in range(4):
                a = math.radians(45 + 90 * k)
                c.layer(S(spike_poly(cx + 11 * math.cos(a), cy + 11 * math.sin(a), math.degrees(a), 6, 1.6)), trim, relief='roof', k=0.7, sh=0.2, ink=0.45)
        elif em == 'beads':
            c.layer(S(stroke(bez([(40, 44), (50, 54), (60, 44)], 12), 1.2)), 'blackleather', relief='round', k=0.8, sh=0.3)
            for k in range(7):
                x, y = bez([(38, 40), (50, 60), (62, 40)], 12)[1 + k * 1]
                c.layer(S(circle_poly(x, y, 2.2, 12)), 'darkwood', relief='round', k=1.0, sh=0.4, ao=0.0, ink=0.55)
        elif em == 'cross':
            c.layer(S(rrect_poly(48, 36, 53, 64, 0.8), rrect_poly(41, 43.5, 60, 48.5, 0.8)), trim, relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
        if o.get('runes'):
            ringp = Shape([('add', ellipse_poly(cx, cy, 17, 17, 0, 50)), ('sub', ellipse_poly(cx, cy, 16.0, 16.0, 0, 50))])
            c.layer(ringp, emit_mat([(0, acc), (1, (1, 1, 1))]), glow=(acc, 1.8, 0.4), sh=0, ink=0, op=0.85)
        # clasp strap
        c.layer(S(rrect_poly(66, 41, 80, 50, 1.0)), 'blackleather', relief='round', k=0.8, sh=0.6, ao=0.0)
        c.layer(S(rrect_poly(70, 39, 76, 52, 1.0)), trim, relief='bevel', r=1.0, k=1.0, sh=0.5, ao=0.0)
        if o.get('ribbon'):
            c.layer(S(stroke(bez([(58, 86), (60, 94), (56, 99)], 10), 1.6, caps='flat')), o['ribbon'], relief='round', k=0.8, sh=0.3)


def arch_horn(c, o):
    rich = o['rich']
    mat = o.get('mat', 'bone')
    trim = o.get('trim', 'bronze')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 37))
    path = bez([(84, 82), (58, 92), (22, 76), (20, 36)], 44)
    wd = 2.2 + 8.3 * np.linspace(0, 1, 44) ** 1.3
    # wind swirls first
    for k in range(5):
        r = 7 + k * 4.5
        a = np.linspace(-30, 150, 20)
        pts = np.stack([20 + r * np.cos(np.radians(a)) - 6, 30 - r * 0.6 * np.sin(np.radians(a)) * 1.0 - 2 - k], 1)
        c.decal(S(stroke(pts, 0.5 - 0.06 * k, caps='round')), (0.75, 0.88, 1.0), 0.35 - 0.04 * k, 'add', blur=0.8)
    horn = S(stroke(path, wd, caps='round'))
    c.layer(horn, mat, relief='round', k=0.9, sh=1.0, ao=0.15, glow=o.get('bglow'), grain='y')
    # bell mouth
    d = path[-1] - path[-5]
    ang = math.degrees(math.atan2(d[1], d[0]))
    bx, by = path[-1]
    c.layer(Shape([('add', ellipse_poly(bx, by, 4.5, 12.5, ang + 0, 30)), ('sub', ellipse_poly(bx, by - 0.5, 2.7, 10.6, ang, 30))]), trim, relief='round', k=0.9, sh=0.6, ao=0.0)
    c.decal(S(ellipse_poly(bx, by - 0.6, 2.6, 10.4, ang, 30)), (0.02, 0.012, 0.01), 0.96, 'over')
    # bands
    for t in (0.18, 0.5):
        j = int(t * 43)
        p = path[j]
        dd = path[min(j + 1, 43)] - path[max(j - 1, 0)]
        n_ = np.array([-dd[1], dd[0]]) / np.linalg.norm(dd)
        w = wd[j] * 1.02
        c.layer(S(line_poly(p + n_ * w, p - n_ * w, 1.3, caps='flat')), trim, relief='round', k=0.9, sh=0.3, ao=0.0)
    c.layer(S(circle_poly(83.5, 82, 3.4, 18)), trim, relief='round', k=0.95, sh=0.7, ao=0.0)
    # strap
    c.layer(S(stroke(bez([(58, 90), (56, 62), (36, 56)], 20), 1.1, caps='round')), 'blackleather', relief='round', k=0.8, sh=0.4)
    if o.get('gem') is not None:
        c.gem(46, 87, 2.4, pal(o['gem']), facets=6, glow=0.5)


def arch_banner(c, o):
    rich = o['rich']
    cloth = o.get('cloth', tinted((.46, .06, .07), 'matte', cloth=True))
    trim = o.get('trim', 'gold')
    ecol, emat, acc, _ = elem_of(o)
    with c.local(rot=o.get('rot', 8), pivot=(50, 55)):
        # pole
        wood_shaft(c, 22, 8, 96, 2.0, 'darkwood', 2.2)
        c.layer(S(spike_poly(22, 10, -90, 9, 3.0)), trim, relief='roof', k=0.7, sh=0.8, ink=0.5)
        c.layer(S(circle_poly(22, 12, 3.0, 16)), trim, relief='round', k=1.0, sh=0.5, ao=0.0)
        ferrule(c, 22, 92, 2.8, 'iron', 5.0)
        # crossbar
        c.layer(S(rrect_poly(19, 19, 84, 23.4, 1.0)), 'darkwood', relief='round', k=0.8, sh=0.8, ao=0.0, grain='x')
        c.layer(S(circle_poly(84, 21.2, 2.2, 14)), trim, relief='round', k=1.0, sh=0.4, ao=0.0)
        # flag
        top = [(26 + k * 4.4, 23 + 1.2 * math.sin(k * 0.9)) for k in range(14)]
        flag = _arr(top + [(82, 40), (80, 60)] + [(80, 74), (72, 68), (66, 86), (58, 78), (46, 92), (36, 80), (26, 88)] + [(25, 60), (26, 40)])
        c.layer(S(flag), cloth, relief='round', r=6.0, k=0.5, sh=1.0, ao=0.2)
        fs = S(flag)
        for k in range(5):
            x = 32 + k * 12
            pts = bez([(x, 24), (x + 2.5 * math.sin(k), 56), (x - 1.5, 88)], 14)
            c.decal(S(stroke(pts, 1.6)), (0.0, 0.0, 0.0), 0.35, 'mul', blur=1.8, clip=fs)
        c.layer(S(stroke(top, 1.3, caps='flat')), trim, relief='round', k=0.8, sh=0.2, ao=0.0)
        # emblem: laurel ring + star
        cx, cy = 53, 52
        for sd in (-1, 1):
            for k in range(7):
                a = math.radians(95 + k * 22) if sd < 0 else math.radians(85 - k * 22)
                lx, ly = cx + 15.5 * math.cos(a), cy + 15.5 * math.sin(a)
                c.layer(S(ellipse_poly(lx, ly, 3.4, 1.5, math.degrees(a) + 90 + sd * 40, 12)), trim, relief='round', k=0.9, sh=0.2, ao=0.0, ink=0.4)
        c.layer(S(star_poly(cx, cy, 10.5, 4.4, 5, -90)), trim, relief='round', k=0.9, sh=0.6, ao=0.0)
        c.layer(S(spike_poly(cx, cy + 14, 90, 8, 1.4)), trim, relief='roof', k=0.7, sh=0.2, ink=0.4)
        if o.get('gem') is not None:
            c.gem(cx, cy, 2.6, pal(o['gem']), facets=6, glow=0.6)


def arch_totem(c, o):
    rich = o['rich']
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 39))
    skin = o.get('skin', tinted((.07, .22, .27), 'matte', spec=.35, shin=22))
    c2 = o.get('c2', (.35, .85, .9))
    # base stones
    c.layer(S(blob_poly(50, 90, 24, rng, n=22, jitter=0.1, squash=0.26)), 'stone', relief='round', k=0.7, sh=0.9, ao=0.3, dent=0.8)
    # pole
    pole = S([(41, 24), (59, 24), (62, 90), (38, 90)])
    c.layer(pole, 'darkwood', relief='round', k=0.8, sh=1.0, ao=0.2, grain='y')
    for y in (36, 58, 78):
        c.layer(S(rrect_poly(37, y, 63, y + 3.6, 1.0)), 'bronze', relief='bevel', r=1.2, k=1.0, sh=0.5, ao=0.0)
    # serpent coils (S curve around the pole)
    pts = []
    for t in np.linspace(0, 1, 70):
        y = 86 - t * 62
        x = 50 + 15 * math.sin(t * 4.2 * math.pi)
        pts.append((x, y))
    pts = _arr(pts)
    body = S(stroke(pts, np.linspace(5.4, 3.4, 70), caps='round'))
    c.layer(body, skin, relief='round', k=0.95, sh=0.9, ao=0.15, glow=((.3, .8, .9), 3.0, 0.1))
    for i in range(1, 69, 3):
        p = pts[i]
        q = pts[i + 1] - pts[i - 1]
        a = math.degrees(math.atan2(q[1], q[0]))
        c.decal(S(ellipse_poly(p[0], p[1], 2.6, 0.7, a + 90, 10)), (0.01, 0.04, 0.05), 0.5, 'over')
    # head at the top
    head = S(ellipse_poly(50, 20, 9.5, 8.0, 0, 28), [(42, 18), (58, 18), (54, 28), (46, 28)])
    c.layer(head, skin, relief='round', k=0.95, sh=0.9, ao=0.1)
    for sd in (-1, 1):
        c.emit_decal(S(ellipse_poly(50 + sd * 4.6, 18, 2.4, 1.2, sd * -18, 12)), pal(c2), 1.0, glow=(1.4, 0.8), blur=0.3)
        c.layer(S(spike_poly(50 + sd * 3.0, 25, 90, 6, 0.9)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
    c.layer(S(stroke(bez([(50, 27), (49, 33), (53, 36)], 8), 0.5)), tinted((.7, .1, .15), 'matte'), relief='round', k=0.6, sh=0.0)
    # astral star above
    spark(c, 50, 7, 5.0, c2)
    for k in range(7):
        spark(c, 50 + rng.uniform(-22, 22), 6 + rng.uniform(0, 30), rng.uniform(0.7, 1.4), c2)
    # eye carvings on the pole
    for yy in (46, 68):
        for sd in (-1, 1):
            c.decal(S(ellipse_poly(50 + sd * 4.5, yy, 1.8, 1.0, 0, 10)), (0.02, 0.012, 0.01), 0.7, 'engrave')


def arch_urn(c, o):
    rich = o['rich']
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 41))
    body = _arr(bez([(40, 22), (22, 36), (24, 64), (38, 90)], 20).tolist() + [(62, 90)] + bez([(62, 90), (76, 64), (78, 36), (60, 22)], 20).tolist()[1:])
    ceram = o.get('ceramic', tinted((.12, .08, .16), 'matte', spec=.5, shin=26))
    # wisps from the opening
    for k in range(5):
        x0 = 44 + k * 3
        pts = bez([(x0, 16), (x0 + (k - 2) * 9, 4 + k * 0.3), (x0 + (k - 2) * 4, -10)], 14) + np.array([0, 4])
        c.layer(S(stroke(pts, np.linspace(1.0, 3.8, 14), caps='round')), VOID, glow=((.55, .3, .85), 2.2, 0.35), sh=0, ink=0, op=0.55)
    c.layer(S(body), ceram, relief='round', r=14.0, k=0.8, sh=1.0, ao=0.3, dent=0.5)
    # rim and base ring
    c.layer(S(ellipse_poly(50, 22, 14, 4.4)), o.get('rimmat', 'blackiron'), relief='round', k=0.9, sh=0.6, ao=0.0)
    c.decal(S(ellipse_poly(50, 22, 10.6, 2.9)), (0.01, 0.005, 0.015), 0.97, 'over')
    c.layer(S(rrect_poly(35, 88, 65, 94, 1.6)), o.get('rimmat', 'blackiron'), relief='bevel', r=1.8, k=1.0, sh=0.8, ao=0.0)
    # bands with glowing runes
    c.layer(S(rrect_poly(26.5, 42, 73.5, 47, 1.0)).clip(S(body)), o.get('rimmat', 'blackiron'), relief='bevel', r=1.2, k=1.0, sh=0.3, ao=0.0)
    c.layer(S(rrect_poly(27.5, 67, 72.5, 71, 1.0)).clip(S(body)), o.get('rimmat', 'blackiron'), relief='bevel', r=1.2, k=1.0, sh=0.3, ao=0.0)
    rune_row(c, (32, 56), (68, 56), 5, 2.3, o.get('runecol', (.65, .4, 1.0)), start=0)
    # handles
    for sd in (-1, 1):
        pts = bez([(50 + sd * 24, 34), (50 + sd * 38, 40), (50 + sd * 28, 54)], 16)
        c.layer(S(stroke(pts, 1.8, caps='round')), o.get('rimmat', 'blackiron'), relief='round', k=0.95, sh=0.7, ao=0.0)
    for k in range(3):
        c.decal(S(ellipse_poly(40 + k * 10, 80 + k % 2, 2.6, 1.0, 0, 10)), (0.7, 0.5, 1.0), 0.18, 'add', blur=1.2)


def arch_pouch(c, o):
    rich = o['rich']
    rng = np.random.default_rng(o.get('seed', 43))
    sack = _arr(bez([(36, 26), (14, 52), (16, 86), (50, 92)], 24).tolist() + bez([(50, 92), (84, 86), (86, 52), (64, 26)], 24).tolist()[1:])
    # coins spilling in front
    for k in range(9):
        x = 52 + k * 3.6 + rng.uniform(-2, 2)
        y = 92 - (k % 3) * 2.5 - rng.uniform(0, 3)
        c.layer(S(ellipse_poly(x, y, 5.0, 3.0, rng.uniform(-20, 20), 16)), 'gold', relief='round', k=0.9, sh=0.6, ao=0.0, ink=0.55)
    c.layer(S(sack), o.get('mat', 'leather'), relief='round', r=14.0, k=0.7, sh=1.0, ao=0.3, wear=1.0)
    ss = S(sack)
    for k in range(4):
        pts = bez([(36 + k * 8, 34), (30 + k * 12, 60), (28 + k * 14, 86)], 14)
        c.decal(S(stroke(pts, 1.4)), (0, 0, 0), 0.32, 'mul', blur=1.6, clip=ss)
    # gathered neck, cord and tie
    c.layer(S(ellipse_poly(50, 27, 16.5, 5.4)), o.get('mat', 'leather'), relief='round', k=0.9, sh=0.8, ao=0.1)
    c.decal(S(ellipse_poly(50, 25.5, 11.5, 3.0)), (0.01, 0.008, 0.005), 0.9, 'over')
    c.layer(S(stroke([(31, 31), (69, 31)], 1.6, caps='round')), tinted((.45, .3, .1), 'matte', cloth=True), relief='round', k=0.8, sh=0.3, ao=0.0)
    for sd in (-1, 1):
        pts = bez([(50, 32), (50 + sd * 7, 40), (50 + sd * 10, 52)], 12)
        c.layer(S(stroke(pts, 1.1, caps='round')), tinted((.45, .3, .1), 'matte', cloth=True), relief='round', k=0.8, sh=0.3)
    # coin peeking out of the neck
    for k, (x, y, r_) in enumerate(((42, 24, 4.4), (52, 21, 4.8), (60, 24, 4.2))):
        c.layer(S(ellipse_poly(x, y, r_, r_ * 0.62, rng.uniform(-25, 25), 14)), 'gold', relief='round', k=0.9, sh=0.4, ao=0.0, ink=0.55)
    c.decal(S(circle_poly(36, 60, 8, 20)), (1, 1, 1), 0.08, 'add', blur=3.0, clip=ss)
    # soft golden glow for the contents
    c.glow_shape(S(ellipse_poly(52, 24, 14, 6)), (1.0, .8, .3), 7, 0.35)


def arch_chalice(c, o):
    rich = o['rich']
    mat = o.get('mat', 'bronze')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 45))
    # soul wisps rising from the bowl
    for k, (dx, h, w) in enumerate(((0, 30, 5.5), (-9, 22, 4.0), (9, 20, 3.8), (-16, 12, 2.6), (16, 11, 2.4))):
        c.layer(S(flame_poly(50 + dx, 38, h, w, lean=(k % 3 - 1) * 2.2)), SOUL, glow=((.3, .95, .8), 2.6, 0.5), sh=0, ink=0, op=0.82)
    # bowl: half ellipse
    bowl = _arr(ellipse_poly(50, 40, 27, 30, 0, 40, 0, 180).tolist())
    c.layer(S(bowl), mat, relief='round', k=0.9, sh=1.0, ao=0.2, wear=1.0)
    # soul liquid surface
    c.layer(S(ellipse_poly(50, 40, 25.5, 6.0)), tinted((.2, .8, .65), 'matte', spec=.3), relief='round', k=0.6, sh=0.0, ao=0.0, ink=0.0)
    c.glow_shape(S(ellipse_poly(50, 40, 24, 5)), (.3, .95, .8), 6, 0.7)
    c.layer(S(rrect_poly(22, 36.5, 78, 41, 1.6)), mat, relief='bevel', r=1.6, k=1.0, sh=0.4, ao=0.0)
    # skull emblem on the bowl
    skull(c, 50, 58, 7.0, glow=(.3, .95, .8), mat='bone')
    # stem and foot
    c.layer(S([(45, 68), (55, 68), (54, 80), (46, 80)]), mat, relief='round', k=0.9, sh=0.8, ao=0.0)
    c.layer(S(ellipse_poly(50, 72, 8, 3.4)), mat, relief='round', k=0.95, sh=0.6, ao=0.0)
    c.layer(S([(30, 94), (70, 94), (62, 82), (38, 82)]), mat, relief='round', r=5.0, k=0.9, sh=1.0, ao=0.2)
    c.layer(S(rrect_poly(28, 91, 72, 96, 1.4)), mat, relief='bevel', r=1.6, k=1.0, sh=0.8, ao=0.0)
    for k in range(5):
        spark(c, 50 + rng.uniform(-22, 22), 6 + rng.uniform(0, 28), rng.uniform(0.8, 1.4), (.6, 1.0, .85))


def arch_chest(c, o):
    rich = o['rich']
    wood = o.get('wood', 'darkwood')
    trim = o.get('trim', 'gold')
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 47))
    # light spilling from under the lid
    c.glow_shape(S(rrect_poly(22, 48, 78, 54, 1)), o.get('lightcol', (.7, .45, 1.0)), 6, 0.6)
    # body
    c.layer(S(rrect_poly(16, 52, 84, 90, 2.4)), wood, relief='bevel', r=3.5, k=0.7, sh=1.0, ao=0.3, grain='x')
    # lid: arched top
    lid = _arr(bez([(16, 54), (16, 22), (84, 22), (84, 54)], 24).tolist() + [(16, 54)])
    c.layer(S(lid), wood, relief='round', r=9.0, k=0.6, sh=1.0, ao=0.3, grain='x')
    # glowing seam
    c.emit_decal(S(rrect_poly(17, 51.2, 83, 53.2, 0.8)), pal(o.get('lightcol', (.8, .55, 1.0))), 0.95, glow=(1.6, 0.6), blur=0.3)
    # metal bands
    for x in (26, 74):
        c.layer(S(rrect_poly(x - 3.4, 30, x + 3.4, 90, 0.9)), trim, relief='bevel', r=1.4, k=1.0, sh=0.5, ao=0.0, wear=0.7)
    c.layer(S(stroke(bez([(16, 54), (16, 22), (84, 22), (84, 54)], 24), 1.4, caps='flat')), trim, relief='round', k=0.9, sh=0.3, ao=0.0)
    c.layer(S(rrect_poly(16, 86, 84, 91.5, 1.0)), trim, relief='bevel', r=1.4, k=1.0, sh=0.5, ao=0.0)
    # corners and rivets
    for x in (21, 79):
        for y in (60, 82):
            rivet(c, x, y, 1.2, 'gold', sh=0.2)
    # lock plate with a glowing keyhole
    c.layer(S(rrect_poly(41, 46, 59, 68, 3.0)), trim, relief='bevel', r=3.0, k=1.0, sh=0.9, ao=0.1)
    c.emit_decal(S(circle_poly(50, 55, 2.6, 14), [(48.4, 56), (51.6, 56), (52.2, 63), (47.8, 63)]), pal(o.get('lightcol', (.8, .55, 1.0))), 1.0, glow=(1.8, 0.8), blur=0.3)
    # runic glow on the lid
    rune_row(c, (32, 38), (68, 38), 5, 2.4, o.get('lightcol', (.8, .55, 1.0)), start=2)
    for k in range(6):
        spark(c, 24 + rng.uniform(0, 52), 8 + rng.uniform(0, 28), rng.uniform(0.8, 1.5), o.get('lightcol', (.8, .55, 1.0)))


# ------------------------------------------------------------------ flasks, elixirs, consumable scrolls

def _flask_geom(shape):
    """(body polygon list, neck polygon, lip polygon, y_top_body, y_bottom, mid x)."""
    if shape == 'round':
        return [ellipse_poly(50, 67, 25, 25, 0, 48)], rrect_poly(43, 26, 57, 46, 1.5), rrect_poly(39.5, 21, 60.5, 28, 2.0), 42, 92
    if shape == 'big':
        return [ellipse_poly(50, 65, 30, 28, 0, 48)], rrect_poly(42, 26, 58, 44, 1.5), rrect_poly(38, 20, 62, 28, 2.0), 37, 93
    if shape == 'tall':
        return [rrect_poly(35, 36, 65, 94, 9.0)], rrect_poly(44, 14, 56, 40, 1.5), rrect_poly(41, 10, 59, 17, 2.0), 36, 94
    if shape == 'hex':
        return [_arr([(50, 34), (71, 48), (71, 78), (50, 94), (29, 78), (29, 48)])], rrect_poly(43, 18, 57, 38, 1.5), rrect_poly(39.5, 13, 60.5, 20, 2.0), 34, 94
    if shape == 'amph':
        body = _arr(bez([(42, 24), (26, 44), (28, 72), (42, 92)], 18).tolist() + [(58, 92)] + bez([(58, 92), (72, 72), (74, 44), (58, 24)], 18).tolist()[1:])
        return [body], rrect_poly(43, 12, 57, 28, 1.5), rrect_poly(39, 8, 61, 14, 2.0), 24, 92
    raise ValueError(shape)


def arch_flask(c, o):
    rich = o['rich']
    shape = o.get('shape', 'round')
    liq = np.asarray(o['liquid'], float)
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 53))
    bodies, neck, lip, ytop, ybot = _flask_geom(shape)
    full = S(*bodies, neck)
    ghost = o.get('ghost', False)
    # glass
    c.layer(full, tinted((.07, .09, .12), 'matte', spec=.5, shin=40), relief='round', r=10.0, k=0.6, sh=1.0, ao=0.1, op=0.55 if not ghost else 0.28, ink=0.55)
    level = o.get('fill', 0.72)
    ly = ybot - level * (ybot - ytop)
    liquid = S(*bodies).clip(S(rect_poly(0, ly, 100, 100)))
    if shape in ('round', 'big'):
        cxp = 50.0
        cyp = 67.0 if shape == 'round' else 65.0
        rr = 25.0 if shape == 'round' else 29.0
        mat = orb_mat(liq * 0.18, liq, 'glass', swirl=o.get('swirl', 0.5), twist=0.4, nscale=18, shine=0.0, seed=o.get('oseed', 2))
        mat['ctr'] = c.tx([[cxp, cyp]])[0]
        mat['rad'] = rr * U
        c.layer(liquid, mat, sh=0.0, op=0.95 if not ghost else 0.55, ink=0.0)
    else:
        c.layer(liquid, tinted(liq * 0.75, 'matte', spec=.4, shin=30), relief='round', r=12.0, k=0.7, sh=0.0, ao=0.0, op=0.95 if not ghost else 0.55, ink=0.0)
        c.glow_shape(liquid, liq, 6, 0.0)
    # meniscus
    c.decal(S(ellipse_poly(50, ly, 22 if shape != 'tall' else 14, 2.2 if shape != 'tall' else 1.6)).clip(full), np.clip(liq * 1.3 + 0.2, 0, 1), 0.5, 'add', blur=0.6)
    if o.get('glow', 0.0):
        c.glow_shape(liquid, liq, 7, o['glow'])
    # glass highlights and rim
    outline = np.vstack([bodies[0], bodies[0][:1]])
    c.decal(S(stroke(outline, 0.55, caps='round')), (1, 1, 1), 0.14, 'add', blur=0.4)
    bx0, bx1 = outline[:, 0].min(), outline[:, 0].max()
    by0, by1 = outline[:, 1].min(), outline[:, 1].max()
    c.decal(S(rrect_poly(bx0 + (bx1 - bx0) * 0.14, by0 + (by1 - by0) * 0.24, bx0 + (bx1 - bx0) * 0.22, by0 + (by1 - by0) * 0.62, 1.8)).clip(full), (1, 1, 1), 0.38, 'add', blur=0.9)
    c.decal(S(ellipse_poly(bx0 + (bx1 - bx0) * 0.30, by0 + (by1 - by0) * 0.2, 2.0, 1.3, -30, 12)).clip(full), (1, 1, 1), 0.55, 'add', blur=0.5)
    c.decal(S(stroke(bez([(bx1 - (bx1 - bx0) * 0.12, by0 + (by1 - by0) * 0.55), (bx1 - (bx1 - bx0) * 0.12, by0 + (by1 - by0) * 0.8), (bx1 - (bx1 - bx0) * 0.3, by1 - 3)], 12), 0.6)).clip(full), (1, 1, 1), 0.2, 'add', blur=0.7)
    if o.get('bubbles'):
        for k in range(7):
            x = 50 + rng.uniform(-14, 14)
            y = ly + 5 + rng.uniform(0, ybot - ly - 10)
            c.decal(S(circle_poly(x, y, rng.uniform(0.8, 2.0), 12)).clip(liquid), (1, 1, 1), 0.4, 'add', blur=0.4)
    if o.get('sparkle'):
        for k in range(8):
            spark(c, 50 + rng.uniform(-26, 26), 24 + rng.uniform(0, 64), rng.uniform(0.9, 1.7), o['sparkle'])
    # lip, cork and cord
    c.layer(S(lip), o.get('lipmat', 'iron'), relief='bevel', r=1.6, k=1.0, sh=0.7, ao=0.0)
    ctop = lip[:, 1].min()
    c.layer(S(rrect_poly(44.5, ctop - 9, 55.5, ctop + 1.5, 1.6)), o.get('cork', 'wood'), relief='round', k=0.8, sh=0.8, ao=0.0, grain='y')
    nk = neck[:, 1]
    c.layer(S(stroke([(43.5, nk.min() + 6), (56.5, nk.min() + 6)], 0.9, caps='round')), o.get('cord', tinted((.5, .06, .06), 'matte', cloth=True)), relief='round', k=0.8, sh=0.3, ao=0.0)
    if shape == 'amph':
        for sd in (-1, 1):
            pts = bez([(50 + sd * 14, 32), (50 + sd * 28, 36), (50 + sd * 24, 50)], 14)
            c.layer(S(stroke(pts, 1.8, caps='round')), o.get('lipmat', 'iron'), relief='round', k=0.95, sh=0.6, ao=0.0)
    if o.get('symbol') == 'cross':
        c.emit_decal(S(rrect_poly(48.4, ly + 6, 51.6, ly + 24, 0.5), rrect_poly(43, ly + 11, 57, ly + 14.2, 0.5)).clip(liquid), (1, 0.95, 0.7), 0.7, glow=(1.0, 0.3), blur=0.4)


def arch_scrollroll(c, o):
    """Consumable scroll: a rolled parchment tied with a coloured cord and a glowing sigil (no wax seal)."""
    rich = o['rich']
    ecol, emat, acc, _ = elem_of(o)
    rng = np.random.default_rng(o.get('seed', 55))
    sig = o.get('sigil', (1.0, .85, .4))
    cord = o.get('cord', tinted((.5, .38, .1), 'matte', cloth=True))
    with c.local(rot=o.get('rot', -32), pivot=(50, 52)):
        roll = rrect_poly(12, 36, 88, 64, 6.0)
        c.layer(S(roll), 'parchment', relief='round', k=0.95, sh=1.0, ao=0.1, edge=((0.18, 0.1, 0.03), 0.9, 0.55))
        for sd in (-1, 1):
            ex = 12 if sd < 0 else 88
            c.layer(S(ellipse_poly(ex, 50, 5.6, 14.2)), 'parchment', relief='round', k=0.9, sh=0.3, ao=0.0, ink=0.6, edge=((0.18, 0.1, 0.03), 0.5, 0.6))
            for k in range(4):
                c.decal(S(stroke(ellipse_poly(ex, 50, 4.6 - k * 1.0, 12.4 - k * 3.0, 0, 28), 0.3)), (0.28, 0.18, 0.08), 0.6, 'over')
        # cord around the middle + tails
        c.layer(S(rrect_poly(44, 34.5, 56, 65.5, 1.6)), cord, relief='round', k=0.8, sh=0.7, ao=0.0)
        for sd in (-1, 1):
            pts = bez([(50, 62), (50 + sd * 3.0, 72), (50 + sd * 6.0, 80)], 12)
            c.layer(S(stroke(pts, np.linspace(2.2, 1.4, 12), caps='round')), cord, relief='round', k=0.8, sh=0.4)
        # sigil
        kind = o.get('kind', 'ankh')
        if kind == 'ankh':
            ankh = Shape([('add', rrect_poly(48.6, 46, 51.4, 62, 0.5)), ('add', rrect_poly(43.5, 51.0, 56.5, 53.6, 0.5)), ('add', ellipse_poly(50, 42, 4.6, 5.6, 0, 20)), ('sub', ellipse_poly(50, 42, 2.6, 3.6, 0, 20))])
            c.emit_decal(ankh, pal(sig), 1.0, glow=(1.6, 0.8), blur=0.35)
        else:
            c.emit_decal(S(star_poly(50, 50, 8.4, 3.0, 4, 0)), pal(sig), 1.0, glow=(1.8, 0.9), blur=0.35)
            c.emit_decal(S(circle_poly(50, 50, 2.0, 12)), (1, 1, 1), 0.9, glow=(0.8, 0.4), blur=0.3)
        for k in range(5):
            spark(c, 14 + rng.uniform(0, 72), 22 + rng.uniform(0, 12) + (k % 2) * 40, rng.uniform(0.8, 1.4), sig)


# ------------------------------------------------------------------ figurines of summoned creatures (on a stone plinth)

def _plinth(c, mat='stone', rim='bronze'):
    c.layer(S(rrect_poly(22, 80, 78, 93, 3.0)), mat, relief='bevel', r=3.5, k=0.8, sh=1.0, ao=0.3, dent=0.6)
    c.layer(S(ellipse_poly(50, 80, 28, 7.5)), mat, relief='round', k=0.7, sh=0.4, ao=0.0, dent=0.6)
    c.layer(S(rrect_poly(20, 88, 80, 92.5, 1.2)), rim, relief='bevel', r=1.4, k=1.0, sh=0.5, ao=0.0, wear=0.7)


def arch_spirit(c, o):
    c1, c2 = np.asarray(o['c1'], float), np.asarray(o['c2'], float)
    rng = np.random.default_rng(o.get('seed', 57))
    _plinth(c)
    c.emit_decal(Shape([('add', ellipse_poly(50, 80, 22, 5.0, 0, 40)), ('sub', ellipse_poly(50, 80, 21, 4.4, 0, 40))]), pal(c2), 0.45, glow=(1.2, 0.22), blur=0.4)
    # trailing tail: two ribbons of ectoplasm
    for k, (dx, w0) in enumerate(((0, 15), (-9, 8), (10, 7))):
        tail = bez([(50 + dx * 0.4, 52), (36 + dx, 66), (62 + dx * 0.6, 74), (50 + dx * 0.3, 88 - abs(dx) * 0.4)], 26)
        c.layer(S(stroke(tail, np.linspace(w0, 1.2, 26), caps='round')), emit_mat([(0, c1 * 0.7), (0.6, c1 + (c2 - c1) * 0.5), (1, c2 * 0.9)]),
                glow=(c2, 3.0, 0.22), sh=0.0, ink=0, op=0.62 - 0.14 * (k > 0))
    c.orb(50, 40, 21, c1 * 0.55, c2 * 0.9, mode='glass', glow=0.5, sh=0.4, swirl=1.0, twist=0.9, nscale=16, op=0.88, seed=o.get('oseed', 1), shine=0.5)
    # streaming wisps above the head
    for k in range(5):
        x = 50 + (k - 2) * 7
        c.layer(S(flame_poly(x, 26 + abs(k - 2) * 2, 15 - abs(k - 2) * 3, 2.2, lean=(k - 2) * 1.6)), emit_mat([(0, c1), (1, c2)]), glow=(c2, 1.6, 0.15), sh=0, ink=0, op=0.42)
    # hollow face: tall slanted eye slits and a wailing mouth
    for sd in (-1, 1):
        c.decal(S(ellipse_poly(50 + sd * 7.6, 39, 2.3, 6.4, sd * -14, 16)), (0.0, 0.01, 0.01), 0.95, 'over')
        c.emit_decal(S(ellipse_poly(50 + sd * 7.6, 39.5, 0.8, 3.6, sd * -14, 12)), pal(np.clip(c2 * 1.2, 0, 1)), 0.9, glow=(0.7, 0.45), blur=0.2)
    c.decal(S(ellipse_poly(50, 53, 2.8, 4.0, 0, 14)), (0.0, 0.01, 0.01), 0.9, 'over')
    if o.get('symbol') == 'cross':
        c.emit_decal(S(rrect_poly(79.2, 24, 81.6, 36, 0.4), rrect_poly(74.4, 28, 86.4, 30.4, 0.4)), pal(c2), 0.8, glow=(1.2, 0.5), blur=0.3)
    else:
        c.emit_decal(S(star_poly(80, 30, 6.4, 2.0, 4, 0)), pal(c2), 0.8, glow=(1.2, 0.5), blur=0.3)
    for k in range(7):
        spark(c, 50 + rng.uniform(-30, 30), 10 + rng.uniform(0, 60), rng.uniform(0.6, 1.1), c2)


def arch_troll(c, o):
    rich = o['rich']
    rng = np.random.default_rng(o.get('seed', 59))
    skin = tinted(o.get('skin', (.23, .28, .2)), 'matte', spec=.15, shin=14, leather=True)
    dark = tinted(tuple(np.asarray(o.get('skin', (.23, .28, .2))) * 0.62), 'matte', leather=True)
    hair = tinted(o.get('hair', (.07, .055, .045)), 'matte', cloth=True)
    accent = tinted(o.get('hair_acc', (.36, .08, .06)), 'matte', cloth=True)
    _plinth(c, rim='iron')
    kind = o.get('kind', 'healer')
    # long stringy locks behind the head
    for sd in (-1, 1):
        for k in range(3):
            pts = bez([(50 + sd * (14 + k * 4), 26), (50 + sd * (30 + k * 4), 40 + k * 4), (50 + sd * (26 + k * 6), 70 + k * 3)], 16)
            c.layer(S(stroke(pts, np.linspace(2.4, 1.0, 16), caps='round')), hair, relief='round', k=0.9, sh=0.5, ao=0.0, ink=0.5)
    # crest of spikes (mohawk) in the accent colour
    for k in range(5):
        x = 50 + (k - 2) * 4.6
        c.layer(S(spike_poly(x, 23 + abs(k - 2) * 2.2, -90 + (k - 2) * 12, 9 - abs(k - 2) * 1.3, 1.8)), accent, relief='round', k=0.9, sh=0.4, ao=0.0, ink=0.5)
    # shoulders / hide armour
    c.layer(S([(22, 83), (28, 66), (72, 66), (78, 83)]), tinted((.17, .12, .08), 'matte', leather=True), relief='round', r=7.0, k=0.7, sh=0.8, ao=0.2)
    # ears
    for sd in (-1, 1):
        c.layer(S([(50 + sd * 17, 40), (50 + sd * 35, 18), (50 + sd * 31, 46), (50 + sd * 19, 52)]), skin, relief='round', r=4.0, k=0.8, sh=0.6, ao=0.1)
        c.decal(S([(50 + sd * 22, 38), (50 + sd * 31, 25), (50 + sd * 29, 42)]), (0.08, 0.04, 0.04), 0.4, 'mul', blur=0.6)
    # head: heavy brow, long face, strong jaw
    half = [(50, 24), (60, 25), (67, 33), (69, 46), (66, 60), (61, 70), (50, 74)]
    head = S(sym_poly(half))
    c.layer(head, skin, relief='round', r=14.0, k=0.62, sh=1.0, ao=0.3)
    # brow ridge + eye shadow
    c.layer(S(rrect_poly(33, 38, 67, 44.5, 2.8)), dark, relief='round', k=0.9, sh=0.4, ao=0.0)
    c.decal(S(rrect_poly(34, 43, 66, 51, 3.0)), (0.02, 0.01, 0.01), 0.55, 'mul', blur=1.4)
    for sd in (-1, 1):
        c.emit_decal(S(ellipse_poly(50 + sd * 8.4, 47.2, 1.9, 1.0, sd * 8, 10)), pal(o.get('eye', (1, .8, .2))), 1.0, glow=(0.8, 0.6), blur=0.2)
    # broad flat nose, scars, warts
    c.decal(S([(46.5, 47), (53.5, 47), (56, 58), (44, 58)]), (0.04, 0.05, 0.03), 0.4, 'mul', blur=0.8)
    for sd in (-1, 1):
        c.decal(S(circle_poly(50 + sd * 3, 57.5, 1.2, 8)), (0.01, 0.01, 0.01), 0.9, 'over')
    c.decal(S(stroke(bez([(57, 36), (60, 46), (57, 55)], 10), 0.35)), (0.6, 0.62, 0.5), 0.5, 'add', blur=0.3)
    for (x, y) in ((41, 52), (60, 63), (44, 36)):
        c.decal(S(circle_poly(x, y, 0.9, 8)), (0.14, 0.12, 0.08), 0.8, 'over')
    # mouth with an underbite and tusks
    c.decal(S(stroke(bez([(38, 64), (50, 70), (62, 64)], 12), 1.1)), (0.02, 0.01, 0.01), 0.95, 'over')
    for sd in (-1, 1):
        c.layer(S(spike_poly(50 + sd * 10.5, 66, -90 - sd * 10, 13, 2.1)), tinted((.78, .7, .52), 'matte', spec=.2), relief='round', k=0.9, sh=0.5, ao=0.0, ink=0.55)
    if kind == 'healer':
        # bone mask pushed up on the forehead, herb bundle, bone beads
        c.layer(S(rrect_poly(36, 25.5, 64, 33, 2.6)), 'bone', relief='bevel', r=2.2, k=0.8, sh=0.6, ao=0.0)
        for k in range(3):
            c.decal(S(line_poly((43 + k * 7, 26.5), (43 + k * 7, 32), 0.3)), (0.05, 0.03, 0.02), 0.8, 'over')
        c.layer(S(stroke(bez([(30, 69), (50, 80), (70, 69)], 14), 1.0)), 'blackleather', relief='round', k=0.8, sh=0.3)
        for k in range(5):
            x, y = bez([(30, 69), (50, 80), (70, 69)], 14)[2 + k * 2]
            c.layer(S(ellipse_poly(x, y + 1.5, 2.8, 1.5, 70 - k * 35, 12)), tinted((.2, .45, .16), 'matte'), relief='round', k=0.8, sh=0.2, ao=0.0, ink=0.45)
        c.layer(S(circle_poly(50, 79, 2.8, 14)), 'bone', relief='round', k=1.0, sh=0.4, ao=0.0)
    else:
        # iron half helm with a nasal bar, riveted pauldrons
        c.layer(S(ellipse_poly(50, 41, 21, 18, 0, 40, 180, 360), rect_poly(29, 41, 71, 44)), 'iron', relief='round', k=0.8, sh=0.9, ao=0.2)
        c.layer(S(rrect_poly(28, 38.5, 72, 45.5, 2.0)), 'bronze', relief='bevel', r=2.0, k=1.0, sh=0.6, ao=0.0)
        c.layer(S(rrect_poly(48.2, 24, 51.8, 56, 1.0)), 'iron', relief='bevel', r=1.2, k=1.0, sh=0.5, ao=0.0)
        c.layer(S(ellipse_poly(24, 76, 11, 8, -20, 24)), 'darksteel', relief='round', k=0.9, sh=0.7, ao=0.1)
        c.layer(S(ellipse_poly(76, 76, 11, 8, 20, 24)), 'darksteel', relief='round', k=0.9, sh=0.7, ao=0.1)
        rivet(c, 24, 76, 1.3, 'bronze', sh=0.2)
        rivet(c, 76, 76, 1.3, 'bronze', sh=0.2)


def arch_murloc(c, o):
    rich = o['rich']
    rng = np.random.default_rng(o.get('seed', 61))
    sk = np.asarray(o.get('skin', (.09, .2, .21)), float)
    skin = tinted(sk, 'matte', spec=.5, shin=24)
    belly = tinted((.42, .48, .36), 'matte', spec=.3)
    fin = tinted(np.asarray(o.get('fin', (.30, .10, .07)), float), 'matte', cloth=True)
    _plinth(c, rim='iron')
    kind = o.get('kind', 'hunter')
    if kind == 'hunter':
        wood_shaft(c, 78, 12, 84, 1.5, 'wood', 1.5)
        c.layer(S([(78, 4), (74.5, 17), (78, 15), (81.5, 17)]), 'iron', relief='roof', k=0.6, sh=0.7, ink=0.5)
        for sd in (-1, 1):
            c.layer(S(spike_poly(78 + sd * 1.5, 17, 90 + sd * 50, 5, 1.1)), 'iron', relief='roof', k=0.6, sh=0.3, ink=0.5)
        for k in range(3):
            c.layer(S(stroke(bez([(78, 24 + k * 3), (82, 28 + k * 3), (81, 33 + k * 3)], 8), 1.0, caps='round')), tinted((.7, .62, .45), 'matte', cloth=True), relief='round', k=0.8, sh=0.0, ink=0.4)
    else:
        wood_shaft(c, 23, 18, 84, 1.7, 'darkwood', 1.7)
        c.orb(23, 13, 6.2, (.02, .06, .28), (.35, .6, 1.0), glow=0.8, sh=0.5, nscale=8)
        _prongs(c, 23, 20, 3, 6, 'iron', spread=4.5, w=1.1)
    # body hint (shoulders) with a pale belly
    c.layer(S([(30, 84), (33, 68), (67, 68), (70, 84)]), skin, relief='round', r=8.0, k=0.7, sh=0.8, ao=0.2)
    c.layer(S([(41, 84), (43, 70), (57, 70), (59, 84)]), belly, relief='round', r=6.0, k=0.5, sh=0.0, ao=0.1)
    # dorsal fin: tall membrane with spines
    fin_pts = [(30, 44), (33, 20), (38, 30), (42, 12), (47, 26), (50, 8), (53, 26), (58, 12), (62, 30), (67, 20), (70, 44)]
    c.layer(S(fin_pts), fin, relief='round', r=4.0, k=0.6, sh=0.6, ao=0.1, ink=0.55)
    for x0, y0 in ((33, 20), (42, 12), (50, 8), (58, 12), (67, 20)):
        c.decal(S(line_poly((x0, y0 + 2), (x0 + (50 - x0) * 0.1, 40), 0.3)), (0.05, 0.02, 0.02), 0.6, 'over')
    # head: wide flattened oval
    c.layer(S(ellipse_poly(50, 52, 29, 20, 0, 48)), skin, relief='round', r=13.0, k=0.6, sh=1.0, ao=0.25)
    hs = S(ellipse_poly(50, 52, 29, 20, 0, 48))
    for r_ in range(5):
        for k_ in range(9):
            x = 26 + k_ * 6.0 + (r_ % 2) * 3.0
            y = 40 + r_ * 6.5
            c.decal(S(stroke(ellipse_poly(x, y, 3.0, 2.4, 0, 12, 0, 180), 0.28)).clip(hs), (0.01, 0.04, 0.04), 0.45, 'over')
    # gill flaps
    for sd in (-1, 1):
        for k in range(3):
            c.layer(S(spike_poly(50 + sd * 28, 49 + k * 5, 90 - sd * 90 + sd * 0, 7 - k, 1.3)), skin, relief='roof', k=0.6, sh=0.2, ink=0.45)
    # eyes: smaller, set high and wide, heavy lids, slit pupils
    for sd in (-1, 1):
        ex = 50 + sd * 15.5
        c.layer(S(circle_poly(ex, 39, 5.6, 24)), tinted((.55, .6, .22), 'matte', spec=.5, shin=30), relief='round', k=0.95, sh=0.7, ao=0.0, ink=0.65)
        c.decal(S(ellipse_poly(ex, 39.5, 1.3, 4.2, 0, 14)), (0.01, 0.01, 0.01), 0.96, 'over')
        c.decal(S(ellipse_poly(ex, 35.6, 6.0, 3.0, 0, 18, 180, 360)), (0.02, 0.05, 0.05), 0.9, 'over')
        c.decal(S(circle_poly(ex - 1.8, 37.0, 0.9, 8)), (1, 1, 1), 0.5, 'add', blur=0.3)
    # wide down-turned mouth full of needle teeth
    mouth = bez([(27, 55), (50, 68), (73, 55)], 20)
    c.decal(S(stroke(mouth, 2.0, caps='round')), (0.015, 0.01, 0.01), 0.96, 'over')
    for k in range(12):
        p = mouth[int(1 + k * 1.6)]
        ln = 3.0 + (k % 3) * 1.2
        c.layer(S(spike_poly(p[0], p[1] - 0.9, 90, ln, 0.75)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
        if k % 2 == 0:
            c.layer(S(spike_poly(p[0] + 1.6, p[1] + 1.0, -90, 2.4, 0.6)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
    for sd in (-1, 1):
        c.decal(S(circle_poly(50 + sd * 2.6, 47, 0.9, 8)), (0.01, 0.02, 0.02), 0.85, 'over')
    c.decal(S(ellipse_poly(40, 44, 8, 2.4, -14, 14)), (1, 1, 1), 0.12, 'add', blur=1.2)
    if kind == 'caster':
        c.glow_shape(S(ellipse_poly(50, 52, 30, 22)), (.3, .55, 1.0), 10, 0.12)
        for k in range(3):
            c.layer(S(circle_poly(34 + k * 16, 78, 2.0, 10)), 'bone', relief='round', k=1.0, sh=0.3, ao=0.0, ink=0.5)


def arch_demonstat(c, o):
    rich = o['rich']
    rng = np.random.default_rng(o.get('seed', 63))
    mat = o.get('mat', 'darkbronze')
    ecol, emat, acc, _ = elem_of(o)
    _plinth(c, rim='bronze')
    dark = tinted((.06, .04, .028), 'metal', spec=.2, shin=20)
    # wings: bony arms with membranes
    for sd in (-1, 1):
        shoulder = (50 + sd * 11, 52)
        tip = (50 + sd * 45, 10)
        mid1, mid2, mid3 = (50 + sd * 47, 36), (50 + sd * 36, 46), (50 + sd * 22, 54)
        memb = [shoulder, (50 + sd * 24, 28), tip, mid1, (50 + sd * 41, 38), mid2, (50 + sd * 31, 49), mid3, (50 + sd * 17, 56)]
        c.layer(S(memb), dark, relief='bevel', r=2.0, k=0.6, sh=0.9, ao=0.2)
        arm = bez([shoulder, (50 + sd * 28, 20), tip], 16)
        c.layer(S(stroke(arm, np.linspace(2.6, 1.0, 16), caps='round')), mat, relief='round', k=0.9, sh=0.5, ao=0.0)
        for (ex_, ey_) in (mid1, mid2, mid3):
            c.layer(S(stroke(bez([shoulder, ((shoulder[0] + ex_) / 2, (shoulder[1] + ey_) / 2 - 4), (ex_, ey_)], 10), np.linspace(1.4, 0.5, 10), caps='round')), mat, relief='round', k=0.9, sh=0.2, ao=0.0, ink=0.45)
    # tail
    c.layer(S(stroke(bez([(60, 82), (86, 86), (84, 62), (74, 58)], 20), np.linspace(2.6, 0.5, 20), caps='round')), mat, relief='round', k=0.9, sh=0.6, ao=0.0)
    c.layer(S(spike_poly(74, 58, 200, 6, 2.0)), mat, relief='roof', k=0.7, sh=0.3, ink=0.5)
    # crouching legs with clawed feet
    for sd in (-1, 1):
        c.layer(S(ellipse_poly(50 + sd * 18, 72, 8.5, 12.5, sd * 18, 24)), mat, relief='round', k=0.9, sh=0.8, ao=0.2)
        c.layer(S(ellipse_poly(50 + sd * 15.5, 84, 9, 3.6, 0, 16)), mat, relief='round', k=0.9, sh=0.5, ao=0.0)
        for k in range(3):
            c.layer(S(spike_poly(50 + sd * (15.5 + (k - 1) * 4.2) - 1.5 * sd, 85.5, 90, 4.0, 1.1)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
    # torso
    c.layer(S(ellipse_poly(50, 61, 12.5, 20, 0, 32)), mat, relief='round', k=0.8, sh=1.0, ao=0.3, wear=1.0)
    for k in range(4):
        c.decal(S(stroke(bez([(40.5, 52 + k * 5), (50, 54.5 + k * 5), (59.5, 52 + k * 5)], 10), 0.35)), (0.01, 0.005, 0.0), 0.6, 'engrave')
    # arms resting on the knees
    for sd in (-1, 1):
        arm = bez([(50 + sd * 14, 52), (50 + sd * 25, 62), (50 + sd * 22, 74)], 16)
        c.layer(S(stroke(arm, np.linspace(4.2, 3.0, 16), caps='round')), mat, relief='round', k=0.95, sh=0.6, ao=0.1)
        for k in range(3):
            c.layer(S(spike_poly(50 + sd * (22 + (k - 1) * 2.5), 75.5, 90 + (k - 1) * 12, 4.5, 1.0)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
    c.gem(50, 60, 3.4, pal(acc), facets=6, glow=0.7)
    # horned head
    horn_pair(c, 50, 36, 9, 12, mat, w=3.4, sh=0.5)
    c.layer(S(sym_poly([(50, 24), (58, 26), (62, 35), (59, 43), (54, 48), (50, 49)])), mat, relief='round', r=7.0, k=0.8, sh=0.8, ao=0.2, wear=1.0)
    for sd in (-1, 1):
        c.decal(S(ellipse_poly(50 + sd * 5.2, 34, 3.2, 1.6, sd * -22, 12)), (0.01, 0.005, 0.005), 0.95, 'over')
        c.emit_decal(S(ellipse_poly(50 + sd * 5.2, 34, 2.2, 0.9, sd * -22, 12)), pal(acc), 1.0, glow=(1.0, 0.7), blur=0.2)
    c.decal(S(stroke(bez([(45, 43), (50, 46.5), (55, 43)], 8), 0.7)), (0.02, 0.01, 0.01), 0.9, 'over')
    for sd in (-1, 1):
        c.layer(S(spike_poly(50 + sd * 3.2, 43.4, 90, 3.0, 0.7)), 'bone', relief='roof', k=0.6, sh=0.0, ink=0.4)
    c.decal(S(circle_poly(50, 62, 22, 24)), (0.35, 0.5, 0.35), 0.12, 'add', blur=4.0)


ARCHS = {
    'sword': arch_sword, 'kusari': arch_kusari, 'claws': arch_claws, 'axe': arch_axe, 'hammer': arch_hammer, 'polearm': arch_polearm,
    'bow': arch_bow, 'staff': arch_staff,
    'glove': arch_glove, 'fist': arch_fist, 'bracers': arch_bracers, 'boot': arch_boot,
    'helm': arch_helm, 'mask': arch_mask, 'crown': arch_crown, 'circlet': arch_circlet,
    'armor': arch_armor, 'robe': arch_robe, 'shield': arch_shield,
    'ring': arch_ring, 'pendant': arch_pendant, 'chain': arch_chain,
    'orbitem': arch_orbitem, 'stone': arch_stone, 'soulstone': arch_soulstone,
    'tome': arch_tome, 'horn': arch_horn, 'banner': arch_banner, 'totem': arch_totem, 'urn': arch_urn, 'pouch': arch_pouch,
    'chalice': arch_chalice, 'chest': arch_chest,
    'flask': arch_flask, 'scrollroll': arch_scrollroll,
    'spirit': arch_spirit, 'troll': arch_troll, 'murloc': arch_murloc, 'demonstat': arch_demonstat,
}

# ------------------------------------------------------------------ catalogue: item name -> (archetype, options)
# Options: el = element (colour of glow and background tint), rich = override of the price based richness 0..1,
# the rest is archetype specific. Levels (I, II) share the design of the base name; the ticks are added by the driver.

DESIGNS = {
    # swords, daggers and other blades
    'Стальной Меч': ('sword', dict(bl=0.62, w=4.6, gmat='iron', gspan=13, rich=0.0)),
    'Огненный Меч': ('sword', dict(el='fire', w=5.0, wave=1.1, waves=2.5, gmat='bronze', guard='curve', glowline=(1, .45, .08), flames=5, gem=(.9, .3, .05), pommel='ball', embers=True)),
    'Меч Пантилуса': ('sword', dict(w=6.0, belly=0.06, bmat='steel', gmat='gold', guard='cross', gspan=17, pommel='disc', gem=(.15, .3, .8), tip=0.2)),
    'Лезвие Демона': ('sword', dict(el='blood', bmat='darksteel', w=5.6, curve=-9, belly=0.12, back=0.15, serr=5, gmat='blackiron', guard='demon', glowline=(.8, .1, .1), pommel='spike')),
    'Меч Единства': ('sword', dict(el='holy', w=5.4, bmat='steel', gmat='gold', guard='ring', ring_r=7.5, gspan=14, pommel='disc', gem=(1.0, .9, .6), glowline=(1, .9, .55), tip=0.18)),
    'Меч Гнева': ('sword', dict(el='blood', bmat='hotiron', w=6.4, serr=4, serr_amp=1.3, belly=0.05, gmat='blackiron', guard='spiked', gspan=14, glowline=(.95, .25, .08), glow_variants=[(.95, .22, .08), (.95, .62, .12), (.72, .2, .95)], gem_variants=[(.9, .12, .1), (.95, .55, .1), (.6, .12, .75)], pommel_variants=['gem', 'spike', 'gem'], grip='redleather', blood=3)),
    'Ледяной Меч': ('sword', dict(el='ice', bmat='mithril', w=5.2, tip=0.2, gmat='silver', gspan=15, glowline=(.5, .85, 1), crystals=(.5, .8, 1), gem=(.5, .85, 1), pommel='gem', pgem=(.5, .85, 1), grip='blackleather')),
    'Призрачный Меч': ('sword', dict(el='soul', bmat='mithril', bop=0.62, w=5.0, bglow=((.3, .9, .75), 1.8, 0.45), wisps=(.5, .95, .85), glowline=(.5, 1, .9), gmat='silver', guard='curve', pommel='ring', grip='blackleather')),
    'Меч Крови': ('sword', dict(el='blood', bmat='steel', blood=7, glowline=(.8, .08, .08), w=4.4, tip=0.2, gmat='blackiron', guard='wing', gspan=15, grip='blackleather', gem=(.8, .05, .07), pommel='ring', bwear=1.4)),
    'Клинок Бестелесности': ('sword', dict(el='arcane', bmat='mithril', bop=0.5, curve=-7, w=4.0, tip=0.3, bglow=((.5, .6, 1), 1.6, 0.4), wisps=(.6, .7, 1), gmat='silver', guard='flat', gspan=9, bl=0.7, pommel='ring', grip='blackleather', fuller=False)),
    'Меч Гоуцзяня': ('sword', dict(bmat='verdigris', diamonds=True, w=3.8, tip=0.14, taper=0.05, gmat='gold', guard='flat', gspan=7, gth=2.0, pommel='disc', pmat='gold', grip='darkwood', bl=0.68, gl=0.2, gem=(.2, .7, .65), gem_r=1.8, el='holy', rich=0.9)),
    'Сокрушитель': ('sword', dict(bmat='darksteel', w=8.4, tip=0.08, taper=0.0, gmat='iron', guard='flat', gspan=17, gth=3.2, pommel='disc', pommel_r=5.2, grip='blackleather', el='bolt', runes=True, runecol=(.5, .7, 1), len=0.96, bl=0.62, fuller=False)),
    'Кинжал Разбойника': ('sword', dict(len=0.62, bl=0.62, w=3.6, gmat='blackiron', guard='curve', gspan=8, grip='blackleather', pommel='ball', pommel_r=3.0, el='poison', glowline=(.5, .9, .2), bmat='darksteel')),
    'Кусаригама': ('kusari', dict(el='blood', glowline=(.85, .15, .1))),
    'Когти': ('claws', dict(rich=0.0)),
    # axes and hammers
    'Топор': ('axe', dict(type='single', rich=0.0)),
    'Боевой Топор': ('axe', dict(type='double', hmat='steel')),
    'Огромный Топор': ('axe', dict(type='great', sz=1.25, hmat='darksteel', notches=3, hl=1.05)),
    'Секира Ярости': ('axe', dict(type='beard', el='fire', hmat='iron', glowline=(1,.4,.08), flames=True, spike=True)),
    'Секира Хана': ('axe', dict(type='beard', hmat='iron', haft='darkwood', trim='bronze', skulls=2, spike=True, notches=2, sz=1.1, rich=0.7)),
    'Секира У-син': ('axe', dict(type='great', five=True, hmat='bronze', engrave=True)),
    'Секира Дромандон': ('axe', dict(type='great', hmat='darksteel', glowline=(.5, .9, .2), el='poison', top_spike=True, notches=2, sz=1.15, rich=0.8)),
    'Молот Превосходства': ('hammer', dict(hmat='steel', cap='gold', gem=(.2,.4,.9), rivets=True)),
    'Молот Титанов': ('hammer', dict(hmat='stone', hw=24, hh=13, runes=True, el='arcane', cap='darksteel', hl=1.1)),
    'Молот Гексли': ('hammer', dict(hmat='darksteel', spike=True, claw=False, el='shadow', runes=True)),
    'Кузнечный Молот': ('hammer', dict(hmat='iron', hw=15, hh=9.5, hl=0.9, cap='darksteel', rich=0.0, sparks=True, rot=34)),
    # polearms
    'Копье': ('polearm', dict(head='spear', rich=0.0)),
    'Копье Аланита': ('polearm', dict(head='spear', hmat='steel', trim='gold', tassel=(.55, .06, .06), wing=True, hw=6.4, el='holy', rich=0.4)),
    'Копье Молний': ('polearm', dict(head='spear', el='bolt', hmat='darksteel', glowline=(.55,.75,1.0), bolts=3)),
    'Глефа Калдорай': ('polearm', dict(head='glaive', el='arcane', hmat='silver', glowline=(.5,.4,1.0), tassel=(.3,.2,.6), gem=(.5,.4,1))),
    'Эфирная Коса': ('polearm', dict(head='scythe', len=0.86, el='soul', hmat='mithril', glowline=(.4,.95,.8), bop=0.85, bglow=((.3,.9,.75),1.5,0.4))),
    'Кровавая Луна': ('polearm', dict(head='crescent', el='blood', hmat='darksteel', moon=(.6,.05,.07), gem=(.8,.1,.1))),
    # bows
    'Волшебный Лук': ('bow', dict(type='bow', limb='wood', metal='silver', glowstring=(.5, .7, 1.0), runes=True, runecol=(.5, .7, 1.0), el='arcane', gem=(.4, .6, 1.0), bglow=((.4, .6, 1.0), 1.4, 0.2))),
    'Лук Молний': ('bow', dict(type='bow', limb='darkwood', metal='darksteel', glowstring=(.7, .85, 1.0), bolts=6, bglow=((.55, .75, 1.0), 1.4, 0.18), el='bolt')),
    'Лук Дедала': ('bow', dict(type='bow', limb='bronze', metal='gold', ornament=(.4, .8, 1.0), headmat='gold', el='holy', grip='blackleather', wings=tinted((.82, .76, .55), 'matte', cloth=True))),
    'Арбалет Головореза': ('bow', dict(type='crossbow', limb='darkwood', prod='blackiron', spikes=True, el='blood', gem=(.85, .1, .1), headmat='darksteel')),
    # staffs and rods
    'Посох': ('staff', dict(head='plain', rich=0.0)),
    'Волшебный Посох': ('staff', dict(head='crystal', col=(.3, .5, 1.0), metal='silver', el='arcane')),
    'Посох Помощи': ('staff', dict(head='cross', metal='gold', col=(.3, .95, .5), wmat=tinted((.85, .85, .8), 'matte', cloth=True), el='poison')),
    'Посох Маны': ('staff', dict(head='orb', metal='silver', c1=(.02, .05, .3), c2=(.35, .55, 1.0), el='arcane', shaft='darkwood')),
    'Посох Иллюзий': ('staff', dict(head='mirror', metal='darksteel', shaft='darkwood', el='shadow')),
    'Посох Вечного Сияния': ('staff', dict(head='sun', metal='gold', el='holy')),
    'Сферический Посох': ('staff', dict(head='sphere', metal='gold', el='arcane', shaft='darkwood', runes=True)),
    'Посох Величия': ('staff', dict(head='grand', metal='gold', shaft='darkwood', el='holy', hy=24, runes=True)),
    'Жезл Огня': ('staff', dict(head='wand', metal='bronze', el='fire', len=0.74, hy=20)),
    'Чистейший Свет': ('staff', dict(head='pure', metal='gold', shaft=tinted((.7, .66, .56), 'matte', grain=True), el='holy')),
    'Кадило Вечного Света': ('staff', dict(head='censer', metal='bronze', el='holy', len=0.95, hy=24)),
    # gloves, gauntlets, bracers
    'Рунные Перчатки': ('glove', dict(style='plate', mat='steel', runes=True, el='arcane', runecol=(.45, .65, 1.0))),
    'Перчатки Скорости': ('glove', dict(style='leather', mat='leather', rich=0.0, strap=True, wing=tinted((.55, .62, .66), 'matte', cloth=True))),
    'Перчатки Силы': ('glove', dict(style='leather', mat='leather', plates=True, studs=True, rich=0.1, strapmat='redleather')),
    'Древняя Перчатка': ('glove', dict(style='plate', mat='verdigris', runes=True, el='holy', runecol=(1, .8, .4), rim='bronze')),
    'Адские Перчатки': ('glove', dict(style='plate', mat='blackiron', spikes=True, el='fire', cracks=(1, .35, .08), rim='darksteel')),
    'Перчатки Огня': ('glove', dict(style='plate', mat='rustiron', el='fire', flames=True, embers=7, rim='bronze', gem=(.95, .35, .05))),
    'Перчатки Боли': ('glove', dict(style='plate', mat='darksteel', spikes=True, el='blood', cracks=(.85, .1, .1), skull=True, rim='blackiron')),
    'Кулак Нарвала': ('fist', dict(mat='steel', el='ice', gem=(.4, .8, 1.0))),
    'Рунные Браслеты': ('bracers', dict(mat='steel', el='arcane', runecol=(.45, .65, 1.0))),
    # boots
    'Сапоги': ('boot', dict(rich=0.0)),
    'Сапоги-Невидимки': ('boot', dict(mat=tinted((.42, .40, .50), 'matte', cloth=True), sole=tinted((.3, .3, .38), 'matte'), op=0.8, wisps=True, el='arcane', strap=tinted((.3, .3, .4), 'matte'))),
    'Сапоги Вдовы': ('boot', dict(mat='blackleather', hourglass=True, legs=True, el='blood', strap='blackleather', trim='darksteel')),
    'Сапоги Защитника': ('boot', dict(plated=True, mat='steel', cuff='darksteel', toe='steel', gem=(.2, .45, 1.0), el='arcane')),
    'Реактивные Сапоги': ('boot', dict(plated=True, mat='darksteel', jet=True, el='bolt', cuff='iron', rich=0.5)),
    'Сапоги Рассвета': ('boot', dict(mat=tinted((.62, .56, .44), 'matte', leather=True), cuff='gold', trim='gold', wing=tinted((.9, .85, .68), 'matte', cloth=True), el='holy', sole='bronze')),
    'Сапоги Пространства': ('boot', dict(mat=tinted((.13, .085, .22), 'matte', leather=True), stars=(.8, .7, 1.0), orbit=(.65, .4, 1.0), el='shadow', sole='blackiron')),
    'Сапоги Тороса': ('boot', dict(plated=True, mat='iron', horns=True, crystals=(.5, .8, 1.0), el='ice', cuff='darksteel')),
    'Сапоги Гравитации': ('boot', dict(plated=True, mat='darksteel', core=(.7, .35, 1.0), runes=True, runecol=(.7, .4, 1.0), gem=(.6, .3, 1.0), el='shadow')),
    'Сапоги Пламени': ('boot', dict(mat=tinted((.42, .09, .06), 'matte', leather=True), flames=True, trim='bronze', el='fire', sole='blackleather', cuff='redleather', rich=0.5)),
    # helms, masks, crowns
    'Шлем': ('helm', dict(rich=0.0)),
    'Магический Шлем': ('helm', dict(el='arcane', wings=True, wmat=tinted((.62, .68, .8), 'matte', cloth=True), gem=(.3, .5, 1.0), runes=True, mat='silver', trim='silver')),
    'Шлем Рокового Лорда': ('helm', dict(mat='blackiron', trim='darksteel', visor='slit', horns=True, hmat='blackiron', eye=(1, .15, .1), spikes=True, el='fire', rich=0.9)),
    'Маска': ('mask', dict(kind='plain', rich=0.0, mat=tinted((.36, .3, .21), 'matte', spec=.12, shin=14, grain=True))),
    'Маска Смерти': ('mask', dict(kind='skull', eye=(.3, .9, .6), el='soul', mat=tinted((.58, .53, .4), 'matte', spec=.12, shin=16))),
    'Адская Маска': ('mask', dict(kind='demon', mat='blackiron', horns=True, eye=(1, .25, .08), el='fire', hmat='bone')),
    'Путь Войны': ('mask', dict(kind='tribal', mat='wood', feathers=tinted((.55, .12, .08), 'matte', cloth=True), el='blood')),
    'Корона Кенария': ('crown', dict(mat='gold', style='leaf', tines=5, tall=28, gems=[(.2, .8, .35), (.2, .8, .35), (.3, .9, .4)], el='holy')),
    'Корона Злобы': ('crown', dict(mat='blackiron', trim='darksteel', tines=7, tall=34, gems=[(.85, .08, .08), (.85, .08, .08)], thorns=True, el='blood', tipgem=(.8, .1, .1))),
    'Чародейская Лента': ('circlet', dict(el='arcane')),
    # body armour and robes
    'Мантия': ('robe', dict(rich=0.0)),
    'Гномья Броня': ('armor', dict(mat='iron', trim='bronze', emblem='gear', pmat='bronze', beltmat='leather')),
    'Мифриловый Доспех': ('armor', dict(mat='mithrilmail', trim='silver', emblem='gem', gem=(.4, .7, 1.0), el='ice')),
    'Доспехи Бога': ('armor', dict(mat='gold', trim='silver', emblem='gem', gem=(1.0, .95, .7), rays=True, el='holy', cloth=tinted((.8, .78, .7), 'matte', cloth=True))),
    'Доспех Красного Тумана': ('armor', dict(mat='blackiron', trim='darksteel', emblem='skull', mist=(.9, .1, .1), pspike=True, el='blood')),
    'Доспех Друида': ('armor', dict(mat=tinted((.2, .27, .13), 'matte', leather=True), pmat='wood', trim='wood', emblem='leaf', cloth=tinted((.14, .24, .12), 'matte', cloth=True), el='poison')),
    'Кираса Рыцаря': ('armor', dict(mat='steel', trim='gold', emblem='cross', emblem_col=(.12, .25, .65), cloth=tinted((.1, .16, .4), 'matte', cloth=True))),
    'Доспехи Заклинателя': ('armor', dict(mat='mithril', trim='silver', emblem='gem', gem=(.35, .5, 1.0), runes=True, runecol=(.5, .65, 1.0), cloth=tinted((.1, .12, .35), 'matte', cloth=True), el='arcane')),
    'Разрушитель Заклинаний': ('armor', dict(mat='darksteel', trim='iron', crack=(.7, .35, 1.0), pspike=True, el='shadow')),
    # shields
    'Рыцарский Щит': ('shield', dict(kind='heater', rim='steel', face=tinted((.09, .14, .36), 'matte', spec=.2), emblem='cross', emblem_col=(.72, .74, .8), boss='steel')),
    'Щит Выносливости': ('shield', dict(kind='heater', rim='bronze', face='wood', emblem='plates', boss='gem', gem=(.2, .8, .3), el='poison')),
    'Щит Феруса': ('shield', dict(kind='kite', rim='gold', face='blackiron', emblem='bars', emblem_col=(.62, .45, .12), boss='gem', gem=(.85, .1, .1), bossmat='gold', el='blood')),
    'Драконий Щит': ('shield', dict(kind='round', rim='darksteel', face=tinted((.28, .05, .04), 'matte', spec=.3, shin=20), emblem='scales', emblem_col=(.6, .12, .06), boss='dragon', el='fire')),
    'Эгида Смерти': ('shield', dict(kind='round', rim='blackiron', face='darksteel', emblem='plates', boss='skull', el='soul', glowring=(.3, .9, .6), runes=True)),
    # rings, amulets, necklaces
    'Кольцо Паука': ('ring', dict(mat='steel', setting='spider', rich=0.1)),
    'Кольцо Регенерации': ('ring', dict(mat='gold', gem=(.2, .85, .35), leaves=True, el='poison', rich=0.1)),
    'Кольцо Защиты': ('ring', dict(mat='silver', setting='shield', gem=(.2, .45, 1.0), el='arcane', rich=0.1)),
    'Ожерелье': ('pendant', dict(kind='beads', rich=0.0)),
    'Амулет Защиты': ('pendant', dict(kind='shield', mat='bronze', gem=(.25, .45, 1.0), rich=0.3)),
    'Амулет': ('pendant', dict(kind='disc', mat='bronze', gem=(.85, .15, .15), rich=0.1)),
    'Колье Лунного Света': ('pendant', dict(kind='moon', mat='silver', chain='silver', el='arcane', gem=(.75, .85, 1.0), bglow=((.6, .75, 1.0), 2.0, 0.3))),
    'Амулет Изменчивости': ('pendant', dict(kind='swirl', mat='gold', chain='gold', c1=(.4, .03, .08), c2=(.3, .55, 1.0), el='arcane')),
    'Амулет Немезиды': ('pendant', dict(kind='eye', mat='blackiron', chain='darksteel', el='blood')),
    'Амулет Вершителя': ('pendant', dict(kind='sun', mat='darksteel', chain='darksteel', gem=(.85, .1, .1), el='blood')),
    'Фибула Умбры': ('pendant', dict(kind='brooch', mat='silver', chain='silver', gem=(.5, .25, .85), el='shadow')),
    'Печать Силы': ('pendant', dict(kind='seal', mat='gold', chain='bronze', gem=(1, .35, .1), el='fire')),
    'Магическая Цепь': ('chain', dict(mat='silver', el='arcane', gem=(.3, .5, 1.0))),
    # orbs and stones
    'Сфера Огня': ('orbitem', dict(c1=(.25, .02, .01), c2=(1, .45, .08), embers=(1, .5, .1), el='fire', stand='bronze', rich=0.1)),
    'Сфера Маны': ('orbitem', dict(c1=(.02, .05, .3), c2=(.35, .55, 1.0), sparks=(.7, .85, 1.0), el='arcane', stand='silver', rich=0.1, oseed=2)),
    'Сфера Яда': ('orbitem', dict(c1=(.03, .15, .02), c2=(.55, .9, .2), bubbles=True, el='poison', stand='darksteel', rich=0.1, oseed=3)),
    'Сфера Льда': ('orbitem', dict(c1=(.03, .12, .25), c2=(.6, .9, 1.0), crystals=(.55, .85, 1.0), el='ice', stand='silver', oseed=4)),
    'Камень Здоровья': ('stone', dict(color=(.8, .08, .1), el='blood')),
    'Камень Маны': ('stone', dict(color=(.2, .42, .95), el='arcane')),
    'Камень Душ': ('soulstone', dict(el='soul')),
    # books, horns, banners, vessels and other objects
    'Том Заклинаний': ('tome', dict(cover=tinted((.07, .1, .26), 'matte', leather=True, spec=.15), trim='silver', emblem='gem', gem=(.3, .5, 1.0), runes=True, el='arcane')),
    'Тракт монахов': ('tome', dict(cover=tinted((.3, .2, .1), 'matte', leather=True), trim='bronze', emblem='beads', ribbon=tinted((.5, .06, .06), 'matte', cloth=True), rich=0.0, rot=-10)),
    'Рог Ветров': ('horn', dict(mat='bone', trim='bronze', el='arcane', rich=0.1)),
    'Знамя Победы': ('banner', dict(el='holy', gem=(.85, .15, .15))),
    'Тотем Астрального Змея': ('totem', dict(el='arcane')),
    'Урна Теней': ('urn', dict(el='shadow')),
    'Мешочек с золотом': ('pouch', dict(rich=0.0)),
    'Чаша с душами': ('chalice', dict(el='soul', rich=0.1)),
    'Сундук Судьбы': ('chest', dict(el='shadow', rich=0.9)),
    # potions, elixirs and consumable scrolls
    'Зелье Здоровья': ('flask', dict(shape='round', liquid=(.66, .04, .06), glow=0.35, rich=0.0)),
    'Зелье Маны': ('flask', dict(shape='round', liquid=(.1, .26, .82), glow=0.35, rich=0.0, oseed=3)),
    'Могущественный Эликсир': ('flask', dict(shape='big', liquid=(.95, .55, .08), glow=0.5, bubbles=True, cork='wood', lipmat='gold', rich=0.1)),
    'Эликсир невидимости': ('flask', dict(shape='tall', liquid=(.62, .72, .9), glow=0.25, ghost=True, sparkle=(.8, .9, 1.0), rich=0.1)),
    'Антимагический Эликсир': ('flask', dict(shape='hex', liquid=(.5, .2, .8), glow=0.4, lipmat='silver', cork='stone', rich=0.1, el='shadow')),
    'Эликсир бессмертия': ('flask', dict(shape='amph', liquid=(1.0, .88, .45), glow=0.7, symbol='cross', lipmat='gold', sparkle=(1, .95, .7), fill=0.8, rich=0.2, el='holy')),
    'Свиток Возрождения': ('scrollroll', dict(kind='ankh', sigil=(1.0, .85, .4), cord=tinted((.55, .4, .1), 'matte', cloth=True), el='holy')),
    'Свиток Тайного Знания': ('scrollroll', dict(kind='star', sigil=(.5, .7, 1.0), cord=tinted((.1, .2, .5), 'matte', cloth=True), el='arcane')),
    # figurines of summoned creatures
    'Дух - Целитель': ('spirit', dict(c1=(.04, .22, .12), c2=(.6, 1.0, .75), symbol='cross')),
    'Дух - Маны': ('spirit', dict(c1=(.03, .06, .3), c2=(.5, .7, 1.0), oseed=2)),
    'Тролль-лекарь': ('troll', dict(kind='healer', hair_acc=(.1, .28, .25), rich=0.1)),
    'Тролль-защитник': ('troll', dict(kind='guard', hair_acc=(.34, .07, .05), skin=(.22, .27, .2), rich=0.2)),
    'Морлок-охотник': ('murloc', dict(kind='hunter', rich=0.1)),
    'Морлок-заклинатель': ('murloc', dict(kind='caster', skin=(.08, .18, .3), fin=(.2, .1, .3), rich=0.1)),
    'Статуэтка демона': ('demonstat', dict(mat='darkbronze', el='fire', rich=0.1)),
}


# ------------------------------------------------------------------ driver

_ITEMS = None
FULL_DIR = None


def clean_name(n):
    n = re.sub(r'\s*\(\d+\)', '', n).replace('/', '')
    return re.sub(r'\s+', ' ', n).strip()


def split_level(n):
    m = re.match(r'^(.*?)\s+(I{1,3})$', n)
    return (m.group(1), len(m.group(2))) if m else (n, 0)


def load_items(path=ITEMS_JSON):
    with open(path, encoding='utf-8') as f:
        items = json.load(f)
    by_id = {i['id']: i for i in items}
    twin = {}
    for i in sorted(items, key=lambda x: x['id']):
        if i['role'] == 'final':
            key = clean_name(i['name'])
            twin.setdefault(key, []).append(i['id'])
    for i in items:
        i['_clean'] = clean_name(i['name'])
        i['_variant'] = 0
        if i['role'] == 'final':
            lst = twin[i['_clean']]
            i['_variant'] = lst.index(i['id']) if len(lst) > 1 else 0
    return items, by_id


def seed_of(item_id):
    return zlib.crc32(item_id.encode('utf-8')) & 0x7FFFFFFF


def rich_of(price, override=None):
    if override is not None:
        return override
    p = price or 0
    return float(np.clip((p - 150) / 2400.0, 0.0, 1.0))


def build_ctx(design, level, variant, price, seed, bare=False, by_id=None):
    arch, opts = DESIGNS[design]
    c = Ctx(seed, bare)
    c.scr = make_scratches(np.random.default_rng(seed + 1))
    o = dict(opts)
    o['rich'] = rich_of(price, o.get('rich'))
    o['variant'] = variant
    o['level'] = level
    o['by_id'] = by_id
    ARCHS[arch](c, o)
    lv = 1 if level else 0
    c.fit(target=o.get('fit', 0.84 - 0.05 * lv), bias=o.get('bias', (-0.004 + 0.035 * lv, -0.008 + 0.035 * lv)), maxscale=o.get('maxscale', 1.6))
    c.tint = o.get('bg', ELEM[o.get('el') or 'none'][3])
    el = o.get('el')
    if el:
        c.aura = ELEM[el][0]
        c.aura_k = 0.2 + 0.8 * o['rich']
    else:
        c.aura, c.aura_k = None, 0.0
    return c


def draw_ticks(c, n):
    with c.reset():
        w = 2.5 + 4.6 * n + 1.6
        c.layer(S(rrect_poly(3.5, 3.5, 3.5 + w + 2.5, 22.5, 2.2)), 'blackiron', relief='bevel', r=1.2, k=0.6, sh=0.5, op=0.78, ao=0.0, wear=0.1)
        for i in range(n):
            x = 8.2 + i * 4.6
            c.layer(S(rrect_poly(x - 1.35, 7.2, x + 1.35, 19.0, 1.0)), 'bronze', relief='round', k=0.95, sh=0.4, ao=0.0, wear=0.2)


def render_item(item, by_id):
    """-> PIL RGBA 128x128 for one catalogue entry."""
    seed = seed_of(item['id'])
    if item['role'] == 'recipe-scroll':
        tgt = by_id[item['scrollFor']]
        design, level = split_level(tgt['_clean'])
        c = build_scroll(design, level, tgt, seed, by_id)
    else:
        design, level = split_level(item['_clean'])
        c = build_ctx(design, level, item['_variant'], item['price'], seed, by_id=by_id)
        if level:
            draw_ticks(c, level)
    im = c.finish(c.tint, c.aura, getattr(c, 'aura_k', 0.0), seed)
    if FULL_DIR:
        c.full.save(os.path.join(FULL_DIR, item['id'] + '_512.png'))
    return im


def build_scroll(design, level, tgt, seed, by_id):
    """Recipe scroll: unrolled parchment between two rolls, a red wax seal and a miniature of the target item."""
    c = Ctx(seed)
    rng = np.random.default_rng(seed)
    paper_edge = ((0.16, 0.085, 0.025), 1.0, 0.62)
    roll_edge = ((0.14, 0.075, 0.025), 1.3, 0.55)
    # sheet with slightly wavy edges
    ys = np.linspace(11, 89, 22)
    left = [(19.5 + 0.9 * math.sin(y * 0.34), y) for y in ys]
    right = [(80.5 + 0.9 * math.sin(y * 0.29 + 1.0), y) for y in ys[::-1]]
    sheet = S(_arr(left + right))
    c.layer(sheet, 'parchment', relief='bevel', r=3.0, k=0.35, sh=1.0, ao=0.0, edge=paper_edge, ink=0.4)
    # stains and burnt corners
    for k in range(6):
        c.decal(S(blob_poly(rng.uniform(26, 74), rng.uniform(22, 78), rng.uniform(5, 12), rng, n=18, jitter=0.25)), (0.42, 0.26, 0.11), 0.20, 'mul', blur=2.4, clip=sheet)
    for (x, y) in ((20, 14), (80, 14), (20, 86), (80, 86)):
        c.decal(S(circle_poly(x, y, 10, 16)), (0.2, 0.1, 0.03), 0.42, 'mul', blur=3.0, clip=sheet)
    # a soft darker patch behind the miniature keeps the picture readable
    c.decal(S(ellipse_poly(50, 50, 27, 27, 0, 40)), (0.45, 0.32, 0.18), 0.30, 'mul', blur=6.0, clip=sheet)
    # miniature of the item, painted onto the sheet
    mini = build_ctx(design, level, tgt['_variant'], tgt['price'], seed_of(tgt['id']), bare=True, by_id=by_id)
    c.paste(mini, 0.68, 50, 49, mix_sepia=0.16, outline=0.9, bright=1.0, glow_k=0.3)
    # roll shadows on the sheet
    c.decal(S(rect_poly(18, 14, 82, 24)), (0.16, 0.08, 0.02), 0.45, 'mul', blur=2.6, clip=sheet)
    c.decal(S(rect_poly(18, 76, 82, 86)), (0.16, 0.08, 0.02), 0.45, 'mul', blur=2.6, clip=sheet)
    # the two rolls
    for (y0, y1) in ((3.5, 16.0), (84.0, 96.5)):
        yc = (y0 + y1) / 2
        c.layer(S(rrect_poly(12.5, y0, 87.5, y1, 6.0)), 'parchment', relief='round', k=0.95, sh=1.0, ao=0.1, edge=roll_edge, ink=0.5)
        for sx in (-1, 1):
            ex = 12.5 if sx < 0 else 87.5
            c.layer(S(ellipse_poly(ex, yc, 3.4, (y1 - y0) / 2)), 'parchment', relief='round', k=0.9, sh=0.2, ao=0.0, ink=0.7, edge=((0.17, 0.09, 0.03), 0.5, 0.7))
            for q in range(3):
                c.decal(S(stroke(ellipse_poly(ex, yc, 2.5 - q * 0.7, (y1 - y0) / 2 - 1.4 - q * 1.8, 0, 24), 0.26)), (0.22, 0.12, 0.05), 0.6, 'over')
    # ribbon tails and the red wax seal (lower right)
    ribbon = tinted((0.26, 0.035, 0.045), 'matte', cloth=True)
    sx0, sy0 = 71.0, 83.0
    for sd, (ex_, ey_) in ((-1, (59, 99)), (1, (74, 100))):
        pts = bez([(sx0, sy0 + 2), ((sx0 + ex_) / 2 + sd * 2, sy0 + 10), (ex_, ey_)], 14)
        c.layer(S(stroke(pts, np.linspace(2.7, 3.2, 14), caps='flat')), ribbon, relief='round', k=0.7, sh=0.5, ao=0.0, ink=0.55)
    seal = blob_poly(sx0, sy0, 9.2, rng, n=30, jitter=0.055)
    c.layer(S(seal), 'wax', relief='round', k=0.95, sh=1.0, ao=0.1, ink=0.5)
    for (dx, dy, r_) in ((-5.5, 8.2, 1.6), (4.0, 8.8, 1.9)):
        c.layer(S(circle_poly(sx0 + dx, sy0 + dy, r_, 12)), 'wax', relief='round', k=0.9, sh=0.2, ao=0.0, ink=0.4)
    c.decal(S(stroke(ellipse_poly(sx0, sy0, 6.0, 6.0, 0, 28), 0.5)), (0.1, 0.0, 0.0), 0.85, 'engrave')
    c.decal(S(star_poly(sx0, sy0, 4.0, 1.6, 5, -90)), (0.32, 0.02, 0.02), 0.95, 'emboss')
    c.decal(S(ellipse_poly(sx0 - 3.2, sy0 - 4.0, 2.4, 1.3, -30, 12)), (1.0, 0.8, 0.7), 0.5, 'add', blur=0.6)
    c.fit(target=0.9, bias=(0.0, 0.0), maxscale=1.0)
    c.tint = 'warm'
    c.aura, c.aura_k = None, 0.0
    return c


def _render_worker(args):
    item_id, = args
    items, by_id = _worker_items()
    im = render_item(by_id[item_id], by_id)
    return item_id, im.tobytes()


def _worker_items():
    global _ITEMS
    if _ITEMS is None:
        _ITEMS = load_items()
    return _ITEMS


def contact_sheet(images, path, cols=12, cell=112, label=True, gap=6, per=None):
    from PIL import ImageFont
    font = None
    if label:
        try:
            font = ImageFont.load_default()
        except Exception:
            font = None
    rows = (len(images) + cols - 1) // cols
    lh = 12 if label else 0
    sheet = Image.new('RGB', (cols * (cell + gap) + gap, rows * (cell + lh + gap) + gap), (46, 44, 42))
    d = ImageDraw.Draw(sheet)
    for i, (iid, im) in enumerate(images):
        x = gap + (i % cols) * (cell + gap)
        y = gap + (i // cols) * (cell + lh + gap)
        sheet.paste(im.convert('RGB').resize((cell, cell), Image.LANCZOS), (x, y))
        if label:
            d.text((x + 1, y + cell + 1), iid, fill=(200, 196, 186), font=font)
    sheet.save(path)


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--only', help='comma separated ids')
    ap.add_argument('--out', default=OUT_DIR)
    ap.add_argument('--sheets', help='directory for contact sheets (icons-sheet-N.png and icons-sheet-64.png)')
    ap.add_argument('--workers', type=int, default=max(1, min(os.cpu_count() or 1, 10)))
    ap.add_argument('--per-sheet', type=int, default=60)
    ap.add_argument('--cell', type=int, default=112)
    ap.add_argument('--cols', type=int, default=12)
    ap.add_argument('--full', help='also dump 512px debug renders to this dir (single process)')
    a = ap.parse_args(argv)
    items, by_id = load_items()
    ids = [i['id'] for i in items]
    if a.only:
        want = [x.strip() for x in a.only.split(',') if x.strip()]
        bad = [x for x in want if x not in by_id]
        if bad:
            sys.exit('unknown ids: ' + ','.join(bad))
        ids = want
    os.makedirs(a.out, exist_ok=True)
    results = {}
    if a.full:
        global FULL_DIR
        FULL_DIR = a.full
        os.makedirs(a.full, exist_ok=True)
        a.workers = 1
    if a.workers > 1 and len(ids) > 3:
        from concurrent.futures import ProcessPoolExecutor
        with ProcessPoolExecutor(max_workers=a.workers) as ex:
            for iid, raw in ex.map(_render_worker, [(i,) for i in ids], chunksize=4):
                results[iid] = Image.frombytes('RGBA', (OUT, OUT), raw)
    else:
        for iid in ids:
            results[iid] = render_item(by_id[iid], by_id)
    for iid in ids:
        results[iid].save(os.path.join(a.out, iid + '.png'))
    print('wrote %d icons to %s' % (len(ids), a.out))
    if a.sheets:
        os.makedirs(a.sheets, exist_ok=True)
        imgs = [(i, results[i]) for i in ids]
        for n, s in enumerate(range(0, len(imgs), a.per_sheet), 1):
            contact_sheet(imgs[s:s + a.per_sheet], os.path.join(a.sheets, 'icons-sheet-%d.png' % n), cols=a.cols, cell=a.cell)
        contact_sheet([(i, im) for i, im in imgs], os.path.join(a.sheets, 'icons-sheet-64.png'), cols=12, cell=64, label=False, gap=4)
        print('sheets written to', a.sheets)


if __name__ == '__main__':
    main()

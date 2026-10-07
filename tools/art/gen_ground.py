"""Arena dirt texture without the evenly spaced large stones that read as polka dots on a big terrain.

Reuses the numpy toolkit from the menu mockup (tg.py). Writes ground_a/ground_n/ground_r.jpg
into unity/Assets/Game/Art/Textures. Run: py -3 -X utf8 tools/art/gen_ground.py
"""
import os
import sys

import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
sys.path.insert(0, os.path.join(ROOT, 'art', 'ui', 'menu-mockup', 'src'))
import tg  # noqa: E402

tg.OUT = os.path.join(ROOT, 'unity', 'Assets', 'Game', 'Art', 'Textures')
os.makedirs(tg.OUT, exist_ok=True)


def ground(n=1024, seed=7):
    fine = tg.fnoise(n, 1.0, seed + 2, hi=.5)
    mid = tg.fnoise(n, 1.8, seed, hi=.2)
    low = tg.fnoise(n, 2.6, seed + 1, hi=.03)
    broad = tg.fnoise(n, 2.4, seed + 5, hi=.02)
    dry = np.array([.2, .165, .125])
    dark = np.array([.115, .095, .074])
    col = tg.mix(dark, dry, mid * .45 + low * .35 + broad * .2) * (.8 + .4 * fine[..., None])
    h = mid * .22 + fine * .16 + low * .1
    # trampled damp patches, soft edged
    mud = tg.sstep(.56, .8, tg.fnoise(n, 2.2, seed + 3, hi=.05)) * .8
    mud_col = np.array([.06, .05, .042]) * (.85 + .3 * tg.fnoise(n, 1.6, seed + 13, hi=.15)[..., None])
    col = tg.mix(col, mud_col, mud)
    h = h * (1 - .6 * mud)
    # fine and medium gravel only, earthy tones
    cover_mask = np.zeros((n, n))
    for (k, rmin, rmax, cov, sd) in [(6500, .25, .6, .45, 41), (1800, .3, .65, .22, 42)]:
        dome, tone, warm = tg.gravel_layer(n, k, seed + sd, rmin, rmax, cov)
        sc = np.stack([tone * 1.0, tone * (.94 - .05 * warm), tone * (.86 - .1 * warm)], -1) * (.8 + .4 * fine[..., None])
        a = tg.sstep(0, .18, dome)
        shade = (.6 + .55 * dome)[..., None]
        col = tg.mix(col, sc * shade, a[..., None] * (1 - .55 * mud[..., None]))
        halo = np.clip(tg.blur(a, 2.2) - a, 0, 1)
        col *= (1 - .35 * halo)[..., None]
        h += dome * .5
        cover_mask = np.maximum(cover_mask, a)
    # dry cracks
    c1, c2, _, _ = tg.voronoi(n, 60, seed + 8)
    crack = (1 - tg.sstep(0, 2.4, c2 - c1)) * tg.sstep(.45, .62, tg.fnoise(n, 2.4, seed + 9, hi=.015)) * (1 - mud)
    col *= (1 - .6 * crack)[..., None]
    h -= crack * .5
    # dead grass flecks
    fl = tg.sstep(.78, .9, tg.fnoise(n, .8, seed + 21, ay=2.5, hi=.5)) * (1 - mud) * (1 - cover_mask)
    col = tg.mix(col, np.array([.27, .23, .15]) * (.8 + .4 * fine[..., None]), fl * .5)
    rough = np.clip(.94 - .06 * fine, 0, 1)
    rough = tg.mix(rough[..., None], (.2 + .22 * tg.fnoise(n, 2.0, seed + 14, hi=.2))[..., None], mud[..., None])[..., 0]
    rough = np.where(cover_mask > .3, .66, rough)
    return col, tg.normal_map(h, 4.5), rough


if __name__ == '__main__':
    albedo, normal, rough = ground()
    tg.save_jpg(albedo, 'ground_a')
    tg.save_jpg(normal, 'ground_n')
    tg.save_jpg(rough, 'ground_r')
    print('ground textures written to', tg.OUT)

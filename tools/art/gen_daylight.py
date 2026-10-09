"""Original tileable daylight PBR textures for the arena.

Run: py -3 -X utf8 tools/art/gen_daylight.py --validate
Albedo values are authored as sRGB bytes; normals and roughness are linear data.
Existing texture metadata is preserved. Local previews and checks are not assets.
"""
import argparse
import hashlib
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage as ndi

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'art/ui/menu-mockup/src'))
import tg  # noqa: E402

OUTPUT = ROOT / 'unity/Assets/Game/Art/Textures'
PREVIEW = ROOT / '.local/art/daylight'


def surface_noise(n, seed):
    return (tg.fnoise(n, 2.7, seed, hi=.025),
            tg.fnoise(n, 2.2, seed + 1, hi=.08),
            tg.fnoise(n, 1.5, seed + 2, hi=.24))


def masonry_cells(n, rows, seed, paving):
    """Unequal staggered slabs, with periodic warping and softly worn joints."""
    rng = np.random.default_rng(seed)
    heights = rng.uniform(.74, .99, rows) if paving else rng.uniform(.82, 1.22, rows)
    bounds = np.rint(np.r_[0, np.cumsum(heights / heights.sum() * n)]).astype(int)
    bounds[-1] = n
    ids = np.zeros((n, n), dtype=np.int32)
    xx = np.arange(n)
    count = 0
    for row in range(rows):
        columns = int(rng.integers(7, 9) if paving else rng.integers(3, 6))
        widths = rng.uniform(.85, 1.2, columns) if paving else rng.uniform(.6, 1.45, columns)
        ends = np.cumsum(widths / widths.sum() * n)
        offset = int(rng.uniform(0, n))
        column_ids = np.searchsorted(ends[:-1], (xx + offset) % n)
        ids[bounds[row]:bounds[row + 1]] = column_ids[None, :] + count
        count += columns
    # Keep the texture boundary through slab interiors instead of an entire mortar row.
    widest_row = int(np.argmax(np.diff(bounds)))
    row_center = int((bounds[widest_row] + bounds[widest_row + 1]) / 2)
    ids = np.roll(ids, -row_center, axis=0)
    yy, xx = np.mgrid[:n, :n]
    warp = n * (.024 if paving else .004)
    wx = (tg.fnoise(n, 2.5, seed + 21, hi=.015) - .5) * warp
    wy = (tg.fnoise(n, 2.5, seed + 22, hi=.018) - .5) * (n * .014 if paving else warp)
    ids = ndi.map_coordinates(ids, [(yy + wy) % n, (xx + wx) % n],
                              order=0, mode='grid-wrap')
    edge = np.zeros((n, n), dtype=bool)
    for axis in (0, 1):
        for shift in (-1, 1):
            edge |= ids != np.roll(ids, shift, axis)
    distance = ndi.distance_transform_edt(np.tile(~edge, (3, 3)))[n:2 * n, n:2 * n]
    return ids, count, distance


def flagstones(n, seed, rows):
    """Small four-sided flagstones authored for an eight metre terrain repeat."""
    rng = np.random.default_rng(seed + 80)
    ids, count, distance = masonry_cells(n, rows, seed, True)
    low, mid, fine = surface_noise(n, seed + 30)
    scale = n / 1024
    chips = tg.sstep(.67, .93, tg.fnoise(n, 1.8, seed + 43, hi=.12)) * 3.1 * scale
    worn_distance = np.maximum(0, distance - chips)
    gap = 1 - tg.sstep(.8 * scale, 2.7 * scale, worn_distance)
    bevel = tg.sstep(1.7 * scale, 7 * scale, worn_distance)
    hue = rng.uniform(-1, 1, count)[:, None] * np.array([.027, .008, -.034])
    tones = (np.array([.447, .484, .548]) + hue) * rng.uniform(.92, 1.08, (count, 1))
    color = tones[ids] * (.96 + .08 * low + .025 * (mid - .5))[..., None]
    color *= (.84 + .16 * bevel)[..., None]
    height = bevel * .50 + (low - .5) * .075 + (mid - .5) * .04
    height += rng.uniform(-.045, .045, count)[ids] * (1 - gap)
    # Broad wear and damp fields break up rows without adding noisy photographic grain.
    yy, xx = np.mgrid[:n, :n] / n
    macro = tg.fnoise(n, 3.0, seed + 44, hi=.009)
    lane = tg.sstep(.56, .94, np.sin((xx + yy + macro * .12) * np.pi * 2) * .5 + .5)
    wear = lane * tg.sstep(.25, .82, low) * (1 - gap)
    color = tg.mix(color, np.array([.58, .605, .64]), wear * .14)
    damp_field = tg.fnoise(n, 3.0, seed + 45, ay=3, hi=.025)
    damp = tg.sstep(.66, .91, damp_field) * (1 - gap)
    color *= (1 - damp * .06)[..., None]
    color = tg.mix(color, np.array([.195, .221, .226]) * (.9 + .16 * low)[..., None], gap)
    moss_field = tg.fnoise(n, 2.8, seed + 40, hi=.024)
    moss = tg.sstep(.69, .87, moss_field) * (1 - tg.sstep(2.5 * scale, 7 * scale, distance))
    moss *= .68 + .32 * fine
    color = tg.mix(color, np.array([.235, .305, .145]) * (.89 + .2 * mid)[..., None], moss * .8)
    height += moss * .035
    crack_image = Image.new('L', (n, n))
    draw = ImageDraw.Draw(crack_image)
    for _ in range(12):
        start = rng.uniform(0, n, 2)
        direction = rng.uniform(-1, 1, 2)
        points = [start]
        for _ in range(4):
            points.append(points[-1] + (direction * rng.uniform(6, 11) + rng.uniform(-3, 3, 2)) * scale)
        for dx in (-n, 0, n):
            for dy in (-n, 0, n):
                draw.line([tuple(p + [dx, dy]) for p in points], fill=255, width=max(1, int(scale)))
    cracks = ndi.gaussian_filter(np.asarray(crack_image) / 255, .5 * scale, mode='wrap')
    cracks *= 1 - gap
    color *= (1 - cracks * .48)[..., None]
    height -= cracks * .11
    rough = .79 + rng.uniform(-.035, .035, count)[ids] + low * .055
    rough += gap * .07 + moss * .055 - damp * .08 - wear * .035
    return np.clip(color, 0, 1), tg.normal_map(height, 4.2 / scale), np.clip(rough, 0, 1)


def limestone(n=1024, seed=71, paving=True, rows=6):
    if paving:
        return flagstones(n, seed, rows)
    rng = np.random.default_rng(seed)
    ids, count, distance = masonry_cells(n, rows, seed, paving)
    low, mid, fine = surface_noise(n, seed + 30)
    scale = n / 1024
    gap = 1 - tg.sstep(.5 * scale, 3.5 * scale, distance)
    bevel = tg.sstep(2.0 * scale, 10 * scale, distance)
    height = bevel * .72 + (low - .5) * .10 + (mid - .5) * .025
    height += rng.uniform(-.035, .035, count)[ids] * (1 - gap)
    palette = np.array([[.735, .705, .625], [.755, .715, .625], [.70, .67, .605],
                        [.78, .735, .645], [.735, .72, .66]])
    joint_color = np.array([.475, .46, .405])
    moss_color = np.array([.36, .425, .255])
    tones = palette[rng.integers(0, len(palette), count)]
    tones *= rng.uniform(.965, 1.035, (count, 1))
    color = tones[ids] * (.945 + .105 * low + .018 * (fine - .5))[..., None]
    # A little crevice darkening is independent of the scene's directional light.
    color *= (.885 + .115 * bevel)[..., None]
    wear = (1 - bevel) * (1 - gap) * tg.sstep(.3, .75, mid)
    color = tg.mix(color, np.array([.825, .785, .70]), wear * .16)
    color = tg.mix(color, joint_color * (.93 + .14 * low)[..., None], gap)
    moss_field = tg.fnoise(n, 2.9, seed + 40, hi=.022)
    moss = tg.sstep(.66, .84, moss_field)
    moss *= (1 - tg.sstep(3 * scale, 14 * scale, distance))
    color = tg.mix(color, moss_color * (.92 + .15 * mid)[..., None], moss * .75)
    height += moss * .035
    rough = .79 + .075 * low + .035 * gap
    rough += moss * .055
    return color, tg.normal_map(height, 4.2 / scale), np.clip(rough, 0, 1)


def ground(n=1024):
    return limestone(n, 71, True, 10)


def cobble(n=1024):
    return limestone(n, 113, True, 10)


def stone(n=1024):
    seed = 137
    rng = np.random.default_rng(seed)
    low, mid, fine = surface_noise(n, seed)
    weather = tg.fnoise(n, 3.0, seed + 3, ay=2.7, hi=.014)
    color = tg.mix(np.array([.665, .625, .535]), np.array([.795, .75, .655]), low)
    color *= (.975 + .05 * mid + .009 * (fine - .5))[..., None]
    height = (low - .5) * .14 + (mid - .5) * .045
    yy, xx = np.mgrid[:n, :n] / n
    pits = np.zeros((n, n))
    for _ in range(95):
        cx, cy = rng.random(2)
        rx, ry = rng.uniform(.0025, .012, 2)
        dx = (xx - cx + .5) % 1 - .5
        dy = (yy - cy + .5) % 1 - .5
        radius = np.sqrt((dx / rx) ** 2 + (dy / ry) ** 2)
        pits = np.maximum(pits, (1 - tg.sstep(.2, 1, radius)) * rng.uniform(.25, 1))
    height -= pits * .16
    color *= (1 - pits * .13)[..., None]
    damp = tg.sstep(.63, .89, weather)
    color = tg.mix(color, np.array([.52, .495, .415]), damp * .24)
    moss = tg.sstep(.78, .96, tg.fnoise(n, 3.0, seed + 4, hi=.026))
    moss *= tg.sstep(.48, .81, weather)
    color = tg.mix(color, np.array([.36, .405, .235]), moss * .32)
    height += moss * .016
    scale = n / 1024
    crack_image = Image.new('L', (n, n))
    draw = ImageDraw.Draw(crack_image)
    for _ in range(9):
        start = rng.uniform(0, n, 2)
        direction = rng.uniform(-1, 1, 2)
        points = [start]
        for _ in range(5):
            points.append(points[-1] + (direction * rng.uniform(8, 17) + rng.uniform(-4, 4, 2)) * scale)
        for dx in (-n, 0, n):
            for dy in (-n, 0, n):
                draw.line([tuple(p + [dx, dy]) for p in points], fill=255, width=max(1, int(scale)))
    cracks = ndi.gaussian_filter(np.asarray(crack_image) / 255, .65 * scale, mode='wrap')
    color *= (1 - cracks * .26)[..., None]
    height -= cracks * .065
    rough = .82 + low * .07 + pits * .045 - damp * .045 + moss * .035
    return np.clip(color, 0, 1), tg.normal_map(height, 7 / scale), np.clip(rough, 0, 1)


def foliage(n=1024, seed=163):
    low, mid, fine = surface_noise(n, seed)
    color = tg.mix(np.array([.195, .29, .15]), np.array([.365, .44, .205]),
                   low * .75 + mid * .25)
    color *= (.98 + .025 * (fine - .5))[..., None]
    height = (mid - .5) * .055 + (low - .5) * .025
    rough = .84 + .06 * low
    return color, tg.normal_map(height, 2.5), rough


def cloth(n=1024, seed=179, blue=False):
    low, mid, fine = surface_noise(n, seed)
    yy, xx = np.mgrid[:n, :n] / n
    warp = tg.fnoise(n, 2.7, seed + 4, hi=.014)
    folds = np.sin((xx * 6 + warp * .22) * np.pi * 2)
    weave = np.sin(xx * np.pi * 2 * 256) * np.sin(yy * np.pi * 2 * 256)
    if blue:
        dark, light = np.array([.15, .295, .465]), np.array([.24, .435, .585])
    else:
        dark, light = np.array([.66, .285, .10]), np.array([.835, .445, .17])
    color = tg.mix(dark, light, low * .68 + .32 * mid)
    color *= (.965 + .04 * folds + .018 * (fine - .5))[..., None]
    height = folds * .055 + weave * .003
    rough = .9 + .04 * low
    return color, tg.normal_map(height, 3), rough


def cloth_blue(n=1024):
    return cloth(n, 193, True)


def timber(n=1024, seed=211, planks=8):
    rng = np.random.default_rng(seed)
    yy, xx = np.mgrid[:n, :n] / n
    low = tg.fnoise(n, 2.8, seed, hi=.018)
    streak = tg.fnoise(n, 2.4, seed + 1, ay=12, hi=.08)
    warp = tg.fnoise(n, 2.5, seed + 2, ay=4, hi=.012)
    phase = xx * planks + .37 + (warp - .5) * .025
    plank_id = np.floor(phase).astype(int) % planks
    local_x = phase % 1
    distance = np.minimum(local_x, 1 - local_x)
    seam = 1 - tg.sstep(.012, .035, distance)
    grain = np.sin(np.pi * 2 * (xx * 83 + (warp - .5) * .38)) * .5 + .5
    tone = rng.uniform(.94, 1.055, planks)[plank_id]
    color = tg.mix(np.array([.37, .235, .135]), np.array([.61, .435, .265]),
                   low * .35 + streak * .65)
    color *= (tone * (.97 + .035 * grain))[..., None]
    # Local knot rings bend the grain, rather than adding photographic grit.
    height = (grain - .5) * .022 + (streak - .5) * .06 - seam * .23
    for _ in range(5):
        cx, cy = rng.random(2)
        dx = (xx - cx + .5) % 1 - .5
        dy = (yy - cy + .5) % 1 - .5
        radius = np.sqrt((dx / .028) ** 2 + (dy / .075) ** 2)
        knot = (1 - tg.sstep(.8, 1.8, radius)) * .14
        rings = np.sin(radius * 13) * (1 - tg.sstep(1, 2.5, radius))
        color *= (1 - knot + rings * .016)[..., None]
        height += rings * .012
    color = tg.mix(color, np.array([.26, .19, .125]), seam * .78)
    rough = .76 + .07 * low + seam * .075
    return color, tg.normal_map(height, 3.5), np.clip(rough, 0, 1)


def barrel(n=1024):
    return timber(n, 211, 9)


def crate(n=1024):
    return timber(n, 227, 6)


def rust(n=1024, seed=241):
    low, mid, fine = surface_noise(n, seed)
    color = tg.mix(np.array([.325, .335, .345]), np.array([.455, .47, .48]), low)
    patina = tg.sstep(.68, .9, tg.fnoise(n, 2.9, seed + 6, hi=.023)) * .2
    color = tg.mix(color, np.array([.42, .30, .19]), patina)
    color *= (.99 + .02 * (fine - .5))[..., None]
    height = (mid - .5) * .035 + patina * .022
    rough = .52 + .09 * low + patina * .3
    return color, tg.normal_map(height, 2), np.clip(rough, 0, 1)


GENERATORS = {name: globals()[name] for name in (
    'ground', 'cobble', 'stone', 'foliage', 'cloth', 'cloth_blue', 'barrel', 'crate', 'rust')}


def quantize(array):
    return (np.clip(array, 0, 1) * 255 + .5).astype(np.uint8)


def seam_metrics(array):
    """Compare the wrap step with internal neighboring pixel changes."""
    a = array.astype(float) / 255
    result = {}
    for label, axis in (('x', 1), ('y', 0)):
        seam = np.take(a, 0, axis) - np.take(a, -1, axis)
        internal = np.diff(a, axis=axis)
        baseline = float(np.mean(np.abs(internal)))
        delta = float(np.mean(np.abs(seam)))
        result[label] = {'mean_abs_delta': delta,
                         'internal_mean_abs_delta': baseline,
                         'seam_to_internal_ratio': delta / max(baseline, 1 / 255)}
    return result


def write_previews(items):
    size = 256
    sheet = Image.new('RGB', (size * 3, (size + 24) * len(items)), (28, 33, 40))
    draw = ImageDraw.Draw(sheet)
    for row, (name, arrays) in enumerate(items.items()):
        for column, (suffix, array) in enumerate(zip(('albedo', 'normal', 'roughness'), arrays)):
            image = Image.fromarray(quantize(array)).convert('RGB')
            image = image.resize((size, size), Image.Resampling.LANCZOS)
            x, y = column * size, row * (size + 24)
            sheet.paste(image, (x, y + 24))
            draw.text((x + 8, y + 5), name + ' / ' + suffix, fill=(228, 228, 226))
    sheet.save(PREVIEW / 'pbr-contact-sheet.jpg', quality=92)
    tile_sheet = Image.new('RGB', (768, 768), (28, 33, 40))
    for index, name in enumerate(('ground', 'cobble', 'stone', 'foliage', 'cloth', 'cloth_blue', 'barrel', 'crate', 'rust')):
        a = quantize(items[name][0])
        image = Image.fromarray(np.tile(a, (2, 2, 1))).resize((256, 256), Image.Resampling.LANCZOS)
        tile_sheet.paste(image, ((index % 3) * 256, (index // 3) * 256))
    tile_sheet.save(PREVIEW / 'tiling-2x2.jpg', quality=94)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--validate', action='store_true', help='Regenerate arrays twice and check quantized equality.')
    parser.add_argument('--sets', nargs='+', choices=GENERATORS, default=list(GENERATORS),
                        help='Regenerate only these material sets.')
    args = parser.parse_args()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    PREVIEW.mkdir(parents=True, exist_ok=True)
    tg.OUT = str(OUTPUT)
    items = {}
    report_path = PREVIEW / 'validation.json'
    report = json.loads(report_path.read_text(encoding='utf-8')) if report_path.exists() else {'textures': {}}
    report.update({'size': 1024, 'albedo_space': 'sRGB', 'data_space': 'linear', 'regenerated_sets': args.sets})
    for name, generate in GENERATORS.items():
        if name not in args.sets:
            items[name] = tuple(np.asarray(Image.open(OUTPUT / (name + '_' + suffix + '.jpg'))) / 255
                                for suffix in ('a', 'n', 'r'))
            continue
        arrays = generate()
        items[name] = arrays
        repeated = generate() if args.validate else None
        for index, (suffix, array) in enumerate(zip(('a', 'n', 'r'), arrays)):
            expected = (1024, 1024) if suffix == 'r' else (1024, 1024, 3)
            assert array.shape == expected and np.isfinite(array).all(), name + '_' + suffix
            assert 0 <= array.min() <= array.max() <= 1, name + '_' + suffix
            if repeated is not None:
                assert np.array_equal(quantize(array), quantize(repeated[index])), name + ' nondeterministic'
            path = Path(tg.save_jpg(array, name + '_' + suffix, 98 if suffix == 'n' else 90))
            decoded = np.asarray(Image.open(path))
            normal_error = None
            normal_angle_p99 = None
            if suffix == 'n':
                vectors = decoded.astype(float) / 255 * 2 - 1
                lengths = np.linalg.norm(vectors, axis=-1)
                normal_error = float(np.max(np.abs(lengths - 1)))
                source_vectors = array * 2 - 1
                assert np.max(np.abs(np.linalg.norm(source_vectors, axis=-1) - 1)) < 1e-6
                cosine = np.sum(source_vectors * vectors / lengths[..., None], axis=-1)
                angles = np.degrees(np.arccos(np.clip(cosine, -1, 1)))
                normal_angle_p99 = float(np.percentile(angles, 99))
                # Unity unpacks normal directions, so validate angular codec error.
                assert normal_angle_p99 < 2, name + ' normal compression'
            metrics = seam_metrics(decoded)
            # JPEG rounding and small knots may differ by a fraction of a byte.
            assert all(value['seam_to_internal_ratio'] < 2.5 for value in metrics.values()), name + ' wrap seam'
            report['textures'][path.name] = {
                'bytes': path.stat().st_size, 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                'deterministic': repeated is not None, 'seams': metrics,
                'normal_length_max_error': normal_error,
                'normal_angle_p99_degrees': normal_angle_p99,
                'mean_rgb_or_roughness': np.mean(decoded, axis=(0, 1)).tolist()}
        sizes = [report['textures'][name + '_' + suffix + '.jpg']['bytes'] for suffix in ('a', 'n', 'r')]
        print(name, '1024x1024', 'bytes', sizes, 'deterministic' if repeated is not None else '')
    write_previews(items)
    report['total_bytes'] = sum(item['bytes'] for item in report['textures'].values())
    report['max_seam_to_internal_ratio'] = max(
        axis['seam_to_internal_ratio'] for item in report['textures'].values() for axis in item['seams'].values())
    report_path.write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    print('TOTAL', report['total_bytes'], 'bytes; max seam ratio', round(report['max_seam_to_internal_ratio'], 3))
    print('Previews and validation:', PREVIEW)


if __name__ == '__main__':
    main()

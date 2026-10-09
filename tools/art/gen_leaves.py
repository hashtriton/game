"""Original alpha-cut leaf sprig and matching normal/roughness textures.

Run: py -3 -X utf8 tools/art/gen_leaves.py --validate
Full-UV cards use one sprig; transparent padded borders protect cutout mipmaps.
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


def generate(n=512, seed=307):
    rng = np.random.default_rng(seed)
    yy, xx = np.mgrid[:n, :n] / (n - 1)
    low = tg.fnoise(n, 2.9, seed, hi=.035)
    middle = tg.fnoise(n, 2.5, seed + 1, hi=.075)
    color = np.zeros((n, n, 3))
    height = np.zeros((n, n))
    roughness = np.full((n, n), .86)
    mask = np.zeros((n, n), dtype=bool)
    twig = Image.new('L', (n, n))
    draw = ImageDraw.Draw(twig)
    spine = [(.49, .94), (.51, .73), (.48, .49), (.53, .27), (.51, .12)]
    draw.line([(x * n, y * n) for x, y in spine], fill=255, width=max(3, n // 92))
    leaves = [
        ((.50, .46), (.49, .065), .184, False),
        ((.52, .43), (.075, .19), .173, False),
        ((.50, .43), (.935, .22), .177, False),
        ((.51, .59), (.055, .49), .185, False),
        ((.48, .58), (.945, .52), .189, False),
        ((.51, .72), (.09, .83), .148, True),
        ((.49, .72), (.91, .84), .147, True),
    ]
    for base, tip, width, heart in leaves:
        bx, by = base
        tx, ty = tip
        dx, dy = tx - bx, ty - by
        length = np.hypot(dx, dy)
        t = ((xx - bx) * dx + (yy - by) * dy) / length ** 2
        across = ((xx - bx) * -dy + (yy - by) * dx) / length
        phase = rng.uniform(0, np.pi * 2)
        cross = across / (width * (1 + .023 * np.sin(t * 41 + phase)))
        if heart:
            longitudinal = 1 - t * 2
            shape = (cross ** 2 + longitudinal ** 2 - 1) ** 3 - cross ** 2 * longitudinal ** 3
            leaf = (shape <= 0) & (t < 1) & (t > -.10)
            edge = np.clip(1 - np.abs(cross), 0, 1)
        else:
            profile = np.sin(np.clip(t, 0, 1) * np.pi) ** .61
            profile *= 1 + .027 * np.sin(t * 29 + phase)
            leaf = (np.abs(cross) < profile) & (t > 0) & (t < 1)
            edge = np.clip((profile - np.abs(cross)) / .4, 0, 1)
        warmth = np.clip(t * .58 + edge * .14 + low * .28, 0, 1)
        leaf_color = tg.mix(np.array([.115, .225, .085]), np.array([.365, .465, .145]), warmth)
        leaf_color *= (.95 + middle * .08)[..., None]
        midrib = np.exp(-((cross + .05 * np.sin(t * 7)) / .024) ** 2)
        secondary = 1 - tg.sstep(.014, .055, np.abs(np.sin((t * 7.2 - np.abs(cross) * .64) * np.pi)))
        veins = midrib * .7 + secondary * .22 * (1 - np.abs(cross))
        leaf_color = tg.mix(leaf_color, np.array([.395, .465, .19]), np.clip(veins * .25, 0, 1))
        color[leaf] = leaf_color[leaf]
        bowed = np.maximum(0, 1 - cross ** 2) * np.sin(np.clip(t, 0, 1) * np.pi)
        leaf_height = bowed * 1.9 + midrib * .12 + secondary * .038
        height[leaf] = leaf_height[leaf]
        roughness[leaf] = (.73 + low * .10 + t * .025)[leaf]
        mask |= leaf
        draw.line([(bx * n, by * n), ((bx + dx * .33) * n, (by + dy * .33) * n)],
                  fill=255, width=max(2, n // 154))
    twig_mask = np.asarray(twig) > 0
    uncovered_twig = twig_mask & ~mask
    color[uncovered_twig] = np.array([.22, .285, .105])
    height[uncovered_twig] = .12
    mask |= twig_mask
    padding = max(12, n // 25)
    mask[:padding] = mask[-padding:] = False
    mask[:, :padding] = mask[:, -padding:] = False
    # Continue the bowed surface through alpha edges so normals do not become rims.
    nearest = ndi.distance_transform_edt(~mask, return_distances=False, return_indices=True)
    extended_height = height[tuple(nearest)]
    normal = tg.normal_map(extended_height, 9 * 512 / n)
    normal[~mask] = np.array([.5, .5, 1])
    color[~mask] = 0
    alpha = mask.astype(float)
    return np.dstack((np.clip(color, 0, 1), alpha)), normal, np.clip(roughness, 0, 1)


def quantize(array):
    return np.rint(np.clip(array, 0, 1) * 255).astype(np.uint8)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--validate', action='store_true')
    args = parser.parse_args()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    PREVIEW.mkdir(parents=True, exist_ok=True)
    arrays = generate()
    if args.validate:
        repeated = generate()
        assert all(np.array_equal(quantize(a), quantize(b)) for a, b in zip(arrays, repeated)), 'nondeterministic'
    for array, shape in zip(arrays, ((512, 512, 4), (512, 512, 3), (512, 512))):
        assert array.shape == shape and np.isfinite(array).all()
        assert 0 <= array.min() <= array.max() <= 1
    albedo, normal, roughness = arrays
    occupancy = float(np.mean(albedo[..., 3] > .5))
    assert .55 <= occupancy <= .70, 'leaf alpha occupancy ' + str(occupancy)
    border = np.concatenate((albedo[:20, :, 3].ravel(), albedo[-20:, :, 3].ravel(),
                             albedo[:, :20, 3].ravel(), albedo[:, -20:, 3].ravel()))
    assert not border.any(), 'transparent mip padding'
    assert np.max(np.abs(np.linalg.norm(normal * 2 - 1, axis=-1) - 1)) < 1e-6
    paths = [OUTPUT / 'leaf_a.png', OUTPUT / 'leaf_n.jpg', OUTPUT / 'leaf_r.jpg']
    Image.fromarray(quantize(albedo)).save(paths[0], optimize=True)
    Image.fromarray(quantize(normal)).save(paths[1], quality=98, subsampling=0, optimize=True)
    Image.fromarray(quantize(roughness)).save(paths[2], quality=90, optimize=True)
    decoded = np.asarray(Image.open(paths[1])) / 255 * 2 - 1
    vectors = normal * 2 - 1
    cosine = np.sum(vectors * decoded / np.linalg.norm(decoded, axis=-1)[..., None], axis=-1)
    angles = np.degrees(np.arccos(np.clip(cosine, -1, 1)))
    p99 = float(np.percentile(angles[albedo[..., 3] > .5], 99))
    assert p99 < 2, 'normal JPEG angular error'
    report = {'size': 512, 'seed': 307, 'deterministic': args.validate, 'alpha_occupancy': occupancy,
              'transparent_padding_pixels': 20, 'normal_jpeg_angle_p99_degrees': p99,
              'textures': {p.name: {'bytes': p.stat().st_size,
                                  'sha256': hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths}}
    (PREVIEW / 'leaf-validation.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    # Composite previews use a restrained gray checker and separate data views.
    checker = (np.indices((512, 512))[0] // 32 + np.indices((512, 512))[1] // 32) % 2
    background = np.where(checker[..., None], np.array([.23, .25, .24]), np.array([.38, .4, .39]))
    composite = albedo[..., :3] * albedo[..., 3:] + background * (1 - albedo[..., 3:])
    sheet = Image.new('RGB', (1024, 1048), (25, 30, 27))
    for i, a in enumerate((composite, np.repeat(albedo[..., 3:], 3, axis=-1), normal,
                            np.repeat(roughness[..., None], 3, axis=-1))):
        sheet.paste(Image.fromarray(quantize(a)), ((i % 2) * 512, (i // 2) * 512 + 24))
    draw = ImageDraw.Draw(sheet)
    draw.text((8, 6), 'Original alpha-cut sprig / alpha / bowed normal / roughness', fill=(231, 238, 218))
    sheet.save(PREVIEW / 'leaf-contact-sheet.jpg', quality=94)
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()

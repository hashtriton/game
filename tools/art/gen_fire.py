"""Original looping 4x2 flame atlas for the arena particle material.

Run: py -3 -X utf8 tools/art/gen_fire.py --validate
Eight 128x256 frames share a periodic phase, with transparent per-frame padding.
"""
import argparse
import hashlib
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'art/ui/menu-mockup/src'))
import tg  # noqa: E402

OUTPUT = ROOT / 'unity/Assets/Game/Art/Textures'
PREVIEW = ROOT / '.local/art/daylight'
FRAME_WIDTH = 128
FRAME_HEIGHT = 256
PADDING = 8


def flame_frame(phase):
    yy, xx = np.mgrid[:FRAME_HEIGHT, :FRAME_WIDTH]
    x = (xx + .5) / FRAME_WIDTH
    h = 1 - (yy + .5) / FRAME_HEIGHT
    density = np.zeros_like(x)
    heat = np.zeros_like(x)
    tongues = ((.49, .25, .88, 0), (.36, .15, .67, 2.2), (.64, .135, .74, 4.1))
    for center, radius, top, offset in tongues:
        height = top + .038 * np.sin(phase + offset)
        t = np.clip((h - .055) / (height - .055), 0, 1)
        width = radius * np.sin(t * np.pi) ** .57 * (1 - t * .48)
        width *= .94 + .06 * np.sin(t * 15 - phase * 2 + offset)
        curve = .045 * np.sin(phase + offset) * t
        curve += .085 * np.sin(t * 5.5 - phase + offset) * t ** 1.6
        axis = center + curve
        across = np.abs(x - axis) / np.maximum(width, .0001)
        body = 1 - tg.sstep(.82, 1.06, across)
        body *= tg.sstep(.055, .09, h) * (1 - tg.sstep(height - .018, height, h))
        body *= (h > .055) & (h < height)
        density = np.maximum(density, body)
        tongue_heat = (1 - tg.sstep(.12, .97, across)) * (1 - t * .64)
        heat = np.maximum(heat, tongue_heat * body)
    # Rolling wisps open small gaps in the upper flame without random frame jumps.
    flow = np.sin(x * 25 + h * 19 - phase * 2) * np.sin(h * 24 + phase)
    wisps = tg.sstep(.48, .95, flow) * tg.sstep(.33, .77, h)
    density *= 1 - wisps * .46
    heat = np.clip(heat * 1.24 + density * .11, 0, 1)
    outer = np.array([.90, .255, .018])
    inner = np.array([1, .84, .115])
    rgb = outer + (inner - outer) * heat[..., None]
    rgb *= (.96 + .04 * np.sin(h * 11 - phase + x * 7))[..., None]
    density[:PADDING] = density[-PADDING:] = 0
    density[:, :PADDING] = density[:, -PADDING:] = 0
    rgb[density < .002] = 0
    density[density < .002] = 0
    return np.dstack((np.clip(rgb, 0, 1), density))


def generate():
    atlas = np.zeros((FRAME_HEIGHT * 2, FRAME_WIDTH * 4, 4))
    for frame in range(8):
        y, x = (frame // 4) * FRAME_HEIGHT, (frame % 4) * FRAME_WIDTH
        atlas[y:y + FRAME_HEIGHT, x:x + FRAME_WIDTH] = flame_frame(frame * np.pi / 4)
    return atlas


def quantize(array):
    return np.rint(np.clip(array, 0, 1) * 255).astype(np.uint8)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--validate', action='store_true')
    args = parser.parse_args()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    PREVIEW.mkdir(parents=True, exist_ok=True)
    atlas = generate()
    assert atlas.shape == (512, 512, 4) and np.isfinite(atlas).all()
    assert 0 <= atlas.min() <= atlas.max() <= 1
    if args.validate:
        assert np.array_equal(quantize(atlas), quantize(generate())), 'nondeterministic'
    assert np.array_equal(quantize(flame_frame(0)), quantize(flame_frame(np.pi * 2))), 'loop closure'
    coverage = []
    for index in range(8):
        y, x = (index // 4) * FRAME_HEIGHT, (index % 4) * FRAME_WIDTH
        frame = atlas[y:y + FRAME_HEIGHT, x:x + FRAME_WIDTH]
        alpha = frame[..., 3]
        border = np.r_[alpha[:PADDING].ravel(), alpha[-PADDING:].ravel(),
                       alpha[:, :PADDING].ravel(), alpha[:, -PADDING:].ravel()]
        assert not border.any(), 'frame padding ' + str(index)
        visible = alpha > .1
        assert .10 < visible.mean() < .6, 'empty or filled frame'
        rgb = quantize(frame[..., :3])[visible]
        assert not np.any(np.all(rgb == 255, axis=-1)), 'white pixel'
        assert np.all(rgb[:, 0] > rgb[:, 1]) and np.all(rgb[:, 1] > rgb[:, 2]), 'cold flame'
        coverage.append(float(visible.mean()))
    path = OUTPUT / 'fire_atlas.png'
    Image.fromarray(quantize(atlas)).save(path, optimize=True)
    report = {'atlas_shape': [512, 512, 4], 'frame_shape': [256, 128, 4], 'tiles': [4, 2],
              'frames': 8, 'deterministic': args.validate, 'phase_zero_equals_two_pi': True,
              'transparent_frame_padding_pixels': PADDING, 'alpha_area_over_point_one': coverage,
              'white_rgb_pixels': 0, 'bytes': path.stat().st_size,
              'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}
    (PREVIEW / 'fire-validation.json').write_text(json.dumps(report, indent=2) + '\n', encoding='utf-8')
    background = np.zeros((512, 512, 3)) + np.array([.07, .085, .09])
    preview = atlas[..., :3] * atlas[..., 3:] + background * (1 - atlas[..., 3:])
    sheet = Image.new('RGB', (1024, 536), (18, 23, 25))
    sheet.paste(Image.fromarray(quantize(preview)), (0, 24))
    sheet.paste(Image.fromarray(quantize(np.repeat(atlas[..., 3:], 3, axis=-1))), (512, 24))
    draw = ImageDraw.Draw(sheet)
    draw.text((8, 6), 'Original looping fire, 4x2 x 128x256 / alpha', fill=(247, 201, 118))
    sheet.save(PREVIEW / 'fire-contact-sheet.jpg', quality=95)
    print(json.dumps(report, indent=2))


if __name__ == '__main__':
    main()

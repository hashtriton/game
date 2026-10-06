"""FOOT1: B009 scale independence and native placement/occupant observations.

Scale invariance at sampled offsets does not expose an internal collision getter.
"""
import importlib.util
import json
import math
import struct
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, flat, load, require, sha, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T042445852512Z-18fe64f746b5'
CACHE = 'LiAFoot1.w3v'
CACHE_SHA = '18fe64f746b5f6fb30f950f05389c323f41c1355cf4906741610a4b459a5b515'
PROBE_SHA = '9553bbe045bca78f585699c03a4309782e46ff36c1afb074ebf2ab31eeac3b16'
SCRIPT_SHA = '569ce807cda08ed8ddad71a8287c189598aa1f89d236efd6df94c0145e4b357b'
MASK_SHA = 'c278ea75ba0fa7392251ad00a1dfd1c069df4583a52e0aacf882135dcd44b639'
POINTS = [(x, 0) for x in (-288,-192,-160,-152,-144,-128,-112,-96,0,96,112,128,144,152,160,192,288)]
POINTS += [(0,128),(0,160),(0,192),(112,112),(144,144),(192,192)]
SCALES = (0, .5, 1, 1.2, 2, 1.2)


def state(row, prefix):
    return {k[len(prefix):]: v for k, v in row.items() if k.startswith(prefix)}


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema'] == 68 and meta['complete'] == 1 and
            meta['records_expected'] == meta['records_finished'] == meta['records_succeeded'] == 6 and
            meta['records_failed'] == 0 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete FOOT1')
    require(set(rows) == {'meta'} | {'scale_' + str(i) for i in range(6)}, 'Wrong FOOT1 categories')
    require([(r['key'], r['id'], r['requestedLevel'], r['scale'], r['occupant']) for r in report['records']] ==
            [('scale_' + str(i), 'H008', 1, scale, i == 5) for i, scale in enumerate(SCALES)], 'Wrong FOOT1 matrix')
    records = []
    for i, scale in enumerate(SCALES):
        key = 'scale_' + str(i)
        row = rows[key]
        require(row['known'] == 1 and row['points_expected'] == 23 and
                abs(row['requested_scale'] - scale) < .000001, 'Invalid FOOT1 scale row')
        require(all(math.isfinite(v) for v in row.values() if type(v) in (int, float)), 'Nonfinite FOOT1')
        require(row['block_created'] == int(i > 0), 'Wrong destructable existence')
        if i > 0:
            require(row['block_id'] == rawcode('B009') and row['block_x'] == 512 and row['block_y'] == 1024 and
                    row['block_life'] == 9999, 'Wrong B009 identity or placement')
        if i < 5:
            require(row['points_finished'] == 23 and row['error'] == 'Raw three-stage placements complete', 'Incomplete placement row')
            stages = {}
            for stage in ('before', 'blocked', 'removed'):
                values = []
                for j, (dx, dy) in enumerate(POINTS):
                    value = state(row, stage + str(j) + '_')
                    require(value['requested_x'] == 512 + dx and value['requested_y'] == 1024 + dy and
                            value['terrain_unwalkable'] in (0, 1) and value['id'] == rawcode('H008') and
                            value['hp'] == 631 and value['paused'] == 1 and value['order'] == 851973,
                            'Wrong placement request or native unit state')
                    values.append(value)
                stages[stage] = values
            require(stages['before'] == stages['removed'], 'Removal did not restore every baseline placement')
            if i == 0:
                require(stages['blocked'] == stages['before'], 'No-blocker control changed')
            else:
                require(stages['before'] == records[0]['stages']['before'], 'Fresh baseline differs between scales')
                moved = [j for j in range(23) if (stages['blocked'][j]['x'], stages['blocked'][j]['y']) !=
                         (stages['before'][j]['x'], stages['before'][j]['y'])]
                require(moved == [5,6,7,8,9,10,11,12,13,17,18,19,20,21], 'Positive obstruction/control pattern changed')
                if i > 1:
                    require(stages['blocked'] == records[1]['stages']['blocked'], 'Scale-invariant native placements changed')
            records.append(dict(sourceKey=key, requestedScale=scale, stages=stages))
        else:
            require(row['hold_accepted'] == row['move_accepted'] == 1 and row['samples'] == 30 and
                    row['error'] == 'Raw occupant movement complete', 'Incomplete occupant control')
            initial = [state(row, 'occupant_' + stage + '_') for stage in ('before', 'immediate', 'next_tick')]
            samples = [state(row, 'sample' + str(j) + '_') for j in range(30)]
            for value in initial + samples:
                require(value['id'] == rawcode('H008') and value['paused'] == 0 and value['hp'] == 631 and
                        value['x'] == 512 and value['y'] == 1024, 'Native occupant state changed')
            require(all(abs(s['elapsed'] - (j + 1) * .1) < .000001 for j, s in enumerate(samples)) and
                    samples[-1]['order'] == 0, 'Occupant trace clock or final order changed')
            records.append(dict(sourceKey=key, requestedScale=scale, initial=initial, samples=samples,
                                moveAccepted=True, nativeRelocationObserved=False, egressObserved=False))
    return records


def extract():
    report = load(LOCAL / 'foot1-verification.json')
    require(report['cacheName'] == CACHE and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and
            report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and report['nativePreflightPassed'],
            'Wrong FOOT1 provenance')
    for path, digest in ((Path(report['map']), PROBE_SHA), (LOCAL / 'foot1.j', SCRIPT_SHA),
                         (CAPTURE / 'Campaigns.w3v', CACHE_SHA),
                         (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)):
        require(sha(path.read_bytes()) == digest, 'Changed FOOT1 source ' + str(path))
    mask_path = ROOT / '.local/lia-port/native126/raw/war3.mpq/PathTextures/8x8Unflyable.tga'
    mask = mask_path.read_bytes()
    require(sha(mask) == MASK_SHA and mask[0] == mask[1] == 0 and mask[2] == 2 and
            struct.unpack_from('<HH', mask, 12) == (8, 8) and mask[16] == 24 and
            mask[18:18+8*8*3] == b'\xff' * (8*8*3), 'Native pathing texture bytes changed')
    spec = importlib.util.spec_from_file_location('footprint_cache', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(reader)
    fresh = reader.parse((CAPTURE / 'Campaigns.w3v').read_bytes())
    saved = load(CAPTURE / 'parsed.json')
    require(saved['sourceSha256'] == CACHE_SHA and fresh['caches'] == saved['caches'], 'Fresh FOOT1 CRC differs')
    rows = {k: flat(v) for k, v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, capturedUtc=saved['capturedUtc'],
                            probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA),
                maskSource=dict(archive='war3.mpq', entry='PathTextures\\8x8Unflyable.tga', sha256=MASK_SHA,
                                width=8, height=8, pixels='all-white', declarationCellSize=32),
                records=normalize(rows, report), scaleInvariantAtSampledOffsets=True,
                limits=[
                    'All23 native H008 placements agree exactly for requested B009 scales0.5/1/1.2/2 at fixedcenter512,1024,facing0. Removal restores every baseline; no-blocker control remains unchanged.',
                    'The native8x8 full-white pathing texture declares256WC at32WC per cell. The sampled placement result supports an unscaled host footprint; it is not a continuous collision-radius getter or complete raster-mask scan.',
                    'Native CreateUnit placement is asymmetric and some Y-axis baseline requests relocate without B009. Preserve raw requested/actual positions; do not infer mask edges from requested coordinates alone.',
                    'An existing center occupant is not auto-ejected. Its accepted ordinary move remains atcenter for3seconds, finalorder0. No automatic escape/relocation policy is justified by this case.',
                    'Only facing0 and an aligned fixed center were measured. Host quantization at source off-grid rift positions, rotated footprints, arbitrary actors and exact path solver parity remain derived.',
                    'IsTerrainPathable is retained as raw native output, not assumed to expose every dynamic blocker.'
                ])


if __name__ == '__main__':
    result = extract()
    path = ROOT / '.local/lia-port/abilities/destructable-footprint-observations.json'
    path.write_text(json.dumps(result, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(path), sha256=sha(path.read_bytes()), records=len(result['records']))))

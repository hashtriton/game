"""Validate REGENL1 lifecycle controls without treating paused rates as full rates."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T022714954153Z-cbd941979144'
CACHE = 'LiARegenL1.w3v'
CACHE_SHA = 'cbd941979144c921ea546fdbe7316b80c26591765f3e3098956765a0b14f6520'
PROBE_SHA = 'cb803d23606c1b3751c6f3e45cbf70f1c67cefdbfa0a37cc89ce3fca7317b5ae'
SCRIPT_SHA = 'e9bf5a70a25f0a186b7f0d287200995190c81030671ddf721bc63dea4f18857c'
CASES = [('H008', 1, 22, 6, 7, 1.3, .05), ('H008', 10, 49, 24, 25, 1.3, .05),
         ('H024', 10, 27, 25, 47, 1.15, .01), ('N0A0', 10, 22, 49, 21, 1, .05),
         ('O006', 50, 250, 250, 250, 0, 0)]
PHASES = [('after_visibility', 'fresh_pause', True, False), ('unpause_start', 'unpaused', False, False),
          ('warm_pause_start', 'warm_paused', True, False), ('hidden_start', 'hidden', True, True),
          ('visible_start', 'visible', True, False)]


def number(row, key):
    value = row[key]
    require(type(value) in (int, float) and math.isfinite(value), 'Invalid number:' + key)
    return value


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema'] == 43 and meta['complete'] == 1 and meta['records_expected'] == 5 and
            meta['records_finished'] == meta['records_succeeded'] == 5 and meta['records_failed'] == 0 and
            meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401', 'Incomplete regeneration matrix')
    keys = [f'regen_lifecycle_{unit}_{level}' for unit, level, *_ in CASES]
    require(set(rows) == {'meta'} | set(keys) and [r['key'] for r in report['records']] == keys, 'Wrong regeneration cases')
    result = []
    for key, (unit, level, strength, agility, intelligence, base_hp, base_mp) in zip(keys, CASES):
        row = rows[key]
        require(row['known'] == row['created'] == 1 and row['strays'] == 0 and row['id'] == unit and
                row['id_integer'] == rawcode(unit) and row['requested_level'] == level, 'Wrong lifecycle identity:' + key)
        snapshots = {}
        for phase_index, (start, phase, paused, hidden) in enumerate(PHASES):
            prefixes = [start, phase + '_1', phase + '_2']
            for index, prefix in enumerate(prefixes):
                p = prefix + '_'
                require(row[p+'id'] == rawcode(unit) and row[p+'owner'] == 0 and row[p+'hero'] == 1 and
                        row[p+'illusion'] == 0 and row[p+'level'] == level and
                        row[p+'str'] == strength and row[p+'agi'] == agility and row[p+'int'] == intelligence and
                        row[p+'paused'] == int(paused) and row[p+'hidden'] == int(hidden), 'Wrong lifecycle state:' + key + ':' + prefix)
                sample = {field: number(row, p+field) for field in ('time', 'hp', 'mp', 'maxhp', 'maxmp', 'x', 'y')}
                require(sample['hp'] > .405 and 0 < sample['mp'] < sample['maxmp'] and sample['hp'] < sample['maxhp'] and
                        sample['x'] == 135 and sample['y'] == 1000, 'Capped, dead or moved lifecycle subject')
                require(abs(sample['time'] - (phase_index * 2 + index)) < .003, 'Unexpected sample clock')
                snapshots[prefix] = sample
            first = snapshots[start]
            for index in (1, 2):
                sample = snapshots[phase + '_' + str(index)]
                expected_hp = base_hp + (0 if paused else strength * .05)
                expected_mp = base_mp + (0 if paused else intelligence * .05)
                elapsed = sample['time'] - first['time']
                require(abs((sample['hp'] - first['hp']) / elapsed - expected_hp) < .02 and
                        abs((sample['mp'] - first['mp']) / elapsed - expected_mp) < .02, 'Changed regeneration rates:' + key + ':' + phase)
        result.append(dict(sourceKey=key, unitId=unit, level=level, strength=strength, intelligence=intelligence,
            pausedHealthRate=base_hp, pausedManaRate=base_mp, unpausedHealthRate=base_hp+strength*.05,
            unpausedManaRate=base_mp+intelligence*.05, snapshots=snapshots))
    return result


def extract():
    report = load(LOCAL / 'regenl1-verification.json')
    require(report['cacheName'] == CACHE and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and
            report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and report['nativePreflightPassed'], 'Wrong probe provenance')
    for path, digest in ((Path(report['map']), PROBE_SHA), (LOCAL/'regenl1.j', SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA), (CAPTURE/'Campaigns.w3v', CACHE_SHA)):
        require(sha(path.read_bytes()) == digest, 'Changed proof:' + str(path))
    spec = importlib.util.spec_from_file_location('regeneration_reader', LOCAL/'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    fresh = reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved = load(CAPTURE/'parsed.json')
    require(saved['sourceSha256'] == CACHE_SHA and fresh['caches'] == saved['caches'], 'Fresh CRC differs')
    rows = {key: flat(value) for key, value in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
            probeScriptSha256=SCRIPT_SHA, capturedUtc=saved['capturedUtc'], records=5), rows=normalize(rows, report),
        limits=['Fresh and warm paused subjects retain base rates but suppress attribute rates. Hidden observations here are also paused.',
            'The attribute decomposition is inferred from five measured profiles and the authored .05 STR/INT regeneration constants.',
            'Samples allow native timer and floating-point drift below .02 resource/second. They do not define an exact subframe scheduler.',
            'No items, buffs, original triggers or damage occurred. Paused item/aura behavior is not established.',
            'O006 level50 has measured base0/0, unlike the sparse mana declaration1. Its unpaused health regenerates despite regenType=none.'])


if __name__ == '__main__':
    output = ROOT/'.local/lia-port/abilities/regeneration-native-observations.json'
    data = extract(); output.write_text(json.dumps(data, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), rows=len(data['rows']))))

"""Normalize the immutable item-resource experiment; do not infer a tie policy."""
from collections import Counter
import hashlib
import importlib.util
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/lia-port/research-map'
CAPTURE = LOCAL / 'cache-captures/20261005T224213631738Z-ba4a1fcb34ce'
MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
CACHE_SHA = 'ba4a1fcb34cebc8f6feaa9ccf4e59abe33df74e85bb57c08008c83fe19f6fc8b'
PROBE_SHA = '296964d05f21536a7b9d6346ec0e7af2df08385392922f74aaca69d118ef193e'
SCRIPT_SHA = 'f4fa17ede351bd66f61f2aa3292b2b085c1af4c62219bdebad5ecc44c6690faf'


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def load(path):
    return json.loads(Path(path).read_text(encoding='utf8'))


def rawcode(value):
    return int.from_bytes(value.encode('ascii'), 'big')


def flat(row):
    result = {}
    for kind in ('integers', 'reals', 'booleans', 'strings'):
        for key, field in row[kind].items():
            require(key not in result, 'Cross-type duplicate ' + key)
            result[key] = field['value']
    return result


def expected_matrix():
    result = []
    configurations = [('H008', []), ('N0A0', []), ('H024', []),
        ('H008', ['I00R']), ('H008', ['I00R', 'I00R']), ('H008', ['I042']),
        ('H008', ['I042', 'I042']), ('H008', ['I00R', 'I042']),
        ('H008', ['I01E']), ('H008', ['I01E', 'I01E']),
        ('H008', ['I00C']), ('H008', ['I00C', 'I00C'])]
    for index, (hero, items) in enumerate(configurations):
        result.append(dict(key=f'regen_{index}_{hero}', hero=hero, level=1, priorRank=0,
                           mode=0, itemIds=items, hpValue=0, fraction=.2, offset=0))
    for index, hp in enumerate((.4051, .45, .499, .5, .5001, .75, 1., 1.001)):
        result.append(dict(key=f'low_remove_{index}', hero='H008', level=1, priorRank=0,
                           mode=2, itemIds=['I00C'], hpValue=hp, fraction=.2, offset=0))
    for index, hp in enumerate((.4051, .45, .5001)):
        result.append(dict(key=f'low_add_{index}', hero='H008', level=1, priorRank=0,
                           mode=1, itemIds=[], hpValue=hp, fraction=.2, offset=0))
    for prior in (0, 14):
        for mode in (1, 3):
            for fraction in (.25, .75):
                for index, offset in enumerate((-.001, 0, .001)):
                    result.append(dict(key=f'tie_r{prior}_m{mode}_f{int(fraction*100)}_o{index}',
                        hero='N0A0', level=27, priorRank=prior, mode=mode, itemIds=[],
                        hpValue=0, fraction=fraction, offset=offset))
    return result


def snapshot(row, prefix, case):
    fields = {key[len(prefix):]: value for key, value in row.items() if key.startswith(prefix)}
    for key in ('time', 'hp', 'maxhp', 'mp', 'maxmp', 'move_speed'):
        require(type(fields[key]) in (int, float) and math.isfinite(fields[key]), 'Nonfinite sample ' + key)
    require(fields['level'] == case['level'] and fields['death_events'] == fields['dead'] == 0,
            'Hero level/death changed')
    require(0 < fields['hp'] <= fields['maxhp'] and 0 <= fields['mp'] <= fields['maxmp'], 'Invalid resources')
    require(all(type(fields[k]) is int and fields[k] >= 0 for k in ('rank', 'points', 'xp')),
            'Invalid progression sample')
    return fields


def inventory(sample):
    return Counter(sample['slot' + str(i)] for i in range(6) if sample['slot' + str(i)] != 0)


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema'] == 2 and meta['complete'] == meta['strings_ok'] == 1 and
            meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401', 'Wrong/incomplete resource probe')
    require(meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 47 and
            meta['records_failed'] == 0, 'Unexpected resource experiment failures')
    expected = expected_matrix()
    matrix = report['records']
    require(len(matrix) == len(expected) and
            [{key: case[key] for key in expected[0]} for case in matrix] == expected,
            'Resource experiment matrix changed')
    require(set(rows) == {'meta'} | {case['key'] for case in matrix}, 'Resource experiment rows changed')
    regen, changes = [], []
    for case in matrix:
        row = rows[case['key']]
        require(row['known'] == 1 and row['requested_hero'] == rawcode(case['hero']) and
                row['mode'] == case['mode'] and row['prior_rank'] == case['priorRank'], 'Wrong resource case identity')
        before = snapshot(row, 'before_', case)
        require(before['rank'] == case['priorRank'] and before['hp'] > .405, 'Wrong before rank/life')
        expected_items = Counter(rawcode(item) for item in case['itemIds'])
        require(inventory(before) == expected_items, 'Wrong before inventory')
        if case['mode'] == 0:
            samples = [snapshot(row, f'sample{i}_', case) for i in range(1, 11)]
            final = snapshot(row, 'final_', case)
            require(final == samples[-1], 'Final regen sample differs from sample10')
            previous = before
            for sample in samples:
                require(sample['time'] > previous['time'], 'Nonmonotonic sample clock')
                require(all(sample[key] == before[key] for key in before if key not in ('hp', 'mp', 'time')),
                        'Regen experiment state changed')
                require(sample['hp'] >= previous['hp'] and sample['mp'] >= previous['mp'] and
                        sample['hp'] < sample['maxhp'] and sample['mp'] < sample['maxmp'], 'Regeneration reached cap or decreased')
                previous = sample
            elapsed = final['time'] - before['time']
            require(4.9 < elapsed < 5.1, 'Wrong regen observation interval')
            regen.append(dict(sourceKey=case['key'], heroId=case['hero'], itemIds=case['itemIds'],
                before=before, samples=samples, elapsed=elapsed,
                measuredHealthPerSecond=(final['hp'] - before['hp']) / elapsed,
                measuredManaPerSecond=(final['mp'] - before['mp']) / elapsed))
        else:
            after = snapshot(row, 'immediate_', case)
            delayed = snapshot(row, 'delayed_', case)
            require(before['time'] == after['time'] < delayed['time'], 'Operation was not measured in the same callback')
            if case['mode'] == 1:
                expected_items.update([rawcode('I00C')])
            elif case['mode'] == 2:
                expected_items.subtract([rawcode('I00C')]); expected_items = +expected_items
            require(inventory(after) == inventory(delayed) == expected_items, 'Inventory transition mismatch')
            require(after['rank'] == delayed['rank'] == before['rank'] + int(case['mode'] == 3), 'Rank transition mismatch')
            require(before['points'] - after['points'] == int(case['mode'] == 3), 'Skill point debit mismatch')
            require(all(after[k] == before[k] for k in ('str_base', 'agi_base', 'int_base', 'xp')), 'Unexpected base-stat/XP change')
            changes.append(dict(sourceKey=case['key'], heroId=case['hero'], mode=case['mode'],
                priorRank=case['priorRank'], requestedFraction=case['fraction'], requestedOffset=case['offset'],
                before=before, immediate=after, delayed=delayed))
    require(len(regen) == 12 and len(changes) == 35, 'Wrong resource category count')
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
        source=dict(cacheName='LiAItemR1.w3v', cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
                    probeScriptSha256=SCRIPT_SHA, records=47, passed=47, failed=0),
        regeneration=regen, transitions=changes,
        limits=['Recorded resources are native float32 getters; exact float bits remain in the immutable parsed capture.',
                'The measured five-second slopes are not a proof of native tick cadence.',
                'Eleven near-death item changes remain alive at one HP; no dead-unit revival was tested.',
                'The 24 half-tie cases contradict uniform double-precision half-up rounding. A general exact tie formula remains unresolved.',
                'No original map triggers ran. Measurements are scoped to the named heroes, items, ranks, and operations.'])


def extract():
    spec = importlib.util.spec_from_file_location('resource_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Resource cache changed')
    parsed = reader.parse(raw)
    saved = load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == saved['caches'], 'Fresh CRC parse differs from saved capture')
    report = load(LOCAL / 'item-resource-probe-verification.json')
    require(report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA,
            'Resource report identity changed')
    for path, expected in ((LOCAL / 'LiA39c_ITEM_RESOURCE_PROBE.w3x', PROBE_SHA),
                           (LOCAL / 'item-resource-probe.j', SCRIPT_SHA),
                           (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)):
        require(sha(path.read_bytes()) == expected, 'Source/probe changed: ' + str(path))
    rows = {key: flat(row) for key, row in parsed['caches']['LiAItemR1.w3v']['categories'].items()}
    return normalize(rows, report)


if __name__ == '__main__':
    result = extract()
    output = ROOT / '.local/lia-port/lia39-observed-resources126.json'
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), regeneration=len(result['regeneration']),
                          transitions=len(result['transitions']))))

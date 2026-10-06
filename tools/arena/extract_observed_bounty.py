"""Read immutable native bounty capture; preserve samples, not inferred dice defaults."""
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
HERE = ROOT / '.local/lia-port/research-map'
sys.path.insert(0, str(HERE))
import read_probe_cache
CAPTURE = HERE / 'cache-captures/20261005T223514453119Z-ecffc4a0cbec'
CACHE_SHA = 'ecffc4a0cbec11b3279b16e7acfba7d054e177fc0e655c7c20810a830e52b28e'
SOURCE_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def flat(row):
    result = {}
    for kind in ('integers', 'reals', 'booleans', 'strings'):
        for key, field in row[kind].items():
            require(key not in result, 'Cross-type duplicate ' + key)
            result[key] = field['value']
    return result


def extract():
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Capture changed')
    parsed = read_probe_cache.parse(raw)
    saved = json.loads((CAPTURE / 'parsed.json').read_text(encoding='utf8'))
    require(parsed['caches'] == saved['caches'], 'Saved parse differs from fresh CRC-verified parse')
    report = json.loads((HERE / 'bounty-probe-verification.json').read_text(encoding='utf8'))
    require(report['sourceMapSha256'] == SOURCE_SHA, 'Probe source changed')
    require(sha((HERE / 'LiA39c_BOUNTY_PROBE.w3x').read_bytes()) == report['mapSha256'], 'Probe map changed')
    require(sha((HERE / 'bounty-probe.j').read_bytes()) == report['scriptSha256'], 'Probe script changed')
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == SOURCE_SHA, 'Original map changed')
    rows = {key: flat(row) for key, row in parsed['caches']['LiABounty1.w3v']['categories'].items()}
    meta = rows.pop('meta')
    require(meta['complete'] == meta['positive_control_passed'] == meta['strings_ok'] == 1, 'Incomplete probe')
    require(meta['records_expected'] == meta['records_finished'] == len(rows) == 340, 'Record count')
    require(meta['records_succeeded'] == 336 and meta['records_failed'] == 4 and meta['stray_deaths'] == 0, 'Unexpected probe counts')
    require(meta['source_map_sha256'] == SOURCE_SHA, 'Cache source identity')
    records = report['records']
    require(set(rows) == {r['key'] for r in records} and len(records) == 340, 'Matrix identity')
    units, controls = {}, []
    for record in records:
        row = rows[record['key']]
        require(row['requested_target'] == int.from_bytes(record['id'].encode('ascii'), 'big'), 'Requested rawcode')
        require(row['requested_user_data'] == record['userData'] and row['bounty_enabled'] == record['bounty'], 'Requested setup')
        valid = row['observation_valid'] == 1
        sample = {'sourceKey': record['key'], 'created': row['created'] == 1, 'valid': valid}
        if not valid:
            require(record['id'] == 'n068' and row['created'] == 0 and 'gold_delta' not in row, 'Unexpected failed case')
            sample['error'] = row['error']
        else:
            require(row['created'] == row['damage_accepted'] == row['event_target_matches'] == row['killer_is_attacker'] == 1, 'Damage identity')
            require(row['death_events'] == 1 and row['killer_owner'] == 0 and row['event_dying_owner'] == 11, 'Death attribution')
            require(row['event_dying_id'] == row['requested_target'] and row['killer_id'] == int.from_bytes(b'H008', 'big'), 'Event rawcode')
            require(row['native_gives_bounty'] == record['bounty'] and row['actual_user_data'] == record['userData'], 'Actual setup')
            if record['id'] == 'O006':
                require(row['actual_unit_level'] == 50, 'Final boss level')
            require(row['before_killer_gold'] == row['before_observer_gold'] == row['before_killer_lumber'] == row['before_observer_lumber'] == 10000, 'Before currency')
            require(row['gold_delta'] >= 0 and row['other_gold_delta'] == row['lumber_delta'] == row['other_lumber_delta'] == 0, 'Unexpected recipient/currency')
            for stage in ('event_', 'immediate_', 'delayed_'):
                require(row[stage + 'killer_gold'] == 10000 + row['gold_delta'], 'Unstable bounty timing')
                require(row[stage + 'observer_gold'] == row[stage + 'killer_lumber'] == row[stage + 'observer_lumber'] == 10000, 'Unexpected currency stage')
            sample.update(gold=row['gold_delta'], observerGold=row['other_gold_delta'], lumber=row['lumber_delta'],
                          actualUnitLevel=row['actual_unit_level'], userData=row['actual_user_data'],
                          killerExperience=row['delayed_killer_xp'] - row['before_killer_xp'],
                          observerExperience=row['delayed_observer_xp'] - row['before_observer_xp'],
                          damageTime=row['before_time'], observedTime=row['delayed_time'])
        if record['control']:
            require(valid and (58 <= sample['gold'] <= 74 if record['control'] == 1 else sample['gold'] == 0), 'Control failed')
            controls.append(sample)
        else:
            unit = units.setdefault(record['id'], {'id': record['id'], 'sourceFields': record['sourceBountyFields'], 'samples': [], 'distributionKnown': False})
            unit['samples'].append(sample)
    require(len(units) == 84 and len(controls) == 4, 'Unit/control counts')
    for unit in units.values():
        require(len(unit['samples']) == 4, 'Missing repetition')
        unit['measured'] = all(sample['valid'] for sample in unit['samples'])
        if unit['measured']:
            gold = [sample['gold'] for sample in unit['samples']]
            unit.update(observedMin=min(gold), observedMax=max(gold))
            base = next((field for field in unit['sourceFields'] if field['key'] == 'bountyplus'), None)
            if base is not None:
                require(base['isNumber'] and not base['conflict'], 'Source base ambiguous')
                unit['observedEqualsDeclaredBase'] = all(value == base['number'] for value in gold)
    return {'schemaVersion': 1, 'mapSha256': SOURCE_SHA, 'engineVersion': '1.26.0.6401',
            'source': {'cacheName': 'LiABounty1.w3v', 'cacheSha256': CACHE_SHA, 'capturedUtc': saved['capturedUtc'],
                       'probeMapSha256': report['mapSha256'], 'probeScriptSha256': report['scriptSha256'],
                       'records': 340, 'succeeded': 336, 'failed': 4, 'controlsPassed': True},
            'units': list(units.values()), 'controls': controls,
            'limits': ['Four samples are observations, not proof of a constant reward or a complete random distribution.',
                       'No original reward triggers ran. Native experience observations must not replace original Ibv custom experience.',
                       'Native GIVES_BOUNTY is enabled except in two negative controls; killer is a real hero owned by player0.',
                       'Missing n068 was not created. Its gold and experience remain unknown.',
                       'Blank bounty fields are preserved, not filled with zero from these four observations.']}


if __name__ == '__main__':
    result = extract()
    output = HERE.parent / 'lia39-observed-bounty126.json'
    with output.open('x', encoding='utf8') as file:
        json.dump(result, file, indent=2, ensure_ascii=False, allow_nan=False)
        file.write('\n')
    print(json.dumps({'path': str(output), 'sha256': sha(output.read_bytes()), 'units': len(result['units'])}))

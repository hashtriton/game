"""Read compact native equip snapshots with explicit sparse-query completeness."""
from pathlib import Path
import argparse
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, require, load, sha, rawcode, flat, bit
from extract_observed_item_remaining import expected_matrix
from extract_observed_item_equip import PHASES, COUNTS

CACHE = 'LiAEq2.w3v'
PROBE_SHA = '980448f918e4b9c4434070f48622b1a6029d21cb01a87a9cd4ac93a76a28d831'
SCRIPT_SHA = 'fae3aacc7c0e764c79473aaf6ed02046923c861605d1e4935a892c0c71d8c3b2'
BULK_SHA = 'ae7ed283712c7c7d9f6fbab40b3014a4f057cfb0e3c7f015f92e0c917712dc53'
REAL_FIELDS = dict(t='time', hp='health', mh='maxHealth', mp='mana', mm='maxMana', s='moveSpeed')
ATTRIBUTE_FIELDS = dict(sb='baseStrength', st='strength', ab='baseAgility', at='agility', ib='baseIntelligence', it='intelligence')


def matrix(items, passives, first, bulk):
    require(bulk['mapSha256'] == MAP_SHA and bulk['source']['cacheSha256'] == BULK_SHA and
            len(bulk['items']) == 440, 'Wrong native powerup exclusion source')
    direct = {i['id']: i for i in bulk['items']}
    require(len(direct) == 440 and all(i['directKnown'] for i in direct.values()), 'Incomplete native item flags')
    original = expected_matrix(items, passives, first)
    included = [r for r in original if not direct[r['id']]['powerup']]
    excluded = [r['id'] for r in original if direct[r['id']]['powerup']]
    require(len(included) == 221 and len(excluded) == 118, 'Unexpected native equip candidate matrix')
    return included, excluded


def normalize(categories, report, cases, *, expected_count=221, schema=3, min_checkpoints=13):
    rows = {key: flat(value) for key, value in categories.items()}
    meta = rows['meta']
    require(report['records'] == cases and set(rows) == {'meta'} | {r['key'] for r in cases}, 'Wrong compact matrix')
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == schema and bit(meta, 'complete') and bit(meta, 'strings_ok') and
            meta['records_expected'] == meta['records_finished'] == len(cases) == expected_count, 'Incomplete compact equip capture')
    require(type(meta['checkpoint_attempts']) is int and meta['checkpoint_attempts'] >= min_checkpoints and
            type(meta['earlier_save_failures']) is int and 0 <= meta['earlier_save_failures'] < meta['checkpoint_attempts'],
            'Invalid native checkpoint ledger')
    result, known = [], 0
    for case in cases:
        row = rows[case['key']]
        require(row['requested_id'] == rawcode(case['id']), 'Wrong requested native item')
        measured = bit(row, 'known')
        require(measured != bool(row.get('error')), 'Success/error disagreement')
        target = dict(id=case['id'], known=measured, error=row.get('error', ''), sourceKey=case['key'],
                      directAbilityIds=case['directAbilities'], abilityIds=case['abilities'], snapshots=[])
        if measured:
            known += 1
            previous_time = -1
            for index, (phase, count) in enumerate(zip(PHASES, COUNTS)):
                prefix = 'p' + str(index)
                values = {k[len(prefix):]: v for k, v in row.items() if k.startswith(prefix)}
                required = set(REAL_FIELDS) | set(ATTRIBUTE_FIELDS) | {'l','e','n','v','q','c'}
                require(required <= values.keys(), 'Incomplete compact snapshot')
                # Native GetUnitAbilityLevel is called for every matrix entry.
                # Missing ranks are zero only after this exact completion gate.
                require(values['c'] == 1 and values['q'] == len(case['abilities']), 'Unfinished native rank query sequence')
                ranks = []
                for i in range(len(case['abilities'])):
                    key = 'r' + str(i)
                    rank = values.get(key, 0)
                    require(type(rank) is int and 0 <= rank <= 2147483647 and (key not in values or rank > 0), 'Invalid sparse rank encoding')
                    ranks.append(rank)
                allowed = required | {'r'+str(i) for i in range(len(case['abilities']))}
                require(not (values.keys() - allowed), 'Unexpected/unqueried compact field')
                require(values['l'] == 1 and values['e'] == 0 and values['n'] == count and values['v'] == 1,
                        'Native level/XP/residency disagreement')
                require(all(type(values[k]) is int and 0 <= values[k] <= 2147483647 for k in ATTRIBUTE_FIELDS), 'Invalid native attributes')
                require([values[k] for k in ('sb','ab','ib')] == [22,6,7], 'Base attributes changed')
                require(all(type(values[k]) in (int,float) and math.isfinite(values[k]) and values[k] >= 0 for k in REAL_FIELDS), 'Invalid native reals')
                require(values['t'] >= previous_time and 0 < values['hp'] <= values['mh'] and values['mp'] <= values['mm'], 'Invalid native vitality/time')
                previous_time = values['t']
                snapshot = dict(phase=phase, copies=count, level=1, xp=0,
                                **{name: values[key] for key,name in REAL_FIELDS.items()},
                                **{name: values[key] for key,name in ATTRIBUTE_FIELDS.items()}, abilityRanks=ranks)
                snapshot['rawRealBits'] = {name: categories[case['key']]['reals'][prefix+key]['encodedHex'] for key,name in REAL_FIELDS.items()}
                target['snapshots'].append(snapshot)
            baseline = target['snapshots'][0]
            require([baseline[k] for k in ('strength','agility','intelligence','maxHealth','maxMana','moveSpeed')] == [22,6,7,631,145,250],
                    'Unexpected fresh native baseline')
        result.append(target)
    require(known == meta['records_passed'] and expected_count-known == meta['records_failed'], 'Native pass/fail counts disagree')
    return result


def extract(capture, expected_sha):
    capture = Path(capture)
    require(sha((ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original map changed')
    report = load(LOCAL/'item-equip-compact-probe-verification.json')
    require(report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and report['cacheName'] == CACHE and
            report['nativePreflightPassed'] and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and
            sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL/'item-equip-compact-probe.j').read_bytes()) == SCRIPT_SHA,
            'Compact probe identity mismatch')
    raw = (capture/'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha, 'Campaign hash mismatch')
    spec = importlib.util.spec_from_file_location('compact_equip_cache_reader', LOCAL/'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed = reader.parse(raw); saved = load(capture/'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Fresh CRC parse differs')
    cases, excluded = matrix(load(ROOT/'unity/Assets/Arena/Data/lia39-items.json'),
                             load(ROOT/'unity/Assets/Arena/Data/lia39-item-passives.json'),
                             load(LOCAL/'item-equip-probe-verification.json'),
                             load(ROOT/'unity/Assets/Arena/Data/lia39-observed-items126.json'))
    rows = normalize(parsed['caches'][CACHE]['categories'], report, cases)
    return dict(schemaVersion=3, mapSha256=MAP_SHA, engineVersion='1.26.0.6401', heroId='H008', heroLevel=1,
                source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], records=221, complete=True), items=rows,
                excludedNativePowerups=dict(cacheSha256=BULK_SHA, ids=excluded), limits=[
                    'Own original-free native equip sequence only; no scripted pickup/use effects.',
                    'Sparse zero ranks are measured queries, guarded by per-snapshot completed/query-count markers.',
                    'No measured inventory slot positions are claimed: native count and absence of unrelated items are recorded.',
                    'No failed or unsaved attempt becomes known. Earlier failed Save attempts do not invalidate this complete persisted CRC-checked capture.',
                    'One/two identical copies only. Additivity, cross-family stacks, native damage/armor/IAS and aura lifetime are separate claims.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture',required=True,type=Path); parser.add_argument('--sha256',required=True)
    parser.add_argument('--output',required=True,type=Path)
    args = parser.parse_args(); data = extract(args.capture,args.sha256)
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(items=len(data['items']),known=sum(r['known'] for r in data['items']),sha256=sha(args.output.read_bytes()))))

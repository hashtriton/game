"""Normalize isolated native equip observations with exact scope and provenance."""
from pathlib import Path
import argparse
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, require, load, sha, rawcode, flat, bit

CACHE = 'LiAEquip1.w3v'
PROBE_SHA = '495c55160012f8aa818d73db54f8e9794e58ffaebfd1fb5b00f78375721d13a3'
SCRIPT_SHA = 'ba7d937448a3153ae86dae9b760d2ce5a39173404ab0c48c9bbcc4f86fdfe7bd'
PHASES = ['baseline', 'one_immediate', 'one_delayed', 'two_immediate', 'two_delayed',
          'remove_first_immediate', 'remove_first_delayed', 'empty_immediate', 'empty_delayed']
COUNTS = [0, 1, 1, 2, 2, 1, 1, 0, 0]
STATS = ['str_total', 'agi_total', 'int_total', 'maxhp', 'maxmp', 'move_speed']


def normalize(categories, report, items, passives):
    rows = {key: flat(value) for key, value in categories.items()}
    meta = rows['meta']
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == 1 and bit(meta, 'complete') and bit(meta, 'strings_ok'), 'Incomplete equip probe')
    abilities = {a['id']: a for a in passives['abilities']}
    matrix = sorted([dict(key='item_' + i['id'], id=i['id'], abilities=i['abilityIds']) for i in items['items']
                     if i['abilityIds'] and all(abilities[a]['baseCode'] in ('AIat','AIde','AIab','AIml','AImm','Arel','AIas','AIrm') and abilities[a]['passiveFamilyMapped'] and not abilities[a]['jassFunctions'] for a in i['abilityIds'])], key=lambda r: r['id'])
    require(report['records'] == matrix and len(matrix) == 43 and set(rows) == {'meta'} | {r['key'] for r in matrix}, 'Equip matrix drift')
    require(meta['records_expected'] == meta['records_finished'] == 43, 'Incomplete equip cases')
    result, known = [], 0
    for case in matrix:
        row = rows[case['key']]
        require(row['requested_id'] == rawcode(case['id']), 'Wrong equip rawcode')
        measured = bit(row, 'known')
        require(measured != bool(row.get('error')), 'Equip success/error disagree')
        target = dict(id=case['id'], known=measured, error=row.get('error', ''), abilityIds=case['abilities'],
                      sourceKey=case['key'], snapshots=[])
        if measured:
            known += 1
            previous_time = -1
            for phase, count in zip(PHASES, COUNTS):
                value = {key[len(phase)+1:]: item for key, item in row.items() if key.startswith(phase + '_')}
                require(value['level'] == 1 and value['xp'] == 0, 'Hero level drift')
                require(all(type(value[k]) is int and value[k] >= 0 for k in ['str_base','agi_base','int_base','str_total','agi_total','int_total']), 'Invalid attributes')
                require([value[k] for k in ['str_base','agi_base','int_base']] == [22, 6, 7], 'Base attributes changed')
                numeric = ['time','hp','maxhp','mp','maxmp','move_speed']
                require(all(type(value[k]) in (int,float) and math.isfinite(value[k]) and value[k] >= 0 for k in numeric), 'Invalid native real')
                require(value['time'] >= previous_time and 0 < value['hp'] <= value['maxhp'] and value['mp'] <= value['maxmp'], 'Invalid native vitality/time')
                previous_time = value['time']
                slots = [value['slot' + str(i)] for i in range(6)]
                require(sum(s == rawcode(case['id']) for s in slots) == count and all(s in (0, rawcode(case['id'])) for s in slots), 'Equip slot identity/count mismatch')
                ranks = []
                for i, ability in enumerate(case['abilities']):
                    require(value['ability'+str(i)+'_id'] == rawcode(ability), 'Ability identity mismatch')
                    rank = value['ability'+str(i)+'_rank']
                    require(type(rank) is int and rank >= 0, 'Invalid native rank')
                    ranks.append(rank)
                bits = {name: categories[case['key']]['reals'][phase+'_'+name]['encodedHex'] for name in numeric}
                target['snapshots'].append(dict(phase=phase, copies=count, level=1, xp=0, time=value['time'],
                    strength=value['str_total'], agility=value['agi_total'], intelligence=value['int_total'],
                    maxHealth=value['maxhp'], maxMana=value['maxmp'], health=value['hp'], mana=value['mp'],
                    moveSpeed=value['move_speed'], abilityRanks=ranks, rawRealBits=bits))
        result.append(target)
    require(known == meta['records_passed'] and 43-known == meta['records_failed'], 'Equip counters disagree')
    return result


def extract(capture, expected_sha):
    capture = Path(capture)
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original map changed')
    report = load(LOCAL / 'item-equip-probe-verification.json')
    require(report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and report['cacheName'] == CACHE and
            sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL / 'item-equip-probe.j').read_bytes()) == SCRIPT_SHA, 'Probe identity changed')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha, 'Campaign hash mismatch')
    spec = importlib.util.spec_from_file_location('equip_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed = reader.parse(raw); saved = load(capture / 'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Fresh CRC parse differs')
    categories = parsed['caches'][CACHE]['categories']
    items = load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json')
    passives = load(ROOT / 'unity/Assets/Arena/Data/lia39-item-passives.json')
    require(items['mapSha256'] == passives['mapSha256'] == MAP_SHA, 'Declaration map mismatch')
    rows = normalize(categories, report, items, passives)
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                    capturedUtc=saved['capturedUtc'], records=43, complete=True),
        heroId='H008', heroLevel=1, items=rows,
        limits=['Only isolated native equip/remove on H008 level1; original map item triggers not executed.',
                '0/1/2/1/0 identical item counts; mixed sets and three or more copies were not measured.',
                'Damage, armor and attack speed are declared native-family fields, not measured by these getters.',
                'Delayed current vitality also includes regeneration; no general resource-change formula is inferred.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path); parser.add_argument('--sha256', required=True)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args(); data = extract(args.capture, args.sha256)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf8')
    print(json.dumps(dict(items=len(data['items']), known=sum(i['known'] for i in data['items']), sha256=sha(args.output.read_bytes()))))

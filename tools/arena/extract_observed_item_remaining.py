"""Normalize every remaining non-PowerUp native equip observation, without promoting unknown effects."""
from pathlib import Path
import argparse
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, require, load, sha, rawcode, flat, bit
from extract_observed_item_equip import PHASES, COUNTS

CACHE = 'LiAEqRest1.w3v'
PROBE_SHA = 'a2b37dfb58e57ba338929a791206f99da4fa3e36ad9336dc1dc8bb26a291f34c'
SCRIPT_SHA = 'fadb4ca2b30db726419cf02e457238eb2712e6f289d157e09871b45dced654c2'
ORIGINAL_EQUIP_SCRIPT_SHA = 'ba7d937448a3153ae86dae9b760d2ce5a39173404ab0c48c9bbcc4f86fdfe7bd'


def expected_matrix(items, passives, first):
    require(items['mapSha256'] == passives['mapSha256'] == MAP_SHA, 'Wrong declaration map')
    require(first['scriptSha256'] == ORIGINAL_EQUIP_SCRIPT_SHA and len(first['records']) == 43,
            'Wrong excluded equip sample')
    excluded = {r['id'] for r in first['records']}
    require(len(excluded) == 43, 'Duplicate excluded identity')
    abilities = {a['id']: a for a in passives['abilities']}
    result = []
    for item in sorted(items['items'], key=lambda row: row['id']):
        if item['classId'] == 'PowerUp' or item['id'] in excluded:
            continue
        closure = []
        def visit(ability):
            if ability in closure:
                return
            require(ability in abilities, 'Unknown closure ability')
            closure.append(ability)
            for level in abilities[ability]['levels']:
                for child in level['spellbookAbilityIds']:
                    visit(child)
        for ability in item['abilityIds']:
            visit(ability)
        require(len(closure) <= 12, 'Native query stride exceeded')
        result.append(dict(key='item_' + item['id'], id=item['id'], classId=item['classId'],
                           directAbilities=item['abilityIds'], abilities=closure))
    require(len(result) == 339 and sum(len(r['abilities']) for r in result) == 472,
            'Remaining source item matrix changed')
    return result


def normalize(categories, report, items, passives, first):
    rows = {key: flat(value) for key, value in categories.items()}
    meta = rows['meta']
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == 2 and bit(meta, 'complete') and bit(meta, 'strings_ok'), 'Incomplete remaining equip probe')
    matrix = expected_matrix(items, passives, first)
    require(report['records'] == matrix and set(rows) == {'meta'} | {r['key'] for r in matrix}, 'Wrong probe matrix')
    require(meta['records_expected'] == meta['records_finished'] == 339, 'Incomplete remaining equip cases')
    result, known = [], 0
    for case in matrix:
        row = rows[case['key']]
        require(row['requested_id'] == rawcode(case['id']), 'Wrong requested item identity')
        measured = bit(row, 'known')
        require(measured != bool(row.get('error')), 'Equip success/error mismatch')
        target = dict(id=case['id'], known=measured, error=row.get('error', ''), sourceKey=case['key'],
                      directAbilityIds=case['directAbilities'], abilityIds=case['abilities'], snapshots=[])
        if measured:
            known += 1
            previous_time = -1
            for phase, count in zip(PHASES, COUNTS):
                values = {key[len(phase)+1:]: value for key, value in row.items() if key.startswith(phase + '_')}
                require(values['level'] == 1 and values['xp'] == 0, 'Hero level/XP changed')
                for name in ('str_base','agi_base','int_base','str_total','agi_total','int_total'):
                    require(type(values[name]) is int and 0 <= values[name] <= 2147483647, 'Invalid attributes')
                require([values[k] for k in ('str_base','agi_base','int_base')] == [22,6,7], 'Base attributes changed')
                numeric = ['time','hp','maxhp','mp','maxmp','move_speed']
                require(all(type(values[k]) in (int,float) and math.isfinite(values[k]) and values[k] >= 0 for k in numeric), 'Invalid native real')
                require(values['time'] >= previous_time and 0 < values['hp'] <= values['maxhp'] and values['mp'] <= values['maxmp'], 'Invalid resources/time')
                previous_time = values['time']
                slots = [values['slot' + str(i)] for i in range(6)]
                require(sum(s == rawcode(case['id']) for s in slots) == count and all(s in (0,rawcode(case['id'])) for s in slots), 'Wrong item residency/count')
                ranks = []
                for i, ability in enumerate(case['abilities']):
                    require(values['ability' + str(i) + '_id'] == rawcode(ability), 'Wrong observed ability identity')
                    rank = values['ability' + str(i) + '_rank']
                    require(type(rank) is int and 0 <= rank <= 2147483647, 'Invalid native rank')
                    ranks.append(rank)
                bits = {name: categories[case['key']]['reals'][phase+'_'+name]['encodedHex'] for name in numeric}
                target['snapshots'].append(dict(phase=phase, copies=count, level=1, xp=0, time=values['time'],
                    strength=values['str_total'], agility=values['agi_total'], intelligence=values['int_total'],
                    maxHealth=values['maxhp'], maxMana=values['maxmp'], health=values['hp'], mana=values['mp'],
                    moveSpeed=values['move_speed'], abilityRanks=ranks, rawRealBits=bits))
            baseline = target['snapshots'][0]
            require([baseline[k] for k in ('strength','agility','intelligence','maxHealth','maxMana','moveSpeed')] == [22,6,7,631,145,250], 'Unexpected fresh baseline')
        result.append(target)
    require(known == meta['records_passed'] and 339-known == meta['records_failed'], 'Equip counters disagree')
    return result


def extract(capture, expected_sha):
    capture = Path(capture)
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original changed')
    report = load(LOCAL / 'item-equip-remaining-probe-verification.json')
    first = load(LOCAL / 'item-equip-probe-verification.json')
    require(report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and report['cacheName'] == CACHE and
            report['nativePreflightPassed'] and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and
            sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL / 'item-equip-remaining-probe.j').read_bytes()) == SCRIPT_SHA,
            'Probe source identity changed')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha, 'Campaign hash mismatch')
    spec = importlib.util.spec_from_file_location('remaining_equip_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed = reader.parse(raw); saved = load(capture / 'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Fresh CRC parse differs')
    rows = normalize(parsed['caches'][CACHE]['categories'], report,
                     load(ROOT/'unity/Assets/Arena/Data/lia39-items.json'),
                     load(ROOT/'unity/Assets/Arena/Data/lia39-item-passives.json'), first)
    return dict(schemaVersion=2, mapSha256=MAP_SHA, engineVersion='1.26.0.6401', heroId='H008', heroLevel=1,
                source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA,
                            probeScriptSha256=SCRIPT_SHA, capturedUtc=saved['capturedUtc'], records=339, complete=True), items=rows,
                limits=['Native isolated equip facts only; original map item triggers and active use were not executed.',
                        'Every remaining non-PowerUp rawcode, including shop aliases; gameplay conversion and ownership are separate.',
                        'No failed case or missing field becomes a zero stat/ability rank.',
                        'Immediate and next .1-second tick observations; aura warmup/persistence may exceed this window.',
                        'No claim of reversible additive behavior, mixed stacks, damage, armor, attack speed or complete item effects.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture',required=True,type=Path); parser.add_argument('--sha256',required=True)
    parser.add_argument('--output',required=True,type=Path)
    args=parser.parse_args(); data=extract(args.capture,args.sha256)
    args.output.parent.mkdir(parents=True,exist_ok=True)
    args.output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(items=len(data['items']),known=sum(i['known'] for i in data['items']),sha256=sha(args.output.read_bytes()))))

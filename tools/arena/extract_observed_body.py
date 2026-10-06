"""BODY1 stationary weapon frontiers, deliberately not a collision getter."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T013936072015Z-1a9a7eacd88e'
CACHE = 'LiABody1.w3v'
CACHE_SHA = '1a9a7eacd88ecd31902ba9919384b530ea9bebeb8d2140dd100758ee6886483d'
PROBE_SHA = '37f02a856d6a642ce91ac5be6c16638df936b1a9be57d2ba3527258d1484b2fc'
SCRIPT_SHA = '01661c42fa667dc89c6a4ad56e4d3d31a71531a920d9e5997327d553273eae09'


def matrix():
    units = load(ROOT / 'research/lia/warcraft/3.9c/units.json')
    cases = []
    for unit, radius in [('n06B',8),('n06A',16),('n008',24),('hwat',32),('n06C',None),('n06I',None)]:
        declaration = units[unit]
        require(declaration.get('collision') == radius and declaration['movetp'] == 'foot', 'Changed body declaration')
        offsets = [23.9,24+radius-.1,24+radius+.1,24+radius+20] if radius is not None else [24+r+d for r in (0,8,16,24,32) for d in (-.1,.1)]
        for offset in offsets:
            cases.append(dict(key='body_'+unit+'_'+str(offset).replace('.','_'), id=unit, requestedLevel=0,
                declaredCollision=radius, range=declaration['rangeN1'], offset=offset, distance=declaration['rangeN1']+offset,
                anchor='H008', anchorLevel=50, anchorCollision=24, rootAbility='A0KV', observeSeconds=2.6))
    return cases


def position(row, prefix, rooted=True):
    result = {k: row[prefix+k] for k in ('time','x','y','anchor_x','anchor_y','distance','order','Bena','B08D')}
    require(all(type(v) in (float,int) and math.isfinite(v) for v in result.values()), 'Invalid native position')
    require(result['anchor_x'] == 135 and result['anchor_y'] == 1000, 'Anchor moved during boundary measurement')
    require(result['x'] == row['before_order_x'] and result['y'] == row['before_order_y'], 'Attacker moved during boundary measurement')
    actual = math.hypot(result['x']-result['anchor_x'], result['y']-result['anchor_y'])
    require(abs(actual-result['distance']) < .01 and actual > 0, 'Incorrect distance observation')
    require(result['Bena'] in (0,1) and result['B08D'] in (0,1) and (not rooted or result['Bena']+result['B08D'] > 0), 'Root buff absent')
    result['coordinateDistance'] = actual
    return result


def normalize(rows, report):
    cases = matrix(); meta = rows['meta']
    require(meta['schema'] == 36 and meta['complete'] == 1 and meta['source_map_sha256'] == MAP_SHA and
        meta['client_expected'] == '1.26.0.6401' and meta['records_expected'] == meta['records_finished'] ==
        meta['records_succeeded'] == 36 and meta['records_failed'] == 0, 'Incomplete BODY1')
    require(report['records'] == cases and set(rows) == {'meta'} | {c['key'] for c in cases}, 'Wrong body matrix')
    records = []; groups = {}
    for case in cases:
        r = rows[case['key']]
        require(r['known'] == r['created'] == r['ensnare_accepted'] == r['attack_order_accepted'] == 1 and
            r['id'] == case['id'] and r['id_integer'] == rawcode(case['id']) and r['requested_level'] == 0 and
            not r.get('error') and r['strays'] == 0 and r['max_shift'] == 0 and r['samples'] == 26,
            'Invalid stationary native row')
        require(abs(r['requested_distance']-case['distance']) < .001 and
            abs(r['before_order_x']-135-case['distance']) < .01 and r['before_order_y'] == 1000,
            'Wrong requested placement')
        record = dict(sourceKey=case['key'],unitId=case['id'],declaredCollision=case['declaredCollision'],
            declaredAttackRange=case['range'],anchorCollision=24,requestedDistance=case['distance'],
            beforeOrder=position(r,'before_order_'),placed=position(r,'placed_',False),rooted=position(r,'rooted_'),
            final=position(r,'final_'),samples=[position(r,'sample'+str(i)+'_') for i in range(26)])
        for field, prefix, cap in [('attacks','attack',32),('damage','damage',64)]:
            count = r['attack_events' if field == 'attacks' else 'damage_events']
            require(type(count) is int and 0 <= count < cap, 'Native event cap')
            record[field] = [position(r,prefix+str(i)+'_') for i in range(count)]
            for i, event in enumerate(record[field]):
                if field == 'damage':
                    event['eventDamage'] = r['damage'+str(i)+'_event_damage']
                    event['anchorHealthBefore'] = r['damage'+str(i)+'_anchor_hp']
                    require(type(event['eventDamage']) in (int,float) and math.isfinite(event['eventDamage']) and
                        event['eventDamage'] > 0 and event['anchorHealthBefore'] > .405, 'Invalid positive weapon damage')
            require(all(b['time'] > a['time'] for a,b in zip(record[field],record[field][1:])), 'Unordered native events')
        start, end = record['beforeOrder']['time'], record['final']['time']
        # Native float timer timestamps drift by ~0.00143s in later rows;
        # 26 ordered0.1s snapshots still cover every one of130 callbacks.
        require(abs(end-start-2.6) < .003 and all(start < s['time'] <= end for s in record['samples']) and
            all(b['time'] > a['time'] for a,b in zip(record['samples'],record['samples'][1:])), 'Incomplete observation duration')
        require(all(start <= e['time'] <= end for kind in ('attacks','damage') for e in record[kind]), 'Event outside observation')
        require(bool(record['damage']) == bool(record['attacks']), 'No corresponding positive weapon control')
        record['weaponHit'] = bool(record['damage'])
        records.append(record); groups.setdefault(case['id'],[]).append(record)
    frontiers = []
    for unit, observations in groups.items():
        hit = [r['beforeOrder']['coordinateDistance'] for r in observations if r['weaponHit']]
        miss = [r['beforeOrder']['coordinateDistance'] for r in observations if not r['weaponHit']]
        require(hit, 'No positive near control for '+unit)
        lo, hi = max(hit), min(miss) if miss else None
        require(hi is None or lo < hi, 'Nonmonotone native boundary')
        first = observations[0]; base = first['declaredAttackRange']+24
        if unit in ('n06B','n06A','hwat'):
            boundary = base+first['declaredCollision']
            require(hi is not None and lo < boundary < hi and hi-lo < .21, 'Declared radius positive/negative control failed')
        frontiers.append(dict(unitId=unit,declaredCollision=first['declaredCollision'],declaredAttackRange=first['declaredAttackRange'],
            largestHitCenterDistance=lo,smallestNoHitCenterDistance=hi,positiveSamples=len(hit),negativeSamples=len(miss),
            effectiveAdditionLowerBound=lo-base,effectiveAdditionUpperBound=None if hi is None else hi-base,
            collisionKnown=False))
    return dict(records=records,frontiers=frontiers)


def extract():
    report = load(LOCAL/'body1-verification.json')
    require(report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and report['cacheName'] == CACHE and
        report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and report['nativePreflightPassed'], 'Wrong BODY1 proof')
    for path, expected in [(Path(report['map']),PROBE_SHA),(LOCAL/'body1.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:
        require(sha(path.read_bytes()) == expected, 'Changed native evidence '+str(path))
    spec=importlib.util.spec_from_file_location('body_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'], 'Fresh body CRC differs')
    data=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
            capturedUtc=saved['capturedUtc'],records=36),**data,limits=[
            'Stationary ensnared attack-frontier observations, not a native collision getter or free-motion collision test.',
            'Explicit8/16/32 controls bracket declared range+both radii. Rooted n008 range100 still hits at168WC; its upper frontier was not captured.',
            'Sparse n06C/n06I positive174.0996 and negative181.8999 only bound effective range addition; no exact collision radius is promoted.',
            'MeleeRangeMax128 in native MiscData is a possible explanation for the rooted100range control, not a measured engine rule.',
            'Both anchor and attacker coordinates are checked in every sample, attack-start and damage event. No-hit interpretations require near positive controls.'])


if __name__=='__main__':
    result=extract(); output=ROOT/'.local/lia-port/abilities/body-native-observations.json'
    output.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),frontiers=result['frontiers'])))

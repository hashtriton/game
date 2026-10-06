"""DOOM1 exact multi-order, cleanup, pause and killed-target observations."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T044510926372Z-2bcee16855ba'
CACHE = 'LiADoom1.w3v'
CACHE_SHA = '2bcee16855bac6c44b7c750257dfb8f8d473c7e146889d29628b744aab96d0bb'
PROBE_SHA = '8daa669775eae145c0b6d696c69f5a0d685eca872c38509a72dac465ea856acf'
SCRIPT_SHA = '2060d7be09d9726b59aac8d4ec273a369a53fd99a818e56d083f6b35117fb8d2'
KEYS = ('single', 'three_orders', 'negative_cleanse', 'paused', 'killed')


def close(a, b, tolerance=.002):
    require(type(a) in (int, float) and math.isfinite(a) and abs(a-b) <= tolerance, 'Unexpected DOOM1 number')


def normalize(rows, report):
    m = rows['meta']
    require(m['schema'] == 71 and m['complete'] == 1 and m['records_expected'] == m['records_finished'] ==
            m['records_succeeded'] == 5 and m['records_failed'] == 0 and m['source_map_sha256'] == MAP_SHA and
            m['client_expected'] == '1.26.0.6401', 'Incomplete DOOM1')
    require(tuple(r['key'] for r in report['records']) == KEYS, 'Wrong DOOM1 matrix')
    expected = {'meta'}; result = []
    for key in KEYS:
        r = rows[key]; expected.add(key)
        count = 3 if key == 'three_orders' else 1
        damage_count = 2 if key in ('negative_cleanse', 'killed') else count * 4
        require(r['known'] == r['accepted'] == 1 and r['strays'] == 0 and r['error'] == '' and r['samples'] == 82 and
                r['effects'] == count and r['spells'] == 5*count and r['events'] == damage_count and r['summons'] == 0 and
                r['deaths'] == int(key == 'killed'), 'Invalid DOOM1 row')
        if count == 3: require(r['extra0_accepted'] == r['extra1_accepted'] == 1, 'Rejected extra order')
        start = r['after_order_time']; spells = []; events = []; deaths = []
        require(r['before_id'] == rawcode('H008') and r['before_caster_id'] == rawcode('h011') and
                r['before_hp'] == r['after_order_hp'] == 847 and r['before_B0BN'] == 0 and r['after_order_B0BN'] == 1,
                'Invalid DOOM1 identities/application')
        roles = [1,20,21][:count]
        for i in range(5*count):
            name = key+'_spell'+str(i); expected.add(name); e = rows[name]; spells.append(e)
            require(e['kind'] == i%5+1 and e['ability'] == rawcode('A0HR') and e['source'] == 3 and
                    e['target'] == (roles[i//5] if i%5 < 3 else -1), 'Wrong DOOM1 lifecycle')
            close(e['time'], start)
        for i in range(damage_count):
            name = key+'_event'+str(i); expected.add(name); e = rows[name]; events.append(e)
            pulse = i//count
            require(e['source'] == 3 and e['source_id'] == rawcode('h011') and e['target'] == roles[i%count] and
                    e['damage'] == 0 and e['target_hp'] == 847 and e['target_B0BN'] == int(pulse > 0), 'Wrong DOOM1 zero event')
            delay = (0, .01, 4, 5)[pulse] if key == 'paused' else (0,.01,1.01,2.01)[pulse]
            close(e['time']-start, delay)
        require(r['before_intervention_B0BN'] == 1 and r['before_intervention_hp'] == 847, 'Missing active intervention control')
        close(r['before_intervention_time']-start, 1)
        require(r['after_intervention_B0BN'] == int(key not in ('negative_cleanse','killed')) and
                r['after_intervention_paused'] == int(key == 'paused'), 'Wrong intervention outcome')
        if key == 'paused':
            require(r['before_unpause_paused'] == r['before_unpause_B0BN'] == r['after_unpause_B0BN'] == 1 and
                    r['after_unpause_paused'] == 0, 'Pause did not preserve the buff')
            close(r['after_unpause_time']-r['after_intervention_time'],2)
        samples = []
        for i in range(82):
            prefix = 'sample'+str(i)+'_'; sample = {k[len(prefix):]:v for k,v in r.items() if k.startswith(prefix)}
            require(sample['id'] == rawcode('H008') and sample['handle'] == r['before_handle'] and
                    sample['caster_id'] == rawcode('h011') and sample['caster_handle'] == r['before_caster_handle'] and
                    sample['hidden'] == int(key == 'killed' and i >= 57) and sample['summons'] == 0, 'Changed DOOM1 sample identity')
            close(sample['hp'], 0 if key == 'killed' and sample['time'] > r['after_intervention_time'] else 847)
            if samples: require(sample['time'] > samples[-1]['time'], 'Reversed sample clock')
            if count == 3:
                for extra in range(2):
                    p = 'extra'+str(extra)+'_'
                    require(sample[p+'id'] == rawcode('H008') and sample[p+'hp'] == 847 and
                            sample[p+'B0BN'] == sample['B0BN'] and sample[p+'paused'] == 0, 'Extra target does not match its own native result')
            samples.append(sample)
        if key in ('single','three_orders','paused'):
            active = [s['time']-start for s in samples if s['B0BN'] == 1]
            end = 5 if key == 'paused' else 3
            require(active and end-.11 <= max(active) <= end+.11, 'Changed native duration bracket')
        if key == 'killed':
            name = key+'_death0'; expected.add(name); d = rows[name]; deaths.append(d)
            require(d['role'] == 1 and d['id'] == rawcode('H008') and d['handle'] == r['before_handle'] and d['hp'] == 0,
                    'Wrong native death identity')
            close(d['time']-start, 1)
        require(r['final_B0BN'] == 0 and r['final_hp'] == (0 if key == 'killed' else 847), 'Wrong final state')
        result.append(dict(sourceKey=key, summary=r, spells=spells, events=events, deaths=deaths, samples=samples))
    require(set(rows) == expected, 'Unexpected DOOM1 categories')
    return result


def extract():
    report=load(LOCAL/'doom1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'], 'Wrong DOOM1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'doom1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]: require(sha(p.read_bytes())==h,'Changed DOOM1 '+str(p))
    spec=importlib.util.spec_from_file_location('doom_cache',LOCAL/'read_probe_cache.py'); reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'], 'Fresh DOOM1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA), records=normalize(rows,report), limits=[
        'Exactly A0HR rank1/h011 to H008level10. Three same-callback target orders all complete; each receives four zero events without HP loss.',
        'UnitRemoveBuffs(false,true) clears B0BN and cancels later periodic events. This is not an observation of an ordinary dispel spell.',
        'Pause1..3s extends buff duration by2s, but periodic events restart1s after unpause rather than preserving the former short residual.',
        'KillUnit at1s records one hero death and zero summoned units for the remaining7s. No generic Doom unit/default is inferred.',
        'Inventory use, invulnerable targets, natural lethal damage and simultaneous reapplication are outside this probe.'])


if __name__=='__main__':
    output=extract();p=ROOT/'.local/lia-port/abilities/doom-observations.json'
    p.write_text(json.dumps(output,indent=2,allow_nan=False)+'\n',encoding='utf-8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(output['records']))))

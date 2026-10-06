"""IMAGE4: positive n017 AOmi copy combat, with independent native controls."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T052552045575Z-55b7fc525da0'
CACHE = 'LiAImage4.w3v'
CACHE_SHA = '55b7fc525da0eba23ee88b700b7d471defc20ea3e1a5514661a5033420991303'
PROBE_SHA = '9379bb65c18310e0b079844903362a3ea3d64baec999f2bfe6e86438d64ea4b6'
SCRIPT_SHA = '3ce51f1c5bcbb8368c0e0d0ad43148b16c77b3505e0f9ab7eca6028f5de52e8b'
KEY = 'aomi_n017'


def close(a, b, tolerance=.001):
    require(abs(a-b) <= tolerance, 'Changed IMAGE4 number')


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema'] == 76 and meta['complete'] == 1 and meta['records_expected'] == meta['records_finished'] ==
            meta['records_succeeded'] == 1 and meta['records_failed'] == 0 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete IMAGE4')
    require(report['records'] == [dict(key=KEY, id='n017', requestedLevel=0, rank=1, factory=1, item=False)], 'Changed IMAGE4 matrix')
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int, float)), 'Nonfinite IMAGE4')
    expected = {'meta'}
    def get(suffix=''):
        key = KEY + ('_' + suffix if suffix else '')
        expected.add(key)
        return rows[key]
    summary = get()
    require(summary['known'] == 1 and summary['images'] == 2 and summary['events'] == 28 and summary['spells'] == 4 and
            summary['samples'] == 163 and summary['deaths'] == summary['strays'] == 0 and
            all(summary[k] == 1 for k in ['baseline_enemy', 'baseline_attack', 'baseline_stop', 'cast_accepted',
                                        'donor_stop', 'image_attack', 'image_stop']), 'Missing IMAGE4 positive controls')
    baseline, precast, aftercast = get('baseline'), get('precast'), get('aftercast')
    preflight, dummy = get('preflight_donor'), get('preflight_dummy')
    births = [get('birth1'), get('birth2')]
    handles = [baseline['handle']] + [r['handle'] for r in births]
    require(len(set(handles + [dummy['handle']])) == 4 and all(h > 0 for h in handles), 'Aliased IMAGE4 identity')
    def identity(row, role=1):
        i = 0 if role == 1 else role-10
        require(row['id'] == rawcode('n017') and row['handle'] == handles[i] and row['owner'] == 11 and
                row['illusion'] == int(i != 0) and row['is_hero'] == row['user_data'] == 0 and
                row['maxhp'] == 35500 and row['maxmp'] == 6000 and row['speed'] == 250 and
                all(row[k] == 0 for k in ['level', 'str', 'agi', 'int'] + ['slot'+str(n) for n in range(6)]) and
                row['A04C'] == 1 and all(row[k] == 0 for k in ['A05T','B008','A05M','A05N','A0LZ','AIat','AOcr','A0K4','A0QE']),
                'Changed IMAGE4 factory profile')
        require(.405 < row['hp'] <= row['maxhp'] and 0 <= row['mp'] <= row['maxmp'] and row['hidden'] in (0,1), 'Invalid IMAGE4 vitality')
        if i: require(row['hidden'] == 0, 'Hidden placeholder admitted as permanent image')
    for row in [baseline, precast, aftercast, preflight, get('final_donor')]: identity(row)
    for i, birth in enumerate(births, 11):
        identity(birth, i)
        require(birth['stop_accepted'] == 1 and birth['paused'] == 0 and birth['hp'] == precast['hp'], 'Invalid visible image birth')
    identity(get('image_before_attack'), 11)
    require(get('image_before_attack')['paused'] == 1 and baseline['paused'] == preflight['paused'] == 1, 'Changed controlled pause')
    require(dummy['id'] == rawcode('O006') and dummy['owner'] == 0 and dummy['paused'] == 1 and dummy['hp'] == dummy['maxhp'] == 30000,
            'Changed durable recipient')
    close(precast['hp'], 17750); close(precast['mp'], 4500)
    spells = [get('spell'+str(i)) for i in range(4)]
    require([s['kind'] for s in spells] == [1,2,3,5], 'Unexpected AOmi spell sequence')
    for s in spells:
        identity(s)
        require(s['ability'] == rawcode('A04C') and s['target'] == -1 and s['paused'] == 0, 'Wrong AOmi spell identity')
    close(spells[0]['time'], spells[1]['time']); close(spells[2]['time'], spells[3]['time'])
    close(spells[2]['time']-spells[0]['time'], .75)
    close(spells[2]['mp']-spells[3]['mp'], 250)
    require(summary['cast_at'] <= spells[0]['time'] <= summary['cast_at']+.021, 'Wrong order/channel boundary')
    require(spells[2]['time']+.5 <= births[0]['time'] <= spells[2]['time']+.53 and
            births[0]['time'] <= births[1]['time'] <= spells[2]['time']+.71, 'Changed observed copy discovery bounds')
    damage = [get('damage'+str(i)) for i in range(summary['events'])]
    for i, e in enumerate(damage):
        require(e['damage'] > 0 and (i == 0 or e['time'] >= damage[i-1]['time']) and e['stage'] in range(6), 'Invalid damage chronology')
        if e['target'] in (1,11): identity(e, e['target'])
        else:
            require(e['target'] == 3 and e['id'] == rawcode('O006') and e['handle'] == dummy['handle'] and
                    e['paused'] == 1 and e['hp'] == e['maxhp'] == 30000 and e['x'] == 200 and e['y'] == 1000,
                    'Wrong IMAGE4 recipient')
    direct = {}
    for label, stage, target in [('donor',1,1), ('image',3,11)]:
        events = [e for e in damage if e['stage'] == stage]
        require(len(events) == 3 and all(e['source'] == 3 and e['target'] == target for e in events), 'Wrong direct damage source')
        controls = [get(label+'_direct_'+axis) for axis in ('chaos','magic','universal')]
        for control, e, value in zip(controls, events, [40/(1+.06*80),40,40]):
            require(control['known'] == control['accepted'] == control['events'] == 1 and control['restored'] == control['before'] and
                    control['before'] > 100 and control['after'] > .405, 'Invalid direct restoration/acceptance')
            close(e['damage'], value)
            # HP≈33820 is a binary32 value, so subtraction rounds in .00390625 steps.
            close(control['before']-control['after'], e['damage'], .00390625)
        direct[label] = dict(controls=controls, events=events)
    for a,b in zip(direct['donor']['events'], direct['image']['events']): close(a['damage'], b['damage'])
    weapons = {}
    for stage, source, count in [(0,1,4),(2,1,1),(4,11,3),(5,11,3)]:
        events = [e for e in damage if e['stage'] == stage]
        require(len(events) == count*2, 'Missing weapon/reflection pair')
        hits = []
        for reflect, hit in zip(events[::2], events[1::2]):
            require(reflect['source'] == 3 and reflect['target'] == source and hit['source'] == source and hit['target'] == 3,
                    'Unregistered weapon/reflection identity')
            close(reflect['time'], hit['time']); close(reflect['damage'], 420)
            close(hit['source_hp'], reflect['hp']-reflect['damage'], .00390625)
            close(hit['damage'], 1400/(1+.06*80))
            if stage != 2:
                require(hit['source_x'] == 135 and hit['source_y'] == 1000, 'Moved selected combat actor')
            hits.append(hit)
        for a,b in zip(hits, hits[1:]): close(b['time']-a['time'], 1.5)
        weapons[str(stage)] = hits
    samples = []
    for i in range(summary['samples']):
        clock, donor = get('sample'+str(i)), get('sample'+str(i)+'_donor')
        identity(donor)
        require(clock['images'] in (0,1,2) and (not samples or clock['time'] > samples[-1]['time']), 'Invalid image sample sequence')
        images = []
        for n in range(1,clock['images']+1):
            row=get('sample'+str(i)+'_image'+str(n)); identity(row,10+n); images.append(row)
            require(clock['time'] >= births[n-1]['time'], 'Image sampled before admission')
            if n==2: require(row['paused'] == 1, 'Secondary pause control changed')
        samples.append(dict(time=clock['time'], stage=clock['stage'], donor=donor, images=images))
    require(any(s['donor']['hidden'] for s in samples) and any(s['images'] and s['images'][0]['paused'] for s in samples), 'Missing native phase controls')
    require(set(rows) == expected, 'Unexpected IMAGE4 categories')
    return dict(unitId='n017', factory='AOmi', abilityId='A04C', rank=1, known=True, summary=summary, baseline=baseline,
                precast=precast, births=births, spells=spells, direct=direct, damage=damage, weapons=weapons, samples=samples,
                observed=dict(nativeHero=False, outgoingMultiplier=1, incomingMultiplier=1, weaponIntervalSeconds=1.5,
                              baseArmor=80, sourceWeaponRaw=1400, manaCost=250, channelToEffectSeconds=.75,
                              maxHealth=35500,maxMana=6000,moveSpeed=250))


def extract():
    report=load(LOCAL/'image4-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'], 'Wrong IMAGE4 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'image4.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed IMAGE4 '+str(path))
    spec=importlib.util.spec_from_file_location('image4_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh IMAGE4 CRC mismatch')
    row=normalize({k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()},report)
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=[row],limits=[
        'Only n017 A04C rank1, no original triggers or player scaling. Native copied HERO=false; direct three damage axes and positive actual weapons independently corroborate both declared multipliers1.',
        'Durable paused O006 retains native reflection. Each weapon hit is preceded by420 reflection; these callbacks are preserved, not misattributed to an image ability.',
        'First visible image discovery is EFFECT+.52002; second is+.70001. These are sampled admission times, not a claim of simultaneous exact birth.',
        'Both copies are artificially paused at admission, the first later resumes. No death occurs within the bounded trace; declared10s duration is not independently measured by this probe.',
        'Actual image attacks continue after accepted stop due to the native scenario. This does not establish a general stop/cancel policy.',
        'Controlled pathingOFF and SetXY combat do not prove host placement, original sV entry scaling, arbitrary passive/item inheritance or private native RNG implementation.'])


if __name__=='__main__':
    result=extract(); output=ROOT/'.local/lia-port/abilities/boss-image-observations.json'
    output.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(result['records']))))

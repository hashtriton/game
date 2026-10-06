"""ITEMWARD2: actual native ward birth, isolated resource windows and expiry."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require

CAPTURE = LOCAL / 'cache-captures/20261006T060026315310Z-b27eb9dffab9'
CACHE = 'LiAItemWard2.w3v'
CACHE_SHA = 'b27eb9dffab9d081a2d4d032c2cc635fe9c1643e8fa39d6ccd5c77bf7d62c3bd'
PROBE_SHA = 'ba58069eb80e153e39b941709ea92303a5bb1c5e276f04bb860e9bb4de86391f'
SCRIPT_SHA = '61401c3ab8355e9aed8dffb1d92d46fe819139975d96071b047150954a299332'
SPECS = [('health_ward', 'I0A1', 'ohwd', 'Aoar', 30), ('mana_ward', 'I0A2', 'o00J', 'A0PU', 15)]

def state(row, prefix):
    return {k[len(prefix):]: v for k, v in row.items() if k.startswith(prefix)}

def slope(samples, actor, resource, lo, hi):
    selected = [s for s in samples if lo < s['time'] < hi]
    require(len(selected) >= 5, 'Missing resource window')
    require(all(0 < s[actor+'_'+resource] < s[actor+'_max'+resource] for s in selected), 'Resource window is capped')
    return (selected[-1][actor+'_'+resource]-selected[0][actor+'_'+resource]) / (selected[-1]['time']-selected[0]['time'])

def normalize(rows, report):
    require(set(rows) == {'meta', 'health_ward', 'mana_ward'}, 'Unexpected ward matrix')
    require([(r['key'],r['id'],r['unit']) for r in report['records']] == [(s[0],s[1],s[2]) for s in SPECS], 'Report matrix changed')
    m=rows['meta']
    require(m['schema']==78 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==2
            and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401', 'Incomplete wards')
    result=[]
    for key,item,unit,aura,duration in SPECS:
        r=rows[key]
        require(all(math.isfinite(v) for v in r.values() if type(v) in (float,int)), 'Nonfinite ward observation')
        require(r['known']==r['wards_seen']==r['add_accepted']==r['powerup']==r['charges']==1 and r['strays']==0
                and r['item_id']==rawcode(item) and r['samples']==360, 'Invalid ward activation')
        birth=state(r,'birth_')
        require(birth['id']==rawcode(unit) and birth['owner']==0 and birth['paused']==birth['dead']==0 and
                birth['hp']==birth['maxhp']==5 and birth['mp']==birth['maxmp']==birth['speed']==0 and
                birth['x']==500 and birth['y']==1000 and birth[aura]==birth['A0K4']==1, 'Wrong ward birth')
        require(r['armor_accepted']==r['armor_events']==1 and r['armor_before']==r['armor_restored']==5 and
                r['armor_after']==3 and r['armor_event1']==2, 'Unproven ward armor')
        require(r['before_add_time']==r['after_add_time']==3 and r['before_add_ward_exists']==0 and
                r['before_helper_remove_time']==r['after_helper_remove_time']==4 and
                r['after_helper_remove_helper_exists']==0 and r['after_helper_remove_ward_hp']==5, 'Helper lifetime changed')
        samples=[state(r,f'sample{i}_') for i in range(1,361)]
        require(all(a['time']<b['time'] for a,b in zip(samples,samples[1:])), 'Reversed ward clock')
        for actor in ('near','outside','enemy'):
            for resource in ('hp','mp'):
                require(all(0<a[actor+'_'+resource]<=a[actor+'_max'+resource] and a[actor+'_'+resource]<=b[actor+'_'+resource]
                            for a,b in zip(samples,samples[1:])), 'Unexpected resource debit or cap')
        for s in samples:
            for actor,x,y,owner in [('near',800,1000,0),('outside',1100,1000,0),('enemy',500,1300,11)]:
                require(s[actor+'_id']==rawcode('H008') and s[actor+'_owner']==owner and s[actor+'_x']==x and s[actor+'_y']==y and
                        s[actor+'_paused']==s[actor+'_dead']==0 and s[actor+'_maxhp']==631 and s[actor+'_maxmp']==145,
                        'Recipient identity or geometry changed')
            if s.get('ward_hp',0)>.405:
                require(s['ward_id']==rawcode(unit) and s['ward_owner']==0 and s['ward_x']==500 and s['ward_y']==1000 and
                        s['ward_paused']==0 and s['ward_hp']==5, 'Living ward changed')
        alive=[s for s in samples if s.get('ward_hp',0)>.405]
        dead=[s for s in samples if s['time']>alive[-1]['time']]
        require(alive and dead and abs(dead[0]['time']-birth['time']-duration)<.11 and
                all(s.get('ward_hp',0)<=.405 for s in dead), 'Missing actual timed-life boundary')
        windows=[]
        for lo,hi in [(1,2.9),(3.7,4.9),(5.3,8.9),(10.2,11.9)]:
            values={a+'_'+v:slope(samples,a,v,lo,hi) for a in ('near','outside','enemy') for v in ('hp','mp')}
            for a in ('outside','enemy'):
                require(abs(values[a+'_hp']-2.4)<.02 and abs(values[a+'_mp']-.4)<.01, 'Negative resource control failed')
            if hi<3: require(abs(values['near_hp']-2.4)<.02 and abs(values['near_mp']-.4)<.01, 'Baseline failed')
            elif key=='health_ward': require(abs(values['near_hp']-(2.4+631*.03))<.02 and abs(values['near_mp']-.4)<.01, 'Health aura failed')
            else:
                expected=.4+145*(.03 if hi<5 else .06)
                require(abs(values['near_mp']-expected)<.04 and abs(values['near_hp']-2.4)<.02, 'Mana phase changed')
            windows.append(dict(fromSeconds=lo,toSeconds=hi,slopes=values))
        result.append(dict(itemId=item,unitId=unit,auraId=aura,known=True,health=5,mana=0,moveSpeed=0,armor=0,
            duration=duration,lastAliveSeconds=alive[-1]['time']-birth['time'],firstDeadSeconds=dead[0]['time']-birth['time'],
            healthRegenFractionKnown=key=='health_ward',healthRegenFraction=.03 if key=='health_ward' else 0,
            manaStableRateKnown=False,windows=windows))
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,complete=True,records=2,passed=2,failed=0),wards=result,
        limits=['Original Acv trigger absent; its hidden-powerup path was reproduced in isolation.',
                'Mana aura has distinct approximately3% and6% maxMana slopes, with an additional transient dip; no constant rate promoted.',
                'Targets300/600WC constrain geometry but do not prove exact radius500 boundary.',
                'Health recipient capped before ward expiry, so aura linger is not measured.',
                'Ward collision, stacking and arbitrary target maxima are not measured.'])

def extract():
    spec=importlib.util.spec_from_file_location('ward_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Ward cache changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh CRC differs')
    report=load(LOCAL/'itemward2-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA,'Ward report changed')
    for file,digest in [('LiA39c_ITEMWARD2.w3x',PROBE_SHA),('itemward2.j',SCRIPT_SHA)]: require(sha((LOCAL/file).read_bytes())==digest,'Ward source changed')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result

if __name__=='__main__':
    result=extract();output=ROOT/'.local/lia-port/abilities/item-ward-observations.json'
    output.write_text(json.dumps(result,indent=2,ensure_ascii=False,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),wards=len(result['wards']))))

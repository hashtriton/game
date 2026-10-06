"""WARD1: native healing and the isolated source pre-damage callback contract."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T034236988700Z-58252a70da4a'
CACHE='LiAWard1.w3v'
CACHE_SHA='58252a70da4a674036700a7591e104dc785877df384b5df0be7ad643cfe15271'
PROBE_SHA='9ce630509c8d45b2a89173539bb20973e30e35422819efc85e79b66690980cd1'
SCRIPT_SHA='cce0a8d8a4f77b45a22a597a23e75e330767d8fa67ad290c1fa06e44271f8203'
KEYS=['heal_near','heal_approach','plain_1','plain_40','hook_1','hook_40']

def normalize(rows):
    m=rows['meta']
    require(set(rows)=={'meta',*KEYS} and m['schema']==63 and m['complete']==1 and
        m['records_expected']==m['records_finished']==m['records_succeeded']==6 and m['records_failed']==0 and
        m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Wrong WARD1 completion')
    result=[]
    for k in KEYS:
        r=rows[k]
        require(r['known']==1 and r['initial_id']==rawcode('u00H') and r['initial_hp']==8 and r['initial_mp']==0 and
            r['ability_rank']==1 and r['initial_boss_hp']==1000,'Wrong ward identity/vitality')
        if k.startswith('heal'):
            require(r['order_accepted']==1 and r['spell_events']==5 and r['damage_events']==0 and r['final_boss_hp']==2000 and
                r['final_mp']==0 and r['final_hp']==8,'Healing positive control failed')
            for i in range(5):
                require(r[f'spell{i}_ability']==rawcode('A0TT') and r[f'spell{i}_target']==(rawcode('u00G') if i<3 else 0) and
                    r[f'spell{i}_kind']==289+i and r[f'spell{i}_mp']==0,'Wrong heal spell sequence')
            require(abs(r['spell2_time']-r['spell0_time']-.5)<.0001 and abs(r['spell3_time']-r['spell2_time']-.51)<.0001,'Changed heal latency')
            if k=='heal_near': require(r['spell0_time']<.001 and r['final_x']==135,'Near cast unexpectedly moved')
            else: require(7.6<r['spell0_time']<7.8 and 138<1035-r['spell0_x']<139,'Approach control changed')
            result.append(dict(key=k,manaCost=0,healing=1000,effectDelay=r['spell2_time']-r['spell0_time'],
                castStarted=r['spell0_time'],castDistance=215-r['spell0_x'] if k=='heal_near' else 1035-r['spell0_x']))
        else:
            amount=40 if k.endswith('40') else 1
            expected={'plain_1':[7,6,5,4],'plain_40':[0,0,0,0],'hook_1':[6,4,2,1],'hook_40':[6,4,2,0]}[k]
            events=1 if k=='plain_40' else 4
            require(r['spell_events']==0 and r['damage_events']==events,'Wrong ward damage sequence')
            require([r[f'after{i}_hp'] for i in range(1,5)]==expected and r['final_hp']==expected[-1],'Changed prehit cancellation')
            for i in range(events):
                require(r[f'damage{i}_source']==rawcode('hfoo') and r[f'damage{i}_id']==rawcode('u00H') and
                    r[f'damage{i}_amount']==amount and r[f'damage{i}_hp']==r[f'before{i+1}_hp'],'Wrong prehit damage stage')
            for i in range(1,4):
                if k.startswith('hook'):
                    require(r[f'restore{i}_hp']==expected[i-1] and 0<r[f'restore{i}_time']-r[f'after{i}_time']<.001,'Missing zero timer restore')
                else: require(f'restore{i}_hp' not in r,'Unexpected callback in untreated control')
            require('restore4_hp' not in r,'HP<=2 must use ordinary damage')
            result.append(dict(key=k,requestedDamage=amount,eventDamage=amount,afterHitHealth=expected,
                hook=k.startswith('hook'),events=events))
    return result

def extract():
    report=load(LOCAL/'ward1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'] and
        [r['key'] for r in report['records']]==KEYS,'Wrong ward provenance')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'ward1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed proof: '+str(p))
    spec=importlib.util.spec_from_file_location('ward_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=normalize(rows),limits=[
            'The damage callback is an independently reproduced behavior. It is not execution of the original match trigger.',
            'Zero-timer native restore was observed about.0001s later; host tick quantization is separate.',
            'Armor0 is inferred from prehit CHAOS/NORMAL40 event40 on a fresh ward; lethal HP clamping itself is not armor evidence.',
            'Native cast distance138.25 is one approach trace; host100+target body range is a derived boundary.',
            'A0TT heal is applied after its EFFECT callback and before FINISH. Exact subframe HP ordering is not a separate getter.',
            'Post-damage attack/heal reorders and source RemoveUnit after.75s were absent, so those remain static-path rules.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/ward-observations.json';data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(data['records']))))

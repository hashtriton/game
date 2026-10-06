"""CAPS3 positive native movement and attack-cycle bounds."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T031626489254Z-ad4611bf2db9'
CACHE = 'LiACaps3.w3v'
CACHE_SHA = 'ad4611bf2db9ec71a9910ae6c67495fdc910f3c3011b710ad29ca8c4210ec9d1'
PROBE_SHA = '85641625b8bcb9a8fd21f0a43b3cdd0f92a95733a39cb19f86da6d8ee22f62c8'
SCRIPT_SHA = 'c0d73169e5deb2c66f44d1e2c5fb7250660aeec46dbc2c6fd29aeb76e7305201'



KEYS=('speed_zero','speed_above_max','ias_floor_hero','ias_floor_footman','movement_stack','ias_high_agility')
def state(row,prefix):return {k[len(prefix):]:v for k,v in row.items() if k.startswith(prefix)}
def normalize(rows,report):
    m=rows['meta'];require(m['schema']==61 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==6 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete CAPS3')
    require([m[k] for k in ('false_and_true_or_true','grouped_false_and_true_or_true','true_or_false_and_false')]==[0,1,0],'Native logical controls differ')
    require(tuple(r['key'] for r in report['records'])==KEYS,'Wrong caps matrix')
    output=[]
    for key in KEYS:
        r=rows[key];require(r['known']==1 and r['strays']==0 and not r.get('error'),'Invalid caps observation')
        for value in r.values():
            if type(value) in (float,int):require(math.isfinite(value),'Nonfinite caps observation')
        expectedid=rawcode('hfoo' if key=='ias_floor_footman' else 'H008')
        require(r['initial_id']==r['final_id']==expectedid and r['initial_order_accepted']==1,'Wrong cap unit identity')
        starts=[state(r,'attack'+str(i)+'_') for i in range(r['starts'])]
        hits=[state(r,'damage'+str(i)+'_') for i in range(r['damage_events'])]
        samples=[state(r,'sample'+str(i)+'_') for i in range(r['samples'])]
        intervals=[]
        if key.startswith('speed_'):
            expected=1 if key=='speed_zero' else 522
            require(r['requested_speed']==r['final_speed']==expected and .99<r['final_time']-r['requested_time']<1.01 and not starts and not hits,'Invalid movement cap control')
            displacement=math.hypot(r['final_x']-r['requested_x'],r['final_y']-r['requested_y'])
            require(.8<displacement<1.1 if expected==1 else 400<displacement<530,'Missing actual movement control')
        elif key=='movement_stack':
            require(r['defend_accepted']==1 and r['walk_before_speed']==r['final_speed']==1 and all(r['walk_before_'+buff]==r['final_'+buff]==1 for buff in ('B06K','Bfro','B06T')),'Stacked lower bound lacks positive buffs')
            require(.8<math.hypot(r['final_x']-r['walk_before_x'],r['final_y']-r['walk_before_y'])<1.1,'No stacked low-speed movement')
        else:
            require(all(x['id']==expectedid and x['x']==135 and x['y']==1000 and x['target_x']==255 and x['target_y']==1000 for x in starts),'Attack geometry changed')
            require(any(x['kind']==1 and x['value']>0 and x['stage']==0 for x in hits),'No positive baseline hit')
            if key=='ias_high_agility':
                for agi in (600,1000):
                    group=[x for x in starts if x.get('agi')==agi]
                    require(len(group)==11 and all(x[b]==0 for x in group for b in ('B06K','Bfro','B0A9','B06T')),'Wrong high-AGI state')
                    require(any(x['kind']==1 and x['value']>0 and x.get('agi')==agi for x in hits),'Missing positive high-AGI damage')
                    values=[right['time']-left['time'] for left,right in zip(group,group[1:])]
                    require(all(abs(v-.37)<.0003 for v in values),'High-AGI native period changed');intervals+=values
            else:
                group=[x for x in starts if all(x[b]==1 for b in ('B06K','Bfro','B0A9'))]
                require(len(group)==3,'Missing complete floor cycles')
                expected=6.75 if key=='ias_floor_footman' else 9.25
                values=[right['time']-left['time'] for left,right in zip(group,group[1:])]
                require(all(abs(v-expected)<.0003 for v in values),'Native slow floor period changed');intervals=values
        require('attack'+str(len(starts))+'_time' not in r and 'damage'+str(len(hits))+'_time' not in r,'Extra cap event')
        output.append(dict(sourceKey=key,initial=state(r,'initial_'),final=state(r,'final_'),attackStarts=starts,damageEvents=hits,samples=samples,completeIntervals=intervals))
    return output

def extract():
    report=load(LOCAL/'caps3-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong combat-caps provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'caps3.j',SCRIPT_SHA),
                        (CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed combat-caps evidence '+str(path))
    spec=importlib.util.spec_from_file_location('combat-caps_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh combat-caps CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
        'Positive controls support normal move-speed1..522 and attack-cycle rate.2..5 for the measured H008/hfoo states. Arrival/acceleration and private engine tick precision are not asserted.',
        'Explicit scripted forced movement still uses its separate zero movement-execution gate. SetUnitMoveSpeed(0) native getter1 is not an immobilization primitive.',
        'CAPS2 failed partial rows are not used. CAPS3 explicit phase branches completed6/6; its pure truth controls document the observed mixed-operator behavior independently.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/combat-caps-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))


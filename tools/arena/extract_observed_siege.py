"""SIEGE1: exact A1DT/A1DU rank-one paired native weapon observations."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T065452001438Z-f48269eefb7f'
CACHE='LiASiege1.w3v'
CACHE_SHA='f48269eefb7f7de92161912424dd2965fabed293ab86725396c702e2fb2b072c'
PROBE_SHA='f9b7fcac0039621624bf34a8f1d5f5584e0a343118c9bb822b6f0adf479ef6f7'
SCRIPT_SHA='21b4bb772a56f34c8aeedd1bfdcada897ad29a54dcd0a8a26386a5d784590d45'

def matrix():
    return [dict(key=f'{target}_{mode}',id='H008',requestedLevel=1,target=target,mode=mode)
            for target in ('H008','hfoo','hhou') for mode in range(3)]

def number(r,k):
    v=r[k];require(type(v) in (int,float) and math.isfinite(v),'Nonfinite siege observation '+k);return v

def state(r,p,c):
    expected=dict(source_id=rawcode('H008'),target_id=rawcode(c['target']),A1DT=int(c['mode']==1),
                  A1DU=int(c['mode']!=0),A0K4=0,source_owner=0,target_owner=11,source_level=1,
                  source_str=22,source_agi=6,source_int=7,source_A05T=0,target_A05T=0,
                  source_B008=0,target_B008=0,target_hero=int(c['target']=='H008'),target_structure=int(c['target']=='hhou'))
    require(all(r[p+k]==v for k,v in expected.items()),'Wrong native siege identity/rank/attributes')
    v={k:number(r,p+k) for k in ('time','hp','maxhp','source_x','source_y','target_x','target_y')}
    require(v['maxhp']==dict(H008=631,hfoo=420,hhou=500)[c['target']] and 0<v['hp']<=v['maxhp'] and
            (v['source_x'],v['source_y'],v['target_x'],v['target_y'])==(135,1000,200,1000),'Wrong siege position/vitals')
    return v

def normalize(rows,report):
    m=rows['meta'];cases=matrix()
    require(m['schema']==85 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==9 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete SIEGE1')
    require(report['records']==cases and set(rows)=={'meta'}|{r['key'] for r in cases},'Wrong SIEGE1 matrix')
    result=[]
    for c in cases:
        r=rows[c['key']]
        require(r['known']==r['created']==r['direct_accepted']==r['attack_order_accepted']==r['stop_accepted']==1 and
                r['events']==5 and r['attacks']==r['positive_weapon_hits']==4 and r['strays']==0 and
                r['id']=='H008' and r['mode']==c['mode'] and r['target_requested']==rawcode(c['target']) and
                r['error']=='Processed weapon observations','Failed siege row')
        if c['mode']:require(r['add_book' if c['mode']==1 else 'add_direct']==1,'Rejected native ability addition')
        initial=state(r,'before_',c);final=state(r,'after_',c);direct=state(r,'event0_',c);maximum=initial['maxhp']
        require(initial['hp']==final['hp']==direct['hp']==maximum and r['event0_direct']==1 and r['event0_attack_starts']==0 and
                direct['time']==initial['time'] and r['direct_before']==r['direct_restored']==maximum and
                0<number(r,'event0_damage')<40 and abs(maximum-number(r,'direct_after')-r['event0_damage'])<.0001,
                'Invalid direct armor control/restoration')
        hits=[];attacks=[]
        for i in range(4):
            a=state(r,f'attack{i}_',c);e=state(r,f'event{i+1}_',c);post=state(r,f'post_event{i+1}_',c)
            d=number(r,f'event{i+1}_damage')
            require(r[f'attack{i}_seed']==12345 and r[f'event{i+1}_direct']==0 and r[f'event{i+1}_attack_starts']==i+1 and
                    a['hp']==e['hp']==maximum and a['time']<e['time']<=post['time']<e['time']+.021 and d>0 and
                    abs(e['hp']-post['hp']-d)<.04,'Invalid positive weapon/HP pairing')
            if i:require(a['time']>attacks[-1]['time'],'Reversed attack timeline')
            attacks.append(a);hits.append(dict(event=e,post=post,eventDamage=d))
        require('event5_time' not in r and 'attack4_time' not in r,'Unexpected siege callbacks')
        result.append(dict(sourceKey=c['key'],target=c['target'],mode=c['mode'],before=initial,after=final,
                           directEventDamage=r['event0_damage'],attacks=attacks,hits=hits))
    for i in range(0,9,3):
        baseline=result[i]
        require(all(r['directEventDamage']==baseline['directEventDamage'] and
                    [h['eventDamage'] for h in r['hits']]==[h['eventDamage'] for h in baseline['hits']]
                    for r in result[i+1:i+3]),'Changed paired siege damage')
    return result

def extract():
    report=load(LOCAL/'siege1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong SIEGE1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'siege1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(p.read_bytes())==h,'Changed SIEGE1 evidence '+str(p))
    spec=importlib.util.spec_from_file_location('siege_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh SIEGE1 CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
        cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Fresh H008 level1 source22STR/6AGI/7INT, paused H008/hfoo/hhou targets, fixed isolated65WC separation; both native thorns removed.',
        'All four positive weapon hits and direct40 CHAOS/NORMAL controls match exactly between baseline, A1DT spellbook and direct A1DU rank1 for each target.',
        'The child getter is rank1 in both spellbook and direct rows. No general bonus damage or structure multiplier is promoted from sparse ANde data.',
        'This does not test other target types, ranks, items, an additional50primary, multiple sources or a different native factory. Native seed repetition is a control, not RNG replay proof.',
        'Post-event HP is sampled on the next tick and includes small target regeneration. Only exact-target DAMAGED/ATTACKED callbacks are registered, not a global event registry.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/siege-observations.json';output.write_text(json.dumps(extract(),ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(output)

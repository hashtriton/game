"""SUMW1: n07C corruption ordering and exact rank-one QE/K4 controls."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T031036837787Z-b52899ebc4a3'
CACHE = 'LiASumW1.w3v'
CACHE_SHA = 'b52899ebc4a3fde938d8e689040ee402751ab5ed13729e983fee57b16541fd69'
PROBE_SHA = '8d42a44a0a8d75342d94824cb8ea829ab5856be9d65f78b7133ad15b2e22762e'
SCRIPT_SHA = '31d6499516bde9ee238a318bbffdcedfd1bbe1e2160b5ac2e79bb3c015be7dd8'


def matrix():
    return [dict(key=k,id='n07C',requestedLevel=0,corruption=c,qe=q,k4=b) for k,c,q,b in [
        ('stripped',0,0,0),('stripped_QE',0,1,0),('stripped_K4',0,0,1),('stripped_both',0,1,1),
        ('intact',1,0,0),('intact_both',1,1,1)]]


def number(r,k):
    v=r[k]; require(type(v) in (int,float) and math.isfinite(v),'Nonfinite summon observation '+k); return v


def state(r,p,c):
    v={k:number(r,p+k) for k in ('time','hp','maxhp','source_x','source_y','target_x','target_y','BIcb')}
    require(r[p+'source_id']==rawcode('n07C') and r[p+'target_id']==rawcode('hfoo') and
            r[p+'A0QZ']==c['corruption'] and r[p+'A0QE']==c['qe'] and r[p+'A0K4']==c['k4'],
            'Wrong fresh summon ability/handle identity')
    require(v['maxhp']==420 and 0<v['hp']<=420 and v['BIcb'] in (0,1) and
            (v['source_x'],v['source_y'],v['target_x'],v['target_y'])==(135,1000,200,1000),
            'Invalid summon position/health/buff state')
    return v


def normalize(rows,report):
    m=rows['meta'];cases=matrix()
    require(m['schema']==55 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==6 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401',
            'Incomplete SUMW1')
    require(report['records']==cases and set(rows)=={'meta'}|{r['key'] for r in cases},'Wrong SUMW1 matrix')
    result=[]
    for c in cases:
        r=rows[c['key']]
        require(r['known']==r['created']==r['direct_accepted']==r['attack_order_accepted']==r['stop_accepted']==1 and
                r['events']==5 and r['attacks']==r['positive_weapon_hits']==4 and r['strays']==0 and r['id']=='n07C' and
                r['error']=='Processed weapon observations','Failed SUMW1 row')
        require([r['corruption_requested'],r['QE_requested'],r['K4_requested']]==[c['corruption'],c['qe'],c['k4']],
                'Wrong requested abilities')
        for key,expected in [('remove_QZ',not c['corruption']),('add_QE',c['qe']),('add_K4',c['k4'])]:
            if expected: require(r[key]==1,'Rejected ability setup')
        initial=state(r,'before_',c);final=state(r,'after_',c);direct=state(r,'event0_',c)
        require(initial['hp']==final['hp']==direct['hp']==420 and initial['BIcb']==direct['BIcb']==0 and
                r['event0_direct']==1 and r['event0_attack_starts']==0 and direct['time']==initial['time'] and
                r['direct_before']==r['direct_restored']==420 and
                abs(number(r,'event0_damage')-40/1.12)<.0001 and
                abs((420-number(r,'direct_after'))-number(r,'event0_damage'))<.0001,'Invalid direct positive control')
        attacks=[];hits=[]
        expected=152*(2-.94**8) if c['corruption'] else 152/1.12
        for i in range(4):
            a=state(r,f'attack{i}_',c);e=state(r,f'event{i+1}_',c);post=state(r,f'post_event{i+1}_',c)
            require(r[f'attack{i}_seed']==12345 and r[f'event{i+1}_direct']==0 and
                    r[f'event{i+1}_attack_starts']==i+1 and a['hp']==e['hp']==420 and
                    e['BIcb']==c['corruption'] and a['time']<e['time']<=post['time']<e['time']+.021,
                    'Invalid weapon event attribution/order')
            damage=number(r,f'event{i+1}_damage')
            # The native negative-armor exponent differs slightly from Python
            # double pow. Check formula compatibility, then exact paired rows.
            require(abs(damage-expected)<.001,'Changed paired native damage')
            require(abs((e['hp']-post['hp'])-damage)<.01,'Post-event life inconsistent with hit/short regen')
            if i: require(a['time']>attacks[-1]['time'],'Attack order reversed')
            attacks.append(a);hits.append(dict(event=e,post=post,eventDamage=damage))
        require('event5_time' not in r and 'attack4_time' not in r,'Unexpected extra event')
        result.append(dict(sourceKey=c['key'],corruption=bool(c['corruption']),qeRank=c['qe'],k4Rank=c['k4'],
                           before=initial,after=final,directEventDamage=r['event0_damage'],attacks=attacks,hits=hits))
    for group in (result[:4],result[4:]):
        baseline=[h['eventDamage'] for h in group[0]['hits']]
        require(all([h['eventDamage'] for h in r['hits']]==baseline for r in group),'Unpaired marker damage')
    return result


def extract():
    report=load(LOCAL/'sumw1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong SUMW1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'sumw1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(p.read_bytes())==h,'Changed SUMW1 evidence '+str(p))
    spec=importlib.util.spec_from_file_location('sumw_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh SUMW1 CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Exact n07C rank1 A0QE/A0K4 separately and together do not alter the tested physical hit sequence. Other ranks/types are not measured.',
        'All paired attacks repeat native roll152. Removed A0QZ gives152/(1+.06*2); intact A0QZ gives152*(2-.94^8), consistent with declared armor reduction10 before the first weapon damage event.',
        'A0QZ BIcb is present in every intact damage callback. Source Dur1/HeroDur1=.01 is not replaced with permanent or stacked armor reduction.',
        'Paused hfoo and forced isolated coordinates are experiment conditions. Buff expiration, pause/hidden behavior, multiple sources and native RNG algorithm are not measured.',
        'Post-event life is sampled on the next .02 tick and includes short native regeneration. It is retained separately from the synchronous GetEventDamage value.'])


if __name__=='__main__':
    data=extract();p=ROOT/'.local/lia-port/abilities/summon-weapon-observations.json'
    p.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(data['records']))))
